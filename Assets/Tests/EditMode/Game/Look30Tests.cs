// NIGHT30 builder V (item 8, design.n30.look30): THE LOOK's pure rules (Game/Look30Math.cs; Shaders/Resources/Crt29.shader mirrors Quantise and GrainScale).
// The palette test is DESIGN30 E's: after the quantise, cyan, red-orange, magenta and violet-white stay at least 0.15 apart in RGB, at every dither offset.
using NUnit.Framework;
using UnityEngine;
using M = Skygrind.Game.Look30Math;

namespace Skygrind.Game.Tests
{
    public class Look30Tests
    {
        [Test]
        public void ThePaletteStaysApartAfterTheQuantise()
        {
            float gap = M.PaletteMinGap(UnityTuning.Look30Levels, UnityTuning.Look30Strength);
            Assert.That(gap, Is.GreaterThanOrEqualTo(UnityTuning.Look30PaletteMinGap), $"the closest pair after the quantise: {gap}");
            float hard = M.PaletteMinGap(UnityTuning.Look30Levels, 1f); // even a full quantise keeps them apart
            Assert.That(hard, Is.GreaterThanOrEqualTo(UnityTuning.Look30PaletteMinGap), $"full strength: {hard}");
        }

        [Test]
        public void TheQuantiseIsSubtleAndTheDitherSpansUnderOneLevel()
        {
            float step = 1f / (UnityTuning.Look30Levels - 1f);
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                {
                    float d = M.Dither(x, y, UnityTuning.Look30DitherScale);
                    Assert.That(d, Is.GreaterThan(-0.5f).And.LessThan(0.5f));
                }
            Assert.That(M.Dither(0, 0, 2), Is.EqualTo(M.Dither(1, 1, 2)), "a 2 px cell");
            for (float c = 0; c <= 1f; c += 0.037f)
            {
                float q = M.Quantise(c, UnityTuning.Look30Levels, 0.47f, UnityTuning.Look30Strength);
                Assert.That(Mathf.Abs(q - c), Is.LessThanOrEqualTo(step * UnityTuning.Look30Strength + 1e-5f), $"c {c}");
            }
            Assert.That(M.Quantise(0.5f, 32, 0f, 0f), Is.EqualTo(0.5f), "strength 0: the source");
        }

        [Test]
        public void PixelSizeOneIsOffAndTheBlockScalesWithTheScreen()
        {
            Assert.That(M.PixelBlock(1, 1080), Is.EqualTo(0f), "1 = off (D-4)");
            Assert.That(M.PixelBlock(2, 1080), Is.EqualTo(2f));
            Assert.That(M.PixelBlock(4, 1440), Is.EqualTo(5f), "4 px at 1080p is 5 px at 1440p");
            Assert.That(M.PixelBlock(2, 720), Is.EqualTo(2f), "never under 2 px when on");
        }

        [Test]
        public void TheGrainHalvesInTheDark()
        {
            Assert.That(M.GrainScale(0.02f), Is.EqualTo(UnityTuning.Look30GrainDarkScale).Within(1e-6));
            Assert.That(M.GrainScale(UnityTuning.Look30GrainDarkLuma), Is.EqualTo(1f).Within(1e-6));
            Assert.That(M.GrainScale(0.6f), Is.EqualTo(1f));
        }

        [Test]
        public void OneFlickerAtATimeAndAtMostThreeASecond()
        {
            var gate = new FlickerGate30();
            Assert.That(gate.TryStart(0, 10.0), Is.True);
            Assert.That(gate.TryStart(1, 10.01), Is.False, "panel 0 is flickering");
            int frames = 0;
            while (gate.Frame(0) >= 0) frames++;
            Assert.That(frames, Is.EqualTo(UnityTuning.Look30FlickerFrames));
            Assert.That(frames, Is.InRange(2, 4), "DESIGN30 E: 2-4 frames");
            Assert.That(gate.TryStart(1, 10.2), Is.False, "under the gap: at most 3 a second");
            Assert.That(gate.TryStart(1, 10.0 + UnityTuning.Look30FlickerGap + 0.001), Is.True);
            Assert.That(1.0 / UnityTuning.Look30FlickerGap, Is.LessThanOrEqualTo(3.0));
        }

        [Test]
        public void TheBuzzIsASmallJitterAndTheGlowFadesInATenthOfASecondOrTwo()
        {
            for (double t = 0; t < 2; t += 0.013)
                Assert.That(Mathf.Abs(M.BuzzOffset(t)), Is.LessThanOrEqualTo(UnityTuning.Look30BuzzPx));
            Assert.That(M.GlowAlpha(0, 0.1f), Is.EqualTo(UnityTuning.Look30GlowAlpha));
            Assert.That(M.GlowAlpha(UnityTuning.Look30GlowSeconds, 0.1f), Is.EqualTo(0.1f));
            Assert.That(M.FlickerAlpha(5, 3, 0.08f), Is.EqualTo(0.08f));
        }
    }
}
