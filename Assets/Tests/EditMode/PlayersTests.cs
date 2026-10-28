// NIGHT9 (the "if time" item: PORT-PLAN.md N10's first item, U4-01's code half): the sim with two players. The web has
// one target (main.js getTarget); COOP-RESEARCH.md's getTarget(from) is new design, so these tests pin the port's rules:
// each enemy hunts the nearest living player, a dead one is dropped, the flow field seeds every living player, a glob
// hits the player it touches and each stands in the acid on their own, and a loadout's events carry its owner (the
// first player's wallet pays only "player"). With one player the golden sets and the 22 replays stay as they were.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class PlayersTests
    {
        static GameConfig cfg;
        static SurvivalMap map;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
        }

        /// <summary>The Survival map, every zone open, the brain and the enemy pools, and two players (HuskNavTests' sim).</summary>
        sealed class Sim
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly GameWorld world;
            public readonly NavField nav;
            public readonly HuskBrain brain;
            public readonly Husks husks;
            public readonly Players players = new Players();
            public readonly Player p1, p2;

            public Sim(Vec3 at1, Vec3 at2)
            {
                world = new GameWorld(map.BuildOctree(), map.CreateCore(cfg.movement));
                var zones = new Zones(cfg, map, world, events);
                zones.OpenAll();
                nav = new NavField(cfg, NavGrid.FromExport(map.navGrid));
                nav.SetBlocked(zones.LockedTag);
                brain = new HuskBrain(cfg, events, nav, world.Geometry);
                brain.onScreen = _ => true; // no relocation: they walk
                husks = new Husks(cfg, events, enemies, world, map.killPlaneY) { jumper = new HuskNavJumper(cfg, events) };
                p1 = players.Add(new Player("player", new Mover(cfg.movement, world), new Health(100000, events, "player")));
                p2 = players.Add(new Player("p2", new Mover(cfg.movement, world), new Health(100000, events, "p2")));
                p1.mover.Reset(at1.x, at1.y, at1.z);
                p2.mover.Reset(at2.x, at2.y, at2.z);
            }

            public Husk Spawn(Vec3 at)
            {
                Husk h = husks.Spawn(at, players.Nearest(at).Feet);
                if (h != null) brain.Attach(h);
                return h;
            }

            public void Step(double dt)
            {
                brain.Step(dt, players, 1, false);
                husks.Step(players, dt);
                brain.Separate(husks);
            }
        }

        static double Flat(Vec3 a, Vec3 b) => JsMath.Hypot(a.x - b.x, a.z - b.z);

        /// <summary>The ground spawn nearest `to` at least `min` m away.</summary>
        static Vec3 SpawnNear(Vec3 to, double min)
        {
            Vec3 best = default;
            double bestD = double.PositiveInfinity;
            foreach (Vec3 s in map.groundSpawns)
            {
                double d = Flat(s, to);
                if (d >= min && d < bestD)
                {
                    best = s;
                    bestD = d;
                }
            }
            return best;
        }

        [Test]
        public void Nearest_IsTheClosestLivingPlayer_ATieKeepsTheFirst_NobodyAliveGivesTheFirst()
        {
            var events = new EventBus();
            var world = new TriangleWorld();
            world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
            var players = new Players();
            Player a = players.Add(new Player("player", new Mover(cfg.movement, world), new Health(100, events, "player")));
            Player b = players.Add(new Player("p2", new Mover(cfg.movement, world), new Health(100, events, "p2")));
            Player c = players.Add(new Player("p3", new Mover(cfg.movement, world), new Health(100, events, "p3")));
            a.mover.Reset(0, 0, 0);
            b.mover.Reset(10, 0, 0);
            c.mover.Reset(-10, 0, 0);
            Assert.That(players.Nearest(new Vec3(8, 0, 1)), Is.SameAs(b));
            Assert.That(players.Nearest(new Vec3(-4, 0, 0)), Is.SameAs(a));
            Assert.That(players.Nearest(new Vec3(5, 0, 0)), Is.SameAs(a), "a tie keeps the earlier player");
            b.health.Damage(100);
            Assert.That(players.Nearest(new Vec3(9, 0, 0)), Is.SameAs(a), "the dead are skipped");
            Assert.That(players.Living, Is.EqualTo(2));
            a.health.Damage(100);
            c.health.Damage(100);
            Assert.That(players.Living, Is.EqualTo(0));
            Assert.That(players.Nearest(new Vec3(9, 0, 0)), Is.SameAs(a), "nobody alive: the first (a solo run keeps its one target)");
        }

        [Test]
        public void TwoPlayers_EachHuskHuntsTheNearest_ThenTheLivingOneWhenTheOtherDies()
        {
            Vec3 at1 = map.playerSpawn;
            Vec3 at2 = default;
            double far = 0;
            foreach (Vec3 s in map.groundSpawns)
            {
                if (Flat(s, at1) > far)
                {
                    far = Flat(s, at1);
                    at2 = s;
                }
            }
            var sim = new Sim(at1, at2);
            Vec3 spawnA = SpawnNear(at2, 4), spawnB = SpawnNear(at1, 4);
            Assert.That(Flat(spawnA, at2), Is.LessThan(Flat(spawnA, at1)), "husk A starts nearer P2");
            Assert.That(Flat(spawnB, at1), Is.LessThan(Flat(spawnB, at2)), "husk B starts nearer P1");
            Husk a = sim.Spawn(spawnA), b = sim.Spawn(spawnB);
            Assert.That(a, Is.Not.Null);
            Assert.That(b, Is.Not.Null);
            var hurt = new Dictionary<string, int> { ["player"] = 0, ["p2"] = 0 };
            sim.events.On<HurtEvent>("hurt", e => hurt[e.owner]++);
            double dt = cfg.Dt;
            double closestA = double.PositiveInfinity, closestB = double.PositiveInfinity;
            for (int i = 0; i < 30 * 120 && (hurt["player"] == 0 || hurt["p2"] == 0); i++)
            {
                sim.Step(dt);
                closestA = System.Math.Min(closestA, Flat(a.position, at2));
                closestB = System.Math.Min(closestB, Flat(b.position, at1));
            }
            TestContext.WriteLine($"P1 {at1}, P2 {at2} ({far:0.0} m apart); A from {spawnA} closest to P2 {closestA:0.00} m; B from {spawnB} closest to P1 {closestB:0.00} m; hurt {hurt["player"]} / {hurt["p2"]}");
            Assert.That(closestA, Is.LessThan(3), "husk A reaches P2");
            Assert.That(closestB, Is.LessThan(3), "husk B reaches P1");
            Assert.That(hurt["p2"], Is.GreaterThan(0), "P2 is hit");
            Assert.That(hurt["player"], Is.GreaterThan(0), "P1 is hit");

            // P2 dies: husk A is the living player's now (its flow field and its think both drop P2).
            sim.p2.health.Damage(sim.p2.health.max);
            int p2Hurt = hurt["p2"];
            double fromA = Flat(a.position, at1), closest = fromA;
            int steps = 0;
            for (; steps < 60 * 120 && closest >= 3; steps++)
            {
                sim.Step(dt);
                closest = System.Math.Min(closest, Flat(a.position, at1));
            }
            TestContext.WriteLine($"after P2's death: A {fromA:0.0} m from P1, closest {closest:0.00} m after {steps / 120.0:0.0} s");
            Assert.That(closest, Is.LessThan(3), "husk A crosses the map to P1");
            Assert.That(hurt["p2"], Is.EqualTo(p2Hurt), "nothing hurts the dead");
        }

        [Test]
        public void TwoPlayers_TheFlowFieldSeedsEveryLivingPlayer()
        {
            Vec3 at1 = map.playerSpawn;
            Vec3 at2 = SpawnNear(at1, 30);
            var sim = new Sim(at1, at2);
            var at = new[] { at1, at2 };
            var alive = new[] { true, true };
            double dt = cfg.Dt;
            for (int i = 0; i < 240; i++) sim.nav.Step(dt, at, alive, 2);
            double d1 = sim.nav.DistanceAt(at1.x, at1.y, at1.z), d2 = sim.nav.DistanceAt(at2.x, at2.y, at2.z);
            TestContext.WriteLine($"both: {d1:0.00} at P1, {d2:0.00} at P2 ({Flat(at1, at2):0.0} m apart)");
            Assert.That(d1, Is.LessThan(2));
            Assert.That(d2, Is.LessThan(2));
            alive[1] = false;
            for (int i = 0; i < 240; i++) sim.nav.Step(dt, at, alive, 2);
            double dead = sim.nav.DistanceAt(at2.x, at2.y, at2.z);
            TestContext.WriteLine($"P2 dead: {dead:0.00} at P2");
            Assert.That(sim.nav.DistanceAt(at1.x, at1.y, at1.z), Is.LessThan(2));
            Assert.That(dead, Is.GreaterThan(20), "a dead player is no goal");
        }

        [Test]
        public void TwoPlayers_AGlobHitsThePlayerItTouches_EachStandsInTheAcidAlone()
        {
            var events = new EventBus();
            var enemies = new EnemyRegistry();
            var world = new TriangleWorld();
            world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
            var husks = new Husks(cfg, events, enemies, world, -30);
            var players = new Players();
            Player a = players.Add(new Player("player", new Mover(cfg.movement, world), new Health(1000, events, "player")));
            Player b = players.Add(new Player("p2", new Mover(cfg.movement, world), new Health(1000, events, "p2")));
            a.mover.Reset(0, 0, 0);
            b.mover.Reset(10, 0, 0);
            SpitterAcid acid = husks.acid;
            double dt = cfg.Dt;
            // A puddle under P2: P2 ticks puddleDps, P1 (10 m away) nothing.
            SpitterAcid.Puddle pd = acid.puddles[0];
            pd.alive = true;
            pd.age = 0;
            pd.position = new Vec3(10, 0, 0);
            int n = MovementMath.StepsFor(acid.p.puddleTick, dt) + 1;
            for (int i = 0; i < n; i++) acid.Step(players, world, enemies, dt);
            Assert.That(b.health.hp, Is.EqualTo(1000 - acid.p.puddleDps * acid.p.puddleTick).Within(1e-9), "P2 stands in the acid");
            Assert.That(a.health.hp, Is.EqualTo(1000), "P1 does not");
            // A glob at P1's chest: it hits P1 (and splashes there), not P2.
            SpitterAcid.Glob g = acid.globs[0];
            g.alive = true;
            g.reflected = false;
            g.age = 0;
            g.position = g.previous = new Vec3(0, 1, 0);
            g.velocity = default;
            double hp2 = b.health.hp;
            acid.Step(players, world, enemies, dt);
            Assert.That(a.health.hp, Is.EqualTo(1000 - acid.p.globDamage).Within(1e-9), "the glob hits P1");
            Assert.That(b.health.hp, Is.EqualTo(hp2).Within(acid.p.puddleDps * acid.p.puddleTick + 1e-9), "and not P2 (whose acid may tick)");
            Assert.That(g.alive, Is.False);
        }

        [Test]
        public void ALoadoutsEventsCarryItsOwner_TheFirstWalletPaysOnlyPlayer()
        {
            var events = new EventBus();
            var enemies = new EnemyRegistry();
            var world = new TriangleWorld();
            world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
            var husks = new Husks(cfg, events, enemies, world, -30);
            var economy = new Economy(cfg, events) { enabled = true };
            (Vec3, Vec3) Aim() => (new Vec3(0, 1.6, 0), new Vec3(0, 0, -1));
            var m1 = new Mover(cfg.movement, world);
            var m2 = new Mover(cfg.movement, world);
            var l1 = new Loadout(cfg, events, enemies, world, m1, new Health(100, events, "player"), Aim);
            var l2 = new Loadout(cfg, events, enemies, world, m2, new Health(100, events, "p2"), Aim, "p2");
            Husk h = husks.Spawn(new Vec3(0, 0, -5), default);
            h.hp = h.maxHp = 1e9;
            var owners = new List<string>();
            events.On<HitEvent>("hit", e => owners.Add(e.owner));
            l2.Damage(h, 10, false, "revolver", h.AimPoint);
            Assert.That(owners, Is.EqualTo(new[] { "p2" }));
            Assert.That(economy.total, Is.EqualTo(0), "P2's hit is not the first wallet's");
            l1.Damage(h, 10, false, "revolver", h.AimPoint);
            Assert.That(owners, Is.EqualTo(new[] { "p2", "player" }));
            Assert.That(economy.total, Is.GreaterThan(0));
            Assert.That(l1.owner, Is.EqualTo("player"), "the default owner is the web's");
        }
    }
}
