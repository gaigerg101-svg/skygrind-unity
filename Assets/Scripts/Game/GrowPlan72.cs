// NIGHT72 builder L (L5; GG 2026-10-26 "there's like weird inconsistent lighting in areas there's flickers there's textures overlapping that bug out"): the grow plan.
// Two boxes of a level with a face on one plane, facing the same way and overlapping, both draw there, and the depth test flips between them as the view moves. NIGHT71's
// Bright71.Grow staggered the drawn boxes by index mod 3 (2 / 4 / 6 mm), which separates a pair only when the two indices fall in different classes: Tools/night/n72-zscan.cjs
// counts what is left on the shipped levels (the coplanar overlapping pairs, raw / after mod 3 / after this plan): foundry 75 / 26 / 0, iceline 33 / 10 / 0, kiln 55 / 16 / 0,
// span 28 / 10 / 0, spire 65 / 20 / 0 (82 pairs, 1 088 m2 on span alone, were still fighting).
// This plan gives each box of a map a growth class instead, chosen so no coplanar overlapping pair is left: boxes in index order, each takes the first class 0..K-1 whose
// separation from every earlier conflicting box is at least SepGood (4 mm), else the class with the best separation. A conflict is a same-side face pair (both min faces or both
// max faces on one axis) whose rectangles overlap by at least MinArea (0.01 m2) and whose raw gap is within Band (a class difference can cancel a small raw gap, so the
// pair is judged on its gap AFTER growth: |gap + s x (e_b - e_a)|, s = +1 on a max face, -1 on a min face). A box with no conflict stays at class 0 (2 mm). It is the same
// algorithm as the scan (the tool and this class agree on every level: Tests/EditMode/Game/GrowPlan72Tests.cs, and the scratch cross-check in Research/2026-10-27-wave72/L/evidence).
// View only: the DRAWN box grows, never the collider (the sim reads the level data). Computed once per map build, O(n^2) over about 200 boxes. Off: -skygrindNoGrow72
// (the NIGHT71 growth, index mod 3, comes back) or -skygrindNoBright71 (no growth at all). Pure (no UnityEngine call): a scratch console compiles it with Skygrind.Core.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class GrowPlan72
    {
        public const string NoSwitch = "-skygrindNoGrow72";

        /// <summary>The pairs counted on one map: the drawn boxes, the coplanar overlapping pairs raw (no growth), after the NIGHT71 growth (index mod 3) and after the plan.</summary>
        public struct Report
        {
            public int boxes, raw, mod3, plan, soft;
        }

        struct Conflict
        {
            public int a, b, ax;
            public bool max;
            public double g;
        }

        static double C(Vec3 v, int ax) => ax == 0 ? v.x : ax == 1 ? v.y : v.z;

        /// <summary>The growth (m) of class c: Bright71's step x (1 + c).</summary>
        public static double Growth(int cls) => UnityTuning.Bright71GrowStep * (1 + cls);

        static double Band => (UnityTuning.L72GrowClasses - 1) * UnityTuning.Bright71GrowStep + UnityTuning.L72GrowSepGood + 1e-9;

        static List<Conflict> Conflicts(IReadOnlyList<MapBox> boxes)
        {
            var list = new List<Conflict>();
            double band = Band, minArea = UnityTuning.L72GrowMinArea;
            for (int a = 0; a < boxes.Count; a++)
            {
                MapBox A = boxes[a];
                for (int b = a + 1; b < boxes.Count; b++)
                {
                    MapBox B = boxes[b];
                    for (int ax = 0; ax < 3; ax++)
                    {
                        double area = 1;
                        for (int k = 0; k < 3; k++)
                        {
                            if (k == ax) continue;
                            area *= Math.Max(0, Math.Min(C(A.max, k), C(B.max, k)) - Math.Max(C(A.min, k), C(B.min, k)));
                        }
                        if (area < minArea) continue;
                        double gmin = C(B.min, ax) - C(A.min, ax), gmax = C(B.max, ax) - C(A.max, ax);
                        if (Math.Abs(gmin) <= band) list.Add(new Conflict { a = a, b = b, ax = ax, max = false, g = gmin });
                        if (Math.Abs(gmax) <= band) list.Add(new Conflict { a = a, b = b, ax = ax, max = true, g = gmax });
                    }
                }
            }
            return list;
        }

        static double GapAfter(Conflict c, double ea, double eb) => Math.Abs(c.g + (c.max ? 1 : -1) * (eb - ea));

        /// <summary>The class (0..K-1) of every box of the map, in index order (the order SurvivalBuilder draws them).</summary>
        public static int[] Classes(IReadOnlyList<MapBox> boxes)
        {
            int n = boxes.Count, K = UnityTuning.L72GrowClasses;
            var cls = new int[n];
            if (n < 2) return cls;
            List<Conflict> all = Conflicts(boxes);
            var byB = new List<Conflict>[n];
            foreach (Conflict c in all)
            {
                if (byB[c.b] == null) byB[c.b] = new List<Conflict>();
                byB[c.b].Add(c);
            }
            double good = UnityTuning.L72GrowSepGood;
            for (int b = 0; b < n; b++)
            {
                if (byB[b] == null) continue;
                int best = -1;
                double bestSep = -1;
                for (int c = 0; c < K; c++)
                {
                    double sep = double.PositiveInfinity;
                    foreach (Conflict p in byB[b]) sep = Math.Min(sep, GapAfter(p, Growth(cls[p.a]), Growth(c)));
                    if (sep >= good - 1e-12) { best = c; bestSep = sep; break; }
                    if (sep > bestSep + 1e-12) { best = c; bestSep = sep; }
                }
                cls[b] = best;
            }
            return cls;
        }

        /// <summary>The pairs left under a growth: `growth` is each box's growth in m (the same length as boxes); a pair is counted when its gap after growth is under tol.</summary>
        static int Left(List<Conflict> all, double[] growth, double tol, out int soft)
        {
            int n = 0;
            soft = 0;
            double good = UnityTuning.L72GrowSepGood;
            foreach (Conflict c in all)
            {
                double gap = GapAfter(c, growth[c.a], growth[c.b]);
                if (gap < tol) n++;
                else if (gap < good - 1e-12) soft++;
            }
            return n;
        }

        /// <summary>The scan's three counts for a map (tol 1 mm): raw, the shipped mod-3 growth, and `classes`.</summary>
        public static Report Count(IReadOnlyList<MapBox> boxes, int[] classes, double tol = 0.001)
        {
            List<Conflict> all = Conflicts(boxes);
            int n = boxes.Count;
            var zero = new double[n];
            var m3 = new double[n];
            var pl = new double[n];
            for (int i = 0; i < n; i++)
            {
                m3[i] = UnityTuning.Bright71GrowStep * (1 + i % UnityTuning.Bright71GrowSteps);
                pl[i] = Growth(classes != null ? classes[i] : 0);
            }
            return new Report { boxes = n, raw = Left(all, zero, tol, out _), mod3 = Left(all, m3, tol, out _), plan = Left(all, pl, tol, out int soft), soft = soft };
        }

        /// <summary>The Player's proof line (L-089) when a map's stone is built: one per map, nothing with the plan off.</summary>
        public static string LogLine(string map, Report r) =>
            "[grow72] " + map + ": " + r.boxes + " drawn boxes, coplanar overlapping pairs raw " + r.raw + ", after the NIGHT71 growth " + r.mod3 + ", after the plan " + r.plan + (r.soft > 0 ? " (" + r.soft + " at 1-4 mm)" : "");

        static bool? off;

        /// <summary>-skygrindNoGrow72 on this process's command line (read once): the NIGHT71 growth, index mod 3.</summary>
        public static bool Off
        {
            get
            {
                if (off == null) off = NightRows.HasSwitch(Environment.GetCommandLineArgs(), NoSwitch);
                return off.Value;
            }
        }
    }
}
