// NIGHT55 builder C step55 (design.n55.step55, a VIEW row: NightRows.ViewOn(cfg, "step55"); off = NIGHT54's camera, move54's Move54Step).
// Slate P0-step / OPEN O-04: move54's step-down smoothing was never measured (a354's line never crossed a step). Two parts, Unity-free (the
// SimHarness compiles this file for `n55look step`, the same scan and the same smoother the Player runs):
//   1. Move55Step: the grounded step's height change held WHOLE in sim time (Research/2026-10-20-wave55/github.md item 1: ioquake3's
//      accumulated step change, Source's clamp, SimpleFPSController's hold-then-return; written here, no code copied). After each 120 Hz sim
//      step with both ends grounded (on the ground or sliding) and the feet moved Step55StepMin or more, the offset takes -dy (capped at
//      Step55MaxOffset); a critically damped spring (Step55Omega) returns it to 0, stepped by the sim's own dt; the render draws the offset at
//      the frame's alpha between the values before and after the last step, as the feet are drawn. The same at 60, 144 or 240 fps by
//      construction (move54 counts FRAMES and subtracts the DRAWN change beyond a keep rate); a freeze (no steps) holds it. Never on a jump,
//      a fall, a landing or a wall ride (both ends grounded only); a teleport (Step55ResetJump) drops it.
//   2. Move55Lines: run lines across a floor step (the a255 reel's and the harness's scan): a grid of start points over a room's rect, each
//      with Step55ScanHeadings headings, a floor sample every Step55ScanStep m; a line holds the first floor change in [StepMin, StepMax]
//      between two flat samples, at least ScanFrom m in and ScanBefore m before its end. A down step ranks first.
// Nothing here writes the sim (the feet, the Mover, every ray are untouched); the camera's height only.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Move55
    {
        /// <summary>The row (a view row: NIGHT54's camera when off).</summary>
        public static bool On(GameConfig cfg) => NightRows.ViewOn(cfg, "step55");
    }

    /// <summary>NIGHT55 C step55: the grounded step's camera offset, held whole in sim time and drawn at alpha (see the file's header).</summary>
    public sealed class Move55Step
    {
        readonly double omega, stepMin, maxOffset, resetJump, dt;
        double off, vel, offPrev;
        bool groundedBefore, have;

        /// <summary>Telemetry (the reel, the harness): grounded steps held, the largest one (m, signed: negative = down).</summary>
        public int StepsHeld;
        public double LargestStep;
        /// <summary>The offset after the last sim step (m; added to the camera's height).</summary>
        public double Offset => off;

        public Move55Step(double simDt, double omega = UnityTuning.Step55Omega, double stepMin = UnityTuning.Step55StepMin,
            double maxOffset = UnityTuning.Step55MaxOffset, double resetJump = UnityTuning.Step55ResetJump)
        {
            dt = simDt;
            this.omega = omega;
            this.stepMin = stepMin;
            this.maxOffset = maxOffset;
            this.resetJump = resetJump;
        }

        /// <summary>After each sim step: whether the step ended grounded (on the ground or sliding) and the feet's height before and after it.</summary>
        public void StepEnded(bool groundedNow, double beforeY, double afterY)
        {
            offPrev = off;
            double dy = afterY - beforeY;
            if (!have || Math.Abs(dy) > resetJump)
            {
                off = vel = offPrev = 0;
                have = true;
                groundedBefore = groundedNow;
                return;
            }
            if (groundedBefore && groundedNow && Math.Abs(dy) >= stepMin)
            {
                off -= dy; // the whole snap held (on top of what is left of the last one: a staircase accumulates under the cap)
                StepsHeld++;
                if (Math.Abs(dy) > Math.Abs(LargestStep)) LargestStep = dy;
            }
            // the critically damped spring's exact step over the sim's dt: x(t) = (x0 + (v0 + w x0) t) e^-wt
            double e = Math.Exp(-omega * dt);
            double x0 = off, v0 = vel, c = v0 + omega * x0;
            off = (x0 + c * dt) * e;
            vel = (v0 - omega * c * dt) * e;
            if (off > maxOffset) { off = maxOffset; vel = 0; }
            else if (off < -maxOffset) { off = -maxOffset; vel = 0; }
            groundedBefore = groundedNow;
        }

        /// <summary>The offset drawn this frame: between its value before and after the last sim step at the frame's alpha (as the feet).</summary>
        public double Draw(double alpha)
        {
            double a = alpha < 0 ? 0 : alpha > 1 ? 1 : alpha;
            return offPrev + (off - offPrev) * a;
        }

        /// <summary>A new run, a respawn, a screen: no offset.</summary>
        public void Reset()
        {
            off = vel = offPrev = 0;
            have = false;
            groundedBefore = false;
        }
    }

    /// <summary>NIGHT55 C: run lines across a floor step (the a255 reel's and `n55look step`'s scan; see the file's header).</summary>
    public static class Move55Lines
    {
        public struct Line
        {
            public Vec3 start, along;
            public double length, stepAt, rise;
            public string room;
        }

        /// <summary>The floor under (x, z) from `top` down (within 4 m): its height and whether it is flat.</summary>
        public static bool Floor(GameWorld world, double x, double z, double top, out double y, out bool flat)
        {
            y = 0;
            flat = false;
            if (world == null || !world.RayIntersect(new Vec3(x, top, z), new Vec3(0, -1, 0), out RayHit h) || h.distance > 4) return false;
            y = top - h.distance;
            flat = h.normal.y >= UnityTuning.Step55ScanFlat;
            return true;
        }

        /// <summary>One heading from `f0` (on its floor) along `u`: the clear length and the first step on it (stepAt m, signed rise; 0 none).</summary>
        public static double Scan(GameWorld world, Vec3 f0, Vec3 u, out double stepAt, out double rise)
        {
            stepAt = rise = 0;
            double top = f0.y + 2, prevY = f0.y, clear = 0;
            bool prevFlat = true;
            for (double d = UnityTuning.Step55ScanStep; d <= UnityTuning.Step55ScanRun; d += UnityTuning.Step55ScanStep)
            {
                // a wall between the samples at the knee and the chest ends the line
                double kx = f0.x + u.x * (d - UnityTuning.Step55ScanStep), kz = f0.z + u.z * (d - UnityTuning.Step55ScanStep);
                bool blocked = false;
                foreach (double hy in new[] { 0.6, 1.4 })
                    if (world.RayIntersect(new Vec3(kx, prevY + hy, kz), u, out RayHit b) && b.distance < UnityTuning.Step55ScanStep + 0.45) { blocked = true; break; }
                if (blocked) break;
                if (!Floor(world, f0.x + u.x * d, f0.z + u.z * d, top, out double y, out bool flat)) break;
                double r = y - prevY;
                if (Math.Abs(r) > UnityTuning.Step55ScanLedge) break;
                if (rise == 0 && flat && prevFlat && Math.Abs(r) >= UnityTuning.Step55ScanStepMin && Math.Abs(r) <= UnityTuning.Step55ScanStepMax
                    && d >= UnityTuning.Step55ScanFrom) { stepAt = d; rise = r; }
                prevY = y;
                prevFlat = flat;
                top = y + 2;
                clear = d;
            }
            if (rise != 0 && stepAt > clear - UnityTuning.Step55ScanBefore) stepAt = rise = 0; // too near the end to cross at a run
            return clear;
        }

        /// <summary>Every step line from a grid over a room's rect (x0, z0, x1, z1) whose floor is near `floorY` (the room's spawn height).</summary>
        public static void Room(GameWorld world, string id, double x0, double z0, double x1, double z1, double floorY, List<Line> into)
        {
            double inset = UnityTuning.Step55ScanInset, g = UnityTuning.Step55ScanGrid;
            for (double x = Math.Min(x0, x1) + inset; x <= Math.Max(x0, x1) - inset; x += g)
                for (double z = Math.Min(z0, z1) + inset; z <= Math.Max(z0, z1) - inset; z += g)
                {
                    if (!Floor(world, x, z, floorY + 3, out double fy, out bool flat) || !flat) continue;
                    var f0 = new Vec3(x, fy, z);
                    for (int k = 0; k < UnityTuning.Step55ScanHeadings; k++)
                    {
                        double a = 2 * Math.PI * k / UnityTuning.Step55ScanHeadings;
                        var u = new Vec3(Math.Sin(a), 0, Math.Cos(a));
                        double len = Scan(world, f0, u, out double at, out double rise);
                        if (rise == 0) continue;
                        into.Add(new Line { start = f0, along = u, length = len, stepAt = at, rise = rise, room = id });
                    }
                }
        }

        /// <summary>Removes the drops deeper than the Mover's ground snap (collision.groundSnap): the Mover falls off those (a landing, never a
        /// grounded step: `n55look step` on n52-kiln's 0.40 m boxes); returns how many were removed.</summary>
        public static int DropFalls(List<Line> lines, double groundSnap) => lines.RemoveAll(l => l.rise < 0 && -l.rise > groundSnap + 1e-6);

        /// <summary>One line per distinct step (its room, its rise to the cm, where it is to the metre), the list's order kept: the a255 reel and
        /// `n55look step` try different steps, not one lip from many grid starts.</summary>
        public static List<Line> Distinct(List<Line> lines)
        {
            var seen = new HashSet<string>();
            var o = new List<Line>();
            foreach (Line l in lines)
            {
                double sx = l.start.x + l.along.x * l.stepAt, sz = l.start.z + l.along.z * l.stepAt;
                if (seen.Add(l.room + "/" + Math.Round(l.rise * 100) + "/" + Math.Round(sx) + "/" + Math.Round(sz))) o.Add(l);
            }
            return o;
        }

        /// <summary>Down steps first, then the larger step, then the longer line.</summary>
        public static int Rank(Line a, Line b)
        {
            int da = a.rise < 0 ? 0 : 1, db = b.rise < 0 ? 0 : 1;
            if (da != db) return da.CompareTo(db);
            int c = Math.Abs(b.rise).CompareTo(Math.Abs(a.rise));
            return c != 0 ? c : b.length.CompareTo(a.length);
        }
    }
}
