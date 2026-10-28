// NIGHT17 U3-30.7 (builder H): the style meter, ULTRAKILL-like (Research/2026-10-05-overhaul.md section 2), replacing the
// web's rank box (the letter in the rank's hue, its name, a 6 px bar and the award feed: Ui.cs builds those hidden now).
// - IDENTITY.md's tile (U3-30.2, direction A): the rank LETTER in UiTheme.Rank (Tourney Black Italic) at S133 on the
//   left, with the hero offset copy (CYAN-DEEP, ThemeHeroOffset px down-right: the wordmark's idea) behind it; right of it
//   a column: the WORD (S32 DISPLAY, CYAN), the slanted decay bar on INK, the callouts. The block sits right of centre at
//   the safe inset. A rank-up is the screen's event (round 1's fix list): the letter slams 3x -> 1x in 120 ms (UiSlam),
//   white for its first frames; the word slams in at 1.8x along the slant and shows WHITE for 600 ms (then CYAN); the whole
//   HUD (the frame and values layers) shakes 5 px for 100 ms (rank-ups only; REDUCED MOTION never starts it).
// - The cyan VALUE ladder (NIGHT17.md rule 2, CLAUDE.md rule 8: never the web's red-orange / magenta rank hues): the low
//   third CYAN-PALE (rank 0 MUTED), the middle CYAN, the top third CYAN-HOT with a glow copy behind (x1.1, CYAN at 0.45).
// - The DECAY BAR under the letter (UiShape .Fill: StyleMeter.Fill, the shown rank's band), draining as the meter drains.
// - A rank DROP shown: on the player's hurt (the meter's damage penalty) and on a rank down, the letter is knocked down
//   22 px and tipped 9 deg, settling over 280 ms; the bar alone flashes DANGER for 160 ms (a danger cue, no new hue).
// - Named CALLOUTS (styleAward, not quiet): "+HEADSHOT", newest on top, each popping in at 1.3x and settling over 120 ms,
//   fading over the last quarter of hud.feedFadeTime; a trick that pays nothing in MUTED. The line's string is made once
//   per award name.
// - The MULTIPLIER CHIP ("x1.2 AIR", NIGHT16 entry 39): while the mover's state pays style.stateMult (StyleCore.cs:193,
//   StyleMath.StateMultiplier), an INK chip slams in from 1.6x with a 2-frame white flash; when the state ends it lingers
//   0.45 s, its thin CYAN bar draining, then leaves. Its label is made once per state.
// - The block shows from the run's first style (UiFeelHud's rankPlateAlpha: no "D" greeting at a run's start).
// - NIGHT19 U3-33.8 (builder H): the COMBO callouts (N19Events.Combo, W's emitters: POP, CONDUCTOR, LAUNCH + SNIPE,
//   SWAP-CANCEL ...): the combo's name in INK on a CYAN slanted plate, slammed into the column's top slot (ComboCallout19:
//   from 1.6x at full alpha, the plate WHITE for its first frames), held 1.5 s; the older lines move down a slot (never
//   under it), and each line now leaves at its own life (StyleCalloutExpire19). A combo shows the block on its own.
// Frame time only; nothing gates the sim. No allocation per frame (texts set on change; colours from UiTheme).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform styleRoot, styleLetterBox, styleWordRt, styleChipRt;
        CanvasGroup styleGroup, styleWordGroup, styleChipGroup;
        TextMeshProUGUI styleLetter, styleLetterOffset, styleLetterGlow, styleWord, styleChipText;
        UiShape styleBarTrack, styleBarFill, styleChipPlate, styleChipBar, styleColPlate, styleStreak;
        double styleStreakAge = double.MaxValue;
        Color styleStreakColor;
        float styleColTop, styleCalloutTopY;
        int styleColShownCount = -1;
        TextMeshProUGUI[] styleCalloutText;
        string[] styleCalloutLine;
        double[] styleCalloutAge;
        bool[] styleCalloutDim, styleCalloutStreak;
        int[] styleCalloutRepeat; // round 5: how many times the line's award landed (x2 ...)
        // NIGHT19 U3-33.8: a line that is a combo (N19Events.Combo), and each text object's CYAN plate behind it
        bool[] styleCalloutCombo;
        UiShape[] styleCalloutPlate19;
        int[] styleCalloutPlateKey19; // per text object: the plate's look shown (-1 hidden, 0 cyan, 1 white)
        readonly Dictionary<string, string> styleCalloutRepeatFormat = new Dictionary<string, string>();
        readonly double[] styleKillTimes = new double[UnityTuning.Hud17StreakKills]; // U3-30.10 (b): the player's recent kills (style clock)
        int styleKillCount;
        int styleCalloutCount;
        bool styleCalloutsDirty;
        readonly Dictionary<string, string> styleCalloutLines = new Dictionary<string, string>();
        readonly Dictionary<string, string> styleChipLabels = new Dictionary<string, string>();
        int styleShownRank = -1, styleShownStep = -1, styleShownBarTint = -1;
        double styleWordAge = double.MaxValue, styleKnockAge = double.MaxValue, styleDangerLeft, styleShakeLeft, styleGlowClock, styleSlamAge = double.MaxValue;
        UiSlam styleChipSlam, styleWordSlam;
        bool styleChipOn;
        string styleChipState;
        double styleChipSince, styleChipWhite;
        float styleShownAlpha = -1, styleShownScale = -1, styleShownKnock = -1, styleShownShake = -1, styleShownChipScale = -1;
        double styleFeedLife;

        // NIGHT18 U3-31.6 (FeelAB on): the block's first showing ENTERS (HudEntrance18 on the whole block: grows, fades in,
        // slides along the slant) instead of appearing whole with the letter held at 2.2x white-hot (NIGHT17's reviewers: "a
        // grey ghost for 100+ ms"); a rank-up POPS the letter from small past its size (UiTween.PopIn, white for its first 50
        // ms) and the word enters; the multiplier chip enters; the streak fades in as it sweeps; each callout line keeps its
        // own text object as it moves down a slot (eased), so only a NEW line enters (grows, fades in, slides) and an older
        // one never reappears whole one slot lower.
        double styleBlockEnterAge18 = double.MaxValue, styleChipEnterAge18 = double.MaxValue;
        Vector2 styleRootHome18;
        double styleFadeAge25 = double.MaxValue; // NIGHT25: seconds since the block was shown (MaxValue: not fading)
        float styleBlockShownScale18 = 1;
        Vector2 styleBlockShownOff18;
        bool styleBlockWhite18, styleBlockWhiteShown18; // round 3: the block's first slam shows the letter white-hot
        int[] styleCalloutObj18; // slot -> its text object (styleCalloutText index)
        float[] styleCalloutY18, styleCalloutPx18; // per text object: its y and font size now (eased toward its slot's)
        bool[] styleCalloutUsed18, styleCalloutFresh18; // scratch; a reused object that must snap to its new slot
        bool styleCallouts18Drawn;

        /// <summary>The rank's colour on the cyan value ladder (rank 0: MUTED).</summary>
        static Color StyleLadderColor(int rank, int count)
        {
            if (rank <= 0) return UiTheme.CyanPale; // round 3: D at full value on the pale step (no grey)
            int step = StyleLadder.Step(rank, count);
            return step == 2 ? UiTheme.CyanHot : step == 1 ? UiTheme.Cyan : UiTheme.CyanPale;
        }

        void BuildStyleMeter()
        {
            float inset = UnityTuning.ThemeSafeInset;
            styleFeedLife = UnityTuning.Hud17CalloutLife; // round 2's reviewer: short-lived lines (was hud.feedFadeTime)
            styleRoot = Hud17Rect(values, "Style meter", new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(-inset - UnityTuning.Hud17StyleRight, -UnityTuning.Hud17StyleTop), new Vector2(UnityTuning.Hud17StyleWidth, UnityTuning.Hud17StyleLetterBox));
            styleGroup = styleRoot.gameObject.AddComponent<CanvasGroup>();
            styleGroup.alpha = 0;
            styleGroup.interactable = styleGroup.blocksRaycasts = false;
            // U3-30.10 (a): the rank-up's streak, a full-width band rotated to the theme's slant through the letter's middle,
            // behind the whole block (its sibling just before it).
            // Its pivot is the letter's middle (the band turns about it); its span is set on each rank-up (StyleStreakLayout:
            // the canvas width is known then), behind the style column only.

            // IDENTITY.md's tile: the letter on the left (its box right-aligned at Hud17StyleLetterWidth), then a column: the
            // word (DISPLAY, cyan), the slanted decay bar on an INK track, the callouts; the chip over the word.
            float lpx = UnityTuning.Hud17StyleLetterPx, box = UnityTuning.Hud17StyleLetterBox, lw = UnityTuning.Hud17StyleLetterWidth, col = UnityTuning.Hud17StyleColumn;
            var left = new Vector2(0, 0.5f);
            styleLetterBox = Hud17Rect(styleRoot, "Letter", left, new Vector2(1, 0.5f), new Vector2(lw, 0), new Vector2(lw, box));
            styleLetterOffset = Hud17Text(styleLetterBox, "Letter (offset)", "", lpx, UiTheme.A(UiTheme.CyanDeep, UnityTuning.Hud17StyleOffsetAlpha), UiTheme.Rank, TextAlignmentOptions.Right, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(UnityTuning.ThemeHeroOffset, -UnityTuning.ThemeHeroOffset), new Vector2(lw * 1.6f, box), false);
            styleLetterGlow = Hud17Text(styleLetterBox, "Letter (glow)", "", lpx, UiTheme.A(UiTheme.Cyan, UnityTuning.Hud17StyleGlowAlpha), UiTheme.Rank, TextAlignmentOptions.Right, new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero, new Vector2(lw * 1.6f, box), false);
            styleLetterGlow.rectTransform.localScale = new Vector3(UnityTuning.Hud17StyleGlowScale, UnityTuning.Hud17StyleGlowScale, 1);
            styleLetterGlow.enabled = false;
            styleLetter = Hud17Text(styleLetterBox, "Letter", "", lpx, UiTheme.CyanPale, UiTheme.Rank, TextAlignmentOptions.Right, new Vector2(1, 0.5f), new Vector2(1, 0.5f), Vector2.zero, new Vector2(lw * 1.6f, box), true);

            // Round 3's reviewer: the whole block (letter, word, bar, callouts) on an OPAQUE INK plate, slanted and cut, in its
            // top-third place (the world's cyan trim ran through the plate-less D); it grows with the callouts (StyleColumnFit).
            float wpx = UiTheme.Px(UiTheme.Size.S32), pad = UnityTuning.Hud17StylePlatePad;
            styleColTop = UnityTuning.Hud17StyleLetterCap * 0.5f + pad * 0.5f; // round 5: the plate fits the letter at rest
            styleColPlate = Hud17Shape(styleRoot, "Block plate", UiTheme.InkSolid, left, new Vector2(0, 1), new Vector2(-pad, styleColTop), new Vector2(UnityTuning.Hud17StyleWidth + 2 * pad, 0));
            styleColPlate.transform.SetAsFirstSibling();
            // U3-30.10 (a): the rank-up streak, over the plate and under the letter: its pivot the letter's middle (the band
            // turns about it); its span set on each rank-up (StyleStreakLayout).
            styleStreak = Hud17Shape(styleRoot, "Rank-up streak", UiTheme.Cyan, left, new Vector2(0.5f, 0.5f), new Vector2(lw * 0.5f, 0), new Vector2(UnityTuning.Hud17StyleWidth, UnityTuning.Hud17StreakHeight));
            styleStreak.SetCuts(0, 0);
            styleStreak.rectTransform.localRotation = Quaternion.Euler(0, 0, UnityTuning.ThemeSlantDeg);
            styleStreak.transform.SetSiblingIndex(1);
            styleStreak.enabled = false;

            // The word, at the column's top (always shown with the block; a rank-up slides it in hot for 600 ms).
            styleWord = Hud17Text(styleRoot, "Word", "", wpx, UiTheme.Cyan, UiTheme.Display, TextAlignmentOptions.Left, left, left, new Vector2(col, UnityTuning.Hud17StyleWordY), new Vector2(UnityTuning.Hud17StyleWidth - col, wpx * 1.4f), true);
            styleWordRt = styleWord.rectTransform;
            styleWordGroup = styleWord.gameObject.AddComponent<CanvasGroup>();
            styleWordGroup.alpha = 1;
            Hud17Stroke(styleLetter);
            Hud17Stroke(styleWord);
            BuildSheen22(styleLetterBox, lw, box); // NIGHT22 D-7: the Player-safe shine band over the letter (UiStamp22.cs)

            // The decay bar under the word: slanted, on an INK track.
            float barY = UnityTuning.Hud17StyleBarY;
            var barSize = new Vector2(UnityTuning.Hud17StyleBarWidth, UnityTuning.Hud17StyleBarHeight);
            styleBarTrack = Hud17Shape(styleRoot, "Decay bar", UiTheme.InkSoft, left, new Vector2(0, 1), new Vector2(col, barY), barSize); // INK-SOFT: visible on the INK plate
            styleBarFill = Hud17Shape(styleRoot, "Decay bar (fill)", UiTheme.CyanPale, left, new Vector2(0, 1), new Vector2(col, barY), barSize);
            styleBarTrack.SetCuts(0, 0);
            styleBarFill.SetCuts(0, 0);

            // The multiplier chip, over the word.
            var chipSize = new Vector2(UnityTuning.Hud17ChipWidth, UnityTuning.Hud17ChipHeight);
            styleChipRt = Hud17Rect(styleRoot, "Multiplier chip", left, new Vector2(1, 0.5f), new Vector2(-UnityTuning.Hud17ChipGap, 0), chipSize); // left of the block (round 2: the top third has no room over it)
            styleChipGroup = styleChipRt.gameObject.AddComponent<CanvasGroup>();
            styleChipGroup.alpha = 0;
            styleChipPlate = Hud17Shape(styleChipRt, "Plate", UiTheme.Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, chipSize);
            styleChipText = Hud17Text(styleChipRt, "Label", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 2), chipSize, false);
            styleChipBar = Hud17Shape(styleChipRt, "Drain", UiTheme.Cyan, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 4), new Vector2(UnityTuning.Hud17ChipWidth - 24, UnityTuning.Hud17ChipBar));
            styleChipBar.SetCuts(0, 0);

            // The callouts under the bar, newest on top (the newest S24 WHITE, the older S18 CYAN: the tile).
            int n = UnityTuning.Hud17CalloutMax;
            styleCalloutText = new TextMeshProUGUI[n];
            styleCalloutLine = new string[n];
            styleCalloutAge = new double[n];
            styleCalloutDim = new bool[n];
            styleCalloutStreak = new bool[n];
            styleCalloutRepeat = new int[n];
            styleCalloutCombo = new bool[n];
            float top = styleCalloutTopY = barY - UnityTuning.Hud17StyleBarHeight - UnityTuning.Hud17CalloutTop;
            StyleColumnFit(0);
            // NIGHT19 U3-33.8: the combo lines' CYAN plates, made before the texts (so drawn behind them), one per text object
            styleCalloutPlate19 = new UiShape[n];
            styleCalloutPlateKey19 = new int[n];
            for (int i = 0; i < n; i++)
            {
                styleCalloutPlate19[i] = Hud17Shape(styleRoot, $"Combo plate {i}", UiTheme.Cyan, left, left, new Vector2(col, top - i * UnityTuning.Hud17CalloutLine), new Vector2(100, UnityTuning.Hud17CalloutLine - 2 * UnityTuning.Hud19ComboPlateInset));
                styleCalloutPlate19[i].SetCuts(UnityTuning.ThemeCut * 0.6f, UnityTuning.ThemeCut * 0.6f);
                styleCalloutPlate19[i].enabled = false;
                styleCalloutPlateKey19[i] = -1;
            }
            for (int i = 0; i < n; i++)
            {
                float cpx = UiTheme.Px(i == 0 ? UiTheme.Size.S24 : UiTheme.Size.S18);
                styleCalloutText[i] = Hud17Text(styleRoot, $"Callout {i}", "", cpx, i == 0 ? UiTheme.White : UiTheme.Cyan, UiTheme.BodyBold, TextAlignmentOptions.Left, left, left, new Vector2(col, top - i * UnityTuning.Hud17CalloutLine), new Vector2(UnityTuning.Hud17CalloutWidth, UnityTuning.Hud17CalloutLine), true);
                styleCalloutText[i].characterSpacing = UnityTuning.ThemeLabelTracking;
                styleCalloutText[i].enabled = false;
                Hud17Stroke(styleCalloutText[i]);
            }
            styleCalloutObj18 = new int[n];
            styleCalloutY18 = new float[n];
            styleCalloutPx18 = new float[n];
            styleCalloutUsed18 = new bool[n];
            styleCalloutFresh18 = new bool[n];
            for (int i = 0; i < n; i++) styleCalloutObj18[i] = i;
            styleRootHome18 = styleRoot.anchoredPosition;

            game.events.On<StyleAwardEvent>("styleAward", a =>
            {
                if (a.quiet || !StyleLadder.Named(a.name)) return; // round 3: named events only (no plain + KILL; MULTIKILL is the streak's name)
                if (!styleCalloutLines.TryGetValue(a.name, out string line)) styleCalloutLines[a.name] = line = "+ " + a.name; // once per name ("+ HEADSHOT": the tile)
                StyleCalloutPush(line, a.points <= 0, false);
            });
            // U3-30.10 (b): kill streaks as named lines (DOUBLE / TRIPLE / MASSACRE) inside the web's multikill window, on the
            // style meter's own clock (StyleMeter.time, the sim's); the handler only queues a line (the kill fires in a step).
            game.events.On<KillEvent>("kill", e =>
            {
                StyleMeter sm = game.style;
                if (e.owner != "player" || sm == null || !sm.enabled) return;
                string name = KillStreak.Name(KillStreak.Push(styleKillTimes, ref styleKillCount, sm.time, sm.Params.multikillWindow));
                if (name != null) StyleCalloutPush(name, false, true);
            });
            // NIGHT19 U3-33.8: a named combo (W's emitters: POP, CONDUCTOR, LAUNCH + SNIPE, SWAP-CANCEL ...) slams into the
            // column's top slot on its CYAN plate (the block shows for it, even before the run's first award)
            game.events.On<ComboEvent>(N19Events.Combo, e =>
            {
                string line = ComboCallout19.Line(e);
                if (line == null) return;
                rankPlateEarned = true;
                StyleCalloutPush(line, false, false, true);
            });
            game.events.On<HurtEvent>("hurt", e =>
            {
                if (e.owner != "player" || game.style == null || !game.style.enabled) return;
                styleDangerLeft = UnityTuning.Hud17StyleDangerTime; // the bar alone flashes DANGER
                styleKnockAge = 0; // the meter just paid its damage penalty: the letter takes the knock
            });
            game.OnNewRun += () =>
            {
                styleCalloutCount = 0;
                styleKillCount = 0; // the style clock restarts with the run
                styleCalloutsDirty = true;
                styleShownRank = -1;
                styleChipOn = false;
                styleChipGroup.alpha = 0;
                styleWordAge = styleKnockAge = double.MaxValue;
            };
        }

        /// <summary>Frame time: the letter (slam, knock, shake), the word, the decay bar, the chip, the callouts.</summary>
        void RenderStyleMeter(double frameDt)
        {
            StyleMeter st = game.style;
            if (styleRoot == null || st == null) return;
            float blockAlpha = rankPlateAlpha > 0 ? 1f : 0f; // round 5: no see-through fade-in: it appears whole, with the slam
            if (blockAlpha != styleShownAlpha)
            {
                if (blockAlpha > 0 && styleShownAlpha <= 0)
                {
                    if (FeelAB.On) styleBlockEnterAge18 = 0; // NIGHT18: the whole block enters (below)
                    else styleSlamAge = 0; // its first showing slams in (UiFeelHud: hidden until the run's first style)
                }
                styleShownAlpha = blockAlpha;
                if (blockAlpha > 0 && styleFadeAge25 >= UnityTuning.StyleMeterFadeIn25) styleFadeAge25 = 0; // NIGHT25 (T-12, M9): a short fade-in under the slam, no full-alpha first frame
                else if (blockAlpha <= 0) styleFadeAge25 = double.MaxValue;
                styleGroup.alpha = blockAlpha > 0 ? Mathf.Clamp01((float)((styleFadeAge25 + frameDt) / UnityTuning.StyleMeterFadeIn25)) : 0f;
            }
            else if (blockAlpha > 0 && styleFadeAge25 < UnityTuning.StyleMeterFadeIn25)
            {
                styleFadeAge25 += frameDt;
                styleGroup.alpha = Mathf.Clamp01((float)((styleFadeAge25 + frameDt) / UnityTuning.StyleMeterFadeIn25));
            }
            // NIGHT18 U3-31.6 round 3's picks ("[N18] the D only fades in place, faintly" vs "[N17] the rank D ~1.7x and white,
            // settles to cyan"): the block's first showing (FeelAB on) SLAMS whole: HudSlam18, about 1.6x down to 1 at full
            // alpha, the letter white-hot for its first frames then its ladder colour (below); drawn at its age, then stepped.
            // A rank-up keeps its grow-in pop.
            float blockScale18 = 1f;
            Vector2 blockOff18 = default;
            styleBlockWhite18 = false;
            if (FeelAB.On && blockAlpha > 0 && styleBlockEnterAge18 < UnityTuning.Hud18SlamTime)
            {
                blockScale18 = HudSlam18.Scale(styleBlockEnterAge18);
                styleBlockWhite18 = HudSlam18.White(styleBlockEnterAge18);
                styleBlockEnterAge18 += frameDt;
            }
            if (blockScale18 != styleBlockShownScale18 || blockOff18 != styleBlockShownOff18)
            {
                styleBlockShownScale18 = blockScale18;
                styleBlockShownOff18 = blockOff18;
                styleRoot.localScale = new Vector3(blockScale18, blockScale18, 1);
                styleRoot.anchoredPosition = styleRootHome18 + blockOff18;
            }
            bool still = game.settings.View.reducedMotion;
            var sp = st.Params;
            int count = sp.letters.Count;
            int rank = Math.Max(0, Math.Min(count - 1, st.rank));
            if (rank != styleShownRank)
            {
                bool up = styleShownRank >= 0 && rank > styleShownRank;
                bool down = styleShownRank >= 0 && rank < styleShownRank;
                styleShownRank = rank;
                string letter = sp.letters[rank];
                styleLetter.text = letter;
                styleLetterOffset.text = letter;
                styleLetterGlow.text = letter;
                styleWord.text = sp.ranks[rank];
                int step = StyleLadder.Step(rank, count);
                Color c = StyleLadderColor(rank, count);
                styleLetter.color = c;
                styleLetterGlow.enabled = step == 2;
                styleShownStep = step;
                styleShownBarTint = -1;
                if (up)
                {
                    // round 1's fix list: the screen's event for its 100 ms: the letter slams from 3x, the word from 1.8x
                    styleSlamAge = 0; // round 3: 2.2x white-hot, held 100 ms, then ease-out-back over 160 ms (StyleLadder.SlamScale)
                    styleWordSlam.Hit(UnityTuning.Hud17StyleWordSlamFrom, UnityTuning.Hud17StyleWordSlamTime);
                    styleWordAge = 0;
                    styleStreakAge = 0; // U3-30.10 (a): the streak behind it, in the new rank's ladder colour
                    StyleStreakLayout();
                    styleStreakColor = StyleLadderColor(rank, count) == UiTheme.Muted ? UiTheme.CyanPale : StyleLadderColor(rank, count);
                    if (!still) styleShakeLeft = UnityTuning.Hud17StyleShakeTime; // the whole HUD shakes (rank-ups only)
                    HudFx20.DoShine(styleLetter, still); // NIGHT20 row 5: one shine sweeps the new letter (UIEffect's Shiny; rank-ups only)
                    SheenStart22(still); // NIGHT22 D-7: the Player (no UIEffect) gets a built-in sheen band instead (UiStamp22.cs)

                }
                else if (down) styleKnockAge = 0;
            }

            // The letter: slam x knock x shake (a white frame on a rank-up).
            float s = 1f;
            if (FeelAB.On)
            {
                // NIGHT18 U3-31.6: a rank-up pops the letter from Hud18RankUpFrom past its size (UiTween.PopIn), white for its
                // first Hud18RankUpWhite s (FLASH INTENSITY 0: CYAN-HOT), drawn at its age then stepped
                if (styleSlamAge < UnityTuning.Hud18RankUpIn)
                {
                    s = UiTween.PopIn(styleSlamAge / UnityTuning.Hud18RankUpIn, UnityTuning.Hud18RankUpFrom, UnityTuning.Hud18RankUpPeak, UnityTuning.Hud18RankUpPeakAt);
                    Color w = styleSlamAge < UnityTuning.Hud18RankUpWhite ? (game.settings.Num("flashIntensity") > 0 ? UiTheme.White : UiTheme.CyanHot) : StyleLadderColor(rank, count);
                    if (styleLetter.color != w) styleLetter.color = w;
                    styleSlamAge += frameDt;
                    if (styleSlamAge >= UnityTuning.Hud18RankUpIn) styleLetter.color = StyleLadderColor(rank, count);
                }
                else if (styleBlockWhite18 != styleBlockWhiteShown18)
                {
                    // round 3: the block's first slam: the letter white-hot (FLASH INTENSITY 0: CYAN-HOT), then its ladder colour
                    styleBlockWhiteShown18 = styleBlockWhite18;
                    styleLetter.color = styleBlockWhite18 ? (game.settings.Num("flashIntensity") > 0 ? UiTheme.White : UiTheme.CyanHot) : StyleLadderColor(rank, count);
                }
            }
            else if (styleSlamAge < UnityTuning.Hud17StyleSlamHold + UnityTuning.Hud17StyleSlamTime)
            {
                s = (float)StyleLadder.SlamScale(styleSlamAge);
                // white-hot through the hold (FLASH INTENSITY 0: CYAN-HOT), the ladder's colour after it
                Color w = StyleLadder.SlamWhite(styleSlamAge) ? (game.settings.Num("flashIntensity") > 0 ? UiTheme.White : UiTheme.CyanHot) : StyleLadderColor(rank, count);
                if (styleLetter.color != w) styleLetter.color = w;
                styleSlamAge += frameDt;
                if (styleSlamAge >= UnityTuning.Hud17StyleSlamHold + UnityTuning.Hud17StyleSlamTime) styleLetter.color = StyleLadderColor(rank, count);
            }
            if (styleKnockAge < UnityTuning.Hud17StyleKnockTime) styleKnockAge += frameDt;
            float knock = StyleLadder.KnockY(styleKnockAge);
            float tip = UnityTuning.Hud17StyleKnockDeg * (knock / -UnityTuning.Hud17StyleKnockPx);
            if (s != styleShownScale || knock != styleShownKnock)
            {
                // written only while the letter moves (an idle HUD doesn't re-batch its canvas)
                styleShownScale = s;
                styleShownKnock = knock;
                styleLetterBox.localScale = new Vector3(s, s, 1);
                styleLetterBox.anchoredPosition = new Vector2(UnityTuning.Hud17StyleLetterWidth, knock);
                styleLetterBox.localRotation = Quaternion.Euler(0, 0, tip);
            }
            // The rank-up's HUD shake: the frame and values layers (the crosshair's layer stays put), Hud17StyleShakePx for
            // Hud17StyleShakeTime, decaying; REDUCED MOTION never starts it. Moving a stretched layer's anchoredPosition shifts
            // both its offsets, so HUD SCALE's symmetric offsets come back exactly at 0.
            float shake = 0, shakeY = 0;
            if (styleShakeLeft > 0)
            {
                styleShakeLeft -= frameDt;
                double k = 1 - Math.Max(0, styleShakeLeft) / UnityTuning.Hud17StyleShakeTime;
                shake = UnityTuning.Hud17StyleShakePx * UiTween.Shake(k, 3);
                shakeY = UnityTuning.Hud17StyleShakePx * 0.6f * UiTween.Shake(k + 0.17, 2);
            }
            if (shake != styleShownShake)
            {
                styleShownShake = shake;
                var off = new Vector2(shake, shakeY);
                frame.anchoredPosition = off;
                values.anchoredPosition = off;
            }
            if (!FeelAB.On)
            {
                float ws = styleWordSlam.Step(frameDt);
                if (ws != styleWordRt.localScale.x) styleWordRt.localScale = new Vector3(ws, ws, 1); // only while it moves
            }
            if (styleShownStep == 2 && !still)
            {
                // the top rungs' glow breathes (alpha only)
                styleGlowClock = (styleGlowClock + frameDt) % 1.0;
                styleLetterGlow.alpha = UnityTuning.Hud17StyleGlowAlpha * (0.7f + 0.3f * Mathf.Sin((float)styleGlowClock * Mathf.PI * 2f));
            }

            // The word (IDENTITY.md's tile: always there, DISPLAY cyan): a rank-up slides it in along the slant (ease-out-back)
            // and shows it WHITE for 600 ms (StyleLadder.WordShown), then it settles to CYAN.
            if (styleWordAge < UnityTuning.Hud17StyleWordTime + frameDt)
            {
                styleWordAge += frameDt;
                bool hot = StyleLadder.WordShown(styleWordAge);
                float e = UiTween.OutBack(styleWordAge / UnityTuning.Hud17StyleWordIn);
                styleWordGroup.alpha = Mathf.Clamp01(e);
                styleWordRt.anchoredPosition = new Vector2(UnityTuning.Hud17StyleColumn + UnityTuning.Hud17StyleWordSlide * (1 - e), UnityTuning.Hud17StyleWordY);
                if (FeelAB.On)
                {
                    // NIGHT18 U3-31.6: the word enters (grows from 0.6x with its overshoot) instead of slamming down from 1.8x
                    float ws18 = HudEntrance18.Scale(styleWordAge);
                    if (ws18 != styleWordRt.localScale.x) styleWordRt.localScale = new Vector3(ws18, ws18, 1);
                }
                Color wc = hot ? UiTheme.White : UiTheme.Cyan;
                if (styleWord.color != wc) styleWord.color = wc;
            }

            // The decay bar (the band's fill), DANGER for a moment after a hurt.
            styleBarFill.Fill = (float)st.Fill;
            styleDangerLeft = Math.Max(0, styleDangerLeft - frameDt);
            int tint = styleDangerLeft > 0 ? 1 : 0;
            if (tint != styleShownBarTint)
            {
                styleShownBarTint = tint;
                styleBarFill.color = tint == 1 ? UiTheme.Danger : StyleLadderColor(rank, count) == UiTheme.Muted ? UiTheme.CyanPale : StyleLadderColor(rank, count);
            }

            RenderStyleChip(st, frameDt);
            RenderStyleCallouts(frameDt);
            RenderStyleStreak(frameDt);
            RenderSheen22(frameDt);
        }

        /// <summary>
        /// Round 2's fix list: the band from Hud17StreakLeft of the canvas width (right of the crosshair's central 40 %) to
        /// Hud17StreakOver px past the right edge, its pivot at the letter's middle so the 12 deg turn stays behind the column.
        /// </summary>
        void StyleStreakLayout()
        {
            RectTransform rt = styleStreak.rectTransform;
            float w = values.rect.width;
            float letterFromRight = UnityTuning.ThemeSafeInset + UnityTuning.Hud17StyleRight + UnityTuning.Hud17StyleWidth - UnityTuning.Hud17StyleLetterWidth * 0.5f; // the letter's middle, px in from the right edge
            float span = w * (1f - UnityTuning.Hud17StreakLeft) + UnityTuning.Hud17StreakOver; // from the left end to past the right edge
            float leftOfLetter = Mathf.Max(0f, w * (1f - UnityTuning.Hud17StreakLeft) - letterFromRight);
            rt.sizeDelta = new Vector2(span, UnityTuning.Hud17StreakHeight);
            rt.pivot = new Vector2(Mathf.Clamp01(leftOfLetter / Mathf.Max(1f, span)), 0.5f); // anchoredPosition (the letter) stays the pivot's place
        }

        /// <summary>
        /// U3-30.10 (a): the rank-up's streak: it sweeps in from the left (its fill, ease-out) over Hud17StreakIn, then thins
        /// and fades (ease-in) over Hud17StreakOut. Shown with the block (the block's alpha) and only while it runs.
        /// </summary>
        void RenderStyleStreak(double frameDt)
        {
            if (styleStreak == null) return;
            bool on = styleStreakAge < UnityTuning.Hud17StreakIn + UnityTuning.Hud17StreakOut && rankPlateAlpha > 0;
            if (styleStreak.enabled != on) styleStreak.enabled = on;
            if (!on) return;
            styleStreakAge += frameDt;
            float a, h;
            if (styleStreakAge < UnityTuning.Hud17StreakIn)
            {
                styleStreak.Fill = UiTween.OutCubic(styleStreakAge / UnityTuning.Hud17StreakIn);
                a = 1f;
                h = 1f;
            }
            else
            {
                float k = UiTween.InCubic((styleStreakAge - UnityTuning.Hud17StreakIn) / UnityTuning.Hud17StreakOut);
                styleStreak.Fill = 1f;
                a = 1f - k;
                h = 1f - 0.7f * k;
            }
            if (FeelAB.On) a *= HudEntrance18.Fade(styleStreakAge); // NIGHT18 U3-31.6: it fades in as it sweeps (the reel sees its rect, not its fill; round 1: 95 % at ~126 ms)
            styleStreak.color = UiTheme.A(styleStreakColor, UnityTuning.Hud17StreakAlpha * a * rankPlateAlpha);
            styleStreak.rectTransform.localScale = new Vector3(1f, h, 1f);
        }

        void RenderStyleChip(StyleMeter st, double frameDt)
        {
            Mover m = game.mover;
            string state = m != null ? PortData.Name(m.state) : null;
            double mult = state != null ? StyleMath.StateMultiplier(state, st.Params) : 1;
            bool live = state != null && StyleChip.Live(st.enabled && game.flow == Flow.Playing, state, mult);
            if (live)
            {
                if (!styleChipOn || !ReferenceEquals(state, styleChipState))
                {
                    if (!styleChipLabels.TryGetValue(state, out string label))
                        styleChipLabels[state] = label = "×" + JsNumber.ToString(mult) + " " + state; // once per state
                    styleChipText.text = label;
                    if (!styleChipOn)
                    {
                        styleChipSlam.Hit(UnityTuning.Hud17ChipFrom, UnityTuning.Hud17ChipSlam);
                        if (game.settings.Num("flashIntensity") > 0) styleChipWhite = UnityTuning.Hud17ChipWhite;
                        styleChipEnterAge18 = 0; // NIGHT18 U3-31.6: it enters (FeelAB on)
                    }
                    styleChipState = state;
                    styleChipOn = true;
                }
                styleChipSince = 0;
            }
            else if (styleChipOn)
            {
                styleChipSince += frameDt;
                if (StyleChip.Drain(styleChipSince) <= 0) styleChipOn = false;
            }
            float a = styleChipOn ? 1 : 0;
            // NIGHT18 U3-31.6 (FeelAB on): the chip enters (grows from 0.6x and fades in) instead of slamming down from 1.6x
            float chipScale18 = 1f;
            if (FeelAB.On && styleChipOn && !HudEntrance18.Done(styleChipEnterAge18))
            {
                styleChipEnterAge18 = HudEntrance18.Step(styleChipEnterAge18, frameDt);
                chipScale18 = HudEntrance18.Scale(styleChipEnterAge18);
                a = HudEntrance18.Alpha(styleChipEnterAge18);
            }
            if (styleChipGroup.alpha != a) styleChipGroup.alpha = a;
            if (!styleChipOn) return;
            float cs = FeelAB.On ? chipScale18 : styleChipSlam.Step(frameDt);
            if (cs != styleShownChipScale)
            {
                styleShownChipScale = cs;
                styleChipRt.localScale = new Vector3(cs, cs, 1);
            }
            styleChipBar.Fill = (float)StyleChip.Drain(styleChipSince); // UiShape rebuilds only when the fill changes
            styleChipWhite = Math.Max(0, styleChipWhite - frameDt);
            Color plate = styleChipWhite > 0 ? UiTheme.White : UiTheme.Ink;
            if (styleChipPlate.color != plate)
            {
                styleChipPlate.color = plate;
                styleChipText.color = styleChipWhite > 0 ? UiTheme.InkSolid : UiTheme.White;
            }
        }

        /// <summary>A line on top of the callouts (the older ones move down; the oldest past the last drops off).</summary>
        void StyleCalloutPush(string line, bool dim, bool streak, bool combo = false)
        {
            int n = styleCalloutText.Length;
            int seen = streak ? -1 : StyleLadder.Repeats(styleCalloutLine, styleCalloutCount, line);
            int repeat = 1;
            bool streakMoved18 = false; // NIGHT18: a live streak line moves back to the top with its own text object
            if (seen >= 0)
            {
                // round 5: the same award again counts on its line ("+ HEADSHOT x2"), which goes back on top
                repeat = styleCalloutRepeat[seen] + 1;
                StyleCalloutObjRemove18(seen);
                for (int i = seen; i < styleCalloutCount - 1; i++)
                {
                    styleCalloutLine[i] = styleCalloutLine[i + 1];
                    styleCalloutAge[i] = styleCalloutAge[i + 1];
                    styleCalloutDim[i] = styleCalloutDim[i + 1];
                    styleCalloutStreak[i] = styleCalloutStreak[i + 1];
                    styleCalloutRepeat[i] = styleCalloutRepeat[i + 1];
                    styleCalloutCombo[i] = styleCalloutCombo[i + 1];
                }
                styleCalloutCount--;
            }
            if (streak)
            {
                // round 2's reviewer: a streak's name prints once: a live copy of it leaves before it goes on top again
                for (int j = 0; j < styleCalloutCount; j++)
                {
                    if (!ReferenceEquals(styleCalloutLine[j], line)) continue;
                    StyleCalloutObjRemove18(j);
                    streakMoved18 = true;
                    for (int i = j; i < styleCalloutCount - 1; i++)
                    {
                        styleCalloutLine[i] = styleCalloutLine[i + 1];
                        styleCalloutAge[i] = styleCalloutAge[i + 1];
                        styleCalloutDim[i] = styleCalloutDim[i + 1];
                        styleCalloutStreak[i] = styleCalloutStreak[i + 1];
                        styleCalloutRepeat[i] = styleCalloutRepeat[i + 1];
                        styleCalloutCombo[i] = styleCalloutCombo[i + 1];
                    }
                    styleCalloutCount--;
                    break;
                }
            }
            // NIGHT18: the text objects move down with their lines; the new top line takes the free one (or the dropped line's)
            int free18 = styleCalloutObj18[Math.Min(styleCalloutCount, n - 1)];
            for (int i = Math.Min(styleCalloutCount, n - 1); i > 0; i--) styleCalloutObj18[i] = styleCalloutObj18[i - 1];
            styleCalloutObj18[0] = free18;
            if (seen < 0 && !streakMoved18) styleCalloutFresh18[free18] = true; // a new line (a reused object snaps to the top slot)
            for (int i = Math.Min(styleCalloutCount, n - 1); i > 0; i--)
            {
                styleCalloutLine[i] = styleCalloutLine[i - 1];
                styleCalloutAge[i] = styleCalloutAge[i - 1];
                styleCalloutDim[i] = styleCalloutDim[i - 1];
                styleCalloutStreak[i] = styleCalloutStreak[i - 1];
                styleCalloutRepeat[i] = styleCalloutRepeat[i - 1];
                styleCalloutCombo[i] = styleCalloutCombo[i - 1];
            }
            styleCalloutRepeat[0] = repeat;
            styleCalloutLine[0] = line;
            styleCalloutAge[0] = 0;
            styleCalloutDim[0] = dim;
            styleCalloutStreak[0] = streak;
            styleCalloutCombo[0] = combo;
            styleCalloutCount = Math.Min(n, styleCalloutCount + 1);
            styleCalloutsDirty = true;
        }

        /// <summary>The column's INK plate: from over the word down past the last live callout (on a change of their count only).</summary>
        void StyleColumnFit(int count)
        {
            if (styleColPlate == null || count == styleColShownCount) return;
            styleColShownCount = count;
            float pad = UnityTuning.Hud17StylePlatePad;
            float bottom = count > 0
                ? styleCalloutTopY - (count - 0.5f) * UnityTuning.Hud17CalloutLine - pad * 0.5f
                : Math.Min(-UnityTuning.Hud17StyleLetterCap * 0.5f, UnityTuning.Hud17StyleBarY - UnityTuning.Hud17StyleBarHeight) - pad * 0.5f;
            bottom = Math.Min(bottom, -UnityTuning.Hud17StyleLetterCap * 0.5f - pad * 0.5f); // the letter at rest always on it
            RectTransform rt = styleColPlate.rectTransform;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, styleColTop - bottom);
        }

        /// <summary>NIGHT18: a line leaves slot `slot`: its text object goes to the end of the live ones (then free).</summary>
        void StyleCalloutObjRemove18(int slot)
        {
            int removed = styleCalloutObj18[slot];
            for (int i = slot; i < styleCalloutCount - 1; i++) styleCalloutObj18[i] = styleCalloutObj18[i + 1];
            styleCalloutObj18[styleCalloutCount - 1] = removed;
        }

        /// <summary>
        /// NIGHT19 U3-33.8: the lines age; each leaves at its own life (ComboCallout19.Expired: a combo's Hud19ComboLife, an
        /// award's styleFeedLife), so a combo can outlive a newer award line under it; the lines past it close up a slot.
        /// `objs`: NIGHT18's text objects follow their lines (FeelAB on).
        /// </summary>
        void StyleCalloutExpire19(double frameDt, bool objs)
        {
            for (int i = styleCalloutCount - 1; i >= 0; i--)
            {
                styleCalloutAge[i] += frameDt;
                if (!ComboCallout19.Expired(styleCalloutAge[i], styleCalloutCombo[i], styleFeedLife)) continue;
                if (objs) StyleCalloutObjRemove18(i);
                for (int j = i; j < styleCalloutCount - 1; j++)
                {
                    styleCalloutLine[j] = styleCalloutLine[j + 1];
                    styleCalloutAge[j] = styleCalloutAge[j + 1];
                    styleCalloutDim[j] = styleCalloutDim[j + 1];
                    styleCalloutStreak[j] = styleCalloutStreak[j + 1];
                    styleCalloutRepeat[j] = styleCalloutRepeat[j + 1];
                    styleCalloutCombo[j] = styleCalloutCombo[j + 1];
                }
                styleCalloutCount--;
                styleCalloutsDirty = true;
            }
        }

        /// <summary>
        /// NIGHT19 U3-33.8: text object k's combo plate: CYAN (WHITE for its first frames), slanted and cut, behind the name
        /// (measured when the lines change), at the text's place, scale and alpha; hidden when the line isn't a combo.
        /// </summary>
        void StyleComboPlate19(int k, bool on, Vector2 at, float scale, float alpha, int white)
        {
            UiShape p = styleCalloutPlate19[k];
            if (!on)
            {
                if (p.enabled) p.enabled = false;
                styleCalloutPlateKey19[k] = -1;
                return;
            }
            if (!p.enabled) p.enabled = true;
            RectTransform rt = p.rectTransform;
            if (styleCalloutsDirty || styleCalloutPlateKey19[k] < 0)
            {
                float w = styleCalloutText[k].GetPreferredValues().x; // on a change of the lines only
                rt.sizeDelta = new Vector2(w + 2 * UnityTuning.Hud19ComboPadX, UnityTuning.Hud17CalloutLine - 2 * UnityTuning.Hud19ComboPlateInset);
            }
            var pos = new Vector2(at.x - UnityTuning.Hud19ComboPadX, at.y);
            if (rt.anchoredPosition != pos) rt.anchoredPosition = pos;
            if (rt.localScale.x != scale) rt.localScale = new Vector3(scale, scale, 1);
            Color c = UiTheme.A(white == 1 ? UiTheme.White : UiTheme.Cyan, alpha);
            if (p.color != c) p.color = c;
            styleCalloutPlateKey19[k] = white;
        }

        /// <summary>
        /// NIGHT18 U3-31.6 (FeelAB on): each live line on its own text object: a new one enters (grows from 0.6x, fades in,
        /// slides in from the right: HudEntrance18) at its slot; older ones ease down a slot (Hud18CalloutFollow) and keep
        /// their look; sizes and colours by slot as NIGHT17's (the newest S24 WHITE, the older S18 CYAN).
        /// </summary>
        void RenderStyleCallouts18(double frameDt)
        {
            styleCallouts18Drawn = true;
            int n = styleCalloutText.Length;
            StyleCalloutExpire19(frameDt, true); // NIGHT19: each line by its own life (a combo's is longer)
            for (int k = 0; k < n; k++) styleCalloutUsed18[k] = false;
            for (int i = 0; i < styleCalloutCount; i++) styleCalloutUsed18[styleCalloutObj18[i]] = true;
            for (int k = 0; k < n; k++)
            {
                if (styleCalloutUsed18[k]) continue;
                if (styleCalloutText[k].enabled) styleCalloutText[k].enabled = false;
                StyleComboPlate19(k, false, default, 0, 1f, 0);
            }
            float follow = 1f - Mathf.Exp(-(float)frameDt * UnityTuning.Hud18CalloutFollow);
            bool flashes = game.settings.Num("flashIntensity") > 0;
            for (int i = 0; i < styleCalloutCount; i++)
            {
                int k = styleCalloutObj18[i];
                TextMeshProUGUI t = styleCalloutText[k];
                bool combo = styleCalloutCombo[i];
                float slotY = styleCalloutTopY - i * UnityTuning.Hud17CalloutLine;
                float slotPx = UiTheme.Px(i == 0 || combo ? UiTheme.Size.S24 : UiTheme.Size.S18); // NIGHT19: a combo keeps S24 in any slot
                if (!t.enabled || styleCalloutFresh18[k])
                {
                    t.enabled = true;
                    styleCalloutFresh18[k] = false;
                    styleCalloutY18[k] = slotY;
                    styleCalloutPx18[k] = slotPx;
                }
                else
                {
                    styleCalloutY18[k] += (slotY - styleCalloutY18[k]) * follow;
                    // the lead's fix list: no 24 -> 18 px jump as a line moves down a slot: its size eases with its place
                    styleCalloutPx18[k] += (slotPx - styleCalloutPx18[k]) * follow;
                    if (Math.Abs(slotPx - styleCalloutPx18[k]) < UnityTuning.Hud18CalloutPxSnap) styleCalloutPx18[k] = slotPx;
                }
                float shownPx = Mathf.Round(styleCalloutPx18[k] * 2f) * 0.5f; // half-pixel steps: the mesh rebuilds only while it moves
                if (t.fontSize != shownPx) t.fontSize = shownPx;
                if (styleCalloutsDirty)
                {
                    if (styleCalloutRepeat[i] > 1)
                    {
                        if (!styleCalloutRepeatFormat.TryGetValue(styleCalloutLine[i], out string fmt))
                            styleCalloutRepeatFormat[styleCalloutLine[i]] = fmt = styleCalloutLine[i] + " ×{0:0}";
                        t.SetText(fmt, styleCalloutRepeat[i]);
                    }
                    else t.text = styleCalloutLine[i];
                    t.color = combo ? UiTheme.InkSolid : styleCalloutStreak[i] ? UiTheme.CyanHot : styleCalloutDim[i] ? UiTheme.Muted : i == 0 ? UiTheme.White : UiTheme.Cyan;
                }
                double age = styleCalloutAge[i];
                // NIGHT19 U3-33.8: a combo SLAMS in place, whole from its first frame (ComboCallout19); an award's line enters
                float s = combo ? ComboCallout19.Scale(age) : HudEntrance18.Scale(age);
                float x = combo ? UnityTuning.Hud17StyleColumn : UnityTuning.Hud17StyleColumn + Mathf.Round(UnityTuning.Hud17CalloutSlide * HudEntrance18.Slide(age));
                t.rectTransform.anchoredPosition = new Vector2(x, styleCalloutY18[k]);
                if (t.rectTransform.localScale.x != s) t.rectTransform.localScale = new Vector3(s, s, 1);
                double fadeFrom = UnityTuning.Hud17CalloutFadeAt;
                float a = combo ? ComboCallout19.Alpha(age, true) * UiFade25.In(age, frameDt) : // NIGHT25 (T-12, M9): the combo slam keeps its scale, its alpha ramps in
                     (age <= fadeFrom ? 1 : 1 - UiTween.InCubic((age - fadeFrom) / Math.Max(1e-4, styleFeedLife - fadeFrom))) * HudEntrance18.Alpha(age);
                if (t.alpha != a) t.alpha = a;
                StyleComboPlate19(k, combo, new Vector2(x, styleCalloutY18[k]), s, a, combo && flashes && ComboCallout19.White(age) ? 1 : 0);
            }
            if (styleCalloutsDirty) StyleColumnFit(styleCalloutCount);
            styleCalloutsDirty = false;
        }

        /// <summary>NIGHT18: a live F6 flip back to NIGHT17: each text object to its own slot, size and scale again.</summary>
        void StyleCallouts18Restore()
        {
            styleCallouts18Drawn = false;
            int n = styleCalloutText.Length;
            for (int k = 0; k < n; k++)
            {
                TextMeshProUGUI t = styleCalloutText[k];
                t.fontSize = UiTheme.Px(k == 0 ? UiTheme.Size.S24 : UiTheme.Size.S18);
                t.rectTransform.localScale = Vector3.one;
                t.rectTransform.anchoredPosition = new Vector2(UnityTuning.Hud17StyleColumn, styleCalloutTopY - k * UnityTuning.Hud17CalloutLine);
                styleCalloutObj18[k] = k;
            }
            styleCalloutsDirty = true;
        }

        void RenderStyleCallouts(double frameDt)
        {
            if (FeelAB.On)
            {
                RenderStyleCallouts18(frameDt);
                return;
            }
            if (styleCallouts18Drawn) StyleCallouts18Restore();
            int n = styleCalloutText.Length;
            StyleCalloutExpire19(frameDt, false); // NIGHT19: each line by its own life (a combo's is longer)
            for (int i = 0; i < n; i++)
            {
                TextMeshProUGUI t = styleCalloutText[i];
                bool on = i < styleCalloutCount;
                if (t.enabled != on) t.enabled = on;
                // NIGHT19 U3-33.8: a combo line on its CYAN plate (NIGHT17's path: no slam, its colours and plate only)
                bool combo17 = on && styleCalloutCombo[i];
                StyleComboPlate19(i, combo17, t.rectTransform.anchoredPosition, 1f, combo17 ? ComboCallout19.Alpha(styleCalloutAge[i], true) : 0f, 0);
                if (!on) continue;
                if (combo17)
                {
                    if (styleCalloutsDirty)
                    {
                        t.text = styleCalloutLine[i];
                        t.color = UiTheme.InkSolid;
                    }
                    float ca = ComboCallout19.Alpha(styleCalloutAge[i], true);
                    if (t.alpha != ca) t.alpha = ca;
                    continue;
                }
                if (styleCalloutsDirty)
                {
                    if (styleCalloutRepeat[i] > 1)
                    {
                        // "+ HEADSHOT x2": the line's format made once per award name
                        if (!styleCalloutRepeatFormat.TryGetValue(styleCalloutLine[i], out string fmt))
                            styleCalloutRepeatFormat[styleCalloutLine[i]] = fmt = styleCalloutLine[i] + " \u00d7{0:0}";
                        t.SetText(fmt, styleCalloutRepeat[i]);
                    }
                    else t.text = styleCalloutLine[i];
                    // the tile: the newest white, the older cyan; a kill streak's name CYAN-HOT wherever it is
                    t.color = styleCalloutStreak[i] ? UiTheme.CyanHot : styleCalloutDim[i] ? UiTheme.Muted : i == 0 ? UiTheme.White : UiTheme.Cyan;
                }
                double age = styleCalloutAge[i];
                // round 3's reviewer: each line slides in from the right over 120 ms (StyleLadder.CalloutX)
                float cx = UnityTuning.Hud17StyleColumn + (float)Math.Round(StyleLadder.CalloutX(age));
                Vector2 at = t.rectTransform.anchoredPosition;
                if (at.x != cx) t.rectTransform.anchoredPosition = new Vector2(cx, at.y);
                double fadeFrom = UnityTuning.Hud17CalloutFadeAt;
                float a = age <= fadeFrom ? 1 : 1 - UiTween.InCubic((age - fadeFrom) / Math.Max(1e-4, styleFeedLife - fadeFrom));
                if (t.alpha != a) t.alpha = a;
            }
            if (styleCalloutsDirty) StyleColumnFit(styleCalloutCount);
            styleCalloutsDirty = false;
        }

        // --- the rebuilt HUD's small builders (NIGHT17 U3-30.7; topic-named: PORT-RULES 11) ------------------------------------

        /// <summary>An empty rect.</summary>
        static RectTransform Hud17Rect(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        /// <summary>A UiShape (the theme's slanted, cut solid) in `c`.</summary>
        static UiShape Hud17Shape(Transform parent, string name, Color c, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UiShape));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var s = go.GetComponent<UiShape>();
            s.color = c;
            s.raycastTarget = false;
            return s;
        }

        /// <summary>Round 2's reviewer: a 4-5 px INK stroke (TMP outline) so a text reads over anything without a plate.</summary>
        static void Hud17Stroke(TextMeshProUGUI t)
        {
            t.outlineWidth = UnityTuning.Hud17PopOutline;
            t.outlineColor = UiTheme.InkSolid;
        }

        /// <summary>A text styled by the theme (face, size on the scale, colour, the hard INK shadow), never wrapping.</summary>
        static TextMeshProUGUI Hud17Text(Transform parent, string name, string s, float px, Color c, TMP_FontAsset face, TextAlignmentOptions align, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 box, bool shadow)
        {
            TextMeshProUGUI t = Text(parent, name, s, px, c, align, pos, box, anchor);
            t.rectTransform.pivot = pivot;
            t.rectTransform.anchoredPosition = pos;
            UiTheme.Style(t, px, c, face, shadow);
            t.fontStyle = FontStyles.Normal; // the face carries its weight and italic (no faux bold / italic)
            t.overflowMode = TextOverflowModes.Overflow;
            return t;
        }
    }
    /// <summary>The style meter's cyan value ladder, its word and its knock-down (Hud17Tests).</summary>
    public static class StyleLadder
    {
        /// <summary>The rank's rung on the cyan value ladder: 0 pale (the low third), 1 cyan, 2 white-hot with a glow (the top third).</summary>
        public static int Step(int rank, int count)
        {
            if (count <= 0) return 0;
            int r = Math.Max(0, Math.Min(count - 1, rank));
            return Math.Min(2, r * 3 / count);
        }

        /// <summary>The rank's word shows for Hud17StyleWordTime s after a rank-up (ULTRAKILL's letter + word).</summary>
        public static bool WordShown(double sinceRankUp) => sinceRankUp >= 0 && sinceRankUp < UnityTuning.Hud17StyleWordTime;

        /// <summary>Round 3's reviewer: the rank-up slam: Hud17StyleSlamFrom (2.2x) held Hud17StyleSlamHold, then ease-out-back to 1 over Hud17StyleSlamTime.</summary>
        public static double SlamScale(double t)
        {
            if (t < UnityTuning.Hud17StyleSlamHold) return UnityTuning.Hud17StyleSlamFrom;
            double k = (t - UnityTuning.Hud17StyleSlamHold) / UnityTuning.Hud17StyleSlamTime;
            if (k >= 1) return 1;
            return UnityTuning.Hud17StyleSlamFrom + (1 - UnityTuning.Hud17StyleSlamFrom) * UiTween.OutBack(k);
        }

        /// <summary>White-hot through the slam's hold.</summary>
        public static bool SlamWhite(double t) => t >= 0 && t < UnityTuning.Hud17StyleSlamHold;

        /// <summary>Round 3's reviewer: named events only: never a plain KILL or HIT; MULTIKILL is the streak's name (DOUBLE ...).</summary>
        public static bool Named(string award) => award != null && award != "KILL" && award != "HIT" && award != "MULTIKILL";

        /// <summary>Round 5: the live line (index under `live`) of the same text, or -1 ("+ HEADSHOT" twice becomes "+ HEADSHOT x2").</summary>
        public static int Repeats(string[] lines, int live, string line)
        {
            for (int i = 0; i < live && i < lines.Length; i++) if (string.Equals(lines[i], line, StringComparison.Ordinal)) return i;
            return -1;
        }

        /// <summary>A callout's x offset: from Hud17CalloutSlide px right to 0 over Hud17CalloutSlideTime (ease-out cubic).</summary>
        public static float CalloutX(double age) => UnityTuning.Hud17CalloutSlide * (1f - UiTween.OutCubic(age / UnityTuning.Hud17CalloutSlideTime));

        /// <summary>px the letter sits low `age` s after a drop: knocked down Hud17StyleKnockPx, back with an ease-out.</summary>
        public static float KnockY(double age) => -UnityTuning.Hud17StyleKnockPx * (1f - UiTween.OutCubic(age / UnityTuning.Hud17StyleKnockTime));
    }

    /// <summary>
    /// U3-30.10 (b): kill streaks as named lines in the style feed (Hud17Tests). Named lines only: the style scoring stays the
    /// web's (StyleCore pays MULTIKILL from its own kill times; this counts the player's kills inside the same window).
    /// </summary>
    public static class KillStreak
    {
        /// <summary>Keep the kills inside `window` s of `now` (the core's `now - t &lt;= window`), add this one, return how many.
        /// A full buffer drops its oldest. No allocation.</summary>
        public static int Push(double[] times, ref int n, double now, double window)
        {
            int m = 0;
            for (int i = 0; i < n; i++) if (now - times[i] <= window) times[m++] = times[i];
            n = m;
            if (n == times.Length)
            {
                for (int i = 1; i < n; i++) times[i - 1] = times[i];
                n--;
            }
            times[n++] = now;
            return n;
        }

        /// <summary>DOUBLE at 2, TRIPLE at 3, MASSACRE at 5 and every fifth after; no line otherwise.</summary>
        public static string Name(int count) =>
            count == 2 ? "DOUBLE" : count == 3 ? "TRIPLE" : count >= 5 && count % 5 == 0 ? "MASSACRE" : null;
    }

    /// <summary>The multiplier chip's rules (Hud17Tests).</summary>
    public static class StyleChip
    {
        /// <summary>The chip shows while the meter runs, the player is in a state that pays the state multiplier, and it is above 1.</summary>
        public static bool Live(bool enabled, string state, double mult) => enabled && mult > 1 && Skygrind.Core.StyleMath.IsMultState(state);

        /// <summary>Its drain bar `sinceLeft` s after the state ended: 1 -> 0 over Hud17ChipLinger.</summary>
        public static double Drain(double sinceLeft) => 1 - Math.Max(0, Math.Min(1, sinceLeft / UnityTuning.Hud17ChipLinger));
    }
}
