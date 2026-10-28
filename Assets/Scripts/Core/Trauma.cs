// NIGHT8 U2-06: ports of skygrind/src/fx/trauma.js (NIGHT5 F2: trauma-based camera shake, Eiserloh GDC 2016) and the pure
// noise of fx/cosmetic.js (lattice, valueNoise). Frame time only (visual): trauma is 0..1, events add to it, it falls
// linearly at `decay` per s, and the shake is trauma^exponent x the max angle x smooth noise per axis. Rotation only
// (pitch, yaw, roll; never a translation), on the drawn camera only: the aim is taken before it. The noise is a pure
// function of the shake clock, so it draws from no random stream. Math.imul is C#'s unchecked int multiply; `>>>` is a
// shift on the uint.
using System;

namespace Skygrind.Core
{
    public static class Cosmetic
    {
        /// <summary>cosmetic.js lattice: a hash of an integer and a salt, in [-1, 1] (no state).</summary>
        public static double Lattice(int i, int salt)
        {
            unchecked
            {
                int h = (i * (int)0x9e3779b1) ^ ((salt + 0x632be5ab) * (int)0x85ebca77);
                h ^= (int)((uint)h >> 15);
                h = h * 0x2c1b3c6d;
                h ^= (int)((uint)h >> 12);
                h = h * 0x297a2d39;
                h ^= (int)((uint)h >> 15);
                return (uint)h / 4294967295.0 * 2 - 1;
            }
        }

        /// <summary>cosmetic.js valueNoise: smooth value noise in [-1, 1] (one lattice step per unit of t).</summary>
        public static double ValueNoise(double t, int salt)
        {
            double fi = Math.Floor(t);
            int i = (int)fi;
            double f = t - fi;
            double u = f * f * (3 - 2 * f);
            return Lattice(i, salt) * (1 - u) + Lattice(i + 1, salt) * u;
        }
    }

    public sealed class Trauma
    {
        public struct Params
        {
            public double maxPitchDeg, maxYawDeg, maxRollDeg, exponent, noiseHz, decay, cap;

            public static Params From(GameConfig c) => new Params
            {
                maxPitchDeg = c.Num("feel.trauma.maxPitchDeg"), maxYawDeg = c.Num("feel.trauma.maxYawDeg"), maxRollDeg = c.Num("feel.trauma.maxRollDeg"),
                exponent = c.Num("feel.trauma.exponent"), noiseHz = c.Num("feel.trauma.noiseHz"), decay = c.Num("feel.trauma.decay"), cap = c.Num("feel.trauma.cap"),
            };
        }

        double trauma, time;

        /// <summary>0..1</summary>
        public double Value => trauma;

        /// <summary>Add trauma (an event), capped. A shake from calm starts its noise clock at 0 (the same at any frame rate).</summary>
        public void Add(double amount, double cap = 1)
        {
            if (!(amount > 0)) return;
            if (trauma == 0) time = 0;
            trauma = Math.Min(cap, trauma + amount);
        }

        /// <summary>Fall `decay` per s and move the noise clock.</summary>
        public void Step(double dt, double decay)
        {
            trauma = Math.Max(0, trauma - decay * dt);
            time += dt;
        }

        /// <summary>The shake now in degrees: trauma^exponent x max x noise x scale (exactly 0 at no trauma or scale 0).</summary>
        public (double pitch, double yaw, double roll) Sample(Params c, double scale)
        {
            double s = trauma > 0 && scale > 0 ? JsMath.Pow(trauma, c.exponent) * scale : 0;
            double t = time * c.noiseHz;
            return s != 0
                ? (c.maxPitchDeg * s * Cosmetic.ValueNoise(t, 11), c.maxYawDeg * s * Cosmetic.ValueNoise(t, 23), c.maxRollDeg * s * Cosmetic.ValueNoise(t, 37))
                : (0, 0, 0);
        }

        public void Clear() => trauma = 0;
    }
}
