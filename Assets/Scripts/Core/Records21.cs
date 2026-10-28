// NIGHT21 U3-35.7 THE GRIND: records.js' endless record, kept per HEAT rung (RESULTS shows the best wave per HEAT). The judging is
// the web's `judge('endless', run, best)`: better = a higher score; the lines are its strings. The store is a versioned text blob
// ("v1;heat:wave:score;..."): a blob of another version is dropped, never misread (records.js readRecords). Pure; the Game keeps the
// blob in the profile (wired after the Core slice, not compiled in Unity yet).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Skygrind.Core
{
    public sealed class Records21
    {
        public const string Version = "v1";

        public struct Entry
        {
            public int wave;
            public double score;
        }

        public struct Judgement
        {
            public bool newBest, first;
            public string text;
        }

        readonly SortedDictionary<int, Entry> byHeat = new SortedDictionary<int, Entry>();

        /// <summary>The best for a HEAT rung (false: none yet).</summary>
        public bool Best(int heat, out Entry best) => byHeat.TryGetValue(heat, out best);

        public int BestWave(int heat) => byHeat.TryGetValue(heat, out Entry b) ? b.wave : 0;

        /// <summary>records.js judge('endless', run, best): the results line.</summary>
        public static Judgement Judge(Entry run, Entry? best)
        {
            bool first = best == null;
            bool newBest = first || run.score > best.Value.score;
            string text;
            if (first) text = "FIRST RUN ON RECORD";
            else
            {
                double diff = Math.Abs(newBest ? run.score - best.Value.score : best.Value.score - run.score);
                text = newBest ? $"NEW BEST · +{Num(diff)}" : $"BEST {Num(best.Value.score)} · {Num(diff)} SHORT";
            }
            return new Judgement { newBest = newBest, first = first, text = text };
        }

        static string Num(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);

        /// <summary>Judge a finished run against its rung's best and keep it when it is better (records.js submit).</summary>
        public Judgement Submit(int heat, int wave, double score)
        {
            var run = new Entry { wave = wave, score = score };
            Entry? best = byHeat.TryGetValue(heat, out Entry b) ? b : (Entry?)null;
            Judgement j = Judge(run, best);
            if (j.newBest) byHeat[heat] = run;
            return j;
        }

        public void Clear() => byHeat.Clear();

        public string Serialize()
        {
            var parts = new List<string> { Version };
            foreach (var kv in byHeat) parts.Add($"{kv.Key}:{kv.Value.wave}:{Num(kv.Value.score)}");
            return string.Join(";", parts);
        }

        // ---- NIGHT22 U3-36.6: kept across a restart (one small text file under the persistent data path; the caller names the path) ----

        /// <summary>The records file: a temp file then a replace, so a crash never leaves half a blob. False: the disk said no (the records stay in memory).</summary>
        public bool Save(string path)
        {
            try
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                string tmp = path + ".tmp";
                File.WriteAllText(tmp, Serialize());
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>The records a restart finds: a missing, unreadable, corrupt or other-version file is empty records, never an error.</summary>
        public static Records21 Load(string path)
        {
            try
            {
                return File.Exists(path) ? Read(File.ReadAllText(path)) : new Records21();
            }
            catch (Exception)
            {
                return new Records21();
            }
        }

        /// <summary>Take another instance's bests (a load into the sim's own, readonly instance).</summary>
        public void CopyFrom(Records21 other)
        {
            byHeat.Clear();
            foreach (var kv in other.byHeat) byHeat[kv.Key] = kv.Value;
        }

        /// <summary>A stored blob -> its records; missing, corrupt or another version: empty.</summary>
        public static Records21 Read(string raw)
        {
            var r = new Records21();
            if (string.IsNullOrEmpty(raw)) return r;
            string[] parts = raw.Split(';');
            if (parts[0] != Version) return r;
            var tmp = new Dictionary<int, Entry>();
            for (int i = 1; i < parts.Length; i++)
            {
                string[] f = parts[i].Split(':');
                if (f.Length != 3 || !int.TryParse(f[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int heat)
                    || !int.TryParse(f[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int wave)
                    || !double.TryParse(f[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double score))
                    return new Records21();
                tmp[heat] = new Entry { wave = wave, score = score };
            }
            foreach (var kv in tmp) r.byHeat[kv.Key] = kv.Value;
            return r;
        }
    }
}
