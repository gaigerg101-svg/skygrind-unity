// NIGHT42 builder U1 (design.n42.style42): the style meter's motion rules (Style42Math, UiHud33.Style42.cs). Pure: no scene, no sim.
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Style42Tests
    {
        [Test]
        public void Punch_StartsBig_Overshoots_Rests()
        {
            Assert.That(Style42Math.LetterScale(0, true), Is.EqualTo(UnityTuning.Style42PunchScale).Within(1e-4));
            float min = 9f;
            for (int i = 1; i < 40; i++) min = Mathf.Min(min, Style42Math.LetterScale(UnityTuning.Style42PunchTime * i / 40.0, true));
            Assert.That(min, Is.LessThan(1f), "an overshoot under rest before it settles");
            Assert.That(Style42Math.LetterScale(UnityTuning.Style42PunchTime, true), Is.EqualTo(1f));
            double peak = UnityTuning.Style42PunchTime * Style42Math.PunchPeakK;
            Assert.That(Style42Math.PunchPeakK, Is.EqualTo(0.58).Within(0.01));
            Assert.That(Style42Math.LetterScale(peak + 1e-4, true), Is.EqualTo(1f), "past the overshoot it snaps to rest");
            Assert.That(Style42Math.Punching(peak * 0.5, true) && !Style42Math.Punching(peak + 1e-4, true) && !Style42Math.Punching(0, false), "the shadow twin hides only through the punch");
            Assert.That(Style42Math.LetterScale(0, false), Is.EqualTo(UnityTuning.Style42DropScale).Within(1e-4));
            Assert.That(Style42Math.LetterScale(double.MaxValue, false), Is.EqualTo(1f));
        }

        [Test]
        public void Shake_Decays_ToZero()
        {
            Assert.That(Style42Math.Shake(UnityTuning.Style42ShakeTime, 8f), Is.EqualTo(Vector2.zero));
            Assert.That(Style42Math.Shake(double.MaxValue, 8f), Is.EqualTo(Vector2.zero));
            float early = 0f, late = 0f;
            for (int i = 0; i < 10; i++)
            {
                early = Mathf.Max(early, Style42Math.Shake(0.01 * i, 8f).magnitude);
                late = Mathf.Max(late, Style42Math.Shake(UnityTuning.Style42ShakeTime * 0.8 + 0.005 * i, 8f).magnitude);
            }
            Assert.That(early, Is.GreaterThan(2f));
            Assert.That(late, Is.LessThan(early * 0.2f));
        }

        [Test]
        public void Combo_Chains_EndsOnHitOrEmpty()
        {
            int c = 0;
            for (int i = 0; i < 5; i++) c = Style42Math.Combo(c, true, false, false);
            Assert.That(c, Is.EqualTo(5));
            Assert.That(Style42Math.Combo(c, false, false, false), Is.EqualTo(5), "nothing happened: the chain holds");
            Assert.That(Style42Math.Combo(c, false, true, false), Is.EqualTo(0), "a hit taken ends it");
            Assert.That(Style42Math.Combo(c, false, false, true), Is.EqualTo(0), "the meter at 0 ends it");
        }

        [Test]
        public void Decay_DimsTheLetter_LineEases()
        {
            Assert.That(Style42Math.DecayAlpha(1f), Is.EqualTo(1f));
            Assert.That(Style42Math.DecayAlpha(0f), Is.EqualTo(UnityTuning.Style42DecayDim).Within(1e-5));
            Assert.That(Style42Math.EaseLine(-1f, 0.4f, 1 / 60.0), Is.EqualTo(0.4f), "first frame: no ease from nothing");
            float v = Style42Math.EaseLine(0f, 1f, 1 / 60.0);
            Assert.That(v, Is.GreaterThan(0.2f).And.LessThan(0.5f));
            for (int i = 0; i < 60; i++) v = Style42Math.EaseLine(v, 1f, 1 / 60.0);
            Assert.That(v, Is.EqualTo(1f), "it lands on the meter within a second");
        }
    }
}
