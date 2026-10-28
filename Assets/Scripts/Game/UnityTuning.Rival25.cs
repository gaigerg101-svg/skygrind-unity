// NIGHT25 builder W: RIVAL PRIME's look (Game/RivalView.cs). Visual only: the sim never reads these (its numbers are design.n25.bosses.rival).
// Blocky boxes, as every enemy is rebuilt (CLAUDE.md rule 7); rule 8: MAGENTA IS THE RIVAL'S ONLY: its visor and trim are the only magenta in the game (0xff2bd6, the
// web's rage colour); the body is the enemy hull's dark steel; its orbs are danger (red-orange). Sizes are the player's capsule's (the rival is a mirror of the player).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const uint RivalBodyHex = 0x3a3f4a, RivalMagentaHex = 0xff2bd6, RivalOrbHex = 0xff7a30;
        /// <summary>Box sizes (m): torso (w, h, d), head, leg (w, h, d), the gun arm (w, h, length); the visor's thickness; the magenta gain on the additive trim; the orbs' size (m) and gain.</summary>
        public static readonly float[] RivalTorso = { 0.62f, 0.7f, 0.34f }, RivalHead = { 0.32f, 0.3f, 0.3f }, RivalLeg = { 0.22f, 0.7f, 0.26f }, RivalArm = { 0.14f, 0.14f, 0.6f };
        public const float RivalVisorThick = 0.06f, RivalMagentaGain = 2.2f, RivalOrbSize = 0.5f, RivalOrbGain = 2f;
        public const int RivalOrbPool = 24;
    }
}
