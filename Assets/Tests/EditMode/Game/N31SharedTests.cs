// NIGHT31 (the lead): the shared commit's rules.
//   1. the tape format 14 carries the NIGHT31 byte, and a format 13 tape (AsFormat13) reads with every NIGHT31 block off
//   2. every test in this assembly runs with the NIGHT31 blocks forced off unless it hands the sim its own override
//   3. N31Blocks round-trips through its bits, and off is the default value
using System;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    [SetUpFixture]
    public sealed class N31TestBaseline
    {
        bool was;

        [OneTimeSetUp]
        public void Up() { was = N31Blocks.ForceOff; N31Blocks.ForceOff = true; }

        [OneTimeTearDown]
        public void Down() { N31Blocks.ForceOff = was; }
    }

    public class N31SharedTests
    {
        [Test]
        public void TheTape_Format14_CarriesTheN31Byte()
        {
            var t = new RunTape { n27 = 0b101101, n29 = 0b100101, n30 = 0b11010011, n31 = 0b10110101, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT32: the current format is 15; the n31 byte is read below
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n31, Is.EqualTo(0b10110101));
            Assert.That(back.n30, Is.EqualTo(0b11010011));
            Assert.That(back.n29, Is.EqualTo(0b100101));
            RunTape old = RunTape.FromBytes(AsFormat13(b));
            Assert.That(old.n31, Is.EqualTo(0), "format 13: every NIGHT31 block off");
            Assert.That(old.n30, Is.EqualTo(0b11010011), "format 13 keeps its NIGHT30 value");
            RunTape older = RunTape.FromBytes(N30SharedTests.AsFormat12(b));
            Assert.That(older.n31, Is.EqualTo(0));
            Assert.That(older.n30, Is.EqualTo(0), "format 12: every NIGHT30 block off");
            Assert.That(older.n29, Is.EqualTo(0b100101));
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndOffIsTheDefault()
        {
            Assert.That(N31Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N31Blocks.Count); b++) Assert.That(N31Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N31Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT31 blocks forced off (N31TestBaseline)");
        }

        /// <summary>A format 14 tape's bytes as a format 13 build wrote them: the version 13, no NIGHT31 byte (after the NIGHT30 byte).
        /// A tape that is already format 13 or older passes through.</summary>
        public static byte[] AsFormat13(byte[] b14)
        {
            b14 = N32SharedTests.AsFormat14(b14); // NIGHT32: a format 15 tape first loses its NIGHT32 byte
            if (BitConverter.ToInt32(b14, RunTape.Magic.Length) < 14) return b14;
            var r = new TapeReader(b14, RunTape.Magic.Length, b14.Length);
            r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
            r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte();
            r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI();
            r.Byte(); // n25
            r.Byte(); // n26
            r.Byte(); // n27
            r.Byte(); // n29
            r.Byte(); // n30
            int at = r.Position;
            r.Byte(); // n31
            var b13 = new byte[b14.Length - 1];
            Buffer.BlockCopy(b14, 0, b13, 0, at);
            Buffer.BlockCopy(b14, at + 1, b13, at, b14.Length - at - 1);
            var w = new TapeWriter(13);
            w.U32(13);
            Buffer.BlockCopy(w.data, 0, b13, RunTape.Magic.Length, 4);
            return b13;
        }
    }
}
