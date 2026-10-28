// NIGHT19 U3-33.7 (builder E owns this file; the shared commit's empty fixture): the CHARGER (src/enemies/charger.js on
// enemyKind.js, ported line for line in Core/Enemies/Charger.cs) on a floor with a wall: it reads charger.* from the
// config; in range (minRange .. maxRange) it tells for tellTime with its charge line locked at your position then; it
// charges straight at chargeSpeed for chargeDistance; passing through you deals damage once and throws you (knockback,
// knockbackUp), unless you are over it (a jump) or dashing (the dash's invulnerability); a wall stops it stunned for
// stunTime with its back weak point exposed (weakMult from behind, through EnemyHits.Filter: the hook Loadout.Damage
// calls); a charge that hits nothing ends in recoverTime; then a cooldown. Its pool is the Husks' (charger.maxCount).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Core.Tests
{
    public class ChargerTests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")), "config.json"));

        static int Steps(string path) => MovementMath.StepsFor(Cfg.Num(path), Cfg.Dt);

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Health health;
            public readonly Husks pool;
            public readonly List<HurtEvent> hurts = new List<HurtEvent>();
            public readonly List<string> said = new List<string>();

            /// <summary>A 160 m floor; with `wallAtZ`, a 2 m thick, 4 m high wall across x at that z (its near face there).</summary>
            public Rig(double? wallAtZ = null)
            {
                world.AddBox(new Vec3(-80, -1, -80), new Vec3(80, 0, 80));
                if (wallAtZ.HasValue) world.AddBox(new Vec3(-40, 0, wallAtZ.Value), new Vec3(40, 4, wallAtZ.Value + 2));
                health = new Health(1000, events, "player");
                pool = new Husks(Cfg, events, enemies, world, -30);
                events.On<HurtEvent>("hurt", e => hurts.Add(new HurtEvent { amount = e.amount, source = e.source }));
                foreach (string name in new[] { "chargerTell", "chargerCharge", "chargerStunned", "chargerBlocked" })
                {
                    string n = name;
                    events.On(n, _ => said.Add(n));
                }
            }

            public void Step(Vec3 player) => pool.Step(player, health, Cfg.Dt);
        }

        static Charger Spawn(Rig r, Vec3 at, Vec3 player)
        {
            Husk h = r.pool.Spawn("charger", at, player);
            Assert.That(h, Is.InstanceOf<Charger>(), "the pool spawns a Charger for 'charger'");
            return (Charger)h;
        }

        /// <summary>Step until the charger is in `state` (at most `max` steps); the steps taken.</summary>
        static int Until(Rig r, Charger c, string state, Vec3 player, int max = 600)
        {
            int k = 0;
            while (c.state != state && k < max)
            {
                r.Step(player);
                k++;
            }
            Assert.That(c.state, Is.EqualTo(state), $"after {k} steps");
            return k;
        }

        [Test]
        public void Params_AreTheConfigsCharger()
        {
            var r = new Rig();
            Charger c = Spawn(r, new Vec3(0, 0, 40), new Vec3(0, 0, 70));
            Assert.That(c.kind, Is.EqualTo("charger"));
            Assert.That(c.hp, Is.EqualTo(Cfg.Num("charger.hp")));
            Assert.That(c.Radius, Is.EqualTo(Cfg.Num("charger.radius")));
            Assert.That(c.Height, Is.EqualTo(Cfg.Num("charger.height")));
            Assert.That(c.AimHeight, Is.EqualTo(Cfg.Num("charger.height") * 0.55), "enemyKind.js aimPoint: c.aimHeight ?? height x 0.55");
            Assert.That(c.RayHit(new Vec3(0, 1.8, 30), new Vec3(0, 0, 1), out bool head), Is.GreaterThan(0));
            Assert.That(head, Is.False, "no head: a body capsule only");
        }

        [Test]
        public void InRange_ItTellsForTellTime_WithTheLineLockedAtYou_ThenChargesAtChargeSpeed()
        {
            var r = new Rig();
            var player = new Vec3(0, 0, 0);
            Charger c = Spawn(r, new Vec3(0, 0, -15), player);
            Assert.That(Until(r, c, "tell", player), Is.LessThanOrEqualTo(30), "landed, 15 m (minRange .. maxRange), grounded, no cooldown: the tell");
            Assert.That(r.said, Does.Contain("chargerTell"));
            Assert.That(c.committed, Is.True);
            double locked = c.chargeYaw;
            Assert.That(locked, Is.EqualTo(JsMath.Atan2(-(player.x - c.position.x), -(player.z - c.position.z))).Within(1e-9), "the line points at you");
            int tell = 0;
            var moved = new Vec3(6, 0, 0); // you step aside during the tell: the line stays
            while (c.state == "tell" && tell < 400)
            {
                r.Step(moved);
                tell++;
                if (c.state == "tell") Assert.That(JsMath.Hypot(c.velocity.x, c.velocity.z), Is.EqualTo(0), "it stands still while it tells");
            }
            Assert.That(tell, Is.EqualTo(Steps("charger.tellTime")), "the tell lasts charger.tellTime");
            Assert.That(c.state, Is.EqualTo("charge"));
            Assert.That(c.chargeYaw, Is.EqualTo(locked), "the charge keeps the painted line");
            Assert.That(c.yaw, Is.EqualTo(locked));
            r.Step(moved);
            Assert.That(JsMath.Hypot(c.velocity.x, c.velocity.z), Is.EqualTo(Cfg.Num("charger.chargeSpeed")).Within(1e-9));
            Assert.That(r.said, Does.Contain("chargerCharge"));
        }

        [Test]
        public void TheCharge_HitsYouOnce_ForItsDamage_AndThrowsYou()
        {
            var r = new Rig();
            var player = new Vec3(0, 0, 0);
            var mover = new Mover(Cfg.movement, r.world);
            mover.Reset(0, 0, 0);
            for (int i = 0; i < 30; i++) mover.Step(new InputFrame(), Cfg.Dt);
            Charger c = Spawn(r, new Vec3(0, 0, -10), player);
            c.targetMover = mover;
            Vec3 before = mover.velocity;
            for (int i = 0; i < 240; i++)
            {
                c.targetMover = mover;
                r.Step(player);
            }
            List<HurtEvent> hits = r.hurts.FindAll(h => h.source == "charger");
            Assert.That(hits.Count, Is.EqualTo(1), "one hit per charge");
            Assert.That(hits[0].amount, Is.EqualTo(Cfg.Num("charger.damage")));
            Assert.That(mover.velocity.y, Is.GreaterThan(before.y), "knockbackUp lifts you");
        }

        [Test]
        public void JumpingOverTheCharge_AvoidsIt()
        {
            var r = new Rig();
            Charger c = Spawn(r, new Vec3(0, 0, -10), new Vec3(0, 0, 0));
            var high = new Vec3(0, Cfg.Num("charger.height") * 0.8 + 0.5, 0); // over it
            for (int i = 0; i < 240; i++) r.Step(high);
            Assert.That(r.hurts.FindAll(h => h.source == "charger").Count, Is.EqualTo(0), "you were over it: no hit");
        }

        [Test]
        public void AChargeIntoAWall_StunsIt_ForStunTime_AndItsBackTakesWeakMult()
        {
            var r = new Rig(wallAtZ: 4); // the wall 4 m behind you, across its line
            Charger c = Spawn(r, new Vec3(0, 0, -12), new Vec3(0, 0, 0));
            var high = new Vec3(0, 3, 0); // you hop over it: it carries on into the wall
            Until(r, c, "stunned", high);
            Assert.That(r.said, Does.Contain("chargerStunned"));
            Assert.That(c.stateSteps, Is.EqualTo(Steps("charger.stunTime")));
            Assert.That(c.velocity.x == 0 && c.velocity.z == 0, Is.True);
            // The back weak point: x weakMult from behind (the charger faces +z, the wall: behind it is -z), x1 in front.
            Vec3 behind = new Vec3(c.position.x, 1.5, c.position.z - 6), front = new Vec3(c.position.x, 1.5, c.position.z + 1.5);
            Assert.That(EnemyHits.Filter(c, 10, false, "revolver", false, behind, null, out bool b1), Is.EqualTo(10 * Cfg.Num("charger.weakMult")).Within(1e-9));
            Assert.That(b1, Is.False);
            Assert.That(EnemyHits.Filter(c, 10, false, "revolver", false, front, null, out _), Is.EqualTo(10));
            // the stun ends in idle with the cooldown
            int stun = 0;
            while (c.state == "stunned" && stun++ < 400) r.Step(high);
            Assert.That(stun, Is.EqualTo(Steps("charger.stunTime")));
            Assert.That(c.state, Is.EqualTo("idle"));
            Assert.That(c.committed, Is.False);
            Assert.That(c.cooldownSteps, Is.GreaterThan(Steps("charger.cooldown") - 3), "the cooldown starts");
        }

        [Test]
        public void TheWeakPoint_IsOnlyOpenWhileStunned()
        {
            var r = new Rig();
            Charger c = Spawn(r, new Vec3(0, 0, -40), new Vec3(0, 0, 0)); // out of range: idle, walking
            for (int i = 0; i < 10; i++) r.Step(new Vec3(0, 0, 0));
            Vec3 behind = new Vec3(c.position.x, 1.5, c.position.z - 6);
            Assert.That(c.state, Is.Not.EqualTo("stunned"));
            Assert.That(EnemyHits.Filter(c, 10, false, "revolver", false, behind, null, out _), Is.EqualTo(10));
        }

        [Test]
        public void AChargeThatHitsNothing_RecoversThenCoolsDown()
        {
            var r = new Rig();
            Charger c = Spawn(r, new Vec3(0, 0, -10), new Vec3(0, 0, 0));
            var high = new Vec3(0, 3, 0);
            Until(r, c, "charge", high);
            Assert.That(Until(r, c, "recover", high), Is.EqualTo(MovementMath.StepsFor(Cfg.Num("charger.chargeDistance") / Cfg.Num("charger.chargeSpeed"), Cfg.Dt)),
                "chargeDistance / chargeSpeed s on open floor");
            int rec = 0;
            while (c.state == "recover" && rec++ < 400) r.Step(high);
            Assert.That(rec, Is.EqualTo(Steps("charger.recoverTime")));
            Assert.That(c.state, Is.EqualTo("idle"));
        }

        [Test]
        public void ThePool_HoldsChargerMaxCount()
        {
            var r = new Rig();
            int n = 0;
            for (int i = 0; i < 20; i++)
                if (r.pool.Spawn("charger", new Vec3(-30 + i * 3, 0, 30), new Vec3(0, 0, 0)) != null) n++;
            Assert.That(n, Is.EqualTo(Cfg.Int("charger.maxCount")));
            Assert.That(r.pool.CountOf("charger"), Is.EqualTo(n));
            Assert.That(r.pool.CountOf("husk"), Is.EqualTo(0), "its own pool, not the husks'");
        }
    }
}
