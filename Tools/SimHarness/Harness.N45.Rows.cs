// NIGHT45 builder S1 (rows as data; NIGHT45.md card S1): the headless proof of the generic rows registry (Core/NightRows.cs,
// Game/SurvivalSim.Rows.cs, Game/Tape/TapeRows.cs), all UNWIRED tonight.
//   n45rows [check] [--offNN] [--expect-wired]
//                                the real config's registry (expected tonight: no sim rows, no problems), the off switch as parsed from the
//                                args, a headless sim's wiring (tonight UNWIRED: rows null, RowsInit not called; after NIGHT46's switch-on
//                                WIRED with the config's registry: --expect-wired makes UNWIRED a failure), and the tape helper's round trip
//                                inside a TapeWriter / TapeReader stream (a value before, the record, a value after).
//   n45rows selftest            Assets/Tests/EditMode/NightRowsTests.cs on the harness's mini NUnit; build with
//                                -p:Extra=../../Assets/Tests/EditMode/NightRowsTests.cs (else it says so and exits 1).
// Hooked without a Program.cs line: a module initializer runs the command and exits when argv[1] starts with "n45rows" (no lead file touched).
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    [ModuleInitializer]
    internal static void S1N45RowsInit()
    {
        string[] argv = Environment.GetCommandLineArgs();
        if (argv.Length < 2 || !argv[1].StartsWith("n45rows")) return;
        string[] args = argv.Skip(1).ToArray();
        int code = 0;
        try { code = S1N45Rows(args); }
        catch (Exception e) { Console.WriteLine("n45rows: EXCEPTION " + e); code = 2; }
        Environment.Exit(code);
    }

    static int S1N45Rows(string[] args)
    {
        string what = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : "check";
        if (what == "selftest")
        {
            int failed = 0, classes = 0;
            foreach (string name in new[] { "Skygrind.Core.Tests.NightRowsTests", "Skygrind.Game.Tests.NightRowsTapeTests" })
            {
                Type t = typeof(Harness).Assembly.GetType(name);
                if (t == null) continue;
                classes++;
                failed += NUnit.Framework.MiniRunner.Run(t, Console.WriteLine);
            }
            if (classes == 0)
            {
                Console.WriteLine("n45rows selftest: no test class compiled in; run with -p:Extra=../../Assets/Tests/EditMode/NightRowsTests.cs (or .../Game/NightRowsTapeTests.cs)");
                return 1;
            }
            Console.WriteLine($"n45rows selftest: {(failed == 0 ? "GREEN" : "RED")} ({classes} class(es), {failed} failing)");
            return failed == 0 ? 0 : 1;
        }
        if (what != "check")
        {
            Console.WriteLine("n45rows: unknown '" + what + "' (check, selftest)");
            return 1;
        }

        int bad = 0;
        GameConfig cfg = Config();
        NightRows reg = NightRows.From(cfg);
        var problems = NightRows.Problems(cfg.Obj("design"));
        Console.WriteLine($"n45rows registry: {reg.Count} sim row(s) from NIGHT{NightRows.FirstNight} on: {reg}");
        Console.WriteLine($"n45rows checker: {problems.Count} problem(s){(problems.Count > 0 ? ": " + string.Join("; ", problems) : "")}");
        if (problems.Count > 0) bad++;

        int off = NightRows.OffFrom(args, "--off");
        Console.WriteLine($"n45rows off switch: --offNN from the args -> {(off == NightRows.NoOff ? "none" : "every row of night " + off + " and later off")} (not applied tonight: NightRows.ForceOffFrom stays {(NightRows.ForceOffFrom == NightRows.NoOff ? "none" : NightRows.ForceOffFrom.ToString())})");

        // wired = RowsInit ran (rows set); tonight's expectation is unwired (rows null, the registry left Empty); after the switch-on the
        // sim's registry must equal the config's and its rows the defaults (no override), else the wiring is wrong
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        bool wired = sim.rows != null;
        bool consistent = wired ? sim.rowsRegistry.Count == reg.Count && sim.rows.ToString() == reg.AllOff().ToString() /* NIGHT46 freeze: RowsInit sets AllOff, RowsStartRun applies the defaults (now ON) */ : sim.rowsRegistry.Count == 0 && sim.rowsOverride == null;
        Console.WriteLine($"n45rows wiring: {(wired ? "WIRED" : "UNWIRED")} {(consistent ? "GREEN" : "RED")} (a headless sim after Init: rowsRegistry {sim.rowsRegistry.Count} rows, rows {(sim.rows == null ? "null" : sim.rows.ToString())}; tape format {RunTape.FormatVersion}){(args.Contains("--expect-wired") && !wired ? " RED: --expect-wired" : "")}");
        if (!consistent || (args.Contains("--expect-wired") && !wired)) bad++;
        if (wired)
        {
            // the probe: a scratch config with one NIGHT46 sim row ON (never written to disk): a run reads it, --off46 / --off19 turn it off,
            // --off47 keeps it, and a tape's override wins
            GameConfig pc = Config();
            pc.Obj("design")["n46"] = new System.Collections.Generic.Dictionary<string, object> { ["probe46"] = new System.Collections.Generic.Dictionary<string, object> { ["sim"] = true, ["enabled"] = true, ["_why"] = "S1 probe" } };
            int was = NightRows.ForceOffFrom;
            string got = "";
            bool probeOk = true;
            foreach (int from in new[] { NightRows.NoOff, 47, 46, 19 })
            {
                NightRows.ForceOffFrom = from;
                SurvivalSim ps = SurvivalSim.Headless(pc, Map());
                ps.noLab = true;
                ps.StartRun(1, null, null);
                bool on = ps.rows.On("probe46");
                got += $" off{(from == NightRows.NoOff ? "-none" : from.ToString())}={(on ? "on" : "off")}";
                probeOk &= on == (from > 46);
            }
            NightRows.ForceOffFrom = NightRows.NoOff;
            SurvivalSim po = SurvivalSim.Headless(pc, Map());
            po.rowsOverride = po.rowsRegistry.AllOff();
            po.noLab = true;
            po.StartRun(1, null, null);
            probeOk &= !po.rows.On("probe46");
            got += $" override-off={(po.rows.On("probe46") ? "on" : "off")}";
            NightRows.ForceOffFrom = was;
            Console.WriteLine($"n45rows probe: {(probeOk ? "GREEN" : "RED")} (a scratch design with n46.probe46 sim ON:{got})");
            if (!probeOk) bad++;

            // a real tape: recorded with probe46 ON, through its bytes, replayed EQUAL on the probe's config; refused on a build without it
            System.Reflection.FieldInfo rowsField = typeof(RunTape).GetField("rows");
            SurvivalSim ts = SurvivalSim.Headless(pc, Map());
            ts.noLab = true;
            RunTape tape = TapeScript.Record(ts, SimFingerprint.Seed, 1200, "n45rows", null, TapeScript.God);
            RunTape tapeBack = RunTape.FromBytes(tape.ToBytes());
            NightRowState taped = TapeRows.Replay(NightRows.From(pc), rowsField?.GetValue(tapeBack) as byte[]);
            TapeResult same = TapeReplay.Replay(pc, Map(), tapeBack);
            string refusedLine = null;
            try { TapeResult other = TapeReplay.Replay(Config(), Map(), tapeBack); refusedLine = other.Equal ? null : "not equal: " + other.Line; }
            catch (TapeFormatException e) { refusedLine = e.Message; }
            bool tapeOk = rowsField != null && taped.On("probe46") && same.Equal && refusedLine != null;
            Console.WriteLine($"n45rows tape: {(tapeOk ? "GREEN" : "RED")} (format {RunTape.FormatVersion}, {tape.steps} steps with probe46 ON; the record replays: {taped}; replay on the probe's config: {same.Line}; on a build without probe46: {refusedLine ?? "NOT refused"})");
            if (!tapeOk) bad++;
        }

        // the tape helper inside a tape stream: a header value before, the record, a value after; then a record of a design with rows
        NightRows demo = NightRows.FromDesign(PortData.Obj(JsJson.Parse(@"{ ""n46"": { ""wall46"": { ""sim"": true, ""enabled"": true, ""_why"": ""demo"" }, ""pour46"": { ""sim"": true, ""enabled"": false, ""_why"": ""demo"" } }, ""n47"": { ""grip47"": { ""sim"": true, ""enabled"": true, ""_why"": ""demo"" } } }")));
        NightRowState ran = demo.AllOff().Set("wall46", true).Set("grip47", true);
        byte[] rec = NightRowsTape.Encode(ran);
        var w = new TapeWriter(64);
        w.VarI(0b101101001011); // stands in for n43
        TapeRows.Write(w, rec);
        w.D(100); // stands in for startMax
        byte[] bytes = new byte[w.length];
        Buffer.BlockCopy(w.data, 0, bytes, 0, w.length);
        var r = new TapeReader(bytes, 0, bytes.Length);
        int n43 = r.VarI();
        byte[] back = TapeRows.Read(bytes, r);
        double tail = r.D();
        NightRowState replay = TapeRows.Replay(demo, back);
        bool streamOk = n43 == 0b101101001011 && tail == 100 && !r.More && replay.ToString() == ran.ToString();
        Console.WriteLine($"n45rows tape stream: {(streamOk ? "GREEN" : "RED")} (record {rec.Length} bytes for 3 rows in 2 nights; replayed: {replay}; the values around it intact: {n43 == 0b101101001011 && tail == 100})");
        if (!streamOk) bad++;

        var w0 = new TapeWriter(8);
        TapeRows.Write(w0, null);
        var r0 = new TapeReader(w0.data, 0, w0.length);
        bool emptyOk = TapeRows.Read(w0.data, r0) == null && !TapeRows.Replay(demo, null).Any && w0.length == 1;
        Console.WriteLine($"n45rows empty record: {(emptyOk ? "GREEN" : "RED")} (1 byte; replays every row off)");
        if (!emptyOk) bad++;

        NightRows lacking = NightRows.FromDesign(PortData.Obj(JsJson.Parse(@"{ ""n46"": { ""wall46"": { ""sim"": true, ""enabled"": true, ""_why"": ""demo"" } } }")));
        string refused = null;
        try { TapeRows.Replay(lacking, rec); }
        catch (TapeFormatException e) { refused = e.Message; }
        Console.WriteLine($"n45rows refused: {(refused != null && refused.Contains("grip47") ? "GREEN" : "RED")} (a tape that ran grip47 against a build without it: {refused ?? "NOT refused"})");
        if (refused == null || !refused.Contains("grip47")) bad++;

        Console.WriteLine($"n45rows: {(bad == 0 ? "GREEN" : "RED")} ({bad} failing)");
        return bad == 0 ? 0 : 1;
    }
}
