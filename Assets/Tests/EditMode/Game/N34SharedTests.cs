// NIGHT34 (the lead): the shared commit's rules.
//   1. the tape format 17 carries the NIGHT34 byte, and a format 16 tape (AsFormat16) reads with every NIGHT34 block off
//   2. every test in this assembly runs with the NIGHT34 blocks forced off unless it hands the sim its own override
//   3. N34Blocks round-trips through its bits, off is the default value, and the split counts only with the bend on
//   4. design.n34.lines34 swaps NIGHT33's level ids for the n34 files only while it is on (and not with --off34)
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    [SetUpFixture]
    public sealed class N34TestBaseline
    {
        bool was;

        [OneTimeSetUp]
        public void Up() { was = N34Blocks.ForceOff; N34Blocks.ForceOff = true; }

        [OneTimeTearDown]
        public void Down() { N34Blocks.ForceOff = was; }
    }

    public class N34SharedTests
    {
        [Test]
        public void TheTape_Format17_CarriesTheN34Byte()
        {
            var t = new RunTape { n31 = 0b10110101, n32 = 0b1011001, n33 = 0b1100101, shop33 = 0b10010011, bank33 = 12345, n34 = 0b1011, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT35: the current format is 18; the n34 bytes are read below
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n34, Is.EqualTo(0b1011));
            Assert.That(back.n33, Is.EqualTo(0b1100101));
            Assert.That(back.bank33, Is.EqualTo(12345));
            RunTape old = RunTape.FromBytes(AsFormat16(b));
            Assert.That(old.n34, Is.EqualTo(0), "format 16: every NIGHT34 block off");
            Assert.That(old.n33, Is.EqualTo(0b1100101), "format 16 keeps its NIGHT33 value");
            Assert.That(old.bank33, Is.EqualTo(12345));
            RunTape older = RunTape.FromBytes(N33SharedTests.AsFormat15(b));
            Assert.That(older.n34, Is.EqualTo(0));
            Assert.That(older.n33, Is.EqualTo(0), "format 15: every NIGHT33 block off");
            Assert.That(older.n32, Is.EqualTo(0b1011001));
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndOffIsTheDefault()
        {
            Assert.That(N34Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N34Blocks.Count); b++) Assert.That(N34Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N34Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT34 blocks forced off (N34TestBaseline)");
            GameConfig Cfg(bool bend, bool split) => new GameConfig(new Dictionary<string, object>
            {
                ["design"] = new Dictionary<string, object> { ["n34"] = new Dictionary<string, object> { ["bend34"] = new Dictionary<string, object> { ["enabled"] = bend, ["split"] = split } } },
            });
            Assert.That(N34Blocks.From(Cfg(false, true)).bendSplit34, Is.False, "the split counts only with the bend on");
            Assert.That(N34Blocks.From(Cfg(true, true)).bendSplit34, Is.True);
            Assert.That(N34Blocks.From(Cfg(true, false)).bendSplit34, Is.False);
        }

        [Test]
        public void TheLinesRow_SwapsNight33Levels_OnlyWhenOn()
        {
            GameConfig Cfg(bool k) => new GameConfig(new Dictionary<string, object>
            {
                ["design"] = new Dictionary<string, object>
                {
                    ["n30"] = new Dictionary<string, object> { ["levels"] = new Dictionary<string, object> { ["enabled"] = true, ["order"] = new List<object> { "n30-foundry", "n30-iceline", "level3" } } },
                    ["n32"] = new Dictionary<string, object> { ["arenas32"] = new Dictionary<string, object> { ["enabled"] = true, ["levelSwap"] = new Dictionary<string, object> { ["n30-foundry"] = "n32-foundry" } } },
                    ["n33"] = new Dictionary<string, object> { ["kit33"] = new Dictionary<string, object> { ["enabled"] = true, ["levelSwap"] = new Dictionary<string, object> { ["n32-foundry"] = "n33-foundry" } } },
                    ["n34"] = new Dictionary<string, object> { ["lines34"] = new Dictionary<string, object> { ["enabled"] = k, ["levelSwap"] = new Dictionary<string, object> { ["n33-foundry"] = "n34-foundry" } } },
                },
            });
            var all = new N25Blocks { tutorial = true, curriculum = true };
            bool was = N34Blocks.ForceOff, was33 = N33Blocks.ForceOff, was32 = N32Blocks.ForceOff, was30 = N30Blocks.ForceOff;
            N30Blocks.ForceOff = false;
            N32Blocks.ForceOff = false;
            N33Blocks.ForceOff = false;
            try
            {
                N34Blocks.ForceOff = false;
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(false), all)), Is.EqualTo("n33-foundry,n30-iceline,level3"), "lines34 off: NIGHT33's order");
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(true), all)), Is.EqualTo("n34-foundry,n30-iceline,level3"), "lines34 on: the n34 file");
                N34Blocks.ForceOff = true;
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(true), all)), Is.EqualTo("n33-foundry,n30-iceline,level3"), "--off34: NIGHT33's order");
            }
            finally { N34Blocks.ForceOff = was; N33Blocks.ForceOff = was33; N32Blocks.ForceOff = was32; N30Blocks.ForceOff = was30; }
        }

        /// <summary>A format 17 tape's bytes as a format 16 build wrote them: the version 16, no NIGHT34 byte (after the NIGHT33 bank). A
        /// tape that is already format 16 or older passes through.</summary>
        public static byte[] AsFormat16(byte[] b17)
        {
            b17 = N35SharedTests.AsFormat17(b17); // NIGHT35: a format 18 tape first loses its NIGHT35 byte
            if (BitConverter.ToInt32(b17, RunTape.Magic.Length) < 17) return b17;
            var r = new TapeReader(b17, RunTape.Magic.Length, b17.Length);
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
            int at = r.Position;
            r.Byte(); // n34
            int cut = r.Position - at;
            var b16 = new byte[b17.Length - cut];
            Buffer.BlockCopy(b17, 0, b16, 0, at);
            Buffer.BlockCopy(b17, at + cut, b16, at, b17.Length - at - cut);
            var w = new TapeWriter(16);
            w.U32(16);
            Buffer.BlockCopy(w.data, 0, b16, RunTape.Magic.Length, 4);
            return b16;
        }
    }
}
