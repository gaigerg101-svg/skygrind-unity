// NIGHT15 (the lead's harness commands):
//   aim15            U3-28.7's checks without NUnit (the Editor may be busy): the Game's settings boot order (the design
//                    defaults, Load, the one-time migration) on a fresh profile, a saved 'snap', a saved 'lock'; and the
//                    fingerprint run with the mouse's SNAP MODE at OFF (RunFingerprint pins the web's 'snap').
//   mapfp <path>     the fingerprint run on another map file (U3-28.4's round trip: an exported copy of survival.json must
//                    print the baseline's hashes).
using System;
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static bool Night15(string cmd, string[] args, ref int code)
    {
        if (cmd == "verbtrace")
        {
            code = VerbTrace(args); // Harness.VerbsDebug.cs
            return true;
        }
        if (cmd == "verbs")
        {
            if (args.Length < 2) throw new ArgumentException("usage: verbs <map.json>");
            code = Verbs(args[1]); // Harness.Verbs.cs
            return true;
        }
        if (cmd == "aim15")
        {
            code = Aim15();
            return true;
        }
        if (cmd == "newmap")
        {
            // The smoke's step 24 headless: the AUTOPILOT on a map file, seed 4242 normal, `seconds` of sim time, twice.
            if (args.Length < 2) throw new ArgumentException("usage: newmap <map.json> [seconds]");
            double seconds = args.Length > 2 ? double.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture) : 300;
            GameConfig cfg = Config();
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(args[1]))));
            var a = new List<string>();
            var b = new List<string>();
            var t = System.Diagnostics.Stopwatch.StartNew();
            Farm.Record ra = Farm.RunOne(cfg, map, 4242, "normal", seconds, a);
            Farm.Record rb = Farm.RunOne(cfg, map, 4242, "normal", seconds, b);
            int same = 0;
            for (int i = 0; i < a.Count && i < b.Count; i++)
                if (a[i] == b[i]) same++;
            bool ok = ra.brain == "botBrain" && ra.cleared >= 3 && a.Count > 0 && same == a.Count && a.Count == b.Count && ra.round == rb.round;
            Console.WriteLine($"newmap {(ok ? "PASS" : "FAIL")} autopilot on {map.name}: seed 4242 normal ({ra.brain}) twice for {seconds:0} s: round {ra.round} / {rb.round}, cleared {ra.cleared}, {ra.kills} kills, cause {ra.cause}, spent {ra.spent:0} of {ra.earned:0}; {same} of {a.Count} hashes equal ({t.Elapsed.TotalSeconds:0.0} s)");
            code = ok ? 0 : 1;
            return true;
        }
        if (cmd == "mapfp")
        {
            if (args.Length < 2) throw new ArgumentException("usage: mapfp <map.json>");
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.GetFullPath(args[1]))));
            SurvivalSim sim = SurvivalSim.Headless(Config(), map);
            List<string> lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            int diff = FingerprintBaseline.FirstDifference(lines);
            Console.WriteLine($"mapfp {args[1]}: {lines.Count} hashes: {(diff < 0 ? "EQUAL to the baseline" : $"first difference at checkpoint {diff + 1}")}; {summary}");
            code = diff < 0 ? 0 : 1;
            return true;
        }
        return false;
    }

    static int Aim15()
    {
        GameConfig cfg = Config();
        int fails = 0;
        void Check(bool ok, string what)
        {
            if (!ok) fails++;
            Console.WriteLine($"aim15 {(ok ? "PASS" : "FAIL")} {what}");
        }
        SettingsStore Boot(string json, out bool resave)
        {
            var store = new SettingsStore(cfg);
            SettingsMath.ApplyDesignDefaults(store, cfg);
            bool loaded = store.Load(json);
            resave = SettingsMath.MigrateDesignDefaults(store, cfg, !string.IsNullOrEmpty(json)) || loaded;
            return store;
        }
        SettingsStore fresh = Boot(null, out _);
        Check(fresh.Str("aimAssist") == "off" && fresh.Str("padAimAssist") == "assist" && fresh.Bool("padVibration"),
            $"a fresh profile: aimAssist {fresh.Str("aimAssist")}, padAimAssist {fresh.Str("padAimAssist")}, padVibration {fresh.Bool("padVibration")}");
        const string old = "{\"aimAssist\":\"snap\",\"padAimAssist\":\"snap\",\"padVibration\":false,\"_migrated\":[\"sens14\",\"snap1\"],\"_version\":1}";
        SettingsStore s = Boot(old, out bool resave);
        Check(resave && s.Str("aimAssist") == "off" && s.Str("padAimAssist") == "assist" && s.Bool("padVibration"),
            $"a saved snap / snap / false migrates: {s.Str("aimAssist")} / {s.Str("padAimAssist")} / {s.Bool("padVibration")}, resave {resave}");
        s.Set("aimAssist", "snap");
        s.Set("padVibration", false);
        SettingsStore again = Boot(s.Save(), out bool resave2);
        Check(again.Str("aimAssist") == "snap" && !again.Bool("padVibration") && !resave2,
            $"once: the player's own snap / false stays: {again.Str("aimAssist")} / {again.Bool("padVibration")}, resave {resave2}");
        SettingsStore locked = Boot("{\"aimAssist\":\"lock\",\"padAimAssist\":\"lock\",\"_migrated\":[\"sens14\",\"snap1\"],\"_version\":1}", out _);
        Check(locked.Str("aimAssist") == "lock" && locked.Str("padAimAssist") == "lock", $"a saved lock is kept: {locked.Str("aimAssist")} / {locked.Str("padAimAssist")}");
        var web = new SettingsStore(cfg);
        web.Load(null);
        Check(web.Str("aimAssist") == "snap" && web.Str("padAimAssist") == "snap", $"the web's store alone: {web.Str("aimAssist")} / {web.Str("padAimAssist")}");
        // The fingerprint with the mouse at OFF.
        var store = new SettingsStore(cfg, new List<(SettingItem, object)>(), new List<(string, string)>());
        store.Load("");
        store.Set("aimAssist", "off");
        var sim = new SurvivalSim(cfg, Map(), new EventBus(), new Lab(cfg), new ScriptedInput(), new Profile(), store.Num, store.Bool, store.Str, _ => false, a => a);
        sim.ToTitle();
        foreach (SurvivalSim.Machine m in sim.machines) sim.RideCore(m);
        List<string> lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
        int diff = FingerprintBaseline.FirstDifference(lines);
        Check(diff < 0 && sim.assistModeFor == null && sim.AssistMode == "off",
            $"the mouse at OFF: the fingerprint {(diff < 0 ? "EQUAL to the baseline" : $"differs at checkpoint {diff + 1}")}, the hook handed back {sim.assistModeFor == null}, the setting {sim.AssistMode}");
        Console.WriteLine($"aim15: {(fails == 0 ? "all passed" : $"{fails} failed")}");
        return fails == 0 ? 0 : 1;
    }
}
