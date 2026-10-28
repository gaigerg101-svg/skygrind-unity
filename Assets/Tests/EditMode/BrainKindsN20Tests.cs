// NIGHT20 (builder N, for builder E): two brain lines E's affixes and WARDEN need. (1) SWIFT's pace (affixes.js: the
// enemy's own speedMult): Husk.paceMult (1 at every spawn) scales the brain's speed cap for that enemy only, as brain.js's
// `k.speed * (e.speedMult ?? 1)`. (2) The WARDEN has a brain: "warden" in BrainKinds, its ai.kinds.warden (heavy token,
// speed 3) instead of the husk's.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class BrainKindsN20Tests
    {
        static GameConfig cfg;

        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly HuskBrain brain;
            public readonly List<Enemy> pool = new List<Enemy>();
            public readonly Health health;

            public Rig()
            {
                world.AddBox(new Vec3(-40, -1, -40), new Vec3(40, 0, 40));
                Dictionary<string, object> gridJson = NavGridBuilder.Build(Cfg, world, 80, null, "flat");
                brain = new HuskBrain(Cfg, events, new NavField(Cfg, NavGrid.FromExport(gridJson)), world) { onScreen = _ => true };
                health = new Health(1e9, events, "player");
            }

            public Husk Add(string kind, Vec3 at, Vec3 player, int id)
            {
                HuskParams hp = HuskParams.From(Cfg, -30);
                hp.kind = kind;
                var h = new Husk(hp);
                Assert.That(h.Spawn(id, at, player, world), Is.True);
                brain.Attach(h);
                pool.Add(h);
                return h;
            }

            /// <summary>Steps the brain and the bodies; the largest speed order each enemy got.</summary>
            public double[] Run(Vec3 player, double seconds)
            {
                var most = new double[pool.Count];
                double g = MovementMath.GravityFor(Cfg.movement.jump.height, Cfg.movement.jump.apexTime);
                for (int i = 0; i < seconds * 120; i++)
                {
                    brain.Step(Cfg.Dt, player, 0, 1, false);
                    for (int k = 0; k < pool.Count; k++)
                    {
                        var h = (Husk)pool[k];
                        h.Step(player, health, pool, world, events, Cfg.Dt, g);
                        most[k] = System.Math.Max(most[k], JsMath.Hypot(h.ai.vx, h.ai.vz));
                    }
                }
                return most;
            }
        }

        [Test]
        public void SwiftsPace_ScalesItsOwnSpeedCapOnly()
        {
            var r = new Rig();
            var player = new Vec3(0, 0, 0);
            Husk plain = r.Add("husk", new Vec3(-6, 0, 30), player, 1);
            Husk swift = r.Add("husk", new Vec3(6, 0, 30), player, 2);
            Assert.That(swift.paceMult, Is.EqualTo(1), "1 at the spawn");
            swift.paceMult = 1.35;
            double[] most = r.Run(player, 2);
            System.Console.WriteLine($"orders: plain {most[0]:0.000} m/s, swift {most[1]:0.000} m/s");
            Assert.That(most[0], Is.EqualTo(Cfg.Num("ai.kinds.husk.speed")).Within(0.01), "the plain husk's cap");
            // (builder EN) brain.js raises the cap (v.maxSpeed = k.speed * pace) but pushes the field's carrot only
            // max(carrotAhead, kindOf(kind).speed x arriveDecel x 1.2) ahead (:357, the kind's speed, not the pace), and Arrive
            // asks distance / arriveDecel: a SWIFT walker on the field runs at min(speed x 1.35, that ask) = 7.2 m/s on the web
            // too (the test first asked 8.1, which the web never reaches on the field).
            double speed = Cfg.Num("ai.kinds.husk.speed"), decel = Cfg.Num("ai.steer.arriveDecel");
            double carrotAsk = System.Math.Max(Cfg.Num("ai.carrotAhead"), speed * decel * 1.2) / decel;
            Assert.That(most[1], Is.EqualTo(System.Math.Min(speed * 1.35, carrotAsk)).Within(0.01), "SWIFT's cap: x paceMult, to the carrot's ask");
            Assert.That(most[1], Is.GreaterThan(most[0] + 1), "SWIFT outruns the plain husk");
            Assert.That(swift.Spawn(2, new Vec3(6, 0, 30), player, r.world) && swift.paceMult == 1, Is.True, "a reused body starts at 1");
        }

        [Test]
        public void AWarden_IsDrivenAsAWarden()
        {
            var r = new Rig();
            var player = new Vec3(0, 0, 0);
            r.Add("warden", new Vec3(0, 0, 30), player, 1);
            double[] most = r.Run(player, 2);
            System.Console.WriteLine($"warden's largest order {most[0]:0.000} m/s (ai.kinds.warden.speed {Cfg.Num("ai.kinds.warden.speed")})");
            Assert.That(most[0], Is.EqualTo(Cfg.Num("ai.kinds.warden.speed")).Within(0.01), "its own kind's speed, not the husk's");
            Assert.That(r.brain.pools.holders.ContainsKey(Cfg.Str("ai.kinds.warden.token")), Is.True, "its token pool exists");
        }
    }
}
