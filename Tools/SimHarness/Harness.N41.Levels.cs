// NIGHT41 builder R5 (spire41, foundry41; NIGHT41.md R5, INVENTORY A10 / A12): the level rows' checks, declared and called by the lead's
// Program.cs (N41Levels). Every number is measured on the real sim (SurvivalSim.Headless, the campaign on the level's own map file).
//   n41lv walk <spire|foundry> [--off] [--seeds n] [--first n] [--cap s] [--trace]
//       walk29's pilot (Walk29Pilot: the rooms' scripts, the autopilot's fights, Span30Pilot in a room with grapple nodes) through
//       Design/levels/n41-<name>.json with the level's NIGHT41 row ON (sim.n41Override = the design blocks + that bit, set BEFORE StartRun so
//       every older system reads the level as its levelFrom through N41BaseId from the run's first step: NIGHT40's start set, the walk
//       pilots). --off: the levelFrom file (n40-spire / n38-foundry) with the row off: the control. Per room: cleared k of n, the median, deaths,
//       falls, the lessons, and the span pilot's node attaches / crossings there (did the nodes hand the room to Span30Pilot: L-045's hijack).
//   n41lv nodes [spire]
//       the grapple nodes of n41-spire per room, and the verbs reel's pick (FeelReel.N35.Verbs.cs Node35, the same loop: the first node with a
//       spot 10 m from it at the room's floor in clear sight; rows 17 / 22 / 23 without the floor test, row 19 with it), then the pull itself
//       from that spot on the real sim (the ARCLINE owned as the campaign gives it on level 5, aim at the node, a tap): a node attach and the feet
//       within 2 m of the node (row 17's PASS rule) within 3 s.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N41Levels(string cmd, string[] args, ref int code)
    {
        if (cmd != "n41lv") return;
        string what = args.Length > 1 ? args[1] : "";
        if (what == "walk" && args.Length > 2 && args[2] == "foundry") { code = N41LvFoundryWalk(args); return; }
        if (what == "walk") { code = N41LvWalk(args); return; }
        if (what == "nodes") { N41LvTrace = Array.IndexOf(args, "--trace") >= 0; code = N41LvNodes(args); return; }
        Console.WriteLine("n41lv: walk <spire|foundry> [--off] [--seeds n] [--first n] [--cap s] [--trace] | nodes");
        code = 2;
    }

    /// <summary>The level row's bit (N41Blocks order), its levelFrom and levelTo, from the design.</summary>
    static (int bit, string from, string to) N41LvRow(GameConfig cfg, string name)
    {
        string row = name == "spire" ? "spire41" : name == "foundry" ? "foundry41" : throw new ArgumentException("n41lv: spire or foundry, not " + name);
        int bit = Array.IndexOf(N41Blocks.Names, row);
        return (bit, cfg.Str("design.n41." + row + ".levelFrom"), cfg.Str("design.n41." + row + ".levelTo"));
    }

    /// <summary>The design's NIGHT41 blocks (off with --off41) with the level row's bit set to `on`.</summary>
    static N41Blocks N41LvBlocks(GameConfig cfg, int bit, bool on)
    {
        int bits = (N41Blocks.ForceOff ? N41Blocks.Off : N41Blocks.From(cfg)).Bits;
        return N41Blocks.FromBits(on ? bits | (1 << bit) : bits & ~(1 << bit));
    }

    static SurvivalSim N41LvSim(GameConfig cfg, string path, int bit, bool on, uint seed, out AutopilotInput ap, out Walk29Pilot pilot)
    {
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.n41Override = N41LvBlocks(cfg, bit, on);
        var a = new AutopilotInput(sim, "normal", seed);
        a.GoalProvider = () =>
        {
            Campaign24 c = sim.campaign;
            LevelDef lv = sim.campaignLevel;
            if (c == null || c.room < 0) return lv.rooms[0].spawn;
            if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
            return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
        };
        pilot = new Walk29Pilot(sim, a);
        sim.UseInput(pilot);
        ap = a;
        return sim;
    }

    static int N41LvWalk(string[] args)
    {
        string name = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : "spire";
        bool off = Array.IndexOf(args, "--off") >= 0, trace = Array.IndexOf(args, "--trace") >= 0;
        int seeds = 10, first = 1;
        double cap = 900;
        for (int i = 2; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--first") first = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--cap") cap = double.Parse(args[i + 1], CultureInfo.InvariantCulture);
        }
        GameConfig cfg = Config();
        var (bit, from, to) = N41LvRow(cfg, name);
        string id = off ? from : to;
        string path = Path.Combine("Design", "levels", id + ".json");
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        var rooms = new List<Walk29Room>();
        for (int i = 0; i < playable; i++) rooms.Add(new Walk29Room { id = level.rooms[i].id });
        var attachBy = new int[playable];
        var crossBy = new int[playable];
        var levelTimes = new List<double>();
        int done = 0;
        string baseId = "";
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
            SurvivalSim sim = N41LvSim(cfg, path, bit, !off, seed, out AutopilotInput ap, out Walk29Pilot pilot);
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room < playable) { rooms[e.room].cleared++; rooms[e.room].times.Add(e.time); } });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room < playable) rooms[e.room].deaths++; });
            sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room < playable) rooms[e.room].falls++; });
            sim.events.On<LessonEvent>("lessonClear", e => { if (e.room < playable) { rooms[e.room].lessons.TryGetValue(e.kind, out int n); rooms[e.room].lessons[e.kind] = n + 1; } });
            sim.events.On<Arcline21RopeEvent>("ropeAttach", e => { int r = sim.campaign != null ? sim.campaign.room : -1; if (e.node && r >= 0 && r < playable) attachBy[r]++; });
            if (trace) sim.events.On<RoomEvent>("roomClear", e => Console.WriteLine($"  [seed {seed}] room {e.room + 1} {e.id} cleared in {e.time:0.0} s at run {sim.run.steps * sim.step:0.0} s"));
            sim.StartRun(seed, null, null);
            baseId = sim.N41BaseId(sim.campaignLevel != null ? sim.campaignLevel.id : "?");
            int max = MovementMath.StepsFor(cap, sim.step);
            int st = 0, lastCrossed = 0;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                Campaign24 c = sim.campaign;
                if (pilot.span30.crossed != lastCrossed) { if (c != null && c.room >= 0 && c.room < playable) crossBy[c.room] += pilot.span30.crossed - lastCrossed; lastCrossed = pilot.span30.crossed; }
                if (c != null && c.clearSteps.Count >= playable) break;
            }
            bool ok = sim.campaign != null && sim.campaign.clearSteps.Count >= playable;
            if (ok) { done++; levelTimes.Add(st * sim.step); }
            else
            {
                Vec3 p = sim.mover.position;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0} s in room {(sim.campaign != null ? sim.campaign.room + 1 : 0)} ({(sim.campaign != null && sim.campaign.room >= 0 ? level.rooms[sim.campaign.room].id : "?")}) {sim.campaign?.phase}, at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), flow {sim.flow}, alive {sim.enemies.AliveCount}, unsticks {pilot.unsticks}");
            }
            ap.Detach();
        }
        string tag = $"n41lv walk {id}{(off ? " (--off: the row off, the control)" : $" (design.n41 row on; N41BaseId -> {baseId})")}";
        for (int i = 0; i < playable; i++)
        {
            Walk29Room r = rooms[i];
            r.times.Sort();
            double med = r.times.Count == 0 ? double.NaN : r.times[r.times.Count / 2];
            string ls = r.lessons.Count == 0 ? "none" : string.Join(", ", r.lessons.Select(kv => $"{kv.Key} x{kv.Value}"));
            Console.WriteLine($"{tag} {r.id}: cleared {r.cleared}/{seeds}, median {med:0.0} s (min {(r.times.Count > 0 ? r.times[0] : double.NaN):0.0}, max {(r.times.Count > 0 ? r.times[r.times.Count - 1] : double.NaN):0.0}), deaths {r.deaths}, falls {r.falls}, lessons: {ls}; span pilot: node attaches {attachBy[i]}, crossings {crossBy[i]}");
        }
        levelTimes.Sort();
        double lm = levelTimes.Count == 0 ? double.NaN : levelTimes[levelTimes.Count / 2];
        Console.WriteLine($"{tag} SUMMARY: all {playable} playable rooms cleared in {done} of {seeds} runs, median {lm:0.0} s (boss rooms excluded)");
        return done < seeds ? 1 : 0;
    }

    /// <summary>n41lv walk foundry [--off] [--from ROOM] [--to ROOM] [--seeds n] [--first n] [--cap s]: n38f walk's pilot and start-room rule
    /// (Harness.N38.Foundry.cs N38fWalk: Walk38Pilot, the doors free until the start room is entered, rooms b1..b2 by default) on n41-foundry with
    /// foundry41 on (n38-foundry with --off), the foundry38 block on as there (the noride boxes reach the Mover only while a NIGHT38 block is on).</summary>
    static int N41LvFoundryWalk(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        int seeds = 10, first = 1;
        double cap = 600;
        string fromId = "b1", toId = "b2";
        for (int i = 2; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--first") first = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--cap") cap = double.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--from") fromId = args[i + 1];
            if (args[i] == "--to") toId = args[i + 1];
        }
        GameConfig cfg = Config();
        var (bit, from, to) = N41LvRow(cfg, "foundry");
        string id = off ? from : to;
        string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int a = level.rooms.FindIndex(r => r.id == fromId), b = level.rooms.FindIndex(r => r.id == toId);
        if (a < 0 || b < a) { Console.WriteLine($"n41lv walk foundry: rooms {fromId}..{toId} not in {id}"); return 2; }
        var clears = new Dictionary<int, List<double>>();
        var deaths = new int[level.rooms.Count];
        var crossedAll = new Dictionary<string, int>();
        int runsDone = 0, rides = 0, pitFalls = 0;
        string baseId = "";
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
            sim.n38Override = N38Blocks.FromBits(N38Blocks.From(cfg).Bits | 2); // foundry38 on (as n38f walk), the other NIGHT38 rows as designed
            sim.n41Override = N41LvBlocks(cfg, bit, !off);
            int st = 0;
            var enterStep = new Dictionary<int, int>();
            sim.events.On<RoomEvent>("roomEnter", e => { if (!enterStep.ContainsKey(e.room)) enterStep[e.room] = st; });
            sim.events.On<RoomEvent>("roomClear", e =>
            {
                if (e.room < a || e.room > b) return;
                if (!clears.TryGetValue(e.room, out var l)) clears[e.room] = l = new List<double>();
                l.Add(enterStep.TryGetValue(e.room, out int es) ? (st - es) * sim.step : e.time);
            });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room >= a && e.room <= b) deaths[e.room]++; });
            sim.StartRun(seed, null, null);
            baseId = sim.N41BaseId(sim.campaignLevel != null ? sim.campaignLevel.id : "?");
            int max = MovementMath.StepsFor(cap, sim.step);
            bool done = false, placed = a == 0, doorFreeOn = a > 0;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (!placed && sim.campaign != null && sim.campaign.room >= 0)
                {
                    sim.campaign.SetDoorFree29(true);
                    sim.Respawn(level.rooms[a].spawn, level.rooms[a].spawnYawDeg);
                    placed = true;
                }
                if (placed && doorFreeOn && sim.campaign != null && sim.campaign.room == a) { sim.campaign.SetDoorFree29(false); doorFreeOn = false; }
                if (clears.ContainsKey(b) && clears[b].Count > runsDone) { done = true; break; }
            }
            foreach (var kv in pilot.crossed) { crossedAll.TryGetValue(kv.Key, out int n); crossedAll[kv.Key] = n + kv.Value; }
            rides += pilot.rides;
            pitFalls += pilot.pitFalls;
            if (done) runsDone++;
            else
            {
                Vec3 p = sim.mover.position;
                Campaign24 c = sim.campaign;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0.0} s in {(c != null && c.room >= 0 ? level.rooms[c.room].id : "?")} {c?.phase} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), state {sim.mover.state}, alive {sim.enemies.AliveCount}, pit falls {pilot.pitFalls}");
            }
            ap.Detach();
        }
        string tag = $"n41lv walk {id}{(off ? " (--off: the control)" : $" (foundry41 on; N41BaseId -> {baseId})")}";
        for (int r = a; r <= b; r++)
        {
            var l = clears.TryGetValue(r, out var x) ? x : new List<double>();
            l.Sort();
            string med = l.Count == 0 ? "-" : l[l.Count / 2].ToString("0.0", CultureInfo.InvariantCulture);
            crossedAll.TryGetValue(level.rooms[r].id, out int cr);
            Console.WriteLine($"{tag} {level.rooms[r].id}: cleared {l.Count}/{seeds}, median {med} s, deaths {deaths[r]}, gate crossings by the pilot {cr}");
        }
        Console.WriteLine($"{tag} SUMMARY: {runsDone}/{seeds} runs through {fromId}..{toId}; {rides} rides started, pit falls {pitFalls}");
        return runsDone == seeds ? 0 : 1;
    }

    static int N41LvNodes(string[] args)
    {
        GameConfig cfg = Config();
        var (bit, from, to) = N41LvRow(cfg, "spire");
        string path = Path.Combine("Design", "levels", to + ".json");
        SurvivalSim sim = N41LvSim(cfg, path, bit, true, 1, out AutopilotInput ap, out Walk29Pilot _);
        sim.StartRun(1, null, null);
        sim.Step(); // the ARCLINE is owned on the run's first step
        ap.Detach();
        LevelDef lv = sim.campaignLevel;
        Console.WriteLine($"n41lv nodes {lv.id} (N41BaseId -> {sim.N41BaseId(lv.id)}): {sim.map.grappleNodes.Count} nodes; ARCLINE {(sim.arcline21 != null ? (sim.arcline21.Owned ? "owned" : "not owned") : "none")}; n30.span {sim.n30.span}");
        foreach (RoomDef r in lv.rooms)
        {
            var ns = sim.map.grappleNodes.Where(n => n.x >= r.x0 && n.x <= r.x1 && n.z >= r.z0 && n.z <= r.z1).ToList();
            if (ns.Count > 0) Console.WriteLine($"  {r.id}: {ns.Count} nodes {string.Join(" ", ns.Select(n => $"({n.x:0.#}, {n.y:0.#}, {n.z:0.#})"))}");
        }
        int fails = 0;
        foreach (bool floors in new[] { false, true })
        {
            if (!N41LvNode35(sim, floors, out Vec3 node, out Vec3 spot, out string room))
            {
                Console.WriteLine($"  Node35 (floors {floors}, rows {(floors ? "19" : "17 / 22 / 23")}): NO node in sight of a floor spot: RED");
                fails++;
                continue;
            }
            Console.WriteLine($"  Node35 (floors {floors}, rows {(floors ? "19" : "17 / 22 / 23")}): node ({node.x:0.#}, {node.y:0.#}, {node.z:0.#}) in {room}, spot ({spot.x:0.#}, {spot.y:0.#}, {spot.z:0.#})");
        }
        // the pull itself, on a fresh run, from each room's first node's spot (row 17's rule: a node attach and the feet within 2 m of the node in 3 s)
        foreach (RoomDef r in lv.rooms)
        {
            var ns = sim.map.grappleNodes.Where(n => n.x >= r.x0 && n.x <= r.x1 && n.z >= r.z0 && n.z <= r.z1).ToList();
            if (ns.Count == 0) continue;
            foreach (Vec3 node in ns)
                foreach (bool tap in new[] { true, false })
                {
                    string res = N41LvPull(cfg, path, bit, r, node, tap, out bool pass);
                    Console.WriteLine($"  pull {r.id} ({node.x:0.#}, {node.y:0.#}, {node.z:0.#}) {res}: {(pass ? "PASS" : "RED")}");
                    if (!pass) fails++;
                }
        }
        Console.WriteLine(fails == 0 ? "n41lv nodes: GREEN" : $"n41lv nodes: RED ({fails})");
        return fails == 0 ? 0 : 1;
    }

    /// <summary>FeelReel.N35.Verbs.cs Node35, the same loop on the harness's sim (the verbs reel's pick for rows 17 / 19 / 22 / 23).</summary>
    static bool N41LvNode35(SurvivalSim sim, bool floors, out Vec3 node, out Vec3 from, out string room)
    {
        node = from = default;
        room = "";
        foreach (Vec3 nd in sim.map.grappleNodes)
            foreach (RoomDef r in sim.campaignLevel.rooms)
            {
                if (nd.x < r.x0 - 2 || nd.x > r.x1 + 2 || nd.z < r.z0 - 2 || nd.z > r.z1 + 2) continue;
                for (int k = 0; k < 8; k++)
                {
                    double a = k * Math.PI / 4;
                    var f = new Vec3(nd.x + Math.Sin(a) * 10, r.spawn.y + 0.2, nd.z + Math.Cos(a) * 10);
                    if (f.x < r.x0 + 1 || f.x > r.x1 - 1 || f.z < r.z0 + 1 || f.z > r.z1 - 1) continue;
                    var eye = new Vec3(f.x, f.y + 1.6, f.z);
                    Vec3 d = nd - eye;
                    double len = d.Length;
                    if (sim.world.RayIntersect(eye, d / len, out RayHit h) && h.distance < len - 0.5) continue;
                    if (floors && !(sim.world.RayIntersect(new Vec3(f.x, r.spawn.y + 1.0, f.z), new Vec3(0, -1, 0), out RayHit dh) && dh.distance <= 1.6 && dh.normal.y > 0.7)) continue;
                    node = nd;
                    from = f;
                    room = r.id;
                    return true;
                }
            }
        return false;
    }

    /// <summary>One pull: a fresh run, the player put on the floor 10 m from the node with a clear line (the first of 8 bearings), looking at it,
    /// a grapple tap; then nothing held for 3 s. PASS: a node attach and the feet within 2 m of the node.</summary>
    /// <summary>The eye's clearance under the roof over a node a pull must keep (m): the bot's bar for "the pull ends with the room in view" (the
    /// lead's n41-r2 frame f1405: eye 0.2 m under the nave's roof, the whole view the roof's underside). A harness bar, not a game number.</summary>
    const double RoofClear41 = 2.5;
    /// <summary>`n41lv nodes --trace`: each pull's height, speeds and rope phase every 0.05 s.</summary>
    static bool N41LvTrace;

    static string N41LvPull(GameConfig cfg, string path, int bit, RoomDef r, Vec3 node, bool tap, out bool pass)
    {
        pass = false;
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.n41Override = N41LvBlocks(cfg, bit, true);
        var inp = new ScriptedInput();
        sim.UseInput(inp);
        sim.StartRun(4101, null, null);
        sim.husks.frozen = true;
        inp.Set(new InputFrame(), new CombatInput { weapon = -1 });
        sim.Step(); // the ARCLINE is owned on the run's first step (SurvivalSim.Systems30.cs N30SystemsStep: grapple35's level rule)
        if (sim.arcline21 == null || !sim.arcline21.Owned) return "the ARCLINE is not owned on this level";
        Vec3 spot = default;
        bool found = false;
        for (int k = 0; k < 8 && !found; k++)
        {
            double a = k * Math.PI / 4;
            var f = new Vec3(node.x + Math.Sin(a) * 10, r.spawn.y, node.z + Math.Cos(a) * 10);
            if (f.x < r.x0 + 1 || f.x > r.x1 - 1 || f.z < r.z0 + 1 || f.z > r.z1 - 1) continue;
            if (!(sim.world.RayIntersect(new Vec3(f.x, 3.5, f.z), new Vec3(0, -1, 0), out RayHit dh) && dh.normal.y > 0.7)) continue;
            f = new Vec3(f.x, 3.5 - dh.distance, f.z);
            var eye = new Vec3(f.x, f.y + 1.6, f.z);
            Vec3 d = node - eye;
            if (sim.world.RayIntersect(eye, d / d.Length, out RayHit h) && h.distance < d.Length - 0.5) continue;
            spot = f;
            found = true;
        }
        if (!found) return "no floor spot 10 m from the node in clear sight";
        sim.Respawn(spot, 0);
        sim.mover.invulnerable = true;
        int attach = 0;
        sim.events.On<Arcline21RopeEvent>("ropeAttach", e => { if (e.node) attach++; });
        double best = double.MaxValue, eyePeak = double.MinValue;
        // the roof over the node: a ray straight up from it (NIGHT41 lead, n41-r2 f1405: after the nave pull the camera sat at y 13.8 under a 14 m roof)
        double roofY = sim.world.RayIntersect(node, new Vec3(0, 1, 0), out RayHit up) ? node.y + up.distance : double.PositiveInfinity;
        int steps = MovementMath.StepsFor(3.0, sim.step);
        for (int st = 0; st < steps; st++)
        {
            (double yaw, double pitch) = ArclineLook(sim.Eye, node);
            inp.SetLook(yaw, pitch);
            bool settle = st < 12; // a few steps on the floor after the respawn (it lands on the next step)
            // tap: row 17's LB tap (two steps held); hold: row 19's held LB (1.6 s, the verbs reel's hold)
            int holdSteps = tap ? 2 : MovementMath.StepsFor(1.6, sim.step);
            inp.Set(new InputFrame { grapple = new Button { pressed = st == 12, held = !settle && st < 12 + holdSteps } }, new CombatInput { weapon = -1 });
            sim.health.hp = sim.health.max;
            sim.Step();
            Vec3 p = sim.mover.position;
            double dist = (new Vec3(p.x, p.y + 0.9, p.z) - node).Length;
            if (attach > 0) { best = Math.Min(best, Math.Min(dist, (p - node).Length)); eyePeak = Math.Max(eyePeak, sim.Eye.y); }
            if (N41LvTrace && st % 6 == 0 && st >= 12 && st < 150)
                Console.WriteLine($"      st {st} y {p.y:0.00} vy {sim.mover.velocity.y:0.00} h {JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z):0.00} {sim.mover.state} rope {sim.mover.rope.phase} to node {(p - node).Length:0.00}");
        }
        double clear = roofY - eyePeak;
        pass = attach > 0 && best <= 2.0 && clear >= RoofClear41;
        return $"{(tap ? "tap " : "hold")} from ({spot.x:0.#}, {spot.y:0.#}, {spot.z:0.#}): node attaches {attach}, the feet's closest to the node {(attach > 0 ? best.ToString("0.00", CultureInfo.InvariantCulture) : "-")} m, "
            + $"the eye's peak y {(attach > 0 ? eyePeak.ToString("0.00", CultureInfo.InvariantCulture) : "-")}, the roof y {roofY.ToString("0.0", CultureInfo.InvariantCulture)}: clearance {(attach > 0 ? clear.ToString("0.00", CultureInfo.InvariantCulture) : "-")} m (>= {RoofClear41} m)";
    }
}
