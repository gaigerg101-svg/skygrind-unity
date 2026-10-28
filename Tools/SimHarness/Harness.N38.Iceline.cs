// NIGHT38 builder S2 (NIGHT38-BRIEF card S2): level 2 (n38-iceline) and stag38, headless. One entry, N38Iceline (declared and called in Program.cs).
//   iceline38 [stag|all]
//     stag   THE CANAL with stag38 off (NIGHT37's) and on (design.n38.stag38): the lane walls' height and runs (PLAN.md section 5: 5 m, runs of 30 m+),
//            the ride faces (the island's added with the row on), and the real Mover on the canal's walls (the canal at (3000, 0, 0) on the survival
//            map, as `stag37`): a held ride along straight B's first lane-wall run started with the feet 3 m up (a 2.6 m wall has no contact there:
//            Mover's band is feet+0.35 .. feet+1.45), and a held ride along the island's straight-A face. PASS: on = 5 m walls, every run >= 30 m,
//            the lane-wall ride holds >= 2.7 s with the feet above 2.6 m the whole time; off = NIGHT37's numbers and no ride at 3 m; the island ride
//            holds >= 3 s either way (it was always solid: the row only trims it cyan); the canal rebuilt at a run's start when the bit changes.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N38Iceline(string cmd, string[] args, ref int code)
    {
        if (cmd != "iceline38") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int fails = 0;
        if (what == "stag" || what == "all") fails += Stag38Check();
        if (what == "gates" || what == "all") fails += Run38Gates();
        if (what == "level" || what == "all") fails += Ice38Level();
        if (what == "walk") // the walk bot through every playable room (Walk29Pilot + Walk38Iceline's THE RUN through the wrapper), --seeds N (default 5)
        {
            int seeds = 5;
            int si = Array.IndexOf(args, "--seeds");
            if (si >= 0 && si + 1 < args.Length) seeds = int.Parse(args[si + 1]);
            fails += Ice38Walk(seeds, Array.IndexOf(args, "--trace") >= 0);
        }
        if (what == "band") // THE STAG's fight (NIGHT37's bot band, Harness.N37.StagFight.cs) on the stag38 canal: the fight logic is unchanged, the walls are not
        {
            int seeds = 10;
            int si = Array.IndexOf(args, "--seeds");
            if (si >= 0 && si + 1 < args.Length) seeds = int.Parse(args[si + 1]);
            StagOn38 = true;
            fails += StagBand37(seeds, 90);
            StagOn38 = false;
        }
        N38IcelineMore(what, args, ref fails);
        Console.WriteLine(fails == 0 ? "iceline38: GREEN" : $"iceline38: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static partial void N38IcelineMore(string what, string[] args, ref int fails);

    /// <summary>Sim37S (Harness.N37.Stag.cs) turns stag38 on while this is set (`iceline38 band`).</summary>
    static bool StagOn38;

    static string P38S(bool ok) => ok ? "PASS" : "FAIL";

    /// <summary>The survival map with THE CANAL at StagCanal37, NIGHT37's stag37 row on and stag38 as given.</summary>
    static SurvivalSim Sim38S(uint seed, bool stag38)
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n37Override = new N37Blocks { stag37 = true, boss37 = true };
        sim.n38Override = new N38Blocks { stag38 = stag38 };
        sim.stag37CentreOverride = StagCanal37;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        return sim;
    }

    /// <summary>A held ride (jump held) from `at` (canal-local) with velocity `vel`, the look `yaw`, sideways input `moveX` into the wall: the seconds and
    /// metres on the wall, the feet's lowest and highest height on it, the end reason.</summary>
    static (double seconds, double metres, double yMin, double yMax, string reason) StagRide38(SurvivalSim sim, Vec3 at, Vec3 vel, double yaw, double moveX)
    {
        var si = (ScriptedInput)sim.input;
        sim.Respawn(at + StagCanal37, yaw * 180 / Math.PI);
        sim.mover.velocity = vel;
        string reason = "none";
        Action<MoverEvent> on = e => { if (e.type == MoverEventType.WallRideEnd) reason = e.reason; };
        sim.mover.Events += on;
        double dt = sim.cfg.Dt, yMin = double.MaxValue, yMax = double.MinValue;
        int steps = 0;
        bool rode = false;
        Vec3 first = default, last = default;
        for (int s = 0; s < (int)(12 / dt); s++)
        {
            si.SetLook(yaw, 0);
            si.Set(new InputFrame { yaw = yaw, moveX = moveX, moveZ = 1, jump = new Button { held = true, pressed = false } }, default);
            sim.Step();
            sim.hitstopLeft = 0;
            if (sim.mover.state == MoveState.WallRide)
            {
                if (!rode) first = sim.mover.position;
                rode = true;
                steps++;
                last = sim.mover.position;
                double y = sim.mover.position.y - StagCanal37.y;
                yMin = Math.Min(yMin, y);
                yMax = Math.Max(yMax, y);
            }
            else if (rode) break;
            else if (s * dt > 1.0) break; // no ride within a second
        }
        sim.mover.Events -= on;
        if (!rode) return (0, 0, 0, 0, "no ride");
        return (steps * dt, JsMath.Hypot(last.x - first.x, last.z - first.z), yMin, yMax, reason);
    }

    static int Stag38Check()
    {
        int fails = 0;
        foreach (bool on in new[] { false, true })
        {
            SurvivalSim sim = Sim38S(1, on);
            Stag37Arena a = sim.stag37Arena;
            if (a == null) { Console.WriteLine($"stag38 {(on ? "on" : "off")}: no canal (FAIL)"); fails++; continue; }
            var runs = new List<double>();
            for (int i = 0; i + 1 < a.p.laneWallRuns.Length; i += 2) runs.Add(a.p.laneWallRuns[i + 1] - a.p.laneWallRuns[i]);
            int islandFaces = 0;
            foreach (var f in a.rideFaces) if (Math.Abs(f.h - a.p.islandHeight) < 1e-9 && Math.Abs(a.p.islandHeight - a.p.wallHeight) > 1e-9) islandFaces++;
            Console.WriteLine($"stag38 {(on ? "ON " : "OFF")}: arena is38 {a.p.is38}; lane walls {a.p.laneWallHeight} m tall, runs [{string.Join(", ", a.p.laneWallRuns)}] (lengths {string.Join(", ", runs)} m); ride faces {a.rideFaces.Count} (the island's {islandFaces}); triangles {a.tris.Count}");
            bool shape = on ? a.p.is38 && Math.Abs(a.p.laneWallHeight - 5) < 1e-9 && runs.Count > 0 && runs.TrueForAll(r => r >= 30) && islandFaces == 2 + 2 * a.p.turnFacets
                            : !a.p.is38 && Math.Abs(a.p.laneWallHeight - 2.6) < 1e-9 && islandFaces == 0;
            Console.WriteLine($"  {(on ? "5 m walls, every run >= 30 m, the island's 50 faces trimmed" : "NIGHT37's canal (2.6 m walls, no island trims)")}: {P38S(shape)}");
            if (!shape) fails++;
            // the lane-wall ride, feet 3 m up: the wall between the inner and middle lanes (k = 1), its WEST face, along the first run, southward
            double r = sim.cfg.movement.body.capsuleRadius, wt = a.p.laneWallThick;
            double xFace = -(a.p.turnR + (1 - a.p.lanes * 0.5) * a.p.laneWidth) - wt / 2;
            double run0 = a.p.laneWallRuns[0];
            var lane = StagRide38(sim, new Vec3(xFace - r - 0.02, 3.0, -a.Half + run0 + 0.5), new Vec3(1, 0, 8), Math.PI, -0.5);
            // off: the 2.6 m wall gives no ride up there: the feet fall to the ice first (a ride, if any, starts below 1.2 m)
            bool laneOk = on ? lane.seconds >= 2.7 && lane.yMin > 2.6 : lane.reason == "no ride" || lane.yMax < 1.2;
            Console.WriteLine($"  lane-wall ride from feet 3.0 m (run {run0}..{a.p.laneWallRuns[1]} m along B): {lane.seconds:0.00} s, {lane.metres:0.0} m, feet {lane.yMin:0.00}..{lane.yMax:0.00} m, end '{lane.reason}': "
                + $"{(on ? "holds >= 2.7 s above the old 2.6 m top" : "no ride at 3 m: the feet fall to the ice before the wall holds (GG's 'too short')")}: {P38S(laneOk)}");
            if (!laneOk) fails++;
            // the island's straight-A face (east, x = +InnerR), northward, feet 3 m up
            sim = Sim38S(1, on);
            a = sim.stag37Arena;
            var isl = StagRide38(sim, new Vec3(a.InnerR + r + 0.02, 3.0, a.Half - 10), new Vec3(-1, 0, -8), 0, -0.5);
            bool islOk = isl.seconds >= 3.0;
            Console.WriteLine($"  island ride (straight A's inside, feet 3.0 m): {isl.seconds:0.00} s, {isl.metres:0.0} m, feet {isl.yMin:0.00}..{isl.yMax:0.00} m, end '{isl.reason}': holds >= 3 s: {P38S(islOk)}");
            if (!islOk) fails++;
        }
        // the bit flips between runs on one sim: the canal follows it at each start
        {
            SurvivalSim sim = Sim38S(1, false);
            bool off0 = !sim.stag37Arena.p.is38;
            sim.n38Override = new N38Blocks { stag38 = true };
            sim.StartRun(2, null, null);
            bool on1 = sim.stag37Arena.p.is38 && sim.stag38Rebuilds == 1;
            sim.n38Override = N38Blocks.Off;
            sim.StartRun(3, null, null);
            bool off2 = !sim.stag37Arena.p.is38 && sim.stag38Rebuilds == 2;
            bool ok = off0 && on1 && off2;
            Console.WriteLine($"  one sim, runs off -> on -> off: arena 38 {!off0} -> {sim.stag38Rebuilds >= 1 && on1} -> {!off2}, rebuilds {sim.stag38Rebuilds}: {P38S(ok)}");
            if (!ok) fails++;
        }
        return fails;
    }

    // ---------------------------------------------------------------- iceline38 gates: THE RUN's skate gap and ride chain on the real level

    /// <summary>n38-iceline in campaign mode (enemies frozen) and room j3's run38 row (gen-n37-iceline-boss.cjs --n38).</summary>
    static SurvivalSim Run38Sim(out Dictionary<string, object> run)
    {
        GameConfig cfg = Config();
        var json = PortData.Obj(PortData.LoadJson("Design/levels/n38-iceline.json"));
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(json));
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(1, null, null);
        sim.husks.frozen = true;
        run = null;
        foreach (object ro in PortData.Arr(PortData.Obj(json["campaign"])["rooms"]))
        {
            var rd = PortData.Obj(ro);
            if (rd.ContainsKey("run38")) run = PortData.Obj(rd["run38"]);
        }
        return sim;
    }

    static double[] Run38Pair(object o) { var a = PortData.Arr(o); return new[] { PortData.Num(a[0]), PortData.Num(a[1]) }; }

    /// <summary>One flight from `start` north (yaw 0) at the lip `lip`: the run-up (`pump`: dash whenever a pip is up and the lip is more than
    /// pumpStop m ahead), the jump at the lip (held), then the double at `dbl` s and the dash at `dash` s after it (negative: none). Returns the
    /// speed at the lip and where it came down (z, y, grounded) within 4 s of the jump.</summary>
    static (double lipSpeed, double z, double y, bool grounded, double reach) Run38Fly(SurvivalSim sim, Vec3 start, Vec3 lip, bool pump, double dbl, double dash)
    {
        var si = (ScriptedInput)sim.input;
        double dt = sim.cfg.Dt;
        sim.Respawn(start, 0);
        int jumpAt = -1, dashCool = 0, cap = (int)(30 / dt);
        double lipSpeed = 0, reach = 0;
        const double pumpStop = 12; // m before the lip the pump stops (a pip refills for the air: design.stamina's regen)
        for (int s = 0; s < cap; s++)
        {
            Vec3 p = sim.mover.position;
            double yaw = JsMath.Atan2(-(lip.x - p.x) * (jumpAt < 0 ? 1 : 0), 10); // steer onto the lip's line, then straight north
            // the pump holds no sprint (the sprint drains the same stamina as the dash: StagSkate37's skater does the same)
            var f = new InputFrame { yaw = yaw, moveZ = 1, sprint = new Button { held = !pump, pressed = !pump && s == 0 } };
            if (jumpAt < 0)
            {
                if (dashCool > 0) dashCool--;
                if (pump && p.z - lip.z > pumpStop && dashCool == 0 && sim.mover.grounded && sim.mover.pips >= 1) { f.dash = new Button { held = true, pressed = true }; dashCool = (int)(0.35 / dt); }
                if (p.z - lip.z <= 0.4) { jumpAt = s; lipSpeed = JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z); f.jump = new Button { held = true, pressed = true }; }
            }
            else
            {
                int k = s - jumpAt;
                if (p.y >= lip.y - 0.05) reach = Math.Max(reach, lip.z - p.z); // E's reach: metres past the lip while the feet are at or above its height
                bool dblNow = dbl >= 0 && k == (int)(dbl / dt), dashNow = dash >= 0 && k == (int)(dash / dt);
                f.jump = new Button { held = true, pressed = dblNow };
                if (dashNow) f.dash = new Button { held = true, pressed = true };
                if (k > 6 && sim.mover.grounded) return (lipSpeed, p.z, p.y, true, reach);
                if (k > (int)(4 / dt) || p.y < -3.5) return (lipSpeed, p.z, p.y, sim.mover.grounded, reach);
            }
            si.SetLook(yaw, 0);
            si.Set(f, default);
            sim.Step();
            sim.hitstopLeft = 0;
        }
        return (lipSpeed, sim.mover.position.z, sim.mover.position.y, false, reach);
    }

    /// <summary>The chain flown: from 6 m south of D2's lip, the jump at the lip (held) toward the first wall; on each wall, released 3 m before its north
    /// end (look level, the stick away), the jump pressed again 0.25 s later and held, the stick toward the next wall; the last wall ridden to its end.
    /// Returns the walls ridden and where it came down.</summary>
    static (int ridden, double z, double y, bool grounded) Run38Chain(SurvivalSim sim, Vec3 lip, List<(double planeX, int side, double zN, double zS)> walls)
    {
        var si = (ScriptedInput)sim.input;
        double dt = sim.cfg.Dt;
        sim.Respawn(new Vec3(lip.x, lip.y, lip.z + 6), 0);
        int ci = 0, released = -1, ridden = 0, jumpAt = -1;
        bool wasRiding = false;
        for (int s = 0; s < (int)(20 / dt); s++)
        {
            Vec3 p = sim.mover.position;
            bool riding = sim.mover.state == MoveState.WallRide;
            if (riding && !wasRiding) ridden++;
            wasRiding = riding;
            var w = walls[Math.Min(ci, walls.Count - 1)];
            var f = new InputFrame { yaw = 0, moveZ = 1, sprint = new Button { held = jumpAt < 0, pressed = s == 0 } };
            bool held = true, press = false;
            double stick = w.side * 0.6;
            if (jumpAt < 0)
            {
                stick = 0;
                if (p.z - lip.z <= 0.4) { jumpAt = s; press = true; stick = w.side * 0.6; }
                else held = false;
            }
            else if (released < 0)
            {
                if (riding && ci < walls.Count - 1 && p.z - w.zN <= 3) { released = s; held = false; stick = -w.side * 0.7; }
            }
            else
            {
                int since = s - released;
                var next = walls[ci + 1];
                if (since < (int)(0.25 / dt)) { held = false; stick = -w.side * 0.7; }
                else
                {
                    press = since == (int)(0.25 / dt);
                    stick = next.side * 0.6;
                    if (riding && since > (int)(0.3 / dt)) { ci++; released = -1; }
                }
            }
            if (jumpAt >= 0 && s - jumpAt > 6 && sim.mover.grounded) return (ridden, p.z, p.y, true);
            if (p.y < -3.5) return (ridden, p.z, p.y, false);
            f.moveX = stick;
            f.jump = new Button { held = held || press, pressed = press };
            si.SetLook(0, 0);
            si.Set(f, default);
            sim.Step();
            sim.hitstopLeft = 0;
        }
        return (ridden, sim.mover.position.z, sim.mover.position.y, sim.mover.grounded);
    }

    /// <summary>level: n38-iceline boots in campaign mode (NIGHT37's stag37 + boss37 rows and stag38 on): 8 rooms, no copied room id (i*), the gauntlet
    /// pickup in j1, THE CANAL at s4's row centre with the stag38 walls, the boss spawn on ice; 2 s stepped at j1's spawn with nothing thrown.</summary>
    static int Ice38Level()
    {
        GameConfig cfg = Config();
        var json = PortData.Obj(PortData.LoadJson("Design/levels/n38-iceline.json"));
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(json));
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.n37Override = new N37Blocks { stag37 = true, boss37 = true };
        sim.n38Override = new N38Blocks { iceline38 = true, stag38 = true };
        sim.StartRun(1, null, null);
        var ids = sim.campaignLevel.rooms.ConvertAll(r => r.id);
        RoomDef boss = sim.campaignLevel.rooms.Find(r => r.kind == "boss");
        bool noOld = !ids.Exists(i => i.StartsWith("i"));
        bool gauntlet = false;
        foreach (object ro in PortData.Arr(PortData.Obj(json["campaign"])["rooms"]))
        {
            var rd = PortData.Obj(ro);
            if (rd.ContainsKey("pickup31") && (PortData.Obj(rd["pickup31"])["weapon"] as string) == "gauntlet" && (rd["id"] as string) == "j1") gauntlet = true;
        }
        var si = (ScriptedInput)sim.input;
        for (int k = 0; k < 240; k++) { si.Set(new InputFrame { yaw = 0 }, default); sim.Step(); }
        bool canal = sim.stag37Centre.HasValue && sim.stag37Arena != null && sim.stag37Arena.p.is38;
        sim.Respawn(boss.spawn, boss.spawnYawDeg);
        for (int k = 0; k < 120; k++) { si.Set(new InputFrame { yaw = 0 }, default); sim.Step(); }
        Vec3 f = sim.mover.position;
        bool ok = ids.Count == 8 && noOld && gauntlet && boss != null && boss.id == "s4" && canal && sim.mover.grounded && sim.OnIce30(f);
        Console.WriteLine($"level n38-iceline: {ids.Count} rooms [{string.Join(" ", ids)}], no copied room {noOld}, the gauntlet pickup in j1 {gauntlet}, boss room {boss?.id} ({boss?.boss}), "
            + $"the canal at the row with stag38's walls {canal}; at the boss spawn grounded {sim.mover.grounded} on ice {sim.OnIce30(f)}: {P38S(ok)}");
        return ok ? 0 : 1;
    }

    /// <summary>walk: n38-iceline's playable rooms (every room but THE CANAL) in order through the real sim (walk29's loop: locked doors, enemies at the
    /// nodes, falls), the pilot Walk29Pilot wrapped by Walk38IcelineInput (THE RUN scripted). Per room: cleared k / n, median s, deaths, falls.</summary>
    static int Ice38Walk(int seeds, bool trace)
    {
        GameConfig cfg = Config();
        const string path = "Design/levels/n38-iceline.json";
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        var cleared = new int[playable];
        var deaths = new int[playable];
        var falls = new int[playable];
        var times = new List<double>[playable];
        for (int i = 0; i < playable; i++) times[i] = new List<double>();
        int done = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(1 + s);
            SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            sim.n38Override = new N38Blocks { iceline38 = true, stag38 = true };
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.GoalProvider = () =>
            {
                Campaign24 c = sim.campaign;
                LevelDef lv = sim.campaignLevel;
                if (c == null || c.room < 0) return lv.rooms[0].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            var input = new Walk38IcelineInput(new Walk29Pilot(sim, ap));
            sim.UseInput(input);
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room < playable) { cleared[e.room]++; times[e.room].Add(e.time); } });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room < playable) deaths[e.room]++; });
            sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room < playable) falls[e.room]++; });
            if (trace) sim.events.On<RoomEvent>("roomClear", e => Console.WriteLine($"  [seed {seed}] room {e.room + 1} {e.id} cleared in {e.time:0.0} s"));
            sim.StartRun(seed, null, null);
            int max = MovementMath.StepsFor(600, sim.step), st = 0;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                int m0 = input.pilot.Mode38;
                bool high0 = sim.mover.position.y > -1;
                sim.Step();
                if (trace && input.pilot.Mode38 != m0) Console.WriteLine($"    j3 flight {m0} -> {input.pilot.Mode38} at ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z:0.0}) {sim.mover.state}");
                if (trace && high0 && sim.mover.position.y <= -1) Console.WriteLine($"    FELL (flight {m0}) at ({sim.mover.position.x:0.0}, {sim.mover.position.z:0.0}) speed {JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z):0.0}");
                if (sim.campaign != null && sim.campaign.clearSteps.Count >= playable) break;
            }
            bool ok = sim.campaign != null && sim.campaign.clearSteps.Count >= playable;
            if (ok) done++;
            else
            {
                Vec3 p = sim.mover.position;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0} s in room {(sim.campaign != null && sim.campaign.room >= 0 ? level.rooms[sim.campaign.room].id : "?")} {sim.campaign?.phase} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), state {sim.mover.state}, alive {sim.enemies.AliveCount}, unsticks {input.pilot.unsticks}");
            }
            ap.Detach();
        }
        for (int i = 0; i < playable; i++)
        {
            times[i].Sort();
            double med = times[i].Count == 0 ? double.NaN : times[i][times[i].Count / 2];
            Console.WriteLine($"walk38 iceline {level.rooms[i].id}: cleared {cleared[i]}/{seeds}, median {med:0.0} s, deaths {deaths[i]}, falls {falls[i]}");
        }
        Console.WriteLine($"walk38 iceline SUMMARY: all {playable} playable rooms cleared in {done} of {seeds} runs: {P38S(done == seeds)}");
        return done == seeds ? 0 : 1;
    }

    static bool Run38Trace =>Array.IndexOf(Environment.GetCommandLineArgs(), "--trace") >= 0;

    static int Run38Gates()
    {
        int fails = 0;
        SurvivalSim sim = Run38Sim(out var run);
        if (run == null) { Console.WriteLine("gates: n38-iceline has no run38 row (FAIL)"); return 1; }
        Vec3 laneStart = PortData.V3(run["laneStart"]), lip = PortData.V3(run["lip1"]);
        double[] d1 = Run38Pair(run["d1"]), d3 = Run38Pair(run["d3"]);
        bool On(double[] zr, (double lipSpeed, double z, double y, bool grounded, double reach) r) => r.grounded && r.y > -0.3 && r.z >= zr[0] && r.z <= zr[1];
        // G1: pumped from the lane's start (the skate: speed built on the ice) vs a 6 m sprint start on the same ice (no speed built) vs E's dry ground
        var plans = new List<(double dbl, double dash)>();
        foreach (double dbl in new[] { -1, 0.3, 0.5, 0.7 }) foreach (double dash in new[] { -1, 0.1, 0.4, 0.7 }) plans.Add((dbl, dash));
        int pumpedLands = 0, sprintLands = 0;
        double pumpSpeed = 0, sprintSpeed = 0, pumpReach = 0, sprintReach = 0;
        string bestPlan = "", sprintPlan = "";
        foreach (var (dbl, dash) in plans)
        {
            var a = Run38Fly(sim, laneStart, lip, true, dbl, dash);
            pumpSpeed = Math.Max(pumpSpeed, a.lipSpeed);
            if (On(d1, a)) pumpedLands++;
            if (a.reach > pumpReach) { pumpReach = a.reach; bestPlan = $"double {dbl} s, dash {dash} s"; }
            var b = Run38Fly(sim, new Vec3(lip.x, lip.y, lip.z + 6), lip, false, dbl, dash);
            if (Run38Trace) Console.WriteLine($"    plan double {dbl} dash {dash}: pumped lip {a.lipSpeed:0.0} m/s reach {a.reach:0.0} -> down {lip.z - a.z:0.0} m past, y {a.y:0.00}, grounded {a.grounded}; sprint lip {b.lipSpeed:0.0} reach {b.reach:0.0} -> down {lip.z - b.z:0.0} m past, y {b.y:0.00}");
            sprintSpeed = Math.Max(sprintSpeed, b.lipSpeed);
            if (On(d1, b)) sprintLands++;
            if (b.reach > sprintReach) { sprintReach = b.reach; sprintPlan = $"double {dbl} s, dash {dash} s"; }
        }
        double gap = lip.z - d1[1];
        // builder E's measured dry-ground envelope (Design/levels/envelopes38.json): the best reach of every try level 2 owns (no boost: level 3)
        double dryBest = 0;
        foreach (var kv in PortData.Obj(PortData.Obj(PortData.LoadJson("Design/levels/envelopes38.json"))["ground"]))
            if (!kv.Key.Contains("boost")) dryBest = Math.Max(dryBest, PortData.Num(PortData.Obj(kv.Value)["reach"]));
        // D-6 for a skate gap: the takeoff IS ice, so every try is a skate (the glide: a dash begun on the ice adds design.n30.glide.dashAdd, Core/Glide30.cs);
        // the verb is "the ice", the other tries are the same verbs off dry ground (builder E's envelope). PASS: the ice's best reach clears the gap by
        // >= 10 % and lands on D1, and the dry-ground best stays short of it.
        double iceReach = Math.Max(pumpReach, sprintReach);
        bool g1 = (pumpedLands + sprintLands) > 0 && iceReach >= gap * 1.1 && dryBest < gap;
        Console.WriteLine($"gates G1 the skate gap ({gap:0.0} m): on the ice, pumped from the lane's start (lip {pumpSpeed:0.0} m/s): reach {pumpReach:0.0} m ({bestPlan}), {pumpedLands}/{plans.Count} air plans land on D1; "
            + $"a 6 m start on the ice (lip {sprintSpeed:0.0} m/s): reach {sprintReach:0.0} m ({sprintPlan}), {sprintLands}/{plans.Count} land; the ice's best {iceReach:0.0} m ({(iceReach / gap - 1) * 100:+0;-0} %); "
            + $"dry ground (E's envelope, every owned verb and combo) {dryBest} m ({(dryBest / gap - 1) * 100:+0;-0} %): {(g1 ? "a skate-only gap" : "INFO, not a D-6 gate (round 3: the dash jump and the PULSE blast clear it off dry ground; it stays a flow beat)")}");
        if (pumpedLands + sprintLands == 0) fails++; // round 3: G1 must still be crossable on the ice (the walk takes it); D-6 is not claimed
        // G3: the chain
        var walls = new List<(double planeX, int side, double zN, double zS)>();
        foreach (object wo in PortData.Arr(run["chain"]))
        {
            var w = PortData.Obj(wo);
            double[] z = Run38Pair(w["z"]);
            walls.Add((PortData.Num(w["planeX"]), (int)PortData.Num(w["side"]), z[0], z[1]));
        }
        Vec3 d2Lip = PortData.V3(run["d2Lip"]);
        var c = Run38Chain(sim, d2Lip, walls);
        bool g3 = c.ridden >= walls.Count && c.grounded && c.y > -0.3 && c.z >= d3[0] && c.z <= d3[1];
        Console.WriteLine($"gates G3 the chain ({d2Lip.z - d3[1]:0.0} m, {walls.Count} walls): rode {c.ridden} wall(s), came down at z {c.z:0.0} y {c.y:0.00} grounded {c.grounded} (D3 z {d3[0]:0.0}..{d3[1]:0.0}); E's checker: no other try crosses: {P38S(g3)}");
        if (!g3) fails++;
        return fails;
    }
}
