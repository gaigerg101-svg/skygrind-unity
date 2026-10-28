// NIGHT11 U3-24.3: THE SWARM ROUND'S BANNER AND DROP (Core/SwarmDrop.cs; new design: CoD's dog rounds end in a Max Ammo,
// design.zombies.swarmDrop). On: a swarm round (the web's isSwarmRound: 8, 13, 18 ...) opens with a banner and
// swarmRound, and its roundClear drops exactly one power-up at the round's last player kill, its type from the run's
// stream; a normal round drops nothing; a swarm round without a player kill drops nothing. Off: the web's swarm round.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class SwarmDropTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly PowerUps powerUps;
            public readonly SwarmDrop swarm;
            public readonly List<string> log = new List<string>();
            public readonly List<PowerUpEvent> spawns = new List<PowerUpEvent>();
            public string dropType;

            public Rig(GameConfig c, uint seed = 7)
            {
                powerUps = new PowerUps(c, events);
                powerUps.SeedRun(seed);
                swarm = new SwarmDrop(c, events, powerUps);
                events.On<BannerEvent>("banner", e => log.Add("banner " + e.text));
                events.On<SwarmRoundEvent>("swarmRound", e => log.Add("swarmRound " + e.round));
                events.On<SwarmRoundEvent>("swarmDrop", e =>
                {
                    log.Add($"swarmDrop {e.round} {e.type} at {e.position.x},{e.position.z}");
                    dropType = e.type;
                });
                events.On<PowerUpEvent>("powerUpSpawn", e => spawns.Add(e));
            }

            public void Begin(int n) => events.Emit("roundBegin", new RoundEvent { number = n });

            public void Clear(int n) => events.Emit("roundClear", new RoundEvent { number = n });

            public void Kill(double x, double z, string owner = "player") =>
                events.Emit("kill", new KillEvent { owner = owner, weapon = "revolver", kind = "swarmer", position = new Vec3(x, 0, z) });
        }

        [Test]
        public void TheFlag_ShipsOn_AndTheSwarmRoundsAreTheWebs()
        {
            Assert.That(SwarmDrop.Enabled(cfg), Is.True, "design.zombies.swarmDrop ships on");
            var r = new Rig(cfg);
            Assert.That(r.swarm.IsSwarmRound(8), Is.True);
            Assert.That(r.swarm.IsSwarmRound(13), Is.True);
            Assert.That(r.swarm.IsSwarmRound(7), Is.False);
            Assert.That(r.swarm.IsSwarmRound(10), Is.False, "round 10 (a boss round)");
            var noDesign = GameConfig.FromText(File.ReadAllText(Path.Combine(Port.Root, "config.json")), null);
            Assert.That(SwarmDrop.Enabled(noDesign), Is.False, "no design config: the web's swarm round");
        }

        [Test]
        public void On_ASwarmRoundOpensWithABanner_AndItsLastKillDropsOnePowerUp()
        {
            var r = new Rig(cfg);
            r.Begin(8);
            Assert.That(r.log, Is.EqualTo(new[] { "banner " + SwarmDrop.BannerText, "swarmRound 8" }));
            r.Kill(3, 4);
            r.Kill(-10, 12.5);
            r.Kill(30, 30, owner: "rival"); // not the player's: not the last kill
            int before = r.spawns.Count;
            r.Clear(8);
            Assert.That(r.spawns.Count - before, Is.EqualTo(1), "exactly one power-up");
            PowerUpEvent drop = r.spawns[r.spawns.Count - 1];
            Assert.That(drop.position.x, Is.EqualTo(-10));
            Assert.That(drop.position.z, Is.EqualTo(12.5), "at the last player kill");
            Assert.That(PowerUps.Types, Does.Contain(drop.type));
            Assert.That(r.dropType, Is.EqualTo(drop.type));
            Assert.That(r.log[r.log.Count - 1], Is.EqualTo($"swarmDrop 8 {drop.type} at -10,12.5"));
            Assert.That(r.swarm.drops, Is.EqualTo(1));
            r.Clear(8);
            Assert.That(r.swarm.drops, Is.EqualTo(1), "once per swarm round");
        }

        [Test]
        public void ANormalRound_AndASwarmRoundWithoutAPlayerKill_DropNothing()
        {
            var r = new Rig(cfg);
            r.Begin(7);
            r.Kill(1, 1);
            r.Clear(7);
            Assert.That(r.log, Is.Empty, "round 7: no banner, no drop");
            Assert.That(r.spawns, Is.Empty);
            r.Begin(13);
            r.Kill(1, 1, owner: "rival");
            r.Clear(13);
            Assert.That(r.log, Is.EqualTo(new[] { "banner " + SwarmDrop.BannerText, "swarmRound 13" }), "no player kill: no drop");
            Assert.That(r.spawns, Is.Empty);
        }

        [Test]
        public void TheType_ComesFromTheRunsStream_SameSeedSameType()
        {
            var types = new List<string>();
            for (uint seed = 1; seed <= 12; seed++)
            {
                var a = new Rig(cfg, seed);
                var b = new Rig(cfg, seed);
                foreach (Rig r in new[] { a, b })
                {
                    r.Begin(18);
                    r.Kill(2, 2);
                    r.Clear(18);
                }
                Assert.That(a.dropType, Is.EqualTo(b.dropType), $"seed {seed}");
                types.Add(a.dropType);
            }
            Assert.That(new HashSet<string>(types).Count, Is.GreaterThan(1), "the stream draws different types across seeds");
        }

        [Test]
        public void Off_NothingHappens()
        {
            var off = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            PortData.Obj(PortData.Obj(off.root["design"])["zombies"])["swarmDrop"] = false;
            var r = new Rig(off);
            Assert.That(r.swarm.enabled, Is.False);
            r.Begin(8);
            r.Kill(3, 4);
            r.Clear(8);
            Assert.That(r.log, Is.Empty, "no banner, no swarmRound, no swarmDrop");
            Assert.That(r.spawns, Is.Empty, "no power-up");
            Assert.That(r.events.Emitted.ContainsKey("swarmRound"), Is.False);
        }
    }
}
