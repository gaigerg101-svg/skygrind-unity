// NIGHT42 builder W1 reticle42 (G8, GG 2026-10-14: "animation for redical for all weapons needs to spin faster and complete animation before you
// can preform necxt attack"). View only; on while design.n42.reticle42 is enabled (N42View.RowOn; off with -skygrindOff42 and older). Hud38Step's
// hook, after Kestrel40Spin (one line in UiHud38.cs):
// - the KESTREL (p39 + charge40 live, as Kestrel40Spin): the tap's quarter turn Reticle42TapSpinS + Reticle42TapSnapS (0.27 s, was 0.34 s);
//   the cannon's full turn design.n42.charge42.cannonTurnS - Reticle42CannonSnapS, then the snap: it ends at cannonTurnS (0.46 s, was 1.82 s),
//   when charge42's gate lets the next shot go (Charge35.turnSteps42); the charge's turn as NIGHT40 (it ends on FULL).
// - every other weapon (and the KESTREL without p39 / charge40): hud38's turn through its own cooldown (or its charge's) runs to Reticle42Share
//   (0.7) of the cycle minus the snap (Reticle42SnapS), so the turn and its snap are over at 70 % of the cycle, before the next shot can leave
//   (NIGHT38: the turn ran the whole cycle and the 0.12 s snap ran into the next shot). The gauntlet's pips (the blast) keep hud38's turn.
// Numbers: UnityTuning.Reticle42.cs (view) and design.n42.charge42.cannonTurnS (the sim's gate). Pure math in Reticle42Math (EditMode-testable).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>NIGHT42 W1: reticle42's turn (pure).</summary>
    public static class Reticle42Math
    {
        /// <summary>A cooldown turn sped up: `left` of `full` steps of a cycle; the turn (0..1) reaches 1 at Share x the cycle less the snap.</summary>
        public static float Spin(int left, int full, double dt)
        {
            full = Math.Max(full, left);
            if (full <= 0 || left <= 0) return 1f;
            double t = (full - left) * dt, end = UnityTuning.Reticle42Share * full * dt - UnityTuning.Reticle42SnapS;
            if (end <= 0) return 1f;
            return (float)Math.Min(1.0, t / end);
        }

        /// <summary>The KESTREL's meter (Kestrel40Math.Kestrel's shape) with reticle42's times; `cannonTurnS` = design.n42.charge42.cannonTurnS.</summary>
        public static Meter38 Kestrel(Meter38 m, bool holding, double level, int cdLeft, int cdFull, int chLeft, int chFull, double dt, double cannonTurnS, out double snapS, out int kind)
        {
            snapS = UnityTuning.Reticle42SnapS;
            if (holding)
            {
                kind = Kestrel40Math.Charge;
                snapS = UnityTuning.Pistol40ChargeSnapS;
                m.spin = level >= 1 ? 1f : (float)Math.Max(0, level);
                return m;
            }
            double cannonSpin = Math.Max(0.05, cannonTurnS - UnityTuning.Reticle42CannonSnapS);
            if (chLeft > 0)
            {
                double t = (Math.Max(chFull, chLeft) - chLeft) * dt; // s since the charged shot
                if (t < cannonSpin)
                {
                    kind = Kestrel40Math.Cannon;
                    snapS = UnityTuning.Reticle42CannonSnapS;
                    m.spin = (float)Pistol39Math.SpinShare(t, cannonSpin);
                    return m;
                }
                if (t < cannonTurnS + 0.5)
                {
                    // the cannon's turn is over; the own cooldown charge42 set (turnSteps42) is not a tap: no tap turn
                    kind = Kestrel40Math.None;
                    m.spin = -1f;
                    return m;
                }
            }
            if (cdLeft > 0)
            {
                double t = (Math.Max(cdFull, cdLeft) - cdLeft) * dt; // s since the tap
                if (t < UnityTuning.Reticle42TapSpinS)
                {
                    kind = Kestrel40Math.Tap;
                    snapS = UnityTuning.Reticle42TapSnapS;
                    m.spin = (float)Pistol39Math.SpinShare(t, UnityTuning.Reticle42TapSpinS) * (UnityTuning.Hud39TapSpinDeg / UnityTuning.Hud38SpinDeg);
                    return m;
                }
            }
            kind = Kestrel40Math.None;
            m.spin = -1f;
            return m;
        }
    }

    public sealed partial class Ui
    {
        int r42Read = -1; // -1 unread, 0 off, 1 on (the row is read once a session)
        double r42CannonS;

        /// <summary>Hud38Step's hook (after Kestrel40Spin): reticle42's faster turns. Nothing with the row off.</summary>
        void Reticle42Spin(ref Meter38 m, ref double snapS, string gun, bool holding, double level, int cdLeft, int cdFull, int chLeft, int chFull, double dt)
        {
            if (r42Read < 0)
            {
                r42Read = N42View.RowOn(game.cfg, "reticle42") ? 1 : 0;
                r42CannonS = r42Read == 1 ? game.cfg.Num("design.n42.charge42.cannonTurnS") : 0;
            }
            if (r42Read == 0 || !m.show && m.spin < 0) return;
            Charge35 q = game.sim != null ? game.sim.charge35 : null;
            if (gun == "revolver" && UnityTuning.Hud39KestrelSpin && q != null && q.p39 && q.c40)
            {
                m = Reticle42Math.Kestrel(m, holding, level, cdLeft, cdFull, chLeft, chFull, dt, r42CannonS, out snapS, out int kind);
                Hud40.SpinKind = kind;
                return;
            }
            if (holding || m.spin < 0 || m.spin >= 1) return;
            if (gun == "gauntlet" && game.sim != null && game.sim.pulse27 != null && game.sim.pulse27.active) return; // the pips' blast: hud38's turn
            bool own = Hud38Math.Shows(cdFull, cdLeft, dt);
            if (own) m.spin = Reticle42Math.Spin(cdLeft, cdFull, dt);
            else if (chLeft > 0) m.spin = Reticle42Math.Spin(chLeft, chFull, dt);
            else return;
            snapS = UnityTuning.Reticle42SnapS;
        }
    }
}
