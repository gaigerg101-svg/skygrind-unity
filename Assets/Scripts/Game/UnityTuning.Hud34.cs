// NIGHT34 builder H (design.n34.rail34, a view row: no sim bit): THE ARSENAL RAIL's layout and look numbers (CLAUDE.md rule 5: Unity-only, each with
// a comment). Every px is at 1080p in HUD33's canvas units (1920 x 1080, scaled with the height). Reference first (NIGHT34 method 1):
// Research/2026-10-11-wave34/research/swap-hud.md recommendation 7 (ULTRAKILL's large current-weapon icon tinted, plus an always-on strip of all
// slots with the current one enlarged and each slot's cooldown shown) and 8 (a holstered weapon turning ready gets a one-shot visual pulse). No
// game's strip sizes were found (swap-hud.md section 4: "not found"), so the sizes below are HUD33's own grid (its 360 px rule, its 54 / 72 px
// margins), not a copy.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the rail (bottom right, where hud33's pip row was) ------------------------------------------------------------------------------
        public const int Rail34MaxSlots = 6; // slots shown at most (a window round the hand past that); 6 x Rail34SlotPitch = 336 px, under HUD33's 360 px rule
        public const float Rail34SlotPitch = 56f; // px from one slot's centre to the next (a 48 px icon and an 8 px gap)
        public const float Rail34IconW = 48f; // the held gun's icon width, px (the silhouette's 2:1 texture: 48 x 24)
        public const float Rail34IconH = 24f; // the held gun's icon height, px
        public const float Rail34DimScale = 0.72f; // a holstered slot's icon at 72 % of the held one's size (the hand reads larger at a glance)
        public const float Rail34LineW = 36f; // the outline line under each slot (the cooldown's drain): px wide
        public const float Rail34LineH = 3f; // ... px tall (HUD33's 3 px meter line, the rank line's height)
        public const float Rail34LineGap = 5f; // px from the icon row's bottom down to the outline line
        public const float Rail34HandBarW = 44f; // the hand's cyan slash under its slot (the highlight that slides on a swap), px wide
        public const float Rail34HandBarH = 4f; // ... px tall (one px over the outline line, so it reads as the mark, not a meter)
        public const float Rail34NameGap = 8f; // px from the rail's top up to the LT label (the held gun's name sits over the LT label, as hud33's)

        // --- the outline icon (a holstered gun: the silhouette's edge only, never a filled shape) ---------------------------------------------
        public const int Rail34OutlinePx = 6; // the outline's thickness in texture px: the 128 px wide silhouette (Hud17CardSilTex) drawn 35 px wide holstered, so about 1.6 canvas px
        public const float Rail34DimAlpha = 0.45f; // a holstered slot's outline opacity at rest (dim: the hand is the read)
        public const float Rail34BendDimAlpha = 0.75f; // ... inside a bend (sim.Bend34Depth01 = 1): the rail brightens one step (DESIGN: "inside a bend the rail brightens")
        public const float Rail34HeldAlpha = 1f; // the held gun's filled cyan icon
        public const float Rail34LineAlpha = 0.55f; // a ready slot's outline line opacity (bone; the hand's line is hidden under its cyan bar)
        public const float Rail34CoolLineAlpha = 0.9f; // a cooling slot's draining line (CyanDeep: never red, red-orange is danger only: CLAUDE.md rule 8)

        // --- motion -----------------------------------------------------------------------------------------------------------------------
        public const double Rail34SlideS = 0.08; // NIGHT34 H1: a swap slides the highlight (and the icon sizes) to the new slot over 80 ms: a swap read, not a delay
        public const double Rail34PulseS = 0.28; // a holstered gun turning ready (HudWeapons30's READY turn rule): one pulse of its slot, in and out over 280 ms
        public const float Rail34PulseScale = 0.3f; // ... the pulse grows the icon by 30 % at its peak (and lifts it to full opacity)
        public const string Rail34SwapEvent = "rail34Swap"; // FeelProbe marker and GG's sound hook name (PORT-STATUS NIGHT34 events)
        public const string Rail34ReadyEvent = "rail34Ready"; // FeelProbe marker: a holstered slot's READY pulse began
    }
}
