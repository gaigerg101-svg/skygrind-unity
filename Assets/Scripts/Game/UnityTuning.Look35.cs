// NIGHT35 V (design.n35.look35 / lights35 / tv35 / ridelook35): every render and UI number of tonight's look, one comment each (CLAUDE.md rule 5).
// Starting numbers from Research/2026-10-11-wave35/RESEARCH.md (ULTRAKILL's PSX options, read with REA + ILSpy: GraphicsSettings.cs:199-212,
// PrefsManager.cs:132) and research/look.md; deltas after the lead's Player frames are logged in the commit messages. View only: no sim path.
// Unity-free (plain consts) so LightPlan33's harness copy can read the lights35 numbers too.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- look35: the PS2 pass (rides Crt29Pass; Crt29.shader _Crt4) ---------------------------------------------------------------------------
        public const float Look35Lines = 480f; // virtual lines drawn, point upscaled (ULTRAKILL's 480 downscale preset, GraphicsSettings.cs:199-212; D-4: 480)
        public const float Look35Levels = 32f; // colour levels per channel (ULTRAKILL's default colour compression, PrefsManager.cs:132)
        public const float Look35QuantStrength = 1f; // the quantise at full strength (THE LOOK's 0.35 blend hid it: "it really shows" needs the real steps)
        public const float Look35DitherCell = 1f; // the Bayer cell in VIRTUAL px (one 4x4 matrix per 4x4 virtual pixels: the PS2 ordered dither)
        public const float Look35ScanCount = 240f; // scanline cycles per screen height: one dark line every 2 virtual rows (4.5 px at 1080p: reads, no moire)
        public const float Look35ScanIntensity = 0.3f; // the dark half's depth at the default CRT setting (NIGHT29's 0.12 x 0.35 = 0.04 was invisible)
        public const float Look35ChromaUv = 0.006f; // the corner-weighted red / blue split at the default CRT setting (NIGHT29: 0.0015 x 0.35)
        public const float Look35FlatChromaPx = 1f; // a red / blue split across the WHOLE screen, in virtual px (one PS2 pixel: the cheap-TV fringe on every edge)
        public const float Look35Grain = 0.012f; // the grain at the default CRT setting, per virtual pixel. r2 delta 0.045 -> 0.012 (the lead's n35-r1: "TV static", the loudest thing on every flat wall): now about a third of one colour level (1/31), so the ordered Bayer dither and the banding read, not noise
        public const float Look35GainMax = 1.5f; // the CRT setting / its default, capped: a player turning CRT up gets at most 1.5 x these; 0 turns them off
        public const float Look35MinBlockPx = 1.5f; // a screen shorter than Look35Lines x this draws at height / this lines (a small window keeps a visible block)

        // --- lights35: fixtures only where a surface holds them (LightPlan33.Green with lights35; the check LightPlan33.SkyFixtures) ------------------
        public const double Look35SkyReach = 0.3; // m: a fixture is held when a solid box comes this close to it (beside, over or under) ...
        public const double Look35SkyDrop = 0.05; // m: ... and reaches up to at least this under the fixture's bottom (a wall top far below a hung panel holds nothing)
        public static readonly double[] Look35KeyYs = { 5.5, 4.0, 2.8 }; // m: an open room's key flood lamp goes at the first of these heights its nearest wall covers (high enough to light the floor, under the wall's top)
        public static readonly double[] Look35SconceYs = { 3.0, 2.2 }; // m: an open room's pool / ceiling socket becomes a sconce at the first of these heights a wall covers
        public const double Look35WallTopGap = 0.3; // m: the wall must reach at least this above the fixture's top (never a lamp perched on a wall's edge against the sky)
        public const double Look35FloodW = 0.7, Look35FloodH = 0.35, Look35FloodD = 0.22; // m: the flood lamp's dark housing (wide and shallow: reads as a work light bolted to the wall)
        public const double Look35FloodHdr = 3.2; // the flood's warm lens HDR (over the bloom threshold: the room's one brightest fixture; L1 cap 4)
        public const double Look35JoistGap = 4.0, Look35JoistW = 0.35, Look35JoistD = 0.45; // m: dark joists across a roofed room's ceiling (spacing, width, depth): the flat violet ceiling reads as a roof, not sky
        public const double Look35JoistEnd = 1.0; // m: no joist closer than this to the room's end walls
        public const uint Look35CeilingHex = 0x4a403a; // RoofView27's ceiling with lights35: a dark warm grey (roof27's 0x... violet read as the sky); the joists (stone x 0.55, lit by ambient only) read darker against it
        public const float Look35MoteTop = 3.5f; // m: Vfx33's dust motes stay under this (look33 filled the room to its ceiling: dots against the violet ceiling read as stars)
        public const float Look35KeySpotAngle = 72f; // deg: r2 (n35-r1 "lit": luminance 0.188 vs 0.193, no pool): the key's cone narrows from look33's 110 so its pool has an edge on the floor
        public const float Look35KeyInner = 0.35f; // the inner cone's share of it (a soft but visible rim)
        public const float Look35KeyRangeExtra = 6f; // m: its range = its height + this (it reaches the floor at full strength)
        public const uint Look35FillHex = 0x6474ff; // the cool fill: blue-violet (not cyan: player-usable; not magenta: RIVAL)
        public const float Look35FillIntensity = 14f; // its intensity before LightScale (a fill: under the key). r4 delta 5 -> 14 (1 / d^2: 5 / pi gave ~0.06 at 5 m)
        public const float Look35KeyFloor = 0.55f; // r4: the key's intensity = this x its height over the floor squared (URP's 1 / d^2): ~0.55 on the floor under it (the lit floor reads warm-bright under the PS2 pass); replaces Look35KeyScale's x2.4 of NIGHT24 (~0.06 on a 9 m room's floor)
        public const float Look35FillY = 2.5f; // m: its height over the room's spawn floor (it washes the far wall, low)
        public const double Look35FillInset = 1.5; // m: in from the room's far wall
        public const float Look35FillRangeShare = 0.6f; // its range = the room's longer side x this
        public const float Look35BloomThreshold = 1.5f; // bloom only over this (look33: 1.0): the fixtures (HDR 1.9-4) glow, panels (1.2) and lit surfaces do not

        // --- tv35: the TV HUD (UiTv35; canvas units = px at 1080) -----------------------------------------------------------------------------------
        public const float Tv35SpringHz = 7f; // the HUD spring's frequency (Hz): a quick wobble, settled in ~0.3 s
        public const float Tv35SpringDamping = 0.32f; // its damping ratio (under 1: it overshoots twice, the "bouncier" GG asked for)
        public const float Tv35HitKickPx = 9f; // a hit taken: the HUD's first swing (px, a fixed-hash direction)
        public const float Tv35HitKickScale = 0.035f; // a hit taken: the HUD's scale punch (3.5 %)
        public const float Tv35SwapKickPx = 4f; // a weapon swap: a lighter swing (px)
        public const float Tv35SwapKickScale = 0.015f; // a weapon swap: scale punch
        public const float Tv35MaxPx = 14f; // the swing never goes past this (px), however many kicks stack
        public const float Tv35MaxScale = 0.06f; // nor the scale past +-6 %
        public const double Tv35GlitchS = 0.2; // s: a glitch burst's length (bars, sweep, tear)
        public const double Tv35GlitchGap = 0.4; // s: at most one burst per this (look30's flicker rule: <= 3 a second)
        public const uint Tv35BlueHex = 0x2a5bff; // the split's second channel on a bend / level start (deep blue beside the cyan)
        public const float Tv35TearPx = 10f; // px: the HUD's sideways tear on a burst's frames (fades over the burst)
        public const int Tv35Bars = 8; // noise bars available per burst frame
        public const float Tv35BarOdds = 0.45f; // each bar's chance to show on a frame (a fixed hash): ~3-4 bars, under 6 % of the screen
        public const float Tv35BarMinPx = 3f, Tv35BarMaxPx = 14f; // a bar's height range (px)
        public const float Tv35SplitPx = 6f; // px: the two channels' bars sit this far left / right (the split signal)
        public const float Tv35BarAlpha = 0.35f; // a bar's alpha (the HUD stays readable through it)
        public const float Tv35SweepPx = 26f; // px: the bright band's height as it sweeps down the screen in one burst
        public const float Tv35SweepAlpha = 0.22f; // its alpha
        public const float Tv35RescanS = 0.5f; // s: the HUD's texts are re-listed this often (new labels join the bounce)
        public const float Tv35BounceS = 0.32f; // s: a new text's pop and wobble
        public const float Tv35BouncePop = 0.15f; // its peak growth (15 %; text only grows, so nothing drops under 16 px)
        public const float Tv35BounceGap = 0.25f; // s: one text bounces at most once per this

        // --- ridelook35: the wall-ride + zoom FOCUS look (SlowAim27.Focus; NIGHT27's cue was vignette 0.25 / chroma 0.15: invisible) ---------------
        public const double Ride35InS = 0.1; // s (real): the look comes in this fast once riding and zoomed (one frame reads it after 0.1 s)
        public const double Ride35OutS = 0.18; // s (real): and leaves over this (a little slower: the cut back to normal never snaps)
        public const uint Ride35FilterHex = 0x8fb4ff; // the colour filter at full focus: a cold blue (blue = focus; the bend's grade is grey)
        public const float Ride35Saturation = -30f; // saturation added at full focus (the warm dusk drains; the blue reads)
        public const float Ride35Contrast = 18f; // contrast added at full focus (a harder, colder image)
        public const float Ride35Temperature = -45f; // white balance at full focus (cooler; URP's -100..100)
        public const float Ride35Chroma = 0.7f; // chromatic aberration added at full focus (URP's is edge-weighted: the "edge chroma"; 0..1)
        public const float Ride35Vignette = 0.22f; // vignette added at full focus, in Ride35VignetteHex (a blue frame, not a dark one)
        public const uint Ride35VignetteHex = 0x0b2f7a; // the focus vignette's colour: deep blue
        public const float Ride35VignetteSmooth = 0.6f; // its softness (URP 0.01..1)
        public const float Ride35Tear = 0.35f; // the CRT pass's band tear at full focus (x 0.03 uv: ~20 px at 1920 on about one band in six: faint)
        public const double Ride35TearHz = 12; // Hz: the torn bands change this often (a fixed hash of the step: no Random)
        public const float Ride35Sweep = 0.22f; // the bright scanline band's gain at full focus
        public const float Ride35SweepHalf = 0.035f; // its half height (uv: ~38 px at 1080p)
        public const double Ride35SweepPeriod = 1.1; // s (real): the band falls from top to bottom once in this
    }
}
