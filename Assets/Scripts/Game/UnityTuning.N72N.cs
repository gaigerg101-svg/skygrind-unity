// NIGHT72 builder N: Unity-only numbers of N's slices (CLAUDE.md rule 5: reel / view / UI numbers live here, each with a comment). No gameplay number: every
// row here is a view row or a reel. No UnityEngine type in this file (the harness reads it). Slices: N1 (the hop row's place and window: HopSite72), N3 (goal72),
// N4 (cam72), N6 (exit63). The specs: Research/2026-10-27-forge72/scout-movement.md, scout-guidance.md, slate.json N1-N6.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- N1: verbs37 row 12 (the bunny hop), the reel's own constants (FeelReel.N35.Verbs.cs row 12), kept as they are: the BAR is unchanged ----
        public const int Hop72Hops = 3; // Jump events the row needs (the bar: "3 hops keep speed")
        public const double Hop72KeptPct = 99.0; // % of the horizontal speed each counted hop must keep (the bar)
        public const double Hop72MinSpeedMps = 0.5; // m/s: a Jump event counts only with a speed before it above this (a jump from a stand is not a hop)
        public const double Hop72FirstPressS = 0.3; // s: the first jump is pressed once the walk speed is up (the row's `t > 0.3`)
        public const double Hop72BufferLeadS = 0.05; // s: a falling player presses its jump when the floor is this far away at its fall speed (inside the 0.1 s jump buffer)
        public const double Hop72HeldS = 0.15; // s: a pressed jump stays held this long
        // ---- N1: what is new (the profile's 1.4 s flat jump made three hops about 26 m; the row's window and place now follow it) ----
        public const double Hop72MinWindowS = 4.0; // s: the row's old window; the window never goes below it
        public const double Hop72MarginPerHopS = 0.5; // s of slack per hop on top of the flat air time (the landing, the press, the hang at the apex)
        public const double Hop72MinLaneM = 8.0; // m: a lane shorter than this is not a place to stage a hop (Open35's own minimum)
        public const double Hop72RealGapOfAir = 0.5; // a retry place's hops count only when each gap between two Jump events is at least this share of the flat air time (a pogo under a low ceiling is not a bunny hop)
        public const int Hop72Retries = 4; // places tried after Open35's own, longest clear lane first, when a run misses the bar (each attempt is printed)
        public const double Hop72ScanM = 60.0; // m: how far the retry scan's rays and floor checks look (Open35 stops at 30 m; three hops are about 26 m)

        // ---- N3: goal72 (Goal72Math.cs the state machine, Goal72View.cs the drawing); the slate's: a line at the thing it names, <= 6 words, >= 22 px, hints at 10 s and 20 s ----
        public const double Goal72Hint1S = 10.0; // s with no progress toward the exit: the line says WHERE (slate N3: 10 s)
        public const double Goal72Hint2S = 20.0; // s with no progress: the line says HOW (slate N3: 20 s)
        public const double Goal72ProgressM = 2.5; // m: the feet must come this much closer than their record in the room to count as progress (resets the clock, retires a hint)
        public const double Goal72UpM = 2.5; // m: an exit this much above the feet is "up" (a jump's reach is 5.07; a step is not a climb), this much below is "down"
        public const double Goal72RideM = 7.5; // m: an exit this much above the feet is past a double jump (9.18 from the lab, minus the landing margin): the hint teaches the ride
        public const double Goal72BehindDeg = 120.0; // deg off the look: the exit is behind the player
        public const double Goal72FirstLevelS = 5.0; // s: no line in a level's first seconds (GG's "way too much instruction at the start")
        public const double Goal72LabelRiseM = 2.6; // m over the exit's floor point: the line and its marker hang here (the doorway's height is about 4 m)
        public const float Goal72MarkerPx = 54f; // px at 1080p: the marker's arrowhead
        public const float Goal72PointerPx = 46f; // px: stage 3's pointer arrowhead on the ring round the crosshair
        public const float Goal72PointerRadiusPx = 170f; // px from the screen centre: where that pointer rides
        public const float Goal72EdgeInsetPx = 120f; // px: an exit off screen keeps its marker this far inside the edge
        public const float Goal72LowestPx = 220f; // px: the marker never sits lower than this above the bottom (the gun, the HP plate, the ammo)
        public const float Goal72PulseHz = 1.3f; // stage 2-3: the marker and the line breathe at this rate (a calm pulse, no full-screen flicker)
        public const float Goal72PulseDepth = 0.18f; // stage 2-3: the scale swing of that breath
        public const float Goal72SlideInPx = 36f; // px at 1080p: the line enters this far above its place and eases down (a cubic ease-out); a changed line enters again
        public const float Goal72SlideS = 0.32f; // s: that slide
        public const float Goal72FadeInS = 0.35f; // s: the line fades in (never pops)
        public const float Goal72FadeOutS = 0.25f; // s: and out
        public const float Goal72FarAlpha = 0.9f; // the line's alpha beyond Goal72FarM
        public const float Goal72FarM = 40f; // m: farther than this the line is Goal72FarAlpha, nearer it is 1
        public const float Goal72LineWidthPx = 760f; // px at 1080p: the line's box (six words at 24 px fit)
        public const float Goal72LineHeightPx = 44f; // px
        // ---- N4: cam72 (Cam72Math.cs, applied in CameraFeel.cs). ULTRAKILL's camera, Research/2026-10-27-forge72/scout-movement.md 2 (a mod author's port: 'C', unverified by us) ----
        public const double Cam72RollDeg = 1.0; // deg of roll at full strafe input (the port: target = -strafe input, +-1 deg)
        public const double Cam72RollBoostMult = 5.0; // x while dashing or sliding (+-5 deg)
        public const double Cam72RollDegPerS = 25.0; // deg/s x (|error| + 0.01) toward the target
        public const double Cam72RollBoostedDegPerS = 100.0; // the same while dashing or sliding
        public const double Cam72RollReturnS = 0.5; // s: the smooth damp back to 0 when the input lets go
        public const double Cam72FovInFrac = 0.05; // a dash / slide forward: FOV x(1 - 1/20)
        public const double Cam72FovOutFrac = 0.10; // backward: FOV x(1 + 1/10)
        public const double Cam72FovDegPerS = 300.0; // deg/s: the FOV change comes and goes at this rate (MoveTowards)
        public const double Cam72SlideDropM = 0.32; // m: the slide's eye drop (0.625 u x 0.5143, the 1.8 m body)
        public const double Cam72DropRateBaseM = 0.5; // m: the drop's rate is (distance + this) x Cam72DropRateMult per second
        public const double Cam72DropRateMult = 10.0; // 1/s
        public const double Cam72EyeClearM = 0.12; // m: the eye stays this far under a ceiling over a slide gap (the slide capsule is 1.0 m: the camera must not look through the gap's roof)
        public const double Cam72CeilingRayFromM = 0.9; // m over the feet: where the ceiling ray starts (inside the slide capsule)
        public const double Cam72DotMin = 0.05; // the dash / slide direction's horizontal speed under this: no FOV change (no direction)
        public const double Goal72Stuck72WindowS = 22.0; // s of game time the stuck72 reel idles a pass: past the second hint's 20 s with a margin (reel only)
        public const int Goal72SortingOrder = 9; // the canvas: under the HUD's (10), over the warning chevrons (8)
        public const string Goal72InkHex = "0B1018"; // the line's plate and the marker's rim (the HUD's ink)
    }
}
