// NIGHT18 U3-31.7 (builder N): the menus' motion rules, measured the way the reel measures them (Tools/feel-schema.md M9 and
// Tools/feel-metrics.py): an element's first visible frame is a pop-in at 95-105 % of its rest scale AND >= 95 % of its rest
// alpha AND < 40 px from rest; an entrance has settled once it stays within 3 % of rest scale, >= 95 % of rest alpha and
// 3 px of rest for 6 frames, and it must settle in 90-300 ms. Build 1's reel failed SETTINGS (12 pop-ins), PAUSED (3),
// TITLE (1) and RESULTS (the grade settled in 67 ms, the card foot in 83 ms, the full-screen flash a pop-in).
// Also the words: a hint's ASCII key "[E]" becomes a drawn chip; FOV KICK / CAMERA SHAKE up front on ACCESSIBILITY.
// No scene: pure functions only.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class MenuMotion18Tests
    {
        const double E = 1e-5;
        static readonly double[] Rates = { 30.0, 60.0, 120.0, 144.0, 240.0 };

        /// <summary>
        /// The reel's settle at 60 fps: the views step their clocks before they draw, so frame f (0 = the first drawn) shows
        /// age (f + 1) / 60. The first frame from which `ok(age)` holds for 6 frames; the time since the first frame (s).
        /// </summary>
        static double SettleAge(Func<double, bool> ok, double horizon = 1.0)
        {
            const double dt = 1 / 60.0;
            for (int f = 0; f * dt <= horizon; f++)
            {
                bool all = true;
                for (int k = 0; k < 6 && all; k++) all = ok((f + k + 1) * dt);
                if (all) return f * dt;
            }
            return double.PositiveInfinity;
        }

        // --- a screen's parts (head, run line, tabs, footer, BACK) ---------------------------------------------------------

        [Test]
        public void APartsFirstFrameIsNeverAPopInAndIsVisible()
        {
            foreach (double fps in Rates)
            {
                double age = 1 / fps;
                float a = MenuMotion18.Alpha(age);
                float px = MenuMotion18.Offset(age).magnitude;
                Assert.IsFalse(MenuMotion18.PopIn(1f, a, px), $"{fps} fps: alpha {a}, {px} px");
                Assert.Greater(a, 0.01f, $"{fps} fps: visible on its first frame (no added latency)");
            }
        }

        [Test]
        public void APartSettlesIn90To300Ms()
        {
            double s = SettleAge(t => MenuMotion18.Alpha(t) >= 0.95f && MenuMotion18.Offset(t).magnitude <= 3f);
            Assert.GreaterOrEqual(s, 0.09, "settled");
            Assert.LessOrEqual(s, 0.30, "settled");
            Assert.AreEqual(1f, MenuMotion18.Alpha(10), E);
            Assert.AreEqual(0f, MenuMotion18.Offset(10).magnitude, E, "at rest");
            Assert.AreEqual(0f, MenuMotion18.Alpha(0), E, "nothing before its first frame");
        }

        [Test]
        public void APartComesAlongTheSlantAndOvershootsAboutTenPercent()
        {
            Vector2 v = MenuMotion18.Offset(0);
            Assert.Less(v.x, 0f, "from the left");
            Assert.Less(v.y, 0f, "and below");
            Assert.AreEqual(Mathf.Tan(UnityTuning.ThemeSlantDeg * Mathf.Deg2Rad), v.y / v.x, 1e-4, "on the 12 deg slant");
            Assert.AreEqual(-UnityTuning.ThemeEnterDistance, v.x, 1e-3, "as the items: the distance along x, the slant's rise on y");
            float max = 0;
            for (int i = 0; i <= 300; i++) max = Math.Max(max, MenuMotion18.Enter(UnityTuning.ThemeInTime * i / 300.0));
            Assert.GreaterOrEqual(max, 1.05f, "an overshoot that shows");
            Assert.LessOrEqual(max, 1.15f, "not a bounce");
        }

        [Test]
        public void AScreenIsWholeByAbout200Ms()
        {
            double last = -1;
            for (int i = 0; i < 24; i++)
            {
                double w = MenuMotion18.Wait(i, true);
                Assert.GreaterOrEqual(w, last, "in order");
                Assert.LessOrEqual(w + UnityTuning.ThemeInTime, 0.2 + 1e-9, $"item {i}: in by 200 ms (round 2: 'still building at 250 ms')");
                Assert.AreEqual(i * UnityTuning.MenuEnterStagger, MenuMotion18.Wait(i, false), E, "NIGHT17's stagger");
                last = w;
            }
            Assert.AreEqual(0, MenuMotion18.Wait(0, true), E, "the first at once");
            Assert.Greater(MenuMotion18.Wait(1, true), 0, "still a sweep, not a block");
        }

        [Test]
        public void ATabsRowsCascadeQuicklyEachAVisibleSlide()
        {
            double gap = MenuMotion18.RowWait(1) - MenuMotion18.RowWait(0);
            Assert.AreEqual(0, MenuMotion18.RowWait(0), E);
            Assert.GreaterOrEqual(gap, 0.02 - E, "one at a time (round 3: 'the whole list at once')");
            Assert.LessOrEqual(gap, 0.025 + E);
            double settle = SettleAge(t => Mathf.Clamp01(MenuMotion18.RowEnter(t)) >= 0.95f
                && Math.Abs(1f - MenuMotion18.RowEnter(t)) * UnityTuning.ThemeEnterDistance <= 3f);
            Assert.GreaterOrEqual(settle, 0.09, "each row's entrance is M9's 90 ms or more");
            Assert.LessOrEqual(MenuMotion18.RowWait(8) + settle, 0.30, "a 9-row tab in by ~250-300 ms (NIGHT17: ~420)");
            foreach (double fps in Rates)
            {
                float e = MenuMotion18.RowEnter(1 / fps);
                Assert.IsFalse(MenuMotion18.PopIn(1f, Mathf.Clamp01(e), (1 - e) * UnityTuning.ThemeEnterDistance), $"{fps} fps");
            }
            Assert.AreEqual(0f, MenuMotion18.RowEnter(0), E, "from nothing");
        }

        [Test]
        public void ASlabSweepsInFirstThenItsItemsCascade()
        {
            Assert.GreaterOrEqual(MenuMotion18.SlabWait(0), 0.05, "the slab leads");
            Assert.Greater(MenuMotion18.SlabWait(1), MenuMotion18.SlabWait(0), "then one at a time");
            Assert.LessOrEqual(MenuMotion18.SlabWait(5) + UnityTuning.ThemeInTime, 0.35, "PAUSED's column in by ~300 ms");
            Vector2 v = MenuMotion18.SlabOffset(0);
            Assert.GreaterOrEqual(-v.x, 300f, "a sweep, not NIGHT17's 140 px");
            Assert.AreEqual(Mathf.Tan(UnityTuning.ThemeSlantDeg * Mathf.Deg2Rad), v.y / v.x, 1e-4, "along the slant");
            Assert.GreaterOrEqual(MenuMotion18.SlabOffset(1 / 60.0).magnitude, 40f, "moving on its first frame");
            Assert.AreEqual(0f, MenuMotion18.SlabOffset(UnityTuning.MenuSlabIn18).magnitude, 1e-3, "at rest");
            Assert.AreEqual(UnityTuning.DraftCardEnter18 / UnityTuning.ThemeEnterDistance * MenuMotion18.Offset(0.05).x,
                MenuMotion18.Offset(0.05, UnityTuning.DraftCardEnter18).x, 1e-3, "MODS' cards: the same curve from farther");
        }

        [Test]
        public void ThePopInRuleIsTheReels()
        {
            Assert.IsTrue(MenuMotion18.PopIn(1f, 1f, 0f), "whole on its first frame");
            Assert.IsTrue(MenuMotion18.PopIn(0.96f, 0.95f, 39f), "the band's edges");
            Assert.IsFalse(MenuMotion18.PopIn(0.94f, 1f, 0f), "smaller");
            Assert.IsFalse(MenuMotion18.PopIn(1.06f, 1f, 0f), "a slam from above is motion");
            Assert.IsFalse(MenuMotion18.PopIn(1f, 0.94f, 0f), "fading in");
            Assert.IsFalse(MenuMotion18.PopIn(1f, 1f, 40f), "a 40 px slide");
        }

        // --- a screen's sheet -----------------------------------------------------------------------------------------------

        [Test]
        public void ASheetFadesInFromTheSheetBeforeIt()
        {
            float rest = UnityTuning.MenuSheetAlpha;
            foreach (float before in new[] { 0f, UnityTuning.TitleSheetAlpha })
            {
                float from = MenuMotion18.SheetFrom(before, rest);
                Assert.AreEqual(before, from, E, "no flash of the world between two screens");
                Assert.AreEqual(from, MenuMotion18.SheetAlpha(0, from, rest), E);
                Assert.AreEqual(rest, MenuMotion18.SheetAlpha(UnityTuning.MenuSheetFade18, from, rest), E);
                Assert.AreEqual(rest, MenuMotion18.SheetAlpha(10, from, rest), E);
                foreach (double fps in Rates)
                {
                    float a = MenuMotion18.SheetAlpha(1 / fps, from, rest);
                    Assert.IsFalse(MenuMotion18.PopIn(1f, a / rest, 0f), $"from {before} at {fps} fps: {a}");
                }
                double s = SettleAge(t => MenuMotion18.SheetAlpha(t, from, rest) >= 0.95f * rest);
                Assert.GreaterOrEqual(s, 0.09, $"from {before}");
                Assert.LessOrEqual(s, 0.30, $"from {before}");
            }
            Assert.AreEqual(rest, MenuMotion18.SheetFrom(1f, rest), E, "never darker than its own sheet");
        }

        // --- the focus -----------------------------------------------------------------------------------------------------

        [Test]
        public void TheFocusSlidesAlongTheSlantWithAnOvershoot()
        {
            float n = UnityTuning.MenuFocusNudge18;
            Assert.AreEqual(Vector2.zero, MenuMotion18.FocusNudge(0, n));
            Vector2 end = MenuMotion18.FocusNudge(1, n);
            Assert.AreEqual(n, end.x, E);
            Assert.AreEqual(n * Mathf.Tan(UnityTuning.ThemeSlantDeg * Mathf.Deg2Rad), end.y, 1e-4, "right and 12 deg up");
            float max = 0;
            for (int i = 0; i <= 100; i++) max = Math.Max(max, MenuMotion18.FocusNudge(i / 100f, n).x);
            Assert.GreaterOrEqual(max, 1.05f * n);
            Assert.LessOrEqual(max, 1.15f * n);
            Assert.GreaterOrEqual(UnityTuning.MenuFocusTime18, 0.09);
            Assert.LessOrEqual(UnityTuning.MenuFocusTime18, 0.30);
            Assert.Greater(UnityTuning.MenuFocusTime18, UnityTuning.MenuFocusTime, "NIGHT17's 90 ms cubic read as a snap");
        }

        // --- RESULTS --------------------------------------------------------------------------------------------------------

        [Test]
        public void TheGradesSlamSettlesPastNinetyMs()
        {
            double Settle(double dur)
            {
                var slam = new UiSlam();
                slam.Hit(UnityTuning.ThemeSlamFrom, dur);
                var s = new List<float>();
                for (int f = 0; f < 70; f++) s.Add(slam.Step(1 / 60.0)); // s[f]: what frame f shows (age (f + 1) / 60)
                return SettleAge(t => Math.Abs(s[Math.Min(s.Count - 1, (int)Math.Round(t * 60) - 1)] - 1f) <= 0.03f);
            }
            Assert.Less(Settle(UnityTuning.ResultsGradeSlamN17), 0.09, "NIGHT17's slam (build 1: 67 ms)");
            double n18 = Settle(UnityTuning.ResultsGradeSlam18);
            Assert.GreaterOrEqual(n18, 0.09);
            Assert.LessOrEqual(n18, 0.30);
        }

        [Test]
        public void TheCardFootSettlesPastNinetyMs()
        {
            double Settle(double fade) => SettleAge(t => UiTween.OutCubic(t / fade) >= 0.95f);
            Assert.Less(Settle(UnityTuning.ResultsFootFadeN17), 0.09, "NIGHT17's fade (build 1: 83 ms)");
            Assert.GreaterOrEqual(Settle(UnityTuning.ResultsFootFade18), 0.09);
            Assert.LessOrEqual(Settle(UnityTuning.ResultsFootFade18), 0.30);
        }

        [Test]
        public void TheGradeFlashIsMotionNotAPopIn()
        {
            float end = MenuMotion18.GradeFlashScale(UnityTuning.TallyFlashTime18);
            Assert.AreEqual(UnityTuning.TallyFlashTo18, end, E);
            foreach (double fps in Rates)
            {
                float s = MenuMotion18.GradeFlashScale(1 / fps);
                Assert.Greater(s, 1.05f * end, $"{fps} fps: it shrinks onto the plate");
                Assert.Greater(MenuMotion18.GradeFlashAlpha(1 / fps), 0.5f, $"{fps} fps: a flash, at once");
            }
            Assert.AreEqual(1f, MenuMotion18.GradeFlashAlpha(0), E);
            Assert.AreEqual(0f, MenuMotion18.GradeFlashAlpha(UnityTuning.TallyFlashTime18), E, "gone");
            Assert.AreEqual(0f, MenuMotion18.GradeFlashAlpha(1), E);
            Assert.LessOrEqual(UnityTuning.TallyFlashTime18, 0.25, "one beat, not a strobe");
        }

        // --- the words ------------------------------------------------------------------------------------------------------

        [Test]
        public void AHintsAsciiKeyBecomesAChip()
        {
            Assert.IsTrue(MenuRules.KeyChipSplit("[E] BUY GUN", out string key, out string rest));
            Assert.AreEqual("E", key);
            Assert.AreEqual("BUY GUN", rest);
            Assert.IsTrue(MenuRules.KeyChipSplit("[LEFT SHIFT]BOOST", out key, out rest));
            Assert.AreEqual("LEFT SHIFT", key);
            Assert.AreEqual("BOOST", rest);
            Assert.IsFalse(MenuRules.KeyChipSplit("POWER-UP · GRAB IT", out key, out rest));
            Assert.IsNull(key);
            Assert.AreEqual("POWER-UP · GRAB IT", rest);
            Assert.IsFalse(MenuRules.KeyChipSplit("[] NOTHING", out key, out rest), "an empty key");
            Assert.IsFalse(MenuRules.KeyChipSplit("[E", out key, out rest), "no close");
            Assert.IsFalse(MenuRules.KeyChipSplit(null, out key, out rest));
            Assert.IsFalse(MenuRules.KeyChipSplit("BUY [E]", out key, out rest), "only a leading key");
            foreach (string line in UnityTuning.TipLines17.Values)
                if (line.StartsWith("[")) Assert.IsTrue(MenuRules.KeyChipSplit(line, out _, out _), line);
            foreach (string line in UnityTuning.LessonLines17.Values)
                if (line.StartsWith("[")) Assert.IsTrue(MenuRules.KeyChipSplit(line, out _, out _), line);
        }

        [Test]
        public void AccessibilityShowsFovKickAndCameraShakeUpFrontWithFeelOn()
        {
            Assert.IsFalse(MenuRules.Advanced(UnityTuning.FovKickSetting, true));
            Assert.IsFalse(MenuRules.Advanced(UnityTuning.CameraShakeSetting, true));
            Assert.IsTrue(MenuRules.Advanced(UnityTuning.FovKickSetting, false), "NIGHT17: behind ADVANCED");
            Assert.IsTrue(MenuRules.Advanced(UnityTuning.CameraShakeSetting, false));
            Assert.IsTrue(MenuRules.Advanced("hurtVignette", true), "LOW HEALTH EDGE makes room");
            Assert.IsFalse(MenuRules.Advanced("hurtVignette", false));
            Assert.IsTrue(MenuRules.Advanced("toneMapping", true), "the rest as the table");
            Assert.IsFalse(MenuRules.Advanced("fovDeg", true));
            foreach (bool n18 in new[] { false, true })
            {
                var front = new Dictionary<string, int>();
                foreach (var row in UnityTuning.MenuSettingsRows)
                {
                    if (MenuRules.Advanced(row.key, n18)) continue;
                    front.TryGetValue(row.tab, out int k);
                    front[row.tab] = k + 1;
                }
                foreach (var kv in front) Assert.LessOrEqual(kv.Value, UnityTuning.MenuRowsBeforeFold, $"tab {kv.Key}, feel {n18}");
            }
        }
    }
}
