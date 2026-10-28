// NIGHT12 U3-25.9: seeded runs. Seeds is a port of skygrind/src/modes/seeds.js (seed codes: base 36, upper case; parseSeed).
// NIGHT24 remove-daily-weekly: the DAILY (daily.js) and today's seed are gone; this file keeps the seed codes (RETRY SAME SEED,
// the CHALLENGE CODE field, RESULTS' SEED line).
// - parseSeed's String.trim and toUpperCase are JS's, by hand (JS trims U+FEFF but not U+0085, and upper-cases U+00DF to
//   "SS"), so a typed code reads the same as on the web whatever the runtime's culture tables.
using System;
using System.Text;

namespace Skygrind.Core
{
    public static class Seeds
    {
        const long Max = 2147483648L; // seeds are 0..2^31-1 (the game's own range, main.js startSurvival)

        const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>seeds.js seedCode: base 36, upper case, at least 6 characters.</summary>
        public static string SeedCode(uint seed)
        {
            // (seed >>> 0).toString(36).toUpperCase().padStart(6, '0')
            var sb = new StringBuilder(7);
            uint v = seed;
            do
            {
                sb.Insert(0, Digits[(int)(v % 36)]);
                v /= 36;
            } while (v > 0);
            while (sb.Length < 6) sb.Insert(0, '0');
            return sb.ToString();
        }

        /// <summary>seeds.js parseSeed: a typed code back to a seed, or null.</summary>
        public static uint? ParseSeed(string text)
        {
            string clean = JsUpper(JsTrim(text ?? ""));
            if (clean.Length < 1 || clean.Length > 7) return null; // /^[0-9A-Z]{1,7}$/
            long n = 0;
            foreach (char c in clean)
            {
                int d = c >= '0' && c <= '9' ? c - '0' : c >= 'A' && c <= 'Z' ? c - 'A' + 10 : -1;
                if (d < 0) return null;
                n = n * 36 + d; // 7 digits at most: below 36^7 < 2^53 (Number.isSafeInteger holds)
            }
            return n < Max ? (uint)n : (uint?)null;
        }

        /// <summary>JS String.prototype.trim: WhiteSpace and LineTerminator (U+FEFF yes, U+0085 no).</summary>
        static string JsTrim(string s)
        {
            int a = 0, b = s.Length;
            while (a < b && JsSpace(s[a])) a++;
            while (b > a && JsSpace(s[b - 1])) b--;
            return s.Substring(a, b - a);
        }

        static bool JsSpace(char c)
        {
            int u = c; // ECMAScript WhiteSpace (TAB, VT, FF, SP, NBSP, ZWNBSP, Zs) and LineTerminator (LF, CR, LS, PS)
            return u == 0x09 || u == 0x0A || u == 0x0B || u == 0x0C || u == 0x0D || u == 0x20 || u == 0xA0 || u == 0x1680 ||
                (u >= 0x2000 && u <= 0x200A) || u == 0x2028 || u == 0x2029 || u == 0x202F || u == 0x205F || u == 0x3000 || u == 0xFEFF;
        }

        /// <summary>
        /// JS toUpperCase as far as parseSeed can see it: a-z, and the non-ASCII characters whose upper case is made of
        /// [0-9A-Z] only (U+0131 dotless i -> I, U+017F long s -> S, U+00DF sharp s -> SS, the Latin ligatures U+FB00..FB06).
        /// Anything else non-ASCII stays as it is (and is refused).
        /// </summary>
        static string JsUpper(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                int u = c;
                if (u >= 'a' && u <= 'z') sb.Append((char)(u - 32));
                else if (u < 128) sb.Append(c);
                else if (u == 0x0131) sb.Append('I');
                else if (u == 0x017F) sb.Append('S');
                else if (u == 0x00DF) sb.Append("SS");
                else if (u == 0xFB00) sb.Append("FF");
                else if (u == 0xFB01) sb.Append("FI");
                else if (u == 0xFB02) sb.Append("FL");
                else if (u == 0xFB03) sb.Append("FFI");
                else if (u == 0xFB04) sb.Append("FFL");
                else if (u == 0xFB05 || u == 0xFB06) sb.Append("ST");
                else sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
