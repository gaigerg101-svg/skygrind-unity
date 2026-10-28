// The oracle's checks, in Core (NIGHT9 U3-04 / U3-07): every golden vector set in PortExport/vectors/ and every
// recorded trajectory, evaluated here so the EditMode tests, the Player's -skygrindGolden and a plain dotnet harness
// run the same code against the same files (PORT-RULES.md section 9: only V8 makes vectors, C# only compares).
// A set's checker walks its file's records, calls the C# port and compares every number bit for bit (NaN equals
// NaN; -0 and +0 differ). Checkers register through the partial Add* methods below, one per file of checkers, so new
// sets are added without editing this file's registry.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Skygrind.Core
{
    /// <summary>A PortExport folder (the repo's in the Editor and tests, StreamingAssets' in a Player), each file read once.</summary>
    public sealed class GoldenData
    {
        public readonly string root;
        readonly Dictionary<string, object> cache = new Dictionary<string, object>();
        MovementConfig movement;
        GameConfig game;

        public GoldenData(string root) => this.root = root;

        public object Json(string rel)
        {
            if (!cache.TryGetValue(rel, out object v)) cache[rel] = v = PortData.LoadJson(Path.Combine(root, rel));
            return v;
        }

        public Dictionary<string, object> Vectors(string set) => PortData.Obj(Json($"vectors/{set}.json"));

        /// <summary>Every record of a set, in file order.</summary>
        public List<Dictionary<string, object>> Cases(string set) => PortData.Arr(Vectors(set)["cases"]).ConvertAll(PortData.Obj);

        /// <summary>The recorded calls of one function.</summary>
        public List<Dictionary<string, object>> Cases(string set, string fn) =>
            Cases(set).FindAll(d => d.TryGetValue("fn", out object f) && (string)f == fn);

        /// <summary>The free-form records with this key (a "sequence", "session" or "constant" name).</summary>
        public List<Dictionary<string, object>> Records(string set, string key, string name) =>
            Cases(set).FindAll(d => d.TryGetValue(key, out object f) && f is string s && s == name);

        public Dictionary<string, object> ConfigJson => PortData.Obj(Json("config.json"));

        /// <summary>The movement sections of config.json (exact).</summary>
        public MovementConfig Movement => movement ??= PortData.LoadConfig(ConfigJson);

        /// <summary>config.json behind dotted paths, without the Unity design config (vectors are the web's game).</summary>
        public GameConfig Config => game ??= new GameConfig(ConfigJson);

        /// <summary>A recorded {"$config": "dotted.path"} argument, resolved in config.json ("" is the whole config).</summary>
        public object ConfigRef(object o)
        {
            string path = (string)PortData.Obj(o)["$config"];
            return path == "" ? ConfigJson : Config.Get(path);
        }

        public string[] VectorSets() => Names("vectors");

        public string[] TrajectoryNames() => Names("trajectories");

        string[] Names(string folder)
        {
            string dir = Path.Combine(root, folder);
            if (!Directory.Exists(dir)) return new string[0];
            var names = Directory.GetFiles(dir, "*.json").Select(Path.GetFileNameWithoutExtension).ToList();
            names.Sort(StringComparer.Ordinal);
            return names.ToArray();
        }
    }

    /// <summary>The outcome of one set (or one function of a set): how many records were compared and each mismatch.</summary>
    public sealed class GoldenResult
    {
        public readonly string name;
        public int cases;
        public readonly List<string> mismatches = new List<string>();
        /// <summary>The record being compared (prefixed to each mismatch).</summary>
        public string where = "";

        public GoldenResult(string name) => this.name = name;

        /// <summary>At least one record compared and none differed.</summary>
        public bool Ok => cases > 0 && mismatches.Count == 0;

        public string Summary => mismatches.Count == 0
            ? $"{name}: {cases} records, 0 mismatches"
            : $"{name}: {cases} records, {mismatches.Count} mismatches; first: {mismatches[0]}";

        public void Fail(string what)
        {
            if (mismatches.Count < 200) mismatches.Add(where == "" ? what : $"{where}: {what}");
            else if (mismatches.Count == 200) mismatches.Add("... (more)");
        }

        /// <summary>Bit for bit (NaN equals NaN; -0 and +0 differ).</summary>
        public static bool Bits(double a, double b) =>
            BitConverter.DoubleToInt64Bits(a) == BitConverter.DoubleToInt64Bits(b) || (double.IsNaN(a) && double.IsNaN(b));

        public bool Same(object want, double got, string what)
        {
            double w = PortData.Num(want);
            if (Bits(w, got)) return true;
            Fail($"{what}: expected {Fmt(w)} got {Fmt(got)} (Δ {Fmt(got - w)})");
            return false;
        }

        public bool SameBool(object want, bool got, string what)
        {
            if (want is bool b && b == got) return true;
            Fail($"{what}: expected {PortData.Describe(want)} got {(got ? "true" : "false")}");
            return false;
        }

        public bool SameStr(object want, string got, string what)
        {
            if ((want as string) == got && (want == null) == (got == null)) return true;
            Fail($"{what}: expected {PortData.Describe(want)} got {PortData.Describe(got)}");
            return false;
        }

        public bool SameVec(object want, Vec3 got, string what)
        {
            var w = PortData.Obj(want);
            bool a = Same(w["x"], got.x, what + ".x");
            bool b = Same(w["y"], got.y, what + ".y");
            bool c = Same(w["z"], got.z, what + ".z");
            return a && b && c;
        }

        /// <summary>
        /// Deep comparison of a recorded value with a C# value built as JSON-like data (double / int / bool / string /
        /// null / Golden.Undefined / IDictionary&lt;string, object&gt; / IList). Numbers bit for bit; object keys must
        /// match exactly (the order too when `ordered`), arrays element by element.
        /// </summary>
        public bool SameJson(object want, object got, string what, bool ordered = false)
        {
            int before = mismatches.Count;
            Deep(want, got, what, ordered);
            return mismatches.Count == before;
        }

        void Deep(object want, object got, string what, bool ordered)
        {
            if (want is Dictionary<string, object> wd && wd.ContainsKey("$num")) want = PortData.Num(want);
            if (want is Dictionary<string, object> wu && wu.ContainsKey("$undefined"))
            {
                if (!ReferenceEquals(got, Golden.Undefined)) Fail($"{what}: expected undefined got {PortData.Describe(got)}");
                return;
            }
            if (got is int gi) got = (double)gi;
            else if (got is long gl) got = (double)gl;
            else if (got is float gf) got = (double)gf;
            else if (got is Vec3 gv) got = new Dictionary<string, object> { ["x"] = gv.x, ["y"] = gv.y, ["z"] = gv.z };
            switch (want)
            {
                case null:
                    if (got != null) Fail($"{what}: expected null got {PortData.Describe(got)}");
                    return;
                case double w:
                    if (!(got is double g)) Fail($"{what}: expected {Fmt(w)} got {PortData.Describe(got)}");
                    else if (!Bits(w, g)) Fail($"{what}: expected {Fmt(w)} got {Fmt(g)} (Δ {Fmt(g - w)})");
                    return;
                case bool wb:
                    if (!(got is bool gb) || gb != wb) Fail($"{what}: expected {PortData.Describe(want)} got {PortData.Describe(got)}");
                    return;
                case string ws:
                    if (!(got is string gs) || gs != ws) Fail($"{what}: expected {PortData.Describe(want)} got {PortData.Describe(got)}");
                    return;
                case List<object> wl:
                {
                    if (!(got is System.Collections.IList gl2))
                    {
                        Fail($"{what}: expected an array got {PortData.Describe(got)}");
                        return;
                    }
                    if (gl2.Count != wl.Count) Fail($"{what}: expected {wl.Count} items got {gl2.Count}");
                    for (int i = 0; i < Math.Min(wl.Count, gl2.Count); i++) Deep(wl[i], gl2[i], $"{what}[{i}]", ordered);
                    return;
                }
                case Dictionary<string, object> wo:
                {
                    if (!(got is IDictionary<string, object> go))
                    {
                        Fail($"{what}: expected an object got {PortData.Describe(got)}");
                        return;
                    }
                    var wk = wo.Keys.ToList();
                    var gk = go.Keys.ToList();
                    if (ordered ? !wk.SequenceEqual(gk) : wk.Count != gk.Count || wk.Except(gk).Any())
                        Fail($"{what}: keys [{string.Join(",", wk)}] vs [{string.Join(",", gk)}]");
                    foreach (string k in wk)
                        if (go.TryGetValue(k, out object gvv)) Deep(wo[k], gvv, $"{what}.{k}", ordered);
                    return;
                }
            }
            Fail($"{what}: unsupported recorded value {PortData.Describe(want)}");
        }

        public static string Fmt(double d) => d.ToString("R", CultureInfo.InvariantCulture);
    }

    public delegate void GoldenCheck(GoldenData data, GoldenResult result);

    /// <summary>The registry of vector-set checkers and the trajectory replays.</summary>
    public static partial class Golden
    {
        /// <summary>The C# value standing for a recorded {"$undefined": true}.</summary>
        public static readonly object Undefined = new object();

        static Dictionary<string, GoldenCheck> sets;

        /// <summary>vector file name (without .json) -> its checker.</summary>
        public static Dictionary<string, GoldenCheck> Sets
        {
            get
            {
                if (sets != null) return sets;
                var d = new Dictionary<string, GoldenCheck>();
                AddMovement(d);
                AddCombat(d);
                AddStyle(d);
                AddAi(d);
                AddPorted(d);
                AddNewSets(d);
                AddJsMath(d);
                AddStyleCore(d);
                AddProjectiles(d);
                AddRunShape(d); // NIGHT11: roundType, runMoments, the objectives and traps sessions (GoldenRunShape.cs)
                AddBot(d); // NIGHT14: astar, reach, navGraph, botBrain, follower (GoldenBot.cs)
                return sets = d;
            }
        }

        // One per file of checkers (Golden*.cs); an unimplemented one is simply not called.
        static partial void AddMovement(Dictionary<string, GoldenCheck> d);
        static partial void AddCombat(Dictionary<string, GoldenCheck> d);
        static partial void AddStyle(Dictionary<string, GoldenCheck> d);
        static partial void AddAi(Dictionary<string, GoldenCheck> d);
        static partial void AddPorted(Dictionary<string, GoldenCheck> d);
        static partial void AddNewSets(Dictionary<string, GoldenCheck> d);
        static partial void AddJsMath(Dictionary<string, GoldenCheck> d);
        static partial void AddStyleCore(Dictionary<string, GoldenCheck> d);
        static partial void AddProjectiles(Dictionary<string, GoldenCheck> d);
        static partial void AddRunShape(Dictionary<string, GoldenCheck> d);
        static partial void AddBot(Dictionary<string, GoldenCheck> d);

        /// <summary>Check one vector set; an exception inside a checker is a mismatch, not a crash.</summary>
        public static GoldenResult Check(GoldenData data, string set)
        {
            var r = new GoldenResult(set);
            if (!Sets.TryGetValue(set, out GoldenCheck check))
            {
                r.Fail($"no checker for vectors/{set}.json");
                return r;
            }
            try
            {
                check(data, r);
            }
            catch (Exception e)
            {
                r.where = "";
                r.Fail($"exception: {e.GetType().Name}: {e.Message}");
            }
            return r;
        }

        /// <summary>The vector files with no checker (the orphan test fails on any).</summary>
        public static List<string> Orphans(GoldenData data) => data.VectorSets().Where(s => !Sets.ContainsKey(s)).ToList();

        // --- trajectories -------------------------------------------------------------------------------------------

        public static Trajectory LoadTrajectory(GoldenData data, string name) => Trajectory.Load(Path.Combine(data.root, "trajectories", name + ".json"));

        /// <summary>
        /// The C# version of the collider a trajectory ran against: boxes, or a map file's static octree. A trial route
        /// (scene.core set) ran on arena.js's whole world collider: the octree, then the core grid at that preset.
        /// </summary>
        public static ICollisionWorld ReferenceWorld(GoldenData data, Trajectory t)
        {
            if (t.SceneKind == "boxes") return ThreeGeometry.BuildOctree(PortData.Arr(t.scene["boxes"]).ConvertAll(PortData.Box), null);
            var map = PortData.Obj(data.Json($"maps/{t.scene["map"]}.json"));
            ThreeOctree octree = PortData.BuildMapOctree(map);
            if (!(t.scene.TryGetValue("core", out object preset) && preset is string name) || !(map["core"] is Dictionary<string, object> core)) return octree;
            var s = PortData.Obj(core["spec"]);
            var spec = new CoreSpec { x0 = PortData.Num(s["x0"]), z0 = PortData.Num(s["z0"]), cells = (int)PortData.Num(s["cells"]), cellSize = PortData.Num(s["cellSize"]) };
            MovementConfig m = data.Movement;
            var grid = new CoreGrid(m, spec);
            grid.SetHeights(CoreGrid.PresetHeights(name, spec.cells, m.core.maxHeight, m.core.heightStep, new Mulberry32(1)));
            return new LayeredWorld(octree, grid);
        }

        /// <summary>A trial route's map pads (scene.pads): arena.js checkPads after every Mover step.</summary>
        static Action<Mover> AfterStep(GoldenData data, Trajectory t)
        {
            if (!(t.scene.TryGetValue("pads", out object p) && p is bool on && on)) return null;
            var arena = new ArenaMap();
            foreach (object o in PortData.Arr(PortData.Obj(data.Json($"maps/{t.scene["map"]}.json"))["pads"]))
            {
                var d = PortData.Obj(o);
                var pad = new PadSpec { pos = new Vec3(PortData.Num(d["x"]), PortData.Num(d["y"]), PortData.Num(d["z"])) };
                if (d.TryGetValue("target", out object target) && target != null)
                {
                    pad.aimed = true;
                    pad.target = PortData.V3(target);
                    pad.apexAbove = PortData.Num(d["apexAbove"]);
                }
                arena.pads.Add(pad);
            }
            MovementConfig m = data.Movement;
            return mover => arena.CheckPads(mover, m);
        }

        /// <summary>Replay a recorded trajectory through the C# Mover on its reference world (and its pads).</summary>
        public static ReplayReport Replay(GoldenData data, string name)
        {
            Trajectory t = LoadTrajectory(data, name);
            return TrajectoryReplay.Run(t, ReferenceWorld(data, t), data.Movement, AfterStep(data, t));
        }

        /// <summary>Every set and every replay, one line each, then a total line (the Player's -skygrindGolden).</summary>
        public static int RunAll(GoldenData data, Action<string> log)
        {
            int bad = 0, records = 0;
            var setNames = data.VectorSets();
            foreach (string set in setNames)
            {
                GoldenResult r = Check(data, set);
                records += r.cases;
                if (!r.Ok) bad++;
                log($"golden {(r.Ok ? "PASS" : "FAIL")} {r.Summary}");
                for (int i = 1; i < Math.Min(r.mismatches.Count, 6); i++) log($"    {r.mismatches[i]}");
            }
            var names = data.TrajectoryNames();
            int exact = 0;
            foreach (string name in names)
            {
                ReplayReport rep;
                try
                {
                    rep = Replay(data, name);
                }
                catch (Exception e)
                {
                    log($"replay FAIL {name}: exception {e.GetType().Name}: {e.Message}");
                    bad++;
                    continue;
                }
                if (rep.Exact) exact++;
                else bad++;
                log(rep.Exact ? $"replay PASS {name}: {rep.steps} steps identical" : $"replay FAIL {name}: step {rep.firstDivergentStep} of {rep.steps}: {rep.firstDifference}");
            }
            log($"golden total: {setNames.Length} vector sets ({records} records), {names.Length} replays ({exact} exact); {bad} failing, {(bad == 0 ? "0 mismatches" : "MISMATCHES")}");
            return bad;
        }

        /// <summary>A compact description of a recorded value, for mismatch lines.</summary>
        public static string Describe(object o)
        {
            string s = PortData.Describe(o);
            return s.Length > 160 ? s.Substring(0, 160) + "..." : s;
        }
    }
}
