// NIGHT25 (the shared commit): tonight's headless instruments. Each builder implements its commands in its own file
// (Harness.N25.<Topic>.cs) by filling the declaration-only partial below; a missing body prints "NOT WRITTEN (RED)" and exits 1, so
// the proof-first rule has a RED line to show.
//   n25shared               the lead (this file): the N25 bits round trip, tape format 9 carries the NIGHT25 byte, a format 8 tape
//                           (recorded on NIGHT24's game) reads with every block off and replays EQUAL
//   meleefire [--tag t]     builder C (Harness.N25.Combat.cs, MeleeFireCommand): shots and punches held together in 3 s, before / after
//   pulse [--tag t]         builder C (Harness.N25.Combat.cs, PulseCommand): the PULSE builds by its own hits, launches, small damage
//   wyrm25 [--tag t]        builder W (Harness.N25.Wyrm.cs, Wyrm25Command): aim / hits per minute / bar / death, before and after
//   tutorial [--tag t]      builder L (Harness.N25.Level.cs, TutorialCommand): the six stages by bot, no soft-lock
//   curriculum [--tag t]    builder L (Harness.N25.Level.cs, CurriculumCommand): levels 2-10, validate report, clear rates
//   level25 <plan> ...      builder L (Harness.N25.Level.cs, Level25Command): one level plan to collision JSON and its report
using System;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void N25(string cmd, string[] args, ref int code)
    {
        if (cmd == "n25shared") N25SharedCommand(args, ref code);
        else if (cmd == "meleefire") { code = 1; Console.WriteLine("meleefire: NOT WRITTEN (RED)"); MeleeFireCommand(args, ref code); }
        else if (cmd == "pulse") { code = 1; Console.WriteLine("pulse: NOT WRITTEN (RED)"); PulseCommand(args, ref code); }
        else if (cmd == "wyrm25") { code = 1; Console.WriteLine("wyrm25: NOT WRITTEN (RED)"); Wyrm25Command(args, ref code); }
        else if (cmd == "tutorial") { code = 1; Console.WriteLine("tutorial: NOT WRITTEN (RED)"); TutorialCommand(args, ref code); }
        else if (cmd == "curriculum") { code = 1; Console.WriteLine("curriculum: NOT WRITTEN (RED)"); CurriculumCommand(args, ref code); }
        else if (cmd == "level25") { code = 1; Console.WriteLine("level25: NOT WRITTEN (RED)"); Level25Command(args, ref code); }
    }

    // A body replaces the RED line by setting `code` (0 = pass) and printing its own line.
    static partial void MeleeFireCommand(string[] args, ref int code);
    static partial void PulseCommand(string[] args, ref int code);
    static partial void Wyrm25Command(string[] args, ref int code);
    static partial void TutorialCommand(string[] args, ref int code);
    static partial void CurriculumCommand(string[] args, ref int code);
    static partial void Level25Command(string[] args, ref int code);

    /// <summary>A format 9 tape's bytes as a format 8 build wrote them: the version 8 and no NIGHT25 byte (the byte after the campaign
    /// state). A tape that is already format 8 or older passes through.</summary>
    public static byte[] AsFormat8(byte[] b9)
    {
        b9 = AsFormat9(b9); // NIGHT26: the current format is 10; its format 9 form first
        if (BitConverter.ToInt32(b9, RunTape.Magic.Length) < 9) return b9;
        var r = new TapeReader(b9, RunTape.Magic.Length, b9.Length);
        r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
        r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); // flags, LAB, move, stamina, n19, n20, n21, n22
        r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI(); // n24 (two bytes), the campaign state
        int at = r.Position;
        r.Byte(); // n25
        var b8 = new byte[b9.Length - 1];
        Buffer.BlockCopy(b9, 0, b8, 0, at);
        Buffer.BlockCopy(b9, at + 1, b8, at, b9.Length - at - 1);
        var w = new TapeWriter(8);
        w.U32(8);
        Buffer.BlockCopy(w.data, 0, b8, RunTape.Magic.Length, 4);
        return b8;
    }

    static void N25SharedCommand(string[] args, ref int code)
    {
        bool ok = true;
        for (int bits = 0; bits < 1 << N25Blocks.Count; bits++)
            if (N25Blocks.FromBits(bits).Bits != bits) ok = false;
        Console.WriteLine($"n25 bits round trip over {1 << N25Blocks.Count} values: {(ok ? "PASS" : "FAIL")}; off.Any {N25Blocks.Off.Any}");

        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n25Override = N25Blocks.Off; // as a NIGHT24 build recorded it
        RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "n25shared", null, TapeScript.God);
        byte[] b9 = tape.ToBytes();
        int version = BitConverter.ToInt32(b9, RunTape.Magic.Length);
        bool v9 = version == RunTape.FormatVersion && RunTape.FromBytes(b9).n25 == 0;
        Console.WriteLine($"tape format {version} (want {RunTape.FormatVersion}; NIGHT26 premise: the current format is 10), n25 value read back {RunTape.FromBytes(b9).n25}: {(v9 ? "PASS" : "FAIL")}");

        var withBits = new RunTape { n24 = 0b101100101, n25 = 0b101011, mode = "campaign" };
        RunTape back = RunTape.FromBytes(withBits.ToBytes());
        bool carry = back.n25 == 0b101011 && back.n24 == 0b101100101;
        Console.WriteLine($"format 9 carries n24/n25 {back.n24:b}/{back.n25:b}: {(carry ? "PASS" : "FAIL")}");

        byte[] b8 = AsFormat8(b9);
        RunTape old = RunTape.FromBytes(b8);
        TapeResult rep8 = TapeReplay.Replay(cfg, Map(), old);
        TapeResult rep9 = TapeReplay.Replay(cfg, Map(), RunTape.FromBytes(b9));
        Console.WriteLine($"format 8 tape: n25 {old.n25} (want 0), replay {(rep8.Equal ? "EQUAL" : "DIFFERS")}; format 9 tape replay {(rep9.Equal ? "EQUAL" : "DIFFERS")}");

        code = ok && v9 && carry && old.n25 == 0 && rep8.Equal && rep9.Equal ? 0 : 1;
        Console.WriteLine(code == 0 ? "n25shared PASS" : "n25shared FAIL");
    }
}
