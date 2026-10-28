// Quality presets, the AUTO benchmark and dynamic resolution (SPEC 2.0 performance, Phase 7).
// LOW / MEDIUM / HIGH set the pixel ratio, bloom, SMAA and the particle cap. AUTO (the default)
// renders the title orbit at HIGH for a few seconds and picks the preset from the p95 frame time.
// While playing, a p95 above 18 ms for 2 s lowers the render scale 10% (min 0.6); under 13 ms it
// climbs back. Frame time only; a reused ring buffer, no allocation per frame.
// Port of skygrind/src/fx/quality.js (p17-night6), line for line, NIGHT13 U3-26.4 (agent P). The web's deps become
// callbacks: `onPreset(level)` is post.setQuality + particles.setCap + main.js applyPreset (the Unity side applies the
// preset: Game/Quality.cs and LightAndLens), `onResize()` is main.js resize (the render scale), `settingQuality()` reads
// config.settings.quality live (the SETTINGS screen changes it); the device pixel ratio and settings.renderScale are
// passed to PixelRatio. The ring is a Float32Array as on the web (float[]: computed in double, stored as float once).
// Allocates only on a pick or a scale change (the event payloads), never per frame.
using System;

namespace Skygrind.Core
{
    /// <summary>quality.js 'qualityPicked': AUTO's pick and the benchmark's p95 (ms).</summary>
    public sealed class QualityPickedEvent
    {
        public string level;
        public double p95;
    }

    /// <summary>quality.js 'renderScale': the dynamic scale after a change and the p95 (ms) that moved it.</summary>
    public sealed class RenderScaleEvent
    {
        public double scale;
        public double p95;
    }

    public sealed class Quality
    {
        const double MaxDt = 0.25; // quality.js:87 Math.min(rawDt, 0.25): one frame counts at most a quarter second
        const int MinSamples = 30; // quality.js:107 count < 30: the dynamic resolution waits for 30 frames

        sealed class Bench
        {
            public double elapsed;
            public bool sampling;
        }

        readonly QualityParams q;
        readonly Func<string> settingQuality;
        readonly EventBus events;
        readonly Action<string> onPreset;
        readonly Action onResize;
        readonly float[] times;
        readonly float[] scratch;
        int head = 0;
        int count = 0;
        readonly RenderScaleState dyn = new RenderScaleState { scale = 1, over = 0, under = 0 };
        double checkTimer = 0;
        string level = null; // the preset in use
        string chosen = null; // AUTO's pick this session
        Bench bench = null; // {elapsed} while the AUTO benchmark runs
        double lastP95 = 0;

        /// <param name="q">config.quality</param>
        /// <param name="settingQuality">config.settings.quality, read live ("auto" | "low" | "medium" | "high")</param>
        /// <param name="events">'qualityPicked' and 'renderScale' are emitted here</param>
        /// <param name="onPreset">a preset is applied (post.setQuality, particles.setCap, main.js applyPreset)</param>
        /// <param name="onResize">main.js resize: the pixel ratio may have changed</param>
        public Quality(QualityParams q, Func<string> settingQuality, EventBus events, Action<string> onPreset, Action onResize)
        {
            this.q = q;
            this.settingQuality = settingQuality;
            this.events = events;
            this.onPreset = onPreset;
            this.onResize = onResize;
            times = new float[q.windowSize];
            scratch = new float[q.windowSize];
        }

        void ClearWindow()
        {
            head = 0;
            count = 0;
        }

        void Apply(string next)
        {
            level = next;
            // post.setQuality(next); particles.setCap(q.presets[next].particleCap): the Unity side's onPreset
            onPreset(next); // POLISH V4: shadows, AO, motes, sheen, lights, grain
            onResize();
        }

        /// <summary>The preset in use (null before the first ApplySetting).</summary>
        public string Level => level;

        /// <summary>The dynamic resolution's scale (1 = full).</summary>
        public double RenderScale => dyn.scale;

        public bool Benchmarking => bench != null;

        /// <summary>The last p95 (ms): the benchmark's, then the dynamic resolution's checks'.</summary>
        public double P95 => lastP95;

        /// <summary>AUTO's pick this session (null until the benchmark ends).</summary>
        public string Chosen => chosen;

        /// <summary>Pixel ratio for the renderer: preset × the settings render scale × the dynamic scale.</summary>
        public double PixelRatio(double devicePixelRatio, double settingsRenderScale)
        {
            double dpr = devicePixelRatio != 0 && !double.IsNaN(devicePixelRatio) ? devicePixelRatio : 1; // window.devicePixelRatio || 1
            return QualityMath.PresetPixelRatio(level ?? "high", dpr, q) * settingsRenderScale * dyn.scale;
        }

        /// <summary>Apply settings.quality (boot + settings changes). AUTO keeps its pick, or benchmarks later.</summary>
        public void ApplySetting()
        {
            string want = settingQuality();
            if (want == "auto") Apply(chosen ?? "high");
            else Apply(want);
        }

        /// <summary>Start the AUTO benchmark (the title calls this; once per session).</summary>
        public void StartBenchmark()
        {
            if (settingQuality() != "auto" || chosen != null || bench != null) return;
            Apply("high");
            bench = new Bench { elapsed = 0, sampling = false };
            ClearWindow();
        }

        /// <summary>Mode changes, loading and menus shouldn't count toward the p95.</summary>
        public void ResetWindow()
        {
            ClearWindow();
            dyn.over = dyn.under = 0;
        }

        /// <summary>One frame.</summary>
        /// <param name="rawDt">s, unclamped frame time</param>
        /// <param name="playing">only gameplay drives the dynamic resolution</param>
        public void Update(double rawDt, bool playing)
        {
            double ms = JsMath.Min(rawDt, MaxDt) * 1000;
            times[head] = (float)ms;
            head = (head + 1) % q.windowSize;
            count = Math.Min(count + 1, q.windowSize);
            if (bench != null)
            {
                bench.elapsed += rawDt;
                if (!bench.sampling && bench.elapsed >= q.benchWarmup)
                {
                    bench.sampling = true;
                    ClearWindow();
                }
                else if (bench.sampling && bench.elapsed >= q.benchWarmup + q.benchTime)
                {
                    lastP95 = QualityMath.Percentile(times, count, 0.95, scratch);
                    chosen = QualityMath.PickQuality(lastP95, QualityMath.Percentile(times, count, 0.5, scratch), q);
                    bench = null;
                    if (settingQuality() == "auto") Apply(chosen);
                    events.Emit("qualityPicked", new QualityPickedEvent { level = chosen, p95 = lastP95 });
                }
                return;
            }
            if (!playing) return;
            checkTimer += rawDt;
            if (checkTimer < q.dynamic.checkEvery || count < MinSamples) return;
            lastP95 = QualityMath.Percentile(times, count, 0.95, scratch);
            double before = dyn.scale;
            QualityMath.StepRenderScale(dyn, lastP95, QualityMath.Percentile(times, count, 0.5, scratch), checkTimer, q);
            checkTimer = 0;
            if (dyn.scale != before)
            {
                ClearWindow(); // judge the new scale on its own frames
                onResize();
                events.Emit("renderScale", new RenderScaleEvent { scale = dyn.scale, p95 = lastP95 });
            }
        }
    }
}
