// NIGHT19 (the shared commit placed the stub and every call; builder P owns this file): the ONE API every prompt calls for
// a key or button name, so the pad shows the pad (S12) without anyone editing another builder's prompt site.
// NIGHT19 U3-33.4 (builder P), with design.pad.n19 live (PlayerDevices.N19; off, or `-skygrindOff19`: `today`, NIGHT18's
// text, everywhere):
// - the pad used last: the live binding's button (PlayerDevices.Layout: remaps included) drawn as a glyph sprite in the
//   pad's set (Xbox, PlayStation, Switch by position; an unknown pad draws Xbox's, as Steam falls back), a HOLD prompt as
//   that glyph inside the cyan ring (the hold mark); the words (Glyphs.Button, "HOLD X") when the sprites aren't built
//   (PromptGlyphs.Ready false: the fallback; Words gives them for a screen reader). Actions: the pad's own names
//   (GamepadMath.ActionNames), "radial" read as the pad's "weaponWheel" (the WHEEL prompt bug: its glyph is the button
//   that holds the wheel open), the move actions as the web's PAD_MOVE (forward: the left stick; back / left / right:
//   nothing); an action no pad button carries ("restart") reads as nothing ("" , or null where the site passed null: the
//   site hides its chip), never a key;
// - the hotbar's cycle line (`today` one of UnityTuning.HudPadCycle*): "<NEXT WEAPON's glyph> NEXT · <PREVIOUS's> BACK";
// - the keyboard and mouse used last: `today` (each site's own live key), except the HOLD buy ("HOLD E" was typed at the
//   site): "HOLD " + the live interact key.
// No allocation per call: every answer is cached per action (and per HOLD) until the device, the glyph set, the layout, the
// key bindings or the sprites change; Version counts those changes so a site can rebuild its text once on a change.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class PromptText
    {
        /// <summary>The glyph sets drawn (Xbox, PlayStation, Switch) and the glyphs a set holds (16 buttons, 2 sticks).</summary>
        public const int SetCount = 3, GlyphCount = 18;

        /// <summary>The left / right stick's glyph index (after PadButtonName's 16).</summary>
        public const int LeftStick = 16, RightStick = 17;

        static readonly string[] Names = new string[2 * SetCount * GlyphCount];
        static readonly string[] Tags = new string[2 * SetCount * GlyphCount];

        static PromptText()
        {
            for (int h = 0; h < 2; h++)
            {
                for (int s = 0; s < SetCount; s++)
                {
                    for (int g = 0; g < GlyphCount; g++)
                    {
                        int i = Index(s, g, h == 1);
                        Names[i] = UnityTuning.PromptSetNames[s] + UnityTuning.PromptGlyphNames[g] + (h == 1 ? "hold" : "");
                        Tags[i] = "<sprite=\"" + UnityTuning.PromptSpriteAsset + "\" name=\"" + Names[i] + "\">";
                    }
                }
            }
        }

        static int Index(int set, int glyph, bool hold) => (hold ? SetCount * GlyphCount : 0) + set * GlyphCount + glyph;

        /// <summary>A glyph's sprite name in the atlas (PromptGlyphs names its characters with it).</summary>
        public static string SpriteName(int set, int glyph, bool hold) => Names[Index(set, glyph, hold)];

        /// <summary>The atlas set a pad's glyph set draws with (Neutral: Xbox's, as Steam falls back to its own art).</summary>
        public static int SetOf(GlyphSet set) => set == GlyphSet.PlayStation ? 1 : set == GlyphSet.Nintendo ? 2 : 0;

        /// <summary>Tests: draw the sprites without building the asset (PromptGlyphs needs Unity's runtime).</summary>
        public static bool ForceSprites;

        static bool Sprites => ForceSprites || PromptGlyphs.Ready;

        /// <summary>A pad glyph as TMP rich text: the sprite tag, or the set's words (+ "HOLD ") without the sprites.</summary>
        public static string Glyph(int glyph, GlyphSet set, bool hold)
        {
            if (Sprites) return Tags[Index(SetOf(set), glyph, hold)];
            return Words(glyph, set, hold);
        }

        /// <summary>A pad button's glyph (Glyph by PadButtonName).</summary>
        public static string Button(PadButtonName b, GlyphSet set, bool hold = false) => Glyph((int)b, set, hold);

        // The words (cached: one string per set x glyph x hold).
        static readonly string[] words = new string[2 * 4 * GlyphCount];

        /// <summary>A glyph's words in the pad's word set (Glyphs.Button; the sticks LEFT / RIGHT STICK), "HOLD " before a HOLD's.</summary>
        public static string Words(int glyph, GlyphSet set, bool hold)
        {
            int i = (hold ? 4 * GlyphCount : 0) + (int)set * GlyphCount + glyph;
            if (words[i] != null) return words[i];
            string w = glyph == LeftStick ? "LEFT STICK" : glyph == RightStick ? "RIGHT STICK" : Glyphs.Button((PadButtonName)glyph, set);
            return words[i] = hold ? UnityTuning.PromptHoldWord + w : w;
        }

        // --- the cache's stamp: what an answer depends on ----------------------------------------------------------------

        static InputDevice stampDevice = (InputDevice)(-1);
        static GlyphSet stampSet;
        static PadLayout stampLayout;
        static bool stampN19, stampSprites;
        static int stampBindings = int.MinValue;
        static int version;
        static readonly Dictionary<string, string> padCache = new Dictionary<string, string>(32), padHoldCache = new Dictionary<string, string>(8);
        static string cycleLine, holdBuyKeys;

        /// <summary>Moves when an answer may have changed (the device, the glyph set, the layout, the keys, the sprites).</summary>
        public static int Version
        {
            get
            {
                Refresh();
                return version;
            }
        }

        static void Refresh()
        {
            int bindings = PlayerDevices.Bindings != null ? PlayerDevices.Bindings.Version : -1;
            bool sprites = Sprites;
            if (PlayerDevices.Last == stampDevice && PlayerDevices.Glyphs == stampSet && ReferenceEquals(PlayerDevices.Layout, stampLayout)
                && PlayerDevices.N19 == stampN19 && bindings == stampBindings && sprites == stampSprites) return;
            stampDevice = PlayerDevices.Last;
            stampSet = PlayerDevices.Glyphs;
            stampLayout = PlayerDevices.Layout;
            stampN19 = PlayerDevices.N19;
            stampBindings = bindings;
            stampSprites = sprites;
            padCache.Clear();
            padHoldCache.Clear();
            cycleLine = holdBuyKeys = null;
            version++;
        }

        /// <summary>The prompt text for an action (an input.bindings / PadAction web name: "interact", "restart",
        /// "nextWeapon", "weaponWheel", "altFire", "special", ...), held or pressed, for the device used last; `today` is what the
        /// site showed before NIGHT19 (the answer with design.pad.n19 off).</summary>
        public static string For(string action, bool hold, string today)
        {
            if (!PlayerDevices.N19 || action == null) return today;
            Refresh();
            if (PlayerDevices.Last != InputDevice.Pad)
            {
                if (action != "interact" || !hold) return today; // the sites' own live keys
                if (holdBuyKeys != null) return holdBuyKeys;
                string key = Glyphs.Label(action, InputDevice.KeyboardMouse, PlayerDevices.Glyphs, PlayerDevices.Bindings, null);
                return holdBuyKeys = key != null ? UnityTuning.PromptHoldWord + key : today;
            }
            if (action == "nextWeapon" && IsCycleLine(today)) return cycleLine ??= CycleLine();
            Dictionary<string, string> cache = hold ? padHoldCache : padCache;
            if (cache.TryGetValue(action, out string hit)) return hit ?? (today == null ? null : string.Empty);
            string s = PadText(action, hold);
            cache[action] = s;
            return s ?? (today == null ? null : string.Empty);
        }

        static bool IsCycleLine(string today) =>
            ReferenceEquals(today, UnityTuning.HudPadCycleNeutral) || ReferenceEquals(today, UnityTuning.HudPadCyclePlayStation) || ReferenceEquals(today, UnityTuning.HudPadCycleNintendo)
            || today == UnityTuning.HudPadCycleNeutral || today == UnityTuning.HudPadCyclePlayStation || today == UnityTuning.HudPadCycleNintendo;

        static PadLayout LayoutNow => PlayerDevices.Layout ?? GamepadMath.DefaultLayoutV2;

        /// <summary>The pad's text for an action (null: no pad button carries it).</summary>
        static string PadText(string action, bool hold)
        {
            GlyphSet set = PlayerDevices.Glyphs;
            switch (action)
            {
                case "forward": return Glyph(LeftStick, set, false); // main.js PAD_MOVE: the left stick
                case "back":
                case "left":
                case "right": return string.Empty; // PAD_MOVE: nothing (forward's stick says it)
                case "move": return Glyph(LeftStick, set, false);
                case "look": return Glyph(RightStick, set, false);
                case "radial": // the keyboard's wheel key: on the pad, the button that holds the wheel open (NIGHT19: the WHEEL bug)
                case "weaponWheel":
                {
                    PadButtonName? w = WheelButton();
                    return w.HasValue ? Button(w.Value, set, true) : null;
                }
            }
            int a = GamepadMath.ActionFromName(action);
            if (a < 0) return null; // a keyboard-only action (restart): nothing on the pad
            PadButtonName? b = GamepadMath.ButtonFor((PadAction)a, LayoutNow);
            return b.HasValue ? Button(b.Value, set, hold) : null;
        }

        /// <summary>The wheel's button: one bound to WEAPON WHEEL, else NEXT WEAPON's (held design.pad.wheelHold: PadReader).</summary>
        static PadButtonName? WheelButton()
        {
            PadLayout l = LayoutNow;
            return GamepadMath.ButtonFor(PadAction.Wheel, l) ?? GamepadMath.ButtonFor(PadAction.NextWeapon, l);
        }

        /// <summary>The hotbar's cycle line: "<NEXT WEAPON's glyph> NEXT · <PREVIOUS WEAPON's> BACK" (an unbound half left out).</summary>
        static string CycleLine()
        {
            GlyphSet set = PlayerDevices.Glyphs;
            PadLayout l = LayoutNow;
            PadButtonName? next = GamepadMath.ButtonFor(PadAction.NextWeapon, l), prev = GamepadMath.ButtonFor(PadAction.PrevWeapon, l);
            string s = next.HasValue ? Button(next.Value, set) + UnityTuning.PromptCycleNext : "";
            if (prev.HasValue) s += (s.Length > 0 ? UnityTuning.PromptCycleSep : "") + Button(prev.Value, set) + UnityTuning.PromptCycleBack;
            return s;
        }

        /// <summary>Is `s` a drawn glyph (it holds a sprite tag)? A key chip shows it without its plate.</summary>
        public static bool IsSprite(string s) => s != null && s.IndexOf("<sprite=", System.StringComparison.Ordinal) >= 0;
    }
}
