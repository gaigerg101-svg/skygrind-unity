// NIGHT43 (the shared commit, the lead): the checks for the NIGHT43 blocks' shared plumbing. Every test here passes with the NIGHT43
// blocks forced off by N37SharedTests' baseline (the Game test namespace runs with NIGHT37-NIGHT43 off unless it hands its own override):
//   1. the tape format 26 carries the NIGHT43 value (a VarI), and a format 25 tape (AsFormat25) reads with every NIGHT43 block off
//   2. N43Blocks round-trips through its bits, off is the default value, every name has a design row, and the three n43 levels are in
//      design.maps.list with their files present
//   3. Curriculum25.N43Levels: with every NIGHT43 block forced off the order is NIGHT42's
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N43SharedTests
    {
        static GameConfig cfg;
        static string Root => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root, "PortExport", "config.json"), Path.Combine(Root, "Design", "unity-config.json"));

        static readonly string[] LevelRows = { "foundry43", "iceline43", "kiln43" };

        [Test]
        public void TheTape_Format26_CarriesTheN43Value()
        {
            var t = new RunTape { n31 = 0b10110101, n36 = 0b110100101, n37 = 0b10110, n38 = 0b10100101, n39 = 0b101, n40 = 0b1011001101, n41 = 0b10110011, n42 = 0b101100110, n43 = 0b101101001011, bank33 = 12345, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(27) /* NIGHT46 rows: format 27 (NightRowsTapeTests.AsFormat26 strips the rows record) */);
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n43, Is.EqualTo(0b101101001011));
            Assert.That(back.n42, Is.EqualTo(0b101100110));
            Assert.That(back.bank33, Is.EqualTo(12345));

            RunTape old = RunTape.FromBytes(AsFormat25(b));
            Assert.That(old.n43, Is.EqualTo(0), "format 25: every NIGHT43 block off");
            Assert.That(old.n42, Is.EqualTo(0b101100110), "format 25 keeps its NIGHT42 value");
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndTheLevelsAreListed()
        {
            Assert.That(N43Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N43Blocks.Count); b++) Assert.That(N43Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N43Blocks.Names.Length, Is.EqualTo(N43Blocks.Count));
            Assert.That(N43Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT43 blocks forced off (N37SharedTests' baseline)");
            var c = Cfg;
            foreach (string n in N43Blocks.Names) Assert.That(c.Has("design.n43." + n + ".enabled"), Is.True, "a design row for " + n);
            var listed = new Dictionary<string, string>();
            foreach (object o in c.Arr("design.maps.list"))
            {
                var d = PortData.Obj(o);
                if (d.TryGetValue("id", out object id) && id is string s && d.TryGetValue("file", out object f) && f is string fs) listed[s] = fs;
            }
            foreach (string row in LevelRows)
            {
                string to = c.Str("design.n43." + row + ".levelTo");
                Assert.That(listed.ContainsKey(to), Is.True, row + "'s level " + to + " is in design.maps.list");
                Assert.That(File.Exists(Path.Combine(Root, listed[to])), Is.True, to + "'s file exists");
                Assert.That(File.Exists(Path.Combine(Root, "Assets", "StreamingAssets", listed[to])), Is.True, to + "'s Player copy exists");
            }
        }

        [Test]
        public void TheLevelRows_Swap_OnlyWhenOn()
        {
            var c = Cfg;
            var order = new List<string> { "n42-foundry", "n40-iceline", "n41-kiln", "n40-span", "n41-spire" };
            var forced = new List<string>(order);
            Curriculum25.N43Levels(c, forced);
            Assert.That(forced, Is.EqualTo(order), "forced off: NIGHT42's order");
            bool was = N43Blocks.ForceOff;
            try
            {
                N43Blocks.ForceOff = false;
                var asConfigured = new List<string>(order);
                Curriculum25.N43Levels(c, asConfigured);
                for (int i = 0; i < LevelRows.Length; i++)
                {
                    string from = c.Str("design.n43." + LevelRows[i] + ".levelFrom"), to = c.Str("design.n43." + LevelRows[i] + ".levelTo");
                    int at = order.IndexOf(from);
                    Assert.That(at, Is.GreaterThanOrEqualTo(0), LevelRows[i] + "'s levelFrom is in NIGHT42's order");
                    Assert.That(asConfigured[at], Is.EqualTo(c.Design("n43." + LevelRows[i] + ".enabled") ? to : from), LevelRows[i]);
                }
            }
            finally { N43Blocks.ForceOff = was; }
        }

        /// <summary>A format 26 tape's bytes as a format 25 build wrote them: the version 25, no NIGHT43 value (after the NIGHT42 value). A
        /// tape that is already format 25 or older passes through.</summary>
        public static byte[] AsFormat25(byte[] b26)
        {
            b26 = NightRowsTapeTests.AsFormat26(b26); // NIGHT46 rows: a format 27 tape first loses its rows record (the chain's last link)
            if (BitConverter.ToInt32(b26, RunTape.Magic.Length) < 26) return b26;
            var r = new TapeReader(b26, RunTape.Magic.Length, b26.Length);
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
            r.VarI(); // n42
            int at = r.Position;
            r.VarI(); // n43
            int cut = r.Position - at;
            var b25 = new byte[b26.Length - cut];
            Buffer.BlockCopy(b26, 0, b25, 0, at);
            Buffer.BlockCopy(b26, at + cut, b25, at, b26.Length - at - cut);
            var w = new TapeWriter(25);
            w.U32(25);
            Buffer.BlockCopy(w.data, 0, b25, RunTape.Magic.Length, 4);
            return b25;
        }
    }
}
