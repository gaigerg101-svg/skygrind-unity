// NIGHT46 builder A1 (span46: level 4's arenas rebuilt from their identity cards + flow plans; docs/ROOMS.md "NIGHT46 identity cards").
// Hooked without a Program.cs line: a module initializer runs the command and exits when argv[1] is "n46span" (no lead file touched).
//   n46span reach [--level n46-span] [--run s]
//       the lead's card-gate note 2: every air-loop link measured with the real Mover on the built level before the far edges are fixed. Per link:
//       the player stands (or hangs in the air) at the link's take-off, looks at the ring, taps the grapple (one charge), then steers toward the
//       link's landing (holding jump where the landing is a ride). Prints the hook (attached or not), the arrival, where the feet come down (or the
//       ride's steps on the wall) and PASS when the landing is the intended surface. --run s: a run of s seconds toward the ring before the tap.
// Room-local coordinates as the generator's (Tools/night/gen-n46-span.cjs): lx east of the room's centre line, lz north of its entry (south) edge.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void A1N46SpanInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n46span") return;
        string[] args = argv.Skip(1).ToArray();
        int code = 0;
        try { code = A1N46Span(args); }
        catch (Exception e) { Console.WriteLine("n46span: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static string A1S46Arg(string[] args, string key, string def)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
    }

    static int A1N46Span(string[] args)
    {
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "reach";
        if (what == "reach") return A1S46Reach(args);
        if (what == "walk") return A1S46Walk(args);
        if (what == "view") return A1S46View(args);
        Console.WriteLine("n46span: reach [--level id] [--trace name] | walk [--off] [--seeds n] [--first n] [--from R] [--to R] [--cap s]");
        return 2;
    }

    // ---- view ---------------------------------------------------------------------------------------------------------------------------------
    /// <summary>The lead's round-3 ask (Part D 1: the lip shows the arena and wave 1): from the reel a146's entrance eye (1.5 m inside each door,
    /// looking north; p1: its spawn), each wave-1 node's body (node + 1 m) and the room's landmark: inside the view (bearing <= 53 deg, FLOW-AUDIT's
    /// 74 deg vertical FOV at 16:9 -> +-37 deg up / down) and a clear line (the world's collider ray). PASS when every landmark is seen and at least
    /// all but one of wave 1's nodes.</summary>
    static int A1S46View(string[] args)
    {
        string level = A1S46Arg(args, "--level", "n46-span");
        string path = "Design/levels/" + level + ".json";
        var json = PortData.Obj(PortData.LoadJson(path));
        LevelDef lv = LevelDef.From(json);
        SurvivalSim sim = SurvivalSim.Headless(Config(), SurvivalMap.FromExport(json));
        var w1 = new Dictionary<string, List<Vec3>>();
        foreach (object o in PortData.Arr(PortData.Obj(json["campaign"])["rooms"]))
        {
            var rr = PortData.Obj(o);
            var l = new List<Vec3>();
            if (rr.TryGetValue("waveNodes43", out object wn) && wn is List<object> wl && wl.Count > 0 && wl[0] is List<object> w0)
                foreach (object q in w0) { var a = PortData.Arr(q); l.Add(new Vec3(PortData.Num(a[0]), PortData.Num(a[1]) + 1.0, PortData.Num(a[2]))); }
            w1[rr["id"] as string] = l;
        }
        var rooms = new (string id, double ex, double ez, (string name, double lx, double y, double lz)[] marks)[]
        {
            ("p1", 0, 2, new[] { ("the GREAT HOOK (its ring)", -8.0, 9.0, 24.0), ("the CABLE DRUM's face", 0.0, 3.0, 15.0) }),
            ("p3", 0, 1.5, new[] { ("the KNOT (its mass's south face)", 0.0, 13.0, 24.15), ("the snapped cable (its west rail's top)", -9.15, 1.0, 20.0), ("a wave-2 spitter's spot on AE (body)", 13.0, 3.5, 22.0) }),
            ("p5", 10, 1.5, new[] { ("the GREAT SPOOL (its top)", 0.0, 3.0, 19.0) }),
        };
        int fails = 0;
        foreach (var rm in rooms)
        {
            RoomDef rd = lv.rooms.Find(r => r.id == rm.id);
            var fr = new A1S46Frame(rd);
            Vec3 eye = fr.P(rm.ex, 1.6, rm.ez);
            (bool inView, bool clear, double bearing) See(Vec3 t)
            {
                Vec3 d = t - eye;
                double flat = Math.Sqrt(d.x * d.x + d.z * d.z);
                double bearing = Math.Abs(Math.Atan2(d.x, -d.z)) * 180 / Math.PI, up = Math.Atan2(d.y, flat) * 180 / Math.PI;
                double len = d.Length;
                bool clear = !(sim.world.RayIntersect(eye, d / len, out RayHit hit) && hit.distance < len - 0.6);
                return (bearing <= 53 && Math.Abs(up) <= 37, clear, bearing);
            }
            int seen = 0, n = w1[rm.id].Count;
            var lines = new List<string>();
            foreach (Vec3 t in w1[rm.id]) { var s = See(t); if (s.inView && s.clear) seen++; lines.Add($"({fr.Lx(t):0},{fr.Lz(t):0}) {(s.inView ? "" : "OUT ")}{(s.clear ? "clear" : "BLOCKED")}"); }
            bool marksOk = true;
            var ml = new List<string>();
            foreach (var mk in rm.marks) { var s = See(fr.P(mk.lx, mk.y, mk.lz)); bool ok = s.inView && s.clear; marksOk &= ok; ml.Add($"{mk.name}: {(ok ? "seen" : s.inView ? "BLOCKED" : "OUT of view")} ({s.bearing:0} deg)"); }
            bool pass = marksOk && seen >= n - 1;
            if (!pass) fails++;
            Console.WriteLine($"  {rm.id} from lx {rm.ex} lz {rm.ez}: wave 1 {seen}/{n} in view [{string.Join("; ", lines)}]; {string.Join("; ", ml)}: {(pass ? "PASS" : "FAIL")}");
        }
        Console.WriteLine($"n46span view {level}: {(fails == 0 ? "GREEN" : "RED")} (the entrance frame's eye; wave 1's bodies and the landmark)");
        return fails == 0 ? 0 : 1;
    }

    // ---- walk ---------------------------------------------------------------------------------------------------------------------------------
    /// <summary>The walk pilot (Walk38Pilot + the ported autopilot brain; Walk38Span's gates on level 4) on the campaign as the config ships it with
    /// design.n46.span46 forced ON in memory (the campaign loads n46-span through NightRows / N45Levels.Swap); --off: the row left off (n40-span, the
    /// control). Per room: cleared k of n, median / max clear time, deaths, lip returns (fallReturn43), stage respawns (respawnY), the bodies ended
    /// by rooms43 (out of the room / in the void: no kill).</summary>
    static int A1S46Walk(string[] args)
    {
        bool off = Array.IndexOf(args, "--off") >= 0;
        int seeds = int.Parse(A1S46Arg(args, "--seeds", "10"), CultureInfo.InvariantCulture), first = int.Parse(A1S46Arg(args, "--first", "1"), CultureInfo.InvariantCulture);
        double cap = double.Parse(A1S46Arg(args, "--cap", "400"), CultureInfo.InvariantCulture);
        string fromId = A1S46Arg(args, "--from", "p1"), toId = A1S46Arg(args, "--to", "p5");
        GameConfig cfg = Config();
        var row = PortData.Obj(PortData.Obj(PortData.Obj(cfg.root["design"])["n46"])["span46"]);
        row["enabled"] = !off;
        string id = off ? cfg.Str("design.n46.span46.levelFrom") : cfg.Str("design.n46.span46.levelTo");
        string path = System.IO.Path.GetFullPath(System.IO.Path.Combine("Design", "levels", id + ".json"));
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int a = level.rooms.FindIndex(q => q.id == fromId), b = level.rooms.FindIndex(q => q.id == toId);
        if (a < 0 || b < a) { Console.WriteLine($"n46span walk: rooms {fromId}..{toId} not in {id}"); return 2; }
        var clears = new Dictionary<int, List<double>>();
        var deaths = new int[level.rooms.Count];
        var falls = new int[level.rooms.Count];
        var respawns = new int[level.rooms.Count];
        int runsDone = 0, ended = 0;
        var fallFrom = new Dictionary<string, int>();
        var dwellWhy = new Dictionary<string, int>();
        var dwellSteps = new int[level.rooms.Count];
        var doorRect = new Dictionary<string, double[]>(); // the entry door's rect per room (the door whose second room it is)
        foreach (object o in PortData.Arr(PortData.Obj(PortData.Obj(PortData.LoadJson(path))["campaign"])["doors"]))
        {
            var d = PortData.Obj(o);
            var bt = PortData.Arr(d["between"]);
            var rc = PortData.Arr(d["rect"]);
            doorRect[(string)bt[1]] = new[] { PortData.Num(rc[0]), PortData.Num(rc[1]), PortData.Num(rc[2]), PortData.Num(rc[3]) };
        }
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
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
            var pilot = new Walk38Pilot(sim, ap);
            sim.UseInput(pilot);
            int st = 0, curRoom = -1;
            var enterStep = new Dictionary<int, int>();
            sim.events.On<RoomEvent>("roomEnter", e => { if (!enterStep.ContainsKey(e.room)) enterStep[e.room] = st; curRoom = e.room; });
            sim.events.On<RoomEvent>("roomClear", e =>
            {
                if (e.room < a || e.room > b) return;
                if (!clears.TryGetValue(e.room, out var l)) clears[e.room] = l = new List<double>();
                l.Add(enterStep.TryGetValue(e.room, out int es) ? (st - es) * sim.step : e.time);
            });
            var fallSteps = new List<int>();
            var hpHist = new double[MovementMath.StepsFor(20, 1.0 / 120)];
            int lastWave = 0;
            string lastAlive = "";
            sim.events.On<RoomEvent>("roomRetry", e =>
            {
                if (e.room < a || e.room > b) return;
                deaths[e.room]++;
                int recent = fallSteps.Count(x => st - x <= MovementMath.StepsFor(20, sim.step));
                double hp20 = hpHist[(st + 1) % hpHist.Length];
                Console.WriteLine($"    seed {seed} death in {level.rooms[e.room].id} (wave {lastWave}, alive {lastAlive}): health 20 s before {hp20:0}; lip returns in those 20 s {recent} (= {recent * 5} health); the rest from enemies ~{Math.Max(0, hp20 - recent * 5):0}");
            });
            sim.events.On<FallReturn43Event>("fallReturn43", e => fallSteps.Add(st));
            sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room >= a && e.room <= b) respawns[e.room]++; });
            sim.StartRun(seed, null, null);
            if (s == 0) Console.WriteLine($"n46span walk: the campaign loaded {sim.campaignLevel?.id} (BaseId -> {N45Levels.BaseId(cfg, sim.campaignLevel?.id)}); rooms43 {sim.n43.rooms43}; span46 {(off ? "off (the control)" : "forced on")}");
            if (sim.campaignLevel == null || sim.campaignLevel.id != id) { Console.WriteLine($"n46span walk: RED: the campaign loaded {sim.campaignLevel?.id}, not {id}"); return 1; }
            int max = MovementMath.StepsFor(cap, sim.step);
            bool done = false, placed = a == 0, doorFreeOn = a > 0;
            int fall0 = 0;
            Vec3 lastFloor = default;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (!placed && sim.campaign != null && sim.campaign.room >= 0)
                {
                    sim.campaign.SetDoorFree29(true);
                    sim.Respawn(level.rooms[a].spawn, level.rooms[a].spawnYawDeg);
                    placed = true;
                }
                if (placed && doorFreeOn && sim.campaign != null && sim.campaign.room == a) { sim.campaign.SetDoorFree29(false); doorFreeOn = false; }
                if (sim.campaign != null)
                {
                    int f = sim.campaign.fallReturns43;
                    if (f > fall0 && curRoom >= a && curRoom <= b)
                    {
                        falls[curRoom] += f - fall0;
                        // where the fall began: the last spot the feet stood on a floor (local lx / lz, 2 m bins, and its height)
                        var fr = new A1S46Frame(level.rooms[curRoom]);
                        string key = $"{level.rooms[curRoom].id} from lx {Math.Round(fr.Lx(lastFloor) / 2) * 2:0} lz {Math.Round(fr.Lz(lastFloor) / 2) * 2:0} y {lastFloor.y:0.0}";
                        fallFrom.TryGetValue(key, out int n0);
                        fallFrom[key] = n0 + 1;
                    }
                    fall0 = f;
                }
                if (sim.mover.grounded) lastFloor = sim.mover.position;
                hpHist[st % hpHist.Length] = sim.health.hp;
                if (sim.campaign != null && sim.campaign.wave > 0) lastWave = sim.campaign.wave;
                if (st % 60 == 0 && sim.enemies.AliveCount > 0) lastAlive = string.Join(", ", sim.enemies.all.Where(en => en.alive).GroupBy(en => en.kind).Select(gq => $"{gq.Key} x{gq.Count()}"));
                // the dwell (F1's rule: within 4 m of the room's entry door rect, the first room: its spawn, while an enemy lives): what holds it
                if (curRoom >= a && curRoom <= b && sim.campaign != null && sim.campaign.room == curRoom && sim.campaign.phase == Campaign24.Phase.Fight)
                {
                    Vec3 p = sim.mover.position;
                    string rid = level.rooms[curRoom].id;
                    double ex, ez;
                    if (doorRect.TryGetValue(rid, out double[] dr)) { ex = Math.Max(dr[0], Math.Min(p.x, dr[2])); ez = Math.Max(dr[1], Math.Min(p.z, dr[3])); }
                    else { ex = level.rooms[curRoom].spawn.x; ez = level.rooms[curRoom].spawn.z; }
                    Enemy near = null;
                    double best = double.MaxValue;
                    foreach (Enemy en in sim.enemies.all)
                    {
                        if (!en.alive || en.standIn29) continue;
                        double d = (en.position.x - p.x) * (en.position.x - p.x) + (en.position.z - p.z) * (en.position.z - p.z);
                        if (d < best) { best = d; near = en; }
                    }
                    if (near != null && (ex - p.x) * (ex - p.x) + (ez - p.z) * (ez - p.z) <= 16)
                    {
                        var fr = new A1S46Frame(level.rooms[curRoom]);
                        string key = $"{rid} w{sim.campaign.wave} nearest {near.kind} {(near.position.y > 1 ? "high" : "low")} at {Math.Round(Math.Sqrt(best) / 5) * 5:0} m (lz {Math.Round(fr.Lz(near.position) / 4) * 4:0}){(sim.mover.grounded ? "" : " pilot airborne")}";
                        dwellWhy.TryGetValue(key, out int n1);
                        dwellWhy[key] = n1 + 1;
                        dwellSteps[curRoom]++;
                    }
                }
                if (clears.ContainsKey(b) && clears[b].Count > runsDone) { done = true; break; }
            }
            ended += sim.rooms43Ended;
            if (done) runsDone++;
            else
            {
                Vec3 p = sim.mover.position;
                Campaign24 c = sim.campaign;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0.0} s in {(c != null && c.room >= 0 ? level.rooms[c.room].id : "?")} {c?.phase} wave {c?.wave} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), state {sim.mover.state}, alive {sim.enemies.AliveCount}, hp {sim.health.hp:0}");
            }
            ap.Detach();
        }
        string tag = $"n46span walk {id}{(off ? " (--off: the control)" : " (span46 on)")}";
        for (int q = a; q <= b; q++)
        {
            var l = clears.TryGetValue(q, out var xx) ? xx : new List<double>();
            l.Sort();
            string med = l.Count == 0 ? "-" : l[l.Count / 2].ToString("0.0", CultureInfo.InvariantCulture);
            string mx = l.Count == 0 ? "-" : l[l.Count - 1].ToString("0.0", CultureInfo.InvariantCulture);
            Console.WriteLine($"{tag} {level.rooms[q].id}: cleared {l.Count}/{seeds}, median {med} s, max {mx} s, deaths {deaths[q]}, lip returns {falls[q]}, stage respawns {respawns[q]}");
        }
        Console.WriteLine($"{tag}: bodies ended by rooms43 (no kill) {ended} over {seeds} runs");
        for (int q = a; q <= b; q++) if (level.rooms[q].kind == "room") Console.WriteLine($"{tag} {level.rooms[q].id}: dwell {dwellSteps[q] / 120.0 / seeds:0.0} s a run (the mean; F1's meter prints the median)");
        Console.WriteLine($"{tag}: the dwell by what the nearest live enemy is (top 16, s over all runs): " + string.Join("; ", dwellWhy.OrderByDescending(kv => kv.Value).Take(16).Select(kv => $"{kv.Key} {kv.Value / 120.0:0.0}")));
        Console.WriteLine($"{tag}: lip returns by where the fall began (top 12): " + string.Join("; ", fallFrom.OrderByDescending(kv => kv.Value).Take(12).Select(kv => $"{kv.Key} x{kv.Value}")));
        Console.WriteLine($"{tag} SUMMARY: {(runsDone == seeds ? "GREEN" : "RED")} {runsDone}/{seeds} runs through {fromId}..{toId}");
        return runsDone == seeds ? 0 : 1;
    }

    /// <summary>A room's local frame (the generator's): lx east of the centre line, lz north of the entry (south, larger world z) edge.</summary>
    sealed class A1S46Frame
    {
        public readonly RoomDef r;
        readonly double cx, zS;
        public A1S46Frame(RoomDef r) { this.r = r; cx = (Math.Min(r.x0, r.x1) + Math.Max(r.x0, r.x1)) / 2; zS = Math.Max(r.z0, r.z1); }
        public Vec3 P(double lx, double y, double lz) => new Vec3(cx + lx, y, zS - lz);
        public double Lx(Vec3 p) => p.x - cx;
        public double Lz(Vec3 p) => zS - p.z;
    }

    /// <summary>One air-loop link: take-off (local, feet), the ring (local), the landing (a local rect and its top y, or a ride: rect = where the
    /// wall-ride must happen), the steer target after the arrival (local), hold jump after the arrival (a ride).</summary>
    sealed class A1S46Link
    {
        public string room, name;
        public double fx, fy, fz, rx, ry, rz, lx0, lz0, lx1, lz1, top, sx, sz;
        public bool ride, air;
        public int ring = -1; // the room's grappleNodes index in the level file (the ring read from the data, never typed twice)
    }

    sealed class A1S46Input : IPlayerInput
    {
        readonly SurvivalSim sim;
        public double yaw, pitch;
        public int s, tapAt = -1;
        public bool steer, holdJump, run;
        public double runYaw;
        public A1S46Input(SurvivalSim sim) { this.sim = sim; }
        public double Yaw => yaw;
        public double Pitch => pitch;
        public void SetLook(double y, double p) { }
        public bool Captured => true;
        public InputFrame SampleMove(bool active)
        {
            s++;
            var f = new InputFrame { yaw = yaw, pitch = pitch };
            if (run || steer) f.moveZ = 1;
            if (s == tapAt) f.grapple = new Button { pressed = true, held = true };
            if (holdJump && !sim.mover.grounded) f.jump = new Button { held = true }; // a held jump on a wall is the ride (a press in the air is the double jump)
            return f;
        }
        public CombatInput SampleCombat(bool active) => new CombatInput { weapon = -1 };
    }

    static (double yaw, double pitch) A1S46Look(Vec3 from, Vec3 to)
    {
        double dx = to.x - from.x, dy = to.y - from.y, dz = to.z - from.z;
        return (Math.Atan2(-dx, -dz), Math.Atan2(dy, Math.Sqrt(dx * dx + dz * dz)));
    }

    static int A1S46Reach(string[] args)
    {
        string level = A1S46Arg(args, "--level", "n46-span");
        double runS = double.Parse(A1S46Arg(args, "--run", "0"), CultureInfo.InvariantCulture);
        string trace = A1S46Arg(args, "--trace", null); // a link-name substring: print its path every 0.1 s
        string path = "Design/levels/" + level + ".json";
        LevelDef lv = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        var links = new List<A1S46Link>
        {
            new A1S46Link { room = "p1", name = "floor -> GREAT HOOK -> the terrace (the lesson)", fx = -4, fy = 0, fz = 14, ring = 0, lx0 = -16, lz0 = 18, lx1 = -8, lz1 = 36, top = 4, sx = -12, sz = 28 },
            new A1S46Link { room = "p1", name = "terrace front -> HOIST ring -> the well's ride", fx = -9.5, fy = 4, fz = 21, ring = 1, lx0 = 13.5, lz0 = 10, lx1 = 16, lz1 = 28, top = -1, sx = 16, sz = 24, ride = true },
            new A1S46Link { room = "p1", name = "the well's ride (north end, in the air) -> GREAT HOOK -> the terrace", fx = 14.6, fy = 3, fz = 26, ring = 0, lx0 = -16, lz0 = 18, lx1 = -8, lz1 = 36, top = 4, sx = -12, sz = 25, air = true },
            new A1S46Link { room = "p3", name = "AW north end -> RN -> AE", fx = -15, fy = 2.5, fz = 28, ring = 2, lx0 = 11, lz0 = 9, lx1 = 18, lz1 = 31, top = 2.5, sx = 15, sz = 28 },
            new A1S46Link { room = "p3", name = "AE south end -> RS -> AW", fx = 15, fy = 2.5, fz = 12, ring = 0, lx0 = -18, lz0 = 9, lx1 = -11, lz1 = 31, top = 2.5, sx = -15, sz = 12 },
            new A1S46Link { room = "p3", name = "entry deck -> KNOT -> the exit deck", fx = 0, fy = 0, fz = 8, ring = 1, lx0 = -11, lz0 = 29, lx1 = 11, lz1 = 40, top = 0, sx = 0, sz = 34 },
            new A1S46Link { room = "p5", name = "gallery -> R1 -> the GREAT SPOOL", fx = 15.5, fy = 4, fz = 20, ring = 0, lx0 = -5, lz0 = 15, lx1 = 5, lz1 = 23, top = 2.5, sx = 0, sz = 19 },
            new A1S46Link { room = "p5", name = "spool -> R2 -> the tail spool", fx = -3, fy = 2.5, fz = 17, ring = 1, lx0 = -18, lz0 = 0, lx1 = -14, lz1 = 4, top = 2.5, sx = -16, sz = 2 },
            new A1S46Link { room = "p5", name = "the south ride's east end (in the air) -> R3 -> the gallery", fx = 5, fy = 3, fz = 1.4, ring = 2, lx0 = 13, lz0 = 12, lx1 = 18, lz1 = 29, top = 4, sx = 15.5, sz = 18, air = true },
        };
        // the rings from the level file (campaign.rooms[].grappleNodes, world metres)
        var ringsOf = new Dictionary<string, List<Vec3>>();
        foreach (object o in PortData.Arr(PortData.Obj(PortData.Obj(PortData.LoadJson(path))["campaign"])["rooms"]))
        {
            var rr = PortData.Obj(o);
            var l = new List<Vec3>();
            if (rr.TryGetValue("grappleNodes", out object gn) && gn is List<object> gl)
                foreach (object q in gl) { var a = PortData.Arr(q); l.Add(new Vec3(PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]))); }
            ringsOf[rr["id"] as string] = l;
        }
        int fails = 0;
        Console.WriteLine($"n46span reach {level}: the real Mover, one tap-pull per link{(runS > 0 ? $", a {runS:0.0} s run toward the ring first" : ", from a standstill")}");
        foreach (A1S46Link k in links)
        {
            GameConfig cfg = Config();
            SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            sim.StartRun(1, null, null);
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            sim.husks.KillAll();
            int ri = lv.rooms.FindIndex(q => q.id == k.room);
            typeof(Campaign24).GetMethod("Enter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(sim.campaign, new object[] { ri });
            var fr = new A1S46Frame(lv.rooms[ri]);
            Vec3 from = fr.P(k.fx, k.fy, k.fz), ring = k.ring >= 0 ? ringsOf[k.room][k.ring] : fr.P(k.rx, k.ry, k.rz);
            bool ropeWas = sim.mover.ropeEnabled;
            sim.mover.ropeEnabled = true;
            var input = new A1S46Input(sim);
            sim.UseInput(input);
            Vec3 start = from;
            if (runS > 0) { var lk0 = A1S46Look(from, ring); start = new Vec3(from.x + Math.Sin(lk0.yaw) * runS * 8, from.y, from.z + Math.Cos(lk0.yaw) * runS * 8); }
            var look = A1S46Look(new Vec3(start.x, start.y + 1.6, start.z), ring);
            sim.Respawn(start, look.yaw * 180 / Math.PI);
            input.yaw = look.yaw; input.pitch = look.pitch;
            sim.grappleCharges32 = Math.Max(sim.grappleCharges32, 2);
            int settle = k.air ? 1 : 12;
            int runSteps = MovementMath.StepsFor(runS, sim.step);
            input.run = runSteps > 0;
            input.tapAt = settle + runSteps;
            bool hooked = false, released = false, down = false, fell = false;
            Vec3 arrive = default, land = default;
            int rideSteps = 0, rideIn = 0, maxSteps = MovementMath.StepsFor(6, sim.step);
            int falls0 = sim.campaign.fallReturns43;
            for (int st = 0; st < maxSteps && !down; st++)
            {
                sim.husks.KillAll();
                AutopilotInput.TakeDraft(sim);
                if (input.s >= input.tapAt - 1 && input.s < input.tapAt + 1)
                {
                    var lk = A1S46Look(sim.mover.position + new Vec3(0, 1.6, 0), ring); // aim at the ring from where the eye is now
                    input.yaw = lk.yaw; input.pitch = lk.pitch;
                }
                sim.Step();
                if (sim.mover.rope.phase != RopePhase.None) hooked = true;
                if (hooked && !released && sim.mover.rope.phase == RopePhase.None)
                {
                    released = true;
                    arrive = sim.mover.position;
                    input.steer = true; input.run = false;
                    input.holdJump = k.ride;
                }
                if (trace != null && k.name.Contains(trace) && (st % 12 == 0 || (released && sim.mover.grounded)))
                    Console.WriteLine($"      t {st * sim.step:0.00} lx {fr.Lx(sim.mover.position):0.0} lz {fr.Lz(sim.mover.position):0.0} y {sim.mover.position.y:0.0} v ({sim.mover.velocity.x:0.0}, {sim.mover.velocity.y:0.0}, {sim.mover.velocity.z:0.0}) {sim.mover.state} rope {sim.mover.rope.phase} grounded {sim.mover.grounded}");
                if (released)
                {
                    Vec3 p = sim.mover.position;
                    var lk = A1S46Look(p, fr.P(k.sx, p.y, k.sz));
                    input.yaw = lk.yaw; input.pitch = 0;
                    if (sim.mover.state == MoveState.WallRide)
                    {
                        rideSteps++;
                        double lx = fr.Lx(p), lz = fr.Lz(p);
                        if (lx >= k.lx0 && lx <= k.lx1 && lz >= k.lz0 && lz <= k.lz1) rideIn++;
                    }
                    if (sim.campaign.fallReturns43 > falls0) { fell = true; down = true; land = p; }
                    else if (sim.mover.grounded) { down = true; land = p; }
                    else if (k.ride && rideIn > MovementMath.StepsFor(0.5, sim.step)) { down = true; land = p; }
                }
            }
            sim.mover.ropeEnabled = ropeWas;
            bool onTarget;
            string where;
            if (k.ride)
            {
                onTarget = rideIn > MovementMath.StepsFor(0.5, sim.step);
                where = $"rode {rideSteps * sim.step:0.00} s ({rideIn * sim.step:0.00} s on the intended wall)";
            }
            else
            {
                double lx = fr.Lx(land), lz = fr.Lz(land);
                onTarget = down && !fell && lx >= k.lx0 - 0.3 && lx <= k.lx1 + 0.3 && lz >= k.lz0 - 0.3 && lz <= k.lz1 + 0.3 && Math.Abs(land.y - k.top) < 0.6;
                where = !down ? "never down in 6 s" : fell ? $"FELL (a lip return) at lx {lx:0.0} lz {lz:0.0}" : $"down at lx {lx:0.0} lz {lz:0.0} y {land.y:0.0}";
            }
            // the margin: how far inside the landing's edges that face the take-off the feet came down (the edge a short pull falls past;
            // a wall on the far side stops an overshoot), negative = short
            double margin = double.NaN;
            if (!k.ride && down && !fell)
            {
                double lx = fr.Lx(land), lz = fr.Lz(land);
                margin = double.PositiveInfinity;
                if (k.fx < k.lx0) margin = Math.Min(margin, lx - k.lx0);
                if (k.fx > k.lx1) margin = Math.Min(margin, k.lx1 - lx);
                if (k.fz < k.lz0) margin = Math.Min(margin, lz - k.lz0);
                if (k.fz > k.lz1) margin = Math.Min(margin, k.lz1 - lz);
                if (double.IsPositiveInfinity(margin)) margin = Math.Min(Math.Min(lx - k.lx0, k.lx1 - lx), Math.Min(lz - k.lz0, k.lz1 - lz));
                if (margin < 1.0) onTarget = false; // the lead's gate note 2: reachable WITH margin (1 m), never a perfect pull only
            }
            if (!onTarget) fails++;
            Console.WriteLine($"  {k.room} {k.name}: hook {(hooked ? "yes" : "NO")} ({Vec3.DistanceTo(start + new Vec3(0, 1.6, 0), ring):0.0} m), "
                + (released ? $"released at lx {fr.Lx(arrive):0.0} lz {fr.Lz(arrive):0.0} y {arrive.y:0.0}" : "never released")
                + $"; {where}{(double.IsNaN(margin) ? "" : $", {margin:0.0} m inside the nearest edge")}: {(onTarget ? "PASS" : "FAIL")}");
        }
        Console.WriteLine($"n46span reach {level}: {(fails == 0 ? "GREEN" : "RED")} ({links.Count - fails} / {links.Count} links land where the flow plan says)");
        return fails == 0 ? 0 : 1;
    }
}
