// NIGHT30 (the shared commit, the lead owns this file): the eight sim-changing NIGHT30 design blocks in the sim (Core/N30Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n30Override), --off30 / -skygrindOff30 (and every older off switch)
// take every block off (N30Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (a partial method with no body costs nothing; the call sites in SurvivalSim.Run.cs are the lead's):
// - S (systems) implements N30SystemsStartRun / N30SystemsStep (Game/SurvivalSim.Systems30.cs): rules, glide, lava, span, teach.
// - B (bosses) implements N30BossStartRun / N30BossStep (Game/SurvivalSim.Boss30.cs): lantern30, glacier30, wyrm30, the Loom.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N30Blocks n30Design;

        /// <summary>A replay's or a harness's NIGHT30 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N30Blocks? n30Override;

        /// <summary>The NIGHT30 blocks the current run reads (set at StartRun).</summary>
        public N30Blocks n30;

        void N30Init() => n30Design = N30Blocks.From(cfg);

        void N30StartRun()
        {
            n30 = n30Override ?? (N30Blocks.ForceOff ? N30Blocks.Off : n30Design);
            N30SystemsStartRun();
            N30BossStartRun();
        }

        partial void N30SystemsStartRun();
        partial void N30SystemsStep(double dt);
        partial void N30BossStartRun();
        partial void N30BossStep(double dt);
    }
}
