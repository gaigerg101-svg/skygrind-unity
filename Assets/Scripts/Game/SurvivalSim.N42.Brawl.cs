// NIGHT42 builder B1 (brawl42; NIGHT42.md card B1, G14): THE ICEBREAKER's new attacks in the sim. The brain is Core/Enemies/Brawl40.N42.cs;
// this hands it the run's flag (N42Blocks.brawl42) and its numbers (design.n42.brawl42) at the run's start and at each fight's begin
// (Brawl40Begin's one line). Off (the row off / --off42 / every older off switch): Arm42(false): NIGHT41's fight bit for bit.
// No UnityEngine: SimHarness compiles it.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        Brawl42Params brawl42Params;

        /// <summary>The brawler's brawl42 switch for this run (the numbers read once, the first time the row is on).</summary>
        void Brawl42Arm()
        {
            if (brawl40 == null) return;
            if (n42.brawl42 && brawl42Params == null) brawl42Params = Brawl42Params.From(cfg);
            brawl40.Arm42(n42.brawl42, n42.brawl42 ? brawl42Params : null);
        }

        partial void N42BrawlStartRun() => Brawl42Arm();
    }
}
