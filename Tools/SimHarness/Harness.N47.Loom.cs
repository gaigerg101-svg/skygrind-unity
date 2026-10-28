// NIGHT47 builder L (NIGHT47.md G36-G40, round 0: the baselines before any tuning, NIGHT5's lesson): THE LOOM's own meters.
// Hooked without a Program.cs line: a module initializer runs the command and exits when argv[1] is "n47loom" (Harness.N46.Flow.cs's head).
//   n47loom base [--seeds N] [--cap S] [--level <id>] [--casual] [--seam] [--rows r1,r2] [--off47] [--trace]
//     --seam: the seam policy (loom47) over the pilot: inside a BEND with a knee latched, shoot that knee
//     THE LOOM's fight as the ON game plays it today: the level file Design/levels/<id>.json (default n46-span), the config's rows (no
//     override), the player on the canyon's start ledge, BossRoomBegin("loom37"), the existing Loom37Pilot (NIGHT37 / NIGHT40) at the
//     controls until the kill or the cap; a death restarts the leg on the last ledge at full health (as n40loom stay). Per seed and pooled:
//       kill s       the moment it falls (loom37Down), from the begin
//       bends        bend34Start events (tier short / full) per leg (the BEND's uses: G40 wants >= 3 per phase for the human pilot)
//       latches      RopeAttach events per leg (every pull / whip / knee hook)
//       tells        per attack: count and the wind-up seconds (boss37Tell's seconds)
//       windows      CYAN windows opened; tears (pull / LT); spool parries
//       damage       to the player (lash / spool / sweep / falls); falls; deaths
//       dealt        bossHit damage in the window vs outside (chip)
//     Prints the medians; no PASS / RED (a baseline). The human-model pilot (the lead's n47human) is the band's meter; this is the old bot's read.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void L47LoomInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n47loom") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "base";
            if (what == "base") code = L47LoomBase(args);
            else { Console.WriteLine("usage: n47loom base [--seeds N] [--cap S] [--level id] [--casual] [--rows r1,r2] [--off47] [--trace]"); code = 1; }
        }
        catch (Exception e) { Console.WriteLine("n47loom: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static readonly CultureInfo L47Inv = CultureInfo.InvariantCulture;

    static string L47Arg(string[] args, string key, string def)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
    }

    /// <summary>One headless campaign sim on a level file with the config's rows (and any --rows forced on).</summary>
    public static SurvivalSim L47Sim(string levelId, uint seed, string rowsCsv)
    {
        GameConfig cfg = Config();
        string path = Path.Combine(MapGenRepo, "Design", "levels", levelId + ".json");
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        if (!string.IsNullOrEmpty(rowsCsv))
        {
            var set = sim.rowsRegistry.Defaults();
            foreach (string r in rowsCsv.Split(',')) if (r.Length > 0) set = set.Set(r, true);
            sim.rowsOverride = set;
        }
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        return sim;
    }

    sealed class L47LoomRun
    {
        public bool begun, killed;
        public double seconds, taken, falls, dealtWin, dealtChip;
        public int deaths, windows, tearsPull, tearsLt, parries;
        public readonly int[] bends = new int[4], latches = new int[4], full = new int[4];
        public int seamTears, dives, diveHits, cages, cageHits, bobbins, bobbinHits, bendRefills;
        public readonly Dictionary<string, List<double>> tells = new Dictionary<string, List<double>>();
        public readonly List<string> legLog = new List<string>();
    }

    static int L47LoomBase(string[] args)
    {
        int seeds = int.Parse(L47Arg(args, "--seeds", "10"), L47Inv);
        double cap = double.Parse(L47Arg(args, "--cap", "240"), L47Inv);
        string level = L47Arg(args, "--level", "n46-span");
        string rows = L47Arg(args, "--rows", "");
        bool casual = Array.IndexOf(args, "--casual") >= 0, trace = Array.IndexOf(args, "--trace") >= 0;
        L47Seam = Array.IndexOf(args, "--seam") >= 0;
        L47TraceSeed = int.Parse(L47Arg(args, "--trace-seed", "1"), L47Inv);
        var runs = new List<L47LoomRun>();
        for (int s = 1; s <= seeds; s++)
        {
            L47LoomRun r = L47LoomFight(level, (uint)s, cap, rows, casual, trace && s == L47TraceSeed);
            runs.Add(r);
            Console.WriteLine($"  seed {s}: {(r.begun ? (r.killed ? "KILLED" : "cap") : "NOT BEGUN")} {r.seconds:0.0} s; bends leg1/2/3 {r.bends[1]}/{r.bends[2]}/{r.bends[3]} (full {r.full[1]}/{r.full[2]}/{r.full[3]}), latches {r.latches[1]}/{r.latches[2]}/{r.latches[3]}; "
                + $"windows {r.windows}, tears pull {r.tearsPull} LT {r.tearsLt}, parries {r.parries}; taken {r.taken:0} hp, falls {r.falls:0}, deaths {r.deaths}; dealt window {r.dealtWin:0} chip {r.dealtChip:0}; loom47: seam tears {r.seamTears}, dives {r.dives} (hit {r.diveHits}), cages {r.cages} (hit {r.cageHits}), bobbins {r.bobbins} (hit {r.bobbinHits}), bend refills {r.bendRefills}");
            if (trace && s == L47TraceSeed) foreach (string l in r.legLog) Console.WriteLine("    " + l);
        }
        var ok = runs.Where(r => r.begun).ToList();
        double Med(IEnumerable<double> v) { var a = v.OrderBy(x => x).ToArray(); return a.Length == 0 ? double.NaN : a.Length % 2 == 1 ? a[a.Length / 2] : (a[a.Length / 2 - 1] + a[a.Length / 2]) / 2; }
        double P90(IEnumerable<double> v) { var a = v.OrderBy(x => x).ToArray(); return a.Length == 0 ? double.NaN : a[Math.Min(a.Length - 1, (int)Math.Ceiling(0.9 * a.Length) - 1)]; }
        var kills = ok.Where(r => r.killed).Select(r => r.seconds).ToList();
        Console.WriteLine($"n47loom base ({level}, {seeds} seeds, cap {cap} s{(casual ? ", casual" : "")}{(rows.Length > 0 ? ", rows " + rows : "")}): begun {ok.Count}, killed {kills.Count}; "
            + $"kill median {Med(kills):0.0} s, p90 {P90(kills):0.0}, min {(kills.Count > 0 ? kills.Min() : double.NaN):0.0}");
        for (int leg = 1; leg <= 3; leg++)
            Console.WriteLine($"  leg {leg}: bends median {Med(ok.Select(r => (double)r.bends[leg])):0.0} (min {ok.Select(r => r.bends[leg]).DefaultIfEmpty().Min()}), latches median {Med(ok.Select(r => (double)r.latches[leg])):0.0}, FULL bends median {Med(ok.Select(r => (double)r.full[leg])):0.0} (min {ok.Select(r => r.full[leg]).DefaultIfEmpty().Min()})");
        Console.WriteLine($"  taken median {Med(ok.Select(r => r.taken)):0} hp, deaths total {ok.Sum(r => r.deaths)}, falls total {ok.Sum(r => r.falls):0}; windows median {Med(ok.Select(r => (double)r.windows)):0}; "
            + $"dealt window median {Med(ok.Select(r => r.dealtWin)):0}, chip median {Med(ok.Select(r => r.dealtChip)):0}");
        var all = new Dictionary<string, List<double>>();
        foreach (var r in ok) foreach (var kv in r.tells) { if (!all.ContainsKey(kv.Key)) all[kv.Key] = new List<double>(); all[kv.Key].AddRange(kv.Value); }
        foreach (var kv in all.OrderBy(k => k.Key))
            Console.WriteLine($"  tell {kv.Key}: {kv.Value.Count} over {ok.Count} fights ({kv.Value.Count / Math.Max(1.0, ok.Count):0.0} a fight), wind-up {kv.Value.Min():0.00}-{kv.Value.Max():0.00} s");
        return 0;
    }

    /// <summary>--seam: the seam policy over Loom37Pilot (loom47's rule; the card's human policy): inside a BEND with a knee latched (hooked,
    /// waiting), look at that knee and fire (the pilot's gun, a press every 4 steps), until it tears or the bend ends.</summary>
    static bool L47Seam;
    static int L47TraceSeed = 1;

    static void L47SeamPolicy(SurvivalSim sim, Loom37 L, Loom37Pilot pilot, int st)
    {
        if (!L.loom47 || !sim.Bend34Inside || L.Open) return;
        int k = -1;
        for (int i = 0; i < 6; i++) if (L.Latched40(i)) { k = i; break; }
        if (k < 0) return;
        Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
        (pilot.Move.yaw, pilot.Move.pitch) = Loom37Pilot.LookAt(eye, L.knees[k].at);
        pilot.Combat.fire = new Button { held = true, pressed = st % 4 == 0 };
    }

    static L47LoomRun L47LoomFight(string level, uint seed, double cap, string rows, bool casual, bool trace)
    {
        var r = new L47LoomRun();
        SurvivalSim sim = L47Sim(level, seed, rows);
        var start = sim.loom37Ledges.Find(l => l.id == "start");
        if (start == null) { Console.WriteLine($"  {level}: no loom37 start ledge (Loom37Level {sim.Loom37Level})"); return r; }
        sim.Respawn(new Vec3(start.at.x, 0, start.at.z), 0);
        for (int i = 0; i < 10; i++) sim.Step();
        r.begun = sim.BossRoomBegin("loom37", out _, out _);
        Loom37 L = sim.loom37;
        if (!r.begun || L == null || !L.Alive) { r.begun = false; return r; }
        var si = (ScriptedInput)sim.input;
        var pilot = new Loom37Pilot { casual = casual };
        sim.mover.invulnerable = false;
        int st = 0, capSteps = (int)(cap * 120), killStep = -1;
        double dt = sim.step;
        int Leg() => Math.Max(1, Math.Min(3, L.brain.leg));
        sim.events.On<Bend34Event>("bend34Start", e => { r.bends[Leg()]++; if (e.tier == "full") r.full[Leg()]++; });
        sim.events.On<Boss37Event>(Boss37Brain.EvTell, e =>
        {
            if (e.boss != "loom37") return;
            if (!r.tells.ContainsKey(e.attack)) r.tells[e.attack] = new List<double>();
            r.tells[e.attack].Add(e.seconds);
        });
        sim.events.On<Boss37Event>(Boss37Brain.EvLeg, e => { if (e.boss == "loom37") r.legLog.Add($"{st * dt:0.0}s {e.stage} leg {e.leg}"); });
        sim.events.On<Loom37Event>("loom37Tear", e => { if (e.stage == "pull") r.tearsPull++; else r.tearsLt++; });
        sim.events.On<Loom37Event>("loom37Open", e => r.windows++);
        sim.events.On<Loom37Event>("loom37Down", e => { if (killStep < 0) killStep = st; });
        int netAt = -1;
        sim.events.On("loom47Net", o => { netAt = st; if (trace) Console.WriteLine($"    {st * dt:0.00}s NET catch at ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}) vel ({sim.mover.velocity.x:0.0},{sim.mover.velocity.y:0.0},{sim.mover.velocity.z:0.0}) home {sim.loom37LastLedge?.id}"); });
        sim.events.On<BossEvent>("bossHit", e => { if (e.boss != "loom37") return; if (L.Open) r.dealtWin += e.damage; else r.dealtChip += e.damage; });
        double hpBefore = sim.health.hp;
        while (st < capSteps && L.Alive)
        {
            if (sim.health.hp <= 0 || sim.flow != Skygrind.Game.Flow.Playing)
            {
                r.deaths++;
                sim.downSteps = 0;
                sim.flow = Skygrind.Game.Flow.Playing; // a death: played on (as n40crown's retry), not the end of the run
                sim.health.Reset();
                hpBefore = sim.health.hp;
                var back = sim.loom37LastLedge ?? start;
                sim.Respawn(new Vec3(back.at.x, back.y, back.at.z), 0);
                L.brain.RestartLeg();
            }
            pilot.Step(sim, st);
            if (L47Seam) L47SeamPolicy(sim, L, pilot, st);
            si.SetLook(pilot.Move.yaw, pilot.Move.pitch);
            si.Set(pilot.Move, pilot.Combat);
            var phaseBefore = sim.mover.rope.phase;
            sim.Step();
            sim.hitstopLeft = 0;
            st++;
            if (phaseBefore == RopePhase.None && sim.mover.rope.phase != RopePhase.None) r.latches[Leg()]++;
            if (sim.health.hp < hpBefore) r.taken += hpBefore - sim.health.hp;
            hpBefore = sim.health.hp;
            if (trace && netAt >= 0 && (st - netAt <= 12 || st - netAt == 30 || st - netAt == 90)) Console.WriteLine($"      +{(st - netAt) * dt:0.00}s after the catch: ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}) vel ({sim.mover.velocity.x:0.0},{sim.mover.velocity.y:0.0},{sim.mover.velocity.z:0.0}) grounded {sim.mover.grounded} rope {sim.mover.rope.phase}");
            if (trace && st % 600 == 0)
                Console.WriteLine($"    {st * dt:0.0}s leg {L.brain.leg} {L.brain.state} {L.brain.Current?.id} stag {L.brain.stagger:0.0} hp {L.body.hp:0}/{L.body.maxHp:0} me ({sim.mover.position.x:0.0},{sim.mover.position.y:0.0},{sim.mover.position.z:0.0}) bend {sim.Bend34TierNow}");
        }
        r.killed = !L.Alive || killStep >= 0;
        r.seconds = (killStep >= 0 ? killStep : st) * dt;
        r.falls = sim.loom37Falls;
        r.parries = L.spoolParries;
        r.seamTears = L.seamTears47; r.dives = L.dives47; r.diveHits = L.diveHits47; r.cages = L.cages47; r.cageHits = L.cageHits47; r.bobbins = L.bobbins47; r.bobbinHits = L.bobbinHits47;
        r.bendRefills = sim.loom47BendTear + sim.loom47BendParry + sim.loom47BendPhase;
        return r;
    }
}
