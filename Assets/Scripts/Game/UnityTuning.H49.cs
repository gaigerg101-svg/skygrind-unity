// NIGHT49 builder H (design.n49.charge49, a view row: no sim bit; NIGHT49.md G1; GG 2026-10-19: "The charge bar under the AIM reticle needs to
// come back"; his 2026-10-18 complaint is the spec: "unclear and misplaced"). The one charge element under the reticle (UiHud38.Arc40.cs,
// Charge49Math / UiCharge49). CLAUDE.md rule 5: Unity-only numbers, each with a comment. SIZES AND TIMINGS ONLY: every colour is a
// UnityTuning.Look49* token (the one palette, DECIDE49 D-3). Every px is at 1080p relative to the reticle's centre (the HUD canvas scales with
// the height: 1 unit = 1 px at 1080p, 0.667 px at 720p). The reticle's box: hud33's ticks end 18 px out (Hud38BarY's comment); nothing here
// may come nearer than that (design.n49.charge49.numbers.minCentrePx), and the element is at least minHeightPx tall.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- which candidate draws (the lead picks one on round 1's a149 frames) ------------------------------------------------------------
        public const int H49CandidateArc = 1; // candidate (a): a thin arc hugging the reticle's bottom. RETIRED (DECIDE49 D-16: lost its blind pair to arc40 0-3 twice)
        public const int H49CandidatePips = 2; // candidate (b): DOOM's segmented pips under the reticle. RETIRED (DECIDE49 D-16: lost 0-3 twice)
        public const int H49CandidateBig = 3; // candidate (c), round 2: the judges' words for arc40 kept (a large arc, a bright fill, a tick at the fill point, clear of the ticks, no dark blob)
        public const int H49Candidate = H49CandidateBig; // the one drawn (the reel overrides it per pass with Charge49Probe.Candidate)

        // --- (c) the large arc with its fill tick (round 2) ---------------------------------------------------------------------------------
        public const float H49BigRadius = 58f; // px: the OUTER edge from the centre (arc40's 60: the judges' "larger"; 2 px in so the tick's outer end stays inside arc40's flash room)
        public const float H49BigThick = 8f; // px: the band (inner edge at 50 px: 32 px clear of the ticks' 18 px end; >= 4 px: minHeightPx)
        public const float H49BigSweepDeg = 100f; // deg: centred straight down (arc40's 104; the ends at x = +-44 px, y = -37 px on the outer edge)
        public const float H49BigSegGapPx = 5f; // px between the PULSE's kill-pip segments along the outer edge (arc40's Hud40SegGapPx)
        public const float H49BigTickW = 3f; // px: the fill tick's width along the arc (a clear mark where the fill is now, as meter42's head)
        public const float H49BigTickOut = 4f; // px: the tick stands out past the band's outer edge (so it reads past the fill's own edge)
        public const float H49BigTickIn = 3f; // px: and in past the inner edge (its inner end 47 px out: far outside the 18 px box)
        public const float H49BigTickCoolAlpha = 0.65f; // the tick on a cooldown's refill: white at this alpha (the refill itself is dim)
        public const float H49BigFillAlpha = 1f; // the fill while charging: the weapon's colour at full brightness (the judges: "bright fill"; candidates 1 / 2 used 0.85)
        public const float H49BigTrackAlpha = 0.3f; // the empty track (a little stronger than 1 / 2's 0.22: the arc's length reads at once)
        public const float H49BigStrokePx = 1f; // px: the dark under-stroke, thinner than 1 / 2's 1.5 (the judges' "dark blob")
        public const float H49BigStrokeAlpha = 0.45f; // the under-stroke's alpha (hud33's shadow colour at this share: an edge on pale floors, never a dark mass)
        public const float H49BigFlashGrowPx = 2f; // px: the band grows outward on the flash frames (arc40's 3 on a 10 px band)

        // --- (a) the thin arc ------------------------------------------------------------------------------------------------------------
        public const float H49ArcRadius = 30f; // px: the arc's OUTER edge from the centre (arc40's was 60: half the distance, so it reads with the reticle, not as a separate bar)
        public const float H49ArcThick = 5f; // px: the band's thickness (>= 4 px at 1080p: the row's minHeightPx; arc40's 10 px band read as "a strange blue bar")
        public const float H49ArcSweepDeg = 96f; // deg: the arc's length, centred straight down (ends 48 deg off the vertical: x = +-22 px, y = -20 px on the outer edge, under the side ticks)
        public const float H49ArcStepDeg = 3f; // deg: one vertex pair every this many degrees (1.6 px of the outer edge at 30 px: a smooth curve at 1080p)
        public const float H49ArcSegGapPx = 3f; // px between two segments (the PULSE's kill pips) along the outer edge (5 segments of about 7 px each)

        // --- (b) DOOM's segmented pips -----------------------------------------------------------------------------------------------------
        public const float H49PipY = -29f; // px: the row's centre under the reticle (its top at -26.5: 8.5 px clear of the ticks' 18 px end)
        public const float H49PipW = 10f; // px: one pip's width (5 pips + 4 gaps = 62 px: about the reticle's own span, DOOM's crosshair-wide segments)
        public const float H49PipH = 5f; // px: one pip's height (>= 4 px at 1080p: the row's minHeightPx)
        public const float H49PipGap = 3f; // px between pips (the gaps make the count read: "segment pips beat a smooth bar", DOOM.md section 5)
        public const float H49PipSlantPx = 2f; // px: each pip's top edge shifted right by this (hud33's 20-degree slash motif on a 5 px cell)
        public const int H49PipCount = 5; // pips for a continuous source (a charge, a cooldown, the heat, the BEND); the PULSE shows its own kill-pip count

        // --- both candidates -----------------------------------------------------------------------------------------------------------------
        public const float H49StrokePx = 1.5f; // px: the dark under-stroke round every band / pip (hud33's text shadow colour): the element reads on pale floors (arc40's n40-r1 lesson)
        public const int H49FlashFrames = 2; // drawn frames of the white flash at full (NIGHT49.md G1: "one 2-frame white flash at full"); frames, not seconds, so it is 2 at any frame rate
        public const float H49FlashGrowPx = 1f; // px: the band grows this much OUTWARD on the flash frames (outward only: the inner edge never moves toward the centre)
        public const float H49TrackAlpha = 0.22f; // the empty track (the weapon's colour at this alpha: the element's length reads before it fills)
        public const float H49ChargeAlpha = 0.85f; // the fill while charging / filling (the weapon's colour, a little under full so FULL steps up)
        public const float H49FullAlpha = 1f; // the fill at FULL (after the flash)
        public const float H49CoolAlpha = 0.4f; // a cooldown: the fill REFILLS left to right in the weapon's colour at this alpha ("a dim refill", NIGHT49.md G1)
        public const float H49HeatAlpha = 0.85f; // the STITCHER's heat rising (as a charge: it fills toward the overheat lock)
        public const float H49FillStep = 0.02f; // fills are rounded to this (the mesh is rebuilt on a change only, as hud38's)
        public const double H49ReadyHoldS = 0.4; // s: after a cooldown ends (the charge shot's, the BLAST's, the STITCHER's lock) the element stays FULL this long, then hides
        public const double H49BendShowS = 1.2; // s: the BEND meter shows this long after it changes (a kill refills it, a FULL bend spends it), and all through a bend (ULTRAKILL's crosshair arcs fade about 1 s after a change, ULTRAKILL.md section 6)
        public const float H49HeatShowMin = 0.001f; // the STITCHER's heat under this is "no heat": the element hides (arc40's Hud40Math.Heat threshold)
        public const float H49HostSize = 150f; // px: the element's host rect (square, centred on the reticle); everything above fits inside it (candidate 3 reaches 58 + 4 + 2 + 1 = 65 px out)

        // --- the a149 reel (FeelReel.N49.Charge.cs) ---------------------------------------------------------------------------------------
        public const double H49ReelHoldMaxS = 3.0; // s of game time a live hold waits for the level it frames (the KESTREL's charge40 is 0.9 s, the THUNDERCLAP's 1.2 s)
        public const int H49ReelSettleFrames = 6; // drawn frames a forced state is held before its capture (the mesh rebuilt, the flash over unless it is the flash frame)
        public const int H49ReelDiffBox = 160; // px at 1080p (each side, scaled with the frame) of the centre box the pixel diff searches (candidate 3 reaches 65 px out)
        public const int H49ReelDiffMin = 40; // 0-255: a channel difference above this between the hidden and the shown frame is the element's pixel
        public const int H49ReelCropPx = 240; // px at 1080p: the blind pair's centre crop (scaled with the frame's height)
        public const int H49ReelPairOutPx = 480; // px: the pair frame's side as written (the 240 px crop upscaled 2x, nearest-neighbour: DECIDE49 D-16)
    }
}
