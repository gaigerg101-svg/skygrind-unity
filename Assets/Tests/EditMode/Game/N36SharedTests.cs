// NIGHT36 (the lead): the shared commit's rules.
//   1. the tape format 19 carries the NIGHT36 value (a VarI: nine bits), and a format 18 tape (AsFormat18) reads with every NIGHT36 block off
//   2. every test in this assembly runs with the NIGHT36 blocks forced off unless it hands the sim its own override
//   3. N36Blocks round-trips through its bits and off is the default value
//   4. design.n36.level36 swaps level 1 for n36-foundry only while it is on
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    [SetUpFixture]
    public sealed class N36TestBaseline
    {
        bool was;

        [OneTimeSetUp]
        public void Up() { was = N36Blocks.ForceOff; N36Blocks.ForceOff = true; }

        [OneTimeTearDown]
        public void Down() { N36Blocks.ForceOff = was; }
    }

    public class N36SharedTests
    {
        [Test]
        public void TheTape_Format19_CarriesTheN36Value()
        {
            var t = new RunTape { n31 = 0b10110101, n32 = 0b1011001, n33 = 0b1100101, shop33 = 0b10010011, bank33 = 12345, n34 = 0b101101, n35 = 0b1011, n36 = 0b110100101, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT37: the current format is 20; the n36 value is read below
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n36, Is.EqualTo(0b110100101), "all nine bits survive");
            Assert.That(back.n35, Is.EqualTo(0b1011));
            Assert.That(back.bank33, Is.EqualTo(12345));
            RunTape old = RunTape.FromBytes(AsFormat18(b));
            Assert.That(old.n36, Is.EqualTo(0), "format 18: every NIGHT36 block off");
            Assert.That(old.n35, Is.EqualTo(0b1011), "format 18 keeps its NIGHT35 value");
            RunTape older = RunTape.FromBytes(N35SharedTests.AsFormat17(b));
            Assert.That(older.n36, Is.EqualTo(0));
            Assert.That(older.n35, Is.EqualTo(0), "format 17: every NIGHT35 block off");
            Assert.That(older.n34, Is.EqualTo(0b101101));
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndOffIsTheDefault()
        {
            Assert.That(N36Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N36Blocks.Count); b++) Assert.That(N36Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N36Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT36 blocks forced off (N36TestBaseline)");
        }

        [Test]
        public void TheLevel36Row_SwapsLevelOne_OnlyWhenOn()
        {
            GameConfig Cfg(bool on) => new GameConfig(new Dictionary<string, object>
            {
                ["design"] = new Dictionary<string, object>
                {
                    ["n30"] = new Dictionary<string, object> { ["levels"] = new Dictionary<string, object> { ["enabled"] = true, ["order"] = new List<object> { "n35-foundry", "n35-iceline", "n35-kiln", "n33-span", "n33-spire", "n35-finale" } } },
                    ["n36"] = new Dictionary<string, object> { ["level36"] = new Dictionary<string, object> { ["enabled"] = on, ["levelSwap"] = new Dictionary<string, object> { ["n35-foundry"] = "n36-foundry" } } },
                },
            });
            var all = new N25Blocks { tutorial = true, curriculum = true };
            bool was = N36Blocks.ForceOff, was35 = N35Blocks.ForceOff, was34 = N34Blocks.ForceOff, was33 = N33Blocks.ForceOff, was32 = N32Blocks.ForceOff, was30 = N30Blocks.ForceOff;
            N30Blocks.ForceOff = false;
            N32Blocks.ForceOff = false;
            N33Blocks.ForceOff = false;
            N34Blocks.ForceOff = false;
            N35Blocks.ForceOff = false;
            try
            {
                N36Blocks.ForceOff = false;
                Assert.That(Curriculum25.Order(Cfg(false), all)[0], Is.EqualTo("n35-foundry"), "level36 off: NIGHT35's level 1");
                List<string> on = Curriculum25.Order(Cfg(true), all);
                Assert.That(on[0], Is.EqualTo("n36-foundry"), "level36 on: the from-scratch level 1");
                Assert.That(on[1], Is.EqualTo("n35-iceline"), "level36 on: levels 2-6 unchanged");
                N36Blocks.ForceOff = true;
                Assert.That(Curriculum25.Order(Cfg(true), all)[0], Is.EqualTo("n35-foundry"), "--off36: NIGHT35's level 1");
            }
            finally { N36Blocks.ForceOff = was; N35Blocks.ForceOff = was35; N34Blocks.ForceOff = was34; N33Blocks.ForceOff = was33; N32Blocks.ForceOff = was32; N30Blocks.ForceOff = was30; }
        }

        /// <summary>A format 19 tape's bytes as a format 18 build wrote them: the version 18, no NIGHT36 value (after the NIGHT35 byte). A
        /// tape that is already format 18 or older passes through.</summary>
        public static byte[] AsFormat18(byte[] b19)
        {
            b19 = N37SharedTests.AsFormat19(b19); // NIGHT37: a format 20 tape first loses its NIGHT37 value
            if (BitConverter.ToInt32(b19, RunTape.Magic.Length) < 19) return b19;
            var r = new TapeReader(b19, RunTape.Magic.Length, b19.Length);
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
            int at = r.Position;
            r.VarI(); // n36
            int cut = r.Position - at;
            var b18 = new byte[b19.Length - cut];
            Buffer.BlockCopy(b19, 0, b18, 0, at);
            Buffer.BlockCopy(b19, at + cut, b18, at, b19.Length - at - cut);
            var w = new TapeWriter(18);
            w.U32(18);
            Buffer.BlockCopy(w.data, 0, b18, RunTape.Magic.Length, 4);
            return b18;
        }
    }
}
