// NIGHT37 (the lead): the shared commit's rules.
//   1. the tape format 20 carries the NIGHT37 value (a VarI), and a format 19 tape (AsFormat19) reads with every NIGHT37 block off
//   2. every test in this assembly runs with the NIGHT37 blocks forced off unless it hands the sim its own override
//   3. N37Blocks round-trips through its bits and off is the default value
//   4. a boss row's levelFrom -> levelTo swap applies only while that row is on
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    [SetUpFixture]
    public sealed class N37TestBaseline
    {
        bool was, was38, was39, was40, was41, was42, was43; int wasRows; // NIGHT46 rows

        [OneTimeSetUp]
        public void Up() { was = N37Blocks.ForceOff; N37Blocks.ForceOff = true; was38 = N38Blocks.ForceOff; N38Blocks.ForceOff = true; was39 = N39Blocks.ForceOff; N39Blocks.ForceOff = true; was40 = N40Blocks.ForceOff; N40Blocks.ForceOff = true; was41 = N41Blocks.ForceOff; N41Blocks.ForceOff = true; was42 = N42Blocks.ForceOff; N42Blocks.ForceOff = true; was43 = N43Blocks.ForceOff; N43Blocks.ForceOff = true; wasRows = NightRows.ForceOffFrom; NightRows.ForceOffFrom = NightRows.FirstNight; /* NIGHT46 rows: and every NIGHT46+ row off */ } // NIGHT38-NIGHT43: and every NIGHT38-NIGHT43 block off (N38SharedTests ... N43SharedTests)

        [OneTimeTearDown]
        public void Down() { N37Blocks.ForceOff = was; N38Blocks.ForceOff = was38; N39Blocks.ForceOff = was39; N40Blocks.ForceOff = was40; N41Blocks.ForceOff = was41; N42Blocks.ForceOff = was42; N43Blocks.ForceOff = was43; NightRows.ForceOffFrom = wasRows; /* NIGHT46 rows */ }
    }

    public class N37SharedTests
    {
        [Test]
        public void TheTape_Format20_CarriesTheN37Value()
        {
            var t = new RunTape { n31 = 0b10110101, n32 = 0b1011001, n33 = 0b1100101, shop33 = 0b10010011, bank33 = 12345, n34 = 0b101101, n35 = 0b1011, n36 = 0b110100101, n37 = 0b10110, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(27) /* NIGHT46 rows: format 27 (NightRowsTapeTests.AsFormat26 strips the rows record) */, "NIGHT43: the current format is 26 (N43SharedTests.AsFormat25, N42SharedTests.AsFormat24, N41SharedTests.AsFormat23, N40SharedTests.AsFormat22, N39SharedTests.AsFormat21 and N38SharedTests.AsFormat20 strip their values)");
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n37, Is.EqualTo(0b10110));
            Assert.That(back.n36, Is.EqualTo(0b110100101));
            Assert.That(back.bank33, Is.EqualTo(12345));
            RunTape old = RunTape.FromBytes(AsFormat19(b));
            Assert.That(old.n37, Is.EqualTo(0), "format 19: every NIGHT37 block off");
            Assert.That(old.n36, Is.EqualTo(0b110100101), "format 19 keeps its NIGHT36 value");
            RunTape older = RunTape.FromBytes(N36SharedTests.AsFormat18(b));
            Assert.That(older.n37, Is.EqualTo(0));
            Assert.That(older.n36, Is.EqualTo(0), "format 18: every NIGHT36 block off");
            Assert.That(older.n35, Is.EqualTo(0b1011));
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndOffIsTheDefault()
        {
            Assert.That(N37Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N37Blocks.Count); b++) Assert.That(N37Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N37Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT37 blocks forced off (N37TestBaseline)");
        }

        [Test]
        public void ABossRow_SwapsItsLevel_OnlyWhenOn()
        {
            GameConfig Cfg(bool wick, bool loom) => new GameConfig(new Dictionary<string, object>
            {
                ["design"] = new Dictionary<string, object>
                {
                    ["n30"] = new Dictionary<string, object> { ["levels"] = new Dictionary<string, object> { ["enabled"] = true, ["order"] = new List<object> { "n36-foundry", "n35-iceline", "n35-kiln", "n33-span", "n33-spire", "n35-finale" } } },
                    ["n37"] = new Dictionary<string, object>
                    {
                        ["wick37"] = new Dictionary<string, object> { ["enabled"] = wick, ["levelFrom"] = "n36-foundry", ["levelTo"] = "n37-foundry" },
                        ["loom37"] = new Dictionary<string, object> { ["enabled"] = loom, ["levelFrom"] = "n33-span", ["levelTo"] = "n37-span" },
                    },
                },
            });
            var all = new N25Blocks { tutorial = true, curriculum = true };
            bool was = N37Blocks.ForceOff, was36 = N36Blocks.ForceOff, was35 = N35Blocks.ForceOff, was34 = N34Blocks.ForceOff, was33 = N33Blocks.ForceOff, was32 = N32Blocks.ForceOff, was30 = N30Blocks.ForceOff;
            N30Blocks.ForceOff = false;
            N32Blocks.ForceOff = false;
            N33Blocks.ForceOff = false;
            N34Blocks.ForceOff = false;
            N35Blocks.ForceOff = false;
            N36Blocks.ForceOff = false;
            try
            {
                N37Blocks.ForceOff = false;
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(false, false), all)), Is.EqualTo("n36-foundry,n35-iceline,n35-kiln,n33-span,n33-spire,n35-finale"), "rows off: NIGHT36's order");
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(true, false), all)), Is.EqualTo("n37-foundry,n35-iceline,n35-kiln,n33-span,n33-spire,n35-finale"), "wick37 on: level 1 swapped only");
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(true, true), all)), Is.EqualTo("n37-foundry,n35-iceline,n35-kiln,n37-span,n33-spire,n35-finale"), "wick37 + loom37 on: levels 1 and 4");
                N37Blocks.ForceOff = true;
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(true, true), all)), Is.EqualTo("n36-foundry,n35-iceline,n35-kiln,n33-span,n33-spire,n35-finale"), "--off37: NIGHT36's order");
            }
            finally { N37Blocks.ForceOff = was; N36Blocks.ForceOff = was36; N35Blocks.ForceOff = was35; N34Blocks.ForceOff = was34; N33Blocks.ForceOff = was33; N32Blocks.ForceOff = was32; N30Blocks.ForceOff = was30; }
        }

        /// <summary>A format 20 tape's bytes as a format 19 build wrote them: the version 19, no NIGHT37 value (after the NIGHT36 value). A
        /// tape that is already format 19 or older passes through.</summary>
        public static byte[] AsFormat19(byte[] b20)
        {
            b20 = N38SharedTests.AsFormat20(b20); // NIGHT38: a format 21 tape first loses its NIGHT38 value
            if (BitConverter.ToInt32(b20, RunTape.Magic.Length) < 20) return b20;
            var r = new TapeReader(b20, RunTape.Magic.Length, b20.Length);
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
            int at = r.Position;
            r.VarI(); // n37
            int cut = r.Position - at;
            var b19 = new byte[b20.Length - cut];
            Buffer.BlockCopy(b20, 0, b19, 0, at);
            Buffer.BlockCopy(b20, at + cut, b19, at, b20.Length - at - cut);
            var w = new TapeWriter(19);
            w.U32(19);
            Buffer.BlockCopy(w.data, 0, b19, RunTape.Magic.Length, 4);
            return b19;
        }
    }
}
