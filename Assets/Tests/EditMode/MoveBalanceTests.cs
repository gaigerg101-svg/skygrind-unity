// NIGHT16 U3-29.4 (builder M owns this file): MoveBalance's rules (the slide charge, the flat-ground decay, the slide-hop
// falloff) and the Mover with them on and off (off: the web's mover bit for bit). The config: PortExport/config.json and
// Design/unity-config.json beside it (design.move); the web's config alone (no design block) for the neutral rules.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class MoveBalanceTests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));
        static MovementConfig Mc => Cfg.movement;
        static double Dt => 1 / Mc.loop.simHz;

        static MoveBalance.Params On => MoveBalance.Params.From(Cfg).With(true, true, true);

        static TriangleWorld Plane()
        {
            var w = new TriangleWorld();
            w.AddBox(new Vec3(-2000, -1, -2000), new Vec3(2000, 0, 2000));
            return w;
        }

        static Mover Walker(MoveBalance.Params? rules, List<MoverEvent> events = null)
        {
            var m = new Mover(Mc, Plane());
            if (rules.HasValue) m.SetMoveBalance(rules.Value);
            if (events != null) m.Events += e => events.Add(e);
            m.Reset(0, 0, 1000);
            for (int i = 0; i < 30; i++) m.Step(new InputFrame(), Dt);
            for (int i = 0; i < 120; i++) m.Step(new InputFrame { moveZ = 1 }, Dt); // walk speed
            return m;
        }

        static InputFrame Slide(bool held, bool pressed, bool jump = false) =>
            new InputFrame { moveZ = 1, slide = new Button { held = held, pressed = pressed }, jump = new Button { held = jump, pressed = jump } };

        // --- Params ---------------------------------------------------------------------------------------------------

        [Test]
        public void Params_ReadDesignMove()
        {
            MoveBalance.Params p = MoveBalance.Params.From(Cfg);
            Assert.That(p.slideCharge && p.slideDecay && p.hopFalloff, Is.EqualTo(Cfg.Design("move.slideCharge.enabled") && Cfg.Design("move.slideDecay.enabled") && Cfg.Design("move.hopFalloff.enabled")));
            Assert.That(p.rechargeTime, Is.EqualTo(Cfg.Num("design.move.slideCharge.rechargeTime")));
            Assert.That(p.rechargeSteps, Is.EqualTo(MovementMath.StepsFor(p.rechargeTime, Dt)));
            Assert.That(p.sustainSteps, Is.EqualTo(MovementMath.StepsFor(Cfg.Num("design.move.slideDecay.sustainTime"), Dt)));
            Assert.That(p.decaySteps, Is.EqualTo(MovementMath.StepsFor(Cfg.Num("design.move.slideDecay.decayTime"), Dt)));
            Assert.That(p.chainSteps, Is.EqualTo(MovementMath.StepsFor(Cfg.Num("design.move.hopFalloff.chainWindow"), Dt)));
            Assert.That(p.hopStep, Is.EqualTo(Cfg.Num("design.move.hopFalloff.step")));
            Assert.That(p.minMult, Is.EqualTo(Cfg.Num("design.move.hopFalloff.minMult")));
        }

        [Test]
        public void Params_WebConfigIsNeutral()
        {
            GameConfig web = GameConfig.FromText(File.ReadAllText(Path.Combine(Port.Root, "config.json")), null);
            MoveBalance.Params p = MoveBalance.Params.From(web);
            Assert.That(p.Any, Is.False);
            Assert.That(MoveBalance.Params.Neutral.Any, Is.False);
        }

        [Test]
        public void Params_WithSetsOnlyTheFlags()
        {
            MoveBalance.Params p = MoveBalance.Params.From(Cfg).With(false, true, false);
            Assert.That((p.slideCharge, p.slideDecay, p.hopFalloff), Is.EqualTo((false, true, false)));
            Assert.That(p.rechargeSteps, Is.EqualTo(MoveBalance.Params.From(Cfg).rechargeSteps));
        }

        // --- the pure rules -------------------------------------------------------------------------------------------

        [Test]
        public void EntryFloor_ChargedSnapsUncharged_KeepsEntryNeverAboveFloor()
        {
            Assert.That(MoveBalance.EntryFloor(11.6, 8, true, 8), Is.EqualTo(11.6));
            Assert.That(MoveBalance.EntryFloor(11.6, 9.5, false, 8), Is.EqualTo(9.5));
            Assert.That(MoveBalance.EntryFloor(11.6, 15, false, 8), Is.EqualTo(11.6));
            Assert.That(MoveBalance.EntryFloor(11.6, 0, false, 8), Is.EqualTo(8), "never below walking (NIGHT16 review: 0 rooted the player)");
            Assert.That(MoveBalance.EntryFloor(11.6, 3, false, 8), Is.EqualTo(8));
        }

        [Test]
        public void DecayedFloor_SustainThenLinearToTarget()
        {
            Assert.That(MoveBalance.DecayedFloor(11.6, 8, 72, 72, 60), Is.EqualTo(11.6));
            Assert.That(MoveBalance.DecayedFloor(11.6, 8, 102, 72, 60), Is.EqualTo(11.6 + (8 - 11.6) * 0.5).Within(1e-12));
            Assert.That(MoveBalance.DecayedFloor(11.6, 8, 132, 72, 60), Is.EqualTo(8));
            Assert.That(MoveBalance.DecayedFloor(11.6, 8, 5000, 72, 60), Is.EqualTo(8));
            Assert.That(MoveBalance.DecayedFloor(6, 8, 5000, 72, 60), Is.EqualTo(6), "never raises a floor below the target");
        }

        [Test]
        public void Hops_FallOffInsideTheWindow()
        {
            Assert.That(MoveBalance.NextHop(false, 0, 0, 120), Is.EqualTo(0), "the first slide-jump");
            Assert.That(MoveBalance.NextHop(true, 0, 10, 120), Is.EqualTo(1));
            Assert.That(MoveBalance.NextHop(true, 1, 120, 120), Is.EqualTo(2));
            Assert.That(MoveBalance.NextHop(true, 2, 121, 120), Is.EqualTo(0), "past the window: a fresh first hop");
            Assert.That(MoveBalance.HopMult(1.2, 0, 0.1, 1), Is.EqualTo(1.2));
            Assert.That(MoveBalance.HopMult(1.2, 1, 0.1, 1), Is.EqualTo(1.2 - 0.1));
            Assert.That(MoveBalance.HopMult(1.2, 2, 0.1, 1), Is.EqualTo(1));
            Assert.That(MoveBalance.HopMult(1.2, 9, 0.1, 1), Is.EqualTo(1));
            Assert.That(MoveBalance.HopMult(1.2, 0, -0.5, 1), Is.EqualTo(1.2), "never above the web's multiplier");
        }

        [Test]
        public void Charge_HoldsWhileSlidingThenReturnsOnce()
        {
            var s = new MoveBalance.State { rules = On, spent = true, rechargeLeft = On.rechargeSteps };
            for (int i = 0; i < 500; i++) Assert.That(MoveBalance.TickCharge(ref s, true), Is.False);
            Assert.That(s.rechargeLeft, Is.EqualTo(On.rechargeSteps));
            Assert.That(MoveBalance.ChargeProgress(s.spent, s.rechargeLeft, On.rechargeSteps), Is.EqualTo(0));
            int back = -1;
            for (int i = 0; i < On.rechargeSteps + 10; i++)
                if (MoveBalance.TickCharge(ref s, false)) Assert.That(back, Is.EqualTo(-1), "once"); else if (!s.spent && back < 0) back = i;
            Assert.That(s.spent, Is.False);
            Assert.That(MoveBalance.ChargeProgress(false, 0, On.rechargeSteps), Is.EqualTo(1));
            Assert.That(MoveBalance.ChargeProgress(true, On.rechargeSteps / 2, On.rechargeSteps), Is.EqualTo(0.5).Within(1e-9));
        }

        // --- the Mover --------------------------------------------------------------------------------------------------

        /// <summary>A script that touches every slide path: tapped, held, hops, a dash; positions and velocities per step.</summary>
        static List<Vec3> Script(MoveBalance.Params? rules)
        {
            Mover m = Walker(rules);
            var trace = new List<Vec3>();
            for (int i = 0; i < 1800; i++)
            {
                bool slide = i < 600 ? i % 36 < 18 : i < 1200 || i % 50 < 40;
                bool jump = i >= 1200 && m.grounded;
                var f = new InputFrame { moveZ = 1, yaw = i * 0.0007, slide = new Button { held = slide, pressed = slide && (i % 36 == 0 || i == 600) }, jump = new Button { held = jump, pressed = jump } };
                f.dash = new Button { held = i == 1500, pressed = i == 1500 };
                m.Step(f, Dt);
                trace.Add(m.position);
                trace.Add(m.velocity);
            }
            return trace;
        }

        [Test]
        public void Mover_NeutralRulesAreTheWebsMover()
        {
            List<Vec3> web = Script(null), neutral = Script(MoveBalance.Params.Neutral), off = Script(MoveBalance.Params.From(Cfg).With(false, false, false));
            for (int i = 0; i < web.Count; i++)
            {
                Assert.That(neutral[i].x == web[i].x && neutral[i].y == web[i].y && neutral[i].z == web[i].z, $"neutral, value {i}");
                Assert.That(off[i].x == web[i].x && off[i].y == web[i].y && off[i].z == web[i].z, $"flags off, value {i}");
            }
        }

        [Test]
        public void Mover_HeldSlideDecaysToWalkSpeed()
        {
            Mover m = Walker(On);
            m.Step(Slide(true, true), Dt);
            Assert.That(m.Speed, Is.EqualTo(Mc.slide.floorSpeed).Within(1e-9), "the charged burst");
            double atSustain = 0;
            for (int i = 1; i < 1200; i++)
            {
                m.Step(Slide(true, false), Dt);
                if (i == On.sustainSteps - 1) atSustain = m.Speed;
            }
            Assert.That(atSustain, Is.EqualTo(Mc.slide.floorSpeed).Within(1e-9), "the floor holds for sustainTime");
            Assert.That(m.state, Is.EqualTo(MoveState.Slide));
            Assert.That(m.Speed, Is.EqualTo(Mc.ground.walkSpeed).Within(1e-6), "10 s later: walk speed");
        }

        [Test]
        public void Mover_UnchargedSlideKeepsItsEntrySpeed()
        {
            var events = new List<MoverEvent>();
            Mover m = Walker(On, events);
            for (int i = 0; i < 12; i++) m.Step(Slide(true, i == 0), Dt); // the charged slide
            Assert.That(m.slideCharge, Is.EqualTo(0));
            for (int i = 0; i < 24; i++) m.Step(Slide(false, false), Dt); // released: 0.2 s of walking (the clock runs)
            double entry = m.Speed;
            Assert.That(entry, Is.GreaterThan(Mc.ground.walkSpeed - 1e-9));
            m.Step(Slide(true, true), Dt);
            Assert.That(m.state, Is.EqualTo(MoveState.Slide), "the press always slides");
            Assert.That(m.Speed, Is.LessThan(Mc.slide.floorSpeed - 0.5), "no burst");
            for (int i = 0; i < 60; i++)
            {
                double before = m.Speed;
                m.Step(Slide(true, false), Dt);
                Assert.That(m.Speed, Is.LessThanOrEqualTo(before + 1e-9), "never snapped up to floorSpeed");
                Assert.That(m.Speed, Is.EqualTo(entry).Within(1e-9), "never a loss: the uncharged floor is the entry speed (inside sustainTime)");
            }
            Assert.That(events.FindAll(e => e.type == MoverEventType.SlideCharged).Count, Is.EqualTo(0), "the clock held while the slide ran");
        }

        [Test]
        public void Mover_ChargeReturnsAfterRechargeTime_Event()
        {
            var events = new List<MoverEvent>();
            Mover m = Walker(On, events);
            for (int i = 0; i < 12; i++) m.Step(Slide(true, i == 0), Dt);
            int charged = -1;
            double mid = -1;
            for (int i = 0; i < On.rechargeSteps + 5; i++)
            {
                m.Step(Slide(false, false), Dt);
                if (i == On.rechargeSteps / 2) mid = m.slideCharge;
                if (charged < 0 && events.Exists(e => e.type == MoverEventType.SlideCharged)) charged = i;
            }
            Assert.That(mid, Is.GreaterThan(0.3).And.LessThan(0.7), "the HUD's ring fills");
            Assert.That(charged, Is.InRange(On.rechargeSteps - 2, On.rechargeSteps + 1));
            Assert.That(m.slideCharge, Is.EqualTo(1));
            Assert.That(events.FindAll(e => e.type == MoverEventType.SlideCharged).Count, Is.EqualTo(1));
            m.Step(Slide(true, true), Dt);
            Assert.That(m.Speed, Is.EqualTo(Mc.slide.floorSpeed).Within(1e-9), "charged again: the burst");
        }

        [Test]
        public void Mover_HopChainFallsOff()
        {
            var events = new List<MoverEvent>();
            Mover m = Walker(On, events);
            m.Step(Slide(true, true), Dt);
            for (int i = 0; i < 1200; i++) m.Step(Slide(true, false, m.grounded), Dt);
            var ratios = new List<double>();
            foreach (MoverEvent e in events)
                if (e.type == MoverEventType.Jump && e.kind == JumpKind.Slide) ratios.Add(e.speedAfter / e.speedBefore);
            Assert.That(ratios.Count, Is.GreaterThanOrEqualTo(4));
            Assert.That(ratios[0], Is.EqualTo(Mc.slide.jumpHorizontalMult).Within(1e-9), "a first hop pays in full");
            Assert.That(ratios[1], Is.EqualTo(MoveBalance.HopMult(Mc.slide.jumpHorizontalMult, 1, On.hopStep, On.minMult)).Within(1e-9));
            for (int i = 2; i < ratios.Count; i++) Assert.That(ratios[i], Is.LessThanOrEqualTo(MoveBalance.HopMult(Mc.slide.jumpHorizontalMult, 2, On.hopStep, On.minMult) + 1e-9));
        }

        [Test]
        public void Mover_SetMoveBalanceResetsTheCharge()
        {
            Mover m = Walker(On);
            for (int i = 0; i < 12; i++) m.Step(Slide(true, i == 0), Dt);
            Assert.That(m.slideCharge, Is.EqualTo(0));
            m.SetMoveBalance(On);
            Assert.That(m.slideCharge, Is.EqualTo(1));
            Assert.That(m.moveBalance.spent, Is.False);
        }

        // --- NIGHT16 review: an uncharged slide never roots the player; a NIGHT15 save still resumes ---------------------

        /// <summary>The rules on with a recharge that never comes back inside the test (the charge stays spent).</summary>
        static MoveBalance.Params Spendable
        {
            get
            {
                MoveBalance.Params p = On;
                p.rechargeSteps = 1 << 20;
                return p;
            }
        }

        /// <summary>A mover with its charge spent (one charged slide from walk speed), then stopped dead (no input) on the plane.</summary>
        static Mover SpentAndStill()
        {
            Mover m = Walker(Spendable);
            for (int i = 0; i < 12; i++) m.Step(Slide(true, i == 0), Dt);
            for (int i = 0; i < 360; i++) m.Step(new InputFrame(), Dt);
            Assert.That(m.moveBalance.spent, Is.True, "the charge is spent");
            Assert.That(m.Speed, Is.EqualTo(0), "standing still");
            return m;
        }

        static double WalkFloor => Mc.ground.walkSpeed;

        [Test]
        public void Mover_UnchargedSlideFromAStandstill_MovesAtWalkSpeedAlongTheInput()
        {
            Mover m = SpentAndStill();
            var f = new InputFrame { moveX = 1, slide = new Button { held = true, pressed = true } }; // strafe right: the wish is +x at yaw 0
            for (int i = 0; i < 24; i++)
            {
                m.Step(f, Dt);
                f.slide.pressed = false;
            }
            Assert.That(m.state, Is.EqualTo(MoveState.Slide));
            Assert.That(m.Speed, Is.GreaterThanOrEqualTo(WalkFloor - 1e-9), "never slower than walking (it rooted the player at 0 m/s)");
            Assert.That(m.velocity.x / m.Speed, Is.EqualTo(1).Within(1e-9), "the slide's direction is the input (the web's StartSlide)");
        }

        [Test]
        public void Mover_UnchargedSlideFromAStandstill_NoInput_GoesAlongTheFacing()
        {
            Mover m = SpentAndStill();
            var f = new InputFrame { slide = new Button { held = true, pressed = true } };
            for (int i = 0; i < 24; i++)
            {
                m.Step(f, Dt);
                f.slide.pressed = false;
            }
            Assert.That(m.Speed, Is.GreaterThanOrEqualTo(WalkFloor - 1e-9));
            Assert.That(-m.velocity.z / m.Speed, Is.EqualTo(1).Within(1e-9), "yaw 0 faces -z");
        }

        [Test]
        public void Mover_UnchargedSlideFromASlowWalk_IsNeverSlowerThanWalking()
        {
            Mover m = SpentAndStill();
            for (int i = 0; i < 240; i++) m.Step(new InputFrame { moveZ = 3 / Mc.ground.walkSpeed }, Dt); // a slow walk: 3 m/s
            Assert.That(m.Speed, Is.EqualTo(3).Within(1e-6));
            var f = new InputFrame { moveZ = 3 / Mc.ground.walkSpeed, slide = new Button { held = true, pressed = true } };
            for (int i = 0; i < 24; i++)
            {
                m.Step(f, Dt);
                f.slide.pressed = false;
            }
            Assert.That(m.state, Is.EqualTo(MoveState.Slide));
            Assert.That(m.Speed, Is.GreaterThanOrEqualTo(WalkFloor - 1e-9));
            for (int i = 0; i < 1200; i++) m.Step(new InputFrame { slide = new Button { held = true } }, Dt);
            Assert.That(m.Speed, Is.GreaterThanOrEqualTo(WalkFloor - 1e-9), "held 10 s: walk speed, never below");
        }

        [Test]
        public void Mover_LandingWithSlideHeldAfterAVerticalJump_IsNeverRooted()
        {
            Mover m = SpentAndStill();
            m.Step(new InputFrame { jump = new Button { held = true, pressed = true } }, Dt);
            bool landed = false;
            int after = 0;
            for (int i = 0; i < 600 && after < 24; i++)
            {
                m.Step(new InputFrame { slide = new Button { held = true } }, Dt); // held, never a fresh press: no slam
                if (m.grounded) landed = true;
                if (landed) after++;
            }
            Assert.That(landed, Is.True);
            Assert.That(m.state, Is.EqualTo(MoveState.Slide));
            Assert.That(m.Speed, Is.GreaterThanOrEqualTo(WalkFloor - 1e-9));
        }

        [Test]
        public void MoverState_ANight15SaveWithoutTheBalanceFields_Loads()
        {
            Mover a = Walker(Spendable);
            for (int i = 0; i < 30; i++) a.Step(Slide(true, i == 0), Dt);
            for (int i = 0; i < 30; i++) a.Step(new InputFrame { moveZ = 1 }, Dt);
            // NIGHT15's writer: every field it knew (MoverState.Fields without slideCharge and moveBalance)
            string Old(Mover m)
            {
                var w = new StateWriter();
                foreach (string f in MoverState.Fields)
                    if (f != "slideCharge" && f != "moveBalance") FieldSnapshot.WriteField(w, m, FieldSnapshot.Field(typeof(Mover), f), null);
                return w.ToJson();
            }
            string json = Old(a);
            Assert.That(json.Contains("\"moveBalance\"") || json.Contains("\"slideCharge\""), Is.False);
            var b = new Mover(Mc, Plane());
            b.SetMoveBalance(On); // as StartRun left it: the run's flags, the charge full
            Assert.DoesNotThrow(() => new MoverState(b).Load(new StateReader(json)));
            Assert.That(Old(b), Is.EqualTo(json), "every other field as saved");
            Assert.That(b.slideCharge, Is.EqualTo(1));
            Assert.That(b.moveBalance.spent, Is.False);
            Assert.That((b.moveBalance.rules.slideCharge, b.moveBalance.rules.slideDecay, b.moveBalance.rules.hopFalloff), Is.EqualTo((On.slideCharge, On.slideDecay, On.hopFalloff)));
            // a NIGHT16 save stays strict and whole
            var full = new StateWriter();
            new MoverState(a).Save(full);
            var c = new Mover(Mc, Plane());
            new MoverState(c).Load(new StateReader(full.ToJson()));
            Assert.That(c.moveBalance.spent, Is.EqualTo(a.moveBalance.spent));
            Assert.That(c.slideCharge, Is.EqualTo(a.slideCharge));
            string broken = Old(a).Replace("\"position\"", "\"positionX\"");
            Assert.Throws<StateException>(() => new MoverState(new Mover(Mc, Plane())).Load(new StateReader(broken)), "any other missing field still throws");
        }
    }
}
