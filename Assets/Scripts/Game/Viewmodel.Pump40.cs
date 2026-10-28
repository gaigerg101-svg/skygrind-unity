// NIGHT40 builder W1 pump40 (a view row, design.n40.pump40; numbers UnityTuning.Pump40.cs): the THUNDERCLAP's pump and its kick spring, faster.
// Three hooks: Viewmodel.cs (the pump's delay at the shot, its duration in the frame) and Viewmodel.Feel.cs (the kick spec at the shot). The design
// flag is read at Init; N40Blocks.ForceOff is read at each USE, because PlayerChecks sets it (-skygrindOff40 and every older off switch) in
// SurvivalGame.StartGame, after the viewmodel is built in Awake. Off: the web's pumpDelay / pumpDuration and HandsKickShotgunA, as NIGHT39 drew them.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class Viewmodel
    {
        /// <summary>design.n40.pump40.enabled, read at Init.</summary>
        bool pump40Design;
        bool pump41Design; // NIGHT41 (the lead's hook): design.n41.pump41On, read at Init
        HandsKickSpec pump40Spec;

        /// <summary>For the reels: is pump40 drawing now (the row on and not forced off)?</summary>
        public static bool Pump40Live;

        /// <summary>pump40 now: the row on, and not forced off by -skygrindOff40 (read each use: ForceOff is set after Init).</summary>
        bool Pump40Now => Pump40Live = (pump40Design && !N40Blocks.ForceOff) || (pump41Design && !N41View.Off); // NIGHT41: or design.n41.pump41On (T2's row; off with --off41)

        /// <summary>After ReadParams: the row's flag and the 6 Hz kick spec (built once).</summary>
        void Pump40Init(GameConfig cfg)
        {
            pump40Design = cfg != null && cfg.Design("n40.pump40.enabled");
            pump41Design = cfg != null && cfg.Design("n41.pump41On.enabled"); // NIGHT41 (the lead's hook)
            pump40Spec = pump40Design || pump41Design
                ? new HandsKickSpec { weapon = "shotgun", a = HandsKick.Layer(UnityTuning.Pump40KickA), b = HandsKick.Layer(UnityTuning.HandsKickShotgunB), hasB = true }
                : null;
        }

        /// <summary>The pump's start after a THUNDERCLAP shot (s).</summary>
        double Pump40Delay(double web) => Pump40Now ? UnityTuning.Pump40DelayS : web;

        /// <summary>The pump's slide out and home (s).</summary>
        double Pump40Duration(double web) => Pump40Now ? UnityTuning.Pump40DurationS : web;

        /// <summary>The kick spec of a shot: the 6 Hz THUNDERCLAP spec while pump40 is on, the gun's own otherwise.</summary>
        HandsKickSpec Pump40Kick(string weapon, HandsKickSpec own) => weapon == "shotgun" && pump40Spec != null && own != null && Pump40Now ? pump40Spec : own;
    }
}
