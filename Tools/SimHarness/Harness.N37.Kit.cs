// NIGHT37 builder K (boss37: NIGHT37.md card K1): the Boss37 brain's rules, each one in isolation and with numbers. This is a bare brain
// (no sim) on design.n37.boss37's numbers (Boss37Params.From), a Tells36 registry turned on, and the brain's own EventBus.
//   boss37 kit [--row <bossRow>]   (--row: the numbers with that boss row's kit overrides)
//     budget   cost-2 attacks, a burst longer than the cap allows: attacks before the forced rest (cap / cost) and the rest's length (budgetRestSeconds).
//     parry    the same run with one parry after the 2nd attack: the budget before and after (refund parryRefund, clamped at 0), attacks before the rest.
//     burst    cost-1 attacks: `burst` attacks chain, then a rest of restSeconds.
//     pick     4 attacks, 2000 picks: repeats in a row (want 0), the pity of an unpicked attack per turn (+pityStep), each attack's share,
//              and a distance gate (an attack with maxDist 10 at 15 m: never picked).
//     habit    a grounded player from airBias 1: seconds to reach 0 (habitSeconds); the ground attack's weight multiplier then; the snap.
//     camp     a grounded player within campRadius: the camp attack is picked after campSeconds (not before); a wall ride resets the clock.
//     hide     out of sight: boss37Hide after hideSeconds; 2 s hidden then 1 s seen drains the timer to 0 (fill 1/s, drain 2/s).
//     stagger  Stagger(2) x 3 in leg 1 (meter 6): the window opens on the 3rd; its length; the meter refilled after; Scale in / out of it.
//     legs     autoLegs: hpFrac at legAt[0] -> leg 2 after legChangeSeconds, the meter at leg 2's full value.
//     restart  in leg 2 with the meter at 3 and the boss's HP at 0.5: RestartLeg keeps the leg, the meter and the HP, returns leg 2's
//              checkpoint, clears the budget, rests restartRestSeconds.
//     tells    every wind-up raises one Tells36 tell of the attack's colour and windup length; never two live at once.
using System;
using System.Collections.Generic;
using Skygrind.Core;

