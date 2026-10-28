// NIGHT38 builder H (design.n38.hud38, a view row: no sim bit): the charge bar under the crosshair, the crosshair's reload spin, the grapple's hook
// icon (UiHud38.cs). CLAUDE.md rule 5: Unity-only numbers, each with a comment. Every px is at 1080p, relative to the reticle's centre (the HUD
// canvas scales with the height, as hud33's). ULTRAKILL's rule (research/uk-hud-feel.md section 1): one colour while charging, another when full, a
// white flash that fades on the frame it fills.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the charge bar under the crosshair ----------------------------------------------------------------------------------------------
        // NIGHT39 V (the lead's n38-r1 frame f0800, THE YARD: "three small cyan dashes", too thin for GG's "easy to tell"): 60 x 4 px with 4 px gaps
        // -> 108 x 10 px with 8 px gaps (each cell 30.7 x 10 px, was 17.3 x 4: 4.4x the area); the bar moves down so its top stays 7 px clear of
        // hud33's bottom tick (18 px out), and the grapple row under it.
        public const float Hud38BarY = -30f; // px: the bar's centre under the reticle (top at -25: 7 px clear of hud33's bottom tick, which ends 18 px out)
        public const float Hud38BarW = 108f; // px: the whole bar's width (segments + gaps); under hud33's 360 px cap, about 5.6 % of the screen's width
        public const float Hud38BarH = 10f; // px: each segment's height (a solid cell that reads at a glance; still under the crosshair's 14 px ticks)
        public const float Hud38BarGap = 8f; // px between segments (ULTRAKILL's stamina reads as three cells because of the gaps; wider with the bigger cells)
        public const int Hud38Segments = 3; // GG: "three bars ... a bar under the crosshair": a charge / a cooldown fills three cells in turn
        public const int Hud38MaxSegments = 6; // the gauntlet's BLAST shows one cell per kill pip (design.n27.pulse27.pips, 5 today); never more than this
        public const float Hud38TrackAlpha = 0.28f; // the empty cell's track (bone at this alpha: the bar's length reads before it fills)
        public const float Hud38ChargingAlpha = 0.9f; // the filled part while charging (bone: not cyan, which means usable; never red-orange, which is danger)
        public const float Hud38RestAlpha = 0.55f; // a full bar at rest (after the ready flash): still cyan, quieter, so a NEW fill stands out
        public const double Hud38FlashTime = 0.35; // s: the ready flash, white fading to cyan (ULTRAKILL fades its flash at 1 per second from alpha 1)
        public const double Hud38FullHold = 0.6; // s: the bar stays at full brightness this long after the flash, then eases to Hud38RestAlpha
        public const double Hud38RestEase = 0.4; // s: the ease from full brightness to Hud38RestAlpha
        public const float Hud38FlashGrow = 4f; // px (NIGHT39 V: 2 -> 4 with the 10 px cells): each cell grows this much taller on the flash's first frame (shrinking back with the flash)
        public const double Hud38HideAfter = 1.2; // s: a gun with only a cooldown meter (no charge, no pips) hides its bar this long after it is ready
        public const float Hud38FillStep = 0.02f; // fills are rounded to this (Image.fillAmount written on a change only, as hud33's)

        // --- the crosshair's reload spin ------------------------------------------------------------------------------------------------------
        public const float Hud38SpinDeg = 360f; // deg per reload / cooldown cycle: a full turn (GG: "the crosshair rotating while reloading ... more of a thing")
        public const double Hud38SnapTime = 0.12; // s: the snap back to square when ready (an out-back ease, so it lands with a small overshoot)
        public const float Hud38SnapPush = 3f; // px: the ticks push out on the snap (hud33's hit push is 3 px: the same language)

        // --- the grapple: a hook icon with two charge pips ------------------------------------------------------------------------------------
        public const float Hud38GrappleY = -52f; // px: the grapple row's centre, under the bar (hook top -46: 11 px under the bar's bottom at -35; NIGHT39 V, was -40 under the 4 px bar; hud33's two slash ticks at -26 hide with hud38)
        public const float Hud38HookX = -12f; // px: the hook icon's centre
        public const float Hud38HookShaftH = 12f; // px: the hook's shaft (a vertical line)
        public const float Hud38HookW = 2f; // px: every hook stroke's width
        public const float Hud38HookCurveW = 7f; // px: the bottom of the J (to the right of the shaft)
        public const float Hud38HookBarbH = 5f; // px: the barb rising from the J's end
        public const float Hud38HookEyeW = 6f; // px: the eye bar across the shaft's top (where the line ties on)
        public const float Hud38PipX0 = 2f; // px: the first charge pip's centre
        public const float Hud38PipPitch = 9f; // px between the two pips
        public const float Hud38PipW = 5f; // px: a pip (a slash tick, hud33's motif)
        public const float Hud38PipH = 11f; // px
        public const float Hud38HookEmptyAlpha = 0.35f; // the hook with no charge left (dim cyan: the verb is not usable right now)
        public const float Hud38PipSpentAlpha = 0.25f; // a spent pip's track
        public const float Hud38HookPop = 0.35f; // the hook's scale pop on a throw (1 + this, back over Hud38HookPopTime)
        public const double Hud38HookPopTime = 0.18; // s
        public const float Hud38BendX = 24f; // px: the NIGHT34 bend meter's slash moves to this x on the grapple row while hud38 is on (it sat where the bar is)

        // --- events (names only: GG's sounds; CLAUDE.md rule 7) -------------------------------------------------------------------------------
        public const string Hud38ReadyEvent = "hud38Ready"; // a meter filled this frame (FeelProbe event + Hud38.Ready(id): id = the gun, "blast" or "grapple")
    }
}
