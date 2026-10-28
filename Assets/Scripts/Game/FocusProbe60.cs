// NIGHT60 builder L (B9, T-138: the focus pause outside the smoke). The rule SurvivalGame.PausesOnLockDrop (SurvivalGame.N56Lead.cs)
// pauses a Playing run whose cursor lock drops (the window losing the focus, or Esc) outside the attract and the reels; only the EditMode
// N56FocusPauseTests pinned it, because the smoke's SmokeGuard55 resolves every pause it did not make and no flag started a run outside the
// smoke or a reel. This probe is that run, in a Player:
//   SKYGRIND.exe -skygrindFocusProbe60 [-skygrindFocusProbe60NoPause] -logFile <f>
// It starts a run as SliceSmoke.Start does (the arena, no records, no LAB, the profile read-only, Survival at heat 0, seed 424242, god
// mode), waits until the run is Playing with the cursor locked and the window focused ("[focus60] ready ..."), prints every flow, cursor
// lock and focus change with its time, and after UnityTuning.FocusProbe60Seconds prints "[focus60] DONE: paused on the focus loss
// True|False" and quits. Tools/night/n60-focus-probe.sh plants one focus steal after the ready line (n56-foreground.ps1, L-099).
// -skygrindFocusProbe60NoPause is the planted RED (L-014): the probe sets a pause it sees straight back to Playing before it looks, so its
// run rides through the focus loss and the line must read False. The rule itself is never changed or read here; the input settings
// (backgroundBehavior) stay the game's. Added only with the flag (PlayerChecks.Attach), like the smoke's components.
using System;
using System.Collections;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT60 L B9: the focus probe's verdict (pure), one observation per frame after the frame's Updates.</summary>
    public sealed class FocusProbe60Rule
    {
        Flow lastFlow;
        bool lastLocked, lastFocused;

        /// <summary>Playing with the cursor locked and the window focused, seen once: only changes after it count.</summary>
        public bool Ready { get; private set; }
        public int FocusLosses { get; private set; }
        public int LockDrops { get; private set; }
        /// <summary>Playing -> Paused changes after ready.</summary>
        public int Pauses { get; private set; }
        /// <summary>Of them, the ones in a frame with the window unfocused (Esc, a MODS draft or a reward reveal pause a focused window).</summary>
        public int PausesUnfocused { get; private set; }
        public bool PausedOnFocusLoss => PausesUnfocused > 0;

        public void Observe(Flow flow, bool locked, bool focused)
        {
            if (Ready)
            {
                if (lastFocused && !focused) FocusLosses++;
                if (lastLocked && !locked) LockDrops++;
                if (lastFlow == Flow.Playing && flow == Flow.Paused)
                {
                    Pauses++;
                    if (!focused) PausesUnfocused++;
                }
            }
            else if (flow == Flow.Playing && locked && focused) Ready = true;
            lastFlow = flow;
            lastLocked = locked;
            lastFocused = focused;
        }

        public static string DoneLine(bool paused) => "[focus60] DONE: paused on the focus loss " + (paused ? "True" : "False");
    }

    /// <summary>NIGHT60 L B9: the Player's focus-pause probe (-skygrindFocusProbe60); after SurvivalGame's Update, as SmokeGuard55.</summary>
    [DefaultExecutionOrder(32100)]
    public sealed class FocusProbe60 : MonoBehaviour
    {
        public const string Flag = "-skygrindFocusProbe60", NoPauseFlag = "-skygrindFocusProbe60NoPause";

        readonly FocusProbe60Rule rule = new FocusProbe60Rule();
        SurvivalGame game;
        bool noPause, watching, done;
        float t0, readyAt = -1;
        Flow lastFlow;
        bool lastLocked, lastFocused;
        int lines, unprinted, resolved;

        public static bool Requested(string[] args) => Array.IndexOf(args, Flag) >= 0 || Array.IndexOf(args, NoPauseFlag) >= 0;

        public static void Attach(SurvivalGame g, string[] args)
        {
            var p = g.gameObject.AddComponent<FocusProbe60>();
            p.noPause = Array.IndexOf(args, NoPauseFlag) >= 0;
        }

        float Now => Time.realtimeSinceStartup - t0;

        static bool CursorLocked => Cursor.lockState == CursorLockMode.Locked;

        void Say(string s) => Debug.Log("[focus60] " + s);

        void Event(string s)
        {
            if (++lines <= UnityTuning.FocusProbe60MaxLines) Say(s);
            else unprinted++;
        }

        IEnumerator Start()
        {
            yield return null;
            t0 = Time.realtimeSinceStartup;
            game = SurvivalGame.Instance;
            if (game == null)
            {
                Say("no SurvivalGame");
                Finish();
                yield break;
            }
            Say($"start{(noPause ? " (PLANTED -skygrindFocusProbe60NoPause: a pause is set back to Playing before the probe looks)" : "")}: focus60 row {(game.FocusPause60On ? "on" : "off")} (design.n60.focus60, -skygrindOff60, -skygrindNoFocus60), map {game.MapId}, focused {Application.isFocused}, runInBackground {Application.runInBackground}, cursor {Cursor.lockState}, flow {game.flow}");
            if (game.MapId != RunSetup.DefaultMap) Say($"the map is '{game.MapId}', not the smoke's arena '{RunSetup.DefaultMap}'");
            // SliceSmoke.Start's run: no personal bests, the web's rules whatever the LAB tab says, the profile never written, Survival at
            // heat 0 on the smoke's seed, god mode (an idle player in round 1 must not go DOWN: only a Playing run can pause)
            game.noRecords = true;
            game.noLab = true;
            game.profileReadOnly = true;
            game.setup.mode = RunSetup.Survival;
            game.setup.heat = 0;
            game.StartRun(UnityTuning.FocusProbe60Seed);
            game.health.max = game.health.hp = UnityTuning.FocusProbe60GodHealth;
            lastFlow = game.flow;
            lastLocked = CursorLocked;
            lastFocused = Application.isFocused;
            watching = true;
            Say($"{Now:0.00} s run started: flow {game.flow}, cursor {Cursor.lockState}, focused {Application.isFocused}");
        }

        void Update()
        {
            if (!watching || done || game == null) return;
            if (noPause && rule.Ready && lastFlow == Flow.Playing && game.flow == Flow.Paused)
            {
                game.flow = Flow.Playing; // the planted RED: this run rides through the pause (the cursor stays as the pause left it)
                resolved++;
            }
            Flow flow = game.flow;
            bool locked = CursorLocked, focused = Application.isFocused;
            if (flow != lastFlow) Event($"{Now:0.00} s flow {lastFlow} -> {flow} (cursor {Cursor.lockState}, focused {focused})");
            if (locked != lastLocked) Event($"{Now:0.00} s cursor {(lastLocked ? "Locked" : "not locked")} -> {Cursor.lockState} (flow {flow}, focused {focused})");
            if (focused != lastFocused) Event($"{Now:0.00} s focus {(focused ? "back" : "LOST")} (flow {flow}, cursor {Cursor.lockState})");
            bool wasReady = rule.Ready;
            rule.Observe(flow, locked, focused);
            if (!wasReady && rule.Ready)
            {
                readyAt = Now;
                Say($"ready: flow Playing, cursor Locked, focused at {readyAt:0.00} s (round {game.run?.round} {game.run?.state})");
            }
            lastFlow = flow;
            lastLocked = locked;
            lastFocused = focused;
            if (!rule.Ready && Now >= UnityTuning.FocusProbe60ReadySeconds)
            {
                Say($"not ready in {UnityTuning.FocusProbe60ReadySeconds:0} s: flow {flow}, cursor {Cursor.lockState}, focused {focused}");
                Finish();
            }
            else if (rule.Ready && Now - readyAt >= UnityTuning.FocusProbe60Seconds) Finish();
        }

        void Finish()
        {
            if (done) return;
            done = true;
            Say($"summary: ready {(rule.Ready ? $"at {readyAt:0.00} s" : "never")}, focus losses {rule.FocusLosses}, cursor-lock drops {rule.LockDrops}, pauses {rule.Pauses} ({rule.PausesUnfocused} with the window unfocused){(noPause ? $", set back to Playing by the planted probe {resolved}" : "")}; flow now {game?.flow}, cursor {Cursor.lockState}, focused {Application.isFocused}{(unprinted > 0 ? $"; {unprinted} change lines not printed" : "")}");
            Debug.Log(FocusProbe60Rule.DoneLine(rule.PausedOnFocusLoss));
            Application.Quit();
        }
    }
}
