// NIGHT14 U3-27.7 (agent K): the kiosk's pure logic (Kiosk.cs, kiosk.js's port): the -skygrindKiosk flag, the idle clock on
// frame time (config.kiosk.idleReset s without any input anywhere puts the game back on TITLE: kioskReset), any input
// restarting the clock and stopping the attract loop, the attract's start on TITLE (at once in the kiosk, the web's
// onTitle; after design.attract.idleSeconds on a normal TITLE, D-54; 0 = never) and debugIdle.
// Frames of 0.25 s (exact in binary) so the thresholds land on whole frames.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class KioskTests
    {
        const double Dt = 0.25;
        const double IdleReset = 60; // config.kiosk.idleReset
        const double IdleSeconds = 30; // design.attract.idleSeconds

        /// <summary>`seconds` of frames with no input; the first step that isn't None (with how long it took), else None.</summary>
        static Kiosk.Step Idle(Kiosk k, double seconds, bool isTitle, bool attractOn, bool canAttract, out double at)
        {
            at = 0;
            for (double t = 0; t < seconds; t += Dt)
            {
                Kiosk.Step s = k.Update(Dt, false, isTitle, attractOn, canAttract);
                at = t + Dt;
                if (s != Kiosk.Step.None) return s;
            }
            return Kiosk.Step.None;
        }

        static Kiosk.Step Idle(Kiosk k, double seconds, bool isTitle, bool attractOn = false, bool canAttract = true) => Idle(k, seconds, isTitle, attractOn, canAttract, out _);

        /// <summary>Idle frames with no attract running and nothing in its way; `at`: when the first step came.</summary>
        static Kiosk.Step IdleUntil(Kiosk k, double seconds, bool isTitle, out double at) => Idle(k, seconds, isTitle, false, true, out at);

        [Test]
        public void FlagParse()
        {
            Assert.IsTrue(Kiosk.FlagIn(new[] { "SKYGRIND.exe", "-skygrindKiosk" }));
            Assert.IsTrue(Kiosk.FlagIn(new[] { "SKYGRIND.exe", "-logFile", "k.log", "-skygrindKiosk" }));
            Assert.IsTrue(Kiosk.FlagIn(new[] { "SKYGRIND.exe", "-SKYGRINDKIOSK" }), "the flag ignores case (typed by hand into a booth's shortcut)");
            Assert.IsFalse(Kiosk.FlagIn(null));
            Assert.IsFalse(Kiosk.FlagIn(new string[0]));
            Assert.IsFalse(Kiosk.FlagIn(new[] { "SKYGRIND.exe" }));
            Assert.IsFalse(Kiosk.FlagIn(new[] { "SKYGRIND.exe", "-skygrindKioskX", "skygrindKiosk", "--skygrindKiosk", "-skygrindDev" }), "only the exact flag");
            Assert.IsFalse(Kiosk.FlagIn(new[] { "SKYGRIND.exe", null }), "a null argument is skipped");
            Assert.AreEqual("-skygrindKiosk", Kiosk.Flag);
        }

        [Test]
        public void IdleResetSendsThePlayerToTitle()
        {
            var k = new Kiosk(true, IdleReset, IdleSeconds);
            Assert.AreEqual(Kiosk.Step.None, Idle(k, IdleReset - Dt, false), "59.75 s idle in a run: nothing yet");
            Assert.AreEqual(Kiosk.Step.Reset, k.Update(Dt, false, false, false, true), "60 s without input: kioskReset, back to TITLE");
            Assert.AreEqual(1, k.resets);
            Assert.AreEqual(0, k.idle, "the clock starts again");
            Assert.AreEqual(Kiosk.Step.None, Idle(k, IdleReset - Dt, false), "the next reset is a full idleReset later");
            Assert.AreEqual(Kiosk.Step.Reset, k.Update(Dt, false, false, false, true));
            Assert.AreEqual(2, k.resets);
        }

        [Test]
        public void IdleResetCountsInEveryScreenButTitle()
        {
            // PAUSED, DOWN, RESULTS and the run are all "not TITLE" (the web's isTitle: title, or the attract behind it).
            var k = new Kiosk(true, IdleReset, IdleSeconds);
            Assert.AreEqual(Kiosk.Step.Reset, IdleUntil(k, 10 * IdleReset, false, out double at));
            Assert.AreEqual(IdleReset, at, 1e-9);
        }

        [Test]
        public void NoIdleResetOnTitleOrWithoutTheFlag()
        {
            var title = new Kiosk(true, IdleReset, IdleSeconds);
            Assert.AreNotEqual(Kiosk.Step.Reset, Idle(title, 10 * IdleReset, true, attractOn: true), "TITLE (with the attract behind it) never resets");
            Assert.AreEqual(0, title.resets);
            var off = new Kiosk(false, IdleReset, IdleSeconds);
            Assert.AreEqual(Kiosk.Step.None, Idle(off, 10 * IdleReset, false), "no kiosk: no idle reset");
            Assert.AreEqual(0, off.resets);
        }

        [Test]
        public void InputRestartsTheClock()
        {
            var k = new Kiosk(true, IdleReset, IdleSeconds);
            Assert.AreEqual(Kiosk.Step.None, Idle(k, 50, false));
            Assert.AreEqual(Kiosk.Step.None, k.Update(Dt, true, false, false, true), "an input in a run is just an input");
            Assert.AreEqual(0, k.idle);
            Assert.AreEqual(Kiosk.Step.None, Idle(k, 50, false), "50 + 50 s with an input between: no reset");
            Assert.AreEqual(Kiosk.Step.Reset, Idle(k, 20, false), "60 s after the input: the reset");
        }

        [Test]
        public void DebugIdlePretendsNobodyTouchedAnything()
        {
            var k = new Kiosk(true, IdleReset, IdleSeconds);
            k.DebugIdle(IdleReset);
            Assert.AreEqual(Kiosk.Step.Reset, k.Update(Dt, false, false, false, true));
            Assert.AreEqual(1, k.resets);
        }

        [Test]
        public void TheKioskStartsTheAttractWhenTitleComesUp()
        {
            var k = new Kiosk(true, IdleReset, IdleSeconds);
            Assert.AreEqual(Kiosk.Step.None, k.Update(Dt, false, false, false, true), "in a run");
            Assert.AreEqual(Kiosk.Step.StartAttract, k.Update(Dt, false, true, false, true), "TITLE came up: the attract at once (kiosk.js onTitle)");
            Assert.AreEqual(Kiosk.Step.None, k.Update(Dt, false, true, true, true), "running: nothing more");
        }

        [Test]
        public void TheKioskAttractWaitsUntilItCanStart()
        {
            var k = new Kiosk(true, IdleReset, IdleSeconds);
            Assert.AreEqual(Kiosk.Step.None, k.Update(Dt, false, true, false, false), "TITLE, but a screen is open over it (or no autopilot)");
            Assert.AreEqual(Kiosk.Step.None, k.Update(Dt, false, true, false, false));
            Assert.AreEqual(Kiosk.Step.StartAttract, k.Update(Dt, false, true, false, true), "the pending start goes once it can");
        }

        [Test]
        public void KioskResetThenTheAttract()
        {
            var k = new Kiosk(true, IdleReset, IdleSeconds);
            Assert.AreEqual(Kiosk.Step.Reset, Idle(k, 2 * IdleReset, false));
            Assert.AreEqual(Kiosk.Step.StartAttract, k.Update(Dt, false, true, false, true), "back on TITLE: the attract plays for the next visitor");
        }

        [Test]
        public void AnyInputStopsTheAttract()
        {
            var k = new Kiosk(true, IdleReset, IdleSeconds);
            Assert.AreEqual(Kiosk.Step.StartAttract, k.Update(Dt, false, true, false, true));
            Assert.AreEqual(Kiosk.Step.None, Idle(k, 300, true, attractOn: true), "five minutes of attract, no input: it keeps playing");
            Assert.AreEqual(Kiosk.Step.StopAttract, k.Update(Dt, true, true, true, true), "any input: back to TITLE at once");
            Assert.AreEqual(0, k.idle);
            var off = new Kiosk(false, IdleReset, IdleSeconds);
            Assert.AreEqual(Kiosk.Step.StopAttract, off.Update(Dt, true, true, true, true), "on a normal TITLE too");
        }

        [Test]
        public void AfterAnInputTheKioskAttractComesBackAfterIdleSeconds()
        {
            var k = new Kiosk(true, IdleReset, IdleSeconds);
            k.Update(Dt, false, true, false, true);
            Assert.AreEqual(Kiosk.Step.StopAttract, k.Update(Dt, true, true, true, true));
            Assert.AreEqual(Kiosk.Step.StartAttract, IdleUntil(k, 10 * IdleReset, true, out double at));
            Assert.AreEqual(IdleSeconds, at, 1e-9, "design.attract.idleSeconds of idle on TITLE");
            Assert.AreEqual(IdleSeconds, k.AttractAfter);
        }

        [Test]
        public void KioskWithIdleSecondsZeroComesBackAfterIdleReset()
        {
            // "0 = never" is a normal TITLE's; the kiosk always runs it, so it falls back to its own idle clock.
            var k = new Kiosk(true, IdleReset, 0);
            Assert.AreEqual(IdleReset, k.AttractAfter);
            k.Update(Dt, false, true, false, true);
            Assert.AreEqual(Kiosk.Step.StopAttract, k.Update(Dt, true, true, true, true));
            Assert.AreEqual(Kiosk.Step.StartAttract, IdleUntil(k, 10 * IdleReset, true, out double at));
            Assert.AreEqual(IdleReset, at, 1e-9);
        }

        [Test]
        public void NormalTitleStartsTheAttractAfterIdleSeconds()
        {
            var k = new Kiosk(false, IdleReset, IdleSeconds);
            Assert.AreEqual(Kiosk.Step.None, k.Update(Dt, true, true, false, true), "TITLE comes up with a click");
            Assert.AreEqual(Kiosk.Step.StartAttract, IdleUntil(k, 10 * IdleSeconds, true, out double at), "D-54: on after idleSeconds");
            Assert.AreEqual(IdleSeconds, at, 1e-9, "not at once (only the kiosk starts it when TITLE comes up)");
        }

        [Test]
        public void NormalTitleWithIdleSecondsZeroNeverStarts()
        {
            var k = new Kiosk(false, IdleReset, 0);
            Assert.IsTrue(double.IsPositiveInfinity(k.AttractAfter));
            Assert.AreEqual(Kiosk.Step.None, Idle(k, 20 * IdleReset, true), "0 = never");
        }

        [Test]
        public void TheAttractStartsOnlyOnTitle()
        {
            var k = new Kiosk(false, IdleReset, IdleSeconds);
            Assert.AreEqual(Kiosk.Step.None, Idle(k, 10 * IdleSeconds, false), "a run left alone (PAUSED, DOWN) is not TITLE");
            Assert.AreEqual(Kiosk.Step.StartAttract, k.Update(Dt, false, true, false, true), "the clock kept counting: TITLE starts it at once");
            var blocked = new Kiosk(false, IdleReset, IdleSeconds);
            Assert.AreEqual(Kiosk.Step.None, Idle(blocked, 10 * IdleSeconds, true, attractOn: false, canAttract: false), "a screen open over TITLE (or no autopilot)");
        }

        [Test]
        public void TheInputFrameNeverStartsTheAttract()
        {
            var k = new Kiosk(true, IdleReset, IdleSeconds);
            Assert.AreEqual(Kiosk.Step.None, k.Update(Dt, false, false, false, true));
            Assert.AreEqual(Kiosk.Step.None, k.Update(Dt, true, true, false, true), "TITLE came up under a click (RESULTS' TITLE): somebody is there");
            Assert.AreEqual(Kiosk.Step.StartAttract, IdleUntil(k, 10 * IdleSeconds, true, out double at));
            Assert.AreEqual(IdleSeconds, at, 1e-9, "the attract waits for the idle clock instead");
        }
    }
}
