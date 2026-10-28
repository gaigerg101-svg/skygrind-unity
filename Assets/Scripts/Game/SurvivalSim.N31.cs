// NIGHT31 (the shared commit, the lead owns this file): the eight sim-changing NIGHT31 design blocks in the sim (Core/N31Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n31Override), --off31 / -skygrindOff31 (and every older off switch)
// take every block off (N31Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (a partial method with no body costs nothing; the call sites in SurvivalSim.Run.cs are the lead's):
// - G (grapple) implements N31GrappleStartRun / N31GrappleStep (Game/SurvivalSim.Grapple31.cs): grapplePull.
// - W (weapons) implements N31WeaponsStartRun / N31WeaponsStep (Game/SurvivalSim.Weapons31.cs): charge, ltAlways, unlocks.
// - B (bosses) implements N31BossStartRun / N31BossStep (Game/SurvivalSim.Boss31.cs): lantern31, stag31, wyrm31, loom31.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N31Blocks n31Design;

        /// <summary>A replay's or a harness's NIGHT31 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N31Blocks? n31Override;

        /// <summary>The NIGHT31 blocks the current run reads (set at StartRun).</summary>
        public N31Blocks n31;

        void N31Init() => n31Design = N31Blocks.From(cfg);

        void N31StartRun()
        {
            n31 = n31Override ?? (N31Blocks.ForceOff ? N31Blocks.Off : n31Design);
            N31GrappleStartRun();
            N31WeaponsStartRun();
            N31BossStartRun();
        }

        partial void N31GrappleStartRun();
        partial void N31GrappleStep(double dt);
        partial void N31GrappleFrame(ref InputFrame frame); // NIGHT31 G: before mover.Step (SurvivalSim.Run.cs Step)
        partial void N31WeaponsStartRun();
        partial void N31WeaponsStep(double dt);
        partial void N31BossStartRun();
        partial void N31BossStep(double dt);
    }
}
