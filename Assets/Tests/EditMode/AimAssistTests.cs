// NIGHT7 if-time 4: the aim-assist lock (aimAssist.js) at the web's default (SNAP, strength 60) against a still Husk on a
// floor: a target 5 deg off the crosshair is pre-locked within the acquire time and the spring settles the look on it;
// OFF never moves the look; a fast flick away breaks the lock and it can't re-lock for reacquireDelay; a press while the
// spring is still closing waits at most fire.maxHold; an assisted hit carries lockPayout(60) to the points and style.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class AimAssistTests
    {
        static GameConfig cfg;
        const double DEG = System.Math.PI / 180;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        sealed class Rig
        {
            public EventBus events = new EventBus();
            public EnemyRegistry enemies = new EnemyRegistry();
            public TriangleWorld world = new TriangleWorld();
            public Husks husks;
            public AimAssist assist;
            public Vec3 eye = new Vec3(0, 1.6, 0);
            public double yaw, pitch;
            public string mode = "snap";
            public readonly List<string> log = new List<string>();
            public Husk husk;

            public Rig()
            {
                world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
                husks = new Husks(cfg, events, enemies, world, -30);
                assist = new AimAssist(cfg, events, enemies, world, () => (eye, AssistMath.LookDir(yaw, pitch)), () => "revolver", () => false, () => mode, () => cfg.Num("settings.snapStrength"));
                events.On<AimLockEvent>("aimLock", e => log.Add("lock " + e.part));
                events.On("aimRelease", r => log.Add("release " + r));
                husk = husks.Spawn(new Vec3(0, 0, -10), new Vec3(0, 0, 0));
                // Look 5 deg to the side of its chest.
                var (y, p) = AssistMath.YawPitchTo(eye, husk.AimPoint);
                yaw = y + 5 * DEG;
                pitch = p;
            }

            public CombatInput Step(bool press = false)
            {
                var frame = new CombatInput { weapon = -1, fire = new Button { held = press, pressed = press } };
                assist.Step(cfg.Dt, ref frame, ref yaw, ref pitch);
                return frame;
            }
        }

        [Test]
        public void Snap_PreLocksNearTheCrosshair_AndSettlesOnIt()
        {
            var r = new Rig();
            AssistMath.Params p = r.assist.Params();
            Assert.That(p, Is.Not.Null);
            Assert.That(p.cone, Is.EqualTo(cfg.Num("aim.lock.modes.snap.cone")), "strength 60 = the reference: the mode's own numbers");
            int lockedAt = -1;
            for (int i = 0; i < 120; i++)
            {
                r.Step();
                if (lockedAt < 0 && r.assist.Target != null) lockedAt = i + 1;
            }
            Assert.That(r.log, Is.Not.Empty, "a pre-lock");
            Assert.That(lockedAt * cfg.Dt, Is.LessThanOrEqualTo(p.acquireTime + cfg.Num("aim.lock.scanEvery") * cfg.Dt * 2 + 1e-9), "within the acquire time (scans every 2 steps)");
            LockInfo info = r.assist.Info();
            Assert.That(info.error, Is.LessThan(cfg.Num("aim.lock.settleError")), $"settled on the {info.part}: {info.error:0.000} deg off");
            Assert.That(info.settled, Is.True);
        }

        [Test]
        public void Off_NeverMovesTheLook()
        {
            var r = new Rig { };
            r.mode = "off";
            double yaw = r.yaw, pitch = r.pitch;
            for (int i = 0; i < 120; i++) r.Step(i == 30);
            Assert.That(r.yaw, Is.EqualTo(yaw));
            Assert.That(r.pitch, Is.EqualTo(pitch));
            Assert.That(r.log, Is.Empty);
            Assert.That(r.assist.Payout(), Is.EqualTo(1));
        }

        [Test]
        public void AFlickAway_BreaksTheLock_AndBlocksAReLock()
        {
            var r = new Rig();
            for (int i = 0; i < 60; i++) r.Step();
            Assert.That(r.assist.Target, Is.SameAs(r.husk));
            r.yaw += 0.3; // 0.3 rad in one step: far past the breakaway speed, away from it
            r.Step();
            Assert.That(r.log, Does.Contain("release breakaway"));
            Assert.That(r.assist.Target, Is.Null);
            // Back onto it: blocked for reacquireDelay, then it locks again.
            var (y, p) = AssistMath.YawPitchTo(r.eye, r.husk.AimPoint);
            r.yaw = y + 1 * DEG;
            r.pitch = p;
            int blockedSteps = MovementMath.StepsFor(cfg.Num("aim.lock.reacquireDelay"), cfg.Dt);
            for (int i = 0; i < blockedSteps - 6; i++)
            {
                r.Step();
                Assert.That(r.assist.Target, Is.Null, $"blocked at step {i}");
            }
            for (int i = 0; i < 60 && r.assist.Target == null; i++) r.Step();
            Assert.That(r.assist.Target, Is.SameAs(r.husk), "it re-locks after the delay");
        }

        [Test]
        public void APressWhileClosing_WaitsAtMostMaxHold_AndTheHitIsAssisted()
        {
            var r = new Rig();
            // Acquire, but press at once on the lock's first steps (the spring still closing from 5 deg).
            int locked = -1;
            for (int i = 0; i < 40 && locked < 0; i++)
            {
                r.Step();
                if (r.assist.Target != null) locked = i;
            }
            Assert.That(locked, Is.GreaterThanOrEqualTo(0));
            int waited = 0;
            CombatInput f = r.Step(true);
            while (!f.fire.pressed && waited < 20)
            {
                waited++;
                f = r.Step(false);
            }
            Assert.That(f.fire.pressed && f.fire.held, Is.True, "the held press leaves");
            Assert.That(waited * cfg.Dt, Is.LessThanOrEqualTo(cfg.Num("aim.lock.fire.maxHold") + cfg.Dt + 1e-9), $"held {waited} steps");
            // The shot: a loadout with this assist sees it as assisted and the hit carries the payout.
            var mover = new Mover(cfg.movement, r.world);
            mover.Reset(0, 0, 0);
            var health = new Health(100, r.events, "player");
            var loadout = new Loadout(cfg, r.events, r.enemies, r.world, mover, health, () => (r.eye, AssistMath.LookDir(r.yaw, r.pitch))) { assist = r.assist };
            var hits = new List<HitEvent>();
            r.events.On<HitEvent>("hit", e => hits.Add(e));
            r.husk.hp = 1000;
            loadout.Step(f, cfg.Dt);
            Assert.That(hits.Count, Is.EqualTo(1), "the shot hit");
            Assert.That(hits[0].assisted, Is.True);
            Assert.That(hits[0].assistPay, Is.EqualTo(AssistMath.LockPayout(cfg.Num("settings.snapStrength"), r.assist.lk)));
            Assert.That(hits[0].assistPay, Is.LessThan(1));
        }
    }
}
