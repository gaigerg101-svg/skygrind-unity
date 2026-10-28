// JavaScript's Math.atan2, exp, tan, acos, asin, atan, tanh and pow as V8 13.6 (Node 24.19.0) computes them, so the
// C# sim gets the web's doubles on every OS (NIGHT9 U3-04). System.Math goes to the C runtime, which differs from V8
// in the last bit on some inputs and differs between operating systems.
// Sources (V8 tag 13.6.233.17):
// - src/base/ieee754.cc: atan2, atan, exp, tan (__kernel_tan, __ieee754_rem_pio2, __kernel_rem_pio2), acos, asin,
//   tanh (expm1); fdlibm 5.3, "modified significantly by Google". builtins/math.tq calls Float64Atan2 / Exp / Tan /
//   Acos / Asin / Atan / Tanh, which src/codegen/external-reference.cc binds to base::ieee754::atan2 ... tanh.
//   V8_USE_LIBM_TRIG_FUNCTIONS only swaps sin / cos (JsMath.cs), none of these.
// - Math.pow: builtins/math.tq MathPow -> Float64Pow -> ieee754_pow_function = src/numbers/ieee754.cc math::pow. With
//   the default --use-std-math-pow (src/flags/flag-definitions.h: DEFINE_BOOL(use_std_math_pow, true, ...)) that is
//   the ECMAScript special cases, y == 2 -> x * x, y == 0.5 -> sqrt(x + 0), then std::pow: the C runtime's pow, not
//   fdlibm (Node on Windows: the UCRT's). Pow below ports that wrapper and, like V8, hands the rest to the C runtime
//   (System.Math.Pow is the CRT's pow on .NET, Mono and IL2CPP). PowFdlibm is V8's fdlibm pow
//   (base::ieee754::legacy::pow, what --no-use-std-math-pow runs): OS-independent, but not what V8 13.6 returns.
// Line for line with V8's code and comments; the decimal constants are V8's, each with its hex pattern.
// The original fdlibm notice (src/base/ieee754.cc):
// ====================================================
// Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved.
//
// Developed at SunSoft, a Sun Microsystems, Inc. business.
// Permission to use, copy, modify, and distribute this
// software is freely granted, provided that this notice
// is preserved.
// ====================================================
using System;

namespace Skygrind.Core
{
    public static partial class JsMath
    {
        // --- word access (ieee754.cc's EXTRACT_WORDS / GET_LOW_WORD / INSERT_WORDS / SET_HIGH_WORD / SET_LOW_WORD) ---

        static uint FdLowWord(double d) => unchecked((uint)BitConverter.DoubleToInt64Bits(d));

        static double FdInsertWords(uint hi, uint lo) => BitConverter.Int64BitsToDouble(unchecked((long)(((ulong)hi << 32) | lo)));

        static double FdSetHighWord(double d, int hi) =>
            BitConverter.Int64BitsToDouble(unchecked((long)(((ulong)(uint)hi << 32) | (ulong)(uint)BitConverter.DoubleToInt64Bits(d))));

        static double FdSetLowWord(double d, uint lo) =>
            BitConverter.Int64BitsToDouble(unchecked((long)(((ulong)BitConverter.DoubleToInt64Bits(d) & 0xFFFFFFFF00000000UL) | lo)));

        /// <summary>
        /// scalbn(x, n) = x * 2^n, rounded once (fdlibm s_scalbn.c; ieee754.cc calls the C library's, which IEEE 754
        /// makes exact, so any correct version returns the same double).
        /// </summary>
        static double FdScalbn(double x, int n)
        {
            const double two54 = 1.80143985094819840000e+16, /* 0x43500000, 0x00000000 */
                twom54 = 5.55111512312578270212e-17, /* 0x3C900000, 0x00000000 */
                huge = 1.0e+300,
                tiny = 1.0e-300;
            int k, hx;
            uint lx;
            hx = HighWord(x);
            lx = FdLowWord(x);
            k = (hx & 0x7ff00000) >> 20; /* extract exponent */
            if (k == 0)
            { /* 0 or subnormal x */
                if ((lx | (uint)(hx & 0x7fffffff)) == 0) return x; /* +-0 */
                x *= two54;
                hx = HighWord(x);
                k = ((hx & 0x7ff00000) >> 20) - 54;
                if (n < -50000) return tiny * x; /*underflow*/
            }
            if (k == 0x7ff) return x + x; /* NaN or Inf */
            k = k + n;
            if (k > 0x7fe) return huge * CopySign(huge, x); /* overflow  */
            if (k > 0) /* normal result */
                return FdSetHighWord(x, unchecked((int)((uint)hx & 0x800fffffu)) | (k << 20));
            if (k <= -54)
            {
                if (n > 50000) /* in case integer overflow in n+k */
                    return huge * CopySign(huge, x); /*overflow*/
                else return tiny * CopySign(tiny, x); /*underflow*/
            }
            k += 54; /* subnormal result */
            x = FdSetHighWord(x, unchecked((int)((uint)hx & 0x800fffffu)) | (k << 20));
            return x * twom54;
        }

        static double CopySign(double magnitude, double sign) =>
            BitConverter.DoubleToInt64Bits(sign) < 0 ? -Math.Abs(magnitude) : Math.Abs(magnitude);

        // --- __ieee754_rem_pio2 for every |x| (JsMath.cs RemPio2 up to 2^19*pi/2, __kernel_rem_pio2 beyond) ---------

        /*
         * Table of constants for 2/pi, 396 Hex digits (476 decimal) of 2/pi
         */
        static readonly int[] TwoOverPi =
        {
            0xA2F983, 0x6E4E44, 0x1529FC, 0x2757D1, 0xF534DD, 0xC0DB62, 0x95993C,
            0x439041, 0xFE5163, 0xABDEBB, 0xC561B7, 0x246E3A, 0x424DD2, 0xE00649,
            0x2EEA09, 0xD1921C, 0xFE1DEB, 0x1CB129, 0xA73EE8, 0x8235F5, 0x2EBB44,
            0x84E99C, 0x7026B4, 0x5F7E41, 0x3991D6, 0x398353, 0x39F49C, 0x845F8B,
            0xBDF928, 0x3B1FF8, 0x97FFDE, 0x05980F, 0xEF2F11, 0x8B5A0A, 0x6D1F6D,
            0x367ECF, 0x27CB09, 0xB74F46, 0x3F669E, 0x5FEA2D, 0x7527BA, 0xC7EBE5,
            0xF17B3D, 0x0739F7, 0x8A5292, 0xEA6BFB, 0x5FB11F, 0x8D5D08, 0x560330,
            0x46FC7B, 0x6BABF0, 0xCFBC20, 0x9AF436, 0x1DA9E3, 0x91615E, 0xE61B08,
            0x659985, 0x5F14A0, 0x68408D, 0xFFD880, 0x4D7327, 0x310606, 0x1556CA,
            0x73A8C9, 0x60E27B, 0xC08C6B,
        };

        /// <summary>
        /// __ieee754_rem_pio2(x, y) for |x| above pi/4 (the callers handle |x| ~&lt;= pi/4 and Inf / NaN first): the
        /// remainder of x rem pi/2 in y0 + y1, and n. Up to 2^19*(pi/2) it is JsMath.cs RemPio2 (the same lines);
        /// beyond, V8's "all other (large) arguments" branch with __kernel_rem_pio2.
        /// </summary>
        static int RemPio2Any(double x, out double y0, out double y1)
        {
            const double zero = 0.00000000000000000000e+00, /* 0x00000000, 0x00000000 */
                two24 = 1.67772160000000000000e+07; /* 0x41700000, 0x00000000 */
            int hx = HighWord(x); /* high word of x */
            int ix = hx & 0x7FFFFFFF;
            if (ix <= 0x413921FB) return RemPio2(x, out y0, out y1); /* |x| ~<= 2^19*(pi/2), medium size */
            /*
             * all other (large) arguments
             */
            if (ix >= 0x7FF00000)
            { /* x is inf or NaN */
                y0 = y1 = x - x;
                return 0;
            }
            double z = 0;
            Span<double> tx = stackalloc double[3];
            Span<double> y = stackalloc double[2];
            int e0, i, nx, n;
            /* set z = scalbn(|x|,ilogb(x)-23) */
            uint low = FdLowWord(x);
            z = FdSetLowWord(z, low);
            e0 = (ix >> 20) - 1046; /* e0 = ilogb(z)-23; */
            z = FdSetHighWord(z, ix - unchecked((int)((uint)e0 << 20)));
            for (i = 0; i < 2; i++)
            {
                tx[i] = (double)(int)z;
                z = (z - tx[i]) * two24;
            }
            tx[2] = z;
            nx = 3;
            while (tx[nx - 1] == zero) nx--; /* skip zero term */
            n = KernelRemPio2(tx, y, e0, nx, 2, TwoOverPi);
            if (hx < 0)
            {
                y0 = -y[0];
                y1 = -y[1];
                return -n;
            }
            y0 = y[0];
            y1 = y[1];
            return n;
        }

        static readonly int[] InitJk = { 2, 3, 4, 6 }; /* initial value for jk */

        static readonly double[] PIo2 =
        {
            1.57079625129699707031e+00, /* 0x3FF921FB, 0x40000000 */
            7.54978941586159635335e-08, /* 0x3E74442D, 0x00000000 */
            5.39030252995776476554e-15, /* 0x3CF84698, 0x80000000 */
            3.28200341580791294123e-22, /* 0x3B78CC51, 0x60000000 */
            1.27065575308067607349e-29, /* 0x39F01B83, 0x80000000 */
            1.22933308981111328932e-36, /* 0x387A2520, 0x40000000 */
            2.73370053816464559624e-44, /* 0x36E38222, 0x80000000 */
            2.16741683877804819444e-51, /* 0x3569F31D, 0x00000000 */
        };

