// NIGHT20 (builder N, for builder E's LANCER): brain.js's stationary kind (ai.kinds.<kind>.stationary, the web's lancer:
// "Lancers never walk: they blink between perches on their own"): the brain thinks for it (its tokens, its line of
// sight) but never steers it: its velocity order stays 0, and there is no navigation and no watchdog (no stuck events).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class BrainStationaryTests
    {
        static GameConfig cfg;

        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        [Test]
        public void TheConfigsLancer_IsStationary()
        {
            Assert.That(Cfg.Obj("ai.kinds.lancer").TryGetValue("stationary", out object s) && s is bool b && b, Is.True, "the web's ai.kinds.lancer.stationary");
        }

        [Test]
        public void ALancerAttachedToTheBrain_NeverMoves()
        {
            var events = new EventBus();
            var world = new TriangleWorld();
            world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
            Dictionary<string, object> gridJson = NavGridBuilder.Build(Cfg, world, 60, null, "flat");
            var nav = new NavField(Cfg, NavGrid.FromExport(gridJson));
            var brain = new HuskBrain(Cfg, events, nav, world) { onScreen = _ => false };
            HuskParams hp = HuskParams.From(Cfg, -30);
            hp.kind = "lancer";
            var lancer = new Husk(hp);
            var player = new Vec3(0, 0, 0);
            Assert.That(lancer.Spawn(1, new Vec3(0, 0, 20), player, world), Is.True);
            brain.Attach(lancer);
            int stuck = 0;
            events.On("navStuck", _ => stuck++);
            var health = new Health(1e9, events, "player");
            var pool = new List<Enemy> { lancer };
            Vec3 start = lancer.position;
            double most = 0;
            for (int i = 0; i < 6 * 120; i++)
            {
                brain.Step(Cfg.Dt, player, 0, 1, false);
                lancer.Step(player, health, pool, world, events, Cfg.Dt, MovementMath.GravityFor(Cfg.movement.jump.height, Cfg.movement.jump.apexTime));
                most = System.Math.Max(most, System.Math.Abs(lancer.ai.vx) + System.Math.Abs(lancer.ai.vz));
            }
            double moved = JsMath.Hypot(lancer.position.x - start.x, lancer.position.z - start.z);
            System.Console.WriteLine($"lancer moved {moved:0.000} m, the brain's largest order {most:0.000} m/s, stuck events {stuck}");
            Assert.That(most, Is.EqualTo(0), "the brain never orders it to move");
            Assert.That(moved, Is.LessThan(1e-6), "it stays where it spawned");
            Assert.That(stuck, Is.EqualTo(0), "no watchdog");
        }
    }
}
