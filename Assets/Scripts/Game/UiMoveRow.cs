// NIGHT16 polish #35 (builder U): the web's move row (hud.js :127-133 CSS, :284 maxTicksShown, :329 the DOM, :907-912 the
// update), which the port never had: in the HP plate under the stamina segments, the horizontal speed (toFixed(1), then a
// small muted "M/S") and, while airborne, the wall-jump ticks (wallJump.maxPerAirtime + perks.grip.wallJumps of them, tick
// i cyan while i < the wall jumps left). The speed is white under hud.speedMid, cyan from it, colors.enemy above
// hud.speedHigh. The row's REGEN tag is UiFeelEdges.cs' (the plate's top right); this file leaves it alone.
// The port's reads: Mover.Speed (MovementMath.HorizontalSpeed(velocity): mover.js `get speed`, the horizontal speed),
// !Mover.grounded (mover.js hudState airborne = !this.grounded), Mover.wallJumpsLeft (mover.js hudState wallJumpsLeft; reset
// to MaxWallJumps = maxPerAirtime + GRIP's bonusWallJumps at each airtime).
// The plate grows down (UnityTuning.MoveRowPlateGrow, its twin in the values layer too) so the row sits inside it.
// PORT-RULES 6: the config read once here; per frame only compares: SetText("{0:1}") when the tenths change, the colour
// when the band changes, the ticks' row when airborne changes, a tick's colour when it turns on or off.
using System;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        TextMeshProUGUI moveRowSpeed, moveRowUnit;
        bool moveRowShown = true; // NIGHT17 round 2: the readout only while moving faster than a walk
        double moveRowShowAt;
        RectTransform moveRowTicks;
        UnityEngine.UI.Image[] moveRowTick;
        bool[] moveRowTickOn;
        double moveRowMid, moveRowHigh;
        Color moveRowTrack;
        int moveRowTenths = int.MinValue, moveRowBand = -1, moveRowAir = -1;

        /// <summary>The row, once (the constructor, after BuildFeelEdges).</summary>
        void BuildMoveRow()
        {
            var plate = frame.Find("HP panel") as RectTransform;
            var twin = values.Find("HP panel (values)") as RectTransform;
            if (plate == null || twin == null) return;
            foreach (RectTransform rt in new[] { plate, twin })
            {
                // Down: the top edge (and everything anchored to it) stays where it is.
                rt.anchoredPosition -= new Vector2(0, UnityTuning.MoveRowPlateGrow);
                rt.sizeDelta += new Vector2(0, UnityTuning.MoveRowPlateGrow);
            }
            moveRowMid = cfg.Num("hud.speedMid");
            moveRowShowAt = cfg.movement.ground.walkSpeed * UnityTuning.Hud17SpeedShowAt;
            moveRowHigh = cfg.Num("hud.speedHigh");

            // The readout (right-aligned in a box as wide as "88.8") and its unit, on one baseline: both boxes are centred on it.
            float h = UnityTuning.MoveRowHeight, top = -(UnityTuning.MoveRowBaseline - h * 0.5f);
            moveRowSpeed = Text(twin, "Speed", "0.0", UnityTuning.MoveRowSpeedSize, text, TextAlignmentOptions.BaselineRight, Vector2.zero, new Vector2(100, h), new Vector2(0, 1), Face.Body);
            moveRowSpeed.overflowMode = TextOverflowModes.Overflow;
            float w = Mathf.Ceil(moveRowSpeed.GetPreferredValues(UnityTuning.MoveRowWidest).x);
            moveRowSpeed.rectTransform.sizeDelta = new Vector2(w, h);
            moveRowSpeed.rectTransform.anchoredPosition = new Vector2(UnityTuning.MoveRowLeft, top);
            float x = UnityTuning.MoveRowLeft + w + UnityTuning.MoveRowUnitGap;
            var unit = moveRowUnit = Text(twin, "Speed unit", UnityTuning.MoveRowUnit, tSmall, muted, TextAlignmentOptions.BaselineLeft, new Vector2(x, top), new Vector2(80, h), new Vector2(0, 1), Face.Body);
            unit.characterSpacing = UnityTuning.MoveRowUnitSpacing;
            unit.overflowMode = TextOverflowModes.Overflow;
            x += Mathf.Ceil(unit.GetPreferredValues(UnityTuning.MoveRowUnit).x) + UnityTuning.MoveRowGap;

            // The ticks (hidden on the ground, their place kept), centred on the digits' middle (half a 20 px digit over the baseline).
            int n = MoveRowMath.TickCount(cfg.Int("wallJump.maxPerAirtime"), cfg.Int("perks.grip.wallJumps"));
            moveRowTicks = new GameObject("Wall-jump ticks", typeof(RectTransform)).GetComponent<RectTransform>();
            moveRowTicks.SetParent(twin, false);
            moveRowTicks.anchorMin = moveRowTicks.anchorMax = moveRowTicks.pivot = new Vector2(0, 1);
            moveRowTicks.anchoredPosition = new Vector2(x, -(UnityTuning.MoveRowBaseline - UnityTuning.MoveRowSpeedSize * 0.35f) + UnityTuning.MoveRowTickHeight * 0.5f);
            moveRowTicks.sizeDelta = new Vector2(n * (UnityTuning.MoveRowTickWidth + UnityTuning.MoveRowTickGap), UnityTuning.MoveRowTickHeight);
            moveRowTrack = new Color(1, 1, 1, UnityTuning.MoveRowTickTrackAlpha);
            moveRowTick = new UnityEngine.UI.Image[n];
            moveRowTickOn = new bool[n];
            for (int i = 0; i < n; i++)
            {
                var t = Img(moveRowTicks, $"Tick {i}", moveRowTrack, new Vector2(0, 1), new Vector2(0, 1), new Vector2(i * (UnityTuning.MoveRowTickWidth + UnityTuning.MoveRowTickGap), 0), new Vector2(UnityTuning.MoveRowTickWidth, UnityTuning.MoveRowTickHeight));
                t.rectTransform.pivot = new Vector2(0, 1);
                moveRowTick[i] = t;
            }
            moveRowTicks.gameObject.SetActive(false);
        }

        /// <summary>Every frame (hud.js :907-912): only what changed is written.</summary>
        void RenderMoveRow(double frameDt)
        {
            if (moveRowSpeed == null || !values.gameObject.activeSelf) return;
            Skygrind.Core.Mover m = game.mover;
            if (m == null) return;
            double speed = m.Speed;
            // NIGHT17 round 2's reviewer: hidden at rest; it shows while moving faster than a walk.
            bool show = UnityTuning.Hud17SpeedOnHud && speed > moveRowShowAt; // round 5: off the default HUD (it read ~10 px at 1080p)
            if (show != moveRowShown)
            {
                moveRowShown = show;
                moveRowSpeed.gameObject.SetActive(show);
                if (moveRowUnit != null) moveRowUnit.gameObject.SetActive(show);
            }
            int tenths = MoveRowMath.Tenths(speed);
            if (tenths != moveRowTenths)
            {
                moveRowTenths = tenths;
                moveRowSpeed.SetText("{0:1}", tenths / 10f);
            }
            int band = MoveRowMath.SpeedBand(speed, moveRowMid, moveRowHigh);
            if (band != moveRowBand)
            {
                moveRowBand = band;
                // NIGHT17: the player's speed on the cyan value ladder (CYAN-HOT over speedHigh): red-orange is danger only
                moveRowSpeed.color = band == MoveRowMath.Danger ? UiTheme.CyanHot : band == MoveRowMath.Usable ? UiTheme.Cyan : UiTheme.White;
            }
            int air = m.grounded ? 0 : 1;
            if (air != moveRowAir)
            {
                moveRowAir = air;
                moveRowTicks.gameObject.SetActive(air == 1);
            }
            MoveRowTicks18(frameDt, air == 1);
            if (air == 0) return;
            int left = m.wallJumpsLeft;
            for (int i = 0; i < moveRowTick.Length; i++)
            {
                bool on = MoveRowMath.TickOn(i, left);
                if (on == moveRowTickOn[i]) continue;
                moveRowTickOn[i] = on;
                moveRowTick[i].color = on ? usable : moveRowTrack;
            }
        }

        // NIGHT18 U3-31.6 (FeelAB on): the wall-jump ticks enter on take-off (grow from 0.6x and fade in: HudEntrance18; a
        // CanvasGroup made on first use); off: they appear whole, as NIGHT17's.
        double moveRowEnterAge18 = double.MaxValue;
        bool moveRowWasAir18;
        float moveRowShownScale18 = 1;
        CanvasGroup moveRowGroup18;

        void MoveRowTicks18(double frameDt, bool air)
        {
            if (air && !moveRowWasAir18) moveRowEnterAge18 = 0;
            moveRowWasAir18 = air;
            float s = 1, a = 1;
            if (FeelAB.On && air && !HudEntrance18.Done(moveRowEnterAge18))
            {
                moveRowEnterAge18 = HudEntrance18.Step(moveRowEnterAge18, frameDt);
                s = HudEntrance18.Scale(moveRowEnterAge18);
                a = HudEntrance18.Alpha(moveRowEnterAge18);
            }
            if (s == moveRowShownScale18) return;
            moveRowShownScale18 = s;
            if (moveRowGroup18 == null)
            {
                moveRowGroup18 = moveRowTicks.gameObject.AddComponent<CanvasGroup>();
                moveRowGroup18.interactable = moveRowGroup18.blocksRaycasts = false;
            }
            moveRowTicks.localScale = new Vector3(s, s, 1);
            moveRowGroup18.alpha = a;
        }
    }

    /// <summary>The move row's pure rules (MoveRowTests).</summary>
    public static class MoveRowMath
    {
        public const int White = 0, Usable = 1, Danger = 2;

        /// <summary>hud.js :907: above speedHigh danger (colors.enemy), from speedMid usable, else white.</summary>
        public static int SpeedBand(double speed, double mid, double high) => speed > high ? Danger : speed >= mid ? Usable : White;

        /// <summary>The readout in tenths (hud.js :908 toFixed(1); halves away from zero, NaN as 0).</summary>
        public static int Tenths(double speed) => double.IsNaN(speed) ? 0 : (int)Math.Round(speed * 10, MidpointRounding.AwayFromZero);

        /// <summary>hud.js :284: wallJump.maxPerAirtime + perks.grip.wallJumps (GRIP adds one).</summary>
        public static int TickCount(int maxPerAirtime, int gripWallJumps) => Math.Max(0, maxPerAirtime + gripWallJumps);

        /// <summary>hud.js :912: tick i is on while i &lt; the wall jumps left.</summary>
        public static bool TickOn(int i, int wallJumpsLeft) => i < wallJumpsLeft;
    }
}
