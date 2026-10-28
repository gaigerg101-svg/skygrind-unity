// NIGHT59 builder G (G1 kill55's marks after the flash): the marks' own draw (SKYGRIND/Kill59Mark), Unity-only render numbers (no gameplay number:
// the marks are pictures, no Core state; the web has no hit marks, so no config number exists for them).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // the depth pull (m): each mark vertex moves this far toward the camera along its own view ray (same pixel, nearer depth), so the decal
        // beats the husk's own parts in front of the torso (n59-g-probe.py: the mark's centre read the same V on and off = hidden). Larger than the
        // pill's parts stand off its profile; anything nearer than this (debris, a wall, another enemy) still covers the mark
        public const float Kill59MarkPull = 0.3f;
        // the pull stops this far (m) past the camera (a husk at arm's length: the mark never crosses the near plane)
        public const float Kill59MarkNearest = 0.35f;
        // the facing cut: a mark whose shell normal dotted with the view direction is under this is not drawn (the far side never shows through)
        public const float Kill59MarkFacing = 0.05f;
    }
}
