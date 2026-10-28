// Golden vectors (PortExport/vectors/movementMath.json, swingMath.json): every recorded call of the web game's pure
// movement modules replayed through the C# port, compared bit for bit (NaN equals NaN; -0 and +0 differ). One test per
// record key (a function, a sequence, a constant). The evaluation is Skygrind.Core's GoldenMovement (NIGHT9 U3-04), the
// code the Player's -skygrindGolden runs too; a failure lists each mismatching record with its first differing field.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using static Skygrind.Core.PortData;

namespace Skygrind.Core.Tests
{
    public static class Port
    {
        public static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static readonly Dictionary<string, object> cache = new Dictionary<string, object>();

        public static object Json(string rel)
        {
            if (!cache.TryGetValue(rel, out object v)) cache[rel] = v = LoadJson(Path.Combine(Root, rel));
            return v;
        }

        static MovementConfig config;

        /// <summary>The game config, read from config.json (exact).</summary>
        public static MovementConfig Config => config ?? (config = LoadConfig(Obj(Json("config.json"))));

        /// <summary>The stored result of a recorded call with exactly these arguments (numbers compared bit for bit).</summary>
        public static object Result(string module, string fn, params double[] args)
        {
            foreach (object c in Arr(Obj(Json($"vectors/{module}.json"))["cases"]))
            {
                var d = Obj(c);
                if (!d.TryGetValue("fn", out object name) || (string)name != fn) continue;
                var a = Arr(d["args"]);
                if (a.Count != args.Length) continue;
                bool same = true;
                for (int i = 0; i < a.Count && same; i++) same = a[i] is double x && BitConverter.DoubleToInt64Bits(x) == BitConverter.DoubleToInt64Bits(args[i]);
                if (same) return d["result"];
            }
            throw new KeyNotFoundException($"no {module}.{fn}({string.Join(", ", args)}) vector");
        }
    }

    /// <summary>The golden checks the tests share with the Player (Skygrind.Core's Golden*.cs) over one read of Port.Root.</summary>
    static class PortGolden
    {
        public static GoldenData Data => OracleGoldenTests.Data; // one instance: each vector file parsed once per test run

        /// <summary>Run one part of a set's checker, print its summary, fail when it compared nothing or anything differed.</summary>
        public static GoldenResult Check(string name, Action<GoldenData, GoldenResult> part)
        {
            var r = new GoldenResult(name);
            part(Data, r);
            TestContext.WriteLine(r.Summary);
            Assert.IsTrue(r.Ok, r.Summary + "\n" + string.Join("\n", r.mismatches));
            return r;
        }
    }

    public class GoldenVectorTests
    {
        static MovementConfig Config => Port.Config;

        static IEnumerable<string> Functions(string module) =>
            Arr(Obj(Port.Json($"vectors/{module}.json"))["cases"]).Select(c =>
            {
                var d = Obj(c);
                return d.TryGetValue("fn", out object f) ? (string)f : d.TryGetValue("sequence", out object s) ? "sequence:" + s : "constant:" + d["constant"];
            }).Distinct();

        static IEnumerable<string> MovementFunctions() => Functions("movementMath");

        static IEnumerable<string> SwingFunctions() => Functions("swingMath");

        [TestCaseSource(nameof(MovementFunctions))]
        public void MovementMath(string fn) => Check("movementMath", fn);

        [TestCaseSource(nameof(SwingFunctions))]
        public void SwingMath(string fn) => Check("swingMath", fn);

        [Test, Description("spawnMath.json mulberry32: the first 16 draws for every recorded seed, bit for bit")]
        public void SpawnMath_Mulberry32()
        {
            var cases = Arr(Obj(Port.Json("vectors/spawnMath.json"))["cases"]).Select(Obj).Where(d => d.TryGetValue("fn", out object f) && (string)f == "mulberry32").ToList();
            var failures = new List<string>();
            foreach (var c in cases)
            {
                var rng = new Mulberry32(Num(c["seed"]));
                var draws = Arr(c["first16"]);
                var diffs = new List<string>();
                for (int i = 0; i < draws.Count && diffs.Count == 0; i++) Same(draws[i], rng.Next(), $"draw {i + 1}", diffs);
                if (diffs.Count > 0) failures.Add($"seed {Describe(c["seed"])}: {diffs[0]}");
            }
            TestContext.WriteLine($"mulberry32: {cases.Count} seeds x 16 draws checked, {failures.Count} mismatching");
            Assert.Greater(cases.Count, 0);
            Assert.IsEmpty(failures, string.Join("\n", failures));
        }

        static IEnumerable<string> CoreMaps() => new[] { "spire", "survival" };

        [TestCaseSource(nameof(CoreMaps)), Description("maps/<map>.json core.presets: presetHeights for every preset (random from mulberry32(1)), bit for bit")]
        public void CoreGrid_PresetHeights(string map)
        {
            var core = Obj(Obj(Port.Json($"maps/{map}.json"))["core"]);
            int cells = (int)Num(Obj(core["spec"])["cells"]);
            var failures = new List<string>();
            foreach (var kv in Obj(core["presets"]))
            {
                double[] ours = CoreGrid.PresetHeights(kv.Key, cells, Config.core.maxHeight, Config.core.heightStep, new Mulberry32(1));
                var want = Arr(kv.Value);
                var diffs = new List<string>();
                if (want.Count != ours.Length) diffs.Add($"count {want.Count} vs {ours.Length}");
                for (int i = 0; i < want.Count && diffs.Count == 0; i++) Same(want[i], ours[i], $"cell {i}", diffs);
                if (diffs.Count > 0) failures.Add($"{kv.Key}: {diffs[0]}");
            }
            Assert.IsEmpty(failures, string.Join("\n", failures));
        }

        /// <summary>One record key of a set (a function's calls, a sequence or a constant) through the Core checker.</summary>
        static void Check(string module, string fn) => PortGolden.Check($"{module}.{fn}", (d, r) => GoldenMovement.CheckFn(d, r, module, fn));

        // --- comparisons (bitwise; SpawnMath_Mulberry32 and CoreGrid_PresetHeights) ---------------------------------

        static void Same(object want, double got, string what, List<string> diffs)
        {
            double w = Num(want);
            bool same = BitConverter.DoubleToInt64Bits(w) == BitConverter.DoubleToInt64Bits(got) || (double.IsNaN(w) && double.IsNaN(got));
            if (!same) diffs.Add($"{what}: expected {Describe(w)} got {Describe(got)} (Δ {Describe(got - w)})");
        }
    }
}
