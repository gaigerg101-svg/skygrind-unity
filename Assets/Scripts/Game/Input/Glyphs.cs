// NIGHT13 U3-26.7 (agent I): the prompts' key and button texts for the device used last (main.js keyLabel :1430-1440): on
// the pad, the left stick for the movement actions (PAD_MOVE: forward reads LEFT STICK, back / left / right nothing) or the
// first button that carries the action, in the pad's glyph set; otherwise (or an action no pad button carries: radial,
// restart) the live key's name. The sets: the web's neutral letters (gamepad.js PAD_GLYPHS, Core), Xbox, and PlayStation
// in words (the TMP atlases lack the shapes): UnityTuning.Input. Also CONTROLS' pad rows (padRows, main.js padBinds.rows)
// in the set. Nothing allocates per call except the row lists (screens, not frames).
// NIGHT15 U3-28.8 (builder K): a Nintendo set (A / B and X / Y at swapped positions: UnityTuning.PadWheel.cs), chosen by
// the device's names (ForDevice); Neutral stays the fallback.
using System.Collections.Generic;
using System.Text;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Glyphs
    {
        /// <summary>main.js PAD_MOVE (:1432): the move actions' pad text.</summary>
        static string PadMove(string action) =>
            action == "forward" ? "LEFT STICK" : action == "back" || action == "left" || action == "right" ? "" : null;

        static string[] Set(GlyphSet set) =>
            set == GlyphSet.Xbox ? UnityTuning.XboxGlyphs
            : set == GlyphSet.PlayStation ? UnityTuning.PlayStationGlyphs
            : set == GlyphSet.Nintendo ? UnityTuning.NintendoGlyphs
            : GamepadMath.Glyphs;

        /// <summary>A pad button's glyph in a set.</summary>
        public static string Button(PadButtonName button, GlyphSet set) => Set(set)[(int)button];

        /// <summary>
        /// NIGHT15 U3-28.8: the glyph set for a pad: its Input System type first (XInputController: Xbox; DualShockGamepad:
        /// PlayStation), then a Nintendo pad by its names (layout, display name, product, maker: UnityTuning.PadNintendoNames,
        /// any case); else the web's neutral set. PadReader calls it once per device (no allocation either way).
        /// </summary>
        public static GlyphSet ForDevice(bool xinput, bool dualShock, string layout, string displayName, string product, string manufacturer)
        {
            if (xinput) return GlyphSet.Xbox;
            if (dualShock) return GlyphSet.PlayStation;
            return NintendoNamed(layout) || NintendoNamed(displayName) || NintendoNamed(product) || NintendoNamed(manufacturer) ? GlyphSet.Nintendo : GlyphSet.Neutral;
        }

        static bool NintendoNamed(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            foreach (string n in UnityTuning.PadNintendoNames)
            {
                if (name.IndexOf(n, System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        /// <summary>
        /// keyLabel(action): the prompt text for an action (a config.input.bindings name, or a pad action's name) on `device`;
        /// null when neither a pad button nor a key carries it (the web's null).
        /// </summary>
        public static string Label(string action, InputDevice device, GlyphSet set, Bindings bindings, PadLayout layout)
        {
            if (device == InputDevice.Pad)
            {
                string move = PadMove(action);
                if (move != null) return move;
                int a = GamepadMath.ActionFromName(action);
                if (a >= 0)
                {
                    PadButtonName? b = GamepadMath.ButtonFor((PadAction)a, layout);
                    if (b.HasValue) return Button(b.Value, set);
                }
            }
            string code = bindings?.Code(action);
            return code != null ? KeyCodes.Label(code) : null;
        }

        /// <summary>Label for the device and glyph set the devices published this frame (PlayerDevices).</summary>
        public static string Label(string action, Bindings bindings, PadLayout layout) => Label(action, PlayerDevices.Last, PlayerDevices.Glyphs, bindings, layout);

        /// <summary>padRows(layout) with the set's glyphs: the sticks, then every button of the layout as [glyph, what].</summary>
        public static List<(string glyph, string what)> PadRows(PadLayout layout, GlyphSet set)
        {
            layout ??= GamepadMath.DefaultLayout;
            var rows = new List<(string, string)> { ("LEFT STICK", "MOVE"), ("RIGHT STICK", "LOOK") };
            for (int i = 0; i < layout.Count; i++)
            {
                PadButtonName b = layout[i];
                var what = new StringBuilder();
                IReadOnlyList<PadAction> acts = layout.ActionsOf(b);
                for (int j = 0; j < acts.Count; j++)
                {
                    if (j > 0) what.Append(" · ");
                    what.Append(GamepadMath.ActionLabels[(int)acts[j]]);
                }
                rows.Add((Button(b, set), what.ToString()));
            }
            return rows;
        }

        /// <summary>
        /// NIGHT19 U3-33.3 / .4: CONTROLS' cell for a pad action: v1 (design.pad.n19 off), the first button's words or '-' (the
        /// web's); v2, the button drawn as its glyph (PromptText: the words without the sprites) or UNBOUND.
        /// </summary>
        public static string PadBindGlyph(PadAction action, PadLayout layout, GlyphSet set, bool v2)
        {
            PadButtonName? b = GamepadMath.ButtonFor(action, layout ?? GamepadMath.DefaultFor(v2));
            if (!b.HasValue) return v2 ? UnityTuning.PadUnbound : "-";
            return v2 ? PromptText.Button(b.Value, set) : Button(b.Value, set);
        }

        /// <summary>
        /// main.js padBinds.rows (:240): every pad action in PAD_ACTION_LABELS' order as (action, label, glyph of its first
        /// button, '-' when none): CONTROLS' GAMEPAD table. Rebind with GamepadMath.RebindPad, save with InputPrefs.SavePadLayout.
        /// </summary>
        public static List<(PadAction action, string label, string glyph)> PadBindRows(PadLayout layout, GlyphSet set)
        {
            layout ??= GamepadMath.DefaultLayout;
            var rows = new List<(PadAction, string, string)>(GamepadMath.LabelOrder.Length);
            foreach (PadAction a in GamepadMath.LabelOrder)
            {
                PadButtonName? b = GamepadMath.ButtonFor(a, layout);
                rows.Add((a, GamepadMath.ActionLabels[(int)a], b.HasValue ? Button(b.Value, set) : "-"));
            }
            return rows;
        }
    }
}
