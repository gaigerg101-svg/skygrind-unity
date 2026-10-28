// NIGHT43 builder M1 round 2 (the lead: GG's G2, "the walls should be slightly closer together so that even though the player is moving quickly,
// the rapid bounce from wall to wall naturally carries them upward"): THE FLUE as L1 built it on port/stage3 (n43-foundry a7: two continuous facing
// walls `half` m off the lane's centre, 12 m tall, from 5 to 25 m past the room's south edge, a deck at 8 m after them) as a box world, climbed by
// L1's own pilot (a copy of Assets/Scripts/Game/Bot/Walk43Foundry.cs Flue43 on port/stage3, unchanged but for the sprint: this bare Mover has no
// stamina rules, so the run-up speed is set to the sprint's 10.5 m/s) on the REAL Mover with the game's ride rules (N43Mover), M1's rows off / on.
//   n43move flue [--persons n] [--half h] [--taps]   40 jittered climbers (L1's seeds 4310 + k): the share that reach the deck, the top heights.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Skygrind.Core;

public static partial class Harness
{
    /// <summary>L1's Flue43 (port/stage3, Walk43Foundry.cs), copied for M1's levers (the climb's controller, not the level).</summary>
    sealed class N43Flue43
    {
        public double cx, z1, half = 2.25, from = 5, to = 25, deckY = 8;
        public double ridePitch = 0.78, releasePitch = 0.78, releaseAfter = 0.35, lean = 0.2, stick = 0.4, jumpLead = 1.0;
        public int repressSteps = 18;
        public bool taps;
        int side = -1, rideAge, released = -1, step, lastTap = -1000, tapUntil = -1;
        bool onWall;
        public int rides;
        public double maxY;
        public enum R { Running, Top, Floor }
        static Button Press(bool held, bool pressed) => new Button { held = held, pressed = pressed };
        static double Toward(Vec3 from, double tx, double tz) => Math.Atan2(-(tx - from.x), -(tz - from.z));
        public void Reset() { side = -1; released = -1; onWall = false; rideAge = 0; }

        public R Drive(Vec3 p, MoveState st, Vec3 wallNormal, bool grounded, double dt, bool touchingWall, out InputFrame f)
        {
            step++;
            double lz = z1 - p.z;
            f = new InputFrame { moveZ = 1 };
            if (p.y > maxY) maxY = p.y;
            if (grounded && (p.y > deckY - 0.6 || lz > to + 1)) return R.Top;
            if (p.y > deckY + 0.15 && lz > to - 2) { f.yaw = 0; f.jump = default; return R.Running; }
            bool riding = st == MoveState.WallRide;
            if (grounded && p.y < 1)
            {
                Reset();
                if (lz > from + 2.5) { f.yaw = Toward(p, cx, z1 - (from - 2.5)); return R.Floor; }
                if (lz < from - 3.5) { f.yaw = Toward(p, cx, z1 - from); return R.Running; }
                f.yaw = Toward(p, cx - 0.6, z1 - (from + 3));
                f.sprint = Press(true, false);
                if (lz >= from - jumpLead) { f.jump = Press(true, true); tapUntil = step + 30; lastTap = step; }
                return R.Running;
            }
            if (taps)
            {
                f.pitch = ridePitch;
                bool near = touchingWall || riding;
                f.yaw = Toward(p, cx + side * (half + 1), z1 - (lz + 2));
                f.moveX = side * 0.6;
                if (near && step - lastTap > MovementMath.StepsFor(0.15, dt)) { f.jump = Press(true, true); tapUntil = step + MovementMath.StepsFor(0.1, dt); lastTap = step; side = -side; }
                else f.jump = Press(step < tapUntil, false);
                return R.Running;
            }
            if (riding && !onWall) { onWall = true; rides++; rideAge = 0; released = -1; side = wallNormal.x > 0 ? -1 : 1; }
            if (onWall && !riding) { onWall = false; if (released < 0) released = step; side = -side; }
            if (riding)
            {
                rideAge++;
                f.yaw = side < 0 ? lean : -lean;
                f.moveX = side * stick;
                f.pitch = ridePitch;
                if (rideAge * dt >= releaseAfter || lz >= to - 1.0)
                {
                    f.jump = Press(false, false);
                    released = step;
                    f.pitch = releasePitch;
                    f.yaw = Toward(p, cx - side * (half + 1), z1 - (lz + 2.5));
                    return R.Running;
                }
                f.jump = Press(true, false);
                return R.Running;
            }
            f.yaw = Toward(p, cx + side * (half + 1), z1 - (lz + 2.5));
            f.pitch = releasePitch;
            if (released < 0) { f.jump = Press(true, false); return R.Running; }
            int since = step - released;
            f.jump = since < repressSteps ? Press(false, false) : Press(true, since == repressSteps);
            return R.Running;
        }
    }

