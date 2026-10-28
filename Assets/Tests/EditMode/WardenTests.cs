// NIGHT20 U3-34.9 C4 (builder E owns this file; tests first): the WARDEN (src/enemies/warden.js, ported as it is in
// Core/Enemies/Warden.cs), behind design.adds.warden. On a floor with a still player:
//   1. config.warden's body; with no brain it walks straight at you at warden.speed
//   2. inside swingStart: `wardenTell`, nothing before tellTime, then the swing for warden.damage within swingRange
//      (`wardenSwing`), recoverTime, then cooldown before the next tell
//   3. the shield: a hit from its front arc (shieldArcDeg) is absorbed (`enemyBlocked`, a blocked hit: no hit at all);
//      from behind, from above (aboveHeight), or a rocket's splash it lands
//   4. a punch on the shield (a knockback from the front) opens it for shieldStun s, stunned (`wardenShieldBreak`): front
//      hits land; then it closes; a knockback from behind is x knockbackScale
//   5. it shoves other walkers aside (shoveRadius, shoveSpeed)
//   6. the director deals wardens from director.rounds.introduce.warden with the row on, never with it off
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Core.Tests
{
    public class WardenTests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")), "config.json"));
        static double W(string k) => Cfg.Num("warden." + k);
        static double Dt => Cfg.Dt;

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Health health;
            public readonly Husks pool;
            public readonly List<double> hurts = new List<double>();
            public readonly List<string> said = new List<string>();
            public Vec3 player = new Vec3(0, 0, 20);

            public Rig()
            {
                world.AddBox(new Vec3(-120, -1, -120), new Vec3(120, 0, 120));
                health = new Health(100000, events, "player");
                pool = new Husks(Cfg, events, enemies, world, -30);
                pool.NewRun(3);
                events.On<HurtEvent>("hurt", e =>
                {
                    if (e.source == "warden") hurts.Add(e.amount);
                });
                foreach (string name in new[] { "wardenTell", "wardenSwing", "wardenShieldBreak", "enemyBlocked" })
                {
                    string n = name;
                    events.On(n, _ => said.Add(n));
                }
            }

            public Warden Spawn(Vec3 at)
            {
                Husk h = pool.Spawn("warden", at, player);
                Assert.That(h, Is.InstanceOf<Warden>(), "the pool spawns a Warden for 'warden'");
                return (Warden)h;
            }

            public void Run(double seconds)
            {
                int n = MovementMath.StepsFor(seconds, Dt);
                for (int i = 0; i < n; i++) pool.Step(player, health, Dt);
            }
        }

        [Test]
        public void ItSpawnsAsTheConfigsWarden_AndWalksAtYou()
        {
            var r = new Rig();
            Warden w = r.Spawn(new Vec3(0, 0, 0));
            Assert.That(w.kind, Is.EqualTo("warden"));
            Assert.That(w.hp, Is.EqualTo(W("hp")));
            Assert.That(w.Radius, Is.EqualTo(W("radius")));
            Assert.That(w.Height, Is.EqualTo(W("height")));
            r.Run(0.5); // settle on the floor
            double z0 = w.position.z;
            r.Run(1);
            Assert.That(w.position.z - z0, Is.EqualTo(W("speed")).Within(0.3), "warden.speed m a second, straight at you");
        }

        [Test]
        public void InsideSwingStart_ItTells_ThenSwingsForItsDamage()
        {
            var r = new Rig { player = new Vec3(0, 0, 2) };
            r.Spawn(new Vec3(0, 0, 0));
            r.Run(0.05);
            Assert.That(r.said, Does.Contain("wardenTell"));
            r.Run(W("tellTime") - 0.1);
            Assert.That(r.hurts, Is.Empty, "nothing before tellTime");
            r.Run(0.1);
            Assert.That(r.hurts, Is.EqualTo(new[] { W("damage") }));
            Assert.That(r.said, Does.Contain("wardenSwing"));
            int tells = r.said.FindAll(s => s == "wardenTell").Count;
            r.Run(W("recoverTime") + W("cooldown") - 0.1);
            Assert.That(r.said.FindAll(s => s == "wardenTell").Count, Is.EqualTo(tells), "recoverTime, then the cooldown");
            r.Run(0.2);
            Assert.That(r.said.FindAll(s => s == "wardenTell").Count, Is.EqualTo(tells + 1));
        }

        [Test]
        public void TheShield_AbsorbsTheFront_NotBehindAboveOrSplash()
        {
            var r = new Rig();
            Warden w = r.Spawn(new Vec3(0, 0, 0));
            r.Run(0.1); // facing the player (+z)
            Vec3 front = new Vec3(0, 1.5, 10), behind = new Vec3(0, 1.5, -10), above = new Vec3(0, w.position.y + W("height") + W("aboveHeight") + 0.5, 2);
            double got = EnemyHits.Filter(w, 30, false, "revolver", false, front, r.events, out bool blocked);
            Assert.That(blocked, Is.True, "the front arc absorbs it");
            Assert.That(got, Is.EqualTo(0));
            Assert.That(r.said, Does.Contain("enemyBlocked"));
            Assert.That(EnemyHits.Filter(w, 30, false, "revolver", false, behind, r.events, out blocked), Is.EqualTo(30));
            Assert.That(blocked, Is.False, "flanked");
            Assert.That(EnemyHits.Filter(w, 30, false, "revolver", false, above, r.events, out blocked), Is.EqualTo(30), "from above");
            Assert.That(EnemyHits.Filter(w, 30, false, "rocket", false, front, r.events, out blocked), Is.EqualTo(30), "a rocket's splash");
        }

        [Test]
        public void APunchOnTheShield_OpensItForShieldStun()
        {
            var r = new Rig();
            Warden w = r.Spawn(new Vec3(0, 0, 0));
            r.Run(0.1);
            w.Knockback(0, -1, 7, 0.2, Dt); // a punch from the front pushes it away from you (-z)
            Assert.That(r.said, Does.Contain("wardenShieldBreak"));
            Assert.That(w.ShieldDown, Is.True);
            Assert.That(w.state, Is.EqualTo("stunned"));
            Vec3 front = new Vec3(0, 1.5, 10);
            Assert.That(EnemyHits.Filter(w, 30, false, "revolver", false, front, r.events, out bool blocked), Is.EqualTo(30), "open: the front lands");
            r.Run(W("shieldStun") + 0.05);
            Assert.That(w.ShieldDown, Is.False, "closed again");
            EnemyHits.Filter(w, 30, false, "revolver", false, front, r.events, out blocked);
            Assert.That(blocked, Is.True);
        }

        [Test]
        public void ItShovesOtherWalkersAside()
        {
            var r = new Rig { player = new Vec3(0, 0, 40) };
            r.Spawn(new Vec3(0, 0, 0));
            Husk other = r.pool.Spawn("husk", new Vec3(1.0, 0, 0), r.player);
            r.pool.frozen = false;
            double x0 = other.position.x;
            r.Run(0.3);
            Assert.That(other.position.x, Is.GreaterThan(x0 + 0.3), "pushed out along the line between them");
        }

        [Test]
        public void TheDirector_DealsWardens_FromTheWebsRound_WithTheRowOn()
        {
            var n19 = new N19Blocks { enemies = true };
            Assert.That(Director.Spawnable("warden", n19, false, true), Is.True);
            Assert.That(Director.Spawnable("warden", n19, false, false), Is.False);
            DirectorMath.D d = DirectorMath.From(Cfg);
            int from = (int)Cfg.Num("director.rounds.introduce.warden");
            int first = from;
            while (DirectorMath.IsSwarmRound(first, d) || DirectorMath.IsBossRound(first, d)) first++;
            bool seen = false;
            for (uint seed = 1; seed <= 60; seed++)
            {
                foreach (List<string> s in DirectorMath.PlanSquads(from - 1, new Mulberry32(seed).Next, d, k => Director.Spawnable(k, n19, false, true)))
                    Assert.That(s, Does.Not.Contain("warden"));
                foreach (List<string> s in DirectorMath.PlanSquads(first, new Mulberry32(seed).Next, d, k => Director.Spawnable(k, n19, false, true)))
                    if (s.Contains("warden")) seen = true;
            }
            Assert.That(seen, Is.True, "a warden squad within 60 seeds of round " + first);
        }
    }
}
