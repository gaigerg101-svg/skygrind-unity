// NIGHT25 (builder F, T-12): the short alpha fade-in shared by the M9 fixes (weapon-name swap, combo plate, hotbar cells, threat chunk, nav focus). Drawn at its age, so the first
// frame (age 0) already counts one frame of the ramp and is well under M9's 0.95; at UnityTuning.Fade25In the ramp is 1. Pure maths: no UnityEngine.
using System;

namespace Skygrind.Game
{
    public static class UiFade25
    {
        /// <summary>Alpha 0..1 at `age` seconds into the entrance; `frameDt` is the frame being drawn (the age counts the frame in progress).</summary>
        public static float In(double age, double frameDt)
        {
            if (age >= UnityTuning.Fade25In) return 1f;
            double a = (Math.Max(0, age) + Math.Max(0, frameDt)) / UnityTuning.Fade25In;
            return (float)Math.Min(1.0, Math.Max(0.0, a));
        }
    }
}
