// NIGHT17 U3-30.5 (builder X): the crunch's render-only numbers (NIGHT17.md U3-30.5; Research/2026-10-05-overhaul.md
// section 1). Every one is a picture or a time on the Game layer: none touches the sim (the composed fingerprint stays
// EQUAL). The web has none of them: it has no chunks, no eased hit flash, no hit tilt, no per-class shake, no muzzle
// star in the world, no impact frame and no sprint (the web's FX numbers stay in PortExport/config.json: fx.gravity,
// fx.flashShapes, fx.flashFrames, fx.flashSecondFrame, fx.flashSize, quality.presets.*.particleCap, style.multikillWindow).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- GG's switches (NIGHT17.md): CHUNKS ON (a kill breaks the enemy into its own blocks; D-29: nothing stays) ---------
        public const bool CrunchChunksOn = true;

        // --- the kill freeze reads as deliberate (Sakurai: the victim vibrates while both freeze, decaying) ------------------
        // The victim's vibration across the screen (the camera's right and up, so a punch's head-on hit still reads), m:
        public const float CrunchVibAmp = 0.035f;
        // Its rate (Hz, right axis; the up axis runs at CrunchVibUpRatio of it, so the path is a small figure, not a line):
        public const float CrunchVibHz = 34f;
        public const float CrunchVibUpRatio = 0.62f;
        // The up axis' share of the amplitude:
        public const float CrunchVibUpShare = 0.6f;
        // It decays to 0 over the freeze plus this tail (s, frame time); with no freeze, over CrunchVibMin:
        public const float CrunchVibTail = 0.05f;
        public const float CrunchVibMin = 0.1f;
        // Inside a freeze the chunks and the particles drift at this share of their speed on frame time (0 = stopped): the
        // break opens a little while the world holds, so the freeze reads as a moment, not as lag.
        public const float CrunchFreezeDrift = 0.12f;

        // --- the chunks (CHUNKS ON): each part splits into blocks of about this size (m), at most this many a side ------------
        public const float ChunkCell = 0.42f;
        public const int ChunkMaxPerAxis = 2; // round 4 (the reviewer: fewer, bigger blocks): 4 -> 2: the husk 2 x 2 x 2 + the head = 9
        // A block's share of its cell (the gaps read as a break):
        public const float ChunkFill = 0.9f;
        // On the freeze's first frame the blocks already stand apart by this much (m, out from the body's middle):
        public const float ChunkPopGap = 0f; // round 4: 0: the body held together (a silhouette) through the freeze
        // Round 4 (the reviewer: one grammar for every kill): the body held together at CrunchHoldScale through the freeze;
        // a kill from CrunchFarWhiteFrom m or more stays pure white through the hold (a close kill white for 1 frame only: the
        // round 3 white wall); a heavy gun's kill (the THUNDERCLAP) slides the body back CrunchHeavyKnock m along the shot over
        // CrunchHeavySlideTime s of world time after the freeze, then bursts; a wall within ChunkWallProbe m behind the body
        // turns the blocks back (the slide stops CrunchWallKeep m short of it).
        public const float CrunchHoldScale = 1.15f; // round 5: 1.1 -> 1.15 (the r4 reviewer: the kill's silhouette)
        public const float CrunchFarWhiteFrom = 4f;
        public const float CrunchHeavyKnock = 1.5f;
        public const float CrunchHeavySlideTime = 0.12f;
        public const float ChunkWallProbe = 6f;
        public const float CrunchWallKeep = 0.5f;
        // The throw: along the shot (m/s, x the overkill share CrunchOverkillMin..Max of damage / max HP), out from the middle,
        // and up, each x a random 0.6..1.2:
        public const float ChunkShotSpeed = 5.5f;
        public const float ChunkSpreadSpeed = 3.2f;
        public const float ChunkUpSpeed = 3.0f;
        public const float CrunchOverkillMin = 0.9f, CrunchOverkillMax = 2.2f; // round 3: the floor 0.6 -> 0.9 (a finishing tap still throws)
        // A punch kill throws harder (x ChunkPunchMult, up and across the screen: below), a headshot pops the head block up:
        public const float ChunkPunchMult = 0.6f; // round 5: x ChunkThrowSpeed 13 (was 0.77 x 10): the punch's 7.7 m/s kept
        // Round 2 (the r2 kill strip: the blocks stayed in the silhouette; along the shot is depth): the throw. Each block
        // flies in a cone ChunkConeDeg wide (full angle) around the axis (the shot; a punch kill's tipped ChunkPunchSide x the
        // camera's right and ChunkPunchLift up: up and across the screen away from the left fist), bent out from the body's
        // middle by ChunkOutBias, at ChunkThrowSpeed m/s x the overkill share x a random 0.7..1.3 (a punch x ChunkPunchMult),
        // plus ChunkUpMin..Max m/s up (a punch x ChunkPunchUpMult). ChunkShotSpeed / ChunkSpreadSpeed / ChunkUpSpeed (above)
        // remain the shed block's throw.
        public const float ChunkConeDeg = 70f; // round 5: +-35 deg (the r4 reviewer)
        public const float ChunkOutBias = 0.6f;
        public const float ChunkThrowSpeed = 13f; // round 5: x 0.8..1.2: 10-16 m/s at any gun kill (the r4 reviewer: they swelled in place at 10)
        // Round 5: the overkill adds at most this (x sqrt(overkill), from 1); gravity from ChunkGravityDelay s after the release
        // (a straight throw first); a wall behind the body turns a block back x ChunkWallBounce (they ricochet into view).
        public const float ChunkOverkillSpeedMax = 1.15f;
        public const float ChunkGravityDelay = 0.08f;
        public const float ChunkWallBounce = 0.6f;
        public const float ChunkUpMin = 2f, ChunkUpMax = 4f;
        public const float ChunkPunchSide = 0.9f, ChunkPunchLift = 0.6f, ChunkPunchUpMult = 1.0f; // round 3: up x1.4 -> x1.0 (the axis already lifts)
        // Round 3 (the reviewer: 2-3 big chunks travelling 3-5 m, one bounce): ChunkHeroes blocks of a part (with more blocks
        // than that) are heroes: x ChunkHeroSize, x ChunkHeroSpeed, x ChunkHeroUp lift (at fx.gravity 18: about 3-5 m). The
        // bounce (ChunkBounce 0.38, rest under ChunkRestSpeed) leaves one visible bounce.
        public const int ChunkHeroes = 3;
        public const float ChunkHeroSize = 1.35f, ChunkHeroSpeed = 1.5f, ChunkHeroUp = 1.3f;
        // The spin, deg/s, a random ChunkSpinMinDeg..MaxDeg (round 3, the reviewer: 360-720 deg/s; round 2 had 515-1550);
        // REDUCED MOTION halves it (NIGHT17.md rule 8):
        public const float ChunkSpinMinDeg = 540f, ChunkSpinMaxDeg = 900f; // round 5: 360-720 -> 540-900 (the r4 reviewer)
        public const float ChunkReducedSpin = 0.5f;
        // The floor (simple maths on the world's ray, no PhysX): the bounce keeps this share of the fall speed, the slide
        // this share of the ground speed per bounce; under ChunkRestSpeed m/s a block lies still. A block probes the
        // world under it (a ray from ChunkProbeUp m above its floor guess) when it reaches that guess.
        public const float ChunkBounce = 0.38f;
        public const float ChunkFriction = 0.55f;
        public const float ChunkRestSpeed = 0.8f;
        public const float ChunkProbeUp = 0.6f;
        // A floor more than this far under the guess is a drop: the block falls on to it.
        public const float ChunkProbeSlack = 0.35f;
        // The life (s, x a random 0.75..1; nothing stays past 1.5 s: D-29) and the shrink over its last share:
        public const float ChunkLife = 1.45f;
        public const float ChunkShrinkShare = 0.3f;
        // Round 3 (the reviewer: a close kill read as a flat white wall through its whole freeze): white for the first
        // CrunchWhiteFrames drawn frames only, then back to its own colour over CrunchWhiteFade s of frame time (inside the
        // freeze too); a hot rim (SKYGRIND/Enemy's rim x CrunchHotRim) through the freeze, cooling over CrunchHotRimTime s of
        // world time after it. The white is HDR x CrunchWhiteHdr, so the bloom catches it:
        public const int CrunchWhiteFrames = 1; // round 4: 2 -> 1 (the r3 punch kill at 1.4 m read white for 4 strip frames)
        public const float CrunchWhiteFade = 0.03f; // round 4: 0.06 -> 0.03 (hot orange by the 2nd-3rd frame)
        public const float CrunchHotRim = 3f;
        public const float CrunchHotRimTime = 0.25f;
        public const float CrunchWhiteHdr = 2.4f; // round 5: 1.6 -> 2.4 (white-hot)
        // Round 5 (the r4 reviewer: a gun kill's frame 1 looked like a hit's): a gun kill's blocks are white-hot for this many
        // drawn frames (a punch kill keeps CrunchWhiteFrames), and its radial burst (SpawnFlashes.KillBurst): flat streaks
        // BurstBodies x the body's height across (never more than BurstMaxAngle x the distance: a close kill), x BurstHdr white,
        // 2 frames x FLASH INTENSITY; the texture BurstTexture px, BurstSpikes streaks, BurstSharpness thin.
        public const int CrunchWhiteFramesGun = 2;
        // Round 6 (the lead's Editor check: at 15-20 m the chunks were 2-4 px specks, the burst and the head ring unseen): a
        // block keeps at least ChunkMinScreen of the screen's height (scaled up to x ChunkMinScreenMax), the kill burst at least
        // BurstMinScreen, the head ring's end size HeadRingMinScreen; a near kill already exceeds them (unchanged).
        public const float ChunkMinScreen = 0.02f, ChunkMinScreenMax = 4f; // 2 %: at 1 % a 0.36 m block at 18 m (1.6 %) was unchanged
        public const float BurstMinScreen = 0.22f;
        public const float HeadRingMinScreen = 0.08f;
        // Round 5 (the r4 reviewer: the headshot had no beat of its own): on the impact frame the head block pops HeadPopUp m/s
        // up (+ HeadPopAlong along the shot; never held, it flies on frame time inside the freeze, gravity from
        // ChunkGravityDelay), and a white ring at the head grows HeadRingStart -> HeadRingEnd m over HeadRingTime s (ease-out,
        // fading, x HeadRingHdr, facing the camera, frame time).
        public const float HeadPopUp = 8f, HeadPopAlong = 1.5f;
        public const double HeadRingStart = 0.15, HeadRingEnd = 0.9, HeadRingTime = 0.15, HeadRingHdr = 3.0;
        public const float BurstBodies = 2f, BurstMaxAngle = 1.2f, BurstHdr = 2.5f;
        public const int BurstTexture = 128, BurstSpikes = 14;
        public const double BurstSharpness = 30;
        // The pool (a Graphics.RenderMesh per block on SKYGRIND/Enemy since round 1) and the preset's cap: this share of the preset's
        // particleCap (LOW 160 -> 96, MEDIUM / HIGH 400 -> 240), the oldest recycled first.
        public const int ChunkPool = 256;
        public const float ChunkCapShare = 0.6f;
        // The draw box's pad (m): a block at the cloud's edge is never culled.
        public const float ChunkBoundsPad = 1f;
        // Round 1: the particles draw per colour (Particles: one CubeBatch mesh and one neon copy per colour a burst uses:
        // the enemy, the gun families, the traps, the sparks); a colour past this many shares the last batch.
        public const int ParticleBuckets = 12;

        // --- the hit on a living enemy (every damaging hit) ----------------------------------------------------------------
        // The flash: full white at once, held CrunchHitFlashHold s, eased back over CrunchHitFlashFade s (world time).
        public const float CrunchHitFlashHold = 0.03f;
        public const float CrunchHitFlashFade = 0.1f;
        // The tilt / pop away from the shot (60-100 ms in all): the top tips away by CrunchTiltDeg and the body is pushed
        // CrunchPush m, in over CrunchTiltIn s, back over CrunchTiltOut s; a melee hit x CrunchTiltMelee.
        public const float CrunchTiltDeg = 11f;
        public const float CrunchPush = 0.09f;
        public const float CrunchTiltIn = 0.025f;
        public const float CrunchTiltOut = 0.07f;
        public const float CrunchTiltMelee = 1.5f;
        // Round 1 (the reviewer: the blast's survivors didn't react; a parried attacker should be knocked back): a heavy gun's
        // hit (feel.punch.weapons.<w>.cls heavy: the THUNDERCLAP's pellets, the COMET, the LONGWAVE) knocks the enemy back
        // CrunchKnockHeavy m, a melee hit CrunchKnockMelee m, a parry's attacker CrunchKnockParry (PERFECT: CrunchKnockPerfect);
        // in over CrunchKnockIn s, back over CrunchKnockOut s, frame time (it plays through a freeze). View only.
        public const float CrunchKnockHeavy = 0.38f;
        public const float CrunchKnockMelee = 0.22f;
        public const float CrunchKnockParry = 0.3f;
        public const float CrunchKnockPerfect = 0.45f;
        public const float CrunchKnockIn = 0.035f;
        public const float CrunchKnockOut = 0.22f;
        // Round 1 (greyscale: the husks had the walls' value): the enemies' colour x this (HDR, SKYGRIND/Enemy is unlit: the
        // hue stays config.colors.enemy's) and their rim x CrunchRimBoost, so an enemy is the brightest warm value in a frame.
        public const float CrunchEnemyValue = 1.35f;
        public const float CrunchRimBoost = 1.5f;

        // --- damage shows before death (DOOM Eternal's destructible demons; view only) --------------------------------------
        // Under this HP share an enemy sheds one block (once), thrown away from the shot, and keeps a dark notch where it
        // was (INK), CrunchShedSize m a side; enemies tracked at once (the oldest record reused):
        public const float CrunchShedAt = 0.5f;
        public const float CrunchShedSize = 0.24f;
        // Round 5: the shed block flies off to the side away from the shot (x ShedAway of the shot's axis plus the side, sides
        // alternating) and up, never back at the player (n17-r4: a red-orange block flying at the camera read as an enemy shot).
        public const float ShedAway = 0.6f;
        public const int CrunchShedSlots = 48;

        // --- the rotational trauma shake (Eiserloh: trauma^2 x noise; first person: rotation only, never a translation) ------
        // The most it turns (deg), on top of the web's feel.trauma shake (whose kill / fire amounts square to under 0.05 deg):
        public const double CrunchShakePitchDeg = 2.0, CrunchShakeYawDeg = 2.0, CrunchShakeRollDeg = 3.0;
        public const double CrunchShakeExponent = 2;
        // Per weapon class (feel.punch.weapons.<w>.cls) and moment: the trauma added, the noise rate (Hz) and the fall (/s).
        // Pistol-class light, shotgun-class heavy (slow, long), the punch sharp (fast, short). Paired with builder V's per-gun
        // viewmodel springs (the gun carries the kick: the KESTREL 13 deg, the THUNDERCLAP 9 deg + 17 cm back, the STITCHER
        // 1.3 deg at 15 Hz): the light class stays near nothing and falls fast, so a light gun's every-shot shake never blurs
        // its chatter; a gun's onset pitches up only (ShakeOnsetPitch: with the recoil), never rolls against the viewmodel's roll.
        public const double ShakeFireLight = 0.06, ShakeFireMedium = 0.22, ShakeFireHeavy = 0.42;
        public const double ShakeFireLightHz = 30, ShakeFireMediumHz = 26, ShakeFireHeavyHz = 17;
        public const double ShakeFireLightFall = 6.0, ShakeFireMediumFall = 3.4, ShakeFireHeavyFall = 2.2;
        public const double ShakeKill = 0.42, ShakeKillHz = 24, ShakeKillFall = 2.6;
        public const double ShakeHeadshotKill = 0.5, ShakeHeadshotKillHz = 26, ShakeHeadshotKillFall = 2.6;
        public const double ShakePunchHit = 0.4, ShakePunchKill = 0.66, ShakePunchHz = 40, ShakePunchFall = 4.2;
        public const double ShakeParry = 0.6, ShakeParryHz = 36, ShakeParryFall = 3.0;
        public const double ShakeSlam = 0.5, ShakeSlamHz = 18, ShakeSlamFall = 2.4;
        // The onset: the first frame turns at once (pitch up, roll to an alternating side), decaying over this time (s), so
        // the shake lands on the freeze's first frame and not one noise period later:
        public const double ShakeOnsetTime = 0.035;
        public const double ShakeOnsetPitch = 0.55, ShakeOnsetRoll = 0.8;

        // --- the muzzle star (combat/effects.js :35-50's flash shape per gun, config fx.flashShapes) -------------------------
        // The star's texture side (px, made in code at load per shape) and its world size x fx.flashSize x the shape's scale:
        public const int StarTexture = 64;
        public const float StarScale = 0.8f; // r1: at 1.25 the star covered a 7 m kill on the freeze's first frames
        // Its brightness (HDR, x FLASH INTENSITY) and its distance in front of the camera (m) when no muzzle is found:
        public const float StarHdr = 2.2f;

        // --- the impact frame (perfect parries and 3+ multikills only): one high-key frame, never an inverted palette --------
        // (inverting maps cyan toward red: CLAUDE.md rule 8). White over the world at this alpha (x FLASH INTENSITY), the
        // second frame at CrunchFrameSecond of it; its canvas over the impact flash (6), under the HUD (10).
        public const float CrunchFrameAlpha = 0.15f; // round 2: at most 15 % (round 1: 25 %, 0.55 before), under the HUD; round 3: unused (the crush)
        // Round 3 (the reviewer: "never lighten the whole screen"): the crush frame: the post exposure drops CrushEv (EV, x
        // FLASH INTENSITY) for CrushHold s, eased back over CrushFade s (30-60 ms in all), real time; the white blocks rise by
        // 2^-EV so the bloom keeps them white-hot.
        public const double CrushEv = -2.0;
        public const double CrushHold = 0.025;
        public const double CrushFade = 0.035;
        public const float CrunchFrameSecond = 0.35f;
        // Round 1: a PERFECT PARRY's cyan ring burst from the contact point: 0 -> CrunchRingPx px at 1080p over CrunchRingTime s
        // (ease-out), fading as (1 - t)^2; its sprite CrunchRingTexture px a side, the band CrunchRingWidth of the radius.
        public const float CrunchRingPx = 600f;
        // Round 4 (the reviewer: about twice the body's size): the ring's final size is the parried body's height on screen
        // (CrunchRingBodyM m at its distance) x CrunchRingBodies, never under CrunchRingPx.
        public const float CrunchRingBodyM = 1.8f, CrunchRingBodies = 2f;
        // Round 4: a reflected glob's trail (TrailRenderer): s it lasts, its width x the glob's, colors.usable x this (additive).
        public const float GlobTrailTime = 0.25f, GlobTrailWidth = 0.8f, GlobTrailHdr = 1.6f;
        public const double CrunchRingTime = 0.12;
        public const int CrunchRingTexture = 256;
        public const float CrunchRingWidth = 0.06f;
        public const int CrunchFrameSortingOrder = 7;
        // Kills inside style.multikillWindow (config) make a multikill; the impact frame fires at every CrunchMultiStep-th.
        public const int CrunchMultiStep = 3;

        // --- U3-30.10: an enemy's arrival (SpawnFlashes, on the web's enemySpawn; the web draws the beam, not its landing) ----
        // The DANGER ground ring: from SpawnRingStart to SpawnRingEnd m (radius) over SpawnRingTime s, ease-out, fading
        // (1 - t)^2 from SpawnRingHdr (x colors.enemy, additive), SpawnRingLift m over the feet; its band SpawnRingWidth of the
        // radius, SpawnRingSegments around. The core: a soft DANGER disc SpawnCoreSize m wide at SpawnCoreHeight m, x
        // SpawnCoreHdr, 2 frames (full, then half) x FLASH INTENSITY; its texture SpawnCoreTexture px. SpawnFlashPool at once.
        public const float SpawnRingStart = 0.3f, SpawnRingEnd = 1.8f;
        public const double SpawnRingTime = 0.35;
        public const float SpawnRingHdr = 2.2f, SpawnRingLift = 0.03f, SpawnRingWidth = 0.18f;
        public const int SpawnRingSegments = 40;
        public const float SpawnCoreSize = 1.4f, SpawnCoreHeight = 0.9f, SpawnCoreHdr = 3.0f;
        public const int SpawnCoreTexture = 64;
        public const int SpawnFlashPool = 12;

        // --- round 2: the warm-up (the first kill's 122 ms hitch) and the log ------------------------------------------------
        // Each draw path of the kill draws once, tiny, on the first frame with a camera: CrunchWarmDistance m ahead at
        // CrunchWarmScale (sub-pixel); the warm line lives CrunchWarmLineLife s of world time (TITLE's is 0: it waits, invisible,
        // until a run clears the lines). The [crunch] lines are written CrunchLogDelayFrames after the kill.
        public const float CrunchWarmDistance = 1f;
        public const float CrunchWarmScale = 0.001f;
        public const float CrunchWarmLineLife = 0.05f;
        public const int CrunchLogDelayFrames = 30;

        // --- round 3: the slide's own cue (the reviewer: the slide had none; MovementFx.SpawnSlideCue, CameraFeel) ---------
        // The widen past the speed cap (deg) and its ease (/s):
        public const double SlideFovDeg = 2.5;
        public const double SlideFovEase = 8.0;
        // The low streaks: per s at full slide speed (x how far past a walk), their brightness (x colors.usable), their place
        // (m: above the floor, ahead, beside the path), length (m), life (s), width (m):
        public const double SlideLineRate = 30;
        public const float SlideLineBrightness = 1.0f; // round 5: 0.45 -> 1.0
        // Round 5 (the r4 reviewer saw no streaks): further ahead (in the view: the eye is 0.9 m up while sliding, so the floor
        // shows from ~1.2 m), wider and longer.
        public const float SlideLineHeightMin = 0.04f, SlideLineHeightMax = 0.3f;
        public const float SlideLineAheadMin = 2.5f, SlideLineAheadMax = 6f;
        public const float SlideLineSideMin = 0.4f, SlideLineSideMax = 1.6f;
        public const float SlideLineLength = 1.4f, SlideLineLife = 0.25f, SlideLineWidth = 0.03f;
        // The sparks thrown up from the floor ahead, in view: per s at full slide speed, ahead (m), beside (m, either way):
        public const double SlideSparkRate = 26;
        // Round 5 (the r4 reviewer saw no sparks: at 0.8-1.6 m ahead they were under the screen's bottom edge): 2-4 m ahead,
        // x SlideSparkScale cubes (fx.size 0.12 x 1.2: 14 cm; fx.sparkSize 0.4 was 5 cm), SlideSparkSpeed m/s, SlideSparkLife s.
        public const float SlideSparkAheadMin = 2f, SlideSparkAheadMax = 4f, SlideSparkSide = 0.8f;
        public const float SlideSparkScale = 1.2f, SlideSparkSpeed = 3.5f, SlideSparkLife = 0.35f;
        // Round 4 (the reviewer: the camera should drop ~0.5 m in 80 ms and roll ~4 deg; the cue from the first frame): on top
        // of the web's eye ease (0.7 m over 0.1 s) a dip of SlideDip m on the slide's start (in SlideDipIn s, back over
        // SlideDipOut s) and SlideRollDeg more roll (the web's slide tilt is 3 deg) eased in at SlideRollEase /s; a first
        // SlideBurstLines lines and SlideBurstSparks sparks on the start, and the cue's rate never under SlideCueFloor of full.
        public const double SlideDip = 0.15, SlideDipIn = 0.04, SlideDipOut = 0.3;
        public const double SlideRollDeg = 1.5, SlideRollEase = 30;
        public const double SlideBurstLines = 8, SlideBurstSparks = 10; // round 5: 6 / 6 -> 8 / 10
        public const double SlideCueFloor = 0.35;

        // --- U3-30.10: a death signature per gun (CrunchMath.SignatureOf / ParamsOf; the KillEvent's weapon) ----------------
        // KESTREL (revolver; every gun without its own): the clean punch-through: the block nearest the shot's line flies
        // along it x SigLeadSpeed (and hero-sized). THUNDERCLAP: the shove back (CrunchHeavyKnock over CrunchHeavySlideTime)
        // then the burst in a SigShoveConeDeg cone x SigShoveSpeed. STITCHER: SigNailKeep of the blocks as chips x SigNailChip,
        // laid along the nail's line (SigNailLine m through the body, jittered SigNailJitter m) for SigNailPin s after the
        // freeze, then dropping (a drift of SigNailDrift m/s); its ring x SigNailRing. COMET: thrown out from the blast's point
        // in every direction x SigBlastSpeed, x SigBlastUp lift, its floor ring x SigBlastRing. LONGWAVE: the body splits on the
        // beam's plane: the halves slide SigSplitGap m apart over SigSplitTime s, a white-hot seam SigSeamWidth m thin for
        // SigSeamFrames drawn frames, then each half thrown across (SigSplitConeDeg cone, SigSplitAlong of the beam). PULSE
        // GAUNTLET / punch: the up-and-across throw (ChunkPunch*).
        public const float SigLeadSpeed = 1.6f;
        public const float SigShoveConeDeg = 100f, SigShoveSpeed = 1.1f;
        public const float SigNailKeep = 0.6f, SigNailChip = 0.55f, SigNailLine = 1.0f, SigNailJitter = 0.08f, SigNailPin = 0.18f, SigNailDrift = 0.6f, SigNailRing = 0.6f;
        public const float SigBlastSpeed = 1.3f, SigBlastUp = 1.5f, SigBlastRing = 1.8f;
        public const float SigSplitGap = 0.35f, SigSplitTime = 0.1f, SigSplitConeDeg = 40f, SigSplitAlong = 0.5f, SigSeamWidth = 0.03f;
        public const int SigSeamFrames = 2;

        // --- the sprint cue (StaminaHud.Info.sprinting; weaker and steadier than the boost's 10 deg kick) ---------------------
        // Round 4 (the reviewer: the sprint read as nothing in a strip): 3 -> 7 deg, eased in about 150 ms (still under the
        // boost's 10 deg kick in 50 ms):
        // Round 5 (the r4 reviewer: the boost's kick then the sprint's widen: +10 within 1-2 frames, easing to +5 over 200 ms):
        public const double SprintFovDeg = 5.0;
        public const double SprintFovEase = 15.0; // per s (exponential): 95 % in 200 ms
        // The speed lines (world-space additive lines streaming past): per s, the ring around the view (m), how far ahead (m),
        // their length (m), life (s), brightness (x colors.usable) and width (m).
        // Round 4: edge-only (the screen's outer 15 %: SprintLineEdgeMin..Max of the way out on the view's ellipse), more and
        // brighter (26 -> 40 a second, x0.32 -> x0.5):
        public const double SprintLineRate = 40;
        public const float SprintLineEdgeMin = 0.85f, SprintLineEdgeMax = 1.0f;
        public const float SprintLineAheadMin = 2.5f, SprintLineAheadMax = 6f;
        // Round 5 (the r4 reviewer: invisible in the strip: 2-3 px, dim): pale cyan (colors.usable toward white by SprintLinePale),
        // brighter (HDR x1.4), wider (0.03 m), longer (2 m), living 0.3 s.
        public const float SprintLineLength = 2f, SprintLineLife = 0.3f, SprintLineBrightness = 1.4f, SprintLineWidth = 0.03f;
        public const float SprintLinePale = 0.45f;
        // The sprint's steady chromatic share (post.js's chroma units; the dash's kick is post.chromaDash) and its ease (/s):
        public const double SprintChroma = 0; // round 1: 0.12 x ChromaScale 36 clamped to full chroma, a MAGENTA fringe (rule 8): off
        public const double SprintChromaEase = 5.0;

        // --- NIGHT18 U3-31.3 (builder X): the kill at every range (NIGHT18.md M1-M3; FeelAB on). Each number sits beside its
        // NIGHT17 one (...N17), picked with UnityTuning.Pick(FeelAB.On, X, XN17); the off side is NIGHT17's look exactly. ---------
        // M1 / M2: the kill's own frame 1 (the first rendered frame after the killing step) and the next: the radial burst is
        // KillBurstBodies x the body's height across, never wider than KillBurstMaxAngle x the distance (a close kill's), never
        // under KillBurstMinScreen of the screen's height (a far kill's floor), x HordeScale on a frame with several kills.
        public const float KillBurstBodies = 2.4f, KillBurstBodiesN17 = BurstBodies;
        public const float KillBurstMaxAngle = 0.55f, KillBurstMaxAngleN17 = BurstMaxAngle; // 1.2: a punch kill's burst filled 80 % of the screen
        public const float KillBurstMinScreen = 0.14f, KillBurstMinScreenN17 = BurstMinScreen;
        // Its brightness (HDR white, additive, x FLASH INTENSITY), its frames at full and its fading frames (NIGHT17: 1 and 1,
        // the second at half: a 2-frame burst fell between a strip's frames), and where it draws: KillBurstDepthShare of the
        // way from the eye to the body's middle, its size scaled to match (the same on screen), so neither the body nor the
        // floor in front of its feet hides any of it (NIGHT17's sat at the middle: the body hid its core, the floor its foot):
        // Round 2 (b3's blind reviewers, 1 of 8, both "too much": the filled core's bloom blurred the body into a white blob;
        // NIGHT17's crisp silhouette and thin streaks read better): HDR 3.2 -> 2.0, no core.
        public const float KillBurstHdr = 2.0f, KillBurstHdrN17 = BurstHdr;
        public const int KillBurstFullFrames = 2, KillBurstFullFramesN17 = 1;
        public const int KillBurstFadeFrames = 2, KillBurstFadeFramesN17 = 1;
        public const float KillBurstDepthShare = 0.5f;
        // Tonight's burst texture (round 2): KillBurstSpikes thin, hard streaks (|cos|^KillBurstSharpness) OUTSIDE the
        // silhouette (they fade in from KillBurstInner to KillBurstInner + KillBurstInnerSoft of the radius: nothing over the
        // body) falling to black at the edge, KillBurstTexture px (sharp at 12 m's ~260 px); NIGHT17's streaks stay theirs.
        public const int KillBurstSpikes = 12, KillBurstTexture = 256;
        public const double KillBurstSharpness = 20, KillBurstInner = 0.3, KillBurstInnerSoft = 0.1;
        public const double KillBurstRayGain = 1.6; // x the rays: full white to ~40 % of the way out, then falling
        // The share of the burst's quad that shows (the probe's rect, M2): tonight's rays reach ~90 %, NIGHT17's streaks ~85 %.
        public const float KillBurstVisibleShare = 0.9f, KillBurstVisibleShareN17 = 0.85f;
        // The crush disc (tonight only): one dark disc over the frame's kills (the world inside it x (1 - strength x the mask),
        // a multiply: no hue, no lightening), on the burst's frames x FLASH INTENSITY, through the flash gate (refused: no
        // disc, and the burst at NIGHT17's strength); none while PostFx's crush already darkens the whole frame (a headshot,
        // punch or multikill). Round 1 (b3 M1 2.60: the soft disc behind the body hid under the burst's bloom): it sits in front
        // of the kills (at the nearest burst's depth; the white silhouette and the bursts draw over it), hard-edged (full to
        // KillDiscEdge of its radius), darker, KillDiscBodies x the body's height across on screen (kill12: ~400 px, the M1
        // window), KillDiscMinScreen..KillDiscMaxScreen of the screen's height. Round 2 (the reviewers' "too much"): x 0.6, 3.0
        // bodies (kill12 ~320 px), 22-42 % of the screen's height. Round 3 (b6 M1 2.82, 2900 px short of 3.0; the disc
        // showed only on the bright floor: the purple walls stayed under the delta): x 0.7, 3.2 bodies (~340 px).
        public const float KillDiscStrength = 0.7f, KillDiscBodies = 3.2f, KillDiscMinScreen = 0.22f, KillDiscMaxScreen = 0.42f;
        public const double KillDiscEdge = 0.85;
        public const int KillDiscTexture = 128;
        // The share of the disc's radius that visibly darkens (full to 85 %, gone at the rim): the probe's rect.
        public const float KillDiscVisibleShare = 0.95f;
        // Round 1: the held body over the disc on the burst's frames: an additive white copy of each block (KillSilhouetteHdr,
        // x the burst's strength, x KillSilhouetteGrow so it covers its block), so the silhouette stays white on the dark.
        // Round 2: crisp, not a bloom: the opaque white x KillWhiteHdr 1.2 plus this copy x 1.2: 1.7 over the disc (0.6), 2.4
        // (NIGHT17's) without it.
        public const float KillSilhouetteHdr = 1.2f, KillSilhouetteGrow = 1.03f;
        // The silhouette white through the freeze: the held body x KillHoldScale, its white x KillWhiteHdr, white through the
        // whole hold from KillFarWhiteFrom m (nearer: KillWhiteFramesGun drawn frames, then its colour). Round 2: 3.6 -> 1.2 (its
        // bloom blurred the body; hard edges read).
        public const float KillHoldScale = 1.25f, KillHoldScaleN17 = CrunchHoldScale;
        public const float KillWhiteHdr = 1.2f, KillWhiteHdrN17 = CrunchWhiteHdr;
        public const float KillFarWhiteFrom = 3f, KillFarWhiteFromN17 = CrunchFarWhiteFrom; // the horde stands 3.6-4.6 m away
        public const int KillWhiteFramesGun = 3, KillWhiteFramesGunN17 = CrunchWhiteFramesGun;
        // M3: the chunks thrown, not swelling. NIGHT17 threw along the shot (depth: a throw straight away from the camera barely
        // moves on screen). Tonight a gun kill's axis is KillThrowAlong along the shot's level line + KillThrowAcross across the
        // screen (the camera's right x the kill's side, alternating per kill frame; one side for every kill of a frame, so a
        // horde's cloud moves as one) + KillThrowLift up, normalised; at KillThrowSpeed m/s x 0.8..1.2, in a KillConeDeg cone,
        // bent out from the body's middle by KillOutBias (+ ChunkUpMin..Max up, as before). The lead hero flies the axis too.
        // Round 1 (b3: kill12's cloud reached 2.03 body heights at +250 ms but left the reel's crop; the horde's 1.22): a pop,
        // a hang, then the fall: 36 m/s with a drag of KillThrowDrag /s fading over KillThrowDragTime s (CrunchMath.ThrowOffset),
        // less along the shot (depth shrinks the cloud on screen): 1.8 body heights at +250 ms, 2.0 at +400 ms (kill12). The
        // split, the blast, the nails and the punch keep NIGHT17's speed (ChunkThrowSpeed) and no drag. Round 2 (the reviewers:
        // "chunks have left the crop"; keep them around the target): 40 m/s against 14 /s: kill12 1.74 body heights at +250 ms,
        // 1.85 at +400 ms (the 480 px crop's edge is 2.24); the THUNDERCLAP's own x KillShoveSpeed carries the horde (1.9).
        public const float KillThrowAlong = 0.15f, KillThrowAcross = 0.85f, KillThrowLift = 0.5f;
        public const float KillThrowSpeed = 40f, KillThrowSpeedN17 = ChunkThrowSpeed;
        public const float KillThrowDrag = 14f, KillThrowDragTime = 0.3f;
        // The side: toward the open side when a wall stands within KillSideRoom m on one side of the body (the b3 horde threw
        // into the pillar beside it and its blocks bounced back), else KillSidePreferred: round 2, the left (the points pop
        // rises up and right of a kill: b3's reviewer saw the chunks cross the +75); one side for every kill of a frame.
        public const float KillSideRoom = 4f;
        public const float KillSidePreferred = -1f;
        public const float KillConeDeg = 40f, KillConeDegN17 = ChunkConeDeg;
        public const float KillOutBias = 0.25f, KillOutBiasN17 = ChunkOutBias;
        // The THUNDERCLAP's shove (DeathSig.Shove): NIGHT17 slid the held body back CrunchHeavyKnock m over CrunchHeavySlideTime s
        // of world time after the freeze, so its blocks flew 120 ms late; tonight the slide plays inside the freeze (frame time)
        // and the blocks fly at its end (KillShoveHold 0), in a KillShoveConeDeg cone.
        public const float KillShoveHold = 0f, KillShoveHoldN17 = CrunchHeavySlideTime;
        // Round 1: the shove's slide KillShoveSlide m (NIGHT17 CrunchHeavyKnock 1.5 m: at 4 m the cloud started 1.5 m deeper and
        // crossed less of the screen)
        public const float KillShoveSlide = 0.4f, KillShoveSlideN17 = CrunchHeavyKnock;
        public const float KillShoveConeDeg = 60f, KillShoveConeDegN17 = SigShoveConeDeg;
        // Round 2: the shove's blocks x KillShoveSpeed (NIGHT17 SigShoveSpeed 1.1): the horde's cloud at 4 m crosses 1.9 body
        // heights at +250 ms (the reel's 480 px crop holds under 1 body height there: the horde leaves it either way)
        public const float KillShoveSpeed = 1.3f, KillShoveSpeedN17 = SigShoveSpeed;
        // Round 3 (b6 horde5 1.06 body heights: the cloud rose 1.06 and its sideways travel cancelled against the pack's
        // offset from the target husk): the THUNDERCLAP's blocks erupt UP: KillShoveAcross across, KillShoveLift up (along as
        // KillThrowAlong): modelled 1.98 body heights straight up at +250 ms at 6.2 m (b6 modelled 1.10, measured 1.06),
        // whatever the pack's layout.
        public const float KillShoveAcross = 0.3f, KillShoveLift = 0.95f;
        // Round 4 (r3b: horde5 "small-cube confetti" against NIGHT17's "big slabs"; kill6 "only sparks"; the punch kill "a
        // pale-orange wash fills the crop"):
        // - the COMET's blast kill breaks into big slabs (KillBlastCellsXZ x KillBlastCellsY x KillBlastCellsXZ a body, not
        //   2 x 2 x 2) erupting at KillThrowSpeed x KillBlastSpeed (NIGHT17: ChunkThrowSpeed x SigBlastSpeed), modelled 2.1 body
        //   heights of rise at +250 ms at 7 m (the Editor read 1.88 where the model said 2.3: still over 1.5);
        // - a near kill throws slower: x distance / KillThrowFullFrom, at least KillThrowNearShare (kill6 x 0.5: ~0.9 body
        //   heights at +400 ms, inside the 480 px crop's 1.1; kill12 and beyond x 1, M3 unchanged);
        // - under KillNearFrom m (a punch kill) the pieces are x KillNearBlockScale and their colour x KillNearValue (less
        //   bloom), and no white silhouette copy.
        public const int KillBlastCellsXZ = 1, KillBlastCellsY = 2;
        public const float KillBlastSpeed = 1.2f, KillBlastSpeedN17 = SigBlastSpeed;
        public const float KillThrowFullFrom = 12f, KillThrowNearShare = 0.3f;
        public const float KillNearFrom = 3f, KillNearBlockScale = 0.6f, KillNearValue = 0.7f;
        // Horde scaling: a frame with N kills draws each burst x (1 + HordeBurstPerKill x (N - 1)), at most HordeBurstMax
        // (NIGHT17: x 1); the freeze grows by design.impact.freeze.killPerExtra (ImpactRules).
        public const float HordeBurstPerKill = 0.08f, HordeBurstMax = 1.35f;

        // --- NIGHT18 U3-31.9 backlog 3 (builder X): the spawn ring in DANGER (a role fix; FeelAB on) ---------------------------
        // NIGHT17-RETRO: the arrival's ring read near white. Its DANGER x 2.2 (the core x 3.0) is added over the world: the red
        // channel passed the bloom threshold (post.bloomThreshold 1), clipped and bloomed white. Tonight both stay at or under
        // it (CrunchMath.AdditivePeak), so the tone map keeps the red-orange, and the band is wider for the dimmer ring. The
        // kill's floor ring (the same draw) follows.
        public const float DangerRingHdr = 0.95f, DangerRingHdrN17 = SpawnRingHdr;
        public const float DangerCoreHdr = 1.0f, DangerCoreHdrN17 = SpawnCoreHdr;
        public const float DangerRingWidth = 0.26f, DangerRingWidthN17 = SpawnRingWidth;

        // --- NIGHT18 U3-31.9 backlog 4 (builder X): the low-HP vignette pulse (PostFx; FeelAB on) ------------------------------
        // Under post.lowHpAt of the HP the post vignette (post.lowHpDarkness, steady before) beats: a beat and a smaller echo
        // (CrunchMath.HeartPulse) at LowHpPulseRestHz at the line, quickening to LowHpPulseLowHz at 0 HP (two bumps a beat: at
        // most 3 a second), adding LowHpPulseAmp x the hurt share to its intensity at the beat's top, x FLASH INTENSITY; steady
        // with REDUCED MOTION. Its colour leans toward DANGER by LowHpPulseTint x the hurt share (NIGHT17: black). Written in
        // LowHpPulseStep steps, so the volume refreshes only as often as it shows.
        public const double LowHpPulseRestHz = 0.9, LowHpPulseLowHz = 1.4;
        public const float LowHpPulseAmp = 0.14f, LowHpPulseAmpN17 = 0f;
        public const float LowHpPulseTint = 0.45f, LowHpPulseTintN17 = 0f;
        public const float LowHpPulseStep = 0.005f;
        // The beat's shape (CrunchMath.HeartPulse): the beat's top at LowHpBeatAt of the beat, LowHpBeatWidth wide (a gaussian's
        // sigma); the echo at LowHpEchoAt, LowHpEchoWidth wide, LowHpEchoShare as high.
        public const double LowHpBeatAt = 0.15, LowHpBeatWidth = 0.045, LowHpEchoAt = 0.35, LowHpEchoWidth = 0.05, LowHpEchoShare = 0.6;
    }
}
