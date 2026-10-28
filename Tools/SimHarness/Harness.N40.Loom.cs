// NIGHT40 builder B4 (NIGHT40.md B4 THE LOOM; boss_rules 4): THE LOOM's NIGHT40 checks. One entry, N40Loom (declared and called in Program.cs).
//   n40loom stay [--seeds 10] [--cap 120] [--on] [--trace]
//     THE LOOM's fight (Loom37Pilot from the canyon's start ledge, BossRoomBegin("loom37"), the live design: loom37, span38 on) over seeds 1..N,
//     with loom40 off (today; the default) or forced on (--on). Per seed and in total: the steps (120 Hz) THE LOOM's body centre spends outside
//     o3's floor plan (x/z) and its maximum distance out; the steps its centre is under the canyon floor (design.n40.loom40.floorY) and the
//     deepest. The level: n38-span (off), n40-span when it exists (on), else n38-span. PASS (--on only): 0 steps out, 0 under the floor.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N40Loom(string cmd, string[] args, ref int code)
    {
        if (cmd != "n40loom") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "stay";
        int fails = 0;
        Trace37O = Array.IndexOf(args, "--trace") >= 0;
        Loom40Casual = Array.IndexOf(args, "--casual") >= 0;
        if (what == "stay") fails += Loom40Stay(args);
        else if (what == "level") fails += Loom40LevelCheck();
        else if (what == "practice") fails += Loom40PracticeCheck();
        else if (what == "noride") fails += Loom40NoRideCheck();
        else if (what == "walk") fails += Loom40Walk(args);
        else if (what == "all") { fails += Loom40LevelCheck(); fails += Loom40PracticeCheck(); fails += Loom40NoRideCheck(); fails += Loom40Stay(args); }
        else { Console.WriteLine("usage: n40loom stay [--seeds n] [--cap s] [--on] [--trace] | n40loom level"); fails = 1; }
        Console.WriteLine(fails == 0 ? "n40loom: GREEN" : $"n40loom: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    /// <summary>--casual: the stay runs use Loom37Pilot.casual (L-058).</summary>
    static bool Loom40Casual;

    static string Loom40Level(bool on)
    {
        string n40 = Path.Combine(MapGenRepo, "Design", "levels", "n40-span.json");
        return on && File.Exists(n40) ? n40 : Path.Combine(MapGenRepo, "Design", "levels", "n38-span.json");
    }

    /// <summary>A campaign run on THE LOOM's level with the live design rows (loom37, span38), loom40 forced on or off.</summary>
    public static SurvivalSim Loom40Sim(uint seed, bool on)
    {
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Loom40Level(on))));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        sim.n40Override = on ? new N40Blocks { loom40 = true } : N40Blocks.Off;
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        return sim;
    }

    /// <summary>`n40loom level`: n40-span (Tools/night/gen-n40-span.cjs) with loom40 on: THE LOOM's level side runs (the canyon o3, 13 anchors live, 8 ledges,
    /// the swing node, the loom37 rooms p2 / p4 / o1-o3 as n38-span); p1 carries no STITCHER (no award, no pickup, no prop, no STITCHER lesson); a
    /// campaign run's start set and 20 s on p1 own no nailgun. With loom40 off on the same file: no level side (n40-span is loom40's only).</summary>
    static int Loom40LevelCheck()
    {
        int fails = 0;
        string path = Path.Combine(MapGenRepo, "Design", "levels", "n40-span.json");
        if (!File.Exists(path)) { Console.WriteLine("level: RED: no Design/levels/n40-span.json (bash Tools/night/n40-span-regen.sh)"); return 1; }
        var json = PortData.Obj(PortData.LoadJson(path));
        var p1 = PortData.Obj(PortData.Arr(PortData.Obj(json["campaign"])["rooms"])[0]);
        var prompts = new List<string>();
        foreach (string key in new[] { "lessons", "lessons31" })
            if (p1.ContainsKey(key)) foreach (object lo in PortData.Arr(p1[key])) { var l = PortData.Obj(lo); if (l.ContainsKey("prompt")) prompts.Add((string)l["prompt"]); }
        string lessons = "[" + string.Join(" | ", prompts) + "]";
        bool p1Clean = (string)p1["id"] == "p1" && !p1.ContainsKey("award") && !p1.ContainsKey("pickup") && lessons.IndexOf("STITCHER", StringComparison.Ordinal) < 0;
        string all = File.ReadAllText(path);
        int nailProps = 0;
        for (int i = all.IndexOf("\"nailgun\"", StringComparison.Ordinal); i >= 0; i = all.IndexOf("\"nailgun\"", i + 1, StringComparison.Ordinal)) nailProps++;
        Console.WriteLine($"level: n40-span p1 '{p1["name"]}': award {(p1.ContainsKey("award") ? p1["award"] : "none")}, pickup {(p1.ContainsKey("pickup") ? "yes" : "none")}, lessons {lessons}; \"nailgun\" in the file {nailProps}x: {(p1Clean && nailProps == 0 ? "PASS" : "FAIL")}");
        if (!(p1Clean && nailProps == 0)) fails++;

        SurvivalSim sim = Loom40Sim(1, true);
        int live = 0;
        foreach (var a in sim.loom37Anchors) if (a.node < sim.map.grappleNodes.Count && Vec3.DistanceTo(sim.map.grappleNodes[a.node], a.at) < 1e-9) live++;
        var rooms = new List<string>();
        foreach (RoomDef r in sim.campaignLevel.rooms) if (sim.Loom37InRoom(new Vec3((r.x0 + r.x1) / 2, 0, (r.z0 + r.z1) / 2))) rooms.Add(r.id);
        bool side = sim.campaignLevel.id == "n40-span" && sim.Loom37Level && sim.boss37WalkInHere && sim.loom37CanyonId == "o3" && sim.loom37Anchors.Count == 13 && live == 13
            && sim.loom37Ledges.Count == 8 && sim.loom37SwingNode >= 0 && string.Join(",", rooms) == "p2,p4,o1,o2,o3";
        sim.husks.frozen = false;
        for (int i = 0; i < 20 * 120; i++) sim.Step();
        bool noNails = !sim.loadout.owned.Contains("nailgun");
        Console.WriteLine($"  loom40 ON: level {sim.campaignLevel.id}, Loom37Level {sim.Loom37Level}, canyon {sim.loom37CanyonId}, {sim.loom37Anchors.Count} anchors ({live} live), {sim.loom37Ledges.Count} ledges, swing node {sim.loom37SwingNode}, "
            + $"loom37 rooms [{string.Join(", ", rooms)}]: {(side ? "PASS" : "FAIL")}; owned after 20 s in room {sim.campaign?.Room?.id} [{string.Join(",", sim.loadout.owned)}], no STITCHER: {(noNails ? "PASS" : "FAIL")}");
        if (!side) fails++;
        if (!noNails) fails++;

        SurvivalSim off = Loom40Sim(1, false);
        // Loom40Sim(off) loads n38-span; load n40-span by hand with loom40 off
        GameConfig cfg = Config();
        SurvivalSim s0 = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(json));
        s0.noLab = s0.noRecords = true;
        s0.n40Override = N40Blocks.Off;
        s0.setup.mode = Campaign24.Mode;
        s0.StartRun(1, null, null);
        bool offOk = !s0.Loom37Level && off.Loom37Level && off.campaignLevel.id == "n38-span";
        Console.WriteLine($"  loom40 off: n40-span Loom37Level {s0.Loom37Level} (want False), n38-span Loom37Level {off.Loom37Level} (want True): {(offOk ? "PASS" : "FAIL")}");
        if (!offOk) fails++;
        return fails;
    }

    /// <summary>`n40loom practice`: o2's practice knee on n40-span with loom40 on: from the post, look at the knee and hook it (it latches), let go,
    /// pull to the far thread (node 3): it tears (event loom40Practice "tear"), and is back practiceBackS later. RED control: with loom40 off there
    /// is no practice node.</summary>
    static int Loom40PracticeCheck()
    {
        int fails = 0;
        foreach (bool on in new[] { true, false })
        {
            GameConfig cfg = Config();
            SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(MapGenRepo, "Design", "levels", "n40-span.json")))));
            sim.noLab = sim.noRecords = true;
            sim.n40Override = on ? new N40Blocks { loom40 = true } : N40Blocks.Off;
            sim.n37Override = new N37Blocks { boss37 = true, loom37 = true };
            sim.setup.mode = Campaign24.Mode;
            sim.StartRun(1, null, null);
            sim.husks.frozen = true;
            if (!on)
            {
                bool ok0 = sim.loom40PracticeNode < 0;
                Console.WriteLine($"practice (loom40 off): practice node {sim.loom40PracticeNode} (want -1): {(ok0 ? "PASS" : "FAIL")}");
                if (!ok0) fails++;
                continue;
            }
            var log = new List<string>();
            int st = 0;
            sim.events.On("loom40Practice", o => log.Add($"{st / 120.0:0.00}s {o}"));
            var si = (ScriptedInput)sim.input;
            Vec3 post = new Vec3(-19.5, 4, -228), knee = sim.loom40PracticeAt, far = new Vec3(-19, 9, -242.5);
            sim.Respawn(post, 0);
            sim.mover.invulnerable = true;
            for (int i = 0; i < 30; i++) { sim.Step(); st++; }
            string phase = "hook";
            int phaseSteps = 0;
            for (; st < 12 * 120 && sim.loom40PracticeTears == 0; st++)
            {
                Vec3 eye = sim.mover.position + new Vec3(0, 1.6, 0);
                Vec3 look = phase == "hook" || phase == "let go" ? knee : far;
                var (yaw, pitch) = Loom37Pilot.LookAt(eye, look);
                var m = new InputFrame { yaw = yaw, pitch = pitch };
                phaseSteps++;
                if (phase == "hook") { m.grapple = new Button { held = true, pressed = phaseSteps == 2 }; if (sim.loom40PracticeLatched) { phase = "let go"; phaseSteps = 0; } else if (phaseSteps > 120) { phase = "hook"; phaseSteps = 0; } }
                else if (phase == "let go")
                {
                    // back onto the post (the hook yanked you toward the knee), then the pull away
                    Vec3 me0 = sim.mover.position;
                    double bx = post.x - me0.x, bz = post.z - me0.z, bl = Math.Sqrt(bx * bx + bz * bz);
                    if (bl > 0.4) { Vec3 fw = MovementMath.WishDirection(0, 1, yaw), rt = MovementMath.WishDirection(1, 0, yaw); m.moveZ = (bx * fw.x + bz * fw.z) / bl; m.moveX = (bx * rt.x + bz * rt.z) / bl; }
                    if (phaseSteps > 30 && sim.mover.grounded && sim.mover.rope.phase == RopePhase.None) { phase = "pull"; phaseSteps = 0; }
                }
                else m.grapple = new Button { held = true, pressed = sim.mover.rope.phase == RopePhase.None && phaseSteps % 8 == 2 };
                si.SetLook(yaw, pitch);
                si.Set(m, new CombatInput { weapon = -1 });
                sim.Step();
                if (Trace37O && st % 15 == 0) Console.WriteLine($"    {st / 120.0:0.00}s {phase} rope {sim.mover.rope.phase} anchor ({sim.mover.rope.anchor.x:0.0},{sim.mover.rope.anchor.y:0.0},{sim.mover.rope.anchor.z:0.0}) me ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}) charges {sim.grappleCharges32} latched {sim.loom40PracticeLatched} d {Vec3.DistanceTo(sim.mover.position, knee):0.0}");
            }
            bool tore = sim.loom40PracticeTears == 1 && sim.loom40PracticeTorn;
            bool parked = sim.map.grappleNodes[sim.loom40PracticeNode].y < -500;
            int back = 0;
            for (int i = 0; i < 4 * 120 && sim.loom40PracticeTorn; i++) { si.Set(default, new CombatInput { weapon = -1 }); sim.Step(); st++; back = i + 1; }
            bool backOk = !sim.loom40PracticeTorn && Vec3.DistanceTo(sim.map.grappleNodes[sim.loom40PracticeNode], knee) < 1e-9;
            bool ok = sim.loom40PracticeLatches >= 1 && tore && parked && backOk;
            Console.WriteLine($"practice (loom40 ON): node {sim.loom40PracticeNode} at ({knee.x:0.0},{knee.y:0.0},{knee.z:0.0}); latches {sim.loom40PracticeLatches}, tears {sim.loom40PracticeTears}, parked while torn {parked}, back after {back / 120.0:0.00} s; events [{string.Join("; ", log)}]: {(ok ? "PASS" : "FAIL")}");
            if (!ok) fails++;
        }
        return fails;
    }

    /// <summary>`n40loom noride`: THE LOOM's no-ride phases on n40-span: the fight's begin puts phase 1 (5 walls) on the Mover after the warning,
    /// a leg change phase 2 (10 walls), the kill back to the level's own set; a probe point on a phase-1 wall is refused only while its phase is on;
    /// every anchor ledge keeps a live thread anchor within 0.9 x the grapple's range. RED control: loom40 off, no phase ever.</summary>
    static int Loom40NoRideCheck()
    {
        int fails = 0;
        foreach (bool on in new[] { true, false })
        {
            SurvivalSim sim = on ? Loom40Sim(1, true) : Loom40OffOnN40();
            var start = sim.loom37Ledges.Find(l => l.id == "start");
            sim.Respawn(new Vec3(start.at.x, 0, start.at.z), 0);
            for (int i = 0; i < 10; i++) sim.Step();
            int level = sim.mover.flow34.ride35.noRide?.Length ?? 0;
            var warns = new List<string>();
            int st = 0;
            sim.events.On("loom40NoRide", o => warns.Add($"{st / 120.0:0.00}s {o}"));
            sim.BossRoomBegin("loom37", out _, out _);
            Loom37 L = sim.loom37;
            sim.mover.invulnerable = true;
            int during = -1, after1 = -1, after2 = -1, afterKill = -1;
            for (; st < 200; st++) { sim.Step(); if (st == 60) during = sim.mover.flow34.ride35.noRide?.Length ?? 0; }
            after1 = sim.mover.flow34.ride35.noRide?.Length ?? 0;
            Vec3 probe = new Vec3(-39.05, 4, -300); // on the canyon's west wall (phase 1)
            bool refused1 = sim.mover.flow34.ride35.noRide != null && Ride38.NoRideAt(sim.mover.flow34.ride35.noRide, probe, 0.12);
            L.brain.NextLeg();
            for (int i = 0; i < 6 * 120 && L.brain.state == Boss37State.LegChange; i++) { sim.Step(); st++; }
            for (int i = 0; i < 200; i++) { sim.Step(); st++; }
            after2 = sim.mover.flow34.ride35.noRide?.Length ?? 0;
            L.Apply(1e9);
            for (int i = 0; i < 4 * 120 && L.Alive; i++) { sim.Step(); st++; }
            for (int i = 0; i < 4; i++) { sim.Step(); st++; }
            afterKill = sim.mover.flow34.ride35.noRide?.Length ?? 0;
            bool refusedAfter = sim.mover.flow34.ride35.noRide != null && Ride38.NoRideAt(sim.mover.flow34.ride35.noRide, probe, 0.12);
            if (!on)
            {
                bool ok0 = after1 == level && after2 == level && afterKill == level && !refused1;
                Console.WriteLine($"noride (loom40 off, n38-span): the Mover's noride boxes {level} -> {after1} -> {after2} -> {afterKill}, west wall refused {refused1}: {(ok0 ? "PASS" : "FAIL")}");
                if (!ok0) fails++;
                continue;
            }
            int p1 = sim.loom40NoRide1?.Length ?? 0, p2 = sim.loom40NoRide2?.Length ?? 0;
            // a live thread anchor within reach of every anchor ledge
            double reach = 0.9 * sim.cfg.Num("design.n32.grapple32.range"), worst = 0;
            foreach (var l in sim.loom37Ledges)
            {
                double best = 1e9;
                foreach (var a in sim.loom37Anchors) if (a.ledge != l.id) best = Math.Min(best, Vec3.DistanceTo(new Vec3(l.at.x, l.y + 1.6, l.at.z), a.at));
                worst = Math.Max(worst, best);
            }
            bool ok = p1 == 5 && p2 == 10 && during == level && after1 == level + p1 && after2 == level + p2 && afterKill == level && refused1 && !refusedAfter && worst <= reach;
            Console.WriteLine($"noride (loom40 ON): level boxes {level}; phase 1 {p1} walls, phase 2 {p2} walls (of 14); the Mover's set during the warning {during}, phase 1 {after1}, after the leg change {after2}, after the kill {afterKill}; "
                + $"west wall refused in phase 1 {refused1}, after the kill {refusedAfter}; the farthest ledge-to-anchor pull {worst:0.0} m (reach {reach:0.0}); events [{string.Join("; ", warns)}]: {(ok ? "PASS" : "FAIL")}");
            if (!ok) fails++;
        }
        return fails;
    }

    static SurvivalSim Loom40OffOnN40()
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(MapGenRepo, "Design", "levels", "n38-span.json")))));
        sim.noLab = sim.noRecords = true;
        sim.n40Override = N40Blocks.Off;
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(1, null, null);
        sim.husks.frozen = true;
        return sim;
    }

    /// <summary>`n40loom walk [--seeds n] [--first n] [--cap s] [--off]`: the n38 walk pilot (Walk29Pilot + Walk38Span, which answers for n40-span) plays
    /// n40-span's rooms in order with loom40 on (the practice node live in o2): per room cleared k of n, the median, deaths, falls; the practice knee's
    /// latches / tears by the walk (a hijack would show as latches). A copy of Harness.N38.Span.cs's Span38Walk on n40-span (that file is not B4's).</summary>
    static int Loom40Walk(string[] args)
    {
        int seeds = ArgInt36(args, "--seeds", 10), first = ArgInt36(args, "--first", 1);
        double cap = ArgInt36(args, "--cap", 900);
        string path = Path.Combine(MapGenRepo, "Design", "levels", "n40-span.json");
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        var rooms = new List<Walk29Room>();
        for (int i = 0; i < playable; i++) rooms.Add(new Walk29Room { id = level.rooms[i].id });
        var levelTimes = new List<double>();
        int levelsDone = 0, latches = 0, tears = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            GameConfig cfg = Config();
            SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
            sim.noLab = sim.noRecords = true;
            sim.n37Override = new N37Blocks { boss37 = true, loom37 = true };
            sim.n38Override = new N38Blocks { span38 = true };
            sim.n40Override = new N40Blocks { loom40 = true };
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
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room < playable) { rooms[e.room].cleared++; rooms[e.room].times.Add(e.time); } });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room < playable) rooms[e.room].deaths++; });
            sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room < playable) rooms[e.room].falls++; });
            sim.StartRun(seed, null, null);
            int max = MovementMath.StepsFor(cap, sim.step);
            int st = 0;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (sim.campaign != null && sim.campaign.clearSteps.Count >= playable) break;
            }
            latches += sim.loom40PracticeLatches;
            tears += sim.loom40PracticeTears;
            bool done = sim.campaign != null && sim.campaign.clearSteps.Count >= playable;
            if (done) { levelsDone++; levelTimes.Add(st * sim.step); }
            else
            {
                Vec3 p = sim.mover.position;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0} s in room {(sim.campaign != null ? sim.campaign.room + 1 : 0)} ({(sim.campaign != null && sim.campaign.room >= 0 ? level.rooms[sim.campaign.room].id : "?")}), at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), rope {sim.mover.rope.phase} anchor ({sim.mover.rope.anchor.x:0.0},{sim.mover.rope.anchor.y:0.0},{sim.mover.rope.anchor.z:0.0})");
            }
            ap.Detach();
        }
        foreach (Walk29Room r in rooms)
        {
            r.times.Sort();
            double med = r.times.Count == 0 ? double.NaN : r.times[r.times.Count / 2];
            Console.WriteLine($"n40loom walk {r.id}: cleared {r.cleared}/{seeds}, median {med:0.0} s, deaths {r.deaths}, falls {r.falls}");
        }
        levelTimes.Sort();
        double lm = levelTimes.Count == 0 ? double.NaN : levelTimes[levelTimes.Count / 2];
        Console.WriteLine($"n40loom walk SUMMARY (n40-span, loom40 ON): all {playable} playable rooms cleared in {levelsDone} of {seeds} runs, median {lm:0.0} s; the practice knee hooked by the walk {latches}x, torn {tears}x");
        return levelsDone < seeds ? 1 : 0;
    }

    sealed class Loom40StayRun
    {
        public bool begun, killed;
        public double seconds, maxOut, maxUnder;
        public int outSteps, underSteps, steps, hides, tearsPull, tearsLt, latches, nailsIn, deaths, windows;
        public string level;
    }

    static Loom40StayRun Loom40StayFight(uint seed, bool on, double cap, bool trace)
    {
        var r = new Loom40StayRun();
        SurvivalSim sim = Loom40Sim(seed, on);
        r.level = sim.campaignLevel?.id;
        var start = sim.loom37Ledges.Find(l => l.id == "start");
        if (start == null) return r;
        sim.Respawn(new Vec3(start.at.x, 0, start.at.z), 0);
        for (int i = 0; i < 10; i++) sim.Step();
        if (!on && !sim.loadout.owned.Contains("nailgun")) sim.loadout.AddWeapon("nailgun"); // loom40 off: the level's STITCHER (loom40: the pilot hooks knees, no nails)
        r.begun = sim.BossRoomBegin("loom37", out _, out _);
        Loom37 L = sim.loom37;
        if (!r.begun || L == null || !L.Alive) return r;
        double floorY = sim.cfg.Has("design.n40.loom40.floorY") ? sim.cfg.Num("design.n40.loom40.floorY") : -24;
        double x0 = sim.loom37CanyonX0, z0 = sim.loom37CanyonZ0, x1 = sim.loom37CanyonX1, z1 = sim.loom37CanyonZ1;
        var si = (ScriptedInput)sim.input;
        var pilot = new Loom37Pilot { casual = Loom40Casual };
        int st = 0, capSteps = (int)(cap * 120), killStep = -1;
        sim.events.On<Loom37Event>("loom37Down", e => { if (killStep < 0) killStep = st; });
        var start0 = start;
        while (st < capSteps && L.Alive)
        {
            if (sim.health.hp <= 0 || sim.flow != Skygrind.Game.Flow.Playing)
            {
                sim.health.hp = sim.health.max;
                r.deaths++;
                var back = sim.loom37LastLedge ?? start0;
                sim.Respawn(new Vec3(back.at.x, back.y, back.at.z), 0);
                L.brain.RestartLeg();
                if (sim.flow != Skygrind.Game.Flow.Playing) break;
            }
            pilot.Step(sim, st);
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            st++;
            if (!L.Alive) break;
            Vec3 c = L.centre;
            double dx = Math.Max(0, Math.Max(x0 - c.x, c.x - x1)), dz = Math.Max(0, Math.Max(z0 - c.z, c.z - z1));
            double outM = Math.Sqrt(dx * dx + dz * dz);
            if (outM > 0) { r.outSteps++; if (outM > r.maxOut) r.maxOut = outM; }
            if (c.y < floorY) { r.underSteps++; if (floorY - c.y > r.maxUnder) r.maxUnder = floorY - c.y; }
            if (trace && (st % 60 == 0 || (Environment.GetEnvironmentVariable("B4DBG") == "1" && sim.mover.rope.phase != RopePhase.None)))
                Console.WriteLine($"    {st * sim.step:0.0}s leg {L.brain.leg} {L.brain.state} centre ({c.x:0.0},{c.y:0.0},{c.z:0.0}) out {outM:0.0} me ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}) hides {L.brain.hides} rope {sim.mover.rope.phase} anc ({sim.mover.rope.anchor.x:0.0},{sim.mover.rope.anchor.y:0.0},{sim.mover.rope.anchor.z:0.0}) knees [{string.Join(",", System.Linq.Enumerable.Select(L.knees, k => k.torn ? "X" : k.nails.ToString()))}] k0 ({L.knees[0].at.x:0.0},{L.knees[0].at.y:0.0},{L.knees[0].at.z:0.0}) latches {L.latches40} hooks {pilot.hooks} pulls {pilot.fightPulls} ch {sim.grappleCharges32} hp {sim.health.hp:0} lashHits {L.lashHits} spoolHits {L.spoolHits} falls {sim.loom37Falls} flow {sim.flow} stag {L.brain.stagger:0.0} parries {L.spoolParries}");
        }
        r.killed = !L.Alive || L.Falling;
        r.steps = st;
        r.hides = L.brain.hides;
        r.tearsPull = L.tears - L.ltTears; r.tearsLt = L.ltTears; r.latches = L.latches40; r.nailsIn = L.nailsIn; r.windows = L.windowsOpened;
        r.seconds = (killStep >= 0 ? killStep : st) * sim.step;
        return r;
    }

    /// <summary>The camper's spots (a person, not the pilot: GG "it often runs out of the map"): the canyon's start ledge, its end ledge, o2's floor
    /// by the canyon door and at its far end (the player backs out of the canyon), tower 2 west's top.</summary>
    static readonly (string name, Vec3 at)[] Loom40Spots =
    {
        ("start ledge", new Vec3(-17, 0, -254.5)), ("end ledge", new Vec3(-17, 0, -385.5)), ("o2 by the door", new Vec3(-17, 0, -243)),
        ("o2 far end", new Vec3(-17, 0, -202.5)), ("t2w top", new Vec3(-28.5, 8, -323.5)),
    };

    /// <summary>A person who stands on one spot (seed s: spot (s-1) mod 5, for 30 s + s, invulnerable) while THE LOOM fights: its anti-hide.</summary>
    static Loom40StayRun Loom40CampFight(uint seed, bool on, bool trace)
    {
        var r = new Loom40StayRun();
        SurvivalSim sim = Loom40Sim(seed, on);
        r.level = sim.campaignLevel?.id;
        var start = sim.loom37Ledges.Find(l => l.id == "start");
        if (start == null) return r;
        sim.Respawn(new Vec3(start.at.x, 0, start.at.z), 0);
        for (int i = 0; i < 10; i++) sim.Step();
        r.begun = sim.BossRoomBegin("loom37", out _, out _);
        Loom37 L = sim.loom37;
        if (!r.begun || L == null || !L.Alive) return r;
        var spot = Loom40Spots[(seed - 1) % Loom40Spots.Length];
        sim.Respawn(spot.at, 0);
        sim.mover.invulnerable = true;
        double x0 = sim.loom37CanyonX0, z0 = sim.loom37CanyonZ0, x1 = sim.loom37CanyonX1, z1 = sim.loom37CanyonZ1;
        var si = (ScriptedInput)sim.input;
        int capSteps = (int)((30 + seed) * 120);
        for (int st = 0; st < capSteps && L.Alive; st++)
        {
            sim.health.hp = sim.health.max;
            si.Set(new InputFrame { yaw = 180 }, new CombatInput { weapon = -1 });
            sim.Step();
            sim.hitstopLeft = 0;
            Vec3 c = L.centre;
            double dx = Math.Max(0, Math.Max(x0 - c.x, c.x - x1)), dz = Math.Max(0, Math.Max(z0 - c.z, c.z - z1));
            double outM = Math.Sqrt(dx * dx + dz * dz);
            if (outM > 0) { r.outSteps++; if (outM > r.maxOut) r.maxOut = outM; }
            r.steps = st + 1;
            if (trace && st % 120 == 0) Console.WriteLine($"    [{spot.name}] {st / 120.0:0}s leg {L.brain.leg} {L.brain.state} centre ({c.x:0.0},{c.y:0.0},{c.z:0.0}) out {outM:0.0} me ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0})");
        }
        r.hides = L.brain.hides;
        r.seconds = r.steps * sim.step;
        r.level += " / " + spot.name;
        return r;
    }

    static int Loom40Stay(string[] args)
    {
        int fails = Loom40StayPilot(args);
        fails += Loom40StayCamp(args);
        return fails;
    }

    static int Loom40StayCamp(string[] args)
    {
        int seeds = ArgInt36(args, "--seeds", 10);
        bool on = Array.IndexOf(args, "--on") >= 0;
        bool trace = Array.IndexOf(args, "--trace") >= 0;
        int outSteps = 0, begun = 0, hides = 0;
        double maxOut = 0;
        for (int s = 1; s <= seeds; s++)
        {
            Loom40StayRun r = Loom40CampFight((uint)s, on, trace);
            if (r.begun) begun++;
            outSteps += r.outSteps;
            hides += r.hides;
            maxOut = Math.Max(maxOut, r.maxOut);
            Console.WriteLine($"  camp seed {s} ({r.level}): {r.seconds:0} s standing, anti-hides {r.hides}, outside o3 {r.outSteps} steps ({r.outSteps / 120.0:0.00} s, max {r.maxOut:0.0} m)");
        }
        bool ok = begun == seeds && outSteps == 0;
        Console.WriteLine($"n40loom camp (loom40 {(on ? "ON" : "off")}, {seeds} seeds): anti-hides {hides}; THE LOOM's body outside o3's rect {outSteps} steps ({outSteps / 120.0:0.00} s), max {maxOut:0.0} m out{(on ? $": {(ok ? "PASS" : "FAIL")}" : " (today: the root cause)")}");
        return on && !ok ? 1 : 0;
    }

    static int Loom40StayPilot(string[] args)
    {
        int seeds = ArgInt36(args, "--seeds", 10);
        double cap = ArgInt36(args, "--cap", 120);
        bool on = Array.IndexOf(args, "--on") >= 0;
        bool trace = Array.IndexOf(args, "--trace") >= 0;
        int outSteps = 0, underSteps = 0, killed = 0, begun = 0;
        double maxOut = 0, maxUnder = 0;
        string level = null;
        for (int s = 1; s <= seeds; s++)
        {
            Loom40StayRun r = Loom40StayFight((uint)s, on, cap, trace && s == ArgInt36(args, "--first", 1));
            level = r.level;
            if (r.begun) begun++;
            if (r.killed) killed++;
            outSteps += r.outSteps;
            underSteps += r.underSteps;
            maxOut = Math.Max(maxOut, r.maxOut);
            maxUnder = Math.Max(maxUnder, r.maxUnder);
            Console.WriteLine($"  seed {s}: {(r.begun ? "" : "NOT begun, ")}{(r.killed ? "killed" : "NOT killed")} {r.seconds:0.0} s, tears pull {r.tearsPull} / LT {r.tearsLt}, latches {r.latches}, nails in {r.nailsIn}, windows {r.windows}, deaths {r.deaths}, anti-hides {r.hides}, outside o3 {r.outSteps} steps ({r.outSteps / 120.0:0.00} s, max {r.maxOut:0.0} m), under the floor {r.underSteps} steps (max {r.maxUnder:0.0} m)");
        }
        bool ok = begun == seeds && outSteps == 0 && underSteps == 0;
        Console.WriteLine($"n40loom stay (loom40 {(on ? "ON" : "off")}, {level}, {seeds} seeds, cap {cap:0} s): {killed}/{seeds} killed; THE LOOM's body outside o3's rect {outSteps} steps ({outSteps / 120.0:0.00} s), max {maxOut:0.0} m out; under the canyon floor {underSteps} steps, max {maxUnder:0.0} m under{(on ? $": {(ok ? "PASS" : "FAIL")}" : " (today: the root cause)")}");
        return on && !ok ? 1 : 0;
    }
}
