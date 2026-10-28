// NIGHT16 U3-29.6 (builder V): the hands' render numbers (CLAUDE.md rule 5: Unity-only numbers live here, each with a
// comment). Why the web has none of them: the web has ONE swap curve for every gun (viewmodel.js swapDropY / swapPitchDeg
// on the loadout's lowered amount) and no per-gun draw, so the holster tip, the draw signatures (the KESTREL's flick and
// spin, the THUNDERCLAP's rack, the COMET's shoulder, the LONGWAVE's sweep, the GAUNTLET's clench) and their timings are
// the port's. The left arm's poses and timings ARE the web's (config.viewmodel.arm / armRest / armPunch / punchOutTime /
// punchHoldTime, config.punch.cycleTime) and are read from config; only the knuckles' parry flash (port-only) is here.
// The glint is the web's too (config.viewmodel.states.glintTime / glintGain / glintMinCooldown): nothing here.
// Every draw number is visual only: the sim's swap timer (weapons.swapTime / swapLowerFraction / swapFireFraction) gates
// fire, and a draw lives inside it (HandsDraw: the raise is up by the step fire opens; a shot cancels the flourish).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the holster (the swap's lowering half): the gun tips toward the right hip as it drops --------------------
        // web camera space: +x is right; roll and yaw negative turn the muzzle down-right (three's rotation.z / .y).
        public const double HandsHolsterX = 0.035; // m to the right at the bottom of the holster (toward the hip)
        public const double HandsHolsterRollDeg = 22; // deg of clockwise roll at the bottom (the top of the gun falls right)
        public const double HandsHolsterYawDeg = 12; // deg the muzzle swings right at the bottom

        // --- REDUCED MOTION and effects intensity ---------------------------------------------------------------------
        // The signature motion (flick, lift, shoulder, rise) is x effects intensity (as the bob) and x this with REDUCED
        // MOTION on; the parts (spin, rack, sweep, clench) and the web's own settle (recoilSprings.<class>.settle*) stay.
        public const double HandsReducedMotionScale = 0.3;

        // --- the KESTREL (revolver): a flick up and a cylinder spin -------------------------------------------------
        public const double HandsKestrelRaisePitchDeg = 7; // deg: the muzzle leads the raise (a sine bump, 0 at both ends)
        public const double HandsKestrelFlickDeg = 9; // deg: after fire opens, the muzzle flicks up on the gun's class spring
        public const double HandsKestrelFlickRollDeg = -5; // deg: a little wrist twist with the flick
        public const double HandsKestrelSpinTurns = 1; // whole turns of the cylinder (whole: it ends on the same chamber)
        public const double HandsKestrelSpinTime = 0.34; // s the spin takes (ease-out), from the step fire opens

        // --- the THUNDERCLAP (shotgun): a pump rack on arrival (the pump part and viewmodel.pumpTiltDeg reused) --------
        public const double HandsThunderclapRackTime = 0.26; // s for the pump's out-and-home (the web's pumpDuration is 0.3; a draw rack is brisker)
        public const double HandsThunderclapRackLiftDeg = 3.5; // deg: the muzzle lifts as the pump is racked

        // --- the COMET (rocket): a heavy lift from the shoulder and a settle on its class spring (heavy) ------------
        public const double HandsCometRaiseX = 0.06; // m: it comes up from further right (the shoulder), 0 at fire-open
        public const double HandsCometRaiseRollDeg = -16; // deg: rolled onto the shoulder while it rises
        public const double HandsCometShoulderY = 0.016; // m: the extra drop of the shoulder settle (x the class spring's unit kick)
        public const double HandsCometShoulderRollDeg = 5; // deg: the roll back as it seats

        // --- the LONGWAVE (rail): a charge glow sweeping from the receiver to the muzzle ----------------------------
        public const double HandsLongwaveSweepTime = 0.32; // s for the band to run the barrel
        public const double HandsLongwaveSweepGlow = 2.5; // x the charge glow at the sweep's peak (1 + this x sin(pi u))
        public const float HandsLongwaveBandW = 0.05f, HandsLongwaveBandH = 0.05f, HandsLongwaveBandD = 0.012f; // m, the band (web rig space)
        public const float HandsLongwaveBandStartZ = 0.02f, HandsLongwaveBandEndZ = -0.42f; // m, web rig z (the barrel runs -z)
        public const float HandsLongwaveBandY = 0.024f; // m, web rig y (the ring's height, BlockyKit.LongwaveRingAt)
        public const float HandsLongwaveBandHdr = 3f; // the band's HDR colour scale at the sweep's peak (x the family colour)

        // --- the PULSE GAUNTLET: the fist rises as it clenches, the gem flares --------------------------------------
        public const double HandsGauntletClenchIn = 0.07; // s to clench
        public const double HandsGauntletClenchHold = 0.06; // s held
        public const double HandsGauntletClenchTotal = 0.3; // s until released
        public const double HandsGauntletRiseY = 0.014; // m: the fist rises (x the class spring's unit kick)
        public const double HandsGauntletRisePitchDeg = 5; // deg: the knuckles tip up with it
        public const double HandsGauntletClenchSquash = 0.07; // the rig's x / y squeeze at full clench (a share of its scale)
        public const double HandsGauntletGemFlare = 2.2; // x the gem at full clench (1 + this x clench), lerped toward white

        // --- the left arm (viewmodel.arm): the port-only knuckle flash on a parry -------------------------------------
        // The web's arm has no flash: on a parry its knuckles light (the family cyan, `colors.usable`), on a PERFECT PARRY
        // white. A thin plate on the fist's front face, drawn only while it flashes (at rest the arm is the web's).
        public const double HandsKnuckleFlashTime = 0.16; // s (world time: it holds through the parry's freeze)
        public const float HandsKnuckleHdr = 3.2f; // HDR colour scale at the flash's peak (x flashIntensity)
        public const float HandsKnuckleDepth = 0.008f; // m, the plate's thickness (it sits on the fist's front face)
        public const float HandsKnuckleInset = 0.004f; // m, the plate is this much smaller than the fist's face on each side
        public const double HandsKnuckleParryWhite = 0.25; // a plain parry's knuckles: the cyan this far toward white (a perfect one: all white)

        // --- the quick draw (FEEL-DESIGN 2.4: ULTRAKILL treats a gun fired in the past 2 s differently on its draw) -------
        // Ours never touches the sim's timer: a gun fired within the window draws with its flourish's motion x the scale
        // (a juggling player sees a brisk draw, not the full show). The web has no per-gun draw, so no such number.
        public const double HandsQuickDrawWindow = 2.0; // s since the gun last fired (real time)
        public const double HandsQuickDrawScale = 0.4; // x the signature motion

        // --- the swap's flick (NIGHT17 round 1, replacing NIGHT16's quick-swap flick): the view shows the loadout's TARGET from
        // the press; the gun on screen flicks out toward the hip (from wherever it is) over HandsFlickTime of world time to
        // HandsFlickDepth of the drop, where every kit rig is off-screen (HandsTests: off from ~0.56 with the tip), and the
        // wanted gun rises from there on the sim's own step clock, at hold exactly on the fire step. No empty hands. The
        // web has one drop-and-raise on the sim's lowered amount (the new gun only from the swap's bottom).
        public const double HandsFlickTime = 0.055; // s, the old gun's flick out (linear: it leaves on the first frame and doesn't linger off-screen)
        // round 2 (the reviewer: "blank 1-2 frames on every gun"): the guns overlap: the new one shows this long before the old
        // one's flick ends (the old one still leaving on screen while the new one's top shows); at least this share of the flick first
        public const double HandsSwapOverlap = 0.045, HandsSwapSwitchMinShare = 0.25;
        public const double HandsFlickShare = 0.3; // at most this share of the time to the fire step (the QUICK perk's shorter swap)
        // x the drop (swapDropY / swapPitchDeg) where the guns switch: just past each rig's off-screen line (HandsTests
        // measures it: 0.53-0.56 with the hip tip, 0.55-0.60 without; the raised GAUNTLET 0.41 / 0.44). With the tip x the
        // motion scale, the depth runs from Still (no tip) to Depth (the full tip).
        public const double HandsFlickDepth = 0.6, HandsFlickDepthStill = 0.645; // round 3: deeper for the bigger rigs
        public const double HandsFlickDepthGauntlet = 0.45, HandsFlickDepthGauntletStill = 0.485;
        public const double HandsFlickDepthThunderclap = 0.47, HandsFlickDepthThunderclapStill = 0.56; // round 6: turned down-right it leaves sooner (off from 0.45 / 0.54)
        public const double HandsRiseMinSteps = 8; // sim steps left to the fire step below which the rise runs on world time (a called-back gun: no 2-frame jump)
        public const double HandsSimJumpSteps = 2.5; // a frame-to-frame jump of the sim's swap step above this is its mirror (re-plan the rise)
        public const double HandsRiseMinTime = 0.08; // s, that rise (a gun called back after fire opened; a shot speeds it x HandsRaiseYieldSpeed)

        // --- the idle flourish (FEEL-DESIGN 5 #14): in a BREAK only, after this long without input, at most once per it
        public const double HandsIdleAfter = 8; // s
        // the KESTREL: rolled out to show the cylinder, one whole turn, a flick back (s from the start; deg)
        public const double HandsIdleKestrelRollDeg = 24, HandsIdleKestrelRollIn = 0.16, HandsIdleKestrelRollHold = 0.42, HandsIdleKestrelRollTotal = 0.8;
        public const double HandsIdleKestrelSpinFrom = 0.12, HandsIdleKestrelSpinTime = 0.42;
        public const double HandsIdleKestrelFlickFrom = 0.58, HandsIdleKestrelFlickTime = 0.2, HandsIdleKestrelFlickDeg = 7;
        // the THUNDERCLAP: the pump checked (half a rack), the gun tipped to look at it
        public const double HandsIdlePumpTravel = 0.5; // x the full rack
        public const double HandsIdlePumpIn = 0.14, HandsIdlePumpHold = 0.12, HandsIdlePumpTotal = 0.46; // s
        public const double HandsIdlePumpRollDeg = 10, HandsIdlePumpPitchDeg = 3, HandsIdlePumpTipIn = 0.15, HandsIdlePumpTipHold = 0.3, HandsIdlePumpTipTotal = 0.72;
        // the rest: a weight-class shift (a re-grip: inward, up a touch, rolled), longer and bigger for heavier guns
        public const double HandsIdleShiftX = -0.008, HandsIdleShiftY = 0.006, HandsIdleShiftRollDeg = 7, HandsIdleShiftPitchDeg = 2.5; // at amplitude 1
        public const double HandsIdleShiftIn = 0.2, HandsIdleShiftHold = 0.22;
        public const double HandsIdleShiftLightTotal = 0.62, HandsIdleShiftMediumTotal = 0.82, HandsIdleShiftHeavyTotal = 1.1; // s
        public const double HandsIdleShiftLight = 0.8, HandsIdleShiftMedium = 1.0, HandsIdleShiftHeavy = 1.3; // amplitude

        // --- movement on the hands (the web has the bob, the poses, the dash pull and the landing dip x landDipScale; these
        // are the port's, x effects intensity, x HandsReducedMotionScale with REDUCED MOTION) ---------------------------
        // a take-off (every jump kind): the gun lifts on its class spring (the recoil bank's channels: pitch deg, drop m)
        public const double HandsJumpLiftDeg = 2.2; // deg, muzzle up at the kick's peak
        public const double HandsJumpLiftY = 0.008; // m up at the peak
        // a landing weighted by fall speed, beyond the web's dip: nothing under MinFall, full at MaxFall (m/s)
        public const double HandsLandMinFall = 7, HandsLandMaxFall = 28;
        public const double HandsLandPitchDeg = 4.5; // deg, muzzle down at full weight
        public const double HandsLandDropY = 0.016; // m down at full weight
        public const double HandsLandRollDeg = 1.5; // deg, a little roll at full weight
        // the slam: a wind-up while slamming (the gun up and back), a hit down on the slam's impact (the `slam` event)
        public const double HandsSlamWindRate = 28; // 1/s, the wind-up's ease toward 1 while in the slam state
        public const double HandsSlamPitchDeg = 8; // deg, muzzle up at full wind-up
        public const double HandsSlamY = 0.022; // m up at full wind-up
        public const double HandsSlamBackZ = 0.03; // m back toward the eye at full wind-up (web +z)
        public const double HandsSlamImpactDeg = 5; // deg, muzzle down at the impact kick's peak
        public const double HandsSlamImpactY = 0.02; // m down at the impact kick's peak
        // the free (left) hand on the wall during a wall-ride with the wall on the LEFT (Titanfall: the free hand rests on
        // the wall); web camera space, the fist's pose (the arm's group, as armRest / armPunch). The web's arm has no wall pose.
        public const double HandsWallHandX = -0.30, HandsWallHandY = -0.10, HandsWallHandZ = -0.40; // m
        public const double HandsWallHandPitchDeg = 20, HandsWallHandYawDeg = -10, HandsWallHandRollDeg = 60; // deg (the flat of the fist to the wall)
        public const double HandsWallHandMinSide = 0.35; // the wall's side (its normal . the view's right) above which it is on the left
        public const double HandsWallHandRate = 14; // 1/s, the hand's ease to / from the wall (world time)
        // the wall-jump: the hand pushes off (out to the left on an envelope), then leaves with the ride
        public const double HandsWallPushX = -0.07; // m, out toward the wall at the push's peak
        public const double HandsWallPushOut = 0.04, HandsWallPushHold = 0.03, HandsWallPushTotal = 0.2; // s (viewmodel.js envelope's rise / hold / total)

        // --- flashes (NIGHT16 rule 8, WCAG 2.3.1): at most this many hand flashes (glint, knuckles) in any second ------
        public const int HandsFlashesPerSecond = 3;

        // ================================================================================================================
        // NIGHT17 U3-30.6 (builder V): the hands overhaul. The web has none of these: one smoothstep drop-and-raise for every
        // gun, one recoil shape per weight class, a sine punch envelope, no sprint (design.stamina is the port's), no
        // QUICKDRAW. Every number is visual only; the sim's swap timer, fire gate and punch timing are untouched.
        // ================================================================================================================

        // --- the swap in-to: an ease-out-back from the bottom to the hold pose, exactly at hold on the step fire opens ----
        // (HandsEase.OutBack's s; the overshoot above hold is about 4 s^3 / (27 (s + 1)^2) of the drop). Heavier guns seat.
        public const double HandsInToBackLight = 2.2; // ~15 % of the drop above hold, then settles
        public const double HandsInToBackMedium = 1.7; // ~10 %
        public const double HandsInToBackHeavy = 1.4; // ~7 %: a heavy gun seats, it doesn't bounce (round 6: 1.3 -> 1.4, the THUNDERCLAP rises from its shallower switch depth)
        // the holster yanks: an ease-out on the press (the gun moves on the first frame), not the web's slow-start smoothstep
        // (no number: HandsEase.OutSine). A shot during the respawn raise (no fire gate there): the raise's clock runs this
        // many times faster from that frame, so the gun is up in a frame or two (the animation yields; no pop).
        public const double HandsRaiseYieldSpeed = 6;

        // --- the QUICKDRAW glint: ULTRAKILL pays QUICKDRAW for a hit within 0.5 s of a swap; the gun shows that window ----
        // A cyan strip (colors.usable) runs along the top of the gun in hand from the back to the muzzle, HandsQuickdrawSweeps
        // times in the window, each pass dimmer; the gem brightens with it. From the step fire opens on a swap (not a respawn);
        // a shot ends it on the same frame. x FLASH INTENSITY. Not a flash (a small moving highlight on the gun, no change in
        // the screen's luminance), so it takes no FlashGate slot: the decision is in the report.
        public const double HandsQuickdrawWindow = 0.5; // s after fire opens
        public const int HandsQuickdrawSweeps = 3; // passes along the gun in the window
        public const float HandsQuickdrawHdr = 3.2f; // the strip's HDR colour scale at a pass's peak (x colors.usable)
        public const float HandsQuickdrawLen = 0.035f; // m, the strip's length along the barrel
        public const float HandsQuickdrawThick = 0.006f; // m, its height
        public const float HandsQuickdrawWide = 1.04f; // x the body's width
        public const float HandsQuickdrawAbove = 0.002f; // m above the body's top
        public const double HandsQuickdrawGem = 1.4; // x the gem at a pass's peak (1 + this x the glint), toward white

        // --- recoil: a spring impulse per gun (pitch up and back, roll, an overshoot and a settle), replacing the web's one
        // shape per class (viewmodel.recoil.<w> on recoilSprings.<class>) for the five kit guns that fire. Two layers each:
        // { freqHz, zeta, pitch deg (+ up), back m (+ toward the eye), drop m (+ down), roll deg (+ counter-clockwise), yaw deg
        // (+ muzzle left), side m (+ right) }; each value is the layer's FIRST PEAK (SpringBank.Add), the overshoot is the
        // spring's (zeta). x STEADY (perks.fx.recoil). The camera's kick (CameraFeel, builder X) pairs with these.
        // round 1: the KESTREL "takes a long time to settle": a stiffer, better-damped snap (was 9.5 Hz / 0.32 and 5.5 / 0.5)
        public static readonly double[] HandsKickRevolverA = { 11, 0.5, 13.5, 0.02, 0.004, 6, -1.5, 0 }; // snaps up and rolls out
        public static readonly double[] HandsKickRevolverB = { 9, 0.65, 0, 0.05, 0.008, 0, 0, 0 }; // the body comes back
        public static readonly double[] HandsKickShotgunA = { 4.2, 0.5, -7, 0.17, 0.03, -3, -6, 0.02 }; // shoves back hard, DOWN and RIGHT (round 5: the barrel lay across the horde; the centre opens on the shot)
        public static readonly double[] HandsKickShotgunB = { 11, 0.4, 0, 0, 0, -2.5, 0, 0 }; // a shudder on top (roll only: the muzzle stays down)
        public static readonly double[] HandsKickNailgunA = { 15, 0.5, 1.3, 0.012, 0.002, 1.5, 0.8, 0.003 }; // chatters small
        public static readonly double[] HandsKickRocketA = { 3.6, 0.55, 6, 0.2, 0.045, 4, 1, 0.006 }; // a heavy shove, a slow seat
        public static readonly double[] HandsKickRocketB = { 8, 0.4, 4, 0, 0, 0, 0, 0 }; // the launch jolt
        public static readonly double[] HandsKickRailA = { 7, 0.26, 12, 0, 0, -5, 1, 0 }; // a sharp pitch that rings (the hum)
        public static readonly double[] HandsKickRailB = { 4, 0.55, 0, 0.16, 0.025, 0, 0, 0 }; // the long push back
        public const double HandsKickNailgunJitter = 0.35; // x (1 + this x lattice(shot)) on the STITCHER's chatter
        public const int HandsKickSlots = 8; // live kicks at once (two layers a shot; the STITCHER fires fast)
        // the punch's body torque on the gun in the right hand (the right shoulder goes back as the left fist goes out)
        public static readonly double[] HandsKickPunchTorque = { 7, 0.5, -1.5, 0.03, 0.006, -5, -2, 0.012 };

        // --- the punch: a wind-up, an accelerating strike, a hard stop past the target, a snap back, a rebound on a hit ----
        // web camera space (the arm's group, as armRest / armPunch). The fist still reaches the target at the web's
        // punchOutTime (its impact frame), returns by punch.cycleTime, and the strike runs on frame time inside a freeze.
        public const double HandsPunchCockShare = 0.4; // the wind-up's share of punchOutTime (the strike gets the rest)
        public const double HandsPunchAimX = -0.03, HandsPunchAimY = -0.012; // m on armPunch: the port's bigger fist lands a touch left / low (the centre box)
        public const double HandsPunchCockX = -0.2, HandsPunchCockY = -0.155, HandsPunchCockZ = -0.29; // m: low left, pulled back
        public const double HandsPunchCockPitchDeg = 30, HandsPunchCockYawDeg = -34, HandsPunchCockRollDeg = 22; // the wrist cocked
        public const double HandsPunchOverZ = 0.04; // m past armPunch at the impact frame (deeper)
        public const double HandsPunchOverPitchDeg = -7, HandsPunchOverRollDeg = -8; // deg: the knuckles turn over into it
        public const double HandsPunchWhiffOver = 1.5; // x the overshoot on a whiff (the follow-through)
        public const double HandsPunchSnapTime = 0.03; // s: back to armPunch after the impact frame (the hard stop)
        public const double HandsPunchWhiffSnapTime = 0.07; // s: a whiff's follow-through comes back slower
        public const double HandsPunchReboundZ = 0.035; // m back toward the eye at the rebound's peak (a hit only)
        public const double HandsPunchReboundPitchDeg = 9; // deg: the knuckles kick up off the target
        public const double HandsPunchReboundTime = 0.1; // s, the rebound's bump after the snap

        // --- the sprint and boost poses (StaminaHud.Read: sprinting / boosting), eased in and out, yielding to a shot ------
        public const double HandsSprintInRate = 9, HandsSprintOutRate = 14; // 1/s eases (world time)
        public const double HandsSprintX = -0.012, HandsSprintY = -0.035, HandsSprintZ = 0.02; // m: the running carry, tucked (round 5: -0.058 hid the gun under the screen's edge; the tilt carries it)
        public const double HandsSprintPitchDeg = -33, HandsSprintYawDeg = 9, HandsSprintRollDeg = 20; // muzzle down, canted in (round 3: "reads as nothing": -13 -> -33)
        public const double HandsSprintBobX = 0.022, HandsSprintBobY = 0.03; // m: the carry's bob in step (the movement bob's phase; round 3: stronger)
        public const double HandsSprintBobRollDeg = 6, HandsSprintBobPitchDeg = 3; // deg (round 3: stronger)
        public const double HandsBoostInRate = 30, HandsBoostOutRate = 10; // 1/s eases (the dash is short)
        public const double HandsBoostLeanRollDeg = 9; // deg: leans into the boost's side
        public const double HandsBoostTuckY = -0.015, HandsBoostTuckZ = 0.015; // m: pulled in while boosting
        // the boost's swing: the gun lags the move, then catches up past it (an underdamped spring's own overshoot)
        public const double HandsDashLagHz = 5, HandsDashLagZeta = 0.38;
        public const double HandsDashLagX = 0.04, HandsDashLagZ = 0.03; // m at the lag's peak (a full sideways / forward boost)
        public const double HandsDashLagRollDeg = 8, HandsDashLagYawDeg = 5; // deg at the lag's peak
        // the slide (round 1, rubric row 4: boost, sprint and slide distinct): the gun low and canted toward the slide's lean,
        // the free hand bracing toward the floor, a fast tremor by speed (skidding); on top of the web's slide dip and roll
        // (viewmodel.slideDipY / slideRollDeg). Eased in / out over these times (smoothstep, world time); a slide-jump snaps it
        // back with an ease-out-back overshoot; yields to a shot like the sprint carry.
        public const double HandsSlideInTime = 0.08, HandsSlideOutTime = 0.12; // s
        public const double HandsSlideX = -0.012, HandsSlideY = -0.04, HandsSlideZ = 0.015; // m: low, in, a little back
        public const double HandsSlidePitchDeg = -6; // deg: the muzzle dips
        public const double HandsSlideRollDeg = 16; // deg at full lean (+: counter-clockwise, toward the left)
        public const double HandsSlideLeanRate = 20; // 1/s: the lean follows the slide's side
        public const double HandsSlideLeanBias = 0.6; // the lean straight ahead (1: full left); a slide to the side leans into it (lateral -1..1)
        public const double HandsSlideTremorX = 0.0025, HandsSlideTremorY = 0.002, HandsSlideTremorRollDeg = 0.7; // at full speed
        public const double HandsSlideTremorHzX = 27, HandsSlideTremorHzY = 31, HandsSlideTremorHzRoll = 23; // incommensurate: a skid, not a hum
        public const double HandsSlideTremorFullSpeed = 14; // m/s at which the tremor is full (0 at rest)
        public const double HandsSlideSnapTime = 0.09; // s: a slide-jump's snap back up (ease-out-back)
        public const double HandsSlideSnapBack = 1.7; // its ease-out-back's s (~10 % past: the gun pops a little above its rest)
        // the free (left) hand braces toward the floor (web camera space, the arm's group; at the screen's bottom-left edge)
        public const double HandsBraceX = -0.18, HandsBraceY = -0.12, HandsBraceZ = -0.42; // m (round 2: up and in, above the HP card)
        public const double HandsBracePitchDeg = -35, HandsBraceYawDeg = -20, HandsBraceRollDeg = 20; // deg: fingers to the floor

        // a shot: every movement pose (sprint, boost lean, the swing) yields: out in YieldTime, held off, back over ReturnTime
        public const double HandsPoseYieldTime = 0.05; // s (ease-out from the frame of the shot)
        public const double HandsPoseShotHold = 0.35; // s held off after the shot (a burst keeps the gun up)
        public const double HandsPoseReturnTime = 0.2; // s back in (smoothstep)

        // --- D-69: the PULSE GAUNTLET's hold sat under the weapon card (H's, bottom right); raised and pulled in, Unity-side
        // (web camera space m, added to viewmodel.kitOffsets.gauntlet; the web has no card there) ----------------------------
        public const double HandsGauntletHoldX = -0.03, HandsGauntletHoldY = 0.08; // m: in, and up to the other guns' height
        // round 6 (n17-r5e horde: at rest the THUNDERCLAP's barrel crossed the screen's centre-right, over the cluster): its kit
        // turned about the rig's origin, the barrel down and right, the body (its silhouette) where it was; its kick unchanged
        public const double HandsThunderclapTurnPitchDeg = -6, HandsThunderclapTurnYawDeg = -10; // deg (three: + up, + left)

        // --- round 3 ("every gun reads dark violet, small, in the same corner"): presence and brighter accents ----------------
        public const double HandsPresenceScale = 1.07, HandsPresenceScaleComet = 1.05; // x every kit rig's scale (~14 % more screen area at hold; the long COMET, already the biggest, 1.05: its idle shift stays out of the centre box)
        public const double HandsAccentValue = 1.3, HandsAccentSat = 1.08; // the family stripe's HSV value / saturation (same hue, capped at 1)
        public const double HandsGemGain = 1.45; // x the core gem's HDR brightness (the web's coreGem 2.4)

        // --- U3-30.10: the kill flourish ("satisfying hits"; the web has none): on a MULTIKILL award (the sim's style
        // window) or a rank-up, the gun in hand plays its own flourish once the shooting pauses; a shot, swing, punch or swap
        // stops it on that frame (Pillar 1); REDUCED MOTION: a short still pose-pop instead ------------------------------------
        public const double HandsFlourishQuiet = 0.35; // s since the last shot / swing / punch (world time)
        public const double HandsFlourishPendingMax = 2.0; // s an armed flourish waits for that pause, then it lapses
        public const double HandsFlourishTime = 0.35; // s: the STITCHER's, the GAUNTLET's, any other's
        public const double HandsFlourishKestrelTime = 0.45, HandsFlourishThunderclapTime = 0.4, HandsFlourishCometTime = 0.45, HandsFlourishLongwaveTime = 0.5; // s
        // the KESTREL: the cylinder spins 2 whole turns (ease-out), the gun twirls a whole turn (in-out cubic), the muzzle lifts
        public const double HandsFlourishKestrelSpinTime = 0.32, HandsFlourishKestrelSpinTurns = 2;
        public const double HandsFlourishKestrelTwirlFrom = 0.05, HandsFlourishKestrelTwirlTime = 0.32; // s
        public const double HandsFlourishKestrelRollDeg = 40; // deg: the wrist roll instead of the twirl below full motion
        public const double HandsFlourishKestrelPitchDeg = 8; // deg
        // the THUNDERCLAP: the pump racked out and home, the muzzle flipped up with it
        public const double HandsFlourishRackFrom = 0.05, HandsFlourishRackTime = 0.22; // s
        public const double HandsFlourishRackPitchDeg = 14, HandsFlourishRackRollDeg = 10, HandsFlourishRackY = 0.02; // deg, deg, m
        // the STITCHER: a flick down, then a decaying chatter of the barrel
        public const double HandsFlourishFlickPitchDeg = 10, HandsFlourishFlickTime = 0.18; // deg, s
        public const double HandsFlourishChatterFrom = 0.1, HandsFlourishChatterRollDeg = 4, HandsFlourishChatterHz = 22; // s, deg, Hz
        // the COMET: a bump on the shoulder and a settle dip
        public const double HandsFlourishBumpTime = 0.25, HandsFlourishBumpY = 0.025, HandsFlourishBumpDipY = 0.012; // s, m, m
        public const double HandsFlourishBumpRollDeg = -8, HandsFlourishBumpPitchDeg = -3; // deg: rolled onto the right shoulder, the muzzle dipping (the long COMET stays out of the centre box)
        // the LONGWAVE: a hum: the coil glows (lerped to the player's cyan, x 1 + HumGlow), a fine vibration, a slight lift
        public const double HandsFlourishHumIn = 0.08, HandsFlourishHumHold = 0.25; // s
        public const double HandsFlourishHumRollDeg = 0.7, HandsFlourishHumHz = 30, HandsFlourishHumPitchDeg = 2; // deg, Hz, deg
        public const double HandsFlourishHumGlow = 2.0; // x the coil's brightness at the hum's peak (x FLASH INTENSITY)
        // the GAUNTLET: two knuckle cracks (the fist clenches twice), rolling one way then the other
        public const double HandsFlourishCrackTime = 0.12, HandsFlourishCrackGap = 0.15; // s
        public const double HandsFlourishCrackRollDeg = 6, HandsFlourishCrackPitchDeg = 4; // deg
        // REDUCED MOTION: a still pose-pop (and any gun without its own: the same shape, longer, x motion)
        public const double HandsFlourishStillTime = 0.2, HandsFlourishStillIn = 0.04, HandsFlourishStillHold = 0.1; // s
        public const double HandsFlourishStillRollDeg = 8, HandsFlourishStillPitchDeg = 5, HandsFlourishStillY = 0.01; // deg, deg, m

        // --- round 3, beyond the ask (D-70): the style aura on the gun in hand (the web's fx/aura.js; port: a cyan ladder) -----
        // An inverted hull: each rig's body mesh again, back faces only, additive, expanded HandsAuraRim m per axis about its
        // bounds' centre; the opaque gun hides its inside, so the glow shows at the silhouette's edge.
        public const float HandsAuraRim = 0.006f; // m per axis
        public const double HandsAuraLow = 0.55, HandsAuraHigh = 2.1; // HDR scale at rank 1 / at the top rank
        public const double HandsAuraPulse = 0.35, HandsAuraPulseHz = 0.9; // the top rank's slow pulse (x 1 +- this)

        // --- the fist (round 1: "a tan plank, not a fist"): config.viewmodel.arm's rows, Unity-side: the fist block scaled up,
        // the forearm thinned, knuckle ridges on the fist's top-front edge and two finger folds across its front --------------
        public const float HandsFistScaleX = 1.4f, HandsFistScaleY = 1.35f, HandsFistScaleZ = 1.2f; // x the web's fist (0.075 x 0.07 x 0.085)
        public const float HandsForearmThin = 0.72f; // x the forearm's cross-section
        public const float HandsCuffOver = 1.14f; // the cuff, x the thinned forearm's cross-section
        public const float HandsKnuckleRidgeH = 0.018f, HandsKnuckleRidgeD = 0.016f; // m, each of the 4 ridges' height and depth
        public const float HandsKnuckleRidgeProud = 0.004f; // m the ridges stand off the fist's front face
        public const float HandsFingerFoldY1 = -0.006f, HandsFingerFoldY2 = -0.026f; // m, the folds' heights on the fist's front
        public const float HandsFingerFold = 0.004f; // m, a fold's thickness
        // round 5 (n17-strip-slide: "a forearm the floor's brown"): the sleeve in the hands' ink (ui.theme.panel, mixed this far
        // toward BlockyKit.Dark so its lit faces stay dark violet) with a cyan edge (colors.usable) along its inner top corner
        public const float HandsSleeveInkMix = 0.5f;
        public const float HandsSleeveEdge = 0.009f; // m, the edge strip's width and height (it stands half proud of the corner)
        // the slide's brace (round 1) is OFF: its forearm filled the left quarter of the screen (round 5); the pose stays in the code
        public const bool HandsBraceOn = false;
        // the QUICKDRAW strip along the gun is OFF (round 5: it floated off the gun and went dark: "a clipping bug"); QUICKDRAW
        // shows on the gem (HandsQuickdrawGem)
        public const bool HandsQuickdrawStrip = false;

        // --- breathing: the web's breath is a y sine (breathAmp / breathPeriod); a pitch with it and a slower roll ----------
        public const double HandsBreathPitchDeg = 0.35; // deg, in step with the web's rise (a quarter period behind)
        public const double HandsBreathRollDeg = 0.22; // deg, on a slower cycle
        public const double HandsBreathRollPeriodMult = 1.6; // x viewmodel.breathPeriod
        public const double HandsBreathX = 0.0015; // m of drift on the slow cycle

        // --- the viewmodel trace (HandsDraw.WriteTrace: Logs/n16-vm-trace.csv) -----------------------------------------
        public const double HandsTraceFps = 144; // the render rate the trace samples (the web's feelTrace ran at 144 too)
        public const double HandsTraceAfterSeconds = 0.6; // s traced after each swap's last step (the settle)
        public const string HandsTraceFile = "n16-vm-trace.csv"; // under <project>/Logs/
    }
}
