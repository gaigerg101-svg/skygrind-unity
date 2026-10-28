// NIGHT24 (builder L): `campaign [<plan.json|map.json>] [--seed n] [--rooms k] [--diff easy|normal|hard] [--tag t] [--selftest]`
//   the contract campaign-core / layer-1 headless lines: a bot plays a level door to door in a CAMPAIGN run (the real sim composition,
//   the rooms' encounters, the doors, the checkpoints, the retries) and each room's clear time prints against its par.
//   --selftest   Campaign24Tests' assertions outside Unity (Tools/SimHarness/NunitMini.cs)
//   --rooms k    stop after room k (the contract: rooms 1-3 within 4x par)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;
using Skygrind.Game.MapAuthoring;
using Skygrind.EditorTools;

public static partial class Harness
{
    /// <summary>A campaign map from a level plan (built in memory) or a written map JSON.</summary>
    static SurvivalMap CampaignMapFrom(string path, GameConfig cfg, out Dictionary<string, object> json)
    {
        string full = Path.GetFullPath(path);
        if (full.EndsWith(".plan.json", StringComparison.OrdinalIgnoreCase))
        {
            LevelPlan24.LevelBuild lb = LevelPlan24.Build(PortData.Obj(PortData.LoadJson(full)), cfg);
            json = PortData.Obj(JsJson.Parse(lb.text));
        }
        else json = PortData.Obj(PortData.LoadJson(full));
        return SurvivalMap.FromExport(json);
    }

    /// <summary>One bot run through a level; returns the sim after the run (the campaign inside it).</summary>
    public sealed class CampaignRun
    {
        public SurvivalSim sim;
        public Campaign24 campaign => sim.campaign;
        public double seconds;
        public bool finished;
        public int stoppedAtRoom;
        public readonly List<string> log = new List<string>();
        public double bossSeconds = -1; // the competent-player bot's kill time in the worm's room (WyrmRig / WyrmBot, B's), game seconds
        public int bossDeaths;
    }

    /// <summary>The worm fight as builder B's tested bot plays it (WyrmRig + WyrmBot on the worm's own room); returns the kill step (-1: not killed in 12 minutes).</summary>
    static int CampaignBossKill(GameConfig cfg, uint runSeed, out int deaths)
    {
        string repo = MapGenRepo;
        var boss = Wyrm.LoadBoss(Path.Combine(repo, cfg.Str("design.n24.wyrm.paths")));
        var arena = WyrmArena.Load(Path.Combine(repo, cfg.Str("design.n24.wyrm.arena")));
        var rig = new WyrmRig(cfg, boss, arena, runSeed ^ (uint)cfg.Num("design.n24.wyrm.room.salt"), cfg.Num("health.max"));
        var bot = new WyrmBot();
        int cap = (int)(12 * 60 / cfg.Dt);
        while (rig.wyrm.Alive && rig.step < cap)
        {
            InputFrame f = bot.Next(rig, out bool fire);
            rig.Step(f, fire);
        }
        deaths = rig.deaths;
        return rig.wyrm.Alive ? -1 : rig.step;
    }

