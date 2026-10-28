// Where Unity's collision queries can't reproduce mover.js: the same presets on the same map, once on the web game's
// own collider (the C# port of three's Octree + the core grid, bit-exact against the web's recorded runs) and once on
// Unity physics (one MeshCollider per Octree triangle + the Grinder's BoxColliders, through ComputePenetration).
// Writes Logs/collision-parity.md (pass/fail per world, how far the two trajectories drift apart, the first step their
// states differ, query cost) for SPIKE-RESULTS.md. Report-only for the Unity world; the reference must pass the web
// presets (MoverPresetTests asserts that per preset).
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Skygrind.Game.Tests
{
    public class CollisionParityTests
    {
        ArenaMap map;
        GameObject arena;
        GameObject probe;
        UnityPhysicsWorld unity;

        [OneTimeSetUp]
        public void BuildArena()
        {
            map = TrainingMap.Build();
            arena = ArenaBuilder.Build(map, new Material(Shader.Find("Universal Render Pipeline/Lit")), map.BuildOctree());
            probe = new GameObject("Mover query capsule (test)") { layer = 2 };
            probe.transform.position = new Vector3(0, -1000, 0);
            unity = new UnityPhysicsWorld(probe.AddComponent<CapsuleCollider>());
            Physics.SyncTransforms();
        }

        [OneTimeTearDown]
        public void DestroyArena()
        {
            Object.DestroyImmediate(arena);
            Object.DestroyImmediate(probe);
        }

        [Test, Description("Unity physics world: a mover placed on the lane settles on the floor (the queries work in EditMode)")]
        public void UnityWorld_StandsOnFloor()
        {
            var runner = new PresetRunner(new MovementConfig(), map, unity);
            runner.Place("lane");
            Snapshot s = runner.State();
            Assert.IsTrue(s.grounded, "grounded");
            Assert.AreEqual(0, s.position.y, 1e-3);
        }

        [Test, Description("sim cost per fixed step (mover + collision) on both worlds, after a warm-up; writes Logs/sim-cost.md")]
        public void SimCost_PerStep()
        {
            string Measure(string label, System.Func<PresetRunner> make)
            {
                var all = new System.Collections.Generic.List<double>();
                for (int round = 0; round < 6; round++)
                {
                    foreach (string name in new[] { "fullChain", "stamina", "dashAllStates", "wallJumpChain" })
                    {
                        PresetRunner r = make();
                        var preset = System.Array.Find(MovementPresets.All, p => p.name == name);
                        var sw = new Stopwatch();
                        r.mover.Events += _ => { };
                        int before = r.trace.Count;
                        sw.Start();
                        preset.run(r);
                        sw.Stop();
                        int steps = r.trace.Count - before;
                        if (round > 0 && steps > 0) all.Add(sw.Elapsed.TotalMilliseconds * 1000 / steps); // round 0: JIT warm-up
                    }
                }
                all.Sort();
                double mean = 0;
                foreach (double v in all) mean += v;
                mean /= all.Count;
                return string.Format(CultureInfo.InvariantCulture, "{0}: {1:0.0} us per step (mean of {2} preset runs; fastest {3:0.0}, slowest {4:0.0})",
                    label, mean, all.Count, all[0], all[all.Count - 1]);
            }
            string web = Measure("web collider (C# three.js Octree)", () =>
            {
                var c = new MovementConfig();
                return new PresetRunner(c, map, map.BuildReferenceWorld(c, out CoreGrid g), g);
            });
            string unityLine = Measure("Unity physics (per-triangle MeshColliders)", () => new PresetRunner(new MovementConfig(), map, unity));
            string text = "Sim cost per 120 Hz step in the Editor (Mono JIT, EditMode), presets fullChain / stamina / dashAllStates / wallJumpChain, including the preset's own bookkeeping:\n- " + web + "\n- " + unityLine + "\n";
            File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "sim-cost.md")), text);
            TestContext.WriteLine(text);
        }

        [Test, Description("every preset on both worlds; writes Logs/collision-parity.md")]
        public void ParityReport()
        {
            var md = new StringBuilder();
            md.AppendLine("| preset | web collider (C# three.js Octree) | Unity physics | max divergence (m) | first state difference (step) | steps ref / unity | Unity queries/step | Unity ms / ref ms |");
            md.AppendLine("|---|---|---|---|---|---|---|---|");
            int refFails = 0;
            foreach (var preset in MovementPresets.All)
            {
                var refConfig = new MovementConfig();
                var reference = new PresetRunner(refConfig, map, map.BuildReferenceWorld(refConfig, out CoreGrid refCore), refCore);
                var unityConfig = new MovementConfig();
                CoreGrid unityCore = map.CreateCore(unityConfig);
                var coreView = new CoreView(unityCore, unityConfig, arena.transform, null);
                var physics = new PresetRunner(unityConfig, map, unity, unityCore) { afterCoreStep = coreView.Sync };
                var sw = Stopwatch.StartNew();
                PresetOutcome a = preset.run(reference);
                double refMs = sw.Elapsed.TotalMilliseconds;
                unity.gathers = unity.penetrationTests = unity.contacts = unity.pointFallbacks = 0;
                sw.Restart();
                PresetOutcome b;
                try
                {
                    b = preset.run(physics);
                }
                finally
                {
                    Object.DestroyImmediate(coreView.Root); // the next preset gets a fresh, flat core
                }
                double unityMs = sw.Elapsed.TotalMilliseconds;
                int steps = physics.trace.Count;
                double queriesPerStep = steps > 0 ? (double)(unity.gathers + unity.penetrationTests) / steps : 0;
                if (preset.web && !a.pass) refFails++;

                int n = System.Math.Min(reference.trace.Count, physics.trace.Count);
                double maxDev = 0;
                int firstState = -1;
                for (int i = 0; i < n; i++)
                {
                    maxDev = System.Math.Max(maxDev, (reference.trace[i] - physics.trace[i]).Length);
                    if (firstState < 0 && reference.stateTrace[i] != physics.stateTrace[i]) firstState = i;
                }
                string Cell(PresetOutcome o) => (o.pass ? "PASS: " : "**FAIL**: ") + o.summary.Replace("|", "/");
                md.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "| {0} | {1} | {2} | {3:0.0000} | {4} | {5} / {6} | {7:0.0} | {8:0.0} / {9:0.0} |",
                    preset.name, Cell(a), Cell(b), maxDev, firstState < 0 ? "none" : firstState.ToString(), reference.trace.Count, physics.trace.Count,
                    queriesPerStep, unityMs, refMs));
                if (unity.pointFallbacks > 0) md.AppendLine($"|   ↳ {preset.name}: {unity.pointFallbacks} of {unity.contacts} Unity contacts had no contact point (triangle MeshColliders): placed from the MTD plane | | | | | | | |");
            }
            string report = md.ToString();
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", "collision-parity.md"));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, report);
            Debug.Log("[parity] wrote " + path + "\n" + report);
            Assert.AreEqual(0, refFails, "reference world failed web presets (see MoverPresetTests)");
        }
    }
}
