// NIGHT26 builder B: the bosses' hooks into the lead's N26 partials (SurvivalSim.N26.cs): THE SLAG HULK (SurvivalSim.Hulk26.cs), THE GLACIER and the worm's
// level-3 behaviours. Nothing here runs with every NIGHT26 block off. No UnityEngine: SimHarness compiles this file.
namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        partial void N26BossStartRun()
        {
            Hulk26StartRun();
            Glacier26StartRun();
        }

        partial void N26BossStep(double dt)
        {
            Hulk26Step(dt);
            Glacier26Step(dt);
        }
    }
}
