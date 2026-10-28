// The 16 PortExport trajectories replayed on Unity physics (Physics.ComputePenetration + Raycast through
// UnityPhysicsWorld), with two collider builds: one MeshCollider per web collision triangle (the faces three's Octree
// drops are left out), and one BoxCollider per box. Report-only: writes Logs/replay-unity-<build>.md with pass / fail,
// the first divergent step and the size of the difference per trajectory. The same replays on the C# port of the
// web's own collision world must be bit exact (Skygrind.Core.Tests PortExportTests); everything that differs here is
// down to the Unity queries.
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using static Skygrind.Core.PortData;

namespace Skygrind.Game.Tests
{
    public class UnityReplayTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static readonly string[] Order =
        {
            "walk", "walk_diagonal", "run_jump_double", "short_hop", "bunny_hop", "slide", "slide_jump", "dash", "dash_jump",
            "slam_boost", "wall_ride", "wall_jump", "swing", "swing_from_ground", "ramp_walk_up", "ramp_slide_down",
        };

        GameObject probe;
        UnityPhysicsWorld world;
        MovementConfig config;

        [OneTimeSetUp]
        public void SetUp()
        {
            probe = new GameObject("Mover query capsule (replay)") { layer = 2 };
            probe.transform.position = new Vector3(0, -1000, 0);
            world = new UnityPhysicsWorld(probe.AddComponent<CapsuleCollider>());
            config = LoadConfig(Obj(LoadJson(Path.Combine(Root, "config.json"))));
        }

        [OneTimeTearDown]
        public void TearDown() => Object.DestroyImmediate(probe);

        static IEnumerable<string> Builds() => new[] { "triangles", "boxes" };

        [TestCaseSource(nameof(Builds))]
        public void ReplayAll_OnUnityPhysics(string build)
        {
            var lines = new List<string>();
            foreach (string name in Order)
            {
                Trajectory t = Trajectory.Load(Path.Combine(Root, "trajectories", name + ".json"));
                GameObject colliders = Build(t, build);
                world.gathers = world.penetrationTests = world.contacts = world.pointFallbacks = world.overflow = 0;
                ReplayReport r;
                try
                {
                    r = TrajectoryReplay.Run(t, world, config);
                }
                finally
                {
                    Object.DestroyImmediate(colliders);
                }
                string stats = string.Format(CultureInfo.InvariantCulture, " [Unity: {0:0.0} contacts/step, {1} with no Unity contact point, {2} candidate overflows]",
                    (double)world.contacts / t.rows.Count, world.pointFallbacks, world.overflow);
                lines.Add(Summary(r) + stats);
            }
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Logs", $"replay-unity-{build}.md"));
            File.WriteAllLines(path, lines, Encoding.UTF8);
            Debug.Log($"[replay] Unity physics ({build}): {lines.Count(l => l.Contains(": PASS"))}/{lines.Count} exact\n" + string.Join("\n", lines));
            TestContext.WriteLine(string.Join("\n", lines));
        }

        static GameObject Build(Trajectory t, string build)
        {
            List<BoxSpec> boxes;
            List<WedgeSpec> wedges = null;
            if (t.SceneKind == "boxes") boxes = Arr(t.scene["boxes"]).ConvertAll(Box);
            else
            {
                var map = Obj(LoadJson(Path.Combine(Root, "maps", $"{t.scene["map"]}.json")));
                var c = Obj(map["collision"]);
                boxes = Arr(c["boxes"]).ConvertAll(Box);
                boxes.AddRange(Arr(c["barriers"]).ConvertAll(Box));
                wedges = Arr(c["wedges"]).ConvertAll(Wedge);
            }
            if (build == "boxes") return UnityColliders.FromBoxes(boxes, wedges, $"{t.name} boxes");
            ThreeOctree octree = ThreeGeometry.BuildOctree(boxes, wedges);
            return UnityColliders.FromTriangles(octree.StoredTriangles(), $"{t.name} triangles");
        }

        static string Summary(ReplayReport r) =>
            r.Exact
                ? $"{r.name}: PASS, {r.steps} steps identical"
                : string.Format(CultureInfo.InvariantCulture,
                    "{0}: FAIL (not bit exact) at step {1} of {2}: {3} | size {4:G3}; max over the run: position {5:G3} m, velocity {6:G3} m/s | BEHAVIOUR {7}",
                    r.name, r.firstDivergentStep, r.steps, Trim(r.firstDifference), r.divergenceAtFirst, r.maxPositionDiff, r.maxVelocityDiff,
                    r.firstBehaviourStep < 0
                        ? "same (every step: same state, contacts, event types and reasons)"
                        : $"differs first at step {r.firstBehaviourStep}: {Trim(r.firstBehaviour)}; steps with a state difference {r.stepsWithStateDiff}, with an event type/reason difference {r.stepsWithEventTypeDiff}");

        static string Trim(string s) => s.Length > 300 ? s.Substring(0, 300) + "..." : s;
    }
}
