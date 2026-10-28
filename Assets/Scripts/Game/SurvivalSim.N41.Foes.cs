// NIGHT41 builder E6 (NIGHT41.md card E6; INVENTORY A5, C5, C7): the sim side of the enemies' NIGHT41 rows, in the lead's hooks
// N41FoesStartRun / N41FoesStep (SurvivalSim.N41.cs).
// - tokens41 (DECIDE41 D-5): at most design.n41.tokens41.cap enemies in an attack at once in a CAMPAIGN run, across the brain's token pools
//   (HuskBrain.windupCap41). Survival keeps the web's pools (the cap stays 0). Set every step: campaignOn is known only after N24StartRun,
//   which runs after N41StartRun, and a step's assignment costs nothing (no allocation).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        int tokens41Cap = -1; // design.n41.tokens41.cap, read once (-1: not read yet)

        partial void N41FoesStartRun()
        {
            if (Force41.HasValue) n41 = Force41.Value; // the harness's walks only (null in the game)
            if (tokens41Cap < 0) tokens41Cap = cfg.Has("design.n41.tokens41.cap") ? cfg.Int("design.n41.tokens41.cap") : 0;
            if (brain != null)
            {
                brain.windupCap41 = 0;
                brain.capWaits41 = 0;
            }
        }

        /// <summary>NIGHT41 E6: the harness's switch for walks that build their own sims (`n41foes walk`: R2's `n40ride walk` with tokens41 / kiln41
        /// forced): the run's NIGHT41 blocks, set in N41FoesStartRun (inside N41StartRun, so N41BaseId and every later reader see them); null (always,
        /// in the game and in a replay) = the run's own blocks.</summary>
        public static N41Blocks? Force41;

        partial void N41FoesStep(double dt)
        {
            if (brain != null) brain.windupCap41 = n41.tokens41 && campaignOn ? tokens41Cap : 0;
        }
    }
}
