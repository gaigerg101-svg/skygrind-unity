// NIGHT36 builder M (NIGHT36.md M1): the movement builder's harness commands, declared and called by the lead's Program.cs (N36Move).
//   n36move grip    design.n36.wallgrip36 BEFORE (wallride35 as the game runs it, the grip off) / AFTER (the grip's rules) on the real Mover in box
//                   worlds (TriangleWorld: the web's collider, no PhysX):
//     entry   a 14 m wall met in the air at up speed 0 / 4 / 7 / 8.33 m/s (8.33: a jump's take-off speed), as a TAP ride (jump not held: the web's
//             timed ride) and a HOLD ride (jump held: wallride35's): the most the player rises above the attach point while riding, the height
//             at attach + 0.5 s and + 1 s, the ride's seconds and metres, its end reason.
//     ride30  NIGHT35's 30 m hold ride (Harness.N35.Move.cs N35Ride30): metres, seconds, height lost.
//     chain   NIGHT35's three-wall chain (N35Chain): the speed at each ride start and after the last jump-off.
//     gaps    NIGHT35's facing-wall gaps 3..12 m (N35Gap): which chain.
// PASS: after, no entry rises more than design.n36.wallgrip36.attachUpMax's card limit (1.5 m) above its attach point; the hold ride still runs
// the 30 m wall (>= 28 m); the chain's last speed is at least 90 % of before's; at least as many gaps chain as before minus one.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    /// <summary>The grip the N35 ride helpers' movers take (Neutral: wallride35 alone).</summary>
    static Grip36.Params n36HarnessGrip;

    /// <summary>NIGHT36.md M1's check: a jump into a ride wall rises at most this far above the attach point (m).</summary>
    const double N36RiseLimit = 1.5;

    static partial void N36Move(string cmd, string[] args, ref int code)
    {
        if (cmd != "n36move") return;
        string sub = args.Length > 1 ? args[1] : "grip";
        if (sub == "grip") code = N36GripCommand(args);
        else if (sub == "walk") code = N36WalkCommand(args);
        else if (sub == "deaths") code = N36DeathsCommand(args);
        else
        {
            Console.WriteLine("n36move: grip | walk");
            code = 2;
        }
    }

    /// <summary>`n36move walk [level-id ...] [--seeds n] [--off] [--debug]`: NIGHT35's walk (n35move walk: the walk29 pilot through the real sim, wallride35 and
    /// grapple35 forced on) with design.n36.wallgrip36 forced ON too (--off: off); default level n36-foundry. The boss room is not played (B's).</summary>
    static int N36WalkCommand(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        var a = new List<string> { "n35move", "walk" };
        bool anyLevel = false;
        for (int k = 2; k < args.Length; k++)
        {
            a.Add(args[k]);
            if (!args[k].StartsWith("--") && args[k].Length >= 5 && !char.IsDigit(args[k][0])) anyLevel = true;
        }
        if (!anyLevel) a.Insert(2, "n36-foundry");
        SurvivalSim.Force36Grip = !off;
        try
        {
            Console.WriteLine($"n36move walk: wallgrip36 forced {(off ? "OFF" : "ON")}");
            return N35WalkCommand(a.ToArray());
        }
        finally
        {
            SurvivalSim.Force36Grip = null;
        }
    }

    struct N36Entry
    {
        public double attachY, rise, at05, at10, seconds, metres, attachVy;
        public string reason;
        public bool rode;
    }

    /// <summary>entry: a 14 m wall (face x = 3, z 0 .. -40) met in the air at up speed vy; tap (jump never held) or hold (jump held throughout).</summary>
    static N36Entry N36EntryRun(GameConfig cfg, double dt, Grip36.Params grip, double vy, bool hold)
    {
        TriangleWorld w = N35Floor();
        w.AddBox(new Vec3(3, 0, -40), new Vec3(4, 14, 0));
        var log = new List<MoverEvent>();
        var m = new Mover(cfg.movement, w);
        m.SetFlow34(Flow34.Params.From(cfg, cfg.Design("n34.flow34.enabled")));
        m.SetRide35(Ride35.Params.From(cfg, true));
        m.SetGrip36(grip);
        m.Events += e => log.Add(e);
        double r = cfg.movement.body.capsuleRadius;
        m.Reset(3 - r - 0.25, 1.2, -1);
        m.velocity = new Vec3(3, vy, -8);
        var e0 = new N36Entry { reason = "none", at05 = double.NaN, at10 = double.NaN };
        int attach = -1;
        double z0 = 0;
        bool prev = hold;
        for (int s = 0; s < 1500; s++)
        {
            m.Step(N35In(0.5, 1, hold, prev), dt);
            prev = hold;
            bool riding = m.state == MoveState.WallRide;
            if (attach < 0)
            {
                if (!riding) continue;
                attach = s;
                e0.rode = true;
                e0.attachY = m.position.y;
                e0.attachVy = m.velocity.y;
                z0 = m.position.z;
            }
            if (!riding)
            {
                e0.reason = N35Reason(log);
                break;
            }
            double dy = m.position.y - e0.attachY;
            e0.rise = Math.Max(e0.rise, dy);
            if (s - attach == 60) e0.at05 = dy;
            if (s - attach == 120) e0.at10 = dy;
            e0.seconds = (s - attach + 1) * dt;
            e0.metres = z0 - m.position.z;
        }
        return e0;
    }

    static int N36GripCommand(string[] args)
    {
        GameConfig cfg = Config();
        double dt = 1 / cfg.movement.loop.simHz;
        Grip36.Params g = Grip36.Params.From(cfg, true);
        if (!g.on)
        {
            Console.WriteLine("n36move grip: design.n36.wallgrip36 has no numbers (Tools/night/n36m-config.cjs writes them)");
            return 1;
        }
        Console.WriteLine($"n36move grip (design.n36.wallgrip36 enabled {cfg.Design("n36.wallgrip36.enabled")}; forced on for AFTER: attachUpMax {g.attachUpMax} m/s, rise x{g.riseGravityMult} g, grip {g.gripTime} s ({g.gripSteps} steps), tap sink +{g.sinkAccel} m/s per s to {g.sinkMax} m/s, hold sink = wallride35's; wallride35 on in both)");
        bool pass = true;
        string F(double v) => F35(v);

        foreach (bool hold in new[] { false, true })
            foreach (double vy in new[] { 0.0, 4, 7, 8.33 })
            {
                N36Entry b = N36EntryRun(cfg, dt, Grip36.Params.Neutral, vy, hold);
                N36Entry a = N36EntryRun(cfg, dt, g, vy, hold);
                bool ok = a.rode && a.rise <= N36RiseLimit;
                pass &= ok;
                Console.WriteLine($"  entry {(hold ? "hold" : "tap ")} vy {vy,4:0.##}: before rise +{F(b.rise)} m (attach vy {F(b.attachVy)}), +0.5 s {F(b.at05)}, +1 s {F(b.at10)}, {F(b.seconds)} s / {F(b.metres)} m, end {b.reason}"
                    + $" | after rise +{F(a.rise)} m (attach vy {F(a.attachVy)}), +0.5 s {F(a.at05)}, +1 s {F(a.at10)}, {F(a.seconds)} s / {F(a.metres)} m, end {a.reason} {(ok ? "" : "RED")}");
            }

        n36HarnessGrip = Grip36.Params.Neutral;
        var rb = N35Ride30(cfg, dt, true);
        double[] cb = N35Chain(cfg, dt, true, out string tb);
        int gapsB = 0;
        var sbB = new StringBuilder();
        for (double gap = 3; gap <= 12; gap += 1)
            if (N35Gap(cfg, dt, gap, out _)) { gapsB++; sbB.Append($" {gap:0}"); }
        n36HarnessGrip = g;
        var ra = N35Ride30(cfg, dt, true);
        double[] ca = N35Chain(cfg, dt, true, out string ta);
        int gapsA = 0;
        var sbA = new StringBuilder();
        for (double gap = 3; gap <= 12; gap += 1)
            if (N35Gap(cfg, dt, gap, out _)) { gapsA++; sbA.Append($" {gap:0}"); }
        n36HarnessGrip = Grip36.Params.Neutral;

        bool rideOk = ra.metres >= 28;
        bool chainOk = !double.IsNaN(ca[3]) && ca[3] >= 0.9 * cb[3] && !double.IsNaN(ca[2]);
        bool gapsOk = gapsA >= gapsB - 1;
        pass &= rideOk && chainOk && gapsOk;
        Console.WriteLine($"  ride30  before: {F(rb.metres)} m in {F(rb.seconds)} s ({F(rb.speed)} m/s), end {rb.reason}, height lost {F(rb.drop)} m | after: {F(ra.metres)} m in {F(ra.seconds)} s ({F(ra.speed)} m/s), end {ra.reason}, height lost {F(ra.drop)} m {(rideOk ? "" : "RED")}");
        Console.WriteLine($"  chain   before:{tb} | after:{ta} {(chainOk ? "" : "RED")}");
        Console.WriteLine($"  gaps    before {gapsB} chain (m:{sbB}) | after {gapsA} chain (m:{sbA}) {(gapsOk ? "" : "RED")}");
        Console.WriteLine($"n36move grip: {(pass ? "PASS" : "FAIL")}");
        return pass ? 0 : 1;
    }
}
