// NIGHT18 U3-31.5 (builder M): movement you can see (NIGHT18.md M6-M8), the camera's and the speed lines' numbers. Every
// NIGHT18 number has its NIGHT17 one beside it (`...N17`, picked by FeelAB.On through UnityTuning.Pick). Unity-only: the web's
// camera numbers (camera.*, feel.*) stay in config.json and are read where the web reads them; these are the port's view
// numbers on top (Research/2026-10-06-feel-in-motion.md section C's inferred targets).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the boost (M6: +10 deg +/- 2 within 2 frames, back over 300-400 ms) ----------------------------------------------
        // NIGHT18: one envelope per boost (the Mover's Dash event): camera.dashFovKickDeg whole on the boost's first frame, back to
        // 0 over BoostFovRelease s on 1 - smoothstep (near the peak through the dash's first frames, a soft landing), so it is
        // within 0.5 deg of the rest at ~0.31 s and at rest at 0.36 s. The speed ramp holds still while dashing (the dash's
        // 24 m/s no longer piles 7 deg of ramp on the kick: NIGHT17's boost peaked near +17 deg). A partial boost (the bar under
        // a full boost's cost) kicks BoostFovPartialMin .. 1 of it by its share. NIGHT17: the web's dashKick (in over
        // dashFovInTime, out over dashFovOutTime) filled to whole while boosting, plus the speed ramp.
        public const double BoostFovRelease = 0.36; // s
        public const double BoostFovPartialMin = 0.6; // of the kick, for the shortest partial boost

        // --- the sprint (M6: +5-6 deg within 250 ms, held) ----------------------------------------------------------------------
        // NIGHT18: the sprint's widen is a floor under the web's speed ramp (max, not a sum: the ramp adds 1.25 deg at the
        // sprint's 10.5 m/s, which NIGHT17 summed to +6.25), eased in from the boost key's press when the key stays held (the
        // sprint follows the boost: one key), under the boost's kick (the larger of the two shows). NIGHT17: 5 deg summed with
        // the ramp, eased in from the sprint's start (after the dash).
        public const double SprintFovDegN18 = 5.95; // round 1: the top of 5-6 (pass 1: 5.5)
        public const double SprintFovEaseN18 = 15.0; // per s (exponential): 95 % in 200 ms

        // --- the slide (M6: +6-8 deg and a >= 0.4 m drop within 100 ms, roll 3-5 deg) -----------------------------------------
        // NIGHT18: the slide's widen is one part of SlideFovDeg on top of the run's widen (it replaces the web's feel.slideFovDeg
        // 3 eased at camera.fovEaseRate 8 /s, 1.7 deg at 100 ms, and NIGHT17's extra 2.5 at 8 /s), eased at SlideFovEase /s
        // (98 % in 100 ms); a slide from a sprint keeps the sprint's widen under it (the slide pauses the sprint), so the
        // slide's change is the slide's own. The roll: the web's feel.slideTiltDeg 3 plus SlideRollExtra, both eased at
        // SlideRollEase /s (3.7 deg at 100 ms; NIGHT17: the tilt at feel.rollEaseRate 10 /s and 1.5 more at 30 /s, 3.3 deg at
        // 100 ms). The drop: the web's eye ease (slide.cameraDrop 0.7 m over camera.eyeEaseTime 0.1 s) and NIGHT17's entry dip
        // (UnityTuning.SlideDip) already give 0.7 m at 100 ms; unchanged.
        public const double SlideFovDegN18 = 7.0;
        public const double SlideFovEaseN18 = 40.0; // per s
        public const double SlideRollExtraN18 = 1.0, SlideRollExtraN17 = SlideRollDeg; // deg on top of feel.slideTiltDeg
        public const double SlideRollEaseN18 = 25.0; // per s, the whole slide roll (NIGHT17: the tilt at feel.rollEaseRate, the extra at SlideRollEase)

        // --- the strafe and the landing (M7: roll 1.5-3 deg at full strafe; a dip of 0.05-0.15 m by fall speed, back in 150-200 ms)
        // NIGHT18: Quake's V_CalcRoll (the lateral speed's share of a walk x the roll), StrafeRollDeg at a walk's speed sideways
        // (NIGHT17: the web's feel.strafeRollDeg 1.5, on the target's lower edge).
        public const double StrafeRollDegN18 = 2.4;
        // NIGHT18: the landing dip keeps the web's depth per speed (feel.landingDipPerSpeed 0.01 m per m/s: 0.137 m from a 3 m
        // drop) but at most LandDipMax, in over LandDipAttack s (a sine) and back over LandDipBack s on 1 - smoothstep (at rest
        // 0.18 s after the landing, within 10 % of it at ~0.15 s; round 1: in over 0.03 s, back over 0.2 s (build 4), at most 0.145 m at
        // LandDipPerSpeedN18 per m/s). NIGHT17: the web's (max feel.landingDipMax 0.25, in over
        // feel.landingDipAttack 0.06 s, back over feel.landingDipReturnTime 0.2 s cubic: at rest 0.26 s after the landing).
        public const double LandDipMaxN18 = 0.145; // m (round 1: the top of 0.05-0.15 for the reel's 13.5 m/s fall; pass 1: 0.15 at the web's 0.01 per m/s)
        public const double LandDipAttackN18 = 0.03, LandDipBackN18 = 0.2; // s (round 1: quicker in, 0.15 back; build 4 read M7's back at 133 ms: 0.2 aims at ~170 ms; pass 1: 0.04 / 0.14)

        // --- the speed lines (M8: alpha > 0.3 only above 70 % of the top speed, never inside the central 60 % of the screen) ---
        // (Round 1: the top is now slide.floorSpeed, 11.6 m/s, the reel's maxSpeed; the numbers below are round 1's where marked.)
        // NIGHT18: the lines' alpha comes from the speed's share of dash.speed (24 m/s, the boost: the top ground speed a player
        // reaches by themselves; the reel's header `maxSpeed` should be the same number): 0 under LinesGate, rising on a
        // smoothstep to LinesMaxAlpha at the top; while sprinting (the sprint's 10.5 m/s is 44 %) faint edge lines at
        // LinesSprintAlpha; and never over LinesQuietAlpha at or under the gate, whatever the fade (the dash's end drops the
        // speed to a walk in one step: the lines drop under the line in that frame). In over LinesFadeIn s (whole on the boost's
        // first frame or two), out over the web's feel.speedLinesFadeTime. The streaks start outside a box (round 2: LinesBoostInner / LinesRunInner)
        // (half-extent, of the image's half-size) and the image shifts at most LinesShift of half the canvas toward the
        // sideways travel, so nothing is drawn inside the screen's central 60 % (LinesClearHalf >= 0.6).
        // NIGHT17: the web's (feel.speedLinesMinSpeed 20 .. FullSpeed 40, or whole while dashing, x speedLinesMaxOpacity 0.55,
        // the fade both ways over speedLinesFadeTime 0.15 s), a radial sprite from r 0.45 and Hud17LinesShift 0.35.
        public const double LinesGate = 0.7; // of dash.speed
        public const double LinesQuietAlpha = 0.28; // the ceiling at or under the gate (M8's line is 0.3)
        public const double LinesMaxAlpha = 0.45; // the run layer at the top speed (round 2: milder; pass 1 / round 1: 0.6)
        public const double LinesSprintAlpha = 0.4; // round 2: ~0.4 (round 1: 0.55) // round 1: the sprint is over 70 % of slide.floorSpeed: strong edge lines (pass 1: 0.22 under dash.speed)
        // Round 1 (build 3: "[N17] dense speed lines held to 250 ms; [N18] lines thin and gone by B6"): above the top (the boost
        // runs at about twice it) the alpha rises on to LinesBoostAlpha by LinesBoostShare of the top; the streaks are denser
        // (LinesStreaksN18, wider) and start nearer the central box (round 2: per layer, LinesBoostInner / LinesRunInner).
        public const double LinesBoostAlpha = 0.8, LinesBoostShare = 1.8;
        public const int LinesStreaksN18 = 150;
        public const float LinesStreakWidthMin = 0.16f, LinesStreakWidthSpan = 0.1f; // of a streak's angular bin
        // Round 2 (build 6: boost to NIGHT17, "[N17] long lines reach toward the centre and are still full at 250 ms"; "too much"
        // on the sprint: "a dense edge band ... over the HP and ammo panels"): two layers. The boost's streaks start
        // LinesBoostInner..+LinesBoostSpread out (to the central 60 % box's edge, not into it: no jitter turn, LinesJitterN18 0,
        // and a smaller shift), whole on the boost's first frame (LinesBoostFadeIn); the run's (sprint, slide) start
        // LinesRunInner..+LinesRunSpread out and fade to 0 over the HUD's bottom corners (|x| from LinesHudX0 to X1 of the half-
        // width and y from LinesHudY0 to Y1 of the half-height below the middle: the HP plate, the ammo and points).
        public const float LinesBoostInner = 0.63f, LinesBoostSpread = 0.04f; // round 3: the thin streaks' starts (BoostStreakAlpha)
        public const float LinesRunInner = 0.74f, LinesRunSpread = 0.06f;
        public const double LinesBoostFadeIn = 0.01; // s per whole alpha
        public const double LinesJitterN18 = 0, LinesJitterN17 = 1; // x feel.speedLinesJitterDeg
        public const float LinesHudX0 = 0.3f, LinesHudX1 = 0.5f, LinesHudY0 = 0.3f, LinesHudY1 = 0.5f;
        // Round 3, final (the boost "too much" and to NIGHT17: "[N18] a short, dense band at the edges ... reads as a frame, not
        // as travel"; "[N17] long radial streaks reach in toward the centre"): the boost layer is LinesBoostStreaks thin, sharp
        // radial lines (CameraMove.BoostStreakAlpha) on a LinesBoostTexW x LinesBoostTexH texture (texels near square on the
        // 1.87 : 1 image: about 2 px at 1080p), each LinesBoostHalfWidthMin..+Span texels either side of its ray (a tent: about
        // 1.5-3 px drawn), from just past the central box (LinesBoostInner + its share of LinesBoostSpread, a LinesBoostStartFade
        // anti-alias start) to the screen's edge; no haze, no fill.
        public const int LinesBoostStreaks = 50;
        public const int LinesBoostTexW = 1024, LinesBoostTexH = 548;
        public const float LinesBoostHalfWidthMin = 0.7f, LinesBoostHalfWidthSpan = 0.7f; // texels
        public const float LinesBoostStartFade = 0.02f; // of the half-size
        public const double LinesFadeIn = 0.03; // s for a whole alpha of 1
        public const double LinesFadeOutN18 = 0.6; // s for a whole alpha of 1 (out; round 1: the quiet 0.28 tail lingers ~170 ms after a dash)
        public const float LinesInnerSpan = 0.08f; // the streaks' fade-in length past their start (round 2: the starts are per layer, below) // the streaks' start (Chebyshev, image half-size) and their fade-in length
        public const float LinesShiftN18 = 0.05f, /* round 2: the boost's longer streaks keep the centre clear (was 0.1) */ LinesShiftN17 = Hud17LinesShift; // of half the canvas
        public const int LinesStreaks = 90; // the sprite's streaks (NIGHT17's count)
        // The reel's FeelFx.SpeedLinesClear on the FeelAB off side: NIGHT17's sprite draws nothing inside r 0.5 of its half-size
        // (each streak starts at 0.5-0.75), so its clear box is the square inside that circle: 0.5 / sqrt(2) of the half-size.
        public const float LinesClearN17 = 0.3535f;

        // --- the stamina bar (readable while it drains; NIGHT17: 160 x 9 px, "a ~75 x 4 px sliver" at the strips' 1/3 scale) ----
        // NIGHT18: StaminaBarWidth x StaminaBarHeight px at StaminaBarY under the crosshair, and a pale trail behind the fill
        // (UiTheme.CyanPale at StaminaTrailAlpha): a spend leaves the spent part pale for StaminaTrailHold s, then the trail eases
        // down to the fill at StaminaTrailRate /s, so the sprint's drain shows a ~0.08-bar (~19 px) pale strip at the fill's end
        // and a boost a 120 px pale chunk that slides away.
        public const double StaminaTrailHold = 0.15; // s after a chunk spend (a boost, a dash-jump)
        public const double StaminaTrailRate = 5.0; // per s (exponential)
        public const float StaminaTrailAlpha = 0.85f;
        // NIGHT18: the bar's entrance from hidden (M9: no element at full size and alpha on its first frame): alpha in over
        // StaminaBarIn s (ease-out-cubic: 0.36 on the first frame at 60 fps), the scale from StaminaBarInScale (ease-out-back).
        // NIGHT17: none (shown at full alpha in one frame).
        public const double StaminaBarIn = 0.12; // s
        public const float StaminaBarInScale = 0.8f;

        // --- the kill flourish (NIGHT18 U3-31.9 #1: captured and readable; Viewmodel.Feel.cs HandsFlourish) -------------------
        // NIGHT18: any kill of the player's arms it (NIGHT17: a MULTIKILL or a rank-up only, so the reel's single kills never
        // showed it); it starts after HandsFlourishQuietN18 s of no shot (world time) with the trigger let go (NIGHT17: 0.35 s,
        // trigger or not: a slow gun's held trigger started it between shots), so in a kill strip it peaks near the flipbook's
        // +400 ms frame; and it shows the gun off: HandsFlourishPresentY m up, PresentX m in toward the middle, PresentYawDeg
        // canted in, on a sine bump over the flourish (x the draw's motion; none with REDUCED MOTION: the still pose-pop stays).
        public const double HandsFlourishQuietN18 = 0.2; // s
        public const double HandsFlourishPresentY = 0.03, HandsFlourishPresentX = 0.025; // m
        public const double HandsFlourishPresentYawDeg = 12; // deg

        // --- the QUICKDRAW glint on the gun's own surface (NIGHT18 U3-31.9 #2; Viewmodel.Feel.cs HandsQuickdraw.Sheen) ---------
        // NIGHT18: in QUICKDRAW's window (the 0.5 s after fire opens on a swap, HandsQuickdrawSweeps passes, each dimmer) the
        // gun's own meshes are drawn again additively in colors.usable x HandsQuickdrawSheenHdr x the part's sheen, exactly where
        // the gun is (the afterimages' way: no separate quad to float off), so the gun's surface lights: HandsQuickdrawSheenBase
        // of the pass everywhere, the rest a band HandsQuickdrawSheenWidth wide (of the gun's length) running back to muzzle
        // over its parts. x FLASH INTENSITY (as NIGHT17's). NIGHT17: the gem only (the strip, HandsQuickdrawStrip, was off: it
        // floated off the gun).
        public const double HandsQuickdrawSheenBase = 0.3;
        public const double HandsQuickdrawSheenWidth = 0.35;
        public const float HandsQuickdrawSheenHdr = 1.6f;

        // --- round 1 (build 3's blind picks: boost and sprint to NIGHT17 with both reviewers, land "Same") -------------------
        // The boost's kick at the top of M6's +10 +/- 2 (NIGHT18 pass 1: camera.dashFovKickDeg 10):
        public const double BoostFovKickDegN18 = 11.8;
        // The landing (the dip 0.136 m on both sides read the same): the dip's depth per m/s of the fall and its quicker attack
        // live above (LandDipPerSpeedN18, LandDipAttackN18); a short pitch nod down, LandNodDeg at LandNodFullFall m/s or faster
        // (none under LandNodMinFall), in over LandNodAttack s, back over LandNodRecover s (none with REDUCED MOTION); and the
        // gun's own landing weighed linearly from HandsLandMinFallN18 to full at HandsLandFullFallN18 (NIGHT17's smoothstep from
        // 7 to 28 m/s gave the reel's 13.5 m/s fall 0.24 of it), at HandsLandPitchDegN18 / DropYN18 / RollDegN18.
        public const double LandDipPerSpeedN18 = 0.0115; // m per m/s of the fall (the web's feel.landingDipPerSpeed is 0.01)
        public const double LandNodDeg = 3.0, LandNodMinFall = 4, LandNodFullFall = 13.5; // deg, m/s, m/s
        public const double LandNodAttack = 0.04, LandNodRecover = 0.16; // s
        public const double HandsLandMinFallN18 = 4, HandsLandFullFallN18 = 14; // m/s
        public const double HandsLandPitchDegN18 = 9, HandsLandDropYN18 = 0.04, HandsLandRollDegN18 = 2; // deg, m, deg

        // --- the accessibility keys ------------------------------------------------------------------------------------------
        /// <summary>The settings store's REDUCED MOTION key (the web's settings.reducedMotion).</summary>
        public const string ReducedMotionKey = "reducedMotion";

        // --- the reel's moment names (FeelProbe.Event, Tools/feel-schema.md's `events`), from the Mover's events in CameraFeel ---
        public const string ProbeBoost = "boost", ProbeSlide = "slide", ProbeLand = "land";
        public const string ProbeFlourish = "flourish"; // the kill flourish's first frame (Viewmodel.Feel.cs)
        /// <summary>NIGHT18 U3-31.9 #6: the FeelFx member the gun in hand is reported as (its on-screen rect through the viewmodel
        /// camera, every frame; the kick per gun). The lead adds `Gun = 10` (Count 11) to FeelFx; until then it is skipped.</summary>
        public const int ProbeGunFx = 10;
    }
}
