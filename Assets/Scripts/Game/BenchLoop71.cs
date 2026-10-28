// NIGHT71 builder F (F1, T-152a "the camera's chop measured honestly"): the bench's player-loop split, a measurement only (it changes
// nothing the game does or draws; installed only while `-skygrindBench` runs, and taken out before the Player quits).
// Why: the bench's worst 1 % of frames (n64-rc2: 10.57 ms against a 3.61 ms median band) are main-thread time OUTSIDE SurvivalGame.Update in
// 154 of 156 frames, even with each interval paired to the frame it measured (Research/2026-10-26-wave71/F/F1/spike-read.txt). Which engine
// system that is needs the Profiler's markers, and a release Player records none (the bench prints "n/a (release Player)"). So this file
// puts a timestamp after every child of every player-loop phase (Unity's own names: PostLateUpdate.FinishFrameRendering,
// PostLateUpdate.PlayerUpdateCanvases, PreLateUpdate.ScriptRunBehaviourLateUpdate, TimeUpdate.WaitForLastPresentationAndUpdateTime ...),
// keeps each frame's time per slot, and at the end ranks the slots by how much more time they take in the worst 1 % of frames than in the
// median band: the top slot is the spike's source by Unity's own phase name. A frame's row is one whole cycle (from one frame's first stamp
// to the next frame's first), so the rows add up to the frame and nothing is paired across frames.
// Cost: about 110 delegate calls and Stopwatch reads a frame (microseconds); both arms of a pair carry it. Off: -skygrindNoLoopSplit71.
// Also counted per row: the frame's kill, hit and fire events (EventBus.Emitted's counts, read without allocating), so the report says
// whether the worst frames are the frames a kill happens in. Numbers: UnityTuning.F71.cs.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skygrind.Core;
using UnityEngine.LowLevel;

namespace Skygrind.Game
{
    public static class BenchLoop71
    {
        /// <summary>The bench switch that leaves the player loop as it is (no split, no report).</summary>
        public const string NoSwitch = "-skygrindNoLoopSplit71";

        /// <summary>The inserted systems' type (Unity names a player-loop system by its type; never instantiated).</summary>
        sealed class BenchStamp71 { }

        const int Warm = 30; // the first rows warm up (PlayerChecks.Report's rule)

        static PlayerLoopSystem saved;
        static bool installed, armed, recording;
        static readonly List<string> names = new List<string>();
        static int slots, rowsCap, frame, dropped;
        static float[] acc, rows;
        static byte[] kills, hits, fires;
        static long last;
        static EventBus bus;
        static int kill0, hit0, fire0;
        static readonly double TicksToMs = 1000.0 / System.Diagnostics.Stopwatch.Frequency;

        /// <summary>Is the split in the player loop now?</summary>
        public static bool Installed => installed;

        /// <summary>Put a stamp after every child of every phase of the current player loop (rows reserved for `seconds` of bench). False
        /// when the loop has no phases (nothing changed).</summary>
        public static bool Install(EventBus events, double seconds)
        {
            if (installed) return true;
            PlayerLoopSystem root = PlayerLoop.GetCurrentPlayerLoop();
            if (root.subSystemList == null || root.subSystemList.Length == 0) return false;
            saved = root;
            names.Clear();
            names.Add("(between frames: after PostLateUpdate, before TimeUpdate)");
            var phases = new PlayerLoopSystem[root.subSystemList.Length];
            for (int p = 0; p < phases.Length; p++)
            {
                PlayerLoopSystem ph = root.subSystemList[p];
                string pn = ph.type != null ? ph.type.Name : "phase" + p;
                PlayerLoopSystem[] kids = ph.subSystemList ?? new PlayerLoopSystem[0];
                var list = new List<PlayerLoopSystem>(kids.Length * 2 + 1);
                list.Add(Stamp(p == 0 ? 0 : SlotOf(pn + " (before its first child)"), p == 0));
                for (int c = 0; c < kids.Length; c++)
                {
                    list.Add(kids[c]);
                    string cn = kids[c].type != null ? kids[c].type.Name : "child" + c;
                    list.Add(Stamp(SlotOf(pn + "." + cn), false));
                }
                ph.subSystemList = list.ToArray();
                phases[p] = ph;
            }
            slots = names.Count;
            rowsCap = Math.Max(1, (int)Math.Min(64 * 1024, Math.Ceiling(Math.Max(1, seconds) * UnityTuning.BenchLoopRowsPerSecond71)));
            acc = new float[slots];
            rows = new float[rowsCap * slots];
            kills = new byte[rowsCap];
            hits = new byte[rowsCap];
            fires = new byte[rowsCap];
            frame = dropped = 0;
            last = 0;
            armed = recording = false;
            bus = events;
            ReadEvents(out kill0, out hit0, out fire0);
            PlayerLoopSystem split = root;
            split.subSystemList = phases;
            PlayerLoop.SetPlayerLoop(split);
            installed = true;
            return true;
        }

