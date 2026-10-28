// NIGHT43 builder F1 round 2 (design.n43.look43, a view row: no sim bit; G8 GG: "stronger lighting, effects, environmental composition ... neon
// strips attached to simple geometry are not enough"): the render numbers of the look kit (Game/Look43View.cs, Look43Math.cs), each with a comment.
// The rules they serve: docs/DESIGN-RULES.md A-5 (hazards light the geometry from below), A-6 (a landmark forward, the exit lit, the next door lit
// when the room clears), V-1 (the saturation budget belongs to gameplay), V-3 (ridable walls look ridable, others never), V-5 (lights on fixtures).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- (1) the doors: a lit frame (the fixture) in the opening; the current room's exit, then the next room's when this one clears -----------
        public const float Look43DoorPost = 0.14f; // m a frame post / lintel is thick (inside the opening, in front of NIGHT29's stone frame)
        public const float Look43DoorInset = 0.04f; // m the frame stands inside the opening's edge (no z-fight with the stone frame)
        public const float Look43DoorLocked = 0.22f; // the exit frame's brightness while the room still fights (it says "this way", not "open")
        public const float Look43DoorOpen = 1.0f; // the exit frame's brightness once the room clears (pulsing)
        public const float Look43DoorNext = 0.7f; // the next room's exit frame, lit when this room clears (seen through the open door)
        public const float Look43DoorGain = 2.6f; // HDR gain on the cyan (the doorway is the brightest cool thing in the room)
        public const float Look43DoorPulseHz = 0.9f; // the open frame's breath (under 3 a second: the flash rule)
        public const float Look43DoorPulseDepth = 0.18f; // +- share
        public const double Look43NextLightIntensity = 2.2, Look43NextLightRange = 9; // the next door's light (one URP point light, moved; x LightScale)
        public const float Look43NextLightY = 2.4f; // m above the door's floor

        // --- (2) lava lights the geometry from below ---------------------------------------------------------------------------------------------
        public const float Look43LavaGlowH = 2.6f; // m the underglow climbs a real wall above the surface (the room's wall on a pool's edge, a ride panel standing in it)
        public const float Look43LavaFloorBand = 1.4f; // m the glow spills across the floor round an open pool edge (the pit wall below it glows to the floor; nothing in mid-air)
        public const float Look43LavaGlowGain = 1.5f; // HDR gain of the underglow at its foot
        public const float Look43LavaPulseHz = 0.35f; // the slow pulse (the surface's own breath is 0.8 Hz: this is the heat under it)
        public const float Look43LavaPulseDepth = 0.25f; // +- share
        public const double Look43LavaLightIntensity = 3.0, Look43LavaLightY = 0.9; // the nearest pool's light (one URP point light over the room's lava)
        public const double Look43LavaLightRangeMin = 6, Look43LavaLightRangeShare = 0.75; // its reach: the pool's long side x share, at least the min
        public const uint Look43LavaHex = 0xff5a1c; // the underglow's red-orange (danger: rule 8; Lava30's surface runs crust red to gold)

        // --- (3) ride walls: one cyan edge strip + a rhythm along the ride (V-3); never on a wall that cannot be ridden -------------------------------
        public const float Look43RideTopDrop = 0.12f; // m below the panel's top edge the strip sits
        public const float Look43RideStripH = 0.12f; // m tall
        public const float Look43RideOut = 0.03f; // m off the face
        public const float Look43RideBandY = 1.35f; // m above the panel's floor the rhythm runs (the ride's eye band)
        public const float Look43RideDashH = 0.18f; // m tall
        public const float Look43RideDash = 0.8f; // m long
        public const float Look43RideGap = 1.2f; // m between dashes (a beat every 2 m: at a 12 m/s ride, 6 a second)
        public const float Look43RideGain = 1.8f; // HDR gain on the cyan

        // --- (4) one landmark per arena, visible from its entrance: two tall pylons flanking the exit in the room's cool hue --------------------------
        public const float Look43MarkSide = 1.1f; // m out from the exit door's edge
        public const float Look43MarkW = 0.35f; // m square
        public const float Look43MarkIn = 0.35f; // m in from the far wall
        public const float Look43MarkTopDrop = 0.4f; // m under the room's roof / wall height
        public const float Look43MarkGain = 2.0f; // HDR gain on the cool hue (cool, low: decoration glows only in the environment's hue, ui-art-web C10)
        public const float Look43MarkMinRoom = 12f; // m: a room whose short side is under this is a corridor (no landmark)

        // --- (5) the saturation budget (V-1): the stone's vertex colours drained toward their grey, a touch darker; enemies / lava / pickups keep theirs --
        public const float Look43StoneDesat = 0.35f; // share toward the colour's luma
        public const float Look43StoneValue = 0.92f; // x value
    }
}
