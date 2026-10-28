// NIGHT41 builder B8 (NIGHT41.md B8): the bosses' harness (the N41Bosses partial, Program.cs).
//   n41loom sweep [--seeds n] [--cap s] [--casual] [--off] [--trace]
//       THE LOOM's fight on n40-span (loom40 on, as NIGHT40's game) with sweep41 on (or --off: the root cause, no sweep exists), the stay pilot
//       (Loom37Pilot; --casual its casual person): per seed killed / seconds / sweeps told / hit / dodged by ride, rope, height / deaths / the
//       weak point's live share; the summary checks every fight killed and the median in the bot band (30-60 s, NIGHT40 D-1).
//   n41loom root     the attacks THE LOOM can pick and where the pilot stood when each landed or missed (sweep41 off): the root cause line.
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void N41Bosses(string cmd, string[] args, ref int code)
    {
        if (cmd != "n41loom") return;
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "sweep";
        int fails = 0;
        Loom40Casual = Array.IndexOf(args, "--casual") >= 0;
        Loom41Force = Array.IndexOf(args, "--force") >= 0;
        int ri = Array.IndexOf(args, "--read");
        if (ri >= 0 && ri + 1 < args.Length) Loom37Pilot.sweepRead = Loom37Pilot.sweepReadCasual = double.Parse(args[ri + 1], System.Globalization.CultureInfo.InvariantCulture);
        if (what == "sweep") fails += Loom41Sweep(args, Array.IndexOf(args, "--off") < 0);
        else if (what == "root") fails += Loom41Sweep(args, false);
        else { Console.WriteLine("usage: n41loom sweep [--seeds n] [--cap s] [--casual] [--off] [--trace] | n41loom root"); fails = 1; }
        Console.WriteLine(fails == 0 ? "n41loom: GREEN" : $"n41loom: RED ({fails} failing)");
        code = fails == 0 ? 0 : 1;
    }

    static bool Loom41Force;

    sealed class Loom41Run
    {
        public bool begun, killed;
        public double seconds, weakShare;
        public int deaths, sweeps, hits, ride, rope, high, lashes, lashHits, spools, spoolHits, webs, groundedSteps, steps, hpLostSweep;
        public string attacks;
    }

    static Loom41Run Loom41Fight(uint seed, bool sweep, double cap, bool trace)
    {
        var r = new Loom41Run();
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Loom40Level(true))));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        sim.n40Override = new N40Blocks { loom40 = true };
        sim.n41Override = new N41Blocks { sweep41 = sweep };
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var start = sim.loom37Ledges.Find(l => l.id == "start");
        if (start == null) return r;
        sim.Respawn(new Vec3(start.at.x, 0, start.at.z), 0);
        for (int i = 0; i < 10; i++) sim.Step();
        r.begun = sim.BossRoomBegin("loom37", out _, out _);
        Loom37 L = sim.loom37;
        if (!r.begun || L == null || !L.Alive) return r;
        r.attacks = string.Join(",", L.brain.attacks.ConvertAll(a => a.id));
        var si = (ScriptedInput)sim.input;
        var pilot = new Loom37Pilot { casual = Loom40Casual };
        int st = 0, capSteps = (int)(cap * 120), killStep = -1, weak = 0;
        sim.events.On<Loom37Event>("loom37Down", e => { if (killStep < 0) killStep = st; });
        int sweepHits = 0, legAt = 0, lastLeg = 1;
        var toldLegs = new HashSet<int>();
        var forcedLegs = new HashSet<int>();
        sim.events.On<Loom37Event>("loom41Sweep", e =>
        {
            if (trace) Console.WriteLine($"    {st / 120.0:0.0}s sweep {e.stage} leg {e.leg} me ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}) state {sim.mover.state} rope {sim.mover.rope.phase} top {L.sweepTopY:0} band z {L.sweepZ0:0}..{L.sweepZ1:0} ch {sim.grappleCharges32} pilot sweepDodges {pilot.sweepDodges} fpullLegacy {pilot.fightPulls}");
            if (e.stage == "hit") sweepHits++;
            if (e.stage == "tell") toldLegs.Add(e.leg);
        });
        while (st < capSteps && L.Alive)
        {
            if (sim.health.hp <= 0 || sim.flow != Skygrind.Game.Flow.Playing)
            {
                sim.health.hp = sim.health.max;
                r.deaths++;
                var back = sim.loom37LastLedge ?? start;
                sim.Respawn(new Vec3(back.at.x, back.y, back.at.z), 0);
                L.brain.RestartLeg();
                if (sim.flow != Skygrind.Game.Flow.Playing) break;
            }
            // --force: the loom37 reel's queue (FeelReel.N37.Loom.cs): one sweep 1 s into legs 1 and 2 if that leg has told none
            if (Loom41Force && L.sweep41 && L.brain.leg <= 2 && L.brain.Fighting && st - legAt >= 120 && !forcedLegs.Contains(L.brain.leg) && !toldLegs.Contains(L.brain.leg))
            { forcedLegs.Add(L.brain.leg); L.brain.Force(Loom37.Sweep); }
            if (L.brain.leg != lastLeg) { lastLeg = L.brain.leg; legAt = st; }
            pilot.Step(sim, st);
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            st++;
            if (L.body.weakPoint41.active) weak++;
            if (trace && L.SweepTelling && st % 12 == 0)
                Console.WriteLine($"      tell {L.brain.Progress:0.00} me ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}) {sim.mover.state} rope {sim.mover.rope.phase} ch {sim.grappleCharges32} gr {sim.mover.grounded}");
            if (sim.mover.grounded) r.groundedSteps++;
            if (!L.Alive) break;
        }
        r.killed = !L.Alive || L.Falling;
        r.attacks += $" (sweep tells in legs [{string.Join(",", toldLegs)}], queued [{string.Join(",", forcedLegs)}])";
        r.steps = st;
        r.seconds = (killStep >= 0 ? killStep : st) * sim.step;
        r.weakShare = st > 0 ? weak / (double)st : 0;
        r.sweeps = L.sweeps; r.hits = L.sweepHits; r.ride = L.sweepRide; r.rope = L.sweepRope; r.high = L.sweepHigh;
        r.lashes = L.lashes; r.lashHits = L.lashHits; r.spools = L.spoolsShot; r.spoolHits = L.spoolHits; r.webs = L.webs;
        return r;
    }

    static int Loom41Sweep(string[] args, bool on)
    {
        int seeds = ArgInt36(args, "--seeds", 10);
        double cap = ArgInt36(args, "--cap", 180);
        bool trace = Array.IndexOf(args, "--trace") >= 0;
        int killed = 0, begun = 0, sweeps = 0, hits = 0, ride = 0, rope = 0, high = 0, deaths = 0;
        var times = new List<double>();
        string attacks = null;
        for (int s = 1; s <= seeds; s++)
        {
            Loom41Run r = Loom41Fight((uint)s, on, cap, trace && s == ArgInt36(args, "--first", 1));
            attacks = r.attacks;
            if (r.begun) begun++;
            if (r.killed) { killed++; times.Add(r.seconds); }
            sweeps += r.sweeps; hits += r.hits; ride += r.ride; rope += r.rope; high += r.high; deaths += r.deaths;
            Console.WriteLine($"  seed {s}: {(r.begun ? "" : "NOT begun, ")}{(r.killed ? "killed" : "NOT killed")} {r.seconds:0.0} s, deaths {r.deaths}; sweeps {r.sweeps}: hit {r.hits}, dodged ride {r.ride} / rope {r.rope} / high {r.high} (band missed {r.sweeps - r.hits - r.ride - r.rope - r.high}); lash {r.lashHits}/{r.lashes}, spool {r.spoolHits}/{r.spools}, webs {r.webs}; grounded {r.groundedSteps * 100.0 / Math.Max(1, r.steps):0} %; weak point live {r.weakShare * 100:0} %; {r.attacks}");
        }
        times.Sort();
        double med = times.Count == 0 ? double.NaN : times[times.Count / 2];
        bool band = med >= 30 && med <= 60;
        bool ok = begun == seeds && killed == seeds && band && (!on || sweeps > 0);
        Console.WriteLine($"n41loom sweep (sweep41 {(on ? "ON" : "off")}, n40-span, {(Loom40Casual ? "casual" : "stay")} pilot, {seeds} seeds, cap {cap:0} s): attacks [{attacks}]; killed {killed}/{seeds}, median {med:0.0} s (band 30-60 s: {(band ? "IN" : "OUT")}), deaths {deaths}; sweeps {sweeps}: hit {hits}, dodged ride {ride} / rope {rope} / high {high}: {(ok ? "PASS" : "FAIL")}");
        return ok ? 0 : 1;
    }
}
