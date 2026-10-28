// NIGHT13 U3-26.4 (agent P): quality.js's remainder on Unity (Core/Quality.cs + QualityMath.cs are the port), the perf
// line and the GC slice at boot. QualityRuntime.Boot(game), once, after LightAndLens exists:
// - logs PerfSwitches.Describe() ("[perf] perf: lights on, ...");
// - sets the incremental GC's time slice to UnityTuning.GcSliceNs (PerfSwitches gcSlice; Unity's default is 3 ms);
// - (PerfSwitches quality) builds Core's Quality on config.quality and the live settings.quality, and drives it once a
//   frame from game.OnRender, as main.js does:
//   - AUTO: the benchmark starts on TITLE (main.js:1176 enterTitle: quality.startBenchmark, once per session); every flow
//     change resets the window (main.js:1372 events.on('flow', () => quality.resetWindow())); the samples are the raw
//     unscaled frame time (main.js:2469 quality.update(rawDt, playing)); 'playing' holds DOWN (as PostFx's);
//   - a settings change re-applies settings.quality (main.js applyConfig: quality.applySetting);
//   - the preset in use is what LightAndLens applies (bloom, SMAA, grain, the pool's lights, the motes) through
//     Effective() and Version; this file applies the rest of main.js applyPreset, the shadows (lights.shadows.<level>):
//     LOW turns the key light's shadows off (castShadow false); MEDIUM and HIGH scale the URP asset's own shadow map size
//     and distance by mapSize / high.mapSize and range / high.range (URP's cascades stand in for the web's box around the
//     player: approximate), so HIGH is the asset as built;
//   - the render scale (main.js resize: quality.pixelRatio()): the preset's ratio at UnityTuning.QualityDevicePixelRatio
//     x settings.renderScale x the dynamic scale, capped at UnityTuning.QualityMaxRenderScale, x the asset's own scale.
//   Under -skygrindBench the preset is pinned to HIGH and neither AUTO nor the dynamic resolution runs (logged). In the
//   Editor neither runs either (AUTO draws HIGH, as in u3-n12; LOW / MEDIUM / HIGH apply): the Editor's frame times
//   (its own overhead, its stalls while unfocused) are not a Player's and would pick a preset or a scale for nothing.
//   Not ported: the presets' particle cap, aura, impact, telegraph, AO and sheen (no Unity consumer yet), the shadow
//   radius (URP's soft shadow quality is the asset's).
// - RuntimeAsset(): the runtime URP asset, an Object.Instantiate'd copy of the active one (HideFlags.DontSave), made only
//   when a value must differ from the asset (at the defaults, HIGH at scale 1, it is never made), assigned through
//   QualitySettings.renderPipeline (the PC level's) or GraphicsSettings.defaultRenderPipeline and put back on quit (the
//   Editor's Play mode ends with Application.quitting): the tracked PC_RPAsset is never written. Making it rebuilds URP's
//   pipeline once (one frame).
// No allocation per frame (a log line only when the preset, the pick or the scale changes).
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Scripting;

namespace Skygrind.Game
{
    public static class QualityRuntime
    {
        struct ShadowPreset
        {
            public bool enabled;
            public double mapSize, range;
        }

        static readonly string[] Levels = { "low", "medium", "high" };

        static SurvivalGame game;
        static Quality quality; // Core's quality.js (null: the switch is off, or before Boot)
        static readonly ShadowPreset[] shadows = new ShadowPreset[3];
        static bool pinned; // -skygrindBench: HIGH whatever the setting
        static bool measured; // AUTO's benchmark and the dynamic resolution run (a Player, not the bench)
        static bool flowSeen, pickLogged;
        static Flow lastFlow;
        static int settingsVersion;
        static string shadowLevel; // the shadows applied (null: the asset's and the key light's as built)
        static Light key;
        static LightShadows keyShadows;
        static float baseScale, baseDistance;
        static int baseResolution;
        static UniversalRenderPipelineAsset original, copy;
        static bool viaQualityLevel;

        /// <summary>+ 1 whenever a preset is applied (LightAndLens re-reads Effective when it changes).</summary>
        public static int Version { get; private set; }

