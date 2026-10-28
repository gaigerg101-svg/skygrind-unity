// NIGHT15 U3-28.6 (builder G): DRYDOCK's greybox (MAP-DESIGN.md sections 3.1 and 4), built from ONE plan file,
// Design/maps/drydock.plan.json, two ways that share every line of the plan -> map conversion (DrydockPlan, below, pure:
// no UnityEngine, so Tools/SimHarness compiles this same file):
// 1. without the Editor: `dotnet run --project Tools/SimHarness -c Release -- mapgen Design/maps/drydock.plan.json
//    Design/maps/drydock.json` (Tools/SimHarness/Harness.MapGen.cs);
// 2. in the Editor: SKYGRIND/Map/Build DRYDOCK (BuildDrydock.Build(), which returns its log line for an eval): the same
//    DrydockPlan.Build, then the markers (MapImport of that JSON) in Assets/Scenes/Maps/drydock-authoring.unity, then
//    builder P's export of the markers (MapExport.ToModel) through the same JSON step, compared byte for byte with the
//    plan's text; the plan's text is what both ways write (Design/maps/drydock.json and its Player copy), then
//    MapValidate (Reports/MAP-drydock-validate.md).
// The conversion (the lead's brief, NIGHT15 U3-28.6): only the plan's `built` zones and what belongs to them; the first
// floor is THE floor (map.floor), further floors are floor slabs (data.boxes, kind "floor"); walls are kind "wall", and a
// barrier (y = the wall's top .. the zones' top) stands on each perimeter wall (the plan's tallest walls); the plan's box
// kinds become the web's kinds (DrydockPlan.KindFor: what RIVAL's graph walks, config nav.walkWalkers, and what gets a
// neon rim, config world.rimKinds); a ramp from its low end to its high end is a wedge (x, z = the low end, yaw =
// atan2(-dx, -dz), length = |to - from|: ArenaMap.WedgeCorners); a doorway's buy spot belongs to the zone it opens; a
// relay's verb comes from the plan's sentence (UnityTuning.MapVerbs); the wall guns go in spots.wallBuys (SurvivalSim.
// AddWallBuys), which builder P's MapModel has no field for: DrydockPlan.Export adds them after MapModelJson.ToJson; the
// core is an empty grid (cells 0: SurvivalMap.FromExport needs a core spec; nothing reshapes it).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;
using Skygrind.Game.MapAuthoring;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace Skygrind.EditorTools
{
    /// <summary>DRYDOCK's plan -> MapModel -> the map JSON (pure; the Editor's build and SimHarness's mapgen share it).</summary>
    public static class DrydockPlan
    {
        public const string Id = "drydock";
        public const string PlanFile = "Design/maps/drydock.plan.json";
        public const string DesignFile = "Design/maps/drydock.json";
        public const string GeneratedBy = "SKYGRIND/Map/Build DRYDOCK (Assets/Editor/Maps/Build_drydock.cs: DrydockPlan) through MapModelJson";
        public const string Source = "Design/maps/drydock.plan.json";

        /// <summary>The top of every zone box and of the barriers (the plan's notes: "zone rects ... at y 0..40"; the web's survival barriers stop at 40 too).</summary>
        const double ZoneTop = 40;

        /// <summary>No core grid: SurvivalMap.FromExport needs a core spec; 0 cells is a grid with nothing in it (CoreGrid count 0, Zones' inCore false), so x0, z0 and cellSize are unused.</summary>
        static readonly AuthoredCore NoCore = new AuthoredCore { x0 = 0, z0 = 0, cells = 0, cellSize = 4 };

        public const string Why =
            "DRYDOCK (NIGHT15 U3-28.6; MAP-DESIGN.md sections 3.1 and 4): \"Finish the ship, or fight on its bones.\" A sky-shipyard cut into a cliff at dusk, "
            + "the last airship half-built in its cradle. The hub, THE CRADLE, is THE RING: a 172 m training loop around the unfinished hull and its 64 m mast "
            + "(the landmark, seen from every zone), cut in two by the keel corridor, with aimed pads up to the hull deck; each side zone is built for its verbs "
            + "(THE RIBS: a canyon of 8-12 m slabs 7 m apart to wall-ride and wall-jump; THE SLIPWAY: two 40 m launch ramps down from a 10 m deck and a pad back up), "
            + "and THE GANGWAY is the start, the mast in view through its door. Tonight's greybox builds these four zones; the boilerworks, the drafting office, "
            + "the gantry and the pier are night 2. New design, so Unity-side (CLAUDE.md rule 5: the web never had this map); every number comes from "
            + "Design/maps/drydock.plan.json, sized from the movement numbers in MAP-DESIGN.md section 1, and Assets/Editor/Maps/Build_drydock.cs turns it into "
            + "this file. The map's identity is GG's decision (D-59): this is the recommended concept, built to be played and judged.";

        /// <summary>The plan's box kinds -> the web's (the map JSON's kinds are read by RIVAL's graph, config nav.walkWalkers, and the neon rims, config world.rimKinds).</summary>
        static readonly string[][] Kinds =
        {
            new[] { "deck", "ledge" }, // a walkable top (the hull deck, the slipway deck, LAUNCH RAIL's platform, the crow's nest): walked by the graph, rimmed
            new[] { "hull", "wall" }, // the hull's sides (under the deck): rimmed, not walked
            new[] { "rib", "wall" }, // the 24 m rib posts and crossbars: rimmed, not walked
            new[] { "landmark", "wall" }, // THE MAST: rimmed, not walked (its top is above ai.grid.scanTop anyway)
            new[] { "slab", "slab" }, // the rib canyon's ride walls: the web's SLAB GARDENS kind (rimmed, not walked: KEEL TOP is a wall-ride)
        };

        /// <summary>The plan's verb sentence -> a verb id (UnityTuning.MapVerbs): the word that comes first in the sentence, its "(night 2: ...)" asides left out.</summary>
        static readonly string[][] VerbWords =
        {
            new[] { "wall-ride", "wallRide" },
            new[] { "slide-jump", "slideJump" },
            new[] { "wall-jump", "wallJump" },
            new[] { "swing", "arcline" },
            new[] { "arcline", "arcline" },
            new[] { "pad", "pad" },
        };

        /// <summary>What a build gives: the model, the wall guns beside it, the JSON and its text, and notes (conversion choices, P's model warnings).</summary>
        public sealed class DrydockBuild
        {
            public MapModel model;
            public List<object> wallBuys;
            public Dictionary<string, object> json;
            public string text;
            public readonly List<string> notes = new List<string>();
        }

        static Dictionary<string, object> O(object o) => o as Dictionary<string, object>;
        static List<object> L(object o) => o as List<object> ?? new List<object>();
        static double N(object o) => PortData.Num(o);
        static string S(Dictionary<string, object> d, string key) => d != null && d.TryGetValue(key, out object v) ? v as string ?? "" : "";
        static object Get(Dictionary<string, object> d, string key) => d != null && d.TryGetValue(key, out object v) ? v : null;
        static MapWebVec V(object o)
        {
            List<object> a = L(o);
            return new MapWebVec(N(a[0]), N(a[1]), N(a[2]));
        }

        /// <summary>The plan file under the repo root.</summary>
        public static Dictionary<string, object> LoadPlan(string repoRoot) => PortData.Obj(PortData.LoadJson(System.IO.Path.Combine(repoRoot, PlanFile)));

        /// <summary>The web kind for a plan kind (unknown kinds kept, with a note).</summary>
        public static string KindFor(string planKind, List<string> notes)
        {
            foreach (string[] k in Kinds) if (k[0] == planKind) return k[1];
            notes?.Add($"box kind '{planKind}' has no web kind in DrydockPlan.Kinds: kept as it is");
            return planKind;
        }

        /// <summary>
        /// A relay's verb id from the plan's sentence: the verb word that comes first, outside parentheses (THE STERN's
        /// "the south pad (night 2: moves to the gantry's HOOK, the swing)" is a pad); "" and a note when none matches.
        /// </summary>
        public static string VerbFor(string sentence, string what, List<string> notes)
        {
            var s = new System.Text.StringBuilder();
            int depth = 0;
            foreach (char ch in (sentence ?? "").ToLowerInvariant())
            {
                if (ch == '(') depth++;
                else if (ch == ')') depth = Math.Max(0, depth - 1);
                else if (depth == 0) s.Append(ch);
            }
            string text = s.ToString();
            string best = "";
            int at = int.MaxValue;
            foreach (string[] v in VerbWords)
            {
                int i = text.IndexOf(v[0], StringComparison.Ordinal);
                if (i >= 0 && i < at)
                {
                    at = i;
                    best = v[1];
                }
            }
            if (best == "") notes?.Add($"{what}: no verb id in \"{sentence}\"");
            return best;
        }

        static AuthoredBox Box(Dictionary<string, object> b, string kind) => new AuthoredBox { min = V(b["min"]), max = V(b["max"]), kind = kind };

        /// <summary>
        /// A ramp {from: the low end's centre [x, z], to: the high end's [x, z], width, rise} as a wedge: (x, z) the low end,
        /// running `length` toward (-sin yaw, -cos yaw) (ArenaMap.WedgeCorners) to the high end. A -0 is written as 0.
        /// </summary>
        public static AuthoredWedge Wedge(Dictionary<string, object> r)
        {
            List<object> from = L(r["from"]), to = L(r["to"]);
            double x0 = N(from[0]), z0 = N(from[1]);
            double dx = N(to[0]) - x0, dz = N(to[1]) - z0;
            double ax = -dx, az = -dz;
            if (ax == 0) ax = 0; // -0 -> 0: atan2(-0, -4) is -pi, atan2(0, -4) pi (the same ramp; one text)
            if (az == 0) az = 0;
            return new AuthoredWedge { x = x0, z = z0, yaw = Math.Atan2(ax, az), length = Math.Sqrt(dx * dx + dz * dz), width = N(r["width"]), rise = N(r["rise"]) };
        }

        /// <summary>The plan -> builder P's model (web coordinates; the built zones only).</summary>
        public static MapModel ToModel(Dictionary<string, object> plan, List<string> notes)
        {
            var built = new HashSet<string>();
            foreach (object z in L(plan["built"])) built.Add((string)z);
            bool In(Dictionary<string, object> d) => !d.ContainsKey("zone") || built.Contains(S(d, "zone"));
            var m = new MapModel
            {
                id = (string)plan["id"], why = Why, floorSize = N(plan["floorSize"]), killPlaneY = N(plan["killPlaneY"]),
                hasCore = true, core = NoCore, startZone = (string)plan["startZone"],
            };

            // Floors: the first is THE floor (map.floor), the rest floor slabs (data.boxes, kind "floor").
            List<object> floors = L(plan["floor"]);
            for (int i = 0; i < floors.Count; i++)
            {
                AuthoredBox f = Box(O(floors[i]), "floor");
                if (i == 0)
                {
                    m.hasFloor = true;
                    m.floor = f;
                }
                else m.boxes.Add(f);
            }
            // Walls (kind "wall"); the tallest close the built footprint and carry a barrier up to the zones' top.
            List<object> walls = L(plan["walls"]);
            double perimeterTop = double.NegativeInfinity;
            foreach (object w in walls) perimeterTop = Math.Max(perimeterTop, N(L(O(w)["max"])[1]));
            foreach (object w in walls)
            {
                AuthoredBox b = Box(O(w), "wall");
                m.boxes.Add(b);
                if (b.max.y == perimeterTop)
                    m.barriers.Add(new AuthoredBox { min = new MapWebVec(b.min.x, b.max.y, b.min.z), max = new MapWebVec(b.max.x, ZoneTop, b.max.z), kind = "barrier" });
            }
            foreach (object o in L(plan["boxes"]))
            {
                var d = O(o);
                if (In(d)) m.boxes.Add(Box(d, KindFor(S(d, "kind"), notes)));
            }
            foreach (object o in L(plan["ramps"]))
                if (In(O(o))) m.wedges.Add(Wedge(O(o)));
            foreach (object o in L(plan["pads"]))
            {
                var d = O(o);
                if (!In(d)) continue;
                var pad = new AuthoredPad { at = V(d["at"]) };
                if (Get(d, "target") != null)
                {
                    pad.aimed = true;
                    pad.target = V(d["target"]);
                    pad.apexAbove = N(d["apexAbove"]);
                }
                m.pads.Add(pad);
            }
            foreach (object o in L(plan["grapple"]))
                if (In(O(o))) m.grappleNodes.Add(V(O(o)["at"]));

            // Zones: the built ones in plan order; a doorway's buy spot is the opened zone's (the one that isn't buyFrom).
            foreach (object o in L(plan["zones"]))
            {
                var d = O(o);
                string id = (string)d["id"];
                if (!built.Contains(id)) continue;
                List<object> rect = L(d["rect"]);
                var zone = new AuthoredZone { id = id, name = (string)d["name"], priority = N(d["priority"]), hasCost = true, cost = N(d["cost"]) };
                zone.boxes.Add(new AuthoredBox { min = new MapWebVec(N(rect[0]), 0, N(rect[1])), max = new MapWebVec(N(rect[2]), ZoneTop, N(rect[3])), kind = "", kindUndefined = true });
                foreach (object w in L(plan["doorways"]))
                {
                    var door = O(w);
                    List<object> between = L(door["between"]);
                    string a = (string)between[0], b = (string)between[1];
                    if (!built.Contains(a) || !built.Contains(b)) continue;
                    string opens = S(door, "buyFrom") == a ? b : a;
                    if (opens != id) continue;
                    List<object> at = L(door["buyAt"]), face = L(door["face"]);
                    zone.spots.Add(new AuthoredZoneSpot { at = new MapWebVec(N(at[0]), 0, N(at[1])), faceX = N(face[0]), faceZ = N(face[1]) });
                }
                m.zones.Add(zone);
            }

            // Spots.
            var spots = O(plan["spots"]);
            var player = O(spots["player"]);
            m.playerSpawn = new AuthoredPlayerSpawn { at = V(player["at"]), yawDeg = N(player["yawDeg"]), raw = "" };
            var spawns = O(spots["spawns"]);
            foreach (object p in L(spawns["ground"])) m.groundSpawns.Add(V(p));
            foreach (object p in L(spawns["high"])) m.highSpawns.Add(V(p));
            foreach (object o in L(spots["perks"]))
            {
                var d = O(o);
                if (In(d)) m.perkSpots.Add(new AuthoredPerk { perk = (string)d["perk"], at = V(d["at"]), yaw = N(d["yaw"]), core = false, verb = "" });
            }
            if (O(Get(spots, "forge")) is Dictionary<string, object> forge && In(forge))
            {
                m.hasForge = true;
                m.forgeSpot = new AuthoredPerk { perk = "", at = V(forge["at"]), yaw = N(forge["yaw"]), core = false, verb = "" };
            }
            if (O(Get(spots, "gadget")) is Dictionary<string, object> gadget && In(gadget))
                m.gadgetSpots.Add(new AuthoredGadget { gadget = (string)gadget["gadget"], at = V(gadget["at"]), yaw = N(gadget["yaw"]) });
            foreach (object o in L(spots["hold"]))
            {
                var d = O(o);
                if (!In(d)) continue;
                m.holdSpots.Add(V(d["at"]));
                m.holdVerbs.Add("");
            }
            foreach (object o in L(spots["relays"]))
            {
                var d = O(o);
                if (In(d)) m.relaySpots.Add(new AuthoredRelay { id = (string)d["id"], name = (string)d["name"], at = V(d["at"]), verb = VerbFor(S(d, "verb"), $"relay {d["id"]}", notes) });
            }
            if (O(Get(spots, "secretRelay")) is Dictionary<string, object> secret && In(secret))
            {
                m.hasSecretRelay = true;
                m.secretRelay = new AuthoredRelay { id = (string)secret["id"], name = (string)secret["name"], at = V(secret["at"]), verb = VerbFor(S(secret, "verb"), $"secret relay {secret["id"]}", notes) };
            }
            foreach (object o in L(spots["box"]))
            {
                var d = O(o);
                if (In(d)) m.boxSpots.Add(new AuthoredUnitySpot { at = V(d["at"]), yaw = N(d["yaw"]), zone = S(d, "zone"), verb = "" });
            }
            if (O(Get(spots, "power")) is Dictionary<string, object> power && In(power))
            {
                m.hasPower = true;
                m.power = new AuthoredUnitySpot { at = V(power["at"]), yaw = N(power["yaw"]), zone = S(power, "zone"), verb = "" };
            }
            if (O(Get(spots, "exfil")) is Dictionary<string, object> exfil)
            {
                if (O(Get(exfil, "radio")) is Dictionary<string, object> radio && In(radio))
                {
                    m.hasExfilRadio = true;
                    m.exfilRadio = new AuthoredUnitySpot { at = V(radio["at"]), yaw = N(radio["yaw"]), zone = S(radio, "zone"), verb = "" };
                }
                foreach (object o in L(Get(exfil, "sites")))
                {
                    var d = O(o);
                    if (In(d)) m.exfilSites.Add(new AuthoredUnitySpot { at = V(d["at"]), yaw = 0, zone = S(d, "zone"), verb = "" }); // a site has no yaw (design.exfil.sites)
                }
            }
            return m;
        }

        /// <summary>
        /// spots.wallBuys (SurvivalSim.AddWallBuys: weapon, x, y = the plate's centre, z, fx / fz = the way it faces, floor =
        /// the buy point's height): the built zones' wall guns; floor is the highest plan box top under the buy point
        /// (wallBuy.buyOut m in front of the plate) that is below the plate.
        /// </summary>
        public static List<object> WallBuys(Dictionary<string, object> plan, GameConfig cfg)
        {
            var built = new HashSet<string>();
            foreach (object z in L(plan["built"])) built.Add((string)z);
            double buyOut = cfg.Num("wallBuy.buyOut");
            var list = new List<object>();
            foreach (object o in L(O(plan["spots"])["wallBuys"]))
            {
                var d = O(o);
                if (!built.Contains(S(d, "zone"))) continue;
                MapWebVec at = V(d["at"]);
                List<object> f = L(d["facing"]);
                double fx = N(f[0]), fz = N(f[1]);
                double floor = TopUnder(plan, built, at.x + fx * buyOut, at.z + fz * buyOut, at.y);
                list.Add(new Dictionary<string, object>
                {
                    ["weapon"] = (string)d["weapon"], ["x"] = MapJsonWriter.Num(at.x), ["y"] = MapJsonWriter.Num(at.y), ["z"] = MapJsonWriter.Num(at.z),
                    ["fx"] = MapJsonWriter.Num(fx), ["fz"] = MapJsonWriter.Num(fz), ["floor"] = MapJsonWriter.Num(floor),
                });
            }
            return list;
        }

        /// <summary>The highest top of a plan box (floors, walls, the built zones' boxes) over (x, z) at or below `below` (0 when none).</summary>
        static double TopUnder(Dictionary<string, object> plan, HashSet<string> built, double x, double z, double below)
        {
            double best = double.NegativeInfinity;
            void Try(Dictionary<string, object> b)
            {
                if (b.ContainsKey("zone") && !built.Contains(S(b, "zone"))) return;
                MapWebVec lo = V(b["min"]), hi = V(b["max"]);
                if (x >= lo.x && x <= hi.x && z >= lo.z && z <= hi.z && hi.y <= below && hi.y > best) best = hi.y;
            }
            foreach (string key in new[] { "floor", "walls", "boxes" }) foreach (object o in L(plan[key])) Try(O(o));
            return double.IsNegativeInfinity(best) ? 0 : best;
        }

        /// <summary>
        /// The model (and the wall guns beside it) -> the map JSON and its text: builder P's MapModelJson.ToJson, this
        /// builder's generatedBy and source, spots.wallBuys (last in "spots"), then the nav grid and zone parts built by
        /// NavGridBuilder.ExportFor (MapModelJson.FillNav, MapNavMode.Build). The Editor's markers export goes through here too.
        /// </summary>
        public static (Dictionary<string, object> json, string text) Export(MapModel m, List<object> wallBuys, GameConfig cfg)
        {
            Dictionary<string, object> json = MapModelJson.ToJson(m, cfg, Source);
            json["generatedBy"] = GeneratedBy; // replaced in place: the key keeps its position
            if (wallBuys != null && wallBuys.Count > 0)
            {
                if (!(Get(json, "spots") is Dictionary<string, object> spots))
                {
                    spots = new Dictionary<string, object>();
                    json["spots"] = spots;
                }
                spots["wallBuys"] = wallBuys;
            }
            MapModelJson.FillNav(json, cfg, MapNavMode.Build, null);
            return (json, MapJsonWriter.WriteFile(json));
        }

        /// <summary>The whole build from the plan (both ways call this).</summary>
        public static DrydockBuild Build(Dictionary<string, object> plan, GameConfig cfg)
        {
            var b = new DrydockBuild();
            b.model = ToModel(plan, b.notes);
            b.wallBuys = WallBuys(plan, cfg);
            (b.json, b.text) = Export(b.model, b.wallBuys, cfg);
            b.notes.AddRange(b.model.warnings);
            return b;
        }

        /// <summary>
        /// What the Editor's way does after the markers, without the markers: the text read back through builder P's
        /// MapModelJson.FromJson and exported again (the model the markers are made from). Equal: the markers, exported
        /// unchanged (MapExport.ToModel keeps each unmoved marker's exact values), give back the same text. Returns the first
        /// differing char (-1: the same) and FromJson's warnings.
        /// </summary>
        public static int ModelRoundTrip(DrydockBuild b, GameConfig cfg, out List<string> warnings)
        {
            MapModel back = MapModelJson.FromJson(PortData.Obj(JsJson.Parse(b.text)));
            warnings = back.warnings;
            return FirstDifference(Export(back, b.wallBuys, cfg).text, b.text);
        }

        public static int FirstDifference(string a, string b)
        {
            int n = Math.Min(a.Length, b.Length);
            for (int i = 0; i < n; i++) if (a[i] != b[i]) return i;
            return a.Length == b.Length ? -1 : n;
        }

        // --- the plan checks: places that stand in a solid or float, wall guns off their wall ----------------------------

        struct DrydockSolid
        {
            public MapWebVec min, max;
            public string what;
        }

        /// <summary>
        /// Geometry checks on the built map (box arithmetic over the exported collision boxes and wedges, no collider): every
        /// spot, spawn, pad, pad target, gate buy spot and wall gun buy point stands on a surface (the highest top under it
        /// within 0.05 m) with a player's capsule (config body) clear of every box; a wall gun's plate has a box right behind
        /// it. Each line is a problem; none: an empty list.
        /// </summary>
        public static List<string> Check(DrydockBuild b, GameConfig cfg)
        {
            var problems = new List<string>();
            var solids = new List<DrydockSolid>();
            List<object> colBoxes = L(O(b.json["collision"])["boxes"]);
            for (int i = 0; i < colBoxes.Count; i++)
            {
                var d = O(colBoxes[i]);
                solids.Add(new DrydockSolid { min = V(d["min"]), max = V(d["max"]), what = $"collision box {i} ({S(d, "kind")})" });
            }
            var wedges = new List<WedgeSpec>();
            foreach (object w in L(O(b.json["collision"])["wedges"])) wedges.Add(PortData.Wedge(w));
            double r = cfg.movement.body.capsuleRadius, h = cfg.movement.body.capsuleHeight;

            double Top(double x, double z, double y)
            {
                double best = double.NegativeInfinity;
                foreach (DrydockSolid s in solids)
                    if (x >= s.min.x && x <= s.max.x && z >= s.min.z && z <= s.max.z && s.max.y <= y + 0.05 && s.max.y > best) best = s.max.y;
                foreach (WedgeSpec w in wedges)
                {
                    double ax = -Math.Sin(w.yaw), az = -Math.Cos(w.yaw), rx = Math.Cos(w.yaw), rz = -Math.Sin(w.yaw);
                    double along = (x - w.x) * ax + (z - w.z) * az, across = (x - w.x) * rx + (z - w.z) * rz;
                    if (along < 0 || along > w.length || Math.Abs(across) > w.width / 2) continue;
                    double top = w.rise * along / w.length;
                    if (top <= y + 0.05 && top > best) best = top;
                }
                return best;
            }
            string Inside(double x, double y, double z)
            {
                foreach (DrydockSolid s in solids)
                    if (x + r > s.min.x && x - r < s.max.x && z + r > s.min.z && z - r < s.max.z && y + 0.05 < s.max.y && y + h > s.min.y) return s.what;
                return null;
            }
            string F(double v) => v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            void Stand(string what, double x, double y, double z)
            {
                string where = $"{what} ({F(x)}, {F(y)}, {F(z)})";
                string inside = Inside(x, y, z);
                if (inside != null) problems.Add($"{where}: a player's capsule there overlaps {inside}");
                double top = Top(x, z, y);
                if (double.IsNegativeInfinity(top)) problems.Add($"{where}: no surface under it");
                else if (Math.Abs(top - y) > 0.05) problems.Add($"{where}: the surface under it is at y {F(top)}, {F(y - top)} m lower");
            }
            MapModel m = b.model;
            Stand("player spawn", m.playerSpawn.at.x, m.playerSpawn.at.y, m.playerSpawn.at.z);
            for (int i = 0; i < m.groundSpawns.Count; i++) Stand($"ground spawn {i}", m.groundSpawns[i].x, m.groundSpawns[i].y, m.groundSpawns[i].z);
            for (int i = 0; i < m.highSpawns.Count; i++) Stand($"high spawn {i}", m.highSpawns[i].x, m.highSpawns[i].y, m.highSpawns[i].z);
            for (int i = 0; i < m.pads.Count; i++)
            {
                Stand($"pad {i}", m.pads[i].at.x, m.pads[i].at.y, m.pads[i].at.z);
                if (m.pads[i].aimed) Stand($"pad {i} target", m.pads[i].target.x, m.pads[i].target.y, m.pads[i].target.z);
            }
            foreach (AuthoredZone z in m.zones)
                for (int i = 0; i < z.spots.Count; i++) Stand($"gate {z.id} {i}", z.spots[i].at.x, z.spots[i].at.y, z.spots[i].at.z);
            foreach (AuthoredPerk p in m.perkSpots) Stand($"perk {p.perk}", p.at.x, p.at.y, p.at.z);
            if (m.hasForge) Stand("forge", m.forgeSpot.at.x, m.forgeSpot.at.y, m.forgeSpot.at.z);
            foreach (AuthoredGadget g in m.gadgetSpots) Stand($"gadget {g.gadget}", g.at.x, g.at.y, g.at.z);
            for (int i = 0; i < m.holdSpots.Count; i++) Stand($"hold ring {i}", m.holdSpots[i].x, m.holdSpots[i].y, m.holdSpots[i].z);
            foreach (AuthoredRelay rl in m.relaySpots) Stand($"relay {rl.id}", rl.at.x, rl.at.y, rl.at.z);
            if (m.hasSecretRelay) Stand($"secret relay {m.secretRelay.id}", m.secretRelay.at.x, m.secretRelay.at.y, m.secretRelay.at.z);
            for (int i = 0; i < m.boxSpots.Count; i++) Stand($"mystery box {i}", m.boxSpots[i].at.x, m.boxSpots[i].at.y, m.boxSpots[i].at.z);
            if (m.hasPower) Stand("power switch", m.power.at.x, m.power.at.y, m.power.at.z);
            if (m.hasExfilRadio) Stand("exfil radio", m.exfilRadio.at.x, m.exfilRadio.at.y, m.exfilRadio.at.z);
            for (int i = 0; i < m.exfilSites.Count; i++) Stand($"exfil site {i}", m.exfilSites[i].at.x, m.exfilSites[i].at.y, m.exfilSites[i].at.z);
            double buyOut = cfg.Num("wallBuy.buyOut");
            foreach (object o in b.wallBuys)
            {
                var d = O(o);
                string gun = $"wall gun {S(d, "weapon")}";
                double x = N(d["x"]), y = N(d["y"]), z = N(d["z"]), fx = N(d["fx"]), fz = N(d["fz"]), floor = N(d["floor"]);
                Stand($"{gun}'s buy point", x + fx * buyOut, floor, z + fz * buyOut);
                double bx = x - fx * 0.2, bz = z - fz * 0.2; // 0.2 m behind the plate: inside the wall it hangs on
                bool backed = false;
                foreach (DrydockSolid s in solids) backed |= bx > s.min.x && bx < s.max.x && bz > s.min.z && bz < s.max.z && y > s.min.y && y < s.max.y;
                if (!backed) problems.Add($"{gun} ({F(x)}, {F(y)}, {F(z)}) facing ({F(fx)}, {F(fz)}): no box 0.2 m behind its plate (it hangs in the air)");
            }
            return problems;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// SKYGRIND/Map/Build DRYDOCK: the plan -> Design/maps/drydock.json (+ its Player copy), the authoring scene's markers
    /// and the validation report. Re-runnable: the same plan gives the same text (and a scene of the same markers).
    /// </summary>
    public static class BuildDrydock
    {
        public const string SceneFolder = "Assets/Scenes/Maps";
        public const string ScenePath = SceneFolder + "/drydock-authoring.unity";

        static string Repo => Path.GetDirectoryName(UnityEngine.Application.dataPath);

        static GameConfig Config() => GameConfig.Load(Path.Combine(GameConfig.PortExportDir(Repo, UnityEngine.Application.streamingAssetsPath, true), "config.json"));

        [MenuItem("SKYGRIND/Map/Build DRYDOCK")]
        public static void Menu() => Build();

        /// <summary>
        /// The build (the menu's; the lead's eval: Skygrind.EditorTools.BuildDrydock.Build()). Returns its "[map] ..." line.
        /// The open scenes stay as they are: the authoring scene is built additively, saved, and closed again unless it was
        /// already open.
        /// </summary>
        public static string Build()
        {
            string refused = EditorApplication.isPlayingOrWillChangePlaymode ? "refused in Play mode (stop it first)" : null;
            for (int i = 0; refused == null && i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.isLoaded && string.IsNullOrEmpty(s.path)) refused = "an untitled scene is open (the authoring scene is added beside the open ones: save it or open a saved scene first)";
            }
            if (refused != null)
            {
                string no = "[map] build DRYDOCK: " + refused;
                UnityEngine.Debug.LogError(no);
                return no;
            }
            var t = System.Diagnostics.Stopwatch.StartNew();
            GameConfig cfg = Config();
            DrydockPlan.DrydockBuild b = DrydockPlan.Build(DrydockPlan.LoadPlan(Repo), cfg);
            List<string> problems = DrydockPlan.Check(b, cfg);

            // The file on disk before (SimHarness's mapgen writes the same text). Its line end normalized: the JSON is one
            // line and a "\n", and git (core.autocrlf true, no .gitattributes rule for .json) checks it out with "\r\n".
            string designPath = Path.Combine(Repo, UnityTuning.MapDesignDir, DrydockPlan.Id + ".json");
            string before = File.Exists(designPath) ? File.ReadAllText(designPath).Replace("\r\n", "\n") : null;

            // The scene: the authoring scene (open or not), emptied, the markers made from the JSON.
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool wasOpen = scene.IsValid() && scene.isLoaded;
            if (wasOpen)
                foreach (UnityEngine.GameObject go in scene.GetRootGameObjects()) UnityEngine.Object.DestroyImmediate(go);
            else scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            MapRoot root = MapImport.Build(b.json, DrydockPlan.DesignFile, out List<string> importWarnings);
            int markers = root.GetComponentsInChildren<MapMarker>().Length;

            // Builder P's export of the markers (layer 1, MapExport.ToModel), through the same JSON step as the plan's.
            MapModel fromMarkers = MapExport.ToModel(root);
            string markersText = DrydockPlan.Export(fromMarkers, b.wallBuys, cfg).text;
            int diff = DrydockPlan.FirstDifference(markersText, b.text);

            if (!AssetDatabase.IsValidFolder(SceneFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "Maps");
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            if (!wasOpen) EditorSceneManager.CloseScene(scene, true);

            // The files: the plan's text (equal to the markers' when diff < 0), then the report.
            (string design, string player) = MapModelJson.WriteFiles(Repo, DrydockPlan.Id, b.text);
            AssetDatabase.Refresh();
            (string report, int findings) = MapValidate.Write(cfg, b.json, Repo, DrydockPlan.DesignFile);
            int verbs = 0;
            foreach (string row in File.ReadAllLines(report)) if (row.Contains("| VERB |")) verbs++;

            foreach (string n in b.notes) UnityEngine.Debug.LogWarning("[map] drydock: " + n);
            foreach (string w in importWarnings) UnityEngine.Debug.LogWarning("[map] drydock import: " + w);
            foreach (string p in problems) UnityEngine.Debug.LogWarning("[map] drydock check: " + p);
            string line = $"[map] build DRYDOCK: {DrydockPlan.DesignFile} and its Player copy ({b.text.Length} chars; the file before: {(before == null ? "none" : before == b.text ? "SAME" : $"DIFFERENT at char {DrydockPlan.FirstDifference(before, b.text)}")}); "
                + $"markers {markers} in {ScenePath} ({(saved ? "saved" : "NOT SAVED")}), their export {(diff < 0 ? "SAME as the plan's text" : $"DIFFERENT at char {diff}")}; "
                + $"plan checks: {problems.Count} problems; validate: {findings} findings, {verbs} VERB -> {UnityTuning.MapReportDir}/MAP-{DrydockPlan.Id}-validate.md; "
                + $"{b.notes.Count} notes, {importWarnings.Count} import warnings ({t.ElapsedMilliseconds} ms)";
            if (diff < 0 && findings == 0 && saved) UnityEngine.Debug.Log(line);
            else UnityEngine.Debug.LogError(line);
            return line;
        }
    }
#endif
}
