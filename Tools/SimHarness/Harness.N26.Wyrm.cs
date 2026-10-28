// NIGHT26 builder B: `wyrm26 [--tag t]`, level 3's worm (design.n26.bosses.wyrm26, Core/Enemies/Wyrm.cs under n26 level3) measured BEFORE (the NIGHT25 worm with its
// aim pass, level3 off) and AFTER (level3 on: proximity aggro, body x0.5, a second spit lane in phase 2, the phase-3 finale, the wall-ride arena) through the COMPOSED
// sim with the lag-limited human bot (WyrmBot human, seeds 3-5): kill time (contract 4:00-5:00 mean), deaths, player hits per minute (spit hits + contacts), the attack
// log (every tell >= 0.5 s, no two lethal attacks inside 0.5 s), the aggros, the second-lane volleys, the finales, the body / head hit split, and the damage a body hit
// does against a head hit (0.5 x versus 2 x). The bot leaves a locked aggro line after its reaction time and, in the finale, runs for the marked gap's angle outside the
// ring. Exit 0 when, AFTER: all killed, mean kill time inside 4:00-5:00, every tell >= 0.5 s, no two lethal attacks inside 0.5 s, aggros > 0, finales > 0.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class Wyrm26Row
    {
        public int seed, steps, deaths, hits, aggros, finales, spits2, headHits, bodyHits, minTellSteps = int.MaxValue, minGapSteps = int.MaxValue, resolves;
        public bool killed;
        public double Minutes(double dt) => steps * dt / 60;
        public double PerMinute(double dt) => hits / Math.Max(1e-9, Minutes(dt));
    }

    static Wyrm26Row Wyrm26Run(GameConfig cfg, uint seed, bool on)
    {
        double dt = cfg.Dt;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n24Override = new N24Blocks { wyrm = true };
        sim.n25Override = new N25Blocks { wyrmAim = true };
        sim.n26Override = new N26Blocks { level3 = on };
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        sim.Wyrm24Begin();
        sim.Respawn(sim.Wyrm24Spawn, 0);
        var rig = new WyrmRig(sim);
        var bot = new WyrmBot(true);
        int react = MovementMath.StepsFor(0.3, dt), telling = 0;
        while (sim.wyrm24.Alive && rig.step < 14 * 60 * 120)
        {
            InputFrame f = bot.Next(rig, out bool fire);
            Wyrm w = sim.wyrm24;
            if (w.AggroTelling) telling++; else telling = 0;
            Vec3 me = sim.mover.position;
            Vec3 want = default;
            bool over = false;
            if (telling > react)
            {
                // off the locked line: sideways, hard
                double ox = me.x - w.HeadPosition.x, oz = me.z - w.HeadPosition.z;
                double perp = ox * w.AggroDirZ - oz * w.AggroDirX;
                double sgn = perp < 0 ? -1 : 1;
                want = new Vec3(w.AggroDirZ * sgn, 0, -w.AggroDirX * sgn);
                over = true;
            }
            else if (w.FinaleGapLocal.HasValue)
            {
                // run for the marked gap's angle and stand outside the ring
                Vec3 g = w.FinaleGapLocal.Value;
                double ang = JsMath.Atan2(g.z, g.x);
                Vec3 goal = new Vec3(sim.wyrm24Origin.x + JsMath.Cos(ang) * 22, me.y, sim.wyrm24Origin.z + JsMath.Sin(ang) * 22);
                double dx = goal.x - me.x, dz = goal.z - me.z, len = JsMath.Hypot(dx, dz);
                if (len > 0.8) { want = new Vec3(dx / len, 0, dz / len); over = true; }
            }
            if (over)
            {
                Vec3 fwd = MovementMath.WishDirection(0, 1, f.yaw), right = MovementMath.WishDirection(1, 0, f.yaw);
                f.moveZ = want.x * fwd.x + want.z * fwd.z;
                f.moveX = want.x * right.x + want.z * right.z;
            }
            rig.Step(f, fire);
        }
        Wyrm ww = sim.wyrm24;
        var row = new Wyrm26Row
        {
            seed = (int)seed, steps = rig.step, deaths = rig.deaths, killed = !ww.Alive,
            hits = ww.stats["shotsHit"] + ww.stats["contactsBody"] + ww.stats["contactsHead"] + ww.stats["contactsLunge"],
            aggros = ww.stats["aggros"], finales = ww.stats["finales"], spits2 = ww.stats["spits2"], headHits = ww.stats["headHits"], bodyHits = ww.stats["bodyHits"],
        };
        int lastRes = -1000000;
        foreach (WyrmLog l in ww.log)
        {
            if (!l.resolve) { row.minTellSteps = Math.Min(row.minTellSteps, MovementMath.StepsFor(l.seconds, dt)); continue; }
            if (l.name == "spit" || l.name == "spit2") continue; // a volley is not a lethal attack
            row.resolves++;
            if (lastRes > -1000000) row.minGapSteps = Math.Min(row.minGapSteps, l.step - lastRes);
            lastRes = l.step;
        }
        return row;
    }

    static partial void Wyrm26Command(string[] args, ref int code)
    {
        GameConfig cfg = Config();
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        double dt = cfg.Dt;
        int minTell = MovementMath.StepsFor(0.5, dt), gapSteps = MovementMath.StepsFor(cfg.Num("design.n24.wyrm.tells.resolveGap"), dt);
        var fails = new List<string>();
        double[] meanMin = new double[2], meanRate = new double[2];
        for (int mode = 0; mode < 2; mode++)
        {
            bool on = mode == 1;
            double sumSteps = 0, sumHits = 0;
            int killed = 0;
            foreach (uint sd in new uint[] { 3, 4, 5 })
            {
                Wyrm26Row r = Wyrm26Run(cfg, sd, on);
                if (r.killed) { killed++; sumSteps += r.steps; }
                sumHits += r.hits;
                Console.WriteLine($"  level3 {(on ? "ON " : "off")} seed {sd}: {(r.killed ? "KILLED" : "NOT KILLED")} at {Wyrm25Mm(r.steps, dt)}; deaths {r.deaths}; player hits {r.hits} = {r.PerMinute(dt):0.0} per minute; head hits {r.headHits}, body hits {r.bodyHits}; aggros {r.aggros}, lane-2 volleys {r.spits2}, finales {r.finales}; shortest tell {r.minTellSteps * dt:0.00} s; nearest two lethal attacks {(r.minGapSteps == int.MaxValue ? 0 : r.minGapSteps * dt):0.00} s");
                if (on)
                {
                    if (!r.killed) fails.Add($"seed {sd} not killed");
                    if (r.minTellSteps < minTell) fails.Add($"seed {sd}: a tell under 0.5 s ({r.minTellSteps * dt:0.00})");
                    if (r.minGapSteps != int.MaxValue && r.minGapSteps < gapSteps) fails.Add($"seed {sd}: two lethal attacks {r.minGapSteps * dt:0.00} s apart");
                    if (r.aggros == 0) fails.Add($"seed {sd}: no aggro");
                    if (r.finales == 0) fails.Add($"seed {sd}: no finale");
                }
            }
            meanMin[mode] = killed > 0 ? sumSteps / killed * dt / 60 : 0;
            meanRate[mode] = sumHits / Math.Max(1e-9, meanMin[mode] * 3);
            Console.WriteLine($"level3 {(on ? "ON " : "off")}: {killed} of 3 killed, mean {(killed > 0 ? Wyrm25Mm((int)(sumSteps / killed), dt) : "n/a")}, player hits per minute {meanRate[mode]:0.0}");
        }
        double bodyDmg = 25 * cfg.Num("design.n24.wyrm.hp.bodyMult") * cfg.Num("design.n26.bosses.wyrm26.body.mult"), headDmg = 25 * cfg.Num("design.n24.wyrm.hp.headMult");
        Console.WriteLine($"  a pistol hit (25): body {bodyDmg:0.0}, head {headDmg:0.0} (ratio {headDmg / bodyDmg:0.0}x)");
        if (meanMin[1] < 4.0 || meanMin[1] > 5.0) fails.Add($"mean kill time {meanMin[1]:0.00} min is outside 4:00-5:00");
        if (!(meanRate[1] > meanRate[0])) fails.Add("hits per minute did not rise");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"wyrm26 {(fails.Count == 0 ? "PASS" : "FAIL")}: hits per minute {meanRate[0]:0.0} -> {meanRate[1]:0.0}, kill time {meanMin[0]:0.00} -> {meanMin[1]:0.00} min (target 4:00-5:00)");
        SurvivalSim.Wyrm24Dir = null;
        code = fails.Count == 0 ? 0 : 1;
    }
}
