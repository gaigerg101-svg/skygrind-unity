// NIGHT29 round 3 (builder R): `walk29 <foundry|iceline|kiln> [--seeds n] [--first n] [--cap seconds] [--trace]`: a pilot (Assets/Scripts/Game/Bot/Walk29Pilot.cs: scripted movement rooms,
// the autopilot's fights) plays the rebuilt level's rooms in order through the REAL sim (the campaign's locked doors, the enemies spawning at the room nodes, hazard damage,
// stand-ins, pickups, the fall cost: every block the design config has ON) on n seeds. Boss rooms are not played (builder B's). Prints per room: cleared k of n, median game
// seconds, deaths, falls (stage respawns), the lessons that cleared; and one summary line per level. The medians are what the plans' `par` and `rank` come from.
// DISPATCH (the lead's Program.cs, ONE line): N29Walk(cmd, args, ref code);
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class Walk29Room
    {
        public string id = "";
        public int cleared;
        public readonly List<double> times = new List<double>();
        public int deaths, falls;
        public readonly Dictionary<string, int> lessons = new Dictionary<string, int>();
    }

    /// <summary>NIGHT30 (builder L): the level-id prefix walk29 loads ("n29": Design/levels/n29-NAME.json); walk30 (Harness.N30.Levels.cs) sets "n30" and calls N29Walk.</summary>
    static string walkPrefix = "n29";
    /// <summary>NIGHT31 L: run on each seed's sim right after StartRun (walk31 --kestrel takes the gauntlet away); null otherwise.</summary>
    static Action<SurvivalSim> walkAfterStart;

    static void N29Walk(string cmd, string[] args, ref int code)
    {
        if (cmd != "walk29") return;
        string tagW = walkPrefix == "n29" ? "walk29" : "walk30";
        string name = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "foundry";
        int seeds = 10, first = 1;
        double cap = 900;
        bool trace = Array.IndexOf(args, "--trace") >= 0;
        for (int i = 1; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--first") first = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--cap") cap = double.Parse(args[i + 1], CultureInfo.InvariantCulture);
        }
        GameConfig cfg = Config();
        string path = Path.Combine("Design", "levels", $"{walkPrefix}-{name}.json");
        var json0 = PortData.Obj(PortData.LoadJson(Path.GetFullPath(path)));
        LevelDef level = LevelDef.From(json0);
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        var rooms = new List<Walk29Room>();
        for (int i = 0; i < playable; i++) rooms.Add(new Walk29Room { id = level.rooms[i].id });
        var levelTimes = new List<double>();
        int levelsDone = 0;
        code = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.GoalProvider = () =>
            {
                Campaign24 c = sim.campaign;
                LevelDef lv = sim.campaignLevel;
                if (c == null || c.room < 0) return lv.rooms[0].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn; // into the next room (its own script takes over there)
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            sim.UseInput(new Walk29Pilot(sim, ap));
            var perRoomTime = new double[playable];
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room < playable) { rooms[e.room].cleared++; rooms[e.room].times.Add(e.time); } });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room < playable) rooms[e.room].deaths++; });
            sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room < playable) rooms[e.room].falls++; });
            sim.events.On<LessonEvent>("lessonClear", e => { if (e.room < playable) { rooms[e.room].lessons.TryGetValue(e.kind, out int n); rooms[e.room].lessons[e.kind] = n + 1; } });
            if (trace) sim.events.On<RoomEvent>("roomClear", e => Console.WriteLine($"  [seed {seed}] room {e.room + 1} {e.id} cleared in {e.time:0.0} s at run {sim.run.steps * sim.step:0.0} s"));
            sim.StartRun(seed, null, null);
            walkAfterStart?.Invoke(sim); // NIGHT31 L: walk31's loadout change (Harness.N31.Levels.cs); null for walk29 / walk30
            int max = MovementMath.StepsFor(cap, sim.step);
            int st = 0;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                Campaign24 c = sim.campaign;
                if (c != null && c.clearSteps.Count >= playable) break;
            }
            bool done = sim.campaign != null && sim.campaign.clearSteps.Count >= playable;
            if (done) { levelsDone++; levelTimes.Add(st * sim.step); }
            else
            {
                Vec3 p = sim.mover.position;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0} s in room {(sim.campaign != null ? sim.campaign.room + 1 : 0)} ({(sim.campaign != null && sim.campaign.room >= 0 ? level.rooms[sim.campaign.room].id : "?")}) {sim.campaign?.phase}, at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), {sim.campaign?.clearSteps.Count} rooms cleared, flow {sim.flow}, state {sim.mover.state} grounded {sim.mover.grounded}, alive {sim.enemies.AliveCount}, unsticks {(sim.input as Walk29Pilot)?.unsticks}");
            }
            ap.Detach();
        }
        foreach (Walk29Room r in rooms)
        {
            r.times.Sort();
            double med = r.times.Count == 0 ? double.NaN : r.times[r.times.Count / 2];
            string ls = r.lessons.Count == 0 ? "none" : string.Join(", ", r.lessons.Select(kv => $"{kv.Key} x{kv.Value}"));
            Console.WriteLine($"{tagW} {name} {r.id}: cleared {r.cleared}/{seeds}, median {med:0.0} s (min {(r.times.Count > 0 ? r.times[0] : double.NaN):0.0}, max {(r.times.Count > 0 ? r.times[r.times.Count - 1] : double.NaN):0.0}), deaths {r.deaths}, falls {r.falls}, lessons: {ls}");
        }
        levelTimes.Sort();
        double lm = levelTimes.Count == 0 ? double.NaN : levelTimes[levelTimes.Count / 2];
        Console.WriteLine($"{tagW} {name} SUMMARY: all {playable} playable rooms cleared in {levelsDone} of {seeds} runs, median {lm:0.0} s over the cleared runs (boss rooms excluded)");
        if (levelsDone < seeds) code = 1;
    }
}
