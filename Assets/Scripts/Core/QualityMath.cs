// Quality + dynamic resolution rules (SPEC 2.0 §12 performance, Phase 7). Pure: no three.js, no DOM,
// unit tested. `q` is config.quality.
// Port of skygrind/src/fx/qualityMath.js (p17-night6), line for line, NIGHT13 U3-26.4 (agent P); tests/quality.test.js
// is QualityTests. The samples are Float32Array values (float[]); every comparison and product is in double, as JS
// widens them. Math.round / min / max are JsMath's (half toward +infinity; NaN and -0 as JS). QualityParams is
// config.quality read once (the web reads `q` live; nothing mutates it at run time).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>config.quality (web config.js:2413-2443), read once.</summary>
    public sealed class QualityParams
    {
        /// <summary>config.quality.dynamic: the dynamic resolution's thresholds and times.</summary>
        public sealed class DynamicParams
        {
            public double highMs, lowMs, holdTime, holdUpTime, helpRatio, lockoutTime, step, minScale, checkEvery;
        }

        struct Ratio
        {
            public bool device; // pixelRatio: 'device'
            public double value;
        }

        readonly Dictionary<string, Ratio> presetRatio = new Dictionary<string, Ratio>();
        public double maxDeviceRatio, benchWarmup, benchTime, autoHighMs, autoMediumMs, maxVsyncMs, vsyncSlack;
        public int windowSize;
        public readonly DynamicParams dynamic = new DynamicParams();

        public static QualityParams From(GameConfig cfg)
        {
            var q = new QualityParams();
            foreach (var kv in cfg.Obj("quality.presets"))
            {
                object r = PortData.Obj(kv.Value).TryGetValue("pixelRatio", out object v) ? v : null;
                if (r is string s && s == "device") q.presetRatio[kv.Key] = new Ratio { device = true };
                else if (r != null) q.presetRatio[kv.Key] = new Ratio { value = PortData.Num(r) };
            }
            q.maxDeviceRatio = cfg.Num("quality.maxDeviceRatio");
            q.benchWarmup = cfg.Num("quality.benchWarmup");
            q.benchTime = cfg.Num("quality.benchTime");
            q.autoHighMs = cfg.Num("quality.autoHighMs");
            q.autoMediumMs = cfg.Num("quality.autoMediumMs");
            q.maxVsyncMs = cfg.Num("quality.maxVsyncMs");
            q.vsyncSlack = cfg.Num("quality.vsyncSlack");
            q.windowSize = cfg.Int("quality.windowSize");
            DynamicParams d = q.dynamic;
            d.highMs = cfg.Num("quality.dynamic.highMs");
            d.lowMs = cfg.Num("quality.dynamic.lowMs");
            d.holdTime = cfg.Num("quality.dynamic.holdTime");
            d.holdUpTime = cfg.Num("quality.dynamic.holdUpTime");
            d.helpRatio = cfg.Num("quality.dynamic.helpRatio");
            d.lockoutTime = cfg.Num("quality.dynamic.lockoutTime");
            d.step = cfg.Num("quality.dynamic.step");
            d.minScale = cfg.Num("quality.dynamic.minScale");
            d.checkEvery = cfg.Num("quality.dynamic.checkEvery");
            return q;
        }

        /// <summary>q.presets[level]?.pixelRatio: false when the level has no preset (or no pixelRatio).</summary>
        public bool PixelRatioOf(string level, out bool device, out double value)
        {
            if (level != null && presetRatio.TryGetValue(level, out Ratio r))
            {
                device = r.device;
                value = r.value;
                return true;
            }
            device = false;
            value = 0;
            return false;
        }
    }

    /// <summary>stepRenderScale's state: {scale, over, under} (lockout and dropFrom start absent: 0).</summary>
    public sealed class RenderScaleState
    {
        public double scale = 1, over, under, lockout, dropFrom;
    }

    public static class QualityMath
    {
        /// <summary>
        /// The p-th percentile (0..1) of the first `count` values of `values`, sorting `scratch` in place (no
        /// allocation). `scratch` must be at least `count` long.
        /// </summary>
        public static double Percentile(float[] values, int count, double p, float[] scratch)
        {
            if (count <= 0) return 0;
            for (int i = 0; i < count; i++) scratch[i] = values[i];
            Array.Sort(scratch, 0, count); // view = scratch.subarray(0, count); view.sort() (a typed array sorts numerically)
            int index = (int)JsMath.Min(count - 1, JsMath.Max(0, Math.Ceiling(p * count) - 1));
            return scratch[index];
        }

        /// <summary>
        /// Keeping up: the frames sit on the display's refresh (vsync) — the p95 barely above the median and
        /// the median at a real refresh rate — so a 60 Hz screen's 16.7 ms counts as fast.
        /// </summary>
        public static bool KeepingUp(double p95Ms, double medianMs, QualityParams q)
        {
            return medianMs <= q.maxVsyncMs && p95Ms <= medianMs * q.vsyncSlack;
        }

        /// <summary>AUTO: pick a preset from the benchmark's p95 + median frame times (ms).</summary>
        public static string PickQuality(double p95Ms, double medianMs, QualityParams q)
        {
            if (p95Ms <= q.autoHighMs || KeepingUp(p95Ms, medianMs, q)) return "high";
            if (p95Ms <= q.autoMediumMs) return "medium";
            return "low";
        }

        /// <summary>Device pixel ratio for a preset (before the render scales).</summary>
        public static double PresetPixelRatio(string level, double dpr, QualityParams q)
        {
            // const ratio = q.presets[level]?.pixelRatio ?? 1;
            bool device = false;
            double ratio = 1;
            if (q.PixelRatioOf(level, out bool d, out double v))
            {
                device = d;
                ratio = v;
            }
            return device ? JsMath.Min(dpr, q.maxDeviceRatio) : ratio; // ratio === 'device' ? Math.min(dpr, q.maxDeviceRatio) : ratio
        }

        /// <summary>
        /// Dynamic resolution, one decision per check. `state` = {scale, over, under} (s spent above the
        /// high threshold / below the low one or keeping up with vsync). Returns the new scale; mutates `state`.
        /// </summary>
        /// <param name="p95Ms">the recent p95 frame time</param>
        /// <param name="medianMs">the recent median frame time</param>
        /// <param name="dt">s since the last check</param>
        public static double StepRenderScale(RenderScaleState state, double p95Ms, double medianMs, double dt, QualityParams q)
        {
            QualityParams.DynamicParams d = q.dynamic;
            state.lockout = JsMath.Max(0, state.lockout - dt); // (state.lockout ?? 0): absent is 0
            state.over = p95Ms > d.highMs ? state.over + dt : 0;
            state.under = p95Ms < d.lowMs || KeepingUp(p95Ms, medianMs, q) ? state.under + dt : 0;
            if (state.over >= d.holdTime && state.lockout == 0)
            {
                if (Truthy(state.dropFrom) && p95Ms > state.dropFrom * d.helpRatio)
                {
                    // The last drop didn't help: the CPU, not the GPU, is the limit. Undo it and stop lowering.
                    state.scale = JsMath.Min(1, JsMath.Round((state.scale + d.step) * 100) / 100);
                    state.dropFrom = 0;
                    state.lockout = d.lockoutTime;
                }
                else
                {
                    state.dropFrom = p95Ms;
                    state.scale = JsMath.Max(d.minScale, JsMath.Round((state.scale - d.step) * 100) / 100);
                }
                state.over = 0;
            }
            else if (state.under >= d.holdUpTime && state.scale < 1)
            {
                state.dropFrom = 0;
                state.scale = JsMath.Min(1, JsMath.Round((state.scale + d.step) * 100) / 100);
                state.under = 0;
            }
            return state.scale;
        }

        /// <summary>JS truthiness of a number (state.dropFrom: absent / 0 / NaN are false).</summary>
        static bool Truthy(double x) => x != 0 && !double.IsNaN(x);
    }
}
