// NIGHT16 U3-29.5 (builder I): `impact [map.json]`, the headless freeze ledger. A 300 s AUTOPILOT run (seed 4242 normal;
// a run that ends sooner is followed by the next seed until 300 s of sim time are covered) per map, with Core's ImpactRules
// on the sim's bus exactly as the Game's loop drives it (SurvivalGame.Update + SurvivalGame.Impact.cs: a 144 Hz display's
// frames, the frame's share from BeginFrame, the 120 Hz accumulator, the sim's own hitstop handed over and drained after
// each step, the stepping stopped when a freeze begins). Real time = sim time + the frozen time granted (+ the slow-mos'
// stretch). Prints the freezes by cause (count, total ms), the frozen ms per minute, the worst rolling second and minute
// (the budgeted freezes; the exempt parries apart), the slow-mos, the requests skipped and cut. Exit 0 when the budget
// (design.impact.budget) holds on every map. No map argument: the arena (PortExport/maps/survival.json) and DRYDOCK
// (Design/maps/drydock.json). The automation runs at HIT FREEZE FULL and SLOW-MO on (the design defaults).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;
using Flow = Skygrind.Game.Flow;

public static partial class Harness
{
    static partial void ImpactCommand(string[] args, ref int code)
    {
        var maps = new List<string>();
        if (args.Length > 1) maps.Add(args[1]);
        else
        {
            maps.Add(Path.Combine(Root, "maps", "survival.json"));
            maps.Add(Path.GetFullPath(Path.Combine("Design", "maps", "drydock.json")));
        }
        double seconds = args.Length > 2 ? double.Parse(args[2], CultureInfo.InvariantCulture) : 300;
        bool all = true;
        foreach (string m in maps) all &= ImpactLedger(m, seconds);
        Console.WriteLine($"impact {(all ? "PASS" : "FAIL")}: the budget {(all ? "holds" : "is broken")} on {maps.Count} map(s)");
        code = all ? 0 : 1;
    }

    struct ImpactFreezeRec
    {
        public double start, seconds;
        public int tier;
        public bool exempt;
    }

