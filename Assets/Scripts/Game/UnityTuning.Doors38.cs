// NIGHT38 builder M (NIGHT38.md M1, PLAN.md section 3, DECIDE38 D-2): Unity-only numbers for the doorway curtains (Doors38View.cs; CLAUDE.md
// rule 5: each with a comment). View only: a door's state (open / locked) is the sim's (Zones), its size the level's (campaign.doors[].rect,
// height); these are only how it is drawn. The colours: CYAN open = colors.usable (player-usable), RED-ORANGE locked = Arena36RedHex
// (danger), PALE VIOLET-WHITE = the boundary tone (CLAUDE.md rule 8) for the boss door.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const uint Doors38BossHex = 0xCFC6FF; // the boss door's pale violet-white (the boundary tone the boss rooms' lights already use: design.n24.wyrm / monolith lightColor)
        public const float Doors38Thick = 0.06f; // m: the curtain slab's thickness (a sheet of light, seen from both sides)
        public const float Doors38OpenAlpha = 0.16f; // an OPEN curtain's additive strength: a thin cyan veil, the room behind stays readable through it
        public const float Doors38LockedAlpha = 0.42f; // a LOCKED curtain's: a dense red-orange sheet that reads as solid (the gate field it draws is a collider)
        public const float Doors38BossAlpha = 0.3f; // the boss door's sheet (between the two: it reads as a threshold, its sign carries the meaning)
        public const float Doors38Glow = 2.4f; // HDR multiplier on every curtain colour, so the bloom carries it across a 40 m room (Arena36Glow's range)
        public const float Doors38PulseHz = 0.8f; // the sheet's slow breathing (Hz)
        public const float Doors38PulseDepth = 0.25f; // how much it breathes (a fraction of its alpha)
        public const int Doors38Lines = 7; // scanlines per curtain (bright bars scrolling up through the sheet: the holographic shimmer)
        public const float Doors38LineH = 0.07f; // m: a scanline's height
        public const float Doors38LineAlpha = 0.55f; // a scanline's additive strength (over the sheet)
        public const float Doors38LineSpeedOpen = 0.35f; // curtain heights per second an open curtain's lines climb (calm)
        public const float Doors38LineSpeedLocked = 0.9f; // a locked curtain's (agitated)
        public const float Doors38EdgeW = 0.09f; // m: the bright post down each side of the opening and the bar along its top (the frame lit)
        public const float Doors38EdgeAlpha = 1.0f; // the frame lines' additive strength (the brightest part: what the eye finds from 40 m)
        public const float Doors38LightIntensity = 3f; // the point light in the doorway (URP units, as Arena36DoorLightIntensity's range), in the curtain's colour
        public const float Doors38LightRange = 7f; // m: so the floor and the frame around the door take its colour
        public const float Doors38BeyondIntensity = 2.2f; // the light just inside the next room behind an OPEN door (Chung's cutaway: the next room lit and visible through it)
        public const float Doors38BeyondRange = 14f; // m: that light's range
        public const float Doors38BeyondDepth = 5f; // m: how far past the curtain, along the passage, into the next room it stands
        public const float Doors38SignSize = 3.2f; // the boss door's sign: TextMeshPro world font size (about 0.5 m tall capitals over a 4 m door)
        public const float Doors38SignWidth = 14f; // m: the sign's text box (wide enough for a room name on one line; no wrap)
        public const float Doors38SignLift = 0.8f; // m: the sign's baseline above the door's top
        public const float Doors38ShimmerS = 0.28f; // s: the camera's shimmer when it passes through an open curtain (a short veil of scanlines over the view)
        public const float Doors38ShimmerAlpha = 0.22f; // that veil's additive strength at its start (fades to 0)
        public const float NoRide38RibPitch = 0.6f; // m: the spacing of the dull ribs wrapped round a noride wall (a ridged surface; a ride panel is smooth with cyan trim)
        public const float NoRide38RibH = 0.14f; // m: a rib's height
        public const float NoRide38RibProud = 0.05f; // m: how far a rib stands off the wall's faces (view only: no collider)
        public const float NoRide38RibShade = 0.55f; // the ribs' colour: colors.pillarSide darkened to this (dull: no glow, never cyan)
        // NIGHT39 V (the lead's n38-r1 frame f0800, THE YARD: the open cyan curtain a thin sliver at ~40 m; f0300 at ~15 m read well): distance only,
        // so the near view is unchanged.
        public const float Doors38EdgeMinPx = 4f; // px at the screen's height: the frame (posts + top bar) never thinner than this (at camera.fovDeg 74 vertical, 1080p: 0.056 m per px at 40 m, so a 0.09 m post is 1.6 px; 4 px = 0.22 m; nearer than 16 m the 0.09 m post is already over 4 px)
        public const float Doors38EdgeMaxShare = 0.12f; // the frame never wider than this share of the opening (a 4 m door: 0.48 m), so a far door stays a doorway
        public const float Doors38FarStart = 16f; // m: nearer than this an open sheet draws as NIGHT38 drew it (where the frame's 4 px floor starts too; f0300, which read well, looks down 1.2 KESTREL's 20 m room: not measured)
        public const float Doors38FarEnd = 40f; // m: the card's "visible from 40 m"; the far boost is full from here
        public const float Doors38FarSheetBoost = 2.2f; // an OPEN sheet's alpha at Doors38FarEnd, times Doors38OpenAlpha (0.16 -> 0.35: still under the locked sheet's 0.42)
        public const float Doors38ShimmerDist = 0.35f; // m: the veil's distance in front of the camera (inside the near geometry, past the near clip)
    }
}