        /* __kernel_rem_pio2(x,y,e0,nx,prec,ipio2)
         * double x[],y[]; int e0,nx,prec; int ipio2[];
         *
         * __kernel_rem_pio2 return the last three digits of N with
         *              y = x - N*pi/2
         * so that |y| < pi/2.
         *
         * The method is to compute the integer (mod 8) and fraction parts of
         * (2/pi)*x without doing the full multiplication. In general we
         * skip the part of the product that are known to be a huge integer (
         * more accurately, = 0 mod 8 ). Thus the number of operations are
         * independent of the exponent of the input.
         *
         * (2/pi) is represented by an array of 24-bit integers in ipio2[].
         *
         * Input parameters:
         *      x[]     The input value (must be positive) is broken into nx
         *              pieces of 24-bit integers in double precision format.
         *              x[i] will be the i-th 24 bit of x. The scaled exponent
         *              of x[0] is given in input parameter e0 (i.e., x[0]*2^e0
         *              match x's up to 24 bits.
         *
         *              Example of breaking a double positive z into x[0]+x[1]+x[2]:
         *                      e0 = ilogb(z)-23
         *                      z  = scalbn(z,-e0)
         *              for i = 0,1,2
         *                      x[i] = floor(z)
         *                      z    = (z-x[i])*2**24
         *
         *
         *      y[]     output result in an array of double precision numbers.
         *              The dimension of y[] is:
         *                      24-bit  precision       1
         *                      53-bit  precision       2
         *                      64-bit  precision       2
         *                      113-bit precision       3
         *              The actual value is the sum of them. Thus for 113-bit
         *              precison, one may have to do something like:
         *
         *              long double t,w,r_head, r_tail;
         *              t = (long double)y[2] + (long double)y[1];
         *              w = (long double)y[0];
         *              r_head = t+w;
         *              r_tail = w - (r_head - t);
         *
         *      e0      The exponent of x[0]
         *
         *      nx      dimension of x[]
         *
         *      prec    an integer indicating the precision:
         *                      0       24  bits (single)
         *                      1       53  bits (double)
         *                      2       64  bits (extended)
         *                      3       113 bits (quad)
         *
         *      ipio2[]
         *              integer array, contains the (24*i)-th to (24*i+23)-th
         *              bit of 2/pi after binary point. The corresponding
         *              floating value is
         *
         *                      ipio2[i] * 2^(-24(i+1)).
         *
         * External function:
         *      double scalbn(), floor();
         *
         *
         * Here is the description of some local variables:
         *
         *      jk      jk+1 is the initial number of terms of ipio2[] needed
         *              in the computation. The recommended value is 2,3,4,
         *              6 for single, double, extended,and quad.
         *
         *      jz      local integer variable indicating the number of
         *              terms of ipio2[] used.
         *
         *      jx      nx - 1
         *
         *      jv      index for pointing to the suitable ipio2[] for the
         *              computation. In general, we want
         *                      ( 2^e0*x[0] * ipio2[jv-1]*2^(-24jv) )/8
         *              is an integer. Thus
         *                      e0-3-24*jv >= 0 or (e0-3)/24 >= jv
         *              Hence jv = max(0,(e0-3)/24).
         *
         *      jp      jp+1 is the number of terms in PIo2[] needed, jp = jk.
         *
         *      q[]     double array with integral value, representing the
         *              24-bits chunk of the product of x and 2/pi.
         *
         *      q0      the corresponding exponent of q[0]. Note that the
         *              exponent for q[i] would be q0-24*i.
         *
         *      PIo2[]  double precision array, obtained by cutting pi/2
         *              into 24 bits chunks.
         *
         *      f[]     ipio2[] in floating point
         *
         *      iq[]    integer array by breaking up q[] in 24-bits chunk.
         *
         *      fq[]    final product of x*(2/pi) in fq[0],..,fq[jk]
         *
         *      ih      integer. If >0 it indicates q[] is >= 0.5, hence
         *              it also indicates the *sign* of the result.
         *
         */
        static int KernelRemPio2(Span<double> x, Span<double> y, int e0, int nx, int prec, int[] ipio2)
        {
            /* Constants:
             * The hexadecimal values are the intended ones for the following
             * constants. The decimal values may be used, provided that the
             * compiler will convert from decimal to binary accurately enough
             * to produce the hexadecimal values shown.
             */
            const double zero = 0.0,
                one = 1.0,
                two24 = 1.67772160000000000000e+07, /* 0x41700000, 0x00000000 */
                twon24 = 5.96046447753906250000e-08; /* 0x3E700000, 0x00000000 */

            int jz, jx, jv, jp, jk, carry, n, i, j, k, m, q0, ih;
            double z, fw;
            Span<int> iq = stackalloc int[20];
            Span<double> f = stackalloc double[20];
            Span<double> fq = stackalloc double[20];
            Span<double> q = stackalloc double[20];

            /* initialize jk*/
            jk = InitJk[prec];
            jp = jk;

            /* determine jx,jv,q0, note that 3>q0 */
            jx = nx - 1;
            jv = (e0 - 3) / 24;
            if (jv < 0) jv = 0;
            q0 = e0 - 24 * (jv + 1);

            /* set up f[0] to f[jx+jk] where f[jx+jk] = ipio2[jv+jk] */
            j = jv - jx;
            m = jx + jk;
            for (i = 0; i <= m; i++, j++)
            {
                f[i] = (j < 0) ? zero : (double)ipio2[j];
            }

            /* compute q[0],q[1],...q[jk] */
            for (i = 0; i <= jk; i++)
            {
                for (j = 0, fw = 0.0; j <= jx; j++) fw += x[j] * f[jx + i - j];
                q[i] = fw;
            }

            jz = jk;
            recompute:
            /* distill q[] into iq[] reversingly */
            for (i = 0, j = jz, z = q[jz]; j > 0; i++, j--)
            {
                fw = (double)(int)(twon24 * z);
                iq[i] = (int)(z - two24 * fw);
                z = q[j - 1] + fw;
            }

            /* compute n */
            z = FdScalbn(z, q0); /* actual value of z */
            z -= 8.0 * Math.Floor(z * 0.125); /* trim off integer >= 8 */
            n = (int)z;
            z -= (double)n;
            ih = 0;
            if (q0 > 0)
            { /* need iq[jz-1] to determine n */
                i = (iq[jz - 1] >> (24 - q0));
                n += i;
                iq[jz - 1] -= i << (24 - q0);
                ih = iq[jz - 1] >> (23 - q0);
            }
            else if (q0 == 0)
            {
                ih = iq[jz - 1] >> 23;
            }
            else if (z >= 0.5)
            {
                ih = 2;
            }

            if (ih > 0)
            { /* q > 0.5 */
                n += 1;
                carry = 0;
                for (i = 0; i < jz; i++)
                { /* compute 1-q */
                    j = iq[i];
                    if (carry == 0)
                    {
                        if (j != 0)
                        {
                            carry = 1;
                            iq[i] = 0x1000000 - j;
                        }
                    }
                    else
                    {
                        iq[i] = 0xFFFFFF - j;
                    }
                }
                if (q0 > 0)
                { /* rare case: chance is 1 in 12 */
                    switch (q0)
                    {
                        case 1:
                            iq[jz - 1] &= 0x7FFFFF;
                            break;
                        case 2:
                            iq[jz - 1] &= 0x3FFFFF;
                            break;
                    }
                }
                if (ih == 2)
                {
                    z = one - z;
                    if (carry != 0) z -= FdScalbn(one, q0);
                }
            }

            /* check if recomputation is needed */
            if (z == zero)
            {
                j = 0;
                for (i = jz - 1; i >= jk; i--) j |= iq[i];
                if (j == 0)
                { /* need recomputation */
                    for (k = 1; jk >= k && iq[jk - k] == 0; k++)
                    {
                        /* k = no. of terms needed */
                    }

                    for (i = jz + 1; i <= jz + k; i++)
                    { /* add q[jz+1] to q[jz+k] */
                        f[jx + i] = ipio2[jv + i];
                        for (j = 0, fw = 0.0; j <= jx; j++) fw += x[j] * f[jx + i - j];
                        q[i] = fw;
                    }
                    jz += k;
                    goto recompute;
                }
            }

            /* chop off zero terms */
            if (z == 0.0)
            {
                jz -= 1;
                q0 -= 24;
                while (iq[jz] == 0)
                {
                    jz--;
                    q0 -= 24;
                }
            }
            else
            { /* break z into 24-bit if necessary */
                z = FdScalbn(z, -q0);
                if (z >= two24)
                {
                    fw = (double)(int)(twon24 * z);
                    iq[jz] = (int)(z - two24 * fw);
                    jz += 1;
                    q0 += 24;
                    iq[jz] = (int)fw;
                }
                else
                {
                    iq[jz] = (int)z;
                }
            }

            /* convert integer "bit" chunk to floating-point value */
            fw = FdScalbn(one, q0);
            for (i = jz; i >= 0; i--)
            {
                q[i] = fw * iq[i];
                fw *= twon24;
            }

            /* compute PIo2[0,...,jp]*q[jz,...,0] */
            for (i = jz; i >= 0; i--)
            {
                for (fw = 0.0, k = 0; k <= jp && k <= jz - i; k++) fw += PIo2[k] * q[i + k];
                fq[jz - i] = fw;
            }

            /* compress fq[] into y[] */
            switch (prec)
            {
                case 0:
                    fw = 0.0;
                    for (i = jz; i >= 0; i--) fw += fq[i];
                    y[0] = (ih == 0) ? fw : -fw;
                    break;
                case 1:
                case 2:
                    fw = 0.0;
                    for (i = jz; i >= 0; i--) fw += fq[i];
                    y[0] = (ih == 0) ? fw : -fw;
                    fw = fq[0] - fw;
                    for (i = 1; i <= jz; i++) fw += fq[i];
                    y[1] = (ih == 0) ? fw : -fw;
                    break;
                case 3: /* painful */
                    for (i = jz; i > 0; i--)
                    {
                        fw = fq[i - 1] + fq[i];
                        fq[i] += fq[i - 1] - fw;
                        fq[i - 1] = fw;
                    }
                    for (i = jz; i > 1; i--)
                    {
                        fw = fq[i - 1] + fq[i];
                        fq[i] += fq[i - 1] - fw;
                        fq[i - 1] = fw;
                    }
                    for (fw = 0.0, i = jz; i >= 2; i--) fw += fq[i];
                    if (ih == 0)
                    {
                        y[0] = fq[0];
                        y[1] = fq[1];
                        y[2] = fw;
                    }
                    else
                    {
                        y[0] = -fq[0];
                        y[1] = -fq[1];
                        y[2] = -fw;
                    }
                    break;
            }
            return n & 7;
        }

        // --- Math.tan ---------------------------------------------------------------------------------------------------

        /* __kernel_tan( x, y, k )
         * kernel tan function on [-pi/4, pi/4], pi/4 ~ 0.7854
         * Input x is assumed to be bounded by ~pi/4 in magnitude.
         * Input y is the tail of x.
         * Input k indicates whether tan (if k=1) or
         * -1/tan (if k= -1) is returned.
         *
         * Algorithm
         *      1. Since tan(-x) = -tan(x), we need only to consider positive x.
         *      2. if x < 2^-28 (hx<0x3E300000 0), return x with inexact if x!=0.
         *      3. tan(x) is approximated by an odd polynomial of degree 27 on
         *         [0,0.67434]
         *                               3             27
         *              tan(x) ~ x + T1*x + ... + T13*x
         *         where
         *
         *              |tan(x)         2     4            26   |     -59.2
         *              |----- - (1+T1*x +T2*x +.... +T13*x    )| <= 2
         *              |  x                                    |
         *
         *         Note: tan(x+y) = tan(x) + tan'(x)*y
         *                        ~ tan(x) + (1+x*x)*y
         *         Therefore, for better accuracy in computing tan(x+y), let
         *                   3      2      2       2       2
         *              r = x *(T2+x *(T3+x *(...+x *(T12+x *T13))))
         *         then
         *                                  3    2
         *              tan(x+y) = x + (T1*x + (x *(r+y)+y))
         *
         *      4. For x in [0.67434,pi/4],  let y = pi/4 - x, then
         *              tan(x) = tan(pi/4-y) = (1-tan(y))/(1+tan(y))
         *                     = 1 - 2*(tan(y) - (tan(y)^2)/(1+tan(y)))
         */
        static readonly double[] KernelTanT =
        {
            3.33333333333334091986e-01, /* 3FD55555, 55555563 */
            1.33333333333201242699e-01, /* 3FC11111, 1110FE7A */
            5.39682539762260521377e-02, /* 3FABA1BA, 1BB341FE */
            2.18694882948595424599e-02, /* 3F9664F4, 8406D637 */
            8.86323982359930005737e-03, /* 3F8226E3, E96E8493 */
            3.59207910759131235356e-03, /* 3F6D6D22, C9560328 */
            1.45620945432529025516e-03, /* 3F57DBC8, FEE08315 */
            5.88041240820264096874e-04, /* 3F4344D8, F2F26501 */
            2.46463134818469906812e-04, /* 3F3026F7, 1A8D1068 */
            7.81794442939557092300e-05, /* 3F147E88, A03792A6 */
            7.14072491382608190305e-05, /* 3F12B80F, 32F0A7E9 */
            -1.85586374855275456654e-05, /* BEF375CB, DB605373 */
            2.59073051863633712884e-05, /* 3EFB2A70, 74BF7AD4 */
        };

