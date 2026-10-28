// NIGHT31 builder G (item 1b / 1d, design.n31.grappleMark; P0 d, design.n31.lowHealthEdge): Grapple31View's and the low-health edge's Unity-only view
// numbers (CLAUDE.md rule 5: each with a comment). The pull's gameplay numbers (speed, tap window, arrival pop) are design.n31.grapplePull rows, read from
// config, never typed here.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the target mark: a cyan diamond on what a press would take this frame (rule 8: cyan is player-usable; never red-orange) ---------------------
        public const float Grapple31MarkAngleDeg = 1.4f; // the diamond's half-size as an angle from the eye, so it reads the same at 5 m and 50 m (about 26 px at 1080p, 75 deg FOV)
        public const float Grapple31MarkBand = 0.24f; // the outline's thickness as a share of the half-size (a hollow diamond: the target shows through)
        public const float Grapple31MarkGain = 2.2f; // additive brightness x the cyan (over 1: bloom carries it against the dusk sky)
        public const float Grapple31MarkPulseHz = 2.5f; // a gentle +-12 % breath so the eye finds it (far under the 3-flashes-a-second limit: a sine, no flash)
        public const float Grapple31MarkPulseDepth = 0.12f; // the breath's depth
        public const float Grapple31MarkLift = 0.35f; // m pulled back toward the eye from the target point (a surface point would clip half the diamond into the wall)
        public const float Grapple31MarkNodeScale = 1.35f; // a node's diamond is drawn larger, around the node's 1.2 m ring (it frames the ring, never covers it)
        public const float Grapple31MarkEnemyScale = 1.6f; // an enemy target's diamond frames the enemy's aim point

        // --- the pull's feel: an FOV kick while a tap pulls you in (DESIGN31 B: +8 deg over 0.12 s, a guess to read on a frame) ---------------------------
        public const float Grapple31FovKickDeg = 8f; // degrees added to the camera's vertical FOV during a pull (x FOV KICK %; none with REDUCED MOTION)
        public const float Grapple31FovInSeconds = 0.12f; // s to reach the whole kick after the pull starts
        public const float Grapple31FovOutSeconds = 0.25f; // s back to none after the pull lets go (slower out than in: the speed is still there)

        // --- the retracting rope: bright beads running down the rope toward the player at the pull speed ----------------------------------------------------
        public const int Grapple31Beads = 5; // beads drawn on the rope during a pull
        public const float Grapple31BeadSpacing = 2.2f; // m between beads along the rope
        public const float Grapple31BeadLength = 0.55f; // m: each bead's length
        public const float Grapple31BeadWidthMul = 2.2f; // x the rope's width
        public const float Grapple31BeadGain = 2.4f; // additive brightness x the rope's cyan

        // --- the low-health red at the edges only (P0 d, design.n31.lowHealthEdge; UiEdge31.cs, PostFx.cs) -----------------------------------------------
        // round 2 (the lead's r2 frame on/edge31 f0107: "the red band is about 15 % of the width each side: make it thinner (about half) and softer"):
        public const float Edge31HurtClear = 0.86f; // the HUD hurt / low-HP vignette: clear inside 86 % of the half-size on each axis (the outer 7 % of the width and of the height carry the red; r1: 0.72 = 14 %)
        public const float Edge31DamageClear = 0.88f; // the HUD damage vignette (NIGHT16's radial one started at 38 %): clear inside 88 % of the half-size on each axis (r1: 0.76)
        public const float Edge31HurtEdgeAlpha = 0.8f; // the hurt / low-HP frame's alpha at the very edge (x the widget's own alpha), softer than r1's full 1
        public const float Edge31DamageEdgeAlpha = 0.7f; // the damage frame's alpha at the very edge (r1 used NIGHT16's 0.95)
        public const float Edge31PostMaxIntensity = 0.34f; // PostFx's URP vignette intensity cap at low HP (NIGHT30 lerped up to the fitted hurt intensity, darkening deep into the frame; r1: 0.42)
        public const float Edge31PostSmoothness = 0.2f; // PostFx's URP vignette smoothness cap at low HP: a tighter falloff, so the centre stays clean (r1: 0.25)

        // --- round 2, the target readable at a glance (the lead's r2 frames ring31 f0028 / grapple31 f0018: "a cyan rounded block in front of a big bright WHITE rectangle") ---
        public const float Grapple31RingBand = 0.045f; // m: GrappleNodes30's ring band with the mark's row on (NIGHT30: 0.1): a thin outline
        public const float Grapple31RingGlowGrow = 0.08f; // m: its glow ring's reach outside the ring (NIGHT30: 0.22, which bloomed into a block at 4.5 m)
        public const float Grapple31RingGlowShare = 0.15f; // the glow's brightness share of the ring's (NIGHT30: 0.25)
        public const float Grapple31RingAimedGain = 1.3f; // the aimed ring's gain (NIGHT30: 1.8; the diamond now says "this one")
        public const float Grapple31PlateClear = 1.8f; // m on the floor plan: a ceiling light panel this close over a grapple node is not drawn (its real light stays): the 2 m plates sat straight over s2 / s3's nodes
        public const float Grapple31MarkLineDeg = 0.28f; // the diamond's outline as an angle from the eye: about 4 px at 1080p with a 75 deg FOV, at any distance (its band share follows)
        public const float Grapple31MarkBandMin = 0.03f; // the band share's floor (a node's 1.1 m diamond at 4.5 m: 3.3 cm, never thinner)
        public const float Grapple31MarkNodeFrame = 1.1f; // m: a node's diamond half-size at least this (its inscribed circle 0.78 m clears the ring's 0.6 m + the 0.08 m glow)
    }
}
