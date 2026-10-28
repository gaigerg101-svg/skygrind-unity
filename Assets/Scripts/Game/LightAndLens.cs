// NIGHT12 U3-25.2, light and lens: the web's look on URP, one view the loop drives once a frame (render-only: it reads
// the sim, never changes it). Its parts:
// - PostFx: post.js's pulses, low HP and energy on the Volume Look.Apply built (Look.Post);
// - LightPools: lightPool.js (blasts, the machines' and the run's steady glows), effects.js's muzzle flash and pops, and
//   the first-run guide's pool;
// - Motes: motes.js, one draw;
// - BlobShadows: blobShadows.js, one instanced draw.
// The quality preset (post.js build + main.js applyPreset): settings.quality (AUTO draws HIGH, as post.js does before
// quality.js's benchmark picks; NIGHT13: QualityRuntime runs the benchmark and dynamic resolution) gives the bloom, SMAA and grain
// (post.quality.*, quality.presets.*.grain with FILM GRAIN on), the pool's lights (quality.presets.*.lights) and the
// motes' share (quality.presets.*.motes x motes.max); re-read when the settings change (GameSettings.version), with the
// tone map and LOW HEALTH VIGNETTE. The shaders it finds by name (SKYGRIND/Motes, SKYGRIND/BlobShadow) must be in the
// build: a missing one turns its effect off with a warning. One profiler marker, SKYGRIND.R.Light.
using Skygrind.Core;
using Unity.Profiling;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class LightAndLens
    {
        struct Preset
        {
            public bool bloom, smaa;
            public double bloomScale, grain, motes;
            public int lights;
        }

        static readonly string[] Levels = { "low", "medium", "high" };
        const int High = 2; // post.js: p.quality[level] ?? p.quality.high (and AUTO's start)

        readonly SurvivalGame game;
        public readonly PostFx post; // null when Look.Apply built no Volume
        public readonly LightPools lights;
        public readonly Motes motes;
        public readonly BlobShadows blobs;
        readonly Preset[] presets = new Preset[3];
        readonly int motesMax;
        int settingsVersion = int.MinValue;
        int qualityVersion = int.MinValue; // NIGHT13 U3-26.4: QualityRuntime.Version (AUTO's pick, the bench's pin)
        static readonly ProfilerMarker Marker = new ProfilerMarker("SKYGRIND.R.Light");

        /// <summary>The preset in use ("low" | "medium" | "high").</summary>
        public string Level { get; private set; }

        public LightAndLens(SurvivalGame g)
        {
            game = g;
            GameConfig cfg = g.cfg;
            for (int i = 0; i < Levels.Length; i++)
            {
                string l = Levels[i];
                presets[i] = new Preset
                {
                    bloom = cfg.Bool($"post.quality.{l}.bloom"),
                    bloomScale = cfg.Num($"post.quality.{l}.bloomScale"),
                    smaa = cfg.Bool($"post.quality.{l}.smaa"),
                    grain = cfg.Num($"quality.presets.{l}.grain"),
                    lights = cfg.Int($"quality.presets.{l}.lights"),
                    motes = cfg.Num($"quality.presets.{l}.motes"),
                };
            }
            motesMax = cfg.Int("motes.max");
            if (Look.Post != null) post = new PostFx(g, Look.Post);
            lights = new LightPools(g);
            motes = new Motes(cfg, Find("SKYGRIND/Motes"));
            blobs = new BlobShadows(g, Find("SKYGRIND/BlobShadow"));
            ReadSettings();
        }

        static Shader Find(string name)
        {
            Shader s = Shader.Find(name);
            if (s == null) Debug.LogWarning($"[light] shader {name} is not in the build (Always Included Shaders): its effect is off");
            return s;
        }

        /// <summary>settings.quality / filmGrain / toneMapping / hurtVignette into the preset (post.js build, main.js applyPreset).</summary>
        void ReadSettings()
        {
            GameSettings s = game.settings;
            settingsVersion = s.version;
            qualityVersion = QualityRuntime.Version;
            string want = QualityRuntime.Effective(s.Str("quality")); // NIGHT13: AUTO's pick (quality.js), HIGH under the bench
            int level = High;
            for (int i = 0; i < Levels.Length; i++) if (want == Levels[i]) level = i; // "auto" (and anything else): HIGH
            Level = Levels[level];
            Preset p = presets[level];
            post?.ApplyPreset(p.bloom, p.bloomScale, p.smaa, s.Bool("filmGrain") ? p.grain : 0, s.Str("toneMapping"), s.Bool("hurtVignette"));
            lights.SetEnabled(p.lights);
            motes.SetCount(p.motes * motesMax);
        }

        /// <summary>
        /// Once a frame, after the views (the loop's Update): the post parameters (frame time), the pool and the flash light
        /// (world time), the motes (world time in play, frame time on TITLE: the living title shows them drift), the blobs.
        /// </summary>
        public void Render(double frameDt, double worldDt)
        {
            using (Marker.Auto())
            {
                if (game.settings.version != settingsVersion || QualityRuntime.Version != qualityVersion) ReadSettings();
                post?.Render(frameDt);
                lights.Render(worldDt, frameDt, game.cam);
                motes.Render(game.flow == Flow.Title ? frameDt : worldDt, game.cam, true);
                blobs.Render(true);
            }
        }

        /// <summary>A new run (StartRun): no pulse, flash or pop left over.</summary>
        public void NewRun()
        {
            post?.NewRun();
            lights.NewRun();
        }

        /// <summary>The first-run guide: one cyan pulsing pool at a Unity-space point (its feet), or none (null).</summary>
        public void Guide(Vector3? at) => lights.Guide(at);
    }
}
