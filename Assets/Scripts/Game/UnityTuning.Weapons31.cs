// NIGHT31 builder W: Unity-only view numbers of the W rows (CLAUDE.md rule 5: render / layout numbers live in UnityTuning, each with a comment).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        public const float Parry31StreakSeconds = 0.15f; // s: a parried boss projectile's streak back to its boss (the lead's round-4 ask: about 0.15 s)
        public const float Parry31StreakTail = 0.35f; // share of the path: the streak's tail trails its head by this much of the player-to-boss line
        public const float Parry31StreakWidth = 0.08f; // m: the streak's width (a LineRenderer facing the view)
        public const float Parry31StreakBrightness = 2.5f; // x colors.usable on the additive neon: bright enough to read against the dusk sky, below the bloom's white-out
        public const float Pickup31Size = 0.6f; // m: the spinning box of a room's pickup31 (i1a's gauntlet), the plans' weaponPickup prop size (k1-P2's [0.6, 0.6, 0.6])
    }
}
