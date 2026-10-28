// NIGHT20 U3-34.7 (builder V): the AIR streaks' render numbers (Game/SpeedLines20.cs; CLAUDE.md rule 5: Unity-only
// numbers, each with a comment). Why the web has none of them: the web's speed cue is a screen-locked streak quad
// (player.js speedLines) and a CSS edge tint; tonight's world-space streaks are port-only, designed in
// Research/2026-10-06-speed-lines.md section 3 (KvantStream's head-to-tail rule, MikeFrom's tunnel ring, both rewritten in
// C#). Every number is a starting value (inferred) for the reel's M8 v2 rows L1-L6; nothing here touches the sim.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the speed signal (v_eff = sqrt(vx^2 + vz^2 + (share vy)^2)) and the intensity k ------------------------------
        public const double SpeedLinesVerticalShare = 0.6; // x the vertical speed: a jump's fall (13.5 x 0.6 = 8.1) stays under the gate; slams show
        public const double SpeedLinesGate = 9.0; // m/s v_eff: no streak at or under it (the walk is 8)
        public const double SpeedLinesFull = 24.0; // m/s v_eff: k = 1 (the boost: config dash.speed)
        public const double SpeedLinesCurve = 0.6; // k = clamp((v - gate) / (full - gate), 0, 1)^this: sprint 0.25, slide 0.35, boost 1
        public const double SpeedLinesReleaseHalfLife = 0.08; // s: k falls with this half-life (attack instant): no flicker on landings
        // build 3's reel (fix V5: 12 of 13 frames 0.45 s after the speed fell under the gate still drew streaks): every streak's
        // alpha follows the speed now, through a faster copy of k
        public const double SpeedLinesFadeHalfLife = 0.03; // s: the fade's k falls with this half-life (attack instant): a burst is gone 0.23 s after a stop ...
        public const double SpeedLinesFadeK = 0.12; // ... and a streak is whole at this k or more (the sprint is 0.25), 0 at 0
        public const float SpeedLinesReportAlpha = 0.02f; // a streak under this alpha is neither drawn nor counted (L2) ...
        public const float SpeedLinesReportPx = 1; // ... nor one shorter than this on screen (px)
        // build 3's reel (fix V4: the slam's streaks turned flat when it landed into a slide): a slam's impact clears the air
        public const double SpeedLinesSlamCut = 0.08; // s: every live streak ends within this of a slam's landing
        // build 3's reel (fix V3: the held sprint drew 59 px on the real map, 35 in the open sample: births near walls): a
        // streak's screen length is capped by the speed, its tail pulled in along its own line (perspective-correct)
        public const float SpeedLinesCapPxLow = 40; // px at 1080p: the cap at SpeedLinesCapKLow or slower (L3 sprint: 25-45) ...
        public const float SpeedLinesCapPxHigh = 130; // ... to this at k = 1 (L3 boost: 80-140)
        public const double SpeedLinesCapKLow = 0.25; // the sprint's k

        // --- births ---------------------------------------------------------------------------------------------------
        public const double SpeedLinesRate = 250; // streaks a second at k = 1 (the research's 120 counted every live streak: its ~11
                                                  // alive sprinting show only ~5 on screen (SpeedLines20Tests' sample: heads close on the
                                                  // eye, fade near it, leave the screen); 250 shows ~11, L2's density)
        public const double SpeedLinesBoostRateMult = 2; // x the rate while a boost runs ...
        public const double SpeedLinesBoostRateTime = 0.25; // ... for this long after its onset (s: the boost's own 0.25 s)
        public const int SpeedLinesPool = 256; // streaks in the pool (the oldest reused; a boost keeps ~190 alive)
        public const int SpeedLinesMaxDrawn = 64; // at most this many drawn a frame (the newest first)
        public const double SpeedLinesRingIn = 0.40; // the birth ring's inner rho (0 the crosshair, 1 the edge midpoints)
        public const double SpeedLinesRingOut = 1.10; // ... and outer (area-uniform between them)
        public const double SpeedLinesDepthMin = 3; // m ahead along the view (a streak never starts an inch from the eye) ...
        public const double SpeedLinesDepthMax = 14; // ... to this
        public const int SpeedLinesSpawnTries = 4; // a birth redraws at most this many times (its ray hit the world, or an enemy is near)
        public const int SpeedLinesRaysPerFrame = 16; // world rays a frame for the births' test; a birth past it waits for the next frame
        // build 2's reel (fix V1: the held sprint drew a median of 4, not 11): a birth whose ray hit the world was dropped, and the
        // floor fills the screen's lower half past ~4 m: now the birth moves in front of what it hit, along the same ray
        public const double SpeedLinesFreeMin = 3.0; // m along the ray: a birth needs this much free air before the world (build 3: 2.5 let births sit close to walls: long on screen) ...
        public const double SpeedLinesFreeMargin = 0.35; // ... and keeps this far off the surface it hit (no z-fighting on the floor)
        public const double SpeedLinesLife = 0.38; // s a flow streak lives ...
        public const double SpeedLinesLifeJitter = 0.25; // ... +- this share (cosmetic dice)

        // --- shape (KvantStream's rule: the tail is head + v x T, recomputed every frame) -----------------------------
        public const double SpeedLinesExposureMin = 0.026; // s of travel the streak shows at k = 0 (build 2's reel, fix V2: the sprint drew 61 px at 0.041 s; the research's 0.040; the samples: 27 px in open air, 36 over a floor) ...
        public const double SpeedLinesExposureMax = 0.050; // ... and at k = 1 (the research's 0.065; with births in front of the floor the boost's flow drew 152 px at 0.060: now ~127, of 80-140) ...
        public const double SpeedLinesExposureCurve = 1.5; // ... along k^this: the sprint (k 0.25) shows 0.029 s, the dash-jump chain (0.69) 0.040, the boost 0.050
        public const double SpeedLinesLengthMin = 0.15; // m: never shorter (a stop shrinks it to a dot of this)
        public const double SpeedLinesLengthMax = 2.5; // m: never longer
        public const float SpeedLinesWidthPx = 1.5f; // px wide at the head, at 1080p, at any depth (thin)
        public const float SpeedLinesFeatherPx = 0.75f; // px of soft edge each side (the shader's across fade)
        public const float SpeedLinesTipTaper = 0.25f; // x the width at the tail
        public const float SpeedLinesRefHeight = 1080; // px: the widths' and the probe's reference screen height

        // --- brightness ------------------------------------------------------------------------------------------------
        public const double SpeedLinesAlphaFlow = 0.30; // a flow streak's peak alpha
        public const double SpeedLinesLifeFadeIn = 0.15; // of its life: fades in over this share ...
        public const double SpeedLinesLifeFadeOut = 0.55; // ... and out from this share to its end
        public const double SpeedLinesNearFade0 = 1.5; // m from the eye: 0 here ...
        public const double SpeedLinesNearFade1 = 3; // ... whole from here (nothing huge an inch from the eye)
        public const double SpeedLinesFarFade0 = 11; // m: whole to here ...
        public const double SpeedLinesFarFade1 = 15; // ... 0 here
        public const float SpeedLinesFlowPale = 0.5f; // the player's cyan toward white by this (cyan-pale; rule 8)
        public const float SpeedLinesBurstPale = 0.3f; // ... for the bursts (a touch more cyan)
        public const double SpeedLinesAdsCut = 0.7; // x (1 - this x the ADS amount): 0.3 of the streaks fully zoomed

        // --- the central clear zone (per pixel in the shader, and the probe's clip) --------------------------------------
        public const double SpeedLinesClearIn = 0.33; // rho: nothing drawn inside this ellipse about the crosshair ...
        public const double SpeedLinesClearFull = 0.55; // ... a smoothstep to whole here
        public const double SpeedLinesCapAlpha = 0.29; // any pixel inside SpeedLinesClearFull is under this alpha (L1: < 0.30 to 0.55) ...
        public const double SpeedLinesCapFull = 0.70; // ... the cap rising to 1 by this rho (a burst's 0.55 reaches whole past it)

        // --- bursts (the boost's onset, a boost-jump, the shockwave) -------------------------------------------------------
        public const int SpeedLinesBoostBurst = 32; // streaks at a boost's onset (x the boost's share: a partial boost 0.6 ..)
        public const double SpeedLinesBoostPartialMin = 0.6; // a partial boost's least share of the burst (CameraFeel's BoostFovPartialMin)
        public const int SpeedLinesBoostJumpBurst = 20; // streaks on a boost-jump
        public const int SpeedLinesShockwaveBurst = 18; // streaks on the THUNDERCLAP's shockwave (its launch)
        public const double SpeedLinesBurstRingIn = 0.45; // a burst's ring (rho) ...
        public const double SpeedLinesBurstRingOut = 1.05; // ... to this
        public const double SpeedLinesBurstDepthMin = 3; // m ...
        public const double SpeedLinesBurstDepthMax = 10; // ... a burst sits nearer (longer on screen)
        public const double SpeedLinesBurstExposure = 0.050; // s of travel a burst streak shows (the research's 0.07; the floor's near births: 0.050 keeps L3's 80-140)
        public const double SpeedLinesAlphaBurst = 0.55; // a burst streak's peak alpha
        public const double SpeedLinesBurstLife = 0.25; // s a burst streak lives

        // --- the travel blur (Render/TravelBlurFeature.cs + Shaders/Resources/TravelBlur.shader; the research's 3.3) -------
        public const double TravelBlurFrom = 14; // m/s v_eff: no blur under it (a sprint, a slide) ...
        public const double TravelBlurFull = 24; // ... whole at the boost (smoothstep between)
        public const double TravelBlurShutter = 0.010; // s: the eye's travel the blur shows (linear velocity only, never the look)
        public const float TravelBlurMaskIn = 0.25f; // rho: the middle stays sharp inside this ...
        public const float TravelBlurMaskFull = 0.60f; // ... whole blur from here (a smoothstep about the crosshair)
        public const float TravelBlurClamp = 0.018f; // of the screen's width: the longest smear (~35 px at 1920)
        public const string MotionBlurSetting = "motionBlur"; // the ACCESS row's key (the lead adds the row: GameSettings.Extras)
        public const string MotionBlurLabel = "MOTION BLUR"; // the row's label
        public const string MotionBlurDesc = "A light smear at the screen's edges on a boost (never on looking around)"; // its line
        public const bool MotionBlurDefault = true; // on by default (GG's SPEED LINES switch: WORLD STREAKS + a light travel blur)

        // --- clarity ---------------------------------------------------------------------------------------------------
        public const float SpeedLinesEnemyMarginPx = 40; // px (at 1080p) a streak keeps off a live enemy's screen rect (L5)
        public const int SpeedLinesEnemyRects = 64; // enemies tested a frame (the nearest in the list order; more are rare)
        public const uint SpeedLinesSeedSalt = 0x61697232; // "air2": the render-side stream's salt (never a sim stream)
    }
}
