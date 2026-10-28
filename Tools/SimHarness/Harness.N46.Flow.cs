// NIGHT46 builder F1 (NIGHT46.md card F1, "the flow meter"; feel-flow-web.md F32, arena-flow-web.md F16, FLOW-AUDIT.md): `n46flow`, the night's
// scoreboard. Hooked without a Program.cs line: a module initializer runs the command and exits when argv[1] is "n46flow" (NIGHT44 R2's pattern);
// it applies the off switches itself (it runs before Program.Main): --off46 (NightRows.ForceOffFrom), --off45 / --off44 (+ N45Levels.ForceOff).
//   n46flow [--level <id>|all] [--seeds N] [--first k] [--rooms a,b] [--rows r1,r2] [--off46] [--cap s] [--bosses] [--no-geo] [--out file]
//           [--per-seed] [--trace]
//     ONE pilot for every room (F1Input below): the walk pilot (Walk38Pilot round the ported autopilot brain, BotNav.Survival, persona "normal"),
//     as the builders' "walk 10 / 10" runs it (n43level walk, n45yard walk), plus one rule read from level data, never a room id: the lava rule
//     (out of the lava by the nearest rim; never step into unbridged lava, stand at its edge and fight; a grid path round it once the room is
//     cleared). A pilot fault is the pilot's: a fight room it does not clear in --cap s (240) ends that seed ("cap"); a link it does not cross
//     in max(30 s, 3 x par), or a cleared room it does not leave in 15 s, puts it at the next room's spawn (counted; links are not measured).
//     Bosses are not measured unless --bosses (the walk pilot killed neither THE LOOM nor THE CROWN in 240 s, and THE BRAWL cleared at once:
//     each boss has its own pilot and harness, e.g. B1's wyrm46 numbers).
//     The level is the map file Design/levels/<id>.json, played as the campaign with the config's rows (the ON game); older systems read it by
//     NightRows.BaseId / N45Levels.BaseId. --rows forces NIGHT46+ sim rows on (sim.rowsOverride = Defaults().Set(r, true)); --off46 is NIGHT45's game.
//     The walk starts at the level's first fight room (DOORFREE for the placement only, as n45yard walk: the opening links are not walked) and ends
//     when the last fight room (the boss) clears. --rooms a,b: measure only these rooms; the walk starts at the first of them and ends when the last
//     of them clears. --level all: the five ON levels 1-5 (n45-foundry, n45-iceline, n45-kiln, n40-span, n41-spire).
//     Per fight room (kind room / boss) and per level, while the room is in its fight (roomEnter to roomClear; the retry pause left out):
//       stall %   in-fight steps with horizontal speed < 25 % of ground.walkSpeed (8 m/s: 2 m/s)
//       maxStall  the longest run of stalled steps (s; the median over seeds)
//       air %     in-fight steps not grounded
//       kgap      the steps between two kills in the room (s): median / max (pooled over seeds)
//       near      the nearest live enemy's horizontal distance (m) on the steps one lives: median / p10 / p90 (pooled; boss rooms: adds only)
//       dwell     s within 4 m of the room's entry door (its rect; the first room: its spawn) while an enemy lives (median over seeds)
//       dead      s in the fight with no enemy alive and the door still shut (wave gaps, holds, the commit gate; median over seeds; boss rooms "-")
//       deaths    room retries (total over seeds); falls: stageRespawn + fallReturn43 (total)
//       clear     roomEnter -> roomClear (s; median over the seeds that cleared it); k/n cleared
//     Geometry (FLOW-AUDIT's rasteriser, ported to Tools/night/n46-flow-geo.cjs; --no-geo skips it): floor m2, tiers, central loop (the largest
//     walk-graph hole >= 50 m2: y / n), pockets, the longest sightline (m), wave 1's nodes in view (bearing <= 53 deg and line of sight) from the spawn.
//     The sim is deterministic: the same build and seeds give the same numbers (a re-run is a determinism check, not a noise band). The noise band in
//     Research/2026-10-17-wave46/F1.md is seeds 1-10 against seeds 11-20 on one build.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void F1N46FlowInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n46flow") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            if (Array.IndexOf(args, "--off45") >= 0 || Array.IndexOf(args, "--off44") >= 0) N45Levels.ForceOff = true;
            code = F1Flow(args);
        }
        catch (Exception e) { Console.WriteLine("n46flow: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static readonly string[] F1Levels = { "n45-foundry", "n45-iceline", "n45-kiln", "n40-span", "n41-spire" };
    static string F1File; // NIGHT66 T1c: --file <path> (one level)
    static readonly CultureInfo F1Inv = CultureInfo.InvariantCulture;
    static bool F1Bosses, F1Trace;
    static readonly List<string> F1Sets = new List<string>();

    static string F1Arg(string[] args, string key, string def)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
    }

    static bool F1Flag(string[] args, string key) => Array.IndexOf(args, key) >= 0;

    /// <summary>One room's numbers for one seed.</summary>
    sealed class F1Room
    {
        public int reveal, phase;
        public int fight, stall, air, curStall, maxStall, dwell, dead, deaths, falls, kills, lastKill = -1, near;
        public bool cleared, entered;
        public int enterStep, clearSteps;
        public readonly List<double> gaps = new List<double>();
        public readonly List<float> nearS = new List<float>();
    }

    /// <summary>One room pooled over the seeds.</summary>
    sealed class F1Pool
    {
        public string id, kind;
        public long fight, stall, air;
        public readonly List<double> maxStall = new List<double>(), dwell = new List<double>(), dead = new List<double>(), clear = new List<double>(), gaps = new List<double>();
        public readonly List<float> near = new List<float>();
        public int deaths, falls, cleared, entered, caps;
    }

    static int F1Flow(string[] args)
    {
        if (F1Flag(args, "--lava-cases")) return F1LavaCases(args); // NIGHT68 I3: the lava rule's planted cases (Harness.N68.Lava.cs)
        F1LavaDetail = F1Flag(args, "--lava-detail"); // NIGHT68 I3: one more line per level (deck holds); off: the output as before
        string lv = F1Arg(args, "--level", "all");
        int seeds = int.Parse(F1Arg(args, "--seeds", "10"), F1Inv), first = int.Parse(F1Arg(args, "--first", "1"), F1Inv);
        double cap = double.Parse(F1Arg(args, "--cap", "240"), F1Inv);
        string roomsArg = F1Arg(args, "--rooms", null), rowsArg = F1Arg(args, "--rows", null), outFile = F1Arg(args, "--out", null);
        bool geo = !F1Flag(args, "--no-geo"), perSeed = F1Flag(args, "--per-seed");
        F1Bosses = F1Flag(args, "--bosses");
        F1Trace = F1Flag(args, "--trace");
        V65Verbs = F1Flag(args, "--verbs65"); // NIGHT65 T: the verb-aware pilot in rooms with verb65 (Harness.N65.VerbPilot.cs); off: nothing changes
        V65ParsePlants(F1Arg(args, "--plant-verb65", null)); // NIGHT65 T: rows planted in memory (verb65 + bot43 false); none: the file as it is
        V68ParseOnly(F1Arg(args, "--plant-only", null)); // NIGHT68 I2: a room's waves planted as one kind (in memory); none: the file as it is
        V68ParseLimit(F1Arg(args, "--plant-limit", null)); // NIGHT68 g7: a room's `limit` row field planted (in memory); none: the file as it is
        F1Sets.Clear();
        for (int k = 0; k + 1 < args.Length; k++) if (args[k] == "--set") F1Sets.Add(args[k + 1]);
        string[] levels = lv == "all" ? F1Levels : lv.Split(',');
        F1File = F1Arg(args, "--file", null); // NIGHT66 T1c: the one level's file (F1Level)
        V67PutInOff = Array.IndexOf(args, "--no-put-in-awards") >= 0; // NIGHT67 T1c: the old put-in (no earlier room's award; the planted RED)
        if (F1File != null && levels.Length != 1) throw new ArgumentException("n46flow --file takes one --level");
        var sb = new StringBuilder();
        void Line(string s) { Console.WriteLine(s); sb.AppendLine(s); }
        string sw = $"n46flow: seeds {first}..{first + seeds - 1}, cap {cap:0} s per room, rows forced on: {(rowsArg ?? "none")}, off switch: {(NightRows.ForceOffFrom == NightRows.NoOff ? "none" : "--off" + NightRows.ForceOffFrom)}{(N45Levels.ForceOff ? " + N45Levels off" : "")}; pilot: Walk38Pilot + the ported autopilot (normal)";
        Line(sw);
        int code = 0;
        foreach (string id in levels)
        {
            var t = Stopwatch.StartNew();
            int c = F1Level(id, seeds, first, cap, roomsArg, rowsArg, geo, perSeed, Line);
            Line($"n46flow {id}: wall time {t.Elapsed.TotalSeconds:0} s");
            if (c != 0) code = c;
        }
        if (outFile != null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outFile)));
            File.WriteAllText(outFile, sb.ToString());
        }
        return code;
    }

    /// <summary>The level file's door rects: the entry door of each room (a door's `between[1]`), [x0, z0, x1, z1].</summary>
    static Dictionary<string, double[]> F1EntryDoors(Dictionary<string, object> json)
    {
        var res = new Dictionary<string, double[]>();
        var camp = PortData.Obj(json["campaign"]);
        foreach (object o in PortData.Arr(camp["doors"]))
        {
            var d = PortData.Obj(o);
            var bt = PortData.Arr(d["between"]);
            var r = PortData.Arr(d["rect"]);
            res[(string)bt[1]] = new[] { PortData.Num(r[0]), PortData.Num(r[1]), PortData.Num(r[2]), PortData.Num(r[3]) };
        }
        return res;
    }

    static int F1Level(string id, int seeds, int first, double cap, string roomsArg, string rowsArg, bool geo, bool perSeed, Action<string> Line)
    {
        GameConfig cfg = Config();
        foreach (string kv in F1Sets)
        {
            // --set design.n46.<row>.numbers.<key>=<number>: a number changed in this process's config only (a probe: never written to disk)
            int eq = kv.IndexOf('=');
            string[] parts = kv.Substring(0, eq).Split('.');
            var o = cfg.root;
            for (int k = 0; k < parts.Length - 1; k++) o = PortData.Obj(o[parts[k]]);
            o[parts[parts.Length - 1]] = double.Parse(kv.Substring(eq + 1), F1Inv);
            Line($"n46flow {id}: --set {kv}");
        }
        // NIGHT66 T1c: --file <path>, the level read from that file instead of Design/levels/<id>.json (one --level only: a round's
        // Logs/n66/<tag>/n65-foundry.json, so a round never writes the tracked level files)
        string fileArg = F1File;
        string path = Path.GetFullPath(fileArg ?? Path.Combine("Design", "levels", id + ".json"));
        if (!File.Exists(path)) { Line($"n46flow {id}: no level file {path}"); return 2; }
        if (fileArg != null) Line($"n46flow {id}: --file {fileArg}");
        var json = V65Planted(PortData.Obj(PortData.LoadJson(path))); // NIGHT65 T: no plant, the same dictionary
        V65Stats.Clear();
        LevelDef level = LevelDef.From(json);
        var doors = F1EntryDoors(json);
        int n = level.rooms.Count;
        var measured = new bool[n];
        int a = 0, b = n - 1;
        if (roomsArg != null)
        {
            var want = roomsArg.Split(',');
            for (int i = 0; i < n; i++) measured[i] = want.Contains(level.rooms[i].id);
            if (!measured.Any(x => x)) { Line($"n46flow {id}: none of the rooms {roomsArg} is in the level"); return 2; }
            a = Array.IndexOf(measured, true);
            b = Array.LastIndexOf(measured, true);
        }
        else
        {
            for (int i = 0; i < n; i++) measured[i] = level.rooms[i].kind == "room" || (F1Bosses && level.rooms[i].kind == "boss"); // a boss is its own pilot's (the walk pilot killed none of o3 / v11 in 240 s: a pilot fault, not the room's)
            b = Array.LastIndexOf(measured, true); // and ends at the last (without --bosses: the room before the boss)
            a = Array.IndexOf(measured, true); // the walk starts at the first fight room (a level's opening link is a taught walk the pilot does not do: a1 held it 240 s)
        }
        double walk = cfg.Num("ground.walkSpeed"), stallV = 0.25 * walk;
        var pools = new F1Pool[n];
        for (int i = 0; i < n; i++) pools[i] = new F1Pool { id = level.rooms[i].id, kind = level.rooms[i].kind };
        F56StyleBegin(n, seeds); // NIGHT56 G1: the style column (Harness.N56.Style.cs)
        int runsDone = 0, lavaX = 0, holdX = 0, detX = 0, chaseX = 0, linkSkips = 0, stuckX = 0, deckX = 0;
        var levelTimes = new List<double>();
        string loaded = null;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
            SurvivalMap map = SurvivalMap.FromExport(V65Planted(PortData.Obj(PortData.LoadJson(path))));
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            if (rowsArg != null)
            {
                NightRowState st0 = sim.rowsRegistry.Defaults();
                foreach (string r in rowsArg.Split(',')) st0 = st0.Set(r.Trim(), true);
                sim.rowsOverride = st0;
            }
            var ap = new AutopilotInput(sim, "normal", seed);
            int aa = a;
            ap.GoalProvider = () =>
            {
                Campaign24 c = sim.campaign;
                LevelDef lv = sim.campaignLevel;
                if (c == null || c.room < 0) return lv.rooms[aa].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            var pilot = new F1Input(sim, new Walk38Pilot(sim, ap), json);
            sim.UseInput(V65Wrap(sim, pilot, json)); // NIGHT65 T: without --verbs65 the pilot itself
            var rs = new F1Room[n];
            for (int i = 0; i < n; i++) rs[i] = new F1Room();
            int stp = 0;
            sim.events.On<RoomEvent>("roomEnter", e => { if (e.room >= 0 && e.room < n && !rs[e.room].entered) { rs[e.room].entered = true; rs[e.room].enterStep = stp; } });
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room >= 0 && e.room < n && !rs[e.room].cleared) { rs[e.room].cleared = true; rs[e.room].clearSteps = stp - rs[e.room].enterStep; } });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room >= 0 && e.room < n) rs[e.room].deaths++; });
            sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room >= 0 && e.room < n) rs[e.room].falls++; });
            sim.events.On<FallReturn43Event>("fallReturn43", e => { Campaign24 c = sim.campaign; if (c != null && c.room >= 0 && c.room < n) rs[c.room].falls++; });
            sim.events.On<KillEvent>("kill", e =>
            {
                Campaign24 c = sim.campaign;
                if (c == null || c.room < 0 || c.room >= n || c.phase != Campaign24.Phase.Fight) return;
                F1Room r = rs[c.room];
                if (r.lastKill >= 0) r.gaps.Add((stp - r.lastKill) * sim.step);
                r.lastKill = stp;
                r.kills++;
            });
            sim.StartRun(seed, null, null);
            loaded = sim.campaignLevel?.id;
            if (sim.campaignLevel == null || sim.campaignLevel.id != id) { Line($"n46flow {id}: RED: the campaign loaded {loaded}, not {id}"); return 1; }
            int capSteps = MovementMath.StepsFor(cap, sim.step), maxSteps = capSteps * (b - a + 2) * 2;
            bool placed = a == 0, doorFreeOn = a > 0, done = false;
            string why = "";
            int roomAt = -1, roomSince = 0, skipTo = -1;
            double hpAtEnter = sim.health.hp;
            bool wasDown = false;
            var nearS = new List<float>();
            for (; stp < maxSteps && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); stp++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                Campaign24 c = sim.campaign;
                if (!placed && c != null && c.room >= 0)
                {
                    c.SetDoorFree29(true);
                    sim.Respawn(level.rooms[a].spawn, level.rooms[a].spawnYawDeg);
                    placed = true;
                    V67PutInAwards(sim, level, a); // NIGHT67 T1c: the awards of the rooms before the put-in room (Harness.N65.Verb.cs)
                }
                if (c == null || c.room < 0) continue;
                if (placed && doorFreeOn && c.room == a) { c.SetDoorFree29(false); doorFreeOn = false; }
                if (a > 0) V67SayEntry(sim, level, a, "n46flow"); // NIGHT67 T1c: a print only (a put-in run's measured room)
                if (c.room != roomAt) { roomAt = c.room; roomSince = stp; hpAtEnter = sim.health.hp; }
                else if (skipTo < 0 && c.room + 1 < n && (level.rooms[c.room].kind == "link" && stp - roomSince > MovementMath.StepsFor(Math.Max(30, 3 * level.rooms[c.room].par), sim.step)
                    || level.rooms[c.room].kind != "link" && c.phase == Campaign24.Phase.Cleared && rs[c.room].cleared && stp - rs[c.room].enterStep - rs[c.room].clearSteps > MovementMath.StepsFor(15, sim.step)))
                {
                    // also a cleared fight room the pilot has not left in 15 s (round 1: seed 2 stood 240 s at b1's near rail end, (8.5, -148.5),
                    // pressing at a free cell; the walk out of a cleared room is not measured)
                    // a traversal link (not measured) the walk pilot has not crossed in max(30 s, 3 x par): put it at the next room's spawn (DOORFREE for
                    // the move only, as the placement), counted (round 1: seed 1 hung 240 s in b2's lava gantry at 1 hp; the walk's fault, not a fight's)
                    skipTo = c.room + 1;
                    linkSkips++;
                    c.SetDoorFree29(true);
                    sim.Respawn(level.rooms[skipTo].spawn, level.rooms[skipTo].spawnYawDeg);
                    if (sim.health.hp < hpAtEnter) sim.health.hp = hpAtEnter; // the falls in the link it did not cross are not carried into the next fight
                }
                else if (stp - roomSince > capSteps) { why = $"cap: {cap:0} s in {level.rooms[c.room].id} ({c.phase})"; break; }
                if (skipTo >= 0 && c.room == skipTo) { c.SetDoorFree29(false); skipTo = -1; }
                if (rs[b].cleared) { done = true; break; }
                int i = c.room;
                if (!measured[i] || c.phase != Campaign24.Phase.Fight) continue;
                bool down = sim.health.Dead || sim.flow == Skygrind.Game.Flow.Down;
                if (down && !wasDown && F1Trace)
                    Console.WriteLine($"      seed {seed} {(stp - rs[i].enterStep) * sim.step:0.0} s into {level.rooms[i].id}: DEATH, waves spawned {c.wave}, alive {sim.enemies.AliveCount} ({string.Join(", ", sim.enemies.all.Where(en => en.alive).Take(8).Select(en => en.kind))}), feet ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z:0.0})");
                wasDown = down;
                if (down) continue;
                F1Room r = rs[i];
                if (level.rooms[i].kind == "boss")
                {
                    // the boss's untouchable windows (reveal46's job): steps in Reveal and in a leg / phase change, any boss module
                    bool rev = sim.lantern36 != null && sim.lantern36.brain.state == Boss36State.Reveal || sim.brawl40 != null && sim.brawl40.state == Brawl40State.Reveal
                        || F1In(sim.crown37?.brain, Boss37State.Reveal) || F1In(sim.loom37?.brain, Boss37State.Reveal) || F1In(sim.stag37?.brain, Boss37State.Reveal) || F1In(sim.wick37?.brain, Boss37State.Reveal);
                    bool ph = sim.brawl40 != null && sim.brawl40.state == Brawl40State.PhaseChange
                        || F1In(sim.crown37?.brain, Boss37State.LegChange) || F1In(sim.loom37?.brain, Boss37State.LegChange) || F1In(sim.stag37?.brain, Boss37State.LegChange) || F1In(sim.wick37?.brain, Boss37State.LegChange);
                    if (rev) r.reveal++;
                    if (ph) r.phase++;
                }
                Mover m = sim.mover;
                Vec3 p = m.position;
                double hv = Math.Sqrt(m.velocity.x * m.velocity.x + m.velocity.z * m.velocity.z);
                r.fight++;
                F56StyleStep(i, s, sim); // NIGHT56 G1: the rank shown on this in-fight step
                if (hv < stallV) { r.stall++; r.curStall++; if (r.curStall > r.maxStall) r.maxStall = r.curStall; }
                else r.curStall = 0;
                if (!m.grounded) r.air++;
                double best = double.MaxValue;
                int alive = 0;
                foreach (Enemy en in sim.enemies.all)
                {
                    if (!en.alive || en.standIn29) continue;
                    alive++;
                    double dx = en.position.x - p.x, dz = en.position.z - p.z, d = dx * dx + dz * dz;
                    if (d < best) best = d;
                }
                if (alive > 0)
                {
                    if (stp % 4 == 0) r.nearS.Add((float)Math.Sqrt(best));
                    double ex, ez;
                    if (doors.TryGetValue(level.rooms[i].id, out double[] dr)) { ex = Math.Max(dr[0], Math.Min(p.x, dr[2])); ez = Math.Max(dr[1], Math.Min(p.z, dr[3])); }
                    else { ex = level.rooms[i].spawn.x; ez = level.rooms[i].spawn.z; }
                    if ((ex - p.x) * (ex - p.x) + (ez - p.z) * (ez - p.z) <= 16) r.dwell++;
                }
                else if (level.rooms[i].kind != "boss") r.dead++;
            }
            if (done) { runsDone++; levelTimes.Add((stp - rs[a].enterStep) * sim.step); }
            else
            {
                Campaign24 c = sim.campaign;
                Vec3 p = sim.mover.position;
                if (why == "") why = $"stopped (flow {sim.flow}, {stp * sim.step:0} s)";
                Line($"  {id} seed {seed}: {why}; in {(c != null && c.room >= 0 ? level.rooms[c.room].id : "?")} {c?.phase} wave {c?.wave} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), alive {sim.enemies.AliveCount} ({string.Join(", ", sim.enemies.all.Where(en => en.alive).Take(3).Select(en => $"{en.kind} ({en.position.x:0.0}, {en.position.y:0.0}, {en.position.z:0.0})"))}), hp {sim.health.hp:0}");
            }
            double dt = sim.step;
            for (int i = 0; i < n; i++)
            {
                if (!measured[i] || !rs[i].entered) continue;
                F1Room r = rs[i];
                F1Pool q = pools[i];
                q.entered++;
                if (level.rooms[i].kind == "boss") Line($"    {id} seed {seed} {level.rooms[i].id} (boss): reveal {r.reveal * dt:0.00} s, leg / phase changes {r.phase * dt:0.00} s in {r.fight * dt:0.0} s of fight{(r.cleared ? "" : " (not killed)")}");
                if (!r.cleared) { q.caps++; continue; } // a room the pilot did not clear (the cap): the pilot's fault, left out of the room's numbers (its kill gaps too)
                q.gaps.AddRange(r.gaps); q.near.AddRange(r.nearS);
                q.fight += r.fight; q.stall += r.stall; q.air += r.air;
                q.maxStall.Add(r.maxStall * dt); q.dwell.Add(r.dwell * dt); q.dead.Add(r.dead * dt);
                q.deaths += r.deaths; q.falls += r.falls;
                if (r.cleared) { q.cleared++; q.clear.Add(r.clearSteps * dt); }
                if (perSeed) Line($"    {id} seed {seed} {level.rooms[i].id}: fight {r.fight * dt:0.0} s, stall {(r.fight > 0 ? 100.0 * r.stall / r.fight : 0):0.0} %, maxStall {r.maxStall * dt:0.0} s, air {(r.fight > 0 ? 100.0 * r.air / r.fight : 0):0.0} %, dwell {r.dwell * dt:0.0} s, dead {r.dead * dt:0.0} s, kills {r.kills}, deaths {r.deaths}, falls {r.falls}, clear {(r.cleared ? (r.clearSteps * dt).ToString("0.0", F1Inv) : "-")}");
            }
            lavaX += pilot.lavaExits; holdX += pilot.edgeHolds; detX += pilot.detours; chaseX += pilot.chases; stuckX += sim.stuckEnded46; deckX += pilot.deckHolds;
            ap.Detach();
        }
        Dictionary<string, string> geoRows = geo ? F1Geo(id, Line) : new Dictionary<string, string>();
        Line($"n46flow {id} (loaded {loaded}; BaseId {NightRows.BaseId(cfg, id)}): {runsDone}/{seeds} walks through {level.rooms[a].id}..{level.rooms[b].id}; level time median {F1Med(levelTimes)} s; the pilot's lava rule: out of lava {lavaX} steps, held at an edge {holdX} steps, chases round it {chaseX}, detours {detX}; put at the next room (a link not crossed in max(30 s, 3 x par) or a cleared room not left in 15 s) {linkSkips}; stuck46 burrowers ended {stuckX}");
        Line($"  room       kind | stall%  maxStall  air% | kgap med/max | near med/p10/p90 | dwell  dead | deaths falls | clear (k/n)    | geometry");
        var all = new F1Pool { id = "LEVEL", kind = "" };
        var perSeedDwell = new double[seeds]; // not tracked per seed across rooms: the level's dwell / dead are the sums of the room medians
        double sumDwell = 0, sumDead = 0, sumClear = 0;
        for (int i = 0; i < n; i++)
        {
            if (!measured[i]) continue;
            F1Pool q = pools[i];
            all.fight += q.fight; all.stall += q.stall; all.air += q.air;
            all.gaps.AddRange(q.gaps); all.near.AddRange(q.near);
            if (q.maxStall.Count > 0) all.maxStall.Add(F1MedD(q.maxStall)); // the level's longest stall: the worst room's median (a max over seeds is one seed's accident)
            all.deaths += q.deaths; all.falls += q.falls;
            bool boss = q.kind == "boss";
            sumDwell += F1MedD(q.dwell); if (!boss) sumDead += F1MedD(q.dead); sumClear += F1MedD(q.clear);
            geoRows.TryGetValue(q.id, out string g);
            Line("  " + F1RowText(q, boss, seeds) + " | " + (g ?? "-"));
        }
        Line($"  {"LEVEL",-10} {"",4} | {F1Pct(all.stall, all.fight),6} {F1Max(all.maxStall),8} {F1Pct(all.air, all.fight),5} | {F1Med(all.gaps),4} / {F1Max(all.gaps),-5} | {F1Med(all.near),4} / {F1Q(all.near, 0.1),4} / {F1Q(all.near, 0.9),-5} | {sumDwell,5:0.0} {sumDead,5:0.0} | {all.deaths,6} {all.falls,5} | {sumClear,6:0.0} sum of medians");
        Line($"n46flow {id} SUMMARY: {(runsDone == seeds ? "GREEN" : "SHORT")} {runsDone}/{seeds}; stall {F1Pct(all.stall, all.fight)} %, longest stall {F1Max(all.maxStall)} s, air {F1Pct(all.air, all.fight)} %, kill gap {F1Med(all.gaps)} / {F1Max(all.gaps)} s, nearest {F1Med(all.near)} m, dwell {sumDwell:0.0} s, dead {sumDead:0.0} s, deaths {all.deaths}, falls {all.falls}, clear {sumClear:0.0} s");
        if (F1LavaDetail) Line($"n46flow {id} lava detail: held at an edge {holdX} steps, {deckX} of them with the feet above 1 m (a deck or a column top over lava)");
        F56StyleTable(id, level, measured, Line); // NIGHT56 G1: the style column (Harness.N56.Style.cs)
        V65Report(id, Line); // NIGHT65 T: the VERBS65 block, only with --verbs65
        return runsDone == seeds ? 0 : 1;
    }

    static string F1RowText(F1Pool q, bool boss, int seeds) =>
        $"{q.id,-10} {q.kind,4} | {F1Pct(q.stall, q.fight),6} {F1Med(q.maxStall),8} {F1Pct(q.air, q.fight),5} | {F1Med(q.gaps),4} / {F1Max(q.gaps),-5} | {F1Med(q.near),4} / {F1Q(q.near, 0.1),4} / {F1Q(q.near, 0.9),-5} | {F1Med(q.dwell),5} {(boss ? "-" : F1Med(q.dead)),5} | {q.deaths,6} {q.falls,5} | {F1Med(q.clear),6} ({q.cleared}/{seeds}{(q.caps > 0 ? ", cap " + q.caps : "")})";

    static bool F1In(Boss37Brain b, Boss37State s) => b != null && b.state == s;

    static string F1Pct(long a, long b) => b == 0 ? "-" : (100.0 * a / b).ToString("0.0", F1Inv);
    static double F1MedD(List<double> l) { if (l.Count == 0) return 0; var s = l.OrderBy(x => x).ToList(); return s[s.Count / 2]; }
    static string F1Med(List<double> l) => l.Count == 0 ? "-" : F1MedD(l).ToString("0.0", F1Inv);
    static string F1Max(List<double> l) => l.Count == 0 ? "-" : l.Max().ToString("0.0", F1Inv);
    static string F1Med(List<float> l) => F1Q(l, 0.5);
    static string F1Q(List<float> l, double q)
    {
        if (l.Count == 0) return "-";
        var s = l.ToArray();
        Array.Sort(s);
        return s[Math.Min(s.Length - 1, (int)(q * s.Length))].ToString("0.0", F1Inv);
    }

    /// <summary>F1's pilot: the walk pilot (Walk38Pilot) everywhere, plus ONE level-data rule in every room that has lava (the level file's top-level
    /// `lava` rects; the autopilot's graph has no lava: n46flow round 0 on n45-foundry, the plain pilot died 18 times in a5 and stopped there):
    /// Walk36's / n45yard's rule made generic: in the lava (inside a rect, feet under -0.2 m) out by the nearest rim; on the ground never step into
    /// a rect where no solid stands over it (a bridge is a box whose top is within 1 m of the floor): stop at the edge and fight; with the room
    /// cleared, a 1 m grid path round the lava to the next room (the straight line to the exit crosses it). No room id is read.</summary>
    sealed class F1Input : IPlayerInput
    {
        readonly SurvivalSim sim;
        readonly Walk38Pilot inner;
        readonly List<double[]> lava = new List<double[]>();
        readonly List<double[]> boxes = new List<double[]>(); // [x0, y0, z0, x1, y1, z1]
        int step, pathRoom = -1, pathAt, holdRun, chaseAt, chaseFrom;
        List<(double x, double z)> path, chase;
        public int lavaExits, edgeHolds, detours, chases;

        public F1Input(SurvivalSim sim, Walk38Pilot inner, Dictionary<string, object> json)
        {
            this.sim = sim;
            this.inner = inner;
            if (json.TryGetValue("lava", out object lv))
                foreach (object o in PortData.Arr(lv))
                {
                    var r = PortData.Arr(PortData.Obj(o)["rect"]);
                    lava.Add(new[] { PortData.Num(r[0]), PortData.Num(r[1]), PortData.Num(r[2]), PortData.Num(r[3]) });
                }
            if (lava.Count > 0)
                foreach (object o in PortData.Arr(PortData.Obj(json["collision"])["boxes"]))
                {
                    var d = PortData.Obj(o);
                    var mn = PortData.Arr(d["min"]); var mx = PortData.Arr(d["max"]);
                    boxes.Add(new[] { PortData.Num(mn[0]), PortData.Num(mn[1]), PortData.Num(mn[2]), PortData.Num(mx[0]), PortData.Num(mx[1]), PortData.Num(mx[2]) });
                }
        }

        public double Yaw => inner.Yaw;
        public double Pitch => inner.Pitch;
        public void SetLook(double y, double p) => inner.SetLook(y, p);
        public bool Captured => inner.Captured;
        public CombatInput SampleCombat(bool active) => inner.SampleCombat(active);

        int LavaAt(double x, double z) => F1LavaAt(lava, x, z);

        bool Burning(double x, double z) => F1Burning(lava, boxes, x, z, 0); // the floor plans (Plan / Blocked, the cleared walk's line): feet on the floor

        public int deckHolds; // NIGHT68 I3: edge holds with the feet above 1 m (on a deck or a column top), printed by --lava-detail
        public Func<bool, InputFrame> moveOverride; // NIGHT68 I3: n46flow --lava-cases only (a scripted walk under the lava rule); null: the walk pilot

        static double Toward(Vec3 from, double tx, double tz) => Math.Atan2(-(tx - from.x), -(tz - from.z));

        bool Blocked(double x, double z)
        {
            if (Burning(x, z)) return true;
            foreach (double[] b in boxes) if (x >= b[0] - 0.4 && x <= b[3] + 0.4 && z >= b[2] - 0.4 && z <= b[5] + 0.4 && b[1] < 1.2 && b[4] > 1.3) return true; // a wall or pillar at body height
            return false;
        }

        /// <summary>A 1 m grid BFS over the room's rect (8 neighbours) from p to the goal, round the burning cells and the body-height solids.</summary>
        List<(double x, double z)> Plan(RoomDef rd, Vec3 p, Vec3 goal)
        {
            double x0 = rd.x0, z0 = rd.z0;
            int nx = (int)Math.Ceiling(rd.x1 - rd.x0), nz = (int)Math.Ceiling(rd.z1 - rd.z0);
            if (nx <= 0 || nz <= 0 || nx * nz > 40000) return null;
            int Cell(double x, double z) { int i = Math.Max(0, Math.Min(nx - 1, (int)Math.Floor(x - x0))), k = Math.Max(0, Math.Min(nz - 1, (int)Math.Floor(z - z0))); return i * nz + k; }
            var blocked = new bool[nx * nz];
            for (int i = 0; i < nx; i++) for (int k = 0; k < nz; k++) blocked[i * nz + k] = Blocked(x0 + i + 0.5, z0 + k + 0.5);
            int s = Cell(p.x, p.z), g = Cell(goal.x, goal.z);
            blocked[s] = false; blocked[g] = false;
            var prev = new int[nx * nz];
            for (int i = 0; i < prev.Length; i++) prev[i] = -2;
            var q = new Queue<int>();
            q.Enqueue(s); prev[s] = -1;
            while (q.Count > 0)
            {
                int c = q.Dequeue();
                if (c == g) break;
                int ci = c / nz, ck = c % nz;
                for (int di = -1; di <= 1; di++) for (int dk = -1; dk <= 1; dk++)
                {
                    if (di == 0 && dk == 0) continue;
                    int i2 = ci + di, k2 = ck + dk;
                    if (i2 < 0 || k2 < 0 || i2 >= nx || k2 >= nz) continue;
                    int c2 = i2 * nz + k2;
                    if (blocked[c2] || prev[c2] != -2) continue;
                    if (di != 0 && dk != 0 && (blocked[ci * nz + k2] || blocked[i2 * nz + ck])) continue;
                    prev[c2] = c; q.Enqueue(c2);
                }
            }
            if (prev[g] == -2) return null;
            var res = new List<(double, double)>();
            for (int c = g; c != -1; c = prev[c]) res.Add((x0 + c / nz + 0.5, z0 + c % nz + 0.5));
            res.Reverse();
            return res;
        }

        public InputFrame SampleMove(bool active)
        {
            InputFrame auto = inner.SampleMove(active);
            if (moveOverride != null) auto = moveOverride(active);
            step++;
            if (active) F57Answer(sim, ref auto); // NIGHT57 G press57: the pilot steps out of a press57 circle under it (Harness.N57.Press.cs; row off: untouched)
            Campaign24 c = sim.campaign;
            if (!active || lava.Count == 0 || c == null || c.room < 0 || sim.campaignLevel == null) return auto;
            RoomDef rd = sim.campaignLevel.rooms[c.room];
            if (rd.kind != "room") return auto; // links (b2's lava gantry: Walk38Pilot's ride chain) and bosses keep the walk pilot's own frame
            Vec3 p = sim.mover.position;
            bool grounded = sim.mover.grounded;
            InputFrame f = auto;
            int li = LavaAt(p.x, p.z);
            if (li >= 0 && p.y < -0.2)
            {
                lavaExits++;
                double[] r = lava[li];
                double dW = p.x - r[0], dE = r[2] - p.x, dS = p.z - r[1], dN = r[3] - p.z, m = Math.Min(Math.Min(dW, dE), Math.Min(dS, dN));
                double tx = p.x, tz = p.z;
                if (m == dW) tx = r[0] - 1.5; else if (m == dE) tx = r[2] + 1.5; else if (m == dS) tz = r[1] - 1.5; else tz = r[3] + 1.5;
                f.yaw = Toward(p, tx, tz);
                f.moveX = 0; f.moveZ = 1;
                f.jump = new Button { held = true, pressed = grounded && step % 12 == 0 };
                SetLook(f.yaw, f.pitch);
                return f;
            }
            if (c.phase == Campaign24.Phase.Cleared && c.room + 1 < sim.campaignLevel.rooms.Count)
            {
                Vec3 goal = sim.campaignLevel.rooms[c.room + 1].spawn;
                bool crosses = false;
                double gx = goal.x - p.x, gz = goal.z - p.z, len = Math.Sqrt(gx * gx + gz * gz);
                for (double t = 0; t <= Math.Min(len, 30) && !crosses; t += 0.5) crosses = Burning(p.x + gx / len * t, p.z + gz / len * t);
                if (crosses || (path != null && pathRoom == c.room))
                {
                    if (path == null || pathRoom != c.room) { path = Plan(rd, p, goal); pathRoom = c.room; pathAt = 0; if (path != null) detours++; if (F1Trace) Console.WriteLine($"      F1Input {rd.id}: detour from ({p.x:0.0}, {p.z:0.0}) to ({goal.x:0.0}, {goal.z:0.0}): {(path == null ? "NO PATH" : path.Count + " cells")}"); }
                    if (F1Trace && step % 120 == 0) Console.WriteLine($"      F1Input {rd.id}: at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}) path {(path == null ? "-" : pathAt + "/" + path.Count)} {sim.mover.state}");
                    if (path != null)
                    {
                        while (pathAt < path.Count - 1 && JsMath.Hypot(p.x - path[pathAt].x, p.z - path[pathAt].z) < 1.2) pathAt++;
                        int look = Math.Min(path.Count - 1, pathAt + 1);
                        // past the path's end (the room's edge toward the next room): straight on to the next room's spawn (round 1: handing back to the
                        // autopilot there, its graph led back over the lava and the two rules traded the bot for 5000 s at a5's far rim)
                        double lx = pathAt >= path.Count - 1 ? goal.x : path[look].x, lzz = pathAt >= path.Count - 1 ? goal.z : path[look].z;
                        f = new InputFrame { moveZ = 1, yaw = Toward(p, lx, lzz), pitch = 0 };
                        SetLook(f.yaw, f.pitch);
                        return f;
                    }
                }
                return auto;
            }
            path = null; pathRoom = -1;
            if (chase != null)
            {
                // held at the lava's edge 2 s with the fight going on (round 1: n45-kiln kb3, 4 of 10 seeds stood 240 s at the pour's rim with the
                // last body across it): 3 s on a grid path round the lava toward the nearest live enemy, the autopilot's look and fire kept
                while (chaseAt < chase.Count - 1 && JsMath.Hypot(p.x - chase[chaseAt].x, p.z - chase[chaseAt].z) < 1.2) chaseAt++;
                if (step - chaseFrom > 360 || chaseAt >= chase.Count - 1) chase = null;
                else
                {
                    int look = Math.Min(chase.Count - 1, chaseAt + 1);
                    Vec3 w2 = MovementMath.WishDirection(0, 1, Toward(p, chase[look].x, chase[look].z));
                    // the move toward the path cell in the look's frame (the autopilot keeps aiming)
                    double yawL = auto.yaw, sx = w2.x, sz = w2.z;
                    // moveX / moveZ such that WishDirection(moveX, moveZ, yawL) points along (sx, sz)
                    Vec3 fw = MovementMath.WishDirection(0, 1, yawL), rt = MovementMath.WishDirection(1, 0, yawL);
                    f.moveZ = sx * fw.x + sz * fw.z;
                    f.moveX = sx * rt.x + sz * rt.z;
                    return f;
                }
            }
            if (grounded)
            {
                Vec3 w = MovementMath.WishDirection(auto.moveX, auto.moveZ, auto.yaw);
                if (F1Burning(lava, boxes, p.x + w.x * 1.5, p.z + w.z * 1.5, p.y)) // NIGHT68 I3: the feet's height passed to the rule
                {
                    edgeHolds++;
                    if (p.y > 1.0) deckHolds++;
                    holdRun++;
                    f.moveX = f.moveZ = 0;
                    f.jump = default;
                    f.dash = default;
                    if (holdRun > 240)
                    {
                        Enemy best = null;
                        double bd = double.MaxValue;
                        foreach (Enemy en in sim.enemies.all)
                        {
                            if (!en.alive || en.standIn29 || en.position.y < -1.5) continue; // a burrower under the floor is not chased
                            double d = (en.position.x - p.x) * (en.position.x - p.x) + (en.position.z - p.z) * (en.position.z - p.z);
                            if (d < bd) { bd = d; best = en; }
                        }
                        // none above ground (round 1: kb3's last body a burrower at y -6 under the pour, the pilot at the rim for 240 s): back off
                        // the rim to the room's spawn, so it can come up on open floor
                        chase = Plan(rd, p, best != null ? best.position : rd.spawn); chaseAt = 0; chaseFrom = step; if (chase != null) chases++;
                        holdRun = 0;
                    }
                }
                else holdRun = 0;
            }
            return f;
        }
    }

    /// <summary>The geometry numbers per room from Tools/night/n46-flow-geo.cjs (node; FLOW-AUDIT's rasteriser): "roomId<TAB>text" lines.</summary>
    static Dictionary<string, string> F1Geo(string id, Action<string> Line)
    {
        var res = new Dictionary<string, string>();
        try
        {
            var psi = new ProcessStartInfo("node", $"Tools/night/n46-flow-geo.cjs {id}") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
            using (Process pr = Process.Start(psi))
            {
                string o = pr.StandardOutput.ReadToEnd();
                string err = pr.StandardError.ReadToEnd();
                pr.WaitForExit();
                foreach (string l in o.Split('\n'))
                {
                    int tab = l.IndexOf('\t');
                    if (tab > 0) res[l.Substring(0, tab)] = l.Substring(tab + 1).TrimEnd('\r');
                }
                if (pr.ExitCode != 0) Line($"n46flow {id}: geometry tool exit {pr.ExitCode}: {err.Split('\n')[0]}");
            }
        }
        catch (Exception e) { Line($"n46flow {id}: geometry not run ({e.Message})"); }
        return res;
    }
}
