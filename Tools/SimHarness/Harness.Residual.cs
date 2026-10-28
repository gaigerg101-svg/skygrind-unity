// NIGHT16 (builder M, the NIGHT14 residual): `residual <recipe...>` plays prior activity on ONE composition, then the
// seed-1 fingerprint run, and compares it with FingerprintBaseline (the Player smoke's steps 18 / 21: a played composition
// differed from the baseline at checkpoint 6). Recipe words, played in order:
//   ap:<seed>:<seconds>   an AUTOPILOT run (normal) on the composition, then EndRun / Detach (the attract, the farm)
//   fp                    a seed-1 fingerprint run (the smoke's earlier fingerprint runs)
//   die:<seed>            a run where the player is killed at once (a blow of twice the max HP), to RESULTS
//   idle:<seed>:<seconds> a run with no input for <seconds>
//   dmg:<hp> / nk:<n>     the director's per-step accumulators set as a run's last step can leave them (a hurt, a nearby kill)
//   at:<seed>:<seconds>   the attract (AUTOPILOT, god mode, metaSilent), stopped by ToTitle (an odd step count: 20.00833 s)
// `residual check`: the recipes the NIGHT14 residual came from, each must leave the next seed-1 run EQUAL (exit 1 if not).
// `residual diff <recipe...>`: after the recipe, the played composition and a fresh one each StartRun(1) and step the
// fingerprint script side by side; every system hashed (Harness.Lead's SlateSections) each step: the first difference.
using System;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void ResidualCommand(string[] args, ref int code) => code = Residual(args);

    static void ResidualPlay(SurvivalSim sim, string word)
    {
        string[] p = word.Split(':');
        if (p[0] == "fp")
        {
            sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out _);
            return;
        }
        if (p[0] == "dmg" || p[0] == "nk")
        {
            // the director's per-step accumulators left as a run's last step leaves them (a hurt / a nearby kill after its Step)
            var fi = typeof(Director).GetField(p[0] == "dmg" ? "damageTaken" : "nearbyKills", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            fi.SetValue(sim.director, double.Parse(p[1], CultureInfo.InvariantCulture));
            return;
        }
        if (p[0] == "at")
        {
            // the attract (SurvivalGame.Kiosk AttractRun / AttractStop): the AUTOPILOT, metaSilent, noLab, god mode; stopped by
            // ToTitle (run.End, never judged), the player's input back first, the pilot detached last
            uint seed = uint.Parse(p[1], CultureInfo.InvariantCulture);
            double secs = double.Parse(p[2], CultureInfo.InvariantCulture);
            IPlayerInput before = sim.input;
            bool silentWas = sim.metaSilent, noLabWas = sim.noLab;
            var ap = new AutopilotInput(sim, "normal", seed);
            sim.metaSilent = true;
            sim.noLab = true;
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.setup.seed = null;
            sim.UseInput(ap);
            sim.StartRun(seed, null, null);
            sim.health.max = sim.health.hp = 1e9;
            int max = MovementMath.StepsFor(secs, sim.step);
            for (int s = 0; s < max && sim.flow != Skygrind.Game.Flow.Results; s++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
            }
            Console.WriteLine($"residual   {word}: round {sim.run.round}, run {sim.run.state}, flow {sim.flow}, kills {sim.run.stats.kills}, alive {sim.enemies.AliveCount}, perks {sim.perks.owned.Count}, owned {sim.loadout.owned.Count}");
            sim.UseInput(before);
            sim.ToTitle();
            ap.Detach();
            sim.metaSilent = silentWas;
            sim.noLab = noLabWas;
            return;
        }
        if (p[0] == "ap")
        {
            uint seed = uint.Parse(p[1], CultureInfo.InvariantCulture);
            double secs = double.Parse(p[2], CultureInfo.InvariantCulture);
            IPlayerInput before = sim.input;
            var ap = new AutopilotInput(sim, "normal", seed);
            sim.UseInput(ap);
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(seed, null, null);
            int max = MovementMath.StepsFor(secs, sim.step);
            for (int s = 0; s < max && sim.flow != Skygrind.Game.Flow.Results; s++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
            }
            Console.WriteLine($"residual   {word}: round {sim.run.round}, flow {sim.flow}, kills {sim.run.stats.kills}, alive {sim.enemies.AliveCount}, perks {sim.perks.owned.Count}, owned {sim.loadout.owned.Count}");
            if (sim.flow != Skygrind.Game.Flow.Results) sim.EndRun();
            ap.Detach();
            sim.UseInput(before);
            return;
        }
        if (p[0] == "die" || p[0] == "idle")
        {
            uint seed = uint.Parse(p[1], CultureInfo.InvariantCulture);
            bool nl = sim.noLab, nr = sim.noRecords;
            sim.noLab = sim.noRecords = true;
            bool keep = p.Length > 3 && p[3] == "rec"; // ":rec": the run records (noRecords false after its start)
            sim.StartRun(seed, null, null);
            if (keep)
            {
                sim.noLab = nl;
                sim.noRecords = nr;
            }
            int max = p[0] == "idle" ? MovementMath.StepsFor(double.Parse(p[2], CultureInfo.InvariantCulture), sim.step) : 2400;
            for (int s = 0; s < max && sim.flow != Skygrind.Game.Flow.Results; s++)
            {
                if (p[0] == "die" && s == 10) sim.health.hp = 1;
                sim.Step();
            }
            Console.WriteLine($"residual   {word}: round {sim.run.round}, flow {sim.flow}");
            if (sim.flow != Skygrind.Game.Flow.Results && sim.run.Live) sim.EndRun();
            sim.noLab = nl;
            sim.noRecords = nr;
            return;
        }
        throw new ArgumentException("residual: unknown recipe word " + word);
    }

    static int Residual(string[] args)
    {
        if (args.Length > 1 && args[1] == "check")
        {
            // at:1:20.00833 = 2401 steps: the aim assist scans when its step count is even (aim.lock.scanEvery 2), and the count
            // ran on across runs; die:5:0 = the killing blow's 15 HP left in the director's damageTaken (and an odd count);
            // dmg:15 / nk:2 = the director's accumulators alone; at:1:20 (even) and fp = the controls that always passed.
            string[] recipes = { "fp", "at:1:20", "at:1:20.00833", "die:5:0", "idle:3:60", "dmg:15", "nk:2" };
            int bad = 0;
            foreach (string r in recipes)
            {
                SurvivalSim s = SurvivalSim.Headless(Config(), Map());
                ResidualPlay(s, r);
                List<string> lines = s.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
                int d = FingerprintBaseline.FirstDifference(lines);
                if (d >= 0) bad++;
                Console.WriteLine($"residual check {r}: then seed 1: {(d < 0 ? "EQUAL to the baseline" : $"first difference at checkpoint {d + 1}")}; {summary}");
            }
            Console.WriteLine($"residual check: {recipes.Length - bad} of {recipes.Length} EQUAL");
            return bad == 0 ? 0 : 1;
        }
        bool diff = args.Length > 1 && args[1] == "diff";
        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        var recipe = new List<string>();
        for (int i = diff ? 2 : 1; i < args.Length; i++) recipe.Add(args[i]);
        foreach (string w in recipe) ResidualPlay(sim, w);
        if (!diff)
        {
            List<string> lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            int d = FingerprintBaseline.FirstDifference(lines);
            Console.WriteLine($"residual {string.Join(" ", recipe)}: {(d < 0 ? "EQUAL to the baseline" : $"first difference at checkpoint {d + 1}")}; {summary}");
            return d < 0 ? 0 : 1;
        }
        SurvivalSim fresh = SurvivalSim.Headless(cfg, Map());
        {
            // the run snapshot of both right after StartRun(1) (the fingerprint's start), section by section
            bool nr = sim.noRecords, nl = sim.noLab;
            sim.noRecords = sim.noLab = fresh.noRecords = fresh.noLab = true;
            sim.setup.mode = fresh.setup.mode = RunSetup.Survival;
            sim.setup.heat = fresh.setup.heat = 0;
            sim.StartRun(SimFingerprint.Seed, null, null);
            fresh.StartRun(SimFingerprint.Seed, null, null);
            List<(string name, IRunState state)> sa = sim.SnapshotSections(), sb = fresh.SnapshotSections();
            for (int i = 0; i < sa.Count; i++)
            {
                string ja = RunSnapshot.Save(new List<(string, IRunState)> { sa[i] }), jb = RunSnapshot.Save(new List<(string, IRunState)> { sb[i] });
                if (ja == jb) continue;
                int at = 0;
                while (at < ja.Length && at < jb.Length && ja[at] == jb[at]) at++;
                {
                    // every top-level-ish token that differs (split on commas)
                    string[] ta = ja.Split(','), tb = jb.Split(',');
                    var diffs = new List<string>();
                    for (int k = 0; k < Math.Min(ta.Length, tb.Length) && diffs.Count < 30; k++)
                        if (ta[k] != tb[k]) diffs.Add(ta[k] + " | " + tb[k]);
                    Console.WriteLine($"residual snapshot: {sa[i].name}: {ta.Length} / {tb.Length} tokens; differing (played | fresh): {string.Join("  ;;  ", diffs)}");
                }
                int from = Math.Max(0, at - 160);
                Console.WriteLine($"residual snapshot: section {sa[i].name} differs at char {at}:\n  played: ...{ja.Substring(from, Math.Min(320, ja.Length - from))}\n  fresh:  ...{jb.Substring(from, Math.Min(320, jb.Length - from))}");
            }
            sim.ToTitle();
            fresh.ToTitle();
            sim.noRecords = fresh.noRecords = nr;
            sim.noLab = fresh.noLab = nl;
        }
        var a = new List<(string, ulong)[]>();
        var b = new List<(string, ulong)[]>();
        List<(string, ulong)[]> cur = null;
        Action onA = () => a.Add(SlateSections(sim)), onB = () => b.Add(SlateSections(fresh));
        sim.RunStarted += onA;
        sim.StepEnded += onA;
        fresh.RunStarted += onB;
        fresh.StepEnded += onB;
        sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out _);
        fresh.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out _);
        for (int i = 0; i < Math.Min(a.Count, b.Count); i++)
        {
            var differ = new List<string>();
            for (int s = 0; s < a[i].Length; s++)
                if (a[i][s].Item2 != b[i][s].Item2) differ.Add(a[i][s].Item1);
            if (differ.Count == 0) continue;
            Console.WriteLine($"residual diff: first difference {(i == 0 ? "right after StartRun" : $"after step {i}")}: {string.Join(", ", differ)}");
            return 1;
        }
        Console.WriteLine($"residual diff: no section differs over {a.Count} points");
        return 0;
    }
}
