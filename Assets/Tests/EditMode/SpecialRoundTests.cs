// NIGHT20 (the shared commit places this empty fixture; E owns it): the special rounds (design.director.n20 / design.adds): THE HUNT on rounds 8, 13, 18 (a count, spawns in a ring, LAST ONE, the drop), THE ELITE; the specialRound event's phases.
// Builder E (U3-34.9 A2 + A4, tests first): the pure rules (Core/Enemies/SpecialRounds.cs: SpecialRoundMath); the sim's
// rounds (the events, the ring, the elite's spawn and drop) are Game/SpecialRoundSimTests.cs. The numbers are
// design.adds.hunt / design.adds.elite (Design/unity-config.json beside PortExport/) and the web's director.rounds.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class SpecialRoundTests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));
        static DirectorMath.D D => DirectorMath.From(Cfg);

        [Test]
        public void TheBlocksNumbers()
        {
            SpecialRoundMath.HuntP h = SpecialRoundMath.HuntP.From(Cfg);
            Assert.That(h.countTime, Is.EqualTo(3), "a 3 s count in the break");
            Assert.That(h.ringMin, Is.GreaterThan(0));
            Assert.That(h.ringMax, Is.GreaterThan(h.ringMin));
            SpecialRoundMath.EliteP e = SpecialRoundMath.EliteP.From(Cfg);
            Assert.That(e.from, Is.EqualTo(8));
            Assert.That((e.gapMin, e.gapMax), Is.EqualTo((3, 5)));
            Assert.That(e.kind, Is.EqualTo("charger"));
            Assert.That(e.affixes, Is.EqualTo(2));
        }

        [Test]
        public void TheHunt_IsTheWebsSwarmRound_8_13_18()
        {
            foreach (int r in new[] { 8, 13, 18, 23 }) Assert.That(SpecialRoundMath.IsHunt(r, D), Is.True, "round " + r);
            foreach (int r in new[] { 1, 5, 7, 9, 10, 12, 15, 20 }) Assert.That(SpecialRoundMath.IsHunt(r, D), Is.False, "round " + r);
        }

        [Test]
        public void TheEliteSchedule_From8_Gaps3To5_NeverOnABossOrHuntRound_Seeded()
        {
            SpecialRoundMath.EliteP p = SpecialRoundMath.EliteP.From(Cfg);
            DirectorMath.D d = D;
            var seen = new HashSet<string>();
            for (uint seed = 1; seed <= 50; seed++)
            {
                List<int> rounds = SpecialRoundMath.EliteRounds(seed, 60, p, d);
                Assert.That(rounds.Count, Is.GreaterThan(5), "seed " + seed);
                Assert.That(rounds[0], Is.GreaterThanOrEqualTo(8));
                int prev = 0;
                foreach (int r in rounds)
                {
                    Assert.That(DirectorMath.IsBossRound(r, d) || DirectorMath.IsSwarmRound(r, d), Is.False, $"seed {seed}: round {r} is a boss or hunt round");
                    if (prev > 0) Assert.That(r - prev, Is.GreaterThanOrEqualTo(3), $"seed {seed}: {prev} -> {r}");
                    prev = r;
                }
                Assert.That(SpecialRoundMath.EliteRounds(seed, 60, p, d), Is.EqualTo(rounds), "seeded: the same seed, the same schedule");
                seen.Add(string.Join(",", rounds));
                // a gap past 5 only where a boss or hunt round pushed it on
                for (int i = 1; i < rounds.Count; i++)
                {
                    if (rounds[i] - rounds[i - 1] <= 5) continue;
                    bool pushed = false;
                    for (int r = rounds[i - 1] + 3; r < rounds[i]; r++) pushed |= DirectorMath.IsBossRound(r, d) || DirectorMath.IsSwarmRound(r, d);
                    Assert.That(pushed, Is.True, $"seed {seed}: a {rounds[i] - rounds[i - 1]}-round gap with nothing in the way");
                }
            }
            Assert.That(seen.Count, Is.GreaterThan(5), "the schedule varies by seed");
        }

        [Test]
        public void TheElitesAffixes_AreTwoDistinct_OfTheWebs()
        {
            var rng = new Mulberry32(3);
            var output = new List<string>();
            for (int i = 0; i < 40; i++)
            {
                SpecialRoundMath.EliteAffixes(rng.Next, 2, output);
                Assert.That(output.Count, Is.EqualTo(2));
                Assert.That(output[0], Is.Not.EqualTo(output[1]));
                foreach (string a in output) Assert.That(AffixMath.Names, Does.Contain(a));
            }
        }

        [Test]
        public void TheTwists_EveryFourthOrdinaryRoundFrom9_SeededDraw_NeverTwiceInARow()
        {
            SpecialRoundMath.TwistP p = SpecialRoundMath.TwistP.From(Cfg);
            Assert.That(p.from, Is.EqualTo(9));
            Assert.That(p.every, Is.EqualTo(4));
            Assert.That(p.ids, Is.EqualTo(new[] { "fog", "swarmageddon", "lethal", "golden" }));
            DirectorMath.D d = D;
            var elites = new List<int> { 11, 14 };
            var seen = new HashSet<string>();
            for (uint seed = 1; seed <= 40; seed++)
            {
                var twists = SpecialRoundMath.TwistRounds(seed, 60, p, d, elites);
                Assert.That(twists.Count, Is.GreaterThan(4));
                // the ordinary rounds from 9: no boss, no hunt, no elite; every 4th of them, the first at 9
                var ordinary = new List<int>();
                for (int r = p.from; r <= 60; r++)
                    if (!DirectorMath.IsBossRound(r, d) && !DirectorMath.IsSwarmRound(r, d) && !elites.Contains(r)) ordinary.Add(r);
                for (int i = 0; i < twists.Count; i++)
                {
                    Assert.That(twists[i].round, Is.EqualTo(ordinary[i * p.every]), $"seed {seed}: twist {i}");
                    Assert.That(p.ids, Does.Contain(twists[i].id));
                    if (i > 0) Assert.That(twists[i].id, Is.Not.EqualTo(twists[i - 1].id), $"seed {seed}: never twice in a row");
                }
                Assert.That(SpecialRoundMath.TwistRounds(seed, 60, p, d, elites), Is.EqualTo(twists), "seeded");
                seen.Add(twists[0].id + twists[1].id);
            }
            Assert.That(seen.Count, Is.GreaterThan(3), "the draw varies by seed");
        }

        [Test]
        public void TheRing_KeepsItsBand()
        {
            SpecialRoundMath.HuntP h = SpecialRoundMath.HuntP.From(Cfg);
            Assert.That(SpecialRoundMath.InRing(h.ringMin, h), Is.True);
            Assert.That(SpecialRoundMath.InRing(h.ringMax, h), Is.True);
            Assert.That(SpecialRoundMath.InRing(h.ringMin - 0.01, h), Is.False, "never inside the min distance");
            Assert.That(SpecialRoundMath.InRing(h.ringMax + 0.01, h), Is.False);
        }
    }
}
