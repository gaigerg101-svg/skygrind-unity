// NIGHT20 A2 (builder V): THE HUNT's ring (Game/SpecialFx20.cs): ripples close from ringMax to ringMin around you over
// HuntRingPeriod, out of step, fading as they land; the ring shows from the hunt's announce (in over 0.4 s), holds 4 s past
// its start (the first squads out), fades over 1 s, and is gone at its end.
using NUnit.Framework;
using S = Skygrind.Game.SpecialFxMath;

namespace Skygrind.Game.Tests
{
    public class SpecialFx20Tests
    {
        [Test]
        public void TheRipplesCloseInFromTheBandsEdge()
        {
            Assert.That(S.RippleRadius(0, 0, 12, 20), Is.EqualTo(20).Within(1e-9), "a ripple starts at ringMax ...");
            Assert.That(S.RippleRadius(UnityTuning.HuntRingPeriod * 0.999, 0, 12, 20), Is.EqualTo(12).Within(0.05), "... and closes to ringMin");
            Assert.That(S.RippleRadius(0.3, 0, 12, 20), Is.LessThan(S.RippleRadius(0.1, 0, 12, 20)), "closing in");
            Assert.That(S.RippleRadius(0, 1, 12, 20), Is.Not.EqualTo(S.RippleRadius(0, 0, 12, 20)), "out of step");
            for (double t = 0; t < 3; t += 0.05)
                for (int r = 0; r < UnityTuning.HuntRingRipples; r++)
                {
                    Assert.That(S.RippleRadius(t, r, 12, 20), Is.InRange(12 - 1e-9, 20 + 1e-9));
                    Assert.That(S.RippleAlpha(t, r), Is.InRange(0.0, 1.0));
                }
        }

        [Test]
        public void TheRingShowsFromTheAnnounceThroughTheFirstSquads()
        {
            Assert.That(S.HuntShare(-1, -1, false), Is.EqualTo(0), "no hunt");
            Assert.That(S.HuntShare(0, -1, false), Is.EqualTo(0).Within(1e-12), "the announce: in ...");
            Assert.That(S.HuntShare(0.4, -1, false), Is.EqualTo(1).Within(1e-12), "... over 0.4 s");
            Assert.That(S.HuntShare(5, 2, false), Is.EqualTo(1).Within(1e-12), "held into the round");
            Assert.That(S.HuntShare(9, 4.5, false), Is.EqualTo(0.5).Within(1e-9), "fading 4 s past the start");
            Assert.That(S.HuntShare(9, 6, false), Is.EqualTo(0), "gone");
            Assert.That(S.HuntShare(3, 1, true), Is.EqualTo(0), "the end: gone");
        }
    }
}
