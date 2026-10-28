// NIGHT43 builder M1 (NIGHT43.md M1): chimney43 / ride43 / boost43's harness commands, declared and called by the lead's Program.cs (N43Move).
// Every movement test runs the REAL Mover in a box world (TriangleWorld: the web's collider, no PhysX) with the game's ride rules as the design has
// them (flow34, wallride35 + ride39, wallgrip36, ride40 = Harness.N40.Ride.cs's N40Mover; wallfling42 per its row), M1's rows off (BEFORE: NIGHT42's
// Mover) and on (AFTER: Move43.Params.From with the row(s) named).
//   n43move attach    ride43 (a)/(e): the share of scripted approaches that attach: angle 10..60 deg into the wall x speed 6..16 m/s x jump held / not x
//                     the stick along the motion / along the wall (no stick into it); the refusals by rule.
//   n43move chimney   chimney43: two facing walls 4.5 m apart, 14 m tall; a press at each wall (the stick into the wall being jumped to): the height
//                     reached, the wall jumps, the presses that became the double jump; pure bounces and bounces with 6 m/s along the walls.
//   n43move bdrop     ride43 (d): a ride at 12 m/s left by a jump-off / by the stick away / by a wall jump, then B 0.1 s later: the horizontal speed
//                     kept 0.2 s after B; looking down 60 deg (the slam stays).
//   n43move jumpseq   ride43 (h) + the lead's note (GG's pad: the bottom face button reads B = JUMP): jump-button sequences on and near a ride and
//                     what each became (wall jump / jump-off / double jump / nothing), its push out from the wall and its up speed.
//   n43move snap      ride43 (b): a jump into a wall (rising 8.3 m/s, jump held): the up speed kept at the attach and the rise after it.
//   n43move dashride  ride43 (g): a dash into a wall in the air at 30 / 60 / 90 deg: attached or not, the ride's speed.
//   n43move boost     boost43: the apex of jump + THUNDERCLAP shot at offsets -0.15..+0.15 s (shot first < 0), before / after, and the heights
//                     K1 builds level 3's steps from (one boost from a standing jump, boost + double jump, two boosts) at lift 11 and 12.
//   n43move gates [--plan p] [--off]   the gate checker with chimney43 / ride43 / boost43 forced on (L-061), default level 3's plan (kiln).
//   n43move all       every table above but gates.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N43Move(string cmd, string[] args, ref int code)
    {
        if (cmd != "n43move") return;
        string sub = args.Length > 1 ? args[1] : "all";
        n42Trace = Array.IndexOf(args, "--trace") >= 0;
        GameConfig cfg = Config();
        switch (sub)
        {
            case "attach": code = N43Attach(cfg); break;
            case "chimney": code = N43Chimney(cfg); break;
            case "bdrop": code = N43BDrop(cfg); break;
            case "jumpseq": code = N43JumpSeq(cfg); break;
            case "snap": code = N43Snap(cfg); break;
            case "dashride": code = N43Dash(cfg); break;
            case "boost": code = N43Boost(cfg, args); break;
            case "gates": code = N43Gates(args); break;
            case "selftest": code = N43SelfTest(cfg); break;
            case "stack": code = N43Stack(cfg, args); break;
            case "reelchim": code = N43ReelChimney(cfg, args); break;
            case "flue": code = N43Flue(cfg, args); break;
            case "all":
                code = N43Attach(cfg) | N43Chimney(cfg) | N43BDrop(cfg) | N43JumpSeq(cfg) | N43Snap(cfg) | N43Dash(cfg) | N43Boost(cfg, args);
                break;
            default:
                Console.WriteLine("n43move attach | chimney | bdrop | jumpseq | snap | dashride | boost | gates [--plan p] [--off] | all");
                code = 2;
                break;
        }
    }

    /// <summary>The game's Mover (N42Mover: the ride rules per the design, wallfling42 per its row) with M1's rows: chimney / ride on or off.</summary>
    static Mover N43Mover(GameConfig cfg, ICollisionWorld w, bool chimney, bool ride, List<MoverEvent> log)
    {
        Mover m = N42Mover(cfg, w, cfg.Design("n42.wallfling42.enabled"), log);
        m.SetMove43(Move43.Params.From(cfg, chimney, ride));
        return m;
    }

    static string P43(int k, int n) => n == 0 ? "n/a" : (100.0 * k / n).ToString("0", CultureInfo.InvariantCulture) + " %";

    // ------------------------------------------------------------------------------------------------ attach

    static int N43Attach(GameConfig cfg)
    {
        double dt = 1 / cfg.movement.loop.simHz, r = cfg.movement.body.capsuleRadius;
        Console.WriteLine("n43move attach: a wall face x = 3 (normal -x), the floor y = 0; the player in the air (feet 2.5 m, rising 2 m/s) heads into the wall at angle a deg off the wall line at v m/s, the camera along the wall");
        int[] angles = { 10, 20, 30, 45, 60 };
        double[] speeds = { 6, 8, 10, 12, 14, 16 };
        string[] modes = { "hold+stick", "hold+nostick", "tap+stick", "tap+nostick" };
        int red = 0;
        foreach (string mode in modes)
        {
            bool held = mode.StartsWith("hold"), stick = !mode.EndsWith("nostick");
            int[] ok = new int[2];
            int n = 0;
            var refuse = new int[2, 6];
            var rowB = new System.Text.StringBuilder();
            var rowA = new System.Text.StringBuilder();
            foreach (int a in angles)
                foreach (double v in speeds)
                {
                    n++;
                    for (int side = 0; side < 2; side++)
                    {
                        var w = N42WallWorld();
                        var log = new List<MoverEvent>();
                        Mover m = N43Mover(cfg, w, false, side == 1, log);
                        double ar = a * Math.PI / 180, vx = v * Math.Sin(ar), vz = -v * Math.Cos(ar);
                        double t0 = 0.25;
                        m.Reset(3 - r - 0.05 - vx * t0, 2.5, 0);
                        m.velocity = new Vec3(vx, 2, vz);
                        double mx = stick ? Math.Sin(ar) : 0, mz = stick ? Math.Cos(ar) : 1;
                        bool rode = false;
                        for (int s = 0; s < (int)Math.Round(0.7 / dt) && !m.grounded; s++)
                        {
                            m.Step(N42In(mx, mz, false, held), dt);
                            if (m.state == MoveState.WallRide) { rode = true; break; }
                        }
                        if (rode) ok[side]++;
                        ref Ride35.State q = ref m.flow34.ride35;
                        refuse[side, 0] += q.r40NoIntent > 0 ? 1 : 0;
                        refuse[side, 1] += q.r40Slow > 0 ? 1 : 0;
                        refuse[side, 2] += q.r40Low > 0 ? 1 : 0;
                        refuse[side, 3] += q.r40Short > 0 ? 1 : 0;
                        refuse[side, 4] += q.r40Locked > 0 ? 1 : 0;
                        refuse[side, 5] += q.reRideBlocks > 0 ? 1 : 0;
                        (side == 0 ? rowB : rowA).Append(rode ? 'R' : '.');
                    }
                }
            Console.WriteLine($"  {mode,-13} attached: before {ok[0]}/{n} ({P43(ok[0], n)}), after {ok[1]}/{n} ({P43(ok[1], n)}); tries refused at least once by: no-intent {refuse[0, 0]}->{refuse[1, 0]}, slow {refuse[0, 1]}->{refuse[1, 1]}, low {refuse[0, 2]}->{refuse[1, 2]}, short wall {refuse[0, 3]}->{refuse[1, 3]}, locked {refuse[0, 4]}->{refuse[1, 4]}, re-ride {refuse[0, 5]}->{refuse[1, 5]}");
            Console.WriteLine($"      grid (angle rows {string.Join("/", angles)} x speeds {string.Join("/", speeds)}): before {rowB}  after {rowA}");
            if (ok[1] < ok[0]) red++;
        }
        // the near miss: moving parallel to the wall (2 deg toward it), the capsule g m off the face, the stick 30 deg into it (tap) or along (hold)
        foreach (string mode in new[] { "hold", "tap+stick" })
        {
            var sb = new System.Text.StringBuilder($"  near miss {mode,-9} (gap 0.15 / 0.35 / 0.55 / 0.75 m x 8 / 12 / 16 m/s): ");
            int[] ok = new int[2];
            int n = 0;
            foreach (double gap in new[] { 0.15, 0.35, 0.55, 0.75 })
                foreach (double v in new[] { 8.0, 12.0, 16.0 })
                {
                    n++;
                    for (int side = 0; side < 2; side++)
                    {
                        var log = new List<MoverEvent>();
                        Mover m = N43Mover(cfg, N42WallWorld(), false, side == 1, log);
                        m.Reset(3 - r - gap, 2.5, 0);
                        double a = 2 * Math.PI / 180;
                        m.velocity = new Vec3(v * Math.Sin(a), 1, -v * Math.Cos(a));
                        bool hold = mode == "hold";
                        double mx = hold ? 0 : Math.Sin(Math.PI / 6), mz = hold ? 1 : Math.Cos(Math.PI / 6);
                        bool rode = false;
                        for (int s = 0; s < (int)Math.Round(0.25 / dt) && !m.grounded; s++)
                        {
                            m.Step(N42In(mx, mz, false, hold), dt);
                            if (m.state == MoveState.WallRide) { rode = true; break; }
                        }
                        if (rode) ok[side]++;
                    }
                }
            sb.Append($"attached within 0.25 s: before {ok[0]}/{n}, after {ok[1]}/{n}");
            Console.WriteLine(sb.ToString());
        }
        Console.WriteLine($"n43move attach: RED {red} (after attaches fewer than before)");
        return red > 0 ? 1 : 0;
    }

    // ------------------------------------------------------------------------------------------------ chimney

    static TriangleWorld N43ChimneyWorld(double half, double top)
    {
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-20, -1, -60), new Vec3(20, 0, 60));
        w.AddBox(new Vec3(-half - 1, 0, -60), new Vec3(-half, top, 60));
        w.AddBox(new Vec3(half, 0, -60), new Vec3(half + 1, top, 60));
        return w;
    }

    struct N43Climb
    {
        public double top;
        public int wallJumps, doubles, jumpOffs, rides, presses, refills;
        public string end;
    }

    /// <summary>A chimney climb: walls at x = -half / +half; start touching the left wall (feet 2 m, falling 0); the stick points into the wall
    /// being jumped to (the left one first) plus `along` m/s worth along +z; a 0.05 s jump tap on the first step within 0.06 m of that wall;
    /// after a wall jump / jump-off the target flips. Runs 8 s or until the feet land.</summary>
    static N43Climb N43ClimbRun(GameConfig cfg, bool chimney, bool ride, double half, double along, double pitchDeg, bool held = false)
    {
        double dt = 1 / cfg.movement.loop.simHz, r = cfg.movement.body.capsuleRadius;
        var w = N43ChimneyWorld(half, 14);
        var log = new List<MoverEvent>();
        Mover m = N43Mover(cfg, w, chimney, ride, log);
        m.Reset(-half + r + 0.005, 2, 0);
        m.velocity = new Vec3(held ? -2 : -0.5, 0, -along);
        int target = -1, tap = 0, seen = 0, rideAt = -1;
        bool hold = held;
        bool armed = true;
        var res = new N43Climb { end = "time" };
        double alongStick = along > 0 ? 0.6 : 0;
        for (int s = 0; s < (int)Math.Round(8 / dt); s++)
        {
            // a press on the step after the player is on the target wall (touching it or riding it), a 0.025 s tap; once per contact
            bool onTarget = (m.touchingWall || m.state == MoveState.WallRide) && m.wallNormal.x * -target > 0.5;
            bool press = false;
            if (tap > 0) tap--;
            if (!onTarget) armed = true;
            else if (armed && m.state != MoveState.Ground)
            {
                press = true;
                armed = false;
                tap = 3;
                res.presses++;
            }
            if (held)
            {
                // the held ride: jump held into each wall, released 0.15 s into the ride (the jump-off), pressed again 0.6 m before the next wall (the buffer holds it)
                press = false;
                if (m.state == MoveState.WallRide) { if (rideAt < 0) rideAt = s; if (s - rideAt >= 18) hold = false; }
                else rideAt = -1;
                double gapT = target < 0 ? m.position.x - r - -half : half - (m.position.x + r);
                if (!hold && m.state != MoveState.WallRide && gapT < 0.6 && gapT > 0.02) { hold = true; press = true; res.presses++; }
            }
            double sx = target, sz = alongStick;
            double sl = Math.Sqrt(sx * sx + sz * sz);
            m.Step(N42In(sx / sl, sz / sl, press, held ? hold : tap > 0, pitchDeg), dt);
            for (; seen < log.Count; seen++)
            {
                MoverEvent e = log[seen];
                if (e.type == MoverEventType.WallJump) { res.wallJumps++; target = -target; }
                else if (e.type == MoverEventType.DoubleJump) res.doubles++;
                else if (e.type == MoverEventType.WallRideStart) res.rides++;
            }
            res.top = Math.Max(res.top, m.position.y);
            if (m.grounded && s > 30) { res.end = "landed"; break; }
            if (m.position.y > 13.5) { res.end = "TOP"; break; }
        }
        res.refills = m.m43.refills + m.m43.rideRefills;
        res.jumpOffs = m.flow34.ride35.jumpOffs;
        return res;
    }

    static int N43Chimney(GameConfig cfg)
    {
        Console.WriteLine($"n43move chimney: two facing walls (14 m tall), presses at each wall (a 0.05 s tap within 0.06 m of the wall being jumped to; the stick into it); wallJump.maxPerAirtime {cfg.movement.wallJump.maxPerAirtime}");
        Console.WriteLine("  case                          | rows                | top m   presses wall jumps  doubles  rides  refills  end");
        int red = 0;
        foreach (double half in new[] { 2.25, 1.75 })
            foreach (double along in new[] { 0.0, 6.0 })
                foreach (int pitch in new[] { 0, 30 })
                foreach (bool hp in new[] { false, true })
                {
                    if (along == 0 && pitch != 0 && !hp) continue;
                    if (hp && pitch == 0) continue;
                    string name = $"{(hp ? "HELD" : "press")} gap {2 * half:0.0} along {along:0} look {pitch,2}";
                    double topBefore = 0, topAfter = 0;
                    foreach ((bool ch, bool ri, string rows) in new[] { (false, false, "before (NIGHT42)"), (true, false, "chimney43"), (true, true, "chimney43+ride43") })
                    {
                        N43Climb c = N43ClimbRun(cfg, ch, ri, half, along, pitch, hp);
                        if (!ch) topBefore = c.top;
                        if (ch && ri) topAfter = c.top;
                        Console.WriteLine($"  {name,-29} | {rows,-19} | {F42(c.top),6}  {c.presses,7} {c.wallJumps,10}  {c.doubles,7}  {c.rides,5}  {c.refills,7}  {c.end}");
                    }
                    if (topAfter + 0.01 < topBefore) red++;
                }
        Console.WriteLine($"n43move chimney: RED {red} (after climbs lower than before)");
        return red > 0 ? 1 : 0;
    }

    // ------------------------------------------------------------------------------------------------ B after a ride

    static int N43BDrop(GameConfig cfg)
    {
        double dt = 1 / cfg.movement.loop.simHz, r = cfg.movement.body.capsuleRadius;
        Console.WriteLine("n43move bdrop: a held ride at 12 m/s along a wall (face x = 3, feet 3 m) left by: a jump-off (release), the stick away, a tap ride's pressed wall jump; then B (slide) pressed k s later; the horizontal speed 0.2 s after B (and what B did)");
        Console.WriteLine("  how          k s   look | before: speed  B was | after: speed  B was");
        int red = 0;
        foreach (string how in new[] { "jumpoff", "away", "walljump" })
            foreach (double k in new[] { 0.05, 0.10, 0.20, 0.40 })
                foreach (int look in new[] { 0, -60 })
                {
                    if (look != 0 && k != 0.10) continue;
                    var outRow = new string[2];
                    var spd = new double[2];
                    for (int side = 0; side < 2; side++)
                    {
                        var log = new List<MoverEvent>();
                        Mover m = N43Mover(cfg, N42WallWorld(), false, side == 1, log);
                        m.Reset(3 - r - 0.02, 3, 0);
                        m.velocity = new Vec3(1, 0, -12);
                        bool hold = how != "walljump";
                        int attach = -1, left = -1, bAt = -1, end = -1;
                        string was = "none";
                        for (int s = 0; s < 400; s++)
                        {
                            if (attach < 0 && m.state == MoveState.WallRide) attach = s;
                            bool ride = m.state == MoveState.WallRide;
                            int age = attach < 0 ? 0 : s - attach;
                            double mx = 0.4, mz = 1;
                            bool jHeld = hold, jPress = false, bPress = false;
                            if (attach >= 0 && left < 0 && age >= 36)
                            {
                                if (how == "jumpoff") jHeld = false;
                                else if (how == "away") mx = -1;
                                else jPress = true;
                            }
                            if (left >= 0)
                            {
                                jHeld = false;
                                mx = 0;
                                if (bAt < 0 && s - left >= (int)Math.Round(k / dt)) { bPress = true; bAt = s; }
                            }
                            m.Step(new InputFrame { moveX = mx, moveZ = mz, pitch = (left >= 0 ? look : 0) * Math.PI / 180, jump = new Button { pressed = jPress, held = jHeld || jPress }, slide = new Button { pressed = bPress, held = bPress } }, dt);
                            if (attach >= 0 && left < 0 && ride && m.state != MoveState.WallRide) left = s;
                            if (bPress) was = m.state == MoveState.Slam ? "slam" : m.m43.bDrops > 0 ? "drop" : m.state.ToString();
                            if (bAt >= 0 && s - bAt >= (int)Math.Round(0.2 / dt)) { end = s; break; }
                            if (m.grounded && attach >= 0 && left >= 0 && bAt < 0) { was = "landed first"; break; }
                        }
                        spd[side] = end >= 0 ? m.Speed : double.NaN;
                        outRow[side] = $"{F42(spd[side]),6}  {was,-12}";
                    }
                    bool bad = look == 0 && k <= 0.3 && !(spd[1] >= 0.8 * spd[0] || double.IsNaN(spd[0])) ;
                    bool badSlam = look == 0 && k <= 0.3 && !double.IsNaN(spd[1]) && spd[1] < 1;
                    if (badSlam) red++;
                    Console.WriteLine($"  {how,-10} {k:0.00}  {look,4} | {outRow[0]} | {outRow[1]}{(badSlam ? "   RED (speed lost)" : "")}");
                }
        Console.WriteLine($"n43move bdrop: RED {red} (B inside the grace, looking level, lost the speed)");
        return red > 0 ? 1 : 0;
    }

    // ------------------------------------------------------------------------------------------------ jump-button sequences

    struct N43Seq
    {
        public string kind;
        public double push, up, speed;
    }

    /// <summary>What the press(es) became: the events after `from` (wall = a pressed wall jump, jumpoff = wallride35's jump-off (Ride35JumpOff), double,
    /// ground, none) joined by '+'.</summary>
    static string N43Kinds(List<MoverEvent> log, int from, Mover m, int jumpOffsBefore)
    {
        var parts = new List<string>();
        int offs = m.flow34.ride35.jumpOffs - jumpOffsBefore;
        for (int i = from; i < log.Count; i++)
        {
            MoverEventType t = log[i].type;
            if (t == MoverEventType.WallJump) parts.Add(offs-- > 0 ? "jumpoff" : "walljump");
            else if (t == MoverEventType.DoubleJump) parts.Add("DOUBLE");
            else if (t == MoverEventType.Jump) parts.Add("ground");
            else if (t == MoverEventType.WallRideStart) parts.Add("ride");
            else if (t == MoverEventType.WallRideEnd && log[i].reason != "jump") parts.Add("end:" + log[i].reason);
        }
        return parts.Count == 0 ? "none" : string.Join("+", parts);
    }

    static int N43JumpSeq(GameConfig cfg)
    {
        double dt = 1 / cfg.movement.loop.simHz, r = cfg.movement.body.capsuleRadius;
        int S(double s) => (int)Math.Round(s / dt);
        Console.WriteLine("n43move jumpseq: jump-button sequences on / near a ride (wall face x = 3, feet 3 m, 10 m/s along it); push = the most m/s out from the wall in the 0.3 s after the sequence's press, up = the up speed right after it");
        Console.WriteLine("  sequence                                  | before: what                 push    up | after: what                  push    up");
        int surprising = 0;
        void Row(string name, Func<bool, (Mover m, List<MoverEvent> log, int from, int offs0)> run, Func<string, bool> fine)
        {
            var o = new string[2];
            bool badAfter = false;
            for (int side = 0; side < 2; side++)
            {
                var (m, log, from, offs0) = run(side == 1);
                string k = N43Kinds(log, from, m, offs0);
                o[side] = $"{k,-26} {F42(n43SeqPush),6} {F42(n43SeqUp),5}";
                if (side == 1 && !fine(k)) badAfter = true;
            }
            if (badAfter) surprising++;
            Console.WriteLine($"  {name,-41} | {o[0]} | {o[1]}{(badAfter ? "   SURPRISING" : "")}");
        }
        // a common run: the player along the wall; `input(s, m, attachStep)` per step; the press of interest at step `at` (set by the input)
        (Mover, List<MoverEvent>, int, int) Run(bool after, bool holdEntry, Func<int, int, Mover, InputFrame> input, Action<Mover> pre = null, int steps = 300)
        {
            var log = new List<MoverEvent>();
            Mover m = N43Mover(cfg, N42WallWorld(), after, after, log);
            m.Reset(3 - r - 0.02, 3, 0);
            m.velocity = new Vec3(1, 0, -10);
            pre?.Invoke(m);
            int attach = -1, from = -1, offs0 = 0;
            n43SeqPush = 0;
            n43SeqUp = double.NaN;
            n43SeqAt = -1;
            for (int s = 0; s < steps; s++)
            {
                if (attach < 0 && m.state == MoveState.WallRide) attach = s;
                InputFrame f = input(s, attach, m);
                bool mark = n43SeqAt == s;
                if (mark) { from = log.Count; offs0 = m.flow34.ride35.jumpOffs; }
                m.Step(f, dt);
                if (mark) n43SeqUp = m.velocity.y;
                if (n43SeqAt >= 0 && s >= n43SeqAt && s <= n43SeqAt + S(0.3)) n43SeqPush = Math.Max(n43SeqPush, -m.velocity.x);
                if (m.grounded && s > 5) break;
            }
            return (m, log, from < 0 ? log.Count : from, offs0);
        }
        InputFrame F(double mx, double mz, bool press, bool held, double pitch = 0) => N42In(mx, mz, press, held, pitch);
        bool Pushes(string k) => k.Contains("walljump") || k.Contains("jumpoff");

        // 1. a tap ride (jump released before the wall), a press 0.3 s into the ride: a pressed wall jump (spends one)
        Row("tap ride, press at 0.3 s", after => Run(after, false, (s, a, m) =>
        {
            bool p = a >= 0 && s - a == S(0.3);
            if (p) n43SeqAt = s;
            return F(0.4, 1, p, p || (a >= 0 && s - a > S(0.3) && s - a < S(0.35)));
        }), Pushes);
        // 2. the same with the wall-jump count already spent (3 presses earlier in the airtime): the press becomes the double jump (no push) before
        Row("tap ride, count empty, press at 0.3 s", after => Run(after, false, (s, a, m) =>
        {
            bool p = a >= 0 && s - a == S(0.3);
            if (p) n43SeqAt = s;
            return F(0.4, 1, p, p || (a >= 0 && s - a > S(0.3) && s - a < S(0.35)));
        }, m => m.wallJumpsLeft = 0), Pushes);
        // 2b. the count spent on THIS wall (the same wall: chimney43 does not refill it): ride43 (h) makes the press the jump-off
        Row("tap ride, count spent on this wall", after => Run(after, false, (s, a, m) =>
        {
            bool p = a >= 0 && s - a == S(0.3);
            if (p) n43SeqAt = s;
            return F(0.4, 1, p, p || (a >= 0 && s - a > S(0.3) && s - a < S(0.35)));
        }, m =>
        {
            m.SetMove43(Move43.Params.From(cfg, false, m.m43.p.ride)); // chimney43 off here: (h) alone
            m.wallJumpsLeft = 0;
            m.m43.hasWall = true;
            m.m43.wallN = new Vec3(-1, 0, 0);
            m.m43.wallAt = new Vec3(m.position.x, 0, -2);
        }), Pushes);
        // 3. a held ride released at 0.4 s (the jump-off), a re-press 0.10 s after (no wall near: the double jump is expected)
        foreach (double k in new[] { 0.10, 0.20 })
            Row($"hold ride, release 0.4 s, re-press +{k:0.00}", after => Run(after, true, (s, a, m) =>
            {
                int rel = a < 0 ? int.MaxValue : a + S(0.4);
                bool p = s == rel + S(k);
                if (s == rel) n43SeqAt = s;
                return F(0.4, 1, p, s < rel || p);
            }), kk => kk.StartsWith("jumpoff"));
        // 4. a short hold: jump pressed into the wall and released 0.1 s after the attach (a quick bounce)
        Row("hold ride, release at 0.1 s", after => Run(after, true, (s, a, m) =>
        {
            int rel = a < 0 ? int.MaxValue : a + S(0.1);
            if (s == rel) n43SeqAt = s;
            return F(0.4, 1, false, s < rel);
        }), Pushes);
        // 5. a tap ride left by the stick (away), a press 0.1 s later (wallride35's late window, 0.2 s)
        Row("tap ride, stick away at 0.3 s, press +0.10", after => Run(after, false, (s, a, m) =>
        {
            int awayAt = a < 0 ? int.MaxValue : a + S(0.3);
            bool p = s == awayAt + S(0.1);
            if (p) n43SeqAt = s;
            return F(s >= awayAt ? -1 : 0.4, s >= awayAt ? 0 : 1, p, p);
        }), Pushes);
        // 6. a press 0.05 s before reaching a wall (approach 30 deg at 10 m/s, the stick along the motion; flow34's wall buffer)
        Row("approach 30 deg, press 0.05 s before contact", after =>
        {
            var log = new List<MoverEvent>();
            Mover m = N43Mover(cfg, N42WallWorld(), after, after, log);
            double vx = 10 * Math.Sin(Math.PI / 6), vz = -10 * Math.Cos(Math.PI / 6);
            m.Reset(3 - r - vx * 0.3, 3, 0);
            m.velocity = new Vec3(vx, 0, vz);
            int pressAt = S(0.25), from = -1;
            n43SeqPush = 0;
            n43SeqUp = double.NaN;
            for (int s = 0; s < 240 && !m.grounded; s++)
            {
                if (s == pressAt) from = log.Count;
                m.Step(F(Math.Sin(Math.PI / 6), Math.Cos(Math.PI / 6), s == pressAt, s >= pressAt && s < pressAt + 6), dt);
                if (s == pressAt + 6) n43SeqUp = m.velocity.y;
                if (s >= pressAt && s <= pressAt + S(0.4)) n43SeqPush = Math.Max(n43SeqPush, -m.velocity.x);
            }
            return (m, log, from, 0);
        }, kk => !kk.StartsWith("DOUBLE"));
        // 7. a held ride, the jump released and pressed again within the same 0.05 s (a stutter on the button)
        Row("hold ride, release + re-press in 0.05 s", after => Run(after, true, (s, a, m) =>
        {
            int rel = a < 0 ? int.MaxValue : a + S(0.4);
            if (s == rel) n43SeqAt = s;
            bool p = s == rel + S(0.05);
            return F(0.4, 1, p, s < rel || p);
        }), kk => kk.StartsWith("jumpoff"));
        Console.WriteLine($"n43move jumpseq: {surprising} sequences still surprising after (a press that pushes nobody off, or a double jump where a wall jump was meant)");
        return 0;
    }

    static double n43SeqPush, n43SeqUp;
    static readonly List<string> n43Ev = new List<string>();
    static int n43SeqAt;

    // ------------------------------------------------------------------------------------------------ the snap

    static int N43Snap(GameConfig cfg)
    {
        double dt = 1 / cfg.movement.loop.simHz, r = cfg.movement.body.capsuleRadius, v0 = MovementMath.JumpVelocity(cfg.movement.jump.height, cfg.movement.jump.apexTime);
        Console.WriteLine($"n43move snap: a jump into the wall (rising {F42(v0)} m/s, jump held, 9 m/s along, 20 deg into it): the up speed the attach keeps and the rise after it");
        foreach (double vy in new[] { v0, 5.0, 3.0 })
            for (int side = 0; side < 2; side++)
            {
                var log = new List<MoverEvent>();
                Mover m = N43Mover(cfg, N42WallWorld(), false, side == 1, log);
                m.Reset(3 - r - 0.25, 1.2, 0);
                m.velocity = new Vec3(5, vy, -9);
                int attach = -1;
                double vyAt = double.NaN, yAt = 0, top = 0;
                for (int s = 0; s < 200; s++)
                {
                    m.Step(N42In(0.34, 0.94, false, true), dt);
                    if (attach < 0 && m.state == MoveState.WallRide) { attach = s; vyAt = m.velocity.y; yAt = m.position.y; }
                    if (attach >= 0) top = Math.Max(top, m.position.y);
                    if (attach >= 0 && s - attach > 60) break;
                }
                Console.WriteLine($"  rising {F42(vy)} m/s | {(side == 0 ? "before" : "after ")}: {(attach < 0 ? "no ride" : $"the up speed right after the attach {F42(vyAt)} m/s, the rise after it {F42(top - yAt)} m")}");
            }
        return 0;
    }

    // ------------------------------------------------------------------------------------------------ dash into a wall

    static int N43Dash(GameConfig cfg)
    {
        double dt = 1 / cfg.movement.loop.simHz, r = cfg.movement.body.capsuleRadius;
        Console.WriteLine("n43move dash: in the air (feet 2.5 m), 2.5 m from a wall (face x = 3), a dash at a deg into it (the stick along the dash, jump not held): attached? the ride's speed");
        int red = 0;
        foreach (int a in new[] { 20, 30, 45, 60, 90 })
        {
            var o = new string[2];
            bool[] rode = new bool[2];
            for (int side = 0; side < 2; side++)
            {
                var log = new List<MoverEvent>();
                Mover m = N43Mover(cfg, N42WallWorld(), false, side == 1, log);
                m.Reset(3 - r - 2.5, 2.5, 0);
                double ar = a * Math.PI / 180, mx = Math.Sin(ar), mz = Math.Cos(ar);
                double speed = double.NaN;
                string how = "no ride";
                for (int s = 0; s < 120 && !m.grounded; s++)
                {
                    m.Step(new InputFrame { moveX = mx, moveZ = mz, dash = new Button { pressed = s == 2, held = s >= 2 && s < 6 } }, dt);
                    if (m.state == MoveState.WallRide) { rode[side] = true; speed = m.Speed; how = $"ride at {F42(speed)} m/s (step {s})"; break; }
                }
                o[side] = how;
            }
            if (a >= 30 && !rode[1]) red++;
            Console.WriteLine($"  {a,2} deg | before: {o[0],-26} | after: {o[1]}");
        }
        Console.WriteLine($"n43move dash: RED {red} (a dash at 30 deg or more into the wall that does not attach after)");
        return red > 0 ? 1 : 0;
    }

    // ------------------------------------------------------------------------------------------------ boost43

    /// <summary>One boost try on the real sim (the gates' rig over a flat floor): `script(i, shots, m)` gives the jump and fire buttons per step;
    /// returns the highest the feet rose over the start.</summary>
    static double N43BoostRun(N38Rig rig, GameConfig cfg, bool on, double pitchDeg, Func<int, int, Mover, (Button jump, Button fire)> script, double lift = double.NaN, int steps = 600)
    {
        SurvivalSim sim = rig.sim;
        Mover m = sim.mover;
        var w = new TriangleWorld();
        w.AddBox(new Vec3(-60, -1, -60) + N38Off, new Vec3(60, 0, 60) + N38Off);
        m.World = w;
        rig.proxy.inner = w;
        Vec3 start = N38Off + new Vec3(0, 0.05, 0);
        m.Reset(start.x, start.y, start.z);
        int slot = sim.loadout.owned.IndexOf("shotgun");
        for (int k = 0; k < 600 && !(k >= 60 && sim.loadout.ready && m.grounded && sim.loadout.Weapon == "shotgun"); k++)
            N38Tick(rig, 0, 0, new InputFrame(), new CombatInput { weapon = sim.loadout.Weapon != "shotgun" && k % 30 == 0 ? slot : -1 });
        for (int k = 0; k < 400 && sim.loadout.GunCycleLeft29 > 0; k++) N38Tick(rig, 0, 0, new InputFrame(), new CombatInput { weapon = -1 });
        Boost43.Params bp = Boost43.Params.From(cfg, on);
        if (on && !double.IsNaN(lift)) bp.push.lift = lift;
        sim.n43.boost43 = on;
        sim.loadout.boost43 = bp;
        sim.loadout.b43.Reset();
        double y0 = m.position.y, top = y0, pitch = -pitchDeg * Math.PI / 180;
        int shots0 = rig.shots;
        bool airborne = false;
        for (int i = 0; i < steps; i++)
        {
            var (jb, fb) = script(i, rig.shots - shots0, m);
            N38Tick(rig, 0, pitch, new InputFrame { jump = jb }, new CombatInput { weapon = -1, fire = fb });
            if (!m.grounded) airborne = true;
            top = Math.Max(top, m.position.y);
            if (n42Trace && i % 6 == 0) Console.WriteLine($"      {(on ? "after " : "before")} i {i} y {F42(m.position.y - y0)} vy {F42(m.velocity.y)} shots {rig.shots - shots0} {m.state}");
            if (airborne && m.grounded) break;
        }
        sim.n43.boost43 = false;
        sim.loadout.boost43 = Boost43.Params.Neutral;
        return top - y0;
    }

    static int N43Boost(GameConfig cfg, string[] args)
    {
        double dt = cfg.Dt;
        int S(double s) => (int)Math.Round(s / dt);
        N38Rig rig = N38NewRig(cfg, false);
        Boost43.Params on = Boost43.Params.From(cfg, true);
        Console.WriteLine($"n43move boost: the real sim (the gates' rig: a Survival run's Mover and Loadout, the THUNDERCLAP in hand, charge40's ground rule {cfg.Design("n40.charge40.enabled")}, boost42 {cfg.Design("n42.boost42.enabled")}); boost43: window {F42(on.window)} s, lift {F42(on.push.lift)} m/s at {F42(on.push.fullDeg)} deg+, fallFloor {F42(on.push.fallFloor)}");
        Button B(bool p, bool h) => new Button { pressed = p, held = h || p };
        // the combine: jump pressed at step J = 30 (held 0.3 s: a player holding the button through the rise, or a 0.1 s tap), the trigger
        // pressed at J + offset and held 0.05 s (a click), looking 90 / 60 deg down
        Console.WriteLine("  jump + shot (the shot's press at the offset from the jump's; < 0 = the shot first), the apex over the takeoff (m): before / after");
        int red = 0;
        foreach (string jmode in new[] { "held", "tap" })
            foreach (int pd in new[] { 90, 60 })
            {
                var sb = new System.Text.StringBuilder($"    jump {jmode,-4} look {pd} down:");
                foreach (double off in new[] { -0.15, -0.10, -0.05, -0.02, 0.0, 0.02, 0.05, 0.10, 0.15 })
                {
                    int J = 30, F = J + S(off), hold = jmode == "held" ? S(0.3) : S(0.1);
                    var h = new double[2];
                    for (int side = 0; side < 2; side++)
                        h[side] = N43BoostRun(rig, cfg, side == 1, pd, (i, shots, m) => (B(i == J, i > J && i < J + hold), B(i == F, i > F && i < F + S(0.05))));
                    sb.Append($"  {(off >= 0 ? "+" : "")}{off:0.00}: {F42(h[0])}/{F42(h[1])}");
                    if (Math.Abs(off) <= 0.15 && h[1] < 4) red++;
                }
                Console.WriteLine(sb.ToString());
            }
        // the heights K1 builds from (look straight down, jump held while rising)
        Console.WriteLine("  K1's heights (look 90 down, jump held while rising; the gun's own fire interval; the feet's highest point over the takeoff, m):");
        foreach (double lift in new[] { 11.0, 12.0 })
        {
            double H(bool after, Func<int, int, Mover, (Button, Button)> sc) => N43BoostRun(rig, cfg, after, 90, sc, lift);
            // jump only
            double jumpOnly = H(false, (i, s, m) => (B(i == 30, i > 30 && m.velocity.y > 0), default));
            // one boost at the combine (the shot pressed with the jump)
            double oneCombine = H(true, (i, s, m) => (B(i == 30, i > 30 && m.velocity.y > 0), B(i == 30 && s == 0, false)));
            // one boost at the jump's apex
            int apexFire = -1;
            double oneApex = H(true, (i, s, m) =>
            {
                bool f = false;
                if (i > 32 && s == 0 && m.velocity.y <= 0 && !m.grounded) f = (i & 1) == 0;
                return (B(i == 30, i > 30 && m.velocity.y > 0 && s == 0), B(f, false));
            });
            // boost at the combine + the double jump at its apex
            bool dbl = false;
            double oneDouble = H(true, (i, s, m) =>
            {
                bool dj = false;
                if (s >= 1 && !dbl && i > 40 && m.velocity.y <= 0) { dj = true; dbl = true; }
                return (B(i == 30 || dj, (i > 30 && m.velocity.y > 0) || dj), B(i == 30 && s == 0, false));
            });
            dbl = false;
            // two boosts: the combine, then a pull every other step from the first boost's apex until the second fires
            double twoBoosts = H(true, (i, s, m) =>
            {
                bool f = i == 30 && s == 0;
                if (s == 1 && i > 40 && m.velocity.y <= 0) f = (i & 1) == 0;
                return (B(i == 30, i > 30 && m.velocity.y > 0 && s == 0), B(f, false));
            });
            // two boosts + the double jump at the second's apex
            double twoDouble = H(true, (i, s, m) =>
            {
                bool f = i == 30 && s == 0, dj = false;
                if (s == 1 && i > 40 && m.velocity.y <= 0) f = (i & 1) == 0;
                if (s >= 2 && !dbl && m.velocity.y <= 0) { dj = true; dbl = true; }
                return (B(i == 30 || dj, (i > 30 && m.velocity.y > 0 && s == 0) || dj), B(f, false));
            });
            dbl = false;
            // the same three with boost43 off (today: the shot pressed with the jump; its apex shot)
            double oneCombineOff = H(false, (i, s, m) => (B(i == 30, i > 30 && m.velocity.y > 0), B(i == 30 && s == 0, false)));
            double oneApexOff = H(false, (i, s, m) =>
            {
                bool f = false;
                if (i > 32 && s == 0 && m.velocity.y <= 0 && !m.grounded) f = (i & 1) == 0;
                return (B(i == 30, i > 30 && m.velocity.y > 0 && s == 0), B(f, false));
            });
            Console.WriteLine($"    lift {F42(lift)}: jump only {F42(jumpOnly)} | one boost: at the takeoff {F42(oneCombine)} (off {F42(oneCombineOff)}), at the jump's apex {F42(oneApex)} (off {F42(oneApexOff)}) | boost + double jump {F42(oneDouble)} | two boosts {F42(twoBoosts)} | two boosts + double {F42(twoDouble)}");
        }
        Console.WriteLine($"  combine late pushes / takeoff fires / air boosts / expired (the run): {rig.sim.loadout.b43.latePushes} / {rig.sim.loadout.b43.holdFires} / {rig.sim.loadout.b43.airBoosts} / {rig.sim.loadout.b43.expired}");
        Console.WriteLine($"n43move boost: RED {red} (a combine inside +-0.15 s under 4 m after)");
        return red > 0 ? 1 : 0;
    }

    // ------------------------------------------------------------------------------------------------ selftest (Move43Tests' Mover cases, headless)

    /// <summary>The two Mover cases of Assets/Tests/EditMode/Move43Tests.cs on the web's Mover (every older row neutral), so a builder sees them
    /// without the Editor: the chimney's wall jumps / doubles / top, off and on; B 12 steps after a wall jump (state, speed), off / on / looking
    /// down / after the grace.</summary>
    static int N43SelfTest(GameConfig cfg)
    {
        double dt = 1.0 / 120, r = cfg.movement.body.capsuleRadius;
        (int, int, double) Chim(bool chimney)
        {
            double half = 2.25;
            var m = new Mover(cfg.movement, N43ChimneyWorld(half, 14));
            m.SetMove43(Move43.Params.From(cfg, chimney, false));
            int wallJumps = 0, doubles = 0;
            m.Events += e => { if (e.type == MoverEventType.WallJump) wallJumps++; if (e.type == MoverEventType.DoubleJump) doubles++; };
            m.Reset(-half + r + 0.005, 2, 0);
            m.velocity = new Vec3(-0.5, 0, 0);
            int target = -1, held = 0;
            bool armed = true;
            double top = 0;
            for (int s = 0; s < 720; s++)
            {
                bool onTarget = m.touchingWall && m.wallNormal.x * -target > 0.5;
                bool press = false;
                if (held > 0) held--;
                if (!onTarget) armed = true;
                else if (armed && !m.grounded) { press = true; armed = false; held = 3; }
                int wj = wallJumps;
                m.Step(new InputFrame { moveX = target, jump = new Button { pressed = press, held = press || held > 0 } }, dt);
                if (wallJumps > wj) target = -target;
                top = Math.Max(top, m.position.y);
                if (m.grounded && s > 30) break;
            }
            return (wallJumps, doubles, top);
        }
        (MoveState, double) BAfter(bool ride, int after, double pitchDeg = 0)
        {
            var m = new Mover(cfg.movement, N42WallWorld());
            n43Ev.Clear(); m.Events += e => n43Ev.Add(e.type + (e.reason != null ? ":" + e.reason : ""));
            m.SetMove43(Move43.Params.From(cfg, false, ride));
            m.Reset(3 - r - 0.005, 4, 0);
            m.velocity = new Vec3(0.5, 0, -10);
            for (int s = 0; s < 4; s++) m.Step(new InputFrame { moveX = 1 }, dt); // touch the wall
            m.Step(new InputFrame { jump = new Button { pressed = true, held = true } }, dt);
            for (int s = 0; s < after; s++) m.Step(new InputFrame(), dt);
            if (n42Trace) Console.WriteLine($"    events: {string.Join(",", n43Ev)}");
            if (n42Trace) Console.WriteLine($"    BAfter ride {ride}: before B state {m.state} sinceRide {m.m43.sinceRide} grace {m.m43.p.slamGraceSteps} v {F42(m.velocity.x)},{F42(m.velocity.y)},{F42(m.velocity.z)} y {F42(m.position.y)}");
            m.Step(new InputFrame { pitch = pitchDeg * Math.PI / 180, slide = new Button { pressed = true, held = true } }, dt);
            return (m.state, m.Speed);
        }
        var off = Chim(false);
        var on = Chim(true);
        Console.WriteLine($"n43move selftest: chimney (web Mover) off: wall jumps {off.Item1}, doubles {off.Item2}, top {F42(off.Item3)} | on: {on.Item1}, {on.Item2}, {F42(on.Item3)}");
        int late = MovementMath.StepsFor(cfg.Num("design.n43.ride43.slamGrace"), dt) + 6;
        var b0 = BAfter(false, 12);
        var b1 = BAfter(true, 12);
        var b2 = BAfter(true, 12, -60);
        var b3 = BAfter(true, late);
        Console.WriteLine($"  B 12 steps after a wall jump: off {b0.Item1} {F42(b0.Item2)} | on {b1.Item1} {F42(b1.Item2)} | on looking -60 {b2.Item1} | on after the grace {b3.Item1}");
        bool ok = off.Item1 == cfg.movement.wallJump.maxPerAirtime && on.Item1 > off.Item1 + 2 && on.Item3 > off.Item3 + 2 &&
                  b0.Item1 == MoveState.Slam && b0.Item2 < 0.01 && b1.Item1 == MoveState.Air && b1.Item2 > 9 && b2.Item1 == MoveState.Slam && b3.Item1 == MoveState.Slam;
        Console.WriteLine($"n43move selftest: {(ok ? "PASS" : "FAIL")} (Move43Tests' Mover asserts)");
        return ok ? 0 : 1;
    }

    // ------------------------------------------------------------------------------------------------ the reel's chimney, step for step

    /// <summary>FeelReel.N43.Move.cs's chimney pilot on a box chimney like n42-foundry a1's (walls `gap` apart, `top` m tall): the feet 1.5 m up,
    /// 0.02 m off wall A, still; look 15 deg up along the wall; a press on the first step on the target wall, held 0.03 s; the target flips on a
    /// WallJump. Prints each press and what it became, per half.</summary>
    static int N43ReelChimney(GameConfig cfg, string[] args)
    {
        double gap = 4.0, top = 6.0;
        for (int k = 2; k + 1 < args.Length; k++)
        {
            if (args[k] == "--gap") gap = double.Parse(args[k + 1], CultureInfo.InvariantCulture);
            if (args[k] == "--top") top = double.Parse(args[k + 1], CultureInfo.InvariantCulture);
        }
        double dt = 1 / cfg.movement.loop.simHz, r = cfg.movement.body.capsuleRadius, half = gap / 2;
        for (int side = 0; side < 2; side++)
        {
            var log = new List<MoverEvent>();
            Mover m = N43Mover(cfg, N43ChimneyWorld(half, top), side == 1, side == 1, log);
            m.Reset(-half + r + 0.02, 1.5, 0);
            Vec3 along = new Vec3(0, 0, -1);
            double yaw = YawOf(along), y0 = 1.5, hi = y0;
            Vec3 target = new Vec3(-1, 0, 0);
            bool armed = true;
            int presses = 0, seen = 0, tapUntil = -1;
            var sb = new System.Text.StringBuilder($"  reel chimney {(side == 0 ? "off" : "on ")} gap {F42(gap)} top {F42(top)}:");
            for (int s = 0; s < 720; s++)
            {
                bool onTarget = (m.touchingWall || m.state == MoveState.WallRide) && -(m.wallNormal.x * target.x + m.wallNormal.z * target.z) > 0.7;
                bool press = false;
                if (!onTarget) armed = true;
                else if (armed && !m.grounded) { press = true; armed = false; presses++; tapUntil = s + 4; sb.Append($" | p{presses} s{s} y {F42(m.position.y)} {m.state} wj {m.wallJumpsLeft}"); }
                double mx = target.x * Math.Cos(yaw) - target.z * Math.Sin(yaw), mz = -target.x * Math.Sin(yaw) - target.z * Math.Cos(yaw);
                m.Step(new InputFrame { moveX = mx, moveZ = mz, yaw = yaw, pitch = 15 * Math.PI / 180, jump = new Button { pressed = press, held = press || s < tapUntil } }, dt);
                for (; seen < log.Count; seen++)
                {
                    MoverEvent e = log[seen];
                    sb.Append($" {e.type}{(e.reason != null ? ":" + e.reason : "")}@{s}");
                    if (e.type == MoverEventType.WallJump) target = new Vec3(-target.x, 0, -target.z);
                }
                hi = Math.Max(hi, m.position.y);
                if (m.grounded && s > 60) break;
            }
            Console.WriteLine(sb.ToString());
            Console.WriteLine($"    presses {presses}, top {F42(hi - y0)} m over the start; refused: no-intent {m.flow34.ride35.r40NoIntent} wall42 no-intent {m.wall42NoIntent} landing {m.wall42Landing}");
        }
        return 0;
    }

    // ------------------------------------------------------------------------------------------------ gates

    static int N43Gates(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        bool ch = Array.IndexOf(args, "--chimney") >= 0, ri = Array.IndexOf(args, "--ride") >= 0, bo = Array.IndexOf(args, "--boost") >= 0;
        if (!ch && !ri && !bo) ch = ri = bo = true;
        SurvivalSim.Force43Chimney = !off && ch;
        SurvivalSim.Force43Ride = !off && ri;
        SurvivalSim.Force43Boost = !off && bo;
        for (int k = 2; k < args.Length; k++)
            if (args[k] == "--lift" && k + 1 < args.Length)
            {
                double lift = double.Parse(args[++k], CultureInfo.InvariantCulture);
                SurvivalSim.Probe43Boost = p => { p.push.lift = lift; return p; };
            }
        string[] plans = { "Design/levels/n40/kiln.plan.json", "Design/levels/n42/foundry.plan.json" };
        var pass = new List<string>();
        for (int k = 2; k < args.Length; k++)
        {
            if (args[k] == "--plan" && k + 1 < args.Length) plans = new[] { args[++k] };
            else if ((args[k] == "--only" || args[k] == "--trace") && k + 1 < args.Length) { pass.Add(args[k]); pass.Add(args[++k]); }
        }
        int red = 0;
        try
        {
            Console.WriteLine($"n43move gates: forced chimney43 {SurvivalSim.Force43Chimney}, ride43 {SurvivalSim.Force43Ride}, boost43 {SurvivalSim.Force43Boost}");
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
            SurvivalSim.Force43Chimney = SurvivalSim.Force43Ride = SurvivalSim.Force43Boost = null;
            SurvivalSim.Probe43Boost = null;
        }
        Console.WriteLine($"n43move gates: {plans.Length} plans, {red} not GREEN");
        return 0;
    }
}
