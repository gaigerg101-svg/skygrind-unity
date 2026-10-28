// NIGHT24 builder B: THE KILN WYRM's look (Game/WyrmView.cs). Visual only: the sim never reads these (its numbers are design.n24.wyrm).
// Blocky boxes, as every enemy is rebuilt (CLAUDE.md rule 7); the palette is the dusk one (rule 8): the body is the enemy hull's dark steel,
// the trim is the pale violet-white boundary (config barrier.boundaryColor), and red-orange is only the danger: the core while it is exposed,
// the spit, the marked spots.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The body and head: the BULWARK's hull colour (the enemy language); the exposed core: its vent core's colour.</summary>
        public const uint WyrmBodyHex = 0x3a3f4a, WyrmHeadHex = 0x4a3f48, WyrmCoreHex = 0xff7a20, WyrmSpitHex = 0xff7a30, WyrmMarkHex = 0xff4a1c;
        /// <summary>The pale-violet trim plate on each piece's back: its share of the piece's width and length, its thickness (m).</summary>
        public const float WyrmTrimWidth = 0.4f, WyrmTrimLength = 0.8f, WyrmTrimThick = 0.14f, WyrmTrimGain = 1.4f;
        /// <summary>The core: the exposed glow (x the colour, pulsing), the roar's dim glow while it is about to open.</summary>
        public const float WyrmCoreOpen = 2.6f, WyrmCorePulse = 0.6f, WyrmCorePulseRate = 14f, WyrmCoreTell = 1.1f;
        /// <summary>The spit's and shards' draw: gain on the additive colour, a shard's length (m).</summary>
        public const float WyrmShotGain = 2f, WyrmShardLength = 1.6f, WyrmShardWidth = 0.28f;
        public const int WyrmShotPool = 120, WyrmMarkerPool = 6;
        /// <summary>The marked spots: the floor crack's square (m) and the wall plate's (m), thickness, the pulse (gain, rate).</summary>
        public const float WyrmCrackSize = 5.2f, WyrmWallPlate = 4.4f, WyrmMarkThick = 0.08f, WyrmMarkGain = 2.2f, WyrmMarkPulseRate = 16f;
        /// <summary>The death burst (the same count and speed as the BULWARK's white burst).</summary>
        /// <summary>The marked spots' draw: the floor crack's lift above the floor, the wall plate's stand-off from the wall (m); the glow ramps from
        /// WyrmMarkMin to full as the event nears (WyrmMarkRamp), pulsing between WyrmMarkPulseMin and 1 (depth WyrmMarkPulseDepth).</summary>
        public const float WyrmCrackLift = 0.06f, WyrmWallStand = 0.05f, WyrmMarkMin = 0.35f, WyrmMarkRamp = 0.65f, WyrmMarkPulseMin = 0.7f, WyrmMarkPulseDepth = 0.3f;
        public const int WyrmDeathBits = 70;
        public const float WyrmDeathSpeed = 16f, WyrmDeathLife = 1f, WyrmDeathSize = 2.4f;
        /// <summary>The core's opening burst.</summary>
        public const int WyrmCoreBits = 24;
        public const float WyrmCoreSpeed = 8f, WyrmCoreLife = 0.7f, WyrmCoreSize = 1.2f;
    }
}
