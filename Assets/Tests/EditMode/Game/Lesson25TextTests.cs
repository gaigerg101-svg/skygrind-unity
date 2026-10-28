// NIGHT25 (builder F): the lesson line's format. {name} placeholders resolve per device: the keyboard's live binding, the pad's live layout (v5: PUNCH is RB, not a
// stick), {move} / {look} through the alias table; a format with no placeholder is kept. Every line of UnityTuning.Lesson25Lines resolves on both devices with no
// brace left, at most 6 words, and the keyboard text never names a pad button (nor the pad text a key).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Lesson25TextTests
    {
        static GameConfig Cfg => GameConfig.Load(Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")), "config.json"));

        [Test]
        public void Aliases_MoveAndLook_FollowTheDevice()
        {
            Assert.That(Lesson25Text.Resolve("{move} MOVE  {look} LOOK", false, _ => null), Is.EqualTo("WASD MOVE  MOUSE LOOK"));
            Assert.That(Lesson25Text.Resolve("{move} MOVE  {look} LOOK", true, _ => null), Is.EqualTo("L STICK MOVE  R STICK LOOK"));
        }

        [Test]
        public void AnAction_UsesTheGlyphSource_AnUnknownOneIsItsNameInCapitals_AndPlainTextIsKept()
        {
            System.Func<string, string> g = a => a == "jump" ? "SPACE" : null;
            Assert.That(Lesson25Text.Resolve("{jump} OFF THE WALLS", false, g), Is.EqualTo("SPACE OFF THE WALLS"));
            Assert.That(Lesson25Text.Resolve("{nothing} HERE", false, g), Is.EqualTo("NOTHING HERE"));
            Assert.That(Lesson25Text.Resolve("STEP ON THE PAD", false, g), Is.EqualTo("STEP ON THE PAD"));
            Assert.That(Lesson25Text.Resolve("A {open", false, g), Is.EqualTo("A {open"));
        }

        [Test]
        public void EveryLessonLine_ResolvesOnBothDevices_FromTheLiveBindingsAndThePadLayoutV5()
        {
            GameConfig cfg = Cfg;
            var bindings = new Bindings(cfg);
            PadLayout v5 = GamepadMath.DefaultForVersion(5);
            System.Func<string, string> kb = a => Glyphs.Label(a, InputDevice.KeyboardMouse, GlyphSet.Xbox, bindings, v5);
            System.Func<string, string> pad = a => Glyphs.Label(a, InputDevice.Pad, GlyphSet.Xbox, bindings, v5);
            string[] padWords = { "L STICK", "R STICK", "LB", "RB", "LT", "RT" };
            foreach (var kv in UnityTuning.Lesson25Lines)
            {
                string k = Lesson25Text.Resolve(kv.Value, false, kb), p = Lesson25Text.Resolve(kv.Value, true, pad);
                Assert.That(k, Does.Not.Contain("{"), kv.Key + " keyboard");
                Assert.That(p, Does.Not.Contain("{"), kv.Key + " pad");
                int words = System.Text.RegularExpressions.Regex.Replace(kv.Value, "\\{[a-zA-Z]+\\}", "X").Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.That(words, Is.LessThanOrEqualTo(6), kv.Key + " words");
                foreach (string w in padWords) Assert.That(k, Does.Not.Contain(w + " "), $"{kv.Key} keyboard text '{k}' names the pad's {w}");
            }
        }

        [Test]
        public void ThePadsPunch_IsWhateverTheLayoutBinds_NotALiteral()
        {
            PadLayout v5 = GamepadMath.DefaultForVersion(5);
            PadButtonName? b = GamepadMath.ButtonFor(PadAction.Punch, v5);
            Assert.That(b.HasValue, Is.True);
            string want = Glyphs.Button(b.Value, GlyphSet.Xbox);
            string got = Lesson25Text.Resolve("{punch} PUNCH WHILE FIRING", true, a => Glyphs.Label(a, InputDevice.Pad, GlyphSet.Xbox, null, v5));
            Assert.That(got, Is.EqualTo(want + " PUNCH WHILE FIRING"));
            Assert.That(got, Does.Not.Contain("R STICK"));
        }

        static void Prompts(object node, List<string> into)
        {
            if (node is Dictionary<string, object> d)
            {
                foreach (var kv in d)
                {
                    if ((kv.Key == "prompt" || kv.Key == "promptPad") && kv.Value is string str && str.Length > 0) into.Add(str);
                    else Prompts(kv.Value, into);
                }
            }
            else if (node is List<object> l) foreach (object o in l) Prompts(o, into);
        }

        /// <summary>The PAD PASS (NIGHT25): every prompt of every level file, resolved on the keyboard (the config's bindings) and on the pad (layout v5), names a real,
        /// bound input: each placeholder is an alias or an action the pad layout and the keyboard bindings both hold (fire / altFire: the mouse), no brace is left, no literal
        /// button or key name is typed in the text, and a prompt is at most 6 words.</summary>
        [Test]
        public void EveryLevelPrompt_NamesARealBoundInput_OnTheKeyboardAndOnTheV5Pad()
        {
            GameConfig cfg = Cfg;
            var bindings = new Bindings(cfg);
            PadLayout v5 = GamepadMath.DefaultForVersion(5);
            string dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Design", "levels"));
            var all = new List<string>();
            foreach (string f in Directory.GetFiles(dir, "*.json"))
            {
                if (f.EndsWith(".plan.json")) continue;
                Prompts(PortData.LoadJson(f), all);
            }
            Assert.That(all.Count, Is.GreaterThan(5), "the level files' prompts were found");
            string[] literal = { "L STICK", "R STICK", " LB ", " RB ", " LT ", " RT ", "SPACE", "SHIFT", "CTRL", "WASD", "MOUSE", "CLICK" };
            var bad = new List<string>();
            foreach (string raw in new HashSet<string>(all))
            {
                string padded = " " + raw + " ";
                foreach (string lit in literal) if (padded.Contains(lit)) bad.Add($"'{raw}' types '{lit.Trim()}' (use a placeholder)");
                foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(raw, "\\{([a-zA-Z]+)\\}"))
                {
                    string a = m.Groups[1].Value;
                    if (UnityTuning.Lesson25Aliases.ContainsKey(a)) continue;
                    int act = GamepadMath.ActionFromName(a);
                    if (act < 0 || !GamepadMath.ButtonFor((PadAction)act, v5).HasValue) bad.Add($"'{raw}': {{{a}}} is not bound on the v5 pad layout");
                    if (bindings.Code(a) == null) bad.Add($"'{raw}': {{{a}}} has no keyboard binding");
                }
                string k = Lesson25Text.Resolve(raw, false, a => Glyphs.Label(a, InputDevice.KeyboardMouse, GlyphSet.Xbox, bindings, v5));
                string p = Lesson25Text.Resolve(raw, true, a => Glyphs.Label(a, InputDevice.Pad, GlyphSet.Xbox, bindings, v5));
                if (k.Contains("{") || p.Contains("{")) bad.Add($"'{raw}' left a brace: '{k}' / '{p}'");
                if (k.IndexOf("FIRE TO FIRE", System.StringComparison.Ordinal) >= 0) bad.Add($"'{raw}' reads '{k}' on the keyboard");
                int words = System.Text.RegularExpressions.Regex.Replace(raw, "\\{[a-zA-Z]+\\}", "X").Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
                if (words > 6) bad.Add($"'{raw}' is {words} words");
            }
            Assert.That(bad, Is.Empty, string.Join("\n", bad));
        }

        [Test]
        public void TheKeyboardsFire_IsTheMouse_NotTheWordFire()
        {
            Assert.That(Lesson25Text.Resolve("{fire} TO FIRE", false, a => a.ToUpperInvariant()), Is.EqualTo("LEFT CLICK TO FIRE"));
            Assert.That(Lesson25Text.Resolve("{fire} TO FIRE", true, a => a == "fire" ? "RT" : null), Is.EqualTo("RT TO FIRE"), "the pad reads its layout");
        }
    }
}
