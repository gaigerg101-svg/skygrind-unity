// NIGHT20 C1 (the lead): the debut card's world slow-down (SurvivalGame.Debut20.cs). Unity-only numbers: the web has no
// debut cards (U3-02's NEW item was never built there).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The world's share of real time at the slow-down's depth (0.35: the card reads, the fight still moves).</summary>
        public const double DebutSlowShare = 0.35;
        /// <summary>The slow-down's longest (s): the backlog's C1 cap, "the world slowed <= 1.5 s".</summary>
        public const double DebutSlowMax = 1.5;
        /// <summary>The ease in and out (s), so the drop and the return never snap.</summary>
        public const double DebutSlowEase = 0.15;
    }
}
