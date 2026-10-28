// NIGHT13 U3-26.12 (agent G): the version stamp on TITLE (BuildStamp.Line, small and muted in the bottom-right corner, under
// the controls line) and COPY REPORT in PAUSED (as RESULTS' COPY SCORE LINE: to the system clipboard, then the hint toast).
// The report, for a tester to paste: the stamp; the OS / CPU / GPU / screen line; the non-default settings; the run (flow,
// round, seed, HEAT); the run log's path (written first, so it holds the run so far); the Player log's path and its last
// UnityTuning.ReportLogLines lines. The user folder is replaced by %USERPROFILE% throughout (logs hold paths; the research's
// "scrub them before sending"). Nothing is sent anywhere: the player decides where to paste it.
// NIGHT17 U3-30.8: the stamp and COPY REPORT only in a developer build (game.devMode; RenderReport follows the flag).
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        TextMeshProUGUI versionStamp;

        /// <summary>The TITLE stamp, built once (the line never changes while the game runs).</summary>
        partial void BuildReport()
        {
            versionStamp = Text(title.transform, "Version", BuildStamp.Line, UiTheme.Px(UiTheme.Size.S18), UiTheme.Muted, TextAlignmentOptions.BottomRight,
                new Vector2(-UnityTuning.ThemeSafeInset, UnityTuning.StampBottom), new Vector2(UnityTuning.StampWidth, UnityTuning.StampHeight), new Vector2(1, 0), Face.Body);
            versionStamp.gameObject.SetActive(game.devMode); // NIGHT17 rule 7: a developer's
        }

        /// <summary>NIGHT17 rule 7: the stamp follows the developer flag (a Play check's eval may set it); a bool compare a frame.</summary>
        partial void RenderReport(double frameDt)
        {
            if (versionStamp != null && versionStamp.gameObject.activeSelf != game.devMode) versionStamp.gameObject.SetActive(game.devMode);
        }

        /// <summary>PAUSED's COPY REPORT: the report to the system clipboard, then REPORT COPIED.</summary>
        partial void CopyReport()
        {
            string report;
            try
            {
                report = ReportBody();
            }
            catch (Exception e)
            {
                report = BuildStamp.Line + "\nthe report failed: " + e.GetType().Name + ": " + e.Message; // still something to paste
            }
            GUIUtility.systemCopyBuffer = report;
            ShowHint("REPORT COPIED");
        }

        /// <summary>The report's text (also for automation: the smoke can read it without the clipboard).</summary>
        public string ReportBody()
        {
            RunLog log = RunLog.Instance;
            log?.FlushNow(); // the run so far is on disk before its path is copied
            string runFile = log?.NewestFile();
            string playerLog = Application.consoleLogPath;
            var sb = new StringBuilder(8192);
            sb.Append(BuildStamp.Line).Append('\n');
            sb.Append(SystemLine()).Append('\n');
            sb.Append("SETTINGS (not default): ").Append(RunLog.SettingsSummary(game.settings)).Append('\n');
            sb.Append("RUN: ").Append(RunLine()).Append('\n');
            sb.Append("RUN LOG: ").Append(runFile ?? "none").Append('\n');
            sb.Append("TAPE: ").Append(TapeFiles.ReportPath() ?? "none").Append('\n'); // NIGHT14 U3-27.8: the running run's tape written first (-skygrindReplay <it>)
            sb.Append("PLAYER LOG: ").Append(string.IsNullOrEmpty(playerLog) ? "none" : playerLog).Append(" (last ").Append(UnityTuning.ReportLogLines).Append(" lines)\n");
            sb.Append(ReportText.ReadTail(playerLog, UnityTuning.ReportLogLines, UnityTuning.ReportTailBytes)).Append('\n');
            return ReportText.Scrub(sb.ToString(), Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        static string SystemLine()
        {
            Resolution r = UnityEngine.Screen.currentResolution; // qualified: Ui.Screen(...) builds a menu screen
            string quality = QualitySettings.names.Length > 0 ? QualitySettings.names[Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, QualitySettings.names.Length - 1)] : "-";
            return $"{SystemInfo.operatingSystem} · CPU {SystemInfo.processorType} ({SystemInfo.processorCount} threads) · RAM {SystemInfo.systemMemorySize} MB · " +
                   $"GPU {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceType}, {SystemInfo.graphicsMemorySize} MB) · " +
                   $"{UnityEngine.Screen.width}x{UnityEngine.Screen.height} {UnityEngine.Screen.fullScreenMode} on {r.width}x{r.height} @ {r.refreshRateRatio.value:0.##} Hz · " +
                   $"vsync {QualitySettings.vSyncCount} · cap {Application.targetFrameRate} · quality {quality}";
        }

        string RunLine()
        {
            SurvivalRun run = game.run;
            if (run == null) return game.flow.ToString();
            return $"{game.flow} · {run.state} · round {run.round} · seed {Seeds.SeedCode(game.runSeed)} · HEAT {run.stats.heat} · {(game.lab != null && game.lab.Any ? "LAB " + game.lab.Names() : "no LAB")}";
        }
    }

    /// <summary>COPY REPORT's text helpers.</summary>
    public static class ReportText
    {
        /// <summary>The last `lines` lines of `text` ("\r\n" or "\n"; a trailing newline ends the last line), joined by "\n".</summary>
        public static string Tail(string text, int lines)
        {
            if (string.IsNullOrEmpty(text) || lines <= 0) return "";
            string[] all = text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');
            int from = Math.Max(0, all.Length - lines);
            return string.Join("\n", all, from, all.Length - from);
        }

        /// <summary>
        /// The user folder (both slash directions, any case: Windows paths ignore it, Unity writes forward slashes) replaced by
        /// %USERPROFILE%; unchanged without a folder.
        /// </summary>
        public static string Scrub(string text, string userProfile)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(userProfile)) return text;
            string home = userProfile.TrimEnd('\\', '/');
            if (home.Length < 3) return text; // never a drive root
            foreach (string form in new[] { home.Replace('/', '\\'), home.Replace('\\', '/') })
                text = Regex.Replace(text, Regex.Escape(form) + @"(?![\w.-])", "%USERPROFILE%", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return text;
        }

        /// <summary>
        /// The last `lines` lines of a log file being written (opened shared), reading at most `maxBytes` from its end; a note
        /// instead when there is no file or it can't be read.
        /// </summary>
        public static string ReadTail(string path, int lines, int maxBytes)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return "(no log file)";
            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    long start = Math.Max(0, fs.Length - Math.Max(1, maxBytes));
                    fs.Seek(start, SeekOrigin.Begin);
                    var bytes = new byte[fs.Length - start];
                    int read = 0;
                    while (read < bytes.Length)
                    {
                        int n = fs.Read(bytes, read, bytes.Length - read);
                        if (n <= 0) break;
                        read += n;
                    }
                    string text = Encoding.UTF8.GetString(bytes, 0, read);
                    if (start > 0)
                    {
                        int nl = text.IndexOf('\n');
                        if (nl >= 0) text = text.Substring(nl + 1); // the first line was cut: drop it
                    }
                    return Tail(text, lines);
                }
            }
            catch (Exception e)
            {
                return "(log unreadable: " + e.Message + ")";
            }
        }
    }
}
