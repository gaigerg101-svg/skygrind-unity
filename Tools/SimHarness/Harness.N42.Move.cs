// NIGHT42 builder M1 (NIGHT42.md M1): wallfling42 / boost42's harness commands, declared and called by the lead's Program.cs (N42Move).
//   n42move wall      G2's before / after on the real Mover in a box world (TriangleWorld: the web's collider, no PhysX), the game's ride rules as the
//                     design has them (flow34, wallride35 + ride39, wallgrip36, ride38, ride40: Harness.N40.Ride.cs's N40Mover with ride40 per the
//                     config), wallfling42 off (BEFORE: NIGHT41's Mover) and on (AFTER). A wall's face at x = 3 (normal -x), the floor at y = 0.
//                     Each row: what the press became (wall = a wall jump or jump-off, double, ground, none) and the BACK speed: the most the
//                     velocity points out from the wall (v . n, m/s) in the 0.30 s after the press; and the up speed right after it.
//       walk  t   walking along the wall (touching it) at walk speed, the stick t deg into the wall; a jump tap (0.05 s), then a second press
//                 k s after the first (the double jump a player means): t = 0 / 10 / 30 / 60 / 90, k = 0.10 / 0.20 / 0.30 / 0.45.
//       stand     standing against the wall, the stick centred; the same tap + second press.
//       land  t   falling beside the wall (touching it, from 2.5 m), the press k steps before the landing (k = 1..12; the jump buffer is 12):
//                 the stick centred and 30 deg into the wall.
//       slide p   a slide at 12 m/s heading p deg into the wall (5 / 15 / 30 / 60), the press on the first contact (flow34's slide bounce).
//       tapride   running along the wall at 9 m/s, the stick 30 deg into it; a jump held 0.12 s then released (a tap): ride or not, the back speed.
//       jumpoff   a held ride at 8 / 12 m/s along the wall, released after 0.1 s (a short ride: capped) or 0.4 s (the chain's: kept) at look pitch -30 / 0 / +30: the out speed, the up speed and the
//                 horizontal speed right after the jump-off.
//       shaft     two walls 4 m apart (faces x = -2 / +2); from the left wall, a press with the stick into the right wall: does the player reach it.
//     PASS (AFTER): no wall jump with the stick under intentDot (walk t 0 / 10, stand); no wall jump on a press with a landing within landSeconds;
//     every wall jump's / jump-off's back speed <= max(outSpeed, rideOutMax) + the along part's share (printed); the shaft still crossed.
//   n42move boost     G5's lift per pitch (before / after) and the share of tries that clear a 12 m climb (level 3's boost-chain rooms, kiln40's
//                     THUNDERCLAP CLIMB: jump, shoot down, shoot again, double jump; envelopes38 'jump+double+boost x2' 13.36 m): the real Mover in
//                     a flat world, the shot applied as Loadout.ShotgunJump42 does (Boost42.Push), at the gun's own fire interval (0.9 s). A try =
//                     a pitch (20..90 deg below level, 5 deg steps) x a first-shot delay after the jump (0.05..0.60 s, 0.05 steps) x the double
//                     jump at the second shot's apex or 0.2 s after it: the highest the feet rise over the takeoff.
//   n42move gates [--wallfling] [--boost] [--off]   the gate checker on every ON level's plan (n41 foundry, n40 iceline / kiln / span, n41 spire)
//                     with M1's rows forced on (--wallfling / --boost; both when neither is named; --off: both forced off) (L-061).
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N42Move(string cmd, string[] args, ref int code)
    {
        if (cmd != "n42move") return;
        string sub = args.Length > 1 ? args[1] : "wall";
        n42Trace = Array.IndexOf(args, "--trace") >= 0;
        n42Only = Array.IndexOf(args, "--only") is int oi && oi >= 0 && oi + 1 < args.Length ? args[oi + 1] : null;
        if (sub == "wall") code = N42WallCommand();
        else if (sub == "boost") code = N42BoostCommand();
        else if (sub == "gates") code = N42GatesCommand(args);
        else
        {
            Console.WriteLine("n42move wall | boost | gates [--wallfling] [--boost] [--off]");
            code = 2;
        }
    }

    static string F42(double v) => double.IsNaN(v) ? "n/a" : v.ToString("0.00", CultureInfo.InvariantCulture);

    static Mover N42Mover(GameConfig cfg, ICollisionWorld w, bool after, List<MoverEvent> log)
    {
        Mover m = N40Mover(cfg, w, cfg.Design("n40.ride40.enabled"), log);
        m.SetWall42(Wall42.Params.From(cfg, after));
        return m;
    }

    static TriangleWorld N42WallWorld()
    {
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-20, -1, -200), new Vec3(20, 0, 60));
        w.AddBox(new Vec3(3, 0, -200), new Vec3(4, 14, 60));
        return w;
    }

    struct N42Press
    {
        public string kind;
        public double back, up, horiz;
    }

    /// <summary>What the press at step `at` became: the first jump-type event from `from` in the log (wall / double / ground / none), the most
    /// the velocity pointed out from the wall (normal -x: back = -vx) over the 0.30 s from the press, the up speed the step after.</summary>
    static string N42Kind(List<MoverEvent> log, int from)
    {
        for (int i = from; i < log.Count; i++)
        {
            MoverEventType t = log[i].type;
            if (t == MoverEventType.WallJump) return "wall";
            if (t == MoverEventType.DoubleJump) return "double";
            if (t == MoverEventType.Jump) return log[i].kind == JumpKind.Slide ? "slidejump" : "ground";
        }
        return "none";
    }

    /// <summary>One scripted run: `input(step)` gives each frame; the press of interest at step `at`; runs `at` + 0.30 s.</summary>
    static N42Press N42Run(Mover m, List<MoverEvent> log, Func<int, InputFrame> input, int at, double dt, Action<Mover, int> pre = null)
    {
        var r = new N42Press { kind = "none", back = 0, up = double.NaN, horiz = double.NaN };
        int window = (int)Math.Round(0.30 / dt), from = -1;
        for (int s = 0; s <= at + window; s++)
        {
            pre?.Invoke(m, s);
            if (s == at) from = log.Count;
            m.Step(input(s), dt);
            if (n42Trace)
            {
                var fl = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                object nw = typeof(Mover).GetField("nearWallNormal", fl)?.GetValue(m), ww = typeof(Mover).GetField("wallNormal", fl)?.GetValue(m);
                string V(object o) => o is Vec3 q ? $"({F42(q.x)},{F42(q.y)},{F42(q.z)})" : "?";
                Console.WriteLine($"      s{s} {m.state} pos {F42(m.position.x)},{F42(m.position.y)} v {F42(m.velocity.x)},{F42(m.velocity.y)},{F42(m.velocity.z)} touch {m.touchingWall} near {V(nw)} wall {V(ww)}");
            }
            if (s == at)
            {
                r.up = m.velocity.y;
                r.horiz = JsMath.Hypot(m.velocity.x, m.velocity.z);
            }
            if (s >= at) r.back = Math.Max(r.back, -m.velocity.x);
        }
        if (from >= 0) r.kind = N42Kind(log, from);
        if (n42Trace)
            foreach (MoverEvent e in log) Console.WriteLine($"      event {e.type} speed {F42(e.speed)} {e.reason}");
        return r;
    }

    static bool n42Trace;
    static string n42Only;

    static InputFrame N42In(double mx, double mz, bool press, bool held, double pitchDeg = 0, bool slideHeld = false, bool slidePress = false) =>
        new InputFrame
        {
            moveX = mx,
            moveZ = mz,
            pitch = pitchDeg * Math.PI / 180,
            jump = new Button { pressed = press, held = held || press },
            slide = new Button { pressed = slidePress, held = slideHeld || slidePress },
        };

    static int N42WallCommand()
    {
        GameConfig cfg = Config();
        double dt = 1 / cfg.movement.loop.simHz, r = cfg.movement.body.capsuleRadius, walk = cfg.movement.ground.walkSpeed;
        Wall42.Params p = Wall42.Params.From(cfg, true);
        Console.WriteLine($"n42move wall: the real Mover, the game's ride rules (flow34 {cfg.Design("n34.flow34.enabled")}, wallride35 {cfg.Design("n35.wallride35.enabled")}, ride39 {cfg.Design("n39.ride39.enabled")}, ride40 {cfg.Design("n40.ride40.enabled")}); wallfling42 off (before) / on (after): intentDot {F42(p.intentDot)}, outSpeed {F42(p.outSpeed)} (web {F42(cfg.movement.wallJump.outSpeed)}), v0Mult {F42(p.v0Mult)} (web {F42(cfg.movement.wallJump.v0Mult)}), landSeconds {F42(p.landSeconds)}, rideOutMax {F42(p.rideOutMax)}, bounceDot {F42(p.bounceDot)}");
        Console.WriteLine("  row | before: kind back up | after: kind back up   (back = the most m/s out from the wall in the 0.30 s after the press)");
        int red = 0;
        double maxWallBackBefore = 0, maxWallBackAfter = 0;
        int wallBefore = 0, wallAfter = 0, rows = 0;
        void Row(string name, Func<bool, N42Press> run, bool expectNoWall)
        {
            if (n42Only != null && name.Trim() != n42Only) return;
            N42Press b = run(false), a = run(true);
            rows++;
            bool headOnBounce = name.StartsWith("slide p60"); // flow34's head-on slide bounce: a reflection at full speed, kept (counted apart)
            if (b.kind == "wall" && !headOnBounce) { wallBefore++; maxWallBackBefore = Math.Max(maxWallBackBefore, b.back); }
            if (a.kind == "wall" && !headOnBounce) { wallAfter++; maxWallBackAfter = Math.Max(maxWallBackAfter, a.back); }
            bool bad = expectNoWall && a.kind == "wall";
            if (bad) red++;
            Console.WriteLine($"  {name,-24} | {b.kind,-9} {F42(b.back),6} {F42(b.up),6} | {a.kind,-9} {F42(a.back),6} {F42(a.up),6}{(bad ? "   RED (a wall jump without intent)" : "")}");
        }

        // walk t: along the wall at walk speed (-z), touching it, the stick t deg into it; tap at step 10, the second press k s later
        foreach (int t in new[] { 0, 10, 30, 60, 90 })
            foreach (double k in new[] { 0.10, 0.20, 0.30, 0.45 })
            {
                int at = 10 + (int)Math.Round(k / dt);
                double mx = Math.Sin(t * Math.PI / 180), mz = Math.Cos(t * Math.PI / 180);
                Row($"walk t{t} k{k:0.00}", after =>
                {
                    var log = new List<MoverEvent>();
                    Mover m = N42Mover(cfg, N42WallWorld(), after, log);
                    m.Reset(3 - r - 0.005, 0.02, 0);
                    m.velocity = new Vec3(0, 0, -walk);
                    return N42Run(m, log, s => N42In(mx, mz, s == 10 || s == at, s >= 10 && s < 16), at, dt);
                }, Wall42.Intent(new Vec3(mx, 0, -mz), 1, new Vec3(-1, 0, 0), p.intentDot) == false);
            }
        // stand: against the wall, stick centred
        foreach (double k in new[] { 0.10, 0.20, 0.30, 0.45 })
        {
            int at = 10 + (int)Math.Round(k / dt);
            Row($"stand k{k:0.00}", after =>
            {
                var log = new List<MoverEvent>();
                Mover m = N42Mover(cfg, N42WallWorld(), after, log);
                m.Reset(3 - r - 0.005, 0.02, 0);
                // touch the wall once (a step pushing into it), then stand
                return N42Run(m, log, s => N42In(s < 4 ? 1 : 0, 0, s == 10 || s == at, s >= 10 && s < 16), at, dt);
            }, true);
        }
        // land: falling beside the wall from 2.5 m, touching it; the press k steps before the landing
        int landStep = -1;
        {
            Mover m = N42Mover(cfg, N42WallWorld(), false, null);
            m.Reset(3 - r - 0.005, 2.5, 0);
            for (int s = 0; s < 600 && landStep < 0; s++) { m.Step(N42In(s < 4 ? 1 : 0, 0, false, false), dt); if (m.grounded) landStep = s; }
        }
        foreach (int t in new[] { -1, 30 })
            foreach (int k in new[] { 1, 3, 6, 9, 12 })
            {
                int at = landStep - k;
                double mx = t < 0 ? 0 : Math.Sin(t * Math.PI / 180), mz = t < 0 ? 0 : Math.Cos(t * Math.PI / 180);
                Row($"land {(t < 0 ? "c" : "t" + t)} k{k}", after =>
                {
                    var log = new List<MoverEvent>();
                    Mover m = N42Mover(cfg, N42WallWorld(), after, log);
                    m.Reset(3 - r - 0.005, 2.5, 0);
                    return N42Run(m, log, s => N42In(s < 4 ? 1 : mx, s < 4 ? 0 : mz, s == at, s == at), at, dt);
                }, true);
            }
        // slide p: a slide at 12 m/s heading p deg into the wall, the press on the first contact
        foreach (int pd in new[] { 5, 15, 30, 60 })
        {
            double ang = pd * Math.PI / 180;
            Row($"slide p{pd}", after =>
            {
                var log = new List<MoverEvent>();
                Mover m = N42Mover(cfg, N42WallWorld(), after, log);
                m.Reset(3 - r - 1.0, 0.02, 0);
                for (int s = 0; s < 10; s++) m.Step(N42In(0, 0, false, false), dt);
                m.velocity = new Vec3(12 * Math.Sin(ang), 0, -12 * Math.Cos(ang));
                int at = -1;
                var res = new N42Press { kind = "none", up = double.NaN };
                int window = (int)Math.Round(0.30 / dt), from = -1;
                for (int s = 0; s < 360; s++)
                {
                    bool press = at < 0 && m.touchingWall;
                    if (press) { at = s; from = log.Count; }
                    m.Step(N42In(0, 0, press, press, 0, at < 0, s == 0), dt);
                    if (press) res.up = m.velocity.y;
                    if (at >= 0) res.back = Math.Max(res.back, -m.velocity.x);
                    if (at >= 0 && s >= at + window) break;
                }
                if (from >= 0) res.kind = N42Kind(log, from);
                return res;
            }, pd < 30);
        }
        // tapride: along the wall at 9 m/s, the stick 30 deg into it, a 0.12 s jump tap
        {
            int hold = (int)Math.Round(0.12 / dt);
            Row("tapride 9 m/s t30", after =>
            {
                var log = new List<MoverEvent>();
                Mover m = N42Mover(cfg, N42WallWorld(), after, log);
                m.Reset(3 - r - 0.005, 0.02, 0);
                m.velocity = new Vec3(0, 0, -9);
                double mx = Math.Sin(Math.PI / 6), mz = Math.Cos(Math.PI / 6);
                N42Press res = N42Run(m, log, s => N42In(mx, mz, s == 10, s >= 10 && s < 10 + hold), 10, dt);
                bool rode = log.Exists(e => e.type == MoverEventType.WallRideStart);
                res.kind = (rode ? "ride>" : "") + (log.Exists(e => e.type == MoverEventType.WallJump) ? "wall" : res.kind);
                for (int s = 0; s < 36; s++) { m.Step(N42In(mx, mz, false, false), dt); res.back = Math.Max(res.back, -m.velocity.x); }
                return res;
            }, false);
        }
        // jumpoff: a held ride at v along the wall, released after 0.4 s at pitch pd
        foreach (double hold in new[] { 0.1, 0.4 })
        foreach (double v in new[] { 8.0, 12.0 })
            foreach (int pd in new[] { -30, 0, 30 })
            {
                Row($"jumpoff {hold:0.0}s {v:0} m/s p{pd}", after =>
                {
                    var log = new List<MoverEvent>();
                    Mover m = N42Mover(cfg, N42WallWorld(), after, log);
                    m.Reset(3 - r - 0.02, 3, 0);
                    m.velocity = new Vec3(1, 0, -v);
                    int attach = -1, rel = -1;
                    var res = new N42Press { kind = "none", up = double.NaN, horiz = double.NaN };
                    int from = -1, window = (int)Math.Round(0.30 / dt);
                    for (int s = 0; s < 480; s++)
                    {
                        if (attach < 0 && m.state == MoveState.WallRide) attach = s;
                        bool held = rel < 0 && !(attach >= 0 && s - attach >= (int)Math.Round(hold / dt));
                        if (!held && rel < 0) { rel = s; from = log.Count; }
                        m.Step(N42In(0.5, 1, false, held, pd), dt);
                        if (s == rel) { res.up = m.velocity.y; res.horiz = JsMath.Hypot(m.velocity.x, m.velocity.z); }
                        if (rel >= 0) res.back = Math.Max(res.back, -m.velocity.x);
                        if (rel >= 0 && s >= rel + window) break;
                    }
                    if (from >= 0) res.kind = N42Kind(log, from) + (attach < 0 ? "(no ride)" : "");
                    return res;
                }, false);
            }
        // shaft: walls 4 m apart; from touching the left wall, in the air, a press with the stick into the right wall
        {
            bool Shaft(bool after)
            {
                var w = new TriangleWorld();
                w.AddBox(new Vec3(-20, -1, -60), new Vec3(20, 0, 60));
                w.AddBox(new Vec3(-3, 0, -60), new Vec3(-2, 40, 60));
                w.AddBox(new Vec3(2, 0, -60), new Vec3(3, 40, 60));
                var log = new List<MoverEvent>();
                Mover m = N42Mover(cfg, w, after, log);
                m.Reset(-2 + r + 0.005, 10, 0);
                m.velocity = new Vec3(-1, 0, 0);
                bool reached = false;
                for (int s = 0; s < 240 && !m.grounded; s++)
                {
                    m.Step(N42In(1, 0, s == 3, s == 3), dt); // stick +x: into the far (right) wall, away from the left one
                    if (m.position.x > 2 - r - 0.05) reached = true;
                }
                Console.WriteLine($"  shaft {(after ? "after " : "before")}: the press became {N42Kind(log, 0)}; the far wall {(reached ? "reached" : "NOT reached")}");
                return reached;
            }
            Shaft(false);
            if (!Shaft(true)) { red++; Console.WriteLine("  RED: the shaft's far wall not reached after"); }
        }
        Console.WriteLine($"n42move wall: {rows} rows; wall jumps / jump-offs (the head-on slide bounce apart) before {wallBefore} (max back {F42(maxWallBackBefore)} m/s), after {wallAfter} (max back {F42(maxWallBackAfter)} m/s); RED {red}");
        return red > 0 ? 1 : 0;
    }

    static int N42BoostCommand()
    {
        GameConfig cfg = Config();
        double dt = 1 / cfg.movement.loop.simHz, jump = cfg.Num("weapons.shotgun.jumpSpeed"), interval = cfg.Num("weapons.shotgun.fireInterval");
        Boost42.Params off = Boost42.Params.Neutral, on = Boost42.Params.From(cfg, true);
        Console.WriteLine($"n42move boost: THUNDERCLAP jumpSpeed {F42(jump)} m/s, fire interval {F42(interval)} s; boost42: lift {F42(on.lift)} m/s at {F42(on.fullDeg)} deg below level or steeper, fallFloor {F42(on.fallFloor)} m/s");
        Console.WriteLine("  pitch below level | lift at rest: before after | falling at -8 m/s, vy after the shot: before after");
        foreach (int p in new[] { 0, 10, 20, 30, 45, 60, 75, 90 })
        {
            double rad = p * Math.PI / 180;
            Vec3 aim = new Vec3(0, -JsMath.Sin(rad), -JsMath.Cos(rad));
            Vec3 fall = new Vec3(0, -8, 0);
            Console.WriteLine($"  {p,3} deg | {F42(Boost42.LiftAt(p, jump, off)),6} {F42(Boost42.LiftAt(p, jump, on)),6} | {F42(Boost42.Push(fall, aim, jump, off).y),6} {F42(Boost42.Push(fall, aim, jump, on).y),6}");
        }
        // the climb on the REAL sim (the gates' rig, Harness.N38.Gates.cs: a Survival run's Mover and Loadout, the THUNDERCLAP in hand, its real fire
        // interval and the real shotgun-jump line) over a flat floor 2 km east: jump (held while rising), from d s after the takeoff a pull every
        // other step until shot 1 fires, from shot 1's apex pulls until shot 2 fires, the double jump at shot 2's apex (or 0.2 s after it), the
        // look p deg below level throughout. A try = p (20..90, 10 deg steps) x d (0.05..0.50, 0.05 steps) x the double's timing.
        N38Rig rig = N38NewRig(cfg, false);
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-60, -1, -60) + N38Off, new Vec3(60, 0, 60) + N38Off);
        int tries = 0;
        int[] clear12 = new int[2], clear10 = new int[2], clear8 = new int[2];
        double[] best = new double[2], sum = new double[2];
        var byPitch = new SortedDictionary<int, int[]>();
        double rdt = rig.dt;
        int S(double s) => MovementMath.StepsFor(s, rdt);
        for (int p = 20; p <= 90; p += 10)
            for (int di = 1; di <= 10; di++)
                for (int dv = 0; dv < 2; dv++)
                for (int hp = 0; hp < 3; hp++) // the jump button: 0 a tap (0.1 s), 1 held until shot 1 then let go, 2 held while rising (the gates' bot)
                for (int ns = 2; ns <= 3; ns++) // the shots before the double jump
                {
                    tries++;
                    for (int a = 0; a < 2; a++)
                    {
                        SurvivalSim sim = rig.sim;
                        Mover m = sim.mover;
                        m.World = w;
                        rig.proxy.inner = w;
                        Vec3 start = N38Off + new Vec3(0, 0.05, 0);
                        m.Reset(start.x, start.y, start.z);
                        int slot = sim.loadout.owned.IndexOf("shotgun");
                        for (int k = 0; k < 600 && !(k >= 60 && sim.loadout.ready && m.grounded && sim.loadout.Weapon == "shotgun"); k++)
                            N38Tick(rig, 0, 0, new InputFrame(), new CombatInput { weapon = sim.loadout.Weapon != "shotgun" && k % 30 == 0 ? slot : -1 });
                        sim.loadout.boost42 = a == 0 ? off : on;
                        double y0 = m.position.y, top = y0, pitch = -p * Math.PI / 180;
                        int shots0 = rig.shots, takeoff = -1, apexAt = -1, apexOf = 0, lastAt = -1, dbl = -1;
                        bool airborne = false, doubled = false;
                        for (int i = 0; i < S(5); i++)
                        {
                            var f = new InputFrame();
                            var c = new CombatInput { weapon = -1 };
                            int shots = rig.shots - shots0;
                            if (i == 0) f.jump = new Button { pressed = true, held = true };
                            else if (!doubled && (hp == 0 ? i < S(0.1) : hp == 1 ? shots == 0 : m.velocity.y > 0)) f.jump = new Button { held = true };
                            if (takeoff < 0 && !m.grounded) takeoff = i;
                            bool pull = false;
                            if (takeoff >= 0 && shots == 0 && i - takeoff >= S(di * 0.05)) pull = true;
                            if (shots >= 1 && shots < ns)
                            {
                                if (apexOf != shots) { apexOf = shots; apexAt = -1; }
                                if (apexAt < 0 && m.velocity.y <= 0) apexAt = i;
                                if (apexAt >= 0) pull = true;
                            }
                            if (shots >= ns && lastAt < 0) lastAt = i;
                            if (shots >= ns && !doubled && (dv == 0 ? m.velocity.y <= 0 : i - lastAt >= S(0.2))) { dbl = i; doubled = true; f.jump = new Button { pressed = true, held = true }; }
                            else if (doubled && i - dbl < S(0.3)) f.jump = new Button { held = true };
                            if (pull && (i & 1) == 0) c.fire = new Button { pressed = true, held = true };
                            N38Tick(rig, 0, pitch, f, c);
                            if (!m.grounded) airborne = true;
                            top = Math.Max(top, m.position.y);
                            if (n42Trace && p == 90 && di == 1 && dv == 0 && hp == 1 && ns == 2 && i % 6 == 0)
                                Console.WriteLine($"      {(a == 0 ? "before" : "after ")} i {i} y {F42(m.position.y - y0)} vy {F42(m.velocity.y)} shots {rig.shots - shots0} pull {(c.fire.pressed ? "x" : "-")} ready {sim.loadout.ready} {sim.loadout.Weapon} {m.state}");
                            if (airborne && m.grounded) break;
                        }
                        sim.loadout.boost42 = Boost42.Params.Neutral;
                        double rise = top - y0;
                        sum[a] += rise;
                        best[a] = Math.Max(best[a], rise);
                        if (rise >= 12) clear12[a]++;
                        if (rise >= 10) clear10[a]++;
                        if (rise >= 8) clear8[a]++;
                        if (!byPitch.ContainsKey(p)) byPitch[p] = new int[2];
                        if (rise >= 12) byPitch[p][a]++;
                    }
                }
        Console.WriteLine($"  the climb on the real sim (jump, shot, 1 or 2 more shots each from the last one's apex, double jump): {tries} tries per side (pitch 20..90 deg x first pull 0.05..0.50 s after the takeoff x the double at the apex / +0.2 s x the jump tapped / held to shot 1 / held while rising x 2 / 3 shots)");
        Console.Write($"  rises 12 m by pitch (before / after of {tries / byPitch.Count}):");
        foreach (var kv in byPitch) Console.Write($" {kv.Key}:{kv.Value[0]}/{kv.Value[1]}");
        Console.WriteLine();
        string Pc(int k) => (100.0 * k / tries).ToString("0.0", CultureInfo.InvariantCulture) + " %";
        Console.WriteLine($"  before: rises 8 m {Pc(clear8[0])}, 10 m {Pc(clear10[0])}, 12 m {Pc(clear12[0])}; best {F42(best[0])} m, mean {F42(sum[0] / tries)} m");
        Console.WriteLine($"  after : rises 8 m {Pc(clear8[1])}, 10 m {Pc(clear10[1])}, 12 m {Pc(clear12[1])}; best {F42(best[1])} m, mean {F42(sum[1] / tries)} m");
        return 0;
    }

    /// <summary>NIGHT42 M1 (the lead's decision, 23:05): per-room weapon ownership for the gate checker. A level row that sets the level's start
    /// set and award rooms (design.n40.kiln40: startSet, awards {room, weapon}; its levelTo is the plan's id) makes the PULSE gauntlet NOT owned in
    /// a room that comes before its award room in the plan's room order (the checker's "pulse added: owned from level 2 on" is level-wide).
    /// Returns the award room when `room` is before it (the gauntlet not yet in hand there), else null.</summary>
    static string N42PulseLockedUntil(N38Plan plan, string room)
    {
        GameConfig cfg = n42GatesCfg ??= Config();
        foreach (string row in new[] { "n40.kiln40" })
        {
            string root = "design." + row + ".";
            if (!cfg.Design(row + ".enabled") || !cfg.Has(root + "startSet") || !cfg.Has(root + "awards")) continue;
            if (!(cfg.Get(root + "levelTo") is string to) || to != plan.id) continue;
            bool start = false;
            foreach (object o in cfg.Arr(root + "startSet")) if (o as string == "gauntlet") start = true;
            if (start) return null;
            foreach (object o in cfg.Arr(root + "awards"))
            {
                var a = PortData.Obj(o);
                if (!(a.TryGetValue("weapon", out object w) && w as string == "gauntlet" && a.TryGetValue("room", out object r) && r is string at)) continue;
                int iAward = plan.rooms.FindIndex(q => q.id == at), iRoom = plan.rooms.FindIndex(q => q.id == room);
                if (iAward >= 0 && iRoom >= 0 && iRoom < iAward) return at;
            }
        }
        return null;
    }

    static GameConfig n42GatesCfg;

    static int N42GatesCommand(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        bool wf = Array.IndexOf(args, "--wallfling") >= 0, bo = Array.IndexOf(args, "--boost") >= 0;
        if (!wf && !bo) wf = bo = true;
        SurvivalSim.Force42Wallfling = off ? false : wf ? true : (bool?)null;
        SurvivalSim.Force42Boost = off ? false : bo ? true : (bool?)null;
        string[] plans = { "Design/levels/n41/foundry.plan.json", "Design/levels/n40/iceline.plan.json", "Design/levels/n40/kiln.plan.json", "Design/levels/n40/span.plan.json", "Design/levels/n41/spire.plan.json" };
        var pass = new List<string>(); // `--plan <p>` (one plan), `--only <gateId>` (n38 gates' own), `--wf key=v,..` (wallfling42 what-ifs)
        string wfProbe = null;
        for (int k = 2; k < args.Length; k++)
        {
            if (args[k] == "--plan" && k + 1 < args.Length) plans = new[] { args[++k] };
            else if ((args[k] == "--only" || args[k] == "--trace") && k + 1 < args.Length) { pass.Add(args[k]); pass.Add(args[++k]); }
            else if (args[k] == "--wf" && k + 1 < args.Length) wfProbe = args[++k];
            else if (args[k] == "--lift" && k + 1 < args.Length)
            {
                double lift = double.Parse(args[++k], CultureInfo.InvariantCulture);
                SurvivalSim.Probe42Boost = p => { p.lift = lift; return p; };
                Console.WriteLine($"n42move gates: boost42 what-if lift {F42(lift)}");
            }
        }
        if (wfProbe != null)
            SurvivalSim.Probe42Wall = p =>
            {
                foreach (string s in wfProbe.Split(','))
                {
                    string[] kv = s.Split('=');
                    double v = double.Parse(kv[1], CultureInfo.InvariantCulture);
                    switch (kv[0])
                    {
                        case "intentDot": p.intentDot = v; break;
                        case "outSpeed": p.outSpeed = v; break;
                        case "v0Mult": p.v0Mult = v; break;
                        case "landSeconds": p.landSeconds = v; break;
                        case "rideOutMax": p.rideOutMax = v; break;
                        case "bounceDot": p.bounceDot = v; break;
                        case "rideShortSeconds": p.rideShortSteps = MovementMath.StepsFor(v, 1.0 / 120); break;
                    }
                }
                return p;
            };
        int red = 0;
        try
        {
            Console.WriteLine($"n42move gates: wallfling42 forced {SurvivalSim.Force42Wallfling?.ToString() ?? "(config)"}, boost42 forced {SurvivalSim.Force42Boost?.ToString() ?? "(config)"}{(wfProbe != null ? ", wallfling42 what-if " + wfProbe : "")}");
            foreach (string plan in plans)
            {
                int c = -1;
                var a = new List<string> { "n38", "gates", plan };
                a.AddRange(pass);
                N38Gates("n38", a.ToArray(), ref c);
                if (c != 0) red++;
            }
        }
        finally
        {
            SurvivalSim.Force42Wallfling = null;
            SurvivalSim.Force42Boost = null;
            SurvivalSim.Probe42Wall = null;
            SurvivalSim.Probe42Boost = null;
        }
        Console.WriteLine($"n42move gates: {plans.Length} plans, {red} not GREEN (n40-iceline has no gate: RED 0 / 0 before too)");
        return 0;
    }
}
