// NIGHT40 builder W1 (NIGHT40.md W1, "the pistol in its rhythm"; GG 2026-10-13 ~22:00, Research/2026-10-14-wave40/GG-REQUEST.md): every KESTREL
// rotation ends inside its shot's rhythm. The rule: each animation a KESTREL shot starts (the gun's kick and settle, the view kick, the NIGHT36
// hammer's re-cock, the crosshair's turn) ends within Pistol40Share of the time to the next shot (a tap: the 0.483 s interval, design.n39.p39's
// x1.2; the charged shot: its 2.5 s charge cooldown, design.n35.charge35.revolver.cooldownS), with an ease-out (fast first, slow into rest; no dip
// below rest). research/feel-web.md section 5: no published rule ties an animation to a share of the fire interval (the Unity forum's "the fire
// animation as long as the fire delay" is the nearest), so 0.75 is a hypothesis for the lead's frames (L-001).
// Live while design.n40.charge40 is live in the run (Charge35.c40; Pistol39View.Rhythm40), so --off40 / -skygrindOff40 is NIGHT39's timing.
// Render and UI numbers only (CLAUDE.md rule 5). The ends (attack + settle) are checked headless by `n40weapons rhythm` (Harness.N40.Weapons.cs).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The cap: every rotation a KESTREL shot starts ends within this share of the time to its next shot (hypothesis; see the header).</summary>
        public const double Pistol40Share = 0.75;

        // --- the gun (Pistol39View): the tap's kick, ease-out ------------------------------------------------------------------------------------
        /// <summary>The tap kick's attack and settle (s). NIGHT39: 0.03 + 0.32 = 0.35 s (72 % of 0.483 s) with a dip below rest; now 0.03 + 0.27 =
        /// 0.30 s (62 %), an ease-out ((1 - u)^2: fast off the peak, slow into rest), so the gun is still before the next press can land.</summary>
        public const float Pistol40KickAttackS = 0.03f, Pistol40KickSettleS = 0.27f;
        /// <summary>The view's tap kick (s): NIGHT39 0.03 + 0.22 = 0.25 s; now 0.03 + 0.20 = 0.23 s (48 %), ease-out.</summary>
        public const float Pistol40CamAttackS = 0.03f, Pistol40CamSettleS = 0.2f;
        /// <summary>The cannon (the charged shot) keeps NIGHT39's 0.03 + 0.6 s gun settle and 0.03 + 0.45 s view settle (25 % / 19 % of the 2.5 s
        /// cooldown), now with the ease-out shape (no dip).</summary>
        public const bool Pistol40EaseOut = true;

        // --- the crosshair's KESTREL spin (H1 wires these: UiHud38.Kestrel39.cs's rule, re-timed) ---------------------------------------------------
        /// <summary>After a TAP: the quarter turn (Hud39TapSpinDeg 90) runs this long from the shot (ease-out), then the snap (Pistol40TapSnapS): ends
        /// at 0.28 + 0.06 = 0.34 s (70 % of the 0.483 s interval). NIGHT39 turned through the whole interval (100 %) and snapped after it.</summary>
        public const float Pistol40TapSpinS = 0.28f, Pistol40TapSnapS = 0.06f;
        /// <summary>The snap at FULL (the charge's turn ends on the FULL frame: charge40's 0.9 s from the press): hud38's snap is 0.12 s; 0.08 s here
        /// so the FULL cue lands as a beat, not a slide.</summary>
        public const float Pistol40ChargeSnapS = 0.08f;
        /// <summary>After the CHARGED shot: the full turn (Hud38SpinDeg 360) runs this long (ease-out), then the snap: ends at 1.70 + 0.12 = 1.82 s
        /// (73 % of the 2.5 s charge cooldown). NIGHT38 / 39 turned through the whole cooldown (100 %).</summary>
        public const float Pistol40CannonSpinS = 1.7f, Pistol40CannonSnapS = 0.12f;

        // --- the slug's look (the charged THUNDERCLAP shot, charge40 live; Pistol39View) -----------------------------------------------------------
        /// <summary>The slug's extra gun kick (deg up, m back, deg roll) at the peak, attack and settle (s; ease-out): about 1.5x the KESTREL cannon's
        /// pitch-back on the heavier gun, settled in 0.53 s (35 % of the THUNDERCLAP's ~1.52 s interval).</summary>
        public const float Slug40KickPitchDeg = 16f, Slug40KickBack = 0.18f, Slug40KickRollDeg = -8f;
        public const float Slug40KickAttackS = 0.03f, Slug40KickSettleS = 0.5f;
        /// <summary>The slug's view kick (deg up, x CAMERA SHAKE, none with REDUCED MOTION), attack and settle (s), and its FOV punch (deg, decay s,
        /// x FOV KICK).</summary>
        public const float Slug40CamDeg = 4.5f, Slug40CamAttackS = 0.03f, Slug40CamSettleS = 0.4f;
        public const float Slug40FovDeg = 8f, Slug40FovS = 0.3f;
        /// <summary>The slug's trauma (game.trauma, 0..1): the KESTREL cannon's is 0.3; the slug is the heavier blow.</summary>
        public const float Slug40Trauma = 0.4f;
        /// <summary>The slug's muzzle blast ball: diameter (m), life (s), brightness x the usable colour (cyan).</summary>
        public const float Slug40BlastSize = 1.1f, Slug40BlastLifeS = 0.16f, Slug40BlastGlow = 3.2f;
        /// <summary>The slug's shock ring at the muzzle, facing along the shot: diameter from / to (m), life (s), thickness (m), brightness. Wider and
        /// slower than the KESTREL cannon's (0.25 -> 2.6 m over 0.28 s).</summary>
        public const float Slug40RingFrom = 0.4f, Slug40RingTo = 3.6f, Slug40RingLifeS = 0.32f, Slug40RingThick = 0.05f, Slug40RingGlow = 2.6f;
    }
}
