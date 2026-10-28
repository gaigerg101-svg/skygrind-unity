// NIGHT55 S (P0-smoke): the slice smoke's guard (SliceSmoke.N55.cs has the why and the rule, Smoke55). It lives on the
// smoke's own GameObject (destroyed with it) and runs after every other Update of a frame, so a Playing -> Paused change it
// sees was made by this frame's Updates (SurvivalGame's dropped-lock rule, RoutePad's START, the MODS draft, the reward
// reveal), never by the smoke's coroutine (that resumes after every Update; LateUpdate records the frame's last flow).
// For the smoke's length: the Input System's backgroundBehavior is IgnoreFocus (the reels' NIGHT33 recipe), the smoke's
// "Smoke ..." pads are enabled whenever found disabled, focus and device changes are logged, and every pause the smoke did
// not make is resolved (Resume; a MODS draft takes card 0). Harness only.
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    [DefaultExecutionOrder(32100)] // after SurvivalGame and Ui (0) and Lineup48Rig (32000): this frame's Updates have run
    public sealed class SmokeGuard55 : MonoBehaviour
    {
        const int MaxEventLines = 40; // focus / device / guard lines printed (the rest only counted): a flickering focus must not flood the log

        SurvivalGame game;
        System.Action<string> say;
        bool began;
        InputSettings.BackgroundBehavior bgWas;
        Flow lastFlow;
        int revealFrame = -1;
        System.Action<object> onReveal;
        int eventLines;

        // this step (since the last Check)
        int stepFrames, stepSteps, stepPaused, stepUnfocused, stepForeign;
        // the whole smoke
        int frames, unfocused, focusChanges, deviceChanges, padsEnabled, ownPauses, resolvedLock, resolvedStart, resolvedDraft, resolvedReveal;
        string lastForeign = "none";
        float lastForeignAt = -1, lastFocusAt = -1, startedAt;

        public void Begin(SurvivalGame g, System.Action<string> line)
        {
            game = g;
            say = line;
            startedAt = Time.realtimeSinceStartup;
            bgWas = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            Application.focusChanged += OnFocus;
            InputSystem.onDeviceChange += OnDevice;
            onReveal = _ => revealFrame = Time.frameCount;
            g.events.On(UnityTuning.Hud20RevealEvent, onReveal);
            lastFlow = g.flow;
            began = true;
            EnableSmokePads();
            say($"n55 input at the start: focused {Application.isFocused}, runInBackground {Application.runInBackground}, backgroundBehavior {bgWas} -> IgnoreFocus for the smoke, cursor {Cursor.lockState}, device last {PlayerDevices.Last}; devices [{SliceSmoke.N55Devices()}]");
        }

        public void StepReset()
        {
            stepFrames = stepSteps = stepPaused = stepUnfocused = stepForeign = 0;
        }

        float Now => Time.realtimeSinceStartup - startedAt;

        public string Why()
        {
            SurvivalSim s = game != null ? game.sim : null;
            bool active = s != null && s.Active;
            string candidate = game != null && game.buyables != null && game.buyables.candidate != null ? game.buyables.candidate.id : "none";
            int resolved = resolvedLock + resolvedStart + resolvedDraft + resolvedReveal;
            return $"n55 why: {Smoke55.Verdict(stepForeign, lastForeign, stepPaused, stepSteps, stepUnfocused, active)}; flow {game?.flow}, sim active {active}; "
                + $"this step {stepFrames} frames, {stepSteps} sim steps, {stepPaused} paused, {stepUnfocused} unfocused; focused {Application.isFocused}, cursor {Cursor.lockState}, "
                + $"backgroundBehavior {InputSystem.settings.backgroundBehavior}, device last {PlayerDevices.Last}, assist {game?.AssistMode}, buy candidate {candidate}; "
                + $"pauses resolved {resolved} (last: {lastForeign}{(lastForeignAt >= 0 ? $" at {lastForeignAt:0.0} s" : "")}), focus changes {focusChanges}{(lastFocusAt >= 0 ? $" (last at {lastFocusAt:0.0} s)" : "")}; "
                + $"devices [{SliceSmoke.N55Devices()}]";
        }

        public void End()
        {
            if (!began) return;
            began = false;
            Application.focusChanged -= OnFocus;
            InputSystem.onDeviceChange -= OnDevice;
            if (game != null && game.events != null) game.events.Off(UnityTuning.Hud20RevealEvent, onReveal);
            InputSystem.settings.backgroundBehavior = bgWas;
            say($"n55 input summary: focus changes {focusChanges}, unfocused {unfocused} of {frames} frames; pauses the smoke did not make, resolved: dropped cursor lock {resolvedLock}, real pad START {resolvedStart}, MODS draft {resolvedDraft}, reward reveal {resolvedReveal} (the smoke's own pad START {ownPauses}); "
                + $"smoke pads enabled by the guard {padsEnabled}; device changes {deviceChanges}; backgroundBehavior restored to {bgWas}; devices [{SliceSmoke.N55Devices()}]");
        }

        void OnDestroy() => End();

        void Event(string s)
        {
            if (++eventLines <= MaxEventLines) say(s);
            else if (eventLines == MaxEventLines + 1) say($"n55: {MaxEventLines} focus / device / guard lines printed; the rest are counted in the summary");
        }

        void OnFocus(bool focused)
        {
            focusChanges++;
            lastFocusAt = Now;
            Event($"n55 focus {(focused ? "back" : "LOST")}: flow {game?.flow}, cursor {Cursor.lockState}");
        }

        void OnDevice(UnityEngine.InputSystem.InputDevice d, InputDeviceChange change)
        {
            bool smokePad = d is Gamepad && Smoke55.IsSmokePadName(d.name);
            if (smokePad)
            {
                if (change == InputDeviceChange.Added && !d.enabled)
                {
                    InputSystem.EnableDevice(d); // a pad added while unfocused starts disabled (NIGHT33)
                    padsEnabled++;
                }
                return; // the steps' own pads: added, removed, their state events
            }
            if (change == InputDeviceChange.UsageChanged || change == InputDeviceChange.ConfigurationChanged || change == InputDeviceChange.SoftReset) return;
            deviceChanges++;
            string product = d.description.product;
            Event($"n55 device {change}: {d.name}{(!string.IsNullOrEmpty(product) && product != d.name ? $" '{product}'" : "")} ({d.layout}), enabled {d.enabled}; flow {game?.flow}, focused {Application.isFocused}");
        }

        void EnableSmokePads()
        {
            var pads = Gamepad.all;
            for (int i = 0; i < pads.Count; i++)
            {
                if (pads[i].enabled || !Smoke55.IsSmokePadName(pads[i].name)) continue;
                InputSystem.EnableDevice(pads[i]);
                padsEnabled++;
            }
        }

        bool SmokePadLive()
        {
            var pads = Gamepad.all;
            for (int i = 0; i < pads.Count; i++)
                if (Smoke55.IsSmokePadName(pads[i].name)) return true;
            return false;
        }

        void Update()
        {
            if (!began || game == null) return;
            frames++;
            stepFrames++;
            stepSteps += game.StepsThisFrame;
            bool focused = Application.isFocused;
            if (!focused)
            {
                unfocused++;
                stepUnfocused++;
            }
            Flow now = game.flow;
            if (now == Flow.Paused) stepPaused++;
            bool padStart = game.padReader != null && game.padReader.commands.pause;
            Smoke55.PauseCause cause = Smoke55.Classify(lastFlow == Flow.Playing, now == Flow.Paused, game.DraftOpen, revealFrame == Time.frameCount, padStart, SmokePadLive());
            if (cause == Smoke55.PauseCause.Own) ownPauses++;
            else if (Smoke55.Resolves(cause)) Resolve(cause, focused);
            EnableSmokePads();
        }

        void Resolve(Smoke55.PauseCause cause, bool focused)
        {
            string what = Smoke55.Name(cause, focused);
            lastForeign = what;
            lastForeignAt = Now;
            stepForeign++;
            if (cause == Smoke55.PauseCause.LockDropped) resolvedLock++;
            else if (cause == Smoke55.PauseCause.RealPadStart) resolvedStart++;
            else if (cause == Smoke55.PauseCause.Draft) resolvedDraft++;
            else if (cause == Smoke55.PauseCause.Reveal) resolvedReveal++;
            if (cause == Smoke55.PauseCause.Draft) game.PickMod(0); // the draft's card 0, then the game resumes (the smoke's steps pick no card)
            else game.Resume(); // a reveal sees the flow leave PAUSED and skips (UiReveal20.RenderReveal20)
            Event($"n55 guard: a pause the smoke did not make ({what}) resolved: flow {game.flow}, focused {focused}, cursor {Cursor.lockState}, round {game.run?.round} {game.run?.state}");
        }

        void LateUpdate()
        {
            if (!began || game == null) return;
            lastFlow = game.flow; // the frame's last flow, after the smoke's coroutine
            EnableSmokePads(); // a pad the coroutine added this frame is on before the next input update reads its events
        }
    }
}
