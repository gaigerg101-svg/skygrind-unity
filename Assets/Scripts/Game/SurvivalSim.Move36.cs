// NIGHT36 builder M (NIGHT36.md M1; the hooks are the lead's SurvivalSim.N36.cs): the movement row in the sim.
// - wallgrip36 (design.n36.wallgrip36; REVIEW.md root cause 4, GG 2026-10-12: "you kind of hook onto a wall and you kind of just start sliding up
//   it"): the run's grip rules handed to the Mover at the run's start (Core/Grip36.cs; the Mover's "NIGHT36 wallgrip36" branches, inside the
//   NIGHT35 wallride35 ones, read them), as wallride35's are. The row off (or --off36 / -skygrindOff36, or a tape of format 18 or older):
//   Grip36.Params.Neutral, every branch skipped: wallride35's ride bit for bit. Needs wallride35 on (the branches sit inside its own).
// No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The harness's switch for wallgrip36 (null: the run's n36 blocks decide). Process-wide, read at each run's start.</summary>
        public static bool? Force36Grip;

        partial void N36MoveStartRun()
        {
            if (Force36Grip.HasValue) n36.wallgrip36 = Force36Grip.Value;
            mover.SetGrip36(Grip36.Params.From(cfg, n36.wallgrip36));
        }
    }
}
