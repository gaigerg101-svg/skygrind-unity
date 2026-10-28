// NIGHT21 U3-35.7 (builder W): `grind [--seeds n] [--start seed] [--seconds cap] [--tag t]` - THE GRIND's autopilot probe, one process.
// Each seed: the honest AUTOPILOT bot (never calls the exfil: the GRIND has none to win) plays the GRIND mode on a fresh composition
// until it dies or the sim-seconds cap. Prints one line per seed (the wave reached, the waves whose core heights hash was new) and
// the summary: the median wave reached, and whether the core's heights hash differed on every wave (a waveStart's hash against all
// the run's earlier ones).
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void GrindCommand(string[] args, ref int code)
    {
        if (Array.IndexOf(args, "--selftest") >= 0) // Endless21Tests' own assertions, outside Unity (Tools/SimHarness/NunitMini.cs)
        {
            code = NUnit.Framework.MiniRunner.Run(typeof(Skygrind.Core.Tests.Endless21Tests), Console.WriteLine) == 0 ? 0 : 1;
            return;
        }
        int seeds = 10;
        uint start = 1;
        double cap = 420, killAt = -1; // --kill s: the player takes a full-HP blow at s sim seconds (the death path: runEnd, RESULTS, the records line)
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--seeds" && i + 1 < args.Length) seeds = int.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (args[i] == "--start" && i + 1 < args.Length) start = uint.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (args[i] == "--kill" && i + 1 < args.Length) killAt = double.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (args[i] == "--seconds" && i + 1 < args.Length) cap = double.Parse(args[++i], CultureInfo.InvariantCulture);
        }
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        Farm.BuildStamp = "harness";
        var t0 = System.Diagnostics.Stopwatch.StartNew();
        var waves = new List<int>();
        bool allDiffer = true;
        int totalWaves = 0, repeats = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = start + (uint)s;
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.Beacon20Ensure();
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.TellTheTruth();
            ap.ExfilFrom = 0; // never calls the exfil (Farm.ExfilFromOverride 0)
            sim.UseInput(ap);
            sim.noRecords = sim.noLab = true;
            sim.n21Override = new N21Blocks { grind = true };
            sim.setup.mode = RunSetup.Grind;
            sim.setup.heat = 0;
            var seen = new HashSet<ulong>();
            int fights = 0, dup = 0, consec = 0;
            ulong lastHash = Endless21.HeightsHash(new double[] { -7 }); // never a core's
            sim.events.On("waveStart", _ =>
            {
                fights++;
                var hs = new List<double>();
                CoreSpec sp = sim.core.spec;
                for (int r = 0; r < sp.cells; r++)
                    for (int k = 0; k < sp.cells; k++)
                        hs.Add(sim.core.TopAt(sp.x0 + (k + 0.5) * sp.cellSize, sp.z0 + (r + 0.5) * sp.cellSize) ?? -1);
                ulong hh = Endless21.HeightsHash(hs); if (hh == lastHash) consec++; lastHash = hh; if (!seen.Add(hh)) dup++; if (args.Length > 0 && Array.IndexOf(args, "--trace") >= 0) Console.WriteLine($"  wave {sim.grind.CurrentWave} preset {sim.grindPreset} hash {hh:x16} top {hs[0]:0.##},{hs[hs.Count / 2]:0.##}");
            });
            sim.StartRun(seed, null, null);
            int maxSteps = MovementMath.StepsFor(cap, sim.step), steps = 0;
            while (steps < maxSteps && sim.flow != Skygrind.Game.Flow.Results)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                steps++;
                if (killAt >= 0 && steps * sim.step >= killAt && sim.flow == Skygrind.Game.Flow.Playing) sim.health.Damage(sim.health.max * 10, "harness");
            }
            bool timeout = sim.flow != Skygrind.Game.Flow.Results;
            int wave = sim.grind != null ? sim.grind.CurrentWave : 0;
            int cleared = sim.grind != null ? sim.grind.Cleared : 0;
            double score = sim.GrindScore;
            string cause = timeout ? "timeout" : sim.lastDeath?.killer ?? "unknown";
            if (killAt >= 0) Console.WriteLine($"  death path: flow {sim.flow}, grind {sim.grind?.Phase}, stats {(sim.lastGrindStats == null ? "none" : $"wave {sim.lastGrindStats.lastWave} score {sim.lastGrindStats.score:0}")}, line \"{sim.lastGrindJudgement?.text}\", runEnd emitted {(sim.events.Emitted.TryGetValue("runEnd", out int re) ? re : 0)}x, waveBreak {(sim.events.Emitted.TryGetValue("waveBreak", out int wb) ? wb : 0)}x, waveStart {(sim.events.Emitted.TryGetValue("waveStart", out int ws) ? ws : 0)}x, waveClear {(sim.events.Emitted.TryGetValue("waveClear", out int wc) ? wc : 0)}x, survival record round {sim.records.Best(sim.RecordsKey)?.round ?? 0}");
            if (timeout) sim.EndRun();
            ap.Detach();
            waves.Add(wave);
            totalWaves += fights;
            if (consec > 0) allDiffer = false;
            repeats += dup;
            Console.WriteLine($"grind seed {seed}: wave {wave} (cleared {cleared}), {fights} fights, {consec} equal to the wave before, {dup} equal to any earlier wave, score {score:0}, {steps * sim.step:0.0} s sim, {cause}");
        }
        waves.Sort();
        double median = waves.Count % 2 == 1 ? waves[waves.Count / 2] : (waves[waves.Count / 2 - 1] + waves[waves.Count / 2]) / 2.0;
        Console.WriteLine($"grind {seeds} seeds: median wave {median.ToString("0.#", CultureInfo.InvariantCulture)} (min {waves[0]}, max {waves[waves.Count - 1]}); core heights hash differs from the wave before on every wave: {(allDiffer ? "YES" : "NO")} over {totalWaves} fights ({repeats} fights wear a shape an earlier wave of the run wore, after the 7-shape lap); {t0.Elapsed.TotalSeconds:0.0} s wall");
        code = allDiffer && totalWaves > 0 ? 0 : 1;
    }
}
