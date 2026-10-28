// NIGHT35 builder M: `n35move verbs [--h wallHeight]` T's verbs35 rows 9 (wall ride), 11 (jump-off), 13 (slide bounce) re-performed on the real Mover
// in a box world with the reel's exact inputs (FeelReel.N35.Verbs.cs :338-425; the look yaw is the frame's, as the game's look sets it), before (the
// row off) / after (wallride35 on): a 30 m wall, its face at x = 3 (normal -x), `--h` m tall (default 4).
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;

public static partial class Harness
{
    static string N35VerbRide(GameConfig cfg, double dt, bool after, double h)
    {
        TriangleWorld w = N35Floor();
        w.AddBox(new Vec3(3, 0, -40), new Vec3(4, h, 0));
        var log = new List<MoverEvent>();
        Mover m = N35Mover(cfg, w, after, log);
        var wNormal = new Vec3(-1, 0, 0);
        var wAlong = new Vec3(0, 0, -1);
        var start = new Vec3(3 - 0.7, 0.02, -3);
        m.Reset(start.x, start.y, start.z);
        double yaw = Math.Atan2(-wAlong.x, -wAlong.z);
        for (int s = 0; s < 42; s++) m.Step(new InputFrame { yaw = yaw }, dt);
        double wLen = 30, rideHold = Math.Min(4.0, wLen / 9.0 + 0.6);
        double t = 0, rideStart = double.NaN, rideEnd = double.NaN, offSpeed = 0, releasedAt = -1, rideSpeed = 0, maxY = 0;
        string reason = "none";
        bool released = false;
        int n0 = 0;
        for (int s = 0; t < rideHold + 0.8; s++, t += dt)
        {
            if (t < 0.05) m.velocity = new Vec3(wAlong.x * 11 - wNormal.x * 0.5, m.velocity.y, wAlong.z * 11 - wNormal.z * 0.5);
            bool hold = t < rideHold;
            if (!hold && !released) { released = true; releasedAt = t; }
            m.Step(new InputFrame { moveZ = 1, yaw = yaw, jump = new Button { pressed = t < 0.02, held = hold } }, dt);
            maxY = Math.Max(maxY, m.position.y);
            for (int i = n0; i < log.Count; i++)
            {
                if (log[i].type == MoverEventType.WallRideStart && double.IsNaN(rideStart)) { rideStart = t; rideSpeed = log[i].speed; }
                if (log[i].type == MoverEventType.WallRideEnd && !double.IsNaN(rideStart) && double.IsNaN(rideEnd)) { rideEnd = t; reason = log[i].reason; }
            }
            n0 = log.Count;
            if (released && t - releasedAt > 0.05 && t - releasedAt < 0.25) offSpeed = Math.Max(offSpeed, m.velocity.Length);
        }
        double rideS = double.IsNaN(rideStart) ? 0 : (double.IsNaN(rideEnd) ? rideHold : rideEnd) - rideStart;
        return $"ride start {F35(rideStart)} s, rode {F35(rideS)} s (end '{reason}'), top {F35(maxY)} m, ride {F35(rideSpeed)} m/s, off speed {F35(offSpeed)} m/s";
    }

