// NIGHT26 builder B: THE GLACIER's look and the gap meter's layout (Game/GlacierView.cs, Game/UiGap26.cs). Visual only: the sim never reads these (its numbers are
// design.n26.bosses.glacier). Dusk palette (rule 8): the hull is the enemy hull's dark steel, ice is the pale violet-white boundary tone, boost strips are cyan (player-usable),
// rails and walls are neutral stone, red-orange only on the danger (the lane attacks, the engine's tell).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const uint GlacierHullHex = 0x3a3f4a, GlacierEngineHex = 0x4a3f48, GlacierAttackHex = 0xff7a30, GlacierBoostHex = 0x36e0ff, GlacierRailHex = 0x6a7080;
        /// <summary>Gains on the additive colours (ice, boost strip, attack bars, the telling bar's pulse).</summary>
        public const float GlacierIceGain = 0.55f, GlacierBoostGain = 1.8f, GlacierAttackGain = 2.2f, GlacierTellMin = 0.35f, GlacierTellRamp = 0.65f, GlacierFloorLift = 0.03f, GlacierStripThick = 0.04f;
        public const int GlacierTileMax = 12, GlacierBarMax = 3, GlacierSpikeMax = 40, GlacierStripMax = 24;
        /// <summary>The course light: its height over the player, range, intensity (the room's light follows the player along the straight).</summary>
        public const float GlacierLightUp = 7f;
        /// <summary>The gap meter: width (px), the bar's height, the foot (px over the screen's bottom: over the boss bar), the red-orange soft-fail zone (m before failAt).</summary>
        public const float Gap26W = 560f, Gap26BarH = 14f, Gap26Bottom = 150f, Gap26SoftZone = 20f;
        public const int GlacierDeathBits = 60;
        public const float GlacierDeathSpeed = 14f, GlacierDeathLife = 1f, GlacierDeathSize = 2f;
    }
}
