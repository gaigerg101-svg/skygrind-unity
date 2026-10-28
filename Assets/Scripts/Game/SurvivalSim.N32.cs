// NIGHT32 (the shared commit, the lead owns this file): the seven sim-changing NIGHT32 design blocks in the sim (Core/N32Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n32Override), --off32 / -skygrindOff32 (and every older off switch)
// take every block off (N32Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (a partial method with no body costs nothing; the call sites in SurvivalSim.Run.cs are the lead's):
// - B (bosses) implements N32BossStartRun / N32BossStep (Game/SurvivalSim.Boss32.cs): bosses32.
// - G (grapple + balance) implements N32GrappleStartRun / N32GrappleStep / N32GrappleFrame (Game/SurvivalSim.Grapple32.cs): grapple32;
//   N32BalanceStartRun / N32BalanceStep (Game/SurvivalSim.Balance32.cs): balance32.
// - C (THE CROWN) implements N32CrownStartRun / N32CrownStep (Game/SurvivalSim.Crown32.cs): boss5.
// - A (arenas), S (span), F (finale) implement N32ArenaStartRun / N32ArenaStep, N32SpanStartRun / N32SpanStep, N32FinaleStartRun /
//   N32FinaleStep only if a level needs sim code (each in its own new file); level data switches through Curriculum25.N32Levels.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N32Blocks n32Design;

        /// <summary>A replay's or a harness's NIGHT32 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N32Blocks? n32Override;

        /// <summary>The NIGHT32 blocks the current run reads (set at StartRun).</summary>
        public N32Blocks n32;

        void N32Init() => n32Design = N32Blocks.From(cfg);

        void N32StartRun()
        {
            n32 = n32Override ?? (N32Blocks.ForceOff ? N32Blocks.Off : n32Design);
            N32GrappleStartRun();
            N32BalanceStartRun();
            N32BossStartRun();
            N32ArenaStartRun();
            N32SpanStartRun();
            N32FinaleStartRun();
            N32CrownStartRun();
        }

        partial void N32GrappleStartRun();
        partial void N32GrappleStep(double dt);
        partial void N32GrappleFrame(ref InputFrame frame); // NIGHT32 G: before mover.Step, after N31GrappleFrame (SurvivalSim.Run.cs Step)
        partial void N32BalanceStartRun();
        partial void N32BalanceStep(double dt);
        partial void N32BossStartRun();
        partial void N32BossStep(double dt);
        partial void N32ArenaStartRun();
        partial void N32ArenaStep(double dt);
        partial void N32SpanStartRun();
        partial void N32SpanStep(double dt);
        partial void N32FinaleStartRun();
        partial void N32FinaleStep(double dt);
        partial void N32CrownStartRun(); // NIGHT32 C: THE CROWN, level 5's boss (Game/SurvivalSim.Crown32.cs)
        partial void N32CrownStep(double dt);
    }
}
