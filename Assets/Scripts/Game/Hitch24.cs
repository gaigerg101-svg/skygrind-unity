// NIGHT24 (builder F, the lead's switch-lag row): the hitch recorder. UNCOMPILED when written. A ring of Time.unscaledDeltaTime per frame; a frame over
// max(3 x the running median, 8 ms) is logged as "[hitch] <ms> ms tag=<last tag> cpu=<ms> gc=<bytes>" (the main thread's ms and the GC bytes allocated in
// that frame, from ProfilerRecorder). Tag() is called at each screen switch; Sample() once per rendered frame (HitchTour24 calls it); Summary() prints the
// worst frame, p99 and the counts above 33 ms and 16.7 ms. A window (BeginWindow / EndWindow) is one switch's 90 frames.
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Hitch24
    {
        const int Cap = UnityTuning.Hitch24Cap, MedianWindow = UnityTuning.Hitch24MedianWindow; // the numbers and why: UnityTuning.Audit24.cs
        const float Floor = UnityTuning.Hitch24Floor, Factor = UnityTuning.Hitch24Factor, Over33 = UnityTuning.Hitch24Over33, Over167 = UnityTuning.Hitch24Over167;

        static readonly float[] dts = new float[Cap];
        static readonly float[] med = new float[MedianWindow];
        static readonly float[] tmp = new float[MedianWindow];
        static int n; // frames since Begin (the ring holds the last Cap)
        static string tag = "start";
        static ProfilerRecorder mainRec, gcRec;
        static bool running;

        /// <summary>Where every logged hitch goes besides Debug.Log (the tour copies it into its result list).</summary>
        public static readonly List<string> Log = new List<string>();

        // the open window
        static string winTag;
        static int winFrames, winWorstAt, winOver33;
        static float winWorst;

        // NIGHT25 (builder F, switch-lag): stopwatch marks. Mark("tag") logs "[hitch-mark] <tag> +<ms> ms since the last mark" while a tour runs (Active),
        // so a first run start's phases show where the cold 346 ms goes. A mark is one static call, no logic; with no tour running it is one bool test.
        static readonly System.Diagnostics.Stopwatch markWatch = new System.Diagnostics.Stopwatch();
        static double lastMarkMs;

        /// <summary>A tour is running (marks print).</summary>
        public static bool Active => running;

        /// <summary>A mark: logs the ms since the previous mark (the first mark of a run start, "startrun:begin", prints the gap since the last mark of the run before).</summary>
        public static void Mark(string t)
        {
            if (!running) return;
            if (!markWatch.IsRunning) markWatch.Start();
            double now = markWatch.Elapsed.TotalMilliseconds;
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            string line = $"[hitch-mark] {t} +{(now - lastMarkMs).ToString("0.0", ci)} ms (frame {Time.frameCount}, tag={tag})";
            lastMarkMs = now;
            Log.Add(line);
            Debug.Log(line);
        }

        // NIGHT25 (builder F, switch-lag): the game's own profiler markers (SurvivalGame.Render.cs and the sim's), read per frame; a hitch line lists every
        // marker that took 2 ms or more that frame ("R.Ui=181.2"), so the 8 MB frame at the first PLAY names its system. Dev Players only (markers exist there).
        static readonly string[] markerNames = { "SKYGRIND.SimStep", "SKYGRIND.R.Feel", "SKYGRIND.R.Viewmodel", "SKYGRIND.R.World", "SKYGRIND.R.Enemies", "SKYGRIND.R.Projectiles", "SKYGRIND.R.Props", "SKYGRIND.R.Ui", "SKYGRIND.R.Signs", "SKYGRIND.R.Fx", "SKYGRIND.R.Light", "SKYGRIND.R.Other", "SKYGRIND.Husks", "SKYGRIND.Brain", "SKYGRIND.Crowd", "SKYGRIND.Mover" };
        static readonly ProfilerRecorder[] markerRec = new ProfilerRecorder[markerNames.Length];

        static string BigMarkers()
        {
            var sb = new System.Text.StringBuilder();
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            for (int i = 0; i < markerRec.Length; i++)
            {
                if (!markerRec[i].Valid) continue;
                double ms = markerRec[i].LastValue / 1e6;
                if (ms >= 2.0) sb.Append(' ').Append(markerNames[i].Substring(9)).Append('=').Append(ms.ToString("0.0", ci));
            }
            return sb.Length == 0 ? " markers: none >= 2 ms" : " markers:" + sb;
        }

        public static void Begin()
        {
            if (running) return;
            running = true;
            n = 0;
            tag = "start";
            Log.Clear();
            mainRec = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
            gcRec = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
            for (int i = 0; i < markerNames.Length; i++) markerRec[i] = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, markerNames[i], 1);
        }

        public static void End()
        {
            if (!running) return;
            running = false;
            mainRec.Dispose();
            gcRec.Dispose();
            for (int i = 0; i < markerRec.Length; i++) markerRec[i].Dispose();
        }

        /// <summary>A screen switch is about to happen: later hitches carry this tag.</summary>
        public static void Tag(string t) => tag = t;

        /// <summary>Forget the frames so far (a pass's statistics start clean); the tag stays.</summary>
        public static void ResetStats() => n = 0;

        public static void BeginWindow(string t)
        {
            winTag = t;
            winFrames = winWorstAt = winOver33 = 0;
            winWorst = 0f;
        }

        /// <summary>The window's line: "hitch: &lt;tag&gt; worst &lt;ms&gt; at frame &lt;n&gt; count&gt;33ms &lt;k&gt;".</summary>
        public static string EndWindow() =>
            $"hitch: {winTag} worst {(winWorst * 1000f).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} ms at frame {winWorstAt} count>33ms {winOver33}";

        /// <summary>Call once per rendered frame (after the frame's yield).</summary>
        public static void Sample()
        {
            if (!running) return;
            float dt = Time.unscaledDeltaTime;
            dts[n % Cap] = dt;
            // the running median of the last MedianWindow frames before this one
            int have = Math.Min(n, MedianWindow);
            float median = 0f;
            if (have > 0)
            {
                for (int i = 0; i < have; i++) tmp[i] = dts[(n - 1 - i) % Cap];
                Array.Sort(tmp, 0, have);
                median = tmp[have / 2];
            }
            n++;
            winFrames++;
            if (dt > winWorst)
            {
                winWorst = dt;
                winWorstAt = winFrames;
            }
            if (dt > Over33) winOver33++;
            float limit = Mathf.Max(Factor * median, Floor);
            if (have >= 10 && dt > limit)
            {
                double cpu = mainRec.Valid ? mainRec.LastValue / 1e6 : -1; // ns -> ms
                long gc = gcRec.Valid ? gcRec.LastValue : -1;
                string line = $"[hitch] {(dt * 1000f).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} ms tag={tag} cpu={cpu.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)} gc={gc}" + BigMarkers();
                Log.Add(line);
                Debug.Log(line);
            }
        }

        /// <summary>The frames since Begin / ResetStats: worst, p99, counts above 33 ms and 16.7 ms.</summary>
        public static void Stats(out float worstMs, out float p99Ms, out int over33, out int over167, out int frames)
        {
            frames = Math.Min(n, Cap);
            worstMs = p99Ms = 0f;
            over33 = over167 = 0;
            if (frames == 0) return;
            var s = new float[frames];
            Array.Copy(dts, s, frames);
            if (n > Cap) Array.Copy(dts, s, frames); // the ring is full: order does not matter for the statistics
            Array.Sort(s);
            worstMs = s[frames - 1] * 1000f;
            p99Ms = s[Math.Min(frames - 1, (int)(frames * 0.99f))] * 1000f;
            foreach (float d in s)
            {
                if (d > Over33) over33++;
                if (d > Over167) over167++;
            }
        }

        public static string Summary()
        {
            Stats(out float worst, out float p99, out int o33, out int o167, out int frames);
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            return $"worst {worst.ToString("0.0", ci)} ms, p99 {p99.ToString("0.0", ci)} ms, n>33 {o33}, n>16.7 {o167} ({frames} frames)";
        }
    }
}