        static double KernelTan(double x, double y, int iy)
        {
            const double one = 1.00000000000000000000e+00, /* 3FF00000, 00000000 */
                pio4 = 7.85398163397448278999e-01, /* 3FE921FB, 54442D18 */
                pio4lo = 3.06161699786838301793e-17; /* 3C81A626, 33145C07 */
            double[] T = KernelTanT;

            double z, r, v, w, s;
            int ix, hx;

            hx = HighWord(x); /* high word of x */
            ix = hx & 0x7FFFFFFF; /* high word of |x| */
            if (ix < 0x3E300000)
            { /* x < 2**-28 */
                if ((int)x == 0)
                { /* generate inexact */
                    uint low = FdLowWord(x);
                    if ((((uint)ix | low) | (uint)(iy + 1)) == 0)
                    {
                        return one / Math.Abs(x);
                    }
                    else
                    {
                        if (iy == 1)
                        {
                            return x;
                        }
                        else
                        { /* compute -1 / (x+y) carefully */
                            double a, t;

                            z = w = x + y;
                            z = FdSetLowWord(z, 0);
                            v = y - (z - x);
                            t = a = -one / w;
                            t = FdSetLowWord(t, 0);
                            s = one + t * z;
                            return t + a * (s + t * v);
                        }
                    }
                }
            }
            if (ix >= 0x3FE59428)
            { /* |x| >= 0.6744 */
                if (hx < 0)
                {
                    x = -x;
                    y = -y;
                }
                z = pio4 - x;
                w = pio4lo - y;
                x = z + w;
                y = 0.0;
            }
            z = x * x;
            w = z * z;
            /*
             * Break x^5*(T[1]+x^2*T[2]+...) into
             * x^5(T[1]+x^4*T[3]+...+x^20*T[11]) +
             * x^5(x^2*(T[2]+x^4*T[4]+...+x^22*[T12]))
             */
            r = T[1] + w * (T[3] + w * (T[5] + w * (T[7] + w * (T[9] + w * T[11]))));
            v = z * (T[2] + w * (T[4] + w * (T[6] + w * (T[8] + w * (T[10] + w * T[12])))));
            s = z * x;
            r = y + z * (s * (r + v) + y);
            r += T[0] * s;
            w = x + r;
            if (ix >= 0x3FE59428)
            {
                v = iy;
                return (1 - ((hx >> 30) & 2)) * (v - 2.0 * (x - (w * w / (w + v) - r)));
            }
            if (iy == 1)
            {
                return w;
            }
            else
            {
                /*
                 * if allow error up to 2 ulp, simply return
                 * -1.0 / (x+r) here
                 */
                /* compute -1.0 / (x+r) accurately */
                double a, t;
                z = w;
                z = FdSetLowWord(z, 0);
                v = r - (z - x); /* z+v = r+x */
                t = a = -1.0 / w; /* a = -1.0/w */
                t = FdSetLowWord(t, 0);
                s = 1.0 + t * z;
                return t + a * (s + t * v);
            }
        }

        /* tan(x)
         * Return tangent function of x.
         *
         * kernel function:
         *      __kernel_tan            ... tangent function on [-pi/4,pi/4]
         *      __ieee754_rem_pio2      ... argument reduction routine
         *
         * Method.
         *      Let S,C and T denote the sin, cos and tan respectively on
         *      [-PI/4, +PI/4]. Reduce the argument x to y1+y2 = x-k*pi/2
         *      in [-pi/4 , +pi/4], and let n = k mod 4.
         *      We have
         *
         *          n        sin(x)      cos(x)        tan(x)
         *     ----------------------------------------------------------
         *          0          S           C             T
         *          1          C          -S            -1/T
         *          2         -S          -C             T
         *          3         -C           S            -1/T
         *     ----------------------------------------------------------
         *
         * Special cases:
         *      Let trig be any of sin, cos, or tan.
         *      trig(+-INF)  is NaN, with signals;
         *      trig(NaN)    is that NaN;
         *
         * Accuracy:
         *      TRIG(x) returns trig(x) nearly rounded
         */
        public static double Tan(double x)
        {
            double z = 0.0;
            int n, ix;

            /* High word of x. */
            ix = HighWord(x);

            /* |x| ~< pi/4 */
            ix &= 0x7FFFFFFF;
            if (ix <= 0x3FE921FB)
            {
                return KernelTan(x, z, 1);
            }
            else if (ix >= 0x7FF00000)
            {
                /* tan(Inf or NaN) is NaN */
                return x - x; /* NaN */
            }
            else
            {
                /* argument reduction needed */
                n = RemPio2Any(x, out double y0, out double y1);
                /* 1 -> n even, -1 -> n odd */
                return KernelTan(y0, y1, 1 - ((n & 1) << 1));
            }
        }

        // --- Math.acos, Math.asin ------------------------------------------------------------------------------------------

        const double FdPS0 = 1.66666666666666657415e-01, /* 0x3FC55555, 0x55555555 */
            FdPS1 = -3.25565818622400915405e-01, /* 0xBFD4D612, 0x03EB6F7D */
            FdPS2 = 2.01212532134862925881e-01, /* 0x3FC9C155, 0x0E884455 */
            FdPS3 = -4.00555345006794114027e-02, /* 0xBFA48228, 0xB5688F3B */
            FdPS4 = 7.91534994289814532176e-04, /* 0x3F49EFE0, 0x7501B288 */
            FdPS5 = 3.47933107596021167570e-05, /* 0x3F023DE1, 0x0DFDF709 */
            FdQS1 = -2.40339491173441421878e+00, /* 0xC0033A27, 0x1C8A2D4B */
            FdQS2 = 2.02094576023350569471e+00, /* 0x40002AE5, 0x9C598AC8 */
            FdQS3 = -6.88283971605453293030e-01, /* 0xBFE6066C, 0x1B8D0159 */
            FdQS4 = 7.70381505559019352791e-02; /* 0x3FB3B8C5, 0xB12E9282 */

        /* acos(x)
         * Method :
         *      acos(x)  = pi/2 - asin(x)
         *      acos(-x) = pi/2 + asin(x)
         * For |x|<=0.5
         *      acos(x) = pi/2 - (x + x*x^2*R(x^2))     (see asin.c)
         * For x>0.5
         *      acos(x) = pi/2 - (pi/2 - 2asin(sqrt((1-x)/2)))
         *              = 2asin(sqrt((1-x)/2))
         *              = 2s + 2s*z*R(z)        ...z=(1-x)/2, s=sqrt(z)
         *              = 2f + (2c + 2s*z*R(z))
         *     where f=hi part of s, and c = (z-f*f)/(s+f) is the correction term
         *     for f so that f+c ~ sqrt(z).
         * For x<-0.5
         *      acos(x) = pi - 2asin(sqrt((1-|x|)/2))
         *              = pi - 0.5*(s+s*z*R(z)), where z=(1-|x|)/2,s=sqrt(z)
         *
         * Special cases:
         *      if x is NaN, return x itself;
         *      if |x|>1, return NaN with invalid signal.
         *
         * Function needed: sqrt
         */
        public static double Acos(double x)
        {
            const double one = 1.00000000000000000000e+00, /* 0x3FF00000, 0x00000000 */
                pi = 3.14159265358979311600e+00, /* 0x400921FB, 0x54442D18 */
                pio2_hi = 1.57079632679489655800e+00, /* 0x3FF921FB, 0x54442D18 */
                pio2_lo = 6.12323399573676603587e-17, /* 0x3C91A626, 0x33145C07 */
                pS0 = FdPS0, pS1 = FdPS1, pS2 = FdPS2, pS3 = FdPS3, pS4 = FdPS4, pS5 = FdPS5,
                qS1 = FdQS1, qS2 = FdQS2, qS3 = FdQS3, qS4 = FdQS4;

            double z, p, q, r, w, s, c, df;
            int hx, ix;
            hx = HighWord(x);
            ix = hx & 0x7FFFFFFF;
            if (ix >= 0x3FF00000)
            { /* |x| >= 1 */
                uint lx = FdLowWord(x);
                if ((unchecked((uint)(ix - 0x3FF00000)) | lx) == 0)
                { /* |x|==1 */
                    if (hx > 0)
                        return 0.0; /* acos(1) = 0  */
                    else
                        return pi + 2.0 * pio2_lo; /* acos(-1)= pi */
                }
                return double.NaN; // acos(|x|>1) is NaN
            }
            if (ix < 0x3FE00000)
            { /* |x| < 0.5 */
                if (ix <= 0x3C600000) return pio2_hi + pio2_lo; /*if|x|<2**-57*/
                z = x * x;
                p = z * (pS0 + z * (pS1 + z * (pS2 + z * (pS3 + z * (pS4 + z * pS5)))));
                q = one + z * (qS1 + z * (qS2 + z * (qS3 + z * qS4)));
                r = p / q;
                return pio2_hi - (x - (pio2_lo - x * r));
            }
            else if (hx < 0)
            { /* x < -0.5 */
                z = (one + x) * 0.5;
                p = z * (pS0 + z * (pS1 + z * (pS2 + z * (pS3 + z * (pS4 + z * pS5)))));
                q = one + z * (qS1 + z * (qS2 + z * (qS3 + z * qS4)));
                s = Math.Sqrt(z);
                r = p / q;
                w = r * s - pio2_lo;
                return pi - 2.0 * (s + w);
            }
            else
            { /* x > 0.5 */
                z = (one - x) * 0.5;
                s = Math.Sqrt(z);
                df = s;
                df = FdSetLowWord(df, 0);
                c = (z - df * df) / (s + df);
                p = z * (pS0 + z * (pS1 + z * (pS2 + z * (pS3 + z * (pS4 + z * pS5)))));
                q = one + z * (qS1 + z * (qS2 + z * (qS3 + z * qS4)));
                r = p / q;
                w = r * s + c;
                return 2.0 * (df + w);
            }
        }

