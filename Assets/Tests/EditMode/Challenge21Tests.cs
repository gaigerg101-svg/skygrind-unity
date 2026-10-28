// NIGHT21 U3-35.9 beyond row 5, CHALLENGE CODES (builder W2): Core/Challenge21.cs packs a run's seed, HEAT rung, mutators, mode and
// target round into a short typable code with a checksum. Tests-first: UNCOMPILED in Unity tonight (no Editor); the same
// assertions run headless in SimHarness `challenge --selftest` (Tools/SimHarness/NunitMini.cs, which has only Assert.That with
// Is.True / Is.False / Is.EqualTo, so the assertions are written as booleans).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Challenge21Tests
    {
        const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root(), "config.json"));

        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        static Challenge21.Spec Sample() => new Challenge21.Spec
        {
            seed = 123456789, heat = 3, mode = RunSetup.Grind, target = 17,
            mutators = new List<string> { "speedDemon", "glassCannon" },
        };

        static bool Throws(System.Action a)
        {
            try { a(); }
            catch (System.ArgumentException) { return true; }
            return false;
        }

        [Test]
        public void NIGHT24_TheRemovedDailyMode_CannotBeEncoded()
        {
            Challenge21.Spec s = Sample();
            s.mode = "daily";
            Assert.That(Throws(() => Challenge21.Encode(s, Cfg)), Is.True, "mode 1 is retired: a code never carries it");
        }

        [Test]
        public void EncodeIsTwelveCharactersFromTheAlphabetInGroups()
        {
            string code = Challenge21.Encode(Sample(), Cfg);
            Assert.That(code.Length == 12 + 2, Is.True, code); // three groups of four, two hyphens
            Assert.That(code[4] == '-' && code[9] == '-', Is.True, code);
            foreach (char c in code.Replace("-", ""))
                Assert.That(Alphabet.IndexOf(c) >= 0, Is.True, "char " + c);
        }

        [Test]
        public void RoundTripKeepsEveryField()
        {
            Challenge21.Spec s = Sample();
            Assert.That(Challenge21.TryDecode(Challenge21.Encode(s, Cfg), Cfg, out Challenge21.Spec d), Is.True);
            Assert.That(d.seed == s.seed && d.heat == s.heat && d.mode == s.mode && d.target == s.target, Is.True);
            Assert.That(d.mutators.Count == 2 && d.mutators.Contains("speedDemon") && d.mutators.Contains("glassCannon"), Is.True);
        }

        [Test]
        public void MutatorOrderDoesNotChangeTheCode()
        {
            Challenge21.Spec a = Sample(), b = Sample();
            b.mutators = new List<string> { "glassCannon", "speedDemon" };
            Assert.That(Challenge21.Encode(a, Cfg) == Challenge21.Encode(b, Cfg), Is.True);
        }

        [Test]
        public void ExtremesRoundTrip()
        {
            var s = new Challenge21.Spec { seed = 2147483647u, heat = 15, mode = RunSetup.Grind, target = 127, mutators = new List<string>(MutatorMath.All) };
            Assert.That(Challenge21.TryDecode(Challenge21.Encode(s, Cfg), Cfg, out Challenge21.Spec d), Is.True);
            Assert.That(d.seed == 2147483647u && d.heat == 15 && d.target == 127 && d.mode == RunSetup.Grind && d.mutators.Count == MutatorMath.All.Length, Is.True);
            var z = new Challenge21.Spec { seed = 0, heat = 0, mode = RunSetup.Survival, target = 0, mutators = new List<string>() };
            Assert.That(Challenge21.TryDecode(Challenge21.Encode(z, Cfg), Cfg, out d), Is.True);
            Assert.That(d.seed == 0u && d.mode == RunSetup.Survival && d.mutators.Count == 0 && d.heat == 0 && d.target == 0, Is.True);
        }

        [Test]
        public void TypedCodesAreForgiving()
        {
            string code = Challenge21.Encode(Sample(), Cfg);
            string typed = "  " + code.Replace("-", " ").ToLowerInvariant() + " ";
            Assert.That(Challenge21.TryDecode(typed, Cfg, out Challenge21.Spec d), Is.True);
            Assert.That(d.seed == Sample().seed, Is.True);
        }

        [Test]
        public void WrongLengthIsRejected()
        {
            string raw = Challenge21.Encode(Sample(), Cfg).Replace("-", "");
            Assert.That(Challenge21.TryDecode(raw.Substring(0, 11), Cfg, out _), Is.False);
            Assert.That(Challenge21.TryDecode(raw + "0", Cfg, out _), Is.False);
            Assert.That(Challenge21.TryDecode("", Cfg, out _), Is.False);
            Assert.That(Challenge21.TryDecode(null, Cfg, out _), Is.False);
        }

        [Test]
        public void ACharacterOutsideTheAlphabetIsRejected()
        {
            string raw = Challenge21.Encode(Sample(), Cfg).Replace("-", "");
            Assert.That(Challenge21.TryDecode("U" + raw.Substring(1), Cfg, out _), Is.False);
            Assert.That(Challenge21.TryDecode("#" + raw.Substring(1), Cfg, out _), Is.False);
        }

        [Test]
        public void ACorruptedCodeIsRejectedAlmostAlways()
        {
            // About 1 in 1024 passes by chance (checkBits 10): over the 12 x 31 single-character edits of 20 codes, under 0.5 % do.
            int tried = 0, passed = 0;
            for (uint k = 0; k < 20; k++)
            {
                string raw = Challenge21.Encode(new Challenge21.Spec { seed = 1000 + k * 7919, heat = (int)(k % 5), mode = RunSetup.Survival, target = (int)k, mutators = new List<string>() }, Cfg).Replace("-", "");
                for (int i = 0; i < raw.Length; i++)
                    foreach (char c in Alphabet)
                    {
                        if (c == raw[i]) continue;
                        tried++;
                        if (Challenge21.TryDecode(raw.Substring(0, i) + c + raw.Substring(i + 1), Cfg, out _)) passed++;
                    }
            }
            Assert.That(passed * 200 < tried, Is.True, $"{passed} of {tried} edited codes decoded");
        }

        [Test]
        public void OutOfRangeFieldsAreRefusedByEncode()
        {
            Challenge21.Spec s = Sample();
            s.heat = 16;
            Assert.That(Throws(() => Challenge21.Encode(s, Cfg)), Is.True);
            s = Sample();
            s.target = 128;
            Assert.That(Throws(() => Challenge21.Encode(s, Cfg)), Is.True);
            s = Sample();
            s.mode = "nope";
            Assert.That(Throws(() => Challenge21.Encode(s, Cfg)), Is.True);
        }

        [Test]
        public void EnabledReadsTheDesignConfigAndForceOffTurnsItOff()
        {
            bool cfgOn = Cfg.Design("n21.challenge.enabled");
            bool was = N21Blocks.ForceOff;
            try
            {
                N21Blocks.ForceOff = false;
                Assert.That(Challenge21.Enabled(Cfg) == cfgOn, Is.True);
                N21Blocks.ForceOff = true;
                Assert.That(Challenge21.Enabled(Cfg), Is.False);
            }
            finally { N21Blocks.ForceOff = was; }
        }
    }
}
