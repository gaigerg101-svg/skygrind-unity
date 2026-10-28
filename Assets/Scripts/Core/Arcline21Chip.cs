// NIGHT22 U3-36.3 (builder M): the ARCLINE chip's three looks as a pure rule (RESEARCH R7: cooldown and out-of-range are different
// looks; DECIDE22 D-3 / CLAUDE.md rule 8: none of them red). The view (Game/UiArcline22.cs) only draws what this says.
//   Hidden    the gadget is not bought (no chip, the reticle untouched)
//   Cooldown  the yank's 15 s cooldown is running (design.n21.arcline.yankCooldown): a dim arc that empties, and the seconds
//   NoTarget  off cooldown but the crosshair has nothing to hook (Arcline21.Reticle == None): the reticle dims to 40 %
//   Ready     off cooldown and the crosshair has something (an enemy to yank, a node, a surface)
// Cooldown wins over NoTarget (the seconds are the more useful number while it runs); the reticle's dim follows the reticle alone
// (ReticleDims), so a swing target still lights the crosshair during the cooldown.
using System;

namespace Skygrind.Core
{
    public enum Arcline22Look
    {
        Hidden,
        Ready,
        Cooldown,
        NoTarget,
    }

    public static class Arcline22Chip
    {
        /// <summary>The chip's look for a gadget state. `yankSteps`: Arcline21.YankSteps; `reticle`: Arcline21.Reticle(look).</summary>
        public static Arcline22Look Resolve(bool owned, int yankSteps, Arcline21Kind reticle)
        {
            if (!owned) return Arcline22Look.Hidden;
            if (yankSteps > 0) return Arcline22Look.Cooldown;
            return reticle == Arcline21Kind.None ? Arcline22Look.NoTarget : Arcline22Look.Ready;
        }

        /// <summary>Does the crosshair dim (to 40 % alpha, never red)? Owned, and the hook has nothing to bite right now.</summary>
        public static bool ReticleDims(bool owned, Arcline21Kind reticle) => owned && reticle == Arcline21Kind.None;

        /// <summary>The cooldown's seconds as the chip prints them: rounded up (a 15 s cooldown reads 15 at its start and 1 at its end).</summary>
        public static int Seconds(int yankSteps, double stepSeconds) => yankSteps <= 0 ? 0 : (int)Math.Ceiling(yankSteps * stepSeconds - 1e-9);

        /// <summary>The cooldown arc's fill, 1 at the press and 0 when it is ready (Arcline21.YankShare).</summary>
        public static double ArcFill(double yankShare) => Math.Max(0, Math.Min(1, yankShare));

        /// <summary>Eye points over the floor on a grid (centre first, then outward), for FindEmptyLook; valid(x, z) keeps the standable ones.</summary>
        public static System.Collections.Generic.List<Vec3> EyeGrid(MapBox floor, double eyeHeight, double step, Func<double, double, bool> valid)
        {
            var list = new System.Collections.Generic.List<Vec3>();
            double cx = (floor.min.x + floor.max.x) * 0.5, cz = (floor.min.z + floor.max.z) * 0.5;
            for (double x = floor.min.x + step; x <= floor.max.x - step; x += step)
                for (double z = floor.min.z + step; z <= floor.max.z - step; z += step)
                    if (valid == null || valid(x, z)) list.Add(new Vec3(x, floor.max.y + eyeHeight, z));
            list.Sort((a, b) => ((a.x - cx) * (a.x - cx) + (a.z - cz) * (a.z - cz)).CompareTo((b.x - cx) * (b.x - cx) + (b.z - cz) * (b.z - cz)));
            return list;
        }

        /// <summary>
        /// A place and a look at which the hook has nothing to bite (Arcline21.ReticleFrom == None): for each eye point, the sky and the
        /// horizon in 72 headings. The NO TARGET scenario's spot (the Player reel and the harness use the same search). False: none on
        /// this map from any of the points (the arena is walled: NO TARGET is rare there).
        /// </summary>
        public static bool FindEmptyLook(Arcline21 a, System.Collections.Generic.IList<Vec3> eyes, out Vec3 eye, out double yaw, out double pitch)
        {
            for (int e = 0; e < eyes.Count; e++)
            {
                for (double p = 1.55; p >= -0.3; p -= 0.05)
                {
                    for (int k = 0; k < 72; k++)
                    {
                        double y = k * Math.PI / 36;
                        if (a.ReticleFrom(eyes[e], new InputFrame { yaw = y, pitch = p }) == Arcline21Kind.None)
                        {
                            eye = eyes[e];
                            yaw = y;
                            pitch = p;
                            return true;
                        }
                    }
                }
            }
            eye = default;
            yaw = pitch = 0;
            return false;
        }
    }
}
