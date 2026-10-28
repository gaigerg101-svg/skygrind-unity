// NIGHT25 builder W: `wyrm25 [--tag t]`, the worm's aim pass measured BEFORE (design.n25.wyrmAim off: NIGHT24's worm) and AFTER (on) through the
// COMPOSED sim with the lag-limited human bot (the same bot as `wyrm --composed`, seeds 3-5): kill time, player hits per minute (spit hits + contacts),
// deaths, the attack log's rules (every tell >= 0.5 s, no two attacks inside resolveGap) and the phase-3 overlap count. Exit 0 when, AFTER: all three
// killed, mean kill time inside 3:30-4:30, every tell >= 0.5 s, no two attacks inside 0.5 s, hits per minute above BEFORE's.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class Wyrm25Row
    {
        public int aims, reaims, seed, steps, deaths, hits, resolves, minTellSteps = int.MaxValue, minGapSteps = int.MaxValue, overlaps;
        public bool killed;
        public double Minutes(double dt) => steps * dt / 60;
        public double PerMinute(double dt) => hits / Math.Max(1e-9, Minutes(dt));
    }

    static Wyrm25Row Wyrm25Run(GameConfig cfg, uint seed, bool aim)
    {
        double dt = cfg.Dt;
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        sim.n24Override = new N24Blocks { wyrm = true };
        sim.n25Override = new N25Blocks { wyrmAim = aim };
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        sim.Wyrm24Begin();
        sim.Respawn(sim.Wyrm24Spawn, 0);
        var rig = new WyrmRig(sim);
        var bot = new WyrmBot(true);
        while (sim.wyrm24.Alive && rig.step < 12 * 60 * 120)
        {
            InputFrame f = bot.Next(rig, out bool fire);
            rig.Step(f, fire);
        }
        Wyrm w = sim.wyrm24;
        var row = new Wyrm25Row { aims = w.stats["lungeAims"], reaims = w.stats["lungeReaims"], seed = (int)seed, steps = rig.step, deaths = rig.deaths, killed = !w.Alive, hits = w.stats["shotsHit"] + w.stats["contactsBody"] + w.stats["contactsHead"] + w.stats["contactsLunge"] };
        // the attack log: tells, resolves, the overlap of a spit tell with a lethal attack's own tell window
        int lastRes = -1000000;
        var lastTell = new Dictionary<string, int>();
        var spitTells = new List<int>();
        var windows = new List<(int a, int b)>();
        var tellOf = new Dictionary<string, string> { { "lungeCommit", "lunge" }, { "diveExit", "dive" }, { "squeeze", "squeeze" } };
        foreach (WyrmLog l in w.log)
        {
            if (!l.resolve)
            {
                row.minTellSteps = Math.Min(row.minTellSteps, MovementMath.StepsFor(l.seconds, dt));
                lastTell[l.name] = l.step;
                if (l.name == "spit") spitTells.Add(l.step);
                continue;
            }
            if (l.name != "spit") row.resolves++;
            if (l.name == "spit") continue; // a volley is not a lethal attack: it may share a window (phase 3), never the 0.5 s after / before a lethal resolve (the sim's gate)
            if (lastRes > -1000000) row.minGapSteps = Math.Min(row.minGapSteps, l.step - lastRes);
            lastRes = l.step;
            if (tellOf.TryGetValue(l.name, out string t) && lastTell.TryGetValue(t, out int ts)) windows.Add((ts, l.step));
        }
        foreach (int st in spitTells)
            foreach (var (a, b) in windows)
                if (st >= a && st <= b) { row.overlaps++; break; }
        return row;
    }

    static string Wyrm25Mm(int steps, double dt)
    {
        int sec = (int)Math.Round(steps * dt);
        return $"{sec / 60}:{sec % 60:00}";
    }

    static partial void Wyrm25Command(string[] args, ref int code)
    {
        if (Array.IndexOf(args, "--rival") >= 0) { Rival25Command(args, ref code); return; } // RIVAL PRIME by the generic bot (Harness.N25.Bosses.cs)
        if (Array.IndexOf(args, "--final") >= 0) { Final25Command(args, ref code); return; } // THE ELDER KILN WYRM by the human-lag bot (Harness.N25.Bosses.cs)
        if (Array.IndexOf(args, "--bosses") >= 0) { Boss25Command(args, ref code); return; } // every campaign boss room by a generic bot (Harness.N25.Bosses.cs)
        GameConfig cfg = Config();
        SurvivalSim.Wyrm24Dir = Path.GetFullPath(Path.Combine(Root, ".."));
        double dt = cfg.Dt;
        int minTell = MovementMath.StepsFor(0.5, dt), gapSteps = MovementMath.StepsFor(cfg.Num("design.n24.wyrm.tells.resolveGap"), dt);
        var fails = new List<string>();
        double[] meanMin = new double[2], meanRate = new double[2];
        for (int mode = 0; mode < 2; mode++)
        {
            bool aim = mode == 1;
            double sumSteps = 0, sumHits = 0;
            int killed = 0;
            foreach (uint sd in new uint[] { 3, 4, 5 })
            {
                Wyrm25Row r = Wyrm25Run(cfg, sd, aim);
                if (r.killed) { killed++; sumSteps += r.steps; }
                sumHits += r.hits;
                Console.WriteLine($"  wyrmAim {(aim ? "ON " : "off")} seed {sd}: {(r.killed ? "KILLED" : "NOT KILLED")} at {Wyrm25Mm(r.steps, dt)}; deaths {r.deaths}; player hits {r.hits} = {r.PerMinute(dt):0.0} per minute; attacks {r.resolves}; shortest tell {r.minTellSteps * dt:0.00} s; nearest two lethal attacks {(r.minGapSteps == int.MaxValue ? 0 : r.minGapSteps * dt):0.00} s; phase-3 spit tells inside a lethal tell window {r.overlaps}; lunge locks {r.aims}, midpoint re-aims {r.reaims}");
                if (aim)
                {
                    if (!r.killed) fails.Add($"seed {sd} not killed");
                    if (r.minTellSteps < minTell) fails.Add($"seed {sd}: a tell under 0.5 s ({r.minTellSteps * dt:0.00})");
                    if (r.minGapSteps != int.MaxValue && r.minGapSteps < gapSteps) fails.Add($"seed {sd}: two lethal attacks {r.minGapSteps * dt:0.00} s apart");
                }
            }
            meanMin[mode] = killed > 0 ? sumSteps / killed * dt / 60 : 0;
            meanRate[mode] = sumHits / Math.Max(1e-9, meanMin[mode] * 3);
            Console.WriteLine($"wyrmAim {(aim ? "ON " : "off")}: {killed} of 3 killed, mean {(killed > 0 ? Wyrm25Mm((int)(sumSteps / killed), dt) : "n/a")}, player hits per minute (mean of the three) {meanRate[mode]:0.0}");
        }
        if (meanMin[1] < 3.5 || meanMin[1] > 4.5) fails.Add($"mean kill time {meanMin[1]:0.00} min is outside 3:30-4:30");
        if (!(meanRate[1] > meanRate[0])) fails.Add("hits per minute did not rise");
        foreach (string f in fails) Console.WriteLine("  FAIL: " + f);
        Console.WriteLine($"wyrm25 {(fails.Count == 0 ? "PASS" : "FAIL")}: hits per minute {meanRate[0]:0.0} -> {meanRate[1]:0.0}, kill time {meanMin[0]:0.00} -> {meanMin[1]:0.00} min (target 3:30-4:30)");
        SurvivalSim.Wyrm24Dir = null;
        code = fails.Count == 0 ? 0 : 1;
    }
}
