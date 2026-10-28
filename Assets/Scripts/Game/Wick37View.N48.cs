// NIGHT48 builder W (level row climbTop48; NIGHT48.md G1): the top-third LIPS as the player sees them. The sim builds each lip as a Step part of
// THE STACK (Core/Enemies/Wick37Stack.cs BuildLips), so its slab is drawn with the steps (the ledge colour) and folds into the wall with them in
// P1 (Wick37View.N47.cs); here only its cyan edge (a floor you stand on is player-usable: DESIGN-RULES V-2), added to the steps' lip mesh at the
// stack's build. Nothing without lips (n47-foundry, --off48: NIGHT47's view). No gameplay number here; the trim sizes are NIGHT37's (UnityTuning).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Wick37View
    {
        /// <summary>From BuildStack (one line): each lip's inner edge as a cyan strip just under its top, in `lip` (the steps' lip mesh).</summary>
        void Lips48Trim(Wick37Stack s, Vec3 o, int ls, MeshKit lip)
        {
            for (int i = 0; i < s.lipTops.Count; i++)
            {
                double a00 = s.lipFrom[i], arc = s.lipTo[i] - s.lipFrom[i], sa = arc / ls, ly = s.lipTops[i];
                double rIn = s.LipInner / System.Math.Cos(sa / 2) - UnityTuning.Stack37TrimOff;
                for (int k = 0; k < ls; k++)
                {
                    double a0 = a00 + k * sa, a1 = a00 + (k + 1) * sa, am = a00 + (k + 0.5) * sa;
                    Vec3 nrm = new Vec3(-System.Math.Cos(am), 0, -System.Math.Sin(am));
                    Vec3 p0 = Polar(a0, rIn, ly - UnityTuning.Stack37TrimH) + o, p1 = Polar(a1, rIn, ly - UnityTuning.Stack37TrimH) + o, hh = new Vec3(0, UnityTuning.Stack37TrimH, 0);
                    lip.Quad(U(p0), U(p1), U(p1 + hh), U(p0 + hh), N(nrm), Color.white);
                }
            }
        }
    }
}
