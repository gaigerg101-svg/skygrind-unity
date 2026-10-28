// NIGHT33 round 3 (builder K): the clear-band cut of a kit box (Game/Kit33Props.cs Kit33Cut.Parts). n33-r4 overflowed the stack at Init: S cut a
// box around a band, recursed on each part, and a part whose edge landed on the band by float rounding was cut again forever. The cut is now one
// bounded pass; these tests pin the edge case and the property that ended the loop (a part fed back through the cut comes out unchanged).
using System.Collections.Generic;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Kit33CutTests
    {
        static readonly float[] Ride = { 1.34f, 1.86f };

        [Test]
        public void ABoxWhoseEdgeLiesExactlyOnTheBandIsLeftWhole()
        {
            List<(float lo, float hi)> below = Kit33Cut.Parts(0f, 1.34f, Ride, 0.02f);
            Assert.That(below.Count, Is.EqualTo(1));
            Assert.That(below[0].lo, Is.EqualTo(0f));
            Assert.That(below[0].hi, Is.EqualTo(1.34f));
            List<(float lo, float hi)> above = Kit33Cut.Parts(1.86f, 9.6f, Ride, 0.02f);
            Assert.That(above.Count, Is.EqualTo(1));
            Assert.That(above[0].lo, Is.EqualTo(1.86f));
            // within the touch tolerance on the wrong side (float rounding) it is still whole
            Assert.That(Kit33Cut.Parts(0f, 1.34f + UnityTuning.Kit33CutTouch * 0.5f, Ride, 0.02f).Count, Is.EqualTo(1));
            Assert.That(Kit33Cut.Parts(1.86f - UnityTuning.Kit33CutTouch * 0.5f, 5f, Ride, 0.02f).Count, Is.EqualTo(1));
        }

        [Test]
        public void ABoxAcrossTheBandBecomesTheTwoPartsOutsideIt()
        {
            List<(float lo, float hi)> p = Kit33Cut.Parts(0f, 9.6f, Ride, 0.02f);
            Assert.That(p.Count, Is.EqualTo(2));
            Assert.That(p[0], Is.EqualTo((0f, 1.34f)));
            Assert.That(p[1], Is.EqualTo((1.86f, 9.6f)));
            Assert.That(Kit33Cut.Parts(1.4f, 1.8f, Ride, 0.02f).Count, Is.EqualTo(0), "inside the band: nothing drawn");
            Assert.That(Kit33Cut.Parts(1.33f, 1.9f, Ride, 0.02f).Count, Is.EqualTo(1), "a 1 cm sliver under the band is dropped, the 4 cm part above kept");
            Assert.That(Kit33Cut.Parts(0f, 5f, new[] { 3f, 2f }, 0.02f).Count, Is.EqualTo(1), "a pair with hi <= lo is ignored");
            Assert.That(Kit33Cut.Parts(0f, 5f, null, 0.02f).Count, Is.EqualTo(1));
        }

        [Test]
        public void ManyBandsGiveAtMostBandsPlusOneParts()
        {
            var bands = new List<float>();
            for (int i = 0; i < 12; i++) { bands.Add(0.5f + i * 0.8f); bands.Add(0.7f + i * 0.8f); }
            List<(float lo, float hi)> p = Kit33Cut.Parts(0f, 10f, bands.ToArray(), 0.02f);
            Assert.That(p.Count, Is.LessThanOrEqualTo(bands.Count / 2 + 1));
            Assert.That(p.Count, Is.EqualTo(13));
            for (int i = 1; i < p.Count; i++) Assert.That(p[i].lo, Is.GreaterThanOrEqualTo(p[i - 1].hi), "bottom to top, no overlap");
        }

        [Test]
        public void APartRebuiltFromItsCentreAndHalfHeightIsNeverCutAgain()
        {
            // S rebuilds each part as a box (centre = row y + local offset, half height) in float: its edges come back off by rounding. Fed back through
            // the cut with the same bands, every part must come out as ONE part (the recursion that overflowed would have cut it again).
            var rng = new System.Random(33);
            float[] bands = { 1.34f, 1.86f, 0.25f, 0.55f, 2.45f, 2.75f };
            for (int n = 0; n < 5000; n++)
            {
                float rowY = (float)(rng.NextDouble() * 20 - 4), b0 = rowY + (float)(rng.NextDouble() * 3 - 1.5), b1 = b0 + (float)(rng.NextDouble() * 10 + 0.05);
                foreach (var (lo, hi) in Kit33Cut.Parts(b0, b1, bands, 0.02f))
                {
                    float local = (lo + hi) * 0.5f - rowY, half = (hi - lo) * 0.5f, cy = rowY + local;
                    List<(float lo, float hi)> again = Kit33Cut.Parts(cy - half, cy + half, bands, 0.02f);
                    Assert.That(again.Count, Is.EqualTo(1), $"part [{lo}, {hi}] of [{b0}, {b1}] was cut again");
                }
            }
        }
    }
}
