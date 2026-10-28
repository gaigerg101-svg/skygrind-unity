// NIGHT43 builder M1 (design.n43.chimney43 / ride43 / boost43; Core/Move43.cs, Core/Boost43.cs): the rows' numbers, the new-wall and same-plane
// rules, and the Mover in a box world (TriangleWorld: the web's collider) with the rows on and off: a chimney of facing walls climbed by presses
// (the count refills on the other wall), and B just after a wall jump (a drop that keeps the speed, not a slam). The harness (n43move) measures
// the same at length with the game's ride rules.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Move43Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root(), "config.json"));
        const double Dt = 1.0 / 120;

        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        [Test]
        public void TheRows_HaveEveryNumber_WithItsWhy_AndStayOff()
        {
            var rows = new Dictionary<string, string[]>
            {
                ["chimney43"] = new[] { "newWallDot", "newWallDist", "rideRefills" },
                ["ride43"] = new[] { "tapDraw", "attachReach", "keepRise", "lostGrace", "slamGrace", "slamLookDeg", "holdNoStick", "lockPlane", "dashAttach", "dashIntoDot", "bufferDeg", "pressPushes", "sameWallDeg", "sameWallPlane", "sameWallAlong", "sameWallRise", "sameWallDrop" },
                ["boost43"] = new[] { "window", "lift", "fullDeg", "fallFloor", "doubleKeeps" },
            };
            foreach (var kv in rows)
            {
                string p = "design.n43." + kv.Key + ".";
                Assert.That(Cfg.Get(p + "enabled"), Is.TypeOf<bool>(), kv.Key + " has its switch (NIGHT43: the lead turned it on at the freeze)");
                foreach (string k in kv.Value)
                {
                    Assert.That(Cfg.Has(p + k), Is.True, kv.Key + "." + k);
                    Assert.That(Cfg.Has(p + k + "_why"), Is.True, kv.Key + "." + k + "_why");
                }
            }
            Assert.That(Move43.Params.From(Cfg, false, false).Any, Is.False, "both bits off: neutral");
            Assert.That(Boost43.Params.From(Cfg, false).on, Is.False);
            Boost43.Params b = Boost43.Params.From(Cfg, true);
            Assert.That(b.on && b.push.on, Is.True);
            Assert.That(b.windowSteps, Is.EqualTo(MovementMath.StepsFor(Cfg.Num("design.n43.boost43.window"), Dt)));
        }

        [Test]
        public void NewWall_IsAnotherFacing_OrFartherThanNewWallDist()
        {
            var s = new Move43.State { p = Move43.Params.From(Cfg, true, false) };
            var n = new Vec3(-1, 0, 0);
            Assert.That(Move43.NewWall(s, n, Vec3.Zero), Is.True, "no wall jump yet this airtime");
            s.hasWall = true;
            s.wallN = n;
            s.wallAt = Vec3.Zero;
            Assert.That(Move43.NewWall(s, n, new Vec3(0, 1.0, 0.5)), Is.False, "the same face, 1 m up: the same wall");
            Assert.That(Move43.NewWall(s, n, new Vec3(0, s.p.sameRise + 0.1, 0)), Is.True, "the same facing sameWallRise higher: a new wall (THE STACK's panel a turn up)");
            Assert.That(Move43.NewWall(s, n, new Vec3(0, -s.p.sameDrop - 0.1, 0)), Is.True, "fallen sameWallDrop below: caught again");
            Assert.That(Move43.NewWall(s, new Vec3(1, 0, 0), Vec3.Zero), Is.True, "the facing wall of a chimney");
            Assert.That(Move43.NewWall(s, new Vec3(0, 0, 1), Vec3.Zero), Is.True, "a panel 90 deg round (THE STACK)");
            Assert.That(Move43.NewWall(s, n, new Vec3(0, 0, s.p.sameAlong + 0.1)), Is.True, "along the ground past sameWallAlong");
            Assert.That(Move43.NewWall(s, n, new Vec3(-(s.p.samePlane + 0.1), 0, 0)), Is.True, "a parallel wall off the plane");
        }

        [Test]
        public void SamePlane_IsTheOffsetAlongTheNormal()
        {
            var n = new Vec3(1, 0, 0);
            Assert.That(Move43.SamePlane(n, Vec3.Zero, new Vec3(0.3, 2, 9), 0.5), Is.True);
            Assert.That(Move43.SamePlane(n, Vec3.Zero, new Vec3(0.8, 0, 0), 0.5), Is.False);
        }

        static TriangleWorld Chimney(double half)
        {
            var w = new TriangleWorld();
            w.AddBox(new Vec3(-20, -1, -60), new Vec3(20, 0, 60));
            w.AddBox(new Vec3(-half - 1, 0, -60), new Vec3(-half, 14, 60));
            w.AddBox(new Vec3(half, 0, -60), new Vec3(half + 1, 14, 60));
            return w;
        }

        /// <summary>The web's Mover (every row neutral) with chimney43 on or off between walls 4.5 m apart: a press on the step after each contact
        /// with the wall being jumped to. Returns the wall jumps and the double jumps over 6 s (or to the landing).</summary>
        static (int wallJumps, int doubles, double top) ChimneyRun(bool chimney)
        {
            double half = 2.25, r = Cfg.movement.body.capsuleRadius;
            var m = new Mover(Cfg.movement, Chimney(half));
            m.SetMove43(Move43.Params.From(Cfg, chimney, false));
            int wallJumps = 0, doubles = 0;
            m.Events += e =>
            {
                if (e.type == MoverEventType.WallJump) wallJumps++;
                if (e.type == MoverEventType.DoubleJump) doubles++;
            };
            m.Reset(-half + r + 0.005, 2, 0);
            m.velocity = new Vec3(-0.5, 0, 0);
            int target = -1, held = 0;
            bool armed = true;
            double top = 0;
            for (int s = 0; s < 720; s++)
            {
                bool onTarget = m.touchingWall && m.wallNormal.x * -target > 0.5;
                bool press = false;
                if (held > 0) held--;
                if (!onTarget) armed = true;
                else if (armed && !m.grounded)
                {
                    press = true;
                    armed = false;
                    held = 3;
                }
                int wj = wallJumps;
                m.Step(new InputFrame { moveX = target, jump = new Button { pressed = press, held = press || held > 0 } }, Dt);
                if (wallJumps > wj) target = -target;
                top = System.Math.Max(top, m.position.y);
                if (m.grounded && s > 30) break;
            }
            return (wallJumps, doubles, top);
        }

        [Test]
        public void Chimney_Off_RunsOutAfterThree_On_KeepsClimbing()
        {
            var off = ChimneyRun(false);
            var on = ChimneyRun(true);
            Assert.That(off.wallJumps, Is.EqualTo(Cfg.movement.wallJump.maxPerAirtime), "today: the count, then no more wall jumps");
            Assert.That(on.wallJumps, Is.GreaterThan(off.wallJumps + 2), "chimney43: the other wall refills the count");
            Assert.That(on.top, Is.GreaterThan(off.top + 2), "and the climb goes higher");
        }

        /// <summary>A wall jump off a wall at x = 3 (the web's Mover, every older row neutral), then B (slide) `after` steps later: the state and
        /// the horizontal speed the step after B.</summary>
        static (MoveState state, double speed) BAfterWallJump(bool ride, int after, double pitchDeg = 0)
        {
            var w = new TriangleWorld();
            w.AddBox(new Vec3(-20, -1, -200), new Vec3(20, 0, 60));
            w.AddBox(new Vec3(3, 0, -200), new Vec3(4, 14, 60));
            var m = new Mover(Cfg.movement, w);
            m.SetMove43(Move43.Params.From(Cfg, false, ride));
            double r = Cfg.movement.body.capsuleRadius;
            m.Reset(3 - r - 0.005, 4, 0);
            m.velocity = new Vec3(0.5, 0, -10);
            for (int s = 0; s < 4; s++) m.Step(new InputFrame { moveX = 1 }, Dt); // touch the wall
            m.Step(new InputFrame { jump = new Button { pressed = true, held = true } }, Dt);
            for (int s = 0; s < after; s++) m.Step(new InputFrame(), Dt);
            m.Step(new InputFrame { pitch = pitchDeg * System.Math.PI / 180, slide = new Button { pressed = true, held = true } }, Dt);
            return (m.state, m.Speed);
        }

        [Test]
        public void B_JustAfterLeavingAWall_IsADrop_LaterOrLookingDownASlam()
        {
            var before = BAfterWallJump(false, 12);
            Assert.That(before.state, Is.EqualTo(MoveState.Slam), "today: B 0.1 s after a wall jump slams");
            Assert.That(before.speed, Is.LessThan(0.01));
            var drop = BAfterWallJump(true, 12);
            Assert.That(drop.state, Is.EqualTo(MoveState.Air), "ride43 (d): a drop");
            Assert.That(drop.speed, Is.GreaterThan(9), "the horizontal speed kept");
            Assert.That(BAfterWallJump(true, 12, -60).state, Is.EqualTo(MoveState.Slam), "looking down past slamLookDeg: the slam");
            int late = MovementMath.StepsFor(Cfg.Num("design.n43.ride43.slamGrace"), Dt) + 6;
            Assert.That(BAfterWallJump(true, late).state, Is.EqualTo(MoveState.Slam), "after the grace: the slam");
        }

        [Test]
        public void Boost43_Lift_IsTheRowsAtFullDeg()
        {
            Boost43.Params b = Boost43.Params.From(Cfg, true);
            double jump = Cfg.Num("weapons.shotgun.jumpSpeed");
            Assert.That(Boost42.LiftAt(90, jump, b.push), Is.EqualTo(b.push.lift).Within(1e-9));
            Assert.That(Boost42.LiftAt(b.push.fullDeg, jump, b.push), Is.EqualTo(b.push.lift).Within(1e-6));
            Vec3 fall = Boost42.Push(new Vec3(0, -9, 0), new Vec3(0, -1, 0), jump, b.push);
            Assert.That(fall.y, Is.EqualTo(-b.push.fallFloor + b.push.lift).Within(1e-9), "a fall is cut first");
        }
    }
}
