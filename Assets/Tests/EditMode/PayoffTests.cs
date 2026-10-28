// NIGHT12 U3-25.8: the grade chase (Core/Payoff.cs). The P rule both ways (design.grade.p: an exfil at grade S with an SS
// peak and no revive; any one missing keeps the run's grade; the flag off never gives P) and the PAYOFF strip's
// personal-best line in records.js's voice.
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public sealed class PayoffTests
    {
        static GameConfig cfg;
        static Payoff.P p;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            p = Payoff.P.From(cfg);
        }

        static RunStats PRun() => new RunStats { round = 21, exfil = Exfil.Win, grade = "S", bestRankIndex = 5, revived = false };

        [Test]
        public void P_ReadsTheRule_TheSSPeakIsTheTopRank()
        {
            Assert.That(p.enabled, Is.True);
            Assert.That(p.grade, Is.EqualTo("S"));
            var ranks = cfg.Arr("style.ranks");
            Assert.That((string)PortData.Obj(ranks[p.peak])["letter"], Is.EqualTo("SS"));
            Assert.That(p.peak, Is.EqualTo(ranks.Count - 1));
        }

        [Test]
        public void P_AnExfilAtS_WithAnSSPeak_AndNoRevive()
        {
            Assert.That(Payoff.GradeWithP(PRun(), p), Is.EqualTo("P"));
            Assert.That(Payoff.IsP(PRun(), p), Is.True);
            Assert.That(Profile.GradeRank("P"), Is.GreaterThan(Profile.GradeRank("S")), "P is kept as the rung's best over S");
        }

        [Test]
        public void P_AnyConditionMissing_KeepsTheRunsGrade()
        {
            RunStats died = PRun();
            died.exfil = null;
            Assert.That(Payoff.GradeWithP(died, p), Is.EqualTo("S"));
            RunStats failed = PRun();
            failed.exfil = Exfil.Lost;
            Assert.That(Payoff.GradeWithP(failed, p), Is.EqualTo("S"));
            RunStats gradeA = PRun();
            gradeA.grade = "A";
            Assert.That(Payoff.GradeWithP(gradeA, p), Is.EqualTo("A"));
            RunStats peakS = PRun();
            peakS.bestRankIndex = 4;
            Assert.That(Payoff.GradeWithP(peakS, p), Is.EqualTo("S"));
            RunStats revived = PRun();
            revived.revived = true;
            Assert.That(Payoff.GradeWithP(revived, p), Is.EqualTo("S"));
            var off = new Payoff.P { enabled = false, grade = "S", peak = p.peak };
            Assert.That(Payoff.GradeWithP(PRun(), off), Is.EqualTo("S"));
            Assert.That(Payoff.GradeWithP(PRun(), null), Is.EqualTo("S"));
        }

        [Test]
        public void BestDelta_InRecordsVoice_NullWithoutABest()
        {
            var s = new RunStats { round = 9, earned = 4200 };
            Assert.That(Payoff.BestDelta(s, null), Is.Null);
            Assert.That(Payoff.BestDelta(s, new Records.Run { round = 7, points = 9000 }), Is.EqualTo("ROUND 9 · +2 ON YOUR BEST"));
            Assert.That(Payoff.BestDelta(s, new Records.Run { round = 12, points = 100 }), Is.EqualTo("BEST ROUND 12 · 3 SHORT"));
            Assert.That(Payoff.BestDelta(s, new Records.Run { round = 9, points = 4000 }), Is.EqualTo("ROUND 9 · +200 POINTS ON YOUR BEST"));
            Assert.That(Payoff.BestDelta(s, new Records.Run { round = 9, points = 4500 }), Is.EqualTo("BEST ROUND 9 · 300 POINTS SHORT"));
            Assert.That(Payoff.BestDelta(s, new Records.Run { round = 9, points = 4200 }), Is.EqualTo("ROUND 9 · YOUR BEST MATCHED"));
        }
    }
}
