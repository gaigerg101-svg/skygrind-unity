// NIGHT38 builder S2 (stag38; NIGHT38-BRIEF card S2, PLAN.md section 5; GG 2026-10-13: "The middle walls are a little too short and clunky you
// can't really use them"): fills the lead's partial N38StagStartRun (SurvivalSim.N38.cs). THE CANAL is built at level load by N37StagStartRun
// (SurvivalSim.Stag37.cs) BEFORE the run's NIGHT38 blocks are known (N38StartRun runs after N37StartRun), so here the canal is rebuilt when the
// arena in hand does not match the run's stag38 bit: with the row on, Stag37ArenaParams.From(cfg, true) takes design.n38.stag38's lane walls (5 m
// tall, runs of 30 m+) and lists the island's faces as ride walls; with it off, NIGHT37's canal (a cached stag38 arena from an earlier run is
// dropped). THE STAG's fight logic is untouched (the fight takes the arena at its begin, after this). No per-step work: N38StagStep has no body.
// No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>NIGHT38 S2: how many times a run's start rebuilt the canal for the stag38 bit (the harness reads it).</summary>
        public int stag38Rebuilds;

        partial void N38StagStartRun()
        {
            bool want = n38.stag38;
            if (stag37Arena == null || stag37Arena.p.is38 == want) return; // nothing built yet (the next build reads the bit), or it matches
            stag37Arena = new Stag37Arena(Stag37ArenaParams.From(cfg, want));
            if (stag37Centre == null) return; // no canal on this level: the next Stag37Place builds from the new arena
            Stag37Build();
            stag38Rebuilds++;
        }
    }
}
