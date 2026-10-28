// NIGHT55 S (P0-smoke): the smoke guard's rule (Smoke55, SliceSmoke.N55.cs): which Playing -> Paused change is the smoke's
// own, which it resolves, and the failing step's verdict. RED before NIGHT55: no rule (the smoke had no guard and a focus
// change paused its run: Logs/n55/smoke-a0.out, 19 failed, EXFIL "Paused").
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Smoke55Tests
    {
        [Test]
        public void NoTransitionIsNoCause()
        {
            Assert.AreEqual(Smoke55.PauseCause.None, Smoke55.Classify(false, true, false, false, false, false), "already paused before the frame (the smoke's own g.Pause in its coroutine)");
            Assert.AreEqual(Smoke55.PauseCause.None, Smoke55.Classify(true, false, true, true, true, true), "still playing");
            Assert.IsFalse(Smoke55.Resolves(Smoke55.PauseCause.None));
        }

        [Test]
        public void TheSmokesPadStartIsItsOwn()
        {
            Smoke55.PauseCause c = Smoke55.Classify(true, true, false, false, true, true);
            Assert.AreEqual(Smoke55.PauseCause.Own, c);
            Assert.IsFalse(Smoke55.Resolves(c), "the pad steps press START to pause: never undone");
        }

        [Test]
        public void EveryOtherPauseIsNamedAndResolved()
        {
            Assert.AreEqual(Smoke55.PauseCause.LockDropped, Smoke55.Classify(true, true, false, false, false, false), "SurvivalGame.cs:295: the cursor lock dropped (a focus change)");
            Assert.AreEqual(Smoke55.PauseCause.RealPadStart, Smoke55.Classify(true, true, false, false, true, false), "a real pad's START with no smoke pad live");
            Assert.AreEqual(Smoke55.PauseCause.Draft, Smoke55.Classify(true, true, true, false, false, false));
            Assert.AreEqual(Smoke55.PauseCause.Reveal, Smoke55.Classify(true, true, false, true, false, false));
            Assert.AreEqual(Smoke55.PauseCause.Draft, Smoke55.Classify(true, true, true, false, true, false), "a draft is named before a pad press");
            foreach (Smoke55.PauseCause c in new[] { Smoke55.PauseCause.LockDropped, Smoke55.PauseCause.RealPadStart, Smoke55.PauseCause.Draft, Smoke55.PauseCause.Reveal })
            {
                Assert.IsTrue(Smoke55.Resolves(c), c.ToString());
                Assert.AreNotEqual("none", Smoke55.Name(c, false), c.ToString());
            }
            StringAssert.Contains("lost the focus", Smoke55.Name(Smoke55.PauseCause.LockDropped, false));
            StringAssert.Contains("focused", Smoke55.Name(Smoke55.PauseCause.LockDropped, true));
        }

        [Test]
        public void TheVerdictNamesTheMostDirectCause()
        {
            StringAssert.Contains("did not make", Smoke55.Verdict(2, "x", 10, 0, 5, false));
            StringAssert.Contains("PAUSED 10", Smoke55.Verdict(0, "none", 10, 0, 5, false));
            StringAssert.Contains("no sim step", Smoke55.Verdict(0, "none", 0, 0, 5, false));
            StringAssert.Contains("unfocused 5", Smoke55.Verdict(0, "none", 0, 120, 5, true));
            StringAssert.Contains("not active", Smoke55.Verdict(0, "none", 0, 120, 0, false));
            StringAssert.Contains("no pause, focus or device cause", Smoke55.Verdict(0, "none", 0, 120, 0, true));
        }

        [Test]
        public void TheSmokesPadsAreKnownByName()
        {
            foreach (string n in new[] { "Smoke pad", "Smoke pad only", "Smoke grind pad" }) Assert.IsTrue(Smoke55.IsSmokePadName(n), n);
            foreach (string n in new[] { "XInputControllerWindows", "Wireless Gamepad", null, "" }) Assert.IsFalse(Smoke55.IsSmokePadName(n), n ?? "null");
        }
    }
}
