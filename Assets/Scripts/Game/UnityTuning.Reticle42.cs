// NIGHT42 builder W1 reticle42 (G8, GG 2026-10-14: "animation for redical for all weapons needs to spin faster and complete animation before you
// can preform necxt attack"): the crosshair's turn faster on every weapon and finished (spin + snap) before the gun can fire again. View only;
// read while design.n42.reticle42 is on (N42View.RowOn; UiHud38.Reticle42.cs). The gate (the shot waits for the cannon turn) is the sim's,
// design.n42.charge42.cannonTurnS. Render / UI numbers only (CLAUDE.md rule 5).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>Every weapon's cooldown turn (hud38's spin through a cycle) ends, its snap included, at this share of the cycle (NIGHT38: the
        /// turn ran the whole cycle and its 0.12 s snap ran past it into the next shot). 0.7 = the KESTREL tap's NIGHT40 share, kept for all.</summary>
        public const double Reticle42Share = 0.7;

        /// <summary>The snap that ends a cooldown turn (s): hud38's 0.12 s shortened so a short cycle's turn still has room to spin.</summary>
        public const float Reticle42SnapS = 0.08f;

        /// <summary>The KESTREL's tap: the quarter turn and its snap (s): 0.22 + 0.05 = 0.27 s (56 % of the 0.483 s interval; NIGHT40 0.34 s).</summary>
        public const float Reticle42TapSpinS = 0.22f, Reticle42TapSnapS = 0.05f;

        /// <summary>The KESTREL's cannon (after a charged shot): the snap (s); the turn is design.n42.charge42.cannonTurnS (0.46 s) less this, so
        /// the full turn spins in 0.40 s (NIGHT40: 1.70 + 0.12 s, with taps firing through it).</summary>
        public const float Reticle42CannonSnapS = 0.06f;
    }
}
