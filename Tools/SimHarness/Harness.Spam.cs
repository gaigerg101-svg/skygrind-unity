// NIGHT16 U3-29.4 (builder M): `spam [map.json] [--off]`, the movement spam table. Scripted abuse per verb, driven
// through Core's Mover at 120 Hz on flat ground, each row against walking (x walk) and beside the skilled line:
//   - "plane": a 4 km flat floor (TriangleWorld): the full 10 s window, the rows the targets gate;
//   - the map (default PortExport/maps/survival.json, the arena; or the given map, e.g. Design/maps/drydock.json): its
//     longest clear flat straight, found by casting the map's own collider (down-rays for a flat floor, side rays for
//     walls), each row cut where the straight ends (the window column says how long it ran; x walk compares mean speeds).
// Every row starts with a 1 s walk-up (W held), so it starts at walk speed. Columns: metres in the window, x walk, the
// peak speed, the speed at the window's end, the inputs per second (press edges of slide / jump / dash).
// Then the skilled lines (target d): the web's recorded runs (dash_jump, slide_jump, wall_ride: their own inputs on their
// own scene) with the rules as set, and, for DRYDOCK, the slipway's peak and LAUNCH RAIL's take-offs (verbs' scenarios).
// `--off` runs every flag of design.move off (the web's mover); default: design.move as configured (the run defaults).
// `spam --fingerprint [--off]`: the composed fingerprint run (seed 1) with design.move's defaults, or with every move flag
// off (--off: must be EQUAL to FingerprintBaseline, the web's mover), the hash lines printed.
// `--charge-from-entry <s>`: the slide charge's clock from the slide's entry, <s> seconds (Apex / Titanfall 2's 2 s
// slide-boost cooldown) instead of design.move's rechargeTime after the slide ends.
// Exit 0 when targets (a) tapped every 0.15 .. 2.0 s <= 1.1 x walk, (b) held <= 1.1 x and (c) hop chain peak <= 15 m/s
// hold on the plane and on the map. The recharge-timed rows (one burst per recharge: a skill rhythm) are printed, not gated.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;

public static partial class Harness
{
    sealed class SpamRow
    {
        public string name;
        public double metres, window, peak, endSpeed, inputs, ratio;
        public bool gateA, gateB, gateC, rhythm;
        public bool sprint; // NIGHT17 U3-30.4: the peak is after the boost (the sprint's speed): gate (e) below the slide's floor
    }

    sealed class SpamWorld
    {
        public string name;
        public ICollisionWorld world;
        public SurvivalMap map; // pads (null: the plane)
        public Vec3 start;
        public double yaw, length = double.PositiveInfinity;
        public int scanned;
        public ICollisionWorld canyon; // the wall-jump row's world (the plane: two walls 7 m apart; a map: its own)
    }

    static partial void SpamCommand(string[] args, ref int code) => code = Spam(args);

    static string F1(double v) => v.ToString("0.0", CultureInfo.InvariantCulture);

    static double spamFromEntry = -1; // --charge-from-entry <s>

    static bool spamStaminaOff; // NIGHT17 U3-30.4: --stamina-off (design.stamina's flags off: the web's pips)

    /// <summary>The rules as a run sets them (design.move's and design.stamina's defaults, as automation's runs; --off: every
    /// flag off, the web's mover; --stamina-off: the stamina flags off only).</summary>
    static void SpamRules(Mover m, GameConfig cfg, bool off)
    {
        MoveBalance.Params p = MoveBalance.Params.From(cfg);
        if (off) p = p.With(false, false, false);
        if (spamFromEntry > 0)
        {
            p.chargeFromEntry = true;
            p.rechargeTime = spamFromEntry;
            p.rechargeSteps = MovementMath.StepsFor(spamFromEntry, 1 / cfg.movement.loop.simHz);
        }
        m.SetMoveBalance(p);
        m.SetStamina(off || spamStaminaOff ? Stamina.Params.Neutral : Stamina.Params.From(cfg)); // NIGHT17 U3-30.4
    }

