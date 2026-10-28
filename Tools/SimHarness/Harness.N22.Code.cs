// NIGHT22 U3-36.8 (builder W2): `code2 [--steps n] [--tag t]` - the contract `challenge`'s headless lines (design.n21.challenge).
//   1. the TITLE field by pad: a code is filled with only UP / DOWN / RIGHT presses (Challenge21.Entry), then read back as the run
//      it holds; a paste fills it too; a mistyped checksum is refused
//   2. a code carrying a mutator starts a run in which it is ACTIVE: the run state is read (RunStats.mutators and the numbers
//      the mutator sets: damage dealt / taken, the enemy pace), not only the tape. No code: all of them x1 (and the list empty)
//   3. two runs of one code record the same tape (the header's wall-clock stamp zeroed): SHA-256 equal
//   4. the RESULTS line shows the run's code, and typing that code again plays the same run
// With the row off (--off21, or design.n21.challenge.enabled false) the reader adds nothing: lines 2 and 4 read RED, the run is NIGHT21's.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static partial void Code2Command(string[] args, ref int code)
    {
        if (Array.IndexOf(args, "--selftest") >= 0)
        {
            code = NUnit.Framework.MiniRunner.Run(typeof(Skygrind.Core.Tests.Code22Tests), Console.WriteLine) == 0 ? 0 : 1;
            return;
        }
        int steps = 600;
        string tag = "";
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--steps" && i + 1 < args.Length) steps = int.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (args[i] == "--tag" && i + 1 < args.Length) tag = args[++i];
        }
        GameConfig cfg = Config();
        bool on = Challenge21.Enabled(cfg);
        Console.WriteLine($"code2: design.n21.challenge {(on ? "ON" : "OFF")}{(tag.Length > 0 ? " [" + tag + "]" : "")}");
        bool ok = true;

        var spec = new Challenge21.Spec
        {
            seed = 1234567u, heat = 2, mode = RunSetup.Survival, target = 9,
            mutators = new List<string> { "glassCannon", "speedDemon" },
        };
        string codeText = Challenge21.Encode(spec, cfg);

        // ---- 1. the TITLE field, by pad ----
        {
            string want = codeText.Replace("-", "");
            var e = new Challenge21.Entry(cfg);
            int presses = 0;
            for (int slot = 0; slot < e.Length; slot++)
            {
                int guard = 0;
                while (!SlotIs(e, slot, want[slot]) && guard++ < 40) { e.Cycle(+1); presses++; }
                if (slot < e.Length - 1) { e.Move(+1); presses++; }
            }
            bool padOk = e.TryResult(cfg, out Challenge21.Spec got) && got.seed == spec.seed && got.heat == spec.heat
                && got.mutators.Count == 2 && got.mutators.Contains("glassCannon") && got.mutators.Contains("speedDemon");
            var p = new Challenge21.Entry(cfg);
            p.Paste(codeText.ToLowerInvariant());
            bool pasteOk = p.TryResult(cfg, out Challenge21.Spec pg) && pg.seed == spec.seed;
            var bad = new Challenge21.Entry(cfg);
            bad.Paste(codeText);
            bad.Move(-bad.Length);
            bad.Cycle(+1); // one slot off: the checksum refuses it
            bool badRefused = !bad.TryResult(cfg, out _);
            var back = new Challenge21.Entry(cfg);
            back.Paste("ABCD");
            back.Back();
            bool backOk = back.Filled == 3;
            ok &= padOk && pasteOk && badRefused && backOk;
            Console.WriteLine($"code2: TITLE field by pad: {presses} UP / RIGHT presses fill {e.Text()} -> run seed {got.seed} HEAT {got.heat} [{string.Join(",", got.mutators)}]; paste {(pasteOk ? "reads it" : "FAILS")}; a wrong slot {(badRefused ? "refused" : "ACCEPTED")}; BACK {(backOk ? "clears" : "FAILS")}: {(padOk && pasteOk && badRefused && backOk ? "PASS" : "FAIL")}");
        }

        // ---- 2. the mutators are active in the run ----
        double kd = cfg.Num("mutators.kinds.glassCannon.dealt"), kt = cfg.Num("mutators.kinds.glassCannon.taken"), ks = cfg.Num("mutators.kinds.speedDemon.enemySpeed");
        {
            Challenge21.TryDecode(codeText, cfg, out Challenge21.Spec dec);
            SurvivalSim s = SurvivalSim.Headless(cfg, Map());
            s.noRecords = s.noLab = true;
            Challenge21.Apply(dec, s.setup);
            s.StartRun(null, null, null);
            var list = s.run.stats.mutators;
            bool listed = list.Contains("glassCannon") && list.Contains("speedDemon");
            double d = s.loadout.damageMult, t = s.health.takenMult, sp = s.brain.speedMult;
            bool numbers = d == kd && t == kt && sp == ks;
            SurvivalSim n = SurvivalSim.Headless(cfg, Map());
            n.noRecords = n.noLab = true;
            n.StartRun(1234567u, null, null);
            bool none = n.run.stats.mutators.Count == 0 && n.loadout.damageMult == 1 && n.health.takenMult == 1 && n.brain.speedMult == 1;
            bool pass = listed && numbers && none;
            ok &= pass;
            Console.WriteLine($"code2: mutators active in the run: stats.mutators [{string.Join(",", list)}], damage dealt x{d.ToString(CultureInfo.InvariantCulture)} (config x{kd.ToString(CultureInfo.InvariantCulture)}), taken x{t.ToString(CultureInfo.InvariantCulture)} (x{kt.ToString(CultureInfo.InvariantCulture)}), enemy pace x{sp.ToString(CultureInfo.InvariantCulture)} (x{ks.ToString(CultureInfo.InvariantCulture)}); no code: list {n.run.stats.mutators.Count}, x{n.loadout.damageMult.ToString(CultureInfo.InvariantCulture)} x{n.health.takenMult.ToString(CultureInfo.InvariantCulture)} x{n.brain.speedMult.ToString(CultureInfo.InvariantCulture)}: {(pass ? "PASS" : "RED")}");
        }

        // ---- 3. the same code, the same tape ----
        {
            string[] hash = new string[2];
            int len = 0;
            for (int k = 0; k < 2; k++)
            {
                Challenge21.TryDecode(codeText, cfg, out Challenge21.Spec dec);
                SurvivalSim s = SurvivalSim.Headless(cfg, Map());
                s.noRecords = s.noLab = true;
                Challenge21.Apply(dec, s.setup);
                RunTape tp = TapeScript.Record(s, null, steps, "code2", null, TapeScript.God);
                tp.utcTicks = 0; // the header's clock stamp is the one byte that may differ
                byte[] b = tp.ToBytes();
                len = b.Length;
                hash[k] = Convert.ToHexString(SHA256.HashData(b));
            }
            bool same = hash[0] == hash[1];
            // for the record: the same run with no mutator (the control) is a different tape when the mutators change the sim
            SurvivalSim c = SurvivalSim.Headless(cfg, Map());
            c.noRecords = c.noLab = true;
            c.setup.heat = spec.heat;
            RunTape ct = TapeScript.Record(c, spec.seed, steps, "code2", null, TapeScript.God);
            ct.utcTicks = 0;
            string ch = Convert.ToHexString(SHA256.HashData(ct.ToBytes()));
            ok &= same;
            Console.WriteLine($"code2: one code, two runs: tape {len} B {hash[0].Substring(0, 16)} / {hash[1].Substring(0, 16)}: {(same ? "EQUAL" : "DIFFERS")}; the no-mutator control {(ch == hash[0] ? "is the same tape (the mutators changed nothing a tape sees)" : "differs from it")}");
        }

        // ---- 4. the RESULTS line ----
        {
            Challenge21.TryDecode(codeText, cfg, out Challenge21.Spec dec);
            SurvivalSim s = SurvivalSim.Headless(cfg, Map());
            s.noRecords = s.noLab = true;
            Challenge21.Apply(dec, s.setup);
            uint seedPlayed = dec.seed;
            s.StartRun(null, null, null);
            string line = Challenge21.ResultsLine(cfg, s.setup, seedPlayed, dec.target);
            bool pass = line == "CODE " + codeText;
            ok &= pass;
            Console.WriteLine($"code2: RESULTS line \"{line ?? "(none)"}\" (the code typed was {codeText}): {(pass ? "PASS" : "RED")}");
        }

        code = ok ? 0 : 1;
        Console.WriteLine(ok ? "code2 PASS" : "code2 FAIL");

    }

    static bool SlotIs(Challenge21.Entry e, int slot, char want)
    {
        // The model keeps no per-slot getter; the shown text is the read: groups of four, hyphens between.
        string t = e.Text();
        int at = slot + slot / 4;
        return t[at] == want;
    }
}
