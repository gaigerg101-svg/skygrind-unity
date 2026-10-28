// NIGHT41 builder P3 round 2 (C1): `n41pad glyphs`: every {token} in a design.n40 / design.n41 line (Design/unity-config.json, the _why keys
// skipped) must resolve to a PAD button on a pad. The resolvers (BossLine40 through UiLesson25.LessonGlyph25 -> PromptGlyph -> Glyphs.Label; the
// kiln40 chain through UiTeach35.Teach35Glyph -> Lesson25Aliases or Glyphs.Label) all end in Glyphs.Label(action, PlayerDevices.Last, ...), whose
// pad branch is GamepadMath.ActionFromName + GamepadMath.ButtonFor(action, layout); a token with no pad action or no button in the layout falls
// through to the KEYBOARD binding's name (Glyphs.cs:69-70): a keyboard name on a pad. This checks that branch for every token, on every
// default layout version (1-5). PASS: every token has a button on every layout.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;

public static partial class Harness
{
    static int N41PadGlyphs()
    {
        var root = PortData.Obj(PortData.LoadJson(Path.Combine(MapGenRepo, "Design", "unity-config.json")));
        var design = PortData.Obj(root["design"]);
        var found = new SortedDictionary<string, SortedSet<string>>();
        void Walk(object o, string path)
        {
            if (o is string s)
            {
                foreach (string t in Teach35Text.Tokens(s))
                {
                    if (!found.TryGetValue(t, out var where)) found[t] = where = new SortedSet<string>();
                    string[] parts = path.Split('.');
                    where.Add(string.Join(".", parts, 0, Math.Min(3, parts.Length)));
                }
            }
            else if (o is Dictionary<string, object> d) { foreach (var kv in d) if (!kv.Key.EndsWith("_why") && kv.Key != "navGrid") Walk(kv.Value, path + "." + kv.Key); }
            else if (o is List<object> l) { for (int i = 0; i < l.Count; i++) Walk(l[i], path); }
        }
        foreach (string row in new[] { "n40", "n41" }) if (design.TryGetValue(row, out object r)) Walk(r, row);
        // and every NIGHT40 / NIGHT41 level file's lines (level 2's prompts, the kiln's, the span's, the spire's)
        foreach (string f in Directory.GetFiles(Path.Combine(MapGenRepo, "Design", "levels"), "n4*.json"))
        {
            string id = Path.GetFileNameWithoutExtension(f);
            if (id.StartsWith("n40-") || id.StartsWith("n41-")) Walk(PortData.LoadJson(f), id);
        }
        int fails = 0;
        foreach (var kv in found)
        {
            int a = GamepadMath.ActionFromName(kv.Key);
            var missing = new List<string>();
            for (int v = 1; v <= 5; v++)
                if (a < 0 || !GamepadMath.ButtonFor((PadAction)a, GamepadMath.DefaultForVersion(v)).HasValue) missing.Add("v" + v);
            bool ok = missing.Count == 0;
            if (!ok) fails++;
            Console.WriteLine($"n41pad glyphs {{{kv.Key}}}: {(a < 0 ? "no pad action" : "pad action " + (PadAction)a)}; {(ok ? "a button on layouts v1-v5" : "NO button on " + string.Join(",", missing))}  {(ok ? "PASS" : "FAIL")}  (in {string.Join(", ", kv.Value)})");
        }
        return fails;
    }
}
