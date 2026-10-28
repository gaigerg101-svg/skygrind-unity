// NIGHT55 builder B (wick55): n47human's WICK window / hot-wall stats line (an H47Policy: read-only, it changes no input; the pilot files are
// untouched) and `n55wick`, n47human's WICK fight with the CASUAL pilot (Wick37Pilot.casual: L-058; the same setup, human model and policies).
// Per run, from the state BEFORE each step (as n47human's own `open`): the boss's damage by the state it landed in: the stagger WINDOW
// (brain.Open), a CRACKED plate (open43 without the window), the P3 crack-open / bare core (coreOpen47 / bare47), or CHIP (everything else);
// the hot-wall burns split into rides and air brushes and by the time since the last burn; the hits taken by source. One line when the last
// seed's fight ends.
//   dotnet run --project Tools/SimHarness -c Release -- n47human --boss wick --seeds 40 [--rows wick55]      (the skilled pilot)
//   dotnet run --project Tools/SimHarness -c Release -- n55wick --seeds 40 [--rows wick55] [--cap 300]       (the casual pilot)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class W55Rec
    {
        public bool w55, done, hooked;
        public double prevFrac = -1, dWindow, dCrack, dCore, dChip;
        public int kind, hotRide, hotBrush, hotQuick, lastBurnStep = -100000, ember, geyser, ring, charge, curtainHits, hot, slag, windows, punchesInWindow, corePunches;
        public bool wasOpen, prevRide;
    }

    static readonly List<W55Rec> W55Recs = new List<W55Rec>();
    static int W55Seeds = 40;
    static bool W55Hooked;

    static void H47PolicyWick55Stats(H47Ctx ctx, ref InputFrame move, ref CombatInput combat)
    {
        if (ctx.boss != "wick") return;
        SurvivalSim sim = ctx.sim;
        Wick37 w = sim.wick37;
        if (w == null || w.brain == null) return;
        if (!W55Hooked) { W55Hooked = true; W55Seeds = int.TryParse(H47Arg(Environment.GetCommandLineArgs(), "--seeds", "40"), out int sd) ? sd : 40; }
        if (!ctx.state.TryGetValue("w55rec", out object o)) { o = new W55Rec(); ctx.state["w55rec"] = o; W55Recs.Add((W55Rec)o); }
        var r = (W55Rec)o;
        if (r.done) return;
        if (!r.hooked)
        {
            r.hooked = true;
            int step0 = ctx.step;
            sim.events.On("punch", _ => { if (!r.done && w.brain != null && w.brain.Open) r.punchesInWindow++; });
        }
        r.w55 = w.w55;
        // the damage of the last step, by the state it was dealt in (the state read at the last call: before that step)
        double fr = w.HpFrac;
        if (r.prevFrac >= 0 && fr < r.prevFrac)
        {
            double d = r.prevFrac - fr;
            if (r.kind == 1) r.dWindow += d; else if (r.kind == 2) r.dCrack += d; else if (r.kind == 3) r.dCore += d; else r.dChip += d;
        }
        r.prevFrac = fr;
        r.kind = w.brain.Open ? 1 : w.CrackedCount > 0 ? 2 : (w.coreOpen47 > 0 || w.bare47 > 0) ? 3 : 0;
        if (w.brain.Open && !r.wasOpen) r.windows++;
        r.wasOpen = w.brain.Open;
        int burns = sim.wick37HotBurns;
        if (burns > r.hot) { if (r.prevRide) r.hotRide++; else r.hotBrush++; if (ctx.step - r.lastBurnStep < 240) r.hotQuick++; r.lastBurnStep = ctx.step; }
        r.prevRide = sim.mover.state == MoveState.WallRide;
        r.hot = burns;
        r.slag = sim.wick37SlagTouches;
        r.ember = w.emberHits; r.geyser = w.geyserHits47; r.ring = w.ringHits47; r.charge = w.chargeHits47; r.corePunches = w.corePunches47;
        if (w.brain.state == Boss37State.Dying || w.brain.state == Boss37State.Dead || !w.Alive)
        {
            r.done = true;
            if (W55Recs.Count(x => x.done) == W55Seeds) W55Print();
        }
    }

    static void W55Print()
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        int n = W55Recs.Count;
        string Med(IEnumerable<double> xs) { var l = xs.OrderBy(x => x).ToList(); return l.Count == 0 ? "n/a" : (l[l.Count / 2] * 100).ToString("0", ci) + " %"; }
        double Tot(W55Rec x) => Math.Max(1e-9, x.dWindow + x.dCrack + x.dCore + x.dChip);
        int hot = W55Recs.Sum(x => x.hot), other = W55Recs.Sum(x => x.ember + x.geyser + x.ring + x.charge + x.slag);
        Console.WriteLine($"wick55 stats ({n} runs, wick55 {(W55Recs.All(x => x.w55) ? "ON" : W55Recs.Any(x => x.w55) ? "MIXED" : "OFF")}; bot): the boss's damage by state, median of runs: " +
            $"stagger window {Med(W55Recs.Select(x => x.dWindow / Tot(x)))}, cracked plate {Med(W55Recs.Select(x => x.dCrack / Tot(x)))}, P3 crack-open / bare core {Med(W55Recs.Select(x => x.dCore / Tot(x)))}, chip {Med(W55Recs.Select(x => x.dChip / Tot(x)))}; " +
            $"stagger windows {W55Recs.Sum(x => x.windows)} (punches inside {W55Recs.Sum(x => x.punchesInWindow)}), core punches {W55Recs.Sum(x => x.corePunches)}; " +
            $"hot-wall burns {hot} (rides {W55Recs.Sum(x => x.hotRide)}, air brushes {W55Recs.Sum(x => x.hotBrush)}, within 2 s of the last {W55Recs.Sum(x => x.hotQuick)}) of {hot + other} hits taken ({(hot + other > 0 ? hot * 100.0 / (hot + other) : 0).ToString("0", ci)} %); " +
            $"other hits: ember {W55Recs.Sum(x => x.ember)}, geyser {W55Recs.Sum(x => x.geyser)}, ring {W55Recs.Sum(x => x.ring)}, charge {W55Recs.Sum(x => x.charge)}, slag {W55Recs.Sum(x => x.slag)}");
    }

    // ---- n55wick: the casual pilot (L-058) ------------------------------------------------------------------------------------------------------

    [ModuleInitializer]
    internal static void N55WickInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n55wick") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            code = N55WickMain(args);
        }
        catch (Exception e) { Console.WriteLine("n55wick: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static H47Fight N55WickCasual(uint seed)
    {
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noLab = sim.noRecords = true;
        H47ApplyRows(sim);
        sim.wick37CentreOverride = Stack37;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        if (!sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
        if (sim.wick37Stack == null) return null;
        sim.Respawn(Stack37 + new Vec3(0, 0, sim.wick37Stack.p.radius + 1.5), 0);
        if (!sim.BossRoomBegin("wick", out _, out _)) return null;
        Wick37 w = sim.wick37;
        var pilot = new Wick37Pilot { hand = new Mulberry32(seed * 7919u + 13u), casual = true };
        double yaw = 0;
        sim.health.max = sim.health.hp = 1e6;
        return new H47Fight
        {
            boss = "wick", sim = sim, reveal = MovementMath.StepsFor(w.p.revealSeconds, sim.cfg.Dt), mortalCounted = true,
            pilot = i => { pilot.Step(sim, yaw, i); yaw = pilot.Move.yaw; return (pilot.Move, pilot.Combat); },
            alive = () => w.Alive || w.brain.state == Boss37State.Dying,
            telling = () => w.brain.state == Boss37State.Tell, at = () => w.Centre,
            frac = () => w.HpFrac,
            note = "survival map stack (2600, 0, 0), Wick37Pilot CASUAL (L-058), deaths mortal-counted (100 hp)",
        };
    }

    static int N55WickMain(string[] args)
    {
        int seeds = int.Parse(H47Arg(args, "--seeds", "40"), CultureInfo.InvariantCulture);
        double cap = double.Parse(H47Arg(args, "--cap", "300"), CultureInfo.InvariantCulture);
        H47Rows = H47Arg(args, "--rows", "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        H47Policies = H47FindPolicies();
        Console.WriteLine($"n55wick (casual): seeds 1..{seeds}, cap {cap:0} s, rows [{string.Join(",", H47Rows)}], off from {NightRows.ForceOffFrom}, policies [{string.Join(",", H47Policies.Select(p => p.Method.Name))}]");
        var runs = new List<H47Run>();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int s = 1; s <= seeds; s++) runs.Add(H47Play(N55WickCasual((uint)s), (uint)s, cap));
        var begun = runs.Where(x => x.begun).ToList();
        if (begun.Count == 0) { Console.WriteLine("n55wick: NO FIGHT began"); return 1; }
        var t = begun.Select(x => x.killed ? x.seconds : cap).OrderBy(x => x).ToList();
        double Q(double q) => t[Math.Min(t.Count - 1, (int)Math.Floor(q * (t.Count - 1) + 0.5))];
        var share = begun.Where(x => x.dealt > 0).Select(x => x.dealtOpen / x.dealt).OrderBy(x => x).ToList();
        int killed = begun.Count(x => x.killed);
        Console.WriteLine($"n55wick casual (bot): killed {killed}/{begun.Count}; kill time median {H47F(Q(0.5))} s, p90 {H47F(Q(0.9))} s, min {H47F(t[0])} s, max {H47F(t[^1])} s; deaths total {begun.Sum(x => x.deaths)}; dealt in windows median {H47Pc(share.Count > 0 ? share[share.Count / 2] : double.NaN)}; damage taken median {begun.Select(x => x.taken).OrderBy(x => x).ElementAt(begun.Count / 2):0}; {sw.Elapsed.TotalSeconds:0} s wall");
        return 0;
    }
}
