// NIGHT42 builder F1 (impact42; G9 / G15): `n42hit ...`, the hit / kill freeze ledger before / after, headless, with Core's ImpactRules driven
// as the Game's loop drives it (Harness.Impact.cs's frame loop: 144 Hz frames, the 120 Hz accumulator, the sim's own hitstop handed over,
// the stepping stopped when a freeze begins). Commands:
//   n42hit rules    the rules alone: each moment's freeze with impact42 off / on (a KESTREL / THUNDERCLAP body hit, a heavy hit, a kill, a
//                   5-kill blast, a gauntlet hit on a small / big / boss body, a gauntlet kill) and the gauntlet's minute share; PASS when on
//                   is longer for every moment, the gauntlet's boss hit is the longest, and the share holds.
//   n42hit ledger   the AUTOPILOT on the arena (seed 4242 normal, --seconds S, default 180) off / on: freezes by cause, frozen ms per minute.
//   n42hit brawl    THE ICEBREAKER (n40brawl's skilled pilot, Brawl40Pilot Mixed; --seeds N, default 3) off / on: the gauntlet's hits by
//                   class, the freezes by cause, the frozen time per minute of the fight (all, and the gauntlet's alone), the kill time.
//   n42hit all      the three. Exit 0 when every PASS line passes.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;
using Flow = Skygrind.Game.Flow;

public static partial class Harness
{
    static readonly CultureInfo Ic42 = CultureInfo.InvariantCulture;

