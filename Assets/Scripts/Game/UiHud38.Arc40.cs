// NIGHT40 builder H1 (design.n40.arc40, a view row: no sim bit; NIGHT40.md H1, ask 5; GG 2026-10-13 ~22:00: "using a single curved bar under the
// reticle instead of three small bars"). With the row on (and hud38's HUD on: the arc lives in hud38's root, on hud33's reticle), ONE curved bar under
// the reticle replaces hud38's three SlashBar cells for every weapon; it reads the same meter hud38 reads (Hud38Step's inputs), so nothing the bar said
// is lost:
//   - the KESTREL's / THUNDERCLAP's charge: the arc FILLS with the hold's level, eased (level^Hud40FillEasePow: slow first, fast at the end; feel-web.md
//     section 4), bone while filling, CYAN at full with hud38's 0.35 s white flash (+ hud38Ready, as today);
//   - a cooldown (the charge shot's, or a slow gun's own, at least Hud30MinCoolSeconds): the arc DRAINS (the share left, bone at Hud40DrainAlpha), then
//     the flash at full when the gun is usable again;
//   - the PULSE gauntlet: the arc splits into one segment per kill pip (0-5 lit), and the BLAST's 15 s cooldown drains as a thin line inside the arc;
//   - the STITCHER (round 2; survival, where it is held): its heat. Firing, the cool capacity left (1 - heat) shrinks in cyan (still usable); the
//     overheat lock (weapons.nailgun.lockTime 1.5 s) drains in bone, and its end is the ready edge (the flash at full, hud38Ready "nailgun": the one
//     new use of hud38's event, arc40 on only). hud38 drew nothing for the STITCHER (its 1/12 s shot cycle is under Hud30MinCoolSeconds).
// Destiny 2 moved its fusion-rifle charge meter into the reticle, under the centre (feel-web.md section 4). The spin, the grapple row and the ready edge
// stay hud38's; the grapple row moves down under the arc (Hud40GrappleY). Off (arc40 off, -skygrindOff40 / --off40 or any older switch): nothing here is
// built and hud38's three cells draw exactly as before. View only: reads the meter, writes nothing back. Numbers: UnityTuning.Hud40.cs.
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Skygrind.Game
{
    /// <summary>NIGHT40 arc40's statics: the reels read them (the Ui instance is private to SurvivalGame).</summary>
    public static class Hud40
    {
        /// <summary>A NIGHT40 view row is on: its `enabled` and not --off40 / -skygrindOff40 (N40Blocks.ForceOff; every older switch sets it too).</summary>
        public static bool RowOn(GameConfig cfg, string row) => !N40Blocks.ForceOff && cfg != null && cfg.Design("n40." + row + ".enabled");

        /// <summary>The arc is built (arc40 on with hud38) and stepped this frame.</summary>
        public static bool Live;
        /// <summary>The arc now: shown, its fill (0..1 across the segments; -1 hidden), its segments, the blast line (0..1; -1 none) and its state:
        /// "CHARGING" (filling, bone), "COOLDOWN" (draining, bone), "PIPS" (the gauntlet, not ready), "HEAT" (the STITCHER's capacity left, cyan), "FULL" (cyan), "" hidden.</summary>
        public static bool ArcShown;
        public static float ArcFill = -1f, ArcLine = -1f;
        public static int ArcSegs;
        public static string ArcState = "";
        /// <summary>The KESTREL's charge40 turn this frame (Kestrel40Math.None / Tap / Charge / Cannon), and the last tap / cannon turn's measured end:
        /// s from the turn's first frame to its snap's end (the shot is at most one frame earlier), its kind, and how many were timed (UiHud38.Kestrel40.cs).</summary>
        public static int SpinKind, SpinEndKind, SpinEnds;
        public static double SpinEndS = -1;
    }

    /// <summary>The arc's content this frame, from the sim's numbers (pure: Hud40Math.Read).</summary>
    public struct Arc40
    {
        public bool show; // the arc has something to say for this gun (as Meter38.show)
        public bool ready; // full and usable (CYAN; always equal to Meter38.ready for the same inputs)
        public int segs; // 1, or the BLAST's pip count
        public float fill; // 0..1 across the segments (a charge: eased; a cooldown: the share LEFT)
        public float line; // the blast cooldown's line: the share left (0..1), -1 none
        public int mode; // Hud40Math.Rest / Charging / Draining / Pips
    }

    /// <summary>NIGHT40 arc40's rules (pure; EditMode tests: Hud40Tests).</summary>
    public static class Hud40Math
    {
        public const int Rest = 0, Charging = 1, Draining = 2, Pips = 3;

        /// <summary>The arc's left end (deg, counter-clockwise from +x; 270 is straight down): it fills left to right.</summary>
        public static float StartDeg => 270f - UnityTuning.Hud40SweepDeg * 0.5f;

        /// <summary>The charge's shown fill: level^Hud40FillEasePow (0 and 1 exact).</summary>
        public static float Ease(double level) => level <= 0 ? 0f : level >= 1 ? 1f : (float)Math.Pow(level, UnityTuning.Hud40FillEasePow);

        /// <summary>A cooldown's share LEFT (1 just started .. 0 done) from the steps left of its full length.</summary>
        public static float Left(int left, int full) => left <= 0 ? 0f : full <= 0 ? 1f : Clamp01(left / (float)full);

        /// <summary>
        /// The gun in hand's arc: the same inputs as Hud38Math.Read (`charge` a charge gun; `holding` a real hold at `level`; its own cooldown
        /// `cdLeft` / `cdFull`; its charge shot's `chLeft` / `chFull`; the gauntlet's `pips` with `pipsNow` / `pipsMax` and the blast's `blLeft` / `blFull`).
        /// </summary>
        public static Arc40 Read(bool charge, bool holding, double level, int cdLeft, int cdFull, int chLeft, int chFull, bool pips, int pipsNow, int pipsMax, int blLeft, int blFull, double dt)
        {
            var a = new Arc40 { segs = 1, line = -1f, mode = Rest };
            if (pips)
            {
                a.show = true;
                a.mode = Pips;
                a.segs = Math.Max(1, Math.Min(UnityTuning.Hud40MaxSegments, pipsMax));
                a.fill = pipsMax > 0 ? Clamp01(pipsNow / (float)pipsMax) : 1f;
                if (blLeft > 0) a.line = Left(blLeft, Math.Max(blFull, blLeft));
                a.ready = a.fill >= 1f && blLeft <= 0;
                return a;
            }
            bool own = Hud38Math.Shows(cdFull, cdLeft, dt);
            if (charge)
            {
                a.show = true;
                if (holding)
                {
                    a.mode = Charging;
                    a.fill = Ease(level);
                    a.ready = level >= 1;
                }
                else if (own) { a.mode = Draining; a.fill = Left(cdLeft, Math.Max(cdFull, cdLeft)); }
                else if (chLeft > 0) { a.mode = Draining; a.fill = Left(chLeft, Math.Max(chFull, chLeft)); }
                else { a.fill = 1f; a.ready = true; }
                return a;
            }
            if (own)
            {
                a.show = true;
                a.mode = Draining;
                a.fill = Left(cdLeft, Math.Max(cdFull, cdLeft));
                return a;
            }
            a.fill = 1f;
            a.ready = true;
            return a;
        }

        /// <summary>
        /// The STITCHER's arc (round 2): its heat. Usable (not locked): the cool capacity LEFT (1 - heat) in the ready colour (cyan: the gun fires),
        /// shown while any heat is on it; overheated: the lock's share left (heat runs lock / lockTime down to 0, WeaponMath.HeatStep) DRAINS in bone,
        /// and the lock's end is the ready edge (the flash at full, as every cooldown's end on the arc).
        /// </summary>
        public static Arc40 Heat(double heat, bool locked)
        {
            float h = Clamp01((float)heat);
            var a = new Arc40 { segs = 1, line = -1f };
            if (locked)
            {
                a.show = true;
                a.mode = Draining;
                a.fill = h;
                return a;
            }
            a.show = h > 0.001f;
            a.mode = Rest;
            a.fill = 1f - h;
            a.ready = true;
            return a;
        }

        static float Clamp01(float v) => v < 0 ? 0 : v > 1 ? 1 : v;
    }

    /// <summary>The arc as one uGUI Graphic (UiArc24's way: a mesh built in OnPopulateMesh): segments (track + fill) along a circle round the rect's
    /// centre, from Hud40Math.StartDeg through Hud40SweepDeg, and the optional blast line inside them.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UiArc40 : MaskableGraphic
    {
        public int segs = 1;
        public float fill, line = -1f, grow;
        public Color track, bar, lineColor, stroke; // stroke: the dark under-stroke (UiStyle33.Shadow)
        /// <summary>NIGHT42 U1 meter42 (UiHud38.Meter42.cs): a second line inside the first (the normal attack's recovery, 0..1 refilled; -1 none) and a
        /// bright head at the fill's front (`head` 0..1 along the arc; -1 none). Both stay -1 with meter42 off: arc40's mesh exactly as before.</summary>
        public float line2 = -1f, head = -1f, headGrow;
        public Color line2Color, headColor;
        /// <summary>NIGHT50 H charge50: the outer edge's radius (px at 1080p). Hud40Radius (NIGHT46's placement) unless charge50 draws the arc
        /// (UnityTuning.H50ArcRadius); a const initialiser, no Unity API (L-035).</summary>
        public float radius = UnityTuning.Hud40Radius;

        /// <summary>The values changed: rebuild the mesh.</summary>
        public void Changed() => SetVerticesDirty();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Vector2 c = GetPixelAdjustedRect().center;
            float rOut = radius, rIn = rOut - UnityTuning.Hud40Thick;
            float a0 = Hud40Math.StartDeg * Mathf.Deg2Rad, sweep = UnityTuning.Hud40SweepDeg * Mathf.Deg2Rad;
            int n = Mathf.Max(1, segs);
            float gap = n > 1 ? UnityTuning.Hud40SegGapPx / rOut : 0f; // rad: the gap measured along the outer edge
            float len = (sweep - gap * (n - 1)) / n;
            float s = UnityTuning.Hud40StrokePx, sa = s / rOut; // the under-stroke: px out from each side, rad past each end
            // the dark under-strokes first (behind everything): every segment's, and the blast line's
            for (int i = 0; i < n; i++)
            {
                float s0 = a0 + i * (len + gap);
                Band(vh, c, s0 - sa, s0 + len + sa, rIn - s, rOut + grow + s, stroke);
            }
            float l1 = rIn - UnityTuning.Hud40LineGap, l0 = l1 - UnityTuning.Hud40LineThick, lineEnd = a0 + sweep * Mathf.Clamp01(line);
            bool lineOn = line > 0.001f;
            if (lineOn) Band(vh, c, a0 - s / l1, lineEnd + s / l1, l0 - s, l1 + s, stroke);
            for (int i = 0; i < n; i++)
            {
                float s0 = a0 + i * (len + gap);
                Band(vh, c, s0, s0 + len, rIn, rOut, track);
                float f = Hud38Math.Cell(fill, n, i);
                if (f > 0.001f) Band(vh, c, s0, s0 + len * f, rIn, rOut + grow, bar);
            }
            if (lineOn) Band(vh, c, a0, lineEnd, l0, l1, lineColor);
            // NIGHT42 U1 meter42: the second line (under the first) and the head (over the band); nothing when both are -1 (arc40)
            if (line2 > 0.001f)
            {
                float m1 = l0 - UnityTuning.Hud40LineGap, m0 = m1 - UnityTuning.Hud40LineThick, end2 = a0 + sweep * Mathf.Clamp01(line2);
                Band(vh, c, a0 - s / m1, end2 + s / m1, m0 - s, m1 + s, stroke);
                Band(vh, c, a0, end2, m0, m1, line2Color);
            }
            if (head >= 0f)
            {
                float hw = UnityTuning.Meter42HeadDeg * Mathf.Deg2Rad, at = a0 + sweep * Mathf.Clamp01(head);
                float h0 = Mathf.Max(a0, at - hw), h1 = Mathf.Min(a0 + sweep, at + hw * 0.25f);
                Band(vh, c, h0 - sa, h1 + sa, rIn - s - headGrow, rOut + grow + headGrow + s, stroke);
                Band(vh, c, h0, h1, rIn - headGrow, rOut + grow + headGrow, headColor);
            }
        }

        static void Band(VertexHelper vh, Vector2 c, float aa, float ab, float r0, float r1, Color col)
        {
            if (ab <= aa || col.a <= 0.001f) return;
            int n = Mathf.Max(1, Mathf.CeilToInt((ab - aa) * Mathf.Rad2Deg / UnityTuning.Hud40StepDeg));
            int start = vh.currentVertCount;
            Color32 c32 = col;
            for (int k = 0; k <= n; k++)
            {
                float a = Mathf.Lerp(aa, ab, (float)k / n);
                var d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                vh.AddVert(c + d * r1, c32, Vector2.zero); // outer
                vh.AddVert(c + d * r0, c32, Vector2.zero); // inner
            }
            for (int k = 0; k < n; k++)
            {
                int i = start + 2 * k;
                vh.AddTriangle(i, i + 1, i + 3);
                vh.AddTriangle(i, i + 3, i + 2);
            }
        }
    }

    public sealed partial class Ui
    {
        bool arc40On, arc40Read;

        bool Arc40On
        {
            get
            {
                if (!arc40Read && cfg != null)
                {
                    arc40On = Hud40.RowOn(cfg, "arc40");
                    arc40Read = true;
                }
                return arc40On;
            }
        }

        /// <summary>The grapple row's y under the reticle (hud38's, lower with the arc on). UiRail34's bend meter joins this row (the lead's line).</summary>
        float GrappleRowY => Arc40On || Charge50On ? UnityTuning.Hud40GrappleY : UnityTuning.Hud38GrappleY; // NIGHT50 H: charge50 builds the arc too

        UiArc40 h40Arc;
        Arc40 h40Now;
        Color h40ShownBar = new Color(-1, -1, -1, -1);

        /// <summary>From BuildHud38 with arc40 on: the arc under the reticle (hidden until the gun has something to say).</summary>
        void BuildArc40(RectTransform root)
        {
            float size = 2f * (UnityTuning.Hud40Radius + UnityTuning.Hud40FlashGrow + UnityTuning.Hud40StrokePx + 2f);
            var go = new GameObject("Arc40", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiArc40));
            var rt = (RectTransform)go.transform;
            rt.SetParent(root, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(size, size);
            h40Arc = go.GetComponent<UiArc40>();
            h40Arc.raycastTarget = false;
            h40Arc.track = UiTheme.A(UiStyle33.Bone, UnityTuning.Hud38TrackAlpha);
            h40Arc.lineColor = UiTheme.A(UiStyle33.Bone, UnityTuning.Hud40LineAlpha);
            h40Arc.stroke = UiStyle33.Shadow; // hud33's 2 px text shadow colour (#0A0E14 at 80 %): the arc reads on pale floors (n40-r1 f1600)
            go.SetActive(false);
        }

        /// <summary>Bar38's arc branch: `show` and `ready` are Bar38's (hud38's hide rule and ready edge); the content is h40Now (Hud38Step).</summary>
        void Arc40Draw(bool show, bool ready, double frameDt)
        {
            if (h42Live) { Meter42Draw(show, frameDt); return; } // NIGHT42 U1 (design.n42.meter42, UiHud38.Meter42.cs): the one language
            Arc40 a = h40Now;
            GameObject go = h40Arc.gameObject;
            if (go.activeSelf != show) go.SetActive(show);
            Hud38.BarShown = Hud40.ArcShown = show;
            Hud38.BarGun = show ? h38Gun ?? "" : "";
            if (!show)
            {
                Hud38.BarFill = Hud40.ArcFill = Hud40.ArcLine = -1f;
                Hud38.BarCells = Hud40.ArcSegs = 0;
                Hud38.BarState = Hud40.ArcState = "";
                if (h38FlashAge < double.MaxValue) h38FlashAge += frameDt;
                return;
            }
            int n = Math.Max(1, Math.Min(UnityTuning.Hud40MaxSegments, a.segs));
            float fill = Mathf.Round(a.fill / UnityTuning.Hud38FillStep) * UnityTuning.Hud38FillStep;
            float line = a.line < 0 ? -1f : Mathf.Round(a.line / UnityTuning.Hud38FillStep) * UnityTuning.Hud38FillStep;
            float k = Tint38(ready, out Color col);
            if (!ready && a.mode == Hud40Math.Draining) col = UiTheme.A(UiStyle33.Bone, UnityTuning.Hud40DrainAlpha);
            if (H50Live) col = Tint50(ready, a.mode == Hud40Math.Draining, col); // NIGHT50 H charge50: the weapon's Look49 token (the track stays bone)
            AgeFlash38(frameDt);
            float grow = Mathf.Round(UnityTuning.Hud40FlashGrow * k);
            bool dirty = false;
            if (h40Arc.radius != Radius50) { h40Arc.radius = Radius50; dirty = true; } // NIGHT50 H charge50: re-placed (Hud40Radius otherwise)
            if (h50Built && (h40Arc.line2 != -1f || h40Arc.head != -1f)) { h40Arc.line2 = h40Arc.head = -1f; dirty = true; } // NIGHT50 H charge50: meter42's last line / head never linger on arc40's path (the BEND, the reel's force)
            if (h40Arc.segs != n) { h40Arc.segs = n; dirty = true; }
            if (h40Arc.fill != fill) { h40Arc.fill = fill; dirty = true; }
            if (h40Arc.line != line) { h40Arc.line = line; dirty = true; }
            if (h40Arc.grow != grow) { h40Arc.grow = grow; dirty = true; }
            if (col != h40ShownBar) { h40ShownBar = col; h40Arc.bar = col; dirty = true; }
            if (dirty) h40Arc.Changed();
            Hud38.BarFill = Hud40.ArcFill = fill;
            Hud38.BarCells = Hud40.ArcSegs = n;
            Hud40.ArcLine = line;
            Hud38.BarState = ready ? "FULL" : "CHARGING";
            Hud40.ArcState = ready ? (a.fill < 1f ? "HEAT" : "FULL") : a.mode == Hud40Math.Draining ? "COOLDOWN" : a.mode == Hud40Math.Pips ? "PIPS" : "CHARGING";
        }
    }

    // =====================================================================================================================================
    // NIGHT49 builder H (design.n49.charge49, a view row: no sim bit; NIGHT49.md G1). GG 2026-10-19: "The charge bar under the AIM reticle needs
    // to come back"; his 2026-10-18 words are the spec ("unclear and misplaced"): ONE element under the reticle for every charge source, closer
    // and thinner than arc40, never two at once:
    //   KESTREL charge / THUNDERCLAP slug (ChargeShots27 / Charge35: the hold's level, linear), the PULSE LT blast (its kill pips, the BLAST's
    //   cooldown), the STITCHER heat (rising; the overheat lock is its cooldown), the BEND (sim.Bend34Meter: shown through a bend and
    //   H49BendShowS after it changes; the gun in hand wins when it has something to say).
    // States: CHARGING (the fill rises in the weapon's colour), FULL (one H49FlashFrames white flash, then the weapon's colour), COOLDOWN (a dim
    // refill, left to right; its end flashes and holds FULL for H49ReadyHoldS), HEAT; hidden when the gun in hand has no charge, no pips, no heat
    // and no cooldown. Colours: Look49's tokens only (Charge49Math.ColorOf); sizes and timings: UnityTuning.H49.cs. Two candidates behind the row
    // (UnityTuning.H49Candidate; the reel's Charge49Probe.Candidate): 1 a thin arc, 2 DOOM's segmented pips (both RETIRED, DECIDE49 D-16: each
    // lost its blind pair to arc40 0-3 twice: arc40 is larger, its fill and tick read at a glance); 3 (round 2, the default): arc40's size, a
    // bright fill, a white tick across the band at the fill's front, a light under-stroke (no dark blob).
    // With charge49 on, arc40 / meter42 / hud38's cells / hud33's dot arcs stay hidden (UiHud38.cs Bar38, UiHud33.cs Reticle33); the weapon's
    // label stays in the weapon panel. Off (the row off, -skygrindOff49 / --off49 or any older switch): nothing here is built.
    // View only: reads the sim and the loadout, writes nothing back.
    // =====================================================================================================================================

    /// <summary>NIGHT49 charge49's content this frame (pure: Charge49Math).</summary>
    public struct Charge49
    {
        public bool show; // the element draws
        public int source; // Charge49Math.Kestrel / Slug / Blast / Heat / Bend (None hidden)
        public int mode; // Charge49Math.Charging / Full / Cooling / HeatUp
        public float fill; // 0..1 across the element
        public int segs; // 1 (a continuous source), or the PULSE's kill-pip count
    }

    /// <summary>NIGHT49 charge49's statics: the reel reads them (the Ui instance is private to SurvivalGame) and forces a content / a candidate.</summary>
    public static class Charge49Probe
    {
        /// <summary>The element is built (charge49 on with hud38 on hud33's reticle).</summary>
        public static bool Built;
        /// <summary>The reel's override of UnityTuning.H49Candidate (null: the constant).</summary>
        public static int? Candidate;
        /// <summary>The reel's forced content (null: the sim's).</summary>
        public static Charge49? Force;
        /// <summary>The reel's "before" for the blind pair: arc40 / meter42 drawn as NIGHT46 drew them (hud47 not hiding them), the element hidden.</summary>
        public static bool OldArc;
        /// <summary>This frame: shown, its source / state words, fill, segments, the candidate drawn, the flash on this frame.</summary>
        public static bool Shown, FlashNow;
        public static int Source, Segs, CandidateNow;
        public static string SourceName = "", State = "";
        public static float Fill = -1f;
        /// <summary>The element's geometry this frame (canvas units = px at 1080p): its nearest drawn point to the screen centre (with the dark
        /// under-stroke / the coloured band alone) and the band's height; and in this frame's screen px.</summary>
        public static float NearestUnits, NearestFillUnits, HeightUnits, NearestPx, HeightPx;
        /// <summary>Since the run's start: flashes, frames drawn.</summary>
        public static int Flashes, Frames;

        public static int CandidateIs => Candidate ?? UnityTuning.H49Candidate;
    }

    /// <summary>NIGHT49 charge49's rules (pure; EditMode-testable: no Unity API but Color).</summary>
    public static class Charge49Math
    {
        public const int None = 0, Kestrel = 1, Slug = 2, Blast = 3, Heat = 4, Bend = 5;
        public const int Rest = 0, Charging = 1, Full = 2, Cooling = 3, HeatUp = 4;

        public static string SourceWord(int s) => s == Kestrel ? "KESTREL" : s == Slug ? "SLUG" : s == Blast ? "BLAST" : s == Heat ? "HEAT" : s == Bend ? "BEND" : "";
        public static string ModeWord(int m) => m == Charging ? "CHARGING" : m == Full ? "FULL" : m == Cooling ? "COOLDOWN" : m == HeatUp ? "HEAT" : "";

        /// <summary>
        /// The gun in hand's content (hud38's inputs, Hud38Step): `charge` a charge gun (the KESTREL / THUNDERCLAP with the charge row live), `holding`
        /// a real hold at `level`; its charge shot's cooldown `chLeft` / `chFull`; the gauntlet's `pips` (`pipsNow` / `pipsMax`) and the BLAST's cooldown
        /// `blLeft` / `blFull`. A charge gun at rest, a glove with no pips and no cooldown, any other gun: hidden.
        /// </summary>
        public static Charge49 Weapon(string gun, bool charge, bool holding, double level, int chLeft, int chFull, bool pips, int pipsNow, int pipsMax, int blLeft, int blFull)
        {
            var c = new Charge49 { segs = 1 };
            if (pips)
            {
                c.source = Blast;
                if (blLeft > 0)
                {
                    c.show = true;
                    c.mode = Cooling;
                    c.fill = Hud38Math.Refill(blLeft, Math.Max(blFull, blLeft));
                    return c;
                }
                if (pipsNow <= 0 || pipsMax <= 0) return c;
                c.show = true;
                c.segs = Math.Max(1, Math.Min(UnityTuning.Hud40MaxSegments, pipsMax));
                c.fill = Clamp01(pipsNow / (float)pipsMax);
                c.mode = c.fill >= 1f ? Full : Charging;
                return c;
            }
            if (!charge) return c;
            c.source = gun == "shotgun" ? Slug : Kestrel;
            if (holding)
            {
                c.show = true;
                c.fill = Clamp01((float)level);
                c.mode = level >= 1 ? Full : Charging;
                return c;
            }
            if (chLeft > 0)
            {
                c.show = true;
                c.mode = Cooling;
                c.fill = Hud38Math.Refill(chLeft, Math.Max(chFull, chLeft));
            }
            return c;
        }

        /// <summary>The STITCHER's heat (0..1): rising while usable (shown above H49HeatShowMin); the overheat lock is its cooldown (heat runs the lock
        /// down to 0, WeaponMath.HeatStep): the refill is 1 - heat.</summary>
        public static Charge49 HeatOf(double heat, bool locked)
        {
            float h = Clamp01((float)heat);
            var c = new Charge49 { source = Heat, segs = 1 };
            if (locked)
            {
                c.show = true;
                c.mode = Cooling;
                c.fill = 1f - h;
                return c;
            }
            c.show = h > UnityTuning.H49HeatShowMin;
            c.mode = HeatUp;
            c.fill = h;
            return c;
        }

        /// <summary>The BEND meter (0..1, kills refill it): shown inside a bend or while `recent` (it changed within H49BendShowS).</summary>
        public static Charge49 BendOf(double meter, bool inside, bool recent)
        {
            float m = Clamp01((float)meter);
            return new Charge49 { source = Bend, segs = 1, show = inside || recent, fill = m, mode = m >= 1f ? Full : Charging };
        }

        /// <summary>Never two at once: the gun in hand's content when it has something to say, else the BEND's.</summary>
        public static Charge49 Pick(Charge49 weapon, Charge49 bend) => weapon.show ? weapon : bend.show ? bend : default;

        /// <summary>The flash's edge: the same source turned FULL from a fill or a cooldown (a switch never flashes).</summary>
        public static bool Flashes(int prevSource, int prevMode, Charge49 now) =>
            now.show && now.mode == Full && now.source == prevSource && (prevMode == Charging || prevMode == Cooling);

        /// <summary>A cooldown ended into rest (nothing to say now): the element holds FULL for H49ReadyHoldS. Not the BLAST: its cooldown's end
        /// with no pips is not "ready" (with pips the gun's own content speaks: CHARGING, or FULL with its flash).</summary>
        public static bool CoolEnded(int prevSource, int prevMode, Charge49 now) => prevMode == Cooling && prevSource != None && prevSource != Blast && !now.show;

        /// <summary>The weapon's colour (Look49's tokens; DECIDE49 D-3). Every charge source is a player verb, so each reads the usable token today;
        /// a per-weapon token is the lead's call (H's report proposes one).</summary>
        public static uint ColorOf(int source) => UnityTuning.Look49UsableHex;

        /// <summary>The candidate's geometry in canvas units (px at 1080p): the nearest drawn point to the centre with the under-stroke, the band
        /// alone, and the band's height. `segs` is the PULSE's pip count (1 otherwise).</summary>
        public static void Geometry(int candidate, int segs, out float nearest, out float nearestFill, out float height)
        {
            float s = UnityTuning.H49StrokePx;
            if (candidate == UnityTuning.H49CandidateBig)
            {
                // the band 50-58 px out; the fill tick reaches H49BigTickIn further in, with its own under-stroke
                nearestFill = UnityTuning.H49BigRadius - UnityTuning.H49BigThick;
                nearest = Mathf.Max(0f, nearestFill - UnityTuning.H49BigTickIn - UnityTuning.H49BigStrokePx);
                height = UnityTuning.H49BigThick;
                return;
            }
            if (candidate == UnityTuning.H49CandidatePips)
            {
                int n = PipCount(segs);
                nearestFill = float.MaxValue;
                for (int i = 0; i < n; i++)
                {
                    PipQuad(n, i, 0f, out Vector2 a, out Vector2 b, out Vector2 c, out Vector2 d);
                    nearestFill = Mathf.Min(nearestFill, QuadDistance(a, b, c, d));
                }
                nearest = Mathf.Max(0f, nearestFill - s);
                height = UnityTuning.H49PipH;
                return;
            }
            nearestFill = UnityTuning.H49ArcRadius - UnityTuning.H49ArcThick;
            nearest = Mathf.Max(0f, nearestFill - s);
            height = UnityTuning.H49ArcThick;
        }

        /// <summary>Pips drawn for `segs` (the PULSE's count; H49PipCount for a continuous source).</summary>
        public static int PipCount(int segs) => segs > 1 ? Math.Min(UnityTuning.Hud40MaxSegments, segs) : UnityTuning.H49PipCount;

        /// <summary>Pip i of n as a parallelogram (bottom-left, bottom-right, top-right, top-left) round the reticle's centre; `grow` px outward
        /// (down and sideways, never toward the centre: the top edge stays).</summary>
        public static void PipQuad(int n, int i, float grow, out Vector2 bl, out Vector2 br, out Vector2 tr, out Vector2 tl)
        {
            float w = UnityTuning.H49PipW, h = UnityTuning.H49PipH, sl = UnityTuning.H49PipSlantPx;
            float x = (i - (n - 1) * 0.5f) * (w + UnityTuning.H49PipGap), top = UnityTuning.H49PipY + h * 0.5f, bot = UnityTuning.H49PipY - h * 0.5f - grow;
            bl = new Vector2(x - w * 0.5f, bot);
            br = new Vector2(x + w * 0.5f, bot);
            tr = new Vector2(x + w * 0.5f + sl, top);
            tl = new Vector2(x - w * 0.5f + sl, top);
        }

        /// <summary>The origin's distance to a convex quad (0 inside).</summary>
        public static float QuadDistance(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            if (Inside(a, b, c, d)) return 0f;
            return Mathf.Min(Mathf.Min(SegDist(a, b), SegDist(b, c)), Mathf.Min(SegDist(c, d), SegDist(d, a)));
        }

        static bool Inside(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float s1 = Cross(b - a, -a), s2 = Cross(c - b, -b), s3 = Cross(d - c, -c), s4 = Cross(a - d, -d);
            return (s1 >= 0 && s2 >= 0 && s3 >= 0 && s4 >= 0) || (s1 <= 0 && s2 <= 0 && s3 <= 0 && s4 <= 0);
        }

        static float Cross(Vector2 u, Vector2 v) => u.x * v.y - u.y * v.x;

        static float SegDist(Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude > 1e-9f ? Mathf.Clamp01(Vector2.Dot(-a, ab) / ab.sqrMagnitude) : 0f;
            return (a + ab * t).magnitude;
        }

        static float Clamp01(float v) => v < 0 ? 0 : v > 1 ? 1 : v;
    }

    /// <summary>The element as one uGUI Graphic (UiArc40's way): candidate 1 a thin arc round the rect's centre, candidate 2 a row of slanted pips
    /// under it; each segment / pip a track, a fill and a dark under-stroke.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UiCharge49 : MaskableGraphic
    {
        public int candidate = UnityTuning.H49CandidateArc, segs = 1;
        public float fill, grow;
        public Color track, bar, stroke;
        /// <summary>Candidate 3: the fill tick's place (0..1 along the fill; -1 none) and colour.</summary>
        public float tick = -1f;
        public Color tickColor;

        public void Changed() => SetVerticesDirty();

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Vector2 c = GetPixelAdjustedRect().center;
            if (candidate == UnityTuning.H49CandidateBig) Big(vh, c);
            else if (candidate == UnityTuning.H49CandidatePips) Pips(vh, c);
            else Arc(vh, c);
        }

        /// <summary>Candidate 3 (round 2, DECIDE49 D-16): arc40's size kept (the judges: "larger"), a bright fill, a white tick across the band at
        /// the fill's front, a light under-stroke (no dark mass).</summary>
        void Big(VertexHelper vh, Vector2 c)
        {
            float rOut = UnityTuning.H49BigRadius, rIn = rOut - UnityTuning.H49BigThick;
            float sweep = UnityTuning.H49BigSweepDeg * Mathf.Deg2Rad, a0 = (270f - UnityTuning.H49BigSweepDeg * 0.5f) * Mathf.Deg2Rad;
            int n = Mathf.Max(1, segs);
            float gap = n > 1 ? UnityTuning.H49BigSegGapPx / rOut : 0f;
            float len = (sweep - gap * (n - 1)) / n;
            float s = UnityTuning.H49BigStrokePx, sa = s / rOut;
            // the tick's angle: its segment and the share inside it (a0 is the LEFT end; the fill runs left to right)
            bool tickOn = tick >= 0f && tick < 1f && tickColor.a > 0.001f;
            float at = 0f;
            if (tickOn)
            {
                int k = Mathf.Min(n - 1, Mathf.FloorToInt(tick * n));
                at = a0 + k * (len + gap) + len * Mathf.Clamp01(tick * n - k);
            }
            float tw = UnityTuning.H49BigTickW * 0.5f / rOut, t0 = rIn - UnityTuning.H49BigTickIn, t1 = rOut + grow + UnityTuning.H49BigTickOut;
            for (int i = 0; i < n; i++)
            {
                float e0 = a0 + i * (len + gap);
                Band(vh, c, e0 - sa, e0 + len + sa, rIn - s, rOut + grow + s, stroke);
            }
            if (tickOn) Band(vh, c, at - tw - sa, at + tw + sa, t0 - s, t1 + s, stroke);
            for (int i = 0; i < n; i++)
            {
                float e0 = a0 + i * (len + gap);
                Band(vh, c, e0, e0 + len, rIn, rOut, track);
                float f = Hud38Math.Cell(fill, n, i);
                if (f > 0.001f) Band(vh, c, e0, e0 + len * f, rIn, rOut + grow, bar);
            }
            if (tickOn) Band(vh, c, at - tw, at + tw, t0, t1, tickColor);
        }

        void Arc(VertexHelper vh, Vector2 c)
        {
            float rOut = UnityTuning.H49ArcRadius, rIn = rOut - UnityTuning.H49ArcThick;
            float sweep = UnityTuning.H49ArcSweepDeg * Mathf.Deg2Rad, a0 = (270f - UnityTuning.H49ArcSweepDeg * 0.5f) * Mathf.Deg2Rad;
            int n = Mathf.Max(1, segs);
            float gap = n > 1 ? UnityTuning.H49ArcSegGapPx / rOut : 0f;
            float len = (sweep - gap * (n - 1)) / n;
            float s = UnityTuning.H49StrokePx, sa = s / rOut;
            // the fill runs left to right: a0 (270 - sweep / 2, counter-clockwise from +x) is the arc's LEFT end and the angle grows to the right
            // (arc40's Hud40Math.StartDeg)
            for (int i = 0; i < n; i++)
            {
                float e0 = a0 + i * (len + gap);
                Band(vh, c, e0 - sa, e0 + len + sa, rIn - s, rOut + grow + s, stroke);
            }
            for (int i = 0; i < n; i++)
            {
                float e0 = a0 + i * (len + gap);
                Band(vh, c, e0, e0 + len, rIn, rOut, track);
                float f = Hud38Math.Cell(fill, n, i);
                if (f > 0.001f) Band(vh, c, e0, e0 + len * f, rIn, rOut + grow, bar);
            }
        }

        void Pips(VertexHelper vh, Vector2 c)
        {
            int n = Charge49Math.PipCount(segs);
            float s = UnityTuning.H49StrokePx;
            for (int i = 0; i < n; i++)
            {
                Charge49Math.PipQuad(n, i, grow, out Vector2 a, out Vector2 b, out Vector2 d, out Vector2 e);
                Quad(vh, c, a + new Vector2(-s, -s), b + new Vector2(s, -s), d + new Vector2(s, s), e + new Vector2(-s, s), stroke);
            }
            for (int i = 0; i < n; i++)
            {
                Charge49Math.PipQuad(n, i, 0f, out Vector2 a, out Vector2 b, out Vector2 d, out Vector2 e);
                Quad(vh, c, a, b, d, e, track);
                float f = Hud38Math.Cell(fill, n, i);
                if (f <= 0.001f) continue;
                Charge49Math.PipQuad(n, i, grow, out a, out b, out d, out e);
                // the pip fills left to right: its right edge moves with f
                Vector2 bf = Vector2.Lerp(a, b, f), df = Vector2.Lerp(e, d, f);
                Quad(vh, c, a, bf, df, e, bar);
            }
        }

        static void Quad(VertexHelper vh, Vector2 c, Vector2 bl, Vector2 br, Vector2 tr, Vector2 tl, Color col)
        {
            if (col.a <= 0.001f) return;
            int i = vh.currentVertCount;
            Color32 c32 = col;
            vh.AddVert(c + bl, c32, Vector2.zero);
            vh.AddVert(c + br, c32, Vector2.zero);
            vh.AddVert(c + tr, c32, Vector2.zero);
            vh.AddVert(c + tl, c32, Vector2.zero);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }

        static void Band(VertexHelper vh, Vector2 c, float aa, float ab, float r0, float r1, Color col)
        {
            if (ab <= aa || col.a <= 0.001f) return;
            int n = Mathf.Max(1, Mathf.CeilToInt((ab - aa) * Mathf.Rad2Deg / UnityTuning.H49ArcStepDeg));
            int start = vh.currentVertCount;
            Color32 c32 = col;
            for (int k = 0; k <= n; k++)
            {
                float a = Mathf.Lerp(aa, ab, (float)k / n);
                var d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                vh.AddVert(c + d * r1, c32, Vector2.zero);
                vh.AddVert(c + d * r0, c32, Vector2.zero);
            }
            for (int k = 0; k < n; k++)
            {
                int i = start + 2 * k;
                vh.AddTriangle(i, i + 1, i + 3);
                vh.AddTriangle(i, i + 3, i + 2);
            }
        }
    }

    public sealed partial class Ui
    {
        bool charge49On, charge49Read;

        /// <summary>design.n49.charge49 (NightRows.ViewOn: off with -skygrindOff49 / --off49 or any older switch), read once a session.</summary>
        bool Charge49On
        {
            get
            {
                if (!charge49Read && cfg != null)
                {
                    charge49On = NightRows.ViewOn(cfg, "charge49");
                    charge49Read = true;
                }
                return charge49On;
            }
        }

        UiCharge49 h49El;
        Canvas h49Canvas;
        Charge49 h49Now;
        bool h49Read;
        int h49PrevSource = Charge49Math.None, h49PrevMode = Charge49Math.Rest, h49FlashLeft;
        double h49ReadyAge = double.MaxValue, h49BendAge = double.MaxValue, h49BendSeen = -1;
        int h49ReadySource;
        string h49Gun;
        Color h49ShownBar = new Color(-1, -1, -1, -1), h49ShownTrack = new Color(-1, -1, -1, -1);

        /// <summary>From BuildHud38 with charge49 on: the element's host on the reticle (hidden until something charges).</summary>
        void BuildCharge49(RectTransform root)
        {
            var go = new GameObject("Charge49", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiCharge49));
            var rt = (RectTransform)go.transform;
            rt.SetParent(root, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(UnityTuning.H49HostSize, UnityTuning.H49HostSize);
            h49El = go.GetComponent<UiCharge49>();
            h49El.raycastTarget = false;
            h49El.stroke = UiStyle33.Shadow;
            h49El.candidate = Charge49Probe.CandidateIs;
            h49Canvas = go.GetComponentInParent<Canvas>();
            go.SetActive(false);
            Charge49Probe.Built = true;
            game.OnNewRun += () =>
            {
                h49PrevSource = Charge49Math.None;
                h49PrevMode = Charge49Math.Rest;
                h49FlashLeft = 0;
                h49ReadyAge = h49BendAge = double.MaxValue;
                h49BendSeen = -1;
                Charge49Probe.Flashes = Charge49Probe.Frames = 0;
            };
        }

        /// <summary>From Hud38Step (the gun in hand read; UiHud38.cs's one line): this frame's weapon content.</summary>
        void Charge49Read(Loadout lo, string gun, bool charge, bool holding, double level, int chLeft, int chFull, bool pips, int pipsNow, int pipsMax, int blLeft, int blFull)
        {
            h49Read = true;
            h49Now = gun == "nailgun" && lo != null ? Charge49Math.HeatOf(lo.Heat, lo.Overheated) : Charge49Math.Weapon(gun, charge, holding, level, chLeft, chFull, pips, pipsNow, pipsMax, blLeft, blFull);
        }

        /// <summary>Bar38's charge49 branch (UiHud38.cs): the one element. `have`: the sim and the gun in hand were read this frame.</summary>
        void Charge49Draw(bool have, double frameDt)
        {
            Charge49 weapon = have && h49Read ? h49Now : default;
            h49Read = false;
            h49Now = default;
            // the BEND: its meter's changes start its show clock; inside a bend it shows throughout
            SurvivalSim sim = game.sim;
            Charge49 bend = default;
            if (sim != null && sim.Bend34On && sim.flow == Flow.Playing)
            {
                double meter = sim.Bend34Meter;
                if (h49BendSeen >= 0 && Math.Abs(meter - h49BendSeen) > 1e-6) h49BendAge = 0;
                h49BendSeen = meter;
                bend = Charge49Math.BendOf(meter, sim.Bend34Inside, h49BendAge < UnityTuning.H49BendShowS);
                if (h49BendAge < double.MaxValue) h49BendAge = h49BendAge > UnityTuning.H49BendShowS ? double.MaxValue : h49BendAge + frameDt;
            }
            if (h38Gun != h49Gun)
            {
                // a switch: the new gun as it is (no ready hold of the old gun's cooldown, no flash)
                h49Gun = h38Gun;
                h49ReadyAge = double.MaxValue;
                h49PrevSource = Charge49Math.None;
                h49PrevMode = Charge49Math.Rest;
            }
            Charge49 c = Charge49Math.Pick(weapon, bend);
            if (Charge49Probe.Force.HasValue) c = Charge49Probe.Force.Value;
            if (Charge49Probe.OldArc) c = default; // the reel's NIGHT46 "before": arc40 draws, the element does not
            // a cooldown that ended into rest: FULL for H49ReadyHoldS (with the flash), unless something new speaks
            if (c.show) h49ReadyAge = double.MaxValue;
            else if (Charge49Math.CoolEnded(h49PrevSource, h49PrevMode, c))
            {
                h49ReadyAge = 0;
                h49ReadySource = h49PrevSource;
            }
            if (!c.show && h49ReadyAge < UnityTuning.H49ReadyHoldS)
            {
                c = new Charge49 { show = true, source = h49ReadySource, mode = Charge49Math.Full, fill = 1f, segs = 1 };
                h49ReadyAge += frameDt;
            }
            else if (!c.show) h49ReadyAge = double.MaxValue;
            // the flash: the same source turned FULL from a fill or a cooldown; a switch shows the new source as it is
            if (Charge49Math.Flashes(h49PrevSource, h49PrevMode, c))
            {
                h49FlashLeft = UnityTuning.H49FlashFrames;
                Charge49Probe.Flashes++;
            }
            else if (c.source != h49PrevSource) h49FlashLeft = 0;
            h49PrevSource = c.show ? c.source : Charge49Math.None;
            h49PrevMode = c.show ? c.mode : Charge49Math.Rest;

            GameObject go = h49El.gameObject;
            if (go.activeSelf != c.show) go.SetActive(c.show);
            Charge49Probe.Shown = c.show;
            Charge49Probe.FlashNow = false;
            int cand = Charge49Probe.CandidateIs;
            Charge49Probe.CandidateNow = cand;
            if (!c.show)
            {
                h49FlashLeft = 0;
                Charge49Probe.Source = Charge49Math.None;
                Charge49Probe.SourceName = Charge49Probe.State = "";
                Charge49Probe.Fill = -1f;
                Charge49Probe.Segs = 0;
                Charge49Probe.NearestUnits = Charge49Probe.NearestFillUnits = Charge49Probe.HeightUnits = Charge49Probe.NearestPx = Charge49Probe.HeightPx = -1f;
                return;
            }
            Charge49Probe.Frames++;
            Color col = Look.Hex(Charge49Math.ColorOf(c.source));
            float alpha = c.mode == Charge49Math.Full ? UnityTuning.H49FullAlpha : c.mode == Charge49Math.Cooling ? UnityTuning.H49CoolAlpha : c.mode == Charge49Math.HeatUp ? UnityTuning.H49HeatAlpha : UnityTuning.H49ChargeAlpha;
            bool big = cand == UnityTuning.H49CandidateBig;
            if (big && (c.mode == Charge49Math.Charging || c.mode == Charge49Math.HeatUp)) alpha = UnityTuning.H49BigFillAlpha; // candidate 3: a bright fill
            Color bar = UiTheme.A(col, alpha), track = UiTheme.A(col, big ? UnityTuning.H49BigTrackAlpha : UnityTuning.H49TrackAlpha);
            float grow = 0f;
            if (h49FlashLeft > 0)
            {
                bar = Color.white;
                grow = big ? UnityTuning.H49BigFlashGrowPx : UnityTuning.H49FlashGrowPx;
                h49FlashLeft--;
                Charge49Probe.FlashNow = true;
            }
            int segs = Math.Max(1, Math.Min(UnityTuning.Hud40MaxSegments, c.segs));
            float fill = Mathf.Round(c.fill / UnityTuning.H49FillStep) * UnityTuning.H49FillStep;
            // candidate 3's tick at the fill's front (not at FULL: the whole band is the read then); white, dimmer on a cooldown's refill
            float tick = big && c.mode != Charge49Math.Full && fill < 1f ? fill : -1f;
            Color tickCol = c.mode == Charge49Math.Cooling ? UiTheme.A(Color.white, UnityTuning.H49BigTickCoolAlpha) : Color.white;
            Color stroke = big ? UiTheme.A(UiStyle33.Shadow, UiStyle33.Shadow.a * UnityTuning.H49BigStrokeAlpha) : UiStyle33.Shadow;
            UiCharge49 el = h49El;
            bool dirty = false;
            if (el.tick != tick) { el.tick = tick; dirty = true; }
            if (el.tickColor != tickCol) { el.tickColor = tickCol; dirty = true; }
            if (el.stroke != stroke) { el.stroke = stroke; dirty = true; }
            if (el.candidate != cand) { el.candidate = cand; dirty = true; }
            if (el.segs != segs) { el.segs = segs; dirty = true; }
            if (el.fill != fill) { el.fill = fill; dirty = true; }
            if (el.grow != grow) { el.grow = grow; dirty = true; }
            if (bar != h49ShownBar) { h49ShownBar = bar; el.bar = bar; dirty = true; }
            if (track != h49ShownTrack) { h49ShownTrack = track; el.track = track; dirty = true; }
            if (dirty) el.Changed();
            Charge49Probe.Source = c.source;
            Charge49Probe.SourceName = Charge49Math.SourceWord(c.source);
            Charge49Probe.State = Charge49Math.ModeWord(c.mode);
            Charge49Probe.Fill = fill;
            Charge49Probe.Segs = segs;
            Charge49Math.Geometry(cand, segs, out float near, out float nearFill, out float height);
            float scale = h49Canvas != null ? h49Canvas.rootCanvas.scaleFactor : 1f;
            Charge49Probe.NearestUnits = near;
            Charge49Probe.NearestFillUnits = nearFill;
            Charge49Probe.HeightUnits = height;
            Charge49Probe.NearestPx = near * scale;
            Charge49Probe.HeightPx = height * scale;
        }
    }

    // =====================================================================================================================================
    // NIGHT50 builder H (design.n50.charge50, a view row: no sim bit; NIGHT50.md G1, DECIDE50 D-5). GG 2026-10-19 ~11:20: "The charge bar under
    // the AIM reticle needs to come back I don't know why it's gone". NIGHT49's three blind pairs preferred arc40, so arc40 comes back as the
    // element (no new element): arc40 + meter42 drawn again over hud47's reticle-alone rule (UiHud38.cs Bar38), re-placed closer under the reticle
    // (UnityTuning.H50ArcRadius; its deepest drawn pixel stays outside the 18 px reticle box), the fill and FULL in the weapon's Look49 token
    // (every player gun reads the usable cyan; the track and the lines stay bone), the STITCHER's heat kept (arc40's), and the BEND meter on the
    // arc when the gun in hand has nothing to say (shown through a bend and H50BendShowS after it changes: charge49's rule). charge49 is never
    // built with it (never two at once). Off (the row off, -skygrindOff50 / --off50 or any older switch): nothing here runs and the HUD is
    // NIGHT49's (hud47 hides the arc). The reel (a150) reads Charge50Probe and can force NIGHT49's hidden frame (Hide), NIGHT46's placement
    // (Old: Hud40Radius, bone / cyan) or a content (Force). View only: reads the sim and the loadout, writes nothing back.
    // =====================================================================================================================================

    /// <summary>NIGHT50 charge50's statics: the reel reads them (the Ui instance is private to SurvivalGame) and forces a frame.</summary>
    public static class Charge50Probe
    {
        /// <summary>charge50 is on in this session (the arc built over hud47's rule).</summary>
        public static bool Built;
        /// <summary>The reel's "off" frame: the arc hidden as NIGHT49 hid it (hud47's rule), the same pose.</summary>
        public static bool Hide;
        /// <summary>The reel's NIGHT46 frame for the blind pair: arc40 / meter42 at Hud40Radius in bone / cyan (not hidden by hud47).</summary>
        public static bool Old;
        /// <summary>The reel's forced content (null: the sim's): drawn on arc40's path (meter42's lines off for that frame).</summary>
        public static Arc40? Force;
        /// <summary>This frame: shown, the source ("BEND" or the gun id), the state word (Hud40.ArcState), the fill, the radius drawn.</summary>
        public static bool Shown;
        public static string Source = "", State = "";
        public static float Fill = -1f, Radius = -1f;
        /// <summary>The arc's deepest possible drawn point to the screen centre (meter42's second line or the head, each with its under-stroke)
        /// and the band's height: canvas units (px at 1080p) and this frame's px.</summary>
        public static float NearestUnits = -1f, HeightUnits = -1f, NearestPx = -1f, HeightPx = -1f;
        /// <summary>Since the run's start: frames drawn, BEND frames drawn.</summary>
        public static int Frames, BendFrames;
    }

    /// <summary>NIGHT50 charge50's rules (pure; EditMode-testable: no Unity API).</summary>
    public static class Charge50Math
    {
        /// <summary>The weapon's Look49 token (CLAUDE.md rule 8: cyan is player-usable; every charge source is a player gun or verb).</summary>
        public static uint TokenOf(string gun) => UnityTuning.Look49UsableHex;

        /// <summary>The arc's deepest possible drawn point (px at 1080p) at outer radius `r`: meter42's second line under the first, with its
        /// under-stroke, or the head's inner edge (Meter42HeadGrow inside the band, with its stroke), whichever is deeper.</summary>
        public static float Nearest(float r)
        {
            float rIn = r - UnityTuning.Hud40Thick, s = UnityTuning.Hud40StrokePx;
            float line2 = rIn - 2f * (UnityTuning.Hud40LineGap + UnityTuning.Hud40LineThick) - s;
            float head = rIn - s - UnityTuning.Meter42HeadGrow;
            return Math.Max(0f, Math.Min(line2, head));
        }

        /// <summary>The BEND on the arc: only when the gun in hand has nothing to say, and inside a bend or within H50BendShowS of a change.</summary>
        public static bool BendShows(bool gunShows, bool inside, bool recent) => !gunShows && (inside || recent);
    }

    public sealed partial class Ui
    {
        bool charge50On, charge50Read, h50Built, h50Bend, h50Forced;
        double h50BendAge = double.MaxValue, h50BendSeen = -1;
        Canvas h50Canvas;

        /// <summary>design.n50.charge50 (NightRows.ViewOn: off with -skygrindOff50 / --off50 or any older switch), read once a session.</summary>
        bool Charge50On
        {
            get
            {
                if (!charge50Read && cfg != null)
                {
                    charge50On = NightRows.ViewOn(cfg, "charge50");
                    charge50Read = true;
                }
                return charge50On;
            }
        }

        /// <summary>charge50 draws this frame in its own look (not the reel's NIGHT46 frame).</summary>
        bool H50Live => h50Built && !Charge50Probe.Old;

        /// <summary>The arc's outer radius now: H50ArcRadius with charge50 drawing, NIGHT46's Hud40Radius otherwise.</summary>
        float Radius50 => H50Live ? UnityTuning.H50ArcRadius : UnityTuning.Hud40Radius;

        /// <summary>The gun in hand's Look49 token.</summary>
        Color Token50 => Look.Hex(Charge50Math.TokenOf(h38Gun));

        /// <summary>From BuildHud38 with charge50 on (after BuildArc40).</summary>
        void BuildCharge50()
        {
            h50Built = h40Arc != null;
            Charge50Probe.Built = h50Built;
            if (!h50Built) return;
            h50Canvas = h40Arc.GetComponentInParent<Canvas>();
            game.OnNewRun += () =>
            {
                h50BendAge = double.MaxValue;
                h50BendSeen = -1;
                Charge50Probe.Frames = Charge50Probe.BendFrames = 0;
            };
        }

        /// <summary>From Hud38Step (after the gun in hand is read): the BEND when the gun has nothing to say; the reel's forced content.</summary>
        void Charge50Step(ref Meter38 m, bool have, SurvivalSim sim, double frameDt)
        {
            h50Bend = h50Forced = false;
            if (Charge50Probe.Force.HasValue)
            {
                h40Now = Charge50Probe.Force.Value;
                m.show = h40Now.show;
                h42Live = false; // arc40's path draws the forced content
                h50Forced = true;
                return;
            }
            if (!have || sim == null || !sim.Bend34On || sim.flow != Flow.Playing || Charge50Probe.Old) return;
            double meter = sim.Bend34Meter;
            if (h50BendSeen >= 0 && Math.Abs(meter - h50BendSeen) > 1e-6) h50BendAge = 0;
            h50BendSeen = meter;
            bool recent = h50BendAge < UnityTuning.H50BendShowS;
            if (h50BendAge < double.MaxValue) h50BendAge = h50BendAge > UnityTuning.H50BendShowS ? double.MaxValue : h50BendAge + frameDt;
            bool holding = h38ReadyAge < UnityTuning.Hud38HideAfter && h38ReadyAge > 0; // hud38's ready hold after a cooldown speaks first
            if (holding || !Charge50Math.BendShows(m.show, sim.Bend34Inside, recent)) return;
            float f = (float)Math.Max(0, Math.Min(1, meter));
            h40Now = new Arc40 { show = true, segs = 1, line = -1f, fill = f, mode = Hud40Math.Charging, ready = false };
            m.show = true;
            h42Live = false;
            h50Bend = true;
        }

        /// <summary>Bar38's show with charge50 on: hud47's rule does not hide the arc; the reel's Hide is NIGHT49's frame (hidden).</summary>
        bool Charge50Show(bool show, bool gunShows) => !Charge50Probe.Hide && show;

        /// <summary>Bar38's ready with charge50 on: the BEND is never the gun's ready (no FULL flash); a forced content carries its own.</summary>
        bool Charge50Ready(bool ready) => h50Bend ? false : h50Forced ? h40Now.ready : ready;

        /// <summary>The arc's fill colour in the weapon's token: the hud38 flash (white to the token), FULL / HEAT at Tint38's alpha, a cooldown's
        /// refill at H50CoolAlpha, a charge at H50ChargeAlpha, the BEND at H50BendAlpha. `was` is arc40's colour (its alpha is kept when ready).</summary>
        Color Tint50(bool ready, bool draining, Color was)
        {
            Color tok = Token50;
            if (h50Bend) return UiTheme.A(tok, UnityTuning.H50BendAlpha);
            if (ready)
            {
                if (h38FlashAge < UnityTuning.Hud38FlashTime) return Color.Lerp(Color.white, tok, (float)(h38FlashAge / UnityTuning.Hud38FlashTime));
                return UiTheme.A(tok, was.a);
            }
            return UiTheme.A(tok, draining ? UnityTuning.H50CoolAlpha : UnityTuning.H50ChargeAlpha);
        }

        /// <summary>After Bar38's arc draw: the probe's frame read.</summary>
        void Charge50Probe50()
        {
            bool shown = h40Arc != null && h40Arc.gameObject.activeSelf;
            Charge50Probe.Shown = shown;
            Charge50Probe.Source = shown ? (h50Bend ? "BEND" : h38Gun ?? "") : "";
            Charge50Probe.State = shown ? Hud40.ArcState : "";
            Charge50Probe.Fill = shown ? Hud40.ArcFill : -1f;
            float r = Radius50;
            Charge50Probe.Radius = r;
            float scale = h50Canvas != null ? h50Canvas.rootCanvas.scaleFactor : 1f;
            float near = Charge50Math.Nearest(r), height = UnityTuning.Hud40Thick;
            Charge50Probe.NearestUnits = shown ? near : -1f;
            Charge50Probe.HeightUnits = shown ? height : -1f;
            Charge50Probe.NearestPx = shown ? near * scale : -1f;
            Charge50Probe.HeightPx = shown ? height * scale : -1f;
            if (!shown) return;
            Charge50Probe.Frames++;
            if (h50Bend) Charge50Probe.BendFrames++;
        }
    }
}
