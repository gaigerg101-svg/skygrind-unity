// NIGHT39 builder P1 (the pistol; NIGHT39.md P1; GG 2026-10-13: the pistol "should feel heavier, slower, and more impactful", the special shot "a
// cooler, more powerful-looking animation", the crosshair "that little rotation animation" like the shotgun's): the Unity-only numbers of Pistol39View
// (the KESTREL's kick, its long settle, the press's cock, the bigger muzzle, the cannon shot) and the KESTREL's part of hud38's spin. Render and UI
// numbers only (CLAUDE.md rule 5); the gameplay numbers (the hold rule, the interval, the hit freezes) are design.n39.p39's. All of them are first
// picks for the lead's frames (L-001): each says what it is measured against.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the tap's kick: the gun (the viewmodel's root, on top of its own kick) -------------------------------------------------------------
        /// <summary>The tap's extra gun pitch-up (deg) at the kick's peak, on top of the KESTREL's own spring kick (HandsKickRevolverA/B): a heavy
        /// revolver snaps up hard and comes down slowly (ULTRAKILL's revolver throws the hand up ~20 deg; this adds about half of that).</summary>
        public const float Pistol39KickPitchDeg = 9f;
        /// <summary>The tap's extra push back toward the eye (m) at the peak.</summary>
        public const float Pistol39KickBack = 0.05f;
        /// <summary>The tap's extra roll (deg, + = clockwise as the player sees it) at the peak: the wrist turns with the recoil.</summary>
        public const float Pistol39KickRollDeg = 5f;
        /// <summary>The tap kick's attack (s, frame time: it plays through a freeze): the snap is one or two frames at 60 fps.</summary>
        public const float Pistol39KickAttackS = 0.03f;
        /// <summary>The tap kick's settle (s): the "longer settle" (NIGHT39.md P1). The KESTREL's own kick settles in ~0.15 s; this one takes about
        /// two thirds of the slower tap interval (0.483 s, design.n39.p39.intervalMult), with one small dip below rest (Pistol39Math.Settle).</summary>
        public const float Pistol39KickSettleS = 0.32f;

        // --- the tap's kick: the camera ---------------------------------------------------------------------------------------------------------
        /// <summary>The tap's view pitch-up (deg) at the peak, x CAMERA SHAKE (CameraFeel.ShakeScale), none with REDUCED MOTION; visual only (the sim's
        /// aim is the look, not the camera).</summary>
        public const float Pistol39CamPitchDeg = 1.2f;
        /// <summary>The view kick's attack and settle (s).</summary>
        public const float Pistol39CamAttackS = 0.03f, Pistol39CamSettleS = 0.22f;

        // --- the press's cock (p39Press: the instant answer to a press that may become a charge) -------------------------------------------------
        /// <summary>The press's gun pull-back (m) and pitch-down (deg): the hammer cocks on the press frame, so a press is seen at once even though the
        /// tap fires on the release (INVERSUS's instant press-time cue, research/feel-web.md section 3).</summary>
        public const float Pistol39CockBack = 0.015f, Pistol39CockPitchDeg = -2f;
        /// <summary>The cock's attack and settle (s): held while the press is held is not needed (a hold turns into the charge's own glow).</summary>
        public const float Pistol39CockAttackS = 0.05f, Pistol39CockSettleS = 0.14f;

        // --- the bigger muzzle ------------------------------------------------------------------------------------------------------------------
        /// <summary>The tap's muzzle bloom: a cyan (colors.usable) additive ball at the muzzle (WeaponFx's star still draws): diameter (m), life (s),
        /// brightness x the usable colour.</summary>
        public const float Pistol39MuzzleSize = 0.22f, Pistol39MuzzleLifeS = 0.06f, Pistol39MuzzleGlow = 2.2f;

        // --- the cannon shot (the charged KESTREL shot) ------------------------------------------------------------------------------------------
        /// <summary>The cannon's gun kick (deg up, m back, deg roll) at the peak, its attack and its settle (s): about 2.5x the tap's, settling over
        /// the next 0.6 s (a cannon's slow return).</summary>
        public const float Pistol39CannonPitchDeg = 24f, Pistol39CannonBack = 0.13f, Pistol39CannonRollDeg = 10f;
        public const float Pistol39CannonAttackS = 0.03f, Pistol39CannonSettleS = 0.6f;
        /// <summary>The cannon's view pitch-up (deg), attack and settle (s), x CAMERA SHAKE (none with REDUCED MOTION).</summary>
        public const float Pistol39CannonCamDeg = 3.5f, Pistol39CannonCamAttackS = 0.03f, Pistol39CannonCamSettleS = 0.45f;
        /// <summary>The cannon's FOV punch-out (deg, added to the frame's FOV) and its decay (s), x FOV KICK (CameraFeel.FovKickScale).</summary>
        public const float Pistol39CannonFovDeg = 7f, Pistol39CannonFovS = 0.3f;
        /// <summary>The cannon's trauma (game.trauma, the web's shake, 0..1), on top of pierce33's own PierceTrauma33.</summary>
        public const float Pistol39CannonTrauma = 0.3f;
        /// <summary>The cannon's muzzle blast ball: diameter (m), life (s), brightness.</summary>
        public const float Pistol39BlastSize = 0.75f, Pistol39BlastLifeS = 0.14f, Pistol39BlastGlow = 3f;
        /// <summary>The cannon's shock ring at the muzzle, facing along the shot: diameter from / to (m), life (s), thickness (m), brightness.</summary>
        public const float Pistol39RingFrom = 0.25f, Pistol39RingTo = 2.6f, Pistol39RingLifeS = 0.28f, Pistol39RingThick = 0.03f, Pistol39RingGlow = 2.2f;
        /// <summary>The cannon's beam from the muzzle to the shot's end (kestrelPierce33's `to`): an outer cyan sleeve and a hot core, widths (m) at
        /// the start, both thinning to 0 over the life (s); brightness of each. pierce33's own line still draws inside it.</summary>
        public const float Pistol39BeamWidth = 0.32f, Pistol39BeamCoreWidth = 0.09f, Pistol39BeamLifeS = 0.4f, Pistol39BeamGlow = 1.6f, Pistol39BeamCoreGlow = 4f;
        /// <summary>The beam's farthest drawn length (m): a shot into the sky ends here.</summary>
        public const float Pistol39BeamMaxLen = 120f;

        // --- the KESTREL's part of hud38's crosshair spin (UiHud38.Kestrel39.cs) -------------------------------------------------------------------
        /// <summary>Spin the crosshair through the KESTREL's charge (a full turn from tapS to FULL, the snap at FULL) and through each tap's interval
        /// (a quarter turn: the four ticks land where they started, like the THUNDERCLAP's pump turn). false: hud38's rule as NIGHT38 left it.</summary>
        public const bool Hud39KestrelSpin = true;
        /// <summary>The tap interval's turn (deg): a quarter turn (the reticle's 4-fold symmetry hides the end of the turn).</summary>
        public const float Hud39TapSpinDeg = 90f;
    }
}
