// NIGHT40 builder S1 (rods40, a view row: design.n40.rods40; GG 2026-10-13: "Many arenas contain awkwardly placed white rods that you clip
// through"; NIGHT40.md S1): the rule "no drawn piece without a collider inside the play volume". A point is in the play volume when it lies inside
// a room's rect, outside every collision box, Rods40FloorMin..Rods40FloorMax above the floor under it (the highest collision box top below it) and
// more than Rods40WallClear from every collision box beside it (horizontally, at its height) and from any box over it. A piece (an AABB, or a beam's segment) is IN the play
// volume when any of its samples is; with rods40 on the views skip such a piece: dress33's decor props (PropsView29 / Kit33Props: truss, chain,
// lamp, ...), the light plan's shafts and pole lamps (LightKit29 Beams33 / Fixtures33), and the crown38 spire slits (B5's Crown37View.Crown38,
// through Rods40.Hide). Pure (no UnityEngine): the harness's `n40start rods` scan runs the same test. Off (the row off, --off40 /
// -skygrindOff40): every piece is drawn as NIGHT39 drew it.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Rods40
    {
        /// <summary>Is the view row on (design.n40.rods40.enabled, not forced off by --off40 / -skygrindOff40 or an older switch)?</summary>
        public static bool On(GameConfig cfg) => cfg != null && !N40Blocks.ForceOff && cfg.Design("n40.rods40.enabled");

        /// <summary>Is the point (web space) in the play volume of this level?</summary>
        public static bool InPlay(LightPlan33.Level lv, double x, double y, double z)
        {
            bool inRoom = false;
            foreach (LightPlan33.Room r in lv.rooms)
                if (x >= r.x0 && x <= r.x1 && z >= r.z0 && z <= r.z1) { inRoom = true; break; }
            if (!inRoom) return false;
            double floor = double.NegativeInfinity;
            foreach (LightPlan33.Box b in lv.solid)
            {
                bool over = x >= b.x0 && x <= b.x1 && z >= b.z0 && z <= b.z1;
                if (over && y >= b.y0 && y <= b.y1) return false; // inside a collider
                if (over && b.y0 > y && b.y0 - y <= UnityTuning.Rods40WallClear) return false; // flush under a ceiling or a deck
                if (over && b.y1 <= y && b.y1 > floor) floor = b.y1;
            }
            double h = y - floor;
            if (!(h >= UnityTuning.Rods40FloorMin && h <= UnityTuning.Rods40FloorMax)) return false;
            foreach (LightPlan33.Box b in lv.solid)
            {
                if (!(y > b.y0 && y < b.y1)) continue; // only a wall beside the point at its height
                double dx = Math.Max(0, Math.Max(b.x0 - x, x - b.x1)), dz = Math.Max(0, Math.Max(b.z0 - z, z - b.z1));
                if (dx * dx + dz * dz <= UnityTuning.Rods40WallClear * UnityTuning.Rods40WallClear) return false; // flush with a wall
            }
            return true;
        }

        /// <summary>The share of this AABB's samples (web space, a grid every Rods40StepXZ / StepY, corners included) in the play volume; `at`
        /// gets the first such sample.</summary>
        public static double BoxShare(LightPlan33.Level lv, LightPlan33.Box b, out Vec3 at)
        {
            at = default;
            if (lv == null) return 0;
            int nx = Math.Max(1, (int)Math.Ceiling((b.x1 - b.x0) / UnityTuning.Rods40StepXZ)), nz = Math.Max(1, (int)Math.Ceiling((b.z1 - b.z0) / UnityTuning.Rods40StepXZ));
            int ny = Math.Max(1, (int)Math.Ceiling((b.y1 - b.y0) / UnityTuning.Rods40StepY));
            int all = 0, free = 0;
            for (int i = 0; i <= nx; i++)
                for (int k = 0; k <= nz; k++)
                    for (int j = 0; j <= ny; j++)
                    {
                        double x = b.x0 + (b.x1 - b.x0) * i / nx, y = b.y0 + (b.y1 - b.y0) * j / ny, z = b.z0 + (b.z1 - b.z0) * k / nz;
                        all++;
                        if (!InPlay(lv, x, y, z)) continue;
                        if (free++ == 0) at = new Vec3(x, y, z);
                    }
            return (double)free / all;
        }

        /// <summary>The share of the segment a -> b's samples (a beam's axis, web space) in the play volume.</summary>
        public static double SegmentShare(LightPlan33.Level lv, Vec3 a, Vec3 b, out Vec3 at)
        {
            at = default;
            if (lv == null) return 0;
            double len = Math.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y) + (b.z - a.z) * (b.z - a.z));
            int n = Math.Max(1, (int)Math.Ceiling(len / Math.Min(UnityTuning.Rods40StepXZ, UnityTuning.Rods40StepY)));
            int free = 0;
            for (int i = 0; i <= n; i++)
            {
                double t = (double)i / n;
                double x = a.x + (b.x - a.x) * t, y = a.y + (b.y - a.y) * t, z = a.z + (b.z - a.z) * t;
                if (!InPlay(lv, x, y, z)) continue;
                if (free++ == 0) at = new Vec3(x, y, z);
            }
            return (double)free / (n + 1);
        }

        /// <summary>Is this AABB a rod (thin across, long)?</summary>
        public static bool IsRod(LightPlan33.Box b)
        {
            double w = b.x1 - b.x0, d = b.z1 - b.z0, h = b.y1 - b.y0;
            return Math.Min(w, d) <= UnityTuning.Rods40ThinMax && Math.Max(Math.Max(w, d), h) >= UnityTuning.Rods40LongMin;
        }

        /// <summary>The rule for one collider-less box: a rod with Rods40FreeShare of it in the play volume, or any piece wholly in it.</summary>
        public static bool BoxHidden(LightPlan33.Level lv, LightPlan33.Box b, out Vec3 at)
        {
            double share = BoxShare(lv, b, out at);
            return IsRod(b) ? share >= UnityTuning.Rods40FreeShare : share >= 1;
        }

        /// <summary>A plan prop row's AABB (web space; `at` the centre, `size` [w, h, d] turned by `yawDeg` about +y). False: no at / size.</summary>
        public static bool PropBox(Dictionary<string, object> r, out LightPlan33.Box box)
        {
            box = default;
            if (r == null || !r.TryGetValue("at", out object ao) || !r.TryGetValue("size", out object so) || !(ao is List<object> a) || !(so is List<object> s) || a.Count < 3 || s.Count < 3) return false;
            double yaw = r.TryGetValue("yawDeg", out object yo) ? PortData.Num(yo) * Math.PI / 180 : 0;
            double hw = PortData.Num(s[0]) / 2, hh = PortData.Num(s[1]) / 2, hd = PortData.Num(s[2]) / 2;
            double c = Math.Abs(Math.Cos(yaw)), sn = Math.Abs(Math.Sin(yaw));
            double ex = hw * c + hd * sn, ez = hw * sn + hd * c;
            box = LightPlan33.Box.Around(PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]), ex, hh, ez);
            return true;
        }

        /// <summary>A prop row the view draws with no collider (dress33's decor rows: `decor: true` or `collider: false`).</summary>
        public static bool NoCollider(Dictionary<string, object> r) =>
            r != null && ((r.TryGetValue("decor", out object d) && d is bool db && db) || (r.TryGetValue("collider", out object c) && c is bool cb && !cb));

        /// <summary>A row the rule never skips: a pickup (a weapon, health: gameplay) or a kit kind that wraps an opening (UnityTuning.Rods40KeepKinds).</summary>
        public static bool IsPickup(Dictionary<string, object> r) => r != null && r.TryGetValue("kind", out object k) && k is string ks && (ks.EndsWith("Pickup", StringComparison.Ordinal) || Array.IndexOf(UnityTuning.Rods40KeepKinds, ks) >= 0);

        /// <summary>The views' test (rods40 on): skip this prop row? `at` gets a sample in the play volume.</summary>
        public static bool HideProp(LightPlan33.Level lv, Dictionary<string, object> r, out Vec3 at)
        {
            at = default;
            return NoCollider(r) && !IsPickup(r) && PropBox(r, out LightPlan33.Box b) && BoxHidden(lv, b, out at);
        }

        /// <summary>The views' test (rods40 on): skip this light-plan fixture? A shaft (beam) when more than half its axis is in the play volume; a
        /// pole lamp's parts (plate, shaft, head: one id) by the pole's shaft; every other fixture by its box.</summary>
        public static bool HideFix(LightPlan33.Level lv, LightPlan33.Fix f, LightPlan33.Plan plan)
        {
            if (lv == null || f == null) return false;
            if (f.kind == "beam") return SegmentShare(lv, new Vec3(f.tx, f.ty, f.tz), new Vec3(f.bx, f.by, f.bz), out _) > 0.5;
            if (f.type == "pole" && plan != null)
            {
                foreach (LightPlan33.Fix p in plan.fixes)
                    if (p.type == "pole" && p.id == f.id && p.kind == "body" && p.hasHolder) return BoxHidden(lv, p.box, out _); // the pole's shaft
                return false;
            }
            return BoxHidden(lv, f.box, out _);
        }

        /// <summary>Crown38's spire slits and any other runtime piece: skip this box (web space, centre and half sizes) by the rule (rods40 on)?</summary>
        public static bool Hide(LightPlan33.Level lv, double cx, double cy, double cz, double hx, double hy, double hz) =>
            BoxHidden(lv, LightPlan33.Box.Around(cx, cy, cz, hx, hy, hz), out _);
    }
}
