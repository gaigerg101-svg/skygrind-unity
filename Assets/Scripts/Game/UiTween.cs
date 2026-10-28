// NIGHT17 U3-30.3 (the shared commit, the lead's): allocation-free UI tweens, driven by frame time (NIGHT17.md section 1:
// in 120-180 ms ease-out-back with ~10 % overshoot, out 80-120 ms ease-in, every number change animated, nothing linear).
// Pure maths and small structs held in fields: no allocation, no coroutine, no Update of their own (the owner steps them in
// its Render with frameDt). Durations and scales come from UnityTuning.Theme.cs unless a caller passes its own (a
// UnityTuning token too). Visuals only (Pillar 1): nothing here gates input or the sim.
using System;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>Easing curves on t in [0, 1] (clamped).</summary>
    public static class UiTween
    {
        public static float Clamp01(double t) => t <= 0 ? 0f : t >= 1 ? 1f : (float)t;

        /// <summary>Ease-out-back: overshoots past 1 then settles (s = 1.70158: 10 %).</summary>
        public static float OutBack(double t, float s = UnityTuning.ThemeOvershoot)
        {
            float x = Clamp01(t) - 1f;
            return x * x * ((s + 1f) * x + s) + 1f;
        }

        public static float OutCubic(double t)
        {
            float x = 1f - Clamp01(t);
            return 1f - x * x * x;
        }

        /// <summary>NIGHT18 round 2 (additive): ease-out quad, the big banners' slam (gentler than cubic at its end).</summary>
        public static float OutQuad(double t)
        {
            float x = 1f - Clamp01(t);
            return 1f - x * x;
        }

        public static float OutQuint(double t)
        {
            float x = 1f - Clamp01(t);
            return 1f - x * x * x * x * x;
        }

        public static float InCubic(double t)
        {
            float x = Clamp01(t);
            return x * x * x;
        }

        public static float InOutCubic(double t)
        {
            float x = Clamp01(t);
            return x < 0.5f ? 4f * x * x * x : 1f - (-2f * x + 2f) * (-2f * x + 2f) * (-2f * x + 2f) * 0.5f;
        }

        /// <summary>A punch's envelope: 0 at t 0, 1 at t 0.25 (a fast attack), easing back to 0 by t 1.</summary>
        public static float Punch(double t)
        {
            float x = Clamp01(t);
            return x < 0.25f ? OutCubic(x / 0.25f) : 1f - OutCubic((x - 0.25f) / 0.75f);
        }

        /// <summary>A decaying shake in [-1, 1]: sin at `hz` under a linear decay (hz in cycles over the whole t).</summary>
        public static float Shake(double t, float cycles)
        {
            float x = Clamp01(t);
            return Mathf.Sin(x * cycles * 2f * Mathf.PI) * (1f - x);
        }

        /// <summary>
        /// NIGHT18 U3-31.6: a pop in: `from` -> `peak` (ease-out cubic) at t = `peakAt`, then `peak` -> 1 (ease-out cubic) by
        /// t = 1 (HudMotionTests). An entrance that grows (M9: never a first frame at full size) and overshoots.
        /// </summary>
        public static float PopIn(double t, float from, float peak, double peakAt)
        {
            float x = Clamp01(t);
            double pa = Math.Max(1e-6, Math.Min(1 - 1e-6, peakAt));
            if (x <= pa) return from + (peak - from) * OutCubic(x / pa);
            return peak + (1f - peak) * OutCubic((x - pa) / (1 - pa));
        }

        /// <summary>A count-up's duration for a change of `delta` (by its log: ThemeCountMin .. ThemeCountMax).</summary>
        public static double CountTime(double delta)
        {
            double a = Math.Abs(delta);
            if (a < 1) return UnityTuning.ThemeCountMin;
            double k = Math.Min(1.0, Math.Log10(a) / 4.0); // 1 -> 0, 10 000 -> 1
            return UnityTuning.ThemeCountMin + (UnityTuning.ThemeCountMax - UnityTuning.ThemeCountMin) * k;
        }
    }

    /// <summary>An entrance: 0 -> 1 with ease-out-back over `dur`; Out() runs 1 -> 0 with ease-in.</summary>
    public struct UiEnter
    {
        double age, dur;
        bool leaving, running;

        public void In(double duration = UnityTuning.ThemeInTime)
        {
            age = 0;
            dur = Math.Max(1e-4, duration);
            leaving = false;
            running = true;
        }

        public void Out(double duration = UnityTuning.ThemeOutTime)
        {
            age = 0;
            dur = Math.Max(1e-4, duration);
            leaving = true;
            running = true;
        }

        public bool Running => running;
        public bool Gone => leaving && !running;

        /// <summary>Advance by frameDt; the eased value (overshooting past 1 on the way in).</summary>
        public float Step(double frameDt)
        {
            if (!running) return leaving ? 0f : 1f;
            age += frameDt;
            double t = age / dur;
            if (t >= 1) running = false;
            return leaving ? 1f - UiTween.InCubic(t) : UiTween.OutBack(t);
        }
    }

    /// <summary>A scale punch (1 -> peak -> 1), re-armed at most once per `gap`.</summary>
    public struct UiPunch
    {
        double age, dur, sinceLast;
        float amp;

        /// <summary>Kick it (amp = peak - 1, e.g. 0.15); ignored inside `gap` s of the last kick unless forced.</summary>
        public bool Kick(float peak = UnityTuning.ThemePunchScale, double duration = UnityTuning.ThemePunchTime, double gap = UnityTuning.ThemePunchGap, bool force = false)
        {
            if (!force && sinceLast < gap && dur > 0) return false;
            age = 0;
            dur = Math.Max(1e-4, duration);
            amp = peak - 1f;
            sinceLast = 0;
            return true;
        }

        /// <summary>Advance; the scale now (1 at rest).</summary>
        public float Step(double frameDt)
        {
            sinceLast += frameDt;
            if (dur <= 0) return 1f;
            age += frameDt;
            if (age >= dur)
            {
                dur = 0;
                return 1f;
            }
            return 1f + amp * UiTween.Punch(age / dur);
        }

        /// <summary>True during the first frame window of a kick (for a one-frame white flash).</summary>
        public bool Fresh(double window) => dur > 0 && age <= window;
    }

    /// <summary>A rolling number: Set(target) eases the shown value there over a log-of-delta time (ease-out-cubic).</summary>
    public struct UiCount
    {
        double from, to, age, dur;
        bool started;

        public double Target => to;
        public bool Moving => started && age < dur;

        /// <summary>Snap to a value (no roll).</summary>
        public void Snap(double v)
        {
            from = to = v;
            age = dur = 0;
            started = true;
        }

        /// <summary>Roll from the value shown now to `v` (a no-op when `v` is already the target).</summary>
        public void Set(double v)
        {
            if (!started)
            {
                Snap(v);
                return;
            }
            if (v == to) return;
            from = Value;
            to = v;
            age = 0;
            dur = UiTween.CountTime(to - from);
        }

        public double Value => !started ? 0 : age >= dur ? to : from + (to - from) * UiTween.OutCubic(age / dur);

        /// <summary>Advance; the value shown now.</summary>
        public double Step(double frameDt)
        {
            if (started && age < dur) age += frameDt;
            return Value;
        }
    }

    /// <summary>A slam: scale from `from` to 1 over `dur` (ease-out-cubic), for rank letters and hero numbers.</summary>
    public struct UiSlam
    {
        double age, dur;
        float from;

        public void Hit(float fromScale = UnityTuning.ThemeSlamFrom, double duration = UnityTuning.ThemeSlamTime)
        {
            age = 0;
            dur = Math.Max(1e-4, duration);
            from = fromScale;
        }

        public bool Running => dur > 0 && age < dur;

        public float Step(double frameDt)
        {
            if (dur <= 0) return 1f;
            age += frameDt;
            if (age >= dur) return 1f;
            return Mathf.LerpUnclamped(from, 1f, UiTween.OutCubic(age / dur));
        }
    }
}
