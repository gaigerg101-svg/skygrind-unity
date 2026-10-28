// NIGHT63 builder D (DECIDE63 D-10, M1's proposal 2: THE EASED EYE; view row design.n63.eye63, OFF in the repo; Tools/night/n63-d-eye63-row.cjs;
// the numbers UnityTuning.N63Eye.cs; the hook CameraFeel.Apply). GG 2026-10-20: "the movement needs to feel cleaner, more fluid, and less
// choppy". M1 (Logs/n63/m1.md) measured the camera's designed events on verbs37: the slide's eye ease (the web's 0.70 m, slide.cameraDrop, in
// 0.10 s, camera.eyeEaseTime: CameraFeel's MoveTowards) starts and stops at full speed in one frame (11.67 cm a frame at 60 fps); the slide dip
// (UnityTuning.SlideDip, 0.15 m in 0.04 s) and the landing dip (UnityTuning.LandDipMaxN18, in 0.03 s) start at full speed too. Each is a peak in
// n63-m1-chop.cjs's residual (the camera path against a centred 9-frame quadratic: 9.04 cm at a slide's entry, 4.55 at its exit, 1-5 at a
// landing dip: the eye's own part).
// THE RULE: the view's eye height above the feet FOLLOWS today's eye less today's dips through a critically damped spring (natural frequency
// UnityTuning.Eye63FollowRate; exact for a target held over the frame, so a frame's length never changes where it settles), and the eye's own
// ease runs a little quicker (UnityTuning.Eye63EaseTimeMult of camera.eyeEaseTime) so the follow's lag keeps the crouch as quick: 90 % of a
// slide's exit in 6 frames at 60 fps as today, half-way one frame later, the drop 83 ms in 66.5 cm (today 69.3). The start and the stop ease
// in: the residual at a slide's entry 9.04 -> 3.20 cm, its exit 4.55 -> 2.02, a landing dip 39 % of today's (N63EyeTests). A tail: within 1 cm
// of the rest at 0.15 s (today 0.10 s). Said plainly: no shape over exactly 0.10 s halves the exit's residual (the best, a smoothstep: 4.55 ->
// 3.54 cm); a plain spring on the step (no inner ease) at the same 0.10 s makes it worse (7-10 cm).
// VIEW ONLY: the sim's eye height (body.eyeHeight: Combat's ray, the bots), the crouch, the hitboxes and every fingerprint are untouched; the
// follow reads the Mover's crouch and the dips CameraFeel already computes. The landings' feet are the sim's: a landing's total residual is
// mostly the feet's stop (M1: the slam's stop is excluded for that reason), and the eye's part is what this halves.
// D1's contract: CameraFeel steps the follow EVERY frame whether the row is on or off (so the next frame on is the frame it would have drawn),
// reads the row every frame (NightRows.Live), and with it off draws today's exact expression; a held frame (dt 0) moves nothing.
// Off: the row off, -skygrindOff63 (or older), -skygrindNoEye63 (this row alone, any case).
using System;
using System.Globalization;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Eye63Math
    {
        /// <summary>The view row's name (design.n63.eye63).</summary>
        public const string Row = "eye63";
        /// <summary>The one-row off switch.</summary>
        public const string NoSwitch = "-skygrindNoEye63";
        /// <summary>The row's night (its -skygrindOffNN switch covers it from this night on).</summary>
        public const int Night = 63;

        static string[] args;

        /// <summary>This process's command line (read once).</summary>
        public static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        /// <summary>A switch turns the row off: -skygrindOffNN with NN &lt;= 63, or -skygrindNoEye63 (any case: NightRows.HasSwitch).</summary>
        public static bool Off(string[] argv) => NightRows.OffFrom(argv, "-skygrindOff") <= Night || NightRows.HasSwitch(argv, NoSwitch);

        /// <summary>The row's own rule (read once at CameraFeel.Wire: its config and the command line); the live value each frame is
        /// NightRows.Live(decided, Row).</summary>
        public static bool Decide(bool rowOn, string[] argv) => rowOn && !Off(argv);

        /// <summary>One frame of a critically damped spring of natural frequency `rate` (1/s) toward `target` held over `dt`: exact (the closed
        /// form of x'' = rate^2 (target - x) - 2 rate x'), so the path does not depend on how the time is cut into frames. dt 0 (a held frame)
        /// or less, or no rate: nothing moves.</summary>
        public static void Spring(ref double x, ref double v, double target, double rate, double dt)
        {
            if (!(dt > 0) || !(rate > 0)) return;
            double d = x - target, e = Math.Exp(-rate * dt), k = (v + rate * d) * dt;
            x = target + (d + k) * e;
            v = (v - rate * k) * e;
        }

        /// <summary>CameraFeel's MoveTowards (the web's eye ease).</summary>
        public static double MoveTowards(double value, double target, double maxStep) =>
            value + Math.Sign(target - value) * Math.Min(Math.Abs(target - value), maxStep);

        /// <summary>The Player log's state line (once, at Wire).</summary>
        public static string Line(bool decided, bool rowOn)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            if (decided)
                return "[eye63] eye63 ON: the eased eye (its own ease x" + UnityTuning.Eye63EaseTimeMult.ToString("0.00", ci) + " of camera.eyeEaseTime, a critically damped follow at "
                    + UnityTuning.Eye63FollowRate.ToString("0", ci) + " /s; view only, read every frame)";
            return "[eye63] eye63 OFF (" + (rowOn ? "-skygrindOff63 or older / " + NoSwitch : "design.n63." + Row + " off") + ")";
        }

        /// <summary>The proof line (L-089: its input fired): the first frame, with the row on, the shown eye is UnityTuning.Eye63FiredM or more
        /// from today's (an ease in progress).</summary>
        public static string FiredLine(double shown, double today, int frame)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            return "[eye63] fired at frame " + frame.ToString(ci) + ": the shown eye " + shown.ToString("0.000", ci) + " m, today's " + today.ToString("0.000", ci) + " m ("
                + (Math.Abs(shown - today) * 100).ToString("0.0", ci) + " cm apart): an ease in progress";
        }
    }

    /// <summary>NIGHT63 eye63: the eased eye's state, one per CameraFeel: its own ease (Eye, a little quicker than the web's) and the follow
    /// (Y, V): the eye height above the feet the camera shows with the row on. Stepped every frame by CameraFeel.Apply, cut on a respawn.</summary>
    public sealed class Eye63Follow
    {
        /// <summary>The eye's own ease (m above the feet): MoveTowards at slide.cameraDrop / (camera.eyeEaseTime x Eye63EaseTimeMult).</summary>
        public double Eye { get; private set; }
        /// <summary>The shown eye (m above the feet): the follow of Eye less the dips.</summary>
        public double Y { get; private set; }
        /// <summary>The follow's speed (m/s).</summary>
        public double V { get; private set; }

        public Eye63Follow(double standEye) { Cut(standEye); }

        /// <summary>A respawn (CameraFeel.Cut): the standing eye, at rest.</summary>
        public void Cut(double standEye)
        {
            Eye = standEye;
            Y = standEye;
            V = 0;
        }

        /// <summary>One frame: `target` is today's eye target (standing or sliding), `dropM` the slide's drop, `easeTime` the web's
        /// camera.eyeEaseTime, `dip` today's dips (the landing's and the slide's, as CameraFeel draws them). Returns the shown eye. dt 0 (a held
        /// frame) or less: nothing moves.</summary>
        public double Step(double target, double dropM, double easeTime, double dip, double dt)
        {
            if (!(dt > 0)) return Y;
            double t = easeTime * UnityTuning.Eye63EaseTimeMult;
            Eye = t > 0 ? Eye63Math.MoveTowards(Eye, target, dropM / t * dt) : target;
            double y = Y, v = V;
            Eye63Math.Spring(ref y, ref v, Eye - dip, UnityTuning.Eye63FollowRate, dt);
            Y = y;
            V = v;
            return Y;
        }
    }
}
