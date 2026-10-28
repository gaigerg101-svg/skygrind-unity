// NIGHT22 U3-36.6 (builder W): the GRIND telegraph's look. Unity-only render numbers (rule 5); the rule (which pillars, when) is
// Core/Endless21.cs + design.n21.grind.telegraphTime.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The mark on a pillar the next shape lifts: a hollow square frame at the height the pillar is going to, `Bar` m thick,
        // `Inset` of the cell's size (a gap between neighbours so the frames read as separate), standing `Lift` m above the future top.
        public const float GrindMarkBar = 0.09f, GrindMarkInset = 0.94f, GrindMarkLift = 0.06f;

        // The frames' glow: the enemy red-orange (danger) times this, pulsing between Floor and 1 at Rate Hz. The pulse is slow
        // so the whole field reads as one warning, not a flicker.
        public const float GrindMarkGlow = 1.15f, GrindMarkFloor = 0.55f, GrindMarkRate = 1.6f;
    }
}
