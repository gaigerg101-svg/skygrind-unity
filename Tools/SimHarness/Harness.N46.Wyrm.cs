// NIGHT46 builder B1 (NIGHT46.md card B1; docs/ROOMS.md "NIGHT46 identity cards", k5): THE KILN WYRM's band and walk-in proof, headless.
// Hooked without a Program.cs line: a module initializer runs the command and exits when argv[1] is "n46wyrm" (Harness.N45.Ring.cs's pattern;
// no older harness file touched; helpers carry the B1 prefix).
//   n46wyrm fight [--level n45-kiln] [--seeds 10] [--cap 300] [--rows wyrm46,walkin46] [--set key=v,key=v]
//     n45ring wyrm's setup (DOORFREE, the player put at k5's spawn, WyrmPilot31 fights, a death respawns at the boss spawn), per seed and in
//     sum: killed, the time, deaths, damage taken by the bot, staggers, the worm's damage dealt while STAGGERED (share), the crumble fallen,
//     the fight's pool. --rows forces the named design.n46 rows on (sim.rowsOverride); --set is a wyrm46 trial (SurvivalSim.Probe46Wyrm:
//     staggerSeconds, open, gunScale, aggroCooldown, poolScale). The bot band: median 30-60 s (DECIDE39 D-1, DECIDE46 N-1).
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
    internal static void B1N46WyrmInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n46wyrm") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try { code = B1Wyrm(args); }
        catch (Exception e) { Console.WriteLine("n46wyrm: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static string B1Arg(string[] args, string name, string dflt)
    {
        int i = Array.LastIndexOf(args, name); // the last one given wins
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : dflt;
    }

    static string B1F(double v) => double.IsNaN(v) || double.IsInfinity(v) ? "n/a" : v.ToString("0.0", CultureInfo.InvariantCulture);

    static string[] B1Rows = new string[0];

    static int B1Wyrm(string[] args)
    {
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "fight";
        string level = B1Arg(args, "--level", "n45-kiln");
        int seeds = int.Parse(B1Arg(args, "--seeds", "10"), CultureInfo.InvariantCulture);
        B1First = int.Parse(B1Arg(args, "--first", "1"), CultureInfo.InvariantCulture);
        double cap = double.Parse(B1Arg(args, "--cap", "300"), CultureInfo.InvariantCulture);
        string rows = B1Arg(args, "--rows", "");
        B1Rows = rows.Split(',', StringSplitOptions.RemoveEmptyEntries);
        B1Walk = Array.IndexOf(args, "--walk") >= 0;
        B1Trace = Array.IndexOf(args, "--trace") >= 0;
        string set = B1Arg(args, "--set", "");
        if (set != "")
        {
            SurvivalSim.Probe46Wyrm = new Dictionary<string, double>();
            foreach (string kv in set.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] p = kv.Split('=');
                SurvivalSim.Probe46Wyrm[p[0]] = double.Parse(p[1], CultureInfo.InvariantCulture);
            }
        }
        if (what == "fight") return B1Fight(level, seeds, cap, rows, set);
        if (what == "walk") return B1LevelWalk(level, seeds, args.Contains("--cap") ? cap : 900);
        if (what == "runup") return B1RunUp(level);
        Console.WriteLine("n46wyrm: unknown '" + what + "' (fight, walk)");
        return 1;
    }

    /// <summary>n46wyrm runup [--level n46-kiln]: each cliff link's east run-up ridden by a scripted pilot step (the player put 2.5 m past the
    /// door, 0.8 m off the east wall, facing the cliff; forward + toward the wall, jump pressed then held: the wall ride), until the feet reach
    /// 1.5 m short of the cliff face: steps in WallRide, the speed and the feet's height there, and whether he was riding then. On n45-kiln the
    /// east wall is a noride liner (the same script: the before).</summary>
    static int B1RunUp(string levelId)
    {
        GameConfig cfg = Config();
        string path = Path.Combine(MapGenRepo, "Design", "levels", levelId + ".json");
        var links = new (string id, double face)[] { ("kb1b", -57), ("kb2b", -135), ("kb3b", -260.5) };
        foreach (var (id, face) in links)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
            sim.noLab = sim.noRecords = true;
            sim.setup.mode = Campaign24.Mode;
            sim.StartRun(1, null, null);
            sim.husks.frozen = true;
            for (int i = 0; i < 10; i++) sim.Step();
            sim.campaign.SetDoorFree29(true);
            RoomDef r = sim.campaignLevel.rooms.Find(x => x.id == id);
            var si = (ScriptedInput)sim.input;
            sim.Respawn(new Vec3(6.6, 0, r.z1 - 2.5), 0);
            for (int i = 0; i < 30; i++) { si.SetLook(0, 0); si.Set(new InputFrame { yaw = 0, pitch = 0 }, default); sim.Step(); } // settle
            int ride = 0, steps = 0;
            double speed = 0, y = 0, rideM = 0;
            bool ridingAtFace = false;
            Vec3 last = sim.mover.position;
            while (steps++ < 600)
            {
                bool first = steps == 1;
                si.SetLook(0, 0);
                si.Set(new InputFrame { yaw = 0, pitch = 0, moveZ = 1, moveX = 0.4, jump = new Button { held = true, pressed = first } }, default);
                sim.Step();
                Vec3 p = sim.mover.position;
                if (sim.mover.state == MoveState.WallRide) { ride++; rideM += JsMath.Hypot(p.x - last.x, p.z - last.z); }
                last = p;
                if (p.z <= face + 1.5) { speed = sim.mover.Speed; y = p.y; ridingAtFace = sim.mover.state == MoveState.WallRide; break; }
            }
            Console.WriteLine($"n46wyrm runup {levelId} {id}: {steps / 120.0:0.00} s door -> 1.5 m short of the cliff; rode {ride / 120.0:0.00} s ({rideM:0.0} m on the wall); there: speed {speed:0.00} m/s, feet y {y:0.00}, riding {ridingAtFace}");
        }
        return 0;
    }

    /// <summary>n46wyrm walk [--level n46-kiln] [--seeds 10] [--cap 900] [--rows walkin46,...]: n43kiln walk's run (Walk29Pilot on the ported
    /// autopilot, the campaign from the level's start, the config's rows as they ship plus --rows) on any level-3 file: per room (links too) cleared
    /// k / n, the median time from its entry to its clear, deaths, falls; the boss room is not walked (as n43kiln walk).</summary>
    static int B1LevelWalk(string levelId, int seeds, double cap)
    {
        GameConfig cfg = Config();
        string path = Path.Combine(MapGenRepo, "Design", "levels", levelId + ".json");
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        var times = new List<double>[playable];
        var deaths = new int[playable];
        var falls = new int[playable];
        for (int q = 0; q < playable; q++) times[q] = new List<double>();
        int done = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(1 + s);
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path)));
            AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
            AutopilotInput.GraphSource = BotNav.Survival;
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            if (B1Rows.Length > 0)
            {
                NightRowState rs = sim.rowsRegistry.Defaults();
                foreach (string r in B1Rows) rs = rs.Set(r, true);
                sim.rowsOverride = rs;
            }
            var ap = new AutopilotInput(sim, "normal", seed);
            ap.GoalProvider = () =>
            {
                Campaign24 c = sim.campaign;
                LevelDef lv = sim.campaignLevel;
                if (c == null || c.room < 0) return lv.rooms[0].spawn;
                if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
                return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
            };
            var pilot = new Walk29Pilot(sim, ap);
            sim.UseInput(pilot);
            int st = 0;
            var enter = new Dictionary<int, int>();
            sim.events.On<RoomEvent>("roomEnter", e => { if (!enter.ContainsKey(e.room)) enter[e.room] = st; });
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room < playable) times[e.room].Add(enter.TryGetValue(e.room, out int es) ? (st - es) * sim.step : e.time); });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room < playable) deaths[e.room]++; });
            sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room < playable) falls[e.room]++; });
            sim.StartRun(seed, null, null);
            if (s == 0) Console.WriteLine($"n46wyrm walk: the campaign loaded {sim.campaignLevel?.id}; rows forced [{string.Join(",", B1Rows)}]");
            int max = MovementMath.StepsFor(cap, sim.step);
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (sim.campaign != null && sim.campaign.clearSteps.Count >= playable) break;
            }
            bool ok = sim.campaign != null && sim.campaign.clearSteps.Count >= playable;
            if (ok) done++;
            else
            {
                Vec3 p = sim.mover.position;
                Campaign24 c = sim.campaign;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0} s in {(c != null && c.room >= 0 ? level.rooms[c.room].id : "?")} {c?.phase} at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), alive {sim.enemies.AliveCount}, unsticks {pilot.unsticks}");
            }
            ap.Detach();
        }
        for (int q = 0; q < playable; q++)
        {
            var l = times[q];
            l.Sort();
            string med = l.Count == 0 ? "-" : l[l.Count / 2].ToString("0.0", CultureInfo.InvariantCulture);
            Console.WriteLine($"n46wyrm walk {levelId} {level.rooms[q].id} {level.rooms[q].name}: cleared {l.Count}/{seeds}, median {med} s, deaths {deaths[q]}, falls {falls[q]}");
        }
        Console.WriteLine($"n46wyrm walk SUMMARY ({levelId}): all {playable} playable rooms cleared in {done} of {seeds} runs");
        return done == seeds ? 0 : 1;
    }

    /// <summary>A headless run of the level with B1's rows forced, the player at k5's spawn, the worm's fight begun (null: no fight).</summary>
    static SurvivalSim B1Start(GameConfig cfg, string path, uint seed, out RoomDef k5)
    {
        SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
        sim.noLab = sim.noRecords = true;
        sim.setup.mode = Campaign24.Mode;
        if (B1Rows.Length > 0)
        {
            NightRowState st = sim.rowsRegistry.Defaults();
            foreach (string r in B1Rows) st = st.Set(r, true);
            sim.rowsOverride = st;
        }
        sim.StartRun(seed, null, null);
        sim.husks.frozen = true;
        for (int i = 0; i < 10; i++) sim.Step();
        sim.campaign.SetDoorFree29(true);
        k5 = sim.campaignLevel.rooms.Find(r => r.id == "k5");
        foreach (string g in new[] { "gauntlet", "shotgun" }) if (!sim.loadout.owned.Contains(g)) sim.loadout.AddWeapon(g);
        B1WalkLine = null;
        if (B1Walk)
        {
            // walk in: from REST (kb4's spawn) straight through d8 at a run, as a player who does not stop at the door
            RoomDef kb4 = sim.campaignLevel.rooms.Find(r => r.id == "kb4");
            sim.Respawn(kb4.spawn, 0);
            var si = (ScriptedInput)sim.input;
            Vec3 last = sim.mover.position;
            double speedBefore = 0, speedAfter = 0, maxJump = 0;
            int steps = 0;
            while ((sim.wyrm24 == null || !sim.wyrm24.Alive) && steps++ < 600)
            {
                speedBefore = sim.mover.Speed;
                si.SetLook(0, 0);
                si.Set(new InputFrame { yaw = 0, pitch = 0, moveZ = 1 }, default);
                sim.Step();
                maxJump = Math.Max(maxJump, (sim.mover.position - last).Length);
                last = sim.mover.position;
            }
            speedAfter = sim.mover.Speed;
            Vec3 c = sim.wyrm24 != null && sim.wyrm24.arenaCenter.HasValue ? sim.wyrm24.arenaCenter.Value : default;
            B1WalkLine = $"walked in: begun after {steps / 120.0:0.00} s from kb4's spawn; speed {speedBefore:0.00} m/s on the last step before the begin, {speedAfter:0.00} on the begin's step ({(speedBefore > 0 ? speedAfter / speedBefore * 100 : 0):0} %); largest move in one step {maxJump:0.00} m (a teleport > 1 m); the player at ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.00}, {sim.mover.position.z:0.0}); the worm's centre ({c.x:0.0}, {c.z:0.0}); BossWalkIn {sim.Boss36WalkIn}";
        }
        else sim.Respawn(k5.spawn, k5.spawnYawDeg);
        int wait = 0;
        while ((sim.wyrm24 == null || !sim.wyrm24.Alive) && wait++ < 600) sim.Step();
        return sim.wyrm24 != null && sim.wyrm24.Alive ? sim : null;
    }

    /// <summary>--walk: enter k5 on foot from kb4 (walkin46's proof) instead of being put at k5's spawn.</summary>
    static bool B1Walk;
    /// <summary>--first n: the first seed (default 1).</summary>
    static int B1First = 1;
    /// <summary>--trace: a line at each death (the worm's phase, pattern and bar, the staggers and damage so far).</summary>
    static bool B1Trace;
    static string B1WalkLine;

    static int B1Fight(string level, int seeds, double cap, string rows, string set)
    {
        GameConfig cfg = Config();
        string path = Path.Combine(MapGenRepo, "Design", "levels", level + ".json");
        SurvivalSim.Wyrm24Dir = MapGenRepo;
        int killed = 0, hurt = 0, crumbled = 0;
        var times = new List<double>();
        var takenL = new List<double>();
        var shares = new List<double>();
        var deathsL = new List<int>();
        var stagL = new List<int>();
        try
        {
            for (int s = 0; s < seeds; s++)
            {
                SurvivalSim sim = B1Start(cfg, path, (uint)(B1First + s), out RoomDef k5);
                if (sim == null) { Console.WriteLine($"n46wyrm fight seed {s + 1}: NO FIGHT (level {level})"); continue; }
                Wyrm w = sim.wyrm24;
                var si = (ScriptedInput)sim.input;
                var wp = new WyrmPilot31();
                int step = 0, capSteps = (int)(cap * 120), deaths = 0, staggers = 0;
                double taken = 0, dealt = 0, dealtStag = 0, prevHp = sim.health.hp, prevWorm = w.body.hp, pool = w.body.maxHp;
                bool wasStag = false, fell = false;
                int nearEntry = 0, sinceDeath = -1;
                bool wasCharging = false, stagPrev = false;
                var charges = new SortedDictionary<string, int>();
                var stagBy = new SortedDictionary<string, int>();
                var lifeBy = new SortedDictionary<string, int>();
                Vec3 entry = B1Walk ? new Vec3(0, 0, k5.z1) : k5.spawn; // walked in: d8's edge on k5's side; else the spawn
                Vec3 retry = k5.spawn;
                while (w.Alive && step < capSteps)
                {
                    bool stagBefore = sim.wyrm31 != null && sim.wyrm31.Staggered;
                    wp.Step(sim, step);
                    si.SetLook(wp.Move.yaw, wp.Move.pitch);
                    si.Set(wp.Move, wp.Combat);
                    sim.Step();
                    sim.hitstopLeft = 0;
                    step++;
                    if (step == 1) { pool = w.body.maxHp; prevWorm = w.body.hp; }
                    if (sim.health.hp < prevHp) taken += prevHp - sim.health.hp;
                    bool stag = sim.wyrm31 != null && sim.wyrm31.Staggered;
                    if (stag && !wasStag) staggers++;
                    wasStag = stag;
                    double d = prevWorm - w.body.hp;
                    if (sinceDeath == 0) d = 0; // the first step after a retry: wyrm46's retryKeep writes the bar back (not damage)
                    if (d > 0) { dealt += d; if (stag || stagBefore) dealtStag += d; }
                    string ph = w.phase ?? "-";
                    if (w.Charging30 && !wasCharging) { charges.TryGetValue(ph, out int cn); charges[ph] = cn + 1; }
                    wasCharging = w.Charging30;
                    if (stag && !stagPrev) { stagBy.TryGetValue(ph, out int sn); stagBy[ph] = sn + 1; }
                    stagPrev = stag;
                    lifeBy.TryGetValue(ph, out int ln); lifeBy[ph] = ln + 1;
                    prevWorm = w.body.hp;
                    if (sinceDeath >= 0) sinceDeath++;
                    if (B1Trace && (sinceDeath == 1 || sinceDeath == 2 || sinceDeath == 10 || sinceDeath == 120 || sinceDeath == 600)) Console.WriteLine($"    [seed {s + 1}]     +{sinceDeath} steps: bar {w.HpFrac:0.000}, phase {w.phase}");
                    if (sim.wyrm31CrumbleState == 2) fell = true;
                    { double ex = sim.mover.position.x - entry.x, ez = sim.mover.position.z - entry.z; if (w.Alive && ex * ex + ez * ez < 16) nearEntry++; }
                    if (sim.flow == Skygrind.Game.Flow.Down || sim.health.Dead)
                    {
                        deaths++;
                        if (B1Trace) Console.WriteLine($"    [seed {s + 1}] death {deaths} at {step / 120.0:0.0} s: the worm {w.phase} / {w.PatternName ?? "-"}, bar {w.HpFrac:0.000}, staggers so far {staggers}, dealt {dealt:0} ({dealtStag:0} staggered), the player at ({sim.mover.position.x:0.0}, {sim.mover.position.y:0.0}, {sim.mover.position.z:0.0})");
                        sim.downSteps = 0;
                        sim.flow = Skygrind.Game.Flow.Playing;
                        sim.health.Reset();
                        if (sim.BossRoomBegin("wyrm", out Vec3 sp, out _)) retry = sp;
                        sim.Respawn(retry, 0);
                        prevWorm = w.body.hp;
                        if (B1Trace) Console.WriteLine($"    [seed {s + 1}]   after the retry: bar {w.HpFrac:0.000} (wyrm46 keeps the next step), phase {w.phase}");
                        sinceDeath = 0;
                    }
                    prevHp = sim.health.hp;
                }
                bool k = !w.Alive;
                if (k) killed++;
                times.Add(step / 120.0); // a fight still alive at the cap counts as the cap (the median is over every seed)
                if (taken > 0) hurt++;
                if (fell) crumbled++;
                double share = dealt > 0 ? dealtStag / dealt : double.NaN;
                takenL.Add(taken);
                shares.Add(share);
                deathsL.Add(deaths);
                stagL.Add(staggers);
                Console.WriteLine($"n46wyrm fight seed {s + 1}: {(k ? "killed" : "ALIVE")} in {step / 120.0:0.0} s, deaths {deaths}, taken {taken:0}, staggers {staggers}, staggered share {B1F(share * 100)} %, crumble {(fell ? "fell" : "NO")}, pool {pool:0}, wyrm46 pools {sim.wyrm46Pools}, retries kept {sim.wyrm46RetriesKept}, per phase (s / head charges / staggers) {string.Join(" ", lifeBy.Select(e => $"{e.Key} {e.Value / 120.0:0}/{(charges.TryGetValue(e.Key, out int c1) ? c1 : 0)}/{(stagBy.TryGetValue(e.Key, out int c2) ? c2 : 0)}"))}, within 4 m of the entry {nearEntry / 120.0:0.0} s" + (B1WalkLine != null ? "; " + B1WalkLine : ""));
            }
        }
        finally { SurvivalSim.Wyrm24Dir = null; SurvivalSim.Probe46Wyrm = null; }
        times.Sort();
        double median = times.Count > 0 ? times[times.Count / 2] : double.NaN;
        double Med(List<double> l) { var c = l.Where(x => !double.IsNaN(x)).OrderBy(x => x).ToList(); return c.Count > 0 ? c[c.Count / 2] : double.NaN; }
        bool band = killed == seeds && median >= 30 && median <= 60;
        Console.WriteLine($"n46wyrm fight SUMMARY (level {level}, rows [{rows}], set [{set}]): killed {killed}/{seeds}, median {B1F(median)} s over every seed, the cap for one alive (min {B1F(times.Count > 0 ? times[0] : double.NaN)}, max {B1F(times.Count > 0 ? times[^1] : double.NaN)}); deaths median {(deathsL.Count > 0 ? deathsL.OrderBy(x => x).ElementAt(deathsL.Count / 2) : 0)}; damage taken > 0 on {hurt}/{seeds} (median {B1F(Med(takenL))}); staggers median {(stagL.Count > 0 ? stagL.OrderBy(x => x).ElementAt(stagL.Count / 2) : 0)}; staggered share median {B1F(Med(shares) * 100)} %; crumble fell {crumbled}/{seeds}; over 60 s {times.Count(t => t > 60)}/{seeds}; band 30-60 s: {(band ? "GREEN" : "RED")}");
        return 0;
    }
}
