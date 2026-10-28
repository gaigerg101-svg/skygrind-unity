// NIGHT27 builder V: the pure arithmetic of the skate feel (SkateFx27) and the roof's ambient occlusion (RoofView27), kept free of any scene so
// EditMode can test it. Every input is a number a caller reads from design.n27.<row>; nothing is typed here. Render only.
using System;

namespace Skygrind.Game
{
    public static class SkateMath27
    {
        static double Clamp01(double v) => v < 0 ? 0 : v > 1 ? 1 : v;

        /// <summary>Speed FOV add: 0 at run speed, maxDeg at fullMult x run speed (linear between), never more than cap.</summary>
        public static double FovAdd(double speed, double runSpeed, double fullMult, double maxDeg, double cap)
        {
            if (runSpeed <= 0 || fullMult <= 1) return 0;
            double t = Clamp01((speed / runSpeed - 1) / (fullMult - 1));
            return Math.Min(cap, maxDeg * t);
        }

        /// <summary>Roll target: clamp(lateral / max, -1, 1) x deg.</summary>
        public static double Roll(double lateral, double max, double deg)
        {
            if (max <= 0) return 0;
            double r = lateral / max;
            return (r < -1 ? -1 : r > 1 ? 1 : r) * deg;
        }

        /// <summary>A first-order smooth toward a target with time constant tau (frame-rate independent).</summary>
        public static double Smooth(double cur, double target, double dt, double tau) => tau <= 0 ? target : cur + (target - cur) * (1 - Math.Exp(-dt / tau));

        /// <summary>The same with different rates up and down (FOV: in 0.15 s, out 0.4 s): `target` above `cur` uses inS.</summary>
        public static double SmoothUpDown(double cur, double target, double dt, double inS, double outS) => Smooth(cur, target, dt, target > cur ? inS : outS);

        /// <summary>Streak strength: 0 at loShare of top speed, 1 at hiShare, linear between.</summary>
        public static double Strength(double speed, double top, double loShare, double hiShare)
        {
            if (top <= 0 || hiShare <= loShare) return 0;
            return Clamp01((speed / top - loShare) / (hiShare - loShare));
        }

        /// <summary>The AO factor at `dist` from a feature: `baseFactor` right at it (0.6 inside corner, 0.8 convex edge), easing linearly to 1 at `falloff`.</summary>
        public static double Ao(double baseFactor, double dist, double falloff) => falloff <= 0 ? 1 : baseFactor + (1 - baseFactor) * Clamp01(dist / falloff);

        /// <summary>The signed heading change in degrees between two horizontal velocities (0 when either is nearly still).</summary>
        public static double HeadingChangeDeg(double ax, double az, double bx, double bz)
        {
            double la = Math.Sqrt(ax * ax + az * az), lb = Math.Sqrt(bx * bx + bz * bz);
            if (la < 1e-3 || lb < 1e-3) return 0;
            double c = (ax * bx + az * bz) / (la * lb);
            c = c < -1 ? -1 : c > 1 ? 1 : c;
            return Math.Acos(c) * 180.0 / Math.PI;
        }

        /// <summary>The number of ceiling strips for a room of this perimeter: one per `perMetre` rounded, held to [min, max].</summary>
        public static int StripCount(double perimeter, double perMetre, int min, int max)
        {
            int n = (int)Math.Round(perimeter / Math.Max(1e-3, perMetre));
            return n < min ? min : n > max ? max : n;
        }
    }
}
