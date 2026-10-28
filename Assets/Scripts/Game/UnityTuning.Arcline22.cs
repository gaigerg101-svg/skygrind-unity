// NIGHT22 U3-36.3 (builder M): the ARCLINE chip's render numbers (Game/UiArcline22.cs). UNCOMPILED when written. Unity-only: the web had
// no gadget chip. Layout px are at the 1080p reference the HUD's other chips use (UiTheme.Px); colours come from UiTheme (cyan is
// player-usable; the NO TARGET look is a neutral dim, never red: CLAUDE.md rule 8, DECIDE22 D-3).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The chip's ring (diameter, band width) and where it sits from the crosshair (right and below it, clear of the arms and the hit marker).
        public const float Arcline22ChipD = 44f, Arcline22RingW = 6f, Arcline22ChipX = 104f, Arcline22ChipY = -58f;
        // The label beside the ring: its box (px); its text is the theme's S18 (18 px at 1080p, M10's floor).
        public const float Arcline22LabelW = 150f, Arcline22LabelH = 30f, Arcline22LabelGap = 10f;
        // NO TARGET: the reticle's arms dim to this alpha (RESEARCH R7: a quiet reticle, 40 %), the ring is broken into this many gaps of this
        // many degrees and drawn at this alpha (a different SHAPE from READY's whole ring and COOLDOWN's partial arc, not only a colour).
        public const float Arcline22NoTargetAlpha = 0.4f, Arcline22NoTargetRingAlpha = 0.4f;
        public const int Arcline22NoTargetGaps = 4;
        public const float Arcline22NoTargetGapDeg = 24f;
        // COOLDOWN: the dim arc's alpha (the remaining share, emptying clockwise) and the label's.
        public const float Arcline22CooldownArcAlpha = 0.6f, Arcline22CooldownLabelAlpha = 0.9f;
        // The look changes ease this long (s) so the ring and the reticle never pop between READY / COOLDOWN / NO TARGET.
        public const double Arcline22EaseTime = 0.15;
        public const int Arcline22ArcSteps = 48;
        // FeelProbe events the reel's metrics read: when the chip's look changes.
        public const string Arcline22ProbeReady = "arclineChipReady", Arcline22ProbeCooldown = "arclineChipCooldown", Arcline22ProbeNoTarget = "arclineChipNoTarget";
    }
}
