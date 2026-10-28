// Port of skygrind/src/combat/assistMath.js (POWER.md P2, OVERNIGHT2 N1, POLISH V1): the pure aim-assist math, bit for
// bit with the web (PortExport/vectors/assistMath.json).
// - The pre-lock (V1): per-mode numbers scaled by SNAP STRENGTH and the ADS zoom, a candidate score, a critically damped
//   spring that pulls the look onto the aim point, hysteresis between two candidates, the fire gate.
// - Look math: yaw / pitch toward a point, look errors, the projectile lead.
// - Homing: a projectile's velocity turns toward a target at a capped rate, speed unchanged.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class AssistMath
    {
        const double DEG = Math.PI / 180;

        static double Len(double x, double y, double z) => Math.Sqrt(x * x + y * y + z * z);

        public sealed class ModeSpec
        {
            public double cone, acquire, pull, omega, follow, friction, breakaway, pulse;
            public bool track;
        }

        public sealed class FireSpec
        {
            public double onTarget, holdBelow, maxHold, boost;
        }

        public sealed class Weights
        {
            public double angle, distance, threat, head, weak;
        }

        /// <summary>config.aim.lock.</summary>
        public sealed class L
        {
            public double strengthRef, kMax, losSlack, range, meleeReach, releaseCone, losLost, manualLosLost, hysteresisMargin, hysteresisHold;
            public double settleError, center, reacquireDelay, acquireMaxTurn, assistedPull, assistedWindow, targetRadius, eliteScale;
            public double payoutMin, payoutFloor, bracketTime, bracketFrom, flashTime, adsCone, adsPull;
            public int scanEvery, losRays;
            public readonly Dictionary<string, ModeSpec> modes = new Dictionary<string, ModeSpec>();
            public readonly Dictionary<string, double> threat = new Dictionary<string, double>();
            public readonly List<string> elite = new List<string>();
            public FireSpec fire;
            public Weights weights;
        }

        public static FireSpec Fire(Dictionary<string, object> f) => new FireSpec
        {
            onTarget = PortData.Num(f["onTarget"]), holdBelow = PortData.Num(f["holdBelow"]), maxHold = PortData.Num(f["maxHold"]), boost = PortData.Num(f["boost"]),
        };

        public static Weights WeightsFrom(Dictionary<string, object> w) => new Weights
        {
            angle = PortData.Num(w["angle"]), distance = PortData.Num(w["distance"]), threat = PortData.Num(w["threat"]), head = PortData.Num(w["head"]), weak = PortData.Num(w["weak"]),
        };

        public static L From(GameConfig cfg) => FromObj(cfg.Obj("aim.lock"));

        public static L FromObj(Dictionary<string, object> o)
        {
            double N(string k) => PortData.Num(o[k]);
            var hy = PortData.Obj(o["hysteresis"]);
            var ads = PortData.Obj(o["ads"]);
            var lk = new L
            {
                strengthRef = N("strengthRef"), kMax = N("kMax"), losSlack = N("losSlack"), range = N("range"), meleeReach = N("meleeReach"),
                releaseCone = N("releaseCone"), losLost = N("losLost"), manualLosLost = N("manualLosLost"),
                hysteresisMargin = PortData.Num(hy["margin"]), hysteresisHold = PortData.Num(hy["hold"]),
                settleError = N("settleError"), center = N("center"), reacquireDelay = N("reacquireDelay"), acquireMaxTurn = N("acquireMaxTurn"),
                assistedPull = N("assistedPull"), assistedWindow = N("assistedWindow"), targetRadius = N("targetRadius"), eliteScale = N("eliteScale"),
                payoutMin = N("payoutMin"), payoutFloor = N("payoutFloor"), bracketTime = N("bracketTime"), bracketFrom = N("bracketFrom"), flashTime = N("flashTime"),
                adsCone = PortData.Num(ads["cone"]), adsPull = PortData.Num(ads["pull"]),
                scanEvery = (int)N("scanEvery"), losRays = (int)N("losRays"),
                fire = Fire(PortData.Obj(o["fire"])), weights = WeightsFrom(PortData.Obj(o["weights"])),
            };
            foreach (var kv in PortData.Obj(o["modes"]))
            {
                var m = PortData.Obj(kv.Value);
                lk.modes[kv.Key] = new ModeSpec
                {
                    cone = PortData.Num(m["cone"]), acquire = PortData.Num(m["acquire"]), pull = PortData.Num(m["pull"]), omega = PortData.Num(m["omega"]),
                    follow = PortData.Num(m["follow"]), friction = PortData.Num(m["friction"]), breakaway = PortData.Num(m["breakaway"]),
                    track = (bool)m["track"], pulse = PortData.Num(m["pulse"]),
                };
            }
            foreach (var kv in PortData.Obj(o["threat"])) lk.threat[kv.Key] = PortData.Num(kv.Value);
            foreach (object e in PortData.Arr(o["elite"])) lk.elite.Add((string)e);
            return lk;
        }

        /// <summary>lockParams' result: cone deg, acquireTime s, speeds deg/s, omega 1/s.</summary>
        public sealed class Params
        {
            public double cone, acquireTime, maxPull, omega, follow, friction, breakaway, pulse, k;
            public bool track;
        }

        public static double Lerp(double a, double b, double t) => a + (b - a) * t;

        /// <summary>Angle (rad) between the unit aim `dir` and the direction from `eye` to `point` (Infinity at the eye).</summary>
        public static double AngleTo(Vec3 eye, Vec3 dir, Vec3 point)
        {
            double dx = point.x - eye.x, dy = point.y - eye.y, dz = point.z - eye.z;
            double d = Len(dx, dy, dz);
            if (d < 1e-6) return double.PositiveInfinity;
            double c = (dx * dir.x + dy * dir.y + dz * dir.z) / d;
            return JsMath.Acos(Math.Max(-1, Math.Min(1, c)));
        }

        /// <summary>The pre-lock numbers for a mode at a SNAP STRENGTH (0..100) and ADS state; null for OFF / unknown / strength 0.</summary>
        public static Params LockParams(string mode, double strength, bool ads, L lk)
        {
            var output = new Params();
            return LockParamsInto(output, mode, strength, ads, lk) ? output : null;
        }

        /// <summary>LockParams into a caller's object (the per-step call allocates nothing; U2-02); false for no lock.</summary>
        public static bool LockParamsInto(Params output, string mode, double strength, bool ads, L lk)
        {
            if (mode == null || !lk.modes.TryGetValue(mode, out ModeSpec m)) return false;
            double k = Math.Min(lk.kMax, Math.Max(0, strength / lk.strengthRef));
            if (k <= 0) return false;
            double cone = ads ? lk.adsCone : 1;
            double pull = ads ? lk.adsPull : 1;
            output.cone = m.cone * k * cone;
            output.acquireTime = (m.acquire / 1000) / k;
            output.maxPull = m.pull * k * pull;
            output.omega = m.omega * Math.Sqrt(k) * pull;
            output.follow = m.follow * Math.Max(1, k);
            output.friction = Lerp(1, m.friction, Math.Min(1, k));
            output.breakaway = m.breakaway * (0.5 + 0.5 * k);
            output.track = m.track;
            output.pulse = m.pulse;
            output.k = k;
            return true;
        }

        /// <summary>An assisted hit's payout at SNAP STRENGTH 0..100: lerp(1, payoutMin) by strength, never under payoutFloor.</summary>
        public static double LockPayout(double strength, L lk)
        {
            double s = Math.Max(0, Math.Min(1, strength / 100));
            return Math.Max(lk.payoutFloor, Lerp(1, lk.payoutMin, s));
        }

        public static double AssistPayout(double amount, bool assisted, double payout) => assisted ? amount * payout : amount;

        /// <summary>A lock candidate: angle (rad) to the aim, distance (m), threat 0..1, a visible head, a boss weak point.</summary>
        public sealed class Candidate
        {
            public object id;
            public double angle, distance, threat;
            public bool head, weak;
            public Enemy enemy;
            public Vec3 chest, headAt;
            public int rejected; // the scan that found it behind a wall
        }

        public static double LockScore(Candidate c, double coneRad, double range, Weights w)
        {
            double aim = 1 - Math.Min(1, c.angle / Math.Max(1e-6, coneRad));
            double near = 1 - Math.Min(1, c.distance / range);
            return aim * w.angle + near * w.distance + c.threat * w.threat + (c.head ? w.head : 0) + (c.weak ? w.weak : 0);
        }

        /// <summary>The best candidate inside the cone and range (by LockScore), or null; `skip` drops candidates.</summary>
        public static Candidate PickBest(List<Candidate> candidates, double coneRad, double range, Weights w, Func<Candidate, bool> skip = null)
        {
            Candidate best = null;
            double bestScore = double.NegativeInfinity;
            foreach (Candidate c in candidates)
            {
                if (c.angle > coneRad || c.distance > range || (skip != null && skip(c))) continue;
                double score = LockScore(c, coneRad, range, w);
                if (score > bestScore)
                {
                    best = c;
                    bestScore = score;
                }
            }
            return best;
        }

        public sealed class Rival
        {
            public object id;
            public double t;
        }

        /// <summary>Hysteresis: the challenger takes over only after scoring `margin` better for `hold` s in a row.</summary>
        public static bool Challenge(Rival state, double currentScore, object challengerId, double challengerScore, double dt, double margin, double hold)
        {
            if (challengerId == null || !(challengerScore > currentScore * (1 + margin)))
            {
                state.id = null;
                state.t = 0;
                return false;
            }
            if (!Equals(state.id, challengerId))
            {
                state.id = challengerId;
                state.t = 0;
            }
            state.t += dt;
            return state.t >= hold - 1e-9;
        }

        /// <summary>One exact step of a critically damped spring pulling x toward 0: the new offset and velocity.</summary>
        public static (double x, double v) SpringStep(double x, double v, double omega, double dt)
        {
            double e = JsMath.Exp(-omega * dt);
            double c = v + omega * x;
            return ((x + c * dt) * e, (v - omega * c * dt) * e);
        }

        /// <summary>The fire gate: does a press leave on this step (on target, far off, not closing, or held too long)?</summary>
        public static bool FireReady(bool onTarget, double errorDeg, double prevErrorDeg, double heldFor, FireSpec fire)
        {
            if (onTarget || errorDeg < fire.onTarget) return true;
            if (errorDeg >= fire.holdBelow) return true;
            if (!(errorDeg < prevErrorDeg - 1e-4)) return true; // not closing: no reason to wait
            return heldFor > fire.maxHold + 1e-9;
        }

        /// <summary>A flick away faster than the breakaway speed (and only one that took the crosshair farther off).</summary>
        public static bool BreaksAway(double userTurnRad, double dt, double breakawayDeg, double errorBefore = 0, double errorAfter = 1) =>
            dt > 0 && (userTurnRad / dt) / DEG > breakawayDeg && errorAfter > errorBefore;

        /// <summary>Turn `vel` toward the target by at most `maxTurn` rad (a slerp, speed kept); returns the angle turned.</summary>
        public static double HomeVelocity(ref Vec3 vel, Vec3 from, Vec3 target, double maxTurn)
        {
            double speed = Len(vel.x, vel.y, vel.z);
            if (speed < 1e-6) return 0;
            double vx = vel.x / speed, vy = vel.y / speed, vz = vel.z / speed;
            double tx = target.x - from.x, ty = target.y - from.y, tz = target.z - from.z;
            double td = Len(tx, ty, tz);
            if (td < 1e-6) return 0;
            tx /= td;
            ty /= td;
            tz /= td;
            double angle = JsMath.Acos(Math.Max(-1, Math.Min(1, vx * tx + vy * ty + vz * tz)));
            if (angle < 1e-6) return 0;
            double k = Math.Min(1, maxTurn / angle);
            double s = JsMath.Sin(angle);
            double a = JsMath.Sin((1 - k) * angle) / s;
            double b = JsMath.Sin(k * angle) / s;
            vel = new Vec3((vx * a + tx * b) * speed, (vy * a + ty * b) * speed, (vz * a + tz * b) * speed);
            return angle * k;
        }

        /// <summary>Cycling: the first candidate (sorted by angle) after the current one, wrapping; null when there is no other.</summary>
        public static Candidate NextLock(List<Candidate> sorted, object currentId)
        {
            if (sorted.Count == 0) return null;
            int i = sorted.FindIndex(c => Equals(c.id, currentId));
            if (i < 0) return sorted[0];
            if (sorted.Count == 1) return null;
            return sorted[(i + 1) % sorted.Count];
        }

        /// <summary>Wrap an angle to (-PI, PI].</summary>
        public static double WrapAngle(double a)
        {
            double x = a % (2 * Math.PI);
            if (x > Math.PI) x -= 2 * Math.PI;
            if (x <= -Math.PI) x += 2 * Math.PI;
            return x;
        }

        /// <summary>The look (yaw, pitch) from `eye` toward `point` (yaw 0 looks toward -z).</summary>
        public static (double yaw, double pitch) YawPitchTo(Vec3 eye, Vec3 point)
        {
            double dx = point.x - eye.x, dy = point.y - eye.y, dz = point.z - eye.z;
            return (JsMath.Atan2(-dx, -dz), JsMath.Atan2(dy, JsMath.Hypot(dx, dz)));
        }

        public static Vec3 LookDir(double yaw, double pitch)
        {
            double cp = JsMath.Cos(pitch);
            return new Vec3(-JsMath.Sin(yaw) * cp, JsMath.Sin(pitch), -JsMath.Cos(yaw) * cp);
        }

        public static double LookError(double yaw, double pitch, double yaw2, double pitch2)
        {
            Vec3 a = LookDir(yaw, pitch), b = LookDir(yaw2, pitch2);
            return JsMath.Acos(Math.Max(-1, Math.Min(1, a.x * b.x + a.y * b.y + a.z * b.z)));
        }

        /// <summary>Where to aim a projectile at a moving target: its point plus velocity x flight time, refined twice.</summary>
        public static Vec3 LeadPoint(Vec3 eye, Vec3 point, Vec3 vel, double speed)
        {
            Vec3 output = point;
            if (!(speed > 0)) return output;
            for (int i = 0; i < 2; i++)
            {
                double t = JsMath.Hypot(output.x - eye.x, output.y - eye.y, output.z - eye.z) / speed;
                output = new Vec3(point.x + vel.x * t, point.y + vel.y * t, point.z + vel.z * t);
            }
            return output;
        }
    }
}
