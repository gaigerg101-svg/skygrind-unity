// NIGHT7 if-time 3: the SWARMER (swarmer.js on enemyKind.js) on a floor: it closes, crouches for its tell, pounces and
// bites once for swarmer.damage, then recovers (the cooldown holds the next one); in your front cone it never pounces
// (the brain's flankNow); it has no head (a body shot, no headshot) and the KESTREL kills it in one; rounds from 3 deal
// swarmer squads and the swarm rounds deal only those.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class SwarmerTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        sealed class Rig
        {
            public EventBus events = new EventBus();
            public EnemyRegistry enemies = new EnemyRegistry();
            public TriangleWorld world = new TriangleWorld();
            public Health health;
            public Husks pool;
            public readonly List<string> log = new List<string>();

            public Rig()
            {
                world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
                health = new Health(100, events, "player");
                pool = new Husks(cfg, events, enemies, world, -30);
                events.On("swarmerCrouch", _ => log.Add("crouch"));
                events.On("swarmerPounce", _ => log.Add("pounce"));
                events.On<HurtEvent>("hurt", e => log.Add($"hurt {e.amount}"));
            }

            public void Run(Vec3 player, double seconds)
            {
                for (int i = 0; i < MovementMath.StepsFor(seconds, cfg.Dt); i++) pool.Step(player, health, cfg.Dt);
            }
        }

        [Test]
        public void Swarmer_Closes_Crouches_PouncesAndBitesOnce()
        {
            var r = new Rig();
            Vec3 player = new Vec3(0, 0, 0);
            Husk s = r.pool.Spawn("swarmer", new Vec3(0, 0, 9), player);
            Assert.That(s, Is.InstanceOf<Swarmer>());
            Assert.That(s.hp, Is.EqualTo(cfg.Num("swarmer.hp")));
            r.Run(player, 1.5);
            string log = string.Join(", ", r.log);
            Assert.That(r.log.FindAll(x => x == "crouch").Count, Is.EqualTo(1), log);
            Assert.That(r.log.IndexOf("crouch"), Is.LessThan(r.log.IndexOf("pounce")), log);
            Assert.That(r.health.hp, Is.EqualTo(100 - cfg.Num("swarmer.damage")), "one bite: " + log);
            // The cooldown runs from the crouch (~0.45 s in): one more pounce in the next cooldown's span, not two.
            r.Run(player, cfg.Num("swarmer.cooldown"));
            Assert.That(r.log.FindAll(x => x == "crouch").Count, Is.EqualTo(2), "after the cooldown it goes again: " + string.Join(", ", r.log));
            Assert.That(r.health.hp, Is.EqualTo(100 - 2 * cfg.Num("swarmer.damage")));
        }

        [Test]
        public void Swarmer_InYourFrontCone_NeverPounces()
        {
            var r = new Rig();
            Vec3 player = new Vec3(0, 0, 0);
            Husk s = r.pool.Spawn("swarmer", new Vec3(0, 0, -3), player);
            s.ai = new HuskAi { attack = true, flankNow = _ => false };
            r.Run(player, 2);
            Assert.That(r.log, Is.Empty, "in front: no crouch");
            s.ai.flankNow = _ => true;
            r.Run(player, 0.5);
            Assert.That(r.log, Does.Contain("crouch"), "on your flank it pounces");
            s.ai.attack = false;
            r.log.Clear();
            r.Run(player, 3);
            Assert.That(r.log.FindAll(x => x == "crouch"), Is.Empty, "without a swarm token: no pounce");
        }

        [Test]
        public void Swarmer_HasNoHead_TheKestrelKillsItInOne()
        {
            var r = new Rig();
            var mover = new Mover(cfg.movement, r.world);
            mover.Reset(0, 0, 0);
            Vec3 eye = new Vec3(0, 1.6, 0), dir = default;
            var loadout = new Loadout(cfg, r.events, r.enemies, r.world, mover, r.health, () => (eye, dir));
            Husk s = r.pool.Spawn("swarmer", new Vec3(0, 0, -6), new Vec3(0, 0, 0));
            Assert.That(s.RayHit(eye, Vec3.Normalize(s.AimPoint - eye), out bool head), Is.GreaterThan(0));
            Assert.That(head, Is.False, "no head sphere");
            var kills = new List<KillEvent>();
            r.events.On<KillEvent>("kill", e => kills.Add(e));
            dir = Vec3.Normalize(s.AimPoint - eye);
            loadout.Step(new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } }, cfg.Dt);
            Assert.That(s.alive, Is.False);
            Assert.That(kills.Count, Is.EqualTo(1));
            Assert.That(kills[0].kind, Is.EqualTo("swarmer"));
            Assert.That(kills[0].headshot, Is.False);
        }

        [Test]
        public void Rounds_DealSwarmersFromRound3_SwarmRoundsOnlySwarmers()
        {
            DirectorMath.D d = DirectorMath.From(cfg);
            for (int round = 1; round <= 2; round++)
            {
                foreach (var squad in DirectorMath.PlanSquads(round, new Mulberry32(5).Next, d, Director.Available))
                    Assert.That(squad, Has.None.EqualTo("swarmer"), $"round {round}");
            }
            int swarmerSquads = 0;
            for (uint seed = 1; seed <= 16; seed++)
            {
                foreach (var squad in DirectorMath.PlanSquads(3, new Mulberry32(seed).Next, d, Director.Available))
                {
                    Assert.That(squad, Has.All.Matches<string>(Director.Available));
                    if (squad.Contains("swarmer")) swarmerSquads++;
                }
            }
            Assert.That(swarmerSquads, Is.GreaterThan(0), "round 3 deals swarmer squads");
            int swarm = 3;
            while (!DirectorMath.IsSwarmRound(swarm, d)) swarm++;
            foreach (var squad in DirectorMath.PlanSquads(swarm, new Mulberry32(9).Next, d, Director.Available))
                Assert.That(squad, Has.All.EqualTo("swarmer"), $"swarm round {swarm}");
        }
    }
}
