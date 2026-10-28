// NIGHT46 builder A3 (foundry46 / iceline46: levels 1 and 2 smoothed by data; NIGHT46.md card A3; docs/ROOMS.md "NIGHT46 A3"). Hooked without a
// Program.cs line: a module initializer runs the command and exits when argv[1] is "n46smooth" (no lead file touched).
//   n46smooth walk --level <id> --rooms a3,b2,... [--seeds n] [--first n] [--cap s] [--rows r1,r2]
//       per room: a fresh run on the level's own map (Design/levels/<id>.json), the room entered as its door would (Campaign24.Enter by reflection,
//       A2's `n45mix walk` way; past g6 the KESTREL), the walk pilot (Walk38Pilot + the ported autopilot brain) until the room clears or the cap.
//       Prints per room: cleared k of n, the median / max clear time, deaths (roomRetry), the first wave's time after the entry (an enemy alive or
//       inside its spawn tell), fallReturn43s, lava burns, enemies the lava killed. Run it on n45-* for the before (the same pilot and seeds).
//       --rows: NIGHT46 sim rows forced on (sim.rowsOverride; e.g. teach46). Iceline levels get n40's gauntlet curriculum as A2's walk did.
//       The pilot is not a person: Walk38Pilot has no lava rule outside the n36 ids (T-75); a number here is the pilot's, said so in the report.
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
    internal static void A3N46SmoothInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n46smooth") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try { code = A3n46Smooth(args); }
        catch (Exception e) { Console.WriteLine("n46smooth: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static string A3n46Arg(string[] args, string key, string def)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
    }

    static string A3n46F(double v) => v.ToString("0.0", CultureInfo.InvariantCulture);

    static int A3n46Smooth(string[] args)
    {
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "";
        if (what == "walk") return A3n46Walk(args);
        if (what == "b2") return A3n46B2(args);
        Console.WriteLine("n46smooth: walk --level <id> --rooms r1,r2 [--seeds n] [--first n] [--cap s] [--rows a,b]");
        return 2;
    }

    /// <summary>A3's still input: no move, no press, a fixed look (the b2 probe's player stands on the landing).</summary>
    sealed class A3n46Still : IPlayerInput
    {
        public double Yaw => 0;
        public double Pitch => 0;
        public void SetLook(double y, double p) { }
        public bool Captured => true;
        public InputFrame SampleMove(bool active) => new InputFrame { yaw = 0, pitch = 0 };
        public CombatInput SampleCombat(bool active) => new CombatInput { weapon = -1 };
    }

    /// <summary>n46smooth b2 [--level n46-foundry] [--seeds n] [--hold s]: F-4's fight without the walk pilot (it never crosses gate 2): enter b2 as
    /// its door would, put the player where gate 1's long wall lands (-2.2, 0, -231), stand still --hold s (default 5: a still player dies to the three by ~11 s): when wave 0 comes (its
    /// lesson met on the landing), where each husk appears, the nearest husk distance over time, husks the lava killed on their own, the player's
    /// health lost. Then every husk removed and the player put on gate 2's far landing (lesson 1's rect): the link must clear within 3 s.</summary>
    static int A3n46B2(string[] args)
    {
        string level = A3n46Arg(args, "--level", "n46-foundry");
        int seeds = int.Parse(A3n46Arg(args, "--seeds", "10"), CultureInfo.InvariantCulture);
        double hold = double.Parse(A3n46Arg(args, "--hold", "5"), CultureInfo.InvariantCulture);
        string path = Path.GetFullPath(Path.Combine("Design", "levels", level + ".json"));
        LevelDef lv = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        int ri = lv.rooms.FindIndex(r => r.id == "b2");
        if (ri < 0) { Console.WriteLine("n46smooth b2: no b2 in " + level); return 2; }
        var land = new Vec3(-2.2, 0, -231);
        int good = 0;
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(1 + s);
            SurvivalSim sim = SurvivalSim.Headless(Config(), SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
            sim.noRecords = sim.noLab = true;
            sim.setup.mode = Campaign24.Mode;
            sim.UseInput(new A3n46Still());
            int lavaKills = 0, cleared = -1, step = 0;
            sim.events.On<Lava30Event>("lavaEnemyDeath", e => lavaKills++);
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room == ri && cleared < 0) cleared = step; });
            sim.StartRun(seed, null, null);
            sim.Step();
            sim.husks.KillAll();
            sim.brain.Clear();
            typeof(Campaign24).GetMethod("Enter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(sim.campaign, new object[] { ri });
            sim.Respawn(lv.rooms[ri].spawn, lv.rooms[ri].spawnYawDeg);
            for (int i = 0; i < 20; i++) { sim.Step(); step++; } // the commit gate's 3 m is not walked: the 3 s cap or the landing below meets it
            sim.Respawn(land, 0);
            double hp0 = sim.health.hp, spawnT = double.NaN, nearMin = double.MaxValue;
            var at = new List<string>();
            int steps = MovementMath.StepsFor(hold, sim.step), seen = 0;
            var near = new List<double>();
            for (int i = 0; i < steps; i++)
            {
                sim.Step(); step++;
                int alive = 0;
                double nd = double.MaxValue;
                foreach (Enemy en in sim.enemies.all)
                {
                    if (!en.alive) continue;
                    alive++;
                    double d = JsMath.Hypot(en.position.x - sim.mover.position.x, en.position.z - sim.mover.position.z);
                    if (d < nd) nd = d;
                    if (alive > seen) { at.Add($"({A3n46F(en.position.x)}, {A3n46F(en.position.z)})"); }
                }
                if (alive > seen) seen = alive;
                if (alive > 0 && double.IsNaN(spawnT)) spawnT = i * sim.step;
                if (alive > 0) { nearMin = Math.Min(nearMin, nd); if (i % 120 == 0) near.Add(nd); }
                if (Array.IndexOf(args, "--trace") >= 0 && i % 120 == 0)
                {
                    var hs = new List<string>();
                    foreach (Enemy en in sim.enemies.all) hs.Add($"{en.kind}#{en.id}{(en.alive ? "" : "x")}({A3n46F(en.position.x)},{A3n46F(en.position.y)},{A3n46F(en.position.z)})");
                    Console.WriteLine($"    t {A3n46F(i * sim.step)}: wave {sim.campaign.wave} phase {sim.campaign.phase} hp {sim.health.hp:0} feet ({A3n46F(sim.mover.position.x)},{A3n46F(sim.mover.position.y)},{A3n46F(sim.mover.position.z)}) {string.Join(" ", hs)}");
                }
            }
            int aliveEnd = sim.enemies.AliveCount;
            double hpLost = hp0 - sim.health.hp;
            int retries = 0;
            sim.events.On<RoomEvent>("roomRetry", e => retries++);
            sim.husks.KillAll(); // as Campaign24's world.KillAllEnemies (husks + the brain)
            sim.brain.Clear();
            sim.Respawn(new Vec3(0, 0, -284), 0);
            int maxClear = step + MovementMath.StepsFor(3, sim.step);
            while (step < maxClear && cleared < 0) { sim.Step(); step++; }
            if (cleared < 0)
            {
                Campaign24 c = sim.campaign;
                Console.WriteLine($"    trace: room {(c.room >= 0 ? lv.rooms[c.room].id : "?")} phase {c.phase} wave {c.wave} alive {sim.enemies.AliveCount} lesson {(c.lesson != null ? c.lesson.index.ToString() : "-")} feet ({A3n46F(sim.mover.position.x)}, {A3n46F(sim.mover.position.y)}, {A3n46F(sim.mover.position.z)}) grounded {sim.mover.grounded} hp {sim.health.hp:0} flow {sim.flow}");
            }
            bool ok = seen >= 3 && lavaKills == 0 && cleared >= 0;
            if (ok) good++;
            Console.WriteLine($"  b2 seed {seed}: wave 0 at {A3n46F(spawnT)} s on the landing, husks seen {seen} at {string.Join(" ", at)}; nearest min {A3n46F(nearMin)} m, "
                + $"every 1 s {string.Join(" ", near.Select(A3n46F))}; alive at {A3n46F(hold)} s {aliveEnd}, lava kills on their own {lavaKills}, player hp lost {hpLost:0}; "
                + $"after the kills + the far landing: {(cleared >= 0 ? "CLEARED" : "NOT CLEARED")}");
        }
        Console.WriteLine($"n46smooth b2 {level} SUMMARY: {(good == seeds ? "GREEN" : "RED")} {good}/{seeds} (3 husks on the landing, none into the lava by themselves, the link clears after them)");
        return good == seeds ? 0 : 1;
    }

    static int A3n46Walk(string[] args)
    {
        string level = A3n46Arg(args, "--level", "n46-foundry");
        string[] rooms = A3n46Arg(args, "--rooms", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        int seeds = int.Parse(A3n46Arg(args, "--seeds", "10"), CultureInfo.InvariantCulture);
        int first = int.Parse(A3n46Arg(args, "--first", "1"), CultureInfo.InvariantCulture);
        double cap = double.Parse(A3n46Arg(args, "--cap", "240"), CultureInfo.InvariantCulture);
        string[] forced = A3n46Arg(args, "--rows", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        string path = Path.GetFullPath(Path.Combine("Design", "levels", level + ".json"));
        LevelDef lv = LevelDef.From(PortData.Obj(PortData.LoadJson(path)));
        GameConfig cfg0 = Config();
        string baseId = N45Levels.BaseId(cfg0, level);
        bool iceline = level.Contains("iceline");
        Console.WriteLine($"n46smooth walk {level} (BaseId -> {baseId}){(forced.Length > 0 ? " rows on: " + string.Join(",", forced) : "")}; {seeds} seeds from {first}, cap {A3n46F(cap)} s");
        int allOk = 0, allRuns = 0;
        foreach (string roomId in rooms)
        {
            int ri = lv.rooms.FindIndex(r => r.id == roomId);
            if (ri < 0) { Console.WriteLine($"  {roomId}: not in {level}"); return 2; }
            var clears = new List<double>();
            var firstWave = new List<double>();
            int deaths = 0, falls = 0, burns = 0, lavaKills = 0, kills = 0;
            for (int s = 0; s < seeds; s++)
            {
                uint seed = (uint)(first + s);
                GameConfig cfg = Config();
                SurvivalSim sim = SurvivalSim.Headless(cfg, SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(path))));
                sim.noRecords = sim.noLab = true;
                sim.setup.mode = Campaign24.Mode;
                if (iceline) sim.n40Override = new N40Blocks { iceline40 = true, foes40 = true, brawl40 = false }; // as A2's n45mix walk (L2's n40ice walk)
                if (forced.Length > 0)
                {
                    NightRowState st0 = sim.rowsRegistry.Defaults();
                    foreach (string r in forced) st0 = st0.Set(r, true);
                    sim.rowsOverride = st0;
                }
                AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
                AutopilotInput.GraphSource = BotNav.Survival;
                var ap = new AutopilotInput(sim, "normal", seed);
                ap.GoalProvider = () =>
                {
                    Campaign24 c = sim.campaign;
                    LevelDef l = sim.campaignLevel;
                    if (c == null || c.room < 0) return l.rooms[ri].spawn;
                    if (c.phase == Campaign24.Phase.Cleared) return l.rooms[Math.Min(c.room + 1, l.rooms.Count - 1)].exit;
                    return l.rooms[c.room].kind == "link" ? l.rooms[c.room].exit : l.rooms[c.room].spawn;
                };
                var pilot = new Walk38Pilot(sim, ap);
                sim.UseInput(pilot);
                double clearT = double.NaN;
                int step = 0;
                sim.events.On<RoomEvent>("roomClear", e => { if (e.room == ri && double.IsNaN(clearT)) clearT = step * sim.step; });
                sim.events.On<RoomEvent>("roomRetry", e => { if (e.room == ri) deaths++; });
                sim.events.On<FallReturn43Event>("fallReturn43", e => { if (e.room == ri) falls++; });
                sim.events.On<Lava30Event>("lavaBurn", e => burns++);
                sim.events.On<Lava30Event>("lavaEnemyDeath", e => lavaKills++);
                sim.events.On<KillEvent>("kill", e => kills++);
                sim.StartRun(seed, null, null);
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                sim.husks.KillAll();
                sim.brain.Clear();
                typeof(Campaign24).GetMethod("Enter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(sim.campaign, new object[] { ri });
                sim.Respawn(lv.rooms[ri].spawn, lv.rooms[ri].spawnYawDeg);
                int g6 = lv.rooms.FindIndex(r => r.id == "g6");
                if (iceline && g6 >= 0 && ri > g6 && !sim.loadout.owned.Contains("revolver")) sim.loadout.AddWeapon("revolver");
                int max = MovementMath.StepsFor(cap, sim.step);
                double fw = double.NaN;
                int seenWave = sim.campaign.wave;
                for (; step < max && double.IsNaN(clearT) && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); step++)
                {
                    AutopilotInput.TakeDraft(sim);
                    sim.Step();
                    if (double.IsNaN(fw) && sim.campaign.wave > seenWave) fw = (step + 1) * sim.step;
                }
                if (!double.IsNaN(clearT)) clears.Add(clearT);
                if (!double.IsNaN(fw)) firstWave.Add(fw);
                if (double.IsNaN(clearT))
                {
                    Vec3 p = sim.mover.position;
                    Console.WriteLine($"    {roomId} seed {seed}: STOPPED after {A3n46F(step * sim.step)} s, {sim.campaign.phase} wave {sim.campaign.wave} at ({A3n46F(p.x)}, {A3n46F(p.y)}, {A3n46F(p.z)}), state {sim.mover.state}, alive {sim.enemies.AliveCount}, hp {sim.health.hp:0}");
                }
                ap.Detach();
            }
            clears.Sort();
            firstWave.Sort();
            string Med(List<double> l) => l.Count == 0 ? "-" : A3n46F(l[l.Count / 2]);
            string Max(List<double> l) => l.Count == 0 ? "-" : A3n46F(l[l.Count - 1]);
            Console.WriteLine($"n46smooth walk {level} {roomId} ({lv.rooms[ri].name}): cleared {clears.Count}/{seeds}, median {Med(clears)} s, max {Max(clears)} s, deaths {deaths}, "
                + $"first wave median {Med(firstWave)} s (max {Max(firstWave)}), fallReturn43 {falls}, lava burns {burns}, lava kills {lavaKills}, kills {kills}");
            allOk += clears.Count; allRuns += seeds;
        }
        Console.WriteLine($"n46smooth walk {level} SUMMARY: {(allOk == allRuns ? "GREEN" : "RED")} {allOk}/{allRuns} room runs cleared");
        return allOk == allRuns ? 0 : 1;
    }
}
