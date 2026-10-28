// PG3 V1 (WEAPON FEEL; Fx74.cs, Shot74.cs, Charge74.cs): Unity-only numbers (CLAUDE.md rule 5: each with a comment; the web never had any of them, the sim never reads
// them, so the golden vectors, the replays and the fingerprints stay as they were). Sizes are metres at the muzzle's depth (UnityTuning.FxMuzzleDepth, 0.6 m in front
// of the eye) unless a comment says otherwise; colours are HDR multipliers of cyan (colors.usable) or white (the post stack blooms anything over 1).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- the shared kit (Fx74.cs) ----
        public const int PG3V1MaxQuads = 160; // quads one view may submit in a frame (a THUNDERCLAP volley peaks near 100: 8 pellets x (2 comet + 3 impact) + the flash)
        public const int PG3V1TexSize = 128; // px: each procedural texture's side (the star's thin spikes need it; four textures of 64 KB)
        public const float PG3V1SparkStretchS = 0.045f; // s: a spark's streak is its speed x this long (capped by its own length), so a fast one is long and a slow one a dot
        public const uint PG3V1SeedSalt = 0x70673176; // "pg1v": the cosmetic dice's salt (XOR the run seed; gameplay randomness is untouched)
        public const float PG3V1HotWhite = 0.8f; // 0..1: how far "white-hot" is from cyan toward white (0.8 reads white with a cyan edge)
        public const float PG3V1CapSoftDeg = 20f; // deg of view: the most a soft white flash (a core) may subtend (GG's NIGHT43 complaint: a charged release's flash "filled most of the screen"; feel43 capped its tip at 4 degrees)
        public const float PG3V1CapStarDeg = 34f; // deg: the most a star may subtend (its spikes are thin and its core is a quarter of its width, so it can be wider than a soft core)
        public const float PG3V1CapRingDeg = 56f; // deg: the most a ring may subtend (a thin line: it may sweep out toward the screen's edge, never past it much)
        // ---- shot74: pools ----
        public const int PG3V1SpritePool = 96; // timed sprites (impacts, bursts, the wall) alive at once
        public const int PG3V1SparkPool = 160; // sparks alive at once (a volley's 8 impacts x 3 plus two bursts)
        public const int PG3V1CometPool = 24; // tracer comets alive at once (a volley's pellets plus the next shot's)
        // ---- shot74: the muzzle flash ----
        public const float PG3V1FlashS = 0.045f; // s: the KESTREL's flash (about 3 frames at 60 fps); whole for the first 30 %, then it snaps down
        public const float PG3V1FlashThunderS = 0.060f; // s: the THUNDERCLAP's (a little longer: it is the bigger gun)
        public const int PG3V1FlashMinFrames = 2; // rendered frames the flash is drawn for however short the frame time is (a 144 Hz frame is 7 ms)
        public const float PG3V1FlashMinEnv = 0.4f; // the least brightness (share) of those minimum frames once the time envelope is under it
        public const float PG3V1FlashHold = 0.30f; // share of the flash's life it stays whole before the squared fall
        public const float PG3V1ThunderScale = 1.45f; // x every flash size for the THUNDERCLAP
        public const float PG3V1FlashHaloM = 0.22f; // m: the soft cyan halo (a muzzle-sized flash like the muzzle star, which the flash gate does not count: UnityTuning.StarScale's note)
        public const float PG3V1FlashHaloGain = 0.7f; // HDR: the halo's cyan
        public const int PG3V1FlashSpikes = 3; // streak spikes out of the KESTREL's muzzle (thin rays in the screen's plane, on top of the star's own)
        public const int PG3V1FlashSpikesThunder = 6; // ... the THUNDERCLAP's
        public const float PG3V1FlashSpikeLenM = 0.30f; // m: a spike's mean length (each is 0.55-1.25 of it, and shortens as the flash dies)
        public const float PG3V1FlashSpikeWidthM = 0.022f; // m: a spike's width at the muzzle
        public const float PG3V1FlashSpikeGain = 3.8f; // HDR: a spike (cyan-white)
        public const float PG3V1FlashSpikeJitterDeg = 40f; // deg: how far a spike may sit off its even share of the circle
        public const float PG3V1FlashStarM = 0.30f; // m: the lens-flare star (its long spike is the horizontal one); the THUNDERCLAP's x1.45 runs into the star cap (PG3V1CapStarDeg: 0.39 m at the muzzle)
        public const float PG3V1FlashStarGain = 4.2f; // HDR: the star (cyan toward white)
        public const float PG3V1FlashStar2M = 0.25f; // m: the second, smaller star rolled off the first: the crossed layers
        public const float PG3V1FlashStar2RollDeg = 37f; // deg: how far the second star is rolled from the first
        public const float PG3V1FlashStar2Gain = 3.2f; // HDR: the second star (white)
        public const float PG3V1FlashCoreM = 0.10f; // m: the white-hot core
        public const float PG3V1FlashCoreGain = 6f; // HDR: the core (white; blooms hard for those frames)
        public const float PG3V1ThunderRingFromM = 0.2f; // m: the THUNDERCLAP's blown-out ring at the start of the flash
        public const float PG3V1ThunderRingToM = 0.85f; // m: ... and at its end
        public const float PG3V1ThunderRingGain = 2.2f; // HDR: that ring
        public const float PG3V1ThunderRingAheadM = 0.15f, PG3V1ThunderRingTravelM = 0.5f; // m: where the ring starts ahead of the muzzle, and how far along the aim it moves as it grows
        public const float PG3V1LightKestrel = 2.4f; // brief point light at the KESTREL's muzzle (LightPools.Flash intensity; Fx49's heavy tier is 2.5)
        public const float PG3V1LightThunder = 3.4f; // ... the THUNDERCLAP's (its critical tier is 4.0)
        public const float PG3V1LightRangeM = 5f; // m: those lights' reach (Fx49's heavy range is 4)
        public const float PG3V1LightS = 0.06f; // s: how long they last
        // ---- shot74: the THUNDERCLAP's wall ----
        public const float PG3V1WallDistM = 3.2f; // m ahead of the muzzle: where the disc and ring across the spread's cone stand
        public const float PG3V1WallWidth = 0.95f; // x the cone's full width at that distance (weapons.shotgun.coneHalfAngleDeg x 2): a little inside the real spread (the pellets' tracers fill it out)
        public const float PG3V1WallS = 0.09f; // s: the wall grows and fades over this (about 5 frames at 60 fps)
        public const float PG3V1WallDiscGain = 0.5f; // HDR: the soft cyan disc across the cone (low: it is a veil, not a flash; at 3.2 m it is 18 degrees wide)
        public const float PG3V1ImpactLead = 0.9f; // x: an impact / burst lands at this share of its tracer's flight time (the round is seen to arrive, the flash is not late)
        public const float PG3V1WallRingGain = 2.4f; // HDR: the ring on the wall's rim
        // ---- shot74: tracers ----
        public const float PG3V1TracerSpeed = 380f; // m/s the tracer's head flies at (it is an effect: the shot itself is hitscan)
        public const float PG3V1TracerMaxTravelS = 0.07f; // s: the longest a head takes to arrive (a far wall is crossed in a few frames)
        public const float PG3V1TracerSettleS = 0.05f; // s: after the head arrives the tail runs on to the end in this
        public const float PG3V1TracerCoreM = 0.022f; // m: the KESTREL tracer's white core width
        public const float PG3V1TracerSleeveM = 0.10f; // m: its cyan sleeve width
        public const float PG3V1TracerTailM = 3.2f; // m: how far the tail trails the head
        public const float PG3V1TracerCoreGain = 7f; // HDR: the core
        public const float PG3V1TracerSleeveGain = 0.9f; // HDR: the sleeve
        public const float PG3V1PelletCoreM = 0.014f; // m: a THUNDERCLAP pellet tracer's core width
        public const float PG3V1PrimaryPelletK = 1.4f; // x: the real first pellet (the shotResult's) is drawn a little thicker than the cosmetic ones
        public const float PG3V1PelletSleeveM = 0.055f; // m: a pellet's sleeve width
        public const float PG3V1PelletTailM = 1.6f; // m: a pellet's tail
        public const float PG3V1PelletSleeveGain = 0.55f; // HDR: a pellet's sleeve (eight of them add up)
        // ---- shot74: impacts on the world ----
        public const float PG3V1DecalLiftM = 0.012f; // m off the surface along its normal (no z-fight with the wall)
        public const float PG3V1ImpactM = 0.13f; // m: the impact's unit size (the ring runs 0.4-2 of it, the decal 1.1-1.45, the star 1.9-2.8)
        public const float PG3V1ImpactRingFromK = 0.4f, PG3V1ImpactRingToK = 2.0f; // x PG3V1ImpactM: the shock ring's size at its start and its end
        public const float PG3V1ImpactRingEdge = 0.4f; // HDR: the ring's cyan as it goes out
        public const float PG3V1DecalFromK = 1.1f, PG3V1DecalToK = 1.45f; // x PG3V1ImpactM: the hot decal's size as it forms and as it cools
        public const float PG3V1DecalOverRingM = 0.003f; // m: the decal sits this far over the ring (no z-fight between the two)
        public const float PG3V1ImpactStarFromK = 1.9f, PG3V1ImpactStarToK = 2.8f; // x PG3V1ImpactM: the star over the impact
        public const float PG3V1ImpactStarLiftM = 0.05f; // m: the star stands this far off the wall toward the shooter
        public const float PG3V1ImpactStarEdge = 1.6f; // HDR: the star's cyan as it goes out
        public const float PG3V1ImpactRingS = 0.16f; // s: the shock ring on the surface
        public const float PG3V1ImpactRingGain = 3f; // HDR: that ring (white)
        public const float PG3V1DecalS = 0.55f; // s: the hot decal cools from white-hot to nothing over this
        public const float PG3V1DecalGain = 3.2f; // HDR: the decal at its hottest
        public const float PG3V1DecalCool = 0.12f; // HDR: the decal's cyan as it goes out
        public const float PG3V1ImpactStarS = 0.07f; // s: the star over the impact (about 4 frames)
        public const float PG3V1ImpactStarGain = 5f; // HDR: that star
        public const float PG3V1PelletImpactK = 0.7f; // x: a THUNDERCLAP pellet's impact is this much of a KESTREL's
        public const int PG3V1WorldSparks = 8; // sparks off a KESTREL world hit
        public const int PG3V1PelletSparks = 3; // sparks off a THUNDERCLAP pellet's world hit
        public const float PG3V1SparkSpeedMin = 3.2f; // m/s
        public const float PG3V1SparkSpeedMax = 7.5f; // m/s
        public const float PG3V1SparkLifeMin = 0.22f; // s
        public const float PG3V1SparkLifeMax = 0.38f; // s
        public const float PG3V1SparkLenM = 0.30f; // m: a spark's longest streak
        public const float PG3V1SparkWidthM = 0.016f; // m
        public const float PG3V1SparkGain = 4f; // HDR: a spark (white-hot to cyan)
        public const float PG3V1SparkDrag = 2.2f; // 1/s: air drag on a spark (fx.gravity, the config's, pulls it down)
        // ---- shot74: the enemy burst ----
        public const float PG3V1BurstM = 0.55f; // m: the burst's unit size at 8 m (its flash 0.55-1.0, star 1.0-1.3, ring 0.2-1.55 of it)
        public const float PG3V1BurstFarRefM = 8f; // m: from this far the burst grows with distance so it reads (a fixed size would be a speck at 25 m)
        public const float PG3V1BurstFarMaxK = 2f; // x: the most it grows
        public const float PG3V1BurstMaxK = 1.6f; // x: the most pellets can grow one enemy's burst
        public const float PG3V1BurstPerPellet = 0.12f; // x each further pellet on the same enemy adds
        public const float PG3V1BurstHeadK = 1.3f; // x for a headshot
        public const float PG3V1BurstKillK = 1.25f; // x for a killing hit
        public const float PG3V1BurstLiftMaxM = 0.3f; // m: the burst sits this much on the camera's side of the hit (never buried in the body)
        public const float PG3V1BurstFlashFromK = 0.55f, PG3V1BurstFlashToK = 1.0f; // x the burst's unit size: the hot flash's size at its start and its end
        public const float PG3V1BurstStarFromK = 1.0f, PG3V1BurstStarToK = 1.3f; // x: the big star's
        public const float PG3V1BurstRingFromK = 0.2f, PG3V1BurstRingToK = 1.55f; // x: the ring blown out
        public const float PG3V1BurstFlashEdge = 1.2f, PG3V1BurstStarEdge = 2.2f, PG3V1BurstRingEdge = 0.6f; // HDR: each one's cyan as it goes out
        public const float PG3V1BurstFlashS = 0.08f; // s
        public const float PG3V1BurstFlashGain = 5f; // HDR: the hot flash (white; it stacks on fx49's own burst, so it is kept under the muzzle core's 6)
        public const float PG3V1BurstStarS = 0.10f; // s
        public const float PG3V1BurstStarGain = 4.2f; // HDR: the big star
        public const float PG3V1BurstRingS = 0.17f; // s: the ring blown out
        public const float PG3V1BurstRingGain = 3f; // HDR: that ring
        public const int PG3V1BurstSparks = 10; // sparks of a plain hit
        public const int PG3V1BurstSparksBig = 18; // sparks of a headshot or a kill
        public const float PG3V1BurstLean = 0.45f; // how far the sparks lean toward the camera (so they read on the screen)
        public const float PG3V1BurstSpeedMin = 4f; // m/s
        public const float PG3V1BurstSpeedMax = 9.5f; // m/s
        public const float PG3V1BurstLight = 2.6f; // brief point light at a burst
        public const float PG3V1BurstLightNearM = 1.2f; // m: no light from a burst this close to the eye (it would light the whole view)
        // ---- charge74 ----
        public const int PG3V1ChargeSpritePool = 64; // sprites alive at once (a release is about 14, a pulse 5, a parry 4)
        public const float PG3V1FullAt = 0.995f; // charge level counted as FULL (the hold's level is 1.0 exactly; the glove's pips are k / n)
        public const float PG3V1DimK = 0.35f; // x: a flash the flash gate refused is drawn this dim, ring only (WCAG 2.3.1: three flashes in a second)
        public const float PG3V1GloveK = 0.6f; // x: the glove's glow (a standing charge) against a hold's
        public const float PG3V1GloveRise = 3f; // level per s: how fast the glove's glow follows its pips (they come in steps)
        public const float PG3V1GlowFromM = 0.06f; // m: the charge glow's size at a touch of charge
        public const float PG3V1GlowToM = 0.26f; // m: ... and at full
        public const float PG3V1GlowFloor = 0.6f; // HDR: the glow's brightness at a touch of charge (it shows at once)
        public const float PG3V1GlowPeak = 2.6f; // HDR: ... and at full (level squared in between)
        public const float PG3V1GlowHaloK = 0.7f; // x the glow gain for the soft halo
        public const float PG3V1GlowCoreK = 0.42f; // x the halo's size for the hot core
        public const float PG3V1HoldSwell = 0.10f; // share: the slow swell of the glow while FULL is held
        public const float PG3V1HoldSwellHz = 2f; // Hz: that swell (a size change, not a flash: under the 3 Hz limit)
        public const float PG3V1RingFrom = 0.35f; // level from which the closing ring shows
        public const float PG3V1RingOpenM = 0.55f; // m: the ring's size as it first shows
        public const float PG3V1RingShutM = 0.22f; // m: ... closed in on the muzzle at full
        public const float PG3V1RingGain = 2f; // HDR: that ring at full
        public const float PG3V1MotesFrom = 0.3f; // level from which motes fall into the muzzle
        public const int PG3V1Motes = 7; // motes on the shell
        public const float PG3V1MoteHzLow = 1.4f; // falls per second at the start...
        public const float PG3V1MoteHzFull = 3f; // ... and at full
        public const float PG3V1MoteTurn = 2.4f; // rad/s: the shell turns
        public const float PG3V1MoteShellM = 0.30f; // m: the shell's radius round the muzzle
        public const float PG3V1MoteM = 0.032f; // m: a mote's size
        public const float PG3V1MoteGain = 3.5f; // HDR: a mote
        public const float PG3V1ReadyRingFromM = 0.18f; // m: the READY ring blown out from the muzzle
        public const float PG3V1ReadyRingToM = 0.85f; // m
        public const float PG3V1ReadyRingS = 0.17f; // s
        public const float PG3V1ReadyRingGain = 4.5f; // HDR
        public const float PG3V1ReadyStarM = 0.55f; // m: the READY star
        public const float PG3V1ReadyStarS = 0.08f; // s
        public const float PG3V1ReadyStarGain = 6f; // HDR
        public const float PG3V1ReadyLight = 3f; // brief point light at READY
        public const float PG3V1ReadyLightS = 0.07f; // s (the release, pulse and parry lights are 1.6 x this)
        public const float PG3V1BeamFallbackM = 40f; // m: a charged shot whose end was not seen is drawn this long along the aim
        public const float PG3V1BeamS = 0.20f; // s: the KESTREL's piercing beam
        public const float PG3V1SlugS = 0.18f; // s: the THUNDERCLAP slug's streak
        public const float PG3V1SlugK = 1.5f; // x: the slug's sizes against the beam's (it is fatter and shorter-lived)
        public const float PG3V1BeamHold = 0.25f; // share of the beam's life it stays whole
        public const float PG3V1BeamNarrow = 0.3f; // x: the beam's widths at the end of its life
        public const float PG3V1BeamHaloM = 0.65f; // m: the wide soft halo of the beam
        public const float PG3V1BeamHaloGain = 0.55f; // HDR
        public const float PG3V1BeamSleeveM = 0.24f; // m: the cyan sleeve
        public const float PG3V1BeamSleeveGain = 2.4f; // HDR
        public const float PG3V1BeamCoreM = 0.07f; // m: the white-hot core
        public const float PG3V1BeamCoreGain = 8f; // HDR
        public const float PG3V1RelCoreGain = 6f; // HDR: the muzzle blast's core at a release
        public const float PG3V1RelStarM = 0.75f; // m: the release star
        public const float PG3V1RelStarGain = 6.5f; // HDR
        public const float PG3V1RelRingFromM = 0.3f; // m: the ring blown along the aim at a release
        public const float PG3V1RelRingToM = 1.5f; // m
        public const float PG3V1RelRingS = 0.20f; // s
        public const float PG3V1RelRingGain = 3.4f; // HDR
        public const float PG3V1MarchFirstM = 1.4f; // m from the muzzle: the first ring marching down the beam
        public const int PG3V1MarchRings = 7; // rings down the beam
        public const float PG3V1MarchGapM = 2.6f; // m between them
        public const float PG3V1MarchFromM = 0.45f; // m: a ring's size at its start...
        public const float PG3V1MarchToM = 1.5f; // m: ... and at its end
        public const float PG3V1MarchS = 0.22f; // s: a ring's life
        public const float PG3V1MarchGain = 2.6f; // HDR
        public const float PG3V1MarchStaggerS = 0.014f; // s: each ring starts this much later than the one before (a pulse running down the line)
        public const float PG3V1EndStarM = 0.9f; // m: the burst where the beam ends
        public const float PG3V1EndStarGain = 5.5f; // HDR
        public const float PG3V1EndRingToM = 1.8f; // m
        public const float PG3V1EndRingGain = 3f; // HDR
        public const float PG3V1BeamLight = 3.6f; // brief point light at a pierce
        public const float PG3V1SlugLight = 4f; // ... at a slug
        public const float PG3V1PulseFloorLiftM = 0.06f; // m: the pulse's floor ring above the floor
        public const float PG3V1PulseFloorFromM = 0.8f; // m: the floor ring's size at the start
        public const float PG3V1PulseFloorToM = 9f; // m: ... and at its end (the blast reaches farther; the ring reads the pulse going out)
        public const float PG3V1PulseFloorS = 0.26f; // s
        public const float PG3V1PulseRingGain = 3.2f; // HDR
        public const float PG3V1PulseAheadM = 2.2f; // m ahead of the eye: the ring standing across the aim
        public const float PG3V1PulseAcrossFromM = 0.6f; // m
        public const float PG3V1PulseAcrossToM = 3.6f; // m
        public const float PG3V1PulseAcrossS = 0.20f; // s
        public const float PG3V1PulseAcrossGain = 3.6f; // HDR
        public const float PG3V1PulseStarM = 0.6f; // m: the star at the glove
        public const float PG3V1PulseStarGain = 6f; // HDR
        public const float PG3V1PulseLight = 3.6f; // brief point light at the glove
        public const float PG3V1ParryM = 0.55f; // m: the parry flash's unit size at 8 m (its core 0.7-1.3, star 1.0-1.35, ring 0.3-2.3 of it)
        public const float PG3V1ParryFarRefM = 8f; // m: from this far the flash grows with distance
        public const float PG3V1ParryFarMaxK = 2f; // x: the most it grows
        public const float PG3V1ParryLiftMaxM = 0.3f; // m: on the camera's side of the parried body
        public const float PG3V1ParryCoreS = 0.08f; // s
        public const float PG3V1ParryCoreGain = 4.5f; // HDR: the soft core
        public const float PG3V1ParryStarS = 0.10f; // s
        public const float PG3V1ParryStarGain = 7f; // HDR: the star (white)
        public const float PG3V1ParryRingS = 0.18f; // s
        public const float PG3V1ParryRingGain = 4f; // HDR
        public const float PG3V1ParryLight = 3.6f; // brief point light at a parry
        public const float PG3V1ParryLightNearM = 1.2f; // m: no light from a parry this close to the eye
        public const float PG3V1KickFovDeg = 2.2f; // deg: the parry's FOV punch-in on the first rendered frame (x FOV KICK)
        public const float PG3V1KickPitchDeg = 0.6f; // deg: and its pitch dip (x CAMERA SHAKE); none with REDUCED MOTION
        public const float PG3V1KickSecond = 0.45f; // x: the second rendered frame's share of the kick (a third frame has none)
        // ---- charge74: the shapes of the flashes (sizes in metres, or x a unit size where the name says K; Edge = the cyan a sprite fades to, HDR) ----
        public const float PG3V1StarGrowK = 1.2f; // x: a star's size at the end of its life against its start
        public const float PG3V1StarSpinDegS = 97f; // deg per second of the view's clock: a star's roll is the clock's (each flash rolled differently, no dice)
        public const float PG3V1ReadyRingEdge = 0.5f, PG3V1ReadyStarEdge = 1.8f; // HDR: the READY ring's and star's cyan as they go out
        public const float PG3V1SlugRingMaxK = 1.25f; // x: the most the slug's rings grow over the beam's (its streak and core grow by PG3V1SlugK)
        public const float PG3V1RelCoreFromM = 0.32f, PG3V1RelCoreToM = 0.55f; // m: the muzzle blast's soft white core at a release (capped to PG3V1CapSoftDeg)
        public const float PG3V1RelCoreS = 0.08f, PG3V1RelStarS = 0.10f; // s: the core's and the star's life
        public const float PG3V1RelCoreEdge = 1.2f, PG3V1RelStarEdge = 2.0f, PG3V1RelRingEdge = 0.5f; // HDR: each one's cyan as it goes out
        public const float PG3V1RelRing1AheadM = 0.25f, PG3V1RelRing2AheadM = 0.45f; // m ahead of the muzzle along the aim: the two rings blown out of it
        public const float PG3V1RelRing2K = 0.7f, PG3V1RelRing2Gain = 0.6f, PG3V1RelRing2DelayS = 0.035f; // the second ring: its size and brightness against the first, and how much later it starts
        public const float PG3V1EndBackM = 0.06f; // m: the beam's end burst stands this far back from the wall (not buried in it)
        public const float PG3V1EndS = 0.11f; // s: the end star's life
        public const float PG3V1EndStarEdge = 1.6f, PG3V1EndRingEdge = 0.4f; // HDR: the end star's and ring's cyan as they go out
        public const float PG3V1EndRingFromM = 0.2f, PG3V1EndRingS = 0.2f; // m and s: the end ring's start size and life (to PG3V1EndRingToM)
        public const float PG3V1PulseRingEdge = 0.4f; // HDR: the pulse rings' cyan as they go out
        public const float PG3V1PulseRing2K = 0.7f, PG3V1PulseRing2Gain = 0.6f, PG3V1PulseRing2DelayS = 0.05f; // the pulse's second floor ring: size and brightness against the first, and its delay
        public const float PG3V1PulseStarS = 0.09f, PG3V1PulseStarEdge = 2.0f; // s and HDR: the star at the glove
        public const float PG3V1ParryCoreFromK = 0.7f, PG3V1ParryCoreToK = 1.3f; // x the parry's unit size: the soft core (capped to PG3V1CapSoftDeg)
        public const float PG3V1ParryStarFromK = 1.0f, PG3V1ParryStarToK = 1.35f; // x: the star (capped to PG3V1CapStarDeg)
        public const float PG3V1ParryRingFromK = 0.3f, PG3V1ParryRingToK = 2.3f; // x: the ring (capped to PG3V1CapRingDeg)
        public const float PG3V1ParryCoreEdge = 1.2f, PG3V1ParryStarEdge = 2.4f, PG3V1ParryRingEdge = 0.6f; // HDR: each one's cyan as it goes out
        public const float PG3V1ParryRing2K = 0.65f, PG3V1ParryRing2Gain = 0.7f, PG3V1ParryRing2DelayS = 0.025f; // the parry's second ring: size and brightness against the first, and its delay
    }
}
