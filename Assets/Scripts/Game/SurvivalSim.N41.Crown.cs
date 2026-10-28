// NIGHT41 P3 (aim41; INVENTORY C2, DECIDE41 D-6): the pad's ASSIST mode finds a boss's weak point (Core/AimAssist.cs, Body).
//   N41AimStartRun  the run's aim41 bit into the assist (AimAssist.weakPoint41: Core takes the flag as a field set at a run's start), and
//                   (round 3) design.n41.aim41.reachM / acquireScale into AimAssist.weakReach41 / weakAcquire41 (0 / 1 with aim41 off).
//   N41CrownStep    THE CROWN's weak point (Enemy.weakPoint41 on Crown37.body): the ring's centre (its opening: the pass, the window's hits,
//                   leg 3's finisher) while it is passable: locked (a still plane within lockRadius) or in the CYAN window; cleared otherwise.
//                   With aim41 off it is never set (cleared every step), so nothing reads it: NIGHT40's game.
// No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        partial void N41AimStartRun()
        {
            if (assist == null) return;
            assist.weakPoint41 = n41.aim41;
            // round 3: a live weak point's own cone and acquire time (design.n41.aim41; neutral 0 / 1 when absent; read only while on)
            const string R = "design.n41.aim41.";
            assist.weakReach41 = n41.aim41 && cfg.Has(R + "reachM") ? cfg.Num(R + "reachM") : 0;
            assist.weakAcquire41 = n41.aim41 && cfg.Has(R + "acquireScale") ? cfg.Num(R + "acquireScale") : 1;
        }

        partial void N41CrownStep(double dt)
        {
            if (crown37 == null) return;
            if (n41.aim41 && crown37Owns && crown37.Alive && !crown37.Dying && (crown37.locked || crown37.Open)) crown37.body.weakPoint41.Set(crown37.centre);
            else crown37.body.weakPoint41.Clear();
        }
    }
}
