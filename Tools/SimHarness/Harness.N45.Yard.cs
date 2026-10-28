// NIGHT45 builder A1 (foundry45: b1 THE YARD rebuilt from its identity card as THE SPILL; NIGHT45.md card A1, docs/ROOMS.md "NIGHT45 identity
// cards"). Hooked without a Program.cs line: a module initializer runs the command and exits when argv[1] is "n45yard" (no lead file touched).
//   n45yard walk [--level n45-foundry] [--off] [--seeds n] [--first n] [--from ROOM] [--to ROOM] [--cap s]
//       the walk pilot (Walk38Pilot + the ported autopilot brain, as L1's `n43level walk`, Harness.N43.Level.cs) on the campaign as the config ships it
//       (NIGHT44's game: the n43 rows as they are ON) with design.n45.foundry45 forced ON in memory, so the campaign itself loads --level (the swap is
//       Curriculum25 -> N45Levels.Swap; nothing here reads a level id directly: N45Levels.BaseId is the only mapping). --off: the row left off (the
//       control: n43-foundry). Per room: cleared k of n, the median clear time, deaths; in b1: the enemies killed by the slag (lavaEnemyDeath), the
//       player's burns (lavaBurn), the waves seen, where each wave spawned (far bank / rim / near bank).
//   n45yard plan [--level n45-foundry]
//       the level file's b1 as the card asks for it (counts read from the generated JSON): lava rects, ride walls, the rim, the bowl, the ramps, the
//       props with a job, the lights (one accent), each wave's waveNodes43 side; prints RED on a missing card field.
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
    internal static void A1N45YardInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n45yard") return;
        string[] args = argv.Skip(1).ToArray();
        int code = 0;
        try { code = A1N45Yard(args); }
        catch (Exception e) { Console.WriteLine("n45yard: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static string A1Arg(string[] args, string key, string def)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
    }

    static bool A1Flag(string[] args, string key) => Array.IndexOf(args, key) >= 0;

    static int A1N45Yard(string[] args)
    {
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "walk";
        if (what == "walk") return A1Walk(args);
        if (what == "plan") return A1Plan(args);
        if (what == "ride") return A1Ride(args);
        Console.WriteLine("n45yard: walk [--level id] [--off] [--seeds n] [--from R] [--to R] [--cap s] | plan [--level id]");
        return 2;
    }

    /// <summary>design.n45.foundry45 forced on (or off) in this process's config only (set before any read: GameConfig caches its paths).</summary>
    static GameConfig A1Config(bool on, string levelTo)
    {
        GameConfig cfg = Config();
        var row = PortData.Obj(PortData.Obj(PortData.Obj(cfg.root["design"])["n45"])["foundry45"]);
        row["enabled"] = on;
        if (levelTo != null) row["levelTo"] = levelTo;
        return cfg;
    }

    static string A1Side(Vec3 p)
    {
        if (p.z > -154) return "near";
        if (p.z < -166) return "far";
        return p.y > 1 ? "rim" : "river";
    }

    static int A1Walk(string[] args)
    {
        bool off = A1Flag(args, "--off");
        string levelId = A1Arg(args, "--level", "n45-foundry");
        int seeds = int.Parse(A1Arg(args, "--seeds", "10"), CultureInfo.InvariantCulture), first = int.Parse(A1Arg(args, "--first", "1"), CultureInfo.InvariantCulture);
        double cap = double.Parse(A1Arg(args, "--cap", "300"), CultureInfo.InvariantCulture);
        string fromId = A1Arg(args, "--from", "b1"), toId = A1Arg(args, "--to", "b1");
        GameConfig cfg = A1Config(!off, levelId);
        string id = off ? cfg.Str("design.n45.foundry45.levelFrom") : levelId;
        string path = Path.GetFullPath(Path.Combine("Design", "levels", id + ".json"));
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int a = level.rooms.FindIndex(q => q.id == fromId), b = level.rooms.FindIndex(q => q.id == toId);
        if (a < 0 || b < a) { Console.WriteLine($"n45yard walk: rooms {fromId}..{toId} not in {id}"); return 2; }
        int yard = level.rooms.FindIndex(q => q.id == "b1");
        var clears = new Dictionary<int, List<double>>();
        var deaths = new int[level.rooms.Count];
        int runsDone = 0, slagKills = 0, burns = 0, waves = 0, kills = 0;
        var sides = new Dictionary<string, int>();
        List<double[]> lavaRects = A1LavaRects(path);
        int lavaExits = 0, edgeHolds = 0;
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
            var pilot0 = new Walk38Pilot(sim, ap);
            var pilot = new A1YardInput(sim, pilot0, lavaRects);
            sim.UseInput(pilot);
            int st = 0;
            bool inYard = false;
            var enterStep = new Dictionary<int, int>();
            sim.events.On<RoomEvent>("roomEnter", e => { if (!enterStep.ContainsKey(e.room)) enterStep[e.room] = st; inYard = e.room == yard; });
            sim.events.On<RoomEvent>("roomClear", e =>
            {
                if (e.room == yard) inYard = false;
                if (e.room < a || e.room > b) return;
                if (!clears.TryGetValue(e.room, out var l)) clears[e.room] = l = new List<double>();
                l.Add(enterStep.TryGetValue(e.room, out int es) ? (st - es) * sim.step : e.time);
            });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room >= a && e.room <= b) deaths[e.room]++; if (A1Flag(args, "--waves")) Console.WriteLine($"    seed {seed} {st * sim.step:0.0} s: RETRY {level.rooms[e.room].id} (feet ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z:0.0}))"); });
            sim.events.On<Lava30Event>("lavaEnemyDeath", e => { if (inYard) slagKills++; });
            sim.events.On<Lava30Event>("lavaBurn", e => { if (inYard) burns++; });
            sim.events.On<KillEvent>("kill", e => { if (inYard) kills++; });
            sim.StartRun(seed, null, null);
            if (s == 0) Console.WriteLine($"n45yard walk: the campaign loaded {sim.campaignLevel?.id} (BaseId -> {N45Levels.BaseId(cfg, sim.campaignLevel?.id)}); rooms43 {sim.n43.rooms43}; foundry45 {cfg.Design("n45.foundry45.enabled")}");
            if (sim.campaignLevel == null || sim.campaignLevel.id != id) { Console.WriteLine($"n45yard walk: RED: the campaign loaded {sim.campaignLevel?.id}, not {id}"); return 1; }
            int max = MovementMath.StepsFor(cap, sim.step);
            bool done = false, placed = a == 0, doorFreeOn = a > 0;
            int seenWave = 0;
            var seen = new HashSet<int>();
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
                if (inYard && sim.campaign != null && sim.campaign.room == yard)
                {
                    if (sim.campaign.wave != seenWave)
                    {
                        if (A1Flag(args, "--waves")) Console.WriteLine($"    seed {seed} {(st - (enterStep.TryGetValue(yard, out int ys) ? ys : 0)) * sim.step:0.0} s: wave {sim.campaign.wave} (alive {sim.enemies.AliveCount}, hp {sim.health.hp:0}, feet ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z:0.0}))");
                        if (sim.campaign.wave > seenWave) waves++;
                        seenWave = sim.campaign.wave;
                    }
                    // each body where it stands on its first step alive (the card's sides: far bank / rim / near bank), first seed only per id
                    foreach (Enemy en in sim.enemies.all)
                        if (en.alive && seen.Add(en.id)) { string sd = $"w{seenWave} {en.kind} {A1Side(en.position)}"; sides.TryGetValue(sd, out int n0); sides[sd] = n0 + 1; }
                }
                if (clears.ContainsKey(b) && clears[b].Count > runsDone) { done = true; break; }
            }
            if (done) runsDone++;
            else
            {
                Vec3 p = sim.mover.position;
                Campaign24 c = sim.campaign;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0.0} s in {(c != null && c.room >= 0 ? level.rooms[c.room].id : "?")} {c?.phase} wave {c?.wave} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), state {sim.mover.state}, alive {sim.enemies.AliveCount}, hp {sim.health.hp:0}");
            }
            lavaExits += pilot.lavaExits; edgeHolds += pilot.edgeHolds;
            ap.Detach();
        }
        string tag = $"n45yard walk {id}{(off ? " (--off: the control)" : " (foundry45 on)")}";
        for (int q = a; q <= b; q++)
        {
            var l = clears.TryGetValue(q, out var xx) ? xx : new List<double>();
            l.Sort();
            string med = l.Count == 0 ? "-" : l[l.Count / 2].ToString("0.0", CultureInfo.InvariantCulture);
            string mx = l.Count == 0 ? "-" : l[l.Count - 1].ToString("0.0", CultureInfo.InvariantCulture);
            Console.WriteLine($"{tag} {level.rooms[q].id}: cleared {l.Count}/{seeds}, median {med} s, max {mx} s, deaths {deaths[q]}");
        }
        Console.WriteLine($"{tag} b1: kills {kills}, by the slag {slagKills}, player burns {burns}, waves seen {waves}; the pilot: out of the slag {lavaExits} steps, held at an edge {edgeHolds} steps");
        Console.WriteLine($"{tag} b1 spawn sides: " + (sides.Count == 0 ? "none" : string.Join(", ", sides.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key} x{kv.Value}"))));
        Console.WriteLine($"{tag} SUMMARY: {(runsDone == seeds ? "GREEN" : "RED")} {runsDone}/{seeds} runs through {fromId}..{toId}");
        return runsDone == seeds ? 0 : 1;
    }

    /// <summary>The walk pilot in THE SPILL (harness only): Walk38Pilot everywhere, plus Walk36's lava rule in b1 (the autopilot's graph has no lava:
    /// out of the slag by the nearest bank, never walk off a bank or a bridge into it: stand at the edge and fight), and once b1 is cleared the
    /// nearer bridge to the exit (the straight line crosses the slag). The Game's Walk38Pilot applies Walk36's rule only on n36 ids: the lead's
    /// hand-off (one hook) if the Player walk needs it.</summary>
    sealed class A1YardInput : IPlayerInput
    {
        readonly SurvivalSim sim;
        readonly Walk38Pilot inner;
        readonly List<double[]> lava;
        int step, wp = -1;
        double side;
        public int lavaExits, edgeHolds;

        public A1YardInput(SurvivalSim sim, Walk38Pilot inner, List<double[]> lava) { this.sim = sim; this.inner = inner; this.lava = lava; }

        public double Yaw => inner.Yaw;
        public double Pitch => inner.Pitch;
        public void SetLook(double y, double p) => inner.SetLook(y, p);
        public bool Captured => inner.Captured;
        public CombatInput SampleCombat(bool active) => inner.SampleCombat(active);

        bool InLava(double x, double z)
        {
            foreach (double[] r in lava) if (x >= r[0] && x <= r[2] && z >= r[1] && z <= r[3]) return true;
            return false;
        }

        static double Toward(Vec3 from, double tx, double tz) => Math.Atan2(-(tx - from.x), -(tz - from.z));

        public InputFrame SampleMove(bool active)
        {
            InputFrame auto = inner.SampleMove(active);
            step++;
            Campaign24 c = sim.campaign;
            if (!active || c == null || c.room < 0 || sim.campaignLevel == null || sim.campaignLevel.rooms[c.room].id != "b1") { wp = -1; return auto; }
            Vec3 p = sim.mover.position;
            bool grounded = sim.mover.grounded;
            InputFrame f = auto;
            if (InLava(p.x, p.z) && p.y < -0.2)
            {
                // out by the nearer bank (the river is z -166..-154)
                lavaExits++;
                double tz = p.z > -160 ? -152.5 : -167.5;
                f.yaw = Toward(p, p.x, tz);
                f.moveX = 0; f.moveZ = 1;
                f.jump = new Button { held = true, pressed = grounded && step % 12 == 0 };
                SetLook(f.yaw, f.pitch);
                return f;
            }
            if (c.phase == Campaign24.Phase.Cleared)
            {
                // to the exit by the nearer bridge: its near end, its far end, the door
                if (wp < 0) { wp = 0; side = p.x < 0 ? -11.5 : 11.5; }
                double[][] via = { new[] { side, -152.5 }, new[] { side, -167.5 }, new[] { 0.0, -178.5 } };
                while (wp < via.Length && JsMath.Hypot(p.x - via[wp][0], p.z - via[wp][1]) < 1.0) wp++;
                if (wp >= via.Length) return auto;
                f = new InputFrame { moveZ = 1, yaw = Toward(p, via[wp][0], via[wp][1]), pitch = 0 };
                SetLook(f.yaw, f.pitch);
                return f;
            }
            if (grounded)
            {
                Vec3 w = MovementMath.WishDirection(auto.moveX, auto.moveZ, auto.yaw);
                if (InLava(p.x + w.x * 1.5, p.z + w.z * 1.5))
                {
                    edgeHolds++;
                    f.moveX = f.moveZ = 0;
                    f.jump = default;
                    f.dash = default;
                }
            }
            return f;
        }
    }

    /// <summary>A1's scripted rider (n45yard ride): run north from (x0, z -146), press jump at the rail's start and hold it leaning into the
    /// wall; `releaseZ` (NaN: never) lets go with the look turned to `relYaw` and pitched `relPitch` (the jump-off reads the pitch).</summary>
    sealed class A1RideInput : IPlayerInput
    {
        readonly SurvivalSim sim;
        readonly double lean, pressZ, releaseZ, relYaw, relPitch;
        double yaw, pitch;
        bool pressed, released;
        public A1RideInput(SurvivalSim sim, double lean, double pressZ, double releaseZ, double relYaw, double relPitch)
        { this.sim = sim; this.lean = lean; this.pressZ = pressZ; this.releaseZ = releaseZ; this.relYaw = relYaw; this.relPitch = relPitch; }
        public double Yaw => yaw;
        public double Pitch => pitch;
        public void SetLook(double y, double p) { yaw = y; pitch = p; }
        public bool Captured => true;
        public CombatInput SampleCombat(bool active) => new CombatInput { weapon = -1 };
        public InputFrame SampleMove(bool active)
        {
            Vec3 p = sim.mover.position;
            var f = new InputFrame { moveZ = 1, moveX = p.z <= pressZ + 0.6 ? lean : 0, yaw = 0, pitch = 0 }; // straight up the bank, the lean only at the rail's start
            if (released) { f.yaw = yaw = relYaw; f.pitch = pitch = relPitch; f.moveX = 0; return f; }
            if (!pressed && p.z <= pressZ) { pressed = true; f.jump = new Button { held = true, pressed = true }; return f; }
            if (pressed && !double.IsNaN(releaseZ) && p.z <= releaseZ && sim.mover.state == MoveState.WallRide)
            {
                released = true;
                f.yaw = yaw = relYaw; f.pitch = pitch = relPitch; f.moveX = 0;
                return f; // jump not held: the release
            }
            if (pressed) f.jump = new Button { held = true, pressed = false };
            return f;
        }
    }

    /// <summary>n45yard ride [--tries n]: the card's verb and shortcut on the real Mover in the n45-foundry campaign (foundry45 on, b1, no enemy yet:
    /// each try is 4 s from the near bank): (A) the bridge face of rail W: the share of tries that ride over the slag to the far bank (feet past
    /// z -166 on the floor, no lava burn); (B) the inner face: the share that jump off onto the ladle's rim (grounded at 2.5 m). Jittered per try
    /// (Mulberry32): the lean, the press point, the release point and pitch.</summary>
    static int A1Ride(string[] args)
    {
        int tries = int.Parse(A1Arg(args, "--tries", "20"), CultureInfo.InvariantCulture);
        string levelId = A1Arg(args, "--level", "n45-foundry");
        GameConfig cfg = A1Config(true, levelId);
        string path = Path.GetFullPath(Path.Combine("Design", "levels", levelId + ".json"));
        var rng = new Mulberry32(45001);
        int[] ok = new int[2], rode = new int[2], burnt = new int[2];
        var ends = new List<string>[] { new List<string>(), new List<string>() };
        for (int way = 0; way < 2; way++)
            for (int t = 0; t < tries; t++)
            {
                SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
                SurvivalSim sim = SurvivalSim.Headless(cfg, map);
                sim.noRecords = sim.noLab = true;
                sim.setup.mode = Campaign24.Mode;
                double j = rng.Next();
                double x0 = way == 0 ? -10.05 - 0.25 * rng.Next() : -8.15 + 0.25 * rng.Next(); // 0.65-0.9 m off the face line (the capsule beside it)
                double lean = (way == 0 ? 1 : -1) * (0.4 + 0.4 * rng.Next());
                double pressZ = -150.6 - 0.8 * rng.Next();
                double releaseZ = way == 0 ? double.NaN : -152.5 - 3.0 * rng.Next(); // round 1: -157.5..-161 flew past the rim (the ride's ~10 m/s north carries ~6 m)
                double relPitch = 0.3 + 0.6 * rng.Next();
                var input = new A1RideInput(sim, lean, pressZ, releaseZ, -Math.PI / 2, relPitch);
                int burns = 0;
                sim.events.On<Lava30Event>("lavaBurn", e => burns++);
                sim.StartRun((uint)(t + 1), null, null);
                bool placed = false, rodeAny = false, success = false;
                int max = MovementMath.StepsFor(60, sim.step), placedAt = -1;
                for (int st = 0; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
                {
                    sim.Step();
                    if (!placed && sim.campaign != null && sim.campaign.room >= 0)
                    {
                        sim.campaign.SetDoorFree29(true);
                        sim.Respawn(new Vec3(x0, 0, -146), 0);
                        sim.UseInput(input);
                        placed = true;
                        placedAt = st;
                        continue;
                    }
                    if (!placed) continue;
                    Vec3 p = sim.mover.position;
                    if (sim.mover.state == MoveState.WallRide) rodeAny = true;
                    if (way == 0 && sim.mover.grounded && p.z < -166.2 && p.y > -0.1 && burns == 0) { success = true; break; }
                    if (way == 1 && sim.mover.grounded && p.y > 2.3 && p.x > -6 && p.x < 6 && p.z > -166 && p.z < -157) { success = true; break; }
                    if (st - placedAt > MovementMath.StepsFor(4, sim.step)) break;
                }
                if (rodeAny) rode[way]++;
                if (burns > 0) burnt[way]++;
                if (success) ok[way]++;
                Vec3 q = sim.mover.position;
                if (ends[way].Count < 6) ends[way].Add($"({q.x:0.0}, {q.y:0.0}, {q.z:0.0}) {(success ? "ok" : sim.mover.state.ToString())}");
            }
        Console.WriteLine($"n45yard ride (A) rail W's bridge face, near bank -> far bank: {ok[0]}/{tries} crossed (rode {rode[0]}, burnt {burnt[0]}); ends {string.Join(", ", ends[0])}");
        Console.WriteLine($"n45yard ride (B) rail W's inner face -> jump-off onto the rim: {ok[1]}/{tries} landed (rode {rode[1]}, burnt {burnt[1]}); ends {string.Join(", ", ends[1])}");
        return ok[0] > 0 && ok[1] > 0 ? 0 : 1;
    }

    static List<double[]> A1LavaRects(string path)
    {
        var res = new List<double[]>();
        var camp = PortData.Obj(PortData.Obj(PortData.LoadJson(path))["campaign"]);
        foreach (object o in PortData.Arr(camp["pits"]))
            if (o is Dictionary<string, object> d && d.TryGetValue("lava", out object lv) && lv is bool lb && lb && d.TryGetValue("zone", out object z) && z as string == "b1")
            {
                var r = PortData.Arr(d["rect"]);
                res.Add(new[] { PortData.Num(r[0]), PortData.Num(r[1]), PortData.Num(r[2]), PortData.Num(r[3]) });
            }
        return res;
    }

    static int A1Plan(string[] args)
    {
        string levelId = A1Arg(args, "--level", "n45-foundry");
        string path = Path.GetFullPath(Path.Combine("Design", "levels", levelId + ".json"));
        var lv = PortData.Obj(PortData.LoadJson(path));
        var camp = PortData.Obj(lv["campaign"]);
        bool In(object o) => o is Dictionary<string, object> d && ((d.TryGetValue("zone", out object z) && z as string == "b1") || (d.TryGetValue("room", out object r) && r as string == "b1"));
        var boxes = PortData.Arr(camp["boxes"]).Where(In).Select(PortData.Obj).ToList();
        var props = PortData.Arr(camp["props"]).Where(In).Select(PortData.Obj).ToList();
        var lights = PortData.Arr(camp["lights"]).Where(In).Select(PortData.Obj).ToList();
        var pits = PortData.Arr(camp["pits"]).Where(In).Select(PortData.Obj).ToList();
        int lava = 0;
        if (lv.TryGetValue("lava", out object la)) foreach (object o in PortData.Arr(la)) { var r = PortData.Arr(PortData.Obj(o)["rect"]); double cx = (PortData.Num(r[0]) + PortData.Num(r[2])) / 2, cz = (PortData.Num(r[1]) + PortData.Num(r[3])) / 2; if (cx >= -18 && cx <= 18 && cz >= -180 && cz <= -140) lava++; }
        var room = PortData.Arr(camp["rooms"]).Select(PortData.Obj).First(r => r["id"] as string == "b1");
        int rides = boxes.Count(bx => bx.TryGetValue("ride", out object rd) && rd is bool rb && rb);
        int accents = lights.Count(l => l["color"] as string == "amber" || l["color"] as string == "orange");
        bool hero = props.Any(p => p.TryGetValue("job", out object j) && j as string == "hero"), mark = props.Any(p => p.TryGetValue("job", out object j) && j as string == "landmark");
        int wn = room.TryGetValue("waveNodes43", out object w) ? PortData.Arr(w).Count : 0;
        bool whyAll = boxes.All(bx => bx.TryGetValue("why", out object y) && (y as string ?? "").Length > 20) && pits.All(p => p.TryGetValue("why", out object y) && (y as string ?? "").Length > 20);
        bool ok = lava >= 1 && rides == 2 && accents == 1 && hero && mark && wn == 3 && whyAll && !lv.ContainsKey("grinders42") && !lv.ContainsKey("cover42");
        Console.WriteLine($"n45yard plan {levelId} b1 \"{room["name"]}\": lava rects {lava} (pits {pits.Count}), boxes {boxes.Count} (ride walls {rides}), props {props.Count} (hero {hero}, landmark {mark}), "
            + $"lights {lights.Count} (accent {accents}), waveNodes43 waves {wn}, every solid's why {whyAll}, arena42 keys {(lv.ContainsKey("grinders42") || lv.ContainsKey("cover42") ? "PRESENT" : "none")}: {(ok ? "GREEN" : "RED")}");
        return ok ? 0 : 1;
    }
}
