// JavaScript (V8) number functions the web code relies on, reproduced bit for bit, so the C# ports compute the same
// doubles as the web game. Measured against V8 (Node 24) on 20,000 inputs: the .NET runtime's own Math.Sin/Cos
// differ in the last bit on ~2.5% of them, Math.Max(0, -0) returns -0 (JS: +0), and a plain sqrt(x²+y²) differs
// from V8's Math.hypot on ~37%. So: fdlibm sin/cos (what V8's base/ieee754.cc uses), V8's hypot (Kahan-summed,
// normalized to the largest argument), and JS max/min (+0 beats -0, NaN wins).
using System;

namespace Skygrind.Core
{
    public static partial class JsMath
    {
        // --- Math.hypot (V8 builtins/math.tq MathHypot) ---------------------------------------------------------

        public static double Hypot(double x, double y)
        {
            double ax = Math.Abs(x), ay = Math.Abs(y);
            if (double.IsInfinity(ax) || double.IsInfinity(ay)) return double.PositiveInfinity;
            if (double.IsNaN(ax) || double.IsNaN(ay)) return double.NaN;
            double max = 0;
            if (ax > max) max = ax;
            if (ay > max) max = ay;
            if (max == 0) return 0;
            double sum = 0, compensation = 0;
            Kahan(ax / max, ref sum, ref compensation);
            Kahan(ay / max, ref sum, ref compensation);
            return Math.Sqrt(sum) * max;
        }

        public static double Hypot(double x, double y, double z)
        {
            double ax = Math.Abs(x), ay = Math.Abs(y), az = Math.Abs(z);
            if (double.IsInfinity(ax) || double.IsInfinity(ay) || double.IsInfinity(az)) return double.PositiveInfinity;
            if (double.IsNaN(ax) || double.IsNaN(ay) || double.IsNaN(az)) return double.NaN;
            double max = 0;
            if (ax > max) max = ax;
            if (ay > max) max = ay;
            if (az > max) max = az;
            if (max == 0) return 0;
            double sum = 0, compensation = 0;
            Kahan(ax / max, ref sum, ref compensation);
            Kahan(ay / max, ref sum, ref compensation);
            Kahan(az / max, ref sum, ref compensation);
            return Math.Sqrt(sum) * max;
        }

        static void Kahan(double n, ref double sum, ref double compensation)
        {
            double summand = n * n - compensation;
            double preliminary = sum + summand;
            compensation = (preliminary - sum) - summand;
            sum = preliminary;
        }

        // --- Math.round, Math.max, Math.min ------------------------------------------------------------------------

        /// <summary>Math.round: halves round toward +infinity (C#'s Math.Round rounds halves to even).</summary>
        public static double Round(double x)
        {
            // ES Number::round: x in [-0.5, 0) rounds to -0 (NIGHT9: floor + 1 gave +0 there).
            if (x < 0 && x >= -0.5) return -0.0;
            double f = Math.Floor(x);
            return x - f >= 0.5 ? f + 1 : f;
        }

        /// <summary>Math.max(a, b): NaN if either is NaN; +0 is larger than -0.</summary>
        public static double Max(double a, double b)
        {
            if (double.IsNaN(a) || double.IsNaN(b)) return double.NaN;
            if (a == b) return a == 0 && IsNegativeZero(a) ? b : a;
            return a > b ? a : b;
        }

        /// <summary>Math.min(a, b): NaN if either is NaN; -0 is smaller than +0.</summary>
        public static double Min(double a, double b)
        {
            if (double.IsNaN(a) || double.IsNaN(b)) return double.NaN;
            if (a == b) return a == 0 && IsNegativeZero(b) ? b : a;
            return a < b ? a : b;
        }

        public static bool IsNegativeZero(double d) => d == 0 && BitConverter.DoubleToInt64Bits(d) < 0;

        // --- Math.sin / Math.cos: fdlibm, as V8's base/ieee754.cc -------------------------------------------------

        static double Bits(ulong u) => BitConverter.Int64BitsToDouble((long)u);

        static int HighWord(double d) => (int)(BitConverter.DoubleToInt64Bits(d) >> 32);

        static readonly double S1 = Bits(0xBFC5555555555549), S2 = Bits(0x3F8111111110F8A6), S3 = Bits(0xBF2A01A019C161D5),
            S4 = Bits(0x3EC71DE357B1FE7D), S5 = Bits(0xBE5AE5E68A2B9CEB), S6 = Bits(0x3DE5D93A5ACFD57C);

        static readonly double C1 = Bits(0x3FA555555555554C), C2 = Bits(0xBF56C16C16C15177), C3 = Bits(0x3EFA01A019CB1590),
            C4 = Bits(0xBE927E4F809C52AD), C5 = Bits(0x3E21EE9EBDB4B1C4), C6 = Bits(0xBDA8FAE9BE8838D4);

        static readonly double InvPio2 = Bits(0x3FE45F306DC9C883),
            Pio2_1 = Bits(0x3FF921FB54400000), Pio2_1t = Bits(0x3DD0B4611A626331),
            Pio2_2 = Bits(0x3DD0B4611A600000), Pio2_2t = Bits(0x3BA3198A2E037073),
            Pio2_3 = Bits(0x3BA3198A2E000000), Pio2_3t = Bits(0x397B839A252049C1);

