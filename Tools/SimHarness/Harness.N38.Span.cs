// NIGHT38 builder O2 (NIGHT38.md L4): level 4 rebuilt as n38-span (Tools/night/gen-n38-span.cjs). One entry, N38Span (declared and called in Program.cs).
//   n38span loom [--off38]
//     The level side of THE LOOM on n38-span (Game/SurvivalSim.Loom37.cs runs there while design.n38.span38 is on): Loom37Level, the walk-in flag,
//     the canyon o3 with its 13 anchors and 8 ledges, the swing anchor moving, the loom37 rooms (the flight rooms p2 / p4 join o1-o3: the ride refill and
//     the arrival stop). --off38 (span38 off): THE LOOM's level side does not run on n38-span (it stays n37-span's).
//   n38span walk [--seeds n] [--first n] [--cap s] [--trace]
//     The walk pilot (Bot/Walk29Pilot + Bot/Walk38Span.cs: the arenas the autopilot's, the flight rooms scripted from the rooms' pull38 rows, o1 / o2
//     the span pilot) plays n38-span's rooms in order through the real sim (span38 and the n37 boss rows on) on n seeds; per room: cleared k of n, the
//     median seconds, deaths, falls; the summary line; the flight script's pulls / ropes / gates / restarts.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N38Span(string cmd, string[] args, ref int code)
    {
        if (cmd != "n38span") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "loom";
        int fails = 0;
        if (what == "loom") fails += Span38LoomCheck(Array.IndexOf(args, "--off38") >= 0);
        else if (what == "walk") fails += Span38Walk(args);
        else if (what == "order") fails += Span38Order();
        else if (what == "entrance") fails += Span38Entrance();
        else { Console.WriteLine("usage: n38span loom [--off38] | n38span walk [--seeds n] [--first n] [--cap s] [--trace]"); fails = 1; }
        Console.WriteLine(fails == 0 ? "n38span: GREEN" : $"n38span: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static string Span38Path => Path.Combine(MapGenRepo, "Design", "levels", "n38-span.json");

    /// <summary>`n38span order`: the campaign order the design config gives (level 4 n38-span while design.n38.span38 is on), and NIGHT37's with
    /// every n38 block forced off (--off38: n37-span); a design run on n38-span has THE LOOM's level side on (no override: the design's bit).</summary>
    static int Span38Order()
    {
        GameConfig cfg = Config();
        bool on = cfg.Design("n38.span38.enabled");
        bool was = N38Blocks.ForceOff;
        List<string> o, o0;
        try
        {
            N38Blocks.ForceOff = false;
            o = Curriculum25.Order(cfg, N25Blocks.From(cfg));
            N38Blocks.ForceOff = true;
            o0 = Curriculum25.Order(cfg, N25Blocks.From(cfg));
        }
        finally { N38Blocks.ForceOff = was; }
        Console.WriteLine($"design.n38.span38 {on}: order {string.Join(", ", o)}");
        Console.WriteLine($"--off38: order {string.Join(", ", o0)}");
        bool ok = o.Count == o0.Count && o0.IndexOf("n37-span") == 3 && o.IndexOf(on ? "n38-span" : "n37-span") == 3;
        for (int i = 0; i < o.Count; i++) if (i != 3 && o[i] != o0[i] && !o[i].StartsWith("n38-")) ok = false; // only n38 rows differ
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Span38Path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(1, null, null);
        Console.WriteLine($"a design run on n38-span: span38 {sim.n38.span38}, Loom37Level {sim.Loom37Level}");
        ok &= sim.Loom37Level == (on && sim.n37.loom37);
        Console.WriteLine($"  order: {(ok ? "PASS" : "FAIL")}");
        return ok ? 0 : 1;
    }

    /// <summary>A campaign run on n38-span: the n37 boss rows (boss37, loom37) on, span38 on (or every n38 block off).</summary>
    static SurvivalSim Span38Sim(uint seed, bool span38)
    {
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Span38Path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        sim.n37Override = new N37Blocks { boss37 = true, loom37 = true };
        sim.n38Override = span38 ? new N38Blocks { span38 = true } : N38Blocks.Off;
        sim.setup.mode = Campaign24.Mode;
        return sim;
    }

    /// <summary>`n38span entrance`: THE LOOM's fight begun on n38-span (the player on the canyon's start ledge, BossRoomBegin) emits boss38Entrance
    /// once with the boss id (builder H's juice38 hook), a leg restart does not emit it again, and with every n38 block forced off it is not emitted.</summary>
    static int Span38Entrance()
    {
        int fails = 0;
        foreach (bool off in new[] { false, true })
        {
            bool was = N38Blocks.ForceOff;
            try
            {
                N38Blocks.ForceOff = off;
                SurvivalSim sim = Span38Sim(1, true);
                sim.StartRun(1, null, null);
                sim.husks.frozen = true;
                string payload = null;
                sim.events.On("boss38Entrance", o => payload = o as string);
                var start = sim.loom37Ledges.Find(l => l.id == "start");
                if (start == null) { Console.WriteLine("entrance: RED: no canyon start ledge (Loom37Level " + sim.Loom37Level + ")"); fails++; continue; }
                sim.Respawn(new Vec3(start.at.x, 0.05, start.at.z), 0);
                bool began = sim.BossRoomBegin("loom37", out _, out _);
                int first = sim.events.Emitted.TryGetValue("boss38Entrance", out int k) ? k : 0;
                sim.BossRoomBegin("loom37", out _, out _); // a leg restart (a fall): the fight goes on, no second entrance
                int second = sim.events.Emitted.TryGetValue("boss38Entrance", out int k2) ? k2 : 0;
                bool ok = began && (off ? first == 0 && second == 0 : first == 1 && second == 1 && payload == sim.loom37?.p.name);
                Console.WriteLine($"entrance{(off ? " (--off38)" : "")}: begun {began}, boss38Entrance {first} then {second} after a restart, payload {payload ?? "none"}: {(ok ? "PASS" : "FAIL")}");
                if (!ok) fails++;
            }
            finally { N38Blocks.ForceOff = was; }
        }
        return fails;
    }

    static int Span38LoomCheck(bool off)
    {
        SurvivalSim sim = Span38Sim(1, !off);
        sim.StartRun(1, null, null);
        sim.husks.frozen = true;
        int live = 0;
        foreach (var a in sim.loom37Anchors) if (a.node < sim.map.grappleNodes.Count && Vec3.DistanceTo(sim.map.grappleNodes[a.node], a.at) < 1e-9) live++;
        var rooms = new List<string>();
        foreach (RoomDef r in sim.campaignLevel.rooms) if (sim.Loom37InRoom(new Vec3((r.x0 + r.x1) / 2, 0, (r.z0 + r.z1) / 2))) rooms.Add(r.id);
        Console.WriteLine($"level: {sim.campaignLevel.id}, {sim.map.grappleNodes.Count} grapple nodes, span38 {sim.n38.span38}, Loom37Level {sim.Loom37Level}, walk-in {sim.boss37WalkInHere}, "
            + $"canyon {sim.loom37CanyonId}, {sim.loom37Anchors.Count} anchors ({live} live), {sim.loom37Ledges.Count} ledges, swing node {sim.loom37SwingNode}, loom37 rooms [{string.Join(", ", rooms)}]");
        if (off)
        {
            bool ok0 = !sim.Loom37Level && !sim.boss37WalkInHere && sim.loom37Anchors.Count == 0 && rooms.Count == 0;
            Console.WriteLine($"  --off38: THE LOOM's level side does not run on n38-span: {(ok0 ? "PASS" : "FAIL")}");
            return ok0 ? 0 : 1;
        }
        bool ok = sim.Loom37Level && sim.boss37WalkInHere && sim.loom37CanyonId == "o3" && sim.loom37Anchors.Count == 13 && live == 13 && sim.loom37Ledges.Count == 8
            && sim.loom37SwingNode >= 0 && rooms.SequenceEqual(new[] { "p2", "p4", "o1", "o2", "o3" });
        Vec3 s0 = sim.map.grappleNodes[sim.loom37SwingNode];
        for (int i = 0; i < 96; i++) sim.Step();
        Vec3 s1 = sim.map.grappleNodes[sim.loom37SwingNode];
        Console.WriteLine($"  swing anchor moved {Vec3.DistanceTo(s0, s1):0.00} m in 0.8 s");
        ok &= Vec3.DistanceTo(s0, s1) > 3;
        Console.WriteLine($"  level side on n38-span: {(ok ? "PASS" : "FAIL")}");
        return ok ? 0 : 1;
    }

    static int Span38Walk(string[] args)
    {
        int seeds = 10, first = 1;
        double cap = 900;
        bool trace = Array.IndexOf(args, "--trace") >= 0;
        for (int i = 1; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--first") first = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--cap") cap = double.Parse(args[i + 1], CultureInfo.InvariantCulture);
        }
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(Span38Path)));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        var rooms = new List<Walk29Room>();
        for (int i = 0; i < playable; i++) rooms.Add(new Walk29Room { id = level.rooms[i].id });
        var levelTimes = new List<double>();
        int levelsDone = 0, pulls = 0, ropes = 0, gates = 0, restarts = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            SurvivalSim sim = Span38Sim(seed, true);
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
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room < playable) { rooms[e.room].cleared++; rooms[e.room].times.Add(e.time); } });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room < playable) rooms[e.room].deaths++; });
            sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room < playable) rooms[e.room].falls++; });
            sim.events.On<LessonEvent>("lessonClear", e => { if (e.room < playable) { rooms[e.room].lessons.TryGetValue(e.kind, out int n); rooms[e.room].lessons[e.kind] = n + 1; } });
            if (trace) sim.events.On<RoomEvent>("roomClear", e => Console.WriteLine($"  [seed {seed}] room {e.room + 1} {e.id} cleared in {e.time:0.0} s at run {sim.run.steps * sim.step:0.0} s"));
            sim.StartRun(seed, null, null);
            int max = MovementMath.StepsFor(cap, sim.step);
            int st = 0;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (trace && st % 30 == 0 && sim.campaign != null && sim.campaign.room >= 0 && (level.rooms[sim.campaign.room].id == "p2" || level.rooms[sim.campaign.room].id == "p4" || level.rooms[sim.campaign.room].id == "o1" || level.rooms[sim.campaign.room].id == "o2"))
                {
                    Vec3 q = sim.mover.position;
                    Console.WriteLine($"    [seed {seed}] {st * sim.step:0.0}s {level.rooms[sim.campaign.room].id} ({q.x:0.0}, {q.y:0.0}, {q.z:0.0}) {sim.mover.state} rope {sim.mover.rope.phase} charges {sim.grappleCharges32} {pilot.State38}");
                }
                if (sim.campaign != null && sim.campaign.clearSteps.Count >= playable) break;
            }
            bool done = sim.campaign != null && sim.campaign.clearSteps.Count >= playable;
            if (done) { levelsDone++; levelTimes.Add(st * sim.step); }
            else
            {
                Vec3 p = sim.mover.position;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0} s in room {(sim.campaign != null ? sim.campaign.room + 1 : 0)} ({(sim.campaign != null && sim.campaign.room >= 0 ? level.rooms[sim.campaign.room].id : "?")}) {sim.campaign?.phase}, at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), {sim.campaign?.clearSteps.Count} rooms cleared, state {sim.mover.state}, alive {sim.enemies.AliveCount}, unsticks {pilot.unsticks}");
            }
            pulls += pilot.pulls38; ropes += pilot.ropes38; gates += pilot.gatesCrossed38; restarts += pilot.restarts38;
            ap.Detach();
        }
        foreach (Walk29Room r in rooms)
        {
            r.times.Sort();
            double med = r.times.Count == 0 ? double.NaN : r.times[r.times.Count / 2];
            string ls = r.lessons.Count == 0 ? "none" : string.Join(", ", r.lessons.Select(kv => $"{kv.Key} x{kv.Value}"));
            Console.WriteLine($"n38span walk {r.id}: cleared {r.cleared}/{seeds}, median {med:0.0} s (min {(r.times.Count > 0 ? r.times[0] : double.NaN):0.0}, max {(r.times.Count > 0 ? r.times[r.times.Count - 1] : double.NaN):0.0}), deaths {r.deaths}, falls {r.falls}, lessons: {ls}");
        }
        levelTimes.Sort();
        double lm = levelTimes.Count == 0 ? double.NaN : levelTimes[levelTimes.Count / 2];
        Console.WriteLine($"n38span walk: the flight script pressed {pulls} pulls, saw {ropes} ropes, crossed {gates} gates, restarted {restarts} times (falls)");
        Console.WriteLine($"n38span walk SUMMARY: all {playable} playable rooms cleared in {levelsDone} of {seeds} runs, median {lm:0.0} s over the cleared runs (boss room excluded)");
        return levelsDone < seeds ? 1 : 0;
    }
}
