// NIGHT38 builder H (design.n38.juice38): ULTRAKILL's shake rule, the hurt burst's level, the flicker's pulses (Juice38Math, Juice38.cs). Pure.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Juice38Tests
    {
        [Test]
        public void Shake_TheBiggerReplaces_NeverAdds()
        {
            Assert.That(Juice38Math.Replace(1.0, 0.5), Is.EqualTo(1.0));
            Assert.That(Juice38Math.Replace(1.0, 2.5), Is.EqualTo(2.5));
            Assert.That(Juice38Math.Replace(0, 0), Is.EqualTo(0));
        }

        [Test]
        public void Shake_DecaysLinearly_AndMovesAtMostOneUnit()
        {
            double s = 2.5;
            for (int i = 0; i < 60; i++) s = Juice38Math.Decay(s, 3, 1.0 / 60);
            Assert.That(s, Is.EqualTo(0.0).Within(1e-9), "2.5 at 3 / s is gone after 0.83 s; after 1 s it is 0, never negative");
            Assert.That(Juice38Math.Decay(2.5, 3, 0.5), Is.EqualTo(1.0).Within(1e-9));
            Assert.That(Juice38Math.Amplitude(2.5), Is.EqualTo(1.0), "above 1 a shake only lasts longer");
            Assert.That(Juice38Math.Amplitude(0.4), Is.EqualTo(0.4));
        }

        [Test]
        public void Hurt_LevelAndShake_ScaleWithTheHit()
        {
            Assert.That(Juice38Math.HurtLevel(10, 0.5, 0.8, 50), Is.EqualTo(0.56).Within(1e-9));
            Assert.That(Juice38Math.HurtLevel(80, 0.5, 0.8, 50), Is.EqualTo(0.8).Within(1e-9));
            Assert.That(Juice38Math.HurtLevel(0, 0.5, 0.8, 50), Is.EqualTo(0));
            Assert.That(Juice38Math.HurtShake(5, 20, 0.15, 0.6), Is.EqualTo(0.25).Within(1e-9));
            Assert.That(Juice38Math.HurtShake(1, 20, 0.15, 0.6), Is.EqualTo(0.15).Within(1e-9), "every hit shakes a little");
            Assert.That(Juice38Math.HurtShake(40, 20, 0.15, 0.6), Is.EqualTo(0.6).Within(1e-9), "a small shake: capped");
        }

        [Test]
        public void Flicker_PulsesAreFewAndEnd()
        {
            Assert.That(Juice38Math.Pulse(0, 0.3, 2), Is.EqualTo(1.0).Within(1e-9));
            Assert.That(Juice38Math.Pulse(0.15, 0.3, 2), Is.EqualTo(1.0).Within(1e-9), "the second pulse starts at half the flicker");
            Assert.That(Juice38Math.Pulse(0.149, 0.3, 2), Is.LessThan(0.01));
            Assert.That(Juice38Math.Pulse(0.3, 0.3, 2), Is.EqualTo(0));
            Assert.That(Juice38Math.Envelope(0.15, 0.3), Is.EqualTo(0.5).Within(1e-9));
            Assert.That(UnityTuning.Juice38EntrancePulses, Is.LessThanOrEqualTo(3), "never more than three flashes in a flicker (FlashGate's second)");
        }

        [Test]
        public void MoveSpec_StageFilter()
        {
            Assert.That(Juice38Math.StageMatches("dive", "dive"), Is.True);
            Assert.That(Juice38Math.StageMatches("dive", "tell"), Is.False);
            Assert.That(Juice38Math.StageMatches(null, "parried"), Is.True);
        }
    }
}
