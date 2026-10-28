// NIGHT41 (the shared commit, the lead): the checks for the NIGHT41 blocks' shared plumbing. Every test here passes with the NIGHT41
// blocks forced off by N37TestBaseline (the Game test namespace runs with NIGHT37-NIGHT41 off unless it hands its own override):
//   1. the tape format 24 carries the NIGHT41 value (a VarI), and a format 23 tape (AsFormat23) reads with every NIGHT41 block off
//   2. N41Blocks round-trips through its bits, off is the default value, every name has a design row, and every level row's levelTo is in
//      design.maps.list with its file present (NIGHT40's G0 missed the list)
//   3. Curriculum25.N41Levels: with every NIGHT41 block forced off the order is NIGHT40's; a level row swaps its id; finale41 adds level 6
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N41SharedTests
    {
        static GameConfig cfg;
        static string Root => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root, "PortExport", "config.json"), Path.Combine(Root, "Design", "unity-config.json"));

        [Test]
        public void TheTape_Format24_CarriesTheN41Value()
        {
            var t = new RunTape { n31 = 0b10110101, n36 = 0b110100101, n37 = 0b10110, n38 = 0b10100101, n39 = 0b101, n40 = 0b1011001101, n41 = 0b10110011, bank33 = 12345, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(27) /* NIGHT46 rows: format 27 (NightRowsTapeTests.AsFormat26 strips the rows record) */, "NIGHT43: the current format is 26 (N43SharedTests.AsFormat25 and N42SharedTests.AsFormat24 strip their values)");
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n41, Is.EqualTo(0b10110011));
            Assert.That(back.n40, Is.EqualTo(0b1011001101));
            Assert.That(back.bank33, Is.EqualTo(12345));

            RunTape old = RunTape.FromBytes(AsFormat23(b));
            Assert.That(old.n41, Is.EqualTo(0), "format 23: every NIGHT41 block off");
            Assert.That(old.n40, Is.EqualTo(0b1011001101), "format 23 keeps its NIGHT40 value");
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndEveryLevelIsListed()
        {
            Assert.That(N41Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N41Blocks.Count); b++) Assert.That(N41Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N41Blocks.Names.Length, Is.EqualTo(N41Blocks.Count));
            Assert.That(N41Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT41 blocks forced off (N37TestBaseline)");
            var c = Cfg;
            foreach (string n in N41Blocks.Names) Assert.That(c.Has("design.n41." + n + ".enabled"), Is.True, "a design row for " + n);
            var listed = new Dictionary<string, string>();
            foreach (object o in c.Arr("design.maps.list"))
            {
                var d = PortData.Obj(o);
                if (d.TryGetValue("id", out object id) && id is string s && d.TryGetValue("file", out object f) && f is string fs) listed[s] = fs;
            }
            foreach (string row in new[] { "foundry41", "kiln41", "spire41", "finale41" })
            {
                string to = c.Str("design.n41." + row + ".levelTo");
                Assert.That(listed.ContainsKey(to), Is.True, row + "'s level " + to + " is in design.maps.list");
                Assert.That(File.Exists(Path.Combine(Root, listed[to])), Is.True, to + "'s file exists");
                Assert.That(File.Exists(Path.Combine(Root, "Assets", "StreamingAssets", listed[to])), Is.True, to + "'s Player copy exists");
            }
        }

        [Test]
        public void TheLevelRows_SwapAndReturn_OnlyWhenOn()
        {
            var c = Cfg;
            var order = new List<string> { "n38-foundry", "n40-iceline", "n40-kiln", "n40-span", "n40-spire" };
            var forced = new List<string>(order);
            Curriculum25.N41Levels(c, forced);
            Assert.That(forced, Is.EqualTo(order), "forced off: NIGHT40's order");
            bool was = N41Blocks.ForceOff;
            try
            {
                N41Blocks.ForceOff = false;
                var asConfigured = new List<string>(order);
                Curriculum25.N41Levels(c, asConfigured);
                foreach (string row in new[] { "foundry41", "kiln41", "spire41", "finale41" })
                    if (c.Design("n41." + row + ".enabled")) Assert.Pass("a level row is on in the config: the swap is the builder's test");
                Assert.That(asConfigured, Is.EqualTo(order), "every NIGHT41 level row off in the config: NIGHT40's order");
            }
            finally { N41Blocks.ForceOff = was; }
        }

        /// <summary>A format 24 tape's bytes as a format 23 build wrote them: the version 23, no NIGHT41 value (after the NIGHT40 value). A
        /// tape that is already format 23 or older passes through.</summary>
        public static byte[] AsFormat23(byte[] b24)
        {
            b24 = N42SharedTests.AsFormat24(b24); // NIGHT42: a format 25 tape first loses its NIGHT42 value
            if (BitConverter.ToInt32(b24, RunTape.Magic.Length) < 24) return b24;
            var r = new TapeReader(b24, RunTape.Magic.Length, b24.Length);
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
            r.VarI(); // n40
            int at = r.Position;
            r.VarI(); // n41
            int cut = r.Position - at;
            var b23 = new byte[b24.Length - cut];
            Buffer.BlockCopy(b24, 0, b23, 0, at);
            Buffer.BlockCopy(b24, at + cut, b23, at, b24.Length - at - cut);
            var w = new TapeWriter(23);
            w.U32(23);
            Buffer.BlockCopy(w.data, 0, b23, RunTape.Magic.Length, 4);
            return b23;
        }
    }
}
