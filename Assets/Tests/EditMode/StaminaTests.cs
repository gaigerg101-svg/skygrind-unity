// NIGHT17 U3-30.4 (builder M owns this file; the shared commit's empty fixture): one bar, a sprint and a partial boost
// (Core/Stamina.cs behind design.stamina): the bar's start and VIGOR, the boost's cost and its duration scaled by the fill
// (floor, fullAt), the invulnerable steps scaling with it, the sprint's speed and drain and its end at empty, the refill's
// delays, the kill / slam / parry rewards, every flag off = the web's pips, an older save without the field. The tape's
// format 3 flags byte and a format 2 tape: Game/StaminaTapeTests.cs (the tape is the Game's). The config: PortExport's
// config.json and Design/unity-config.json beside it (design.stamina); the web's config alone for the neutral rules.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class StaminaTests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));
        static MovementConfig Mc => Cfg.movement;
        static double Dt => 1 / Mc.loop.simHz;

        /// <summary>design.stamina with every rule on (the defaults).</summary>
        static Stamina.Params On => Stamina.Params.From(Cfg).With(true, true, true, true, true);

        static TriangleWorld Plane()
        {
            var w = new TriangleWorld();
            w.AddBox(new Vec3(-2000, -1, -2000), new Vec3(2000, 0, 2000));
            return w;
        }

        /// <summary>A mover on a plane, walked up to walk speed (W held 1 s), with these stamina rules (null: none set).</summary>
        static Mover Walker(Stamina.Params? rules, List<MoverEvent> events = null, int bonusPips = 0)
        {
            var m = new Mover(Mc, Plane());
            m.bonusPips = bonusPips;
            if (rules.HasValue) m.SetStamina(rules.Value);
            if (events != null) m.Events += e => events.Add(e);
            m.Reset(0, 0, 1000);
            for (int i = 0; i < 30; i++) m.Step(new InputFrame(), Dt);
            for (int i = 0; i < 120; i++) m.Step(new InputFrame { moveZ = 1 }, Dt);
            return m;
        }

        static InputFrame Shift(bool held, bool pressed, double moveZ = 1) =>
            new InputFrame { moveZ = moveZ, dash = new Button { held = held, pressed = pressed } };

        /// <summary>Set the bar with the refill's delay just started (no refill on the next steps).</summary>
        static void Bar(Mover m, double bar)
        {
            m.stamina.bar = bar;
            m.stamina.sinceSpend = 0;
            m.stamina.full = bar >= m.StaminaMax;
        }

        /// <summary>A tap of the boost key (one press step), then W until the boost ends: the boost's steps and invulnerable steps.</summary>
        static (int steps, int invulnerable) Tap(Mover m)
        {
            m.Step(Shift(true, true), Dt);
            int steps = 1, inv = m.invulnerable ? 1 : 0;
            while (m.state == MoveState.Dash && steps < 200)
            {
                m.Step(new InputFrame { moveZ = 1 }, Dt);
                steps++;
                if (m.invulnerable) inv++;
            }
            return (steps, inv);
        }

        // --- Params ---------------------------------------------------------------------------------------------------

        [Test]
        public void Params_ReadDesignStamina()
        {
            Stamina.Params p = Stamina.Params.From(Cfg);
            Assert.That(p.bar, Is.EqualTo(Cfg.Design("stamina.bar.enabled")));
            Assert.That(p.boost, Is.EqualTo(Cfg.Design("stamina.boost.enabled")));
            Assert.That(p.sprint, Is.EqualTo(Cfg.Design("stamina.sprint.enabled")));
            Assert.That(p.refill, Is.EqualTo(Cfg.Design("stamina.refill.enabled")));
            Assert.That(p.rewards, Is.EqualTo(Cfg.Design("stamina.rewards.enabled")));
            Assert.That(p.max, Is.EqualTo(Cfg.Num("design.stamina.bar.max")));
            Assert.That(p.bonusBars, Is.EqualTo(Cfg.Num("design.stamina.bar.bonusBars")));
            Assert.That(p.cost, Is.EqualTo(Cfg.Num("design.stamina.boost.cost")));
            Assert.That(p.floor, Is.EqualTo(Cfg.Num("design.stamina.boost.floor")));
            Assert.That(p.fullAt, Is.EqualTo(Cfg.Num("design.stamina.boost.fullAt")));
            Assert.That(p.dashJumpExtra, Is.EqualTo(Cfg.Num("design.stamina.boost.dashJumpExtra")));
            Assert.That(p.sprintSpeed, Is.EqualTo(Cfg.Num("design.stamina.sprint.speed")));
            Assert.That(p.drain, Is.EqualTo(Cfg.Num("design.stamina.sprint.drain")));
            Assert.That(p.rate, Is.EqualTo(Cfg.Num("design.stamina.refill.rate")));
            Assert.That(p.delaySteps, Is.EqualTo(MovementMath.StepsFor(Cfg.Num("design.stamina.refill.delay"), Dt)));
            Assert.That(p.emptyDelaySteps, Is.EqualTo(MovementMath.StepsFor(Cfg.Num("design.stamina.refill.emptyDelay"), Dt)));
            Assert.That(p.webRate, Is.EqualTo(Mc.dash.regenPerSec / Mc.dash.maxPips));
            Assert.That(p.kill, Is.EqualTo(Cfg.Num("design.stamina.rewards.kill")));
            Assert.That(p.slam, Is.EqualTo(Cfg.Num("design.stamina.rewards.slam")));
            Assert.That(p.parry, Is.EqualTo(Cfg.Num("design.stamina.rewards.parry")));
            Assert.That(p.sprintSpeed, Is.LessThan(Mc.slide.floorSpeed), "the slide stays the fastest free move");
            Assert.That(p.sprintSpeed, Is.GreaterThan(Mc.ground.walkSpeed));
        }

        [Test]
        public void Params_WebConfigIsNeutral()
        {
            GameConfig web = GameConfig.FromText(File.ReadAllText(Path.Combine(Port.Root, "config.json")), null);
            Assert.That(Stamina.Params.From(web).Any, Is.False);
            Assert.That(Stamina.Params.Neutral.Any, Is.False);
        }

        [Test]
        public void Params_WithSetsOnlyTheFlags_FlagReadsThemInOrder()
        {
            Stamina.Params p = Stamina.Params.From(Cfg).With(true, false, true, false, true);
            Assert.That((p.bar, p.boost, p.sprint, p.refill, p.rewards), Is.EqualTo((true, false, true, false, true)));
            Assert.That(p.cost, Is.EqualTo(Stamina.Params.From(Cfg).cost));
            Assert.That(p.delaySteps, Is.EqualTo(Stamina.Params.From(Cfg).delaySteps));
            for (int i = 0; i < Stamina.FlagCount; i++) Assert.That(p.Flag(i), Is.EqualTo(i % 2 == 0), "flag " + i);
            Assert.That(p.With(false, true, true, true, true).Any, Is.False, "every rule is a rule of the bar");
        }

        // --- the pure rules -------------------------------------------------------------------------------------------

        [Test]
        public void Max_OneBar_VigorAddsBonusBars()
        {
            Assert.That(Stamina.Max(On, 0), Is.EqualTo(1));
            Assert.That(Stamina.Max(On, 1), Is.EqualTo(2));
        }

        [Test]
        public void Boost_CostFloorAndFullAt()
        {
            Stamina.Params p = On; // cost 0.5, floor 0.25, fullAt 0.9
            Assert.That(Stamina.FloorFill(p), Is.EqualTo(0.125));
            Assert.That(Stamina.CanBoost(p, 0.124), Is.False);
            Assert.That(Stamina.CanBoost(p, 0.125), Is.True);
            Assert.That(Stamina.BoostFraction(p, 1.0), Is.EqualTo(1));
            Assert.That(Stamina.BoostFraction(p, 0.45), Is.EqualTo(1), "0.9 of the cost counts as full");
            Assert.That(Stamina.BoostFraction(p, 0.44), Is.EqualTo(0.88).Within(1e-12));
            Assert.That(Stamina.BoostFraction(p, 0.25), Is.EqualTo(0.5));
            Assert.That(Stamina.BoostFraction(p, 0.125), Is.EqualTo(0.25));
            Assert.That(Stamina.BoostSpend(p, 1.0), Is.EqualTo(0.5));
            Assert.That(Stamina.BoostSpend(p, 0.3), Is.EqualTo(0.3), "up to its cost");
            // the boost rule off: a boost needs its whole cost (the web's rule in bar units), always full
            Stamina.Params off = p.With(true, false, true, true, true);
            Assert.That(Stamina.FloorFill(off), Is.EqualTo(0.5));
            Assert.That(Stamina.CanBoost(off, 0.49), Is.False);
            Assert.That(Stamina.BoostFraction(off, 0.5), Is.EqualTo(1));
        }

        [Test]
        public void BoostSteps_ScaleTheDuration_InWholeSteps()
        {
            double d = Mc.dash.duration; // 0.25 s: 30 steps
            Assert.That(Stamina.BoostSteps(d, 1, Dt), Is.EqualTo(MovementMath.StepsFor(d, Dt)));
            Assert.That(Stamina.BoostSteps(d, 0.5, Dt), Is.EqualTo(15));
            Assert.That(Stamina.BoostSteps(d, 0.25, Dt), Is.EqualTo(8), "the floor: 8 of 30 steps");
        }

        [Test]
        public void Refill_WaitsItsDelay_NeverWhileBusy_CapsAtMax()
        {
            Stamina.Params p = On;
            double step = p.rate * Dt;
            Assert.That(Stamina.Refill(p, 0.2, 1, 1, p.delaySteps - 1, false, false, Dt), Is.EqualTo(0.2), "inside the delay");
            Assert.That(Stamina.Refill(p, 0.2, 1, 1, p.delaySteps, false, false, Dt), Is.EqualTo(0.2 + step).Within(1e-12));
            Assert.That(Stamina.Refill(p, 0.2, 1, 1, p.delaySteps, true, false, Dt), Is.EqualTo(0.2), "a sprint emptied it: the longer delay");
            Assert.That(Stamina.Refill(p, 0.2, 1, 1, p.emptyDelaySteps, true, false, Dt), Is.EqualTo(0.2 + step).Within(1e-12));
            Assert.That(Stamina.Refill(p, 0.2, 1, 1, 1000, false, true, Dt), Is.EqualTo(0.2), "never while boosting or sprinting");
            Assert.That(Stamina.Refill(p, 0.2, 1, 2, 1000, false, false, Dt), Is.EqualTo(0.2 + 2 * step).Within(1e-12), "x regenMult (the rank's reward)");
            Assert.That(Stamina.Refill(p, 0.999, 1, 1, 1000, false, false, Dt), Is.EqualTo(1));
            // the refill rule off: the web's regen in bar units, no delay
            Stamina.Params off = p.With(true, true, true, false, true);
            Assert.That(Stamina.Refill(off, 0.2, 1, 1, 0, false, false, Dt), Is.EqualTo(0.2 + p.webRate * Dt).Within(1e-12));
            Assert.That(Stamina.Refill(off, 0.2, 1, 1, 0, false, true, Dt), Is.EqualTo(0.2));
        }

        [Test]
        public void Drain_AndAdd_StayInsideTheBar()
        {
            Stamina.Params p = On;
            Assert.That(Stamina.Drain(p, 0.5, Dt), Is.EqualTo(0.5 - p.drain * Dt).Within(1e-12));
            Assert.That(Stamina.Drain(p, 0.001, Dt), Is.EqualTo(0));
            Assert.That(Stamina.Add(0.5, 0.15, 1), Is.EqualTo(0.65).Within(1e-12));
            Assert.That(Stamina.Add(0.95, 0.15, 1), Is.EqualTo(1));
            Assert.That(Stamina.SprintWish(p, 0, 1), Is.EqualTo(p.sprintSpeed));
            Assert.That(Stamina.SprintWish(p, 1, 1), Is.EqualTo(p.sprintSpeed), "the stick's length at most 1");
            Assert.That(Stamina.SprintWish(p, 0, 0.5), Is.EqualTo(p.sprintSpeed * 0.5));
        }

        // --- the Mover ------------------------------------------------------------------------------------------------

        [Test]
        public void Mover_TheBarStartsFull_OneBar_TwoWithVigor()
        {
            Mover m = Walker(On);
            Assert.That(m.StaminaMax, Is.EqualTo(1));
            Assert.That(m.stamina.bar, Is.EqualTo(1));
            Mover v = Walker(On, bonusPips: 1);
            Assert.That(v.StaminaMax, Is.EqualTo(2));
            Assert.That(v.stamina.bar, Is.EqualTo(2));
        }

        [Test]
        public void Mover_AFullBoost_CostsItsCost_RunsTheWebsDuration_InvulnerableThroughout()
        {
            var events = new List<MoverEvent>();
            Mover m = Walker(On, events);
            (int steps, int inv) = Tap(m);
            Assert.That(m.stamina.bar, Is.EqualTo(0.5).Within(1e-12), "a full boost costs 0.5 (the refill waits its delay)");
            Assert.That(steps, Is.EqualTo(MovementMath.StepsFor(Mc.dash.duration, Dt)));
            Assert.That(inv, Is.EqualTo(steps - 1), "invulnerable until the step EndDash runs (the web's)");
            Assert.That(events.Exists(e => e.type == MoverEventType.BoostPartial), Is.False);
            Assert.That(m.lastDashDistance, Is.GreaterThan(5.5), "24 m/s for 0.25 s");
        }

        [Test]
        public void Mover_APartialBoost_ScalesItsDuration_DistanceAndInvulnerability_SpeedUnchanged()
        {
            Mover full = Walker(On);
            Tap(full);
            double fullDistance = full.lastDashDistance;
            var events = new List<MoverEvent>();
            Mover m = Walker(On, events);
            Bar(m, 0.25); // half the cost
            m.Step(Shift(true, true), Dt);
            Assert.That(m.Speed, Is.EqualTo(Mc.dash.speed), "the speed stays dash.speed");
            int steps = 1, inv = 1;
            while (m.state == MoveState.Dash && steps < 200)
            {
                m.Step(new InputFrame { moveZ = 1 }, Dt);
                steps++;
                if (m.invulnerable) inv++;
            }
            Assert.That(steps, Is.EqualTo(15));
            Assert.That(inv, Is.EqualTo(14), "the invulnerable steps scale with the duration (the full boost's: 29 of 30)");
            Assert.That(m.stamina.bar, Is.EqualTo(0));
            Assert.That(m.lastDashDistance / fullDistance, Is.EqualTo(0.5).Within(0.05), "about half a full boost");
            MoverEvent partial = events.Find(e => e.type == MoverEventType.BoostPartial);
            Assert.That(partial.type, Is.EqualTo(MoverEventType.BoostPartial));
            Assert.That(partial.fraction, Is.EqualTo(0.5));
            // the floor: a quarter of the cost fires the shortest boost
            Mover f = Walker(On);
            Bar(f, 0.125);
            Assert.That(Tap(f).steps, Is.EqualTo(8));
        }

        [Test]
        public void Mover_UnderTheFloor_TheBoostFails()
        {
            var events = new List<MoverEvent>();
            Mover m = Walker(On, events);
            Bar(m, 0.1);
            m.Step(Shift(true, true), Dt);
            Assert.That(m.state, Is.Not.EqualTo(MoveState.Dash));
            Assert.That(events.Exists(e => e.type == MoverEventType.DashFail), Is.True);
        }

        [Test]
        public void Mover_TheDashJump_CostsItsExtra_OrIsANormalJumpWithout()
        {
            Mover m = Walker(On);
            m.Step(Shift(true, true), Dt);
            m.Step(new InputFrame { moveZ = 1, jump = new Button { held = true, pressed = true } }, Dt);
            Assert.That(m.DashJumpActive, Is.True, "a dash-jump with 0.5 left");
            Assert.That(m.stamina.bar, Is.EqualTo(0.25).Within(1e-12), "0.5 - its extra 0.25");
            Mover n = Walker(On);
            Bar(n, 0.6);
            n.Step(Shift(true, true), Dt); // 0.1 left
            n.Step(new InputFrame { moveZ = 1, jump = new Button { held = true, pressed = true } }, Dt);
            Assert.That(n.DashJumpActive, Is.False, "under the extra cost: a normal jump");
            Assert.That(n.state, Is.EqualTo(MoveState.Air));
        }

        [Test]
        public void Mover_HoldingAfterTheBoost_Sprints_BelowTheSlide_DrainsWhileMoving()
        {
            var events = new List<MoverEvent>();
            Mover m = Walker(On, events);
            m.Step(Shift(true, true), Dt);
            for (int i = 0; i < 40; i++) m.Step(Shift(true, false), Dt); // the boost (30 steps), then the sprint
            Assert.That(m.stamina.sprinting, Is.True);
            Assert.That(events.FindAll(e => e.type == MoverEventType.SprintStart).Count, Is.EqualTo(1));
            for (int i = 0; i < 60; i++) m.Step(Shift(true, false), Dt);
            Assert.That(m.Speed, Is.EqualTo(On.sprintSpeed).Within(1e-9), "10.5 m/s, under the slide's 11.6");
            double before = m.stamina.bar;
            for (int i = 0; i < 24; i++) m.Step(Shift(true, false), Dt); // 0.2 s
            Assert.That(before - m.stamina.bar, Is.EqualTo(On.drain * 0.2).Within(1e-9));
            // standing still on the ground ends it (no drain without moving)
            m.Step(Shift(true, false, 0), Dt);
            Assert.That(m.stamina.sprinting, Is.False);
            Assert.That(events.FindAll(e => e.type == MoverEventType.SprintEnd).Count, Is.EqualTo(1));
        }

        [Test]
        public void Mover_TheSprint_EndsByItselfAtEmpty_NoLockout_TheNextPressBoosts()
        {
            var events = new List<MoverEvent>();
            Mover m = Walker(On, events);
            m.Step(Shift(true, true), Dt);
            int i = 0;
            for (; i < 600 && m.stamina.bar > 0; i++) m.Step(Shift(true, false), Dt);
            Assert.That(m.stamina.bar, Is.EqualTo(0));
            Assert.That(i, Is.EqualTo(30 + (int)Math.Ceiling(0.5 / On.drain * 120)).Within(2), "the boost, then 0.5 bar at 0.4 / s");
            Assert.That(m.stamina.sprinting, Is.False);
            Assert.That(events.Exists(e => e.type == MoverEventType.SprintEnd), Is.True);
            for (int k = 0; k < 120; k++) m.Step(Shift(true, false), Dt); // still held: walking, no new sprint
            Assert.That(m.Speed, Is.EqualTo(Mc.ground.walkSpeed).Within(1e-9));
            Assert.That(m.stamina.sprinting, Is.False);
            Assert.That(events.FindAll(e => e.type == MoverEventType.SprintStart).Count, Is.EqualTo(1));
            // no lockout: once the bar is over the boost's floor, the next press boosts
            for (int k = 0; k < 120; k++) m.Step(new InputFrame { moveZ = 1 }, Dt);
            Assert.That(m.stamina.bar, Is.GreaterThan(Stamina.FloorFill(On)));
            m.Step(Shift(true, true), Dt);
            Assert.That(m.state, Is.EqualTo(MoveState.Dash));
        }

        [Test]
        public void Mover_TheRefill_WaitsItsDelays()
        {
            Mover m = Walker(On);
            Tap(m); // the boost ended: 0.5 left
            double b = m.stamina.bar;
            int delay = On.delaySteps;
            for (int i = 0; i < delay - 2; i++) m.Step(new InputFrame { moveZ = 1 }, Dt);
            Assert.That(m.stamina.bar, Is.EqualTo(b), "nothing inside 0.4 s");
            for (int i = 0; i < 120; i++) m.Step(new InputFrame { moveZ = 1 }, Dt);
            Assert.That(m.stamina.bar, Is.GreaterThan(b + 0.6 * On.rate).And.LessThanOrEqualTo(1));
            // emptied by a sprint: 0.8 s
            Mover s = Walker(On);
            s.Step(Shift(true, true), Dt);
            for (int i = 0; i < 600 && s.stamina.bar > 0; i++) s.Step(Shift(true, false), Dt);
            for (int i = 0; i < On.emptyDelaySteps - 2; i++) s.Step(new InputFrame { moveZ = 1 }, Dt);
            Assert.That(s.stamina.bar, Is.EqualTo(0), "nothing inside 0.8 s after a sprint emptied it");
            for (int i = 0; i < 12; i++) s.Step(new InputFrame { moveZ = 1 }, Dt);
            Assert.That(s.stamina.bar, Is.GreaterThan(0));
        }

        [Test]
        public void Mover_TheBarRefilledToFull_EmitsStaminaFullOnce()
        {
            var events = new List<MoverEvent>();
            Mover m = Walker(On, events);
            Tap(m);
            for (int i = 0; i < 240; i++) m.Step(new InputFrame { moveZ = 1 }, Dt);
            Assert.That(m.stamina.bar, Is.EqualTo(1));
            Assert.That(events.FindAll(e => e.type == MoverEventType.StaminaFull).Count, Is.EqualTo(1));
        }

        [Test]
        public void Mover_Rewards_KillSlamParry_AndOffWithTheirFlag()
        {
            Mover m = Walker(On);
            Bar(m, 0.2);
            m.StaminaReward(On.kill);
            Assert.That(m.stamina.bar, Is.EqualTo(0.35).Within(1e-12));
            m.RestoreStamina(); // a parry (design.ultra.parryRefillsDash): a full bar
            Assert.That(m.stamina.bar, Is.EqualTo(1));
            Mover n = Walker(On.With(true, true, true, true, false));
            Bar(n, 0.2);
            n.StaminaReward(On.kill);
            Assert.That(n.stamina.bar, Is.EqualTo(0.2), "the rewards' flag off");
            // a slam's landing
            var s = new Mover(Mc, Plane());
            s.SetStamina(On);
            s.Reset(0, 8, 1000); // 8 m up
            for (int i = 0; i < 3; i++) s.Step(new InputFrame(), Dt);
            Bar(s, 0.2);
            s.Step(new InputFrame { slide = new Button { held = true, pressed = true } }, Dt);
            Assert.That(s.state, Is.EqualTo(MoveState.Slam), $"the slam (y {s.position.y}, vy {s.velocity.y})");
            double before = s.stamina.bar;
            int k = 0;
            while (s.state == MoveState.Slam && k++ < 240) s.Step(new InputFrame(), Dt);
            Assert.That(s.stamina.bar, Is.EqualTo(before + On.slam).Within(1e-9), "a slam's landing adds 0.25 (the refill still waits its delay)");
        }

        [Test]
        public void Mover_RulesOff_IsTheWebsMoverBitForBit()
        {
            // a scripted run with boosts, held boosts, jumps and dash-jumps: no rules set, Neutral, and every rule but the bar
            foreach (Stamina.Params? rules in new Stamina.Params?[] { Stamina.Params.Neutral, On.With(false, true, true, true, true) })
            {
                Mover web = Walker(null);
                Mover m = Walker(rules);
                for (int i = 0; i < 1500; i++)
                {
                    bool press = i % 97 == 0;
                    var f = new InputFrame
                    {
                        moveZ = 1,
                        moveX = (i / 200) % 2 == 0 ? 0.3 : -0.3,
                        dash = new Button { held = i % 97 < 40, pressed = press },
                        jump = new Button { held = i % 151 == 5, pressed = i % 151 == 5 },
                    };
                    web.Step(f, Dt);
                    m.Step(f, Dt);
                    Assert.That(m.position.x, Is.EqualTo(web.position.x), "step " + i);
                    Assert.That(m.position.z, Is.EqualTo(web.position.z), "step " + i);
                    Assert.That(m.position.y, Is.EqualTo(web.position.y), "step " + i);
                    Assert.That(m.pips, Is.EqualTo(web.pips), "step " + i);
                    Assert.That(m.state, Is.EqualTo(web.state), "step " + i);
                }
                Assert.That(m.MaxPips, Is.EqualTo(Mc.dash.maxPips));
            }
        }

        [Test]
        public void Mover_BoostReady_PipsWithTheRulesOff_TheWholeCostWithTheBar()
        {
            Mover web = Walker(null);
            Assert.That(web.BoostReady(Mc.dash.pipCost + Mc.dashJump.extraPipCost, true), Is.True);
            web.pips = 1.5;
            Assert.That(web.BoostReady(Mc.dash.pipCost + Mc.dashJump.extraPipCost, true), Is.False);
            Mover m = Walker(On);
            Assert.That(m.BoostReady(Mc.dash.pipCost + Mc.dashJump.extraPipCost, true), Is.True);
            Bar(m, 0.7);
            Assert.That(m.BoostReady(Mc.dash.pipCost + Mc.dashJump.extraPipCost, true), Is.False, "a boost and its dash-jump need 0.75");
            Assert.That(m.BoostReady(Mc.dash.pipCost, false), Is.True);
            Assert.That(m.BoostReady(0, true), Is.True);
        }

        [Test]
        public void MoverState_ANight16SaveWithoutStamina_Loads_KeepingTheRunsRules()
        {
            Mover a = Walker(On);
            var old = new StateWriter(); // NIGHT16's writer: every field it knew (MoverState.Fields without stamina)
            foreach (string f in MoverState.Fields)
                if (f != "stamina") FieldSnapshot.WriteField(old, a, FieldSnapshot.Field(typeof(Mover), f), null);
            Assert.That(old.ToJson().Contains("\"stamina\""), Is.False);
            Mover b = Walker(On);
            Bar(b, 0.3);
            Assert.DoesNotThrow(() => new MoverState(b).Load(new StateReader(old.ToJson())));
            Assert.That(b.stamina.rules.Any, Is.True, "absent: the rules StartRun set");
            // a full save round-trips the bar
            var full = new StateWriter();
            new MoverState(b).Save(full);
            Mover c = Walker(null);
            new MoverState(c).Load(new StateReader(full.ToJson()));
            Assert.That(c.stamina.bar, Is.EqualTo(0.3));
            Assert.That(c.stamina.rules.cost, Is.EqualTo(On.cost));
        }
    }
}
