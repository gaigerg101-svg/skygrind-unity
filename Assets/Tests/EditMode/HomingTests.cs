// NIGHT10 U3-13.5: aim assist homing. The web's homing tests (tests/assistMath.test.js :21-45, homeVelocity) ported as
// they are, plus aimAssist.js homingTarget() (:483-491): no lock, no homing; a lock gives {enemy, id, turn} with turn =
// aim.homingTurn deg/s x the mode's k, x aim.lockHomingMult for a V-key lock; OFF never homes. Projectiles.Steer turns a
// nail toward it and marks the hit assisted once it turned past aim.homingAssistAngle (the p19 vectors check the steer
// bit for bit; this checks the wiring from the lock to a fired nail).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class HomingTests
    {
        static GameConfig cfg;
        const double DEG = System.Math.PI / 180;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static double Deg(double rad) => rad / DEG;

        [Test]
        public void HomeVelocity_TurnsAtMostMaxTurnPerStep_KeepingItsSpeed()
        {
            var vel = new Vec3(0, 0, -30);
            double turned = AssistMath.HomeVelocity(ref vel, new Vec3(0, 0, 0), new Vec3(10, 0, -10), 120 * System.Math.PI / 180 / 120);
            Assert.That(Deg(turned), Is.EqualTo(1).Within(1e-5)); // 120 deg/s at 120 Hz
            Assert.That(JsMath.Hypot(vel.x, vel.y, vel.z), Is.EqualTo(30).Within(1e-5));
            Assert.That(vel.x, Is.GreaterThan(0));
            // Already on target: no turn.
            var straight = new Vec3(0, 0, -30);
            Assert.That(AssistMath.HomeVelocity(ref straight, new Vec3(0, 0, 0), new Vec3(0, 0, -10), 1), Is.EqualTo(0));
        }

        [Test]
        public void HomeVelocity_ReachesTheTargetDirection_WhenTheGapIsUnderTheCap()
        {
            var vel = new Vec3(0, 0, -10);
            AssistMath.HomeVelocity(ref vel, new Vec3(0, 0, 0), new Vec3(0.1, 0, -10), 1);
            Assert.That(vel.x / JsMath.Hypot(vel.x, vel.z), Is.EqualTo(0.1 / JsMath.Hypot(0.1, 10)).Within(1e-6));
        }

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Husks husks;
            public readonly AimAssist assist;
            public readonly Loadout loadout;
            public readonly Projectiles projectiles;
            public Vec3 eye = new Vec3(0, 1.6, 0);
            public double yaw, pitch;
            public string mode = "snap";

            public Rig()
            {
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                husks = new Husks(cfg, events, enemies, world, -30);
                assist = new AimAssist(cfg, events, enemies, world, () => (eye, AssistMath.LookDir(yaw, pitch)), () => "nailgun", () => false, () => mode, () => cfg.Num("settings.snapStrength"));
                var mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (eye, AssistMath.LookDir(yaw, pitch))) { assist = assist };
                loadout.arsenal.enabled = false;
                projectiles = new Projectiles(cfg, events, world, enemies, 7);
                loadout.projectiles = projectiles;
            }

            public Husk Husk(double x, double z)
            {
                Husk h = husks.Spawn(new Vec3(x, 0, z), new Vec3(0, 0, 0));
                h.hp = h.maxHp = 1e6;
                return h;
            }
        }

        [Test]
        public void HomingTarget_NullWithoutALock_ManualLockHomesTwiceAsHard_OffNever()
        {
            var r = new Rig();
            Husk h = r.Husk(0, -12);
            Assert.That(r.assist.HomingTarget(), Is.Null, "no lock, no homing");
            r.assist.ToggleLock(); // V: a manual lock on the enemy nearest the crosshair
            HomingTarget? t = r.assist.HomingTarget();
            Assert.That(t, Is.Not.Null);
            Assert.That(t.Value.enemy, Is.SameAs(h));
            Assert.That(t.Value.id, Is.EqualTo(h.id));
            AssistMath.Params p = r.assist.Params();
            double k = p != null ? p.k : 1;
            Assert.That(t.Value.turn, Is.EqualTo(cfg.Num("aim.homingTurn") * DEG * k * cfg.Num("aim.lockHomingMult")));
            r.mode = "off";
            Assert.That(r.assist.HomingTarget(), Is.Null, "OFF never homes");
        }

        [Test]
        public void ANailFiredAtALockedEnemyOffTheAimLine_TurnsOntoIt_AndItsHitIsAssisted()
        {
            var r = new Rig();
            Husk h = r.Husk(1.6, -14); // off the aim line: a straight nail passes it
            r.assist.ToggleLock();
            Assert.That(r.assist.ManualTarget, Is.SameAs(h));
            r.loadout.Reset(new List<string> { "nailgun" }, 1);
            r.loadout.arsenal.enabled = false;
            var hits = new List<HitEvent>();
            r.events.On<HitEvent>("hit", e => hits.Add(e));
            r.loadout.Step(new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } }, cfg.Dt);
            Assert.That(r.projectiles.Count("nail"), Is.EqualTo(1), "one nail out");
            Projectiles.Projectile nail = r.projectiles.nails[0];
            Assert.That(nail.hasHoming, Is.True, "the nail took the lock as its homing target");
            for (int i = 0; i < 60 && hits.Count == 0; i++) r.projectiles.Step(cfg.Dt);
            Assert.That(hits.Count, Is.EqualTo(1), "the homing nail hit the locked husk");
            Assert.That(hits[0].enemy, Is.EqualTo(h.id));
            Assert.That(nail.turned, Is.GreaterThan(cfg.Num("aim.homingAssistAngle")));
            Assert.That(hits[0].assisted, Is.True, "turned past homingAssistAngle: an assisted hit");
        }
    }
}
