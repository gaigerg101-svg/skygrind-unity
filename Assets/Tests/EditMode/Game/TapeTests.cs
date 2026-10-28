// NIGHT14 U3-27.8 (agent T): THE TAPE (design.tape). A run's inputs and every non-input answer the sim consumed, recorded
// compactly (TapeRecorder), saved and loaded (RunTape), and replayed on a FRESH headless composition (TapeReplay): EQUAL at
// every checkpoint, or the first differing step. Both the recording and the replay use fresh compositions (U3-27.2's
// second-run difference is the lead's, not the tape's).
//   1. the fingerprint's scripted run, taped: its checkpoints are the baseline's (the recorder changes nothing), the tape
//      saved, loaded and replayed: EQUAL (12 of 12 checkpoints, 7200 steps); bytes per minute printed (well under 1 MB)
//   2. every non-input answer the sim reads, exercised and replayed: the assist mode and breakaway cap changing mid-run,
//      lock toggles and MODS drafts between steps, a LAB toggle, a DAILY on a pinned clock, a HEAT rung from the profile,
//      the first-run script; and the onScreen answers (rare: a stuck enemy's relocation) shown to decide the run, replayed
//      from the tape
//   3. the replay is sensitive (seen red): another seed in the header DIFFERS at an early step
//   4. an old-format tape, a foreign file and a truncated tape are REFUSED with a line, never a crash
//   5. an AUTOPILOT run taped and replayed (agent A's AutopilotInput, U3-27.5; on since the NIGHT14 close)
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class TapeTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static string TempTape(string name) => Path.Combine(Path.GetTempPath(), "skygrind-tapetests-" + name + RunTape.Extension);

        [Test]
        public void ScriptedRun_TapedAndReplayedOnAFreshComposition_EqualAtEveryCheckpoint()
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            RunTape tape = TapeScript.Fingerprint(sim, SimFingerprint.Steps, "tests");
            // The recorder only observes: the taped run's checkpoints are the fingerprint baseline's.
            List<string> lines = tape.CheckpointLines();
            Assert.That(FingerprintBaseline.FirstDifference(lines), Is.EqualTo(-1), "the taped run left the baseline:\n" + string.Join("\n", lines));
            Assert.That(tape.steps, Is.EqualTo(SimFingerprint.Steps));
            string path = TempTape("scripted");
            try
            {
                tape.Save(path);
                RunTape loaded = RunTape.Load(path);
                TapeResult r = TapeReplay.Replay(Cfg(), Map(), loaded);
                Assert.That(r.Equal, Is.True, r.Line);
                Assert.That(r.Line, Does.StartWith("EQUAL (12 of 12 checkpoints, 7200 steps)"));
                long bytes = new FileInfo(path).Length;
                double minutes = SimFingerprint.Steps / 120.0 / 60.0;
                TestContext.WriteLine($"tape: {bytes} B for {SimFingerprint.Steps} steps ({minutes:0.##} min): {bytes / minutes:0} B per minute");
                Assert.That(bytes / minutes, Is.LessThan(1024 * 1024), "a tape must stay well under 1 MB per minute");
                Assert.That(TapeReplay.ReplayFile(Cfg(), Map(), path), Does.StartWith("EQUAL (12 of 12 checkpoints, 7200 steps)"));
            }
            finally
            {
                File.Delete(path);
            }
        }

        /// <summary>A composition whose every non-input answer moves: onScreen by the point, the assist mode and cap by the step, a LAB toggle on, a pinned clock.</summary>
        static SurvivalSim Busy(Profile profile, Func<int> stepNow)
        {
            GameConfig cfg = Cfg();
            var store = new SettingsStore(cfg, new List<(SettingItem, object)>(), new List<(string, string)>());
            store.Load("");
            Func<string, bool> settingBool = k => k == LabMath.StyleRamp || (!k.StartsWith("lab.", StringComparison.Ordinal) && store.Bool(k)); // the LAB rows are the Game's settings: STYLE RAMP on, the rest off
            var sim = new SurvivalSim(cfg, Map(), new EventBus(), new Lab(cfg), new ScriptedInput(), profile, store.Num, settingBool, store.Str,
                p => ((long)Math.Floor(p.x * 3 + p.z) & 1) == 0, a => a);
            sim.ToTitle();
            foreach (SurvivalSim.Machine m in sim.machines) sim.RideCore(m);
            sim.assistModeFor = mode => stepNow() < 1500 ? "snap" : stepNow() < 3000 ? "lock" : stepNow() < 5000 ? "assist" : mode;
            sim.breakawayCap = () => stepNow() < 2000 ? double.PositiveInfinity : 4.5;
            sim.utcNow = () => new DateTime(2026, 10, 4, 22, 30, 0, DateTimeKind.Utc);
            return sim;
        }

        static RunTape RecordBusy(int steps)
        {
            int now = 0;
            var profile = new Profile { heat = 2 };
            SurvivalSim sim = Busy(profile, () => now);
            sim.noRecords = true; // judged, never stored (the replay pins the same)
            sim.n24Override = N24Blocks.Off; // NIGHT24 premise: this test needs the draft of NIGHT21 (round 2); the BIG PICK has none before round 5 (Harness fist --bigpick judges it); the tape records the blocks, so the replay is the same game
            sim.setup.mode = RunSetup.Survival; // NIGHT24: this tape was a DAILY (today's seed, its twist and first mod); now a SURVIVAL run on a fixed seed
            sim.setup.seed = 1702702973u; // what the pinned day's DAILY seed was
            sim.setup.heat = 1;
            return TapeScript.Record(sim, null, steps, "tests", (s, i) =>
            {
                now = i;
                if (i % 450 == 225) s.assist.ToggleLock(); // V between steps (HotKeys / the pad's lockOn)
                if (s.flow == Flow.Playing && s.run.state == SurvivalRun.State.Break && s.mods.Pending)
                {
                    List<string> offer = s.mods.Open(); // SurvivalGame.Update's draft, then PickMod from the UI
                    if (offer != null && offer.Count > 0) s.mods.Pick((i / 7) % offer.Count);
                }
            }, TapeScript.God);
        }

        [Test]
        public void EveryNonInputAnswer_RecordedAndReplayed()
        {
            RunTape tape = RecordBusy(SimFingerprint.Steps);
            Assert.That(tape.mode, Is.EqualTo(RunSetup.Survival));
            Assert.That(tape.seed, Is.EqualTo(1702702973u), "the fixed seed the setup gave");
            Assert.That(tape.lab[2], Is.True, "STYLE RAMP (the LAB toggle) is in the header");
            Assert.That(tape.heat, Is.EqualTo(1), "the HEAT rung the profile opened");
            Assert.That(tape.onboarding, Is.True, "the first-run script ran (a fresh profile)");
            Assert.That(tape.CountOps(TapeOp.Assist), Is.GreaterThanOrEqualTo(4), "the mode (snap, lock, assist, the setting's) and the cap changed");
            Assert.That(tape.CountOps(TapeOp.Toggle), Is.GreaterThan(0), "a lock toggle between steps changed the lock");
            Assert.That(tape.CountOps(TapeOp.DraftOpen), Is.GreaterThan(0), "a MODS draft opened");
            Assert.That(tape.CountOps(TapeOp.DraftPick), Is.EqualTo(tape.CountOps(TapeOp.DraftOpen)), "each draft was picked");
            RunTape loaded = RunTape.FromBytes(tape.ToBytes());
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), loaded);
            Assert.That(r.Equal, Is.True, r.Line);
            TestContext.WriteLine(r.Line + $"; {tape.ToBytes().Length} B; ops: toggles {tape.CountOps(TapeOp.Toggle)}, drafts {tape.CountOps(TapeOp.DraftOpen)}, assist changes {tape.CountOps(TapeOp.Assist)}, onScreen answers {tape.OnScreenAnswers}");
        }

        /// <summary>A headless composition with its own onScreen (the settings' defaults, a fresh profile, no LAB, no records).</summary>
        static SurvivalSim OnScreenSim(Func<Vec3, bool> onScreen)
        {
            GameConfig cfg = Cfg();
            var store = new SettingsStore(cfg, new List<(SettingItem, object)>(), new List<(string, string)>());
            store.Load("");
            var sim = new SurvivalSim(cfg, Map(), new EventBus(), new Lab(cfg), new ScriptedInput(), new Profile(), store.Num, store.Bool, store.Str, onScreen, a => a);
            sim.ToTitle();
            foreach (SurvivalSim.Machine m in sim.machines) sim.RideCore(m);
            sim.noRecords = sim.noLab = true;
            // NIGHT21: this test needs the run in which the stuck-enemy watchdog asks onScreen (NIGHT20's game: 12 answers on the
            // fingerprint script); with the NIGHT21 blocks as configured the SLIDE BITES / ARCLINE change that run to 0 answers
            // (SimHarness tape-onscreen 1 8: 0, and with --off21: 12). The tape records the blocks it read, so the replay matches.
            sim.n21Override = N21Blocks.Off;
            return sim;
        }

        [Test]
        public void OnScreenAnswers_DecideTheRun_AndTheReplayAnswersFromTheTape()
        {
            // The watchdog's camera test (HuskBrain.cs :962) answered by the point, then every answer inverted: the answers
            // change the run (other checkpoints), and each tape replays EQUAL on the same fresh composition (whose onScreen
            // is the tape's: the player's bits in call order).
            var lines = new List<string>[2];
            for (int inv = 0; inv < 2; inv++)
            {
                bool flip = inv == 1;
                RunTape tape = TapeScript.Record(OnScreenSim(p => (((long)Math.Floor(p.x * 3 + p.z) & 1) == 0) != flip), SimFingerprint.Seed, SimFingerprint.Steps, "tests", null, TapeScript.God);
                Assert.That(tape.OnScreenAnswers, Is.GreaterThan(0), "the watchdog asked onScreen");
                TapeResult r = TapeReplay.Replay(Cfg(), Map(), RunTape.FromBytes(tape.ToBytes()));
                Assert.That(r.Equal, Is.True, r.Line);
                lines[inv] = tape.CheckpointLines();
                TestContext.WriteLine($"{(flip ? "inverted" : "by the point")}: onScreen answers {tape.OnScreenAnswers}; {r.Line}");
            }
            Assert.That(lines[1], Is.Not.EqualTo(lines[0]), "the answers changed nothing: the test proves nothing");
        }

        [Test]
        public void AnotherSeedInTheHeader_DiffersAtAnEarlyStep()
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            RunTape tape = TapeScript.Fingerprint(sim, 1800, "tests");
            tape.seed = SimFingerprint.RedSeed;
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), tape);
            Assert.That(r.Equal, Is.False, r.Line);
            Assert.That(r.Line, Does.StartWith("DIFFERS at step "));
            Assert.That(r.FirstDifferentStep, Is.InRange(1, 600), r.Line);
            Assert.That(r.FailedCheckpoint, Is.EqualTo(1), r.Line);
        }

        [Test]
        public void OldFormatForeignAndTruncatedTapes_AreRefusedWithALine()
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            byte[] good = TapeScript.Fingerprint(sim, 120, "tests").ToBytes();
            string path = TempTape("refused");
            try
            {
                // A format this build doesn't read: the version field (after the magic) set to 0. (NIGHT16: format 2 added
                // the move flags and format 1 is still read, with the flags off, so "one lower" is no longer refused.)
                const int unread = 0;
                byte[] old = (byte[])good.Clone();
                int at = RunTape.Magic.Length;
                int version = BitConverter.ToInt32(old, at);
                Assert.That(version, Is.EqualTo(RunTape.FormatVersion));
                byte[] v = BitConverter.GetBytes(unread);
                Array.Copy(v, 0, old, at, 4);
                File.WriteAllBytes(path, old);
                Assert.Throws<TapeFormatException>(() => RunTape.Load(path));
                string line = TapeReplay.ReplayFile(Cfg(), Map(), path);
                Assert.That(line, Does.StartWith("REFUSED"), line);
                Assert.That(line, Does.Contain("format " + unread), line);
                // A file that is no tape, a truncated tape, no file at all: a line each.
                File.WriteAllText(path, "{\"not\": \"a tape\"}");
                Assert.That(TapeReplay.ReplayFile(Cfg(), Map(), path), Does.StartWith("REFUSED"));
                byte[] cut = new byte[good.Length / 2];
                Array.Copy(good, cut, cut.Length);
                File.WriteAllBytes(path, cut);
                Assert.That(TapeReplay.ReplayFile(Cfg(), Map(), path), Does.StartWith("REFUSED"));
                File.Delete(path);
                Assert.That(TapeReplay.ReplayFile(Cfg(), Map(), path), Does.StartWith("REFUSED"));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Test]
        public void TapeFiles_NamesFollowTheRunLogAndPruneKeepsTheNewest()
        {
            Assert.That(TapeFiles.FileName(new DateTime(2026, 10, 4, 22, 5, 9, DateTimeKind.Utc), 42u), Is.EqualTo("run-20261004-220509-42" + RunTape.Extension));
            Assert.That(TapeFiles.NameBeside("C:/x/runs/run-20261004-220509-42-2.jsonl"), Is.EqualTo("run-20261004-220509-42-2" + RunTape.Extension));
            var files = new List<string>();
            for (int i = 0; i < 25; i++) files.Add($"run-202610{i:00}-000000-1{RunTape.Extension}");
            files.Add("run-20260101-000000-1.jsonl"); // the run log's: never the tape's to prune
            files.Add("notes.txt");
            List<string> drop = TapeFiles.ToPrune(files, 20);
            Assert.That(drop.Count, Is.EqualTo(5));
            Assert.That(drop[0], Is.EqualTo($"run-20261000-000000-1{RunTape.Extension}"));
            Assert.That(drop, Has.None.EndsWith(".jsonl"));
        }

        [Test]
        public void AutopilotRun_TapedAndReplayedOnAFreshComposition_Equal()
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            const uint seed = 7;
            sim.metaSilent = true;
            sim.UseInput(new AutopilotInput(sim, "normal", seed));
            RunTape tape = TapeScript.Record(sim, seed, SimFingerprint.Steps, "tests", (s, i) =>
            {
                if (s.flow == Flow.Playing && s.run.state == SurvivalRun.State.Break && s.mods.Pending) AutopilotInput.TakeDraft(s);
            }, null, scripted: false);
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), RunTape.FromBytes(tape.ToBytes()));
            Assert.That(r.Equal, Is.True, r.Line);
            TestContext.WriteLine(r.Line);
        }
    }
}
