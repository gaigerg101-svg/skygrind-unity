// NIGHT20 row 6 (builder P; NIGHT20.md U3-34.4 beyond: "a HOLD / TOGGLE row for L3 in CONTROLS ... CONTROLS draws glyphs,
// not words"; Research/2026-10-04-controller.md checklist rows 2 and 12): CONTROLS' GAMEPAD page in the pad's drawn glyphs
// while design.pad.n19 is live (PromptText: the sprite, its words without the atlas): a swap's note's buttons, the page's
// hint (<A> SELECT, THEN PRESS A PAD BUTTON · <B> BACK), the sticks' rows (MOVE, LOOK); off: NIGHT18's words. And L3
// SPRINT's TOGGLE / HOLD as a setting (UnityTuning.PadSprintModeSetting, a select on the GAMEPAD tab, its default GG's
// switch design.pad.n19.l3Sprint): ReaderInput.ApplySettings hands it to PadReader.sprintToggle; CONTROLS' row flips it.
// Strings are built when the page refreshes (never per frame).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class ControlsPad20
    {
        /// <summary>A pad button for CONTROLS' texts: drawn with the v2 rules live (`v2`), else the set's words (NIGHT18).</summary>
        public static string Glyph(PadButtonName b, GlyphSet set, bool v2) => v2 ? PromptText.Button(b, set) : Glyphs.Button(b, set);

        /// <summary>The GAMEPAD page's hint line: the confirm and back buttons drawn (v2), else NIGHT18's line.</summary>
        public static string HintLine(bool v2)
        {
            if (!v2) return UnityTuning.ControlsPadHint18;
            GlyphSet set = PlayerDevices.Glyphs;
            return PromptText.Button(PadButtonName.A, set) + UnityTuning.ControlsPadHintSelect + PromptText.Button(PadButtonName.B, set) + UnityTuning.ControlsPadHintBack;
        }

        /// <summary>The sticks' rows (gamepad.js padRows' first two): MOVE on the left stick, LOOK on the right, drawn.</summary>
        public static List<(string what, string glyph)> StickRows()
        {
            GlyphSet set = PlayerDevices.Glyphs;
            return new List<(string, string)>
            {
                (UnityTuning.ControlsPadMove, PromptText.Glyph(PromptText.LeftStick, set, false)),
                (UnityTuning.ControlsPadLook, PromptText.Glyph(PromptText.RightStick, set, false)),
            };
        }

        /// <summary>The L3 SPRINT row (the GAMEPAD tab of SETTINGS): TOGGLE or HOLD.</summary>
        public static SettingItem SprintModeItem()
        {
            var item = new SettingItem { key = UnityTuning.PadSprintModeSetting, tab = "pad", label = UnityTuning.PadSprintModeLabel, type = "select", desc = UnityTuning.PadSprintModeDesc };
            item.options.Add("toggle");
            item.labels["toggle"] = UnityTuning.PadSprintToggleLabel;
            item.options.Add("hold");
            item.labels["hold"] = UnityTuning.PadSprintHoldLabel;
            return item;
        }

        /// <summary>Its default: GG's L3 SPRINT switch (design.pad.n19.l3Sprint; TOGGLE without it).</summary>
        public static string SprintModeDefault(GameConfig cfg) =>
            cfg.Has("design.pad.n19.l3Sprint") && cfg.Str("design.pad.n19.l3Sprint") == "hold" ? "hold" : "toggle";

        /// <summary>The store holds the row (SETTINGS built with it).</summary>
        public static bool Has(SettingsStore s) => s != null && s.values.ContainsKey(UnityTuning.PadSprintModeSetting);

        /// <summary>The input layer's reading: TOGGLE unless the row says HOLD (`fallback` without the row).</summary>
        public static bool SprintToggle(SettingsStore s, bool fallback) => Has(s) ? s.Str(UnityTuning.PadSprintModeSetting) != "hold" : fallback;

        /// <summary>CONTROLS' cell: TOGGLE or HOLD.</summary>
        public static string SprintModeLabel(SettingsStore s) => SprintToggle(s, true) ? UnityTuning.PadSprintToggleLabel : UnityTuning.PadSprintHoldLabel;

        /// <summary>The other mode (CONTROLS' row flips it).</summary>
        public static string NextSprintMode(SettingsStore s) => SprintToggle(s, true) ? "hold" : "toggle";
    }
}
