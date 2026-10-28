// NIGHT24 (the shared commit; the lead's dispatch): tonight's headless instruments. Each builder implements its commands in its
// own file (Harness.N24.<Topic>.cs) by filling the partial method below; nothing runs without a body (a missing body prints
// "NOT WRITTEN (RED)" and exits 1, so the proof-first rule has a RED line to show).
//   n24shared            the lead (this file): the N24 bits round trip, tape format 8 carries the NIGHT24 value and the campaign
//                        state, a format 7 tape (recorded on NIGHT22's game) reads with every block off and replays EQUAL
//   levelgen <plan> ...  builder L (Harness.N24.Level.cs, LevelgenCommand): a level plan to collision JSON, a bot walking it
//   campaign [--tag t]   builder L (Harness.N24.Level.cs, CampaignCommand): layer 1 door to door
//   wyrm [--tag t]       builder B (Harness.N24.Wyrm.cs, WyrmCommand): a bot fights the worm
//   fist [--tag t]       builder F (Harness.N24.Fist.cs, FistCommand): idle 60 s = +0, the kill-run charge number
//   shotgun [--tag t]    builder R (Harness.N24.Shotgun.cs, ShotgunCommand): the shotgun's fire interval, before and after
//   hitch [--tag t]      the lead (Harness.N24.Hitch.cs, HitchCommand): the screen-switch hitch tour
using System;
using Skygrind.Core;
using Skygrind.Game;

public static partial class Harness
{
    static void N24(string cmd, string[] args, ref int code)
    {
        if (cmd == "n24shared") N24SharedCommand(args, ref code);
        else if (cmd == "levelgen") { code = 1; Console.WriteLine("levelgen: NOT WRITTEN (RED)"); LevelgenCommand(args, ref code); }
        else if (cmd == "campaign") { code = 1; Console.WriteLine("campaign: NOT WRITTEN (RED)"); CampaignCommand(args, ref code); }
        else if (cmd == "monolith") MonolithCommand(args, ref code); // NIGHT24 leftover 8 (builder B): Harness.N24.Monolith.cs
        else if (cmd == "wyrm") { code = 1; Console.WriteLine("wyrm: NOT WRITTEN (RED)"); WyrmCommand(args, ref code); }
        else if (cmd == "fist") { code = 1; Console.WriteLine("fist: NOT WRITTEN (RED)"); FistCommand(args, ref code); }
        else if (cmd == "shotgun") { code = 1; Console.WriteLine("shotgun: NOT WRITTEN (RED)"); ShotgunCommand(args, ref code); }
        else if (cmd == "remove") RemoveCommand(args, ref code);
        else if (cmd == "ranks") RanksCommand(args, ref code);
        else if (cmd == "hitch") { code = 1; Console.WriteLine("hitch: NOT WRITTEN (RED)"); HitchCommand(args, ref code); }
    }

    // A body replaces the RED line by setting `code` (0 = pass) and printing its own line.
    static partial void LevelgenCommand(string[] args, ref int code);
    static partial void CampaignCommand(string[] args, ref int code);
    static partial void WyrmCommand(string[] args, ref int code);
    static partial void FistCommand(string[] args, ref int code);
    static partial void ShotgunCommand(string[] args, ref int code);
    static partial void HitchCommand(string[] args, ref int code);

    /// <summary>A format 8 tape's bytes as a format 7 build wrote them: the version 7, no NIGHT24 value and no campaign state (after the
    /// NIGHT22 byte). A tape that is already format 7 or older passes through.</summary>
    public static byte[] AsFormat7(byte[] b8)
    {
        b8 = AsFormat8(b8); // NIGHT25: the current format is 9; its format 8 form first
        if (BitConverter.ToInt32(b8, RunTape.Magic.Length) < 8) return b8;
        var r = new TapeReader(b8, RunTape.Magic.Length, b8.Length);
        r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
        r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); // flags, LAB, move, stamina, n19, n20, n21, n22
        int at = r.Position;
        r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI(); // n24 (two bytes), the campaign state
        int end = r.Position;
        var b7 = new byte[b8.Length - (end - at)];
        Buffer.BlockCopy(b8, 0, b7, 0, at);
        Buffer.BlockCopy(b8, end, b7, at, b8.Length - end);
        var w = new TapeWriter(8);
        w.U32(7);
        Buffer.BlockCopy(w.data, 0, b7, RunTape.Magic.Length, 4);
        return b7;
    }

    static void N24SharedCommand(string[] args, ref int code)
    {
        bool ok = true;
        for (int bits = 0; bits < 1 << N24Blocks.Count; bits++)
            if (N24Blocks.FromBits(bits).Bits != bits) ok = false;
        Console.WriteLine($"n24 bits round trip over {1 << N24Blocks.Count} values: {(ok ? "PASS" : "FAIL")}; off.Any {N24Blocks.Off.Any}");

        GameConfig cfg = Config();
        SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
        sim.noRecords = sim.noLab = true;
        sim.n24Override = N24Blocks.Off; // as a NIGHT22 build recorded it
        RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "n24shared", null, TapeScript.God);
        byte[] b8 = tape.ToBytes();
        int version = BitConverter.ToInt32(b8, RunTape.Magic.Length);
        bool v8 = version == RunTape.FormatVersion && RunTape.FromBytes(b8).n24 == 0 && !RunTape.FromBytes(b8).campaign.Any;
        Console.WriteLine($"tape format {version} (want {RunTape.FormatVersion}; NIGHT25 premise: the current format is 9), n24 value read back {RunTape.FromBytes(b8).n24}: {(v8 ? "PASS" : "FAIL")}");

        var camp = new Campaign24State { levelId = "layer1", room = 3, checkpoint = 2, limits = 5 };
        var withBits = new RunTape { n22 = 0b110101, n24 = 0b101100101, campaign = camp, mode = "campaign" };
        RunTape back = RunTape.FromBytes(withBits.ToBytes());
        bool carry = back.n24 == 0b101100101 && back.n22 == 0b110101 && back.campaign.Same(camp);
        bool mode = back.mode == "campaign";
        Console.WriteLine($"format 8 carries n22/n24 {back.n22:b}/{back.n24:b} and the campaign state ({back.campaign.levelId} room {back.campaign.room} checkpoint {back.campaign.checkpoint} limits {back.campaign.limits}): {(carry ? "PASS" : "FAIL")}; the mode round trips ({back.mode}): {(mode ? "PASS" : "FAIL")}");

        byte[] b7 = AsFormat7(b8);
        RunTape old = RunTape.FromBytes(b7);
        TapeResult rep7 = TapeReplay.Replay(cfg, Map(), old);
        TapeResult rep8 = TapeReplay.Replay(cfg, Map(), RunTape.FromBytes(b8));
        Console.WriteLine($"format 7 tape: n24 {old.n24} (want 0), campaign {(old.campaign.Any ? "set" : "none")}, replay {(rep7.Equal ? "EQUAL" : "DIFFERS")}; format 8 tape replay {(rep8.Equal ? "EQUAL" : "DIFFERS")}");

        code = ok && v8 && carry && mode && old.n24 == 0 && !old.campaign.Any && rep7.Equal && rep8.Equal ? 0 : 1;
        Console.WriteLine(code == 0 ? "n24shared PASS" : "n24shared FAIL");
    }
}
