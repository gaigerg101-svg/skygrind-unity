// NIGHT34 builder M (design.n34.flow34; NIGHT34.md M1): `flow34`, the techniques' before / after numbers on the real Mover in a box world
// (a TriangleWorld floor and one wall: the web's collider, no PhysX), each run with Flow34.Params.Neutral (BEFORE: the web's mover) and with
// the row's rules (AFTER), called from Harness.N34.Flow.cs:
//   bhop        (table M 2, EXISTS: not rebuilt) a run at 12 m/s, the jump pressed 1..12 steps before the landing: the horizontal speed kept
//               on the hop (the step after the jump / the landing step), min over the presses. Expect 100 % before and after.
//   wallCoyote  a wall ride at 10 m/s, ended by steering away (EndRide "away"); the jump pressed k = 1..16 steps after the ride ended:
//               how many presses are WALL jumps (the rest double jumps). Before: only while the wall is inside the 0.12 m probe.
//   wallBuffer  flying head-on at a wall at 9 m/s with a double jump left, the jump pressed k = 1..14 steps before the contact: how many
//               presses end as a WALL jump with the double jump still in hand.
//   zipCarry    running sideways (+x) at 12 m/s, a ZIP pull (the tap grapple's rope.zip) to a point 14 m ahead (-z), let go by a jump
//               after 20 steps: the sideways speed after the release (before: the pull erased it).
// PASS: each built technique's AFTER beats its BEFORE, and bhop keeps 100 % (the row changes nothing there).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static TriangleWorld Flow34World(bool wall)
    {
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60)); // the floor, top at y = 0
        if (wall) w.AddBox(new Vec3(3, 0, -60), new Vec3(4, 12, 60)); // a wall, its face at x = 3 (normal -x)
        w.AddBox(new Vec3(-60, 0, -40), new Vec3(60, 20, -39)); // the far wall the zip hooks (face at z = -39)
        return w;
    }

    static Mover Flow34Mover(GameConfig cfg, TriangleWorld w, bool after, List<MoverEvent> log)
    {
        var m = new Mover(cfg.movement, w);
        m.SetFlow34(after ? Flow34.Params.From(cfg, true) : Flow34.Params.Neutral);
        if (log != null) m.Events += e => log.Add(e);
        return m;
    }

    static bool flow34Verbose;
    static readonly double[] flow34LurchSpeed = new double[2];
    static readonly string[] flow34Bounce = new string[2];

    /// <summary>wallJumps: two parallel walls 4 m apart (faces at x = -2 and x = 2), high in the air; a jump pressed on every new wall contact:
    /// the wall jumps in one airtime (until the floor).</summary>
    static double Flow34WallJumps(GameConfig cfg, double dt, bool after)
    {
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
        w.AddBox(new Vec3(-3, 0, -60), new Vec3(-2, 40, 60));
        w.AddBox(new Vec3(2, 0, -60), new Vec3(3, 40, 60));
        var log = new List<MoverEvent>();
        Mover m = Flow34Mover(cfg, w, after, log);
        m.Reset(1.2, 20, 0);
        m.velocity = new Vec3(8, 0, 0);
        bool wasNear = false;
        for (int s = 0; s < 720 && !m.grounded; s++)
        {
            bool near = m.touchingWall || m.state == MoveState.WallRide;
            bool press = near && !wasNear;
            wasNear = near;
            m.Step(Flow34In(0, 0, press, press), dt);
        }
        return log.FindAll(e => e.type == MoverEventType.WallJump).Count;
    }

    /// <summary>slideBounce: a slide on the floor heading 45 deg into the wall at x = 3 at about 9.9 m/s; the jump pressed on the first wall
    /// contact: the horizontal speed just after the jump (and the jump's kind, its speed away from the wall).</summary>
    static double Flow34SlideBounce(GameConfig cfg, double dt, bool after)
    {
        TriangleWorld w = Flow34World(true);
        var log = new List<MoverEvent>();
        Mover m = Flow34Mover(cfg, w, after, log);
        m.Reset(0, 0.02, 0);
        for (int s = 0; s < 10; s++) m.Step(Flow34In(0, 0), dt);
        m.velocity = new Vec3(7, 0, -7);
        bool pressed = false;
        double speed = double.NaN;
        string kind = "none";
        for (int s = 0; s < 240; s++)
        {
            bool press = !pressed && m.touchingWall;
            if (press) pressed = true;
            var f = new InputFrame { slide = new Button { held = !pressed, pressed = s == 0 }, jump = new Button { pressed = press, held = press } };
            m.Step(f, dt);
            if (press)
            {
                speed = JsMath.Hypot(m.velocity.x, m.velocity.z);
                MoverEvent j = log.FindLast(e => e.type == MoverEventType.Jump || e.type == MoverEventType.WallJump);
                kind = j.type == MoverEventType.WallJump ? "wall jump" : j.type == MoverEventType.Jump ? j.kind + " jump" : "none";
                kind += $", away from the wall {-m.velocity.x:0.00} m/s, along {-m.velocity.z:0.00} m/s, up {m.velocity.y:0.00}";
                break;
            }
        }
        flow34Bounce[after ? 1 : 0] = kind;
        return speed;
    }

    /// <summary>ridePhase: a full wall ride on a long wall (entered level at 10 m/s, pushing into the wall): the height at the ride's end minus at its start.</summary>
    static double Flow34RidePhase(GameConfig cfg, double dt, bool after)
    {
        TriangleWorld w = Flow34World(true);
        var log = new List<MoverEvent>();
        Mover m = Flow34Mover(cfg, w, after, log);
        m.Reset(3 - 0.45, 4, 40);
        m.velocity = new Vec3(1, 0, -10);
        double y0 = double.NaN, y1 = double.NaN;
        int steps = 0;
        for (int s = 0; s < 480; s++)
        {
            m.Step(Flow34In(1, 1), dt);
            if (double.IsNaN(y0) && m.state == MoveState.WallRide) y0 = m.position.y;
            if (!double.IsNaN(y0) && m.state == MoveState.WallRide) steps++;
            if (!double.IsNaN(y0) && m.state != MoveState.WallRide) { y1 = m.position.y; break; }
        }
        if (flow34Verbose) Console.WriteLine($"    ridePhase {(after ? "after " : "before")}: ride {steps} steps, y {y0:0.000} -> {y1:0.000}, end {log.FindLast(e => e.type == MoverEventType.WallRideEnd).reason}");
        return y1 - y0;
    }

    /// <summary>lurch: a ground jump at about 10 m/s (-z); 6 steps later the input turns 90 deg right (+x): the heading change 30 steps (0.25 s) after the jump.</summary>
    static double Flow34Lurch(GameConfig cfg, double dt, bool after)
    {
        TriangleWorld w = Flow34World(false);
        Mover m = Flow34Mover(cfg, w, after, null);
        m.Reset(0, 0.02, 0);
        for (int s = 0; s < 10; s++) m.Step(Flow34In(0, 1), dt);
        m.velocity = new Vec3(0, m.velocity.y, -10);
        m.Step(Flow34In(0, 1, true, true), dt); // the jump
        double h0 = Math.Atan2(m.velocity.x, -m.velocity.z), s0 = JsMath.Hypot(m.velocity.x, m.velocity.z);
        for (int s = 1; s < 30; s++) m.Step(s < 6 ? Flow34In(0, 1, false, true) : Flow34In(1, 0, false, true), dt);
        double h1 = Math.Atan2(m.velocity.x, -m.velocity.z), s1 = JsMath.Hypot(m.velocity.x, m.velocity.z);
        flow34LurchSpeed[after ? 1 : 0] = 100 * s1 / s0;
        if (flow34Verbose) Console.WriteLine($"    lurch {(after ? "after " : "before")}: heading {h0 * 180 / Math.PI:0.0} -> {h1 * 180 / Math.PI:0.0} deg, speed {s0:0.00} -> {s1:0.00} m/s, lurches {m.flow34.lurches}, grounded {m.grounded}");
        return (h1 - h0) * 180 / Math.PI;
    }

    static int Flow34Command(string[] args)
    {
        flow34Verbose = Array.IndexOf(args, "--verbose") >= 0;
        GameConfig cfg = Config();
        double dt = 1 / cfg.movement.loop.simHz;
        Flow34.Params on = Flow34.Params.From(cfg, true);
        Console.WriteLine($"flow34 (design.n34.flow34; the row forced on here, its sub-switches: wallCoyote {on.wallCoyote} ({on.wallCoyoteTime} s = {on.wallCoyoteSteps} steps), wallBuffer {on.wallBuffer}, zipCarry {on.zipCarry} (carryMult {on.carryMult}), ridePhase {on.ridePhase} (grip {on.rideGripSteps} + ramp {on.rideRampSteps} steps), lurch {on.lurch} (window {on.lurchSteps} steps, >= {on.lurchMinAngleDeg} deg, turn <= {on.lurchMaxTurnDeg} deg, speed x{on.lurchSpeedKeep}))");
        Console.WriteLine("  technique 6 (corner correction, apex hang) EXISTS in the web's mover: collision.cornerCorrect 0.25 (the head's corner slide, Mover.cs) and jump.apexHangMult 0.5 under apexHangSpeed 2 (MovementMath.GravityScale): not rebuilt");
        int bad = 0;
        string only = Opt33(args, "--only", null);
        var res = new Dictionary<string, (double before, double after)>();
        foreach (bool after in new[] { false, true })
        {
            if (only == null || only == "bhop") Acc(res, "bhop", after, Flow34Bhop(cfg, dt, after));
            if (only == null || only == "wallCoyote") Acc(res, "wallCoyote", after, Flow34WallCoyote(cfg, dt, after));
            if (only == null || only == "wallBuffer") Acc(res, "wallBuffer", after, Flow34WallBuffer(cfg, dt, after));
            if (only == null || only == "zipCarry") Acc(res, "zipCarry", after, Flow34Zip(cfg, dt, after));
            if (only == null || only == "ridePhase") Acc(res, "ridePhase", after, Flow34RidePhase(cfg, dt, after));
            if (only == null || only == "lurch") Acc(res, "lurch", after, Flow34Lurch(cfg, dt, after));
            if (only == null || only == "wallJumps") Acc(res, "wallJumps", after, Flow34WallJumps(cfg, dt, after));
            if (only == null || only == "slideBounce") Acc(res, "slideBounce", after, Flow34SlideBounce(cfg, dt, after));
        }
        foreach (var kv in res)
        {
            bool ok = kv.Key == "bhop" ? kv.Value.before >= 99.99 && kv.Value.after >= 99.99 : kv.Key == "wallJumps" ? kv.Value.before == 3 && kv.Value.after == 3 : kv.Value.after > kv.Value.before;
            string unit = kv.Key == "wallJumps" ? "wall jumps chained between two parallel walls in one airtime (wallJump.maxPerAirtime 3: exists, not rebuilt)"
                : kv.Key == "slideBounce" ? $"m/s horizontal after the jump off a wall slid into at 45 deg ({flow34Bounce[0]} -> {flow34Bounce[1]})"
                : kv.Key == "ridePhase" ? "m of height over a full ride (end - start; 0 = level)" : kv.Key == "lurch" ? $"deg of heading change 0.25 s after a 90 deg input change 6 steps into a jump (speed kept {flow34LurchSpeed[0]:0.0} -> {flow34LurchSpeed[1]:0.0} %)" : kv.Key == "bhop" ? "% speed kept (min)" : kv.Key == "zipCarry" ? "m/s sideways after the release" : kv.Key == "wallCoyote" ? "of 16 late presses wall-jump" : "of 14 early presses wall-jump with the double jump kept";
            Console.WriteLine($"  {kv.Key,-11} before {kv.Value.before,7:0.00}  after {kv.Value.after,7:0.00}  {unit}  {(ok ? "PASS" : "RED")}{(kv.Key == "bhop" ? " (exists in the web's mover: not rebuilt)" : "")}");
            if (!ok) bad++;
        }
        Console.WriteLine($"flow34 {(bad == 0 ? "PASS" : "RED")}");
        return bad == 0 ? 0 : 1;
    }

    static void Acc(Dictionary<string, (double, double)> d, string k, bool after, double v)
    {
        d.TryGetValue(k, out var t);
        d[k] = after ? (t.Item1, v) : (v, t.Item2);
    }

    static InputFrame Flow34In(double moveX, double moveZ, bool jumpPress = false, bool jumpHeld = false, bool grapple = false, bool grapplePress = false, GrappleTarget? target = null) =>
        new InputFrame { moveX = moveX, moveZ = moveZ, yaw = 0, jump = new Button { pressed = jumpPress, held = jumpHeld || jumpPress }, grapple = new Button { pressed = grapplePress, held = grapple || grapplePress }, grappleTarget = target };

    /// <summary>bhop: min % of horizontal speed kept from the landing step to the step after the buffered hop.</summary>
    static double Flow34Bhop(GameConfig cfg, double dt, bool after)
    {
        TriangleWorld w = Flow34World(false);
        // the landing step of a plain fall (the control)
        int land = -1;
        {
            Mover m = Flow34Mover(cfg, w, after, null);
            m.Reset(0, 1.2, 0);
            m.velocity = new Vec3(0, 0, -12);
            for (int k = 0; k < 240 && land < 0; k++) { m.Step(Flow34In(0, 1), dt); if (m.grounded) land = k; }
        }
        double worst = 100;
        for (int lead = 1; lead <= 12; lead++)
        {
            Mover m = Flow34Mover(cfg, w, after, null);
            m.Reset(0, 1.2, 0);
            m.velocity = new Vec3(0, 0, -12);
            double atLand = -1, hop = -1;
            for (int k = 0; k < 240; k++)
            {
                bool press = k == land - lead;
                m.Step(Flow34In(0, 1, press, press), dt);
                double hs = JsMath.Hypot(m.velocity.x, m.velocity.z);
                if (k == land) atLand = hs;
                if (k == land + 1) { hop = hs; break; }
            }
            if (atLand > 0) worst = Math.Min(worst, 100 * hop / atLand);
        }
        return worst;
    }

    /// <summary>wallCoyote: a ride along a SHORT wall (z -6 .. 6) at 10 m/s runs off its end (EndRide "lost"): of the presses k = 1..16 steps
    /// after the ride ended, how many were wall jumps. (Steering away from a long wall is not the case: the air drift off it is slow, so the
    /// web's 0.12 m probe already holds the wall jump about 28 steps there; measured with --verbose.)</summary>
    static double Flow34WallCoyote(GameConfig cfg, double dt, bool after)
    {
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
        w.AddBox(new Vec3(3, 0, -6), new Vec3(4, 12, 6)); // a wall 12 m long, its face at x = 3
        int walls = 0;
        for (int k = 1; k <= 16; k++)
        {
            var log = new List<MoverEvent>();
            Mover m = Flow34Mover(cfg, w, after, log);
            m.Reset(3 - 0.45, 3, 5);
            m.velocity = new Vec3(1, 0, -10);
            int ended = -1;
            bool done = false;
            for (int s = 0; s < 480 && !done; s++)
            {
                double mx = 1; // into the wall the whole time: the ride ends where the wall does
                bool press = ended >= 0 && s == ended + k;
                m.Step(Flow34In(mx, 1, press, press), dt);
                if (ended < 0 && log.Exists(e => e.type == MoverEventType.WallRideEnd)) ended = s;
                if (press)
                {
                    bool wj = log.Exists(e => e.type == MoverEventType.WallJump);
                    if (wj) walls++;
                    if (flow34Verbose) Console.WriteLine($"    wallCoyote {(after ? "after " : "before")} k {k,2}: ended at {ended} ({log.Find(e => e.type == MoverEventType.WallRideEnd).reason}), gap to the wall {3 - m.position.x:0.000} m, {(wj ? "WALL" : log.Exists(e => e.type == MoverEventType.DoubleJump) ? "double" : "none")}");
                    done = true;
                }
            }
        }
        return walls;
    }

    /// <summary>wallBuffer: of the presses k = 1..14 steps before the contact, how many end as a wall jump with the double jump kept.</summary>
    static double Flow34WallBuffer(GameConfig cfg, double dt, bool after)
    {
        TriangleWorld w = Flow34World(true);
        // the contact step of a plain flight (the control): from x = -3 toward the wall at 9 m/s, in the air
        int contact = -1;
        {
            var log = new List<MoverEvent>();
            Mover m = Flow34Mover(cfg, w, after, log);
            m.Reset(-3, 8, 0);
            m.velocity = new Vec3(9, 2, 0);
            for (int s = 0; s < 240 && contact < 0; s++) { m.Step(Flow34In(1, 0), dt); if (m.touchingWall || m.state == MoveState.WallRide) contact = s; }
        }
        int good = 0;
        for (int k = 1; k <= 14; k++)
        {
            var log = new List<MoverEvent>();
            Mover m = Flow34Mover(cfg, w, after, log);
            m.Reset(-3, 8, 0);
            m.velocity = new Vec3(9, 2, 0);
            for (int s = 0; s < contact + 20; s++)
            {
                bool press = s == contact - k;
                m.Step(Flow34In(1, 0, press, press), dt);
                if (log.Exists(e => e.type == MoverEventType.WallJump)) break;
            }
            bool wall = log.Exists(e => e.type == MoverEventType.WallJump), dbl = log.Exists(e => e.type == MoverEventType.DoubleJump);
            if (wall && !dbl) good++;
            if (flow34Verbose) Console.WriteLine($"    wallBuffer {(after ? "after " : "before")} k {k,2} (contact at {contact}): {string.Join(" ", log.ConvertAll(e => e.type.ToString()))}");
        }
        if (contact < 0) Console.WriteLine("  wallBuffer: no contact in the control (rig)");
        return good;
    }

    /// <summary>zipCarry: sideways (+x) speed after a jump-release of a ZIP pull begun while running sideways at 12 m/s.</summary>
    static double Flow34Zip(GameConfig cfg, double dt, bool after)
    {
        TriangleWorld w = Flow34World(false);
        Mover m = Flow34Mover(cfg, w, after, null);
        m.ropeEnabled = true;
        m.Reset(0, 4, -24); // in the air (no ground friction on the sideways run while the hook flies)
        m.velocity = new Vec3(12, 0, 0);
        var target = new GrappleTarget { point = new Vec3(0, 3, -39), node = 0 };
        m.Step(Flow34In(0, 0, false, false, true, true, target), dt);
        for (int s = 0; s < 120 && m.rope.phase != RopePhase.Attached; s++) { m.rope.zip = true; m.Step(Flow34In(0, 0, false, false, true), dt); }
        for (int s = 0; s < 20 && m.rope.phase == RopePhase.Attached; s++) { m.rope.zip = true; m.Step(Flow34In(0, 0, false, false, true), dt); }
        m.Step(Flow34In(0, 0, true, true, true), dt); // the jump lets go
        return m.velocity.x;
    }
}
