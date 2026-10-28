// NIGHT29 (the lead): the shared commit's rules.
//   1. the tape format 12 carries the NIGHT29 byte, and a format 11 tape (AsFormat11) reads with every NIGHT29 block off
//   2. every test in this assembly runs with the NIGHT29 blocks forced off unless it hands the sim its own override
//   3. N29Blocks round-trips through its bits, and off is the default value
using System;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    [SetUpFixture]
    public sealed class N29TestBaseline
    {
        bool was;

        [OneTimeSetUp]
        public void Up() { was = N29Blocks.ForceOff; N29Blocks.ForceOff = true; }

        [OneTimeTearDown]
        public void Down() { N29Blocks.ForceOff = was; }
    }

    public class N29SharedTests
    {
        [Test]
        public void TheTape_Format12_CarriesTheN29Byte()
        {
            var t = new RunTape { n27 = 0b101101, n29 = 0b100101, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT30: the current format is 13; the n29 byte is read below
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n29, Is.EqualTo(0b100101));
            Assert.That(back.n27, Is.EqualTo(0b101101));
            RunTape old = RunTape.FromBytes(AsFormat11(b));
            Assert.That(old.n29, Is.EqualTo(0), "format 11: every NIGHT29 block off");
            Assert.That(old.n27, Is.EqualTo(0b101101), "format 11 keeps its NIGHT27 value");
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndOffIsTheDefault()
        {
            Assert.That(N29Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N29Blocks.Count); b++) Assert.That(N29Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N29Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT29 blocks forced off (N29TestBaseline)");
        }

        /// <summary>A format 12 tape's bytes as a format 11 build wrote them: the version 11, no NIGHT29 byte (after the NIGHT27 byte).
        /// A tape that is already format 11 or older passes through.</summary>
        public static byte[] AsFormat11(byte[] b12)
        {
            b12 = N30SharedTests.AsFormat12(b12); // NIGHT30: a format 13 tape first loses its NIGHT30 byte
            if (BitConverter.ToInt32(b12, RunTape.Magic.Length) < 12) return b12;
            var r = new TapeReader(b12, RunTape.Magic.Length, b12.Length);
            r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
            r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte();
            r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI();
            r.Byte(); // n25
            r.Byte(); // n26
            r.Byte(); // n27
            int at = r.Position;
            r.Byte(); // n29
            var b11 = new byte[b12.Length - 1];
            Buffer.BlockCopy(b12, 0, b11, 0, at);
            Buffer.BlockCopy(b12, at + 1, b11, at, b12.Length - at - 1);
            var w = new TapeWriter(11);
            w.U32(11);
            Buffer.BlockCopy(w.data, 0, b11, RunTape.Magic.Length, 4);
            return b11;
        }
    }
}
