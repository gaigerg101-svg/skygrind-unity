// NIGHT22 (the shared commit, the lead owns this file): the six NIGHT22 design blocks in the sim (Core/N22Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n22Override), --off22 / -skygrindOff22 (and --off21,
// --off20, --off19) take every block off (N22Blocks.ForceOff), otherwise the design defaults. The builders hook in through
// the partial methods below (nothing runs without a body); each is called once at the run's start or once per step, from the
// lead's call sites (SurvivalSim.Run.cs).
// - M (moves) implements N22HapticsStartRun / N22HapticsStep (Game/Haptics22*.cs hooks).
// - W2 implements N22GhostStartRun and N22DuskStep (Game/DuskClock22*.cs).
// - Any builder with a sim-changing leftover (directed, trickChain, overdrive) implements N22DirectedStep / N22TrickStep.
namespace Skygrind.Game
{
    using Skygrind.Core;

    public sealed partial class SurvivalSim
    {
        /// <summary>The design config's NIGHT22 blocks, read once.</summary>
        N22Blocks n22Design;

        /// <summary>A tape's replay: the blocks its run read; null: the design defaults (or every block off with ForceOff).</summary>
        public N22Blocks? n22Override;

        /// <summary>The NIGHT22 blocks this run read (what a tape records).</summary>
        public N22Blocks n22;

        void N22Init() => n22Design = N22Blocks.From(cfg);

        void N22StartRun()
        {
            n22 = n22Override ?? (N22Blocks.ForceOff ? N22Blocks.Off : n22Design);
            N22HapticsStartRun();
            N22GhostStartRun();
            N22DuskStartRun();
            N22DirectedStartRun();
            N22TrickStartRun();
        }

        partial void N22HapticsStartRun();
        partial void N22HapticsStep(double dt);
        partial void N22GhostStartRun();
        partial void N22DuskStartRun();
        partial void N22DuskStep(double dt);
        partial void N22DirectedStartRun();
        partial void N22DirectedStep(double dt);
        partial void N22TrickStartRun();
        partial void N22TrickStep(double dt);
    }
}
