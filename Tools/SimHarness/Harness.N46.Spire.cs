// NIGHT46 builder A2 (spire46; NIGHT46.md card A2): level 5's checks for n46-spire (v2 THE FALLEN BELL, v6 THE ROPE CHOIR, the belfry gates). Picked
// up by SimHarness.csproj's `*.cs` (no Program.cs line): a [ModuleInitializer] takes `n46spire ...` argv, as Harness.N45.Mix.cs does.
//   n46spire walk [--level n46-spire|n41-spire] [--seeds n] [--first n] [--cap s] [--trace]
//       walk29's pilot (Walk29Pilot: the rooms' scripts, the autopilot's fights, Span30Pilot in a room with grapple nodes) through the level file
//       with the NIGHT41 spire41 row ON, as `n41lv walk spire` (its setup copied here, A2-prefixed: Harness.N41.Levels.cs is not edited). Both
//       n41-spire and n46-spire read as n40-spire through the BaseId chain (NightRows -> N45Levels -> N41BaseId). Per room: cleared k of n, the
//       median / min / max, deaths, falls, the lessons; the summary line. The walk pilot never jump-dashes between v2's aisles (the 18 m closure is
//       optional by design: the lead's card-gate ruling 2), so a v2 clear here is a clear without it.
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
    internal static void A2N46SpireInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || !argv[1].StartsWith("n46spire")) return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try { code = A2SpireMain(args); }
        catch (Exception e) { Console.WriteLine("n46spire: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static string A2SpireArg(string[] args, string key, string def)
    {
        int i = Array.IndexOf(args, key);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : def;
    }

    static int A2SpireMain(string[] args)
    {
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "walk";
        if (what == "walk") return A2SpireWalk(args);
        Console.WriteLine("n46spire: walk [--level n46-spire|n41-spire] [--seeds n] [--first n] [--cap s] [--trace]");
        return 2;
    }

    /// <summary>The sim for a walk: the level file loaded headless, the campaign mode, NIGHT41's blocks with spire41 on (as N41LvSim), the walk pilot.</summary>
    static SurvivalSim A2SpireSim(GameConfig cfg, string path, uint seed, out AutopilotInput ap, out Walk29Pilot pilot)
    {
        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        sim.noRecords = sim.noLab = true;
        sim.setup.mode = Campaign24.Mode;
        int bit = Array.IndexOf(N41Blocks.Names, "spire41");
        int bits = (N41Blocks.ForceOff ? N41Blocks.Off : N41Blocks.From(cfg)).Bits;
        sim.n41Override = N41Blocks.FromBits(bits | (1 << bit));
        var a = new AutopilotInput(sim, "normal", seed);
        a.GoalProvider = () =>
        {
            Campaign24 c = sim.campaign;
            LevelDef lv = sim.campaignLevel;
            if (c == null || c.room < 0) return lv.rooms[0].spawn;
            if (c.phase == Campaign24.Phase.Cleared) return lv.rooms[Math.Min(c.room + 1, lv.rooms.Count - 1)].spawn;
            return lv.rooms[c.room].kind == "link" ? lv.rooms[c.room].exit : lv.rooms[c.room].spawn;
        };
        pilot = new Walk29Pilot(sim, a);
        sim.UseInput(pilot);
        ap = a;
        return sim;
    }

    static int A2SpireWalk(string[] args)
    {
        string id = A2SpireArg(args, "--level", "n46-spire");
        int seeds = int.Parse(A2SpireArg(args, "--seeds", "10"), CultureInfo.InvariantCulture);
        int first = int.Parse(A2SpireArg(args, "--first", "1"), CultureInfo.InvariantCulture);
        double cap = double.Parse(A2SpireArg(args, "--cap", "900"), CultureInfo.InvariantCulture);
        bool trace = Array.IndexOf(args, "--trace") >= 0;
        GameConfig cfg = Config();
        string path = Path.Combine("Design", "levels", id + ".json");
        LevelDef level = LevelDef.From(PortData.Obj(PortData.LoadJson(Path.GetFullPath(path))));
        int playable = level.rooms.FindAll(r => r.kind != "boss").Count;
        var rooms = new List<Walk29Room>();
        for (int i = 0; i < playable; i++) rooms.Add(new Walk29Room { id = level.rooms[i].id });
        var levelTimes = new List<double>();
        int done = 0;
        string baseId = "";
        for (int s = 0; s < seeds; s++)
        {
            uint seed = (uint)(first + s);
            SurvivalSim sim = A2SpireSim(cfg, path, seed, out AutopilotInput ap, out Walk29Pilot pilot);
            sim.events.On<RoomEvent>("roomClear", e => { if (e.room < playable) { rooms[e.room].cleared++; rooms[e.room].times.Add(e.time); } });
            sim.events.On<RoomEvent>("roomRetry", e => { if (e.room < playable) rooms[e.room].deaths++; });
            sim.events.On<RoomEvent>("stageRespawn", e => { if (e.room < playable) rooms[e.room].falls++; });
            sim.events.On<LessonEvent>("lessonClear", e => { if (e.room < playable) { rooms[e.room].lessons.TryGetValue(e.kind, out int n); rooms[e.room].lessons[e.kind] = n + 1; } });
            if (trace) sim.events.On<RoomEvent>("roomClear", e => Console.WriteLine($"  [seed {seed}] room {e.room + 1} {e.id} cleared in {e.time:0.0} s at run {sim.run.steps * sim.step:0.0} s"));
            sim.StartRun(seed, null, null);
            baseId = sim.N41BaseId(sim.campaignLevel != null ? sim.campaignLevel.id : "?");
            int max = MovementMath.StepsFor(cap, sim.step);
            int st = 0;
            for (; st < max && (sim.flow == Skygrind.Game.Flow.Playing || sim.flow == Skygrind.Game.Flow.Down); st++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                Campaign24 c = sim.campaign;
                if (c != null && c.clearSteps.Count >= playable) break;
            }
            bool ok = sim.campaign != null && sim.campaign.clearSteps.Count >= playable;
            if (ok) { done++; levelTimes.Add(st * sim.step); }
            else
            {
                Vec3 p = sim.mover.position;
                int rr = sim.campaign != null ? sim.campaign.room : -1;
                Console.WriteLine($"  seed {seed}: STOPPED after {st * sim.step:0} s in room {rr + 1} ({(rr >= 0 ? level.rooms[rr].id : "?")}) at ({p.x:0.0}, {p.y:0.0}, {p.z:0.0}), alive {sim.enemies.AliveCount}, hp {sim.health.hp:0}");
            }
            ap.Detach();
        }
        string tag = $"n46spire walk {id} (spire41 on; N41BaseId -> {baseId})";
        for (int i = 0; i < playable; i++)
        {
            Walk29Room r = rooms[i];
            r.times.Sort();
            double med = r.times.Count == 0 ? double.NaN : r.times[r.times.Count / 2];
            string ls = r.lessons.Count == 0 ? "none" : string.Join(", ", r.lessons.Select(kv => $"{kv.Key} x{kv.Value}"));
            Console.WriteLine($"{tag} {r.id}: cleared {r.cleared}/{seeds}, median {med:0.0} s (min {(r.times.Count > 0 ? r.times[0] : double.NaN):0.0}, max {(r.times.Count > 0 ? r.times[r.times.Count - 1] : double.NaN):0.0}), deaths {r.deaths}, falls {r.falls}, lessons: {ls}");
        }
        levelTimes.Sort();
        double lm = levelTimes.Count == 0 ? double.NaN : levelTimes[levelTimes.Count / 2];
        Console.WriteLine($"{tag} SUMMARY: all {playable} playable rooms cleared in {done} of {seeds} runs, median {lm:0.0} s (boss rooms excluded)");
        return done < seeds ? 1 : 0;
    }
}