        /// <summary>
        /// The preset LightAndLens should draw for settings.quality `want`: quality.js's level in use (AUTO's pick, HIGH
        /// while it benchmarks; HIGH under -skygrindBench); `want` itself with the quality switch off (LightAndLens maps
        /// "auto" to HIGH, as in u3-n12).
        /// </summary>
        public static string Effective(string want) => quality != null && quality.Level != null ? quality.Level : want;

        public static void Boot(SurvivalGame g)
        {
            if (ReferenceEquals(game, g)) return; // once per game
            game = g;
            Debug.Log("[perf] " + PerfSwitches.Describe());
            if (PerfSwitches.On(PerfFix.GcSlice))
            {
                if (GarbageCollector.isIncremental)
                {
                    ulong was = GarbageCollector.incrementalTimeSliceNanoseconds;
                    GarbageCollector.incrementalTimeSliceNanoseconds = UnityTuning.GcSliceNs;
                    Debug.Log($"[perf] incremental GC slice {was / 1e6:0.###} ms -> {UnityTuning.GcSliceNs / 1e6:0.###} ms");
                }
                else Debug.Log("[perf] the GC is not incremental: no slice to set");
            }
            if (!PerfSwitches.On(PerfFix.Quality))
            {
                Debug.Log("[quality] off: settings.quality as in u3-n12 (AUTO draws HIGH; no benchmark, render scale or preset shadows)");
                return;
            }
            GameConfig cfg = g.cfg;
            for (int i = 0; i < Levels.Length; i++)
            {
                string p = $"lights.shadows.{Levels[i]}.";
                shadows[i] = new ShadowPreset { enabled = cfg.Bool(p + "enabled"), mapSize = cfg.Num(p + "mapSize"), range = cfg.Num(p + "range") };
            }
            UniversalRenderPipelineAsset active = Active();
            if (active != null)
            {
                baseScale = active.renderScale;
                baseResolution = active.mainLightShadowmapResolution;
                baseDistance = active.shadowDistance;
            }
            key = RenderSettings.sun; // Look.Apply's key light
            keyShadows = key != null ? key.shadows : LightShadows.Soft;
            pinned = IsBench();
            measured = !pinned && !Application.isEditor;
            Func<string> setting = pinned ? (Func<string>)PinnedHigh
                : measured ? () => g.settings.Str("quality")
                : () => AutoAsHigh(g.settings.Str("quality")); // the Editor: no benchmark, AUTO draws HIGH
            quality = new Quality(QualityParams.From(Move54.QualityConfig(cfg)), setting, g.events, OnPreset, OnResize); // NIGHT54 M move54 (Game/Move54.cs): the row's render scale step and holds (cfg itself when off)
            settingsVersion = g.settings.version;
            quality.ApplySetting(); // boot (main.js applyConfig)
            string want = g.settings.Str("quality");
            Debug.Log(pinned
                ? "[quality] -skygrindBench: the preset pinned to HIGH; AUTO and the dynamic resolution off"
                : $"[quality] setting {want}: preset {quality.Level}{(measured ? want == "auto" ? " until AUTO's benchmark on TITLE picks" : "" : " (the Editor: AUTO draws HIGH, no dynamic resolution)")}; render scale {Active()?.renderScale ?? 1:0.##}");
            g.OnRender += Frame;
        }

        static string PinnedHigh() => "high";

        static string AutoAsHigh(string want) => want == "auto" ? "high" : want;

        static bool IsBench()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-skygrindBench") >= 0) return true;
            string url = Application.absoluteURL;
            return !string.IsNullOrEmpty(url) && url.Contains("skygrindBench"); // a Web build's ?skygrindBench=30
        }

        /// <summary>Once a frame (game.OnRender): settings changes, flow changes, then quality.js's update.</summary>
        static void Frame(double frameDt, double alpha)
        {
            SurvivalGame g = game;
            if (g.settings.version != settingsVersion)
            {
                settingsVersion = g.settings.version;
                quality.ApplySetting(); // main.js applyConfig: LOW / MEDIUM / HIGH, or AUTO's pick
            }
            Flow f = g.flow;
            if (!flowSeen || f != lastFlow)
            {
                flowSeen = true;
                lastFlow = f;
                quality.ResetWindow(); // main.js:1372: loads and menus never count toward the p95
                if (f == Flow.Title && measured) quality.StartBenchmark(); // main.js:1176: once per session
            }
            if (!measured) return;
            if (FeelClock.Active) return; // NIGHT18: a reel pins the preset (its slow JPG frames would drop it mid-scenario)
            quality.Update(Time.unscaledDeltaTime, f == Flow.Playing || f == Flow.Down);
            if (!pickLogged && quality.Chosen != null)
            {
                pickLogged = true;
                Debug.Log($"[quality] AUTO picked {quality.Chosen} (benchmark p95 {quality.P95:0.00} ms); preset in use {quality.Level}");
            }
        }

