// NIGHT19 U3-33.7 (builder E owns this file; the shared commit's empty fixture): the DRONE (src/enemies/drone.js, ported
// in Core/Enemies/Drone.cs) on a floor, without a brain (drone.js's own steering): it hovers drone.hoverMin .. hoverMax
// over the ground, closes in from beyond rangeMax and backs off inside rangeMin, charges chargeTime and fires a pooled orb
// (orb.speed, orb.damage on a still player; a dashing player's i-frames let it through), at most orb.poolSize orbs; a
// punch reflects an orb (SpitterAcid.TryReflect: the punch's one reflect call) back at the nearest enemy at
// orb.reflectSpeedMult x speed for orb.reflectDamageMult x damage; it is Enemy.Airborne (LAUNCH + SNIPE). The director
// deals drones from director.rounds.introduce.drone with design.enemies.n19 on.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Core.Tests
{
    public class DroneTests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")), "config.json"));

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Health health;
            public readonly Husks pool;
            public readonly List<HurtEvent> hurts = new List<HurtEvent>();
            public readonly List<string> said = new List<string>();

            public Rig()
            {
                world.AddBox(new Vec3(-120, -1, -120), new Vec3(120, 0, 120));
                health = new Health(100000, events, "player");
                pool = new Husks(Cfg, events, enemies, world, -30);
                pool.NewRun(7);
                events.On<HurtEvent>("hurt", e => hurts.Add(new HurtEvent { amount = e.amount, source = e.source }));
                foreach (string name in new[] { "droneCharge", "orbFire", "orbDestroyed" })
                {
                    string n = name;
                    events.On(n, _ => said.Add(n));
                }
            }

            public void Step(Vec3 player) => pool.Step(player, health, Cfg.Dt);
        }

        static Drone Spawn(Rig r, Vec3 at, Vec3 player)
        {
            Husk h = r.pool.Spawn("drone", at, player);
            Assert.That(h, Is.InstanceOf<Drone>(), "the pool spawns a Drone for 'drone'");
            return (Drone)h;
        }

        [Test]
        public void ItSpawnsAsTheConfigsDrone_Airborne()
        {
            var r = new Rig();
            Drone d = Spawn(r, new Vec3(0, 0, 0), new Vec3(0, 0, 30));
            Assert.That(d.kind, Is.EqualTo("drone"));
            Assert.That(d.hp, Is.EqualTo(Cfg.Num("drone.hp")));
            Assert.That(d.Radius, Is.EqualTo(Cfg.Num("drone.radius")));
            Assert.That(d.position.y, Is.EqualTo(Cfg.Num("drone.spawnRise") + Cfg.Num("drone.radius")).Within(1e-9), "drone.js spawn: spawnRise + radius over the point");
            Assert.That(d.hover, Is.InRange(Cfg.Num("drone.hoverMin"), Cfg.Num("drone.hoverMax")));
            Assert.That(d.Airborne, Is.True, "a flying kind: LAUNCH + SNIPE's target");
            Assert.That(d.RayHit(new Vec3(0, d.position.y, -10), new Vec3(0, 0, 1), out bool head), Is.GreaterThan(0));
            Assert.That(head, Is.False, "no headshots");
        }

        [Test]
        public void ItHoversInItsBand_AndClosesInFromBeyondRangeMax()
        {
            var r = new Rig();
            var player = new Vec3(0, 0, 60);
            Drone d = Spawn(r, new Vec3(0, 0, 0), player);
            for (int i = 0; i < MovementMath.StepsFor(8, Cfg.Dt); i++) r.Step(player);
            Assert.That(d.position.y, Is.EqualTo(d.hover).Within(0.35), "it settles at its rolled hover over the floor");
            Assert.That(d.hover, Is.InRange(Cfg.Num("drone.hoverMin"), Cfg.Num("drone.hoverMax")));
            double dist = JsMath.Hypot(player.x - d.position.x, player.z - d.position.z);
            Assert.That(dist, Is.LessThanOrEqualTo(Cfg.Num("drone.rangeMax") + 1), "it closed to its range");
        }

        [Test]
        public void ItBacksOff_InsideRangeMin()
        {
            var r = new Rig();
            var player = new Vec3(0, 0, 4);
            Drone d = Spawn(r, new Vec3(0, 0, 0), player);
            for (int i = 0; i < MovementMath.StepsFor(4, Cfg.Dt); i++) r.Step(player);
            double dist = JsMath.Hypot(player.x - d.position.x, player.z - d.position.z);
            Assert.That(dist, Is.GreaterThanOrEqualTo(Cfg.Num("drone.rangeMin") - 1), "it keeps away");
        }

        [Test]
        public void ItCharges_ThenFiresAnOrb_ThatHitsAStillPlayer()
        {
            var r = new Rig();
            var player = new Vec3(0, 0, 15);
            Drone d = Spawn(r, new Vec3(0, 0, 0), player);
            int k = 0;
            while (!r.said.Contains("droneCharge") && k++ < 2000) r.Step(player);
            Assert.That(r.said, Does.Contain("droneCharge"), "in sight: it charges");
            Assert.That(d.committed, Is.True);
            int charge = 0;
            while (!r.said.Contains("orbFire") && charge++ < 400) r.Step(player);
            Assert.That(charge, Is.EqualTo(MovementMath.StepsFor(Cfg.Num("drone.chargeTime"), Cfg.Dt)), "chargeTime, then the orb");
            DroneOrbs.Orb orb = null;
            foreach (DroneOrbs.Orb o in r.pool.orbs.pool) if (o.alive) orb = o;
            Assert.That(orb, Is.Not.Null);
            Assert.That(orb.velocity.Length, Is.EqualTo(Cfg.Num("orb.speed")).Within(1e-9));
            int fly = 0;
            while (r.hurts.Count == 0 && fly++ < 600) r.Step(player);
            Assert.That(r.hurts.Count, Is.GreaterThan(0), "the orb reached the still player");
            Assert.That(r.hurts[0].source, Is.EqualTo("drone"));
            Assert.That(r.hurts[0].amount, Is.EqualTo(Cfg.Num("orb.damage")));
            Assert.That(r.said, Does.Contain("orbDestroyed"));
        }

        [Test]
        public void TheOrbs_ArePooled_AtMostPoolSize()
        {
            var r = new Rig();
            DroneOrbs orbs = r.pool.orbs;
            Assert.That(orbs.pool.Length, Is.EqualTo(Cfg.Int("orb.poolSize")));
            int fired = 0;
            for (int i = 0; i < orbs.pool.Length + 5; i++)
                if (orbs.Fire(new Vec3(0, 5, 0), new Vec3(0, 0, 1), null) != null) fired++;
            Assert.That(fired, Is.EqualTo(Cfg.Int("orb.poolSize")), "the pool is full: no orb");
        }

        [Test]
        public void APunch_ReflectsAnOrb_BackIntoTheNearestEnemy()
        {
            var r = new Rig();
            var player = new Vec3(0, 0, 15);
            Drone d = Spawn(r, new Vec3(0, 0, 0), player);
            r.Step(player);
            var hits = new List<(Enemy e, double damage)>();
            r.pool.orbs.onReflectHit = (e, damage, at) =>
            {
                hits.Add((e, damage));
                return false;
            };
            Vec3 eye = new Vec3(player.x, Cfg.movement.body.eyeHeight, player.z);
            Vec3 toward = Vec3.Normalize(d.position - eye);
            DroneOrbs.Orb orb = r.pool.orbs.Fire(eye + toward * 1.0, toward * -1, d); // an orb right in front of you, coming at you
            SpitterAcid.Glob reflected = r.pool.acid.TryReflect(eye, toward, r.enemies);
            Assert.That(reflected, Is.Not.Null, "the punch's reflect finds the orb in reach");
            Assert.That(orb.reflected, Is.True);
            Assert.That(orb.target, Is.SameAs(d), "back at the nearest enemy");
            Assert.That(orb.damage, Is.EqualTo(Cfg.Num("orb.damage") * Cfg.Num("orb.reflectDamageMult")));
            Assert.That(orb.velocity.Length, Is.EqualTo(Cfg.Num("orb.speed") * Cfg.Num("orb.reflectSpeedMult")).Within(1e-9));
            for (int i = 0; i < 240 && hits.Count == 0; i++) r.Step(player);
            Assert.That(hits.Count, Is.EqualTo(1), "it homes into the drone");
            Assert.That(hits[0].e, Is.SameAs(d));
        }

        [Test]
        public void TheDirector_DealsDrones_FromTheWebsRound_WithTheBlockOn()
        {
            var on = new N19Blocks { enemies = true };
            Assert.That(Director.Spawnable("drone", on), Is.True);
            Assert.That(Director.Spawnable("drone", N19Blocks.Off), Is.False);
            DirectorMath.D d = DirectorMath.From(Cfg);
            int from = (int)Cfg.Num("director.rounds.introduce.drone");
            bool seen = false;
            for (uint seed = 1; seed <= 40; seed++)
            {
                foreach (List<string> s in DirectorMath.PlanSquads(from - 1, new Mulberry32(seed).Next, d, k => Director.Spawnable(k, on)))
                    Assert.That(s, Does.Not.Contain("drone"), "round " + (from - 1));
                foreach (List<string> s in DirectorMath.PlanSquads(from, new Mulberry32(seed).Next, d, k => Director.Spawnable(k, on)))
                    if (s.Contains("drone")) seen = true;
            }
            Assert.That(seen, Is.True, "a drone squad within 40 seeds of round " + from);
        }
    }
}
