// NIGHT22 (the shared commit; the lead's dispatch): tonight's headless instruments. Each builder implements its commands in its
// own file (Harness.N22.<Topic>.cs) by filling the partial method below; nothing runs without a body.
//   n22shared            the lead (this file): the N22 bits round trip, tape format 7 carries the NIGHT22 byte, a format 6 tape
//                        (recorded on NIGHT21's game) reads with every block off and replays EQUAL, and a GRIND tape round-trips its mode
//   arcline2 [--tag t]   builder M (Harness.N22.Arcline.cs, Arcline2Command)
//   stamp2 [--tag t]     builder S (Harness.N22.Stamp.cs, Stamp2Command)
//   deadline2 [--tag t]  builder S (Harness.N22.Deadline.cs, Deadline2Command)
//   grind2 [--tag t]     builder W (Harness.N22.Grind.cs, Grind2Command)
//   code2 [--tag t]      builder W2 (Harness.N22.Code.cs, Code2Command)
//   alloc [--tag t]      builder M (Harness.N22.Alloc.cs, AllocCommand): A-7, the per-system allocation counter
using System;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void N22(string cmd, string[] args, ref int code)
    {
        if (cmd == "n22shared") N22SharedCommand(args, ref code);
        else if (cmd == "arcline2") Arcline2Command(args, ref code);
        else if (cmd == "stamp2") Stamp2Command(args, ref code);
        else if (cmd == "deadline2") Deadline2Command(args, ref code);
        else if (cmd == "grind2") Grind2Command(args, ref code);
        else if (cmd == "code2") Code2Command(args, ref code);
        else if (cmd == "alloc") AllocCommand(args, ref code);
        else if (cmd == "trick2") TrickCommand(args, ref code);
    }

    static partial void Arcline2Command(string[] args, ref int code);
    static partial void Stamp2Command(string[] args, ref int code);
    static partial void Deadline2Command(string[] args, ref int code);
    static partial void Grind2Command(string[] args, ref int code);
    static partial void Code2Command(string[] args, ref int code);
    static partial void AllocCommand(string[] args, ref int code);

    /// <summary>A format 7 tape's bytes as a format 6 build wrote them: the version 6, no NIGHT22 byte (after the NIGHT21 byte).
    /// A tape that is already format 6 or older passes through.</summary>
    public static byte[] AsFormat6(byte[] b7)
    {
        b7 = AsFormat7(b7); // NIGHT24: the current format is 8; a format 7 tape passes through (Harness.N24.cs)
        if (BitConverter.ToInt32(b7, RunTape.Magic.Length) < 7) return b7;
        var r = new TapeReader(b7, RunTape.Magic.Length, b7.Length);
        r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
        r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); // flags, LAB, move, stamina, n19, n20, n21
        int at = r.Position;
        var b6 = new byte[b7.Length - 1];
        Buffer.BlockCopy(b7, 0, b6, 0, at);
        Buffer.BlockCopy(b7, at + 1, b6, at, b7.Length - at - 1);
        var w = new TapeWriter(8);
        w.U32(6);
        Buffer.BlockCopy(w.data, 0, b6, RunTape.Magic.Length, 4);
        return b6;
    }

    static void N22SharedCommand(string[] args, ref int code)
    {
        bool ok = true;
        for (int bits = 0; bits < 1 << N22Blocks.Count; bits++)
            if (N22Blocks.FromBits(bits).Bits != bits) ok = false;
        Console.WriteLine($"n22 bits round trip over {1 << N22Blocks.Count} values: {(ok ? "PASS" : "FAIL")}; off.Any {N22Blocks.Off.Any}");

        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n22Override = N22Blocks.Off; // as a NIGHT21 build recorded it
        RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "n22shared", null, TapeScript.God);
        byte[] b7 = AsFormat7(tape.ToBytes()); // NIGHT24: tape format 8 is current; the NIGHT22 byte is the same in its format 7 form
        int version = BitConverter.ToInt32(b7, RunTape.Magic.Length);
        bool v7 = version == 7 && RunTape.FromBytes(b7).n22 == 0;
        Console.WriteLine($"tape format {version} (want 7), n22 byte read back {RunTape.FromBytes(b7).n22}: {(v7 ? "PASS" : "FAIL")}");

        var withBits = new RunTape { n20 = 0b11001010, n21 = 0b101101, n22 = 0b110101, mode = RunSetup.Grind };
        RunTape back = RunTape.FromBytes(withBits.ToBytes());
        bool carry = back.n22 == 0b110101 && back.n21 == 0b101101 && back.n20 == 0b11001010;
        bool mode = back.mode == RunSetup.Grind;
        Console.WriteLine($"format 7 carries n20/n21/n22 bytes {back.n20:b}/{back.n21:b}/{back.n22:b}: {(carry ? "PASS" : "FAIL")}; a GRIND tape's mode round trips ({back.mode}): {(mode ? "PASS" : "FAIL")}");

        byte[] b6 = AsFormat6(b7);
        RunTape old = RunTape.FromBytes(b6);
        TapeResult rep6 = TapeReplay.Replay(cfg, Map(), old);
        TapeResult rep7 = TapeReplay.Replay(cfg, Map(), RunTape.FromBytes(b7));
        Console.WriteLine($"format 6 tape: n22 {old.n22} (want 0), replay {(rep6.Equal ? "EQUAL" : "DIFFERS")}; format 7 tape replay {(rep7.Equal ? "EQUAL" : "DIFFERS")}");

        code = ok && v7 && carry && mode && old.n22 == 0 && rep6.Equal && rep7.Equal ? 0 : 1;
        Console.WriteLine(code == 0 ? "n22shared PASS" : "n22shared FAIL");
    }
}
