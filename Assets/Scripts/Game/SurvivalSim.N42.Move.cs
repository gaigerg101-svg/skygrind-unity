// NIGHT42 builder M1 (NIGHT42.md M1; the hooks are the lead's SurvivalSim.N42.cs): wallfling42 and boost42 in the sim.
// - wallfling42 (design.n42.wallfling42, G2, DECIDE42 D-4): the run's rules handed to the Mover at the run's start (Core/Wall42.cs; the Mover's
//   "NIGHT42 wallfling42" lines in its wall-jump / jump-off branches read them). Off: Wall42.Params.Neutral (NIGHT41's Mover bit for bit).
// - boost42 (design.n42.boost42, G5): the run's THUNDERCLAP boost rule handed to the Loadout (Core/Boost42.cs; Loadout.ShotgunJump42 at the
//   three shotgun-jump lines). Off: Boost42.Params.Neutral (the web's `velocity -= aim x jumpSpeed`).
// The harness's switches (Force42Wallfling / Force42Boost) force a row on or off for a run (n42move, the gates with the row forced on: L-061).
// No UnityEngine: SimHarness compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The harness's switches for wallfling42 / boost42 (null: the run's n42 blocks decide). Process-wide, read at each run's start.</summary>
        public static bool? Force42Wallfling, Force42Boost;

        /// <summary>The harness's what-if on wallfling42's numbers (null: the config's). Process-wide, read at each run's start; never set by the game.</summary>
        public static System.Func<Wall42.Params, Wall42.Params> Probe42Wall;

        /// <summary>The harness's what-if on boost42's numbers (null: the config's). Process-wide, read at each run's start; never set by the game.</summary>
        public static System.Func<Boost42.Params, Boost42.Params> Probe42Boost;

        partial void N42MoveStartRun()
        {
            if (Force42Wallfling.HasValue) n42.wallfling42 = Force42Wallfling.Value;
            if (Force42Boost.HasValue) n42.boost42 = Force42Boost.Value;
            Wall42.Params wp = Wall42.Params.From(cfg, n42.wallfling42);
            if (Probe42Wall != null && wp.on) wp = Probe42Wall(wp);
            mover.SetWall42(wp);
            Boost42.Params bp = Boost42.Params.From(cfg, n42.boost42);
            if (Probe42Boost != null && bp.on) bp = Probe42Boost(bp);
            if (loadout != null) loadout.boost42 = bp;
        }
    }
}
