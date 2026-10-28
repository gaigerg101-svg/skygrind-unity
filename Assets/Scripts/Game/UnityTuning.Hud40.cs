// NIGHT40 builder H1 (design.n40.arc40, a view row: no sim bit; NIGHT40.md H1; GG 2026-10-13: "a single curved bar under the reticle instead of three
// small bars"): the numbers of the arc (UiHud38.Arc40.cs). CLAUDE.md rule 5: Unity-only numbers, each with a comment. Every px is at 1080p, relative
// to the reticle's centre (the HUD canvas scales with the height, as hud33's / hud38's). Colours and the flash's timing stay hud38's
// (UnityTuning.Hud38.cs: bone while charging, cyan full, the 0.35 s white flash, the rest alpha), so the arc speaks the same language as the bar.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the arc's shape --------------------------------------------------------------------------------------------------------------------
        // The lead's n40-r1 frames (r1kiln verbs37 f1300 / f1600 / f1750): "it works and reads as one curved bar, but it is small next to the crosshair"
        // for GG's "easier to see": about 1.4x (44 -> 60 px radius, 7 -> 10 px thick, 90 -> 104 deg: the band's area x2.3), and a dark under-stroke
        // so it reads on level 3's pale sand floors (f1600: the bar over a pale floor).
        public const float Hud40Radius = 60f; // px: the arc's OUTER edge from the reticle's centre (n40-r1: 44 -> 60); its lowest point sits 60 px under the dot
        public const float Hud40Thick = 10f; // px: the band's thickness (n40-r1: 7 -> 10; inner edge at 50 px, 32 px clear of hud33's ticks, which end 18 px out)
        public const float Hud40SweepDeg = 104f; // deg: the arc's length, centred straight down (n40-r1: 90 -> 104; -52 to +52 deg off the vertical: the ends at x = +-47 px, y = -37 px)
        public const float Hud40StepDeg = 2.5f; // deg: one vertex pair every this many degrees (about 2.6 px of the outer edge at 60 px: a smooth curve at 1080p)
        public const float Hud40SegGapPx = 5f; // px between two segments (the gauntlet's pips) along the outer edge (hud38's cells read as cells because of the gaps; 4 -> 5 with the bigger arc)
        public const float Hud40StrokePx = 1.5f; // px: the dark under-stroke round every band (each side and each end), in hud33's text shadow colour (Hud33ShadowHex at Hud33ShadowAlpha): the bone track reads on pale floors
        public const int Hud40MaxSegments = 6; // segments at most (the gauntlet's BLAST: one per kill pip, design.n27.pulse27.pips = 5 today); as Hud38MaxSegments

        // --- the fill ------------------------------------------------------------------------------------------------------------------------------
        public const float Hud40FillEasePow = 1.6f; // the charge's shown fill = level^this: slow first, fast at the end (Harrison et al. UIST 2007: accelerating bars feel faster; feel-web.md section 4). 1 = linear
        public const float Hud40DrainAlpha = 0.6f; // a cooldown: the arc DRAINS (the share left, bone at this alpha: quieter than a charge, never cyan until it is usable again)
        public const float Hud40FlashGrow = 3f; // px: the band grows this much outward on the ready flash's first frame, shrinking back with the flash (hud38's 4 px on a 10 px cell; 3 px on the 10 px band keeps the hook row clear)

        // --- the gauntlet's blast cooldown: a thin line inside the arc (the pips own the band) ---------------------------------------------------------
        public const float Hud40LineThick = 3f; // px: the cooldown line's thickness (n40-r1: 2 -> 3 with the bigger arc)
        public const float Hud40LineGap = 3f; // px between the band's inner edge and the line (room for both under-strokes; the line sits 44-47 px out: clear of hud33's ticks)
        public const float Hud40LineAlpha = 0.8f; // the line's alpha (bone: a cooldown is neutral, not usable)

        // --- the grapple row moves down under the arc ----------------------------------------------------------------------------------------------
        public const float Hud40GrappleY = -76f; // px: the grapple row's centre with the arc on (n40-r1: -58 -> -76 under the bigger arc: the hook's top at -70, 8.5 px under the stroke's bottom at -61.5, 5.5 px at the flash's grow; UiRail34's bend meter follows through GrappleRowY)
    }
}
