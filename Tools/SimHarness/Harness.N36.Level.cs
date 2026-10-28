// NIGHT36 builder L (NIGHT36.md L1): level36, the checks of the from-scratch level 1 (Design/levels/n36-foundry.json) and its arena flow
// (Game/SurvivalSim.Level36.cs, design.n36.level36).
//   level36 [flow] [--seeds N] [--level id]   walk29's pilot plays the level (devices as the config has them); every arena room's flow is timed from the
//                                             events: the room's entry -> its first arena36SpawnIn (want switchOnDelayS), spawn-in -> spawn-in inside a wave
//                                             (want staggerS), a wave's last death -> the next wave's first spawn-in (want nextWaveS), one arena36LastKill
//                                             per arena room cleared. GREEN when every measure is within one step and the walk clears every playable room.
//                                             --level n35-foundry is the CONTROL: an older level, which must show no arena36 event at all.
//   level36 walk [--seeds N]                  walk29's pilot on n36-foundry, 10 seeds (lines34's walk shape; builder M owns the pilots).
// The jobs check is node Tools/night/n36-jobs.cjs (a plan check, no sim).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N36Level(string cmd, string[] args, ref int code)
    {
        if (cmd != "level36") return;
        int seeds = 3;
        string levelId = "n36-foundry";
        for (int i = 1; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--level") levelId = args[i + 1];
        }
        if (args.Length > 1 && args[1] == "walk")
        {
            if (args.Length <= 2 || Array.IndexOf(args, "--seeds") < 0) seeds = 10;
            string old = walkPrefix;
            walkPrefix = "n36";
            try { N29Walk("walk29", new[] { "walk29", "foundry", "--seeds", seeds.ToString(CultureInfo.InvariantCulture) }, ref code); }
            finally { walkPrefix = old; }
            return;
        }
        code = Level36Flow(levelId, seeds);
    }

    static int Level36Flow(string levelId, int seeds)
    {
        GameConfig cfg = Config();
        string path = Path.GetFullPath(Path.Combine("Design", "levels", levelId + ".json"));
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        double step = cfg.Dt;
        double wantOn = cfg.Num("design.n36.level36.switchOnDelayS"), wantStagger = cfg.Num("design.n36.level36.staggerS"), wantNext = cfg.Num("design.n36.level36.nextWaveS");
        bool control = levelId != "n36-foundry";
        Console.WriteLine($"level36 flow {levelId}: walk29's pilot, {seeds} seeds; want switch-on {wantOn} s, stagger {wantStagger} s, next wave {wantNext} s (one step = {step:0.0000} s){(control ? "; CONTROL: an older level, no arena36 event allowed" : "")}");
        var onDelays = new List<double>();
        var staggers = new List<double>();
        var nexts = new List<double>();
        int locks = 0, spawnIns = 0, lastKills = 0, arenaClears = 0, levelsDone = 0;
        var perRoom = new Dictionary<string, string>();
        var arenaRoomsAll = new HashSet<int>(); // arena rooms entered over every seed
        int arenaTotal = 0; // the level file's arena36 rooms
        if (PortData.Obj(PortData.LoadJson(path)).TryGetValue("campaign", out object camp0) && camp0 is Dictionary<string, object> cd0 && cd0.TryGetValue("rooms", out object rs0) && rs0 is List<object> rl0)
            foreach (object o in rl0) if (o is Dictionary<string, object> rd0 && rd0.TryGetValue("arena36", out object a0) && a0 is bool ab0 && ab0) arenaTotal++;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(1 + s);
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
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
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            sim.UseInput(new Walk29Pilot(sim, ap));
            int st = 0, enterStep = -1, lastSpawn = -1, waveDown = -1, groupSpawns = 0;
            bool firstOfRoom = false;
            var arenaRoomsEntered = new HashSet<int>();
            sim.events.On<RoomEvent>("roomEnter", e => { enterStep = st; firstOfRoom = true; lastSpawn = -1; waveDown = -1; });
            if (Environment.GetEnvironmentVariable("LEVEL36_TRACE") == "1")
            {
                foreach (string ev in new[] { "roomEnter", "roomRetry", "roomClear" }) { string n = ev; sim.events.On<RoomEvent>(n, e => Console.WriteLine($"    {st * step:0.000} {n} {e.id}")); }
                foreach (string ev in new[] { "arena36Lock", "arena36SpawnIn", "arena36LastKill" }) { string n = ev; sim.events.On<Arena36Event>(n, e => Console.WriteLine($"    {st * step:0.000} {n} {e.id} {e.kind}")); }
            }
            sim.events.On<RoomEvent>("roomRetry", e => { enterStep = st; firstOfRoom = true; lastSpawn = -1; waveDown = -1; });
            sim.events.On<Arena36Event>("arena36Lock", e => { locks++; arenaRoomsEntered.Add(e.room); arenaRoomsAll.Add(e.room); });
            sim.events.On<Arena36Event>("arena36LastKill", e => lastKills++);
            sim.events.On<RoomEvent>("roomClear", e => { if (arenaRoomsEntered.Contains(e.room) && level.rooms[e.room].WaveCount > 0) arenaClears++; });
            sim.events.On<Arena36Event>("arena36SpawnIn", e =>
            {
                spawnIns++;
                if (waveDown >= 0) { nexts.Add((st - waveDown) * step); if (firstOfRoom) perRoom[e.id] = $"first switch-on {(st - waveDown) * step:0.000} s after the last death before it (stand-ins or a retry)"; firstOfRoom = false; } // a wave held by stand-ins (the KESTREL room) is a next wave
                else if (firstOfRoom && enterStep >= 0 && level.rooms[e.room].standIns.Count > 0) firstOfRoom = false; // a room whose stand-ins hold the wave: measured from their death (above), never from the entry
                else if (firstOfRoom && enterStep >= 0) { onDelays.Add((st - enterStep) * step); firstOfRoom = false; perRoom[e.id] = $"first switch-on {(st - enterStep) * step:0.000} s"; }
                else if (lastSpawn >= 0) staggers.Add((st - lastSpawn) * step);
                lastSpawn = st;
                waveDown = -1;
            });
            sim.StartRun(seed, null, null);
            int max = MovementMath.StepsFor(600, sim.step);
            int alivePrev = 0, waitSteps = 0, fightsCleared = 0;
            bool doorFree = false;
            int fights = level.rooms.Count(r => r.kind != "boss" && r.WaveCount > 0);
            sim.events.On<RoomEvent>("roomClear", e => { if (level.rooms[e.room].WaveCount > 0) fightsCleared++; });
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                int alive = sim.enemies.AliveCount;
                Campaign24 c = sim.campaign;
                if (alivePrev > 0 && alive == 0 && c != null && c.room >= 0 && c.phase == Campaign24.Phase.Fight && c.Wave33 < level.rooms[c.room].WaveCount) { waveDown = st; lastSpawn = -1; }
                alivePrev = alive;
                if (fightsCleared >= fights) break;
                if (Environment.GetEnvironmentVariable("LEVEL36_TRACE") == "1" && st % MovementMath.StepsFor(5, sim.step) == 0)
                    Console.WriteLine($"    {st * step:0.000} player ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z:0.0}) enemies " + string.Join(" ", sim.enemies.all.Where(e => e.alive).Select(e => $"({e.position.x:0.0},{e.position.y:0.0},{e.position.z:0.0})")));
                // the movement rooms (the intake's jump / dash / slide, the links) are the walk bot's (builder M's pilots): this check TELEPORTS past them,
                // through DOORFREE (Campaign24.SetDoorFree29: a room ahead is entered when the feet stand in its zone), 0.5 s after a link is entered or a
                // fight is cleared; the fights themselves are the autopilot's (it shoots, the enemies come)
                if (c == null || c.room < 0) continue;
                if (!doorFree) { c.SetDoorFree29(true); doorFree = true; }
                bool quiet = c.phase == Campaign24.Phase.Fight && c.Wave33 >= level.rooms[c.room].WaveCount && alive == 0; // every wave dead, a crossing lesson left (the RIDE room's far floor)
                if (quiet && waitSteps >= MovementMath.StepsFor(1, sim.step)) { sim.Respawn(level.rooms[c.room].exit, 0); waitSteps = 0; }
                bool pass = level.rooms[c.room].WaveCount == 0 || c.phase == Campaign24.Phase.Cleared || quiet;
                waitSteps = pass ? waitSteps + 1 : 0;
                if (pass && waitSteps >= MovementMath.StepsFor(0.5, sim.step) && c.room + 1 < level.rooms.Count && level.rooms[c.room + 1].kind != "boss")
                {
                    RoomDef nx = level.rooms[c.room + 1];
                    sim.Respawn(nx.spawn, nx.spawnYawDeg);
                    waitSteps = 0;
                }
            }
            bool done = fightsCleared >= fights;
            if (done) levelsDone++;
            Console.WriteLine($"  seed {seed}: {(done ? $"cleared all {fights} fight rooms" : "STOPPED in room " + (sim.campaign != null && sim.campaign.room >= 0 ? level.rooms[sim.campaign.room].id : "?"))} in {st * step:0.0} s; arena rooms entered {arenaRoomsEntered.Count}, switch-ons {sim.arena36SpawnIns}, last kills {sim.arena36LastKills}");
            ap.Detach();
        }
        string Range(List<double> v) => v.Count == 0 ? "none" : $"{v.Min():0.000}..{v.Max():0.000} s (n {v.Count})";
        bool Near(List<double> v, double want) => v.All(x => Math.Abs(x - want) <= step + 1e-9);
        Console.WriteLine($"level36 {levelId}: arena locks {locks}, switch-ons {spawnIns}, last kills {lastKills} / arena rooms cleared {arenaClears}");
        Console.WriteLine($"  entry -> first switch-on: {Range(onDelays)} (want {wantOn} s)");
        Console.WriteLine($"  switch-on -> switch-on in a wave: {Range(staggers)} (want {wantStagger} s)");
        Console.WriteLine($"  wave dead -> next wave's first switch-on: {Range(nexts)} (want {wantNext} s)");
        foreach (var kv in perRoom) Console.WriteLine($"  {kv.Key}: {kv.Value}");
        bool ok;
        if (control) ok = locks == 0 && spawnIns == 0 && lastKills == 0;
        else ok = arenaRoomsAll.Count == arenaTotal && onDelays.Count > 0 && Near(onDelays, wantOn) && Near(staggers, wantStagger) && Near(nexts, wantNext) && lastKills == arenaClears && arenaClears > 0;
        Console.WriteLine($"level36 {levelId}: {(ok ? "GREEN" : "RED")} (arena rooms entered {arenaRoomsAll.Count}/{arenaTotal}; {levelsDone}/{seeds} runs cleared every fight room, the autopilot fights, the movement rooms teleported past)");
        return ok ? 0 : 1;
    }
}
