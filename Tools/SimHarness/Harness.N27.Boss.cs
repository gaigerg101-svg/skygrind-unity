// NIGHT27 builder B: `boss27 [lantern|glacier] [--seeds n] [--dbg]` the bosses' headless judges (Core/Enemies/Lantern27.cs, Glacier27.cs; design.n27.boss1 / boss2).
// Each fight is the COMPOSED sim, a scripted pilot (Game/Bot/LanternPilot27.cs, GlacierPilot27.cs) through ScriptedInput, ten seeds, ORGANIC kills only (no HP cut anywhere):
// the pilot rides the wall (the Mover's own rule), shoots only the lit face, and the boss dies of the hits it takes. Lines printed per seed: kill time, deaths, rides, windows, hits.
// RED lines (preconditions printed with them): the row off gives no fight (BossRoomBegin returns false); a STANDING-STILL pilot (fires at the box all the time, never rides) deals nothing and
// never wins in the cap; the pure shield rule (ShieldDown) cases. DISPATCH (the lead's Tools/SimHarness/Program.cs, one line next to N26(...)): N27Boss(cmd, args, ref code);
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static int firstSeed = 1;

    static void N27Boss(string cmd, string[] args, ref int code)
    {
        if (cmd != "boss27") return;
        string which = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = 10;
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == "--seeds") int.TryParse(args[i + 1], out seeds);
        bool dbg = Array.IndexOf(args, "--dbg") >= 0;
        for (int i = 0; i < args.Length - 1; i++) if (args[i] == "--first") int.TryParse(args[i + 1], out firstSeed);
        code = 0;
        GameConfig cfg = Config();
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        if (which == "all" || which == "lantern") code |= Lantern27Command(cfg, seeds, dbg);
        if (which == "all" || which == "glacier") code |= Glacier27Command(cfg, seeds, dbg);
        if (which == "all" || which == "curve") code |= Curve27Command(cfg);
        SurvivalSim.Wyrm24Dir = null;
    }

    sealed class LanternRow
    {
        public int seed, steps, deaths, rides, jumps, attacks, orbHits, litHits, blocks, sentryKills, windows, retries;
        public bool killed;
        public double hpEnd;
        public double litSeconds;
        public int minTellSteps = int.MaxValue, retracts, retryPhaseOk;
        public string phases = "";
    }

    /// <summary>mode 0: the pilot; 1: a standing-still shooter (never moves, fires at the box whenever the ray lands).</summary>
    static LanternRow LanternRun(GameConfig cfg, uint seed, int capSteps, int mode, bool dbg)
    {
        double dt = cfg.Dt;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n27Override = new N27Blocks { boss1 = true };
        sim.n26Override = N26Blocks.Off;
        sim.bosses25Override = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var row = new LanternRow { seed = (int)seed };
        if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yaw)) { row.steps = -1; return row; }
        sim.Respawn(spawn, yaw);
        Lantern27 l = sim.lantern27;
        var si = (ScriptedInput)sim.input;
        var pilot = new LanternPilot27 { hand = seed == 0 ? null : new Mulberry32(seed * 7919u + 13u) };
        double yawL = yaw * Math.PI / 180;
        int step = 0, litSteps = 0, phaseSteps2 = 0;
        string lastPhase = "";
        var phaseAt = new List<string>();
        while (l.Alive && step < capSteps)
        {
            if (mode == 0 || mode == 3)
            {
                pilot.Step(sim, yawL, step);
                yawL = pilot.Move.yaw;
                si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
                si.Set(pilot.Move, pilot.Combat);
            }
            else
            {
                Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z), d = l.Centre - eye;
                double wy = JsMath.Atan2(-d.x, -d.z), wp = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
                var f = new InputFrame { yaw = wy, pitch = wp };
                si.SetLook(wy, wp);
                int gun = sim.loadout.owned.IndexOf("revolver");
                si.Set(f, new CombatInput { weapon = gun >= 0 && sim.loadout.Weapon != "revolver" ? gun : -1, fire = new Skygrind.Core.Button { held = true, pressed = step % 2 == 0 } });
            }
            sim.Step();
            step++;
            if (l.LitSegment >= 0) litSteps++;
            if (dbg && mode == 3 && pilot.jumps >= 70 && pilot.jumps <= 70 && step % 4 == 0)
            {
                Vec3 mp = sim.mover.position - l.Centre, mv = sim.mover.velocity;
                Console.WriteLine($"      j{pilot.jumps} t={step * dt:0.00} pos ({mp.x:0.0},{mp.y:0.0},{mp.z:0.0}) vel ({mv.x:0.0},{mv.y:0.0},{mv.z:0.0}) {sim.mover.state} wish ({pilot.Move.moveX:0.0},{pilot.Move.moveZ:0.0}) yaw {pilot.Move.yaw:0.0}");
            }
            if (mode == 3 && row.retryPhaseOk == 0 && l.phase == "p2" && phaseSteps2++ > 600)
            {
                // a retry (the player died in phase 2): BossRoomBegin with the lantern alive restarts at the START of the phase, same stream
                double hpBefore = l.HpFrac;
                sim.BossRoomBegin("hulk", out spawn, out yaw);
                sim.Respawn(spawn, yaw);
                row.retryPhaseOk = l.Alive && l.phase == "p2" && Math.Abs(l.HpFrac - cfg.Num("design.n27.boss1.hp.phase2At")) < 1e-6 && sim.lantern27Retries == 1 ? 1 : -1;
                Console.WriteLine($"  retry in phase 2: hp {hpBefore:0.00} -> {l.HpFrac:0.00}, phase {l.phase}, retries {sim.lantern27Retries}: {(row.retryPhaseOk == 1 ? "back at the START of phase 2" : "WRONG")}");
            }
            if (l.phase != lastPhase) { lastPhase = l.phase; if (l.phase.StartsWith("p")) phaseAt.Add($"{l.phase}@{step * dt:0}s"); }
            if (dbg && step % 600 == 0) Console.WriteLine($"    t={step * dt:0}s phase {l.phase} hp {l.HpFrac:0.00} lit {l.LitSegment} hits {l.stats["litHits"]}/{l.stats["shieldBlocks"]} rides {pilot.rides} jumps {pilot.jumps} mask {l.retractedMask}/{l.retractWarnMask} [{pilot.dbg}] state {sim.mover.state} pos ({sim.mover.position.x - l.Centre.x:0.0},{sim.mover.position.z - l.Centre.z:0.0}) hp {sim.health.hp:0}");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                row.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.BossRoomBegin("hulk", out spawn, out yaw);
                sim.Respawn(spawn, yaw);
            }
        }
        row.steps = step;
        row.killed = !l.Alive;
        row.hpEnd = l.HpFrac;
        row.rides = pilot.rides;
        row.jumps = pilot.jumps;
        row.attacks = l.stats["attacks"];
        row.orbHits = l.stats["orbHits"];
        row.litHits = l.stats["litHits"];
        row.blocks = l.stats["shieldBlocks"];
        row.sentryKills = l.stats["sentryKills"];
        row.retracts = l.stats["retracts"];
        row.retries = sim.lantern27Retries;
        row.litSeconds = litSteps * dt;
        row.phases = string.Join(" ", phaseAt);
        foreach (Lantern27Log g in l.log) if (!g.resolve && g.name != "walls") row.minTellSteps = Math.Min(row.minTellSteps, MovementMath.StepsFor(g.seconds, dt));
        return row;
    }

    static int Lantern27Command(GameConfig cfg, int seeds, bool dbg)
    {
        double dt = cfg.Dt;
        var fails = new List<string>();
        // RED 1: the row off gives no fight
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n27Override = N27Blocks.Off;
            sim.n26Override = N26Blocks.Off;
            sim.bosses25Override = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(3, null, null);
            bool began = sim.BossRoomBegin("hulk", out _, out _) && sim.lantern27 != null;
            Console.WriteLine($"  RED lantern row OFF (n27.boss1 false): the plan's boss 'hulk' starts {(began ? "THE LANTERN (wrong)" : "no lantern (the Hulk's room, NIGHT26)")}");
            if (began) fails.Add("the row off starts the lantern");
        }
        // the pure shield rule
        {
            bool a = Lantern27Shield.ShieldDown(3, 3, true), b = Lantern27Shield.ShieldDown(3, 3, false), c2 = Lantern27Shield.ShieldDown(3, 4, true), d = Lantern27Shield.ShieldDown(3, -1, true);
            Console.WriteLine($"  shield rule: same wall + riding {(a ? "DOWN" : "up")}; same wall not riding {(b ? "down" : "UP")}; riding another wall {(c2 ? "down" : "UP")}; riding no wall id {(d ? "down" : "UP")}");
            if (!(a && !b && !c2 && !d)) fails.Add("the shield rule is wrong");
        }
        // the FSM's contract (the same case as the EditMode test): one transition per step, needsExitTime holds the state
        {
            var m = new Fsm27("t");
            bool go = false;
            int enteredB = 0;
            m.Add("a", needsExitTime: true, minSteps: 3);
            m.Add("b", enter: s => enteredB++);
            m.Add("c");
            m.Transit("a", "b", () => go);
            m.Transit("b", "c", () => true);
            m.Enter();
            go = true;
            var trace = new List<string>();
            for (int i = 0; i < 7; i++) { m.Step(); trace.Add(m.ActiveName); }
            Console.WriteLine($"  fsm: {string.Join(" ", trace)} (needsExitTime holds 'a' for 3 steps, then one transition per step)");
            if (trace[0] != "a" || trace[6] != "c" || enteredB != 1 || trace[3] != "b" || trace[4] != "c") fails.Add("the fsm trace is wrong");
        }
        // RED 2: a standing-still shooter (row on, full fire on the box, never rides) cannot win
        {
            LanternRow r = LanternRun(cfg, 3, 160 * 120, 1, dbg);
            Console.WriteLine($"  RED standing-still shooter (precondition: row on, pistol, 160 s cap): {(r.killed ? "KILLED (wrong)" : "NOT killed")}; lit hits {r.litHits}, blocked by the shield {r.blocks}, hp left {r.hpEnd:0.00}");
            if (r.killed || r.litHits != 0) fails.Add("a standing shooter damaged / killed the lantern");
        }
        {
            LanternRow r3 = LanternRun(cfg, 4, 240 * 120, 3, dbg);
            Console.WriteLine($"  retry run (seed 4, a retry forced in phase 2): {(r3.killed ? "KILLED" : "NOT killed")} at {r3.steps * dt:0.0} s");
            if (r3.retryPhaseOk != 1 || !r3.killed) fails.Add("the phase retry is wrong");
        }
        double sum = 0;
        int killed = 0, minTell = int.MaxValue;
        var times = new List<double>();
        for (uint sd = (uint)firstSeed; sd < (uint)firstSeed + seeds; sd++)
        {
            LanternRow r = LanternRun(cfg, sd, 240 * 120, 0, dbg);
            if (r.steps < 0) { fails.Add($"seed {sd}: BossRoomBegin failed"); continue; }
            Console.WriteLine($"  GREEN lantern seed {sd}: {(r.killed ? "KILLED" : "NOT KILLED (hp " + r.hpEnd.ToString("0.00") + ")")} at {r.steps * dt:0.0} s; deaths {r.deaths}, rides {r.rides} (jumps {r.jumps}), lit {r.litSeconds:0.0} s, lit hits {r.litHits}, shield blocks {r.blocks}, attacks {r.attacks} (orb hits {r.orbHits}), sentry kills {r.sentryKills}, walls retracted {r.retracts}x; {r.phases}");
            if (r.killed) { killed++; sum += r.steps * dt; times.Add(r.steps * dt); }
            else fails.Add($"seed {sd} not killed");
            minTell = Math.Min(minTell, r.minTellSteps);
        }
        times.Sort();
        double mean = killed > 0 ? sum / killed : 0, med = times.Count > 0 ? times[times.Count / 2] : 0;
        if (killed > 0 && (med < 75 || med > 100)) fails.Add($"median kill time {med:0} s is outside 75-100 s");
        if (minTell != int.MaxValue && minTell < MovementMath.StepsFor(0.6, dt)) fails.Add("a tell under 0.6 s");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"boss27 lantern {(fails.Count == 0 ? "PASS" : "FAIL")}: {killed} of {seeds} killed organically by the pilot (pistol, wall rides), mean {mean:0} s, median {med:0} s (target 75-100 s), shortest tell {(minTell == int.MaxValue ? 0 : minTell * dt):0.00} s");
        return fails.Count == 0 ? 0 : 1;
    }

    sealed class GlacierRow27
    {
        public int seed, steps, deaths, windows, misses, pulseHits, softFails, attacks, attackHits, launches, blocked;
        public bool killed;
        public double hpEnd, gapMin = 1e9, gapMax = -1e9, speedMin = 1e9, speedMax = -1e9, distEnd;
        public int minTellSteps = int.MaxValue, minGapSteps = int.MaxValue;
    }

    /// <summary>mode 0: the pilot (launches with the PULSE); 1: standing still; 2: chases and dodges but never launches (the RED pilot).</summary>
    static GlacierRow27 GlacierRun27(GameConfig cfg, uint seed, int capSteps, int mode, bool dbg)
    {
        double dt = cfg.Dt;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n27Override = new N27Blocks { boss2 = true };
        sim.n24Override = new N24Blocks { chargeFist = true };
        sim.n25Override = new N25Blocks { meleeFire = true, pulseCharge = true, shockwaveOff = true };
        sim.n26Override = new N26Blocks { pulseLT = true, surfaces = true, level2 = true };
        sim.bosses25Override = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var row = new GlacierRow27 { seed = (int)seed };
        if (!sim.BossRoomBegin("glacier", out Vec3 spawn, out double yaw)) { row.steps = -1; return row; }
        sim.Respawn(spawn, yaw);
        Glacier27 g = sim.glacier27;
        if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
        var si = (ScriptedInput)sim.input;
        var pilot = new GlacierPilot27 { noLaunch = mode == 2, hand = new Mulberry32(seed * 7919u + 13u) };
        double yawL = yaw * Math.PI / 180;
        int step = 0, lastFlicks = 0, lastFlickStep = 0;
        Fist24 fist = sim.fist24;
        while (g.Alive && step < capSteps)
        {
            if (mode != 1)
            {
                // the charge arrives banked (the intro's kills fill it): the harness tops it up so a launch is always available
                // (the blast's cooldown is modelled: a launch is available again 15 s after the last one, the cooldown the night asks of a movement verb)
                if (fist != null && fist.Active && fist.Tier < 3 && g.gap < 60 && (fist.flicks == lastFlicks ? step - lastFlickStep > 15 * 120 || fist.flicks == 0 : false)) fist.charge = 1;
                if (fist != null && fist.flicks != lastFlicks) { lastFlicks = fist.flicks; lastFlickStep = step; }
                pilot.Step(sim, yawL, step);
                yawL = pilot.Move.yaw;
                si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
                si.Set(pilot.Move, pilot.Combat);
            }
            else si.Set(new InputFrame { yaw = yawL }, new CombatInput { weapon = -1 });
            int fl0 = fist != null ? fist.flicks : 0;
            Vec3 pre = sim.mover.position, bpre = g.box.c;
            sim.Step();
            step++;
            if (dbg && fist != null && fist.flicks > fl0) Console.WriteLine($"      FLICK t={step * dt:0.00} tier {fist.Tier} wanted {fist.lastWanted:0.0} allowed {fist.lastAllowed:0.0} me ({pre.x:0.0},{pre.y:0.0},{pre.z:0.0}) boss ({bpre.x:0.0},{bpre.y:0.0},{bpre.z:0.0}) dist {Vec3.DistanceTo(pre, bpre):0.0} launchHits {sim.loadout.pulseLaunchHits} win {g.WindowOpen}");
            if (g.phase.StartsWith("p"))
            {
                row.gapMin = Math.Min(row.gapMin, g.gap);
                row.gapMax = Math.Max(row.gapMax, g.gap);
                row.speedMin = Math.Min(row.speedMin, g.CurrentSpeed);
                row.speedMax = Math.Max(row.speedMax, g.CurrentSpeed);
            }
            if (dbg && mode == 0 && pilot.launches > 0 && g.stats["windows"] > 0 && step % 60 == 0 && step < 1500) Console.WriteLine($"      L t={step * dt:0.0} launches {pilot.launches} flicks {fist.flicks} blocked {fist.blocked} pulseLaunches {sim.loadout.pulseLaunches} launchHits {sim.loadout.pulseLaunchHits} gblocked {g.stats["blocked"]} win {g.WindowOpen} gap {g.gap:0.0} weapon {sim.loadout.Weapon}");
            if (dbg && step % 600 == 0) Console.WriteLine($"    t={step * dt:0}s phase {g.phase} hits {g.hits} gap {g.gap:0.0} bossV {g.CurrentSpeed:0.0} playerV {-sim.mover.velocity.z:0.0} win {g.WindowOpen} windows {g.stats["windows"]} misses {g.stats["misses"]} launches {pilot.launches} fist {fist?.charge:0.0} hp {sim.health.hp:0}");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                row.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.BossRoomBegin("glacier", out spawn, out yaw);
                sim.Respawn(spawn, yaw);
            }
        }
        row.steps = step;
        row.killed = !g.Alive;
        row.hpEnd = g.HpFrac;
        row.distEnd = g.playerS;
        row.windows = g.stats["windows"]; row.misses = g.stats["misses"]; row.pulseHits = g.stats["pulseHits"]; row.softFails = g.stats["softFails"];
        row.attacks = g.stats["attacks"]; row.attackHits = g.stats["attackHits"]; row.launches = pilot.launches; row.blocked = g.stats["blocked"];
        int lastRes = -1000000;
        foreach (Glacier27Log l in g.log)
        {
            if (!l.resolve) { row.minTellSteps = Math.Min(row.minTellSteps, MovementMath.StepsFor(l.seconds, dt)); continue; }
            if (lastRes > -1000000) row.minGapSteps = Math.Min(row.minGapSteps, l.step - lastRes);
            lastRes = l.step;
        }
        return row;
    }

    static int Glacier27Command(GameConfig cfg, int seeds, bool dbg)
    {
        double dt = cfg.Dt;
        var fails = new List<string>();
        Glacier27Params gp = Glacier27Params.From(cfg);
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n27Override = N27Blocks.Off;
            sim.n26Override = new N26Blocks { level2 = true, surfaces = true };
            sim.bosses25Override = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(3, null, null);
            sim.BossRoomBegin("glacier", out _, out _);
            bool is27 = sim.glacier27 != null && sim.glacier27.Alive;
            Console.WriteLine($"  RED glacier row OFF (n27.boss2 false): the plan's boss 'glacier' starts {(is27 ? "the NIGHT27 chase (wrong)" : "NIGHT26's glacier")}");
            if (is27) fails.Add("the row off starts the chase");
        }
        {
            // the gap controller's clamp: far under and far over the target stay within +-clampFrac x top of the base
            double lim = gp.clampFrac * gp.top, b = gp.baseFrac * gp.top;
            double lo = Glacier27.SpeedAt(gp, 0.01), hi = Glacier27.SpeedAt(gp, 1000), mid = Glacier27.SpeedAt(gp, gp.targetGap);
            bool exact = Math.Abs(lo - (b - lim)) < 1e-9 && Math.Abs(hi - (b + lim)) < 1e-9 && Math.Abs(mid - b) < 1e-9;
            Console.WriteLine($"  gap controller: speed at gap 0.01 / target / +1000 m = {lo:0.00} / {mid:0.00} / {hi:0.00} m/s; base {b:0.00}, clamp +-{lim:0.00} (15 % of the top speed {gp.top:0.0}): {(exact ? "EXACT" : "WRONG")}");
            if (!exact) fails.Add("the gap controller's clamp is wrong");
        }
        {
            // the hit rule on a fresh fight: a gun never lands, a gauntlet hit outside the window never lands
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n27Override = new N27Blocks { boss2 = true };
            sim.n26Override = new N26Blocks { level2 = true, surfaces = true };
            sim.bosses25Override = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(3, null, null);
            sim.BossRoomBegin("glacier", out _, out _);
            Glacier27 g = sim.glacier27;
            g.phase = "p1";
            g.Filter(25, "revolver", out bool b1);
            g.Filter(32, "gauntlet", out bool b2);
            Console.WriteLine($"  hit rule (no window open): pistol {(b1 ? "blocked" : "LANDED")}, gauntlet {(b2 ? "blocked" : "LANDED")}");
            if (!b1 || !b2) fails.Add("a gun / a hit outside the window landed");
        }
        // RED: standing still, and a chaser that never launches (no PULSE): neither can win
        {
            GlacierRow27 r = GlacierRun27(cfg, 3, 150 * 120, 1, dbg);
            Console.WriteLine($"  RED standing-still pilot (precondition: row on, 150 s cap): {(r.killed ? "KILLED (wrong)" : "NOT killed")}; pulse hits {r.pulseHits}, soft fails {r.softFails}, gap {r.gapMin:0}..{r.gapMax:0} m");
            if (r.killed || r.pulseHits != 0) fails.Add("a standing pilot hurt / killed the glacier");
            GlacierRow27 n = GlacierRun27(cfg, 3, 150 * 120, 2, dbg);
            Console.WriteLine($"  RED chaser that never launches (precondition: row on, gauntlet in hand, charge banked, 150 s cap): {(n.killed ? "KILLED (wrong)" : "NOT killed")}; windows {n.windows}, misses {n.misses} (each costs {gp.missCost:0} m of gap), pulse hits {n.pulseHits}, deaths {n.deaths}, gap {n.gapMin:0}..{n.gapMax:0} m");
            if (n.killed || n.pulseHits != 0) fails.Add("a chaser with no launch hurt / killed the glacier");
        }
        double sum = 0;
        int killed = 0, minTell = int.MaxValue;
        var times = new List<double>();
        for (uint sd = (uint)firstSeed; sd < (uint)firstSeed + seeds; sd++)
        {
            GlacierRow27 r = GlacierRun27(cfg, sd, 220 * 120, 0, dbg);
            if (r.steps < 0) { fails.Add($"seed {sd}: BossRoomBegin failed"); continue; }
            Console.WriteLine($"  GREEN glacier seed {sd}: {(r.killed ? "KILLED" : "NOT KILLED (hits " + r.pulseHits + ")")} at {r.steps * dt:0.0} s over {r.distEnd:0} m; pulse hits {r.pulseHits} of {r.windows} windows ({r.misses} missed), launches {r.launches}; deaths {r.deaths}, soft fails {r.softFails}; attacks {r.attacks} (hit {r.attackHits}); gap {r.gapMin:0}..{r.gapMax:0} m; boss speed {r.speedMin:0.0}..{r.speedMax:0.0} m/s");
            if (r.killed) { killed++; sum += r.steps * dt; times.Add(r.steps * dt); }
            else fails.Add($"seed {sd} not killed");
            minTell = Math.Min(minTell, r.minTellSteps);
            if (r.minGapSteps != int.MaxValue && r.minGapSteps < MovementMath.StepsFor(gp.resolveGap, dt)) fails.Add($"seed {sd}: two lethal attacks {r.minGapSteps * dt:0.00} s apart");
        }
        times.Sort();
        double mean = killed > 0 ? sum / killed : 0, med = times.Count > 0 ? times[times.Count / 2] : 0, max = times.Count > 0 ? times[times.Count - 1] : 0;
        if (killed > 0 && max > 150) fails.Add($"the longest kill {max:0} s is over the 150 s the course allows");
        if (minTell != int.MaxValue && minTell < MovementMath.StepsFor(0.6, dt)) fails.Add("a tell under 0.6 s");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"boss27 glacier {(fails.Count == 0 ? "PASS" : "FAIL")}: {killed} of {seeds} killed organically by PULSE launches (no HP cut), mean {mean:0} s, median {med:0} s, longest {max:0} s (course allows 150 s), shortest tell {(minTell == int.MaxValue ? 0 : minTell * dt):0.00} s");
        return fails.Count == 0 ? 0 : 1;
    }

    /// <summary>The Glacier's view curve (Core/Enemies/Glacier27.cs Glacier27Curve), as numbers: continuity at the tile joins, the tangent against the heading (the camera forward is the sim's
    /// forward (-sin h, -cos h) at yaw + h), the arc length, the cap per tile, the left/right sign, the lateral offset.</summary>
    static int Curve27Command(GameConfig cfg)
    {
        Glacier27Params p = Glacier27Params.From(cfg);
        var fails = new List<string>();
        double worstJoin = 0, worstHead = 0, worstTan = 0, worstLen = 0, worstCap = 0, worstLat = 0, worstPerp = 0;
        int leftOk = 0, leftN = 0, rightOk = 0, rightN = 0;
        for (uint seed = 1; seed <= 20; seed++)
        {
            var c = new Glacier27Curve(seed * 2654435761u, p.curveSalt, p.tileLen, p.curveMaxDeg);
            for (int k = 0; k < 10; k++)
            {
                double e = 1e-6;
                c.Pose(p.tileLen * (k + 1) - e, out double ax, out double az, out double ah);
                c.Pose(p.tileLen * (k + 1) + e, out double bx, out double bz, out double bh);
                worstJoin = Math.Max(worstJoin, JsMath.Hypot(ax - bx, az - bz));
                worstHead = Math.Max(worstHead, Math.Abs(ah - bh));
                worstCap = Math.Max(worstCap, Math.Abs(c.TurnDeg(k)) - p.curveMaxDeg);
                // arc length of tile k by 400 chords
                double len = 0, px = 0, pz = 0;
                for (int i = 0; i <= 400; i++)
                {
                    c.Pose(p.tileLen * k + p.tileLen * i / 400.0, out double x, out double z, out _);
                    if (i > 0) len += JsMath.Hypot(x - px, z - pz);
                    px = x; pz = z;
                }
                worstLen = Math.Max(worstLen, Math.Abs(len - p.tileLen) / p.tileLen);
                // the tangent against the heading, mid tile
                double s0 = p.tileLen * k + 13.7, d = 1e-4;
                c.Pose(s0 - d, out double x0, out double z0, out _);
                c.Pose(s0 + d, out double x1, out double z1, out double h1);
                c.Pose(s0, out _, out _, out double hm);
                double tx = (x1 - x0) / (2 * d), tz = (z1 - z0) / (2 * d);
                worstTan = Math.Max(worstTan, Math.Max(Math.Abs(tx - (-JsMath.Sin(hm))), Math.Abs(tz - (-JsMath.Cos(hm)))));
                // lateral: 3 m to the right of the centre line is 3 m from it and perpendicular to the tangent
                c.Bend(s0, 3, out double lx, out double lz, out _);
                c.Pose(s0, out double cx, out double cz, out _);
                worstLat = Math.Max(worstLat, Math.Abs(JsMath.Hypot(lx - cx, lz - cz) - 3));
                worstPerp = Math.Max(worstPerp, Math.Abs((lx - cx) * tx + (lz - cz) * tz));
                // sign: a left turn (turn > 0) ends to the LEFT of straight on (-x when the heading starts at 0, tile 0)
                if (k == 0 && Math.Abs(c.TurnDeg(0)) > 1)
                {
                    c.Pose(p.tileLen, out double ex, out _, out double eh);
                    if (c.TurnDeg(0) > 0) { leftN++; if (ex < 0 && eh > 0) leftOk++; } else { rightN++; if (ex > 0 && eh < 0) rightOk++; }
                }
            }
        }
        c0(out Glacier27Curve straight);
        straight.Bend(0, 3, out double sx, out _, out double sh);
        Console.WriteLine($"  curve27 (20 seeds x 10 tiles, cap {p.curveMaxDeg:0} deg per {p.tileLen:0} m tile): join position gap {worstJoin:0.0e+0} m, join heading gap {worstHead:0.0e+0} rad, tangent vs (-sin h, -cos h) {worstTan:0.0e+0}, arc length error {worstLen * 100:0.000} %, turn over the cap {Math.Max(0, worstCap):0.0e+0} deg, lateral {worstLat:0.0e+0} m, right-normal x tangent {worstPerp:0.0e+0}; left turns end at -x with h > 0: {leftOk}/{leftN}, right turns at +x with h < 0: {rightOk}/{rightN}; start pose heading {sh:0.0} and +3 m right = x {sx:0.0}");
        if (worstJoin > 1e-4 || worstHead > 1e-4) fails.Add("the curve is not continuous at a tile join");
        if (worstTan > 1e-4) fails.Add("the tangent does not follow the heading");
        if (worstLen > 5e-4) fails.Add("the arc length is off");
        if (worstCap > 1e-9) fails.Add("a tile turns more than the cap");
        if (worstLat > 1e-9 || worstPerp > 1e-3) fails.Add("the lateral offset is wrong");
        if (leftOk != leftN || rightOk != rightN || leftN == 0 || rightN == 0) fails.Add("the turn sign is wrong");
        if (Math.Abs(sh) > 1e-12 || Math.Abs(sx - 3) > 1e-12) fails.Add("the start pose is not the straight course");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"boss27 curve {(fails.Count == 0 ? "PASS" : "FAIL")}");
        return fails.Count == 0 ? 0 : 1;
        void c0(out Glacier27Curve cv) => cv = new Glacier27Curve(5, p.curveSalt, p.tileLen, p.curveMaxDeg);
    }
}
