// NIGHT14: the two Unity-side names the sim composition reads, for the harness only (Unity compiles the real ones:
// Flow in SurvivalGame.cs, UnityTuning.BoxDepth in UnityTuning.cs). Keep them equal to those.
namespace Skygrind.Game
{
    public enum Flow { Title, Playing, Paused, Down, Results }

    public static partial class UnityTuning
    {
        public const float BoxDepth = 0.75f;
    }
}
