// NIGHT40 builder R2 (NIGHT40.md R2): ride40's harness commands, declared and called by the lead's Program.cs (N40Ride).
//   n40ride measure   the ride TODAY (the design config as it is: wallride35, wallgrip36, ride39 on; ride38 off) against ride40 FORCED ON, on the
//                     real Mover in a box world (TriangleWorld: the web's collider, no PhysX): a wall 60 m tall and 420 m long, met in the air
//                     beside it at 8 m/s along it, feet 3 m up, pitch 0:
//     tap / hold x entry vy 0 (level) and 6 m/s (a jump's rise): the feet's height lost over 3 s from the attach (to the floor if the ride ended
//               and the feet landed), still riding at 3 s, the end, and the settle (the seconds from the attach until the vertical speed is
//               0 and stays 0 for 0.1 s). PASS (ride40): riding at 3 s, |height lost| <= 0.05 m, settle <= 0.1 s.
//     ends      ride40 on, a tap ride and a hold ride each ended by: the player (a tap ride's jump press: "jump"; a hold ride's release: "jump";
//               crouch: "drop"; the stick away: "away"), the wall's end (a 20 m wall: "lost", "corner", or "away" once wallride35 has turned the
//               box's outside corner and the stick, still forward, points off the end face), a landing (pitch -45 from 3 m: "land"), and
//               nothing in 30 s (still riding). PASS: each end as named.
//   n40ride walk [level-id ...] [--seeds N] [--first N] [--cap S] [--ride38] [--ride40] [--trace]
//               the walk pilot (Walk38Pilot: Walk29Pilot plus the n38 levels' gate scripts) through every playable room of each level (default: the
//               campaign's levels 1-5 with the design config) on the REAL sim, campaign mode, the design blocks as they are (--ride38 / --ride40:
//               that row forced on). Per room: cleared k of N, deaths (retries), falls; per level: the rides started and the ATTACH AUDIT: each
//               ride's start classified by the wall's face above the feet (rays toward the wall every 0.1 m up from the feet; the first miss) and
//               the ground below the feet (a ray down): "short" = the face reaches less than 2.0 m above the feet, "low" = ground within 1.0 m
//               below the feet; "either" counts a start once. The before for round 2's attach rules (the after must be 0).
// PASS lines print RED on a failure; the command exits 1 on any RED.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N40Ride(string cmd, string[] args, ref int code)
    {
        if (cmd != "n40ride") return;
        string sub = args.Length > 1 ? args[1] : "measure";
        if (sub == "measure") code = N40MeasureCommand();
        else if (sub == "walk") code = N40WalkCommand(args);
        else if (sub == "n38f" || sub == "gates")
        {
            // `n40ride n38f [n38f walk args] [--off]`: builder F's walk (THE GANTRY: n38f walk foundry) with ride40 forced ON (--off: OFF);
            // `n40ride gates <plan.json> [n38 gates args] [--off]`: builder E's checker with ride40 forced ON and the climb tries (--off: neither)
            bool off = Array.IndexOf(args, "--off") >= 0;
            var a = new List<string> { sub == "gates" ? "n38" : "n38f", sub == "gates" ? "gates" : "walk" };
            for (int k = 2; k < args.Length; k++)
            {
                if (args[k] == "--off") continue;
                if (args[k] == "--probe" && k + 1 < args.Length) { N40SetProbe(args[++k]); continue; }
                a.Add(args[k]);
            }
            if (sub == "gates" && !off) a.Add("--ride40");
            SurvivalSim.Force40Ride = sub == "n38f" ? !off : (bool?)null;
            try
            {
                Console.WriteLine($"n40ride {sub}: ride40 forced {(off ? "OFF" : "ON")}");
                int c = -1;
                if (sub == "gates") N38Gates("n38", a.ToArray(), ref c);
                else N38Foundry("n38f", a.ToArray(), ref c);
                code = c;
            }
            finally
            {
                SurvivalSim.Force40Ride = null;
                SurvivalSim.Probe40Ride = null;
                N38GatesRide40 = false;
            }
        }
        else
        {
            Console.WriteLine("n40ride measure | walk [level-id ...] [--seeds N] [--first N] [--cap S] [--ride38] [--ride40] [--trace]");
            code = 2;
        }
    }

    /// <summary>`--probe a,b,..`: what-ifs on ride40's attach rules (each name turns one rule off; `clear=0.8` etc. set a number).</summary>
    static void N40SetProbe(string spec)
    {
        SurvivalSim.Probe40Ride = p =>
        {
            foreach (string s in spec.Split(','))
            {
                string[] kv = s.Split('=');
                double v = kv.Length > 1 ? double.Parse(kv[1], CultureInfo.InvariantCulture) : 0;
                switch (kv[0])
                {
                    case "clear": p.minClearance = v; break;
                    case "speed": p.minSpeed = v; break;
                    case "stick": p.inputDot = kv.Length > 1 ? v : -2; break;
                    case "height": p.minWallHeight = v; break;
                    case "reach": p.attachReach = kv.Length > 1 ? v : -1; break;
                    case "lock": p.lockoutSteps = 0; break;
                }
            }
            return p;
        };
        Console.WriteLine($"n40ride: probe {spec}");
    }

    static string F40(double v) => double.IsNaN(v) ? "n/a" : v.ToString("0.000", CultureInfo.InvariantCulture);

    /// <summary>The game's ride rules on a fresh Mover: flow34, wallride35 (+ ride39's numbers when that row is on), wallgrip36, ride38 as the
    /// design has them; ride40 forced on (after) or off.</summary>
    static Mover N40Mover(GameConfig cfg, ICollisionWorld w, bool after, List<MoverEvent> log)
    {
        var m = new Mover(cfg.movement, w);
        m.SetFlow34(Flow34.Params.From(cfg, cfg.Design("n34.flow34.enabled")));
        Ride35.Params p = Ride35.Params.From(cfg, cfg.Design("n35.wallride35.enabled"));
        m.SetRide35(cfg.Design("n39.ride39.enabled") ? p.With39(cfg) : p);
        m.SetGrip36(Grip36.Params.From(cfg, cfg.Design("n36.wallgrip36.enabled")));
        m.SetRide38(Ride38.Params.From(cfg, cfg.Design("n38.ride38.enabled")));
        if (after)
        {
            m.SetRide38(Ride40.Hold(cfg, true));
            m.SetRide40(Ride40.Params.From(cfg, true));
        }
        if (log != null) m.Events += e => log.Add(e);
        return m;
    }

    struct N40RideRun
    {
        public bool rode, ridingAt3;
        public double attachY, y3, yEnd, seconds, settle;
        public string reason;
    }

    /// <summary>What the player does on the ride, from `at` steps after the attach.</summary>
    enum N40Act { None, Jump, Release, Crouch, Away }

    /// <summary>A ride at a wall (face x = 3, facing -x, z from 0 to -wallLen, `height` tall) met at feet height y0 with up speed vy; jump held
    /// throughout (a hold ride) or never (a tap ride); the act at `actAt` steps after the attach. Runs until the ride ends and the feet land, or
    /// `steps` after the attach.</summary>
    static N40RideRun N40Ride1(GameConfig cfg, double dt, bool after, bool hold, double vy, double pitchDeg, int steps, double wallLen = 420, N40Act act = N40Act.None, int actAt = -1)
    {
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-20, -1, -460), new Vec3(20, 0, 40));
        w.AddBox(new Vec3(3, 0, -wallLen), new Vec3(4, 60, 0));
        var log = new List<MoverEvent>();
        Mover m = N40Mover(cfg, w, after, log);
        double r = cfg.movement.body.capsuleRadius;
        m.Reset(3 - r - 0.02, 3, -1);
        m.velocity = new Vec3(1, vy, -8);
        var run = new N40RideRun { reason = "none", y3 = double.NaN, settle = double.NaN };
        int attach = -1, still = 0, s3 = (int)Math.Round(3 / dt), settleN = (int)Math.Round(0.1 / dt);
        bool prevJ = hold, ended = false;
        for (int s = 0; s < steps + 2400; s++)
        {
            int k = attach >= 0 ? s - attach : -1;
            bool doIt = act != N40Act.None && k >= actAt && actAt >= 0;
            bool j = hold;
            if (doIt && act == N40Act.Release) j = false;
            if (doIt && act == N40Act.Jump) j = k == actAt;
            double mx = doIt && act == N40Act.Away ? -1 : 0.5, mz = doIt && act == N40Act.Away ? 0 : 1;
            InputFrame f = N35In(mx, mz, j, prevJ, pitchDeg, 0, doIt && act == N40Act.Crouch && k == actAt);
            prevJ = j;
            m.Step(f, dt);
            bool riding = m.state == MoveState.WallRide;
            if (attach < 0)
            {
                if (!riding)
                {
                    if (s > 240) break; // no ride within 2 s
                    continue;
                }
                attach = s;
                run.rode = true;
                run.attachY = m.position.y;
                continue;
            }
            k = s - attach;
            if (!ended && !riding)
            {
                ended = true;
                run.reason = N35Reason(log);
                run.seconds = k * dt;
                run.yEnd = m.position.y;
            }
            if (!ended)
            {
                if (Math.Abs(m.velocity.y) < 1e-9) still++;
                else still = 0;
                if (double.IsNaN(run.settle) && still >= settleN) run.settle = (k - settleN + 1) * dt;
            }
            if (k == s3)
            {
                run.y3 = m.position.y;
                run.ridingAt3 = riding;
            }
            if (!ended && k >= steps)
            {
                run.reason = "riding";
                run.seconds = k * dt;
                run.yEnd = m.position.y;
                break;
            }
            if (ended && k >= s3) break;
        }
        return run;
    }

    static int N40MeasureCommand()
    {
        GameConfig cfg = Config();
        double dt = 1 / cfg.movement.loop.simHz;
        Ride38.Params h = Ride40.Hold(cfg, true);
        Console.WriteLine($"n40ride measure (design: wallride35 {cfg.Design("n35.wallride35.enabled")}, wallgrip36 {cfg.Design("n36.wallgrip36.enabled")}, ride38 {cfg.Design("n38.ride38.enabled")}, ride39 {cfg.Design("n39.ride39.enabled")}, ride40 {cfg.Design("n40.ride40.enabled")}; AFTER = ride40 forced on: holdSink {h.holdSink} m/s, guard {h.guard} s, drift +-{h.driftMax} m/s from {h.deadzoneDeg} to {h.fullDeg} deg)");
        bool pass = true;
        int s3 = (int)Math.Round(3 / dt);
        foreach (bool hold in new[] { false, true })
            foreach (double vy in new[] { 0.0, 6.0 })
            {
                N40RideRun b = N40Ride1(cfg, dt, false, hold, vy, 0, s3 + 10);
                N40RideRun a = N40Ride1(cfg, dt, true, hold, vy, 0, s3 + 10);
                double lostB = b.attachY - b.y3, lostA = a.attachY - a.y3;
                bool ok = a.rode && a.ridingAt3 && Math.Abs(lostA) <= 0.05 && !double.IsNaN(a.settle) && a.settle <= 0.1 + 1e-9;
                pass &= ok;
                Console.WriteLine($"  {(hold ? "hold" : "tap ")} vy {vy:0}: TODAY lost {F40(lostB)} m over 3 s (feet {F40(b.attachY)} -> {F40(b.y3)}), riding at 3 s {b.ridingAt3}, end {b.reason} at {F40(b.seconds)} s (lost {F40(b.attachY - b.yEnd)} m on the ride), settle {F40(b.settle)} s"
                    + $" | RIDE40 lost {F40(lostA)} m, riding at 3 s {a.ridingAt3}, end {a.reason}, settle {F40(a.settle)} s {(ok ? "PASS" : "RED")}");
            }
        // the ends (ride40 on): the player, the wall's end, a landing
        int at = (int)Math.Round(1.0 / dt), s30 = (int)Math.Round(30 / dt);
        foreach (bool hold in new[] { false, true })
        {
            var rows = new List<(string name, N40RideRun run, string want)>
            {
                (hold ? "release" : "jump", N40Ride1(cfg, dt, true, hold, 0, 0, s30, 420, hold ? N40Act.Release : N40Act.Jump, at), "jump"),
                ("crouch", N40Ride1(cfg, dt, true, hold, 0, 0, s30, 420, N40Act.Crouch, at), "drop"),
                ("away", N40Ride1(cfg, dt, true, hold, 0, 0, s30, 420, N40Act.Away, at), "away"),
                ("wall end (20 m)", N40Ride1(cfg, dt, true, hold, 0, 0, s30, 20), "lost|corner|away"),
                ("pitch -45 (land)", N40Ride1(cfg, dt, true, hold, 0, -45, s30), "land"),
                ("nothing 30 s", N40Ride1(cfg, dt, true, hold, 0, 0, s30), "riding"),
            };
            var sb = new System.Text.StringBuilder();
            foreach (var (name, run, want) in rows)
            {
                bool ok = run.rode && want.Split('|').Contains(run.reason);
                pass &= ok;
                sb.Append($" {name}: {run.reason} @ {run.seconds:0.00} s{(ok ? "" : " RED")};");
            }
            Console.WriteLine($"  ends ({(hold ? "hold" : "tap")}, ride40):{sb}");
        }
        // the attach (round 2): a deliberate jump at a ride wall rides; the surfaces and inputs that should not, do not (ride40 on), against today
        var a40 = Ride40.Params.From(cfg, true);
        Console.WriteLine($"  attach rules (ride40): clearance >= {a40.minClearance} m, speed >= {a40.minSpeed} m/s, stick toward the wall dot >= {a40.inputDot}, face >= {a40.minWallHeight} m above the feet, near-miss reach {a40.attachReach} m, lockout {a40.lockout} s");
        // want: 1 a ride, 0 none, -1 informational (printed, not judged)
        double rad0 = cfg.movement.body.capsuleRadius;
        var cases = new List<(string name, double wallH, double gap, double along, double into, double stickX, bool hold, int want)>
        {
            ("floor jump at a 6 m wall, jump held", 6, 0.6, 8, 2, 0.5, true, 1),
            ("floor jump at a 6 m wall from 2 m, jump held", 6, 2.0, 8, 3, 0.6, true, 1),
            ("floor jump at a 4 m wall, jump held", 4, 0.6, 8, 2, 0.5, true, 1),
            ("floor SHORT HOP at a 6 m wall (jump let go at 0.1 s)", 6, 0.6, 8, 2, 0.5, false, -1),
            ("floor jump at a 6 m wall, the stick straight ahead (along it)", 6, 0.6, 8, 2, 0, true, 0),
            ("floor jump at a 6 m wall from 3 m/s (the stick accelerates it)", 6, 0.6, 3, 1, 0.5, true, -1),
            ("jump along a 0.5 m riser (a stair), jump held", 0.5, 0.4, 8, 1, 0.5, true, 0),
            ("jump along a 1.5 m platform face, jump held", 1.5, 0.4, 8, 1, 0.5, true, 0),
            ("jump along a 2.4 m crate, jump held", 2.4, 0.4, 8, 1, 0.5, true, 0),
            ($"near miss {0.35:0.00} m off a 6 m wall, jump held, the stick straight ahead (the draw)", 6, rad0 + 0.35, 8, 0, 0, true, 0),
            ($"near miss {0.35:0.00} m off a 6 m wall, jump held, the stick toward it (the draw)", 6, rad0 + 0.35, 8, 0, 0.5, true, 1),
        };
        foreach (var cs in cases)
        {
            var (rb, yb) = N40Attach1(cfg, dt, false, cs.wallH, cs.gap, cs.along, cs.into, cs.stickX, cs.hold);
            var (ra, ya) = N40Attach1(cfg, dt, true, cs.wallH, cs.gap, cs.along, cs.into, cs.stickX, cs.hold);
            bool ok = cs.want < 0 || ra == (cs.want == 1);
            pass &= ok;
            string verdict = cs.want < 0 ? "(info)" : $"(want {(cs.want == 1 ? "ride" : "none")}) {(ok ? "PASS" : "RED")}";
            Console.WriteLine($"    {cs.name}: TODAY {(rb ? $"rides (feet {F40(yb)} m)" : "no ride")} | RIDE40 {(ra ? $"rides (feet {F40(ya)} m)" : "no ride")} {verdict}");
        }
        Console.WriteLine($"n40ride measure: {(pass ? "PASS" : "FAIL")}");
        return pass ? 0 : 1;
    }

    /// <summary>A jump from the floor beside a wall box (face x = 3, facing -x, z 0 .. -60, `wallH` tall): the feet start `gap` m from the face
    /// (the capsule's side `gap` - radius off it) moving `along` m/s along it and `into` m/s toward it, the stick (moveX = stickX, moveZ = 1), the
    /// jump pressed on the first step and held (hold) or let go after 0.1 s (tap). Did a ride start within 1.5 s, and at what feet height?</summary>
    static (bool rode, double feet) N40Attach1(GameConfig cfg, double dt, bool after, double wallH, double gap, double along, double into, double stickX, bool hold)
    {
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-20, -1, -80), new Vec3(20, 0, 20));
        w.AddBox(new Vec3(3, 0, -60), new Vec3(4, wallH, 0));
        Mover m = N40Mover(cfg, w, after, null);
        m.Reset(3 - gap, 0, -1);
        for (int s = 0; s < 3; s++) m.Step(N35In(0, 0, false, false), dt); // stand
        m.velocity = new Vec3(into, 0, -along);
        bool prev = false;
        for (int s = 0; s < (int)Math.Round(1.5 / dt); s++)
        {
            bool j = hold ? true : s < (int)Math.Round(0.1 / dt);
            m.Step(N35In(stickX, 1, j, prev), dt);
            prev = j;
            if (m.state == MoveState.WallRide) return (true, m.position.y);
        }
        return (false, double.NaN);
    }

    // ---- the walk + the attach audit ----

    sealed class N40Room
    {
        public string id;
        public int cleared, deaths, falls;
        public List<double> times = new List<double>();
    }

    struct N40Audit
    {
        public int rides, shortWall, lowGround, either;
    }

    /// <summary>The wall's face above the feet at a ride's start: rays toward the wall (-n) from the capsule axis every 0.1 m up from the feet + 0.1;
    /// the height of the first that finds no valid wall within the radius + 0.3 m (the face's top above the feet; 3.0 = taller).</summary>
    static double N40WallAbove(ICollisionWorld w, Vec3 feet, Vec3 n, double radius, double maxAbsNy)
    {
        for (double hgt = 0.1; hgt <= 3.0 + 1e-9; hgt += 0.1)
        {
            Vec3 o = new Vec3(feet.x, feet.y + hgt, feet.z);
            if (!w.RayIntersect(o, new Vec3(-n.x, 0, -n.z), out RayHit hit) || hit.distance > radius + 0.3 || Math.Abs(hit.normal.y) >= maxAbsNy) return hgt - 0.1;
        }
        return 3.0;
    }

    /// <summary>The ground's distance below the feet (a ray down from the feet + 0.01 m; infinity when none).</summary>
    static double N40GroundBelow(ICollisionWorld w, Vec3 feet)
    {
        if (!w.RayIntersect(new Vec3(feet.x, feet.y + 0.01, feet.z), new Vec3(0, -1, 0), out RayHit hit)) return double.PositiveInfinity;
        return hit.distance - 0.01;
    }

    static int N40WalkCommand(string[] args)
    {
        GameConfig cfg = Config();
        int seeds = 3, first = 1;
        double cap = 900;
        bool ride38 = Array.IndexOf(args, "--ride38") >= 0, ride40 = Array.IndexOf(args, "--ride40") >= 0, trace = Array.IndexOf(args, "--trace") >= 0;
        var levels = new List<string>();
        for (int i = 2; i < args.Length; i++)
        {
            string a = args[i];
            if (a == "--seeds" || a == "--first" || a == "--cap")
            {
                double v = double.Parse(args[i + 1], CultureInfo.InvariantCulture);
                if (a == "--seeds") seeds = (int)v;
                else if (a == "--first") first = (int)v;
                else cap = v;
                i++;
                continue;
            }
            if (!a.StartsWith("--")) levels.Add(a);
        }
        if (levels.Count == 0)
        {
            List<string> order = Curriculum25.Order(cfg, N25Blocks.From(cfg));
            levels.AddRange(order.Take(5));
            Console.WriteLine($"n40ride walk: the campaign order with the design config: [{string.Join(", ", order)}]; levels 1-5 walked");
        }
        int code = 0;
        var total = new N40Audit();
        foreach (string id in levels)
        {
            code |= N40WalkLevel(cfg, id, seeds, first, cap, ride38, ride40, trace, ref total);
        }
        if (levels.Count > 1)
            Console.WriteLine($"n40ride walk ALL: rides {total.rides}, short wall (< 2.0 m above the feet) {total.shortWall}, low (ground within 1.0 m) {total.lowGround}, either {total.either}");
        return code;
    }

    static int N40WalkLevel(GameConfig cfg, string spec, int seeds, int first, double cap, bool ride38, bool ride40, bool trace, ref N40Audit total)
    {
        // `level-id` or `level-id:ROOM` (the run starts at that room's spawn, the doors free until it is entered: N38fWalk's way past rooms the
        // pilot cannot play, e.g. n38-foundry's a1, whose slide tunnel Walk29Pilot never slides: NIGHT39-LOG "n36move walk sticks in room a1")
        string id = spec, fromId = null;
        int colon = spec.IndexOf(':');
        if (colon > 0) { id = spec.Substring(0, colon); fromId = spec.Substring(colon + 1); }
        string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
        if (!File.Exists(path)) { Console.WriteLine($"n40ride walk: no level file {path}"); return 2; }
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        int from = fromId == null ? 0 : level.rooms.FindIndex(r => r.id == fromId);
        if (from < 0 || from >= playable) { Console.WriteLine($"n40ride walk: room {fromId} not a playable room of {id}"); return 2; }
        var rooms = new List<N40Room>();
        for (int i = 0; i < playable; i++) rooms.Add(new N40Room { id = level.rooms[i].id });
        var audit = new N40Audit();
        var perRoom = new Dictionary<string, N40Audit>();
        int done = 0, ridesPilot = 0;
        double radius = cfg.movement.body.capsuleRadius, maxNy = cfg.movement.surface.wallMaxAbsNormalY;
        Console.WriteLine($"n40ride walk {id}: {playable} playable rooms, {seeds} seeds from {first}; ride38 {(ride38 ? "FORCED ON" : "design")}, ride40 {(ride40 ? "FORCED ON" : "design")}");
        SurvivalSim.Force38Ride = ride38 ? true : (bool?)null;
        SurvivalSim.Force40Ride = ride40 ? true : (bool?)null;
        try
        {
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
                    if (c == null || c.room < 0) return lv.rooms[0].spawn;
                    if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                    return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
                };
                var pilot = new Walk38Pilot(sim, ap);
                sim.UseInput(pilot);
                var clearedThis = new bool[playable];
                int clearedN = 0;
                sim.events.On<RoomEvent>("roomClear", e =>
                {
                    if (e.room < from || e.room >= playable || clearedThis[e.room]) return;
                    clearedThis[e.room] = true;
                    clearedN++;
                    rooms[e.room].cleared++;
                    rooms[e.room].times.Add(e.time);
                    if (trace) Console.WriteLine($"    [seed {seed}] {sim.run.steps * sim.step:0.00} clear {e.id}");
                });
                sim.events.On<RoomEvent>("roomRetry", e => { if (e.room >= from && e.room < playable) rooms[e.room].deaths++; });
                sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room >= from && e.room < playable) rooms[e.room].falls++; });
                sim.StartRun(seed, null, null);
                sim.mover.Events += e =>
                {
                    if (e.type != MoverEventType.WallRideStart) return;
                    Mover mv = sim.mover;
                    double above = N40WallAbove(mv.World, mv.position, mv.wallNormal, radius, maxNy);
                    double below = N40GroundBelow(mv.World, mv.position);
                    bool sh = above < 2.0 - 1e-9, lo = below < 1.0;
                    string rid = sim.campaign != null && sim.campaign.room >= 0 ? level.rooms[sim.campaign.room].id : "?";
                    perRoom.TryGetValue(rid, out N40Audit ra);
                    ra.rides++; audit.rides++;
                    if (sh) { ra.shortWall++; audit.shortWall++; }
                    if (lo) { ra.lowGround++; audit.lowGround++; }
                    if (sh || lo) { ra.either++; audit.either++; }
                    perRoom[rid] = ra;
                    if (trace) Console.WriteLine($"    [seed {seed}] {sim.run.steps * sim.step:0.00} ride {rid} feet ({mv.position.x:0.00},{mv.position.y:0.00},{mv.position.z:0.00}) n ({mv.wallNormal.x:0.00},{mv.wallNormal.z:0.00}) face above {above:0.0} m, ground below {below:0.00} m, speed {e.speed:0.0}{(sh ? " SHORT" : "")}{(lo ? " LOW" : "")}");
                };
                int max = MovementMath.StepsFor(cap, sim.step);
                int st = 0, want = playable - from;
                bool placed = from == 0, doorFreeOn = false;
                for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
                {
                    AutopilotInput.TakeDraft(sim);
                    sim.Step();
                    Campaign24 c = sim.campaign;
                    if (!placed && c != null && c.room >= 0) // N38fWalk's start room: the doors free, the player put at its spawn
                    {
                        c.SetDoorFree29(true);
                        doorFreeOn = true;
                        sim.Respawn(level.rooms[from].spawn, level.rooms[from].spawnYawDeg);
                        placed = true;
                    }
                    if (doorFreeOn && c != null && c.room == from) { c.SetDoorFree29(false); doorFreeOn = false; } // entered: the game's door rule again
                    if (clearedN >= want) break;
                }
                bool ok = clearedN >= want;
                if (ok) done++;
                else
                {
                    Vec3 p = sim.mover.position;
                    Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0} s in {(sim.campaign != null && sim.campaign.room >= 0 ? level.rooms[sim.campaign.room].id : "?")} {sim.campaign?.phase} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), state {sim.mover.state}, alive {sim.enemies.AliveCount}");
                }
                ridesPilot += pilot.rides;
                ap.Detach();
            }
        }
        finally
        {
            SurvivalSim.Force38Ride = null;
            SurvivalSim.Force40Ride = null;
        }
        int deaths = 0;
        foreach (N40Room r in rooms)
        {
            r.times.Sort();
            deaths += r.deaths;
            perRoom.TryGetValue(r.id, out N40Audit ra);
            string med = r.times.Count == 0 ? "-" : r.times[r.times.Count / 2].ToString("0.0", CultureInfo.InvariantCulture);
            Console.WriteLine($"  {id} {r.id}: cleared {r.cleared}/{seeds}, median {med} s, deaths {r.deaths}, falls {r.falls}; rides {ra.rides} (short {ra.shortWall}, low {ra.lowGround}, either {ra.either})");
        }
        total.rides += audit.rides;
        total.shortWall += audit.shortWall;
        total.lowGround += audit.lowGround;
        total.either += audit.either;
        bool green = done == seeds;
        Console.WriteLine($"n40ride walk {id}: {(green ? "GREEN" : "RED")} ({done}/{seeds} runs through every playable room; deaths {deaths}); rides {audit.rides}: short wall (< 2.0 m above the feet) {audit.shortWall}, low (ground within 1.0 m) {audit.lowGround}, either {audit.either}");
        return green ? 0 : 1;
    }
}
