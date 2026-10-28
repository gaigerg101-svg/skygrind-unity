// NIGHT32 (the lead): the shared commit's rules.
//   1. the tape format 15 carries the NIGHT32 byte, and a format 14 tape (AsFormat14) reads with every NIGHT32 block off
//   2. every test in this assembly runs with the NIGHT32 blocks forced off unless it hands the sim its own override
//   3. N32Blocks round-trips through its bits, and off is the default value
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    [SetUpFixture]
    public sealed class N32TestBaseline
    {
        bool was;

        [OneTimeSetUp]
        public void Up() { was = N32Blocks.ForceOff; N32Blocks.ForceOff = true; }

        [OneTimeTearDown]
        public void Down() { N32Blocks.ForceOff = was; }
    }

    public class N32SharedTests
    {
        [Test]
        public void TheTape_Format15_CarriesTheN32Byte()
        {
            var t = new RunTape { n27 = 0b101101, n29 = 0b100101, n30 = 0b11010011, n31 = 0b10110101, n32 = 0b1011001, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT33: the current format is 16; the n32 byte is read below
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n32, Is.EqualTo(0b1011001));
            Assert.That(back.n31, Is.EqualTo(0b10110101));
            Assert.That(back.n30, Is.EqualTo(0b11010011));
            RunTape old = RunTape.FromBytes(AsFormat14(b));
            Assert.That(old.n32, Is.EqualTo(0), "format 14: every NIGHT32 block off");
            Assert.That(old.n31, Is.EqualTo(0b10110101), "format 14 keeps its NIGHT31 value");
            RunTape older = RunTape.FromBytes(N31SharedTests.AsFormat13(b));
            Assert.That(older.n32, Is.EqualTo(0));
            Assert.That(older.n31, Is.EqualTo(0), "format 13: every NIGHT31 block off");
            Assert.That(older.n30, Is.EqualTo(0b11010011));
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndOffIsTheDefault()
        {
            Assert.That(N32Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N32Blocks.Count); b++) Assert.That(N32Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N32Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT32 blocks forced off (N32TestBaseline)");
        }

        [Test]
        public void TheLevelRows_SwapRebuiltLevels_AndInsertLevel5_OnlyWhenOn()
        {
            Dictionary<string, object> Row(bool on, string extraKey, object extra) => new Dictionary<string, object> { ["enabled"] = on, [extraKey] = extra };
            GameConfig Cfg(bool a, bool s, bool f) => new GameConfig(new Dictionary<string, object>
            {
                ["design"] = new Dictionary<string, object>
                {
                    ["n30"] = new Dictionary<string, object> { ["levels"] = new Dictionary<string, object> { ["enabled"] = true, ["order"] = new List<object> { "n30-foundry", "n30-iceline", "n30-kiln", "n30-span", "level3" } } },
                    ["n32"] = new Dictionary<string, object>
                    {
                        ["arenas32"] = Row(a, "levelSwap", new Dictionary<string, object> { ["n30-foundry"] = "n32-foundry" }),
                        ["span32"] = Row(s, "levelSwap", new Dictionary<string, object> { ["n30-span"] = "n32-span" }),
                        ["level5"] = new Dictionary<string, object> { ["enabled"] = f, ["id"] = "n32-five", ["after"] = "n30-span" },
                    },
                },
            });
            var all = new N25Blocks { tutorial = true, curriculum = true };
            bool was = N32Blocks.ForceOff, was30 = N30Blocks.ForceOff;
            N30Blocks.ForceOff = false;
            try
            {
                N32Blocks.ForceOff = false;
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(false, false, false), all)), Is.EqualTo("n30-foundry,n30-iceline,n30-kiln,n30-span,level3"), "every NIGHT32 row off: NIGHT31's order");
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(true, true, true), all)), Is.EqualTo("n32-foundry,n30-iceline,n30-kiln,n32-span,n32-five,level3"), "swaps, and level 5 right after level 4");
                N32Blocks.ForceOff = true;
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(true, true, true), all)), Is.EqualTo("n30-foundry,n30-iceline,n30-kiln,n30-span,level3"), "--off32: NIGHT31's order");
            }
            finally { N32Blocks.ForceOff = was; N30Blocks.ForceOff = was30; }
        }

        /// <summary>A format 15 tape's bytes as a format 14 build wrote them: the version 14, no NIGHT32 byte (after the NIGHT31 byte).
        /// A tape that is already format 14 or older passes through.</summary>
        public static byte[] AsFormat14(byte[] b15)
        {
            b15 = N33SharedTests.AsFormat15(b15); // NIGHT33: a format 16 tape first loses its NIGHT33 bytes
            if (BitConverter.ToInt32(b15, RunTape.Magic.Length) < 15) return b15;
            var r = new TapeReader(b15, RunTape.Magic.Length, b15.Length);
            r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
            r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte();
            r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI();
            r.Byte(); // n25
            r.Byte(); // n26
            r.Byte(); // n27
            r.Byte(); // n29
            r.Byte(); // n30
            r.Byte(); // n31
            int at = r.Position;
            r.Byte(); // n32
            var b14 = new byte[b15.Length - 1];
            Buffer.BlockCopy(b15, 0, b14, 0, at);
            Buffer.BlockCopy(b15, at + 1, b14, at, b15.Length - at - 1);
            var w = new TapeWriter(14);
            w.U32(14);
            Buffer.BlockCopy(w.data, 0, b14, RunTape.Magic.Length, 4);
            return b14;
        }
    }
}