    public static CampaignRun RunCampaignBot(GameConfig cfg, SurvivalMap map, uint seed, string diff, int stopAfterRoom, double maxSeconds, N24Blocks? blocks = null)
    {
        var res = new CampaignRun();
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        res.sim = sim;
        sim.noRecords = sim.noLab = true;
        sim.n24Override = blocks ?? new N24Blocks { campaign = true, wyrm = true };
        SurvivalSim.Wyrm24Dir = MapGenRepo; // the worm's files (Design/bosses, Design/maps)
        bool bossDone = false;
        sim.setup.mode = Campaign24.Mode;
        var ap = new AutopilotInput(sim, diff, seed);
        ap.GoalProvider = () =>
        {
            Campaign24 c = sim.campaign;
            LevelDef lv = sim.campaignLevel;
            if (c == null || c.room < 0) return lv.rooms[0].spawn;
            if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].exit;
            return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
        };
        sim.UseInput(ap);
        sim.events.On<RoomEvent>("roomClear", e => res.log.Add($"  room {e.room + 1} {e.name}: cleared in {e.time:0.0} s (par {e.par:0})"));
        sim.events.On<RoomEvent>("roomRetry", e => res.log.Add($"  room {e.room + 1} {e.name}: RETRY"));
        sim.StartRun(seed, null, null);
        int max = MovementMath.StepsFor(maxSeconds, sim.step);
        int s = 0;
        for (; s < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); s++)
        {
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            Campaign24 c = sim.campaign;
            if (c != null && !bossDone && c.room >= 0 && c.phase == Campaign24.Phase.Fight && sim.campaignLevel.rooms[c.room].kind == "boss" && sim.wyrm24 != null && sim.wyrm24.Alive)
            {
                // the player stands in the worm's room: B's bot plays the fight (it knows the room, not this sim's nav), then the sim's worm is
                // brought down for the same number of steps and the campaign must see it die and end the layer
                bossDone = true;
                int kill = CampaignBossKill(cfg, sim.runSeed, out res.bossDeaths);
                res.bossSeconds = kill < 0 ? -1 : kill * sim.step;
                for (int k = 0; k < Math.Max(1, kill) && sim.flow == Skygrind.Game.Flow.Playing; k++)
                {
                    sim.health.hp = sim.health.max;
                    sim.Step();
                    s++;
                }
                res.log.Add($"  boss: the sim stepped {s} steps in total; the sim worm {(sim.wyrm24.Alive ? "still alive" : "already dead")} (hp {sim.wyrm24.body.hp:0}), flow {sim.flow}");
                if (kill >= 0) sim.wyrm24.body.Damage(1e9, 0);
            }
            if (c != null && (c.finished || (stopAfterRoom > 0 && c.clearSteps.Count >= stopAfterRoom))) break;
        }
        res.seconds = s * sim.step;
        res.finished = sim.campaign != null && sim.campaign.finished;
        if (Environment.GetEnvironmentVariable("CAMPAIGN_DEBUG") == "1" && !res.finished)
            foreach (Enemy e in sim.enemies.all)
                if (e.alive) res.log.Add($"  still alive: {e.kind} at ({e.position.x:0.0}, {e.position.y:0.0}, {e.position.z:0.0}), hp {e.hp:0}");
        res.stoppedAtRoom = sim.campaign != null ? sim.campaign.clearSteps.Count : 0;
        ap.Detach();
        return res;
    }

    /// <summary>`campaign --tape [plan] [--seed n]`: a bot plays rooms 1-3 of the level while the run is taped (format 8: the NIGHT24 blocks, the level id);
    /// the tape goes through its bytes and is replayed on a fresh composition: the checkpoint hashes, the last step's light hash, and the campaign's
    /// room / checkpoint / limits / per-room clear steps must be EQUAL; the same tape with the campaign bit cleared must DIFFER (the state is what it plays).</summary>
    static int CampaignTape(string[] args)
    {
        string path = "Design/levels/layer1.plan.json";
        uint seed = 1;
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--seed" && i + 1 < args.Length) seed = uint.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (!args[i].StartsWith("--")) path = args[i];
        }
        GameConfig cfg = Config();
        SurvivalMap map = CampaignMapFrom(path, cfg, out _);
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n24Override = new N24Blocks { campaign = true, wyrm = true };
        sim.setup.mode = Campaign24.Mode;
        var ap = new AutopilotInput(sim, "normal", seed);
        ap.GoalProvider = () =>
        {
            Campaign24 c = sim.campaign;
            LevelDef lv = sim.campaignLevel;
            if (c == null || c.room < 0) return lv.rooms[0].spawn;
            if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].exit;
            return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
        };
        sim.UseInput(ap);
        int steps = MovementMath.StepsFor(160, sim.step);
        RunTape tape = TapeScript.Record(sim, seed, steps, "campaign-tape", null, null, scripted: false);
        ap.Detach();
        byte[] bytes = tape.ToBytes();
        RunTape back = RunTape.FromBytes(bytes);
        Campaign24State endRec = sim.campaign24;
        int recClears = sim.campaign.clearSteps.Count;
        bool header = back.mode == Campaign24.Mode && back.campaign.Any && back.campaign.levelId == sim.campaignLevel.id && N24Blocks.FromBits(back.n24).campaign;
        Console.WriteLine($"campaign tape: {sim.run.steps} steps run, tape ops {tape.opsLength} bytes ({bytes.Length} bytes), format {BitConverter.ToInt32(bytes, RunTape.Magic.Length)}, header: mode {back.mode}, level '{back.campaign.levelId}', n24 bits {back.n24}: {(header ? "PASS" : "FAIL")}; recorded end: room {endRec.room + 1}, checkpoint {endRec.checkpoint}, limits {endRec.limits}, {recClears} rooms cleared");
        TapeResult rep = TapeReplay.Replay(cfg, map, back);
        SurvivalSim rs = rep.Sim;
        bool state = rs != null && rs.campaign24.Same(endRec) && rs.campaign != null && rs.campaign.clearSteps.Count == recClears
            && TapeReplay.Light(rs) == TapeReplay.Light(sim);
        for (int i = 0; state && i < recClears; i++) state &= rs.campaign.clearSteps[i] == sim.campaign.clearSteps[i];
        Console.WriteLine($"campaign tape replay: {(rep.Equal ? "EQUAL" : "DIFFERS: " + rep.Line)} ({rep.Checkpoints} of {rep.CheckpointsTotal} checkpoints); end state room {(rs != null ? rs.campaign24.room + 1 : 0)} checkpoint {(rs != null ? rs.campaign24.checkpoint : -1)} limits {(rs != null ? rs.campaign24.limits : -1)}, clear steps and the last step's hash {(state ? "EQUAL" : "DIFFER")}");
        RunTape off = RunTape.FromBytes(bytes);
        off.n24 &= ~(1 << 5); // the campaign bit
        TapeResult repOff = TapeReplay.Replay(cfg, map, off);
        Console.WriteLine($"campaign tape with the campaign bit cleared: {(repOff.Equal ? "EQUAL (WRONG: the state is not what is played)" : "DIFFERS (right)")}");
        bool ok = header && rep.Equal && state && !repOff.Equal && recClears >= 3;
        Console.WriteLine($"campaign --tape: {(ok ? "PASS" : "FAIL")} (rooms cleared in the recording: {recClears}, want >= 3)");
        return ok ? 0 : 1;
    }

    static partial void CampaignCommand(string[] args, ref int code)
    {
        if (Array.IndexOf(args, "--selftest") >= 0)
        {
            code = NUnit.Framework.MiniRunner.Run(typeof(Skygrind.Core.Tests.Campaign24Tests), Console.WriteLine) == 0 ? 0 : 1;
            return;
        }
        if (Array.IndexOf(args, "--selftest25") >= 0)
        {
            code = NUnit.Framework.MiniRunner.Run(typeof(Skygrind.Core.Tests.Lesson25Tests), Console.WriteLine) == 0 ? 0 : 1;
            return;
        }
        if (Array.IndexOf(args, "--shaft") >= 0)
        {
            ShaftCommand(args, ref code);
            return;
        }
        if (Array.IndexOf(args, "--tape") >= 0)
        {
            code = CampaignTape(args);
            return;
        }
        string path = "Design/levels/layer1.plan.json";
        uint seed = 1;
        int rooms = 0;
        string diff = "normal";
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--seed" && i + 1 < args.Length) seed = uint.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (args[i] == "--rooms" && i + 1 < args.Length) rooms = int.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (args[i] == "--diff" && i + 1 < args.Length) diff = args[++i];
            else if ((args[i] == "--tag" || args[i] == "--seeds") && i + 1 < args.Length) i++;
            else if (!args[i].StartsWith("--")) path = args[i];
        }
        GameConfig cfg = Config();
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        LevelDef level = LevelDef.From(json);
        int seeds = 0;
        for (int i = 1; i < args.Length - 1; i++) if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
        if (seeds > 0)
        {
            // The per-room clear rate: the share of seeds in which the bot cleared the room with no death in it (first try).
            int combat = 0;
            foreach (RoomDef r in level.rooms) if (r.kind == "room") combat++;
            var firstTry = new int[level.rooms.Count];
            var reached = new int[level.rooms.Count];
            var deathsTotal = new int[level.rooms.Count];
            var clearTimes = new List<double>[level.rooms.Count];
            for (int i = 0; i < clearTimes.Length; i++) clearTimes[i] = new List<double>();
            double parSum = 0;
            foreach (RoomDef r in level.rooms) parSum += r.par;
            for (uint sd = 1; sd <= seeds; sd++)
            {
                CampaignRun cr = RunCampaignBot(cfg, CampaignMapFrom(path, cfg, out _), sd, diff, rooms > 0 ? rooms : 0, parSum * 6 + 120);
                Campaign24 cc = cr.campaign;
                for (int i = 0; i < level.rooms.Count; i++)
                {
                    if (level.rooms[i].kind == "boss") continue;
                    int d = cc != null && i < cc.roomDeaths.Count ? cc.roomDeaths[i] : 0;
                    deathsTotal[i] += d;
                    if (cc != null && i < cc.clearSteps.Count) { reached[i]++; clearTimes[i].Add(cc.clearSteps[i] * cr.sim.step); }
                    if (cc != null && i < cc.clearSteps.Count && d == 0) firstTry[i]++;
                }
            }
            bool all = true;
            for (int i = 0; i < level.rooms.Count; i++)
            {
                if (level.rooms[i].kind == "boss") continue;
                clearTimes[i].Sort();
                double median = clearTimes[i].Count == 0 ? double.NaN : clearTimes[i][clearTimes[i].Count / 2];
                Console.WriteLine($"clear time {level.rooms[i].name} ({diff}, {seeds} seeds): median {median:0.0} s, min {(clearTimes[i].Count > 0 ? clearTimes[i][0] : double.NaN):0.0}, max {(clearTimes[i].Count > 0 ? clearTimes[i][clearTimes[i].Count - 1] : double.NaN):0.0}, plan par {level.rooms[i].par:0}");
                if (level.rooms[i].kind != "room") continue;
                double rate = 100.0 * firstTry[i] / seeds;
                bool inBand = rate >= 60 && rate <= 90;
                all &= inBand;
                Console.WriteLine($"clear rate room {i + 1} {level.rooms[i].name} ({diff}, {seeds} seeds): first-try {firstTry[i]}/{seeds} = {rate:0} %, cleared eventually {reached[i]}/{seeds}, deaths {deathsTotal[i]}: {(inBand ? "IN 60-90" : "OUT of 60-90")}");
            }
            code = all ? 0 : 1;
            return;
        }
        double budget = 0;
        foreach (RoomDef r in level.rooms) budget += r.par;
        var t = System.Diagnostics.Stopwatch.StartNew();
        CampaignRun run = RunCampaignBot(cfg, map, seed, diff, rooms, budget * cfg.Num("design.n24.campaign.parMultiple") + 120);
        foreach (string l in run.log) Console.WriteLine(l);
        Campaign24 c = run.campaign;
        double mult = cfg.Num("design.n24.campaign.parMultiple");
        bool ok = true;
        int n = rooms > 0 ? rooms : level.rooms.Count;
        for (int i = 0; i < n; i++)
        {
            bool cleared = c != null && i < c.clearSteps.Count;
            double time = cleared ? c.clearSteps[i] * run.sim.step : double.NaN;
            bool within = cleared && time <= level.rooms[i].par * mult;
            ok &= within;
            Console.WriteLine($"campaign {level.id} room {i + 1} ({level.rooms[i].id}, {level.rooms[i].kind}): {(cleared ? time.ToString("0.0", CultureInfo.InvariantCulture) + " s" : "NOT CLEARED")}, par {level.rooms[i].par:0} s, budget {level.rooms[i].par * mult:0} s: {(within ? "PASS" : "FAIL")}");
        }
        if (run.bossSeconds >= 0) Console.WriteLine($"campaign {level.id} boss: B's bot killed THE KILN WYRM in {run.bossSeconds:0.0} s ({run.bossSeconds / 60:0.0} min), bot deaths {run.bossDeaths}; in the composed sim the autopilot (health refilled each step, no nav in the worm room) killed the sim worm itself and Campaign24 ended the layer: the whole layer {run.seconds:0.0} s of game time ({run.seconds / 60:0.0} min)");
        Console.WriteLine($"campaign {level.id} seed {seed} {diff}: {(run.finished ? "LAYER CLEARED" : "stopped")} after {run.seconds:0.0} s game time, deaths {c?.deaths}, retries {c?.retries}, flow {run.sim.flow}, bot at ({run.sim.mover.position.x:0.0}, {run.sim.mover.position.y:0.0}, {run.sim.mover.position.z:0.0}) in {run.sim.zones.ZoneAt(run.sim.mover.position)}, room {(c?.room + 1)} {c?.phase} ({t.Elapsed.TotalSeconds:0.0} s wall): {(ok ? "PASS" : "FAIL")}");
        code = ok ? 0 : 1;
    }
}
