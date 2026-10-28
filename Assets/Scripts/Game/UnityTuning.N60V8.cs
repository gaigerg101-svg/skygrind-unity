// NIGHT60 builder V (B8 the exit lamp): a157's clear shot (FeelReel.N60.Lamp.cs, `-skygrindClearShots60`), reel-only Unity numbers (no gameplay
// number). A file of its own so B7's attempt 2 (UnityTuning.N60V.cs) and this item pick in either order without a conflict.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // the most real seconds a157's clear shot kills the room's waves while waiting for its exit door to open (a room's waves follow each
        // other after their telegraph; L56WaveWaitS is one wave's wait): past it the shot is taken anyway and its line says "opened NO"
        public const float Lamp60ClearWaitS = 30f;
        // real seconds after the lamp's rise (Exit58FadeS) before the clear frame: the rise done and the room's last deaths settled
        public const float Lamp60SettleS = 0.5f;
        // B8 attempt 2 (Exit58.FaceAnchor): the gap (m) between room A's face of a door's passage and the lamp frame's back, so the frame stands in
        // front of the wall, never in it (n57-kiln's doors are 4 m deep: at the rect's centre the frame sat inside the walls, the lamp's gain 1.00-1.06x)
        public const double Exit60FaceGap = 0.02;
    }
}
