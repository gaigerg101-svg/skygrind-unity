// NIGHT17 U3-30.3 (the lead's): the feel reel's numbers (`SKYGRIND.exe -skygrindReel <dir>`: FeelStrip's reel scenarios,
// then ShotTour's reel pass over the screens). Capture tooling only: none is a gameplay or a render number.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // kill / headshot / punchkill (the reel's names): frames every gap-th, past the freeze into the blocks' release.
        public const int ReelKillFrames = 16;
        public const int ReelKillGap = 2;
        // horde: this many husks at 1 HP in a knot ahead, the gun that blasts them (the THUNDERCLAP), frames every gap-th.
        public const int ReelHordeCount = 5;
        public const string ReelHordeGun = "shotgun";
        public const int ReelHordeFrames = 16;
        public const int ReelHordeGap = 2;
        // hit: the husk's HP is raised to at least this, so the first hits never kill (a hit that lands and doesn't kill).
        public const double ReelHitHp = 400;
        // boost / sprint: run this long (s) before the boost key; the sprint's trigger this long (s) after the dash starts.
        public const double ReelBoostRunIn = 0.35;
        public const double ReelSprintAfter = 0.45;
        public const int ReelBoostFrames = 14;
        public const int ReelBoostGap = 2;
        public const int ReelSprintFrames = 12;
        public const int ReelSprintGap = 4;
        // rankup: this many husks spread by this many degrees each side; frames every gap-th from the rank's change.
        public const int ReelRankUpHusks = 8;
        public const double ReelRankUpSpreadDeg = 14;
        public const int ReelRankUpFrames = 12;
        public const int ReelRankUpGap = 2;
        // roundend: frames every gap-th from round 1's clear (the round's end beat, the break's start).
        public const int ReelRoundEndFrames = 20;
        public const int ReelRoundEndGap = 6;
        // spawn / pickup (U3-30.10's evidence): frames every gap-th; the power-up this far ahead (m), of this type, walked into
        // at this share of a walk with the camera this far down (deg), the trigger this far from it (m: 1 m outside the grab).
        public const int ReelBeyondFrames = 14;
        public const int ReelBeyondGap = 2;
        public const double ReelPickupAhead = 4.5;
        public const string ReelPickupType = "double";
        public const double ReelPickupWalk = 0.6;
        public const double ReelPickupPitchDeg = -22;
        public const double ReelPickupTriggerOutside = 1.0; // the trigger this far outside powerUps.grabRadius (where the pull starts)
        // A strip holds the first break this many steps (90 s at 120 Hz: longer than any scenario's cap), so the top line
        // reads "ROUND 1 IN 90", not the old sentinel's "ROUND 1 IN 8947848" (the round-1 reviewer read it as a leak).
        public const int ReelHoldBreakSteps = 120 * 90;
        // The screens pass: seconds to let a screen's entrance land before its capture, and the fight's aim-bot time.
        public const float ReelScreenSettle = 0.9f;
        public const float ReelFightTime = 3.5f;
        // NIGHT19: the `gust` scenario's second LT aims this far down (a gust-jump's aim); `vitals` kills a husk this far ahead
        // (inside design.stamina.n19's nearRange of 6 m, so the close kill's refill shows).
        public const double ReelGustDownDeg = 70;
        public const double ReelVitalsKillAt = 5;
        /// <summary>NIGHT20: the vitals scenario's sprint turns this many deg/s (a ~6 m circle at the sprint's speed), so the start area stays open for the kill (NIGHT19's straight sprint ran into walls: NO SPOT).</summary>
        public const double ReelVitalsTurnDegPerS = 120;
        /// <summary>NIGHT20: the slam scenario's height over the floor (m): high enough that the slam passes the speed lines' 9 m/s gate (M8 v2 L4's vertical case).</summary>
        public const double ReelSlamHeight = 8;
    }
}
