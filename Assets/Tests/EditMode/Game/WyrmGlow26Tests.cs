// NIGHT26 builder V: the KILN WYRM's glow curves (Game/WyrmGlow26.cs; numbers UnityTuning.Wyrm26Fx.cs). Pure functions: no scene, no sim.
// Unrun by the author (no Editor): written to be run by the lead (Tools/unity-loop.sh tests WyrmGlow26).
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class WyrmGlow26Tests
    {
        const int N = 26;

        static WyrmFxState26 Calm() => new WyrmFxState26
        {
            pieces = N, releaseAge = WyrmGlow26.None, impactAge = WyrmGlow26.None, trailAge = WyrmGlow26.None, hpFrac = 1f, deathAge = -1f, tailFrom = N - 4,
        };

        [Test]
        public void IdleStaysInItsBand_AndTheWaveRunsDownTheChain()
        {
            float lo = 9f, hi = -9f;
            for (int i = 0; i < N; i++)
                for (float ph = 0; ph < 2f; ph += 0.02f)
                {
                    float g = WyrmGlow26.Idle(i, ph);
                    lo = Mathf.Min(lo, g);
                    hi = Mathf.Max(hi, g);
                }
            Assert.GreaterOrEqual(lo, UnityTuning.WyrmFxIdleMin - 1e-4f);
            Assert.LessOrEqual(hi, UnityTuning.WyrmFxIdleMax + 1e-4f);
            // the crest (phase of a piece's maximum) comes later for later pieces: one piece after another, head to tail
            float crest0 = Crest(0), crest5 = Crest(5);
            Assert.Greater(Mathf.Abs(crest0 - crest5), 1e-3f);
        }

        // the phase in [0,1) at which piece i is brightest, searching one period
        static float Crest(int i)
        {
            float best = -1, at = 0;
            for (float ph = 0; ph < 1f; ph += 0.001f)
            {
                float g = WyrmGlow26.Idle(i, ph);
                if (g > best) { best = g; at = ph; }
            }
            return at;
        }

        [Test]
        public void CoilRate_RisesToTheNearRate_AndTheCoilIsBrighterThanTheIdle()
        {
            Assert.AreEqual(UnityTuning.WyrmFxCoilRateFar, WyrmGlow26.CoilRate(0f), 1e-4f);
            Assert.AreEqual(UnityTuning.WyrmFxCoilRateNear, WyrmGlow26.CoilRate(1f), 1e-4f);
            float prev = 0f;
            for (float a = 0; a <= 1.001f; a += 0.1f)
            {
                float r = WyrmGlow26.CoilRate(a);
                Assert.GreaterOrEqual(r, prev);
                prev = r;
            }
            float coilMax = 0, idleMax = 0;
            for (float ph = 0; ph < 1f; ph += 0.01f)
            {
                coilMax = Mathf.Max(coilMax, WyrmGlow26.Coil(0, ph, 0f));
                idleMax = Mathf.Max(idleMax, WyrmGlow26.Idle(0, ph));
            }
            Assert.Greater(coilMax, idleMax * 0.7f);
            float peak = 0;
            for (float ph = 0; ph < 1f; ph += 0.01f) peak = Mathf.Max(peak, WyrmGlow26.Coil(0, ph, 1f));
            Assert.AreEqual(UnityTuning.WyrmFxCoilPeak, peak, 0.02f);
        }

        [Test]
        public void Approach_IsZeroFarAndOneAtTheRelease()
        {
            Assert.AreEqual(0f, WyrmGlow26.Approach(UnityTuning.WyrmFxCoilApproachSec + 1f), 1e-6f);
            Assert.AreEqual(1f, WyrmGlow26.Approach(0f), 1e-6f);
            Assert.Greater(WyrmGlow26.Approach(1f), WyrmGlow26.Approach(2f));
        }

        [Test]
        public void Release_PeaksAtTheReleaseAndDecaysToZero()
        {
            float g0 = WyrmGlow26.Release(0, N, 0f), g1 = WyrmGlow26.Release(0, N, UnityTuning.WyrmFxReleaseSec * 0.5f);
            Assert.Greater(g0, g1);
            Assert.AreEqual(0f, WyrmGlow26.Release(0, N, UnityTuning.WyrmFxReleaseSec), 1e-6f);
            Assert.AreEqual(0f, WyrmGlow26.Release(3, N, WyrmGlow26.None), 1e-6f);
            Assert.Greater(WyrmGlow26.Release(2, N, 0f), WyrmGlow26.Release(N - 1, N, 0f));
        }

        [Test]
        public void TheAttacksTelegraphDifferently_AndRiseWithTheirRamp()
        {
            var kinds = new[] { WyrmKind26.Dive, WyrmKind26.Lunge, WyrmKind26.Spit, WyrmKind26.Roar, WyrmKind26.Squeeze, WyrmKind26.Shed };
            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (WyrmKind26 k in kinds)
            {
                WyrmGlow26.Telegraph(k, 0, N, N - 4, 0.2f, 0.25f, out float g02, out float j02);
                WyrmGlow26.Telegraph(k, 0, N, N - 4, 1f, 0.25f, out float g1, out float j1);
                Assert.Greater(g1 + j1, g02 + j02, k + " rises with its ramp");
                seen.Add($"{UnityTuning.WyrmFxTellHue[(int)k]:0.00}|{UnityTuning.WyrmFxTellRate[(int)k]:0.0}|{UnityTuning.WyrmFxTellJaw[(int)k]:0.00}");
            }
            Assert.AreEqual(kinds.Length, seen.Count, "every tell has its own hue / strobe / jaw profile");
            WyrmGlow26.Telegraph(WyrmKind26.Dive, 0, N, N - 4, 0f, 0f, out float g0, out float j0);
            Assert.AreEqual(0f, g0 + j0, 1e-6f);
        }

        [Test]
        public void TheSpitLightsItsPorts_AndTheShedLightsTheTail()
        {
            WyrmGlow26.Telegraph(WyrmKind26.Spit, 3, N, N - 4, 1f, 0.25f, out float port, out _);
            WyrmGlow26.Telegraph(WyrmKind26.Spit, 4, N, N - 4, 1f, 0.25f, out float between, out _);
            Assert.Greater(port, between);
            WyrmGlow26.Telegraph(WyrmKind26.Shed, N - 2, N, N - 4, 1f, 0.25f, out float tail, out _);
            WyrmGlow26.Telegraph(WyrmKind26.Shed, 8, N, N - 4, 1f, 0.25f, out float mid, out _);
            Assert.Greater(tail, mid);
        }

        [Test]
        public void TheTrailFrontRunsHeadToTail()
        {
            int hotAt0 = Front(0.05f), hotAt1 = Front(0.4f);
            Assert.Less(hotAt0, hotAt1);
            Assert.AreEqual(0f, WyrmGlow26.Trail(5, N, WyrmGlow26.None, false), 1e-6f);
            Assert.Greater(WyrmGlow26.Trail(0, N, WyrmGlow26.None, true), WyrmGlow26.Trail(N - 1, N, WyrmGlow26.None, true));
        }

        static int Front(float age)
        {
            int best = 0;
            float v = -1;
            for (int i = 0; i < N; i++)
            {
                float g = WyrmGlow26.Trail(i, N, age, false);
                if (g > v) { v = g; best = i; }
            }
            return best;
        }

        [Test]
        public void TheImpactFlashesTheHeadMost()
        {
            Assert.Greater(WyrmGlow26.Impact(0, 0f), WyrmGlow26.Impact(1, 0f));
            Assert.AreEqual(0f, WyrmGlow26.Impact(5, 0f), 1e-6f);
            Assert.AreEqual(0f, WyrmGlow26.Impact(0, UnityTuning.WyrmFxImpactSec), 1e-6f);
        }

        [Test]
        public void TheDamageFlashIsWhiteHotThenGone()
        {
            Assert.AreEqual(1f, WyrmGlow26.Hot(0f), 1e-6f);
            Assert.AreEqual(0f, WyrmGlow26.Hot(UnityTuning.WyrmFxHotSec), 1e-6f);
            Assert.AreEqual(0f, WyrmGlow26.Hot(-1f), 1e-6f);
            WyrmFxState26 s = Calm();
            Assert.AreEqual(1f, WyrmGlow26.Evaluate(s, 7, 1f).hot, 1e-6f);
            Assert.AreEqual(0f, WyrmGlow26.Evaluate(s, 7, 0f).hot, 1e-6f);
        }

        [Test]
        public void TheGlowDimsAsTheHealthFalls()
        {
            WyrmFxState26 full = Calm(), low = Calm();
            low.hpFrac = 0.1f;
            Assert.Greater(WyrmGlow26.Evaluate(full, 3, 0f).glow, WyrmGlow26.Evaluate(low, 3, 0f).glow);
            Assert.AreEqual(UnityTuning.WyrmFxHpFloor, WyrmGlow26.HpDim(0f), 1e-6f);
            Assert.AreEqual(1f, WyrmGlow26.HpDim(1f), 1e-6f);
        }

        [Test]
        public void DeathFlaresThenBurnsOutHeadToTail()
        {
            WyrmFxState26 s = Calm();
            // the flare: every piece is brighter than alive, none burnt yet
            s.deathAge = UnityTuning.WyrmFxFlareSec;
            for (int i = 0; i < N; i++)
            {
                Assert.Greater(WyrmGlow26.Evaluate(s, i, 0f).glow, WyrmGlow26.Evaluate(Calm(), i, 0f).glow, "piece " + i + " flares");
                Assert.AreEqual(0f, WyrmGlow26.Evaluate(s, i, 0f).burn, 1e-6f);
            }
            // the burn-out times grow from the head to the tail, and at its time a piece is fully burnt with no glow
            for (int i = 1; i < N; i++) Assert.Greater(WyrmGlow26.BurnOutTime(i), WyrmGlow26.BurnOutTime(i - 1));
            s.deathAge = WyrmGlow26.BurnOutTime(0) + 1e-3f;
            Assert.AreEqual(1f, WyrmGlow26.Evaluate(s, 0, 0f).burn, 1e-3f);
            Assert.AreEqual(0f, WyrmGlow26.Evaluate(s, 0, 0f).glow, 1e-3f);
            Assert.Less(WyrmGlow26.Evaluate(s, N - 1, 0f).burn, 1f, "the tail still burns when the head is out");
            s.deathAge = WyrmGlow26.DeathTotal(N) + 1e-3f;
            for (int i = 0; i < N; i++) Assert.AreEqual(1f, WyrmGlow26.Evaluate(s, i, 0f).burn, 1e-3f);
        }

        [Test]
        public void EveryHueStaysInTheDangerFamily()
        {
            // the hue is 0 (red-orange) .. 1 (amber): the shader lerps between two red-orange colours, never to cyan or magenta
            Color glow = Look.Hex(UnityTuning.WyrmFxGlowHex), amber = Look.Hex(UnityTuning.WyrmFxAmberHex);
            foreach (Color c in new[] { glow, amber })
            {
                Assert.Greater(c.r, c.g);
                Assert.Greater(c.g, c.b);
            }
            foreach (float h in UnityTuning.WyrmFxTellHue) Assert.That(h, Is.InRange(0f, 1f));
        }
    }
}
