// NIGHT21 (the shared commit, the lead): the contract every builder compiles against.
//   1. the six NIGHT21 blocks: the bits round trip, every block off is NIGHT20's game, each block present in the design
//      config (design.n21.<row>) with an `enabled` and a `_why`, all off in the shared commit; the Player's copy stays
//      byte-identical to the Design copy
//   2. --off21 / -skygrindOff21 (N21Blocks.ForceOff) takes every block off for a run whatever the config says
//   3. THE TAPE's format 6 carries the NIGHT21 byte; a format 5 tape is read with every block off and replays EQUAL
//      (recorded on NIGHT20's game)
//   4. the composed fingerprint runs with every NIGHT21 block off until the close (FingerprintBaseline.N21OnPath)
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N21SharedTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static readonly string[] Rows = { "arcline", "slideBites", "stamp", "deadline", "grind" }; // NIGHT24: bit 5 (the removed replay) has no config row

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static SurvivalSim Sim(GameConfig cfg)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noRecords = sim.noLab = true;
            return sim;
        }

        [Test]
        public void N21Blocks_Bits_RoundTrip_AndOffIsNight20()
        {
            for (int bits = 0; bits < 1 << N21Blocks.Count; bits++)
                Assert.That(N21Blocks.FromBits(bits).Bits, Is.EqualTo(bits));
            Assert.That(N21Blocks.Off.Any, Is.False);
            Assert.That(N21Blocks.Off.Bits, Is.EqualTo(0));
            Assert.That(N21Blocks.Names.Length, Is.EqualTo(N21Blocks.Count));
            Assert.That(N21Blocks.Count, Is.EqualTo(Rows.Length + 1), "five rows plus the retired bit 5");
        }

        [Test]
        public void EveryN21Block_IsInTheDesignConfig_WithItsWhy()
        {
            GameConfig cfg = Cfg();
            Assert.That(cfg.Has("design.n21._why"), Is.True, "n21._why");
            foreach (string b in Rows)
            {
                Assert.That(cfg.Has("design.n21." + b + ".enabled"), Is.True, b + ".enabled");
                Assert.That(cfg.Has("design.n21." + b + "._why"), Is.True, b + "._why");
            }
        }

        [Test]
        public void ThePlayersConfigCopy_IsByteIdentical()
        {
            string design = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Design", "unity-config.json"));
            string player = Path.Combine(Application.streamingAssetsPath, "Design", "unity-config.json");
            Assert.That(File.ReadAllBytes(player), Is.EqualTo(File.ReadAllBytes(design)));
        }

        [Test]
        public void ForceOff_TakesEveryBlockOff_ForARun()
        {
            SurvivalSim sim = Sim(Cfg());
            bool was = N21Blocks.ForceOff;
            try
            {
                N21Blocks.ForceOff = true;
                sim.StartRun(1, null, null);
                Assert.That(sim.n21.Any, Is.False);
            }
            finally { N21Blocks.ForceOff = was; }
            sim.n21Override = N21Blocks.FromBits(0b101001);
            sim.StartRun(1, null, null);
            Assert.That(sim.n21.Bits, Is.EqualTo(0b101001), "an override (a tape's) wins");
        }

        [Test]
        public void TheTape_Format6_CarriesTheN21Byte()
        {
            var t = new RunTape { n19 = 0b1010101, n20 = 0b11001010, n21 = 0b101101 };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion), "NIGHT22: the current format is 7; format 6's n21 byte is read below");
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n21, Is.EqualTo(0b101101));
            Assert.That(back.n20, Is.EqualTo(0b11001010));
            Assert.That(back.n19, Is.EqualTo(0b1010101));
            RunTape old = RunTape.FromBytes(AsFormat5(b));
            Assert.That(old.n21, Is.EqualTo(0), "format 5: every NIGHT21 block off");
            Assert.That(old.n20, Is.EqualTo(0b11001010), "format 5 keeps its NIGHT20 byte");
        }

        [Test]
        public void AFormat5Tape_RecordedOnNight20sGame_ReplaysEqual()
        {
            SurvivalSim sim = Sim(Cfg());
            sim.n21Override = N21Blocks.Off; // as a NIGHT20 build recorded it
            RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "tests", null, TapeScript.God);
            RunTape old = RunTape.FromBytes(AsFormat5(tape.ToBytes()));
            Assert.That(old.n21, Is.EqualTo(0));
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), old);
            Assert.That(r.Equal, Is.True, r.Line);
        }

        [Test]
        public void NIGHT24_RemoveReplay_ATapeRecordedWithTheOldReplayBitOn_StillReadsAndReplaysEqual_AndANewOneToo()
        {
            // INSTANT REPLAY is gone (NIGHT24 remove-replay) but bit 5 of the NIGHT21 byte stays in the tape: a tape recorded before the
            // removal (the default config had the block on, so the bit was set) reads, round-trips its bits and replays EQUAL.
            SurvivalSim sim = Sim(Cfg());
            sim.n21Override = N21Blocks.FromBits(0b100000);
            RunTape before = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "tests", null, TapeScript.God);
            RunTape back = RunTape.FromBytes(before.ToBytes());
            Assert.That(back.n21 & 0b100000, Is.EqualTo(0b100000), "the retired bit survives the round trip");
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), back);
            Assert.That(r.Equal, Is.True, r.Line);
            SurvivalSim fresh = Sim(Cfg()); // a tape recorded after the removal: the blocks as the config gives them (bit 5 never set)
            RunTape after = TapeScript.Record(fresh, SimFingerprint.Seed, 1800, "tests", null, TapeScript.God);
            Assert.That(after.n21 & 0b100000, Is.EqualTo(0), "no config row sets the retired bit");
            Assert.That(TapeReplay.Replay(Cfg(), Map(), RunTape.FromBytes(after.ToBytes())).Equal, Is.True);
        }

        [Test]
        public void TheFingerprint_OnPathIsN21_AndTheOffPathIsNight20()
        {
            // NIGHT21's close (FingerprintBaseline.N21OnPath true) re-recorded the on-baseline with the blocks as configured
            // (FingerprintBaselineN21 in FingerprintBaseline.cs); the NIGHT21 off path still prints NIGHT20's lines.
            Assert.That(FingerprintBaseline.N21OnPath, Is.True, "the close set it");
            SurvivalSim sim = Sim(Cfg());
            var on = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string onSummary);
            Assert.That(FingerprintBaseline.FirstDifference(on), Is.EqualTo(-1), onSummary);
            sim.n21Override = N21Blocks.Off;
            var lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            Assert.That(FingerprintBaselineN20.FirstDifference(lines), Is.EqualTo(-1), summary);
            Assert.That(sim.n21Override.Value.Bits, Is.EqualTo(0), "the override handed back");
        }

        /// <summary>A format 6 tape's bytes as a format 5 build wrote them: the version 5, no NIGHT21 byte (after the NIGHT20 byte).</summary>
        public static byte[] AsFormat5(byte[] b6)
        {
            b6 = N22SharedTests.AsFormat6(b6); // NIGHT22: the current format is 7; a format 6 tape passes through
            var r = new TapeReader(b6, RunTape.Magic.Length, b6.Length);
            r.U32(); // the version
            r.Str(); // the build stamp
            r.Str(); // the baseline id
            r.U32(); // the seed
            r.Str(); // the mode
            r.VarI(); // the HEAT rung
            r.U64(); // the clock
            r.Byte(); // the flags
            r.Byte(); // the LAB toggles
            r.Byte(); // the move flags (format 2)
            r.Byte(); // the stamina flags (format 3)
            r.Byte(); // the NIGHT19 blocks (format 4)
            r.Byte(); // the NIGHT20 blocks (format 5)
            int at = r.Position; // the NIGHT21 blocks (format 6)
            var b5 = new byte[b6.Length - 1];
            Buffer.BlockCopy(b6, 0, b5, 0, at);
            Buffer.BlockCopy(b6, at + 1, b5, at, b6.Length - at - 1);
            var w = new TapeWriter(8);
            w.U32(5);
            Buffer.BlockCopy(w.data, 0, b5, RunTape.Magic.Length, 4);
            return b5;
        }
    }
}
