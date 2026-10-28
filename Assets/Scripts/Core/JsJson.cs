// JSON reading for the web game's exported data (PortExport/), exact to the bit. The exports write every double in
// its shortest round-trip form (JSON.stringify); reading them back needs a correctly rounded decimal parser, and the
// Unity runtime's double.Parse is not one: it misread 1 of 60,000 such numbers in a measured check (so did anything
// built on it, such as Newtonsoft). ParseNumber does the decimal -> binary conversion exactly with BigInteger
// (round half to even, like JS Number()). Values come back as Dictionary<string, object>, List<object>, string,
// double, bool or null.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace Skygrind.Core
{
    public static class JsJson
    {
        public static object Parse(string text)
        {
            int i = 0;
            object v = Value(text, ref i);
            Skip(text, ref i);
            if (i != text.Length) throw new FormatException($"JSON: trailing data at {i}");
            return v;
        }

        /// <summary>A decimal number literal to the nearest double (ties to even), exactly as JS Number() does.</summary>
        public static double ParseNumber(string s)
        {
            int i = 0;
            bool negative = false;
            if (s[i] == '-' || s[i] == '+')
            {
                negative = s[i] == '-';
                i++;
            }
            var digits = new StringBuilder();
            int exp10 = 0;
            bool point = false;
            for (; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= '0' && c <= '9')
                {
                    digits.Append(c);
                    if (point) exp10--;
                }
                else if (c == '.') point = true;
                else if (c == 'e' || c == 'E')
                {
                    exp10 += int.Parse(s.Substring(i + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                    break;
                }
                else throw new FormatException($"number: '{s}'");
            }
            BigInteger mantissa = BigInteger.Parse(digits.ToString(), CultureInfo.InvariantCulture);
            double magnitude;
            if (mantissa.IsZero) magnitude = 0;
            else if (mantissa < TwoPow53 && exp10 >= -22 && exp10 <= 22)
            {
                // Clinger's fast path: both operands exact, so one IEEE multiply / divide is correctly rounded.
                double m = (double)(long)mantissa;
                magnitude = exp10 >= 0 ? m * Pow10[exp10] : m / Pow10[-exp10];
            }
            else magnitude = Exact(mantissa, exp10);
            return negative ? -magnitude : magnitude;
        }

        static readonly BigInteger TwoPow53 = BigInteger.One << 53;

        static readonly double[] Pow10 = BuildPow10(); // 1e0 .. 1e22, all exact doubles

        static double[] BuildPow10()
        {
            var p = new double[23];
            p[0] = 1;
            for (int k = 1; k < p.Length; k++) p[k] = p[k - 1] * 10;
            return p;
        }

        /// <summary>m · 10^e rounded to the nearest double, ties to even (normals and, since NIGHT9, subnormals).</summary>
        static double Exact(BigInteger m, int e)
        {
            BigInteger num = m, den = BigInteger.One;
            if (e >= 0) num *= BigInteger.Pow(10, e);
            else den = BigInteger.Pow(10, -e);
            // Scale so the quotient has exactly 53 significant bits: q = floor(num · 2^-shift / den).
            int shift = BitLength(num) - BitLength(den) - 53;
            BigInteger q, r;
            for (;;)
            {
                BigInteger n = shift >= 0 ? num : num << -shift;
                BigInteger d = shift >= 0 ? den << shift : den;
                q = BigInteger.DivRem(n, d, out r);
                int bits = BitLength(q);
                if (bits > 53) { shift++; continue; }
                if (bits < 53) { shift--; continue; }
                // Round half to even on the remainder.
                int cmp = (r * 2).CompareTo(d);
                if (cmp > 0 || (cmp == 0 && !q.IsEven)) q += 1;
                break;
            }
            if (BitLength(q) == 54)
            {
                q >>= 1;
                shift++;
            }
            int exponent = shift + 52; // value = q · 2^shift, q in [2^52, 2^53)
            if (exponent < -1022)
            {
                // NIGHT9 U3-04 (the jsMath vectors hold subnormals): a subnormal is a multiple of 2^-1074, so round
                // num / den · 2^1074 to an integer, ties to even, straight from the exact fraction (no double rounding);
                // its bits are that integer (2^52 rounds up into the smallest normal on its own).
                BigInteger sub = BigInteger.DivRem(num << 1074, den, out BigInteger rest);
                int c = (rest * 2).CompareTo(den);
                if (c > 0 || (c == 0 && !sub.IsEven)) sub += 1;
                return BitConverter.Int64BitsToDouble((long)sub);
            }
            if (exponent > 1023) throw new OverflowException("number outside the double range");
            long bits64 = ((long)(exponent + 1023) << 52) | ((long)(ulong)q & 0xFFFFFFFFFFFFFL);
            return BitConverter.Int64BitsToDouble(bits64);
        }

        static int BitLength(BigInteger v)
        {
            int n = 0;
            byte[] bytes = v.ToByteArray(); // little endian, two's complement (v >= 0 here)
            int last = bytes.Length - 1;
            while (last > 0 && bytes[last] == 0) last--;
            n = last * 8;
            for (int b = bytes[last]; b != 0; b >>= 1) n++;
            return n;
        }

        // --- a small recursive-descent JSON reader ------------------------------------------------------------------

        static void Skip(string t, ref int i)
        {
            while (i < t.Length && (t[i] == ' ' || t[i] == '\n' || t[i] == '\r' || t[i] == '\t')) i++;
        }

        static object Value(string t, ref int i)
        {
            Skip(t, ref i);
            char c = t[i];
            if (c == '{') return Obj(t, ref i);
            if (c == '[') return Arr(t, ref i);
            if (c == '"') return Str(t, ref i);
            if (string.CompareOrdinal(t, i, "true", 0, 4) == 0) { i += 4; return true; }
            if (string.CompareOrdinal(t, i, "false", 0, 5) == 0) { i += 5; return false; }
            if (string.CompareOrdinal(t, i, "null", 0, 4) == 0) { i += 4; return null; }
            int start = i;
            while (i < t.Length && "+-0123456789.eE".IndexOf(t[i]) >= 0) i++;
            return ParseNumber(t.Substring(start, i - start));
        }

        static Dictionary<string, object> Obj(string t, ref int i)
        {
            var d = new Dictionary<string, object>();
            i++;
            Skip(t, ref i);
            if (t[i] == '}') { i++; return d; }
            for (;;)
            {
                Skip(t, ref i);
                string key = Str(t, ref i);
                Skip(t, ref i);
                i++; // ':'
                d[key] = Value(t, ref i);
                Skip(t, ref i);
                if (t[i++] == '}') return d;
            }
        }

        static List<object> Arr(string t, ref int i)
        {
            var a = new List<object>();
            i++;
            Skip(t, ref i);
            if (t[i] == ']') { i++; return a; }
            for (;;)
            {
                a.Add(Value(t, ref i));
                Skip(t, ref i);
                if (t[i++] == ']') return a;
            }
        }

        static string Str(string t, ref int i)
        {
            var sb = new StringBuilder();
            i++; // opening quote
            for (;;)
            {
                char c = t[i++];
                if (c == '"') return sb.ToString();
                if (c != '\\')
                {
                    sb.Append(c);
                    continue;
                }
                char e = t[i++];
                switch (e)
                {
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case 'r': sb.Append('\r'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u': sb.Append((char)Convert.ToInt32(t.Substring(i, 4), 16)); i += 4; break;
                    default: sb.Append(e); break; // \" \\ \/
                }
            }
        }
    }
}
