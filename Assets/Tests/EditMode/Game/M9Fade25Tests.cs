// NIGHT25 (builder F, T-12): the fade-ins that remove M9's first-frame-at-full-alpha entries (style meter block, kill marker, the lesson line): each is at most
// 0.12 s (UiDeadline22's rule) and a first frame at one 60 fps frame's dt is under M9's POP_ALPHA 0.95. The elements themselves are read by the Player reel + m9-list.py.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class M9Fade25Tests
    {
        const double Frame = 1.0 / 60, PopAlpha = 0.95;

        [TestCase(0.1, "style meter block")]
        [TestCase(0.04, "kill marker")]
        [TestCase(0.2, "lesson line in")]
        public void FirstFrame_IsUnderPopAlpha(double fade, string what)
        {
            Assert.That(Frame / fade, Is.LessThan(PopAlpha), what);
        }

        [Test]
        public void TheShortFades_AreAtMostTheDeadlineBars()
        {
            Assert.That(UnityTuning.StyleMeterFadeIn25, Is.LessThanOrEqualTo(0.12));
            Assert.That(UnityTuning.KillMarkerFadeIn25, Is.LessThanOrEqualTo(0.12));
            Assert.That(UnityTuning.Lesson25FadeIn, Is.LessThanOrEqualTo(0.3));
        }
    }
}
