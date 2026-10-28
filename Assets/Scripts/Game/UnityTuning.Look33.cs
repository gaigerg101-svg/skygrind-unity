// NIGHT33 L (design.n33.look33): the view numbers of "every light has a fixture" (Research/2026-10-11-wave33/L1-lighting-vfx.md section 5).
// Unity-free on purpose: LightPlan33 (the placement rules) and the harness's light33 read these too. View only; nothing here touches the sim.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const double Look33LightOff = 0.4; // m: a fixture's light stands this far out of its surface along the socket normal (L1: 0.3-0.5)
        public const double Look33MaxLightToFixture = 0.6; // m: the light33 check: a light with no fixture box this close is a floating light (L1 rule 1)
        public const double Look33MountGap = 0.02; // m: an emissive bar is mounted when every 0.5 m slice of it is this close to level geometry (L1 placement rules)
        public const double Look33MountSample = 0.5; // m: the slice step of that check
        public const double Look33SnapReach = 0.12; // m: a plan socket this close to a wall / ceiling face is snapped flush onto it (NIGHT32's sockets hang 0.05 off)
        public const double Look33PitMargin = 0.3; // m: a floor trim stops this short of a pit or lava edge (it runs on floor, never over the pit)
        public const double Look33TrimMinLen = 0.6; // m: a trim piece shorter than this after the pit cut is dropped
        public const double Look33PanelSize = 1.2; // m: the flush ceiling panel at a room's middle (the room's key fixture; L1: up to 2 x 2)
        public const double Look33PanelDepth = 0.05; // m: the panel's thickness under the ceiling (flush: L1 "within 0.05 m")
        public const double Look33FrameWidth = 0.08; // m: the dark frame around a ceiling panel (it reads as a fitting, not a hole of light)
        public const double Look33FrameDepth = 0.07; // m: the frame hangs this far under the ceiling (a lip just deeper than the panel)
        public const double Look33PoolPanelSize = 0.9; // m: the downlight panel a floor pool socket gets over it on the room's ceiling
        public const double Look33GuideLampW = 0.9, Look33GuideLampH = 0.12, Look33GuideLampD = 0.25; // m: the guide lamp under the exit door's lintel (w across the door, h, d)
        public const double Look33LavaLightUp = 1.2; // m: a lava light stands this high over the lava surface (L1 lava row)
        public const double Look33LavaLightRange = 6, Look33LavaLightIntensity = 1.0; // the lava light's range (m) and intensity (L1: range 8, 1.0-1.6; the lead's n33-r5: orange 56-61 % of saturated pixels, so 6 m and L1's floor: the glow stays on the lava's lip)
        public const float Look33LavaGainScale = 0.6f; // the lava surface's and rim's emission x this with look33: 1.6 -> 0.96 and 1.3 -> 0.78, under the bloom threshold 1.0 (no orange halo over the dark walls)
        public const double Look33LavaLightMaxUp = 1.25; // m: the light33 check: a lava light is mounted on its lava when it is at most this high over it
        public const double Look33StripLift = 0.012; // m: a roof strip sits this far under its room's own roof (flush; under the 0.02 mount gap)
        public const double Look33CeilLift = 0.005; // m: the drawn ceiling sits this far under the roof slab (no z-fight with the map's roof box)
        public const float Look33FrameShade = 0.55f; // the ceiling panels' frames: colors.pillarSide x this (a darker fitting around the light, on the stone material)
        public const uint Look33PanelWarmHex = 0xffe6c8; // the ceiling panels and the room key: warm white (tungsten-ish; the lead 11:57: cyan is player-usable only, never a ceiling light)
        public const float Look33PanelHdr = 1.2f; // the flush ceiling panels' emissive HDR (L1 ceiling panel row: at most 1.2; big and soft, so dim)
        public const float Look33LampHdr = 2.5f; // the guide lamps' emissive HDR (L1 wall sconce row: 2.5)
        // --- round 2: L1's six fixture types (section 5 "Fixtures" table; m, HDR, light range m / intensity before UnityTuning.LightScale) ---
        public const double Look33HdrMax = 4; // L1 check 3: no fixture emissive over HDR 4 (lava / area surfaces: 2.2, LavaView30's Lava30Gain 1.6 / rim 1.3)
        public const int Look33KitSlots = 3; // LightKit29's real lights for the room's fixtures with look33 (plus one kept for the room's lava): with the room light and the guide, 5-6 per room (L1: 3-6)
        public const double Look33SconceMaxW = 1.0; // m: a wall socket wider than this is a recessed strip, else a sconce
        public const double Look33SconceW = 0.25, Look33SconceH = 0.4, Look33SconceD = 0.15; // the sconce body (L1: 0.25 x 0.4 x 0.15, bolted flush)
        public const double Look33LensD = 0.02; // m: a sconce lens stands this proud of its body's front
        public const double Look33SconceHdr = 2.5, Look33SconceRange = 7, Look33SconceIntensity = 1.75; // L1 wall sconce: HDR 2.5, point range 7, intensity 1.5-2
        public const double Look33StripSeg = 3, Look33StripGap = 0.1; // m: a recessed strip comes in segments of at most 3 m with a 0.1 m joint (L1)
        public const double Look33StripInsetH = 0.1, Look33StripInsetD = 0.03; // m: the strip's emissive inset (L1: 0.1 wide) and how proud it is (behind the trims)
        public const double Look33StripTrimH = 0.06, Look33StripTrimD = 0.07; // m: the dark trims above and below it (they stand prouder: the inset reads recessed)
        public const double Look33StripHdr = 1.9, Look33StripRange = 6, Look33StripIntensity = 1.2; // L1 recessed strip: HDR 1.5-2.2, a point light range 6
        public const double Look33PanelMax = 2.0; // m: a ceiling panel is at most 2 x 2 (L1)
        public const double Look33PanelRange = 8, Look33PanelIntensity = 1.2; // L1 ceiling panel light (L1 says a spot down, angle 90: the kit's pooled lights are points; not checked as spots)
        public const double Look33BrazierW = 0.5, Look33BrazierH = 0.55, Look33BrazierD = 0.3, Look33PlateD = 0.04; // m: the wall cage (back plate, tray, side bars)
        public const double Look33BrazierY = 1.3; // m: a brazier made from a danger pool hangs on the room wall at this height (its plate centre: above the floor, under head height)
        public const double Look33BrazierHdr = 3, Look33BrazierRange = 6, Look33BrazierIntensity = 1.6; // L1 brazier / danger lamp: HDR 3, orange, range 6 (flicker +-15 %: round 3)
        public const double Look33PoleMinRoom = 12; // m: a room gets a pole lamp when both sides are at least this long
        public const double Look33PoleInset = 0.6; // m: the pole stands this far in from the room's corner walls
        public const double Look33PoleH = 2.6, Look33PoleW = 0.12, Look33PoleBaseW = 0.4, Look33PoleHead = 0.4; // m: L1 pole lamp (0.12 pole, 0.4 head, base plate)
        public const double Look33PoleHdr = 3.5, Look33PoleRange = 9, Look33PoleIntensity = 2.5; // L1 pole lamp: HDR 3-4, range 9, intensity 2.5
        public const double Look33LavaArea = 100; // m2: one lava light per this much lava (L1: one per 10 x 10 m)
        // --- round 2: the post profile and the per-theme look (L1 section 5 "Post profile per theme"; order = LightPlan33.Themes: foundry, iceline, kiln, span, spire) ---
        public const float Look33BloomThreshold = 1.0f, Look33BloomScatter = 0.6f, Look33BloomClamp = 10f; // L1: threshold 1.0 in HDR (only emissives bloom), scatter 0.6, clamp 10
        public const float Look33BloomDefault = 0.5f; // the bloom intensity of a campaign level with no theme match (L1: 0.4-0.6); tonemapping is Neutral (ACES pushes cyan to white)
        public static readonly double[] Look33ThemeBloom = { 0.5, 0.4, 0.45, 0.5, 0.6 }; // L1 bloom intensity per theme (x the rank energy lift as before)
        public static readonly double[] Look33ThemeExposure = { 0, 0.2, -0.1, 0, -0.2 }; // L1 post-exposure per theme, EV added to UnityTuning.Exposure
        public static readonly double[] Look33ThemeSaturation = { 5, -10, 5, 0, 10 }; // L1 saturation per theme, URP ColorAdjustments units added to the grade's
        // round 3 (the lead's n33-r3 frames: mean 0.34-0.36, dark 2-3 %, "flat washed-out lavender"): L1's densities greyed the far wall; a third of them, in darker colours
        public static readonly double[] Look33ThemeFogDensity = { 0.004, 0.006, 0.005, 0.002, 0.007 }; // exponential fog density per theme (RenderSettings: the Lit / Unlit fog; the stone keeps its sky fog)
        public static readonly uint[] Look33ThemeFogHex = { 0x1a1624, 0x2a2a40, 0x1c1014, 0x262038, 0x120e1c }; // L1's hues (grey-violet, pale violet, rust, light violet, violet) at dusk darkness: fog darkens, never lifts (never orange: lava rule)
        public static readonly uint[] Look33ThemeAmbientSky = { 0x14101e, 0x161a2a, 0x170c14, 0x1a1630, 0x0e0a16 }; // round 3: ambient (trilight top) ~0.06-0.11 (the lead: 0.06-0.10; r3 printed 0.22-0.33): the fixtures make the pools, the ambient only keeps the dark readable
        public static readonly uint[] Look33ThemeAmbientEquator = { 0x0d0a14, 0x0f1220, 0x10080e, 0x110e20, 0x09070f }; // the trilight's middle: a step darker
        public static readonly uint[] Look33ThemeAmbientGround = { 0x07060b, 0x080a12, 0x080406, 0x090712, 0x050408 }; // the trilight's bottom: darker still (floors read by the fixtures)
        public const float Look33RimScale = 0.3f; // round 3: the stone's unshadowed rim light (_RimColor) x this with look33: it lit every wall inside the closed rooms evenly (no pools)
        public const float Look33StoneFogDim = 0.4f; // round 3: the stone's sky-coloured fog (_SkyFog* colours) x this with look33: far walls fade to dusk dark, not lavender
        public const float Look33ExposureBase = 0f; // round 3: look33's post-exposure before the theme's EV (NIGHT32's UnityTuning.Exposure 0.35 lifted every frame; the lead: 0 or negative)
        // --- round 2, the lead's n33-r1 frames (dark % 10.5-12.7 under the 15 floor, f2-a mean 0.289): pools of light, not a brighter room ---
        public const float Look33KeySpotAngle = 110f; // deg: the room key is a spot straight down from its ceiling panel (the ceiling round it stays dark)
        public const float Look33KeyRangeExtra = 3f; // m: the key's range = its height + this (it reaches the floor, not the far walls)
        public const float Look33KeyScale = 0.7f; // the key's intensity x NIGHT24's roomLight.intensity (it no longer floods 38 m)
        public const float Look33GuideRange = 8f; // m: the guide lamp's light range (NIGHT24's 16 m lit half the room from the exit)
        public const float Look33PanelSpotAngle = 90f; // deg: a ceiling panel's light (L1: spot down, angle 90)
        public const double Look33BeamTopGap = 0.08; // m: a light shaft is mounted when its top end is this close to a fixture box (the light that casts it)
    }
}
