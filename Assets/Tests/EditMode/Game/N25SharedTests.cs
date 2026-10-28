// NIGHT25 (the shared commit, the lead): the contract every builder compiles against.
//   1. the six NIGHT25 blocks: the bits round trip, every block off is NIGHT24's game, each block present in the design config
//      (design.n25.<row>) with an `enabled` and a `_why`, all off in the shared commit; the Player's copy stays byte-identical
//   2. --off25 / -skygrindOff25 (N25Blocks.ForceOff) takes every block off for a run whatever the config says
//   3. THE TAPE's format 9 carries the NIGHT25 byte; a format 8 tape is read with every block off and replays EQUAL
//   4. the composed fingerprint runs with every NIGHT25 block off unless the freeze sets FingerprintBaseline.N25OnPath
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N25SharedTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static readonly string[] Rows = { "meleeFire", "pulseCharge", "shockwaveOff", "wyrmAim", "tutorial", "curriculum" };

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static SurvivalSim Sim(GameConfig cfg)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noRecords = sim.noLab = true;
            return sim;
        }

        [Test]
        public void N25Blocks_Bits_RoundTrip_AndOffIsNight24()
        {
            for (int bits = 0; bits < 1 << N25Blocks.Count; bits++)
                Assert.That(N25Blocks.FromBits(bits).Bits, Is.EqualTo(bits));
            Assert.That(N25Blocks.Off.Any, Is.False);
        }

        [Test]
        public void EveryN25Block_IsInTheDesignConfig_WithItsWhy()
        {
            GameConfig cfg = Cfg();
            string design = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Design", "unity-config.json"));
            GameConfig full = GameConfig.Load(Path.Combine(Root, "config.json"), design);
            Assert.That(full.Has("design.n25._why"), Is.True, "n25._why");
            foreach (string b in Rows)
            {
                Assert.That(full.Has("design.n25." + b + ".enabled"), Is.True, b + ".enabled");
                Assert.That(full.Has("design.n25." + b + "._why"), Is.True, b + "._why");
            }
        }

        [Test]
        public void ForceOff_TakesEveryBlockOff_ForARun()
        {
            SurvivalSim sim = Sim(Cfg());
            bool was = N25Blocks.ForceOff;
            try
            {
                N25Blocks.ForceOff = true;
                sim.StartRun(1, null, null);
                Assert.That(sim.n25.Any, Is.False);
            }
            finally { N25Blocks.ForceOff = was; }
            sim.n25Override = N25Blocks.FromBits(0b101001);
            sim.StartRun(1, null, null);
            Assert.That(sim.n25.Bits, Is.EqualTo(0b101001), "an override (a tape's) wins");
        }

        [Test]
        public void TheTape_Format9_CarriesTheN25Byte()
        {
            var t = new RunTape { n24 = 0b101100101, n25 = 0b101011, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion), "NIGHT26 premise: the current format is 10; format 9 keeps these fields");
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n25, Is.EqualTo(0b101011));
            Assert.That(back.n24, Is.EqualTo(0b101100101));
            RunTape old = RunTape.FromBytes(AsFormat8(b));
            Assert.That(old.n25, Is.EqualTo(0), "format 8: every NIGHT25 block off");
            Assert.That(old.n24, Is.EqualTo(0b101100101), "format 8 keeps its NIGHT24 value");
        }

        [Test]
        public void AFormat8Tape_RecordedOnNight24sGame_ReplaysEqual()
        {
            SurvivalSim sim = Sim(Cfg());
            sim.n25Override = N25Blocks.Off; // as a NIGHT24 build recorded it
            RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "tests", null, TapeScript.God);
            RunTape old = RunTape.FromBytes(AsFormat8(tape.ToBytes()));
            Assert.That(old.n25, Is.EqualTo(0));
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), old);
            Assert.That(r.Equal, Is.True, r.Line);
        }

        [Test]
        public void TheFingerprint_N25IsOffThePath_AndTheOffPathIsNight24()
        {
            Assert.That(FingerprintBaseline.N25OnPath, Is.False, "the freeze sets it only if a sim-changing NIGHT25 default is on");
            SurvivalSim sim = Sim(Cfg());
            var on = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string onSummary);
            Assert.That(FingerprintBaseline.FirstDifference(on), Is.EqualTo(-1), onSummary);
            sim.n25Override = N25Blocks.Off;
            var lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            Assert.That(FingerprintBaseline.FirstDifference(lines), Is.EqualTo(-1), summary);
            Assert.That(sim.n25Override.Value.Bits, Is.EqualTo(0), "the override handed back");
        }

        /// <summary>A format 9 tape's bytes as a format 8 build wrote them: the version 8, no NIGHT25 byte (after the campaign state).
        /// A tape that is already format 8 or older passes through.</summary>
        public static byte[] AsFormat8(byte[] b9)
        {
            b9 = N26SharedTests.AsFormat9(b9); // NIGHT26: the current format is 10; its format 9 form first
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
    }
}
