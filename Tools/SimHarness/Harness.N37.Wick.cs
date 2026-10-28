// NIGHT37 builder W (wick37): THE STACK and THE WICK, headless. One entry, N37Wick (declared and called in Program.cs).
//   wick37 [stack|climb|walkin|band|all] [--seeds N] [--cap S] [--mode climb|level|nodash] [--trace] [--off37]
//     The stack (Wick37Stack) is put at (2600, 0, 0) on the survival map (wick37CentreOverride; n37-foundry gives it by its row), the player in
//     its tunnel's mouth, wick37 forced on (--off37: off).
//     stack   the geometry: every panel (section, angle, bottom, top), the ledges, the cap, the triangle count; rays from the axis hit the wall.
//     climb   Wick37Pilot alone in the stack (no boss): the panels it rode in order, the time to ledge 1 / ledge 2 / the cap, the hot-wall burns.
//             --mode level: the CONTROL (a ride alone: pitch 0, no double jump, no dash) must NOT climb past the first panels; climb must reach
//             the cap. The hop: every gap made by the pitched leap, none by the level one.
//     walkin  BossRoomBegin("wick") owned (true, Wick37 alive, BossRoomSystem("wick") is it), the feet unmoved, no damage in the reveal.
//     band    the pilot with the fight (mortal, retries) over seeds 1..N: kill time p10 / p50 / p90, deaths, parries, pierces, launches,
//             the share of damage dealt while grounded and not moving (want ~0). PASS: p50 15-22 s, p90 <= 26 s.
using System;
using System.Collections.Generic;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static readonly Vec3 Stack37 = new Vec3(2600, 0, 0);
    static bool Trace37W;
    static int Every37W = 30;

    static partial void N37Wick(string cmd, string[] args, ref int code)
    {
        if (cmd != "wick37") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = ArgInt36(args, "--seeds", 10);
        double cap = ArgInt36(args, "--cap", 90);
        Trace37W = Array.IndexOf(args, "--trace") >= 0;
        Every37W = ArgInt36(args, "--every", 30);
        int mi = Array.IndexOf(args, "--mode");
        string mode = mi >= 0 && mi + 1 < args.Length ? args[mi + 1] : null;
        int fails = 0;
        if (what == "stack" || what == "all") fails += Stack37Facts();
        if (what == "climb" || what == "all")
        {
            if (mode == null || mode == "climb") fails += Climb37(Wick37Pilot.Mode.Climb, true);
            if (mode == "one") { var q = ClimbRun37(Wick37Pilot.Mode.Climb, 60, (uint)ArgInt36(args, "--seed", 1)); Console.WriteLine($"one: cap {q.capT:0.0} max {q.maxY:0.0}"); }
            if (mode == null || mode == "level") fails += Climb37(Wick37Pilot.Mode.Level, false);
            if (mode == "nodash") Climb37(Wick37Pilot.Mode.NoDash, true);
        }
        if (what == "gaps" || what == "all") fails += Gaps37W();
        if (what == "walkin" || what == "all") fails += Walkin37W();
        if (what == "band" || what == "all") fails += Band37W(seeds, cap);
        Console.WriteLine(fails == 0 ? "wick37: GREEN" : $"wick37: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static SurvivalSim Sim37W(uint seed, bool on)
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n37Override = on && !N37Blocks.ForceOff ? new N37Blocks { wick37 = true } : N37Blocks.Off;
        sim.wick37CentreOverride = Stack37;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        return sim;
    }

    static Vec3 Mouth37(SurvivalSim sim) => Stack37 + new Vec3(0, 0, sim.wick37Stack.p.radius + 1.5);

    static int Stack37Facts()
    {
        SurvivalSim sim = Sim37W(1, true);
        Wick37Stack s = sim.wick37Stack;
        Console.WriteLine($"wick37 stack: radius {s.p.radius} m, cap {s.p.capY} m (catwalk to {s.p.capRadius} m, wall {s.p.capWallHeight} m), {s.p.facets} facets, gap {2 * s.GapHalf:0.00} m x {s.p.gapHeight} m, ledges [{string.Join(", ", s.p.ledges)}], {s.tris.Count} triangles");
        foreach (Wick37Panel p in s.panels)
        {
            string gap = "";
            if (p.index + 1 < s.panels.Count)
            {
                Wick37Panel q = s.panels[p.index + 1];
                gap = $" -> next: end-to-start {Vec3.DistanceTo(new Vec3(p.end.x, 0, p.end.z), new Vec3(q.start.x, 0, q.start.z)):0.00} m, +{q.bottom - p.bottom:0.00} m";
            }
            Console.WriteLine($"  panel {p.index,2} section {p.section} angle {p.angle * 180 / Math.PI % 360,6:0} deg, y {p.bottom:0.00}..{p.top:0.00}{gap}");
        }
        int fails = 0;
        for (int k = 0; k < 8; k++)
        {
            double a = k * Math.PI / 4 + 0.1;
            var d = new Vec3(JsMath.Cos(a), 0, JsMath.Sin(a));
            Vec3 from = Stack37 + new Vec3(0, 30.5, 0);
            bool hit = sim.world.RayIntersect(from, d, out RayHit h);
            if (!hit || h.distance > s.CornerRadius + 0.01) fails++;
            if (Trace37W) Console.WriteLine($"  ray {a * 180 / Math.PI:0} deg at y 30.5: {(hit ? h.distance.ToString("0.00") : "miss")}");
        }
        Console.WriteLine($"  rays from the axis at 30.5 m hit the shaft or a panel inside its corner radius: {Pass36(fails == 0)}");
        return fails;
    }

    sealed class Climb37Run
    {
        public double ledge1 = -1, ledge2 = -1, capT = -1, maxY;
        public int burns, slag, rides;
        public List<int> ridden = new List<int>();
    }

    static Climb37Run ClimbRun37(Wick37Pilot.Mode mode, double seconds, uint seed = 1)
    {
        var r = new Climb37Run();
        SurvivalSim sim = Sim37W(seed, true);
        Wick37Stack s = sim.wick37Stack;
        sim.Respawn(Mouth37(sim), 0);
        sim.mover.invulnerable = true;
        var si = (ScriptedInput)sim.input;
        var hand = new Mulberry32(seed * 7919u + 3u);
        var pilot = new Wick37Pilot { mode = mode, shoot = false, parry = false, hand = hand };
        if (seed > 1) { pilot.scriptRel = 0.5 + 0.2 * hand.Next(); pilot.scriptPitch = 40 + 10 * hand.Next(); pilot.scriptLedgeRel = 0.6 + 0.2 * hand.Next(); } // a different hand each seed
        double yaw = 0, dt = sim.cfg.Dt;
        int steps = (int)(seconds / dt);
        for (int i = 0; i < steps; i++)
        {
            pilot.Step(sim, yaw, i);
            yaw = pilot.Move.yaw;
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            Vec3 l = sim.mover.position - Stack37;
            r.maxY = Math.Max(r.maxY, l.y);
            double tt = i * dt;
            if (sim.mover.grounded)
            {
                if (r.ledge1 < 0 && Math.Abs(l.y - s.p.ledges[0]) < 0.3) r.ledge1 = tt;
                if (r.ledge2 < 0 && s.p.ledges.Length > 1 && Math.Abs(l.y - s.p.ledges[1]) < 0.3) r.ledge2 = tt;
                if (r.capT < 0 && l.y > s.p.capY - 0.3) { r.capT = tt; break; }
            }
            if (Trace37W && i % Every37W == 0) Console.WriteLine($"    t {tt:0.00} pitch {pilot.Move.pitch * 57.3:0} jump {(pilot.Move.jump.held ? "H" : "-")}{(pilot.Move.jump.pressed ? "P" : "")} y {l.y:0.00} r {JsMath.Hypot(l.x, l.z):0.00} a {JsMath.Atan2(l.z, l.x) * 180 / Math.PI:0} {sim.mover.state} ride {(sim.mover.state == MoveState.WallRide ? Wick37Pilot.RiddenPanel(s, l, 0.35) : -1)} v ({sim.mover.velocity.x:0.0},{sim.mover.velocity.y:0.0},{sim.mover.velocity.z:0.0})");
        }
        r.burns = sim.wick37HotBurns;
        r.slag = sim.wick37SlagTouches;
        r.rides = pilot.rides;
        r.ridden = pilot.ridden;
        if (Trace37W) foreach (string ln in pilot.log) Console.WriteLine("   " + ln);
        return r;
    }

    static int Climb37(Wick37Pilot.Mode mode, bool mustClimb)
    {
        if (mustClimb)
        {
            // 10 seeds, each a different hand (release point 50-70 % of a panel, look 40-50 deg up, ledge release 60-80 %)
            int made = 0;
            var times = new List<double>();
            for (uint sd = 1; sd <= 10; sd++)
            {
                Climb37Run q = ClimbRun37(mode, 60, sd);
                if (q.capT > 0) { made++; times.Add(q.capT); }
                Console.WriteLine($"  seed {sd}: the cap {(q.capT > 0 ? q.capT.ToString("0.0") + " s" : "never (max feet " + q.maxY.ToString("0.0") + " m)")}, ledge 1 {(q.ledge1 < 0 ? "-" : q.ledge1.ToString("0.0"))}, ledge 2 {(q.ledge2 < 0 ? "-" : q.ledge2.ToString("0.0"))}, rides {q.rides}, hot burns {q.burns}");
            }
            times.Sort();
            Console.WriteLine($"wick37 climb ({mode}): {made}/10 reach the cap; median {(times.Count > 0 ? times[times.Count / 2].ToString("0.0") : "-")} s: {Pass36(made == 10)}");
            return made == 10 ? 0 : 1;
        }
        Climb37Run r = ClimbRun37(mode, 60);
        string T(double t) => t < 0 ? "never" : $"{t:0.0} s";
        Console.WriteLine($"wick37 climb ({mode}): panels ridden in order [{string.Join(" ", r.ridden)}], rides {r.rides}, max feet {r.maxY:0.0} m; ledge 1 {T(r.ledge1)}, ledge 2 {T(r.ledge2)}, the cap {T(r.capT)}; hot-wall burns {r.burns}");
        if (mustClimb)
        {
            bool ok = r.capT > 0;
            Console.WriteLine($"  the pitched leap climbs to the cap: {Pass36(ok)}");
            return ok ? 0 : 1;
        }
        bool held = r.ledge1 < 0 && r.maxY < 6;
        Console.WriteLine($"  the control (a ride alone, a level leap) does not climb (no ledge 1, max feet under 6 m): {Pass36(held)}");
        return held ? 0 : 1;
    }

    /// <summary>One leap from panel i: ride it from its start (feet bottom + 0.5, `speed` along it, jump held), release at fraction `rel` of its length
    /// with the look pitched `pitchDeg`, double jump (`dj`) once falling, keep moving at the next panel's face. Returns: did it ride panel i+1, and the
    /// margin (feet + 1.45 - next bottom) when it first came within 0.6 m of the next face's plane inside its span (-9 if never).</summary>
    static (bool made, double margin) Leap37W(int i, double rel, double speed, double pitchDeg, bool dj)
    {
        SurvivalSim sim = Sim37W(1, true);
        Wick37Stack s = sim.wick37Stack;
        Wick37Panel p = s.panels[i], q = s.panels[i + 1];
        Vec3 tg = p.end - p.start;
        double len = tg.Length;
        Vec3 tu = tg * (1 / len);
        double capR = sim.cfg.movement.body.capsuleRadius;
        Vec3 at = Stack37 + p.start + tu * 0.6 + p.normal * (capR + 0.02) + new Vec3(0, 0.5, 0);
        sim.Respawn(at, 0);
        sim.mover.invulnerable = true;
        sim.mover.velocity = tu * speed + p.normal * -1.0;
        var si = (ScriptedInput)sim.input;
        double yawRide = JsMath.Atan2(-tu.x, -tu.z);
        bool rode = false, released = false, doubled = false;
        double margin = -9;
        int since = 0;
        for (int k = 0; k < 360; k++)
        {
            Vec3 l = sim.mover.position - Stack37;
            double a = ((l.x - p.start.x) * tu.x + (l.z - p.start.z) * tu.z) / len;
            var f = new InputFrame { yaw = yawRide, pitch = pitchDeg * Math.PI / 180, moveZ = 1 };
            if (sim.mover.state == MoveState.WallRide) rode = true;
            if (!released && rode && (a >= rel || sim.mover.state != MoveState.WallRide)) { released = true; since = 0; }
            if (released)
            {
                since++;
                bool toL = q.section != p.section; double la = toL && p.section < s.ledgeAngle.Length ? s.ledgeAngle[p.section] - 0.15 : 0;
                Vec3 aim = toL ? new Vec3(JsMath.Cos(la) * 8.4, 0, JsMath.Sin(la) * 8.4) : q.start + (q.end - q.start) * 0.4;
                f.yaw = JsMath.Atan2(-(aim.x - l.x), -(aim.z - l.z));
                bool press = dj && !doubled && since > 4 && sim.mover.velocity.y < 1.0;
                if (press) doubled = true;
                f.jump = new Skygrind.Core.Button { held = since > 3 && !toL, pressed = press };
            }
            else f.jump = new Skygrind.Core.Button { held = true, pressed = k == 0 };
            si.SetLook(f.yaw, f.pitch);
            si.Set(f, new CombatInput { weapon = -1 });
            sim.Step();
            l = sim.mover.position - Stack37;
            if (released)
            {
                double off = (l.x - q.start.x) * q.normal.x + (l.z - q.start.z) * q.normal.z;
                Vec3 qt = q.end - q.start;
                double aq = ((l.x - q.start.x) * qt.x + (l.z - q.start.z) * qt.z) / Vec3.Dot(qt, qt);
                if (q.section == p.section && margin == -9 && off < capR + 0.6 && aq > -0.02 && aq < 1.02) margin = l.y + 1.45 - q.bottom;
                if (q.section != p.section && p.section < s.p.ledges.Length)
                {
                    double ly = s.p.ledges[p.section], ang = JsMath.Atan2(l.z, l.x) - s.ledgeAngle[p.section];
                    while (ang > Math.PI) ang -= 2 * Math.PI; while (ang < -Math.PI) ang += 2 * Math.PI;
                    if (margin == -9 && Math.Abs(ang) < s.p.ledgeArcDeg * Math.PI / 360) margin = l.y - ly;
                    if (sim.mover.grounded && Math.Abs(l.y - ly) < 0.3) return (true, margin);
                }
                else if (sim.mover.state == MoveState.WallRide && Wick37Pilot.RiddenPanel(s, l, capR) == q.index) return (true, margin);
                if (sim.mover.grounded || l.y < q.bottom - 4) break;
            }
        }
        return (false, margin);
    }

    static int Gaps37W()
    {
        SurvivalSim sim = Sim37W(1, true);
        Wick37Stack s = sim.wick37Stack;
        int fails = 0;
        const double Step = 0.1;
        Console.WriteLine($"wick37 gaps: per gap, the RELEASE WINDOW (metres of the panel where letting go rides the next panel; releases sampled every {Step * 100:0} % of the panel from 20 %) and the best MARGIN (feet + 1.45 - the next panel's bottom where the arc meets its face: the capsule's side over the face's bottom edge), for: 45 deg up + a double jump at 10.4 m/s (the ride) and at 20 m/s (the chain cap); 45 deg up, no double jump; the LEVEL control (pitch 0, no double jump). Every ride from the panel's low line (feet bottom + 0.5).");
        for (int i = 0; i + 1 < s.panels.Count; i++)
        {
            var sb = new System.Text.StringBuilder();
            bool any = false;
            int lvl = 0;
            double plen = (s.panels[i].end - s.panels[i].start).Length;
            foreach (var (sp, pitch, dj, name) in new[] { (10.4, 45.0, true, "up+dj 10.4"), (20.0, 45.0, true, "up+dj 20"), (10.4, 45.0, false, "up 10.4"), (10.4, 0.0, false, "LEVEL") })
            {
                int made = 0, tried = 0;
                double best = -9;
                for (double rel = 0.2; rel <= 1.001; rel += Step)
                {
                    tried++;
                    var (ok, m) = Leap37W(i, rel, sp, pitch, dj);
                    if (ok) { made++; best = Math.Max(best, m); }
                }
                if (name == "up+dj 10.4") any = made > 0;
                if (name == "LEVEL") lvl = made;
                sb.Append($" | {name}: {made * Step * plen:0.0} m{(best > -9 ? ", " + best.ToString("+0.00;-0.00") + " m" : "")}");
            }
            bool ledgeGap = s.panels[i + 1].section != s.panels[i].section;
            Console.WriteLine(ledgeGap ? $"  gap {i}->ledge {s.panels[i].section + 1} (+{s.p.ledges[s.panels[i].section] - s.panels[i].bottom:0.00} m over the panel's bottom; margin = feet - the ledge top over the arc){sb}" : $"  gap {i}->{i + 1} (+{s.panels[i + 1].bottom - s.panels[i].bottom:0.00} m){sb}");
            if (!any || (lvl > 0 && !ledgeGap)) fails++;
        }
        Console.WriteLine($"  every gap makeable at the ride's speed, none by the level control: {Pass36(fails == 0)}");
        return fails;
    }

    static int Walkin37W()
    {
        int fails = 0;
        bool on = !N37Blocks.ForceOff;
        SurvivalSim sim = Sim37W(1, on);
        sim.Respawn(Mouth37(sim), 0);
        sim.Step();
        Vec3 before = sim.mover.position;
        bool ok = sim.BossRoomBegin("wick", out Vec3 spawn, out double yaw);
        bool owned = on && sim.wick37 != null && sim.wick37.Alive && ReferenceEquals(sim.BossRoomSystem("wick"), sim.wick37);
        Console.WriteLine($"wick37 walkin: begin {ok}, owns 'wick' {owned}, walkIn {sim.boss37WalkInHere}, feet moved {Vec3.DistanceTo(before, sim.mover.position):0.000} m");
        if (!on) return owned ? 1 : 0;
        if (!(ok && owned && sim.boss37WalkInHere && Vec3.DistanceTo(before, sim.mover.position) < 1e-9)) fails++;
        Wick37 w = sim.wick37;
        double hp0 = w.body.hp;
        int reveal = MovementMath.StepsFor(w.p.revealSeconds, sim.cfg.Dt), fight = -1;
        var si = (ScriptedInput)sim.input;
        for (int i = 0; i < reveal + 30; i++)
        {
            si.Set(new InputFrame(), new CombatInput { weapon = -1 });
            sim.Step();
            if (fight < 0 && w.brain.Fighting) fight = i;
        }
        Console.WriteLine($"  the fight starts at {fight * sim.cfg.Dt:0.00} s (reveal {w.p.revealSeconds} s), damage in the reveal {hp0 - w.body.hp:0}, the core at {w.local.y:0.0} m: {Pass36(fight > 0 && hp0 == w.body.hp)}");
        if (!(fight > 0)) fails++;
        return fails;
    }

    sealed class Band37Run
    {
        public bool killed;
        public double capAt = -1;
        public double seconds, stillShare, dealt;
        public int stalls, stallPunches, stallLaunch, stallDash; public double stallDist;
        public int deaths, parries, pierces, launches, chips, emberHits, flareHits, slamHits, diveHits, slag, burns, windows, restarts, leg;
        public List<(string, int)> tells = new List<(string, int)>();
    }

    static Band37Run Fight37W(uint seed, double cap)
    {
        var r = new Band37Run();
        SurvivalSim sim = Sim37W(seed, true);
        sim.Respawn(Mouth37(sim), 0);
        if (!sim.BossRoomBegin("wick", out Vec3 spawn, out double yaw0)) return r;
        Wick37 w = sim.wick37;
        var si = (ScriptedInput)sim.input;
        var pilot = new Wick37Pilot { hand = new Mulberry32(seed * 7919u + 13u) };
        double yaw = 0, dt = sim.cfg.Dt, still = 0;
        int steps = (int)(cap / dt), reveal = MovementMath.StepsFor(w.p.revealSeconds, dt);
        double lastHp = w.body.hp;
        int capStep = -1;
        sim.health.max = sim.health.hp = 1e6;
        int i = 0;
        for (; i < steps + reveal && w.Alive; i++)
        {
            pilot.Step(sim, yaw, i);
            yaw = pilot.Move.yaw;
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            if (capStep < 0 && sim.mover.grounded && (sim.mover.position - Stack37).y > sim.wick37Stack.p.capY - 0.3) capStep = i;
            double d = lastHp - w.body.hp;
            if (d > 0 && sim.mover.grounded && JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z) < 1.0) still += d;
            lastHp = w.body.hp;
            // a "death" (the player's 100 hp taken) is counted, never played out (a dead run stops the input): the bar and the leg go on
            if (sim.health.hp < sim.health.max - 100) { r.deaths++; sim.health.hp = sim.health.max; }
            if (Trace37W && i % Every37W == 0) Console.WriteLine($"    t {i * dt:0.0} y {(sim.mover.position - Stack37).y:0.0} {sim.mover.state} brain {w.brain.state} {w.AttackId} leg {w.brain.leg} hp {w.body.hp:0} stagger {w.brain.stagger:0.0} slag {sim.wick37Slag.y:0.0} php {sim.health.hp:0} stalled {w.stalled} dist {Vec3.DistanceTo(w.Centre, sim.Eye):0.0} embers {w.embers.Count} core {(w.Centre - Stack37).y:0.0} pl {(sim.mover.position - Stack37).y:0.0} r {JsMath.Hypot((sim.mover.position - Stack37).x, (sim.mover.position - Stack37).z):0.0} a {JsMath.Atan2((sim.mover.position - Stack37).z, (sim.mover.position - Stack37).x) * 57.3:0} v {JsMath.Hypot(sim.mover.velocity.x, sim.mover.velocity.z):0.0} mv {pilot.Move.moveX:0.0},{pilot.Move.moveZ:0.0} dash {pilot.Move.dash.pressed} punch {pilot.Combat.punch.pressed}");
        }
        r.killed = !w.Alive;
        r.seconds = (i - reveal) * dt;
        r.capAt = capStep >= 0 ? (capStep - reveal) * dt : -1;
        r.dealt = w.dealt;
        r.stillShare = w.dealt > 0 ? still / w.dealt : 0;
        r.stallDash = w.stallDashes; r.stalls = w.stalls; r.stallPunches = w.stallPunches; r.stallLaunch = w.stallLaunchPunches; r.stallDist = w.lastStallPunchDist;
        r.parries = w.parries; r.pierces = w.pierces; r.launches = w.launches; r.chips = w.chips; r.emberHits = w.emberHits; r.flareHits = w.flareHits;
        r.slamHits = w.slamHits; r.diveHits = w.diveHits; r.slag = sim.wick37SlagTouches; r.burns = sim.wick37HotBurns; r.windows = w.brain.windows; r.restarts = w.restarts; r.leg = w.brain.leg;
        r.tells = new List<(string, int)>(w.brain.tellLog);
        return r;
    }

    static int Band37W(int seeds, double cap)
    {
        var runs = new List<Band37Run>();
        for (uint s = 1; s <= seeds; s++)
        {
            Band37Run r = Fight37W(s, cap);
            runs.Add(r);
            Console.WriteLine($"  seed {s}: {(r.killed ? "killed" : "ALIVE")} {r.seconds:0.0} s (the cap at {r.capAt:0.0} s), leg {r.leg}, deaths {r.deaths}, restarts {r.restarts}, parries {r.parries}, pierces {r.pierces}, windows {r.windows}, launches {r.launches} (stalls {r.stalls}, dash steps in a stall {r.stallDash}, punches in a stall {r.stallPunches}, with the dash {r.stallLaunch}, last at {r.stallDist:0.0} m), chips {r.chips}; hits taken: ember {r.emberHits} flare {r.flareHits} slam {r.slamHits} dive {r.diveHits} slag {r.slag} hot {r.burns}; still share {r.stillShare:P1}");
        }
        var ks = runs.Where(x => x.killed).Select(x => x.seconds).OrderBy(x => x).ToList();
        double P(double q) => ks.Count == 0 ? double.NaN : ks[Math.Min(ks.Count - 1, (int)Math.Floor(q * (ks.Count - 1) + 0.5))];
        var tellSteps = runs.SelectMany(x => x.tells).GroupBy(x => x.Item1).Select(g => $"{g.Key} x{g.Count()} {g.Average(x => x.Item2) / 120.0:0.00} s");
        var cs = runs.Where(x => x.killed && x.capAt >= 0).Select(x => x.seconds - x.capAt).OrderBy(x => x).ToList();
        double Q(double q) => cs.Count == 0 ? double.NaN : cs[Math.Min(cs.Count - 1, (int)Math.Floor(q * (cs.Count - 1) + 0.5))];
        Console.WriteLine($"wick37 band: killed {ks.Count}/{runs.Count}; TOTAL (walk-in + climb + cap) p10 {P(0.1):0.0} s, p50 {P(0.5):0.0} s, p90 {P(0.9):0.0} s; FROM THE CAP p10 {Q(0.1):0.0} s, p50 {Q(0.5):0.0} s, p90 {Q(0.9):0.0} s; still share max {runs.Max(x => x.stillShare):P1}; tells {string.Join(", ", tellSteps)}");
        // DECIDE37 D-7 (the lead, round 3): THE WICK's band is climb + fight (the 15-22 s band was set against health-sponge fights); from the cap is printed too
        bool ok = ks.Count == runs.Count && P(0.9) <= 40 && runs.Max(x => x.stillShare) <= 0.01;
        Console.WriteLine($"  band (D-7: all killed, total p90 <= 40 s, still share <= 1 %): {Pass36(ok)}");
        return ok ? 0 : 1;
    }
}
