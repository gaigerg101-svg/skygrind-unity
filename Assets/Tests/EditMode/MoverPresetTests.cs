// The web game's movement regression presets (scripts.js CHECKS) on the ported Mover with the reference collision
// world (the three.js per-triangle test). Passing here means the C# Mover reproduces the web's results on the same
// training map; the Unity-physics runs of the same presets are in Skygrind.Game.Tests (CollisionParityTests).
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class MoverPresetTests
    {
        static IEnumerable<string> Presets() => MovementPresets.All.Select(p => p.name);

        [TestCaseSource(nameof(Presets))]
        public void ReferenceWorld_PassesPreset(string name)
        {
            var preset = MovementPresets.All.First(p => p.name == name);
            ArenaMap map = TrainingMap.Build();
            var config = new MovementConfig();
            LayeredWorld world = map.BuildReferenceWorld(config, out CoreGrid core);
            var runner = new PresetRunner(config, map, world, core);
            PresetOutcome outcome = preset.run(runner);
            TestContext.WriteLine(outcome.summary);
            Assert.IsTrue(outcome.pass, outcome.summary);
        }
    }

    public class TriangleWorldTests
    {
        static readonly Vec3 A = new Vec3(-10, 0, -10), B = new Vec3(10, 0, -10), C = new Vec3(0, 0, 10);

        [Test, Description("a capsule 5 cm into a floor triangle: face contact, normal up, depth 0.05, point on the axis")]
        public void FloorFaceContact()
        {
            var cap = new Capsule { start = new Vec3(0, 0.30, 0), end = new Vec3(0, 1.45, 0), radius = 0.35 };
            var w = new TriangleWorld();
            w.AddOutward(A, B, C, new Vec3(0, -1, 0));
            Assert.AreEqual(1, w.GatherCandidates(cap));
            Assert.IsTrue(w.CapsuleIntersect(cap, 0, out CapsuleContact c));
            Expect.CloseTo(c.normal.y, 1, 10);
            Expect.CloseTo(c.depth, 0.05, 10);
            // three.js: d1 = −0.05, d2 = 1.10, delta = 0.05 / 1.15 along the axis -> y = 0.35
            Expect.CloseTo(c.point.y, 0.35, 10);
        }

        [Test, Description("a capsule overhanging a box's top edge: an edge contact with the point on the edge")]
        public void BoxEdgeContact()
        {
            var w = new TriangleWorld();
            w.AddBox(new Vec3(-1, 0, -1), new Vec3(1, 1, 1));
            // Lower sphere center 0.2 m past the +x edge and 0.25 m above the top: distance 0.32 < 0.35.
            var cap = new Capsule { start = new Vec3(1.2, 1.25, 0), end = new Vec3(1.2, 2.35, 0), radius = 0.35 };
            int n = w.GatherCandidates(cap);
            bool any = false;
            for (int i = 0; i < n && !any; i++)
            {
                if (!w.CapsuleIntersect(cap, i, out CapsuleContact c)) continue;
                any = true;
                Expect.CloseTo(c.point.x, 1, 10);
                Expect.CloseTo(c.point.y, 1, 10);
                Expect.CloseTo(c.depth, 0.35 - System.Math.Sqrt(0.2 * 0.2 + 0.25 * 0.25), 10);
                Expect.CloseTo(c.normal.x, 0.2 / System.Math.Sqrt(0.1025), 10);
            }
            Assert.IsTrue(any);
        }
    }
}
