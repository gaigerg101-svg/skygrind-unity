// NIGHT60 builder L (B9, T-138: the focus pause outside the smoke): the Player probe -skygrindFocusProbe60's verdict (FocusProbe60Rule,
// FocusProbe60.cs). The probe watches a real run (no smoke, no reel, no attract) once per frame after the frame's Updates and prints
// "[focus60] DONE: paused on the focus loss True|False". Pinned:
//   1. a focus loss while Playing with the cursor locked, then a Playing -> Paused change with the window unfocused: True (in the same
//      frame, or a frame later)
//   2. a focus loss the run rides through (the planted -skygrindFocusProbe60NoPause resolves the pause before the probe looks): False
//   3. a pause with the window focused (Esc, a MODS draft, a reward reveal) is not the focus loss's: False
//   4. nothing counts before the probe is ready (Playing with the cursor locked, seen once)
//   5. the DONE line's exact text (n60-focus-probe.sh greps it)
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class N60FocusProbeTests
    {
        static FocusProbe60Rule Ready()
        {
            var r = new FocusProbe60Rule();
            r.Observe(Flow.Title, false, true);
            r.Observe(Flow.Playing, true, true);
            r.Observe(Flow.Playing, true, true);
            return r;
        }

        [Test]
        public void FocusLostWhilePlaying_PausedInTheSameFrame_True()
        {
            FocusProbe60Rule r = Ready();
            r.Observe(Flow.Paused, false, false); // the steal: the focus lost, the cursor lock dropped, SurvivalGame paused in that frame
            Assert.IsTrue(r.Ready, "ready: Playing with the cursor locked");
            Assert.AreEqual(1, r.FocusLosses, "one focus loss");
            Assert.AreEqual(1, r.LockDrops, "one cursor-lock drop");
            Assert.AreEqual(1, r.Pauses, "one Playing -> Paused change");
            Assert.IsTrue(r.PausedOnFocusLoss, "paused on the focus loss");
        }

        [Test]
        public void FocusLostWhilePlaying_PausedAFrameLater_True()
        {
            FocusProbe60Rule r = Ready();
            r.Observe(Flow.Playing, true, false); // the focus went first, the lock still reads Locked
            r.Observe(Flow.Paused, false, false);
            Assert.AreEqual(1, r.FocusLosses);
            Assert.IsTrue(r.PausedOnFocusLoss);
        }

        [Test]
        public void FocusLostAndTheRunRidesThrough_False()
        {
            FocusProbe60Rule r = Ready();
            for (int i = 0; i < 5; i++) r.Observe(Flow.Playing, false, false); // -skygrindFocusProbe60NoPause: the pause resolved before the probe looks
            r.Observe(Flow.Playing, false, true);
            Assert.AreEqual(1, r.FocusLosses, "the focus loss is seen");
            Assert.AreEqual(0, r.Pauses, "no pause seen");
            Assert.IsFalse(r.PausedOnFocusLoss);
        }

        [Test]
        public void APauseWithTheWindowFocused_IsNotTheFocusLoss()
        {
            FocusProbe60Rule r = Ready();
            r.Observe(Flow.Paused, false, true); // Esc, a MODS draft, a reward reveal: Pause() frees the cursor, the window keeps the focus
            Assert.AreEqual(1, r.Pauses, "the pause is seen");
            Assert.AreEqual(0, r.FocusLosses);
            Assert.IsFalse(r.PausedOnFocusLoss, "not the focus loss's pause");
        }

        [Test]
        public void NothingCountsBeforeReady()
        {
            var r = new FocusProbe60Rule();
            r.Observe(Flow.Playing, false, true); // Playing, but the cursor never locked
            r.Observe(Flow.Paused, false, false);
            Assert.IsFalse(r.Ready);
            Assert.AreEqual(0, r.Pauses);
            Assert.IsFalse(r.PausedOnFocusLoss);
        }

        [Test]
        public void TheDoneLine()
        {
            Assert.AreEqual("[focus60] DONE: paused on the focus loss True", FocusProbe60Rule.DoneLine(true));
            Assert.AreEqual("[focus60] DONE: paused on the focus loss False", FocusProbe60Rule.DoneLine(false));
        }
    }
}
