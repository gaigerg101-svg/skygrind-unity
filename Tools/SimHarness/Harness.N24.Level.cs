// NIGHT24 (builder L): the level pipeline's headless commands.
//   levelgen <plan.json> <out.json> [--walk] [--seed n]
//       a level plan (Design/levels/LEVEL-FORMAT.md) -> the map JSON with a "campaign" block (LevelPlan24 through DrydockPlan), the plan
//       checks, MapValidate's findings, and with --walk a bot walking room to room on the written collision (the spike: the doors are
//       opened by hand when the bot stands at a room's exit).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Skygrind.Core;
using Skygrind.Game;
using Skygrind.Game.MapAuthoring;
using Skygrind.EditorTools;

public static partial class Harness
{
    static partial void LevelgenCommand(string[] args, ref int code)
    {
        if (args.Length < 3) throw new ArgumentException("usage: levelgen <plan.json> <out.json> [--walk] [--seed n]");
        bool walk = Array.IndexOf(args, "--walk") >= 0;
        uint seed = 1;
        string reportArg = null; // NIGHT66 T1c: --report <path>, the level report written there instead of Reports/LEVEL-<id>-validate.md (a round's own folder)
        for (int i = 3; i < args.Length; i++)
        {
            if (args[i] == "--seed" && i + 1 < args.Length) seed = uint.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (args[i] == "--report" && i + 1 < args.Length) reportArg = args[++i];
        }
        GameConfig cfg = Config();
        var plan = PortData.Obj(PortData.LoadJson(Path.GetFullPath(args[1])));
        var t = System.Diagnostics.Stopwatch.StartNew();
        LevelPlan24.LevelBuild lb = LevelPlan24.Build(plan, cfg);
        string outPath = Path.GetFullPath(args[2]);
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllText(outPath, lb.text, new UTF8Encoding(false));
        MapModel m = lb.build.model;
        Console.WriteLine($"levelgen {m.id}: {lb.text.Length} chars -> {MapGenRelative(outPath)}; {m.boxes.Count} boxes + the floor, {m.barriers.Count} barriers, {m.wedges.Count} wedges, {m.pads.Count} pads, {m.grappleNodes.Count} grapple nodes, "
            + $"{m.zones.Count} zones ({string.Join(", ", m.zones.ConvertAll(z => z.id))}) ({t.Elapsed.TotalSeconds:0.0} s)");
        foreach (string n in lb.notes) Console.WriteLine($"  note: {n}");
        Console.WriteLine($"  plan checks: {lb.problems.Count} problems");
        foreach (string p in lb.problems) Console.WriteLine($"    {p}");
        code = lb.problems.Count == 0 ? 0 : 1;
        // The level report: Reports/LEVEL-<id>-validate.md (LevelValidate24: spots, the player's route, doors close the nav, every tier reachable by enemies)
        var tv = System.Diagnostics.Stopwatch.StartNew();
        (string md, int errors) = LevelValidate24.Run(cfg, PortData.Obj(JsJson.Parse(lb.text)), MapGenRelative(outPath));
        string reportPath = reportArg != null ? Path.GetFullPath(reportArg) : Path.Combine(MapGenRepo, "Reports", $"LEVEL-{m.id}-validate.md");
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
        File.WriteAllText(reportPath, md, new UTF8Encoding(false));
        Console.WriteLine($"  level report: {errors} errors -> {MapGenRelative(reportPath)} ({tv.Elapsed.TotalSeconds:0.0} s)");
        foreach (string line in md.Split((char)10)) if (line.Contains("| ERROR |")) Console.WriteLine("    " + line);
        foreach (string line in LevelValidate24.N27Lines) Console.WriteLine("  n27: " + line); // NIGHT27: the wall-vs-floor ratio per room, kills per level
        if (errors > 0) code = 1;
        if (!walk) return;

        SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(JsJson.Parse(lb.text)));
        SurvivalSim sim = SurvivalSim.Headless(cfg, map);
        AutopilotInput.BrainFactory = AutopilotInput.PortedBrain;
        AutopilotInput.GraphSource = BotNav.Survival;
        var ap = new AutopilotInput(sim, "normal", seed);
        sim.UseInput(ap);
        sim.noRecords = sim.noLab = true;
        var rooms = PortData.Arr(PortData.Obj(plan)["rooms"]);
        var doors = PortData.Arr(PortData.Obj(plan)["doors"]);
        int stage = 0;
        Vec3 ExitOf(int i)
        {
            var e = PortData.Arr(PortData.Obj(rooms[i])["exit"]);
            return new Vec3(PortData.Num(e[0]), PortData.Num(e[1]), PortData.Num(e[2]));
        }
        ap.GoalProvider = () => ExitOf(stage);
        sim.StartRun(seed, null, null);
        var arrive = new List<string>();
        int maxSteps = MovementMath.StepsFor(90, sim.step);
        bool done = false;
        for (int s = 0; s < maxSteps && sim.flow == Skygrind.Game.Flow.Playing && !done; s++)
        {
            AutopilotInput.TakeDraft(sim);
            sim.Step();
            Vec3 p = sim.mover.position, g = ExitOf(stage);
            if (JsMath.Hypot(p.x - g.x, p.z - g.z) < 2.5)
            {
                arrive.Add($"room {stage + 1} exit at {s * sim.step:0.0} s");
                if (stage + 1 >= rooms.Count) done = true;
                else
                {
                    var door = PortData.Obj(doors[stage]);
                    sim.zones.Open((string)door["id"], true);
                    sim.zones.Open((string)PortData.Obj(rooms[stage + 1])["id"], true);
                    stage++;
                }
            }
        }
        Console.WriteLine($"  bot walk: {string.Join("; ", arrive)}; {(done ? "REACHED the last exit" : "DID NOT reach the last exit")}; zone at the end {sim.zones.ZoneAt(sim.mover.position)}");
        if (!done) code = 1;
    }
}
