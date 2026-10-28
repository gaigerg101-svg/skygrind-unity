// NIGHT32 builder G (design.n32.grapplePips, a view row): GrapplePips32's Unity-only view numbers (CLAUDE.md rule 5: each with a comment). The grapple's
// gameplay numbers (charges, recharge, gap, range) are design.n32.grapple32 rows, read from config, never typed here.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const int Grapple32PipSort = 12; // canvas order: over the HUD's canvas (10) and Hud29's (11), under the impact frame and flash canvases
        public const float Grapple32PipRefW = 1920f, Grapple32PipRefH = 1080f; // the CanvasScaler's reference (the 1080p layout, like Hud29)
        public const float Grapple32PipSize = 19.5f; // px at 1080p: one pip's side (a diamond: the square turned 45 degrees, echoing Grapple31View's target mark); the lead's n32-r0 frame f0040: 13 px (about 16-18 px tip to tip) read small: 1.5x, about 28 px tip to tip
        public const float Grapple32PipGap = 30f; // px between the two pips' centres (side by side; 1.5x with the size, so the gap between the tips stays a third of a pip)
        public const float Grapple32PipX = 48f; // px right of the screen centre (the reticle) to the first pip's centre: its inner tip about 34 px out, clear of the reticle's spread
        public const float Grapple32PipY = -32f; // px below the centre: under the reticle's right arm, off the line of fire (moved down with the larger pip)
        public const float Grapple32PipEmptyAlpha = 0.28f; // a spent pip's outline / backing: still there, so "two" reads even when empty
        public const float Grapple32PipFillAlpha = 0.65f; // the refilling part of a spent pip (rising from the bottom as the charge comes back)
        public const float Grapple32PipFlashSeconds = 0.25f; // a pip that just came back flashes white-cyan this long (once per charge, never a strobe)

        // --- hud32 (UiHud32.cs) ---
        public const double Hud32BossLineMax = 4.5; // s: a boss's teaching line never stays up longer (the Lantern's lineSeconds is 3, plus the widget's fades and a margin)
    }
}
