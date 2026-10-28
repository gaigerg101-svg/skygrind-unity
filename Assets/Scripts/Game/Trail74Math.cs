// PG3 builder V2 (view row design.n74.trail74; the pure half of Trail74.cs, pinned by Trail74Tests): the rules and the curves of the movement trails.
// No UnityEngine call here (the numbers are UnityTuning.PG3V2.cs's), so an EditMode test runs it without a scene.
using System;

namespace Skygrind.Game
{
    public static class Trail74Math
    {
        public const string Row = "trail74";

        static double Clamp01(double x) => x < 0 ? 0 : x > 1 ? 1 : x;

        static double Smooth(double e0, double e1, double x)
        {
            double t = Clamp01((x - e0) / (e1 - e0));
            return t * t * (3 - 2 * t);
        }

        public static double Lerp(double a, double b, double t) => a + (b - a) * Clamp01(t);

        /// <summary>The fall's share, 0 at N74SlamFallMinM or less, 1 at N74SlamFallMaxM or more (the mover's Slam event carries fallDist).</summary>
        public static double Fall01(double fallDist) => Clamp01((fallDist - UnityTuning.N74SlamFallMinM) / (UnityTuning.N74SlamFallMaxM - UnityTuning.N74SlamFallMinM));

        /// <summary>A ring's growth over its life: an ease-out (1 - (1 - t)^2), the web's slam ring curve (effects.js :291-300).</summary>
        public static double RingEase(double t01) { double t = Clamp01(t01); return 1 - (1 - t) * (1 - t); }

        /// <summary>A ring's radius at t (0..1 of its life).</summary>
        public static double RingRadius(double r0, double r1, double t01) => r0 + (r1 - r0) * RingEase(t01);

        /// <summary>A ring's brightness at t: peak x (1 - t)^1.5, whole at birth and nothing at its end.</summary>
        public static double RingBrightness(double peak, double t01) => peak * Math.Pow(1 - Clamp01(t01), 1.5);

        /// <summary>The slam ring's peak brightness: N74SlamRingPeakMin at a small fall, N74SlamRingPeakMax at a whole one.</summary>
        public static double SlamRingPeak(double fall01) => Lerp(UnityTuning.N74SlamRingPeakMin, UnityTuning.N74SlamRingPeakMax, fall01);

        /// <summary>A tall fall also lays an inner ring.</summary>
        public static bool SlamHasInner(double fall01) => fall01 >= UnityTuning.N74SlamInnerFall01;

        public static int SlamDust(double fall01) => (int)Math.Round(Lerp(UnityTuning.N74SlamDustMin, UnityTuning.N74SlamDustMax, fall01));

        public static int SlamShards(double fall01) => (int)Math.Round(Lerp(UnityTuning.N74SlamShardsMin, UnityTuning.N74SlamShardsMax, fall01));

        public static double SlamFlashSize(double fall01) => Lerp(UnityTuning.N74SlamFlashMin, UnityTuning.N74SlamFlashMax, fall01);

        public static double SlamDustSpeed(double fall01) => Lerp(UnityTuning.N74SlamDustSpeedMin, UnityTuning.N74SlamDustSpeedMax, fall01);

        public static double SlamDustSize(double fall01) => Lerp(UnityTuning.N74SlamDustSizeMin, UnityTuning.N74SlamDustSizeMax, fall01);

        public static double SlamDustLife(double fall01) => Lerp(UnityTuning.N74SlamDustLifeMin, UnityTuning.N74SlamDustLifeMax, fall01);

        public static double SlamShardSpeed(double fall01) => Lerp(UnityTuning.N74SlamShardSpeedMin, UnityTuning.N74SlamShardSpeedMax, fall01);

        /// <summary>The slide trail's speed share: 0 at a walk or slower, the floor (N74SlideCueFloor) just past it, 1 at `full` m/s (the existing slide cue's rule, so the trail starts on the slide's first fast frame).</summary>
        public static double SlideShare(double speed, double walk, double full)
        {
            if (!(speed > walk)) return 0;
            return Math.Max(UnityTuning.N74SlideCueFloor, Clamp01((speed - walk) / Math.Max(1e-6, full - walk)));
        }

        /// <summary>A share moved toward its target at 1 / riseS (up) or 1 / fallS (down) a second; held at dt 0 (a hitstop).</summary>
        public static double Step(double cur, double target, double dt, double riseS, double fallS)
        {
            if (!(dt > 0)) return cur;
            double rate = target >= cur ? 1.0 / Math.Max(1e-4, riseS) : 1.0 / Math.Max(1e-4, fallS);
            double d = target - cur, m = rate * dt;
            return Math.Abs(d) <= m ? target : cur + Math.Sign(d) * m;
        }

        /// <summary>Echo `i` (0 based) of the dash's afterimages: its brightness at the dash share `k`.</summary>
        public static double GhostBrightness(int i, double k)
        {
            if (i < 0 || i >= UnityTuning.N74DashGhostAlpha.Length) return 0;
            return UnityTuning.N74DashGhostAlpha[i] * UnityTuning.N74DashGhostGain * Clamp01(k);
        }

        /// <summary>Echo `i`'s extra scale.</summary>
        public static double GhostScale(int i) => 1 + UnityTuning.N74DashGhostScale * (i + 1);

        /// <summary>Echo `i`'s offset along the motion's opposite, m.</summary>
        public static double GhostBack(int i) => UnityTuning.N74DashGhostBack * (i + 1);

        /// <summary>A dust puff's size over its life (a bloom: it grows and thins).</summary>
        public static double PuffGrow(double t01) => UnityTuning.N74PuffGrowFrom + (UnityTuning.N74PuffGrowTo - UnityTuning.N74PuffGrowFrom) * RingEase(t01);

        /// <summary>A dust puff's brightness over its life: in over the first 20 %, whole to the half, out from there.</summary>
        public static double PuffBrightness(double t01) => UnityTuning.N74PuffPeak * Smooth(0, 0.2, t01) * (1 - Smooth(0.5, 1, t01));

        /// <summary>How far a puff has drifted at `t` s leaving at `v0` m/s against the drag: v0 / k (1 - e^-kt).</summary>
        public static double Drift(double v0, double t) => v0 / UnityTuning.N74PuffDrag * (1 - Math.Exp(-UnityTuning.N74PuffDrag * Math.Max(0, t)));

        /// <summary>A ground-trail segment's brightness at age/life t: whole at birth, a soft ease-out to nothing.</summary>
        public static double SegBrightness(double t01) { double u = 1 - Clamp01(t01); return UnityTuning.N74SlideSegPeak * u * u; }

        /// <summary>A spark's brightness at age/life t: the peak at birth, linear to nothing.</summary>
        public static double SparkBrightness(double t01) => UnityTuning.N74SparkPeak * (1 - Clamp01(t01));
    }
}
