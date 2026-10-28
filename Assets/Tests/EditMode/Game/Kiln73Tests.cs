// NIGHT73 builder K (Game/Kiln73.cs, kiln73). Pinned:
//   1. the cause: the config's THE WYRM shadows (grade54 P1 (0.36, 0.34, 0.20), P2 (0.34, 0.27, 0.15), P3 (0.28, 0.21, 0.13)) clamp a channel when BossGrade54 pushes them 1.8x
//      away from grey and come out fully saturated with blue 0 (P1 amber-yellow hue 51, P2 orange hue 24 in the lead's danger band, P3 dark red hue 0, V 0.10);
//      kiln73's shadows, filter, wash and highlights clamp nothing and stay out of the band
//   2. every colour kiln73 writes is out of the danger band (PushedInDanger = 0) and the shadows are blue-dominant (indigo-plum, hue 230-290)
//   3. the lead's band is the meter's (hue 0-40 / 345-360, S >= 0.35, V >= 0.20): orange in, plum / sand / white / cyan out
//   4. the rim colour (grade54's rimHdr, pre-divided by P1's filter) is recomputed: with the config's blue-cutting filter it was boosted several times over; with kiln73's it is not
//   5. (K2, the EXPOSED edge hidden in the arena, was retired in NIGHT73 K3: its test went with it; kiln73 no longer touches the HUD's edges)
//   6. the palette copies: Cool54 / Cool55 write kiln73's numbers and leave `ev` (and grade55's turnBalance) as the config has them; Copy54 / Copy55 put every field back
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Kiln73Tests
    {
        const float K = 1.8f; // design.n54.grade54.numbers.common.intensity

        static float Hue(Color c)
        {
            Color.RGBToHSV(c, out float h, out _, out _);
            return h * 360f;
        }

        [Test]
        public void TheConfigsWyrmShadows_ClampAndSitInTheBand_KilnsDoNot()
        {
            var p1 = new Color(0.36f, 0.34f, 0.20f);
            var p3 = new Color(0.28f, 0.21f, 0.13f);
            Assert.IsTrue(Kiln73Math.PushClamps(p1, Color.grey, K), "P1: blue 0.5 + (0.2 - 0.5) x 1.8 = -0.04");
            Assert.IsTrue(Kiln73Math.PushClamps(p3, Color.grey, K), "P3: green and blue go negative");
            Assert.AreEqual(0f, Kiln73Math.Push(p1, Color.grey, K).b, 1e-6f, "the clamped blue is 0: SoftLight with a blend of 0 squares the channel");
            // what the push makes of the config's three shadows (computed by hand: max - min and (g - b) / (max - min) for a red-max colour):
            //   P1 (0.248, 0.212, 0.000): hue 51.3, S 1.00, V 0.248: a saturated amber-yellow, blue gone (hue over 40: out of the danger band, in the parry arc)
            //   P2 (0.212, 0.086, 0.000): hue 24.3, S 1.00, V 0.212: a saturated orange, in the danger band
            //   P3 (0.104, 0.000, 0.000): hue 0, S 1.00, V 0.104: a pure dark red (value under the band's 0.20)
            Color.RGBToHSV(Kiln73Math.Push(p1, Color.grey, K), out float h1, out float s1, out float v1);
            Assert.AreEqual(51.3f, h1 * 360f, 0.5f, "P1 pushed: hue (the first draft of this test said 9: (r - g) was used for (g - b))");
            Assert.AreEqual(1f, s1, 1e-4f, "P1 pushed: fully saturated (blue clamped to 0)");
            Assert.AreEqual(0.248f, v1, 1e-3f);
            Assert.IsFalse(Kiln73Math.InDanger(Kiln73Math.Push(p1, Color.grey, K)), "hue 51 is over the band's 40");
            var p2 = new Color(0.34f, 0.27f, 0.15f);
            Assert.IsTrue(Kiln73Math.PushClamps(p2, Color.grey, K), "P2: blue 0.5 + (0.15 - 0.5) x 1.8 = -0.13");
            Assert.IsTrue(Kiln73Math.InDanger(Kiln73Math.Push(p2, Color.grey, K)), "the pushed P2 shadow (hue 24, S 1, V 0.21) is in the danger band");
            Color.RGBToHSV(Kiln73Math.Push(p3, Color.grey, K), out float h3, out float s3, out float v3);
            Assert.AreEqual(0f, h3 * 360f, 0.5f, "P3 pushed: pure red");
            Assert.AreEqual(1f, s3, 1e-4f);
            Assert.Less(v3, 0.20f, "P3 pushed: dark (0.104), under the band's value floor");
            for (int i = 0; i < 3; i++)
            {
                Color s = Kiln73Math.C(UnityTuning.Kiln73Shadows, i);
                Assert.IsFalse(Kiln73Math.PushClamps(s, Color.grey, K), "kiln73 shadows P" + (i + 1) + " clamp nothing");
                Assert.IsFalse(Kiln73Math.InDanger(Kiln73Math.Push(s, Color.grey, K)), "and are out of the band");
            }
            Assert.IsFalse(Kiln73Math.PushClamps(Kiln73Math.C(UnityTuning.Kiln73Filter), Color.white, K), "the filter");
            Assert.IsFalse(Kiln73Math.PushClamps(Kiln73Math.C(UnityTuning.Kiln73Wash), Color.white, K), "the wash");
            Assert.IsFalse(Kiln73Math.PushClamps(Kiln73Math.C(UnityTuning.Kiln73Highlights), Color.grey, K), "the highlights");
        }

        [Test]
        public void EveryColourKilnWrites_IsOutOfTheBand_AndTheShadowsAreCool()
        {
            Assert.AreEqual(0, Kiln73Math.PushedInDanger(K), "none of the " + Kiln73Math.PushedColours + " colours is in the danger band at the config's intensity");
            Assert.AreEqual(0, Kiln73Math.PushedInDanger(1f), "nor at intensity 1");
            Assert.AreEqual(9, UnityTuning.Kiln73Shadows.Length, "three phases of (r, g, b)");
            Assert.AreEqual(3, UnityTuning.Kiln73Filter.Length);
            Assert.AreEqual(3, UnityTuning.Kiln73Split55.Length);
            for (int i = 0; i < 3; i++)
            {
                Color s = Kiln73Math.Push(Kiln73Math.C(UnityTuning.Kiln73Shadows, i), Color.grey, K);
                Assert.Greater(s.b, s.r, "P" + (i + 1) + " shadow: blue over red");
                Assert.Greater(s.b, s.g, "P" + (i + 1) + " shadow: blue over green");
                Assert.That(Hue(s), Is.InRange(230f, 290f), "P" + (i + 1) + " shadow hue");
            }
            Assert.That(UnityTuning.Kiln73Sat, Is.LessThanOrEqualTo(0f), "no saturation lift (a lifted sand floor crosses S 0.35)");
            Assert.That(UnityTuning.Kiln73TurnSat, Is.EqualTo(0f));
        }

        [Test]
        public void TheDangerBand_IsTheMeters()
        {
            Assert.IsTrue(Kiln73Math.InDanger(new Color(0.9f, 0.3f, 0.1f)), "danger orange");
            Assert.IsTrue(Kiln73Math.InDanger(new Color(0.55f, 0.1f, 0.2f)), "crimson (hue 348) in");
            Assert.IsTrue(Kiln73Math.InDanger(new Color(0.20f, 0.10f, 0.02f)), "dark amber at value 0.20: in (the edge)");
            Assert.IsFalse(Kiln73Math.InDanger(new Color(0.19f, 0.10f, 0.02f)), "value 0.19 is under the meter's 0.20");
            Assert.IsFalse(Kiln73Math.InDanger(new Color(0.49f, 0.42f, 0.36f)), "sand: hue 30 but S 0.27 is under 0.35");
            Assert.IsFalse(Kiln73Math.InDanger(new Color(0.42f, 0.38f, 0.62f)), "plum");
            Assert.IsFalse(Kiln73Math.InDanger(new Color(0.95f, 0.95f, 1f)), "violet-white");
            Assert.IsFalse(Kiln73Math.InDanger(new Color(0.1f, 0.8f, 0.9f)), "cyan");
            Assert.IsFalse(Kiln73Math.InDanger(new Color(0.9f, 0.7f, 0.1f)), "yellow (hue 47) is the parry arc, not the band");
        }

        [Test]
        public void RimHdr_IsRecomputedAgainstTheNewFilter()
        {
            var rim = new Color(0.08f, 0.86f, 1f); // design.n54.grade54.numbers.wyrm.rim
            float gain = 2.2f, minF = 0.3f;
            Color cfgFilter = new Color(0.95f, 0.96f, 0.78f);
            Color cool = Kiln73Math.C(UnityTuning.Kiln73Filter);
            Color a = Kiln73Math.RimHdr(rim, cfgFilter, K, minF, gain);
            Color b = Kiln73Math.RimHdr(rim, cool, K, minF, gain);
            Assert.Greater(a.b, b.b * 2.5f, "the config's filter (blue pushed to 0.60) boosted the rim's blue several times over");
            Assert.AreEqual(rim.b * gain / Kiln73Math.Push(cool, Color.white, K).linear.b, b.b, 1e-3f, "the cool filter's blue is 1.0: the rim's blue x the gain");
            Assert.AreEqual(gain, b.a, 1e-6f, "alpha is the gain, as BossGrade54.ReadBoss writes it");
        }

        [Test]
        public void Palettes_CoolAndRestore()
        {
            var cfg54 = new Grade54Phase
            {
                filter = new Color(0.95f, 0.96f, 0.78f), shadows = new Color(0.36f, 0.34f, 0.2f), highlights = new Color(0.54f, 0.76f, 0.3f), fog = new Color(0.16f, 0.14f, 0.06f),
                ambient = new Color(0.85f, 0.82f, 0.55f), key = new Color(0.95f, 0.95f, 0.7f), wash = new Color(0.85f, 0.95f, 0.45f), balance = -8, sat = 4, contrast = 16, ev = -0.32f, fogMix = 0.55f,
            };
            var keep54 = new Grade54Phase();
            Kiln73Math.Copy54(keep54, cfg54);
            Kiln73Math.Cool54(cfg54, 1);
            Assert.AreEqual(Kiln73Math.C(UnityTuning.Kiln73Shadows, 1), cfg54.shadows, "phase 2's shadows");
            Assert.AreEqual(Kiln73Math.C(UnityTuning.Kiln73Filter), cfg54.filter);
            Assert.AreEqual(Kiln73Math.C(UnityTuning.Kiln73Wash), cfg54.wash);
            Assert.AreEqual(UnityTuning.Kiln73Sat, cfg54.sat);
            Assert.AreEqual(UnityTuning.Kiln73Contrast, cfg54.contrast);
            Assert.AreEqual(0f, cfg54.balance);
            Assert.AreEqual(-0.32f, cfg54.ev, "ev is the config's (Bright71 zeroes it in the game)");
            Kiln73Math.Copy54(cfg54, keep54);
            Assert.AreEqual(new Color(0.36f, 0.34f, 0.2f), cfg54.shadows, "restored");
            Assert.AreEqual(4f, cfg54.sat);
            Assert.AreEqual(-8f, cfg54.balance);
            Assert.AreEqual(0.55f, cfg54.fogMix);

            var cfg55 = new Grade55Phase
            {
                split = new Color(0.62f, 0.48f, 0.34f), turn = new Color(0.7f, 0.42f, 0.22f), fog = new Color(0.16f, 0.09f, 0.05f), ambient = new Color(1f, 0.88f, 0.72f), key = new Color(1f, 0.9f, 0.75f),
                balance = 30, ev = -0.15f, contrast = 12, fogMix = 0.45f, turnBalance = 0,
            };
            var keep55 = new Grade55Phase();
            Kiln73Math.Copy55(keep55, cfg55);
            Kiln73Math.Cool55(cfg55);
            Assert.AreEqual(Kiln73Math.C(UnityTuning.Kiln73Split55), cfg55.split);
            Assert.AreEqual(Kiln73Math.C(UnityTuning.Kiln73Turn55), cfg55.turn);
            Assert.AreEqual(UnityTuning.Kiln73Balance55, cfg55.balance);
            Assert.AreEqual(-0.15f, cfg55.ev, "ev is the config's");
            Assert.AreEqual(0f, cfg55.turnBalance, "turnBalance is the config's");
            Kiln73Math.Copy55(cfg55, keep55);
            Assert.AreEqual(new Color(0.62f, 0.48f, 0.34f), cfg55.split, "restored");
            Assert.AreEqual(30f, cfg55.balance);
            Assert.AreEqual(12f, cfg55.contrast);
            Assert.AreEqual(0.45f, cfg55.fogMix);
        }
    }
}
