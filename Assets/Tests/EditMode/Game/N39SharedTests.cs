// NIGHT39 (the shared commit, the lead): the checks for the NIGHT39 blocks' shared plumbing. Every test here passes with the NIGHT39
// blocks forced off by N37TestBaseline (the Game test namespace runs with NIGHT37, NIGHT38 and NIGHT39 off unless it hands its own override):
//   1. the tape format 22 carries the NIGHT39 value (a VarI), and a format 21 tape (AsFormat21) reads with every NIGHT39 block off
//   2. N39Blocks round-trips through its bits and off is the default value
using System;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N39SharedTests
    {
        [Test]
        public void TheTape_Format22_CarriesTheN39Value()
        {
            var t = new RunTape { n31 = 0b10110101, n36 = 0b110100101, n37 = 0b10110, n38 = 0b10100101, n39 = 0b101, bank33 = 12345, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(27) /* NIGHT46 rows: format 27 (NightRowsTapeTests.AsFormat26 strips the rows record) */, "NIGHT43: the current format is 26 (N43SharedTests.AsFormat25, N42SharedTests.AsFormat24, N41SharedTests.AsFormat23 and N40SharedTests.AsFormat22 strip their values)");
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n39, Is.EqualTo(0b101));
            Assert.That(back.n38, Is.EqualTo(0b10100101));
            Assert.That(back.bank33, Is.EqualTo(12345));

            RunTape old = RunTape.FromBytes(AsFormat21(b));
            Assert.That(old.n39, Is.EqualTo(0), "format 21: every NIGHT39 block off");
            Assert.That(old.n38, Is.EqualTo(0b10100101), "format 21 keeps its NIGHT38 value");
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndOffIsTheDefault()
        {
            Assert.That(N39Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N39Blocks.Count); b++) Assert.That(N39Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N39Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT39 blocks forced off (N37TestBaseline)");
        }

        /// <summary>A format 22 tape's bytes as a format 21 build wrote them: the version 21, no NIGHT39 value (after the NIGHT38 value). A
        /// tape that is already format 21 or older passes through.</summary>
        public static byte[] AsFormat21(byte[] b22)
        {
            b22 = N40SharedTests.AsFormat22(b22); // NIGHT40: a format 23 tape first loses its NIGHT40 value
            if (BitConverter.ToInt32(b22, RunTape.Magic.Length) < 22) return b22;
            var r = new TapeReader(b22, RunTape.Magic.Length, b22.Length);
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
            r.VarI(); // n37
            r.VarI(); // n38
            int at = r.Position;
            r.VarI(); // n39
            int cut = r.Position - at;
            var b21 = new byte[b22.Length - cut];
            Buffer.BlockCopy(b22, 0, b21, 0, at);
            Buffer.BlockCopy(b22, at + cut, b21, at, b22.Length - at - cut);
            var w = new TapeWriter(21);
            w.U32(21);
            Buffer.BlockCopy(w.data, 0, b21, RunTape.Magic.Length, 4);
            return b21;
        }
    }
}