    /// <summary>NIGHT17 U3-30.4: the boost key ready for the skilled rows: the web's pips (rules off: the rows' NIGHT16 tests), or on
    /// the bar a full boost (+ its dash-jump).</summary>
    static bool SpamBoostReady(Mover m, MovementConfig mc, bool dashJump) =>
        m.stamina.rules.Any ? m.BoostReady(mc.dash.pipCost, dashJump) : dashJump ? m.pips >= mc.dash.pipCost + mc.dashJump.extraPipCost : m.pips >= mc.dash.pipCost + 0.99;

    static int Spam(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        spamStaminaOff = Array.IndexOf(args, "--stamina-off") >= 0;
        string mapPath = null;
        spamFromEntry = -1;
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--charge-from-entry" && i + 1 < args.Length) spamFromEntry = double.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (!args[i].StartsWith("--")) mapPath = args[i];
        }
        GameConfig cfg = Config();
        MovementConfig mc = cfg.movement;
        if (Array.IndexOf(args, "--fingerprint") >= 0)
        {
            Skygrind.Game.SurvivalSim sim = Skygrind.Game.SurvivalSim.Headless(cfg, Map());
            if (off) sim.moveBalanceOverride = new[] { false, false, false };
            if (off || spamStaminaOff) sim.staminaOverride = new bool[Stamina.FlagCount]; // NIGHT17 U3-30.4: the web's pips
            int boosts = 0, partials = 0, sprints = 0, fulls = 0;
            sim.mover.Events += e =>
            {
                if (e.type == MoverEventType.Dash) boosts++;
                else if (e.type == MoverEventType.BoostPartial) partials++;
                else if (e.type == MoverEventType.SprintStart) sprints++;
                else if (e.type == MoverEventType.StaminaFull) fulls++;
            };
            List<string> lines = sim.RunFingerprint(Skygrind.Game.SimFingerprint.Seed, Skygrind.Game.SimFingerprint.Steps, Skygrind.Game.SimFingerprint.Every, out string summary);
            int diff = Skygrind.Game.FingerprintBaseline.FirstDifference(lines);
            Console.WriteLine($"spam fingerprint, move flags {(off ? "OFF" : "design.move's defaults")}, stamina flags {(off || spamStaminaOff ? "OFF (the web's pips)" : "design.stamina's defaults")}: {lines.Count} hashes: {(diff < 0 ? "EQUAL to the baseline" : $"first difference at checkpoint {diff + 1}")}; {summary}");
            Console.WriteLine($"  stamina: bar on {sim.mover.stamina.rules.Any}, boosts {boosts}, partial {partials}, sprints {sprints}, refilled to full {fulls}, the bar at the end {F(sim.mover.stamina.bar)} of {F(sim.mover.StaminaMax)}");
            foreach (string l in lines) Console.WriteLine("  " + l);
            return off ? (diff < 0 ? 0 : 1) : 0;
        }
        MoveBalance.Params shown = MoveBalance.Params.From(cfg);
        Console.WriteLine($"spam: design.move {(off ? "OFF (--off: the web's mover)" : $"ON (charge {shown.slideCharge}: {(spamFromEntry > 0 ? $"{spamFromEntry} s from the slide's entry" : $"{shown.rechargeTime} s after the slide ends")}; decay {shown.slideDecay}: sustain {shown.sustainTime} s, decay {shown.decayTime} s; hops {shown.hopFalloff}: window {shown.chainWindow} s, step {shown.hopStep}, min {shown.minMult})")}; 120 Hz; every row: 1 s walk-up, then the window");

