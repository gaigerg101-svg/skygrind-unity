// NIGHT55 S (P0-smoke; OPEN O-03 / O-52, THREADS T-73 / T-114 / T-133): the slice smoke made trustworthy.
// The evidence (Logs/n55/smoke-a0.out, Logs/player/n55-smoke-a0.log, and NIGHT54's n54-smoke-n54-freeze2b.log): round 1
// passes, then from the first buy every hold-E step and every fire-at-a-placed-husk step fails, the EXFIL line reads the
// flow "Paused", and every virtual-pad step fails with "real pads disabled for a virtual-pad step: 0" where a clean run on
// the same build printed 1 (the attached Wireless Gamepad). The code read for the candidates:
//   1. The Input System's default backgroundBehavior (ResetAndDisableNonBackgroundDevices; the project has no input settings
//      asset) disables every non-background device while the Player window is unfocused: the real pad (DisableRealPads
//      skips a disabled pad: the 0) and the smoke's virtual pads (a device added while unfocused starts disabled), so A,
//      the stick, the d-pad and START never arrive (FeelReel.N33.Shop.cs:72-81 found the same in the reels).
//   2. SurvivalGame.Update pauses a Playing run whose cursor lock drops in the frame (SurvivalGame.cs:295, "Esc frees the
//      cursor"); the reels are exempt (!FeelReel.Running), the smoke is not. A paused run steps no sim: a hold-E buy never
//      completes and a placed husk is never shot, until the next StartRun / Restart sets Playing again (the death steps
//      pass because they call health.Damage directly). The memory note player-smoke-focus-pause (NIGHT17) saw the same.
//   3. A MODS draft (SurvivalGame.cs:302) and a reward reveal (UiReveal20.cs:214) pause from the game's Update too; the
//      smoke's steps handle neither.
//   4. A real pad's START (RoutePad) pauses; its disconnect does not (PadReader: no pause on ConnectedChanged).
// What this file does (one call-site line each in SliceSmoke.cs: Start, Check, Finish):
//   (1) every failing step is followed by a "n55 why:" line: the verdict (what paused the run or dropped its presses), the
//       flow, sim steps / paused / unfocused frames inside the step, focus, the cursor, backgroundBehavior, the device used
//       last, the aim assist, the buy candidate, the pauses resolved and every Input System device with its enabled state;
//       focus changes and device changes are logged as they happen, and a summary prints before DONE;
//   (2) the smoke takes IgnoreFocus for its length (restored at Finish), keeps its own "Smoke ..." pads enabled, and its
//       guard (SmokeGuard55, after every other Update) resolves any pause the smoke did not make (a dropped cursor lock, a
//       real pad's START, a MODS draft: card 0, a reward reveal: skipped), so a focus change can no longer fail a step;
//   (3) Smoke55 is the guard's rule, pure, tested in Assets/Tests/EditMode/Game/Smoke55Tests.cs.
// Harness only: nothing here runs outside -skygrindSmoke / SliceSmoke.Begin.
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    /// <summary>NIGHT55 S: the smoke guard's rule (pure): which pause is the smoke's own, which it resolves, and the verdict of a failing step.</summary>
    public static class Smoke55
    {
        public enum PauseCause { None, Own, LockDropped, RealPadStart, Draft, Reveal }

        /// <summary>
        /// A transition seen after every Update of a frame (SmokeGuard55): Playing at the end of the last frame and Paused now
        /// means this frame's Updates paused the run (the smoke's coroutine resumes after every Update, so its own g.Pause()
        /// is never seen as a transition). A pad's START while one of the smoke's virtual pads is live is the smoke's own (its
        /// pad steps press START to pause); a MODS draft and a reward reveal are named; any other is SurvivalGame.cs:295's
        /// dropped cursor lock (Esc or a focus change).
        /// </summary>
        public static PauseCause Classify(bool playingBefore, bool pausedNow, bool draftOpen, bool revealOpenedThisFrame, bool padStartThisFrame, bool smokePadLive)
        {
            if (!playingBefore || !pausedNow) return PauseCause.None;
            if (padStartThisFrame && smokePadLive) return PauseCause.Own;
            if (draftOpen) return PauseCause.Draft;
            if (revealOpenedThisFrame) return PauseCause.Reveal;
            if (padStartThisFrame) return PauseCause.RealPadStart;
            return PauseCause.LockDropped;
        }

        /// <summary>The guard resolves every pause the smoke did not make.</summary>
        public static bool Resolves(PauseCause c) => c != PauseCause.None && c != PauseCause.Own;

        public static string Name(PauseCause c, bool focused)
        {
            switch (c)
            {
                case PauseCause.Own: return "the smoke's own pad START";
                case PauseCause.LockDropped: return focused ? "the cursor lock dropped with the window focused (Esc, or a focus flicker back within the frame)" : "the cursor lock dropped: the window lost the focus";
                case PauseCause.RealPadStart: return "a real pad's START";
                case PauseCause.Draft: return "a MODS draft opened";
                case PauseCause.Reveal: return "a reward reveal opened";
                default: return "none";
            }
        }

        /// <summary>The failing step's verdict, most direct cause first.</summary>
        public static string Verdict(int foreignPauses, string lastForeign, int pausedFrames, int simSteps, int unfocusedFrames, bool simActive)
        {
            if (foreignPauses > 0) return $"{foreignPauses} pause(s) the smoke did not make, resolved (last: {lastForeign})";
            if (pausedFrames > 0) return $"the run sat in PAUSED {pausedFrames} frame(s) (the smoke's own pause, or one not seen as a transition)";
            if (simSteps == 0) return "no sim step ran in the step";
            if (unfocusedFrames > 0) return $"the window was unfocused {unfocusedFrames} frame(s) of the step (IgnoreFocus on: devices kept)";
            if (!simActive) return "the sim is not active now (not Playing, or DOWN)";
            return "no pause, focus or device cause seen in the step (the step's own logic or timing)";
        }

        /// <summary>A virtual pad the smoke added (SliceSmoke.Night13 "Smoke pad", Night15Pad "Smoke pad only", N22.Grind "Smoke grind pad").</summary>
        public static bool IsSmokePadName(string name) => name != null && name.StartsWith("Smoke");
    }

    public sealed partial class SliceSmoke
    {
        SmokeGuard55 guard55;

        /// <summary>NIGHT55 S: the guard on (IgnoreFocus, the logs, the pause resolution); called once from Start before the first step.</summary>
        void N55Begin(SurvivalGame g)
        {
            guard55 = gameObject.AddComponent<SmokeGuard55>();
            guard55.Begin(g, Line);
        }

        /// <summary>NIGHT55 S: after each Check: a failing step's "n55 why:" line; the step's counters start again.</summary>
        void N55Checked(bool ok)
        {
            if (guard55 == null) return;
            if (!ok) Line(guard55.Why());
            guard55.StepReset();
        }

        /// <summary>NIGHT55 S: the summary and IgnoreFocus restored (Finish, before DONE).</summary>
        void N55End()
        {
            if (guard55 == null) return;
            guard55.End();
            guard55 = null;
        }

        /// <summary>Every Input System device: its name (a pad's product too), on / OFF.</summary>
        public static string N55Devices()
        {
            var sb = new StringBuilder();
            foreach (UnityEngine.InputSystem.InputDevice d in InputSystem.devices)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(d.name);
                if ((d is Gamepad || d is Joystick) && !string.IsNullOrEmpty(d.description.product) && d.description.product != d.name) sb.Append(" '").Append(d.description.product).Append('\'');
                sb.Append(d.enabled ? " on" : " OFF");
            }
            return sb.ToString();
        }
    }
}
