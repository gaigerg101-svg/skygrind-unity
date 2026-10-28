// NIGHT72 builder C (slate C1, colours63 made right; Game/Colours63Math.N63D.cs InDangerReach / WorldAt, Colours63.N63D.cs Scan, UnityTuning.N72C.cs Colours72*). Pinned:
//   1. the danger reach: the danger band (345 .. 35) widened by Colours72DangerMarginDeg takes the spire's rose accent (hue 345.0: the "dark-red stripes") and
//      no other level's accent (foundry violet, iceline periwinkle, kiln blue, span indigo), nor cyan, nor RIVAL's magenta
//   2. WorldAt: a colour goes to the exit hue at the world tier's saturation (a lower one is kept) and its own luminance; the result wears no role
//      (never danger, parry, player, rival, nor the boundary's pale violet-white), and a second pass over it changes nothing
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using M = Skygrind.Game.Colours63Math;

namespace Skygrind.Game.Tests
{
    public class Colours72Tests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static M.Table Table() => M.From(GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json")));

        static double[] Hex(uint x) => new[] { ((x >> 16) & 255) / 255.0, ((x >> 8) & 255) / 255.0, (x & 255) / 255.0 };

        static double Luma(double r, double g, double b) => 0.2126 * r + 0.7152 * g + 0.0722 * b;

        static double HueOf(uint hex)
        {
            double[] c = Hex(hex);
            M.Hsv(c[0], c[1], c[2], out double h, out _, out _);
            return h;
        }

        [Test]
        public void TheDangerReach_TakesTheRose_AndNoOtherAccent()
        {
            M.Table t = Table();
            Assert.That(t, Is.Not.Null);
            double m = UnityTuning.Colours72DangerMarginDeg;
            Assert.That(M.InDangerReach(t, HueOf(0xE67793), m), Is.True, "the spire's rose accent (colour56): hue " + HueOf(0xE67793));
            Assert.That(M.InDangerReach(t, 345, m), Is.True, "the band's own edge");
            Assert.That(M.InDangerReach(t, 11, m), Is.True, "the danger token's hue");
            Assert.That(M.InDangerReach(t, 330, m), Is.True, "the margin's near edge");
            Assert.That(M.InDangerReach(t, 328, m), Is.False, "past the margin");
            Assert.That(M.InDangerReach(t, 49, m), Is.True);
            Assert.That(M.InDangerReach(t, 52, m), Is.False);
            foreach (string level in new[] { "foundry", "iceline", "kiln", "span" })
            {
                uint hex = (uint)GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json")).Num("design.n56.colour56.numbers.levels." + level + ".accentHex");
                Assert.That(M.InDangerReach(t, HueOf(hex), m), Is.False, level + "'s colour56 accent (hue " + HueOf(hex).ToString("0") + ") wears no danger");
            }
            Assert.That(M.InDangerReach(t, 186, m), Is.False, "usable cyan");
            Assert.That(M.InDangerReach(t, 311, m), Is.False, "RIVAL's magenta");
        }

        [Test]
        public void WorldAt_MovesTheHue_KeepsTheLuminance_AndWearsNoRole()
        {
            M.Table t = Table();
            double[] rose = Hex(0xCF6B84), dark = Hex(0x5D303B);
            foreach (double[] c in new[] { rose, dark })
            {
                M.WorldAt(t, c[0], c[1], c[2], UnityTuning.Colours72ExitHueDeg, out double r, out double g, out double b);
                M.Hsv(r, g, b, out double h, out double s, out _);
                Assert.That(h, Is.EqualTo(UnityTuning.Colours72ExitHueDeg).Within(0.5), "the hue moved to the exit hue");
                Assert.That(s, Is.EqualTo(t.worldSat).Within(1e-6), "the saturation at the world tier's");
                Assert.That(Luma(r, g, b), Is.EqualTo(Luma(c[0], c[1], c[2])).Within(1e-9), "the luminance kept");
                string role = M.RoleOf(t, r, g, b);
                Assert.That(role, Is.Not.EqualTo("danger").And.Not.EqualTo("parry").And.Not.EqualTo("player").And.Not.EqualTo("rival"), "the result wears " + role);
                Assert.That(role, Is.Not.EqualTo("boundary"), "never the boundary's pale violet-white");
                Assert.That(M.Routes(t, r, g, b), Is.False, "a second scan would not route it again");
            }
        }

        [Test]
        public void WorldAt_KeepsALowerSaturation_AndHdr()
        {
            M.Table t = Table();
            M.WorldAt(t, 0.40, 0.36, 0.34, 40, out double r, out double g, out double b);
            M.Hsv(r, g, b, out _, out double s, out _);
            Assert.That(s, Is.EqualTo(0.15).Within(0.01), "S 0.15 is under the tier: kept (not raised to 0.25)");
            M.WorldAt(t, 1.7, 0.6, 0.8, 40, out double hr, out double hg, out double hb);
            Assert.That(Luma(hr, hg, hb), Is.EqualTo(Luma(1.7, 0.6, 0.8)).Within(1e-9), "an HDR colour keeps its luminance");
        }
    }
}
