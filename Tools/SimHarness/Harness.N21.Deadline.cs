// NIGHT21 U3-35.6 DEADLINE (builder S owns this file): `deadline [--seed s] [--round r] [--seconds n] [--tag t]`: one short headless
// autopilot run (design.n21.deadline on, every other NIGHT21 block off) with the DEADLINE twist forced on round r (default 2),
// run to that round's clear (or the cap). It prints what the fuse did: its length, the ticks, the refills (and which kill paid
// them), the burns (each one's HP), the HP end, the round's time and points, and that the row off (the same seed and forced
// round) never starts a fuse. The lead's `chase --twist deadline` (the KITE bot at round 10) is the survival check.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    sealed class DeadlineRun
    {
        public bool started, ended;
        public int ticks, refills, burns;
        public double fuse, hpLost, hpEnd, startPoints, endPoints, startTime, endTime, firstBurnAt = -1;
        public readonly List<double> hits = new List<double>();
        public int steps;
        public bool dead;
    }

    static DeadlineRun DeadlineOne(GameConfig cfg, SurvivalMap map, uint seed, int round, double maxSeconds, bool on)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.n21Override = new N21Blocks { deadline = on };
        var ap = new AutopilotInput(sim, "normal", seed);
        ap.TellTheTruth();
        sim.UseInput(ap);
        sim.setup.mode = RunSetup.Survival;
        sim.setup.heat = 0;
        var r = new DeadlineRun();
        sim.events.On<SpecialRoundEvent>(N20Events.SpecialRound, e =>
        {
            if (e.name != "twist:" + Deadline21.TwistId) return;
            if (e.phase == "start")
            {
                r.started = true;
                r.startPoints = sim.economy.total;
                r.startTime = sim.SimTime;
                r.fuse = 0; // read from the first tick
            }
            else if (e.phase == "end")
            {
                r.ended = true;
                r.endPoints = sim.economy.total;
                r.endTime = sim.SimTime;
                r.hpEnd = sim.health.hp;
            }
        });
        sim.events.On<DeadlineEvent>(Deadline21.Tick, e =>
        {
            if (r.fuse == 0) r.fuse = e.full;
            if (e.refill) r.refills++;
            else r.ticks++;
        });
        sim.events.On<DeadlineEvent>(Deadline21.Burn, e =>
        {
            r.burns++;
            r.hpLost += e.hit;
            r.hits.Add(e.hit);
            if (r.firstBurnAt < 0) r.firstBurnAt = sim.SimTime - r.startTime;
        });
        sim.StartRun(seed, null, null);
        sim.specials.ForceTwist(round, Deadline21.TwistId);
        int maxSteps = MovementMath.StepsFor(maxSeconds, sim.step);
        while (r.steps < maxSteps && sim.flow != Skygrind.Game.Flow.Results && !(r.started && r.ended))
        {
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            r.steps++;
        }
        r.dead = sim.flow == Skygrind.Game.Flow.Results && !r.ended;
        if (r.started && !r.ended) r.hpEnd = sim.health.hp;
        return r;
    }

    static partial void DeadlineCommand(string[] args, ref int code)
    {
        uint seed = uint.Parse(StampArg(args, "--seed", "1"));
        int round = int.Parse(StampArg(args, "--round", "2"));
        double maxSeconds = double.Parse(StampArg(args, "--seconds", "300"), System.Globalization.CultureInfo.InvariantCulture);
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var t = System.Diagnostics.Stopwatch.StartNew();
        DeadlineRun on = DeadlineOne(cfg, map, seed, round, maxSeconds, true);
        DeadlineRun off = DeadlineOne(cfg, map, seed, round, maxSeconds, false);
        double span = on.ended ? on.endTime - on.startTime : (on.steps * cfg.Dt - on.startTime);
        string hits = on.hits.Count > 0 ? string.Join("/", on.hits.ConvertAll(h => h.ToString("0.#"))) : "none";
        Console.WriteLine($"deadline seed {seed} round {round}: fuse {on.fuse:0.0} s at the start; round {(on.ended ? "cleared" : on.dead ? "ended by a down" : "not finished")} in {span:0.0} s; ticks {on.ticks}, refills {on.refills}, burns {on.burns} (HP {hits}; first at {(on.firstBurnAt < 0 ? "never" : on.firstBurnAt.ToString("0.0") + " s")}), HP at the end {on.hpEnd:0.0}; points in the round {on.endPoints - on.startPoints:0}");
        bool onOk = on.started && on.ticks > 0 && (on.ended || on.dead);
        bool offOk = off.ticks == 0 && off.burns == 0 && off.refills == 0;
        Console.WriteLine($"deadline: the fuse started {(on.started ? "PASS" : "FAIL")}, ticked and ended {(onOk ? "PASS" : "FAIL")}; the row off (the same seed, the twist forced) never runs a fuse {(offOk ? "PASS" : "FAIL")}; {t.Elapsed.TotalSeconds:0.0} s wall");
        // the idle fuse (no kills, 90 s on a bare bus): when it burns, what each burn costs, and that it never kills
        var bus = new EventBus();
        var hp = new Health(cfg.Num("health.max"), bus, "player");
        var idle = new Deadline21(cfg, bus, hp, null);
        var at = new List<string>();
        double clock = 0;
        bus.On<DeadlineEvent>(Deadline21.Burn, e => at.Add($"{clock:0.0} s -{e.hit:0.#}"));
        idle.NewRun(true);
        bus.Emit(N20Events.SpecialRound, new SpecialRoundEvent { name = "twist:" + Deadline21.TwistId, round = 9, phase = "start" });
        for (int i = 0; i < MovementMath.StepsFor(90, cfg.Dt); i++)
        {
            clock = (i + 1) * cfg.Dt;
            idle.Step();
        }
        bool idleOk = at.Count >= 3 && !hp.Dead && hp.hp >= 1 - 1e-9;
        Console.WriteLine($"deadline idle fuse (no kills, 90 s): burns at {string.Join(", ", at)}; HP left {hp.hp:0.#}, alive {(!hp.Dead ? "yes" : "NO")} {(idleOk ? "PASS" : "FAIL")}");
        code = on.started && onOk && offOk && idleOk ? 0 : 1;
    }
}
