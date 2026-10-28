// NIGHT54 builder G: design.n54.grade54's numbers as data (BossGrade54.cs reads them; the a254 reel measures the frames). Pinned:
//   1. each campaign boss (wick, brawl40, wyrm, loom37, crown37, rival) has a _why, a rim in the player's cyan family (hue 160-215) with a
//      gain > 0, and three phases whose colours are 0..1 triples, whose EV is never above 0 (never brighter) and whose fogMix is 0..1
//   2. the turn: 0.3-0.8 s (GG's ask), at most 3 a second (turnGapS >= 1/3: the photosensitivity limit), darkening only (turnEv <= 0)
//   3. the six P1 palettes are distinct: the duotone's midpoint x the filter x 2^EV differs pairwise by >= 40 deg in hue, >= 0.1 in value or >= 0.15 in saturation
//   4. rule 8: no palette but RIVAL's puts a saturated colour in the magenta band (hue 280-335) in its highlights or its turn's wash
using System;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N54GradeTests
    {
        const string Row = "design.n54.grade54.numbers.";
        static readonly string[] Bosses = { "wick", "brawl40", "wyrm", "loom37", "crown37", "rival" };

        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        static double[] Rgb(GameConfig c, string k)
        {
            Assert.That(c.Has(Row + k), Is.True, k);
            double[] v = c.Nums(Row + k);
            Assert.That(v.Length, Is.EqualTo(3), k + " is a triple");
            foreach (double x in v) Assert.That(x >= 0 && x <= 1, Is.True, k + " in 0..1");
            return v;
        }

        static void Hsv(double r, double g, double b, out double h, out double s, out double v)
        {
            double mx = Math.Max(r, Math.Max(g, b)), mn = Math.Min(r, Math.Min(g, b)), d = mx - mn;
            h = 0;
            if (d > 0) h = mx == r ? ((g - b) / d) % 6 : mx == g ? (b - r) / d + 2 : (r - g) / d + 4;
            h = (h * 60 + 360) % 360;
            s = mx > 0 ? d / mx : 0;
            v = mx;
        }

        static double HueDist(double a, double b)
        {
            double d = Math.Abs(a - b) % 360;
            return d > 180 ? 360 - d : d;
        }

        [Test]
        public void EachBoss_HasThreePhases_ARim_InTheCyanFamily_NeverBrighter()
        {
            GameConfig c = Load();
            foreach (string id in Bosses)
            {
                Assert.That(c.Get(Row + id + "._why") is string w && w.Length > 0, Is.True, id + "._why");
                double[] rim = Rgb(c, id + ".rim");
                Hsv(rim[0], rim[1], rim[2], out double rh, out double rs, out _);
                Assert.That(rh >= 160 && rh <= 215 && rs >= 0.3, Is.True, id + ": the rim is the player's cyan family (rule 8), hue " + rh);
                Assert.That(c.Num(Row + id + ".rimGain"), Is.GreaterThan(0), id + ".rimGain");
                for (int p = 1; p <= 3; p++)
                {
                    string k = id + ".p" + p + ".";
                    foreach (string col in new[] { "filter", "shadows", "highlights", "fog", "ambient", "key", "wash" }) Rgb(c, k + col);
                    Assert.That(c.Num(Row + k + "ev"), Is.LessThanOrEqualTo(0), k + "ev: never brighter");
                    double fm = c.Num(Row + k + "fogMix");
                    Assert.That(fm >= 0 && fm <= 1, Is.True, k + "fogMix");
                    Assert.That(c.Get(Row + k + "_why") is string pw && pw.Length > 0, Is.True, k + "_why");
                }
            }
        }

        [Test]
        public void TheTurn_IsOneShortDarkeningEnvelope_AtMostThreeASecond()
        {
            GameConfig c = Load();
            double s = c.Num(Row + "common.turnS");
            Assert.That(s >= 0.3 && s <= 0.8, Is.True, "turnS in GG's 0.3-0.8 s");
            Assert.That(c.Num(Row + "common.turnGapS"), Is.GreaterThanOrEqualTo(1.0 / 3), "at most 3 turns a second (WCAG 2.3.1)");
            Assert.That(c.Num(Row + "common.turnEv"), Is.LessThanOrEqualTo(0), "a turn darkens, never a white flash");
            double wm = c.Num(Row + "common.washMix");
            Assert.That(wm >= 0 && wm <= 1, Is.True, "washMix");
        }

        [Test]
        public void TheSixPalettes_AreDistinct_InHueOrValue()
        {
            GameConfig c = Load();
            var h = new double[Bosses.Length];
            var s = new double[Bosses.Length];
            var v = new double[Bosses.Length];
            for (int i = 0; i < Bosses.Length; i++)
            {
                string k = Bosses[i] + ".p1.";
                double[] f = Rgb(c, k + "filter"), sh = Rgb(c, k + "shadows"), hi = Rgb(c, k + "highlights");
                double ev = Math.Pow(2, c.Num(Row + k + "ev"));
                Hsv((sh[0] + hi[0]) / 2 * f[0] * ev, (sh[1] + hi[1]) / 2 * f[1] * ev, (sh[2] + hi[2]) / 2 * f[2] * ev, out h[i], out s[i], out v[i]);
            }
            // distinct in hue, in value, or in chroma (the n54-freeze cuts of THE WICK / THE CROWN are near-neutral: they differ by chroma)
            for (int a = 0; a < Bosses.Length; a++)
                for (int b = a + 1; b < Bosses.Length; b++)
                    Assert.That(HueDist(h[a], h[b]) >= 40 || Math.Abs(v[a] - v[b]) >= 0.1 || Math.Abs(s[a] - s[b]) >= 0.15, Is.True,
                        Bosses[a] + " vs " + Bosses[b] + ": dH " + HueDist(h[a], h[b]).ToString("0") + ", dV " + Math.Abs(v[a] - v[b]).ToString("0.00") + ", dS " + Math.Abs(s[a] - s[b]).ToString("0.00"));
        }

        [Test]
        public void MagentaIsRivalsOnly()
        {
            GameConfig c = Load();
            foreach (string id in Bosses)
            {
                if (id == "rival") continue;
                for (int p = 1; p <= 3; p++)
                    foreach (string col in new[] { "highlights", "wash" })
                    {
                        double[] x = Rgb(c, id + ".p" + p + "." + col);
                        Hsv(x[0], x[1], x[2], out double hh, out double ss, out _);
                        Assert.That(ss > 0.15 && hh >= 280 && hh <= 335, Is.False, id + ".p" + p + "." + col + " is magenta (rule 8: RIVAL's only), hue " + hh.ToString("0"));
                    }
            }
        }
    }
}
