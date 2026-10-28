// NIGHT26 (the shared commit, the lead owns this file): the seven NIGHT26 design blocks in the sim (Core/N26Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n26Override), --off26 / -skygrindOff26 (and --off25 and older)
// take every block off (N26Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (nothing runs without a body); each is called once at the run's start or once per step from the lead's call
// sites (SurvivalSim.Run.cs).
// - W (weapons) implements N26WeaponsStartRun / N26WeaponsStep (Game/SurvivalSim.Weapons26.cs).
// - M (movement) implements N26SurfaceStartRun / N26SurfaceStep (Game/SurvivalSim.Surface26.cs).
// - B (bosses) implements N26BossStartRun / N26BossStep (Game/SurvivalSim.Boss26.cs).
// - L (levels) implements N26LevelStartRun / N26LevelStep (Game/SurvivalSim.Level26.cs).
namespace Skygrind.Game
{
    using Skygrind.Core;

    public sealed partial class SurvivalSim
    {
        /// <summary>The design config's NIGHT26 blocks, read once.</summary>
        N26Blocks n26Design;

        /// <summary>A tape's replay: the blocks its run read; null: the design defaults (or every block off with ForceOff).</summary>
        public N26Blocks? n26Override;

        /// <summary>The NIGHT26 blocks this run read (what a tape records).</summary>
        public N26Blocks n26;

        void N26Init() => n26Design = N26Blocks.From(cfg);

        void N26StartRun()
        {
            n26 = n26Override ?? (N26Blocks.ForceOff ? N26Blocks.Off : n26Design);
            N26WeaponsStartRun();
            N26SurfaceStartRun();
            N26BossStartRun();
            N26LevelStartRun();
        }

        partial void N26WeaponsStartRun();
        partial void N26WeaponsStep(double dt);
        partial void N26SurfaceStartRun();
        partial void N26SurfaceStep(double dt);
        partial void N26BossStartRun();
        partial void N26BossStep(double dt);
        partial void N26LevelStartRun();
        partial void N26LevelStep(double dt);
    }
}
