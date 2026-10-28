// NIGHT39 builder F1 (NIGHT39.md F1; G0 (c)): slow39's headless checks, declared and called by the lead's Program.cs (N39Slow).
//   n39slow signal   G0 (c): does slowAim27's signal fire? The REAL sim (Survival, husks frozen) with its mover moved into a box world (a floor and a
//                    60 m x 420 m wall), met at 3 m in the air moving along the wall, LT held from the first step on the KESTREL (revolver) and on the
//                    THUNDERCLAP (shotgun); per ride row set (wallride35 / wallgrip36 / ride38 as the config has them, ride38 forced on, wallgrip36 off,
//                    wallride35 off) and per ride kind (HOLD: jump held; TAP: jump never pressed): the ride's seconds, the seconds with the signal
//                    (mover.state == WallRide && loadout.adsActive), its share of the ride and the first step it fired. Then the same ride under the
//                    loop's frame clock (60 fps) with NIGHT38's pacing (design.n27.slowAim27) and with slow39 forced on: real seconds for the sim's
//                    ride and the least world share. GREEN when every row with a ride and a zoom gun shows the signal on >= 90 % of the ride.
//   n39slow latch    the whip latch: Whip33Setup's SURVIVAL rig (whip33 as the config has it), a charger pinned 14 m ahead, one grapple tap at its aim
//                    point: steps latched (SurvivalSim.Slow39Latched), the latch under the frame clock (slow39 on: real vs sim seconds, least share).
//                    A light husk's YANK (no rope) must give 0 latched steps. GREEN: latched > 0 on the charger, 0 on the yank.
//   n39slow parry    the parry window's pacing (Slow39 alone, the loop's 60 fps clock): a parry with the impact layer's 0.25 s freeze (share 0), and
//                    with HIT FREEZE off: when the window opens, its real length, the world seconds inside it, the least share; GREEN: the window
//                    opens on the first running frame after the freeze, holds parryScale for parryHoldS and is back to 1 by parryHoldS + parryOutS.
//   n39slow lantern [--off39]   the Player reel `slowworld` re-run headless (THE LANTERN, LanternPilot27, LT on riding frames after 2 control rides,
//                    60 fps clock, 73.6 s): the reel's OLD leak rule (WorldScale) against the NEW one (the row's own share); see N39SlowLantern.
//   n39slow all      the four.
// Every slow39 number comes from design.n39.slow39 (forced on here whatever its `enabled`); the sim never sees the pacing (the same inputs per
// step under both clocks: the end positions are compared bit for bit).
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N39Slow(string cmd, string[] args, ref int code)
    {
        if (cmd != "n39slow") return;
        string sub = args.Length > 1 ? args[1] : "all";
        bool ok = true;
        if (sub == "signal" || sub == "all") ok &= N39SlowSignal();
        if (sub == "latch" || sub == "all") ok &= N39SlowLatch();
        if (sub == "parry" || sub == "all") ok &= N39SlowParry();
        if (sub == "lantern" || sub == "all") ok &= N39SlowLantern(Array.IndexOf(args, "--off39") >= 0);
        if (sub != "signal" && sub != "latch" && sub != "parry" && sub != "lantern" && sub != "all")
        {
            Console.WriteLine("n39slow: signal | latch | parry | lantern | all");
            code = 2;
            return;
        }
        code = ok ? 0 : 1;
    }

    static readonly Vec3 N39SlowOff = new Vec3(2600, 0, 0);
    const double N39SlowFrameDt = 1.0 / 60;

    static string N39SlowF(double v) => double.IsNaN(v) ? "n/a" : v.ToString("0.000", CultureInfo.InvariantCulture);

    /// <summary>slow39's rules with the row forced on (the numbers are read whatever `enabled` says).</summary>
    static Slow39Rules N39SlowRules(GameConfig cfg)
    {
        bool was = N39Blocks.ForceOff;
        N39Blocks.ForceOff = false;
        var r = new Slow39Rules
        {
            on = true,
            scale = cfg.Num("design.n39.slow39.scale"),
            easeInS = cfg.Num("design.n39.slow39.easeInS"),
            easeOutS = cfg.Num("design.n39.slow39.easeOutS"),
            latch = cfg.Bool("design.n39.slow39.latch"),
            parry = cfg.Bool("design.n39.slow39.parry"),
            parryScale = cfg.Num("design.n39.slow39.parryScale"),
            parryHoldS = cfg.Num("design.n39.slow39.parryHoldS"),
            parryOutS = cfg.Num("design.n39.slow39.parryOutS"),
        };
        N39Blocks.ForceOff = was;
        return r;
    }

    /// <summary>The loop's pacing for one frame: slow39 when `s39` is given, else NIGHT38's slowAim27 line (linear, design.n27.slowAim27).</summary>
    sealed class N39SlowPacer
    {
        public Slow39 s39;
        public double scale27, in27, out27, depth27;
        public double Frame(bool target)
        {
            if (s39 != null) return s39.Frame(N39SlowFrameDt, 1, target, true, out _);
            depth27 = target ? Math.Min(1, depth27 + N39SlowFrameDt / in27) : Math.Max(0, depth27 - N39SlowFrameDt / out27);
            return depth27 <= 0 ? 1 : 1 - (1 - scale27) * depth27;
        }
    }

    static N39SlowPacer N39SlowPacer27(GameConfig cfg) => new N39SlowPacer { scale27 = cfg.Num("design.n27.slowAim27.scale"), in27 = cfg.Num("design.n27.slowAim27.easeInS"), out27 = cfg.Num("design.n27.slowAim27.easeOutS") };

    sealed class N39SlowPaced
    {
        public double realS, realTargetS, simTargetS, minShare = 1;
        public int steps;
        public Vec3 end;
    }

    /// <summary>Runs `tick` (one sim step; false = done) under the loop's 60 fps clock with `pacer`: the accumulator gets frameDt x share; real
    /// seconds while `target()` holds vs the sim's seconds while it holds.</summary>
    static N39SlowPaced N39SlowRunPaced(SurvivalSim sim, N39SlowPacer pacer, Func<bool> target, Func<int, bool> tick, int maxSteps)
    {
        var r = new N39SlowPaced();
        double acc = 0, step = sim.cfg.Dt;
        bool go = true;
        while (go && r.steps < maxSteps && r.realS < 120)
        {
            bool t = target();
            double share = pacer.Frame(t);
            r.minShare = Math.Min(r.minShare, share);
            r.realS += N39SlowFrameDt;
            if (t) r.realTargetS += N39SlowFrameDt;
            acc += N39SlowFrameDt * share;
            while (acc >= step && go)
            {
                acc -= step;
                if (target()) r.simTargetS += step;
                go = tick(r.steps);
                r.steps++;
            }
        }
        r.end = sim.mover.position;
        return r;
    }

    // ================================================================================================ signal (G0 c)

    sealed class N39SlowRig
    {
        public SurvivalSim sim;
        public ScriptedInput inp;
    }

    /// <summary>The real sim, Survival, husks frozen, the ride rows as asked; `weapon` in hand and ready; the mover in the box world.</summary>
    static N39SlowRig N39SlowNewRig(GameConfig cfg, string weapon, bool? wallride35, bool? grip36, bool? ride38)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        if (wallride35.HasValue)
        {
            N35Blocks b = N35Blocks.ForceOff ? N35Blocks.Off : N35Blocks.From(cfg);
            b.wallride35 = wallride35.Value;
            sim.n35Override = b;
        }
        if (grip36.HasValue)
        {
            N36Blocks b = N36Blocks.ForceOff ? N36Blocks.Off : N36Blocks.From(cfg);
            b.wallgrip36 = grip36.Value;
            sim.n36Override = b;
        }
        SurvivalSim.Force38Ride = ride38;
        try
        {
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(3901, null, null);
        }
        finally
        {
            SurvivalSim.Force38Ride = null;
        }
        sim.husks.frozen = true;
        sim.map.pads.Clear();
        var rig = new N39SlowRig { sim = sim, inp = (ScriptedInput)sim.input };
        if (!sim.loadout.owned.Contains(weapon)) sim.loadout.AddWeapon(weapon);
        int slot = sim.loadout.owned.IndexOf(weapon);
        for (int k = 0; k < 360 && !(sim.loadout.Weapon == weapon && sim.loadout.ready); k++)
            N39SlowTick(rig, new InputFrame(), new CombatInput { weapon = k % 30 == 0 && sim.loadout.Weapon != weapon ? slot : -1 });
        var w = new TriangleWorld();
        Vec3 o = N39SlowOff;
        w.AddBox(new Vec3(-20, -1, -460) + o, new Vec3(20, 0, 40) + o);
        w.AddBox(new Vec3(3, 0, -420) + o, new Vec3(4, 60, 0) + o);
        sim.mover.World = w;
        sim.mover.SetNoRide38(null);
        return rig;
    }

    static void N39SlowTick(N39SlowRig r, InputFrame move, CombatInput combat)
    {
        r.inp.SetLook(0, 0);
        r.inp.Set(move, combat);
        r.sim.hitstopLeft = 0;
        r.sim.health.hp = r.sim.health.max;
        r.sim.mover.invulnerable = true;
        r.sim.Step();
    }

    sealed class N39SlowRide
    {
        public int rideSteps, signalSteps, firstRide = -1, firstSignal = -1, adsSteps;
        public bool zoomGun;
        public N39SlowPaced paced;
    }

    /// <summary>One ride: met at 3 m moving along the wall, LT held from the first step, jump held (hold) or never pressed (tap); 4 s of steps
    /// (stops 0.3 s after the ride ends). With `pacer`, the run goes through the 60 fps clock.</summary>
    static N39SlowRide N39SlowRideRun(GameConfig cfg, string weapon, bool? wr35, bool? grip36, bool? ride38, bool hold, N39SlowPacer pacer)
    {
        N39SlowRig rig = N39SlowNewRig(cfg, weapon, wr35, grip36, ride38);
        SurvivalSim sim = rig.sim;
        double r = cfg.movement.body.capsuleRadius;
        sim.mover.Reset(N39SlowOff.x + 3 - r - 0.02, 3, -1);
        sim.mover.velocity = new Vec3(1, 0, -8);
        var res = new N39SlowRide { zoomGun = sim.loadout.ZoomGun(weapon) > 1 };
        int endedAt = -1;
        bool Tick(int s)
        {
            var f = new InputFrame { moveX = 0.5, moveZ = 1, jump = new Button { held = hold, pressed = hold && s == 0 } };
            var c = new CombatInput { weapon = -1, altFire = new Button { held = true, pressed = s == 0 } };
            N39SlowTick(rig, f, c);
            bool riding = sim.mover.state == MoveState.WallRide;
            if (sim.loadout.adsActive) res.adsSteps++;
            if (riding)
            {
                res.rideSteps++;
                if (res.firstRide < 0) res.firstRide = s;
                if (sim.Slow39RideAim)
                {
                    res.signalSteps++;
                    if (res.firstSignal < 0) res.firstSignal = s;
                }
            }
            else if (res.firstRide >= 0 && endedAt < 0) endedAt = s;
            int limit = (int)Math.Round(4 / cfg.Dt);
            return s < limit && !(endedAt >= 0 && s - endedAt > (int)Math.Round(0.3 / cfg.Dt));
        }
        if (pacer == null)
        {
            for (int s = 0; Tick(s); s++) { }
        }
        else res.paced = N39SlowRunPaced(sim, pacer, () => sim.Slow39RideAim, Tick, 100000);
        return res;
    }

    static bool N39SlowSignal()
    {
        GameConfig cfg = Config();
        double dt = cfg.Dt;
        Console.WriteLine($"n39slow signal (G0 c): config wallride35 {cfg.Design("n35.wallride35.enabled")}, wallgrip36 {cfg.Design("n36.wallgrip36.enabled")}, ride38 {cfg.Design("n38.ride38.enabled")}; "
            + $"slowAim27 {cfg.Design("n27.slowAim27.enabled")} scale {cfg.Num("design.n27.slowAim27.scale")}; slow39 enabled {cfg.Design("n39.slow39.enabled")} (forced on below) scale {cfg.Num("design.n39.slow39.scale")}; "
            + $"ads.suspendStates [{string.Join(", ", cfg.Strs("ads.suspendStates"))}]");
        var sets = new (string name, bool? wr35, bool? grip, bool? r38)[]
        {
            ("config", null, null, null),
            ("ride38 on", null, null, true),
            ("wallgrip36 off", null, false, null),
            ("wallride35 off", false, null, null),
        };
        bool pass = true;
        foreach (string weapon in new[] { "revolver", "shotgun" })
        {
            foreach (var set in sets)
            {
                foreach (bool hold in new[] { true, false })
                {
                    N39SlowRide a = N39SlowRideRun(cfg, weapon, set.wr35, set.grip, set.r38, hold, null);
                    double share = a.rideSteps > 0 ? (double)a.signalSteps / a.rideSteps : double.NaN;
                    bool green = a.rideSteps == 0 || !a.zoomGun || share >= 0.9;
                    pass &= green;
                    string pacing = "";
                    if (a.rideSteps > 0)
                    {
                        N39SlowRide p27 = N39SlowRideRun(cfg, weapon, set.wr35, set.grip, set.r38, hold, N39SlowPacer27(cfg));
                        N39SlowRide p39 = N39SlowRideRun(cfg, weapon, set.wr35, set.grip, set.r38, hold, new N39SlowPacer { s39 = new Slow39(N39SlowRules(cfg)) });
                        bool same = p27.paced.end.x == p39.paced.end.x && p27.paced.end.y == p39.paced.end.y && p27.paced.end.z == p39.paced.end.z && p27.paced.steps == p39.paced.steps;
                        pass &= same;
                        pacing = $" | signal {N39SlowF(p27.paced.simTargetS)} sim s -> real: n27 {N39SlowF(p27.paced.realTargetS)} s (least share {N39SlowF(p27.paced.minShare)}), slow39 {N39SlowF(p39.paced.realTargetS)} s (least share {N39SlowF(p39.paced.minShare)}); same steps + end {(same ? "YES" : "NO (RED)")}";
                    }
                    Console.WriteLine($"  {weapon,-8} {set.name,-15} {(hold ? "HOLD" : "TAP "),-4}: zoom gun {(a.zoomGun ? "yes" : "NO")}, ride {N39SlowF(a.rideSteps * dt)} s (from step {a.firstRide}), "
                        + $"signal {N39SlowF(a.signalSteps * dt)} s = {(double.IsNaN(share) ? "n/a" : (share * 100).ToString("0", CultureInfo.InvariantCulture) + " %")} of the ride (first at step {a.firstSignal}), zoomed {N39SlowF(a.adsSteps * dt)} s {(green ? "GREEN" : "RED")}{pacing}");
                }
            }
        }
        Console.WriteLine($"n39slow signal: {(pass ? "GREEN" : "RED")}");
        return pass;
    }

    // ================================================================================================ latch

    static bool N39SlowLatch()
    {
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        double dt = cfg.Dt;
        bool pass = true;
        Console.WriteLine($"n39slow latch: whip33 {cfg.Design("n33.whip33.enabled")}, slow39 latch {cfg.Bool("design.n39.slow39.latch")} (forced on), scale {cfg.Num("design.n39.slow39.scale")}");
        foreach (string kind in new[] { "charger", "husk" })
        {
            foreach (bool paced in new[] { false, true })
            {
                Whip33Run r = Whip33Setup(cfg, map, cfg.Design("n33.whip33.enabled"), 3920);
                if (r == null) { Console.WriteLine("  no ARCLINE: RED"); return false; }
                Husk e = Whip33Place(r, kind, kind == "charger" ? 14 : 10, 0, 1e9);
                if (e == null) { Console.WriteLine($"  {kind}: not placed: RED"); pass = false; continue; }
                Vec3 pin = e.position;
                int latched = 0, first = -1;
                bool Tick(int s)
                {
                    if (e.alive && kind == "charger") { e.position = pin; e.velocity = default; }
                    G32Tick(r.sim, r.si, e.alive ? e.AimPoint : (Vec3?)null, s < 2, s == 0, false);
                    if (r.sim.Slow39Latched) { latched++; if (first < 0) first = s; }
                    return s < (int)Math.Round(2.5 / dt);
                }
                N39SlowPaced p = null;
                if (!paced) { for (int s = 0; Tick(s); s++) { } }
                else p = N39SlowRunPaced(r.sim, new N39SlowPacer { s39 = new Slow39(N39SlowRules(cfg)) }, () => r.sim.Slow39Latched, Tick, 100000);
                bool green = kind == "charger" ? latched > 0 : latched == 0;
                pass &= green;
                Console.WriteLine($"  {kind,-7} {(paced ? "paced" : "steps")}: latched {N39SlowF(latched * dt)} s (first at step {first}), whip stops {r.arrivals}, yanks {r.sim.arcline21.pulls}"
                    + (p != null ? $", latched {N39SlowF(p.simTargetS)} sim s -> {N39SlowF(p.realTargetS)} real s, least share {N39SlowF(p.minShare)}" : "") + $" {(green ? "GREEN" : "RED")}");
            }
        }
        Console.WriteLine($"n39slow latch: {(pass ? "GREEN" : "RED")}");
        return pass;
    }

    // ================================================================================================ parry

    static bool N39SlowParry()
    {
        GameConfig cfg = Config();
        Slow39Rules rules = N39SlowRules(cfg);
        double freeze = cfg.Has("design.ultra.parryHitstop") ? cfg.Num("design.ultra.parryHitstop") : cfg.Num("punch.parryHitstop");
        bool pass = true;
        Console.WriteLine($"n39slow parry: parryScale {rules.parryScale}, hold {rules.parryHoldS} s, out {rules.parryOutS} s; the freeze {freeze} s (design.ultra.parryHitstop)");
        foreach (bool hitFreeze in new[] { true, false })
        {
            var s = new Slow39(rules);
            int frozenFrames = hitFreeze ? (int)Math.Ceiling(freeze / N39SlowFrameDt) : 0;
            double t = 0, worldInWindow = 0, minShare = 1, openedAt = double.NaN, closedAt = double.NaN, holdEnd = double.NaN;
            bool parried = false;
            for (int f = 0; f < 300; f++)
            {
                double share = parried && f > 10 && f <= 10 + frozenFrames ? 0 : 1;
                double outShare = s.Frame(N39SlowFrameDt, share, false, true, out _);
                // the parry fires in a sim step of frame 10 (after the loop's ImpactBeginFrame: the event arms the window); its freeze runs from frame 11
                if (f == 10) { s.OnParry(); parried = true; }
                if (double.IsNaN(openedAt) && s.parryWindows > 0) openedAt = t;
                if (!double.IsNaN(openedAt) && share > 0 && outShare < 1)
                {
                    worldInWindow += N39SlowFrameDt * outShare;
                    minShare = Math.Min(minShare, outShare);
                    if (outShare > rules.parryScale + 1e-12 && double.IsNaN(holdEnd)) holdEnd = t;
                }
                if (!double.IsNaN(openedAt) && double.IsNaN(closedAt) && share > 0 && outShare >= 1 && t > openedAt) closedAt = t;
                t += N39SlowFrameDt;
            }
            double firstRun = 10 * N39SlowFrameDt + (frozenFrames + 1) * N39SlowFrameDt; // the first frame after the freeze
            double len = closedAt - openedAt;
            bool green = Math.Abs(openedAt - (hitFreeze ? firstRun : 11 * N39SlowFrameDt)) < 1e-9 && Math.Abs(minShare - rules.parryScale) < 1e-12
                && len >= rules.parryHoldS + rules.parryOutS - N39SlowFrameDt - 1e-9 && len <= rules.parryHoldS + rules.parryOutS + N39SlowFrameDt + 1e-9
                && holdEnd - openedAt >= rules.parryHoldS - N39SlowFrameDt - 1e-9;
            pass &= green;
            Console.WriteLine($"  HIT FREEZE {(hitFreeze ? "on " : "off")}: parry at {N39SlowF(10 * N39SlowFrameDt)} s, freeze {frozenFrames} frames, window opens at {N39SlowF(openedAt)} s, holds {N39SlowF(holdEnd - openedAt)} s at {N39SlowF(minShare)}, back to 1 after {N39SlowF(len)} real s, "
                + $"world time inside {N39SlowF(worldInWindow)} s (x{N39SlowF(len / Math.Max(1e-9, worldInWindow))} slower) {(green ? "GREEN" : "RED")}");
        }
        Console.WriteLine($"n39slow parry: {(pass ? "GREEN" : "RED")}");
        return pass;
    }

    // ================================================================================================ lantern (the slowworld reel, headless)

    /// <summary>The Player reel `slowworld` (FeelReel.N27.Slow.cs) re-run headless: THE LANTERN's room (BossRoomBegin "hulk", husks frozen), the
    /// LanternPilot27 with the reel's seed, LT held only on riding frames after 2 control rides, the loop's 60 fps clock with the loop's own order
    /// (the lantern's floor slow and the bend first, then slowAim27 / slow39), 73.6 s of game time (the n39-r1 run's length). Counts each frame as the
    /// reel does: the OLD leak rule (WorldScale under 1 while not riding + aiming, past the ease-out + 3 frames) and the NEW one (the row's OWN share,
    /// so another system's slow-down is not this row's), the frames THE LANTERN's floor slowed, and the control frames (riding, LT off, before any
    /// aimed ride). `--off39`: NIGHT38's slowAim27 pacing. GREEN: the new rule counts 0 leaks and 0 wrong control frames.</summary>
    static bool N39SlowLantern(bool off39)
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.setup.mode = RunSetup.Survival;
        SurvivalSim.Wyrm24Dir = MapGenRepo; // the boss files (Design/maps)
        sim.bosses25Override = true; // the boss rooms (as Harness.N27.Boss.cs: the Player reel has them through its own setup)
        sim.StartRun(160716, null, null);
        sim.husks.frozen = true;
        bool on27 = sim.Lantern27On, begun = sim.BossRoomBegin("hulk", out Vec3 spawn, out double yawDeg);
        if (!on27 || !begun || sim.lantern27 == null) { Console.WriteLine($"n39slow lantern: no LANTERN room (Lantern27On {on27}, BossRoomBegin {begun}, lantern27 {(sim.lantern27 != null ? "built" : "null")}): RED"); return false; }
        sim.Respawn(spawn, yawDeg);
        Lantern27 l = sim.lantern27;
        var si = (ScriptedInput)sim.input;
        var pilot = new LanternPilot27 { hand = new Mulberry32(160716u * 7919u + 13u) };
        Slow39Rules r39 = N39SlowRules(cfg);
        N39SlowPacer pacer = off39 ? N39SlowPacer27(cfg) : new N39SlowPacer { s39 = new Slow39(r39) };
        double scaleWant = off39 ? cfg.Num("design.n27.slowAim27.scale") : r39.scale;
        double easeOut = off39 ? cfg.Num("design.n27.slowAim27.easeOutS") : r39.easeOutS;
        int outFrames = (int)Math.Ceiling(easeOut * 60) + 3, outFramesOld = (int)Math.Ceiling(cfg.Num("design.n27.slowAim27.easeOutS") * 60) + 3;
        double yawL = yawDeg * Math.PI / 180, acc = 0, step = sim.step;
        int frame = 0, rides = 0, lastTarget = -100000, leakOld = 0, leakNew = 0, floorFrames = 0, floorOnlyLeaks = 0, ctl = 0, ctlOld = 0, ctlNew = 0, targetFrames = 0, parryFrames = 0;
        double minWorld = 1, minOwn = 1;
        bool wasRiding = false;
        int step0 = sim.simStep;
        while (l.Alive && (sim.simStep - step0) * step < 73.6)
        {
            frame++;
            // the loop's frame: the lantern's floor and the bend, then the row (its target read from the sim before this frame's steps)
            double other = Math.Min(sim.Lantern29SlowScale, sim.Bend34Share);
            bool tgt = sim.Slow39RideAim || (!off39 && r39.latch && sim.Slow39Latched);
            double own = pacer.Frame(tgt);
            double world = Math.Min(other, own);
            acc += N39SlowFrameDt * world;
            while (acc >= step)
            {
                acc -= step;
                pilot.Step(sim, yawL, sim.simStep);
                yawL = pilot.Move.yaw;
                bool ridingNow = sim.mover.state == MoveState.WallRide;
                CombatInput cb = pilot.Combat;
                cb.altFire = ridingNow && rides > 2 ? new Skygrind.Core.Button { held = true, pressed = !sim.loadout.adsActive } : default;
                si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
                si.Set(pilot.Move, cb);
                sim.health.hp = sim.health.max;
                sim.Step();
                bool riding = sim.mover.state == MoveState.WallRide;
                if (riding && !wasRiding) rides++;
                wasRiding = riding;
            }
            // what the frame did, the reel's way
            bool target = sim.mover.state == MoveState.WallRide && sim.loadout.adsActive;
            bool nowRiding = sim.mover.state == MoveState.WallRide;
            if (target) { targetFrames++; lastTarget = frame; }
            minWorld = Math.Min(minWorld, world);
            minOwn = Math.Min(minOwn, own);
            if (other < 0.999) floorFrames++;
            bool parryWin = pacer.s39 != null && pacer.s39.parryT >= 0;
            if (parryWin) parryFrames++;
            if (world < 0.999 && !target && frame - lastTarget > outFramesOld) { leakOld++; if (own >= 0.999) floorOnlyLeaks++; }
            if (own < 0.999 && !target && frame - lastTarget > outFrames && !parryWin) leakNew++;
            if (rides <= 2 && nowRiding && !sim.loadout.adsActive && lastTarget < 0)
            {
                ctl++;
                if (world < 0.999) ctlOld++;
                if (own < 0.999) ctlNew++;
            }
        }
        bool green = leakNew == 0 && ctlNew == 0 && targetFrames > 0 && Math.Abs(minOwn - scaleWant) < 0.01;
        Console.WriteLine($"n39slow lantern ({(off39 ? "--off39: slowAim27 0.75" : "slow39 forced on")}): {frame} frames, {(sim.simStep - step0) * step:0.0} s game time, rides {rides}, riding + aiming frames {targetFrames}; "
            + $"THE LANTERN's floor / the bend slowed {floorFrames} frames (design.n29.boss1.slowScale {cfg.Num("design.n29.boss1.slowScale")}); least world share {N39SlowF(minWorld)}, least own share {N39SlowF(minOwn)} (want {N39SlowF(scaleWant)}); "
            + $"OLD rule (WorldScale, 0.10 s ease-out): {leakOld} leak frames ({floorOnlyLeaks} with the row's own share at 1: another system's slow), control {ctlOld} of {ctl} under 1; "
            + $"NEW rule (own share, {N39SlowF(easeOut)} s ease-out, parry window frames {parryFrames} apart): {leakNew} leak frames, control {ctlNew} of {ctl} under 1 {(green ? "GREEN" : "RED")}");
        return green;
    }
}
