// NIGHT48 builder W (G1, NIGHT48.md section 1: the climb's top third): `n48wick`, THE STACK's climb meters with the climbTop48 lips, headless.
// Hooked without a Program.cs line: a module initializer runs the command and exits when argv[1] is "n48wick" (Harness.N46.Flow.cs's pattern);
// it applies the off switches itself (--off48: NightRows.ForceOffFrom).
//   n48wick sloppy [--seeds 40] [--cap 90] [--nolips] [--control] [--trace]
//     NIGHT47's SLOPPY CLIMB PILOT (Harness.N47.Wick.cs W47Sloppy: +-12 deg look per ride, a level look on half the rides, one release in four
//     150 ms late, air presses 0-240 ms late) on the stack WITH the lips (sim.climbTop48Force: design.n48.climbTop48.numbers), then G1's
//     verdict: the bottom (the steps' first tries) >= 95 %, each top-third gap's first tries (4 -> 5, 5 -> 6, 6 -> 7) >= 80 %, 39+ / 40 seeds
//     reach the cap, the climb median 25-45 s; and the landings on a lip. --nolips: the same on NIGHT47's stack (the RED half).
//   n48wick lips
//     The lips as built (per lip: the panel it follows, its top over that panel's bottom, its arc, the open wall to the next panel's start).
//   n48wick windows
//     NIGHT47's release windows per ride gap (n47wick windows) on the lipped stack.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    /// <summary>NIGHT48 W: the climbTop48 lips on THE STACK of every W47Stack sim (n48wick sets it; false: NIGHT47's stack).</summary>
    static bool W48Lips;
    static int W48LipLandsTotal;
    /// <summary>n48wick (default; --oldlate: off): the sloppy pilot's late release lets go 150 ms late for 4 steps (NIGHT47's model held jump
    /// through the pilot's whole 4-step release, so a "late" rider never let go: traced, n48wick sloppy --steps).</summary>
    static bool W48LateLetGo;
    /// <summary>n48wick sloppy --steps: each top-third gap's steps (state, jump, along %, r, feet over the panel's bottom, vy, the late window).</summary>
    static List<string> W48Ring;
    /// <summary>The last sloppy run's panel count (the verdict's ride gaps end at count - 2).</summary>
    static int W48Panels;
    static (Dictionary<int, (int ok, int n)> first, Dictionary<int, (int ok, int n)> steps, int made, List<double> times) W48Last;

    /// <summary>n48wick's sweep (harness only): W48LIP="lipOver,lipArcDeg,lipStartDeg,lipDepth" replaces design.n48.climbTop48.numbers in this
    /// run's config (in memory); unset: the config's numbers.</summary>
    static void W48Tune(GameConfig cfg)
    {
        string e = Environment.GetEnvironmentVariable("W48LIP");
        if (string.IsNullOrEmpty(e)) return;
        string[] v = e.Split(',');
        var n = PortData.Obj(PortData.Obj(PortData.Obj(PortData.Obj(cfg.root["design"])["n48"])["climbTop48"])["numbers"]);
        string[] keys = { "lipOver", "lipArcDeg", "lipStartDeg", "lipDepth" };
        for (int i = 0; i < keys.Length && i < v.Length; i++) n[keys[i]] = double.Parse(v[i], System.Globalization.CultureInfo.InvariantCulture);
        if (v.Length > 4) // the 5th value: extraTopPanels; the lips then follow every top panel but the last (4 .. 6 + extra)
        {
            int extra = int.Parse(v[4], System.Globalization.CultureInfo.InvariantCulture);
            n["extraTopPanels"] = (double)extra;
            var lips = new List<object>();
            for (int p = 4; p <= 6 + extra; p++) lips.Add((double)p);
            n["lipAfter"] = lips;
        }
    }

    /// <summary>The human pilot's hook (Harness.N47.Human.cs H47ApplyRows, one line): `--rows ...,climbTop48,...` sets the lips on this sim (and
    /// on every later one: the name leaves the list, W48HumanForce stays on).</summary>
    static bool W48HumanForce;
    static bool W48HumanLips(SurvivalSim sim, ref string[] rows)
    {
        if (Array.IndexOf(rows, "climbTop48") >= 0) { W48HumanForce = true; rows = rows.Where(r => r != "climbTop48").ToArray(); }
        if (W48HumanForce) { sim.climbTop48Force = true; W48Tune(sim.cfg); }
        return W48HumanForce;
    }

    [ModuleInitializer]
    internal static void W48WickInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n48wick") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            code = W48Wick(args);
        }
        catch (Exception e) { Console.WriteLine("n48wick: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static int W48Wick(string[] args)
    {
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "sloppy";
        W48Lips = Array.IndexOf(args, "--nolips") < 0;
        W48LateLetGo = Array.IndexOf(args, "--oldlate") < 0;
        Console.WriteLine($"n48wick {what}: climbTop48 lips {(W48Lips ? "ON (forced)" : "OFF (NIGHT47's stack)")}, the late release {(W48LateLetGo ? "lets go 150 ms late (n48)" : "NIGHT47's model (held through the release)")}, off from night {NightRows.ForceOffFrom}");
        if (what == "lips")
        {
            SurvivalSim sim = W47Stack(1);
            Wick37Stack s = sim.wick37Stack;
            Console.WriteLine($"  stack: {s.panels.Count} panels, cap {s.p.capY:0.0} m, lips {s.lipTops.Count} (Climb48On {sim.Climb48On})");
            for (int i = 0; i < s.lipTops.Count; i++)
            {
                Wick37Panel pn = s.panels[s.lipPanel[i]];
                string next = "-";
                if (s.lipPanel[i] + 1 < s.panels.Count)
                {
                    Wick37Panel q = s.panels[s.lipPanel[i] + 1];
                    double qa = q.angle - JsMath.Atan2((q.end - q.start).Length * 0.5, s.p.panelApothem);
                    next = $"{(qa - s.lipTo[i]) * 180 / Math.PI:0.0} deg of open wall to panel {q.index}'s start (its bottom {q.bottom - s.lipTops[i]:+0.00;-0.00} m over the lip)";
                }
                Console.WriteLine($"  lip {i}: after panel {pn.index} (bottom {pn.bottom:0.0}), top {s.lipTops[i]:0.00} ({s.lipTops[i] - pn.bottom:+0.00} over it), {s.lipFrom[i] * 180 / Math.PI:0.0} .. {s.lipTo[i] * 180 / Math.PI:0.0} deg, r {s.LipInner:0.0} .. {s.p.radius:0.0} m; {next}");
            }
            return 0;
        }
        if (what == "windows") return W47Wick(new[] { "n47wick", "windows" });
        if (what == "sloppy")
        {
            int si = Array.IndexOf(args, "--seeds"), ci = Array.IndexOf(args, "--cap");
            int seeds = si >= 0 ? int.Parse(args[si + 1], System.Globalization.CultureInfo.InvariantCulture) : 40;
            double cap = ci >= 0 ? double.Parse(args[ci + 1], System.Globalization.CultureInfo.InvariantCulture) : 90;
            W47Trace = Array.IndexOf(args, "--trace") >= 0;
            if (Array.IndexOf(args, "--steps") >= 0) { W47Trace = true; W48Ring = new List<string>(); }
            if (Array.IndexOf(args, "--control") >= 0) { W48LipLandsTotal = 0; W47Sloppy(seeds, false, cap); Console.WriteLine($"  (control) lip landings {W48LipLandsTotal}"); }
            W48LipLandsTotal = 0;
            W47Sloppy(seeds, true, cap);
            var (first, steps, made, times) = W48Last;
            int sOk = steps.Values.Sum(v => v.ok), sN = steps.Values.Sum(v => v.n);
            double bottom = sN > 0 ? 100.0 * sOk / sN : 0;
            var top = new List<string>();
            bool topOk = true;
            int tOk = 0, tN = 0;
            int lastGap = Math.Max(6, W48Panels - 2); // the top section's ride gaps: 4 -> 5 .. the last ride gap (panel count - 2; more with extraTopPanels)
            foreach (int gp in Enumerable.Range(4, lastGap - 3))
            {
                var v = first.TryGetValue(gp, out var x) ? x : (0, 0);
                double pc = v.Item2 > 0 ? 100.0 * v.Item1 / v.Item2 : 0;
                topOk &= v.Item2 > 0 && pc >= 80 - 1e-9;
                tOk += v.Item1; tN += v.Item2;
                top.Add($"{gp}->{gp + 1} {v.Item1}/{v.Item2} ({pc:0.0} %)");
            }
            double med = times.Count == 0 ? 0 : times[Math.Min(times.Count - 1, times.Count / 2)];
            bool capOk = made >= seeds - 1, timeOk = med >= 25 && med <= 45, bottomOk = bottom >= 95 - 1e-9;
            bool green = capOk && timeOk && bottomOk && topOk;
            Console.WriteLine($"n48wick G1 {(green ? "GREEN" : "RED")} (lips {(W48Lips ? "on" : "off")}): bottom (steps) first tries {sOk}/{sN} = {bottom:0.0} % (>= 95: {(bottomOk ? "met" : "NOT met")}); top third first tries {string.Join(", ", top)}, pooled {tOk}/{tN} = {(tN > 0 ? 100.0 * tOk / tN : 0):0.0} % (each >= 80: {(topOk ? "met" : "NOT met")}); the cap {made} / {seeds} (39+: {(capOk ? "met" : "NOT met")}); climb median {med:0.0} s (25-45: {(timeOk ? "met" : "NOT met")}); lip landings {W48LipLandsTotal}");
            return 0;
        }
        Console.WriteLine("n48wick: unknown command " + what);
        return 1;
    }
}
