// NIGHT40 builder B2 (brawl40): THE ICEBREAKER and its arena as Brawl40View draws them. Visual only: the sim never reads these (its numbers are
// design.n40.brawl40). Dusk palette (rule 8): the ice a cold dark slate, the walls and pillars dusk stone; cyan is player-usable (the ride
// trims on the rim walls and pillars, the stuck fist and the reeling body: "hit it now"); red-orange is danger only (the wind-up glows, the
// slam's ground ring and shockwave, the rush's ground line, the locked slab, the rim strips of the body); yellow is the parry (the block).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The arena: the ice, the walls, the pillars (opaque dusk colours); the cyan ride trims (height m, gain, stand-off m); the slab's gain.</summary>
        public const uint Brawl40IceHex = 0x2a3346, Brawl40WallHex = 0x2b2536, Brawl40PillarHex = 0x241f2e;
        public const uint Brawl40CyanHex = 0x36e0ff, Brawl40DangerHex = 0xff3a14, Brawl40YellowHex = 0xffd23a, Brawl40PaleHex = 0xd8d0ff;
        public const float Brawl40TrimHeight = 0.18f, Brawl40TrimGain = 1.6f, Brawl40TrimStandOff = 0.04f, Brawl40SlabGain = 1.4f;
        /// <summary>The body: dark torso / limbs (opaque silhouette), the red-orange rim strips' gain (an enemy), the knuckles' resting pale gain.</summary>
        public const uint Brawl40BodyHex = 0x2a2438, Brawl40BodyDarkHex = 0x1a1622;
        public const float Brawl40RimGain = 1.3f, Brawl40KnuckleGain = 0.45f;
        /// <summary>The tells: a wind-up glow from min to max gain over the wind-up (red-orange; the block yellow); the stuck fist's and the window's
        /// cyan gain; the hit flash (s).</summary>
        public const float Brawl40TellMin = 0.4f, Brawl40TellMax = 3.0f, Brawl40CyanGain = 2.4f, Brawl40HitFlash = 0.08f;
        /// <summary>The slam: the ground decal's ring (segments, width m, gain) during the wind-up at the ring's full reach and the moving shockwave
        /// (height m, gain); the rush's ground line (width m, gain, length m); the block's size (m, its sim radius x2) and gains (yellow out, white back).</summary>
        public const int Brawl40RingSegments = 40;
        public const float Brawl40RingWidth = 0.25f, Brawl40DecalGain = 0.9f, Brawl40WaveHeight = 0.5f, Brawl40WaveGain = 2.2f;
        public const float Brawl40LineWidth = 1.2f, Brawl40LineGain = 1.1f, Brawl40LineLength = 24f;
        public const float Brawl40BlockGain = 2.4f, Brawl40BlockBackGain = 3f;
        /// <summary>Phase 2's floes: the open water's colour (near black, under the ice's slate), the plates' lift off the ice (m), the gap between
        /// plates (m, so the grid reads as floes), the RED crack's top gain.</summary>
        public const uint Brawl40WaterHex = 0x0b0f1c;
        public const float Brawl40FloeLift = 0.025f, Brawl40FloeGap = 0.3f, Brawl40FloeGain = 1.8f;
        /// <summary>The walk cycle: metres walked per stride, the legs' swing (deg).</summary>
        public const float Brawl40Stride = 2.6f, Brawl40LegSwing = 28f;
    }
}
