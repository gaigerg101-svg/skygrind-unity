// NIGHT30 (the lead): the shared commit's rules.
//   1. the tape format 13 carries the NIGHT30 byte, and a format 12 tape (AsFormat12) reads with every NIGHT30 block off
//   2. every test in this assembly runs with the NIGHT30 blocks forced off unless it hands the sim its own override
//   3. N30Blocks round-trips through its bits, and off is the default value
using System;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    [SetUpFixture]
    public sealed class N30TestBaseline
    {
        bool was;

        [OneTimeSetUp]
        public void Up() { was = N30Blocks.ForceOff; N30Blocks.ForceOff = true; }

        [OneTimeTearDown]
        public void Down() { N30Blocks.ForceOff = was; }
    }

    public class N30SharedTests
    {
        [Test]
        public void TheTape_Format13_CarriesTheN30Byte()
        {
            var t = new RunTape { n27 = 0b101101, n29 = 0b100101, n30 = 0b11010011, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT31: the current format is 14; the n30 byte is read below
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n30, Is.EqualTo(0b11010011));
            Assert.That(back.n29, Is.EqualTo(0b100101));
            Assert.That(back.n27, Is.EqualTo(0b101101));
            RunTape old = RunTape.FromBytes(AsFormat12(b));
            Assert.That(old.n30, Is.EqualTo(0), "format 12: every NIGHT30 block off");
            Assert.That(old.n29, Is.EqualTo(0b100101), "format 12 keeps its NIGHT29 value");
            RunTape older = RunTape.FromBytes(N29SharedTests.AsFormat11(b));
            Assert.That(older.n30, Is.EqualTo(0));
            Assert.That(older.n29, Is.EqualTo(0), "format 11: every NIGHT29 block off");
            Assert.That(older.n27, Is.EqualTo(0b101101));
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndOffIsTheDefault()
        {
            Assert.That(N30Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N30Blocks.Count); b++) Assert.That(N30Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N30Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT30 blocks forced off (N30TestBaseline)");
        }

        /// <summary>A format 13 tape's bytes as a format 12 build wrote them: the version 12, no NIGHT30 byte (after the NIGHT29 byte).
        /// A tape that is already format 12 or older passes through.</summary>
        public static byte[] AsFormat12(byte[] b13)
        {
            b13 = N31SharedTests.AsFormat13(b13); // NIGHT31: a format 14 tape first loses its NIGHT31 byte
            if (BitConverter.ToInt32(b13, RunTape.Magic.Length) < 13) return b13;
            var r = new TapeReader(b13, RunTape.Magic.Length, b13.Length);
            r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
            r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte();
            r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI();
            r.Byte(); // n25
            r.Byte(); // n26
            r.Byte(); // n27
            r.Byte(); // n29
            int at = r.Position;
            r.Byte(); // n30
            var b12 = new byte[b13.Length - 1];
            Buffer.BlockCopy(b13, 0, b12, 0, at);
            Buffer.BlockCopy(b13, at + 1, b12, at, b13.Length - at - 1);
            var w = new TapeWriter(12);
            w.U32(12);
            Buffer.BlockCopy(w.data, 0, b12, RunTape.Magic.Length, 4);
            return b12;
        }
    }
}
