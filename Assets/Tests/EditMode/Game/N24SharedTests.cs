// NIGHT24 (the shared commit, the lead): the contract every builder compiles against.
//   1. the nine NIGHT24 blocks: the bits round trip, every block off is NIGHT22's game, each block present in the design
//      config (design.n24.<row>) with an `enabled` and a `_why`, all off in the shared commit; the Player's copy stays
//      byte-identical to the Design copy
//   2. --off24 / -skygrindOff24 (N24Blocks.ForceOff) takes every block off for a run whatever the config says
//   3. THE TAPE's format 8 carries the NIGHT24 value (two bytes) and the campaign state; a format 7 tape is read with every block
//      off and no campaign, and replays EQUAL
//   4. the composed fingerprint runs with every NIGHT24 block off unless the freeze sets FingerprintBaseline.N24OnPath
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N24SharedTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static readonly string[] Rows = { "padY", "shotgunTune", "bigPick", "chargeFist", "reticleArc", "campaign", "wyrm", "lighting", "prewarm" };

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static SurvivalSim Sim(GameConfig cfg)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noRecords = sim.noLab = true;
            return sim;
        }

        [Test]
        public void N24Blocks_Bits_RoundTrip_AndOffIsNight22()
        {
            for (int bits = 0; bits < 1 << N24Blocks.Count; bits++)
                Assert.That(N24Blocks.FromBits(bits).Bits, Is.EqualTo(bits));
            Assert.That(N24Blocks.Off.Any, Is.False);
            Assert.That(N24Blocks.Off.Bits, Is.EqualTo(0));
            Assert.That(N24Blocks.Names.Length, Is.EqualTo(N24Blocks.Count));
            Assert.That(N24Blocks.Count, Is.EqualTo(Rows.Length));
        }

        [Test]
        public void EveryN24Block_IsInTheDesignConfig_WithItsWhy_AndOff()
        {
            GameConfig cfg = Cfg();
            Assert.That(cfg.Has("design.n24._why"), Is.True, "n24._why");
            foreach (string b in Rows)
            {
                Assert.That(cfg.Has("design.n24." + b + ".enabled"), Is.True, b + ".enabled");
                Assert.That(cfg.Has("design.n24." + b + "._why"), Is.True, b + "._why");
            }
            // NIGHT24 premise change: the shared commit had every row off; a row is turned on when its contract passes
            // (padY, shotgunTune, bigPick, chargeFist, reticleArc, campaign, wyrm on at C1), so the test now reads the flags
            // back through the bits instead of asserting that all are off.
            N24Blocks from = N24Blocks.From(cfg);
            Assert.That(N24Blocks.FromBits(from.Bits).Bits, Is.EqualTo(from.Bits), "the config's flags round trip through the bits");
        }

        [Test]
        public void TheTwoConfigCopies_AreByteIdentical()
        {
            string design = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Design", "unity-config.json"));
            string player = Path.Combine(Application.dataPath, "StreamingAssets", "Design", "unity-config.json");
            Assert.That(File.ReadAllBytes(player), Is.EqualTo(File.ReadAllBytes(design)), "StreamingAssets copy of the design config");
        }

        [Test]
        public void ForceOff_TakesEveryBlockOff_ForARun()
        {
            SurvivalSim sim = Sim(Cfg());
            bool was = N24Blocks.ForceOff;
            try
            {
                N24Blocks.ForceOff = true;
                sim.StartRun(1, null, null);
                Assert.That(sim.n24.Any, Is.False);
            }
            finally { N24Blocks.ForceOff = was; }
            sim.n24Override = N24Blocks.FromBits(0b101001);
            sim.StartRun(1, null, null);
            Assert.That(sim.n24.Bits, Is.EqualTo(0b101001), "an override (a tape's) wins");
        }

        [Test]
        public void TheTape_Format8_CarriesTheN24Value_AndTheCampaignState()
        {
            var camp = new Campaign24State { levelId = "layer1", room = 3, checkpoint = 2, limits = 5 };
            var t = new RunTape { n22 = 0b110101, n24 = 0b101100101, campaign = camp, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion), "NIGHT25 premise: the current format is 9; format 8 keeps these fields");
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n24, Is.EqualTo(0b101100101), "bit 8 (prewarm) needs the second byte");
            Assert.That(back.n22, Is.EqualTo(0b110101));
            Assert.That(back.campaign.Same(camp), Is.True, "level, room, checkpoint, limits");
            Assert.That(back.mode, Is.EqualTo("campaign"));
            RunTape old = RunTape.FromBytes(AsFormat7(b));
            Assert.That(old.n24, Is.EqualTo(0), "format 7: every NIGHT24 block off");
            Assert.That(old.campaign.Any, Is.False, "format 7: no campaign");
            Assert.That(old.n22, Is.EqualTo(0b110101), "format 7 keeps its NIGHT22 byte");
        }

        [Test]
        public void AFormat7Tape_RecordedOnNight22sGame_ReplaysEqual()
        {
            SurvivalSim sim = Sim(Cfg());
            sim.n24Override = N24Blocks.Off; // as a NIGHT22 build recorded it
            RunTape tape = TapeScript.Record(sim, SimFingerprint.Seed, 1800, "tests", null, TapeScript.God);
            RunTape old = RunTape.FromBytes(AsFormat7(tape.ToBytes()));
            Assert.That(old.n24, Is.EqualTo(0));
            TapeResult r = TapeReplay.Replay(Cfg(), Map(), old);
            Assert.That(r.Equal, Is.True, r.Line);
        }

        [Test]
        public void TheFingerprint_N24IsOffThePath_AndTheOffPathIsNight22()
        {
            Assert.That(FingerprintBaseline.N24OnPath, Is.False, "the freeze sets it only if a sim-changing NIGHT24 default is on");
            SurvivalSim sim = Sim(Cfg());
            var on = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string onSummary);
            Assert.That(FingerprintBaseline.FirstDifference(on), Is.EqualTo(-1), onSummary);
            sim.n24Override = N24Blocks.Off;
            var lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            Assert.That(FingerprintBaseline.FirstDifference(lines), Is.EqualTo(-1), summary);
            Assert.That(sim.n24Override.Value.Bits, Is.EqualTo(0), "the override handed back");
        }

        /// <summary>A format 8 tape's bytes as a format 7 build wrote them: the version 7, no NIGHT24 value and no campaign state (after the NIGHT22 byte).
        /// A tape that is already format 7 or older passes through.</summary>
        public static byte[] AsFormat7(byte[] b8)
        {
            b8 = N25SharedTests.AsFormat8(b8); // NIGHT25: the current format is 9; its format 8 form first
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
    }
}
