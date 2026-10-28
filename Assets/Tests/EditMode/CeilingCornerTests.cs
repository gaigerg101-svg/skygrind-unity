// Port of skygrind/tests/movementSim.test.js "ceiling corner correction (NIGHT5 F6)": the real Mover against a box
// ceiling whose edge is at x = 0 (it covers x < 0, its underside 2.6 m up). A standing jump whose head's rim is under
// the edge by `overlap` m. The web builds the boxes with three's BoxGeometry + Octree; here TriangleWorld.AddBox (the
// same 12 outward triangles per box). Same expectations and tolerances.
using System;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class CeilingCornerTests
    {
        static readonly MovementConfig config = new MovementConfig();
        static readonly double DT = 1 / config.loop.simHz;
        static readonly double R = config.body.capsuleRadius;

        static TriangleWorld World(bool ceiling)
        {
            var w = new TriangleWorld();
            w.AddBox(new Vec3(-20, -1, -20), new Vec3(20, 0, 20)); // BoxGeometry(40, 1, 40) at y -0.5
            if (ceiling) w.AddBox(new Vec3(-6, 2.6, -6), new Vec3(0, 3.0, 6)); // BoxGeometry(6, 0.4, 12) at (-3, 2.8, 0)
            return w;
        }

        /// <summary>Apex of a standing jump from x (jump held), and the largest sideways drift.</summary>
        static (double apex, double drift) Jump(TriangleWorld world, double x)
        {
            var m = new Mover(config, world);
            m.Reset(x, 0, 0);
            var f = new InputFrame();
            for (int i = 0; i < 30; i++) m.Step(f, DT);
            double apex = 0;
            double drift = 0;
            for (int i = 0; i < 200; i++)
            {
                f.jump.held = true;
                f.jump.pressed = i == 0;
                m.Step(f, DT);
                apex = Math.Max(apex, m.position.y);
                drift = Math.Max(drift, Math.Abs(m.position.x - x));
            }
            return (apex, drift);
        }

        static readonly double Open = Jump(World(false), 0.5).apex;

        [Test]
        public void EdgeWithinAQuarterRadius_SlidesPastAndKeepsTheJump()
        {
            foreach (double overlap in new[] { 0.02, 0.05, 0.08 })
            {
                var (apex, drift) = Jump(World(true), R - overlap);
                Assert.That(Math.Abs(apex - Open), Is.LessThan(0.005), $"overlap {overlap}: apex {apex} vs open {Open}"); // toBeCloseTo(open, 2)
                Assert.That(drift, Is.LessThan(overlap + 0.02), $"overlap {overlap}: drift {drift}"); // nudged just clear, not thrown
            }
        }

        [Test]
        public void DeeperUnderTheCeiling_StillBlocks()
        {
            foreach (double overlap in new[] { 0.15, 0.3, 0.6 })
                Assert.That(Jump(World(true), R - overlap).apex, Is.LessThan(Open - 0.2), $"overlap {overlap}");
        }

        [Test]
        public void AwayFromCeilings_NothingChanges()
        {
            Assert.That(Jump(World(true), 3).apex, Is.EqualTo(Open));
        }
    }
}
