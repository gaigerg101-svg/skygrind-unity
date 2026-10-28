// NIGHT13 U3-26.5: SurvivalGame split by concern (audit-perf section 5 move 2): the flow (Hitstop, Respawn, TITLE, StartRun, EndRun, restart, retry, MODS picks, pause).
// NIGHT13 U3-26.6: the run's flow is the sim's (SurvivalSim.Run.cs); these add the Game's parts: the cursor, the camera
// and gun respawned, the views reset at a new run (at the points the sim calls back), OnNewRun.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        public void Hitstop(double duration, string cause) => sim.Hitstop(duration, cause);

        public void Respawn(Vec3 at, double yawDeg)
        {
            sim.Respawn(at, yawDeg);
            ViewRespawn();
        }

        /// <summary>The Game's half of a respawn: no interpolation debt, the camera's smoothing cut, the gun's motion settled.</summary>
        void ViewRespawn()
        {
            accumulator = 0;
            feel.Cut();
            viewmodel?.Respawn(); // NIGHT12: the gun's motion starts settled
        }

        void FreeCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // --- the flow --------------------------------------------------------------------------------------------

        public void ToTitle()
        {
            ImpactToTitle(); // NIGHT16 review finding 5: no freeze or slow-mo carried to TITLE (SurvivalGame.Impact.cs)
            sim.ToTitle();
            ViewRespawn();
            FreeCursor();
        }

        /// <summary>
        /// The title's / pause's AIM ASSIST button: off, or back to the last mode used (SNAP at first). It writes the mode
        /// AssistMode reads: SNAP MODE (PAD) while the pad is the device (NIGHT13 review), else the mouse's.
        /// </summary>
        public void ToggleAssist()
        {
            string mode = AssistMode;
            if (mode != "off") assistBack = mode;
            settings.Set(PlayerDevices.Last == InputDevice.Pad ? "padAimAssist" : "aimAssist", mode == "off" ? assistBack : "off");
        }

        /// <summary>startSurvival (SurvivalSim.StartRun): a fresh world, the start loadout and bank, round 1 after the first break.</summary>
        public void StartRun(uint? seed = null)
        {
            AttractYield(); // NIGHT14 U3-27.7: a run started by anything else stops the attract first (its own runs pass)
            N33ShopBeforeRun(); // NIGHT33 P: a campaign run's wallet and upgrades (design.n33.shop33; none otherwise), before the tape records them (SurvivalGame.Shop33.cs)
            sim.StartRun(seed, ResetViewsForRun, RaiseNewRun);
            LockCursor();
        }

        /// <summary>The views' resets at a new run (they ran inside StartRun; the sim calls back at that point).</summary>
        void ResetViewsForRun()
        {
            ViewRespawn(); Hitch24.Mark("views:respawn");
            playerInput.ResetRun(0); // NIGHT13 U3-26.7 (main.js respawnAt): toggles off, slot 0 with no last weapon, no latched pad press
            decals.Clear(); // effects.js clear: no decals left from the last run
            trauma.Clear(); Hitch24.Mark("views:decalsTrauma");
            lightAndLens.NewRun(); Hitch24.Mark("views:lightAndLens");
        }

        void RaiseNewRun() => OnNewRun?.Invoke();

        void EndRun() => sim.EndRun(); // the sim frees the cursor through runEnded

        /// <summary>
        /// R from DOWN or RESULTS (docs/FRICTION.md #4; Neon White's one-key restart): from DOWN it skips the wait (the run
        /// still ends and is judged against the best), then a new run starts at once, on a new seed.
        /// </summary>
        public bool Restart()
        {
            // NIGHT16 polish #48 (menus.js :786-790): R from PAUSED too, as PAUSED's RESTART button does (RestartRun).
            RestartRule.Kind k = RestartRule.For(flow, DraftOpen, ui != null && (ui.SettingsOpen || ui.ControlsOpen));
            if (k == RestartRule.Kind.None) return false;
            if (k == RestartRule.Kind.FromPause)
            {
                RestartRun();
                return true;
            }
            if (k == RestartRule.Kind.EndAndStart)
            {
                sim.downSteps = 0;
                EndRun();
            }
            StartRun();
            return true;
        }

        /// <summary>NIGHT12 (seeds.js "Same seed, same run"; RESULTS' RETRY SAME SEED): a new run on the last run's seed and rung.</summary>
        public void RetrySameSeed()
        {
            if (lastStats == null || (flow != Flow.Results && flow != Flow.Down)) return;
            if (flow == Flow.Down)
            {
                sim.downSteps = 0;
                EndRun();
            }
            setup.seed = lastStats.seed;
            setup.heat = lastStats.heat;
            StartRun();
        }

        /// <summary>Take card `i` of the open draft; the game resumes when no draft is left open.</summary>
        public bool PickMod(int i)
        {
            if (!DraftOpen || !mods.Pick(i)) return false;
            Resume();
            return true;
        }

        int pausedFrame;

        public void Pause()
        {
            if (flow != Flow.Playing) return;
            flow = Flow.Paused;
            pausedFrame = Time.frameCount;
            rumble?.Stop(); // NIGHT13: rumble never runs on under a menu (SetMotorSpeeds persists)
            FreeCursor();
        }

        public void Resume()
        {
            if (flow != Flow.Paused) return;
            flow = Flow.Playing;
            LockCursor();
        }
    }

    /// <summary>NIGHT16 polish #48: what the restart key does in each flow (SmallUiTests).</summary>
    public static class RestartRule
    {
        public enum Kind { None, EndAndStart, Start, FromPause }

        /// <summary>
        /// DOWN: end the run (skip the wait), then a new one; RESULTS: a new one; PAUSED (menus.js :786-790, the web's
        /// 'pause' screen only): PAUSED's own RESTART (RestartRun), not with the MODS draft or SETTINGS / CONTROLS over it.
        /// </summary>
        public static Kind For(Flow flow, bool draftOpen, bool menuOpen)
        {
            switch (flow)
            {
                case Flow.Down: return Kind.EndAndStart;
                case Flow.Results: return Kind.Start;
                case Flow.Paused: return draftOpen || menuOpen ? Kind.None : Kind.FromPause;
                default: return Kind.None;
            }
        }
    }
}
