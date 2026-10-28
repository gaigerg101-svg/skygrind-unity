// NIGHT38 builder C2 (spire38): level 5 rebuilt for flight, headless. One entry, N38Spire (declared and called in Program.cs).
//   spire38 [gates|meter|all|walk] [--trace] (walk: walk29's pilot through n38-spire with spire38 on; needs the lead's hook line in Walk29Pilot, see Walk38Spire.cs; --seeds N --cap S)
//     gates  every gate of Design/levels/n38/spire.plan.json (gates38.list) flown IN THE LEVEL ITSELF (n38-spire.json, the real SurvivalSim, the
//            spire38 block on, so the pad's meter is the bar the tries start with): the JUMP-DASH (dash, then jump inside the dash: the Mover's
//            JumpKind.Dash) over a grid of dash points and jump delays, and every line WITHOUT it that spends the bar (jump + dash, + a second dash,
//            + the double jump, slide-hop + double + dash) over a grid of timings. PASS (D-6): a jump-dash lands on the far floor with a margin
//            (metres past the near edge / the gap) >= 10 %, and no other try lands there. Builder E's checker (`n38 gates`) runs the other verbs
//            (rides, wall jumps, boosts) but has no dash-then-jump try (its jump-dash is a jump, then a dash): this is that try.
//            The same lines with the meter OFF (the block's meter forced off: a full 1.0 bar) are printed as the control.
//     meter  the meter's rules on the real sim: the pad sets the bar to one jump-dash; after a jump-dash the bar does not refill in the flight room
//            (2 s standing on the landing); the next room's pad fills it again; the grapple is off in a flight room and back outside it; the
//            control (the block off): the bar refills.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N38Spire(string cmd, string[] args, ref int code)
    {
        if (cmd != "spire38") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        Trace38C = Array.IndexOf(args, "--trace") >= 0;
        int fails = 0;
        if (what == "meter" || what == "all") fails += Meter38C();
        if (what == "gates" || what == "all") fails += Gates38C();
        if (what == "walk")
        {
            // the walk pilot (Walk29Pilot + Walk38Spire.cs) through n38-spire with the spire38 block on (the gate meter live): walk29's own report per room
            string old = walkPrefix;
            Action<SurvivalSim> oldHook = walkAfterStart;
            walkPrefix = "n38";
            walkAfterStart = s => s.n38.spire38 = true;
            if (Trace38C) Walk29Pilot.SpireTrace38 = Console.WriteLine;
            var wargs = new List<string> { "walk29", "spire" };
            for (int i = 2; i < args.Length; i++) wargs.Add(args[i]);
            int wc = 0;
            try { N29Walk("walk29", wargs.ToArray(), ref wc); }
            finally { walkPrefix = old; walkAfterStart = oldHook; }
            fails += wc == 0 ? 0 : 1;
        }
        Console.WriteLine(fails == 0 ? "spire38: GREEN" : $"spire38: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static bool Trace38C;
    const string Spire38Path = "Design/levels/n38-spire.json", Spire38Plan = "Design/levels/n38/spire.plan.json";

    static SurvivalSim Sim38C(bool meterOn)
    {
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Spire38Path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        N38Blocks b = N38Blocks.From(cfg);
        b.spire38 = meterOn;
        sim.n38Override = b;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(3805, null, null);
        sim.husks.frozen = true;
        sim.map.pads.Clear();
        return sim;
    }

    static string F38(double v) => v.ToString("0.00", CultureInfo.InvariantCulture);

    sealed class Gate38C
    {
        public string id;
        public Vec3 from, to;
        public double gap, runUp;
    }

    static List<Gate38C> Gates38List()
    {
        var plan = PortData.Obj(PortData.LoadJson(Spire38Plan));
        var list = new List<Gate38C>();
        foreach (object o in PortData.Arr(PortData.Obj(plan["gates38"])["list"]))
        {
            var g = PortData.Obj(o);
            list.Add(new Gate38C { id = (string)g["id"], from = PortData.V3(g["from"]), to = PortData.V3(g["to"]), gap = PortData.Num(g["gap"]), runUp = g.ContainsKey("runUp") ? PortData.Num(g["runUp"]) : 8 });
        }
        return list;
    }

    /// <summary>The bar's value in the Mover's units (the stamina bar with its rule on, else the pips).</summary>
    static double Bar38(SurvivalSim sim) => sim.mover.stamina.rules.Any ? sim.mover.stamina.bar : sim.mover.pips;

    /// <summary>One try: respawn runUp m behind the lip (on the pad), sprint north, and at `trigger` m before the lip start the action script
    /// (k = steps since the trigger -> the frame's extra buttons). Returns metres past the landing's near edge where the feet came down on the
    /// far floor (negative / NaN: not there), the bar at the trigger, whether a jump-dash happened.</summary>
    static (double over, double bar0, bool dashJumped, double landY) Try38(SurvivalSim sim, Gate38C g, double trigger, Func<int, (bool jump, bool dash, bool slide)> act)
    {
        var si = (ScriptedInput)sim.input;
        double lipZ = g.from.z, nearZ = g.from.z - g.gap;
        sim.Respawn(new Vec3(g.from.x, g.from.y, g.from.z + g.runUp), 0);
        bool dj = false;
        Action<MoverEvent> on = e => { if (e.type == MoverEventType.DashJump) dj = true; };
        sim.mover.Events += on;
        int s = 0;
        for (; s < 600 && sim.mover.position.z > lipZ + trigger; s++)
        {
            si.Set(new InputFrame { moveZ = 1, yaw = 0, sprint = new Button { held = true } }, new CombatInput { weapon = -1 });
            sim.Step();
        }
        double bar0 = Bar38(sim);
        bool prevJ = false, prevD = false, prevS = false;
        double over = double.NaN, landY = double.NaN;
        for (int k = 0; k < 480; k++)
        {
            var (j, d, sl) = act(k);
            var f = new InputFrame { moveZ = 1, yaw = 0, sprint = new Button { held = true } };
            f.jump = new Button { held = j, pressed = j && !prevJ };
            f.dash = new Button { held = d, pressed = d && !prevD };
            f.slide = new Button { held = sl, pressed = sl && !prevS };
            prevJ = j; prevD = d; prevS = sl;
            si.Set(f, new CombatInput { weapon = -1 });
            sim.Step();
            Vec3 m = sim.mover.position;
            if (k > 8 && sim.mover.grounded)
            {
                landY = m.y;
                if (m.y > -0.5 && m.z < lipZ - 0.5) over = nearZ - m.z;
                break;
            }
        }
        sim.mover.Events -= on;
        if (Trace38C) Console.WriteLine($"      trigger {F38(trigger)} bar0 {F38(bar0)} dj {dj} over {F38(over)} landY {F38(landY)}");
        return (over, bar0, dj, landY);
    }

    /// <summary>A press of `steps` steps starting at step `at` (a held button).</summary>
    static bool Hold38(int k, int at, int steps) => at >= 0 && k >= at && k < at + steps;

    static int Gates38C()
    {
        int fails = 0;
        double dt = Config().Dt;
        int S(double sec) => (int)Math.Round(sec / dt);
        foreach (Gate38C g in Gates38List())
        {
            // the jump-dash: the dash pressed `lead` m before the lip, the jump `j` steps into the dash (the dash lasts dash.duration 0.25 s)
            double best = double.NegativeInfinity;
            string bestAt = "";
            SurvivalSim sim = Sim38C(true);
            foreach (double lead in new[] { 0.2, 0.6, 1.0, 1.6, 2.4, 3.4 })
                foreach (int j in new[] { 1, 3, 6, 10, 15, 20, 25 })
                {
                    var r = Try38(sim, g, lead, k => (Hold38(k, j, 45), Hold38(k, 0, 2), false));
                    if (r.dashJumped && !double.IsNaN(r.over) && r.over > best) { best = r.over; bestAt = $"dash {F38(lead)} m before the lip, jump {j} steps in (bar at the dash {F38(r.bar0)})"; }
                }
            double margin = best / g.gap;
            // every line without the dash-jump that spends the bar: jump + dash (+ dash) (+ double), slide-hop + double + dash; meter on, then off
            (double best, string at) Others(bool meterOn)
            {
                SurvivalSim o = Sim38C(meterOn);
                double ob = double.NegativeInfinity;
                string oat = "";
                void Keep((double over, double bar0, bool dj, double landY) r, string name)
                {
                    if (r.dj) return; // a dash-jump is the gate's verb
                    double v = double.IsNaN(r.over) ? double.NegativeInfinity : r.over;
                    if (v > ob) { ob = v; oat = name; }
                }
                foreach (double d1 in new[] { 0.05, 0.15, 0.3, 0.45 })
                    foreach (double d2 in new[] { -1, 0.3, 0.45, 0.6 })
                        foreach (double dbl in new[] { -1, 0.3, 0.55, 0.8 })
                        {
                            int a = S(d1), b = d2 < 0 ? -1 : S(d1 + d2), c = dbl < 0 ? -1 : S(dbl);
                            if (c >= 0 && (c == a || c == b)) continue;
                            var r = Try38(o, g, 0.3, k => (Hold38(k, 0, 30) || Hold38(k, c, 30), Hold38(k, a, 2) || Hold38(k, b, 2), false));
                            Keep(r, $"jump, dash at {F38(d1)} s{(b >= 0 ? $", dash +{F38(d2)} s" : "")}{(c >= 0 ? $", double at {F38(dbl)} s" : "")}");
                        }
                foreach (double dbl in new[] { 0.3, 0.55 })
                    foreach (double d1 in new[] { 0.15, 0.4, 0.7 })
                    {
                        int c = S(dbl), a = S(d1);
                        var r = Try38(o, g, 1.5, k => (Hold38(k, 6, 30) || Hold38(k, 6 + c, 30), Hold38(k, 6 + a, 2), k < 8));
                        Keep(r, $"slide-hop, double at {F38(dbl)} s, dash at {F38(d1)} s");
                    }
                return (ob, oat);
            }
            var on = Others(true);
            var off = Others(false);
            bool ok = margin >= 0.10 && on.best < 0;
            if (!ok) fails++;
            Console.WriteLine($"  gate {g.id}: gap {F38(g.gap)} m: jump-dash {(double.IsNegativeInfinity(best) ? "does NOT cross" : $"lands {F38(best)} m past the near edge = margin {F38(100 * margin)} %")} ({bestAt}); "
                + $"best other (meter on) {(double.IsNegativeInfinity(on.best) ? "never lands on the far floor" : $"{F38(on.best)} m ({on.at})")}; control, meter off: {(double.IsNegativeInfinity(off.best) ? "never lands" : $"{F38(off.best)} m ({off.at})")}: {Pass36(ok)}");
        }
        return fails;
    }

    static int Meter38C()
    {
        int fails = 0;
        var gates = Gates38List();
        Gate38C g = gates[0];
        SurvivalSim sim = Sim38C(true);
        var si = (ScriptedInput)sim.input;
        int metered = 0;
        sim.events.On<Spire38Event>("spire38Meter", e => metered++);
        void Idle(int n, Vec3? hold = null)
        {
            for (int i = 0; i < n; i++) { si.Set(new InputFrame { yaw = 0 }, new CombatInput { weapon = -1 }); sim.Step(); }
        }
        // 1. on the pad: the bar is set to one jump-dash's cost
        sim.mover.ropeEnabled = true; // as if the grapple were owned (levels 4+)
        sim.Respawn(new Vec3(g.from.x, 0, g.from.z + 4), 0);
        Idle(30);
        double unit = (sim.mover.stamina.rules.Any ? sim.mover.stamina.rules.cost + sim.mover.stamina.rules.dashJumpExtra : 2) + Config().Num("design.n38.spire38.meter.slack");
        double onPad = Bar38(sim);
        bool a = Math.Abs(onPad - unit) < 1e-9 && sim.Spire38Live;
        Console.WriteLine($"meter: on BELFRY I's pad the bar is {F38(onPad)} (one jump-dash + the slack = {F38(unit)}), spire38Meter events {metered}: {Pass36(a)}");
        bool hookOff = !sim.mover.ropeEnabled;
        Console.WriteLine($"meter: the grapple in the flight room: ropeEnabled {sim.mover.ropeEnabled}: {Pass36(hookOff)}");
        // 2. the jump-dash over the gap, then 2 s on the landing: no refill
        var r = Try38(sim, g, 1.0, k => (Hold38(k, 3, 45), Hold38(k, 0, 2), false));
        double after = Bar38(sim);
        Idle(240);
        double later = Bar38(sim);
        bool b = r.dashJumped && !double.IsNaN(r.over) && later <= after + 1e-9;
        Console.WriteLine($"meter: a jump-dash crossed ({F38(r.over)} m past the near edge); the bar after it {F38(after)}, after 2 s on the landing {F38(later)} (no refill): {Pass36(b)}");
        // 3. the next gate room's pad fills it again; outside a flight room the grapple comes back
        Gate38C g2 = gates[1];
        sim.Respawn(new Vec3(g2.from.x, 0, g2.from.z + 4), 0);
        Idle(30);
        double next = Bar38(sim);
        bool c = Math.Abs(next - unit) < 1e-9;
        Console.WriteLine($"meter: on BELFRY II's pad the bar is {F38(next)}: {Pass36(c)}");
        sim.Respawn(new Vec3(g.from.x, 0, g.from.z + g.runUp + 20), 0); // THE NAVE (v2), south of BELFRY I: no flight room
        Idle(30);
        bool d = sim.mover.ropeEnabled;
        Console.WriteLine($"meter: outside a flight room (THE NAVE) the grapple is back: ropeEnabled {sim.mover.ropeEnabled}: {Pass36(d)}");
        // 4. the control: the block off, the same jump-dash and 2 s: the bar refills
        SurvivalSim off = Sim38C(false);
        var r2 = Try38(off, g, 1.0, k => (Hold38(k, 3, 45), Hold38(k, 0, 2), false));
        double offAfter = Bar38(off);
        for (int i = 0; i < 240; i++) { ((ScriptedInput)off.input).Set(new InputFrame { yaw = 0 }, new CombatInput { weapon = -1 }); off.Step(); }
        double offLater = Bar38(off);
        bool e2 = !off.Spire38Live && offLater > offAfter;
        Console.WriteLine($"meter: control (spire38 off): bar at the takeoff {F38(r2.bar0)}, after 2 s {F38(offLater)} (refills), meter live {off.Spire38Live}: {Pass36(e2)}");
        foreach (bool ok in new[] { a, hookOff, b, c, d, e2 }) if (!ok) fails++;
        return fails;
    }
}
