// NIGHT57 builder G: Unity-only numbers of G's reels (FeelReel.N57.Play.cs, a257). Probe layout only: no gameplay number.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the a257 reel (G2 press57's Player proof) ---
        /// <summary>a257: the spitter's distance ahead on the clear line (m): inside its spitter.minRange 6 - maxRange 24 band, near enough to read
        /// on a 1080p frame.</summary>
        public const double Press57ReelAhead = 12;
        /// <summary>a257: the look's pitch (deg, down): the floor round the feet (the 3 m circle) at the frame's bottom and the spitter 12 m ahead
        /// in it (camera.fovDeg 74).</summary>
        public const double Press57ReelPitchDeg = -25;
        /// <summary>a257: frames a phase waits for the spitter's next mark (and for a mark to land) before it gives up (10 s at 60 fps).</summary>
        public const int Press57ReelWaitFrames = 600;
        /// <summary>a257: the tell frame is taken this long after the mark (s of play: the circle part filled, the tell 1.5-2.3 s long).</summary>
        public const double Press57ReelShotS = 0.6;
        /// <summary>a257: frames the stick is held back on the moving phase's mark (0.75 s at 60 fps: ~5 m at the walk speed, out of the 3 m
        /// circle and still in the frame ahead; the circle lasts 1.5-2.3 s, the spitter stays inside its 24 m range).</summary>
        public const int Press57ReelMoveFrames = 45;
        /// <summary>a257: frames after a landing the phase still counts the hurts (the acid's first ticks, 0.5 s at 60 fps).</summary>
        public const int Press57ReelAfterFrames = 30;
    }
}
