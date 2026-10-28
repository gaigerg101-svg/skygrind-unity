// Port of skygrind/src/input/bindings.js: key bindings (OVERNIGHT2 N6 item 7): every keyboard action can be rebound on the
// CONTROLS screen. A key already used by another action swaps with it (the screen names the conflict), Ctrl is never
// bindable (Ctrl+W closes the tab), Esc stays the menu key, and RESET puts the defaults back. Saved in localStorage
// (try/catch). The mouse buttons and the number keys (weapon slots) stay fixed. Pure table logic (tested) + a small live
// store.
// NIGHT13 U3-26.7 (Unity): the codes stay the web's KeyboardEvent.code strings ("KeyW", "ShiftLeft", "Space"), so a saved
// map reads the same; Game/Input/KeyCodes.cs turns them into Input System keys. A map is a BindingMap (the web object's
// key order kept: rebind's "the other action" and actionOf take the first in that order). The store saves through a
// `persist` callback (the Game side writes the JSON to PlayerPrefs under StorageKey, as the web's localStorage) and
// counts its changes in Version (the readers re-resolve their keys when it moves).
using System;
using System.Collections.Generic;
using System.Text;

namespace Skygrind.Core
{
    /// <summary>An action -> key code map (a bindings.js map object) in its key order.</summary>
    public sealed class BindingMap
    {
        readonly List<string> actions = new List<string>();
        readonly Dictionary<string, string> codes = new Dictionary<string, string>();

        /// <summary>config.input.bindings ({action: code}) in the JSON's order.</summary>
        public static BindingMap From(Dictionary<string, object> bindings)
        {
            var m = new BindingMap();
            foreach (var kv in bindings) m.Put(kv.Key, (string)kv.Value);
            return m;
        }

        public int Count => actions.Count;

        /// <summary>The i-th action in key order.</summary>
        public string ActionAt(int i) => actions[i];

        public bool Has(string action) => action != null && codes.ContainsKey(action);

        /// <summary>map[action]: the action's code, or null.</summary>
        public string this[string action] => action != null && codes.TryGetValue(action, out string c) ? c : null;

