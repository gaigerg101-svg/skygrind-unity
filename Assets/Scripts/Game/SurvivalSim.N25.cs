// NIGHT25 (the shared commit, the lead owns this file): the six NIGHT25 design blocks in the sim (Core/N25Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n25Override), --off25 / -skygrindOff25 (and --off24 and older)
// take every block off (N25Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (nothing runs without a body); each is called once at the run's start or once per step from the lead's call
// sites (SurvivalSim.Run.cs).
// - C (combat) implements N25CombatStartRun / N25CombatStep (Game/SurvivalSim.Combat25.cs).
// - W (the worm) implements N25WyrmStartRun / N25WyrmStep (Game/SurvivalSim.Wyrm25.cs).
// - L (levels) implements N25LevelStartRun / N25LevelStep (Game/SurvivalSim.Level25.cs).
namespace Skygrind.Game
{
    using Skygrind.Core;

    public sealed partial class SurvivalSim
    {
        /// <summary>The design config's NIGHT25 blocks, read once.</summary>
        N25Blocks n25Design;

        /// <summary>A tape's replay: the blocks its run read; null: the design defaults (or every block off with ForceOff).</summary>
        public N25Blocks? n25Override;

        /// <summary>The NIGHT25 blocks this run read (what a tape records).</summary>
        public N25Blocks n25;

        void N25Init() => n25Design = N25Blocks.From(cfg);

        void N25StartRun()
        {
            n25 = n25Override ?? (N25Blocks.ForceOff ? N25Blocks.Off : n25Design);
            N25CombatStartRun();
            N25WyrmStartRun();
            N25LevelStartRun();
        }

        partial void N25CombatStartRun();
        partial void N25CombatStep(double dt);
        partial void N25WyrmStartRun();
        partial void N25WyrmStep(double dt);
        partial void N25LevelStartRun();
        partial void N25LevelStep(double dt);
    }
}
