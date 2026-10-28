// Port of skygrind/src/ui/settings.js (createSettings, migrateSettings: the saved settings over config.settings, the
// one-time migrations) and menus.js's pure settings helpers (tabDefaults: RESET THIS TAB; rangeText: a range row's
// value text), for NIGHT8 U2-04's SETTINGS screen. The items and tabs are config.ui.settings / settingsTabs with the
// web's key names, so a saved profile reads the same keys. Saving is a flat JSON object (the web's localStorage value:
// every setting, "_migrated", plus "_version" here). Unity-only settings (vsync, the LAB) join as extra items with
// their own defaults; the web's loader ignores unknown keys.
// NIGHT13 U3-26.8: the Player keeps that JSON in a file beside the profile (GameSettings; PickSaved: the file wins,
// PlayerPrefs' copy migrates once), the store counts its changes (version: the Game's typed SettingsView rebuilds once
// per change), and one Unity-only migration (window1: a saved FULLSCREEN becomes the WINDOW MODE row's fullscreen).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Skygrind.Core
{
    /// <summary>One config.ui.settings row: key, tab, label, type (range | toggle | select) and its range or options.</summary>
    public sealed class SettingItem
    {
        public string key, tab, label, type, show, desc;
        public double min, max, step, fine;
        public int digits = 2;
        public bool hasDigits;
        public List<object> options = new List<object>();
        public Dictionary<string, string> labels = new Dictionary<string, string>();

        public static SettingItem From(Dictionary<string, object> o)
        {
            var i = new SettingItem
            {
                key = (string)o["key"],
                tab = o.TryGetValue("tab", out object t) ? (string)t : null,
                label = (string)o["label"],
                type = (string)o["type"],
                show = o.TryGetValue("show", out object s) ? (string)s : null,
                desc = o.TryGetValue("desc", out object d) ? (string)d : null,
            };
            if (o.TryGetValue("min", out object mn)) i.min = PortData.Num(mn);
            if (o.TryGetValue("max", out object mx)) i.max = PortData.Num(mx);
            if (o.TryGetValue("step", out object st)) i.step = PortData.Num(st);
            if (o.TryGetValue("fine", out object fn)) i.fine = PortData.Num(fn);
            if (o.TryGetValue("digits", out object dg))
            {
                i.digits = (int)PortData.Num(dg);
                i.hasDigits = true;
            }
            if (o.TryGetValue("options", out object op)) i.options.AddRange(PortData.Arr(op));
            if (o.TryGetValue("labels", out object lb)) foreach (var kv in PortData.Obj(lb)) i.labels[kv.Key] = (string)kv.Value;
            return i;
        }

        /// <summary>An option's label: its labels entry, else the option upper-cased (menus.js).</summary>
        public string OptionLabel(object option)
        {
            string k = SettingsMath.Key(option);
            return labels.TryGetValue(k, out string l) ? l : k.ToUpperInvariant();
        }
    }

    /// <summary>Where the saved settings came from at boot (SettingsMath.PickSaved).</summary>
    public enum SettingsSource { None, File, Legacy }

    public static class SettingsMath
    {
        /// <summary>The Unity-only migration id: a saved FULLSCREEN on becomes the WINDOW MODE row's fullscreen mode, once.</summary>
        public const string WindowMigration = "window1";

        /// <summary>
        /// NIGHT13 U3-26.8: the saved settings to load at boot. The settings file (beside the profile) wins whenever it holds
        /// anything, even a corrupt text (the store then keeps the defaults, as createSettings' catch does); PlayerPrefs' copy
        /// (the store until NIGHT13) is read only while there is no file, so it migrates once (the caller writes the file at
        /// once); neither: a new profile (json null).
        /// </summary>
        public static SettingsSource PickSaved(string fileText, string legacyText, out string json)
        {
            if (!string.IsNullOrWhiteSpace(fileText))
            {
                json = fileText;
                return SettingsSource.File;
            }
            if (!string.IsNullOrWhiteSpace(legacyText))
            {
                json = legacyText;
                return SettingsSource.Legacy;
            }
            json = null;
            return SettingsSource.None;
        }

        /// <summary>
        /// NIGHT13 U3-26.8 (Unity-only, id WindowMigration in "_migrated"): SETTINGS shows WINDOW MODE in place of the web's
        /// FULLSCREEN toggle, so a profile saved with FULLSCREEN on takes `fullscreenMode` once. True when `migrated` changed
        /// (the caller saves). A later choice stays: the id is remembered even when there was nothing to move.
        /// </summary>
        public static bool MigrateWindowMode(Dictionary<string, object> values, List<string> migrated, string windowKey, string fullscreenMode)
        {
            if (migrated.Contains(WindowMigration)) return false;
            if (values.TryGetValue("fullscreen", out object f) && f is bool on && on && values.ContainsKey(windowKey)) values[windowKey] = fullscreenMode;
            migrated.Add(WindowMigration);
            return true;
        }

        /// <summary>NIGHT15 U3-28.7: the Unity-only migration id of the design defaults (design.aimAssist, design.pad).</summary>
        public const string DesignDefaultsMigration = "assist15";

        /// <summary>
        /// NIGHT15 U3-28.7 (GG, 2026-10-05: "auto aim is too strong"): the Unity-side defaults over config.settings' (the web's),
        /// before Load: aimAssist = design.aimAssist.mouse, padAimAssist = design.aimAssist.pad, padVibration =
        /// design.pad.vibrationDefault. Without the design blocks the store keeps the web's defaults. The Game calls it; the
        /// web's own store (the ported tests, the headless fingerprint) never does.
        /// </summary>
        public static void ApplyDesignDefaults(SettingsStore store, GameConfig cfg)
        {
            foreach (var (key, path) in DesignDefaultKeys)
            {
                if (!cfg.Has(path) || !store.defaults.ContainsKey(key)) continue;
                object value = cfg.Get(path);
                if (value == null || value.GetType() != store.defaults[key].GetType()) continue; // the store's type rule
                store.defaults[key] = value;
                store.values[key] = value;
            }
        }

        /// <summary>The settings the design blocks give new defaults (key, design path).</summary>
        static readonly (string key, string path)[] DesignDefaultKeys =
        {
            ("aimAssist", "design.aimAssist.mouse"), ("padAimAssist", "design.aimAssist.pad"), ("padVibration", "design.pad.vibrationDefault"),
        };

        /// <summary>
        /// NIGHT15 U3-28.7, after Load, once per profile (DesignDefaultsMigration in "_migrated"): a saved value still equal to
        /// the web's old default (design.aimAssist.migrateFrom for both aim modes; false for VIBRATION) moves to the new
        /// default; any other saved value is the player's choice and stays. A new profile (nothing saved) just records the id.
        /// True when the store changed (the caller saves).
        /// </summary>
        public static bool MigrateDesignDefaults(SettingsStore store, GameConfig cfg, bool hadSaved)
        {
            if (store.migrated.Contains(DesignDefaultsMigration)) return false;
            if (!cfg.Has("design.aimAssist") && !cfg.Has("design.pad")) return false; // no design blocks: the web's store
            if (hadSaved)
            {
                string oldAim = cfg.Has("design.aimAssist.migrateFrom") ? cfg.Str("design.aimAssist.migrateFrom") : null;
                foreach (var (key, path) in DesignDefaultKeys)
                {
                    if (!cfg.Has(path) || !store.values.TryGetValue(key, out object cur)) continue;
                    object web = cfg.Has("settings." + key) ? cfg.Get("settings." + key) : null; // the web's default
                    object old = key == "padVibration" ? web : oldAim;
                    if (old != null && Equals(cur, old)) store.values[key] = store.defaults[key];
                }
            }
            store.migrated.Add(DesignDefaultsMigration);
            store.version++; // a value may have changed outside Set: the Game's view rebuilds
            return true;
        }

        /// <summary>A setting value as the web's object key / text (numbers in their shortest JS form).</summary>
        public static string Key(object v) => v is double d ? d.ToString("R", CultureInfo.InvariantCulture) : v is bool b ? (b ? "true" : "false") : (string)v;

        /// <summary>
        /// settings.js migrateSettings: one-time migrations of a saved settings object, applied in place to `values` (the
        /// live settings, already filled from `saved`); returns the migration ids done (saved as "_migrated").
        /// - 'sens14': a saved sensitivity equal to the old default (0.0022) becomes the new default (0.0014).
        /// - 'snap1': the old aim-assist default 'soft' becomes 'snap'.
        /// </summary>
        public static List<string> MigrateSettings(Dictionary<string, object> saved, Dictionary<string, object> values, double oldSensitivity, double sensitivity, List<object> aimModes)
        {
            var done = new List<string>();
            if (saved != null && saved.TryGetValue("_migrated", out object m) && m is List<object> list) foreach (object x in list) done.Add((string)x);
            if (saved == null) return new List<string> { "sens14", "snap1" }; // a new profile starts on the new defaults: nothing to migrate
            if (!done.Contains("sens14"))
            {
                double s = saved.TryGetValue("sensitivity", out object so) && so is double sd ? sd : double.NaN;
                if (Math.Abs(s - oldSensitivity) < 1e-9) values["sensitivity"] = sensitivity;
                done.Add("sens14");
            }
            if (!done.Contains("snap1"))
            {
                if (saved.TryGetValue("aimAssist", out object a) && a is string sa && sa == "soft") values["aimAssist"] = "snap";
                done.Add("snap1");
            }
            if (!(values.TryGetValue("aimAssist", out object cur) && aimModes.Contains(cur))) values["aimAssist"] = "snap";
            return done;
        }

        /// <summary>menus.js tabDefaults: the (key, value) pairs that put one settings tab back to its defaults.</summary>
        public static List<(string key, object value)> TabDefaults(List<SettingItem> items, string tab, Dictionary<string, object> defaults, string firstTab)
        {
            var output = new List<(string, object)>();
            foreach (SettingItem i in items)
            {
                if ((i.tab ?? firstTab) == tab && defaults.ContainsKey(i.key)) output.Add((i.key, defaults[i.key]));
            }
            return output;
        }

        /// <summary>menus.js rangeText: the value to item.digits (default 2); the sensitivity also in degrees per 1000 counts.</summary>
        public static string RangeText(SettingItem item, double value)
        {
            string text = value.ToString("F" + (item.hasDigits ? item.digits : 2), CultureInfo.InvariantCulture);
            if (item.show == "degPer1000") return $"{text} · {(value * 1000 * 180 / Math.PI).ToString("F1", CultureInfo.InvariantCulture)}°/1K";
            return text;
        }

        /// <summary>A range value moved by `dir` steps (fine steps for a row that has them), clamped, rounded to the step's decimals.</summary>
        public static double Nudge(SettingItem item, double value, int dir)
        {
            double step = item.fine > 0 ? item.fine : item.step > 0 ? item.step : 1;
            int decimals = Math.Max(0, (int)Math.Ceiling(-Math.Log10(step) - 1e-9));
            double v = Math.Round(value + dir * step, decimals);
            return Math.Max(item.min, Math.Min(item.max, v));
        }

        /// <summary>The next option of a select (wrapping), from the current value.</summary>
        public static object Cycle(SettingItem item, object value, int dir)
        {
            if (item.options.Count == 0) return value;
            int i = item.options.FindIndex(o => Key(o) == Key(value));
            int n = item.options.Count;
            return item.options[((i < 0 ? 0 : i + dir) % n + n) % n];
        }

        /// <summary>A flat JSON object of strings, numbers, booleans and string lists (the saved settings).</summary>
        public static string ToJson(IEnumerable<KeyValuePair<string, object>> fields)
        {
            var sb = new StringBuilder("{");
            bool first = true;
            foreach (var kv in fields)
            {
                if (!first) sb.Append(',');
                first = false;
                Str(sb, kv.Key);
                sb.Append(':');
                Value(sb, kv.Value);
            }
            return sb.Append('}').ToString();
        }

        static void Value(StringBuilder sb, object v)
        {
            switch (v)
            {
                case null: sb.Append("null"); break;
                case bool b: sb.Append(b ? "true" : "false"); break;
                case double d: sb.Append(d.ToString("R", CultureInfo.InvariantCulture)); break;
                case int i: sb.Append(i.ToString(CultureInfo.InvariantCulture)); break;
                case string s: Str(sb, s); break;
                case IEnumerable<string> list:
                    sb.Append('[');
                    bool first = true;
                    foreach (string s in list)
                    {
                        if (!first) sb.Append(',');
                        first = false;
                        Str(sb, s);
                    }
                    sb.Append(']');
                    break;
                default: throw new FormatException($"settings JSON can't hold a {v.GetType().Name}");
            }
        }

        static void Str(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                if (c == '"' || c == '\\') sb.Append('\\').Append(c);
                else if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                else sb.Append(c);
            }
            sb.Append('"');
        }
    }

    /// <summary>settings.js createSettings: the live values (config.settings defaults, then the saved ones of the same type), saved as JSON.</summary>
    public sealed class SettingsStore
    {
        public const string StorageKey = "skygrind.settings.v1"; // the web's localStorage key
        public const int Version = 1;

        public readonly Dictionary<string, object> defaults = new Dictionary<string, object>();
        public readonly Dictionary<string, object> values = new Dictionary<string, object>();
        public readonly List<SettingItem> items = new List<SettingItem>();
        public readonly List<(string id, string label)> tabs = new List<(string, string)>();
        public List<string> migrated = new List<string>();
        public int version; // NIGHT13 U3-26.8: + 1 on every change of the values (Load, Set, ResetTab): the Game's typed view rebuilds on it
        readonly double oldSensitivity, sensitivity;
        readonly List<object> aimModes;

        /// <summary>From config.settings and config.ui.settings / settingsTabs, plus Unity-only items (key -> default).</summary>
        public SettingsStore(GameConfig cfg, IList<(SettingItem item, object value)> extras = null, IList<(string id, string label)> extraTabs = null)
        {
            foreach (var kv in cfg.Obj("settings")) defaults[kv.Key] = kv.Value;
            foreach (object o in cfg.Arr("ui.settings")) items.Add(SettingItem.From(PortData.Obj(o)));
            foreach (object o in cfg.Arr("ui.settingsTabs"))
            {
                var t = PortData.Obj(o);
                tabs.Add(((string)t["id"], (string)t["label"]));
            }
            if (extraTabs != null) tabs.AddRange(extraTabs);
            if (extras != null)
            {
                foreach (var (item, value) in extras)
                {
                    items.Add(item);
                    defaults[item.key] = value;
                }
            }
            oldSensitivity = cfg.Num("mouse.oldDefault");
            sensitivity = cfg.Num("mouse.sensitivity");
            SettingItem aim = items.Find(i => i.key == "aimAssist");
            aimModes = aim != null ? aim.options : new List<object> { "off", "assist", "snap", "lock" };
            foreach (var kv in defaults) values[kv.Key] = kv.Value;
        }

        public string FirstTab => tabs.Count > 0 ? tabs[0].id : "aim";

        /// <summary>
        /// Load a saved JSON (null, empty or corrupt: the defaults). Only values of the default's type are taken. Returns
        /// true when the migrations changed the list, so the caller saves at once (createSettings does).
        /// </summary>
        public bool Load(string json)
        {
            foreach (var kv in defaults) values[kv.Key] = kv.Value;
            Dictionary<string, object> saved = null;
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    saved = JsJson.Parse(json) as Dictionary<string, object>;
                }
                catch (Exception)
                {
                    saved = null; // corrupt: keep the defaults
                }
            }
            if (saved != null)
            {
                foreach (var kv in defaults)
                {
                    if (saved.TryGetValue(kv.Key, out object v) && v != null && v.GetType() == kv.Value.GetType()) values[kv.Key] = v;
                }
            }
            int before = saved != null && saved.TryGetValue("_migrated", out object m) && m is List<object> l ? l.Count : 0;
            migrated = SettingsMath.MigrateSettings(saved, values, oldSensitivity, sensitivity, aimModes);
            version++;
            return saved != null && migrated.Count != before;
        }

        public string Save()
        {
            var fields = new List<KeyValuePair<string, object>>();
            foreach (var kv in values) fields.Add(kv);
            fields.Add(new KeyValuePair<string, object>("_migrated", migrated));
            fields.Add(new KeyValuePair<string, object>("_version", (double)Version));
            return SettingsMath.ToJson(fields);
        }

        public double Num(string key) => (double)values[key];

        public bool Bool(string key) => (bool)values[key];

        public string Str(string key) => (string)values[key];

        public SettingItem Item(string key) => items.Find(i => i.key == key);

        /// <summary>Set a value of the default's type (a range is clamped to its item's min / max).</summary>
        public void Set(string key, object value)
        {
            if (!defaults.TryGetValue(key, out object d) || value == null || value.GetType() != d.GetType()) throw new ArgumentException($"setting '{key}' takes a {d?.GetType().Name ?? "known key"}");
            SettingItem item = Item(key);
            if (item != null && item.type == "range" && value is double v) value = Math.Max(item.min, Math.Min(item.max, v));
            values[key] = value;
            version++;
        }

        /// <summary>RESET THIS TAB: the tab's settings back to their defaults.</summary>
        public void ResetTab(string tab)
        {
            foreach (var (key, value) in SettingsMath.TabDefaults(items, tab, defaults, FirstTab)) values[key] = value;
            version++;
        }
    }
}
