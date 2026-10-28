// NIGHT39 builder P1 (NIGHT39.md P1; GG 2026-10-13: "the pistol—similar to the shotgun—needs the crosshair to do that little rotation animation"): the
// KESTREL's part of hud38's crosshair spin (builder V owns the rest of UiHud38). NIGHT38's rule spins the ticks only through a cooldown of at least
// Hud30MinCoolSeconds (0.6 s: the THUNDERCLAP's pump), so the KESTREL's 0.4-0.48 s interval never spun. While design.n39.p39 is live and
// UnityTuning.Hud39KestrelSpin is on, the KESTREL's meter also spins:
// - through the CHARGE: a full turn from the hold's start (tapS) to FULL, and the snap (white, pushed out) the frame the charge is full;
// - through each TAP's interval: a quarter turn (Hud39TapSpinDeg; the reticle's four ticks land where they began), then the snap;
// - the charged shot's 2.5 s cooldown keeps NIGHT38's full turn (Hud38Math.Read already spins it).
// View only (hud38 is a view row: on only after the lead's frames). Numbers: UnityTuning.Pistol39.cs.
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>NIGHT39 P1: the KESTREL's spin rule (pure; Hud38Math's shape).</summary>
    public static class Hud39Math
    {
        /// <summary>The KESTREL's meter with its spin: `holding` a real hold at `level` (0..1); else its own interval `cdLeft` of `cdFull` steps.</summary>
        public static Meter38 Kestrel(Meter38 m, bool holding, double level, int cdLeft, int cdFull)
        {
            if (holding)
            {
                m.spin = level >= 1 ? 1f : (float)System.Math.Max(0, level); // a full turn through the charge; 1 = the snap at FULL
                return m;
            }
            if (m.spin < 0 && cdLeft > 0)
                m.spin = Hud38Math.Refill(cdLeft, System.Math.Max(cdFull, cdLeft)) * (UnityTuning.Hud39TapSpinDeg / UnityTuning.Hud38SpinDeg);
            return m;
        }
    }

    public sealed partial class Ui
    {
        /// <summary>Hud38Step's hook (the line after Hud38Math.Read): the KESTREL's spin while p39 is live.</summary>
        void Kestrel39Spin(ref Meter38 m, string gun, bool holding, double level, int cdLeft, int cdFull)
        {
            if (!UnityTuning.Hud39KestrelSpin || gun != "revolver") return;
            Charge35 q = game.sim != null ? game.sim.charge35 : null;
            if (q == null || !q.p39) return;
            m = Hud39Math.Kestrel(m, holding, level, cdLeft, cdFull);
        }
    }
}
