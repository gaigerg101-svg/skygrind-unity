// NIGHT39 builder R1 (NIGHT39.md R1; the hook is the lead's SurvivalSim.N39.cs): ride39 in the sim.
// - ride39 (design.n39.ride39; GG 2026-10-13: "the wall riding speed is a little too fast—you gain speed too quickly—and the jump height should be
//   increased slightly"): at a run's start (N39StartRun runs after N35StartRun, so wallride35's rules are already the Mover's) the Mover's
//   wallride35 rules get the row's speedBuff / exitBoost / chainCap / upLow / upHigh (Ride35.Params.With39). The Mover's wallride35 branches read
//   them as they read wallride35's own; no Mover line changes. The row off (or --off39 / -skygrindOff39, or a tape of format 21 or older):
//   nothing is touched, NIGHT38's ride bit for bit. Needs wallride35 on (Neutral rules stay neutral).
// No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The harness's switch for ride39 (null: the run's n39 blocks decide). Process-wide, read at each run's start.</summary>
        public static bool? Force39Ride;

        partial void N39RideStartRun()
        {
            if (Force39Ride.HasValue) n39.ride39 = Force39Ride.Value;
            if (!n39.ride39 || !mover.flow34.ride35.rules.on) return;
            mover.SetRide35(mover.flow34.ride35.rules.With39(cfg));
        }
    }
}
