// NIGHT38 builder F (NIGHT38-BRIEF card F): the checks of builder F's two levels (n38-foundry's new rooms b1 / b2; n38-finale, rebuilt whole).
//   n38f walk <foundry|finale> [--seeds N] [--first N] [--from ROOM] [--to ROOM] [--cap S] [--trace] [--ride38]
//       The walk pilot (Assets/Scripts/Game/Bot/Walk38Foundry.cs + Walk38Finale.cs: a wrapper round Walk29Pilot) plays the level's rooms in order through
//       the REAL sim (campaign mode, arena36 flow, the doors free so the run may start in a later room) on N seeds: the fights by the autopilot, each gate by
//       its scripted verb. --from: the run starts at that room's spawn (default: n38-foundry b1, the first new room; n38-finale x1); --to: the last room
//       played (default: n38-foundry b2; n38-finale x8, the rest before the boss). Prints per room: cleared k of N, the median seconds, the deaths
//       (room retries), the pit falls, the gates the pilot crossed; one summary line; exit 1 unless every room cleared on every seed.
// The plan checks are node Tools/night/n36-jobs.cjs and levelgen (bash Tools/night/n38f-regen.sh); the gates are builder E's checker (`n38 gates`).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N38Foundry(string cmd, string[] args, ref int code)
    {
        if (cmd != "n38f") return;
        if (args.Length < 2 || args[1] != "walk") { Console.WriteLine("n38f walk <foundry|finale> [--seeds N] [--first N] [--from ROOM] [--to ROOM] [--cap S] [--trace]"); code = 2; return; }
        string name = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : "foundry";
        int seeds = 5, first = 1;
        double cap = 600;
        string from = name == "foundry" ? "b1" : "x1", to = name == "foundry" ? "b2" : "x8";
        bool trace = Array.IndexOf(args, "--trace") >= 0, ride38 = Array.IndexOf(args, "--ride38") >= 0;
        for (int i = 1; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--first") first = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--cap") cap = double.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--from") from = args[i + 1];
            if (args[i] == "--to") to = args[i + 1];
        }
        code = N38fWalk(name, seeds, first, from, to, cap, trace, ride38);
    }

    static int N38fWalk(string name, int seeds, int first, string fromId, string toId, double cap, bool trace, bool ride38)
    {
        GameConfig cfg = Config();
        string path = Path.GetFullPath(Path.Combine("Design", "levels", $"n38-{name}.json"));
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int a = level.rooms.FindIndex(r => r.id == fromId), b = level.rooms.FindIndex(r => r.id == toId);
        if (a < 0 || b < a) { Console.WriteLine($"n38f walk {name}: rooms {fromId}..{toId} not in the level"); return 2; }
        var clears = new Dictionary<int, List<double>>();
        var deaths = new int[level.rooms.Count];
        var falls = new int[level.rooms.Count];
        var crossedAll = new Dictionary<string, int>();
        int runsDone = 0, rides = 0;
        var runTimes = new List<double>();
        Console.WriteLine($"n38f walk n38-{name}: rooms {fromId}..{toId}, {seeds} seeds from {first}; ride38 {(ride38 ? "ON (--ride38)" : "off")}, the level row on, wallride35 {cfg.Design("n35.wallride35.enabled")}");
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
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
                if (c == null || c.room < 0) return lv.rooms[a].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            var pilot = new Walk38Pilot(sim, ap);
            sim.UseInput(pilot);
            // the level's own row on for the run (foundry38 / finale38, + ride38 with --ride38): builder M's noride flag reaches the Mover only while a NIGHT38
            // block is on (SurvivalSim.N38.Ride.cs), as it will in the campaign with the row on (round 2: x7's noride landing face was ridden with every block off)
            sim.n38Override = N38Blocks.FromBits((name == "foundry" ? 2 : 64) | (ride38 ? 1 : 0));
            int st = 0;
            var enterStep = new Dictionary<int, int>();
            sim.events.On<RoomEvent>("roomEnter", e => { if (!enterStep.ContainsKey(e.room)) enterStep[e.room] = st; if (trace) Console.WriteLine($"    {st * sim.step:0.00} enter {e.id}"); });
            sim.events.On<RoomEvent>("roomClear", e =>
            {
                if (e.room < a || e.room > b) return;
                if (!clears.TryGetValue(e.room, out var l)) clears[e.room] = l = new List<double>();
                l.Add(enterStep.TryGetValue(e.room, out int es) ? (st - es) * sim.step : e.time);
                if (trace) Console.WriteLine($"    {st * sim.step:0.00} clear {e.id}");
            });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room >= a && e.room <= b) deaths[e.room]++; });
            sim.StartRun(seed, null, null);
            int max = MovementMath.StepsFor(cap, sim.step);
            bool done = false, placed = a == 0, doorFreeOn = a > 0;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                // the start room: once the campaign runs (room 0 entered), the doors free (Campaign24.SetDoorFree29: a room ahead is entered when the feet stand
                // in its zone) and the player put at the start room's spawn (Level36Flow's way past the rooms this walk does not play)
                if (!placed && sim.campaign != null && sim.campaign.room >= 0)
                {
                    sim.campaign.SetDoorFree29(true);
                    sim.Respawn(level.rooms[a].spawn, level.rooms[a].spawnYawDeg);
                    placed = true;
                }
                // round 2: once the start room is entered the doors go back to the game's rule (a room's door ahead stays shut until it clears: with the
                // doors free the bot flew THE GANTRY's landing straight into the next room and the gantry never cleared)
                if (placed && doorFreeOn && sim.campaign != null && sim.campaign.room == a) { sim.campaign.SetDoorFree29(false); doorFreeOn = false; }
                if (trace && st % MovementMath.StepsFor(0.25, sim.step) == 0)
                {
                    Campaign24 c = sim.campaign;
                    Vec3 p = sim.mover.position;
                    string rid = c != null && c.room >= 0 ? level.rooms[c.room].id : "?";
                    RoomDef rd = c != null && c.room >= 0 ? level.rooms[c.room] : null;
                    double lz = rd != null ? rd.z1 - p.z : 0, lx = rd != null ? p.x - (rd.x0 + rd.x1) / 2 : 0;
                    Console.WriteLine($"    {st * sim.step:0.00} {rid} {c?.phase} lx {lx:0.00} lz {lz:0.00} y {p.y:0.00} v ({sim.mover.velocity.x:0.0},{sim.mover.velocity.y:0.0},{sim.mover.velocity.z:0.0}) {sim.mover.state} g {sim.mover.grounded} alive {sim.enemies.AliveCount} wpn {sim.loadout.Weapon}");
                }
                if (clears.ContainsKey(b) && clears[b].Count > runsDone) { done = true; break; }
            }
            for (int r = a; r <= b; r++) falls[r] += 0;
            foreach (var kv in pilot.crossed) { crossedAll.TryGetValue(kv.Key, out int n); crossedAll[kv.Key] = n + kv.Value; }
            rides += pilot.rides;
            if (done) { runsDone++; runTimes.Add(st * sim.step); }
            else
            {
                Vec3 p = sim.mover.position;
                Campaign24 c = sim.campaign;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0.0} s in {(c != null && c.room >= 0 ? level.rooms[c.room].id : "?")} {c?.phase} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), state {sim.mover.state}, alive {sim.enemies.AliveCount}, pit falls {pilot.pitFalls}");
            }
            Console.WriteLine($"  seed {seed}: {(done ? "through" : "stopped")}; pit falls {pilot.pitFalls}, rides {pilot.rides}, gates crossed [{string.Join(", ", pilot.crossed.Select(kv => kv.Key + " x" + kv.Value))}]");
            ap.Detach();
        }
        for (int r = a; r <= b; r++)
        {
            var l = clears.TryGetValue(r, out var x) ? x : new List<double>();
            l.Sort();
            string med = l.Count == 0 ? "-" : l[l.Count / 2].ToString("0.0", CultureInfo.InvariantCulture);
            crossedAll.TryGetValue(level.rooms[r].id, out int cr);
            Console.WriteLine($"n38f walk {name} {level.rooms[r].id} ({level.rooms[r].name}): cleared {l.Count}/{seeds}, median {med} s, deaths {deaths[r]}, gate crossings by the pilot {cr}");
        }
        runTimes.Sort();
        bool ok = runsDone == seeds;
        Console.WriteLine($"n38f walk {name}: {(ok ? "GREEN" : "RED")} ({runsDone}/{seeds} runs through {fromId}..{toId}; {rides} rides started)");
        return ok ? 0 : 1;
    }
}
