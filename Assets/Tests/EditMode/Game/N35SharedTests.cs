// NIGHT35 (the lead): the shared commit's rules.
//   1. the tape format 18 carries the NIGHT35 byte, and a format 17 tape (AsFormat17) reads with every NIGHT35 block off
//   2. every test in this assembly runs with the NIGHT35 blocks forced off unless it hands the sim its own override
//   3. N35Blocks round-trips through its bits and off is the default value
//   4. design.n35.campaign6 makes the order six levels (the swap, the finale after `after`, nothing after it) only while it is on
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    [SetUpFixture]
    public sealed class N35TestBaseline
    {
        bool was;

        [OneTimeSetUp]
        public void Up() { was = N35Blocks.ForceOff; N35Blocks.ForceOff = true; }

        [OneTimeTearDown]
        public void Down() { N35Blocks.ForceOff = was; }
    }

    public class N35SharedTests
    {
        [Test]
        public void TheTape_Format18_CarriesTheN35Byte()
        {
            var t = new RunTape { n31 = 0b10110101, n32 = 0b1011001, n33 = 0b1100101, shop33 = 0b10010011, bank33 = 12345, n34 = 0b101101, n35 = 0b1011, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT36: the current format is 19; the n35 byte is read below
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n35, Is.EqualTo(0b1011));
            Assert.That(back.n34, Is.EqualTo(0b101101));
            Assert.That(back.bank33, Is.EqualTo(12345));
            RunTape old = RunTape.FromBytes(AsFormat17(b));
            Assert.That(old.n35, Is.EqualTo(0), "format 17: every NIGHT35 block off");
            Assert.That(old.n34, Is.EqualTo(0b101101), "format 17 keeps its NIGHT34 value");
            RunTape older = RunTape.FromBytes(N34SharedTests.AsFormat16(b));
            Assert.That(older.n35, Is.EqualTo(0));
            Assert.That(older.n34, Is.EqualTo(0), "format 16: every NIGHT34 block off");
            Assert.That(older.n33, Is.EqualTo(0b1100101));
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndOffIsTheDefault()
        {
            Assert.That(N35Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N35Blocks.Count); b++) Assert.That(N35Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N35Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT35 blocks forced off (N35TestBaseline)");
        }

        [Test]
        public void TheCampaign6Row_MakesSixLevels_OnlyWhenOn()
        {
            GameConfig Cfg(bool on) => new GameConfig(new Dictionary<string, object>
            {
                ["design"] = new Dictionary<string, object>
                {
                    ["n30"] = new Dictionary<string, object> { ["levels"] = new Dictionary<string, object> { ["enabled"] = true, ["order"] = new List<object> { "n33-foundry", "n33-iceline", "n33-kiln", "n33-span", "n33-spire", "level3", "level4" } } },
                    ["n35"] = new Dictionary<string, object> { ["campaign6"] = new Dictionary<string, object> { ["enabled"] = on, ["after"] = "n33-spire", ["finaleId"] = "n35-finale", ["levelSwap"] = new Dictionary<string, object> { ["n33-foundry"] = "n35-foundry" } } },
                },
            });
            var all = new N25Blocks { tutorial = true, curriculum = true };
            bool was = N35Blocks.ForceOff, was34 = N34Blocks.ForceOff, was33 = N33Blocks.ForceOff, was32 = N32Blocks.ForceOff, was30 = N30Blocks.ForceOff;
            N30Blocks.ForceOff = false;
            N32Blocks.ForceOff = false;
            N33Blocks.ForceOff = false;
            N34Blocks.ForceOff = false;
            try
            {
                N35Blocks.ForceOff = false;
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(false), all)), Is.EqualTo("n33-foundry,n33-iceline,n33-kiln,n33-span,n33-spire,level3,level4"), "campaign6 off: NIGHT34's order");
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(true), all)), Is.EqualTo("n35-foundry,n33-iceline,n33-kiln,n33-span,n33-spire,n35-finale"), "campaign6 on: six levels, the finale last");
                N35Blocks.ForceOff = true;
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(true), all)), Is.EqualTo("n33-foundry,n33-iceline,n33-kiln,n33-span,n33-spire,level3,level4"), "--off35: NIGHT34's order");
            }
            finally { N35Blocks.ForceOff = was; N34Blocks.ForceOff = was34; N33Blocks.ForceOff = was33; N32Blocks.ForceOff = was32; N30Blocks.ForceOff = was30; }
        }

        /// <summary>A format 18 tape's bytes as a format 17 build wrote them: the version 17, no NIGHT35 byte (after the NIGHT34 byte). A
        /// tape that is already format 17 or older passes through.</summary>
        public static byte[] AsFormat17(byte[] b18)
        {
            b18 = N36SharedTests.AsFormat18(b18); // NIGHT36: a format 19 tape first loses its NIGHT36 value
            if (BitConverter.ToInt32(b18, RunTape.Magic.Length) < 18) return b18;
            var r = new TapeReader(b18, RunTape.Magic.Length, b18.Length);
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
            int at = r.Position;
            r.Byte(); // n35
            int cut = r.Position - at;
            var b17 = new byte[b18.Length - cut];
            Buffer.BlockCopy(b18, 0, b17, 0, at);
            Buffer.BlockCopy(b18, at + cut, b17, at, b18.Length - at - cut);
            var w = new TapeWriter(17);
            w.U32(17);
            Buffer.BlockCopy(w.data, 0, b17, RunTape.Magic.Length, 4);
            return b17;
        }
    }
}
