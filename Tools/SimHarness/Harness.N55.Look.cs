// NIGHT55 builder C (camera and look): `n55look`, C's headless checks. Hooked without a Program.cs line (a module initializer runs the command
// and exits when argv[1] is "n55look", Harness.N53.RideKill.cs's pattern). Build with the Game files it reads:
//   dotnet run --project Tools/SimHarness -c Release -p:Extra="../../Assets/Scripts/Game/Move55.cs;../../Assets/Scripts/Game/Move54.cs;../../Assets/Scripts/Game/UnityTuning.N55C.cs;../../Assets/Tests/EditMode/Game/N55LookTests.cs" -- n55look <sub>
//   (Git Bash: wrap the whole -p:Extra="..." in single quotes so MSBuild gets the semicolons)
//   n55look step [--levels n52-foundry,n52-iceline,n52-kiln] [--lines 6]
//     P0-step's OFF number first (slate: "off value printed first and must exceed 2x the flat-ground noise"). Per level: Move55Lines' scan of
//     every non-boss room (the a255 reel's scan), then the Mover RUN across the best lines (a CAMPAIGN run, DOORFREE, God, husks cleared, the
//     stick forward along the line), the feet's height and grounded state recorded every 120 Hz step. What the Mover did at each scanned step
//     is printed (grounded snaps held vs falls). Then the render's own math at 60 / 144 / 240 fps (the accumulator, alpha, drawn = prev +
//     (cur - prev) x alpha, SurvivalGame.cs / SurvivalGame.Render.cs): the camera's height RAW (NIGHT53: move54 off), OFF (NIGHT54: move54's
//     Move54Step, the shipped numbers) and ON (step55's Move55Step); per side the largest one-frame camera drop within Step55StepWindowS of a
//     held step, and the flat-ground noise (the largest one-frame change on grounded frames Step55FlatAwayS from any step).
//   n55look selftest   N55LookTests (Game) on the harness's mini NUnit (the EditMode test file listed in Extra).
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
    internal static void N55LookInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || argv[1] != "n55look") return;
        string[] args = argv.Skip(1).ToArray();
        int code;
        try
        {
            NightRows.ForceOffFrom = NightRows.OffFrom(args, "--off");
            string sub = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "step";
            code = sub == "step" ? N55Step(args) : sub == "selftest" ? N55Selftest() : sub == "fov" ? N55FovCmd(args) : 2;
            if (code == 2) Console.WriteLine("n55look: unknown subcommand " + sub + " (step | fov | selftest)");
        }
        catch (Exception e) { Console.WriteLine("n55look: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static string N55Opt(string[] args, string name, string dflt)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : dflt;
    }

    static int N55Selftest()
    {
        Type t = typeof(Harness).Assembly.GetType("Skygrind.Game.Tests.N55LookTests");
        if (t == null) { Console.WriteLine("n55look selftest: N55LookTests not compiled in (add it to -p:Extra)"); return 1; }
        int failed = NUnit.Framework.MiniRunner.Run(t, Console.WriteLine);
        Console.WriteLine($"n55look selftest: {(failed == 0 ? "GREEN" : "RED")} ({failed} failing)");
        return failed == 0 ? 0 : 1;
    }

    static partial void N55FovPartial(string[] args, ref int code);
    static int N55FovCmd(string[] args) { int code = 2; N55FovPartial(args, ref code); return code; }

    /// <summary>One recorded run: the feet's height and grounded (ground or slide) after each 120 Hz step (index 0: before the first).</summary>
    sealed class N55Run
    {
        public readonly List<double> y = new List<double>();
        public readonly List<bool> g = new List<bool>();
        public int held, falls;
        public double largestHeld, largestFall, along;
    }

    static N55Run N55RunLine(SurvivalSim sim, Move55Lines.Line ln)
    {
        var run = new N55Run();
        var inp = (ScriptedInput)sim.input;
        double yaw = Math.Atan2(-ln.along.x, -ln.along.z);
        // start 0.5 m back from the line's start on its floor (the run is up to speed at the step: stepAt >= Step55ScanFrom)
        sim.Respawn(new Vec3(ln.start.x, ln.start.y + 0.05, ln.start.z), yaw * 180 / Math.PI);
        sim.mover.invulnerable = true;
        for (int i = 0; i < 20; i++) { inp.SetLook(yaw, 0); inp.Set(new InputFrame { yaw = yaw }, new CombatInput { weapon = -1 }); sim.hitstopLeft = 0; sim.health.hp = sim.health.max; sim.Step(); }
        bool G() => sim.mover.grounded && (sim.mover.state == MoveState.Ground || sim.mover.state == MoveState.Slide);
        run.y.Add(sim.mover.position.y);
        run.g.Add(G());
        double airFrom = double.NaN;
        for (int i = 0; i < UnityTuning.Step55RunSteps; i++)
        {
            Vec3 p = sim.mover.position;
            double along = (p.x - ln.start.x) * ln.along.x + (p.z - ln.start.z) * ln.along.z;
            run.along = along;
            if (along >= ln.length - 1) break;
            // stalled against something after the run got going (the Mover pinned on a corner): the line ends there
            if (i > 60 && Math.Sqrt(sim.mover.velocity.x * sim.mover.velocity.x + sim.mover.velocity.z * sim.mover.velocity.z) < 1) break;
            inp.SetLook(yaw, 0);
            inp.Set(new InputFrame { yaw = yaw, moveZ = 1 }, new CombatInput { weapon = -1 });
            sim.hitstopLeft = 0;
            sim.health.hp = sim.health.max;
            double y0 = sim.mover.position.y;
            bool g0 = run.g[run.g.Count - 1];
            sim.Step();
            double y1 = sim.mover.position.y;
            bool g1 = G();
            run.y.Add(y1);
            run.g.Add(g1);
            double dy = y1 - y0;
            if (g0 && g1 && Math.Abs(dy) >= UnityTuning.Step55StepMin) { run.held++; if (Math.Abs(dy) > Math.Abs(run.largestHeld)) run.largestHeld = dy; }
            if (g0 && !g1) airFrom = y0;
            if (!g0 && g1 && !double.IsNaN(airFrom)) { run.falls++; double d = y1 - airFrom; if (Math.Abs(d) > Math.Abs(run.largestFall)) run.largestFall = d; airFrom = double.NaN; }
        }
        return run;
    }

    /// <summary>The render's math over a recorded run at `fps`: per side (0 raw, 1 move54 off, 2 step55 on) the largest one-frame camera
    /// drop near a held step, the largest one-frame change on flat grounded frames, and the step's own largest one-frame feet drop.</summary>
    static void N55Render(GameConfig cfg, N55Run run, double fps, double[] stepDrop, double[] flatNoise, out double feetDrop, out int stepFrames, out int flatFrames)
    {
        const double step = 1.0 / 120;
        double frameDt = 1.0 / fps, acc = 0;
        var m54 = new Move54Step(cfg);
        var m55 = new Move55Step(step);
        // the held steps' times (s)
        var heldAt = new List<double>();
        for (int i = 1; i < run.y.Count; i++)
            if (run.g[i - 1] && run.g[i] && Math.Abs(run.y[i] - run.y[i - 1]) >= UnityTuning.Step55StepMin) heldAt.Add(i * step);
        for (int k = 0; k < 3; k++) { stepDrop[k] = 0; flatNoise[k] = 0; }
        feetDrop = 0;
        stepFrames = flatFrames = 0;
        int idx = 0; // steps run
        double[] last = new double[3];
        bool haveLast = false, lastG = false;
        double lastFeet = 0;
        for (int f = 0; idx < run.y.Count - 1; f++)
        {
            acc += frameDt;
            while (acc >= step && idx < run.y.Count - 1)
            {
                idx++;
                m54.StepEnded(run.g[idx], run.y[idx - 1], run.y[idx]);
                m55.StepEnded(run.g[idx], run.y[idx - 1], run.y[idx]);
                acc -= step;
            }
            double alpha = acc / step;
            double prev = run.y[Math.Max(0, idx - 1)], cur = run.y[idx];
            double drawn = prev + (cur - prev) * alpha;
            double[] cam = { drawn, drawn + m54.Frame(drawn, frameDt, true), drawn + m55.Draw(alpha) };
            bool g = run.g[idx];
            double t = idx * step;
            if (haveLast && g && lastG)
            {
                double near = heldAt.Count == 0 ? double.PositiveInfinity : heldAt.Min(h => Math.Abs(h - t));
                double fd = lastFeet - drawn;
                if (near <= UnityTuning.Step55StepWindowS)
                {
                    stepFrames++;
                    if (Math.Abs(fd) > Math.Abs(feetDrop)) feetDrop = fd;
                    for (int k = 0; k < 3; k++) stepDrop[k] = Math.Max(stepDrop[k], Math.Abs(last[k] - cam[k])); // a drop on a step down, a rise on a step up
                }
                else if (near >= UnityTuning.Step55FlatAwayS)
                {
                    flatFrames++;
                    for (int k = 0; k < 3; k++) flatNoise[k] = Math.Max(flatNoise[k], Math.Abs(last[k] - cam[k]));
                }
            }
            for (int k = 0; k < 3; k++) last[k] = cam[k];
            lastFeet = drawn;
            lastG = g;
            haveLast = true;
        }
    }

    static int N55Step(string[] args)
    {
        CultureInfo ci = CultureInfo.InvariantCulture;
        string F3(double v) => v.ToString("0.000", ci);
        string[] levels = N55Opt(args, "--levels", "n52-foundry,n52-iceline,n52-kiln").Split(',');
        int maxLines = int.Parse(N55Opt(args, "--lines", "6"), ci);
        GameConfig cfg = Config();
        Console.WriteLine($"n55look step: move54 on {Move54.On(cfg)} (the OFF side is NIGHT54's shipped camera), step55 numbers omega {UnityTuning.Step55Omega}, stepMin {UnityTuning.Step55StepMin}, maxOffset {UnityTuning.Step55MaxOffset}");
        int defects = 0, measured = 0;
        foreach (string lvId in levels)
        {
            bool survival = lvId.Trim() == "survival";
            string path = survival ? System.IO.Path.Combine(Root, "maps", "survival.json") : "Design/levels/" + lvId.Trim() + ".json";
            SurvivalMap map = CampaignMapFrom(path, cfg, out var json);
            LevelDef level = survival ? null : LevelDef.From(json);
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.n24Override = new N24Blocks { campaign = true };
            sim.n25Override = new N25Blocks { tutorial = true };
            if (!survival) sim.setup.mode = Campaign24.Mode;
            sim.StartRun(5501, null, null);
            for (int s = 0; s < 120; s++) { sim.health.hp = sim.health.max; sim.Step(); }
            sim.zones?.OpenAll();
            sim.campaign?.SetDoorFree29(true);
            sim.husks.KillAll();
            sim.husks.frozen = true;
            var lines = new List<Move55Lines.Line>();
            int rooms = 0;
            if (survival) // the endless map: one rect over its floor (floorSize), the floor at 0
            {
                double half = map.floorSize / 2;
                rooms = 1;
                Move55Lines.Room(sim.world, "survival", -half, -half, half, half, 0, lines);
            }
            else foreach (RoomDef rd in level.rooms)
            {
                if (rd.kind == "boss") continue;
                rooms++;
                Move55Lines.Room(sim.world, rd.id, rd.x0, rd.z0, rd.x1, rd.z1, rd.spawn.y, lines);
            }
            int downs = lines.Count(l => l.rise < 0), ups = lines.Count - downs;
            var rises = lines.GroupBy(l => Math.Round(l.rise, 2)).OrderBy(gp => gp.Key).Select(gp => $"{gp.Key.ToString("0.00", ci)} m x{gp.Count()}");
            Console.WriteLine($"== {lvId}: {rooms} rooms scanned, step lines {lines.Count} (down {downs}, up {ups}); rises: {string.Join(", ", rises)}");
            if (Array.IndexOf(args, "--falls") < 0) // the a255 reel's rule: a drop over the ground snap is a fall, never a step (--falls runs them too)
                Console.WriteLine($"   drops over the ground snap ({cfg.movement.collision.groundSnap} m) left out as falls: {Move55Lines.DropFalls(lines, cfg.movement.collision.groundSnap)}");
            lines.Sort(Move55Lines.Rank);
            // one line per distinct (room, rise) first, so the runs cover different steps
            var pick = Move55Lines.Distinct(lines); // the reel's order: one line per distinct step
            // the downs first (the pop GG sees most), then as many ups (a step up is held the same way)
            var runList = pick.Where(l => l.rise < 0).Take(maxLines).Concat(pick.Where(l => l.rise > 0).Take(maxLines)).ToList();
            foreach (var l in runList)
            {
                N55Run run = N55RunLine(sim, l);
                Vec3 at = l.start + l.along * l.stepAt;
                string what = $"line room {l.room} from ({l.start.x.ToString("0.0", ci)}, {l.start.y.ToString("0.00", ci)}, {l.start.z.ToString("0.0", ci)}) along ({l.along.x.ToString("0.00", ci)}, {l.along.z.ToString("0.00", ci)}), step {(l.rise < 0 ? "down" : "up")} {Math.Abs(l.rise).ToString("0.00", ci)} m at {l.stepAt.ToString("0.0", ci)} m (x {at.x.ToString("0.0", ci)}, z {at.z.ToString("0.0", ci)})";
                string did = $"ran {run.along.ToString("0.0", ci)} m in {run.y.Count - 1} steps; the Mover: grounded steps held (|dy| >= {UnityTuning.Step55StepMin}) {run.held} (largest {F3(run.largestHeld)} m), falls {run.falls} (largest {F3(run.largestFall)} m)";
                Console.WriteLine("  " + what + " | " + did);
                if (run.held == 0) continue;
                measured++;
                if (Array.IndexOf(args, "--trace") >= 0)
                {
                    var sb = new System.Text.StringBuilder("    trace (step: dy mm, G grounded / a air):");
                    for (int i = 1; i < run.y.Count; i++)
                    {
                        double dy = run.y[i] - run.y[i - 1];
                        if (Math.Abs(dy) < 1e-4 && run.g[i] == run.g[i - 1]) continue;
                        sb.Append($" {i}:{(dy * 1000).ToString("0", ci)}{(run.g[i] ? "G" : "a")}");
                    }
                    Console.WriteLine(sb.ToString());
                }
                foreach (double fps in new[] { 60.0, 144.0, 240.0 })
                {
                    double[] sd = new double[3], fn = new double[3];
                    N55Render(cfg, run, fps, sd, fn, out double feet, out int sf, out int ff);
                    double noise = Math.Max(fn[1], 1e-6);
                    bool defect = sd[1] > UnityTuning.Step55DefectOverNoise * fn[1] && sd[1] > 1e-4;
                    if (defect && fps == 60.0) defects++;
                    double ratio = sd[1] > 1e-9 ? sd[2] / sd[1] : double.NaN;
                    Console.WriteLine($"    {fps.ToString("0", ci)} fps: feet's largest one-frame change {F3(feet)} m | camera RAW (NIGHT53) {F3(sd[0])} | OFF (NIGHT54 move54) {F3(sd[1])} | ON (step55) {F3(sd[2])} (x{(double.IsNaN(ratio) ? "n/a" : ratio.ToString("0.00", ci))} of OFF, want <= x{UnityTuning.Step55OnOverOff.ToString("0.00", ci)}) | flat noise RAW {F3(fn[0])} OFF {F3(fn[1])} ON {F3(fn[2])} ({ff} flat frames, {sf} step frames) | OFF over 2x noise: {(defect ? "yes (a defect)" : "NO (no defect)")}");
                }
            }
        }
        Console.WriteLine($"n55look step: lines with a held step measured {measured}; with an OFF drop over {UnityTuning.Step55DefectOverNoise}x the flat noise at 60 fps {defects}");
        return 0;
    }
}
