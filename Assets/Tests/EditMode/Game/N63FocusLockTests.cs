// NIGHT63 builder V (leftover 3: T-146's Editor item; the NIGHT60 code review's S4, Research/2026-10-21-wave60/review-n60-code.md): an Editor Play
// run whose cursor never locked (the eval-driven checks of the play-check recipe) could be paused by focus60's focus rule when the Editor lost the
// focus. The rule now needs the cursor locked this frame too: SurvivalGame.PausesOnFocusLoss63. In a Player the lock still reads Locked after a
// focus loss (NIGHT60 B9's finding), so a player's alt-tab still pauses; an unlocked Editor run never does; a locked Editor run is already paused
// by the lock rule (PausesOnLockDrop). NIGHT60's PausesOnFocusLoss stays as it was (N60FocusPauseTests pins it). Pinned:
//   1. unlocked: never a pause, whatever the focus did
//   2. locked: exactly NIGHT60's rule (every flow, both focus frames, the attract, a reel)
//   3. the frame (SurvivalGame.cs) calls the locked rule with this frame's lock (input.Locked), not NIGHT60's
// Written test first (RED on its commit: PausesOnFocusLoss63 is a placeholder answering false and SurvivalGame.cs still calls NIGHT60's rule); the
// Editor run is the lead's.
using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N63FocusLockTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        [Test]
        public void AnUnlockedRun_NeverPausesOnAFocusLoss()
        {
            Assert.IsFalse(SurvivalGame.PausesOnFocusLoss63(Flow.Playing, true, false, false, false, false), "Playing, the focus lost, the cursor never locked: no pause");
            Assert.IsTrue(SurvivalGame.PausesOnFocusLoss63(Flow.Playing, true, false, true, false, false), "the same loss with the cursor locked: Paused");
        }

        [Test]
        public void Locked_IsNight60sRuleExactly()
        {
            foreach (Flow f in (Flow[])Enum.GetValues(typeof(Flow)))
                foreach (bool was in new[] { false, true })
                    foreach (bool now in new[] { false, true })
                        foreach (bool attract in new[] { false, true })
                            foreach (bool reel in new[] { false, true })
                            {
                                bool n60 = SurvivalGame.PausesOnFocusLoss(f, was, now, attract, reel);
                                Assert.That(SurvivalGame.PausesOnFocusLoss63(f, was, now, true, attract, reel), Is.EqualTo(n60), $"locked: {f} {was} {now} {attract} {reel}");
                                Assert.That(SurvivalGame.PausesOnFocusLoss63(f, was, now, false, attract, reel), Is.False, $"unlocked: {f} {was} {now} {attract} {reel}");
                            }
        }

        [Test]
        public void TheFrame_CallsTheLockedRuleWithThisFramesLock()
        {
            string src = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "SurvivalGame.cs"));
            Assert.That(src.Contains("PausesOnFocusLoss63(flow, wasFocused60, focused60, input.Locked, attractOn, FeelReel.Running)"), Is.True, "SurvivalGame.cs's focus line passes input.Locked");
            Assert.That(src.Contains("PausesOnFocusLoss(flow, wasFocused60"), Is.False, "NIGHT60's unlocked rule is no longer the frame's");
        }
    }
}
