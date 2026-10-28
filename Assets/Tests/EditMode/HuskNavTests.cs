// NIGHT7 S6: Husks with the Survival brain on the real map (the web's exported nav grid, every zone open). A husk spawned
// far away in another zone reaches the player (flow field, nav jumps, the watchdog), and the melee token pool holds:
// at most ai.tokens.base.melee husks wind up at once.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class HuskNavTests
    {
        static GameConfig cfg;
        static SurvivalMap map;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
        }

        sealed class Sim
        {
            public EventBus events = new EventBus();
            public EnemyRegistry enemies = new EnemyRegistry();
            public GameWorld world;
            public Zones zones;
            public NavField nav;
            public HuskBrain brain;
            public Husks husks;
            public Health health;

            public Sim()
            {
                world = new GameWorld(map.BuildOctree(), map.CreateCore(cfg.movement));
                zones = new Zones(cfg, map, world, events);
                zones.OpenAll();
                nav = new NavField(cfg, NavGrid.FromExport(map.navGrid));
                nav.SetBlocked(zones.LockedTag);
                brain = new HuskBrain(cfg, events, nav, world.Geometry);
                brain.onScreen = _ => false; // headless: the watchdog may relocate
                husks = new Husks(cfg, events, enemies, world, map.killPlaneY) { jumper = new HuskNavJumper(cfg, events) };
                health = new Health(100000, events, "player");
            }

            public Husk Spawn(Vec3 at, Vec3 player)
            {
                Husk h = husks.Spawn(at, player);
                if (h != null) brain.Attach(h);
                return h;
            }

            public void Step(Vec3 player, double dt)
            {
                brain.Step(dt, player, 0, 1, false);
                husks.Step(player, health, dt);
                brain.Separate(husks);
            }
        }

        [Test]
        public void AHuskFromAnotherZone_ReachesThePlayer()
        {
            var sim = new Sim();
            sim.brain.onScreen = _ => true; // no relocation: it has to walk (and jump) the whole way
            Vec3 player = map.playerSpawn; // LANDING
            Husk h = sim.Spawn(new Vec3(-20, 0, -44), player); // the far north wall's ground spawn
            Assert.That(h, Is.Not.Null);
            double dt = cfg.Dt;
            double closest = double.PositiveInfinity;
            int steps = 0;
            for (; steps < 40 * 120 && h.alive; steps++)
            {
                sim.Step(player, dt);
                closest = System.Math.Min(closest, JsMath.Hypot(h.position.x - player.x, h.position.z - player.z));
                if (closest < 3) break;
            }
            TestContext.WriteLine($"closest {closest:0.00} m after {steps / 120.0:0.0} s; link jumps {sim.brain.linkJumps}, hops {sim.brain.hops}, sidesteps {sim.brain.sidesteps}, repaths {sim.brain.repaths}, relocations {sim.brain.relocations}");
            Assert.That(closest, Is.LessThan(3), $"the husk reaches the player (closest {closest:0.00} m, {steps / 120.0:0.0} s, link jumps {sim.brain.linkJumps}, hops {sim.brain.hops})");
            Assert.That(sim.brain.relocations, Is.EqualTo(0));
            TestContext.WriteLine($"reached in {steps / 120.0:0.0} s; link jumps {sim.brain.linkJumps}, hops {sim.brain.hops}, sidesteps {sim.brain.sidesteps}, repaths {sim.brain.repaths}");
        }

        [Test]
        public void TheMeleeTokens_CapTheWindUps()
        {
            var sim = new Sim();
            Vec3 player = map.playerSpawn;
            var list = new List<Husk>();
            foreach (var at in new[] { new Vec3(3, 0, 34), new Vec3(-3, 0, 34), new Vec3(5, 0, 37), new Vec3(-5, 0, 37), new Vec3(2, 0, 31), new Vec3(-2, 0, 31) })
            {
                Husk h = sim.Spawn(at, player);
                if (h != null) list.Add(h);
            }
            Assert.That(list.Count, Is.GreaterThanOrEqualTo(4));
            int cap = cfg.Int("ai.tokens.base.melee");
            int most = 0, windups = 0;
            sim.events.On("huskWindup", _ => windups++);
            for (int i = 0; i < 20 * 120; i++)
            {
                sim.Step(player, cfg.Dt);
                int now = 0;
                foreach (Husk h in list) if (h.alive && h.windupSteps > 0) now++;
                most = System.Math.Max(most, now);
            }
            TestContext.WriteLine($"most winding up at once {most} (cap {cap}), wind-ups {windups}");
            Assert.That(windups, Is.GreaterThan(0), "they attack");
            Assert.That(most, Is.LessThanOrEqualTo(cap));
        }
    }
}
