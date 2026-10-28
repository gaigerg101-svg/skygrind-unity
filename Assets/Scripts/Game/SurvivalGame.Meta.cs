// NIGHT13 U3-26.5: SurvivalGame split by concern (audit-perf section 5 move 2): the profile, tips and x-ray hint glue (SaveProfile, SettingOr, Tip, TipForCandidate, XrayHint, KeyLabel).
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
        /// <summary>Write the profile now (a run's end, a choice on TITLE). A failed write is logged, never thrown.</summary>
        public void SaveProfile()
        {
            if (profile == null || profileReadOnly) return;
            if (!profile.Save(profilePath)) Debug.LogWarning($"[profile] could not write {profilePath}");
        }

        /// <summary>A boolean setting, or `fallback` when the key isn't a setting (read at a run's start, never per frame).</summary>
        bool SettingOr(string key, bool fallback)
        {
            try
            {
                return settings.Bool(key);
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        /// <summary>main.js frame (NIGHT6 B6): the buy prompt's kind has a tip (the sim asks when the prompt changes).</summary>
        void TipForCandidate() => sim.TipForCandidate();

        int xraySettingsVersion = -1;
        bool xrayShown, xrayKnown;

        /// <summary>main.js input:xray: X flips the ENEMY X-RAY setting in play (EnemyLook) and the hint says so.</summary>
        void XrayHint()
        {
            if (settings.version == xraySettingsVersion) return;
            xraySettingsVersion = settings.version;
            bool on = SettingOr("xray", true);
            if (on == xrayShown && xrayKnown) return;
            if (xrayKnown && flow == Flow.Playing) ui?.ShowHint(on ? "ENEMY X-RAY ON" : "ENEMY X-RAY OFF");
            xrayShown = on;
            xrayKnown = true;
        }

        /// <summary>
        /// An input action's key label for the tips' {action} placeholders: bindings.js keyName through Ui.KeyLabel (NIGHT13
        /// U3-26.5: one binding label; this used its own "KeyX -> X" copy, which printed ShiftLeft as SHIFTLEFT); the action's
        /// name when it has no binding, as before.
        /// </summary>
        string KeyLabel(string action) => PromptText.For(action, false, (bindings != null && padReader != null ? Glyphs.Label(action, bindings, padReader.layout) : null) ?? Ui.KeyLabel(cfg, action) ?? action.ToUpperInvariant()); // NIGHT19: PromptText (builder P) // NIGHT13 U3-26.7: the device's label (main.js keyLabel)
    }
}
