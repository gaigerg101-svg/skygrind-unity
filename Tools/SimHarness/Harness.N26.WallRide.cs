// NIGHT26 (builder M): `surfaces --wallride [--trace] [--off]` - the wall-ride hall, measured (the platforming kit, Tools/night/gen-platforming-kits.py).
// The kit plan (Design/levels/kits/platforming.plan.json) is built in memory (LevelPlan24), the real Mover runs the scripted wall-jump chain along the hall's
// walls (PlatformRoutes26.HallRun: a dash at each gap) and RIVAL's graph gives the floor route (down the pit, the serpentine, the far stair). Prints both times,
// the wall-jump / dash counts, and the FOV lift reading: design.n26.wallRide.fovLiftDeg while riding with the row on, 0 with it off (CameraFeel's target).
// PASS: the wall route reaches the far floor and the floor route is at least `minFloorOverWall` x slower. `--off` runs with the wallRide row off: the lift
// reads 0 and the row FAILs (the RED line; the sim route itself does not change: the feel is view-only, so the RED is the lift).
// A dash gap row follows (a dash alone crosses; a jump alone falls short).
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;
using Skygrind.Game.MapAuthoring;
using Skygrind.EditorTools;

public static partial class Harness
{
    static int WallRide26Run(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0, trace = Array.IndexOf(args, "--trace") >= 0;
        GameConfig cfg = Config();
        var plan = PortData.Obj(PortData.LoadJson(Path.GetFullPath("Design/levels/kits/platforming.plan.json")));
        LevelPlan24.LevelBuild lb = LevelPlan24.Build(plan, cfg);
        var map = PortData.Obj(JsJson.Parse(lb.text));
        SurvivalMap sm = SurvivalMap.FromExport(map);
        CoreGrid core = sm.CreateCore(cfg.movement);
        var world = new GameWorld(sm.BuildOctree(), core);
        LevelDef level = LevelDef.From(map);
        var hall = (Dictionary<string, object>)null;
        foreach (object ro in PortData.Arr(PortData.Obj(map["campaign"])["rooms"]))
            if (PortData.Obj(ro).ContainsKey("hallRoute")) hall = PortData.Obj(ro);
        if (hall == null) { Console.WriteLine("wallride FAIL: the kit plan has no room with a hallRoute"); return 1; }
        var hr = PortData.Obj(hall["hallRoute"]);
        var from = PortData.Arr(hr["from"]);
        var to = PortData.Arr(hr["to"]);
        var gl = PortData.Arr(hr["gaps"]);
        var gaps = new double[gl.Count][];
        for (int i = 0; i < gaps.Length; i++) gaps[i] = new[] { PortData.Num(PortData.Arr(gl[i])[0]), PortData.Num(PortData.Arr(gl[i])[1]) };
        double fx = PortData.Num(from[0]), fz = PortData.Num(from[2]), tz = PortData.Num(to[2]);
        var mover = new Mover(cfg.movement, world);
        if (trace)
        {
            PlatformRoutes26.Trace = (i, mm) => { if (i % 6 == 0) Console.WriteLine($"  t {i / 120.0:0.00} pos ({mm.position.x:0.00}, {mm.position.y:0.00}, {mm.position.z - fz:0.00}) v {mm.Speed:0.0} {mm.state} wj {mm.wallJumpsLeft}"); };
            // (the trace re-runs the script's first seconds one step at a time through HallRun's own loop: print the end state per second)
        }
        PlatformRoutes26.RunResult run = PlatformRoutes26.HallRun(mover, cfg.Dt, fx, fz, tz, gaps, 20, hr.ContainsKey("jumpAtZ") ? PortData.Num(hr["jumpAtZ"]) : double.PositiveInfinity);
        RoomDef room = level.rooms.Find(r => r.id == (string)hall["id"]);
        NavGraph graph = NavGraph.Build(NavMap.From(sm), world, cfg, core, NavGrid.FromExport(PortData.Obj(map["navGrid"])));
        var lipP = hr.ContainsKey("lip") ? PortData.Arr(hr["lip"]) : null;
        Vec3 lipAt = lipP != null ? new Vec3(PortData.Num(lipP[0]), PortData.Num(lipP[1]), PortData.Num(lipP[2])) : room.spawn;
        int a = Astar.NearestNode(graph, lipAt), b = Astar.NearestNode(graph, room.exit);
        var path = new NavPath();
        bool floor = a >= 0 && b >= 0 && Astar.Path(graph, a, b, cfg.movement.dash.speed, path);
        double lead = -run.jumpSeconds; // seconds are counted from the lip's jump
        double wallS = run.reached ? run.seconds + lead : double.NaN, floorS = floor ? path.cost : double.NaN;
        bool rowOn = cfg.Design("n26.wallRide.enabled") && !off && !N26Blocks.ForceOff;
        double lift = rowOn ? cfg.Num("design.n26.wallRide.fovLiftDeg") : 0;
        double need = hr.ContainsKey("minFloorOverWall") ? PortData.Num(hr["minFloorOverWall"]) : 2;
        Console.WriteLine($"wallride: hall wall route {(run.reached ? Sf(wallS) + " s (" + run.wallJumps + " wall jumps, " + run.dashes + " dashes, " + Sf(run.wallRideSeconds) + " s riding)" : "does NOT reach the far floor (best z " + Sf(run.bestZ) + ")")}; floor route {(floor ? Sf(floorS) + " s" : "none")}; ratio {Sf(floorS / wallS)}x (needs {Sf(need)}x)");
        bool routeOk = run.reached && floor && floorS >= need * wallS;
        bool fovOk = lift > 0;
        Console.WriteLine($"wallride {(routeOk ? "PASS" : "FAIL")} routes: the wall route is {Sf(floorS / wallS)}x faster than the floor route");
        Console.WriteLine($"wallride {(fovOk ? "PASS" : "FAIL")} fov lift: {Sf(lift)} deg while riding (design.n26.wallRide {(rowOn ? "on" : "OFF")}; the camera eases it in at fovEaseRate)");
        int fails = (routeOk ? 0 : 1) + (fovOk ? 0 : 1);
        // the dash gap (a dash alone crosses; a jump alone falls short): the same rows the validate report prints
        foreach (object ro in PortData.Arr(PortData.Obj(map["campaign"])["rooms"]))
        {
            var rd = PortData.Obj(ro);
            if (!rd.ContainsKey("dashGap")) continue;
            var dg = PortData.Obj(rd["dashGap"]);
            double gap = PortData.Num(dg["gap"]), lip = PortData.Num(PortData.Arr(dg["from"])[2]) - 1, dx = PortData.Num(PortData.Arr(dg["from"])[0]);
            bool dashOk = PlatformRoutes26.DashCross(new Mover(cfg.movement, world), cfg.Dt, dx, lip + 6, lip, gap, true);
            bool jumpOk = PlatformRoutes26.DashCross(new Mover(cfg.movement, world), cfg.Dt, dx, lip + 6, lip, gap, false);
            bool ok = dashOk && !jumpOk;
            Console.WriteLine($"wallride {(ok ? "PASS" : "FAIL")} dash gap {Sf(gap)} m: dash alone {(dashOk ? "crosses" : "does not cross")}, jump alone {(jumpOk ? "crosses" : "falls short")}");
            if (!ok) fails++;
        }
        Console.WriteLine($"wallride: {(fails == 0 ? "ALL PASS" : fails + " FAIL")}");
        return fails == 0 ? 0 : 1;
    }
}
