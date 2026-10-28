// NIGHT19 U3-33.8 (builder V): critically damped half-life springs, ported from Daniel Holden's Spring-It-On
// (github.com/orangeduck/Spring-It-On, common.h: fast_negexp, halflife_to_damping, damper_exact,
// simple_spring_damper_exact, decay_spring_damper_exact, critical_spring_damper_exact), line for line in double precision.
// A spring here is set by its half-life (the time to cover half the distance to the goal), never overshoots, and settles
// without the rubbery wobble of an underdamped spring (the research's ranked pull 5; NIGHT19 banned pattern 4). Pure maths:
// no UnityEngine call, no allocation (HandsN19Tests run it under dotnet).
//
// The Spring-It-On licence (MIT), kept with the port as it requires:
//
// MIT License
//
// Copyright (c) 2021 Daniel Holden
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;

namespace Skygrind.Game
{
    /// <summary>Spring-It-On's half-life springs (common.h), in double precision.</summary>
    public static class HalfLifeSpring
    {
        const double Ln2 = 0.69314718056; // common.h's literal
        const double Eps = 1e-5; // common.h's default eps

        /// <summary>fast_negexp(x): a cheap e^-x (the Pade-like fit common.h uses).</summary>
        public static double FastNegexp(double x) => 1.0 / (1.0 + x + 0.48 * x * x + 0.235 * x * x * x);

        /// <summary>halflife_to_damping(halflife).</summary>
        public static double HalflifeToDamping(double halflife) => (4.0 * Ln2) / (halflife + Eps);

        /// <summary>damper_exact(x, g, halflife, dt): x eased toward g by its half-life over dt.</summary>
        public static double DamperExact(double x, double g, double halflife, double dt) =>
            x + (g - x) * (1.0 - FastNegexp((Ln2 * dt) / (halflife + Eps)));

        /// <summary>simple_spring_damper_exact(x, v, x_goal, halflife, dt): a critically damped spring toward x_goal.</summary>
        public static void SimpleSpringDamperExact(ref double x, ref double v, double xGoal, double halflife, double dt)
        {
            double y = HalflifeToDamping(halflife) / 2.0;
            double j0 = x - xGoal;
            double j1 = v + j0 * y;
            double eydt = FastNegexp(y * dt);
            x = eydt * (j0 + j1 * dt) + xGoal;
            v = eydt * (v - j1 * y * dt);
        }

        /// <summary>decay_spring_damper_exact(x, v, halflife, dt): the same toward 0 (an impulse that settles).</summary>
        public static void DecaySpringDamperExact(ref double x, ref double v, double halflife, double dt)
        {
            double y = HalflifeToDamping(halflife) / 2.0;
            double j1 = v + x * y;
            double eydt = FastNegexp(y * dt);
            x = eydt * (x + j1 * dt);
            v = eydt * (v - j1 * y * dt);
        }

        /// <summary>critical_spring_damper_exact(x, v, x_goal, v_goal, halflife, dt): toward a moving goal.</summary>
        public static void CriticalSpringDamperExact(ref double x, ref double v, double xGoal, double vGoal, double halflife, double dt)
        {
            double g = xGoal;
            double q = vGoal;
            double d = HalflifeToDamping(halflife);
            double c = g + (d * q) / ((d * d) / 4.0);
            double y = d / 2.0;
            double j0 = x - c;
            double j1 = v + j0 * y;
            double eydt = FastNegexp(y * dt);
            x = eydt * (j0 + j1 * dt) + c;
            v = eydt * (v - j1 * y * dt);
        }

        /// <summary>
        /// Port-only: the velocity a decay spring must start with to peak at `peak` (its first and only extremum, reached at
        /// t = 1 / y: x(t) = v0 t e^(-y t), so the peak is v0 / (y e)). The kick's size is set as a peak, not a speed.
        /// </summary>
        public static double ImpulseForPeak(double peak, double halflife)
        {
            double y = HalflifeToDamping(halflife) / 2.0;
            return peak * y * Math.E;
        }

        /// <summary>Port-only: when a decay spring kicked from rest peaks (1 / y, s).</summary>
        public static double PeakTime(double halflife) => 2.0 / HalflifeToDamping(halflife);
    }

    /// <summary>
    /// A bank of critically damped impulse channels (each x, v decaying to 0 by its half-life): a kick adds velocity sized
    /// so each channel peaks at its given amount, the spring settles without overshoot. Fixed size, no allocation.
    /// </summary>
    public sealed class HalfLifeKick
    {
        readonly double[] x, v;
        public readonly int Channels;
        public double halflife;

        public HalfLifeKick(int channels, double halflife)
        {
            Channels = channels;
            x = new double[channels];
            v = new double[channels];
            this.halflife = halflife;
        }

        /// <summary>A kick: channel i peaks near peaks[i] (from rest), on top of what is already moving.</summary>
        public void Kick(double[] peaks, double scale = 1)
        {
            for (int i = 0; i < Channels && i < peaks.Length; i++) v[i] += HalfLifeSpring.ImpulseForPeak(peaks[i] * scale, halflife);
        }

        public void Step(double dt)
        {
            if (!(dt > 0)) return;
            for (int i = 0; i < Channels; i++)
            {
                double xi = x[i], vi = v[i];
                HalfLifeSpring.DecaySpringDamperExact(ref xi, ref vi, halflife, dt);
                x[i] = xi;
                v[i] = vi;
            }
        }

        public double Value(int channel) => x[channel];

        /// <summary>Any channel still moving (above the floor)?</summary>
        public bool Active
        {
            get
            {
                for (int i = 0; i < Channels; i++) if (Math.Abs(x[i]) > 1e-4 || Math.Abs(v[i]) > 1e-3) return true; // 0.1 mm / 1e-4 deg: nothing a screen shows
                return false;
            }
        }

        public void Clear()
        {
            Array.Clear(x, 0, x.Length);
            Array.Clear(v, 0, v.Length);
        }
    }
}
