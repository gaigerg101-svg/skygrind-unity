// NIGHT32 (builder G, DESIGN32 G, D-1 / D-3): the grapple-spam bot and the grapple's limits, headless. ONE entry method, N32Grapple(cmd, args, ref code)
// (declared and called in Program.cs).
//   grapple32 [--off32] [--secs 20] [--seed n]   the spam bots, each for `secs` seconds, invulnerable, on two maps:
//     span: level 4 (Design/levels/n30-span.json, the CAMPAIGN composition with the ARCLINE owned), from the first room spawn that sees a node;
//     survival: PortExport/maps/survival.json (the SURVIVAL composition, round 1 live, the ARCLINE owned).
//     Bot TAP:    whenever the rope is idle, aim at the nearest target (a node in range with a clear line, else the nearest big enemy, else the
//                 farthest wall within 38 m) and TAP (press one step, up the next). Never presses while a pull is on (that would cancel it).
//     Bot CANCEL: the same, then a JUMP 0.2 s into each pull (A1: a cancel kept 44 m/s and skipped the 12 m/s arrival cap: the endless chain).
//     Bot HOLD:   whenever the rope is idle, aim the same way and HOLD 0.6 s (the swing), up, again.
//     Each line: hooks fired, pulls (taps that reeled in), the speed (mean / max over the run), the time airborne, and (rows on) charges spent / refunded /
//     kills that gave one back. The contract (closing reminder): "a spam bot gets 2 pulls, then one per 4 s (plus kills)".
//   `--off32` (Program.cs: N32Blocks.ForceOff): NIGHT31's grapple, the BEFORE line.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N32Grapple(string cmd, string[] args, ref int code)
    {
        if (cmd == "grapple32") Grapple32Command(args, ref code);
        else if (cmd == "ring32") Ring32Command(args, ref code);
        else if (cmd == "balance32") Balance32Command(args, ref code);
    }

    /// <summary>`balance32 [--seeds 5] [--rooms 2] [--nottk] [--norooms]` (DESIGN32 G balance): rows as the config has them (--off32: NIGHT31).
    /// A. TIME TO KILL: the SURVIVAL composition with the round clock held (holdRunClock: no director spawns), invulnerable, one enemy of each common
    ///    campaign kind (husk 162, swarmer 68, spitter 31, hulk 2 spawn rows in the n30 / n32 levels) spawned 10 m away in the open; the weapon in hand,
    ///    perfect aim at its aim point, fire held (pressed every step); time from the first step to its death, cap 15 s; p50 over seeds.
    ///    Weapons: KESTREL (revolver), PULSE gauntlet (gauntlet), THUNDERCLAP (shotgun), NAILGUN (nailgun).
    /// B. ROOM CLEAR: the level pilot (Walk29Pilot + Span30Pilot, as walk30) on each campaign level file on disk (foundry / iceline / kiln / span /
    ///    spire, the n32 file where one exists), `rooms` seeds: each non-boss room's clear time (Campaign24.clearSteps); the median and every room over 3x.</summary>
    static void Balance32Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        int seeds = int.Parse(Opt30(args, "--seeds", "5"), G32ci);
        int roomSeeds = int.Parse(Opt30(args, "--rooms", "2"), G32ci);
        bool on = !N32Blocks.ForceOff;
        Console.WriteLine($"balance32 {(on ? "rows as configured (balance32 " + cfg.Design("n32.balance32.enabled") + ")" : "--off32 (NIGHT31)")}");
        if (!Flag(args, "--nottk"))
        {
            string[] weapons = { "revolver", "gauntlet", "shotgun", "nailgun" };
            string[] names = { "KESTREL", "PULSE", "THUNDERCLAP", "NAILGUN" };
            string[] kinds = { "husk", "swarmer", "spitter", "tank", "husk x4" };
            Console.WriteLine($"balance32 A time to kill (s, p50 of {seeds}; '>15' = not killed in 15 s)   " + string.Join("  ", Array.ConvertAll(kinds, k => k.PadLeft(8))));
            foreach (int wi in new[] { 0, 1, 2, 3 })
            {
                var cells = new List<string>();
                foreach (string kind in kinds)
                {
                    var times = new List<double>();
                    for (int s = 0; s < seeds; s++) times.Add(B32Ttk(cfg, weapons[wi], kind, (uint)(3300 + s), out _));
                    times.Sort();
                    double p50 = times[times.Count / 2];
                    cells.Add((p50 >= 15 ? ">15" : G32f(p50, "0.00")).PadLeft(8));
                }
                Console.WriteLine($"balance32 A {names[wi],-12} ({weapons[wi],-8})                              " + string.Join("  ", cells));
            }
        }
        if (!Flag(args, "--norooms"))
        {
            var levels = new List<string>();
            foreach (string id in new[] { "foundry", "iceline", "kiln", "span", "spire" })
            {
                string p32 = $"Design/levels/n32-{id}.json", p30 = $"Design/levels/n30-{id}.json";
                if (File.Exists(p32)) levels.Add(p32); else if (File.Exists(p30)) levels.Add(p30);
            }
            var all = new List<(string level, string room, double secs)>();
            foreach (string path in levels)
            {
                var perRoom = new Dictionary<string, List<double>>();
                LevelDef lv0 = LevelDef.From(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
                for (int s = 0; s < roomSeeds; s++)
                {
                    List<(string, double)> rooms = B32Walk(cfg, path, (uint)(s + 1), out string status);
                    foreach (var (room, secs) in rooms) { if (!perRoom.TryGetValue(room, out var l)) perRoom[room] = l = new List<double>(); l.Add(secs); }
                    Console.WriteLine($"balance32 B {lv0.id} seed {s + 1}: {status}");
                }
                var parts = new List<string>();
                foreach (RoomDef rd in lv0.rooms)
                {
                    if (rd.kind == "boss" || !perRoom.TryGetValue(rd.id, out var l)) continue;
                    l.Sort();
                    double med = l[l.Count / 2];
                    if (rd.kind != "link") all.Add((lv0.id, rd.id, med)); // links (corridors, no fight) stay out of the median
                    parts.Add($"{rd.id}{(rd.kind == "link" ? "(link)" : "")} {G32f(med)}{(rd.par > 0 ? $"/par {G32f(rd.par, "0")}" : "")}");
                }
                Console.WriteLine($"balance32 B {lv0.id}: room clear s (median of seeds): {string.Join(", ", parts)}");
            }
            var secsAll = all.ConvertAll(x => x.secs);
            secsAll.Sort();
            double median = secsAll.Count > 0 ? secsAll[secsAll.Count / 2] : 0;
            var over = all.FindAll(x => x.secs > 3 * median);
            Console.WriteLine($"balance32 B: {all.Count} rooms, median clear {G32f(median)} s; over 3x ({G32f(3 * median)} s): {(over.Count == 0 ? "none" : string.Join(", ", over.ConvertAll(x => $"{x.level} {x.room} {G32f(x.secs)} s")))}");
        }
        code = 0;
    }

    /// <summary>One time-to-kill run: seconds until the spawned `kind` dies (15: not killed); `why` notes.</summary>
    static double B32Ttk(GameConfig cfg, string weapon, string kind, uint seed, out string why)
    {
        why = "";
        var si = new ScriptedInput();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.UseInput(si);
        sim.StartRun(seed, null, null);
        sim.holdRunClock = true;
        for (int s = 0; s < 12; s++) G32Tick(sim, si, null, false, false, false);
        Loadout lo = sim.loadout;
        if (!lo.owned.Contains(weapon)) lo.AddWeapon(weapon);
        int slotWant = lo.owned.IndexOf(weapon);
        // a clear direction from the player (no wall within 14 m at chest height)
        Vec3 p = sim.mover.position;
        Vec3 chest = new Vec3(p.x, p.y + 1.2, p.z);
        Vec3? at = null;
        for (int k = 0; k < 16 && !at.HasValue; k++)
        {
            double a = k * Math.PI / 8;
            var d = new Vec3(-Math.Sin(a), 0, -Math.Cos(a));
            if (sim.world.RayIntersect(chest, d, out RayHit h) && h.distance < 14) continue;
            at = new Vec3(p.x + d.x * 10, p.y, p.z + d.z * 10);
        }
        if (!at.HasValue) { why = "no open direction"; return 15; }
        // the swap first (the lockout), then the target
        for (int s = 0; s < 120 && lo.Weapon != weapon; s++)
        {
            si.Set(new InputFrame(), new CombatInput { weapon = slotWant });
            sim.health.hp = sim.health.max; sim.mover.invulnerable = true; sim.hitstopLeft = 0;
            sim.Step();
        }
        for (int s = 0; s < 60; s++) G32Tick(sim, si, null, false, false, false);
        // the target(s): one of `kind`, or "husk x4" (four husks 1.5 m apart across the line), each given to the brain as a campaign room spawn does
        var targets = new List<Husk>();
        bool group = kind.EndsWith(" x4", StringComparison.Ordinal);
        string k0 = group ? kind.Substring(0, kind.Length - 3) : kind;
        Vec3 dir = Vec3.Normalize(new Vec3(at.Value.x - p.x, 0, at.Value.z - p.z));
        for (int i = 0; i < (group ? 4 : 1); i++)
        {
            double off = group ? (i - 1.5) * 1.5 : 0;
            var pos = new Vec3(at.Value.x - dir.z * off, at.Value.y, at.Value.z + dir.x * off);
            Husk h = sim.husks.Spawn(k0, pos, sim.mover.position);
            if (h == null) continue;
            if (!(h is Lancer)) sim.brain.Attach(h);
            targets.Add(h);
        }
        if (targets.Count == 0) { why = "no spawn"; return 15; }
        bool melee = weapon == "gauntlet";
        int max = MovementMath.StepsFor(15, sim.step);
        for (int s = 0; s < max; s++)
        {
            Husk target = null;
            double bd = double.MaxValue;
            foreach (Husk h in targets) if (h.alive) { double d = Vec3.DistanceTo(h.position, sim.mover.position); if (d < bd) { bd = d; target = h; } }
            if (target == null) return s * sim.step;
            (double yaw, double pitch) = ArclineLook(sim.Eye, target.AimPoint);
            si.SetLook(yaw, pitch);
            // the gauntlet walks in (a melee weapon is used by closing the gap; standing still it never reaches a spitter that keeps its range)
            var mv = new InputFrame { moveZ = melee && bd > 2.2 ? 1 : 0 };
            bool swing = !melee || bd <= cfg.Num("punch.range") + 1.0; // a melee swing only within reach (a whiff at 10 m costs the whole 2.5 s swing)
            si.Set(mv, new CombatInput { weapon = -1, fire = new Button { held = swing, pressed = swing } });
            sim.health.hp = sim.health.max;
            sim.mover.invulnerable = true;
            sim.hitstopLeft = 0;
            sim.Step();
        }
        return 15;
    }

    /// <summary>The level pilot through one level file: each cleared non-boss room's id and clear seconds (Campaign24.clearSteps, in clear order).</summary>
    static List<(string, double)> B32Walk(GameConfig cfg, string path, uint seed, out string status)
    {
        SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
        LevelDef level = LevelDef.From(json);
        int playable = level.rooms.FindAll(rr => rr.kind != "boss").Count;
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        N30Blocks b = N30Blocks.From(cfg);
        b.span = true;
        sim.n30Override = b;
        var ap = new AutopilotInput(sim, "normal", seed);
        ap.GoalProvider = () =>
        {
            Campaign24 c = sim.campaign;
            LevelDef lv = sim.campaignLevel;
            if (c == null || c.room < 0) return lv.rooms[0].spawn;
            if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
            return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
        };
        var pilot = new Walk29Pilot(sim, ap);
        sim.UseInput(pilot);
        var order = new List<string>();
        sim.events.On<RoomEvent>("roomClear", e => order.Add(e.id));
        sim.StartRun(seed, null, null);
        int max = MovementMath.StepsFor(600, sim.step), st = 0;
        for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
        {
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            if (sim.campaign != null && sim.campaign.clearSteps.Count >= playable) break;
        }
        ap.Detach();
        var res = new List<(string, double)>();
        var cs = sim.campaign != null ? sim.campaign.clearSteps : new List<int>();
        for (int i = 0; i < cs.Count && i < order.Count; i++) res.Add((order[i], cs[i] * sim.step));
        var deaths = new List<string>();
        int total = 0;
        if (sim.campaign != null)
            for (int i = 0; i < sim.campaign.roomDeaths.Count && i < level.rooms.Count; i++)
                if (sim.campaign.roomDeaths[i] > 0) { deaths.Add($"{level.rooms[i].id} x{sim.campaign.roomDeaths[i]}"); total += sim.campaign.roomDeaths[i]; }
        status = $"level {level.id}: cleared {cs.Count} of {playable} rooms in {G32f(st * sim.step, "0")} s, deaths {total} ({(deaths.Count == 0 ? "none" : string.Join(", ", deaths))})";
        return res;
    }

    /// <summary>`ring32 [--seeds 3]` (the lead, 18:41: why grapple31 E's tape went from 8 pulls to 0 with the row on; do n30-span's ring routes still work at 40 m?):
    /// A. grapple31 E's exact script (G31Sim, seed 3106, 2400 steps, the same `between`), row off and on: every hook fired: what it took (node / surface), the
    ///    distance to the anchor and to the node the script aimed at, and how the press ended (pull, refused, cleared).
    /// B. n30-span's nodes: for each, the distance from the nearest other node in sight and from the nearest room spawn in sight (the press points of a ring route).
    /// C. the level pilot (Walk29Pilot + Span30Pilot, as spanpilot30) on n30-span, row off then on, `seeds` seeds: every node hook's press distance (max), and the
    ///    crossing rooms crossed by the grapple.</summary>
    static void Ring32Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        int seeds = int.Parse(Opt30(args, "--seeds", "3"), G32ci);
        string path = "Design/levels/n30-span.json";
        // ---- A ----
        foreach (bool on in new[] { false, true })
        {
            SurvivalMap map = CampaignMapFrom(path, cfg, out _);
            var si = new ScriptedInput();
            SurvivalSim sim = G31Sim(cfg, map, true, si);
            sim.n32Override = new N32Blocks { grapple32 = on };
            var lines = new List<string>();
            Vec3 aimAt = default;
            int i = 0, pullsE = 0;
            sim.events.On<GrapplePull31Event>("grapplePull", e => pullsE++);
            sim.mover.Events += e =>
            {
                if (e.type != MoverEventType.RopeFire) return;
                lines.Add($"s{i} {(e.node ? "NODE" : "surface")} {G32f(e.distance)} m (aimed node {G32f(Vec3.DistanceTo(sim.Eye, aimAt))} m away)");
            };
            sim.StartRun(3106, null, null);
            TapeScript.God(sim);
            int empty0 = 0;
            for (i = 0; i < 2400 && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); i++)
            {
                aimAt = map.grappleNodes[0];
                double bd = double.MaxValue;
                foreach (Vec3 gn in map.grappleNodes) { double d = Vec3.DistanceTo(gn, sim.mover.position); if (d > 6 && d < bd) { bd = d; aimAt = gn; } }
                (double yaw, double pitch) = ArclineLook(sim.Eye, aimAt);
                int k = i % 180;
                bool tapCycle = (i / 180) % 3 != 2;
                bool held = i >= 240 && (tapCycle ? k < 5 : k < 100);
                si.SetLook(yaw, pitch);
                si.Set(new InputFrame { moveZ = k > 120 ? 1 : 0, grapple = new Button { held = held, pressed = i >= 240 && k == 0 }, jump = new Button { pressed = k == 150, held = k >= 150 && k < 156 } },
                    new CombatInput { weapon = -1, fire = new Button { held = k % 60 < 10, pressed = k % 60 == 0 } });
                sim.Step();
            }
            empty0 = sim.g32Empty;
            Console.WriteLine($"ring32 A ({(on ? "row ON" : "row OFF")}) grapple31 E's script: hooks {lines.Count}, pulls {pullsE}, refused taps {sim.grappleTap31.refused}, presses cleared {empty0}; player at ({G32f(sim.mover.position.x)}, {G32f(sim.mover.position.y)}, {G32f(sim.mover.position.z)})");
            foreach (string l in lines) Console.WriteLine("ring32 A   " + l);
        }
        // ---- B ----
        {
            SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
            LevelDef level = LevelDef.From(json);
            var si = new ScriptedInput();
            SurvivalSim sim = G31Sim(cfg, map, true, si);
            G31Warm(sim, si, 3107);
            int planNodes = json.TryGetValue("grappleNodes", out object gnO) && gnO is List<object> gnl ? gnl.Count : map.grappleNodes.Count;
            double worst = 0;
            for (int n = 0; n < planNodes; n++)
            {
                Vec3 p = map.grappleNodes[n];
                double fromNode = double.MaxValue, fromSpawn = double.MaxValue;
                string fromId = "-";
                for (int m = 0; m < planNodes; m++)
                {
                    if (m == n) continue;
                    double d = Vec3.DistanceTo(p, map.grappleNodes[m]);
                    if (d < fromNode && G31Sees(sim, map.grappleNodes[m], p)) fromNode = d;
                }
                foreach (RoomDef rr in level.rooms)
                {
                    Vec3 eye = new Vec3(rr.spawn.x, rr.spawn.y + 1.6, rr.spawn.z);
                    double d = Vec3.DistanceTo(eye, p);
                    if (d < fromSpawn && G31Sees(sim, eye, p)) { fromSpawn = d; fromId = rr.id; }
                }
                double best = Math.Min(fromNode, fromSpawn);
                worst = Math.Max(worst, best);
                Console.WriteLine($"ring32 B node {n} ({G32f(p.x)}, {G32f(p.y)}, {G32f(p.z)}): nearest node in sight {(fromNode < 1e9 ? G32f(fromNode) : "-")} m, nearest spawn in sight {(fromSpawn < 1e9 ? $"{G32f(fromSpawn)} m ({fromId})" : "-")}");
            }
            Console.WriteLine($"ring32 B: the farthest node from its nearest press point (another node or a spawn, in sight): {G32f(worst)} m");
        }
        // ---- C ----
        foreach (bool on in new[] { false, true })
        {
            double maxNode = 0, sumNode = 0;
            int nodeFires = 0, passes = 0, over40 = 0;
            var dists = new List<double>();
            for (int s = 0; s < seeds; s++)
            {
                SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
                LevelDef level = LevelDef.From(json);
                int last = level.rooms.FindAll(rr => rr.kind != "boss").Count - 1;
                AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
                AutopilotInput.GraphSource = BotNav.Survival;
                SurvivalSim sim = SurvivalSim.Headless(cfg, map);
                sim.noRecords = sim.noLab = true;
                sim.setup.mode = Campaign24.Mode;
                N30Blocks b = N30Blocks.From(cfg);
                b.span = true;
                sim.n30Override = b;
                sim.n32Override = new N32Blocks { grapple32 = on };
                var ap = new AutopilotInput(sim, "normal", (uint)(s + 1));
                ap.GoalProvider = () =>
                {
                    Campaign24 c = sim.campaign;
                    LevelDef lv = sim.campaignLevel;
                    if (c == null || c.room < 0) return lv.rooms[0].spawn;
                    if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                    return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
                };
                var pilot = new Walk29Pilot(sim, ap);
                sim.UseInput(pilot);
                var crossing = level.rooms.FindAll(rr => rr.kind != "boss" && Span30Pilot.IsCrossing(map, rr)).ConvertAll(rr => rr.id);
                int targetIdx = crossing.Count > 0 ? level.rooms.FindIndex(rr => rr.id == crossing[crossing.Count - 1]) : last;
                sim.mover.Events += e =>
                {
                    if (e.type != MoverEventType.RopeFire || !e.node) return;
                    nodeFires++;
                    sumNode += e.distance;
                    maxNode = Math.Max(maxNode, e.distance);
                    dists.Add(e.distance);
                    if (e.distance > 40) over40++;
                };
                sim.StartRun((uint)(s + 1), null, null);
                int max = MovementMath.StepsFor(240, sim.step), st = 0;
                bool reached = false;
                for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
                {
                    AutopilotInput.TakeDraft(sim);
                    sim.Step();
                    if (sim.campaign != null && (sim.campaign.room > targetIdx || sim.campaign.clearSteps.Count > last)) { reached = true; break; }
                }
                ap.Detach();
                Span30Pilot sp = pilot.span30;
                bool allCrossed = crossing.TrueForAll(id => sp.crossedRooms.Contains(id));
                if (reached && allCrossed) passes++;
                Console.WriteLine($"ring32 C ({(on ? "row ON" : "row OFF")}) seed {s + 1}: crossing rooms [{string.Join(", ", crossing)}], crossed by the grapple [{string.Join(", ", sp.crossedRooms)}], past the last {reached} in {G32f(st * sim.step)} s; attaches {sp.attaches}, given up {sp.gaveUp}{(on ? $"; presses cleared {sim.g32Empty}, refused taps {sim.grappleTap31.refused}" : "")}");
            }
            dists.Sort();
            Console.WriteLine($"ring32 C ({(on ? "row ON" : "row OFF")}): {passes}/{seeds} seeds past every crossing; node hooks {nodeFires}, press-to-node distance mean {G32f(sumNode / Math.Max(1, nodeFires))} max {G32f(maxNode)} m, over 40 m: {over40}; sorted [{string.Join(" ", dists.ConvertAll(d => G32f(d, "0")))}]");
        }
        code = 0;
    }

    static readonly CultureInfo G32ci = CultureInfo.InvariantCulture;
    static string G32f(double v, string f = "0.0") => v.ToString(f, G32ci);

    /// <summary>One spam bot's result.</summary>
    sealed class G32Result
    {
        public int fires, pulls, cancels, releases, steps, airSteps, kills32, refunds;
        public double speedSum, speedMax, cancelSpeedMax;
        public readonly List<double> pullTimes = new List<double>();
        public string extra = "";
    }

    static void Grapple32Command(string[] args, ref int code)
    {
        double secs = double.Parse(Opt30(args, "--secs", "20"), G32ci);
        uint seed = uint.Parse(Opt30(args, "--seed", "3201"), G32ci);
        bool on = !N32Blocks.ForceOff;
        GameConfig cfg = Config();
        Console.WriteLine($"grapple32 {(on ? "ON (design.n32.grapple32 forced on for these runs)" : "OFF (--off32: NIGHT31's grapple)")}: {G32f(secs, "0")} s per bot, seed {seed}; arcline range {G32f(cfg.movement.arcline.range, "0")} m, cooldown {G32f(cfg.movement.arcline.cooldown, "0.00")} s");
        var results = new List<(string map, string bot, G32Result r)>();
        foreach (string mapName in new[] { "span", "survival" })
            foreach (string bot in new[] { "tap", "cancel", "hold" })
            {
                G32Result r = G32Spam(cfg, mapName, bot, secs, seed, on);
                if (r == null) { Console.WriteLine($"grapple32 {mapName} {bot}: no start"); code = 2; return; }
                results.Add((mapName, bot, r));
                double hz = cfg.movement.loop.simHz;
                string times = r.pullTimes.Count == 0 ? "-" : string.Join(" ", r.pullTimes.ConvertAll(t => G32f(t, "0.0")));
                Console.WriteLine($"grapple32 {mapName,-8} {bot,-6}: fires {r.fires,3}, pulls {r.pulls,3}, cancels {r.cancels,2}; speed mean {G32f(r.speedSum / Math.Max(1, r.steps))} max {G32f(r.speedMax)} m/s (after a cancel max {G32f(r.cancelSpeedMax)}); airborne {G32f(r.airSteps / hz)} of {G32f(r.steps / hz)} s{r.extra}; hook times [{times}]");
            }
        // WALL (D-3): a tap aimed at a bare wall (no node in reach of the pick) and a hold at the same wall, on the survival map.
        string wall = G32Wall(cfg, seed, on, out bool wallOk);
        Console.WriteLine($"grapple32 wall: {wall}");
        // The contract (rows on): the TAP bot on the span fires at most 2 + ceil((secs - 0.8) / recharge) + kills hooks; with the rows off it is a free spam.
        G32Result tapSpan = results.Find(x => x.map == "span" && x.bot == "tap").r;
        if (on)
        {
            double recharge = cfg.Has("design.n32.grapple32.rechargeSeconds") ? cfg.Num("design.n32.grapple32.rechargeSeconds") : 4;
            int cap = 2 + (int)Math.Floor(secs / recharge);
            bool ok = wallOk;
            foreach (var x in results)
                if (x.r.fires - x.r.refunds > cap + x.r.kills32) ok = false;
            Console.WriteLine(ok ? $"grapple32 GREEN: every bot's charged hooks (fired minus given back) at most 2 + floor({secs}/{recharge}) = {cap} (plus its kills); the span TAP bot {tapSpan.pulls} pulls vs NIGHT31's 38" : $"grapple32 RED: a bot's charged hooks went over {cap} plus its kills");
            code = ok ? 0 : 1;
        }
        else
        {
            Console.WriteLine($"grapple32 BEFORE (--off32): the span TAP bot fired {tapSpan.fires} hooks in {G32f(secs, "0")} s ({G32f(tapSpan.fires / secs, "0.00")} per s)");
            code = 0;
        }
    }

    /// <summary>One spam run. `on`: design.n32.grapple32 for this run (the override; --off32 forces it off).</summary>
    static G32Result G32Spam(GameConfig cfg, string mapName, string bot, double secs, uint seed, bool on)
    {
        SurvivalSim sim;
        SurvivalMap map;
        var si = new ScriptedInput();
        Vec3? start = null;
        double startYaw = 0;
        if (mapName == "span")
        {
            map = CampaignMapFrom("Design/levels/n30-span.json", cfg, out var json);
            LevelDef level = LevelDef.From(json);
            sim = G31Sim(cfg, map, true, si);
            sim.n32Override = new N32Blocks { grapple32 = on };
            G31Warm(sim, si, seed);
            sim.campaign?.SetDoorFree29(true);
            foreach (RoomDef rr in level.rooms)
            {
                Vec3 eye = new Vec3(rr.spawn.x, rr.spawn.y + 1.6, rr.spawn.z);
                foreach (Vec3 n in map.grappleNodes)
                    if (Vec3.DistanceTo(eye, n) < 35 && G31Sees(sim, eye, n)) { start = rr.spawn; startYaw = rr.spawnYawDeg; break; }
                if (start.HasValue) break;
            }
            if (!start.HasValue) return null;
            sim.Respawn(start.Value, startYaw);
        }
        else
        {
            map = Map();
            sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.n32Override = new N32Blocks { grapple32 = on };
            sim.UseInput(si);
            sim.StartRun(seed, null, null);
            if (sim.arcline21 == null) return null;
            sim.arcline21.Own(true);
            sim.run.Ready(); // round 1 now (enemies to zip to)
        }
        if (sim.arcline21 != null && !sim.arcline21.Owned) sim.arcline21.Own(true);
        for (int s = 0; s < 30; s++) G32Tick(sim, si, null, false, false, false);
        var r = new G32Result();
        double hz = cfg.movement.loop.simHz;
        int max = MovementMath.StepsFor(secs, 1 / hz);
        int fires0 = 0, step = 0;
        bool wasCancel = false;
        int cancelWatch = 0;
        sim.mover.Events += e =>
        {
            if (e.type == MoverEventType.RopeFire) { r.fires++; r.pullTimes.Add(step / hz); }
            if (e.type == MoverEventType.RopeRelease) { r.releases++; if (e.reason == "jump" || e.reason == "dash" || e.reason == "letGo") wasCancel = true; }
        };
        int kills0 = 0;
        sim.events.On<KillEvent>("kill", e => { if (e.kind != "boss") kills0++; });
        int pulls0 = sim.grappleTap31.pulls, can0 = sim.grappleTap31.cancels;
        int holdLeft = 0, tapUp = 0, pullAge = 0;
        Vec3? aim = null;
        for (step = 0; step < max; step++)
        {
            Rope rope = sim.mover.rope;
            bool press = false, held = false, jump = false;
            if (rope.phase == RopePhase.None && holdLeft == 0 && tapUp == 0)
            {
                aim = G32Target(sim, rope);
                if (aim.HasValue && rope.cooldownSteps == 0)
                {
                    press = held = true;
                    if (bot == "hold") holdLeft = MovementMath.StepsFor(0.6, 1 / hz);
                    else tapUp = 1;
                }
            }
            else if (holdLeft > 0) { held = true; holdLeft--; }
            else if (tapUp > 0) tapUp--;
            if (sim.grappleTap31.Pulling) pullAge++; else pullAge = 0;
            if (bot == "cancel" && pullAge == MovementMath.StepsFor(0.2, 1 / hz)) jump = true;
            if (rope.phase == RopePhase.None && holdLeft == 0 && !press) aim = G32Target(sim, rope) ?? aim;
            G32Tick(sim, si, aim, held, press, jump);
            double sp = sim.mover.velocity.Length;
            r.speedSum += sp;
            r.speedMax = Math.Max(r.speedMax, sp);
            if (wasCancel) { cancelWatch = MovementMath.StepsFor(0.25, 1 / hz); wasCancel = false; }
            if (cancelWatch > 0) { cancelWatch--; r.cancelSpeedMax = Math.Max(r.cancelSpeedMax, sp); }
            if (!sim.mover.grounded) r.airSteps++;
            r.steps++;
        }
        r.pulls = sim.grappleTap31.pulls - pulls0;
        r.cancels = sim.grappleTap31.cancels - can0;
        r.kills32 = kills0;
        G32Extra(sim, r);
        _ = fires0;
        return r;
    }

    /// <summary>D-3 on the survival map before round 1 (no enemies): from each of 16 yaws at +8 degrees, the first spot whose pick
    /// (Arcline21.Peek) is a bare SURFACE in reach; a TAP there (press, up after 2 steps) then, 1.2 s later, a HOLD (0.6 s). Rows on: the tap is
    /// refused (no pull, the charge given back) and the hold swings (the rope attaches, one charge spent). Rows off: the tap pulls.</summary>
    static string G32Wall(GameConfig cfg, uint seed, bool on, out bool ok)
    {
        ok = false;
        var si = new ScriptedInput();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n32Override = new N32Blocks { grapple32 = on };
        sim.UseInput(si);
        sim.StartRun(seed, null, null);
        if (sim.arcline21 == null) return "no ARCLINE";
        sim.arcline21.Own(true);
        for (int s = 0; s < 30; s++) G32Tick(sim, si, null, false, false, false);
        double yawW = 0, pitchW = 8 * Math.PI / 180;
        bool found = false;
        for (int k = 0; k < 16 && !found; k++)
        {
            double yaw = k * Math.PI / 8;
            if (sim.arcline21.Peek(new InputFrame { yaw = yaw, pitch = pitchW }, out Vec3 _) == Arcline21Kind.Surface) { yawW = yaw; found = true; }
        }
        if (!found) return "no bare surface in reach from the spawn";
        int pulls0 = sim.grappleTap31.pulls;
        bool attachedTap = false, attachedHold = false;
        sim.mover.Events += e => { if (e.type == MoverEventType.RopeAttach) { if (sim.mover.rope.zip) attachedTap = true; else attachedHold = true; } };
        si.SetLook(yawW, pitchW);
        for (int s = 0; s < 144; s++) G32Tick(sim, si, null, s < 2, s == 0, false);
        int tapPulls = sim.grappleTap31.pulls - pulls0;
        int chargesAfterTap = sim.grappleCharges32;
        si.SetLook(yawW, pitchW);
        bool held = false;
        for (int s = 0; s < 72; s++)
        {
            G32Tick(sim, si, null, true, s == 0, false);
            if (sim.mover.rope.phase == RopePhase.Attached && !sim.mover.rope.zip) held = true;
        }
        int chargesAfterHold = sim.grappleCharges32;
        // The kill refill: a boss kill gives nothing, a husk kill gives one back (the "kill" event as Combat / BossKit emit it).
        sim.events.Emit("kill", new KillEvent { kind = "boss", weapon = "boss" });
        int afterBoss = sim.grappleCharges32;
        sim.events.Emit("kill", new KillEvent { kind = "husk", weapon = "pistol" });
        int afterHusk = sim.grappleCharges32;
        ok = on ? tapPulls == 0 && sim.grappleTap31.refused == 1 && chargesAfterTap == 2 && held && chargesAfterHold == 1 && afterBoss == 1 && afterHusk == 2 : tapPulls == 1;
        return $"tap at a bare wall: pulls {tapPulls}, refused {sim.grappleTap31.refused}, charges after {(on ? chargesAfterTap.ToString() : "-")}; hold at it: swing attached {held}, charges after {(on ? chargesAfterHold.ToString() : "-")}; then a boss kill -> {(on ? afterBoss.ToString() : "-")}, a husk kill -> {(on ? afterHusk.ToString() : "-")} => {(ok ? "as designed" : "NOT as designed")}";
    }

    /// <summary>Rows-on counters (SurvivalSim.Grapple32.cs): charges spent, given back, refilled by kills, presses cleared, cancels clamped.</summary>
    static void G32Extra(SurvivalSim sim, G32Result r)
    {
        r.refunds = sim.g32Refunds;
        if (!sim.Grapple32On) return;
        r.extra = $"; charges spent {sim.g32Spent}, given back {sim.g32Refunds} (refused wall taps {sim.grappleTap31.refused}), kill refills {sim.g32KillRefills}, presses cleared {sim.g32Empty}, cancels clamped {sim.g32Clamps}, charges at the end {sim.grappleCharges32}";
    }

    /// <summary>The nearest target: a node in range (60 m) with a clear line and at least 4 m away (not the last node), else the nearest big enemy
    /// in range in sight, else the wall direction (16 yaws at +8 degrees pitch) whose hit is farthest under 38 m.</summary>
    static Vec3? G32Target(SurvivalSim sim, Rope rope)
    {
        Vec3 eye = sim.Eye;
        Vec3? best = null;
        double bd = double.MaxValue;
        for (int i = 0; i < sim.map.grappleNodes.Count; i++)
        {
            Vec3 n = sim.map.grappleNodes[i];
            double d = Vec3.DistanceTo(eye, n);
            if (d < 4 || d > 60 || i + 1 == rope.lastNode || !G31Sees(sim, eye, n)) continue;
            if (d < bd) { bd = d; best = n; }
        }
        if (best.HasValue) return best;
        foreach (Enemy e in sim.enemies.all)
        {
            if (!e.alive || (sim.arcline21 != null && sim.arcline21.IsPullKind(e.kind))) continue;
            Vec3 a = e.AimPoint;
            double d = Vec3.DistanceTo(eye, a);
            if (d < 3 || d > 60 || !G31Sees(sim, eye, a)) continue;
            if (d < bd) { bd = d; best = a; }
        }
        if (best.HasValue) return best;
        double far = 0;
        for (int k = 0; k < 16; k++)
        {
            double yaw = k * Math.PI / 8;
            Vec3 dir = GrappleAim.AimDirection(yaw, 8 * Math.PI / 180);
            if (sim.world.RayIntersect(eye, dir, out RayHit h) && h.distance <= 38 && h.distance > far) { far = h.distance; best = eye + dir * h.distance; }
        }
        return best;
    }

    /// <summary>One step with the look at `aim`, the grapple button and a jump; invulnerable, full health, no hitstop.</summary>
    static void G32Tick(SurvivalSim sim, ScriptedInput si, Vec3? aim, bool held, bool pressed, bool jump)
    {
        if (aim.HasValue)
        {
            (double yaw, double pitch) = ArclineLook(sim.Eye, aim.Value);
            si.SetLook(yaw, pitch);
        }
        si.Set(new InputFrame { grapple = new Button { held = held, pressed = pressed }, jump = new Button { held = jump, pressed = jump } }, new CombatInput { weapon = -1 });
        sim.health.hp = sim.health.max;
        sim.mover.invulnerable = true;
        sim.hitstopLeft = 0;
        sim.Step();
    }
}
