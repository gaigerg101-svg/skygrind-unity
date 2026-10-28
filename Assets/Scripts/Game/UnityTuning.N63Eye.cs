// NIGHT63 builder D (DECIDE63 D-10, M1's proposal 2: THE EASED EYE; Eye63Math.N63D.cs, CameraFeel.cs; design.n63.eye63, a VIEW row): Unity-only
// view numbers, each with its reason. No gameplay number: the sim's eye height (body.eyeHeight: Combat's ray, the bots), the crouch, its timing
// and its events are the web's and untouched; these only move the camera. Measured offline on NIGHT62's freeze telemetry (Logs/feel/n62-freeze
// verbs37, the five levels) through n63-m1-chop.cjs's own residual (a 9-frame quadratic Savitzky-Golay fit at 60 fps); the tests pin the result.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>eye63: the eased eye's own ease (the 0.70 m slide drop, slide.cameraDrop) takes this share of the web's camera.eyeEaseTime
        /// (0.1 s -> 0.08 s), so the ease plus the follow's lag keep the crouch as quick as today: 90 % of a slide's exit in 6 frames at 60 fps as
        /// today, its half-way one frame later, the drop 83 ms into a slide 66.5 cm (today 69.3). At 1 (the web's time) the follow alone makes a
        /// slower crouch (the drop at 83 ms 56.4 cm).</summary>
        public const double Eye63EaseTimeMult = 0.8;

        /// <summary>eye63: the critically damped follow's natural frequency (1/s). The view's eye height follows today's eye less the dips (the
        /// slide dip, the landing dip) through it, so its start and its stop ease in instead of jumping to full speed in one frame: the chop
        /// tool's residual at a slide's entry 9.04 -> 3.20 cm, at its exit 4.55 -> 2.02, at a landing dip 39 % of today's (the eye's own part).
        /// Lower is smoother and slower (50: the exit 0.17 s to within 1 cm), higher snaps again (100 on the 0.08 s ease: the entry 5.75 cm).</summary>
        public const double Eye63FollowRate = 70;

        /// <summary>eye63: the proof line's trigger (m): the first frame, with the row on, the shown eye is this far from today's eye
        /// ("[eye63] fired ..."): an ease in progress, which today's path has no way to show. 5 mm: under it the two read the same.</summary>
        public const double Eye63FiredM = 0.005;
    }
}
