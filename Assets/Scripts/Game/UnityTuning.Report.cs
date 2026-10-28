// NIGHT13 U3-26.12 (agent G: the version stamp, the run log, COPY REPORT, the dev keys): the Unity-only values (the run
// log's folder and count kept, the report's line count, the stamp's file and place), each with a comment saying why the
// web has no value for it. None is a gameplay number.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the version stamp (BuildStamp at run time, Editor/BuildInfo at build time) ----------------------------------
        // The file BuildInfo writes into the built Player's data folder (Application.dataPath) after a build: one line, the
        // commit's short hash (+ "-dirty"). Nothing in the repo changes. The web had no build of its own to stamp (Vite).
        public const string BuildStampFile = "build-stamp.txt";
        // The product name the stamp line starts with (ProjectSettings' productName is "SKYGRIND-unity", the repo's name).
        public const string StampProduct = "SKYGRIND";
        // The stamp on TITLE: a box this wide and tall (px at 1080p), its bottom this far above the screen's bottom edge, at
        // the right edge's hud.edgeMargin (below the controls line, which starts 40 px up). menus.css had no version text.
        public const float StampWidth = 640f;
        public const float StampHeight = 24f;
        public const float StampBottom = 10f;

        // --- the run log (RunLog) ------------------------------------------------------------------------------------------
        // The folder under Application.persistentDataPath that holds one JSONL file per run. The browser game had no file
        // system to write to (its runs live in localStorage records only).
        public const string RunLogFolder = "runs";
        // Run files kept: after a new file's first write, all but the newest this many (by name: the UTC start time) go.
        public const int RunLogKeep = 20;
        // The settings key that turns the run log off (a Unity-only setting: the lead adds its row, GAME tab) and its
        // default when the row isn't there (on: the log is local and never leaves the machine).
        public const string RunLogSetting = "runLog";
        public const bool RunLogDefault = true;
        // Buffered lines that force a write before the round ends (a long round can't grow the buffer without bound).
        public const int RunLogFlushLines = 256;
        // Writes run on a worker thread (a round's clear never waits on the disk); a write that must land now (quitting,
        // COPY REPORT, the smoke) waits at most this long (ms) for the ones still queued.
        public const int RunLogWaitMs = 3000;
        // A Player started with one of these (PlayerChecks' modes) is automation: it writes no run log (the bench's play
        // scenario would allocate a line per kill inside the measured frames). Mirrors PlayerChecks.Attach's flags.
        public static readonly string[] AutomationFlags = { "-skygrindSmoke", "-skygrindGolden", "-skygrindFingerprint", "-skygrindBench", "-skygrindFarm", "-skygrindReplay", FocusProbe60.Flag, FocusProbe60.NoPauseFlag }; // NIGHT14: the farm, a tape's replay; NIGHT60 B9: the focus probe (no run log, no attract, as the smoke)

        // --- COPY REPORT (UiReport) ----------------------------------------------------------------------------------------
        // The Player log's last lines copied with the report (Application.consoleLogPath).
        public const int ReportLogLines = 50;
        // The most of the log's end read to find those lines (bytes): the Editor's log can be tens of MB.
        public const int ReportTailBytes = 256 * 1024;

        // --- the dev keys (DevKeys; S13, D-15) -----------------------------------------------------------------------------
        // F3 (the debug overlay), F8 (every gate open) and F9 (the next round now) work in the Editor, and in a Player only
        // when started with this flag. The web's debug keys sat behind ?debug=1; a Player has a command line instead.
        public const string DevFlag = "-skygrindDev";
    }
}
