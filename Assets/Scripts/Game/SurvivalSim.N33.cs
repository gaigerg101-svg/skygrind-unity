// NIGHT33 (the shared commit, the lead owns this file): the seven sim-changing NIGHT33 design blocks in the sim (Core/N33Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n33Override), --off33 / -skygrindOff33 (and every older off switch)
// take every block off (N33Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (a partial method with no body costs nothing; the call sites in SurvivalSim.Run.cs are the lead's):
// - E (enemies) implements N33AiStartRun / N33AiStep / N33AfterCrowd (Game/SurvivalSim.Ai33.cs): ai33 (the lava mask is filled at the
//   run's start; the edge guard runs after brain.Separate, so it sees the crowd's pushes); N33SpawnsStartRun / N33SpawnsStep: spawns33;
//   N33BossStartRun / N33BossStep: bosses33.
// - W (whip, piercer) implements N33WhipStartRun / N33WhipFrame / N33WhipStep (Game/SurvivalSim.Whip33.cs): whip33 (the frame hook runs
//   after N32GrappleFrame, before the Mover reads the frame); N33PierceStartRun / N33PierceStep: pierce33.
// - R (devices) implements N33DevicesStartRun / N33DevicesMove / N33DevicesStep (Game/SurvivalSim.Devices33.cs): devices33
//   (N33DevicesMove runs right after the Mover and the jump pads, the place a device pushes the player; never a Mover.cs edit).
// - P (shop) implements N33ShopStartRun / N33ShopAfterStart (Game/SurvivalSim.Shop33.cs): shop33. N33ShopAfterStart runs at the end
//   of StartRun, after run.Start and the HEAT rung (the economy and the health are set by then), and applies `shop33Owned` and
//   `bank33` only while shop33 is on.
// The shop's two run inputs: the game sets `shop33Owned` (the owned upgrades, P's encoding) and `bank33` (the campaign's banked points)
// before a campaign StartRun; a tape records both and its replay hands them back; the fingerprint runs with both 0.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N33Blocks n33Design;

        /// <summary>A replay's or a harness's NIGHT33 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N33Blocks? n33Override;

        /// <summary>The NIGHT33 blocks the current run reads (set at StartRun).</summary>
        public N33Blocks n33;

        /// <summary>NIGHT33 P: the shop's owned upgrades this run starts with (P's encoding; 0 = none). Set by the game before a campaign
        /// StartRun and by a tape's replay; read only while n33.shop33 is on.</summary>
        public int shop33Owned;

        /// <summary>NIGHT33 P: the campaign's banked points this run starts with (0 = none). Set like shop33Owned; read only while
        /// n33.shop33 is on.</summary>
        public int bank33;

        void N33Init() => n33Design = N33Blocks.From(cfg);

        void N33StartRun()
        {
            n33 = n33Override ?? (N33Blocks.ForceOff ? N33Blocks.Off : n33Design);
            N33AiStartRun();
            N33SpawnsStartRun();
            N33BossStartRun();
            N33WhipStartRun();
            N33PierceStartRun();
            N33DevicesStartRun();
            N33ShopStartRun();
        }

        partial void N33AiStartRun();
        partial void N33AiStep(double dt);
        partial void N33AfterCrowd(double dt); // NIGHT33 E: after brain.Separate (the crowd's pushes), the lava edge guard
        partial void N33SpawnsStartRun();
        partial void N33SpawnsStep(double dt);
        partial void N33BossStartRun();
        partial void N33BossStep(double dt);
        partial void N33WhipStartRun();
        partial void N33WhipFrame(ref InputFrame frame); // NIGHT33 W: before mover.Step, after N32GrappleFrame (SurvivalSim.Run.cs Step)
        partial void N33WhipStep(double dt);
        partial void N33PierceStartRun();
        partial void N33PierceStep(double dt);
        partial void N33DevicesStartRun();
        partial void N33DevicesMove(double dt); // NIGHT33 R: right after mover.Step and the jump pads (SurvivalSim.Run.cs Step)
        partial void N33DevicesStep(double dt);
        partial void N33ShopStartRun();
        partial void N33ShopAfterStart(); // NIGHT33 P: the end of StartRun, after run.Start and heat.ApplyRung
    }
}
