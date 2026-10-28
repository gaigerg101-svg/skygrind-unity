// NIGHT43 builder M1 (NIGHT43.md M1; the hooks are the lead's SurvivalSim.N43.cs): chimney43, ride43 and boost43 in the sim.
// - chimney43 / ride43 (design.n43.chimney43 / ride43; G13 / G2 / G3; DECIDE43 D-1 / D-2): the run's rules handed to the Mover at the run's
//   start (Core/Move43.cs; the Mover's "NIGHT43 chimney43" / "NIGHT43 ride43" branches read them). Off: Move43.Params.Neutral (NIGHT42's Mover).
// - boost43 (design.n43.boost43, G26): the run's combine rule handed to the Loadout (Core/Boost43.cs) and its late push stepped here, after the
//   loadout's step. Off: Boost43.Params.Neutral (boost42 / the web's line).
// The harness's switches (Force43Chimney / Force43Ride / Force43Boost) force a row on or off for a run (n43move, the gates with the rows forced
// on: L-061). No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The harness's switches for chimney43 / ride43 / boost43 (null: the run's n43 blocks decide). Process-wide, read at each run's start.</summary>
        public static bool? Force43Chimney, Force43Ride, Force43Boost;

        /// <summary>The harness's what-if on boost43's numbers (null: the config's). Process-wide, read at each run's start; never set by the game.</summary>
        public static System.Func<Boost43.Params, Boost43.Params> Probe43Boost;

        partial void N43MoveStartRun()
        {
            if (Force43Chimney.HasValue) n43.chimney43 = Force43Chimney.Value;
            if (Force43Ride.HasValue) n43.ride43 = Force43Ride.Value;
            if (Force43Boost.HasValue) n43.boost43 = Force43Boost.Value;
            mover.SetMove43(Move43.Params.From(cfg, n43.chimney43, n43.ride43, n43.boost43));
            Boost43.Params bp = Boost43.Params.From(cfg, n43.boost43);
            if (Probe43Boost != null && bp.on) bp = Probe43Boost(bp);
            if (loadout != null)
            {
                loadout.boost43 = bp;
                loadout.b43.Reset();
            }
        }

        partial void N43MoveStep(double dt)
        {
            if (n43.boost43 && Active && loadout != null) loadout.Boost43Step();
        }
    }
}
