// Port of skygrind/src/combat/gunMath.js (p17-night6), line for line, the web's comments kept (NIGHT9 U3-07).
// Pure maths for the POWER P4 weapons (guns + melee). No three.js, no DOM, unit tested. Vectors are plain
// {x, y, z}; directions are unit length.
// - sweepTargets is already ported as CombatMath.SweepTargets (CombatMath.cs, NIGHT5): it is not duplicated here.
// - The functions that mutate a vector (bounceVelocity, reflectDir) take it by ref (Vec3 is a struct) and return it.
// - chainTargets' `used` Set is the output list itself (every index added to `used` is pushed to `out` on the same
//   line), so a Zapper shot allocates nothing when the caller passes a reused list.
// Golden vectors: PortExport/vectors/gunMath.json (Golden/GoldenPorted.cs). Web tests: tests/gunMath.test.js.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>One Zapper chain link: the candidate's index and its damage multiplier.</summary>
    public struct ChainLink
    {
        public int index;
        public double mult;
    }

    /// <summary>thumperWave's result.</summary>
    public struct ThumperWaveResult
    {
        public double radius;
        public double damage;
        public bool big;
    }

    /// <summary>config.weapons.thumper: the numbers thumperWave reads.</summary>
    public sealed class ThumperParams
    {
        public double slamWindow, ringRadius, ringDamage, bigRingRadius, bigRingDamage;

        public static ThumperParams From(GameConfig cfg, string path = "weapons.thumper") => new ThumperParams
        {
            slamWindow = cfg.Num(path + ".slamWindow"),
            ringRadius = cfg.Num(path + ".ringRadius"),
            ringDamage = cfg.Num(path + ".ringDamage"),
            bigRingRadius = cfg.Num(path + ".bigRingRadius"),
            bigRingDamage = cfg.Num(path + ".bigRingDamage"),
        };
    }

    public static class GunMath
    {
        // sweepTargets(origin, dir, targets, range, halfAngle, out): CombatMath.SweepTargets.

        /// <summary>
        /// The Zapper chain: from the first target, jump to the nearest target not yet hit within `range` of the last
        /// one, up to `count` more; each link deals `falloff` x the one before.
        /// `start` the first target's point; `points` candidates (the first target excluded); `canReach(i, from)` line of
        /// sight (null: always true). Fills and returns `output` (a new list when null).
        /// </summary>
        public static List<ChainLink> ChainTargets(Vec3 start, IReadOnlyList<Vec3> points, double count, double range, double falloff, Func<int, Vec3, bool> canReach = null, List<ChainLink> output = null)
        {
            var @out = output ?? new List<ChainLink>();
            @out.Clear();
            // const used = new Set(): the indices already in `out` (see the header).
            Vec3 from = start;
            double mult = 1;
            for (int k = 0; k < count; k++)
            {
                int best = -1;
                double bestD = range;
                for (int i = 0; i < points.Count; i++)
                {
                    if (Used(@out, i)) continue;
                    Vec3 p = points[i];
                    double d = JsMath.Hypot(p.x - from.x, p.y - from.y, p.z - from.z);
                    if (d <= bestD && (canReach == null || canReach(i, from)))
                    {
                        best = i;
                        bestD = d;
                    }
                }
                if (best < 0) break;
                // used.add(best)
                mult *= falloff;
                @out.Add(new ChainLink { index = best, mult = mult });
                from = points[best];
            }
            return @out;
        }

        static bool Used(List<ChainLink> links, int i)
        {
            for (int j = 0; j < links.Count; j++) if (links[j].index == i) return true;
            return false;
        }

        /// <summary>
        /// A bounce off a surface with unit normal n: the normal part flips and keeps `restitution`, the tangent part
        /// keeps `friction`. Mutates and returns v.
        /// </summary>
        public static Vec3 BounceVelocity(ref Vec3 v, Vec3 n, double restitution, double friction)
        {
            double vn = v.x * n.x + v.y * n.y + v.z * n.z;
            if (vn >= 0) return v; // already leaving the surface
            double tx = v.x - vn * n.x;
            double ty = v.y - vn * n.y;
            double tz = v.z - vn * n.z;
            v.x = tx * friction - vn * restitution * n.x;
            v.y = ty * friction - vn * restitution * n.y;
            v.z = tz * friction - vn * restitution * n.z;
            return v;
        }

        /// <summary>A mirror reflection (the Discus ricochet: no speed lost). Mutates and returns d.</summary>
        public static Vec3 ReflectDir(ref Vec3 d, Vec3 n)
        {
            double k = 2 * (d.x * n.x + d.y * n.y + d.z * n.z);
            d.x -= k * n.x;
            d.y -= k * n.y;
            d.z -= k * n.z;
            return d;
        }

        /// <summary>The Discus hit: base x (1 + bonus per wall bounce so far).</summary>
        public static double DiscDamage(double baseDamage, double bounces, double bonus) => baseDamage * (1 + bonus * bounces);

        /// <summary>
        /// The Vortex pull on a body at horizontal offset (dx, dz) from the centre: toward the centre at `speed`,
        /// easing to 0 inside `core` m (no jitter at the middle). Returns the speed (0 outside `radius`).
        /// The web's default `core = 1.5` is config.weapons.vortex.pullCore, which its one caller (projectiles.js) passes:
        /// required here (no gameplay number typed in code).
        /// </summary>
        public static double PullSpeed(double distance, double radius, double speed, double core)
        {
            if (distance > radius) return 0;
            return speed * JsMath.Min(1, distance / core);
        }

        /// <summary>
        /// The next swing of a melee combo: 0, 1, 2, ... wrapping at `count`, back to 0 when the last swing was more
        /// than `window` s ago.
        /// </summary>
        public static int NextCombo(int last, double sinceLast, double window, int count)
        {
            if (last < 0 || sinceLast > window) return 0;
            return (last + 1) % count;
        }

        /// <summary>
        /// A 3-round burst's timing: is a round due `steps` into the burst? Rounds go out at 0, gap, 2 x gap...
        /// Returns the round index due this step, or -1. (A gap of 0 is -1, as JS's `steps % 0` is NaN.)
        /// </summary>
        public static int BurstRoundAt(int steps, int gapSteps, int rounds)
        {
            if (gapSteps == 0 || steps % gapSteps != 0) return -1;
            int i = steps / gapSteps; // exact: steps is a multiple of gapSteps here
            return i < rounds ? i : -1;
        }

        /// <summary>
        /// The Thumper's shockwave: the big wave when the swing lands out of a slam (or within `slamWindow` s of a slam
        /// landing), else the normal one.
        /// </summary>
        public static ThumperWaveResult ThumperWave(bool slamming, double sinceSlamLand, ThumperParams t)
        {
            bool big = slamming || sinceSlamLand <= t.slamWindow;
            return big ? new ThumperWaveResult { radius = t.bigRingRadius, damage = t.bigRingDamage, big = big } : new ThumperWaveResult { radius = t.ringRadius, damage = t.ringDamage, big = big };
        }
    }
}
