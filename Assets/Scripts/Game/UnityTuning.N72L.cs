// NIGHT72 builder L (LIGHT & SCREEN): Unity-only numbers for L's slices (CLAUDE.md rule 5: each with a comment; the web never had any of them, and the
// sim never reads them: golden vectors, replays and fingerprints stay as they were). L1's reel numbers first; L3 / L4 / L6 / L2 add theirs below.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- L1: the flicker meter's still-camera reel (FeelReel.N72.L.cs; Tools/night/n72-flicker.py reads what it writes) ----
        public const int L72StillFrames = 60; // rendered frames held at each pose (the slate's L1: "60 rendered frames"; 1 s at ReelFps 60, long enough for a 3-flip test)
        public const int L72StillSettleFrames = 30; // frames at the pose before the first one is kept (0.5 s: the camera cut, the bloom and the room's lights settle)
        public const int L72PlantSize = 32; // px: the planted case's block (the slate's L1: "a 32x32 block toggled every frame by -skygrindFlickerPlant72")
        public const int L72PlantX = 160; // px from the left edge: the planted block's corner (over the left wall, clear of the HUD's rail and the crosshair)
        public const int L72PlantY = 540; // px from the top edge: the planted block's corner (mid-height)

        // ---- L3: lit72, the boss arenas' and level 5's shadow lift (Lit72.cs; Crt29.shader's Lit72 block; Tools/night/n72-lit-sim.py is its offline model) ----
        // Chosen on the n71-forge72 frames with n72-lit-sim.py (Research/2026-10-27-wave72/L/evidence/lit72-sim.txt): dark < 30 share 9-48 % -> at most 11 %.
        public const float L72LitA = 0.12f; // display luma (0-1) added at black in the shadows, in each pixel's own hue: 0.12 = 31 of 255, so a shadow clears the "dark < 30" line
        public const float L72LitL0 = 0.12f; // display luma under which the lift is whole (0.12 = 31 of 255)
        public const float L72LitL1 = 0.42f; // display luma from which a pixel is left alone (0.42 = 107 of 255): lights, tells and the player's cyan keep their value
        public const float L72LitKMax = 9f; // the most a pixel's colour is scaled by (a near-black pixel stays dark rather than turning to a grey of the floor). 9, not 4.5: THE LOOM's and the iceline boss's arenas
                                            // are mostly (3, 3, 11): at 4.5 their dark share stayed 29-41 % in the model, at 9 it is 14-23 %; the wyrm, THE CROWN and the spire move by under 0.5 points (lit72-sim.txt)
        public const float L72LitFadeInS = 0.8f; // s for the lift to come up on entering a boss room or level 5 (no pop; a boss fight's own grades fade in over 0.8 s)
        public const float L72LitFadeOutS = 0.8f; // s for the lift to go on leaving
        public const int L72LitSpireTheme = 4; // LightPlan33.Themes index of "spire": level 5, all its rooms are lifted (gg-proxy #1: spire v2 / v5 / v6 are 23-30 % dark)
        public const float L72LitMinA = 0.002f; // a lift under this luma counts as 0 (the pass is not forced on for nothing)
        public const bool L72LitProbeFloor = false; // the ambient probe built from Bright71's floors in boss rooms and level 5 (Lit72.FloorProbe): false until a Player side measures it (-skygrindLit72Probe=floor tries it)
        public const float L72LitProbeK = 1f; // the ambient probe's scale in boss rooms and level 5 at full weight (1 = untouched): Stone.shader lights with the probe, and Bright71's ambient floors never reach it; -skygrindLit72Probe=<k> tries a lift (Lit72.cs), unmeasured so off

        // ---- L4: calm72, no red border at full health; the low-health edge thin and only under 30 % (Calm72.cs, PostFx.cs's low-HP line) ----
        public const float L72CalmEdgeBelow = 0.30f; // fraction of max health under which an edge may show (the slate's L4: "under 30 %"; the web's post.lowHpAt is 0.45)
        public const float L72CalmEdgeClear = 0.94f; // the thin frame's clear share of the half-size on each axis: the ramp is 58 px deep at 1920 wide, 32 px at 1080 high (the slate's bar: at most 60 px; UiEdge31's 0.86 / 0.88 are 135 px)
        public const int L72CalmProbeMax = 200; // [calm72] probe lines per run (a changing HUD cannot flood the log)

        // ---- L5: the grow plan (GrowPlan72.cs; Tools/night/n72-zscan.cjs is the same algorithm in node, and the tool that counts the pairs) ----
        public const int L72GrowClasses = 6; // growth classes a box can take (Bright71GrowStep x (1 + class): 2 .. 12 mm; every decor lift, 20-80 mm, stays clear of the largest)
        public const double L72GrowSepGood = 0.004; // m: the gap two coplanar faces should keep after growth (4 mm; at 2 mm a far wall can still fight at 60 m on a 24-bit depth buffer)
        public const double L72GrowMinArea = 0.01; // m2: two same-facing coplanar faces overlapping by less than this (10 cm x 10 cm) are not a conflict (a sliver shows nothing)
    }
}
