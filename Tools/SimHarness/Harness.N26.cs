// NIGHT26 (the shared commit): tonight's headless instruments. Each builder implements its commands in its own file
// (Harness.N26.<Topic>.cs) by filling the declaration-only partial below; a missing body prints "NOT WRITTEN (RED)" and exits 1, so
// the proof-first rule has a RED line to show.
//   n26shared               the lead (this file): the N26 bits round trip, tape format 10 carries the NIGHT26 byte, a format 9 tape
//                           (recorded on NIGHT25's game) reads with every block off and replays EQUAL
//   weapons [--tag t]       builder W (Harness.N26.Weapons.cs, WeaponsCommand): every weapon's LT, costs, the weapon-by-enemy table
//   surfaces [--tag t]      builder M (Harness.N26.Surface.cs, SurfacesCommand): ice / boost strips, friction multiplier, golden exact when absent
//   hulk [--tag t]          builder B (Harness.N26.Boss.cs, HulkCommand): THE SLAG HULK killed by a bot, tells, tape EQUAL
//   glacier [--tag t]       builder B (Harness.N26.Boss.cs, GlacierCommand): THE GLACIER chase: the gap meter, tells, a bot run
//   wyrm26 [--tag t]        builder B (Harness.N26.Boss.cs, Wyrm26Command): proximity aggro, body / head ratio, the final attack
//   level26 <plan> ...      builder L (Harness.N26.Level.cs, Level26Command): one level plan to collision JSON and its report, ranks
using System;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void N26(string cmd, string[] args, ref int code)
    {
        if (cmd == "n26shared") N26SharedCommand(args, ref code);
        else if (cmd == "pulse26") Pulse26Command(args, ref code); // NIGHT26 builder W: Harness.N26.Weapons.cs
        else if (cmd == "weapons") { code = 1; Console.WriteLine("weapons: NOT WRITTEN (RED)"); WeaponsCommand(args, ref code); }
        else if (cmd == "surfaces") { code = 1; Console.WriteLine("surfaces: NOT WRITTEN (RED)"); SurfacesCommand(args, ref code); }
        else if (cmd == "hulk") { code = 1; Console.WriteLine("hulk: NOT WRITTEN (RED)"); HulkCommand(args, ref code); }
        else if (cmd == "glacier") { code = 1; Console.WriteLine("glacier: NOT WRITTEN (RED)"); GlacierCommand(args, ref code); }
        else if (cmd == "wyrm26") { code = 1; Console.WriteLine("wyrm26: NOT WRITTEN (RED)"); Wyrm26Command(args, ref code); }
        else if (cmd == "level26") { code = 1; Console.WriteLine("level26: NOT WRITTEN (RED)"); Level26Command(args, ref code); }
    }

    // A body replaces the RED line by setting `code` (0 = pass) and printing its own line.
    static partial void WeaponsCommand(string[] args, ref int code);
    static partial void SurfacesCommand(string[] args, ref int code);
    static partial void HulkCommand(string[] args, ref int code);
    static partial void GlacierCommand(string[] args, ref int code);
    static partial void Wyrm26Command(string[] args, ref int code);
    static partial void Level26Command(string[] args, ref int code);

    /// <summary>A format 10 tape's bytes as a format 9 build wrote them: the version 9 and no NIGHT26 byte (the byte after the NIGHT25
    /// byte). A tape that is already format 9 or older passes through.</summary>
    public static byte[] AsFormat9(byte[] b10)
    {
        if (BitConverter.ToInt32(b10, RunTape.Magic.Length) < 10) return b10;
        var r = new TapeReader(b10, RunTape.Magic.Length, b10.Length);
        r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
        r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); // flags, LAB, move, stamina, n19, n20, n21, n22
        r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI(); // n24 (two bytes), the campaign state
        r.Byte(); // n25
        int at = r.Position;
        r.Byte(); // n26
        var b9 = new byte[b10.Length - 1];
        Buffer.BlockCopy(b10, 0, b9, 0, at);
        Buffer.BlockCopy(b10, at + 1, b9, at, b10.Length - at - 1);
        var w = new TapeWriter(9);
        w.U32(9);
        Buffer.BlockCopy(w.data, 0, b9, RunTape.Magic.Length, 4);
        return b9;
    }

    static void N26SharedCommand(string[] args, ref int code)
    {
        bool ok = true;
        for (int bits = 0; bits < 1 << N26Blocks.Count; bits++)
            if (N26Blocks.FromBits(bits).Bits != bits) ok = false;
        Console.WriteLine($"n26 bits round trip over {1 << N26Blocks.Count} values: {(ok ? "PASS" : "FAIL")}; off.Any {N26Blocks.Off.Any}");

        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n26Override = N26Blocks.Off; // as a NIGHT25 build recorded it
        RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "n26shared", null, TapeScript.God);
        byte[] b10 = tape.ToBytes();
        int version = BitConverter.ToInt32(b10, RunTape.Magic.Length);
        bool v10 = version == RunTape.FormatVersion && version == 10 && RunTape.FromBytes(b10).n26 == 0;
        Console.WriteLine($"tape format {version} (want 10), n26 value read back {RunTape.FromBytes(b10).n26}: {(v10 ? "PASS" : "FAIL")}");

        var withBits = new RunTape { n24 = 0b101100101, n25 = 0b101011, n26 = 0b1010011, mode = "campaign" };
        RunTape back = RunTape.FromBytes(withBits.ToBytes());
        bool carry = back.n26 == 0b1010011 && back.n25 == 0b101011 && back.n24 == 0b101100101;
        Console.WriteLine($"format 10 carries n24/n25/n26 {back.n24:b}/{back.n25:b}/{back.n26:b}: {(carry ? "PASS" : "FAIL")}");

        byte[] b9 = AsFormat9(b10);
        RunTape old = RunTape.FromBytes(b9);
        TapeResult rep9 = TapeReplay.Replay(cfg, Map(), old);
        TapeResult rep10 = TapeReplay.Replay(cfg, Map(), RunTape.FromBytes(b10));
        Console.WriteLine($"format 9 tape: n26 {old.n26} (want 0), replay {(rep9.Equal ? "EQUAL" : "DIFFERS")}; format 10 tape replay {(rep10.Equal ? "EQUAL" : "DIFFERS")}");

        code = ok && v10 && carry && old.n26 == 0 && rep9.Equal && rep10.Equal ? 0 : 1;
        Console.WriteLine(code == 0 ? "n26shared PASS" : "n26shared FAIL");
    }
}
