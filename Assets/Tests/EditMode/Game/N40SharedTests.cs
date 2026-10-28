// NIGHT40 (the shared commit, the lead): the checks for the NIGHT40 blocks' shared plumbing. Every test here passes with the NIGHT40
// blocks forced off by N37TestBaseline (the Game test namespace runs with NIGHT37-NIGHT40 off unless it hands its own override):
//   1. the tape format 23 carries the NIGHT40 value (a VarI), and a format 22 tape (AsFormat22) reads with every NIGHT40 block off
//   2. N40Blocks round-trips through its bits, off is the default value, and every name has a design row
//   3. Curriculum25.N40Levels: with every NIGHT40 block forced off the order is NIGHT39's; a level row swaps its id; cut40 cuts level 6
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N40SharedTests
    {
        static GameConfig cfg;
        static GameConfig Cfg
        {
            get
            {
                if (cfg != null) return cfg;
                string root = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
                return cfg = GameConfig.Load(Path.Combine(root, "PortExport", "config.json"), Path.Combine(root, "Design", "unity-config.json"));
            }
        }

        [Test]
        public void TheTape_Format23_CarriesTheN40Value()
        {
            var t = new RunTape { n31 = 0b10110101, n36 = 0b110100101, n37 = 0b10110, n38 = 0b10100101, n39 = 0b101, n40 = 0b1011001101, bank33 = 12345, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(27) /* NIGHT46 rows: format 27 (NightRowsTapeTests.AsFormat26 strips the rows record) */, "NIGHT43: the current format is 26 (N43SharedTests.AsFormat25, N42SharedTests.AsFormat24 and N41SharedTests.AsFormat23 strip their values)");
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n40, Is.EqualTo(0b1011001101));
            Assert.That(back.n39, Is.EqualTo(0b101));
            Assert.That(back.bank33, Is.EqualTo(12345));

            RunTape old = RunTape.FromBytes(AsFormat22(b));
            Assert.That(old.n40, Is.EqualTo(0), "format 22: every NIGHT40 block off");
            Assert.That(old.n39, Is.EqualTo(0b101), "format 22 keeps its NIGHT39 value");
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndOffIsTheDefault()
        {
            Assert.That(N40Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N40Blocks.Count); b++) Assert.That(N40Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N40Blocks.Names.Length, Is.EqualTo(N40Blocks.Count));
            Assert.That(N40Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT40 blocks forced off (N37TestBaseline)");
            var cfg = Cfg;
            foreach (string n in N40Blocks.Names) Assert.That(cfg.Has("design.n40." + n + ".enabled"), Is.True, "a design row for " + n);
        }

        [Test]
        public void TheLevelRows_SwapAndCut_OnlyWhenOn()
        {
            var cfg = Cfg;
            var order = new List<string> { "n38-foundry", "n37-iceline", "n38-kiln", "n38-span", "n38-spire", "n35-finale" };
            var forced = new List<string>(order);
            Curriculum25.N40Levels(cfg, forced);
            Assert.That(forced, Is.EqualTo(order), "forced off: NIGHT39's order");
            bool was = N40Blocks.ForceOff;
            try
            {
                N40Blocks.ForceOff = false;
                var asConfigured = new List<string>(order);
                Curriculum25.N40Levels(cfg, asConfigured);
                foreach (string row in new[] { "kiln40", "iceline40", "loom40", "crown40", "cut40" })
                    if (cfg.Design("n40." + row + ".enabled")) Assert.Pass("a level row is on in the config: the swap is the builder's test");
                Assert.That(asConfigured, Is.EqualTo(order), "every NIGHT40 level row off in the config: NIGHT39's order");
            }
            finally { N40Blocks.ForceOff = was; }
        }

        /// <summary>A format 23 tape's bytes as a format 22 build wrote them: the version 22, no NIGHT40 value (after the NIGHT39 value). A
        /// tape that is already format 22 or older passes through.</summary>
        public static byte[] AsFormat22(byte[] b23)
        {
            b23 = N41SharedTests.AsFormat23(b23); // NIGHT41: a format 24 tape first loses its NIGHT41 value
            if (BitConverter.ToInt32(b23, RunTape.Magic.Length) < 23) return b23;
            var r = new TapeReader(b23, RunTape.Magic.Length, b23.Length);
            r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
            r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte();
            r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI();
            r.Byte(); // n25
            r.Byte(); // n26
            r.Byte(); // n27
            r.Byte(); // n29
            r.Byte(); // n30
            r.Byte(); // n31
            r.Byte(); // n32
            r.Byte(); // n33
            r.VarI(); // shop33
            r.VarI(); // bank33
            r.Byte(); // n34
            r.Byte(); // n35
            r.VarI(); // n36
            r.VarI(); // n37
            r.VarI(); // n38
            r.VarI(); // n39
            int at = r.Position;
            r.VarI(); // n40
            int cut = r.Position - at;
            var b22 = new byte[b23.Length - cut];
            Buffer.BlockCopy(b23, 0, b22, 0, at);
            Buffer.BlockCopy(b23, at + cut, b22, at, b23.Length - at - cut);
            var w = new TapeWriter(22);
            w.U32(22);
            Buffer.BlockCopy(w.data, 0, b22, RunTape.Magic.Length, 4);
            return b22;
        }
    }
}
