// NIGHT37 builder O (NIGHT37.md O1; BOSSES.md "L4 THE LOOM"): THE LOOM's headless checks. One entry, N37Loom (declared and called in Program.cs).
//   loom37 [level|fall|cross|fight|band|all] [--seeds N] [--cap S] [--off37]
//     The level is Design/levels/n37-span.json (Tools/night/gen-n37-span-boss.cjs) on a CAMPAIGN run with sim.n37.loom37 forced on (boss37 too).
//     level   the run knows its canyon: Loom37Level, the walk-in flag, the anchors and ledges read, every anchor a live grapple node; --off37: none of it.
//     fall    the player on the start ledge, then on tower 1's top, then dropped into the void: back on the last ledge stood on, -fallCostHp.
//     cross   Loom37Pilot pulls across THE CANYON (start ledge -> t1w -> t2w -> t3w -> end): the time, pulls, falls, waits for a charge.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N37Loom(string cmd, string[] args, ref int code)
    {
        if (cmd != "loom37") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = ArgInt36(args, "--seeds", 10);
        double cap = ArgInt36(args, "--cap", 90);
        bool off = Array.IndexOf(args, "--off37") >= 0 || N37Blocks.ForceOff;
        Trace37O = Array.IndexOf(args, "--trace") >= 0;
        Frames37O = Array.IndexOf(args, "--frames") >= 0;
        int fails = 0;
        if (what == "level" || what == "all") fails += Loom37LevelCheck(off);
        if (!off && (what == "fall" || what == "all")) fails += Loom37FallCheck();
        if (!off && (what == "cross" || what == "all")) fails += Loom37CrossCheck(cap);
        Loom37Extra(what, seeds, cap, off, ref fails);
        Console.WriteLine(fails == 0 ? "loom37: GREEN" : $"loom37: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static partial void Loom37Extra(string what, int seeds, double cap, bool off, ref int fails);

    static bool Trace37O, Frames37O;

    static string Pass37O(bool ok) => ok ? "PASS" : "FAIL";

    /// <summary>A campaign run on n37-span with loom37 (and boss37) on, or every n37 block off.</summary>
    public static SurvivalSim Loom37Sim(uint seed, bool on)
    {
        GameConfig cfg = Config();
        string path = Path.Combine(MapGenRepo, "Design", "levels", "n37-span.json");
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        sim.n37Override = on ? new N37Blocks { boss37 = true, loom37 = true } : N37Blocks.Off;
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        return sim;
    }

    static int Loom37LevelCheck(bool off)
    {
        SurvivalSim sim = Loom37Sim(1, !off);
        int live = 0;
        foreach (var a in sim.loom37Anchors) if (a.node < sim.map.grappleNodes.Count && Vec3.DistanceTo(sim.map.grappleNodes[a.node], a.at) < 1e-9) live++;
        Console.WriteLine($"level: n37-span, {sim.map.grappleNodes.Count} grapple nodes, Loom37Level {sim.Loom37Level}, walk-in {sim.boss37WalkInHere}, canyon {sim.loom37CanyonId}, "
            + $"{sim.loom37Anchors.Count} anchors ({live} live at their place), {sim.loom37Ledges.Count} ledges, grapple35 pull {sim.Grapple35Pull} (n35.grapple35 {sim.n35.grapple35}, n31.grapplePull {sim.n31.grapplePull}, tap {sim.grappleTap31.tapSteps}), swing node {sim.loom37SwingNode}");
        if (off)
        {
            bool ok0 = !sim.Loom37Level && !sim.boss37WalkInHere && sim.loom37Anchors.Count == 0;
            Console.WriteLine($"  --off37: nothing of THE LOOM's level side runs: {Pass37O(ok0)}");
            return ok0 ? 0 : 1;
        }
        bool ok = sim.Loom37Level && sim.boss37WalkInHere && sim.loom37CanyonId == "o3" && sim.loom37Anchors.Count == 13 && live == 13 && sim.loom37Ledges.Count == 8 && sim.loom37SwingNode >= 0;
        // the swing moves: two samples a quarter period apart
        Vec3 s0 = sim.map.grappleNodes[sim.loom37SwingNode];
        for (int i = 0; i < 96; i++) sim.Step();
        Vec3 s1 = sim.map.grappleNodes[sim.loom37SwingNode];
        Console.WriteLine($"  swing anchor moved {Vec3.DistanceTo(s0, s1):0.00} m in 0.8 s (x {s0.x:0.00} -> {s1.x:0.00}, rest x {sim.loom37SwingRest.x:0.00})");
        ok &= Vec3.DistanceTo(s0, s1) > 3;
        Console.WriteLine($"  level side: {Pass37O(ok)}");
        return ok ? 0 : 1;
    }

    static int Loom37FallCheck()
    {
        SurvivalSim sim = Loom37Sim(1, true);
        var start = sim.loom37Ledges.Find(l => l.id == "start");
        var t1 = sim.loom37Ledges.Find(l => l.id == "t1w");
        int fails = 0;
        foreach (var stand in new[] { start, t1 })
        {
            sim.Respawn(new Vec3(stand.at.x, stand.y, stand.at.z), 0);
            for (int i = 0; i < 30; i++) sim.Step();
            string stood = sim.loom37LastLedge?.id;
            double hp0 = sim.health.hp;
            int falls0 = sim.loom37Falls;
            // a drop into the void 6 m north of tower 2's south end (no ledge under it)
            sim.Respawn(new Vec3(-17, 2, -320), 0);
            int st = 0;
            while (sim.loom37Falls == falls0 && st < 600) { sim.Step(); st++; }
            Vec3 p = sim.mover.position;
            bool ok = stood == stand.id && sim.loom37Falls == falls0 + 1 && Math.Abs(hp0 - sim.health.hp - sim.cfg.Num("design.n37.loom37.fallCostHp")) < 1e-9
                && Math.Abs(p.x - stand.at.x) < 0.01 && Math.Abs(p.z - stand.at.z) < 0.01 && Math.Abs(p.y - stand.y) < 0.01;
            Console.WriteLine($"fall: stood on {stood}, dropped into the void: back at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}) after {st * sim.step:0.00} s, hp {hp0:0} -> {sim.health.hp:0}: {Pass37O(ok)}");
            if (!ok) fails++;
        }
        return fails;
    }

    static int Loom37CrossCheck(double cap)
    {
        SurvivalSim sim = Loom37Sim(1, true);
        var start = sim.loom37Ledges.Find(l => l.id == "start");
        sim.Respawn(new Vec3(start.at.x, 0, start.at.z), 0);
        var si = (ScriptedInput)sim.input;
        var pilot = new Loom37Pilot();
        int st = 0, capSteps = (int)(cap * 120);
        var log = new List<string>();
        string lastLedge = null;
        sim.events.On<string>("loom37Ledge", id => log.Add($"{st * sim.step:0.0}s {id}"));
        sim.events.On<string>("loom37Fall", id => log.Add($"{st * sim.step:0.0}s FALL->{id}"));
        while (st < capSteps && pilot.at < pilot.route.Count)
        {
            pilot.Step(sim, st);
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            st++;
            if (Trace37O && st % 60 == 0) Console.WriteLine($"    {st * sim.step:0.0}s {pilot.phase} at {pilot.at} pos ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z:0.0}) rope {sim.mover.rope.phase} charges {sim.grappleCharges32}");
        }
        // walk on to stand on the end ledge
        for (int i = 0; i < 240 && sim.loom37LastLedge?.id != "end"; i++) { si.Set(new InputFrame { yaw = 0, moveZ = 1 }, new CombatInput { weapon = -1 }); sim.Step(); st++; }
        lastLedge = sim.loom37LastLedge?.id;
        bool ok = lastLedge == "end";
        Console.WriteLine($"cross: {(ok ? "ACROSS" : "NOT across")} in {st * sim.step:0.0} s, {pilot.pulls} pulls ({pilot.misses} misses), {sim.loom37Falls} falls, {pilot.waits * sim.step:0.0} s waiting for a charge, "
            + $"{sim.loom37RideRefills} ride refills, airborne {pilot.airborneSteps / Math.Max(1, pilot.totalSteps) * 100:0} %, last ledge {lastLedge}: {Pass37O(ok)}");
        Console.WriteLine("  ledges: " + string.Join("; ", log));
        return ok ? 0 : 1;
    }
}
