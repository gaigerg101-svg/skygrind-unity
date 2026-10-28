// NIGHT57 builder V (slate V1-colour56): colour56's WORLD SATURATION CAP (Game/Render/Cap57Pass.cs; the shader Assets/Shaders/Resources/Cap57.shader
// runs Cap57Math's maths; Tools/night/n57-v-cap.py planted runs the same cases in Python). Pinned:
//   1. the maths: a non-role colour over the cap moves to the cap at its own hue and luminance; the role tokens (danger, parry yellow, usable cyan, RIVAL),
//      a colour under the cap and near black never move; the feather's half-way pixel moves part way; a cap of 1 moves nothing
//   2. the numbers (design.n56.colour56.numbers.cap): a cap per level 1-5 in [0.5, 1); the role arcs protect every role token whole and leave every
//      colour56 key / accent hue outside them (so the key and the accent sit under the cap); each cap >= V1's MAIN fallback threshold (S 0.35) with room
//   3. the row: colour56 off in the shipped config -> no cap (NIGHT56's frame); colour56 forced on -> the level's cap; an unknown level -> none
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N57ColourTests
    {
        static string Root => Path.GetFullPath("PortExport");
        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));
        const string P = "design.n56.colour56.numbers.cap.";
        static readonly string[] Levels = { "foundry", "iceline", "kiln", "span", "spire" };
        static readonly double[] Arcs = { 350, 70, 150, 200, 285, 335 };

        static void Hsv(double h, double s, double v, out double r, out double g, out double b)
        {
            double c = v * s, hp = (h % 360) / 60.0, x = c * (1 - System.Math.Abs(hp % 2 - 1)), m = v - c;
            double r1 = 0, g1 = 0, b1 = 0;
            if (hp < 1) { r1 = c; g1 = x; } else if (hp < 2) { r1 = x; g1 = c; } else if (hp < 3) { g1 = c; b1 = x; }
            else if (hp < 4) { g1 = x; b1 = c; } else if (hp < 5) { r1 = x; b1 = c; } else { r1 = c; b1 = x; }
            r = r1 + m; g = g1 + m; b = b1 + m;
        }

        static void Through(double h, double s, double v, double cap, out double h2, out double s2, out double v2, out bool moved)
        {
            Hsv(h, s, v, out double r, out double g, out double b);
            Cap57Math.Apply(r, g, b, cap, Arcs, 6, out double r2, out double g2, out double b2);
            moved = System.Math.Abs(r2 - r) > 1e-9 || System.Math.Abs(g2 - g) > 1e-9 || System.Math.Abs(b2 - b) > 1e-9;
            Cap57Math.Hs(r2, g2, b2, out h2, out s2);
            v2 = Cap57Math.Luma(r2, g2, b2) - Cap57Math.Luma(r, g, b); // the luminance change (the cap keeps it: 0)
        }

        static double Gap(double a, double b)
        {
            double d = System.Math.Abs(a - b) % 360;
            return d > 180 ? 360 - d : d;
        }

        [Test]
        public void TheMaths_NonRoleOverTheCap_MovesToTheCap_HueAndLuminanceKept()
        {
            Through(120, 1.0, 0.9, 0.6, out double h, out double s, out double dl, out bool moved);
            Assert.That(moved && System.Math.Abs(s - 0.6) < 1e-6 && Gap(h, 120) < 1e-6 && System.Math.Abs(dl) < 1e-9, Is.True, $"green: H {h} S {s} dLuma {dl}");
            Through(345, 0.9, 0.7, 0.7, out h, out s, out dl, out moved);
            Assert.That(moved && System.Math.Abs(s - 0.7) < 1e-6 && Gap(h, 345) < 1e-6 && System.Math.Abs(dl) < 1e-9, Is.True, $"the kiln's rose key: H {h} S {s} dLuma {dl}");
            Through(230, 0.95, 0.8, 0.7, out h, out s, out dl, out moved);
            Assert.That(moved && System.Math.Abs(s - 0.7) < 1e-6 && Gap(h, 230) < 1e-6 && System.Math.Abs(dl) < 1e-9, Is.True, $"a saturated blue: S {s} dLuma {dl}");
        }

        [Test]
        public void TheMaths_RoleTokensUnderTheCapAndBlack_NeverMove()
        {
            foreach (double hue in new double[] { 12, 0, 358, 44, 60, 186, 160, 312, 325 })
            {
                Through(hue, 0.95, 0.95, 0.5, out _, out double s, out _, out bool moved);
                Assert.That(!moved, Is.True, $"role hue {hue} moved (S now {s})");
            }
            Through(240, 0.2, 0.5, 0.6, out _, out _, out _, out bool m1);
            Assert.That(!m1, Is.True, "a low-S wall under the cap moved");
            Through(220, 0.0, 0.02, 0.6, out _, out _, out _, out bool m2);
            Assert.That(!m2, Is.True, "near black moved");
            Through(120, 1.0, 0.9, 1.0, out _, out _, out _, out bool m3);
            Assert.That(!m3, Is.True, "a cap of 1 moved a pixel");
            Through(153, 1.0, 0.9, 0.6, out _, out double sf, out _, out bool m4);
            Assert.That(m4 && sf > 0.65 && sf < 0.98, Is.True, $"the feather's half-way pixel (153): S {sf}");
        }

        [Test]
        public void TheNumbers_ACapPerLevel_TheArcsProtectEveryRoleToken_TheKeysUnderTheCap()
        {
            GameConfig c = Cfg();
            Assert.That(c.Has(P + "roleArcs") && c.Has(P + "featherDeg"), Is.True, "cap.roleArcs / featherDeg");
            double[] arcs = c.Nums(P + "roleArcs");
            Assert.That(arcs.Length == 6, Is.True, "three (lo, hi) arcs");
            double f = c.Num(P + "featherDeg");
            // the role tokens (colors: danger ~12, the parry token #FFC93A ~44, usable cyan ~186, RIVAL ~312) whole inside an arc
            foreach (double token in new double[] { 12, 44, 186, 312 })
                Assert.That(Cap57Math.Protect(token, arcs, f) >= 1 - 1e-9, Is.True, $"role token hue {token} not whole inside an arc");
            foreach (string lv in Levels)
            {
                Assert.That(c.Has(P + "levels." + lv), Is.True, "cap for " + lv);
                double cap = c.Num(P + "levels." + lv);
                Assert.That(cap >= 0.5 && cap < 1, Is.True, $"{lv} cap {cap} in [0.5, 1)");
                foreach (string k in new[] { "keyHex", "accentHex" })
                {
                    uint hex = (uint)c.Num("design.n56.colour56.numbers.levels." + lv + "." + k);
                    Cap57Math.Hs(((hex >> 16) & 255) / 255.0, ((hex >> 8) & 255) / 255.0, (hex & 255) / 255.0, out double h, out _);
                    Assert.That(Cap57Math.Protect(h, arcs, f) == 0, Is.True, $"{lv} {k} hue {h:0} inside a role arc (the cap would not hold it)");
                }
            }
        }

        /// <summary>NIGHT57 V4 yellow56's dim yellow (design.n56.yellow56.numbers.dimYellow; the same pass): a dim mustard drops under the meter's
        /// yellow (S 0.5), luminance kept; the parry token (#FFC93A, V 1.0), a bright yellow and a dim red-orange never move; yellow56 off: none.</summary>
        [Test]
        public void TheDimYellow_UnderTheMetersYellow_TheParryTokenKept()
        {
            GameConfig c = Cfg();
            const string Y = "design.n56.yellow56.numbers.dimYellow.";
            double[] y = { c.Num(Y + "sMax"), c.Num(Y + "vFull"), c.Num(Y + "vNone"), c.Num(Y + "hueLo"), c.Num(Y + "hueHi"), c.Num(Y + "featherDeg") };
            Assert.That(y[0] < 0.5 && y[1] < y[2] && y[2] < 0.85, Is.True, "sMax under the meter's 0.5; vFull < vNone < the orbs' 0.85");
            Assert.That(Cap57Math.ArcWeight(45, y[3], y[4], y[5]) >= 1 - 1e-9 && Cap57Math.ArcWeight(70, y[3], y[4], y[5]) >= 1 - 1e-9, Is.True, "the meter's 45-70 whole");
            Assert.That(Cap57Math.ArcWeight(35, y[3], y[4], y[5]) == 0, Is.True, "the danger band (to 35) outside the arc");
            Hsv(55, 0.8, 0.35, out double r, out double g, out double b);
            Cap57Math.Apply(r, g, b, 1, Arcs, 6, y, out double r2, out double g2, out double b2);
            Cap57Math.Hs(r2, g2, b2, out _, out double s2);
            Assert.That(s2 <= y[0] + 1e-6 && System.Math.Abs(Cap57Math.Luma(r2, g2, b2) - Cap57Math.Luma(r, g, b)) < 1e-9, Is.True, $"dim mustard: S {s2}");
            foreach (double[] k in new[] { new double[] { 44, 0.77, 1.0 }, new double[] { 60, 0.9, 0.9 }, new double[] { 20, 0.9, 0.35 } })
            {
                Hsv(k[0], k[1], k[2], out r, out g, out b);
                Cap57Math.Apply(r, g, b, 1, Arcs, 6, y, out r2, out g2, out b2);
                Assert.That(System.Math.Abs(r2 - r) < 1e-12 && System.Math.Abs(g2 - g) < 1e-12 && System.Math.Abs(b2 - b) < 1e-12, Is.True, $"hue {k[0]} V {k[2]} moved");
            }
            Cap57.ForMap(c, "n52-spire");
            Assert.That(!Cap57.YellowOn, Is.True, "yellow56 off in the shipped config: no dim-yellow cap; " + Cap57.YellowLine);
        }

        /// <summary>NIGHT57 V r2 (the r1 greyscale guard): the frieze's luma ceiling low, and the NIGHT57 walls neutral and no brighter (Rec. 709 luma)
        /// than NIGHT56's neutral walls they replace.</summary>
        [Test]
        public void TheR2Numbers_TheFriezeDarkened_TheWallsAtTheirOldLuminance()
        {
            GameConfig c = Cfg();
            const string N = "design.n56.colour56.numbers.";
            Assert.That(c.Num(N + "friezeLumaMax") > 0.05 && c.Num(N + "friezeLumaMax") <= 0.3, Is.True, "friezeLumaMax in (0.05, 0.3]");
            Assert.That(UnityTuning.Colour57AccentMaxChannel > 0.2f && UnityTuning.Colour57AccentMaxChannel < UnityTuning.Colour56AccentMaxChannel, Is.True, "the accent's channel lower than NIGHT56's");
            foreach (string lv in new[] { "iceline", "span" })
                foreach (string w in new[] { "wallBaseHex", "wallUpperHex" })
                {
                    uint h57 = (uint)c.Num(N + "levels." + lv + "." + w + "57"), h56 = (uint)c.Num(N + "levels." + lv + "." + w);
                    double r = ((h57 >> 16) & 255) / 255.0, g = ((h57 >> 8) & 255) / 255.0, b = (h57 & 255) / 255.0;
                    Cap57Math.Hs(r, g, b, out _, out double s);
                    double l57 = Cap57Math.Luma(r, g, b), l56 = Cap57Math.Luma(((h56 >> 16) & 255) / 255.0, ((h56 >> 8) & 255) / 255.0, (h56 & 255) / 255.0);
                    Assert.That(s < 0.15 && l57 < l56, Is.True, $"{lv} {w}57: S {s:0.00}, luma {l57:0.000} vs NIGHT56's {l56:0.000}");
                }
        }

        [Test]
        public void TheRow_OffInTheShippedConfig_NoCap_ForcedOn_TheLevelsCap()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                // NIGHT57 lead (the suite, 13:5x): colour56 ships ON since the NIGHT57 freeze, and a row's on / off is read once per config
                // object, so each side gets its own fresh config with the row set explicitly
                GameConfig off = Cfg();
                off.Obj("design.n56.colour56")["enabled"] = false;
                Cap57.ForMap(off, "n52-foundry");
                Assert.That(Cap57.Value >= 1f, Is.True, "colour56 off: no cap (NIGHT56's frame); " + Cap57.Line);
                GameConfig c = Cfg();
                c.Obj("design.n56.colour56")["enabled"] = true;
                Cap57.ForMap(c, "n52-foundry");
                Assert.That(System.Math.Abs(Cap57.Value - c.Num(P + "levels.foundry")) < 1e-6, Is.True, "colour56 on: foundry's cap; " + Cap57.Line);
                Cap57.ForMap(c, "n52-nowhere");
                Assert.That(Cap57.Value >= 1f, Is.True, "a level with no cap: none; " + Cap57.Line);
                NightRows.ForceOffFrom = 57;
                Cap57.ForMap(c, "n52-foundry");
                Assert.That(Cap57.Value >= 1f, Is.True, "-skygrindOff57: NIGHT56's colour56, no cap; " + Cap57.Line);
            }
            finally
            {
                NightRows.ForceOffFrom = was;
                Cap57.Value = 1f;
            }
        }
    }
}
