// NIGHT38 (the shared commit, the lead owns this file): the eight sim-changing NIGHT38 design blocks in the sim (Core/N38Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n38Override), --off38 / -skygrindOff38 (and every older off switch) take
// every block off (N38Blocks.ForceOff), otherwise the design defaults. The level rows (foundry38 .. finale38) are flow only: they swap the
// level id in Curriculum25.N38Levels and need no hook here. The sim hooks a builder may fill (a partial with no body costs nothing; the
// call sites are the lead's, in SurvivalSim.Run.cs):
// - M (builder ride38): N38RideStartRun / N38RideStep. The Mover's own branch (Core/Mover.cs wallride35) reads n38.ride38 through the
//   sim's Mover input; the hook here only resets its per-run state.
// - S2 (builder stag38): N38StagStartRun / N38StagStep. THE STAG's lane walls and read pass live in Stag37* and gen-n37-iceline-boss.cjs.
// Every hook runs in simulate() order: the step hooks right after N37Step (SurvivalSim.Run.cs).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N38Blocks n38Design;

        /// <summary>A replay's or a harness's NIGHT38 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N38Blocks? n38Override;

        /// <summary>The NIGHT38 blocks the current run reads (set at StartRun).</summary>
        public N38Blocks n38;

        void N38Init() => n38Design = N38Blocks.From(cfg);

        void N38StartRun()
        {
            n38 = n38Override ?? (N38Blocks.ForceOff ? N38Blocks.Off : n38Design);
            N38RideStartRun();
            N38StagStartRun();
            N38SpireStartRun();
        }

        partial void N38RideStartRun(); // NIGHT38 M: ride38's per-run state (no body until M lands one)
        partial void N38RideStep(double dt); // NIGHT38 M: every step, after N37Step
        partial void N38StagStartRun(); // NIGHT38 S2: stag38's per-run state
        partial void N38StagStep(double dt); // NIGHT38 S2: every step, after N38RideStep
        partial void N38SpireStartRun(); // NIGHT38 C2: the spire38 gate meter's per-run state (SurvivalSim.N38.Spire.cs)
        partial void N38SpireStep(double dt); // NIGHT38 C2: the spire38 gate meter, every step, after N38StagStep

        /// <summary>NIGHT38: every step, after N37Step (SurvivalSim.Run.cs, the lead's line).</summary>
        void N38Step(double dt)
        {
            N38RideStep(dt);
            N38StagStep(dt);
            N38SpireStep(dt);
        }
    }
}