        /* asin(x)
         * Method :
         *      Since  asin(x) = x + x^3/6 + x^5*3/40 + x^7*15/336 + ...
         *      we approximate asin(x) on [0,0.5] by
         *              asin(x) = x + x*x^2*R(x^2)
         *      where
         *              R(x^2) is a rational approximation of (asin(x)-x)/x^3
         *      and its remez error is bounded by
         *              |(asin(x)-x)/x^3 - R(x^2)| < 2^(-58.75)
         *
         *      For x in [0.5,1]
         *              asin(x) = pi/2-2*asin(sqrt((1-x)/2))
         *      Let y = (1-x), z = y/2, s := sqrt(z), and pio2_hi+pio2_lo=pi/2;
         *      then for x>0.98
         *              asin(x) = pi/2 - 2*(s+s*z*R(z))
         *                      = pio2_hi - (2*(s+s*z*R(z)) - pio2_lo)
         *      For x<=0.98, let pio4_hi = pio2_hi/2, then
         *              f = hi part of s;
         *              c = sqrt(z) - f = (z-f*f)/(s+f)         ...f+c=sqrt(z)
         *      and
         *              asin(x) = pi/2 - 2*(s+s*z*R(z))
         *                      = pio4_hi+(pio4-2s)-(2s*z*R(z)-pio2_lo)
         *                      = pio4_hi+(pio4-2f)-(2s*z*R(z)-(pio2_lo+2c))
         *
         * Special cases:
         *      if x is NaN, return x itself;
         *      if |x|>1, return NaN with invalid signal.
         */
        public static double Asin(double x)
        {
            const double one = 1.00000000000000000000e+00, /* 0x3FF00000, 0x00000000 */
                huge = 1.000e+300,
                pio2_hi = 1.57079632679489655800e+00, /* 0x3FF921FB, 0x54442D18 */
                pio2_lo = 6.12323399573676603587e-17, /* 0x3C91A626, 0x33145C07 */
                pio4_hi = 7.85398163397448278999e-01, /* 0x3FE921FB, 0x54442D18 */
                /* coefficient for R(x^2) */
                pS0 = FdPS0, pS1 = FdPS1, pS2 = FdPS2, pS3 = FdPS3, pS4 = FdPS4, pS5 = FdPS5,
                qS1 = FdQS1, qS2 = FdQS2, qS3 = FdQS3, qS4 = FdQS4;

            double t, w, p, q, c, r, s;
            int hx, ix;

            t = 0;
            hx = HighWord(x);
            ix = hx & 0x7FFFFFFF;
            if (ix >= 0x3FF00000)
            { /* |x|>= 1 */
                uint lx = FdLowWord(x);
                if ((unchecked((uint)(ix - 0x3FF00000)) | lx) == 0)
                { /* asin(1)=+-pi/2 with inexact */
                    return x * pio2_hi + x * pio2_lo;
                }
                return double.NaN; // asin(|x|>1) is NaN
            }
            else if (ix < 0x3FE00000)
            { /* |x|<0.5 */
                if (ix < 0x3E400000)
                { /* if |x| < 2**-27 */
                    if (huge + x > one) return x; /* return x with inexact if x!=0*/
                }
                else
                {
                    t = x * x;
                }
                p = t * (pS0 + t * (pS1 + t * (pS2 + t * (pS3 + t * (pS4 + t * pS5)))));
                q = one + t * (qS1 + t * (qS2 + t * (qS3 + t * qS4)));
                w = p / q;
                return x + x * w;
            }
            /* 1> |x|>= 0.5 */
            w = one - Math.Abs(x);
            t = w * 0.5;
            p = t * (pS0 + t * (pS1 + t * (pS2 + t * (pS3 + t * (pS4 + t * pS5)))));
            q = one + t * (qS1 + t * (qS2 + t * (qS3 + t * qS4)));
            s = Math.Sqrt(t);
            if (ix >= 0x3FEF3333)
            { /* if |x| > 0.975 */
                w = p / q;
                t = pio2_hi - (2.0 * (s + s * w) - pio2_lo);
            }
            else
            {
                w = s;
                w = FdSetLowWord(w, 0);
                c = (t - w * w) / (s + w);
                r = p / q;
                p = 2.0 * s * r - (pio2_lo - 2.0 * c);
                q = pio4_hi - 2.0 * w;
                t = pio4_hi - (p - q);
            }
            if (hx > 0)
                return t;
            else
                return -t;
        }

        // --- Math.atan, Math.atan2 -------------------------------------------------------------------------------------

        /* atan(x)
         * Method
         *   1. Reduce x to positive by atan(x) = -atan(-x).
         *   2. According to the integer k=4t+0.25 chopped, t=x, the argument
         *      is further reduced to one of the following intervals and the
         *      arctangent of t is evaluated by the corresponding formula:
         *
         *      [0,7/16]      atan(x) = t-t^3*(a1+t^2*(a2+...(a10+t^2*a11)...)
         *      [7/16,11/16]  atan(x) = atan(1/2) + atan( (t-0.5)/(1+t/2) )
         *      [11/16.19/16] atan(x) = atan( 1 ) + atan( (t-1)/(1+t) )
         *      [19/16,39/16] atan(x) = atan(3/2) + atan( (t-1.5)/(1+1.5t) )
         *      [39/16,INF]   atan(x) = atan(INF) + atan( -1/t )
         *
         * Constants:
         * The hexadecimal values are the intended ones for the following
         * constants. The decimal values may be used, provided that the
         * compiler will convert from decimal to binary accurately enough
         * to produce the hexadecimal values shown.
         */
        static readonly double[] AtanHi =
        {
            4.63647609000806093515e-01, /* atan(0.5)hi 0x3FDDAC67, 0x0561BB4F */
            7.85398163397448278999e-01, /* atan(1.0)hi 0x3FE921FB, 0x54442D18 */
            9.82793723247329054082e-01, /* atan(1.5)hi 0x3FEF730B, 0xD281F69B */
            1.57079632679489655800e+00, /* atan(inf)hi 0x3FF921FB, 0x54442D18 */
        };

        static readonly double[] AtanLo =
        {
            2.26987774529616870924e-17, /* atan(0.5)lo 0x3C7A2B7F, 0x222F65E2 */
            3.06161699786838301793e-17, /* atan(1.0)lo 0x3C81A626, 0x33145C07 */
            1.39033110312309984516e-17, /* atan(1.5)lo 0x3C700788, 0x7AF0CBBD */
            6.12323399573676603587e-17, /* atan(inf)lo 0x3C91A626, 0x33145C07 */
        };

        static readonly double[] AtanT =
        {
            3.33333333333329318027e-01, /* 0x3FD55555, 0x5555550D */
            -1.99999999998764832476e-01, /* 0xBFC99999, 0x9998EBC4 */
            1.42857142725034663711e-01, /* 0x3FC24924, 0x920083FF */
            -1.11111104054623557880e-01, /* 0xBFBC71C6, 0xFE231671 */
            9.09088713343650656196e-02, /* 0x3FB745CD, 0xC54C206E */
            -7.69187620504482999495e-02, /* 0xBFB3B0F2, 0xAF749A6D */
            6.66107313738753120669e-02, /* 0x3FB10D66, 0xA0D03D51 */
            -5.83357013379057348645e-02, /* 0xBFADDE2D, 0x52DEFD9A */
            4.97687799461593236017e-02, /* 0x3FA97B4B, 0x24760DEB */
            -3.65315727442169155270e-02, /* 0xBFA2B444, 0x2C6A6C2F */
            1.62858201153657823623e-02, /* 0x3F90AD3A, 0xE322DA11 */
        };

        public static double Atan(double x)
        {
            double[] atanhi = AtanHi, atanlo = AtanLo, aT = AtanT;
            const double one = 1.0, huge = 1.0e300;

            double w, s1, s2, z;
            int ix, hx, id;

            hx = HighWord(x);
            ix = hx & 0x7FFFFFFF;
            if (ix >= 0x44100000)
            { /* if |x| >= 2^66 */
                uint low = FdLowWord(x);
                if (ix > 0x7FF00000 || (ix == 0x7FF00000 && (low != 0)))
                    return x + x; /* NaN */
                if (hx > 0)
                    return atanhi[3] + atanlo[3];
                else
                    return -atanhi[3] - atanlo[3];
            }
            if (ix < 0x3FDC0000)
            { /* |x| < 0.4375 */
                if (ix < 0x3E400000)
                { /* |x| < 2^-27 */
                    if (huge + x > one) return x; /* raise inexact */
                }
                id = -1;
            }
            else
            {
                x = Math.Abs(x);
                if (ix < 0x3FF30000)
                { /* |x| < 1.1875 */
                    if (ix < 0x3FE60000)
                    { /* 7/16 <=|x|<11/16 */
                        id = 0;
                        x = (2.0 * x - one) / (2.0 + x);
                    }
                    else
                    { /* 11/16<=|x|< 19/16 */
                        id = 1;
                        x = (x - one) / (x + one);
                    }
                }
                else
                {
                    if (ix < 0x40038000)
                    { /* |x| < 2.4375 */
                        id = 2;
                        x = (x - 1.5) / (one + 1.5 * x);
                    }
                    else
                    { /* 2.4375 <= |x| < 2^66 */
                        id = 3;
                        x = -1.0 / x;
                    }
                }
            }
            /* end of argument reduction */
            z = x * x;
            w = z * z;
            /* break sum from i=0 to 10 aT[i]z**(i+1) into odd and even poly */
            s1 = z * (aT[0] + w * (aT[2] + w * (aT[4] + w * (aT[6] + w * (aT[8] + w * aT[10])))));
            s2 = w * (aT[1] + w * (aT[3] + w * (aT[5] + w * (aT[7] + w * aT[9]))));
            if (id < 0)
            {
                return x - x * (s1 + s2);
            }
            else
            {
                z = atanhi[id] - ((x * (s1 + s2) - atanlo[id]) - x);
                return (hx < 0) ? -z : z;
            }
        }

