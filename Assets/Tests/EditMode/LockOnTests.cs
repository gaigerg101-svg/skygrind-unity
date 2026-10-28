// NIGHT9 U3-10: aim assist's lock-on key (aimAssist.js toggleLock) and the melee lunge (lungeTarget; punch.js lunge,
// melee.js assistLunge). The web has no tests for either; these check the module's documented rules: V makes a sticky
// manual lock on the enemy nearest the crosshair (lockOn), again cycles to the next, alone it releases (lockOff
// 'toggle', blocked for reacquireDelay); a manual lock is never taken over by a better candidate; a punch / Gauntlet
// swing with a body just out of reach lunges you toward it and lands (an assisted hit), and OFF never lunges.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class LockOnTests
    {
        static GameConfig cfg;
        const double DEG = System.Math.PI / 180;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly EnemyRegistry enemies = new EnemyRegistry();
            public readonly TriangleWorld world = new TriangleWorld();
            public readonly Husks husks;
            public readonly AimAssist assist;
            public readonly Mover mover;
            public readonly Loadout loadout;
            public Vec3 eye = new Vec3(0, 1.6, 0);
            public double yaw, pitch;
            public string mode = "snap";
            public readonly List<string> log = new List<string>();

            public Rig()
            {
                world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
                husks = new Husks(cfg, events, enemies, world, -30);
                assist = new AimAssist(cfg, events, enemies, world, () => (eye, AssistMath.LookDir(yaw, pitch)), () => "revolver", () => false, () => mode, () => cfg.Num("settings.snapStrength"));
                events.On<AimLockEvent>("lockOn", e => log.Add("lockOn " + e.id));
                events.On("lockOff", r => log.Add("lockOff " + r));
                mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (eye, AssistMath.LookDir(yaw, pitch))) { assist = assist };
                loadout.arsenal.enabled = false;
            }

            public Husk Husk(double x, double z)
            {
                Husk h = husks.Spawn(new Vec3(x, 0, z), new Vec3(0, 0, 0));
                h.hp = h.maxHp = 1e6;
                return h;
            }

            public void Step()
            {
                var frame = new CombatInput { weapon = -1 };
                assist.Step(cfg.Dt, ref frame, ref yaw, ref pitch);
            }
        }

        [Test, Description("V locks the enemy nearest the crosshair (manual, lockOn), again the next one (wrapping), alone it releases (lockOff toggle)")]
        public void V_Locks_Cycles_AndReleases()
        {
            var r = new Rig();
            Husk a = r.Husk(0.6, -10); // about 3.4 deg off the crosshair (looking down -z)
            Husk b = r.Husk(-1.5, -10); // about 8.5 deg off
            r.Step();
            r.assist.ToggleLock();
            Assert.That(r.assist.ManualTarget, Is.SameAs(a), "the nearest to the crosshair");
            Assert.That(r.assist.Info().manual, Is.True);
            r.assist.ToggleLock();
            Assert.That(r.assist.ManualTarget, Is.SameAs(b), "again: the next one");
            Assert.That(r.assist.cycles, Is.EqualTo(1));
            r.assist.ToggleLock();
            Assert.That(r.assist.ManualTarget, Is.SameAs(a), "wrapping");
            Assert.That(r.log, Is.EqualTo(new[] { "lockOn " + a.id, "lockOn " + b.id, "lockOn " + a.id }), "manual to manual: no lockOff between");
            b.alive = false;
            r.assist.ToggleLock();
            Assert.That(r.assist.Target, Is.Null, "alone: V releases");
            Assert.That(r.log[r.log.Count - 1], Is.EqualTo("lockOff toggle"));
            // Blocked for reacquireDelay: no automatic pre-lock on it meanwhile.
            int blocked = MovementMath.StepsFor(r.assist.lk.reacquireDelay, cfg.Dt) - 2;
            for (int i = 0; i < blocked; i++) r.Step();
            Assert.That(r.assist.Target, Is.Null);
        }

        [Test, Description("a manual lock is never taken over by a better candidate (the hysteresis skips it)")]
        public void AManualLock_IsNotTakenOver()
        {
            var r = new Rig();
            Husk a = r.Husk(-1.5, -10);
            r.Step();
            r.assist.ToggleLock();
            Assert.That(r.assist.ManualTarget, Is.SameAs(a));
            Husk b = r.Husk(0.4, -6); // nearer and closer to the crosshair: it would win an automatic lock's challenge
            for (int i = 0; i < 120; i++) r.Step();
            Assert.That(r.assist.Target, Is.SameAs(a), "still the manual target after 1 s");
            Assert.That(r.assist.Info().manual, Is.True);
            _ = b;
        }

        [Test, Description("a punch at a body just out of reach: lunge (an impulse toward it, the 'lunge' event), the punch lands, assisted; OFF: no lunge, no hit")]
        public void APunchJustOutOfReach_Lunges_AndLands()
        {
            double reach = cfg.Num("punch.range"), lunge = cfg.Num("aim.lunge");
            var r = new Rig();
            Husk h = r.Husk(0, -(reach + lunge * 0.5 + 0.4));
            var (y, p) = AssistMath.YawPitchTo(r.eye, h.AimPoint);
            r.yaw = y;
            r.pitch = p;
            var lunges = new List<double>();
            r.events.On("lunge", g => lunges.Add((double)g));
            var hits = new List<HitEvent>();
            r.events.On<HitEvent>("hit", e => hits.Add(e));
            r.loadout.Step(new CombatInput { weapon = -1, punch = new Button { held = true, pressed = true } }, cfg.Dt);
            Assert.That(lunges.Count, Is.EqualTo(1), "a lunge");
            Assert.That(lunges[0], Is.GreaterThan(0).And.LessThanOrEqualTo(lunge + 1e-9));
            Assert.That(r.mover.velocity.z, Is.LessThan(0), "pushed toward the husk (-z)");
            Assert.That(hits.Count, Is.EqualTo(1), "the punch lands");
            Assert.That(hits[0].assisted, Is.True, "a lunged punch counts as assisted");

            var off = new Rig { mode = "off" };
            Husk h2 = off.Husk(0, -(reach + lunge * 0.5 + 0.4));
            var (y2, p2) = AssistMath.YawPitchTo(off.eye, h2.AimPoint);
            off.yaw = y2;
            off.pitch = p2;
            int offHits = 0;
            off.events.On<HitEvent>("hit", e => offHits++);
            off.loadout.Step(new CombatInput { weapon = -1, punch = new Button { held = true, pressed = true } }, cfg.Dt);
            Assert.That(offHits, Is.EqualTo(0));
            Assert.That(off.mover.velocity.z, Is.EqualTo(0));
        }

        [Test, Description("the Gauntlet swing lunges too (melee.js assistLunge) and the swing lands on the body within range + lunge")]
        public void TheGauntletSwing_Lunges()
        {
            double reach = cfg.Num("weapons.gauntlet.range"), lunge = cfg.Num("aim.lunge");
            var r = new Rig();
            if (!r.loadout.owned.Contains("gauntlet")) r.loadout.AddWeapon("gauntlet");
            int slot = r.loadout.owned.IndexOf("gauntlet"); // weapons.survivalStart may already hold it
            for (int i = 0; i < 200 && !(r.loadout.Weapon == "gauntlet" && r.loadout.ready); i++) r.loadout.Step(new CombatInput { weapon = slot }, cfg.Dt);
            Assert.That(r.loadout.Weapon, Is.EqualTo("gauntlet"));
            Husk h = r.Husk(0, -(reach + lunge * 0.5 + 0.4));
            var (y, p) = AssistMath.YawPitchTo(r.eye, h.AimPoint);
            r.yaw = y;
            r.pitch = p;
            var hits = new List<HitEvent>();
            r.events.On<HitEvent>("hit", e => hits.Add(e));
            int lunges = 0;
            r.events.On("lunge", g => lunges++);
            r.loadout.Step(new CombatInput { weapon = -1, fire = new Button { held = true, pressed = true } }, cfg.Dt);
            Assert.That(lunges, Is.EqualTo(1));
            Assert.That(hits.Count, Is.EqualTo(1));
            Assert.That(hits[0].weapon, Is.EqualTo("gauntlet"));
            Assert.That(hits[0].assisted, Is.True);
        }
    }
}
