// NIGHT72 builder L (L3; Game/Lit72.cs, Crt29.shader's Lit72 block, Tools/night/n72-lit-sim.py). Pinned:
//   1. the gain: 1 from l1 up (a lit pixel is untouched), (l + A) / l under l0, half the weight halfway, capped at kmax, the same numbers as the sim
//   2. a lifted colour keeps its hue: the three channels are scaled by one gain (their ratios hold), and a channel clamps at 1
//   3. a gain of 1 leaves a value over 1 alone (an HDR highlight is not clipped by the pass)
//   4. the weight's target: a boss room, level 5's theme (spire = 4) or a live boss grade asks for it; nothing else does
//   5. the fade steps linearly in and out and never overshoots
//   6. -skygrindLit72Tune: a prefix of A, l0, l1, kmax replaces UnityTuning's numbers, the rest stay; a bad number is skipped; l1 stays above l0
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Lit72Tests
    {
        const float A = 0.12f, L0 = 0.12f, L1 = 0.42f, K = 4.5f;

        [Test]
        public void Gain_MatchesTheSim()
        {
            Assert.AreEqual(1f, Lit72Math.Gain(0.5f, A, L0, L1, K), 1e-5f, "a lit pixel is untouched");
            Assert.AreEqual(1f, Lit72Math.Gain(L1, A, L0, L1, K), 1e-5f, "at l1 the weight is 0");
            Assert.AreEqual(2f, Lit72Math.Gain(L0, A, L0, L1, K), 1e-5f, "at l0 the weight is 1: (0.12 + 0.12) / 0.12");
            Assert.AreEqual(1.2222222f, Lit72Math.Gain(0.27f, A, L0, L1, K), 1e-4f, "halfway: w = 0.5, (0.27 + 0.06) / 0.27");
            Assert.AreEqual(K, Lit72Math.Gain(0f, A, L0, L1, K), 1e-5f, "black is capped");
            Assert.AreEqual(2.6371f, Lit72Math.Gain(0.0733f, A, L0, L1, K), 1e-3f, "the wyrm's wall (49, 11, 6 of 255): (0.0733 + 0.12) / 0.0733");
        }

        [Test]
        public void Lift_KeepsTheHue_AndClampsAtOne()
        {
            float r = 49f / 255f, g = 11f / 255f, b = 6f / 255f;
            float k = Lit72Math.Gain(Lit72Math.Luma(r, g, b), A, L0, L1, K);
            Assert.Greater(k, 1f);
            float r2 = Lit72Math.Lift(r, k), g2 = Lit72Math.Lift(g, k), b2 = Lit72Math.Lift(b, k);
            Assert.AreEqual(r2 / g2, r / g, 1e-4f, "R:G holds");
            Assert.AreEqual(g2 / b2, g / b, 1e-4f, "G:B holds");
            Assert.AreEqual(1f, Lit72Math.Lift(0.9f, 4f), 1e-6f, "a channel clamps at 1");
            // the lifted wall clears the "dark < 30 of 255" line
            Assert.Greater(Lit72Math.Luma(r2, g2, b2) * 255f, 30f);
        }

        [Test]
        public void GainOfOne_LeavesAnHdrValueAlone()
        {
            Assert.AreEqual(1.8f, Lit72Math.Lift(1.8f, 1f), 1e-6f);
            Assert.AreEqual(0.4f, Lit72Math.Lift(0.4f, 1f), 1e-6f);
        }

        [Test]
        public void Target_BossRoom_Spire_OrGrade()
        {
            const int spire = 4;
            Assert.AreEqual(0f, Lit72Math.Target(false, 0, spire, 0f));
            Assert.AreEqual(0f, Lit72Math.Target(false, -1, spire, 0f), "a survival map has no theme");
            Assert.AreEqual(1f, Lit72Math.Target(true, 2, spire, 0f), "a boss room, any theme");
            Assert.AreEqual(1f, Lit72Math.Target(false, spire, spire, 0f), "level 5's rooms");
            Assert.AreEqual(0.6f, Lit72Math.Target(false, -1, spire, 0.6f), 1e-6f, "a live boss grade (survival boss fights)");
            Assert.AreEqual(1f, Lit72Math.Target(false, -1, spire, 3f), "a grade weight over 1 clamps");
        }

        [Test]
        public void Step_FadesLinearly_NeverOvershoots()
        {
            Assert.AreEqual(0.25f, Lit72Math.Step(0f, 1f, 0.2f, 0.8f, 0.8f), 1e-6f);
            Assert.AreEqual(1f, Lit72Math.Step(0.9f, 1f, 0.2f, 0.8f, 0.8f), 1e-6f, "reaches the target and stops");
            Assert.AreEqual(0.75f, Lit72Math.Step(1f, 0f, 0.2f, 0.8f, 0.8f), 1e-6f);
            Assert.AreEqual(0f, Lit72Math.Step(0.1f, 0f, 0.2f, 0.8f, 0.8f), 1e-6f);
            Assert.AreEqual(1f, Lit72Math.Step(0f, 1f, 0.016f, 0f, 0.8f), "a fade of 0 jumps");
            Assert.AreEqual(0.5f, Lit72Math.Step(0.5f, 0.5f, 0.016f, 0.8f, 0.8f), 1e-6f, "at the target it holds");
        }

        [Test]
        public void Tune_ReplacesAPrefix_AndSkipsABadNumber()
        {
            try
            {
                Lit72.ReadTune(new[] { "-skygrindLit72Tune=0.2,0.1" });
                Assert.AreEqual(new[] { 0.2f, 0.1f, UnityTuning.L72LitL1, UnityTuning.L72LitKMax }, Lit72.Tuned, "a prefix replaces, the rest stay");
                Lit72.ReadTune(new[] { "-skygrindLit72Tune", "0.3,x,0.5,6" });
                Assert.AreEqual(new[] { 0.3f, UnityTuning.L72LitL0, 0.5f, 6f }, Lit72.Tuned, "the next-argument form; a bad number is skipped");
                Lit72.ReadTune(new[] { "-skygrindLit72Tune=0.2,0.5,0.3" });
                Assert.Greater(Lit72.Tuned[2], Lit72.Tuned[1], "l1 stays above l0");
                Lit72.ReadTune(new[] { "-skygrindNothing" });
                Assert.AreEqual(new[] { UnityTuning.L72LitA, UnityTuning.L72LitL0, UnityTuning.L72LitL1, UnityTuning.L72LitKMax }, Lit72.Tuned, "no switch: UnityTuning's numbers");
            }
            finally { Lit72.ReadTune(null); } // back to UnityTuning's numbers for the rest of the suite
        }
    }
}
