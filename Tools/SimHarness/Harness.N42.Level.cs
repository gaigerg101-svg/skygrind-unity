// NIGHT42 builder L1 (foundry42, teach42; NIGHT42.md L1, GG G1 / G6): level 1's checks, declared and called by the lead's Program.cs (N42Level).
// Every number is measured on the real sim (the gate checker's rig: SurvivalSim.Headless's Mover in the plan's box world, ride40 forced on as
// `n38 gates --ride40`; or a campaign run of the level).
//   n42level sweep <plan.json> [--only <gateId>] [--trace]
//       G1's forgiveness number: the share of a PERSON-LIKE sweep of ride tries that cross each gate. The gate checker's ride tries (`n38 gates`)
//       are its proof that the ride is needed; most of them are edge probes (a ride held to one wall's end, the release N m before the landing).
//       This sweep is a person riding the chain: the run-up on three lines (the gate's own line next to W1, and 1.6 m / 3.2 m toward the lane's
//       middle: the room's spawn line), the jump at the lip held into the ride, each wall ridden T s (0.3-1.4 s) then let go (the jump-off) and
//       the jump pressed again 0.25 s later held into the next wall (N38Run's held ride with releaseAfter = T), the look pitched 0 / +20 while
//       riding and -20 / 0 / +20 / +40 at each release; plus the checker's wall-end chain (each wall held to its end). Prints per gate: the
//       crossings of all tries and of the tries where a ride started, by T, and where the misses landed. Exit 0 always (a measurement).
//   n42level walk [--off] [--seeds n] [--first n] [--from ROOM] [--to ROOM] [--cap s]
//       n41lv walk foundry's pilot and start-room rule on n42-foundry with foundry42 forced on (and foundry41, its levelFrom's row; --off: the
//       design's blocks on n41-foundry, the control): per room cleared k of n, the median, deaths, the pilot's gate crossings and pit falls.
//   n42level teach
//       G6: the lines a level shows in its first 60 s (see Harness.N42.Teach.cs).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N42Level(string cmd, string[] args, ref int code)
    {
        if (cmd != "n42level") return;
        string what = args.Length > 1 ? args[1] : "";
        if (what == "sweep") { code = N42LvSweep(args); return; }
        if (what == "walk") { code = N42LvWalk(args); return; }
        if (what == "teach") { code = N42LvTeach(args); return; }
        Console.WriteLine("n42level: sweep <plan.json> [--only <gateId>] | walk [--off] [--seeds n] [--first n] [--from ROOM] [--to ROOM] [--cap s] | teach");
        code = 2;
    }

    static partial void N42LvTeachImpl(string[] args, ref int code);
    static int N42LvTeach(string[] args) { int c = 2; N42LvTeachImpl(args, ref c); return c; }

    static int N42LvSweep(string[] args)
    {
        string path = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : null;
        if (path == null) { Console.WriteLine("n42level sweep <plan.json> [--only <gateId>]"); return 2; }
        int oi = Array.IndexOf(args, "--only");
        string only = oi >= 0 && oi + 1 < args.Length ? args[oi + 1] : null;
        bool trace = Array.IndexOf(args, "--trace") >= 0;
        GameConfig cfg = Config();
        N38Plan plan = N38LoadPlan(path);
        bool was40 = N38GatesRide40;
        N38GatesRide40 = true; // as `n38 gates --ride40`: ride40 is on in the game (design.n40.ride40)
        N38Rig rig = N38NewRig(cfg, false);
        N38GatesRide40 = was40;
        double r = cfg.movement.body.capsuleRadius;
        var gates = plan.solids.Where(s => s.gate != null && s.gate.atom == "ride").Select(s => s.gate).Where(g => only == null || g.id == only).ToList();
        double[] ts = { 0.3, 0.45, 0.6, 0.75, 0.9, 1.05, 1.2, 1.4 };
        double[] ridePitch = { 0, 20 };
        double[] relPitch = { -20, 0, 20, 40 };
        double[] lines = { 0, 1.6, 3.2 };
        Console.WriteLine($"n42level sweep {plan.id}: {gates.Count} ride gate(s); per gate {lines.Length} lines x ({ts.Length} T x {ridePitch.Length} ride pitches x {relPitch.Length} release pitches + 9 wall-end chains); ride40 forced on");
        int allN = 0, allX = 0, allRode = 0, allRodeX = 0;
        foreach (N38Gate g in gates)
        {
            N38Solid sol = g.solid;
            Vec3 flat = new Vec3(g.to.x - g.from.x, 0, g.to.z - g.from.z);
            Vec3 dir = flat / flat.Length;
            double top = sol.max.y;
            N38Arena arena = N38BuildArena(plan, g.from - dir * g.runUp, g.to, 45, null);
            Vec3 perp = new Vec3(-dir.z, 0, dir.x);
            int n = 0, x = 0, rode = 0, rodeX = 0;
            var byT = new SortedDictionary<double, int[]>();
            var misses = new SortedDictionary<int, int>(); // landing prog bucket (5 m) -> count, for the tries that rode and missed
            foreach (double off in lines)
            {
                Vec3 fromL = g.from + perp * off;
                double edgeL = N38Edge(fromL, dir, sol.min, sol.max);
                if (double.IsInfinity(edgeL) || !N38Standable(plan, fromL - dir * g.runUp, r)) { Console.WriteLine($"  {g.id}: line {off:+0.0} skipped (no run-up or it misses the landing)"); continue; }
                Vec3 O = fromL + N38Off, start = fromL - dir * g.runUp + new Vec3(0, 0.05, 0) + N38Off;
                var tries = new List<(N38Try t, double key)>();
                foreach (double T in ts)
                    foreach (double rp in ridePitch)
                        foreach (double p in relPitch)
                            tries.Add((new N38Try { name = $"person T{T:0.00} ride p{rp:0} rel p{p:0}", atoms = new HashSet<string> { "jump", "ride", "double" }, rideSide = -1, releaseAfter = T, ridePitchDeg = rp, releasePitchDeg = p }, T));
                foreach (double look in new[] { 0.5, 1.5, 3.0 })
                    foreach (double p in new[] { -20.0, 0, 30 })
                        tries.Add((new N38Try { name = $"wall-end {look:0.0}m p{p:0}", atoms = new HashSet<string> { "jump", "ride", "double" }, rideSide = -1, releaseLeft = double.NaN, chainLook = look, releasePitchDeg = p }, 99));
                foreach (var (t, key) in tries)
                {
                    N38Trace tr = N38Run(rig, arena, start, O, dir, t, top, edgeL, 9);
                    bool crossed = tr.landed && !tr.dropped && tr.landY >= top - 0.1 && tr.landProg >= edgeL - r;
                    n++; if (crossed) x++;
                    if (tr.rides > 0) { rode++; if (crossed) rodeX++; else { int bkt = (int)Math.Floor(tr.landProg / 5) * 5; misses.TryGetValue(bkt, out int c0); misses[bkt] = c0 + 1; } }
                    if (!byT.TryGetValue(key, out int[] a)) byT[key] = a = new int[2];
                    a[0]++; if (crossed) a[1]++;
                    if (trace) Console.WriteLine($"    {g.id} line {off:+0.0} {t.name,-36} {(crossed ? "CROSSES" : "no     ")} land prog {tr.landProg:0.00} y {tr.landY:0.00} rides {tr.rides}");
                }
            }
            string Pc(int a, int b) => b == 0 ? "-" : (100.0 * a / b).ToString("0.0", CultureInfo.InvariantCulture) + " %";
            Console.WriteLine($"  {g.id}: gap {N38Edge(g.from, dir, sol.min, sol.max):0.0} m; crosses {x}/{n} ({Pc(x, n)}); of the tries where a ride started {rodeX}/{rode} ({Pc(rodeX, rode)})");
            Console.WriteLine("    by T (s on each wall): " + string.Join(", ", byT.Select(kv => $"{(kv.Key == 99 ? "wall-end" : kv.Key.ToString("0.00", CultureInfo.InvariantCulture))} {kv.Value[1]}/{kv.Value[0]}")));
            Console.WriteLine("    the rides that missed landed at prog (m past the takeoff, 5 m buckets): " + string.Join(", ", misses.Select(kv => $"{kv.Key}-{kv.Key + 5} x{kv.Value}")));
            allN += n; allX += x; allRode += rode; allRodeX += rodeX;
        }
        Console.WriteLine($"n42level sweep {plan.id} SUMMARY: crosses {allX}/{allN} ({(allN == 0 ? 0 : 100.0 * allX / allN):0.0} %); of the tries where a ride started {allRodeX}/{allRode} ({(allRode == 0 ? 0 : 100.0 * allRodeX / allRode):0.0} %)");
        return 0;
    }

    /// <summary>n42level walk [--off] ...: N41LvFoundryWalk's loop (Harness.N41.Levels.cs) on n42-foundry, foundry42 forced on (+ foundry41, its levelFrom's
    /// row, so every older system reads n42-foundry -> n41-foundry -> n38-foundry); --off: n41-foundry with the design's NIGHT42 blocks (the control).</summary>
    static int N42LvWalk(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        int seeds = 10, first = 1;
        double cap = 600;
        string fromId = "b1", toId = "b2";
        bool traceWalk = false, arena42 = Array.IndexOf(args, "--arena42") >= 0; // --arena42: K1's arena42 forced on too (level 1's grinder / cover in b1)
        double traceUntil = 0;
        for (int i = 2; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--first") first = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--trace") { traceWalk = true; traceUntil = double.Parse(args[i + 1], CultureInfo.InvariantCulture); } // the first seed's positions in the last room, every 0.25 s, to this game time
            if (args[i] == "--cap") cap = double.Parse(args[i + 1], CultureInfo.InvariantCulture);
            if (args[i] == "--from") fromId = args[i + 1];
            if (args[i] == "--to") toId = args[i + 1];
        }
        GameConfig cfg = Config();
        int bit42 = Array.IndexOf(N42Blocks.Names, "foundry42");
        string id = off ? cfg.Str("design.n42.foundry42.levelFrom") : cfg.Str("design.n42.foundry42.levelTo");
        var (bit41, _, _) = N41LvRow(cfg, "foundry");
        string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int a = level.rooms.FindIndex(q => q.id == fromId), b = level.rooms.FindIndex(q => q.id == toId);
        if (a < 0 || b < a) { Console.WriteLine($"n42level walk: rooms {fromId}..{toId} not in {id}"); return 2; }
        var clears = new Dictionary<int, List<double>>();
        var deaths = new int[level.rooms.Count];
        var crossedAll = new Dictionary<string, int>();
        int runsDone = 0, rides = 0, pitFalls = 0;
        string baseId = "";
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.GoalProvider = () =>
            {
                Campaign24 c = sim.campaign;
                LevelDef lv = sim.campaignLevel;
                if (c == null || c.room < 0) return lv.rooms[a].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            var pilot = new Walk38Pilot(sim, ap);
            sim.UseInput(pilot);
            sim.n38Override = N38Blocks.FromBits(N38Blocks.From(cfg).Bits | 2); // foundry38 on (as n38f walk / n41lv walk)
            sim.n41Override = N41LvBlocks(cfg, bit41, true); // foundry41 on: n41-foundry (and n42-foundry through N42BaseId) reads as n38-foundry
            N42Blocks b42 = N42Blocks.From(cfg);
            if (off) sim.n42Override = b42;
            else sim.n42Override = N42Blocks.FromBits(b42.Bits | (1 << bit42) | (arena42 ? 1 << Array.IndexOf(N42Blocks.Names, "arena42") : 0));
            int st = 0;
            var enterStep = new Dictionary<int, int>();
            sim.events.On<RoomEvent>("roomEnter", e => { if (!enterStep.ContainsKey(e.room)) enterStep[e.room] = st; });
            sim.events.On<RoomEvent>("roomClear", e =>
            {
                if (e.room < a || e.room > b) return;
                if (!clears.TryGetValue(e.room, out var l)) clears[e.room] = l = new List<double>();
                l.Add(enterStep.TryGetValue(e.room, out int es) ? (st - es) * sim.step : e.time);
            });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room >= a && e.room <= b) deaths[e.room]++; });
            sim.StartRun(seed, null, null);
            baseId = sim.N41BaseId(sim.campaignLevel != null ? sim.campaignLevel.id : "?");
            string runId = sim.campaignLevel != null ? sim.campaignLevel.id : "?";
            if (s == 0) Console.WriteLine($"n42level walk: the campaign loaded {runId} (N41BaseId -> {baseId}); foundry42 {sim.n42.foundry42}, arena42 {sim.n42.arena42} (the sim's grinders {(sim.arena42 != null ? sim.arena42.grinders.Count : 0)}, cover {(sim.arena42 != null ? sim.arena42.cover.Count : 0)})");
            int max = MovementMath.StepsFor(cap, sim.step);
            bool done = false, placed = a == 0, doorFreeOn = a > 0;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (!placed && sim.campaign != null && sim.campaign.room >= 0)
                {
                    sim.campaign.SetDoorFree29(true);
                    sim.Respawn(level.rooms[a].spawn, level.rooms[a].spawnYawDeg);
                    placed = true;
                }
                if (placed && doorFreeOn && sim.campaign != null && sim.campaign.room == a) { sim.campaign.SetDoorFree29(false); doorFreeOn = false; }
                if (clears.ContainsKey(b) && clears[b].Count > runsDone) { done = true; break; }
                if (traceWalk && s == 0 && st % 30 == 0 && sim.campaign != null && sim.campaign.room == b && st * sim.step < traceUntil)
                {
                    Vec3 tp = sim.mover.position;
                    Console.WriteLine($"    {st * sim.step:0.00} s ({tp.x:0.0}, {tp.y:0.0}, {tp.z:0.0}) lz {level.rooms[b].z1 - tp.z:0.0} {sim.mover.state} falls {pilot.pitFalls}");
                }
            }
            foreach (var kv in pilot.crossed) { crossedAll.TryGetValue(kv.Key, out int nn); crossedAll[kv.Key] = nn + kv.Value; }
            rides += pilot.rides;
            pitFalls += pilot.pitFalls;
            if (done) runsDone++;
            else
            {
                Vec3 p = sim.mover.position;
                Campaign24 c = sim.campaign;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0.0} s in {(c != null && c.room >= 0 ? level.rooms[c.room].id : "?")} {c?.phase} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), state {sim.mover.state}, alive {sim.enemies.AliveCount}, pit falls {pilot.pitFalls}");
            }
            ap.Detach();
        }
        string tag = $"n42level walk {id}{(off ? " (--off: the control)" : $" (foundry42 on; N41BaseId -> {baseId})")}";
        for (int q = a; q <= b; q++)
        {
            var l = clears.TryGetValue(q, out var xx) ? xx : new List<double>();
            l.Sort();
            string med = l.Count == 0 ? "-" : l[l.Count / 2].ToString("0.0", CultureInfo.InvariantCulture);
            crossedAll.TryGetValue(level.rooms[q].id, out int cr);
            Console.WriteLine($"{tag} {level.rooms[q].id}: cleared {l.Count}/{seeds}, median {med} s, deaths {deaths[q]}, gate crossings by the pilot {cr}");
        }
        Console.WriteLine($"{tag} SUMMARY: {runsDone}/{seeds} runs through {fromId}..{toId}; {rides} rides started, pit falls {pitFalls}");
        return runsDone == seeds ? 0 : 1;
    }
}
