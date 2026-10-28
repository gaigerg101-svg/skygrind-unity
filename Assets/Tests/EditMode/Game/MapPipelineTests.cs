// NIGHT15 U3-28.4 (builder P): the map pipeline: markers -> Design/maps/<id>.json, a map JSON -> markers, the validator.
// - The writer prints JSON.stringify's text: the web's three map files come back byte for byte from JsJson.Parse.
// - The round trip on survival.json: import, export; the sim-relevant keys (MapRoundTrip.SimKeys: every key the game
//   reads, and the whole level data) print the same text, and so does the whole file once the diagnostics the export
//   drops or rewrites (MapRoundTrip.DiagnosticKeys: generatedBy, source, the notes, collision.triangles,
//   collision.droppedByOctree, duelNavGraph) are stripped from both sides. With the nav grid kept (no builder N needed)
//   and with it built by NavGridBuilder.ExportFor (U3-28.3), whose round trip also plays the fingerprint run.
// - A moved handle exports its snapped values; a Unity-authored map writes _why, spots, zone costs and its own floors.
// - The validator on survival.json: every zone reached in price order; its findings are the perches only.
// The core.autocrlf checkout holds CRLF; the files (and the index) hold LF, so the texts compare with LF.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using Skygrind.Game.MapAuthoring;
using UnityEngine;
using MarkerBox = Skygrind.Game.MapAuthoring.MapBox; // Skygrind.Core has a struct MapBox too

namespace Skygrind.Game.Tests
{
    public class MapPipelineTests
    {
        static readonly string Repo = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        static GameConfig config;
        static GameConfig Cfg => config ??= GameConfig.Load(Path.Combine(Repo, "PortExport", "config.json"));

        static string MapText(string name) => File.ReadAllText(Path.Combine(Repo, "PortExport", "maps", name + ".json")).Replace("\r\n", "\n");

        static Dictionary<string, object> LoadMap(string name) => PortData.Obj(JsJson.Parse(MapText(name)));

