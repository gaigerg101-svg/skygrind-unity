// NIGHT47 builder W (G1 / G2; NIGHT47.md section 3 "The climb": "Measure first"): `n47wick`, THE WICK's climb meters, headless. Hooked without a
// Program.cs line: a module initializer runs the command and exits when argv[1] is "n47wick" (Harness.N46.Flow.cs's pattern); it applies the off
// switches itself (--off47: NightRows.ForceOffFrom).
//   n47wick reach [--rows r1,r2] [--off47] [--trace]
//     THE REACH TABLE: the max height gain (feet, m, over the feet where the move starts) of a jump, a double jump, a wall jump, a wall jump chain, a
//     ride jump-off at +45 deg pitch (and 0 deg), each from rest and from a ride, on a TEST WALL (a 40 m tall, 40 m long, 1 m thick box at
//     x = 3000, its face toward -x; a 60 x 60 m floor slab) put in the survival map's world as its own layer (GameWorld.SetLayer "n47wick"), so
//     every movement row runs as configured (the ON game) and nothing else stands near. A press is one step's `pressed` with `held` kept while the
//     move wants it; a double jump is swept over every press step 2..90 (the best and the at-apex one are printed).
//   n47wick chain [--rows ...] [--off47] [--trace]
//     FIVE WALL JUMPS on the test wall from rest, pressing into it (moveZ 1), each press at the first step the feet fall (vy <= 0) after the
//     last one: per press the event it made (WallJump / DoubleJump / none), the feet at the press, the peak after it, and the gain per press
//     (peak_i - peak_i-1). Then the CHIMNEY: two walls 4 m apart, the look turned to the other wall after each press. GG: "every attempt lowers
//     your momentum slightly so you end up slipping further down each time".
//   n47wick stackchain [--rows ...]
//     THE STACK (today's wick37 stack at (2600, 0, 0)): five panel-to-panel jump-offs from panel 3, per gap the feet at the release and at the
//     next attach over each panel's bottom, for a level look while riding vs the look up (ride38's drift), releases at 50 % / 80 %.
//   n47wick sloppy [--seeds 40] [--cap 90] [--control] [--rows ...] [--trace]
//     THE SLOPPY CLIMB PILOT (G1; W-cards.md section 5) on THE STACK from its mouth to the cap, no boss: the cap share, the climb time, retries,
//     the deepest fall, every try and the FIRST try per gap. --control also runs Wick37Pilot's own hand.
// Every number is the real Mover in the real sim (SurvivalSim.Headless on the survival map, survival mode, the husks frozen, invulnerable).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void W47WickInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n47wick") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            code = W47Wick(args);
        }
        catch (Exception e) { Console.WriteLine("n47wick: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static readonly Vec3 W47T = new Vec3(3000, 0, 0); // the test wall's face is x = W47T.x (its normal -x), the floor's top y = 0
    static bool W47Trace;
    static string[] W47Rows = new string[0];

    static int W47Wick(string[] args)
    {
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "reach";
        W47Trace = Array.IndexOf(args, "--trace") >= 0;
        int ri = Array.IndexOf(args, "--rows");
        W47Rows = ri >= 0 && ri + 1 < args.Length ? args[ri + 1].Split(',', StringSplitOptions.RemoveEmptyEntries) : new string[0];
        Console.WriteLine($"n47wick {what}: rows forced on [{string.Join(",", W47Rows)}], off from night {NightRows.ForceOffFrom}");
        if (what == "reach" || what == "all") W47Reach();
        if (what == "chain" || what == "all") { W47Chain(false); W47Chain(true); }
        if (what == "sloppy")
        {
            int si = Array.IndexOf(args, "--seeds"), ci = Array.IndexOf(args, "--cap");
            int seeds = si >= 0 ? int.Parse(args[si + 1], CultureInfo.InvariantCulture) : 40;
            double cap = ci >= 0 ? double.Parse(args[ci + 1], CultureInfo.InvariantCulture) : 90;
            if (Array.IndexOf(args, "--control") >= 0) W47Sloppy(seeds, false, cap);
            W47Sloppy(seeds, true, cap);
        }
        if (what == "chip") W47Chip();
        if (what == "windows")
        {
            // G1: "no segment needs an input inside a window under 150 ms": per ride gap (panel -> next panel in its section), the release points
            // (5 % steps of the panel, 10-100 %) that ride the next panel, look level on the ride and 45 deg at the release (the taught move);
            // the window in time = the made points x 5 % x the panel's length / the 10.4 m/s ride
            SurvivalSim s0 = W47Stack(1);
            Wick37Stack st = s0.wick37Stack;
            Console.WriteLine($"n47wick windows (rows [{string.Join(",", W47Rows)}]): {st.panels.Count} panels");
            for (int i = 0; i + 1 < st.panels.Count; i++)
            {
                if (st.panels[i + 1].section != st.panels[i].section) continue;
                int made = 0, n = 0, first = -1, lastOk = -1;
                for (double rel = 0.10; rel <= 1.0001; rel += 0.05) { n++; if (W47StackChainRun(i, 1, rel, 0, 45, true) > 0) { made++; if (first < 0) first = n; lastOk = n; } }
                double len = (st.panels[i].end - st.panels[i].start).Length;
                double win = made * 0.05 * len / 10.4;
                Console.WriteLine($"  gap {i} -> {i + 1}: {made} / {n} release points ride the next panel (from {(first < 0 ? "-" : (10 + 5 * (first - 1)) + " %")} to {(lastOk < 0 ? "-" : (10 + 5 * (lastOk - 1)) + " %")}): a window of {win * 1000:0} ms {(win >= 0.15 ? "(>= 150 ms)" : "(UNDER 150 ms)")}");
            }
        }
        if (what == "stackchain" || what == "all")
        {
            // the sloppy-looking hand (look level on the ride, up only at the release) vs the skilled one (look up the whole ride: ride38's drift)
            foreach (double rel in new[] { 0.5, 0.8 })
            {
                W47StackChain(3, 5, rel, 0, 45);
                W47StackChain(3, 5, rel, 45, 45);
            }
        }
        return 0;
    }

    /// <summary>The sim with the test layer: one wall (face x = T.x, toward -x) or, chimney, a second wall whose face is x = T.x - 4 (toward +x).</summary>
    static SurvivalSim W47Sim(bool chimney)
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        if (W47Rows.Length > 0)
        {
            var o = sim.rowsRegistry.Defaults();
            foreach (string r in W47Rows) o = o.Set(r, true);
            sim.rowsOverride = o;
        }
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(1, null, null);
        sim.husks.frozen = true;
        var boxes = new List<(Vec3, Vec3)>
        {
            (W47T + new Vec3(-30, -1, -30), W47T + new Vec3(30, 0, 30)), // the floor slab
            (W47T + new Vec3(0, 0, -20), W47T + new Vec3(1, 40, 20)),     // the test wall
        };
        if (chimney) boxes.Add((W47T + new Vec3(-5, 0, -20), W47T + new Vec3(-4, 40, 20)));
        var layer = new BoxLayer();
        layer.SetBoxes(boxes);
        sim.world.SetLayer("n47wick", layer);
        return sim;
    }

    /// <summary>THE WICK's human-model policy (n47human's hook, Harness.N47.Human.cs; W-cards.md: the verb each phase stars): with wick47 off it does
    /// nothing (Wick37Pilot's own fight). P1 THE FLOOR: circle the axis at 5 m looking at the WICK (the pilot's own parries and shots; the human's
    /// trigger fires), step off a lit vent; P2 THE CLIMB: the pilot's climb as is; P3 THE CAP: the pilot's cap fight, and when the core is OPEN
    /// run at it and punch inside 3 m (the parry language: the punch is the answer). ctx.tell on the new attacks' tells. W47STATS=1 prints each
    /// phase's start and the end per seed.</summary>
    static void H47PolicyWick(H47Ctx ctx, ref InputFrame move, ref CombatInput combat)
    {
        if (ctx.boss != "wick") return;
        SurvivalSim sim = ctx.sim;
        Wick37 w = sim.wick37;
        if (w == null || !w.w47 || w.brain == null) return;
        bool stats = Environment.GetEnvironmentVariable("W47STATS") == "1";
        int ph = w.Phase47;
        if (Environment.GetEnvironmentVariable("W47TRACE") == "1" && ctx.step % 240 == 0) { Vec3 tl = sim.mover.position - w.origin; Console.WriteLine($"    W47T {ctx.step / 120.0:0} s ph {ph} feet ({tl.x:0.0}, {tl.y:0.00}, {tl.z:0.0}) {sim.mover.state} hp {sim.health.hp:0} slag {sim.wick37Slag?.y:0.0} netY {sim.wick47Nets} atk {w.AttackId} {w.brain.state}"); }
        string key = "ph" + ph;
        if (!ctx.state.ContainsKey(key)) { ctx.state[key] = ctx.step; if (stats) Console.WriteLine($"    W47 phase {ph} at {ctx.step / 120.0:0.0} s, bar {w.HpFrac * 100:0} %"); }
        if (stats && w.brain.state == Boss37State.Dying && !ctx.state.ContainsKey("dead"))
        {
            ctx.state["dead"] = ctx.step;
            Console.WriteLine($"    W47 dead at {ctx.step / 120.0:0.0} s: geysers {w.geysers47} (hits {w.geyserHits47}), curtains {w.curtains47}, rings {w.rings47} (hits {w.ringHits47}), charges {w.charges47} (hits {w.chargeHits47}), opens {w.opens47}, core punches {w.corePunches47} (missed {w.coreMissed47}), parries {w.parries}, pierces {w.pierces}, nets {sim.wick47Nets}, launch {w.lastLaunch47:0.0} m/s, hitstops {sim.wick47Hitstops}, ride-parries {w.rideParries47} (cracks +{w.rideCracksMade47}), intros {sim.wick47Intros}");
        }
        // G7 / G8: the launch's distance and landing after each core punch; falls into the shaft in P3
        {
            Vec3 lp = sim.mover.position - w.origin;
            int cp = ctx.state.TryGetValue("cp", out object cpo) ? (int)cpo : 0;
            if (w.corePunches47 > cp) { ctx.state["cp"] = w.corePunches47; ctx.state["from"] = sim.mover.position; ctx.state["air"] = 0; }
            if (ctx.state.TryGetValue("from", out object fo) && sim.mover.grounded && (int)ctx.state["air"] > 5)
            {
                Vec3 f0 = (Vec3)fo;
                double dist = JsMath.Hypot(sim.mover.position.x - f0.x, sim.mover.position.z - f0.z);
                bool onCap = lp.y >= w.stack.p.capY - 0.2;
                if (stats) Console.WriteLine($"    W47 launch {dist:0.0} m, landed y {lp.y:0.00} ({(onCap ? "on the cap" : "OFF the cap")})");
                ctx.state.Remove("from");
            }
            if (ctx.state.ContainsKey("from") && !sim.mover.grounded) ctx.state["air"] = (int)ctx.state["air"] + 1;
            if (ph == 3 && lp.y < w.stack.p.capY - 1 && JsMath.Hypot(lp.x, lp.z) < w.stack.p.radius && !ctx.state.ContainsKey("fell"))
            {
                ctx.state["fell"] = 1;
                if (stats) Console.WriteLine($"    W47 FELL into the shaft in P3 at {ctx.step / 120.0:0.0} s (glass {sim.Wick47GlassOn})");
            }
        }
        // the tells of the new attacks (the brain's Tell state is the pilot's 'telling' already; mark it for the reaction model)
        string id = w.AttackId;
        if (w.brain.state == Boss37State.Tell && w.brain.entered && (id == Wick37.Geyser47 || id == Wick37.Ring47 || id == Wick37.Charge47 || id == Wick37.Curtain47)) ctx.tell = true;
        Vec3 me = sim.mover.position, c = w.Centre, o = w.origin, l = me - o;
        Vec3 eye = new Vec3(me.x, me.y + 1.6, me.z), to = c - eye;
        double lookYaw = JsMath.Atan2(-to.x, -to.z), lookPitch = JsMath.Atan2(to.y, JsMath.Hypot(to.x, to.z));
        if (ph == 1)
        {
            // circle at 5 m (strafe round the axis), look at the WICK
            double r = JsMath.Hypot(l.x, l.z), a = JsMath.Atan2(l.z, l.x);
            double tx = JsMath.Cos(a + 0.6) * 5 - l.x, tz = JsMath.Sin(a + 0.6) * 5 - l.z; // the next point on the 5 m circle
            double ty = JsMath.Atan2(-tx, -tz);
            move.yaw = lookYaw;
            move.pitch = lookPitch;
            double rel = ty - lookYaw;
            move.moveZ = JsMath.Cos(rel);
            move.moveX = -JsMath.Sin(rel);
            move.jump = default;
            move.dash = default;
            // a lit vent under the feet: jump (the geyser's counter)
            if (w.brain.state == Boss37State.Tell && id == Wick37.Geyser47)
                for (int k = 0; k < w.geyserN47; k++)
                {
                    Vec3 g = w.geyserAt47[k];
                    if ((g.x - l.x) * (g.x - l.x) + (g.z - l.z) * (g.z - l.z) < (w.p47.geyserRadius + 0.8) * (w.p47.geyserRadius + 0.8)) { move.moveZ = -JsMath.Cos(rel); move.moveX = JsMath.Sin(rel); }
                }
            return;
        }
        if (ph == 2)
        {
            // round 3: the bottom section is steps (and every ledge restart a mount): the step pilot, as n47wick sloppy (its own errors with W47SLOPPY)
            if (!ctx.state.TryGetValue("steps", out object sto)) { sto = new W47StepState(); ctx.state["steps"] = sto; }
            InputFrame sf = move;
            if (W47StepFrame(sim, ctx.step, (W47StepState)sto, () => ctx.rng.Next(), Environment.GetEnvironmentVariable("W47SLOPPY") == "1", ref sf, null)) { move = sf; return; }
        }
        if (ph == 2 && Environment.GetEnvironmentVariable("W47SLOPPY") == "1")
        {
            // the coordinator's caution 1: P2's climb by the SLOPPY hand with the boss attacking (W47Sloppy's errors, from the human's own stream)
            bool riding = sim.mover.state == MoveState.WallRide;
            bool was = ctx.state.TryGetValue("wasRide", out object wr) && (bool)wr;
            if (riding && !was)
            {
                ctx.state["eyaw"] = (ctx.rng.Next() * 24 - 12) * Math.PI / 180;
                ctx.state["epitch"] = (ctx.rng.Next() * 24 - 12) * Math.PI / 180;
                ctx.state["level"] = ctx.rng.Next() < 0.5;
                ctx.state["late"] = ctx.rng.Next() < 0.25;
            }
            ctx.state["wasRide"] = riding;
            if (riding && ctx.state.ContainsKey("eyaw"))
            {
                move.yaw += (double)ctx.state["eyaw"];
                move.pitch = ((bool)ctx.state["level"] ? 0 : move.pitch) + (double)ctx.state["epitch"];
                bool held = ctx.state.TryGetValue("held", out object ho) && (bool)ho;
                if (held && !move.jump.held && (bool)ctx.state["late"]) { ctx.state["lateLeft"] = 18; ctx.state["late"] = false; }
            }
            int ll = ctx.state.TryGetValue("lateLeft", out object llo) ? (int)llo : 0;
            if (ll > 0) { ctx.state["lateLeft"] = ll - 1; move.jump = new Skygrind.Core.Button { held = true }; }
            int pend = ctx.state.TryGetValue("pend", out object po) ? (int)po : -1;
            if (move.jump.pressed && !riding && pend < 0) { pend = (int)(ctx.rng.Next() * 28.8); move.jump = new Skygrind.Core.Button { held = move.jump.held }; }
            if (pend >= 0) { if (pend == 0) move.jump = new Skygrind.Core.Button { held = true, pressed = true }; pend--; }
            ctx.state["pend"] = pend;
            ctx.state["held"] = move.jump.held;
        }
        if (ph == 3 && w.coreOpen47 > 0)
        {
            // the open core: run at it and punch inside 3 m
            move.yaw = lookYaw;
            move.pitch = lookPitch;
            move.moveZ = 1;
            move.moveX = 0;
            move.jump = default;
            double d = Vec3.DistanceTo(eye, c);
            combat.punch = new Skygrind.Core.Button { pressed = d < w.p47.punchReach + w.p.cageR - 0.3 && ctx.step % 12 == 0, held = false };
            combat.fire = default;
        }
        if (ph == 3 && w.ring47 > 0 && !sim.mover.grounded == false)
        {
            // the ring sweeping out: jump it as it comes
            double d = JsMath.Hypot(me.x - c.x, me.z - c.z);
            if (Math.Abs(d - w.ring47) < 2.5) move.jump = new Skygrind.Core.Button { pressed = true, held = true };
        }
    }

    /// <summary>G4's second half: "10 s of steady fire with the starting gun outside any window moves the bar by >= 5 %". The human pilot's WICK
    /// setup (H47Wick: the survival map's stack, the rows from --rows), the reveal waited out, then 10 s standing 5 m out on the floor, the look on
    /// the cage, a KESTREL click every 15 steps (8 Hz, the human model's trigger), no punch: the bar before / after, and the chips vs the full hits
    /// (a full hit is a window; the line counts both). Every shot is aimed (a hit share near 100 %: the human's 45 % would scale the move).</summary>
    static void W47Chip()
    {
        H47Rows = W47Rows;
        H47Fight f = H47Wick(1);
        if (f == null) { Console.WriteLine("n47wick chip: no fight"); return; }
        SurvivalSim sim = f.sim;
        Wick37 w = sim.wick37;
        var si = (ScriptedInput)sim.input;
        for (int i = 0; i < f.reveal + 60; i++) { si.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); }
        Vec3 at = w.origin + new Vec3(0, 0, 5);
        sim.Respawn(at, 0);
        sim.mover.invulnerable = true;
        sim.health.max = sim.health.hp = 1e6;
        int slot = sim.loadout.owned.IndexOf("revolver");
        double before = w.HpFrac;
        int chips0 = w.chips, fulls0 = w.fulls;
        for (int i = 0; i < 1200; i++)
        {
            Vec3 eye = sim.Eye, d = w.Centre - eye;
            double yaw = JsMath.Atan2(-d.x, -d.z), pitch = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
            si.SetLook(yaw, pitch);
            si.Set(new InputFrame { yaw = yaw, pitch = pitch }, new CombatInput { weapon = i == 0 ? slot : -1, fire = new Skygrind.Core.Button { pressed = i % 15 == 0, held = i % 15 < 2 } });
            sim.Step();
            if (!w.Alive) break;
        }
        double moved = before - w.HpFrac;
        Console.WriteLine($"n47wick chip (rows [{string.Join(",", W47Rows)}]): 10 s of KESTREL clicks at 8 Hz from the floor, every shot aimed: the bar {before * 100:0.0} % -> {w.HpFrac * 100:0.0} % (moved {moved * 100:0.0} %; G4 asks >= 5 %); hits that chipped {w.chips - chips0}, full (a window) {w.fulls - fulls0}; phase {w.Phase47}");
    }

    /// <summary>THE STEPS' pilot (round 3: the stack's bottom section is plain step ledges, design.n47.wick47.numbers.stack.stepCount): from the
    /// floor or a step, run along the shaft to the next step's centre and jump inside 5 m of it, double jump at the apex; `sloppy`: the yaw off
    /// by +-12 deg per attempt, each press 0-240 ms late (the sloppy pilot's errors). Returns false once the feet stand on the last step (ledge 1:
    /// the ride pilot takes over). Judges each attempt: the landing on the target step is a success; first tries per step in `first`.</summary>
    sealed class W47StepState
    {
        public int target = -2, jumped = -1, pend = -1, cur = -1;
        public bool doubled, airborne;
        public int mounting = -1;
        public Vec3 from;
        public double err;
        public readonly HashSet<int> seen = new HashSet<int>();
    }

    static bool W47StepFrame(SurvivalSim sim, int i, W47StepState k, Func<double> rnd, bool sloppy, ref InputFrame f, Dictionary<int, (int ok, int n)> first)
    {
        Wick37Stack s = sim.wick37Stack;
        if (s == null || s.stepTops.Count == 0) return false;
        Vec3 o = sim.wick37Centre.Value, l = sim.mover.position - o;
        double last = s.stepTops[s.stepTops.Count - 1];
        bool grounded = sim.mover.grounded;
        if (grounded && k.airborne && k.jumped >= 0)
        {
            // the step attempt's landing (judged first, so the last step's landing counts too)
            int on = -1;
            for (int j = 0; j < s.stepTops.Count; j++) if (Math.Abs(l.y - s.stepTops[j]) < 0.3) on = j;
            bool ok = on == k.jumped;
            if (!ok && W47Trace && k.jumped >= 3) Console.WriteLine($"      STEP MISS target {k.jumped} (top {s.stepTops[k.jumped]:0.0}, {s.stepAngles[k.jumped] * 180 / Math.PI:0} deg): landed on {on} at ({l.x:0.0}, {l.y:0.00}, {l.z:0.0}) r {JsMath.Hypot(l.x, l.z):0.0} a {JsMath.Atan2(l.z, l.x) * 180 / Math.PI:0} deg; took off at ({k.from.x:0.0}, {k.from.y:0.00}, {k.from.z:0.0}) a {JsMath.Atan2(k.from.z, k.from.x) * 180 / Math.PI:0} err {k.err * 180 / Math.PI:0}");
            if (first != null && k.seen.Add(k.jumped)) { var e = first.TryGetValue(k.jumped, out var v) ? v : (0, 0); first[k.jumped] = (e.Item1 + (ok ? 1 : 0), e.Item2 + 1); }
            k.jumped = -1;
        }
        if (sim.mover.state == MoveState.WallRide) { k.mounting = -1; return false; }
        // a LEDGE MOUNT (ledge 1 = the last step, ledge 2, ...; never the cap): run along the ledge to the next section's first panel, jump at its
        // face (35 % along, 0.6 m in), jump held; the ride pilot takes over once riding (Wick37Pilot's own ledge start stalls: round 3's trace)
        if (grounded)
        {
            k.mounting = -1;
            double rr0 = JsMath.Hypot(l.x, l.z);
            for (int j = 0; j + 1 < s.p.ledges.Length; j++) if (Math.Abs(l.y - s.p.ledges[j]) < 0.3 && rr0 > s.p.radius - JsMath.Max(s.p.ledgeWidth, s.p.stepDepth) - 0.6) k.mounting = j;
            int lip = s.LipAt(l, 0.3); // NIGHT48 W (climbTop48): on a lip, the mount is the next panel (the lip's panel + 1); 1000 + the panel's index
            if (lip >= 0 && s.lipPanel[lip] + 1 < s.panels.Count) k.mounting = 1000 + s.lipPanel[lip] + 1;
        }
        if (k.mounting >= 0)
        {
            int pi = -1;
            if (k.mounting >= 1000) pi = k.mounting - 1000; // NIGHT48 W: a lip's next panel
            else for (int j = 0; j < s.panels.Count; j++) if (s.panels[j].section == k.mounting + 1) { pi = j; break; }
            if (pi < 0) return false;
            Wick37Panel p0 = s.panels[pi];
            Vec3 a0 = p0.start + (p0.end - p0.start) * 0.35 - p0.normal * 0.6;
            double pa = JsMath.Atan2(p0.start.z, p0.start.x), la = JsMath.Atan2(l.z, l.x), dd = pa - la;
            while (dd > Math.PI) dd -= 2 * Math.PI; while (dd < -Math.PI) dd += 2 * Math.PI;
            if (grounded && Math.Abs(dd) > 0.45) a0 = new Vec3(JsMath.Cos(la + Math.Sign(dd) * 0.35) * 8.2, 0, JsMath.Sin(la + Math.Sign(dd) * 0.35) * 8.2);
            f = new InputFrame { yaw = JsMath.Atan2(-(a0.x - l.x), -(a0.z - l.z)) + k.err, moveZ = 1 };
            if (grounded && Math.Abs(dd) <= 0.45 && k.pend < 0) k.pend = sloppy ? (int)(rnd() * 28.8) : 0;
            bool pr = false;
            if (k.pend >= 0) { if (k.pend == 0) pr = true; k.pend--; }
            f.jump = new Skygrind.Core.Button { held = !grounded || pr, pressed = pr };
            if (grounded) k.cur = s.stepTops.Count - 1;
            return true;
        }
        if (l.y >= last - 0.3) return false;
        if (grounded)
        {
            int cur = -1;
            for (int j = 0; j < s.stepTops.Count; j++) if (Math.Abs(l.y - s.stepTops[j]) < 0.3) cur = j;
            if (k.airborne && k.jumped >= 0)
            {
                bool ok = cur == k.jumped;
                if (first != null && k.seen.Add(k.jumped)) { var e = first.TryGetValue(k.jumped, out var v) ? v : (0, 0); first[k.jumped] = (e.Item1 + (ok ? 1 : 0), e.Item2 + 1); }
                k.jumped = -1;
            }
            k.airborne = false;
            k.cur = cur;
            if (k.target != cur + 1) { k.target = cur + 1; k.err = sloppy ? (rnd() * 24 - 12) * Math.PI / 180 : 0; k.doubled = false; }
        }
        else { if (!k.airborne) k.from = l; k.airborne = true; }
        int t = Math.Max(0, Math.Min(s.stepTops.Count - 1, k.target));
        double rr = s.p.radius - 1.3, ta = s.stepAngles[t]; // the aim stays 1.3 m off the wall (a deeper step is margin inward)
        var aim = new Vec3(JsMath.Cos(ta) * rr, 0, JsMath.Sin(ta) * rr);
        double yaw = JsMath.Atan2(-(aim.x - l.x), -(aim.z - l.z)) + k.err;
        double dist = JsMath.Hypot(aim.x - l.x, aim.z - l.z);
        f = new InputFrame { yaw = yaw, pitch = 0, moveZ = dist > 0.6 || !grounded ? 1 : 0 };
        bool want = false;
        bool under = false; // under another step's overhang: walk out first (a jump there bumps its underside)
        if (l.y < 0.3) { double ra = JsMath.Hypot(l.x, l.z), aa = JsMath.Atan2(l.z, l.x); for (int j = 0; j < s.stepTops.Count; j++) { double dj = aa - s.stepAngles[j]; while (dj > Math.PI) dj -= 2 * Math.PI; while (dj < -Math.PI) dj += 2 * Math.PI; if (Math.Abs(dj) < s.StepArc(j) / 2 + 0.08 && ra > s.p.radius - s.StepDepth(j) - 0.5 && s.stepTops[j] - 0.4 > l.y + 0.3 && s.stepTops[j] < l.y + 4.5) under = true; } }
        if (grounded && !under && dist < 5.0 && k.jumped < 0) { want = true; k.jumped = t; k.doubled = false; }
        if (under && grounded) { var away = new Vec3(-l.x, 0, -l.z); f.yaw = JsMath.Atan2(-away.x, -away.z); f.moveZ = 1; } // step toward the axis, out from under
        else if (!grounded && k.jumped >= 0 && !k.doubled && sim.mover.velocity.y <= 0.5) { want = true; k.doubled = true; }
        if (want)
        {
            if (sloppy) k.pend = (int)(rnd() * 28.8);
            else k.pend = 0;
        }
        bool press = false;
        if (k.pend >= 0) { if (k.pend == 0) press = true; k.pend--; }
        f.jump = new Skygrind.Core.Button { held = !grounded || press, pressed = press };
        return true;
    }

    /// <summary>Harness.N37.Wick.cs's Sim37W (today's stack at Stack37, wick37 on) with W's --rows forced on before StartRun.</summary>
    static SurvivalSim W47Stack(uint seed)
    {
        GameConfig cfg = Config();
        W48Tune(cfg); // NIGHT48 W (n48wick, W48LIP="over,arc,start,depth"): a harness-only sweep of the lips' numbers (nothing without the env)
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n37Override = !N37Blocks.ForceOff ? new N37Blocks { wick37 = true } : N37Blocks.Off;
        sim.wick37CentreOverride = Stack37;
        sim.climbTop48Force = W48Lips; // NIGHT48 W (n48wick, Harness.N48.Wick.cs): the climbTop48 lips (false: NIGHT47's stack)
        if (W47Rows.Length > 0)
        {
            var o = sim.rowsRegistry.Defaults();
            foreach (string r in W47Rows) o = o.Set(r, true);
            sim.rowsOverride = o;
        }
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        return sim;
    }

    sealed class W47Run
    {
        public double startY, maxY, pressY;
        public int wallJumps, doubleJumps, jumps, rideStarts, jumpOffs;
        public readonly List<string> ev = new List<string>();
        public double Gain => maxY - startY;
    }

    const double YawIntoWall = -Math.PI / 2; // forward (-sin yaw, -cos yaw) = (+1, 0, 0): into the test wall
    const double YawAlongWall = 0;           // forward (0, 0, -1): along the wall's face

    /// <summary>Put the feet at `at` (world), settle on the floor, then run `steps` steps of `frame(k, sim)`; the run's max feet and events.</summary>
    static W47Run W47Do(SurvivalSim sim, Vec3 at, Vec3 vel, bool settle, int steps, Func<int, SurvivalSim, InputFrame> frame, Func<int, SurvivalSim, bool> stop = null)
    {
        var r = new W47Run();
        sim.Respawn(at, 0);
        sim.mover.invulnerable = true;
        var si = (ScriptedInput)sim.input;
        if (settle)
            for (int i = 0; i < 30; i++) { si.SetLook(0, 0); si.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); }
        sim.mover.velocity = vel;
        Action<MoverEvent> h = e =>
        {
            if (e.type == MoverEventType.WallJump) r.wallJumps++;
            else if (e.type == MoverEventType.DoubleJump) r.doubleJumps++;
            else if (e.type == MoverEventType.Jump) r.jumps++;
            else if (e.type == MoverEventType.WallRideStart) r.rideStarts++;
            else return;
            r.ev.Add($"{e.type}@{sim.mover.position.y - W47T.y:0.00}");
        };
        sim.mover.Events += h;
        r.startY = r.maxY = sim.mover.position.y;
        for (int k = 0; k < steps; k++)
        {
            InputFrame f = frame(k, sim);
            si.SetLook(f.yaw, f.pitch);
            si.Set(f, new CombatInput { weapon = -1 });
            sim.Step();
            r.maxY = Math.Max(r.maxY, sim.mover.position.y);
            if (W47Trace && k % 6 == 0) Console.WriteLine($"      k {k} y {sim.mover.position.y:0.00} x {sim.mover.position.x - W47T.x:0.00} vy {sim.mover.velocity.y:0.00} {sim.mover.state} jump {(f.jump.held ? "H" : "-")}{(f.jump.pressed ? "P" : "")}");
            if (stop != null && stop(k, sim)) break;
        }
        sim.mover.Events -= h;
        return r;
    }

    static Skygrind.Core.Button B(bool held, bool pressed) => new Skygrind.Core.Button { held = held, pressed = pressed };

    static void W47Reach()
    {
        double capR = Config().movement.body.capsuleRadius;
        Vec3 open = W47T + new Vec3(-15, 0, 0);                    // clear of the wall
        Vec3 atWall = W47T + new Vec3(-capR - 0.05, 0, 0);          // standing at the wall, facing it
        var rows = new List<string>();
        void Row(string name, string from, W47Run r, string note = "") { rows.Add($"| {name} | {from} | {r.Gain:0.00} | {string.Join(" ", r.ev)} | {note} |"); }

        // 1 jump from rest: press, hold to the apex
        SurvivalSim sim = W47Sim(false);
        W47Run jr = W47Do(sim, open, default, true, 240, (k, s) => new InputFrame { yaw = YawIntoWall, jump = B(true, k == 0) }, (k, s) => k > 10 && s.mover.grounded);
        Row("jump (held)", "rest", jr);
        // 2 double jump from rest: the second press swept over steps 2..90
        double best = 0; int bestK = -1; W47Run atApex = null; int apexK = -1;
        for (int p = 2; p <= 90; p++)
        {
            sim = W47Sim(false);
            int pk = p;
            W47Run q = W47Do(sim, open, default, true, 300, (k, s) => new InputFrame { yaw = YawIntoWall, jump = B(k < pk - 1 || k >= pk, k == 0 || k == pk) }, (k, s) => k > pk + 4 && s.mover.grounded);
            if (q.doubleJumps > 0 && q.Gain > best) { best = q.Gain; bestK = p; }
        }
        {
            sim = W47Sim(false);
            bool pressed = false;
            int relK = -1;
            atApex = W47Do(sim, open, default, true, 300, (k, s) =>
            {
                // held to the apex (no release cut), let go one step at the apex, press on the next
                if (relK < 0 && k > 2 && s.mover.velocity.y <= 0) relK = k;
                bool pr = !pressed && relK >= 0 && k == relK + 1;
                if (pr) { pressed = true; apexK = k; }
                return new InputFrame { yaw = YawIntoWall, jump = B(k != relK, k == 0 || pr) };
            }, (k, s) => pressed && k > apexK + 4 && s.mover.grounded);
        }
        rows.Add($"| double jump (best press, step {bestK} = {bestK / 120.0:0.000} s) | rest | {best:0.00} | | swept 2..90 |");
        Row($"double jump (press at the apex, step {apexK})", "rest", atApex);

        // 3 wall jump from rest: stand at the wall facing it, stick into it, jump, press again (swept); best with a WallJump event
        double bw = 0; int bwK = -1; string bwEv = "";
        int wjMade = 0, wjTried = 0;
        for (int p = 2; p <= 90; p++)
        {
            sim = W47Sim(false);
            int pk = p;
            W47Run q = W47Do(sim, atWall, default, true, 300, (k, s) => new InputFrame { yaw = YawIntoWall, moveZ = 1, jump = B(k < pk - 1 || k >= pk, k == 0 || k == pk) }, (k, s) => k > pk + 4 && s.mover.grounded);
            wjTried++;
            if (q.wallJumps > 0) { wjMade++; if (q.Gain > bw) { bw = q.Gain; bwK = p; bwEv = string.Join(" ", q.ev); } }
        }
        rows.Add($"| wall jump (best second press, step {bwK}) | rest, at the wall, stick into it | {bw:0.00} | {bwEv} | a WallJump on {wjMade} / {wjTried} press steps |");
        // 3b wall jump then a double jump at its apex
        {
            sim = W47Sim(false);
            int stage = 0, at2 = -1, relK = -1;
            W47Run q = W47Do(sim, atWall, default, true, 360, (k, s) =>
            {
                // held to each apex, let go one step there, press on the next
                bool pr = false;
                if (relK < 0 && stage < 2 && k > at2 + 3 && s.mover.velocity.y <= 0) relK = k;
                else if (relK >= 0 && k == relK + 1) { pr = true; stage++; at2 = k; relK = -1; }
                return new InputFrame { yaw = YawIntoWall, moveZ = 1, jump = B(k != relK, k == 0 || pr) };
            }, (k, s) => stage == 2 && k > at2 + 4 && s.mover.grounded);
            Row("jump, wall jump at its apex, then a double jump at that apex", "rest, at the wall", q);
        }

        // 4 ride jump-offs: run along the wall at `speed` with jump held (the ride), ride 1 s, release with the look at `pitch`; optional double jump
        foreach (double speed in new[] { 10.4, 20.0 })
            foreach (var (pitch, rideLook) in new[] { (0.0, 0.0), (45.0, 0.0), (45.0, 45.0) })
                foreach (bool dj in new[] { false, true })
                {
                    sim = W47Sim(false);
                    int rideSince = -1, rel = -1; bool djDone = false; double relY = 0;
                    // ride40's intent: the feet >= minClearance (1 m) over the ground, >= minSpeed, the stick toward the wall (inputDot 0.1): start 2 m up,
                    // the stick 20 deg into the wall (moveX +0.34: facing -z, +x is the stick's right = into the wall)
                    W47Run q = W47Do(sim, W47T + new Vec3(-capR - 0.05, 2.0, 15), new Vec3(0.5, 0, -speed), false, 600, (k, s) =>
                    {
                        if (rideSince < 0 && s.mover.state == MoveState.WallRide) rideSince = k;
                        bool releaseNow = rel < 0 && rideSince >= 0 && k - rideSince >= 120;
                        if (releaseNow) { rel = k; relY = s.mover.position.y; }
                        bool pr = false;
                        if (dj && rel >= 0 && !djDone && k > rel + 4 && s.mover.velocity.y <= 0) { pr = true; djDone = true; }
                        double py = rel >= 0 ? pitch : rideLook; // rideLook 45: the look up the whole ride (ride38 / ride40's drift up)
                        return new InputFrame { yaw = YawAlongWall, pitch = py * Math.PI / 180, moveZ = rel < 0 ? 0.94 : 1, moveX = rel < 0 ? 0.34 : 0, jump = B(rel < 0 || pr, pr) };
                    }, (k, s) => (rel < 0 && k > 300) || (rel >= 0 && k > rel + 10 && (s.mover.grounded || s.mover.position.y < relY - 6)));
                    // the gain over the feet at the release
                    double gainOverRelease = q.maxY - (rel >= 0 ? relY : q.startY);
                    rows.Add($"| ride jump-off, pitch {pitch:0} deg at the release, {rideLook:0} deg while riding{(dj ? ", + double jump at the apex" : "")} | a ride at {speed:0.0} m/s (1 s held, attached at 2.00 m) | {gainOverRelease:0.00} over the release; {q.maxY - W47T.y - 2.0:0.00} over the attach | {string.Join(" ", q.ev)} | ride {(rideSince >= 0 ? "attached" : "NEVER attached")}; feet at the release {relY - W47T.y:0.00} m |");
                }
        // 4b a ride "from rest": stand at the wall, jump with jump held and the stick along it (no run-up)
        {
            sim = W47Sim(false);
            W47Run q = W47Do(sim, atWall + new Vec3(0, 0, 10), default, true, 360, (k, s) => new InputFrame { yaw = YawAlongWall, moveZ = 0.94, moveX = 0.34, jump = B(k < 200, k == 0) }, (k, s) => k > 20 && s.mover.grounded);
            Row("jump held along the wall from standing (does a ride attach?)", "rest", q, $"rides {q.rideStarts}");
        }

        Console.WriteLine("| move | from | max height gain (m) | events (type@feet y) | note |");
        Console.WriteLine("|---|---|---|---|---|");
        foreach (string s in rows) Console.WriteLine(s);
    }

    /// <summary>THE STACK chain: ride panel `first` of today's stack (wick37's, at Stack37), release at fraction `rel` of each panel with the look
    /// `relPitch` up, look `ridePitch` while riding, steer at the next panel's 40 % point, hold jump again 3 steps after the release (the ride on the
    /// next face). Per gap: the feet at the release and at the next attach, each over its panel's bottom; the chain ends at a fall (grounded or 4 m
    /// under the next bottom) or after `gaps` gaps.</summary>
    static void W47StackChain(int first, int gaps, double rel, double ridePitch, double relPitch) => W47StackChainRun(first, gaps, rel, ridePitch, relPitch, false);

    /// <summary>The stack chain; quiet: no printing; returns the gaps made.</summary>
    static int W47StackChainRun(int first, int gaps, double rel, double ridePitch, double relPitch, bool quiet)
    {
        SurvivalSim sim = W47Stack(1);
        Wick37Stack s = sim.wick37Stack;
        double capR = sim.cfg.movement.body.capsuleRadius;
        Wick37Panel p = s.panels[first];
        Vec3 tu0 = (p.end - p.start) * (1 / (p.end - p.start).Length);
        sim.Respawn(Stack37 + p.start + tu0 * 0.6 + p.normal * (capR + 0.02) + new Vec3(0, 0.5, 0), 0);
        sim.mover.invulnerable = true;
        sim.mover.velocity = tu0 * 10.4 + p.normal * -1.0;
        var si = (ScriptedInput)sim.input;
        int cur = first, made = 0, since = -1;
        bool riding = false, released = false;
        double relRel = 0;
        var lines = new List<string>();
        if (!quiet) Console.WriteLine($"n47wick stackchain: from panel {first}, release at {rel * 100:0} % of each panel, look {ridePitch:0} deg while riding and {relPitch:0} deg at the release (today's stack: rise {s.p.rise} m a panel)");
        for (int k = 0; k < 120 * 30 && made < gaps; k++)
        {
            Wick37Panel a = s.panels[cur], q = s.panels[Math.Min(cur + 1, s.panels.Count - 1)];
            Vec3 tg = a.end - a.start;
            double len = tg.Length;
            Vec3 tu = tg * (1 / len);
            Vec3 l = sim.mover.position - Stack37;
            double frac = ((l.x - a.start.x) * tu.x + (l.z - a.start.z) * tu.z) / len;
            var f = new InputFrame { yaw = JsMath.Atan2(-tu.x, -tu.z), pitch = ridePitch * Math.PI / 180, moveZ = 1 };
            if (!released && sim.mover.state == MoveState.WallRide) riding = true;
            if (riding && !released && (frac >= rel || sim.mover.state != MoveState.WallRide))
            {
                released = true; since = 0; relRel = l.y - a.bottom;
            }
            if (released)
            {
                since++;
                Vec3 aim = q.start + (q.end - q.start) * 0.4;
                f.yaw = JsMath.Atan2(-(aim.x - l.x), -(aim.z - l.z));
                f.pitch = relPitch * Math.PI / 180;
                f.jump = B(since > 3, false);
            }
            else f.jump = B(true, k == 0);
            si.SetLook(f.yaw, f.pitch);
            si.Set(f, new CombatInput { weapon = -1 });
            sim.Step();
            l = sim.mover.position - Stack37;
            if (released && sim.mover.state == MoveState.WallRide && Wick37Pilot.RiddenPanel(s, l, capR) == q.index)
            {
                lines.Add($"| {cur} -> {q.index} | {relRel:+0.00;-0.00} | {l.y - q.bottom:+0.00;-0.00} |");
                made++; cur = q.index; released = false; riding = true;
                if (q.section != a.section || cur + 1 >= s.panels.Count) break;
                continue;
            }
            if (released && (sim.mover.grounded || l.y < q.bottom - 4))
            {
                lines.Add($"| {cur} -> {q.index} | {relRel:+0.00;-0.00} | FELL (feet {l.y - q.bottom:+0.00;-0.00} m, {(sim.mover.grounded ? "landed" : "under")}) |");
                break;
            }
        }
        if (!quiet) Console.WriteLine("| gap | feet at the release over its panel's bottom (m) | feet at the next attach over that panel's bottom (m) |");
        if (!quiet) Console.WriteLine("|---|---|---|");
        if (!quiet) foreach (string ln in lines) Console.WriteLine(ln);
        if (!quiet) Console.WriteLine($"  gaps made {made} of {gaps}");
        return made;
    }

    /// <summary>THE SLOPPY CLIMB PILOT (G1's meter; W-cards.md section 5): Wick37Pilot's climb (no boss, no shooting, no parry; its casual hand on:
    /// a release anywhere 35-85 % of a panel, the look up 28-50 deg) with a human's errors laid over its frames, drawn from a Mulberry32 per seed:
    /// per ride a look error U(-12, +12) deg in yaw and in pitch; the look LEVEL while riding in half of the rides (a hand that does not know
    /// ride38's drift; the pilot itself looks up hard on a low ride); one release in four late by 150 ms (18 steps); every press in the air
    /// delayed U(0, 240) ms (the +-120 ms band shifted late: the pilot cannot press before its own decision). `sloppy` false: the pilot's own
    /// hand (the control). Per seed: the cap or not in `cap` s, the climb time, per gap the first try (the ride after leaving panel i is i + 1,
    /// not a re-ride of a panel at or under i), the retries and the deepest fall (in panels).</summary>
    static void W47Sloppy(int seeds, bool sloppy, double capS)
    {
        var first = new Dictionary<int, (int ok, int n)>();
        var firstSeed = new Dictionary<int, (int ok, int n)>();
        var stepFirst = new Dictionary<int, (int ok, int n)>();
        var times = new List<double>();
        int made = 0, retries = 0, worstDrop = 0, bigDrops = 0;
        int panels = 0;
        for (uint sd = 1; sd <= seeds; sd++)
        {
            SurvivalSim sim = W47Stack(sd);
            Wick37Stack s = sim.wick37Stack;
            panels = s.panels.Count;
            sim.Respawn(Mouth37(sim), 0);
            sim.mover.invulnerable = true;
            var si = (ScriptedInput)sim.input;
            var hand = new Mulberry32(sd * 7919u + 3u);
            var jit = new Mulberry32(sd * 104729u + 47u);
            var seen = new HashSet<int>();
            var stepState = new W47StepState();
            var pilot = new Wick37Pilot { mode = Wick37Pilot.Mode.Climb, shoot = false, parry = false, hand = hand };
            if (sloppy) { pilot.casual = true; pilot.aimRate = 10; }
            double yaw = 0, dt = sim.cfg.Dt, errYaw = 0, errPitch = 0, capT = -1;
            bool level = false, late = false, wasRiding = false, wasHeld = false;
            int lateLeft = 0, pending = -1, lastPanel = -1, leftPanel = -1, curPanel = -1, letGo = 0;
            string relInfo = "";
            bool judged = true;
            int steps = (int)(capS / dt);
            for (int i = 0; i < steps; i++)
            {
                pilot.Step(sim, yaw, i);
                yaw = pilot.Move.yaw;
                InputFrame f = pilot.Move;
                bool stepping = W47StepFrame(sim, i, stepState, () => jit.Next(), sloppy, ref f, stepFirst);
                bool riding = sim.mover.state == MoveState.WallRide;
                Vec3 l = sim.mover.position - Stack37;
                if (riding && !wasRiding)
                {
                    int j = Wick37Pilot.RiddenPanel(s, l, sim.cfg.movement.body.capsuleRadius);
                    if (j >= 0)
                    {
                        if (!judged && leftPanel >= 0)
                        {
                            bool ok = j == leftPanel + 1;
                            if (!ok && W47Trace && leftPanel >= 4) Console.WriteLine($"      RIDE MISS {leftPanel} -> rode {j} next; {relInfo}");
                            if (!ok && W48Ring != null && leftPanel >= 4) { Console.WriteLine("        steps from the release: " + string.Join(" | ", W48Ring)); } // NIGHT48 W: n48wick sloppy --steps
                            W48Ring?.Clear();
                            var e = first.TryGetValue(leftPanel, out var v) ? v : (0, 0);
                            first[leftPanel] = (e.Item1 + (ok ? 1 : 0), e.Item2 + 1);
                            if (seen.Add(leftPanel)) { var f1 = firstSeed.TryGetValue(leftPanel, out var w1) ? w1 : (0, 0); firstSeed[leftPanel] = (f1.Item1 + (ok ? 1 : 0), f1.Item2 + 1); } // the seed's FIRST try of this gap (G1)
                            judged = true;
                        }
                        if (lastPanel >= 0 && j <= lastPanel) { retries++; int drop = lastPanel - j + 1; worstDrop = Math.Max(worstDrop, drop); if (drop > 3) bigDrops++; }
                        lastPanel = Math.Max(lastPanel, j);
                        curPanel = j;
                    }
                    errYaw = (jit.Next() * 24 - 12) * Math.PI / 180;
                    errPitch = (jit.Next() * 24 - 12) * Math.PI / 180;
                    level = jit.Next() < 0.5;
                    late = jit.Next() < 0.25;
                }
                if (!riding && wasRiding && curPanel >= 0 && judged) { leftPanel = curPanel; judged = false; var lp = s.panels[curPanel]; Vec3 tg2 = lp.end - lp.start; double fr = ((l.x - lp.start.x) * tg2.x + (l.z - lp.start.z) * tg2.z) / Vec3.Dot(tg2, tg2); relInfo = $"left {curPanel} at {fr * 100:0} % feet {l.y - lp.bottom:+0.00;-0.00} v ({sim.mover.velocity.x:0.0},{sim.mover.velocity.y:0.0},{sim.mover.velocity.z:0.0}) level {level} late {late} jump {(f.jump.held ? "H" : "-")} pitch {f.pitch * 57.3:0}"; } // left panel curPanel: the next ride judges the gap
                if (sloppy && !stepping)
                {
                    string abl = Environment.GetEnvironmentVariable("W47ABL") ?? "yaw,pitch,level,late,press"; // the ablation: only the named errors
                    if (riding) { if (abl.Contains("yaw")) f.yaw += errYaw; f.pitch = (level && abl.Contains("level") ? 0 : f.pitch) + (abl.Contains("pitch") ? errPitch : 0); }
                    if (riding && wasHeld && !f.jump.held && late && lateLeft == 0 && abl.Contains("late")) lateLeft = 18;
                    if (lateLeft > 0) { lateLeft--; f.jump = B(true, false); if (lateLeft == 0) { late = false; if (W48LateLetGo) letGo = 4; } }
                    else if (letGo > 0) { letGo--; f.jump = B(false, false); } // NIGHT48 W (n48wick, W48LateLetGo): the late hand still lets go, 150 ms late, for the pilot's own 4-step release (NIGHT47's model held through it: no release at all)
                    if (f.jump.pressed && !riding && pending < 0 && abl.Contains("press")) { pending = (int)(jit.Next() * 28.8); f.jump = B(f.jump.held, false); }
                    if (pending >= 0) { if (pending == 0) f.jump = B(true, true); pending--; }
                }
                wasHeld = f.jump.held;
                wasRiding = riding;
                si.SetLook(f.yaw, f.pitch);
                si.Set(f, pilot.Combat);
                sim.Step();
                l = sim.mover.position - Stack37;
                if (W48Ring != null && riding && W48Ring.Count > 30) W48Ring.RemoveAt(0); // NIGHT48 W: while riding, the last 30 steps; after the leave, up to 50 more, until grounded
                if (W48Ring != null && curPanel >= 4 && (riding || !judged && !sim.mover.grounded && W48Ring.Count < 80))
                {
                    var cp = s.panels[curPanel];
                    Vec3 tg3 = cp.end - cp.start;
                    double fr3 = ((l.x - cp.start.x) * tg3.x + (l.z - cp.start.z) * tg3.z) / Vec3.Dot(tg3, tg3);
                    W48Ring.Add($"{sim.mover.state.ToString()[0]}{(f.jump.held ? "H" : "-")} a{fr3 * 100:0} r{JsMath.Hypot(l.x, l.z):0.0} y{l.y - cp.bottom:+0.0;-0.0} vy{sim.mover.velocity.y:0.0} L{sim.mover.flow34.ride35.lateLeft}");
                }
                if (W47Trace && sd == 1 && i % 60 == 0) Console.WriteLine($"      t {i * dt:0.0} feet ({l.x:0.0}, {l.y:0.00}, {l.z:0.0}) r {JsMath.Hypot(l.x, l.z):0.0} {sim.mover.state} jump {(f.jump.held ? "H" : "-")} v ({sim.mover.velocity.x:0.0},{sim.mover.velocity.y:0.0},{sim.mover.velocity.z:0.0}) panel {Wick37Pilot.RiddenPanel(s, l, 0.35)}");
                if (sim.mover.grounded && l.y > s.p.capY - 0.3) { capT = i * dt; break; }
            }
            if (capT > 0) { made++; times.Add(capT); }
            W48LipLandsTotal += sim.climb48LipLands; // NIGHT48 W: landings on a climbTop48 lip (0 without them)
            if (W47Trace) Console.WriteLine($"  seed {sd}: {(capT > 0 ? "the cap in " + capT.ToString("0.0") + " s" : "no cap")}, top panel {lastPanel}");
        }
        times.Sort();
        W48Last = (firstSeed, stepFirst, made, new List<double>(times)); W48Panels = panels; // NIGHT48 W: n48wick's G1 verdict reads the run
        string Q(double q) => times.Count == 0 ? "-" : times[Math.Min(times.Count - 1, (int)(q * times.Count))].ToString("0.0");
        Console.WriteLine($"n47wick sloppy ({(sloppy ? "SLOPPY: +-12 deg look per ride, level look on half the rides, 1 in 4 releases 150 ms late, air presses 0-240 ms late" : "CONTROL: Wick37Pilot's own hand")}), {seeds} seeds, cap {capS:0} s: the cap {made} / {seeds}; climb time median {Q(0.5)} s, p90 {Q(0.9)} s; retries {retries}; the deepest fall {worstDrop} panels ({bigDrops} falls of more than 3)");
        var sb = new System.Text.StringBuilder("  every try per gap (ok / tries):");
        for (int g = 0; g + 1 < panels; g++) sb.Append(first.TryGetValue(g, out var v) ? $" {g}->{g + 1} {v.ok}/{v.n}" : $" {g}->{g + 1} -");
        Console.WriteLine(sb.ToString());
        if (stepFirst.Count > 0)
        {
            var sbs = new System.Text.StringBuilder("  FIRST try per STEP (ok / seeds that reached it):");
            foreach (var kv in stepFirst.OrderBy(q => q.Key)) sbs.Append($" step {kv.Key} {kv.Value.ok}/{kv.Value.n}");
            Console.WriteLine(sbs.ToString());
        }
        sb = new System.Text.StringBuilder("  FIRST try per gap (ok / seeds that reached it; G1: bottom third >= 95 %, top third >= 80 %):");
        for (int g = 0; g + 1 < panels; g++) sb.Append(firstSeed.TryGetValue(g, out var v) ? $" {g}->{g + 1} {v.ok}/{v.n}" : $" {g}->{g + 1} -");
        Console.WriteLine(sb.ToString());
    }

    /// <summary>Five presses on one wall (or a chimney of two walls 4 m apart): each press at the first falling step after the last.</summary>
    static void W47Chain(bool chimney)
    {
        double capR = Config().movement.body.capsuleRadius;
        SurvivalSim sim = W47Sim(chimney);
        Vec3 at = W47T + new Vec3(-capR - 0.05, 0, 0);
        int presses = 0, last = -1, wjSeen = 0, wjTurned = 0;
        bool facingA = true;
        Action<MoverEvent> turn = e => { if (e.type == MoverEventType.WallJump) wjSeen++; };
        sim.mover.Events += turn;
        var pressY = new List<double>();
        var peaks = new List<double>();
        var kinds = new List<string>();
        double peak = double.NegativeInfinity;
        int wjBefore = 0, djBefore = 0;
        W47Run r = null;
        r = W47Do(sim, at, default, true, 1200, (k, s) =>
        {
            bool pr = false;
            if (k == 0) { pr = true; presses = 1; last = 0; pressY.Add(s.mover.position.y - W47T.y); }
            else if (presses < 6 && k > last + 3 && s.mover.velocity.y <= 0 && !s.mover.grounded)
            {
                peaks.Add(peak - W47T.y);
                peak = double.NegativeInfinity;
                pr = true; presses++; last = k;
                pressY.Add(s.mover.position.y - W47T.y);
            }
            if (chimney && wjSeen > wjTurned) { wjTurned = wjSeen; facingA = !facingA; } // after each WALL jump, face the other wall
            peak = Math.Max(peak, s.mover.position.y);
            double yaw = chimney ? (facingA ? YawIntoWall : Math.PI / 2) : YawIntoWall;
            return new InputFrame { yaw = yaw, moveZ = 1, jump = B(true, pr) };
        }, (k, s) => presses >= 6 && k > last + 3 && (s.mover.velocity.y <= 0 || s.mover.grounded));
        sim.mover.Events -= turn;
        peaks.Add(peak - W47T.y);
        Console.WriteLine($"n47wick chain ({(chimney ? "CHIMNEY: two walls 4 m apart, the look turned after each press" : "ONE WALL, the stick into it")}): events {string.Join(" ", r.ev)}");
        Console.WriteLine("| press | feet at the press (m) | peak after it (m) | gain over the last peak (m) |");
        Console.WriteLine("|---|---|---|---|");
        for (int i = 0; i < pressY.Count && i < peaks.Count; i++)
            Console.WriteLine($"| {i + 1} | {pressY[i]:0.00} | {peaks[i]:0.00} | {(i == 0 ? peaks[0] : peaks[i] - peaks[i - 1]):+0.00;-0.00} |");
    }
}
