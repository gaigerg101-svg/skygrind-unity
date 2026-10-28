// NIGHT37 builder S (stag37): THE CANAL and THE STAG as Stag37View draws them. Visual only: the sim never reads these (its numbers are
// design.n37.stag37). Dusk palette (rule 8): the ice a cold dark slate so the lanes' pale lines and the cyan trims read on it; the walls dusk
// stone; cyan is player-usable (the ride trims on the bank and lane walls, the slipstream streak, the CYAN skid window); red-orange is danger
// only (the locked slab, a floe about to drop, the shard lane, the lane-cut barrier); yellow is the parry (the ice chunk, the antler charge).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The canal: the ice (opaque), the bank and lane walls, the island, the tunnel's roof (dusk stone); the floes' ice (a shade lighter so
        /// straight B reads as floes); the lane lines (pale, width m, gain) and the ride trims (cyan, height m, gain, stand-off m).</summary>
        public const uint Canal37IceHex = 0x2a3346, Canal37FloeHex = 0x34405a, Canal37WallHex = 0x2b2536, Canal37IslandHex = 0x1e1a26, Canal37TunnelHex = 0x16131d;
        public const uint Canal37LaneHex = 0xd8d0ff, Canal37TrimHex = 0x36e0ff, Canal37DangerHex = 0xff3a14;
        public const float Canal37LaneWidth = 0.12f, Canal37LaneGain = 0.35f, Canal37LaneLift = 0.015f, Canal37LaneDash = 3f, Canal37LaneGap = 2f;
        public const float Canal37TrimHeight = 0.18f, Canal37TrimGain = 1.6f, Canal37TrimStandOff = 0.04f, Canal37TrimTop = 0.5f;
        /// <summary>The locked slab's gain; a floe's RED telegraph (gain min..max over the tell) and the hole's dark water.</summary>
        public const float Canal37SlabGain = 1.4f, Canal37FloeTellMin = 0.3f, Canal37FloeTellMax = 1.6f;
        /// <summary>The canal's lights: point lights round the loop (count per straight, height m, range m, intensity, pale boundary colour).</summary>
        public const int Canal37LightsPerStraight = 3;
        public const float Canal37LightY = 9f, Canal37LightRange = 38f, Canal37LightIntensity = 5f;
        public const uint Canal37LightHex = 0xd8d0ff;

        /// <summary>THE STAG's body: the dark torso / limbs (opaque: a silhouette with no light), the antlers' and hooves' resting pale ice (gain),
        /// the red-orange rim (an enemy), the hind legs' bands (pale; CYAN gain while you can punch them), the leg segment length (m), the stride
        /// (m of run per gait cycle), the roll when it falls (deg), the window glow's radius (m).</summary>
        public const uint Stag37BodyHex = 0x2a2438, Stag37BodyDarkHex = 0x1a1622, Stag37AntlerHex = 0xd8e4ff;
        public const float Stag37AntlerGain = 0.55f, Stag37HoofGain = 0.35f, Stag37RimGain = 1.3f, Stag37BandGain = 0.6f, Stag37BandCyanGain = 2.4f;
        public const float Stag37LegLength = 0.85f, Stag37Stride = 3.2f, Stag37FallRoll = 75f, Stag37GlowRadius = 1.8f;
        /// <summary>The fight's fx: lift off the ice (m); the shard lane strip's gain; the shards (count, size m, gain); the barrier's gain; the chunk's
        /// gain (yellow) and on its way back (white); the slipstream streak (width m, gain); the pacer's gain.</summary>
        public const float Stag37FxLift = 0.03f, Stag37StripGain = 0.9f, Stag37ShardSize = 0.35f, Stag37ShardGain = 2.2f, Stag37BarrierGain = 1.6f;
        public const float Stag37ChunkGain = 2.4f, Stag37ChunkBackGain = 3f, Stag37StreakWidth = 0.6f, Stag37StreakGain = 1.4f, Stag37PacerGain = 1.8f;
        public const int Stag37Shards = 14;
    }
}
