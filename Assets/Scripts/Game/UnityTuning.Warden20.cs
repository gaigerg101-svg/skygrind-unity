// NIGHT20 U3-34.9 C4 (builder E owns this file): the WARDEN's look, warden.js's render numbers as it draws them (GG's
// design, rebuilt as it is; CLAUDE.md rule 7). Visual only: the sim never reads them.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>warden.js: the shield up (0xffb05a, a warm amber: the enemy's shield, DANGER's family) x (1.3 + 0.4 sin(age x 3)),
        /// x (1.3 + 2.5) on a block; down: 0x40302a (spent), pulled to the side and tilted 0.9 rad.</summary>
        public const uint WardenShieldHex = 0xffb05a, WardenShieldDownHex = 0x40302a;
        public const float WardenShieldBase = 1.3f, WardenShieldShimmer = 0.4f, WardenShieldBlock = 2.5f, WardenShieldDownTilt = 0.9f;
        /// <summary>warden.js: the body x (1.25, 1, 1.1); the tell pulses toward husk.windupColor at 20 rad/s (the DANGER tell);
        /// shield down: 0.25 toward white; the arm raised -1.2 rad in the tell, 1.1 in the swing.</summary>
        public const float WardenBodyX = 1.25f, WardenBodyZ = 1.1f, WardenTellPulse = 20f, WardenDownWhite = 0.25f, WardenArmTell = -1.2f, WardenArmSwing = 1.1f;
    }
}