        // High words of k·π/2 for k = 1..32 (fdlibm npio2_hw).
        static readonly int[] Npio2Hw =
        {
            0x3FF921FB, 0x400921FB, 0x4012D97C, 0x401921FB, 0x401F6A7A, 0x4022D97C, 0x4025FDBB, 0x402921FB,
            0x402C463A, 0x402F6A7A, 0x4031475C, 0x4032D97C, 0x40346B9C, 0x4035FDBB, 0x40378FDB, 0x403921FB,
            0x403AB41B, 0x403C463A, 0x403DD85A, 0x403F6A7A, 0x40407E4C, 0x4041475C, 0x4042106C, 0x4042D97C,
            0x4043A28C, 0x40446B9C, 0x404534AC, 0x4045FDBB, 0x4046C6CB, 0x40478FDB, 0x404858EB, 0x404921FB,
        };

        public static double Sin(double x)
        {
            int ix = HighWord(x) & 0x7FFFFFFF;
            if (ix <= 0x3FE921FB) return KernelSin(x, 0, false); // |x| ~< pi/4
            if (ix >= 0x7FF00000) return x - x; // NaN or Inf
            int n = RemPio2(x, out double y0, out double y1);
            switch (n & 3)
            {
                case 0: return KernelSin(y0, y1, true);
                case 1: return KernelCos(y0, y1);
                case 2: return -KernelSin(y0, y1, true);
                default: return -KernelCos(y0, y1);
            }
        }

        public static double Cos(double x)
        {
            int ix = HighWord(x) & 0x7FFFFFFF;
            if (ix <= 0x3FE921FB) return KernelCos(x, 0);
            if (ix >= 0x7FF00000) return x - x;
            int n = RemPio2(x, out double y0, out double y1);
            switch (n & 3)
            {
                case 0: return KernelCos(y0, y1);
                case 1: return -KernelSin(y0, y1, true);
                case 2: return -KernelCos(y0, y1);
                default: return KernelSin(y0, y1, true);
            }
        }

        static double KernelSin(double x, double y, bool iy)
        {
            int ix = HighWord(x) & 0x7FFFFFFF;
            if (ix < 0x3E400000 && (int)x == 0) return x; // |x| < 2^-27
            double z = x * x;
            double v = z * x;
            double r = S2 + z * (S3 + z * (S4 + z * (S5 + z * S6)));
            if (!iy) return x + v * (S1 + z * r);
            return x - ((z * (0.5 * y - v * r) - y) - v * S1);
        }

        static double KernelCos(double x, double y)
        {
            int ix = HighWord(x) & 0x7FFFFFFF;
            if (ix < 0x3E400000 && (int)x == 0) return 1; // |x| < 2^-27
            double z = x * x;
            double r = z * (C1 + z * (C2 + z * (C3 + z * (C4 + z * (C5 + z * C6)))));
            if (ix < 0x3FD33333) return 1 - (0.5 * z - (z * r - x * y)); // |x| < 0.3
            double qx = ix > 0x3FE90000 ? 0.28125 : Bits((ulong)(uint)(ix - 0x00200000) << 32); // x > 0.78125 ? : x/4
            double iz = 0.5 * z - qx;
            double a = 1 - qx;
            return a - (iz - (z * r - x * y));
        }

        /// <summary>fdlibm __ieee754_rem_pio2 for |x| up to 2^19·π/2 (the game's angles are far smaller).</summary>
        static int RemPio2(double x, out double y0, out double y1)
        {
            int hx = HighWord(x);
            int ix = hx & 0x7FFFFFFF;
            double z;
            if (ix < 0x4002D97C)
            {
                // |x| < 3pi/4: n = ±1
                if (hx > 0)
                {
                    z = x - Pio2_1;
                    if (ix != 0x3FF921FB)
                    {
                        y0 = z - Pio2_1t;
                        y1 = (z - y0) - Pio2_1t;
                    }
                    else
                    {
                        z -= Pio2_2;
                        y0 = z - Pio2_2t;
                        y1 = (z - y0) - Pio2_2t;
                    }
                    return 1;
                }
                z = x + Pio2_1;
                if (ix != 0x3FF921FB)
                {
                    y0 = z + Pio2_1t;
                    y1 = (z - y0) + Pio2_1t;
                }
                else
                {
                    z += Pio2_2;
                    y0 = z + Pio2_2t;
                    y1 = (z - y0) + Pio2_2t;
                }
                return -1;
            }
            if (ix > 0x413921FB) throw new NotSupportedException("JsMath.Sin/Cos: |x| beyond 2^19·π/2 (fdlibm's Payne-Hanek path is not ported)");
            double t = Math.Abs(x);
            int n = (int)(t * InvPio2 + 0.5);
            double fn = n;
            double r = t - fn * Pio2_1;
            double w = fn * Pio2_1t; // 1st round, good to 85 bits
            if (n < 32 && ix != Npio2Hw[n - 1])
            {
                y0 = r - w; // quick check: no cancellation
            }
            else
            {
                int j = ix >> 20;
                y0 = r - w;
                int i = j - ((HighWord(y0) >> 20) & 0x7FF);
                if (i > 16)
                {
                    // 2nd iteration, good to 118 bits
                    t = r;
                    w = fn * Pio2_2;
                    r = t - w;
                    w = fn * Pio2_2t - ((t - r) - w);
                    y0 = r - w;
                    i = j - ((HighWord(y0) >> 20) & 0x7FF);
                    if (i > 49)
                    {
                        // 3rd iteration, 151 bits
                        t = r;
                        w = fn * Pio2_3;
                        r = t - w;
                        w = fn * Pio2_3t - ((t - r) - w);
                        y0 = r - w;
                    }
                }
            }
            y1 = (r - y0) - w;
            if (hx < 0)
            {
                y0 = -y0;
                y1 = -y1;
                return -n;
            }
            return n;
        }
    }
}
