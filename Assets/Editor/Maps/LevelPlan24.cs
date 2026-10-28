// NIGHT24 (builder L): the CAMPAIGN's level plan -> the same map JSON a DRYDOCK-style map is, plus a `campaign` block. Pure (no
// UnityEngine): the headless `levelgen` command (Tools/SimHarness/Harness.N24.Level.cs) and, later, the Editor share it. The plan
// format is documented in Design/levels/LEVEL-FORMAT.md. The conversion reuses DrydockPlan (Assets/Editor/Maps/Build_drydock.cs):
// the level plan is first turned into a DRYDOCK plan (rooms and doors become zones, the rooms' perimeter walls are made with a gap at
// each door, one floor slab covers the level), so every box, ramp, pad, grapple node and plan check is the code the greybox
// map already uses; then the rooms block rides along in the JSON as "campaign" (SurvivalMap.FromExport ignores keys it does not
// know).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.EditorTools;
using Skygrind.Game.MapAuthoring;

namespace Skygrind.EditorTools
{
    public static class LevelPlan24
    {
        public const string Source = "Design/levels (a level plan, LEVEL-FORMAT.md)";
        public const double WallThick = 2;

        public sealed class LevelBuild
        {
            public Dictionary<string, object> drydockPlan;
            public DrydockPlan.DrydockBuild build;
            public Dictionary<string, object> json;
            public string text;
            public List<string> problems = new List<string>();
            public List<string> notes = new List<string>();
        }

        static Dictionary<string, object> O(object o) => o as Dictionary<string, object>;
        static List<object> L(object o) => o as List<object> ?? new List<object>();
        static double N(object o) => PortData.Num(o);
        static string S(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out object v) ? v as string ?? "" : "";
        static object Get(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out object v) ? v : null;
        static List<object> Arr(params double[] v)
        {
            var l = new List<object>();
            foreach (double d in v) l.Add(d);
            return l;
        }

        struct R2
        {
            public double x0, z0, x1, z1;
            public bool Empty => x1 - x0 < 1e-9 || z1 - z0 < 1e-9;
        }

        static R2 Rect(object o)
        {
            List<object> a = L(o);
            return new R2 { x0 = N(a[0]), z0 = N(a[1]), x1 = N(a[2]), z1 = N(a[3]) };
        }

        static List<R2> Subtract(R2 a, R2 b)
        {
            var res = new List<R2>();
            if (b.x0 >= a.x1 || b.x1 <= a.x0 || b.z0 >= a.z1 || b.z1 <= a.z0)
            {
                res.Add(a);
                return res;
            }
            double ix0 = Math.Max(a.x0, b.x0), ix1 = Math.Min(a.x1, b.x1);
            if (b.z0 > a.z0) res.Add(new R2 { x0 = a.x0, z0 = a.z0, x1 = a.x1, z1 = b.z0 });
            if (b.z1 < a.z1) res.Add(new R2 { x0 = a.x0, z0 = b.z1, x1 = a.x1, z1 = a.z1 });
            double mz0 = Math.Max(a.z0, b.z0), mz1 = Math.Min(a.z1, b.z1);
            if (b.x0 > a.x0) res.Add(new R2 { x0 = a.x0, z0 = mz0, x1 = b.x0, z1 = mz1 });
            if (b.x1 < a.x1) res.Add(new R2 { x0 = b.x1, z0 = mz0, x1 = a.x1, z1 = mz1 });
            return res;
        }

        static Dictionary<string, object> Box(double x0, double y0, double z0, double x1, double y1, double z1, string note)
        {
            return new Dictionary<string, object> { ["min"] = Arr(x0, y0, z0), ["max"] = Arr(x1, y1, z1), ["note"] = note };
        }

