// NIGHT14 U3-27.5 / U3-27.6 (agent A): the AUTOPILOT's and THE FARM's harness commands (the build stamp is "harness").
//   autopilot <seed> <difficulty>                    one autopilot run (real HP, design.farm.maxMinutes): its JSONL line
//   autopilot-twice <seed> <difficulty>              the same run twice on fresh compositions: the hashes (every 600 steps) equal?
//   farm <runs> <difficulty> <seedStart> <out.jsonl> Farm.Run: one JSONL line per run appended to the file
//   table [seeds] [seedStart]                        seeds (20) x easy / normal / hard: round, time, cause (a markdown table)
//   autopilot-map                                    the buyables, zones and gate spots the shopper plans over
//   autopilot-trace <seed> <difficulty> [every] [s]  a state line every `every` s (the round, place, HP, brain, shop, target)
//   autopilot-selftest                               AutopilotTests' and FarmTests' assertions outside Unity
// Any command takes "ported" (agent N's BotBrain on BotNav's graph) or "standin" after its arguments to pick the brain.
// NIGHT20 (builder P2): farm / table / autopilot take --beacon <share>, --assist <downs>, --noexfil after their arguments.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void Autopilot(string cmd, string[] args, ref int code)
    {
        // The brain, by a word anywhere after the command: "ported" (agent N's BotBrain on BotNav's graph) or "standin".
        foreach (string a in args)
        {
            if (a == "ported")
            {
                AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
                AutopilotInput.GraphSource = BotNav.Survival;
            }
            else if (a == "standin")
            {
                AutopilotInput.BrainFactory = AutopilotInput.StandInBrain;
                AutopilotInput.GraphSource = AutopilotInput.NoGraph;
            }
        }
        // NIGHT20 (builder P2), after the positional arguments: --beacon <share> (THE BEACON: the share of standing beacons the
        // farm's bot takes; Farm.BeaconShare), --assist <downs> (E2: ASSIST's damage at that many saved downs; Farm.AssistDowns),
        // --noexfil (the honest bot never calls the exfil: a depth measure; Farm.ExfilFromOverride 0).
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--beacon" && i + 1 < args.Length) Farm.BeaconShare = double.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (args[i] == "--assist" && i + 1 < args.Length) Farm.AssistDowns = int.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (args[i] == "--noexfil") Farm.ExfilFromOverride = 0;
        }
        switch (cmd)
        {
            case "autopilot":
                code = AutopilotOne(Arg(args, 1, 1u), Arg(args, 2, "normal"));
                break;
            case "autopilot-twice":
                code = AutopilotTwice(Arg(args, 1, 1u), Arg(args, 2, "normal"));
                break;
            case "farm":
                code = AutopilotFarm((int)Arg(args, 1, 30u), Arg(args, 2, "normal"), Arg(args, 3, 1u), Arg(args, 4, "Temp/farm.jsonl"));
                break;
            case "table":
                code = AutopilotTable((int)Arg(args, 1, 20u), Arg(args, 2, 1u));
                break;
            case "autopilot-map":
                code = AutopilotMap();
                break;
            case "autopilot-trace":
                code = AutopilotTrace(Arg(args, 1, 1u), Arg(args, 2, "normal"), Arg(args, 3, 10u), Arg(args, 4, 600u));
                break;
            case "autopilot-selftest":
                code = AutopilotSelfTest();
                break;
        }
    }

    /// <summary>AutopilotTests' and FarmTests' assertions outside Unity (the gate is the Editor's EditMode run; this is the
    /// fast loop): each check prints PASS or FAIL with its numbers.</summary>
    static int AutopilotSelfTest()
    {
        int fails = 0;
        void Check(string name, bool ok, string detail)
        {
            Console.WriteLine($"{(ok ? "PASS" : "FAIL")} {name}: {detail}");
            if (!ok) fails++;
        }
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            var ap = new AutopilotInput(sim, "normal", 1);
            sim.UseInput(ap);
            sim.StartRun(1, null, null);
            Check("Constructor_SetsTheBotsRules", sim.metaSilent && sim.noLab && sim.AssistMode == "off" && ap.Brain.Difficulty == "normal" && sim.health.max == cfg.Num("health.max"),
                $"metaSilent {sim.metaSilent}, noLab {sim.noLab}, assist {sim.AssistMode}, difficulty {ap.Brain.Difficulty}, max HP {sim.health.max}");
        }
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.UseInput(new AutopilotInput(sim, "normal", 4));
            sim.StartRun(4, null, null);
            sim.mods.Queue(sim.run.round);
            bool pending = sim.mods.Pending;
            int before = 0, after = 0;
            foreach (var kv in sim.mods.stacks) before += kv.Value;
            AutopilotInput.TakeDraft(sim);
            foreach (var kv in sim.mods.stacks) after += kv.Value;
            Check("TakeDraft_TakesTheFirstCard", pending && !sim.mods.Pending && sim.mods.offer == null && after == before + 1, $"pending before {pending}, after {sim.mods.Pending}, stacks {before} -> {after}");
        }
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            var ap = new AutopilotInput(sim, "normal", 3);
            sim.UseInput(ap);
            sim.StartRun(3, null, null);
            Vec3 spawn = sim.mover.position;
            bool had = false;
            double moved = 0;
            for (int i = 0; i < 7200 && sim.flow == Skygrind.Game.Flow.Playing; i++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                had |= ap.HasTarget;
                double dx = sim.mover.position.x - spawn.x, dz = sim.mover.position.z - spawn.z;
                moved = Math.Max(moved, Math.Sqrt(dx * dx + dz * dz));
            }
            Check("TheBotFights", had && sim.run.stats.shots > 0 && moved > 2, $"target {had}, shots {sim.run.stats.shots}, moved {moved:0.0} m");
        }
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            string before = sim.profile.ToJson();
            sim.UseInput(new AutopilotInput(sim, "normal", 5));
            sim.StartRun(5, null, null);
            int steps = 0;
            for (; steps < 72000 && sim.flow != Skygrind.Game.Flow.Results; steps++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
            }
            if (sim.flow != Skygrind.Game.Flow.Results) sim.EndRun();
            Check("TheMetaRecordsNothing", sim.lastStats != null && !sim.lastStats.recorded && sim.profile.runs == 0 && sim.records.Best("survival") == null && sim.profile.ToJson() == before,
                $"{steps} steps, recorded {sim.lastStats?.recorded}, runs {sim.profile.runs}, profile unchanged {sim.profile.ToJson() == before}");
        }
        {
            var a = new List<string>();
            var b = new List<string>();
            Farm.RunOne(cfg, map, 1, "normal", 60, a);
            Farm.RunOne(cfg, map, 1, "normal", 60, b);
            Farm.RunOne(cfg, map, 2, "normal", 20, null);
            var c = new List<string>();
            Farm.RunOne(cfg, map, 2, "normal", 60, c);
            Check("SameSeedTwice_SameHashes", a.Count > 0 && string.Join("|", a) == string.Join("|", b), $"{a.Count} hashes");
            Check("AnotherSeed_PlaysAnotherRun", string.Join("|", a) != string.Join("|", c), "seed 2 differs from seed 1");
        }
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"skygrind-farm-selftest-{Guid.NewGuid():N}.jsonl");
            var log = new List<string>();
            string summary = Farm.Run(Config(), Map(), 2, "normal", 11, path, log.Add, 1.0);
            string[] lines = System.IO.File.ReadAllLines(path);
            System.IO.File.Delete(path);
            bool ok = lines.Length == 2 && log.Count >= 2;
            for (int i = 0; ok && i < lines.Length; i++)
            {
                var o = PortData.Obj(JsJson.Parse(lines[i]));
                ok = PortData.Num(o["seed"]) == 11 + i && (string)o["difficulty"] == "normal" && o.ContainsKey("build") && PortData.Num(o["round"]) >= 1
                     && PortData.Num(o["simTime"]) > 0 && PortData.Num(o["simTime"]) <= 60.01 && o["cause"] is string;
                foreach (string key in new[] { "earnedByRound", "spentByRound", "guns", "killsByGun", "perks", "mods", "forge", "rankSteps", "downs", "exfil" }) ok &= o.ContainsKey(key);
            }
            Check("Run_WritesOneLinePerRun", ok, summary);
            Farm.Record r = Farm.RunOne(cfg, map, 21, "normal", 20);
            Check("AShortCap_EndsAsTimeout", r.cause == "timeout" && Math.Abs(r.simTime - 20) < 0.01 && !r.ToJson().Contains("\n"), $"cause {r.cause}, simTime {r.simTime}");
            string j1 = Farm.RunOne(cfg, map, 31, "hard", 90).ToJson(), j2 = Farm.RunOne(cfg, map, 31, "hard", 90).ToJson();
            Check("SameSeed_SameLine", j1 == j2, j1.Length + " chars");
            Farm.Record r4 = Farm.RunOne(cfg, map, 41, "normal", 120);
            var o4 = PortData.Obj(JsJson.Parse(r4.ToJson()));
            double kills = 0, rankSteps = 0;
            foreach (var kv in PortData.Obj(o4["killsByGun"])) kills += PortData.Num(kv.Value);
            foreach (object n in PortData.Arr(o4["rankSteps"])) rankSteps += PortData.Num(n);
            Check("TheLine_CountsWhatTheRunDid", kills == r4.kills && rankSteps == r4.steps && PortData.Arr(o4["earnedByRound"]).Count == r4.round,
                $"kills {kills} / {r4.kills}, rank steps {rankSteps} / {r4.steps}, rounds {PortData.Arr(o4["earnedByRound"]).Count} / {r4.round}");
        }
        {
            // The autopilot's per-step path (SampleMove: the target, the shopper or the hold, the brain; SampleCombat) allocates
            // nothing once warm: measured around the two calls (made beside the sim's own; the extra brain steps don't matter here).
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            var factory = AutopilotInput.BrainFactory; // the adapter's own path, deterministic: the stand-in brain, no graph (the ported brain's and A*'s list growth is agent N's ZeroAllocTests line)
            AutopilotInput.BrainFactory = AutopilotInput.StandInBrain;
            var graphs = AutopilotInput.GraphSource;
            AutopilotInput.GraphSource = AutopilotInput.NoGraph;
            var ap = new AutopilotInput(sim, "normal", 6);
            AutopilotInput.BrainFactory = factory;
            AutopilotInput.GraphSource = graphs;
            sim.UseInput(ap);
            sim.StartRun(6, null, null);
            long bytes = 0;
            int fightSteps = 0, shopSteps = 0, allocSteps = 0;
            for (int i = 0; i < 24000 && sim.flow == Skygrind.Game.Flow.Playing; i++)
            {
                AutopilotInput.TakeDraft(sim);
                if (i >= 2400)
                {
                    long b0 = GC.GetAllocatedBytesForCurrentThread();
                    ap.SampleMove(true);
                    ap.SampleCombat(true);
                    long db = GC.GetAllocatedBytesForCurrentThread() - b0;
                    bytes += db;
                    if (db > 0 && allocSteps++ < 5) Console.WriteLine($"  alloc {db} B at step {i} ({sim.run.state}, round {sim.run.round}, brain {ap.Brain.State}, shop {ap.Shopper.mode}, holding {sim.objectives.Holding})");
                    if (sim.run.state == SurvivalRun.State.Break) shopSteps++;
                    else fightSteps++;
                }
                sim.Step();
            }
            Check("PerStep_AllocatesNothing", bytes == 0, $"{bytes} bytes in {allocSteps} steps over {fightSteps} fight + {shopSteps} break steps");
        }
        Console.WriteLine(fails == 0 ? "selftest: all passed" : $"selftest: {fails} failed");
        return fails == 0 ? 0 : 1;
    }

    /// <summary>A line every `every` s of sim time for `seconds`: the round, the bot's place, HP, mode, target and the enemies left.</summary>
    static int AutopilotTrace(uint seed, string difficulty, uint every, uint seconds)
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        var ap = new AutopilotInput(sim, difficulty, seed);
        sim.UseInput(ap);
        sim.StartRun(seed, null, null);
        int per = MovementMath.StepsFor(every, sim.step), max = MovementMath.StepsFor(seconds, sim.step);
        for (int i = 1; i <= max && sim.flow != Skygrind.Game.Flow.Results; i++)
        {
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            if (i % per != 0) continue;
            Vec3 p = sim.mover.position;
            var alive = new List<string>();
            foreach (Enemy e in sim.enemies.all)
                if (e.alive) alive.Add($"{e.kind}@({e.position.x:0},{e.position.y:0},{e.position.z:0})");
            string tgt = ap.HasTarget ? $"({ap.TargetPosition.x:0},{ap.TargetPosition.y:0},{ap.TargetPosition.z:0}) vis {ap.Target.visible}" : "-";
            Console.WriteLine($"t {sim.SimTime - 0:0} r {sim.run.round} {sim.run.state} flow {sim.flow} pos ({p.x:0.0},{p.y:0.0},{p.z:0.0}) hp {sim.health.hp:0} pts {sim.economy.total:0} brain {ap.Brain.State}{(ap.Brain is BotBrain bb ? $" plans {bb.navPlans} stuck {bb.navStuck} ovr {(bb.overrideGoal.HasValue ? $"({bb.overrideGoal.Value.x:0},{bb.overrideGoal.Value.z:0})" : "-")} walk ({ap.Shopper.walkTo.x:0},{ap.Shopper.walkTo.z:0})" : "")} obj {sim.objectives.Type}{(sim.objectives.Holding ? $" HOLD ({sim.objectives.Spot.Value.x:0},{sim.objectives.Spot.Value.y:0},{sim.objectives.Spot.Value.z:0}) {sim.objectives.Held}/{sim.objectives.Need}" : "")} shop {ap.Shopper.mode} {ap.Shopper.goal?.id} cand {sim.buyables.candidate?.id} tgt {tgt} gun {sim.loadout.Weapon} alive {alive.Count}: {string.Join(" ", alive.GetRange(0, Math.Min(6, alive.Count)))}");
        }
        return 0;
    }

    static uint Arg(string[] args, int i, uint fallback) => i < args.Length && uint.TryParse(args[i], out uint v) ? v : fallback;

    static string Arg(string[] args, int i, string fallback) => i < args.Length ? args[i] : fallback;

    static double FarmMinutes(GameConfig cfg) => cfg.Num("design.farm.maxMinutes");

    static int AutopilotOne(uint seed, string difficulty)
    {
        Farm.BuildStamp = "harness";
        GameConfig cfg = Config();
        var t = System.Diagnostics.Stopwatch.StartNew();
        Farm.Record r = Farm.RunOne(cfg, Map(), seed, difficulty, FarmMinutes(cfg) * 60);
        Console.WriteLine(r.ToJson());
        Console.WriteLine($"autopilot seed {seed} {difficulty} ({r.brain}): round {r.round}, {r.simTime:0.0} s sim, {r.cause}, kills {r.kills}; {t.Elapsed.TotalSeconds:0.00} s wall");
        return 0;
    }

    static int AutopilotTwice(uint seed, string difficulty)
    {
        Farm.BuildStamp = "harness";
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var a = new List<string>();
        var b = new List<string>();
        Farm.Record ra = Farm.RunOne(cfg, map, seed, difficulty, FarmMinutes(cfg) * 60, a);
        Farm.Record rb = Farm.RunOne(cfg, map, seed, difficulty, FarmMinutes(cfg) * 60, b);
        int first = -1;
        for (int i = 0; i < Math.Max(a.Count, b.Count); i++)
            if (i >= a.Count || i >= b.Count || a[i] != b[i])
            {
                first = i;
                break;
            }
        bool lines = ra.ToJson() == rb.ToJson();
        Console.WriteLine($"autopilot-twice seed {seed} {difficulty} ({ra.brain}): {(first < 0 && lines ? $"EQUAL ({a.Count} of {a.Count} hashes, the JSONL lines equal)" : first >= 0 ? $"first difference at checkpoint {first + 1}" : "hashes equal, the JSONL lines differ")}; run 1 round {ra.round} {ra.simTime:0.0} s {ra.cause}, run 2 round {rb.round} {rb.simTime:0.0} s {rb.cause}");
        return first < 0 && lines ? 0 : 1;
    }

    static int AutopilotFarm(int runs, string difficulty, uint seedStart, string path)
    {
        Farm.BuildStamp = "harness";
        string summary = Farm.Run(Config(), Map(), runs, difficulty, seedStart, path, Console.WriteLine);
        Console.WriteLine(summary);
        return 0;
    }

    static int AutopilotTable(int seeds, uint seedStart)
    {
        Farm.BuildStamp = "harness";
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        string[] diffs = { "easy", "normal", "hard" };
        var t = System.Diagnostics.Stopwatch.StartNew();
        var rows = new Farm.Record[seeds, diffs.Length];
        string brain = "?";
        for (int s = 0; s < seeds; s++)
            for (int d = 0; d < diffs.Length; d++)
            {
                rows[s, d] = Farm.RunOne(cfg, map, unchecked(seedStart + (uint)s), diffs[d], FarmMinutes(cfg) * 60);
                brain = rows[s, d].brain;
            }
        Console.WriteLine($"AUTOPILOT table: {seeds} seeds from {seedStart} x {string.Join(" / ", diffs)}, real HP, brain {brain}, cap {FarmMinutes(cfg)} min ({t.Elapsed.TotalSeconds:0.0} s wall)");
        Console.WriteLine();
        Console.WriteLine("| seed | easy: round, time, cause | normal: round, time, cause | hard: round, time, cause |");
        Console.WriteLine("|---|---|---|---|");
        for (int s = 0; s < seeds; s++)
        {
            var cells = new List<string>();
            for (int d = 0; d < diffs.Length; d++)
            {
                Farm.Record r = rows[s, d];
                cells.Add(string.Format(CultureInfo.InvariantCulture, "{0}, {1:0.0} s, {2}", r.round, r.simTime, r.cause));
            }
            Console.WriteLine($"| {unchecked(seedStart + (uint)s)} | {string.Join(" | ", cells)} |");
        }
        var means = new List<string>();
        for (int d = 0; d < diffs.Length; d++)
        {
            double sum = 0, time = 0;
            int max = 0;
            var causes = new SortedDictionary<string, int>(StringComparer.Ordinal);
            for (int s = 0; s < seeds; s++)
            {
                sum += rows[s, d].round;
                time += rows[s, d].simTime;
                max = Math.Max(max, rows[s, d].round);
                causes[rows[s, d].cause] = (causes.TryGetValue(rows[s, d].cause, out int n) ? n : 0) + 1;
            }
            var cs = new List<string>();
            foreach (var kv in causes) cs.Add($"{kv.Key} {kv.Value}");
            means.Add(string.Format(CultureInfo.InvariantCulture, "{0}: mean round {1:0.00}, max {2}, mean time {3:0.0} s; {4}", diffs[d], sum / seeds, max, time / seeds, string.Join(", ", cs)));
        }
        Console.WriteLine();
        foreach (string m in means) Console.WriteLine(m);
        return 0;
    }

    static int AutopilotMap()
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = true;
        sim.StartRun(1, null, null);
        Console.WriteLine($"spawn {sim.map.playerSpawn} yaw {sim.map.playerSpawnYawDeg} start zone {map.startZone} floor {map.floorSize} core {(map.core.HasValue ? $"{map.core.Value.x0},{map.core.Value.z0} x{map.core.Value.cells} @{map.core.Value.cellSize}" : "none")}");
        for (int z = 0; z < map.zones.Count; z++) Console.WriteLine($"zone {z} {map.zones[z].id} locked {sim.zones.IsLocked(z)}");
        foreach (Buyables.Item it in sim.buyables.list)
        {
            int zone = ZoneMath.ZoneIndexAt(map.zoneParts, it.position.x, it.position.y + 1, it.position.z);
            Console.WriteLine($"item {it.id,-22} {it.kind,-8} at ({it.position.x:0.0},{it.position.y:0.0},{it.position.z:0.0}) zone {zone} price {it.price():0} available {it.available?.Invoke()}");
        }
        foreach (BuySpot s in sim.zones.spots) Console.WriteLine($"gate spot zone {s.zone} {s.zoneId} at ({s.at.x:0.0},{s.at.y:0.0},{s.at.z:0.0}) spotZone {ZoneMath.ZoneIndexAt(map.zoneParts, s.spot.pos.x, s.spot.pos.y + 1, s.spot.pos.z)} buyable {sim.zones.SpotBuyable(s)}");
        return 0;
    }
}
