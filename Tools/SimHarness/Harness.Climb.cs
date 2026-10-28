// NIGHT20 U3-34.2 (the lead; builders never edit this file: NIGHT20.md "How tonight works" rule 11): two more judges.
//   climb [--map survival|drydock] [--perch-seeds n] [--no-perch] [--hand k] [--tag t] [--off20|--off19]
//                  THE CLIMB (Game/Bot/SandboxClimb.cs): R1 the player's nodes the horde can't reach, R2 the worst horde
//                  time (and the floor rule), R2b the worst time to get back down, the 10 worst perches, R3 THE CHASE's
//                  STAND on each perch vs the ground (round 10, 300 s, median of 5 seeds). --hand k: the hand check on
//                  perch k (the bot graph's path flown with the real Mover; one husk walked by the real brain).
//   marathon [--seeds n] [--from s] [--tag t] [--off20|--off19]
//                  THE MARATHON (Game/Bot/SandboxMarathon.cs): the free-play farm read for balance; each process writes
//                  marathon-<from>.jsonl and then MARATHON.md from every marathon-*.jsonl in the tag's folder (so four
//                  processes on seeds 1-10, 11-20, 21-30, 31-40 merge into one report).
// Each writes Markdown and JSON to Logs/sandbox/<tag>/ (default "dev"); each is deterministic (SandboxClimbTests).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static SurvivalMap MapNamed(string name)
    {
        if (name == null || name == "survival") return Map();
        string p = Path.GetFullPath(Path.Combine(RepoRoot, "Design", "maps", name + ".json"));
        return SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(p)));
    }

    static string Side() => N19Blocks.ForceOff ? "--off19 (NIGHT18's game)" : N20Blocks.ForceOff ? "--off20 (NIGHT19's game)" : "the design blocks as configured";

    static void ClimbCommand(string[] args, ref int code)
    {
        var wall = System.Diagnostics.Stopwatch.StartNew();
        GameConfig cfg = Config();
        string mapName = Opt(args, "--map", "survival");
        string tag = Opt(args, "--tag", "dev");
        string dir = Skygrind.Game.Sandbox.Dir(RepoRoot, tag);
        SurvivalMap map = MapNamed(mapName);
        SandboxClimb.Report rep = SandboxClimb.Measure(cfg, map);
        Console.WriteLine($"climb {mapName}: {rep.nodes.Count} player nodes ({rep.reached} graph nodes reached of {rep.graphNodes}, {rep.unmapped} off the grid), {rep.spawnNodes} ground spawns; measured in {wall.Elapsed.TotalSeconds:0.0} s");
        int hand = int.Parse(Opt(args, "--hand", "-1"), CultureInfo.InvariantCulture);
        if (hand >= 0 && hand < rep.perches.Count)
        {
            SandboxClimb.PlayerNode p = rep.perches[hand];
            var (edges, flown, husk) = SandboxClimb.HandCheck(cfg, map, p);
            Console.WriteLine($"hand check, perch {hand}: {p.Name}, the judge's horde time {Inv(p.horde)} s");
            foreach (string e in edges) Console.WriteLine("  " + e);
            Console.WriteLine($"  the path flown with the real Mover: {(flown ? "every edge made it" : "NOT flown")}; one husk walked by the real brain reached the player in {(double.IsPositiveInfinity(husk) ? "never (90 s)" : Inv(husk) + " s")}");
            code = 0;
            return;
        }
        if (!Flag(args, "--no-perch"))
        {
            int ps = int.Parse(Opt(args, "--perch-seeds", SandboxClimb.PerchSeeds.ToString(CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture);
            SandboxClimb.PerchTests(cfg, map, rep, ps, l => Console.WriteLine("  stand " + l));
        }
        string Inf(double v) => double.IsPositiveInfinity(v) ? "never" : double.IsNaN(v) ? "-" : Inv(v, "0.0");
        var md = new StringBuilder();
        md.Append($"# THE CLIMB ({tag}, {mapName})\n\n{Skygrind.Game.Sandbox.Version}; {Side()}; every gate open. The player's set: {rep.nodes.Count} grid nodes under the {rep.reached} bot-graph nodes reachable from the spawn ({rep.unmapped} graph nodes with no grid node under them: left out); {rep.spawnNodes} ground spawn nodes; the husk's speed {Inv(cfg.Num("ai.kinds.husk.speed"), "0.#")} m/s.\n\n");
        md.Append("| Row | Value | Target |\n| --- | --- | --- |\n");
        md.Append($"| R1 player nodes the horde can't reach | {rep.unreachable} of {rep.nodes.Count} | 0 |\n");
        md.Append($"| R2 the worst horde time to a player node | {Inf(rep.worstHorde)} s | <= 12 s (inferred) |\n");
        md.Append($"| R2 floor rule: nodes more than 6 s slower than the floor under them | {rep.floorRuleFails} | 0 |\n");
        md.Append($"| R2b nodes the horde reaches that can't get down within 6 s | {rep.downFails} of {rep.huskNodes} (the worst {Inf(rep.worstDown)} s) | 0 (<= 6 s) |\n");
        int r3Fail = 0;
        if (rep.ground != null)
        {
            foreach (var t in rep.perchTests)
                if (!(t.dpm >= 0.5 * rep.ground.dpm) || !(t.survival <= 2 * rep.ground.survival)) r3Fail++;
            md.Append($"| R3 perches where the STAND bot is safer (dpm < 50 % of the ground's or survival > 2 x) | {r3Fail} of {rep.perchTests.Count} | 0 |\n");
        }
        else md.Append("| R3 perch tests | n/a (--no-perch) | 0 |\n");
        md.Append("\n## The 10 worst perches\n\n| # | Perch | Horde (s) | Floor under (s) | Down (s) | STAND survival (s) | STAND dpm | Died |\n| --- | --- | --- | --- | --- | --- | --- | --- |\n");
        for (int i = 0; i < rep.perches.Count; i++)
        {
            SandboxClimb.PlayerNode p = rep.perches[i];
            SandboxClimb.PerchResult t = i < rep.perchTests.Count ? rep.perchTests[i] : null;
            md.Append($"| {i} | {p.Name} | {Inf(p.horde)} | {Inf(p.floorHorde)} | {Inf(p.down)} | {(t != null ? Inv(t.survival, "0.0") : "-")} | {(t != null ? Inv(t.dpm, "0.0") : "-")} | {(t != null ? t.died + " / " + SandboxClimb.PerchSeeds : "-")} |\n");
        }
        if (rep.ground != null) md.Append($"| ground | the map's spawn (THE CHASE's STAND) | - | - | - | {Inv(rep.ground.survival, "0.0")} | {Inv(rep.ground.dpm, "0.0")} | {rep.ground.died} |\n");
        // Height bands: how much of each band the horde reaches.
        md.Append("\n## By height\n\n| Band (m) | Player nodes | Reached by the horde | Median horde (s) |\n| --- | --- | --- | --- |\n");
        double[] edgesY = { -99, 0.3, 2.5, 5, 8, 12, 20, 999 };
        for (int b = 0; b + 1 < edgesY.Length; b++)
        {
            var inB = rep.nodes.Where(n => n.y > edgesY[b] && n.y <= edgesY[b + 1]).ToList();
            if (inB.Count == 0) continue;
            var fin = inB.Where(n => !double.IsPositiveInfinity(n.horde)).Select(n => n.horde).ToList();
            md.Append($"| {(b == 0 ? "ground" : Inv(edgesY[b], "0.#") + "-" + (edgesY[b + 1] > 100 ? "" : Inv(edgesY[b + 1], "0.#")))} | {inB.Count} | {fin.Count} | {(fin.Count > 0 ? Inv(Skygrind.Game.Sandbox.Median(fin), "0.0") : "-")} |\n");
        }
        string J(double v) => double.IsNaN(v) || double.IsInfinity(v) ? "null" : v.ToString("0.####", CultureInfo.InvariantCulture);
        var js = new StringBuilder("{\"version\":" + Skygrind.Game.Sandbox.Q(Skygrind.Game.Sandbox.Version) + ",\"map\":" + Skygrind.Game.Sandbox.Q(mapName) + ",\"side\":" + Skygrind.Game.Sandbox.Q(Side()));
        js.Append($",\"playerNodes\":{rep.nodes.Count},\"r1\":{rep.unreachable},\"r2\":{J(rep.worstHorde)},\"floorFails\":{rep.floorRuleFails},\"r2b\":{rep.downFails},\"huskNodes\":{rep.huskNodes},\"worstDown\":{J(rep.worstDown)},\"r3\":{(rep.ground != null ? r3Fail.ToString(CultureInfo.InvariantCulture) : "null")}");
        js.Append(",\"perches\":[");
        for (int i = 0; i < rep.perches.Count; i++)
        {
            SandboxClimb.PlayerNode p = rep.perches[i];
            SandboxClimb.PerchResult t = i < rep.perchTests.Count ? rep.perchTests[i] : null;
            js.Append(i > 0 ? "," : "").Append($"{{\"name\":{Skygrind.Game.Sandbox.Q(p.Name)},\"horde\":{J(p.horde)},\"down\":{J(p.down)},\"survival\":{(t != null ? J(t.survival) : "null")},\"dpm\":{(t != null ? J(t.dpm) : "null")}}}");
        }
        js.Append("]");
        if (rep.ground != null) js.Append($",\"ground\":{{\"survival\":{J(rep.ground.survival)},\"dpm\":{J(rep.ground.dpm)}}}");
        js.Append('}');
        string suffix = mapName == "survival" ? "" : "-" + mapName;
        File.WriteAllText(Path.Combine(dir, "CLIMB" + suffix + ".md"), md.ToString());
        File.WriteAllText(Path.Combine(dir, "climb" + suffix + ".json"), js.ToString());
        Console.Write(md.ToString());
        Console.WriteLine($"climb: wrote {Path.Combine(dir, "CLIMB" + suffix + ".md")} in {wall.Elapsed.TotalSeconds:0.0} s");
        code = 0;
    }

    static void MarathonCommand(string[] args, ref int code)
    {
        var wall = System.Diagnostics.Stopwatch.StartNew();
        GameConfig cfg = Config();
        string tag = Opt(args, "--tag", "dev");
        string dir = Skygrind.Game.Sandbox.Dir(RepoRoot, tag);
        int seeds = int.Parse(Opt(args, "--seeds", "40"), CultureInfo.InvariantCulture);
        uint from = uint.Parse(Opt(args, "--from", "1"), CultureInfo.InvariantCulture);
        double maxSeconds = 60 * cfg.Num("design.farm.maxMinutes");
        string part = Path.Combine(dir, $"marathon-{from}.jsonl");
        if (File.Exists(part)) File.Delete(part);
        for (uint s = from; s < from + seeds; s++)
        {
            SandboxMarathon.Run r = SandboxMarathon.RunOne(cfg, Map(), s, maxSeconds);
            File.AppendAllText(part, r.ToJson() + "\n");
            Console.WriteLine($"marathon seed {s}: round {r.round}, {r.end}, heal in-round {r.healFight:0} / break {r.healBreak:0} / hurt {r.hurt:0} ({wall.Elapsed.TotalSeconds:0} s)");
        }
        // Merge every part in the folder into one report.
        var runs = new List<SandboxMarathon.Run>();
        foreach (string f in Directory.GetFiles(dir, "marathon-*.jsonl").OrderBy(x => x, StringComparer.Ordinal))
        {
            foreach (string line in File.ReadAllLines(f))
            {
                if (line.Trim().Length == 0) continue;
                using JsonDocument d = JsonDocument.Parse(line);
                JsonElement e = d.RootElement;
                double D(JsonElement x) => x.ValueKind == JsonValueKind.Number ? x.GetDouble() : double.NaN;
                var r = new SandboxMarathon.Run
                {
                    seed = e.GetProperty("seed").GetUInt32(),
                    round = e.GetProperty("round").GetInt32(),
                    end = e.GetProperty("end").GetString(),
                    deathRound = e.GetProperty("deathRound").GetInt32(),
                    deathInRound = D(e.GetProperty("deathInRound")),
                    healFight = D(e.GetProperty("healFight")),
                    healBreak = D(e.GetProperty("healBreak")),
                    hurt = D(e.GetProperty("hurt")),
                    simTime = D(e.GetProperty("simTime")),
                };
                foreach (JsonElement x in e.GetProperty("startHp").EnumerateArray()) r.startHp.Add(D(x));
                foreach (JsonElement c in e.GetProperty("clears").EnumerateArray())
                {
                    r.clearRound.Add(c[0].GetInt32());
                    r.clearHp.Add(D(c[1]));
                    r.clearTime.Add(D(c[2]));
                }
                runs.Add(r);
            }
        }
        runs.Sort((a, b) => a.seed.CompareTo(b.seed));
        string md = SandboxMarathon.Report(runs, tag, Side(), out string json);
        File.WriteAllText(Path.Combine(dir, "MARATHON.md"), md);
        File.WriteAllText(Path.Combine(dir, "marathon.json"), json);
        Console.Write(md);
        Console.WriteLine($"marathon: {runs.Count} runs in the report; this process {seeds} in {wall.Elapsed.TotalSeconds:0.0} s");
        code = 0;
    }
}
