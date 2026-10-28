// NIGHT16 (the lead; the reviewer's finding 7): one gate for every flash the feel night adds or touches (WCAG 2.3.1: at most
// three flashes in any one second): the impact layer's headshot-kill flash (ImpactFrames), the HUD's parry flash
// (UiFeel), the hands' knuckle flash and glint and the draws' glow peaks (Viewmodel). Before, the hands and the impact
// layer each kept their own three-a-second ring, so together they could pass six. A flash asks TryFlash at its start: true
// (the flash plays and is counted) or false (it is skipped this time: the moment's freeze and pictures stay, only the
// luminance pop is dropped). Real time (unscaled), so a freeze or a slow-mo can't stretch the window. No allocation.
namespace Skygrind.Game
{
    public sealed class FlashGate
    {
        /// <summary>WCAG 2.3.1's count and window.</summary>
        public const int PerWindow = 3;
        public const double Window = 1.0;

        readonly double[] flashAt = new double[PerWindow];
        int flashHead;

        public FlashGate() => Clear();

        /// <summary>Forget every flash (a new run, a test).</summary>
        public void Clear()
        {
            for (int i = 0; i < flashAt.Length; i++) flashAt[i] = double.NegativeInfinity;
            flashHead = 0;
        }

        /// <summary>May a flash start at `now` (real s, a monotonic clock)? True and counted, or false (three already in the last Window).</summary>
        public bool TryFlash(double now)
        {
            if (now - flashAt[flashHead] < Window) return false; // the oldest of the last three is under a second old
            flashAt[flashHead] = now;
            flashHead = (flashHead + 1) % flashAt.Length;
            return true;
        }
    }

    public sealed partial class SurvivalGame
    {
        /// <summary>NIGHT16: the game's one flash gate (FlashGate). Ask with RealNow.</summary>
        public readonly FlashGate flashGate = new FlashGate();

        /// <summary>The flash gate's clock: real seconds since start-up (unscaled, monotonic).</summary>
        public static double RealNow => FeelClock.RealNow; // NIGHT18: the wall clock, or the reel's fixed clock while it records
    }
}
