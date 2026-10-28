// NIGHT41 K2 (design.n41.look41; INVENTORY A3; DECIDE41 D-4): the room light's colour by theme (UnityTuning.Look41.cs, Look41.cs).
// - one key and one fill per LightPlan33 theme;
// - no room light reads as a reserved colour (CLAUDE.md rule 8): every key and fill is clear of cyan (colors.usable), red-orange (colors.enemy)
//   and magenta (RIVAL) by Look41.ClearOf (hue gap >= Look41MinHueGap or near-white);
// - the theme words: an n41 level id finds its levelFrom's theme ("n41-kiln" -> kiln), and level 2's key is bluer than the warm white;
// - look41 off (theme -1) gives NIGHT40's colours exactly.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Look41Tests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));
        static GameConfig cfg;
        static GameConfig Cfg() => cfg ??= GameConfig.Load(Path.Combine(Root, "config.json"));

        static Dictionary<string, object> Named(string name) => new Dictionary<string, object> { { "name", name } };

        [Test]
        public void EveryTheme_HasAKeyAndAFill()
        {
            Assert.AreEqual(LightPlan33.Themes.Length, UnityTuning.Look41KeyHex.Length);
            Assert.AreEqual(LightPlan33.Themes.Length, UnityTuning.Look41FillHex.Length);
        }

        [Test]
        public void NoRoomLight_IsCyanRedOrangeOrMagenta()
        {
            uint usable = (uint)Cfg().Num("colors.usable"), enemy = (uint)Cfg().Num("colors.enemy"), magenta = 0xff00ff; // RIVAL's magenta (CLAUDE.md rule 8)
            for (int t = 0; t < LightPlan33.Themes.Length; t++)
                foreach (uint c in new[] { UnityTuning.Look41KeyHex[t], UnityTuning.Look41FillHex[t] })
                {
                    Assert.IsTrue(Look41.ClearOf(c, usable), $"{LightPlan33.Themes[t]} {c:x6} reads cyan");
                    Assert.IsTrue(Look41.ClearOf(c, enemy), $"{LightPlan33.Themes[t]} {c:x6} reads red-orange");
                    Assert.IsTrue(Look41.ClearOf(c, magenta), $"{LightPlan33.Themes[t]} {c:x6} reads magenta");
                }
        }

        [Test]
        public void TheThemedSuns_AreClearOfTheReservedColours_AndFoundryKeepsTheConfigSun()
        {
            uint usable = (uint)Cfg().Num("colors.usable"), enemy = (uint)Cfg().Num("colors.enemy"), magenta = 0xff00ff;
            Assert.AreEqual(LightPlan33.Themes.Length, UnityTuning.Look41SunHex.Length);
            Assert.AreEqual(0u, Look41.SunHex(0), "foundry keeps colors.lightSun");
            Assert.AreEqual(0u, Look41.SunHex(-1), "look41 off keeps colors.lightSun");
            for (int t = 1; t < LightPlan33.Themes.Length; t++)
            {
                uint c = UnityTuning.Look41SunHex[t];
                Assert.IsTrue(Look41.ClearOf(c, usable) && Look41.ClearOf(c, enemy) && Look41.ClearOf(c, magenta), $"{LightPlan33.Themes[t]} sun {c:x6}");
            }
            Assert.Greater(Look41.SunHex(1) & 255, (Look41.SunHex(1) >> 16) & 255, "iceline's sun has more blue than red");
        }

        [Test]
        public void Planted_CyanAndRedOrange_AreCaught()
        {
            // L-034: the check catches a planted error
            Assert.IsFalse(Look41.ClearOf(0x40e0f0, (uint)Cfg().Num("colors.usable")));
            Assert.IsFalse(Look41.ClearOf(0xff6030, (uint)Cfg().Num("colors.enemy")));
        }

        [Test]
        public void TheThemeWords_FindEveryCampaignLevel()
        {
            Assert.AreEqual(0, LightPlan33.Theme(Named("n38-foundry")));
            Assert.AreEqual(1, LightPlan33.Theme(Named("n40-iceline")));
            Assert.AreEqual(2, LightPlan33.Theme(Named("n41-kiln")));
            Assert.AreEqual(3, LightPlan33.Theme(Named("n40-span")));
            Assert.AreEqual(4, LightPlan33.Theme(Named("n41-spire")));
            Assert.AreEqual(0, LightPlan33.Theme(Named("n41-foundry")));
        }

        [Test]
        public void Iceline_KeyIsBlueAndKiln_KeyIsWarmer_ThanTheWarmWhite()
        {
            uint warm = UnityTuning.Look33PanelWarmHex, ice = Look41.KeyHex(1), kiln = Look41.KeyHex(2);
            Assert.Greater(ice & 255, warm & 255, "iceline's key has more blue");
            Assert.Less(ice >> 16, warm >> 16, "iceline's key has less red");
            Assert.Less(kiln & 255, warm & 255, "kiln's key has less blue");
        }

        [Test]
        public void Off_GivesNight40sColours()
        {
            Assert.AreEqual(UnityTuning.Look33PanelWarmHex, Look41.KeyHex(-1));
            Assert.AreEqual(UnityTuning.Look35FillHex, Look41.FillHex(-1));
            Assert.AreEqual(UnityTuning.Look33PanelWarmHex, Look41.KeyHex(0), "foundry keeps the warm white");
        }
    }
}