        /// <summary>Start recording at the next frame boundary (the first whole frame).</summary>
        public static void Begin() => armed = true;

        /// <summary>Stop recording (the frame in progress is not kept).</summary>
        public static void End() => armed = recording = false;

        /// <summary>The player loop as it was before Install.</summary>
        public static void Uninstall()
        {
            if (!installed) return;
            PlayerLoop.SetPlayerLoop(saved);
            installed = false;
        }

        static int SlotOf(string name)
        {
            int max = UnityTuning.BenchLoopMaxSlots71;
            if (names.Count < max - 1)
            {
                names.Add(name);
                return names.Count - 1;
            }
            if (names.Count == max - 1) names.Add("(the rest: more children than BenchLoopMaxSlots71)");
            return max - 1;
        }

        static PlayerLoopSystem Stamp(int slot, bool frameStart)
        {
            var s = new PlayerLoopSystem { type = typeof(BenchStamp71) };
            if (frameStart) s.updateDelegate = FrameStart;
            else
            {
                int k = slot;
                s.updateDelegate = () => Mark(k);
            }
            return s;
        }

        static void Mark(int slot)
        {
            long now = System.Diagnostics.Stopwatch.GetTimestamp();
            if (last != 0) acc[slot] += (float)((now - last) * TicksToMs);
            last = now;
        }

        static void ReadEvents(out int k, out int h, out int f)
        {
            k = h = f = 0;
            if (bus == null) return;
            bus.Emitted.TryGetValue("kill", out k);
            bus.Emitted.TryGetValue("hit", out h);
            bus.Emitted.TryGetValue("fire", out f);
        }

        // The first stamp of the first phase: the frame boundary. The time since the last frame's last stamp closes that frame's row.
        static void FrameStart()
        {
            Mark(0);
            ReadEvents(out int k, out int h, out int f);
            if (recording)
            {
                if (frame < rowsCap)
                {
                    Array.Copy(acc, 0, rows, frame * slots, slots);
                    kills[frame] = (byte)Math.Min(255, k - kill0);
                    hits[frame] = (byte)Math.Min(255, h - hit0);
                    fires[frame] = (byte)Math.Min(255, f - fire0);
                    frame++;
                }
                else dropped++;
            }
            else if (armed) recording = true; // the next row is the first whole frame
            kill0 = k;
            hit0 = h;
            fire0 = f;
            Array.Clear(acc, 0, slots);
        }

