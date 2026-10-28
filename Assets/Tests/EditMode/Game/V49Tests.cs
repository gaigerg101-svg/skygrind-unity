// NIGHT49 builder V (design.n49.look49; UnityTuning.V49.cs): the colour script's rules, pinned.
// - one row per LightPlan33 theme in every table;
// - each level hue sits >= Look49AccentMinHueGapDeg (20) from every role token of UnityTuning.Look49.cs (rule 8), and the five level hues are
//   at least 25 deg from each other (each level its own identity);
// - every environment hex is within 8 deg of its level's hue (the dark ones round coarsely) and under the roles' saturation (<= 0.7, round 2: the rims 0.65; the
//   saturated roles are 0.77-0.89);
// - the config's design.n49.look49.numbers mirror UnityTuning.V49 (the code reads UnityTuning; the row is the data copy).
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class V49Tests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));
        static GameConfig cfg;
        static GameConfig Cfg() => cfg ??= GameConfig.Load(Path.Combine(Root, "config.json"));

        static readonly uint[] Roles = { UnityTuning.Look49UsableHex, UnityTuning.Look49DangerHex, UnityTuning.Look49ParryHex, UnityTuning.Look49RivalHex, UnityTuning.Look49BoundaryHex };

        static void Hsv(uint hex, out float h, out float s, out float v)
        {
            Color.RGBToHSV(Look.Hex(hex), out h, out s, out v);
            h *= 360;
        }

        static double Gap(double a, double b)
        {
            double d = Math.Abs(a - b) % 360;
            return d > 180 ? 360 - d : d;
        }

        static uint[][] Tables() => new[]
        {
            UnityTuning.V49FogHex, UnityTuning.V49StoneFogZenithHex, UnityTuning.V49StoneFogMidHex, UnityTuning.V49StoneFogHorizonHex,
            UnityTuning.V49AmbientSkyHex, UnityTuning.V49AmbientEquatorHex, UnityTuning.V49AmbientGroundHex, UnityTuning.V49RimHex, UnityTuning.V49GradeTintHex,
        };

        [Test]
        public void EveryTable_HasOneRowPerTheme()
        {
            int n = LightPlan33.Themes.Length;
            Assert.AreEqual(n, UnityTuning.V49HueDeg.Length);
            foreach (uint[] t in Tables()) Assert.AreEqual(n, t.Length);
            Assert.AreEqual(n, UnityTuning.V49FogDensity.Length);
            Assert.AreEqual(n, UnityTuning.V49Contrast.Length);
            Assert.AreEqual(n, UnityTuning.V49ExposureEv.Length);
            Assert.AreEqual(n, UnityTuning.V49Saturation.Length);
        }

        [Test]
        public void LevelHues_ClearOfEveryRole_AndOfEachOther()
        {
            for (int t = 0; t < UnityTuning.V49HueDeg.Length; t++)
            {
                foreach (uint r in Roles)
                {
                    Hsv(r, out float rh, out _, out _);
                    Assert.GreaterOrEqual(Gap(UnityTuning.V49HueDeg[t], rh), UnityTuning.Look49AccentMinHueGapDeg, $"{LightPlan33.Themes[t]} hue {UnityTuning.V49HueDeg[t]} vs role {r:x6} ({rh:0})");
                }
                for (int u = t + 1; u < UnityTuning.V49HueDeg.Length; u++)
                    Assert.GreaterOrEqual(Gap(UnityTuning.V49HueDeg[t], UnityTuning.V49HueDeg[u]), 25, $"{LightPlan33.Themes[t]} vs {LightPlan33.Themes[u]}");
            }
        }

        [Test]
        public void EveryEnvironmentColour_InItsLevelHue_UnderTheRolesSaturation()
        {
            foreach (uint[] table in Tables())
                for (int t = 0; t < table.Length; t++)
                {
                    Hsv(table[t], out float h, out float s, out _);
                    Assert.LessOrEqual(Gap(h, UnityTuning.V49HueDeg[t]), 8, $"{LightPlan33.Themes[t]} {table[t]:x6} hue {h:0}");
                    Assert.LessOrEqual(s, 0.7f, $"{LightPlan33.Themes[t]} {table[t]:x6} saturation {s:0.00}");
                }
        }

        [Test]
        public void ConfigNumbers_MirrorUnityTuning()
        {
            GameConfig c = Cfg();
            const string p = "design.n49.look49.numbers.";
            Assert.AreEqual(UnityTuning.V49HueDeg, c.Nums(p + "hueDeg"));
            Assert.AreEqual(UnityTuning.V49FogDensity, c.Nums(p + "fogDensity"));
            Assert.AreEqual(UnityTuning.V49Contrast, c.Nums(p + "contrast"));
            Assert.AreEqual(UnityTuning.V49ExposureEv, c.Nums(p + "exposureEv"));
            Assert.AreEqual(UnityTuning.V49Saturation, c.Nums(p + "saturation"));
            Assert.AreEqual(UnityTuning.V49ShadowTint, c.Num(p + "shadowTint"), 1e-6);
            Assert.AreEqual(UnityTuning.V49MidTint, c.Num(p + "midTint"), 1e-6);
            Assert.AreEqual(UnityTuning.V49ShadowOffset, c.Num(p + "shadowOffset"), 1e-6);
            Assert.AreEqual(UnityTuning.V49RimScale, c.Num(p + "rimScale"), 1e-6);
            Assert.AreEqual(UnityTuning.V49Bloom, c.Num(p + "bloom"), 1e-9);
            // NIGHT50 freeze (DECIDE50 D-12): look49 is ON on the listed levels; its enabled_why says so
            Assert.That(c.Design("n49.look49.enabled"), Is.True, "look49 is on at the NIGHT50 freeze (levels listed in numbers.levels)");
            Assert.That(c.Get("design.n49.look49.enabled_why") as string, Does.StartWith("ON"), "look49's enabled_why starts ON");
        }
    }
}