        /* atan2(y,x)
         * Method :
         *  1. Reduce y to positive by atan2(y,x)=-atan2(-y,x).
         *  2. Reduce x to positive by (if x and y are unexceptional):
         *    ARG (x+iy) = arctan(y/x)       ... if x > 0,
         *    ARG (x+iy) = pi - arctan[y/(-x)]   ... if x < 0,
         *
         * Special cases:
         *
         *  ATAN2((anything), NaN ) is NaN;
         *  ATAN2(NAN , (anything) ) is NaN;
         *  ATAN2(+-0, +(anything but NaN)) is +-0  ;
         *  ATAN2(+-0, -(anything but NaN)) is +-pi ;
         *  ATAN2(+-(anything but 0 and NaN), 0) is +-pi/2;
         *  ATAN2(+-(anything but INF and NaN), +INF) is +-0 ;
         *  ATAN2(+-(anything but INF and NaN), -INF) is +-pi;
         *  ATAN2(+-INF,+INF ) is +-pi/4 ;
         *  ATAN2(+-INF,-INF ) is +-3pi/4;
         *  ATAN2(+-INF, (anything but,0,NaN, and INF)) is +-pi/2;
         *
         * Constants:
         * The hexadecimal values are the intended ones for the following
         * constants. The decimal values may be used, provided that the
         * compiler will convert from decimal to binary accurately enough
         * to produce the hexadecimal values shown.
         */
        public static double Atan2(double y, double x)
        {
            const double tiny = 1.0e-300;
            const double zero = 0.0,
                pi_o_4 = 7.8539816339744827900E-01, /* 0x3FE921FB, 0x54442D18 */
                pi_o_2 = 1.5707963267948965580E+00, /* 0x3FF921FB, 0x54442D18 */
                pi = 3.1415926535897931160E+00; /* 0x400921FB, 0x54442D18 */
            const double pi_lo = 1.2246467991473531772E-16; /* 0x3CA1A626, 0x33145C07 */

            double z;
            int k, m, hx, hy, ix, iy;
            uint lx, ly;

            hx = HighWord(x);
            lx = FdLowWord(x);
            ix = hx & 0x7FFFFFFF;
            hy = HighWord(y);
            ly = FdLowWord(y);
            iy = hy & 0x7FFFFFFF;
            if (unchecked((uint)ix | ((lx | (uint)(-(int)lx)) >> 31)) > 0x7FF00000 ||
                unchecked((uint)iy | ((ly | (uint)(-(int)ly)) >> 31)) > 0x7FF00000)
            {
                return x + y; /* x or y is NaN */
            }
            if ((unchecked((uint)(hx - 0x3FF00000)) | lx) == 0)
            {
                return Atan(y); /* x=1.0 */
            }
            m = ((hy >> 31) & 1) | ((hx >> 30) & 2); /* 2*sign(x)+sign(y) */

            /* when y = 0 */
            if (((uint)iy | ly) == 0)
            {
                switch (m)
                {
                    case 0:
                    case 1:
                        return y; /* atan(+-0,+anything)=+-0 */
                    case 2:
                        return pi + tiny; /* atan(+0,-anything) = pi */
                    case 3:
                        return -pi - tiny; /* atan(-0,-anything) =-pi */
                }
            }
            /* when x = 0 */
            if (((uint)ix | lx) == 0) return (hy < 0) ? -pi_o_2 - tiny : pi_o_2 + tiny;

            /* when x is INF */
            if (ix == 0x7FF00000)
            {
                if (iy == 0x7FF00000)
                {
                    switch (m)
                    {
                        case 0:
                            return pi_o_4 + tiny; /* atan(+INF,+INF) */
                        case 1:
                            return -pi_o_4 - tiny; /* atan(-INF,+INF) */
                        case 2:
                            return 3.0 * pi_o_4 + tiny; /*atan(+INF,-INF)*/
                        case 3:
                            return -3.0 * pi_o_4 - tiny; /*atan(-INF,-INF)*/
                    }
                }
                else
                {
                    switch (m)
                    {
                        case 0:
                            return zero; /* atan(+...,+INF) */
                        case 1:
                            return -zero; /* atan(-...,+INF) */
                        case 2:
                            return pi + tiny; /* atan(+...,-INF) */
                        case 3:
                            return -pi - tiny; /* atan(-...,-INF) */
                    }
                }
            }
            /* when y is INF */
            if (iy == 0x7FF00000) return (hy < 0) ? -pi_o_2 - tiny : pi_o_2 + tiny;

            /* compute y/x */
            k = (iy - ix) >> 20;
            if (k > 60)
            { /* |y/x| >  2**60 */
                z = pi_o_2 + 0.5 * pi_lo;
                m &= 1;
            }
            else if (hx < 0 && k < -60)
            {
                z = 0.0; /* 0 > |y|/x > -2**-60 */
            }
            else
            {
                z = Atan(Math.Abs(y / x)); /* safe to do y/x */
            }
            switch (m)
            {
                case 0:
                    return z; /* atan(+,+) */
                case 1:
                    return -z; /* atan(-,+) */
                case 2:
                    return pi - (z - pi_lo); /* atan(+,-) */
                default: /* case 3 */
                    return (z - pi_lo) - pi; /* atan(-,-) */
            }
        }

        // --- Math.exp ------------------------------------------------------------------------------------------------------

        /* exp(x)
         * Returns the exponential of x.
         *
         * Method
         *   1. Argument reduction:
         *      Reduce x to an r so that |r| <= 0.5*ln2 ~ 0.34658.
         *      Given x, find r and integer k such that
         *
         *               x = k*ln2 + r,  |r| <= 0.5*ln2.
         *
         *      Here r will be represented as r = hi-lo for better
         *      accuracy.
         *
         *   2. Approximation of exp(r) by a special rational function on
         *      the interval [0,0.34658]:
         *      Write
         *          R(r**2) = r*(exp(r)+1)/(exp(r)-1) = 2 + r*r/6 - r**4/360 + ...
         *      We use a special Remes algorithm on [0,0.34658] to generate
         *      a polynomial of degree 5 to approximate R. The maximum error
         *      of this polynomial approximation is bounded by 2**-59. In
         *      other words,
         *          R(z) ~ 2.0 + P1*z + P2*z**2 + P3*z**3 + P4*z**4 + P5*z**5
         *      (where z=r*r, and the values of P1 to P5 are listed below)
         *      and
         *          |                  5          |     -59
         *          | 2.0+P1*z+...+P5*z   -  R(z) | <= 2
         *          |                             |
         *      The computation of exp(r) thus becomes
         *                             2*r
         *              exp(r) = 1 + -------
         *                            R - r
         *                                 r*R1(r)
         *                     = 1 + r + ----------- (for better accuracy)
         *                                2 - R1(r)
         *      where
         *                               2       4             10
         *              R1(r) = r - (P1*r  + P2*r  + ... + P5*r   ).
         *
         *   3. Scale back to obtain exp(x):
         *      From step 1, we have
         *         exp(x) = 2^k * exp(r)
         *
         * Special cases:
         *      exp(INF) is INF, exp(NaN) is NaN;
         *      exp(-INF) is 0, and
         *      for finite argument, only exp(0)=1 is exact.
         *
         * Accuracy:
         *      according to an error analysis, the error is always less than
         *      1 ulp (unit in the last place).
         *
         * Misc. info.
         *      For IEEE double
         *          if x >  7.09782712893383973096e+02 then exp(x) overflow
         *          if x < -7.45133219101941108420e+02 then exp(x) underflow
         *
         * Constants:
         * The hexadecimal values are the intended ones for the following
         * constants. The decimal values may be used, provided that the
         * compiler will convert from decimal to binary accurately enough
         * to produce the hexadecimal values shown.
         */
        static readonly double[] ExpHalF = { 0.5, -0.5 };

        static readonly double[] ExpLn2HI =
        {
            6.93147180369123816490e-01, /* 0x3FE62E42, 0xFEE00000 */
            -6.93147180369123816490e-01, /* 0xBFE62E42, 0xFEE00000 */
        };

        static readonly double[] ExpLn2LO =
        {
            1.90821492927058770002e-10, /* 0x3DEA39EF, 0x35793C76 */
            -1.90821492927058770002e-10, /* 0xBDEA39EF, 0x35793C76 */
        };

        public static double Exp(double x)
        {
            double[] halF = ExpHalF, ln2HI = ExpLn2HI, ln2LO = ExpLn2LO;
            const double one = 1.0,
                o_threshold = 7.09782712893383973096e+02, /* 0x40862E42, 0xFEFA39EF */
                u_threshold = -7.45133219101941108420e+02, /* 0xC0874910, 0xD52D3051 */
                invln2 = 1.44269504088896338700e+00, /* 0x3FF71547, 0x652B82FE */
                P1 = 1.66666666666666019037e-01, /* 0x3FC55555, 0x5555553E */
                P2 = -2.77777777770155933842e-03, /* 0xBF66C16C, 0x16BEBD93 */
                P3 = 6.61375632143793436117e-05, /* 0x3F11566A, 0xAF25DE2C */
                P4 = -1.65339022054652515390e-06, /* 0xBEBBBD41, 0xC5D26BF1 */
                P5 = 4.13813679705723846039e-08, /* 0x3E663769, 0x72BEA4D0 */
                E = 2.718281828459045; /* 0x4005BF0A, 0x8B145769 */

            const double huge = 1.0e+300,
                twom1000 = 9.33263618503218878990e-302, /* 2**-1000=0x01700000,0*/
                two1023 = 8.988465674311579539e307; /* 0x1p1023 */

            double y, hi = 0.0, lo = 0.0, c, t, twopk;
            int k = 0, xsb;
            uint hx;

            hx = unchecked((uint)HighWord(x));
            xsb = (int)((hx >> 31) & 1); /* sign bit of x */
            hx &= 0x7FFFFFFF; /* high word of |x| */

            /* filter out non-finite argument */
            if (hx >= 0x40862E42)
            { /* if |x|>=709.78... */
                if (hx >= 0x7FF00000)
                {
                    uint lx = FdLowWord(x);
                    if (((hx & 0xFFFFF) | lx) != 0)
                        return x + x; /* NaN */
                    else
                        return (xsb == 0) ? x : 0.0; /* exp(+-inf)={inf,0} */
                }
                if (x > o_threshold) return huge * huge; /* overflow */
                if (x < u_threshold) return twom1000 * twom1000; /* underflow */
            }

            /* argument reduction */
            if (hx > 0x3FD62E42)
            { /* if  |x| > 0.5 ln2 */
                if (hx < 0x3FF0A2B2)
                { /* and |x| < 1.5 ln2 */
                    /* TODO(rtoy): We special case exp(1) here to return the correct
                     * value of E, as the computation below would get the last bit
                     * wrong. We should probably fix the algorithm instead.
                     */
                    if (x == 1.0) return E;
                    hi = x - ln2HI[xsb];
                    lo = ln2LO[xsb];
                    k = 1 - xsb - xsb;
                }
                else
                {
                    k = (int)(invln2 * x + halF[xsb]);
                    t = k;
                    hi = x - t * ln2HI[0]; /* t*ln2HI is exact here */
                    lo = t * ln2LO[0];
                }
                x = hi - lo;
            }
            else if (hx < 0x3E300000)
            { /* when |x|<2**-28 */
                if (huge + x > one) return one + x; /* trigger inexact */
            }
            else
            {
                k = 0;
            }

            /* x is now in primary range */
            t = x * x;
            if (k >= -1021)
            {
                twopk = FdInsertWords(unchecked((uint)(0x3FF00000 + (int)((uint)k << 20))), 0);
            }
            else
            {
                twopk = FdInsertWords(unchecked(0x3FF00000u + ((uint)(k + 1000) << 20)), 0);
            }
            c = x - t * (P1 + t * (P2 + t * (P3 + t * (P4 + t * P5))));
            if (k == 0)
            {
                return one - ((x * c) / (c - 2.0) - x);
            }
            else
            {
                y = one - ((lo - (x * c) / (2.0 - c)) - hi);
            }
            if (k >= -1021)
            {
                if (k == 1024) return y * 2.0 * two1023;
                return y * twopk;
            }
            else
            {
                return y * twopk * twom1000;
            }
        }

        // --- Math.tanh (and the expm1 it calls) ----------------------------------------------------------------------------

