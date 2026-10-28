// NIGHT19 U3-33.7 (builder E owns this file; the shared commit's empty fixture): design.stamina.n19 in the sim (S11). With
// the block on: the sprint drains design.stamina.n19.drain (0.3 bar/s: a full bar sprints 3.0-3.6 s), a kill within
// nearRange m of the player's eye refills killNear (>= 0.25), one beyond farRange m killFar (<= 0.1), one between them
// rewards.kill; a slam's landing refills rewards.slam only when an enemy is within the slam's shockwave (slam.
// shockwaveRadius); a parry still refills the whole bar; one bar at the start, VIGOR one more. With the block off every
// number is NIGHT18's (sprint.drain, the flat kill reward, the slam's landing always). The config: PortExport's
// config.json and Design/unity-config.json beside it.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class StaminaN19Tests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static double Dt => 1.0 / Cfg.movement.loop.simHz;

        /// <summary>A run with only the stamina block on (on: true) or every block off, settled on the floor at the spawn.</summary>
        static SurvivalSim Run(bool on)
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg, Map());
            sim.noRecords = sim.noLab = true;
            sim.n19Override = on ? new N19Blocks { stamina = true } : N19Blocks.Off;
            sim.StartRun(1, null, null);
            for (int i = 0; i < 60; i++) sim.mover.Step(new InputFrame { yaw = 0 }, Dt);
            return sim;
        }

        /// <summary>The bar set with the refill's delay just started (no passive refill on the next steps).</summary>
        static void Bar(Mover m, double bar)
        {
            m.stamina.bar = bar;
            m.stamina.sinceSpend = 0;
            m.stamina.full = bar >= m.StaminaMax;
        }

        static Vec3 Eye(SurvivalSim sim) => new Vec3(sim.mover.position.x, sim.mover.position.y + Cfg.movement.body.eyeHeight, sim.mover.position.z);

        /// <summary>The player's kill `metres` from the eye (straight along +x), as the loadout emits it.</summary>
        static void KillAt(SurvivalSim sim, double metres)
        {
            Vec3 eye = Eye(sim);
            sim.events.Emit("kill", new KillEvent { owner = "player", weapon = "revolver", kind = "husk", enemy = 999, position = new Vec3(eye.x + metres, eye.y, eye.z), distance = metres });
        }

        // --- the drain ------------------------------------------------------------------------------------------------

        [Test]
        public void TheDrain_IsTheBlocksWithItOn_AndNight18sWithItOff()
        {
            Assert.That(Run(true).mover.stamina.rules.drain, Is.EqualTo(Cfg.Num("design.stamina.n19.drain")));
            Assert.That(Run(false).mover.stamina.rules.drain, Is.EqualTo(Cfg.Num("design.stamina.sprint.drain")));
        }

        [Test]
        public void AFullBar_SprintsBetween3_0And3_6Seconds_WithTheBlockOn()
        {
            SurvivalSim sim = Run(true);
            Mover m = sim.mover;
            Assert.That(m.stamina.rules.ownKey, Is.True, "the block's own sprint key (the lead's N19StaminaStartRun)");
            Assert.That(m.stamina.bar, Is.EqualTo(m.StaminaMax), "a full bar at the start");
            int steps = 0, sprintSteps = 0;
            while (steps++ < 1200)
            {
                m.Step(new InputFrame { moveZ = 1, yaw = 0, sprint = new Button { held = true, pressed = steps == 1 } }, Dt);
                if (m.stamina.sprintStep) sprintSteps++;
                if (m.stamina.bar <= 0) break;
            }
            double seconds = sprintSteps * Dt;
            Assert.That(seconds, Is.InRange(3.0, 3.6), $"a full bar sprinted {seconds:0.###} s");
        }

        // --- the kill rewards by range ----------------------------------------------------------------------------------

        [Test]
        public void AKillWithinNearRange_RefillsKillNear_AtLeastAQuarterBar()
        {
            SurvivalSim sim = Run(true);
            Bar(sim.mover, 0.2);
            KillAt(sim, Cfg.Num("design.stamina.n19.nearRange") - 1);
            double got = sim.mover.stamina.bar - 0.2;
            Assert.That(got, Is.EqualTo(Cfg.Num("design.stamina.n19.killNear")).Within(1e-12));
            Assert.That(got, Is.GreaterThanOrEqualTo(0.25), "S11: a kill within 6 m refills >= 0.25");
        }

        [Test]
        public void AKillBeyondFarRange_RefillsKillFar_AtMostATenth()
        {
            SurvivalSim sim = Run(true);
            Bar(sim.mover, 0.2);
            KillAt(sim, Cfg.Num("design.stamina.n19.farRange") + 2);
            double got = sim.mover.stamina.bar - 0.2;
            Assert.That(got, Is.EqualTo(Cfg.Num("design.stamina.n19.killFar")).Within(1e-12));
            Assert.That(got, Is.LessThanOrEqualTo(0.1), "S11: a kill beyond 15 m refills <= 0.1");
        }

        [Test]
        public void AKillBetweenTheRanges_RefillsTheFlatKillReward()
        {
            SurvivalSim sim = Run(true);
            Bar(sim.mover, 0.2);
            KillAt(sim, (Cfg.Num("design.stamina.n19.nearRange") + Cfg.Num("design.stamina.n19.farRange")) / 2);
            Assert.That(sim.mover.stamina.bar - 0.2, Is.EqualTo(Cfg.Num("design.stamina.rewards.kill")).Within(1e-12));
        }

        [Test]
        public void WithTheBlockOff_EveryKillRefillsTheFlatReward_WhateverTheRange()
        {
            foreach (double metres in new[] { 2.0, 10.0, 30.0 })
            {
                SurvivalSim sim = Run(false);
                Bar(sim.mover, 0.2);
                KillAt(sim, metres);
                Assert.That(sim.mover.stamina.bar - 0.2, Is.EqualTo(Cfg.Num("design.stamina.rewards.kill")).Within(1e-12), $"{metres} m");
            }
        }

        [Test]
        public void AnotherOwnersKill_RefillsNothing()
        {
            SurvivalSim sim = Run(true);
            Bar(sim.mover, 0.2);
            Vec3 eye = Eye(sim);
            sim.events.Emit("kill", new KillEvent { owner = "rival", weapon = "revolver", kind = "husk", enemy = 999, position = new Vec3(eye.x + 2, eye.y, eye.z) });
            Assert.That(sim.mover.stamina.bar, Is.EqualTo(0.2));
        }

        // --- the slam ---------------------------------------------------------------------------------------------------

        /// <summary>A slam from 8 m above the spawn (with a husk standing `huskAt` m beside the landing, or none): the bar's gain.</summary>
        static double SlamGain(bool on, double? huskAt)
        {
            SurvivalSim sim = Run(on);
            Mover m = sim.mover;
            Vec3 ground = m.position;
            if (huskAt.HasValue)
            {
                Husk h = sim.husks.Spawn("husk", new Vec3(ground.x + huskAt.Value, ground.y, ground.z), ground);
                Assert.That(h, Is.Not.Null, "a husk fits beside the spawn");
            }
            m.Reset(ground.x, ground.y + 8, ground.z);
            for (int i = 0; i < 3; i++) m.Step(new InputFrame(), Dt);
            m.Step(new InputFrame { slide = new Button { held = true, pressed = true } }, Dt);
            Assert.That(m.state, Is.EqualTo(MoveState.Slam), "the slam");
            Bar(m, 0.2);
            int k = 0;
            while (m.state == MoveState.Slam && k++ < 240) m.Step(new InputFrame(), Dt);
            return m.stamina.bar - 0.2;
        }

        [Test]
        public void ASlamThatHitsNothing_RefillsNothing_WithTheBlockOn()
        {
            Assert.That(SlamGain(true, null), Is.EqualTo(0).Within(1e-12), "slamOnHit: no enemy within the shockwave");
        }

        [Test]
        public void ASlamThatHits_RefillsTheSlamReward_WithTheBlockOn()
        {
            Assert.That(SlamGain(true, 2), Is.EqualTo(Cfg.Num("design.stamina.rewards.slam")).Within(1e-12), "a husk 2 m from the landing");
        }

        [Test]
        public void ASlam_AlwaysRefills_WithTheBlockOff()
        {
            Assert.That(SlamGain(false, null), Is.EqualTo(Cfg.Num("design.stamina.rewards.slam")).Within(1e-12), "NIGHT18: every slam's landing");
        }

        // --- the bar ----------------------------------------------------------------------------------------------------

        [Test]
        public void OneBarAtTheStart_VigorAddsOne_AndAParryRefillsTheWholeBar()
        {
            SurvivalSim sim = Run(true);
            Mover m = sim.mover;
            Assert.That(m.StaminaMax, Is.EqualTo(1), "one bar at the start: start with less");
            m.bonusPips = 1; // VIGOR
            Assert.That(m.StaminaMax, Is.EqualTo(2), "VIGOR: one bar more");
            m.bonusPips = 0;
            Bar(m, 0.1);
            sim.events.Emit("parry", new HitEvent { owner = "player", weapon = "punch", kind = "husk", enemy = 999 });
            Assert.That(m.stamina.bar, Is.EqualTo(m.StaminaMax).Within(1e-12), "a parry: +1, the whole bar");
        }
    }
}
