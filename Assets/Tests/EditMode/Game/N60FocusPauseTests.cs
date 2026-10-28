// NIGHT60 builder L (B9 attempt 2, T-138): the window's focus loss pauses a run (SurvivalGame.PausesOnFocusLoss, SurvivalGame.N60L.cs; the
// call beside the lock-drop line in SurvivalGame.cs's Update, only with the view row design.n60.focus60 on). The Player's probe found the lock
// rule blind there: in a standalone Player Cursor.lockState still reads Locked after the window loses the focus (Logs/n60/b9/
// focus-n60-b7-green.txt: "focus LOST (flow Playing, cursor Locked)", pauses 0). Pinned, the rule's truth table:
//   1. Playing, focused last frame, unfocused now, not the attract, not a reel: pause
//   2. no pause outside Playing (Title / Paused / Down / Results), without a focus change (unfocused both frames, focused both frames, the
//      focus coming back), in the attract, or inside a reel (NIGHT18: another window taking the focus must not pause a scenario)
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class N60FocusPauseTests
    {
        [Test]
        public void FocusLostWhilePlaying_Pauses()
        {
            Assert.IsTrue(SurvivalGame.PausesOnFocusLoss(Flow.Playing, true, false, false, false), "Playing, the window lost the focus: Paused");
        }

        [Test]
        public void NoPause_WhenNotAFocusLossInPlay()
        {
            Assert.IsFalse(SurvivalGame.PausesOnFocusLoss(Flow.Playing, false, false, false, false), "unfocused both frames: no loss this frame");
            Assert.IsFalse(SurvivalGame.PausesOnFocusLoss(Flow.Playing, true, true, false, false), "still focused");
            Assert.IsFalse(SurvivalGame.PausesOnFocusLoss(Flow.Playing, false, true, false, false), "the focus coming back");
            foreach (Flow f in new[] { Flow.Title, Flow.Paused, Flow.Down, Flow.Results })
                Assert.IsFalse(SurvivalGame.PausesOnFocusLoss(f, true, false, false, false), f + ": not Playing");
            Assert.IsFalse(SurvivalGame.PausesOnFocusLoss(Flow.Playing, true, false, true, false), "the attract");
            Assert.IsFalse(SurvivalGame.PausesOnFocusLoss(Flow.Playing, true, false, false, true), "inside a reel (NIGHT18)");
        }
    }
}
