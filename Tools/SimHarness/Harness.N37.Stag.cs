// NIGHT37 builder S (stag37): THE STAG and THE CANAL, headless. One entry, N37Stag (declared and called in Program.cs).
//   stag37 [canal|speeds|all] [--seeds N] [--cap S]
//     The canal (Stag37Arena) is put at (3000, 0, 0) on the survival map (stag37CentreOverride; n37-iceline gives it by its room row).
//     canal   the loop's numbers (length, triangles, floes, ride faces), the path maths (PointAt / Project round trip), a skater dropped on each
//             part (straight A, turn N, straight B's floes, turn S) stands on ice (grounded, glide on). PASS each.
//     speeds  the player's skating speed on the canal's ice, measured by a scripted skater steering round the loop: no dash (glide only), and
//             dashing whenever a pip is up (the pump): top and lap-average speed; the Stag's path speed beside it (the brief's check: catching it
//             must need the slipstream).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static readonly Vec3 StagCanal37 = new Vec3(3000, 0, 0);
    static bool StagTrace37;

    static partial void N37Stag(string cmd, string[] args, ref int code)
    {
        if (cmd != "stag37") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int fails = 0;
        if (what == "canal" || what == "all") fails += StagCanal37Check();
        if (what == "speeds" || what == "all") fails += StagSpeeds37();
        if (what == "level" || what == "all") fails += StagLevel37();
        N37StagMore(what, args, ref fails);
        Console.WriteLine(fails == 0 ? "stag37: GREEN" : $"stag37: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static partial void N37StagMore(string what, string[] args, ref int fails);

    static string StagP37(bool ok) => ok ? "PASS" : "FAIL";

    static SurvivalSim Sim37S(uint seed, bool on)
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        if (on) sim.n37Override = new N37Blocks { stag37 = true, boss37 = true };
        else sim.n37Override = N37Blocks.Off;
        if (StagOn38) sim.n38Override = new N38Blocks { stag38 = true }; // NIGHT38 S2: `iceline38 band` runs NIGHT37's fight on the stag38 canal
        sim.stag37CentreOverride = StagCanal37;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        return sim;
    }

    /// <summary>level: n37-iceline boots in campaign mode with the canal at its row's centre (the player put at the canal's spawn stands on ice);
    /// level 6 (n35-finale, the OLD Stag "glacier") boots with the row on and steps 2 s with no canal.</summary>
    static int StagLevel37()
    {
        int fails = 0;
        GameConfig cfg = Config();
        foreach (string id in new[] { "n37-iceline", "n35-finale" })
        {
            string path = "Design/levels/" + id + ".json";
            var json = PortData.Obj(PortData.LoadJson(path));
            SurvivalMap map = SurvivalMap.FromExport(json);
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            sim.n37Override = new N37Blocks { stag37 = true, boss37 = true };
            sim.StartRun(1, null, null);
            Vec3? want = null;
            RoomDef boss = null;
            foreach (RoomDef r in sim.campaignLevel.rooms) if (r.kind == "boss") boss = r;
            foreach (object ro in PortData.Arr(PortData.Obj(json["campaign"])["rooms"]))
            {
                var rd = PortData.Obj(ro);
                if (rd.ContainsKey("stag37")) want = PortData.V3(PortData.Obj(rd["stag37"])["centre"]);
            }
            var si = (ScriptedInput)sim.input;
            if (id == "n37-iceline")
            {
                bool placed = sim.stag37Centre.HasValue && want.HasValue && Vec3.DistanceTo(sim.stag37Centre.Value, want.Value) < 1e-9;
                sim.Respawn(boss.spawn, boss.spawnYawDeg);
                for (int k = 0; k < 120; k++) { si.Set(new InputFrame { yaw = 0 }, default); sim.Step(); }
                Vec3 f = sim.mover.position;
                bool ok = placed && sim.mover.grounded && sim.OnIce30(f) && Math.Abs(f.y) < 0.05 && sim.Boss36WalkIn;
                Console.WriteLine($"level {id}: {sim.campaignLevel.rooms.Count} rooms, boss room {boss?.id} ({boss?.boss}), canal at the row's centre {placed}, walk-in {sim.Boss36WalkIn}; at the boss spawn: feet y {f.y:0.000} grounded {sim.mover.grounded} on ice {sim.OnIce30(f)}: {StagP37(ok)}");
                if (!ok) fails++;
            }
            else
            {
                for (int k = 0; k < 240; k++) { si.Set(new InputFrame { yaw = 0 }, default); sim.Step(); }
                Console.WriteLine($"level {id} (level 6, the old Stag): boots, {sim.campaignLevel.rooms.Count} rooms, boss room {boss?.id} ({boss?.boss}), no canal {sim.stag37Centre == null}, 2 s stepped: {StagP37(sim.stag37Centre == null)}");
                if (sim.stag37Centre != null) fails++;
            }
        }
        return fails;
    }

    static double StagYaw37(Vec3 heading) => JsMath.Atan2(-heading.x, -heading.z);

    static int StagCanal37Check()
    {
        int fails = 0;
        SurvivalSim sim = Sim37S(1, true);
        Stag37Arena a = sim.stag37Arena;
        if (a == null) { Console.WriteLine("canal: not built (FAIL)"); return 1; }
        Console.WriteLine($"canal: loop {a.Length:0.0} m (straights {a.p.straight} m, turns {a.TurnLength:0.0} m at R {a.p.turnR}), lanes {a.p.lanes} x {a.p.laneWidth} m, outer R {a.OuterR:0.00}, inner R {a.InnerR:0.00}");
        Console.WriteLine($"  triangles {a.tris.Count} (layer stag37Canal), floes {a.floes.Count}, ride faces {a.rideFaces.Count}");
        bool lenOk = Math.Abs(a.Length - 260) < 3;
        Console.WriteLine($"  the loop is ~260 m (BOSSES.md): {StagP37(lenOk)}");
        if (!lenOk) fails++;
        // the path maths: Project(PointAt(s, lat)) = (s, lat)
        double worst = 0;
        for (double s = 0.5; s < a.Length; s += 3.1)
            for (int lane = 0; lane < a.p.lanes; lane++)
            {
                double lat = a.LaneLat(lane) + 0.7;
                Vec3 q = a.PointAt(s, lat);
                a.Project(q.x, q.z, out double s2, out double lat2);
                worst = Math.Max(worst, Math.Max(Math.Abs(a.Ahead(s, s2)), Math.Abs(lat - lat2)));
            }
        Console.WriteLine($"  path round trip worst error {worst:0.000000} m: {StagP37(worst < 1e-6)}");
        if (worst >= 1e-6) fails++;
        // drop a skater on each part: it lands on ice, grounded, the glide's ice test true
        double dt = sim.cfg.Dt;
        var si = (ScriptedInput)sim.input;
        foreach (var (name, s) in new[] { ("straight A", 40.0), ("turn N", a.STurnN + a.TurnLength / 2), ("straight B (a floe)", a.SB + 40), ("turn S", a.STurnS + a.TurnLength / 3), ("the tunnel", a.SB + (a.p.tunnelFrom + a.p.tunnelTo) / 2) })
        {
            Vec3 at = a.PointAt(s, 0, out Vec3 hd) + StagCanal37;
            sim.Respawn(new Vec3(at.x, at.y + 0.5, at.z), StagYaw37(hd) * 180 / Math.PI);
            for (int k = 0; k < 60; k++) { si.Set(new InputFrame { yaw = StagYaw37(hd) }, default); sim.Step(); }
            Vec3 f = sim.mover.position;
            bool ok = sim.mover.grounded && Math.Abs(f.y - at.y) < 0.05 && sim.OnIce30(f);
            Console.WriteLine($"  {name}: feet y {f.y - at.y:0.000}, grounded {sim.mover.grounded}, on ice {sim.OnIce30(f)}: {StagP37(ok)}");
            if (!ok) fails++;
        }
        // the slab: open at load, closed when asked
        bool gapOpen = !sim.world.RayIntersect(a.GapPoint + StagCanal37 + new Vec3(0, 1.2, 3), new Vec3(0, 0, -1), out RayHit gh) || gh.distance > 5;
        Console.WriteLine($"  the entry gap is open at load: {StagP37(gapOpen)}");
        if (!gapOpen) fails++;
        return fails;
    }

    /// <summary>A scripted skater round the loop on its middle lane: steers at the path 6 m ahead; `pump` dashes whenever a pip is up (and 0.35 s
    /// since the last dash). Returns (top, lap-average) speeds over `laps`.</summary>
    static (double top, double avg, int dashes) StagSkate37(SurvivalSim sim, bool pump, int laps, double lane = 0)
    {
        Stag37Arena a = sim.stag37Arena;
        double dt = sim.cfg.Dt;
        var si = (ScriptedInput)sim.input;
        Vec3 at = a.PointAt(2, lane, out Vec3 hd) + StagCanal37;
        sim.Respawn(at, StagYaw37(hd) * 180 / Math.PI);
        double top = 0, sStart = 2, travelled = 0, sPrev = 2;
        int steps = 0, dashCool = 0, dashes = 0, cap = (int)(laps * 60 / dt), real = 0, failed = 0;
        sim.mover.Events += e => { if (e.type == MoverEventType.Dash) real++; else if (e.type == MoverEventType.DashFail) failed++; };
        bool counting = false;
        int countSteps = 0;
        while (steps < cap)
        {
            Vec3 l = sim.mover.position - StagCanal37;
            a.Project(l.x, l.z, out double s, out double lat);
            Vec3 aim = a.PointAt(s + 6, lane);
            Vec3 d = aim - l;
            double yaw = JsMath.Atan2(-d.x, -d.z);
            var fr = new InputFrame { yaw = yaw, moveZ = 1, sprint = new Skygrind.Core.Button { held = !pump, pressed = !pump && steps == 0 } };
            if (dashCool > 0) dashCool--;
            if (pump && dashCool == 0 && sim.mover.grounded && sim.mover.pips >= 1) { fr.dash = new Skygrind.Core.Button { held = true, pressed = true }; dashCool = (int)(0.35 / dt); dashes++; }
            si.SetLook(yaw, 0);
            si.Set(fr, default);
            sim.Step();
            sim.hitstopLeft = 0;
            steps++;
            double ds = a.Ahead(sPrev, s);
            sPrev = s;
            if (steps * dt > 6) { counting = true; }
            if (counting) { travelled += ds; countSteps++; double sp = JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z); if (sp > top) top = sp; }
            if (travelled >= a.Length * Math.Max(1, laps - 1)) break;
        }
        if (StagTrace37) Console.WriteLine($"    skater: dash presses {dashes}, dashes {real}, fails {failed}, stamina rules {sim.mover.stamina.rules.Any}");
        return (top, countSteps > 0 ? travelled / (countSteps * dt) : 0, real);
    }

    static int StagSpeeds37()
    {
        SurvivalSim sim = Sim37S(1, true);
        var glide = StagSkate37(sim, false, 3);
        sim = Sim37S(1, true);
        var pump = StagSkate37(sim, true, 3);
        Console.WriteLine($"speeds: skating the canal's middle lane (after 6 s): glide only top {glide.top:0.00} m/s, lap average {glide.avg:0.00} m/s; pumping dashes ({pump.dashes}) top {pump.top:0.00} m/s, lap average {pump.avg:0.00} m/s");
        GameConfig c = sim.cfg;
        if (c.Has("design.n37.stag37.run.speed"))
        {
            double v = c.Num("design.n37.stag37.run.speed"), g = c.Num("design.n37.stag37.slip.gain");
            Console.WriteLine($"  the Stag's path speed {v:0.00} m/s vs the pumped lap average {pump.avg:0.00} (x{v / Math.Max(1e-6, pump.avg):0.00}); the slipstream's cap {pump.avg * (1 + g):0.00} m/s: catching it needs the slipstream: {StagP37(v > pump.avg && v < pump.avg * (1 + g))}");
            return v > pump.avg && v < pump.avg * (1 + g) ? 0 : 1;
        }
        return 0;
    }
}
