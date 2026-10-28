// NIGHT50 builder V (G3: look49 per level; UnityTuning.V50.cs, PostFx's gate). Pinned:
// - the gate: an empty design.n49.look49.numbers.levels (the shared commit) turns the grade off on every level, with the row on or off; a listed
//   level id (exact, case-insensitive) with the row on turns it on for that level only; the reel's force wins both ways;
// - a level's id is LevelDef.From's (the json's id, else its name in lower case);
// - V50's slots name iceline and span; each V50 table has one entry per slot; the level hue of each slot is >= Look49AccentMinHueGapDeg from every
//   role token; the tint stays in the level hue and is less saturated than V49's; the saturation lift is lower than V49's; the sun scale darkens.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class V50Tests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));
        static GameConfig Fresh() => GameConfig.Load(Path.Combine(Root, "config.json")); // a fresh config per test: the gate tests change it in memory

        static readonly uint[] Roles = { UnityTuning.Look49UsableHex, UnityTuning.Look49DangerHex, UnityTuning.Look49ParryHex, UnityTuning.Look49RivalHex, UnityTuning.Look49BoundaryHex };

        static void Hsv(uint hex, out float h, out float s)
        {
            Color.RGBToHSV(Look.Hex(hex), out h, out s, out _);
            h *= 360;
        }

        static double Gap(double a, double b)
        {
            double d = Math.Abs(a - b) % 360;
            return d > 180 ? 360 - d : d;
        }

        static Dictionary<string, object> Map(string name) => new Dictionary<string, object> { { "name", name } };

        /// <summary>The row on and the list set, in this config only (Obj and Get return the config's own objects).</summary>
        static void Turn(GameConfig c, bool row, params string[] levels)
        {
            c.Obj("design.n49.look49")["enabled"] = row;
            var list = (List<object>)c.Get("design.n49.look49.numbers.levels");
            list.Clear();
            foreach (string l in levels) list.Add(l);
        }

        static void Clean(Action body)
        {
            bool? force = PostFx.Look49Force;
            int off = NightRows.ForceOffFrom;
            try
            {
                PostFx.Look49Force = null;
                NightRows.ForceOffFrom = 53; // NIGHT53 (n53-v50pin.cjs): polish53's iceline grade off: these pins are NIGHT49 / NIGHT52's lists (N53PolishTests pins polish53's)
                body();
            }
            finally
            {
                PostFx.Look49Force = force;
                NightRows.ForceOffFrom = off;
            }
        }

        [Test]
        public void LevelId_IsLevelDefs()
        {
            Assert.AreEqual("n48-foundry", PostFx.LevelIdOf(Map("n48-foundry")));
            Assert.AreEqual("n47-spire", PostFx.LevelIdOf(Map("N47-Spire")));
            Assert.AreEqual("lvl", PostFx.LevelIdOf(new Dictionary<string, object> { { "id", "lvl" }, { "name", "other" } }));
            Assert.AreEqual("", PostFx.LevelIdOf(null));
            var lv = LevelDef.From(new Dictionary<string, object> { { "name", "N47-Kiln" }, { "campaign", new Dictionary<string, object> { { "rooms", new List<object>() }, { "doors", new List<object>() } } } });
            Assert.AreEqual(lv.id, PostFx.LevelIdOf(Map("N47-Kiln")), "the same id LevelDef.From gives");
        }

        [Test]
        public void EmptyList_IsOffOnEveryLevel() => Clean(() =>
        {
            Assert.That(PostFx.Look49LevelsOf(Fresh()), Is.EquivalentTo(new[] { "n48-foundry", "n47-spire", "n52-foundry", "n52-spire" }), "the freeze's list (DECIDE50 D-4) + NIGHT52's copies of those two levels (n52-shared.cjs, DECIDE52 D-7)");
            foreach (bool row in new[] { false, true })
            {
                GameConfig c = Fresh(); // GameConfig caches a path once read: one config per setting
                Turn(c, row);
                foreach (string id in new[] { "n48-foundry", "n47-iceline", "n47-kiln", "n47-span", "n47-spire" })
                    Assert.That(PostFx.Look49OnFor(c, Map(id)), Is.False, $"row {row}, empty list: {id} off");
            }
        });

        [Test]
        public void ListedLevel_OnOnlyThere_AndOnlyWithTheRow() => Clean(() =>
        {
            GameConfig c = Fresh();
            Turn(c, true, "n48-foundry", "N47-Kiln");
            CollectionAssert.AreEqual(new[] { "n48-foundry", "n47-kiln" }, PostFx.Look49LevelsOf(c), "lower case");
            Assert.That(PostFx.Look49OnFor(c, Map("n48-foundry")), Is.True);
            Assert.That(PostFx.Look49OnFor(c, Map("n47-kiln")), Is.True, "case-insensitive");
            Assert.That(PostFx.Look49OnFor(c, Map("n47-foundry")), Is.False, "exact: an older foundry is not the listed one");
            Assert.That(PostFx.Look49OnFor(c, Map("n47-iceline")), Is.False);
            Assert.That(PostFx.Look49OnFor(c, Map("survival")), Is.False);
            GameConfig off = Fresh(); // GameConfig caches a path once read: one config per setting
            Turn(off, false, "n48-foundry");
            Assert.That(PostFx.Look49OnFor(off, Map("n48-foundry")), Is.False, "row off: listed or not, off");
            NightRows.ForceOffFrom = 49; // --off49: the row is NIGHT49's
            Assert.That(PostFx.Look49OnFor(c, Map("n48-foundry")), Is.False, "--off49: off");
        });

        [Test]
        public void ReelForce_WinsOverTheList() => Clean(() =>
        {
            GameConfig unlisted = Fresh();
            Turn(unlisted, false);
            PostFx.Look49Force = true;
            Assert.That(PostFx.Look49OnFor(unlisted, Map("n47-iceline")), Is.True, "forced on: an unlisted level, the row off");
            GameConfig listed = Fresh();
            Turn(listed, true, "n47-iceline");
            PostFx.Look49Force = false;
            Assert.That(PostFx.Look49OnFor(listed, Map("n47-iceline")), Is.False, "forced off: a listed level, the row on");
            PostFx.Look49Force = null;
            Assert.That(PostFx.Look49OnFor(listed, Map("n47-iceline")), Is.True, "no force: the config decides");
        });

        [Test]
        public void V50Slots_AreIcelineAndSpan_OneEntryEach()
        {
            CollectionAssert.AreEqual(new[] { "iceline", "span" }, Array.ConvertAll(UnityTuning.V50Themes, t => LightPlan33.Themes[t]));
            int n = UnityTuning.V50Themes.Length;
            Assert.AreEqual(n, UnityTuning.V50GradeTintHex.Length);
            Assert.AreEqual(n, UnityTuning.V50Contrast.Length);
            Assert.AreEqual(n, UnityTuning.V50ExposureEv.Length);
            Assert.AreEqual(n, UnityTuning.V50Saturation.Length);
            Assert.AreEqual(n, UnityTuning.V50SunScale.Length);
            Assert.AreEqual(0, PostFx.V50Slot(1));
            Assert.AreEqual(1, PostFx.V50Slot(3));
            foreach (int t in new[] { 0, 2, 4, -1 }) Assert.AreEqual(-1, PostFx.V50Slot(t), "foundry, kiln, spire keep V49");
        }

        [Test]
        public void V50_TheRecipe_LowerLift_WeakerTint_DarkerSun_HueClearOfRoles()
        {
            for (int s = 0; s < UnityTuning.V50Themes.Length; s++)
            {
                int t = UnityTuning.V50Themes[s];
                string lv = LightPlan33.Themes[t];
                double hue = UnityTuning.V49HueDeg[t];
                foreach (uint r in Roles)
                {
                    Hsv(r, out float rh, out _);
                    Assert.GreaterOrEqual(Gap(hue, rh), UnityTuning.Look49AccentMinHueGapDeg, $"{lv} hue {hue} vs role {r:x6}");
                }
                Hsv(UnityTuning.V50GradeTintHex[s], out float th, out float ts);
                Hsv(UnityTuning.V49GradeTintHex[t], out _, out float ts49);
                Assert.LessOrEqual(Gap(th, hue), 8, $"{lv} tint hue {th:0}");
                Assert.Less(ts, ts49, $"{lv}: a weaker tint than V49's");
                Assert.Less(UnityTuning.V50Saturation[s], UnityTuning.V49Saturation[t], $"{lv}: a lower saturation lift than V49's");
                Assert.That(UnityTuning.V50SunScale[s], Is.GreaterThan(0f).And.LessThan(1f), $"{lv}: the sun darkens, never off");
                Assert.That(UnityTuning.V50Contrast[s], Is.InRange(-100.0, 100.0), $"{lv}: URP's contrast range");
            }
        }
    }
}
