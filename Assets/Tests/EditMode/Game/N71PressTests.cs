// NIGHT71 builder F (LEFTOVER L10 press71; Game/Press71.cs). Pinned: the delivery rule never gives fire's press on two steps in a row, keeps
// every press (a held-back one comes on the next step without one), keeps the order, and forgets on Reset; the switch is off without
// -skygrindPress71 (the EditMode command line has none). The sim side (taps inside freezes 6, fired 3 -> 6) is the headless probe's:
// Research/2026-10-26-wave71/F/L10/taps71h-press71.txt.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class N71PressTests
    {
        static bool[] Run(params bool[] latched)
        {
            var p = new Press71();
            var d = new bool[latched.Length];
            for (int i = 0; i < latched.Length; i++) d[i] = p.Deliver(latched[i]);
            return d;
        }

        [Test]
        public void APressAfterAPressedStep_WaitsOneStep()
        {
            // the freeze case: the step that started it was pressed, the tap's press is latched for the next step
            Assert.That(Run(true, true, false), Is.EqualTo(new[] { true, false, true }));
        }

        [Test]
        public void SeparatePresses_Unchanged()
        {
            Assert.That(Run(true, false, true, false), Is.EqualTo(new[] { true, false, true, false }));
            Assert.That(Run(false, true, false, false), Is.EqualTo(new[] { false, true, false, false }));
        }

        [Test]
        public void NeverTwoInARow_EveryPressKept()
        {
            bool[] d = Run(true, true, true, false, false);
            Assert.That(d, Is.EqualTo(new[] { true, false, true, false, true }));
            int inN = 3, outN = 0;
            for (int i = 0; i < d.Length; i++)
            {
                if (d[i]) outN++;
                if (i > 0) Assert.That(d[i] && d[i - 1], Is.False, "two delivered presses in a row at " + i);
            }
            Assert.That(outN, Is.EqualTo(inN), "no press lost, none doubled");
        }

        [Test]
        public void Reset_ForgetsAHeldBackPress()
        {
            var p = new Press71();
            Assert.That(p.Deliver(true), Is.True);
            Assert.That(p.Deliver(true), Is.False, "held back");
            p.Reset();
            Assert.That(p.Deliver(false), Is.False, "a new run: nothing carried in");
        }

        [Test]
        public void Switch_OffWithoutTheFlag()
        {
            Press71.ForceOn(null);
            try { Assert.That(Press71.On, Is.False, "no -skygrindPress71 on the EditMode command line"); }
            finally { Press71.ForceOn(null); }
        }
    }
}
