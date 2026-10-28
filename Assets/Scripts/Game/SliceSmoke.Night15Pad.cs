// NIGHT15 smoke step (U3-28.8, builder K): THE PAD ONLY. The lead calls PadOnly(g) from the night's step list.
// A virtual pad (InputSystem.AddDevice<Gamepad>, its state sent by InputSystem.QueueStateEvent), never the mouse, the
// keyboard or a forced input: TITLE -> A on PLAY starts a run -> Y held opens the weapon wheel (design.pad.wheelHold),
// the right stick points at the other gun, the release takes it (the slot in hand changed, no NEXT WEAPON on top) and the
// view held still while it was open -> a tap of Y is NEXT WEAPON -> START pauses -> the d-pad reaches SETTINGS, A opens
// it, B closes it back to PAUSED -> START resumes the run; the pad removed, back to TITLE. Every wait is in real time (a
// Player runs ~400 fps: frames are never counted). Lines start "pad only:".
// PadOnlyLinger (s, default 0): the wheel stays open and pointed that long after its checks, for the lead's capture.
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Skygrind.Game
{
    public sealed partial class SliceSmoke
    {
        /// <summary>Seconds the pad's wheel stays open (pointed) after its checks, for a capture (0: none).</summary>
        public static float PadOnlyLinger;

        /// <summary>NIGHT15 (lead): a PNG path for one capture of the open wheel during the linger (null: none).</summary>
        public static string PadOnlyShot;

        // Each press is held, then let go, this long (s): under gamepad.menuDelay (0.35 s, one menu move and no repeat) and
        // under design.pad.wheelHold (a tap of Y stays NEXT WEAPON).
        const float PadOnlyPressHold = 0.06f;

        /// <summary>Real time, and at least two frames (a queued state event is read on the next frame's poll).</summary>
        IEnumerator PadOnlyWait(float seconds)
        {
            float t0 = Time.realtimeSinceStartup;
            int frames = 0;
            while (frames < 2 || Time.realtimeSinceStartup - t0 < seconds)
            {
                frames++;
                yield return null;
            }
        }

        static string PadOnlySelected()
        {
            GameObject sel = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            return sel != null ? sel.name : "none";
        }

        IEnumerator PadOnly(SurvivalGame g)
        {
            DisableRealPads(); // NIGHT24: see SliceSmoke.DisableRealPads
            Gamepad pad = InputSystem.AddDevice<Gamepad>("Smoke pad only");
            var state = new GamepadState();
            void Send() => InputSystem.QueueStateEvent(pad, state);
            IEnumerator Press(GamepadButton b)
            {
                state = state.WithButton(b, true);
                Send();
                yield return PadOnlyWait(PadOnlyPressHold);
                state = state.WithButton(b, false);
                Send();
                yield return PadOnlyWait(PadOnlyPressHold);
            }

            // TITLE -> A on PLAY.
            g.ToTitle();
            ClearMenuSelection(); // NIGHT24
            yield return PadOnlyWait(0.3f); // TITLE settles; the nav selects PLAY
            string onTitle = PadOnlySelected();
            yield return Press(GamepadButton.South);
            float t0 = Time.realtimeSinceStartup;
            while (g.flow != Flow.Playing && Time.realtimeSinceStartup - t0 < 3) yield return null;
            bool played = g.flow == Flow.Playing;
            Check(played, $"pad only: TITLE ('{onTitle}' selected), A started a run: {g.flow}");
            if (!played)
            {
                yield return PadOnlyEnd(g, pad);
                yield break;
            }
            g.health.max = g.health.hp = 1e6; // the smoke's god mode (not an input)

            // The wheel: hold Y until it opens, point the right stick at the next gun, release Y.
            int n = g.loadout.owned.Count;
            int from = g.loadout.targetSlot;
            int want = n > 0 ? (from + 1) % n : 0;
            GamepadButton cycle = g.padReader.NextWeaponInputButton; // NIGHT20 (builder P): the layout's NEXT WEAPON (v3: RB; v2: Y)
            state = state.WithButton(cycle, true);
            Send();
            t0 = Time.realtimeSinceStartup;
            bool opened = false;
            while (Time.realtimeSinceStartup - t0 < 3)
            {
                yield return null;
                if (g.combatReader.radial.open && g.combatReader.radial.pad)
                {
                    opened = true;
                    break;
                }
            }
            float heldFor = Time.realtimeSinceStartup - t0;
            double yaw0 = g.input.yaw, pitch0 = g.input.pitch;
            double a = (double)want / System.Math.Max(1, n) * System.Math.PI * 2; // slot i at i / n of a turn, 0 up, clockwise
            state.rightStick = new Vector2((float)System.Math.Sin(a), (float)System.Math.Cos(a)); // Unity's stick y is + up
            Send();
            yield return PadOnlyWait(0.25f);
            bool shown = g.UiView != null && g.UiView.RadialShown;
            int hi = g.combatReader.radial.highlighted, hudHi = g.UiView != null ? g.UiView.RadialHighlight : -1;
            bool still = g.input.yaw == yaw0 && g.input.pitch == pitch0;
            string when = $"{g.run.state} of round {g.run.round}";
            if (PadOnlyLinger > 0)
            {
                Line($"pad only: the wheel held open {PadOnlyLinger:0} s for a capture");
                float l0 = Time.realtimeSinceStartup;
                bool shot = false;
                while (Time.realtimeSinceStartup - l0 < PadOnlyLinger)
                {
                    g.health.hp = g.health.max;
                    // NIGHT15 (lead): -skygrindPadShot <png>: one capture of the open wheel, half a second in (the HUD drawn).
                    if (!shot && !string.IsNullOrEmpty(PadOnlyShot) && Time.realtimeSinceStartup - l0 > 0.5f)
                    {
                        ScreenCapture.CaptureScreenshot(PadOnlyShot);
                        Line($"pad only: captured {PadOnlyShot}");
                        shot = true;
                    }
                    yield return null;
                }
            }
            state = state.WithButton(cycle, false); // the stick still held: the release takes the pick
            Send();
            yield return PadOnlyWait(0.05f);
            state.rightStick = Vector2.zero;
            Send();
            t0 = Time.realtimeSinceStartup;
            while (g.loadout.targetSlot != want && Time.realtimeSinceStartup - t0 < 3) yield return null;
            yield return PadOnlyWait(0.1f); // a NEXT WEAPON on top of the pick would show by now
            bool took = g.loadout.targetSlot == want && !g.combatReader.radial.open;
            Check(n >= 2 && opened && shown && hi == want && hudHi == want && still && took,
                $"pad only: {n} guns; Y held {heldFor:0.00} s opened the wheel {opened} (on the HUD {shown}); the right stick pointed at slot {hi} (want {want}, the HUD {hudHi}); the view held still {still} ({when}); the release took it: slot {from} -> {g.loadout.targetSlot}, wheel shut {!g.combatReader.radial.open}");

            // A tap of Y: NEXT WEAPON.
            int before = g.loadout.targetSlot, next = n > 0 ? (before + 1) % n : 0;
            yield return Press(cycle);
            t0 = Time.realtimeSinceStartup;
            while (g.loadout.targetSlot != next && Time.realtimeSinceStartup - t0 < 2) yield return null;
            Check(n >= 2 && g.loadout.targetSlot == next && !g.combatReader.radial.open, $"pad only: a tap of Y is NEXT WEAPON: slot {before} -> {g.loadout.targetSlot} (want {next}), the wheel shut {!g.combatReader.radial.open}");

            // START pauses; the d-pad reaches SETTINGS, A opens it, B closes it back to PAUSED.
            yield return Press(GamepadButton.Start);
            t0 = Time.realtimeSinceStartup;
            while (g.flow != Flow.Paused && Time.realtimeSinceStartup - t0 < 2) yield return null;
            bool paused = g.flow == Flow.Paused;
            yield return PadOnlyWait(0.2f); // PAUSED settles; the nav selects RESUME
            string settingsSnap = g.settings.store.Save();
            for (int k = 0; k < 8 && paused && PadOnlySelected() != "SETTINGS"; k++) yield return Press(GamepadButton.DpadDown);
            string at = PadOnlySelected();
            bool opensSettings = false, closedToPause = false;
            if (at == "SETTINGS") // never A on another button (RESTART, SAVE & QUIT, QUIT TO TITLE)
            {
                yield return Press(GamepadButton.South);
                t0 = Time.realtimeSinceStartup;
                while (!(g.UiView != null && g.UiView.SettingsOpen) && Time.realtimeSinceStartup - t0 < 2) yield return null;
                opensSettings = g.UiView != null && g.UiView.SettingsOpen;
                yield return PadOnlyWait(0.2f);
                yield return Press(GamepadButton.East);
                t0 = Time.realtimeSinceStartup;
                while (g.UiView != null && g.UiView.SettingsOpen && Time.realtimeSinceStartup - t0 < 2) yield return null;
                yield return PadOnlyWait(0.1f);
                closedToPause = g.UiView != null && !g.UiView.SettingsOpen && g.flow == Flow.Paused;
            }
            Check(paused && opensSettings && closedToPause, $"pad only: START paused {paused}; the d-pad reached '{at}'; A opened SETTINGS {opensSettings}; B closed it back to PAUSED {closedToPause} ({g.flow})");
            if (g.settings.store.Save() != settingsSnap)
            {
                g.settings.store.Load(settingsSnap); // the player's settings as they were
                g.settings.Save();
                g.settings.Apply();
            }

            // START resumes the run.
            int slotBefore = g.loadout.targetSlot;
            if (g.flow == Flow.Paused) yield return Press(GamepadButton.Start);
            t0 = Time.realtimeSinceStartup;
            while (g.flow != Flow.Playing && Time.realtimeSinceStartup - t0 < 2) yield return null;
            Check(g.flow == Flow.Playing && g.loadout.targetSlot == slotBefore,
                $"pad only: START resumed the run: {g.flow}, slot {g.loadout.targetSlot}; the device used last {PlayerDevices.Last} (the smoke sent no mouse or keyboard input)");
            yield return PadOnlyEnd(g, pad);
        }

        /// <summary>Every button and stick let go, the virtual pad removed, back to TITLE.</summary>
        IEnumerator PadOnlyEnd(SurvivalGame g, Gamepad pad)
        {
            InputSystem.QueueStateEvent(pad, new GamepadState());
            yield return PadOnlyWait(0.05f);
            InputSystem.RemoveDevice(pad);
            RestoreRealPads(); // NIGHT24
            g.ToTitle();
            yield return null;
        }
    }
}
