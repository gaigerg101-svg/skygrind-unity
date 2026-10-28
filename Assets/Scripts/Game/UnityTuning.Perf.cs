// NIGHT13 U3-26.4 (agent P: the perf switches, quality.js's remainder, the shader warm-up): the Unity-only render and
// timing numbers, each with a comment saying why the web has no value for it.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // The incremental GC's time slice per frame (ns), set at boot (PerfSwitches gcSlice). Unity's default is 3 ms, and
        // uncapped (vsync 0, no targetFrameRate) a collecting frame may spend all of it; 1 ms bounds that cost (a
        // collection then spans more frames). The web's GC is the browser's: it has no such number.
        public const ulong GcSliceNs = 1000000;

        // quality.js's devicePixelRatio on the desktop. Unity renders at the native resolution (Screen.width / height are
        // device pixels), so the web's 'device' ratio (HIGH) is the native one: 1. The web reads window.devicePixelRatio
        // (CSS px to device px); Unity has no CSS pixels.
        public const double QualityDevicePixelRatio = 1;

        // The highest URP render scale the quality presets may ask for (x the URP asset's own scale). The web's MEDIUM
        // renders at a 1.25 pixel ratio, which on a ratio-1 screen supersamples above HIGH; above native is GG's call
        // (Research/2026-10-04-audit-input.md section 7), so the presets stay at or under native. The web has no cap.
        public const float QualityMaxRenderScale = 1f;

        // Shader warm-up (Warmup.cs): the StreamingAssets folder of the recorded GraphicsStateCollection files
        // (SKYGRIND-<graphics API>-<quality level>.graphicsstate). The web has no shader warm-up (WebGL compiles on use).
        public const string WarmupFolder = "Warmup";
    }
}
