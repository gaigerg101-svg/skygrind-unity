// NIGHT17 U3-30.8 (builder N, menus and words): the menus' pure rules (MenuRules, UiMenus.cs) and the copy's word counts
// (UnityTuning.Menus17.cs). GG after NIGHT16: "the menus ... way too wordy ... way too cluttered and has way too many
// options". The rules: SETTINGS at most 5 tabs (LAB only in a developer build, GAMEPAD only with a pad), at most 8 rows a
// tab before ADVANCED; PAUSED exactly RESUME / RESTART / SETTINGS / QUIT (QUIT asks once); TITLE at most 5 rows; every
// line at most 8 words (3 on the HUD); RESULTS' lines 200 ms apart; a prompt shows when useful, goes on use, comes back
// after 60 s unused, at most 3 at once.
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class MenusTests
    {
        [Test]
        public void SettingsShowsAtMostFiveTabsWithTheLabOnlyInADeveloperBuild()
        {
            CollectionAssert.AreEqual(new[] { "game", "controls", "video", "access" }, MenuRules.Tabs(false, false, true), "no pad, a player's build");
            CollectionAssert.AreEqual(new[] { "game", "controls", "video", "access", "pad" }, MenuRules.Tabs(true, false, true), "a pad: GAMEPAD joins");
            CollectionAssert.AreEqual(new[] { "game", "controls", "video", "access", "pad", "lab" }, MenuRules.Tabs(true, true, true), "a developer build: the LAB too");
            CollectionAssert.AreEqual(new[] { "game", "controls", "video", "access" }, MenuRules.Tabs(false, true, false), "no LAB rows: no LAB tab");
            Assert.LessOrEqual(MenuRules.Tabs(true, false, true).Count, 5);
        }

        [Test]
        public void EveryTabShowsAtMostEightRowsBeforeAdvanced()
        {
            var front = new Dictionary<string, int>();
            foreach (var row in UnityTuning.MenuSettingsRows)
            {
                if (row.advanced) continue;
                front.TryGetValue(row.tab, out int n);
                front[row.tab] = n + 1;
            }
            foreach (var kv in front) Assert.LessOrEqual(kv.Value, UnityTuning.MenuRowsBeforeFold, $"tab {kv.Key}");
            Assert.AreEqual(8, UnityTuning.MenuRowsBeforeFold);
            Assert.IsTrue(MenuRules.Advanced("toneMapping"));
            Assert.IsFalse(MenuRules.Advanced("fovDeg"));
        }

        [Test]
        public void EveryRowTheSettingsShowedHasAHomeAndTheLabKeepsItsOwn()
        {
            string[] shown =
            {
                "sensitivity", "invertY", "aimAssist", "snapStrength", "adsSensitivity", "fovDeg", "fpsCap", "vsync", "quality", "filmGrain",
                "toneMapping", "renderScale", "windowMode", "resolution", "crosshairColor", "damageDirection", "showPrices", "xray", "damageNumbers",
                "effectsIntensity", "reducedMotion", "hurtVignette", "cameraRoll", "flashIntensity", "slideMode", "arclineMode", "padAimAssist",
                "padSensX", "padSensY", "padCurve", "padDeadzoneInner", "padDeadzoneOuter", "padInvertY", "padRumble", "onboarding",
                "runLog", "recap", "hitFreeze", "slowMo",
            };
            foreach (string k in shown) Assert.IsNotNull(MenuRules.TabOf(k, "aim"), k);
            Assert.AreEqual("controls", MenuRules.TabOf("sensitivity", "aim"), "the mouse's rows are CONTROLS");
            Assert.AreEqual("pad", MenuRules.TabOf("padSensX", "pad"));
            Assert.AreEqual("game", MenuRules.TabOf(UnityTuning.HudScaleSetting, null), "HUD SCALE in GAME");
            Assert.AreEqual("lab", MenuRules.TabOf("closeKillHeal", "lab"), "a LAB toggle stays in the LAB");
            Assert.IsNull(MenuRules.TabOf("uiScale", "hud"), "UI SCALE gives way to HUD SCALE");
            Assert.IsNull(MenuRules.TabOf("padVibration", "pad"), "round 2: VIBRATION folds into RUMBLE (0 is off)");
            Assert.IsNull(MenuRules.TabOf("gunFinish", "hud"), "a row Unity doesn't apply stays hidden");
            Assert.AreEqual("MOUSE SPEED", MenuRules.LabelOf("sensitivity", "MOUSE SENSITIVITY"));
            Assert.AreEqual("SOMETHING", MenuRules.LabelOf("closeKillHeal", "SOMETHING"), "no label of ours: the store's");
            Assert.AreEqual("", MenuRules.FootOf("closeKillHeal"), "no footer of ours: none (never the long desc)");
        }

        [Test]
        public void PausedIsResumeRestartSettingsQuitAndCopyReportOnlyForADeveloper()
        {
            CollectionAssert.AreEqual(new[] { "RESUME", "RESTART", "SETTINGS", "QUIT" }, MenuRules.PauseItems(false));
            CollectionAssert.AreEqual(new[] { "RESUME", "RESTART", "SETTINGS", "QUIT", "COPY REPORT" }, MenuRules.PauseItems(true));
        }

        [Test]
        public void QuitAsksOnceThenQuitsSavingAtABreak()
        {
            var c = new MenuRules.QuitConfirm();
            Assert.AreEqual(MenuRules.QuitAct.Arm, MenuRules.Quit(ref c, 10, 3, true), "the first press asks");
            Assert.IsTrue(MenuRules.Armed(c, 11, 3));
            Assert.AreEqual(MenuRules.QuitAct.SaveAndQuit, MenuRules.Quit(ref c, 11.5, 3, true), "the second, at a break: save and quit");
            Assert.IsFalse(MenuRules.Armed(c, 11.6, 3), "spent");
            Assert.AreEqual(MenuRules.QuitAct.Arm, MenuRules.Quit(ref c, 20, 3, false));
            Assert.AreEqual(MenuRules.QuitAct.ToTitle, MenuRules.Quit(ref c, 21, 3, false), "no save allowed: to TITLE");
            Assert.AreEqual(MenuRules.QuitAct.Arm, MenuRules.Quit(ref c, 30, 3, false));
            Assert.IsFalse(MenuRules.Armed(c, 33.5, 3), "the ask runs out");
            Assert.AreEqual(MenuRules.QuitAct.Arm, MenuRules.Quit(ref c, 33.5, 3, false), "too late: asks again");
        }

        [Test]
        public void EveryLineIsAtMostEightWordsAndThreeOnTheHud()
        {
            Assert.AreEqual(3, MenuRules.Words("[SHIFT] BOOST NOW"));
            Assert.AreEqual(2, MenuRules.Words("SAVE & QUIT"), "a lone symbol is no word");
            Assert.AreEqual(4, MenuRules.Words("ROUND 3 · 1250 · 4:12"));
            foreach (var row in UnityTuning.MenuSettingsRows)
            {
                Assert.LessOrEqual(MenuRules.Words(row.foot), 8, row.foot);
                Assert.LessOrEqual(MenuRules.Words(row.label), 3, row.label);
            }
            foreach (var p in UnityTuning.Prompts) Assert.LessOrEqual(MenuRules.Words("[X] " + p.verb), 3, p.verb);
            foreach (var kv in UnityTuning.LessonLines17) Assert.LessOrEqual(MenuRules.Words(kv.Value), 3, kv.Value);
            string[] copy =
            {
                UnityTuning.PauseQuitSure, UnityTuning.StaleTip17, UnityTuning.SlideChargeHint17, UnityTuning.ContractBadgeReward17,
                UnityTuning.ContractDormantLine17, UnityTuning.PauseSaveQuitLabel, UnityTuning.TitleProgressLabel,
            };
            foreach (string s in copy) Assert.LessOrEqual(MenuRules.Words(s), 8, s);
        }

        [Test]
        public void ResultsLinesArriveTwoHundredMillisecondsApart()
        {
            Assert.AreEqual(0, MenuRules.LineAt(0), 1e-9);
            Assert.AreEqual(0.2, MenuRules.LineAt(1), 1e-9);
            Assert.AreEqual(0.6, MenuRules.LineAt(3), 1e-9);
        }

        [Test]
        public void APromptShowsWhenUsefulGoesOnUseAndComesBackAfterAMinute()
        {
            Assert.IsTrue(MenuRules.PromptDue(true, 1.0, false, 0), "useful, never used");
            Assert.IsFalse(MenuRules.PromptDue(true, 0.2, false, 0), "a passing moment flashes nothing");
            Assert.IsFalse(MenuRules.PromptDue(false, 5, false, 0), "not useful now");
            Assert.IsFalse(MenuRules.PromptDue(true, 5, true, 10), "used 10 s ago: gone");
            Assert.IsTrue(MenuRules.PromptDue(true, 5, true, 61), "unused for a minute: back");
            var picked = new int[3];
            Assert.AreEqual(3, MenuRules.PickPrompts(new[] { true, true, false, true, true, true }, 3, picked), "at most 3 at once");
            CollectionAssert.AreEqual(new[] { 0, 1, 3 }, picked, "in their order");
            Assert.AreEqual(1, MenuRules.PickPrompts(new[] { false, false, true }, 3, picked));
            Assert.AreEqual(2, picked[0]);
        }

        [Test]
        public void EveryColumnSitsInsideTheSafeInsetAtSixteenByTen()
        {
            // the canvas matches 1080 units of height: at 16:10 its half-width is 864, minus the 56 px inset
            Assert.AreEqual(-864f + UnityTuning.ThemeSafeInset, UnityTuning.MenuSafeLeft, 0.5f);
            for (float y = 440; y >= -460; y -= 10) Assert.GreaterOrEqual(MenuRules.Left(y), UnityTuning.MenuSafeLeft, $"the column's left edge at y {y}");
            Assert.AreEqual(UnityTuning.MenuColumnLeft + System.Math.Tan(12 * System.Math.PI / 180) * 100, MenuRules.Left(100), 0.01, "the 12 deg diagonal");
            int tabs = UnityTuning.MenuSettingsTabs.Length;
            Assert.LessOrEqual(MenuRules.Left(UnityTuning.SettingsTabsY) + tabs * (UnityTuning.SettingsTab17Width + UnityTuning.SettingsTabGap), UnityTuning.MenuSafeRight, "six tabs fit");
            Assert.LessOrEqual(MenuRules.Left(UnityTuning.ResultsHeroY) + UnityTuning.ResultsGradeFromLeft + UnityTuning.ResultsGradeWidth, UnityTuning.MenuSafeRight, "the grade");
            Assert.LessOrEqual(UnityTuning.ResultsPayoffX + UnityTuning.PayoffWidth, UnityTuning.MenuSafeRight, "the PAYOFF strip");
            Assert.LessOrEqual(UnityTuning.ResultsRouteRight, UnityTuning.MenuSafeRight, "THE ROUTE");
            Assert.LessOrEqual(UnityTuning.SettingsRow17Width * 0.5f, UnityTuning.MenuSafeRight, "a settings row");
        }

        [Test]
        public void ASentenceToastNeverReachesTheHud()
        {
            Assert.AreEqual("POWER-UP · GRAB IT", MenuRules.HudHint("powerUp", "A POWER-UP DROPPED: RUN THROUGH IT BEFORE IT FADES"));
            Assert.AreEqual("ENEMY X-RAY ON", MenuRules.HudHint(null, "ENEMY X-RAY ON"), "short already: as it is");
            Assert.IsNull(MenuRules.HudHint("someNewTip", "A BRAND NEW TIP THAT SAYS FAR TOO MUCH"), "a long line with no short one: dropped");
            Assert.IsNotNull(MenuRules.HudHint(null, "THIS GAMEPAD DOES NOT REPORT THE STANDARD LAYOUT: BUTTONS MAY BE SWAPPED · SEE SETTINGS > GAMEPAD"), "a known notice has its short line");
            foreach (var kv in UnityTuning.TipLines17) Assert.LessOrEqual(MenuRules.Words(kv.Value), 3, kv.Key);
            foreach (var kv in UnityTuning.HintLines17) Assert.LessOrEqual(MenuRules.Words(kv.Value), 3, kv.Key);
        }

        [Test]
        public void APromptWaitsForAQuietMomentAndTheWheelShowsOnce()
        {
            // round 2's reviewer: never with an enemy within 15 m, never within 3 s of a shot / a swap / a hit, never while
            // sprinting or sliding; only its first 3 showings; gone the moment its verb is used
            Assert.AreEqual(15.0, UnityTuning.PromptQuietRange);
            Assert.AreEqual(3.0, UnityTuning.PromptActionQuiet);
            Assert.AreEqual(3, UnityTuning.PromptMaxShows);
            Assert.IsFalse(MenuRules.PromptQuiet(12, 10, false, false), "an enemy 12 m away: no prompt");
            Assert.IsTrue(MenuRules.PromptQuiet(16, 10, false, false));
            Assert.IsTrue(MenuRules.PromptQuiet(double.PositiveInfinity, double.PositiveInfinity, false, false), "no enemy, no action yet");
            Assert.IsFalse(MenuRules.PromptQuiet(40, 1.5, false, false), "1.5 s after a shot, a swap or a hit");
            Assert.IsFalse(MenuRules.PromptQuiet(40, 10, true, false), "while sprinting");
            Assert.IsFalse(MenuRules.PromptQuiet(40, 10, false, true), "while sliding");
            Assert.IsTrue(MenuRules.PromptDue(true, 5, false, 0, false, 0), "never used, never shown: due");
            Assert.IsFalse(MenuRules.PromptDue(true, 5, false, 0, false, 3), "shown 3 times: never again");
            Assert.IsFalse(MenuRules.PromptDue(true, 5, true, 120, true, 1), "the wheel: gone for good after its first use");
            Assert.IsTrue(MenuRules.PromptDue(true, 5, true, 120, false, 1), "BOOST: back after a minute unused (under the cap)");
            Assert.IsFalse(MenuRules.PromptDue(true, 5, true, 2, false, 1), "used 2 s ago: gone");
            CollectionAssert.Contains(UnityTuning.PromptsOnce, "wheel");
            CollectionAssert.Contains(UnityTuning.PromptsBreakOnly, "wheel");
            foreach (var p in UnityTuning.Prompts) Assert.AreNotEqual("punch", p.id, "PUNCH would only show with an enemy close: never quiet");
        }

        [Test]
        public void EveryNumberShowsInOneFormat()
        {
            Assert.AreEqual("1.00", MenuRules.RangeShown(1.0, 0.1, 1), "RUMBLE: 1.00, like every fractional value (was 1.0)");
            Assert.AreEqual("0.25", MenuRules.RangeShown(0.25, 0.05, 1));
            Assert.AreEqual("90", MenuRules.RangeShown(90, 1, 1), "a whole-number range: no decimals");
            Assert.AreEqual("1.40", MenuRules.RangeShown(0.0014, 0.0001, 1000), "MOUSE SPEED: one plain number");
            CollectionAssert.DoesNotContain(System.Array.ConvertAll(UnityTuning.MenuSettingsRows, r => r.key), "padVibration", "VIBRATION folds into RUMBLE (0 = off): one row");
        }

        [Test]
        public void TitleShowsOnlyTheRungsWithAGrade()
        {
            CollectionAssert.AreEqual(new[] { 0 }, MenuRules.BestRungs(new[] { "D", null, null, null }));
            CollectionAssert.AreEqual(new[] { 0, 2 }, MenuRules.BestRungs(new[] { "B", null, "S", null }));
            CollectionAssert.IsEmpty(MenuRules.BestRungs(new string[] { null, null }), "all dashes: no row");
        }

        [Test]
        public void TheResultsCardFitsTheInsetAtBothAspects()
        {
            // the card is a slanted UiShape: its top leans right and its bottom left by tan(12 deg) x half its height
            float skew = (float)System.Math.Tan(UnityTuning.ThemeSlantDeg * System.Math.PI / 180) * (UnityTuning.ResultsCardTop - UnityTuning.ResultsCardBottom) * 0.5f;
            float left = MenuRules.ResultsCardLeft();
            Assert.GreaterOrEqual(left - skew, UnityTuning.MenuSafeLeft, "the card's bottom-left corner");
            Assert.LessOrEqual(UnityTuning.ResultsCardRight + skew, UnityTuning.MenuSafeRight, "the card's top-right corner");
            Assert.LessOrEqual(UnityTuning.ResultsCardTop + 12, UnityTuning.ResultsHeadY - 30, "under the outcome's line");
            Assert.LessOrEqual(UnityTuning.ResultsFirstRowY + UnityTuning.MenuItemHeight * 0.5f, UnityTuning.ResultsCardBottom - 10, "the buttons under the card");
            float lowest = UnityTuning.ResultsFirstRowY - 4 * UnityTuning.MenuItemPitch - UnityTuning.MenuItemHeight * 0.5f;
            Assert.GreaterOrEqual(lowest, -540 + UnityTuning.ThemeSafeInset, "five buttons (with a developer's) above the bottom inset");
            Assert.Less(MenuRules.Left(UnityTuning.ResultsHeroY) + UnityTuning.ResultsGradeFromLeft + UnityTuning.ResultsGradeWidth, UnityTuning.ResultsMapX - UnityTuning.ResultsMapSize * 0.5f, "the grade left of the map");
            Assert.GreaterOrEqual(UnityTuning.ResultsMapSize, 280f, "round 2: a map that can be read");
            Assert.GreaterOrEqual(UnityTuning.ResultsCardPad, 60f, "round 2: the accent bar clear of the ROUND label");
        }

        [Test]
        public void TheCardsBestMomentIsOneShortLine()
        {
            Assert.AreEqual("BEST BURST +33", MenuRules.BestMoment("KILL", 33, 31), "round 2: a plain kill isn't a name");
            Assert.AreEqual("BEST BURST +480 · TRIPLE", MenuRules.BestMoment("TRIPLE", 480, 31));
            Assert.AreEqual("TOP SPEED 31 M/S", MenuRules.BestMoment(null, 0, 31), "no award: the top speed");
            Assert.AreEqual("", MenuRules.BestMoment(null, 0, 0), "nothing to say: nothing");
            Assert.LessOrEqual(MenuRules.Words(MenuRules.BestMoment("FLYING DOUBLE HEADSHOT", 1200, 40)), 8);
            Assert.AreEqual("5 OCT 2026  ·  SEED 0038WR", MenuRules.CardStamp(new System.DateTime(2026, 10, 5), "0038WR"));
            Assert.AreEqual("5 OCT 2026", MenuRules.CardStamp(new System.DateTime(2026, 10, 5), null));
        }

        [Test]
        public void AModCardsHeroIsItsNumber()
        {
            Assert.AreEqual("+25%", MenuRules.ModHero("HITS ON ENEMIES OVER 20 M AWAY DEAL +25%", out string rest));
            Assert.AreEqual("HITS ON ENEMIES OVER 20 M AWAY DEAL", rest, "the hero leaves the line");
            Assert.AreEqual("+100 POINTS", MenuRules.ModHero("ELITE AND AFFIXED KILLS PAY +100 POINTS", out rest));
            Assert.AreEqual("ELITE AND AFFIXED KILLS PAY", rest);
            Assert.AreEqual("5 HP", MenuRules.ModHero("KILLS HEAL 5 HP", out rest), "no + or %: the first number and its unit");
            Assert.AreEqual("KILLS HEAL", rest);
            Assert.AreEqual("25%", MenuRules.ModHero("HEADSHOTS STAGGER (25% OF THE TIME)", out rest));
            Assert.AreEqual("HEADSHOTS STAGGER OF THE TIME", rest, "no stray brackets");
            Assert.AreEqual("", MenuRules.ModHero("NO NUMBER HERE", out rest));
            Assert.AreEqual("NO NUMBER HERE", rest);
        }

        [Test]
        public void ASettingsFooterSaysWhatTheRowDoes()
        {
            foreach (var row in UnityTuning.MenuSettingsRows)
            {
                Assert.IsFalse(row.foot.Contains(row.label), $"{row.label}: the footer repeats the label ('{row.foot}')");
                Assert.AreNotEqual(row.label, row.foot);
            }
            Assert.AreEqual("SCALES THE HUD 80-125 %", MenuRules.FootOf(UnityTuning.HudScaleSetting));
        }

        [Test]
        public void OnlyOneItemIsEverLit()
        {
            // round 4's reviewer: TITLE and DETAILS both looked focused while RETRY was ink. The one leaving goes dark at once;
            // the one arriving lights over the focus time: at no frame are two items lit.
            float a = 1f, b = 0f, step = 0.3f;
            a = MenuRules.FocusLit(a, false, step);
            b = MenuRules.FocusLit(b, true, step);
            Assert.AreEqual(0f, a, "the item the focus left is dark at once");
            Assert.AreEqual(0.3f, b, 1e-6, "the new one lights over the focus time");
            for (int i = 0; i < 5; i++) b = MenuRules.FocusLit(b, true, step);
            Assert.AreEqual(1f, b, "and stays lit");
            Assert.AreEqual(0f, MenuRules.FocusLit(-1f, false, step), "a fresh item at rest");
            Assert.AreEqual(1f, MenuRules.FocusLit(-1f, true, step), "a fresh focused item lit at once (its entrance)");
        }

        [Test]
        public void AModLineIsOneFinishedThought()
        {
            // round 4: the line under the hero was the sentence with its number cut out ("... OVER 20 M AWAY DEAL"): now the
            // number and a noun ("+25%" over "DAMAGE BEYOND 20 M"), one finished line per mod, at most 6 words.
            string[] ids =
            {
                "skydancer", "wallbanger", "railgrinder", "conductor", "pyre", "keenEdge", "gravityWell", "shrapnel", "lance", "styleTax",
                "variety", "bloodPrice", "interest", "scavenger", "adrenaline", "momentumRounds", "freshStart", "bountyHunter", "crossfire",
            };
            foreach (string id in ids)
            {
                Assert.IsTrue(UnityTuning.ModLines17.ContainsKey(id), id);
                Assert.LessOrEqual(MenuRules.Words(UnityTuning.ModLines17[id]), 6, id);
            }
            Assert.AreEqual("DAMAGE BEYOND 20 M", MenuRules.ModLine17("lance", "HITS ON ENEMIES OVER 20 M AWAY DEAL"));
            Assert.AreEqual("SOME REST", MenuRules.ModLine17("aNewMod", "SOME REST"), "a mod with no line of ours: the cut sentence");
            Assert.IsFalse(MenuRules.ModPaysPoints("scavenger"), "SCAVENGER drops power-ups: not gold");
            Assert.IsTrue(MenuRules.ModPaysPoints("bountyHunter"));
        }

        [Test]
        public void TheRunSetupCyclesItsRowsAndTitleKeepsFiveRows()
        {
            Assert.AreEqual(1, MenuRules.CycleHeat(0, 2, 1));
            Assert.AreEqual(0, MenuRules.CycleHeat(2, 2, 1), "wraps");
            Assert.AreEqual(2, MenuRules.CycleHeat(0, 2, -1), "wraps back");
            Assert.AreEqual(0, MenuRules.CycleHeat(0, 0, 1), "nothing open: 0");
            Assert.AreEqual(5, MenuRules.TitleRows(false));
            Assert.AreEqual(5, MenuRules.TitleRows(true), "RESUME rides PLAY's row");
        }
    }
}
