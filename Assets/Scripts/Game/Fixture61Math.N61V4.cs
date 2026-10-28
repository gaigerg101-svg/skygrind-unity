// NIGHT61 builder V (slate S4, lights hang from real fixtures; view row design.n61.fixture61): the mounts' pure parts, in web space and Unity-free
// (like LightPlan33 and Rods40, whose types it reads), so EditMode tests and a harness run them (N61FixtureTests). Fixture61.N61V4.cs draws them.
// A CEILING LIGHT is a light source the plan draws on a ceiling: a room's key panel (look33 / lights35: flush at the middle of a roofed room's
// ceiling, a 0.08 m frame round it), a pool's downlight, a ceiling socket's panel. Each gets:
//  - a HOUSING: four dark walls Dims.rim wide just outside its frame, from the ceiling down Dims.drop (the lens stays in view, recessed in a fitting);
//  - HANGERS: a rail (Dims.hangerW x Dims.hangerH, Dims.hangerTop under the ceiling) from the housing to the nearest joist on each side within
//    Dims.beamMax (lights35's dark beams; the joists are the plan's own, after rods40's filter), else one rail to the nearest wall face within
//    Dims.wallMax. Every part stays within Rods40WallClear under its ceiling (the tuning says so; N61FixtureTests checks it).
// THE METER (Count): a ceiling light is MOUNTED when its dark border (the frame plus a housing) is at least minRim wide AND a hanger holds it to a
// beam or a wall; the red (no mounts) counts every ceiling light unmounted. Nothing here changes the plan: no light moves, dims or flickers.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Fixture61Math
    {
        /// <summary>The view row's name (design.n61.fixture61).</summary>
        public const string Row = "fixture61";
        /// <summary>The one-row off switch: this row alone off.</summary>
        public const string NoSwitch = "-skygrindNoFixture61";
        /// <summary>The night of the row (its -skygrindOffNN switch covers it from this night on).</summary>
        public const int Night = 61;

        /// <summary>One mount part: its box (web space), what it is ("housing", "beam hanger", "wall hanger") and the index in the plan's fixes of the
        /// ceiling light it holds.</summary>
        public readonly struct Mount
        {
            public readonly LightPlan33.Box box;
            public readonly string kind;
            public readonly int light;

            public Mount(LightPlan33.Box box, string kind, int light)
            {
                this.box = box;
                this.kind = kind;
                this.light = light;
            }
        }

        /// <summary>The mounts' numbers (m): the housing's rim and drop, the plan's frame width round a panel, a hanger's width, height and top under
        /// the ceiling, the farthest joist and wall a hanger reaches.</summary>
        public readonly struct Dims
        {
            public readonly double rim, drop, frame, hangerW, hangerH, hangerTop, beamMax, wallMax;

            public Dims(double rim, double drop, double frame, double hangerW, double hangerH, double hangerTop, double beamMax, double wallMax)
            {
                this.rim = rim;
                this.drop = drop;
                this.frame = frame;
                this.hangerW = hangerW;
                this.hangerH = hangerH;
                this.hangerTop = hangerTop;
                this.beamMax = beamMax;
                this.wallMax = wallMax;
            }

            /// <summary>The numbers the view uses (UnityTuning.N61V4.cs; the frame is LightPlan33's Look33FrameWidth).</summary>
            public static Dims Tuning => new Dims(UnityTuning.Fixture61RimW, UnityTuning.Fixture61Drop, UnityTuning.Look33FrameWidth, UnityTuning.Fixture61HangerW,
                UnityTuning.Fixture61HangerH, UnityTuning.Fixture61HangerTop, UnityTuning.Fixture61BeamMaxM, UnityTuning.Fixture61WallMaxM);
        }

        /// <summary>The meter's counts for one plan: ceiling lights, housed (border >= minRim), held by a beam, by a wall (no beam), unmounted.</summary>
        public struct Report
        {
            public int lights, housed, beam, wall, unmounted;
        }

        /// <summary>A light source on a ceiling: the room's key panel, a pool's downlight, a ceiling socket's panel (LightPlan33's kinds).</summary>
        public static bool CeilingLight(LightPlan33.Fix f) => f != null && f.emissive && (f.kind == "panel" || f.kind == "poolPanel" || (f.kind == "lens" && f.type == "panel"));

        /// <summary>The mounts of every ceiling light of a plan that `drawn` accepts (null: all): its housing and its hangers (see the file header).
        /// The plan is only read.</summary>
        public static List<Mount> Mounts(LightPlan33.Level lv, LightPlan33.Plan p, Dims d, Func<LightPlan33.Fix, bool> drawn = null)
        {
            var list = new List<Mount>();
            if (p == null) return list;
            for (int i = 0; i < p.fixes.Count; i++)
            {
                LightPlan33.Fix f = p.fixes[i];
                if (!CeilingLight(f) || (drawn != null && !drawn(f))) continue;
                LightPlan33.Box b = f.box;
                double cy = b.y1, fy = cy - d.drop;
                double ix0 = b.x0 - d.frame, ix1 = b.x1 + d.frame, iz0 = b.z0 - d.frame, iz1 = b.z1 + d.frame; // the frame's outer edge
                double ox0 = ix0 - d.rim, ox1 = ix1 + d.rim, oz0 = iz0 - d.rim, oz1 = iz1 + d.rim; // the housing's
                list.Add(new Mount(new LightPlan33.Box(ox0, fy, oz0, ox1, cy, iz0), "housing", i));
                list.Add(new Mount(new LightPlan33.Box(ox0, fy, iz1, ox1, cy, oz1), "housing", i));
                list.Add(new Mount(new LightPlan33.Box(ox0, fy, iz0, ix0, cy, iz1), "housing", i));
                list.Add(new Mount(new LightPlan33.Box(ix1, fy, iz0, ox1, cy, iz1), "housing", i));
                double lx = (b.x0 + b.x1) * 0.5, lz = (b.z0 + b.z1) * 0.5;
                double hy1 = cy - d.hangerTop, hy0 = hy1 - d.hangerH, hw = d.hangerW * 0.5;
                int beams = 0;
                for (int dir = 0; dir < 4; dir++) // -z, +z, -x, +x
                {
                    double best = double.MaxValue;
                    foreach (LightPlan33.Fix j in p.fixes)
                    {
                        if (j.type != "joist" || j.room != f.room) continue;
                        double gap = Gap(dir, j.box, lx, lz, 0, ox0, ox1, oz0, oz1, hy0, hy1, true);
                        if (gap >= 0 && gap <= d.beamMax && gap < best) best = gap;
                    }
                    if (best == double.MaxValue) continue;
                    beams++;
                    list.Add(new Mount(Rail(dir, best, lx, lz, hw, ox0, ox1, oz0, oz1, hy0, hy1), "beam hanger", i));
                }
                if (beams > 0 || lv == null) continue;
                int wallDir = -1;
                double wallGap = double.MaxValue;
                for (int dir = 0; dir < 4; dir++)
                    foreach (LightPlan33.Box s in lv.solid)
                    {
                        double gap = Gap(dir, s, lx, lz, hw, ox0, ox1, oz0, oz1, hy0, hy1, false);
                        if (gap >= 0 && gap <= d.wallMax && gap < wallGap) { wallGap = gap; wallDir = dir; }
                    }
                if (wallDir >= 0) list.Add(new Mount(Rail(wallDir, wallGap, lx, lz, hw, ox0, ox1, oz0, oz1, hy0, hy1), "wall hanger", i));
            }
            return list;
        }

        /// <summary>The distance from the housing (outer edges ox0..ox1, oz0..oz1) to box `s` in direction `dir` (0 -z, 1 +z, 2 -x, 3 +x), or -1 when
        /// `s` is not there: it must reach over the hanger's band (hy0..hy1) and cross the hanger's line (lx or lz, widened by `hw`); a joist (`within`)
        /// must span the line (it holds the light from the side), a wall need only meet the rail.</summary>
        static double Gap(int dir, LightPlan33.Box s, double lx, double lz, double hw, double ox0, double ox1, double oz0, double oz1, double hy0, double hy1, bool within)
        {
            if (s.y0 > hy0 || s.y1 < hy1) return -1;
            if (dir < 2)
            {
                if (within ? lx < s.x0 || lx > s.x1 : lx + hw <= s.x0 || lx - hw >= s.x1) return -1;
                double g = dir == 0 ? oz0 - s.z1 : s.z0 - oz1;
                return g >= -1e-9 ? Math.Max(0, g) : -1;
            }
            if (within ? lz < s.z0 || lz > s.z1 : lz + hw <= s.z0 || lz - hw >= s.z1) return -1;
            double h = dir == 2 ? ox0 - s.x1 : s.x0 - ox1;
            return h >= -1e-9 ? Math.Max(0, h) : -1;
        }

        /// <summary>A hanger rail from the housing's edge in direction `dir`, `length` m long, on the light's line, in the hanger's band.</summary>
        static LightPlan33.Box Rail(int dir, double length, double lx, double lz, double hw, double ox0, double ox1, double oz0, double oz1, double hy0, double hy1)
        {
            switch (dir)
            {
                case 0: return new LightPlan33.Box(lx - hw, hy0, oz0 - length, lx + hw, hy1, oz0);
                case 1: return new LightPlan33.Box(lx - hw, hy0, oz1, lx + hw, hy1, oz1 + length);
                case 2: return new LightPlan33.Box(ox0 - length, hy0, lz - hw, ox0, hy1, lz + hw);
                default: return new LightPlan33.Box(ox1, hy0, lz - hw, ox1 + length, hy1, lz + hw);
            }
        }

        /// <summary>THE METER: the ceiling lights of a plan (those `drawn` accepts) and how many the mounts hold (see the file header). With no mounts
        /// every ceiling light is unmounted (its border is the frame alone).</summary>
        public static Report Count(LightPlan33.Plan p, IList<Mount> mounts, double minRim, double frame, Func<LightPlan33.Fix, bool> drawn = null)
        {
            var rep = new Report();
            if (p == null) return rep;
            for (int i = 0; i < p.fixes.Count; i++)
            {
                if (!CeilingLight(p.fixes[i]) || (drawn != null && !drawn(p.fixes[i]))) continue;
                rep.lights++;
                double border = frame;
                bool beam = false, wall = false;
                if (mounts != null)
                    foreach (Mount m in mounts)
                    {
                        if (m.light != i) continue;
                        if (m.kind == "housing") border = Math.Max(border, frame + Math.Min(m.box.x1 - m.box.x0, m.box.z1 - m.box.z0));
                        else if (m.kind == "beam hanger") beam = true;
                        else if (m.kind == "wall hanger") wall = true;
                    }
                bool housed = border >= minRim - 1e-9;
                if (housed) rep.housed++;
                if (beam) rep.beam++;
                else if (wall) rep.wall++;
                if (!housed || !(beam || wall)) rep.unmounted++;
            }
            return rep;
        }

        /// <summary>Is the row off for this config and command line? Off when the row is off (NightRows.ViewOn: its design `enabled`, the process's
        /// -skygrindOffNN), under -skygrindOff61 or an older night's switch, or -skygrindNoFixture61 (this row alone).</summary>
        public static bool OffFor(GameConfig cfg, string[] args) => OffReason(cfg, args) != null;

        /// <summary>Why it is off (for the log line), null when on.</summary>
        public static string OffReason(GameConfig cfg, string[] args)
        {
            if (!NightRows.ViewOn(cfg, Row)) return "design.n61." + Row + " off";
            if (NightRows.OffFrom(args, "-skygrindOff") <= Night) return "-skygrindOff61 (or older)";
            if (NightRows.HasSwitch(args, NoSwitch)) return NoSwitch; // NIGHT63 R1 N5: the one rule (case ignored, as before)
            return null;
        }

        /// <summary>The Player's proof line when a map's mounts are built (L-089: the reel greps "[fixture61] mounts on").</summary>
        public static string LogLine(string level, Report r) =>
            "[fixture61] mounts on " + level + ": ceiling lights " + r.lights + ", housed " + r.housed + ", held by a beam " + r.beam + ", by a wall " + r.wall + ", unmounted " + r.unmounted;
    }
}
