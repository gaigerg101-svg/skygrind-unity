// NIGHT60 builder L (B9 attempt 2, T-138; view row design.n60.focus60, OFF in the repo): the window's focus loss pauses a run. The Player
// probe (-skygrindFocusProbe60, FocusProbe60.cs) found the lock-drop rule (PausesOnLockDrop, SurvivalGame.N56Lead.cs) blind in a standalone
// Player: Cursor.lockState still reads Locked after the window loses the focus (Logs/n60/b9/focus-n60-b7-green.txt: "focus LOST (flow Playing,
// cursor Locked)", pauses 0), so alt-tab never paused; only the Editor drops the lock. This rule reads the focus itself (Application.isFocused,
// last frame against this one) beside the lock line in SurvivalGame.cs's Update, only with the row on (read once, at the first frame after the
// boot, when -skygrindOffNN is known). Not paused: the attract and the reels (as the lock rule), and -skygrindBench (an automation run that
// measures a live fight for its length and has no guard: a pause would change what it measures). The smoke needs no exception: its
// SmokeGuard55 resolves every pause it did not make (Smoke55.Classify: a Playing -> Paused change with no own START, draft or reveal is
// LockDropped, resolved by Resume). Off: the row off, -skygrindOff60 (or older), or -skygrindNoFocus60 (this rule alone).
// NIGHT61 builder D (B1, T-146; the NIGHT60 code review's S3): every other run PlayerChecks drives is not paused either (MayPauseOnFocusLoss):
// the smoke, the tours -skygrindShots / -skygrindReel / -skygrindTile / -skygrindStrip / -skygrindHitch, the NIGHT29 checks, golden, the
// fingerprint, the farm and the replay (FeelReel.Running shields only the reel's own scenarios). The focus probe's own run still pauses: it
// measures this pause. So a tour's frames and a check's timings never hold a pause that another window taking the focus caused.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        /// <summary>True when this frame's focus loss pauses the game: Playing, focused last frame, unfocused now, not the attract, not
        /// inside a reel (NIGHT18: another window taking the focus must not pause a scenario).</summary>
        public static bool PausesOnFocusLoss(Flow flow, bool wasFocused, bool focused, bool attractOn, bool reelRunning)
            => flow == Flow.Playing && wasFocused && !focused && !attractOn && !reelRunning;

        /// <summary>NIGHT63 V (leftover 3, T-146; the w60 code review's S4): NIGHT60's rule and the cursor locked this frame (the frame passes
        /// input.Locked). An Editor Play run whose cursor never locked (the eval-driven checks) is never paused by a focus loss; in a Player the lock
        /// still reads Locked after the focus is lost (NIGHT60 B9's probe), so a player's alt-tab still pauses; a locked Editor run is paused by the
        /// lock rule already (PausesOnLockDrop: the Editor drops the lock with the focus). N63FocusLockTests.</summary>
        public static bool PausesOnFocusLoss63(Flow flow, bool wasFocused, bool focused, bool locked, bool attractOn, bool reelRunning)
            => locked && PausesOnFocusLoss(flow, wasFocused, focused, attractOn, reelRunning);

        /// <summary>NIGHT61 builder D (B1, T-146; the NIGHT60 code review's S3): may a run started with these arguments pause on a focus loss, as
        /// far as automation goes (FocusAutomation61Tests)? False for every mode PlayerChecks.Attach starts except the focus probe's: those runs
        /// play live with no player at the window, and a pause would put the PAUSED screen in a tour's frames or a paused stretch in a check's
        /// timings. True for a player's game (no flag, or only switches and options that start no mode: -skygrindOffNN, -skygrindNo..., -skygrindMap
        /// alone, -skygrindDev) and for the focus probe's own run, planted or not (FocusProbe60 measures this very pause). Attach's order: the
        /// NIGHT29 checks start before the probe, the probe before every other mode (PlayerChecks.FocusAutomation61Modes). The row switches
        /// (-skygrindOff60 or older, -skygrindNoFocus60) stay FocusPause60On's own. A Web build's ?skygrindSmoke / ?skygrindBench are not read
        /// (no Web build is made; the NIGHT60 rule did not read them either).</summary>
        public static bool MayPauseOnFocusLoss(string[] args)
        {
            if (args == null) return true;
            if (Array.IndexOf(args, "-skygrindDoorFree29") >= 0 || Array.IndexOf(args, "-skygrindLevelSelect29") >= 0) return false; // DoorFree29Check / LevelSelect29Check.Requested: Attach's first two modes
            if (FocusProbe60.Requested(args)) return true; // the probe's own run, Attach's third mode (-skygrindFocusProbe60, or its planted -skygrindFocusProbe60NoPause)
            foreach (string mode in PlayerChecks.FocusAutomation61Modes)
                if (Array.IndexOf(args, mode) >= 0) return false;
            return true;
        }

        int focusPause60 = -1;
        bool focusWas60 = true;

        /// <summary>The focus60 rule on for this process: the row on, not -skygrindOff60 (or older), not -skygrindNoFocus60, not -skygrindBench,
        /// and (NIGHT61 B1) not a run PlayerChecks drives, the focus probe's excepted (MayPauseOnFocusLoss).
        /// Read once (the first call, after the boot set NightRows.ForceOffFrom); FocusProbe60's start line prints it.</summary>
        public bool FocusPause60On
        {
            get
            {
                if (focusPause60 < 0)
                {
                    string[] args;
                    try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                    catch (Exception) { args = new string[0]; }
                    bool off = NightRows.OffFrom(args, "-skygrindOff") <= 60 || NightRows.HasSwitch(args, "-skygrindNoFocus60") || Array.IndexOf(args, "-skygrindBench") >= 0 // NIGHT63 R1 N5: the row's own switch through the one rule
                        || !MayPauseOnFocusLoss(args); // NIGHT61 D B1: every other automation run too (the -skygrindBench test stays as NIGHT60 wrote it)
                    focusPause60 = !off && NightRows.ViewOn(cfg, "focus60") ? 1 : 0;
                }
                return focusPause60 == 1;
            }
        }

        /// <summary>The frame's start (beside wasLocked): last frame's focus, and this frame's kept for the next.</summary>
        bool FocusFrame60(out bool focused)
        {
            bool was = focusWas60;
            focused = Application.isFocused;
            focusWas60 = focused;
            return was;
        }
    }
}
