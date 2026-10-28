// NIGHT22 U3-36.5 DEADLINE bar (builder S owns this file): `deadline2 [--chase] [--seeds n] [--tag t]`, the headless half of the
// contract `deadline`:
//   1. the bar's data (Core/Deadline21.cs): a kill's refill event carries what it paid (the HUD's separate "+N" pop: 2.5 a kill,
//      5 a slide kill, 6 a slam kill), a clock tick and a burn pay 0, a burn is one hit that never kills (the burn flash);
//   2. with --chase (A-5, ONE batch a night: run it alone): THE CHASE's KITE over `--seeds` seeds (default 30) at rounds 10 and 15,
//      the round as the design draws it against the DEADLINE twist forced on that round: the twist's median survival must be at
//      most 50 % of the normal median at BOTH rounds. Both medians and the ratio are printed for each round. If a round fails,
//      retune design.n21.deadline (both config copies, with a _why) and log both numbers.
// Code 0 when every line PASSes.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void Deadline2Command(string[] args, ref int code)
    {
        var t = System.Diagnostics.Stopwatch.StartNew();
        GameConfig cfg = Config();
        bool ok = true;

        // 1. the bar's data on a bare bus
        var bus = new EventBus();
        var hp = new Health(cfg.Num("health.max"), bus, "player");
        bool sliding = false;
        var fuse = new Deadline21(cfg, bus, hp, () => sliding);
        var paids = new List<double>();
        int ticks = 0, burns = 0, badTick = 0;
        double burnHit = 0;
        bus.On<DeadlineEvent>(Deadline21.Tick, e =>
        {
            if (e.refill) paids.Add(e.paid);
            else
            {
                ticks++;
                if (e.paid != 0) badTick++;
            }
        });
        bus.On<DeadlineEvent>(Deadline21.Burn, e =>
        {
            burns++;
            if (burns == 1) burnHit = e.hit;
            if (e.paid != 0) badTick++;
        });
        fuse.NewRun(true);
        bus.Emit(N20Events.SpecialRound, new SpecialRoundEvent { name = "twist:" + Deadline21.TwistId, round = 9, phase = "start" });
        for (int i = 0; i < MovementMath.StepsFor(6, cfg.Dt); i++) fuse.Step(); // let the fuse burn down so a refill is not capped
        bus.Emit("kill", new KillEvent { weapon = "revolver" });
        sliding = true;
        bus.Emit("kill", new KillEvent { weapon = "revolver" });
        sliding = false;
        bus.Emit("kill", new KillEvent { weapon = "slam" });
        double want0 = cfg.Num("design.n21.deadline.killRefill"), want1 = cfg.Num("design.n21.deadline.slideRefill"), want2 = cfg.Num("design.n21.deadline.slamRefill");
        bool paidOk = paids.Count == 3 && Math.Abs(paids[0] - want0) < 1e-9 && Math.Abs(paids[1] - want1) < 1e-9 && Math.Abs(paids[2] - want2) < 1e-9;
        Console.WriteLine($"deadline2 bar data: the kill refills carry what they paid ({string.Join("/", paids.ConvertAll(p => p.ToString("0.#", CultureInfo.InvariantCulture)))} s; want {want0}/{want1}/{want2}), clock ticks pay 0 ({ticks} ticks, {badTick} wrong): {(paidOk && badTick == 0 && ticks > 0 ? "PASS" : "FAIL")}");
        ok &= paidOk && badTick == 0 && ticks > 0;
        for (int i = 0; i < MovementMath.StepsFor(60, cfg.Dt) && burns < 3; i++) fuse.Step();
        bool burnOk = burns >= 1 && burnHit > 0 && !hp.Dead && hp.hp >= 1 - 1e-9;
        Console.WriteLine($"deadline2 burn flash data: {burns} burns, the first took {burnHit:0.#} HP, alive {(!hp.Dead ? "yes" : "NO")}, HP left {hp.hp:0.#}: {(burnOk ? "PASS" : "FAIL")}");
        ok &= burnOk;

        // 2. A-5: THE CHASE at rounds 10 and 15 (one batch; alone)
        if (Array.IndexOf(args, "--chase") >= 0)
        {
            int seeds = int.Parse(StampArg(args, "--seeds", "30"), CultureInfo.InvariantCulture);
            SandboxChase.GustOff = false;
            foreach (int round in new[] { 10, 15 })
            {
                // --normal10 / --normal15: the normal median already measured on these seeds (the run is deterministic), so a retune's
                // re-check runs only the twist side
                string kn = StampArg(args, round == 10 ? "--normal10" : "--normal15", null);
                double? knownNormal = kn != null ? double.Parse(kn, CultureInfo.InvariantCulture) : (double?)null;
                var normal = new List<double>();
                var twist = new List<double>();
                int diedN = 0, diedT = 0;
                for (int s = 1; s <= seeds; s++)
                {
                    if (knownNormal == null)
                    {
                        SandboxChase.Twist = null;
                        SandboxChase.Result a = SandboxChase.Run(cfg, Map(), "kite", round, (uint)s);
                        normal.Add(a.survival);
                        if (a.died) diedN++;
                    }
                    SandboxChase.Twist = "deadline";
                    SandboxChase.Result b = SandboxChase.Run(cfg, Map(), "kite", round, (uint)s);
                    twist.Add(b.survival);
                    if (b.died) diedT++;
                }
                SandboxChase.Twist = null;
                double mn = knownNormal != null ? knownNormal.Value : Skygrind.Game.Sandbox.Median(normal), mt = Skygrind.Game.Sandbox.Median(twist);
                double ratio = mn > 0 ? mt / mn : double.NaN;
                bool pass = mn > 0 && ratio <= 0.5;
                Console.WriteLine($"deadline2 chase round {round} ({seeds} seeds, KITE): normal median {mn:0.0} s (died {diedN}/{seeds}), DEADLINE median {mt:0.0} s (died {diedT}/{seeds}), {ratio * 100:0} % of normal (want at most 50 %): {(pass ? "PASS" : "FAIL")}");
                ok &= pass;
            }
        }
        else Console.WriteLine("deadline2 chase: not run (add --chase [--seeds 30]; one batch a night, alone)");
        code = ok ? 0 : 1;
        Console.WriteLine($"deadline2 {(code == 0 ? "PASS" : "FAIL")}; {t.Elapsed.TotalSeconds:0.0} s wall");
    }
}
