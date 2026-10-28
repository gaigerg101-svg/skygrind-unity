// NIGHT48 builder L (SurvivalSim.N48.cs's N48L hooks): the sim row loomReach48 (design.n48.loomReach48, DECIDE48 D-34): THE LOOM's hit test
// gets two capsules along its drawn hull's long axis (Core/Enemies/Loom37.cs Raycast / Reach48Ray), so a shot at a visible point lands. Off
// (or --off48 / -skygrindOff48): rows.On reads false, Loom37.reach48 stays false, the 3 m sphere alone (NIGHT47's test).
// Resolved once a run (StartRun); handed to the live LOOM each step (a bool and five doubles: no allocation).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        internal int idLoomReach48 = -1;
        double l48ReachR = 1.2, l48ReachInner = 0, l48ReachOuter = 5.6, l48ReachFollow = 0.35, l48ReachYawMax = 20;

        double L48Num(string key, double def) => cfg.Has("design.n48.loomReach48.numbers." + key) ? cfg.Num("design.n48.loomReach48.numbers." + key) : def;

        partial void N48LStartRun()
        {
            idLoomReach48 = rowsRegistry.IndexOf("loomReach48");
            l48ReachR = L48Num("radiusM", 1.2);
            l48ReachInner = L48Num("innerM", 0);
            l48ReachOuter = L48Num("outerM", 5.6);
            l48ReachFollow = L48Num("yawFollow", 0.35);
            l48ReachYawMax = L48Num("yawMaxDeg", 20);        }

        partial void N48LStep(double dt)
        {
            if (loom37 == null) return;
            bool on = rows.On(idLoomReach48);
            loom37.reach48 = on;
            if (!on) return;
            loom37.reach48R = l48ReachR;
            loom37.reach48Inner = l48ReachInner;
            loom37.reach48Outer = l48ReachOuter;
            loom37.reach48Follow = l48ReachFollow;
            loom37.reach48YawMax = l48ReachYawMax;        }
    }
}
