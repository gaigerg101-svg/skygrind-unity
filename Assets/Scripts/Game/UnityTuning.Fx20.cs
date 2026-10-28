// NIGHT20 U3-34.3 / U3-34.10 (builder V): the effects' render numbers for tonight's things (CLAUDE.md rule 5: Unity-only
// numbers, each with a comment). Why the web has none of them: the web's THUNDERCLAP shock (weapons.specials.shotgun.shock)
// has no air of its own (its cone is a stagger, drawn by nothing but the pellets), the web never zoomed the nailgun (the
// STITCHER's ADS pose is new), and the web's LONGWAVE scope is a flat HUD vignette (hud.js .hud-scope) with no rim on the
// gun. Every number here is visual only: the sim fires the shockwave and decides the zoom.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the THUNDERCLAP's shockwave (MovementFx.Gust.cs's air re-aimed: N20Events.Shockwave / ConeWave) -------------
        public const double ShockStreakMult = 1.6; // x GustStreaks: the shockwave throws more air than NIGHT19's gust (35 streaks)
        public const double ShockStreakSpeedMult = 1.3; // x GustStreakSpeed: the air reaches further (the shock's 8 m cone)
        public const double ShockRingScale = 1.4; // x the gust's two rings' radii and the far ring's travel
        public const double ShockPuffScale = 1.3; // x the floor swirl's radius (a shockwave aimed down: the launch)
        public const double ShockKickMult = 1.35; // x the gust's blowback in the hands (a shotgun's double blast: a bigger shove)
        public const double ShockFovMult = 1.25; // x the gust's FOV kick (GustFovKickDeg 6.5 -> 8.1 deg; the boost's is 11.8)
        public const string ProbeShockwave = "shockwave"; // the reel's event marker on the shockwave's step (the bus's own name)

        // --- the STITCHER's ADS pose (Viewmodel.cs; the web's viewmodel.ads has none: the web never zoomed the nailgun) --
        public const double NailgunAdsX = 0.012; // m right of the eye (web camera space): the feed sits left of the sight line
        public const double NailgunAdsY = -0.125; // m: a touch lower than the default (-0.11): the drum is tall
        public const double NailgunAdsZ = -0.29; // m ahead (the default's -0.3; the KESTREL's -0.28)
        public const double NailgunAdsYawDeg = 0.5; // deg: the barrel toward the crosshair
        public const double NailgunAdsPitchDeg = 0.4; // deg
        public const double NailgunAdsRollDeg = -3; // deg: the slight cant every ADS pose has (default -4)

        // --- the LONGWAVE's scope rims (Viewmodel.Scope20.cs): two lens edges on the overlay camera, the tube's parallax ---
        public const double ScopeRimScreen = 0.31; // the rims' radius as a share of the screen's height (hud.js .hud-scope's clear 31vh)
        public const double ScopeEyeDepth = 0.06; // m in front of the eye: the eyepiece's rim
        public const double ScopeObjectiveDepth = 0.32; // m: the objective's rim (the tube's far end)
        public const double ScopeObjectiveShrink = 0.93; // x the radius: the far rim reads a touch smaller (a tube, not a decal)
        public const double ScopeRimWidth = 0.035; // of the radius: each rim's band
        public const double ScopeParallax = 2.2; // x the gun's sway offset the objective rim moves by (the eyepiece stays on the eye)
        public const double ScopeRimAlpha = 0.45; // the eyepiece rim's peak (additive, cyan-pale)
        public const bool ScopeDrawEyepiece = false; // H's UiZoom20 draws the lens rim at 31vh on the HUD: the gun's eyepiece rim stays off (nothing drawn twice)
        public const double ScopeObjectiveAlpha = 0.3; // the objective rim's peak
        public const double ScopeShowFrom = 0.6; // of the ADS amount: the rims fade in from here to whole (the scope arrives at the eye)
        public const int ScopeRimSegments = 96; // the rings' segments (smooth at 1440p)

        // --- SKYGRIND TIME's look (SurvivalGame.SkygrindTime20.cs: a global Volume over Look's, weighted by the rule's Look) --
        public const float StSaturation = -60; // the world's saturation at full (URP ColorAdjustments, -100..100): the colour drains
        public const float StInkLean = 0.3f; // the colour filter: white toward INK-SOLID (UiTheme) by this: the dusk deepens
        public const float StVolumePriority = 20; // over Look's post volume (priority 0)
        public const string StEvent = "skygrindTime"; // the port-only bus event on its start (payload: "blast" or "headshot") for GG's sounds and H's slab

        // --- A2 THE HUNT's dusk shift (HuntSky20.cs: the sky dome, the stone's sky fog and the URP fog toward DANGER) -------
        public const double HuntSkyIn = 2.5; // s from the hunt's announce to the whole shift (the row: within 3 s)
        public const double HuntSkyOut = 3; // s back to the dusk after its end
        public const float HuntHorizon = 0.55f; // the horizon's share of DANGER at the whole shift (the band the eye reads)
        public const float HuntMid = 0.35f; // the mid sky's
        public const float HuntZenith = 0.18f; // the zenith's (it stays night above)
        public const float HuntFog = 0.4f; // the fog's (the far map sinks into a red haze)
        public const string HuntName = "hunt"; // SpecialRoundEvent.name of THE HUNT (E's director)

        // --- the LANCER's laser and beam (LancerBeam20.cs; the web's lancer.js render :166-178 literals, kept) -------------
        public const double LancerAimRadius0 = 0.012; // m: the laser sight's radius as the aim starts (lancer.js: 0.012 + ...)
        public const double LancerAimRadiusGrow = 0.02; // m more by the end of the aim (... + 0.02 x the aim's elapsed share): it thickens to the shot
        public const float LancerAimGlow = 2.5f; // x colors.enemy (HDR: the bloom carries it; lancer.js multiplyScalar(2.5))
        public const double LancerBeamRadius = 0.08; // m: the shot's beam (lancer.js 0.08 x (1 - age / 0.2))
        public const double LancerBeamLife = 0.2; // s the beam lasts (frame time, as lancer.js beamAge)
        public const float LancerBeamGlow = 4; // x the beam's colour (lancer.js multiplyScalar(4))
        public const uint LancerBeamHex = 0xffe0c0; // the beam's hot colour (lancer.js setHex(0xffe0c0): DANGER gone white-hot)
        public const int LancerBeams = 8; // beams drawn at once (lancer.maxCount is 6 in the web's config)
        public const int LancerBeamSides = 6; // the cylinder's sides (lancer.js CylinderGeometry(1, 1, 1, 6))

        // --- the telegraph language (Telegraph20.cs; src/fx/telegraph.js's TELEGRAPH colours and shape literals, kept) ------
        public const uint TelegraphDanger = 0xff4a1c; // red-orange: avoid, it hits when full
        public const uint TelegraphFlashHex = 0xffffff; // white: it lands next frame
        public const uint TelegraphSafe = 0x3d7bff; // blue: a safe lane
        public const uint TelegraphHazard = 0xcfffd8; // green-white: will turn hot
        public const uint TelegraphHot = 0x7dff4a; // the hazard once it burns
        public const float TelegraphRingInner = 0.93f; // RingGeometry(0.93, 1, 56): the outline's inner radius
        public const int TelegraphSegments = 56; // the circles' segments
        public const float TelegraphBar = 0.04f; // the rectangle outline's bar width (m, of a unit rectangle)
        public const float TelegraphDiscOpacity = 0.55f; // the disc fill's material opacity
        public const float TelegraphRectOpacity = 0.5f; // the rectangle fill's
        public const float TelegraphFlashOpacity = 0.9f; // the flash sphere's
        public const int TelegraphDraws = 192; // draws a frame at most (each its own property block)

        // --- MovementFx textured (MovementFx.Textured20.cs: the Kenney Particle Pack's smoke_04 / dirt_01 / trace_06, CC0) ----
        public const string KenneySmoke = "KenneyParticles/smoke_04"; // Resources path: a soft cloud (the land, slide and climb dust)
        public const string KenneyDirt = "KenneyParticles/dirt_01"; // speckled debris (a climber knocked off its face, a hard landing)
        public const int Puffs20 = 96; // smoke puffs alive at once (the oldest reused)
        public const int Trails20 = 96; // textured streaks alive at once (the wall-ride's and the slide's lines)
        public const double PuffGrowFrom = 0.55; // x its size at birth ...
        public const double PuffGrowTo = 1.45; // ... and at its end (an ease-out: a puff blooms, then hangs)
        public const double PuffFadeIn = 0.12; // of its life: in over this share ...
        public const double PuffFadeOut = 0.45; // ... out from this share to its end
        public const double PuffDrag = 3.5; // 1/s: a puff's drift slows (v e^-kt)
        public const float PuffAlpha = 0.32f; // a dust puff's peak brightness (additive: the dusk's grey haze, never a glow)
        public const int LandPuffs20Min = 2; // puffs on a landing at the puff speed ...
        public const int LandPuffs20Max = 7; // ... and at LandFullFallSpeed or harder
        public const double LandPuffSize20 = 0.9; // m across at the feet (x 1.6 at the hardest landing)
        public const double LandPuffLife20 = 0.55; // s
        public const double SlidePuffRate20 = 14; // puffs a second from the heels at the slide's full speed
        public const double SlidePuffSize20 = 0.55; // m
        public const double SlidePuffLife20 = 0.5; // s
        public const double TrailWidth20 = 0.06; // m: a textured streak's width (the soft trace sprite reads thinner than a quad)
        public const float TrailBrightness20 = 1.25f; // x the line's colour (the sprite's soft edge takes some away)
        // the climb (N's ClimbEvent: start / up / top / knocked / land)
        public const int ClimbStartPuffs = 2; // the tell: grit at the foot of the face
        public const double ClimbUpRate = 9; // puffs a second while a climber goes up (from its hands, falling along the face)
        public const double ClimbUpTime = 1.2; // s of grit after an "up" (the climb's length at its default speed)
        public const int ClimbTopPuffs = 4; // the mantle at the lip
        public const int ClimbKnockedPuffs = 5; // a punch knocked it off: a burst and debris
        public const int ClimbLandPuffs = 3; // it lands at the foot
        public const double ClimbPuffSize = 0.7; // m
        public const double ClimbPuffLife = 0.6; // s
        public const double ClimbDebrisSize = 1.1; // m: the knocked climber's debris sprite
        public const int ClimbStreams20 = 16; // climbers streaming grit at once

        // --- the long slide's wind wisps (MovementFx.Wind20.cs; dip000/my-godotshaders WindTrailWithLoops, MIT: its uniforms) --
        public const double WindAfter = 0.45; // s of sliding before the wisps come (a long slide only; a tap slide shows none)
        public const double WindFade = 0.25; // s in and out (the shader's fade_in_out, shortened: a slide is short)
        public const int WindWisps = 2; // wisps at once (one each side)
        public const int WindPoints = 14; // points along a wisp (13 soft segments)
        public const double WindLength = 2.4; // m a wisp trails behind (the shader's wave_length: a unit mesh x this)
        public const double WindSide = 0.55; // m to each side of the body
        public const double WindHeight = 0.55; // m above the feet
        public const double WindTimeScale = 3.0; // time_scale
        public const double WindWaveAmp = 0.1; // m: wave_amplitude
        public const double WindWaveFreq = 4.0; // wave_frequency (per unit length: the mesh is unit-long)
        public const double WindTwistAmp = 0.08; // m: twist_amplitude
        public const double WindTwistFreq = 3.0; // twist_frequency
        public const double WindLoopRadius = 0.22; // m: loop_radius (a small curl, not a ring)
        public const double WindLoopCount = 0.5; // loop_count (the shader's advice: a fraction, so the blend never bends awkwardly)
        public const double WindWaveCount = 1.0; // wave_count: waves of straight travel between curls
        public const float WindAlpha = 0.28f; // the wisps' brightness (additive, cyan-pale: the player's)
        public const double WindWidth = 0.035; // m: a segment's width

        // --- the kill's floor ring under a flying kill (Chunks.cs KillRing20) ---------------------------------------------
        public const float KillRingProbeLift = 0.3f; // m above the feet the down ray starts (a body standing on the floor finds it at once)
        public const double KillRingReach = 8; // m below the feet a floor still takes the ring (further: a drop, no ring)
        public const float KillRingSnap = 0.4f; // m: within this the ring stays at the feet as before (a grounded kill is NIGHT19's)

        // --- gunFx.js's sprays (GunFx.Spray20.cs: doubleBlast, coneWave, flame) ---------------------------------------------
        public const int Spray20Pool = 256; // particles alive at once (particles.js's pool is shared; this one is the guns')
        public const float Spray20StreakTime = 0.03f; // s of travel a particle's streak shows behind it (a cube in the web)
        public const float Spray20Glow = 1.6f; // x the cyan ladder (HDR: the web's family colours x 2 on the neon material)
        public const uint Spray20SeedSalt = 0x73707279; // "spry": the render-side stream's salt
        public const float Spray20ClearInner = 34f; // px at 1080p round the crosshair an aim spray's streak isn't drawn in (banned 16: the reticle's own ~30 px)
        public const float Spray20ClearOuter = 80f; // px at 1080p from which it's whole again (smoothstep between)

        // --- NIGHT20 A1: the BULWARK's attacks (BulwarkFx20.cs; bulwark.js render's own literals) ---------------------------
        public const uint BulwarkRingHex = 0xff7a30; // bulwark.js :70 the shock ring's colour (red-orange: an enemy's attack)
        public const float BulwarkRingGain = 2f; // :70 .multiplyScalar(2)
        public const int BulwarkRingSides = 56; // :71 CylinderGeometry(1, 1, 1, 56, 1, true)
        public const float BulwarkRingRampPow = 1.6f; // the port's ramp up the ring (brightest at the floor; ^1.6: half bright at 35 % up)
        public const uint BulwarkBeamHex = 0xff3a10; // :72 the sweep beam's colour
        public const float BulwarkBeamThick = 0.3f; // :73 BoxGeometry(1, 0.3, 0.3): m across

        // --- NIGHT20 B5: the VORTEX's orbs (VortexFx20.cs; projectiles.js render's own numbers are weapons.vortex's) ----------
        public const int VortexFxOrbs = 4; // orbs drawn at once (weapons.vortex.pool is 3)
        public const int VortexFxMotes = 96; // motes alive at once (33 a second for <= 0.6 s, three orbs)
        public const uint VortexFxSalt = 0x766f7278; // "vorx": the motes' render-side stream's salt (never a sim stream)
        public const float VortexCoreGlow = 2.4f; // projectiles.js :69 the orb's hdr(fam.gravity, 2.4), in CYAN-HOT
        public const float VortexRingGlow = 2f; // :71 the rings' hdr(.., 3) on the ring sprite's soft edge: a little under
        public const float VortexMoteGlow = 1.6f; // the motes (particles.homing's cubes x 2 on the neon material: Spray20Glow)
        public const float VortexGlowSize = 3f; // x the orb's radius: its soft glow behind (Kenney flare)
        public const float KenneyRingMid = 0.63f; // circle_02's ring at 0.54 .. 0.72 of the half size: a ring of radius r is a sprite r / 0.63
        public const double VortexRepelBirth = 0.2; // the port's repel: motes born at this share of pullRadius, flying out to it
        public const float VortexMoteLife = 0.6f; // :543 particles.homing(..., 0.6): s
        public const float VortexMoteIn = 0.08f; // s a mote takes to come up (no pop)
        public const float VortexMoteStreak = 0.05f; // s of its travel a mote's streak shows (0.7 m at 14 m/s)
        public const float VortexMoteWidth = 0.06f; // m across a mote's streak
        public const float VortexBurstRingTime = 0.3f; // s: the burst's ring runs out to burstRadius (ease-out)
        public const float VortexBurstLift = 0.1f; // m: the burst ring over the blast's point

        // --- NIGHT20 B5: the TORCH's fire wall (GunFx.Spray20.cs; gunFx.js's flames plus the port's floor band) -------------
        public const double FireWallBandIn = 0.12; // s the floor band takes to come up (ease-out) as the wall is laid
        public const double FireWallBandOut = 0.6; // s: it fades over the wall's last part (ease-in), gone with it
        public const int FireWall20Slots = 4; // walls drawn at once (Core/Guns/Torch.cs keeps 4: the oldest gives way)
        public const float FireWallBandGlow = 1.2f; // x CYAN (additive HDR: under the flames' 1.6, a floor mark, not a light)
        public const float FireWallBandAlpha = 0.55f; // its brightness at full (the flames above it carry the heat)
        public const float FireWallBandWidth = 3f; // x the wall's half width (Torch.cs's burn reach): trace_06's light is its middle ~27 %, so the bright core is ~0.8 x the burn
        public const float FireWallBandLength = 1.15f; // x the line: trace_06 fades at its ends, so the core covers the whole line
        public const float FireWallBandLift = 0.06f; // m over the floor the band lies (no z-fight with the floor)

        // --- the affixes' looks (AffixFx20.cs; the web's affixes.js colours: config.affixes.kinds.<name>.color) -----------
        public const double FuseGlowFrom = 0.25; // VOLATILE's fuse glow: this share of the burst's radius at the kill ...
        public const double FuseGlowTo = 0.6; // ... to this at the burst (affixes.js's flash is radius x 0.6)
        public const float FuseGlowHdr = 2.2f; // x its colour (config.telegraph.glow: the language's rim brightness)
        public const double BlinkTrailLife = 0.25; // s the BLINKER's path shows from where it left to where it landed
        public const float BlinkTrailWidth = 0.12f; // m
        public const int BlinkTrailEchoes = 4; // afterimage sparks along the path
        public const double RegenGlowSize = 1.3; // m across: REGENERATING's glow on the body while it heals
        public const double RegenPulseHz = 1.6; // its breathing (a slow heal reads slow)
        public const double RegenMoteEvery = 0.22; // s between the rising motes while it heals
        public const double RegenMoteRise = 1.4; // m/s the motes rise
        public const double RegenMoteLife = 0.6; // s
        public const double SwiftTrailFrom = 3; // m/s: a SWIFT body trails nothing slower than this ...
        public const double SwiftTrailTime = 0.08; // ... then this many s of its travel behind it ...
        public const double SwiftTrailMax = 1.2; // ... at most this long (m)
        public const float SwiftTrailWidth = 0.05f; // m
        public const float AffixFxAlpha = 0.8f; // the looks' brightness (additive)
        public const int AffixFxPool = 64; // sprites a frame per texture
        public const float AffixSparkWidth = 1.6f; // x the web's cube size across a burst spark's streak (trace_06's light is its middle ~27 %: a ~0.45 x cube core)
        public const float AffixSparkStreak = 0.03f; // s of a spark's travel its streak shows behind it (GunSpray20's)
        public const int AffixSparkPool = 96; // sparks alive at once (three bursts)
        public const uint AffixSparkSalt = 0x766f6c74; // "volt": the sparks' render-side stream's salt (never a sim stream)

        // --- A2 THE HUNT's ring and A4 THE ELITE's aura (SpecialFx20.cs; DANGER: the enemies') ---------------------------
        public const double HuntRingPeriod = 1.1; // s a ripple takes to close from ringMax to ringMin (the hunt closing in)
        public const int HuntRingRipples = 3; // ripples at once, out of step
        public const double HuntRingIn = 0.4; // s the ring fades in on the announce
        public const double HuntRingHold = 4; // s it stays after the round starts (its first squads are out) ...
        public const double HuntRingOut = 1; // ... then fades over this
        public const float HuntRingAlpha = 0.5f; // its brightness (additive DANGER, just off the floor)
        public const float HuntRingWidth = 0.06f; // of the radius: each ripple's band
        public const int HuntRingSegments = 96;
        public const double EliteAuraSize = 2.6; // x the elite's radius: its floor rim (the Kenney ring sprite)
        public const double EliteAuraSpin = 0.8; // rad/s the rim turns
        public const double EliteAuraPulseHz = 1.2; // its glow breathes
        public const float EliteAuraAlpha = 0.7f; // the rim's brightness
        public const double EliteGlowSize = 1.4; // x the elite's height: the glow behind its body
        public const float EliteGlowAlpha = 0.35f; // the glow's brightness
        public const double EliteEmberEvery = 0.12; // s between the embers rising off it
        public const double EliteEmberRise = 1.8; // m/s
        public const double EliteEmberLife = 0.7; // s
    }
}
