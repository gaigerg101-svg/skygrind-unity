// NIGHT27 builder V: Unity-only numbers of the view rows that are not design rows (the rows' own numbers are design.n27.<row> with a _why in
// Design/unity-config.json). Reel bounds only: the scenarios roof / skate / slowaim (Game/FeelReel.N27.View.cs).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The View reel scenarios' wall guard in real seconds (a scenario also bounds itself by game time; unrecorded frames run at the display rate).</summary>
        public const float ReelViewWallGuard27 = 45f;
        /// <summary>Frames averaged per side of the roof's frame-ms A/B (ceiling and lights ON, then OFF).</summary>
        /// <summary>The roof scenario's flat-frame guard: the luminance std (0..1) of the centre 40 % must reach this (a real room frame is about 0.1; a one-colour fill is 0).</summary>
        public const double ReelMinLumaStd27 = 0.02;
        public const int ReelRoofMsFrames27 = 150;
    }
}