    static partial void N42Impact(string cmd, string[] args, ref int code)
    {
        if (cmd != "n42hit") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "all";
        int fails = 0;
        if (what == "rules" || what == "all") fails += Hit42Rules();
        if (what == "ledger" || what == "all") fails += Hit42Ledger(ArgInt36(args, "--seconds", 180));
        if (what == "brawl" || what == "all") fails += Hit42Brawl(ArgInt36(args, "--seeds", 3));
        Console.WriteLine(fails == 0 ? "n42hit: GREEN" : $"n42hit: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static string P42(bool ok) => ok ? "PASS" : "FAIL";

    // --- the rules alone -------------------------------------------------------------------------------------------------------------

    /// <summary>One moment through fresh rules (clock past every cooldown): the freeze granted (s) and its tier.</summary>
    static (double s, ImpactTier t) Hit42Moment(GameConfig cfg, bool on, Action<ImpactRules> ask)
    {
        var r = new ImpactRules(cfg) { impact42 = on, crunch36 = cfg.Design("n36.crunch36.enabled") };
        r.BeginFrame(5, false); // 5 s on the clock: every cooldown and window is clear
        r.ClearPending();
        ask(r);
        r.EndStep(false);
        return (r.grantedSeconds, r.granted);
    }

    static HitEvent Hit42(string weapon, bool killed = false, bool headshot = false, double damage = 10, int shot = 1) =>
        new HitEvent { weapon = weapon, killed = killed, headshot = headshot, damage = damage, shot = shot, melee = weapon == "gauntlet" || weapon == "punch", distance = 1, kind = "husk" };

    static void Gauntlet42Ask(ImpactRules r, int cls, bool killed)
    {
        r.OnHit(Hit42("gauntlet", killed));
        if (killed) r.OnKill(new KillEvent { weapon = "gauntlet", kind = "husk" });
        r.OnGauntletHit42(new GauntletHit42Event { cls = cls, killed = killed });
    }

    static int Hit42Rules()
    {
        GameConfig cfg = Config();
        double heavyShot = cfg.Num("design.impact.freeze.heavyPellets") * cfg.Num("weapons.shotgun.pelletDamage");
        var moments = new List<(string name, Action<ImpactRules> ask)>
        {
            ("KESTREL body hit", r => r.OnHit(Hit42("revolver"))),
            ("THUNDERCLAP body hit (3 pellets)", r => r.OnHit(Hit42("shotgun", damage: heavyShot / 2))),
            ("THUNDERCLAP heavy hit", r => r.OnHit(Hit42("shotgun", damage: heavyShot))),
            ("KESTREL kill", r => { r.OnHit(Hit42("revolver", true)); r.OnKill(new KillEvent { weapon = "revolver" }); }),
            ("KESTREL headshot kill", r => { r.OnHit(Hit42("revolver", true, true)); r.OnKill(new KillEvent { weapon = "revolver", headshot = true }); }),
            ("THUNDERCLAP 5-kill blast", r => { for (int i = 0; i < 5; i++) { r.OnHit(Hit42("shotgun", true, damage: heavyShot)); r.OnKill(new KillEvent { weapon = "shotgun" }); } }),
            ("punch kill", r => { r.OnHit(Hit42("punch", true)); r.OnKill(new KillEvent { weapon = "punch" }); }),
            ("gauntlet hit, husk", r => Gauntlet42Ask(r, GauntletHit42Event.Small, false)),
            ("gauntlet hit, tank", r => Gauntlet42Ask(r, GauntletHit42Event.Big, false)),
            ("gauntlet hit, THE ICEBREAKER", r => Gauntlet42Ask(r, GauntletHit42Event.Boss, false)),
            ("gauntlet kill, husk", r => Gauntlet42Ask(r, GauntletHit42Event.Small, true)),
        };
        Console.WriteLine("n42hit rules: each moment's freeze, impact42 off -> on (ms, tier; crunch36 as configured):");
        bool longer = true;
        double bossS = 0, maxOther = 0;
        foreach (var (name, ask) in moments)
        {
            var off = Hit42Moment(cfg, false, ask);
            var on = Hit42Moment(cfg, true, ask);
            bool ok = on.s > off.s + 1e-9;
            longer &= ok;
            if (name.Contains("ICEBREAKER")) bossS = on.s;
            else maxOther = Math.Max(maxOther, on.s);
            Console.WriteLine(string.Format(Ic42, "  {0,-34} {1,6:0.0} ms {2,-12} -> {3,6:0.0} ms {4,-12} {5}", name, off.s * 1000, off.t, on.s * 1000, on.t, ok ? "" : "NOT LONGER"));
        }
        // the gauntlet's own share: boss hits every 0.5 s for 90 s of clock; the frozen gauntlet time in the last rolling minute
        var g = new ImpactRules(cfg) { impact42 = true };
        double perMinute = cfg.Num("design.n42.impact42.gauntletPerMinute") * 60, total = 0;
        var starts = new List<(double at, double s)>();
        for (int k = 0; k < 180; k++)
        {
            g.BeginFrame(k == 0 ? 1 : 0.5, false);
            g.ClearPending();
            g.OnGauntletHit42(new GauntletHit42Event { cls = GauntletHit42Event.Boss });
            if (g.EndStep(false)) starts.Add((g.Clock, g.grantedSeconds));
            g.BeginFrame(g.FreezeLeft, false); // the freeze runs out (real time passes)
        }
        double worst = 0;
        foreach (var (at, _) in starts)
        {
            double sum = 0;
            foreach (var (a, s) in starts) if (a <= at && a > at - 60) sum += s;
            worst = Math.Max(worst, sum);
        }
        foreach (var (_, s) in starts) total += s;
        bool share = worst <= perMinute + 1e-6;
        bool top = bossS > maxOther;
        Console.WriteLine(string.Format(Ic42, "  every moment longer with impact42: {0}", P42(longer)));
        Console.WriteLine(string.Format(Ic42, "  THE ICEBREAKER's gauntlet hit the longest ({0:0} ms vs {1:0} ms the next): {2}", bossS * 1000, maxOther * 1000, P42(top)));
        Console.WriteLine(string.Format(Ic42, "  the gauntlet's share: a boss hit asked every 0.5 s: {0} freezes, {1:0.00} s frozen, the worst rolling minute {2:0.00} s (share {3:0.00} s), cut {4}, skipped {5}: {6}",
            starts.Count, total, worst, perMinute, g.cuts, g.skips[(int)ImpactTier.Gauntlet42], P42(share)));
        return (longer ? 0 : 1) + (top ? 0 : 1) + (share ? 0 : 1);
    }

    // --- the frame loop (Harness.Impact.cs's) ----------------------------------------------------------------------------------------

    sealed class Ledger42
    {
        public readonly int[] count = new int[ImpactRules.TierCount], skips = new int[ImpactRules.TierCount];
        public readonly double[] ms = new double[ImpactRules.TierCount];
        public readonly List<(double start, double s, int tier)> freezes = new List<(double, double, int)>();
        public double real, sim;
        public int cuts, kills;
        public readonly int[] gHits = new int[3], kTiers = new int[5];

        public void Add(ImpactRules r)
        {
            for (int t = 0; t < ImpactRules.TierCount; t++)
            {
                count[t] += r.freezes[t];
                ms[t] += r.frozenSeconds[t] * 1000;
                skips[t] += r.skips[t];
            }
            cuts += r.cuts;
        }

        public void AddSim(SurvivalSim s)
        {
            for (int i = 0; i < 3; i++) gHits[i] += s.gauntletHits42[i];
            for (int i = 0; i < 5; i++) kTiers[i] += s.killTiers42[i];
        }

        public double Worst(double width, int tier = -1)
        {
            double worst = 0;
            for (int i = 0; i < freezes.Count; i++)
            {
                double to = freezes[i].start + freezes[i].s, from = to - width, sum = 0;
                for (int j = i; j >= 0; j--)
                {
                    var f = freezes[j];
                    if (f.start + f.s <= from) break;
                    if (tier >= 0 && f.tier != tier) continue;
                    double lo = Math.Max(f.start, from), hi = Math.Min(f.start + f.s, to);
                    if (hi > lo) sum += hi - lo;
                }
                worst = Math.Max(worst, sum);
            }
            return worst;
        }

        public void Print(string label)
        {
            double total = 0, g = 0;
            foreach (var f in freezes)
            {
                total += f.s;
                if (f.tier == (int)ImpactTier.Gauntlet42) g += f.s;
            }
            double min = real / 60;
            Console.WriteLine(string.Format(Ic42, "  {0}: sim {1:0.0} s, real {2:0.0} s, {3} kills; frozen {4:0.00} s = {5:0} ms per minute ({6:0.0} % of real time); the gauntlet's {7:0} ms per minute; worst second {8:0} ms, worst minute {9:0} ms; cut {10}",
                label, sim, real, kills, total, min > 0 ? total * 1000 / min : 0, real > 0 ? total * 100 / real : 0, min > 0 ? g * 1000 / min : 0, Worst(1) * 1000, Worst(60) * 1000, cuts));
            for (int t = 1; t < ImpactRules.TierCount; t++)
            {
                if (count[t] == 0 && skips[t] == 0) continue;
                Console.WriteLine(string.Format(Ic42, "      {0,-12} {1,5} x {2,8:0.0} ms (mean {3,5:0.0} ms)  skipped {4}", ImpactRules.TierNames[t], count[t], ms[t], count[t] > 0 ? ms[t] / count[t] : 0, skips[t]));
            }
            if (gHits[0] + gHits[1] + gHits[2] + kTiers[1] + kTiers[2] + kTiers[3] + kTiers[4] > 0)
                Console.WriteLine($"      events: gauntletHit42 small {gHits[0]} / big {gHits[1]} / boss {gHits[2]}; killTier42 plain {kTiers[1]} / heavy {kTiers[2]} / melee {kTiers[3]} / big {kTiers[4]}");
        }
    }

    /// <summary>Steps `sim` under `rules` frame by frame until `before` says stop or `capSim` s of sim time; `before(step)` sets the step's input.</summary>
    static void Hit42Drive(SurvivalSim sim, ImpactRules rules, Ledger42 L, double capSim, Func<int, bool> before, Action after)
    {
        const double frameDt = 1.0 / 144;
        double acc = 0, runSim = 0, runReal = 0;
        int step = 0;
        bool done = false;
        while (!done && runSim < capSim - 1e-9)
        {
            if (sim.hitstopLeft > 0) sim.hitstopLeft = 0;
            rules.ClearPending();
            double share = rules.BeginFrame(frameDt, sim.flow != Flow.Playing);
            runReal += frameDt;
            if (share <= 0) continue;
            acc += frameDt * share;
            while (acc >= sim.step)
            {
                if (!before(step)) { done = true; break; }
                sim.hitstopRequested = false;
                sim.Step();
                step++;
                runSim += sim.step;
                acc -= sim.step;
                double simAsk = sim.hitstopRequested ? sim.hitstopLeft : 0;
                sim.hitstopLeft = 0;
                after?.Invoke();
                if (rules.EndStep(sim.flow != Flow.Playing, simAsk))
                {
                    L.freezes.Add((L.real + rules.Clock, rules.grantedSeconds, (int)rules.granted));
                    acc %= sim.step;
                    break;
                }
            }
        }
        L.real += runReal;
        L.sim += runSim;
    }

    static int Hit42Ledger(double seconds)
    {
        GameConfig cfg = Config();
        Console.WriteLine($"n42hit ledger: the AUTOPILOT on the arena, seed 4242 normal, {seconds:0} s of sim time, impact42 off / on:");
        foreach (bool on in new[] { false, true })
        {
            var L = new Ledger42();
            uint seed = 4242;
            while (L.sim < seconds - 1e-9)
            {
                SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
                var ap = new AutopilotInput(sim, "normal", seed);
                sim.UseInput(ap);
                var rules = new ImpactRules(cfg);
                rules.Watch(sim.events);
                sim.events.On<KillEvent>("kill", e => { if (e.owner == "player") L.kills++; });
                sim.setup.mode = RunSetup.Survival;
                sim.setup.heat = 0;
                N42Blocks b = N42Blocks.ForceOff ? N42Blocks.Off : N42Blocks.From(cfg);
                b.impact42 = on;
                sim.n42Override = b;
                sim.StartRun(seed, null, null);
                rules.crunch36 = sim.n36.crunch36;
                rules.impact42 = sim.n42.impact42;
                Hit42Drive(sim, rules, L, seconds - L.sim, _ => { AutopilotInput.TakeDraft(sim); return sim.flow != Flow.Results; }, null);
                L.Add(rules);
                L.AddSim(sim);
                ap.Detach();
                seed++;
            }
            L.Print(on ? "impact42 ON " : "impact42 off");
        }
        return 0;
    }

    static int Hit42Brawl(int seeds)
    {
        Console.WriteLine($"n42hit brawl: THE ICEBREAKER, the skilled pilot (Brawl40Pilot Mixed), seeds 1..{seeds}, impact42 off / on (brawl40 on, the rest as configured):");
        int fails = 0;
        double offPerMin = 0;
        foreach (bool on in new[] { false, true })
        {
            var L = new Ledger42();
            var times = new List<double>();
            for (uint seed = 1; seed <= seeds; seed++)
            {
                SurvivalSim sim = Brawl40Sim(seed, true);
                N42Blocks b = sim.n42;
                b.impact42 = on;
                sim.n42 = b; // after StartRun (Brawl40Sim starts it): the hit hook reads the field live
                var rules = new ImpactRules(sim.cfg);
                rules.Watch(sim.events);
                rules.crunch36 = sim.n36.crunch36;
                rules.impact42 = on;
                sim.events.On<KillEvent>("kill", e => { if (e.owner == "player") L.kills++; });
                double dt = sim.cfg.Dt;
                sim.Respawn(sim.brawl40Arena.spawn + Brawl40At, 0);
                Vec3 spawn = default;
                double yaw = 0;
                if (!sim.Brawl40BeginFor("brawl40", out _, ref spawn, ref yaw)) { Console.WriteLine("  the fight did not begin: FAIL"); return 1; }
                Brawl40 g = sim.brawl40;
                var si = (ScriptedInput)sim.input;
                var pilot = new Brawl40Pilot { hand = new Mulberry32(seed * 7919u + 40u), style = Brawl40Pilot.Style.Mixed };
                int reveal = MovementMath.StepsFor(g.p.revealSeconds, dt), last = 0;
                double yawL = 0;
                Hit42Drive(sim, rules, L, 240 + reveal * dt, step =>
                {
                    if (!g.Alive) return false;
                    pilot.Step(sim, yawL, step);
                    yawL = pilot.Move.yaw;
                    si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
                    si.Set(pilot.Move, pilot.Combat);
                    last = step;
                    return true;
                }, () =>
                {
                    if (sim.flow == Flow.Down || sim.health.Dead)
                    {
                        sim.downSteps = 0;
                        sim.flow = Flow.Playing;
                        sim.health.Reset();
                        sim.Brawl40BeginFor("brawl40", out _, ref spawn, ref yaw);
                        sim.Respawn(spawn, yaw);
                    }
                });
                times.Add(Math.Max(0, last + 1 - reveal) * dt);
                if (g.Alive) fails++;
                L.Add(rules);
                L.AddSim(sim);
            }
            times.Sort();
            Console.WriteLine(string.Format(Ic42, "  impact42 {0}: kill times (sim s) {1}", on ? "ON " : "off", string.Join(" ", times.ConvertAll(t => t.ToString("0.0", Ic42)))));
            L.Print(on ? "impact42 ON " : "impact42 off");
            double total = 0;
            foreach (var f in L.freezes) total += f.s;
            double perMin = L.real > 0 ? total * 60 / L.real : 0;
            if (!on) offPerMin = perMin;
            else
            {
                // sane: at most 12 % of the fight's real time frozen (7.2 s a minute; the shared 6 % + the gauntlet's 8 % share is the ceiling 14 %)
                bool sane = perMin <= 7.2;
                Console.WriteLine(string.Format(Ic42, "  frozen per minute of the fight: {0:0.00} s off -> {1:0.00} s on (sane <= 7.2 s, 12 %): {2}", offPerMin, perMin, P42(sane)));
                if (!sane) fails++;
            }
        }
        return fails;
    }
}
