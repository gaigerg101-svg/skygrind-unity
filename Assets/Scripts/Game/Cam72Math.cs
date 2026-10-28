// NIGHT72 builder N, slice N4 (view row design.n72.cam72; GG 2026-10-26: the movement "almost a one to one copy" of ULTRAKILL; scout-movement.md 2, a mod author's port of ULTRAKILL's CameraController, 'C':
// one source, unverified by us): the ULTRAKILL camera's three motions, PURE (no Unity types: the harness `n72cam` and the EditMode tests drive it); CameraFeel.cs applies them.
//   STRAFE ROLL  target = the strafe input (-1..1) x 1 deg, x5 while dashing or sliding. Toward a target: 25 deg/s x (|error| + 0.01) (100 deg/s boosted); back to 0: a smooth damp, 0.5 s.
//   DASH FOV     a dash or slide FORWARD narrows the field of view x(1 - 1/20) (0.95), BACKWARD widens it x(1 + 1/10) (1.1), sideways nothing; the change returns at 300 deg/s.
//   SLIDE DROP   the eye drops 0.32 m (ULTRAKILL's 0.625 u x0.5143) while sliding, eased at (distance + 0.5) x 10 m/s; CameraFeel never lets the eye above the ceiling over a slide gap (the
//                capsule is 1.0 m tall while sliding, so a 1.28 m eye would look through a low ceiling): the eye is clamped under it.
// Numbers: UnityTuning.N72N.cs (Cam72*). With the row on, it replaces the web's strafe lean, slide tilt, dash / slide FOV widen and the slide's eye drop (the wall ride's lean, the speed ramp, the
// landing dip and every shake stay). REDUCED MOTION: no roll, no FOV change (the same stills as the web's). View only.
using System;

namespace Skygrind.Game
{
    public static class Cam72Math
    {
        public const string Row = "cam72";
        /// <summary>The one-row off switch (Rows72.cs reads -skygrindNo&lt;Row&gt;72).</summary>
        public const string NoSwitch = "-skygrindNoCam72";

        /// <summary>The roll the strafe input asks for (deg; + leans right).</summary>
        public static double RollTarget(double strafeInput, bool boosted) =>
            Math.Max(-1, Math.Min(1, strafeInput)) * UnityTuning.Cam72RollDeg * (boosted ? UnityTuning.Cam72RollBoostMult : 1);

        /// <summary>One frame of the roll toward `target` (deg): the stepped approach toward a target, a smooth damp back to 0.</summary>
        public static double RollStep(double roll, ref double vel, double target, bool boosted, double dt)
        {
            if (Math.Abs(target) < 1e-9) return SmoothDamp(roll, 0, ref vel, UnityTuning.Cam72RollReturnS, dt);
            vel = 0;
            double err = target - roll;
            double rate = (boosted ? UnityTuning.Cam72RollBoostedDegPerS : UnityTuning.Cam72RollDegPerS) * (Math.Abs(err) + 0.01);
            return MoveTowards(roll, target, rate * dt);
        }

        /// <summary>The FOV factor of a dash / slide whose direction has `dot` with the look's forward (1 straight ahead, -1 straight back): 0.95 ahead, 1.1 back, 1 sideways; 1 when not dashing / sliding.</summary>
        public static double FovFactor(double dot, bool active)
        {
            if (!active) return 1;
            dot = Math.Max(-1, Math.Min(1, dot));
            return dot >= 0 ? 1 - UnityTuning.Cam72FovInFrac * dot : 1 + UnityTuning.Cam72FovOutFrac * -dot;
        }

        /// <summary>The FOV offset (deg) one frame later: it follows `targetDeg` at most Cam72FovDegPerS.</summary>
        public static double FovStep(double offsetDeg, double targetDeg, double dt) => MoveTowards(offsetDeg, targetDeg, UnityTuning.Cam72FovDegPerS * dt);

        /// <summary>The slide eye drop (m) one frame later, toward `target` (0 or Cam72SlideDropM) at (distance + 0.5) x 10 m/s.</summary>
        public static double DropStep(double drop, double target, double dt) => MoveTowards(drop, target, (Math.Abs(target - drop) + UnityTuning.Cam72DropRateBaseM) * UnityTuning.Cam72DropRateMult * dt);

        /// <summary>The eye height (m over the feet) with the slide drop, never above `ceilingM - Cam72EyeClearM` (the ceiling over the player, m over the feet; +infinity: none).</summary>
        public static double EyeY(double standEye, double drop, double ceilingM) => Math.Min(standEye - drop, ceilingM - UnityTuning.Cam72EyeClearM);

        public static double MoveTowards(double a, double b, double maxDelta) => Math.Abs(b - a) <= maxDelta ? b : a + Math.Sign(b - a) * maxDelta;

        /// <summary>Unity's Mathf.SmoothDamp (the critically damped spring ULTRAKILL's camera uses for the roll's return).</summary>
        public static double SmoothDamp(double current, double target, ref double velocity, double smoothTime, double dt)
        {
            smoothTime = Math.Max(0.0001, smoothTime);
            double omega = 2.0 / smoothTime, x = omega * dt;
            double exp = 1.0 / (1.0 + x + 0.48 * x * x + 0.235 * x * x * x);
            double change = current - target;
            double temp = (velocity + omega * change) * dt;
            velocity = (velocity - omega * temp) * exp;
            double output = target + (change + temp) * exp;
            if ((target - current > 0) == (output > target)) { output = target; velocity = (output - target) / dt; }
            return output;
        }
    }
}
