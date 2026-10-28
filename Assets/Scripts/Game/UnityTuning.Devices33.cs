// NIGHT33 builder R: the arena devices' view numbers (Game/Devices33View.cs). Unity-only (render); every gameplay number is in design.n33.devices33.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The devices' housing stone (dark dusk steel) and the grate slats' colour.</summary>
        public const uint Devices33StoneHex = 0x2e2a3c, Devices33GrateHex = 0x14121b;
        /// <summary>Red-orange danger (rule 8): the vent's tell and burst.</summary>
        public const uint Devices33DangerHex = 0xff5a1f;
        /// <summary>The fan's column: the cyan rim's gain (player-usable), the rising motes per fan, their size (m), their rise speed (m/s) and gain.</summary>
        public const float Devices33FanRimGain = 1.2f, Devices33MoteSize = 0.12f, Devices33MoteSpeed = 3.5f, Devices33MoteGain = 0.7f;
        public const int Devices33Motes = 14;
        /// <summary>The grate's housing lip (m above the floor, m wide) and the slat count across a grate.</summary>
        public const float Devices33LipHeight = 0.06f, Devices33LipWidth = 0.18f;
        public const int Devices33Slats = 5;
        /// <summary>The vent's tell: the grate glow's gain at the tell's start and end (red-orange, flat on the grate), its pulse (Hz, under 3 a second: rule 8
        /// flicker on danger only), and the burst column's gain and width share. Round 3 (the lead's Player frame n33-r6: the burst column at gain 1.3 in gold read
        /// as an opaque saturated YELLOW wall): the column is red-orange only and translucent (the additive gain is its opacity: <= 0.35).</summary>
        public const float Devices33TellGainLo = 0.2f, Devices33TellGainHi = 0.9f, Devices33TellHz = 2.5f, Devices33BurstGain = 0.35f, Devices33BurstShare = 0.85f;
        /// <summary>The tell's SHIMMER over the grate: thin red-orange strips rising (count, width m, height m, rise m/s, gain <= 0.35: translucent) to burstHeight.</summary>
        public const int Devices33ShimmerStrips = 8;
        public const float Devices33ShimmerWidth = 0.05f, Devices33ShimmerHeight = 0.6f, Devices33ShimmerSpeed = 2.2f, Devices33ShimmerGain = 0.3f;
        /// <summary>The platform's cyan edge strips (m thick, gain) and its under-glow light (range m, intensity before LightScale).</summary>
        public const float Devices33EdgeThick = 0.08f, Devices33EdgeGain = 1.0f, Devices33VentLightRange = 6f, Devices33VentLightIntensity = 2.2f;
    }
}
