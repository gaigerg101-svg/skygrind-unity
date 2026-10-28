// NIGHT29 (builder R): the room standard's extra keys of a level plan, beside LevelPlan24 (pure, no UnityEngine). A plan opts in with
// `"n29": true` at the top level; a plan without it converts exactly as before (every NIGHT24-28 plan, tape and test is untouched).
//   room `height`  the ring walls' height for that room (default: the plan's wallHeight)
//   room `roof`    true: a collider ROOF slab (1 m thick, lying on the ring walls, covering the room's rect and its ring): no sky gap
//   door `height`  the doorway's clear height: a hull LINTEL fills the wall opening above it (the frame the view builds is this opening)
//   plan `lights`  [{id, room, kind: wall|ceiling|pool, at, normal, size [w,h,d], color, range, intensity, collider?, danger?}] light SOCKETS
//   plan `props`   [{id, room, kind, at, size [w,h,d], yawDeg, collider, color?}] AUTHORED props; a collider prop becomes a hull box (its AABB)
// `lights` and `props` ride into the map JSON's `campaign` block unchanged (builder V's views read them); LevelValidate29 holds them to the standard.
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.EditorTools
{
    public static class LevelPlan29
    {
        static Dictionary<string, object> O(object o) => o as Dictionary<string, object>;
        static List<object> L(object o) => o as List<object> ?? new List<object>();
        static double N(object o) => PortData.Num(o);
        static object Get(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out object v) ? v : null;
        static string S(Dictionary<string, object> d, string k) => Get(d, k) as string ?? "";

        /// <summary>The plan asks for the NIGHT29 room standard.</summary>
        public static bool Wanted(Dictionary<string, object> plan) => Get(plan, "n29") is bool b && b;

        static List<object> Arr(params double[] v)
        {
            var l = new List<object>();
            foreach (double d in v) l.Add(d);
            return l;
        }

        /// <summary>The ring wall height of one room row: its `height`, else the plan's wallHeight.</summary>
        public static double RoomHeight(Dictionary<string, object> room, double wallH) => Get(room, "height") != null ? N(room["height"]) : wallH;

        static Dictionary<string, object> RoomById(Dictionary<string, object> plan, string id)
        {
            foreach (object o in L(plan["rooms"])) if (S(O(o), "id") == id) return O(o);
            return null;
        }

        /// <summary>The boxes the extra keys add: a roof per `roof` room, a lintel per door with a `height`, a hull box per collider prop.</summary>
        public static List<object> ExtraBoxes(Dictionary<string, object> plan, double wallThick)
        {
            var list = new List<object>();
            if (!Wanted(plan)) return list;
            double wallH = plan.ContainsKey("wallHeight") ? N(plan["wallHeight"]) : 14;
            foreach (object o in L(plan["rooms"]))
            {
                var r = O(o);
                if (!(Get(r, "roof") is bool roof) || !roof) continue;
                List<object> rc = L(r["rect"]);
                double h = RoomHeight(r, wallH);
                list.Add(new Dictionary<string, object>
                {
                    ["zone"] = S(r, "id"), ["kind"] = "hull",
                    ["min"] = Arr(N(rc[0]) - wallThick, h, N(rc[1]) - wallThick), ["max"] = Arr(N(rc[2]) + wallThick, h + 1, N(rc[3]) + wallThick),
                    ["note"] = "roof of " + S(r, "id"),
                });
            }
            foreach (object o in L(plan["doors"]))
            {
                var d = O(o);
                if (Get(d, "height") == null) continue;
                double dh = N(d["height"]);
                List<object> dr = L(d["rect"]);
                double top = 0;
                foreach (object bo in L(d["between"]))
                {
                    var rr = RoomById(plan, bo as string);
                    if (rr != null) top = Math.Max(top, RoomHeight(rr, wallH));
                }
                if (top <= dh + 1e-9) continue;
                list.Add(new Dictionary<string, object>
                {
                    ["zone"] = S(d, "id"), ["kind"] = "hull",
                    ["min"] = Arr(N(dr[0]), dh, N(dr[1])), ["max"] = Arr(N(dr[2]), top, N(dr[3])),
                    ["note"] = "lintel of " + S(d, "id"),
                });
            }
            foreach (object o in L(Get(plan, "props")))
            {
                var p = O(o);
                if (!(Get(p, "collider") is bool c) || !c) continue;
                Aabb(p, out double[] mn, out double[] mx);
                list.Add(new Dictionary<string, object>
                {
                    ["zone"] = S(p, "room"), ["kind"] = "hull", ["min"] = Arr(mn[0], mn[1], mn[2]), ["max"] = Arr(mx[0], mx[1], mx[2]),
                    ["note"] = "prop " + S(p, "id") + " (" + S(p, "kind") + ")",
                });
            }
            foreach (object o in L(Get(plan, "lights")))
            {
                var p = O(o);
                if (!(Get(p, "collider") is bool c) || !c) continue; // a light deeper than maxWallDepth is solid
                // size is [w, h, d]: d along the normal; w along z (normal x), x (normal y or z); h along y (normal x or z), z (normal y)
                List<object> at = L(p["at"]), sz = L(p["size"]), nm = L(p["normal"]);
                double w = N(sz[0]), h = N(sz[1]), d = N(sz[2]);
                double ex, ey, ez;
                if (Math.Abs(N(nm[0])) > 0.5) { ex = d; ey = h; ez = w; }
                else if (Math.Abs(N(nm[1])) > 0.5) { ex = w; ey = d; ez = h; }
                else { ex = w; ey = h; ez = d; }
                list.Add(new Dictionary<string, object>
                {
                    ["zone"] = S(p, "room"), ["kind"] = "hull",
                    ["min"] = Arr(N(at[0]) - ex / 2, N(at[1]) - ey / 2, N(at[2]) - ez / 2), ["max"] = Arr(N(at[0]) + ex / 2, N(at[1]) + ey / 2, N(at[2]) + ez / 2),
                    ["note"] = "light " + S(p, "id") + " (collider)",
                });
            }
            return list;
        }

        /// <summary>A prop's world AABB from `at` (its centre), `size` [w, h, d] and `yawDeg` (about y; w along x, d along z at yaw 0).</summary>
        public static void Aabb(Dictionary<string, object> p, out double[] min, out double[] max)
        {
            List<object> at = L(p["at"]), sz = L(p["size"]);
            double yaw = Get(p, "yawDeg") != null ? N(p["yawDeg"]) * Math.PI / 180 : 0;
            double c = Math.Abs(JsMath.Cos(yaw)), s = Math.Abs(JsMath.Sin(yaw));
            double w = N(sz[0]), h = N(sz[1]), d = N(sz[2]);
            double ex = (w * c + d * s) / 2, ez = (w * s + d * c) / 2;
            min = new[] { N(at[0]) - ex, N(at[1]) - h / 2, N(at[2]) - ez };
            max = new[] { N(at[0]) + ex, N(at[1]) + h / 2, N(at[2]) + ez };
        }

        /// <summary>The plan keys the map JSON's `campaign` block carries for the room standard.</summary>
        /// NIGHT31 (builder L): + "n30", so the report's spawn31 row (LevelValidate29.Spawn31) runs only on the NIGHT30 levels (no sim code reads it).
        public static readonly string[] CampaignKeys = { "n29", "lights", "props", "boxes", "n30" };

        /// <summary>NIGHT30 (builder L): the LAVA rows. A pit row with `lava: true` is a lava trench: its `floorY` is the lava SURFACE (0.6-1.0 m below the floor,
        /// so a player knocked up clears the rim) and the room carries no `respawnY` over it (lava burns and knocks, it never teleports; only a void respawns).
        /// The map JSON gets a top-level array `lava: [{rect: [x0, z0, x1, z1], y: surfaceY, room: id}]`, one entry per lava pit, which the sim (design.n30.lava,
        /// builder S) and the lava view (builder V) read. Empty for a plan with no lava pit (no key is written then, so every older map is byte-identical).</summary>
        public static List<object> Lava(Dictionary<string, object> plan)
        {
            var list = new List<object>();
            foreach (object o in L(Get(plan, "pits")))
            {
                var p = O(o);
                if (!(Get(p, "lava") is bool lv) || !lv) continue;
                List<object> rc = L(p["rect"]);
                list.Add(new Dictionary<string, object>
                {
                    ["rect"] = Arr(N(rc[0]), N(rc[1]), N(rc[2]), N(rc[3])), ["y"] = N(p["floorY"]), ["room"] = S(p, "zone"),
                });
            }
            return list;
        }

        /// <summary>Writes the `lava` array into the map JSON (only when the plan has a lava pit).</summary>
        public static void AddLava(Dictionary<string, object> plan, Dictionary<string, object> json)
        {
            List<object> lava = Lava(plan);
            if (lava.Count > 0) json["lava"] = lava;
            // NIGHT30 (builder L): GRAPPLE NODES. A room row's `grappleNodes` [[x, y, z], ...] (world metres) are gathered, in room order, into the map JSON's
            // top-level `grappleNodes` (SurvivalMap reads it; with design.n30.span on, a campaign level that has nodes owns THE ARCLINE: SurvivalSim.Systems30.cs).
            // No node in the plan: no key (older maps unchanged). The map's format is {x, y, z} per node (SurvivalMap.P); the plan's rows are [x, y, z].
            var nodes = new List<object>();
            foreach (object o in L(Get(plan, "rooms")))
                foreach (object n in L(Get(O(o), "grappleNodes"))) { List<object> v = L(n); nodes.Add(new Dictionary<string, object> { ["x"] = N(v[0]), ["y"] = N(v[1]), ["z"] = N(v[2]) }); }
            if (nodes.Count > 0) json["grappleNodes"] = nodes;
        }
    }
}
