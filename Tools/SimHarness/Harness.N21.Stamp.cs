// NIGHT21 U3-35.5 THE STAMP (builder S owns this file): `stamp [--seeds n] [--from s] [--rounds r] [--tag t]`: the round
// grades of short headless autopilot runs (design.n21.stamp on, every other NIGHT21 block off), printed per round and as a
// distribution. Not a farm: a few seeds, a few rounds. The line "stamp: ..." closes it; code 0 when every cleared round said its
// roundGrade once and the off run (the row off, the same seed) said none.
using System;
using System.Collections.Generic;
using System.Text;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static string StampArg(string[] args, string name, string fallback)
    {
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == name) return args[i + 1];
        return fallback;
    }

    static List<RoundGradeEvent> StampRun(GameConfig cfg, SurvivalMap map, uint seed, int rounds, double maxSeconds, bool on, out int said)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n21Override = new N21Blocks { stamp = on };
        var ap = new AutopilotInput(sim, "normal", seed);
        ap.TellTheTruth();
        sim.UseInput(ap);
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        int count = 0;
        sim.events.On<RoundGradeEvent>(Grades21.EventName, e => count++);
        sim.StartRun(seed, null, null);
        int maxSteps = MovementMath.StepsFor(maxSeconds, sim.step);
        int steps = 0;
        while (steps < maxSteps && sim.flow != Skygrind.Game.Flow.Results && (sim.grades21 == null || sim.grades21.Rounds.Count < rounds))
        {
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            steps++;
        }
        said = count;
        return sim.grades21 != null ? new List<RoundGradeEvent>(sim.grades21.Rounds) : new List<RoundGradeEvent>();
    }

    static partial void StampCommand(string[] args, ref int code)
    {
        int seeds = int.Parse(StampArg(args, "--seeds", "3"));
        uint from = uint.Parse(StampArg(args, "--from", "1"));
        int rounds = int.Parse(StampArg(args, "--rounds", "4"));
        double maxSeconds = double.Parse(StampArg(args, "--seconds", "420"), System.Globalization.CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var dist = new SortedDictionary<string, int>();
        int total = 0, ok = 0;
        bool consistent = true;
        var t = System.Diagnostics.Stopwatch.StartNew();
        for (uint seed = from; seed < from + seeds; seed++)
        {
            List<RoundGradeEvent> grades = StampRun(cfg, map, seed, rounds, maxSeconds, true, out int said);
            if (said != grades.Count) consistent = false;
            foreach (RoundGradeEvent g in grades)
            {
                Console.WriteLine($"seed {seed} round {g.round}: {g.grade}{(g.p ? "" : "")} (letter {g.letter}, score {g.score:0.000}, damage {g.damage:0.0}, peak rank {g.peakRank}, {g.time:0.0} s, {g.kills} kills)");
                dist[g.grade] = dist.TryGetValue(g.grade, out int n) ? n + 1 : 1;
                total++;
            }
            if (grades.Count > 0) ok++;
        }
        // the off side: the same first seed with the row off says nothing and keeps no rounds
        List<RoundGradeEvent> offGrades = StampRun(cfg, map, from, 1, 60, false, out int offSaid);
        bool offQuiet = offSaid == 0 && offGrades.Count == 0;
        var sb = new StringBuilder();
        foreach (var kv in dist) sb.Append($"{kv.Key} {kv.Value}  ");
        Console.WriteLine($"stamp: {total} round grades over {seeds} seeds ({ok} with a cleared round), distribution {sb.ToString().TrimEnd()}; the event said once per round {(consistent ? "PASS" : "FAIL")}; row off says none {(offQuiet ? "PASS" : "FAIL")}; {t.Elapsed.TotalSeconds:0.0} s wall");
        code = consistent && offQuiet && total > 0 ? 0 : 1;
    }
}
