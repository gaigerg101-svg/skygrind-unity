// NIGHT74 builder L (LIGHT and SIGHT): Unity-only numbers for L's slices (CLAUDE.md rule 5: each with a comment; the web never had any of them, and the sim
// never reads them: golden vectors, replays and fingerprints stay as they were). L1's reel numbers first; L2 / L3 / L4 add theirs below.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- L1: the moving-camera flicker meter's reel (FeelReel.N74.L.cs; Pan74.cs; Tools/night/n74-flick.py reads what it writes) ----
        public const int N74PanFrames = 180; // rendered frames kept in one sweep (3 s at ReelFps 60, +-17.9 deg about the pose's yaw; a frame costs about 90 ms to read back (still72) plus the luma, the block sums and the warp's residual: 150-200 ms, so 27-36 s a room)
        public const double N74PanRateDeg = 0.2; // degrees of yaw per rendered frame (12 deg/s: slow on purpose, the fallback test is weak on fast pans: at 0.2 deg a frame the picture moves 2.5 px a frame in the middle and about 7 px at the edges, under the 8 px block)
        public const int N74PanSettleFrames = 30; // frames at the sweep's first yaw before the first is kept (0.5 s: the camera cut, the bloom and the room's lights settle; still72's number)
        public const int N74PanMaxRooms = 5; // toured rooms swept per level (in level order): 5 x (36 s + the room's wave wait of up to 10 s) is under the 420 s a side is given
        public const double N74PanBudgetS = 370; // wall seconds from the phase's start after which no new room is begun that the last room's wall time says would not finish (reel.sh closes a Player at 420 s: a cut Player prints no `done:` line; the skipped rooms are named in it instead)
        public const int N74PanBlock = 8; // pixels per block side: the scout's 8 x 8 block means (1920 x 1080 is 240 x 135 blocks)
        public const double N74PanCoreX0 = 0.125, N74PanCoreY0 = 0.148, N74PanCoreX1 = 0.79, N74PanCoreY1 = 0.593; // the WORLD core the calibration reads, as shares of the picture (x 240-1520, y 160-640 of 1920 x 1080: the HUD counter's, the weapon card's and the gun's rectangles are outside it; n74-warp-quality.py's own "world core")
        public static readonly double[] N74PanFovTry = { 0.90, 0.92, 0.94, 0.96, 0.98, 1.0, 1.02, 1.04, 1.06, 1.08, 1.10 }; // NIGHT74 sharpened (was 1.0 / 0.92 / 1.08): the vertical field of view the warp tries as multiples of the camera's own, in steps of 2 %; the n74-t2 sweeps' real shift (2.58 px a frame) is x0.984; the camera's fieldOfView is what the picture used, so 1.0 should be near the winner // the vertical field of view the motion-compensation warp tries, as multiples of the camera's own (a safety net: the camera's fieldOfView is what the picture used, so 1.0 should win; the sweep's own calibration line prints the winner)
        public const double N74PanCalibGood = 0.6; // the chosen warp counts as compensating when its mean |residual| is under this share of the plain frame difference (the line says WEAK otherwise)
        // ---- L2: reveal74, the edge chevron toward the nearest live enemy when none is on screen (Reveal74.cs, Reveal74Math.cs; FeelReel.N74.L.cs's reveal74 phase) ----
        public const double N74RevealDelayS = 3.0; // s with N LEFT showing and no live enemy on screen before the chevron comes: the slate's L2 ("for 3 s"; the web had no such cue)
        public const float N74RevealFadeInS = 0.25f; // s for the chevron to come up (a calm entrance: no pop)
        public const float N74RevealFadeOutS = 0.10f; // s for it to go once an enemy is on screen: the slate's "hidden within 0.2 s of an enemy entering the screen" with a whole frame to spare
        public const float N74RevealChevronPx = 96f; // px at 1080p: the double chevron's box (UI text is 16 px and up; this is a shape, big enough to read at the screen's edge)
        public const float N74RevealEdgeInsetPx = 84f; // px from the screen's edge to the chevron's centre
        public const float N74RevealLowestPx = 200f; // px above the bottom inset the chevron may sit at the lowest: the gun, the weapon bar and the HP plate own the bottom (Goal72's rule)
        public const float N74RevealPulseHz = 1.1f; // a slow breath (a scale, never a flicker; none with REDUCED MOTION)
        public const float N74RevealPulseDepth = 0.10f; // the breath's depth (1 .. 1.1 x)
        public const float N74RevealFollow = 14f; // 1/s: how fast the chevron glides when the nearest enemy changes or the player turns (its angle is smoothed so the edge point never jumps)
        public const int N74RevealSortingOrder = 9; // under the HUD's canvas (10), the same layer as goal72's
        public const double N74RevealHoldS = 4.5; // s the reel holds the camera facing away from every enemy (the delay 3 s, the fade-in 0.25 s, and the shot at 4 s with room to spare)
        public const double N74RevealShotAtS = 4.0; // s into the hold the facing-away frame is taken: the slate's "at 4 s facing away"
        public const int N74RevealTurnFrames = 90; // frames the reel waits for the chevron to go after it turns to face the nearest enemy (1.5 s; it should take 6)
        // ---- L3: crown74, THE CROWN's ceiling lifted with a cool fill (Crown74.cs; Assets/Shaders/Resources/Ceiling27.shader's _Crown74 block; Tools/night/n74-crown.py measures it) ----
        public const float N74CrownFillR = 0.30f; // the fill in display (sRGB) values 0-1, added to the darkest ceiling and stone pixels: (77, 79, 128) of 255, a cool slate-violet. RETAKE (n74-t2): the first fill, 0.15 / 0.156 / 0.25 on the ceiling alone, lifted the region 900-1300 x 150-450 by +9.3 / +8.0 / +2.2 / +2.5 / +5.7 (flinch, idle, stagger, tell, windup: the bar is +25), about a third of the model; now doubled, on the arena's stone too. Red never over green and blue leading: no danger hue (CLAUDE.md rule 8); not the player's cyan
        public const float N74CrownFillG = 0.31f; // (the dark ceiling pixel (29, 41, 28) of the n73 freeze frames rises to about (67, 81, 92) before lit72's own lift. Sized on those frames by Research/2026-10-27-wave74/L/crown74-model.txt: lit72's lift inverted per pixel,
        public const float N74CrownFillB = 0.50f; // the fill added, lit72 applied again: the region 900-1300 x 150-450 up +29.7 on the worst of the five frames at this size (+25.5 at 0.12 / 0.125 / 0.20, the bar's edge); a MODEL, not a Player frame: the Player gave about a third of it (n74-t2), hence the retake on FillR; these two lines' numbers are the FIRST fill's)
        public const float N74CrownL0 = 0.28f; // display luma (0-1; 71 of 255; was 0.20: the n74-t2 stagger / tell ceiling and walls sit at 70-90) under which a ceiling or stone pixel takes the whole fill (the "dark < 30" line is 0.118; the fogged centre of the ceiling measures 0.02 - 0.15)
        public const float N74CrownL1 = 0.70f; // display luma (178 of 255; was 0.60) from which a pixel takes none: a lit coffer beam keeps its value and the grid keeps its shape
        public const float N74CrownFadeInS = 0.25f; // s for the lift to come up when THE CROWN's fight starts (was 0.8 s, lit72's own: the earliest boss63 frames were taken inside it)
        public const float N74CrownFadeOutS = 0.8f; // s for it to go when the fight ends
        // ---- leftover 4: ice74, THE ICEBREAKER's arena lifted by lit72's own block with the arena's numbers (Ice74.cs; Crt29Pass.cs hands the vector to Crt29.shader's Lit72 block; Tools/night/n74-ice-model.py sized it, Tools/night/n74-ice.py measures it) ----
        // (the first try, an ambient probe fill with N74IceFillR / G / B 0.045 / 0.060 / 0.100, did nothing in the n74-t4 Player: the arena's URP Lit parts do not take a runtime probe write; it is gone)
        public const float N74IceA = 0.14f; // luma (0-1) the arena's lift adds at black (lit72's L72LitA is 0.12): a wall pixel at display luma 0.010 takes (0.010 + 0.14) / 0.010 = 15 times its value, under the cap below
        public const float N74IceL0 = 0.12f; // display luma under which the lift is whole, used only when lit72 is not live (lit72's L72LitL0)
        public const float N74IceL1 = 0.42f; // display luma from which a pixel is left alone (lit72's L72LitL1: the lit floor, the lights and the boss keep their value)
        public const float N74IceCap = 20f; // the most a pixel is scaled by (lit72's L72LitKMax is 9: THE ICEBREAKER's walls are 2, 2, 6 of 255 before the lift, and x9 leaves them at 23; at 20 they reach about 38 with A 0.14: the model, Tools/night/n74-ice-model.py)
        public const float N74IceFadeInS = 0.8f; // s for the lift to come up when the player is in the arena (lit72's own fade: no pop)
        public const float N74IceFadeOutS = 0.8f; // s for it to go on leaving
        public const double N74IceMarginM = 0.5; // m beyond the arena's rim walls that still count as inside (the entry gap's threshold)
        public const int N74PlantX = 160; // px from the left edge: the planted control's corner (a multiple of the block size; over the left wall, clear of the HUD's rail)
        public const int N74PlantY = 544; // px from the top edge: the planted control's corner (a multiple of the block size; mid-height)
        public const int N74PlantSize = 96; // px: the planted control's side (12 x 12 whole blocks: every block it covers sees the whole swap)
        public const float N74PlantMid = 0.5f; // display luma (0-1) the planted block alternates around
        public const float N74PlantAmp = 0.03f; // the planted alternation's half-height: luma 0.5 +- 0.03 every frame, the scout's "+-3 % alternating luma" (7.65 of 255 levels)
        // ---- pads74: the jump pad's particle stream steadied (Pad74.cs; JumpPadView27.cs builds the steady twin and picks one by the row every frame; Pan74Pads.cs writes the pad regions the meter reads) ----
        // The shipped stream (UnityTuning.Pad27Stream*) is hard-edged squares of the HDR cyan neon material: 0.13 m wide at their best, 0.026 m at birth and shrinking to nothing at death, so at 15-30 m they are
        // 1-4 px: a quad that small gains and loses pixel centres as the camera turns, and the meter (n74-t1 / t2 heat maps) flags the pad column among the hottest moving-camera blocks. The steady twin has bigger,
        // soft, slower and fewer particles that never go below a floor size and fade through a sprite sheet instead of shrinking away; the shipped stream is untouched (the row off draws exactly it).
        public const float N74PadStreamRate = 5f; // particles a second (the shipped Pad27StreamRate is 12): the stream's glitter is the count of tiny independent flickerers
        public const float N74PadStreamLife = 2.0f; // s (the shipped 1.5)
        public const float N74PadStreamSpeed = 1.5f; // m/s (the shipped 2.0): speed x life is 3.0 m of rise as before (2.0 x 1.5), the speed is a quarter lower so a particle moves fewer pixels a frame
        public const float N74PadStreamSize = 0.34f; // m: the soft sprite's quad at its biggest (the shipped square is 0.13); the sprite falls to nothing at the quad's edge, so the bright core is about half this
        public const int N74PadStreamMax = 24; // cap (the shipped 48): 5 a second x 2 s is 10 alive, the launch burst's 10 on top is 20
        public const int N74PadBurstParticles = 10; // the launch burst's motes (the shipped Pad27BurstParticles is 24)
        public const float N74PadStreamTint = 0.7f; // x the glow material's colour (usable cyan x world.neonBase): one soft sprite holds about 1.8 times the light of a shipped square at its peak, and there are 5 not 12 a second
        public const float N74PadSizeFloor = 0.6f; // x N74PadStreamSize at birth and death (the size curve's ends): a particle is never smaller than 0.2 m (3.7 px at 25 m); its light fades through the sprite sheet
        public const int N74PadSpriteTexels = 32; // px: one sprite frame's side (a power of two: its mip 5 is exactly one frame)
        public const int N74PadSpriteFrames = 32; // frames in the sheet (a strip, 1024 x 32): a particle's brightness over its life in 32 steps of at most 16 % of the peak (16 frames with a 0.15 fade-in jumped 57 % in one step: the pure test caught it)
        public const float N74PadFadeIn = 0.30f, N74PadFadeOut = 0.45f; // share of a particle's life it takes to come up from nothing, and to go back to nothing (smoothstep both: the light is whole between them, from 0.30 to 0.55 of its life)
        public const uint N74PadSeed = 7401; // the steady stream's random seed (+ the pad's index, x 7919): the same motes in the same order every run, so two reel runs of one build draw the same stream
        // pads74's second part: the pad beam's BRIGHTNESS (GG, gg-proxy-start #5: the cyan light columns are "blinding"; the hue and the beam stay). The n74-t1 foundry-b1 mid shot, 5 m from a pad: the beam's lower third reads RGB
        // (8, 245, 245) with half its pixels at 250 or more, the middle (1, 222, 228), the top (2, 185, 192), the wall behind 45-56 luma. Through the Neutral tonemap (PostFx.cs: look33 is always Neutral) those read about 1.16, 0.73 and
        // 0.49 of raw light; half of it puts them at 0.58 / 0.37 / 0.25 (sRGB 196 / 160 / 133: no clip, a column 2-3 times the wall's luma). A column also fills the screen when the player walks up to a pad or launches from it (the burst
        // glow takes the beam to 1.48 x its idle number): the fade below takes it down further inside a few metres.
        public const float N74PadBeamScale = 0.5f; // x the shipped beam's HDR (Pad27BeamHdr x (0.6 + 0.4 glow)) at any distance
        public const float N74PadBeamNearM = 1.0f, N74PadBeamFarM = 4.0f; // m: the camera's horizontal distance to the pad's axis under which the beam is at its floor, and from which it is whole (smoothstep between)
        public const float N74PadBeamNearFloor = 0.4f; // share of the softened beam that is left at or under the near distance (0.5 x 0.4 = 0.2 of the shipped light: still there, a faint column, never 0)
        // the pad regions the pan74 reel writes beside each sweep (Pan74Pads.cs; n74-flick.py pads reads them)
        public const double N74PadRegionHalfR = 1.7; // x the pad's radius: the pad volume's half width (the halo reaches 1.7 R, the expanding ring 1.9 R at its last, faintest moment)
        public const double N74PadRegionHeightM = 4.0; // m: the pad volume's height (the stream rises 3 m from 0.19 m; the faint beam above is left out)
        public const double N74PadStreamRegionHalfR = 0.8; // x the pad's radius: the stream column's half width (the cone's radius is 0.7 R; Pad27's MakeStream)
        public const double N74PadStreamRegionTopM = 3.4; // m: the stream column's top (0.19 + speed x life of the shipped stream is 3.2, a little over)
        public const double N74PadSightLiftM = 1.5; // m above the pad's floor point: where the line of sight to the pad is tested against the level's boxes (the stream's middle)
        public const double N74PadSightEps = 0.05; // m a box is shrunk by in the line-of-sight test (a pad against a wall is not hidden by it)
    }
}