        /// <summary>The level plan -> a DRYDOCK plan (zones = rooms + doors, walls with door gaps, one floor slab).</summary>
        public static Dictionary<string, object> ToDrydock(Dictionary<string, object> plan, List<string> notes)
        {
            double wallH = plan.ContainsKey("wallHeight") ? N(plan["wallHeight"]) : 14;
            var zones = new List<object>();
            var built = new List<object>();
            var doorRects = new List<R2>();
            var roomRects = new List<R2>();
            double minX = double.MaxValue, minZ = double.MaxValue, maxX = double.MinValue, maxZ = double.MinValue;
            string first = null;
            foreach (object o in L(plan["rooms"]))
            {
                var r = O(o);
                string id = S(r, "id");
                first ??= id;
                R2 rc = Rect(r["rect"]);
                roomRects.Add(rc);
                minX = Math.Min(minX, rc.x0 - WallThick); minZ = Math.Min(minZ, rc.z0 - WallThick);
                maxX = Math.Max(maxX, rc.x1 + WallThick); maxZ = Math.Max(maxZ, rc.z1 + WallThick);
                zones.Add(new Dictionary<string, object>
                {
                    ["id"] = id, ["name"] = S(r, "name"), ["cost"] = 0.0, ["priority"] = 2.0, ["rect"] = Arr(rc.x0, rc.z0, rc.x1, rc.z1), ["built"] = true,
                });
                built.Add(id);
            }
            foreach (object o in L(plan["doors"]))
            {
                var d = O(o);
                R2 dc = Rect(d["rect"]);
                doorRects.Add(dc);
                minX = Math.Min(minX, dc.x0); minZ = Math.Min(minZ, dc.z0); maxX = Math.Max(maxX, dc.x1); maxZ = Math.Max(maxZ, dc.z1);
                zones.Add(new Dictionary<string, object>
                {
                    ["id"] = S(d, "id"), ["name"] = S(d, "name") == "" ? "DOOR" : S(d, "name"), ["cost"] = 0.0, ["priority"] = 3.0, ["rect"] = Arr(dc.x0, dc.z0, dc.x1, dc.z1), ["built"] = true,
                });
                built.Add(S(d, "id"));
            }

            // Perimeter walls: a ring WallThick outside each room's rect, minus every door rect (the gap), merged as is (overlaps are harmless).
            var walls = new List<object>();
            for (int i = 0; i < roomRects.Count; i++)
            {
                R2 c = roomRects[i];
                var sides = new[]
                {
                    new R2 { x0 = c.x0 - WallThick, z0 = c.z0 - WallThick, x1 = c.x1 + WallThick, z1 = c.z0 },
                    new R2 { x0 = c.x0 - WallThick, z0 = c.z1, x1 = c.x1 + WallThick, z1 = c.z1 + WallThick },
                    new R2 { x0 = c.x0 - WallThick, z0 = c.z0, x1 = c.x0, z1 = c.z1 },
                    new R2 { x0 = c.x1, z0 = c.z0, x1 = c.x1 + WallThick, z1 = c.z1 },
                };
                foreach (R2 side in sides)
                {
                    var pieces = new List<R2> { side };
                    foreach (R2 dr in doorRects)
                    {
                        var next = new List<R2>();
                        foreach (R2 p in pieces) next.AddRange(Subtract(p, dr));
                        pieces = next;
                    }
                    foreach (R2 p in pieces) if (!p.Empty) walls.Add(Box(p.x0, 0, p.z0, p.x1, LevelPlan29.RoomHeight(O(L(plan["rooms"])[i]), wallH), p.z1, $"wall of {S(O(L(plan["rooms"])[i]), "id")}")); // NIGHT29: a room's own `height`
                }
            }

            // One floor slab per room (its rect and the wall ring) and per door, not one slab over the bounding box: the void between the
            // rooms gets no floor, so the nav grid (the map JSON's bulk) only covers what can be walked.
            var floor = new List<object>();
            for (int i = 0; i < roomRects.Count; i++)
            {
                R2 c = roomRects[i];
                string rid = S(O(L(plan["rooms"])[i]), "id");
                var slab = new List<R2> { new R2 { x0 = c.x0 - WallThick, z0 = c.z0 - WallThick, x1 = c.x1 + WallThick, z1 = c.z1 + WallThick } };
                // PITS (the platforming stretches): the room's floor is cut by each pit's rect; the pieces around it are solid down to the pit
                // floor (so the pit has real walls), and the pit has its own floor slab at floorY. A failed hop lands there; the plan's boxes
                // put a recovery stair in it (LevelValidate24 proves every pit can be left).
                double bottom = -0.5;
                foreach (object po in L(Get(plan, "pits")))
                {
                    var pit = O(po);
                    if (S(pit, "zone") != rid) continue;
                    R2 pr = Rect(pit["rect"]);
                    double py = N(pit["floorY"]);
                    bottom = Math.Min(bottom, py - 1);
                    var next = new List<R2>();
                    foreach (R2 piece in slab) next.AddRange(Subtract(piece, pr));
                    slab = next;
                    floor.Add(new Dictionary<string, object> { ["min"] = Arr(pr.x0, py - 1, pr.z0), ["max"] = Arr(pr.x1, py, pr.z1), ["kind"] = "floor", ["note"] = "pit floor in " + rid });
                }
                foreach (R2 piece in slab)
                    if (!piece.Empty) floor.Add(new Dictionary<string, object> { ["min"] = Arr(piece.x0, bottom, piece.z0), ["max"] = Arr(piece.x1, 0, piece.z1), ["kind"] = "floor", ["note"] = "floor of " + rid });
            }
            foreach (R2 c in doorRects)
                floor.Add(new Dictionary<string, object> { ["min"] = Arr(c.x0, -0.5, c.z0), ["max"] = Arr(c.x1, 0, c.z1), ["kind"] = "floor", ["note"] = "floor of a door" });

            var spots = new Dictionary<string, object>();
            var ground = new List<object>();
            var high = new List<object>();
            foreach (object o in L(plan["rooms"]))
            {
                var r = O(o);
                foreach (object n in L(Get(r, "nodes"))) ground.Add(n);
                foreach (object n in L(Get(r, "highNodes"))) high.Add(n);
            }
            var firstRoom = O(L(plan["rooms"])[0]);
            var spawn = O(firstRoom["spawn"]);
            spots["player"] = new Dictionary<string, object> { ["at"] = spawn["at"], ["yawDeg"] = spawn.ContainsKey("yawDeg") ? spawn["yawDeg"] : 0.0, ["note"] = "room 1's spawn" };
            spots["spawns"] = new Dictionary<string, object> { ["ground"] = ground, ["high"] = high };
            spots["perks"] = new List<object>();
            spots["wallBuys"] = new List<object>();
            spots["relays"] = new List<object>();
            spots["hold"] = new List<object>();
            spots["box"] = new List<object>();

            return new Dictionary<string, object>
            {
                ["id"] = S(plan, "id"), ["name"] = S(plan, "name"), ["floorSize"] = plan["floorSize"], ["killPlaneY"] = plan.ContainsKey("killPlaneY") ? plan["killPlaneY"] : -30.0,
                ["startZone"] = first, ["built"] = built, ["zones"] = zones, ["floor"] = floor, ["walls"] = walls, ["doorways"] = new List<object>(),
                ["boxes"] = WithTunnels(plan), ["ramps"] = Get(plan, "ramps") ?? new List<object>(),
                ["pads"] = Get(plan, "pads") ?? new List<object>(), ["grapple"] = Get(plan, "grapple") ?? new List<object>(), ["spots"] = spots,
            };
        }

