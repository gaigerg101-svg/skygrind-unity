// NIGHT20 (the shared commit, the lead): the contract every builder compiles against.
//   1. the eight NIGHT20 blocks: the bits round trip, every block off is NIGHT19's game, each block present in the design
//      config with an `enabled` and a `_why`; the Player's copy stays byte-identical (N19SharedTests checks the bytes)
//   2. --off20 / -skygrindOff20 (N20Blocks.ForceOff) takes every block off for a run whatever the config says
//   3. THE TAPE's format 5 carries the NIGHT20 byte; a format 4 tape is read with every block off and replays EQUAL
//      (recorded on NIGHT19's game)
//   4. the composed fingerprint runs with every NIGHT19 / NIGHT20 block off until the close (FingerprintBaseline.N19OnPath)
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N20SharedTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static SurvivalSim Sim(GameConfig cfg)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noRecords = sim.noLab = true;
            return sim;
        }

        [Test]
        public void N20Blocks_Bits_RoundTrip_AndOffIsNight19()
        {
            for (int bits = 0; bits < 1 << N20Blocks.Count; bits++)
                Assert.That(N20Blocks.FromBits(bits).Bits, Is.EqualTo(bits));
            Assert.That(N20Blocks.Off.Any, Is.False);
            Assert.That(N20Blocks.Off.Bits, Is.EqualTo(0));
            Assert.That(N20Blocks.Names.Length, Is.EqualTo(N20Blocks.Count));
        }

        [Test]
        public void EveryN20Block_IsInTheDesignConfig_WithItsWhy()
        {
            GameConfig cfg = Cfg();
            foreach (string b in new[] { "shockwave", "zoom", "regen.n20", "routes", "director.n20", "arsenal.n20", "pad.n20", "adds" })
            {
                Assert.That(cfg.Has("design." + b + ".enabled"), Is.True, b + ".enabled");
                Assert.That(cfg.Has("design." + b + "._why"), Is.True, b + "._why");
            }
        }

        [Test]
        public void ForceOff_TakesEveryBlockOff_ForARun()
        {
            SurvivalSim sim = Sim(Cfg());
            bool was = N20Blocks.ForceOff;
            try
            {
                N20Blocks.ForceOff = true;
                sim.StartRun(1, null, null);
                Assert.That(sim.n20.Any, Is.False);
                Assert.That(sim.loadout.n20.Any || sim.director.n20.Any, Is.False, "handed to the loadout and the director");
            }
            finally { N20Blocks.ForceOff = was; }
            sim.n20Override = N20Blocks.FromBits(0b10100101);
            sim.StartRun(1, null, null);
            Assert.That(sim.n20.Bits, Is.EqualTo(0b10100101), "an override (a tape's) wins");
            Assert.That(sim.loadout.n20.Bits, Is.EqualTo(0b10100101));
        }

        [Test]
        public void TheTape_Format5_CarriesTheN20Byte()
        {
            var t = new RunTape { n19 = 0b1010101, n20 = 0b11001010 };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT21: format 6 keeps format 5's byte
            b = N21SharedTests.AsFormat5(b); // a format 5 build's bytes: the NIGHT20 byte is the last block
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n20, Is.EqualTo(0b11001010));
            Assert.That(back.n19, Is.EqualTo(0b1010101));
            RunTape old = RunTape.FromBytes(AsFormat4(b));
            Assert.That(old.n20, Is.EqualTo(0), "format 4: every NIGHT20 block off");
            Assert.That(old.n19, Is.EqualTo(0b1010101), "format 4 keeps its NIGHT19 byte");
        }

        [Test]
        public void AFormat4Tape_RecordedOnNight19sGame_ReplaysEqual()
        {
            SurvivalSim sim = Sim(Cfg());
            sim.n20Override = N20Blocks.Off; // as a NIGHT19 build recorded it
            sim.n21Override = N21Blocks.Off; // NIGHT21: and no NIGHT21 block (a format 4 tape reads them off)
            RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "tests", null, TapeScript.God);
            RunTape old = RunTape.FromBytes(AsFormat4(N21SharedTests.AsFormat5(tape.ToBytes())));
            Assert.That(old.n20, Is.EqualTo(0));
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), old);
            Assert.That(r.Equal, Is.True, r.Line);
        }

        [Test]
        public void TheFingerprint_RunsEveryN20BlockOff_UntilTheClose()
        {
            // NIGHT20's close re-recorded the on-baseline (FingerprintBaseline, N19OnPath true): the run as configured prints
            // it, and the off path (both nights' blocks off) still prints NIGHT18's lines.
            Assert.That(FingerprintBaseline.N19OnPath, Is.True, "NIGHT20's close re-recorded the on-baseline");
            SurvivalSim sim = Sim(Cfg());
            var on = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string onSummary);
            Assert.That(FingerprintBaseline.FirstDifference(on), Is.EqualTo(-1), onSummary);
            sim.n19Override = N19Blocks.Off;
            sim.n20Override = N20Blocks.Off;
            sim.n21Override = N21Blocks.Off;
            var lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            Assert.That(FingerprintBaselineN18.FirstDifference(lines), Is.EqualTo(-1), summary);
            Assert.That(sim.n20Override.Value.Bits, Is.EqualTo(0), "the override handed back");
        }

        /// <summary>A format 5 tape's bytes as a format 4 build wrote them: the version 4, no NIGHT20 byte (after the NIGHT19 byte).</summary>
        public static byte[] AsFormat4(byte[] b5)
        {
            var r = new TapeReader(b5, RunTape.Magic.Length, b5.Length);
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
            int at = r.Position; // the NIGHT20 blocks (format 5)
            var b4 = new byte[b5.Length - 1];
            Buffer.BlockCopy(b5, 0, b4, 0, at);
            Buffer.BlockCopy(b5, at + 1, b4, at, b5.Length - at - 1);
            var w = new TapeWriter(8);
            w.U32(4);
            Buffer.BlockCopy(w.data, 0, b4, RunTape.Magic.Length, 4);
            return b4;
        }
    }
}