        /// <summary>The first action (in key order) bound to `code`, or null.</summary>
        public string ActionOf(string code)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                if (codes[actions[i]] == code) return actions[i];
            }
            return null;
        }

        /// <summary>`{ ...map }`.</summary>
        public BindingMap Copy()
        {
            var m = new BindingMap();
            foreach (string a in actions) m.Put(a, codes[a]);
            return m;
        }

        /// <summary>map[action] = code (a new action goes last; an existing one keeps its place).</summary>
        public void Put(string action, string code)
        {
            if (!codes.ContainsKey(action)) actions.Add(action);
            codes[action] = code;
        }

        /// <summary>vitest toEqual: the same actions with the same codes.</summary>
        public bool SameAs(BindingMap other)
        {
            if (other == null || other.Count != Count) return false;
            foreach (string a in actions)
            {
                if (other[a] != codes[a]) return false;
            }
            return true;
        }

        /// <summary>JSON.stringify(map): {"forward":"KeyW",...} in key order.</summary>
        public string ToJson()
        {
            var sb = new StringBuilder("{");
            for (int i = 0; i < actions.Count; i++)
            {
                if (i > 0) sb.Append(',');
                Bindings.JsonString(sb, actions[i]);
                sb.Append(':');
                Bindings.JsonString(sb, codes[actions[i]]);
            }
            return sb.Append('}').ToString();
        }
    }

    /// <summary>rebind's result: the map (a copy, or the input when refused), ok, and the action that took the old key (or null).</summary>
    public struct RebindResult
    {
        public BindingMap map;
        public bool ok;
        public string conflict;
    }

    /// <summary>bindings.js: the pure table logic (static) and createBindings' live store (an instance).</summary>
    public sealed class Bindings
    {
        /// <summary>The web's localStorage key (STORE_KEY :6).</summary>
        public const string StorageKey = "skygrind.bindings.v1";

        /// <summary>Keys that can never be bound (UNBINDABLE :7-8).</summary>
        public static readonly HashSet<string> Unbindable = new HashSet<string> { "ControlLeft", "ControlRight", "MetaLeft", "MetaRight", "Escape", "F5", "F11", "F12" };

        static readonly Dictionary<string, string> Names = new Dictionary<string, string>
        {
            ["Space"] = "SPACE", ["ShiftLeft"] = "L-SHIFT", ["ShiftRight"] = "R-SHIFT", ["AltLeft"] = "L-ALT", ["AltRight"] = "R-ALT",
            ["Tab"] = "TAB", ["CapsLock"] = "CAPS", ["Enter"] = "ENTER", ["Backquote"] = "`",
        };

        /// <summary>Pure: a readable name for a key code (KeyW -> W, ShiftLeft -> L-SHIFT, Space -> SPACE).</summary>
        public static string KeyName(string code)
        {
            if (string.IsNullOrEmpty(code)) return "-";
            if (code.StartsWith("Key", StringComparison.Ordinal)) return code.Substring(3);
            if (code.StartsWith("Digit", StringComparison.Ordinal)) return code.Substring(5);
            if (Names.TryGetValue(code, out string name)) return name;
            int arrow = code.IndexOf("Arrow", StringComparison.Ordinal); // String.replace with a string: the first one only
            string rest = arrow < 0 ? code : code.Substring(0, arrow) + code.Substring(arrow + 5);
            return rest.ToUpperInvariant();
        }

        /// <summary>
        /// Pure: fill `{action}` placeholders in a prompt with that action's key name (OVERNIGHT3 P2: the Training signs and
        /// the hints follow a rebind, like CONTROLS). Unknown names stay as they are (keyOf returns null for them).
        /// </summary>
        public static string FillKeys(string text, Func<string, string> keyOf) => Fill(text, keyOf, false);

        /// <summary>
        /// text.replace(/\{(\w+)\}/g, ...): each `{name}` (name = one or more [A-Za-z0-9_]) becomes keyOf(name); a null
        /// keeps the placeholder (fillKeys' `?? whole`), or reads "undefined" when `undefinedText` (menus.js controlRows,
        /// whose replacer returns keyOf's value as it is).
        /// </summary>
        public static string Fill(string text, Func<string, string> keyOf, bool undefinedText)
        {
            var sb = new StringBuilder(text.Length);
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] == '{')
                {
                    int j = i + 1;
                    while (j < text.Length && IsWord(text[j])) j++;
                    if (j > i + 1 && j < text.Length && text[j] == '}')
                    {
                        string name = text.Substring(i + 1, j - i - 1);
                        string key = keyOf(name);
                        sb.Append(key ?? (undefinedText ? "undefined" : text.Substring(i, j - i + 1)));
                        i = j + 1;
                        continue;
                    }
                }
                sb.Append(text[i]);
                i++;
            }
            return sb.ToString();
        }

        static bool IsWord(char c) => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_';

        /// <summary>
        /// Pure: bind `action` to `code` in `map` (a copy comes back). A code another action uses swaps with it (that action
        /// takes this one's old key). Unbindable codes are refused.
        /// </summary>
        public static RebindResult Rebind(BindingMap map, string action, string code)
        {
            if ((code != null && Unbindable.Contains(code)) || !map.Has(action)) return new RebindResult { map = map, ok = false, conflict = null };
            BindingMap next = map.Copy();
            string other = null;
            for (int i = 0; i < next.Count; i++)
            {
                string a = next.ActionAt(i);
                if (a != action && next[a] == code)
                {
                    other = a;
                    break;
                }
            }
            if (other != null) next.Put(other, map[action]);
            next.Put(action, code);
            return new RebindResult { map = next, ok = true, conflict = other };
        }

        /// <summary>Pure: actions that share a key (should never happen after rebind; a hand-edited store could).</summary>
        public static List<string> Conflicts(BindingMap map)
        {
            var codeOrder = new List<string>();
            var by = new Dictionary<string, List<string>>();
            for (int i = 0; i < map.Count; i++)
            {
                string a = map.ActionAt(i), c = map[a] ?? "null";
                if (!by.TryGetValue(c, out List<string> list))
                {
                    by[c] = list = new List<string>();
                    codeOrder.Add(c);
                }
                list.Add(a);
            }
            var output = new List<string>();
            foreach (string c in codeOrder)
            {
                if (by[c].Count > 1) output.AddRange(by[c]);
            }
            return output;
        }

        internal static void JsonString(StringBuilder sb, string s)
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

        // --- createBindings: the live store --------------------------------------------------------------------------

        /// <summary>config.input.bindings: the {action: code} defaults.</summary>
        public readonly BindingMap defaults;
        BindingMap map;
        readonly List<Action<BindingMap>> listeners = new List<Action<BindingMap>>();

        /// <summary>The web's save(): called with the map's JSON on every Set / Reset (the Game side writes it under StorageKey).</summary>
        public Action<string> persist;

        /// <summary>+1 on every change of the map (Load, Set, Reset): the readers re-resolve their keys when it moves.</summary>
        public int Version { get; private set; }

        public Bindings(GameConfig cfg) : this(BindingMap.From(cfg.Obj("input.bindings")))
        {
        }

        public Bindings(BindingMap defaults)
        {
            this.defaults = defaults.Copy();
            map = defaults.Copy();
        }

        /// <summary>
        /// The saved map (null, corrupt or not an object: the defaults): a saved action the defaults know, with a string
        /// code that can be bound, replaces the default; a saved map whose keys clash falls back to the defaults.
        /// </summary>
        public void Load(string json)
        {
            map = defaults.Copy();
            try
            {
                object saved = JsJson.Parse(json ?? "null");
                if (saved is Dictionary<string, object> obj)
                {
                    foreach (var kv in obj)
                    {
                        if (map.Has(kv.Key) && kv.Value is string c && !Unbindable.Contains(c)) map.Put(kv.Key, c);
                    }
                    if (Conflicts(map).Count > 0) map = defaults.Copy();
                }
            }
            catch (Exception)
            {
                map = defaults.Copy(); // storage blocked or corrupt: the defaults
            }
            Version++;
        }

        /// <summary>JSON.stringify(map): what the store saves.</summary>
        public string Save() => map.ToJson();

        /// <summary>The live action -> key code map (read it every time; a rebind replaces it).</summary>
        public BindingMap Map => map;

        public string Code(string action) => map[action];

        /// <summary>The action a key code triggers, or null.</summary>
        public string ActionOf(string code) => map.ActionOf(code);

        public RebindResult Set(string action, string code)
        {
            RebindResult r = Rebind(map, action, code);
            if (r.ok)
            {
                map = r.map;
                Version++;
                persist?.Invoke(Save());
                foreach (Action<BindingMap> fn in listeners) fn(map);
            }
            return r;
        }

        public void Reset()
        {
            map = defaults.Copy();
            Version++;
            persist?.Invoke(Save());
            foreach (Action<BindingMap> fn in listeners) fn(map);
        }

        public void OnChange(Action<BindingMap> fn) => listeners.Add(fn);
    }
}
