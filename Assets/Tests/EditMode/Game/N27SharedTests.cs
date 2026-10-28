// NIGHT27 (the lead): the shared commit's rules.
//   1. the tape format 11 carries the NIGHT27 byte, and a format 10 tape (AsFormat10) reads with every NIGHT27 block off
//   2. every test in this assembly runs with the NIGHT27 blocks forced off unless it hands the sim its own override (the older nights'
//      premises assume the sim without pulse27 / chargeShots27 / the bosses; the NIGHT27 rows have their own harness lines and tests)
//   3. N27Blocks round-trips through its bits, and off is the default value
using System;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    [SetUpFixture]
    public sealed class N27TestBaseline
    {
        bool was;

        [OneTimeSetUp]
        public void Up() { was = N27Blocks.ForceOff; N27Blocks.ForceOff = true; }

        [OneTimeTearDown]
        public void Down() { N27Blocks.ForceOff = was; }
    }

    public class N27SharedTests
    {
        [Test]
        public void TheTape_Format11_CarriesTheN27Byte()
        {
            var t = new RunTape { n26 = 0b1010011, n27 = 0b101101, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion)); // NIGHT29: the current format is 12; the n27 byte is read below
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n27, Is.EqualTo(0b101101));
            Assert.That(back.n26, Is.EqualTo(0b1010011));
            RunTape old = RunTape.FromBytes(N26SharedTests.AsFormat10(b));
            Assert.That(old.n27, Is.EqualTo(0), "format 10: every NIGHT27 block off");
            Assert.That(old.n26, Is.EqualTo(0b1010011), "format 10 keeps its NIGHT26 value");
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndOffIsTheDefault()
        {
            Assert.That(N27Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N27Blocks.Count); b++) Assert.That(N27Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N27Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT27 blocks forced off (N27TestBaseline)");
        }
    }
}
