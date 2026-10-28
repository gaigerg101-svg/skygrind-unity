// NIGHT20 (the shared commit, the lead owns this file): the eight NIGHT20 design blocks in the sim (Core/N20Blocks.cs).
// Read once (N20Init, in the constructor); each run takes them at its start (N20StartRun, right after N19StartRun): a
// tape's replay takes the blocks it recorded (n20Override; a format 1-4 tape: every block off), the harnesses' --off20
// (and --off19) take every block off (N20Blocks.ForceOff), otherwise the design defaults. The run hands them to the Core
// systems that read them (Loadout.n20, Director.n20) before their own resets.
// The builders' hooks (partial methods: a hook nobody implements compiles to nothing):
// - G (guns) implements N20ArsenalStartRun / N20ArsenalStep in Game/SurvivalSim.Arsenal20.cs (after the loadout's step);
// - E (enemies, regen) implements N20RegenStartRun / N20RegenStep in Game/SurvivalSim.Regen20.cs (after regen.Step) and
//   N20DirectorStartRun / N20DirectorStep in Game/SurvivalSim.Director20.cs (after the enemies' step);
// - N (routes) implements N20RoutesStartRun / N20RoutesStep in Game/SurvivalSim.Routes20.cs (after the brain's step:
//   the fields and the AI, before the loadout).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The design config's NIGHT20 blocks, read once.</summary>
        N20Blocks n20Design;

        /// <summary>A tape's replay: the blocks its run read; null: the design defaults (or every block off with ForceOff).</summary>
        public N20Blocks? n20Override;

        /// <summary>The NIGHT20 blocks this run read (what a tape records).</summary>
        public N20Blocks n20;

        void N20Init() => n20Design = N20Blocks.From(cfg);

        void N20StartRun()
        {
            n20 = n20Override ?? (N20Blocks.ForceOff ? N20Blocks.Off : n20Design);
            loadout.n20 = n20;
            director.n20 = n20;
            N20RoutesStartRun();
            Hitch24.Mark("n20:routes");
            N20ArsenalStartRun();
            Hitch24.Mark("n20:arsenal");
            N20RegenStartRun();
            Hitch24.Mark("n20:regen");
            N20DirectorStartRun();
            Hitch24.Mark("n20:director");
        }

        partial void N20RoutesStartRun();
        partial void N20RoutesStep(double dt);
        partial void N20ArsenalStartRun();
        partial void N20ArsenalStep(double dt);
        partial void N20RegenStartRun();
        partial void N20RegenStep(double dt);
        partial void N20DirectorStartRun();
        partial void N20DirectorStep(double dt);
    }
}
