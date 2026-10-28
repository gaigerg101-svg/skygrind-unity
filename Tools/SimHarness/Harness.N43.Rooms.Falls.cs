// NIGHT43 builder E1 round 2: `n43rooms falls [--file <level.json>] [--room b2]` (L1's THE GANTRY: fallReturn43's `by`). A campaign run of the
// level (rooms43 + foundry43 on), DOORFREE into the room; for each `by` entry the player is dropped from 2 m over its rect's centre into the
// pit; the fallReturn43 that follows must put the player at THAT entry's `to` (its own lip), with the damage. Also the single-`to` form: a fall
// with `by` removed returns to `to`. Exit 0 when every pit returns to its own lip.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N43RoomsMore6(string what, string[] args, ref int code)
    {
        if (what == "falls") { code = N43RmFalls(args); return; }
        if (what == "fusion") { code = N43RmFusion(args); return; }
        if (what == "roles") { code = N43RmRoles(args); return; }
    }

    static int N43RmFalls(string[] args)
    {
        GameConfig cfg = Config();
        string path = Path.GetFullPath(N43Arg(args, "--file", Path.Combine("Design", "levels", "n43-foundry.json")));
        string roomId = N43Arg(args, "--room", "b2");
        int bad = 0;
        foreach (bool dropBy in new[] { false, true })
        {
            var json = PortData.Obj(PortData.LoadJson(path));
            Dictionary<string, object> row = null;
            foreach (object o in PortData.Arr(PortData.Obj(json["campaign"])["rooms"]))
                if (o is Dictionary<string, object> d && d["id"] as string == roomId) row = d;
            if (row == null || !(row.TryGetValue("fallReturn43", out object fo) && fo is Dictionary<string, object> fr)) { Console.WriteLine($"n43rooms falls: {roomId} has no fallReturn43 in {path}"); return 2; }
            Vec3 lastPit = default;
            if (fr.TryGetValue("by", out object byo) && byo is List<object> byl && byl.Count > 0 && byl[byl.Count - 1] is Dictionary<string, object> lb)
            {
                List<object> q = PortData.Arr(lb["rect"]);
                lastPit = new Vec3((PortData.Num(q[0]) + PortData.Num(q[2])) / 2, 0, (PortData.Num(q[1]) + PortData.Num(q[3])) / 2);
            }
            if (dropBy) fr.Remove("by");
            SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(json));
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            int bits = N43RmBlocks(cfg, true, false).Bits | 1 << Array.IndexOf(N43Blocks.Names, "foundry43");
            sim.n43Override = N43Blocks.FromBits(bits);
            int falls = 0;
            FallReturn43Event last = null;
            sim.events.On<FallReturn43Event>("fallReturn43", e => { falls++; last = new FallReturn43Event { x = e.x, y = e.y, z = e.z, damage = e.damage }; });
            sim.StartRun(1, null, null);
            LevelDef lv = sim.campaignLevel;
            int ri = lv.rooms.FindIndex(q => q.id == roomId);
            RoomDef r = lv.rooms[ri];
            for (int k = 0; k < 120 && sim.campaign == null; k++) sim.Step();
            sim.campaign.SetDoorFree29(true);
            sim.Respawn(r.spawn, r.spawnYawDeg);
            for (int k = 0; k < 2400 && sim.campaign.room != ri; k++) { sim.health.hp = sim.health.max; sim.Step(); }
            var tests = new List<(string name, Vec3 drop, Vec3 want)>();
            if (!dropBy && r.fallBy43 != null)
                for (int i = 0; i < r.fallBy43.Count; i++)
                {
                    FallBy43 b = r.fallBy43[i];
                    tests.Add(($"pit {i + 1}", new Vec3((b.x0 + b.x1) / 2, r.fallBelowY43 + 2, (b.z0 + b.z1) / 2), b.to));
                }
            else if (dropBy)
            {
                // the single-`to` form (`by` removed): a fall in the LAST pit returns to `to` (round 1's rule, unchanged)
                tests.Add(("single to (by removed), last pit", new Vec3(lastPit.x, r.fallBelowY43 + 2, lastPit.z), r.fallTo43));
            }
            foreach (var (name, drop, want) in tests)
            {
                int f0 = falls;
                sim.health.hp = sim.health.max;
                double hp0 = sim.health.hp;
                sim.Respawn(drop, r.spawnYawDeg);
                int k = 0;
                for (; k < 600 && falls == f0; k++) sim.Step();
                Vec3 p = sim.mover.position;
                double dx = p.x - want.x, dz = p.z - want.z;
                bool ok = falls > f0 && Math.Sqrt(dx * dx + dz * dz) < 0.6;
                if (!ok) bad++;
                Console.WriteLine($"  {Path.GetFileNameWithoutExtension(path)} {roomId} {name}: dropped at ({drop.x:0.0}, {drop.y:0.0}, {drop.z:0.0}) -> {(falls > f0 ? $"returned after {k * sim.step:0.00} s to ({p.x:0.00}, {p.y:0.00}, {p.z:0.00}), want ({want.x:0.00}, {want.z:0.00}); health {hp0:0} -> {sim.health.hp:0}" : "NO fallReturn43")} {(ok ? "PASS" : "RED")}");
            }
        }
        Console.WriteLine($"n43rooms falls SUMMARY: {(bad == 0 ? "PASS" : $"RED ({bad})")}");
        return bad == 0 ? 0 : 1;
    }
}
