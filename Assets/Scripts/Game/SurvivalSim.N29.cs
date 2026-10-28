// NIGHT29 (the shared commit, the lead owns this file): the six sim-changing NIGHT29 design blocks in the sim (Core/N29Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n29Override), --off29 / -skygrindOff29 (and every older off switch)
// take every block off (N29Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (a partial method with no body costs nothing; the call sites in SurvivalSim.Run.cs are the lead's):
// - S (systems) implements N29SystemsStartRun / N29SystemsStep (Game/SurvivalSim.Systems29.cs): lockout, shotgun, tank, doorFree.
// - B (bosses) implements N29BossStartRun / N29BossStep (Game/SurvivalSim.Boss29.cs): boss1, boss2.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N29Blocks n29Design;

        /// <summary>A replay's or a harness's NIGHT29 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N29Blocks? n29Override;

        /// <summary>The NIGHT29 blocks the current run reads (set at StartRun).</summary>
        public N29Blocks n29;

        void N29Init() => n29Design = N29Blocks.From(cfg);

        void N29StartRun()
        {
            n29 = n29Override ?? (N29Blocks.ForceOff ? N29Blocks.Off : n29Design);
            N29SystemsStartRun();
            N29BossStartRun();
        }

        partial void N29SystemsStartRun();
        partial void N29SystemsStep(double dt);
        partial void N29BossStartRun();
        partial void N29BossStep(double dt);
    }
}
