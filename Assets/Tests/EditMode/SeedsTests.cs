// NIGHT12 U3-25.9: seeds.js. seeds.test.js's seed tests, ported as they are (the web's expected values); values computed by V8
// (node on the frozen web repo's src/modes/seeds.js, 2026-10-03, read only) for the exact codes and edge-case parses.
// NIGHT24 remove-daily-weekly: this file was DailyTests.cs; the DAILY's tests (dailySeed, the twist, the counted run and the
// streak, the first mod) went with the DAILY (listed in the commit); the seed-code tests are unchanged.
using System;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public sealed class SeedsTests
    {
        // --- seeds.test.js: seed codes (OVERNIGHT3 P7) ---------------------------------------------------------------------

        [Test]
        public void ASeed_RoundTripsThroughItsCode_JunkIsRefused()
        {
            foreach (uint s in new uint[] { 0, 1, 123456, 2147483647 }) Assert.That(Seeds.ParseSeed(Seeds.SeedCode(s)), Is.EqualTo(s));
            Assert.That(Seeds.SeedCode(35), Is.EqualTo("00000Z"));
            Assert.That(Seeds.ParseSeed(" 4f2k9a "), Is.EqualTo(267178510)); // parseInt('4F2K9A', 36) in V8
            Assert.That(Seeds.ParseSeed(""), Is.Null);
            Assert.That(Seeds.ParseSeed("NOT A SEED"), Is.Null);
            Assert.That(Seeds.ParseSeed("ZZZZZZZZ"), Is.Null); // too long
        }

        // --- V8's values ---

        [Test]
        public void SeedCodes_MatchV8()
        {
            Assert.That(Seeds.SeedCode(0), Is.EqualTo("000000"));
            Assert.That(Seeds.SeedCode(36), Is.EqualTo("000010"));
            Assert.That(Seeds.SeedCode(123456), Is.EqualTo("002N9C"));
            Assert.That(Seeds.SeedCode(2147483647), Is.EqualTo("ZIK0ZJ"));
            Assert.That(Seeds.SeedCode(4294967295), Is.EqualTo("1Z141Z3"));
        }

        [Test]
        public void ParseSeed_TrimsAndUpperCasesAsJs()
        {
            string S(params int[] cs)
            {
                var a = new char[cs.Length];
                for (int i = 0; i < cs.Length; i++) a[i] = (char)cs[i];
                return new string(a);
            }
            Assert.That(Seeds.ParseSeed(S(0xDF)), Is.EqualTo(1036), "sharp s upper-cases to SS");
            Assert.That(Seeds.ParseSeed(S(0xFEFF, '1', '2', 0xA0)), Is.EqualTo(38), "JS trims U+FEFF and NBSP");
            Assert.That(Seeds.ParseSeed(S(0x85, '1', '2')), Is.Null, "JS doesn't trim U+0085");
            Assert.That(Seeds.ParseSeed(S(0x131)), Is.EqualTo(18), "dotless i upper-cases to I");
            Assert.That(Seeds.ParseSeed("1234567"), Is.Null);
            Assert.That(Seeds.ParseSeed("ZZZZZZZ"), Is.Null);
            Assert.That(Seeds.ParseSeed("ZIK0ZK"), Is.Null, "2^31");
            Assert.That(Seeds.ParseSeed("ZIK0ZJ"), Is.EqualTo(2147483647));
            Assert.That(Seeds.ParseSeed("-1"), Is.Null);
            Assert.That(Seeds.ParseSeed("+1"), Is.Null);
            Assert.That(Seeds.ParseSeed("1 2"), Is.Null);
            Assert.That(Seeds.ParseSeed(null), Is.Null);
        }
    }
}
