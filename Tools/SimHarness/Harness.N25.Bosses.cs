// NIGHT25 builder W: `wyrm25 --bosses [--cap M]`: each campaign boss room (wyrm, bulwark, monolith: SurvivalSim.Boss25.cs, design.n25.bosses) fought by a generic
// human-like bot through the COMPOSED sim (the real Mover, Health and revolver through the sim's input, the room as a world layer): kill time, deaths, retries
// (a death calls BossRoomBegin again: a live boss goes back to the START of its phase, the same stream) and the HP before / after each retry. The bot keeps a
// standoff, strafes, aims at the boss's aim point at a limited turn rate, fires only on a hit and jumps the rings and the laser; it is NOT tuned per boss.
// Exit 0 when every boss dies within the cap (default 10 min of game time) and every retry restored a phase's threshold, not full HP.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class Boss25Result
    {
        public string id;
        public bool killed;
        public int steps, deaths, retriesCounted, retriesChecked, retriesBad;
        public string retryLines = "", extra = "";
        public int minTellSteps = int.MaxValue;
        public double hpMax;
    }

    static Boss25Result Boss25Run(GameConfig cfg, string id, uint seed, int capSteps, string forceAt = null)
    {
        double dt = cfg.Dt;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n24Override = new N24Blocks { wyrm = true };
        sim.n25Override = new N25Blocks { wyrmAim = true };
        sim.bosses25Override = true;
        sim.rival25Override = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var res = new Boss25Result { id = id };
        if (!sim.BossRoomBegin(id, out Vec3 spawn, out double yaw)) { res.retryLines = "BossRoomBegin failed"; return res; }
        sim.Respawn(spawn, yaw);
        IBossSystem boss = sim.BossRoomSystem(id);
        var si = (ScriptedInput)sim.input;
        double yawL = yaw * Math.PI / 180, pitchL = 0;
        int strafeFlip = 0;
        var phaseHp = new Dictionary<string, double>();
        double Frac() => id == "rival" ? sim.rival25.HpFrac : id == "wyrm" ? sim.wyrm24.HpFrac : id == "bulwark" ? sim.bulwark.HpFrac : sim.monolith24.HpFrac;
        string Phase() => id == "rival" ? sim.rival25.phase : id == "wyrm" ? sim.wyrm24.phase : id == "bulwark" ? sim.bulwark.phase : sim.monolith24.phase;
        Enemy Body() => id == "rival" ? sim.rival25.body : id == "wyrm" ? sim.wyrm24.body : id == "bulwark" ? (Enemy)sim.bulwark.body : sim.monolith24.body;
        int step = 0;
        bool forced = false;
        while (boss.Alive && step < capSteps)
        {
            if (forceAt != null && !forced && Phase() == forceAt && step % 120 == 60 && sim.wyrm24.phaseSteps > 120 * 8) { forced = true; sim.health.Damage(1e9, "test"); } // a death in that phase: the rig below retries the room
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            Enemy b = Body();
            Vec3 aim = b.AimPoint;
            if (id == "wyrm")
            {
                Wyrm w = sim.wyrm24;
                int pick = 0;
                for (int i = 0; i < w.PieceCount; i++) if (!w.buried[i]) { pick = i; break; }
                aim = w.boxes[pick].c;
            }
            else if (id == "monolith") aim = sim.monolith24.CoreWorld();
            Vec3 d = aim - eye;
            double wantYaw = JsMath.Atan2(-d.x, -d.z), wantPitch = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z));
            double stp = 6.0 * dt;
            double dy = wantYaw - yawL;
            while (dy > Math.PI) dy -= 2 * Math.PI;
            while (dy < -Math.PI) dy += 2 * Math.PI;
            yawL += Math.Max(-stp, Math.Min(stp, dy));
            pitchL += Math.Max(-stp, Math.Min(stp, wantPitch - pitchL));
            Vec3 dir = WyrmRig.AimDir(yawL, pitchL);
            bool fire = b.RayHit(eye, dir, out _) >= 0;
            // standoff and strafe
            Vec3 bp = b.position;
            double dx = me.x - bp.x, dz = me.z - bp.z, dist = JsMath.Hypot(dx, dz);
            if (dist < 1e-6) { dx = 0; dz = 1; dist = 1; }
            double rx = dx / dist, rz = dz / dist, tx = -rz, tz = rx;
            double want = id == "bulwark" ? 15 : id == "monolith" ? 17 : id == "rival" ? 14 : 18;
            if (step % (120 * 3) == 0) strafeFlip ^= 1;
            double side = strafeFlip == 0 ? 1 : -1;
            double mx = rx * Math.Max(-1, Math.Min(1, (want - dist) / 4)) + tx * 0.6 * side, mz = rz * Math.Max(-1, Math.Min(1, (want - dist) / 4)) + tz * 0.6 * side;
            bool jump = false;
            if (id == "monolith")
            {
                Monolith m = sim.monolith24;
                for (int i = 0; i < m.RingCount; i++)
                    if (m.RingLive(i) && Math.Abs(dist - m.RingRadius(i)) < 3.2) jump = true;
                if (m.LaserLive && !m.LaserCharging && Math.Abs(JsMath.Atan2(me.z - bp.z, me.x - bp.x) - m.LaserAngle) < 0.5) jump = true;
                for (int i = 0; i < m.WarnCount; i++)
                {
                    m.Warn(i, out double wx, out _, out double wz, out double wr, out _);
                    if (wr > 5) continue;
                    double ex = me.x - wx, ez = me.z - wz, ed = JsMath.Hypot(ex, ez);
                    if (ed < wr + 2.5) { if (ed < 1e-6) { ex = 1; ez = 0; ed = 1; } mx += ex / ed * 3; mz += ez / ed * 3; }
                }
            }
            else if (id == "bulwark")
            {
                Bulwark bw = sim.bulwark;
                if (bw.RingLive && Math.Abs(JsMath.Hypot(me.x - bw.RingX, me.z - bw.RingZ) - bw.RingRadius) < 3.0) jump = true;
                if (bw.Ramming || bw.RamTelegraph > 0) { mx += tx * 3 * side; mz += tz * 3 * side; } // off the line of a charge
            }
            double ml = JsMath.Hypot(mx, mz);
            var f = new InputFrame { yaw = yawL, pitch = pitchL };
            if (ml > 0.05)
            {
                mx /= Math.Max(1, ml);
                mz /= Math.Max(1, ml);
                Vec3 fwd = MovementMath.WishDirection(0, 1, yawL), right = MovementMath.WishDirection(1, 0, yawL);
                f.moveZ = mx * fwd.x + mz * fwd.z;
                f.moveX = mx * right.x + mz * right.z;
            }
            if (jump) f.jump = new Skygrind.Core.Button { held = true, pressed = true };
            si.SetLook(f.yaw, f.pitch);
            si.Set(f, fire ? new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = true } } : new CombatInput { weapon = -1 });
            sim.Step();
            step++;
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                res.deaths++;
                string ph = Phase();
                double before = Frac();
                // what the rule must restore to: the phase's threshold (RetryPhase), or full HP for the intro and phase 1 (a fresh fight)
                string rp = id == "rival" ? sim.rival25.RetryPhase : id == "wyrm" ? sim.wyrm24.RetryPhase : id == "bulwark" ? sim.bulwark.RetryPhase : sim.monolith24.RetryPhase;
                double expect = rp == null ? 1.0
                    : id == "rival" ? sim.rival25.p.p2At
                    : id == "wyrm" ? (rp == "p3" ? sim.wyrm24.p.phase3At : sim.wyrm24.p.phase2At)
                    : id == "bulwark" ? sim.bulwark.b.p2At
                    : (rp == "p3" ? sim.monolith24.b.p3At : sim.monolith24.b.midAt);
                int r0 = sim.bossRoom25Retries + (id == "wyrm" ? sim.wyrm24Retries : 0);
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                sim.BossRoomBegin(id, out spawn, out yaw);
                sim.Respawn(spawn, yaw);
                double after = Frac();
                res.retriesCounted = sim.bossRoom25Retries + (id == "wyrm" ? sim.wyrm24Retries : 0);
                res.retriesChecked++;
                bool ok = Math.Abs(after - expect) < 1e-6 && (rp == null || res.retriesCounted > r0);
                if (!ok) res.retriesBad++;
                res.retryLines += $"[{step * dt / 60:0.0} min {ph}: HP {before:0.00} -> {after:0.00}] ";
            }
        }
        res.steps = step;
        res.killed = !boss.Alive;
        if (id == "rival")
        {
            RivalPrime rv = sim.rival25;
            foreach (var l in rv.log) if (!l.resolve) res.minTellSteps = Math.Min(res.minTellSteps, MovementMath.StepsFor(l.seconds, dt));
            res.extra = $"dashes {rv.stats["dashes"]} (hit {rv.stats["dashHits"]}), volleys {rv.stats["volleys"]} (orb hits {rv.stats["orbHits"]}), bursts {rv.stats["bursts"]} (shot hits {rv.stats["shotsHit"]})";
        }
        return res;
    }

    /// <summary>`wyrm25 --final`: THE ELDER KILN WYRM (design.n25.bosses.final) by the lag-limited human bot (WyrmBot) through the COMPOSED sim, seeds 3-5, with the
    /// phase-start retry on a death (Wyrm25FinalBegin again): kill time (target 5:00-6:30), deaths, the attack log's rules; and the variant OFF (bosses25Override false) must
    /// give no fight (RED).</summary>
    static void Final25Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        double dt = cfg.Dt;
        var fails = new List<string>();
        {
            SurvivalSim off = SurvivalSim.Headless(cfg, Map());
            off.noLab = off.noRecords = true;
            off.n24Override = new N24Blocks { wyrm = true };
            off.bosses25Override = false;
            off.StartRun(3, null, null);
            bool began = off.Wyrm25FinalBegin();
            Console.WriteLine($"  variant OFF (design.n25.bosses.enabled false): Wyrm25FinalBegin {(began ? "STARTED (FAIL)" : "false: RED, no ELDER WYRM")}");
            if (began) fails.Add("the variant started with the row off");
        }
        int minTell = MovementMath.StepsFor(0.5, dt), gapSteps = MovementMath.StepsFor(cfg.Num("design.n24.wyrm.tells.resolveGap"), dt);
        double sum = 0;
        int killed = 0;
        foreach (uint sd in new uint[] { 3, 4, 5 })
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n24Override = new N24Blocks { wyrm = true };
            sim.n25Override = new N25Blocks { wyrmAim = Array.IndexOf(args, "--noaim") < 0 }; // --noaim: the aimed pass off (diagnostic)
            sim.bosses25Override = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(sd, null, null);
            sim.husks.frozen = true;
            string card = null;
            sim.events.On<BossEvent>("bossIntro", e => card = e.name);
            if (!sim.Wyrm25FinalBegin()) { Console.WriteLine("  final: Wyrm25FinalBegin failed"); fails.Add("begin failed"); continue; }
            Console.WriteLine($"  intro card: {card}"); if (card != "ELDER KILN WYRM") fails.Add($"intro card reads {card}");
            sim.Respawn(sim.Wyrm24Spawn, 0);
            var rig = new WyrmRig(sim);
            var bot = new WyrmBot(true);
            Wyrm w = sim.wyrm24;
            int deaths0 = 0, retried = 0, restoredOk = 0;
            string lines = "";
            while (w.Alive && rig.step < 14 * 60 * 120)
            {
                InputFrame f = bot.Next(rig, out bool fire);
                double before = w.HpFrac;
                string rp = w.RetryPhase;
                rig.Step(f, fire);
                if (rig.deaths > deaths0)
                {
                    deaths0 = rig.deaths;
                    double expect = rp == null ? 1.0 : rp == "p4" ? w.p.phase4At : rp == "p3" ? w.p.phase3At : w.p.phase2At;
                    sim.Wyrm25FinalBegin(); // Campaign24.Retry: a live ELDER WYRM goes back to its phase start
                    sim.Respawn(sim.Wyrm24Spawn, 0);
                    retried++;
                    if (Math.Abs(w.HpFrac - expect) < 1e-6) restoredOk++;
                    lines += $"[{rig.step * dt / 60:0.0} min {rp ?? "p1"}: HP {before:0.00} -> {w.HpFrac:0.00}] ";
                }
            }
            bool k = !w.Alive;
            if (k) { killed++; sum += rig.step * dt; }
            // the attack log's rules
            int lastRes = -1000000, minGap = int.MaxValue, minT = int.MaxValue, resolves = 0;
            foreach (WyrmLog l in w.log)
            {
                if (!l.resolve) { minT = Math.Min(minT, MovementMath.StepsFor(l.seconds, dt)); continue; }
                if (l.name == "spit") continue;
                resolves++;
                if (lastRes > -1000000) minGap = Math.Min(minGap, l.step - lastRes);
                lastRes = l.step;
            }
            Console.WriteLine($"  [contacts body {w.stats["contactsBody"]} head {w.stats["contactsHead"]} lunge {w.stats["contactsLunge"]}, spit hits {w.stats["shotsHit"]}, volleys {w.stats["volleys"]}]"); Console.WriteLine($"  final seed {sd}: {(k ? "KILLED" : "NOT KILLED")} at {Wyrm25Mm(rig.step, dt)}; deaths {rig.deaths}; retries restored {restoredOk}/{retried} {lines}; phases {string.Join(">", rig.phases.ConvertAll(x => x.phase))}; sheds {w.stats["sheds"]} (shot down {w.stats["shedKills"]}); lunge locks {w.stats["lungeAims"]}; lethal attacks {resolves}, shortest tell {(minT == int.MaxValue ? 0 : minT * dt):0.00} s, nearest two lethal {(minGap == int.MaxValue ? 0 : minGap * dt):0.00} s");
            if (!k) fails.Add($"seed {sd} not killed");
            if (restoredOk != retried) fails.Add($"seed {sd}: a retry did not restore a threshold");
            if (minT < minTell) fails.Add($"seed {sd}: a tell under 0.5 s");
            if (minGap != int.MaxValue && minGap < gapSteps) fails.Add($"seed {sd}: two lethal attacks {minGap * dt:0.00} s apart");
        }
        double meanMin = killed > 0 ? sum / killed / 60 : 0;
        Console.WriteLine($"wyrm25 --final: {killed} of 3 killed, mean {(killed > 0 ? Wyrm25Mm((int)(sum / killed / dt), dt) : "n/a")} (human-lag bot; target 5:00-6:30)");
        if (meanMin < 5.0 || meanMin > 6.5) fails.Add($"mean kill time {meanMin:0.00} min is outside 5:00-6:30");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"wyrm25 --final {(fails.Count == 0 ? "PASS" : "FAIL")}");
        SurvivalSim.Wyrm24Dir = null;
        code = fails.Count == 0 ? 0 : 1;
    }

    /// <summary>`wyrm25 --rival`: RIVAL PRIME by the generic bot through the composed sim, seeds 3-5 (a death retries the room: phase 2 restarts at 0.5): kill time (target 2:00-4:00), deaths
    /// (1-3 each), the attack log (every tell >= 0.5 s), and the row OFF must give no fight (RED).</summary>
    static void Rival25Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        double dt = cfg.Dt;
        var fails = new List<string>();
        {
            SurvivalSim off = SurvivalSim.Headless(cfg, Map());
            off.noLab = off.noRecords = true;
            off.bosses25Override = true;
            off.rival25Override = false;
            off.StartRun(3, null, null);
            bool began = off.BossRoomBegin("rival", out _, out _);
            Console.WriteLine($"  rival row OFF (design.n25.bosses.rival.enabled false): BossRoomBegin {(began ? "STARTED (FAIL)" : "false: RED, no duel")}");
            if (began) fails.Add("the rival started with its row off");
        }
        int minTell = MovementMath.StepsFor(0.5, dt);
        double sum = 0, deathSum = 0;
        int killed = 0;
        foreach (uint sd in new uint[] { 3, 4, 5 })
        {
            Boss25Result r = Boss25Run(cfg, "rival", sd, (int)(10 * 60 / dt));
            // re-run is not needed: the log lives on the sim; the tell rule is checked inside Boss25Run's sim through the stats below
            Console.WriteLine($"  rival seed {sd}: {(r.killed ? "KILLED" : "NOT KILLED")} at {Wyrm25Mm(r.steps, dt)}; deaths {r.deaths}; retries restored {r.retriesChecked - r.retriesBad}/{r.retriesChecked} {r.retryLines}; shortest tell {(r.minTellSteps == int.MaxValue ? 0 : r.minTellSteps * dt):0.00} s; {r.extra}");
            if (!r.killed) fails.Add($"seed {sd} not killed");
            else { killed++; sum += r.steps * dt; deathSum += r.deaths; }
            if (r.retriesBad > 0) fails.Add($"seed {sd}: a retry restored the wrong HP");
            if (r.minTellSteps != int.MaxValue && r.minTellSteps < minTell) fails.Add($"seed {sd}: a tell under 0.5 s");
            if (r.deaths < 1 || r.deaths > 3) fails.Add($"seed {sd}: {r.deaths} deaths (target 1-3)");
        }
        double meanMin = killed > 0 ? sum / killed / 60 : 0;
        Console.WriteLine($"wyrm25 --rival: {killed} of 3 killed, mean {(killed > 0 ? Wyrm25Mm((int)(sum / killed / dt), dt) : "n/a")}, mean deaths {(killed > 0 ? deathSum / killed : 0):0.0} (target 2:00-4:00, 1-3 deaths)");
        if (meanMin < 2.0 || meanMin > 4.0) fails.Add($"mean kill time {meanMin:0.00} min is outside 2:00-4:00");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"wyrm25 --rival {(fails.Count == 0 ? "PASS" : "FAIL")}");
        SurvivalSim.Wyrm24Dir = null;
        code = fails.Count == 0 ? 0 : 1;
    }

    static void Boss25Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        double dt = cfg.Dt;
        int cap = (int)(10 * 60 / dt);
        for (int i = 0; i + 1 < args.Length; i++) if (args[i] == "--cap") cap = (int)(double.Parse(args[i + 1]) * 60 / dt);
        var fails = new List<string>();
        foreach (string id in new[] { "wyrm", "bulwark", "monolith" })
        {
            Boss25Result r = Boss25Run(cfg, id, 3, cap, id == "wyrm" ? "p3" : null); // the worm never dies to this bot: force one death in phase 3
            Console.WriteLine($"  boss {id}: {(r.killed ? "KILLED" : "NOT KILLED")} at {Wyrm25Mm(r.steps, dt)}; deaths {r.deaths}; retries counted {r.retriesCounted}, phase-start retries checked {r.retriesChecked} (bad {r.retriesBad}); {r.retryLines}");
            if (!r.killed) fails.Add(id + " not killed in the cap");
            if (r.retriesBad > 0) fails.Add(id + " a retry restored more than a phase start");
        }
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"wyrm25 --bosses {(fails.Count == 0 ? "PASS" : "FAIL")}");
        SurvivalSim.Wyrm24Dir = null;
        code = fails.Count == 0 ? 0 : 1;
    }
}
