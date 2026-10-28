// NIGHT14 U3-27.8 (agent T): THE TAPE on the live game (design.tape). BootTape attaches one TapeRecorder to the sim (the
// settings it was built with), begins a tape on every run start (sim.RunStarted), and writes it when the run's tape ends:
// RESULTS, EndRun from DOWN (R / RETRY), a run left without RESULTS (QUIT, a new run over it: written at the next run's
// start), the app closing (written at once). Where:
//   - a player's run: beside its run log (RunLog.DefaultFolder: persistentDataPath/runs), named as the run log
//     (run-<UTC>-<seed>.sktape), while the RUN LOG setting is on (the run log's rule); the newest design.tape.keep (20, D-56)
//   - automation (a PlayerChecks flag, noRecords / profileReadOnly: the smoke): temporaryCachePath/tapes, the same keep
//     (so the lead's smoke can tape a run and -skygrindReplay it; the player's runs/ folder never holds them)
//   - a bot's run (metaSilent, the attract): no tape (NIGHT14 rule 7; the farm and the tests tape explicitly with TapeScript)
//   - a resumed run (RESUME's snapshot): no tape (it starts from the snapshot, not from StartRun); a Web build: no file
// An unhandled exception writes the running tape at once (the step that threw included, so the replay reaches it); COPY
// REPORT writes it too and prints its path (TapeFiles.ReportPath). Each write is one log line ([tape] ...). The bytes are
// taken on the main thread; the file write runs on a worker (a run's end never waits on the disk; quitting waits).
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        TapeRecorder tapeRecorder;
        string tapePath; // the running tape's file (null: this run's tape is not kept)
        int tapeKeep;
        bool tapeAutomationProcess, tapeQuitting, tapeInLog, tapeExceptionWritten, tapeNextRunStarting;
        bool tapeSuspended; // NIGHT14 review A-4: RunFingerprint's own run is not taped
        Task tapeChain = Task.CompletedTask; // the worker's writes, in order

        /// <summary>The live recorder (null with design.tape off): automation reads its tape.</summary>
        public TapeRecorder TapeLive => tapeRecorder;

        partial void BootTape()
        {
            if (!cfg.Bool("design.tape.enabled"))
            {
                Debug.Log("[tape] off (design.tape.enabled false)");
                return;
            }
            tapeKeep = cfg.Int("design.tape.keep");
            tapeAutomationProcess = RunLogMath.IsAutomation(Environment.GetCommandLineArgs());
            tapeRecorder = new TapeRecorder(sim, BuildStamp.Line, settings.Bool, settings.Num);
            tapeRecorder.Finished += TapeFinished;
            sim.RunStarted += TapeRunStarted;
            Application.logMessageReceived += TapeOnLog;
            Application.quitting += TapeOnQuit;
            TapeFiles.saveNow = TapeSaveNow;
        }

        void TapeRunStarted()
        {
            if (tapeRecorder.Recording)
            {
                tapeNextRunStarting = true; // the live flags are the new run's now, not the abandoned one's
                try
                {
                    tapeRecorder.Finish(TapeEnd.Abandoned); // the last run, left without RESULTS (TapeFinished writes it)
                }
                finally
                {
                    tapeNextRunStarting = false;
                }
            }
            tapePath = null;
            tapeExceptionWritten = false;
            if (sim.metaSilent || attractOn || tapeSuspended) return; // NIGHT14 rule 7: a bot's run is never taped live; nor a fingerprint run (A-4)
            string folder = TapeFolder();
            if (folder == null) return;
            tapeRecorder.Begin();
            RunLog log = RunLog.Instance;
            string name = log != null && log.Logging && log.LastPath != null ? TapeFiles.NameBeside(log.LastPath) : TapeFiles.FileName(DateTime.UtcNow, sim.runSeed);
            tapePath = Path.Combine(folder, name);
        }

        /// <summary>The run's tape folder: automation's temporary one, the player's runs/ (the RUN LOG setting on), or none.</summary>
        string TapeFolder()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return null; // a Web build's persistentDataPath is IndexedDB: no tape there (as the run log)
