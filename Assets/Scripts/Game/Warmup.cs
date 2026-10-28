// NIGHT13 U3-26.4 (agent P): the shader / pipeline-state warm-up at boot (PerfSwitches warmup), Unity 6's way on DX12:
// a GraphicsStateCollection (UnityEngine.Experimental.Rendering, experimental) recorded from real play in a Player,
// loaded from StreamingAssets and warmed up before the first frame, so a shader variant's first use in play doesn't
// stall a frame on its PSO (the max frame and the 0.1%, not the p99). No asset is made in the Editor:
// - record:  SKYGRIND.exe -skygrindTraceShaders <dir or file.graphicsstate> [any check, e.g. -skygrindSmoke]
//            traces every variant and graphics state the run draws and saves them on quit to
//            <dir>/SKYGRIND-<graphics API>-<quality level>.graphicsstate (an existing file there is loaded first, so runs
//            add up). Tracing skips the warm-up, so first uses are seen.
// - ship:    copy the file to Assets/StreamingAssets/<UnityTuning.WarmupFolder>/ and build.
// - at boot: Run() loads StreamingAssets/<WarmupFolder>/SKYGRIND-<API>-<level>.graphicsstate when it is there and was
//            recorded for this API, warms it up (WarmUp, waited on) and logs "[warmup] <what> in <ms> ms"; without the
//            file it logs and does nothing. The Editor skips both (its shaders compile asynchronously; its variants are
//            not the Player's); a Web build has no file system.
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Debug = UnityEngine.Debug;

namespace Skygrind.Game
{
    public static class Warmup
    {
        const string TraceFlag = "-skygrindTraceShaders";
        const string Extension = ".graphicsstate";
        static GraphicsStateCollection trace; // the collection being recorded (kept alive until quit)
        static string tracePath;
        static bool ran;

        /// <summary>SKYGRIND-<graphics API>-<quality level>.graphicsstate: a collection is per API and per quality level.</summary>
        public static string FileName => $"SKYGRIND-{SystemInfo.graphicsDeviceType}-{QualitySettings.names[QualitySettings.GetQualityLevel()]}{Extension}";

        /// <summary>Once, at the game's start: record (-skygrindTraceShaders) or warm up the recorded collection.</summary>
        public static void Run()
        {
            if (ran) return;
            ran = true;
            if (Application.isEditor || Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Debug.Log("[warmup] skipped: " + (Application.isEditor ? "the Editor" : "a Web build"));
                return;
            }
            string[] args = Environment.GetCommandLineArgs();
            int t = Array.IndexOf(args, TraceFlag);
            if (t >= 0 && t + 1 < args.Length)
            {
                BeginTrace(args[t + 1]);
                return;
            }
            if (!PerfSwitches.On(PerfFix.Warmup))
            {
                Debug.Log("[warmup] off (-skygrindPerfOff warmup)");
                return;
            }
            string path = Path.Combine(Application.streamingAssetsPath, UnityTuning.WarmupFolder, FileName);
            if (!File.Exists(path))
            {
                Debug.Log($"[warmup] none: no {path} (record one with {TraceFlag})");
                return;
            }
            var watch = Stopwatch.StartNew();
            var collection = new GraphicsStateCollection();
            if (!collection.LoadFromFile(path))
            {
                Debug.LogWarning($"[warmup] could not load {path}");
                return;
            }
            if (collection.graphicsDeviceType != SystemInfo.graphicsDeviceType)
            {
                Debug.Log($"[warmup] skipped: {path} was recorded for {collection.graphicsDeviceType}, this is {SystemInfo.graphicsDeviceType}");
                return;
            }
            double loadMs = watch.Elapsed.TotalMilliseconds;
            collection.WarmUp().Complete();
            Debug.Log($"[warmup] {collection.variantCount} shader variants, {collection.totalGraphicsStateCount} graphics states ({collection.completedWarmupCount} warmed; recorded on {collection.runtimePlatform}, quality level {collection.qualityLevelName}) in {watch.Elapsed.TotalMilliseconds:0.0} ms (load {loadMs:0.0} ms)");
        }

        /// <summary>-skygrindTraceShaders: trace this run's variants and states, saved on quit (added to the file there).</summary>
        static void BeginTrace(string target)
        {
            tracePath = target.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) ? target : Path.Combine(target, FileName);
            string dir = Path.GetDirectoryName(Path.GetFullPath(tracePath));
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            trace = new GraphicsStateCollection();
            bool added = File.Exists(tracePath) && trace.LoadFromFile(tracePath);
            if (!trace.BeginTrace())
            {
                Debug.LogWarning($"[warmup] tracing could not start ({SystemInfo.graphicsDeviceType})");
                trace = null;
                return;
            }
            Application.quitting += EndTrace;
            Debug.Log($"[warmup] tracing shader variants and graphics states to {Path.GetFullPath(tracePath)}{(added ? $" (adding to its {trace.variantCount} variants)" : "")}; no warm-up this run");
        }

        static void EndTrace()
        {
            Application.quitting -= EndTrace;
            if (trace == null) return;
            trace.EndTrace();
            bool saved = trace.SaveToFile(tracePath);
            Debug.Log($"[warmup] trace {(saved ? "saved" : "NOT saved")}: {trace.variantCount} shader variants, {trace.totalGraphicsStateCount} graphics states to {Path.GetFullPath(tracePath)}");
            trace = null;
        }
    }
}
