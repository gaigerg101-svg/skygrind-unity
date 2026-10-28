// NIGHT55 K brief55 (design.n55.brief55, a VIEW row: NightRows.ViewOn(cfg, "brief55"); -skygrindOff55 or under: NIGHT54's text exactly).
// Slate P1-teachText (critic-ggproxy.md #8; GG NIGHT47: the text "sounds awkward and unnatural"; OWNER w41: "way too much instruction at the
// start of the level"), then P2-brief (the boss brief, later in this file's row). The level text, in the existing styles (NIGHT54 D-4: no restyle):
//   1. THE LINES: every lesson / teach line a whole sentence with its verb and its bound glyph, at most 6 words (a glyph is a word):
//      numbers.teachMap (the OLD template exactly as the config / level has it -> the new one, the same {action} placeholders so the live glyph
//      still fills in), applied ahead of copy47 at Copy47.Map's one hook line. A line the map lacks is copy47's (or the old text).
//   2. THE TIME: the centre card (UiTeach43, hud52's chip) shows one line at most numbers.teachMaxS, then closes and stays closed while the
//      slot keeps that same line (a new line opens it again); a lesson done closes it sooner (the sim's rule, unchanged). UiBrief55.cs.
//   3. NO BASIC JUMP after level 1: a "{jump} to jump" line is not shown outside the campaign's first level. UiBrief55.cs.
//   4. NO TOAST OVER THE COUNTER: the top-left column (the lesson card, the hint, the toasts) starts under the room counter's plate while the
//      counter shows (the critic's "SLIDE BURST: RECHARGING" over "2 LEFT"). UiBrief55.cs.
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Brief55
    {
        public const string Numbers = "design.n55.brief55.numbers.";

        /// <summary>The reels' override of the row (null: the config and the command line).</summary>
        public static bool? Force;
        static GameConfig readFor;
        static bool on;
        static double teachMaxS;
        static Dictionary<string, string> map;

        /// <summary>Lines rewritten this session (the reel prints it).</summary>
        public static int Swapped;

        static void Read(GameConfig cfg)
        {
            if (ReferenceEquals(cfg, readFor)) return;
            readFor = cfg;
            on = NightRows.ViewOn(cfg, "brief55");
            map = new Dictionary<string, string>();
            teachMaxS = cfg.Has(Numbers + "teachMaxS") ? cfg.Num(Numbers + "teachMaxS") : 0;
            if (cfg.Has(Numbers + "teachMap") && cfg.Get(Numbers + "teachMap") is Dictionary<string, object> d)
                foreach (KeyValuePair<string, object> kv in d)
                    if (kv.Value is string s && !kv.Key.EndsWith("_why")) map[kv.Key] = s;
        }

        /// <summary>The row (the reel's Force first).</summary>
        public static bool On(GameConfig cfg)
        {
            if (cfg == null) return false;
            Read(cfg);
            return Force ?? on;
        }

        static int off57 = -1;

        /// <summary>NIGHT57 V (slate V2-brief55): the column clears the counter on the frame it opens (UiBrief55.cs Brief55CounterShows), with the
        /// row on and no -skygrindOff57 or lower (NIGHT56's brief55 laid the column one frame late: a card 2.4 px over the opening plate on kiln and
        /// spire, n56-r5 verbs37 f702).</summary>
        public static bool OpenFix57(GameConfig cfg)
        {
            if (!On(cfg)) return false;
            if (NightRows.ForceOffFrom <= 57) return false;
            if (off57 < 0)
            {
                try { off57 = NightRows.OffFrom(System.Environment.GetCommandLineArgs(), "-skygrindOff") <= 57 ? 1 : 0; }
                catch (System.Exception) { off57 = 0; }
            }
            return off57 == 0;
        }

        /// <summary>The card's longest stand for one line (s; 0: no cap, the row off).</summary>
        public static double TeachMaxS(GameConfig cfg) => On(cfg) ? teachMaxS : 0;

        /// <summary>Copy47.Map's first line: the row's rewrite of template `s` (true), else false (copy47 then decides).</summary>
        public static bool TeachMap(GameConfig cfg, string s, out string t)
        {
            t = null;
            if (string.IsNullOrEmpty(s) || !On(cfg)) return false;
            if (!map.TryGetValue(s, out t)) return false;
            Swapped++;
            return true;
        }

        /// <summary>Pure (N55BriefTests): a shown line (rich tags stripped) is the basic jump's ("B to jump.", "Press SPACE to jump.").</summary>
        public static bool IsBasicJump(string plain)
        {
            if (string.IsNullOrEmpty(plain)) return false;
            string s = plain.Trim().TrimEnd('.', '!').ToLowerInvariant();
            if (s.StartsWith("press ")) s = s.Substring(6);
            int sp = s.IndexOf(' ');
            if (sp <= 0) return false;
            return s.Substring(sp + 1) == "to jump";
        }

        /// <summary>Pure: `s` without its rich-text tags (&lt;color=...&gt;, &lt;sprite ...&gt; become nothing; a sprite's name is kept as a word).</summary>
        public static string Plain(string s)
        {
            if (string.IsNullOrEmpty(s)) return s ?? "";
            var sb = new System.Text.StringBuilder(s.Length);
            int i = 0;
            while (i < s.Length)
            {
                if (s[i] == '<')
                {
                    int e = s.IndexOf('>', i);
                    if (e < 0) { sb.Append(s, i, s.Length - i); break; }
                    string tag = s.Substring(i + 1, e - i - 1);
                    if (tag.StartsWith("sprite")) sb.Append('B'); // a pad glyph: one word
                    i = e + 1;
                    continue;
                }
                sb.Append(s[i++]);
            }
            return sb.ToString();
        }

        /// <summary>Pure: the words of a template (a {placeholder} is one word; punctuation splits nothing).</summary>
        public static int Words(string template)
        {
            if (string.IsNullOrEmpty(template)) return 0;
            int n = 0;
            bool inWord = false;
            foreach (char c in template)
            {
                bool w = !char.IsWhiteSpace(c);
                if (w && !inWord) n++;
                inWord = w;
            }
            return n;
        }

        /// <summary>The campaign's first level (the order's first non-tutorial id) is `mapId`'s, or there is no campaign order to say otherwise.</summary>
        public static bool FirstLevel(GameConfig cfg, string mapId)
        {
            if (cfg == null || string.IsNullOrEmpty(mapId)) return true;
            if (mapId == firstFor) return firstWas;
            firstFor = mapId;
            firstWas = true;
            try
            {
                // filtered only where the level is FOUND in the order past its first level (an id the order lacks keeps every line)
                List<string> order = Curriculum25.Order(cfg, N25Blocks.From(cfg));
                order.Remove("tutorial");
                int at = order.IndexOf(mapId);
                if (at < 0) at = order.IndexOf(NightRows.BaseId(cfg, mapId));
                if (at < 0)
                {
                    // a rebuilt level ("n52-kiln") against the order's older id ("n30-kiln"): the place name after the "nNN-" prefix
                    string place = Place(mapId);
                    for (int i = 0; i < order.Count && at < 0; i++) if (Place(order[i]) == place) at = i;
                }
                firstWas = at <= 0;
            }
            catch (System.Exception) { firstWas = true; }
            return firstWas;
        }

        static string firstFor;
        static bool firstWas = true;

        /// <summary>Pure: "n52-kiln" -> "kiln"; an id without an "n" + digits + "-" prefix as it is.</summary>
        public static string Place(string id)
        {
            if (string.IsNullOrEmpty(id) || id[0] != 'n') return id;
            int i = 1;
            while (i < id.Length && char.IsDigit(id[i])) i++;
            return i > 1 && i < id.Length && id[i] == '-' ? id.Substring(i + 1) : id;
        }
    }
}
