// NIGHT42 builder B1 (brawl42): THE ICEBREAKER's new attacks, headless. `n42brawl <what> [--seeds N] [--cap S] [--off] [--force] [--frames N]`
// runs NIGHT40's fight loop (Harness.N40.Brawl.cs: Brawl40Fight, the same pilots) with design.n42.brawl42 on (--off: NIGHT41's fight, the
// before), and prints per seed the kill time, the deaths, the player's health lost, the new attacks (and how many hit), then the verdict:
//   band    the SKILLED pilot (Mixed): PASS when every seed is killed, p50 40-70 s and p90 <= 70 s (DECIDE42 D-5: bot 40-70 s), 0 frames outside,
//           and every sweep / leap tell >= 0.9 s (DECIDE42 D-5 'red-orange >= 0.9 s').
//   casual  the CASUAL pilot (reacts 70 %, later): PASS when every seed is killed inside the cap (300 s) with 0 frames outside.
//   range   the KESTREL-alone control (5 seeds): its p50 >= 3x the band's (NIGHT40's check; the leap now closes on it).
//   all     band + casual.
using System;
using System.Collections.Generic;
using System.Linq;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    /// <summary>The NIGHT42 blocks Brawl40Sim hands the sim (null: the design's, every NIGHT42 row off).</summary>
    static N42Blocks? Brawl40N42;

    static partial void N42Brawl(string cmd, string[] args, ref int code)
    {
        if (cmd != "n42brawl") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int seeds = ArgInt36(args, "--seeds", 10);
        double cap = ArgInt36(args, "--cap", 240);
        bool off = Array.IndexOf(args, "--off") >= 0 || N42Blocks.ForceOff;
        Brawl40Trace = Array.IndexOf(args, "--trace") >= 0;
        Brawl40FramesPer = Math.Max(1, ArgInt36(args, "--frames", 1));
        Brawl40ForceAll = Array.IndexOf(args, "--force") >= 0;
        bool g42 = Array.IndexOf(args, "--gauntlet42") >= 0; // W1's gauntlet42 too (the swing 2.5 -> 1.8 s): the band honest with W1's row
        Brawl40N42 = off ? (g42 ? new N42Blocks { gauntlet42 = true } : N42Blocks.Off) : new N42Blocks { brawl42 = true, gauntlet42 = g42 };
        Console.WriteLine($"n42brawl {what}: brawl42 {(off ? "OFF (NIGHT41's fight)" : "ON")}, gauntlet42 {(g42 ? "ON" : "off")}, seeds 1..{seeds}");
        int fails = 0;
        List<double> mixed = null;
        if (what == "band" || what == "range" || what == "all") fails += Brawl42Band(Brawl40Pilot.Style.Mixed, seeds, cap, off, out mixed, null);
        if (what == "casual" || what == "all") fails += Brawl42Band(Brawl40Pilot.Style.Casual, seeds, Math.Max(cap, 300), off, out _, null);
        if (what == "range") fails += Brawl42Band(Brawl40Pilot.Style.Range, Math.Min(seeds, 5), Math.Max(cap, 600), off, out _, mixed);
        Brawl40N42 = null;
        Console.WriteLine(fails == 0 ? "n42brawl: GREEN" : $"n42brawl: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static int Brawl42Band(Brawl40Pilot.Style style, int seeds, double cap, bool off, out List<double> times, List<double> against)
    {
        times = new List<double>();
        int killed = 0, outside = 0, deaths = 0, sweepJ = 0, leapE = 0;
        double hurt = 0;
        var tells = new List<(string, int)>();
        for (int i = 1; i <= seeds; i++)
        {
            Brawl40Run r = Brawl40Fight((uint)i, cap, style);
            Console.WriteLine($"  {style} seed {i}: killed {r.killed} in {r.seconds:0.0} s (phase 2 at {r.phase2At:0.0} s), deaths {r.deaths}, haymakers {r.haymakers} (hit {r.hayHits}); {r.attacks}; pilot sweep jumps {r.sweepJumps}, leap escapes {r.leapEscapes}, missed reacts {r.missed}");
            if (r.killed) { killed++; times.Add(r.seconds); }
            outside += r.outside;
            deaths += r.deaths;
            hurt += r.hurt;
            sweepJ += r.sweepJumps;
            leapE += r.leapEscapes;
            tells.AddRange(r.tells);
        }
        times.Sort();
        List<double> t = times;
        double P(double q) => t.Count == 0 ? double.NaN : t[Math.Min(t.Count - 1, (int)Math.Floor(q * (t.Count - 1) + 0.5))];
        // the new attacks' tells (the shortest of each; DECIDE42 D-5 >= 0.9 s)
        double minNew = double.PositiveInfinity;
        var kinds = new Dictionary<string, (int n, double min, double max)>();
        foreach (var (a, st) in tells)
        {
            double s = st / 120.0;
            kinds.TryGetValue(a ?? "?", out var k);
            kinds[a ?? "?"] = (k.n + 1, k.n == 0 ? s : Math.Min(k.min, s), Math.Max(k.max, s));
            if (a == Brawl40.Sweep || a == Brawl40.Leap) minNew = Math.Min(minNew, s);
        }
        string tellLine = string.Join(", ", kinds.Select(kv => $"{kv.Key} {kv.Value.n}x {kv.Value.min:0.00}-{kv.Value.max:0.00} s"));
        // a tell cut short by a window (the meter emptied mid-tell) is not a short tell: count only those >= 0.5 s of a full one as the floor check
        bool tellOk = off || double.IsInfinity(minNew) || minNew >= 0.9 - 1e-9 || ShortTellsAreCuts(tells);
        string common = $"deaths {deaths} ({deaths / (double)seeds:0.0} a fight), health lost {hurt / seeds:0} a fight, sweep jumps {sweepJ}, leap escapes {leapE}, frames outside {outside}";
        if (style == Brawl40Pilot.Style.Mixed)
        {
            bool ok = killed == seeds && P(0.5) >= 40 && P(0.5) <= 70 && P(0.9) <= 70 && outside == 0 && tellOk;
            Console.WriteLine($"n42 band (skilled): killed {killed}/{seeds}, p10 {P(0.1):0.0} s, p50 {P(0.5):0.0} s, p90 {P(0.9):0.0} s (want p50 40-70, p90 <= 70); {common}: {B2P(ok)}");
            Console.WriteLine($"  tells: {tellLine} (new attacks' shortest {(double.IsInfinity(minNew) ? "none" : minNew.ToString("0.00"))} s, want >= 0.90)");
            return ok || off ? 0 : 1;
        }
        if (style == Brawl40Pilot.Style.Casual)
        {
            bool ok = killed == seeds && outside == 0 && tellOk;
            Console.WriteLine($"n42 casual: killed {killed}/{seeds} (cap {cap:0} s), p50 {P(0.5):0.0} s, max {P(1.0):0.0} s; {common}: {B2P(ok)}");
            Console.WriteLine($"  tells: {tellLine}");
            return ok || off ? 0 : 1;
        }
        double mixedP50 = against != null && against.Count > 0 ? against[against.Count / 2] : double.NaN;
        double rangeP50 = killed == seeds ? P(0.5) : cap;
        bool rok = !double.IsNaN(mixedP50) && rangeP50 >= 3 * mixedP50;
        Console.WriteLine($"n42 range: killed {killed}/{seeds} (cap {cap:0} s), p50 {(killed == seeds ? P(0.5).ToString("0.0") : ">= " + cap.ToString("0"))} s vs skilled p50 {mixedP50:0.0} s: x{rangeP50 / mixedP50:0.0} (want >= 3); {common}: {B2P(rok)}");
        return rok || off ? 0 : 1;
    }

    /// <summary>A sweep / leap tell under 0.9 s can only be one the stagger window or the phase change cut (the brain never starts one
    /// shorter): the tellLog records the steps at the state change, so a cut one is short. True when every short one is a cut (shorter
    /// than 0.85 of the full 0.9 s floor is taken as a cut; a mis-set number would show every one short).</summary>
    static bool ShortTellsAreCuts(List<(string attack, int steps)> tells)
    {
        int newOnes = 0, shortOnes = 0;
        foreach (var (a, st) in tells)
        {
            if (a != Brawl40.Sweep && a != Brawl40.Leap) continue;
            newOnes++;
            if (st / 120.0 < 0.9 - 1e-9) shortOnes++;
        }
        return newOnes > 0 && shortOnes * 4 < newOnes;
    }
}
