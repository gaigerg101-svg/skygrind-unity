// The camera side of the feel, after skygrind/src/player/player.js render(): interpolated feet + eased eye height,
// the landing dip, the §3.5 FOV (speed ramp, slide, dash kick, swing lift), roll (strafe lean, wall ride, slide
// tilt) and pitch kicks (jump, wall jump, boost jump up; slam landing down). No head bob (the web has none).
// NIGHT12 U3-25.4, on once Wire hands over the events and the config (SurvivalGame; the Arena doesn't): the aim-lock FOV
// pinch (player.js :197-201, :229-231: aim.lock.pinchDeg over aim.lock.pinchTime, 30% of it the attack; none with reduced
// motion or effects at 0), the wall ride's pre-tilt (:240-270 wallLead, NIGHT5 F5: airborne and closing on a wall you could
// ride, two rays from wallRide.leadHeight reaching as far as you travel toward it in wallRide.leadTime; the roll starts
// before the contact) and the speed-driven FOV (the ramp and the slide) capped at camera.speedFovCapDeg (:228, NIGHT5 F5).
// The dash edge tint is MovementFx's (a canvas under the HUD); the speed lines are the HUD's.
// NIGHT16: the multikill / rank-up FOV punch and the melee lunge (feel.fovPunch, feel.meleeLunge) are ported in
// ImpactFrames.cs (player.js :166-212, one owner for the FOV kicks); the rank-up kick (:207-210) is here (NIGHT16). Not ported:
// the barrier-touch pinch.
// NIGHT17 U3-30.5 (builder X, UnityTuning.Crunch.cs): the crunch shake (CrunchShake below): a rotational trauma shake on
// top of the web's feel.trauma (whose kill and fire amounts square to under 0.05 deg): trauma^2 x smooth noise (Cosmetic's
// value noise) on pitch / yaw (<= 2 deg) and roll (<= 3 deg), never a translation, tuned per weapon class (a light gun's
// small and quick, a heavy gun's big and slow, the punch's sharp: fast noise, a fast fall), per kill kind (a kill, a
// headshot kill, a punch kill) and for a parry and a slam; its onset turns the view on the freeze's first frame (sampled
// before it steps). Frame time (it plays through a freeze), x EFFECTS INTENSITY, stilled by REDUCED MOTION. And the
// sprint's cue (StaminaHud.Info.sprinting): a 3 deg FOV widen eased in over about 0.4 s (the boost's kick is 10 deg in 50 ms).
// NIGHT18 U3-31.5 (builder M; NIGHT18.md M6-M7; FeelAB off = NIGHT17's path and numbers): the widen is one composition
// (CameraMove.Widen): the boost's kick whole on its first frame and back over 0.36 s, with the speed ramp held still while
// dashing (NIGHT17's boost peaked near +17 deg as the dash's 24 m/s ramped on top); the sprint a 5.95 deg floor under the ramp
// from the press (one key: the boost, then the sprint); the slide 7 deg on top within 100 ms; the larger of the kick and the
// rest shows. The slide's roll eases in fast (4 deg, 3.7 at 100 ms); the strafe roll is Quake's at 2.4 deg for a walk
// sideways; the landing dip is at most 0.145 m and within 10 % of the rest by ~0.19 s, with a pitch nod (round 1). FOV KICK % scales every FOV kick, CAMERA SHAKE % every
// shake (both sides; 100 % by default), and REDUCED MOTION stills every FOV kick (FeelAB on; NIGHT17 stilled some). The
// Mover's boost, slide and landing are the reel's `boost`, `slide`, `land` moments (FeelProbe.Event).
// NIGHT19 U3-33.5 (builder V): the KESTREL's gust widens the view on its step (GustFxMath.FovKick: UnityTuning.GustFovKickDeg
// whole on its first frame, back over GustFovBackTime, x GustFovAirMult in the air), in the one composition, x EFFECTS
// INTENSITY x FOV KICK %, none with REDUCED MOTION.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class CameraFeel
    {
        readonly MovementConfig config;
        readonly Mover mover;
        readonly KickStack kicks;
        readonly KickStack pinch = new KickStack(2); // player.js :123 createKickStack(2)
        double eye;
        double speedFov;
        double slideFov;
        double dashKick;
        double rollRight;
        double dipAmount;
        double dipTime = double.PositiveInfinity;
        bool wired;
        double pinchDeg, pinchTime, leadTime, leadHeight, speedFovCap = double.PositiveInfinity;

        public double Fov { get; private set; }

        // NIGHT26 (builder M): the wall ride's FOV lift (eased in while riding, out after) and the lean's own angle (design.n26.wallRide)
        bool wall26;
        double wall26FovDeg, wall26FovEase, wall26RollDeg, wall26Lift;
        /// <summary>The wall ride's current FOV lift in degrees (0 with the row off): the scenario `wallride` reads it.</summary>
        public double WallLiftDeg => wall26Lift;

        public CameraFeel(MovementConfig config, Mover mover)
        {
            this.config = config;
            this.mover = mover;
            kicks = new KickStack(config.feel.kickSlots);
            eye = config.body.eyeHeight;
            eye63 = new Eye63Follow(config.body.eyeHeight); // NIGHT63 eye63 (Eye63Math.N63D.cs): the eased eye's state
            mover.Events += OnEvent;
        }

        /// <summary>NIGHT12: the pinch, the pre-tilt and the speed-FOV cap (their numbers are in config.json, not MovementConfig).</summary>
        public void Wire(EventBus events, GameConfig cfg)
        {
            wired = true;
            cfg72 = cfg; // NIGHT72 N4: cam72 reads its row from it every frame
            // NIGHT63 eye63 (builder D, DECIDE63 D-10; a VIEW row): decided once here (its config and the command line), read every frame in Apply
            bool eye63Row = NightRows.ForceOffFrom > Eye63Math.Night && cfg.Design("n63." + Eye63Math.Row + ".enabled"); // the config's own answer (D1's override is per frame)
            eye63On = Eye63Math.Decide(eye63Row, Eye63Math.Args());
            Debug.Log(Eye63Math.Line(eye63On, eye63Row));
            pinchDeg = cfg.Num("aim.lock.pinchDeg");
            pinchTime = cfg.Num("aim.lock.pinchTime");
            leadTime = cfg.Num("wallRide.leadTime");
            leadHeight = cfg.Num("wallRide.leadHeight");
            speedFovCap = cfg.Num("camera.speedFovCapDeg");
            // NIGHT26 (builder M, design.n26.wallRide): a FOV lift and a steeper lean while wall riding; off (--off26): the web's roll, no lift
            wall26 = cfg.Design("n26.wallRide.enabled") && !N26Blocks.ForceOff;
            if (wall26)
            {
                wall26FovDeg = cfg.Num("design.n26.wallRide.fovLiftDeg");
                wall26FovEase = cfg.Num("design.n26.wallRide.fovEaseRate");
                wall26RollDeg = cfg.Num("design.n26.wallRide.rollDeg");
            }
            // POLISH V1: a lock taking hold pinches the FOV a touch (recover includes the attack: 80 ms in all)
            events.On("aimLock", _ =>
            {
                if (!config.settings.reducedMotion && config.settings.effectsIntensity > 0) pinch.Add(pinchDeg, pinchTime * 0.3, pinchTime);
            });
            // NIGHT16 polish (a web port): player.js :207-210, a rank-up (SPEC 2.0 section 7, Phase 5): a short upward kick
            // (style.kickDeg over kickAttack / kickRecover; the FOV half of that handler is ImpactFrames'). The port's
            // `rankChange` carries the rank only (StyleMeter.cs:61), so "up" is a rank above the last one seen.
            rankKickDeg = cfg.Num("style.kickDeg");
            rankKickAttack = cfg.Num("style.kickAttack");
            rankKickRecover = cfg.Num("style.kickRecover");
            events.On("rankChange", OnRankChange);
            crunch.Wire(events, cfg); // NIGHT17 U3-30.5
            juice38.Wire(events, cfg); // NIGHT38 H (design.n38.juice38, Juice38.cs): the boss entrance / big move / damage cues; nothing with the row off
            // NIGHT19 U3-33.5 (builder V): the KESTREL's gust widens the view on its step (GustFxMath.FovKick), through this
            // file's one composition (x EFFECTS INTENSITY x FOV KICK %; REDUCED MOTION stills it)
            events.On<GustEvent>(N19Events.Gust, e =>
            {
                if (e == null || e.owner != "player") return;
                gustAge = 0;
                gustAir = e.airborne;
                gustMult = 1;
            });
            // NIGHT20 U3-34.3 (builder V): the THUNDERCLAP's shockwave kicks the view the gust's way, x ShockFovMult
            events.On(N20Events.Shockwave, o =>
            {
                if (!(o is ShockwaveEvent e) || e.owner != "player") return;
                gustAge = 0;
                gustAir = !mover.grounded;
                gustMult = UnityTuning.ShockFovMult;
            });
        }

        double gustAge = double.PositiveInfinity; // NIGHT19: s since the last gust (its FOV kick)
        bool gustAir;
        double gustMult = 1; // NIGHT20: 1 for a gust, ShockFovMult for the shockwave

        /// <summary>
        /// NIGHT18 (the shared commit; builder M owns the body): every FOV kick's scale, the FOV KICK setting (0-1). The
        /// composition point for every kick, the impact layer's included (ImpactFrames.Render). NIGHT18 U3-31.5: with FeelAB on,
        /// REDUCED MOTION stills every FOV kick (NIGHT17 stilled the boost's fill, the slide's cue and the pinch only, and kept
        /// the rest); the setting itself scales on both sides (100 % by default: the A / B is unchanged by it).
        /// </summary>
        public static double FovKickScale(GameSettings s) =>
            s == null ? 1 : CameraMove.KickScale(s.Num(UnityTuning.FovKickSetting), FeelAB.On && s.Bool(UnityTuning.ReducedMotionKey));

        /// <summary>NIGHT18 (the shared commit; builder M owns the body): every camera shake's scale, the CAMERA SHAKE setting
        /// (0-1; REDUCED MOTION already stills the trauma shake in the loop and the crunch shake here).</summary>
        public static double ShakeScale(GameSettings s) => s == null ? 1 : CameraMove.KickScale(s.Num(UnityTuning.CameraShakeSetting), false);

        // NIGHT18 U3-31.5 (builder M): the boost's kick, the run's and the slide's widen as one composition (CameraMove.Widen),
        // and the landing dip on its own clock (both FeelAB sides advance every frame; FeelAB.On picks what shows).
        CameraMove.Fov fov18 = CameraMove.Fov.Rest;
        double land18Age = double.MaxValue, land18Depth;

        /// <summary>NIGHT17 U3-30.5: the crunch shake (on top of the web's trauma shake).</summary>
        public readonly CrunchShake crunch = new CrunchShake();
        /// <summary>NIGHT38 H: juice38's boss / hurt cues (its shake joins the composition below; its flicker and static feed Crt29's pass).</summary>
        public readonly Juice38 juice38 = new Juice38();
        double sprintFov;
        double slideCue; // NIGHT17 round 3: the slide's own widen (past the speed cap, which the slide's web FOV sits under)
        // NIGHT17 round 4 (the reviewer: the camera should drop ~0.5 m in 80 ms and roll ~4 deg): on top of the web's eye ease
        // (slide.cameraDrop 0.7 m over camera.eyeEaseTime) and its 3 deg slide tilt (eased at feel.rollEaseRate), a fast
        // extra dip on the slide's start and an extra roll eased in quickly while sliding; visual only (the sim's eye is the
        // mover's), x EFFECTS INTENSITY, none with REDUCED MOTION.
        double slideDipAge = double.MaxValue, slideRoll;
        // NIGHT63 eye63 (builder D, DECIDE63 D-10, design.n63.eye63; Eye63Math.N63D.cs): the eased eye's follow, the row's decision (Wire) and
        // the proof line's once
        readonly Eye63Follow eye63;
        bool eye63On, eye63Fired;
        StaminaHud.Info stamina;
        // NIGHT72 N4 (design.n72.cam72, Cam72Math.cs; a VIEW row): ULTRAKILL's camera. Its three states step EVERY frame whether the row reads on or off (D1's contract: the next frame on is the
        // frame it would have drawn); the row is read every frame through NightRows.ViewOn. cam72Now is this frame's answer.
        GameConfig cfg72;
        bool cam72Now;
        double roll72, roll72Vel, fov72Off, drop72, dash72Dot = 1, strafe72;
        /// <summary>What cam72 did this frame (the Player log's [cam72] lines and the reel read it): the FOV factor now, the eye's drop, the roll, and whether the row was on.</summary>
        public double Cam72Roll => roll72;
        public double Cam72FovOffset => fov72Off;
        public double Cam72Drop => drop72;
        public bool Cam72On => cam72Now;

        double rankKickDeg, rankKickAttack, rankKickRecover;
        int rankLast;

        /// <summary>How many rank-up kicks were added (a test reads it).</summary>
        public int RankUpKicks { get; private set; }

        void OnRankChange(object o)
        {
            int rank = o is int r ? r : rankLast;
            bool up = rank > rankLast;
            rankLast = rank;
            if (!up) return;
            kicks.Add(rankKickDeg, rankKickAttack, rankKickRecover);
            RankUpKicks++;
        }

        void OnEvent(MoverEvent e)
        {
            FeelConfig f = config.feel;
            switch (e.type)
            {
                case MoverEventType.Land:
                    dipAmount = Math.Min(f.landingDipMax, f.landingDipPerSpeed * e.fallSpeed);
                    dipTime = 0;
                    land18Depth = CameraMove.LandDepth(e.fallSpeed, UnityTuning.LandDipPerSpeedN18, UnityTuning.LandDipMaxN18); // NIGHT18
                    // NIGHT18 round 1: a short pitch nod down by fall speed (the reel looks 20 deg down at the landing)
                    if (FeelAB.On && wired && !config.settings.reducedMotion)
                    {
                        double nod = CameraMove.LandNod(e.fallSpeed);
                        if (nod > 0) kicks.Add(-nod, UnityTuning.LandNodAttack, UnityTuning.LandNodRecover);
                    }
                    land18Age = 0;
                    FeelProbe.Event(UnityTuning.ProbeLand);
                    break;
                case MoverEventType.Dash: // NIGHT18: the boost's kick (a partial boost kicks less)
                    CameraMove.Boost(ref fov18, CameraMove.BoostPeak(UnityTuning.BoostFovKickDegN18, mover.stamina.rules.Any ? mover.stamina.lastBoost : 1, UnityTuning.BoostFovPartialMin));
                    FeelProbe.Event(UnityTuning.ProbeBoost);
                    break;
                case MoverEventType.Jump: // ground, coyote and slide jumps
                case MoverEventType.WallJump:
                case MoverEventType.BoostJump:
                    kicks.Add(f.jumpKickDeg, f.jumpKickAttack, f.jumpKickRecover);
                    break;
                case MoverEventType.Slam:
                    kicks.Add(-f.slamPunchDeg, f.slamPunchAttack, f.slamPunchRecover);
                    break;
                case MoverEventType.SlideStart:
                    slideDipAge = 0; // NIGHT17 round 4: the slide's entry dip
                    FeelProbe.Event(UnityTuning.ProbeSlide);
                    break;
            }
        }

        /// <summary>A respawn is a cut: the camera feel goes neutral at once.</summary>
        public void Cut()
        {
            eye = config.body.eyeHeight;
            eye63.Cut(config.body.eyeHeight); // NIGHT63 eye63: the standing eye, at rest
            dipTime = double.PositiveInfinity;
            speedFov = slideFov = dashKick = rollRight = 0;
            roll72 = roll72Vel = fov72Off = drop72 = 0; // NIGHT72 N4: the cut is neutral
            sprintFov = 0;
            slideCue = 0;
            slideDipAge = double.MaxValue;
            slideRoll = 0;
            fov18 = CameraMove.Fov.Rest; // NIGHT18
            land18Age = double.MaxValue;
            gustAge = double.PositiveInfinity; // NIGHT19
            crunch.Clear();
            juice38.Clear(); // NIGHT38 H
            kicks.Clear();
            pinch.Clear();
            rankLast = 0; // NIGHT16: a respawn starts a run: the meter starts at the lowest rank
        }

        /// <summary>Camera for this frame. `drawn` is the feet interpolated between the last two steps.</summary>
        public void Apply(Camera camera, Vec3 drawn, double frameDt, double yaw, double pitch, (double pitch, double yaw, double roll) shakeDeg = default)
        {
            MovementConfig c = config;
            double intensity = Clamp(c.settings.effectsIntensity, 0, c.feel.effectsIntensityMax);
            bool dashing = mover.state == MoveState.Dash;
            cam72Now = cfg72 != null && NightRows.ViewOn(cfg72, Cam72Math.Row); // NIGHT72 N4: read every frame (D1); the states below step either way
            Cam72Step(frameDt, yaw);

            double standEye = c.body.eyeHeight;
            double slideEye = standEye - c.slide.cameraDrop;
            double target = mover.crouched ? slideEye : standEye;
            eye = MoveTowards(eye, target, ((standEye - slideEye) / c.camera.eyeEaseTime) * frameDt);

            // NIGHT18 U3-31.5: the landing dip at most LandDipMax, back by 0.18 s (NIGHT17: the web's, back by 0.26 s); both clocks run
            double dip17 = LandingDip(frameDt);
            if (land18Age < double.MaxValue) land18Age = land18Age >= UnityTuning.LandDipAttackN18 + UnityTuning.LandDipBackN18 ? double.MaxValue : land18Age + frameDt;
            double dip18 = land18Age < double.MaxValue ? CameraMove.LandDip(land18Age, land18Depth, UnityTuning.LandDipAttackN18, UnityTuning.LandDipBackN18) : 0;
            double dip = (FeelAB.On ? dip18 : dip17) * intensity;
            double dipLand72 = dip; // NIGHT72 N4: the landing's share (cam72 keeps it; the slide's own dip below is not ULTRAKILL's)
            // NIGHT17 round 4: the slide's entry dip (in UnityTuning.SlideDipIn s, back over SlideDipOut s)
            if (slideDipAge < double.MaxValue) slideDipAge += frameDt;
            if (wired && !c.settings.reducedMotion && slideDipAge < UnityTuning.SlideDipIn + UnityTuning.SlideDipOut)
                dip += UnityTuning.SlideDip * CrunchMath.Tilt((float)slideDipAge, (float)UnityTuning.SlideDipIn, (float)UnityTuning.SlideDipOut) * intensity;
            // NIGHT63 eye63 (builder D, DECIDE63 D-10; Eye63Math.N63D.cs): the eased eye follows today's eye less the dips through a critically
            // damped spring. D1's contract: it steps every frame whether the row is on or off, the row is read every frame, off draws today's
            // exact expression. View only: the sim's eye height is not this.
            double eye63Y = eye63.Step(target, standEye - slideEye, c.camera.eyeEaseTime, dip, frameDt);
            bool eye63Live = NightRows.Live(eye63On, Eye63Math.Row);
            if (eye63Live && !eye63Fired && Math.Abs(eye63Y - (eye - dip)) >= UnityTuning.Eye63FiredM)
            {
                eye63Fired = true;
                Debug.Log(Eye63Math.FiredLine(eye63Y, eye - dip, Time.frameCount));
            }
            double eyeY72 = 0;
            if (cam72Now)
            {
                // NIGHT72 N4: ULTRAKILL's slide drop (0.32 m, fast), never above a ceiling over the slide (the slide capsule is 1.0 m: Cam72Math.EyeY)
                double ceilM = double.PositiveInfinity;
                if (drop72 > 0.001 && mover.World != null && mover.World.RayIntersect(new Vec3(drawn.x, drawn.y + UnityTuning.Cam72CeilingRayFromM, drawn.z), new Vec3(0, 1, 0), out RayHit up))
                    ceilM = UnityTuning.Cam72CeilingRayFromM + up.distance;
                eyeY72 = Cam72Math.EyeY(standEye, drop72, ceilM) - dipLand72;
            }
            camera.transform.position = WebSpace.ToUnity(new Vec3(drawn.x, cam72Now ? drawn.y + eyeY72 : eye63Live ? drawn.y + eye63Y : drawn.y + eye - dip, drawn.z));

            UpdateFov(camera, frameDt, intensity, dashing);
            UpdateRoll(frameDt, intensity, yaw);
            kicks.Advance(frameDt);
            double limit = c.mouse.pitchLimitDeg * Math.PI / 180;
            // NIGHT17 U3-30.5: the crunch shake on top of the web's, sampled before it steps (the onset lands on this frame)
            // NIGHT18: CAMERA SHAKE % scales both (the web's trauma arrives scaled by EFFECTS INTENSITY and REDUCED MOTION)
            double calm = c.settings.reducedMotion ? 0 : 1;
            double shake = wired ? ShakeScale(SurvivalGame.Instance != null ? SurvivalGame.Instance.settings : null) : 1;
            var (cp, cy, cr) = crunch.Sample(intensity * calm * shake);
            crunch.Step(frameDt);
            var (jp, jy, jr) = juice38.Step(frameDt, intensity * calm * shake); // NIGHT38 H: (0, 0, 0) with juice38 off
            shakeDeg = (shakeDeg.pitch * shake + cp + jp, shakeDeg.yaw * shake + cy + jy, shakeDeg.roll * shake + cr + jr);
            // NIGHT8 U2-06: the trauma shake (player.js), drawn only: pitch and yaw added after the kicks, roll against rollRight.
            double kicked = Clamp(pitch + (kicks.Value() * intensity + shakeDeg.pitch) * Math.PI / 180, -limit, limit);
            // NIGHT17 round 4: the slide's extra roll (leaning left, as the web's slide tilt), eased in at SlideRollEase
            // NIGHT18 U3-31.5: SlideRollExtra on the web's 3 deg tilt, both at SlideRollEase (3.7 deg at 100 ms, 4 deg held)
            bool rollSlide = wired && mover.state == MoveState.Slide && !c.settings.reducedMotion;
            double extra = UnityTuning.Pick(FeelAB.On, UnityTuning.SlideRollExtraN18, UnityTuning.SlideRollExtraN17);
            double extraEase = UnityTuning.Pick(FeelAB.On, UnityTuning.SlideRollEaseN18, UnityTuning.SlideRollEase);
            slideRoll = CameraMove.RollStep(slideRoll, rollSlide && !cam72Now ? -extra * intensity : 0, extraEase, frameDt); // NIGHT72 N4: cam72's slide roll is the strafe's (Cam72Step)
            camera.transform.rotation = WebSpace.LookRotation(yaw + shakeDeg.yaw * Math.PI / 180, kicked, rollRight + slideRoll - shakeDeg.roll);
            if (wired) Cam72Log(camera, drawn.y, standEye);
        }

        void UpdateFov(Camera camera, double frameDt, double intensity, bool dashing)
        {
            CameraConfig cam = config.camera;
            double baseFov = config.settings.fovDeg;
            double ramp = Clamp01((mover.Speed - cam.fovBoostStartSpeed) / (cam.fovBoostFullSpeed - cam.fovBoostStartSpeed));
            speedFov += (ramp * cam.fovBoostDeg - speedFov) * EaseFactor(cam.fovEaseRate, frameDt);
            bool sliding = mover.state == MoveState.Slide;
            slideFov += ((sliding ? config.feel.slideFovDeg : 0) - slideFov) * EaseFactor(cam.fovEaseRate, frameDt);
            double kickTime = dashing ? cam.dashFovInTime : cam.dashFovOutTime;
            dashKick = MoveTowards(dashKick, dashing ? 1 : 0, frameDt / kickTime);
            double swingLift = mover.state == MoveState.Swing ? config.arcline.fovLiftDeg * Clamp01(mover.velocity.Length / config.arcline.softCap) : 0;
            // NIGHT5 F5: the speed-driven part (the ramp and the slide) stays within speedFovCapDeg; the dash kick rides on top
            // NIGHT17 U3-30.5: the sprint's widen (StaminaHud: the one place views read the bar), steadier and weaker than the boost
            bool sprinting = false;
            SurvivalGame g = wired ? SurvivalGame.Instance : null;
            if (g != null)
            {
                StaminaHud.Read(g, ref stamina);
                sprinting = stamina.valid && stamina.sprinting && !dashing;
            }
            sprintFov += ((sprinting ? UnityTuning.SprintFovDeg : 0) - sprintFov) * EaseFactor(UnityTuning.SprintFovEase, frameDt);
            // NIGHT17 round 3 (the reviewer: the slide had no cue of its own): the slide widens UnityTuning.SlideFovDeg more,
            // eased in at SlideFovEase, past the speed cap (the web's slideFovDeg sits under the cap, which the slide's speed fills)
            bool slideCueOn = wired && sliding && !config.settings.reducedMotion;
            slideCue += ((slideCueOn ? UnityTuning.SlideFovDeg : 0) - slideCue) * EaseFactor(UnityTuning.SlideFovEase, frameDt);
            // NIGHT17 round 5 (the r4 reviewer: the boost and the sprint showed nothing): the boost's kick is whole from its first
            // frame (the web's dashKick eases in over camera.dashFovInTime: the rest is filled at once while boosting), and the
            // sprint's widen (UnityTuning.SprintFovDeg) eases in at SprintFovEase as the kick eases out (~200 ms to +5 deg)
            double boostFill = dashing && wired && !config.settings.reducedMotion ? (1 - dashKick) * cam.dashFovKickDeg : 0;
            double boost = Math.Min(speedFov + slideFov, speedFovCap) + dashKick * cam.dashFovKickDeg + boostFill + swingLift + sprintFov + slideCue;
            // NIGHT18 U3-31.5 (M6): one composition (CameraMove.Widen): the boost's kick (+11.8 deg whole on its first frame, back
            // over 0.36 s; the ramp held still while dashing), the run's widen (the sprint's 5.95 deg floor under the ramp, eased
            // from the press when the key stays held: the sprint follows the boost), the slide's 7 deg on top within 100 ms.
            // It runs every frame (both sides stay warm for F6); FeelAB.On picks it.
            var fin = new CameraMove.FovIn
            {
                speedDeg = ramp * cam.fovBoostDeg,
                speedRate = cam.fovEaseRate,
                speedCap = speedFovCap,
                swingDeg = swingLift,
                dt = frameDt,
                dashing = dashing,
                sprint = stamina.valid && (stamina.sprinting || (stamina.sprintArmed && stamina.boosting)),
                sliding = sliding,
            };
            double widen18 = CameraMove.Widen(ref fov18, fin);
            double kickScale = wired ? FovKickScale(g != null ? g.settings : null) : 1; // NIGHT18: FOV KICK % (and REDUCED MOTION with FeelAB on)
            pinch.Advance(frameDt);
            // NIGHT19 U3-33.5: the gust's kick on top, whole on its first frame (none with REDUCED MOTION, on either FeelAB side)
            double gust = wired && !config.settings.reducedMotion ? GustFxMath.FovKick(gustAge, gustAir) * gustMult : 0;
            if (gustAge < double.PositiveInfinity) gustAge = gustAge >= UnityTuning.GustFovBackTime ? double.PositiveInfinity : gustAge + frameDt;
            // NIGHT26 (builder M): the wall ride's lift, eased toward its target (none with REDUCED MOTION), on top within the same cap
            double wallTarget = wall26 && wired && mover.state == MoveState.WallRide && !config.settings.reducedMotion ? wall26FovDeg : 0;
            wall26Lift += (wallTarget - wall26Lift) * EaseFactor(wall26FovEase > 0 ? wall26FovEase : cam.fovEaseRate, frameDt);
            double wall26Add = wall26Lift * kickScale;
            Fov = FeelAB.On
                ? Math.Min(baseFov + (widen18 + gust) * intensity * kickScale + wall26Add * intensity, baseFov + cam.maxFovBoostDeg) - pinch.Value() * kickScale
                : Math.Min(baseFov + (boost + gust) * intensity * kickScale + wall26Add * intensity, baseFov + cam.maxFovBoostDeg) - pinch.Value();
            if (cam72Now)
            {
                // NIGHT72 N4 (cam72): ULTRAKILL has no dash kick and no slide widen: the speed ramp (within its cap), the gust and the wall ride's lift stay, the pinch too; a dash or slide narrows
                // the whole field x0.95 forward / widens it x1.1 back (fov72Off, in degrees, Cam72Step)
                Fov = Math.Min(baseFov + (Math.Min(speedFov, speedFovCap) + gust) * intensity * kickScale + wall26Add * intensity, baseFov + cam.maxFovBoostDeg) - pinch.Value() * kickScale + fov72Off;
            }
            camera.fieldOfView = (float)Fov; // vertical, like three.js
        }

        void UpdateRoll(double frameDt, double intensity, double yaw)
        {
            FeelConfig f = config.feel;
            double target = 0, lead = 0;
            if (config.settings.cameraRoll && !config.settings.reducedMotion) // REDUCED MOTION: no camera roll (settings desc)
            {
                double rightX = Math.Cos(yaw);
                double rightZ = -Math.Sin(yaw);
                lead = wired && mover.state == MoveState.Air ? WallLead(yaw) : 0;
                if (mover.state == MoveState.WallRide)
                {
                    // Scales with how far the wall is to your side: no sign flip when facing the wall.
                    double side = mover.wallNormal.x * rightX + mover.wallNormal.z * rightZ;
                    target = side * (wall26 && wall26RollDeg > 0 ? wall26RollDeg : config.wallRide.rollDeg); // wall on the left -> lean right, away from it
                }
                else if (lead != 0)
                {
                    target = lead * (wall26 && wall26RollDeg > 0 ? wall26RollDeg : config.wallRide.rollDeg); // NIGHT5 F5: about to ride: the lean starts before the contact
                }
                else if (mover.state == MoveState.Slide)
                {
                    target = -f.slideTiltDeg; // leans left
                }
                else
                {
                    // NIGHT18 U3-31.5 (M7): Quake's V_CalcRoll at StrafeRollDeg for a walk's speed sideways (NIGHT17: the web's 1.5)
                    double lateral = mover.velocity.x * rightX + mover.velocity.z * rightZ;
                    target = CameraMove.StrafeRoll(lateral, config.ground.walkSpeed, UnityTuning.Pick(FeelAB.On, UnityTuning.StrafeRollDegN18, f.strafeRollDeg));
                }
                target *= intensity;
            }
            if (config.settings.reducedMotion) target = 0;
            // NIGHT18 U3-31.5 (M6): the slide's tilt eases in at SlideRollEase (3.7 deg with the extra at 100 ms; NIGHT17: the web's rate)
            bool fastRoll = FeelAB.On && wired && mover.state == MoveState.Slide;
            // NIGHT36 M (wallgrip36): the ride's lean eases IN and OUT (a critically damped spring: no snap at the attach, none at the release) while the
            // ride or its pre-tilt drives the roll and until it has settled back; the exponential ease starts at full speed on the attach's frame.
            bool rideRoll = mover.Grip36On && (mover.state == MoveState.WallRide || lead != 0);
            if (rideRoll) ride36Roll = true;
            if (ride36Roll && !fastRoll)
            {
                rollRight = CameraMove.SpringStep(rollRight, ref ride36RollVel, target, UnityTuning.Ride36RollSmoothS, frameDt);
                if (!rideRoll && Math.Abs(rollRight - target) < UnityTuning.Ride36RollSettleDeg && Math.Abs(ride36RollVel) < UnityTuning.Ride36RollSettleDeg) ride36Roll = false;
                return;
            }
            ride36Roll = false;
            ride36RollVel = 0;
            if (cam72Now && !rideRoll)
            {
                // NIGHT72 N4 (cam72): the strafe input's roll (x5 dashing / sliding) replaces the web's strafe lean and the slide's fixed tilt; the wall ride's own lean is untouched
                rollRight = roll72;
                return;
            }
            rollRight = CameraMove.RollStep(rollRight, target, fastRoll ? UnityTuning.SlideRollEaseN18 : f.rollEaseRate, frameDt);
        }

        /// <summary>NIGHT72 N4 (cam72, Cam72Math.cs): the three states step every frame, row on or off (D1): the strafe roll, the dash / slide FOV offset, the slide eye drop.</summary>
        void Cam72Step(double frameDt, double yaw)
        {
            bool calm = config.settings.reducedMotion;
            bool dashing = mover.state == MoveState.Dash, sliding = mover.state == MoveState.Slide;
            bool boosted = dashing || sliding;
            double rightX = Math.Cos(yaw), rightZ = -Math.Sin(yaw), fwdX = -Math.Sin(yaw), fwdZ = -Math.Cos(yaw);
            // the strafe input: the step's own move frame when the sim keeps one (a campaign run), else the lateral speed as a share of the walk speed
            SurvivalGame g = wired ? SurvivalGame.Instance : null;
            double strafe;
            if (g != null && g.sim != null && g.sim.campaignOn) strafe = g.sim.moveFrame30.moveX;
            else strafe = Clamp((mover.velocity.x * rightX + mover.velocity.z * rightZ) / Math.Max(1e-6, config.ground.walkSpeed), -1, 1);
            strafe72 = strafe;
            double rollTarget = calm || mover.state == MoveState.WallRide ? 0 : Cam72Math.RollTarget(strafe, boosted);
            roll72 = Cam72Math.RollStep(roll72, ref roll72Vel, rollTarget, boosted, frameDt);
            double sp = Math.Sqrt(mover.velocity.x * mover.velocity.x + mover.velocity.z * mover.velocity.z);
            if (boosted && sp > UnityTuning.Cam72DotMin) dash72Dot = (mover.velocity.x * fwdX + mover.velocity.z * fwdZ) / sp;
            double baseFov = config.settings.fovDeg;
            double factor = Cam72Math.FovFactor(dash72Dot, boosted && sp > UnityTuning.Cam72DotMin && !calm);
            fov72Off = Cam72Math.FovStep(fov72Off, baseFov * (factor - 1), frameDt);
            drop72 = Cam72Math.DropStep(drop72, mover.crouched && !calm ? UnityTuning.Cam72SlideDropM : 0, frameDt);
        }

        // NIGHT72 N4: the Player log's [cam72] lines: the first dash, slide and strafe of a run, measured from the camera itself after the frame is set (the row reads on or off in the line:
        // the same reel with the row off prints today's numbers). Once each; reset by Cut.
        bool cam72LoggedDash, cam72LoggedSlide, cam72LoggedStrafe;
        double cam72HeldDash, cam72HeldSlide, cam72HeldStrafe;

        void Cam72Log(Camera camera, double feetY, double standEye)
        {
            MoveState s = mover.state;
            double dt = Time.unscaledDeltaTime;
            cam72HeldDash = s == MoveState.Dash ? cam72HeldDash + dt : 0;
            cam72HeldSlide = s == MoveState.Slide ? cam72HeldSlide + dt : 0;
            bool strafing = s == MoveState.Ground && Math.Abs(strafe72) > 0.9;
            cam72HeldStrafe = strafing ? cam72HeldStrafe + dt : 0;
            double baseFov = config.settings.fovDeg, fov = camera.fieldOfView;
            if (!cam72LoggedDash && cam72HeldDash > 0.06)
            {
                cam72LoggedDash = true;
                Debug.Log($"[cam72] dash {(dash72Dot >= 0 ? "forward" : "back")}: fov {fov:0.0} against base {baseFov:0.0} (x{fov / baseFov:0.000}; ULTRAKILL x{Cam72Math.FovFactor(dash72Dot, true):0.000}), row {(cam72Now ? "ON" : "off")}, roll {rollRight:0.0} deg, direction dot {dash72Dot:0.00}");
            }
            if (!cam72LoggedSlide && cam72HeldSlide > 0.25)
            {
                cam72LoggedSlide = true;
                double eyeOver = camera.transform.position.y - feetY;
                Debug.Log($"[cam72] slide: eye {eyeOver:0.00} m over the feet (standing {standEye:0.00}, drop {standEye - eyeOver:0.00}; ULTRAKILL {UnityTuning.Cam72SlideDropM:0.00}), roll {rollRight:0.0} deg (strafe x5 = +-{UnityTuning.Cam72RollDeg * UnityTuning.Cam72RollBoostMult:0}), fov {fov:0.0}, row {(cam72Now ? "ON" : "off")}");
            }
            if (!cam72LoggedStrafe && cam72HeldStrafe > 0.3)
            {
                cam72LoggedStrafe = true;
                Debug.Log($"[cam72] strafe: roll {rollRight:0.00} deg (ULTRAKILL +-{UnityTuning.Cam72RollDeg:0.0}), row {(cam72Now ? "ON" : "off")}");
            }
        }

        bool ride36Roll;
        double ride36RollVel;

        /// <summary>
        /// player.js wallLead (NIGHT5 F5, Titanfall's pre-tilt): airborne, closing on a wall you could ride. Two rays from the
        /// chest (wallRide.leadHeight) to your left and right reach as far as you travel toward that side in wallRide.leadTime;
        /// the result is the ride's roll side (the wall normal . right) x how close the contact is (0 far .. 1 touching).
        /// Visual only (the mover's world, read only). The 0.5 m/s floor is player.js's own literal.
        /// </summary>
        double WallLead(double yaw)
        {
            WallRideConfig w = config.wallRide;
            Vec3 v = mover.velocity;
            ICollisionWorld world = mover.World;
            if (world == null || Math.Sqrt(v.x * v.x + v.z * v.z) < w.entrySpeed) return 0;
            double rx = Math.Cos(yaw), rz = -Math.Sin(yaw);
            double r = config.body.capsuleRadius;
            double best = 0;
            for (int s = -1; s <= 1; s += 2)
            {
                double toward = (v.x * rx + v.z * rz) * s; // m/s toward that side
                if (toward <= 0.5) continue;
                var origin = new Vec3(mover.position.x, mover.position.y + leadHeight, mover.position.z);
                if (!world.RayIntersect(origin, new Vec3(rx * s, 0, rz * s), out RayHit hit) || hit.distance > r + toward * leadTime) continue;
                Vec3 n = hit.normal;
                if (Math.Abs(n.y) >= config.surface.wallMaxAbsNormalY) continue;
                double into = v.x * n.x + v.z * n.z;
                double ax = v.x - n.x * into, az = v.z - n.z * into;
                if (Math.Sqrt(ax * ax + az * az) < w.entrySpeed) continue; // it would not become a ride
                double closeness = 1 - Math.Max(0, hit.distance - r) / (toward * leadTime);
                double lead = (n.x * rx + n.z * rz) * closeness;
                if (Math.Abs(lead) > Math.Abs(best)) best = lead;
            }
            return best;
        }

        /// <summary>Landing dip: eases in over landingDipAttack, then springs back over landingDipReturnTime.</summary>
        double LandingDip(double frameDt)
        {
            double attack = config.feel.landingDipAttack;
            double back = config.feel.landingDipReturnTime;
            if (dipTime >= attack + back) return 0;
            dipTime += frameDt;
            if (dipTime < attack) return dipAmount * Math.Sin((Math.PI / 2) * (dipTime / attack));
            double k = 1 - Clamp01((dipTime - attack) / back);
            return dipAmount * k * k * k;
        }

        static double EaseFactor(double rate, double dt) => 1 - Math.Exp(-rate * dt);

        static double MoveTowards(double value, double target, double maxStep) =>
            value + Math.Sign(target - value) * Math.Min(Math.Abs(target - value), maxStep);

        static double Clamp01(double t) => Clamp(t, 0, 1);

        static double Clamp(double v, double lo, double hi) => Math.Max(lo, Math.Min(hi, v));
    }

    /// <summary>
    /// NIGHT17 U3-30.5 (builder X): the crunch's rotational trauma shake (Eiserloh: trauma 0..1, falling linearly; the shake
    /// is trauma^2 x smooth noise per axis; first person: rotation only). Each moment adds its trauma and sets the shake's
    /// character (the noise rate and the fall) when it is the strongest part of what is left; its onset turns the view at once
    /// (pitch up, roll to an alternating side), so the first frame moves. Frame time; drawn only (the aim is taken before it).
    /// </summary>
    public sealed class CrunchShake
    {
        double trauma, time, hz = UnityTuning.ShakeKillHz, fall = UnityTuning.ShakeKillFall, onsetAge = double.MaxValue, onsetSign = 1, onsetRoll = 1;
        readonly Dictionary<string, int> weaponClass = new Dictionary<string, int>(); // 0 light, 1 medium, 2 heavy, -1 none
        GameConfig cfg;
        int lastPunchShot = -1;

        /// <summary>The trauma now (0..1; a Play check reads it).</summary>
        public double Trauma => trauma;

        /// <summary>The moments: a shot by its gun's class, a kill by its kind, a punch hit, a parry, a slam.</summary>
        public void Wire(EventBus events, GameConfig config)
        {
            cfg = config;
            events.On("fire", o =>
            {
                if (!(o is string w)) return;
                switch (ClassOf(w))
                {
                    case 0: Add(UnityTuning.ShakeFireLight, UnityTuning.ShakeFireLightHz, UnityTuning.ShakeFireLightFall, false); break;
                    case 1: Add(UnityTuning.ShakeFireMedium, UnityTuning.ShakeFireMediumHz, UnityTuning.ShakeFireMediumFall, false); break;
                    case 2: Add(UnityTuning.ShakeFireHeavy, UnityTuning.ShakeFireHeavyHz, UnityTuning.ShakeFireHeavyFall, true, false); break; // pitch only: V's spring rolls the gun
                }
            });
            events.On<KillEvent>("kill", e =>
            {
                if (e.owner != "player") return;
                if (e.melee) Add(UnityTuning.ShakePunchKill, UnityTuning.ShakePunchHz, UnityTuning.ShakePunchFall, true);
                else if (e.headshot) Add(UnityTuning.ShakeHeadshotKill, UnityTuning.ShakeHeadshotKillHz, UnityTuning.ShakeHeadshotKillFall, true);
                else Add(Night51.On ? UnityTuning.N51ShakeKill : UnityTuning.ShakeKill, UnityTuning.ShakeKillHz, UnityTuning.ShakeKillFall, true); // NIGHT51 crunch51
            });
            events.On<HitEvent>("hit", e =>
            {
                if (e.owner != "player" || !e.melee || e.killed || e.shot == lastPunchShot) return;
                lastPunchShot = e.shot; // once a swing
                Add(UnityTuning.ShakePunchHit, UnityTuning.ShakePunchHz, UnityTuning.ShakePunchFall, true);
            });
            events.On<HitEvent>("parry", e =>
            {
                if (e.owner == "player") Add(UnityTuning.ShakeParry, UnityTuning.ShakeParryHz, UnityTuning.ShakeParryFall, true);
            });
            events.On("slamUse", _ => Add(UnityTuning.ShakeSlam, UnityTuning.ShakeSlamHz, UnityTuning.ShakeSlamFall, true));
        }

        /// <summary>A gun's weight class (feel.punch.weapons.&lt;w&gt;.cls), looked up once per gun.</summary>
        int ClassOf(string w)
        {
            if (weaponClass.TryGetValue(w, out int c)) return c;
            string cls = cfg != null && cfg.Has($"feel.punch.weapons.{w}.cls") ? cfg.Str($"feel.punch.weapons.{w}.cls") : null;
            c = cls == "light" ? 0 : cls == "medium" ? 1 : cls == "heavy" ? 2 : -1;
            weaponClass[w] = c;
            return c;
        }

        /// <summary>
        /// Add `amount` trauma (capped at 1); the moment sets the noise rate and fall when it is at least half of what is left.
        /// `onset`: the first frame turns at once (pitch up; and roll to an alternating side when `onsetRoll`).
        /// </summary>
        public void Add(double amount, double noiseHz, double fallPerSec, bool onset, bool onsetRoll = true)
        {
            if (!(amount > 0)) return;
            if (trauma <= 0) time = 0;
            if (amount >= trauma * 0.5)
            {
                hz = noiseHz;
                fall = fallPerSec;
            }
            trauma = Math.Min(1, trauma + amount);
            if (!onset) return;
            onsetAge = 0;
            onsetSign = -onsetSign;
            this.onsetRoll = onsetRoll ? 1 : 0;
        }

        /// <summary>The shake now (deg: pitch, yaw, roll), x `scale` (effects intensity, 0 with REDUCED MOTION).</summary>
        public (double pitch, double yaw, double roll) Sample(double scale)
        {
            double share = CrunchMath.ShakeShare(trauma, UnityTuning.CrunchShakeExponent, scale);
            if (share <= 0) return (0, 0, 0);
            double t = time * hz;
            double on = CrunchMath.Onset(onsetAge, UnityTuning.ShakeOnsetTime);
            return (CrunchMath.ShakeDeg(UnityTuning.CrunchShakePitchDeg, share, Cosmetic.ValueNoise(t, 101), on * UnityTuning.ShakeOnsetPitch),
                CrunchMath.ShakeDeg(UnityTuning.CrunchShakeYawDeg, share, Cosmetic.ValueNoise(t, 211), 0),
                CrunchMath.ShakeDeg(UnityTuning.CrunchShakeRollDeg, share, Cosmetic.ValueNoise(t, 307), on * UnityTuning.ShakeOnsetRoll * onsetSign * onsetRoll));
        }

        /// <summary>Fall and move the noise clock (frame time).</summary>
        public void Step(double dt)
        {
            trauma = Math.Max(0, trauma - fall * dt);
            time += dt;
            if (onsetAge < double.MaxValue) onsetAge += dt;
        }

        public void Clear()
        {
            trauma = 0;
            onsetAge = double.MaxValue;
            lastPunchShot = -1;
        }
    }

    /// <summary>
    /// NIGHT18 U3-31.5 (builder M): the movement camera's rules, pure (CameraMoveTests): the boost's kick envelope, the
    /// widen's composition (the run's widen, the slide's, the boost's kick), Quake's strafe roll, the landing dip, the speed
    /// lines' alpha and their clear centre, the stamina bar's trail, the FOV KICK scale. UnityTuning.CameraMove.cs has the numbers.
    /// </summary>
    public static class CameraMove
    {
        /// <summary>The widen's parts between frames (deg; the boost's age in s, double.MaxValue: none).</summary>
        public struct Fov
        {
            public double speed, sprint, slide;
            public double boostAge, boostPeak;
            public bool sprintBeforeSlide;

            public static Fov Rest => new Fov { boostAge = double.MaxValue };
        }

        /// <summary>One frame's inputs: the web's speed ramp target (deg) and its ease (/s) and cap (deg), the swing's lift (deg),
        /// dashing, the sprint's intent, sliding, the frame's time (s).</summary>
        public struct FovIn
        {
            public double speedDeg, speedRate, speedCap, swingDeg, dt;
            public bool dashing, sprint, sliding;
        }

        /// <summary>An exponential ease's share of the gap closed in `dt` s at `rate` /s.</summary>
        public static double Ease(double rate, double dt) => 1 - Math.Exp(-rate * dt);

        /// <summary>smoothstep(0, 1, u).</summary>
        public static double Smooth(double u)
        {
            u = Math.Max(0, Math.Min(1, u));
            return u * u * (3 - 2 * u);
        }

        /// <summary>The boost's kick (0..1) `age` s after it: 1 on its first frame, back to 0 over `release` s on 1 - smoothstep.</summary>
        public static double BoostEnvelope(double age, double release) =>
            age < 0 || age >= release || release <= 0 ? 0 : 1 - Smooth(age / release);

        /// <summary>A boost's kick (deg): the full kick for a full boost, `partialMin` .. 1 of it by a partial boost's share.</summary>
        public static double BoostPeak(double kickDeg, double fraction, double partialMin) =>
            kickDeg * (partialMin + (1 - partialMin) * Math.Max(0, Math.Min(1, fraction)));

        /// <summary>A boost started (the Mover's Dash event): its kick plays from the next frame drawn.</summary>
        public static void Boost(ref Fov s, double peak)
        {
            s.boostAge = 0;
            s.boostPeak = peak;
        }

        /// <summary>
        /// One frame of NIGHT18's widen (deg, before EFFECTS INTENSITY and FOV KICK): the run's widen (the larger of the web's
        /// speed ramp, capped and held still while dashing, and the sprint's floor, which a slide from a sprint keeps), plus the
        /// slide's own part and the swing's lift; the boost's kick shows when it is the larger.
        /// </summary>
        public static double Widen(ref Fov s, in FovIn i)
        {
            if (!i.dashing) s.speed += (i.speedDeg - s.speed) * Ease(i.speedRate, i.dt);
            if (!i.sliding) s.sprintBeforeSlide = i.sprint;
            bool sprint = i.sprint || (i.sliding && s.sprintBeforeSlide);
            s.sprint += ((sprint ? UnityTuning.SprintFovDegN18 : 0) - s.sprint) * Ease(UnityTuning.SprintFovEaseN18, i.dt);
            s.slide += ((i.sliding ? UnityTuning.SlideFovDegN18 : 0) - s.slide) * Ease(UnityTuning.SlideFovEaseN18, i.dt);
            double run = Math.Max(Math.Min(s.speed, i.speedCap), s.sprint);
            double sustained = run + s.slide + i.swingDeg;
            double kick = s.boostAge < double.MaxValue ? s.boostPeak * BoostEnvelope(s.boostAge, UnityTuning.BoostFovRelease) : 0;
            if (s.boostAge < double.MaxValue) s.boostAge = s.boostAge >= UnityTuning.BoostFovRelease ? double.MaxValue : s.boostAge + i.dt;
            return Math.Max(kick, sustained);
        }

        /// <summary>Quake's V_CalcRoll: the lateral speed's share of `fullSpeed` (at most 1 either way) x `maxDeg`, signed.</summary>
        public static double StrafeRoll(double lateral, double fullSpeed, double maxDeg) =>
            fullSpeed <= 0 ? 0 : Math.Max(-1, Math.Min(1, lateral / fullSpeed)) * maxDeg;

        /// <summary>The roll eased toward `target` at `rate` /s over `dt` s.</summary>
        public static double RollStep(double roll, double target, double rate, double dt) => roll + (target - roll) * Ease(rate, dt);

        /// <summary>NIGHT36 M: a critically damped spring toward `target` reaching it in about `smoothTime` s (the standard closed-form approximation of
        /// e^-x, Game Programming Gems 4 ch. 1.10): it starts and stops with zero speed, so a lean eases in and out. `vel` is its state (deg/s).</summary>
        public static double SpringStep(double x, ref double vel, double target, double smoothTime, double dt)
        {
            if (dt <= 0) return x;
            double omega = 2 / Math.Max(1e-4, smoothTime);
            double k = omega * dt;
            double e = 1 / (1 + k + 0.48 * k * k + 0.235 * k * k * k);
            double d = x - target;
            double temp = (vel + omega * d) * dt;
            vel = (vel - omega * temp) * e;
            return target + (d + temp) * e;
        }

        /// <summary>The landing dip's depth (m): `perSpeed` per m/s of the fall, at most `max`.</summary>
        public static double LandDepth(double fallSpeed, double perSpeed, double max) => Math.Min(max, Math.Max(0, perSpeed * fallSpeed));

        /// <summary>NIGHT18 round 1: the landing's pitch nod (deg, down) by fall speed: LandNodDeg at LandNodFullFall m/s or faster,
        /// none under LandNodMinFall.</summary>
        public static double LandNod(double fallSpeed) => fallSpeed < UnityTuning.LandNodMinFall ? 0
            : UnityTuning.LandNodDeg * Math.Min(1, (fallSpeed - UnityTuning.LandNodMinFall) / (UnityTuning.LandNodFullFall - UnityTuning.LandNodMinFall));

        /// <summary>The landing dip (m) `t` s after the landing: in over `attack` (a sine), back over `back` (1 - smoothstep).</summary>
        public static double LandDip(double t, double depth, double attack, double back)
        {
            if (t < 0 || t >= attack + back) return 0;
            if (t < attack) return depth * Math.Sin(Math.PI / 2 * (t / attack));
            return depth * (1 - Smooth((t - attack) / back));
        }

        /// <summary>The speed lines' alpha wanted at `share` of the top speed (0 to the gate, a smoothstep to the max above it),
        /// and the sprint's faint edge lines.</summary>
        public static double LinesTarget(double share, bool sprinting)
        {
            double gate = UnityTuning.LinesGate;
            // round 2: the run layer (the boost layer is LinesBoostTarget's)
            double fast = share <= gate ? 0 : UnityTuning.LinesMaxAlpha * Smooth((share - gate) / (1 - gate));
            return Math.Max(fast, sprinting ? UnityTuning.LinesSprintAlpha : 0);
        }

        /// <summary>The lines' alpha one frame on: toward `target` (in over LinesFadeIn s, out over `fadeOut` s, per whole alpha),
        /// never over LinesQuietAlpha at or under the gate.</summary>
        public static double LinesStep(double shown, double target, double share, double dt, double fadeOut) =>
            LinesStep(shown, target, share, dt, UnityTuning.LinesFadeIn, fadeOut);


        /// <summary>NIGHT18 round 2: the boost layer's alpha wanted: above the top speed (the boost runs at about twice it) up to
        /// LinesBoostAlpha by LinesBoostShare of it; 0 at or under the top.</summary>
        public static double LinesBoostTarget(double share) =>
            share <= 1 ? 0 : UnityTuning.LinesBoostAlpha * Smooth((share - 1) / (UnityTuning.LinesBoostShare - 1));

        /// <summary>LinesStep with its own fade-in (s per whole alpha).</summary>
        public static double LinesStep(double shown, double target, double share, double dt, double fadeIn, double fadeOut)
        {
            double next = shown < target ? Math.Min(target, shown + dt / Math.Max(1e-6, fadeIn)) : Math.Max(target, shown - dt / Math.Max(1e-6, fadeOut));
            return share > UnityTuning.LinesGate ? next : Math.Min(next, UnityTuning.LinesQuietAlpha);
        }

        /// <summary>NIGHT18 round 2: a layer's streak sprite: the boost's (from LinesBoostInner) or the run's (from LinesRunInner, off
        /// the HUD's bottom corners).</summary>
        public static float StreakAlpha(float dx, float dy, bool boost)
        {
            float box = Math.Max(Math.Abs(dx), Math.Abs(dy));
            float inner = boost ? UnityTuning.LinesBoostInner : UnityTuning.LinesRunInner;
            if (box < inner) return 0;
            float mask = 1;
            if (!boost)
            {
                // off the HUD's bottom corners (the HP plate, the ammo and points): 0 where both edges are past their band
                float sx = Math.Max(0, Math.Min(1, (Math.Abs(dx) - UnityTuning.LinesHudX0) / (UnityTuning.LinesHudX1 - UnityTuning.LinesHudX0)));
                float sy = Math.Max(0, Math.Min(1, (-dy - UnityTuning.LinesHudY0) / (UnityTuning.LinesHudY1 - UnityTuning.LinesHudY0)));
                mask = 1 - sx * sx * (3 - 2 * sx) * sy * sy * (3 - 2 * sy);
                if (mask <= 0) return 0;
            }
            int streaks = UnityTuning.LinesStreaksN18;
            float ang = (float)((Math.Atan2(dy, dx) / (2 * Math.PI) + 0.5) * streaks);
            int i = (int)Math.Floor(ang);
            float f = ang - i;
            float center = 0.5f + 0.35f * (float)Cosmetic.Lattice(i, 5); // each streak at its own angle in its bin (NIGHT17's)
            float width = UnityTuning.LinesStreakWidthMin + UnityTuning.LinesStreakWidthSpan * (float)(Cosmetic.Lattice(i, 9) * 0.5 + 0.5);
            float start = inner + (boost ? UnityTuning.LinesBoostSpread : UnityTuning.LinesRunSpread) * (float)(Cosmetic.Lattice(i, 13) * 0.5 + 0.5);
            float line = Math.Max(0, Math.Min(1, 1 - Math.Abs(f - center) / width));
            float u = Math.Max(0, Math.Min(1, (box - start) / UnityTuning.LinesInnerSpan));
            return line * u * u * (3 - 2 * u) * mask;
        }

        /// <summary>NIGHT18 round 3 (final): the boost layer's sprite at texel (x, y) of a w x h texture whose texels are near
        /// square on screen: LinesBoostStreaks thin radial lines (a tent LinesBoostHalfWidth texels each side), each from its
        /// start just past the central box (LinesBoostInner + its share of LinesBoostSpread) to the edge; 0 elsewhere.</summary>
        public static float BoostStreakAlpha(float x, float y, int w, int h)
        {
            float hx = w * 0.5f, hy = h * 0.5f;
            float sx = x + 0.5f - hx, sy = y + 0.5f - hy;
            float box = Math.Max(Math.Abs(sx) / hx, Math.Abs(sy) / hy);
            if (box < UnityTuning.LinesBoostInner) return 0;
            int n = UnityTuning.LinesBoostStreaks;
            double f = (Math.Atan2(sy, sx) / (2 * Math.PI) + 0.5) * n;
            int bin = (int)Math.Floor(f);
            float best = 0;
            for (int k = -1; k <= 1; k++)
            {
                int i = ((bin + k) % n + n) % n;
                // the streak's ray: at its own angle within its bin (no two bunched), through the centre
                double a = ((i + 0.5 + 0.4 * Cosmetic.Lattice(i, 5)) / n - 0.5) * 2 * Math.PI;
                double dx = Math.Cos(a), dy = Math.Sin(a);
                if (sx * dx + sy * dy <= 0) continue; // the ray's own side
                double perp = Math.Abs(sx * dy - sy * dx);
                double half = UnityTuning.LinesBoostHalfWidthMin + UnityTuning.LinesBoostHalfWidthSpan * (Cosmetic.Lattice(i, 9) * 0.5 + 0.5);
                double line = 1 - perp / half;
                if (line <= 0) continue;
                double start = UnityTuning.LinesBoostInner + UnityTuning.LinesBoostSpread * (Cosmetic.Lattice(i, 13) * 0.5 + 0.5);
                double u = Math.Max(0, Math.Min(1, (box - start) / UnityTuning.LinesBoostStartFade));
                best = Math.Max(best, (float)(line * u));
            }
            return best;
        }

        /// <summary>The half-extent (of half the screen's width) the lines leave clear when the image is (1 + shift) wide and
        /// moves up to `shift` sideways.</summary>
        public static float LinesClearHalf(float inner, float shift) => inner * (1 + shift) - shift;

        /// <summary>
        /// NIGHT18 (M8's FeelFx.SpeedLinesClear): the largest screen-aligned box, about the image's centre, inside the streaks'
        /// clear box (`inner` of the image's half-size `halfW` x `halfH` px) when the image is turned `angleDeg` (the jitter):
        /// its half-extents in px.
        /// </summary>
        public static void ClearBox(double halfW, double halfH, double inner, double angleDeg, out double clearHalfW, out double clearHalfH)
        {
            // a corner (p, q) turned into the image's frame: p c + q s <= a and p s + q c <= b (s = |sin|); both met with equality
            double a = inner * halfW, b = inner * halfH, t = angleDeg * Math.PI / 180;
            double c = Math.Cos(t), s = Math.Abs(Math.Sin(t)), det = c * c - s * s;
            clearHalfW = Math.Max(0, (a * c - b * s) / det);
            clearHalfH = Math.Max(0, (b * c - a * s) / det);
        }

        /// <summary>The stamina bar's pale trail one frame on: a rise takes it along; a drain leaves it behind, easing down at
        /// `rate` /s.</summary>
        public static double TrailStep(double trail, double fill, double dt, double rate) =>
            fill >= trail ? fill : fill + (trail - fill) * Math.Exp(-rate * dt);

        /// <summary>FOV KICK's scale: the setting (0..1), 0 when `still` (REDUCED MOTION).</summary>
        public static double KickScale(double setting, bool still) => still ? 0 : Math.Max(0, Math.Min(1, setting));
    }
}
