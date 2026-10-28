// NIGHT11 U3-24.9: ULTRA HEALTH, the LAB toggle (Core/UltraHealth.cs; new design: ULTRAKILL's hard damage and blood heal,
// design.lab.ultraHealth). Off: heals and regen fill to max as on the web, hits heal nothing. On: 35% of a hit is hard
// damage (a 20 HP hit: 7), heals and regen stop at max - hard, the timer (1 s + damage / 20 s) then the drain at 15 HP/s,
// 40 at AIRBORNE, 60 at SAVAGE, a wipe at SKYGRIND; the blood heal per weapon within 4 m (none beyond); a parry heals to
// the cap. A run with the toggle is a LAB run (Lab.Any, its name on the results; the Game never stores it as a best).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class UltraHealthTests
    {
        static GameConfig cfg;
        static double dt;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            dt = cfg.Dt;
        }

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly Health health;
            public readonly Regen regen;
            public readonly UltraHealth ultra;
            public Vec3 player = new Vec3(0, 0, 0);
            public int rank;

            public Rig(bool on)
            {
                health = new Health(100, events, "player");
                regen = new Regen(cfg, events, health, () => 0);
                ultra = new UltraHealth(cfg, events, health, () => player, () => rank);
                ultra.enabled = on;
            }

            public void Hit(string weapon, double distance, bool headshot = false, string owner = "player", bool shock = false) =>
                events.Emit("hit", new HitEvent { owner = owner, weapon = weapon, kind = "husk", damage = 10, headshot = headshot, shock = shock, position = new Vec3(player.x + distance, player.y, player.z) });

            public void Steps(int n)
            {
                for (int i = 0; i < n; i++) ultra.Step(dt);
            }
        }

        static int Steps(double seconds) => MovementMath.StepsFor(seconds, dt);

        [Test]
        public void TheToggle_IsALabRow_OffByDefault_AndALabRunIsStamped()
        {
            var lab = new Lab(cfg);
            Lab.Toggle t = lab.toggles.Find(x => x.key == LabMath.UltraHealthKey);
            Assert.That(t, Is.Not.Null, "design.lab.toggles has lab.ultraHealth");
            Assert.That(t.name, Is.EqualTo("ULTRA HEALTH"));
            Assert.That(t.on, Is.False, "off by default (D-18 stays GG's)");
            lab.Read(k => k == LabMath.UltraHealthKey);
            Assert.That(lab.ultraHealth, Is.True);
            Assert.That(lab.Any, Is.True, "a LAB run: the Game judges it but never stores it as a best");
            Assert.That(lab.Names(), Is.EqualTo("ULTRA HEALTH"), "the results' LAB line");
            var r = new Rig(false);
            lab.Apply(cfg, r.regen, null, null, null, r.ultra);
            Assert.That(r.ultra.enabled, Is.True, "Apply switches it on for the run");
            lab.Read(null);
            lab.Apply(cfg, r.regen, null, null, null, r.ultra);
            Assert.That(r.ultra.enabled, Is.False, "null: the web's game");
            Assert.That(lab.Any, Is.False);
        }

        [Test]
        public void Off_HealsAndRegenFillToMax_HitsHealNothing()
        {
            var r = new Rig(false);
            Assert.That(r.health.Damage(20, "husk"), Is.True);
            Assert.That(r.health.hard, Is.EqualTo(0), "no hard damage");
            r.Hit("punch", 1);
            Assert.That(r.health.hp, Is.EqualTo(80), "no blood heal");
            Assert.That(r.health.Heal(100), Is.EqualTo(20));
            Assert.That(r.health.hp, Is.EqualTo(100), "the web's cap");
            r.health.hp = 50;
            r.regen.SetEnabled(true);
            for (int i = 0; i < Steps(10); i++) r.regen.Step(dt);
            Assert.That(r.health.hp, Is.EqualTo(100), "regen fills to max");
            r.events.Emit("parry", new HitEvent { owner = "player", weapon = "punch" });
            Assert.That(r.health.hp, Is.EqualTo(100));
        }

        [Test]
        public void On_AHitLeaves35PercentHardDamage_HealsAndRegenStopAtTheCap()
        {
            var r = new Rig(true);
            r.health.Damage(20, "husk");
            Assert.That(r.health.hp, Is.EqualTo(80));
            Assert.That(r.health.hard, Is.EqualTo(7).Within(1e-12), "35% of 20");
            Assert.That(r.ultra.Cap, Is.EqualTo(93).Within(1e-12));
            Assert.That(r.health.Heal(100), Is.EqualTo(13).Within(1e-12), "a heal fills only to max - hard");
            Assert.That(r.health.hp, Is.EqualTo(93).Within(1e-12));
            r.health.hp = 60;
            r.regen.SetEnabled(true); // no delay left: it climbs at once
            for (int i = 0; i < Steps(10); i++) r.regen.Step(dt);
            Assert.That(r.health.hp, Is.EqualTo(93).Within(1e-9), "regen too");
            Assert.That(r.regen.Regenerating, Is.False, "at the cap regen stops");
            // Hard damage never takes more than the room hp left
            r.health.hp = 10;
            r.health.hard = 0;
            r.health.Damage(9, "husk");
            Assert.That(r.health.hard, Is.EqualTo(0.35 * 9).Within(1e-12));
            r.health.Reset();
            Assert.That(r.health.hard, Is.EqualTo(0), "Health.Reset clears it");
        }

        [Test]
        public void TheTimer_ThenTheDrain_15PerS_FasterAtAirborneAndSavage_WipedAtSkygrind()
        {
            var r = new Rig(true);
            r.health.Damage(20, "husk");
            int timer = Steps(1 + 20.0 / 20); // 1 s + damage / 20 s
            Assert.That(r.ultra.TimerLeft, Is.EqualTo(timer * dt).Within(1e-9));
            r.Steps(timer - 1);
            Assert.That(r.health.hard, Is.EqualTo(7).Within(1e-12), "nothing drains while the timer runs");
            r.Steps(1); // the timer's last step
            r.Steps(Steps(0.2));
            Assert.That(r.health.hard, Is.EqualTo(7 - 15 * Steps(0.2) * dt).Within(1e-9), "15 HP/s");
            // A second hit restarts the timer from 1 s (it had run out) and adds its damage / 20
            r.health.Damage(10, "husk");
            Assert.That(r.ultra.TimerLeft, Is.EqualTo(Steps(1 + 10.0 / 20) * dt).Within(1e-9));
            r.health.Damage(10, "husk");
            Assert.That(r.ultra.TimerLeft, Is.EqualTo(Steps(Steps(1 + 10.0 / 20) * dt + 10.0 / 20) * dt).Within(1e-9), "a hit while it runs adds damage / 20");

            double DrainAt(int rank, double seconds)
            {
                var x = new Rig(true) { rank = rank };
                x.health.Damage(20, "husk");
                int t = Steps(2);
                int rate = rank >= 4 ? 3 : rank >= 3 ? 2 : 1; // the timer runs x2 at AIRBORNE, x3 at SAVAGE
                x.Steps((t + rate - 1) / rate);
                Assert.That(x.ultra.TimerLeft, Is.EqualTo(0), $"rank {rank}: the timer ran out");
                x.Steps(Steps(seconds));
                return 7 - x.health.hard;
            }
            Assert.That(DrainAt(2, 0.1), Is.EqualTo(15 * Steps(0.1) * dt).Within(1e-9), "BOLD: 15");
            Assert.That(DrainAt(3, 0.1), Is.EqualTo(40 * Steps(0.1) * dt).Within(1e-9), "AIRBORNE: 40");
            Assert.That(DrainAt(4, 0.1), Is.EqualTo(60 * Steps(0.1) * dt).Within(1e-9), "SAVAGE: 60");
            var w = new Rig(true) { rank = 5 };
            w.health.Damage(20, "husk");
            w.Steps(1);
            Assert.That(w.health.hard, Is.EqualTo(0), "SKYGRIND wipes it at once");
            Assert.That(w.ultra.TimerLeft, Is.EqualTo(0));
        }

        [Test]
        public void TheBloodHeal_PerWeapon_Within4m_NoneBeyond()
        {
            var r = new Rig(true);
            double Heal(string weapon, double distance, bool headshot = false, string owner = "player", bool shock = false)
            {
                r.health.hp = 10;
                r.Hit(weapon, distance, headshot, owner, shock);
                return r.health.hp - 10;
            }
            Assert.That(Heal("punch", 3), Is.EqualTo(20));
            Assert.That(Heal("gauntlet", 3), Is.EqualTo(20), "PULSE GAUNTLET");
            Assert.That(Heal("revolver", 3), Is.EqualTo(10), "KESTREL");
            Assert.That(Heal("revolver", 3, headshot: true), Is.EqualTo(30), "KESTREL headshot");
            Assert.That(Heal("ricochet", 3), Is.EqualTo(10), "KESTREL's ricochet");
            Assert.That(Heal("shotgun", 3), Is.EqualTo(10), "THUNDERCLAP, per enemy hit");
            Assert.That(Heal("nailgun", 3), Is.EqualTo(1), "STITCHER, per nail");
            Assert.That(Heal("rail", 3), Is.EqualTo(30), "LONGWAVE");
            Assert.That(Heal("rocket", 3), Is.EqualTo(0), "COMET's blasts");
            Assert.That(Heal("burn", 3), Is.EqualTo(0), "secondary damage");
            Assert.That(Heal("punch", 3, shock: true), Is.EqualTo(0), "an impact wave's share");
            Assert.That(Heal("punch", 4), Is.EqualTo(20), "4 m is in range");
            Assert.That(Heal("punch", 4.01), Is.EqualTo(0), "beyond 4 m: nothing");
            Assert.That(Heal("rail", 30), Is.EqualTo(0));
            Assert.That(Heal("punch", 1, owner: "rival"), Is.EqualTo(0), "the player's hits only");
            // The cap holds for blood too
            r.health.hp = 80;
            r.health.hard = 15;
            r.Hit("rail", 1);
            Assert.That(r.health.hp, Is.EqualTo(85), "max - hard");
            Assert.That(r.ultra.bloodHealed, Is.GreaterThan(0));
        }

        [Test]
        public void TheParry_HealsToTheCap()
        {
            var r = new Rig(true);
            r.health.Damage(20, "husk");
            r.health.hp = 50;
            r.events.Emit("parry", new HitEvent { owner = "player", weapon = "punch", kind = "husk" });
            Assert.That(r.health.hp, Is.EqualTo(93).Within(1e-12), "max - hard");
            r.events.Emit("parry", new HitEvent { owner = "player", weapon = "punch", kind = "husk" });
            Assert.That(r.health.hp, Is.EqualTo(93).Within(1e-12), "no further");
        }

        [Test]
        public void TheStep_AllocatesNothing()
        {
            var r = new Rig(true);
            r.health.Damage(20, "husk");
            r.Steps(10); // warm the delegates
            long allocs = AllocCount.Of(() => r.Steps(600)); // the timer, then the drain
            Assert.That(r.health.hard, Is.LessThan(7), "it drained");
            Assert.That(allocs, Is.EqualTo(0));
        }
    }
}