    static bool ImpactLedger(string mapPath, double seconds)
    {
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(mapPath))));
        double perSecond = cfg.Num("design.impact.budget.perSecond"), perMinute = cfg.Num("design.impact.budget.perMinute") * 60;
        bool parryExempt = cfg.Bool("design.impact.budget.parryExempt");
        const double frameDt = 1.0 / 144;
        var freezes = new List<ImpactFreezeRec>();
        var count = new int[ImpactRules.TierCount];
        var ms = new double[ImpactRules.TierCount];
        var skips = new int[ImpactRules.TierCount];
        var slow = new int[ImpactRules.SlowMoCount];
        var slowSkips = new int[ImpactRules.SlowMoCount];
        int cuts = 0, kills = 0, cleared = 0, runs = 0, simEvents = 0;
        double simTime = 0, realTime = 0;
        uint seed = 4242;
        var wall = System.Diagnostics.Stopwatch.StartNew();
        while (simTime < seconds - 1e-9)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            var ap = new AutopilotInput(sim, "normal", seed);
            sim.UseInput(ap);
            var rules = new ImpactRules(cfg);
            rules.Watch(sim.events);
            sim.events.On("hitstop", _ => simEvents++);
            sim.events.On<KillEvent>("kill", e => { if (e.owner == "player") kills++; });
            sim.events.On("roundClear", _ => cleared++);
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(seed, null, null);
            runs++;
            double acc = 0, runReal = 0, runSim = 0;
            while (sim.flow != Flow.Results && simTime + runSim < seconds - 1e-9)
            {
                if (sim.hitstopLeft > 0) sim.hitstopLeft = 0;
                rules.ClearPending();
                double share = rules.BeginFrame(frameDt, sim.flow != Flow.Playing);
                runReal += frameDt;
                if (share <= 0) continue;
                acc += frameDt * share;
                while (acc >= sim.step && simTime + runSim < seconds - 1e-9)
                {
                    AutopilotInput.TakeDraft(sim);
                    sim.hitstopRequested = false;
                    sim.Step();
                    runSim += sim.step;
                    acc -= sim.step;
                    double simAsk = sim.hitstopRequested ? sim.hitstopLeft : 0;
                    sim.hitstopLeft = 0;
                    bool froze = rules.EndStep(sim.flow != Flow.Playing, simAsk);
                    if (froze)
                    {
                        int t = (int)rules.granted;
                        freezes.Add(new ImpactFreezeRec
                        {
                            start = realTime + rules.Clock, seconds = rules.grantedSeconds, tier = t,
                            exempt = parryExempt && (rules.granted == ImpactTier.Parry || rules.granted == ImpactTier.PerfectParry),
                        });
                    }
                    if (froze)
                    {
                        acc %= sim.step;
                        break;
                    }
                }
                if (sim.flow == Flow.Results) break;
            }
            for (int t = 0; t < ImpactRules.TierCount; t++)
            {
                count[t] += rules.freezes[t];
                ms[t] += rules.frozenSeconds[t] * 1000;
                skips[t] += rules.skips[t];
            }
            for (int c = 0; c < ImpactRules.SlowMoCount; c++)
            {
                slow[c] += rules.slowMos[c];
                slowSkips[c] += rules.slowMoSkips[c];
            }
            cuts += rules.cuts;
            simTime += runSim;
            realTime += runReal;
            ap.Detach();
            seed++;
        }

        // The worst rolling windows, exactly: a window's frozen time peaks when it ends at a freeze's end.
        double WorstWindow(double width, bool budgetedOnly)
        {
            double worst = 0;
            for (int i = 0; i < freezes.Count; i++)
            {
                if (budgetedOnly && freezes[i].exempt) continue;
                double to = freezes[i].start + freezes[i].seconds, from = to - width, sum = 0;
                for (int j = i; j >= 0; j--)
                {
                    ImpactFreezeRec f = freezes[j];
                    double b = f.start + f.seconds;
                    if (b <= from) break;
                    if (budgetedOnly && f.exempt) continue;
                    double lo = Math.Max(f.start, from), hi = Math.Min(b, to);
                    if (hi > lo) sum += hi - lo;
                }
                worst = Math.Max(worst, sum);
            }
            return worst;
        }

        // NIGHT16 review (finding 1): the rhythm: the most freezes starting in any rolling second, the shortest gap between two.
        int mostStarts = 0;
        double shortestGap = double.PositiveInfinity;
        for (int i = 0; i < freezes.Count; i++)
        {
            int n = 0;
            for (int j = i; j < freezes.Count && freezes[j].start < freezes[i].start + 1 - 1e-9; j++) n++;
            mostStarts = Math.Max(mostStarts, n);
            if (i > 0) shortestGap = Math.Min(shortestGap, freezes[i].start - freezes[i - 1].start);
        }
        double totalMs = 0, budgetedMs = 0;
        foreach (ImpactFreezeRec f in freezes)
        {
            totalMs += f.seconds * 1000;
            if (!f.exempt) budgetedMs += f.seconds * 1000;
        }
        double minutes = realTime / 60;
        double worstSecond = WorstWindow(1, true), worstMinute = WorstWindow(60, true), worstSecondAll = WorstWindow(1, false);
        bool ok = worstSecond <= perSecond + 1e-9 && worstMinute <= perMinute + 1e-9 && mostStarts <= 2;
        var ic = CultureInfo.InvariantCulture;
        Console.WriteLine(string.Format(ic, "impact ledger {0}: {1} run(s) from seed 4242 normal, sim {2:0.0} s, real {3:0.00} s (+{4:0.00} s frozen, +{5:0.00} s slow-mo stretch), {6} kills, {7} rounds cleared, {8} sim hitstop events ({9:0.0} s wall)",
            map.name, runs, simTime, realTime, totalMs / 1000, realTime - simTime - totalMs / 1000, kills, cleared, simEvents, wall.Elapsed.TotalSeconds));
        Console.WriteLine("  freezes by cause (count, total ms; skipped = cooldown or budget):");
        for (int t = 1; t < ImpactRules.TierCount; t++)
        {
            if (count[t] == 0 && skips[t] == 0) continue;
            Console.WriteLine(string.Format(ic, "    {0,-13} {1,5} x  {2,8:0.0} ms   skipped {3}", ImpactRules.TierNames[t], count[t], ms[t], skips[t]));
        }
        Console.WriteLine(string.Format(ic, "  cut to the budget left: {0}", cuts));
        Console.WriteLine(string.Format(ic, "  frozen per minute of real time: {0:0.0} ms all, {1:0.0} ms budgeted ({2:0.00} % of real time; budget {3:0.0} ms = {4:0.0} %)",
            minutes > 0 ? totalMs / minutes : 0, minutes > 0 ? budgetedMs / minutes : 0, realTime > 0 ? budgetedMs / 10 / realTime : 0, perMinute * 1000, perMinute / 60 * 100));
        Console.WriteLine(string.Format(ic, "  worst rolling second: {0:0.0} ms budgeted (budget {1:0.0} ms), {2:0.0} ms with the exempt parries", worstSecond * 1000, perSecond * 1000, worstSecondAll * 1000));
        Console.WriteLine(string.Format(ic, "  worst rolling minute: {0:0.0} ms budgeted (budget {1:0.0} ms)", worstMinute * 1000, perMinute * 1000));
        Console.WriteLine(string.Format(ic, "  rhythm: at most {0} freeze(s) starting in any rolling second; the shortest gap between two starts {1:0.000} s (cooldown.any {2:0.00} s)",
            mostStarts, freezes.Count > 1 ? shortestGap : 0, cfg.Num("design.impact.cooldown.any")));
        var parts = new List<string>();
        for (int c = 1; c < ImpactRules.SlowMoCount; c++) parts.Add(string.Format(ic, "{0} {1} (skipped {2})", ImpactRules.SlowMoNames[c], slow[c], slowSkips[c]));
        Console.WriteLine("  slow-mos: " + string.Join(", ", parts));
        Console.WriteLine($"  budget {(ok ? "HOLDS" : "BROKEN")}");
        return ok;
    }
}
