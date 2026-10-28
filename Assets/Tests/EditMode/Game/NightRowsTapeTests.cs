// NIGHT45 builder S1 (rows as data; NIGHT45.md card S1): the tape side of the rows record (Game/Tape/TapeRows.cs), and AsFormat26, the
// test chain's last link, pre-placed: NIGHT46's switch-on (Tools/night/rows-switch-on.cjs) bumps RunTape to format 27 ONCE and chains
// N43SharedTests.AsFormat25 through AsFormat26; no later night adds a link for a row. No UnityEngine here, so it also runs headless:
// `dotnet run --project Tools/SimHarness -c Release -p:Extra=../../Assets/Tests/EditMode/Game/NightRowsTapeTests.cs -- n45rows selftest`.
using System;
using System.Reflection;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class NightRowsTapeTests
    {
        static NightRows Demo() => NightRows.FromDesign(PortData.Obj(JsJson.Parse(@"{ ""n46"": { ""wall46"": { ""sim"": true, ""enabled"": true, ""_why"": ""t"" }, ""pour46"": { ""sim"": true, ""enabled"": false, ""_why"": ""t"" } }, ""n47"": { ""grip47"": { ""sim"": true, ""enabled"": true, ""_why"": ""t"" } } }")));

        [Test]
        public void TheRecord_RoundTripsInsideATapeStream()
        {
            NightRows reg = Demo();
            byte[] rec = NightRowsTape.Encode(reg.AllOff().Set("wall46", true).Set("grip47", true));
            var w = new TapeWriter(16);
            w.VarI(-12345);
            TapeRows.Write(w, rec);
            w.D(100.5);
            TapeRows.Write(w, null);
            w.Str("tail");
            byte[] bytes = new byte[w.length];
            Buffer.BlockCopy(w.data, 0, bytes, 0, w.length);
            var r = new TapeReader(bytes, 0, bytes.Length);
            Assert.That(r.VarI(), Is.EqualTo(-12345));
            NightRowState back = TapeRows.Replay(reg, TapeRows.Read(bytes, r));
            Assert.That(back.ToString(), Is.EqualTo("wall46, grip47"));
            Assert.That(r.D(), Is.EqualTo(100.5), "the value after the record is intact");
            Assert.That(TapeRows.Read(bytes, r), Is.Null, "an empty record reads as none");
            Assert.That(r.Str(), Is.EqualTo("tail"));
            Assert.That(r.More, Is.False);
            Assert.That(TapeRows.Replay(reg, null).Any, Is.False, "no record: every row off");
        }

        [Test]
        public void AReplayOfARowTheBuildLacks_IsATapeLine()
        {
            byte[] rec = NightRowsTape.Encode(Demo().AllOff().Set("grip47", true));
            NightRows lacking = NightRows.FromDesign(PortData.Obj(JsJson.Parse(@"{ ""n46"": { ""wall46"": { ""sim"": true, ""enabled"": true, ""_why"": ""t"" } } }")));
            string msg = null;
            try { TapeRows.Replay(lacking, rec); }
            catch (TapeFormatException e) { msg = e.Message; }
            Assert.That(msg != null && msg.Contains("grip47"), Is.True, "refused as a TapeFormatException naming the row (" + (msg ?? "not refused") + ")");

            var w = new TapeWriter(8);
            w.VarU(200); // a length past the end
            byte[] b = new byte[w.length];
            Buffer.BlockCopy(w.data, 0, b, 0, w.length);
            string cut = null;
            try { TapeRows.Read(b, new TapeReader(b, 0, b.Length)); }
            catch (TapeFormatException e) { cut = e.Message; }
            Assert.That(cut != null, Is.True, "a truncated record is a tape line");
        }

        /// <summary>Tonight (format 26, unwired): RunTape has no rows field and AsFormat26 passes every tape through. From NIGHT46's switch-on
        /// (format 27): the field round-trips, and AsFormat26 strips it to a format 26 tape that reads with every row off.</summary>
        [Test]
        public void TheRunTape_CarriesTheRecord_FromFormat27()
        {
            FieldInfo f = typeof(RunTape).GetField("rows");
            var t = new RunTape { n43 = 0b101101001011, bank33 = 12345, mode = "campaign" };
            if (RunTape.FormatVersion < NightRowsTape.FirstTapeFormat)
            {
                Assert.That(RunTape.FormatVersion, Is.EqualTo(26), "NIGHT45: unwired, the tape is NIGHT43's format 26");
                Assert.That(f == null, Is.True, "no rows field before the switch-on");
                byte[] b26 = t.ToBytes();
                Assert.That(AsFormat26(b26) == b26, Is.True, "a format 26 tape passes through");
                return;
            }
            Assert.That(f != null, Is.True, "format 27: RunTape.rows");
            byte[] rec = NightRowsTape.Encode(Demo().AllOff().Set("wall46", true));
            f.SetValue(t, rec);
            byte[] b = t.ToBytes();
            Assert.That(BitConverter.ToInt32(b, RunTape.Magic.Length), Is.EqualTo(RunTape.FormatVersion));
            RunTape back = RunTape.FromBytes(b);
            Assert.That(TapeRows.Replay(Demo(), (byte[])f.GetValue(back)).ToString(), Is.EqualTo("wall46"));
            Assert.That(back.n43, Is.EqualTo(0b101101001011));
            Assert.That(back.bank33, Is.EqualTo(12345));
            RunTape old = RunTape.FromBytes(AsFormat26(b));
            Assert.That(f.GetValue(old) == null, Is.True, "format 26: no record, every row off");
            Assert.That(old.n43, Is.EqualTo(0b101101001011), "format 26 keeps its NIGHT43 value");
        }

        /// <summary>A format 27+ tape's bytes as a format 26 build wrote them: the version 26, no rows record (after the NIGHT43 value). A tape
        /// that is already format 26 or older passes through (the same array). N43SharedTests.AsFormat25 calls this first from the switch-on.</summary>
        public static byte[] AsFormat26(byte[] b27)
        {
            if (BitConverter.ToInt32(b27, RunTape.Magic.Length) < NightRowsTape.FirstTapeFormat) return b27;
            var r = new TapeReader(b27, RunTape.Magic.Length, b27.Length);
            r.U32(); r.Str(); r.Str(); r.U32(); r.Str(); r.VarI(); r.U64();
            r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte();
            r.Byte(); r.Byte(); r.Str(); r.VarI(); r.VarI(); r.VarI();
            r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); r.Byte(); // n25 n26 n27 n29 n30 n31 n32
            r.Byte(); r.VarI(); r.VarI(); // n33 shop33 bank33
            r.Byte(); r.Byte(); // n34 n35
            r.VarI(); r.VarI(); r.VarI(); r.VarI(); r.VarI(); r.VarI(); r.VarI(); r.VarI(); // n36 ... n43
            int at = r.Position;
            uint n = r.VarU(); // the rows record's length ...
            r.Skip((int)n); // ... and its bytes
            int cut = r.Position - at;
            var b26 = new byte[b27.Length - cut];
            Buffer.BlockCopy(b27, 0, b26, 0, at);
            Buffer.BlockCopy(b27, at + cut, b26, at, b27.Length - at - cut);
            var w = new TapeWriter(4);
            w.U32(26);
            Buffer.BlockCopy(w.data, 0, b26, RunTape.Magic.Length, 4);
            return b26;
        }
    }
}
