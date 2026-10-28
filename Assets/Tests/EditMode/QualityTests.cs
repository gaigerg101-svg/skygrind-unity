// NIGHT13 U3-26.4 (agent P): quality presets + dynamic resolution. The web's tests/quality.test.js (6 cases, pure maths:
// percentile, pickQuality, presetPixelRatio, stepRenderScale) ported one for one on config.json's quality block, plus
// one Unity-side check of quality.js's state machine (the AUTO benchmark: warm-up, sampling, the pick), which has no
// web test.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class QualityTests
    {
        static QualityParams q;

        [OneTimeSetUp]
        public void Load() => q = QualityParams.From(GameConfig.Load(Path.Combine(Port.Root, "config.json")));

        // describe('percentile')

        [Test]
        public void Percentile_ReadsTheP95OfAWindowWithoutTouchingTheSource()
        {
            var values = new float[100];
            for (int i = 0; i < 100; i++) values[i] = 100 - i; // 100 .. 1
            var scratch = new float[100];
            Assert.That(QualityMath.Percentile(values, 100, 0.95, scratch), Is.EqualTo(95));
            Assert.That(QualityMath.Percentile(values, 100, 0.5, scratch), Is.EqualTo(50));
            Assert.That(values[0], Is.EqualTo(100));
            Assert.That(QualityMath.Percentile(values, 0, 0.95, scratch), Is.EqualTo(0));
        }

        // describe('AUTO + presets')

        [Test]
        public void Auto_PicksHighOnAFastMachine_IncludingOneLockedTo60HzVsync_LowOnASlowOne()
        {
            Assert.That(QualityMath.PickQuality(8, 7, q), Is.EqualTo("high"));
            Assert.That(QualityMath.PickQuality(16.8, 16.7, q), Is.EqualTo("high")); // 60 Hz vsync, keeping up
            Assert.That(QualityMath.PickQuality(22, 17, q), Is.EqualTo("medium")); // hitches
            Assert.That(QualityMath.PickQuality(24, 22, q), Is.EqualTo("medium")); // ~45 fps
            Assert.That(QualityMath.PickQuality(40, 33, q), Is.EqualTo("low"));
        }

        [Test]
        public void PixelRatios_Low1_Medium1point25_HighTheDeviceRatioCappedAt2()
        {
            Assert.That(QualityMath.PresetPixelRatio("low", 3, q), Is.EqualTo(1));
            Assert.That(QualityMath.PresetPixelRatio("medium", 3, q), Is.EqualTo(1.25));
            Assert.That(QualityMath.PresetPixelRatio("high", 3, q), Is.EqualTo(2));
            Assert.That(QualityMath.PresetPixelRatio("high", 1, q), Is.EqualTo(1));
        }

        // describe('dynamic resolution')

        [Test]
        public void DynamicResolution_UndoesADropThatDidNotHelp_CpuBound_AndHoldsOffForAWhile()
        {
            QualityParams.DynamicParams d = q.dynamic;
            var s = new RenderScaleState { scale = 1, over = 0, under = 0 };
            QualityMath.StepRenderScale(s, 33, 25, d.holdTime, q);
            Expect.CloseTo(s.scale, 0.9, 10); // first drop
            QualityMath.StepRenderScale(s, 33, 25, d.holdTime, q); // same p95 after the drop
            Assert.That(s.scale, Is.EqualTo(1)); // undone
            QualityMath.StepRenderScale(s, 33, 25, d.holdTime, q);
            Assert.That(s.scale, Is.EqualTo(1)); // locked out
            QualityMath.StepRenderScale(s, 33, 25, d.lockoutTime, q);
            Expect.CloseTo(s.scale, 0.9, 10); // tries again later
        }

        [Test]
        public void DynamicResolution_KeepsLoweringWhileEachDropHelps()
        {
            QualityParams.DynamicParams d = q.dynamic;
            var s = new RenderScaleState { scale = 1, over = 0, under = 0 };
            QualityMath.StepRenderScale(s, 30, 25, d.holdTime, q);
            QualityMath.StepRenderScale(s, 24, 20, d.holdTime, q);
            Expect.CloseTo(s.scale, 0.8, 10);
        }

        [Test]
        public void DynamicResolution_Drops10PercentAfter2sOver18ms_NeverBelow0point6_AndClimbsBackWhenFastOrKeepingUpWithVsync()
        {
            QualityParams.DynamicParams d = q.dynamic;
            var s = new RenderScaleState { scale = 1, over = 0, under = 0 };
            QualityMath.StepRenderScale(s, 40, 30, d.holdTime / 2, q);
            Assert.That(s.scale, Is.EqualTo(1)); // not long enough yet
            QualityMath.StepRenderScale(s, 40, 30, d.holdTime / 2, q);
            Expect.CloseTo(s.scale, 0.9, 10);
            foreach (double p in new double[] { 36, 32, 28, 25, 22 }) QualityMath.StepRenderScale(s, p, 20, d.holdTime, q); // each drop helps
            Assert.That(s.scale, Is.EqualTo(d.minScale));
            QualityMath.StepRenderScale(s, 15, 12, d.holdUpTime, q); // between the thresholds, not on vsync: holds
            Assert.That(s.scale, Is.EqualTo(d.minScale));
            QualityMath.StepRenderScale(s, 16.8, 16.7, d.holdUpTime / 2, q); // on 60 Hz vsync, not long enough
            Assert.That(s.scale, Is.EqualTo(d.minScale));
            QualityMath.StepRenderScale(s, 16.8, 16.7, d.holdUpTime / 2, q);
            Expect.CloseTo(s.scale, d.minScale + d.step, 10);
            for (int i = 0; i < 20; i++) QualityMath.StepRenderScale(s, 8, 7, d.holdUpTime, q);
            Assert.That(s.scale, Is.EqualTo(1));
        }

        // quality.js's state machine (no web test): AUTO renders HIGH, ignores the warm-up, then picks from the sampled
        // frames' p95 and median, applies the pick, says so ('qualityPicked'), and benchmarks once per session.

        [Test]
        public void Auto_IgnoresTheWarmUp_ThenPicksFromTheSampledFrames_OncePerSession()
        {
            var events = new EventBus();
            QualityPickedEvent picked = null;
            events.On<QualityPickedEvent>("qualityPicked", e => picked = e);
            var applied = new List<string>();
            int resized = 0;
            var quality = new Quality(q, () => "auto", events, applied.Add, () => resized++);
            quality.ApplySetting(); // boot: AUTO draws HIGH until it has picked
            Assert.That(quality.Level, Is.EqualTo("high"));
            quality.StartBenchmark();
            Assert.That(quality.Benchmarking, Is.True);
            // The warm-up: fast frames that must not count.
            double warm = 0;
            while (warm < q.benchWarmup)
            {
                quality.Update(0.008, false);
                warm += 0.008;
            }
            // Then slow frames (30 ms: over autoMediumMs) until the benchmark ends.
            int n = 0;
            while (quality.Benchmarking && n++ < 10000) quality.Update(0.03, false);
            Assert.That(quality.Benchmarking, Is.False);
            Assert.That(quality.Chosen, Is.EqualTo("low"));
            Assert.That(quality.Level, Is.EqualTo("low"));
            Assert.That(applied[applied.Count - 1], Is.EqualTo("low"));
            Assert.That(resized, Is.EqualTo(applied.Count)); // every apply resizes
            Assert.That(picked, Is.Not.Null);
            Assert.That(picked.level, Is.EqualTo("low"));
            Assert.That(picked.p95, Is.EqualTo((double)(float)(0.03 * 1000))); // a Float32Array sample
            quality.StartBenchmark(); // once per session
            Assert.That(quality.Benchmarking, Is.False);
        }
    }
}
