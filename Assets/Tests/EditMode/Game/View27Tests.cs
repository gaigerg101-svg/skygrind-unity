// NIGHT27 builder V: the pure maths of the slow-aim cue, the skate feel and the roof's AO (Game/SlowAim27.cs, SkateMath27.cs).
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class View27Tests
    {
        [Test]
        public void SlowAimScaleLerps()
        {
            Assert.AreEqual(1f, SlowAim27.Scale(0f, 0.75f), 1e-6);
            Assert.AreEqual(0.75f, SlowAim27.Scale(1f, 0.75f), 1e-6);
            Assert.AreEqual(0.875f, SlowAim27.Scale(0.5f, 0.75f), 1e-6);
            Assert.AreEqual(0.75f, SlowAim27.Scale(3f, 0.75f), 1e-6, "slow01 is clamped");
        }

        [Test]
        public void SlowAimEaseTakesTheConfiguredTimes()
        {
            float v = 0;
            int steps = 0;
            while (v < 1 && steps < 1000) { v = SlowAim27.Ease(v, true, 0.01f, 0.13f, 0.1f); steps++; }
            Assert.That(steps, Is.InRange(13, 14), "in over 130 ms at 10 ms steps");
            steps = 0;
            while (v > 0 && steps < 1000) { v = SlowAim27.Ease(v, false, 0.01f, 0.13f, 0.1f); steps++; }
            Assert.That(steps, Is.InRange(10, 11), "out over 100 ms");
        }

        [Test]
        public void SkateFovAddAndCap()
        {
            Assert.AreEqual(0, SkateMath27.FovAdd(8, 8, 1.5, 10, 12), 1e-9);
            Assert.AreEqual(0, SkateMath27.FovAdd(4, 8, 1.5, 10, 12), 1e-9);
            Assert.AreEqual(10, SkateMath27.FovAdd(12, 8, 1.5, 10, 12), 1e-9);
            Assert.AreEqual(10, SkateMath27.FovAdd(40, 8, 1.5, 10, 12), 1e-9);
            Assert.AreEqual(12, SkateMath27.FovAdd(40, 8, 1.5, 20, 12), 1e-9, "the cap wins");
        }

        [Test]
        public void SkateRollAndStrength()
        {
            Assert.AreEqual(4, SkateMath27.Roll(99, 10, 4), 1e-9);
            Assert.AreEqual(-2, SkateMath27.Roll(-5, 10, 4), 1e-9);
            Assert.AreEqual(0, SkateMath27.Strength(7, 24, 0.3, 1), 1e-9);
            Assert.AreEqual(1, SkateMath27.Strength(24, 24, 0.3, 1), 1e-9);
            Assert.AreEqual(0.5, SkateMath27.Strength(24 * 0.65, 24, 0.3, 1), 1e-9);
        }

        [Test]
        public void SmoothUpDownIsSlowerOut()
        {
            double up = SkateMath27.SmoothUpDown(0, 10, 0.05, 0.15, 0.4);
            double down = 10 - SkateMath27.SmoothUpDown(10, 0, 0.05, 0.15, 0.4);
            Assert.Greater(up, down);
        }

        [Test]
        public void AoAndHeadingAndStrips()
        {
            Assert.AreEqual(0.6, SkateMath27.Ao(0.6, 0, 0.6), 1e-9);
            Assert.AreEqual(0.8, SkateMath27.Ao(0.6, 0.3, 0.6), 1e-9);
            Assert.AreEqual(1, SkateMath27.Ao(0.6, 5, 0.6), 1e-9);
            Assert.AreEqual(90, SkateMath27.HeadingChangeDeg(1, 0, 0, 1), 1e-9);
            Assert.AreEqual(0, SkateMath27.HeadingChangeDeg(0, 0, 0, 1), 1e-9);
            Assert.AreEqual(6, SkateMath27.StripCount(4, 4, 6, 10));
            Assert.AreEqual(10, SkateMath27.StripCount(400, 4, 6, 10));
            Assert.AreEqual(8, SkateMath27.StripCount(32, 4, 6, 10));
        }
    }
}
