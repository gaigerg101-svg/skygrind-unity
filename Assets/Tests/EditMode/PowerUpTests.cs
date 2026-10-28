// NIGHT8 U2-12: kill drops and power-ups. powerUpMath.js has no web tests; these check its documented shape (none
// inside minGap, the base chance, x2 after rise, x4 after rise2, x eliteMult, the cap; the chain and air style drops
// and their cooldown), each type's effect and timer as powerUps.js runs them, and that a run's seed decides its drops.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class PowerUpTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static PowerUpMath.KillDrop K() => new PowerUpMath.KillDrop
        {
            enabled = cfg.Bool("powerUps.killDrop.enabled"), chance = cfg.Num("powerUps.killDrop.chance"), eliteMult = cfg.Num("powerUps.killDrop.eliteMult"),
            rise = cfg.Num("powerUps.killDrop.rise"), rise2 = cfg.Num("powerUps.killDrop.rise2"), minGap = cfg.Num("powerUps.killDrop.minGap"),
            maxChance = cfg.Num("powerUps.killDrop.maxChance"),
        };

        [Test]
        public void KillDropChance_RisesWhileNoneDrops_AndIsCapped()
        {
            var k = K();
            double c = k.chance;
            Assert.That(PowerUpMath.KillDropChance(k.minGap - 0.1, k), Is.EqualTo(0), "none inside minGap");
            Assert.That(PowerUpMath.KillDropChance(k.minGap, k), Is.EqualTo(c));
            Assert.That(PowerUpMath.KillDropChance(k.rise, k), Is.EqualTo(c * 2));
            Assert.That(PowerUpMath.KillDropChance(k.rise2, k), Is.EqualTo(c * 4));
            Assert.That(PowerUpMath.KillDropChance(k.rise2, k, elite: true), Is.EqualTo(c * 4 * k.eliteMult));
            Assert.That(PowerUpMath.KillDropChance(k.rise2, k, mult: 100), Is.EqualTo(k.maxChance));
            k.enabled = false;
            Assert.That(PowerUpMath.KillDropChance(k.rise2, k), Is.EqualTo(0));
        }

        [Test]
        public void StyleDrop_AChainOrAnAirMultikill_ThenTheCooldown()
        {
            var c = new PowerUpMath.StyleDrop
            {
                chainKills = cfg.Num("powerUps.styleDrop.chainKills"), chainWindow = cfg.Num("powerUps.styleDrop.chainWindow"), airKills = cfg.Num("powerUps.styleDrop.airKills"),
                airWindow = cfg.Num("powerUps.styleDrop.airWindow"), cooldown = cfg.Num("powerUps.styleDrop.cooldown"),
            };
            var s = new PowerUpMath.StyleDropState();
            string got = null;
            for (int i = 0; i < c.chainKills; i++) got = PowerUpMath.StyleDropOnKill(s, 1 + i * 0.5, false, c);
            Assert.That(got, Is.EqualTo("chain"));
            for (int i = 0; i < c.chainKills; i++) got = PowerUpMath.StyleDropOnKill(s, 10 + i * 0.5, false, c);
            Assert.That(got, Is.Null, "cooling down");
            double t = 1 + (c.chainKills - 1) * 0.5 + c.cooldown + 1; // the cooldown runs from the chain's last kill
            got = PowerUpMath.StyleDropOnKill(s, t, true, c);
            got = PowerUpMath.StyleDropOnKill(s, t + 0.2, false, c); // a grounded kill breaks the air chain
            got = PowerUpMath.StyleDropOnKill(s, t + 0.4, true, c);
            Assert.That(got, Is.Null);
            for (int i = 1; i < c.airKills; i++) got = PowerUpMath.StyleDropOnKill(s, t + 0.4 + i * 0.3, true, c);
            Assert.That(got, Is.EqualTo("air"));
        }

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly Economy economy;
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly PowerUps pu;
            public Vec3 player = new Vec3(0, 0, 0);
            public bool stasis, overclock;
            public readonly List<string> log = new List<string>();

            public Rig()
            {
                economy = new Economy(cfg, events);
                economy.Start(5000);
                pu = new PowerUps(cfg, events)
                {
                    playerPos = () => player, economy = economy, enemies = enemies,
                    kill = (e, d, at) => e.Damage(d, 0), setStasis = on => stasis = on, setOverclock = on => overclock = on,
                };
                foreach (string t in new[] { "powerUpSpawn", "powerUpGrab", "powerUpEnd", "powerShockwave" }) events.On(t, o => log.Add($"{t} {((PowerUpEvent)o).type}"));
            }

            public void Grab(string type)
            {
                pu.Spawn(new Vec3(5, 0, 0), type);
                player = new Vec3(5, 0, 0);
                pu.Step();
                player = new Vec3(0, 0, 0);
            }

            public void Run(double seconds)
            {
                for (int i = 0; i < MovementMath.StepsFor(seconds, cfg.Dt); i++) pu.Step();
            }
        }

        [Test]
        public void DoublePoints_Stasis_Overclock_RunForTheirTime()
        {
            var r = new Rig();
            r.Grab("double");
            Assert.That(r.economy.boost, Is.EqualTo(cfg.Num("powerUps.types.double.mult")));
            r.Grab("stasis");
            Assert.That(r.stasis, Is.True);
            r.Grab("overclock");
            Assert.That(r.overclock, Is.True);
            r.Run(cfg.Num("powerUps.types.stasis.time") + 0.1);
            Assert.That(r.stasis, Is.False, "STASIS ends after its time");
            Assert.That(r.economy.boost, Is.EqualTo(cfg.Num("powerUps.types.double.mult")), "DOUBLE POINTS still running");
            r.Run(cfg.Num("powerUps.types.double.time"));
            Assert.That(r.economy.boost, Is.EqualTo(1));
            Assert.That(r.overclock, Is.False);
            Assert.That(r.log, Does.Contain("powerUpEnd double"));
        }

        [Test]
        public void Shockwave_KillsTheNonElites_Refund_HalvesTheNextBuy()
        {
            var r = new Rig();
            var world = new TriangleWorld();
            world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
            var husks = new Husks(cfg, r.events, r.enemies, world, -30);
            Husk h = husks.Spawn("husk", new Vec3(3, 0, -3), r.player);
            Husk s = husks.Spawn("spitter", new Vec3(-3, 0, -3), r.player);
            r.Grab("shockwave");
            Assert.That(h.alive, Is.False, "a husk dies");
            Assert.That(s.alive, Is.True, "an elite (spitter) stands");
            Assert.That(r.pu.shockwaveKills, Is.EqualTo(1));
            r.Grab("refund");
            Assert.That(r.economy.Price(1000), Is.EqualTo(1000 * cfg.Num("powerUps.types.refund.mult")));
            Assert.That(r.economy.Spend(r.economy.Price(1000), "test"), Is.True);
            Assert.That(r.economy.Price(1000), Is.EqualTo(1000), "the discount went into that purchase");
            Assert.That(r.log, Does.Contain("powerUpEnd refund"));
        }

        [Test]
        public void TheRunSeed_DecidesTheKillDrops()
        {
            List<string> Drops(uint seed)
            {
                var r = new Rig();
                r.pu.SeedRun(seed);
                var got = new List<string>();
                r.events.On<PowerUpEvent>("powerUpSpawn", e => got.Add(e.type));
                for (int i = 0; i < 2400; i++)
                {
                    r.pu.Step();
                    if (i % 30 == 0) r.events.Emit("kill", new KillEvent { kind = "husk", position = new Vec3(40, 0, 40) });
                }
                return got;
            }
            List<string> a = Drops(1234), b = Drops(1234);
            Assert.That(a.Count, Is.GreaterThan(0), "20 s of kills drops something");
            Assert.That(b, Is.EqualTo(a), "the same seed, the same drops");
        }
    }
}
