// NIGHT43 builder E1 round 2: `n43rooms idle` (G4 / T-4 "teach, confirm, then spawn"). Every fight room of levels 1-3 entered by a DOORWAY
// pilot: put at the room's spawn facing in, it stands --wait s (a person looking into the room), then steps 4 m in along its yaw; the room's
// enemies are killed 0.5 s after they appear once the pilot is in (as `n43rooms clear`'s kill path), taught links get their lessons done. Per room:
// the enemy-seconds of bodies alive IN VIEW (in the pilot's 90 deg front cone, within 40 m) while it still stood in the doorway (before the fight
// started for it), stand-ins counted apart, and when the first wave appeared (s after the entry).
//   n43rooms idle [--nocommit] [--n43] [--levels foundry,iceline,kiln] [--wait s] [--seed n]
//   --nocommit: rooms43 on without its commit gate and stand-in rule (the control: round 1's game).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N43RoomsMore4(string what, string[] args, ref int code)
    {
        if (what == "idle") { code = N43RmIdle(args); return; }
        N43RoomsMore5(what, args, ref code);
    }

    static partial void N43RoomsMore5(string what, string[] args, ref int code);

    static int N43RmIdle(string[] args)
    {
        bool noCommit = Array.IndexOf(args, "--nocommit") >= 0, n43 = Array.IndexOf(args, "--n43") >= 0;
        string[] levels = N43Arg(args, "--levels", "foundry,iceline,kiln").Split(',');
        double wait = double.Parse(N43Arg(args, "--wait", "5"), CultureInfo.InvariantCulture);
        uint seed = uint.Parse(N43Arg(args, "--seed", "1"), CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        Console.WriteLine($"n43rooms idle: rooms43 on{(noCommit ? ", the commit gate and stand-in rule OFF (the control)" : "")}, the pilot waits {wait:0.0} s in each doorway, seed {seed}");
        double all = 0, allSi = 0;
        int held = 0;
        foreach (string lvName in levels)
        {
            string id = N43RmLevelId(cfg, lvName, n43);
            string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
            var rows = N43RmIdleRun(cfg, path, n43, seed, wait, noCommit);
            double sum = rows.Sum(r => r.idle), sumSi = rows.Sum(r => r.standIn);
            all += sum; allSi += sumSi;
            held += rows.Count(r => r.fight && !r.opened);
            Console.WriteLine($"  {id,-12} enemy-seconds in view from the doorway {sum:0.0} (stand-ins {sumSi:0.0})");
            foreach (var r in rows.Where(q => q.fight || q.standIn > 0))
                Console.WriteLine($"      {r.id,-5} first wave at {(r.firstWave < 0 ? "-" : r.firstWave.ToString("0.00", CultureInfo.InvariantCulture))} s, in view before the fight {r.idle:0.0} enemy-s, stand-ins {r.standIn:0.0} s, door {(r.opened ? "open" : "SHUT")}");
        }
        Console.WriteLine($"n43rooms idle SUMMARY: enemy-seconds in view from the doorway {all:0.0}, stand-in seconds {allSi:0.0}, doors held {held}{(noCommit ? " (the control)" : "")}");
        return held == 0 ? 0 : 1;
    }

    sealed class N43IdleRow
    {
        public string id;
        public bool fight, opened;
        public double idle, standIn, firstWave = -1;
    }

    static List<N43IdleRow> N43RmIdleRun(GameConfig cfg, string path, bool n43Levels, uint seed, double wait, bool noCommit)
    {
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.n43Override = N43RmBlocks(cfg, true, n43Levels);
        sim.rooms43CommitOff = noCommit;
        int st = 0, enterStep = 0, cur = -1;
        var rows = new List<N43IdleRow>();
        sim.events.On<RoomEvent>("roomEnter", e => { enterStep = st; cur = e.room; });
        sim.events.On<RoomEvent>("roomClear", e => { N43IdleRow r = rows.Find(q => q.id == e.id); if (r != null) r.opened = true; });
        sim.events.On<SpawnEvent>("enemySpawn", e =>
        {
            if (cur < 0 || cur >= rows.Count) return;
            N43IdleRow r = rows[cur];
            if (r.firstWave < 0 && st > enterStep) r.firstWave = (st - enterStep) * sim.step; // (the stand-ins spawn on the entry step itself)
        });
        sim.StartRun(seed, null, null);
        LevelDef level = sim.campaignLevel;
        if (level == null) return rows;
        foreach (RoomDef r in level.rooms) rows.Add(new N43IdleRow { id = r.id, fight = r.kind != "boss" && r.WaveCount > 0 });
        var born = new Dictionary<Enemy, int>();
        int waitSteps = MovementMath.StepsFor(wait, sim.step), placedRoom = -2, steppedRoom = -2, forced = -1;
        int total = MovementMath.StepsFor(150 * level.rooms.Count, sim.step);
        double cone = Math.Cos(45 * Math.PI / 180);
        for (; st < total; st++)
        {
            Campaign24 c = sim.campaign;
            bool inDoor = false;
            if (c != null && c.room >= 0)
            {
                RoomDef r = level.rooms[c.room];
                if (r.kind == "boss") break;
                if (c.phase == Campaign24.Phase.Cleared && c.room + 1 < level.rooms.Count)
                {
                    if (level.rooms[c.room + 1].kind == "boss") break;
                    sim.Respawn(level.rooms[c.room + 1].spawn, level.rooms[c.room + 1].spawnYawDeg);
                }
                else if (c.phase == Campaign24.Phase.Fight)
                {
                    if (placedRoom != c.room) { placedRoom = c.room; sim.Respawn(r.spawn, r.spawnYawDeg); }
                    int age = st - enterStep;
                    inDoor = age < waitSteps;
                    if (!inDoor && steppedRoom != c.room)
                    {
                        steppedRoom = c.room;
                        double yaw = r.spawnYawDeg * Math.PI / 180;
                        sim.Respawn(r.spawn + new Vec3(-Math.Sin(yaw) * 4, 0, -Math.Cos(yaw) * 4), r.spawnYawDeg);
                    }
                    if (r.WaveCount == 0 && c.lesson != null && !c.lesson.Done && forced != c.room && age * sim.step > 1) { forced = c.room; c.lesson.index = c.lesson.defs.Count; }
                    if (age * sim.step > 140) break;
                }
            }
            sim.health.hp = sim.health.max;
            if (sim.flow == Skygrind.Game.Flow.Down) sim.flow = Skygrind.Game.Flow.Playing;
            var now = new List<Enemy>(sim.enemies.all);
            Vec3 p = sim.mover.position;
            double yawP = sim.input.Yaw, fx = -Math.Sin(yawP), fz = -Math.Cos(yawP);
            foreach (Enemy e in now)
            {
                if (!e.alive || !(e is Husk h)) continue;
                if (inDoor && cur >= 0)
                {
                    double dx = e.position.x - p.x, dz = e.position.z - p.z, d = Math.Sqrt(dx * dx + dz * dz);
                    if (d > 0.1 && d < 40 && (dx * fx + dz * fz) / d >= cone)
                    {
                        if (e.standIn29) rows[cur].standIn += sim.step;
                        else rows[cur].idle += sim.step;
                    }
                }
                if (e.standIn29 || inDoor) continue;
                if (!born.TryGetValue(e, out int b0)) { born[e] = st; continue; }
                int ageE = st - b0, first = MovementMath.StepsFor(0.5, sim.step);
                if (ageE >= first && (ageE - first) % MovementMath.StepsFor(3, sim.step) == 0) sim.loadout.Damage(h, h.hp + 100000, false, "revolver", p, shot: 0);
            }
            foreach (Enemy e in now) if (!e.alive) born.Remove(e);
            sim.Step();
            if (sim.flow == Skygrind.Game.Flow.Results) break;
        }
        return rows;
    }
}
