// NIGHT34 builder M (design.n34.reach34; NIGHT34.md M1): the Pulse Gauntlet's punch and blast reach a BOSS with bossMargin m added to the boss
// body's radius (GG 2026-10-11: "a slightly larger active hitbox when striking bosses like the worm"). The rule lives in Core (Loadout's
// reach34BossMargin, GauntletTargets34, BossMargin34: Core/Guns/Pulse25.cs); this hook hands it the run's margin at the run's start: the row's
// bossMargin with reach34 on, 0 off (NIGHT33's path exactly). Reach34Margin is the same number for the KILN WYRM head-punch stagger's reach test
// (SurvivalSim.Boss30.cs Wyrm30OnHit, by the lead's patch Tools/night/n34m-reach-patch.cjs), so the stagger lands on every swing the margin lands.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The harness's switch for reach34 (null: the run's n34 blocks decide). Process-wide, read at each run's start.</summary>
        public static bool? Force34Reach;

        /// <summary>NIGHT34 reach34: the run's boss margin (m) for the gauntlet; 0 with the row off.</summary>
        public double Reach34Margin { get; private set; }

        partial void N34ReachStartRun()
        {
            if (Force34Reach.HasValue) n34.reach34 = Force34Reach.Value;
            Reach34Margin = n34.reach34 ? cfg.Num("design.n34.reach34.bossMargin") : 0;
            if (loadout != null) loadout.reach34BossMargin = Reach34Margin;
        }
    }
}
