// NIGHT39 (the shared commit, the lead owns this file): the three sim-changing NIGHT39 design blocks in the sim (Core/N39Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n39Override), --off39 / -skygrindOff39 (and every older off switch) take
// every block off (N39Blocks.ForceOff), otherwise the design defaults. The sim hooks a builder may fill (a partial with no body costs
// nothing; the call sites are the lead's, in SurvivalSim.Run.cs):
// - P1 (p39): N39PistolStartRun / N39PistolStep. The charge rule itself lives in Core/Guns/Charge35.cs (P1's file).
// - R1 (ride39): N39RideStartRun. The numbers are read inside Core/Mover.cs's wallride35 branch (R1's).
// - B1 (boss39): N39BossStartRun. The per-boss overrides are read where each boss reads its numbers.
// The step hooks run right after N38Step (SurvivalSim.Run.cs).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N39Blocks n39Design;

        /// <summary>A replay's or a harness's NIGHT39 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N39Blocks? n39Override;

        /// <summary>The NIGHT39 blocks the current run reads (set at StartRun).</summary>
        public N39Blocks n39;

        void N39Init() => n39Design = N39Blocks.From(cfg);

        void N39StartRun()
        {
            n39 = n39Override ?? (N39Blocks.ForceOff ? N39Blocks.Off : n39Design);
            N39PistolStartRun();
            N39RideStartRun();
            N39BossStartRun();
        }

        partial void N39PistolStartRun(); // NIGHT39 P1
        partial void N39PistolStep(double dt); // NIGHT39 P1: every step, after N38Step
        partial void N39RideStartRun(); // NIGHT39 R1
        partial void N39BossStartRun(); // NIGHT39 B1
        partial void N39BossStep(double dt); // NIGHT39 B1: after N39PistolStep

        /// <summary>NIGHT39: every step, after N38Step (SurvivalSim.Run.cs, the lead's line).</summary>
        void N39Step(double dt)
        {
            N39PistolStep(dt);
            N39BossStep(dt);
        }
    }
}
