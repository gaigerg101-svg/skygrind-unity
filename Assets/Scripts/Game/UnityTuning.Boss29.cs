// NIGHT29 builder B: the look of the reworked bosses (Game/Boss29View.cs, the dusk rink in RinkView28). View only: the sim never reads these. Rule 8: cyan = player-usable (the lane lines), red-orange =
// danger (the lava floor, a frost tell), a dusk blue for a standing frost field, magenta never, the pale violet-white only for the boundary.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The rink's walls and roof with the reworked Glacier on (GG: the old ones read pale lavender / white): the stone tint (vertex colour) and the roof's underside (an unlit colour); both dusk-dark.</summary>
        public const uint Rink29WallHex = 0x2a2446, Rink29RoofHex = 0x15102a;
        /// <summary>The rink's fog and ambient while the reworked Glacier fight runs (dusk-dark violet; restored after): the fog colour (also the stone shader's sky-fog gradient) and the trilight ambient.</summary>
        public const uint Fog29Hex = 0x16102c, Fog29AmbSkyHex = 0x241c44, Fog29AmbEqHex = 0x1a1432, Fog29AmbGroundHex = 0x120e24;
        /// <summary>The ice with the reworked Glacier on: a deeper blue (deep side, lit side) and a dim sheen (the boundary tone at about a third), so the cyan lane lines read against it.</summary>
        public const uint Rink29IceDeepHex = 0x0c1030, Rink29IceLitHex = 0x1e2c66, Rink29IceSheenHex = 0x4a4670;
        /// <summary>The wall's pale grid (RinkWall28 _GridGain) with the reworked Glacier on: fainter than the 0.28 default.</summary>
        public const float Rink29WallGrid = 0.10f;
        /// <summary>The three skate lane lines: width m, lift above the ice m, HDR gain on the cyan (so bloom makes it glow), and the gain on the lane the player is locked to.</summary>
        public const float Lane29Width = 0.08f, Lane29Lift = 0.035f, Lane29Gain = 1.5f, Lane29ActiveGain = 2.6f;
        /// <summary>Metres per segment of a lane line along the lap (the caps curve).</summary>
        public const float Lane29Step = 1.0f;
        /// <summary>A frost trail field: its standing colour (dusk blue, additive), gain, height above the ice m and how many fields can show at once.</summary>
        public const uint Frost29Hex = 0x4a6cff;
        public const float Frost29Gain = 0.9f, Frost29Lift = 0.05f;
        public const int Frost29Max = 6;
        /// <summary>The Lantern's lava floor: the glow's colour (red-orange, danger), the glow's gain in the tell (rises to) and live, the pulse rate (rad/s) of the tell, and the light's range share / intensity.</summary>
        public const uint Lava29Hex = 0xff5a20;
        public const float Lava29TellGain = 1.1f, Lava29LiveGain = 0.8f, Lava29TellPulse = 22f, Lava29Lift = 0.04f, Lava29LightIntensity = 6f, Lava29LightRange = 30f;
    }
}
