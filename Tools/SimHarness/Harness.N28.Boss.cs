// NIGHT28 builder B2: `boss28 [--seeds n] [--first n] [--dbg]` THE GLACIER's lap chase, headless (Core/Enemies/Glacier28.cs; design.n28.glacier).
// Each fight is the COMPOSED sim on the rink (the flat test world until builder A2's banked arena is wired), the scripted pilot (Game/Bot/GlacierPilot28.cs) through ScriptedInput, ORGANIC kills only
// (no HP cut anywhere): it chases along the boss's flee direction, dodges the shard trail, and kills with three PULSE blasts. RED lines (preconditions printed with them): the row off gives no fight; a
// pilot that STANDS STILL never wins; a pilot that never presses LT never wins; the pure gap clamp, the hit rule and the flee-direction hysteresis cases.
// DISPATCH (the lead's Tools/SimHarness/Program.cs, ONE line next to N27Boss(cmd, args, ref code);): N28Boss(cmd, args, ref code);
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void N28Boss(string cmd, string[] args, ref int code)
    {
        if (cmd != "boss28") return;
        int seeds = 10, first = 1;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--seeds") int.TryParse(args[i + 1], out seeds);
            if (args[i] == "--first") int.TryParse(args[i + 1], out first);
        }
        bool dbg = Array.IndexOf(args, "--dbg") >= 0;
        code = Glacier28Command(Config(), seeds, first, dbg);
    }

    sealed class GlacierRow28
    {
        public int clangs, cracks, cuts, melts, meltTicks, cols, shots, seed, steps, deaths, windows, misses, pulseHits, attacks, attackHits, blasts, blocked, flips, shards;
        public bool killed;
        public double hpEnd, gapMin = 1e9, gapMax = -1e9, speedMin = 1e9, speedMax = -1e9, playerSpeedSum, hitAt1, hitAt2, hitAt3;
        public int playSteps, minTellSteps = int.MaxValue;
    }

    static SurvivalSim Glacier28Sim(GameConfig cfg, uint seed)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n27Override = new N27Blocks { pulse27 = true };
        sim.n26Override = new N26Blocks { surfaces = true, level2 = true };
        sim.bosses25Override = true;
        sim.glacier28Override = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        return sim;
    }

    /// <summary>mode 0: the pilot; 1: standing still; 2: chases and dodges but never presses LT (the RED pilot).</summary>
    static GlacierRow28 GlacierRun28(GameConfig cfg, uint seed, int capSteps, int mode, bool dbg)
    {
        double dt = cfg.Dt;
        SurvivalSim sim = Glacier28Sim(cfg, seed);
        var row = new GlacierRow28 { seed = (int)seed };
        if (!sim.Glacier28Begin()) { row.steps = -1; return row; }
        sim.Respawn(sim.Glacier28Spawn, sim.Glacier28SpawnYawDeg);
        Glacier28 g = sim.glacier28;
        if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
        var si = (ScriptedInput)sim.input;
        var pilot = new GlacierPilot28 { noLaunch = mode == 2, hand = new Mulberry32(seed * 7919u + 13u) };
        double yawL = sim.Glacier28SpawnYawDeg * Math.PI / 180;
        int step = 0, lastHits = 0, lastFlips = 0; double prevPS = 0;
        while (g.Alive && step < capSteps)
        {
            if (mode != 1)
            {
                pilot.Step(sim, yawL, step);
                yawL = pilot.Move.yaw;
                si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
                si.Set(pilot.Move, pilot.Combat);
            }
            else si.Set(new InputFrame { yaw = yawL }, new CombatInput { weapon = -1 });
            sim.Step();
            step++;
            if (g.phase.StartsWith("p"))
            {
                row.gapMin = Math.Min(row.gapMin, g.gap);
                row.gapMax = Math.Max(row.gapMax, g.gap);
                row.speedMin = Math.Min(row.speedMin, g.CurrentSpeed);
                row.speedMax = Math.Max(row.speedMax, g.CurrentSpeed);
                Vec3 v = sim.mover.velocity;
                row.playerSpeedSum += Math.Sqrt(v.x * v.x + v.z * v.z);
                row.playSteps++;
            }
            if (g.hits != lastHits) { lastHits = g.hits; if (g.hits == 1) row.hitAt1 = step * dt; else if (g.hits == 2) row.hitAt2 = step * dt; else row.hitAt3 = step * dt; }
            if (dbg && mode == 0 && g.stats["flips"] != lastFlips) { lastFlips = g.stats["flips"]; if (lastFlips < 40) Console.WriteLine($"      flip {lastFlips} t={step * dt:0.00} {g.phase} dir {g.dir} gap {g.gap:0.0} signed {g.gapSigned:0.0} playerV ({sim.mover.velocity.x:0.0},{sim.mover.velocity.z:0.0}) lat {g.playerLat:0.0}"); }
            if (dbg && mode == 0 && step % 240 == 0) { double ds = g.rink.SignedGap(prevPS, g.playerS) / (240 * dt); prevPS = g.playerS; Console.WriteLine($"    sRate {ds:0.0}"); }
            if (dbg && mode == 0 && step % 240 == 0) Console.WriteLine($"    t={step * dt:0}s {g.phase} hits {g.hits} gap {g.gap:0.0} dir {g.dir} bossV {g.CurrentSpeed:0.0} playerV {Math.Sqrt(sim.mover.velocity.x * sim.mover.velocity.x + sim.mover.velocity.z * sim.mover.velocity.z):0.0} lat {g.playerLat:0.0} win {g.WindowOpen} windows {g.stats["windows"]} misses {g.stats["misses"]} pips {sim.pulse27?.pips} cd {sim.pulse27?.cooldownSteps} blasts {pilot.blasts} hp {sim.health.hp:0} {sim.mover.state}");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                row.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.Glacier28Begin();
                sim.Respawn(sim.Glacier28Spawn, sim.Glacier28SpawnYawDeg);
            }
        }
        row.steps = step;
        row.killed = !g.Alive;
        row.hpEnd = g.HpFrac;
        row.windows = g.stats["windows"]; row.misses = g.stats["misses"]; row.pulseHits = g.stats["pulseHits"]; row.flips = g.stats["flips"]; row.shards = g.stats["shards"]; row.clangs = g.stats["clangs"]; row.cracks = g.stats["cracks"]; row.cuts = g.stats["cuts"]; row.melts = g.stats["melts"]; row.meltTicks = g.stats["meltTicks"]; row.cols = g.stats["columns"] + g.stats["rollers"] + g.stats["bars"] + g.stats["chasms"]; row.shots = pilot.shots;
        row.attacks = g.stats["attacks"]; row.attackHits = g.stats["attackHits"]; row.blasts = pilot.blasts; row.blocked = g.stats["blocked"];
        foreach (Glacier28Log l in g.log) if (!l.resolve) row.minTellSteps = Math.Min(row.minTellSteps, MovementMath.StepsFor(l.seconds, dt));
        return row;
    }

    static int Glacier28Command(GameConfig cfg, int seeds, int first, bool dbg)
    {
        double dt = cfg.Dt;
        var fails = new List<string>();
        Glacier28Params gp = Glacier28Params.From(cfg);
        {
            SurvivalSim sim = Glacier28Sim(cfg, 3);
            sim.glacier28Override = false;
            bool began = sim.Glacier28Begin();
            Console.WriteLine($"  RED glacier28 row OFF (override false): Glacier28Begin {(began ? "STARTS A FIGHT (wrong)" : "gives no fight")}; design default enabled = {cfg.Design("n28.glacier.enabled")} (the plan's boss 'glacier' stays NIGHT27's)");
            if (began) fails.Add("the row off starts the lap fight");
        }
        {
            double lim = gp.clampFrac * gp.slideFloor, b = gp.baseSpeed;
            double lo = Glacier28.SpeedAt(gp, 0.01), hi = Glacier28.SpeedAt(gp, 1000), mid = Glacier28.SpeedAt(gp, gp.targetGap), spent = Glacier28.SpeedAt(gp, gp.spentTarget, false);
            bool exact = Math.Abs(lo - (b - lim)) < 1e-9 && Math.Abs(hi - (b + lim)) < 1e-9 && Math.Abs(mid - b) < 1e-9 && Math.Abs(spent - b) < 1e-9;
            Console.WriteLine($"  gap controller: speed at gap 0.01 / target {gp.targetGap:0} / +1000 m = {lo:0.000} / {mid:0.000} / {hi:0.000} m/s (glove spent, at its target gap: {spent:0.000}); base {b:0.0}, clamp +-{lim:0.000} (15 % of the slide floor {gp.slideFloor:0.0}): {(exact ? "EXACT" : "WRONG")}");
            if (!exact) fails.Add("the gap controller's clamp is wrong");
        }
        {
            // the flee direction's hysteresis: the other arc must be shorter by more than `hysteresis` m
            var r = new Rink28Math(gp.L, gp.rc, 0, 0);
            double lap = r.Lap, hy = gp.hysteresis;
            int d = 1;
            Glacier28.Arcs(r, hy, 0, 44, ref d, out double a1); // plus 44, minus lap-44 = 40.1: shorter by 3.9 < 8: stays
            bool stay1 = d == 1;
            Glacier28.Arcs(r, hy, 0, 47, ref d, out double a2); // minus 37.1 + 8 < 47: flips
            bool flip = d == -1;
            Glacier28.Arcs(r, hy, 0, 44, ref d, out double a3); // dir -1, plus 44, minus 40.1: stays -1 (no oscillation)
            bool stay2 = d == -1;
            Glacier28.Arcs(r, hy, 0, 36, ref d, out double a4); // plus 36 + 8 < minus 48.1: flips back
            bool back = d == 1;
            Glacier28.Arcs(r, hy, 0, 5, ref d, out double a5); // boss just ahead
            bool ok = stay1 && flip && stay2 && back && Math.Abs(a5 - 5) < 1e-9 && Math.Abs(a2 - (lap - 47)) < 1e-9;
            Console.WriteLine($"  flee-direction hysteresis (lap {lap:0.0} m, {hy:0} m): boss at 44 m stays {(stay1 ? "ccw" : "FLIPPED")}, at 47 m {(flip ? "flips cw" : "STAYS")}, back at 44 m {(stay2 ? "stays cw" : "FLIPPED")}, at 36 m {(back ? "flips ccw" : "STAYS")}: {(ok ? "OK" : "WRONG")}");
            if (!ok) fails.Add("the flee-direction hysteresis is wrong");
        }
        {
            SurvivalSim sim = Glacier28Sim(cfg, 3);
            sim.Glacier28Begin();
            Glacier28 g = sim.glacier28;
            g.phase = "p1";
            g.Filter(25, "revolver", default, out bool b1);
            g.Filter(32, "gauntlet", default, out bool b2);
            Console.WriteLine($"  hit rule (no window open): pistol {(b1 ? "blocked" : "LANDED")}, gauntlet {(b2 ? "blocked" : "LANDED")}");
            if (!b1 || !b2) fails.Add("a gun / a hit outside the window landed");
        }
        {
            // the glaze: 8 plain pistol hits crack it (7 do not), one charged shot (4x damage) cracks it, a shotgun hit within 10 m cracks it (at 15 m it does not); a clang costs gap, never health
            SurvivalSim sim = Glacier28Sim(cfg, 3);
            sim.Glacier28Begin();
            sim.Respawn(sim.Glacier28Spawn, sim.Glacier28SpawnYawDeg);
            Glacier28 g = sim.glacier28;
            var si0 = (ScriptedInput)sim.input;
            for (int i = 0; i < 20 * 120 && g.phase != "p1"; i++) { si0.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); }
            Vec3 pp = sim.mover.position;
            double dmg = cfg.Num("weapons.revolver.damage");
            for (int i = 0; i < 7; i++) g.Filter(dmg, "revolver", pp, out _);
            bool after7 = g.glazed;
            g.Filter(dmg, "revolver", pp, out bool bl);
            bool after8 = g.glazed;
            g.glazed = true;
            g.Filter(dmg * 4, "revolver", pp, out _);
            bool charged = !g.glazed;
            g.glazed = true;
            Vec3 far = new Vec3(g.box.c.x + 15, 0, g.box.c.z), nearP = new Vec3(g.box.c.x + 5, 0, g.box.c.z);
            g.Filter(8, "shotgun", far, out _);
            bool farNo = g.glazed;
            g.Filter(8, "shotgun", nearP, out _);
            bool nearYes = !g.glazed;
            bool ok = after7 && !after8 && bl && charged && farNo && nearYes;
            Console.WriteLine($"  glaze: 7 pistol hits {(after7 ? "still glazed" : "CRACKED")}, 8 {(after8 ? "STILL GLAZED" : "cracked")} (never lands: {(bl ? "blocked" : "LANDED")}), one charged shot {(charged ? "cracks" : "DOES NOT")}, shotgun at 15 m {(farNo ? "no" : "CRACKS")}, at 5 m {(nearYes ? "cracks" : "NO")}: {(ok ? "OK" : "WRONG")}");
            if (!ok) fails.Add("the glaze's crack rule is wrong");
            // a blast that reaches a glazed boss clangs: no hit, the boss runs on, health untouched
            g.glazed = true;
            Vec3 bc = g.box.c;
            double hp0 = g.HpFrac;
            int h0 = g.hits;
            bool res = g.LaunchLanded(new Vec3(bc.x - 4, 0, bc.z), new Vec3(1, 0.25, 0));
            Console.WriteLine($"  clang: a blast on the glazed boss {(res && g.stats["clangs"] == 1 ? "clangs" : "DOES NOT CLANG")}; hits {h0} -> {g.hits}, hp {hp0:0.00} -> {g.HpFrac:0.00}");
            if (!(res && g.stats["clangs"] == 1 && g.hits == h0 && Math.Abs(g.HpFrac - hp0) < 1e-9)) fails.Add("the clang rule is wrong");
        }
        {
            // the island door cut: go through the door, come out ahead of the boss: it reverses (a skid) and one pulse hit lands through the glaze; a second cut in the same phase does not
            SurvivalSim sim = Glacier28Sim(cfg, 5);
            sim.Glacier28Begin();
            sim.Respawn(sim.Glacier28Spawn, sim.Glacier28SpawnYawDeg);
            Glacier28 g = sim.glacier28;
            var si1 = (ScriptedInput)sim.input;
            for (int i = 0; i < 20 * 120 && g.phase != "p1"; i++) { si1.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); }
            if (!sim.loadout.owned.Contains("gauntlet")) sim.loadout.AddWeapon("gauntlet");
            int cutsDone = 0, hitsFromCut = 0;
            bool skid = false, glazedThrough = false;
            for (int round = 0; round < 2; round++)
            {
                double ox = g.origin.x, oz = g.origin.z;
                sim.Respawn(new Vec3(ox, 0, oz), 0); // inside the door
                for (int i = 0; i < 3; i++) { si1.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); }
                g.rink.PointAt(g.BossS + g.dir * 3, 0, out double px, out double pz, out _, out _); // 3 m past the boss (ahead of it along the way it runs)
                sim.Respawn(new Vec3(px, 0, pz), 0);
                int pre = g.stats["cuts"];
                for (int i = 0; i < 4; i++) { si1.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); }
                if (g.stats["cuts"] > pre) { cutsDone++; skid |= g.Skidding; }
                glazedThrough = g.glazed;
                int ph0 = g.hits;
                Vec3 mp = sim.mover.position, bc = g.box.c;
                Vec3 aimv = new Vec3(bc.x - mp.x, bc.y - (mp.y + 1.0), bc.z - mp.z);
                if (g.WindowOpen && aimv.Length < g.p.reach) { sim.pulse27?.AddKill(); g.LaunchLanded(mp, aimv); }
                if (g.hits > ph0) hitsFromCut++;
                // let the beat go and the glaze/cut state be seen again within the same phase: the second round reuses p1 only if no hit landed
                if (g.hits > 0) break;
            }
            Console.WriteLine($"  cut: through the door and out ahead of the boss: {cutsDone} cut(s), skid {(skid ? "yes" : "NO")}, glazed at the hit {(glazedThrough ? "yes" : "NO")}, pulse hit through the glaze {(hitsFromCut == 1 ? "landed once" : hitsFromCut + " (WRONG)")}");
            if (cutsDone < 1 || !skid || hitsFromCut != 1) fails.Add("the island door cut is wrong");
            // once per phase: the used cut does not count again
            bool again = g.CutOpen;
            Console.WriteLine($"  cut: after its hit the same phase's cut is {(again ? "STILL OPEN (wrong)" : "used")}");
            if (again) fails.Add("a second cut counted in the same phase");
        }
        {
            GlacierRow28 r = GlacierRun28(cfg, 3, 150 * 120, 1, dbg);
            Console.WriteLine($"  RED standing-still pilot (precondition: row on, 150 s cap): {(r.killed ? "KILLED (wrong)" : "NOT killed")}; pulse hits {r.pulseHits}, windows {r.windows}, gap {r.gapMin:0}..{r.gapMax:0} m");
            if (r.killed || r.pulseHits != 0) fails.Add("a standing pilot hurt / killed the glacier");
            GlacierRow28 n = GlacierRun28(cfg, 3, 150 * 120, 2, dbg);
            Console.WriteLine($"  RED chaser that never presses LT (precondition: row on, gauntlet in hand, glove banked, 150 s cap): {(n.killed ? "KILLED (wrong)" : "NOT killed")}; windows {n.windows}, misses {n.misses} (each costs {gp.missCost:0} m of gap), pulse hits {n.pulseHits}, deaths {n.deaths}, gap {n.gapMin:0}..{n.gapMax:0} m");
            if (n.killed || n.pulseHits != 0) fails.Add("a chaser with no blast hurt / killed the glacier");
        }
        double sum = 0;
        int killed = 0, minTell = int.MaxValue;
        var times = new List<double>();
        for (uint sd = (uint)first; sd < (uint)first + seeds; sd++)
        {
            GlacierRow28 r = GlacierRun28(cfg, sd, 220 * 120, 0, dbg);
            if (r.steps < 0) { fails.Add($"seed {sd}: Glacier28Begin failed"); continue; }
            double pv = r.playSteps > 0 ? r.playerSpeedSum / r.playSteps : 0;
            Console.WriteLine($"  GREEN glacier28 seed {sd}: {(r.killed ? "KILLED" : "NOT KILLED (hits " + r.pulseHits + ")")} at {r.steps * dt:0.0} s; hits at {r.hitAt1:0} / {r.hitAt2:0} / {r.hitAt3:0} s; {r.pulseHits} of {r.windows} windows ({r.misses} missed), blasts {r.blasts}; deaths {r.deaths}; cracks {r.cracks}, clangs {r.clangs}, shots {r.shots}, melts {r.melts} ({r.meltTicks} ticks), P3 attacks {r.cols}; shards {r.shards} placed, {r.attacks} trails stood, {r.attackHits} hit; reversals {r.flips}; gap {r.gapMin:0}..{r.gapMax:0} m; boss {r.speedMin:0.0}..{r.speedMax:0.0} m/s, player mean {pv:0.0} m/s");
            if (r.killed) { killed++; sum += r.steps * dt; times.Add(r.steps * dt); if (r.pulseHits != gp.hitsToKill) fails.Add($"seed {sd}: killed with {r.pulseHits} hits (an HP cut?)"); }
            else fails.Add($"seed {sd} not killed");
            minTell = Math.Min(minTell, r.minTellSteps);
        }
        times.Sort();
        double mean = killed > 0 ? sum / killed : 0, med = times.Count > 0 ? times[times.Count / 2] : 0, min = times.Count > 0 ? times[0] : 0, max = times.Count > 0 ? times[times.Count - 1] : 0;
        if (killed > 0 && (med < 45 || med > 150)) fails.Add($"median kill time {med:0} s is outside 45-150 s");
        if (killed > 0 && min < 20) fails.Add($"the shortest kill {min:0} s is under 20 s");
        if (minTell != int.MaxValue && minTell < MovementMath.StepsFor(0.6, dt)) fails.Add("a tell under 0.6 s");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"boss28 glacier {(fails.Count == 0 ? "PASS" : "FAIL")}: {killed} of {seeds} killed organically by PULSE blasts (no HP cut), mean {mean:0} s, median {med:0} s, shortest {min:0} s, longest {max:0} s (target 45-150 s; the full fight is about 135 s), shortest tell {(minTell == int.MaxValue ? 0 : minTell * dt):0.00} s");
        return fails.Count == 0 ? 0 : 1;
    }
}
