// NIGHT34 builder A (NIGHT34.md A1): lines34 / calm34.
//   lines34 measure          the movement distances every lines34 spacing comes from (the real Mover with the game's move rules, SpamRules):
//                            a running jump on flat ground, a bhop chain, a dash, a wall ride at entry speed, the grapple's reach (config).
//   lines34 <level>          per fight room of Design/levels/<level>.json: wall-run walls (>= 1 at length >= the measured ride), grapple nodes
//                            (>= 1 within reach of a floor and a wall), floors (>= 2), a bhop lane (>= 3 jumps, no prop in it), a climb route per
//                            floor (an enemy-walkable ramp or stair from the floor below). Exit 1 on any fight room short.
//   calm34 <level>           the dressing props (dress33's rows) placed with design.n34.calm34 off / on (Calm34.Keep), and the cut share (>= 40 %).
//   lines34 walk <level>     the walk bot over the level (NIGHT33's arenas33 walk), 10 seeds.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N34Lines(string cmd, string[] args, ref int code)
    {
        if (cmd != "lines34" && cmd != "calm34") return;
        string a1 = args.Length > 1 ? args[1] : "";
        if (cmd == "lines34" && a1 == "measure") { code = Lines34Measure(); return; }
        if (cmd == "calm34") { code = Calm34Count(args.Skip(1).Where(a => !a.StartsWith("--")).ToArray()); return; }
        if (cmd == "lines34" && a1 == "walk")
        {
            int seeds = 10;
            for (int i = 1; i < args.Length - 1; i++) if (args[i] == "--seeds") seeds = int.Parse(args[i + 1], CultureInfo.InvariantCulture);
            code = 0;
            foreach (string g in args.Skip(2).Where(a => !a.StartsWith("--") && !char.IsDigit(a[0]))) code |= Lines34Walk(g, seeds);
            return;
        }
        if (cmd == "lines34" && a1.Length > 0 && !a1.StartsWith("--")) { code = 0; foreach (string g in args.Skip(1).Where(a => !a.StartsWith("--"))) code |= Lines34Rooms(g); return; }
        Console.WriteLine($"{cmd}: unknown arguments '{string.Join(" ", args.Skip(1))}'");
        code = 2;
    }

    // ============================================================ measure ============================================================

    /// <summary>The distances (m) the lines34 checks and the n34 generators space by; Lines34Measure prints them, Lines34Numbers returns them.</summary>
    public sealed class Lines34Numbers
    {
        public double slideHop, slideHopSpeed, slideHop3, runJump, runJumpAir, bhopHop, bhopSpeed, bhop3, dashGround, dashAir, rideWalk, rideWalkSpeed, rideFast, rideFastSpeed, grappleReach, jumpPeak, doubleJumpPeak;
    }

    static Mover Lines34Mover(GameConfig cfg, ICollisionWorld w)
    {
        var m = new Mover(cfg.movement, w);
        SpamRules(m, cfg, false);
        return m;
    }

    static Lines34Numbers Lines34Measure(GameConfig cfg, bool print)
    {
        double dt = cfg.Dt;
        var n = new Lines34Numbers();
        var plane = new TriangleWorld();
        plane.AddBox(new Vec3(-2000, -1, -2000), new Vec3(2000, 0, 2000));
        Button Tap() => new Button { held = true, pressed = true };

        // 1. a running jump: 2 s run-up toward -z, one press, held; takeoff to the first landing (horizontal)
        {
            Mover m = Lines34Mover(cfg, plane);
            m.Reset(0, 0, 1000);
            for (int i = 0; i < 240; i++) m.Step(new InputFrame { moveZ = 1 }, dt);
            Vec3 t0 = m.position;
            double speed0 = m.Speed, peak = 0;
            m.Step(new InputFrame { moveZ = 1, jump = Tap() }, dt);
            int air = 0;
            for (int i = 0; i < 600 && !m.grounded; i++) { m.Step(new InputFrame { moveZ = 1, jump = new Button { held = true } }, dt); air++; peak = Math.Max(peak, m.position.y); }
            n.runJump = Math.Abs(m.position.z - t0.z);
            n.runJumpAir = air * dt;
            n.jumpPeak = peak;
            if (print) Console.WriteLine($"lines34 measure: running jump at {Sf(speed0)} m/s: {Sf(n.runJump)} m takeoff to landing, {Sf(n.runJumpAir)} s in the air, feet peak {Sf(peak)} m");
        }
        // 1b. a double jump's feet peak (the second press at the first jump's apex)
        {
            Mover m = Lines34Mover(cfg, plane);
            m.Reset(0, 0, 1000);
            for (int i = 0; i < 30; i++) m.Step(new InputFrame(), dt);
            m.Step(new InputFrame { jump = Tap() }, dt);
            double peak = 0, lastY = 0; bool second = false;
            for (int i = 0; i < 600; i++)
            {
                bool press = !second && m.position.y < lastY; // falling: the apex has passed
                lastY = m.position.y;
                m.Step(new InputFrame { jump = press ? Tap() : new Button { held = true } }, dt);
                if (press) second = true;
                peak = Math.Max(peak, m.position.y);
                if (second && m.grounded) break;
            }
            n.doubleJumpPeak = peak;
            if (print) Console.WriteLine($"lines34 measure: double jump (second press at the apex): feet peak {Sf(peak)} m");
        }
        // 2. a bhop chain: jump held, pressed again on every landing (the buffer), 8 hops; the per-hop distance at hops 4-8 and the speed
        {
            Mover m = Lines34Mover(cfg, plane);
            m.Reset(0, 0, 1000);
            for (int i = 0; i < 240; i++) m.Step(new InputFrame { moveZ = 1 }, dt);
            var takeoffs = new List<double>();
            var speeds = new List<double>();
            for (int i = 0; i < 120 * 12 && takeoffs.Count < 9; i++)
            {
                bool g = m.grounded; // pressed on every grounded step: the first step of each landing jumps again
                m.Step(new InputFrame { moveZ = 1, jump = g ? Tap() : new Button { held = true } }, dt);
                if (g && !m.grounded) { takeoffs.Add(m.position.z); speeds.Add(m.Speed); }
            }
            var hops = new List<double>();
            for (int k = 1; k < takeoffs.Count; k++) hops.Add(Math.Abs(takeoffs[k] - takeoffs[k - 1]));
            var steady = hops.Skip(3).ToList();
            n.bhopHop = steady.Count > 0 ? steady.Average() : (hops.Count > 0 ? hops.Average() : 0);
            n.bhopSpeed = speeds.Count > 4 ? speeds.Skip(4).Average() : 0;
            n.bhop3 = hops.Take(3).Sum();
            if (print) Console.WriteLine($"lines34 measure: bhop chain ({hops.Count} hops): first three {string.Join(" / ", hops.Take(3).Select(Sf))} m (sum {Sf(n.bhop3)} m), steady hop {Sf(n.bhopHop)} m at {Sf(n.bhopSpeed)} m/s");
        }
        // 2b. a slide-hop chain (the speed line): on each landing a slide press, the jump on the next step (the slide-jump carry)
        {
            Mover m = Lines34Mover(cfg, plane);
            m.Reset(0, 0, 1000);
            for (int i = 0; i < 240; i++) m.Step(new InputFrame { moveZ = 1 }, dt);
            var takeoffs = new List<double>();
            var speeds = new List<double>();
            for (int i = 0; i < 120 * 12 && takeoffs.Count < 9; i++)
            {
                bool g = m.grounded;
                var f = new InputFrame { moveZ = 1, jump = new Button { held = !g } };
                if (g && m.state != MoveState.Slide) f.slide = new Button { held = true, pressed = true };
                else if (g) { f.slide = new Button { held = true }; f.jump = Tap(); }
                m.Step(f, dt);
                if (g && !m.grounded) { takeoffs.Add(m.position.z); speeds.Add(m.Speed); }
            }
            var hops = new List<double>();
            for (int k = 1; k < takeoffs.Count; k++) hops.Add(Math.Abs(takeoffs[k] - takeoffs[k - 1]));
            double sh = hops.Count > 3 ? hops.Skip(3).Average() : hops.Count > 0 ? hops.Average() : 0;
            n.slideHop = sh; n.slideHopSpeed = speeds.Count > 4 ? speeds.Skip(4).Average() : 0; n.slideHop3 = hops.Take(3).Sum();
            if (print) Console.WriteLine($"lines34 measure: slide-hop chain ({hops.Count} hops): first three {string.Join(" / ", hops.Take(3).Select(Sf))} m (sum {Sf(n.slideHop3)} m), steady hop {Sf(sh)} m at {Sf(n.slideHopSpeed)} m/s");
        }
        // 3. a dash: from a run, on the ground (to the end of the dash's duration plus the carry until speed is back under walk + 0.5), and in the air
        //    (a running jump with a dash pressed at the apex: the gap that combination crosses)
        {
            Mover m = Lines34Mover(cfg, plane);
            m.Reset(0, 0, 1000);
            for (int i = 0; i < 240; i++) m.Step(new InputFrame { moveZ = 1 }, dt);
            double z0 = m.position.z;
            m.Step(new InputFrame { moveZ = 1, dash = Tap() }, dt);
            int k = 0;
            double walk = cfg.movement.ground.walkSpeed;
            while (k < 600 && (k < MovementMath.StepsFor(cfg.movement.dash.duration, dt) || m.Speed > walk + 0.5)) { m.Step(new InputFrame { moveZ = 1 }, dt); k++; }
            n.dashGround = Math.Abs(m.position.z - z0) - walk * (k + 1) * dt; // the dash's gain over running the same time
            double dashTotal = Math.Abs(m.position.z - z0);
            Mover a = Lines34Mover(cfg, plane);
            a.Reset(0, 0, 1000);
            for (int i = 0; i < 240; i++) a.Step(new InputFrame { moveZ = 1 }, dt);
            double az = a.position.z, lastY = 0;
            a.Step(new InputFrame { moveZ = 1, jump = Tap() }, dt);
            bool dashed = false;
            for (int i = 0; i < 600 && !a.grounded; i++)
            {
                bool press = !dashed && a.position.y < lastY;
                lastY = a.position.y;
                a.Step(new InputFrame { moveZ = 1, jump = new Button { held = true }, dash = press ? Tap() : default }, dt);
                if (press) dashed = true;
            }
            n.dashAir = Math.Abs(a.position.z - az);
            if (print) Console.WriteLine($"lines34 measure: ground dash from a run: {Sf(dashTotal)} m in {Sf((k + 1) * dt)} s ({Sf(n.dashGround)} m more than running); running jump + air dash at the apex: {Sf(n.dashAir)} m takeoff to landing");
        }
        // 4. a wall ride: a wall face at x = -3.5 along z; a run-up parallel to it 0.45 m off (feet centre), a jump, a lean into the wall (PlatformRoutes26.HallRun's
        //    0.25 rad); the first ride's time and length along the wall (no jump off: the ride ends at maxTime or the speed floor)
        double Ride(double runSteps, bool slide, out double entry, out double seconds)
        {
            double best = RideLean(runSteps, slide, 0.25, out entry, out seconds);
            double other = RideLean(runSteps, slide, -0.25, out double e2b, out double s2b);
            if (other > best) { best = other; entry = e2b; seconds = s2b; }
            return best;
        }
        double RideLean(double runSteps, bool slide, double lean, out double entry, out double seconds)
        {
            var canyon = new TriangleWorld();
            canyon.AddBox(new Vec3(-2000, -1, -2000), new Vec3(2000, 0, 2000));
            canyon.AddBox(new Vec3(-4.5, 0, -2000), new Vec3(-3.5, 14, 2000));
            Mover m = Lines34Mover(cfg, canyon);
            m.Reset(0, 0, 1000);
            entry = 0; seconds = 0;
            // the run-up: straight for runSteps, then turned by `lean` (the sign that closes on the wall), a slide 0.3 s before the jump when asked,
            // the jump when the feet centre is 0.6 m off the wall face; the air steers on along the same heading
            bool jumped = false, slid = false;
            for (int i = 0; i < runSteps + 1200 && !jumped; i++)
            {
                bool turn = i >= runSteps;
                double gap = m.position.x - (-3.5);
                var f = new InputFrame { moveZ = 1, yaw = turn ? lean : 0 };
                if (slide && turn && !slid && gap < 0.6 + 0.3 * m.Speed) { f.slide = new Button { held = true, pressed = true }; slid = true; }
                else if (slide && slid) f.slide = new Button { held = true };
                if (turn && gap < 0.6) { f.jump = Tap(); f.slide = default; jumped = true; }
                m.Step(f, dt);
                if (gap > 8) break; // turned away from the wall
            }
            if (!jumped) return 0;
            double zStart = 0; int steps = 0; bool rode = false;
            for (int i = 0; i < 600; i++)
            {
                m.Step(new InputFrame { moveZ = 1, yaw = lean, jump = new Button { held = true } }, dt);
                bool on = m.state == MoveState.WallRide;
                if (on && !rode) { rode = true; zStart = m.position.z; entry = m.Speed; }
                if (on) steps++;
                if (rode && !on) break;
                if (m.grounded && !rode && i > 10) break;
            }
            seconds = steps * dt;
            return rode ? Math.Abs(m.position.z - zStart) : 0;
        }
        n.rideWalk = Ride(240, false, out double e1, out double s1);
        n.rideWalkSpeed = e1;
        n.rideFast = Ride(240, true, out double e2, out double s2);
        n.rideFastSpeed = e2;
        if (print)
        {
            Console.WriteLine($"lines34 measure: wall ride from a run (entry {Sf(e1)} m/s): {Sf(n.rideWalk)} m along the wall in {Sf(s1)} s (wallRide.maxTime {Sf(cfg.movement.wallRide.maxTime)} s, minSpeed {Sf(cfg.movement.wallRide.minSpeed)})");
            Console.WriteLine($"lines34 measure: wall ride from a slide-jump (entry {Sf(e2)} m/s): {Sf(n.rideFast)} m along the wall in {Sf(s2)} s");
        }
        // 5. the grapple's reach: the ray range the run uses (design.n32.grapple32.range when that row is on; the web's arcline.range otherwise)
        bool g32 = !N32Blocks.ForceOff && cfg.Design("n32.grapple32.enabled");
        n.grappleReach = g32 ? cfg.Num("design.n32.grapple32.range") : cfg.Num("arcline.range");
        if (print) Console.WriteLine($"lines34 measure: grapple reach {Sf(n.grappleReach)} m ({(g32 ? "design.n32.grapple32.range" : "arcline.range")}; the ray range GrappleAim reads)");
        return n;
    }

    static int Lines34Measure()
    {
        Lines34Measure(Config(), true);
        return 0;
    }

    // ============================================================ calm34 ============================================================

    static readonly string[] Lines34AllLevels = { "foundry", "iceline", "kiln", "span", "spire" };

    /// <summary>A level id from an argument: "foundry" -> n33-foundry (calm34 dresses NIGHT33's files), "n34-kiln" as given.</summary>
    static string Lines34LevelId(string a, string prefix) => a.StartsWith("n3", StringComparison.Ordinal) ? a : prefix + "-" + a;

    static List<Dictionary<string, object>> Lines34Props(Dictionary<string, object> map)
    {
        var res = new List<Dictionary<string, object>>();
        if (map.TryGetValue("campaign", out object co) && co is Dictionary<string, object> c && c.TryGetValue("props", out object po) && po is List<object> rows)
            foreach (object r in rows) if (r is Dictionary<string, object> d) res.Add(d);
        return res;
    }

    /// <summary>calm34 [level ...] (default: all five n33 levels, plus every n34 file that exists): the dress33 rows placed off / on, the cut share.
    /// PASS per level: at least 40 % fewer dressing rows (washes counted, and again with the washes left out: they are the wall surface).</summary>
    static int Calm34Count(string[] names)
    {
        GameConfig cfg = Config();
        HashSet<string> keep = Calm34.KeepKinds(cfg);
        if (keep == null) { Console.WriteLine("calm34 FAIL: design.n34.calm34.keepKinds is missing"); return 1; }
        var ids = new List<string>();
        if (names.Length == 0)
        {
            foreach (string n in Lines34AllLevels) ids.Add("n33-" + n);
            foreach (string n in Lines34AllLevels) if (File.Exists(Path.Combine(RepoRoot, "Design", "levels", "n34-" + n + ".json"))) ids.Add("n34-" + n);
        }
        else foreach (string a in names) ids.Add(Lines34LevelId(a, "n33"));
        int fails = 0;
        foreach (string id in ids)
        {
            string path = Path.Combine(RepoRoot, "Design", "levels", id + ".json");
            if (!File.Exists(path)) { Console.WriteLine($"calm34 FAIL {id}: no {path}"); fails++; continue; }
            var map = PortData.Obj(PortData.LoadJson(path));
            int dressing = 0, kept = 0, wash = 0, washKept = 0, colliders = 0, others = 0, flagged = 0;
            var cutBy = new SortedDictionary<string, int>(StringComparer.Ordinal);
            foreach (var r in Lines34Props(map))
            {
                string rid = r.TryGetValue("id", out object io) ? io as string : null;
                string kind = r.TryGetValue("kind", out object ko) ? ko as string ?? "crate" : "crate";
                bool col = r.TryGetValue("collider", out object cl) && cl is bool cb && cb;
                if (r.TryGetValue("dress33", out object d3) && d3 is bool db && db) flagged++;
                if (!Calm34.IsDressing(rid)) { if (col) colliders++; else others++; continue; }
                dressing++;
                if (kind == "wash") wash++;
                bool k = Calm34.Keep(keep, rid, kind);
                if (k) { kept++; if (kind == "wash") washKept++; }
                else cutBy[kind] = (cutBy.TryGetValue(kind, out int v) ? v : 0) + 1;
                if (col && !k) { Console.WriteLine($"calm34 FAIL {id}: {rid} has a collider and would be cut"); fails++; }
            }
            double cut = dressing > 0 ? 1 - kept / (double)dressing : 0;
            int pieces = dressing - wash, piecesKept = kept - washKept;
            double cutPieces = pieces > 0 ? 1 - piecesKept / (double)pieces : 0;
            bool ok = cut >= 0.4 && cutPieces >= 0.4 && flagged == dressing;
            if (!ok) fails++;
            Console.WriteLine($"calm34 {(ok ? "PASS" : "FAIL")} {id}: dressing rows off {dressing} / on {kept} ({Sf(100 * cut)} % fewer); without the wall washes off {pieces} / on {piecesKept} ({Sf(100 * cutPieces)} % fewer); untouched rows: {colliders} with a collider, {others} other (devices, pickups, strips); dress33 flags {flagged} = ids {dressing}{(flagged == dressing ? "" : " MISMATCH")}");
            Console.WriteLine($"  cut by kind: {string.Join(", ", cutBy.Select(kv => kv.Key + " " + kv.Value))}");
        }
        Console.WriteLine($"calm34: {(fails == 0 ? "ALL PASS" : fails + " FAIL")} ({ids.Count} levels; keep {string.Join(" ", keep.OrderBy(s => s, StringComparer.Ordinal))})");
        return fails == 0 ? 0 : 1;
    }

    // ============================================================ lines34 <level> ============================================================

    struct L34Rect { public double x0, z0, x1, z1; public L34Rect(double a, double b, double c, double d) { x0 = Math.Min(a, c); z0 = Math.Min(b, d); x1 = Math.Max(a, c); z1 = Math.Max(b, d); } public bool In(double x, double z) => x > x0 && x < x1 && z > z0 && z < z1; public double W => x1 - x0; public double D => z1 - z0; }

    sealed class L34Tier { public double y; public L34Rect rect; public bool reached; public string how = ""; }

    static List<object> L34Arr(Dictionary<string, object> o, string k) => o != null && o.TryGetValue(k, out object v) && v is List<object> l ? l : new List<object>();
    static double L34N(object o) => PortData.Num(o);
    static double L34Num(Dictionary<string, object> o, string k, double d) => o.TryGetValue(k, out object v) && v != null ? PortData.Num(v) : d;

    /// <summary>lines34 [n34-|n33-]<name>: per fight room of the level's PLAN (Design/levels/<prefix>/<name>.plan.json, after dress33), the movement-line
    /// counts. A fight room = a room row with waves, not a boss arena. The bar (NIGHT34.md A1): ride walls >= 1 at length >= the measured ride, grapple
    /// nodes >= 1 (each within the grapple's reach of the floor and of a wall), floors >= 2, a lane of >= 3 running jumps with no prop in it, a climb route
    /// to every raised floor (a ramp or a chain of steps <= 1.3 m from the ground: the enemies' way up, NIGHT20).</summary>
    /// <summary>NIGHT35 A (lines35, Harness.N35.Levels.cs): a level before the grapple's level (design.n35.grapple35.fromLevel) needs no node per fight room
    /// (DECIDE35 D-3: its line is the wall rides); the count is still printed.</summary>
    static bool lines34NodesOptional;

    static int Lines34Rooms(string arg)
    {
        GameConfig cfg = Config();
        Lines34Numbers mn = Lines34Measure(cfg, false);
        string id = Lines34LevelId(arg, "n34");
        string prefix = id.Substring(0, 3), name = id.Substring(4);
        string path = Path.Combine(RepoRoot, "Design", "levels", prefix, name + ".plan.json");
        if (!File.Exists(path)) { Console.WriteLine($"lines34 FAIL {id}: no plan {path}"); return 1; }
        var plan = PortData.Obj(PortData.LoadJson(path));
        double ride = mn.rideWalk, lane = 3 * mn.runJump, reach = mn.grappleReach;
        Console.WriteLine($"lines34 {id}: the bar from lines34 measure: ride wall >= {Sf(ride)} m (a ride from a run), lane >= {Sf(lane)} m (3 running jumps of {Sf(mn.runJump)} m), grapple reach {Sf(reach)} m; step <= 1.3 m");
        var boxes = L34Arr(plan, "boxes").Cast<Dictionary<string, object>>().ToList();
        var ramps = L34Arr(plan, "ramps").Cast<Dictionary<string, object>>().ToList();
        var pits = L34Arr(plan, "pits").Cast<Dictionary<string, object>>().ToList();
        var props = L34Arr(plan, "props").Cast<Dictionary<string, object>>().Where(p => p.TryGetValue("collider", out object c) && c is bool b && b).ToList();
        var doors = L34Arr(plan, "doors").Cast<Dictionary<string, object>>().ToList();
        int fights = 0, short_ = 0;
        foreach (Dictionary<string, object> room in L34Arr(plan, "rooms").Cast<Dictionary<string, object>>())
        {
            string rid = (string)room["id"];
            string kind = room.TryGetValue("kind", out object ko) ? ko as string : "room";
            if (kind == "boss" || L34Arr(room, "waves").Count == 0) continue;
            fights++;
            var rr = L34Arr(room, "rect");
            var R = new L34Rect(L34N(rr[0]), L34N(rr[1]), L34N(rr[2]), L34N(rr[3]));
            double H = L34Num(room, "height", 10);
            bool Mine(Dictionary<string, object> o) => o.TryGetValue("zone", out object z) && z as string == rid;
            var myBoxes = boxes.Where(Mine).ToList();
            var myRamps = ramps.Where(Mine).ToList();
            var myPits = pits.Where(p => !p.ContainsKey("zone") || (string)p["zone"] == rid).Where(p => { var q = L34Arr(p, "rect"); var pr = new L34Rect(L34N(q[0]), L34N(q[1]), L34N(q[2]), L34N(q[3])); return pr.x0 < R.x1 && pr.x1 > R.x0 && pr.z0 < R.z1 && pr.z1 > R.z0; }).ToList();
            var myProps = props.Where(p => p.TryGetValue("room", out object rm) && rm as string == rid).ToList();
            // ---- walls: the room's wall runs (each edge minus its door gaps) and inner walls (a box >= 3 m tall standing on the floor, a face >= ride)
            var runs = new List<double>();
            var edges = new[] { (true, R.z0, R.x0, R.x1), (true, R.z1, R.x0, R.x1), (false, R.x0, R.z0, R.z1), (false, R.x1, R.z0, R.z1) };
            foreach (var (alongX, c, a, b) in edges)
            {
                var gaps = new List<(double, double)>();
                foreach (var d in doors)
                {
                    var dr = L34Arr(d, "rect");
                    double dx0 = L34N(dr[0]), dz0 = L34N(dr[1]), dx1 = L34N(dr[2]), dz1 = L34N(dr[3]);
                    bool touches = alongX ? c >= Math.Min(dz0, dz1) - 0.01 && c <= Math.Max(dz0, dz1) + 0.01 : c >= Math.Min(dx0, dx1) - 0.01 && c <= Math.Max(dx0, dx1) + 0.01;
                    if (touches) gaps.Add(alongX ? (Math.Min(dx0, dx1), Math.Max(dx0, dx1)) : (Math.Min(dz0, dz1), Math.Max(dz0, dz1)));
                }
                gaps.Sort();
                double cur = a;
                foreach (var (g0, g1) in gaps) { if (g0 > cur) runs.Add(Math.Min(g0, b) - cur); cur = Math.Max(cur, g1); }
                if (cur < b) runs.Add(b - cur);
            }
            int roomWalls = runs.Count(l => l >= ride - 1e-6);
            int inner = 0;
            foreach (var bx in myBoxes)
            {
                var mn0 = L34Arr(bx, "min"); var mx0 = L34Arr(bx, "max");
                double h = L34N(mx0[1]) - Math.Max(0, L34N(mn0[1]));
                double face = Math.Max(L34N(mx0[0]) - L34N(mn0[0]), L34N(mx0[2]) - L34N(mn0[2]));
                if (h >= 3 && L34N(mn0[1]) <= 0.01 && face >= ride - 1e-6) inner++;
            }
            int walls = roomWalls + inner;
            // ---- grapple nodes in the room, each within reach of the floor (straight down) and of a wall (the nearest room edge or inner wall)
            var nodes = L34Arr(room, "grappleNodes").Select(o => L34Arr(new Dictionary<string, object> { ["p"] = o }, "p")).ToList();
            int nodesOk = 0;
            foreach (var n in nodes)
            {
                double nx = L34N(n[0]), ny = L34N(n[1]), nz = L34N(n[2]);
                double toWall = Math.Min(Math.Min(nx - R.x0, R.x1 - nx), Math.Min(nz - R.z0, R.z1 - nz));
                if (R.In(nx, nz) && ny <= reach && toWall <= reach && ny >= 3) nodesOk++;
            }
            // ---- floors: the ground and every box top >= 0.4 m with >= 6 m2 standing on it (a top with another box on it still counts)
            var tiers = new List<L34Tier>();
            foreach (var bx in myBoxes)
            {
                var mn0 = L34Arr(bx, "min"); var mx0 = L34Arr(bx, "max");
                double top = L34N(mx0[1]);
                var r2 = new L34Rect(L34N(mn0[0]), L34N(mn0[2]), L34N(mx0[0]), L34N(mx0[2]));
                if (top >= 0.4 && top <= H - 2 && r2.W * r2.D >= 6 && Math.Min(r2.W, r2.D) >= 1.5) tiers.Add(new L34Tier { y = top, rect = r2 });
            }
            var heights = new SortedSet<double> { 0 };
            foreach (var t in tiers) heights.Add(Math.Round(t.y * 2) / 2);
            // ---- climb: ramps (low end on the ground or a reached tier, high end on a tier at the ramp's rise) and steps (<= 1.3 m, touching)
            bool Touch(L34Rect a, double x, double z, double m) => x >= a.x0 - m && x <= a.x1 + m && z >= a.z0 - m && z <= a.z1 + m;
            bool Near(L34Rect a, L34Rect b, double m) => a.x0 <= b.x1 + m && b.x0 <= a.x1 + m && a.z0 <= b.z1 + m && b.z0 <= a.z1 + m;
            for (int pass = 0; pass < 8; pass++)
                foreach (var t in tiers)
                {
                    if (t.reached) continue;
                    if (t.y <= 1.3) { t.reached = true; t.how = "a step from the ground"; continue; }
                    foreach (var rp in myRamps)
                    {
                        var to = L34Arr(rp, "to"); var fr = L34Arr(rp, "from");
                        double rise = L34N(rp["rise"]);
                        double baseY = rp.ContainsKey("baseY") ? L34N(rp["baseY"]) : 0;
                        bool lowOk = baseY <= 0.01 || tiers.Any(o => o.reached && Math.Abs(o.y - baseY) < 0.3 && Touch(o.rect, L34N(fr[0]), L34N(fr[1]), 0.6));
                        if (lowOk && Math.Abs(baseY + rise - t.y) < 0.35 && Touch(t.rect, L34N(to[0]), L34N(to[1]), 0.6)) { t.reached = true; t.how = "ramp"; break; }
                    }
                    if (t.reached) continue;
                    foreach (var o in tiers)
                        if (o != t && o.reached && t.y - o.y <= 1.3 && t.y > o.y && Near(o.rect, t.rect, 0.3)) { t.reached = true; t.how = "steps"; break; }
                }
            int raised = tiers.Count(t => t.y > 1.3);
            int unreached = tiers.Count(t => !t.reached);
            // ---- the lane: the longest straight 1.4 m strip on the ground or one tier top with nothing in it (no box rising over that floor, no
            //      colliding prop, no ramp, pit, lava or device grate), every 7.5 degrees from every 1 m grid point
            var devs = L34Arr(room, "devices33").Cast<Dictionary<string, object>>().Where(d => d.ContainsKey("rect")).Select(d => { var q = L34Arr(d, "rect"); return new L34Rect(L34N(q[0]), L34N(q[1]), L34N(q[2]), L34N(q[3])); }).ToList();
            var rampRects = myRamps.Select(rp =>
            {
                var to = L34Arr(rp, "to"); var fr = L34Arr(rp, "from"); double w = L34N(rp["width"]) / 2;
                double ax = L34N(fr[0]), az = L34N(fr[1]), bx2 = L34N(to[0]), bz = L34N(to[1]);
                bool alongZ = Math.Abs(bz - az) >= Math.Abs(bx2 - ax);
                return alongZ ? new L34Rect(ax - w, az, bx2 + w, bz) : new L34Rect(ax, az - w, bx2, bz + w);
            }).ToList();
            // a slide tunnel (its low ceiling): no lane runs through it (you slide there, you do not jump)
            var tunnelRects = L34Arr(plan, "tunnels").Cast<Dictionary<string, object>>().Where(Mine).Select(t => { var q = L34Arr(t, "rect"); return new L34Rect(L34N(q[0]), L34N(q[1]), L34N(q[2]), L34N(q[3])); }).ToList();
            var pitRects = myPits.Select(p => { var q = L34Arr(p, "rect"); return new L34Rect(L34N(q[0]), L34N(q[1]), L34N(q[2]), L34N(q[3])); }).ToList();
            var propRects = myProps.Select(p => { var at = L34Arr(p, "at"); var sz = L34Arr(p, "size"); bool turned = Math.Abs(Math.Sin(L34Num(p, "yawDeg", 0) * Math.PI / 180)) > 0.5; double ex = (turned ? L34N(sz[2]) : L34N(sz[0])) / 2, ez = (turned ? L34N(sz[0]) : L34N(sz[2])) / 2; return (new L34Rect(L34N(at[0]) - ex, L34N(at[2]) - ez, L34N(at[0]) + ex, L34N(at[2]) + ez), L34N(at[1]) - L34N(sz[1]) / 2, L34N(at[1]) + L34N(sz[1]) / 2); }).ToList();
            var boxRects = myBoxes.Select(bx => { var mn0 = L34Arr(bx, "min"); var mx0 = L34Arr(bx, "max"); return (new L34Rect(L34N(mn0[0]), L34N(mn0[2]), L34N(mx0[0]), L34N(mx0[2])), L34N(mn0[1]), L34N(mx0[1])); }).ToList();
            double best = 0; string bestAt = "";
            var floorsToScan = new List<(double y, L34Rect area, string name)> { (0, R, "ground") };
            foreach (var t in tiers) floorsToScan.Add((t.y, t.rect, "tier " + Sf(t.y)));
            foreach (var (fy, area, fname) in floorsToScan)
            {
                bool Free(double x, double z)
                {
                    if (!(x > area.x0 + 0.35 && x < area.x1 - 0.35 && z > area.z0 + 0.35 && z < area.z1 - 0.35)) return false;
                    foreach (var (br, b0, b1) in boxRects) if (b1 > fy + 0.3 && b0 < fy + 1.8 && br.In(x, z)) return false;
                    foreach (var (pr, p0, p1) in propRects) if (p1 > fy + 0.3 && p0 < fy + 1.8 && pr.In(x, z)) return false;
                    if (fy <= 0.01)
                    {
                        foreach (var q in rampRects) if (q.In(x, z)) return false;
                        foreach (var q in pitRects) if (q.In(x, z)) return false;
                        foreach (var q in devs) if (q.In(x, z)) return false;
                        foreach (var q in tunnelRects) if (q.In(x, z)) return false;
                    }
                    return true;
                }
                for (double sx = Math.Floor(area.x0) + 0.5; sx < area.x1; sx += 1)
                    for (double sz = Math.Floor(area.z0) + 0.5; sz < area.z1; sz += 1)
                    {
                        if (!Free(sx, sz)) continue;
                        for (int k = 0; k < 24; k++)
                        {
                            double ang = k * Math.PI / 24, dx = Math.Cos(ang), dz = Math.Sin(ang), px = -dz * 0.7, pz = dx * 0.7;
                            double len = 0;
                            for (double s = 0.25; s < 80; s += 0.25)
                            {
                                double x = sx + dx * s, z = sz + dz * s;
                                if (!Free(x, z) || !Free(x + px, z + pz) || !Free(x - px, z - pz)) break;
                                len = s;
                            }
                            if (len > best) { best = len; bestAt = $"{fname} from local ({Sf(sx - (R.x0 + R.x1) / 2)}, {Sf(R.z1 - sz)}) at {k * 7.5:0.#} deg"; }
                        }
                    }
            }
            bool wOk = walls >= 1, nOk = nodesOk >= 1 || lines34NodesOptional, fOk = heights.Count >= 2, lOk = best >= lane - 1e-6, cOk = unreached == 0;
            bool all = wOk && nOk && fOk && lOk && cOk;
            if (!all) short_++;
            Console.WriteLine($"lines34 {(all ? "GREEN" : "RED")} {id} {rid} ({Sf(R.W)} x {Sf(R.D)} m): walls {walls} ({roomWalls} room runs + {inner} inner >= {Sf(ride)} m) {(wOk ? "ok" : "NONE")}; nodes {nodesOk} of {nodes.Count} {(nOk ? "ok" : "NONE")}; floors {heights.Count} ({string.Join(" / ", heights.Select(h => Sf(h)))}) {(fOk ? "ok" : "ONE")}; lane {Sf(best)} m ({bestAt}) {(lOk ? "ok" : "SHORT")}; climb {tiers.Count - unreached} of {tiers.Count} tiers ({raised} over 1.3 m) {(cOk ? "ok" : "MISSING: " + string.Join(", ", tiers.Where(t => !t.reached).Select(t => Sf(t.y) + " m")))}");
        }
        Console.WriteLine($"lines34 {id}: {(short_ == 0 ? "ALL GREEN" : short_ + " RED")} ({fights} fight rooms)");
        return short_ == 0 ? 0 : 1;
    }

    // ============================================================ lines34 walk ============================================================

    /// <summary>NIGHT33's arenas33 walk (walk29's pilot, devices ON) on Design/levels/n34-<name>.json: every playable room cleared in every seed is the gate.</summary>
    static int Lines34Walk(string arg, int seeds)
    {
        string id = Lines34LevelId(arg, "n34");
        string prefix = id.Substring(0, 3), g = id.Substring(4);
        var a = new List<string> { "walk29", g, "--seeds", seeds.ToString(CultureInfo.InvariantCulture) };
        string old = walkPrefix;
        Action<SurvivalSim> oldHook = walkAfterStart;
        walkPrefix = prefix;
        walkAfterStart = sim =>
        {
            sim.Devices33Force(true);
            if (g == "spire" && sim.input is Walk29Pilot w) sim.UseInput(new Spire33Guard(sim, w));
        };
        int code = 0;
        Console.WriteLine($"lines34 walk {id}: walk29's pilot on Design/levels/{id}.json, devices ON, {seeds} seeds");
        try { N29Walk("walk29", a.ToArray(), ref code); }
        finally { walkPrefix = old; walkAfterStart = oldHook; }
        return code;
    }
}
