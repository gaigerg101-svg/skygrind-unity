// NIGHT18 U3-31.4 round 1 fix (builder R, tests first and seen red): Game/HitSpark.cs's rules. The impact at the hit
// point: 3-6 chips by the hit's size, thrown up and off the struck side in the screen plane, never toward the camera; a
// flare full on the hit's own frame and gone inside 100 ms; a chip full-size, then shrinking to nothing at its life.
using System;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class HitSparkTests
    {
        static readonly Vector3 Right = new Vector3(1, 0, 0), Up = new Vector3(0, 1, 0), Away = new Vector3(0, 0, 1);

        [Test]
        public void TheChipCountFollowsTheHitsSize()
        {
            Assert.That(HitSparkMath.ChipCount(1), Is.EqualTo(4), "a revolver body hit");
            Assert.That(HitSparkMath.ChipCount(UnityTuning.HitReactMinScale), Is.EqualTo(UnityTuning.HitSparkChipsMin), "a nailgun's");
            Assert.That(HitSparkMath.ChipCount(UnityTuning.HitReactHeavy), Is.EqualTo(UnityTuning.HitSparkChipsMax), "a heavy gun's");
            for (float s = 0; s <= 10; s += 0.25f)
                Assert.That(HitSparkMath.ChipCount(s), Is.InRange(UnityTuning.HitSparkChipsMin, UnityTuning.HitSparkChipsMax), $"scale {s}");
        }

        [Test]
        public void ChipsRiseInTheScreenPlaneAndNeverComeAtTheCamera()
        {
            for (int n = UnityTuning.HitSparkChipsMin; n <= UnityTuning.HitSparkChipsMax; n++)
                for (int side = -1; side <= 1; side++)
                    for (int j = 0; j <= 2; j++)
                        for (int k = 0; k < n; k++)
                        {
                            Vector3 d = HitSparkMath.ChipDir(HitSparkMath.ChipAngleDeg(k, n, side, j * 0.5f), Right, Up, Away);
                            string at = $"n {n} side {side} jitter {j * 0.5f} chip {k}";
                            Assert.That(d.magnitude, Is.EqualTo(1f).Within(1e-4f), at);
                            Assert.That(d.z, Is.GreaterThanOrEqualTo(0), "never toward the camera: " + at);
                            Assert.That((float)Math.Sqrt(d.x * d.x + d.y * d.y), Is.GreaterThan(0.85f), "mostly across the screen: " + at);
                            Assert.That(d.y, Is.GreaterThan(0), "upward: " + at);
                        }
        }

        [Test]
        public void ChipsComeOffTheStruckSide()
        {
            for (int side = -1; side <= 1; side++)
            {
                float sum = 0;
                const int n = 4;
                for (int k = 0; k < n; k++) sum += HitSparkMath.ChipDir(HitSparkMath.ChipAngleDeg(k, n, side, 0.5f), Right, Up, Away).x;
                float mean = sum / n;
                if (side == 0) Assert.That(Math.Abs(mean), Is.LessThan(0.05f), "a centred hit: a symmetric fan");
                else Assert.That(mean * side, Is.GreaterThan(0.2f), $"struck on side {side}: the fan leans that way");
            }
        }

        [Test]
        public void TheFlareIsFullOnTheHitsFrameAndGoneInside100Ms()
        {
            Assert.That(HitSparkMath.FlareShare(0), Is.EqualTo(1f));
            Assert.That(HitSparkMath.FlareShare(1f / 60), Is.EqualTo(1f), "full on the hit's own frame");
            Assert.That(HitSparkMath.FlareShare(0.1f), Is.EqualTo(0f), "gone by 100 ms");
            float last = 1;
            for (float t = 0; t < 0.12f; t += 0.002f)
            {
                float s = HitSparkMath.FlareShare(t);
                Assert.That(s, Is.LessThanOrEqualTo(last + 1e-6f), $"never grows back (t {t})");
                Assert.That(s, Is.InRange(0f, 1f));
                last = s;
            }
        }

        [Test]
        public void AChipIsFullSizeThenShrinksToNothingAtItsLife()
        {
            float life = UnityTuning.HitSparkChipLife;
            Assert.That(HitSparkMath.ChipShare(0, life), Is.EqualTo(1f));
            Assert.That(HitSparkMath.ChipShare(life * 0.5f, life), Is.EqualTo(1f), "hard-edged and full for most of its life");
            Assert.That(HitSparkMath.ChipShare(life, life), Is.EqualTo(0f));
            Assert.That(HitSparkMath.ChipShare(life * 0.8f, life), Is.InRange(0.01f, 0.99f), "shrinking near the end");
            Assert.That(UnityTuning.HitSparkChipLife - UnityTuning.HitSparkChipLifeJitter, Is.GreaterThanOrEqualTo(0.15f - 1e-6f));
            Assert.That(UnityTuning.HitSparkChipLife + UnityTuning.HitSparkChipLifeJitter, Is.LessThanOrEqualTo(0.25f + 1e-6f));
        }
    }
}
