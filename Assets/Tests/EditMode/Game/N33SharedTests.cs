// NIGHT33 (the lead): the shared commit's rules.
//   1. the tape format 16 carries the NIGHT33 byte, the shop's owned upgrades and the banked points, and a format 15 tape (AsFormat15)
//      reads with every NIGHT33 block off, nothing bought, nothing banked
//   2. every test in this assembly runs with the NIGHT33 blocks forced off unless it hands the sim its own override
//   3. N33Blocks round-trips through its bits, and off is the default value
//   4. design.n33.kit33 swaps NIGHT32's level ids for the n33 files only while it is on (and not with --off33)
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    [SetUpFixture]
    public sealed class N33TestBaseline
    {
        bool was;

        [OneTimeSetUp]
        public void Up() { was = N33Blocks.ForceOff; N33Blocks.ForceOff = true; }

        [OneTimeTearDown]
        public void Down() { N33Blocks.ForceOff = was; }
    }

    public class N33SharedTests
    {
        [Test]
        public void TheTape_Format16_CarriesTheN33Bytes()
        {
            var t = new RunTape { n27 = 0b101101, n29 = 0b100101, n30 = 0b11010011, n31 = 0b10110101, n32 = 0b1011001, n33 = 0b1100101, shop33 = 0b10010011, bank33 = 12345, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT34: the current format is 17; the n33 bytes are read below
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n33, Is.EqualTo(0b1100101));
            Assert.That(back.shop33, Is.EqualTo(0b10010011));
            Assert.That(back.bank33, Is.EqualTo(12345));
            Assert.That(back.n32, Is.EqualTo(0b1011001));
            Assert.That(back.n31, Is.EqualTo(0b10110101));
            RunTape old = RunTape.FromBytes(AsFormat15(b));
            Assert.That(old.n33, Is.EqualTo(0), "format 15: every NIGHT33 block off");
            Assert.That(old.shop33, Is.EqualTo(0), "format 15: nothing bought");
            Assert.That(old.bank33, Is.EqualTo(0), "format 15: nothing banked");
            Assert.That(old.n32, Is.EqualTo(0b1011001), "format 15 keeps its NIGHT32 value");
            RunTape older = RunTape.FromBytes(N32SharedTests.AsFormat14(b));
            Assert.That(older.n33, Is.EqualTo(0));
            Assert.That(older.n32, Is.EqualTo(0), "format 14: every NIGHT32 block off");
            Assert.That(older.n31, Is.EqualTo(0b10110101));
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndOffIsTheDefault()
        {
            Assert.That(N33Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N33Blocks.Count); b++) Assert.That(N33Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N33Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT33 blocks forced off (N33TestBaseline)");
        }

        [Test]
        public void TheKitRow_SwapsNight32Levels_OnlyWhenOn()
        {
            GameConfig Cfg(bool k) => new GameConfig(new Dictionary<string, object>
            {
                ["design"] = new Dictionary<string, object>
                {
                    ["n30"] = new Dictionary<string, object> { ["levels"] = new Dictionary<string, object> { ["enabled"] = true, ["order"] = new List<object> { "n30-foundry", "n30-iceline", "level3" } } },
                    ["n32"] = new Dictionary<string, object> { ["arenas32"] = new Dictionary<string, object> { ["enabled"] = true, ["levelSwap"] = new Dictionary<string, object> { ["n30-foundry"] = "n32-foundry" } } },
                    ["n33"] = new Dictionary<string, object> { ["kit33"] = new Dictionary<string, object> { ["enabled"] = k, ["levelSwap"] = new Dictionary<string, object> { ["n32-foundry"] = "n33-foundry" } } },
                },
            });
            var all = new N25Blocks { tutorial = true, curriculum = true };
            bool was = N33Blocks.ForceOff, was32 = N32Blocks.ForceOff, was30 = N30Blocks.ForceOff;
            N30Blocks.ForceOff = false;
            N32Blocks.ForceOff = false;
            try
            {
                N33Blocks.ForceOff = false;
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(false), all)), Is.EqualTo("n32-foundry,n30-iceline,level3"), "kit33 off: NIGHT32's order");
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(true), all)), Is.EqualTo("n33-foundry,n30-iceline,level3"), "kit33 on: the n33 file");
                N33Blocks.ForceOff = true;
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(true), all)), Is.EqualTo("n32-foundry,n30-iceline,level3"), "--off33: NIGHT32's order");
            }
            finally { N33Blocks.ForceOff = was; N32Blocks.ForceOff = was32; N30Blocks.ForceOff = was30; }
        }

        /// <summary>A format 16 tape's bytes as a format 15 build wrote them: the version 15, no NIGHT33 byte, shop or bank (after the NIGHT32
        /// byte). A tape that is already format 15 or older passes through.</summary>
        public static byte[] AsFormat15(byte[] b16)
        {
            b16 = N34SharedTests.AsFormat16(b16); // NIGHT34: a format 17 tape first loses its NIGHT34 byte
            if (BitConverter.ToInt32(b16, RunTape.Magic.Length) < 16) return b16;
            var r = new TapeReader(b16, RunTape.Magic.Length, b16.Length);
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
            int at = r.Position;
            r.Byte(); // n33
            r.VarI(); // shop33
            r.VarI(); // bank33
            int cut = r.Position - at;
            var b15 = new byte[b16.Length - cut];
            Buffer.BlockCopy(b16, 0, b15, 0, at);
            Buffer.BlockCopy(b16, at + cut, b15, at, b16.Length - at - cut);
            var w = new TapeWriter(15);
            w.U32(15);
            Buffer.BlockCopy(w.data, 0, b15, RunTape.Magic.Length, 4);
            return b15;
        }
    }
}
