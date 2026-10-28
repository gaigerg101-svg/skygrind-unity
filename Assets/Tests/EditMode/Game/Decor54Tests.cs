// NIGHT54 builder D (design.n54.decor54, Decor54.cs): the colour scripts as data. Pinned:
//   1. every campaign level (foundry, iceline, kiln, span, spire, finale) has a script, read through its level id (KeyOf strips "nNN-");
//   2. no accent is cyan, red-orange, magenta or the boundary: a saturated accent keeps numbers.hueGapMinDeg from every role token
//      (UnityTuning.Look49*); a pale one (s < Decor54PaleSat) is told from them by saturation and is never in the boundary's violet hue band;
//   3. the six accents differ from each other (hue >= 20 deg apart, or saturation >= 0.2 apart);
//   4. each level's walls have value structure: the upper tone lighter than the base (x1.8 at least, in value);
//   5. off = NIGHT53: Fixtures33's hook returns every fix's colour unchanged with the row off; on, only a trim in cyan / level49 / accentN moves.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Decor54Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg() => cfg ??= GameConfig.Load(Path.Combine(PortRoot(), "config.json"));

        static string PortRoot()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        static readonly string[] Levels = { "foundry", "iceline", "kiln", "span", "spire", "finale" };
        const string P = "design.n54.decor54.numbers.levels.";

        static void Hsv(uint hex, out float h, out float s, out float v)
        {
            Color.RGBToHSV(Look.Hex(hex), out h, out s, out v);
            h *= 360f;
        }

        static float Gap(float a, float b)
        {
            float d = Mathf.Abs(a - b) % 360f;
            return d > 180f ? 360f - d : d;
        }

        [Test]
        public void EveryLevel_HasAScript_ReadByItsId()
        {
            Assert.That(Decor54.KeyOf("n52-foundry"), Is.EqualTo("foundry"));
            Assert.That(Decor54.KeyOf("n38-finale"), Is.EqualTo("finale"));
            Assert.That(Decor54.KeyOf("survival"), Is.EqualTo("survival"));
            Assert.That(Decor54.KeyOf(""), Is.EqualTo(""));
            foreach (string l in Levels)
                foreach (string k in new[] { "accentHex", "wallBaseHex", "wallUpperHex" })
                    Assert.That(Cfg().Has(P + l + "." + k), Is.True, l + "." + k);
        }

        [Test]
        public void NoAccent_IsARoleColour()
        {
            uint[] roles = { UnityTuning.Look49UsableHex, UnityTuning.Look49DangerHex, UnityTuning.Look49ParryHex, UnityTuning.Look49RivalHex, UnityTuning.Look49BoundaryHex };
            float minGap = (float)Cfg().Num("design.n54.decor54.numbers.hueGapMinDeg");
            Hsv(UnityTuning.Look49BoundaryHex, out float bh, out _, out _);
            foreach (string l in Levels)
            {
                Hsv((uint)Cfg().Num(P + l + ".accentHex"), out float h, out float s, out _);
                if (s < UnityTuning.Decor54PaleSat)
                {
                    Assert.That(Gap(h, bh), Is.GreaterThan(UnityTuning.Decor54CyanBandDeg), l + ": a pale accent is never in the boundary's violet band");
                    continue;
                }
                foreach (uint r in roles)
                {
                    Hsv(r, out float rh, out _, out _);
                    Assert.That(Gap(h, rh), Is.GreaterThanOrEqualTo(minGap), $"{l}: hue {h:0} vs role #{r:x6} hue {rh:0}");
                }
            }
        }

        [Test]
        public void TheAccents_DifferFromEachOther()
        {
            for (int i = 0; i < Levels.Length; i++)
                for (int j = i + 1; j < Levels.Length; j++)
                {
                    Hsv((uint)Cfg().Num(P + Levels[i] + ".accentHex"), out float hi, out float si, out _);
                    Hsv((uint)Cfg().Num(P + Levels[j] + ".accentHex"), out float hj, out float sj, out _);
                    Assert.That(Gap(hi, hj) >= 20f || Mathf.Abs(si - sj) >= 0.2f, Is.True, $"{Levels[i]} vs {Levels[j]}: hue {hi:0} / {hj:0}, s {si:0.00} / {sj:0.00}");
                }
        }

        [Test]
        public void TheWalls_HaveValueStructure()
        {
            foreach (string l in Levels)
            {
                Hsv((uint)Cfg().Num(P + l + ".wallBaseHex"), out _, out _, out float vb);
                Hsv((uint)Cfg().Num(P + l + ".wallUpperHex"), out _, out _, out float vu);
                Assert.That(vu, Is.GreaterThanOrEqualTo(vb * 1.8f), $"{l}: the upper band {vu:0.00} lighter than the base {vb:0.00}");
            }
            Assert.That(Cfg().Num("design.n54.decor54.numbers.toneMix"), Is.InRange(0.0, 1.0));
            Assert.That(Cfg().Num("design.n54.decor54.numbers.accentScale"), Is.InRange(0.0, 1.0), "a moved piece is never brighter than it was");
            Assert.That(Cfg().Num("design.n54.decor54.numbers.usableCap"), Is.InRange(0.0, 1.0), "nor brighter than the usable cyan");
        }

        [Test]
        public void TheFixtureHook_OffIsNight53_OnMovesOnlyTrims()
        {
            string[] cols = { "cyan", "level49", "accent0", "accent3", "white", "orange", "warm", "amber" };
            string[] kinds = { "trim", "lens", "guideLamp", "roofStrip", "panel" };
            foreach (string k in kinds)
                foreach (string c in cols)
                {
                    var f = new LightPlan33.Fix { kind = k, color = c };
                    Assert.That(Decor54.FixColour(f, c, false), Is.EqualTo(c), $"off: {k} {c} unchanged");
                    bool moves = k == "trim" && (c == "cyan" || c == "level49" || c.StartsWith("accent"));
                    if (k != "lens") Assert.That(Decor54.FixColour(f, c, true), Is.EqualTo(moves ? Decor54.FixKey : c), $"on: {k} {c}"); // a lens moves by lights49's read of the map (not in a unit test)
                }
        }
    }
}
