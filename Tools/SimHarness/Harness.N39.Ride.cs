// NIGHT39 builder R1 (NIGHT39.md R1): ride39's harness commands, declared and called by the lead's Program.cs (N39Ride).
//   n39ride chain   BEFORE (wallride35's numbers: NIGHT38's ride) against AFTER (design.n39.ride39's overrides, Ride35.Params.With39) on the real
//                   Mover in a box world (TriangleWorld: the web's collider, no PhysX), flow34 / wallgrip36 / ride38 as the config has them:
//     chain   six 30 m walls facing alternate ways (planes 8 m apart, THE GANTRY's lx +-4), met at the run speed with jump held; each ride let go
//             2 m before its wall's end at the pitch (0 and +30 deg: the walk bot's release under THE GANTRY's ceiling), the jump re-pressed 0.25 s
//             later. Per wall: the horizontal speed at the ride's start and at its jump-off (m/s), wall by wall.
//     jump    the jump-off off one 60 m wall at pitch -45 / 0 / +30 / +45: the up speed (m/s) and the rise above the release (m) (GG: "the jump
//             height should be increased slightly").
//   n39ride gates <plan.json> [checker args]   builder E's `n38 gates` with ride39 forced ON (--off: OFF).
//   n39ride walk  [n38f walk args]              builder F's `n38f walk` with ride39 forced ON (--off: OFF).
//   n39ride walk36 <level> [n36move walk args]  NIGHT36's level walk (n36move walk: every room, wallgrip36 forced on) with ride39 forced ON (--off: OFF).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N39Ride(string cmd, string[] args, ref int code)
    {
        if (cmd != "n39ride") return;
        string sub = args.Length > 1 ? args[1] : "chain";
        bool off = Array.IndexOf(args, "--off") >= 0;
        var rest = new List<string>();
        for (int k = 2; k < args.Length; k++) if (args[k] != "--off") rest.Add(args[k]);
        if (sub == "chain") code = N39ChainCommand();
        else if (sub == "verbs9") code = N39Verbs9(rest.Count > 0 ? rest[0] : "n38-span", rest.Contains("--noride-off"), rest.Contains("--ride39"));
        else if (sub == "gates" || sub == "walk" || sub == "walk36")
        {
            SurvivalSim.Force39Ride = !off;
            double? Opt(string k) { int i = rest.IndexOf(k); if (i < 0 || i + 1 >= rest.Count) return null; double v = double.Parse(rest[i + 1], CultureInfo.InvariantCulture); rest.RemoveRange(i, 2); return v; }
            Walk38Pilot.Probe39Rel = Opt("--rel");
            Walk38Pilot.Probe39Last = Opt("--last");
            Walk38Pilot.Probe39Pitch = Opt("--pitch");
            double? rp = Opt("--repress");
            Walk38Pilot.Probe39Repress = rp.HasValue ? (int?)(int)rp.Value : null;
            try
            {
                Console.WriteLine($"n39ride {sub}: ride39 forced {(off ? "OFF" : "ON")}");
                var a = new List<string> { sub == "gates" ? "n38" : sub == "walk36" ? "n36move" : "n38f", "walk" };
                if (sub == "gates") a[1] = "gates";
                a.AddRange(rest);
                string[] aa = a.ToArray();
                code = sub == "gates" ? N38GatesCommand(aa) : sub == "walk36" ? N36WalkCommand(aa) : N39RunN38f(aa);
            }
            finally
            {
                SurvivalSim.Force39Ride = null;
            }
        }
        else
        {
            Console.WriteLine("n39ride chain | gates <plan.json> [--off] | walk [args] [--off]");
            code = 2;
        }
    }

    static int N39RunN38f(string[] a)
    {
        int c = -1;
        N38Foundry("n38f", a, ref c);
        return c;
    }

    static Mover N39Mover(GameConfig cfg, TriangleWorld w, bool after, List<MoverEvent> log)
    {
        var m = new Mover(cfg.movement, w);
        m.SetFlow34(Flow34.Params.From(cfg, cfg.Design("n34.flow34.enabled")));
        Ride35.Params p = Ride35.Params.From(cfg, true);
        m.SetRide35(after ? p.With39(cfg) : p);
        m.SetGrip36(Grip36.Params.From(cfg, cfg.Design("n36.wallgrip36.enabled")));
        m.SetRide38(Ride38.Params.From(cfg, cfg.Design("n38.ride38.enabled")));
        if (log != null) m.Events += e => log.Add(e);
        return m;
    }

    const int N39Walls = 6;
    const double N39WallLen = 30, N39Pitch = 28, N39Plane = 4;

    /// <summary>The chain: per wall the speed at the ride's start and at its jump-off (NaN: not reached); the trace.</summary>
    static (double[] start, double[] off, string trace) N39Chain(GameConfig cfg, bool after, double pitchDeg)
    {
        double dt = cfg.Dt;
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-40, -1, -N39Walls * N39Pitch - 60), new Vec3(40, 0, 20));
        var ends = new double[N39Walls];
        for (int k = 0; k < N39Walls; k++)
        {
            double z1 = -k * N39Pitch + 2, z0 = z1 - N39WallLen;
            ends[k] = z0;
            if (k % 2 == 0) w.AddBox(new Vec3(N39Plane, 0, z0), new Vec3(N39Plane + 1, 20, z1)); // face x = +4 (normal -x)
            else w.AddBox(new Vec3(-N39Plane - 1, 0, z0), new Vec3(-N39Plane, 20, z1)); // face x = -4 (normal +x)
        }
        var log = new List<MoverEvent>();
        Mover m = N39Mover(cfg, w, after, log);
        double r = cfg.movement.body.capsuleRadius;
        m.Reset(N39Plane - r - 0.02, 2.5, 1);
        m.velocity = new Vec3(1, 0, -8);
        var start = new double[N39Walls];
        var off = new double[N39Walls];
        for (int k = 0; k < N39Walls; k++) start[k] = off[k] = double.NaN;
        int wall = 0, releasedAt = -1;
        bool prev = true, wasRide = false;
        double side = 1;
        var sb = new StringBuilder();
        for (int s = 0; s < 6000 && wall < N39Walls; s++)
        {
            bool ride = m.state == MoveState.WallRide;
            if (ride && wasRide && double.IsNaN(start[wall])) start[wall] = m.flow34.ride35.rideSpeed; // the ride's own speed along the wall (its first ride step)
            wasRide = ride;
            bool held = true;
            double pitch = 0;
            if (ride && m.position.z < ends[wall] + 2 && releasedAt < 0)
            {
                releasedAt = s;
                held = false;
                pitch = pitchDeg;
            }
            else if (releasedAt >= 0)
            {
                held = s >= releasedAt + 30;
                if (s == releasedAt + 1)
                {
                    off[wall] = m.flow34.ride35.lastSpeed;
                    side = -side;
                    wall++;
                }
                if (s >= releasedAt + 30) releasedAt = -1;
            }
            m.Step(N35In(side * 0.7, 1, held, prev, pitch), dt);
            prev = held;
            if (m.grounded && s > 30) break;
        }
        for (int k = 0; k < N39Walls; k++) sb.Append($" w{k + 1} {N39F(start[k])}>{N39F(off[k])}");
        if (m.grounded) sb.Append(" (landed)");
        return (start, off, sb.ToString());
    }

    static string N39F(double v) => double.IsNaN(v) ? "-" : v.ToString("0.0", CultureInfo.InvariantCulture);

    /// <summary>The jump-off at a pitch off one 60 m wall: the up speed and the rise above the release height.</summary>
    static (double up, double rise, double speed) N39Jump(GameConfig cfg, bool after, double pitchDeg)
    {
        double dt = cfg.Dt;
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-40, -1, -200), new Vec3(40, 0, 20));
        w.AddBox(new Vec3(N39Plane, 0, -60), new Vec3(N39Plane + 1, 40, 2));
        Mover m = N39Mover(cfg, w, after, null);
        double r = cfg.movement.body.capsuleRadius;
        m.Reset(N39Plane - r - 0.02, 3, 1);
        m.velocity = new Vec3(1, 0, -8);
        bool prev = true, rode = false;
        double y0 = double.NaN, top = double.MinValue, up = double.NaN, sp = double.NaN;
        for (int s = 0; s < 3000; s++)
        {
            bool ride = m.state == MoveState.WallRide;
            if (ride) rode = true;
            bool held = !(rode && s > 120);
            bool release = rode && double.IsNaN(y0) && !held;
            m.Step(N35In(0.3, 1, held, prev, release || !double.IsNaN(y0) ? pitchDeg : 0), dt);
            prev = held;
            if (release && m.state != MoveState.WallRide)
            {
                y0 = m.position.y;
                up = m.flow34.ride35.lastUp;
                sp = m.flow34.ride35.lastSpeed;
            }
            if (!double.IsNaN(y0)) top = Math.Max(top, m.position.y);
            if (!double.IsNaN(y0) && m.grounded) break;
        }
        return (up, top - y0, sp);
    }

    /// <summary>The campaign boxes (index, zone, kind, noride, the why's head) that hold a point (grown by skin).</summary>
    static string N39BoxesAt(SurvivalMap map, Vec3 q, double skin)
    {
        var sb = new StringBuilder();
        if (map.json == null || !(map.json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp) || !(camp.TryGetValue("boxes", out object bo) && bo is List<object> boxes)) return "(no campaign boxes)";
        for (int i = 0; i < boxes.Count; i++)
        {
            if (!(boxes[i] is Dictionary<string, object> b)) continue;
            Vec3 mn = PortData.V3(b["min"]), mx = PortData.V3(b["max"]);
            if (q.x < mn.x - skin || q.x > mx.x + skin || q.y < mn.y - skin || q.y > mx.y + skin || q.z < mn.z - skin || q.z > mx.z + skin) continue;
            string why = b.TryGetValue("why", out object w) ? w.ToString() : "";
            if (why.Length > 70) why = why.Substring(0, 70) + "...";
            sb.Append($" [box {i} zone {(b.TryGetValue("zone", out object z) ? z : "-")} kind {(b.TryGetValue("kind", out object k) ? k : "-")} noride {(b.TryGetValue("noride", out object nr) && nr is bool nb && nb)} min ({mn.x}, {mn.y}, {mn.z}) max ({mx.x}, {mx.y}, {mx.z}): {why}]");
        }
        return sb.Length > 0 ? sb.ToString() : " (no campaign box: a room wall or another solid)";
    }

    /// <summary>`n39ride verbs9 <level-id> [--noride-off] [--ride39]`: the verbs37 reel's rows 9 / 11 (FeelReel.N35.Verbs.cs) re-performed headless on a
    /// campaign level: the wall the reel picks (PWall = its Wall35), the solid behind it (the campaign box, its noride flag), then the reel's ride (11 m/s
    /// along the wall, jump pressed and held, stick forward and half into the wall) with every ride's start / end / reason and, on a corner end, the
    /// contacts that ended it. --noride-off: the level's noride boxes taken off the Mover (Mover.SetNoRide38(null)).</summary>
    static int N39Verbs9(string id, bool norideOff, bool ride39)
    {
        GameConfig cfg = Config();
        string path = System.IO.Path.Combine("Design", "levels", id + ".json");
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(System.IO.Path.GetFullPath(path))));
        if (ride39) SurvivalSim.Force39Ride = true;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var si = new ScriptedInput();
        sim.UseInput(si);
        sim.StartRun(1, null, null);
        SurvivalSim.Force39Ride = null;
        var p = new Probe37 { sim = sim, si = si };
        Mover m = sim.mover;
        var log = new List<(double t, MoverEvent e)>();
        m.Events += e => { p.mv[e.type] = p.M(e.type) + 1; log.Add((sim.simStep * sim.step, e)); };
        sim.husks.KillAll();
        sim.husks.frozen = true;
        int noRideCount = m.flow34.ride35.noRide != null ? m.flow34.ride35.noRide.Length : 0;
        if (norideOff) m.SetNoRide38(null);
        Console.WriteLine($"n39ride verbs9 {id}: level {sim.campaignLevel?.id}, n35 {sim.n35}, n38 {sim.n38}, n39 {sim.n39}; noride boxes on the Mover {noRideCount}{(norideOff ? " (taken off: --noride-off)" : "")}");
        if (!PWall(sim, out Vec3 wStart, out Vec3 wAlong, out Vec3 wNormal, out double wLen, out string wRoom)) { Console.WriteLine("  no wall >= 8 m (row 9 RED as the reel)"); return 1; }
        Vec3 face = new Vec3(wStart.x - wNormal.x * 0.75, wStart.y + 1.0, wStart.z - wNormal.z * 0.75);
        Console.WriteLine($"  the reel's wall: {wLen} m in room {wRoom}, start ({wStart.x:0.00}, {wStart.y:0.00}, {wStart.z:0.00}), along ({wAlong.x:0.00}, {wAlong.z:0.00}), normal ({wNormal.x:0.00}, {wNormal.z:0.00})");
        Console.WriteLine($"  the solid at the start (1 m up, 0.05 m inside the face):{N39BoxesAt(map, face, 0.06)}");
        Vec3 mid = new Vec3(face.x + wAlong.x * wLen / 2, face.y, face.z + wAlong.z * wLen / 2);
        Console.WriteLine($"  the solid at the wall's middle ({wLen / 2:0} m along):{N39BoxesAt(map, mid, 0.06)}");
        double ry = PYaw(wAlong);
        Vec3 rightV = MovementMath.WishDirection(1, 0, ry);
        double into = rightV.x * wNormal.x + rightV.z * wNormal.z > 0 ? -1 : 1;
        p.Reset(wStart, ry);
        log.Clear();
        double t0 = sim.simStep * sim.step, rideHold = Math.Max(1.3, (wLen - 3) / 11.7);
        double startT = double.NaN, endT = double.NaN;
        bool released = false;
        var fHits = typeof(Mover).GetField("wallHits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fCount = typeof(Mover).GetField("wallHitCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fNormal = typeof(Mover).GetField("rideNormal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var ends = new StringBuilder();
        int n = MovementMath.StepsFor(rideHold + 1.2, sim.step);
        for (int i = 0; i < n; i++)
        {
            double t = i * sim.step;
            if (t < 0.05) m.velocity = new Vec3(wAlong.x * 11 - wNormal.x * 0.5, m.velocity.y, wAlong.z * 11 - wNormal.z * 0.5);
            double now = sim.simStep * sim.step;
            double onWall = double.IsNaN(startT) ? t : now - startT;
            bool off = !double.IsNaN(endT) && now - endT > 0.15;
            bool hold = !released && onWall < rideHold && !off;
            if (!hold) released = true;
            si.SetLook(ry, 0);
            si.Set(new InputFrame { moveZ = 1, moveX = released ? 0 : into * 0.5, jump = new Button { pressed = t < 0.02, held = hold } }, new CombatInput { weapon = -1 });
            Vec3 rn = (Vec3)fNormal.GetValue(m);
            int c0 = log.Count;
            m.invulnerable = true;
            sim.Step();
            for (int k = c0; k < log.Count; k++)
            {
                var (et, e) = log[k];
                if (e.type == MoverEventType.WallRideStart) { if (double.IsNaN(startT)) startT = et; ends.Append($"\n    start {et - t0:0.00} s at ({m.position.x:0.00}, {m.position.y:0.00}, {m.position.z:0.00}) speed {e.speed:0.0}"); }
                if (e.type == MoverEventType.WallRideEnd)
                {
                    endT = et;
                    ends.Append($"\n    end {et - t0:0.00} s '{e.reason}' at ({m.position.x:0.00}, {m.position.y:0.00}, {m.position.z:0.00}), ride normal ({rn.x:0.00}, {rn.y:0.00}, {rn.z:0.00})");
                    if (e.reason == "corner")
                    {
                        var hits = (Vec3[])fHits.GetValue(m);
                        int hc = (int)fCount.GetValue(m);
                        for (int h = 0; h < hc; h++)
                        {
                            Vec3 hn = hits[h];
                            Vec3 mc = new Vec3(m.position.x, m.position.y + 0.9, m.position.z);
                            string solid = "";
                            foreach (double yy in new[] { 0.1, 0.5, 0.9, 1.4 })
                                if (sim.world.RayIntersect(new Vec3(m.position.x, m.position.y + yy, m.position.z), new Vec3(-hn.x, -hn.y, -hn.z), out RayHit hh) && hh.distance < 1.5)
                                {
                                    Vec3 q = new Vec3(m.position.x - hn.x * (hh.distance + 0.05), m.position.y + yy - hn.y * (hh.distance + 0.05), m.position.z - hn.z * (hh.distance + 0.05));
                                    solid = $" (ray at +{yy} m: {hh.distance:0.00} m, point ({q.x:0.00}, {q.y:0.00}, {q.z:0.00}){N39BoxesAt(map, q, 0.06)})";
                                    break;
                                }
                            ends.Append($"\n      contact normal ({hn.x:0.00}, {hn.y:0.00}, {hn.z:0.00}) dot ride normal {hn.x * rn.x + hn.y * rn.y + hn.z * rn.z:0.00}{solid}");
                        }
                    }
                }
            }
        }
        Console.WriteLine($"  the reel's ride (11 m/s along, jump held, planned release {rideHold:0.0} s after the first start): rides {p.M(MoverEventType.WallRideStart)}{ends}");
        return 0;
    }

    static int N39ChainCommand()
    {
        GameConfig cfg = Config();
        Ride35.Params b = Ride35.Params.From(cfg, true), a = b.With39(cfg);
        Console.WriteLine($"n39ride chain (design.n39.ride39 enabled {cfg.Design("n39.ride39.enabled")}; flow34 {cfg.Design("n34.flow34.enabled")}, wallgrip36 {cfg.Design("n36.wallgrip36.enabled")}, ride38 {cfg.Design("n38.ride38.enabled")})");
        Console.WriteLine($"  numbers  before: buff {b.speedBuff} (floor {cfg.movement.wallRide.minSpeed * (1 + b.speedBuff):0.00} m/s), exit +{b.exitBoost}, cap {b.chainCap}, up {b.upLow}..{b.upHigh}");
        Console.WriteLine($"           after:  buff {a.speedBuff} (floor {cfg.movement.wallRide.minSpeed * (1 + a.speedBuff):0.00} m/s), exit +{a.exitBoost}, cap {a.chainCap}, up {a.upLow}..{a.upHigh}");
        bool pass = true;
        foreach (double pitch in new[] { 0.0, 30.0 })
        {
            var cb = N39Chain(cfg, false, pitch);
            var ca = N39Chain(cfg, true, pitch);
            Console.WriteLine($"  chain p{pitch:0}  start>jump-off per wall, m/s");
            Console.WriteLine($"    before:{cb.trace}");
            Console.WriteLine($"    after: {ca.trace}");
            // PASS: every wall reached in both; after gains less per wall than before (the third wall's start slower), and the chain still builds
            bool reached = !double.IsNaN(ca.start[N39Walls - 1]) && !double.IsNaN(cb.start[N39Walls - 1]);
            bool slower = ca.start[2] < cb.start[2] - 1e-9;
            bool builds = ca.off[N39Walls - 1] > ca.start[0] + 1e-9;
            Console.WriteLine($"    {(reached ? "all walls reached" : "RED a wall missed")}; third wall {N39F(cb.start[2])} -> {N39F(ca.start[2])} m/s {(slower ? "(slower)" : "RED (not slower)")}; after still builds {N39F(ca.start[0])} -> {N39F(ca.off[N39Walls - 1])} {(builds ? "" : "RED")}");
            pass &= reached && slower && builds;
        }
        Console.WriteLine("  jump-off (one 60 m wall): pitch: up m/s, rise m, before | after");
        foreach (double pitch in new[] { -45.0, 0.0, 30.0, 45.0 })
        {
            var jb = N39Jump(cfg, false, pitch);
            var ja = N39Jump(cfg, true, pitch);
            bool higher = pitch < 0 ? ja.rise >= jb.rise - 1e-9 : ja.rise > jb.rise;
            Console.WriteLine($"    {pitch,4:0}: {jb.up:0.00} m/s {jb.rise:0.00} m | {ja.up:0.00} m/s {ja.rise:0.00} m {(higher ? "" : "RED (not higher)")}");
            pass &= higher;
        }
        Console.WriteLine($"n39ride chain: {(pass ? "PASS" : "FAIL")}");
        return pass ? 0 : 1;
    }
}
