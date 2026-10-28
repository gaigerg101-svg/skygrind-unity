// NIGHT30 builder V (item 8, design.n30.look30; DESIGN30 E; D-4): THE LOOK's Unity-only numbers (CLAUDE.md rule 5: each with a comment). View only.
// R2 (Research/2026-10-09-wave30/R2-look-hud.md): open MIT dither shaders use precision 32 and dither scale 2; flicker safety: at most 3 flashes a second,
// under 25 % of the screen, nothing flashing longer than 5 s; never more than one panel flickering at a time.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the world: colour quantise + ordered 4x4 Bayer dither (in the CRT pass, after post-processing, before the HUD) --------------------------
        public const float Look30Levels = 32f; // levels per channel (R2's "precision 32"): subtle banding, the palette intact
        public const float Look30DitherScale = 2f; // px per Bayer cell (R2's "dither scale 2"): the pattern reads as grain, not a grid
        public const float Look30Strength = 0.35f; // the blend from the source to the quantised colour (DESIGN30 E: subtle; GUESS until a frame is looked at)
        public const float Look30GrainDarkLuma = 0.15f; // luminance under which the CRT's grain is halved (D-4: "half the grain in the dark")
        public const float Look30GrainDarkScale = 0.5f; // the grain's share in the dark
        public const float Look30PaletteMinGap = 0.15f; // RGB distance the four palette colours must keep after the quantise (the unit test)

        // --- PIXEL SIZE (VIDEO, design D-4: off by default): the CRT pass's pixel block in screen px ------------------------------------------------
        public const string Pixel30Setting = "pixelSize30"; // the settings key
        public const string Pixel30Label = "PIXEL SIZE"; // its label
        public const string Pixel30Desc = "Draw the world in bigger pixels (1 is off; the HUD stays sharp)"; // its line
        public const double Pixel30Default = 1, Pixel30Min = 1, Pixel30Max = 4, Pixel30Step = 1; // 1 = off (D-4), up to 4 px blocks

        // --- HUD FLICKER (ACCESS): the accessibility switch for every HUD fx below -------------------------------------------------------------------
        public const string HudFx30Setting = "hudFx30"; // the settings key
        public const string HudFx30Label = "HUD FLICKER"; // its label
        public const string HudFx30Desc = "The HUD's old-TV scanlines, flicker, glow and buzz (off: a still, plain HUD)"; // its line
        public const bool HudFx30Default = true; // on (D-4); a player who needs it off flips this

        // --- the HUD's panels (HudFx30.cs) ---------------------------------------------------------------------------------------------------------
        public const int Look30ScanPeriod = 2; // px: the scanline texture's period (1 dark row in 2)
        public const float Look30ScanAlpha = 0.08f; // the dark rows' alpha over a panel (DESIGN30 E: 8 %)
        public const float Look30EdgeAlpha = 0.10f; // the faint cyan edge glow at rest
        public const float Look30GlowAlpha = 0.55f; // the edge glow's peak on a state change (READY, a kill, a hit taken)
        public const double Look30GlowSeconds = 0.2; // s: the glow's fade (DESIGN30 E: "glows 0.2 s")
        public const int Look30FlickerFrames = 3; // drawn frames a value change flickers (DESIGN30 E: 2-4)
        public const float Look30FlickerHigh = 0.32f, Look30FlickerLow = 0.0f; // the scanline overlay's alpha on a flicker's bright / dark frame (it never hides the panel)
        public const double Look30FlickerGap = 0.34; // s between two flicker starts, any panel (at most 3 a second: R2)
        public const double Look30BuzzBelow = 0.25; // the health share under which the vitals panel buzzes (DESIGN30 E)
        public const float Look30BuzzPx = 2f; // px: the buzz's largest horizontal jitter (1-2 px)
        public const float Look30BuzzHz = 12f; // Hz: the buzz's rate (a jitter, never a flash: the panel's brightness is unchanged)
        public const float Look30PanelInset = 3f; // px: the overlays stand this far inside a panel's rect (the theme's cut corners stay clean)

        // --- bolder light (LightKit29 with look30): the accent light and the view-only beams --------------------------------------------------------
        public const float Look30AccentBoost = 1.6f; // x the room's accent light (slot 1: the amber / orange row the plan points at its hero object)
        public const float Look30BeamGain = 0.12f; // the beams' additive brightness x the socket's colour (faint: a shaft in the dusk, never a wash)
        public const float Look30BeamPoolHeight = 6f; // m: a pool socket's shaft comes down from this high
        public const float Look30BeamLength = 5f; // m: an accent socket's shaft along its normal
        public const float Look30BeamTilt = 0.6f; // the accent shaft's downward tip (added to the normal before normalising)
        public const float Look30BeamNarrow = 0.35f; // m: a shaft's narrow end
        public const float Look30BeamSpread = 2.2f; // m: an accent shaft's wide end
        public const float Look30BeamMaxWidth = 2.2f; // m: round 5: no shaft's wide end is wider (f9's 5 m pool would have made a wall of light)
        public const float Look30BeamKeepClear = 5f; // m (horizontal): round 5: no shaft within this of a room's spawn (the player's entry, the camera) or a pickup (its own glow)
        public const float Look30PickupScale = 0.35f; // round 5: a pickup's glow share with look30 (f6's health pickup bloomed to a white human-shaped figure; at this share it reads green)

        // --- THE ARCLINE's grapple nodes as rings in a campaign level (GrappleNodes30.cs; the lead's r5 frame s2) ---------------------------------------
        public const float Grapple30RingRadius = 0.6f; // m: the ring's outer radius (about 1.2 m across: readable as "a ring" at 10-20 m)
        public const float Grapple30RingBand = 0.1f; // m: the ring's band
        public const float Grapple30GlowGrow = 0.22f; // m: the soft glow ring reaches this far outside the ring
        public const float Grapple30GlowShare = 0.25f; // the glow ring's brightness share of the ring's
        public const int Grapple30RingSteps = 40; // the ring mesh's steps round a turn
        public const float Grapple30FarGain = 0.45f, Grapple30ReachGain = 1.0f, Grapple30AimedGain = 1.8f; // out of the ARCLINE's reach / in reach / the one the crosshair would hook
        public const float Grapple30PulseHz = 0.8f; // a slow +-15 % breath (never a flash)

        // --- the floor panels that light nothing (GG frame 170047) -------------------------------------------------------------------------------
        public const float Look30FloorStripScale = 0.3f; // a prop floorStrip's brightness share: a trim, no longer the brightest thing in the room (pool quads are not drawn at all)
    }
}
