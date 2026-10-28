// NIGHT12 U3-25.6 (design.ceremony.round; Research/2026-10-03-polish.md F1: CoD Zombies' round change): display only.
// - The round counter at rest, bottom left (CoD's corner), hidden before round 1.
// - roundClear: the counter flashes white RoundClearFlashes times over RoundClearTime s (CoD: the marks flash white).
// - roundBegin: the new round as a ceremony at the centre, then it flies to the counter. It replaces the ROUND N banner.
// - The port-only event MetaEvents.RoundCeremony at each beat (phase "clear": the flash starts; "burn": the ceremony
//   starts), emitted on frame time from one reused payload (GG binds the sounds).
// NIGHT17 U3-30.7 (builder H): the round as a HERO NUMBER (Research/2026-10-05-overhaul.md section 2: 133-160 px at the
// centre for 1.5 s, then a 400 ms tween to its corner):
// - the numeral in UiTheme.Rank at Hud17RoundHeroPx (150), WHITE with the hero offset copy in CYAN-DEEP (ThemeHeroOffset px
//   down-right; GOLD is points only now, so the counter is no longer gold), two digits ("07": IDENTITY.md's tile), slammed
//   in from 1.7x over 120 ms, "ROUND" (S24, CYAN-PALE) under it through the hold (gone at rest: the glance tier has no
//   labels), then an ease-in-out-cubic flight of 400 ms to its corner, shrinking to S56 (RoundHeroMath);
// - its corner: right of the vitals plate, bottom left (clear of the perks' row over the plate);
// - CoD's tally strokes (rounds 1-5) are retired: the numeral is the identity's hero number from round 1; at rest a small
//   ROUND (S18 MUTED) sits over it (round 1's reviewer);
// - roundClear (round 1's reviewer): "ROUND N CLEAR" (S75 DISPLAY, WHITE, its CYAN-DEEP offset) slams in at the centre
//   1.6 -> 1.0 (ease-out-back, 160 ms), holds, and is gone by 1.2 s (RoundClearMath); the round's bonus pops under it and
//   tallies into the points counter (UiPointsPops.cs);
// - the clear's flash alternates WHITE and CYAN-PALE.
// Every number is set only when the round changes; the motion writes a position and a scale while it runs.
using System;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform cerMark; // the counter's mark: flies from the centre to its home
        TextMeshProUGUI cerNumeral, cerNumeralOffset, cerLabel;
        int cerRound, cerShownRound = -1, cerClearRound;
        double cerClearT = double.PositiveInfinity, cerBurnT = double.PositiveInfinity;
        bool cerClearPending, cerBurnPending, cerVisible = true;
        float cerMarkWidth;
        int cerShownTint = -1;
        readonly RoundCeremonyEvent cerEvent = new RoundCeremonyEvent();
        TextMeshProUGUI cerRestLabel, cerClearText, cerClearOffset; // NIGHT17 round 1: the corner's ROUND (round 2: unused, hidden); the CLEAR slam
        CanvasGroup cerMarkGroup;
        RectTransform cerClearRoot;
        CanvasGroup cerClearGroup;
        double cerClearSlamT = double.PositiveInfinity;
        float cerClearShownScale = -1;
        UiShape cerClearBand; // round 3's reviewer: the full-width INK band behind ROUND N CLEAR
        TextMeshProUGUI cerTally; // U3-30.10 (c): "14 KILLS · BEST +480" under the CLEAR
        string cerTallyFormat;
        Vector2 cerTallyHome;
        int cerRoundKills;

        void BuildCeremony()
        {
            float px = UnityTuning.Hud17RoundHeroPx;
            cerMark = new GameObject("Round mark", typeof(RectTransform)).GetComponent<RectTransform>();
            cerMark.SetParent(values, false);
            cerMark.anchorMin = cerMark.anchorMax = Vector2.zero;
            cerMark.pivot = new Vector2(0.5f, 0.5f);
            cerMark.sizeDelta = new Vector2(px, px);
            cerMarkGroup = cerMark.gameObject.AddComponent<CanvasGroup>(); // round 2: it fades into the top line
            cerMarkGroup.interactable = cerMarkGroup.blocksRaycasts = false;
            var box = new Vector2(px * 3, px * 1.1f);
            cerNumeralOffset = Hud17Text(cerMark, "Numeral (offset)", "", px, UiTheme.CyanDeep, UiTheme.Rank, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(UnityTuning.ThemeHeroOffset * 1.6f, -UnityTuning.ThemeHeroOffset * 1.6f), box, false);
            cerNumeral = Hud17Text(cerMark, "Numeral", "", px, UiTheme.White, UiTheme.Rank, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, box, true);
            // "ROUND" under the numeral through the hold (IDENTITY.md's tile: "07" over a small CYAN-PALE "ROUND").
            float lpx = UiTheme.Px(UiTheme.Size.S24);
            cerLabel = Hud17Text(cerMark, "Round label", "ROUND", lpx, UiTheme.CyanPale, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(0, px * 0.02f), new Vector2(px * 3, lpx * 1.3f), true);
            cerLabel.characterSpacing = UnityTuning.RoundLabelSpacing;
            // Round 1's reviewer: at rest the corner's numeral gets a small ROUND (S18 MUTED) over it.
            cerRestLabel = Hud17Text(values, "Round label (rest)", "ROUND", UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, UiTheme.BodyBold, TextAlignmentOptions.Center, Vector2.zero, new Vector2(0.5f, 0), Vector2.zero, new Vector2(160, 24), true);
            cerRestLabel.characterSpacing = UnityTuning.ThemeLabelTracking;
            cerRestLabel.enabled = false;
            // Round 1's reviewer: the round's CLEAR: "ROUND N CLEAR" slammed in at the centre (RoundClearMath), with its CYAN-DEEP
            // offset copy; the round's bonus pops under it and tallies into the counter (UiPointsPops.cs).
            // round 3's reviewer: the band first (behind the CLEAR; it doesn't take the CLEAR's scale)
            cerClearBand = Hud17Shape(values, "Round clear band", UiTheme.Ink, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, UnityTuning.Hud17ClearBandY), new Vector2(1920 * UnityTuning.Hud17ClearBandWidth, UnityTuning.Hud17ClearBandH));
            cerClearBand.SetCuts(0, 0);
            cerClearBand.enabled = false;
            if (styleRoot != null) cerClearBand.transform.SetSiblingIndex(styleRoot.GetSiblingIndex()); // round 5: behind the style block's plate
            float cpx = UiTheme.Px(UiTheme.Size.S133); // round 3's reviewer: 120-140 px
            cerClearRoot = Hud17Rect(values, "Round clear", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, UnityTuning.Hud17ClearY), new Vector2(1200, cpx * 1.3f));
            cerClearGroup = cerClearRoot.gameObject.AddComponent<CanvasGroup>();
            cerClearGroup.alpha = 0;
            cerClearGroup.interactable = cerClearGroup.blocksRaycasts = false;
            cerClearOffset = Hud17Text(cerClearRoot, "Clear (offset)", "", cpx, UiTheme.CyanDeep, UiTheme.Display, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(UnityTuning.ThemeHeroOffset, -UnityTuning.ThemeHeroOffset), new Vector2(1200, cpx * 1.3f), false);
            cerClearText = Hud17Text(cerClearRoot, "Clear", "", cpx, UiTheme.White, UiTheme.Display, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200, cpx * 1.3f), true);
            // U3-30.10 (c): the round's tally under it: its kills and its best burst (GOLD: points), sliding in along the slant.
            cerTallyFormat = "{0:0} KILLS · BEST <color=#" + UiTheme.GoldHex + ">+{1:0}</color>"; // made once
            cerTallyHome = new Vector2(0, -UnityTuning.Hud17ClearTallyDrop);
            cerTally = Hud17Text(cerClearRoot, "Clear tally", "", UiTheme.Px(UiTheme.Size.S24), UiTheme.White, UiTheme.BodyBold, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), cerTallyHome, new Vector2(1200, 34), true);
            cerTally.characterSpacing = UnityTuning.ThemeLabelTracking;
            cerTally.richText = true;
            cerClearRoot.gameObject.SetActive(false);
            game.events.On<KillEvent>("kill", e => { if (e.owner == "player") cerRoundKills++; }); // the round's kills (queued count)
            SetCeremonyVisible(false);

            game.events.On<RoundEvent>("roundClear", e =>
            {
                if (game.setup.mode == RunSetup.Grind || game.setup.mode == RunSetup.Campaign) return; // NIGHT25 (lead): a campaign level has no round ceremony (GG: not a wave counter). NIGHT22: THE GRIND has waves, not rounds: no ROUND N CLEAR slam
                cerClearRound = e.number;
                cerClearPending = true;
            });
            game.events.On<RoundEvent>("roundBegin", e =>
            {
                if (game.setup.mode == RunSetup.Grind || game.setup.mode == RunSetup.Campaign) return; // NIGHT25 (lead): no round ceremony in a campaign level. NIGHT22: no ROUND burn-in in THE GRIND (its WAVE banner is Ui's)
                cerRound = e.number;
                cerBurnPending = true;
                cerRoundKills = 0; // U3-30.10 (c): a new round's tally
                popRoundBest = 0;
            });
            game.OnNewRun += ResetCeremony;
        }

        void ResetCeremony()
        {
            cerRound = 0;
            cerShownRound = -1;
            cerClearT = cerBurnT = cerClearSlamT = double.PositiveInfinity;
            cerClearPending = cerBurnPending = false;
            cerRoundKills = 0;
            popRoundBest = 0;
            if (cerClearRoot.gameObject.activeSelf) cerClearRoot.gameObject.SetActive(false);
            cerClearBand.enabled = false;
            SetCeremonyVisible(false);
        }

        void SetCeremonyVisible(bool on)
        {
            if (on == cerVisible) return;
            cerVisible = on;
            cerMark.gameObject.SetActive(on);
            if (!on) cerRestLabel.enabled = false;
        }

        /// <summary>
        /// Round 1's reviewer: "ROUND N CLEAR" at the centre: slammed in 1.6 -> 1.0 (ease-out-back, 160 ms), held, faded by
        /// 1.2 s (RoundClearMath); the round's bonus pops under it and flies into the counter inside that time.
        /// </summary>
        void RenderRoundClear(double frameDt)
        {
            bool on = !RoundClearMath.Done(cerClearSlamT);
            if (cerClearRoot.gameObject.activeSelf != on) cerClearRoot.gameObject.SetActive(on);
            if (cerClearBand.enabled != on) cerClearBand.enabled = on;
            if (!on) return;
            if (FeelAB.On)
            {
                RenderRoundClear18(frameDt);
                return;
            }
            if (cerClear18Moved)
            {
                // a live F6 flip: back to NIGHT17's place and its band's alpha
                cerClear18Moved = false;
                cerClearRoot.anchoredPosition = cerClearHome18;
            }
            // the band: it sweeps in from the left with the CLEAR, fades with it
            cerClearBand.Fill = UiTween.OutCubic((cerClearSlamT + frameDt) / UnityTuning.Hud17ClearBandIn);
            cerClearBand.color = UiTheme.A(UiTheme.InkSolid, (float)RoundClearMath.Alpha(cerClearSlamT)); // round 5: opaque (the sign showed through)
            float s = (float)RoundClearMath.Scale(cerClearSlamT); // round 5: its first frame at t = 0 (the slam's 1.6x), then time moves
            if (s != cerClearShownScale)
            {
                cerClearShownScale = s;
                cerClearRoot.localScale = new Vector3(s, s, 1);
            }
            cerClearGroup.alpha = (float)RoundClearMath.Alpha(cerClearSlamT);
            // U3-30.10 (c): the tally slides in along the slant from down-left (ease-out-back), after the slam's first beat
            double k = (cerClearSlamT - UnityTuning.Hud17ClearTallyDelay) / UnityTuning.Hud17ClearTallyIn;
            float e = k <= 0 ? 0f : UiTween.OutBack(k);
            float rad = UnityTuning.ThemeSlantDeg * Mathf.Deg2Rad;
            var along = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * UnityTuning.ThemeEnterDistance;
            cerTally.rectTransform.anchoredPosition = cerTallyHome - along * (1f - e);
            cerClearSlamT += frameDt;
            cerTally.alpha = Mathf.Clamp01(e);
        }

        // NIGHT18 U3-31.6 (FeelAB on; M9: NIGHT17's 1.6x slam was a pop-in by the reel's rule and "jumped" on frame 2): ROUND N
        // CLEAR ENTERS along the slant (HudEntrance18: grows from 0.6x with a ~10 % overshoot, fades in, slides
        // ThemeEnterDistance px), its band fades in as it sweeps; then a LIVING hold (it creeps Hud18ClearDrift px along the
        // slant and grows Hud18ClearGrow, ease-out: never a static plate over the kill); NIGHT17's 200 ms ease-in exit.
        Vector2 cerClearHome18;
        bool cerClearHomeSet18, cerClear18Moved, cerClearHot18;

        void RenderRoundClear18(double frameDt)
        {
            if (!cerClearHomeSet18)
            {
                cerClearHomeSet18 = true;
                cerClearHome18 = cerClearRoot.anchoredPosition;
            }
            // round 2's picks (the reviewer wanted a slam for the big banners): HudSlam18, about 1.4x down to 1 over 150 ms,
            // fading in from about 0.6 (its first frame an M9 entrance by its alpha); drawn at its age, then stepped
            double t = cerClearSlamT;
            double hold = UiTween.OutCubic((t - UnityTuning.Hud18SlamTime) / (UnityTuning.Hud17ClearFadeFrom - UnityTuning.Hud18SlamTime));
            if (t < UnityTuning.Hud18SlamTime) hold = 0;
            bool still = game.settings.View.reducedMotion;
            float a = HudSlam18.Alpha(t) * (float)RoundClearMath.Alpha(t);
            float s = HudSlam18.Scale(t) * (1f + (still ? 0f : UnityTuning.Hud18ClearGrow * (float)hold));
            Vector2 off = still ? Vector2.zero : HudEntrance18.Along(UnityTuning.Hud18ClearDrift * (float)hold);
            cerClearRoot.localScale = new Vector3(s, s, 1);
            cerClearRoot.anchoredPosition = cerClearHome18 + off;
            cerClear18Moved = true;
            cerClearShownScale = -1; // NIGHT17's path rewrites its scale after a flip
            cerClearGroup.alpha = a;
            // round 3's picks: white-hot on the slam's first frames (its CYAN-DEEP offset copy white too; FLASH INTENSITY 0: none)
            bool hot = HudSlam18.White(t) && game.settings.Num("flashIntensity") > 0;
            if (hot != cerClearHot18)
            {
                cerClearHot18 = hot;
                cerClearOffset.color = hot ? UiTheme.White : UiTheme.CyanDeep;
            }
            cerClearBand.Fill = UiTween.OutCubic(t / UnityTuning.Hud17ClearBandIn);
            // round 1: the band only fades (the reel sees its rect, not its sweep): HudEntrance18.Fade, 95 % at ~126 ms
            cerClearBand.color = UiTheme.A(UiTheme.InkSolid, HudEntrance18.Fade(t) * (float)RoundClearMath.Alpha(t));
            // U3-30.10 (c): the tally as NIGHT17's (it already slides in along the slant and fades in)
            double k = (t - UnityTuning.Hud17ClearTallyDelay) / UnityTuning.Hud17ClearTallyIn;
            float e = k <= 0 ? 0f : UiTween.OutBack(k);
            cerTally.rectTransform.anchoredPosition = cerTallyHome - HudEntrance18.Along(UnityTuning.ThemeEnterDistance) * (1f - e);
            cerTally.alpha = Mathf.Clamp01(e);
            cerClearSlamT += frameDt;
        }

        /// <summary>A new round on the mark: the numeral and the mark's width to fit it (at the hero size).</summary>
        void LayoutMark(int round)
        {
            cerShownRound = round;
            cerNumeral.SetText("{0:00}", round); // two digits: "07" (IDENTITY.md's tile)
            cerNumeralOffset.SetText("{0:00}", round);
            cerMarkWidth = cerNumeral.GetPreferredValues().x;
        }

        /// <summary>The mark's centre at home (bottom left, right of the vitals plate), in the HUD layer's space.</summary>
        Vector2 MarkHome
        {
            get
            {
                // round 2's reviewer: the top line's middle (the hero lands in "ROUND N · N LEFT")
                Vector2 size = values.rect.size;
                return new Vector2(size.x * 0.5f, size.y - margin - UnityTuning.Hud17RoundTopDrop);
            }
        }

        void EmitCeremony(int number, string phase)
        {
            cerEvent.number = number;
            cerEvent.phase = phase;
            game.events.Emit(MetaEvents.RoundCeremony, cerEvent);
        }

        /// <summary>Frame time: the clear's flash, the hero number and its flight, the counter at rest.</summary>
        void RenderCeremony(double frameDt)
        {
            if (cerClearPending)
            {
                cerClearPending = false;
                cerClearT = 0;
                cerClearSlamT = 0; // the CLEAR slam
                cerClearText.SetText("ROUND {0:0} CLEAR", cerClearRound);
                cerClearOffset.SetText("ROUND {0:0} CLEAR", cerClearRound);
                cerTally.SetText(cerTallyFormat, cerRoundKills, (float)popRoundBest);
                cerClearShownScale = -1;
                EmitCeremony(cerClearRound, "clear");
            }
            RenderRoundClear(frameDt);
            if (cerBurnPending)
            {
                cerBurnPending = false;
                cerBurnT = 0;
                cerClearT = double.PositiveInfinity; // the ceremony takes over
                LayoutMark(cerRound);
                EmitCeremony(cerRound, "burn");
            }
            SetCeremonyVisible(cerShownRound > 0 && game.run.round > 0);
            if (!cerVisible) return;
            cerClearT += frameDt;
            Vector2 home = MarkHome;
            if (!RoundHeroMath.Done(cerBurnT))
            {
                // The hero: slammed in at the centre, held, then the flight to its corner (scale to the corner's size).
                cerBurnT += frameDt;
                Vector2 size = values.rect.size;
                var centre = new Vector2(size.x * 0.5f, size.y * 0.5f + UnityTuning.Hud17RoundHeroY);
                float fly = (float)RoundHeroMath.Fly(cerBurnT);
                float s = (float)RoundHeroMath.Scale(cerBurnT);
                float enterAlpha = 1f;
                if (FeelAB.On)
                {
                    // NIGHT18 U3-31.6: the hero enters (grows from 0.6x and fades in: HudEntrance18), no slam down from 1.4x
                    s = HudEntrance18.Scale(cerBurnT) * (1f + (UnityTuning.Hud17RoundCornerPx / UnityTuning.Hud17RoundHeroPx - 1f) * fly);
                    enterAlpha = HudEntrance18.Alpha(cerBurnT);
                }
                cerMark.anchoredPosition = Vector2.LerpUnclamped(centre, home, fly);
                cerMark.localScale = new Vector3(s, s, 1);
                cerMarkGroup.alpha = (1f - UiTween.InCubic(fly)) * enterAlpha; // it melts into the top line as it lands
                // "ROUND" over it through the hold, out with the flight's first half
                float la = cerBurnT < UnityTuning.Hud17RoundSlam ? (float)(cerBurnT / UnityTuning.Hud17RoundSlam) : 1f - Mathf.Clamp01(fly * 2f);
                cerLabel.alpha = la;
                if (cerRestLabel.enabled) cerRestLabel.enabled = false;
                CeremonyTint(0);
                return;
            }

            // At rest: nothing in a corner (round 2's reviewer): the round lives in the top line (Ui.cs); the CLEAR is the
            // round's end.
            if (cerMarkGroup.alpha != 0) cerMarkGroup.alpha = 0;
            if (cerRestLabel.enabled) cerRestLabel.enabled = false;
        }

        /// <summary>The numeral's colour: 0 WHITE (its rest and the hero), 1 CYAN-PALE (the clear's flash beat).</summary>
        void CeremonyTint(int tint)
        {
            if (tint == cerShownTint) return;
            cerShownTint = tint;
            cerNumeral.color = tint == 1 ? UiTheme.CyanPale : UiTheme.White;
        }
    }

    /// <summary>NIGHT17 round 1's reviewer: the round's CLEAR: a slam 1.6 -> 1.0 in 160 ms, held, gone by 1.2 s (Hud17Tests).</summary>
    public static class RoundClearMath
    {
        /// <summary>Round 5: a slam: Hud17ClearFrom (1.6x) -> 1 with an ease-out cubic over Hud17ClearIn (140 ms), then 1.</summary>
        public static double Scale(double t) =>
            t >= UnityTuning.Hud17ClearIn ? 1 : UnityTuning.Hud17ClearFrom + (1 - UnityTuning.Hud17ClearFrom) * UiTween.OutCubic(t / UnityTuning.Hud17ClearIn);

        /// <summary>1 to Hud17ClearFadeFrom, then out with an ease-in by Hud17ClearEnd.</summary>
        public static double Alpha(double t) =>
            t <= UnityTuning.Hud17ClearFadeFrom ? 1 : 1 - UiTween.InCubic((t - UnityTuning.Hud17ClearFadeFrom) / (UnityTuning.Hud17ClearEnd - UnityTuning.Hud17ClearFadeFrom));

        public static bool Done(double t) => t >= UnityTuning.Hud17ClearEnd;
    }

    /// <summary>NIGHT17 U3-30.7: the round's hero number: a slam at the centre, 1.5 s there, 400 ms to its corner (Hud17Tests).</summary>
    public static class RoundHeroMath
    {
        /// <summary>0 at the centre through the hold, then 0 -> 1 (ease-in-out cubic) to the corner.</summary>
        public static double Fly(double t) => t <= UnityTuning.Hud17RoundHold ? 0 : UiTween.InOutCubic((t - UnityTuning.Hud17RoundHold) / UnityTuning.Hud17RoundFly);

        public static bool Done(double t) => t >= UnityTuning.Hud17RoundHold + UnityTuning.Hud17RoundFly;

        /// <summary>x the hero size: the slam (Hud17RoundSlamFrom -> 1), the hold (1), the flight (1 -> corner / hero).</summary>
        public static double Scale(double t)
        {
            if (t < UnityTuning.Hud17RoundSlam) return UnityTuning.Hud17RoundSlamFrom + (1 - UnityTuning.Hud17RoundSlamFrom) * UiTween.OutCubic(t / UnityTuning.Hud17RoundSlam);
            double end = UnityTuning.Hud17RoundCornerPx / UnityTuning.Hud17RoundHeroPx;
            return 1 + (end - 1) * Fly(t);
        }
    }
}
