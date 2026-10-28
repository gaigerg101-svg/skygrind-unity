// NIGHT34 builder H (design.n34.bendLook, a view row: no sim bit): THE BEND's look numbers (CLAUDE.md rule 5: render / camera numbers, Unity-only,
// each with a comment). The bend's time numbers (scale, ease in / out, lengths) are the sim's (design.n34.bend34, builder B): the look follows
// sim.Bend34Depth01, which already carries the ease in 0.08 s and out 0.3 s. Reference first (NIGHT34 method 1): Research/2026-10-11-wave34/RESEARCH.md
// table B, "overlay: world desaturated, enemies red-orange, usable cyan, a step deeper per kill" (slowmo.md 5 and 9: Max Payne 2's deepening
// read) and "entry: a FOV thump, no freeze by default" (slowmo.md 2). No game's desaturation amount or thump size was found; Desat 0.65 and
// EntryFovDeg -4 are NIGHT34.md H1's starting values, judged on the bend34 reel's frames f1-f4.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the grade (PostFx: a ColorCurves hueVsSat curve on the post Volume, built only with the row on) --------------------------------
        public const float Bend34Desat = 0.65f; // NIGHT34 H1: the world loses 65 % of its saturation at full depth (every hue but the kept two)
        public const float Bend34KillStep = 0.08f; // each kill inside the bend takes 8 % more (Max Payne 2's deepening read, slowmo.md 5)
        public const float Bend34DesatMax = 0.9f; // ... never past 90 % (the world stays readable: walls and ledges keep their value contrast)
        public const int Bend34GradeSteps = 64; // the depth is written in 1/64 steps (the curve texture and the Volume rebuild on a change only)
        // the kept hues (HSV hue 0..1, the curve loops): red-orange = enemies and danger (#FF5A2B: hue 0.039), cyan = player-usable (#36E0F2: 0.517).
        // Each band: a full keep from Lo to Hi, the curve easing to the desaturated level by Fall either side (Keyframe tangents 0: an S between keys).
        public const float Bend34RedLo = 0.985f; // the red-orange band starts just under red (it loops through 0)
        public const float Bend34RedHi = 0.075f; // ... to orange-red (30 deg is 0.083: the band stops just short of plain orange)
        public const float Bend34CyanLo = 0.47f; // the cyan band: teal-cyan ...
        public const float Bend34CyanHi = 0.56f; // ... to sky-cyan
        public const float Bend34Fall = 0.06f; // the curve's fall from kept to desaturated on each side of a band (hue share)
        // the neutral cool grey the desaturated world leans to: a colour filter at full depth (never violet: violet has red over green; pale
        // violet-white is the boundary's colour, CLAUDE.md rule 8)
        public const float Bend34GreyR = 0.93f; // the cool grey's red multiplier at full depth
        public const float Bend34GreyG = 0.97f; // ... green
        public const float Bend34GreyB = 1.0f; // ... blue (the coolest channel stays whole)

        // --- the sky (the dusk sky's tint dims: the hue curve alone would keep the orange horizon saturated) ---------------------------------
        public const float Bend34SkyDesat = 0.85f; // the sky's colours (Sky material, the stone's sky fog, the Lit / Unlit fog) go 85 % to their own grey at full depth
        public const float Bend34SkyDim = 0.7f; // ... and that grey is 70 % as bright (the sky recedes; enemies and cyan pop against it)

        // --- the entry thump (world FOV only: the gun never moves; GG w33 "the gun sliding to the centre on zoom": never again) ----------------
        public const float Bend34EntryFovDeg = -4f; // NIGHT34 H1: the world FOV narrows 4 degrees on the bend's entry ...
        public const double Bend34EntryFovS = 0.15; // ... and comes back over 150 ms (an ease-out); x FOV KICK %, none with REDUCED MOTION

        // --- the hands inside a bend (Viewmodel.Draw: the sim's swap is instant there) --------------------------------------------------------
        public const double Bend34HandsSlideS = 0.08; // the new gun snaps in over 80 ms (the rail's SlideS) instead of the 0.25 s swap timeline
        public const float Bend34HandsDip = 0.05f; // ... rising from 5 cm under its hold (a snap, not a pop)

        // --- the meter (one cyan slash beside hud33's two grapple ticks under the reticle: nothing new on screen) -------------------------------
        public const double Bend34ComboS = 1.2; // a bend combo's name stands in hud33's message slot 1.2 s (the slot's own minimum hold, Hud33SlotMinHold)
        public const float Bend34MeterGap = 6f; // px between the second grapple tick and the meter's slash (HUD33 canvas units, 1080p)
    }
}
