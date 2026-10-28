// NIGHT43 builder E1: `n43rooms still` (G22 "fail to move"). Every fight room of levels 1-3 with the harness player of `n43rooms clear` (idle,
// invulnerable, at the room's exit side) but the enemies are left alive for --hold s before the person kills them, and the brain is told every
// enemy is ON screen (the worst case for NIGHT42's watchdog, which relocated only off screen; headless runs are otherwise all off screen).
// Per enemy: its longest STILL spell (horizontal speed under 0.3 m/s, alive, not in an attack, not a stand-in or a perching LANCER) while the
// player is within 16 m. Prints per level the max spell by kind, the enemies with a spell over 3 s, the on-screen tells / relocations.
//   n43rooms still [--off] [--n43] [--levels foundry,iceline,kiln] [--seeds n] [--hold s]
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N43RoomsMore2(string what, string[] args, ref int code)
    {
        if (what == "still") { code = N43RmStill(args); return; }
        N43RoomsMore3(what, args, ref code);
    }

    static partial void N43RoomsMore3(string what, string[] args, ref int code);

    static bool N43StillDbg;

    static int N43RmStill(string[] args)
    {
        N43StillDbg = Array.IndexOf(args, "--dbg") >= 0;
        bool off = Array.IndexOf(args, "--off") >= 0, n43 = Array.IndexOf(args, "--n43") >= 0, noWatch = Array.IndexOf(args, "--nowatch") >= 0;
        string[] levels = N43Arg(args, "--levels", "foundry,iceline,kiln").Split(',');
        int seeds = int.Parse(N43Arg(args, "--seeds", "3"), CultureInfo.InvariantCulture);
        double hold = double.Parse(N43Arg(args, "--hold", "20"), CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        Console.WriteLine($"n43rooms still: rooms43 {(off ? "OFF (the control)" : noWatch ? "on, its on-screen watchdog OFF (the control)" : "on")}, every enemy on screen, enemies alive {hold:0} s each, {seeds} seed(s)");
        double worst = 0;
        int over3 = 0, bodies = 0;
        foreach (string lvName in levels)
        {
            string id = N43RmLevelId(cfg, lvName, n43);
            string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
            var byKind = new SortedDictionary<string, List<double>>();
            int tells = 0, rel43 = 0, rel = 0;
            for (int s = 0; s < seeds; s++)
            {
                var (spells, t, r43, r0) = N43RmStillRun(cfg, path, !off, n43, (uint)(1 + s), hold, noWatch);
                tells += t; rel43 += r43; rel += r0;
                foreach (var (kind, sec) in spells)
                {
                    if (!byKind.TryGetValue(kind, out var l)) byKind[kind] = l = new List<double>();
                    l.Add(sec);
                }
            }
            var all = byKind.Values.SelectMany(x => x).ToList();
            int o3 = all.Count(x => x > 3);
            over3 += o3; bodies += all.Count;
            if (all.Count > 0) worst = Math.Max(worst, all.Max());
            Console.WriteLine($"  {id,-12} enemies {all.Count}: max still {(all.Count == 0 ? 0 : all.Max()):0.0} s, over 3 s {o3}; on-screen tells {tells}, steps {rel43}; off-screen relocations {rel}");
            foreach (var kv in byKind)
            {
                kv.Value.Sort();
                Console.WriteLine($"      {kv.Key,-8} n {kv.Value.Count,3}: median {kv.Value[kv.Value.Count / 2]:0.0} s, max {kv.Value.Max():0.0} s, over 3 s {kv.Value.Count(x => x > 3)}");
            }
        }
        Console.WriteLine($"n43rooms still SUMMARY: {bodies} enemies, max still {worst:0.0} s, {over3} with a still spell over 3 s near the player ({(off ? "rooms43 off" : "rooms43 on")})");
        return 0;
    }

    static (List<(string kind, double sec)> spells, int tells, int rel43, int rel) N43RmStillRun(GameConfig cfg, string path, bool rooms, bool n43Levels, uint seed, double hold, bool noWatch)
    {
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.n43Override = N43RmBlocks(cfg, rooms, n43Levels);
        sim.brain.onScreen = _ => true; // every enemy in view (the person looks at the fight)
        sim.rooms43NoWatch = noWatch; // the control: the door rules on, the on-screen watchdog off
        int st = 0, enterStep = 0;
        sim.events.On<RoomEvent>("roomEnter", e => enterStep = st);
        sim.StartRun(seed, null, null);
        LevelDef level = sim.campaignLevel;
        var spells = new List<(string, double)>();
        if (level == null) return (spells, 0, 0, 0);
        var born = new Dictionary<Enemy, int>();
        var still = new Dictionary<Enemy, int>();
        var best = new Dictionary<Enemy, int>();
        var last = new Dictionary<Enemy, Vec3>();
        int holdSteps = MovementMath.StepsFor(hold, sim.step), placedRoom = -2, forced = -1;
        int total = MovementMath.StepsFor(150 * level.rooms.Count, sim.step);
        int rel0 = sim.brain.relocations;
        double slow = 0.3 * sim.step;
        for (; st < total; st++)
        {
            Campaign24 c = sim.campaign;
            if (c != null && c.room >= 0)
            {
                RoomDef r = level.rooms[c.room];
                if (r.kind == "boss") break;
                if (c.phase == Campaign24.Phase.Cleared && c.room + 1 < level.rooms.Count)
                {
                    if (level.rooms[c.room + 1].kind == "boss") break;
                    sim.Respawn(level.rooms[c.room + 1].spawn, level.rooms[c.room + 1].spawnYawDeg);
                }
                else if (placedRoom != c.room && c.phase == Campaign24.Phase.Fight)
                {
                    placedRoom = c.room;
                    Vec3 at = r.exit;
                    if (sim.zones.ZoneAt(new Vec3(at.x, at.y + 1, at.z)) != r.id) at = r.spawn;
                    sim.Respawn(at, r.spawnYawDeg);
                }
                if (c.phase == Campaign24.Phase.Fight && r.WaveCount == 0 && c.lesson != null && !c.lesson.Done && forced != c.room && (st - enterStep) * sim.step > 1)
                {
                    forced = c.room;
                    c.lesson.index = c.lesson.defs.Count;
                }
                if (c.phase == Campaign24.Phase.Fight && (st - enterStep) * sim.step > 140) break;
            }
            sim.health.hp = sim.health.max;
            if (sim.flow == Skygrind.Game.Flow.Down) sim.flow = Skygrind.Game.Flow.Playing;
            var now = new List<Enemy>(sim.enemies.all);
            Vec3 p = sim.mover.position;
            foreach (Enemy e in now)
            {
                if (!e.alive || e.standIn29 || !(e is Husk h) || h is Lancer) continue;
                if (!born.ContainsKey(e)) { born[e] = st; last[e] = e.position; still[e] = 0; best[e] = 0; continue; }
                Vec3 q = last[e];
                double mv = Math.Sqrt((e.position.x - q.x) * (e.position.x - q.x) + (e.position.z - q.z) * (e.position.z - q.z));
                last[e] = e.position;
                double dp = Math.Sqrt((e.position.x - p.x) * (e.position.x - p.x) + (e.position.z - p.z) * (e.position.z - p.z));
                bool isStill = mv < slow && !h.committed && dp <= 16;
                still[e] = isStill ? still[e] + 1 : 0;
                if (still[e] > best[e]) best[e] = still[e];
                if (N43StillDbg && still[e] == MovementMath.StepsFor(3, sim.step))
                    Console.WriteLine($"      dbg {Path.GetFileNameWithoutExtension(path)} room {sim.campaign?.Room?.id} {e.kind} #{e.id} still 3 s at ({e.position.x:0.0}, {e.position.y:0.0}, {e.position.z:0.0}) d {dp:0.0} mode {sim.brain.ModeOf(e.id) ?? "(no brain)"} stuck {sim.brain.StuckFor(e.id):0.0} grounded {h.grounded} knock {h.knockSteps} jump {(h.jump != null)} stagger {h.staggerSteps} nudges {sim.brain.nudges43}");
                if (st - born[e] >= holdSteps) sim.loadout.Damage(h, h.hp + 100000, false, "revolver", p, shot: 0);
            }
            foreach (Enemy e in now)
                if (!e.alive && born.ContainsKey(e))
                {
                    spells.Add((e.kind, best[e] * sim.step));
                    born.Remove(e); last.Remove(e); still.Remove(e); best.Remove(e);
                }
            sim.Step();
            if (sim.flow == Skygrind.Game.Flow.Results) break;
        }
        foreach (var kv in best) spells.Add((kv.Key.kind, kv.Value * sim.step));
        return (spells, sim.brain.tells43, sim.brain.relocations43, sim.brain.relocations - rel0);
    }
}
