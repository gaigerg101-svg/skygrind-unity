// NIGHT14 U3-27.8 (agent T): THE TAPE's commands.
//   tape-scripted     the fingerprint's scripted run (7200 steps) taped on a fresh composition: its checkpoints against
//                     the baseline, the recorder's allocations against the same run untaped, the tape saved and loaded,
//                     then replayed on a FRESH composition: EQUAL at every checkpoint; bytes and bytes per minute
//   replay <file>     a tape replayed on a fresh composition: EQUAL, DIFFERS at step S, or REFUSED
//   tape-old          a tape of the previous format version, and a file that is no tape: each REFUSED with a line
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void Tape(string cmd, string[] args, ref int code)
    {
        switch (cmd)
        {
            case "tape-scripted":
                code = TapeScripted(args.Length > 1 ? int.Parse(args[1]) : SimFingerprint.Steps);
                break;
            case "replay":
                if (args.Length < 2)
                {
                    Console.WriteLine("usage: replay <tape file>");
                    code = 2;
                    break;
                }
                string line = TapeReplay.ReplayFile(Config(), Map(), args[1]);
                Console.WriteLine("replay: " + line);
                code = line.StartsWith("EQUAL", StringComparison.Ordinal) ? 0 : 1;
                break;
            case "tape-old":
                code = TapeOld();
                break;
            case "tape-busy":
                if (args.Length > 1) BusyUtc = BusyUtc.AddDays(int.Parse(args[1]));
                code = TapeBusy();
                break;
            case "tape-red":
                code = TapeRed();
                break;
            case "tape-onscreen":
                code = TapeOnScreen(args.Length > 1 ? uint.Parse(args[1]) : 1, args.Length > 2 ? uint.Parse(args[2]) : 8);
                break;
        }
    }

    /// <summary>TapeTests.EveryNonInputAnswer_RecordedAndReplayed outside Unity: every non-input answer moving, replayed.</summary>
    static DateTime BusyUtc = new DateTime(2026, 10, 4, 22, 30, 0, DateTimeKind.Utc);

    static int TapeBusy()
    {
        int now = 0;
        GameConfig cfg = Config();
        var store = new SettingsStore(cfg, new List<(SettingItem, object)>(), new List<(string, string)>());
        store.Load("");
        Func<string, bool> settingBool = k => k == LabMath.StyleRamp || (!k.StartsWith("lab.", StringComparison.Ordinal) && store.Bool(k)); // the LAB rows are the Game's settings: STYLE RAMP on, the rest off
        var sim = new SurvivalSim(cfg, Map(), new EventBus(), new Lab(cfg), new ScriptedInput(), new Profile { heat = 2 }, store.Num, settingBool, store.Str,
            p => ((long)Math.Floor(p.x * 3 + p.z) & 1) == 0, a => a);
        sim.ToTitle();
        foreach (SurvivalSim.Machine m in sim.machines) sim.RideCore(m);
        sim.assistModeFor = mode => now < 1500 ? "snap" : now < 3000 ? "lock" : now < 5000 ? "assist" : mode;
        sim.breakawayCap = () => now < 2000 ? double.PositiveInfinity : 4.5;
        sim.utcNow = () => BusyUtc;
        sim.noRecords = true;
        sim.setup.mode = RunSetup.Survival; // NIGHT24: the DAILY is gone; a fixed seed
        sim.setup.seed = 1702702973u;
        sim.setup.heat = 1;
        RunTape tape = TapeScript.Record(sim, null, SimFingerprint.Steps, "harness", (s, i) =>
        {
            now = i;
            if (i % 450 == 225) s.assist.ToggleLock();
            if (s.flow == Skygrind.Game.Flow.Playing && s.run.state == SurvivalRun.State.Break && s.mods.Pending)
            {
                List<string> offer = s.mods.Open();
                if (offer != null && offer.Count > 0) s.mods.Pick((i / 7) % offer.Count);
            }
        }, TapeScript.God);
        Console.WriteLine($"tape-busy: seed {tape.seed} (fixed), heat {tape.heat}, lab styleRamp {tape.lab[2]}, onboarding {tape.onboarding}, {tape.steps} steps, {tape.ToBytes().Length} B");
        Console.WriteLine($"  ops: assist changes {tape.CountOps(TapeOp.Assist)}, toggles {tape.CountOps(TapeOp.Toggle)}, drafts {tape.CountOps(TapeOp.DraftOpen)} / picks {tape.CountOps(TapeOp.DraftPick)}, onScreen answers {tape.OnScreenAnswers}, checkpoints {tape.CountOps(TapeOp.Check)}");
        TapeResult r = TapeReplay.Replay(Config(), Map(), RunTape.FromBytes(tape.ToBytes()));
        Console.WriteLine("  replay: " + r.Line);
        return r.Equal ? 0 : 1;
    }

    /// <summary>The watchdog's onScreen answers, varied by the point, taped and replayed over seeds first .. last (the fingerprint's script, 1e6 HP).</summary>
    static int TapeOnScreen(uint first, uint last)
    {
        int bad = 0;
        for (uint seed = first; seed <= last; seed++)
        {
            // The same run with every answer inverted: the answers change the run (other checkpoints), and each tape replays
            // EQUAL on the same fresh composition: the replay answers from the tape.
            var lines = new List<string>[2];
            for (int inv = 0; inv < 2; inv++)
            {
                bool flip = inv == 1;
                RunTape tape = TapeScript.Record(OnScreenSim(p => (((long)Math.Floor(p.x * 3 + p.z) & 1) == 0) != flip), seed, SimFingerprint.Steps, "harness", null, TapeScript.God);
                TapeResult r = TapeReplay.Replay(Config(), Map(), RunTape.FromBytes(tape.ToBytes()));
                lines[inv] = tape.CheckpointLines();
                Console.WriteLine($"tape-onscreen seed {seed}{(flip ? " (answers inverted)" : "")}: onScreen answers {tape.OnScreenAnswers}; {r.Line}");
                if (!r.Equal) bad++;
            }
            int first2 = -1;
            for (int i = 0; i < lines[0].Count && i < lines[1].Count; i++)
                if (lines[0][i] != lines[1][i]) { first2 = i; break; }
            Console.WriteLine($"  the inverted answers' run: {(first2 < 0 ? "the same checkpoints" : $"other checkpoints from {first2 + 1}")}");
        }
        return bad == 0 ? 0 : 1;
    }

    /// <summary>A headless composition with its own onScreen (the settings' defaults, a fresh profile, no LAB, no records).</summary>
    public static SurvivalSim OnScreenSim(Func<Vec3, bool> onScreen)
    {
        GameConfig cfg = Config();
        var store = new SettingsStore(cfg, new List<(SettingItem, object)>(), new List<(string, string)>());
        store.Load("");
        var sim = new SurvivalSim(cfg, Map(), new EventBus(), new Lab(cfg), new ScriptedInput(), new Profile(), store.Num, store.Bool, store.Str, onScreen, a => a);
        sim.ToTitle();
        foreach (SurvivalSim.Machine m in sim.machines) sim.RideCore(m);
        sim.noRecords = sim.noLab = true;
        return sim;
    }

    /// <summary>TapeTests.AnotherSeedInTheHeader_DiffersAtAnEarlyStep outside Unity (seen red).</summary>
    static int TapeRed()
    {
        RunTape tape = TapeScript.Fingerprint(SurvivalSim.Headless(Config(), Map()), 1800, "harness");
        tape.seed = SimFingerprint.RedSeed;
        TapeResult r = TapeReplay.Replay(Config(), Map(), tape);
        Console.WriteLine($"tape-red (seed {SimFingerprint.RedSeed} in a seed-1 tape's header): {r.Line}; first different step {r.FirstDifferentStep}, failed checkpoint {r.FailedCheckpoint}");
        return !r.Equal && r.FirstDifferentStep >= 1 && r.FirstDifferentStep <= 600 && r.FailedCheckpoint == 1 ? 0 : 1;
    }

    static int TapeScripted(int steps)
    {
        // The recorder's allocations: the same run untaped, then taped (each on a fresh composition, the steps alone measured;
        // a first untaped run warms the process's static caches, so neither measured run pays them).
        AllocOfScriptedRun(steps, null);
        long plain = AllocOfScriptedRun(steps, null);
        RunTape tape = null;
        long taped = AllocOfScriptedRun(steps, t => tape = t);
        List<string> lines = tape.CheckpointLines();
        int baseDiff = FingerprintBaseline.FirstDifference(lines);
        Console.WriteLine($"tape-scripted: {tape.steps} steps taped, {lines.Count} checkpoints: {(steps == SimFingerprint.Steps ? baseDiff < 0 ? "the fingerprint baseline's (the recorder only observes)" : $"DIFFER from the baseline at checkpoint {baseDiff + 1}" : "(not the baseline's length)")}");
        Console.WriteLine($"  allocations over the steps: untaped {plain} B, taped {taped} B: the recorder's {taped - plain} B ({lines.Count} checkpoint hashes; the buffer started at {TapeRecorder.InitialBytes} B)");
        string path = Path.Combine(Path.GetTempPath(), "skygrind-tape-scripted" + RunTape.Extension);
        tape.Save(path);
        long bytes = new FileInfo(path).Length;
        double minutes = tape.steps / 120.0 / 60.0;
        Console.WriteLine($"  saved {path}: {bytes} B for {tape.steps} steps ({minutes:0.###} min of play): {bytes / minutes:0} B per minute ({bytes / minutes / 1024:0.0} KB/min); ops: assist changes {tape.CountOps(TapeOp.Assist)}, toggles {tape.CountOps(TapeOp.Toggle)}, drafts {tape.CountOps(TapeOp.DraftOpen)}, onScreen answers {tape.OnScreenAnswers}");
        var t0 = System.Diagnostics.Stopwatch.StartNew();
        RunTape loaded = RunTape.Load(path);
        TapeResult r = TapeReplay.Replay(Config(), Map(), loaded);
        Console.WriteLine($"  replay on a fresh composition ({t0.Elapsed.TotalSeconds:0.00} s): {r.Line}");
        return r.Equal && (steps != SimFingerprint.Steps || baseDiff < 0) ? 0 : 1;
    }

    /// <summary>Bytes allocated on this thread over the fingerprint run's steps (fresh composition, StartRun and the setup outside the window).</summary>
    static long AllocOfScriptedRun(int steps, Action<RunTape> taped)
    {
        SurvivalSim sim = SurvivalSim.Headless(Config(), Map());
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        var script = new ScriptedInput();
        sim.UseInput(script);
        TapeRecorder rec = taped != null ? new TapeRecorder(sim, "harness") : null;
        sim.StartRun(SimFingerprint.Seed, null, null);
        rec?.Begin();
        TapeScript.God(sim);
        TapeScript.Drive(sim, 60, null, script); // warm the JIT outside the window (both runs the same)
        long before = GC.GetAllocatedBytesForCurrentThread();
        TapeScript.Drive(sim, steps, null, script, 60);
        long after = GC.GetAllocatedBytesForCurrentThread();
        if (rec != null) taped(rec.Finish(TapeEnd.Stopped));
        return after - before;
    }

    static int TapeOld()
    {
        SurvivalSim sim = SurvivalSim.Headless(Config(), Map());
        byte[] good = TapeScript.Fingerprint(sim, 240, "harness").ToBytes();
        byte[] v = BitConverter.GetBytes(RunTape.FormatVersion - 1);
        Array.Copy(v, 0, good, RunTape.Magic.Length, 4);
        string path = Path.Combine(Path.GetTempPath(), "skygrind-tape-old" + RunTape.Extension);
        File.WriteAllBytes(path, good);
        string old = TapeReplay.ReplayFile(Config(), Map(), path);
        Console.WriteLine("tape-old: " + old);
        File.WriteAllText(path, "{\"not\": \"a tape\"}");
        string foreign = TapeReplay.ReplayFile(Config(), Map(), path);
        Console.WriteLine("tape-old (a file that is no tape): " + foreign);
        File.Delete(path);
        return old.StartsWith("REFUSED", StringComparison.Ordinal) && foreign.StartsWith("REFUSED", StringComparison.Ordinal) ? 0 : 1;
    }
}
