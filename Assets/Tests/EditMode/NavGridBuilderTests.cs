// NIGHT15 U3-28.3 (builder N): NavGridBuilder (createNavGrid's port) and ZoneMath.ResolveZones against the oracle in the
// repo: the exported navGrid of survival (11,228 nodes), spire (11,178) and training (4,048), and survival's zoneParts
// (114 parts). Every number bit for bit (GoldenResult.SameJson: -0 and +0 differ; object keys in the export's order, so
// a JSON writer prints the same text). Also the duel's grid (maps/spire-duel-navgrid.json: the Spire with the core at
// config.core.duelPreset), which needs the core's rays and capsules beside the map's Octree (collider.js geometry).
// Each map file (up to 2.5 MB) is read and built once per run (NavGridOracle's caches).
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using static Skygrind.Core.PortData;

namespace Skygrind.Core.Tests
{
    /// <summary>The oracle files and the C# builds, each loaded or built once.</summary>
    public static class NavGridOracle
    {
        static GameConfig cfg;
        static readonly Dictionary<string, Dictionary<string, object>> maps = new Dictionary<string, Dictionary<string, object>>();
        static readonly Dictionary<string, (Dictionary<string, object> navGrid, Dictionary<string, object> zoneParts)> builds =
            new Dictionary<string, (Dictionary<string, object>, Dictionary<string, object>)>();
        public static readonly Dictionary<string, double> buildMs = new Dictionary<string, double>();

        public static GameConfig Config => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        public static Dictionary<string, object> Map(string name)
        {
            if (!maps.TryGetValue(name, out var m)) maps[name] = m = Obj(LoadJson(Path.Combine(Port.Root, "maps", name + ".json")));
            return m;
        }

        /// <summary>NavGridBuilder.ExportFor on the exported map file (the exporter's whole step).</summary>
        public static (Dictionary<string, object> navGrid, Dictionary<string, object> zoneParts) Built(string name)
        {
            if (builds.TryGetValue(name, out var b)) return b;
            var t = Stopwatch.StartNew();
            b = NavGridBuilder.ExportFor(Config, Map(name));
            buildMs[name] = t.Elapsed.TotalMilliseconds;
            TestContext.Progress.WriteLine($"[navgrid] {name}: ExportFor {buildMs[name]:F0} ms");
            return builds[name] = b;
        }

        /// <summary>A zone or hole box as the export writes it ({min, max, kind}; kind may be {"$undefined": true}).</summary>
        public static MapBox Box(object o)
        {
            var d = Obj(o);
            return new MapBox { min = V3(d["min"]), max = V3(d["max"]), kind = d.TryGetValue("kind", out object k) ? k as string : null };
        }

        /// <summary>Every mismatch of a C# value against the recorded one (numbers bit for bit, keys in order).</summary>
        public static List<string> Diff(object want, object got, string what)
        {
            var r = new GoldenResult(what);
            r.SameJson(want, got, what, ordered: true);
            return r.mismatches;
        }
    }

    public class NavGridBuilderTests
    {
        static void AssertSame(object want, object got, string what)
        {
            List<string> d = NavGridOracle.Diff(want, got, what);
            Assert.That(d.Count, Is.EqualTo(0), $"{d.Count} mismatches; first: {string.Join(" | ", d.Take(5))}");
        }

        static void AssertGridEqualsExport(string name, int nodes)
        {
            var want = Obj(NavGridOracle.Map(name)["navGrid"]);
            Assert.That((int)Num(want["count"]), Is.EqualTo(nodes), "the oracle file");
            var got = NavGridOracle.Built(name).navGrid;
            Assert.That(got, Is.Not.Null);
            Assert.That((int)Num(got["count"]), Is.EqualTo(nodes), "node count");
            AssertSame(want["linkCounts"], got["linkCounts"], name + ".linkCounts");
            AssertSame(want, got, name + ".navGrid");
        }

        // --- the exported grids (maps.mjs navGridExport) ------------------------------------------------------------

        [Test]
        public void Survival_TheBuiltGridEqualsTheExport_EveryNodeAndLink() => AssertGridEqualsExport("survival", 11228);

        [Test]
        public void Spire_TheBuiltGridEqualsTheExport_EveryNodeAndLink() => AssertGridEqualsExport("spire", 11178);

        [Test]
        public void Training_TheBuiltGridEqualsTheExport_EveryNodeAndLink() => AssertGridEqualsExport("training", 4048);

        /// <summary>The contract's own call (a ThreeOctree, no tagAt): training's collider is the map's Octree alone (no core).</summary>
        [Test]
        public void Build_OnTheMapOctree_EqualsTrainingsExport()
        {
            var json = NavGridOracle.Map("training");
            var got = NavGridBuilder.Build(NavGridOracle.Config, BuildMapOctree(json), Num(json["floorSize"]), null);
            AssertSame(json["navGrid"], got, "training.navGrid");
        }

