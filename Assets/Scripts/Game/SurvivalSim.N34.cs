// NIGHT34 (the shared commit, the lead owns this file): the four sim-changing NIGHT34 design blocks in the sim (Core/N34Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n34Override), --off34 / -skygrindOff34 (and every older off switch)
// take every block off (N34Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (a partial method with no body costs nothing; the call sites in SurvivalSim.Run.cs are the lead's):
// - B (the bend) implements N34BendStartRun / N34BendBeginStep / N34BendStep (Game/SurvivalSim.Bend34.cs): bend34 and bendSplit34.
//   N34BendBeginStep runs at the top of every Step (after simStep++) and sets `n34HoldWorld` for the split path: true on a step where
//   the world block is skipped (the world steps 1 in N while bending). SurvivalSim.Run.cs wraps every WORLD call of Step in
//   `if (!n34HoldWorld)`, in place, nothing reordered (CLAUDE.md rule 4); with the hook empty or the block off it stays false, so Step
//   is NIGHT33's. N34BendStep runs every step after the NIGHT33 hooks (the player's clock), where the grapple / whip latches of this
//   step have fired. `Bend34Share` (1 = real time) is the global path's time share the loop reads (SurvivalGame.SlowAimFrame), exactly
//   as Lantern29SlowScale; B sets it, the view reads it and `Bend34Depth01` (0 .. 1, the overlay's input).
// - M (movement) implements N34FlowStartRun (Game/SurvivalSim.Flow34.cs): flow34 (its Mover.cs branches read the mover's own flags);
//   N34ReachStartRun: reach34 (the gauntlet's boss margin).
// The WORLD / PLAYER split of Step (NIGHT34.md B1; B proves each player-side hook by a harness test and asks the lead for a change by a
// patch): the world calls are core?.Step, N33DevicesMove, brain.Step, N20RoutesStep, projectiles.Step, husks.Step, the enemy / boss /
// director / level / campaign hooks, brain.Separate, N33AfterCrowd, traps.Step and StepMode (the round clock); every other call is the
// player's (input, the Mover, health, pads, assist, the loadout and the arsenal hooks, the grapple / whip / pierce / fist / combat /
// weapons / trick / haptics / surface hooks, N30SystemsStep (lava, the glide rail), regen, buyables, style, power-ups, moments, contracts).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N34Blocks n34Design;

        /// <summary>A replay's or a harness's NIGHT34 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N34Blocks? n34Override;

        /// <summary>The NIGHT34 blocks the current run reads (set at StartRun).</summary>
        public N34Blocks n34;

        /// <summary>NIGHT34 B (the split path): true on a step whose world block is skipped. Set by N34BendBeginStep each step; false
        /// with bendSplit34 off.</summary>
        bool n34HoldWorld;

        /// <summary>NIGHT34 B (the global path): the world's time share the loop applies (1 = real time). Visual pacing only: the sim
        /// steps at 120 Hz, fewer per real second. Set by B's hooks; 1 at every run start.</summary>
        public double Bend34Share { get; private set; } = 1;

        /// <summary>NIGHT34 B: the bend's depth 0 (none) .. 1 (full), whichever path; the overlay's one input (H reads it).</summary>
        public double Bend34Depth01 { get; private set; }

        /// <summary>NIGHT34 (the lead's render line): the world views' animation pace inside a split bend, 1 - depth x (1 - 1/N); 1 outside one
        /// (and on the global path, where WorldScale already slows everything).</summary>
        public double Bend34WorldAnimMul => n34.bendSplit34 && Bend34Depth01 > 0 && bend34 != null && bend34.WorldN > 1 ? 1 - Bend34Depth01 * (1 - 1.0 / bend34.WorldN) : 1;

        void N34Init() => n34Design = N34Blocks.From(cfg);

        void N34StartRun()
        {
            n34 = n34Override ?? (N34Blocks.ForceOff ? N34Blocks.Off : n34Design);
            n34HoldWorld = false;
            Bend34Share = 1;
            Bend34Depth01 = 0;
            N34BendStartRun();
            N34FlowStartRun();
            N34ReachStartRun();
        }

        partial void N34BendStartRun();
        partial void N34BendBeginStep(); // NIGHT34 B: the top of Step, after simStep++: sets n34HoldWorld (the split path)
        partial void N34BendStep(double dt); // NIGHT34 B: every step, after the NIGHT33 hooks: the latches, the meter, the share
        partial void N34FlowStartRun();
        partial void N34ReachStartRun();
    }
}
