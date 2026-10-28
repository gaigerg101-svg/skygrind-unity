// NIGHT42 builder K1 (arena42; G12): Arena42View's Unity-only view numbers (CLAUDE.md rule 5: each with a comment). The elements' gameplay numbers are
// design.n42.arena42 (Core/Arena42.cs); their places come from the level JSON (`grinders42`, `cover42`).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- THE GRINDER ----------------------------------------------------------------------------------------------------------------------------
        public const uint Arena42GrinderBladeHex = 0xff5a1f; // the blades: danger red-orange (rule 8: red-orange is enemies and danger only; a grinder is danger)
        public const uint Arena42GrinderFloorHex = 0x2a0c08; // the pit floor under the blades: a dark burnt red, so the blades read against it
        public const uint Arena42GrinderRimHex = 0xff8a3a; // the rim strips round the rect: a lighter red-orange, the edge you knock them over
        public const float Arena42BladeGain = 1.5f; // the blades' colour scale (over 1: bloom carries a glow; they light nothing)
        public const float Arena42BladeSpacing = 1.6f; // m between blade hubs along the rect's long side (a 4 m grinder: 3 hubs)
        public const float Arena42BladeThick = 0.14f; // the blade bar's height, m
        public const float Arena42BladeWide = 0.22f; // the blade bar's width, m
        public const float Arena42BladeLift = 0.25f; // the blades' height over the grinder floor, m (low enough to read as the floor's teeth)
        public const float Arena42BladeTurnsPerSecond = 2.2f; // the blades' spin (fast enough to read as moving on a still frame's blur, under 3 Hz: no strobe)
        public const float Arena42RimWidth = 0.18f; // the rim strip's width, m
        public const float Arena42RimLift = 0.04f; // the rim strip over the floor, m (no z-fight)
        public const int Arena42FlarePool = 6; // ground flares at once (a crowd knocked in together)
        public const float Arena42FlareSeconds = 0.55f; // a ground flare's life
        public const float Arena42FlareHeight = 3.2f; // the flare column's height, m (seen over a 4 m pit's rim)
        public const float Arena42FlareWidth = 0.9f; // the flare column's width, m
        public const uint Arena42FlareHex = 0xffb070; // the flare: red-orange toward white at its start (a body ground: danger's colour, hot)

        // --- BREAKABLE COVER ------------------------------------------------------------------------------------------------------------------------
        public const uint Arena42CoverHex = 0x3b3a4a; // the cover's body: a dark slate (not a wall's tan: it reads as a separate, breakable thing)
        public const uint Arena42CoverEdgeHex = 0x29e0ff; // the cover's top edge band: cyan (rule 8: player-usable; you can shoot it apart)
        public const float Arena42CoverEdgeGain = 1.2f; // the band's colour scale
        public const float Arena42CoverEdgeHeight = 0.08f; // the band's height over the box top, m
        public const float Arena42CoverHitFlashSeconds = 0.12f; // a hit's white flash on the body
        public const int Arena42ShardCount = 10; // shards per shatter
        public const float Arena42ShardSeconds = 0.9f; // a shard's life
        public const float Arena42ShardSize = 0.35f; // a shard's edge, m
        public const float Arena42ShardSpeed = 7f; // a shard's outward speed, m/s (the view's: the sim's burstPush moves the bodies)
        public const float Arena42ShardGravity = 14f; // the shards' fall, m/s^2 (a quick arc that lands round the box)
    }
}
