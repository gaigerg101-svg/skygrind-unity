// NIGHT21 (the shared commit; the lead's dispatch): tonight's headless instruments. Each builder implements its commands in
// its own file (Harness.N21.<Topic>.cs) by filling the partial method below; nothing runs without a body.
//   n21shared             the lead (this file): the shared contract headless: the N21 bits round trip, tape format 6 carries
//                         the NIGHT21 byte, a format 5 tape (recorded on NIGHT20's game) reads with every block off and replays EQUAL
//   arcline [--tag t]     builder M (Harness.N21.Arcline.cs, ArclineCommand): THE ARCLINE's scripted swing / yank probe
//   slidebite [--tag t]   builder M (Harness.N21.SlideBite.cs, SlideBiteCommand): a scripted slide through three husks
//   stamp [--tag t]       builder S (Harness.N21.Stamp.cs, StampCommand): the round grades' distribution
//   deadline [--tag t]    builder S (Harness.N21.Deadline.cs, DeadlineCommand): the DEADLINE twist (chase --twist deadline is
//                         the lead's Sandbox hook)
//   grind [--seeds n]     builder W (Harness.N21.Grind.cs, GrindCommand): THE GRIND's autopilot, the median wave reached
using System;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void N21(string cmd, string[] args, ref int code)
    {
        if (cmd == "n21shared") N21SharedCommand(args, ref code);
        else if (cmd == "arcline") ArclineCommand(args, ref code);
        else if (cmd == "slidebite") SlideBiteCommand(args, ref code);
        else if (cmd == "stamp") StampCommand(args, ref code);
        else if (cmd == "deadline") DeadlineCommand(args, ref code);
        else if (cmd == "grind") GrindCommand(args, ref code);
        else if (cmd == "challenge") ChallengeCommand(args, ref code);
    }

    static partial void ArclineCommand(string[] args, ref int code);
    static partial void SlideBiteCommand(string[] args, ref int code);
    static partial void StampCommand(string[] args, ref int code);
    static partial void DeadlineCommand(string[] args, ref int code);
    static partial void GrindCommand(string[] args, ref int code);

    static void N21SharedCommand(string[] args, ref int code)
    {
        bool ok = true;
        for (int bits = 0; bits < 1 << N21Blocks.Count; bits++)
            if (N21Blocks.FromBits(bits).Bits != bits) ok = false;
        Console.WriteLine($"n21 bits round trip over {1 << N21Blocks.Count} values: {(ok ? "PASS" : "FAIL")}; off.Any {N21Blocks.Off.Any}");

        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n21Override = N21Blocks.Off; // as a NIGHT20 build recorded it
        RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "n21shared", null, TapeScript.God);
        byte[] b7 = tape.ToBytes(); // NIGHT22: the current format is 7 (premise changed: the NIGHT21 checks below read format 6, derived from it)
        byte[] b6 = AsFormat6(b7);
        int version = BitConverter.ToInt32(b7, RunTape.Magic.Length);
        bool v6 = version == RunTape.FormatVersion && RunTape.FromBytes(b6).n21 == 0 && BitConverter.ToInt32(b6, RunTape.Magic.Length) == 6;
        Console.WriteLine($"tape format {version} (want {RunTape.FormatVersion}), derived format 6 n21 byte read back {RunTape.FromBytes(b6).n21}: {(v6 ? "PASS" : "FAIL")}");

        var withBits = new RunTape { n19 = 0b1010101, n20 = 0b11001010, n21 = 0b101101 };
        RunTape back = RunTape.FromBytes(withBits.ToBytes());
        bool carry = back.n21 == 0b101101 && back.n20 == 0b11001010 && back.n19 == 0b1010101;
        Console.WriteLine($"format 6 carries n19/n20/n21 bytes {back.n19:b}/{back.n20:b}/{back.n21:b}: {(carry ? "PASS" : "FAIL")}");

        // A format 5 tape: the same bytes without the NIGHT21 byte (after the NIGHT20 byte), the version 5.
        var r = new TapeReader(b6, RunTape.Magic.Length, b6.Length);
        r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
        r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); // flags, LAB, move, stamina, n19, n20
        int at = r.Position;
        var b5 = new byte[b6.Length - 1];
        Buffer.BlockCopy(b6, 0, b5, 0, at);
        Buffer.BlockCopy(b6, at + 1, b5, at, b6.Length - at - 1);
        var w = new TapeWriter(8);
        w.U32(5);
        Buffer.BlockCopy(w.data, 0, b5, RunTape.Magic.Length, 4);
        RunTape old = RunTape.FromBytes(b5);
        TapeResult rep5 = TapeReplay.Replay(cfg, Map(), old);
        TapeResult rep6 = TapeReplay.Replay(cfg, Map(), RunTape.FromBytes(b6));
        Console.WriteLine($"format 5 tape: n21 {old.n21} (want 0), replay {(rep5.Equal ? "EQUAL" : "DIFFERS")}; format 6 tape replay {(rep6.Equal ? "EQUAL" : "DIFFERS")}");

        code = ok && v6 && carry && old.n21 == 0 && rep5.Equal && rep6.Equal ? 0 : 1;
        Console.WriteLine(code == 0 ? "n21shared PASS" : "n21shared FAIL");
    }
}
