// NIGHT38 builder M (NIGHT38.md M1; the hooks are the lead's SurvivalSim.N38.cs): the movement row in the sim.
// - ride38 (design.n38.ride38; PLAN.md section 1, DECIDE38 D-3): the run's hold-ride rules handed to the Mover at the run's start
//   (Core/Ride38.cs; the Mover's "NIGHT38 ride38" branches, inside the NIGHT35 wallride35 ones, read them), as wallride35's and wallgrip36's
//   are (N38StartRun runs after N35StartRun / N36StartRun, so SetRide35 has already reset the ride state). The row off (or --off38 /
//   -skygrindOff38, or a tape of format 20 or older): Ride38.Params.Neutral, every branch skipped: NIGHT37's ride bit for bit. Needs
//   wallride35 on (the branches sit inside its own).
// - noride: the level's campaign boxes tagged `noride: true` (Ride38.NoRideFrom) handed to the Mover while any NIGHT38 block is on (a level
//   with such boxes is an n38 level, reached only through a NIGHT38 level row); otherwise none.
// No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The harness's switch for ride38 (null: the run's n38 blocks decide). Process-wide, read at each run's start.</summary>
        public static bool? Force38Ride;

        partial void N38RideStartRun()
        {
            if (Force38Ride.HasValue) n38.ride38 = Force38Ride.Value;
            mover.SetRide38(Ride38.Params.From(cfg, n38.ride38));
            mover.SetNoRide38(n38.Any && map != null ? Ride38.NoRideFrom(map.json) : null);
        }
    }
}
