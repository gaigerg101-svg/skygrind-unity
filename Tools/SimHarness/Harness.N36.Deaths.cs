// NIGHT36 builder M (the lead, round 3: "diagnose the a5 CRUCIBLE lava deaths"): `n36move deaths [room-id] [--seeds n]`: the walk pilot on n36-foundry
// as `n36move walk` runs it (wallride35, grapple35, wallgrip36 forced on, devices on), and for every death (roomRetry) in the room (default a5) one line:
// the seed, the run time, where the player was (room-relative lx / lz, y), the mover's state, the last hurt sources and amounts with their positions,
// the last mover events, the nearest enemy, and what the pilot was doing (a script or the autopilot). Diagnosis only.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static int N36DeathsCommand(string[] args)
    {
        string roomId = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : "a5";
        int seeds = 10;
        for (int i = 1; i < args.Length - 1; i++) if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        string path = Path.Combine("Design", "levels", "n36-foundry.json");
        SurvivalSim.Force35Ride = true;
        SurvivalSim.Force35Grapple = true;
        SurvivalSim.Force36Grip = true;
        var causes = new Dictionary<string, int>();
        try
        {
            for (int s = 0; s < seeds; s++)
            {
                uint seed = (uint)(1 + s);
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
                    if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                    return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
                };
                var pilot = new Walk29Pilot(sim, ap);
                sim.UseInput(pilot);
                var hurts = new Queue<string>();
                var moves = new Queue<string>();
                int deaths = 0;
                RoomDef rd = null;
                string Rel(Vec3 p) => rd == null ? $"({p.x:0.0},{p.y:0.00},{p.z:0.0})" : $"lx {p.x - (rd.x0 + rd.x1) / 2:+0.0;-0.0} lz {rd.z1 - p.z:0.0} y {p.y:+0.00;-0.00}";
                sim.events.On<HurtEvent>("hurt", e =>
                {
                    if (e.owner != "player") return;
                    hurts.Enqueue($"t{sim.run.steps * sim.step:0.00} {e.source} -{e.amount:0} @{Rel(sim.mover.position)}{(e.hasFrom ? $" from {Rel(e.from)}" : "")}");
                    if (hurts.Count > 4) hurts.Dequeue();
                });
                sim.events.On<RoomEvent>("roomRetry", e =>
                {
                    if (rd == null || e.id != roomId) return;
                    deaths++;
                    Enemy near = null;
                    double nd = 1e9;
                    foreach (Enemy en in sim.enemies.all)
                    {
                        if (!en.alive) continue;
                        double d = JsMath.Hypot(en.position.x - sim.mover.position.x, en.position.z - sim.mover.position.z);
                        if (d < nd) { nd = d; near = en; }
                    }
                    string last = hurts.Count > 0 ? string.Join(" | ", hurts) : "no hurt";
                    string src = "?";
                    foreach (string h in hurts) src = h.Split(' ')[1];
                    causes.TryGetValue(src, out int n);
                    causes[src] = n + 1;
                    if (deaths <= 4)
                        Console.WriteLine($"  seed {seed} death {deaths} in {roomId} (phase {sim.campaign.phase}, alive {sim.enemies.AliveCount}): at {Rel(sim.mover.position)} state {sim.mover.state}; hurts: {last}; moves: {string.Join(" ", moves)}; nearest enemy {(near != null ? near.kind + " " + nd.ToString("0.0") + " m @" + Rel(near.position) : "none")}");
                    hurts.Clear();
                });
                sim.mover.Events += e =>
                {
                    if (rd == null) return;
                    moves.Enqueue($"{e.type}{(e.reason != null ? ":" + e.reason : "")}@lz{rd.z1 - sim.mover.position.z:0.0}");
                    if (moves.Count > 5) moves.Dequeue();
                };
                bool trace = Array.IndexOf(args, "--trace") >= 0 && s == 0;
                if (trace)
                {
                    sim.events.On<LessonEvent>("lessonClear", e => { if (rd != null) Console.WriteLine($"    t{sim.run.steps * sim.step:0.00} lessonClear {e.kind} #{e.index} at {Rel(sim.mover.position)}"); });
                    sim.events.On<RoomEvent>("roomClear", e => { if (e.id == roomId) Console.WriteLine($"    t{sim.run.steps * sim.step:0.00} roomClear {e.id} {e.time:0.0} s"); });
                    sim.mover.Events += e => { if (rd != null && e.type != MoverEventType.Land) Console.WriteLine($"    t{sim.run.steps * sim.step:0.00} {e.type}{(e.reason != null ? ":" + e.reason : "")} at {Rel(sim.mover.position)}"); };
                }
                sim.StartRun(seed, null, null);
                sim.Devices33Force(true);
                int max = MovementMath.StepsFor(600, sim.step);
                LevelDef lv0 = sim.campaignLevel;
                int playable = lv0.rooms.FindAll(r => r.kind != "boss").Count;
                for (int st = 0; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
                {
                    AutopilotInput.TakeDraft(sim);
                    sim.Step();
                    Campaign24 c = sim.campaign;
                    rd = c != null && c.room >= 0 && lv0.rooms[c.room].id == roomId ? lv0.rooms[c.room] : null;
                    if (trace && rd != null && st % 120 == 0) Console.WriteLine($"    t{sim.run.steps * sim.step:0.00} at {Rel(sim.mover.position)} {sim.mover.state} speed {sim.mover.Speed:0.0} phase {c.phase}");
                    if (c != null && c.clearSteps.Count >= playable) break;
                }
                Console.WriteLine($"seed {seed}: {deaths} deaths in {roomId}");
                ap.Detach();
            }
        }
        finally
        {
            SurvivalSim.Force35Ride = null;
            SurvivalSim.Force35Grapple = null;
            SurvivalSim.Force36Grip = null;
        }
        var parts = new List<string>();
        foreach (var kv in causes) parts.Add($"{kv.Key} {kv.Value}");
        Console.WriteLine($"n36move deaths {roomId}: last hurt source before each death: {string.Join(", ", parts)}");
        return 0;
    }
}
