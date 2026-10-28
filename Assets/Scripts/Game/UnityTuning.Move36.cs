// NIGHT36 builder M (NIGHT36.md M1): Unity-only numbers for the movement builder's view rows (CLAUDE.md rule 5: each with a comment). Every px is at
// 1080p (the HUD canvas scales with the height from 1920 x 1080).
// - text36 (design.n36.text36, REVIEW.md root cause 10; GG 2026-10-12: "the tutorial text is way too high up"): the teaching slot in the lower middle.
// - wallgrip36's camera: the ride's lean eased in and out (CameraFeel.UpdateRoll).
// - the ride trim (RideTrim36.cs): the cyan stripe on wall-ride panels.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- text36: the teaching slot ---------------------------------------------------------------------------------------------------
        public const double Hud36TeachCentre = 0.62; // NIGHT36.md M1: the slot's centre this share of the screen down from the top (under the crosshair, where the eye already is; the room name keeps the top slot)
        public const float Hud36TeachPx = 32f; // NIGHT36.md M1: 32 px at 1080p (the top slot's lines were 24 px: Hud33Name)
        public const float Hud36TeachH = 80f; // the slot's box: two lines of 32 px (TMP's line height ~1.2 em = 38 px each) inside the hud33 rule's 80 px height cap
        public const float Hud36TeachTextW = 528f; // the line's wrap width: with the padding the plate is at most 560 px (29 % of a 16:9 screen; the lead's n36-r1: "max ~44 %"), so the line sits between the crosshair and the KESTREL's viewmodel (x 1120-1250 at 1080p), wrapping to two lines
        public const float Hud36PlatePad = 16f; // the lead's n36-r1: the plate reaches 16 px past the lines on every side
        public const uint Hud36PlateHex = 0x0B0A1A; // the lead's n36-r1: a REAL dark plate (round 1's soft scrim was invisible: the white line's end ran onto the pale viewmodel)
        public const float Hud36PlateAlpha = 0.68f; // the lead's n36-r1: 60-70 % alpha
        public const float Hud36PlateEdgeW = 2f; // the plate's cyan left edge (the HUD's slash-cyan accent), px
        public const float Hud36TeachOutline = 0.22f; // the line's TMP outline width (SDF units, about 2 px at 32 px; the top slot's 0.28 was for 24 px), in the shadow colour

        // --- wallgrip36: the ride's lean -------------------------------------------------------------------------------------------------
        public const double Ride36RollSmoothS = 0.16; // s: the critically damped lean's settle time (the web's exponential ease at feel.rollEaseRate 10 /s reaches 63 % in 0.1 s but starts at full speed on the attach's frame; this starts and stops at rest)
        public const double Ride36RollSettleDeg = 0.05; // deg (and deg/s): the spring hands the roll back to the web's ease once this close to its target at rest

        // --- the ride trim (RideTrim36.cs) -----------------------------------------------------------------------------------------------
        public const float RideTrim36Height = 0.22f; // m: the stripe's height (twice NIGHT26's 0.12 m strip: readable as a stripe from 20 m)
        public const float RideTrim36TopDrop = 0.35f; // m: the upper stripe's centre under the panel's top edge (the panel's outline reads from below and above)
        public const float RideTrim36LowY = 1.6f; // m above the panel's base: the lower stripe's centre (about the eye: body.eyeHeight; the ride's own band, as NIGHT26's strip)
        public const float RideTrim36Offset = 0.04f; // m off the panel's face so the stripe never z-fights it
        public const float RideTrim36Brightness = 2.6f; // HDR gain on the usable cyan (colors.usable) so the bloom picks it up (NIGHT26's strip: 2.2)
        public const float RideTrim36EndW = 0.18f; // m: the vertical end caps' width (the panel's two ends lit top to bottom: "ride from here to here")
    }
}
