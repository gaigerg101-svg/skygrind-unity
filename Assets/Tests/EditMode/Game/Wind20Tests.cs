// NIGHT20 row 6 beyond (builder V): the long slide's wind wisps (Game/MovementFx.Wind20.cs), ported from
// dip000/my-godotshaders WindTrailWithLoops (MIT): the straight wave for wave_count periods, then a blend toward the loop
// over one period and back over the next (the factor 0 -> 0.5 -> 0); a wisp's point along its length: the wave's along
// coordinate grows with the length, its side and up wiggle stay within the amplitudes while straight; the fade in and out.
using NUnit.Framework;
using UnityEngine;
using W = Skygrind.Game.WindTrailMath;

namespace Skygrind.Game.Tests
{
    public class Wind20Tests
    {
        const double Tau = System.Math.PI * 2;

        [Test]
        public void TheWaveHoldsThenBlendsTowardTheLoopAndBack()
        {
            // the shader: offset_time = period / 2 (so t = scaled + pi); hold = wave_count x period
            double hold = UnityTuning.WindWaveCount * Tau;
            Assert.That(W.Factor(0), Is.EqualTo(0), "t = pi < hold: the straight wave");
            Assert.That(W.Factor(hold + Tau - Tau * 0.5), Is.EqualTo(0.5).Within(1e-9), "a period into the blend: half loop");
            Assert.That(W.Factor(hold + Tau * 0.5 - Tau * 0.5), Is.EqualTo(0.25).Within(1e-9), "half way in");
            Assert.That(W.Factor(hold + 2 * Tau - Tau * 0.5 - 1e-9), Is.EqualTo(0).Within(1e-6), "back to the wave");
            Assert.That(W.Factor(hold + 2 * Tau + 0.1 - Tau * 0.5), Is.EqualTo(0), "and the cycle restarts");
        }

        [Test]
        public void AWispRunsItsLengthAndWigglesWithinItsAmplitudes()
        {
            for (double y = 0; y <= 1; y += 0.1)
            {
                Vector3 p = W.Point(y, 0); // in the hold: the straight wave
                Assert.That(p.y, Is.EqualTo(y * UnityTuning.WindLength).Within(1e-5), "along: the length x y");
                Assert.That(Mathf.Abs(p.x), Is.LessThanOrEqualTo((float)UnityTuning.WindTwistAmp + 1e-6f), "side: the twist");
                Assert.That(Mathf.Abs(p.z), Is.LessThanOrEqualTo((float)UnityTuning.WindWaveAmp + 1e-6f), "up: the wave");
            }
            Vector3 a = W.Point(0.5, 0), b = W.Point(0.5, 0.3);
            Assert.That((a - b).magnitude, Is.GreaterThan(1e-3f), "it moves with time");
        }

        [Test]
        public void TheWispsFadeInAndOut()
        {
            Assert.That(W.Fade(0, 10), Is.EqualTo(0));
            Assert.That(W.Fade(UnityTuning.WindFade, 10), Is.EqualTo(1).Within(1e-12));
            Assert.That(W.Fade(1, 0), Is.EqualTo(0), "the slide's end: out");
            Assert.That(W.Fade(1, UnityTuning.WindFade * 0.5), Is.EqualTo(0.5).Within(1e-9));
        }
    }
}
