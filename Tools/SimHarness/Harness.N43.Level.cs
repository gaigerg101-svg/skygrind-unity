// NIGHT43 builder L1 (foundry43; NIGHT43.md L1: G1 THE GANTRY, G2 THE FLUE, G4 PIERCE, G11): level 1's checks, declared and called by the lead's
// Program.cs (N43Level). Every number is measured on the real sim (SurvivalSim.Headless's Mover in the plan's box world, as the gate checker's rig,
// or a campaign run of the level). The person controllers are the walk pilot's (Assets/Scripts/Game/Bot/Walk43Foundry.cs: Chain43, Flue43) with
// their numbers jittered per person (Mulberry32, seeded).
//   n43level gates [--plan p] [--m1]
//       the gate checker (`n38 gates <plan> --ride40`) on n43-foundry's plan; --m1: chimney43 / ride43 forced on in the rig (needs M1's
//       SurvivalSim.Force43 hook; without it the flag is printed as not available).
//   n43level cross [--plan p] [--persons n] [--m1] [--only g1|g2] [--trace k]
//       G1's number: per gate, n persons (jittered Chain43: release distance, re-press delay, release pitch, ride pitch, lean, the dash's timing,
//       20 % press the dash with the stick still into the wall) each get two tries: the share across on the FIRST try and within TWO tries; on
//       gate 2 the crossings that dashed vs not; the flight time between walls (M-5: the time off a wall between two rides) median / max.
//   n43level flue [--plan p] [--persons n] [--taps] [--m1]
//       G2's number: n jittered climbers (Flue43: held rides with the look up; --taps: a press on every wall contact) from THE FLUE's floor: the
//       share that reach the deck, the median top height and time.
//   n43level walk [--off] [--seeds n] [--first n] [--from ROOM] [--to ROOM] [--cap s] [--rooms43] [--m1]
//       the walk pilot on n43-foundry with foundry43 forced on (and foundry42 / 41, its levelFrom's rows; --off: n42-foundry, the control);
//       --rooms43: E1's row on too (fallReturn43, afterLesson43). Per room cleared k of n, the median, deaths, gate crossings, pit falls,
//       fallReturn43s, the teach35 lines shown (l1.dash: "check it shows").
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    const string N43Plan = "Design/levels/n43/foundry.plan.json";

    static partial void N43Level(string cmd, string[] args, ref int code)
    {
        if (cmd != "n43level") return;
        string what = args.Length > 1 ? args[1] : "";
        if (what == "gates") { code = N43LvGates(args); return; }
        if (what == "cross") { code = N43LvCross(args); return; }
        if (what == "flue") { code = N43LvFlue(args); return; }
        if (what == "walk") { code = N43LvWalk(args); return; }
        Console.WriteLine("n43level: gates [--plan p] [--m1] | cross [--persons n] [--only g1|g2] [--m1] | flue [--persons n] [--taps] [--m1] | walk [--off] [--seeds n] [--from R] [--to R] [--rooms43] [--m1]");
        code = 2;
    }

    static string N43LevelArg(string[] args, string k, string d) { int i = Array.IndexOf(args, k); return i >= 0 && i + 1 < args.Length ? args[i + 1] : d; }
    static bool N43Flag(string[] args, string k) => Array.IndexOf(args, k) >= 0;

    /// <summary>The NIGHT43 blocks a harness run of this file uses: the design's, with foundry43 on, rooms43 when asked, chimney43 / ride43 with --m1.</summary>
    static N43Blocks N43LvBlocks(GameConfig cfg, bool rooms43, bool m1, bool foundry = true)
    {
        N43Blocks b = N43Blocks.ForceOff ? N43Blocks.Off : N43Blocks.From(cfg);
        if (foundry) b.foundry43 = true;
        if (rooms43) b.rooms43 = true;
        if (m1) { b.chimney43 = true; b.ride43 = true; }
        return b;
    }

    static int N43LvGates(string[] args)
    {
        string plan = N43LevelArg(args, "--plan", N43Plan);
        if (N43Flag(args, "--m1")) Console.WriteLine("n43level gates: --m1: the gate checker's rig (N38NewRig) takes no NIGHT43 blocks; M1's chimney43 / ride43 reach it only through a static force of M1's (not in this branch): run as plain --ride40");
        var a = new List<string> { "n38", "gates", plan, "--ride40" };
        string only = N43LevelArg(args, "--only", null);
        if (only != null) { a.Add("--only"); a.Add(only); }
        return N38GatesCommand(a.ToArray());
    }

    /// <summary>The gate checker's rig (N38NewRig's settings) with this run's NIGHT43 blocks in the sim (n43Override before StartRun).</summary>
    static N38Rig N43Rig(GameConfig cfg, N43Blocks b43)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        N40Blocks b40 = N40Blocks.ForceOff ? N40Blocks.Off : N40Blocks.From(cfg);
        b40.ride40 = true;
        sim.n40Override = b40;
        sim.n43Override = b43;
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        sim.StartRun(4301, null, null);
        sim.husks.frozen = true;
        sim.map.pads.Clear();
        var rig = new N38Rig { sim = sim, inp = (ScriptedInput)sim.input, dt = cfg.Dt };
        sim.mover.Events += e => rig.log.Add(e);
        if (!sim.loadout.owned.Contains("shotgun")) sim.loadout.AddWeapon("shotgun");
        return rig;
    }

    /// <summary>A person's jittered chain (Walk43Foundry.cs's Chain43 with its numbers drawn from r).</summary>
    static Chain43 N43Person(Mulberry32 r, Wall43[] walls, double t, double plane, double z1)
    {
        double U(double a, double b) => a + (b - a) * r.Next();
        return new Chain43
        {
            walls = walls, t = t, gap = 37, plane = plane, cx = 0, z1 = z1,
            releaseBefore = U(1.5, 4.5), lastReleaseBefore = U(1.5, 3.5), releasePitch = U(0.2, 0.7), ridePitch = r.Next() < 0.5 ? 0 : 0.17,
            dashBefore = U(0.3, 1.5), lean = U(0.05, 0.25), stick = U(0.3, 0.7), repressSteps = 12 + (int)(r.Next() * 25), dashStickInto = r.Next() < 0.2,
        };
    }

    sealed class N43Try { public bool across, fell; public int rides, dashes, steps; public List<double> flights = new List<double>(); public double endLz, endY; }

    /// <summary>One chain attempt in the arena: the Mover set on the run-up line 6 m before the lip, settled, then driven by the controller.</summary>
    static N43Try N43RunChain(N38Rig rig, N38Arena arena, Chain43 c, double maxS, bool trace)
    {
        var res = new N43Try();
        SurvivalSim sim = rig.sim;
        Mover m = sim.mover;
        m.World = arena.world;
        m.SetNoRide38(arena.noride.Count > 0 ? arena.noride.ConvertAll(s => new Ride38.Box { min = s.min + N38Off, max = s.max + N38Off }).ToArray() : null);
        double sx = c.cx + c.walls[0].side * (c.plane - 0.9), sz = c.z1 - (c.t - 5);
        m.Reset(sx + N38Off.x, 0.05, sz);
        for (int k = 0; k < 24; k++) N38Tick(rig, 0, 0, new InputFrame(), new CombatInput { weapon = -1 });
        c.Reset();
        int max = MovementMath.StepsFor(maxS, rig.dt), offWall = -1;
        bool wasRiding = false;
        for (int i = 0; i < max; i++)
        {
            Vec3 p = m.position - N38Off;
            Chain43.R r = c.Drive(p, m.state, m.grounded, out InputFrame f);
            if (r == Chain43.R.Across) { res.across = true; break; }
            if (r == Chain43.R.Fell) { res.fell = true; break; }
            N38Tick(rig, f.yaw, f.pitch, f, new CombatInput { weapon = -1 });
            bool riding = m.state == MoveState.WallRide;
            if (wasRiding && !riding) offWall = i;
            if (!wasRiding && riding && offWall >= 0) { res.flights.Add((i - offWall) * rig.dt); offWall = -1; }
            wasRiding = riding;
            res.steps = i;
            if (trace && i % 12 == 0) Console.WriteLine($"      {i * rig.dt:0.00} s lz {c.z1 - p.z:0.0} x {p.x:0.00} y {p.y:0.00} {m.state} k {c.k} jump {(f.jump.pressed ? "PRESS" : f.jump.held ? "held" : "-")}{(f.dash.pressed ? " DASH" : "")}");
        }
        Vec3 e = m.position - N38Off;
        res.endLz = c.z1 - e.z;
        res.endY = e.y;
        res.rides = c.rides;
        res.dashes = c.dashes;
        return res;
    }

    static int N43LvCross(string[] args)
    {
        string path = N43LevelArg(args, "--plan", N43Plan);
        int persons = int.Parse(N43LevelArg(args, "--persons", "60"), CultureInfo.InvariantCulture);
        string only = N43LevelArg(args, "--only", null);
        int traceK = int.Parse(N43LevelArg(args, "--trace", "-1"), CultureInfo.InvariantCulture);
        bool m1 = N43Flag(args, "--m1");
        GameConfig cfg = Config();
        N38Plan plan = N38LoadPlan(path);
        N38Rig rig = N43Rig(cfg, N43LvBlocks(cfg, false, m1));
        Console.WriteLine($"n43level cross {plan.id}: {persons} persons x 2 tries per gate; ride40 on; chimney43 {rig.sim.n43.chimney43}, ride43 {rig.sim.n43.ride43} (M1's rows: {(m1 ? "forced" : "the design's")})");
        const double z1 = -184; // b2's south edge (lz 0)
        int code = 0;
        foreach (var (gid, t) in new[] { ("g1", 8.0), ("g2", 57.0) })
        {
            if (only != null && only != gid) continue;
            Wall43[] walls = N43WallsFromPlan(plan, z1 - t, z1 - (t + 37), out double plane);
            Wall43[] pilotW = Walk43GateWalls(gid == "g1" ? 1 : 2);
            string Ws(Wall43[] a) => string.Join(" ", a.Select(w => $"{(w.side < 0 ? "W" : "E")}{w.from}..{w.to}{(w.dashAfter ? "+dash" : "")}"));
            Console.WriteLine($"  b2-{gid}: the plan's walls {Ws(walls)} (plane {plane}){(Ws(walls) == Ws(pilotW) ? "" : "; the walk pilot's table differs: " + Ws(pilotW))}");
            Vec3 a = new Vec3(0, 0, z1 - (t - 8)), b = new Vec3(0, 0, z1 - (t + 40));
            N38Arena arena = N38BuildArena(plan, a, b, 30, null);
            int first = 0, two = 0, dashX = 0, noDashX = 0, falls = 0;
            var flights = new List<double>();
            var missAt = new SortedDictionary<int, int>();
            for (int k = 0; k < persons; k++)
            {
                var r = new Mulberry32((uint)(4300 + k));
                bool ok2 = false;
                for (int tryN = 0; tryN < 2; tryN++)
                {
                    Chain43 c = N43Person(r, walls, t, plane, z1);
                    N43Try tr = N43RunChain(rig, arena, c, 12, k == traceK && tryN == 0);
                    if (N43Flag(args, "--verbose")) Console.WriteLine($"    {gid} person {k} try {tryN + 1}: {(tr.across ? "ACROSS" : "miss  ")} end lz {tr.endLz - t:0.0} y {tr.endY:0.00} rides {tr.rides} dashes {tr.dashes} | rel {c.releaseBefore:0.00} last {c.lastReleaseBefore:0.00} pitch {c.releasePitch:0.00} ride {c.ridePitch:0.00} dashB {c.dashBefore:0.00} into {c.dashStickInto} lean {c.lean:0.00} stick {c.stick:0.00} rep {c.repressSteps}");
                    if (tr.across) { ok2 = true; if (tryN == 0) first++; if (tr.dashes > 0) dashX++; else noDashX++; flights.AddRange(tr.flights); break; }
                    falls++;
                    int bk = (int)Math.Floor((tr.endLz - t) / 5) * 5;
                    missAt.TryGetValue(bk, out int n0);
                    missAt[bk] = n0 + 1;
                }
                if (ok2) two++;
            }
            flights.Sort();
            string Pc(int x) => (100.0 * x / persons).ToString("0.0", CultureInfo.InvariantCulture) + " %";
            string fl = flights.Count == 0 ? "-" : $"median {flights[flights.Count / 2]:0.00} s, max {flights[flights.Count - 1]:0.00} s ({flights.Count} hops)";
            Console.WriteLine($"  b2-{gid}: across on the first try {first}/{persons} ({Pc(first)}), within two tries {two}/{persons} ({Pc(two)}); crossings that dashed {dashX}, without a dash {noDashX}; flight between walls {fl}");
            Console.WriteLine("    the misses ended at (m past the lip, 5 m buckets): " + (missAt.Count == 0 ? "none" : string.Join(", ", missAt.Select(kv => $"{kv.Key}-{kv.Key + 5} x{kv.Value}"))));
        }
        return code;
    }

    /// <summary>A gate's walls read from the PLAN (b2's ride solids between the lip and the landing), so a probe plan (N43F_* knobs) is swept
    /// with its own walls; the walk pilot's tables (Walk43Foundry.cs) are the shipped plan's and are compared here.</summary>
    static Wall43[] N43WallsFromPlan(N38Plan plan, double lipZ, double landZ, out double plane)
    {
        var ws = plan.solids.Where(s => s.zone == "b2" && s.ride && s.max.z > landZ && s.min.z < lipZ + 7).OrderByDescending(s => s.max.z).ToList();
        var outW = new List<Wall43>();
        plane = 0;
        foreach (N38Solid s in ws)
        {
            int side = s.max.x <= 0 ? -1 : 1;
            plane = Math.Max(plane, side < 0 ? -s.max.x : s.min.x);
            outW.Add(new Wall43(side, Math.Round(lipZ - s.max.z, 2), Math.Round(lipZ - s.min.z, 2)));
        }
        for (int i = 0; i + 1 < outW.Count; i++)
            if (outW[i + 1].side == outW[i].side) { Wall43 w = outW[i]; w.dashAfter = true; outW[i] = w; }
        return outW.ToArray();
    }

    static Wall43[] Walk43GateWalls(int gate) => gate == 1 ? Walk38Pilot.Gate1Walls43 : Walk38Pilot.Gate2Walls43;

    static int N43LvFlue(string[] args)
    {
        string path = N43LevelArg(args, "--plan", N43Plan);
        int persons = int.Parse(N43LevelArg(args, "--persons", "40"), CultureInfo.InvariantCulture);
        bool taps = N43Flag(args, "--taps"), straight = N43Flag(args, "--straight"), m1 = N43Flag(args, "--m1");
        int traceK = int.Parse(N43LevelArg(args, "--trace", "-1"), CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        N38Plan plan = N38LoadPlan(path);
        N38Rig rig = N43Rig(cfg, N43LvBlocks(cfg, false, m1));
        Mover m = rig.sim.mover;
        const double z1 = -306; // a7's south edge
        N38Arena arena = N38BuildArena(plan, new Vec3(0, 0, -306), new Vec3(0, 0, -338), 12, null);
        m.World = arena.world;
        m.SetNoRide38(arena.noride.Count > 0 ? arena.noride.ConvertAll(s => new Ride38.Box { min = s.min + N38Off, max = s.max + N38Off }).ToArray() : null);
        N38Solid east = plan.solids.FirstOrDefault(s => s.zone == "a7" && s.ride && s.min.x > 0);
        double half = east != null ? east.min.x : 2.25; // the chimney's half lane from the plan (a probe plan has its own)
        double fFrom = east != null ? Math.Round(-306 - east.max.z, 2) : 5, fTo = east != null ? Math.Round(-306 - east.min.z, 2) : 25; // its lz span
        int top = 0;
        var tops = new List<double>();
        var times = new List<double>();
        var ridesL = new List<int>();
        for (int k = 0; k < persons; k++)
        {
            var r = new Mulberry32((uint)(4310 + k));
            double U(double a, double b) => a + (b - a) * r.Next();
            var fc = new Flue43 { cx = 0, z1 = z1, half = half, from = fFrom, to = fTo, taps = taps, straight = straight, entryAim = U(0.3, 1.3), ridePitch = U(0.4, 0.95), releasePitch = U(0.4, 0.95), releaseAfter = U(0.3, 0.6), lean = U(0.1, 0.3), jumpLead = U(0.6, 1.4), repressSteps = 10 + (int)(r.Next() * 20) };
            m.Reset(N38Off.x, 0.05, -307.5);
            for (int i = 0; i < 24; i++) N38Tick(rig, 0, 0, new InputFrame(), new CombatInput { weapon = -1 });
            int max = MovementMath.StepsFor(10, rig.dt), i2 = 0;
            bool reached = false;
            for (; i2 < max; i2++)
            {
                Vec3 p = m.position - N38Off;
                Flue43.R res = fc.Drive(p, m.state, m.wallNormal, m.grounded, rig.dt, m.touchingWall, out InputFrame f);
                if (res == Flue43.R.Top) { reached = true; break; }
                if (res == Flue43.R.Floor && i2 > 60) break; // fell back to the floor: this try is over
                N38Tick(rig, f.yaw, f.pitch, f, new CombatInput { weapon = -1 });
                if (k == traceK && i2 % 12 == 0) Console.WriteLine($"      {i2 * rig.dt:0.00} s lz {z1 - p.z:0.0} x {p.x:0.00} y {p.y:0.00} {m.state} jump {(f.jump.pressed ? "PRESS" : f.jump.held ? "held" : "-")}");
            }
            if (N43Flag(args, "--verbose")) { Vec3 e = m.position - N38Off; Console.WriteLine($"    climber {k}: {(reached ? "DECK" : "no  ")} top {fc.maxY:0.00} end lz {z1 - e.z:0.0} y {e.y:0.00} {m.state} rides {fc.rides} | ridePitch {fc.ridePitch:0.00} rel {fc.releasePitch:0.00} after {fc.releaseAfter:0.00} rep {fc.repressSteps}"); }
            if (reached) { top++; times.Add(i2 * rig.dt); }
            tops.Add(fc.maxY);
            ridesL.Add(fc.rides);
        }
        tops.Sort();
        times.Sort();
        Console.WriteLine($"n43level flue {plan.id} ({(straight ? "presses straight across from 2 m inside the mouth" : taps ? "taps: a press on every wall contact" : "held rides, the look up")}; chimney43 {rig.sim.n43.chimney43}, ride43 {rig.sim.n43.ride43}): "
            + $"reach the {8} m deck {top}/{persons} ({100.0 * top / persons:0.0} %); top height median {tops[tops.Count / 2]:0.00} m (min {tops[0]:0.00}, max {tops[tops.Count - 1]:0.00}); "
            + $"time to the deck median {(times.Count > 0 ? times[times.Count / 2].ToString("0.00", CultureInfo.InvariantCulture) : "-")} s; rides per climb median {ridesL.OrderBy(x => x).ElementAt(ridesL.Count / 2)}");
        return 0;
    }

    static int N43LvWalk(string[] args)
    {
        bool off = N43Flag(args, "--off"), rooms43 = N43Flag(args, "--rooms43"), m1 = N43Flag(args, "--m1");
        int seeds = int.Parse(N43LevelArg(args, "--seeds", "10"), CultureInfo.InvariantCulture), first = int.Parse(N43LevelArg(args, "--first", "1"), CultureInfo.InvariantCulture);
        double cap = double.Parse(N43LevelArg(args, "--cap", "600"), CultureInfo.InvariantCulture);
        string fromId = N43LevelArg(args, "--from", "b1"), toId = N43LevelArg(args, "--to", "a8"), traceRoom = N43LevelArg(args, "--trace", null);
        int traceN = 0;
        GameConfig cfg = Config();
        int bit42 = Array.IndexOf(N42Blocks.Names, "foundry42");
        string id = off ? cfg.Str("design.n43.foundry43.levelFrom") : cfg.Str("design.n43.foundry43.levelTo");
        var (bit41, _, _) = N41LvRow(cfg, "foundry");
        string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int a = level.rooms.FindIndex(q => q.id == fromId), b = level.rooms.FindIndex(q => q.id == toId);
        if (a < 0 || b < a) { Console.WriteLine($"n43level walk: rooms {fromId}..{toId} not in {id}"); return 2; }
        var clears = new Dictionary<int, List<double>>();
        var deaths = new int[level.rooms.Count];
        var crossedAll = new Dictionary<string, int>();
        var teach = new Dictionary<string, int>();
        int runsDone = 0, pitFalls = 0, fallReturns = 0;
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
            sim.n38Override = N38Blocks.FromBits(N38Blocks.From(cfg).Bits | 2); // foundry38 on (as n42level walk)
            sim.n41Override = N41LvBlocks(cfg, bit41, true); // foundry41 on
            N42Blocks b42 = N42Blocks.From(cfg);
            sim.n42Override = N42Blocks.FromBits(b42.Bits | (1 << bit42)); // foundry42 on (n43-foundry reads as n42-foundry -> n41 -> n38)
            sim.n43Override = off ? N43LvBlocks(cfg, rooms43, m1, false) : N43LvBlocks(cfg, rooms43, m1);
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
            sim.events.On<Teach35Event>("teach35", e => { teach.TryGetValue(e.id ?? "?", out int n0); teach[e.id ?? "?"] = n0 + 1; });
            sim.StartRun(seed, null, null);
            if (traceRoom != null && s == 0)
                sim.mover.Events += e =>
                {
                    if ((e.type == MoverEventType.WallRideEnd || e.type == MoverEventType.WallRideStart || e.type == MoverEventType.WallJump) && sim.campaign != null && sim.campaign.room >= 0 && level.rooms[sim.campaign.room].id == traceRoom && traceN < 120)
                        Console.WriteLine($"      {st * sim.step:0.00} s {e.type} {e.reason} at ({sim.mover.position.x:0.00}, {sim.mover.position.y:0.00}, {sim.mover.position.z:0.00})");
                };
            baseId = sim.N41BaseId(sim.campaignLevel != null ? sim.campaignLevel.id : "?");
            if (s == 0) Console.WriteLine($"n43level walk: the campaign loaded {sim.campaignLevel?.id} (N41BaseId -> {baseId}); foundry43 {sim.n43.foundry43}, rooms43 {sim.n43.rooms43}, chimney43 {sim.n43.chimney43}, ride43 {sim.n43.ride43}");
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
                if (traceRoom != null && s == 0 && st % 30 == 0 && sim.campaign != null && sim.campaign.room >= 0 && level.rooms[sim.campaign.room].id == traceRoom && traceN++ < 120)
                {
                    Vec3 tp = sim.mover.position;
                    Console.WriteLine($"    {st * sim.step:0.0} s {traceRoom} ({tp.x:0.00}, {tp.y:0.00}, {tp.z:0.00}) lz {level.rooms[sim.campaign.room].z1 - tp.z:0.0} {sim.mover.state} {sim.campaign.phase} alive {sim.enemies.AliveCount} hp {sim.health.hp:0}");
                }
            }
            foreach (var kv in pilot.crossed) { crossedAll.TryGetValue(kv.Key, out int nn); crossedAll[kv.Key] = nn + kv.Value; }
            pitFalls += pilot.pitFalls;
            fallReturns += pilot.returns43; // E1's fallReturn43s (the pilot counts the teleports back to a lip: E1's counter is not in this branch)
            if (done) runsDone++;
            else
            {
                Vec3 p = sim.mover.position;
                Campaign24 c = sim.campaign;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0.0} s in {(c != null && c.room >= 0 ? level.rooms[c.room].id : "?")} {c?.phase} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), state {sim.mover.state}, alive {sim.enemies.AliveCount}, pit falls {pilot.pitFalls}");
            }
            ap.Detach();
        }
        string tag = $"n43level walk {id}{(off ? " (--off: the control)" : " (foundry43 on)")}{(rooms43 ? " rooms43" : "")}{(m1 ? " m1" : "")}";
        for (int q = a; q <= b; q++)
        {
            var l = clears.TryGetValue(q, out var xx) ? xx : new List<double>();
            l.Sort();
            string med = l.Count == 0 ? "-" : l[l.Count / 2].ToString("0.0", CultureInfo.InvariantCulture);
            crossedAll.TryGetValue(level.rooms[q].id, out int cr);
            Console.WriteLine($"{tag} {level.rooms[q].id}: cleared {l.Count}/{seeds}, median {med} s, deaths {deaths[q]}, gate crossings by the pilot {cr}");
        }
        Console.WriteLine($"{tag} teach35 lines shown: " + (teach.Count == 0 ? "none" : string.Join(", ", teach.Select(kv => $"{kv.Key} x{kv.Value}"))));
        Console.WriteLine($"{tag} SUMMARY: {runsDone}/{seeds} runs through {fromId}..{toId}; pit falls {pitFalls}, fallReturn43s {fallReturns}");
        return runsDone == seeds ? 0 : 1;
    }
}
