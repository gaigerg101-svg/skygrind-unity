// NIGHT15 U3-28.4 (builder P): JSON text exactly as the web's exporter writes it: JSON.stringify(enc(value)) (V8; port-export
// scripts/lib.mjs writeJson, no indentation, a trailing "\n" added by the caller). JsJson only parses; this is its writer
// for the values JsJson returns (Dictionary<string, object> in key order, List<object>, string, double, bool, null).
// - Numbers print as V8's Number::toString: the shortest digits that read back to the same double, the closest such
//   digits to it, ties to the even digit (V8's bignum-dtoa, which Grisu3 agrees with whenever it succeeds), then
//   ECMA-262's layout (integers without ".0", "1e-7", "1e+21", "0.000001"). The digits come from an exact BigInteger
//   free-format conversion (Steele & White / Burger & Dybvig), never from the runtime's double.ToString: Mono's "R" can
//   print 17 digits where V8 prints 16.
// - Num(v) is lib.mjs enc() for a number: -0, NaN and the infinities become {"$num": ...}; JSON.stringify itself writes
//   -0 as 0 and non-finite numbers as null (so does Write, for a raw double).
// - Strings are quoted as V8's JSON.stringify quotes them (\" \\ \b \f \n \r \t, other controls and lone surrogates as
//   \u00xx / \udxxx in lower-case hex, everything else literal).
// No UnityEngine: it runs in the Editor, the tests and a plain dotnet console alike.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapJsonWriter
    {
        readonly StringBuilder sb;
        readonly Dictionary<long, string> numberCache = new Dictionary<long, string>(); // a map repeats its non-integers

        MapJsonWriter(StringBuilder sb) => this.sb = sb;

        /// <summary>JSON.stringify(value) (no trailing newline).</summary>
        public static string Write(object value)
        {
            var sb = new StringBuilder();
            new MapJsonWriter(sb).Value(value);
            return sb.ToString();
        }

        /// <summary>The map file's text: JSON.stringify(value) + "\n", as lib.mjs writeJson writes it.</summary>
        public static string WriteFile(object value) => Write(value) + "\n";

        /// <summary>lib.mjs enc() for a number: -0, NaN, Infinity and -Infinity as {"$num": "..."}; any other double as itself.</summary>
        public static object Num(double v)
        {
            if (v == 0 && BitConverter.DoubleToInt64Bits(v) != 0) return NumWrapper("-0");
            if (double.IsNaN(v)) return NumWrapper("NaN");
            if (double.IsPositiveInfinity(v)) return NumWrapper("Infinity");
            if (double.IsNegativeInfinity(v)) return NumWrapper("-Infinity");
            return v;
        }

        static Dictionary<string, object> NumWrapper(string s) => new Dictionary<string, object> { ["$num"] = s };

        /// <summary>lib.mjs enc(undefined): {"$undefined": true}.</summary>
        public static Dictionary<string, object> Undefined() => new Dictionary<string, object> { ["$undefined"] = true };

        /// <summary>True for enc()'s {"$undefined": true}.</summary>
        public static bool IsUndefined(object o) => o is Dictionary<string, object> d && d.Count == 1 && d.TryGetValue("$undefined", out object u) && u is bool b && b;

        /// <summary>V8's Number.prototype.toString() (radix 10) for a double.</summary>
        public static string NumberText(double v) => new MapJsonWriter(null).Number(v);

        void Value(object o)
        {
            switch (o)
            {
                case null:
                    sb.Append("null");
                    return;
                case bool b:
                    sb.Append(b ? "true" : "false");
                    return;
                case string s:
                    Quote(sb, s);
                    return;
                case double d:
                    sb.Append(double.IsNaN(d) || double.IsInfinity(d) ? "null" : Number(d));
                    return;
                case float f:
                    Value((double)f);
                    return;
                case int i:
                    sb.Append(i.ToString(CultureInfo.InvariantCulture));
                    return;
                case long l:
                    Value((double)l);
                    return;
                case Dictionary<string, object> dict:
                {
                    sb.Append('{');
                    bool first = true;
                    foreach (KeyValuePair<string, object> kv in dict)
                    {
                        if (!first) sb.Append(',');
                        first = false;
                        Quote(sb, kv.Key);
                        sb.Append(':');
                        Value(kv.Value);
                    }
                    sb.Append('}');
                    return;
                }
                case List<object> list:
                {
                    sb.Append('[');
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        Value(list[i]);
                    }
                    sb.Append(']');
                    return;
                }
                case double[] arr:
                {
                    sb.Append('[');
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (i > 0) sb.Append(',');
                        Value(arr[i]);
                    }
                    sb.Append(']');
                    return;
                }
                default:
                    throw new ArgumentException($"MapJsonWriter: no JSON form for a {o.GetType().Name}");
            }
        }

        // --- strings ------------------------------------------------------------------------------------------------

        static void Quote(StringBuilder sb, string s)
        {
            sb.Append('"');
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '"': sb.Append("\\\""); continue;
                    case '\\': sb.Append("\\\\"); continue;
                    case '\b': sb.Append("\\b"); continue;
                    case '\f': sb.Append("\\f"); continue;
                    case '\n': sb.Append("\\n"); continue;
                    case '\r': sb.Append("\\r"); continue;
                    case '\t': sb.Append("\\t"); continue;
                }
                if (c < 0x20)
                {
                    Hex4(sb, c);
                    continue;
                }
                if (char.IsHighSurrogate(c))
                {
                    if (i + 1 < s.Length && char.IsLowSurrogate(s[i + 1]))
                    {
                        sb.Append(c).Append(s[i + 1]);
                        i++;
                    }
                    else Hex4(sb, c); // well-formed JSON.stringify (ES2019): a lone surrogate is escaped
                    continue;
                }
                if (char.IsLowSurrogate(c))
                {
                    Hex4(sb, c);
                    continue;
                }
                sb.Append(c);
            }
            sb.Append('"');
        }

        static void Hex4(StringBuilder sb, char c) => sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));

        // --- numbers ------------------------------------------------------------------------------------------------

        static readonly double TwoPow53 = 9007199254740992.0;

        string Number(double v)
        {
            if (v == 0) return "0"; // +0 and -0 alike (JSON.stringify(-0) is "0")
            if (Math.Abs(v) < TwoPow53 && v == Math.Floor(v)) return ((long)v).ToString(CultureInfo.InvariantCulture);
            long bits = BitConverter.DoubleToInt64Bits(v);
            if (numberCache != null && sb != null && numberCache.TryGetValue(bits, out string cached)) return cached;
            string text = v < 0 ? "-" + Layout(Math.Abs(v)) : Layout(v);
            if (sb != null) numberCache[bits] = text;
            return text;
        }

        /// <summary>ECMA-262 Number::toString for v > 0: the shortest digits d1..dk with value 0.d1..dk x 10^n.</summary>
        static string Layout(double v)
        {
            Shortest(v, out string digits, out int n);
            int k = digits.Length;
            if (k <= n && n <= 21) return digits + new string('0', n - k);
            if (0 < n && n <= 21) return digits.Substring(0, n) + "." + digits.Substring(n);
            if (-6 < n && n <= 0) return "0." + new string('0', -n) + digits;
            int e = n - 1;
            string exp = (e < 0 ? "-" : "+") + Math.Abs(e).ToString(CultureInfo.InvariantCulture);
            return k == 1 ? digits + "e" + exp : digits.Substring(0, 1) + "." + digits.Substring(1) + "e" + exp;
        }

        /// <summary>
        /// The shortest decimal digits that read back (round half to even) to v > 0, the closest of them to v, a tie to
        /// the even digit (V8's bignum-dtoa BIGNUM_DTOA_SHORTEST); value = 0.digits x 10^n. Exact (BigInteger).
        /// </summary>
        static void Shortest(double v, out string digits, out int n)
        {
            long bits = BitConverter.DoubleToInt64Bits(v);
            int biased = (int)((bits >> 52) & 0x7FF);
            long fraction = bits & 0xFFFFFFFFFFFFFL;
            long f;
            int e;
            if (biased == 0)
            {
                f = fraction;
                e = -1074;
            }
            else
            {
                f = fraction | (1L << 52);
                e = biased - 1075;
            }
            bool even = (f & 1) == 0; // a boundary reads back to an even significand, so it may be printed
            bool lowerCloser = fraction == 0 && biased > 1; // a power of two: the gap below is half the gap above
            // v = r / s; the upper boundary (v + next) / 2 = (r + mPlus) / s, the lower (prev + v) / 2 = (r - mMinus) / s.
            BigInteger r, s, mPlus, mMinus;
            if (e >= 0)
            {
                BigInteger be = BigInteger.One << e;
                if (!lowerCloser)
                {
                    r = new BigInteger(f) * be * 2;
                    s = 2;
                    mPlus = be;
                    mMinus = be;
                }
                else
                {
                    r = new BigInteger(f) * be * 4;
                    s = 4;
                    mPlus = be * 2;
                    mMinus = be;
                }
            }
            else
            {
                if (!lowerCloser)
                {
                    r = new BigInteger(f) * 2;
                    s = BigInteger.One << (1 - e);
                    mPlus = 1;
                    mMinus = 1;
                }
                else
                {
                    r = new BigInteger(f) * 4;
                    s = BigInteger.One << (2 - e);
                    mPlus = 2;
                    mMinus = 1;
                }
            }
            // The decimal exponent: the smallest k with high <= 10^k (high < 10^k when the boundary may not be printed).
            int k = (int)Math.Ceiling(Math.Log10(v));
            if (k >= 0) s *= BigInteger.Pow(10, k);
            else
            {
                BigInteger scale = BigInteger.Pow(10, -k);
                r *= scale;
                mPlus *= scale;
                mMinus *= scale;
            }
            for (;;) // too low: high reaches 10^k
            {
                int c = (r + mPlus).CompareTo(s);
                if (even ? c >= 0 : c > 0)
                {
                    s *= 10;
                    k++;
                }
                else break;
            }
            for (;;) // too high: high also fits under 10^(k-1)
            {
                int c = ((r + mPlus) * 10).CompareTo(s);
                if (even ? c < 0 : c <= 0)
                {
                    r *= 10;
                    mPlus *= 10;
                    mMinus *= 10;
                    k--;
                }
                else break;
            }
            var buf = new StringBuilder(17);
            for (;;)
            {
                r *= 10;
                mPlus *= 10;
                mMinus *= 10;
                BigInteger d = BigInteger.DivRem(r, s, out BigInteger rem);
                r = rem;
                int digit = (int)d;
                int lowCmp = r.CompareTo(mMinus);
                int highCmp = (r + mPlus).CompareTo(s);
                bool low = even ? lowCmp <= 0 : lowCmp < 0; // rounding down stays inside the interval
                bool high = even ? highCmp >= 0 : highCmp > 0; // rounding up stays inside the interval
                if (!low && !high)
                {
                    buf.Append((char)('0' + digit));
                    continue;
                }
                if (low && high)
                {
                    int half = (r * 2).CompareTo(s);
                    if (half > 0 || (half == 0 && digit % 2 == 1)) digit++;
                }
                else if (high) digit++;
                buf.Append((char)('0' + digit));
                break;
            }
            digits = buf.ToString();
            n = k;
        }
    }
}
