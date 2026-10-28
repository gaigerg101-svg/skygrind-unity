// Command-line checks for the built Player (the CLI's Pipeline server doesn't run in a Player): they write to the
// Player log and quit, so a check needs no screenshot of the desktop.
//   SKYGRIND.exe -skygrindSmoke            the slice smoke (SliceSmoke), then quit (NIGHT15: + -skygrindPadShot <png>: the pad wheel captured)
//   SKYGRIND.exe -skygrindShots <dir>      NIGHT15: the capture tour (ShotTour: TITLE, the GAMEPAD tab, a Unity map's views; with -skygrindMap <id>)
//   SKYGRIND.exe -skygrindStrip <dir> [scenario]   NIGHT16: the strip tool's contact sheets (FeelStrip: swap, punch, freeze, slowmo, slide)
//   SKYGRIND.exe -skygrindGolden           every golden vector set and trajectory replay (Skygrind.Core Golden), then quit
//   SKYGRIND.exe -skygrindFingerprint      NIGHT13 U3-26.2: the composed fingerprint (SimFingerprint): a seeded run on
//                                          the fixed input script, a hash every 600 steps, for the baseline seed and
//                                          another (seen red), compared with FingerprintBaseline; then quit
//   SKYGRIND.exe -skygrindBench 60 [play]  a round with director.maxAlive husks chasing a god-mode player for N seconds:
//                                          frame time median / p95 / worst 1% and fps, GC collections and allocations,
//                                          FrameTimingManager CPU / GPU times, then quit (Tools/survival-bench.sh polls
//                                          the power plan before and after it). NIGHT13 U3-26.2: also p99.9, max, the
//                                          count over 6.94 ms, and the breakdown: per frame the interval, the sim steps
//                                          and their ms, Update's ms, GC collections and the heap; FrameTiming's main /
//                                          render / present-wait / GPU; each column's mean over the worst-1% frames
//                                          against the median band; two CSVs beside the -logFile. "play": the player
//                                          strafes, aims at the nearest husk and fires (the scripted input), so guns,
//                                          kills, hitstop and particles run; without it the player stands and is hurt
//                                          every frame (the UI-hurt scenario, D-39's default).
//   ... -skygrindBench 60 -skygrindFpsCap 144         NIGHT13: a pacing arm (the FPS CAP row's targetFrameRate); the "bench cpu"
//                                          line gives FrameTiming's p99s (capped, the interval reads the cap)
//   ... -skygrindBench 60 play -skygrindVsync 1     NIGHT71 F1 (T-152a): the VSYNC arm over the profile's saved VSYNC (0: unsynced,
//                                          uncapped); every bench also prints "bench refresh" (frames over 1.5 display refreshes)
//                                          and "bench loop" (BenchLoop71: the worst 1 %'s time by player-loop slot, a release
//                                          Player's split; -skygrindNoLoopSplit71 leaves the loop as it is)
//   ... -skygrindBench 60 -skygrindProfile 300 f.raw   (a Development Player, NIGHT11 U3-24.2) also writes the
//                                          Profiler's binary log for 300 frames from the bench's middle
//   SKYGRIND.exe -skygrindFarm 100 normal 1 [out.jsonl]   NIGHT14 U3-27.6: THE FARM, seeded AUTOPILOT runs on fresh
//                                          headless compositions, one JSONL line per run (beside the -logFile)
//   SKYGRIND.exe -skygrindReplay <tape>    NIGHT14 U3-27.8: a tape replayed headless: EQUAL or the first differing step
//   ... -skygrindBench 60 autopilot        NIGHT14 U3-27.12: the AUTOPILOT fights the bench's horde (god mode, the meta silent)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skygrind.Core;
using Unity.Profiling;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class PlayerChecks : MonoBehaviour
    {
        /// <summary>NIGHT61 builder D (B1, T-146): the flags of Attach's modes after the focus probe's, in Attach's order; each drives a run, then
        /// quits. focus60 never pauses those runs (SurvivalGame.MayPauseOnFocusLoss reads the NIGHT29 checks and the probe first, as Attach
        /// does). A mode added to Attach adds its flag here (FocusAutomation61Tests scans this file for a mode flag the predicate misses).
        /// NIGHT63 V (R1's N3): read only (an IReadOnlyList over a private array's read-only wrapper), so no caller can change what pauses.</summary>
        public static readonly IReadOnlyList<string> FocusAutomation61Modes = Array.AsReadOnly(new[] { "-skygrindSmoke", "-skygrindShots", "-skygrindReel", "-skygrindTile", "-skygrindStrip", "-skygrindHitch", "-skygrindGolden", "-skygrindFingerprint", "-skygrindFarm", "-skygrindReplay", "-skygrindBench" });

        public static void Attach(SurvivalGame game)
        {
            string[] args = Environment.GetCommandLineArgs();
            // NIGHT8 U2-14: a Web build has no command line: the same checks by URL (?skygrindSmoke, ?skygrindBench=30).
            string url = Application.absoluteURL ?? "";
            if (Array.IndexOf(args, "-skygrindOff19") >= 0) N19Blocks.ForceOff = N20Blocks.ForceOff = N21Blocks.ForceOff = N22Blocks.ForceOff = N24Blocks.ForceOff = N25Blocks.ForceOff = N26Blocks.ForceOff = N27Blocks.ForceOff = true; // NIGHT19: every NIGHT19 (and NIGHT20) design block off for every run (NIGHT18's game)
            if (Array.IndexOf(args, "-skygrindOff20") >= 0) N20Blocks.ForceOff = N21Blocks.ForceOff = N22Blocks.ForceOff = N24Blocks.ForceOff = N25Blocks.ForceOff = N26Blocks.ForceOff = N27Blocks.ForceOff = true; // NIGHT20: every NIGHT20 design block off for every run (NIGHT19's game, the A side)
            if (Array.IndexOf(args, "-skygrindOff21") >= 0) N21Blocks.ForceOff = N22Blocks.ForceOff = N24Blocks.ForceOff = N25Blocks.ForceOff = N26Blocks.ForceOff = N27Blocks.ForceOff = true; // NIGHT21: every NIGHT21 design block off for every run (NIGHT20's game, the A side)
            if (Array.IndexOf(args, "-skygrindOff22") >= 0) N22Blocks.ForceOff = N24Blocks.ForceOff = N25Blocks.ForceOff = N26Blocks.ForceOff = N27Blocks.ForceOff = true; // NIGHT22: every NIGHT22 design block off for every run (NIGHT21's game, the A side)
            if (Array.IndexOf(args, "-skygrindOff24") >= 0) N24Blocks.ForceOff = N25Blocks.ForceOff = N26Blocks.ForceOff = N27Blocks.ForceOff = true; // NIGHT24: every NIGHT24 design block off for every run (NIGHT22's game, the A side)
            if (Array.IndexOf(args, "-skygrindOff27") >= 0) N27Blocks.ForceOff = true; // NIGHT27: every NIGHT27 design block off for every run (NIGHT26's game, the A side)
            for (int o29 = 19; o29 <= 29; o29++) if (Array.IndexOf(args, "-skygrindOff" + o29) >= 0) N29Blocks.ForceOff = true; // NIGHT29: -skygrindOff29 and every older switch turn the NIGHT29 blocks off
            for (int o30 = 19; o30 <= 30; o30++) if (Array.IndexOf(args, "-skygrindOff" + o30) >= 0) N30Blocks.ForceOff = true; // NIGHT30: -skygrindOff30 and every older switch turn the NIGHT30 blocks off
            for (int o31 = 19; o31 <= 31; o31++) if (Array.IndexOf(args, "-skygrindOff" + o31) >= 0) N31Blocks.ForceOff = true; // NIGHT31: -skygrindOff31 and every older switch turn the NIGHT31 blocks off
            for (int o32 = 19; o32 <= 32; o32++) if (Array.IndexOf(args, "-skygrindOff" + o32) >= 0) N32Blocks.ForceOff = true; // NIGHT32: -skygrindOff32 and every older switch turn the NIGHT32 blocks off
            for (int o33 = 19; o33 <= 33; o33++) if (Array.IndexOf(args, "-skygrindOff" + o33) >= 0) N33Blocks.ForceOff = true; // NIGHT33: -skygrindOff33 and every older switch turn the NIGHT33 blocks off
            for (int o34 = 19; o34 <= 34; o34++) if (Array.IndexOf(args, "-skygrindOff" + o34) >= 0) N34Blocks.ForceOff = true; // NIGHT34: -skygrindOff34 and every older switch turn the NIGHT34 blocks off
            for (int o35 = 19; o35 <= 35; o35++) if (Array.IndexOf(args, "-skygrindOff" + o35) >= 0) N35Blocks.ForceOff = true; // NIGHT35: -skygrindOff35 and every older switch turn the NIGHT35 blocks off
            for (int o36 = 19; o36 <= 36; o36++) if (Array.IndexOf(args, "-skygrindOff" + o36) >= 0) N36Blocks.ForceOff = true; // NIGHT36: -skygrindOff36 and every older switch turn the NIGHT36 blocks off
            for (int o37 = 19; o37 <= 37; o37++) if (Array.IndexOf(args, "-skygrindOff" + o37) >= 0) N37Blocks.ForceOff = true; // NIGHT37: -skygrindOff37 and every older switch turn the NIGHT37 blocks off
            for (int o38 = 19; o38 <= 38; o38++) if (Array.IndexOf(args, "-skygrindOff" + o38) >= 0) N38Blocks.ForceOff = true; // NIGHT38: -skygrindOff38 and every older switch turn the NIGHT38 blocks off
            for (int o39 = 19; o39 <= 39; o39++) if (Array.IndexOf(args, "-skygrindOff" + o39) >= 0) N39Blocks.ForceOff = true; // NIGHT39: -skygrindOff39 and every older switch turn the NIGHT39 blocks off
            for (int o40 = 19; o40 <= 40; o40++) if (Array.IndexOf(args, "-skygrindOff" + o40) >= 0) N40Blocks.ForceOff = true; // NIGHT40: -skygrindOff40 and every older switch turn the NIGHT40 blocks off
            for (int o41 = 19; o41 <= 41; o41++) if (Array.IndexOf(args, "-skygrindOff" + o41) >= 0) N41Blocks.ForceOff = true; // NIGHT41: -skygrindOff41 and every older switch turn the NIGHT41 blocks off
            for (int o42 = 19; o42 <= 42; o42++) if (Array.IndexOf(args, "-skygrindOff" + o42) >= 0) N42Blocks.ForceOff = true; // NIGHT42: -skygrindOff42 and every older switch turn the NIGHT42 blocks off
            for (int o45 = 19; o45 <= 45; o45++) if (Array.IndexOf(args, "-skygrindOff" + o45) >= 0) N45Levels.ForceOff = true; // NIGHT45: -skygrindOff45 and every older switch: NIGHT44's level order
            for (int o43 = 19; o43 <= 43; o43++) if (Array.IndexOf(args, "-skygrindOff" + o43) >= 0) N43Blocks.ForceOff = true; // NIGHT43: -skygrindOff43 and every older switch turn the NIGHT43 blocks off
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "-skygrindOff"); // NIGHT46 rows: -skygrindOffNN turns off every row of night NN and later (every older switch: all of them)
            if (Array.IndexOf(args, "-skygrindOff26") >= 0) N26Blocks.ForceOff = N27Blocks.ForceOff = true; // NIGHT26: every NIGHT26 design block off for every run (NIGHT25's game, the A side)
            if (Array.IndexOf(args, "-skygrindOff25") >= 0) N25Blocks.ForceOff = N26Blocks.ForceOff = N27Blocks.ForceOff = true; // NIGHT25: every NIGHT25 design block off for every run (NIGHT24's game, the A side)
            if (DoorFree29Check.Requested) { DoorFree29Check.Attach(game); return; } // NIGHT29 item 14
            if (LevelSelect29Check.Requested) { LevelSelect29Check.Attach(game); return; } // NIGHT29 item 1: the real-reload level-select check
            if (FocusProbe60.Requested(args)) { FocusProbe60.Attach(game, args); return; } // NIGHT60 L B9: the focus-pause probe outside the smoke (FocusProbe60.cs)
            int i = Array.IndexOf(args, "-skygrindSmoke");
            if (i >= 0 || url.Contains("skygrindSmoke"))
            {
                game.gameObject.AddComponent<PlayerChecks>().mode = "smoke";
                // NIGHT15: -skygrindPadShot <png>: the pad-only step holds the wheel open 2 s and captures it once.
                int ps = Array.IndexOf(args, "-skygrindPadShot");
                if (ps >= 0 && ps + 1 < args.Length)
                {
                    SliceSmoke.PadOnlyLinger = 2;
                    SliceSmoke.PadOnlyShot = args[ps + 1];
                }
                return;
            }
            // NIGHT15 (rule 7): -skygrindShots <dir> [-skygrindMap <id>]: the capture tour (ShotTour), then quit.
            i = Array.IndexOf(args, "-skygrindShots");
            if (i >= 0)
            {
                var c = game.gameObject.AddComponent<PlayerChecks>();
                c.mode = "shots";
                c.shotsDir = i + 1 < args.Length ? args[i + 1] : "shots";
                ShotTour.UiPass = Array.IndexOf(args, "-skygrindShotsUi") >= 0; // NIGHT16: the screens instead of the title / map views
                return;
            }
            // NIGHT17 U3-30.3: -skygrindReel <dir> (an absolute dir: a relative one lands under SKYGRIND_Data): the feel reel,
            // FeelStrip's reel scenarios then ShotTour's screens pass, one process, then quit. Not automation (devMode off).
            i = Array.IndexOf(args, "-skygrindReel");
            if (i >= 0)
            {
                var c = game.gameObject.AddComponent<PlayerChecks>();
                c.mode = "reel";
                c.shotsDir = i + 1 < args.Length && !args[i + 1].StartsWith("-", StringComparison.Ordinal) ? args[i + 1] : "reel";
                return;
            }
            // NIGHT17 U3-30.2: -skygrindTile <dir>: the style tile (StyleTile: two directions of the identity), then quit.
            i = Array.IndexOf(args, "-skygrindTile");
            if (i >= 0)
            {
                var c = game.gameObject.AddComponent<PlayerChecks>();
                c.mode = "tile";
                c.shotsDir = i + 1 < args.Length && !args[i + 1].StartsWith("-", StringComparison.Ordinal) ? args[i + 1] : "tile";
                return;
            }
            // NIGHT16 U3-29.3: -skygrindStrip <dir> [scenario]: the strip tool's contact sheets (FeelStrip, builder U), then quit.
            i = Array.IndexOf(args, "-skygrindStrip");
            if (i >= 0)
            {
                var c = game.gameObject.AddComponent<PlayerChecks>();
                c.mode = "strip";
                c.shotsDir = i + 1 < args.Length && !args[i + 1].StartsWith("-", StringComparison.Ordinal) ? args[i + 1] : "strips";
                c.stripScenario = i + 2 < args.Length && !args[i + 2].StartsWith("-", StringComparison.Ordinal) ? args[i + 2] : null;
                return;
            }
            // NIGHT24 (builder F, the lead's switch-lag row): -skygrindHitch: the screen-switch hitch tour (HitchTour24), cold then warm, then quit.
            if (Array.IndexOf(args, "-skygrindHitch") >= 0)
            {
                game.gameObject.AddComponent<PlayerChecks>().mode = "hitch";
                return;
            }
            if (Array.IndexOf(args, "-skygrindGolden") >= 0)
            {
                game.gameObject.AddComponent<PlayerChecks>().mode = "golden";
                return;
            }
            if (Array.IndexOf(args, "-skygrindFingerprint") >= 0)
            {
                game.gameObject.AddComponent<PlayerChecks>().mode = "fingerprint";
                return;
            }
            // NIGHT14 (the shared types): -skygrindFarm <runs> <difficulty> <seedStart> [out.jsonl] (U3-27.6, agent A's Farm.Run)
            // and -skygrindReplay <tape> (U3-27.8, agent T's TapeReplay.ReplayFile); both on fresh headless compositions.
            i = Array.IndexOf(args, "-skygrindFarm");
            if (i >= 0)
            {
                var c = game.gameObject.AddComponent<PlayerChecks>();
                c.mode = "farm";
                c.farmRuns = i + 1 < args.Length && int.TryParse(args[i + 1], out int runs) ? runs : 10;
                c.farmDifficulty = i + 2 < args.Length && !args[i + 2].StartsWith("-") ? args[i + 2] : "normal";
                c.farmSeed = i + 3 < args.Length && uint.TryParse(args[i + 3], out uint seed0) ? seed0 : 1;
                c.farmOut = i + 4 < args.Length && !args[i + 4].StartsWith("-") ? args[i + 4] : null;
                int lf = Array.IndexOf(args, "-logFile");
                if (lf >= 0 && lf + 1 < args.Length) c.csvBase = args[lf + 1];
                return;
            }
            i = Array.IndexOf(args, "-skygrindReplay");
            if (i >= 0)
            {
                var c = game.gameObject.AddComponent<PlayerChecks>();
                c.mode = "replay";
                c.replayTape = i + 1 < args.Length ? args[i + 1] : null;
                return;
            }
            i = Array.IndexOf(args, "-skygrindBench");
            int u = url.IndexOf("skygrindBench", StringComparison.Ordinal);
            if (i >= 0 || u >= 0)
            {
                var c = game.gameObject.AddComponent<PlayerChecks>();
                c.mode = "bench";
                if (i >= 0 && i + 1 < args.Length && double.TryParse(args[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out double s)) c.seconds = s;
                else if (u >= 0)
                {
                    string rest = url.Substring(u + "skygrindBench".Length).TrimStart('=');
                    int end = 0;
                    while (end < rest.Length && char.IsDigit(rest[end])) end++;
                    if (end > 0 && double.TryParse(rest.Substring(0, end), out double us)) c.seconds = us;
                }
                c.play = i >= 0 && i + 2 < args.Length && args[i + 2] == "play";
                c.autopilot = i >= 0 && i + 2 < args.Length && args[i + 2] == "autopilot"; // NIGHT14 U3-27.12
                int cap = Array.IndexOf(args, "-skygrindFpsCap"); // NIGHT13: a pacing arm (the FPS CAP row's targetFrameRate)
                if (cap >= 0 && cap + 1 < args.Length && int.TryParse(args[cap + 1], out int fps)) c.fpsCap = fps;
                // NIGHT71 F1 (T-152a): -skygrindVsync [0|1]: the VSYNC arm (a bench switch, not a game row), applied over the profile's saved
                // VSYNC (move54 makes VSYNC the default for a profile that never saved one, so without the arm the bench reads whoever ran it).
                // 1 (or no number): present on the display's refresh; 0: unsynced, uncapped unless -skygrindFpsCap.
                int vs = Array.IndexOf(args, "-skygrindVsync");
                if (vs >= 0) c.vsyncArm = vs + 1 < args.Length && int.TryParse(args[vs + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int vsv) ? (vsv > 0 ? 1 : 0) : 1;
                c.loopSplit = !NightRows.HasSwitch(args, BenchLoop71.NoSwitch); // NIGHT71 F1: the player-loop split (BenchLoop71.cs), on unless -skygrindNoLoopSplit71
                int l = Array.IndexOf(args, "-logFile");
                if (l >= 0 && l + 1 < args.Length) c.csvBase = args[l + 1];
                // NIGHT11 U3-24.2: -skygrindProfile <frames> <file.raw>: a Development Player also writes the Profiler's
                // binary log for that many frames from the bench's middle (the Editor reads it: GC.Alloc per marker).
                int p = Array.IndexOf(args, "-skygrindProfile");
                if (p >= 0 && p + 2 < args.Length && int.TryParse(args[p + 1], out int pf))
                {
                    c.profileFrames = pf;
                    c.profileFile = args[p + 2];
                }
            }
        }

        string mode;
        int farmRuns; // NIGHT14 U3-27.6
        string farmDifficulty, farmOut;
        uint farmSeed;
        string replayTape; // NIGHT14 U3-27.8
        string shotsDir; // NIGHT15: the capture tour's folder (NIGHT16: also the strips')
        string stripScenario; // NIGHT16: -skygrindStrip's scenario (null: all)
        double seconds = 60;
        bool play; // NIGHT13: the playing scenario
        bool autopilot; // NIGHT14 U3-27.12: the AUTOPILOT fights (real fights instead of the scripted strafe)
        int fpsCap; // NIGHT13: a pacing arm (0: uncapped, as the bench always ran)
        int vsyncArm = -1; // NIGHT71 F1: -skygrindVsync's arm (-1: not given, the profile's saved VSYNC as before; 0 off; 1 on)
        bool loopSplit; // NIGHT71 F1: the player-loop split (BenchLoop71)
        string csvBase; // NIGHT13: the -logFile path; the CSVs go beside it
        int profileFrames; // NIGHT11 U3-24.2 (0: no profiler log)
        string profileFile;

        IEnumerator Start()
        {
            yield return null;
            SurvivalGame g = SurvivalGame.Instance;
            Debug.Log($"[skygrind] check '{mode}' starting; {Screen.width}x{Screen.height}, {SystemInfo.graphicsDeviceName}, {SystemInfo.processorType}");
            if (mode == "smoke")
            {
                SliceSmoke.Begin();
                while (!SliceSmoke.Done) yield return null;
                Debug.Log("[skygrind] smoke report\n" + SliceSmoke.Report);
            }
            else if (mode == "golden")
            {
                // NIGHT9 U3-04: the oracle inside the built Player: every vector set (Skygrind.Core's Golden checkers,
                // the same code the EditMode tests run) and every trajectory replay, on StreamingAssets' copy.
                string root = System.IO.Path.Combine(Application.streamingAssetsPath, "PortExport");
                float t0 = Time.realtimeSinceStartup;
                int bad = Golden.RunAll(new GoldenData(root), line => Debug.Log("[golden] " + line));
                Debug.Log($"[golden] {(bad == 0 ? "DONE: 0 mismatches" : $"DONE: {bad} failing")} in {Time.realtimeSinceStartup - t0:0.0} s ({root}; {Application.platform}, {SystemInfo.processorType})");
            }
            else if (mode == "fingerprint") Fingerprint(g);
            else if (mode == "farm")
            {
                // NIGHT14 U3-27.6: one JSONL line per run beside the -logFile (or the given path).
                string outPath = farmOut ?? Path.Combine(Path.GetDirectoryName(Path.GetFullPath(csvBase ?? Path.Combine(Application.persistentDataPath, "farm.log"))), $"farm-{farmDifficulty}-{farmSeed}.jsonl");
                Farm.BuildStamp = BuildStamp.Line; // each JSONL line carries the build it ran on
                string summary = Farm.Run(g.cfg, g.map, farmRuns, farmDifficulty, farmSeed, outPath, line => Debug.Log("[farm] " + line));
                Debug.Log("[farm] " + summary);
            }
            else if (mode == "replay") Debug.Log("[replay] " + (replayTape == null ? "REFUSED: no tape path" : TapeReplay.ReplayFile(g.cfg, g.map, replayTape)));
            else if (mode == "bench")
            {
                yield return Bench(g);
            }
            else if (mode == "shots") yield return ShotTour.Run(g, shotsDir, new List<string>()); // NIGHT15
            else if (mode == "strip") yield return FeelStrip.Run(g, shotsDir, stripScenario, new List<string>()); // NIGHT16
            else if (mode == "tile") yield return StyleTile.Run(g, shotsDir, new List<string>()); // NIGHT17 U3-30.2
            else if (mode == "hitch") yield return HitchTour24.Run(g, new List<string>()); // NIGHT24: the switch-lag tour
            else if (mode == "reel")
            {
                // NIGHT18: the measured reel (FeelReel: every frame as a JPG + telemetry.jsonl per scenario; Tools/feel-schema.md).
                // NIGHT17's strips and screens stay on -skygrindStrip / -skygrindShots.
                string[] a = Environment.GetCommandLineArgs();
                int oi = Array.IndexOf(a, "-skygrindReelOnly");
                if (oi >= 0 && oi + 1 < a.Length) FeelReel.Only = a[oi + 1].Split(',');
                FeelReel.PadPass = Array.IndexOf(a, "-skygrindReelPad") >= 0; // NIGHT19: the pad pass (S12)
                yield return FeelReel.Run(g, shotsDir, new List<string>());
            }
            Application.Quit();
        }

        /// <summary>NIGHT13 U3-26.2: the baseline seed's hashes (compared with FingerprintBaseline), then another seed's.</summary>
        static void Fingerprint(SurvivalGame g)
        {
            float t0 = Time.realtimeSinceStartup;
            List<string> lines = g.RunFingerprint(SimFingerprint.Seed);
            float t1 = Time.realtimeSinceStartup;
            foreach (string l in lines) Debug.Log($"[fingerprint] seed {SimFingerprint.Seed} {l}");
            int diff = FingerprintBaseline.FirstDifference(lines);
            string verdict = FingerprintBaseline.Hashes.Length == 0 ? "no baseline recorded" : diff < 0 ? "EQUAL to the baseline" : $"DIFFERENT from the baseline at checkpoint {diff + 1} ({lines[Math.Min(diff, lines.Count - 1)]} vs {FingerprintBaseline.Hashes[Math.Min(diff, FingerprintBaseline.Hashes.Length - 1)]})";
            Debug.Log($"[fingerprint] seed {SimFingerprint.Seed}: {lines.Count} hashes over {SimFingerprint.Steps} steps in {t1 - t0:0.00} s: {verdict}; at the end: {g.FingerprintSummary}");
            // NIGHT19 (rule 3): the off path, every NIGHT19 design block off, against NIGHT18's baseline (FingerprintBaselineN18).
            // Until the close's re-record (FingerprintBaseline.N19OnPath false) the run above already is the off path.
            List<string> off = lines;
            if (FingerprintBaseline.N19OnPath)
            {
                N19Blocks? saved = g.sim.n19Override;
                N20Blocks? saved20 = g.sim.n20Override;
                g.sim.n19Override = N19Blocks.Off;
                g.sim.n20Override = N20Blocks.Off;
                N21Blocks? saved21 = g.sim.n21Override; // NIGHT21: and every NIGHT21 block off (NIGHT18's game)
                g.sim.n21Override = N21Blocks.Off;
                try { off = g.RunFingerprint(SimFingerprint.Seed); }
                finally
                {
                    g.sim.n19Override = saved;
                    g.sim.n20Override = saved20;
                    g.sim.n21Override = saved21;
                }
            }
            int offDiff = FingerprintBaselineN18.FirstDifference(off);
            Debug.Log($"[fingerprint] seed {SimFingerprint.Seed} off path (every NIGHT19 block off) vs FingerprintBaselineN18: {(offDiff < 0 ? "EQUAL" : $"DIFFERENT at checkpoint {offDiff + 1}")}");
            // NIGHT21 (rule 3): the NIGHT21 off path, only the NIGHT21 blocks off, against NIGHT20's baseline (FingerprintBaselineN20).
            N21Blocks? saved21b = g.sim.n21Override;
            g.sim.n21Override = N21Blocks.Off;
            List<string> off21;
            try { off21 = g.RunFingerprint(SimFingerprint.Seed); }
            finally { g.sim.n21Override = saved21b; }
            int off21Diff = FingerprintBaselineN20.FirstDifference(off21);
            Debug.Log($"[fingerprint] seed {SimFingerprint.Seed} off path (every NIGHT21 block off) vs FingerprintBaselineN20: {(off21Diff < 0 ? "EQUAL" : $"DIFFERENT at checkpoint {off21Diff + 1}")}");
            List<string> red = g.RunFingerprint(SimFingerprint.RedSeed);
            int same = 0;
            for (int i = 0; i < red.Count && i < lines.Count; i++) if (red[i] == lines[i]) same++;
            Debug.Log($"[fingerprint] seed {SimFingerprint.RedSeed} (red): last {red[red.Count - 1]}; {same} of {red.Count} checkpoints equal to seed {SimFingerprint.Seed}'s");
        }

        // --- the bench ---------------------------------------------------------------------------------------------------

        const int Cap = 64 * 1024; // frames (60 s at ~370 fps is ~22,000)
        const double Over = 6.94; // ms: the count of frames above this (144 Hz's frame; NIGHT13 U3-26.2)

        IEnumerator Bench(SurvivalGame g)
        {
            g.StartRun(99);
            g.health.max = g.health.hp = 1e9;
            g.zones.OpenAll(); // the whole map: the fight spreads like a late run
            if (vsyncArm >= 0) SurvivalGame.ApplyFrameCap(fpsCap, vsyncArm > 0); // NIGHT71 F1: the VSYNC arm over the saved setting (with VSYNC on the cap is ignored: ApplyFrameCap's rule)
            else if (fpsCap > 0) SurvivalGame.ApplyFrameCap(fpsCap, false); // after the settings applied theirs at boot
            {
                double hz = Screen.currentResolution.refreshRateRatio.value;
                Debug.Log($"[skygrind] bench vsync: {(vsyncArm < 0 ? "no arm (the profile's saved VSYNC)" : "arm -skygrindVsync " + vsyncArm)}; vsync {QualitySettings.vSyncCount}, cap {Application.targetFrameRate}; display {hz:0.##} Hz ({(hz > 0 ? 1000 / hz : 0):0.00} ms a refresh), {Screen.fullScreenMode} {Screen.width}x{Screen.height}");
            }
            int target = (int)g.cfg.Num("director.maxAlive");
            var spots = new List<SpawnNode>();
            var high = new List<SpawnNode>();
            g.zones.SpawnPoints(spots, high, g.core);
            int k = 0;
            for (int tries = 0; g.husks.Count < target && tries < 400; tries++)
            {
                SpawnNode node = spots[(k++ * 7919) % spots.Count];
                Husk h = g.husks.Spawn(new Vec3(node.x, node.y, node.z), g.mover.position);
                if (h != null) g.brain.Attach(h);
            }
            // NIGHT14 U3-27.12: the autopilot scenario: the AUTOPILOT (BotBrain on the player's body, normal) plays the fight;
            // god mode as the bench always had (the hp topped up each frame); the meta silent; the camera follows its look.
            AutopilotInput pilot = null;
            if (autopilot)
            {
                g.sim.metaSilent = true;
                pilot = new AutopilotInput(g.sim, "normal", 99);
                g.sim.UseInput(pilot);
            }
            Debug.Log($"[skygrind] bench: {g.husks.Count} husks alive, {seconds} s, scenario {(autopilot ? "autopilot (the bot fights)" : play ? "play (strafe, aim, fire)" : "ui-hurt (stand, hurt every frame)")}");
            // Per frame (preallocated: the bench allocates nothing per frame).
            var interval = new float[Cap];
            var steps = new byte[Cap];
            var stepMs = new float[Cap];
            var updateMs = new float[Cap];
            var gcCount = new int[Cap];
            var heap = new long[Cap];
            // FrameTiming (it lags a few frames; its own frames, deduplicated by their start timestamp).
            var ftCpu = new float[Cap];
            var ftMain = new float[Cap];
            var ftRender = new float[Cap];
            var ftPresent = new float[Cap];
            var ftGpu = new float[Cap];
            int ftN = 0;
            ulong lastFtStart = 0;
            var timings = new FrameTiming[1];
            using var gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            using var gcUsed = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
            using var simRec = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, "SKYGRIND.SimStep");
            long allocSum = 0, usedRise = 0, lastUsed = -1;
            double simSum = 0;
            int gc0 = GC.CollectionCount(0), n = 0, profiled = -1, playStep = 0;
            // NIGHT71 F1: the player-loop split (BenchLoop71.cs, measurement only), recording from the next frame boundary.
            if (loopSplit && BenchLoop71.Install(g.events, seconds)) BenchLoop71.Begin();
            else Debug.Log($"[skygrind] bench loop: not split ({(loopSplit ? "the player loop has no phases" : BenchLoop71.NoSwitch)})");
            float t0 = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - t0 < seconds)
            {
                // NIGHT11 U3-24.2: the profiler's window, from the bench's middle (a Development Player only).
                if (profileFrames > 0 && profiled < 0 && Time.realtimeSinceStartup - t0 >= seconds / 2 && UnityEngine.Profiling.Profiler.supported)
                {
                    UnityEngine.Profiling.Profiler.logFile = profileFile;
                    UnityEngine.Profiling.Profiler.enableBinaryLog = true;
                    UnityEngine.Profiling.Profiler.enabled = true;
                    profiled = 0;
                    Debug.Log($"[skygrind] bench profile: logging {profileFrames} frames to {profileFile}");
                }
                else if (profiled >= 0 && profiled < profileFrames && ++profiled == profileFrames)
                {
                    UnityEngine.Profiling.Profiler.enabled = false;
                    UnityEngine.Profiling.Profiler.enableBinaryLog = false;
                    UnityEngine.Profiling.Profiler.logFile = "";
                    Debug.Log($"[skygrind] bench profile: {profileFrames} frames logged");
                }
                if (n < Cap)
                {
                    interval[n] = Time.unscaledDeltaTime * 1000;
                    steps[n] = (byte)Math.Min(255, g.StepsThisFrame);
                    stepMs[n] = (float)g.StepMsThisFrame;
                    updateMs[n] = (float)g.UpdateMsThisFrame;
                    gcCount[n] = GC.CollectionCount(0);
                    heap[n] = gcUsed.Valid ? gcUsed.LastValue : 0;
                    n++;
                }
                FrameTimingManager.CaptureFrameTimings();
                if (FrameTimingManager.GetLatestTimings(1, timings) > 0 && timings[0].frameStartTimestamp != lastFtStart && ftN < Cap)
                {
                    FrameTiming t = timings[0];
                    lastFtStart = t.frameStartTimestamp;
                    ftCpu[ftN] = (float)t.cpuFrameTime;
                    ftMain[ftN] = (float)t.cpuMainThreadFrameTime;
                    ftRender[ftN] = (float)t.cpuRenderThreadFrameTime;
                    ftPresent[ftN] = (float)t.cpuMainThreadPresentWaitTime;
                    ftGpu[ftN] = (float)t.gpuFrameTime;
                    ftN++;
                }
                if (gcAlloc.Valid) allocSum += gcAlloc.LastValue;
                if (simRec.Valid) simSum += simRec.LastValue / 1e6;
                if (gcUsed.Valid)
                {
                    long used = gcUsed.LastValue;
                    if (lastUsed >= 0 && used > lastUsed) usedRise += used - lastUsed;
                    lastUsed = used;
                }
                if (autopilot)
                {
                    g.input.SetLook(pilot.Yaw, pilot.Pitch); // the camera reads the devices' look: it follows the bot's
                    if (g.husks.Count < target)
                    {
                        SpawnNode sn = spots[(k++ * 7919) % spots.Count];
                        Husk h = g.husks.Spawn(new Vec3(sn.x, sn.y, sn.z), g.mover.position);
                        if (h != null) g.brain.Attach(h);
                    }
                }
                else if (play)
                {
                    // The scripted player (SimFingerprint.Script's input, by frame): strafe, aim at the nearest husk,
                    // fire. Keep the count up: one husk back per frame while short.
                    double yaw = g.input.yaw, pitch = g.input.pitch;
                    SimFingerprint.Script(playStep, g.Eye, g.enemies, ref yaw, ref pitch, out InputFrame move, out CombatInput combat);
                    playStep += Math.Max(1, g.StepsThisFrame);
                    move.jump = move.dash = move.slide = default; // stay on the floor near the fight
                    combat.interact = default;
                    combat.weapon = -1;
                    g.input.SetLook(yaw, pitch);
                    g.forcedMove = move;
                    g.forcedCombat = combat;
                    if (g.husks.Count < target)
                    {
                        SpawnNode sn = spots[(k++ * 7919) % spots.Count];
                        Husk h = g.husks.Spawn(new Vec3(sn.x, sn.y, sn.z), g.mover.position);
                        if (h != null) g.brain.Attach(h);
                    }
                }
                g.health.hp = g.health.max;
                yield return null;
            }
            BenchLoop71.End(); // NIGHT71 F1
            g.forcedMove = null;
            g.forcedCombat = null;
            if (pilot != null)
            {
                g.sim.UseInput(g.playerInput);
                pilot.Detach(); // the run rules it set (metaSilent, noLab, the aim assist) back
            }
            int collections = GC.CollectionCount(0) - gc0;
            Report(g, interval, steps, stepMs, updateMs, gcCount, heap, n, ftCpu, ftMain, ftRender, ftPresent, ftGpu, ftN, collections, usedRise, allocSum, simSum, gcAlloc.Valid, gcUsed.Valid, simRec.Valid);
            BenchLoop71.Report(s => Debug.Log(s), csvBase); // NIGHT71 F1: the worst 1%'s source by player-loop slot
            BenchLoop71.Uninstall();
        }

        static double Mean(float[] a, int[] idx, int from, int to)
        {
            if (to <= from) return 0;
            double s = 0;
            for (int i = from; i < to; i++) s += a[idx[i]];
            return s / (to - from);
        }

        static int[] Order(float[] key, int n)
        {
            var idx = new int[n];
            for (int i = 0; i < n; i++) idx[i] = i;
            Array.Sort(idx, (x, y) => key[x].CompareTo(key[y]));
            return idx;
        }

        void Report(SurvivalGame g, float[] interval, byte[] steps, float[] stepMs, float[] updateMs, int[] gcCount, long[] heap, int n,
            float[] ftCpu, float[] ftMain, float[] ftRender, float[] ftPresent, float[] ftGpu, int ftN, int collections, long usedRise, long allocSum, double simSum,
            bool allocValid, bool usedValid, bool simValid)
        {
            const int warm = 30; // the first frames warm up
            int m = Math.Max(0, n - warm);
            var iv = new float[m];
            Array.Copy(interval, warm, iv, 0, m);
            var sorted = (float[])iv.Clone();
            Array.Sort(sorted);
            float Q(float q) => m == 0 ? 0 : sorted[Math.Min(m - 1, (int)(m * q))];
            float avg = 0;
            int over = 0;
            foreach (float f in sorted)
            {
                avg += f;
                if (f > Over) over++;
            }
            avg /= Math.Max(1, m);
            // The old lines first, unchanged (comparable with every earlier bench log).
            Debug.Log($"[skygrind] bench result: {m} frames, alive at end {g.husks.Count}, median {Q(0.5f):0.00} ms, p95 {Q(0.95f):0.00} ms, worst 1% {Q(0.99f):0.00} ms, mean fps {1000 / avg:0}, median fps {1000 / Math.Max(0.01f, Q(0.5f)):0}, vsync {QualitySettings.vSyncCount}, cap {Application.targetFrameRate}");
            Debug.Log($"[skygrind] bench gc: collections {collections} in {n} frames; GC Used Memory rises {usedRise / 1024.0:0.0} KB ({(usedValid ? "valid" : "n/a")}); GC Allocated In Frame {(allocValid ? $"{allocSum / 1024.0:0.0} KB total, {allocSum / Math.Max(1.0, n):0} B / frame" : "n/a (release Player)")}");
            var ftSorted = new float[ftN];
            Array.Copy(ftCpu, ftSorted, ftN);
            Array.Sort(ftSorted);
            var gpuSorted = new float[ftN];
            Array.Copy(ftGpu, gpuSorted, ftN);
            Array.Sort(gpuSorted);
            float FQ(float[] s, float q) => s.Length == 0 ? 0 : s[Math.Min(s.Length - 1, (int)(s.Length * q))];
            Debug.Log($"[skygrind] bench timing: FrameTimingManager cpu median {FQ(ftSorted, 0.5f):0.00} ms, worst 1% {FQ(ftSorted, 0.99f):0.00} ms; gpu median {FQ(gpuSorted, 0.5f):0.00} ms, worst 1% {FQ(gpuSorted, 0.99f):0.00} ms ({ftN} samples); sim steps {(simValid ? $"{simSum:0.0} ms total" : "n/a (release Player)")}");
            // NIGHT13 U3-26.2: the tail.
            Debug.Log($"[skygrind] bench tail: p99 {Q(0.99f):0.00} ms, p99.9 {Q(0.999f):0.00} ms, max {(m > 0 ? sorted[m - 1] : 0):0.00} ms, frames over {Over:0.00} ms {over} of {m} ({100.0 * over / Math.Max(1, m):0.00}%)");
            {
                // NIGHT71 F1 (T-152a): the chop by the display's own refresh, on both arms (under VSYNC the 6.94 ms count reads the panel's
                // period, not a stutter): a frame over BenchMissedRefresh71 refresh periods missed a refresh.
                double hz = Screen.currentResolution.refreshRateRatio.value;
                double period = hz > 0 ? 1000 / hz : 0, lim = period * UnityTuning.BenchMissedRefresh71;
                int missed = 0;
                if (period > 0) foreach (float f in sorted) if (f > lim) missed++;
                Debug.Log($"[skygrind] bench refresh: display {hz:0.##} Hz ({period:0.00} ms), frames over {UnityTuning.BenchMissedRefresh71:0.0} refreshes ({lim:0.00} ms) {missed} of {m} ({100.0 * missed / Math.Max(1, m):0.00}%); vsync {QualitySettings.vSyncCount}, cap {Application.targetFrameRate}");
            }
            {
                // NIGHT13: FrameTiming's p99s (capped, the interval reads the cap; these read the CPU's busy time).
                var mainS = new float[ftN];
                var busyS = new float[ftN];
                var rendS = new float[ftN];
                for (int i2 = 0; i2 < ftN; i2++)
                {
                    mainS[i2] = ftMain[i2];
                    busyS[i2] = ftMain[i2] - ftPresent[i2];
                    rendS[i2] = ftRender[i2];
                }
                Array.Sort(mainS);
                Array.Sort(busyS);
                Array.Sort(rendS);
                Debug.Log($"[skygrind] bench cpu: FrameTiming p99 main thread {FQ(mainS, 0.99f):0.00} ms, main minus present wait {FQ(busyS, 0.99f):0.00} ms, render thread {FQ(rendS, 0.99f):0.00} ms; cap {Application.targetFrameRate}, vsync {QualitySettings.vSyncCount}");
            }
            // The breakdown by interval: the worst 1% (rank >= floor(0.99 m)) against the median band (ranks 45%..55%).
            var st = new float[m];
            var sm = new float[m];
            var um = new float[m];
            var gcHit = new float[m];
            var gcNear = new float[m];
            for (int i = 0; i < m; i++)
            {
                int f = i + warm;
                st[i] = steps[f];
                sm[i] = stepMs[f];
                um[i] = updateMs[f];
                gcHit[i] = f > 0 && gcCount[f] != gcCount[f - 1] ? 1 : 0;
                // a collection completed within 60 frames after this one (incremental GC slices run before the count moves)
                int end = Math.Min(n - 1, f + 60);
                gcNear[i] = gcCount[end] != gcCount[f] ? 1 : 0;
            }
            int[] idx = Order(iv, m);
            int w0 = (int)(m * 0.99), b0 = (int)(m * 0.45), b1 = (int)(m * 0.55);
            Debug.Log("[skygrind] bench breakdown (by interval; mean over worst 1% | median band 45-55% | all)");
            void Row(string name, float[] col, string unit) => Debug.Log($"[skygrind] bench breakdown   {name,-26} {Mean(col, idx, w0, m),8:0.000} | {Mean(col, idx, b0, b1),8:0.000} | {Mean(col, idx, 0, m),8:0.000} {unit}");
            Row("interval", iv, "ms");
            Row("sim steps run", st, "steps");
            Row("sim step time", sm, "ms");
            Row("Update (all scripts)", um, "ms");
            var engine = new float[m];
            for (int i = 0; i < m; i++) engine[i] = iv[i] - um[i];
            Row("interval - Update (engine)", engine, "ms");
            Row("GC count moved this frame", gcHit, "share");
            Row("GC completes within 60 fr", gcNear, "share");
            // The breakdown by FrameTiming's own frames (ranked by its cpuFrameTime).
            if (ftN > warm)
            {
                int fm = ftN - warm;
                var cpu = new float[fm];
                var main = new float[fm];
                var render = new float[fm];
                var present = new float[fm];
                var gpu = new float[fm];
                var mainNoWait = new float[fm];
                Array.Copy(ftCpu, warm, cpu, 0, fm);
                Array.Copy(ftMain, warm, main, 0, fm);
                Array.Copy(ftRender, warm, render, 0, fm);
                Array.Copy(ftPresent, warm, present, 0, fm);
                Array.Copy(ftGpu, warm, gpu, 0, fm);
                for (int i = 0; i < fm; i++) mainNoWait[i] = main[i] - present[i];
                int[] fidx = Order(cpu, fm);
                int fw0 = (int)(fm * 0.99), fb0 = (int)(fm * 0.45), fb1 = (int)(fm * 0.55);
                Debug.Log("[skygrind] bench breakdown (FrameTiming, by its cpuFrameTime; mean over worst 1% | median band | all)");
                void FRow(string name, float[] col) => Debug.Log($"[skygrind] bench breakdown   {name,-26} {Mean(col, fidx, fw0, fm),8:0.000} | {Mean(col, fidx, fb0, fb1),8:0.000} | {Mean(col, fidx, 0, fm),8:0.000} ms");
                FRow("cpuFrameTime", cpu);
                FRow("main thread", main);
                FRow("main - present wait", mainNoWait);
                FRow("present wait", present);
                FRow("render thread", render);
                FRow("gpu", gpu);
            }
            // The CSVs beside the log (once, at the end).
            if (!string.IsNullOrEmpty(csvBase))
            {
                try
                {
                    var sb = new StringBuilder(n * 48);
                    sb.Append("frame,interval_ms,steps,step_ms,update_ms,gc_count,gc_used_bytes\n");
                    for (int i = 0; i < n; i++)
                        sb.Append(i).Append(',').Append(interval[i].ToString("0.000", CultureInfo.InvariantCulture)).Append(',').Append(steps[i]).Append(',')
                            .Append(stepMs[i].ToString("0.000", CultureInfo.InvariantCulture)).Append(',').Append(updateMs[i].ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                            .Append(gcCount[i]).Append(',').Append(heap[i]).Append('\n');
                    File.WriteAllText(csvBase + ".frames.csv", sb.ToString());
                    sb.Clear();
                    sb.Append("ft,cpu_ms,main_ms,render_ms,present_wait_ms,gpu_ms\n");
                    for (int i = 0; i < ftN; i++)
                        sb.Append(i).Append(',').Append(ftCpu[i].ToString("0.000", CultureInfo.InvariantCulture)).Append(',').Append(ftMain[i].ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                            .Append(ftRender[i].ToString("0.000", CultureInfo.InvariantCulture)).Append(',').Append(ftPresent[i].ToString("0.000", CultureInfo.InvariantCulture)).Append(',')
                            .Append(ftGpu[i].ToString("0.000", CultureInfo.InvariantCulture)).Append('\n');
                    File.WriteAllText(csvBase + ".ft.csv", sb.ToString());
                    Debug.Log($"[skygrind] bench csv: {csvBase}.frames.csv ({n} rows), {csvBase}.ft.csv ({ftN} rows)");
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[skygrind] bench csv: not written: " + e.Message);
                }
            }
        }
    }
}