        var worlds = new List<SpamWorld>();
        {
            var plane = new TriangleWorld();
            plane.AddBox(new Vec3(-2000, -1, -2000), new Vec3(2000, 0, 2000));
            var canyon = new TriangleWorld();
            canyon.AddBox(new Vec3(-2000, -1, -2000), new Vec3(2000, 0, 2000));
            canyon.AddBox(new Vec3(-4.5, 0, -2000), new Vec3(-3.5, 12, 2000)); // a canyon 7 m wide (MAP-DESIGN's rideable wall)
            canyon.AddBox(new Vec3(3.5, 0, -2000), new Vec3(4.5, 12, 2000));
            worlds.Add(new SpamWorld { name = "plane", world = plane, start = new Vec3(0, 0, 1000), yaw = 0, canyon = canyon });
        }
        string path = mapPath ?? Path.Combine(Root, "maps", "survival.json");
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        ThreeOctree octree = map.BuildOctree();
        CoreGrid core = map.CreateCore(mc);
        ICollisionWorld mapWorld = core == null ? (ICollisionWorld)octree : new GameWorld(octree, core); // the arena's middle is the core grid
        SpamWorld mw = SpamStraight(map, mapWorld);
        worlds.Add(mw);
        Console.WriteLine($"spam {map.name}: the longest clear flat straight: from {V(mw.start)} yaw {F(mw.yaw)} rad, {F1(mw.length)} m (floor y {F(mw.start.y)}; {mw.scanned} floor points scanned)");

