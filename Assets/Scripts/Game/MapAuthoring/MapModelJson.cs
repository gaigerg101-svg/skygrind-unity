// NIGHT15 U3-28.4 (builder P): MapModel <-> the map JSON, in the web exporter's schema and key order
// (port-export/scripts/maps.mjs exportMap; SurvivalMap.FromExport, PortData.BuildMapOctree, NavGrid.FromExport, NavMap and
// the bench read it). FromJson reads any map JSON (the web's PortExport/maps/*.json or a Design/maps/<id>.json); ToJson
// writes the map back with the level data under "data" and its copies at the top (collision, core, spawns, pads,
// grappleNodes), as maps.mjs does, plus a Unity-authored map's "_why" (first) and "spots" (last).
// What the export does not write (diagnostics; nothing in the game reads them): the web exporter's notes (collision.note,
// collision.wedgeNote, core.note, core.randomNote, padNote, dataNote), collision.triangles and collision.droppedByOctree
// (three.js Octree dumps), duelNavGraph (the Spire's Duel graph); generatedBy and source are this exporter's own.
// The floor (the lead, NIGHT15): the model's floor (the first kind "floor" MapBox) goes into collision.boxes right after
// the last kind "floor" box of data.boxes (at index 0 when there is none: the web's [floor, ...boxes]), so it is the last
// floor there and SurvivalMap.FromExport takes it as map.floor; extra floor slabs stay in data.boxes, which the game
// draws and collides. With no floor marker the export adds the web exporter's floor (floorSize, config core.floorThickness).
// Pure (no UnityEngine).
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game.MapAuthoring
{
    /// <summary>Where the export's navGrid and zoneParts come from.</summary>
    public enum MapNavMode
    {
        /// <summary>NavGridBuilder.ExportFor on the exported map (the C# port of the web's createNavGrid and resolveZones).</summary>
        Build,
        /// <summary>The imported map's navGrid / zoneParts when the geometry the grid depends on is unchanged, else Build.</summary>
        KeepIfGeometryUnchanged,
    }

    public static class MapModelJson
    {
        public const string GeneratedBy = "SKYGRIND/Map/Export (Assets/Scripts/Game/MapAuthoring/MapExport.cs)";

        /// <summary>maps.mjs EXTRA_PRESETS: the boss arenas' core presets, exported after CoreGrid.Presets (CORE_PRESETS).</summary>
        static readonly string[] ExtraCorePresets = { "platforms", "pen", "pillars" };

        /// <summary>The data keys FromJson reads into the model (any other one is kept as extraDataJson).</summary>
        static readonly string[] DataKeys =
        {
            "name", "floorSize", "killPlaneY", "boxes", "wedges", "strips", "barriers", "pads", "core", "spawns", "playerSpawn",
            "duelSpawns", "grappleNodes", "startZone", "armoryWall", "armoryRack", "armoryBack", "perkSpots", "trapSpots",
            "gadgetSpots", "forgeSpot", "holdSpots", "secretRelay", "relaySpots", "zones", "zoneHoles",
        };

        // --- reading ------------------------------------------------------------------------------------------------

        static Dictionary<string, object> O(object o) => o as Dictionary<string, object>;
        static List<object> L(object o) => o as List<object>;
        static double N(object o) => PortData.Num(o);
        static MapWebVec P(object o)
        {
            var d = O(o);
            return new MapWebVec(N(d["x"]), N(d["y"]), N(d["z"]));
        }

        static MapWebVec A(object o)
        {
            var a = L(o);
            return new MapWebVec(N(a[0]), N(a[1]), N(a[2]));
        }

        static object Get(Dictionary<string, object> d, string key) => d != null && d.TryGetValue(key, out object v) ? v : null;

        static void Expect(Dictionary<string, object> d, string path, List<string> warnings, params string[] keys)
        {
            foreach (string k in d.Keys)
                if (Array.IndexOf(keys, k) < 0) warnings.Add($"{path}: key '{k}' is not carried (the export drops it)");
        }

        static AuthoredBox ReadBox(object o, string path, List<string> warnings)
        {
            var d = O(o);
            Expect(d, path, warnings, "min", "max", "kind");
            var b = new AuthoredBox { min = A(d["min"]), max = A(d["max"]) };
            object k = Get(d, "kind");
            if (k is string s) b.kind = s;
            else
            {
                b.kind = "";
                b.kindUndefined = true;
                if (!MapJsonWriter.IsUndefined(k)) warnings.Add($"{path}: kind {(d.ContainsKey("kind") ? MapJsonWriter.Write(k) : "missing")} is written as undefined");
            }
            return b;
        }

        static List<AuthoredBox> ReadBoxes(object o, string path, List<string> warnings)
        {
            var list = new List<AuthoredBox>();
            if (L(o) is List<object> a) for (int i = 0; i < a.Count; i++) list.Add(ReadBox(a[i], $"{path}[{i}]", warnings));
            return list;
        }

        static AuthoredPlayerSpawn ReadSpawn(object o, string path, List<string> warnings)
        {
            var d = O(o);
            if (d == null || !d.ContainsKey("x"))
            {
                warnings.Add($"{path}: {MapJsonWriter.Write(o)} is not {{x, y, z, yawDeg}}: kept verbatim (its marker sits at the origin)");
                return new AuthoredPlayerSpawn { raw = MapJsonWriter.Write(o) };
            }
            Expect(d, path, warnings, "x", "y", "z", "yawDeg");
            return new AuthoredPlayerSpawn { at = P(d), yawDeg = d.TryGetValue("yawDeg", out object y) ? N(y) : 0, raw = "" };
        }

        static List<MapWebVec> ReadPoints(object o, string path, List<string> warnings)
        {
            var list = new List<MapWebVec>();
            if (L(o) is List<object> a)
                for (int i = 0; i < a.Count; i++)
                {
                    Expect(O(a[i]), $"{path}[{i}]", warnings, "x", "y", "z");
                    list.Add(P(a[i]));
                }
            return list;
        }

        static AuthoredPerk ReadPerk(object o, string path, bool forge, List<string> warnings)
        {
            var d = O(o);
            if (forge) Expect(d, path, warnings, "x", "y", "z", "core", "yaw", "verb");
            else Expect(d, path, warnings, "perk", "x", "y", "z", "core", "yaw", "verb");
            object core = Get(d, "core");
            if (core != null && !(core is bool cb && cb)) warnings.Add($"{path}: core {MapJsonWriter.Write(core)} is written as absent");
            return new AuthoredPerk
            {
                perk = forge ? "" : (string)d["perk"], at = P(d), yaw = d.TryGetValue("yaw", out object y) ? N(y) : 0,
                core = core is bool c && c, verb = Verb(d),
            };
        }

        /// <summary>A spot's designed verb gate (a Unity-authored map's "verb": MapValidate reports it VERB, not FINDING), or "".</summary>
        static string Verb(Dictionary<string, object> d) => Get(d, "verb") as string ?? "";

        static AuthoredRelay ReadRelay(object o, string path, List<string> warnings)
        {
            var d = O(o);
            Expect(d, path, warnings, "id", "name", "x", "y", "z", "verb");
            return new AuthoredRelay { id = (string)d["id"], name = (string)d["name"], at = P(d), verb = Verb(d) };
        }

        static AuthoredWallRow ReadWallRow(object o, bool zPlane, string path, List<string> warnings)
        {
            var d = O(o);
            if (d == null) return null;
            string plane = zPlane ? "z" : "x", slots = zPlane ? "xs" : "zs";
            Expect(d, path, warnings, plane, "y", slots, "facing");
            var row = new AuthoredWallRow { zPlane = zPlane, plane = N(d[plane]), y = N(d["y"]), facing = N(d["facing"]) };
            foreach (object s in L(d[slots])) row.slots.Add(N(s));
            return row;
        }

        static AuthoredUnitySpot ReadUnitySpot(object o, bool yaw, string path, List<string> warnings)
        {
            var d = O(o);
            if (yaw) Expect(d, path, warnings, "x", "y", "z", "yaw", "zone", "verb");
            else Expect(d, path, warnings, "x", "y", "z", "zone", "verb");
            return new AuthoredUnitySpot { at = P(d), yaw = yaw && d.TryGetValue("yaw", out object y) ? N(y) : 0, zone = d.TryGetValue("zone", out object z) ? z as string ?? "" : "", verb = Verb(d) };
        }

        /// <summary>Any map JSON (a web export or a Design/maps file) as a model; what it can't carry is in model.warnings.</summary>
        public static MapModel FromJson(Dictionary<string, object> map)
        {
            var m = new MapModel();
            List<string> w = m.warnings;
            var data = O(Get(map, "data")) ?? throw new ArgumentException("map JSON: no 'data' (the level data the markers come from)");
            m.id = Get(map, "name") as string ?? Get(data, "name") as string ?? "map";
            m.why = Get(map, "_why") as string ?? "";
            m.floorSize = N(Get(data, "floorSize") ?? map["floorSize"]);
            m.killPlaneY = N(Get(data, "killPlaneY") ?? map["killPlaneY"]);
            object core = data.ContainsKey("core") ? data["core"] : Get(O(Get(map, "core")), "spec");
            if (O(core) is Dictionary<string, object> c)
            {
                Expect(c, "data.core", w, "x0", "z0", "cells", "cellSize");
                m.hasCore = true;
                m.core = new AuthoredCore { x0 = N(c["x0"]), z0 = N(c["z0"]), cells = N(c["cells"]), cellSize = N(c["cellSize"]) };
            }
            m.boxes.AddRange(ReadBoxes(data["boxes"], "data.boxes", w));
            ReadFloor(map, data, m);
            m.strips.AddRange(ReadBoxes(Get(data, "strips"), "data.strips", w));
            var col = O(Get(map, "collision"));
            m.barriers.AddRange(ReadBoxes(Get(data, "barriers") ?? Get(col, "barriers"), "data.barriers", w));
            if (L(Get(data, "wedges") ?? Get(col, "wedges")) is List<object> wedges)
                for (int i = 0; i < wedges.Count; i++)
                {
                    var d = O(wedges[i]);
                    Expect(d, $"data.wedges[{i}]", w, "x", "z", "yaw", "length", "width", "rise");
                    m.wedges.Add(new AuthoredWedge { x = N(d["x"]), z = N(d["z"]), yaw = N(d["yaw"]), length = N(d["length"]), width = N(d["width"]), rise = N(d["rise"]) });
                }
            if (L(Get(data, "pads") ?? Get(map, "pads")) is List<object> pads)
                for (int i = 0; i < pads.Count; i++)
                {
                    var d = O(pads[i]);
                    Expect(d, $"data.pads[{i}]", w, "x", "y", "z", "target", "apexAbove");
                    var pad = new AuthoredPad { at = P(d) };
                    if (Get(d, "target") is Dictionary<string, object> t)
                    {
                        pad.aimed = true;
                        pad.target = P(t);
                        pad.apexAbove = N(d["apexAbove"]);
                    }
                    m.pads.Add(pad);
                }
            m.grappleNodes.AddRange(ReadPoints(Get(data, "grappleNodes") ?? Get(map, "grappleNodes"), "data.grappleNodes", w));
            var spawns = O(Get(data, "spawns")) ?? O(Get(map, "spawns"));
            m.groundSpawns.AddRange(ReadPoints(Get(spawns, "ground"), "data.spawns.ground", w));
            m.highSpawns.AddRange(ReadPoints(Get(spawns, "high"), "data.spawns.high", w));
            object ps = Get(data, "playerSpawn") ?? Get(O(Get(map, "spawns")), "playerSpawn");
            if (ps != null) m.playerSpawn = ReadSpawn(ps, "data.playerSpawn", w);
            else w.Add("no playerSpawn: the export writes (0, 0, 0)");
            if (L(Get(data, "duelSpawns") ?? Get(O(Get(map, "spawns")), "duelSpawns")) is List<object> duel)
                for (int i = 0; i < duel.Count; i++) m.duelSpawns.Add(ReadSpawn(duel[i], $"data.duelSpawns[{i}]", w));
            m.startZone = Get(data, "startZone") as string ?? Get(O(Get(map, "zoneParts")), "startZone") as string ?? "";
            if (L(Get(data, "zones")) is List<object> zones)
                for (int i = 0; i < zones.Count; i++)
                {
                    var d = O(zones[i]);
                    string path = $"data.zones[{i}]";
                    Expect(d, path, w, "id", "name", "priority", "cost", "boxes", "spots");
                    var z = new AuthoredZone { id = (string)d["id"], name = Get(d, "name") as string ?? "" };
                    if (d.TryGetValue("priority", out object pr)) z.priority = N(pr);
                    else w.Add($"{path}: no priority (the export writes 0)");
                    if (d.TryGetValue("cost", out object cost))
                    {
                        z.hasCost = true;
                        z.cost = N(cost);
                    }
                    z.boxes.AddRange(ReadBoxes(Get(d, "boxes"), path + ".boxes", w));
                    if (L(Get(d, "spots")) is List<object> zs)
                        for (int j = 0; j < zs.Count; j++)
                        {
                            var sd = O(zs[j]);
                            Expect(sd, $"{path}.spots[{j}]", w, "x", "y", "z", "face");
                            var spot = new AuthoredZoneSpot { at = P(sd) };
                            if (L(Get(sd, "face")) is List<object> f)
                            {
                                spot.faceX = N(f[0]);
                                spot.faceZ = N(f[1]);
                            }
                            z.spots.Add(spot);
                        }
                    m.zones.Add(z);
                }
            m.zoneHoles.AddRange(ReadBoxes(Get(data, "zoneHoles"), "data.zoneHoles", w));
            m.armoryWall = ReadWallRow(Get(data, "armoryWall"), false, "data.armoryWall", w);
            m.armoryRack = ReadWallRow(Get(data, "armoryRack"), true, "data.armoryRack", w);
            m.armoryBack = ReadWallRow(Get(data, "armoryBack"), true, "data.armoryBack", w);
            if (L(Get(data, "perkSpots")) is List<object> perks)
                for (int i = 0; i < perks.Count; i++) m.perkSpots.Add(ReadPerk(perks[i], $"data.perkSpots[{i}]", false, w));
            if (L(Get(data, "trapSpots")) is List<object> traps)
                for (int i = 0; i < traps.Count; i++)
                {
                    var d = O(traps[i]);
                    Expect(d, $"data.trapSpots[{i}]", w, "trap", "x", "y", "z", "sx", "sz");
                    m.trapSpots.Add(new AuthoredTrap { trap = (string)d["trap"], at = P(d), sx = N(d["sx"]), sz = N(d["sz"]) });
                }
            if (L(Get(data, "gadgetSpots")) is List<object> gadgets)
                for (int i = 0; i < gadgets.Count; i++)
                {
                    var d = O(gadgets[i]);
                    Expect(d, $"data.gadgetSpots[{i}]", w, "gadget", "x", "y", "z", "yaw");
                    m.gadgetSpots.Add(new AuthoredGadget { gadget = (string)d["gadget"], at = P(d), yaw = d.TryGetValue("yaw", out object y) ? N(y) : 0 });
                }
            if (Get(data, "forgeSpot") is Dictionary<string, object> forge)
            {
                m.hasForge = true;
                m.forgeSpot = ReadPerk(forge, "data.forgeSpot", true, w);
            }
            if (L(Get(data, "holdSpots")) is List<object> holds)
                for (int i = 0; i < holds.Count; i++)
                {
                    var d = O(holds[i]);
                    Expect(d, $"data.holdSpots[{i}]", w, "x", "y", "z", "verb");
                    m.holdSpots.Add(P(d));
                    m.holdVerbs.Add(Verb(d));
                }
            if (Get(data, "secretRelay") is Dictionary<string, object> secret)
            {
                m.hasSecretRelay = true;
                m.secretRelay = ReadRelay(secret, "data.secretRelay", w);
            }
            if (L(Get(data, "relaySpots")) is List<object> relays)
                for (int i = 0; i < relays.Count; i++) m.relaySpots.Add(ReadRelay(relays[i], $"data.relaySpots[{i}]", w));
            var extra = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> kv in data)
                if (Array.IndexOf(DataKeys, kv.Key) < 0) extra[kv.Key] = kv.Value;
            m.extraDataJson = extra.Count > 0 ? MapJsonWriter.Write(extra) : "";
            if (Get(map, "spots") is Dictionary<string, object> spots)
            {
                Expect(spots, "spots", w, "box", "power", "exfil");
                if (L(Get(spots, "box")) is List<object> box)
                    for (int i = 0; i < box.Count; i++) m.boxSpots.Add(ReadUnitySpot(box[i], true, $"spots.box[{i}]", w));
                if (Get(spots, "power") is Dictionary<string, object> power)
                {
                    m.hasPower = true;
                    m.power = ReadUnitySpot(power, true, "spots.power", w);
                }
                if (Get(spots, "exfil") is Dictionary<string, object> exfil)
                {
                    Expect(exfil, "spots.exfil", w, "radio", "sites");
                    if (Get(exfil, "radio") is Dictionary<string, object> radio)
                    {
                        m.hasExfilRadio = true;
                        m.exfilRadio = ReadUnitySpot(radio, true, "spots.exfil.radio", w);
                    }
                    if (L(Get(exfil, "sites")) is List<object> sites)
                        for (int i = 0; i < sites.Count; i++) m.exfilSites.Add(ReadUnitySpot(sites[i], false, $"spots.exfil.sites[{i}]", w));
                }
            }
            return m;
        }

        /// <summary>
        /// The floor: collision.boxes is data.boxes with the floor inserted (the web: at index 0). The inserted box, when
        /// its kind is "floor", is the model's floor; anything else is a warning (the export rebuilds collision from data).
        /// </summary>
        static void ReadFloor(Dictionary<string, object> map, Dictionary<string, object> data, MapModel m)
        {
            var colBoxes = L(Get(O(Get(map, "collision")), "boxes"));
            var dataBoxes = L(data["boxes"]);
            if (colBoxes == null)
            {
                m.warnings.Add("no collision.boxes: the export adds the web exporter's floor");
                return;
            }
            if (colBoxes.Count != dataBoxes.Count + 1)
            {
                m.warnings.Add($"collision.boxes has {colBoxes.Count} boxes, data.boxes {dataBoxes.Count}: the export rebuilds collision from data.boxes and the first collision box of kind \"floor\"");
                foreach (object o in colBoxes)
                    if (Get(O(o), "kind") as string == "floor")
                    {
                        m.hasFloor = true;
                        m.floor = ReadBox(o, "collision.boxes (floor)", m.warnings);
                        return;
                    }
                return;
            }
            int at = 0;
            while (at < dataBoxes.Count && MapJsonWriter.Write(colBoxes[at]) == MapJsonWriter.Write(dataBoxes[at])) at++;
            for (int i = at; i < dataBoxes.Count; i++)
                if (MapJsonWriter.Write(colBoxes[i + 1]) != MapJsonWriter.Write(dataBoxes[i]))
                {
                    m.warnings.Add($"collision.boxes[{i + 1}] differs from data.boxes[{i}]: the export rebuilds collision from data.boxes");
                    break;
                }
            if (Get(O(colBoxes[at]), "kind") as string != "floor")
            {
                m.warnings.Add($"collision.boxes[{at}] (not in data.boxes) is not kind \"floor\": dropped; the export adds the web exporter's floor");
                return;
            }
            m.hasFloor = true;
            m.floor = ReadBox(colBoxes[at], $"collision.boxes[{at}]", m.warnings);
        }

        // --- writing ------------------------------------------------------------------------------------------------

        static object Nm(double v) => MapJsonWriter.Num(v);
        static List<object> Arr3(MapWebVec v) => new List<object> { Nm(v.x), Nm(v.y), Nm(v.z) };
        static Dictionary<string, object> Xyz(MapWebVec v) => new Dictionary<string, object> { ["x"] = Nm(v.x), ["y"] = Nm(v.y), ["z"] = Nm(v.z) };

        static Dictionary<string, object> BoxJson(AuthoredBox b) => new Dictionary<string, object>
        {
            ["min"] = Arr3(b.min),
            ["max"] = Arr3(b.max),
            ["kind"] = b.kindUndefined ? MapJsonWriter.Undefined() : (object)(b.kind ?? ""),
        };

        static List<object> Boxes(List<AuthoredBox> list) => list.ConvertAll(b => (object)BoxJson(b));
        static List<object> Points(List<MapWebVec> list) => list.ConvertAll(p => (object)Xyz(p));

        static object SpawnJson(AuthoredPlayerSpawn s)
        {
            if (!string.IsNullOrEmpty(s.raw)) return JsJson.Parse(s.raw);
            Dictionary<string, object> d = Xyz(s.at);
            d["yawDeg"] = Nm(s.yawDeg);
            return d;
        }

        static List<object> Wedges(List<AuthoredWedge> list) => list.ConvertAll(w => (object)new Dictionary<string, object>
        {
            ["x"] = Nm(w.x), ["z"] = Nm(w.z), ["yaw"] = Nm(w.yaw), ["length"] = Nm(w.length), ["width"] = Nm(w.width), ["rise"] = Nm(w.rise),
        });

        static List<object> Pads(List<AuthoredPad> list) => list.ConvertAll(p =>
        {
            Dictionary<string, object> d = Xyz(p.at);
            if (p.aimed)
            {
                d["target"] = Xyz(p.target);
                d["apexAbove"] = Nm(p.apexAbove);
            }
            else d["target"] = null; // arena.js's plain pad: straight up to config.jumpPad.apexHeight
            return (object)d;
        });

        static Dictionary<string, object> CoreSpec(AuthoredCore c) => new Dictionary<string, object>
        {
            ["x0"] = Nm(c.x0), ["z0"] = Nm(c.z0), ["cells"] = Nm(c.cells), ["cellSize"] = Nm(c.cellSize),
        };

        static Dictionary<string, object> PerkJson(AuthoredPerk p, bool forge)
        {
            var d = new Dictionary<string, object>();
            if (!forge) d["perk"] = p.perk ?? "";
            d["x"] = Nm(p.at.x);
            d["y"] = Nm(p.at.y);
            d["z"] = Nm(p.at.z);
            if (p.core) d["core"] = true;
            d["yaw"] = Nm(p.yaw);
            if (!string.IsNullOrEmpty(p.verb)) d["verb"] = p.verb;
            return d;
        }

        static Dictionary<string, object> RelayJson(AuthoredRelay r)
        {
            var d = new Dictionary<string, object> { ["id"] = r.id ?? "", ["name"] = r.name ?? "" };
            d["x"] = Nm(r.at.x);
            d["y"] = Nm(r.at.y);
            d["z"] = Nm(r.at.z);
            if (!string.IsNullOrEmpty(r.verb)) d["verb"] = r.verb;
            return d;
        }

        static Dictionary<string, object> WallRowJson(AuthoredWallRow r)
        {
            var d = new Dictionary<string, object>
            {
                [r.zPlane ? "z" : "x"] = Nm(r.plane),
                ["y"] = Nm(r.y),
                [r.zPlane ? "xs" : "zs"] = r.slots.ConvertAll(s => Nm(s)),
                ["facing"] = Nm(r.facing),
            };
            return d;
        }

        static Dictionary<string, object> UnitySpotJson(AuthoredUnitySpot s, bool yaw)
        {
            Dictionary<string, object> d = Xyz(s.at);
            if (yaw) d["yaw"] = Nm(s.yaw);
            d["zone"] = s.zone ?? "";
            if (!string.IsNullOrEmpty(s.verb)) d["verb"] = s.verb;
            return d;
        }

        /// <summary>data.holdSpots: {x, y, z} each, and its verb (holdVerbs) last when set.</summary>
        static List<object> HoldsJson(MapModel m)
        {
            var list = new List<object>(m.holdSpots.Count);
            for (int i = 0; i < m.holdSpots.Count; i++)
            {
                Dictionary<string, object> d = Xyz(m.holdSpots[i]);
                string verb = i < m.holdVerbs.Count ? m.holdVerbs[i] : null;
                if (!string.IsNullOrEmpty(verb)) d["verb"] = verb;
                list.Add(d);
            }
            return list;
        }

        /// <summary>The auto floor (maps.mjs exportMap): {min: [-half, -config.core.floorThickness, -half], max: [half, 0, half], kind: 'floor'}.</summary>
        public static AuthoredBox WebFloor(double floorSize, GameConfig cfg)
        {
            double half = floorSize / 2;
            return new AuthoredBox
            {
                min = new MapWebVec(-half, -cfg.movement.core.floorThickness, -half), max = new MapWebVec(half, 0, half), kind = "floor",
            };
        }

        /// <summary>collision.boxes: data.boxes with the floor inserted after the last kind "floor" box (index 0 when none).</summary>
        public static List<AuthoredBox> CollisionBoxes(MapModel m, GameConfig cfg)
        {
            var list = new List<AuthoredBox>(m.boxes);
            int at = 0;
            for (int i = 0; i < m.boxes.Count; i++) if (!m.boxes[i].kindUndefined && m.boxes[i].kind == "floor") at = i + 1;
            list.Insert(at, m.hasFloor ? m.floor : WebFloor(m.floorSize, cfg));
            return list;
        }

        /// <summary>
        /// The map JSON for a model, keys in the web exporter's order; "zoneParts" and "navGrid" are null placeholders that
        /// FillNav sets (their positions are kept). `source` says where the markers came from.
        /// </summary>
        public static Dictionary<string, object> ToJson(MapModel m, GameConfig cfg, string source)
        {
            var o = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(m.why)) o["_why"] = m.why;
            o["name"] = m.id;
            o["generatedBy"] = GeneratedBy;
            o["source"] = source ?? "";
            o["floorSize"] = Nm(m.floorSize);
            o["killPlaneY"] = Nm(m.killPlaneY);
            o["collision"] = new Dictionary<string, object>
            {
                ["boxes"] = Boxes(CollisionBoxes(m, cfg)),
                ["barriers"] = Boxes(m.barriers),
                ["wedges"] = Wedges(m.wedges),
            };
            if (m.hasCore)
            {
                var presets = new Dictionary<string, object>();
                var names = new List<string>(CoreGrid.Presets);
                names.AddRange(ExtraCorePresets);
                MovementConfig mc = cfg.movement;
                foreach (string name in names)
                {
                    double[] h = CoreGrid.PresetHeights(name, (int)m.core.cells, mc.core.maxHeight, mc.core.heightStep, new Mulberry32(1)); // maps.mjs: a fresh mulberry32(1) per preset
                    var list = new List<object>(h.Length);
                    foreach (double v in h) list.Add(Nm(v));
                    presets[name] = list;
                }
                o["core"] = new Dictionary<string, object> { ["spec"] = CoreSpec(m.core), ["presets"] = presets };
            }
            else o["core"] = null;
            o["spawns"] = new Dictionary<string, object>
            {
                ["playerSpawn"] = SpawnJson(m.playerSpawn),
                ["duelSpawns"] = m.duelSpawns.ConvertAll(s => (object)SpawnJson(s)),
                ["ground"] = Points(m.groundSpawns),
                ["high"] = Points(m.highSpawns),
            };
            o["pads"] = Pads(m.pads);
            o["grappleNodes"] = Points(m.grappleNodes);
            o["zoneParts"] = null;
            o["navGrid"] = null;
            o["data"] = DataJson(m);
            if (m.HasUnitySpots) o["spots"] = SpotsJson(m);
            return o;
        }

        static Dictionary<string, object> DataJson(MapModel m)
        {
            var d = new Dictionary<string, object>
            {
                ["name"] = m.id,
                ["floorSize"] = Nm(m.floorSize),
                ["killPlaneY"] = Nm(m.killPlaneY),
                ["boxes"] = Boxes(m.boxes),
                ["wedges"] = Wedges(m.wedges),
                ["strips"] = Boxes(m.strips),
                ["barriers"] = Boxes(m.barriers),
                ["pads"] = Pads(m.pads),
                ["core"] = m.hasCore ? CoreSpec(m.core) : null,
                ["spawns"] = new Dictionary<string, object> { ["ground"] = Points(m.groundSpawns), ["high"] = Points(m.highSpawns) },
                ["playerSpawn"] = SpawnJson(m.playerSpawn),
                ["duelSpawns"] = m.duelSpawns.ConvertAll(s => (object)SpawnJson(s)),
            };
            if (m.grappleNodes.Count > 0) d["grappleNodes"] = Points(m.grappleNodes);
            if (m.zones.Count > 0) d["startZone"] = m.startZone ?? "";
            if (m.armoryWall != null) d["armoryWall"] = WallRowJson(m.armoryWall);
            if (m.armoryRack != null) d["armoryRack"] = WallRowJson(m.armoryRack);
            if (m.armoryBack != null) d["armoryBack"] = WallRowJson(m.armoryBack);
            if (m.perkSpots.Count > 0) d["perkSpots"] = m.perkSpots.ConvertAll(p => (object)PerkJson(p, false));
            if (m.trapSpots.Count > 0)
                d["trapSpots"] = m.trapSpots.ConvertAll(t => (object)new Dictionary<string, object>
                {
                    ["trap"] = t.trap ?? "", ["x"] = Nm(t.at.x), ["y"] = Nm(t.at.y), ["z"] = Nm(t.at.z), ["sx"] = Nm(t.sx), ["sz"] = Nm(t.sz),
                });
            if (m.gadgetSpots.Count > 0)
                d["gadgetSpots"] = m.gadgetSpots.ConvertAll(g => (object)new Dictionary<string, object>
                {
                    ["gadget"] = g.gadget ?? "", ["x"] = Nm(g.at.x), ["y"] = Nm(g.at.y), ["z"] = Nm(g.at.z), ["yaw"] = Nm(g.yaw),
                });
            if (m.hasForge) d["forgeSpot"] = PerkJson(m.forgeSpot, true);
            if (m.holdSpots.Count > 0) d["holdSpots"] = HoldsJson(m);
            if (m.hasSecretRelay) d["secretRelay"] = RelayJson(m.secretRelay);
            if (m.relaySpots.Count > 0) d["relaySpots"] = m.relaySpots.ConvertAll(r => (object)RelayJson(r));
            if (m.zones.Count > 0 || m.zoneHoles.Count > 0)
            {
                d["zones"] = m.zones.ConvertAll(z =>
                {
                    var zd = new Dictionary<string, object> { ["id"] = z.id ?? "", ["name"] = z.name ?? "", ["priority"] = Nm(z.priority) };
                    if (z.hasCost) zd["cost"] = Nm(z.cost);
                    zd["boxes"] = Boxes(z.boxes);
                    zd["spots"] = z.spots.ConvertAll(s =>
                    {
                        Dictionary<string, object> sd = Xyz(s.at);
                        sd["face"] = new List<object> { Nm(s.faceX), Nm(s.faceZ) };
                        return (object)sd;
                    });
                    return (object)zd;
                });
                d["zoneHoles"] = Boxes(m.zoneHoles);
            }
            if (!string.IsNullOrEmpty(m.extraDataJson) && JsJson.Parse(m.extraDataJson) is Dictionary<string, object> extra)
                foreach (KeyValuePair<string, object> kv in extra)
                    if (!d.ContainsKey(kv.Key)) d[kv.Key] = kv.Value;
            return d;
        }

        static Dictionary<string, object> SpotsJson(MapModel m)
        {
            var s = new Dictionary<string, object>();
            if (m.boxSpots.Count > 0) s["box"] = m.boxSpots.ConvertAll(b => (object)UnitySpotJson(b, true));
            if (m.hasPower) s["power"] = UnitySpotJson(m.power, true);
            if (m.hasExfilRadio && m.exfilSites.Count > 0)
            {
                s["exfil"] = new Dictionary<string, object>
                {
                    ["radio"] = UnitySpotJson(m.exfilRadio, true),
                    ["sites"] = m.exfilSites.ConvertAll(x => (object)UnitySpotJson(x, false)),
                };
            }
            else if (m.hasExfilRadio || m.exfilSites.Count > 0)
                m.warnings.Add($"spots.exfil needs the radio and at least one site (radio {(m.hasExfilRadio ? "set" : "missing")}, {m.exfilSites.Count} sites): not written, the design block's exfil applies");
            return s;
        }

        /// <summary>
        /// The whole export of a model (no Unity needed): ToJson, FillNav, and the file's text (JSON.stringify + "\n").
        /// `imported`: the source map for MapNavMode.KeepIfGeometryUnchanged (null: Build).
        /// </summary>
        public static (Dictionary<string, object> json, string text, bool navKept) Export(MapModel m, GameConfig cfg, string source, MapNavMode nav = MapNavMode.Build, Dictionary<string, object> imported = null)
        {
            Dictionary<string, object> json = ToJson(m, cfg, source);
            bool kept = FillNav(json, cfg, nav, imported);
            return (json, MapJsonWriter.WriteFile(json), kept);
        }

        /// <summary>The web's maps (PortExport/maps): a Design/maps file must not reuse their ids (NIGHT15 rule 4).</summary>
        public static readonly string[] WebMapIds = { "survival", "spire", "training" };

        /// <summary>
        /// Writes a map's text to Design/maps/&lt;id&gt;.json and its byte-identical Player copy under
        /// Assets/StreamingAssets/Design/maps (UTF-8, no BOM, LF as written). Refuses the web's map ids.
        /// </summary>
        public static (string designPath, string playerCopyPath) WriteFiles(string repoRoot, string id, string text)
        {
            if (Array.IndexOf(WebMapIds, id) >= 0)
                throw new ArgumentException($"'{id}' is a web map (PortExport/maps/{id}.json stays the web's, NIGHT15 rule 4): give the map another id, or write the round trip with WriteTo");
            string design = System.IO.Path.Combine(repoRoot, UnityTuning.MapDesignDir, id + ".json");
            string player = System.IO.Path.Combine(repoRoot, UnityTuning.MapPlayerCopyDir, id + ".json");
            WriteTo(design, text);
            WriteTo(player, text);
            return (design, player);
        }

        /// <summary>Writes a map's text to any path (the folder made), UTF-8 without a BOM.</summary>
        public static void WriteTo(string path, string text)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) System.IO.Directory.CreateDirectory(dir);
            System.IO.File.WriteAllText(path, text, new System.Text.UTF8Encoding(false));
        }

        // --- the nav grid -------------------------------------------------------------------------------------------

        /// <summary>
        /// The text of everything the nav grid and the zone parts are built from (NavGridBuilder.ExportFor reads collision,
        /// core.spec, floorSize, data.zones, data.zoneHoles, data.startZone). Equal texts: the imported grid is still valid
        /// (for the same config.ai.grid).
        /// </summary>
        public static string NavSignature(Dictionary<string, object> map)
        {
            var col = O(Get(map, "collision"));
            var data = O(Get(map, "data"));
            return MapJsonWriter.Write(new List<object>
            {
                Get(map, "floorSize"), Get(col, "boxes"), Get(col, "barriers"), Get(col, "wedges"), Get(O(Get(map, "core")), "spec"),
                Get(data, "zones"), Get(data, "zoneHoles"), Get(data, "startZone"),
            });
        }

        /// <summary>
        /// Sets json's "navGrid" and "zoneParts": the imported map's when `mode` allows and NavSignature is unchanged, else
        /// NavGridBuilder.ExportFor's. Returns true when the imported grid was kept.
        /// </summary>
        public static bool FillNav(Dictionary<string, object> json, GameConfig cfg, MapNavMode mode, Dictionary<string, object> imported)
        {
            if (mode == MapNavMode.KeepIfGeometryUnchanged && imported != null && Get(imported, "navGrid") is Dictionary<string, object> grid
                && NavSignature(json) == NavSignature(imported))
            {
                json["zoneParts"] = Get(imported, "zoneParts");
                json["navGrid"] = grid;
                return true;
            }
            (Dictionary<string, object> navGrid, Dictionary<string, object> zoneParts) = NavGridBuilder.ExportFor(cfg, json);
            json["zoneParts"] = zoneParts;
            json["navGrid"] = navGrid;
            return false;
        }
    }
}
