// NIGHT27 (the shared commit, the lead owns this file): the six sim-changing NIGHT27 design blocks in the sim (Core/N27Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n27Override), --off27 / -skygrindOff27 (and --off26 and older)
// take every block off (N27Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (a partial method with no body costs nothing; the call sites in SurvivalSim.Run.cs are the lead's, one each):
// - W (weapons) implements N27WeaponsStartRun / N27WeaponsStep (Game/SurvivalSim.Weapons27.cs): pulse27, chargeShots27.
// - B (bosses) implements N27BossStartRun / N27BossStep (Game/SurvivalSim.Boss27.cs): boss1 (THE LANTERN), boss2 (THE GLACIER).
// - A (levels) implements N27LevelStartRun / N27LevelStep (Game/SurvivalSim.Level27.cs): arenas27, hazardFloor27.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N27Blocks n27Design;

        /// <summary>A replay's or a harness's NIGHT27 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N27Blocks? n27Override;

        /// <summary>The NIGHT27 blocks the current run reads (set at StartRun).</summary>
        public N27Blocks n27;

        void N27Init() => n27Design = N27Blocks.From(cfg);

        void N27StartRun()
        {
            n27 = n27Override ?? (N27Blocks.ForceOff ? N27Blocks.Off : n27Design);
            N27WeaponsStartRun();
            pulseKillSink27 = new PulseKillSink27(this); // the bosses' kills (sentries, the GLACIER's hits) fill the glove's pips
            N27BossStartRun();
            N27LevelStartRun();
        }

        partial void N27WeaponsStartRun();
        partial void N27WeaponsStep(double dt);
        partial void N27BossStartRun();
        partial void N27BossStep(double dt);
        partial void N27LevelStartRun();
        partial void N27LevelStep(double dt);
    }

    /// <summary>The lead's adapter from the bosses' IPulseKillSink27 to the glove (Core/PulseCharge27.cs): n kills = n AddKill calls.</summary>
    sealed class PulseKillSink27 : IPulseKillSink27
    {
        readonly SurvivalSim sim;
        public PulseKillSink27(SurvivalSim sim) { this.sim = sim; }
        public void PulseKillFilled(int kills) { for (int i = 0; i < kills; i++) sim.pulse27?.AddKill(); }
    }
}
