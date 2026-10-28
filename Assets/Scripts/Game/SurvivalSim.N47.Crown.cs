// NIGHT47 builder L (NIGHT47.md G41 / G43 boss part; Research/2026-10-18-wave47/L-cards.md section 2): the sim side of crown47.
//   crown47  THE CROWN rebuilt (Core/Enemies/Crown37.N47.cs): at each fight's begin (Crown47Begin, one line in Crown37BossBegin before the kit's
//            Start) the row's flag and numbers go into THE CROWN: chip outside the window, the window's length and gem cap, the summit kept for
//            phase 1, CROWN FIRE (the boss flame) and BELL TOLL in the brain's list, the GEM SHARD's longer tell. The fall rule (Crown37BossStep,
//            one line): with the row on a fall is CAUGHT (Crown47Catch: the leg's pieces back, the leg's checkpoint, fallHp; the brain keeps its
//            state: no leg restart). VERBS 8 (the jump-dash: the pass), 7 (the slam) and 9 (the ride) are untouched by this row: it changes the
//            boss's rules and the fall's cost, never the Mover.
// Off (the row off, --off47 / -skygrindOff47): Crown47Begin hands THE CROWN `false` (its list and numbers NIGHT46's), the fall rule is NIGHT46's.
// No allocation per step. No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        Crown47Params crown47Params;

        partial void N47CrownStartRun()
        {
            c47WasDying = false;
            crown47Finales = 0;
        }

        bool c47WasDying;
        /// <summary>The harness's counter (this run): finale beats held.</summary>
        public int crown47Finales;

        partial void N47CrownStep(double dt)
        {
            // THE FINALE BEAT (as THE LOOM's, SurvivalSim.N47.Loom.cs): the step THE CROWN starts to die, a numbers.finaleS hold
            bool dying = crown37 != null && crown37.Dying;
            if (dying && !c47WasDying && crown37.crown47 && crown47Params != null && crown47Params.finaleS > 0) { Hitstop(crown47Params.finaleS, "crown47Finale"); events.Emit("crown47Finale", "kill"); crown47Finales++; }
            c47WasDying = dying;
        }

        /// <summary>At THE CROWN's fight begin (Crown37BossBegin, a fresh fight, before the kit's Start): crown47 in or out.</summary>
        void Crown47Begin()
        {
            if (crown37 == null) return;
            bool on = rows.On(idCrown47);
            if (on && crown47Params == null) crown47Params = Crown47Params.From(cfg);
            crown37.Crown47(on, crown47Params);
        }

        /// <summary>crown47's fall: caught on the leg's checkpoint, the leg not restarted.</summary>
        Vec3 Crown47Catch(int leg)
        {
            crown37.Catch47();
            return spire37Checkpoint[System.Math.Max(0, System.Math.Min(2, leg - 1))];
        }
    }
}
