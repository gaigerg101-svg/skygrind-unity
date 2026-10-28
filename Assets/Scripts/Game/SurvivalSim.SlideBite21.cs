// NIGHT21 U3-35.4 (builder M owns this file): SLIDE BITES (design.n21.slideBites; the rule is Core/SlideBite21.cs).
// Here (not in a Game file of its own) because SimHarness compiles Game/SurvivalSim*.cs: the harness plays it.
// The run's start is the ARCLINE's StartRun hook (SurvivalSim.Arcline21.cs calls SlideBite21StartRun first: both are builder M's
// and the lead's StartRun has one hook for M); the step hook runs once per step after the loadout (SurvivalSim.Run.cs).
// Off (the row, or --off21 / --off20 / --off19): nothing is made, nothing runs: NIGHT20's game bit for bit.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT21: SLIDE BITES' rule (null until a run starts with the block on).</summary>
        public SlideBite21 slideBite21;

        void SlideBite21StartRun()
        {
            if (!n21.slideBites) return;
            slideBite21 ??= new SlideBite21(cfg, events, mover, enemies, loadout);
            slideBite21.Reset(); // a new run: no memory of the last run's step, no cooldowns
        }

        partial void N21SlideBiteStep(double dt)
        {
            if (!n21.slideBites || slideBite21 == null || !Active) return;
            slideBite21.Step();
        }
    }
}