        /* expm1(x)
         * Returns exp(x)-1, the exponential of x minus 1.
         *
         * Method
         *   1. Argument reduction:
         *  Given x, find r and integer k such that
         *
         *               x = k*ln2 + r,  |r| <= 0.5*ln2 ~ 0.34658
         *
         *      Here a correction term c will be computed to compensate
         *  the error in r when rounded to a floating-point number.
         *
         *   2. Approximating expm1(r) by a special rational function on
         *  the interval [0,0.34658]:
         *  Since
         *      r*(exp(r)+1)/(exp(r)-1) = 2+ r^2/6 - r^4/360 + ...
         *  we define R1(r*r) by
         *      r*(exp(r)+1)/(exp(r)-1) = 2+ r^2/6 * R1(r*r)
         *  That is,
         *      R1(r**2) = 6/r *((exp(r)+1)/(exp(r)-1) - 2/r)
         *         = 6/r * ( 1 + 2.0*(1/(exp(r)-1) - 1/r))
         *         = 1 - r^2/60 + r^4/2520 - r^6/100800 + ...
         *      We use a special Reme algorithm on [0,0.347] to generate
         *   a polynomial of degree 5 in r*r to approximate R1. The
         *  maximum error of this polynomial approximation is bounded
         *  by 2**-61. In other words,
         *      R1(z) ~ 1.0 + Q1*z + Q2*z**2 + Q3*z**3 + Q4*z**4 + Q5*z**5
         *  where   Q1  =  -1.6666666666666567384E-2,
         *     Q2  =   3.9682539681370365873E-4,
         *     Q3  =  -9.9206344733435987357E-6,
         *     Q4  =   2.5051361420808517002E-7,
         *     Q5  =  -6.2843505682382617102E-9;
         *    z   =  r*r,
         *  with error bounded by
         *      |                  5           |     -61
         *      | 1.0+Q1*z+...+Q5*z   -  R1(z) | <= 2
         *      |                              |
         *
         *  expm1(r) = exp(r)-1 is then computed by the following
         *   specific way which minimize the accumulation rounding error:
         *             2     3
         *            r     r    [ 3 - (R1 + R1*r/2)  ]
         *        expm1(r) = r + --- + --- * [--------------------]
         *                  2     2    [ 6 - r*(3 - R1*r/2) ]
         *
         *  To compensate the error in the argument reduction, we use
         *    expm1(r+c) = expm1(r) + c + expm1(r)*c
         *         ~ expm1(r) + c + r*c
         *  Thus c+r*c will be added in as the correction terms for
         *  expm1(r+c). Now rearrange the term to avoid optimization
         *   screw up:
         *            (      2                                    2 )
         *            ({  ( r    [ R1 -  (3 - R1*r/2) ]  )  }    r  )
         *   expm1(r+c)~r - ({r*(--- * [--------------------]-c)-c} - --- )
         *                  ({  ( 2    [ 6 - r*(3 - R1*r/2) ]  )  }    2  )
         *                      (                                             )
         *
         *       = r - E
         *   3. Scale back to obtain expm1(x):
         *  From step 1, we have
         *     expm1(x) = either 2^k*[expm1(r)+1] - 1
         *        = or     2^k*[expm1(r) + (1-2^-k)]
         *   4. Implementation notes:
         *  (A). To save one multiplication, we scale the coefficient Qi
         *       to Qi*2^i, and replace z by (x^2)/2.
         *  (B). To achieve maximum accuracy, we compute expm1(x) by
         *    (i)   if x < -56*ln2, return -1.0, (raise inexact if x!=inf)
         *    (ii)  if k=0, return r-E
         *    (iii) if k=-1, return 0.5*(r-E)-0.5
         *        (iv)  if k=1 if r < -0.25, return 2*((r+0.5)- E)
         *                  else       return  1.0+2.0*(r-E);
         *    (v)   if (k<-2||k>56) return 2^k(1-(E-r)) - 1 (or exp(x)-1)
         *    (vi)  if k <= 20, return 2^k((1-2^-k)-(E-r)), else
         *    (vii) return 2^k(1-((E+2^-k)-r))
         *
         * Special cases:
         *  expm1(INF) is INF, expm1(NaN) is NaN;
         *  expm1(-INF) is -1, and
         *  for finite argument, only expm1(0)=0 is exact.
         *
         * Accuracy:
         *  according to an error analysis, the error is always less than
         *  1 ulp (unit in the last place).
         *
         * Misc. info.
         *  For IEEE double
         *      if x >  7.09782712893383973096e+02 then expm1(x) overflow
         *
         * Constants:
         * The hexadecimal values are the intended ones for the following
         * constants. The decimal values may be used, provided that the
         * compiler will convert from decimal to binary accurately enough
         * to produce the hexadecimal values shown.
         */
        static double Expm1(double x)
        {
            const double one = 1.0,
                tiny = 1.0e-300,
                o_threshold = 7.09782712893383973096e+02, /* 0x40862E42, 0xFEFA39EF */
                ln2_hi = 6.93147180369123816490e-01, /* 0x3FE62E42, 0xFEE00000 */
                ln2_lo = 1.90821492927058770002e-10, /* 0x3DEA39EF, 0x35793C76 */
                invln2 = 1.44269504088896338700e+00, /* 0x3FF71547, 0x652B82FE */
                /* Scaled Q's: Qn_here = 2**n * Qn_above, for R(2*z) where z = hxs =
                   x*x/2: */
                Q1 = -3.33333333333331316428e-02, /* BFA11111 111110F4 */
                Q2 = 1.58730158725481460165e-03, /* 3F5A01A0 19FE5585 */
                Q3 = -7.93650757867487942473e-05, /* BF14CE19 9EAADBB7 */
                Q4 = 4.00821782732936239552e-06, /* 3ED0CFCA 86E65239 */
                Q5 = -2.01099218183624371326e-07; /* BE8AFDB7 6E09C32D */

            const double huge = 1.0e+300;

            double y, hi, lo, c = 0, t, e, hxs, hfx, r1, twopk;
            int k, xsb;
            uint hx;

            hx = unchecked((uint)HighWord(x));
            xsb = unchecked((int)(hx & 0x80000000)); /* sign bit of x */
            hx &= 0x7FFFFFFF; /* high word of |x| */

            /* filter out huge and non-finite argument */
            if (hx >= 0x4043687A)
            { /* if |x|>=56*ln2 */
                if (hx >= 0x40862E42)
                { /* if |x|>=709.78... */
                    if (hx >= 0x7FF00000)
                    {
                        uint low = FdLowWord(x);
                        if (((hx & 0xFFFFF) | low) != 0)
                            return x + x; /* NaN */
                        else
                            return (xsb == 0) ? x : -1.0; /* exp(+-inf)={inf,-1} */
                    }
                    if (x > o_threshold) return huge * huge; /* overflow */
                }
                if (xsb != 0)
                { /* x < -56*ln2, return -1.0 with inexact */
                    if (x + tiny < 0.0) /* raise inexact */
                        return tiny - one; /* return -1 */
                }
            }

            /* argument reduction */
            if (hx > 0x3FD62E42)
            { /* if  |x| > 0.5 ln2 */
                if (hx < 0x3FF0A2B2)
                { /* and |x| < 1.5 ln2 */
                    if (xsb == 0)
                    {
                        hi = x - ln2_hi;
                        lo = ln2_lo;
                        k = 1;
                    }
                    else
                    {
                        hi = x + ln2_hi;
                        lo = -ln2_lo;
                        k = -1;
                    }
                }
                else
                {
                    k = (int)(invln2 * x + ((xsb == 0) ? 0.5 : -0.5));
                    t = k;
                    hi = x - t * ln2_hi; /* t*ln2_hi is exact here */
                    lo = t * ln2_lo;
                }
                x = hi - lo;
                c = (hi - x) - lo;
            }
            else if (hx < 0x3C900000)
            { /* when |x|<2**-54, return x */
                t = huge + x; /* return x with inexact flags when x!=0 */
                return x - (t - (huge + x));
            }
            else
            {
                k = 0;
            }

            /* x is now in primary range */
            hfx = 0.5 * x;
            hxs = x * hfx;
            r1 = one + hxs * (Q1 + hxs * (Q2 + hxs * (Q3 + hxs * (Q4 + hxs * Q5))));
            t = 3.0 - r1 * hfx;
            e = hxs * ((r1 - t) / (6.0 - x * t));
            if (k == 0)
            {
                return x - (x * e - hxs); /* c is 0 */
            }
            else
            {
                twopk = FdInsertWords(unchecked((uint)(0x3FF00000 + (int)((uint)k << 20))), 0); /* 2^k */
                e = (x * (e - c) - c);
                e -= hxs;
                if (k == -1) return 0.5 * (x - e) - 0.5;
                if (k == 1)
                {
                    if (x < -0.25)
                        return -2.0 * (e - (x + 0.5));
                    else
                        return one + 2.0 * (x - e);
                }
                if (k <= -2 || k > 56)
                { /* suffice to return exp(x)-1 */
                    y = one - (e - x);
                    // TODO(mvstanton): is this replacement for the hex float
                    // sufficient?
                    // if (k == 1024) y = y*2.0*0x1p1023;
                    if (k == 1024)
                        y = y * 2.0 * 8.98846567431158e+307;
                    else
                        y = y * twopk;
                    return y - one;
                }
                t = one;
                if (k < 20)
                {
                    t = FdSetHighWord(t, 0x3FF00000 - (0x200000 >> k)); /* t=1-2^-k */
                    y = t - (e - x);
                    y = y * twopk;
                }
                else
                {
                    t = FdSetHighWord(t, ((0x3FF - k) << 20)); /* 2^-k */
                    y = x - (e + t);
                    y += one;
                    y = y * twopk;
                }
            }
            return y;
        }

        /* Tanh(x)
         * Return the Hyperbolic Tangent of x
         *
         * Method :
         *                                 x    -x
         *                                e  - e
         *  0. tanh(x) is defined to be -----------
         *                                 x    -x
         *                                e  + e
         *  1. reduce x to non-negative by tanh(-x) = -tanh(x).
         *  2.  0      <= x <  2**-28 : tanh(x) := x with inexact if x != 0
         *                                          -t
         *      2**-28 <= x <  1      : tanh(x) := -----; t = expm1(-2x)
         *                                         t + 2
         *                                               2
         *      1      <= x <  22     : tanh(x) := 1 - -----; t = expm1(2x)
         *                                             t + 2
         *      22     <= x <= INF    : tanh(x) := 1.
         *
         * Special cases:
         *      tanh(NaN) is NaN;
         *      only tanh(0)=0 is exact for finite argument.
         */
        public static double Tanh(double x)
        {
            const double tiny = 1.0e-300;
            const double one = 1.0, two = 2.0, huge = 1.0e300;
            double t, z;
            int jx, ix;

            jx = HighWord(x);
            ix = jx & 0x7FFFFFFF;

            /* x is INF or NaN */
            if (ix >= 0x7FF00000)
            {
                if (jx >= 0)
                    return one / x + one; /* tanh(+-inf)=+-1 */
                else
                    return one / x - one; /* tanh(NaN) = NaN */
            }

            /* |x| < 22 */
            if (ix < 0x40360000)
            { /* |x|<22 */
                if (ix < 0x3E300000)
                { /* |x|<2**-28 */
                    if (huge + x > one) return x; /* tanh(tiny) = tiny with inexact */
                }
                if (ix >= 0x3FF00000)
                { /* |x|>=1  */
                    t = Expm1(two * Math.Abs(x));
                    z = one - two / (t + two);
                }
                else
                {
                    t = Expm1(-two * Math.Abs(x));
                    z = -t / (t + two);
                }
                /* |x| >= 22, return +-1 */
            }
            else
            {
                z = one - tiny; /* raise inexact flag */
            }
            return (jx >= 0) ? z : -z;
        }