        /// <summary>
        /// navgrid-duel.mjs: the Spire with the core at config.core.duelPreset ('towers'), collider.js geometry (the Octree,
        /// then the core). The file keeps y, tag and col of the nodes and every link.
        /// </summary>
        [Test]
        public void DuelGrid_TheCoreAtTowers_EqualsTheExport()
        {
            GameConfig cfg = NavGridOracle.Config;
            var json = NavGridOracle.Map("spire");
            var want = NavGridOracle.Map("spire-duel-navgrid"); // the file is the grid object itself
            var spec = Obj(Obj(json["core"])["spec"]);
            var cs = new CoreSpec { x0 = Num(spec["x0"]), z0 = Num(spec["z0"]), cells = (int)Num(spec["cells"]), cellSize = Num(spec["cellSize"]) };
            MovementConfig m = cfg.movement;
            var core = new CoreGrid(m, cs);
            string preset = cfg.Str("core.duelPreset");
            core.SetHeights(CoreGrid.PresetHeights(preset, cs.cells, m.core.maxHeight, m.core.heightStep, new Mulberry32(1)));
            var got = NavGridBuilder.Build(cfg, new LayeredWorld(BuildMapOctree(json), core), Num(json["floorSize"]), null, preset);
            Assert.That((int)Num(got["count"]), Is.EqualTo((int)Num(want["count"])), "node count");
            foreach (string k in new[] { "cell", "size", "origin", "count", "linkKinds", "linkCounts", "corePreset" }) AssertSame(want[k], got[k], "duel." + k);
            var wn = Obj(want["nodes"]);
            var gn = Obj(got["nodes"]);
            foreach (string k in new[] { "y", "tag", "col" }) AssertSame(wn[k], gn[k], "duel.nodes." + k);
            var wl = Obj(want["links"]);
            var gl = Obj(got["links"]);
            foreach (string k in new[] { "linkStart", "linkTo", "linkKind", "linkCost" }) AssertSame(wl[k], gl[k], "duel.links." + k);
        }

        /// <summary>The grid the builder returns loads where the game reads it (NavField.cs NavGrid.FromExport).</summary>
        [Test]
        public void TheBuiltGrid_LoadsIntoNavGrid()
        {
            NavGrid g = NavGrid.FromExport(NavGridOracle.Built("survival").navGrid);
            var want = Obj(NavGridOracle.Map("survival")["navGrid"]);
            Assert.That(g.count, Is.EqualTo(11228));
            Assert.That(g.linkTo.Length, Is.EqualTo(Arr(Obj(want["links"])["linkTo"]).Count));
            Assert.That(g.colStart[g.size * g.size], Is.EqualTo(g.count));
        }

        // --- zones (zoneMath.js resolveZones) ----------------------------------------------------------------------

        [Test]
        public void ResolveZones_EqualsSurvivalsZoneParts()
        {
            var json = NavGridOracle.Map("survival");
            var data = Obj(json["data"]);
            var zones = new List<ZoneSource>();
            foreach (object z in Arr(data["zones"]))
            {
                var d = Obj(z);
                var src = new ZoneSource { id = (string)d["id"], priority = Num(d["priority"]) };
                foreach (object b in Arr(d["boxes"])) src.boxes.Add(NavGridOracle.Box(b));
                zones.Add(src);
            }
            var holes = Arr(data["zoneHoles"]).ConvertAll(NavGridOracle.Box);
            List<ZonePart> parts = ZoneMath.ResolveZones(zones, holes);
            var want = Arr(Obj(json["zoneParts"])["parts"]);
            Assert.That(want.Count, Is.EqualTo(114), "the oracle file");
            Assert.That(parts.Count, Is.EqualTo(want.Count), "part count");
            var bad = new List<string>();
            for (int i = 0; i < want.Count; i++)
            {
                var w = Obj(want[i]);
                ZonePart p = parts[i];
                Vec3 wmin = V3(w["min"]), wmax = V3(w["max"]);
                if ((int)Num(w["zone"]) != p.zone || !Bits(wmin, p.min) || !Bits(wmax, p.max))
                    bad.Add($"part {i}: expected zone {Num(w["zone"])} {wmin}..{wmax} got zone {p.zone} {p.min}..{p.max}");
            }
            Assert.That(bad, Is.Empty, string.Join(" | ", bad.Take(5)));
        }

        static bool Bits(Vec3 a, Vec3 b) => GoldenResult.Bits(a.x, b.x) && GoldenResult.Bits(a.y, b.y) && GoldenResult.Bits(a.z, b.z);

        [Test]
        public void ExportFor_Survival_ZonePartsEqualTheExport()
        {
            var got = NavGridOracle.Built("survival").zoneParts;
            Assert.That(got, Is.Not.Null);
            AssertSame(NavGridOracle.Map("survival")["zoneParts"], got, "survival.zoneParts");
        }

        [Test]
        public void ExportFor_MapsWithoutZones_HaveNoZoneParts_AndEveryTagIsMinusOne()
        {
            foreach (string name in new[] { "spire", "training" })
            {
                Assert.That(NavGridOracle.Map(name)["zoneParts"], Is.Null, name + ": the oracle file");
                var b = NavGridOracle.Built(name);
                Assert.That(b.zoneParts, Is.Null, name);
                Assert.That(Arr(Obj(b.navGrid["nodes"])["tag"]).All(t => Num(t) == -1), Is.True, name + " tags");
            }
        }
    }
}
