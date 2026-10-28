// NIGHT31 builder B: THE STAG's look and the KILN WYRM's crumble (Game/Boss31View.cs). Visual only: the sim never reads these (its numbers are design.n31.stag31 / wyrm31).
// Dusk palette (rule 8): cyan is player-usable (the antler crystals a charged shot breaks, the open back a punch lands on), red-orange is danger only (the charge line, the shards, the
// melted banks, the crumble cracks), the Stag's body is dark so the cyan and the red-orange read on it and it stands out against the pale ice (the lead's n31-r2 frame: pale grey washed out).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The Stag's body, neck and head (dark slate) and its legs (darker), as an opaque material's _BaseColor.</summary>
        public const uint Stag31BodyHex = 0x2a2f3e, Stag31LegHex = 0x181b24;
        /// <summary>The cyan of the antler crystals and of the open back (gain: crystals at rest, the back while open, pulsing at Stag31PulseRate Hz between 0.6 and 1 of the gain).</summary>
        public const uint Stag31CyanHex = 0x36e0ff;
        public const float Stag31CrystalGain = 2.6f, Stag31BackGain = 2.0f, Stag31PulseRate = 4f;
        /// <summary>The charge line on the ice (red-orange): width (m), lift off the ice (m), gain from TellMin to TellMax across the tell.</summary>
        public const uint Stag31DangerHex = 0xff5a1f;
        public const float Stag31LineWidth = 3.0f, Stag31LineLift = 0.05f, Stag31LineTellMin = 0.5f, Stag31LineTellMax = 2.4f;
        /// <summary>The stomp ring (m tall, seconds it shows) and the shards (m across, gain).</summary>
        public const float Stag31StompRingHeight = 0.08f, Stag31StompLife = 0.35f, Stag31ShardSize = 0.35f, Stag31ShardGain = 2.0f;
        /// <summary>The melted banks: a red-orange film over each bank (gain, lift m, the lap cut into this many strips).</summary>
        public const float Stag31WaterGain = 0.9f, Stag31WaterLift = 0.06f;
        public const int Stag31WaterStrips = 96, Stag31ShardPool = 24;
        /// <summary>The silhouette: the legs take this fraction of the shoulder height, the head is this many times longer than wide, the neck this fraction of the half width thick,
        /// the legs this thick (m).</summary>
        public const float Stag31LegFrac = 0.45f, Stag31HeadLength = 1.5f, Stag31NeckThick = 0.9f, Stag31LegThick = 0.55f;
        /// <summary>The Wyrm's crumbling platforms: their stone (as the room's dusk stone), the cracks' red-orange gain across the tell, pulse rate (Hz) and how far the crack shell
        /// stands out (m).</summary>
        public const uint Wyrm31StoneHex = 0x4a4560;
        /// <summary>THE LANTERN's platforms with design.n31.lantern31 on (L's r2 look: they read as low purple walls, the panels' colour): pale stone tops, darker sides, a cyan edge band
        /// (gain at rest / while the core is open; height m, lift above the top m, stand-off from the face m); the band turns red-orange through the sink.</summary>
        /// <summary>THE LOOM rebuilt: the deck break film's lift over the deck (m) and its lava gain once open; the shake shell's size (x the joint) and its view-only jitter (m).</summary>
        public const float Loom31BreakLift = 0.05f, Loom31LavaGain = 1.1f, Loom31ShakeScale = 1.6f, Loom31ShakeJitter = 0.12f;
        public const uint Lantern31TopHex = 0xd6d0e6, Lantern31SideHex = 0x5a5470;
        public const float Lantern31EdgeGain = 0.9f, Lantern31EdgeOpenGain = 2.0f, Lantern31EdgeHeight = 0.12f, Lantern31EdgeLift = 0.01f, Lantern31EdgeStandOff = 0.03f;
        public const float Wyrm31CrackMin = 0.4f, Wyrm31CrackMax = 2.2f, Wyrm31CrackRate = 8f, Wyrm31CrackGrow = 0.06f;
    }
}