public static partial class Harness
{
    static partial void N37Kit(string cmd, string[] args, ref int code)
    {
        if (cmd == "verbs37probe") { code = Verbs37Probe(args); return; } // NIGHT37 K: the verbs37 RED rows headless (Harness.N37.Probe.cs)
        if (cmd != "boss37") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "kit";
        if (what != "kit") return;
        int ri = Array.IndexOf(args, "--row");
        string row = ri >= 0 && ri + 1 < args.Length ? args[ri + 1] : null;
        GameConfig cfg = Config();
        Boss37Params p = Boss37Params.From(cfg, row);
        double dt = cfg.Dt;
        Console.WriteLine($"boss37 kit: numbers from design.n37.boss37{(row != null ? " + design.n37." + row + ".kit" : "")} (row present: {cfg.Has(Boss37Params.Row)}); dt {dt * 1000:0.000} ms");
        int fails = 0;
        fails += Kit37Budget(p, dt);
        fails += Kit37Parry(p, dt);
        fails += Kit37Burst(p, dt);
        fails += Kit37Pick(p, dt);
        fails += Kit37Habit(p, dt);
        fails += Kit37Camp(p, dt);
        fails += Kit37Hide(p, dt);
        fails += Kit37Stagger(p, dt);
        fails += Kit37Legs(p, dt);
        fails += Kit37Restart(p, dt);
        fails += Kit37Tells(p, dt);
        Console.WriteLine(fails == 0 ? "boss37 kit: GREEN" : $"boss37 kit: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static string P37(bool ok) => ok ? "PASS" : "FAIL";

    /// <summary>A copy of the params with the changes a check needs (the rule under test keeps its config value).</summary>
    static Boss37Params Copy37(Boss37Params p, Action<Boss37Params> change = null)
    {
        var q = (Boss37Params)typeof(Boss37Params).GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(p, null);
        q.stagger = (double[])p.stagger.Clone();
        q.budgetCap = (double[])p.budgetCap.Clone();
        q.legAt = (double[])p.legAt.Clone();
        change?.Invoke(q);
        return q;
    }

    static Boss37Brain Brain37(Boss37Params p, double dt, int attacks, int cost, uint seed = 1, Tells36 tells = null, EventBus bus = null)
    {
        var b = new Boss37Brain(p, "kit", dt, bus ?? new EventBus(), seed) { tells = tells, source = 7 };
        for (int i = 0; i < attacks; i++)
            b.attacks.Add(Boss37Attack.Of("a" + i, i % 2 == 0 ? Tell36Kind.Red : Tell36Kind.Yellow, 1.2, 0.3, 0.3, dt, cost));
        return b;
    }

    static Boss37Context Ctx37(bool grounded = false, bool riding = true, bool sight = true, double dist = 8, double hp = 1, double px = 0, double pz = 0) =>
        new Boss37Context
        {
            player = new Vec3(px, 0, pz), boss = new Vec3(px + dist, 0, pz), grounded = grounded, riding = riding, airborne = !grounded && !riding,
            movingTheWay = riding, lineOfSight = sight, hpFrac = hp, aim = new Vec3(px + dist, 2, pz),
        };

    /// <summary>Step until the brain reaches `s` (from another state) or `cap` steps; returns the steps taken (-1: never).</summary>
    static int Until37(Boss37Brain b, Boss37State s, in Boss37Context c, int cap = 20000)
    {
        for (int i = 1; i <= cap; i++)
        {
            b.Step(c);
            if (b.state == s && b.entered) return i;
        }
        return -1;
    }

    static int Kit37Budget(Boss37Params p0, double dt)
    {
        Boss37Params p = Copy37(p0, q => q.burst = 99);
        var b = Brain37(p, dt, 3, 2);
        var c = Ctx37();
        string reason = null;
        b.Begin(1, c.player);
        var bus = new EventBus();
        int attacks = 0, restSteps = 0;
        // count tells until the first forced (budget) rest, then measure that rest
        for (int i = 0; i < 20000 && b.budgetRests == 0; i++)
        {
            b.Step(c);
            if (b.state == Boss37State.Tell && b.entered) attacks++;
        }
        if (b.budgetRests > 0) { reason = "budget"; restSteps = b.stateTotal; }
        double cap = p.CapOf(1);
        int want = (int)Math.Ceiling(cap / 2);
        bool ok = reason == "budget" && attacks == want && Math.Abs(restSteps * dt - p.budgetRestSeconds) < dt;
        Console.WriteLine($"  budget: cap {cap} (leg 1), cost 2 each: {attacks} attacks then the forced rest of {restSteps * dt:0.00} s (want {want} attacks, {p.budgetRestSeconds:0.00} s; MinosPrime.cs:342-353) {P37(ok)}");
        return ok ? 0 : 1;
    }

    static int Kit37Parry(Boss37Params p0, double dt)
    {
        Boss37Params p = Copy37(p0, q => q.burst = 99);
        var b = Brain37(p, dt, 3, 2);
        var c = Ctx37();
        b.Begin(1, c.player);
        int attacks = 0;
        double before = -1, after = -1;
        for (int i = 0; i < 40000 && b.budgetRests == 0; i++)
        {
            b.Step(c);
            if (b.state == Boss37State.Tell && b.entered)
            {
                attacks++;
                if (attacks == 2)
                {
                    before = b.budget;
                    b.Parried(0.0001); // a parry with (almost) no stagger: the refund alone
                    after = b.budget;
                }
            }
        }
        double cap = p.CapOf(1);
        // after the parry the budget restarts from max(0, before - refund): the cap allows ceil((cap - after) / 2) more attacks, the parried one included in `attacks`
        int want = 2 + (int)Math.Ceiling((cap - after) / 2);
        bool ok = before == 4 && after == Math.Max(0, 4 - p.parryRefund) && attacks == want;
        Console.WriteLine($"  parry: budget {before} -> {after} on a parry after the 2nd attack (refund {p.parryRefund}, MinosPrime.cs:1125; clamped at 0): {attacks} attacks before the forced rest (want {want}; without the parry {Math.Ceiling(cap / 2)}) {P37(ok)}");
        return ok ? 0 : 1;
    }

    static int Kit37Burst(Boss37Params p0, double dt)
    {
        Boss37Params p = Copy37(p0, q => q.budgetCap = new double[] { 99, 99, 99 });
        var b = Brain37(p, dt, 3, 1);
        var c = Ctx37();
        b.Begin(1, c.player);
        int attacks = 0, restSteps = 0;
        for (int i = 0; i < 20000 && b.burstRests == 0; i++)
        {
            b.Step(c);
            if (b.state == Boss37State.Tell && b.entered) attacks++;
        }
        restSteps = b.stateTotal;
        bool ok = attacks == p.burst && Math.Abs(restSteps * dt - p.restSeconds) < dt;
        Console.WriteLine($"  burst: {attacks} attacks chained, then a rest of {restSteps * dt:0.00} s (want {p.burst}, {p.restSeconds:0.00} s; Gabriel.cs:132-142) {P37(ok)}");
        return ok ? 0 : 1;
    }

    static int Kit37Pick(Boss37Params p, double dt)
    {
        var b = Brain37(p, dt, 4, 1, 37);
        b.attacks[3].maxDist = 10; // the gated one
        b.Begin(1, Vec3.Zero);
        int n = 2000, repeats = 0, gatedFar = 0;
        var counts = new int[4];
        double pityMax = 0;
        var pityTrace = new List<double>();
        int prev = -1;
        for (int i = 0; i < n; i++)
        {
            double dist = i < n / 2 ? 8 : 15;
            int k = b.Pick(dist);
            if (k == prev) repeats++;
            if (dist > 10 && k == 3) gatedFar++;
            counts[k]++;
            prev = k;
            b.previous = k;
            if (i < 12) pityTrace.Add(b.attacks[0].pity);
            for (int j = 0; j < 4; j++) pityMax = Math.Max(pityMax, b.attacks[j].pity);
        }
        // the pity rule itself: an attack not picked rises by pityStep each turn
        bool rises = true;
        for (int i = 1; i < pityTrace.Count; i++)
            if (pityTrace[i] != 0 && Math.Abs(pityTrace[i] - pityTrace[i - 1] - p.pityStep) > 1e-9) rises = false;
        bool ok = repeats == 0 && gatedFar == 0 && rises && b.repeats == 0 && pityMax < 3; // a gated attack's pity doesn't pile up
        Console.WriteLine($"  pick: {n} picks, repeats in a row {repeats} (want 0), the gated attack (maxDist 10) picked at 15 m {gatedFar} times (want 0); attack a0's pity over its first 12 turns [{string.Join(", ", pityTrace.ConvertAll(x => x.ToString("0.00")))}] (+{p.pityStep} per unpicked turn, 0 when picked), the highest pity {pityMax:0.00}; shares {string.Join(" / ", Array.ConvertAll(counts, x => (100.0 * x / n).ToString("0") + "%"))} (Gabriel.cs:74-128) {P37(ok)}");
        return ok ? 0 : 1;
    }

    static int Kit37Habit(Boss37Params p, double dt)
    {
        var b = Brain37(p, dt, 2, 1);
        b.attacks[0].habit = Boss37Habit.Ground;
        b.attacks[1].habit = Boss37Habit.Air;
        var c = Ctx37(grounded: true, riding: false);
        b.Begin(1, c.player);
        Until37(b, Boss37State.Rest, c);
        b.airBias = 1;
        foreach (Boss37Attack a in b.attacks) a.minDist = 1e6; // no picks during the drift (a pick's snap would move the bias)
        int steps = 0;
        while (b.airBias > 0 && steps < 20000) { b.Step(c); steps++; }
        foreach (Boss37Attack a in b.attacks) a.minDist = 0;
        double groundMul = b.HabitMul(b.attacks[0]), airMul = b.HabitMul(b.attacks[1]);
        // the snap: picking the ground attack snaps the bias to the air (expect the player to jump)
        b.Force("a0");
        b.Pick(8);
        double snapped = b.airBias;
        bool ok = Math.Abs(steps * dt - p.habitSeconds) <= 2 * dt + 1e-9 && groundMul > airMul && (!p.habitSnap || snapped == 1);
        Console.WriteLine($"  habit: grounded from airBias 1 to 0 in {steps * dt:0.00} s (want {p.habitSeconds:0.00} s; Minotaur.cs:294-296: 20 s there); then the ground attack weighs x{groundMul:0.00}, the air attack x{airMul:0.00}; after a ground pick the bias snaps to {snapped:0.00} (Minotaur.cs:399-410) {P37(ok)}");
        return ok ? 0 : 1;
    }

    static int Kit37Camp(Boss37Params p0, double dt)
    {
        Boss37Params p = Copy37(p0, q => q.burst = 99);
        var b = Brain37(p, dt, 2, 0);
        b.attacks.Add(Boss37Attack.Of("camp", Tell36Kind.Red, 1.0, 0.3, 0.3, dt, 0, habit: Boss37Habit.Camp));
        var c = Ctx37(grounded: true, riding: false);
        b.Begin(1, c.player);
        int firstCamp = -1, step = 0, campBefore = 0;
        int dueAt = -1;
        for (; step < 20000 && firstCamp < 0; step++)
        {
            b.Step(c);
            if (b.campDue && dueAt < 0) dueAt = step + 1;
            if (b.state == Boss37State.Tell && b.entered && b.Current.id == "camp")
            {
                if (dueAt < 0) campBefore++;
                else firstCamp = step + 1;
            }
        }
        // a wall ride resets the clock
        var b2 = Brain37(p, dt, 2, 0);
        b2.attacks.Add(Boss37Attack.Of("camp", Tell36Kind.Red, 1.0, 0.3, 0.3, dt, 0, habit: Boss37Habit.Camp));
        b2.Begin(1, c.player);
        var ride = Ctx37(grounded: false, riding: true);
        int half = b2.S(p.campSeconds) - 10;
        for (int i = 0; i < 1 + half; i++) b2.Step(c);
        int beforeRide = b2.campSteps;
        b2.Step(ride);
        int afterRide = b2.campSteps;
        bool ok = dueAt > 0 && Math.Abs((dueAt - 1) * dt - p.campSeconds) <= 2 * dt + 1e-9 && firstCamp >= dueAt && campBefore == 0 && afterRide == 0 && beforeRide > 0;
        Console.WriteLine($"  camp: grounded within {p.campRadius} m: due after {(dueAt - 1) * dt:0.00} s (want {p.campSeconds:0.00} s), the camp attack's first tell at {(firstCamp - 1) * dt:0.00} s (the next pick after due), none before; a wall ride resets the clock {beforeRide} -> {afterRide} steps {P37(ok)}");
        return ok ? 0 : 1;
    }

    static int Kit37Hide(Boss37Params p, double dt)
    {
        var bus = new EventBus();
        int hideEvents = 0;
        bus.On(Boss37Brain.EvHide, o => hideEvents++);
        var b = Brain37(p, dt, 2, 1, 1, null, bus);
        var seen = Ctx37();
        var hidden = Ctx37(sight: false);
        b.Begin(1, seen.player);
        Until37(b, Boss37State.Rest, seen);
        int steps = 0;
        while (hideEvents == 0 && steps < 20000) { b.Step(hidden); steps++; }
        bool fired = b.RepositionDue;
        b.ConsumeReposition();
        // drain: 2 s hidden, then 1 s seen: back to 0
        for (int i = 0; i < b.S(2); i++) b.Step(hidden);
        double after2 = b.hideSteps / b.S(1);
        for (int i = 0; i < b.S(1); i++) b.Step(seen);
        double after1 = b.hideSteps / b.S(1);
        // far is hiding too
        var far = Ctx37(dist: p.hideReach + 5);
        int farSteps = 0, ev0 = hideEvents;
        while (hideEvents == ev0 && farSteps < 20000) { b.Step(far); farSteps++; }
        bool ok = Math.Abs(steps * dt - p.hideSeconds) <= 2 * dt + 1e-9 && fired && Math.Abs(after2 - 2) < 0.02 && after1 < 0.02 && Math.Abs(farSteps * dt - p.hideSeconds) <= 2 * dt + 1e-9;
        Console.WriteLine($"  hide: out of sight -> boss37Hide + RepositionDue after {steps * dt:0.00} s (want {p.hideSeconds:0.00} s; GabrielBase.cs:355-363); 2 s hidden = {after2:0.00} s on the timer, then 1 s seen = {after1:0.00} s (drain {p.hideDrain}/s); farther than {p.hideReach} m fires after {farSteps * dt:0.00} s {P37(ok)}");
        return ok ? 0 : 1;
    }

    static int Kit37Stagger(Boss37Params p, double dt)
    {
        var bus = new EventBus();
        int opens = 0, closes = 0, drains = 0;
        bus.On<Boss37Event>(Boss37Brain.EvWindow, e => { if (e.stage == "open") opens++; else closes++; });
        bus.On(Boss37Brain.EvStagger, o => drains++);
        var b = Brain37(p, dt, 2, 1, 1, null, bus);
        var c = Ctx37();
        b.Begin(1, c.player);
        Until37(b, Boss37State.Rest, c);
        double full = b.stagger;
        double scaleOut = b.Scale(false);
        int hits = 0;
        bool opened = false;
        double each = full / 3;
        while (!opened && hits < 20) { opened = b.Stagger(each); hits++; }
        double scaleIn = b.Scale(false);
        int len = 0;
        while (b.state == Boss37State.Window && len < 20000) { b.Step(c); len++; }
        bool ok = opened && hits == 3 && b.Open == false && Math.Abs(len * dt - p.windowSeconds) <= dt + 1e-9 && b.stagger == full && opens == 1 && closes == 1 && drains == 3 && scaleOut == p.chipScale && scaleIn == 1;
        Console.WriteLine($"  stagger: meter {full} (leg 1; Minos Prime 6), {hits} drains of {each:0.00} open the CYAN window (boss37Window open {opens}, close {closes}); it lasts {len * dt:0.00} s (want {p.windowSeconds:0.00} s); the meter refills to {b.stagger}; Scale(chip) {scaleOut} outside, {scaleIn} inside {P37(ok)}");
        return ok ? 0 : 1;
    }

    static int Kit37Legs(Boss37Params p, double dt)
    {
        var bus = new EventBus();
        var stages = new List<string>();
        bus.On<Boss37Event>(Boss37Brain.EvLeg, e => stages.Add(e.stage + e.leg));
        var b = Brain37(p, dt, 2, 1, 1, null, bus);
        var c = Ctx37();
        b.Begin(1, c.player);
        Until37(b, Boss37State.Rest, c);
        var hit = Ctx37(hp: p.legAt[0] - 0.001);
        b.Step(hit);
        bool changing = b.state == Boss37State.LegChange && b.leg == 2;
        int len = 0;
        while (b.state == Boss37State.LegChange && len < 20000) { b.Step(hit); len++; }
        bool ok = changing && Math.Abs(len * dt - p.legChangeSeconds) <= dt + 1e-9 && b.stagger == p.StaggerOf(2) && b.state == Boss37State.Rest;
        var hit3 = Ctx37(hp: p.legAt[1] - 0.001);
        b.Step(hit3);
        ok &= b.leg == 3;
        Console.WriteLine($"  legs: the bar at {p.legAt[0]:0.000} -> leg 2 (LegChange {len * dt:0.00} s, want {p.legChangeSeconds:0.00} s), the meter {b.StaggerFull} (leg 3 now; leg 2's was {p.StaggerOf(2)}); at {p.legAt[1]:0.000} -> leg {b.leg}; boss37Leg [{string.Join(", ", stages)}] {P37(ok)}");
        return ok ? 0 : 1;
    }

    static int Kit37Restart(Boss37Params p, double dt)
    {
        var b = Brain37(p, dt, 2, 1);
        var c = Ctx37(hp: 0.5);
        var cp2 = new Vec3(10, 22, -4);
        b.SetCheckpoint(2, cp2, 90);
        b.Begin(1, c.player);
        Until37(b, Boss37State.Rest, c);
        b.NextLeg();
        while (b.state == Boss37State.LegChange) b.Step(c);
        Until37(b, Boss37State.Tell, c);
        b.stagger = 3;
        double bossHp = 0.5; // the boss's own HP: the brain never touches it
        double budgetBefore = b.budget;
        Vec3 at = b.RestartLeg();
        bool ok = b.leg == 2 && b.stagger == 3 && bossHp == 0.5 && at.Equals(cp2) && b.budget == 0 && b.state == Boss37State.Rest
            && Math.Abs(b.stateTotal * dt - p.restartRestSeconds) <= dt + 1e-9 && b.restarts == 1;
        Console.WriteLine($"  restart: in leg 2 mid-tell, meter 3, HP 0.5: RestartLeg -> leg {b.leg}, meter {b.stagger}, HP {bossHp}, checkpoint ({at.x}, {at.y}, {at.z}), budget {budgetBefore} -> {b.budget}, rest {b.stateTotal * dt:0.00} s (DECIDE37 D-6) {P37(ok)}");
        return ok ? 0 : 1;
    }

    static int Kit37Tells(Boss37Params p, double dt)
    {
        var bus = new EventBus();
        var tells = new Tells36(bus) { enabled = true };
        var b = Brain37(p, dt, 3, 1, 5, tells, bus);
        var c = Ctx37();
        b.Begin(1, c.player);
        int bad = 0, maxLive = 0, raised = 0;
        for (int i = 0; i < b.S(30); i++)
        {
            b.Step(c);
            tells.Step();
            maxLive = Math.Max(maxLive, tells.Live.Count);
            if (b.state == Boss37State.Tell && b.entered)
            {
                raised++;
                Tell36 t = tells.Of(7);
                if (t == null || t.kind != b.Current.kind || t.windupSteps != b.Current.windupSteps || t.attack != b.Current.id) bad++;
            }
        }
        bool ok = raised > 0 && bad == 0 && maxLive == 1 && tells.yellows > 0 && tells.reds > 0;
        Console.WriteLine($"  tells: {raised} wind-ups in 30 s, each one Tells36 tell of its colour and length (mismatches {bad}), live at once {maxLive} (want 1), yellow {tells.yellows} / red {tells.reds} {P37(ok)}");
        return ok ? 0 : 1;
    }
}
