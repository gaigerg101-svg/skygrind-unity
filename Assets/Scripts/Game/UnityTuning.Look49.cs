// NIGHT49 shared commit (Tools/night/n49-shared.cjs): THE ONE PALETTE. These role tokens are the only colour table tonight: charge49, look49,
// lights49 and fx49 read them and hold only their own sizes and timings (UnityTuning.H49.cs, V49, L49, Fx49). View only: nothing here touches the sim.
// CLAUDE.md rule 8 roles: cyan = player-usable; red-orange = enemies and danger only; yellow = punch it / parryable; magenta = RIVAL only;
// pale violet-white = the boundary. The first four values are the web's own colours (config colors.usable, colors.enemy) or the constants in use
// (Rival25 RivalMagentaHex, roof27 boundaryHex), so the screen does not move at the shared commit. The parry yellow is a Proposal (UNIFY.md's
// role bands; hue 44 deg, 31 deg from the danger's 12 deg), not measured: V / X may propose a change in their report.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>Player-usable cyan (config colors.usable 0x19D3E6; hue about 186 deg).</summary>
        public const uint Look49UsableHex = 0x19D3E6;
        /// <summary>Enemies and danger, red-orange (config colors.enemy 0xFF4A1C; hue about 12 deg). Nothing else takes this hue at high saturation.</summary>
        public const uint Look49DangerHex = 0xFF4A1C;
        /// <summary>Parryable / punch it: yellow (a Proposal, DECIDE49 D-3; hue about 44 deg, at least 20 deg from the danger and the usable).</summary>
        public const uint Look49ParryHex = 0xFFC93A;
        /// <summary>RIVAL only (Rival25 RivalMagentaHex 0xff2bd6; hue about 312 deg). The gauntlet's pink (Fx47BodyHex, Feel43 pink) stays apart from it.</summary>
        public const uint Look49RivalHex = 0xFF2BD6;
        /// <summary>The boundary: pale violet-white (roof27 boundaryHex 0xd8d0ff; hue about 250 deg).</summary>
        public const uint Look49BoundaryHex = 0xD8D0FF;
        /// <summary>A level accent must sit at least this many degrees of hue from every role token above (rule 8; V and L read it as the guard).</summary>
        public const float Look49AccentMinHueGapDeg = 20f;
    }
}
