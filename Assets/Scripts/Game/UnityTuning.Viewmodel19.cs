// NIGHT19 U3-33.5 / U3-33.8 (builder V): the hands' and effects' render numbers for tonight's moves (CLAUDE.md rule 5:
// Unity-only numbers, each with a comment). Why the web has none of them: the web's KESTREL zooms on its alt-fire (the
// gust is port-only, NIGHT19's), the web has no per-gun alt-fire animation (its viewmodel animates the primary only), its
// particles are untextured cubes (the textured sprites are the Kenney Particle Pack's, CC0, D-72), and its arcs are the
// volt family's lime (tonight's ZAPPER arc is the player's cyan, CLAUDE.md rule 8). Every number here is visual only: the
// sim fires the gust, the zaps, the grenades and the alts; nothing here gates input or fire, or changes the fingerprint.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the gust's kick in the hands (Viewmodel.Gust.cs; Spring-It-On's critically damped decay spring) ------------
        public const double GustKickHalflife = 0.075; // s: the blowback's half-life (peaks at ~54 ms, settled by ~0.4 s, no wobble)
        public const double GustKickBackZ = 0.11; // m the gun is shoved back toward the eye at the peak (web +z)
        public const double GustKickPitchDeg = 16; // deg the muzzle flips up at the peak (the air pushes the hand up and back)
        public const double GustKickRollDeg = 7; // deg of wrist roll at the peak (counter-clockwise: the hand opens)
        public const double GustKickDropY = -0.03; // m: negative lifts (the hand rides up with the flip)
        public const double GustKickSideX = 0.018; // m to the right (the hand is thrown out of the line of the air)
        public const double GustThrustZ = 0.06; // m the gun first thrusts forward (pushing the air out) ...
        public const double GustThrustTime = 0.05; // ... over this sine bump (s), before the blowback carries it back
        public const double GustKickDelay = 0.025; // s after the gust the blowback lands (the thrust's peak: forward first, then back)
        public const double GustAirLiftY = 0.03; // m the gun drops in view on a gust that launches the body up (its inertia)
        public const double GustGemFlash = 1; // x the gem's fire flash on a gust (through game.flashGate)

        // --- the gust's FOV kick (CameraFeel; every FOV kick's scale and REDUCED MOTION apply) --------------------------
        public const double GustFovKickDeg = 6.5; // deg wider on the gust's first frame (the boost's is 11.8: the gust is smaller)
        public const double GustFovBackTime = 0.3; // s back to rest (a smoothstep fall)
        public const double GustFovAirMult = 1.25; // x the kick for a gust fired in the air (the gust-jump reads bigger)

        // --- the gust's air (MovementFx.Gust.cs): a cone of streaks and two shock rings, the player's cyan ---------------
        public const int GustStreaks = 22; // streaks a gust throws out along its cone
        public const double GustStreakSpeed = 42; // m/s a streak leaves the muzzle at (slowed by the drag below)
        public const double GustStreakSpeedJitter = 0.35; // +- share of the speed per streak
        public const double GustStreakDrag = 6.5; // 1/s: the air's drag (x(t) = v0 / k (1 - e^-kt): ~6 m in its life)
        public const double GustStreakLife = 0.3; // s a streak lives (fading over the second half)
        public const double GustStreakLifeJitter = 0.3; // +- share of the life
        public const double GustStreakStretch = 0.04; // s of travel a streak's length shows (long while fast, short as it slows)
        public const double GustStreakMinLen = 0.25; // m: a streak never shrinks below this
        public const double GustStreakWidth = 0.14; // m across a streak's sprite (trace_06's light is its middle 27 %: ~4 cm seen)
        public const double GustStreakStart = 0.35; // m ahead of the muzzle a streak starts (clear of the gun)
        public const double GustStreakRadial = 0.12; // m of radial scatter at the start
        public const double GustConeShare = 0.85; // x the sim's cone half-angle the streaks fill (inside what it pushes)
        public const float GustStreakHdr = 2.2f; // x the streak's colour (additive, under the bloom's clip)
        public const double GustRingLife = 0.24; // s the near ring lives
        public const double GustRingStartR = 0.22; // m the near ring starts at ...
        public const double GustRingEndR = 2.1; // ... and grows to (ease-out)
        public const double GustRingAhead = 0.75; // m ahead of the muzzle the near ring stands
        public const double GustRing2Life = 0.34; // s the far ring lives
        public const double GustRing2StartR = 0.45, GustRing2EndR = 3.4; // m: the far ring's radius over its life
        public const double GustRing2From = 0.9, GustRing2To = 5.5; // m ahead of the muzzle: the far ring travels out along the aim
        public const float GustRingHdr = 2.4f; // x the rings' colour
        public const double GustRingAlpha = 0.95; // the rings' brightness at birth (x (1 - t)^1.5)
        // the wake: lines streaming past while the push carries the body
        public const double GustWakeTime = 0.55; // s after the gust the wake may run
        public const double GustWakeMinSpeed = 3.5; // m/s along the push below which the wake stops
        public const double GustWakeRate = 90; // lines a second at full push
        public const float GustWakeLife = 0.2f; // s a wake line lives
        public const float GustWakeLength = 1.5f; // m a wake line is long (laid along the body's velocity)
        public const float GustWakeWidth = 0.022f; // m across
        public const float GustWakeBrightness = 0.75f; // x the cyan (the wake is quieter than the burst)
        public const float GustWakeEdgeMin = 0.45f, GustWakeEdgeMax = 1.05f; // the view's band the wake lines appear in (centre 0 .. edge 1)
        public const float GustWakeAheadMin = 2.5f, GustWakeAheadMax = 6f; // m ahead of the eye along the view
        public const double GustPuffLife = 0.36; // s the swirl under a gust-jump lives
        public const double GustPuffStartR = 0.35, GustPuffEndR = 1.9; // m: the swirl's radius over its life
        public const double GustPuffMinUp = 0.35; // the push's up share (unit) a swirl needs (a gust aimed down)
        public const float GustPuffHdr = 1.8f; // x the swirl's colour
        public const double GustPuffSpinDeg = 140; // deg the swirl turns over its life
        public const int GustPool = 4; // gusts drawn at once (a gust a step at most; the oldest gives way)
        public const string ProbeGust = "gust"; // the reel's event marker on the gust's step (FeelProbe.Event; the bus's own name)
        public const double GustAirLiftTime = 0.35; // s: the gun's drop in view on a gust that launches the body (a sine bump)
        public const uint GustFxSeedSalt = 0x67757374; // "gust": the render-side stream's salt (never a sim stream)

        // --- the textured sprites (FxSprites.cs; the Kenney Particle Pack, CC0) --------------------------------------------
        public const int FxSpriteLevels = 8; // brightness steps per colour (a step is one additive material: URP Unlit has no vertex colour)
        public const string KenneyStar = "KenneyParticles/star_06"; // Resources path: a sharp four-point star (the hit flare)
        public const string KenneyTrace = "KenneyParticles/trace_06"; // a soft streak (chips, the gust's streaks, rays)
        public const string KenneyRing = "KenneyParticles/circle_02"; // a crisp ring (the gust's shock rings, the impact ring)
        public const string KenneyTwirl = "KenneyParticles/twirl_02"; // a swirl (the gust-jump's air under the feet)
        public const string KenneyBolt = "KenneyParticles/spark_07"; // a horizontal lightning strip (the ZAPPER's arc texture)
        public const string KenneyFlare = "KenneyParticles/flare_01"; // a soft point glow (the grenade's fuse light)
        public const float KenneyRingInner = 0.54f, KenneyRingOuter = 0.72f; // circle_02's ring as a share of the half-size (measured on the PNG: a crisp inner edge at 0.54, fading out by 0.72)

        // --- the textured hit spark (HitSpark.cs, FeelAB on) ----------------------------------------------------------------
        public const float HitStarScale = 1.9f; // x the flare's size for the star sprite (its rays are thin: same read, less area)
        public const float HitStarHdr = 2.2f; // x white for the star (additive)
        public const float HitChipStreak = 0.035f; // s of a chip's travel its streak shows
        public const float HitChipStreakWidth = 3.2f; // x the chip's size across its streak's sprite (trace_06's light is the middle 27 %: ~0.9 chip seen)
        public const float HitChipHdr = 2.0f; // x the chips' colour (white-hot frames, then the enemy's red-orange)

        // --- the impact ring (ImpactFx.cs, FeelAB on): the ring sprite laid flat ------------------------------------------
        public const float ImpactRingHdr = 1.0f; // x the kinetic family's glow on the sprite ring

        // --- horde5's confetti (NIGHT18's "too much"; NIGHT19 banned pattern 6) -------------------------------------------
        public const int DeathBurstWithChunks = 0; // cubes a kill bursts when the chunks draw (the web's fx.deathCount 26: the confetti)
        public const int BlastBurstShareN19Pct = 35; // % of the rocket's / grenade's blast cubes kept (the web's 30 + 12)

        // --- the ZAPPER's arc (GunFx.Zapper.cs; the web's arcs.js with weaponFx.arc*) -------------------------------------
        public const int ZapArcPool = 12; // arcs at once (the web pools weaponFx.arcPool 32 for every gun's arcs; the ZAPPER's alone)
        public const float ZapArcHdr = 1.0f; // x the web's arcIntensity on the cyan
        public const float ZapCoreShare = 0.35f; // the white core's width share (arcs.js `core`)
        public const int ZapSparkCount = 5; // textured sparks where an arc lands (the web bursts 4 volt cubes)
        public const float ZapSparkSize = 0.32f; // m: a landing spark's star

        // --- the BOUNCER's grenade (GunFx.Bouncer.cs; projectiles.js render :495-506) -------------------------------------
        public const double GrenadeSpinRate = 9; // rad/s: projectiles.js `p.spin += frameDt * 9` (the web's literal)
        public const double GrenadeBlinkScale = 1.25; // projectiles.js: the blink's scale (the web's literal)
        public const double GrenadeBlinkFrom = 0.6; // s of fuse left the blink starts (the web's literal)
        public const double GrenadeBlinkHz = 20; // projectiles.js floor(left x 20) % 2 (the web's literal)
        public const float GrenadeSize = 0.2f; // m: projectiles.js BoxGeometry(0.2, 0.2, 0.2)
        public const float GrenadeHdr = 1.8f; // projectiles.js hdr(fam.blast, 1.8)
        public const float GrenadeFuseLight = 0.55f; // m: the fuse light's flare sprite while it blinks (port-only)
        public const float GrenadeFuseHdr = 2.4f; // x the danger red-orange (a grenade about to burst is a danger)

        // --- the alt-fires' animations (Viewmodel.Alt19.cs) -------------------------------------------------------------------
        public const double AltHalflife = 0.06; // s: every alt kick's spring half-life
        public const double AltParryZ = -0.07, AltParryPitchDeg = -8, AltParryRollDeg = -14; // THUNDERCLAP: a bash forward, barrels turned
        public const double AltDetonateRollDeg = 28, AltDetonatePitchDeg = 6; // STITCHER: a key-turn twist on the detonation
        public const double AltOverchargeShakeDeg = 1.4, AltOverchargeHz = 31, AltOverchargeIn = 0.03; // LONGWAVE: the tremble at full overcharge, its rate, the pull-in (m)
        public const double AltSteerX = -0.06, AltSteerY = 0.035, AltSteerPitchDeg = 3, AltSteerRollDeg = -9, AltSteerRate = 14; // COMET: raised to the eye, canted, eased in at this rate
        public const double AltDischargeZ = -0.05, AltDischargePitchDeg = -5; // ZAPPER: the fork thrust at the discharge
        public const double AltAirburstPitchDeg = 12, AltAirburstY = -0.015; // BOUNCER: a flick up on the airburst
        public const double AltSteerHold = 0.06; // s the COMET's steer pose holds after the sim's last steering step

        // --- the R3 jab (the punch's pose read as a quick jab; it stays weak) -------------------------------------------------
        public const double JabCockShare = 0.25; // the wind-up's share of punchOutTime (NIGHT18's 0.4: a jab barely loads)
        public const double JabCockX = -0.16, JabCockY = -0.13, JabCockZ = -0.32; // m: tucked low left, not pulled back far
        public const double JabCockPitchDeg = 24, JabCockYawDeg = -12, JabCockRollDeg = 4; // the wrist level (a jab, not a hook)
        public const double JabAimX = 0.0, JabAimY = -0.02; // m on armPunch: straight down the middle
        public const double JabOverZ = 0.06; // m past armPunch on the impact frame (a straight reach)
        public const double JabOverRollDeg = -3; // deg: the knuckles stay level
        public const double JabSnapTime = 0.025; // s back after the impact frame (snappier than NIGHT18's 0.03)
    }
}