        /// <summary>The report: the loop's own frame tail, the slots ranked by their excess in the worst 1 % over the median band, the top
        /// source, the frames' events, and `csvBase`.loop.csv (one line per slot). Lines through `log`.</summary>
        public static void Report(Action<string> log, string csvBase)
        {
            if (!installed && frame == 0) return;
            int m = Math.Min(frame, rowsCap);
            int n = m - Warm;
            if (n < 100)
            {
                log($"[skygrind] bench loop: {m} rows timed, too few to rank (need {Warm + 100})");
                return;
            }
            var total = new float[n];
            for (int i = 0; i < n; i++)
            {
                double s = 0;
                int r = (i + Warm) * slots;
                for (int j = 0; j < slots; j++) s += rows[r + j];
                total[i] = (float)s;
            }
            var idx = new int[n];
            for (int i = 0; i < n; i++) idx[i] = i;
            Array.Sort(idx, (x, y) => total[x].CompareTo(total[y]));
            int w0 = (int)(n * 0.99), b0 = (int)(n * 0.45), b1 = (int)(n * 0.55);
            double Mean(Func<int, double> col, int from, int to)
            {
                if (to <= from) return 0;
                double s = 0;
                for (int q = from; q < to; q++) s += col(idx[q]);
                return s / (to - from);
            }
            float Q(double q) => total[idx[Math.Min(n - 1, (int)(n * q))]];
            double worstAll = Mean(i => total[i], w0, n), bandAll = Mean(i => total[i], b0, b1), excessAll = worstAll - bandAll;
            log($"[skygrind] bench loop: {m} frames timed by the player loop ({dropped} past the reserve), {slots} slots; frame p99 {Q(0.99):0.00} ms, p99.9 {Q(0.999):0.00} ms, max {total[idx[n - 1]]:0.00} ms; worst 1% {worstAll:0.000} ms | median band {bandAll:0.000} ms (+{excessAll:0.000})");
            var worst = new double[slots];
            var band = new double[slots];
            var all = new double[slots];
            var wmax = new double[slots];
            for (int j = 0; j < slots; j++)
            {
                int jj = j;
                worst[j] = Mean(i => rows[(i + Warm) * slots + jj], w0, n);
                band[j] = Mean(i => rows[(i + Warm) * slots + jj], b0, b1);
                all[j] = Mean(i => rows[(i + Warm) * slots + jj], 0, n);
                double mx = 0;
                for (int q = w0; q < n; q++) mx = Math.Max(mx, rows[(idx[q] + Warm) * slots + j]);
                wmax[j] = mx;
            }
            var order = new int[slots];
            for (int j = 0; j < slots; j++) order[j] = j;
            Array.Sort(order, (x, y) => (worst[y] - band[y]).CompareTo(worst[x] - band[x]));
            log("[skygrind] bench loop slots by excess (mean over worst 1% | median band 45-55% | all; the worst 1%'s largest; the share of the worst frames' excess)");
            int top = Math.Min(UnityTuning.BenchLoopTop71, slots);
            for (int t = 0; t < top; t++)
            {
                int j = order[t];
                double e = worst[j] - band[j];
                log($"[skygrind] bench loop   {names[j],-64} {worst[j],8:0.000} | {band[j],8:0.000} | {all[j],8:0.000} ms; max {wmax[j],7:0.00} ms; excess {e:+0.000;-0.000} ms ({(excessAll > 0 ? 100 * e / excessAll : 0):0}%)");
            }
            int s0 = order[0];
            log($"[skygrind] bench loop top source: {names[s0]} (+{worst[s0] - band[s0]:0.000} ms of the worst 1%'s +{excessAll:0.000} ms)");
            double Share(byte[] col, int from, int to) => Mean(i => col[i + Warm] > 0 ? 1 : 0, from, to) * 100;
            log($"[skygrind] bench loop events (share of frames with one; worst 1% | median band | all): a kill {Share(kills, w0, n):0.0}% | {Share(kills, b0, b1):0.0}% | {Share(kills, 0, n):0.0}%; a hit {Share(hits, w0, n):0.0}% | {Share(hits, b0, b1):0.0}% | {Share(hits, 0, n):0.0}%; a shot {Share(fires, w0, n):0.0}% | {Share(fires, b0, b1):0.0}% | {Share(fires, 0, n):0.0}%");
            if (string.IsNullOrEmpty(csvBase)) return;
            try
            {
                var sb = new StringBuilder(slots * 96);
                sb.Append("slot,name,worst1_ms,median_band_ms,all_ms,worst1_max_ms,excess_ms\n");
                for (int j = 0; j < slots; j++)
                    sb.Append(j).Append(",\"").Append(names[j].Replace("\"", "'")).Append("\",")
                        .Append(worst[j].ToString("0.0000", CultureInfo.InvariantCulture)).Append(',')
                        .Append(band[j].ToString("0.0000", CultureInfo.InvariantCulture)).Append(',')
                        .Append(all[j].ToString("0.0000", CultureInfo.InvariantCulture)).Append(',')
                        .Append(wmax[j].ToString("0.0000", CultureInfo.InvariantCulture)).Append(',')
                        .Append((worst[j] - band[j]).ToString("0.0000", CultureInfo.InvariantCulture)).Append('\n');
                File.WriteAllText(csvBase + ".loop.csv", sb.ToString());
                log($"[skygrind] bench loop csv: {csvBase}.loop.csv ({slots} slots)");
            }
            catch (Exception e)
            {
                log("[skygrind] bench loop csv: not written: " + e.Message);
            }
        }
    }
}
