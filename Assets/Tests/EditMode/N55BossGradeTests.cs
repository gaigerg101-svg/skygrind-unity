// NIGHT55 builder B: design.n55.grade55's numbers as data (BossGrade55.cs and Wick37View.N55.cs read them; the a155 reel measures the frames).
// Pinned:
//   1. shadows only: THE WICK's and THE CROWN's palettes carry no colour filter, no saturation and no highlight colour (the keys that tinted the
//      whole frame in grade54); each phase's balance is positive (the tint's weight falls off toward the highlights) and its EV never above 0
//   2. rule 8 / D-16: every shadow colour and every turn colour sits >= 60 deg from the danger hue (colors.enemy), outside the magenta band
//      (280-335, RIVAL's) and >= 10 deg from the player's cyan (colors.usable)
//   3. the turn: 0.3-0.8 s, at most 3 a second (turnGapS >= 1/3), darkening only
//   4. the hot wall's cold seams: value-only (S < 0.15) or >= 30 deg from the danger hue, and dim (gain < 1); a burn hotter than the approach
using System;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N55BossGradeTests
    {
        const string Row = "design.n55.grade55.numbers.";
        static readonly string[] Bosses = { "wick", "crown37" };

        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        static double[] Rgb(GameConfig c, string path)
        {
            Assert.That(c.Has(path), Is.True, path);
            double[] v = c.Nums(path);
            Assert.That(v.Length, Is.EqualTo(3), path + " is a triple");
            foreach (double x in v) Assert.That(x >= 0 && x <= 1, Is.True, path + " in 0..1");
            return v;
        }

        static void Hsv(double[] c, out double h, out double s)
        {
            double r = c[0], g = c[1], b = c[2];
            double mx = Math.Max(r, Math.Max(g, b)), mn = Math.Min(r, Math.Min(g, b)), d = mx - mn;
            h = 0;
            if (d > 0) h = mx == r ? ((g - b) / d) % 6 : mx == g ? (b - r) / d + 2 : (r - g) / d + 4;
            h = (h * 60 + 360) % 360;
            s = mx > 0 ? d / mx : 0;
        }

        static double HueDist(double a, double b)
        {
            double d = Math.Abs(a - b) % 360;
            return d > 180 ? 360 - d : d;
        }

        static double HueOf(GameConfig c, string path)
        {
            var (r, g, b) = c.Rgb(path);
            Hsv(new double[] { r, g, b }, out double h, out _);
            return h;
        }

        [Test]
        public void ShadowsOnly_NoFilter_NoSaturation_NoHighlightColour_NeverBrighter()
        {
            GameConfig c = Load();
            Assert.That(c.Has("design.n55.grade55.enabled"), Is.True, "the row exists");
            foreach (string id in Bosses)
            {
                Assert.That(c.Get(Row + id + "._why") is string w && w.Length > 0, Is.True, id + "._why");
                Assert.That(c.Num(Row + id + ".on"), Is.EqualTo(1), id + ".on");
                for (int p = 1; p <= 3; p++)
                {
                    string k = Row + id + ".p" + p + ".";
                    foreach (string banned in new[] { "filter", "highlights", "wash" })
                        Assert.That(c.Has(k + banned), Is.False, k + banned + ": grade55 never tints the whole frame or the highlights");
                    // NIGHT56 V (the r1 critic on THE CROWN: |dS| >= 0.10 against raw): a saturation delta is allowed, never above 0 (no added chroma)
                    if (c.Has(k + "sat")) Assert.That(c.Num(k + "sat"), Is.LessThanOrEqualTo(0), k + "sat: a grade never adds chroma");
                    Rgb(c, k + "split");
                    Rgb(c, k + "turn");
                    Assert.That(c.Num(k + "balance"), Is.GreaterThan(0).And.LessThanOrEqualTo(60), k + "balance: the tint confined toward the shadows");
                    Assert.That(c.Num(k + "ev"), Is.LessThanOrEqualTo(0), k + "ev: never brighter");
                    double fm = c.Num(k + "fogMix");
                    Assert.That(fm >= 0 && fm <= 1, Is.True, k + "fogMix");
                }
            }
        }

        [Test]
        public void EveryShadowAndTurnColour_IsFarFromTheDangerHue_NotMagenta_NotTheCyan()
        {
            GameConfig c = Load();
            double danger = HueOf(c, "colors.enemy"), cyan = HueOf(c, "colors.usable");
            foreach (string id in Bosses)
                for (int p = 1; p <= 3; p++)
                    foreach (string col in new[] { "split", "turn" })
                    {
                        string k = Row + id + ".p" + p + "." + col;
                        Hsv(Rgb(c, k), out double h, out double s);
                        // NIGHT57 V (V3-grade55, THE CROWN light-only): a neutral grey (S < 0.05) tones nothing (SoftLight with 0.5 is the identity): no hue
                        if (s < 0.05) continue;
                        Assert.That(HueDist(h, danger), Is.GreaterThanOrEqualTo(60), k + ": hue " + h + " vs the danger hue " + danger);
                        Assert.That(h < 280 || h > 335 || s < 0.15, Is.True, k + ": not RIVAL's magenta (hue " + h + ")");
                        Assert.That(HueDist(h, cyan), Is.GreaterThanOrEqualTo(10), k + ": not the player's cyan (hue " + h + " vs " + cyan + ")");
                    }
        }

        /// <summary>NIGHT57 V (slate V3-grade55: THE CROWN's tells kept >= the no-grade count): THE CROWN's palette is light-only: the post is the
        /// identity at rest (a neutral split and turn, no EV / contrast / saturation delta) and the fog colour untouched (the ring is URP Unlit: only the
        /// fog and the post reach it); the ambient and the key may stay cool. And a155's ride-cue floor exists for THE WICK.</summary>
        [Test]
        public void TheCrown_LightOnly_ThePostAndTheFogNeverTouchTheRing()
        {
            GameConfig c = Load();
            for (int p = 1; p <= 3; p++)
            {
                string k = Row + "crown37.p" + p + ".";
                foreach (string col in new[] { "split", "turn" })
                {
                    double[] v = Rgb(c, k + col);
                    Assert.That(Math.Abs(v[0] - 0.5) < 1e-9 && Math.Abs(v[1] - 0.5) < 1e-9 && Math.Abs(v[2] - 0.5) < 1e-9, Is.True, k + col + ": neutral 0.5 grey");
                }
                Assert.That(c.Num(k + "ev"), Is.EqualTo(0), k + "ev");
                Assert.That(c.Num(k + "contrast"), Is.EqualTo(0), k + "contrast");
                Assert.That(c.Num(k + "fogMix"), Is.EqualTo(0), k + "fogMix");
                Assert.That(c.Has(k + "sat") ? c.Num(k + "sat") : 0, Is.EqualTo(0), k + "sat");
            }
            Assert.That(c.Num(Row + "proof.panelCyanMin"), Is.GreaterThan(0).And.LessThan(1), "proof.panelCyanMin");
        }

        [Test]
        public void TheTurn_IsOneDarkeningBeat_AtMostThreeASecond()
        {
            GameConfig c = Load();
            double s = c.Num(Row + "common.turnS");
            Assert.That(s >= 0.3 && s <= 0.8, Is.True, "turnS " + s);
            Assert.That(c.Num(Row + "common.turnGapS"), Is.GreaterThanOrEqualTo(1.0 / 3), "at most 3 turns a second (WCAG 2.3.1)");
            Assert.That(c.Num(Row + "common.turnEv"), Is.LessThanOrEqualTo(0), "a turn darkens");
            Assert.That(c.Num(Row + "common.priority"), Is.GreaterThan(c.Num("design.n54.grade54.numbers.common.priority")), "over grade54's volume");
        }

        [Test]
        public void TheColdSeams_AreOffTheDangerHue_AndDim_ABurnIsTheHottest()
        {
            GameConfig c = Load();
            double danger = HueOf(c, "colors.enemy");
            Hsv(Rgb(c, Row + "hot.seam"), out double h, out double s);
            Assert.That(s < 0.15 || HueDist(h, danger) >= 30, Is.True, "the cold seams: hue " + h + " S " + s);
            Assert.That(c.Num(Row + "hot.seamGain"), Is.LessThan(1), "the cold seams are dim");
            Assert.That(c.Num(Row + "hot.burn"), Is.GreaterThan(c.Num(Row + "hot.contact")), "a burn is hotter than the approach");
            Assert.That(c.Num(Row + "hot.rippleHz"), Is.LessThan(3), "the ripple is under 3 Hz");
            Assert.That(c.Num(Row + "hot.rippleAmp"), Is.LessThanOrEqualTo(0.3), "the ripple is a shimmer, not a flash");
            Assert.That(c.Num(Row + "proof.wallMinDeg"), Is.GreaterThanOrEqualTo(30), "the contract's 30 deg");
        }

        /// <summary>NIGHT56 V (V4): THE WYRM's palette keeps GG's yellow-red-orange theme (NIGHT47) instead of rule 2's cool shadows: every split and
        /// turn colour's hue inside proof.wyrmTurnLo..Hi (15-55), shadows only (a positive balance, no filter / highlights / wash), never brighter.</summary>
        [Test]
        public void TheWyrm_WarmShadowsAndTurns_InsideItsBand_ShadowsOnly()
        {
            GameConfig c = Load();
            if (!c.Has(Row + "wyrm.p1.split")) Assert.Ignore("no wyrm palette in grade55 (grade54's look)");
            double lo = c.Num(Row + "proof.wyrmTurnLo"), hi = c.Num(Row + "proof.wyrmTurnHi");
            Assert.That(lo >= 15 && hi <= 55 && lo < hi, Is.True, "the slate's 15-55: " + lo + "-" + hi);
            for (int p = 1; p <= 3; p++)
            {
                string k = Row + "wyrm.p" + p + ".";
                foreach (string col in new[] { "split", "turn" })
                {
                    Hsv(Rgb(c, k + col), out double h, out _);
                    Assert.That(h >= lo && h <= hi, Is.True, k + col + ": hue " + h + " inside " + lo + "-" + hi);
                }
                foreach (string banned in new[] { "filter", "highlights", "wash" })
                    Assert.That(c.Has(k + banned), Is.False, k + banned + ": shadows only");
                Assert.That(c.Num(k + "balance"), Is.GreaterThan(0).And.LessThanOrEqualTo(60), k + "balance");
                Assert.That(c.Num(k + "ev"), Is.LessThanOrEqualTo(0), k + "ev: never brighter");
            }
        }
    }
}
