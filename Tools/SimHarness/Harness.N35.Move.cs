// NIGHT35 builder M (NIGHT35.md M1): the movement builder's harness commands, declared and called by the lead's Program.cs (N35Move).
//   n35move ride [--verbose] [--write]   design.n35.wallride35's before (the row off: the web's ride, NIGHT34's flow34 on as in the game) / after
//                                        (the row's rules) on the real Mover in box worlds (TriangleWorld: the web's collider, no PhysX):
//     ride30   a 30 m wall entered at walk speed with jump held: metres ridden along it, seconds, the end reason, the height lost.
//     jumpoff  a hold ride released after 0.5 s at look pitch -45 / 0 / +45 (before: a press, the web's wall jump): up speed, horizontal speed,
//              the distance out from the wall when the jump is back at its takeoff height and 2 m under it, the height gained.
//     chain    three 10 m walls facing alternate ways (planes 6 m apart, 3 m gaps along): the speed at each ride's start and after the last
//              jump-off (before: a press wall jump at the same point; after: the release, jump held again 0.25 s later).
//     gaps     two facing walls g m apart (g = 3..12): does the jump-off from one (pitch 0, jump held again 0.25 s later) start a hold ride on
//              the other (the chain gaps that work).
//     corners  an outside box corner, an inside corner, a 16-sided pillar of radius 2 m: the degrees the ride turns and its end reason.
//     --write  the numbers to Design/levels/n35-ride-distances.json (builder A spaces rooms from it).
// PASS (ride): after rides the 30 m wall to its end, the chain's last speed beats before, each corner is followed (turn >= 80 deg; the pillar
// >= 180 deg), and before (the row off) is NIGHT34's ride (it ends by its 1.2 s timer on the 30 m wall).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N35Move(string cmd, string[] args, ref int code)
    {
        if (cmd != "n35move") return;
        string sub = args.Length > 1 ? args[1] : "ride";
        if (sub == "ride") code = N35RideCommand(args);
        else if (sub == "pull" || sub == "walk" || sub == "verbs") code = N35MoveMore(sub, args);
        else
        {
            Console.WriteLine("n35move: ride | pull | walk");
            code = 2;
        }
    }

    static partial void N35MoveMorePartial(string sub, string[] args, ref int code);

    static int N35MoveMore(string sub, string[] args)
    {
        int code = 2;
        N35MoveMorePartial(sub, args, ref code);
        return code;
    }

    static bool n35Verbose;
    static int n35VerboseKind = -1;

    static Mover N35Mover(GameConfig cfg, TriangleWorld w, bool after, List<MoverEvent> log)
    {
        var m = new Mover(cfg.movement, w);
        m.SetFlow34(Flow34.Params.From(cfg, cfg.Design("n34.flow34.enabled"))); // NIGHT34's flow as the game runs it
        m.SetRide35(after ? Ride35.Params.From(cfg, true) : Ride35.Params.Neutral);
        m.SetGrip36(n36HarnessGrip); // NIGHT36 M (Harness.N36.Move.cs): neutral unless n36move grip measures these same rides with wallgrip36
        if (log != null) m.Events += e => log.Add(e);
        return m;
    }

    static InputFrame N35In(double moveX, double moveZ, bool held, bool prevHeld, double pitchDeg = 0, double yaw = 0, bool slidePress = false) =>
        new InputFrame
        {
            moveX = moveX,
            moveZ = moveZ,
            yaw = yaw,
            pitch = pitchDeg * Math.PI / 180,
            jump = new Button { held = held, pressed = held && !prevHeld },
            slide = new Button { pressed = slidePress, held = slidePress },
        };

    static string N35Reason(List<MoverEvent> log)
    {
        int i = log.FindLastIndex(e => e.type == MoverEventType.WallRideEnd);
        return i >= 0 ? log[i].reason : "none";
    }

    static TriangleWorld N35Floor()
    {
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-80, -1, -80), new Vec3(80, 0, 80));
        return w;
    }

    /// <summary>ride30: the metres ridden along a 30 m wall (face x = 3, z 0 .. -30), the seconds, the end reason, the height lost.</summary>
    static (double metres, double seconds, string reason, double drop, double speed) N35Ride30(GameConfig cfg, double dt, bool after)
    {
        TriangleWorld w = N35Floor();
        w.AddBox(new Vec3(3, 0, -30), new Vec3(4, 14, 0));
        var log = new List<MoverEvent>();
        Mover m = N35Mover(cfg, w, after, log);
        double r = cfg.movement.body.capsuleRadius;
        m.Reset(3 - r - 0.02, 3, -1);
        m.velocity = new Vec3(1, 0, -8);
        double z0 = double.NaN, y0 = 0, z1 = 0, y1 = 0, sp = 0;
        int steps = 0;
        for (int s = 0; s < 1500; s++)
        {
            m.Step(N35In(0.5, 1, true, true), dt);
            if (m.state == MoveState.WallRide)
            {
                if (double.IsNaN(z0))
                {
                    z0 = m.position.z;
                    y0 = m.position.y;
                }
                if (m.wallNormal.x < -0.99) // still on the first face
                {
                    z1 = m.position.z;
                    y1 = m.position.y;
                    steps++;
                    sp = m.Speed;
                }
            }
            else if (!double.IsNaN(z0)) break;
        }
        if (double.IsNaN(z0)) return (0, 0, "no ride", 0, 0);
        return (z0 - z1, steps * dt, N35Reason(log), y0 - y1, sp);
    }

    /// <summary>jumpoff: a hold ride on a long wall (face x = 3) released after 0.5 s at the pitch; before: a press (the web's wall jump).
    /// Returns up, horizontal speed after, out distance at the takeoff height and 2 m under it, the height gained, the air seconds to the takeoff height.</summary>
    static double[] N35JumpOff(GameConfig cfg, double dt, bool after, double pitchDeg)
    {
        TriangleWorld w = N35Floor();
        w.AddBox(new Vec3(3, 0, -60), new Vec3(4, 30, 20));
        var log = new List<MoverEvent>();
        Mover m = N35Mover(cfg, w, after, log);
        double r = cfg.movement.body.capsuleRadius;
        m.Reset(3 - r - 0.02, 12, 10);
        m.velocity = new Vec3(1, 0, -8);
        int rideAt = -1;
        bool prev = true;
        double up = double.NaN, hs = 0, x0 = 0, y0 = 0, outAt0 = double.NaN, outAtM2 = double.NaN, peak = 0, airT = double.NaN;
        int jumped = -1;
        for (int s = 0; s < 1500; s++)
        {
            bool held = true;
            if (rideAt >= 0 && s >= rideAt + 60) held = !after && s > rideAt + 60; // after: the release; before: the release, then a press (the web's wall jump)
            var f = N35In(0, 1, held, prev, pitchDeg);
            prev = held;
            m.Step(f, dt);
            if (rideAt < 0 && m.state == MoveState.WallRide) rideAt = s;
            if (jumped < 0 && rideAt >= 0 && s > rideAt + 59 && m.state == MoveState.Air && log.Exists(e => e.type == MoverEventType.WallJump))
            {
                jumped = s;
                up = m.velocity.y;
                hs = m.Speed;
                x0 = m.position.x;
                y0 = m.position.y;
                continue;
            }
            if (jumped >= 0)
            {
                peak = Math.Max(peak, m.position.y - y0);
                if (double.IsNaN(outAt0) && m.velocity.y < 0 && m.position.y <= y0)
                {
                    outAt0 = x0 - m.position.x;
                    airT = (s - jumped) * dt;
                }
                if (m.velocity.y < 0 && m.position.y <= y0 - 2)
                {
                    outAtM2 = x0 - m.position.x;
                    break;
                }
                if (m.grounded) break;
            }
        }
        return new[] { up, hs, outAt0, outAtM2, peak, airT };
    }

    /// <summary>chain: three 10 m walls facing alternate ways. The speed at each ride start, and after the last jump-off (0.1 s later).</summary>
    static double[] N35Chain(GameConfig cfg, double dt, bool after, out string trace)
    {
        TriangleWorld w = N35Floor();
        w.AddBox(new Vec3(3, 0, -10), new Vec3(4, 20, 2)); // A: face x = 3 (normal -x)
        w.AddBox(new Vec3(-4, 0, -23), new Vec3(-3, 20, -13)); // B: face x = -3 (normal +x)
        w.AddBox(new Vec3(3, 0, -36), new Vec3(4, 20, -26)); // C
        double[] wallEnd = { -10, -23, -36 };
        var log = new List<MoverEvent>();
        Mover m = N35Mover(cfg, w, after, log);
        double r = cfg.movement.body.capsuleRadius;
        m.Reset(3 - r - 0.02, 6, 1);
        m.velocity = new Vec3(1, 0, -8);
        var res = new double[4];
        for (int i = 0; i < 4; i++) res[i] = double.NaN;
        int wall = 0, releasedAt = -1, lastOff = -1;
        bool prev = true, wasRide = false;
        double side = 1; // the stick toward the wall being approached (+x for A and C)
        var sb = new StringBuilder();
        for (int s = 0; s < 1500 && wall < 3; s++)
        {
            bool ride = m.state == MoveState.WallRide;
            if (ride && !wasRide)
            {
                res[wall] = m.Speed;
                sb.Append($" ride{wall} {m.Speed:0.0}");
            }
            wasRide = ride;
            bool held = true;
            if (ride && m.position.z < wallEnd[wall] + 1.5 && releasedAt < 0)
            {
                releasedAt = s;
                held = false;
            }
            else if (releasedAt >= 0)
            {
                held = after ? s >= releasedAt + 30 : s >= releasedAt + 1; // after: hold again 0.25 s later; before: the press at once (the web's wall jump)
                if (s == releasedAt + 1)
                {
                    side = -side;
                    lastOff = s;
                    wall++;
                    if (wall == 3) { }
                }
                if (s >= releasedAt + 30) releasedAt = -1;
            }
            var f = N35In(side * 0.7, 1, held, prev);
            prev = held;
            m.Step(f, dt);
            if (m.grounded) break;
        }
        // after the third jump-off: the speed 12 steps later
        for (int k = 0; k < 12 && !m.grounded; k++)
        {
            m.Step(N35In(0, 1, false, prev), dt);
            prev = false;
        }
        res[3] = m.Speed;
        sb.Append($" end {m.Speed:0.0}{(m.grounded ? " (grounded)" : "")}");
        trace = sb.ToString();
        return res;
    }

    /// <summary>gaps: two facing walls g m apart (planes), the second starting 2 m past the first's end: does a hold ride start on the second?</summary>
    static bool N35Gap(GameConfig cfg, double dt, double g, out double dropAtEntry)
    {
        TriangleWorld w = N35Floor();
        double h = g / 2;
        w.AddBox(new Vec3(h, 0, -10), new Vec3(h + 1, 20, 2));
        w.AddBox(new Vec3(-h - 1, 0, -40), new Vec3(-h, 20, -10));
        var log = new List<MoverEvent>();
        Mover m = N35Mover(cfg, w, true, log);
        double r = cfg.movement.body.capsuleRadius;
        m.Reset(h - r - 0.02, 8, 1);
        m.velocity = new Vec3(1, 0, -8);
        int released = -1, rides = 0;
        bool prev = true, wasRide = false;
        double y0 = 0;
        dropAtEntry = double.NaN;
        for (int s = 0; s < 1200; s++)
        {
            bool ride = m.state == MoveState.WallRide;
            if (ride && !wasRide)
            {
                rides++;
                if (rides == 2)
                {
                    dropAtEntry = y0 - m.position.y;
                    return m.HoldRide35;
                }
            }
            wasRide = ride;
            bool held = true;
            double stick = 0.5;
            if (released < 0 && ride && m.position.z < -6)
            {
                released = s;
                y0 = m.position.y;
            }
            if (released >= 0)
            {
                held = s >= released + 30;
                stick = -0.7;
            }
            var f = N35In(stick, 1, held, prev);
            prev = held;
            m.Step(f, dt);
            if (m.grounded) return false;
        }
        return false;
    }

    /// <summary>corners: kind 0 outside box corner, 1 inside corner, 2 a 16-sided pillar. The degrees the ride's direction turned and the end reason.</summary>
    static (double turned, string reason, int corners) N35Corner(GameConfig cfg, double dt, bool after, int kind)
    {
        TriangleWorld w = N35Floor();
        double r = cfg.movement.body.capsuleRadius;
        Vec3 start, vel;
        double yaw = 0;
        if (kind == 0)
        {
            w.AddBox(new Vec3(3, 0, -20), new Vec3(13, 14, 0)); // face x = 3 for z in [-20, 0], the corner at z = 0, then the face z = 0 (normal +z)
            start = new Vec3(3 - r - 0.02, 4, -14);
            vel = new Vec3(1, 0, 9);
            yaw = Math.PI;
        }
        else if (kind == 1)
        {
            w.AddBox(new Vec3(3, 0, -40), new Vec3(4, 14, 0)); // face x = 3
            w.AddBox(new Vec3(-12, 0, -31), new Vec3(3, 14, -30)); // across the ride at z = -30 (face normal +z)
            start = new Vec3(3 - r - 0.02, 4, -18);
            vel = new Vec3(1, 0, -9);
        }
        else
        {
            const int sides = 16;
            double R = 2;
            var inside = new Vec3(0, 7, 0);
            for (int i = 0; i < sides; i++)
            {
                double a0 = 2 * Math.PI * i / sides, a1 = 2 * Math.PI * (i + 1) / sides;
                var p0 = new Vec3(R * Math.Cos(a0), 0, R * Math.Sin(a0));
                var p1 = new Vec3(R * Math.Cos(a1), 0, R * Math.Sin(a1));
                var q0 = new Vec3(p0.x, 14, p0.z);
                var q1 = new Vec3(p1.x, 14, p1.z);
                w.AddOutward(p0, p1, q1, inside);
                w.AddOutward(p0, q1, q0, inside);
            }
            start = new Vec3(R + r + 0.02, 4, 1.5);
            vel = new Vec3(-0.5, 0, -9);
        }
        var log = new List<MoverEvent>();
        Mover m = N35Mover(cfg, w, after, log);
        m.Reset(start.x, start.y, start.z);
        m.velocity = vel;
        double turned = 0, lastHeading = double.NaN;
        bool riding = false;
        int s;
        for (s = 0; s < 1200; s++)
        {
            m.Step(N35In(0, 0, true, true, 0, yaw), dt);
            if (n35Verbose && kind == n35VerboseKind && (m.state == MoveState.WallRide || riding)) Console.WriteLine($"      s{s} {m.state} p ({m.position.x:0.000}, {m.position.y:0.00}, {m.position.z:0.000}) v ({m.velocity.x:0.00}, {m.velocity.z:0.00}) n ({m.wallNormal.x:0.00}, {m.wallNormal.z:0.00}) touch {m.touchingWall}");
            if (m.state == MoveState.WallRide)
            {
                riding = true;
                double hd = Math.Atan2(m.velocity.x, m.velocity.z);
                if (!double.IsNaN(lastHeading))
                {
                    double d = hd - lastHeading;
                    while (d > Math.PI) d -= 2 * Math.PI;
                    while (d < -Math.PI) d += 2 * Math.PI;
                    turned += Math.Abs(d);
                }
                lastHeading = hd;
            }
            else if (riding) break;
            if (turned > 2 * Math.PI) break;
        }
        string reason = m.state == MoveState.WallRide ? "still riding" : riding ? N35Reason(log) : "no ride";
        return (turned * 180 / Math.PI, reason, m.flow34.ride35.corners);
    }

    static string F35(double v) => double.IsNaN(v) ? "n/a" : v.ToString("0.00", CultureInfo.InvariantCulture);

    static int N35RideCommand(string[] args)
    {
        n35Verbose = Array.IndexOf(args, "--verbose") >= 0;
        int vk = Array.IndexOf(args, "--corner");
        if (vk >= 0 && vk + 1 < args.Length) n35VerboseKind = int.Parse(args[vk + 1], CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        double dt = 1 / cfg.movement.loop.simHz;
        Ride35.Params p = Ride35.Params.From(cfg, true);
        if (!p.on)
        {
            Console.WriteLine("n35move ride: design.n35.wallride35 has no numbers (Tools/night/n35m-config.cjs writes them)");
            return 1;
        }
        Console.WriteLine($"n35move ride (design.n35.wallride35 enabled {cfg.Design("n35.wallride35.enabled")}; forced on for AFTER: entry {p.entrySpeed} m/s, buff {p.speedBuff}, exit +{p.exitBoost} m/s (cap {p.chainCap}), up {p.upLow}..{p.upHigh} m/s at {p.pitchLowDeg}..{p.pitchHighDeg} deg, safety {p.safetyCap} s, late {p.lateSteps} steps, re-ride {p.reRideDeg} deg / {p.reRideDistance} m)");
        bool pass = true;

        var b = N35Ride30(cfg, dt, false);
        var a = N35Ride30(cfg, dt, true);
        Console.WriteLine($"  ride30   before: {F35(b.metres)} m in {F35(b.seconds)} s, end {b.reason}, height lost {F35(b.drop)} m | after: {F35(a.metres)} m in {F35(a.seconds)} s ({F35(a.speed)} m/s), end {a.reason}, height lost {F35(a.drop)} m");
        bool rideOk = a.metres >= 28 && b.reason == "timeout";
        pass &= rideOk;

        var jo = new Dictionary<double, double[]>();
        foreach (double pitch in new[] { -45.0, 0, 45 })
        {
            double[] jb = N35JumpOff(cfg, dt, false, pitch);
            double[] ja = N35JumpOff(cfg, dt, true, pitch);
            jo[pitch] = ja;
            Console.WriteLine($"  jumpoff pitch {pitch,3}: before (press) up {F35(jb[0])} m/s, h {F35(jb[1])} m/s, out {F35(jb[2])} m at takeoff height ({F35(jb[3])} m at -2 m), peak +{F35(jb[4])} m | after (release) up {F35(ja[0])} m/s, h {F35(ja[1])} m/s, out {F35(ja[2])} m in {F35(ja[5])} s ({F35(ja[3])} m at -2 m), peak +{F35(ja[4])} m");
        }

        double[] cb = N35Chain(cfg, dt, false, out string tb);
        double[] ca = N35Chain(cfg, dt, true, out string ta);
        Console.WriteLine($"  chain    before:{tb} | after:{ta}");
        bool chainOk = ca[3] > cb[3] && !double.IsNaN(ca[2]);
        pass &= chainOk;

        var works = new List<double>();
        var drops = new List<double>();
        var sbg = new StringBuilder();
        for (double g = 3; g <= 12; g += 1)
        {
            bool ok = N35Gap(cfg, dt, g, out double drop);
            if (ok)
            {
                works.Add(g);
                drops.Add(drop);
            }
            sbg.Append($" {g:0}m:{(ok ? "ride" : "miss")}{(ok ? $"({-drop:+0.0;-0.0})" : "")}");
        }
        Console.WriteLine($"  gaps     (facing planes, pitch 0, jump held again 0.25 s after the release; height of the next ride's start vs the release, m):{sbg}");

        string[] names = { "outside corner", "inside corner", "pillar r2 16-gon" };
        double[] need = { 80, 80, 180 };
        var cornerRes = new (double, string, int)[3];
        for (int k = 0; k < 3; k++)
        {
            var cbk = N35Corner(cfg, dt, false, k);
            var cak = N35Corner(cfg, dt, true, k);
            cornerRes[k] = cak;
            bool ok = cak.turned >= need[k];
            pass &= ok;
            Console.WriteLine($"  corners  {names[k]}: before turned {F35(cbk.turned)} deg, end {cbk.reason} | after turned {F35(cak.turned)} deg, end {cak.reason}, corners followed {cak.corners} {(ok ? "" : "RED")}");
        }
        Console.WriteLine($"n35move ride: {(pass ? "PASS" : "FAIL")} (ride30 {(rideOk ? "ok" : "RED")}, chain {(chainOk ? "ok" : "RED")})");

        if (Array.IndexOf(args, "--write") >= 0)
        {
            var sb = new StringBuilder();
            string N(double v) => double.IsNaN(v) ? "null" : v.ToString("0.00", CultureInfo.InvariantCulture);
            sb.Append("{\n");
            sb.Append("  \"_why\": \"NIGHT35 builder M: design.n35.wallride35's distances for room spacing, measured by `dotnet run --project Tools/SimHarness -c Release -- n35move ride --write` on the real Mover (box worlds, 120 Hz, NIGHT34's flow34 on). Not a level file. Numbers are the row's at the time of writing; re-run after a tuning change.\",\n");
            sb.Append($"  \"rideSpeed\": {N(a.speed)},\n");
            sb.Append($"  \"rideSpeed_why\": \"m/s along the wall on a hold ride entered at walk speed (wallRide.minSpeed 8 x 1.3): metres ridden per second held.\",\n");
            sb.Append($"  \"ride30\": {{ \"metres\": {N(a.metres)}, \"seconds\": {N(a.seconds)}, \"heightLost\": {N(a.drop)}, \"end\": \"{a.reason}\", \"before\": {{ \"metres\": {N(b.metres)}, \"seconds\": {N(b.seconds)}, \"end\": \"{b.reason}\" }} }},\n");
            sb.Append($"  \"safetyCapSeconds\": {N(p.safetyCap)},\n");
            sb.Append($"  \"sinkPerSecond\": {N(p.sinkSpeed)},\n");
            sb.Append("  \"jumpOff\": {\n");
            int i = 0;
            foreach (var kv in jo)
            {
                sb.Append($"    \"pitch{kv.Key:0}\": {{ \"up\": {N(kv.Value[0])}, \"speed\": {N(kv.Value[1])}, \"outAtTakeoffHeight\": {N(kv.Value[2])}, \"outAt2mBelow\": {N(kv.Value[3])}, \"peakGain\": {N(kv.Value[4])}, \"airSecondsToTakeoffHeight\": {N(kv.Value[5])} }}{(++i < jo.Count ? "," : "")}\n");
            }
            sb.Append("  },\n");
            sb.Append("  \"jumpOff_why\": \"Metres out from the wall's plane (the capsule's travel) after a release at look pitch -45 / 0 / +45 degrees; speed = horizontal m/s after the exit boost. Along the wall the jump keeps the ride's speed (about speed x airSeconds).\",\n");
            sb.Append($"  \"facingGapsThatWork\": [{string.Join(", ", works.ConvertAll(x => x.ToString("0", CultureInfo.InvariantCulture)))}],\n");
            sb.Append($"  \"facingGapDrops\": [{string.Join(", ", drops.ConvertAll(x => N(x)))}],\n");
            sb.Append("  \"facingGaps_why\": \"Plane-to-plane gaps (m) between two facing walls where a pitch-0 release, jump held again 0.25 s later, starts a hold ride on the second wall (stick toward it); facingGapDrops = metres the second ride starts BELOW the release height (negative: above it; the re-press in the air is the double jump, which lifts it).\",\n");
            sb.Append($"  \"chainSpeeds\": [{N(ca[0])}, {N(ca[1])}, {N(ca[2])}, {N(ca[3])}],\n");
            sb.Append($"  \"chainSpeedsBefore\": [{N(cb[0])}, {N(cb[1])}, {N(cb[2])}, {N(cb[3])}],\n");
            sb.Append("  \"chain_why\": \"Three 10 m walls facing alternate ways, planes 6 m apart, 3 m gaps along: m/s at each ride's start and 0.1 s after the third jump-off (before: the web's press wall jump).\",\n");
            sb.Append($"  \"corners\": {{ \"outsideTurnDeg\": {N(cornerRes[0].Item1)}, \"insideTurnDeg\": {N(cornerRes[1].Item1)}, \"pillarR2TurnDeg\": {N(cornerRes[2].Item1)} }},\n");
            sb.Append($"  \"reRideDistance\": {N(p.reRideDistance)},\n");
            sb.Append("  \"reRide_why\": \"A wall facing within 30 deg of the last ridden one gives no ride until a landing or until the player is this many metres from where that ride ended: alternate the walls' facing in a chain.\"\n");
            sb.Append("}\n");
            string path = Path.Combine("Design", "levels", "n35-ride-distances.json");
            File.WriteAllText(path, sb.ToString());
            Console.WriteLine($"  wrote {path}");
        }
        return pass ? 0 : 1;
    }
}
