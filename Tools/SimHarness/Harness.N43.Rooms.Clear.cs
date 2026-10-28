// NIGHT43 builder E1: `n43rooms clear` (G5: "a door always opens when its room is empty"). Every room of levels 1-3 is played by a harness
// player who never performs a verb: placed at the room's exit side (so every spawns33 trigger line is crossed), kept at full health, and every
// enemy of the room is removed by one KILL PATH 0.5 s after it appears:
//   kill   the loadout's Damage (a real kill: the kill event, the lessons' kill counts)
//   pit    the body moved 3 m under its room's floor (a dry pit; a fall through geometry)
//   stuck  the body moved 12 m outside its room's rect (out of reach)
//   lava   the body moved onto the room's lava (Lava30's own burn), or `pit` where the room has none
// A taught link (lessons, no waves) has its lessons marked done after 1 s (a person doing the verb; not under test). The run ends at the
// level's boss room. Per room: did its door open (roomClear) within --cap s, and when; RED when a fight room's door stays shut.
//   n43rooms clear [--off] [--n43] [--levels foundry,iceline,kiln] [--paths kill,pit,stuck,lava] [--seed n] [--cap s] [--trace]
//   --off: rooms43 off (NIGHT42's door rule: the control); --n43: the n43 level files (foundry43 / iceline43 / kiln43 on).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N43RoomsMore(string what, string[] args, ref int code)
    {
        if (what == "clear") { code = N43RmClear(args); return; }
        N43RoomsMore2(what, args, ref code);
    }

    static partial void N43RoomsMore2(string what, string[] args, ref int code);

    static string N43Arg(string[] args, string name, string def)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
    }

    /// <summary>The level file a level row plays: NIGHT42's (n42-foundry, n40-iceline, n41-kiln) or, with n43, the n43 row's levelTo.</summary>
    static string N43RmLevelId(GameConfig cfg, string name, bool n43)
    {
        if (n43) return cfg.Str("design.n43." + name + "43.levelTo");
        return name == "foundry" ? cfg.Str("design.n42.foundry42.levelTo") : name == "iceline" ? cfg.Str("design.n40.iceline40.levelTo") : cfg.Str("design.n41.kiln41.levelTo");
    }

    static N43Blocks N43RmBlocks(GameConfig cfg, bool rooms, bool n43Levels)
    {
        int bits = N43Blocks.From(cfg).Bits;
        int r = 1 << Array.IndexOf(N43Blocks.Names, "rooms43");
        bits = rooms ? bits | r : bits & ~r;
        if (n43Levels) foreach (string n in new[] { "foundry43", "iceline43", "kiln43" }) bits |= 1 << Array.IndexOf(N43Blocks.Names, n);
        return N43Blocks.FromBits(bits);
    }

    static int N43RmClear(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0, n43 = Array.IndexOf(args, "--n43") >= 0, trace = Array.IndexOf(args, "--trace") >= 0;
        string[] levels = N43Arg(args, "--levels", "foundry,iceline,kiln").Split(',');
        string[] paths = N43Arg(args, "--paths", "kill,pit,stuck,lava").Split(',');
        uint seed = uint.Parse(N43Arg(args, "--seed", "1"), CultureInfo.InvariantCulture);
        double cap = double.Parse(N43Arg(args, "--cap", "120"), CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        int red = 0, fightRooms = 0, opened = 0;
        Console.WriteLine($"n43rooms clear: rooms43 {(off ? "OFF (the control)" : "on")}, {(n43 ? "the n43 level files" : "NIGHT42's level files")}, paths {string.Join("/", paths)}, seed {seed}, cap {cap:0} s per room");
        foreach (string lvName in levels)
        {
            string id = N43RmLevelId(cfg, lvName, n43);
            string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
            foreach (string kp in paths)
            {
                var res = N43RmClearRun(cfg, path, kp, !off, n43, seed, cap, trace);
                foreach (var row in res)
                {
                    if (row.fight) { fightRooms++; if (row.opened) opened++; else red++; }
                }
                string line = string.Join(" ", res.Select(r => $"{r.id}{(r.fight ? "" : "*")}:{(r.opened ? r.seconds.ToString("0.0", CultureInfo.InvariantCulture) : r.note)}"));
                Console.WriteLine($"  {id,-12} {kp,-5} {line}");
                foreach (var row in res.Where(r => r.detail != null)) Console.WriteLine($"      {row.id}: {row.detail}");
            }
        }
        Console.WriteLine($"  (* a link or taught room; times = s from the room's entry to its door opening; a fight room with no time is RED)");
        Console.WriteLine($"n43rooms clear SUMMARY: fight-room doors opened {opened}/{fightRooms}{(red == 0 ? " PASS" : $" RED ({red} held)")}");
        return red == 0 ? 0 : 1;
    }

    sealed class N43RoomRow
    {
        public string id, note = "-", detail;
        public bool fight, opened;
        public double seconds;
    }

    static List<N43RoomRow> N43RmClearRun(GameConfig cfg, string path, string killPath, bool rooms, bool n43Levels, uint seed, double cap, bool trace, Action<SurvivalSim> hook = null)
    {
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        sim.n43Override = N43RmBlocks(cfg, rooms, n43Levels);
        var rows = new List<N43RoomRow>();
        int st = 0, enterStep = 0, curRoom = -1;
        bool cleared = false;
        sim.events.On<RoomEvent>("roomEnter", e => { enterStep = st; curRoom = e.room; cleared = false; });
        sim.events.On<RoomEvent>("roomClear", e =>
        {
            cleared = true;
            N43RoomRow row = rows.Find(r => r.id == e.id);
            if (row != null && !row.opened) { row.opened = true; row.seconds = (st - enterStep) * sim.step; }
        });
        int caps = 0, topCaps = 0, fallbacks = 0;
        sim.events.On<RoomEvent>("lessonCap43", _ => caps++);
        sim.events.On<RoomEvent>("lessonTopUpCap", _ => topCaps++);
        hook?.Invoke(sim);
        sim.StartRun(seed, null, null);
        LevelDef level = sim.campaignLevel;
        if (level == null) { rows.Add(new N43RoomRow { id = "?", note = "no campaign" }); return rows; }
        foreach (RoomDef r in level.rooms)
            rows.Add(new N43RoomRow { id = r.id, fight = r.kind != "boss" && r.WaveCount > 0, note = r.kind == "boss" ? "boss" : "shut" });
        var seen = new Dictionary<Enemy, int>();
        int maxSteps = MovementMath.StepsFor(cap, sim.step), placedRoom = -2, forced = -1;
        int total = MovementMath.StepsFor(cap * level.rooms.Count, sim.step);
        for (; st < total; st++)
        {
            Campaign24 c = sim.campaign;
            if (c != null && c.room >= 0)
            {
                RoomDef r = level.rooms[c.room];
                if (r.kind == "boss") break;
                if (c.phase == Campaign24.Phase.Cleared && c.room + 1 < level.rooms.Count)
                {
                    RoomDef nx = level.rooms[c.room + 1];
                    if (nx.kind == "boss") break;
                    sim.Respawn(nx.spawn, nx.spawnYawDeg); // walk through the open door
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
                    c.lesson.index = c.lesson.defs.Count; // a taught link: the person did the verb (not under test)
                }
                if (c.phase == Campaign24.Phase.Fight && (st - enterStep) >= maxSteps) break; // this room held its door past the cap
            }
            sim.health.hp = sim.health.max; // the harness player never dies (the door is under test, not the fight)
            if (sim.flow == Skygrind.Game.Flow.Down) sim.flow = Skygrind.Game.Flow.Playing;
            var now = new List<Enemy>(sim.enemies.all);
            foreach (Enemy e in now)
            {
                if (!e.alive || e.standIn29 || !(e is Husk h)) continue;
                if (!seen.TryGetValue(e, out int at0) || at0 < 0) { seen[e] = st; continue; }
                int age = st - at0, first = MovementMath.StepsFor(0.5, sim.step), again = MovementMath.StepsFor(3, sim.step);
                if (age < first || (age - first) % again != 0) continue; // 0.5 s after it appears, then every 3 s while it lives (a body that climbed back)
                if ((age - first) / again >= 2) { sim.loadout.Damage(h, h.hp + 100000, false, "revolver", sim.mover.position, shot: 0); fallbacks++; continue; } // a body the path could not remove twice (a drone flies back up): the person shoots it
                N43RmApply(sim, level, h, killPath);
            }
            foreach (Enemy e in now) if (!e.alive && seen.ContainsKey(e)) seen.Remove(e);
            sim.Step();
            if (sim.flow == Skygrind.Game.Flow.Results) break;
        }
        Campaign24 cc = sim.campaign;
        if (cc != null && cc.room >= 0 && cc.room < rows.Count && !rows[cc.room].opened && rows[cc.room].fight)
        {
            RoomDef r = level.rooms[cc.room];
            rows[cc.room].note = "HELD";
            rows[cc.room].detail = $"held: phase {cc.phase}, wave {cc.Wave33}/{r.WaveCount}, own alive {(sim.Rooms43On ? sim.N43RoomLeft.ToString() : "-")}, level alive {sim.enemies.AliveCount}, lesson {(cc.lesson == null ? "none" : cc.lesson.Done ? "done" : cc.lesson.index + "/" + cc.lesson.defs.Count + " " + LessonDef.KindName(cc.lesson.Current.kind))}, top-ups {cc.roomTopUps}";
        }
        if (trace) Console.WriteLine($"      ({Path.GetFileNameWithoutExtension(path)} {killPath}: ended {sim.rooms43Ended}, tracked {sim.rooms43Tracked}, lesson caps {caps}, top-up caps {topCaps}, fallback kills {fallbacks}, falls {cc?.fallReturns43})");
        return rows;
    }

    /// <summary>One kill path on one body.</summary>
    static void N43RmApply(SurvivalSim sim, LevelDef level, Husk h, string killPath)
    {
        Campaign24 c = sim.campaign;
        RoomDef r = c != null && c.room >= 0 ? level.rooms[c.room] : null;
        double floor = r == null ? h.position.y : r.nodes.Count > 0 ? Math.Min(r.spawn.y, r.nodes.Min(n => n.y)) : r.spawn.y;
        switch (killPath)
        {
            case "kill":
                sim.loadout.Damage(h, h.hp + 100000, false, "revolver", sim.mover.position, shot: 0);
                break;
            case "pit":
                h.Teleport(new Vec3(h.position.x, floor - 3, h.position.z));
                break;
            case "stuck":
                if (r != null) h.Teleport(new Vec3(Math.Max(r.x0, r.x1) + 12, floor, (r.z0 + r.z1) / 2));
                break;
            case "lava":
                if (sim.lava30 != null && r != null)
                {
                    foreach (Lava30Zone z in sim.lava30.zones)
                        if (z.room == r.id || (z.x0 >= Math.Min(r.x0, r.x1) - 1 && z.x1 <= Math.Max(r.x0, r.x1) + 1 && z.z0 >= Math.Min(r.z0, r.z1) - 1 && z.z1 <= Math.Max(r.z0, r.z1) + 1))
                        {
                            h.Teleport(new Vec3((z.x0 + z.x1) / 2, z.y, (z.z0 + z.z1) / 2));
                            return;
                        }
                }
                h.Teleport(new Vec3(h.position.x, floor - 3, h.position.z)); // no lava in this room: the pit
                break;
        }
    }
}
