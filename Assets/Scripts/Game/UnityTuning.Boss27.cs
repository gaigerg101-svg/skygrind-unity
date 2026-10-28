// NIGHT27 builder B: THE LANTERN's and THE GLACIER's (pulse-only chase) look (Game/LanternView.cs, Game/GlacierView.cs). Visual only: the sim never reads these (its numbers are design.n27.boss1 / boss2).
// Dusk palette (rule 8): cyan is player-usable (the lit face, the wall-ride lane strips, the PULSE-ready flash), red-orange is danger only (the orbs, the lane attacks, a segment about to retract),
// pale violet-white is the boundary (the shield plates, the ice); the bodies are dark steel. Blocky boxes as every enemy is rebuilt (CLAUDE.md rule 7).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The lantern's body, its frame, the orbs, the lit cyan, the shield's pale tone, the retract warning (red-orange).</summary>
        public const uint LanternBodyHex = 0x2b2f3a, LanternFrameHex = 0x4a4f5e, LanternOrbHex = 0xff7a30, LanternLitHex = 0x36e0ff, LanternLaneHex = 0x36e0ff;
        /// <summary>Gains on the additive colours: the shield plate, the lit face, the orbs, the wall-ride lane strip, the retract warning, the core flame.</summary>
        public const float LanternShieldGain = 0.9f, LanternLitGain = 2.6f, LanternOrbGain = 2.2f, LanternLaneGain = 1.2f, LanternWarnGain = 2.4f, LanternCoreGain = 2.4f;
        /// <summary>The plates' thickness (m), their share of the face, the wall-ride lane strip (height of its centre, its height, its thickness).</summary>
        public const float LanternPlateThick = 0.12f, LanternPlateShare = 0.82f, LanternLaneY = 2.4f, LanternLaneH = 0.35f, LanternLaneThick = 0.06f;
        /// <summary>Pool sizes: orbs, and the segments (the ring has eight).</summary>
        public const int LanternOrbMax = 48, LanternSegments = 8;
        /// <summary>The core flame (a cube on the lantern's top, lit in the last stretch of its HP): its edge as a share of the body's.</summary>
        public const float LanternCoreShare = 0.42f;
        public const int LanternDeathBits = 70;
        public const float LanternDeathSpeed = 14f, LanternDeathLife = 1f, LanternDeathSize = 2f;

        /// <summary>THE GLACIER's skater: the steel body, the visor (red-orange: an enemy's eye), the blade's pale glow, the aura (boundary tone), the PULSE-ready flash (cyan).</summary>
        public const uint SkaterBodyHex = 0x3a3f4a, SkaterVisorHex = 0xff7a30, SkaterBladeHex = 0xcfc6ff, SkaterAuraHex = 0xcfc6ff, SkaterReadyHex = 0x36e0ff;
        public const float SkaterVisorGain = 2.2f, SkaterBladeGain = 1.6f, SkaterAuraGain = 0.35f, SkaterReadyGain = 2.4f, SkaterAuraGrow = 1.12f;
        /// <summary>The two blade trails: length (m) per m/s of speed, width (m), the gain, and the roller / icicle draw (the roller's glow, the icicle's spike count and height).</summary>
        public const float SkaterTrailPerSpeed = 0.9f, SkaterTrailWidth = 0.1f, SkaterTrailGain = 1.1f, GlacierRollerGain = 2.2f;
        public const int SkaterPartMax = 14, GlacierIcicleMax = 24, GlacierRollerMax = 2;
        public const float GlacierIcicleEvery = 1.6f, GlacierIcicleWidth = 0.55f;
    }
}
