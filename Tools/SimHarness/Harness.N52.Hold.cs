// NIGHT52 builder K (NIGHT52.md section 2 item 7, THE ROOM CONTRACT's "no camping spot"; NIGHT52-BRIEF.md section 3): `hold52`, the hold probe.
// Hooked without a Program.cs line: a module initializer runs the command and exits when argv[1] is "hold52" (n46flow's pattern, Harness.N46.Flow.cs).
//   hold52 --level <id> --room <room> [--spots auto|x,y,z;x,y,z...] [--perches] [--seeds N] [--first k] [--hold s] [--file f] [--off52] [--trace]
//     --perches (D-15): also every other solid top >= 1 m a player can stand on (a post, a ride wall's top, a collider prop), tier "perch".
//     For each standing spot of the room (auto: the floor's centre, and on each TIER the highest top of its cluster, the tiers as
//     Tools/night/n52-room-check.py counts them: the room's deck / slab boxes in the level's campaign.boxes with a top >= 1 m, a cluster = tops
//     within 1.5 m of its lowest), the player stands there `--hold` s (20) with the room's waves live and does nothing (no move, no shot, no
//     look): the probe counts the hits taken ("hurt" events on the player, god mode so it never dies). The level is the map file
//     Design/levels/<id>.json played as the campaign with the config's rows (as n46flow plays it); the player is put in the room by DOORFREE at
//     its spawn (n46flow's placement), then on the spot once the room's fight runs; the 20 s start at the first live enemy (15 s at most).
//     One line per spot, exactly: `hold52 <room> tier <t> spot [x,y,z] hits <n>` (n = the fewest over the seeds: one quiet seed is a camping
//     spot), then an indented detail line (per seed: hits, damage, the enemies alive, how far the body was pushed off the spot). A spot with 0
//     hits is a camping spot (n52-room-check.py --hold fails the room on it).
//   hold52 --level <id> --room <room> --pads [--off52]
//     The pad proof (NIGHT52-BRIEF.md section 3, "Pads"): for each jump pad inside the room's rect (the map's `pads`), the player is put in the
//     room (DOORFREE, its spawn) and then on the pad; the sim's own pad check (ArenaMap.CheckPads through SurvivalSim's step) launches it. One
//     line per pad: `pad52 <room> pad <i> at [x,y,z] target [x,y,z] launched <yes|no> apex <y> landed [x,y,z] miss <m> m <PASS|FAIL>` (PASS: it
//     launched and landed within 1.5 m of the target on the floor plan and 0.3 m in height, in 5 s). Exit 0 only when every pad passes.
// Probe numbers only (the hold, the waits, the pass margins): not gameplay.
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
    internal static void K52HoldInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "hold52") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            code = K52Hold(args);
        }
        catch (Exception e) { Console.WriteLine("hold52: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static readonly CultureInfo K52Inv = CultureInfo.InvariantCulture;
    static string K52F(double v) => Math.Round(v, 2).ToString("0.##", K52Inv);
    static string K52V(Vec3 v) => $"[{K52F(v.x)},{K52F(v.y)},{K52F(v.z)}]";

    /// <summary>A collision box of the level file, [x0, y0, z0, x1, y1, z1].</summary>
    static List<double[]> K52Boxes(Dictionary<string, object> json)
    {
        var res = new List<double[]>();
        foreach (object o in PortData.Arr(PortData.Obj(json["collision"])["boxes"]))
        {
            var d = PortData.Obj(o);
            var mn = PortData.Arr(d["min"]); var mx = PortData.Arr(d["max"]);
            res.Add(new[] { PortData.Num(mn[0]), PortData.Num(mn[1]), PortData.Num(mn[2]), PortData.Num(mx[0]), PortData.Num(mx[1]), PortData.Num(mx[2]) });
        }
        return res;
    }

    static List<double[]> K52Lava(Dictionary<string, object> json)
    {
        var res = new List<double[]>();
        if (json.TryGetValue("lava", out object lv))
            foreach (object o in PortData.Arr(lv))
            {
                var r = PortData.Arr(PortData.Obj(o)["rect"]);
                res.Add(new[] { PortData.Num(r[0]), PortData.Num(r[1]), PortData.Num(r[2]), PortData.Num(r[3]) });
            }
        return res;
    }

    /// <summary>Can a player stand at (x, y, z): a box top within 0.05 m under the feet, no box in the body (radius 0.5, 1.8 m tall), no lava under it.</summary>
    static bool K52Standable(List<double[]> boxes, List<double[]> lava, double x, double y, double z)
    {
        const double r = 0.5, h = 1.8;
        if (y < 0.5) foreach (double[] l in lava) if (x >= l[0] && x <= l[2] && z >= l[1] && z <= l[3]) return false; // a top over the lava (a ride wall standing in it) is a perch
        bool floor = false;
        foreach (double[] b in boxes)
        {
            if (x + r > b[0] && x - r < b[3] && z + r > b[2] && z - r < b[5] && y + 0.05 < b[4] && y + h > b[1]) return false;
            if (x >= b[0] && x <= b[3] && z >= b[2] && z <= b[5] && Math.Abs(b[4] - y) <= 0.05) floor = true;
        }
        return floor;
    }

    /// <summary>The nearest standable point to (x, z) at height y, searched in rings of 0.5 m out to `maxR` m (inside [x0, z0, x1, z1]); null when none.</summary>
    static Vec3? K52Near(List<double[]> boxes, List<double[]> lava, double x, double y, double z, double maxR, double x0, double z0, double x1, double z1)
    {
        for (double rr = 0; rr <= maxR + 1e-9; rr += 0.5)
        {
            int n = rr == 0 ? 1 : (int)Math.Ceiling(2 * Math.PI * rr / 0.5);
            for (int k = 0; k < n; k++)
            {
                double a = 2 * Math.PI * k / n, px = x + rr * Math.Cos(a), pz = z + rr * Math.Sin(a);
                if (px < x0 || px > x1 || pz < z0 || pz > z1) continue;
                if (!K52Standable(boxes, lava, px, y, pz)) continue;
                double sx = Math.Round(px * 2) / 2, sz = Math.Round(pz * 2) / 2; // on the 0.5 m grid when that point stands too (a readable spot)
                return K52Standable(boxes, lava, sx, y, sz) ? new Vec3(sx, y, sz) : new Vec3(Math.Round(px, 2), y, Math.Round(pz, 2));
            }
        }
        return null;
    }

    /// <summary>The room's tiers (n52-room-check.py's rule) as [lowest top, highest top] with the box of the highest top; deck / slab boxes only.</summary>
    static List<(double lo, double hi, double[] box)> K52Tiers(Dictionary<string, object> json, string room)
    {
        var tops = new List<(double h, double[] box)>();
        if (PortData.Obj(json["campaign"]).TryGetValue("boxes", out object bo))
            foreach (object o in PortData.Arr(bo))
            {
                var d = PortData.Obj(o);
                if (!(d.TryGetValue("zone", out object z) && z as string == room)) continue;
                string kind = d.TryGetValue("kind", out object k) ? k as string : "";
                if (kind != "deck" && kind != "slab") continue;
                var mn = PortData.Arr(d["min"]); var mx = PortData.Arr(d["max"]);
                double top = PortData.Num(mx[1]);
                if (top < 1.0) continue;
                tops.Add((top, new[] { PortData.Num(mn[0]), PortData.Num(mn[1]), PortData.Num(mn[2]), PortData.Num(mx[0]), top, PortData.Num(mx[2]) }));
            }
        tops.Sort((a, b) => a.h.CompareTo(b.h));
        var res = new List<(double lo, double hi, double[] box)>();
        foreach (var t in tops)
        {
            if (res.Count > 0 && t.h - res[res.Count - 1].lo <= 1.5)
            {
                var c = res[res.Count - 1];
                double[] best = t.h >= c.hi ? t.box : c.box; // the highest top of the cluster (the later of equals: the file's order)
                res[res.Count - 1] = (c.lo, Math.Max(c.hi, t.h), best);
            }
            else res.Add((t.h, t.h, t.box));
        }
        return res;
    }

    static string K52Arg(string[] args, string key, string def)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
    }

    /// <summary>The probe's player: nothing pressed, the look fixed (yaw toward the room's middle).</summary>
    sealed class K52Still : IPlayerInput
    {
        double yaw, pitch;
        public double Yaw => yaw;
        public double Pitch => pitch;
        public void SetLook(double y, double p) { yaw = y; pitch = p; }
        public bool Captured => true;
        public InputFrame SampleMove(bool active) => new InputFrame { yaw = yaw, pitch = pitch };
        public CombatInput SampleCombat(bool active) => new CombatInput { weapon = -1 };
    }

    /// <summary>A headless campaign on the level file with the player in `room` (DOORFREE to its spawn, as n46flow places it); false when the
    /// campaign did not reach the room in 20 s of game time.</summary>
    static bool K52Enter(GameConfig cfg, string path, string id, string room, uint seed, out SurvivalSim sim, out RoomDef rd, out string why)
    {
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var still = new K52Still();
        sim.UseInput(still);
        sim.StartRun(seed, null, null);
        rd = null;
        why = null;
        if (sim.campaignLevel == null || sim.campaignLevel.id != id) { why = $"the campaign loaded {sim.campaignLevel?.id ?? "nothing"}, not {id}"; return false; }
        int ri = sim.campaignLevel.rooms.FindIndex(r => r.id == room);
        if (ri < 0) { why = $"no room {room} in {id}"; return false; }
        rd = sim.campaignLevel.rooms[ri];
        bool placed = false;
        int max = MovementMath.StepsFor(20, sim.step);
        for (int s = 0; s < max; s++)
        {
            sim.Step();
            Campaign24 c = sim.campaign;
            if (c == null || c.room < 0) continue;
            if (!placed)
            {
                if (ri == 0) { placed = true; continue; }
                c.SetDoorFree29(true);
                sim.Respawn(rd.spawn, rd.spawnYawDeg);
                placed = true;
                continue;
            }
            if (c.room == ri)
            {
                c.SetDoorFree29(false);
                return true;
            }
        }
        why = $"the campaign did not enter {room} in 20 s (at room {sim.campaign?.room})";
        return false;
    }

    static int K52Hold(string[] args)
    {
        string id = K52Arg(args, "--level", null), room = K52Arg(args, "--room", null);
        if (id == null || room == null) { Console.WriteLine("usage: hold52 --level <id> --room <room> [--spots auto|x,y,z;...] [--seeds N] [--first k] [--hold s] [--pads] [--off52] [--trace]"); return 2; }
        string spotsArg = K52Arg(args, "--spots", "auto");
        int seeds = int.Parse(K52Arg(args, "--seeds", "3"), K52Inv), first = int.Parse(K52Arg(args, "--first", "1"), K52Inv);
        double hold = double.Parse(K52Arg(args, "--hold", "20"), K52Inv);
        bool trace = Array.IndexOf(args, "--trace") >= 0, pads = Array.IndexOf(args, "--pads") >= 0;
        GameConfig cfg = Config();
        string path = Path.GetFullPath(K52Arg(args, "--file", Path.Combine("Design", "levels", id + ".json"))); // --file: another copy of the level (e.g. from port/stage3)
        if (!File.Exists(path)) { Console.WriteLine($"hold52: no level file {path}"); return 2; }
        var json = PortData.Obj(PortData.LoadJson(path));
        LevelDef level = LevelDef.From(json);
        RoomDef rd0 = level.rooms.Find(r => r.id == room);
        if (rd0 == null) { Console.WriteLine($"hold52: no room {room} in {id}"); return 2; }
        double x0 = Math.Min(rd0.x0, rd0.x1), x1 = Math.Max(rd0.x0, rd0.x1), z0 = Math.Min(rd0.z0, rd0.z1), z1 = Math.Max(rd0.z0, rd0.z1);
        List<double[]> boxes = K52Boxes(json), lava = K52Lava(json);
        Console.WriteLine($"hold52: level {id} ({path}), room {room} \"{rd0.name}\" rect [{K52F(x0)},{K52F(z0)},{K52F(x1)},{K52F(z1)}], off switch {(NightRows.ForceOffFrom == NightRows.NoOff ? "none" : "--off" + NightRows.ForceOffFrom)}");
        if (Array.IndexOf(args, "--dwelltrace") >= 0) return K52DwellTrace(cfg, path, id, room, json, int.Parse(K52Arg(args, "--first", "1"), K52Inv), int.Parse(K52Arg(args, "--seeds", "1"), K52Inv));
        if (pads) return Array.IndexOf(args, "--raw") >= 0 ? K52PadsRaw(cfg, path, id, room, json, x0, z0, x1, z1) : K52Pads(cfg, path, id, room, json, x0, z0, x1, z1);

        // the spots: (tier, point)
        var tiers = K52Tiers(json, room);
        var spots = new List<(int tier, Vec3 at, string how)>();
        if (spotsArg == "auto")
        {
            Vec3? c = K52Near(boxes, lava, (x0 + x1) / 2, 0, (z0 + z1) / 2, 12, x0, z0, x1, z1);
            if (c.HasValue) spots.Add((0, c.Value, "the floor's centre (nearest standable point)"));
            else Console.WriteLine("hold52: no standable floor point within 12 m of the room's centre");
            for (int t = 0; t < tiers.Count; t++)
            {
                double[] b = tiers[t].box;
                Vec3? p = K52Near(boxes, lava, (b[0] + b[3]) / 2, b[4], (b[2] + b[5]) / 2, Math.Max(b[3] - b[0], b[5] - b[2]), b[0], b[2], b[3], b[5]);
                if (p.HasValue) spots.Add((t + 1, p.Value, $"tier {t + 1} ({K52F(tiers[t].lo)}-{K52F(tiers[t].hi)} m): the top of box [{K52F(b[0])},{K52F(b[2])}]-[{K52F(b[3])},{K52F(b[5])}] at {K52F(b[4])} m"));
                else Console.WriteLine($"hold52: tier {t + 1}: no standable point on its highest top at {K52F(b[4])} m");
            }
            // --perches (the lead's D-15 ask): every other solid top in the room a player can stand on (a hull / wall / any box top >= 1 m, under the
            // roof by a body's height, with a standable point): a post, a ride wall's top, a prop's collider: each probed as tier "perch"
            if (Array.IndexOf(args, "--perches") >= 0)
            {
                double roof = double.MaxValue;
                foreach (double[] b in boxes) if (b[0] <= x0 && b[3] >= x1 && b[2] <= z0 && b[5] >= z1 && b[1] > 2) roof = Math.Min(roof, b[1]);
                var done = new HashSet<string>();
                foreach (var sp0 in spots) done.Add($"{K52F(sp0.at.y)}");
                foreach (double[] b in boxes)
                {
                    double cx = (b[0] + b[3]) / 2, cz = (b[2] + b[5]) / 2, top = b[4];
                    if (cx < x0 || cx > x1 || cz < z0 || cz > z1 || top < 1.0 || top > roof - 1.9) continue;
                    if (b[3] - b[0] > (x1 - x0) * 0.9 && b[5] - b[2] > (z1 - z0) * 0.9) continue; // the floor slab or the roof
                    bool tierTop = false;
                    foreach (var t in tiers) if (Math.Abs(t.hi - top) < 1e-6 && cx >= t.box[0] && cx <= t.box[3] && cz >= t.box[2] && cz <= t.box[5]) tierTop = true;
                    if (tierTop) continue;
                    Vec3? p = K52Near(boxes, lava, cx, top, cz, Math.Max(b[3] - b[0], b[5] - b[2]), b[0], b[2], b[3], b[5]);
                    if (!p.HasValue) continue;
                    string key = $"{K52F(p.Value.x)},{K52F(p.Value.y)},{K52F(p.Value.z)}";
                    if (!done.Add(key)) continue;
                    spots.Add((-1, p.Value, $"perch: the top of box [{K52F(b[0])},{K52F(b[1])},{K52F(b[2])}]-[{K52F(b[3])},{K52F(b[4])},{K52F(b[5])}]"));
                }
            }
        }
        else
        {
            foreach (string s in spotsArg.Split(';'))
            {
                string[] v = s.Split(',');
                var at = new Vec3(double.Parse(v[0], K52Inv), double.Parse(v[1], K52Inv), double.Parse(v[2], K52Inv));
                int t = 0;
                for (int k = 0; k < tiers.Count; k++) if (at.y >= tiers[k].lo - 0.75) t = k + 1;
                if (at.y < 0.75) t = 0;
                spots.Add((t, at, "given"));
            }
        }
        int zeros = 0;
        foreach (var sp in spots)
        {
            int fewest = int.MaxValue;
            var detail = new List<string>();
            for (int k = 0; k < seeds; k++)
            {
                uint seed = (uint)(first + k);
                if (!K52Enter(cfg, path, id, room, seed, out SurvivalSim sim, out RoomDef rd, out string why)) { Console.WriteLine($"hold52: RED: {why}"); return 2; }
                int hits = 0;
                double dmg = 0;
                sim.events.On<HurtEvent>("hurt", e => { if (e.owner == "player" && e.amount > 0) { hits++; dmg += e.amount; } });
                double yawDeg = Math.Atan2(-((x0 + x1) / 2 - sp.at.x), -((z0 + z1) / 2 - sp.at.z)) * 180 / Math.PI;
                sim.health.max = sim.health.hp = 1e6; // god mode (the reels' God): the hits are counted, the room never restarts
                sim.Respawn(sp.at, yawDeg); // sets the still input's look too
                // the wait for the room's first live enemy (the commit gate, the spawn tell), then the hold
                int wait = 0, maxWait = MovementMath.StepsFor(15, sim.step);
                while (wait < maxWait && sim.enemies.AliveCount == 0) { sim.Step(); sim.health.hp = sim.health.max; wait++; }
                hits = 0; dmg = 0;
                int steps = MovementMath.StepsFor(hold, sim.step), aliveMax = 0, aliveEnd = 0;
                double pushed = 0;
                for (int s = 0; s < steps; s++)
                {
                    sim.Step();
                    sim.health.hp = sim.health.max;
                    int alive = 0;
                    foreach (Enemy en in sim.enemies.all) if (en.alive && !en.standIn29) alive++;
                    aliveMax = Math.Max(aliveMax, alive);
                    aliveEnd = alive;
                    Vec3 p = sim.mover.position;
                    pushed = Math.Max(pushed, Math.Sqrt((p.x - sp.at.x) * (p.x - sp.at.x) + (p.z - sp.at.z) * (p.z - sp.at.z)));
                    if (trace && s % 240 == 0)
                        Console.WriteLine($"    seed {seed} t {s * sim.step:0.0} s: feet {K52V(p)} hits {hits}; {string.Join(", ", sim.enemies.all.Where(en => en.alive && !en.standIn29).Take(8).Select(en => en.kind + " " + K52V(en.position)))}");
                }
                fewest = Math.Min(fewest, hits);
                detail.Add($"seed {seed}: hits {hits}, damage {K52F(dmg)}, wave wait {K52F(wait * sim.step)} s, alive max {aliveMax} / at the end {aliveEnd}, pushed off the spot {K52F(pushed)} m, feet at the end {K52V(sim.mover.position)}");
            }
            if (fewest == 0) zeros++;
            Console.WriteLine($"hold52 {room} tier {(sp.tier < 0 ? "perch" : sp.tier.ToString(K52Inv))} spot {K52V(sp.at)} hits {fewest}");
            Console.WriteLine($"    {sp.how}; {hold:0} s each, the fewest of {seeds} seed(s): " + string.Join("; ", detail));
        }
        Console.WriteLine($"hold52 {room}: {spots.Count} spots, {zeros} with 0 hits ({(zeros == 0 && spots.Count > 0 ? "no camping spot" : "CAMPING SPOT")})");
        return zeros == 0 && spots.Count > 0 ? 0 : 1;
    }

    /// <summary>--dwelltrace (a diagnostic for THE ROOM CONTRACT's dwell item): n46flow's isolated walk of one room (its pilot F1Input round
    /// Walk38Pilot, the DOORFREE placement at the room's spawn, as `n46flow --rooms <room>`), and every 0.25 s the pilot spends within 4 m of the
    /// entry door's rect with an enemy alive: where it stands, its state, the nearest enemy (kind, place, distance). One line per such sample.</summary>
    static int K52DwellTrace(GameConfig cfg, string path, string id, string room, Dictionary<string, object> json, int first, int seeds)
    {
        LevelDef level = LevelDef.From(json);
        int a = level.rooms.FindIndex(r => r.id == room);
        var doors = F1EntryDoors(json);
        double[] dr = doors[room];
        for (int k = 0; k < seeds; k++)
        {
            uint seed = (uint)(first + k);
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.GoalProvider = () =>
            {
                Campaign24 c = sim.campaign;
                LevelDef lv = sim.campaignLevel;
                if (c == null || c.room < 0) return lv.rooms[a].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            var pilot = new F1Input(sim, new Walk38Pilot(sim, ap), json);
            sim.UseInput(pilot);
            sim.StartRun(seed, null, null);
            bool placed = false, doorFreeOn = true;
            int dwellSteps = 0;
            for (int stp = 0; stp < MovementMath.StepsFor(240, sim.step) && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); stp++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                Campaign24 c = sim.campaign;
                if (!placed && c != null && c.room >= 0) { c.SetDoorFree29(true); sim.Respawn(level.rooms[a].spawn, level.rooms[a].spawnYawDeg); placed = true; }
                if (c == null || c.room < 0) continue;
                if (doorFreeOn && c.room == a) { c.SetDoorFree29(false); doorFreeOn = false; }
                if (c.room != a) { if (c.room > a) break; continue; }
                if (c.phase == Campaign24.Phase.Cleared) break;
                if (c.phase != Campaign24.Phase.Fight) continue;
                Vec3 p = sim.mover.position;
                double ex = Math.Max(dr[0], Math.Min(p.x, dr[2])), ez = Math.Max(dr[1], Math.Min(p.z, dr[3]));
                if ((ex - p.x) * (ex - p.x) + (ez - p.z) * (ez - p.z) > 16) continue;
                Enemy best = null;
                double bd = double.MaxValue;
                foreach (Enemy en in sim.enemies.all)
                {
                    if (!en.alive || en.standIn29) continue;
                    double d = Math.Sqrt((en.position.x - p.x) * (en.position.x - p.x) + (en.position.z - p.z) * (en.position.z - p.z));
                    if (d < bd) { bd = d; best = en; }
                }
                if (best == null) continue;
                dwellSteps++;
                if (dwellSteps % 30 == 1)
                    Console.WriteLine($"dwell52 {room} seed {seed} t {(stp * sim.step):0.0} s: feet {K52V(p)} {sim.mover.state} hp {sim.health.hp:0}; nearest {best.kind} {K52V(best.position)} at {bd:0.0} m; wave {c.wave}, alive {sim.enemies.AliveCount}");
            }
            Console.WriteLine($"dwell52 {room} seed {seed}: {dwellSteps * sim.step:0.0} s within 4 m of the entry door with an enemy alive");
            ap.Detach();
        }
        return 0;
    }

    /// <summary>The probe's scripted player for --raw: one frame held until changed.</summary>
    sealed class K52Script : IPlayerInput
    {
        public InputFrame frame;
        double yaw, pitch;
        public double Yaw => yaw;
        public double Pitch => pitch;
        public void SetLook(double y, double p) { yaw = y; pitch = p; }
        public bool Captured => true;
        public InputFrame SampleMove(bool active) { InputFrame f = frame; f.yaw = yaw; f.pitch = pitch; return f; }
        public CombatInput SampleCombat(bool active) => new CombatInput { weapon = -1 };
    }

    /// <summary>--pads --raw: verbs37's row 15 as the Player reel runs it (FeelReel.N35.Verbs.cs: Campaign35 = DOORFREE from the level's first room, the
    /// husks killed and frozen, Reset35 = the feet 0.3 m over the pad + 0.35 s still, then 1.0 s with moveZ 0.3 for 0.3 s), traced step by step.</summary>
    static int K52PadsRaw(GameConfig cfg, string path, string id, string room, Dictionary<string, object> json, double x0, double z0, double x1, double z1)
    {
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        var si = new K52Script();
        sim.UseInput(si);
        sim.StartRun(160716, null, null);
        for (int s = 0; s < 240 && (sim.campaign == null || sim.campaign.room < 0); s++) sim.Step();
        sim.campaign?.SetDoorFree29(true);
        sim.husks.KillAll();
        sim.husks.frozen = true;
        int jp = 0, ev = 0;
        sim.mover.Events += e => { if (e.type == MoverEventType.JumpPad) jp++; };
        sim.events.On("jumpPad", _ => ev++);
        string roomAt() => sim.campaign != null && sim.campaign.room >= 0 ? sim.campaignLevel.rooms[sim.campaign.room].id : "?";
        int fails = 0;
        for (int i = 0; i < map.pads.Count; i++)
        {
            PadSpec pd = map.pads[i];
            if (pd.pos.x < x0 || pd.pos.x > x1 || pd.pos.z < z0 || pd.pos.z > z1) continue;
            int jp0 = jp, ev0 = ev;
            si.frame = new InputFrame();
            sim.Respawn(new Vec3(pd.pos.x, pd.pos.y + 0.3, pd.pos.z), 0);
            Console.WriteLine($"pad52raw {room} pad {i}: campaign room {roomAt()} (DOORFREE on), awake {sim.zones.padEnabled[i]}");
            int n = MovementMath.StepsFor(1.35, sim.step), still = MovementMath.StepsFor(0.35, sim.step), walk = MovementMath.StepsFor(0.3, sim.step);
            for (int s = 0; s < n; s++)
            {
                si.frame = s < still ? new InputFrame() : new InputFrame { moveZ = s - still < walk ? 0.3 : 0 };
                Vec3 b = sim.mover.position;
                bool g0 = sim.mover.grounded;
                sim.Step();
                Vec3 p = sim.mover.position;
                double d = Math.Sqrt((p.x - pd.pos.x) * (p.x - pd.pos.x) + (p.z - pd.pos.z) * (p.z - pd.pos.z));
                if (s < 24 || s % 12 == 0 || jp > jp0)
                    Console.WriteLine($"    step {s}: feet {K52V(p)} grounded {sim.mover.grounded} vy {K52F(sim.mover.velocity.y)} state {sim.mover.state} room {roomAt()} awake {sim.zones.padEnabled[i]} from pad {K52F(d)} m dy {K52F(p.y - pd.pos.y)}; JumpPad {jp - jp0}, jumpPad events {ev - ev0}");
                if (jp > jp0 && s > still + 2) break;
            }
            bool ok = jp > jp0;
            if (!ok) fails++;
            Console.WriteLine($"pad52raw {room} pad {i}: JumpPad {jp - jp0}, jumpPad events {ev - ev0} -> {(ok ? "PASS" : "FAIL")}");
        }
        return fails == 0 ? 0 : 1;
    }

    static int K52Pads(GameConfig cfg, string path, string id, string room, Dictionary<string, object> json, double x0, double z0, double x1, double z1)
    {
        var pads = new List<(int i, Vec3 at, Vec3 target, bool aimed)>();
        if (json.TryGetValue("pads", out object po))
        {
            List<object> list = PortData.Arr(po);
            for (int i = 0; i < list.Count; i++)
            {
                var d = PortData.Obj(list[i]);
                var at = new Vec3(PortData.Num(d["x"]), PortData.Num(d["y"]), PortData.Num(d["z"]));
                if (at.x < x0 || at.x > x1 || at.z < z0 || at.z > z1) continue;
                bool aimed = d.TryGetValue("target", out object to) && to != null;
                Vec3 target = at;
                if (aimed)
                {
                    if (to is List<object> ta) target = new Vec3(PortData.Num(ta[0]), PortData.Num(ta[1]), PortData.Num(ta[2]));
                    else { var td = PortData.Obj(to); target = new Vec3(PortData.Num(td["x"]), PortData.Num(td["y"]), PortData.Num(td["z"])); }
                }
                pads.Add((i, at, target, aimed));
            }
        }
        if (pads.Count == 0) { Console.WriteLine($"pad52 {room}: no jump pad inside the room's rect (the map's pads: {(json.ContainsKey("pads") ? PortData.Arr(json["pads"]).Count : 0)})"); return 1; }
        int fails = 0;
        foreach (var pd in pads)
        {
            if (!K52Enter(cfg, path, id, room, 1, out SurvivalSim sim, out RoomDef rd, out string why)) { Console.WriteLine($"pad52: RED: {why}"); return 2; }
            sim.health.max = sim.health.hp = 1e6;
            int launches = 0;
            sim.events.On<int>("jumpPad", i => { if (i == pd.i) launches++; });
            sim.Respawn(pd.at, 0);
            double apex = pd.at.y;
            bool left = false, landed = false;
            int max = MovementMath.StepsFor(5, sim.step);
            for (int s = 0; s < max && !landed; s++)
            {
                sim.Step();
                sim.health.hp = sim.health.max;
                Vec3 p = sim.mover.position;
                apex = Math.Max(apex, p.y);
                if (!sim.mover.grounded) left = true;
                else if (left && s > 6) landed = true;
            }
            Vec3 f = sim.mover.position;
            double miss = Math.Sqrt((f.x - pd.target.x) * (f.x - pd.target.x) + (f.z - pd.target.z) * (f.z - pd.target.z));
            bool pass = launches > 0 && landed && (!pd.aimed || (miss <= 1.5 && Math.Abs(f.y - pd.target.y) <= 0.3));
            if (!pass) fails++;
            Console.WriteLine($"pad52 {room} pad {pd.i} at {K52V(pd.at)} target {(pd.aimed ? K52V(pd.target) : "plain")} launched {(launches > 0 ? "yes" : "no")} apex {K52F(apex)} landed {(landed ? K52V(f) : "no (" + K52V(f) + ")")} miss {K52F(miss)} m {(pass ? "PASS" : "FAIL")}");
        }
        return fails == 0 ? 0 : 1;
    }
}
