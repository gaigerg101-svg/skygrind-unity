// NIGHT21 U3-35.3 (builder M): THE ARCLINE's look (Game/Arcline21View.cs; the rule is Core/Arcline21.cs). UNCOMPILED on the night it was
// written (the Editor was stuck on a dialog). Render-only numbers with no web counterpart; the web's own (ropeColor, ropeWidth,
// nodeColor, nodeRadius, nodeGlow) come from config.json `arcline`.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The rope's width is the web's ropeWidth (a three.js line width in m) x this: a Unity LineRenderer's width is the full width.
        public const float Arcline21RopeWidthMul = 2f;
        // The node glows' pulse while the gadget is owned (Hz), their floor (x the colour), and the share they brighten to when the
        // crosshair has a node in range (the hook reticle's cyan: something to hook).
        public const float Arcline21NodePulseRate = 0.8f, Arcline21NodeFloor = 0.45f, Arcline21NodeAimed = 1.6f;
        // A node glow is a cube of this edge (m) x the web's nodeRadius.
        public const float Arcline21NodeCube = 0.9f;
        // The terminal's cabinet (m): the footprint, the height, the glowing hook on its face.
        public const float Arcline21CabinetWidth = 1.1f, Arcline21CabinetDepth = 0.7f, Arcline21CabinetHeight = 2.1f, Arcline21HookSize = 0.6f;
    }
}
