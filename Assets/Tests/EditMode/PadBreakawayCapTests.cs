// NIGHT13 U3-26.7: the pad's breakaway cap (aimAssist.js :34-39, :434; main.js :377-379: on the pad,
// gamepad.yawRate x settings.padSensX x gamepad.breakawayShare deg/s; +Infinity otherwise). The same SNAP lock on a still
// Husk as AimAssistTests: a turn away between the cap and the mode's own breakaway (450 deg/s) keeps the lock with no cap
// (the mouse) and breaks it with the pad's cap; with the default (no cap given) nothing changes.
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class PadBreakawayCapTests
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
            public readonly AimAssist assist;
            public readonly Husk husk;
            public readonly Vec3 eye = new Vec3(0, 1.6, 0);
            public double yaw, pitch;
            public int breakaways;

            public Rig(System.Func<double> cap)
            {
                world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
                var husks = new Husks(cfg, events, enemies, world, -30);
                assist = cap == null
                    ? new AimAssist(cfg, events, enemies, world, () => (eye, AssistMath.LookDir(yaw, pitch)), () => "revolver", () => false, () => "snap", () => cfg.Num("settings.snapStrength"))
                    : new AimAssist(cfg, events, enemies, world, () => (eye, AssistMath.LookDir(yaw, pitch)), () => "revolver", () => false, () => "snap", () => cfg.Num("settings.snapStrength"), cap);
                events.On("aimRelease", r => { if ((string)r == "breakaway") breakaways++; });
                husk = husks.Spawn(new Vec3(0, 0, -10), new Vec3(0, 0, 0));
                var (y, p) = AssistMath.YawPitchTo(eye, husk.AimPoint);
                yaw = y + 5 * DEG;
                pitch = p;
            }

            public void Step()
            {
                var frame = new CombatInput { weapon = -1 };
                assist.Step(cfg.Dt, ref frame, ref yaw, ref pitch);
            }
        }

        static double PadCap() => cfg.Num("gamepad.yawRate") * cfg.Num("settings.padSensX") * cfg.Num("gamepad.breakawayShare");

        /// <summary>Lock, then turn away at `degPerS` for one step; true when the lock broke away.</summary>
        static bool BreaksAway(System.Func<double> cap, double degPerS)
        {
            var r = new Rig(cap);
            for (int i = 0; i < 60; i++) r.Step();
            Assert.That(r.assist.Target, Is.SameAs(r.husk), "locked before the turn");
            r.yaw += degPerS * DEG * cfg.Dt;
            r.Step();
            return r.breakaways > 0;
        }

        [Test]
        public void ThePadsCap_BreaksAwayATurnTheMouseLockHolds()
        {
            double cap = PadCap();
            double modeBreakaway = cfg.Num("aim.lock.modes.snap.breakaway");
            Assert.That(cap, Is.LessThan(modeBreakaway), "the pad's cap sits under SNAP's own breakaway");
            double turn = (cap + modeBreakaway) / 2;
            Assert.That(BreaksAway(null, turn), Is.False, "no cap given (the default): the mode's breakaway");
            Assert.That(BreaksAway(() => double.PositiveInfinity, turn), Is.False, "the mouse: +Infinity");
            Assert.That(BreaksAway(PadCap, turn), Is.True, "the pad: min(breakaway, cap)");
        }
    }
}
