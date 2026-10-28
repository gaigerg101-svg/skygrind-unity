// NIGHT26 builder B: `hulk [--tag t]` THE SLAG HULK (Core/Enemies/Hulk26.cs, design.n26.bosses.hulk) fought by a lag-limited human-like bot with the PISTOL
// (the sim's default weapon, the revolver) through the COMPOSED sim, seeds 3-5: kill time (contract 60-90 s), deaths (at most 1), the attack log's rules
// (every tell >= stepsFor(0.5 s) and >= the contract's 0.8 s, no two lethal attacks inside 0.5 s), the part split of the hits, determinism (a seed run twice
// ends in the same step and HP) and RED: with the row off (hulk26Override false) BossRoomBegin gives no fight. The bot sees the hulk 0.25 s late for its
// dodges (the tell is 0.8 s), keeps a standoff, strafes, aims at the head (x2) at a limited turn rate, fires only on a ray hit, jumps the rings and leaves the
// charge's locked line after its reaction time.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class HulkRow
    {
        public int seed, steps, deaths, hits, minTellSteps = int.MaxValue, minGapSteps = int.MaxValue, retries;
        public bool killed;
        public double hpAtEnd;
        public string parts = "";
    }

    static HulkRow HulkRun(GameConfig cfg, uint seed, int capSteps)
    {
        double dt = cfg.Dt;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n26Override = new N26Blocks { level1 = true };
        sim.bosses25Override = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var row = new HulkRow { seed = (int)seed };
        if (!sim.BossRoomBegin("hulk", out Vec3 spawn, out double yaw)) { row.steps = -1; return row; }
        sim.Respawn(spawn, yaw);
        Hulk h = sim.hulk26;
        var si = (ScriptedInput)sim.input;
        double yawL = yaw * Math.PI / 180, pitchL = 0;
        double aimErr0 = 0;
        int step = 0, strafeFlip = 0, reacquire = 0, jumpHold = 0, tellSeen = 0;
        bool targetSeen = false;
        var jr = new Mulberry32(seed * 7919u + 13u); // the human's hand: a slow wobble the aim has to fight
        double jy = 0, jp = 0;
        var hist = new Queue<Vec3>(); // where the head was: the human sees it 0.25 s late
        const double AimRate = 3.5;
        int reaction = MovementMath.StepsFor(0.3, dt);
        while (h.Alive && step < capSteps)
        {
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            Vec3 hp = h.FeetWorld;
            // aim: the head (x2); the torso when the head is not open
            hist.Enqueue(h.HeadPosition);
            while (hist.Count > 30) hist.Dequeue();
            jy = jy * 0.97 + (jr.Next() - 0.5) * 0.035;
            jp = jp * 0.97 + (jr.Next() - 0.5) * 0.035;
            Vec3 aim = hist.Peek();
            if (!(h.Raycast(eye, Vec3.Normalize(aim - eye)) >= 0)) aim = new Vec3(hp.x, hp.y + 2.5, hp.z);
            if (jumpHold > 0 || h.state == "slamTell" || h.state == "chargeTell" || h.state == "charge" || h.state == "slamRecover") aimErr0 = 9; else aimErr0 = 0; // a human does not shoot while watching a tell, dodging or while the ring runs
            Vec3 d = aim - eye;
            double wantYaw = JsMath.Atan2(-d.x, -d.z) + jy, wantPitch = JsMath.Atan2(d.y, JsMath.Hypot(d.x, d.z)) + jp;
            double stp = AimRate * dt;
            double dy = wantYaw - yawL;
            while (dy > Math.PI) dy -= 2 * Math.PI;
            while (dy < -Math.PI) dy += 2 * Math.PI;
            double aimErr = Math.Max(Math.Abs(dy), Math.Abs(wantPitch - pitchL));
            yawL += Math.Max(-stp, Math.Min(stp, dy));
            pitchL += Math.Max(-stp, Math.Min(stp, wantPitch - pitchL));
            if (!targetSeen) { targetSeen = true; reacquire = MovementMath.StepsFor(0.3, dt); }
            if (reacquire > 0) reacquire--;
            Vec3 dir = WyrmRig.AimDir(yawL, pitchL);
            bool fire = aimErr0 == 0 && reacquire == 0 && aimErr < 0.02 && h.Raycast(eye, dir) >= 0 && !(sim.world.RayIntersect(eye, dir, out RayHit wh) && wh.distance < h.Raycast(eye, dir));
            // movement: a standoff ring around the hulk, strafing; off a locked charge line; jumping rings
            double dx = me.x - hp.x, dz = me.z - hp.z, dist = JsMath.Hypot(dx, dz);
            if (dist < 1e-6) { dx = 0; dz = 1; dist = 1; }
            double rx = dx / dist, rz = dz / dist, tx = -rz, tz = rx;
            if (step % (120 * 3) == 0) strafeFlip ^= 1;
            double side = strafeFlip == 0 ? 1 : -1;
            const double want = 8.5;
            double mx = rx * Math.Max(-1, Math.Min(1, (want - dist) / 3)) + tx * 0.5 * side, mz = rz * Math.Max(-1, Math.Min(1, (want - dist) / 3)) + tz * 0.5 * side;
            if (h.state == "chargeTell" || h.state == "charge") tellSeen++; else tellSeen = 0;
            if (tellSeen > reaction)
            {
                // perpendicular distance from the locked line; leave it to the near side, hard
                Vec3 cd = h.ChargeDir;
                double ox = me.x - hp.x, oz = me.z - hp.z;
                double along = ox * cd.x + oz * cd.z, perp = ox * cd.z - oz * cd.x;
                if (along > -2 && Math.Abs(perp) < 4.5)
                {
                    double s = perp < 0 ? -1 : 1;
                    mx = cd.z * s * 3;
                    mz = -cd.x * s * 3;
                }
            }
            bool jump = false;
            for (int i = 0; i < h.rings.Count; i++)
            {
                double pd = JsMath.Hypot(me.x - hp.x, me.z - hp.z);
                double gapR = pd - h.rings[i].r;
                if (!h.rings[i].hit && gapR > -0.5 && gapR < 2.8 && sim.mover.grounded) jump = true;
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
            if (jump) { f.jump = new Skygrind.Core.Button { held = true, pressed = true }; jumpHold = 48; }
            else if (jumpHold > 0) { f.jump = new Skygrind.Core.Button { held = true }; jumpHold--; }
            si.SetLook(f.yaw, f.pitch);
            si.Set(f, fire ? new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = true } } : new CombatInput { weapon = -1 });
            int sh0 = h.stats["slamHits"];
            sim.Step();
            step++;
            if (Environment.GetEnvironmentVariable("HULKDBG") != null && h.stats["slamHits"] > sh0) Console.WriteLine($"    slam hit t={step * dt:0.00} feetY {sim.mover.position.y:0.00} grounded {sim.mover.grounded} jumpHold {jumpHold} rings {h.rings.Count} ring0 r {(h.rings.Count > 0 ? h.rings[0].r : 0):0.0} pd {JsMath.Hypot(sim.mover.position.x - h.FeetWorld.x, sim.mover.position.z - h.FeetWorld.z):0.0}");
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
        row.killed = !h.Alive;
        row.hpAtEnd = h.HpFrac;
        row.retries = sim.hulk26Retries;
        row.hits = h.stats["slamHits"] + h.stats["chargeHits"] + h.stats["contacts"];
        row.parts = $"head {h.stats["headHits"]}, back {h.stats["backHits"]}, body {h.stats["bodyHits"]} (stunned {h.stats["stunHits"]}); slams {h.stats["slams"]} (hit {h.stats["slamHits"]}), charges {h.stats["charges"]} (hit {h.stats["chargeHits"]}), contacts {h.stats["contacts"]}, stuns {h.stats["stuns"]}";
        int lastRes = -1000000;
        foreach (HulkLog l in h.log)
        {
            if (!l.resolve) { row.minTellSteps = Math.Min(row.minTellSteps, MovementMath.StepsFor(l.seconds, dt)); continue; }
            if (l.name == "slam2") continue; // the phase-3 second ring is the same slam's follow-up (0.9 s behind), not a new attack
            if (lastRes > -1000000) row.minGapSteps = Math.Min(row.minGapSteps, l.step - lastRes);
            lastRes = l.step;
        }
        return row;
    }

    static partial void HulkCommand(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        double dt = cfg.Dt;
        var fails = new List<string>();
        // RED: the row off gives no fight
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n26Override = N26Blocks.Off;
            sim.bosses25Override = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(3, null, null);
            bool began = sim.BossRoomBegin("hulk", out _, out _);
            Console.WriteLine($"  hulk row OFF: BossRoomBegin {(began ? "STARTED A FIGHT (wrong)" : "gives no fight")}");
            if (began) fails.Add("the row off still fights");
        }
        double sumSteps = 0;
        int killed = 0, minTell = MovementMath.StepsFor(0.5, dt), contractTell = MovementMath.StepsFor(0.8, dt), gap = MovementMath.StepsFor(cfg.Num("design.n26.bosses.hulk.tells.resolveGap"), dt);
        foreach (uint sd in new uint[] { 3, 4, 5 })
        {
            HulkRow r = HulkRun(cfg, sd, 6 * 60 * 120);
            if (r.steps < 0) { fails.Add($"seed {sd}: BossRoomBegin failed"); continue; }
            Console.WriteLine($"  hulk ON seed {sd}: {(r.killed ? "KILLED" : "NOT KILLED (hp " + r.hpAtEnd.ToString("0.00") + ")")} at {r.steps * dt:0.0} s; deaths {r.deaths}; player hits {r.hits}; shortest tell {r.minTellSteps * dt:0.00} s; nearest two lethal attacks {(r.minGapSteps == int.MaxValue ? 0 : r.minGapSteps * dt):0.00} s; {r.parts}");
            if (!r.killed) fails.Add($"seed {sd} not killed");
            else { killed++; sumSteps += r.steps; }
            if (r.deaths > 1) fails.Add($"seed {sd}: {r.deaths} deaths (at most 1)");
            if (r.minTellSteps < contractTell) fails.Add($"seed {sd}: a tell under 0.8 s ({r.minTellSteps * dt:0.00})");
            if (r.minGapSteps != int.MaxValue && r.minGapSteps < gap) fails.Add($"seed {sd}: two lethal attacks {r.minGapSteps * dt:0.00} s apart");
        }
        double mean = killed > 0 ? sumSteps / killed * dt : 0;
        if (killed > 0 && (mean < 60 || mean > 90)) fails.Add($"mean kill time {mean:0.0} s is outside 60-90 s");
        HulkRow a = HulkRun(cfg, 3, 6 * 60 * 120), b = HulkRun(cfg, 3, 6 * 60 * 120);
        bool same = a.steps == b.steps && a.hpAtEnd == b.hpAtEnd && a.deaths == b.deaths;
        Console.WriteLine($"  determinism: seed 3 twice: {a.steps} / {b.steps} steps: {(same ? "EQUAL" : "DIFFERS")}");
        if (!same) fails.Add("not deterministic");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"hulk {(fails.Count == 0 ? "PASS" : "FAIL")}: {killed} of 3 killed with the pistol, mean {mean:0.0} s (target 60-90 s)");
        SurvivalSim.Wyrm24Dir = null;
        code = fails.Count == 0 ? 0 : 1;
    }
}
