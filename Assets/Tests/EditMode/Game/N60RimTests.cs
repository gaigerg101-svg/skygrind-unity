// NIGHT60 builder V (B7, rim56's trims; design.n56.rim56): the string course takes rim56's accent dim (Rim60.Band, Decor55's band colour). Pinned:
//   1. with the row on, the band colour's largest channel is x UnityTuning.Rim60BandScale, its hue, saturation and alpha kept (decor55's band never
//      took NIGHT58's dim: Tools/night/n60-v-rimpath.py, 85-97 % of the accent pixels the same on and off)
//   2. with the row off, the band colour is exactly as it came (NIGHT59's band)
//   3. (B7 attempt 2) with the row on, the lit side's gain is UnityTuning.Rim60LitGain, under NIGHT57's 2.205 and not under skin52's own; off,
//      NIGHT59's gain as it came
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N60RimTests
    {
        // foundry's accent #a267e6 at the band's luminance 0.42 (Decor55.Recolour: the accent x 0.42 / its own luminance 0.489)
        static readonly Color Foundry = new Color(0.546f, 0.347f, 0.776f, 1f);

        [Test]
        public void Band_RowOn_TakesRim56sAccentDim_HueAndSaturationKept()
        {
            Color on = Rim60.Band(Foundry, true);
            Color.RGBToHSV(Foundry, out float h0, out float s0, out float v0);
            Color.RGBToHSV(on, out float h1, out float s1, out float v1);
            Assert.That(UnityTuning.Rim60BandScale, Is.GreaterThan(0f).And.LessThan(1f));
            Assert.That(v1, Is.EqualTo(v0 * UnityTuning.Rim60BandScale).Within(1e-5f), "the largest channel x Rim60BandScale");
            Assert.That(h1, Is.EqualTo(h0).Within(1e-4f), "the hue kept");
            Assert.That(s1, Is.EqualTo(s0).Within(1e-4f), "the saturation kept");
            Assert.That(on.a, Is.EqualTo(Foundry.a), "the alpha kept");
        }

        [Test]
        public void LitGain_RowOn_UnderNight57s_RowOff_AsItCame()
        {
            float n57 = UnityTuning.Rim56LitGain * UnityTuning.Rim57Gain; // 2.205: the brightest bodies washed under the env's saturation
            Assert.That(Rim60.LitGain(n57, true), Is.EqualTo(UnityTuning.Rim60LitGain), "the row on: NIGHT60's gain");
            Assert.That(UnityTuning.Rim60LitGain, Is.LessThan(n57).And.GreaterThanOrEqualTo(UnityTuning.Skin52LitGain), "between skin52's own and NIGHT57's");
            Assert.That(Rim60.LitGain(n57, false), Is.EqualTo(n57), "the row off: NIGHT59's gain as it came");
        }

        [Test]
        public void LitGain_KeepsTheTellsOverTheWorldsCap()
        {
            // B7 attempt 3 (Tools/night/n60-v-rimgain2.py on the Player builds n60-b4 / n60-b5): at 1.3 iceline g2-entry's tells (median S 0.646) fell
            // under the world's capped 0.654 (colour56's guard 3 -> 4 / 40); 1.2 leaves a 0.015 margin, 1.1 a 0.040 margin with the MAIN at 130 / 141
            Assert.That(UnityTuning.Rim60LitGain, Is.LessThanOrEqualTo(1.1f).And.GreaterThanOrEqualTo(UnityTuning.Skin52LitGain),
                "the lit gain at or under the guard's bound, not under skin52's own");
        }

        [Test]
        public void Band_RowOff_AsItCame()
        {
            Color off = Rim60.Band(Foundry, false);
            Assert.That(off.r, Is.EqualTo(Foundry.r));
            Assert.That(off.g, Is.EqualTo(Foundry.g));
            Assert.That(off.b, Is.EqualTo(Foundry.b));
            Assert.That(off.a, Is.EqualTo(Foundry.a));
        }
    }
}
