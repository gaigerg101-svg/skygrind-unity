// NIGHT25 (builder L): the teaching campaign's headless instruments.
//   tutorial [--seeds n] [--seed s] [--plan path]   a bot walks the six tutorial stages in the real sim (a CAMPAIGN run on tutorial.json) and prints
//       the lesson-clear order and times, the room clears, the stage respawns, the first-try rate of the combat stages over n seeds.
//       The movement stages (1.1-1.3) are driven by a scripted pilot (TutorialBot: a slide tunnel and a wall-jump flue need the real verbs; the
//       autopilot's graph has neither); the fights (1.4-1.6) are the autopilot's. The block-off lessons (punchWhileFiring needs design.n25.meleeFire,
//       flickCrossed needs the charged fist) are passed over by the rule and printed as SHORT.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class TutorialRun
    {
        public SurvivalSim sim;
        public Campaign24 campaign => sim.campaign;
        public double seconds;
        public bool finished;
        public readonly List<string> lessons = new List<string>();
        public readonly List<string> log = new List<string>();
        public int stageRespawns => campaign != null ? campaign.stageRespawns : 0;
    }

    static TutorialRun RunTutorialBot(GameConfig cfg, SurvivalMap map, uint seed, double maxSeconds, N25Blocks n25, N24Blocks? n24 = null, N26Blocks? n26 = null)
    {
        var res = new TutorialRun();
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        res.sim = sim;
        sim.noRecords = sim.noLab = true;
        sim.n24Override = n24 ?? new N24Blocks { campaign = true };
        sim.n25Override = n25;
        if (n26 != null) // NIGHT26 (--n26): the Player's level-1 blocks: the LT pulse, weapon identity, the hulk; the campaign loadout = the start weapons plus the cleared levels' (none): KESTREL + PULSE
        {
            sim.n26Override = n26;
            sim.n27Override = N27Blocks.ForceOff ? N27Blocks.Off : N27Blocks.From(cfg); // NIGHT27: the config's NIGHT27 rows (boss1: THE LANTERN; pulse27; chargeShots27)
            sim.bosses25Override = true;
            SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        }
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
        var bot = new TutorialPilot25(sim, ap);
        sim.UseInput(bot);
        sim.events.On<LessonEvent>("lessonClear", e => res.lessons.Add($"{e.id} {e.kind} at {e.time:0.0} s in the stage (run {sim.run.steps * sim.step:0.0} s)"));
        sim.events.On<RoomEvent>("roomClear", e => res.log.Add($"  stage {e.room + 1} {e.name}: cleared in {e.time:0.0} s (par {e.par:0})"));
        sim.events.On<RoomEvent>("roomRetry", e => res.log.Add($"  stage {e.room + 1} {e.name}: RETRY (a death)"));
        sim.events.On<RoomEvent>("stageRespawn", e => res.log.Add($"  stage {e.room + 1} {e.name}: STAGE RESPAWN at {sim.run.steps * sim.step:0.0} s"));
        sim.StartRun(seed, null, null);
        int max = MovementMath.StepsFor(maxSeconds, sim.step);
        int s = 0;
        for (; s < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); s++)
        {
            AutopilotInput.TakeDraft(sim);
            if (n26 != null && n26.Value.weaponIdentity && sim.campaign != null && sim.campaign.unlocks == null && File.Exists("Design/levels/unlocks.json"))
            {
                Unlocks26 u = Unlocks26.Parse(File.ReadAllText("Design/levels/unlocks.json")); // CampaignUnlocks26.Apply's work, without a game's map list
                sim.campaign.unlocks = u;
                List<string> owned = u.OwnedAfter(null);
                foreach (string w in owned) if (!sim.loadout.owned.Contains(w)) sim.loadout.AddWeapon(w);
                sim.loadout.FilterOwned26(owned);
                res.log.Add("  loadout: " + string.Join(", ", sim.loadout.owned));
            }
            sim.Step();
            if (sim.campaign != null && sim.campaign.finished) break;
        }
        res.seconds = s * sim.step;
        res.finished = sim.campaign != null && sim.campaign.finished;
        ap.Detach();
        return res;
    }

    static partial void TutorialCommand(string[] args, ref int code)
    {
        string path = "Design/levels/tutorial.plan.json";
        uint seed = 1;
        int seeds = 0;
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--seed" && i + 1 < args.Length) seed = uint.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (args[i] == "--seeds" && i + 1 < args.Length) seeds = int.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (args[i] == "--plan" && i + 1 < args.Length) path = args[++i];
            else if (args[i] == "--tag" && i + 1 < args.Length) i++;
        }
        GameConfig cfg = Config();
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        LevelDef level = LevelDef.From(json);
        double budget = 0;
        foreach (RoomDef r in level.rooms) budget += r.par;
        double cap = budget * cfg.Num("design.n24.campaign.parMultiple") + 120;
        // the NIGHT25 blocks the run reads: the tutorial on; meleeFire / pulseCharge off until builder C's land (their lessons are passed over)
        bool n26 = Array.IndexOf(args, "--n26") >= 0; // NIGHT26: the LT pulse, weapon identity and the hulk on (level 1 as the Player plays it)
        N26Blocks? n26b = n26 ? new N26Blocks { pulseLT = true, weaponIdentity = true, level1 = true } : (N26Blocks?)null;
        bool c25 = n26 || Array.IndexOf(args, "--c25") >= 0; // the Player's blocks: meleeFire, pulseCharge and the charged fist on (builder C's rules)
        var n25 = new N25Blocks { tutorial = true, meleeFire = c25 && !n26, pulseCharge = c25 && !n26 };
        if (seeds <= 0)
        {
            var t = System.Diagnostics.Stopwatch.StartNew();
            TutorialRun run = RunTutorialBot(cfg, map, seed, cap, n25, new N24Blocks { campaign = true, chargeFist = c25 }, n26b);
            foreach (string l in run.log) Console.WriteLine(l);
            Console.WriteLine($"tutorial lesson-clear order ({run.lessons.Count} lessons):");
            foreach (string l in run.lessons) Console.WriteLine("  " + l);
            Campaign24 c = run.campaign;
            bool all = c != null && c.clearSteps.Count == level.rooms.Count;
            for (int i = 0; i < level.rooms.Count; i++)
            {
                bool cleared = c != null && i < c.clearSteps.Count;
                RoomDef r = level.rooms[i];
                Console.WriteLine($"tutorial stage {i + 1} {r.name} ({r.kind}): {(cleared ? (c.clearSteps[i] * run.sim.step).ToString("0.0", CultureInfo.InvariantCulture) + " s" : "NOT CLEARED")}, lessons {r.lessons.Count}, par {r.par:0} s");
            }
            Console.WriteLine($"tutorial seed {seed}: {(run.finished ? "TUTORIAL CLEARED" : "stopped")} after {run.seconds:0.0} s game time, stage respawns {run.stageRespawns}, deaths {c?.deaths}, retries {c?.retries}, wave top-ups {c?.lessonTopUps}, flow {run.sim.flow}, "
                + $"bot at ({run.sim.mover.position.x:0.0}, {run.sim.mover.position.y:0.0}, {run.sim.mover.position.z:0.0}) in {run.sim.zones.ZoneAt(run.sim.mover.position)}, room {(c?.room + 1)} {c?.phase} ({t.Elapsed.TotalSeconds:0.0} s wall): {(all && run.finished ? "PASS" : "FAIL")}");
            if (!c25) Console.WriteLine("tutorial SHORT (blocked on builder C, design.n25.meleeFire / pulseCharge off): lessons punchWhileFiring and flickCrossed are passed over by the rule (Lesson25.Skip), the fist stage clears on its husks");
            code = all && run.finished ? 0 : 1;
            return;
        }
        // the per-stage numbers over seeds: clear-time medians (par = the median), first try rate of the fights, stage respawns, soft-locks
        int n = level.rooms.Count;
        var times = new List<double>[n];
        for (int i = 0; i < n; i++) times[i] = new List<double>();
        var firstTry = new int[n];
        var reached = new int[n];
        int finished = 0, respawns = 0;
        for (uint sd = 1; sd <= seeds; sd++)
        {
            TutorialRun run = RunTutorialBot(cfg, CampaignMapFrom(path, cfg, out _), sd, cap, n25, new N24Blocks { campaign = true, chargeFist = c25 }, n26b);
            Campaign24 c = run.campaign;
            if (run.finished) finished++;
            respawns += run.stageRespawns;
            for (int i = 0; i < n; i++)
            {
                if (c == null || i >= c.clearSteps.Count) continue;
                reached[i]++;
                times[i].Add(c.clearSteps[i] * run.sim.step);
                if ((i < c.roomDeaths.Count ? c.roomDeaths[i] : 0) == 0) firstTry[i]++;
            }
        }
        bool ok = finished == seeds;
        for (int i = 0; i < n; i++)
        {
            times[i].Sort();
            double med = times[i].Count == 0 ? double.NaN : times[i][times[i].Count / 2];
            RoomDef r = level.rooms[i];
            double rate = 100.0 * firstTry[i] / seeds;
            bool fight = r.kind == "room";
            bool inBand = !fight || rate >= 90; // the tutorial is easier than the levels (60-90): a first-try rate of at least 90 %
            Console.WriteLine($"tutorial stage {i + 1} {r.name} ({r.kind}, {seeds} seeds): median {med:0.0} s (plan par {r.par:0}), cleared {reached[i]}/{seeds}, first-try {firstTry[i]}/{seeds} = {rate:0} %{(fight ? (inBand ? " IN the tutorial band (>= 90)" : " OUT of the tutorial band (>= 90)") : "")}");
        }
        Console.WriteLine($"tutorial over {seeds} seeds: {finished}/{seeds} cleared the whole tutorial, stage respawns {respawns}: {(ok ? "PASS" : "FAIL")}");
        code = ok ? 0 : 1;
    }
}
