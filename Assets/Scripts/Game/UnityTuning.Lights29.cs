// NIGHT29 items 2 and 5 (builder V): the light sockets' and door frames' view numbers (Game/LightKit29.cs, Game/DoorFrames29.cs).
// CLAUDE.md rule 5: each with a comment. Positions, sizes, colours, ranges and intensities come from the map JSON's
// `campaign.lights` rows (builder R's plans); the wall depth limit is design.n29.lights.maxWallDepth. Only the fallbacks and the
// plumbing live here. View only: no collider, no sim path.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float CeilingFixtureMaxDepth = 0.05f; // m: a ceiling fixture hangs at most this far below the ceiling (R4b: thickness 0 to 0.05, above jump apex)
        public const float FixtureDefaultW = 2f; // m: a fixture row without a size is this long ...
        public const float FixtureDefaultH = 0.12f; // ... and this tall (a trim strip)
        public const float FixtureDefaultD = 0.1f; // ... and this deep (under maxWallDepth)
        public const float PoolBrightness = 0.55f; // x the strip brightness: a pool's floor panel is dimmer than a trim (it is a glow, not a fixture)
        public const float PoolLift = 0.02f; // m: a pool's panel floats this far over the floor (no z-fight)
        public const uint FixtureGreenHex = 0x3CFF9A; // green accent (cool, player-side: never red-orange or magenta)
        public const uint FixtureAmberHex = 0xFFB347; // amber accent (warm, not red-orange)
        public const int LightKitRealLights = 3; // real point lights the kit lends to the fixtures nearest the player (roof27 allows 4 in all; the room kit owns two of its own)
        public const float LightKitRepick = 0.25f; // s: the nearest fixtures are re-picked this often; no light is created per frame
        public const float DoorJambDepth = 0.12f; // m: a door frame's jamb stands this far into the opening (under design.n29.lights.maxWallDepth, so a player's shoulder never meets it)
        public const float DoorHeaderH = 0.35f; // m: the lintel's thickness under the wall top
        public const float DoorNeonW = 0.05f; // m: the neon line on a jamb's inner face and under the lintel
        public const float DoorSillW = 0.12f; // m: the two threshold lines on the floor across the opening
        public const float DoorSillLift = 0.012f; // m: they float this far over the floor
        public const float DoorFallbackH = 6f; // m: a door's frame height when no wall box touches the opening
        public const float DoorWallTouch = 0.05f; // m: a wall box whose end face is within this of the opening's edge counts as its wall
    }
}
