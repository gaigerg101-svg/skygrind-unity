// NIGHT16 U3-29.7 (builder U): the Unity-only numbers and texts of the feel night's UI (CLAUDE.md rule 5: render, layout
// and tool numbers, each with why; colours come from config through Look.C / the Ui's theme). The web has none of these:
// its HUD has no crosshair movement ticks, no screen transitions, no strip tool, and its hotbar cuts names to 5 letters.
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the movement HUD at the crosshair (UiFeelHud.cs) ----------------------------------------------------------
        // Small cyan ticks under the crosshair: one per dash pip, then a gap and the slide charge's longer tick. Sized to
        // read in the eye's corner without covering what you aim at (the crosshair's arms are 10 x 2; the hit marker's
        // reach is about 20 px).
        public const float FeelHudY = -30; // px under the crosshair's centre (clear of the hit marker and the momentum ring's inner edge)
        // Build 1's slide strip (a 2x crop): at 3 px tall and a 0.22 track the ticks read faint at 1080p: 4 px, a 0.3 track.
        public const float FeelHudPipWidth = 10, FeelHudPipHeight = 4, FeelHudPipGap = 3; // each pip tick
        public const float FeelHudSlideWidth = 28, FeelHudSlideGap = 6; // the slide charge's tick, after a wider gap
        public const float FeelHudBackAlpha = 0.3f; // a spent tick's empty track (white at this alpha, as the hotbar's fresh bar track)
        public const float FeelHudMaxAlpha = 0.9f; // the ticks at full show (the crosshair stays the brightest thing)
        public const double FeelHudHold = 1.0; // s the ticks stay after everything is full (NIGHT16.md: "fade 1 s after everything is full")
        public const double FeelHudFade = 0.25; // s of the fade after the hold
        public const int FeelHudMaxPips = 8; // ticks made once (dash.maxPips 3 + VIGOR's bonus has room)
        // Port-only: a pip that refills and the slide charge's return pulse their tick (the web flashes only spends).
        public const double FeelHudPulseTime = 0.15; // s, 1 -> peak -> 1
        public const double FeelHudPulseScale = 1.4; // the pulse's peak scale

        // --- the screens' transitions (UiFeelHud.cs) ------------------------------------------------------------------
        // 0.15-0.25 s (NIGHT16.md U3-29.7): looks only, never input: the screen under a transition is live at once.
        public const double TransitionResults = 0.22; // DOWN -> RESULTS: RESULTS fades in and rises TransitionRise px
        public const double TransitionDown = 0.18; // the run -> DOWN: the DOWN screen fades in
        public const double TransitionMods = 0.18; // the MODS draft fades in and rises
        public const double TransitionRun = 0.15; // every new run: a dusk curtain over the first frames lifts (menus.js :283 / :1146-1154: 0.15 s; NIGHT16 polish #54, was 0.2 for TITLE -> the run only)
        public const double TransitionTitle = 0.15; // every return to TITLE: TITLE fades in (menus.js's 0.15 s "in" half; NIGHT16 polish #54)
        public const float TransitionRise = 24; // px a screen rises while it fades in
        public const float TransitionCurtainAlpha = 0.85f; // the curtain's start (the panel colour): the run shows through at once

        // --- the style rank plate's zero state (UiFeelHud.cs) -----------------------------------------------------------
        public const double RankPlateFadeIn = 0.25; // s: the plate fades in on the run's first style (no "D DULL" greeting)

        // --- the pad wheel's backdrop (UiRadial.cs) --------------------------------------------------------------------
        // The radial's ring is a translucent band (hud.js .ring), so the world's labels (zone signs, buy prices) read through
        // the open disc: a dim disc of the panel colour sits behind the ring and the options.
        // Build 2's pad capture (Logs/shots/n16-b2/n16-pad-wheel.png): at 0.72 a zone sign (THE GRINDER 1250) still read
        // through the disc: 0.86.
        public const float WheelBackdropAlpha = 0.86f;
        public const float WheelBackdropPad = 28; // px past the ring's outer edge (the options' plates overhang it)

        // --- the headshot marker (Ui.cs) --------------------------------------------------------------------------------
        // A headshot's marker differs by SHAPE (not only the colour or size): a small hollow diamond at the centre inside the
        // four arms (the twin ticks of NIGHT12 read as a thicker X at a glance).
        public const float HeadDiamondRadius = 6; // px from the centre to a corner, x the marker's scale (9 px at headshotScale 1.5: outside the crosshair's 5 px arms, inside the marker's 7.5 px gap on the diagonals)
        public const float HeadDiamondWidth = 2; // px, the arms' width

        // --- the hold-Y wheel hint (UiHotbar.cs) ------------------------------------------------------------------------
        // The pad's wheel (NIGHT15 U3-28.8) had no teaching: a pad player with 3+ guns sees this over the hotbar until they
        // open the wheel once this session. The glyph word follows the pad's set (Y on Xbox; the north button).
        public const float WheelHintGap = 6; // px above the hotbar row

        // --- TITLE's MAP sub-line (UiMeta.cs) ---------------------------------------------------------------------------
        // A short muted line under the MAP row saying what the selected map is (Design/unity-config.json maps.list has ids
        // and names only; the web has one map, so no such text).

        // --- the top-left column (UiMeta.cs LayoutLeft) ---------------------------------------------------------------
        // The lesson card, the hint and the toasts stacked 8 px apart (ToastGap) nearly touched in the n15 tour: one wider gap
        // between every card of the column (UnityTuning.Meta.cs' ToastGap is no longer read).
        public const float LeftColumnGap = 14;

        // --- the move row (UiMoveRow.cs; NIGHT16 polish #35): hud.js's speed readout and wall-jump ticks ------------------
        // The CSS literals (hud.js, the web repo's src/ui/hud.js), then the port's placement in the HP plate.
        public const float MoveRowHeight = 22; // :127 .hud-moverow min-height 22px
        public const float MoveRowGap = 12; // :127 .hud-moverow gap 12px (the speed, the ticks)
        public const float MoveRowSpeedSize = 20; // :129 .hud-speed font-size 20px
        public const float MoveRowUnitGap = 4; // :130 .hud-speed small margin-left 4px
        public const float MoveRowUnitSpacing = 10; // :130 letter-spacing 0.1em (TMP characterSpacing is in hundredths of an em)
        public const string MoveRowUnit = "M/S"; // :329 <small>M/S</small>
        public const string MoveRowWidest = "88.8"; // the readout's box is this wide (right-aligned: the unit never shifts; tabular-nums at :129)
        public const float MoveRowTickWidth = 14, MoveRowTickHeight = 4, MoveRowTickGap = 4; // :131-132 .hud-ticks gap 4px, .hud-tick 14 x 4
        public const float MoveRowTickTrackAlpha = 0.18f; // :132 .hud-tick background rgba(255,255,255,0.18) (.on: colors.usable, :133)
        // Port-only placement: the row sits in the HP plate under the stamina segments (they end 75 px under the plate's
        // top). The plate grows down MoveRowPlateGrow px (92 -> 102; nothing is under it) so the row fits inside it.
        public const float MoveRowBaseline = 96; // px under the plate's top: the readout's baseline (NIGHT17 U3-30.7: under the S42 HP numeral and its 18 px bar)
        public const float MoveRowLeft = 16; // px: the plate's inner left, as the HP number and the segments
        public const float MoveRowPlateGrow = 10;

        // --- the strip tool (FeelStrip.cs) ------------------------------------------------------------------------------
        public const int StripFrames = 10; // frames per strip (NIGHT16.md: N default 10); -skygrindStripFrames N overrides every scenario
        public const int StripMaxFrames = 60; // a group's full frames are kept until its row is drawn (a 1080p frame is 8 MB)
        public const int StripCellWidth = 640; // px per cell on a one-strip sheet (4 x 3 cells: 2560 wide at 16:9)
        public const int StripGutter = 4; // px between cells
        public const float StripStampSheetPx = 16; // the stamp's text height ON THE SHEET (px): the overlay is sized per cell (FeelStrip.StampSize)
        // Per scenario: frames, the gap (grab every K-th frame), the cells a row holds (0: a near-square grid), the cell width.
        // swap / draw read a gun's whole draw: the gun changes at ~125 ms, fire opens at ~217 ms, the flourish settles by
        // ~550 ms (build 1's swap sheet), so swap grabs 36 frames at gap 2 (~0.6 s) and draw 40 at gap 1.
        public const int StripSwapFrames = 36, StripSwapGap = 2, StripSwapCols = 12, StripSwapCellWidth = 384;
        public const int StripDrawFrames = 40, StripDrawCols = 10, StripDrawCellWidth = 480;
        public const int StripPunchPressFrames = 24; // punch-press: from the punch's press (the `punch` event), the fist's travel
        public const int StripLandFrames = 12; // land: the landing dust ring
        // The guns the viewmodel has a kit rig for (Viewmodel's rigs; the rest of the catalog draws nothing): swap's rows in
        // order (the hand ends on the KESTREL), and draw's close look.
        public static readonly string[] StripSwapGuns = { "gauntlet", "shotgun", "nailgun", "rocket", "rail", "revolver" };
        public static readonly string[] StripDrawGuns = { "shotgun", "revolver" };
        // punch / punch-press / parry: the camera turns off the husk (at 1.4 m it fills the centre) so the left fist's path,
        // from the lower left to the centre, crosses open floor. Negative: the camera turns LEFT of the husk, so the husk sits
        // right of centre (+12 is the literal "camera 12 deg right of the husk": the husk then sits left of centre, in the
        // fist's path). Inside punch.coneHalfAngleDeg 35, so the punch still picks it.
        public const double StripMeleeYawOffsetDeg = -12;
        public const double StripLandHeight = 8; // m above open floor (a collider cast) the player is let go from
        public const double StripLandPitchDeg = -58; // the camera looks down at the floor it lands on (build 2: at -35 the feet / the ring were off-frame)
        public const int StripResultsFrames = 14; // results: RESULTS' fade-in and rise (TransitionResults 0.22 s) at gap 1
        public const float StripStampAlpha = 0.8f; // the plate (black) under the stamp
        public const double StripCap = 30; // s per scenario before it is SKIPPED
        public const double StripCapRound = 120; // s for slowmo (a whole round 1 cleared by the aim bot)
    }
}
