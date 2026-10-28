// NIGHT42 (the shared commit, the lead): the checks for the NIGHT42 blocks' shared plumbing. Every test here passes with the NIGHT42
// blocks forced off by N37SharedTests' baseline (the Game test namespace runs with NIGHT37-NIGHT42 off unless it hands its own override):
//   1. the tape format 25 carries the NIGHT42 value (a VarI), and a format 24 tape (AsFormat24) reads with every NIGHT42 block off
//   2. N42Blocks round-trips through its bits, off is the default value, every name has a design row, and n42-foundry is in
//      design.maps.list with its file present
//   3. Curriculum25.N42Levels: with every NIGHT42 block forced off the order is NIGHT41's
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N42SharedTests
    {
        static GameConfig cfg;
        static string Root => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root, "PortExport", "config.json"), Path.Combine(Root, "Design", "unity-config.json"));

        [Test]
        public void TheTape_Format25_CarriesTheN42Value()
        {
            var t = new RunTape { n31 = 0b10110101, n36 = 0b110100101, n37 = 0b10110, n38 = 0b10100101, n39 = 0b101, n40 = 0b1011001101, n41 = 0b10110011, n42 = 0b101100110, bank33 = 12345, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(27) /* NIGHT46 rows: format 27 (NightRowsTapeTests.AsFormat26 strips the rows record) */, "NIGHT43: the current format is 26 (N43SharedTests.AsFormat25 strips its value)");
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n42, Is.EqualTo(0b101100110));
            Assert.That(back.n41, Is.EqualTo(0b10110011));
            Assert.That(back.bank33, Is.EqualTo(12345));

            RunTape old = RunTape.FromBytes(AsFormat24(b));
            Assert.That(old.n42, Is.EqualTo(0), "format 24: every NIGHT42 block off");
            Assert.That(old.n41, Is.EqualTo(0b10110011), "format 24 keeps its NIGHT41 value");
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndTheLevelIsListed()
        {
            Assert.That(N42Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N42Blocks.Count); b++) Assert.That(N42Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N42Blocks.Names.Length, Is.EqualTo(N42Blocks.Count));
            Assert.That(N42Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT42 blocks forced off (N37SharedTests' baseline)");
            var c = Cfg;
            foreach (string n in N42Blocks.Names) Assert.That(c.Has("design.n42." + n + ".enabled"), Is.True, "a design row for " + n);
            var listed = new Dictionary<string, string>();
            foreach (object o in c.Arr("design.maps.list"))
            {
                var d = PortData.Obj(o);
                if (d.TryGetValue("id", out object id) && id is string s && d.TryGetValue("file", out object f) && f is string fs) listed[s] = fs;
            }
            string to = c.Str("design.n42.foundry42.levelTo");
            Assert.That(listed.ContainsKey(to), Is.True, "foundry42's level " + to + " is in design.maps.list");
            Assert.That(File.Exists(Path.Combine(Root, listed[to])), Is.True, to + "'s file exists");
            Assert.That(File.Exists(Path.Combine(Root, "Assets", "StreamingAssets", listed[to])), Is.True, to + "'s Player copy exists");
        }

        [Test]
        public void TheLevelRow_Swaps_OnlyWhenOn()
        {
            var c = Cfg;
            var order = new List<string> { "n41-foundry", "n40-iceline", "n41-kiln", "n40-span", "n41-spire" };
            var forced = new List<string>(order);
            Curriculum25.N42Levels(c, forced);
            Assert.That(forced, Is.EqualTo(order), "forced off: NIGHT41's order");
            bool was = N42Blocks.ForceOff;
            try
            {
                N42Blocks.ForceOff = false;
                var asConfigured = new List<string>(order);
                Curriculum25.N42Levels(c, asConfigured);
                if (c.Design("n42.foundry42.enabled")) { Assert.That(asConfigured[0], Is.EqualTo("n42-foundry")); return; }
                Assert.That(asConfigured, Is.EqualTo(order), "foundry42 off in the config: NIGHT41's order");
            }
            finally { N42Blocks.ForceOff = was; }
        }

        /// <summary>A format 25 tape's bytes as a format 24 build wrote them: the version 24, no NIGHT42 value (after the NIGHT41 value). A
        /// tape that is already format 24 or older passes through.</summary>
        public static byte[] AsFormat24(byte[] b25)
        {
            b25 = N43SharedTests.AsFormat25(b25); // NIGHT43: a format 26 tape first loses its NIGHT43 value
            if (BitConverter.ToInt32(b25, RunTape.Magic.Length) < 25) return b25;
            var r = new TapeReader(b25, RunTape.Magic.Length, b25.Length);
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
            r.VarI(); // n41
            int at = r.Position;
            r.VarI(); // n42
            int cut = r.Position - at;
            var b24 = new byte[b25.Length - cut];
            Buffer.BlockCopy(b25, 0, b24, 0, at);
            Buffer.BlockCopy(b25, at + cut, b24, at, b25.Length - at - cut);
            var w = new TapeWriter(24);
            w.U32(24);
            Buffer.BlockCopy(w.data, 0, b24, RunTape.Magic.Length, 4);
            return b24;
        }
    }
}