        bool pass = true;
        foreach (SpamWorld w in worlds)
        {
            List<SpamRow> rows = SpamRows(cfg, mc, w, off);
            Console.WriteLine($"spam {w.name}: {"row",-34} {"metres",7} {"window",7} {"x walk",7} {"peak",6} {"end",6} {"in/s",5}");
            foreach (SpamRow r in rows)
            {
                string gate = r.gateA ? (r.ratio <= 1.1 ? "  (a) ok" : "  (a) OVER 1.1") : r.gateB ? (r.ratio <= 1.1 ? "  (b) ok" : "  (b) OVER 1.1") : r.gateC ? (r.peak <= 15 ? "  (c) ok" : "  (c) OVER 15 m/s")
                    : r.sprint ? (r.peak < mc.slide.floorSpeed ? $"  (e) ok: under the slide's {F1(mc.slide.floorSpeed)}" : $"  (e) NOT under the slide's {F1(mc.slide.floorSpeed)}") : r.rhythm ? $"  (a') the charge's rhythm: {F(r.ratio)} x (not gated)" : "";
                if ((r.gateA || r.gateB) && r.ratio > 1.1) pass = false;
                if (r.gateC && r.peak > 15) pass = false;
                if (r.sprint && r.peak >= mc.slide.floorSpeed) pass = false; // NIGHT17 U3-30.4 (e): the slide stays the fastest free move
                Console.WriteLine($"spam {w.name}: {r.name,-34} {F1(r.metres),7} {F(r.window),7} {F(r.ratio),7} {F1(r.peak),6} {F1(r.endSpeed),6} {F1(r.inputs),5}{gate}");
            }
        }
        SpamSkilled(cfg, mc, off, map, octree, mapPath != null && map.name.ToLowerInvariant().Contains("drydock"));
        if (!SpamBoosts(cfg, mc, worlds[0], off)) pass = false;
        Console.WriteLine($"spam: targets (a) (b) (c) (e) (f) {(pass ? "HOLD" : "MISSED")}  (* NIGHT17 rows: the peak after the boost, the sprint's)");
        return pass ? 0 : 1;
    }

    /// <summary>NIGHT17 U3-30.4: one boost from walk speed on the plane at a full bar and with the bar at given shares of the boost's
    /// cost (set just before the press, the refill's delay started): its steps, its distance (Mover.lastDashDistance) and its share
    /// of a full boost's. Target (f): at 0.5 of the cost it travels 0.45 .. 0.55 of a full boost (the duration scales, the speed doesn't).</summary>
    static bool SpamBoosts(GameConfig cfg, MovementConfig mc, SpamWorld w, bool off)
    {
        double dt = 1 / mc.loop.simHz;
        Stamina.Params p = Stamina.Params.From(cfg);
        double full = -1;
        bool pass = true;
        Console.WriteLine($"spam boost: {"fill (of the cost)",-22} {"bar",5} {"steps",6} {"metres",7} {"x full",7} {"peak",6}");
        foreach (double share in new[] { 1.0, 0.9, 0.75, 0.5, 0.25, 0.2 })
        {
            var m = new Mover(mc, w.world);
            SpamRules(m, cfg, off);
            m.Reset(w.start.x, w.start.y, w.start.z);
            for (int i = 0; i < 30; i++) m.Step(new InputFrame { yaw = w.yaw }, dt);
            for (int i = 0; i < 120; i++) m.Step(new InputFrame { yaw = w.yaw, moveZ = 1 }, dt);
            bool bar = m.stamina.rules.Any;
            if (bar && share < 1)
            {
                m.stamina.bar = share * p.cost;
                m.stamina.sinceSpend = 0;
            }
            int steps = 0;
            double peak = 0;
            bool fired = false;
            m.Step(new InputFrame { yaw = w.yaw, moveZ = 1, dash = new Button { held = true, pressed = true } }, dt);
            fired = m.state == MoveState.Dash;
            if (fired) steps = 1;
            while (m.state == MoveState.Dash && steps < 200)
            {
                peak = Math.Max(peak, m.Speed);
                m.Step(new InputFrame { yaw = w.yaw, moveZ = 1 }, dt);
                steps++;
            }
            double metres = fired ? m.lastDashDistance : 0;
            if (share == 1) full = metres;
            double x = full > 0 ? metres / full : 0;
            string gate = "";
            if (bar && share == 0.5)
            {
                bool ok = x >= 0.45 && x <= 0.55;
                gate = ok ? "  (f) ok: about half" : "  (f) NOT about half";
                if (!ok) pass = false;
            }
            if (!fired) gate = "  no boost (under the floor: dashFail)";
            Console.WriteLine($"spam boost: {(share == 1 ? "full bar" : F(share)),-22} {F(bar ? (share == 1 ? m.StaminaMax : share * p.cost) : m.pips),5} {steps,6} {F(metres),7} {F(x),7} {F1(peak),6}{gate}");
        }
        return pass;
    }

    /// <summary>Every row on one world. The window is 10 s, cut where the straight ends (a map).</summary>
    static List<SpamRow> SpamRows(GameConfig cfg, MovementConfig mc, SpamWorld w, bool off)
    {
        var rows = new List<SpamRow>();
        Button P(bool held, bool edge) => new Button { held = held, pressed = edge };
        double yaw = w.yaw;
        SpamRow walk = SpamDrive(cfg, mc, w, w.world, off, "walk (W held)", (i, m) => new InputFrame { yaw = yaw, moveZ = 1 });
        if (walk.endSpeed < 1 && walk.metres < w.length)
        {
            // something the rays missed stopped the walker: the straight ends 3 m before it
            w.length = walk.metres - 3;
            walk = SpamDrive(cfg, mc, w, w.world, off, "walk (W held)", (i, m) => new InputFrame { yaw = yaw, moveZ = 1 });
        }
        rows.Add(walk);
        foreach (double period in new[] { 0.15, 0.3, 0.5, 1.0, 1.6, 2.0 })
        {
            int per = (int)Math.Round(period * 120);
            int half = Math.Max(1, per / 2);
            SpamRow r = SpamDrive(cfg, mc, w, w.world, off, $"slide tapped every {period.ToString("0.00", CultureInfo.InvariantCulture)} s", (i, m) =>
                new InputFrame { yaw = yaw, moveZ = 1, slide = P(i >= 0 && i % per < half, i >= 0 && i % per == 0) });
            r.gateA = true;
            rows.Add(r);
        }
        foreach (double period in new[] { 0.15, 0.5 })
        {
            // NIGHT16 review: from a standstill (1 s with no input stops the walk-up), slide tapped with no W: an uncharged
            // slide moves at walk speed along the facing (it rooted the player at 0 m/s)
            int per = (int)Math.Round(period * 120);
            int half = Math.Max(1, per / 2);
            SpamRow r = SpamDrive(cfg, mc, w, w.world, off, $"standstill, no W, tapped every {period.ToString("0.00", CultureInfo.InvariantCulture)} s", (i, m) =>
                new InputFrame { yaw = yaw, slide = P(i >= 120 && (i - 120) % per < half, i >= 120 && (i - 120) % per == 0) });
            r.gateA = true;
            rows.Add(r);
        }
        foreach (bool full in new[] { false, true })
        {
            // the recharge-timed taps (the worst case for the charge): slide for the sustain (or the sustain + the decay: the
            // whole burst), release, walk the recharge (+ 2 steps), repeat; design.move's numbers (the web's: 0.6 / 1.0 s)
            MoveBalance.Params bp = MoveBalance.Params.From(cfg);
            double sustain = bp.slideDecay || bp.sustainTime > 0 ? bp.sustainTime : 0.6, decay = bp.decayTime, recharge = bp.rechargeTime > 0 ? bp.rechargeTime : 1.0;
            if (spamFromEntry > 0) recharge = spamFromEntry;
            int hold = (int)Math.Round((full ? sustain + decay : sustain) * 120);
            int per = spamFromEntry > 0 ? Math.Max(hold + 2, (int)Math.Round(recharge * 120) + 2) : hold + (int)Math.Round(recharge * 120) + 2;
            SpamRow r = SpamDrive(cfg, mc, w, w.world, off, $"slide {F(hold / 120.0)} s / cycle {F(per / 120.0)} s (timed)", (i, m) =>
                new InputFrame { yaw = yaw, moveZ = 1, slide = P(i >= 0 && i % per < hold, i >= 0 && i % per == 0) });
            r.rhythm = true; // a skill rhythm (one burst per recharge), reported, not gated: target (a) is the spam rates
            rows.Add(r);
        }
        {
            SpamRow r = SpamDrive(cfg, mc, w, w.world, off, "slide held 10 s", (i, m) => new InputFrame { yaw = yaw, moveZ = 1, slide = P(i >= 0, i == 0) });
            r.gateB = true;
            rows.Add(r);
        }
        {
            // the slide-hop chain: slide held; a jump pressed on every grounded step (the first landing step after each hop)
            SpamRow r = SpamDrive(cfg, mc, w, w.world, off, "slide-hop chain (jump each landing)", (i, m) =>
            {
                bool jump = i >= 6 && m.grounded;
                return new InputFrame { yaw = yaw, moveZ = 1, slide = P(i >= 0, i == 0), jump = P(jump, jump) };
            });
            r.gateC = true;
            rows.Add(r);
        }
        {
            // the skilled hop: one slide-jump, then a bunny hop (no slide) on each landing
            SpamRow r = SpamDrive(cfg, mc, w, w.world, off, "one slide-jump, then bunny hops", (i, m) =>
            {
                bool jump = (i == 6) || (i > 6 && m.grounded);
                return new InputFrame { yaw = yaw, moveZ = 1, slide = P(i >= 0 && i <= 6, i == 0), jump = P(jump, jump) };
            });
            rows.Add(r);
        }
        rows.Add(SpamDrive(cfg, mc, w, w.world, off, "bunny hops (jump each landing)", (i, m) =>
        {
            bool jump = m.grounded;
            return new InputFrame { yaw = yaw, moveZ = 1, jump = P(jump, jump) };
        }));
        rows.Add(SpamDrive(cfg, mc, w, w.world, off, "dash mashed (every 0.1 s)", (i, m) =>
            new InputFrame { yaw = yaw, moveZ = 1, dash = P(i % 12 == 0, i % 12 == 0) }));
        rows.Add(SpamDrive(cfg, mc, w, w.world, off, "dash on each pip (skilled)", (i, m) =>
        {
            bool dash = m.state != MoveState.Dash && i % 12 == 0 && SpamBoostReady(m, mc, false); // NIGHT17: the bar's full boost with the rules on
            return new InputFrame { yaw = yaw, moveZ = 1, dash = P(dash, dash) };
        }));
        {
            int dashed = -1000;
            rows.Add(SpamDrive(cfg, mc, w, w.world, off, "dash-jump on 2 pips (skilled)", (i, m) =>
            {
                if (i == 0) dashed = -1000;
                bool dash = m.grounded && m.state != MoveState.Dash && SpamBoostReady(m, mc, true) && i - dashed > 24;
                if (dash) dashed = i;
                bool jump = i - dashed == 12;
                return new InputFrame { yaw = yaw, moveZ = 1, dash = P(dash, dash), jump = P(jump, jump) };
            }));
        }
        {
            // NIGHT17 U3-30.4 (one key): the boost key pressed, then held the whole window: the boost, then the sprint until the bar
            // is empty (no lockout: the hold doesn't restart it). The peak column after the boost (from 0.33 s): the sprint's speed.
            SpamRow r = SpamDrive(cfg, mc, w, w.world, off, "sprint held (press, hold 10 s) *", (i, m) =>
                new InputFrame { yaw = yaw, moveZ = 1, dash = P(true, i == 0) }, 40);
            r.sprint = true;
            rows.Add(r);
        }
        {
            // a tap, then at 1 s a press held: the second boost with what refilled (a partial one), then the sprint
            SpamRow r = SpamDrive(cfg, mc, w, w.world, off, "boost-then-sprint (tap; hold at 1 s) *", (i, m) =>
                new InputFrame { yaw = yaw, moveZ = 1, dash = P(i == 0 || i >= 120, i == 0 || i == 120) }, 160);
            r.sprint = true;
            rows.Add(r);
        }
        {
            // a press-and-hold every 3 s (the bar refills between): boost, sprint, release, refill
            SpamRow r = SpamDrive(cfg, mc, w, w.world, off, "boost + sprint every 3 s (hold 1.5 s) *", (i, m) =>
                new InputFrame { yaw = yaw, moveZ = 1, dash = P(i % 360 < 180, i % 360 == 0) });
            rows.Add(r);
        }
        {
            // slam-boost chain: jump on landing (a boost jump inside the window), C pressed 0.25 s after each jump (the slam)
            int jumped = -1000;
            rows.Add(SpamDrive(cfg, mc, w, w.world, off, "slam-boost chain", (i, m) =>
            {
                if (i == 0) jumped = -1000;
                bool jump = m.grounded && i - jumped > 6;
                if (jump) jumped = i;
                bool slam = !m.grounded && i - jumped == 30;
                return new InputFrame { yaw = yaw, moveZ = 1, jump = P(jump, jump), slide = P(slam, slam) };
            }));
        }
        if (w.canyon != null)
        {
            // the wall-jump chain in a 7 m canyon: facing along it, angled 0.35 rad toward a wall; jump mashed every 0.2 s
            rows.Add(SpamDrive(cfg, mc, w, w.canyon, off, "wall-jump chain (canyon, jump 0.2 s)", (i, m) =>
                new InputFrame { yaw = yaw + 0.35 * (((i / 60) % 2) == 0 ? 1 : -1), moveZ = 1, jump = P(i % 24 == 0, i % 24 == 0) }));
        }
        foreach (SpamRow r in rows) r.ratio = walk.metres > 0 ? (r.metres / r.window) / (walk.metres / walk.window) : 0;
        return rows;
    }

    /// <summary>A 1 s walk-up (W held, 30 settle steps first), then `input` for the window (10 s, or the straight's end).</summary>
    static SpamRow SpamDrive(GameConfig cfg, MovementConfig mc, SpamWorld w, ICollisionWorld world, bool off, string name, Func<int, Mover, InputFrame> input, int peakFrom = 0)
    {
        var m = new Mover(mc, world);
        SpamRules(m, cfg, off);
        m.Reset(w.start.x, w.start.y, w.start.z);
        double dt = 1 / mc.loop.simHz;
        for (int i = 0; i < 30; i++) m.Step(new InputFrame { yaw = w.yaw }, dt);
        for (int i = 0; i < 120; i++)
        {
            m.Step(new InputFrame { yaw = w.yaw, moveZ = 1 }, dt);
            w.map?.CheckPads(m, mc, null);
        }
        var r = new SpamRow { name = name };
        Vec3 last = m.position;
        int presses = 0;
        int steps = 0;
        double travelled = 0;
        for (int i = 0; i < 1200; i++)
        {
            InputFrame f = input(i, m);
            if (f.slide.pressed) presses++;
            if (f.jump.pressed) presses++;
            if (f.dash.pressed) presses++;
            m.Step(f, dt);
            w.map?.CheckPads(m, mc, null);
            travelled += Math.Sqrt(Sq(m.position.x - last.x) + Sq(m.position.z - last.z));
            last = m.position;
            if (i >= peakFrom) r.peak = Math.Max(r.peak, m.Speed);
            steps = i + 1;
            if (travelled >= w.length - 3) break;
        }
        r.metres = travelled;
        r.window = steps * dt;
        r.endSpeed = m.Speed;
        r.inputs = presses / r.window;
        return r;
    }

    /// <summary>The map's longest clear flat straight: a 6 m grid of floor points, 16 headings each, down-rays every 1 m
    /// (the same flat floor height), and rays at 0.3 / 1.0 / 1.7 m on the centre line and 0.6 m to each side for walls.</summary>
    static SpamWorld SpamStraight(SurvivalMap map, ICollisionWorld world)
    {
        var best = new SpamWorld { name = map.name, world = world, map = map, length = 0 };
        Vec3 down = new Vec3(0, -1, 0);
        for (double x = -150; x <= 150; x += 6)
        for (double z = -150; z <= 150; z += 6)
        {
            if (!world.RayIntersect(new Vec3(x, 35, z), down, out RayHit h0)) continue;
            best.scanned++;
            if (Math.Abs(h0.normal.y) < 0.9999) continue;
            double y0 = 35 - h0.distance; // from 35 m: the arena's lid is at 41
            bool nearPad = false;
            foreach (PadSpec pad in map.pads)
                if (Math.Abs(pad.pos.x - x) < 3 && Math.Abs(pad.pos.z - z) < 3) nearPad = true;
            if (nearPad) continue;
            if (y0 < -1 || y0 > 30) continue;
            for (int k = 0; k < 16; k++)
            {
                double yaw = k * Math.PI / 8;
                var dir = new Vec3(-Math.Sin(yaw), 0, -Math.Cos(yaw)); // yaw 0 looks toward -z
                var side = new Vec3(dir.z, 0, -dir.x);
                double clear = 400;
                foreach (double hy in new[] { 0.3, 1.0, 1.7 })
                foreach (double s in new[] { -0.6, 0.0, 0.6 })
                {
                    var o = new Vec3(x + side.x * s, y0 + hy, z + side.z * s);
                    if (world.RayIntersect(o, dir, out RayHit hw)) clear = Math.Min(clear, hw.distance - 0.5);
                }
                if (clear <= best.length) continue;
                double flat = 0;
                for (double d = 1; d <= clear; d += 1)
                {
                    var p = new Vec3(x + dir.x * d, y0 + 2, z + dir.z * d);
                    bool floor = true;
                    foreach (double s in new[] { -1.5, 0.0, 1.5 }) // the floor under the line and 1.5 m to each side (no wall tops)
                    {
                        var q = new Vec3(p.x + side.x * s, p.y, p.z + side.z * s);
                        if (!world.RayIntersect(q, down, out RayHit hd) || Math.Abs(2 - hd.distance) > 1e-6 || Math.Abs(hd.normal.y) < 0.9999) floor = false;
                    }
                    if (!floor) break;
                    bool pad = false;
                    foreach (PadSpec ps in map.pads)
                        if (Sq(ps.pos.x - p.x) + Sq(ps.pos.z - p.z) < 9) pad = true; // a jump pad on the line: not a straight (the pads launch)
                    if (pad) break;
                    flat = d;
                }
                if (flat > best.length)
                {
                    best.length = flat;
                    best.start = new Vec3(x, y0, z);
                    best.yaw = yaw;
                }
            }
        }
        return best;
    }

    /// <summary>Target (d): the web's recorded skilled runs and DRYDOCK's slipway / LAUNCH RAIL with the rules as set.</summary>
    static void SpamSkilled(GameConfig cfg, MovementConfig mc, bool off, SurvivalMap map, ThreeOctree octree, bool drydock)
    {
        var data = new GoldenData(Root);
        foreach (string name in new[] { "dash_jump", "slide_jump", "wall_ride" })
        {
            Trajectory t = Trajectory.Load(Path.Combine(Root, "trajectories", name + ".json"));
            var m = new Mover(mc, Golden.ReferenceWorld(data, t)) { ropeEnabled = t.ropeEnabled };
            SpamRules(m, cfg, off);
            m.Reset(t.start.x, t.start.y, t.start.z);
            bool wasGrounded = true, flying = false, done = false;
            Vec3 takeoff = t.start;
            double across = 0, up = 0, maxY = t.start.y;
            foreach (Dictionary<string, object> row in t.rows)
            {
                m.Step(TrajectoryReplay.Frame(PortData.Obj(row["input"]), t.anchor), t.dt);
                maxY = Math.Max(maxY, m.position.y);
                if (!done && wasGrounded && !m.grounded && !flying)
                {
                    flying = true;
                    takeoff = m.position;
                }
                if (flying && !done)
                {
                    up = Math.Max(up, m.position.y - takeoff.y);
                    if (m.grounded)
                    {
                        across = Math.Sqrt(Sq(m.position.x - takeoff.x) + Sq(m.position.z - takeoff.z));
                        done = true;
                    }
                }
                wasGrounded = m.grounded;
            }
            Console.WriteLine($"spam skilled {name}: first airtime {F(across)} m across, {F(up)} m up; climb (max y - start y) {F(maxY - t.start.y)} m");
        }
        if (!drydock) return;
        Button Press(bool on, bool edge) => new Button { held = on, pressed = edge };
        Func<int, Mover, InputFrame> Ruled(Func<int, Mover, InputFrame> f)
        {
            Mover seen = null;
            return (i, m) =>
            {
                if (!ReferenceEquals(seen, m))
                {
                    seen = m;
                    SpamRules(m, cfg, off);
                }
                return f(i, m);
            };
        }
        VerbRun s = VerbDrive(mc, octree, map, new Vec3(30, 10, -25), 360, Ruled((i, m) => new InputFrame { yaw = Math.PI, moveZ = 1, slide = Press(i >= 30, i == 30) }));
        Console.WriteLine($"spam skilled drydock slipway lane A slide: peak {F(s.peakSpeed)} m/s");
        int hits = 0;
        foreach (double jumpZ in new[] { 22.5, 23.0, 23.5, 24.0, 24.4, 24.8 })
        {
            int jumped = -1;
            VerbRun r = VerbDrive(mc, octree, map, new Vec3(30, 10, -25), 600, Ruled((i, m) =>
            {
                bool now = jumped < 0 && m.position.z >= jumpZ;
                if (now) jumped = i;
                return new InputFrame { yaw = Math.PI, moveZ = 1, slide = Press(i >= 30 && jumped < 0, i == 30), jump = Press(jumped >= 0 && i - jumped < 30, now) };
            }));
            bool onRail = r.landed && r.firstLanding.y > 2.5 && r.firstLanding.x >= 26 && r.firstLanding.x <= 34 && r.firstLanding.z >= 30 && r.firstLanding.z <= 34;
            if (onRail) hits++;
        }
        Console.WriteLine($"spam skilled drydock launch rail: {hits} of 6 take-off points land on the rail");
    }
}
