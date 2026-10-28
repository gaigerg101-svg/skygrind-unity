// NIGHT47 builder L (NIGHT47.md G41-G43, round 0: the baselines before any tuning): THE CROWN's own meters.
// Hooked without a Program.cs line: a module initializer runs the command and exits when argv[1] is "n47crown" (Harness.N46.Flow.cs's head).
//   n47crown base [--seeds N] [--cap S] [--level <id>] [--casual] [--rows r1,r2] [--off47] [--trace]
//     THE CROWN's fight as the ON game plays it today: the level file Design/levels/<id>.json (default n46-spire), the config's rows (no
//     override; L47Sim in Harness.N47.Loom.cs), the player at the spire's summit (-10, 0.02, -267: n40crown's start), BossRoomBegin("crown37"),
//     the existing Crown37Pilot (skilled, or --casual) until the kill or the cap; a death begins the fight again at its spawn (as n40crown).
//     Per seed and pooled: the kill s (after the reveal), passes, tells per attack (count, wind-up s), legs' start times, damage taken, falls,
//     deaths. No PASS / RED (a baseline; the band is the human-model pilot's).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void L47CrownInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n47crown") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "base";
            if (what == "base") code = L47CrownBase(args);
            else { Console.WriteLine("usage: n47crown base [--seeds N] [--cap S] [--level id] [--casual] [--rows r1,r2] [--off47] [--trace]"); code = 1; }
        }
        catch (Exception e) { Console.WriteLine("n47crown: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    sealed class L47CrownRun
    {
        public bool begun, killed;
        public double seconds, taken;
        public int passes, falls, deaths, parries;
        public readonly double[] legAt = { 0, 0, -1, -1 };
        public readonly Dictionary<string, List<double>> tells = new Dictionary<string, List<double>>();
    }

    static int L47CrownBase(string[] args)
    {
        int seeds = int.Parse(L47Arg(args, "--seeds", "10"), L47Inv);
        double cap = double.Parse(L47Arg(args, "--cap", "240"), L47Inv);
        string level = L47Arg(args, "--level", "n46-spire");
        string rows = L47Arg(args, "--rows", "");
        bool casual = Array.IndexOf(args, "--casual") >= 0, trace = Array.IndexOf(args, "--trace") >= 0;
        var runs = new List<L47CrownRun>();
        for (int s = 1; s <= seeds; s++)
        {
            L47CrownRun r = L47CrownFight(level, (uint)s, cap, rows, casual, trace && s == 1);
            runs.Add(r);
            Console.WriteLine($"  seed {s}: {(r.begun ? (r.killed ? "KILLED" : "cap") : "NOT BEGUN")} {r.seconds:0.0} s; legs at {r.legAt[2]:0.0} / {r.legAt[3]:0.0} s; passes {r.passes}, parries {r.parries}; "
                + $"taken {r.taken:0} hp, falls {r.falls}, deaths {r.deaths}");
        }
        var ok = runs.Where(r => r.begun).ToList();
        double Med(IEnumerable<double> v) { var a = v.OrderBy(x => x).ToArray(); return a.Length == 0 ? double.NaN : a.Length % 2 == 1 ? a[a.Length / 2] : (a[a.Length / 2 - 1] + a[a.Length / 2]) / 2; }
        double P90(IEnumerable<double> v) { var a = v.OrderBy(x => x).ToArray(); return a.Length == 0 ? double.NaN : a[Math.Min(a.Length - 1, (int)Math.Ceiling(0.9 * a.Length) - 1)]; }
        var kills = ok.Where(r => r.killed).Select(r => r.seconds).ToList();
        Console.WriteLine($"n47crown base ({level}, {seeds} seeds, cap {cap} s, {(casual ? "casual" : "skilled")}{(rows.Length > 0 ? ", rows " + rows : "")}): begun {ok.Count}, killed {kills.Count}; "
            + $"kill median {Med(kills):0.0} s, p90 {P90(kills):0.0}, min {(kills.Count > 0 ? kills.Min() : double.NaN):0.0}");
        Console.WriteLine($"  passes median {Med(ok.Select(r => (double)r.passes)):0}, taken median {Med(ok.Select(r => r.taken)):0} hp, falls total {ok.Sum(r => r.falls)}, deaths total {ok.Sum(r => r.deaths)}; "
            + $"leg 2 at median {Med(ok.Where(r => r.legAt[2] >= 0).Select(r => r.legAt[2])):0.0} s, leg 3 at {Med(ok.Where(r => r.legAt[3] >= 0).Select(r => r.legAt[3])):0.0} s");
        var all = new Dictionary<string, List<double>>();
        foreach (var r in ok) foreach (var kv in r.tells) { if (!all.ContainsKey(kv.Key)) all[kv.Key] = new List<double>(); all[kv.Key].AddRange(kv.Value); }
        foreach (var kv in all.OrderBy(k => k.Key))
            Console.WriteLine($"  tell {kv.Key}: {kv.Value.Count} over {ok.Count} fights ({kv.Value.Count / Math.Max(1.0, ok.Count):0.0} a fight), wind-up {kv.Value.Min():0.00}-{kv.Value.Max():0.00} s");
        return 0;
    }

    static L47CrownRun L47CrownFight(string level, uint seed, double cap, string rows, bool casual, bool trace)
    {
        var r = new L47CrownRun();
        SurvivalSim sim = L47Sim(level, seed, rows);
        var si = (ScriptedInput)sim.input;
        sim.Respawn(new Vec3(-10, 0.02, -267), 0);
        si.Set(new InputFrame(), new CombatInput { weapon = -1 });
        sim.Step();
        if (!sim.BossRoomBegin("crown37", out Vec3 spawn, out double yaw) || sim.crown37 == null) return r;
        r.begun = true;
        Crown37 c = sim.crown37;
        double dt = sim.step;
        sim.mover.invulnerable = false;
        var pilot = new Crown37Pilot { hand = new Mulberry32(seed * 7919u + 37u), casual = casual };
        int step = 0, revealSteps = MovementMath.StepsFor(c.p.revealSeconds, dt), capSteps = revealSteps + (int)(cap / dt);
        double yawL = 0, hpBefore = sim.health.hp;
        sim.events.On<Boss37Event>(Boss37Brain.EvTell, e =>
        {
            if (e.boss != "crown37") return;
            if (!r.tells.ContainsKey(e.attack)) r.tells[e.attack] = new List<double>();
            r.tells[e.attack].Add(e.seconds);
        });
        sim.events.On<Boss37Event>(Boss37Brain.EvLeg, e => { if (e.boss == "crown37" && e.stage == "change" && e.leg >= 2 && e.leg <= 3 && r.legAt[e.leg] < 0) r.legAt[e.leg] = Math.Max(0, step - revealSteps) * dt; });
        while (c.Alive && !c.Dying && step < capSteps)
        {
            pilot.Step(sim, yawL, step);
            yawL = pilot.Move.yaw;
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            sim.Step();
            sim.hitstopLeft = 0;
            step++;
            if (sim.health.hp < hpBefore) r.taken += hpBefore - sim.health.hp;
            hpBefore = sim.health.hp;
            if (trace && step % 600 == 0)
                Console.WriteLine($"    {Math.Max(0, step - revealSteps) * dt:0.0}s leg {c.Leg} {c.brain.state} {c.brain.Current?.id} hp {c.body.hp:0}/{c.body.maxHp:0} me ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0})");
            if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
            {
                r.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing;
                sim.health.Reset();
                hpBefore = sim.health.hp;
                sim.BossRoomBegin("crown37", out spawn, out yaw);
                sim.Respawn(spawn, yaw);
            }
        }
        r.killed = !c.Alive || c.Dying;
        r.seconds = Math.Max(0, step - revealSteps) * dt;
        r.passes = c.passes;
        r.falls = sim.crown37Falls;
        r.parries = c.parries;
        return r;
    }
}