        // --- Math.pow ------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Math.pow / the ** operator: V8 13.6 src/numbers/ieee754.cc math::pow with --use-std-math-pow (the default),
        /// up to the host call (NIGHT9: deterministic from there on; see the comment at the end).
        /// The ECMAScript cases and V8's y == 2 / y == 0.5 shortcuts are V8's own; where V8 then hands the input to the
        /// C runtime's std::pow (so V8's own last bit depends on the OS), this port is deterministic: integer exponents
        /// correctly rounded (exact), the rest fdlibm. vectors/jsMath.json records pow where V8's value doesn't depend on
        /// the host (port-export-p18 jsMath.mjs: the V8-decided and C99-exact cases, and integer exponents whose value is
        /// the correctly rounded one); every one matches.
        /// </summary>
        public static double Pow(double x, double y)
        {
            if (double.IsNaN(y))
            {
                // 1. If exponent is NaN, return NaN.
                return double.NaN;
            }
            if (double.IsInfinity(y) && (x == 1 || x == -1))
            {
                // 9. If exponent is +∞𝔽, then
                //   b. If abs(ℝ(base)) = 1, return NaN.
                // and
                // 10. If exponent is -∞𝔽, then
                //   b. If abs(ℝ(base)) = 1, return NaN.
                return double.NaN;
            }
            if (double.IsNaN(x))
            {
                // std::pow distinguishes between quiet and signaling NaN; JS doesn't.
                x = double.NaN;
            }

            // The following special cases just exist to match the optimizing compilers'
            // behavior, which avoid calls to `pow` in those cases.
            if (y == 2)
            {
                // x ** 2   ==>   x * x
                return x * x;
            }
            else if (y == 0.5)
            {
                // x ** 0.5   ==>  sqrt(x), except if x is -Infinity
                if (double.IsInfinity(x))
                {
                    return double.PositiveInfinity;
                }
                else
                {
                    // Note the +0 so that we get +0 for -0**0.5 rather than -0.
                    return Math.Sqrt(x + 0);
                }
            }

            // NIGHT9 U3-04: here V8 calls the host's std::pow (the UCRT on Windows, glibc on Linux, whose last bits differ;
            // Mono's and CoreCLR's Math.Pow differ again: 25 and 1 of the recorded inputs). JsMath stays deterministic
            // instead: an integer exponent (|y| <= 2048, x finite and nonzero: the only kind the sim passes, 0.6 ** n,
            // 0.96 ** n) gets the correctly rounded x^y, exact, which is glibc's result and the UCRT's on every recorded
            // input but its rare non-correctly-rounded ties; anything else gets V8's own fdlibm pow (legacy::pow below).
            if (!double.IsNaN(x) && !double.IsInfinity(x) && x != 0 && Math.Floor(y) == y && Math.Abs(y) <= 2048) return PowInt(x, (int)y);
            return PowFdlibm(x, y);
        }

        // NIGHT9: PowInt's results by (x, n); the sim asks the same few (style.diminish.repeat ** n), so after the first
        // time a call allocates nothing. Bounded: past the cap a result is computed and not kept.
        static readonly System.Collections.Generic.Dictionary<(long, int), double> powIntCache = new System.Collections.Generic.Dictionary<(long, int), double>();
        const int PowIntCacheCap = 4096;

        /// <summary>x ** n for an integer n and a finite nonzero x, correctly rounded (ties to even), from the exact value.</summary>
        static double PowInt(double x, int n)
        {
            long key = BitConverter.DoubleToInt64Bits(x);
            if (powIntCache.TryGetValue((key, n), out double hit)) return hit;
            bool neg = key < 0;
            int be = (int)((key >> 52) & 0x7FF);
            long mant = key & 0xFFFFFFFFFFFFFL;
            int e;
            if (be == 0) e = -1074;
            else
            {
                mant |= 1L << 52;
                e = be - 1075;
            }
            while ((mant & 1) == 0)
            {
                mant >>= 1;
                e++;
            }
            int an = Math.Abs(n);
            System.Numerics.BigInteger p = System.Numerics.BigInteger.Pow(mant, an); // |x|^|n| = p * 2^(e * |n|), exact
            long e2 = (long)e * an;
            double mag;
            if (an == 0) mag = 1;
            else if (n > 0) mag = RoundRational(p, System.Numerics.BigInteger.One, e2);
            else mag = RoundRational(System.Numerics.BigInteger.One, p, -e2);
            double result = neg && (an & 1) == 1 ? -mag : mag;
            if (powIntCache.Count < PowIntCacheCap) powIntCache[(key, n)] = result;
            return result;
        }

        /// <summary>The nearest double to num / den * 2^e2 (num, den > 0), ties to even, with subnormals, 0 and infinity.</summary>
        static double RoundRational(System.Numerics.BigInteger num, System.Numerics.BigInteger den, long e2)
        {
            // Leading-bit estimate of the value's exponent, then out of range answers at once.
            long lead = PowBitLength(num) - PowBitLength(den) + e2;
            if (lead > 1100) return double.PositiveInfinity;
            if (lead < -1200) return 0;
            int k = (int)(PowBitLength(num) - PowBitLength(den)) - 53; // q = floor(num * 2^-k / den) has 53 bits
            System.Numerics.BigInteger q, r, d;
            for (;;)
            {
                System.Numerics.BigInteger nn = k >= 0 ? num : num << -k;
                d = k >= 0 ? den << k : den;
                q = System.Numerics.BigInteger.DivRem(nn, d, out r);
                long bits = PowBitLength(q);
                if (bits > 53) { k++; continue; }
                if (bits < 53) { k--; continue; }
                break;
            }
            long exp = k + e2 + 52; // the exponent of q's leading bit
            if (exp < -1022)
            {
                // A subnormal (or 0): the nearest multiple of 2^-1074, straight from the exact fraction.
                long s = e2 + 1074;
                System.Numerics.BigInteger n2 = s >= 0 ? num << (int)s : num, d2 = s >= 0 ? den : den << (int)-s;
                System.Numerics.BigInteger sub = System.Numerics.BigInteger.DivRem(n2, d2, out System.Numerics.BigInteger rest);
                int c2 = (rest * 2).CompareTo(d2);
                if (c2 > 0 || (c2 == 0 && !sub.IsEven)) sub += 1;
                return BitConverter.Int64BitsToDouble((long)sub);
            }
            int c = (r * 2).CompareTo(d);
            if (c > 0 || (c == 0 && !q.IsEven)) q += 1;
            if (PowBitLength(q) == 54)
            {
                q >>= 1;
                exp++;
            }
            if (exp > 1023) return double.PositiveInfinity;
            return BitConverter.Int64BitsToDouble(((exp + 1023) << 52) | ((long)(ulong)q & 0xFFFFFFFFFFFFFL));
        }

        static long PowBitLength(System.Numerics.BigInteger v)
        {
            byte[] bytes = v.ToByteArray(); // little endian, two's complement (v >= 0 here)
            int last = bytes.Length - 1;
            while (last > 0 && bytes[last] == 0) last--;
            long n = (long)last * 8;
            for (int b = bytes[last]; b != 0; b >>= 1) n++;
            return n;
        }

        /*
         * ES2019 Draft 2019-01-02 12.6.4
         * Math.pow & Exponentiation Operator
         *
         * Return X raised to the Yth power
         *
         * Method:
         *     Let x =  2   * (1+f)
         *     1. Compute and return log2(x) in two pieces:
         *        log2(x) = w1 + w2,
         *        where w1 has 53-24 = 29 bit trailing zeros.
         *     2. Perform y*log2(x) = n+y' by simulating muti-precision
         *        arithmetic, where |y'|<=0.5.
         *     3. Return x**y = 2**n*exp(y'*log2)
         *
         * Special cases:
         *     1.  (anything) ** 0  is 1
         *     2.  (anything) ** 1  is itself
         *     3.  (anything) ** NAN is NAN
         *     4.  NAN ** (anything except 0) is NAN
         *     5.  +-(|x| > 1) **  +INF is +INF
         *     6.  +-(|x| > 1) **  -INF is +0
         *     7.  +-(|x| < 1) **  +INF is +0
         *     8.  +-(|x| < 1) **  -INF is +INF
         *     9.  +-1         ** +-INF is NAN
         *     10. +0 ** (+anything except 0, NAN)               is +0
         *     11. -0 ** (+anything except 0, NAN, odd integer)  is +0
         *     12. +0 ** (-anything except 0, NAN)               is +INF
         *     13. -0 ** (-anything except 0, NAN, odd integer)  is +INF
         *     14. -0 ** (odd integer) = -( +0 ** (odd integer) )
         *     15. +INF ** (+anything except 0,NAN) is +INF
         *     16. +INF ** (-anything except 0,NAN) is +0
         *     17. -INF ** (anything)  = -0 ** (-anything)
         *     18. (-anything) ** (integer) is (-1)**(integer)*(+anything**integer)
         *     19. (-anything except 0 and inf) ** (non-integer) is NAN
         *
         * Accuracy:
         *      pow(x,y) returns x**y nearly rounded. In particular,
         *      pow(integer, integer) always returns the correct integer provided it is
         *      representable.
         *
         * Constants:
         *     The hexadecimal values are the intended ones for the following
         *     constants. The decimal values may be used, provided that the
         *     compiler will convert from decimal to binary accurately enough
         *     to produce the hexadecimal values shown.
         */
        static readonly double[] PowBp = { 1.0, 1.5 };
        static readonly double[] PowDpH = { 0.0, 5.84962487220764160156e-01 }; // 0x3FE2B803, 0x40000000
        static readonly double[] PowDpL = { 0.0, 1.35003920212974897128e-08 }; // 0x3E4CFDEB, 0x43CFD006

