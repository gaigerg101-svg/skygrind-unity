// NIGHT26 builder B: THE SLAG HULK's look (Game/HulkView.cs). Visual only: the sim never reads these (its numbers are design.n26.bosses.hulk). Blocky boxes as every
// enemy is rebuilt (CLAUDE.md rule 7); dusk palette (rule 8): the body is the enemy hull's dark steel, the weak points (head, back plate) and the stun are the pale
// violet-white boundary tone, red-orange is only the danger (the shockwave ring).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The body (the BULWARK's hull colour), the head's tint (the worm's), the shockwave ring's red-orange.</summary>
        public const uint HulkBodyHex = 0x3a3f4a, HulkHeadHex = 0x4a3f48, HulkRingHex = 0xff7a30;
        /// <summary>The weak points' glow plate (head, back): gain on the boundary colour; brighter while the hulk is stunned.</summary>
        public const float HulkWeakGain = 1.3f, HulkStunGain = 2.6f, HulkWeakThick = 0.06f;
        /// <summary>The shockwave ring's draw: boxes per ring, the ring's height and width (m), the gain, how many rings at most.</summary>
        public const int HulkRingBoxes = 36, HulkRingMax = 2;
        public const float HulkRingHeight = 1.0f, HulkRingWidth = 1.2f, HulkRingGain = 2.2f;
        /// <summary>The finisher's burst (the worm's numbers).</summary>
        public const int HulkDeathBits = 60;
        public const float HulkDeathSpeed = 14f, HulkDeathLife = 1f, HulkDeathSize = 2f;
    }
}
