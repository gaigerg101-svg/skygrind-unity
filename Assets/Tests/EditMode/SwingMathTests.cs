// Port of skygrind/tests/swingMath.test.js (OVERNIGHT2 N4: the ARCLINE swing math, pure, no collision).
// Same inputs, same expected values, same tolerances.
using System;
using NUnit.Framework;
using static Skygrind.Core.SwingMath;
using static Skygrind.Core.Tests.Expect;

namespace Skygrind.Core.Tests
{
    public class SwingMathTests
    {
        static readonly MovementConfig config = new MovementConfig();
        static readonly ArclineConfig a = config.arcline;
        static readonly double g0 = MovementMath.GravityFor(config.jump.height, config.jump.apexTime);
        static readonly double h = 1 / (config.loop.simHz * a.substeps);
        static readonly SwingInput none = new SwingInput { wish = null, reel = false, pulling = false };

        /// <summary>A body L m from the anchor at the origin, horizontal, at rest.</summary>
        static SwingState Horizontal(double L) => new SwingState { pos = new Vec3(L, 0, 0), vel = Vec3.Zero, L = L };

        [Test, Description("a pendulum released from horizontal at rest (L 20 m, no drag) reaches sqrt(2 g L) at the bottom (5%)")]
        public void Pendulum_ReachesIdealBottomSpeed()
        {
            SwingParams p = Params(a, g0);
            p.drag = 0;
            SwingState s = Horizontal(20);
            Vec3 anchor = Vec3.Zero;
            double bottom = 0;
            for (int i = 0; i < 20000 && s.pos.x > 0; i++)
            {
                Step(ref s, anchor, none, p, h);
                bottom = Math.Max(bottom, JsMath.Hypot(s.vel.x, s.vel.y, s.vel.z));
            }
            double ideal = Math.Sqrt(2 * p.g * 20);
            Assert.Less(Math.Abs(bottom - ideal) / ideal, 0.05);
        }

        [Test, Description("keeps 98% of its energy over a swing (with the real drag)")]
        public void Swing_Keeps98PercentEnergy()
        {
            SwingParams p = Params(a, g0);
            var s = new SwingState { pos = new Vec3(0, -20, 0), vel = new Vec3(20, 0, 0), L = 20 };
            Vec3 anchor = Vec3.Zero;
            double e0 = SwingEnergy(s.pos, s.vel, p.g);
            // One full period: out, back through the bottom, out the other side and back.
            double T = 2 * Math.PI * Math.Sqrt(20 / p.g) * 1.2;
            for (double t = 0; t < T; t += h) Step(ref s, anchor, none, p, h);
            double e1 = SwingEnergy(s.pos, s.vel, p.g);
            double range = e0 - p.g * -20; // the energy above the lowest point (what a swing can lose)
            Assert.Less((e0 - e1) / range, 0.02);
        }

        [Test, Description("the rope never exceeds L x 1.03 after any step, even on a hard catch")]
        public void Rope_NeverExceedsStretchLimit()
        {
            SwingParams p = Params(a, g0);
            var s = new SwingState { pos = new Vec3(3, -2, 0), vel = new Vec3(30, -25, 5), L = 10 };
            Vec3 anchor = Vec3.Zero;
            var push = new SwingInput { wish = new Vec3(1, 0, 0), reel = false, pulling = false };
            double worst = 0;
            for (int i = 0; i < 4000; i++)
            {
                Step(ref s, anchor, push, p, h);
                worst = Math.Max(worst, RopeDistance(s.pos, anchor) / s.L);
            }
            Assert.LessOrEqual(worst, 1 + a.stretch + 1e-9);
        }

        [Test, Description("release keeps the velocity (to float tolerance); rising adds 8%, a jump adds a little up")]
        public void Release_KeepsVelocity_RisingBoost_JumpUp()
        {
            var v = new Vec3(12.5, -3.25, 7);
            Assert.AreEqual(v, ReleaseVelocity(v, a.risingBoost));
            Vec3 up = ReleaseVelocity(new Vec3(10, 5, 0), a.risingBoost);
            CloseTo(up.x, 10 * a.risingBoost, 10);
            CloseTo(up.y, 5 * a.risingBoost, 10);
            CloseTo(ReleaseVelocity(new Vec3(0, -1, 0), a.risingBoost, a.jumpUp).y, -1 + a.jumpUp, 10);
        }

        [Test, Description("is deterministic: two runs match exactly")]
        public void Step_IsDeterministic()
        {
            double[] Run()
            {
                SwingParams p = Params(a, g0);
                SwingState s = Horizontal(25);
                for (int i = 0; i < 3000; i++)
                {
                    var input = new SwingInput { wish = new Vec3(0, 0, 1), reel = i > 1500, pulling = i < 20 };
                    Step(ref s, Vec3.Zero, input, p, h);
                }
                return new[] { s.pos.x, s.pos.y, s.pos.z, s.vel.x, s.vel.y, s.vel.z, s.L };
            }

            Assert.AreEqual(Run(), Run());
        }

        [Test, Description("reeling from 30 m to 10 m on a steady swing speeds you up (angular momentum)")]
        public void Reel_SpeedsUpWithAngularMomentum()
        {
            SwingParams p = Params(a, g0);
            p.g = 0; // a flat circle: only the reel changes the speed
            p.drag = 0;
            var s = new SwingState { pos = new Vec3(30, 0, 0), vel = new Vec3(0, 0, 12), L = 30 };
            Vec3 anchor = Vec3.Zero;
            for (int i = 0; i < 480; i++) Step(ref s, anchor, none, p, h); // settle on the circle
            double before = JsMath.Hypot(s.vel.x, s.vel.y, s.vel.z);
            var reel = new SwingInput { wish = null, reel = true, pulling = false };
            for (int i = 0; i < 20000 && s.L > 10.01; i++) Step(ref s, anchor, reel, p, h);
            // Stop at 10 m and let it settle.
            p.minLen = s.L;
            for (int i = 0; i < 240; i++) Step(ref s, anchor, none, p, h);
            double after = JsMath.Hypot(s.vel.x, s.vel.y, s.vel.z);
            Assert.LessOrEqual(s.L, 10.02);
            Assert.Greater(after, before * 2.5); // ideal x3 (30 / 10)
        }

        [Test, Description("the hook flies in 0.06-0.1 s; anchors in front and above the horizon win")]
        public void Hook_TimeClamped_AnchorsInFrontAndAboveWin()
        {
            Exactly(HookTime(1, a), a.hookMin);
            Exactly(HookTime(60, a), a.hookMax);
            Vec3 eye = Vec3.Zero;
            var aim = new Vec3(0, 0, -1);
            var cands = new[]
            {
                new AnchorCandidate { point = new Vec3(0, -3, -20) }, // below the horizon
                new AnchorCandidate { point = new Vec3(0, 5, -20) }, // above: preferred
                new AnchorCandidate { point = new Vec3(0, 3, 20) }, // behind: never
            };
            Assert.AreEqual(1, PickAnchor(cands, eye, aim, (15 * Math.PI) / 180, 60));
            Assert.AreEqual(-1, PickAnchor(new[] { new AnchorCandidate { point = new Vec3(0, 0, -70) } }, eye, aim, 0.3, 60)); // out of range
            var withNode = new[] { cands[0], cands[1], cands[2], new AnchorCandidate { point = new Vec3(0, 4, -22), node = true } };
            Assert.AreEqual(3, PickAnchor(withNode, eye, aim, 0.3, 60)); // a node wins
        }
    }
}
