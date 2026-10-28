// NIGHT22 leftover 1 (builder M): `arcline2 --directed [--selftest]` (the lead may route a `directed` command to DirectedCommand), DIRECTED STEPS
// for THE BEACON and THE HUNT on the harness (Core/Directed22.cs). The cue is render-side: this feeds it the events of real runs and prints
// what it would show. Prints "directed PASS|FAIL: ..." (exit 1 on FAIL).
//   beacon  a run with the beacon forced, skipped to its break: `beaconStand` gives step 0 with the beacon's spot as the target; the player
//           beside it: step 1; the buy: step 2 with no target, kept through the round's begin; the clear ends the cue
//   hunt    a run with design.adds.hunt on, skipped to a swarm round's break: announce step 0, start step 1, last step 2, end: none; never a target
//   sim     the cue only listens: the same run with and without the listener ends at the same step count, round and score
using System;
using System.Collections.Generic;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void Directed22Wire(SurvivalSim sim, Directed22 d)
    {
        sim.events.On<BeaconEvent>(Beacon20.StandEvent, e => d.BeaconStood(e.round, e.position));
        sim.events.On<BeaconEvent>(Beacon20.TakeEvent, _ => d.BeaconTaken());
        sim.events.On<RoundEvent>("roundBegin", _ => d.RoundBegan());
        sim.events.On<RoundEvent>("roundClear", _ => d.RoundCleared());
        sim.events.On<SpecialRoundEvent>(N20Events.SpecialRound, e => d.Special(e.name, e.round, e.phase));
    }

    static void DirectedCommand(string[] args, ref int code)
    {
        if (Array.IndexOf(args, "--selftest") >= 0)
        {
            int failed = NUnit.Framework.MiniRunner.Run(typeof(Skygrind.Core.Tests.Directed22Tests), Console.WriteLine);
            Console.WriteLine($"directed --selftest {(failed == 0 ? "PASS" : "FAIL")}");
            code = failed == 0 ? 0 : 1;
            return;
        }
        GameConfig cfg = Config();
        SurvivalMap map = Map();
        var fails = new List<string>();
        void Need(bool ok, string what) { if (!ok) fails.Add(what); }

        // --- beacon -------------------------------------------------------------------------------------------------------------
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        Beacon20Run b = sim.Beacon20Ensure();
        b.Force = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(3, null, null);
        var d = new Directed22(cfg);
        Directed22Wire(sim, d);
        Beacon20.P p = Beacon20.P.From(cfg);
        DirectorMath.D dm = DirectorMath.From(cfg);
        int r = p.from;
        while (!Beacon20.Stands(sim.runSeed, r, p, dm) && r < 60) r++;
        sim.run.DebugSkipTo(r);
        string s0 = d.Kind == Directed22Kind.Beacon ? $"step {d.Step}, target ({d.Target.x:0.0}, {d.Target.z:0.0}), round {d.Round}" : "NO CUE";
        Need(d.Kind == Directed22Kind.Beacon && d.Step == 0 && d.HasTarget && Vec3.DistanceTo(d.Target, b.Spot) < 0.01 && d.Round == r, "the beacon's stand did not give step 0 with its spot");
        sim.Respawn(new Vec3(b.Spot.x + 1, b.Spot.y, b.Spot.z), 0);
        for (int s = 0; s < 6; s++) sim.Step();
        d.Update(sim.mover.position);
        int sNear = d.Step;
        Need(sNear == 1, $"beside the beacon the step was {sNear}, not 1");
        b.Item.buy();
        int sTaken = d.Step;
        bool noTarget = !d.HasTarget;
        Need(sTaken == 2 && noTarget, $"after the buy the step was {sTaken}, target gone {noTarget}");
        int guard = 0;
        while (sim.run.state != SurvivalRun.State.Fight && guard++ < 120 * 30) sim.Step();
        bool keptThroughBegin = d.Kind == Directed22Kind.Beacon && d.Step == 2;
        Need(keptThroughBegin, "the lit beacon's cue did not survive its round's begin");
        guard = 0;
        while (d.Active && guard++ < 120 * 120)
        {
            sim.husks.KillAll();
            sim.health.hp = sim.health.max;
            sim.Step();
        }
        Need(!d.Active, "the round's clear did not end the cue");
        Console.WriteLine($"  beacon: round {r}: stood -> {s0}; beside it -> step {sNear}; bought -> step {sTaken}, no target {noTarget}; through the round's begin {keptThroughBegin}; cleared -> {(d.Active ? "STILL ACTIVE" : "ended")}");

        // --- hunt ---------------------------------------------------------------------------------------------------------------------
        SurvivalSim hs = SurvivalSim.Headless(cfg, map);
        hs.noLab = hs.noRecords = true;
        hs.n20Override = new N20Blocks { adds = true, director = true };
        hs.setup.mode = RunSetup.Survival;
        hs.StartRun(5, null, null);
        var hd = new Directed22(cfg);
        Directed22Wire(hs, hd);
        var seen = new List<string>();
        bool targetEver = false;
        hs.events.On<SpecialRoundEvent>(N20Events.SpecialRound, e =>
        {
            if (e.name == "hunt") seen.Add($"{e.phase}->{hd.Step}");
            targetEver |= hd.HasTarget;
        });
        int hr = 3;
        while (!SpecialRoundMath.IsHunt(hr, DirectorMath.From(cfg)) && hr < 60) hr++;
        hs.run.DebugSkipTo(hr);
        guard = 0;
        while (hs.run.state != SurvivalRun.State.Fight && guard++ < 120 * 30) hs.Step();
        guard = 0;
        while (hs.run.state == SurvivalRun.State.Fight && guard++ < 120 * 120)
        {
            hs.health.hp = hs.health.max;
            hs.Step();
            if (guard % 120 == 0) foreach (Enemy e in hs.enemies.all) if (e.alive && guard > 120 * 20) { hs.husks.KillAll(); break; }
        }
        Console.WriteLine($"  hunt: round {hr}: phases seen [{string.Join(", ", seen)}]; a target ever: {targetEver}; cue after the round: {(hd.Active ? "STILL ACTIVE" : "ended")}");
        Need(seen.Count > 0 && seen[0] == "announce->0", seen.Count == 0 ? "no hunt announce (is design.adds.hunt on?)" : $"the hunt's first beat read {seen[0]}");
        Need(!targetEver, "the hunt's cue named a place");
        Need(!hd.Active, "the hunt's end did not end the cue");

        // --- sim: the listener changes nothing -----------------------------------------------------------------------------------------
        string With = DirectedFinger(cfg, map, true), Without = DirectedFinger(cfg, map, false);
        Console.WriteLine($"  sim: with the listener {With}; without {Without}");
        Need(With == Without, "the listener changed the run");

        bool ok = fails.Count == 0;
        Console.WriteLine($"directed {(ok ? "PASS" : "FAIL")}: beacon cue step 0 -> 1 -> 2 -> ended with the beacon's spot as the target until lit; hunt cue announce -> start -> last -> end with no target; the run is identical with the listener{(ok ? "" : "; " + string.Join("; ", fails))}");
        code = ok ? 0 : 1;
    }

    static string DirectedFinger(GameConfig cfg, SurvivalMap map, bool listen)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        sim.Beacon20Ensure().Force = true;
        sim.setup.mode = RunSetup.Survival;
        sim.StartRun(3, null, null);
        if (listen) Directed22Wire(sim, new Directed22(cfg));
        var ap = new AutopilotInput(sim, "normal", 3);
        sim.UseInput(ap);
        for (int i = 0; i < 120 * 90 && sim.flow == Skygrind.Game.Flow.Playing; i++) sim.Step();
        return $"{sim.simStep} steps, round {sim.run.round}, kills {sim.run.stats.kills}, earned {sim.economy.total:0}";
    }
}
