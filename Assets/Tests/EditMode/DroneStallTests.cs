// NIGHT20 (builder EN owns this file; tests first): THE FARM's DRONE stall (builder P2: seed 21, round 6, 3 drones still
// for 45+ s, 63-66 m out, two of them at y 39.4 under the map's 40 m roof, the round never clears). drone.js's climb looks
// down from climbProbeHeight (40 m) above a blocked probe: under the Survival map's roof barrier (y 40..41) that look-down
// lands on the roof, so every blocked drone climbs to 43.6 m, pins itself under the roof with no horizontal speed (the
// climb zeroes it) and never lets go (the latch clears only on reaching its height). Behind design.director.n20 (the
// drone's flags, set by the director at its spawn; off: the web's latch exactly):
//   1. a wall under the map's roof is climbed over: the look-down starts under the first roof above the probe
//      (droneRoof), so the climb is to the wall's top + climbAbove; off: it pins under the roof
//   2. a climb that can't rise (a low roof over it) gives up after droneClimbStall s without gaining height and does not
//      re-latch for droneClimbHold s: it slides round the obstacle; off: it stays under the low roof
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Core.Tests
{
    public class DroneStallTests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")), "config.json"));
        static double N(string k) => Cfg.Num("design.director.n20." + k);

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Health health;
            public readonly Husks pool;

            public Rig()
            {
                world.AddBox(new Vec3(-120, -1, -120), new Vec3(120, 0, 120));
                world.AddBox(new Vec3(-120, 40, -120), new Vec3(120, 41, 120)); // the Survival map's roof barrier
                health = new Health(1e9, events, "player");
                pool = new Husks(Cfg, events, enemies, world, -30);
                pool.NewRun(7);
            }

            public Drone Spawn(Vec3 at, Vec3 player, bool fix)
            {
                var d = (Drone)pool.Spawn("drone", at, player);
                if (fix)
                {
                    d.roofCheck = true;
                    d.climbStall = N("droneClimbStall");
                    d.climbHold = N("droneClimbHold");
                }
                return d;
            }

            public void Run(Vec3 player, double seconds)
            {
                for (int i = 0; i < MovementMath.StepsFor(seconds, Cfg.Dt); i++) pool.Step(player, health, Cfg.Dt);
            }
        }

        [Test]
        public void AWallUnderTheMapsRoof_IsClimbedOver()
        {
            var player = new Vec3(40, 0, 0);
            foreach (bool fix in new[] { false, true })
            {
                var r = new Rig();
                r.world.AddBox(new Vec3(8, 0, -60), new Vec3(10, 12, 60)); // a 12 m wall across the way
                Drone d = r.Spawn(new Vec3(0, 0, 0), player, fix);
                r.Run(player, 15);
                System.Console.WriteLine($"fix {fix}: drone at ({d.position.x:0.0}, {d.position.y:0.0}, {d.position.z:0.0}), climbTo {d.climbTo:0.0}");
                if (!fix)
                {
                    Assert.That(d.position.x, Is.LessThan(8), "the web's latch: it never crosses");
                    Assert.That(d.position.y, Is.GreaterThan(35), "pinned under the roof");
                    continue;
                }
                Assert.That(d.position.x, Is.GreaterThan(10), "over the wall");
                Assert.That(d.position.y, Is.LessThan(25), "no higher than the wall's top + its hover band");
            }
        }

        [Test]
        public void AClimbThatCantRise_GivesUp_AndGoesRound()
        {
            var player = new Vec3(40, 0, 0);
            foreach (bool fix in new[] { false, true })
            {
                var r = new Rig();
                r.world.AddBox(new Vec3(8, 0, -1), new Vec3(10, 30, 1)); // a 30 m pillar, 2 m wide, in the way
                r.world.AddBox(new Vec3(-20, 9, -30), new Vec3(7, 10, 30)); // a low roof over the drone, short of the pillar
                Drone d = r.Spawn(new Vec3(0, 0, 0), player, fix);
                r.Run(player, 20);
                System.Console.WriteLine($"fix {fix}: drone at ({d.position.x:0.0}, {d.position.y:0.0}, {d.position.z:0.0}), climbTo {d.climbTo:0.0}");
                if (!fix)
                {
                    Assert.That(d.position.x, Is.LessThan(8), "the web's latch: stuck under the low roof");
                    continue;
                }
                Assert.That(d.position.x, Is.GreaterThan(10), "round the pillar");
            }
        }
    }
}
