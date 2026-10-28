// ARCLINE swing math, ported from skygrind/src/movement/swingMath.js (OVERNIGHT2 N4). Pure: no UnityEngine, no
// scene. The mover calls Step() on its fixed step (config.arcline.substeps sub-steps while attached), then collides.
// The rope is a spherical wall: past the rope length L the body is pulled back and only the OUTWARD radial velocity
// is removed, so tangential motion is kept (a pendulum); inward motion is never touched. The first 3% past L is a
// critically damped spring (the catch is a tug, not a wall); past that the position is projected back hard.
// Gravity ×0.85 while attached, light quadratic drag, a pump (tangential, strongest at the bottom, fading toward a
// soft cap), a reel (L shrinks; tangential speed grows with L_old / L_new: angular momentum), a pull-in on attach.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>The body on the rope. Step() updates it in place.</summary>
    public struct SwingState
    {
        public Vec3 pos;
        public Vec3 vel;
        public double L; // current rope length (m)
    }

    public struct SwingInput
    {
        public Vec3? wish; // the move input on the ground plane (unit), or null
        public bool reel;
        public bool pulling; // the attach pull-in
    }

    public struct SwingParams
    {
        public double g;
        public double drag;
        public double pump;
        public double softCap;
        public double reelSpeed;
        public double minLen;
        public double stretch;
        public double omega;
        public double pullAccel;
    }

    public struct SwingStepResult
    {
        public bool taut;
        public double stretch; // m past L (for the visuals)
    }

    public struct AnchorCandidate
    {
        public Vec3 point;
        public bool node; // a grapple node (wins ties)
    }

    public static class SwingMath
    {
        /// <summary>The rope's reach now: distance from the anchor to the body.</summary>
        public static double RopeDistance(Vec3 pos, Vec3 anchor) =>
            JsMath.Hypot(pos.x - anchor.x, pos.y - anchor.y, pos.z - anchor.z);

        /// <summary>Kinetic + potential energy per unit mass: v² / 2 + g · height.</summary>
        public static double SwingEnergy(Vec3 pos, Vec3 vel, double g) => 0.5 * Vec3.Dot(vel, vel) + g * pos.y;

        /// <summary>The swing's parameters from config.arcline and the base gravity (m/s², the mover's jump gravity).</summary>
        public static SwingParams Params(ArclineConfig a, double baseGravity) =>
            new SwingParams
            {
                g = baseGravity * a.gravityMult,
                drag = a.drag,
                pump = a.pumpAccel,
                softCap = a.softCap,
                reelSpeed = a.reelSpeed,
                minLen = a.minLen,
                stretch = a.stretch,
                omega = a.springOmega,
                pullAccel = a.pullAccel,
            };

        /// <summary>
        /// One sub-step of the swing (updates s): forces, pump, reel, integrate, then the rope constraint.
        /// The body moves to s.pos (the caller then collides).
        /// </summary>
        public static SwingStepResult Step(ref SwingState s, Vec3 anchor, in SwingInput input, in SwingParams p, double h)
        {
            Vec3 pos = s.pos;
            Vec3 vel = s.vel;
            // Forces: gravity (×0.85), the pull-in toward the anchor, light quadratic drag.
            vel.y -= p.g * h;
            double nx = pos.x - anchor.x;
            double ny = pos.y - anchor.y;
            double nz = pos.z - anchor.z;
            double d = JsMath.Hypot(nx, ny, nz);
            if (!(d > 0)) d = 1e-6; // JS `|| 1e-6`
            nx /= d;
            ny /= d;
            nz /= d;
            if (input.pulling)
            {
                vel.x -= nx * p.pullAccel * h;
                vel.y -= ny * p.pullAccel * h;
                vel.z -= nz * p.pullAccel * h;
            }
            double speed = Math.Sqrt(Vec3.Dot(vel, vel));
            if (p.drag > 0 && speed > 0)
            {
                double k = JsMath.Min(1, p.drag * speed * h);
                vel.x -= vel.x * k;
                vel.y -= vel.y * k;
                vel.z -= vel.z * k;
            }
            // Pump: the move input's part across the rope (the swing plane), strongest at the bottom (the body under
            // the anchor: n.y = -1), fading toward the soft cap.
            if (input.wish.HasValue)
            {
                Vec3 w = input.wish.Value;
                double along = w.x * nx + w.y * ny + w.z * nz;
                double tx = w.x - nx * along;
                double ty = w.y - ny * along;
                double tz = w.z - nz * along;
                double tl = JsMath.Hypot(tx, ty, tz);
                if (tl > 1e-6)
                {
                    double bottom = JsMath.Max(0, -ny);
                    double r = speed / p.softCap;
                    double fade = JsMath.Max(0, 1 - r * r);
                    double a = (p.pump * bottom * fade * h) / tl;
                    tx *= a;
                    ty *= a;
                    tz *= a;
                    vel.x += tx;
                    vel.y += ty;
                    vel.z += tz;
                }
            }
            // Reel: the rope shortens; while taut the tangential speed grows as L_old / L_new (angular momentum kept).
            if (input.reel && s.L > p.minLen)
            {
                double L0 = s.L;
                s.L = JsMath.Max(p.minLen, s.L - p.reelSpeed * h);
                if (d >= L0 * 0.999)
                {
                    double vr = vel.x * nx + vel.y * ny + vel.z * nz;
                    double k = L0 / s.L;
                    vel.x = (vel.x - nx * vr) * k + nx * JsMath.Min(vr, 0);
                    vel.y = (vel.y - ny * vr) * k + ny * JsMath.Min(vr, 0);
                    vel.z = (vel.z - nz * vr) * k + nz * JsMath.Min(vr, 0);
                }
            }
            // Integrate.
            pos.x += vel.x * h;
            pos.y += vel.y * h;
            pos.z += vel.z * h;
            // The constraint: past L a critically damped spring pulls back (only ever pulling); past L × (1 + stretch)
            // the position is projected back and the outward radial velocity removed.
            nx = pos.x - anchor.x;
            ny = pos.y - anchor.y;
            nz = pos.z - anchor.z;
            d = JsMath.Hypot(nx, ny, nz);
            if (!(d > 0)) d = 1e-6;
            nx /= d;
            ny /= d;
            nz /= d;
            double over = d - s.L;
            bool taut = false;
            if (over > 0)
            {
                taut = true;
                double vr = vel.x * nx + vel.y * ny + vel.z * nz;
                double acc = -(p.omega * p.omega * over + 2 * p.omega * vr);
                if (acc < 0)
                {
                    vel.x += nx * acc * h;
                    vel.y += ny * acc * h;
                    vel.z += nz * acc * h;
                }
                double max = s.L * (1 + p.stretch);
                if (d > max)
                {
                    pos.x = anchor.x + nx * max;
                    pos.y = anchor.y + ny * max;
                    pos.z = anchor.z + nz * max;
                    double outward = vel.x * nx + vel.y * ny + vel.z * nz;
                    if (outward > 0)
                    {
                        vel.x -= nx * outward;
                        vel.y -= ny * outward;
                        vel.z -= nz * outward;
                    }
                }
            }
            s.pos = pos;
            s.vel = vel;
            return new SwingStepResult { taut = taut, stretch = JsMath.Max(0, over) };
        }

        /// <summary>Letting go: all velocity kept, × risingBoost while rising; a jump release adds jumpUp m/s upward.</summary>
        public static Vec3 ReleaseVelocity(Vec3 vel, double risingBoost, double jumpUp = 0)
        {
            if (vel.y > 0)
            {
                vel.x *= risingBoost;
                vel.y *= risingBoost;
                vel.z *= risingBoost;
            }
            vel.y += jumpUp;
            return vel;
        }

        /// <summary>How long the hook takes to fly distance m: distance / speed, clamped to [min, max] s.</summary>
        public static double HookTime(double distance, ArclineConfig a) =>
            JsMath.Max(a.hookMin, JsMath.Min(a.hookMax, distance / a.hookSpeed));

        /// <summary>
        /// Anchor choice: the best candidate is in front of the aim and above the horizon, nearest the crosshair;
        /// nodes win ties. aim is unit. Returns the index or -1.
        /// </summary>
        public static int PickAnchor(IReadOnlyList<AnchorCandidate> candidates, Vec3 eye, Vec3 aim, double coneRad, double range)
        {
            int best = -1;
            double bestScore = double.NegativeInfinity;
            double cos = JsMath.Cos(coneRad);
            for (int i = 0; i < candidates.Count; i++)
            {
                AnchorCandidate c = candidates[i];
                double dx = c.point.x - eye.x;
                double dy = c.point.y - eye.y;
                double dz = c.point.z - eye.z;
                double d = JsMath.Hypot(dx, dy, dz);
                if (d < 1e-3 || d > range) continue;
                double align = (dx * aim.x + dy * aim.y + dz * aim.z) / d;
                if (align < cos) continue;
                double score = align * 2 + (dy > 0 ? 0.35 : 0) + (c.node ? 0.5 : 0) - d / range * 0.2;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }
            return best;
        }
    }
}