        /// <summary>
        /// V8's fdlibm pow (src/base/ieee754.cc legacy::pow, what `node --no-use-std-math-pow` runs). Pure C#, the same
        /// on every OS, but V8 13.6's Math.pow is std::pow by default (see Pow), so this is not bit-exact with Node.
        /// </summary>
        public static double PowFdlibm(double x, double y)
        {
            double[] bp = PowBp, dp_h = PowDpH, dp_l = PowDpL;
            const double zero = 0.0, one = 1.0, two = 2.0,
                two53 = 9007199254740992.0, // 0x43400000, 0x00000000
                huge = 1.0e300, tiny = 1.0e-300,
                // poly coefs for (3/2)*(log(x)-2s-2/3*s**3
                L1 = 5.99999999999994648725e-01, // 0x3FE33333, 0x33333303
                L2 = 4.28571428578550184252e-01, // 0x3FDB6DB6, 0xDB6FABFF
                L3 = 3.33333329818377432918e-01, // 0x3FD55555, 0x518F264D
                L4 = 2.72728123808534006489e-01, // 0x3FD17460, 0xA91D4101
                L5 = 2.30660745775561754067e-01, // 0x3FCD864A, 0x93C9DB65
                L6 = 2.06975017800338417784e-01, // 0x3FCA7E28, 0x4A454EEF
                P1 = 1.66666666666666019037e-01, // 0x3FC55555, 0x5555553E
                P2 = -2.77777777770155933842e-03, // 0xBF66C16C, 0x16BEBD93
                P3 = 6.61375632143793436117e-05, // 0x3F11566A, 0xAF25DE2C
                P4 = -1.65339022054652515390e-06, // 0xBEBBBD41, 0xC5D26BF1
                P5 = 4.13813679705723846039e-08, // 0x3E663769, 0x72BEA4D0
                lg2 = 6.93147180559945286227e-01, // 0x3FE62E42, 0xFEFA39EF
                lg2_h = 6.93147182464599609375e-01, // 0x3FE62E43, 0x00000000
                lg2_l = -1.90465429995776804525e-09, // 0xBE205C61, 0x0CA86C39
                ovt = 8.0085662595372944372e-0017, // -(1024-log2(ovfl+.5ulp))
                cp = 9.61796693925975554329e-01, // 0x3FEEC709, 0xDC3A03FD =2/(3ln2)
                cp_h = 9.61796700954437255859e-01, // 0x3FEEC709, 0xE0000000 =(float)cp
                cp_l = -7.02846165095275826516e-09, // 0xBE3E2FE0, 0x145B01F5 =tail cp_h
                ivln2 = 1.44269504088896338700e+00, // 0x3FF71547, 0x652B82FE =1/ln2
                ivln2_h = 1.44269502162933349609e+00, // 0x3FF71547, 0x60000000 =24b 1/ln2
                ivln2_l = 1.92596299112661746887e-08; // 0x3E54AE0B, 0xF85DDF44 =1/ln2 tail

            double z, ax, z_h, z_l, p_h, p_l;
            double y1, t1, t2, r, s, t, u, v, w;
            int i, j, k, yisint, n;
            int hx, hy, ix, iy;
            uint lx, ly;

            hx = HighWord(x);
            lx = FdLowWord(x);
            hy = HighWord(y);
            ly = FdLowWord(y);
            ix = hx & 0x7fffffff;
            iy = hy & 0x7fffffff;

            /* y==zero: x**0 = 1 */
            if (((uint)iy | ly) == 0) return one;

            /* +-NaN return x+y */
            if (ix > 0x7ff00000 || ((ix == 0x7ff00000) && (lx != 0)) || iy > 0x7ff00000 ||
                ((iy == 0x7ff00000) && (ly != 0)))
            {
                return x + y;
            }

            /* determine if y is an odd int when x < 0
             * yisint = 0 ... y is not an integer
             * yisint = 1 ... y is an odd int
             * yisint = 2 ... y is an even int
             */
            yisint = 0;
            if (hx < 0)
            {
                if (iy >= 0x43400000)
                {
                    yisint = 2; /* even integer y */
                }
                else if (iy >= 0x3ff00000)
                {
                    k = (iy >> 20) - 0x3ff; /* exponent */
                    if (k > 20)
                    {
                        j = (int)(ly >> (52 - k));
                        if ((j << (52 - k)) == unchecked((int)ly)) yisint = 2 - (j & 1);
                    }
                    else if (ly == 0)
                    {
                        j = iy >> (20 - k);
                        if ((j << (20 - k)) == iy) yisint = 2 - (j & 1);
                    }
                }
            }

            /* special value of y */
            if (ly == 0)
            {
                if (iy == 0x7ff00000)
                { /* y is +-inf */
                    if ((unchecked((uint)(ix - 0x3ff00000)) | lx) == 0)
                    {
                        return y - y; /* inf**+-1 is NaN */
                    }
                    else if (ix >= 0x3ff00000)
                    { /* (|x|>1)**+-inf = inf,0 */
                        return (hy >= 0) ? y : zero;
                    }
                    else
                    { /* (|x|<1)**-,+inf = inf,0 */
                        return (hy < 0) ? -y : zero;
                    }
                }
                if (iy == 0x3ff00000)
                { /* y is  +-1 */
                    if (hy < 0)
                    {
                        return one / x;
                    }
                    else
                    {
                        return x;
                    }
                }
                if (hy == 0x40000000) return x * x; /* y is  2 */
                if (hy == 0x3fe00000)
                { /* y is  0.5 */
                    if (hx >= 0)
                    { /* x >= +0 */
                        return Math.Sqrt(x);
                    }
                }
            }

            ax = Math.Abs(x);
            /* special value of x */
            if (lx == 0)
            {
                if (ix == 0x7ff00000 || ix == 0 || ix == 0x3ff00000)
                {
                    z = ax; /*x is +-0,+-inf,+-1*/
                    if (hy < 0) z = one / z; /* z = (1/|x|) */
                    if (hx < 0)
                    {
                        if (((ix - 0x3ff00000) | yisint) == 0)
                        {
                            /* (-1)**non-int is NaN */
                            z = double.NaN;
                        }
                        else if (yisint == 1)
                        {
                            z = -z; /* (x<0)**odd = -(|x|**odd) */
                        }
                    }
                    return z;
                }
            }

            n = (hx >> 31) + 1;

            /* (x<0)**(non-int) is NaN */
            if ((n | yisint) == 0)
            {
                return double.NaN;
            }

            s = one; /* s (sign of result -ve**odd) = -1 else = 1 */
            if ((n | (yisint - 1)) == 0) s = -one; /* (-ve)**(odd int) */

            /* |y| is huge */
            if (iy > 0x41e00000)
            { /* if |y| > 2**31 */
                if (iy > 0x43f00000)
                { /* if |y| > 2**64, must o/uflow */
                    if (ix <= 0x3fefffff) return (hy < 0) ? huge * huge : tiny * tiny;
                    if (ix >= 0x3ff00000) return (hy > 0) ? huge * huge : tiny * tiny;
                }
                /* over/underflow if x is not close to one */
                if (ix < 0x3fefffff) return (hy < 0) ? s * huge * huge : s * tiny * tiny;
                if (ix > 0x3ff00000) return (hy > 0) ? s * huge * huge : s * tiny * tiny;
                /* now |1-x| is tiny <= 2**-20, suffice to compute
                   log(x) by x-x^2/2+x^3/3-x^4/4 */
                t = ax - one; /* t has 20 trailing zeros */
                w = (t * t) * (0.5 - t * (0.3333333333333333333333 - t * 0.25));
                u = ivln2_h * t; /* ivln2_h has 21 sig. bits */
                v = t * ivln2_l - w * ivln2;
                t1 = u + v;
                t1 = FdSetLowWord(t1, 0);
                t2 = v - (t1 - u);
            }
            else
            {
                double ss, s2, s_h, s_l, t_h, t_l;
                n = 0;
                /* take care subnormal number */
                if (ix < 0x00100000)
                {
                    ax *= two53;
                    n -= 53;
                    ix = HighWord(ax);
                }
                n += ((ix) >> 20) - 0x3ff;
                j = ix & 0x000fffff;
                /* determine interval */
                ix = j | 0x3ff00000; /* normalize ix */
                if (j <= 0x3988E)
                {
                    k = 0; /* |x|<sqrt(3/2) */
                }
                else if (j < 0xBB67A)
                {
                    k = 1; /* |x|<sqrt(3)   */
                }
                else
                {
                    k = 0;
                    n += 1;
                    ix -= 0x00100000;
                }
                ax = FdSetHighWord(ax, ix);

                /* compute ss = s_h+s_l = (x-1)/(x+1) or (x-1.5)/(x+1.5) */
                u = ax - bp[k]; /* bp[0]=1.0, bp[1]=1.5 */
                v = one / (ax + bp[k]);
                ss = u * v;
                s_h = ss;
                s_h = FdSetLowWord(s_h, 0);
                /* t_h=ax+bp[k] High */
                t_h = zero;
                t_h = FdSetHighWord(t_h, ((ix >> 1) | 0x20000000) + 0x00080000 + (k << 18));
                t_l = ax - (t_h - bp[k]);
                s_l = v * ((u - s_h * t_h) - s_h * t_l);
                /* compute log(ax) */
                s2 = ss * ss;
                r = s2 * s2 * (L1 + s2 * (L2 + s2 * (L3 + s2 * (L4 + s2 * (L5 + s2 * L6)))));
                r += s_l * (s_h + ss);
                s2 = s_h * s_h;
                t_h = 3.0 + s2 + r;
                t_h = FdSetLowWord(t_h, 0);
                t_l = r - ((t_h - 3.0) - s2);
                /* u+v = ss*(1+...) */
                u = s_h * t_h;
                v = s_l * t_h + t_l * ss;
                /* 2/(3log2)*(ss+...) */
                p_h = u + v;
                p_h = FdSetLowWord(p_h, 0);
                p_l = v - (p_h - u);
                z_h = cp_h * p_h; /* cp_h+cp_l = 2/(3*log2) */
                z_l = cp_l * p_h + p_l * cp + dp_l[k];
                /* log2(ax) = (ss+..)*2/(3*log2) = n + dp_h + z_h + z_l */
                t = (double)n;
                t1 = (((z_h + z_l) + dp_h[k]) + t);
                t1 = FdSetLowWord(t1, 0);
                t2 = z_l - (((t1 - t) - dp_h[k]) - z_h);
            }

            /* split up y into y1+y2 and compute (y1+y2)*(t1+t2) */
            y1 = y;
            y1 = FdSetLowWord(y1, 0);
            p_l = (y - y1) * t1 + y * t2;
            p_h = y1 * t1;
            z = p_l + p_h;
            j = HighWord(z);
            i = unchecked((int)FdLowWord(z));
            if (j >= 0x40900000)
            { /* z >= 1024 */
                if (((j - 0x40900000) | i) != 0)
                { /* if z > 1024 */
                    return s * huge * huge; /* overflow */
                }
                else
                {
                    if (p_l + ovt > z - p_h) return s * huge * huge; /* overflow */
                }
            }
            else if ((j & 0x7fffffff) >= 0x4090cc00)
            { /* z <= -1075 */
                if ((unchecked((uint)j - 0xc090cc00u) | (uint)i) != 0)
                { /* z < -1075 */
                    return s * tiny * tiny; /* underflow */
                }
                else
                {
                    if (p_l <= z - p_h) return s * tiny * tiny; /* underflow */
                }
            }
            /*
             * compute 2**(p_h+p_l)
             */
            i = j & 0x7fffffff;
            k = (i >> 20) - 0x3ff;
            n = 0;
            if (i > 0x3fe00000)
            { /* if |z| > 0.5, set n = [z+0.5] */
                n = j + (0x00100000 >> (k + 1));
                k = ((n & 0x7fffffff) >> 20) - 0x3ff; /* new k for n */
                t = zero;
                t = FdSetHighWord(t, n & ~(0x000fffff >> k));
                n = ((n & 0x000fffff) | 0x00100000) >> (20 - k);
                if (j < 0) n = -n;
                p_h -= t;
            }
            t = p_l + p_h;
            t = FdSetLowWord(t, 0);
            u = t * lg2_h;
            v = (p_l - (t - p_h)) * lg2 + t * lg2_l;
            z = u + v;
            w = v - (z - u);
            t = z * z;
            t1 = z - t * (P1 + t * (P2 + t * (P3 + t * (P4 + t * P5))));
            r = (z * t1) / ((t1 - two) - (w + z * w));
            z = one - (r - z);
            j = HighWord(z);
            j += unchecked((int)((uint)n << 20));
            if ((j >> 20) <= 0)
            {
                z = FdScalbn(z, n); /* subnormal output */
            }
            else
            {
                int tmp = HighWord(z);
                z = FdSetHighWord(z, tmp + unchecked((int)((uint)n << 20)));
            }
            return s * z;
        }
    }
}
