// NIGHT19 (the shared commit, the lead owns this file): the seven NIGHT19 design blocks in the sim (Core/N19Blocks.cs).
// Read once (N19Init, in the constructor); each run takes them at its start (N19StartRun, right after CleanSlate): a
// tape's replay takes the blocks it recorded (n19Override; a format 1-3 tape: every block off), the harnesses' --off19
// takes every block off (N19Blocks.ForceOff), otherwise the design defaults. The run hands them to the Core systems that
// read them (Loadout.n19, Director.n19) before their own resets, and sets the Mover's sprint key rule after the stamina
// rules (design.stamina.n19.ownKey, only with the stamina block on).
// The builders' hooks (partial methods: a hook nobody implements compiles to nothing):
// - W (the weapon sandbox) implements N19ArsenalStartRun / N19ArsenalStep in Game/SurvivalSim.Arsenal19.cs;
// - E (threats and stamina) implements N19EnemiesStartRun / N19EnemiesStep in Game/SurvivalSim.Enemies19.cs.
// N19ArsenalStep runs right after the loadout's step, N19EnemiesStep right after the enemies' (husks.Step: main.js
// simulate's order).
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The design config's NIGHT19 blocks, read once.</summary>
        N19Blocks n19Design;

        /// <summary>A tape's replay: the blocks its run read; null: the design defaults (or every block off with ForceOff).</summary>
        public N19Blocks? n19Override;

        /// <summary>The NIGHT19 blocks this run read (what a tape records).</summary>
        public N19Blocks n19;

        /// <summary>THE RANGE (Game/Bot/SandboxRange.cs): the run's round clock held while a cell plays (no break ends, no wave
        /// starts: StepMode skips run.Step); everything else steps. False outside the judges.</summary>
        public bool holdRunClock;

        void N19Init() => n19Design = N19Blocks.From(cfg);

        void N19StartRun()
        {
            n19 = n19Override ?? (N19Blocks.ForceOff ? N19Blocks.Off : n19Design);
            loadout.n19 = n19;
            director.n19 = n19;
            N19ArsenalStartRun();
            N19EnemiesStartRun();
        }

        /// <summary>After StaminaStartRun: the sprint's own key (InputFrame.sprint) with design.stamina.n19 on and its ownKey.</summary>
        void N19StaminaStartRun()
        {
            mover.stamina.rules.ownKey = n19.stamina && mover.stamina.rules.Any && cfg.Design("stamina.n19.ownKey");
        }

        partial void N19ArsenalStartRun();
        partial void N19ArsenalStep(double dt);
        partial void N19EnemiesStartRun();
        partial void N19EnemiesStep(double dt);
    }
}
