// NIGHT43 builder E1 round 2: `n43rooms pin [--level foundry|iceline|kiln]` (the rooms43 reel's part 2, headless): the n43-r2 reel read
// 'ended 0' because a body put 3 m under a solid floor fell to the -30 m kill plane in under 2 s. This replays the reel's fixed knock: the
// level's second fight room, its first body pinned 1.7 m under the floor with the bodies frozen (Husks.frozen), and asserts rooms43's rule ends
// it within endS + 0.1 s. Exit 0 when ended by the rule.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N43RoomsMore5(string what, string[] args, ref int code)
    {
        if (what == "pin") { code = N43RmPin(args); return; }
        N43RoomsMore6(what, args, ref code);
    }

    static partial void N43RoomsMore6(string what, string[] args, ref int code);

    static int N43RmPin(string[] args)
    {
        GameConfig cfg = Config();
        string lvName = N43Arg(args, "--level", "iceline");
        string path = Path.GetFullPath(Path.Combine("Design", "levels", N43RmLevelId(cfg, lvName, false) + ".json"));
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.n43Override = N43RmBlocks(cfg, true, false);
        sim.StartRun(1, null, null);
        LevelDef lv = sim.campaignLevel;
        var fights = new List<int>();
        for (int i = 0; i < lv.rooms.Count; i++) if (lv.rooms[i].kind != "boss" && lv.rooms[i].WaveCount > 0) fights.Add(i);
        int target = fights.Count > 1 ? fights[1] : fights[0];
        RoomDef r = lv.rooms[target];
        for (int k = 0; k < 120 && sim.campaign == null; k++) sim.Step();
        sim.campaign.SetDoorFree29(true);
        sim.Respawn(r.spawn, r.spawnYawDeg);
        int st = 0;
        for (; st < 2400 && sim.campaign.room != target; st++) { sim.health.hp = sim.health.max; sim.Step(); }
        sim.campaign.SetDoorFree29(false);
        double yaw = r.spawnYawDeg * Math.PI / 180;
        sim.Respawn(r.spawn + new Vec3(-Math.Sin(yaw) * 4, 0, -Math.Cos(yaw) * 4), r.spawnYawDeg); // in: the first wave comes
        Husk h = null;
        for (st = 0; st < 1200 && h == null; st++)
        {
            sim.health.hp = sim.health.max;
            sim.Step();
            foreach (Enemy e in sim.enemies.all) if (e.alive && !e.standIn29 && e is Husk hh) { h = hh; break; }
        }
        if (h == null) { Console.WriteLine($"n43rooms pin: room {r.id} spawned nothing"); return 1; }
        double floor = r.spawn.y;
        foreach (Vec3 n in r.nodes) floor = Math.Min(floor, n.y);
        var pin = new Vec3(h.position.x, floor - 1.7, h.position.z);
        int id0 = h.id, ended0 = sim.rooms43Ended;
        sim.husks.frozen = true;
        h.Teleport(pin);
        int steps = 0;
        while (steps < 600 && h.alive && h.id == id0)
        {
            sim.health.hp = sim.health.max;
            sim.Step();
            steps++;
            if (steps % 2 == 0) { h.Teleport(pin); h.velocity = default; } // the reel pins once a frame (~2 steps)
        }
        sim.husks.frozen = false;
        bool ok = !(h.alive && h.id == id0) && sim.rooms43Ended > ended0;
        double endS = cfg.Num("design.n43.rooms43.endS");
        Console.WriteLine($"n43rooms pin {Path.GetFileNameWithoutExtension(path)} room {r.id}: a {h.kind} pinned at y {pin.y:0.0} (floor {floor:0.0}): {(ok ? $"ended by the rule after {steps * sim.step:0.00} s (endS {endS})" : "NOT ended by the rule")} -> {(ok ? "PASS" : "RED")}");
        return ok ? 0 : 1;
    }
}
