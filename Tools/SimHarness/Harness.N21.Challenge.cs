// NIGHT21 U3-35.9 beyond row 5 (builder W2): `challenge [--selftest] [--steps n] [--cases n]` - CHALLENGE CODES' check.
//   1. 200 seeded cases (Mulberry32 draws) encode -> decode and keep every field;
//   2. corrupted codes (one edited character, a cut, an extra character) are rejected;
//   3. a run started from a decoded code (the code applied to a RunSetup, StartRun on setup.seed) records a tape whose bytes
//      (SHA-256) equal the tape of the same run started from the raw seed + mutators (format 6).
// --selftest runs Challenge21Tests' own assertions outside Unity (Tools/SimHarness/NunitMini.cs).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void ChallengeCommand(string[] args, ref int code)
    {
        if (Array.IndexOf(args, "--selftest") >= 0)
        {
            code = NUnit.Framework.MiniRunner.Run(typeof(Skygrind.Core.Tests.Challenge21Tests), Console.WriteLine) == 0 ? 0 : 1;
            return;
        }
        int cases = 200, steps = 600;
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--cases" && i + 1 < args.Length) cases = int.Parse(args[++i], CultureInfo.InvariantCulture);
            else if (args[i] == "--steps" && i + 1 < args.Length) steps = int.Parse(args[++i], CultureInfo.InvariantCulture);
        }
        GameConfig cfg = Config();
        var rng = new Mulberry32(0xC0DE21u);

        // 1. round trip
        int rtBad = 0;
        var codes = new List<string>();
        var specs = new List<Challenge21.Spec>();
        string[] modes = { RunSetup.Survival, RunSetup.Grind };
        for (int n = 0; n < cases; n++)
        {
            var s = new Challenge21.Spec
            {
                seed = (uint)(rng.Next() * 2147483648.0), heat = (int)(rng.Next() * 16), mode = modes[(int)(rng.Next() * 2)],
                target = (int)(rng.Next() * 128), mutators = new List<string>(),
            };
            for (int m = 0; m < MutatorMath.All.Length; m++)
                if (rng.Next() < 0.5) s.mutators.Add(MutatorMath.All[m]);
            string c = Challenge21.Encode(s, cfg);
            codes.Add(c);
            specs.Add(s);
            if (!Challenge21.TryDecode(c, cfg, out Challenge21.Spec d) || d.seed != s.seed || d.heat != s.heat || d.mode != s.mode
                || d.target != s.target || d.mutators.Count != s.mutators.Count || Challenge21.Encode(d, cfg) != c) rtBad++;
        }
        Console.WriteLine($"challenge: {cases} seeded cases round-trip (e.g. {codes[0]} {codes[1]}): {(rtBad == 0 ? "PASS" : "FAIL")} ({rtBad} bad)");

        // 2. corruption
        int tried = 0, accepted = 0, badLen = 0;
        string alpha = cfg.Str("design.n21.challenge.alphabet");
        for (int n = 0; n < cases; n++)
        {
            string raw = codes[n].Replace("-", "");
            int at = (int)(rng.Next() * raw.Length);
            char repl;
            do repl = alpha[(int)(rng.Next() * alpha.Length)]; while (repl == raw[at]);
            tried++;
            if (Challenge21.TryDecode(raw.Substring(0, at) + repl + raw.Substring(at + 1), cfg, out _)) accepted++;
            if (Challenge21.TryDecode(raw.Substring(0, raw.Length - 1), cfg, out _)) badLen++;
            if (Challenge21.TryDecode(raw + alpha[0], cfg, out _)) badLen++;
            if (Challenge21.TryDecode(raw.Substring(0, at) + "#" + raw.Substring(at + 1), cfg, out _)) badLen++;
        }
        // The checksum is checkBits wide: a one-character edit passes by chance about 1 in 2^checkBits; wrong lengths and
        // foreign characters never pass.
        int allowed = Math.Max(1, tried / 100);
        bool corruptOk = badLen == 0 && accepted <= allowed;
        Console.WriteLine($"challenge: corrupted codes rejected ({tried} one-character edits, {accepted} passed by chance, allowed {allowed}; {badLen} wrong-length / foreign-character codes passed): {(corruptOk ? "PASS" : "FAIL")}");

        // 3. the tape
        bool tapesOk = true;
        int[] heats = { 0, 1 };
        foreach (int heat in heats)
        {
            var spec = new Challenge21.Spec
            {
                seed = 41017u + (uint)heat, heat = heat, mode = RunSetup.Survival, target = 9,
                mutators = new List<string> { "speedDemon", "glassCannon" },
            };
            string codeText = Challenge21.Encode(spec, cfg);
            Challenge21.TryDecode(codeText, cfg, out Challenge21.Spec dec);

            SurvivalSim a = SurvivalSim.Headless(cfg, Map());
            a.noRecords = a.noLab = true;
            a.setup.heat = spec.heat;
            a.setup.mode = spec.mode;
            a.setup.mutators.AddRange(spec.mutators);
            RunTape ta = TapeScript.Record(a, spec.seed, steps, "challenge", null, TapeScript.God); // the raw seed + mutators

            SurvivalSim b = SurvivalSim.Headless(cfg, Map());
            b.noRecords = b.noLab = true;
            Challenge21.Apply(dec, b.setup); // the CODE field: the setup holds the decoded run, StartRun reads setup.seed
            RunTape tb = TapeScript.Record(b, null, steps, "challenge", null, TapeScript.God);

            ta.utcTicks = tb.utcTicks = 0; // the header's wall-clock stamp (when the tape was recorded) is the only byte that may differ
            byte[] ba = ta.ToBytes(), bb = tb.ToBytes();
            string ha = Convert.ToHexString(SHA256.HashData(ba)), hb = Convert.ToHexString(SHA256.HashData(bb));
            bool same = ha == hb && ba.Length == bb.Length;
            if (!same)
            {
                int first = -1, n = 0;
                for (int i = 0; i < Math.Min(ba.Length, bb.Length); i++)
                    if (ba[i] != bb[i]) { if (first < 0) first = i; n++; }
                Console.WriteLine($"challenge:   {n} bytes differ, the first at {first} (lengths {ba.Length} / {bb.Length})");
            }
            tapesOk &= same;
            Console.WriteLine($"challenge: code {codeText} heat {heat}: raw tape {ba.Length} B {ha.Substring(0, 16)}, decoded tape {bb.Length} B {hb.Substring(0, 16)}, format {BitConverter.ToInt32(ba, RunTape.Magic.Length)}, n21 byte {ta.n21}: {(same ? "EQUAL" : "DIFFERS")}");
        }
        Console.WriteLine($"challenge: a run from a decoded code records the tape of the raw seed + mutators: {(tapesOk ? "PASS" : "FAIL")}");

        code = rtBad == 0 && corruptOk && tapesOk ? 0 : 1;
        Console.WriteLine(code == 0 ? "challenge PASS" : "challenge FAIL");
    }
}
