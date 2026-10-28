// NIGHT38 builder M (NIGHT38.md M1): the movement builder's harness commands, declared and called by the lead's Program.cs (N38Ride).
//   n38ride hold   design.n38.ride38 forced ON (AFTER) against the row off (BEFORE: NIGHT37's ride) on the real Mover in box worlds (TriangleWorld:
//                  the web's collider, no PhysX), wallride35 on, wallgrip36 and flow34 as the config has them (the game's ride):
//     hold30   a hold ride (jump held, pitch 0) on a wall 60 m tall and 420 m long, met at 3 m: the feet's spread from the attach height
//              over 30 s (PASS: within 0.05 m and still riding at 30 s; before: wallride35's 0.1 m/s sink and its 8 s safety end).
//     rise     pitch +45 on a wall 10 m tall met at 3 m: the climb speed (m/s over 0.5 s once braked) and the highest feet (PASS: ~driftMax
//              within 10 %, the feet stop at or under the top - 1.45 m (the capsule's upper contact sample) within 0.05 m, still riding).
//     sink     pitch -45 on the 60 m wall met at 6 m: the sink speed (PASS: ~driftMax within 10 %) and the end (a landing).
//     band     pitch +20 (inside 15..45): the climb speed (PASS: driftMax x (20 - 15) / (45 - 15) within 10 %); pitch +10: holds (PASS: < 0.01 m).
//     tap      a TIMED ride (jump not held) is the same with the row on and off (PASS: every step's position bit for bit).
//     release  a 2 s hold ride, then jump let go: the jump-off (PASS: the ride ends "jump", the up speed is wallride35's for the pitch).
//     noride   a hold ride at a 60 m wall tagged noride (Mover.SetNoRide38): no ride (PASS), and the same wall untagged rides (PASS).
// PASS lines print RED on a failure; the command exits 1 on any RED.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N38Ride(string cmd, string[] args, ref int code)
    {
        if (cmd != "n38ride") return;
        string sub = args.Length > 1 ? args[1] : "hold";
        if (sub == "hold") code = N38HoldCommand();
        else if (sub == "walk") code = N38WalkCommand(args);
        else
        {
            Console.WriteLine("n38ride: hold | walk");
            code = 2;
        }
    }

    /// <summary>`n38ride walk [level-id ...] [--seeds n] [--off] [--debug]`: NIGHT36's walk (n36move walk: the walk29 pilot through the real sim,
    /// wallride35 / grapple35 / wallgrip36 forced on) with design.n38.ride38 forced ON too (--off: off); default level n37-foundry.</summary>
    static int N38WalkCommand(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        var a = new List<string> { "n36move", "walk" };
        bool anyLevel = false;
        for (int k = 2; k < args.Length; k++)
        {
            if (args[k] == "--off") continue;
            a.Add(args[k]);
            if (!args[k].StartsWith("--") && args[k].Length >= 5 && !char.IsDigit(args[k][0])) anyLevel = true;
        }
        if (!anyLevel) a.Insert(2, "n37-foundry");
        SurvivalSim.Force38Ride = !off;
        try
        {
            Console.WriteLine($"n38ride walk: ride38 forced {(off ? "OFF" : "ON")}");
            return N36WalkCommand(a.ToArray());
        }
        finally
        {
            SurvivalSim.Force38Ride = null;
        }
    }

    /// <summary>A floor (x -20..20, z -460..40) and one wall box: the face at x = 3 (facing -x), z -420..0, `height` m tall.</summary>
    static TriangleWorld N38World(double height)
    {
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-20, -1, -460), new Vec3(20, 0, 40));
        w.AddBox(new Vec3(3, 0, -420), new Vec3(4, height, 0));
        return w;
    }

    static Mover N38Mover(GameConfig cfg, TriangleWorld w, bool after, List<MoverEvent> log)
    {
        var m = new Mover(cfg.movement, w);
        m.SetFlow34(Flow34.Params.From(cfg, cfg.Design("n34.flow34.enabled")));
        m.SetRide35(Ride35.Params.From(cfg, true));
        m.SetGrip36(Grip36.Params.From(cfg, cfg.Design("n36.wallgrip36.enabled")));
        m.SetRide38(after ? Ride38.Params.From(cfg, true) : Ride38.Params.Neutral);
        if (log != null) m.Events += e => log.Add(e);
        return m;
    }

    struct N38RideRun
    {
        public bool rode;
        public double attachY, minY, maxY, seconds, endY, rate, lastUp;
        public string reason;
        public bool ridingAtEnd;
        public List<Vec3> path;
    }

    /// <summary>A ride at the wall of `height` met at feet height y0, for `steps` steps at the pitch (deg); jump held (a hold ride) or never held
    /// (a timed ride). releaseAt >= 0: jump let go at that step after the attach. rateFrom / rateTo: steps after the braking ends over which the
    /// vertical rate is measured.</summary>
    static N38RideRun N38Ride1(GameConfig cfg, double dt, bool after, double height, double y0, double pitchDeg, int steps, bool hold, int releaseAt = -1, Ride38.Box[] noRide = null, bool keepPath = false)
    {
        TriangleWorld w = N38World(height);
        var log = new List<MoverEvent>();
        Mover m = N38Mover(cfg, w, after, log);
        m.SetNoRide38(noRide);
        double r = cfg.movement.body.capsuleRadius;
        m.Reset(3 - r - 0.02, y0, -1);
        m.velocity = new Vec3(1, 0, -8);
        var run = new N38RideRun { reason = "none", minY = double.MaxValue, maxY = double.MinValue, rate = double.NaN, path = keepPath ? new List<Vec3>() : null };
        int attach = -1, braked = -1;
        double yBraked = 0;
        double drift = Ride38.Drift(pitchDeg * Math.PI / 180, Ride38.Params.From(cfg, true));
        bool prev = hold;
        for (int s = 0; s < steps + 2000; s++)
        {
            bool held = hold && !(releaseAt >= 0 && attach >= 0 && s - attach >= releaseAt);
            m.Step(N35In(0.5, 1, held, prev, pitchDeg), dt);
            prev = held;
            run.path?.Add(m.position);
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
            }
            if (!riding)
            {
                run.reason = N35Reason(log);
                run.lastUp = m.flow34.ride35.lastUp;
                break;
            }
            run.minY = Math.Min(run.minY, m.position.y);
            run.maxY = Math.Max(run.maxY, m.position.y);
            if (braked < 0 && s > attach && Math.Abs(m.velocity.y - drift) < 1e-9) // the drift's steady speed reached (a rising entry braked)
            {
                braked = s;
                yBraked = m.position.y;
            }
            if (braked >= 0 && s - braked == 60) run.rate = (m.position.y - yBraked) / (60 * dt);
            run.seconds = (s - attach + 1) * dt;
            run.endY = m.position.y;
            if (s - attach + 1 >= steps)
            {
                run.ridingAtEnd = true;
                run.reason = "riding";
                break;
            }
        }
        return run;
    }

    static int N38HoldCommand()
    {
        GameConfig cfg = Config();
        double dt = 1 / cfg.movement.loop.simHz;
        Ride38.Params p = Ride38.Params.From(cfg, true);
        if (!p.on)
        {
            Console.WriteLine("n38ride hold: design.n38.ride38 has no numbers (Tools/night/n38-ride-config.cjs writes them)");
            return 1;
        }
        string F(double v) => double.IsNaN(v) ? "n/a" : v.ToString("0.000", CultureInfo.InvariantCulture);
        double top = cfg.movement.body.capsuleHeight - cfg.movement.body.capsuleRadius; // the upper contact sample above the feet
        Console.WriteLine($"n38ride hold (design.n38.ride38 enabled {cfg.Design("n38.ride38.enabled")}; forced on for AFTER: holdSink {p.holdSink} m/s, guard {p.guard} s ({p.guardSteps} steps), drift +-{p.driftMax} m/s from {p.deadzoneDeg} to {p.fullDeg} deg, topProbe {p.topProbe} m; wallride35 on, wallgrip36 {cfg.Design("n36.wallgrip36.enabled")}, upper sample {top} m)");
        bool pass = true;
        int s30 = (int)Math.Round(30 / dt);

        // hold30
        N38RideRun b = N38Ride1(cfg, dt, false, 60, 3, 0, s30, true);
        N38RideRun a = N38Ride1(cfg, dt, true, 60, 3, 0, s30, true);
        double spreadA = a.maxY - a.minY;
        bool ok = a.rode && a.ridingAtEnd && Math.Abs(a.endY - a.attachY) <= 0.05 && a.maxY - a.attachY <= 0.05 + 1e-9 && a.attachY - a.minY <= 0.05 + 1e-9;
        pass &= ok;
        Console.WriteLine($"  hold30  before: {F(b.seconds)} s, end {b.reason}, feet {F(b.attachY)} -> {F(b.endY)} ({F(b.endY - b.attachY)} m)"
            + $" | after: {F(a.seconds)} s, end {a.reason}, feet {F(a.attachY)} -> {F(a.endY)} (spread {F(spreadA)} m, max {F(a.maxY - a.attachY)} / min {F(a.minY - a.attachY)} m from the attach) {(ok ? "PASS" : "RED")}");

        // rise
        double wall = 10, cap = wall - top;
        N38RideRun rb = N38Ride1(cfg, dt, false, wall, 3, 45, 720, true);
        N38RideRun ra = N38Ride1(cfg, dt, true, wall, 3, 45, 720, true);
        ok = ra.rode && ra.ridingAtEnd && Math.Abs(ra.rate - p.driftMax) <= 0.1 * p.driftMax && ra.maxY <= cap + 1e-9 && ra.maxY >= cap - 0.05;
        pass &= ok;
        Console.WriteLine($"  rise    pitch +45, a {wall} m wall (feet cap {F(cap)} m): before: {F(rb.seconds)} s, end {rb.reason}, feet max {F(rb.maxY)}"
            + $" | after: climb {F(ra.rate)} m/s, feet max {F(ra.maxY)} ({F(ra.maxY - cap)} m from the cap), {F(ra.seconds)} s, end {ra.reason} {(ok ? "PASS" : "RED")}");

        // sink
        N38RideRun kb = N38Ride1(cfg, dt, false, 60, 6, -45, 720, true);
        N38RideRun ka = N38Ride1(cfg, dt, true, 60, 6, -45, 720, true);
        ok = ka.rode && Math.Abs(-ka.rate - p.driftMax) <= 0.1 * p.driftMax && ka.reason == "land";
        pass &= ok;
        Console.WriteLine($"  sink    pitch -45 from 6 m: before: {F(kb.seconds)} s, end {kb.reason}, feet {F(kb.attachY)} -> {F(kb.endY)}"
            + $" | after: sink {F(-ka.rate)} m/s, {F(ka.seconds)} s, end {ka.reason}, feet {F(ka.attachY)} -> {F(ka.endY)} {(ok ? "PASS" : "RED")}");

        // band
        N38RideRun b20 = N38Ride1(cfg, dt, true, 60, 3, 20, 360, true);
        N38RideRun b10 = N38Ride1(cfg, dt, true, 60, 3, 10, 360, true);
        double want20 = p.driftMax * (20 - p.deadzoneDeg) / (p.fullDeg - p.deadzoneDeg);
        bool ok20 = b20.rode && Math.Abs(b20.rate - want20) <= 0.1 * want20;
        bool ok10 = b10.rode && b10.ridingAtEnd && Math.Abs(b10.endY - b10.attachY) < 0.01;
        pass &= ok20 && ok10;
        Console.WriteLine($"  band    pitch +20: climb {F(b20.rate)} m/s (want {F(want20)}) {(ok20 ? "PASS" : "RED")} | pitch +10: feet {F(b10.endY - b10.attachY)} m over {F(b10.seconds)} s {(ok10 ? "PASS" : "RED")}");

        // tap
        N38RideRun tb = N38Ride1(cfg, dt, false, 60, 3, 30, 600, false, -1, null, true);
        N38RideRun ta = N38Ride1(cfg, dt, true, 60, 3, 30, 600, false, -1, null, true);
        bool same = tb.path.Count == ta.path.Count;
        for (int i = 0; same && i < tb.path.Count; i++) same = tb.path[i].x == ta.path[i].x && tb.path[i].y == ta.path[i].y && tb.path[i].z == ta.path[i].z;
        ok = tb.rode && same;
        pass &= ok;
        Console.WriteLine($"  tap     a timed ride (jump not held, pitch +30): before {F(tb.seconds)} s, end {tb.reason} | after {F(ta.seconds)} s, end {ta.reason}; {tb.path.Count} steps {(same ? "bit for bit" : "DIFFER")} {(ok ? "PASS" : "RED")}");

        // release
        N38RideRun ja = N38Ride1(cfg, dt, true, 60, 3, 30, 600, true, 240);
        double wantUp = Ride35.JumpUp(30 * Math.PI / 180, Ride35.Params.From(cfg, true));
        ok = ja.rode && ja.reason == "jump" && Math.Abs(ja.lastUp - wantUp) < 1e-9;
        pass &= ok;
        Console.WriteLine($"  release jump let go after 2 s (pitch +30): end {ja.reason} at {F(ja.seconds)} s, up {F(ja.lastUp)} m/s (wallride35's {F(wantUp)}) {(ok ? "PASS" : "RED")}");

        // noride
        var tag = new[] { new Ride38.Box { min = new Vec3(3, 0, -420), max = new Vec3(4, 60, 0) } };
        N38RideRun na = N38Ride1(cfg, dt, true, 60, 3, 0, 240, true, -1, tag);
        N38RideRun nu = N38Ride1(cfg, dt, true, 60, 3, 0, 240, true);
        ok = !na.rode && nu.rode;
        pass &= ok;
        Console.WriteLine($"  noride  the 60 m wall tagged noride: {(na.rode ? "RODE" : "no ride")} | untagged: {(nu.rode ? "rides" : "NO RIDE")} {(ok ? "PASS" : "RED")}");

        Console.WriteLine($"n38ride hold: {(pass ? "PASS" : "FAIL")}");
        return pass ? 0 : 1;
    }
}
