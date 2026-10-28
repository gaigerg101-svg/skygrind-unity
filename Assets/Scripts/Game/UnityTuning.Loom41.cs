// NIGHT41 builder B8: THE LOOM's floor sweep's look (Loom37View.N41.cs). Render numbers only; the sweep's rules are design.n41.sweep41's.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The sweep's decal on each ledge it covers (red-orange: danger only): its height over the ledge's top and thickness (m), so it
        /// sits on the stone without z-fighting.</summary>
        public const float Loom41DecalLift = 0.04f, Loom41DecalThick = 0.03f;
        /// <summary>The decal's gain at the wind-up's start and end (it brightens toward the pass), its blink in the last quarter (Hz, amplitude).</summary>
        public const float Loom41DecalGain0 = 0.8f, Loom41DecalGain1 = 2.6f, Loom41DecalBlinkHz = 6f, Loom41DecalBlinkAmp = 0.35f;
        /// <summary>The fill: a brighter strip that grows from the start wall across the decal with the wind-up's progress (its share of the
        /// decal's gain added), so the direction the line will come from reads before it comes.</summary>
        public const float Loom41FillGain = 1.4f;
        /// <summary>The height mark: a thin bar along the start wall at the sweep's top over the band (m thick), so a tower top under it reads
        /// as covered (legs 2-3) and one over it as safe (leg 1).</summary>
        public const float Loom41TopThick = 0.25f, Loom41TopGain = 2.0f;
        /// <summary>The height sheet: a faint red-orange plane over the band at the sweep's height (m thick), its gain from the wind-up's start
        /// to its end (additive: faint so the canyon reads through it; MEASURED n41-r3 frames: without it leg 1's sweep drew nothing, since no
        /// ledge in its band was under its top).</summary>
        public const float Loom41SheetThick = 0.05f, Loom41SheetGain0 = 0.25f, Loom41SheetGain1 = 0.6f;
        /// <summary>The line itself while it passes: a sheet across the band from the void up to the top (m thick), at the live gain.</summary>
        public const float Loom41LineThick = 0.6f, Loom41LineGain = 3.2f, Loom41LineBottomY = -6f;
    }
}
