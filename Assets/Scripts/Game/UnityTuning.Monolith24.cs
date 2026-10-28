// NIGHT24 leftover 8 (builder B): MONOLITH's look (Game/MonolithView.cs): boss.js's render numbers as it draws them, rebuilt as blocky boxes (CLAUDE.md
// rule 7). Visual only: the sim never reads them (its numbers are config.boss).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>boss.js: the stone 0x6b625c (Lambert, a red-orange rim in the web), the core 0xff7a20 (red-orange: the weak point is the enemy's own glow),
        /// the shock rings 0xff7a30 and the laser 0xff3a10 (danger), the orbs 0xff4a1c.</summary>
        public const uint MonolithStoneHex = 0x6b625c, MonolithCoreHex = 0xff7a20, MonolithRingHex = 0xff7a30, MonolithLaserHex = 0xff3a10, MonolithOrbHex = 0xff4a1c;
        /// <summary>The core's glow: shut (x), kneeling and open (x + pulse), the pulse rate (rad/s).</summary>
        public const float MonolithCoreShut = 1.3f, MonolithCoreOpen = 2.8f, MonolithCorePulse = 0.7f, MonolithCorePulseRate = 12f;
        /// <summary>The rings' gain, the laser's thickness (m) while it charges (dim) and sweeps, its gains, the open cylinder's sides, the pool sizes.</summary>
        public const float MonolithRingGain = 2f, MonolithLaserThick = 0.35f, MonolithLaserCharge = 0.4f, MonolithLaserChargeGain = 1.2f, MonolithLaserGain = 4f, MonolithOrbGain = 2.5f;
        public const int MonolithRingSides = 56, MonolithRingPool = 6, MonolithOrbPool = 40, MonolithSurgePool = 6;
        /// <summary>A surge's eruption pillar: its width (m, boss.js 2.4) and the share of surgeHeight it rises to at the peak.</summary>
        public const float MonolithSurgeWidth = 2.4f;
        /// <summary>The death burst and the arms' size (boss.js: 1.4 x 6 x 1.4 at +-3.4, 7.5).</summary>
        public const int MonolithDeathBits = 60;
        public const float MonolithDeathSpeed = 16f, MonolithDeathLife = 1f, MonolithDeathSize = 2.2f;
        public const float MonolithArmW = 1.4f, MonolithArmH = 6f, MonolithArmX = 3.4f, MonolithArmY = 7.5f;
    }
}
