// NIGHT40 builder W1 pump40 (NIGHT40.md W1; GG 2026-10-13: "The shotgun's firing animation should also be sped up"): the THUNDERCLAP's pump and kick,
// faster. A view row: design.n40.pump40.enabled (no sim bit; off with -skygrindOff40 too: Viewmodel.Pump40On). The web's numbers stay as they are
// (weapons.shotgun.pumpDelay 0.45 s, viewmodel.pumpDuration 0.3 s, UnityTuning.HandsKickShotgunA 4.2 Hz); these replace them in the view while the
// row is on. The fire interval (~1.52 s) is unchanged: the pump ends at 0.20 + 0.22 = 0.42 s (28 % of it; NIGHT39 0.75 s, 49 %).
// Render numbers only (CLAUDE.md rule 5). Viewmodel.Pump40.cs reads them.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The pump starts this long after the shot (s; the web's weapons.shotgun.pumpDelay 0.45): the slide racks while the kick is still
        /// settling, so the shot reads as one motion (blast, rack) instead of a blast, a pause, then a rack.</summary>
        public const double Pump40DelayS = 0.2;
        /// <summary>The pump's slide back and home (s; the web's viewmodel.pumpDuration 0.3): a sin(pi u) out-and-back over this.</summary>
        public const double Pump40DurationS = 0.22;
        /// <summary>The THUNDERCLAP's first kick layer with the spring at 6 Hz (HandsKickShotgunA's 4.2 Hz; every peak, the damping 0.5 and the
        /// second layer HandsKickShotgunB unchanged): its envelope falls to 5 % in ln 20 / (0.5 x 2 pi x 6) = 0.16 s (was 0.23 s), so the shove
        /// snaps back before the rack. { freqHz, zeta, pitch deg, back m, drop m, roll deg, yaw deg, side m } as HandsKickShotgunA.</summary>
        public static readonly double[] Pump40KickA = { 6, 0.5, -7, 0.17, 0.03, -3, -6, 0.02 };
    }
}
