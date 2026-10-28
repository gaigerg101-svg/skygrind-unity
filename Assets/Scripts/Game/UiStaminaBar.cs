// NIGHT19 U3-33.8 (builder H): the stamina leaves the centre (GG 2026-10-06: "the stamina bar should not be just in the
// middle of the screen, it's really ugly"; NIGHT19.md S14). NIGHT17-18 drew a slanted bar under the crosshair (the sight
// layer); tonight the stamina is a row of chunky pips in the bottom-left vitals plate (Ui.cs), under the HP bar: ULTRAKILL's
// Standard HUD as the feel (one block of information, HP over the stamina, the centre empty), not its art.
// - One pip per bar (VIGOR adds one; the web's pips when design.stamina is off: three), Hud19Layout's widths, along the
//   theme's 12 deg slant, hard edges (no cut, no glow), on the plate's INK. Each pip is cut by INK slits at each boost's cost
//   (one bar = two boosts), so the pips read in boosts as ULTRAKILL's segments read in dashes.
// - The fill: CYAN while a full boost fires, CYAN-DEEP under its cost; always shown (no fade at rest), so a boost reads on
//   its own frame (StaminaPips; Hud19Tests: within 2 frames).
// - A DRAIN FLASH: the part a boost or a dash-jump spent shows WHITE for Hud19DrainFlash s, then a pale trail that slides
//   down to the fill (NIGHT18's trail). A REFILL TICK: the pip whose boost came back punches and its fill is WHITE for three
//   frames. Under the boost's floor (a press fails) the tracks pulse INK-SOFT -> CYAN-DEEP.
// - The slide charge's cell sits at the row's right end (UiFeelHud.cs moves it there), and the wall-jump ticks (UiMoveRow)
//   move down Hud19MoveRowShift px to clear the row.
// StaminaHud.Read stays the one place this view reads the bar. Frame time only; no allocation per frame (fills and colours
// set on change; UiShape rebuilds only when its fill moves).
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        RectTransform staminaBarRoot; // the pips' row in the HP plate's values twin
        RectTransform[] staminaPipRt;
        UiShape[] staminaPipTrack, staminaPipTrail, staminaPipFill;
        UiShape[,] staminaPipSlit;
        StaminaHud.Info staminaBarInfo;
        readonly StaminaPips staminaPips = new StaminaPips();
        int staminaBarSegments = -1, staminaPipSlits = -1, staminaPipTrackLevel = -2, staminaPipFillTint = -1;
        double staminaPipCost = double.NaN;
        readonly int[] staminaPipShownTick = new int[StaminaPips.Max]; // 0 none, 1 white
        readonly float[] staminaPipShownScale = new float[StaminaPips.Max];
        readonly float[] staminaPipShownFill = { -1, -1, -1, -1 }, staminaPipShownTrail = { -1, -1, -1, -1 }; // px wide
        bool staminaPipShownFlash;

        void BuildStaminaBar()
        {
            RectTransform hpValues = values.Find("HP panel (values)") as RectTransform;
            if (hpValues == null) return;
            // the row: top-left anchored under the HP bar, as wide as the HP bar
            staminaBarRoot = Hud17Rect(hpValues, "Stamina pips", new Vector2(0, 1), new Vector2(0, 1), new Vector2(UnityTuning.Hud17VitalsPad, -UnityTuning.Hud19PipsY), new Vector2(UnityTuning.Hud17HpBarWidth, UnityTuning.Hud19PipH));
            int n = StaminaPips.Max, slits = UnityTuning.Hud19MaxSlits;
            staminaPipRt = new RectTransform[n];
            staminaPipTrack = new UiShape[n];
            staminaPipTrail = new UiShape[n];
            staminaPipFill = new UiShape[n];
            staminaPipSlit = new UiShape[n, slits];
            var size = new Vector2(UnityTuning.Hud19PipMaxW, UnityTuning.Hud19PipH);
            for (int i = 0; i < n; i++)
            {
                // each pip on its own rect (its pivot the middle: the refill tick's punch scales it in place)
                staminaPipRt[i] = Hud17Rect(staminaBarRoot, $"Pip {i}", new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
                staminaPipTrack[i] = Hud17Shape(staminaPipRt[i], "Track", UiTheme.InkSoft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
                // the trail and the fill are sized to their share (left-anchored), not UiShape.Fill: the same slanted shape, and
                // the reel's UI scanner sees the fill's width change on the boost's frame (S14's 2-frame response)
                staminaPipTrail[i] = Hud17Shape(staminaPipRt[i], "Trail", UiTheme.A(UiTheme.CyanPale, UnityTuning.StaminaTrailAlpha), new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, size);
                staminaPipFill[i] = Hud17Shape(staminaPipRt[i], "Fill", UiTheme.Cyan, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, size);
                foreach (UiShape s in new[] { staminaPipTrack[i], staminaPipTrail[i], staminaPipFill[i] }) s.SetCuts(0, 0);
                for (int k = 0; k < slits; k++)
                {
                    // an INK slit across the pip at each boost's cost (over the fill), along the slant like the pip's edges
                    UiShape slit = Hud17Shape(staminaPipRt[i], $"Slit {k}", UiTheme.InkSolid, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(UnityTuning.Hud19CellSlit, UnityTuning.Hud19PipH));
                    slit.SetCuts(0, 0);
                    slit.enabled = false;
                    staminaPipSlit[i, k] = slit;
                }
                staminaPipRt[i].gameObject.SetActive(false);
                staminaPipShownScale[i] = 1;
            }
            // the wall-jump ticks (UiMoveRow, airborne only) sat where the pips are now: down below the row
            var down = new Vector2(0, -UnityTuning.Hud19MoveRowShift);
            if (moveRowTicks != null) moveRowTicks.anchoredPosition += down;
            if (moveRowSpeed != null) moveRowSpeed.rectTransform.anchoredPosition += down;
            if (moveRowUnit != null) moveRowUnit.rectTransform.anchoredPosition += down;
            game.OnNewRun += () => staminaPips.Reset();
        }

        /// <summary>The pips' count or the boost's cost changed (a run, VIGOR, the rules): widths, places and slits.</summary>
        void StaminaPipLayout(int n, double cost)
        {
            staminaBarSegments = n;
            staminaPipCost = cost;
            float w = Hud19Layout.PipWidth(n);
            int slits = Hud19Layout.Slits(cost);
            staminaPipSlits = slits;
            var size = new Vector2(w, UnityTuning.Hud19PipH);
            for (int i = 0; i < staminaPipRt.Length; i++)
            {
                bool on = i < n;
                if (staminaPipRt[i].gameObject.activeSelf != on) staminaPipRt[i].gameObject.SetActive(on);
                if (!on) continue;
                staminaPipRt[i].anchoredPosition = new Vector2(Hud19Layout.PipX(i, n) + w * 0.5f, 0);
                staminaPipRt[i].sizeDelta = size;
                staminaPipTrack[i].rectTransform.sizeDelta = size;
                staminaPipShownFill[i] = staminaPipShownTrail[i] = -1; // resized below from the pip's new width
                for (int k = 0; k < UnityTuning.Hud19MaxSlits; k++)
                {
                    UiShape slit = staminaPipSlit[i, k];
                    bool show = k < slits;
                    if (slit.enabled != show) slit.enabled = show;
                    if (show) slit.rectTransform.anchoredPosition = new Vector2((float)(Hud19Layout.SlitAt(k, cost) * w), 0);
                }
            }
            staminaPipFillTint = -1;
            staminaPipTrackLevel = -2;
        }

        void RenderStaminaBar(double frameDt)
        {
            if (staminaBarRoot == null) return;
            StaminaHud.Read(game, ref staminaBarInfo);
            if (!staminaBarInfo.valid) return;
            double fill = staminaBarInfo.fill, cost = staminaBarInfo.boostCost;
            bool flashes = game.settings.Num("flashIntensity") > 0;
            staminaPips.Step(fill, staminaBarInfo.max, cost, staminaBarInfo.boostFloor, frameDt, flashes);
            // the reel's markers (S14: the pips respond within 2 frames of a boost; the vitals scenario's refill)
            if (staminaPips.spentNow) FeelProbe.Event(UnityTuning.Hud19PipsSpentEvent);
            if (staminaPips.tickedNow) FeelProbe.Event(UnityTuning.Hud19PipsTickEvent);
            int n = staminaPips.count;
            if (n != staminaBarSegments || cost != staminaPipCost) StaminaPipLayout(n, cost);
            bool flash = staminaPips.drainFlash > 0;
            if (flash != staminaPipShownFlash)
            {
                staminaPipShownFlash = flash;
                Color tc = flash ? UiTheme.White : UiTheme.A(UiTheme.CyanPale, UnityTuning.StaminaTrailAlpha);
                for (int i = 0; i < staminaPipTrail.Length; i++) staminaPipTrail[i].color = tc;
            }
            // the fill's value: CYAN while a full boost fires, CYAN-DEEP under its cost
            int tint = fill >= cost - 1e-9 ? 1 : 0;
            if (tint != staminaPipFillTint)
            {
                staminaPipFillTint = tint;
                for (int i = 0; i < n; i++) if (staminaPipShownTick[i] == 0) staminaPipFill[i].color = tint == 1 ? UiTheme.Cyan : UiTheme.CyanDeep;
            }
            for (int i = 0; i < n; i++)
            {
                StaminaPipWidth(staminaPipFill[i], ref staminaPipShownFill[i], staminaPips.fill[i]);
                StaminaPipWidth(staminaPipTrail[i], ref staminaPipShownTrail[i], staminaPips.trail[i]);
                int tick = staminaPips.TickWhite(i) && flashes ? 1 : 0;
                if (tick != staminaPipShownTick[i])
                {
                    staminaPipShownTick[i] = tick;
                    staminaPipFill[i].color = tick == 1 ? UiTheme.White : tint == 1 ? UiTheme.Cyan : UiTheme.CyanDeep;
                }
                float s = staminaPips.PipScale(i);
                if (s != staminaPipShownScale[i])
                {
                    staminaPipShownScale[i] = s;
                    staminaPipRt[i].localScale = new Vector3(s, s, 1);
                }
            }
            // the tracks: INK-SOFT; under the boost's floor a slow pulse toward CYAN-DEEP (in levels: set on change only)
            int level = staminaPips.empty ? StaminaBarLook.PulseLevel(staminaPips.TrackPulse, UnityTuning.StaminaPulseLevels) : -1;
            if (level != staminaPipTrackLevel)
            {
                staminaPipTrackLevel = level;
                Color tr = level < 0 ? UiTheme.InkSoft : Color.Lerp(UiTheme.InkSoft, UiTheme.A(UiTheme.CyanDeep, UnityTuning.StaminaPulseAlpha), level / (float)UnityTuning.StaminaPulseLevels);
                for (int i = 0; i < staminaPipTrack.Length; i++) staminaPipTrack[i].color = tr;
            }
        }

        /// <summary>A fill or trail shape's width: its share of the pip, in 0.5 px steps (written only when that changes; a
        /// zero share draws nothing).</summary>
        void StaminaPipWidth(UiShape s, ref float shown, double share)
        {
            float w = Mathf.Round((float)share * Hud19Layout.PipWidth(staminaBarSegments) * 2f) * 0.5f;
            if (w == shown) return;
            shown = w;
            s.rectTransform.sizeDelta = new Vector2(w, UnityTuning.Hud19PipH);
        }

        /// <summary>Automation and the reel: the pips' row (null before the HUD is built) and the fill they show now.</summary>
        public RectTransform StaminaPipsRow => staminaBarRoot;
        public double StaminaPipsShownFill(int i) => i >= 0 && i < StaminaPips.Max ? staminaPips.fill[i] : 0;
    }

    /// <summary>The stamina bar's rules (Hud17Tests; NIGHT19's pips read them too).</summary>
    public static class StaminaBarMath
    {
        /// <summary>A segment per bar (VIGOR: two; the web's pips: three), at least one, at most the segments made.</summary>
        public static int Segments(double max) => Math.Max(1, Math.Min(UnityTuning.Hud17StaminaMaxSegments, (int)Math.Ceiling(max - 1e-9)));

        /// <summary>Each segment's width in a bar of a fixed total width (round 1's fix list: never a sliver): the gaps come out of it.</summary>
        public static float SegmentWidth(float total, int segments, float gap) => segments <= 1 ? total : (total - (segments - 1) * gap) / segments;

        /// <summary>Segment i's share of the fill (segment i holds i..i+1).</summary>
        public static double SegmentFill(double fill, int i) => Math.Max(0, Math.Min(1, fill - i));

        /// <summary>Solid from the boost floor (a boost fires); dim and hollow under it.</summary>
        public static bool Armed(double fill, double floor) => fill >= floor - 1e-9;

        /// <summary>The frame the bar refills to full (the one flash).</summary>
        public static bool JustFull(double before, double fill, double max) => before < max - 1e-9 && fill >= max - 1e-9;
    }
}