    static TriangleWorld N43FlueWorld(double half)
    {
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-20, -1, -60), new Vec3(20, 0, 20));
        w.AddBox(new Vec3(-half - 1, 0, -25), new Vec3(-half, 12, -5)); // west wall, lz 5..25
        w.AddBox(new Vec3(half, 0, -25), new Vec3(half + 1, 12, -5)); // east wall
        w.AddBox(new Vec3(-half - 1, 0, -32), new Vec3(half + 1, 8, -25)); // the deck at 8 m after the walls
        return w;
    }

    /// <summary>One FLUE run set: `persons` of L1's jittered climbers; returns (reached, median top).</summary>
    static (int reached, double medTop, double medTime) N43FlueRun(GameConfig cfg, double half, bool taps, bool rows, int persons, Action<Mover> tweak = null, int traceK = -1)
    {
        double dt = 1 / cfg.movement.loop.simHz;
        var w = N43FlueWorld(half);
        int top = 0;
        var tops = new List<double>();
        var times = new List<double>();
        for (int k = 0; k < persons; k++)
        {
            var r = new Mulberry32((uint)(4310 + k));
            double U(double a, double b) => a + (b - a) * r.Next();
            var fc = new N43Flue43 { cx = 0, z1 = 0, half = half, taps = taps, ridePitch = U(0.4, 0.95), releasePitch = U(0.4, 0.95), releaseAfter = U(0.3, 0.6), lean = U(0.1, 0.3), jumpLead = U(0.6, 1.4), repressSteps = 10 + (int)(r.Next() * 20) };
            Mover m = N43Mover(cfg, w, rows, rows, null);
            tweak?.Invoke(m);
            m.Reset(0, 0.05, -1.5);
            int max = MovementMath.StepsFor(10, dt), i2 = 0;
            bool reached = false;
            for (; i2 < max; i2++)
            {
                N43Flue43.R res = fc.Drive(m.position, m.state, m.wallNormal, m.grounded, dt, m.touchingWall, out InputFrame f);
                if (res == N43Flue43.R.Top) { reached = true; break; }
                if (res == N43Flue43.R.Floor && i2 > 60) break;
                if (f.sprint.held && m.grounded && m.Speed < 10.5 && m.state == MoveState.Ground) // the sprint (no stamina rules in this bare Mover)
                {
                    double yawF = f.yaw;
                    var dir = MovementMath.WishDirection(f.moveX, f.moveZ, yawF);
                    double sp = Math.Min(10.5, m.Speed + 10.5 * dt * 10);
                    m.velocity = new Vec3(dir.x * sp, m.velocity.y, dir.z * sp);
                }
                m.Step(f, dt);
                if (k == traceK && i2 % 12 == 0) Console.WriteLine($"      {i2 * dt:0.00} s lz {-m.position.z:0.0} x {m.position.x:0.00} y {m.position.y:0.00} vy {m.velocity.y:0.00} {m.state} jump {(f.jump.pressed ? "PRESS" : f.jump.held ? "held" : "-")} wj {m.wallJumpsLeft}");
            }
            if (reached) { top++; times.Add(i2 * dt); }
            tops.Add(fc.maxY);
        }
        tops.Sort();
        times.Sort();
        return (top, tops[tops.Count / 2], times.Count > 0 ? times[times.Count / 2] : double.NaN);
    }

    static int N43Flue(GameConfig cfg, string[] args)
    {
        int persons = 40, traceK = -1;
        var halves = new List<double> { 2.25 };
        for (int k = 2; k + 1 < args.Length; k++)
        {
            if (args[k] == "--persons") persons = int.Parse(args[k + 1]);
            if (args[k] == "--half") halves = args[k + 1].Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToList();
            if (args[k] == "--tracek") traceK = int.Parse(args[k + 1]);
        }
        // what-ifs on the AFTER side: --chain up,out,window (chimney43's chain), --reach m (ride40's sideways draw reach with jump held)
        double[] chain = null;
        double reach = double.NaN;
        for (int k = 2; k + 1 < args.Length; k++)
        {
            if (args[k] == "--chain") chain = args[k + 1].Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
            if (args[k] == "--reach") reach = double.Parse(args[k + 1], CultureInfo.InvariantCulture);
        }
        Action<Mover> tweak = m =>
        {
            if (chain != null)
            {
                m.m43.p.chainUp = chain[0];
                m.m43.p.chainOut = chain[1];
                m.m43.p.chainWindow = chain[2];
                m.m43.p.chainSteps = MovementMath.StepsFor(chain[2], 1.0 / 120);
            }
            if (!double.IsNaN(reach)) m.m43.p.holdReach = reach;
        };
        Console.WriteLine($"n43move flue: THE FLUE as a box (walls 12 m tall from 5 to 25 m along, a deck at 8 m), L1's Flue43 pilot (copied), {persons} climbers, the real Mover{(chain != null ? $"; what-if chain up {F42(chain[0])} out {F42(chain[1])} window {F42(chain[2])}" : "")}{(!double.IsNaN(reach) ? $"; what-if hold reach {F42(reach)}" : "")}");
        foreach (double half in halves)
            foreach (bool taps in new[] { false, true })
            {
                var b = N43FlueRun(cfg, half, taps, false, persons, null, traceK);
                var a = N43FlueRun(cfg, half, taps, true, persons, tweak, traceK);
                Console.WriteLine($"  gap {F42(2 * half)} m {(taps ? "taps " : "held ")}: before {b.reached}/{persons} (top median {F42(b.medTop)} m) | after {a.reached}/{persons} (top median {F42(a.medTop)} m, time {F42(a.medTime)} s)");
            }
        return 0;
    }
}
