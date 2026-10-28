// NIGHT55 S: Unity-only numbers of builder S's reels (FeelReel.N55.Smoke.cs: a555 and a152's T-125 block). Harness numbers, no gameplay.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const int T125Taps = 6; // the THUNDERCLAP taps per pass (n52-r20's six taps, T-125)
        public const int T125TapFrames = 3; // each tap holds the trigger this many reel frames (n52-r20's 3-frame taps), counted in sim steps at ReelFps so a fast clock cannot drop it
        public const int T125TapMarginSteps = 12; // sim steps past the gun's own cycle before the next tap (0.1 s at 120 Hz: every tap meets a ready gun)
        public const int T125WantFires = 4; // GREEN at this many THUNDERCLAP "fire" events of the six taps (slate P0-t125 doneWhen)
        public const double T125TapPitchRad = 1.2; // the look during the taps (about 69 deg up): the pellets hit no frozen husk, so no kill clears the room mid-reel
        public const float T125WallCapS = 30f; // real seconds a pass of taps may take at most
    }
}
