// NIGHT17 U3-30.6 (builder V): the hands overhaul (GG: "animations, swapping weapons, satisfying hits"; the research's
// ranking items 9 and 12). Port-only: the web has one smoothstep swap, one recoil shape per weight class, a sine punch.
// - the swap in-to is an ease-out-back (HandsEase.OutBack, the weight class's overshoot), at hold exactly on the step the
//   sim opens fire; the holster yanks (an ease-out from the press); a shot in the respawn raise speeds it up (Draw.cs);
// - QUICKDRAW: a cyan strip runs along the gun in hand for the 0.5 s after fire opens on a swap (HandsQuickdraw);
// - a kick per gun (HandsKick: two spring layers each; the revolver snaps up and rolls, the shotgun shoves back hard, the
//   STITCHER chatters small), on its own bank, its attack on frame time inside the shot's freeze (HandsRecoilClock);
// - the punch (HandsPunch): a wind-up at the screen's edge, an accelerating strike that lands past the target on the web's
//   impact frame (punchOutTime), a snap back (the hard stop), a rebound on a hit; the gun takes the body's torque;
// - the sprint carry and the boost lean with its swing (HandsMovePose, from StaminaHud.Read), eased, yielding to a shot;
// - the breath gains a pitch and a slow roll (HandsBreath).
// The sim is untouched: every rule here reads the sim's timers and events and draws; none gates input or fire.
// The pure parts use no UnityEngine call and allocate nothing per frame (HandsTests runs them under dotnet).
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>The eases the hands use (u clamped to 0..1).</summary>
    public static class HandsEase
    {
        static double C(double u) => u <= 0 ? 0 : u >= 1 ? 1 : u;

        /// <summary>Ease-out-back: 0 at 0, 1 at 1, past 1 by 4 s^3 / (27 (s + 1)^2) at u = 1 - 2s / (3 (s + 1)), then back.</summary>
        public static double OutBack(double u, double s)
        {
            double t = C(u) - 1;
            return 1 + (s + 1) * t * t * t + s * t * t;
        }

        /// <summary>The holster's yank: full speed from the press (slope pi / 2 at 0, about the old smoothstep's peak).</summary>
        public static double OutSine(double u) => Math.Sin(Math.PI / 2 * C(u));

        public static double OutQuad(double u)
        {
            double t = 1 - C(u);
            return 1 - t * t;
        }

        public static double OutCubic(double u)
        {
            double t = 1 - C(u);
            return 1 - t * t * t;
        }

        public static double InQuad(double u)
        {
            double t = C(u);
            return t * t;
        }

        /// <summary>The in-to's overshoot for a weight class (viewmodel.js classOf): light bounces, heavy seats.</summary>
        public static double InToBack(string cls) =>
            cls == "light" ? UnityTuning.HandsInToBackLight : cls == "heavy" ? UnityTuning.HandsInToBackHeavy : UnityTuning.HandsInToBackMedium;
    }

    /// <summary>One spring layer of a kick: its spring and its first peaks per channel.</summary>
    public struct HandsKickLayer
    {
        public SpringShape spring;
        public double pitchDeg, backZ, dropY, rollDeg, yawDeg, sideX;
    }

    /// <summary>A gun's kick: one or two layers, the STITCHER's chatter.</summary>
    public sealed class HandsKickSpec
    {
        public string weapon;
        public HandsKickLayer a, b;
        public bool hasB, chatter;
        public double jitter;
    }

    /// <summary>The per-gun kick (UnityTuning.HandsKick*: { freqHz, zeta, pitch, back, drop, roll, yaw, side } per layer).</summary>
    public static class HandsKick
    {
        public const int Channels = 6; // 0 pitch deg, 1 back m, 2 drop m, 3 roll deg, 4 yaw deg, 5 side m

        /// <summary>The gun's kick, or null (the gauntlet's swing is its kick; a rig-less gun keeps the web's recoil).</summary>
        public static HandsKickSpec SpecFor(string weapon)
        {
            switch (weapon)
            {
                case "revolver": return Two(weapon, UnityTuning.HandsKickRevolverA, UnityTuning.HandsKickRevolverB);
                case "shotgun": return Two(weapon, UnityTuning.HandsKickShotgunA, UnityTuning.HandsKickShotgunB);
                case "nailgun":
                    return new HandsKickSpec { weapon = weapon, a = Layer(UnityTuning.HandsKickNailgunA), chatter = true, jitter = UnityTuning.HandsKickNailgunJitter };
                case "rocket": return Two(weapon, UnityTuning.HandsKickRocketA, UnityTuning.HandsKickRocketB);
                case "rail": return Two(weapon, UnityTuning.HandsKickRailA, UnityTuning.HandsKickRailB);
                default: return null;
            }
        }

        static HandsKickSpec Two(string weapon, double[] a, double[] b) => new HandsKickSpec { weapon = weapon, a = Layer(a), b = Layer(b), hasB = true };

        public static HandsKickLayer Layer(double[] row) => new HandsKickLayer
        {
            spring = SpringShape.Of(row[0], row[1]),
            pitchDeg = row[2], backZ = row[3], dropY = row[4], rollDeg = row[5], yawDeg = row[6], sideX = row[7],
        };

        /// <summary>The bank's drop thresholds per channel (a kick is dropped once every channel is under them).</summary>
        public static double[] Snaps() => new[] { 0.01, 0.0001, 0.0001, 0.01, 0.01, 0.0001 };

        /// <summary>A layer's peaks for shot `shot` x `scale` (STEADY, motion): the chatter flips roll / yaw / side on odd
        /// shots; the jitter is x (1 + jitter x the shot's lattice hash) (no random stream).</summary>
        public static void Peaks(in HandsKickLayer l, int shot, bool chatter, double jitter, double scale, double[] into)
        {
            double j = jitter != 0 ? 1 + jitter * Cosmetic.Lattice(shot, 13) : 1;
            double alt = chatter && (shot & 1) == 1 ? -1 : 1;
            double k = scale * j;
            into[0] = l.pitchDeg * k;
            into[1] = l.backZ * k;
            into[2] = l.dropY * k;
            into[3] = l.rollDeg * k * alt;
            into[4] = l.yawDeg * k * alt;
            into[5] = l.sideX * k * alt;
        }

        /// <summary>A shot's kick on `bank` (both layers); `scratch` is Channels long.</summary>
        public static void Add(SpringBank bank, HandsKickSpec s, int shot, double scale, double[] scratch)
        {
            Peaks(s.a, shot, s.chatter, s.jitter, scale, scratch);
            bank.Add(s.a.spring, scratch);
            if (!s.hasB) return;
            Peaks(s.b, shot, s.chatter, s.jitter, scale, scratch);
            bank.Add(s.b.spring, scratch);
        }

        /// <summary>
        /// The kick bank's clock (round 2: "the KESTREL's flip peaks late and is still held at 217 ms"): max(worldDt,
        /// frameDt), so a shot's own kill freeze no longer holds the gun at its kick's peak: the kick plays out in real time
        /// (the KESTREL up in ~17 ms, down by ~100 ms) while the world is frozen or slowed; world time when it runs.
        /// </summary>
        public static double ClockDt(double worldDt, double frameDt) => Math.Max(worldDt, frameDt);

        /// <summary>The kick's attack (frame time inside a freeze): the main layer's first peak.</summary>
        public static double AttackTime(HandsKickSpec s) => s.a.spring.tPeak;
    }

    /// <summary>
    /// The left fist's punch (port-only; the web's is a sine out, a hold and a smoothstep back): rest -> the cocked pose
    /// (ease-out over HandsPunchCockShare of punchOutTime: the wind-up, at the screen's bottom edge) -> the strike
    /// (accelerating, InQuad) to past armPunch at punchOutTime (the impact frame) -> the snap back over HandsPunchSnapTime
    /// (a hit; a whiff's follow-through is deeper and slower), a hit rebounding off the target on top -> the web's return to
    /// rest from punchOutTime + punchHoldTime to punch.cycleTime. Hidden at age 0 and from cycleTime (as the web's arm).
    /// </summary>
    public static class HandsPunch
    {
        /// <summary>
        /// The arm's age on the punch event (round 1: "the punch arrives after the kill"): the sim's punch lands on its
        /// press step (the hit, the kill and the freeze's first frame are that step), so a punch that connects starts AT
        /// its impact frame (punchOutTime: full extension, past the target) and holds it through the freeze; the snap and
        /// the rebound play when the world runs. A whiff winds up and strikes (age 0). The picture catching up to the sim.
        /// </summary>
        public static double StartAge(HandsArmParams a, bool hit) => hit ? a.outTime : 0;

        public static HandsArmPose Pose(HandsArmParams a, double age, bool hit, double px, double py)
        {
            var p = new HandsArmPose();
            if (!(age > 0) || !(age < a.cycleTime))
            {
                p.x = a.restX + px;
                p.y = a.restY + py;
                p.z = a.restZ;
                p.pitchDeg = a.restPitch;
                p.yawDeg = a.restYaw;
                p.rollDeg = a.restRoll;
                return p;
            }
            p.visible = true;
            double cockT = a.outTime * UnityTuning.HandsPunchCockShare;
            double over = hit ? 1 : UnityTuning.HandsPunchWhiffOver;
            double fromX, fromY, fromZ, fromP, fromYaw, fromR, toX, toY, toZ, toP, toYaw, toR, k;
            if (age < cockT)
            {
                // the wind-up: rest to the cocked pose, ease-out (it gets there, then the strike leaves it)
                k = HandsEase.OutCubic(age / cockT);
                fromX = a.restX; fromY = a.restY; fromZ = a.restZ; fromP = a.restPitch; fromYaw = a.restYaw; fromR = a.restRoll;
                toX = UnityTuning.HandsPunchCockX; toY = UnityTuning.HandsPunchCockY; toZ = UnityTuning.HandsPunchCockZ;
                toP = UnityTuning.HandsPunchCockPitchDeg; toYaw = UnityTuning.HandsPunchCockYawDeg; toR = UnityTuning.HandsPunchCockRollDeg;
                p.w = UnityTuning.HandsPunchCockShare * k;
            }
            else if (age < a.outTime)
            {
                // the strike: accelerating into the target, past it by the overshoot on the impact frame
                double u = (age - cockT) / (a.outTime - cockT);
                k = HandsEase.InQuad(u);
                fromX = UnityTuning.HandsPunchCockX; fromY = UnityTuning.HandsPunchCockY; fromZ = UnityTuning.HandsPunchCockZ;
                fromP = UnityTuning.HandsPunchCockPitchDeg; fromYaw = UnityTuning.HandsPunchCockYawDeg; fromR = UnityTuning.HandsPunchCockRollDeg;
                toX = (a.punchX + UnityTuning.HandsPunchAimX); toY = (a.punchY + UnityTuning.HandsPunchAimY); toZ = a.punchZ - UnityTuning.HandsPunchOverZ * over;
                toP = a.punchPitch + UnityTuning.HandsPunchOverPitchDeg * over; toYaw = a.punchYaw; toR = a.punchRoll + UnityTuning.HandsPunchOverRollDeg * over;
                p.w = UnityTuning.HandsPunchCockShare + (1 - UnityTuning.HandsPunchCockShare) * k;
            }
            else
            {
                // the impact frame and after: the overshoot snaps back (fast first: a hard stop), a hit rebounds off the
                // target; the web's return to rest from out + hold
                double t = age - a.outTime;
                double snapT = hit ? UnityTuning.HandsPunchSnapTime : UnityTuning.HandsPunchWhiffSnapTime;
                double s = t < snapT ? 1 - t / snapT : 0;
                double o = s * s * s * over;
                double r = hit && t < UnityTuning.HandsPunchReboundTime + snapT ? Math.Sin(Math.PI * Math.Min(1, t / (UnityTuning.HandsPunchReboundTime + snapT))) : 0;
                double ret = age > a.outTime + a.holdTime ? 1 - HandsDraw.Smooth(HandsDraw.Clamp((age - a.outTime - a.holdTime) / (a.cycleTime - a.outTime - a.holdTime), 0, 1)) : 1;
                fromX = a.restX; fromY = a.restY; fromZ = a.restZ; fromP = a.restPitch; fromYaw = a.restYaw; fromR = a.restRoll;
                toX = (a.punchX + UnityTuning.HandsPunchAimX);
                toY = (a.punchY + UnityTuning.HandsPunchAimY);
                toZ = a.punchZ - UnityTuning.HandsPunchOverZ * o + UnityTuning.HandsPunchReboundZ * r;
                toP = a.punchPitch + UnityTuning.HandsPunchOverPitchDeg * o + UnityTuning.HandsPunchReboundPitchDeg * r;
                toYaw = a.punchYaw;
                toR = a.punchRoll + UnityTuning.HandsPunchOverRollDeg * o;
                k = ret;
                p.w = ret;
            }
            p.x = fromX + (toX - fromX) * k + px;
            p.y = fromY + (toY - fromY) * k + py;
            p.z = fromZ + (toZ - fromZ) * k;
            p.pitchDeg = fromP + (toP - fromP) * k;
            p.yawDeg = fromYaw + (toYaw - fromYaw) * k;
            p.rollDeg = fromR + (toR - fromR) * k;
            return p;
        }

        /// <summary>The same with the free hand on a wall (wallW 0..1) and the wall-jump's push (HandsArm's wall pose): the
        /// punch blends over the wall base by its weight. No wall, no push: exactly Pose(a, age, hit, px, py).</summary>
        /// <summary>The same with the slide's brace (braceW 0..1, the slide pose's weight): the free hand eases from its base
        /// (rest or the wall) to UnityTuning.HandsBrace* (fingers toward the floor, at the screen's bottom-left edge); a punch
        /// blends over it by its weight, as over the wall. braceW 0: exactly the wall variant.</summary>
        public static HandsArmPose Pose(HandsArmParams a, double age, bool hit, double px, double py, double wallW, double pushAge, double braceW, bool jab = false)
        {
            HandsArmPose p = Pose(a, age, hit, px, py, wallW, pushAge, jab);
            double k = HandsDraw.Clamp(braceW, 0, 1);
            if (!(k > 0)) return p;
            bool punching = age > 0 && age < a.cycleTime;
            double free = (1 - (punching ? HandsDraw.Clamp(p.w, 0, 1) : 0)) * k;
            p.x += (UnityTuning.HandsBraceX + px - p.x) * free;
            p.y += (UnityTuning.HandsBraceY + py - p.y) * free;
            p.z += (UnityTuning.HandsBraceZ - p.z) * free;
            p.pitchDeg += (UnityTuning.HandsBracePitchDeg - p.pitchDeg) * free;
            p.yawDeg += (UnityTuning.HandsBraceYawDeg - p.yawDeg) * free;
            p.rollDeg += (UnityTuning.HandsBraceRollDeg - p.rollDeg) * free;
            p.visible = p.visible || k > 0.001;
            return p;
        }

        public static HandsArmPose Pose(HandsArmParams a, double age, bool hit, double px, double py, double wallW, double pushAge, bool jab = false)
        {
            HandsArmPose p = jab ? HandsJab.Pose(a, age, hit, px, py) : Pose(a, age, hit, px, py); // NIGHT19: the R3 jab (Viewmodel.Alt19.cs)
            if (!(wallW > 0) && !(pushAge < UnityTuning.HandsWallPushTotal)) return p;
            double k = HandsDraw.Clamp(wallW, 0, 1), push = HandsDraw.Envelope(pushAge, UnityTuning.HandsWallPushOut, UnityTuning.HandsWallPushHold, UnityTuning.HandsWallPushTotal);
            double free = (1 - (p.visible ? HandsDraw.Clamp(p.w, 0, 1) : 0)) * k;
            p.x += (UnityTuning.HandsWallHandX + UnityTuning.HandsWallPushX * push - a.restX) * free;
            p.y += (UnityTuning.HandsWallHandY - a.restY) * free;
            p.z += (UnityTuning.HandsWallHandZ - a.restZ) * free;
            p.pitchDeg += (UnityTuning.HandsWallHandPitchDeg - a.restPitch) * free;
            p.yawDeg += (UnityTuning.HandsWallHandYawDeg - a.restYaw) * free;
            p.rollDeg += (UnityTuning.HandsWallHandRollDeg - a.restRoll) * free;
            p.visible = p.visible || k > 0.001;
            return p;
        }
    }

    /// <summary>
    /// The sprint carry and the boost lean (StaminaHud.Read's sprinting / boosting), eased in and out on world time; the
    /// boost's swing (a dash: the gun lags the move, then catches up past it on an underdamped spring). A shot yields all
    /// of it: the gate eases from where it is to 0 over HandsPoseYieldTime from the shot's frame, holds off for
    /// HandsPoseShotHold, comes back over HandsPoseReturnTime. No allocation per frame.
    /// </summary>
    public sealed class HandsMovePose
    {
        public double sprintW, boostW, gate = 1, side, fwd;
        double yieldFrom, yieldAge = double.PositiveInfinity;
        readonly SpringBank lag = new SpringBank(3, new[] { 0.0002, 0.0002, 0.02, 0.02 }); // x m, z m, roll deg, yaw deg
        readonly SpringShape lagShape = SpringShape.Of(UnityTuning.HandsDashLagHz, UnityTuning.HandsDashLagZeta);
        readonly double[] peaks = new double[4];

        /// <summary>A shot (or a swing): every movement pose yields from this frame.</summary>
        public void Shot()
        {
            yieldFrom = gate;
            yieldAge = 0;
        }

        /// <summary>A boost: its direction in the view (side: + right, fwd: + ahead; unit), x motion (effects, REDUCED MOTION).</summary>
        public void Dash(double side, double fwd, double motion)
        {
            this.side = side;
            this.fwd = fwd;
            if (!(motion > 0)) return;
            peaks[0] = -side * UnityTuning.HandsDashLagX * motion; // the hand lags the move
            peaks[1] = fwd * UnityTuning.HandsDashLagZ * motion; // a boost ahead pulls it back toward the eye
            peaks[2] = side * UnityTuning.HandsDashLagRollDeg * motion; // rolled against the move (+ counter-clockwise)
            peaks[3] = side * UnityTuning.HandsDashLagYawDeg * motion; // the muzzle trails (+ left)
            lag.Add(lagShape, peaks);
        }

        /// <summary>The slide's drawn weight (smoothstep of its progress; below 0 in a slide-jump's overshoot) and its lean
        /// (-1 right .. 1 left).</summary>
        public double slideW, slideLean;
        double slideU, slideSpeedNow, slideClock, snapFrom, snapAge = double.PositiveInfinity;

        /// <summary>A slide-jump (the mover's Jump of kind Slide): the slide pose snaps back up past rest (an ease-out-back).</summary>
        public void SlideJump()
        {
            if (!(slideW > 0)) return;
            snapFrom = slideW;
            snapAge = 0;
            slideU = 0;
        }

        /// <summary>One frame with the slide: `sliding` the mover's Slide state, `slideSpeed` m/s (the tremor), `lateral` the
        /// slide's side in the view (-1 left .. 1 right: the lean follows it).</summary>
        public void Frame(bool sprinting, bool boosting, bool sliding, double slideSpeed, double lateral, double dt)
        {
            Frame(sprinting, boosting, dt);
            slideClock += dt;
            slideSpeedNow = slideSpeed;
            double lean = HandsDraw.Clamp(UnityTuning.HandsSlideLeanBias - lateral, -1, 1);
            if (snapAge < double.PositiveInfinity)
            {
                // the slide-jump's snap: from where it was, past rest, settled at its end (a new slide waits for it)
                snapAge += dt;
                double u = snapAge / UnityTuning.HandsSlideSnapTime;
                if (u < 1)
                {
                    slideW = snapFrom * (1 - HandsEase.OutBack(u, UnityTuning.HandsSlideSnapBack));
                    return;
                }
                snapAge = double.PositiveInfinity;
                slideW = slideU = 0;
            }
            if (sliding && !(slideU > 0)) slideLean = lean; // a new slide leans from its first frame (its weight eases in)
            else if (sliding) slideLean += (lean - slideLean) * (1 - Math.Exp(-UnityTuning.HandsSlideLeanRate * dt));
            slideU = HandsDraw.Clamp(slideU + (sliding ? dt / UnityTuning.HandsSlideInTime : -dt / UnityTuning.HandsSlideOutTime), 0, 1);
            slideW = HandsDraw.Smooth(slideU);
        }

        public void Frame(bool sprinting, bool boosting, double dt)
        {
            double ts = sprinting ? 1 : 0, tb = boosting ? 1 : 0;
            sprintW += (ts - sprintW) * (1 - Math.Exp(-(sprinting ? UnityTuning.HandsSprintInRate : UnityTuning.HandsSprintOutRate) * dt));
            boostW += (tb - boostW) * (1 - Math.Exp(-(boosting ? UnityTuning.HandsBoostInRate : UnityTuning.HandsBoostOutRate) * dt));
            if (sprintW < 1e-5 && !sprinting) sprintW = 0;
            if (boostW < 1e-5 && !boosting) boostW = 0;
            lag.Update(dt);
            if (yieldAge < double.PositiveInfinity)
            {
                yieldAge += dt;
                double y = UnityTuning.HandsPoseYieldTime, h = UnityTuning.HandsPoseShotHold, r = UnityTuning.HandsPoseReturnTime;
                if (yieldAge < y) gate = yieldFrom * (1 - HandsEase.OutCubic(yieldAge / y));
                else if (yieldAge < y + h) gate = 0;
                else if (yieldAge < y + h + r) gate = HandsDraw.Smooth((yieldAge - y - h) / r);
                else
                {
                    gate = 1;
                    yieldAge = double.PositiveInfinity;
                }
            }
        }

        /// <summary>The offsets now (web camera space; added to the viewmodel's), `bobPhase` the movement bob's phase,
        /// `motion` x the bob and the lean (effects intensity, REDUCED MOTION).</summary>
        public HandsDrawPose Sample(double bobPhase, double motion)
        {
            var p = new HandsDrawPose();
            double s = sprintW * gate;
            if (s > 0)
            {
                p.x = UnityTuning.HandsSprintX * s;
                p.y = UnityTuning.HandsSprintY * s;
                p.z = UnityTuning.HandsSprintZ * s;
                p.pitchDeg = UnityTuning.HandsSprintPitchDeg * s;
                p.yawDeg = UnityTuning.HandsSprintYawDeg * s;
                p.rollDeg = UnityTuning.HandsSprintRollDeg * s;
                double sb = Math.Sin(bobPhase), bm = s * motion;
                p.x += sb * UnityTuning.HandsSprintBobX * bm;
                p.y -= sb * sb * UnityTuning.HandsSprintBobY * bm;
                p.rollDeg += sb * UnityTuning.HandsSprintBobRollDeg * bm;
                p.pitchDeg += Math.Sin(2 * bobPhase) * UnityTuning.HandsSprintBobPitchDeg * bm;
            }
            double sl = slideW * gate;
            if (sl != 0)
            {
                // the slide: low, canted toward its lean, a skid's tremor by speed (x motion; none in the snap's overshoot)
                p.x += UnityTuning.HandsSlideX * sl;
                p.y += UnityTuning.HandsSlideY * sl;
                p.z += UnityTuning.HandsSlideZ * sl;
                p.pitchDeg += UnityTuning.HandsSlidePitchDeg * sl;
                p.rollDeg += UnityTuning.HandsSlideRollDeg * slideLean * sl;
                double k = HandsDraw.Clamp(slideSpeedNow / UnityTuning.HandsSlideTremorFullSpeed, 0, 1) * Math.Max(0, sl) * motion;
                if (k > 0)
                {
                    double c = 2 * Math.PI * slideClock;
                    p.x += UnityTuning.HandsSlideTremorX * Math.Sin(c * UnityTuning.HandsSlideTremorHzX) * k;
                    p.y += UnityTuning.HandsSlideTremorY * Math.Sin(c * UnityTuning.HandsSlideTremorHzY + 1.1) * k;
                    p.rollDeg += UnityTuning.HandsSlideTremorRollDeg * Math.Sin(c * UnityTuning.HandsSlideTremorHzRoll + 2.3) * k;
                }
            }
            double b = boostW * gate;
            if (b > 0)
            {
                p.rollDeg -= side * UnityTuning.HandsBoostLeanRollDeg * b * motion; // leans into the boost's side
                p.y += UnityTuning.HandsBoostTuckY * b;
                p.z += UnityTuning.HandsBoostTuckZ * b;
            }
            if (lag.Active > 0)
            {
                p.x += lag.Sample(0) * gate;
                p.z += lag.Sample(1) * gate;
                p.rollDeg += lag.Sample(2) * gate;
                p.yawDeg += lag.Sample(3) * gate;
            }
            return p;
        }

        public void Clear()
        {
            slideW = slideU = slideLean = 0;
            snapAge = double.PositiveInfinity;
            sprintW = boostW = 0;
            gate = 1;
            yieldAge = double.PositiveInfinity;
            lag.Clear();
        }
    }

    /// <summary>QUICKDRAW's glint: `age` s after fire opened on a swap (HandsDrawState.QuickdrawAge).</summary>
    public static class HandsQuickdraw
    {
        /// <summary>0..1 the strip's brightness: a sine bump per pass, each pass dimmer; 0 outside the window.</summary>
        public static double Amount(double age)
        {
            if (!(age >= 0) || age >= UnityTuning.HandsQuickdrawWindow) return 0;
            double f = age / (UnityTuning.HandsQuickdrawWindow / UnityTuning.HandsQuickdrawSweeps);
            int pass = (int)f;
            return Math.Sin(Math.PI * (f - pass)) * (1 - (double)pass / UnityTuning.HandsQuickdrawSweeps);
        }

        /// <summary>NIGHT18 U3-31.9 #2 (builder M): the sheen on the gun's own surface: a part at `u` (0 the back .. 1 the muzzle)
        /// lights by the pass's `amount`: HandsQuickdrawSheenBase of it everywhere, the rest in a band HandsQuickdrawSheenWidth
        /// wide at the pass's place `along`.</summary>
        public static double Sheen(double amount, double along, double u)
        {
            if (!(amount > 0)) return 0;
            double band = Math.Max(0, 1 - Math.Abs(u - along) / UnityTuning.HandsQuickdrawSheenWidth);
            return amount * (UnityTuning.HandsQuickdrawSheenBase + (1 - UnityTuning.HandsQuickdrawSheenBase) * band);
        }

        /// <summary>NIGHT18 U3-31.9 #2: a part's place along the gun (0 the back .. 1 the muzzle) from its centre's z in rig
        /// space (the barrel runs +z) and the rig's parts' z range.</summary>
        public static double Place(double z, double zMin, double zMax) =>
            zMax - zMin < 1e-9 ? 0.5 : Math.Max(0, Math.Min(1, (z - zMin) / (zMax - zMin)));

        /// <summary>0..1 the strip's place from the back of the gun to the muzzle in the current pass.</summary>
        public static double Along(double age)
        {
            if (!(age >= 0) || age >= UnityTuning.HandsQuickdrawWindow) return 0;
            double f = age / (UnityTuning.HandsQuickdrawWindow / UnityTuning.HandsQuickdrawSweeps);
            return f - Math.Floor(f);
        }
    }

    /// <summary>
    /// U3-30.10, the kill flourish ("satisfying hits"; port-only): a MULTIKILL award (the sim's style.multikillWindow) or
    /// a rank-up arms it; it plays once the shooting has paused HandsFlourishQuiet s (world time), at most once per arming,
    /// and an armed one lapses after HandsFlourishPendingMax. A shot, a swing, a punch (Cancel) or a swap stops it on that
    /// frame (Pillar 1). Each gun its own: the KESTREL spins its cylinder and twirls back to hold, the THUNDERCLAP racks
    /// one-handed, the STITCHER flicks and chatters, the COMET bumps on the shoulder, the LONGWAVE hums (its coil lit cyan),
    /// the GAUNTLET cracks its knuckles. REDUCED MOTION: a short still pose-pop (no spin, no chatter).
    /// </summary>
    public sealed class HandsFlourish
    {
        public bool pending, started;
        public double age = double.PositiveInfinity;
        double quiet = 1e9, pendingAge;

        public static bool IsTrigger(string award) => award == "MULTIKILL";

        public static bool IsRankUp(int last, int now) => now > last;

        public static bool Playing(double age, double duration) => age >= 0 && age < duration;

        public void Arm()
        {
            pending = true;
            pendingAge = 0;
        }

        /// <summary>A shot, a swing, a punch: stops now; the pause starts over.</summary>
        public void Cancel()
        {
            age = double.PositiveInfinity;
            quiet = 0;
        }

        /// <summary>A new run / a respawn: nothing armed, nothing playing.</summary>
        public void Clear()
        {
            pending = false;
            age = double.PositiveInfinity;
            quiet = 1e9;
        }

        /// <summary>NIGHT18 U3-31.9 #1 (builder M): every kill of the player's arms it too (FeelAB on), not only a MULTIKILL or a
        /// rank-up, so a single kill shows it.</summary>
        public static bool IsKillTrigger(string owner) => owner == "player";

        /// <summary>NIGHT18 U3-31.9 #1: the flourish shows the gun off: up, in toward the middle and canted to the eye over the
        /// flourish (a sine bump over `duration`), x `motion`.</summary>
        public static HandsDrawPose Present(double age, double duration, double motion)
        {
            double b = Bump(age, 0, duration) * motion;
            return new HandsDrawPose { x = -UnityTuning.HandsFlourishPresentX * b, y = UnityTuning.HandsFlourishPresentY * b, yawDeg = UnityTuning.HandsFlourishPresentYawDeg * b };
        }

        /// <summary>NIGHT17's frame: it starts after HandsFlourishQuiet s of no shot, the trigger held or not.</summary>
        public void Frame(bool swapping, double dt, double duration) => Frame(swapping, dt, duration, UnityTuning.HandsFlourishQuiet, false);

        /// <summary>NIGHT18 U3-31.9 #1: a frame with NIGHT18's start rule: it starts after `quietNeeded` s of no shot and only with
        /// the trigger let go (a held trigger between a slow gun's shots never starts it).</summary>
        public void Frame(bool swapping, double dt, double duration, double quietNeeded, bool triggerHeld)
        {
            started = false;
            if (swapping)
            {
                age = double.PositiveInfinity;
                quiet = 0;
            }
            else quiet += dt;
            if (age < double.PositiveInfinity)
            {
                age += dt;
                if (age >= duration) age = double.PositiveInfinity;
            }
            if (!pending) return;
            pendingAge += dt;
            if (pendingAge > UnityTuning.HandsFlourishPendingMax) pending = false;
            else if (!swapping && quiet >= quietNeeded && !triggerHeld && !(age < double.PositiveInfinity))
            {
                pending = false;
                age = 0;
                started = true;
            }
        }

        public static double Duration(HandsDrawSpec s, bool reduced)
        {
            if (reduced) return UnityTuning.HandsFlourishStillTime;
            switch (s.weapon)
            {
                case "revolver": return UnityTuning.HandsFlourishKestrelTime;
                case "shotgun": return UnityTuning.HandsFlourishThunderclapTime;
                case "rocket": return UnityTuning.HandsFlourishCometTime;
                case "rail": return UnityTuning.HandsFlourishLongwaveTime;
                default: return UnityTuning.HandsFlourishTime; // the STITCHER, the GAUNTLET, the rest
            }
        }

        static double Bump(double a, double from, double time) => a >= from && a < from + time ? Math.Sin(Math.PI * (a - from) / time) : 0;

        /// <summary>The flourish's offsets at `age` (zero outside it; at hold at both ends), x `motion` (the parts: not).</summary>
        public static HandsDrawPose Sample(HandsDrawSpec s, double age, bool reduced, double motion)
        {
            var p = new HandsDrawPose();
            double d = Duration(s, reduced);
            if (!(age >= 0) || age >= d) return p;
            if (reduced)
            {
                // a still pose-pop: the gun shown off and back, no spin, no chatter
                double e = HandsDraw.Envelope(age, UnityTuning.HandsFlourishStillIn, UnityTuning.HandsFlourishStillHold, d);
                p.rollDeg = UnityTuning.HandsFlourishStillRollDeg * e;
                p.pitchDeg = UnityTuning.HandsFlourishStillPitchDeg * e;
                p.y = UnityTuning.HandsFlourishStillY * e;
                if (s.weapon == "rail") p.glow = e;
                if (s.weapon == "gauntlet") p.clench = e;
                return p;
            }
            double m = HandsDraw.Clamp(motion, 0, 1);
            switch (s.weapon)
            {
                case "revolver":
                {
                    // the cylinder spins (whole chambers: it stops looking as at rest) and the gun twirls a whole turn back to hold
                    double sa = age / UnityTuning.HandsFlourishKestrelSpinTime;
                    if (sa < 1) p.spinRad = HandsDraw.SpinRad(s, UnityTuning.HandsFlourishKestrelSpinTurns, m) * HandsEase.OutCubic(sa);
                    double tw = (age - UnityTuning.HandsFlourishKestrelTwirlFrom) / UnityTuning.HandsFlourishKestrelTwirlTime;
                    if (tw > 0 && tw < 1)
                    {
                        // a whole turn only at full motion (it ends where it began); scaled down, a wrist roll out and back
                        double inOut = tw < 0.5 ? 4 * tw * tw * tw : 1 - Math.Pow(-2 * tw + 2, 3) / 2;
                        p.rollDeg = m >= 0.999 ? 360 * inOut : UnityTuning.HandsFlourishKestrelRollDeg * m * Math.Sin(Math.PI * tw);
                        if (p.rollDeg >= 359.999) p.rollDeg = 0;
                    }
                    p.pitchDeg = UnityTuning.HandsFlourishKestrelPitchDeg * m * Bump(age, 0, d);
                    break;
                }
                case "shotgun":
                {
                    // a hard one-handed rack: the pump out and home fast, the muzzle flipped up with it
                    p.pump = Bump(age, UnityTuning.HandsFlourishRackFrom, UnityTuning.HandsFlourishRackTime);
                    double flip = Bump(age, 0, d);
                    p.pitchDeg = UnityTuning.HandsFlourishRackPitchDeg * m * flip;
                    p.rollDeg = -UnityTuning.HandsFlourishRackRollDeg * m * flip;
                    p.y = UnityTuning.HandsFlourishRackY * m * flip;
                    break;
                }
                case "nailgun":
                {
                    // a flick down (the spent drum shaken out) and a chatter of the barrel, decaying to rest
                    p.pitchDeg = -UnityTuning.HandsFlourishFlickPitchDeg * m * Bump(age, 0, UnityTuning.HandsFlourishFlickTime);
                    double c = (age - UnityTuning.HandsFlourishChatterFrom) / (d - UnityTuning.HandsFlourishChatterFrom);
                    if (c > 0 && c < 1) p.rollDeg = UnityTuning.HandsFlourishChatterRollDeg * m * (1 - c) * Math.Sin(2 * Math.PI * UnityTuning.HandsFlourishChatterHz * (age - UnityTuning.HandsFlourishChatterFrom));
                    break;
                }
                case "rocket":
                {
                    // a bump on the shoulder: up, rolled onto it, a little settle dip
                    double up = Bump(age, 0, UnityTuning.HandsFlourishBumpTime), dip = Bump(age, UnityTuning.HandsFlourishBumpTime, d - UnityTuning.HandsFlourishBumpTime);
                    p.y = (UnityTuning.HandsFlourishBumpY * up - UnityTuning.HandsFlourishBumpDipY * dip) * m;
                    p.rollDeg = UnityTuning.HandsFlourishBumpRollDeg * m * Bump(age, 0, d);
                    p.pitchDeg = UnityTuning.HandsFlourishBumpPitchDeg * m * up;
                    break;
                }
                case "rail":
                {
                    // a hum: the charge coil lit (cyan, Viewmodel.AnimateParts), a fine vibration, a slight lift
                    double g = HandsDraw.Envelope(age, UnityTuning.HandsFlourishHumIn, UnityTuning.HandsFlourishHumHold, d);
                    p.glow = g;
                    p.sweep = age / d;
                    p.rollDeg = UnityTuning.HandsFlourishHumRollDeg * m * g * Math.Sin(2 * Math.PI * UnityTuning.HandsFlourishHumHz * age);
                    p.pitchDeg = UnityTuning.HandsFlourishHumPitchDeg * m * g;
                    break;
                }
                case "gauntlet":
                {
                    // two knuckle cracks: the fist clenches twice, rolling one way then the other
                    double c1 = Bump(age, 0, UnityTuning.HandsFlourishCrackTime), c2 = Bump(age, UnityTuning.HandsFlourishCrackGap, UnityTuning.HandsFlourishCrackTime);
                    p.clench = Math.Max(c1, c2);
                    p.rollDeg = UnityTuning.HandsFlourishCrackRollDeg * m * (c1 - c2);
                    p.pitchDeg = UnityTuning.HandsFlourishCrackPitchDeg * m * Bump(age, 0, d);
                    break;
                }
                default:
                {
                    double b = Bump(age, 0, d);
                    p.rollDeg = UnityTuning.HandsFlourishStillRollDeg * m * b;
                    p.pitchDeg = UnityTuning.HandsFlourishStillPitchDeg * m * b;
                    break;
                }
            }
            return p;
        }
    }

    /// <summary>The style aura's look: its colour (LDR, the cyan ladder) and its HDR intensity (0: none).</summary>
    public struct HandsAuraLook
    {
        public Color color;
        public double intensity;
    }

    /// <summary>
    /// Round 3, beyond the ask (D-70; the web's fx/aura.js, absent in the port): the style aura on the gun in hand climbs
    /// the cyan value ladder with the style rank: none at rank 0, CYAN-PALE at rank 1, CYAN in the middle, CYAN-HOT at the
    /// top, its HDR intensity rising with the rank; the top rank alone pulses slowly (REDUCED MOTION: no pulse, the glow
    /// stays). Never the ranks' own hues (style.ranks' colours run to red-orange and magenta): the player's cyan only.
    /// </summary>
    public static class HandsAura
    {
        public static HandsAuraLook Sample(int rank, int count, double time, bool reducedMotion, Color pale, Color cyan, Color hot)
        {
            var a = new HandsAuraLook();
            if (rank <= 0 || count < 2) return a;
            int top = count - 1;
            double t = top > 1 ? HandsDraw.Clamp((rank - 1.0) / (top - 1.0), 0, 1) : 1;
            a.color = t < 0.5 ? Color.Lerp(pale, cyan, (float)(t / 0.5)) : Color.Lerp(cyan, hot, (float)((t - 0.5) / 0.5));
            a.color.a = 1;
            a.intensity = UnityTuning.HandsAuraLow + (UnityTuning.HandsAuraHigh - UnityTuning.HandsAuraLow) * t;
            if (rank >= top && !reducedMotion) a.intensity *= 1 + UnityTuning.HandsAuraPulse * Math.Sin(2 * Math.PI * UnityTuning.HandsAuraPulseHz * time);
            return a;
        }
    }

    /// <summary>The aura's material colour, set only on a change (one material property, at most once a frame).</summary>
    public sealed class HandsAuraSetter
    {
        Color last;
        bool any;

        public bool Changed(Color c)
        {
            if (any && Mathf.Abs(c.r - last.r) < 1e-4f && Mathf.Abs(c.g - last.g) < 1e-4f && Mathf.Abs(c.b - last.b) < 1e-4f) return false;
            last = c;
            any = true;
            return true;
        }
    }

    /// <summary>The guns' look (round 3: "dark violet, small, in the same corner").</summary>
    public static class HandsLook
    {
        /// <summary>A family colour as the gun's accent (its stripe on the body): the same hue, its value x HandsAccentValue and
        /// its saturation x HandsAccentSat (each capped at 1). No new hue (CLAUDE.md rule 8).</summary>
        public static Color Accent(Color family)
        {
            double r = family.r, g = family.g, b = family.b;
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b)), d = max - min;
            if (!(max > 0)) return family;
            double h = 0;
            if (d > 0)
            {
                if (max == r) h = ((g - b) / d % 6 + 6) % 6;
                else if (max == g) h = (b - r) / d + 2;
                else h = (r - g) / d + 4;
            }
            double s = Math.Min(1, d / max * UnityTuning.HandsAccentSat), v = Math.Min(1, max * UnityTuning.HandsAccentValue);
            double c = v * s, x = c * (1 - Math.Abs(h % 2 - 1)), m = v - c;
            double rr, gg, bb;
            if (h < 1) { rr = c; gg = x; bb = 0; }
            else if (h < 2) { rr = x; gg = c; bb = 0; }
            else if (h < 3) { rr = 0; gg = c; bb = x; }
            else if (h < 4) { rr = 0; gg = x; bb = c; }
            else if (h < 5) { rr = x; gg = 0; bb = c; }
            else { rr = c; gg = 0; bb = x; }
            return new Color((float)(rr + m), (float)(gg + m), (float)(bb + m), family.a);
        }
    }

    /// <summary>The breath beyond the web's y sine: a pitch a quarter period behind it, a roll and a drift on a slower cycle.</summary>
    public static class HandsBreath
    {
        public static HandsDrawPose Sample(double time, double period, double motion)
        {
            var p = new HandsDrawPose();
            if (!(motion > 0) || !(period > 0)) return p;
            double ph = 2 * Math.PI * time / period, slow = ph / UnityTuning.HandsBreathRollPeriodMult;
            p.pitchDeg = UnityTuning.HandsBreathPitchDeg * Math.Sin(ph - Math.PI / 2) * motion;
            p.rollDeg = UnityTuning.HandsBreathRollDeg * Math.Sin(slow) * motion;
            p.x = UnityTuning.HandsBreathX * Math.Sin(slow + 1.3) * motion;
            return p;
        }
    }

    /// <summary>The view's half: the per-gun kick bank, the punch's torque, the poses, the breath, the QUICKDRAW strip.</summary>
    public sealed partial class Viewmodel
    {
        SpringBank feelKick;
        readonly double[] feelKickScratch = new double[HandsKick.Channels];
        int feelKickShot;
        HandsKickSpec feelPunchTorque;
        readonly HandsMovePose feelMove = new HandsMovePose();
        StaminaHud.Info feelStamina;
        Transform feelGlint;
        Material feelGlintMat;
        Color feelUsable;
        Rig feelGlintRig;
        bool feelPunchHit;

        void FeelBuild()
        {
            feelKick = new SpringBank(UnityTuning.HandsKickSlots, HandsKick.Snaps());
            feelPunchTorque = new HandsKickSpec { weapon = "punch", a = HandsKick.Layer(UnityTuning.HandsKickPunchTorque) };
            feelUsable = Look.Hex((uint)cfg.Num("colors.usable")); // the player's cyan (CLAUDE.md rule 8)
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Object.Destroy(g.GetComponent<Collider>());
            g.name = "quickdraw glint";
            g.layer = Layer;
            feelGlintMat = new Material(game.mats.neon);
            var rend = g.GetComponent<Renderer>();
            rend.sharedMaterial = feelGlintMat;
            rend.shadowCastingMode = ShadowCastingMode.Off;
            g.SetActive(false);
            feelGlint = g.transform;
            feelGlint.SetParent(root, false);
            foreach (Rig r in rigList)
            {
                // the strip runs along the top of the body (its mesh bounds, Unity rig space: the barrel runs +z)
                if (r.meshes.Count == 0) continue;
                Bounds bb = r.meshes[0].bounds;
                float y = bb.max.y + UnityTuning.HandsQuickdrawAbove, half = UnityTuning.HandsQuickdrawLen / 2;
                r.glintFrom = new Vector3(bb.center.x, y, bb.min.z + half);
                r.glintTo = new Vector3(bb.center.x, y, Mathf.Max(bb.min.z + half, bb.max.z - half));
                r.glintWide = bb.size.x * UnityTuning.HandsQuickdrawWide;
            }
            FeelAuraBuild(); // round 3: the style aura's shells
            // U3-30.10: the kill flourish's triggers (the handlers only arm it; it plays from the frame loop)
            game.events.On<StyleAwardEvent>("styleAward", e =>
            {
                if (HandsFlourish.IsTrigger(e.name)) feelFlourish.Arm();
            });
            game.events.On("rankChange", o =>
            {
                int r = o is int i ? i : 0;
                if (HandsFlourish.IsRankUp(feelLastRank, r)) feelFlourish.Arm();
                feelLastRank = r;
            });
            // NIGHT18 U3-31.9 #1 (builder M): any kill of the player's arms it too (FeelAB on)
            game.events.On<KillEvent>("kill", e =>
            {
                if (FeelAB.On && HandsFlourish.IsKillTrigger(e.owner)) feelFlourish.Arm();
            });
        }

        // --- U3-30.10: the kill flourish -------------------------------------------------------------------------------------
        readonly HandsFlourish feelFlourish = new HandsFlourish();
        int feelLastRank;
        double feelFlourishGlow; // 0..1 x FLASH INTENSITY: the LONGWAVE's hum (AnimateParts lights its coil)

        /// <summary>The flourish this frame (world time), folded into drawPose like the idle flourish; a swap stops it.</summary>
        void FeelFlourishFrame(Loadout lo, double animDt, double motion)
        {
            bool reduced = cfg.movement.settings.reducedMotion;
            HandsDrawSpec s = drawState.spec;
            feelFlourishGlow = 0;
            if (s == null) return;
            double dur = HandsFlourish.Duration(s, reduced);
            bool swapping = SwapActive(lo) || drawState.started;
            // NIGHT18 U3-31.9 #1: it starts after HandsFlourishQuietN18 s with the trigger let go (NIGHT17: 0.35 s, held or not)
            if (FeelAB.On) feelFlourish.Frame(swapping, animDt, dur, UnityTuning.HandsFlourishQuietN18, game.sim != null && game.lastCombat.fire.held);
            else feelFlourish.Frame(swapping, animDt, dur);
            if (feelFlourish.started) FeelProbe.Event(UnityTuning.ProbeFlourish); // the reel's `flourish` moment (both sides)
            if (!HandsFlourish.Playing(feelFlourish.age, dur)) return;
            HandsDrawPose p = HandsFlourish.Sample(s, feelFlourish.age, reduced, motion);
            if (FeelAB.On && !reduced)
            {
                // NIGHT18 U3-31.9 #1: shown off: up, in and canted to the eye (readable at a glance, not just the gun's own parts)
                HandsDrawPose q = HandsFlourish.Present(feelFlourish.age, dur, motion);
                p.x += q.x;
                p.y += q.y;
                p.yawDeg += q.yawDeg;
            }
            drawPose.x += p.x;
            drawPose.y += p.y;
            drawPose.z += p.z;
            drawPose.pitchDeg += p.pitchDeg;
            drawPose.yawDeg += p.yawDeg;
            drawPose.rollDeg += p.rollDeg;
            drawPose.spinRad += p.spinRad;
            drawPose.pump = Math.Max(drawPose.pump, p.pump);
            drawPose.clench = Math.Max(drawPose.clench, p.clench);
            feelFlourishGlow = p.glow * FlashIntensity();
        }

        /// <summary>A shot of `weapon`: its own kick (true), or false (the web's class recoil applies). Every shot yields the
        /// movement poses.</summary>
        bool FeelShot(string weapon, Gun g)
        {
            feelMove.Shot();
            if (g.kickSpec == null) return false;
            double steady = HandsSteady.Mult(game.sim != null && game.sim.perks != null ? game.sim.perks.fx : null);
            HandsKick.Add(feelKick, Pump40Kick(weapon, g.kickSpec), feelKickShot++, steady, feelKickScratch); // NIGHT40 W1 pump40: the THUNDERCLAP's 6 Hz kick while on
            return true;
        }

        /// <summary>The punch event: did it connect (the fist's rebound), and the body's torque on the gun.</summary>
        void FeelOnPunch(MeleeEvent e)
        {
            feelPunchHit = e.hits > 0;
            feelFlourish.Cancel(); // U3-30.10: a punch stops the kill flourish on its frame
            double m = DrawMotion();
            if (!(m > 0)) return;
            HandsKick.Add(feelKick, feelPunchTorque, 0, m, feelKickScratch);
        }

        /// <summary>A boost (the mover's dash): its direction in the view from the dash's velocity.</summary>
        void FeelOnDash()
        {
            Vec3 vel = game.mover.velocity;
            double sp = Math.Sqrt(vel.x * vel.x + vel.z * vel.z);
            if (!(sp > 1e-6)) return;
            double yaw = game.input.yaw;
            double side = (vel.x * Math.Cos(yaw) - vel.z * Math.Sin(yaw)) / sp; // . the view's right (cos yaw, 0, -sin yaw)
            double fwd = (-vel.x * Math.Sin(yaw) - vel.z * Math.Cos(yaw)) / sp; // . the view's forward (-sin yaw, 0, -cos yaw)
            feelMove.Dash(side, fwd, DrawMotion());
        }

        /// <summary>The frame's movement poses and breath (world time), added to the pose sums.</summary>
        void FeelPose(double animDt, ref double px, ref double py, ref double pz, ref double rx, ref double ry, ref double rz)
        {
            StaminaHud.Read(game, ref feelStamina);
            // the slide (round 1): the mover's Slide state, its speed (the tremor) and its side in the view (the lean)
            Mover mover = game.mover;
            bool sliding = mover.state == MoveState.Slide;
            double lateral = 0;
            if (sliding)
            {
                double sp = Math.Sqrt(mover.velocity.x * mover.velocity.x + mover.velocity.z * mover.velocity.z), yaw = game.input.yaw;
                if (sp > 1e-6) lateral = (mover.velocity.x * Math.Cos(yaw) - mover.velocity.z * Math.Sin(yaw)) / sp; // . the view's right
            }
            feelMove.Frame(feelStamina.valid && feelStamina.sprinting, feelStamina.valid && feelStamina.boosting, sliding, mover.Speed, lateral, animDt);
            double m = DrawMotion();
            HandsDrawPose mp = feelMove.Sample(bobPhase, m);
            HandsDrawPose br = HandsBreath.Sample(time, v.breathPeriod, m);
            px += mp.x + br.x;
            py += mp.y;
            pz += mp.z;
            rx += (mp.pitchDeg + br.pitchDeg) * Deg;
            ry += mp.yawDeg * Deg;
            rz += (mp.rollDeg + br.rollDeg) * Deg;
        }

        /// <summary>The per-gun kick bank (its attack on frame time inside a freeze), added to the pose sums.</summary>
        void FeelKickFrame(double animDt, double frameDt, ref double px, ref double py, ref double pz, ref double rx, ref double ry, ref double rz)
        {
            feelKick.Update(HandsKick.ClockDt(animDt, frameDt) * RecoilRate33(game.loadout.Weapon)); // round 2: frame time through the freeze (not held at its peak); NIGHT33 view33: x the class rate
            if (feelKick.Active == 0) return;
            rx += feelKick.Sample(0) * Deg;
            pz += feelKick.Sample(1);
            py -= feelKick.Sample(2);
            rz += feelKick.Sample(3) * Deg;
            ry += feelKick.Sample(4) * Deg;
            px += feelKick.Sample(5);
        }

        /// <summary>QUICKDRAW now (0..1, x FLASH INTENSITY): the gem reads it too.</summary>
        double FeelQuickdraw() => HandsQuickdraw.Amount(drawState.QuickdrawAge) * FlashIntensity();

        /// <summary>The strip along the gun in hand while QUICKDRAW lives.</summary>
        // NIGHT18 U3-31.9 #2 (builder M): QUICKDRAW on the gun's own surface: its meshes drawn again additively where they are
        // (the afterimages' way), each lit by HandsQuickdraw.Sheen at its place along the gun (back to muzzle). FeelAB off: none.
        Material feelSheenMat;
        RenderParams feelSheenParams;
        MaterialPropertyBlock[] feelSheenBlocks = new MaterialPropertyBlock[0];

        void FeelSheen(Rig rig, bool show, double amount)
        {
            if (!FeelAB.On || !show || rig == null || !(amount > 0) || rig.filters == null || rig.filters.Length == 0) return;
            if (feelSheenMat == null)
            {
                feelSheenMat = FxDraw.Additive(game.mats.neon, "Quickdraw sheen");
                feelSheenParams = FxDraw.Params(feelSheenMat, Layer);
            }
            if (feelSheenBlocks.Length < rig.filters.Length)
            {
                // made once per the most parts any gun has (not per frame)
                var grown = new MaterialPropertyBlock[rig.filters.Length];
                for (int i = 0; i < grown.Length; i++) grown[i] = i < feelSheenBlocks.Length ? feelSheenBlocks[i] : new MaterialPropertyBlock();
                feelSheenBlocks = grown;
            }
            if (rig.sheenPlace == null) FeelSheenPlaces(rig);
            double along = HandsQuickdraw.Along(drawState.QuickdrawAge);
            for (int i = 0; i < rig.filters.Length; i++)
            {
                MeshFilter f = rig.filters[i];
                Mesh mesh = f != null ? f.sharedMesh : null;
                if (mesh == null || !f.gameObject.activeInHierarchy) continue;
                double s = HandsQuickdraw.Sheen(amount, along, rig.sheenPlace[i]);
                if (s <= 0) continue;
                feelSheenBlocks[i].SetColor(BaseColor, feelUsable * (float)(UnityTuning.HandsQuickdrawSheenHdr * s));
                feelSheenParams.matProps = feelSheenBlocks[i];
                Graphics.RenderMesh(feelSheenParams, mesh, 0, f.transform.localToWorldMatrix);
            }
        }

        /// <summary>Each part's place along the gun (its mesh centre's z in rig space, over the rig's range), once per rig.</summary>
        void FeelSheenPlaces(Rig rig)
        {
            var z = new double[rig.filters.Length];
            double lo = double.MaxValue, hi = double.MinValue;
            for (int i = 0; i < z.Length; i++)
            {
                MeshFilter f = rig.filters[i];
                Mesh mesh = f != null ? f.sharedMesh : null;
                z[i] = mesh != null ? rig.t.InverseTransformPoint(f.transform.TransformPoint(mesh.bounds.center)).z : 0;
                lo = Math.Min(lo, z[i]);
                hi = Math.Max(hi, z[i]);
            }
            rig.sheenPlace = new double[z.Length];
            for (int i = 0; i < z.Length; i++) rig.sheenPlace[i] = HandsQuickdraw.Place(z[i], lo, hi);
        }

        // NIGHT18 U3-31.9 #6 (builder M): the reel hears the gun in hand: its renderers' bounds through the viewmodel camera,
        // each rendered frame (the kick per gun: the rect's move and size after each `shot`). The group is the lead's
        // FeelFx member at UnityTuning.ProbeGunFx; until it exists (FeelFx.Count <= it) nothing is reported.
        readonly Vector3[] feelGunCorners = new Vector3[8];

        void FeelProbeGun(Rig rig, bool show)
        {
            if (!FeelProbe.Recording || !show || rig == null || (int)FeelFx.Count <= UnityTuning.ProbeGunFx) return;
            if (rig.renderers == null) rig.renderers = rig.t.GetComponentsInChildren<Renderer>(true); // once per rig
            bool any = false;
            Bounds b = default;
            for (int i = 0; i < rig.renderers.Length; i++)
            {
                Renderer r = rig.renderers[i];
                if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (!any) b = r.bounds;
                else b.Encapsulate(r.bounds);
                any = true;
            }
            if (!any) return;
            Vector3 mn = b.min, mx = b.max;
            feelGunCorners[0] = new Vector3(mn.x, mn.y, mn.z);
            feelGunCorners[1] = new Vector3(mx.x, mn.y, mn.z);
            feelGunCorners[2] = new Vector3(mn.x, mx.y, mn.z);
            feelGunCorners[3] = new Vector3(mx.x, mx.y, mn.z);
            feelGunCorners[4] = new Vector3(mn.x, mn.y, mx.z);
            feelGunCorners[5] = new Vector3(mx.x, mn.y, mx.z);
            feelGunCorners[6] = new Vector3(mn.x, mx.y, mx.z);
            feelGunCorners[7] = new Vector3(mx.x, mx.y, mx.z);
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            bool seen = false;
            for (int i = 0; i < 8; i++)
            {
                Vector3 s = vmCam.WorldToScreenPoint(feelGunCorners[i]);
                if (s.z <= vmCam.nearClipPlane) continue;
                seen = true;
                x0 = Mathf.Min(x0, s.x);
                y0 = Mathf.Min(y0, s.y);
                x1 = Mathf.Max(x1, s.x);
                y1 = Mathf.Max(y1, s.y);
            }
            if (seen) FeelProbe.Effect((FeelFx)UnityTuning.ProbeGunFx, Rect.MinMaxRect(x0, y0, x1, y1), 1f, 1);
        }

        void FeelGlintBand(Rig rig, bool show, double amount)
        {
            FeelSheen(rig, show, amount); // NIGHT18 U3-31.9 #2
            FeelProbeGun(rig, show); // NIGHT18 U3-31.9 #6
            bool on = UnityTuning.HandsQuickdrawStrip && show && rig != null && amount > 0 && rig.glintWide > 0; // round 5: the strip is off (it read as a clipping bug); QUICKDRAW stays on the gem
            if (feelGlint.gameObject.activeSelf != on) feelGlint.gameObject.SetActive(on);
            if (!on) return;
            if (feelGlintRig != rig)
            {
                feelGlint.SetParent(rig.t, false);
                feelGlint.localRotation = Quaternion.identity;
                feelGlint.localScale = new Vector3(rig.glintWide, UnityTuning.HandsQuickdrawThick, UnityTuning.HandsQuickdrawLen);
                feelGlintRig = rig;
            }
            feelGlint.localPosition = Vector3.Lerp(rig.glintFrom, rig.glintTo, (float)HandsQuickdraw.Along(drawState.QuickdrawAge));
            feelGlintMat.SetColor(BaseColor, feelUsable * (float)(UnityTuning.HandsQuickdrawHdr * amount));
        }

        // --- round 3: the style aura (HandsAura) ----------------------------------------------------------------------------
        Material feelAuraMat;
        readonly HandsAuraSetter feelAuraSet = new HandsAuraSetter();
        Color feelAuraPale, feelAuraCyan, feelAuraHot;
        bool feelAuraOn;

        /// <summary>Each rig's shell (its body mesh, back faces only, additive, expanded HandsAuraRim per axis about its bounds'
        /// centre) on one shared material; the ladder's colours from the theme's cyan mixes over colors.usable.</summary>
        void FeelAuraBuild()
        {
            feelAuraCyan = feelUsable;
            feelAuraCyan.a = 1;
            feelAuraPale = Color.Lerp(feelAuraCyan, Color.white, UnityTuning.ThemeCyanPaleMix);
            feelAuraHot = Color.Lerp(feelAuraCyan, Color.white, UnityTuning.ThemeCyanHotMix);
            feelAuraMat = FxDraw.Additive(game.mats.neon, "Style aura");
            feelAuraMat.SetFloat("_Cull", (float)CullMode.Front); // the inverted hull: the gun hides its inside, the rim shows
            foreach (Rig r in rigList)
            {
                if (r.meshes.Count == 0) continue;
                var go = new GameObject("style aura");
                go.layer = Layer;
                go.transform.SetParent(r.t, false);
                go.AddComponent<MeshFilter>().sharedMesh = r.meshes[0];
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = feelAuraMat;
                mr.shadowCastingMode = ShadowCastingMode.Off;
                mr.receiveShadows = false;
                Bounds bb = r.meshes[0].bounds;
                var s = new Vector3(1 + 2 * UnityTuning.HandsAuraRim / Mathf.Max(bb.size.x, 1e-3f), 1 + 2 * UnityTuning.HandsAuraRim / Mathf.Max(bb.size.y, 1e-3f),
                    1 + 2 * UnityTuning.HandsAuraRim / Mathf.Max(bb.size.z, 1e-3f));
                go.transform.localScale = s;
                go.transform.localPosition = bb.center - Vector3.Scale(bb.center, s); // expanded about the bounds' centre
                go.SetActive(false);
                r.aura = go;
            }
        }

        /// <summary>The aura now: the style rank (game.style) on the cyan ladder; the shells shown / hidden and the material's
        /// colour set only on a change (the top rank's pulse changes it each frame).</summary>
        void FeelAuraFrame()
        {
            StyleMeter st = game.sim != null ? game.sim.style : null;
            int rank = st != null ? st.rank : 0, count = st != null ? st.Params.ranks.Count : 0;
            HandsAuraLook a = HandsAura.Sample(rank, count, clock, cfg.movement.settings.reducedMotion, feelAuraPale, feelAuraCyan, feelAuraHot);
            bool on = a.intensity > 0;
            if (on != feelAuraOn)
            {
                feelAuraOn = on;
                foreach (Rig r in rigList)
                    if (r.aura != null) r.aura.SetActive(on);
            }
            if (!on) return;
            Color c = a.color * (float)a.intensity;
            if (feelAuraSet.Changed(c)) feelAuraMat.SetColor(BaseColor, c);
        }

        void FeelRespawn()
        {
            feelKick.Clear();
            feelMove.Clear();
            feelFlourish.Clear();
        }
    }
}
