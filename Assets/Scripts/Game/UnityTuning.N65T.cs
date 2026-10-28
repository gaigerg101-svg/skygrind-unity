// NIGHT65 builder T (T2 tour65): the tour65 reel's numbers (FeelReel.N65.Tour.cs). Unity-only: a reel reads them, the sim never does (golden
// vectors, replays and fingerprints untouched). The room waits are a157's (UnityTuning.N56L.cs: L56RoomWaitS / L56WaveWaitS / L56SettleS).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const int T65LineFrames = 8; // frames along a room's line65 (the slate's T2: "line65 ... framed as a sequence of 8 frames")
        public const double T65LinePitchMax = 0.45; // rad: a line frame's pitch follows the line's slope, clamped to +- this (a steep pad arc still frames the room, not the sky)
        public const double T65LinePitchBias = -0.08; // rad: a line frame looks this much below the line's slope (the floor and the next piece in view)
        public const double T65InsideFeetH = 0.3; // m above the feet: the low point tested for "inside a solid" (above a floor's top, inside a wall)
        public const double T65InsideEyeH = 1.5; // m above the feet: the eye point tested for "inside a solid" (the camera in a wall)
        public const double T65InsideEps = 0.02; // m: a point this close to a box face counts as outside (the feet stand ON a deck's top)
    }
}
