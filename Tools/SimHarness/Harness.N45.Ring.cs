// NIGHT45 builder A3 (NIGHT45.md card A3, T-65): THE RING's proof, headless. Hooked without a Program.cs line: a module initializer runs the
// command and exits when argv[1] is "n45ring" (NIGHT44 R2's pattern, Harness.N44.Proof.cs); no lead file and no older harness touched.
//   n45ring wyrm   [--level n45-kiln] [--seeds N] [--cap S]
//     THE KILN WYRM's fight on the level's k5 (n43kiln wyrm's / n43wick wyrm's setup: DOORFREE, the player put at k5's spawn, WyrmPilot31 fights,
//     a death respawns at the boss room), the design config's rows as the game ships them: killed or not, the time, the deaths, the bar valid every
//     live step (SurvivalSim.N43BossState), the legs reached, OPEN steps; and for each plate of the level's arena (plates45 / pieces43) the worm's
//     nearest approach to the plate's standing volume (the plate's top + 1.8 m, its bounding spheres: conservative) and the attack it came in.
//     Run it with --level n43-kiln (wyrm43: K1's arena) for BEFORE and --level n45-kiln (wyrm45) for AFTER. The bot band: 30-60 s (DECIDE39 D-1).
//   n45ring plates [--level n45-kiln]
//     each plate is solid in the sim: the player put on its top centre during the fight stands on it (feet at the top, grounded) after 0.5 s.
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
    internal static void A3N45RingInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n45ring") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try { code = A3Ring(args); }
        catch (Exception e) { Console.WriteLine("n45ring: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static string A3Arg(string[] args, string name, string dflt)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : dflt;
    }

    /// <summary>--cands: a JSON file of [x, z] arena-local spots: each a 4 x 4 m standing volume (1.2-3.0 m up) measured like a plate, not built.</summary>
    static string A3RingCands;
    /// <summary>n45ring crumble --move no: write the bar without the reel's 6 m step first (round 1's reel: the still armor gives half back).</summary>
    static bool A3RingMove = true;

    static string A3F(double v) => double.IsNaN(v) || double.IsInfinity(v) ? "n/a" : v.ToString("0.0", CultureInfo.InvariantCulture);

    static int A3Ring(string[] args)
    {
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "wyrm";
        string level = A3Arg(args, "--level", "n45-kiln");
        int seeds = int.Parse(A3Arg(args, "--seeds", "3"), CultureInfo.InvariantCulture);
        double cap = double.Parse(A3Arg(args, "--cap", "300"), CultureInfo.InvariantCulture);
        A3RingCands = A3Arg(args, "--cands", null);
        if (what == "wyrm") return A3RingWyrm(level, seeds, cap);
        if (what == "plates") return A3RingPlates(level);
        A3RingMove = A3Arg(args, "--move", "yes") == "yes";
        if (what == "crumble") return A3RingCrumble(level, A3Arg(args, "--hold", "no") == "yes");
        Console.WriteLine("n45ring: unknown '" + what + "' (wyrm, plates)");
        return 1;
    }

    /// <summary>The level's file and its worm arena (the k5 row's wyrmArena).</summary>
    static (string path, string arena) A3RingLevel(string level)
    {
        string path = Path.Combine(MapGenRepo, "Design", "levels", level + ".json");
        var map = PortData.Obj(PortData.LoadJson(path));
        string arena = "(none)";
        if (map.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp["rooms"] is List<object> rooms)
            foreach (object o in rooms)
                if (o is Dictionary<string, object> r && r.TryGetValue("id", out object id) && id as string == "k5" && r.TryGetValue("wyrmArena", out object wa))
                    arena = wa as string;
        return (path, arena);
    }

    /// <summary>The arena's plates: plates45's (index, attack) when the file has them, else pieces43's platforms (K1's: attack "coil").</summary>
    static List<(int index, string id, string attack)> A3RingPlateList(string arena)
    {
        var list = new List<(int, string, string)>();
        var j = PortData.Obj(PortData.LoadJson(Path.Combine(MapGenRepo, arena)));
        if (j.TryGetValue("plates45", out object po) && po is Dictionary<string, object> pd && pd["plates"] is List<object> plates)
        {
            foreach (object o in plates)
                if (o is Dictionary<string, object> p) list.Add(((int)PortData.Num(p["index"]), p["id"] as string, p["attack"] as string));
        }
        else if (j.TryGetValue("pieces43", out object qo) && qo is List<object> pieces)
        {
            foreach (object o in pieces)
                if (o is Dictionary<string, object> p && p["kind"] as string == "platform") list.Add(((int)PortData.Num(p["index"]), "plate" + PortData.Num(p["index"]), "coil"));
        }
        return list;
    }

    /// <summary>A headless run of the level, the player at k5's spawn with the worm's fight begun (null: no fight).</summary>
    static SurvivalSim A3RingStart(GameConfig cfg, string path, uint seed, out RoomDef k5)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
        sim.noLab = sim.noRecords = true;
        sim.setup.mode = Campaign24.Mode;
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        for (int i = 0; i < 10; i++) sim.Step();
        sim.campaign.SetDoorFree29(true);
        k5 = sim.campaignLevel.rooms.Find(r => r.id == "k5");
        sim.Respawn(k5.spawn, k5.spawnYawDeg);
        foreach (string g in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
        int wait = 0;
        while ((sim.wyrm24 == null || !sim.wyrm24.Alive) && wait++ < 600) sim.Step();
        return sim.wyrm24 != null && sim.wyrm24.Alive ? sim : null;
    }

    /// <summary>The distance from the worm's nearest piece (its bounding sphere) to an AABB; below 0: they may touch.</summary>
    static double A3RingNear(Wyrm w, Vec3 lo, Vec3 hi)
    {
        double m = double.PositiveInfinity;
        for (int i = 0; i < w.PieceCount; i++)
        {
            if (w.buried[i]) continue;
            WyrmBox b = w.boxes[i];
            double dx = Math.Max(lo.x - b.c.x, Math.Max(0, b.c.x - hi.x)), dy = Math.Max(lo.y - b.c.y, Math.Max(0, b.c.y - hi.y)), dz = Math.Max(lo.z - b.c.z, Math.Max(0, b.c.z - hi.z));
            double d = Math.Sqrt(dx * dx + dy * dy + dz * dz) - Math.Sqrt(b.hr * b.hr + b.hu * b.hu + b.hf * b.hf);
            if (d < m) m = d;
        }
        return m;
    }

    static int A3RingWyrm(string level, int seeds, double cap)
    {
        GameConfig cfg = Config();
        var (path, arena) = A3RingLevel(level);
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        var plates = A3RingPlateList(arena);
        int killed = 0, barBad = 0;
        var times = new List<double>();
        var plateMin = new double[plates.Count];
        var plateAt = new string[plates.Count];
        for (int k = 0; k < plates.Count; k++) plateMin[k] = double.PositiveInfinity;
        // each plate's (and each --cands spot's) nearest approach PER ATTACK: the aggro hunts the player wherever he stands (design.n26 wyrm26.aggro),
        // the aimed lunge re-aims at him (wyrmAim), so a plate's claim is about the attacks that run their written route
        var perPlate = new List<Dictionary<string, double>>();
        for (int k = 0; k < plates.Count; k++) perPlate.Add(new Dictionary<string, double>());
        var cands = new List<(double x, double z)>();
        if (A3RingCands != null)
            foreach (object o in (List<object>)PortData.LoadJson(A3RingCands))
            {
                var c = (List<object>)o;
                cands.Add((PortData.Num(c[0]), PortData.Num(c[1])));
            }
        var perCand = cands.Select(_ => new Dictionary<string, double>()).ToList();
        void Note(Dictionary<string, double> m, string key, double d) { if (!m.TryGetValue(key, out double v) || d < v) m[key] = d; }
        try
        {
            for (int s = 0; s < seeds; s++)
            {
                SurvivalSim sim = A3RingStart(cfg, path, (uint)(1 + s), out RoomDef k5);
                if (sim == null) { Console.WriteLine($"n45ring wyrm seed {s + 1}: NO FIGHT (level {level}, arena {arena})"); barBad++; continue; }
                Wyrm w = sim.wyrm24;
                WyrmArena a = sim.BossRoomArena("wyrm", out Vec3 o);
                var si = (ScriptedInput)sim.input;
                var wp = new WyrmPilot31();
                int step = 0, capSteps = (int)(cap * 120), deaths = 0, live = 0, invalid = 0, open = 0, maxLeg = 0, onPlate = 0;
                var hints = new HashSet<string>();
                Vec3 retry = k5.spawn;
                while (w.Alive && step < capSteps)
                {
                    wp.Step(sim, step);
                    si.SetLook(wp.Move.yaw, wp.Move.pitch);
                    si.Set(wp.Move, wp.Combat);
                    sim.Step();
                    sim.hitstopLeft = 0;
                    step++;
                    if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead) { deaths++; sim.downSteps = 0; sim.flow = Skygrind.Game.Flow.Playing; sim.health.Reset(); if (sim.BossRoomBegin("wyrm", out Vec3 sp, out _)) retry = sp; sim.Respawn(retry, 0); }
                    if (w.phase == "intro" || w.phase == "dead") continue;
                    live++;
                    Boss43State st = sim.N43BossState;
                    if (!st.valid) invalid++;
                    else
                    {
                        if (st.open) open++;
                        maxLeg = Math.Max(maxLeg, st.leg);
                        if (st.hint != null) hints.Add(st.hint);
                    }
                    Vec3 f = sim.mover.position;
                    for (int k = 0; k < plates.Count; k++)
                    {
                        var (min, max, _) = a.boxes[plates[k].index];
                        Vec3 lo = o + new Vec3(min.x, max.y, min.z), hi = o + new Vec3(max.x, max.y + 1.8, max.z);
                        double d = A3RingNear(w, lo, hi);
                        if (d < plateMin[k]) { plateMin[k] = d; plateAt[k] = (w.PatternName ?? w.phase) + " (phase " + w.phase + ")"; }
                        Note(perPlate[k], w.PatternName ?? w.phase, d);
                        if (sim.mover.grounded && f.x >= lo.x && f.x <= hi.x && f.z >= lo.z && f.z <= hi.z && Math.Abs(f.y - lo.y) < 0.1) onPlate++;
                    }
                    if (step % 6 == 0)
                        for (int k = 0; k < cands.Count; k++)
                        {
                            Vec3 lo = o + new Vec3(cands[k].x - 2, 1.2, cands[k].z - 2), hi = o + new Vec3(cands[k].x + 2, 3.0, cands[k].z + 2);
                            Note(perCand[k], w.PatternName ?? w.phase, A3RingNear(w, lo, hi));
                        }
                }
                if (!w.Alive) { killed++; times.Add(step / 120.0); }
                if (invalid > 0 || live == 0) barBad++;
                Console.WriteLine($"n45ring wyrm seed {s + 1}: THE KILN WYRM {(w.Alive ? "ALIVE" : "killed")} in {step / 120.0:0.0} s, deaths {deaths}; bar valid {live - invalid}/{live} live steps, legs up to {maxLeg}, OPEN steps {open}, hints {hints.Count}; the bot on a plate {onPlate / 120.0:0.0} s");
            }
        }
        finally { SurvivalSim.Wyrm24Dir = null; }
        times.Sort();
        double median = times.Count > 0 ? times[times.Count / 2] : double.NaN;
        for (int k = 0; k < plates.Count; k++)
        {
            Console.WriteLine($"  plate {plates[k].id} (box {plates[k].index}, answers the {plates[k].attack}): the worm's nearest approach to its standing volume {A3F(plateMin[k])} m (in {plateAt[k] ?? "-"}){(plateMin[k] > 0 ? "" : ": MAY TOUCH")}");
            Console.WriteLine("    per attack: " + string.Join(", ", perPlate[k].OrderBy(e => e.Key).Select(e => e.Key + " " + A3F(e.Value))));
        }
        for (int k = 0; k < cands.Count; k++)
        {
            double scripted = perCand[k].Where(e => e.Key != "aggro" && e.Key != "lunge").Select(e => e.Value).DefaultIfEmpty(double.PositiveInfinity).Min();
            Console.WriteLine($"  cand {cands[k].x:0} {cands[k].z:0}: written routes (not aggro / lunge) {A3F(scripted)}; " + string.Join(", ", perCand[k].OrderBy(e => e.Key).Select(e => e.Key + " " + A3F(e.Value))));
        }
        bool band = killed == seeds && median >= 30 && median <= 60;
        Console.WriteLine($"n45ring wyrm SUMMARY (level {level}, arena {arena}): killed {killed}/{seeds}, median {A3F(median)} s; bar valid every live step: {(barBad == 0 ? "yes" : "NO (" + barBad + " seeds)")}; the bot band 30-60 s: {(band ? "GREEN" : "RED")}; plates touched {plateMin.Count(m => m <= 0)}/{plates.Count}");
        return killed == seeds && barBad == 0 ? 0 : 1;
    }

    /// <summary>n45ring crumble [--hold yes]: the a345 reel's crumble headless: the player idle (God), 6 s of fight, the bar written under
    /// crumbleAt (the reel's cheat), then 6 s traced: the bar, the worm's phase, the crumble state. --hold: the reel's round-2 fix.</summary>
    static int A3RingCrumble(string level, bool hold)
    {
        GameConfig cfg = Config();
        var (path, _) = A3RingLevel(level);
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        try
        {
            SurvivalSim sim = A3RingStart(cfg, path, 1, out _);
            if (sim == null || sim.wyrm31 == null) { Console.WriteLine("n45ring crumble: NO FIGHT / no wyrm31"); return 1; }
            Wyrm w = sim.wyrm24;
            var si = (ScriptedInput)sim.input;
            void God() { sim.health.max = sim.health.hp = 1e6; }
            for (int i = 0; i < 720; i++) { God(); si.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); sim.hitstopLeft = 0; }
            double mark = Math.Min(0.49, sim.wyrm31.p.crumbleAt - 0.01);
            Console.WriteLine($"n45ring crumble: before the write: bar {w.HpFrac:0.00}, phase {w.phase}, crumbleAt {sim.wyrm31.p.crumbleAt}, phase2At {w.p.phase2At}, phase3At {w.p.phase3At}");
            if (A3RingMove)
            {
                // round 2 (the reel's fix): an idle player is still (design.n33.bosses33.stillArmor: within stillRadius 4 m for stillSeconds 1.5 s)
                // and the still armor gives back half of every drop of the bar each step, the cheat's write included (2754 -> 4187 of 5620 in one
                // step); a step of 6 m first resets the stillness, so the write lands whole
                Vec3 at = sim.mover.position;
                sim.Respawn(new Vec3(at.x, at.y, at.z - 6), 0);
                si.Set(new InputFrame(), new CombatInput { weapon = -1 });
                sim.Step();
            }
            A3RingHold(w, mark, true);
            Console.WriteLine($"  after the write: hp {w.body.hp:0} / max {w.body.maxHp:0}, the still armor {sim.bosses33Armor}");
            int fallenAt = -1;
            for (int i = 0; i < 720; i++)
            {
                God();
                if (hold && sim.wyrm31CrumbleState != 2) A3RingHold(w, mark, false);
                si.Set(new InputFrame(), new CombatInput { weapon = -1 });
                sim.Step();
                sim.hitstopLeft = 0;
                if (sim.wyrm31CrumbleState == 2 && fallenAt < 0) fallenAt = i;
                if (i % 24 == 0 || i < 2) Console.WriteLine($"  t {i / 120.0:0.00}: bar {w.HpFrac:0.00} (hp {w.body.hp:0} / {w.body.maxHp:0}) phase {w.phase} crumble {sim.wyrm31CrumbleState}");
            }
            Console.WriteLine($"n45ring crumble SUMMARY: fallen at {(fallenAt < 0 ? "never" : (fallenAt / 120.0).ToString("0.00", CultureInfo.InvariantCulture) + " s")}, the end: state {sim.wyrm31CrumbleState}, bar {w.HpFrac:0.00}");
            return sim.wyrm31CrumbleState == 2 ? 0 : 1;
        }
        finally { SurvivalSim.Wyrm24Dir = null; }
    }

    /// <summary>The reel's cheat (FeelReel.N45.Ring.cs mirrors it): the bar at `mark` (a fraction), only ever lowered.</summary>
    static void A3RingHold(Wyrm w, double mark, bool first)
    {
        if (first || w.HpFrac > mark) w.body.hp = w.body.maxHp * mark;
    }

    static int A3RingPlates(string level)
    {
        GameConfig cfg = Config();
        var (path, arena) = A3RingLevel(level);
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        int fails = 0;
        try
        {
            foreach (var pl in A3RingPlateList(arena))
            {
                SurvivalSim sim = A3RingStart(cfg, path, 1, out _);
                if (sim == null) { Console.WriteLine("n45ring plates: NO FIGHT"); return 1; }
                WyrmArena a = sim.BossRoomArena("wyrm", out Vec3 o);
                var (min, max, kind) = a.boxes[pl.index];
                Vec3 top = o + new Vec3((min.x + max.x) * 0.5, max.y + 0.05, (min.z + max.z) * 0.5);
                sim.Respawn(top, 0);
                var si = (ScriptedInput)sim.input;
                for (int i = 0; i < 60; i++) { si.Set(new InputFrame(), new CombatInput { weapon = -1 }); sim.Step(); sim.hitstopLeft = 0; }
                Vec3 f = sim.mover.position;
                bool ok = sim.mover.grounded && Math.Abs(f.y - (o.y + max.y)) < 0.05;
                if (!ok) fails++;
                Console.WriteLine($"  plate {pl.id} (box {pl.index} {kind}, top {max.y - min.y:0.0} m): the player after 0.5 s at y {f.y - o.y:0.00} (top {max.y:0.00}), grounded {sim.mover.grounded}: {(ok ? "STANDS" : "FAIL")}");
            }
        }
        finally { SurvivalSim.Wyrm24Dir = null; }
        Console.WriteLine($"n45ring plates SUMMARY (level {level}, arena {arena}): {(fails == 0 ? "GREEN" : "RED")} ({fails} fail)");
        return fails == 0 ? 0 : 1;
    }
}
