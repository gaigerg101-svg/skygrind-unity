// NIGHT41 builder E6 (models41, a view row: design.n41.models41.enabled, N41View.RowOn; NIGHT41.md card E6; INVENTORY A7, A8): Unity-only render numbers
// of the HUSK's own silhouette feature and the parts' death break-apart. No gameplay number lives here (no part has a collider; the hit shapes are the sim's).
// A part uses NIGHT40's layout (UnityTuning.Models40.cs: prim, x, y x the height, z, scale x y z, euler x y z, role, group, tell pitch, tell grow).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // husk (height 1.8; A7: the blind reader's weakest pair was HUSK / SPITTER, "both small capsule-like blobs", and "a hunched yoke is not enough"):
        // ONE long arm dragging to the floor on its right side, and a claw at its end. The husk's outline becomes lopsided (the spitter's is symmetric:
        // a dome and two pods), its attack is the claw (DANGER red-orange: an ember at rest, lit in the wind-up), and the arm lifts in the wind-up
        // (tell pitch: the swing reads before it lands). Group 1: a husk only (the TANK keeps its slab). Renderers: husk 4 -> 6 while models41 is on.
        public static readonly float[] Models41Husk =
        {
            // the arm: a thin cylinder from the shoulder (~1.3 m) to the floor, leaning forward and out
            2, 0.40f, 0.40f, 0.12f, 0.15f, 1.25f, 0.15f, 18, 0, 8, 0, 1, -55, 0,
            // the claw: a flat wedge at the arm's end, just off the floor, ahead of the feet
            0, 0.46f, 0.07f, 0.34f, 0.26f, 0.12f, 0.36f, 18, 0, 8, 2, 1, -55, 0.2f,
        };
    }
}
