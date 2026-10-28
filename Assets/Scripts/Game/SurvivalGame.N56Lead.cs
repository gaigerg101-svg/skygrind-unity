// NIGHT56 (the lead; NIGHT55 P0-smoke, gate-r1 / gate-r2: "a test ... that drops focus while Playing and asserts flow -> Paused"): the
// focus-loss pause's rule (SurvivalGame.cs's Update: the cursor lock drops when the window loses the focus or on Esc), held in one pure
// function so an EditMode test can pin it. Behaviour unchanged: the same five conditions as the inline line it replaces. Outside
// -skygrindSmoke nothing undoes the pause (the smoke's SmokeGuard55, SliceSmoke.N55.cs, is a component only the smoke adds).
namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        /// <summary>True when this frame's cursor-lock drop pauses the game: Playing, locked last frame, unlocked now, not the attract,
        /// not inside a reel (NIGHT18: another window taking the focus must not pause a scenario).</summary>
        public static bool PausesOnLockDrop(Flow flow, bool wasLocked, bool locked, bool attractOn, bool reelRunning)
            => flow == Flow.Playing && wasLocked && !locked && !attractOn && !reelRunning;
    }
}
