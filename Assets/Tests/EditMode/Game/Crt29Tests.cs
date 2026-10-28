// NIGHT29 item 4 (builder V): the CRT overlay's pure rules (Game/Crt29Math.cs; the shader mirrors them): the warp pulls the
// corners 2 % and never shows a black border, the chromatic split is 0 in the middle, the scanlines keep the mean
// brightness, the grain is a fixed function (same pixel, same step, same value) that changes with the step, the flicker
// step follows time and freezes with REDUCED MOTION, strength 0 or the overlay off skips the pass.
using NUnit.Framework;
using UnityEngine;
using M = Skygrind.Game.Crt29Math;

namespace Skygrind.Game.Tests
{
    public class Crt29Tests
    {
        [Test]
        public void TheWarpIsTheIdentityAtZeroAndKeepsTheCornersOnTheEdge()
        {
            Assert.That(M.Warp(new Vector2(0.3f, -0.7f), 0), Is.EqualTo(new Vector2(0.3f, -0.7f)));
            Vector2 corner = M.Warp(new Vector2(1, 1), 0.02f);
            Assert.That(corner.x, Is.EqualTo(1f).Within(1e-6), "the corner lands on the edge: no black border");
            Vector2 mid = M.Warp(new Vector2(1, 0), 0.02f);
            Assert.That(mid.x, Is.LessThan(1f), "an edge's middle samples inside the image");
            Assert.That(M.Warp(Vector2.zero, 0.02f), Is.EqualTo(Vector2.zero));
            Vector2 centre = M.Warp(new Vector2(0.1f, 0), 0.02f);
            Assert.That(centre.x, Is.EqualTo(0.1f / 1.02f).Within(1e-3), "the middle is zoomed by 1 / (1 + k): the 2 % pull");
        }

        [Test]
        public void TheChromaSplitIsZeroInTheMiddleAndWholeAtTheCorner()
        {
            Assert.That(M.ChromaOffset(Vector2.zero, 0.0015f), Is.EqualTo(Vector2.zero));
            Vector2 o = M.ChromaOffset(new Vector2(1, 1), 0.0015f);
            Assert.That(o.magnitude, Is.EqualTo(0.0015f * 1.4142135f).Within(1e-5));
            Assert.That(M.ChromaOffset(new Vector2(0.2f, 0), 0.0015f).magnitude, Is.LessThan(1e-4f));
        }

        [Test]
        public void TheScanlinesKeepTheMeanBrightness()
        {
            double sum = 0;
            const int n = 720 * 8;
            for (int i = 0; i < n; i++) sum += M.Scan((i + 0.5f) / n, 720, 0.12f);
            Assert.That(sum / n, Is.EqualTo(1.0).Within(0.01), "brightness compensated: the lines darken the half, the whole stays level");
            Assert.That(M.Scan(0.25f / 720, 720, 0.12f), Is.GreaterThan(M.Scan(0.75f / 720, 720, 0.12f)), "a line has a bright half and a dark one");
            Assert.That(M.Scan(0.3f, 720, 0), Is.EqualTo(1f).Within(1e-6), "intensity 0: no lines");
        }

        [Test]
        public void TheGrainIsAFixedFunctionOfPixelAndStep()
        {
            Assert.That(M.Noise(10, 20, 3), Is.EqualTo(M.Noise(10, 20, 3)));
            Assert.That(M.Noise(10, 20, 3), Is.Not.EqualTo(M.Noise(10, 20, 4)), "the next step flickers");
            Assert.That(M.Noise(10, 20, 3), Is.Not.EqualTo(M.Noise(11, 20, 3)), "neighbours differ");
            double sum = 0;
            for (uint i = 0; i < 4096; i++)
            {
                float v = M.Noise(i % 64, i / 64, 7);
                Assert.That(v, Is.InRange(0f, 1f));
                sum += v;
            }
            Assert.That(sum / 4096, Is.EqualTo(0.5).Within(0.03), "the grain is centred: no net brighten or darken");
        }

        [Test]
        public void TheFlickerStepFollowsTimeAndFreezesWithReducedMotion()
        {
            Assert.That(M.GrainStep(1.0, 24, false), Is.EqualTo(24u));
            Assert.That(M.GrainStep(1.0 + 1.0 / 24, 24, false), Is.EqualTo(25u));
            Assert.That(M.GrainStep(1.0, 24, true), Is.EqualTo(0u), "REDUCED MOTION: steady grain");
            Assert.That(M.GrainStep(1.0, 0, false), Is.EqualTo(0u));
            Assert.That(M.GrainStep(1e6, 24, false), Is.LessThan(4096u), "wrapped");
        }

        [Test]
        public void TheVignetteIsClearInsideAndFullAtTheCorner()
        {
            Assert.That(M.Vignette(0, 0.25f), Is.EqualTo(1f));
            Assert.That(M.Vignette(0.4f, 0.25f), Is.EqualTo(1f).Within(1e-6));
            Assert.That(M.Vignette(1.414f, 0.25f), Is.EqualTo(0.75f).Within(1e-3));
        }

        [Test]
        public void StrengthZeroOrTheOverlayOffSkipsThePass()
        {
            Assert.That(M.Strength(0.35, true), Is.EqualTo(0.35f).Within(1e-6));
            Assert.That(M.Strength(0.35, false), Is.EqualTo(0f), "the accessibility switch: off whatever the strength");
            Assert.That(M.Strength(0, true), Is.EqualTo(0f));
            Assert.That(M.Strength(5, true), Is.EqualTo(1f), "clamped");
            Assert.That(M.PixelBlock(0, 1440), Is.EqualTo(0f), "the pixel step is off by default");
            Assert.That(M.PixelBlock(2, 720), Is.EqualTo(1f));
        }
    }
}
