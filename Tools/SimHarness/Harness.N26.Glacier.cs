// NIGHT26 builder B: `glacier [--tag t]` THE GLACIER (Core/Enemies/Glacier26.cs, design.n26.bosses.glacier, level 2) chased by a headless goal-provider bot (the pistol only: the sim's
// default weapon) through the COMPOSED sim on real ice (the room's own Surfaces26: design.n26.surfaces numbers), seeds 3-5: the bot sprints down the course after the boss, matches
// its lane, shoots when the gap is inside the hurt line and the ray finds the hull, jumps the sweeping bar, slides the low bar and steers into the spike wall's open lane. Prints per
// seed: kill time (target 2:00-3:30 mean), deaths, soft fails, attacks, hits, the gap range, the nearest tell and the nearest two lethal attacks; RED lines: the row off gives no
// fight, and a shot at gap > 25 m is blocked (the boss cannot be hurt far away); the rubber band's direction (the boss's speed at gap 80 is >= at gap 20, never below base);
// determinism (seed 3 twice).
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class GlacierRow
    {
        public int seed, steps, deaths, softFails, attacks, attackHits, tilesSeen, minTellSteps = int.MaxValue, minGapSteps = int.MaxValue, hullHits, engineHits;
        public bool killed;
        public double minGap = 1e9, maxGap = -1e9, hpEnd, distEnd;
    }

    static GlacierRow GlacierRun(GameConfig cfg, uint seed, int capSteps, bool dbg = false)
    {
        double dt = cfg.Dt;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n26Override = new N26Blocks { level2 = true, surfaces = true };
        sim.bosses25Override = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var row = new GlacierRow { seed = (int)seed };
        if (!sim.BossRoomBegin("glacier", out Vec3 spawn, out double yaw)) { row.steps = -1; return row; }
        sim.Respawn(spawn, yaw);
        Glacier g = sim.glacier26;
        var si = (ScriptedInput)sim.input;
        double yawL = 0, pitchL = 0;
        double? laneGoalX = null;
        int step = 0, slideHold = 0, jumpHold = 0, lastLaneJump = -100;
        var seen = new HashSet<GlacierObstacle>();
        int first = -1;
        while (g.Alive && step < capSteps)
        {
            Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
            Vec3 bw = g.BossWorld;
            Vec3 aim = g.boxes[1].c; // the engine block on its back (x2)
            double aimYaw = JsMath.Atan2(-(aim.x - eye.x), -(aim.z - eye.z)), aimPitch = JsMath.Atan2(aim.y - eye.y, JsMath.Hypot(aim.x - eye.x, aim.z - eye.z));
            double stp = 6.0 * dt, dy = aimYaw - yawL;
            while (dy > Math.PI) dy -= 2 * Math.PI;
            while (dy < -Math.PI) dy += 2 * Math.PI;
            yawL += Math.Max(-stp, Math.Min(stp, dy));
            pitchL += Math.Max(-stp, Math.Min(stp, aimPitch - pitchL));
            Vec3 dir = WyrmRig.AimDir(yawL, pitchL);
            double rc = g.Raycast(eye, dir);
            bool fire = g.gap <= g.p.gapHurt + 1 && rc >= 0 && Math.Abs(dy) < 0.04 && !(sim.world.RayIntersect(eye, dir, out RayHit wh) && wh.distance < rc);
            // steer: the boss's lane x; dodge what is coming
            double targetX = laneGoalX ?? sim.glacier26Origin.x; // the middle lane: the bot does not chase the boss's lane changes (a rail is a jump)
            bool jump = false, slide = false;
            double vz = Math.Max(2, -sim.mover.velocity.z);
            foreach (GlacierObstacle o in g.obstacles)
            {
                double dist = me.z - o.z; // m ahead (the course runs toward -z)
                if (dist < -2.5) continue;
                double tta = dist / vz;
                if (o.kind == "spikes")
                {
                    var (mn, mx) = g.OpenRange;
                    targetX = (mn + mx) * 0.5;
                    if (o.tellLeft <= 0 || o.tellLeft < o.total - 20) laneGoalX = targetX; // stay in the open lane afterwards
                }
                else if (o.kind == "sweep" && o.tellLeft <= 0 && dist < vz * 0.2 + 0.3 && dist > vz * 0.1 && sim.mover.grounded) jump = true;
                else if (o.kind == "low" && o.tellLeft <= 0 && dist < Math.Max(5, vz * 0.6) && dist > -1.5) slide = true;
            }
            bool nearBar = false; foreach (GlacierObstacle ob in g.obstacles) if (ob.kind != "spikes" ? me.z - ob.z > -3 && me.z - ob.z < 9 : me.z - ob.z > -3 && me.z - ob.z < 3) nearBar = true;
            if (!jump && !nearBar && sim.mover.grounded && Math.Abs(targetX - me.x) > 3.2 && step - lastLaneJump > 50) { jump = true; lastLaneJump = step; } // over a rail to change lane
            if (dbg && step < 6000) foreach (GlacierObstacle ob in g.obstacles) if (ob.kind == "sweep" && ob.tellLeft <= 0 && me.z - ob.z < 6 && me.z - ob.z > -2) Console.WriteLine($"      sweep dist {me.z - ob.z:0.0} y {me.y:0.00} grounded {sim.mover.grounded} vz {vz:0.0} jump {jump} hold {jumpHold}");
            if (dbg && step < 3000 && g.obstacles.Exists(q => q.kind == "sweep" && q.tellLeft <= 0 && me.z - q.z < 5 && me.z - q.z > -2)) Console.WriteLine($"      SW t={step} dist {me.z - g.obstacles.Find(q => q.kind == "sweep").z:0.0} y {me.y:0.00} gr {sim.mover.grounded} jump {jump} hold {jumpHold} vz {vz:0.0}");
            if (jump) jumpHold = 14;
            if (slide) slideHold = 24;
            var f = new InputFrame { yaw = yawL, pitch = pitchL };
            double wx = Math.Max(-1, Math.Min(1, (targetX - me.x) * 0.8 - sim.mover.velocity.x * 0.25)), wz = g.gap < 6 ? 0.2 : g.gap < 14 ? -0.35 : -1; // hold station behind it: not past it
            Vec3 fwd = MovementMath.WishDirection(0, 1, yawL), right = MovementMath.WishDirection(1, 0, yawL);
            double ml = JsMath.Hypot(wx, wz);
            wx /= ml; wz /= ml;
            f.moveZ = wx * fwd.x + wz * fwd.z;
            f.moveX = wx * right.x + wz * right.z;
            f.sprint = new Skygrind.Core.Button { held = true, pressed = step % 60 == 0 }; // the sprint key re-pressed once a half second (a respawn drops it)
            if (jumpHold > 0) { f.jump = new Skygrind.Core.Button { held = true, pressed = jumpHold == 14 }; jumpHold--; }
            if (slideHold > 0) { f.slide = new Skygrind.Core.Button { held = true, pressed = slideHold == 24 }; slideHold--; }
            si.SetLook(f.yaw, f.pitch);
            si.Set(f, fire ? new CombatInput { weapon = -1, fire = new Skygrind.Core.Button { held = true, pressed = true } } : new CombatInput { weapon = -1 });
            int ah0 = g.stats["attackHits"];
            sim.Step();
            step++;
            if (dbg && g.stats["attackHits"] > ah0) { var ob0 = g.obstacles.Find(q => q.hit); Console.WriteLine($"    HIT {ob0?.kind} t={step * dt:0.0} x {me.x - sim.glacier26Origin.x:0.0} y {me.y:0.00} grounded {sim.mover.grounded} crouched {sim.mover.crouched} vz {vz:0.0} slideHold {slideHold} jumpHold {jumpHold} open {g.OpenRange.min - sim.glacier26Origin.x:0.0}..{g.OpenRange.max - sim.glacier26Origin.x:0.0}"); }
            if (g.phase.StartsWith("p")) { row.minGap = Math.Min(row.minGap, g.gap); row.maxGap = Math.Max(row.maxGap, g.gap); }
            if (dbg && step % 600 == 0 && step < 12000) Console.WriteLine($"    hp {sim.health.hp:0} x {me.x - sim.glacier26Origin.x:0.0} t={step * dt:0}s phase {g.phase} hp {g.HpFrac:0.00} gap {g.gap:0.0} playerS {g.playerS:0} bossS {g.BossS:0} speed {-sim.mover.velocity.z:0.0} bossV {g.CurrentSpeed:0.0}");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                row.deaths++;
                if (dbg) Console.WriteLine($"    DEATH t={step * dt:0.0} x {me.x:0.0} y {me.y:0.0} gap {g.gap:0.0} obstacles [{string.Join(",", g.obstacles.ConvertAll(o => o.kind + (o.Live ? "!" : "?") + (me.z - o.z).ToString("0")))}] hp {sim.health.hp}");
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
        row.softFails = g.stats["softFails"];
        row.attacks = g.stats["attacks"];
        row.attackHits = g.stats["attackHits"];
        row.hullHits = g.stats["hullHits"];
        row.engineHits = g.stats["engineHits"];
        int lastRes = -1000000;
        foreach (GlacierLog l in g.log)
        {
            if (!l.resolve) { row.minTellSteps = Math.Min(row.minTellSteps, MovementMath.StepsFor(l.seconds, dt)); continue; }
            if (lastRes > -1000000) row.minGapSteps = Math.Min(row.minGapSteps, l.step - lastRes);
            lastRes = l.step;
        }
        return row;
    }

    static partial void GlacierCommand(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        double dt = cfg.Dt;
        var fails = new List<string>();
        bool dbg = Array.IndexOf(args, "--dbg") >= 0;
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n26Override = N26Blocks.Off;
            sim.bosses25Override = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(3, null, null);
            bool began = sim.BossRoomBegin("glacier", out _, out _);
            Console.WriteLine($"  glacier row OFF: BossRoomBegin {(began ? "STARTED A FIGHT (wrong)" : "gives no fight")}");
            if (began) fails.Add("the row off still fights");
        }
        {
            // the rubber band's direction and the hurt line, on a fresh fight
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n26Override = new N26Blocks { level2 = true, surfaces = true };
            sim.bosses25Override = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(3, null, null);
            sim.BossRoomBegin("glacier", out Vec3 sp, out double yw);
            sim.Respawn(sp, yw);
            Glacier g = sim.glacier26;
            double baseV = g.p.baseSpeed;
            var gaps = new[] { 10.0, 30.0, 50.0, 70.0, 79.0 };
            double prev = 0;
            bool monotone = true, never = true;
            foreach (double gp in gaps)
            {
                g.gap = gp;
                double v = g.p.baseSpeed * g.RubberMult;
                if (v + 1e-9 < prev) monotone = false;
                if (v + 1e-9 < baseV) never = false;
                prev = v;
            }
            Console.WriteLine($"  rubber band: boss speed at gap 10/30/50/70/79 m = {baseV * 1:0.0}.. up to {prev:0.0} m/s: non-decreasing with the gap {(monotone ? "yes" : "NO")}, never below base {(never ? "yes" : "NO")}; '{g.RubberText}'");
            if (!monotone || !never) fails.Add("the rubber band is not one way");
            // a shot beyond the hurt line is blocked
            sim.glacier26.gap = 40;
            double before = g.body.hp;
            Vec3 eye = new Vec3(0, 1.6, 0);
            g.Raycast(g.boxes[0].c + new Vec3(0, 0, 8) - new Vec3(0, 0, 0), new Vec3(0, 0, -1));
            double landed = g.Filter(25, eye, out bool blocked);
            Console.WriteLine($"  a hit at gap 40 m (hurt line {g.p.gapHurt:0} m): {(blocked ? "blocked, no damage" : "LANDED " + landed)}");
            if (!blocked) fails.Add("a hit beyond the hurt line landed");
        }
        {
            // the pistol's close-range bonus really applies on level 2: the same shot through the game's own damage filter (EnemyHits.Filter), 20 m versus 5 m from the hull
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noLab = sim.noRecords = true;
            sim.n26Override = new N26Blocks { level2 = true, surfaces = true };
            sim.bosses25Override = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(3, null, null);
            sim.BossRoomBegin("glacier", out Vec3 sp, out double yw);
            Glacier g = sim.glacier26;
            g.phase = "p1";
            g.gap = 10;
            double pistol = cfg.Num("weapons.revolver.damage");
            Vec3 hull = g.boxes[0].c;
            double[] dmg = new double[2];
            int n = 0;
            foreach (double d in new[] { 20.0, 5.0 })
            {
                g.Raycast(hull + new Vec3(0, 1.6, d), Vec3.Normalize(new Vec3(0, -1.6 / d, -1)));
                dmg[n++] = EnemyHits.Filter(g.body, pistol, false, "revolver", false, hull + new Vec3(0, 1.6, d), sim.events, out bool bl);
            }
            Console.WriteLine($"  pistol ({pistol:0} dmg) on THE GLACIER: {dmg[0]:0.0} at 20 m, {dmg[1]:0.0} at 5 m (x{dmg[1] / Math.Max(1e-9, dmg[0]):0.00}); the bonus is x1 at {g.p.closeRange:0} m, up to x{g.p.closeMax:0.0} at point blank");
            if (!(dmg[1] > dmg[0] * 1.3)) fails.Add("the close-range bonus does not apply");
        }
        double sumSteps = 0;
        int killed = 0, contractTell = MovementMath.StepsFor(0.6, dt), gap = MovementMath.StepsFor(cfg.Num("design.n26.bosses.glacier.tells.resolveGap"), dt);
        foreach (uint sd in new uint[] { 3, 4, 5 })
        {
            GlacierRow r = GlacierRun(cfg, sd, 8 * 60 * 120, dbg);
            if (r.steps < 0) { fails.Add($"seed {sd}: BossRoomBegin failed"); continue; }
            Console.WriteLine($"  glacier ON seed {sd}: {(r.killed ? "KILLED" : "NOT KILLED (hp " + r.hpEnd.ToString("0.00") + ")")} at {r.steps * dt:0.0} s over {r.distEnd:0} m; deaths {r.deaths}, soft fails {r.softFails}; attacks {r.attacks} (hit {r.attackHits}); hull hits {r.hullHits}, engine hits {r.engineHits}; gap {r.minGap:0}..{r.maxGap:0} m; shortest tell {r.minTellSteps * dt:0.00} s; nearest two lethal attacks {(r.minGapSteps == int.MaxValue ? 0 : r.minGapSteps * dt):0.00} s");
            if (!r.killed) fails.Add($"seed {sd} not killed");
            else { killed++; sumSteps += r.steps; }
            if (r.minTellSteps < contractTell) fails.Add($"seed {sd}: a tell under 0.6 s");
            if (r.minGapSteps != int.MaxValue && r.minGapSteps < gap) fails.Add($"seed {sd}: two lethal attacks {r.minGapSteps * dt:0.00} s apart");
        }
        double mean = killed > 0 ? sumSteps / killed * dt : 0;
        if (killed > 0 && (mean < 120 || mean > 210)) fails.Add($"mean kill time {mean:0} s is outside 2:00-3:30");
        GlacierRow a = GlacierRun(cfg, 3, 8 * 60 * 120), b = GlacierRun(cfg, 3, 8 * 60 * 120);
        bool same = a.steps == b.steps && a.hpEnd == b.hpEnd && a.deaths == b.deaths;
        Console.WriteLine($"  determinism: seed 3 twice: {a.steps} / {b.steps} steps: {(same ? "EQUAL" : "DIFFERS")}");
        if (!same) fails.Add("not deterministic");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"glacier {(fails.Count == 0 ? "PASS" : "FAIL")}: {killed} of 3 killed with the pistol, mean {mean:0} s (target 120-210 s)");
        SurvivalSim.Wyrm24Dir = null;
        code = fails.Count == 0 ? 0 : 1;
    }
}
