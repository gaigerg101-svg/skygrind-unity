// NIGHT40 builder H1 (NIGHT40.md W1 "the pistol in its rhythm": "W1 sets the numbers in UnityTuning.Pistol40.cs; H1 wires the crosshair"): the KESTREL's
// crosshair spin re-timed so every turn ends inside its shot's rhythm (W1's rule: within Pistol40Share 0.75 of the time to the next shot). While
// design.n40.charge40 is live in the run (Charge35.c40) with p39's spin (UiHud38.Kestrel39.cs), the KESTREL's meter spins:
// - after a TAP: the quarter turn (Hud39TapSpinDeg 90) over Pistol40TapSpinS from the shot, ease-out (Pistol39Math.SpinShare), then the snap over
//   Pistol40TapSnapS: it ends 0.28 + 0.06 = 0.34 s after the tap (NIGHT39 turned through the whole 0.483 s interval and snapped after it);
// - through the CHARGE: a full turn from the hold's start to FULL (as NIGHT39), the snap at FULL over Pistol40ChargeSnapS (0.08 s, was 0.12);
// - after the CHARGED shot: the full turn (Hud38SpinDeg 360) over Pistol40CannonSpinS, ease-out, then the snap over Pistol40CannonSnapS: it ends
//   1.70 + 0.12 = 1.82 s after the shot (NIGHT38 / 39 turned through the whole 2.5 s cooldown).
// The four ticks are square under a quarter or a full turn, so the turn's end lands where it began. charge40 off (or -skygrindOff40): NIGHT39's spin
// exactly (this hook returns at once and the snap keeps Hud38SnapTime). View only. Numbers: UnityTuning.Pistol40.cs (W1's).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>NIGHT40 H1: the KESTREL's re-timed spin (pure; Hud38Math's shape; EditMode tests: Hud40Tests).</summary>
    public static class Kestrel40Math
    {
        public const int None = 0, Tap = 1, Charge = 2, Cannon = 3;

        /// <summary>
        /// The KESTREL's meter with its charge40 spin: `holding` a real hold at `level` (0..1); else the charged shot's cooldown `chLeft` of `chFull` steps
        /// (its turn first), else the tap's interval `cdLeft` of `cdFull` steps; `dt` the sim step. `snapS`: the snap's length if this turn ends now;
        /// `kind`: which turn runs (None: no spin, m.spin = -1, so hud38's snap starts the frame a turn ends).
        /// </summary>
        public static Meter38 Kestrel(Meter38 m, bool holding, double level, int cdLeft, int cdFull, int chLeft, int chFull, double dt, out double snapS, out int kind)
        {
            snapS = UnityTuning.Hud38SnapTime;
            if (holding)
            {
                kind = Charge;
                snapS = UnityTuning.Pistol40ChargeSnapS;
                m.spin = level >= 1 ? 1f : (float)Math.Max(0, level); // a full turn through the charge; 1 = the snap at FULL (NIGHT39's)
                return m;
            }
            if (chLeft > 0)
            {
                double t = (Math.Max(chFull, chLeft) - chLeft) * dt; // s since the charged shot
                if (t < UnityTuning.Pistol40CannonSpinS)
                {
                    kind = Cannon;
                    snapS = UnityTuning.Pistol40CannonSnapS;
                    m.spin = (float)Pistol39Math.SpinShare(t, UnityTuning.Pistol40CannonSpinS);
                    return m;
                }
            }
            if (cdLeft > 0)
            {
                double t = (Math.Max(cdFull, cdLeft) - cdLeft) * dt; // s since the tap
                if (t < UnityTuning.Pistol40TapSpinS)
                {
                    kind = Tap;
                    snapS = UnityTuning.Pistol40TapSnapS;
                    m.spin = (float)Pistol39Math.SpinShare(t, UnityTuning.Pistol40TapSpinS) * (UnityTuning.Hud39TapSpinDeg / UnityTuning.Hud38SpinDeg);
                    return m;
                }
            }
            kind = None;
            m.spin = -1f;
            return m;
        }
    }

    public sealed partial class Ui
    {
        /// <summary>The snap's length (s) for the turn that ends next (hud38's Hud38SnapTime unless charge40's KESTREL turn set it while spinning).</summary>
        double h38SnapLen = UnityTuning.Hud38SnapTime;
        int h40TurnKind;
        double h40TurnAge;

        /// <summary>After hud38's snap step: times the KESTREL's tap / cannon turn from its first frame to its snap's end (Hud40.SpinEndS; the shot is
        /// at most one frame before that first frame). The charge's turn ends on FULL and is not timed.</summary>
        void Spin40Clock(double frameDt)
        {
            int k = Hud40.SpinKind;
            if (k != Kestrel40Math.None)
            {
                if (k != h40TurnKind) { h40TurnKind = k; h40TurnAge = 0; }
                else h40TurnAge += frameDt;
                return;
            }
            if (h40TurnKind == Kestrel40Math.None) return;
            h40TurnAge += frameDt;
            if (h38SnapAge < double.MaxValue) return; // the snap still runs
            if (h40TurnKind != Kestrel40Math.Charge)
            {
                Hud40.SpinEndS = h40TurnAge;
                Hud40.SpinEndKind = h40TurnKind;
                Hud40.SpinEnds++;
            }
            h40TurnKind = Kestrel40Math.None;
        }

        /// <summary>Hud38Step's hook (after Kestrel39Spin): the KESTREL's re-timed spin while charge40 is live with p39; `snapS` the snap's length.</summary>
        void Kestrel40Spin(ref Meter38 m, ref double snapS, string gun, bool holding, double level, int cdLeft, int cdFull, int chLeft, int chFull, double dt)
        {
            Hud40.SpinKind = Kestrel40Math.None;
            if (!UnityTuning.Hud39KestrelSpin || gun != "revolver") return;
            Charge35 q = game.sim != null ? game.sim.charge35 : null;
            if (q == null || !q.p39 || !q.c40) return;
            m = Kestrel40Math.Kestrel(m, holding, level, cdLeft, cdFull, chLeft, chFull, dt, out snapS, out int kind);
            Hud40.SpinKind = kind;
        }
    }
}
