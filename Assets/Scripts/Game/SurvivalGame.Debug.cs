// NIGHT13 U3-26.5: SurvivalGame split by concern (audit-perf section 5 move 2): the hot keys and F3's debug line and overlay.
// The same partial type, the statements unchanged and in the same order (SurvivalGame.cs has the rest).
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        void HotKeys()
        {
            FeelAB.Poll(this); // NIGHT18: F6 (a developer build), the LAB row, the corner chip
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            if (ui != null && ui.ControlsCapturing) return; // NIGHT13 U3-26.8: a key pressed to bind it in CONTROLS does nothing else
            if (bindings != null && bindings.Version != hotKeysVersion) // NIGHT13 review: R / V follow a rebind (the readers' rule)
            {
                hotKeysVersion = bindings.Version;
                restartKey = KeyCodes.Bound(bindings, "restart");
                lockOnKey = KeyCodes.Bound(bindings, "lockOn");
            }
            if (DevKeys.Enabled && kb.f3Key.wasPressedThisFrame) // NIGHT13 S13 (D-15): F3 / F8 / F9 only in the Editor or with -skygrindDev
            {
                debugOverlay = !debugOverlay;
                // NIGHT11 U3-24.2: the overlay's OnGUI lives on its own component, enabled only while shown (an OnGUI on
                // this component cost 384 B and 4 allocations every frame through IMGUI, overlay off or not).
                if (overlay == null) overlay = gameObject.AddComponent<DebugOverlay>();
                overlay.enabled = debugOverlay;
            }
            if (DevKeys.Enabled && kb.f8Key.wasPressedThisFrame) zones.OpenAll(); // debug: every gate open (NIGHT7 cut order 3)
            if (DevKeys.Enabled && kb.f9Key.wasPressedThisFrame && flow == Flow.Playing) run.DebugSkipTo(run.round + 1); // debug: the next round now
            if (restartKey != Key.None && kb[restartKey].wasPressedThisFrame) Restart(); // R (bindings.restart)
            // NIGHT9 U3-10 (main.js input:lockOn): V toggles the manual lock while playing (input.bindings.lockOn).
            if (lockOnKey != Key.None && kb[lockOnKey].wasPressedThisFrame && flow == Flow.Playing && assist != null) assist.ToggleLock();
            if (kb.escapeKey.wasPressedThisFrame && ui != null && ui.SettingsOpen) ui.CloseSettings(); // Esc leaves SETTINGS first (menus.js)
            else if (kb.escapeKey.wasPressedThisFrame && ui != null && ui.ControlsOpen) ui.CloseControls(); // NIGHT13 U3-26.8: and CONTROLS (from PAUSED it must not resume)
            else if (kb.escapeKey.wasPressedThisFrame && ui != null && ui.MetaOpen) ui.CloseMeta(); // NIGHT12: Esc leaves CONTRACTS / WEAPONS
            else if (flow == Flow.Paused && !DraftOpen && kb.escapeKey.wasPressedThisFrame && Time.frameCount > pausedFrame) Resume(); // Esc again resumes (not the press that paused); a draft waits for a pick
            if (DraftOpen)
            {
                // Keys 1-3 take a card (the mouse clicks them too).
                if (kb.digit1Key.wasPressedThisFrame) PickMod(0);
                else if (kb.digit2Key.wasPressedThisFrame) PickMod(1);
                else if (kb.digit3Key.wasPressedThisFrame) PickMod(2);
            }
        }

        DebugOverlay overlay;

        /// <summary>F3's line (state, round, move state, speed, HP, points, zone, step).</summary>
        public string DebugLine() => $"{flow} {run?.state} round {run?.round}  {mover.state}{(mover.grounded ? " grounded" : "")}  speed {mover.Speed:0.0} m/s  hp {health.hp:0}  pts {economy.total:0}  zone {zones.ZoneAt(new Vec3(current.x, current.y + 1, current.z))}  step {simStep}";
    }

    /// <summary>F3's debug overlay (IMGUI), a component of its own so no OnGUI runs while it is off (NIGHT11 U3-24.2).</summary>
    public sealed class DebugOverlay : MonoBehaviour
    {
        GUIStyle guiStyle;

        void OnGUI()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g == null || g.cfg == null || g.run == null) return;
            if (guiStyle == null) guiStyle = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            string body = g.DebugLine();
            GUI.color = Color.black;
            GUI.Label(new Rect(11, Screen.height - 31, Screen.width, 30), body, guiStyle);
            GUI.color = Color.white;
            GUI.Label(new Rect(10, Screen.height - 32, Screen.width, 30), body, guiStyle);
        }
    }
}