    static string N35VerbBounce(GameConfig cfg, double dt, bool after, double h)
    {
        TriangleWorld w = N35Floor();
        w.AddBox(new Vec3(3, 0, -40), new Vec3(4, h, 0));
        var log = new List<MoverEvent>();
        Mover m = N35Mover(cfg, w, after, log);
        var wNormal = new Vec3(-1, 0, 0);
        var wAlong = new Vec3(0, 0, -1);
        var wStart = new Vec3(3 - 0.7, 0.02, -3);
        var back = new Vec3(wStart.x + wAlong.x * 3 + wNormal.x * 7, wStart.y, wStart.z + wAlong.z * 3 + wNormal.z * 7);
        m.Reset(back.x, back.y, back.z);
        double yaw = Math.Atan2(wNormal.x, wNormal.z); // facing the wall (-wNormal)
        for (int s = 0; s < 42; s++) m.Step(new InputFrame { yaw = yaw }, dt);
        bool jumped = false;
        double slideSpeed = 0, wjSpeed = 0, gapAtJump = double.NaN;
        int wj = 0, n0 = log.Count;
        string kinds = "";
        for (double t = 0; t < 1.6; t += dt)
        {
            double gap = Math.Abs((m.position.x - wStart.x) * wNormal.x + (m.position.z - wStart.z) * wNormal.z);
            bool jump = !jumped && t > 0.3 && gap < 0.6;
            if (jump) { jumped = true; gapAtJump = 3 - m.position.x; }
            m.Step(new InputFrame { moveZ = 1, yaw = yaw, slide = new Button { pressed = t >= 0.2 && t < 0.22, held = t >= 0.2 && !jumped }, jump = new Button { pressed = jump, held = jump } }, dt);
            for (int i = n0; i < log.Count; i++)
            {
                if (log[i].type == MoverEventType.SlideStart) slideSpeed = log[i].speed;
                if (log[i].type == MoverEventType.WallJump) { wj++; wjSpeed = log[i].speed; }
                if (log[i].type == MoverEventType.Jump) kinds += log[i].kind + " jump ";
            }
            n0 = log.Count;
        }
        return $"slide {F35(slideSpeed)} m/s, jump pressed {F35(gapAtJump)} m from the wall face, {kinds}WallJump {wj} at {F35(wjSpeed)} m/s";
    }

    /// <summary>A shaft (two walls 3 m apart, 40 m tall), a person climbing it from a standing jump: `hold` false = TAPS (jump pressed and held
    /// 0.1 s on every new wall contact: the web's wall jumps); true = the Lucio way (jump held into each ride, released 0.25 s into it looking up 40
    /// deg, pressed and held again 0.1 s later). The height reached in 4 s and the wall jumps / jump-offs used.</summary>
    static string N35Shaft(GameConfig cfg, double dt, bool after, bool hold)
    {
        TriangleWorld w = N35Floor();
        w.AddBox(new Vec3(-2.5, 0, -10), new Vec3(-1.5, 40, 10));
        w.AddBox(new Vec3(1.5, 0, -10), new Vec3(2.5, 40, 10));
        var log = new List<MoverEvent>();
        Mover m = N35Mover(cfg, w, after, log);
        m.Reset(-0.5, 0.02, 0);
        for (int s = 0; s < 20; s++) m.Step(new InputFrame(), dt);
        double top = 0, side = 1;
        int heldUntil = 30, releasedAt = -1, rideSteps = 0, lastPress = 0;
        bool wasNear = false;
        for (int s = 0; s < 480; s++)
        {
            bool near = m.touchingWall || m.state == MoveState.WallRide;
            bool riding = m.state == MoveState.WallRide;
            rideSteps = riding ? rideSteps + 1 : 0;
            bool press = s == 0, held;
            double pitch = 0;
            if (!hold)
            {
                if (near && !m.grounded && s > 0 && s - lastPress > 18) { press = true; lastPress = s; heldUntil = s + 12; side = -side; }
                if (m.grounded && s > 30 && s - lastPress > 18) { press = true; lastPress = s; heldUntil = s + 30; } // landed: jump again
                held = press || s < heldUntil;
            }
            else
            {
                held = true;
                if (riding && m.HoldRide35 && rideSteps >= 30 && releasedAt < 0) { releasedAt = s; side = -side; }
                if (riding && !m.HoldRide35 && rideSteps == 30) { press = true; side = -side; } // a ride without the hold (the row off): the press
                if (releasedAt >= 0 && s - releasedAt < 12) { held = false; pitch = 40; }
                else if (releasedAt >= 0) { press = true; releasedAt = -1; }
            }
            wasNear = near;
            m.Step(new InputFrame { moveX = side, pitch = pitch * Math.PI / 180, jump = new Button { pressed = press, held = held || press } }, dt);
            top = Math.Max(top, m.position.y);
        }
        int wj = log.FindAll(e => e.type == MoverEventType.WallJump).Count;
        return $"top {F35(top)} m in 4 s, WallJump events {wj} (jump-offs {m.flow34.ride35.jumpOffs})";
    }

