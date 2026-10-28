// NIGHT47 builder L: the human-model pilot (the lead's Harness.N47.Human.cs, H47Play and its human model, untouched) pointed at builder L's
// levels, plus THE LOOM's policy for it. Hooked without a Program.cs line: a module initializer runs the command when argv[1] is "n47lhuman".
//   n47lhuman --boss <loom|crown> [--level <id>] [--seeds 40] [--first 1] [--rows r1,r2] [--off47] [--cap 300] [--per-seed]
//     loom   Loom37Pilot from the canyon's "start" ledge of --level (default n46-span: today's level 4; n47-span: span47's arena), the fight set up
//            as n47human's H47Loom (a death respawns at the last ledge, the leg restarts) but on the given level file.
//     crown  Crown37Pilot (skilled) from (-10, 0.02, -267) on --level (default n46-spire), as n47human's H47Crown.
//     Reports n47human's numbers (kill time median / p90 / min, killed, taken, deaths, the band) plus THE LOOM's BEND starts per phase (leg) and
//     the FULL ones (G40: >= 3 BEND uses a phase), and the falls (both bosses).
// THE POLICY (n47human's hook, found by reflection: it also runs under `n47human --boss loom`): H47PolicyLoom47: with loom47 on, inside a BEND
// with a knee latched, the player looks at that knee and fires (the seam: L-cards.md section 1); the human model then limits the aim. THE LOOM's
// new attacks run through the brain's Tell state, so n47human's `telling` already delays the reaction to them (ctx.tell not needed).
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
    internal static void L47HumanInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n47lhuman") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            code = L47HumanMain(args);
        }
        catch (Exception e) { Console.WriteLine("n47lhuman: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static void H47PolicyLoom47(H47Ctx ctx, ref InputFrame move, ref CombatInput combat)
    {
        if (ctx.boss != "loom") return;
        if (L47HTraceSeed == L47HSeed && ctx.step % 600 == 0 && ctx.sim.loom37 != null)
        {
            SurvivalSim ts = ctx.sim;
            Loom37 TL = ts.loom37;
            Vec3 tm = ts.mover.position;
            Console.WriteLine($"    {ctx.step / 120.0:0.0}s leg {TL.brain.leg} {TL.brain.state} {TL.brain.Current?.id} stag {TL.brain.stagger:0.0} hp {TL.body.hp:0} me ({tm.x:0.0},{tm.y:0.0},{tm.z:0.0}) ledge {ts.loom37LastLedge?.id} rope {ts.mover.rope.phase} charges {ts.grappleCharges32} torn {TL.Torn} hp {ts.health.hp:0}");
        }
        SurvivalSim sim = ctx.sim;
        Loom37 L = sim.loom37;
        if (L == null || !L.loom47 || !sim.Bend34Inside || L.Open) return;
        int k = -1;
        for (int i = 0; i < 6; i++) if (L.Latched40(i)) { k = i; break; }
        if (k < 0) return;
        Vec3 me = sim.mover.position, eye = new Vec3(me.x, me.y + 1.6, me.z);
        (move.yaw, move.pitch) = Loom37Pilot.LookAt(eye, L.knees[k].at);
        combat.fire = new Button { held = true, pressed = ctx.step % 4 == 0 };
    }

    /// <summary>THE CROWN's policy (crown47's BELL TOLL is new: Crown37Pilot has no answer for it): a grounded player jumps as the wave's front
    /// comes within 2.5 m (a person sees the gold ring coming along the floor and hops it; the human model then delays the press in its window).</summary>
    static void H47PolicyCrown47(H47Ctx ctx, ref InputFrame move, ref CombatInput combat)
    {
        if (ctx.boss != "crown") return;
        Crown37 c = ctx.sim.crown37;
        if (c == null || !c.TollLive || !ctx.sim.mover.grounded) return;
        Vec3 me = ctx.sim.mover.position;
        double rr = JsMath.Hypot(me.x - c.centre.x, me.z - c.centre.z);
        if (rr - c.tollR <= 2.5 && rr - c.tollR >= -0.5) move.jump = new Button { held = true, pressed = true };
    }

    static int L47HTraceSeed, L47HSeed;

    sealed class L47HumanExtra
    {
        public readonly int[] bends = new int[4], full = new int[4];
        public int holds, nets, finales, falls, fireHits, tollHits, bladeHits, shardHits, lowHits, catches, fires, tolls, slams, volleys;
    }

    static int L47HumanMain(string[] args)
    {
        string boss = H47Arg(args, "--boss", "loom");
        string level = H47Arg(args, "--level", boss == "crown" ? "n46-spire" : "n46-span");
        int seeds = int.Parse(H47Arg(args, "--seeds", "40"), CultureInfo.InvariantCulture);
        int first = int.Parse(H47Arg(args, "--first", "1"), CultureInfo.InvariantCulture);
        double cap = double.Parse(H47Arg(args, "--cap", "300"), CultureInfo.InvariantCulture);
        H47Rows = H47Arg(args, "--rows", "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        L47HTraceSeed = int.Parse(H47Arg(args, "--trace-seed", "0"), CultureInfo.InvariantCulture);
        bool perSeed = Array.IndexOf(args, "--per-seed") >= 0;
        H47Policies = H47FindPolicies();
        var runs = new List<(H47Run r, L47HumanExtra x)>();
        for (int s = first; s < first + seeds; s++)
        {
            var x = new L47HumanExtra();
            H47Fight f = boss == "crown" ? L47HumanCrown((uint)s, level, x) : L47HumanLoom((uint)s, level, x);
            L47HSeed = s;
            H47Run r = H47Play(f, (uint)s, cap);
            if (f != null) x.falls = boss == "crown" ? f.sim.crown37Falls : f.sim.loom37Falls;
            if (f != null) x.finales = boss == "crown" ? f.sim.crown47Finales : f.sim.loom47Finales;
            if (f != null && boss == "loom") { x.nets = f.sim.loom47NetCatches; if (f.sim.loom37 != null) x.holds = f.sim.loom37.holds47; }
            if (f != null && boss == "crown" && f.sim.crown37 != null)
            {
                Crown37 c = f.sim.crown37;
                x.fireHits = c.fireHits47; x.tollHits = c.tollHits47; x.bladeHits = c.bladeHits; x.shardHits = c.shardHits; x.lowHits = c.lowHits; x.catches = c.catches47; x.fires = c.fires47; x.tolls = c.tolls47; x.slams = c.slams47; x.volleys = c.volleys47;
            }
            runs.Add((r, x));
            if (perSeed || !r.begun)
                Console.WriteLine($"  {boss} seed {s}: {(r.begun ? "" : "NOT BEGUN ")}{(r.killed ? "killed" : "CAP")} {H47F(r.seconds)} s, deaths {r.deaths}, taken {r.taken:0}, falls {x.falls}, "
                    + $"bends {x.bends[1]}/{x.bends[2]}/{x.bends[3]} (full {x.full[1]}/{x.full[2]}/{x.full[3]})");
        }
        var begun = runs.Where(p => p.r.begun).ToList();
        if (begun.Count == 0) { Console.WriteLine($"n47lhuman {boss} {level}: NO FIGHT began"); return 1; }
        var t = begun.Select(p => p.r.killed ? p.r.seconds : cap).OrderBy(v => v).ToList();
        double Q(List<double> l, double q) => l.Count == 0 ? double.NaN : l[Math.Min(l.Count - 1, (int)Math.Floor(q * (l.Count - 1) + 0.5))];
        double Med(IEnumerable<double> e) => Q(e.OrderBy(v => v).ToList(), 0.5);
        int killed = begun.Count(p => p.r.killed);
        double med = Q(t, 0.5), p90 = Q(t, 0.9), mn = t[0];
        bool band = med >= 90 && med <= 150 && p90 <= 180 && mn >= 60 && killed == begun.Count;
        Console.WriteLine($"n47lhuman {boss} ({level}, rows [{string.Join(",", H47Rows)}], off from {NightRows.ForceOffFrom}): killed {killed}/{begun.Count}; kill median {H47F(med)} s, p90 {H47F(p90)}, min {H47F(mn)}, max {H47F(t[^1])}; "
            + $"taken median {Med(begun.Select(p => p.r.taken)):0}, deaths {begun.Sum(p => p.r.deaths)}, falls {begun.Sum(p => p.x.falls)} ({begun.Sum(p => p.x.falls) / (double)begun.Count:0.0} a fight); "
            + $"finale beats {begun.Sum(p => p.x.finales)}; knee-held steps {begun.Sum(p => p.x.holds) / (double)begun.Count:0.0} a fight; net catches {begun.Sum(p => p.x.nets) / (double)begun.Count:0.0} a fight; dealt in windows {H47Pc(begun.Sum(p => p.r.dealtOpen) / Math.Max(1e-9, begun.Sum(p => p.r.dealt)))}; band {(band ? "IN" : "OUT")}");
        if (boss == "crown")
            Console.WriteLine($"  hits taken (totals): fire ticks {begun.Sum(p => p.x.fireHits)} of {begun.Sum(p => p.x.fires)} flames, toll {begun.Sum(p => p.x.tollHits)} of {begun.Sum(p => p.x.tolls)}, blade {begun.Sum(p => p.x.bladeHits)}, shard {begun.Sum(p => p.x.shardHits)}, low {begun.Sum(p => p.x.lowHits)}; catches {begun.Sum(p => p.x.catches)}; slam passes {begun.Sum(p => p.x.slams)}, shard volleys {begun.Sum(p => p.x.volleys)}");
        if (boss == "loom")
            for (int leg = 1; leg <= 3; leg++)
                Console.WriteLine($"  phase {leg}: BEND starts median {Med(begun.Select(p => (double)p.x.bends[leg])):0.0} (min {begun.Min(p => p.x.bends[leg])}), FULL median {Med(begun.Select(p => (double)p.x.full[leg])):0.0} (min {begun.Min(p => p.x.full[leg])})");
        return 0;
    }

    static H47Fight L47HumanLoom(uint seed, string level, L47HumanExtra x)
    {
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(MapGenRepo, "Design", "levels", level + ".json"))));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        H47ApplyRows(sim);
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var start = sim.loom37Ledges.Find(l => l.id == "start");
        if (start == null) return null;
        sim.Respawn(new Vec3(start.at.x, 0, start.at.z), 0);
        for (int i = 0; i < 10; i++) sim.Step();
        if (!sim.BossRoomBegin("loom37", out _, out _)) return null;
        Loom37 L = sim.loom37;
        if (L == null || !L.Alive) return null;
        int Leg() => Math.Max(1, Math.Min(3, L.brain.leg));
        sim.events.On<Bend34Event>("bend34Start", e => { x.bends[Leg()]++; if (e.tier == "full") x.full[Leg()]++; });
        var pilot = new Loom37Pilot();
        return new H47Fight
        {
            boss = "loom", sim = sim, reveal = 0,
            pilot = i => { pilot.Step(sim, i); return (pilot.Move, pilot.Combat); },
            alive = () => L.Alive && !L.Falling,
            telling = () => L.brain.state == Boss37State.Tell, at = () => L.centre,
            frac = () => L.HpFrac,
            retry = () =>
            {
                var back = sim.loom37LastLedge ?? start;
                sim.Respawn(new Vec3(back.at.x, back.y, back.at.z), 0);
                L.brain.RestartLeg();
            },
            note = level + " from the start ledge, Loom37Pilot stay (n47lhuman)",
        };
    }

    static H47Fight L47HumanCrown(uint seed, string level, L47HumanExtra x)
    {
        GameConfig cfg = Config();
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(MapGenRepo, "Design", "levels", level + ".json"))));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noLab = sim.noRecords = true;
        H47ApplyRows(sim);
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        var si = (ScriptedInput)sim.input;
        sim.Respawn(new Vec3(-10, 0.02, -267), 0);
        si.Set(new InputFrame(), new CombatInput { weapon = -1 });
        sim.Step();
        if (!sim.BossRoomBegin("crown37", out Vec3 spawn, out double yaw0)) return null;
        Crown37 c = sim.crown37;
        if (c == null) return null;
        var pilot = new Crown37Pilot { hand = new Mulberry32(seed * 7919u + 37u) };
        double yawL = 0;
        return new H47Fight
        {
            boss = "crown", sim = sim, reveal = MovementMath.StepsFor(c.p.revealSeconds, sim.step),
            pilot = i => { pilot.Step(sim, yawL, i); yawL = pilot.Move.yaw; return (pilot.Move, pilot.Combat); },
            alive = () => c.Alive && !c.Dying,
            telling = () => c.brain.state == Boss37State.Tell, at = () => c.centre,
            frac = () => c.body.maxHp > 0 ? Math.Max(0, c.body.hp) / c.body.maxHp : 0,
            retry = () => { sim.BossRoomBegin("crown37", out spawn, out yaw0); sim.Respawn(spawn, yaw0); },
            note = level + " from (-10, 0.02, -267), Crown37Pilot skilled (n47lhuman)",
        };
    }
}
