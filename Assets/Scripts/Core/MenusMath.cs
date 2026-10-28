// Port of skygrind/src/ui/menus.js's pure helpers (:96-140) for the CONTROLS screen, the pause key list and the screens'
// first focus: controlRows (the controls as table rows, from the live bindings), primaryIndex (which button a screen
// focuses when it opens) and keyGrid (the pause screen's compact key list). tabDefaults is already ported
// (SettingsMath.TabDefaults, Core/Settings.cs). NIGHT13 U3-26.7: keyGrid returns its rows ("KEY action") instead of the
// web's HTML spans; the UI draws them. `keyOf(action)` is the key's name for an action (the live binding's label, or the
// pad's glyph on the pad: main.js keyLabel).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>One controlRows group: a title and its [what, key] rows.</summary>
    public sealed class ControlGroup
    {
        public string title;
        public readonly List<(string what, string key)> rows = new List<(string, string)>();
    }

    public static class MenusMath
    {
        /// <summary>config.input.labels[a] ?? a.toUpperCase(): an action's label.</summary>
        public static string ActionLabel(GameConfig cfg, string action)
        {
            Dictionary<string, object> labels = cfg.Obj("input.labels");
            return labels.TryGetValue(action, out object l) && l is string s ? s : action.ToUpperInvariant();
        }

        /// <summary>
        /// The controls as table rows, from the live bindings (POLISH V5): every rebindable action by group, the mouse and the
        /// fixed keys, and the techniques with their keys filled in. Pure (unit tested). `keyOf(action)` -> the key's name.
        /// </summary>
        public static List<ControlGroup> ControlRows(GameConfig cfg, Func<string, string> keyOf)
        {
            var groups = new List<ControlGroup>();
            foreach (object o in cfg.Arr("ui.controlGroups"))
            {
                List<object> pair = PortData.Arr(o);
                var g = new ControlGroup { title = (string)pair[0] };
                foreach (object a in PortData.Arr(pair[1])) g.rows.Add((ActionLabel(cfg, (string)a), keyOf((string)a)));
                groups.Add(g);
            }
            groups.Add(Rows("MOUSE + FIXED KEYS", cfg.Arr("ui.mouseControls"), null));
            groups.Add(Rows("TECHNIQUES", cfg.Arr("ui.techniques"), keyOf));
            groups.Add(Rows("READING THE HUD", cfg.Arr("ui.hudLegend"), null));
            return groups;
        }

        /// <summary>A group of [what, text] pairs; with `keyOf`, the text's {action} placeholders filled (controlRows' fill).</summary>
        static ControlGroup Rows(string title, List<object> pairs, Func<string, string> keyOf)
        {
            var g = new ControlGroup { title = title };
            foreach (object o in pairs)
            {
                List<object> pair = PortData.Arr(o);
                string text = (string)pair[1];
                g.rows.Add(((string)pair[0], keyOf == null ? text : Bindings.Fill(text, keyOf, true)));
            }
            return g;
        }

        /// <summary>
        /// Pure (OVERNIGHT3 P2, docs/FRICTION.md #4): which button a screen focuses when it opens. A results screen focuses its
        /// one primary action (RETRY / REMATCH: Enter retries) instead of its first button (COPY SCORE LINE); every other
        /// screen its first button. `name` the screen; `ids` its enabled buttons' data-ids, in order.
        /// </summary>
        public static int PrimaryIndex(string name, IList<string> ids)
        {
            if (!name.StartsWith("end", StringComparison.Ordinal)) return 0;
            int i = -1;
            for (int k = 0; k < ids.Count; k++)
            {
                if (ids[k] == "retry" || ids[k] == "rematch")
                {
                    i = k;
                    break;
                }
            }
            return i < 0 ? 0 : i;
        }

        /// <summary>Pure (OVERNIGHT3 P2): the pause screen's compact key list: every rebindable action, (key, label) rows in the groups' order.</summary>
        public static List<(string key, string label)> KeyGrid(GameConfig cfg, Func<string, string> keyOf)
        {
            var rows = new List<(string, string)>();
            foreach (object o in cfg.Arr("ui.controlGroups"))
            {
                foreach (object a in PortData.Arr(PortData.Arr(o)[1])) rows.Add((keyOf((string)a), ActionLabel(cfg, (string)a)));
            }
            return rows;
        }
    }
}
