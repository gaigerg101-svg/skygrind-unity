// NIGHT38 (the shared commit, the lead): the checks for the NIGHT38 blocks' shared plumbing. Every test here passes with the NIGHT38
// blocks forced off by N37TestBaseline (the whole assembly runs with NIGHT37 and NIGHT38 off unless it hands the sim its own override):
//   1. the tape format 21 carries the NIGHT38 value (a VarI), and a format 20 tape (AsFormat20) reads with every NIGHT38 block off
//   2. N38Blocks round-trips through its bits and off is the default value
//   3. a level row's levelFrom -> levelTo swap applies only while that row is on, and --off38 (ForceOff) gives NIGHT37's order
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N38SharedTests
    {
        [Test]
        public void TheTape_Format21_CarriesTheN38Value()
        {
            var t = new RunTape { n31 = 0b10110101, n36 = 0b110100101, n37 = 0b10110, n38 = 0b10100101, bank33 = 12345, mode = "campaign" };
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(27) /* NIGHT46 rows: format 27 (NightRowsTapeTests.AsFormat26 strips the rows record) */, "NIGHT43: the current format is 26 (N43SharedTests.AsFormat25, N42SharedTests.AsFormat24, N41SharedTests.AsFormat23, N40SharedTests.AsFormat22 and N39SharedTests.AsFormat21 strip their values)");
            RunTape back = RunTape.FromBytes(b);
            Assert.That(back.n38, Is.EqualTo(0b10100101));
            Assert.That(back.n37, Is.EqualTo(0b10110));
            Assert.That(back.bank33, Is.EqualTo(12345));

            RunTape old = RunTape.FromBytes(AsFormat20(b));
            Assert.That(old.n38, Is.EqualTo(0), "format 20: every NIGHT38 block off");
            Assert.That(old.n37, Is.EqualTo(0b10110), "format 20 keeps its NIGHT37 value");
        }

        [Test]
        public void TheBlocks_RoundTripThroughTheirBits_AndOffIsTheDefault()
        {
            Assert.That(N38Blocks.Off.Any, Is.False);
            for (int b = 0; b < (1 << N38Blocks.Count); b++) Assert.That(N38Blocks.FromBits(b).Bits, Is.EqualTo(b));
            Assert.That(N38Blocks.ForceOff, Is.True, "the test assembly runs with the NIGHT38 blocks forced off (N37TestBaseline)");
        }

        [Test]
        public void ALevelRow_SwapsItsLevel_OnlyWhenOn()
        {
            GameConfig Cfg(bool kiln) => new GameConfig(new Dictionary<string, object>
            {
                ["design"] = new Dictionary<string, object>
                {
                    ["n30"] = new Dictionary<string, object> { ["levels"] = new Dictionary<string, object> { ["enabled"] = true, ["order"] = new List<object> { "n36-foundry", "n35-iceline", "n35-kiln", "n33-span", "n33-spire", "n35-finale" } } },
                    ["n38"] = new Dictionary<string, object>
                    {
                        ["kiln38"] = new Dictionary<string, object> { ["enabled"] = kiln, ["levelFrom"] = "n35-kiln", ["levelTo"] = "n38-kiln" },
                    },
                },
            });
            var all = new N25Blocks { tutorial = true, curriculum = true };
            bool was = N38Blocks.ForceOff, was37 = N37Blocks.ForceOff, was36 = N36Blocks.ForceOff, was35 = N35Blocks.ForceOff, was34 = N34Blocks.ForceOff, was33 = N33Blocks.ForceOff, was32 = N32Blocks.ForceOff, was30 = N30Blocks.ForceOff;
            N30Blocks.ForceOff = false;
            N32Blocks.ForceOff = false;
            N33Blocks.ForceOff = false;
            N34Blocks.ForceOff = false;
            N35Blocks.ForceOff = false;
            N36Blocks.ForceOff = false;
            N37Blocks.ForceOff = false;
            try
            {
                N38Blocks.ForceOff = false;
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(false), all)), Is.EqualTo("n36-foundry,n35-iceline,n35-kiln,n33-span,n33-spire,n35-finale"), "rows off: NIGHT37's order (no NIGHT37 row on here)");
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(true), all)), Is.EqualTo("n36-foundry,n35-iceline,n38-kiln,n33-span,n33-spire,n35-finale"), "kiln38 on: level 3 swapped only");
                N38Blocks.ForceOff = true;
                Assert.That(string.Join(",", Curriculum25.Order(Cfg(true), all)), Is.EqualTo("n36-foundry,n35-iceline,n35-kiln,n33-span,n33-spire,n35-finale"), "--off38: NIGHT37's order");
            }
            finally { N38Blocks.ForceOff = was; N37Blocks.ForceOff = was37; N36Blocks.ForceOff = was36; N35Blocks.ForceOff = was35; N34Blocks.ForceOff = was34; N33Blocks.ForceOff = was33; N32Blocks.ForceOff = was32; N30Blocks.ForceOff = was30; }
        }

        /// <summary>A format 21 tape's bytes as a format 20 build wrote them: the version 20, no NIGHT38 value (after the NIGHT37 value). A
        /// tape that is already format 20 or older passes through.</summary>
        public static byte[] AsFormat20(byte[] b21)
        {
            b21 = N39SharedTests.AsFormat21(b21); // NIGHT39: a format 22 tape first loses its NIGHT39 value
            if (BitConverter.ToInt32(b21, RunTape.Magic.Length) < 21) return b21;
            var r = new TapeReader(b21, RunTape.Magic.Length, b21.Length);
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
            int at = r.Position;
            r.VarI(); // n38
            int cut = r.Position - at;
            var b20 = new byte[b21.Length - cut];
            Buffer.BlockCopy(b21, 0, b20, 0, at);
            Buffer.BlockCopy(b21, at + cut, b20, at, b21.Length - at - cut);
            var w = new TapeWriter(20);
            w.U32(20);
            Buffer.BlockCopy(w.data, 0, b20, RunTape.Magic.Length, 4);
            return b20;
        }
    }
}