        /// <summary>quality.js apply(level): LightAndLens re-reads (Version); the shadows here (main.js applyPreset).</summary>
        static void OnPreset(string level)
        {
            Version++;
            ApplyShadows(level);
        }

        /// <summary>main.js resize: the render scale from quality.pixelRatio().</summary>
        static void OnResize()
        {
            double ratio = quality.PixelRatio(UnityTuning.QualityDevicePixelRatio, game.settings.Num("renderScale"));
            float scale = baseScale * Mathf.Min(UnityTuning.QualityMaxRenderScale, (float)ratio);
            UniversalRenderPipelineAsset a = Active();
            if (a == null || a.renderScale == scale) return;
            a = RuntimeAsset();
            if (a == null) return;
            a.renderScale = scale;
            Debug.Log($"[quality] render scale {a.renderScale:0.##} (preset {quality.Level}, dynamic {quality.RenderScale:0.##}, p95 {quality.P95:0.00} ms)");
        }

        /// <summary>main.js applyPreset's shadows: lights.shadows.<level> on the key light and the runtime asset.</summary>
        static void ApplyShadows(string level)
        {
            if (level == shadowLevel) return;
            int i = Array.IndexOf(Levels, level);
            if (i < 0) return; // not a preset: left as it is
            shadowLevel = level;
            ShadowPreset sh = shadows[i], high = shadows[Levels.Length - 1];
            if (key != null)
            {
                LightShadows want = sh.enabled ? keyShadows : LightShadows.None; // sunLight.castShadow = sh.enabled
                if (key.shadows != want) key.shadows = want;
            }
            if (!sh.enabled) return; // the web leaves the map alone while the light casts none
            UniversalRenderPipelineAsset a = Active();
            if (a == null) return;
            int resolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo((int)System.Math.Round(baseResolution * sh.mapSize / high.mapSize)), 256, 8192);
            float distance = (float)(baseDistance * sh.range / high.range);
            if (a.mainLightShadowmapResolution == resolution && a.shadowDistance == distance) return;
            a = RuntimeAsset();
            if (a == null) return;
            a.mainLightShadowmapResolution = resolution;
            a.shadowDistance = distance;
            Debug.Log($"[quality] {level} shadows: map {resolution}, distance {distance:0.#} m (the web's {sh.mapSize} map over a {sh.range} m box: approximate)");
        }

        static UniversalRenderPipelineAsset Active() => GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        /// <summary>
        /// The runtime URP asset: a copy of the active one, made on first use (in Play only) and assigned where the active
        /// one was (the quality level's pipeline, else the default), put back on quit. Null when there is no URP asset.
        /// Writing it never touches the tracked asset.
        /// </summary>
        public static UniversalRenderPipelineAsset RuntimeAsset()
        {
            if (copy != null) return copy;
            if (!Application.isPlaying) return null;
            UniversalRenderPipelineAsset active = Active();
            if (active == null) return null;
            original = active;
            copy = UnityEngine.Object.Instantiate(active);
            copy.name = active.name + " (runtime copy)";
            copy.hideFlags = HideFlags.DontSave;
            viaQualityLevel = QualitySettings.renderPipeline == active;
            if (viaQualityLevel) QualitySettings.renderPipeline = copy;
            else GraphicsSettings.defaultRenderPipeline = copy;
            Application.quitting += Restore;
            Debug.Log($"[quality] runtime copy of {active.name} in use ({(viaQualityLevel ? "quality level " + QualitySettings.names[QualitySettings.GetQualityLevel()] : "default pipeline")}); the asset file is never written");
            return copy;
        }

        /// <summary>On quit (and at the end of the Editor's Play mode): the project's own asset back where it was.</summary>
        static void Restore()
        {
            Application.quitting -= Restore;
            if (copy == null) return;
            if (viaQualityLevel) QualitySettings.renderPipeline = original;
            else GraphicsSettings.defaultRenderPipeline = original;
            copy = null;
        }
    }
}
