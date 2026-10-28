// NIGHT21 (the shared commit, the lead owns this file): the six NIGHT21 design blocks in the sim (Core/N21Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n21Override), --off21 / -skygrindOff21 (and --off20,
// --off19) take every block off (N21Blocks.ForceOff), otherwise the design defaults. The builders hook in through the
// partial methods below (nothing runs without a body); each is called once at the run's start or once per step, in the
// web's simulate() order, from the lead's call sites (SurvivalSim.Run.cs).
// - M (moves) implements N21ArclineStartRun / N21ArclineStep (Game/SurvivalSim.Arcline21.cs) and N21SlideBiteStep.
// - S (style and rounds) implements N21StampStartRun / N21StampStep and N21DeadlineStartRun / N21DeadlineStep.
// - W (world) implements N21GrindStartRun / N21GrindStep (Game/SurvivalSim.Grind21.cs).
namespace Skygrind.Game
{
    using Skygrind.Core;

    public sealed partial class SurvivalSim
    {
        /// <summary>The design config's NIGHT21 blocks, read once.</summary>
        N21Blocks n21Design;

        /// <summary>A tape's replay: the blocks its run read; null: the design defaults (or every block off with ForceOff).</summary>
        public N21Blocks? n21Override;

        /// <summary>The NIGHT21 blocks this run read (what a tape records).</summary>
        public N21Blocks n21;

        void N21Init() => n21Design = N21Blocks.From(cfg);

        void N21StartRun()
        {
            n21 = n21Override ?? (N21Blocks.ForceOff ? N21Blocks.Off : n21Design);
            N21ArclineStartRun();
            N21StampStartRun();
            N21DeadlineStartRun();
            N21GrindStartRun();
        }

        partial void N21ArclineStartRun();
        partial void N21ArclineStep(double dt);
        partial void N21SlideBiteStep(double dt);
        partial void N21StampStartRun();
        partial void N21StampStep(double dt);
        partial void N21DeadlineStartRun();
        partial void N21DeadlineStep(double dt);
        partial void N21GrindStartRun();
        partial void N21GrindStep(double dt);
    }
}
