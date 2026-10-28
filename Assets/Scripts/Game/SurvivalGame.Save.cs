// NIGHT13 U3-26.10: SAVE & QUIT in PAUSED and RESUME on TITLE on the run snapshot (design.save; BO6's rules: solo, one
// slot, once a round is cleared, at a break only, never downed or in an exfil), and RESTART in PAUSED (S12).
// The file (beside the profile, UnityTuning.SaveFileName) is two lines: a header {"saveVersion", "seed", "heat"} and the
// snapshot (RunSnapshot.Save over SurvivalSim.SnapshotSections). RESUME reads it and deletes it (consumed on load), starts
// a run on the saved seed and HEAT rung (the run's structure), then loads the snapshot over it; a file of another
// snapshot version, or one that fails to load, is discarded with a log line and the game stays on TITLE. A save is
// written to a temporary file and swapped in. With design.save.enabled off there is no button and no file.
using System;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        bool saveEnabled, saveFileExists;
        int saveMinRounds;
        string savePath;

        /// <summary>Read design.save once (Init's end) and look for a saved run.</summary>
        void InitSave()
        {
            saveEnabled = cfg.Has("design.save.enabled") && cfg.Bool("design.save.enabled");
            saveMinRounds = cfg.Has("design.save.minRounds") ? cfg.Int("design.save.minRounds") : 1;
            savePath = Path.Combine(Application.persistentDataPath, UnityTuning.SaveFileName);
            saveFileExists = saveEnabled && File.Exists(savePath);
        }

        /// <summary>A saved run exists (TITLE shows RESUME).</summary>
        public bool HasSave => saveEnabled && saveFileExists;

        /// <summary>SAVE & QUIT is allowed now (PAUSED shows it): paused (no draft open) at a break the rule allows.</summary>
        public bool SaveAllowed => saveEnabled && flow == Flow.Paused && !DraftOpen && sim.CanSaveAtBreak(saveMinRounds);

        /// <summary>The saved run's path (automation reads it).</summary>
        public string SavePath => savePath;

        /// <summary>Automation (the smoke): save and resume through another file (null: the player's slot again).</summary>
        public void UseSavePath(string path)
        {
            savePath = path ?? Path.Combine(Application.persistentDataPath, UnityTuning.SaveFileName);
            saveFileExists = saveEnabled && File.Exists(savePath);
        }

        /// <summary>Write the run snapshot and go to TITLE (the run is not judged: it goes on later). Logs and stays on failure.</summary>
        public void SaveAndQuit()
        {
            if (!SaveAllowed) return;
            string snapshot;
            try
            {
                snapshot = RunSnapshot.Save(sim.SnapshotSections());
            }
            catch (StateException e)
            {
                Debug.LogWarning("[save] not saved: " + e.Message);
                ui?.ShowHint("CAN NOT SAVE HERE");
                return;
            }
            string header = $"{{\"saveVersion\":{RunSnapshot.SaveVersion},\"seed\":{runSeed},\"heat\":{run.stats.heat}}}";
            string tmp = savePath + ".tmp";
            try
            {
                File.WriteAllText(tmp, header + "\n" + snapshot + "\n");
                if (File.Exists(savePath)) File.Replace(tmp, savePath, null);
                else File.Move(tmp, savePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[save] could not write " + savePath + ": " + e.Message);
                try { if (File.Exists(tmp)) File.Delete(tmp); } catch (Exception) { }
                ui?.ShowHint("COULD NOT SAVE");
                return;
            }
            saveFileExists = true;
            Debug.Log($"[save] saved round {run.round} (seed {runSeed}, {snapshot.Length} bytes) to {savePath}");
            ToTitle();
        }

        /// <summary>Load the saved run (the file is consumed) and play on from its break; a bad file is discarded with a log line.</summary>
        public void ResumeSave()
        {
            AttractYield(); // NIGHT14 U3-27.7: RESUME over the attract stops it first (the sim's flow is TITLE again)
            if (!HasSave || flow != Flow.Title) return;
            string[] lines;
            try
            {
                lines = File.ReadAllLines(savePath);
                File.Delete(savePath); // consumed on load (BO6: one slot, used once)
            }
            catch (Exception e)
            {
                Debug.LogWarning("[save] could not read " + savePath + ": " + e.Message);
                saveFileExists = false;
                return;
            }
            saveFileExists = false;
            uint seed;
            int heatRung;
            try
            {
                if (lines.Length < 2) throw new StateException("the file has no snapshot line");
                var head = new StateReader(lines[0]);
                int version = head.Int("saveVersion");
                if (version != RunSnapshot.SaveVersion) throw new StateException($"saveVersion {version}, this build reads {RunSnapshot.SaveVersion}");
                seed = head.U("seed");
                heatRung = head.Int("heat");
            }
            catch (StateException e)
            {
                Debug.LogWarning("[save] discarded: " + e.Message);
                ui?.ShowHint("THE SAVED RUN COULD NOT BE LOADED");
                return;
            }
            string keepMode = setup.mode;
            int keepHeat = setup.heat;
            setup.mode = RunSetup.Survival;
            setup.heat = heatRung;
            StartRun(seed); // the run's structure on the saved seed and rung; the snapshot then overwrites its state
            setup.mode = keepMode; // NIGHT13 review: TITLE's own choices stay (StartRun has taken the saved ones)
            setup.heat = keepHeat;
            try
            {
                RunSnapshot.Load(lines[1], sim.SnapshotSections());
            }
            catch (StateException e)
            {
                Debug.LogWarning("[save] discarded: " + e.Message);
                ToTitle();
                ui?.ShowHint("THE SAVED RUN COULD NOT BE LOADED");
                return;
            }
            ViewRespawn(); // the camera and the gun at the restored feet
            if (flow == Flow.Paused) FreeCursor(); // saved from PAUSED: it comes back paused (RESUME plays on)
            Debug.Log($"[save] resumed round {run.round} (seed {runSeed}), points {economy.total}, guns {loadout.owned.Count}");
        }

        /// <summary>
        /// PAUSED's RESTART (S12; menus.js pause list): the run ends and is judged as R from DOWN does (Restart), then a new
        /// run starts at once on a new seed. Not while a MODS draft holds the pause.
        /// </summary>
        public void RestartRun()
        {
            if (flow != Flow.Paused || DraftOpen) return;
            EndRun();
            StartRun();
        }
    }
}
