// NIGHT26 (the shared commit, the lead): the contract every builder compiles against.
//   1. the six NIGHT26 blocks: the bits round trip, every block off is NIGHT25's game, each block present in the design config
//      (design.n26.<row>) with an `enabled` and a `_why`, all off in the shared commit; the Player's copy stays byte-identical
//   2. --off25 / -skygrindOff26 (N26Blocks.ForceOff) takes every block off for a run whatever the config says
//   3. THE TAPE's format 9 carries the NIGHT26 byte; a format 9 tape is read with every block off and replays EQUAL
//   4. the composed fingerprint runs with every NIGHT26 block off unless the freeze sets FingerprintBaseline.N26OnPath
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N26SharedTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static readonly string[] Rows = { "pulseLT", "weaponIdentity", "surfaces", "levelSelect", "level1", "level2", "level3" };

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static SurvivalSim Sim(GameConfig cfg)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noRecords = sim.noLab = true;
            return sim;
        }

        [Test]
        public void N26Blocks_Bits_RoundTrip_AndOffIsNight25()
        {
            for (int bits = 0; bits < 1 << N26Blocks.Count; bits++)
                Assert.That(N26Blocks.FromBits(bits).Bits, Is.EqualTo(bits));
            Assert.That(N26Blocks.Off.Any, Is.False);
        }

        [Test]
        public void EveryN26Block_IsInTheDesignConfig_WithItsWhy()
        {
            GameConfig cfg = Cfg();
            string design = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Design", "unity-config.json"));
            GameConfig full = GameConfig.Load(Path.Combine(Root, "config.json"), design);
            Assert.That(full.Has("design.n26._why"), Is.True, "n26._why");
            foreach (string b in Rows)
            {
                Assert.That(full.Has("design.n26." + b + ".enabled"), Is.True, b + ".enabled");
                Assert.That(full.Has("design.n26." + b + "._why"), Is.True, b + "._why");
            }
        }

        [Test]
        public void ForceOff_TakesEveryBlockOff_ForARun()
        {
            SurvivalSim sim = Sim(Cfg());
            bool was = N26Blocks.ForceOff;
            try
            {
                N26Blocks.ForceOff = true;
                sim.StartRun(1, null, null);
                Assert.That(sim.n26.Any, Is.False);
            }
            finally { N26Blocks.ForceOff = was; }
            sim.n26Override = N26Blocks.FromBits(0b1010011);
            sim.StartRun(1, null, null);
            Assert.That(sim.n26.Bits, Is.EqualTo(0b1010011), "an override (a tape's) wins");
        }

        [Test]
        public void TheTape_Format10_CarriesTheN26Byte()
        {
            var t = new RunTape { n24 = 0b101100101, n25 = 0b101011, n26 = 0b1010011, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion), "NIGHT27 made the current format 11, NIGHT29 12 (it still carries the NIGHT26 byte)");
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n26, Is.EqualTo(0b1010011));
            Assert.That(back.n25, Is.EqualTo(0b101011));
            Assert.That(back.n24, Is.EqualTo(0b101100101));
            RunTape old = RunTape.FromBytes(AsFormat9(b));
            Assert.That(old.n26, Is.EqualTo(0), "format 9: every NIGHT26 block off");
            Assert.That(old.n25, Is.EqualTo(0b101011), "format 9 keeps its NIGHT25 value");
        }

        [Test]
        public void AFormat9Tape_RecordedOnNight25sGame_ReplaysEqual()
        {
            SurvivalSim sim = Sim(Cfg());
            sim.n26Override = N26Blocks.Off; // as a NIGHT25 build recorded it
            RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "tests", null, TapeScript.God);
            RunTape old = RunTape.FromBytes(AsFormat9(tape.ToBytes()));
            Assert.That(old.n26, Is.EqualTo(0));
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), old);
            Assert.That(r.Equal, Is.True, r.Line);
        }

        [Test]
        public void TheFingerprint_N26IsOffThePath_AndTheOffPathIsNight25()
        {
            Assert.That(FingerprintBaseline.N26OnPath, Is.False, "the freeze sets it only if a sim-changing NIGHT26 default is on");
            SurvivalSim sim = Sim(Cfg());
            var on = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string onSummary);
            Assert.That(FingerprintBaseline.FirstDifference(on), Is.EqualTo(-1), onSummary);
            sim.n26Override = N26Blocks.Off;
            var lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            Assert.That(FingerprintBaseline.FirstDifference(lines), Is.EqualTo(-1), summary);
            Assert.That(sim.n26Override.Value.Bits, Is.EqualTo(0), "the override handed back");
        }

        /// <summary>NIGHT27: a format 11 tape's bytes as a format 10 build wrote them: the version 10, no NIGHT27 byte (right after the NIGHT26 byte).
        /// A tape that is already format 10 or older passes through.</summary>
        public static byte[] AsFormat10(byte[] b11)
        {
            b11 = N29SharedTests.AsFormat11(b11); // NIGHT29: a format 12 tape first loses its NIGHT29 byte
            if (BitConverter.ToInt32(b11, RunTape.Magic.Length) < 11) return b11;
            var r = new TapeReader(b11, RunTape.Magic.Length, b11.Length);
            r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
            r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte();
            r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI();
            r.Byte(); // n25
            r.Byte(); // n26
            int at = r.Position;
            r.Byte(); // n27
            var b10 = new byte[b11.Length - 1];
            Buffer.BlockCopy(b11, 0, b10, 0, at);
            Buffer.BlockCopy(b11, at + 1, b10, at, b11.Length - at - 1);
            var w = new TapeWriter(10);
            w.U32(10);
            Buffer.BlockCopy(w.data, 0, b10, RunTape.Magic.Length, 4);
            return b10;
        }

        /// <summary>A format 10 tape's bytes as a format 9 build wrote them: the version 9, no NIGHT26 byte (after the campaign state).
        /// A tape that is already format 9 or older passes through.</summary>
        public static byte[] AsFormat9(byte[] b9)
        {
            b9 = AsFormat10(b9); // NIGHT27: a format 11 tape first loses its NIGHT27 byte
            if (BitConverter.ToInt32(b9, RunTape.Magic.Length) < 10) return b9;
            var r = new TapeReader(b9, RunTape.Magic.Length, b9.Length);
            r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
            r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); // flags, LAB, move, stamina, n19, n20, n21, n22
            r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI(); // n24 (two bytes), the campaign state
            r.Byte(); // n25
            int at = r.Position;
            r.Byte(); // n26
            var b8 = new byte[b9.Length - 1];
            Buffer.BlockCopy(b9, 0, b8, 0, at);
            Buffer.BlockCopy(b9, at + 1, b8, at, b9.Length - at - 1);
            var w = new TapeWriter(9);
            w.U32(9);
            Buffer.BlockCopy(w.data, 0, b8, RunTape.Magic.Length, 4);
            return b8;
        }
    }
}