        /// <summary>The plan's boxes plus, for each TUNNEL kit ({zone, rect, ceiling}: a passage a player must SLIDE under), its ceiling slab over the
        /// rect from `ceiling` up 2 m (the slide collider is 1 m, a swarmer 0.75 m: they fit under 1.2 m; a husk, 1.8 m, does not). The tunnel's side
        /// walls are ordinary boxes in the plan; LevelValidate24 proves the geometry with the real Mover.</summary>
        static List<object> WithTunnels(Dictionary<string, object> plan)
        {
            var list = new List<object>(L(Get(plan, "boxes")));
            foreach (object o in L(Get(plan, "tunnels")))
            {
                var t = O(o);
                R2 r = Rect(t["rect"]);
                double c = N(t["ceiling"]);
                list.Add(new Dictionary<string, object> { ["zone"] = S(t, "zone"), ["kind"] = "hull", ["min"] = Arr(r.x0, c, r.z0), ["max"] = Arr(r.x1, c + 2, r.z1), ["note"] = "tunnel ceiling" });
            }
            list.AddRange(LevelPlan29.ExtraBoxes(plan, WallThick)); // NIGHT29: roofs, door lintels, collider props (only for a plan with "n29": true)
            return list;
        }

        /// <summary>The whole conversion: the level plan -> DrydockPlan.Build -> the map JSON with a "campaign" block (the rooms, doors and layer rows as they are in the plan).</summary>
        public static LevelBuild Build(Dictionary<string, object> plan, GameConfig cfg)
        {
            var lb = new LevelBuild();
            lb.drydockPlan = ToDrydock(plan, lb.notes);
            lb.build = DrydockPlan.Build(lb.drydockPlan, cfg);
            lb.problems.AddRange(DrydockPlan.Check(lb.build, cfg));
            lb.notes.AddRange(lb.build.notes);
            lb.json = lb.build.json;
            lb.json["generatedBy"] = "levelgen (Tools/SimHarness/Harness.N24.Level.cs: LevelPlan24 through DrydockPlan)";
            var campaign = new Dictionary<string, object>
            {
                ["rooms"] = plan["rooms"], ["doors"] = plan["doors"],
            };
            foreach (string key in new[] { "layer", "rank", "weapons", "links", "pits", "tunnels", "surfaces", "n27", "wallHeight", "teach35" }) if (plan.ContainsKey(key)) campaign[key] = plan[key]; // NIGHT35 A: teach35 = builder T's trigger rects (Core/Lesson25.cs Teach35Table.Rects)
            if (LevelPlan29.Wanted(plan)) foreach (string key in LevelPlan29.CampaignKeys) if (plan.ContainsKey(key)) campaign[key] = plan[key]; // NIGHT29: n29, lights, props, boxes (only for an "n29": true plan)
            lb.json["campaign"] = campaign;
            LevelPlan29.AddLava(plan, lb.json); // NIGHT30 (builder L): the top-level `lava` array (only for a plan with a `lava: true` pit)
            lb.text = MapJsonWriter.WriteFile(lb.json);
            return lb;
        }
    }
}
