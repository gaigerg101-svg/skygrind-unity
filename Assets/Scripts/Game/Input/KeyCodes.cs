// NIGHT13 U3-26.7 (agent I; audit-input section 4 row 8, risk 2): the web's key codes (KeyboardEvent.code: "KeyE",
// "ShiftLeft", "ArrowUp", "Equal", "Digit1", ...; what config.input.bindings and a saved bindings map hold) <-> the Input
// System's Key, both ways. Replaces SurvivalGame.KeyOf, which parsed the code as a Key name and so read "ShiftLeft" (the
// dash default) as Key.None. Label(code) is a prompt's key text: bindings.js keyName upper-cased, as main.js keyLabel
// prints it (KeyE -> E, ShiftLeft -> L-SHIFT, Space -> SPACE, ArrowUp -> UP). The codes the table doesn't know (IntlRo,
// media keys, ...) read Key.None. Built once; lookups allocate nothing (Label caches its text per code).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Skygrind.Game
{
    public static class KeyCodes
    {
        static readonly (string code, Key key)[] Table =
        {
            ("KeyA", Key.A), ("KeyB", Key.B), ("KeyC", Key.C), ("KeyD", Key.D), ("KeyE", Key.E), ("KeyF", Key.F), ("KeyG", Key.G),
            ("KeyH", Key.H), ("KeyI", Key.I), ("KeyJ", Key.J), ("KeyK", Key.K), ("KeyL", Key.L), ("KeyM", Key.M), ("KeyN", Key.N),
            ("KeyO", Key.O), ("KeyP", Key.P), ("KeyQ", Key.Q), ("KeyR", Key.R), ("KeyS", Key.S), ("KeyT", Key.T), ("KeyU", Key.U),
            ("KeyV", Key.V), ("KeyW", Key.W), ("KeyX", Key.X), ("KeyY", Key.Y), ("KeyZ", Key.Z),
            ("Digit0", Key.Digit0), ("Digit1", Key.Digit1), ("Digit2", Key.Digit2), ("Digit3", Key.Digit3), ("Digit4", Key.Digit4),
            ("Digit5", Key.Digit5), ("Digit6", Key.Digit6), ("Digit7", Key.Digit7), ("Digit8", Key.Digit8), ("Digit9", Key.Digit9),
            ("Space", Key.Space), ("Enter", Key.Enter), ("Tab", Key.Tab), ("Backquote", Key.Backquote), ("Quote", Key.Quote),
            ("Semicolon", Key.Semicolon), ("Comma", Key.Comma), ("Period", Key.Period), ("Slash", Key.Slash), ("Backslash", Key.Backslash),
            ("BracketLeft", Key.LeftBracket), ("BracketRight", Key.RightBracket), ("Minus", Key.Minus), ("Equal", Key.Equals),
            ("ShiftLeft", Key.LeftShift), ("ShiftRight", Key.RightShift), ("AltLeft", Key.LeftAlt), ("AltRight", Key.RightAlt),
            ("ControlLeft", Key.LeftCtrl), ("ControlRight", Key.RightCtrl), ("MetaLeft", Key.LeftMeta), ("MetaRight", Key.RightMeta),
            ("ContextMenu", Key.ContextMenu), ("Escape", Key.Escape),
            ("ArrowLeft", Key.LeftArrow), ("ArrowRight", Key.RightArrow), ("ArrowUp", Key.UpArrow), ("ArrowDown", Key.DownArrow),
            ("Backspace", Key.Backspace), ("PageDown", Key.PageDown), ("PageUp", Key.PageUp), ("Home", Key.Home), ("End", Key.End),
            ("Insert", Key.Insert), ("Delete", Key.Delete), ("CapsLock", Key.CapsLock), ("NumLock", Key.NumLock),
            ("PrintScreen", Key.PrintScreen), ("ScrollLock", Key.ScrollLock), ("Pause", Key.Pause),
            ("NumpadEnter", Key.NumpadEnter), ("NumpadDivide", Key.NumpadDivide), ("NumpadMultiply", Key.NumpadMultiply),
            ("NumpadAdd", Key.NumpadPlus), ("NumpadSubtract", Key.NumpadMinus), ("NumpadDecimal", Key.NumpadPeriod),
            ("NumpadEqual", Key.NumpadEquals),
            ("Numpad0", Key.Numpad0), ("Numpad1", Key.Numpad1), ("Numpad2", Key.Numpad2), ("Numpad3", Key.Numpad3), ("Numpad4", Key.Numpad4),
            ("Numpad5", Key.Numpad5), ("Numpad6", Key.Numpad6), ("Numpad7", Key.Numpad7), ("Numpad8", Key.Numpad8), ("Numpad9", Key.Numpad9),
            ("F1", Key.F1), ("F2", Key.F2), ("F3", Key.F3), ("F4", Key.F4), ("F5", Key.F5), ("F6", Key.F6), ("F7", Key.F7), ("F8", Key.F8),
            ("F9", Key.F9), ("F10", Key.F10), ("F11", Key.F11), ("F12", Key.F12), ("F13", Key.F13), ("F14", Key.F14), ("F15", Key.F15),
            ("F16", Key.F16), ("F17", Key.F17), ("F18", Key.F18), ("F19", Key.F19), ("F20", Key.F20),
        };

        static readonly Dictionary<string, Key> keyOf = new Dictionary<string, Key>(Table.Length);
        static readonly string[] codeOf; // by (int)Key (an array: no enum-keyed dictionary)
        static readonly Dictionary<string, string> labels = new Dictionary<string, string>();
        static readonly List<string> codes = new List<string>(Table.Length);

        static KeyCodes()
        {
            int max = 0;
            foreach (var (_, key) in Table) max = System.Math.Max(max, (int)key);
            codeOf = new string[max + 1];
            foreach (var (code, key) in Table)
            {
                keyOf[code] = key;
                codeOf[(int)key] = code;
                codes.Add(code);
            }
        }

        /// <summary>Every web code the table knows, in the table's order.</summary>
        public static IReadOnlyList<string> Codes => codes;

        /// <summary>A web key code as an Input System key (Key.None for null or a code the table doesn't know).</summary>
        public static Key ToKey(string code) => code != null && keyOf.TryGetValue(code, out Key k) ? k : Key.None;

        /// <summary>An Input System key as the web's code, or null (Key.None, or a key the web names differently: OEM keys, IMESelected).</summary>
        public static string ToCode(Key key) => (int)key > 0 && (int)key < codeOf.Length ? codeOf[(int)key] : null;

        /// <summary>(int)ToKey(code): for callers without the Input System's assembly (the Game tests). 0 is Key.None.</summary>
        public static int KeyIndex(string code) => (int)ToKey(code);

        /// <summary>ToCode((Key)index).</summary>
        public static string CodeOfIndex(int index) => ToCode((Key)index);

        /// <summary>The Input System's name of the code's key ("ShiftLeft" -> "LeftShift"; unknown -> "None"). Tests and logs only (allocates).</summary>
        public static string KeyEnumName(string code) => ToKey(code).ToString();

        /// <summary>The key's prompt text: bindings.js keyName(code).toUpperCase() (main.js keyLabel); cached per code.</summary>
        public static string Label(string code)
        {
            if (code == null) return Bindings.KeyName(null);
            if (!labels.TryGetValue(code, out string text)) labels[code] = text = Bindings.KeyName(code).ToUpperInvariant();
            return text;
        }

        /// <summary>The key bound to `action` in the store (Key.None when the store is null or the action unbound).</summary>
        public static Key Bound(Bindings bindings, string action) => bindings == null ? Key.None : ToKey(bindings.Code(action));

        /// <summary>A key's held state, Key.None never held (Keyboard's indexer has no None).</summary>
        public static bool Held(Keyboard kb, Key key) => key != Key.None && kb[key].isPressed;

        /// <summary>A key's press this frame, Key.None never pressed.</summary>
        public static bool Pressed(Keyboard kb, Key key) => key != Key.None && kb[key].wasPressedThisFrame;

        /// <summary>A key's release this frame, Key.None never released.</summary>
        public static bool Released(Keyboard kb, Key key) => key != Key.None && kb[key].wasReleasedThisFrame;

        /// <summary>
        /// CONTROLS' key capture (menus.js :548-558): the web code of the first key pressed this frame that the table knows,
        /// or null. Walks Keyboard.allKeys by index (no allocation); call it only while waiting for a key.
        /// </summary>
        public static string PressedCode(Keyboard kb)
        {
            if (kb == null) return null;
            var keys = kb.allKeys;
            for (int i = 0; i < keys.Count; i++)
            {
                KeyControl k = keys[i];
                if (k == null || !k.wasPressedThisFrame) continue;
                string c = ToCode(k.keyCode);
                if (c != null) return c;
            }
            return null;
        }
    }
}
