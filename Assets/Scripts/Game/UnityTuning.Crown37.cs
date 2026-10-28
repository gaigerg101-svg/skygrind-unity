// NIGHT37 builder C (crown37): THE CROWN's descent's view numbers (Game/Crown37View.cs). Unity-only (render); every gameplay number is in
// design.n37.crown37.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The spire's pieces: summit / gallery / plaza stone (pale dusk stone), the orbit fragments (darker, crowned), red-orange danger (rule 8).</summary>
        public const uint Crown37StoneHex = 0x5a5470, Crown37OrbitHex = 0x3c3650, Crown37DangerHex = 0xff5a1f;
        /// <summary>The pieces' cyan top edges (m thick, gain): usable footing (rule 8); a checkpoint-free piece reads by its edge at 30 m.</summary>
        public const float Crown37EdgeThick = 0.1f, Crown37EdgeGain = 1.1f;
        /// <summary>A piece's TELL: the red-orange top glow's gain (low to high over the tell), its pulse (Hz, under 3: rule 8) and its shake (m).</summary>
        public const float Crown37TellGainLo = 0.35f, Crown37TellGainHi = 1.2f, Crown37TellHz = 2.5f, Crown37TellShake = 0.06f;
        /// <summary>A dropped piece falls away (m/s^2 of the view's fall) for design.n37.crown37.pieces.dropSeconds, then hides.</summary>
        public const float Crown37DropGravity = 22f;
        /// <summary>The ring targets (room v10) and the Crown's blade ring: segments round the ring, each segment's thickness (m), the spin (rad/s) of a target.</summary>
        public const int Crown37RingSegments = 16;
        public const float Crown37RingThick = 0.28f, Crown37HoopSpin = 1.4f;
        /// <summary>A pass's cyan flash on the ring (s, gain).</summary>
        public const float Crown37PassFlashSeconds = 0.35f, Crown37PassFlashGain = 2.4f;
        /// <summary>The ring's idle tint (steel) and its gems (red-orange: the enemy's; CYAN in the window).</summary>
        public const uint Crown37RingHex = 0xb8b2cc, Crown37GemHex = 0xff5a1f;
        public const float Crown37GemSize = 0.7f, Crown37GemGain = 1.4f, Crown37BladeLength = 1.6f;
        /// <summary>The blade ring attack's band (the red-orange ring cut): its height (m) and gain.</summary>
        public const float Crown37CutHeight = 0.25f, Crown37CutGain = 0.9f;
        /// <summary>The gem shard (m).</summary>
        public const float Crown37ShardSize = 0.5f;
        /// <summary>The point lights: one on the Crown (range m, intensity), one under each checkpoint is the level's.</summary>
        public const float Crown37LightRange = 14f, Crown37LightIntensity = 3f;
    }
}
