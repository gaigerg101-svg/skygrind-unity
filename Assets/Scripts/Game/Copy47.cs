// NIGHT47 builder F (design.n47.copy47, a view row; NIGHT47.md G15; GG 2026-10-18: "The text that appears when you pick up the gauntlet or start the
// game also sounds awkward and unnatural, it needs to be rewritten to feel like real game dialogue or clear, human-sounding explanations"). Every
// rewritten line is one entry of design.n47.copy47.numbers.map: the OLD template exactly as the config / level file has it (with its {action}
// placeholders) -> the new one (the same placeholders, so the live glyph or key still fills in: PromptText). Applied where a template is shown,
// before its placeholders resolve: the teach35 card (UiTeach35 / UiTeach43), the lesson widget (lessons and the boss lines through BossLine30:
// UiLesson25.ShowLesson25), and the room title banner (the internal "1.1 " numbers dropped). No string is deleted: off (the row off,
// -skygrindOff47) or a line the map lacks = the old text exactly. The inventory and the rules: Research/2026-10-18-wave47/F-cards.md section 3; the
// lead's picks (DECIDE47 D-11): the gauntlet pickup = variant A, the game start = variant B.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Copy47
    {
        /// <summary>The a647 reel's off / on passes (null: design.n47.copy47).</summary>
        public static bool? Force;
        static GameConfig readFor;
        static bool on;
        static Dictionary<string, string> map;

        /// <summary>Lines swapped this session (the reel prints it).</summary>
        public static int Swapped;

        static void Read(GameConfig cfg)
        {
            if (ReferenceEquals(cfg, readFor)) return;
            readFor = cfg;
            on = NightRows.ViewOn(cfg, "copy47");
            map = new Dictionary<string, string>();
            if (cfg.Has("design.n47.copy47.numbers.map") && cfg.Get("design.n47.copy47.numbers.map") is Dictionary<string, object> d)
                foreach (KeyValuePair<string, object> kv in d)
                    if (kv.Value is string s && !kv.Key.EndsWith("_why")) map[kv.Key] = s;
        }

        public static bool On(GameConfig cfg)
        {
            if (cfg == null) return false;
            Read(cfg);
            return Force ?? on;
        }

        /// <summary>The line to show for template `s`: its rewrite with the row on, else `s` itself.</summary>
        public static string Map(GameConfig cfg, string s)
        {
            if (Brief55.TeachMap(cfg, s, out string b55)) return b55; // NIGHT55 K brief55 (Brief55.cs): whole sentences, verb + glyph, <= 6 words (nothing with the row off)
            if (string.IsNullOrEmpty(s) || !On(cfg)) return s;
            if (map.TryGetValue(s, out string t)) { Swapped++; return t; }
            return s;
        }

        /// <summary>A room title without its internal number ("1.1 INTAKE" -> "INTAKE", "3.2b THE STACK" -> "THE STACK"), row on; else as it is.</summary>
        public static string Title(GameConfig cfg, string name)
        {
            if (string.IsNullOrEmpty(name) || !On(cfg)) return name;
            return StripNumber(name);
        }

        /// <summary>Pure: "1.1 INTAKE" -> "INTAKE"; a name with no leading "digits.digits[letter] " is returned as it is.</summary>
        public static string StripNumber(string name)
        {
            int i = 0;
            while (i < name.Length && char.IsDigit(name[i])) i++;
            if (i == 0 || i >= name.Length || name[i] != '.') return name;
            int j = i + 1, k = j;
            while (k < name.Length && char.IsDigit(name[k])) k++;
            if (k == j) return name;
            if (k < name.Length && char.IsLetter(name[k]) && k + 1 < name.Length && name[k + 1] == ' ') k++;
            if (k >= name.Length || name[k] != ' ') return name;
            string rest = name.Substring(k + 1).Trim();
            return rest.Length > 0 ? rest : name;
        }
    }
}
