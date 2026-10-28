// NIGHT22 (the shared commit, the lead): the contract every builder compiles against.
//   1. the six NIGHT22 blocks: the bits round trip, every block off is NIGHT21's game, each block present in the design
//      config (design.n22.<row>) with an `enabled` and a `_why`, all off in the shared commit; the Player's copy stays
//      byte-identical to the Design copy
//   2. --off22 / -skygrindOff22 (N22Blocks.ForceOff) takes every block off for a run whatever the config says
//   3. THE TAPE's format 7 carries the NIGHT22 byte and the run mode; a format 6 tape is read with every block off and replays EQUAL
//   4. the composed fingerprint runs with every NIGHT22 block off unless the close sets FingerprintBaseline.N22OnPath
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N22SharedTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static readonly string[] Rows = { "duskClock", "haptics", "directed", "trickChain", "overdrive" };

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static SurvivalSim Sim(GameConfig cfg)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noRecords = sim.noLab = true;
            return sim;
        }

        [Test]
        public void N22Blocks_Bits_RoundTrip_AndOffIsNight21()
        {
            for (int bits = 0; bits < 1 << N22Blocks.Count; bits++)
                Assert.That(N22Blocks.FromBits(bits).Bits, Is.EqualTo(bits));
            Assert.That(N22Blocks.Off.Any, Is.False);
            Assert.That(N22Blocks.Off.Bits, Is.EqualTo(0));
            Assert.That(N22Blocks.Names.Length, Is.EqualTo(N22Blocks.Count));
            Assert.That(N22Blocks.Count, Is.EqualTo(Rows.Length + 1), "five rows plus the retired bit 0 (the removed DAILY ghost)");
        }

        [Test]
        public void EveryN22Block_IsInTheDesignConfig_WithItsWhy()
        {
            GameConfig cfg = Cfg();
            Assert.That(cfg.Has("design.n22._why"), Is.True, "n22._why");
            foreach (string b in Rows)
            {
                Assert.That(cfg.Has("design.n22." + b + ".enabled"), Is.True, b + ".enabled");
                Assert.That(cfg.Has("design.n22." + b + "._why"), Is.True, b + "._why");
            }
        }

        [Test]
        public void ForceOff_TakesEveryBlockOff_ForARun()
        {
            SurvivalSim sim = Sim(Cfg());
            bool was = N22Blocks.ForceOff;
            try
            {
                N22Blocks.ForceOff = true;
                sim.StartRun(1, null, null);
                Assert.That(sim.n22.Any, Is.False);
            }
            finally { N22Blocks.ForceOff = was; }
            sim.n22Override = N22Blocks.FromBits(0b101001);
            sim.StartRun(1, null, null);
            Assert.That(sim.n22.Bits, Is.EqualTo(0b101001), "an override (a tape's) wins");
        }

        [Test]
        public void TheTape_Format7_CarriesTheN22Byte_AndTheMode()
        {
            var t = new RunTape { n20 = 0b11001010, n21 = 0b101101, n22 = 0b110101, mode = RunSetup.Grind };
            byte[] b = t.ToBytes();
            b = N24SharedTests.AsFormat7(b); // NIGHT24: the current format is 8; its format 7 form carries the same NIGHT22 byte
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(7));
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n22, Is.EqualTo(0b110101));
            Assert.That(back.n21, Is.EqualTo(0b101101));
            Assert.That(back.mode, Is.EqualTo(RunSetup.Grind), "a GRIND tape keeps its mode");
            RunTape old = RunTape.FromBytes(AsFormat6(b));
            Assert.That(old.n22, Is.EqualTo(0), "format 6: every NIGHT22 block off");
            Assert.That(old.n21, Is.EqualTo(0b101101), "format 6 keeps its NIGHT21 byte");
        }

        [Test]
        public void AFormat6Tape_RecordedOnNight21sGame_ReplaysEqual()
        {
            SurvivalSim sim = Sim(Cfg());
            sim.n22Override = N22Blocks.Off; // as a NIGHT21 build recorded it
            RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "tests", null, TapeScript.God);
            RunTape old = RunTape.FromBytes(AsFormat6(tape.ToBytes()));
            Assert.That(old.n22, Is.EqualTo(0));
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), old);
            Assert.That(r.Equal, Is.True, r.Line);
        }

        [Test]
        public void TheFingerprint_N22IsOffThePath_AndTheOffPathIsNight21()
        {
            Assert.That(FingerprintBaseline.N22OnPath, Is.False, "the close sets it only if a sim-changing NIGHT22 default is on");
            SurvivalSim sim = Sim(Cfg());
            var on = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string onSummary);
            Assert.That(FingerprintBaseline.FirstDifference(on), Is.EqualTo(-1), onSummary);
            sim.n22Override = N22Blocks.Off;
            var lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            Assert.That(FingerprintBaseline.FirstDifference(lines), Is.EqualTo(-1), summary);
            Assert.That(sim.n22Override.Value.Bits, Is.EqualTo(0), "the override handed back");
        }

        /// <summary>A format 7 tape's bytes as a format 6 build wrote them: the version 6, no NIGHT22 byte (after the NIGHT21 byte).</summary>
        public static byte[] AsFormat6(byte[] b7)
        {
            b7 = N24SharedTests.AsFormat7(b7); // NIGHT24: the current format is 8; a format 7 tape passes through
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
    }
}