#else
            if (tapeAutomationProcess || noRecords || profileReadOnly) return Path.Combine(Application.temporaryCachePath, UnityTuning.TapeAutomationFolder);
            bool on;
            try
            {
                on = settings.Bool(UnityTuning.RunLogSetting);
            }
            catch (Exception)
            {
                on = UnityTuning.RunLogDefault; // the settings row isn't there (yet)
            }
            return on ? RunLog.DefaultFolder : null;
#endif
        }

        void TapeFinished(RunTape tape)
        {
            string path = tapePath;
            tapePath = null;
            if (path == null) return;
            // A bot's run: metaSilent when the tape began, or set on this same run since (the attract taking a run over: its
            // UseInput ends the tape as InputSwapped).
            bool bot = tape.metaSilent || (!tapeNextRunStarting && (sim.metaSilent || attractOn));
            if (tape.end == TapeEnd.Restored || bot)
            {
                Debug.Log($"[tape] not kept ({(tape.end == TapeEnd.Restored ? "a resumed run" : "a bot's run")})");
                return;
            }
            TapeWrite(tape, path, tapeQuitting);
        }

        /// <summary>The bytes now; the file (and the pruning to the newest design.tape.keep) on the worker, or at once.</summary>
        void TapeWrite(RunTape tape, string path, bool sync)
        {
            byte[] bytes = tape.ToBytes();
            TapeFiles.LastPath = path;
            int keep = tapeKeep;
            Action write = () =>
            {
                try
                {
                    RunTape.WriteFile(path, bytes);
                    string dir = Path.GetDirectoryName(path);
                    foreach (string old in TapeFiles.ToPrune(Directory.GetFiles(dir, "run-*" + RunTape.Extension), keep)) File.Delete(old);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[tape] {path} not written: {e.GetType().Name}: {e.Message}");
                }
            };
            if (sync)
            {
                tapeChain.Wait(UnityTuning.TapeQuitWaitMs);
                write();
            }
            else tapeChain = tapeChain.ContinueWith(_ => write());
            Debug.Log($"[tape] {path}: {tape.steps} steps, {bytes.Length} B, {tape.end}");
        }

        /// <summary>COPY REPORT (TapeFiles.ReportPath): the running tape written now (the run so far), its path; else the last tape's.</summary>
        string TapeSaveNow()
        {
            if (this == null || tapeRecorder == null || !tapeRecorder.Recording || tapePath == null) return TapeFiles.LastPath;
            TapeWrite(tapeRecorder.Snapshot(), tapePath, true);
            return tapePath;
        }

        /// <summary>An unhandled exception: noted in the tape, and the tape so far written once a run (a crash's tape is the one a report needs).</summary>
        void TapeOnLog(string condition, string stackTrace, LogType type)
        {
            if (this == null)
            {
                Application.logMessageReceived -= TapeOnLog; // a Play session ended with domain reload off: the old game is gone
                return;
            }
            if (type != LogType.Exception || tapeInLog || tapeRecorder == null || !tapeRecorder.Recording) return;
            tapeInLog = true;
            try
            {
                tapeRecorder.NoteException(condition);
                if (!tapeExceptionWritten && tapePath != null)
                {
                    tapeExceptionWritten = true;
                    TapeWrite(tapeRecorder.Snapshot(), tapePath, false);
                }
            }
            finally
            {
                tapeInLog = false;
            }
        }

        void TapeOnQuit()
        {
            if (this == null)
            {
                Application.quitting -= TapeOnQuit;
                return;
            }
            tapeQuitting = true;
            if (tapeRecorder != null && tapeRecorder.Recording) tapeRecorder.Finish(TapeEnd.Abandoned); // written at once (TapeFinished, sync)
            tapeChain.Wait(UnityTuning.TapeQuitWaitMs);
        }
    }
}
