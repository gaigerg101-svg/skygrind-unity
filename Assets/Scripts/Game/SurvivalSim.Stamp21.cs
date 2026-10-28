// NIGHT21 U3-35.5 THE STAMP (builder S owns this file): the round grades in the sim, through the shared commit's hooks
// (SurvivalSim.N21.cs: N21StampStartRun at the run's start, N21StampStep once per fixed step after the enemies). With
// design.n21.stamp off (or --off21) the grader counts nothing and says nothing: NIGHT20's game bit for bit. The grader is made
// once (its handlers stay on the event bus) and each run turns it on or off. The rules are Core/Grades21.cs's.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The run's round grades (Core/Grades21.cs; Rounds is RESULTS' strip, Last the stamp on screen). Null before the first run.</summary>
        public RoundGrader21 grades21;

        partial void N21StampStartRun()
        {
            grades21 ??= new RoundGrader21(cfg, events, () => style.rank, () => health.max, () => run.stats.revived);
            grades21.NewRun(n21.stamp);
        }

        partial void N21StampStep(double dt)
        {
            if (grades21 != null && grades21.on) grades21.Step();
        }
    }
}
