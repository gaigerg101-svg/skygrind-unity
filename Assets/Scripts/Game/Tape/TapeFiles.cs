// NIGHT14 U3-27.8 (agent T): THE TAPE's files. A tape is named as its run's log (RunLog: run-<UTC start yyyyMMdd-HHmmss>-
// <seed>.jsonl), with RunTape.Extension in place of .jsonl, and lives beside it (persistentDataPath/runs; the Game picks the
// folder: SurvivalGame.Tape.cs); the newest design.tape.keep (20, D-56: the run log's rule) are kept. Only run-*<ext> files
// are ever pruned (never the run logs). LastPath is the newest tape written this session and ReportPath what COPY REPORT
// prints (the running run's tape written first, as the run log is). No UnityEngine use.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Skygrind.Game
{
    public static class TapeFiles
    {
        /// <summary>The newest tape written this session (null: none).</summary>
        public static string LastPath;

        /// <summary>The Game's hook: write the running run's tape so far and return its path (null: no running tape).</summary>
        public static Func<string> saveNow;

        /// <summary>COPY REPORT's tape: the running run's (written now), else the last one written; null: none.</summary>
        public static string ReportPath()
        {
            try
            {
                return saveNow?.Invoke() ?? LastPath;
            }
            catch (Exception)
            {
                return LastPath; // never fail the report over a tape
            }
        }

        /// <summary>"run-&lt;UTC yyyyMMdd-HHmmss&gt;-&lt;seed&gt;.sktape": RunLogMath.FileName's name with the tape's extension.</summary>
        public static string FileName(DateTime utc, uint seed) =>
            "run-" + utc.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + "-" + seed.ToString(CultureInfo.InvariantCulture) + RunTape.Extension;

        /// <summary>The tape's name beside a run log's file (the same stem, "-2" suffixes included).</summary>
        public static string NameBeside(string runLogPath) => Path.GetFileNameWithoutExtension(runLogPath) + RunTape.Extension;

        /// <summary>Which tapes go so the newest `keep` stay: the run-*.sktape names sorted by name (their UTC start), all but the last `keep`.</summary>
        public static List<string> ToPrune(IEnumerable<string> files, int keep)
        {
            var tapes = new List<string>();
            foreach (string f in files)
            {
                if (f == null) continue;
                string n = Path.GetFileName(f);
                if (n.StartsWith("run-", StringComparison.Ordinal) && n.EndsWith(RunTape.Extension, StringComparison.Ordinal)) tapes.Add(f);
            }
            tapes.Sort((a, b) => string.CompareOrdinal(Path.GetFileName(a), Path.GetFileName(b)));
            int drop = tapes.Count - Math.Max(0, keep);
            return drop > 0 ? tapes.GetRange(0, drop) : new List<string>();
        }
    }
}
