// NIGHT33 builder E (DESIGN33 E): `ai33`, `bosses33` (and `spawns33` in round 2). Declared and called by the lead's Program.cs (N33Enemies).
// ai33 [--off] [--cap s] [--seed n] [--level path]: per lava level (Design/levels/n32-*.json with a `lava` array), per non-boss lava room: a CAMPAIGN run, DOORFREE,
//   the player put at the room's spawn and the level pilot (Walk29Pilot) fighting there for cap s (60), kept alive. Every lava enemy death is told by its cause at the
//   touch that began its burn: PLAYER (the enemy was knocked: knockSteps > 0 or launched) or AI (its own step, the crowd, a dash, a pounce). The row is forced on
//   (SurvivalSim.Force33Ai; --off forces it off: the RED run). PASS (on): 0 AI lava deaths on every level.
// bosses33 [--off] [--seeds N] [--cap s] [--only boss] [--nocamp] [--nocrown]: the NIGHT32 pilots (Boss32Run: THE LANTERN's rider, StagPilot31, WyrmPilot31, LoomPilot32 on
//   the full sim) and THE CROWN's (CrownPilotRun32), with bosses33 forced on (SurvivalSim.Force33Bosses; --off: NIGHT32's pools): kill time p10 / p50 / p90 per boss.
//   PASS: every seed killed and p50 15-22 s (DESIGN33 E: harness p50 about 18 s). Then the campers (camp32's spots, the CROWN's camp table): a camper LOSES when it dies
//   or loses half its health within 30 s having taken under 40 % of the bar (DESIGN33 E: "campers die before dealing 40 %").
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N33Enemies(string cmd, string[] args, ref int code)
    {
        if (cmd == "ai33") { code = Ai33Command(args); return; }
        if (cmd == "bosses33") { code = Bosses33Command(args); return; }
        if (cmd == "spawns33") { code = Spawns33Command(args); return; }
        if (cmd == "spawns33walk") { code = Spawns33Walk(args); return; }
    }

    /// <summary>`spawns33walk [foundry|iceline|kiln|span|spire|all] [--seeds n] [--off]`: builder R's walk bot (arenas33's A33Walk: walk29's pilot on the n33 files, devices on)
    /// with spawns33 forced on (off with --off), plus the hold assertion: no campaign room may sit with 0 alive + 0 told + waves left (not cleared) for over 25 s
    /// (SurvivalSim.Spawns33IdleMax, the longest such spell across every run of the command). PASS: every walk GREEN and the longest spell <= 25 s.</summary>
    static int Spawns33Walk(string[] args)
    {
        string lvl = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = int.Parse(Opt33(args, "--seeds", "10"), CultureInfo.InvariantCulture);
        bool off = Array.IndexOf(args, "--off") >= 0;
        SurvivalSim.Force33Spawns = !off;
        int code = 0;
        var lines = new List<string>();
        string first = Opt33(args, "--first", null);
        if (first != null)
        {
            // diagnosis: one seed of the walk, the deaths and hurts by source per room
            var hurtBy = new Dictionary<string, int>();
            string oldP = walkPrefix;
            Action<SurvivalSim> oldH = walkAfterStart;
            walkPrefix = "n33";
            walkAfterStart = sim =>
            {
                sim.Devices33Force(true);
                sim.events.On<HurtEvent>("hurt", e =>
                {
                    if (e.owner != "player") return;
                    string room = sim.campaign != null && sim.campaign.room >= 0 ? sim.campaignLevel.rooms[sim.campaign.room].id : "-";
                    string k = $"{room} {(e.hp <= 0 ? "DEATH" : "hurt")} by {e.source ?? "?"}";
                    hurtBy.TryGetValue(k, out int n);
                    hurtBy[k] = n + 1;
                });
                sim.events.On<SpawnTell33Event>("enemySpawnTell33", e =>
                {
                    string k = $"tell {(sim.campaign != null && sim.campaign.room >= 0 ? sim.campaignLevel.rooms[sim.campaign.room].id : "-")} {e.kind} at ({e.x:0},{e.y:0.0},{e.z:0}) player ({sim.mover.position.x:0},{sim.mover.position.z:0})";
                    hurtBy.TryGetValue(k, out int n);
                    hurtBy[k] = n + 1;
                });
            };
            try { N29Walk("walk29", new[] { "walk29", lvl, "--seeds", "1", "--first", first }, ref code); }
            finally { walkPrefix = oldP; walkAfterStart = oldH; SurvivalSim.Force33Spawns = null; }
            var rows = new List<KeyValuePair<string, int>>(hurtBy);
            rows.Sort((x, y) => y.Value.CompareTo(x.Value));
            for (int i = 0; i < Math.Min(30, rows.Count); i++) Console.WriteLine($"  {rows[i].Value,5} {rows[i].Key}");
            return code;
        }
        try
        {
            foreach (string g in new[] { "foundry", "iceline", "kiln", "span", "spire" })
            {
                if (lvl != "all" && lvl != g) continue;
                SurvivalSim.Spawns33IdleMax = 0;
                SurvivalSim.Spawns33IdleWhere = "";
                int c = A33Walk(g, seeds);
                bool idleOk = SurvivalSim.Spawns33IdleMax <= 25.0;
                if (!idleOk) c |= 1;
                code |= c;
                lines.Add($"spawns33walk {g} (spawns33 {(off ? "OFF" : "ON")}): walk {(c == 0 ? "GREEN" : "RED")}, the longest idle hold {SurvivalSim.Spawns33IdleMax:0.0} s{(SurvivalSim.Spawns33IdleWhere.Length > 0 ? " (" + SurvivalSim.Spawns33IdleWhere + ")" : "")} {(idleOk ? "<= 25 s" : "OVER 25 s")}");
            }
        }
        finally { SurvivalSim.Force33Spawns = null; }
        foreach (string l in lines) Console.WriteLine(l);
        Console.WriteLine($"spawns33walk {(code == 0 ? "PASS" : "RED")}");
        return code;
    }

    // ---- spawns33 -----------------------------------------------------------------------------------------------------------------------------
    // spawns33 [--off] [--cap s] [--seed n] [--level path]: per non-boss lava room of the n32 lava levels: a CAMPAIGN run, DOORFREE, the player put at the room's spawn and
    // the level pilot walking to the room's EXIT (across the lava), kept alive, cap s (40). Per room: the first wave's delay (room entry -> the first spawn, or the first tell
    // with the row on), whether the pilot was past the lava line then, the tells, the nearest a spawn came to the player. Row forced on (off with --off: the RED run).
    // PASS (on): every lava room's first wave waits (delay over 1 s) and comes past the line or by the wait timer, and every spawn is at least 6 m from the player.

    static int Spawns33Command(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        double cap = double.Parse(Opt33(args, "--cap", "40"), CultureInfo.InvariantCulture);
        uint seed = uint.Parse(Opt33(args, "--seed", "1"), CultureInfo.InvariantCulture);
        string only = Opt33(args, "--level", null);
        var levels = new List<string>();
        if (only != null) levels.Add(only);
        else foreach (string f in Directory.GetFiles(Path.Combine("Design", "levels"), Array.IndexOf(args, "--n33") >= 0 ? "n33-*.json" : "n32-*.json")) if (!f.EndsWith(".plan.json")) levels.Add(f.Replace('\\', '/'));
        levels.Sort(StringComparer.Ordinal);
        SurvivalSim.Force33Spawns = !off;
        int rooms = 0, bad = 0;
        double minAll = double.PositiveInfinity;
        Console.WriteLine($"spawns33 (row {(off ? "OFF (forced)" : "ON (forced)")}; {cap:0} s per lava room, seed {seed}; the level pilot walks from the room's spawn to its exit, DOORFREE, kept alive)");
        Console.WriteLine($"  {"level",-20} {"room",-6} {"1st wave s",11} {"how",-10} {"tells",6} {"spawns",7} {"min dist m",11} verdict");
        try
        {
            foreach (string path in levels)
            {
                GameConfig cfg = Config();
                CampaignMapFrom(path, cfg, out var json);
                if (!json.ContainsKey("lava")) continue;
                LevelDef level = LevelDef.From(json);
                var ids = new List<string>();
                foreach (object o in PortData.Arr(json["lava"]))
                {
                    var z = PortData.Obj(o);
                    string rm = z.TryGetValue("room", out object r) ? r as string : null;
                    RoomDef rd = rm != null ? level.rooms.Find(x => x.id == rm) : null;
                    if (rd != null && rd.kind != "boss" && rd.WaveCount > 0 && !ids.Contains(rm)) ids.Add(rm);
                }
                foreach (string id in ids)
                {
                    rooms++;
                    GameConfig c2 = Config();
                    SurvivalMap map = CampaignMapFrom(path, c2, out var j2);
                    LevelDef lv = LevelDef.From(j2);
                    RoomDef room = lv.rooms.Find(r => r.id == id);
                    AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
                    AutopilotInput.GraphSource = BotNav.Survival;
                    SurvivalSim sim = SurvivalSim.Headless(c2, map);
                    sim.noRecords = sim.noLab = true;
                    N29Blocks b29 = N29Blocks.From(c2);
                    b29.doorFree = true;
                    sim.n29Override = b29;
                    sim.setup.mode = Campaign24.Mode;
                    var ap = new AutopilotInput(sim, "normal", seed);
                    ap.GoalProvider = () => room.exit;
                    sim.UseInput(new Walk29Pilot(sim, ap));
                    double enterT = -1, firstT = -1, minD = double.PositiveInfinity;
                    string firstHow = "none";
                    int tells = 0, spawns = 0;
                    bool armed = false;
                    sim.events.On<RoomEvent>("roomEnter", e => { if (armed && e.id == id && enterT < 0) enterT = sim.SimTime; });
                    void Seen(double x, double z)
                    {
                        if (!armed || enterT < 0) return;
                        if (firstT < 0) { firstT = sim.SimTime; firstHow = sim.spawns33FirstHow; }
                        minD = Math.Min(minD, JsMath.Hypot(x - sim.mover.position.x, z - sim.mover.position.z));
                    }
                    sim.events.On<SpawnTell33Event>("enemySpawnTell33", e => { tells++; Seen(e.x, e.z); });
                    sim.events.On<SpawnEvent>("enemySpawn", e => { spawns++; if (off) Seen(e.position.x, e.position.z); });
                    sim.StartRun(seed, null, null);
                    for (int s = 0; s < 240; s++) { sim.health.hp = sim.health.max; AutopilotInput.TakeDraft(sim); sim.Step(); }
                    sim.campaign?.SetDoorFree29(true);
                    tells = spawns = 0;
                    armed = true;
                    sim.Respawn(new Vec3(room.spawn.x, room.spawn.y + 0.05, room.spawn.z), room.spawnYawDeg);
                    int max = MovementMath.StepsFor(cap, sim.step);
                    for (int st = 0; st < max; st++)
                    {
                        sim.health.hp = sim.health.max;
                        if (sim.flow == Skygrind.Game.Flow.Down) { sim.downSteps = 0; sim.flow = Skygrind.Game.Flow.Playing; }
                        AutopilotInput.TakeDraft(sim);
                        sim.Step();
                        sim.hitstopLeft = 0;
                    }
                    ap.Detach();
                    double delay = firstT >= 0 && enterT >= 0 ? firstT - enterT : -1;
                    string how = off ? "-" : firstHow;
                    // a room with no lava before its exit has no line (its first wave comes at once, as before); a room DOORFREE never entered is not judged
                    bool standIns = tells == 0 && spawns > 0; // a lesson room whose stand-ins hold its waves (f5: the same with the row off)
                    bool ok = off || enterT < 0 || standIns || (how == "no line" || delay > 1.0) && minD >= 6.0;
                    if (standIns && !off) how = "stand-ins";
                    if (!ok) bad++;
                    if (minD < minAll) minAll = minD;
                    Console.WriteLine($"  {Path.GetFileName(path),-20} {id,-6} {(delay >= 0 ? delay.ToString("0.0", CultureInfo.InvariantCulture) : (enterT < 0 ? "not entered" : "none")),11} {how,-10} {tells,6} {spawns,7} {(minD < 1e9 ? minD.ToString("0.0", CultureInfo.InvariantCulture) : "-"),11} {(off ? "" : ok ? "PASS" : "RED")}");
                }
            }
        }
        finally { SurvivalSim.Force33Spawns = null; }
        Console.WriteLine($"spawns33 {(off ? "(row OFF) " : "")}{(bad == 0 ? "PASS" : "RED")}: {rooms} lava rooms; the nearest spawn {(minAll < 1e9 ? minAll.ToString("0.0", CultureInfo.InvariantCulture) : "-")} m from the player{(off ? "" : $"; {bad} rooms whose first wave came at once or nearer than 6 m")}");
        return off ? 0 : bad == 0 ? 0 : 1;
    }

    static string Opt33(string[] args, string key, string def)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
    }

    // ---- ai33 ---------------------------------------------------------------------------------------------------------------------------------

    sealed class Ai33Row
    {
        public int aiDeaths, playerDeaths, burns, spared, crowd, masked, spawned, moved;
        public double pinned;
        public string level, room;
    }

    static bool ai33Dbg;

    static int Ai33Command(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        ai33Dbg = Array.IndexOf(args, "--dbg") >= 0;
        SurvivalSim.Probe33NoGoalLine = Array.IndexOf(args, "--nogoalline") >= 0;
        double cap = double.Parse(Opt33(args, "--cap", "60"), CultureInfo.InvariantCulture);
        uint seed = uint.Parse(Opt33(args, "--seed", "1"), CultureInfo.InvariantCulture);
        string only = Opt33(args, "--level", null);
        var levels = new List<string>();
        if (only != null) levels.Add(only);
        else foreach (string f in Directory.GetFiles(Path.Combine("Design", "levels"), Array.IndexOf(args, "--n33") >= 0 ? "n33-*.json" : "n32-*.json")) if (!f.EndsWith(".plan.json")) levels.Add(f.Replace('\\', '/'));
        levels.Sort(StringComparer.Ordinal);
        SurvivalSim.Force33Ai = !off;
        int ai = 0, pl = 0, rooms = 0, v6Spawned = -1;
        double pinSum = 0;
        Console.WriteLine($"ai33 (row {(off ? "OFF (forced)" : "ON (forced)")}; {cap:0} s per lava room, seed {seed}; the level pilot fights at the room's spawn, DOORFREE, kept alive)");
        Console.WriteLine($"  {"level",-28} {"room",-6} {"spawned",8} {"burns",6} {"AI dead",8} {"PLAYER dead",12} {"spared",7} {"backs",6} {"moved",6} {"masked",7} {"pinned",7}");
        try
        {
            foreach (string path in levels)
            {
                GameConfig cfg = Config();
                CampaignMapFrom(path, cfg, out var json);
                if (!json.ContainsKey("lava")) continue;
                LevelDef level = LevelDef.From(json);
                var ids = new List<string>();
                foreach (object o in PortData.Arr(json["lava"]))
                {
                    var z = PortData.Obj(o);
                    string rm = z.TryGetValue("room", out object r) ? r as string : null;
                    RoomDef rd = rm != null ? level.rooms.Find(x => x.id == rm) : null;
                    if (rd != null && rd.kind != "boss" && !ids.Contains(rm)) ids.Add(rm);
                }
                foreach (string id in ids)
                {
                    Ai33Row row = Ai33Room(path, id, seed, cap);
                    rooms++;
                    ai += row.aiDeaths;
                    pl += row.playerDeaths;
                    Console.WriteLine($"  {Path.GetFileName(path),-28} {id,-6} {row.spawned,8} {row.burns,6} {row.aiDeaths,8} {row.playerDeaths,12} {row.spared,7} {row.crowd,6} {row.moved,6} {row.masked,7} {row.pinned,7:0.00}");
                    pinSum += row.pinned;
                    if (id == "v6") v6Spawned = row.spawned;
                }
            }
        }
        finally { SurvivalSim.Force33Ai = null; SurvivalSim.Probe33NoGoalLine = false; }
        bool pass = ai == 0;
        Console.WriteLine($"ai33 {(off ? "(row OFF) " : "")}{(pass ? "PASS" : "RED")}: {rooms} lava rooms, enemy lava deaths by cause: AI {ai}, PLAYER {pl} (PASS: 0 AI deaths); walkers pinned at a lava edge: mean {(rooms > 0 ? pinSum / rooms : 0):0.00} per room (walkers the edge guard pushes back each step, mean over the room's steps); v6 spawned {v6Spawned}{(Array.IndexOf(args, "--nogoalline") >= 0 ? " [--nogoalline]" : "")}");
        return off ? 0 : pass ? 0 : 1;
    }

    /// <summary>Feet on the bank within 1 m of a lava rect (outside it), less than 2.5 m over its surface.</summary>
    static bool Ai33NearLava(SurvivalSim sim, Vec3 p)
    {
        if (sim.lava30 == null) return false;
        foreach (Lava30Zone z in sim.lava30.zones)
        {
            double dx = Math.Max(Math.Max(z.x0 - p.x, 0), p.x - z.x1), dz = Math.Max(Math.Max(z.z0 - p.z, 0), p.z - z.z1);
            double d = Math.Sqrt(dx * dx + dz * dz);
            if (d > 0 && d <= 1.0 && p.y < z.y + 2.5) return true;
        }
        return false;
    }

    static Ai33Row Ai33Room(string path, string roomId, uint seed, double cap)
    {
        var row = new Ai33Row { level = path, room = roomId };
        GameConfig cfg = Config();
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        LevelDef level = LevelDef.From(json);
        RoomDef room = level.rooms.Find(r => r.id == roomId);
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        N29Blocks b29 = N29Blocks.From(cfg);
        b29.doorFree = true;
        sim.n29Override = b29;
        sim.setup.mode = Campaign24.Mode;
        var ap = new AutopilotInput(sim, "normal", seed);
        ap.GoalProvider = () => room.spawn;
        sim.UseInput(new Walk29Pilot(sim, ap));
        var cause = new Dictionary<int, bool>(); // id -> knocked by the player at the touch
        var spawnAt = new Dictionary<int, double>();
        var lastKnock = new Dictionary<int, double>();
        int dbgLeft = 6;
        sim.events.On<Lava30Event>("lavaEnemyBurn", e =>
        {
            row.burns++;
            Enemy en = sim.enemies.all.Find(x => x.id == e.id);
            cause[e.id] = en != null && (en.knockSteps > 0 || en is Husk h && h.launched || lastKnock.TryGetValue(e.id, out double tk) && sim.SimTime - tk <= 1.0);
            if (ai33Dbg && en != null && dbgLeft-- > 0)
            {
                spawnAt.TryGetValue(e.id, out double t0);
                Console.WriteLine($"    burn {roomId} id {e.id} {e.kind} at ({en.position.x:0.0},{en.position.y:0.00},{en.position.z:0.0}) prev ({en.previous.x:0.0},{en.previous.y:0.00},{en.previous.z:0.0}) knock {en.knockSteps} launched {(en is Husk hh && hh.launched)} {(en is Charger cc ? cc.state : "")} {sim.SimTime - t0:0.00} s after spawn");
            }
        });
        sim.events.On<SpawnEvent>("enemySpawn", en => { spawnAt[en.id] = sim.SimTime; if (ai33Dbg) Console.WriteLine($"    spawn {roomId} {en.kind} id {en.id} at ({en.position.x:0.0},{en.position.y:0.00},{en.position.z:0.0}) t {sim.SimTime:0.0}"); });
        sim.events.On<Lava30Event>("lavaEnemyDeath", e =>
        {
            if (cause.TryGetValue(e.id, out bool p) && p) row.playerDeaths++;
            else row.aiDeaths++;
        });
        sim.events.On("enemySpawn", _ => row.spawned++);
        sim.StartRun(seed, null, null);
        for (int s = 0; s < 240; s++) { sim.health.hp = sim.health.max; AutopilotInput.TakeDraft(sim); sim.Step(); }
        sim.campaign?.SetDoorFree29(true);
        sim.Respawn(new Vec3(room.spawn.x, room.spawn.y + 0.05, room.spawn.z), room.spawnYawDeg);
        int max = MovementMath.StepsFor(cap, sim.step);
        long pinSteps = 0;
        for (int st = 0; st < max; st++)
        {
            sim.health.hp = sim.health.max;
            if (sim.flow == Skygrind.Game.Flow.Down) { sim.downSteps = 0; sim.flow = Skygrind.Game.Flow.Playing; }
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            sim.hitstopLeft = 0;
            foreach (Enemy en in sim.enemies.all)
            {
                if (en.alive && (en.knockSteps > 0 || en is Husk h && h.launched)) lastKnock[en.id] = sim.SimTime; // a player's knock within 1 s of the touch counts as PLAYER
                if (en.alive && en is Husk && !(en is Drone) && JsMath.Hypot(en.velocity.x, en.velocity.z) < 0.5 && Ai33NearLava(sim, en.position)) pinSteps++;
            }
            if (ai33Dbg && (st == max / 3 || st == 2 * max / 3))
                foreach (Enemy en in sim.enemies.all)
                    if (en.alive && en is Husk hu && !(en is Drone))
                        Console.WriteLine($"    t {sim.SimTime:0.0} {roomId} {en.kind} id {en.id} at ({en.position.x:0.0},{en.position.y:0.00},{en.position.z:0.0}) v ({en.velocity.x:0.0},{en.velocity.z:0.0}) nearLava {Ai33NearLava(sim, en.position)} player ({sim.mover.position.x:0.0},{sim.mover.position.y:0.00},{sim.mover.position.z:0.0}) {(en is Charger ch ? ch.state : "")} mode {sim.brain.ModeOf(en.id)} links cut {sim.ai33LinksCut}");
        }
        row.pinned = max > 0 ? (double)sim.ai33CrowdBacks / max : 0; // walkers the edge guard pushed back, per step: the mean count pressing at a lava edge (pinSteps: still near lava, the --dbg view)
        row.spared = sim.lava30 != null ? sim.lava30.spared33 : 0;
        row.crowd = sim.ai33CrowdBacks;
        row.moved = sim.ai33SpawnMoves;
        row.masked = sim.ai33Masked;
        ap.Detach();
        return row;
    }

    // ---- bosses33 -----------------------------------------------------------------------------------------------------------------------------

    static int Bosses33Command(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0, noCamp = Array.IndexOf(args, "--nocamp") >= 0, noCrown = Array.IndexOf(args, "--nocrown") >= 0;
        int seeds = int.Parse(Opt33(args, "--seeds", "10"), CultureInfo.InvariantCulture);
        double cap = double.Parse(Opt33(args, "--cap", "120"), CultureInfo.InvariantCulture);
        string only = Opt33(args, "--only", null);
        int bad = 0;
        SurvivalSim.Force33Bosses = !off;
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "--set")
            {
                string[] kv = args[i + 1].Split('=');
                SurvivalSim.Probe33Bosses ??= new Dictionary<string, double>();
                SurvivalSim.Probe33Bosses[kv[0]] = double.Parse(kv[1], CultureInfo.InvariantCulture);
                Console.WriteLine($"  --set {kv[0]} = {kv[1]} ({(kv[0] == "crown" ? "pool" : "hpMult")})");
            }
        sets30.Clear();
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        Console.WriteLine($"bosses33 (row {(off ? "OFF (forced): NIGHT32's pools" : "ON (forced)")}; {seeds} seeds, cap {cap:0} s; band: every seed killed, p50 15-22 s)");
        try
        {
            // --sweep boss=v1,v2,...: a tuning probe, that boss only, one line per value (no campers)
            string sweep = Opt33(args, "--sweep", null);
            if (sweep != null)
            {
                string[] kv = sweep.Split('=');
                SurvivalSim.Probe33Bosses ??= new Dictionary<string, double>();
                foreach (string v in kv[1].Split(','))
                {
                    SurvivalSim.Probe33Bosses[kv[0]] = double.Parse(v, CultureInfo.InvariantCulture);
                    Sweep33(kv[0], seeds, cap, off, $"{kv[0]} {v}");
                }
                return 0;
            }
            foreach (string boss in new[] { "lantern", "stag", "wyrm", "loom", "crown" })
            {
                if (only != null && only != boss || boss == "crown" && noCrown) continue;
                if (!Sweep33(boss, seeds, cap, off, boss) && !off) bad++;
            }
            if (!noCamp)
                foreach (string boss in new[] { "lantern", "stag", "wyrm", "loom" })
                    if (only == null || only == boss) bad += Camp33(boss, 1, cap);
            if (!noCamp && !noCrown && (only == null || only == "crown")) bad += Camp33Crown(1);
        }
        finally
        {
            SurvivalSim.Force33Bosses = null;
            SurvivalSim.Probe33Bosses = null;
            SurvivalSim.Wyrm24Dir = null;
            SurvivalSim.Boss32Probe = null;
        }
        Console.WriteLine($"bosses33 {(off ? "(row OFF) " : "")}{(bad == 0 ? "PASS" : "RED")}{(off ? "" : $": {bad} rows out of band or camping")}");
        return off ? 0 : bad == 0 ? 0 : 1;
    }

    /// <summary>One boss's pilot over seeds 1..N (the NIGHT32 pilots; THE CROWN's CrownPilotRun32): one line, p10 / p50 / p90. True when in DESIGN33 E's band.</summary>
    static bool Sweep33(string boss, int seeds, double cap, bool off, string label)
    {
        var times = new List<double>();
        int killed = 0, deaths = 0;
        GameConfig crownCfg = boss == "crown" ? CrownCfg32() : null;
        for (int s = 1; s <= seeds; s++)
        {
            bool k;
            double sec;
            int d;
            if (boss == "crown") { CrownRow32 r = CrownPilotRun32(crownCfg, (uint)s, cap, false); k = r.killed; sec = r.seconds; d = r.deaths; }
            else { Run32 r = Boss32Run(boss, (uint)s, cap); k = r.killed; sec = r.seconds; d = r.deaths; }
            times.Add(k ? sec : cap);
            if (k) killed++;
            deaths += d;
        }
        double p10 = Pct32(times, 0.1), p50 = Pct32(times, 0.5), p90 = Pct32(times, 0.9);
        bool pass = killed == seeds && p50 >= 15 && p50 <= 22;
        Console.WriteLine($"  {label,-14} killed {killed}/{seeds}  p10 {p10,5:0.0}  p50 {p50,5:0.0}  p90 {p90,5:0.0} s  deaths/run {(double)deaths / seeds:0.0}  {(off ? "" : pass ? "PASS" : "RED")}");
        return pass;
    }

    /// <summary>THE CROWN's campers (crown32's spots, CrownCampRun32) with DESIGN33 E's rule: dies or loses half its health having taken under 40 % of the bar.</summary>
    static int Camp33Crown(uint seed)
    {
        GameConfig cfg = CrownCfg32();
        var arena = new Crown32Arena(Crown32ArenaParams.From(cfg));
        var spots = CrownSpots32(arena, new Vec3(0, 0, 0));
        int bad = 0;
        var lines = new List<string>();
        for (int i = 0; i < spots.Count; i++)
        {
            CrownCampRow32 r = CrownCampRun32(cfg, seed, i, false);
            if (r == null) { lines.Add($"    spot {i}: rig failed"); bad++; continue; }
            double took = r.rate * r.alive;
            bool hurt = r.died || r.lost >= 0.5, loses = hurt && !r.killedBoss && took < 0.4;
            if (!loses) bad++;
            lines.Add($"    {r.name,-52} {(r.died ? "DIED " : "") + r.alive.ToString("0.0") + " s",10} hp lost {r.lost * 100,4:0} %  took {took * 100,4:0} % of the bar  mortar hits {r.mortarHits}  {(loses ? "LOSES" : hurt ? "CAMPS (took 40 % or more before falling)" : "CAMPS (not hurt enough)")}");
        }
        Console.WriteLine($"  camp33 crown {(bad == 0 ? "PASS" : "RED")}: {spots.Count - bad} of {spots.Count} campers lose (dies or loses half its health having taken under 40 % of the bar)");
        foreach (string l in lines) Console.WriteLine(l);
        return bad == 0 ? 0 : 1;
    }

    /// <summary>camp32's campers with DESIGN33 E's rule: a camper LOSES when it dies or loses half its health within 30 s having taken under 40 % of the bar.</summary>
    static int Camp33(string boss, uint seed, double cap)
    {
        string rigName = boss == "lantern" ? "lantern31" : boss == "wyrm" ? "wyrm31" : boss == "loom" ? "loom31" : "stag";
        CampRig31 probe = CampRig31For(rigName, seed);
        if (probe == null) { Console.WriteLine($"  camp33 {boss}: NOT BUILT"); return 1; }
        int bad = 0;
        var lines = new List<string>();
        for (int i = 0; i < probe.spots.Count; i++)
        {
            CampRow31 row = CampRun31(rigName, seed, i, false);
            if (row == null) { lines.Add($"    spot {i}: rig failed"); bad++; continue; }
            double took = row.rate * row.alive;
            bool hurt = row.died || row.lost >= 0.5, loses = hurt && took < 0.4;
            if (!loses) bad++;
            lines.Add($"    {row.name,-52} {(row.died ? "DIED " : "") + row.alive.ToString("0.0") + " s",10} hp lost {row.lost * 100,4:0} %  took {took * 100,4:0} % of the bar  {(loses ? "LOSES" : hurt ? "CAMPS (took 40 % or more before falling)" : "CAMPS (not hurt enough)")}");
        }
        Console.WriteLine($"  camp33 {boss} {(bad == 0 ? "PASS" : "RED")}: {probe.spots.Count - bad} of {probe.spots.Count} campers lose (dies or loses half its health in 30 s having taken under 40 % of the bar)");
        foreach (string l in lines) Console.WriteLine(l);
        return bad == 0 ? 0 : 1;
    }
}