    /// <summary>T's r3 row 9 (FeelReel.N35.Verbs.cs on port/stage3): moveX 0.5 toward the wall, the jump pressed while t under 0.02 s, in FRAMES of
    /// `perFrame` sim steps (a frame's press on each of its steps), held until the first WallRideEnd; the ride / jump events with their times.</summary>
    static string N35VerbRideR3(GameConfig cfg, double dt, bool after, int perFrame)
    {
        TriangleWorld w = N35Floor();
        w.AddBox(new Vec3(3, 0, -40), new Vec3(4, 4, 0));
        var log = new List<MoverEvent>();
        Mover m = N35Mover(cfg, w, after, log);
        m.Reset(3 - 0.7, 0.02, -3);
        for (int s = 0; s < 42; s++) m.Step(new InputFrame { yaw = 0 }, dt);
        string trace = "";
        bool ended = false;
        int n0 = 0;
        double t = 0;
        for (int fr = 0; fr < 300 / perFrame; fr++)
        {
            double ft = t;
            for (int k = 0; k < perFrame; k++, t += dt)
            {
                if (ft < 0.05) m.velocity = new Vec3(0.5, m.velocity.y, -11);
                m.Step(new InputFrame { moveZ = 1, moveX = ended ? 0 : 0.5, yaw = 0, jump = new Button { pressed = ft < 0.02, held = !ended } }, dt);
                for (int i = n0; i < log.Count; i++)
                {
                    MoverEvent e = log[i];
                    if (e.type == MoverEventType.WallRideStart || e.type == MoverEventType.WallRideEnd || e.type == MoverEventType.WallJump || e.type == MoverEventType.Jump || e.type == MoverEventType.DoubleJump)
                        trace += $" {t:0.000}:{e.type}{(e.reason != null ? "/" + e.reason : "")}@y{m.position.y:0.00}";
                    if (e.type == MoverEventType.WallRideEnd) ended = true;
                }
                n0 = log.Count;
            }
        }
        return trace;
    }

    static int N35VerbsCommand(string[] args)
    {
        GameConfig cfg = Config();
        double dt = 1 / cfg.movement.loop.simHz;
        int hi = Array.IndexOf(args, "--h");
        double h = hi >= 0 ? double.Parse(args[hi + 1], CultureInfo.InvariantCulture) : 4;
        Console.WriteLine($"n35move verbs (T's verbs35 rows 9 / 11 / 13 inputs; a 30 m wall {h} m tall)");
        Console.WriteLine($"  rows 9+11 before: {N35VerbRide(cfg, dt, false, h)}");
        Console.WriteLine($"  rows 9+11 after:  {N35VerbRide(cfg, dt, true, h)}");
        Console.WriteLine($"  row 13 before: {N35VerbBounce(cfg, dt, false, h)}");
        Console.WriteLine($"  row 13 after:  {N35VerbBounce(cfg, dt, true, h)}");
        Console.WriteLine($"  row 9 r3, 1 step/frame:  {N35VerbRideR3(cfg, dt, true, 1)}");
        Console.WriteLine($"  row 9 r3, 2 steps/frame: {N35VerbRideR3(cfg, dt, true, 2)}");
        Console.WriteLine($"  row 9 r3, 3 steps/frame: {N35VerbRideR3(cfg, dt, true, 3)}");
        Console.WriteLine($"  shaft taps   before: {N35Shaft(cfg, dt, false, false)} | after: {N35Shaft(cfg, dt, true, false)}");
        Console.WriteLine($"  shaft Lucio  before: {N35Shaft(cfg, dt, false, true)} | after: {N35Shaft(cfg, dt, true, true)}");
        return 0;
    }
}
