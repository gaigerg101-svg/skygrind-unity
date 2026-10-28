// NIGHT40 builder R2 (NIGHT40.md R2; the hook is the lead's SurvivalSim.N40.cs): ride40 in the sim.
// - ride40 (design.n40.ride40; DECIDE40 D-9): the run's rules handed to the Mover at the run's start (Core/Ride40.cs; the Mover's "NIGHT40 ride40"
//   branches, inside the NIGHT35 wallride35 ones, read them). On: ride38's hold rule with design.n40.ride40's numbers (Ride40.Hold) for every ride.
//   N40StartRun runs after N38StartRun / N39StartRun, so this replaces ride38's rules for the run (the n38 row itself is untouched). The row off
//   (or --off40 / -skygrindOff40, or a tape of format 22 or older): Ride40.Params.Neutral and ride38's rules as N38RideStartRun set them:
//   NIGHT39's ride bit for bit. Needs wallride35 on (the branches sit inside its own).
// No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The harness's switch for ride40 (null: the run's n40 blocks decide). Process-wide, read at each run's start.</summary>
        public static bool? Force40Ride;

        /// <summary>The harness's what-if on ride40's attach rules (null: the config's). Process-wide, read at each run's start; never set by the game.</summary>
        public static System.Func<Ride40.Params, Ride40.Params> Probe40Ride;

        partial void N40RideStartRun()
        {
            if (Force40Ride.HasValue) n40.ride40 = Force40Ride.Value;
            if (n40.ride40)
            {
                mover.SetRide38(Ride40.Hold(cfg, true));
                Ride40.Params p = Ride40.Params.From(cfg, true);
                mover.SetRide40(Probe40Ride != null ? Probe40Ride(p) : p);
            }
            else mover.SetRide40(Ride40.Params.Neutral);
        }
    }
}
