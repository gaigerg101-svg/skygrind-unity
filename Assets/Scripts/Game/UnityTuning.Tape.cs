// NIGHT14 U3-27 (the shared types): agent T's Unity-only numbers, U3-27.8 (the tape's folder, file names, the kept count's file side). Each with a comment saying why
// the web has no value for it (CLAUDE.md rule 5).
// The tape's format constants (RunTape.Extension ".sktape", RunTape.FormatVersion, TapeRecorder.CheckEvery / InitialBytes)
// live beside the format in Assets/Scripts/Game/Tape (Unity-free: Tools/SimHarness compiles that folder without this file);
// the kept count is design.tape.keep (Design/unity-config.json, 20: D-56).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // Automation's tapes (a PlayerChecks flag, noRecords / profileReadOnly: the smoke) go under
        // Application.temporaryCachePath in this folder, never beside the player's run logs. The web had no tapes (its runs
        // were not replayable), so no folder.
        public const string TapeAutomationFolder = "tapes";

        // Quitting (or COPY REPORT) waits at most this long for the worker's earlier tape writes before writing the running
        // tape itself (RunLog's quit wait is the same idea). The web had no files.
        public const int TapeQuitWaitMs = 2000;
    }
}
