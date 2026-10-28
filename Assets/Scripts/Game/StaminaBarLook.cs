// NIGHT17 U3-30.4 round 5 (builder M): the stamina bar's look as rules, pure (no UnityEngine; StaminaBarTests), so it reads
// at a glance (round 4's reviewer: "a pale bar ... nearly empty and unchanged" in the sprint strip: the fill was CYAN-HOT,
// near white, while sprinting, on a pale dusk sky):
// - the cyan VALUE ladder by state: CYAN-DEEP while low (under a full boost's cost: only a partial boost, or none),
//   CYAN from a full boost's cost up (full), CYAN-HOT while it refills, WHITE for a moment when a bar is spent;
// - "spent": a chunk left in one frame (a boost, a dash-jump: at least SpendChunk of a bar, more than any frame of the
//   sprint's drain) or a bar emptied (the fill crossed a whole bar downward: the sprint ran it out, VIGOR's second bar);
// - "refilling": the sim's refill ran on the last step (Mover.stamina: past its delay, not boosting or sprinting, under
//   the max; the web's pips: under MaxPips and not dashing), read from the Mover, so it never flickers between frames;
// - a slow pulse while empty (under the boost floor: a press would fail), a raised cosine from 0.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class StaminaBarLook
    {
        /// <summary>The ladder's steps: CYAN-DEEP (low), CYAN (full), CYAN-HOT (refilling), WHITE (spent).</summary>
        public const int Deep = 0, Full = 1, Refill = 2, Spent = 3;

        /// <summary>The fill's tint: WHITE while the spent flash runs, CYAN-HOT while refilling, CYAN from a full boost's cost, else CYAN-DEEP.</summary>
        public static int Tint(double fill, double cost, bool refilling, bool flashing) =>
            flashing ? Spent : refilling ? Refill : fill >= cost - 1e-9 ? Full : Deep;

        /// <summary>Was a bar spent between two frames' fills? A chunk of at least `chunk` left, or a whole bar emptied.</summary>
        public static bool WasSpent(double before, double fill, double chunk)
        {
            if (double.IsNaN(before) || fill >= before) return false;
            return before - fill >= chunk - 1e-9 || Math.Ceiling(fill - 1e-9) < Math.Ceiling(before - 1e-9);
        }

        /// <summary>The spent flash's time left: `duration` when it starts, else down by the frame's time, never below 0.</summary>
        public static double FlashStep(double left, bool start, double frameDt, double duration) => start ? duration : Math.Max(0, left - frameDt);

        /// <summary>Empty: under the boost floor (a press fails).</summary>
        public static bool Empty(double fill, double floor) => fill < floor - 1e-9;

        /// <summary>The empty pulse, 0 .. 1: a raised cosine of `period` s, 0 at t = 0.</summary>
        public static double Pulse(double t, double period) => period <= 0 ? 0 : Math.Max(0, Math.Min(1, 0.5 - 0.5 * Math.Cos(2 * Math.PI * t / period)));

        /// <summary>The pulse in `steps` levels (the colour is set only when the level changes).</summary>
        public static int PulseLevel(double pulse, int steps) => (int)Math.Round(Math.Max(0, Math.Min(1, pulse)) * steps);

        /// <summary>Did the sim's refill run on the mover's last step?</summary>
        public static bool Refilling(Mover m)
        {
            if (m == null) return false;
            if (!m.stamina.rules.Any) return m.state != MoveState.Dash && m.pips < m.MaxPips - 1e-9;
            Stamina.State s = m.stamina;
            if (m.state == MoveState.Dash || s.sprintStep || s.bar >= m.StaminaMax - 1e-9) return false;
            if (!s.rules.refill) return true;
            return s.sinceSpend >= (s.emptied ? s.rules.emptyDelaySteps : s.rules.delaySteps);
        }
    }

    /// <summary>NIGHT19 U3-33.8: a rect in canvas px at 1080p, origin bottom left (Hud19Layout; no UnityEngine).</summary>
    public struct HudRect
    {
        public float x, y, w, h;
        public HudRect(float x, float y, float w, float h) { this.x = x; this.y = y; this.w = w; this.h = h; }
    }

    /// <summary>
    /// NIGHT19 U3-33.8 (builder H): the vitals cluster's layout (Hud19Tests), in canvas px at 1080p with the origin at the
    /// screen's bottom left. The HP plate (Ui.cs) sits at the safe inset, grown down by UiMoveRow; inside it, under the HP
    /// bar, the stamina pips (one per bar, Hud19PipMaxW at most, sharing Hud19PipRowW past two) and the slide charge's cell
    /// at the row's right end. HUD SCALE scales a corner block toward its corner (UiHudMotion's layers): OnScreen.
    /// </summary>
    public static class Hud19Layout
    {
        /// <summary>The plate's height (NIGHT17's vitals plate grown by the move row).</summary>
        public static float PlateHeight => UnityTuning.Hud17VitalsHeight + UnityTuning.MoveRowPlateGrow;

        /// <summary>A pip's width for `n` pips: chunky (Hud19PipMaxW) while they fit, else they share the row.</summary>
        public static float PipWidth(int n)
        {
            if (n <= 1) return UnityTuning.Hud19PipMaxW;
            float share = (UnityTuning.Hud19PipRowW - (n - 1) * UnityTuning.Hud19PipGap) / n;
            return Math.Min(UnityTuning.Hud19PipMaxW, share);
        }

        /// <summary>Pip i's left edge from the row's left.</summary>
        public static float PipX(int i, int n) => i * (PipWidth(n) + UnityTuning.Hud19PipGap);

        /// <summary>The slide cell's left edge from the row's left (its right edge under the HP bar's right end).</summary>
        public static float SlideX => UnityTuning.Hud19PipRowW + UnityTuning.Hud19SlideGap;

        /// <summary>The INK slits a boost's `cost` cuts into a pip (one bar of cost 0.5: one; the web's pip of cost 1: none).</summary>
        public static int Slits(double cost)
        {
            if (!(cost > 0) || cost >= 1 - 1e-9) return 0;
            int n = (int)Math.Ceiling(1 / cost - 1e-9) - 1;
            return Math.Max(0, Math.Min(UnityTuning.Hud19MaxSlits, n));
        }

        /// <summary>Slit k's place inside a pip (0..1): (k + 1) x the cost.</summary>
        public static double SlitAt(int k, double cost) => (k + 1) * cost;

        /// <summary>The plate (after the move row grew it down).</summary>
        public static HudRect Plate() => new HudRect(UnityTuning.ThemeSafeInset, UnityTuning.ThemeSafeInset - UnityTuning.MoveRowPlateGrow, UnityTuning.Hud17VitalsWidth, PlateHeight);

        /// <summary>A rect `top` px under the plate's top, `x` px in from its inner left, w x h; widened by the slant's lean.</summary>
        static HudRect InPlate(float x, float top, float w, float h)
        {
            HudRect p = Plate();
            float lean = (float)Math.Tan(UnityTuning.ThemeSlantDeg * Math.PI / 180) * h * 0.5f;
            return new HudRect(p.x + UnityTuning.Hud17VitalsPad + x - lean, p.y + p.h - top - h, w + 2 * lean, h);
        }

        public static HudRect PipRect(int i, int n) => InPlate(PipX(i, n), UnityTuning.Hud19PipsY, PipWidth(n), UnityTuning.Hud19PipH);

        public static HudRect SlideRect() => InPlate(SlideX, UnityTuning.Hud19PipsY, UnityTuning.Hud19SlideW, UnityTuning.Hud19PipH);

        /// <summary>S14's box: the central Hud19CentreBox of each side.</summary>
        public static HudRect CentreBox(float width, float height)
        {
            float k = (1 - UnityTuning.Hud19CentreBox) * 0.5f;
            return new HudRect(width * k, height * k, width * UnityTuning.Hud19CentreBox, height * UnityTuning.Hud19CentreBox);
        }

        /// <summary>Where a corner block's rect lands under HUD SCALE: toward the bottom-left corner, or the bottom-right one.</summary>
        public static HudRect OnScreen(HudRect r, float hudScale, bool fromRight, float width)
        {
            float y = r.y * hudScale, w = r.w * hudScale, h = r.h * hudScale;
            float x = fromRight ? width - (width - r.x) * hudScale : r.x * hudScale;
            return new HudRect(x, y, w, h);
        }

        /// <summary>Two rects share area (touching edges don't).</summary>
        public static bool Overlaps(HudRect a, HudRect b) => a.x < b.x + b.w && b.x < a.x + a.w && a.y < b.y + b.h && b.y < a.y + a.h;

        /// <summary>The weapon card's slots row width: `slots` cells and their gaps, "+N" before them when `more`.</summary>
        public static float CardRowWidth(int slots, bool more)
        {
            float w = slots > 0 ? slots * (UnityTuning.Hud17StripCellW + UnityTuning.Hud17StripGap) - UnityTuning.Hud17StripGap : 0;
            if (more) w += UnityTuning.Hud17StripMoreW + UnityTuning.Hud17StripGap;
            return Math.Max(w, UnityTuning.Hud17StripCellW);
        }

        /// <summary>The card's inner width: the slots row or the header's text (`headW`), the wider.</summary>
        public static float CardInnerWidth(int slots, bool more, float headW) => Math.Max(CardRowWidth(slots, more), headW);

        /// <summary>The card's height: padding, the meter, the slots, the header, padding (UiHotbar.FitWeaponBlock).</summary>
        public static float CardHeight =>
            UnityTuning.Hud17StripPad + UnityTuning.Hud17StripMeterH + UnityTuning.Hud17StripGap + UnityTuning.Hud17StripCellH
            + UnityTuning.Hud19CardHeadGap + UnityTuning.Hud19CardHeadH + UnityTuning.Hud17StripPad;

        /// <summary>The card's bottom edge: over the points' numeral, its bar and the gap (Ui.cs places the weapon line there).</summary>
        public static float CardBottom =>
            UnityTuning.ThemeSafeInset + UnityTuning.Hud17PointsBox + UnityTuning.Hud17PointsBarHeight + 2 * UnityTuning.Hud17PointsBarGap + UnityTuning.PointsWeaponGap;

        /// <summary>The weapon card's plate (bottom right) with `slots` slots, a header up to `headW` px wide.</summary>
        public static HudRect CardRect(int slots, bool more, float width, float headW = UnityTuning.Hud19CardHeadMaxW)
        {
            float w = CardInnerWidth(slots, more, headW) + 2 * UnityTuning.Hud17StripPad + UnityTuning.ThemeAccentBar;
            return new HudRect(width - UnityTuning.ThemeSafeInset - w, CardBottom, w, CardHeight);
        }
    }

    /// <summary>
    /// NIGHT19 U3-33.8: a combo's callout line in the style meter's column (UiStyleMeter.cs): the player's N19Events.Combo
    /// only, its name as the line (the event's own cached string), slammed in whole (HudSlam18), white-hot for its first
    /// frames, held Hud19ComboLife (an award's line: Hud17CalloutLife), fading from Hud19ComboFadeAt.
    /// </summary>
    public static class ComboCallout19
    {
        public static string Line(ComboEvent e) => e == null || e.owner != "player" || string.IsNullOrEmpty(e.name) ? null : e.name;

        public static float Scale(double age) => HudSlam18.Scale(age);

        /// <summary>A combo line's alpha (an award line's is UiStyleMeter's own): whole, then its fade.</summary>
        public static float Alpha(double age, bool combo)
        {
            double fadeAt = combo ? UnityTuning.Hud19ComboFadeAt : UnityTuning.Hud17CalloutFadeAt;
            double life = combo ? UnityTuning.Hud19ComboLife : UnityTuning.Hud17CalloutLife;
            return age <= fadeAt ? 1f : 1f - UiTween.InCubic((age - fadeAt) / Math.Max(1e-4, life - fadeAt));
        }

        public static bool White(double age) => age >= 0 && age < UnityTuning.Hud19ComboWhite;

        public static bool Expired(double age, bool combo, double awardLife) => age >= (combo ? UnityTuning.Hud19ComboLife : awardLife);
    }

    /// <summary>
    /// NIGHT19 U3-33.8: the WEAPONS rows' cascade (NIGHT18's leftover: the body appeared at once). Row i waits
    /// min(i x Hud19RowStagger, Hud19RowStaggerMax), then enters as the menus' parts do (MenuMotion18: ease-out-back over
    /// ThemeInTime from ThemeEnterDistance px down-left along the slant, fading in; REDUCED MOTION: the fade only); its role
    /// tag pops Hud19TagPopAfter s after the row starts (HudEntrance18: from 0.6x past its size).
    /// </summary>
    public static class RowCascade19
    {
        public static double Wait(int index) => Math.Min(Math.Max(0, index) * UnityTuning.Hud19RowStagger, UnityTuning.Hud19RowStaggerMax);

        public static float Alpha(double age, int index) => MenuMotion18.Alpha(age - Wait(index));

        public static UnityEngine.Vector2 Offset(double age, int index, bool still) => still ? default : MenuMotion18.Offset(age - Wait(index));

        public static float TagScale(double age, int index)
        {
            double t = age - Wait(index) - UnityTuning.Hud19TagPopAfter;
            return t <= 0 ? HudEntrance18.Scale(0) : HudEntrance18.Scale(t);
        }

        /// <summary>Every row of `rows` and its tag at rest.</summary>
        public static bool Done(double age, int rows) =>
            age >= Wait(rows - 1) + Math.Max(UnityTuning.ThemeInTime, UnityTuning.Hud19TagPopAfter + UnityTuning.Hud18EnterTime);
    }

    /// <summary>NIGHT19 U3-33.8: the guns' roles (UnityTuning.Hud19Roles): the card's tag and the WEAPONS screen's role card.</summary>
    public static class WeaponRole19
    {
        public static string Tag(string id) => id != null && UnityTuning.Hud19Roles.TryGetValue(id, out var r) ? r.tag : null;
        public static string GoodAgainst(string id) => id != null && UnityTuning.Hud19Roles.TryGetValue(id, out var r) ? r.good : null;
        public static string CombosWith(string id) => id != null && UnityTuning.Hud19Roles.TryGetValue(id, out var r) ? r.combos : null;
    }

    /// <summary>
    /// NIGHT19 U3-33.8: the weapon card's header row: on a swap the gun's name slams in (HudSlam18: from above its size at
    /// full alpha, ease-out) for Hud17SwapNameTime, its last Hud17SwapNameOut fading; the role is hidden meanwhile, then
    /// enters again (alpha ease-out over Hud19CardTagIn). Never both at once (the banned "effects over text").
    /// </summary>
    public static class CardHead19
    {
        public static bool NameShown(double swapAge) => swapAge >= 0 && swapAge < UnityTuning.Hud17SwapNameTime;

        public static float NameScale(double swapAge) => HudSlam18.Scale(swapAge);

        public static float NameAlpha(double swapAge)
        {
            if (!NameShown(swapAge)) return 0f;
            double outAt = UnityTuning.Hud17SwapNameTime - UnityTuning.Hud17SwapNameOut;
            return swapAge <= outAt ? 1f : 1f - UiTween.InCubic((swapAge - outAt) / UnityTuning.Hud17SwapNameOut);
        }

        public static float TagAlpha(double swapAge)
        {
            if (NameShown(swapAge)) return 0f;
            if (swapAge >= double.MaxValue * 0.5) return 1f;
            return UiTween.OutCubic((swapAge - UnityTuning.Hud17SwapNameTime) / UnityTuning.Hud19CardTagIn);
        }
    }

    /// <summary>
    /// NIGHT19 U3-33.8 (builder H): the stamina pips' state (Hud19Tests), stepped once a frame from StaminaHud.Read's fill:
    /// - each pip shows its bar's fill on the frame the sim changed it (S14: a boost reads within 2 frames), always visible
    ///   (no fade at rest: the vitals cluster, like HP);
    /// - a DRAIN FLASH: a chunk spent in one frame (StaminaBarLook.WasSpent: a boost, a dash-jump, a bar emptied) shows the
    ///   spent part WHITE for Hud19DrainFlash s, held as a pale trail StaminaTrailHold s, then easing down at StaminaTrailRate;
    /// - a REFILL TICK: the frame a boost's cell (each cost inside a bar, or a whole bar) is back, that pip punches
    ///   (Hud19TickPunch, ease-out over Hud19TickTime) and the cell is WHITE for Hud19TickWhite s;
    /// - EMPTY under the boost's floor (a press fails): the tracks pulse slowly (TrackPulse 0..1).
    /// No allocation; a new run calls Reset (its first frame compares with nothing).
    /// </summary>
    public sealed class StaminaPips
    {
        public const int Max = 4;
        public int count;
        /// <summary>Each pip's fill and its trail (0..1; trail >= fill).</summary>
        public readonly double[] fill = new double[Max], trail = new double[Max];
        /// <summary>s left of the drain flash (the trail's part WHITE).</summary>
        public double drainFlash;
        /// <summary>Under the boost's floor.</summary>
        public bool empty;
        /// <summary>This step: a chunk was spent (a boost, a dash-jump, a bar emptied); a refill tick started (the reel's events).</summary>
        public bool spentNow, tickedNow;
        readonly double[] tickAge = new double[Max];
        double before = double.NaN, trailFill = double.NaN, trailHold, pulseT;

        public StaminaPips() => Reset();

        /// <summary>The empty pulse now, 0..1 (0 when a boost fires).</summary>
        public double TrackPulse => empty ? StaminaBarLook.Pulse(pulseT, UnityTuning.Hud19EmptyPulse) : 0;

        public void Reset()
        {
            before = double.NaN;
            trailFill = double.NaN;
            trailHold = 0;
            drainFlash = 0;
            pulseT = 0;
            for (int i = 0; i < Max; i++) tickAge[i] = double.MaxValue;
        }

        public void Step(double fillNow, double max, double cost, double floor, double frameDt, bool flashes)
        {
            count = Math.Min(Max, StaminaBarMath.Segments(max));
            for (int i = 0; i < Max; i++) if (tickAge[i] < UnityTuning.Hud19TickTime) tickAge[i] += frameDt;
            // the drain flash and the trail (NIGHT18's trail rule: held after a chunk, then easing down; a rise takes it along)
            bool spent = StaminaBarLook.WasSpent(before, fillNow, UnityTuning.StaminaSpendChunk);
            spentNow = spent;
            tickedNow = false;
            drainFlash = StaminaBarLook.FlashStep(drainFlash, flashes && spent, frameDt, UnityTuning.Hud19DrainFlash);
            if (double.IsNaN(trailFill) || fillNow >= trailFill) trailFill = fillNow;
            else if (spent) trailHold = UnityTuning.StaminaTrailHold;
            else if (trailHold > 0) trailHold = Math.Max(0, trailHold - frameDt);
            else trailFill = Math.Max(fillNow, trailFill + (fillNow - trailFill) * (1 - Math.Exp(-UnityTuning.StaminaTrailRate * frameDt)));
            // the refill tick: every cell boundary (k x cost inside each bar, and each whole bar) crossed upward this frame
            if (!double.IsNaN(before) && fillNow > before && cost > 0)
            {
                // a reward's chunk (a kill's refill: at least StaminaSpendChunk up in one frame) ticks the pip it lands in
                if (fillNow - before >= UnityTuning.StaminaSpendChunk - 1e-9)
                {
                    tickAge[Math.Max(0, Math.Min(count - 1, (int)Math.Ceiling(fillNow - 1e-9) - 1))] = 0;
                    tickedNow = true;
                }
                double step = Math.Min(cost, 1);
                for (int bar = 0; bar < count; bar++)
                {
                    for (int k = 1; k <= UnityTuning.Hud19MaxSlits + 1; k++)
                    {
                        double edge = Math.Min(bar + k * step, bar + 1); // the last cell ends at the bar's end
                        if (before < edge - 1e-9 && fillNow >= edge - 1e-9)
                        {
                            tickAge[bar] = 0;
                            tickedNow = true;
                        }
                        if (edge >= bar + 1 - 1e-9) break;
                    }
                }
            }
            for (int i = 0; i < Max; i++)
            {
                fill[i] = i < count ? StaminaBarMath.SegmentFill(fillNow, i) : 0;
                trail[i] = i < count ? Math.Max(fill[i], StaminaBarMath.SegmentFill(trailFill, i)) : 0;
            }
            empty = StaminaBarLook.Empty(fillNow, floor);
            pulseT = empty ? pulseT + frameDt : 0;
            before = fillNow;
        }

        /// <summary>Pip i's refill tick is running.</summary>
        public bool Ticking(int i) => i >= 0 && i < Max && tickAge[i] < UnityTuning.Hud19TickTime;

        /// <summary>Pip i's refilled cell is WHITE (the tick's first frames).</summary>
        public bool TickWhite(int i) => i >= 0 && i < Max && tickAge[i] < UnityTuning.Hud19TickWhite;

        /// <summary>Pip i's scale: the tick's punch easing out to 1.</summary>
        public float PipScale(int i)
        {
            if (!Ticking(i)) return 1f;
            double k = tickAge[i] / UnityTuning.Hud19TickTime, e = 1 - (1 - k) * (1 - k) * (1 - k);
            return (float)(UnityTuning.Hud19TickPunch + (1 - UnityTuning.Hud19TickPunch) * e);
        }
    }
}
