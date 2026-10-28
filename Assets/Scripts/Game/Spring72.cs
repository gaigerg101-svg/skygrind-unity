// NIGHT72 builder B (slates B1 body72 and B4 spring72): the one second-order spring the night's secondary motion uses, pure maths (no engine call), so an
// EditMode test pins it. Research/2026-10-27-forge72/scout-animation.md section 1 (t3ssel8r's second-order system; Orange Duck's spring roll call): a damped
// spring x'' = -2 z w x' - w^2 (x - target), w = 2 pi f. f in Hz is how fast it follows (1-3 Hz a body, 4-8 Hz a flinch), z the damping ratio (0.3-0.5 rings
// like jelly, 0.6-0.8 like plates, 1 critical). It is stepped on FRAME time (render only: no sim read, no timer; CLAUDE.md rule 4's loop is untouched) in sub-steps
// small enough to stay stable (w h <= 0.5).
using System;

namespace Skygrind.Game
{
    public static class Spring72Math
    {
        /// <summary>One frame of the spring: x and v advance toward `target` over `dt` seconds (semi-implicit Euler in sub-steps). dt 0 or less: nothing.</summary>
        public static void Step(ref float x, ref float v, float target, float hz, float zeta, float dt)
        {
            if (dt <= 0f || hz <= 0f) return;
            float w = 2f * (float)Math.PI * hz;
            int n = (int)Math.Ceiling(w * dt / 0.5f);
            if (n < 1) n = 1;
            if (n > 24) n = 24;
            float h = dt / n;
            for (int i = 0; i < n; i++)
            {
                v += (-2f * zeta * w * v - w * w * (x - target)) * h;
                x += v * h;
            }
        }

        /// <summary>The overshoot of a unit step as a share (0 for a damping ratio at or over 1): exp(-pi z / sqrt(1 - z^2)); z 0.3 is 0.37, 0.5 is 0.16.</summary>
        public static float Overshoot(float zeta)
        {
            if (zeta <= 0f) return 1f;
            if (zeta >= 1f) return 0f;
            return (float)Math.Exp(-Math.PI * zeta / Math.Sqrt(1.0 - (double)zeta * zeta));
        }

        /// <summary>The scale on each of the two other axes that keeps a body's volume when one axis is scaled by `stretch` (1 / sqrt(stretch)).</summary>
        public static float Volume(float stretch) => 1f / (float)Math.Sqrt(Math.Max(1e-4f, stretch));

        /// <summary>The steady lag of a spring that follows a target moving at `speed` (m/s): 2 z speed / w. A spring at 4 Hz, z 0.35 on a 10 m/s target trails 0.28 m.</summary>
        public static float SteadyLag(float speed, float hz, float zeta) => hz <= 0f ? 0f : 2f * zeta * speed / (2f * (float)Math.PI * hz);
    }
}