        readonly List<GameObject> made = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in made) if (go != null) Object.DestroyImmediate(go);
            made.Clear();
        }

        MapRoot Import(string name)
        {
            MapRoot root = MapImport.Build(LoadMap(name), $"PortExport/maps/{name}.json");
            made.Add(root.gameObject);
            return root;
        }

        static void AssertRoundTrip(Dictionary<string, object> source, string text, string what)
        {
            var back = PortData.Obj(JsJson.Parse(text));
            List<MapKeyCompare> keys = MapRoundTrip.Compare(source, back);
            Assert.That(keys.FindAll(k => !k.same), Is.Empty, $"{what}: sim keys that differ:\n{MapRoundTrip.Describe(keys.FindAll(k => !k.same))}");
            Assert.That(MapRoundTrip.SameWithoutDiagnostics(source, back, out int at), Is.True, $"{what}: the files differ at char {at} once the diagnostics are stripped");
            foreach (string path in MapRoundTrip.DiagnosticKeys)
            {
                if (path == "generatedBy" || path == "source") continue;
                // Has, not At: At reads a key under a null parent as found (null on both sides, for Compare); training's "core" is null.
                Assert.That(MapRoundTrip.Has(back, path), Is.False, $"{what}: the export writes the diagnostic {path}");
            }
        }

        // --- the writer ---

        [Test]
        public void Writer_PrintsNumbersAsV8()
        {
            var cases = new (double v, string js)[]
            {
                (0, "0"), (-0.0, "0"), (100, "100"), (-47.5, "-47.5"), (12.3, "12.3"), (0.1 + 0.2, "0.30000000000000004"),
                (1.5707963267948966, "1.5707963267948966"), (2.9117618741296187, "2.9117618741296187"), (8.399999618530273, "8.399999618530273"),
                (1e-7, "1e-7"), (0.000001, "0.000001"), (1.5e-7, "1.5e-7"), (1e21, "1e+21"), (1e20, "100000000000000000000"),
                (123456789012345680000.0, "123456789012345680000"), (1152921504606846976.0, "1152921504606847000"), (9007199254740993.0, "9007199254740992"),
                (5e-324, "5e-324"), (double.MaxValue, "1.7976931348623157e+308"), (2.2250738585072014e-308, "2.2250738585072014e-308"),
                (-1.4142135381698608, "-1.4142135381698608"), (4.35, "4.35"), (0.5, "0.5"),
            };
            foreach ((double v, string js) in cases) Assert.That(MapJsonWriter.NumberText(v), Is.EqualTo(js), $"Number({v:R})");
            Assert.That(MapJsonWriter.Write(MapJsonWriter.Num(-0.0)), Is.EqualTo("{\"$num\":\"-0\"}"), "enc(-0)");
            Assert.That(MapJsonWriter.Write(MapJsonWriter.Num(double.NaN)), Is.EqualTo("{\"$num\":\"NaN\"}"), "enc(NaN)");
            Assert.That(MapJsonWriter.Write(MapJsonWriter.Num(double.NegativeInfinity)), Is.EqualTo("{\"$num\":\"-Infinity\"}"), "enc(-Infinity)");
            Assert.That(MapJsonWriter.Write(double.NaN), Is.EqualTo("null"), "JSON.stringify(NaN)");
        }

        [Test]
        public void Writer_QuotesStringsAsV8()
        {
            Assert.That(MapJsonWriter.Write("a\"b\\c\n\t\u0001/\u007f°"), Is.EqualTo("\"a\\\"b\\\\c\\n\\t\\u0001/\u007f°\""));
            Assert.That(MapJsonWriter.Write("\ud800x"), Is.EqualTo("\"\\ud800x\""), "a lone surrogate");
            var o = new Dictionary<string, object> { ["b"] = 1.0, ["a"] = new List<object> { true, null, "s" } };
            Assert.That(MapJsonWriter.Write(o), Is.EqualTo("{\"b\":1,\"a\":[true,null,\"s\"]}"), "key order kept, no spaces");
        }

        [Test]
        public void Writer_PrintsTheWebsMapsByteForByte()
        {
            foreach (string name in new[] { "survival", "spire", "training" })
            {
                string text = MapText(name);
                Assert.That(MapJsonWriter.WriteFile(JsJson.Parse(text)), Is.EqualTo(text), $"{name}.json: JsJson.Parse then MapJsonWriter.WriteFile");
            }
        }

        // --- the model's round trip (no Unity objects) ---

        [Test]
        public void Model_RoundTripsTheWebsMaps_NavKept()
        {
            foreach (string name in new[] { "survival", "spire", "training" })
            {
                Dictionary<string, object> source = LoadMap(name);
                MapModel model = MapModelJson.FromJson(source);
                (Dictionary<string, object> _, string text, bool kept) = MapModelJson.Export(model, Cfg, "test", MapNavMode.KeepIfGeometryUnchanged, source);
                Assert.That(kept, Is.True, $"{name}: the imported nav grid is kept (geometry unchanged)");
                AssertRoundTrip(source, text, name);
                Assert.That(model.warnings.Count, Is.EqualTo(name == "training" ? 3 : 0), $"{name}: {string.Join("; ", model.warnings)}");
            }
        }

        // --- import ---

        [Test]
        public void Import_Survival_MakesOneMarkerPerEntry()
        {
            MapRoot root = Import("survival");
            Assert.That(root.mapId, Is.EqualTo("survival"));
            Assert.That(root.startZone, Is.EqualTo("landing"));
            Assert.That(root.hasCore, Is.True);
            Assert.That(root.GetComponentsInChildren<MarkerBox>().Length, Is.EqualTo(97), "the floor and data.boxes' 96");
            Assert.That(root.GetComponentsInChildren<MapStrip>().Length, Is.EqualTo(18));
            Assert.That(root.GetComponentsInChildren<MapBarrier>().Length, Is.EqualTo(5));
            Assert.That(root.GetComponentsInChildren<MapWedge>().Length, Is.EqualTo(8));
            Assert.That(root.GetComponentsInChildren<MapPad>().Length, Is.EqualTo(8));
            Assert.That(root.GetComponentsInChildren<MapGrapple>().Length, Is.EqualTo(17));
            Assert.That(root.GetComponentsInChildren<MapSpawn>().Length, Is.EqualTo(38), "28 ground and 10 high");
            Assert.That(root.GetComponentsInChildren<MapPlayerSpawn>().Length, Is.EqualTo(3), "the player's and 2 Duel spawns");
            Assert.That(root.GetComponentsInChildren<MapZone>().Length, Is.EqualTo(6));
            Assert.That(root.GetComponentsInChildren<MapZoneBox>().Length, Is.EqualTo(28), "25 zone boxes (1, 1, 4, 1, 8, 10) and 3 holes");
            Assert.That(root.GetComponentsInChildren<MapZoneBuy>().Length, Is.EqualTo(24));
            Assert.That(root.GetComponentsInChildren<MapPerkSpot>().Length, Is.EqualTo(14));
            Assert.That(root.GetComponentsInChildren<MapForge>().Length, Is.EqualTo(1));
            Assert.That(root.GetComponentsInChildren<MapWallBuy>().Length, Is.EqualTo(16), "armoryWall 11, armoryRack 4, armoryBack 1");
            Assert.That(root.GetComponentsInChildren<MapTrap>().Length, Is.EqualTo(4));
            Assert.That(root.GetComponentsInChildren<MapGadgetSpot>().Length, Is.EqualTo(1));
            Assert.That(root.GetComponentsInChildren<MapHoldSpot>().Length, Is.EqualTo(5));
            Assert.That(root.GetComponentsInChildren<MapRelay>().Length, Is.EqualTo(5), "4 relays and the secret one");
            Assert.That(root.GetComponentsInChildren<MapBoxSpot>().Length + root.GetComponentsInChildren<MapPower>().Length + root.GetComponentsInChildren<MapExfil>().Length, Is.EqualTo(0), "survival's are the design block's");
        }

        [Test]
        public void Import_Survival_PlacesTheHandlesInUnitySpace()
        {
            MapRoot root = Import("survival");
            MarkerBox floor = root.GetComponentsInChildren<MarkerBox>()[0];
            Assert.That(floor.IsFloor, Is.True, "the floor marker comes first");
            Assert.That(floor.transform.position, Is.EqualTo(new Vector3(0, -1, 0)));
            Assert.That(floor.transform.lossyScale, Is.EqualTo(new Vector3(96, 2, 96)));
            MarkerBox cover = root.GetComponentsInChildren<MarkerBox>()[5]; // data.boxes[4]: min (-6, 0, -47), max (-4, 1.5, -45)
            Assert.That(cover.transform.position, Is.EqualTo(new Vector3(-5, 0.75f, 46)), "web z -46 is Unity z 46");
            MapPad pad = root.GetComponentsInChildren<MapPad>()[0]; // (0, 0, -26.5) aimed at (0, 9, -39.5)
            Assert.That(pad.transform.position, Is.EqualTo(new Vector3(0, 0, 26.5f)));
            Assert.That(pad.target, Is.Not.Null);
            Assert.That(pad.target.position, Is.EqualTo(new Vector3(0, 9, 39.5f)));
            Assert.That(pad.target.parent, Is.EqualTo(pad.transform.parent), "the target is a sibling handle");
            MapPerkSpot vigor = root.GetComponentsInChildren<MapPerkSpot>()[3]; // yaw pi / 2: its front faces web +x
            Assert.That(Mathf.DeltaAngle(vigor.transform.eulerAngles.y, 90), Is.EqualTo(0).Within(1e-3));
            Assert.That(Vector3.Dot(vigor.transform.forward, Vector3.right), Is.EqualTo(1).Within(1e-5));
            MapPlayerSpawn player = root.GetComponentsInChildren<MapPlayerSpawn>()[0];
            Assert.That(player.duel, Is.False);
            Assert.That(player.transform.position, Is.EqualTo(new Vector3(0, 0, -40)));
        }

        // --- the round trip ---

        [Test]
        public void RoundTrip_Survival_NavKept_SimKeysByteIdentical()
        {
            MapRoot root = Import("survival");
            MapExportResult r = MapExport.Export(root, Cfg, MapNavMode.KeepIfGeometryUnchanged, Repo);
            Assert.That(r.navKept, Is.True, "nothing moved: the imported nav grid is kept");
            Assert.That(r.warnings, Is.Empty);
            AssertRoundTrip(LoadMap("survival"), r.text, "survival (nav kept)");
        }

        /// <summary>The acceptance: the nav grid and zone parts rebuilt by NavGridBuilder.ExportFor (builder N, U3-28.3), the
        /// sim keys byte-identical, and the fingerprint run on the round-tripped map equal to the baseline.</summary>
        [Test]
        public void RoundTrip_Survival_NavBuilt_SimKeysByteIdentical_AndTheFingerprintIsTheBaseline()
        {
            MapRoot root = Import("survival");
            MapExportResult r = MapExport.Export(root, Cfg, MapNavMode.Build, Repo);
            Assert.That(r.navKept, Is.False);
            AssertRoundTrip(LoadMap("survival"), r.text, "survival (nav built)");
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(JsJson.Parse(r.text)));
            SurvivalSim sim = SurvivalSim.Headless(GameConfig.Load(Path.Combine(Repo, "PortExport", "config.json")), map); // a fresh config, as FingerprintTests
            List<string> lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            int diff = FingerprintBaseline.FirstDifference(lines);
            Assert.That(diff, Is.EqualTo(-1), $"the round-tripped map's fingerprint: first difference at checkpoint {diff + 1}; {summary}");
        }

        [Test]
        public void RoundTrip_SpireAndTraining_FromMarkers()
        {
            foreach (string name in new[] { "spire", "training" })
            {
                MapRoot root = Import(name);
                MapExportResult r = MapExport.Export(root, Cfg, MapNavMode.KeepIfGeometryUnchanged, Repo);
                Assert.That(r.navKept, Is.True, name);
                AssertRoundTrip(LoadMap(name), r.text, name);
            }
        }

        // --- moved handles ---

        [Test]
        public void Export_MovedHandles_SnapToTheGrid_TheRestStaysExact()
        {
            MapRoot root = Import("survival");
            MapModel before = MapExport.ToModel(root);
            MarkerBox cover = root.GetComponentsInChildren<MarkerBox>()[5]; // data.boxes[4]
            cover.transform.position += new Vector3(1.234f, 0, 0);
            MapPad pad = root.GetComponentsInChildren<MapPad>()[0];
            pad.target.position += new Vector3(2, 0, 0);
            MapPerkSpot steady = root.GetComponentsInChildren<MapPerkSpot>()[0]; // yaw 0: euler 180
            steady.transform.rotation = Quaternion.Euler(0, 92, 0); // snaps to 90: yaw 90 degrees
            MapModel after = MapExport.ToModel(root);
            // web x -6 .. -4 moved 1.234 m: -4.766 .. -2.766 on the 0.05 m grid; y and z come back exact from the float transform.
            Assert.That(after.boxes[4].min, Is.EqualTo(new MapWebVec(-4.75, 0, -47)));
            Assert.That(after.boxes[4].max, Is.EqualTo(new MapWebVec(-2.75, 1.5, -45)));
            Assert.That(after.boxes[4].kind, Is.EqualTo("cover"));
            for (int i = 0; i < before.boxes.Count; i++)
                if (i != 4) Assert.That(MapJsonWriter.Write(new List<object> { after.boxes[i].min.x, after.boxes[i].max.z }), Is.EqualTo(MapJsonWriter.Write(new List<object> { before.boxes[i].min.x, before.boxes[i].max.z })), $"box {i} unmoved");
            Assert.That(after.pads[0].target, Is.EqualTo(new MapWebVec(2, 9, -39.5)), "the target handle moved 2 m along Unity +x (web +x)");
            Assert.That(after.pads[0].at, Is.EqualTo(before.pads[0].at), "the pad itself didn't move");
            Assert.That(after.pads[1].target, Is.EqualTo(before.pads[1].target));
            Assert.That(after.perkSpots[0].yaw, Is.EqualTo(90 * System.Math.PI / 180), "euler 92 snaps to 90; yaw = 180 - 90 degrees");
            Assert.That(after.perkSpots[0].at, Is.EqualTo(before.perkSpots[0].at), "rotating keeps the exact position");
            Assert.That(after.wedges[0].yaw, Is.EqualTo(1.5707963267948966), "an unrotated wedge keeps its exact yaw");
            // The moved geometry no longer matches the imported nav grid: keep mode builds it instead.
            Assert.That(MapModelJson.NavSignature(MapModelJson.ToJson(after, Cfg, "test")), Is.Not.EqualTo(MapModelJson.NavSignature(LoadMap("survival"))));
        }

        // --- a Unity-authored map ---

        MapRoot AuthoredMap(bool floors)
        {
            MapRoot root = MapAuthor.Root("testyard", 64, -20, "start", "a test map");
            made.Add(root.gameObject);
            root.hasCore = true; // SurvivalMap.FromExport reads core.spec unconditionally
            root.core = new AuthoredCore { x0 = -8, z0 = -8, cells = 4, cellSize = 4 };
            Transform geo = MapAuthor.Group(root.transform, "Geometry");
            if (floors)
            {
                MapAuthor.Box(geo, MapAuthor.V(-32, -2, -32), MapAuthor.V(0, 0, 32), "floor", "main floor");
                MapAuthor.Box(geo, MapAuthor.V(0, -2, -10), MapAuthor.V(20, 0, 10), "floor", "extra slab");
            }
            MapAuthor.Box(geo, MapAuthor.V(-4, 0, -4), MapAuthor.V(4, 2.5, 4), "cover");
            MapAuthor.Pad(geo, MapAuthor.V(-10, 0, 0), MapAuthor.V(-10, 6, 12), 1.5);
            Transform zones = MapAuthor.Group(root.transform, "Zones");
            MapZone start = MapAuthor.Zone(zones, "start", "START", 1);
            MapAuthor.ZoneBox(start.transform, MapAuthor.V(-32, 0, -32), MapAuthor.V(0, 40, 32));
            MapZone yard = MapAuthor.Zone(zones, "yard", "YARD", 2, 1500);
            MapAuthor.ZoneBox(yard.transform, MapAuthor.V(0, 0, -10), MapAuthor.V(20, 40, 10));
            MapAuthor.ZoneBuy(yard.transform, MapAuthor.V(-1.5, 0, 0), -1, 0);
            Transform spots = MapAuthor.Group(root.transform, "Spots");
            MapAuthor.PlayerSpawn(spots, MapAuthor.V(-20, 0, 0), 90);
            MapAuthor.BoxSpot(spots, MapAuthor.V(-25, 0, 10), System.Math.PI, "start");
            MapAuthor.Power(spots, MapAuthor.V(15, 0, 5), 0, "yard");
            MapAuthor.Exfil(spots, MapAuthor.V(-28, 0, -5), System.Math.PI / 2, "start", true);
            MapAuthor.Exfil(spots, MapAuthor.V(10, 0, 0), 0, "yard", false);
            MapAuthor.WallBuy(spots, MapWallRows.Wall, MapAuthor.V(-31.5, 1.7, 2), -1);
            MapAuthor.WallBuy(spots, MapWallRows.Wall, MapAuthor.V(-31.5, 1.7, 4), -1);
            return root;
        }

        [Test]
        public void Export_UnityAuthoredMap_WritesWhySpotsCostAndItsFloors()
        {
            MapModel m = MapExport.ToModel(AuthoredMap(true));
            Assert.That(m.warnings, Is.Empty, string.Join("; ", m.warnings));
            Dictionary<string, object> json = MapModelJson.ToJson(m, Cfg, "test");
            Assert.That(new List<string>(json.Keys)[0], Is.EqualTo("_why"), "_why is the file's header");
            var data = PortData.Obj(json["data"]);
            var col = PortData.Obj(json["collision"]);
            string main = "{\"min\":[-32,-2,-32],\"max\":[0,0,32],\"kind\":\"floor\"}", slab = "{\"min\":[0,-2,-10],\"max\":[20,0,10],\"kind\":\"floor\"}";
            string cover = "{\"min\":[-4,0,-4],\"max\":[4,2.5,4],\"kind\":\"cover\"}";
            Assert.That(MapJsonWriter.Write(data["boxes"]), Is.EqualTo($"[{slab},{cover}]"), "the extra slab is drawn and collided with the boxes; the main floor is not a data box");
            Assert.That(MapJsonWriter.Write(col["boxes"]), Is.EqualTo($"[{slab},{main},{cover}]"), "the main floor is the last floor in collision.boxes (SurvivalMap's map.floor), no automatic floor");
            var zones = PortData.Arr(data["zones"]);
            Assert.That(PortData.Obj(zones[0]).ContainsKey("cost"), Is.False, "no price set: the config's");
            Assert.That(MapJsonWriter.Write(zones[1]), Is.EqualTo("{\"id\":\"yard\",\"name\":\"YARD\",\"priority\":2,\"cost\":1500,\"boxes\":[{\"min\":[0,0,-10],\"max\":[20,40,10],\"kind\":{\"$undefined\":true}}],\"spots\":[{\"x\":-1.5,\"y\":0,\"z\":0,\"face\":[-1,0]}]}"));
            Assert.That(data["startZone"], Is.EqualTo("start"));
            Assert.That(MapJsonWriter.Write(data["armoryWall"]), Is.EqualTo("{\"x\":-31.5,\"y\":1.7,\"zs\":[2,4],\"facing\":-1}"));
            Assert.That(MapJsonWriter.Write(json["spots"]), Is.EqualTo(
                "{\"box\":[{\"x\":-25,\"y\":0,\"z\":10,\"yaw\":3.141592653589793,\"zone\":\"start\"}],\"power\":{\"x\":15,\"y\":0,\"z\":5,\"yaw\":0,\"zone\":\"yard\"},"
                + "\"exfil\":{\"radio\":{\"x\":-28,\"y\":0,\"z\":-5,\"yaw\":1.5707963267948966,\"zone\":\"start\"},\"sites\":[{\"x\":10,\"y\":0,\"z\":0,\"zone\":\"yard\"}]}}"));
            Assert.That(MapJsonWriter.Write(PortData.Obj(json["spawns"])["playerSpawn"]), Is.EqualTo("{\"x\":-20,\"y\":0,\"z\":0,\"yawDeg\":90}"));
            Assert.That(MapJsonWriter.Write(json["pads"]), Is.EqualTo("[{\"x\":-10,\"y\":0,\"z\":0,\"target\":{\"x\":-10,\"y\":6,\"z\":12},\"apexAbove\":1.5}]"));
            // SurvivalMap reads it (the spots and the floor) once the nav grid is in.
            json["zoneParts"] = new Dictionary<string, object> { ["startZone"] = "start", ["parts"] = new List<object>() };
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(JsJson.Parse(MapJsonWriter.Write(json))));
            Assert.That(map.floor.max.x, Is.EqualTo(0), "map.floor is the main floor");
            Assert.That(map.boxes.Count, Is.EqualTo(2), "the slab and the cover are drawn");
            Assert.That(map.spots, Is.Not.Null);
        }

        [Test]
        public void Export_NoFloorMarker_AddsTheWebExportersFloor()
        {
            MapModel m = MapExport.ToModel(AuthoredMap(false));
            var col = PortData.Obj(MapModelJson.ToJson(m, Cfg, "test")["collision"]);
            string floor = MapJsonWriter.Write(PortData.Arr(col["boxes"])[0]);
            Assert.That(floor, Is.EqualTo("{\"min\":[-32,-2,-32],\"max\":[32,0,32],\"kind\":\"floor\"}"), "floorSize 64, config core.floorThickness 2");
        }

        [Test]
        public void Export_WallGunsOffOnePlane_Warn()
        {
            MapRoot root = AuthoredMap(true);
            MapAuthor.WallBuy(root.transform, MapWallRows.Wall, MapAuthor.V(-30, 1.7, 6), -1, "off the plane");
            MapModel m = MapExport.ToModel(root);
            Assert.That(m.warnings.Exists(w => w.Contains("off the plane") && w.Contains("one plane")), Is.True, string.Join("; ", m.warnings));
            Assert.That(m.armoryWall.slots, Is.EqualTo(new List<double> { 2, 4, 6 }));
            Assert.That(m.armoryWall.plane, Is.EqualTo(-31.5), "the row's plane is its first slot's");
        }

        [Test]
        public void Export_RefusesTheWebsMapIds()
        {
            Assert.Throws<System.ArgumentException>(() => MapModelJson.WriteFiles(Path.GetTempPath(), "survival", "{}\n"));
        }

        // --- the validator ---

        [Test]
        public void Validate_Survival_EveryZoneInPriceOrder_TheFindingsArePerchesOnly()
        {
            (string md, int findings) = MapValidate.Run(Cfg, LoadMap("survival"), "PortExport/maps/survival.json");
            var rows = new List<string>(md.Split('\n')).FindAll(l => l.StartsWith("| ") && !l.StartsWith("| check"));
            foreach (string zone in new[] { "landing (start)", "armory (cost 750)", "slabs (cost 1000)", "grinder (cost 1250)", "heights (cost 1500)", "islands (cost 2000)" })
                Assert.That(rows.Exists(l => l.StartsWith($"| zone {zone} | PASS |")), Is.True, $"zone {zone}:\n{md}");
            var found = rows.FindAll(l => l.Contains("| FINDING |"));
            Assert.That(findings, Is.EqualTo(found.Count));
            var perches = new[] { "| perk flow |", "| perk static |", "| relay chimney |", "| relay pinnacle |", "| secret relay roof |" };
            foreach (string l in found) Assert.That(System.Array.Exists(perches, p => l.StartsWith(p)), Is.True, $"an unexpected finding: {l}");
            Assert.That(findings, Is.EqualTo(5), md);
        }

        // --- designed verb gates (the lead, NIGHT15: DRYDOCK's KEEL TOP, LAUNCH RAIL, crow's nest) ---

        static Dictionary<string, object> SurvivalRelay(Dictionary<string, object> map, string id) =>
            PortData.Obj(PortData.Arr(PortData.Obj(map["data"])["relaySpots"]).Find(o => (string)PortData.Obj(o)["id"] == id));

        [Test]
        public void Validate_AVerbSpotTheGraphCantReach_IsAVerbNotAFinding()
        {
            Dictionary<string, object> map = LoadMap("survival");
            SurvivalRelay(map, "chimney")["verb"] = "wallRide"; // (42, 20.5, 1.6) in islands: no path on RIVAL's graph
            (string md, int findings) = MapValidate.Run(Cfg, map);
            Assert.That(md, Does.Contain("| relay chimney | VERB | by design: reached by wallRide;"), md);
            Assert.That(md, Does.Contain("zone islands is reached"));
            Assert.That(md, Does.Contain("**findings: 4** (113 checks, 1 VERB)"), "a VERB row is not a finding");
            Assert.That(findings, Is.EqualTo(4));
        }

        [Test]
        public void Validate_AVerbSpotInAZoneNotReached_IsStillAFinding()
        {
            Dictionary<string, object> map = LoadMap("survival");
            SurvivalRelay(map, "chimney")["verb"] = "wallRide";
            foreach (object z in PortData.Arr(PortData.Obj(map["data"])["zones"]))
                if ((string)PortData.Obj(z)["id"] == "islands") PortData.Obj(z)["cost"] = 1.0; // checked while only landing is open
            (string md, int findings) = MapValidate.Run(Cfg, map);
            Assert.That(md, Does.Contain("| zone islands (cost 1) | FINDING |"), md);
            Assert.That(md, Does.Contain("| relay chimney | FINDING | by design reached by wallRide, but its zone islands is not reached;"), md);
            Assert.That(md, Does.Not.Contain("| VERB |"));
            Assert.That(findings, Is.EqualTo(6), "the 5 perches and the islands zone");
            SurvivalRelay(map, "chimney")["verb"] = "teleport"; // and a verb outside UnityTuning.MapVerbs stays a FINDING
            foreach (object z in PortData.Arr(PortData.Obj(map["data"])["zones"])) PortData.Obj(z).Remove("cost");
            (string md2, int findings2) = MapValidate.Run(Cfg, map);
            Assert.That(md2, Does.Contain("| relay chimney | FINDING | verb 'teleport' is none of"), md2);
            Assert.That(findings2, Is.EqualTo(5));
        }

        [Test]
        public void RoundTrip_KeepsTheVerbs_FromMarkers()
        {
            Dictionary<string, object> map = LoadMap("survival");
            var data = PortData.Obj(map["data"]);
            SurvivalRelay(map, "chimney")["verb"] = "wallRide";
            PortData.Obj(data["secretRelay"])["verb"] = "wallJump";
            PortData.Obj(PortData.Arr(data["perkSpots"])[10])["verb"] = "slideJump";
            PortData.Obj(data["forgeSpot"])["verb"] = "pad";
            PortData.Obj(PortData.Arr(data["holdSpots"])[1])["verb"] = "arcline";
            map["spots"] = PortData.Obj(JsJson.Parse("{\"box\":[{\"x\":1,\"y\":0,\"z\":2,\"yaw\":0,\"zone\":\"landing\",\"verb\":\"pad\"}]}"));
            MapRoot root = MapImport.Build(map, "test", out List<string> warnings);
            made.Add(root.gameObject);
            Assert.That(warnings, Is.Empty, string.Join("; ", warnings));
            Assert.That(root.GetComponentsInChildren<MapHoldSpot>()[1].verb, Is.EqualTo("arcline"));
            MapExportResult r = MapExport.Export(root, Cfg, MapNavMode.KeepIfGeometryUnchanged, Repo);
            Assert.That(r.navKept, Is.True, "a verb is not geometry");
            AssertRoundTrip(map, r.text, "survival with verbs");
            var back = PortData.Obj(PortData.Obj(JsJson.Parse(r.text))["data"]);
            Assert.That(MapJsonWriter.Write(back["forgeSpot"]), Is.EqualTo("{\"x\":-22,\"y\":0,\"z\":-22,\"core\":true,\"yaw\":0.7853981633974483,\"verb\":\"pad\"}"), "the verb is written last");
        }

        [Test]
        public void Validate_BrokenMap_ReportsTheBreaks()
        {
            Dictionary<string, object> map = LoadMap("survival");
            var data = PortData.Obj(map["data"]);
            var target = PortData.Obj(PortData.Obj(PortData.Arr(data["pads"])[0])["target"]);
            target["y"] = 19.0; // 10 m over the bridge it lands on
            map["killPlaneY"] = 5.0; // above the floor
            (string md, int findings) = MapValidate.Run(Cfg, map);
            Assert.That(md, Does.Contain("| pad 0 | FINDING |"));
            Assert.That(md, Does.Contain("| kill plane | FINDING |"));
            Assert.That(md, Does.Contain("| above the kill plane | FINDING |"));
            Assert.That(findings, Is.GreaterThanOrEqualTo(8), md);
        }
    }
}
