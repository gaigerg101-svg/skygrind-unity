// NIGHT56 (the lead; NIGHT55 P0-smoke's note "outside -skygrindSmoke the game still pauses on focus loss (a test)"): the focus-loss pause
// (SurvivalGame.PausesOnLockDrop, SurvivalGame.N56Lead.cs; the call in SurvivalGame.cs's Update). Pinned:
//   1. the window losing the focus while Playing (the cursor lock drops: locked last frame, unlocked now) pauses the game
//   2. no pause when the cursor was not locked, outside Playing (Title / Paused / Down / Results), in the attract, or inside a reel
//   3. outside the smoke nothing undoes it: the only code that resolves a foreign pause is the smoke's SmokeGuard55, a component the smoke
//      alone adds (SliceSmoke.N55Begin); the rule itself never reads the smoke
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class N56FocusPauseTests
    {
        [Test]
        public void FocusLostWhilePlaying_Pauses()
        {
            Assert.IsTrue(SurvivalGame.PausesOnLockDrop(Flow.Playing, true, false, false, false), "Playing, the lock dropped: Paused");
        }

        [Test]
        public void NoPause_WhenNotAFocusLossInPlay()
        {
            Assert.IsFalse(SurvivalGame.PausesOnLockDrop(Flow.Playing, false, false, false, false), "not locked last frame: no drop");
            Assert.IsFalse(SurvivalGame.PausesOnLockDrop(Flow.Playing, true, true, false, false), "still locked");
            foreach (Flow f in new[] { Flow.Title, Flow.Paused, Flow.Down, Flow.Results })
                Assert.IsFalse(SurvivalGame.PausesOnLockDrop(f, true, false, false, false), f + ": not Playing");
            Assert.IsFalse(SurvivalGame.PausesOnLockDrop(Flow.Playing, true, false, true, false), "the attract");
            Assert.IsFalse(SurvivalGame.PausesOnLockDrop(Flow.Playing, true, false, false, true), "inside a reel (NIGHT18)");
        }

        [Test]
        public void OnlyTheSmokeUndoesAForeignPause()
        {
            // The smoke's guard is a component it adds itself (SliceSmoke.N55Begin: gameObject.AddComponent<SmokeGuard55>()); the game never
            // adds one, so outside -skygrindSmoke a focus-loss pause stays until the player resumes.
            Assert.IsTrue(typeof(UnityEngine.MonoBehaviour).IsAssignableFrom(typeof(SmokeGuard55)), "SmokeGuard55 is a component the smoke adds");
            Assert.IsTrue(Smoke55.Resolves(Smoke55.PauseCause.LockDropped), "inside the smoke the guard resolves a dropped lock");
            Assert.IsTrue(SurvivalGame.PausesOnLockDrop(Flow.Playing, true, false, false, false), "the rule does not read the smoke");
        }
    }
}
