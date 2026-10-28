// NIGHT19 U3-33.8 (builder V): every gun's alt-fire in the hands (each LT has a job tonight), and the R3 jab. Port-only:
// the web's viewmodel animates the primary only. Each alt follows the sim (its event or its counter); nothing here gates
// input or fire:
// - THUNDERCLAP, the point-blank parry: a bash forward with the barrels turned (the bus's `parry` with weapon "shotgun",
//   or `shotgunParry`: builder W's name, whichever lands);
// - STITCHER, the detonation: a key-turn twist (the bus's `nailDetonate`);
// - LONGWAVE, the overcharge: the gun pulls in and trembles harder as the charge fills (`railCharge` with `over`, then
//   the loadout's RailCharge until it fires or is let go);
// - COMET, steering: raised toward the eye and canted while the sim steers a rocket (the loadout's `steered` counter);
// - KESTREL, the gust: Viewmodel.Gust.cs;
// - ZAPPER, the discharge: the fork thrust forward (`discharge`); BOUNCER, the airburst: a flick up (`airburst`).
// The kicks are Spring-It-On's critically damped decay springs (Viewmodel.Springs.cs), x the effects' share; REDUCED MOTION
// stills every kick and the tremble (the COMET's raised pose stays: it is a pose, not a motion).
// The R3 jab (U3-33.3's quick melee, 10 damage): the punch's pose reads as a straight jab with tonight's look (FeelAB on):
// a short wind-up tucked low (JabCock*), a straight strike down the middle past the target with the knuckles level, a
// snappier stop (HandsJab); NIGHT18's hook (HandsPunch) stays the FeelAB-off side.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>The alt-fires' poses, pure (HandsN19Tests).</summary>
    public sealed class HandsAlt
    {
        readonly HalfLifeKick kick = new HalfLifeKick(6, UnityTuning.AltHalflife); // 0 pitch deg, 1 back m, 2 drop m, 3 roll deg, 4 yaw deg, 5 side m
        readonly double[] peaks = new double[6];
        public double steerW, over;
        double clock;

        void Add(double motion, double pitch, double back, double drop, double roll)
        {
            if (!(motion > 0)) return;
            peaks[0] = pitch;
            peaks[1] = back;
            peaks[2] = drop;
            peaks[3] = roll;
            peaks[4] = peaks[5] = 0;
            kick.Kick(peaks, Math.Min(1, motion));
        }

        /// <summary>THUNDERCLAP: the point-blank parry's bash.</summary>
        public void Parry(double motion) => Add(motion, UnityTuning.AltParryPitchDeg, UnityTuning.AltParryZ, 0, UnityTuning.AltParryRollDeg);

        /// <summary>STITCHER: the detonation's key-turn.</summary>
        public void Detonate(double motion) => Add(motion, UnityTuning.AltDetonatePitchDeg, 0, 0, UnityTuning.AltDetonateRollDeg);

        /// <summary>ZAPPER: the discharge's thrust.</summary>
        public void Discharge(double motion) => Add(motion, UnityTuning.AltDischargePitchDeg, UnityTuning.AltDischargeZ, 0, 0);

        /// <summary>BOUNCER: the airburst's flick.</summary>
        public void Airburst(double motion) => Add(motion, UnityTuning.AltAirburstPitchDeg, 0, UnityTuning.AltAirburstY, 0);

        /// <summary>A frame: the kicks settle, the steer pose eases toward `steering`, the overcharge is `overcharge` (0..1).</summary>
        public void Frame(double dt, bool steering, double overcharge)
        {
            if (!(dt > 0)) return;
            kick.Step(dt);
            clock += dt;
            steerW += ((steering ? 1 : 0) - steerW) * (1 - Math.Exp(-UnityTuning.AltSteerRate * dt));
            if (steerW < 1e-4 && !steering) steerW = 0;
            over = overcharge < 0 ? 0 : overcharge > 1 ? 1 : overcharge;
        }

        /// <summary>The offsets now (web camera space), `motion` the effects' share (0: no tremble).</summary>
        public HandsDrawPose Sample(double motion)
        {
            var p = new HandsDrawPose
            {
                pitchDeg = kick.Value(0),
                z = kick.Value(1),
                y = -kick.Value(2),
                rollDeg = kick.Value(3),
                yawDeg = kick.Value(4),
                x = kick.Value(5),
            };
            if (steerW > 0)
            {
                p.x += UnityTuning.AltSteerX * steerW;
                p.y += UnityTuning.AltSteerY * steerW;
                p.pitchDeg += UnityTuning.AltSteerPitchDeg * steerW;
                p.rollDeg += UnityTuning.AltSteerRollDeg * steerW;
            }
            if (over > 0)
            {
                p.z += UnityTuning.AltOverchargeIn * over; // pulled in toward the eye as it fills
                double m = Math.Min(1, motion);
                if (m > 0)
                {
                    double a = UnityTuning.AltOverchargeShakeDeg * over * m, w = 2 * Math.PI * UnityTuning.AltOverchargeHz * clock;
                    p.rollDeg += a * Math.Sin(w);
                    p.pitchDeg += a * 0.5 * Math.Sin(w * 1.37 + 1);
                }
            }
            return p;
        }

        public void Clear()
        {
            kick.Clear();
            steerW = over = 0;
        }
    }

    /// <summary>The R3 jab (tonight's punch pose), pure (HandsN19Tests): HandsPunch's phases with a jab's numbers.</summary>
    public static class HandsJab
    {
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
            double cockT = a.outTime * UnityTuning.JabCockShare;
            double over = hit ? 1 : UnityTuning.HandsPunchWhiffOver;
            double toX = a.punchX + UnityTuning.JabAimX, toY = a.punchY + UnityTuning.JabAimY;
            double fromX, fromY, fromZ, fromP, fromYaw, fromR, endZ, endP, endR, k;
            if (age < cockT)
            {
                // the wind-up: barely loaded, tucked low (an ease-out)
                k = HandsEase.OutCubic(age / cockT);
                fromX = a.restX; fromY = a.restY; fromZ = a.restZ; fromP = a.restPitch; fromYaw = a.restYaw; fromR = a.restRoll;
                toX = UnityTuning.JabCockX; toY = UnityTuning.JabCockY; endZ = UnityTuning.JabCockZ;
                endP = UnityTuning.JabCockPitchDeg; double toYawC = UnityTuning.JabCockYawDeg; endR = UnityTuning.JabCockRollDeg;
                p.w = UnityTuning.JabCockShare * k;
                p.x = fromX + (toX - fromX) * k + px;
                p.y = fromY + (toY - fromY) * k + py;
                p.z = fromZ + (endZ - fromZ) * k;
                p.pitchDeg = fromP + (endP - fromP) * k;
                p.yawDeg = fromYaw + (toYawC - fromYaw) * k;
                p.rollDeg = fromR + (endR - fromR) * k;
                return p;
            }
            if (age < a.outTime)
            {
                // the strike: straight down the middle, accelerating, past the target by the overshoot on the impact frame
                double u = (age - cockT) / (a.outTime - cockT);
                k = HandsEase.InQuad(u);
                fromX = UnityTuning.JabCockX; fromY = UnityTuning.JabCockY; fromZ = UnityTuning.JabCockZ;
                fromP = UnityTuning.JabCockPitchDeg; fromYaw = UnityTuning.JabCockYawDeg; fromR = UnityTuning.JabCockRollDeg;
                endZ = a.punchZ - UnityTuning.JabOverZ * over;
                endP = a.punchPitch;
                endR = a.punchRoll + UnityTuning.JabOverRollDeg * over;
                p.w = UnityTuning.JabCockShare + (1 - UnityTuning.JabCockShare) * k;
                p.x = fromX + (toX - fromX) * k + px;
                p.y = fromY + (toY - fromY) * k + py;
                p.z = fromZ + (endZ - fromZ) * k;
                p.pitchDeg = fromP + (endP - fromP) * k;
                p.yawDeg = fromYaw + (a.punchYaw - fromYaw) * k;
                p.rollDeg = fromR + (endR - fromR) * k;
                return p;
            }
            // the impact frame and after: the snap back (fast first), a hit's rebound, the web's return to rest from out + hold
            double t = age - a.outTime;
            double snapT = hit ? UnityTuning.JabSnapTime : UnityTuning.HandsPunchWhiffSnapTime;
            double s = t < snapT ? 1 - t / snapT : 0;
            double o = s * s * s * over;
            double r = hit && t < UnityTuning.HandsPunchReboundTime + snapT ? Math.Sin(Math.PI * Math.Min(1, t / (UnityTuning.HandsPunchReboundTime + snapT))) : 0;
            double ret = age > a.outTime + a.holdTime ? 1 - HandsDraw.Smooth(HandsDraw.Clamp((age - a.outTime - a.holdTime) / (a.cycleTime - a.outTime - a.holdTime), 0, 1)) : 1;
            endZ = a.punchZ - UnityTuning.JabOverZ * o + UnityTuning.HandsPunchReboundZ * r;
            endP = a.punchPitch + UnityTuning.HandsPunchReboundPitchDeg * r;
            endR = a.punchRoll + UnityTuning.JabOverRollDeg * o;
            k = ret;
            p.w = ret;
            p.x = a.restX + (toX - a.restX) * k + px;
            p.y = a.restY + (toY - a.restY) * k + py;
            p.z = a.restZ + (endZ - a.restZ) * k;
            p.pitchDeg = a.restPitch + (endP - a.restPitch) * k;
            p.yawDeg = a.restYaw + (a.punchYaw - a.restYaw) * k;
            p.rollDeg = a.restRoll + (endR - a.restRoll) * k;
            return p;
        }
    }

    public sealed partial class Viewmodel
    {
        readonly HandsAlt altHands = new HandsAlt();
        bool altOver; // the LONGWAVE's charge is an overcharge (the last railCharge said so)
        int altSteered = -1; // the loadout's `steered` counter last frame
        double altSteerHold; // s the steer pose holds after the sim's last steering step (a frame without a step keeps it)

        void AltBuild()
        {
            game.events.On<HitEvent>("parry", e =>
            {
                if (e != null && e.owner == "player" && e.weapon == "shotgun") altHands.Parry(AltMotion());
            });
            game.events.On("shotgunParry", _ => altHands.Parry(AltMotion())); // builder W's name for the THUNDERCLAP's parry, if it lands so
            game.events.On<DetonateEvent>("nailDetonate", e =>
            {
                if (e != null && e.owner == "player") altHands.Detonate(AltMotion());
            });
            game.events.On<ZapEvent>(N19Events.Discharge, e =>
            {
                if (e != null && e.owner == "player") altHands.Discharge(AltMotion());
            });
            game.events.On("airburst", _ => altHands.Airburst(AltMotion())); // projectiles.js :320 (its payload puts the count in `owner`)
            game.events.On<RailChargeEvent>("railCharge", e =>
            {
                if (e != null && e.owner == "player") altOver = e.over;
            });
        }

        /// <summary>The alt kicks' share: effects intensity, 0 with REDUCED MOTION (the kick is stilled).</summary>
        double AltMotion()
        {
            MovementConfig mc = cfg.movement;
            return mc.settings.reducedMotion ? 0 : Clamp(mc.settings.effectsIntensity, 0, mc.feel.effectsIntensityMax);
        }

        /// <summary>The alts this frame (world time; a freeze holds them), added to the pose sums.</summary>
        void AltFrame(Loadout lo, string held, double animDt, ref double px, ref double py, ref double pz, ref double rx, ref double ry, ref double rz)
        {
            int steered = lo != null ? lo.steered : 0;
            if (altSteered >= 0 && steered > altSteered && held == "rocket") altSteerHold = UnityTuning.AltSteerHold; // the sim steered since the last frame
            else altSteerHold = held == "rocket" ? altSteerHold - animDt : 0;
            bool steering = altSteerHold > 0;
            altSteered = steered;
            double charge = lo != null && held == "rail" ? lo.RailCharge : 0;
            if (!(charge > 0)) altOver = false;
            altHands.Frame(animDt, steering, altOver ? charge : 0);
            HandsDrawPose p = altHands.Sample(AltMotion());
            px += p.x;
            py += p.y;
            pz += p.z;
            rx += p.pitchDeg * Deg;
            ry += p.yawDeg * Deg;
            rz += p.rollDeg * Deg;
        }
    }
}
