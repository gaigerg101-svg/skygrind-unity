// NIGHT21 U3-35.5 THE STAMP (builder S): the round grade's pure rules (Core/Grades21.cs) and its tracker on a bare event bus.
// The numbers are design.n21.stamp (Design/unity-config.json beside PortExport/); the letter bands are config.results.grade;
// the round P is design.grade.p's rule applied to a round. The sim's wiring (SurvivalSim.Stamp21.cs) is the SimHarness `stamp`
// command's (the grade per round of a headless run).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class Stamp21Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));
        static Grades21.Params P => Grades21.Params.From(Cfg);
        static RunMoments.Grade Bands => RunMoments.Grade.From(Cfg);
        static int Top => Cfg.Arr("style.ranks").Count - 1;

        [Test]
        public void TheNumbers_WeightsSumToOne_AndTheBandsAreTheRunGrades()
        {
            Grades21.Params p = P;
            Assert.That(p.damageWeight + p.rankWeight + p.timeWeight, Is.EqualTo(1).Within(1e-9));
            Assert.That(p.damageFull, Is.GreaterThan(0));
            Assert.That(p.slowMult, Is.GreaterThan(1));
            Assert.That(Bands.letters, Is.EqualTo(new List<string> { "S", "A", "B", "C" }), "the run grade's bands, reused");
        }

        [Test]
        public void ACleanFastTopRankRound_IsS_AndAnUntouchedLowRankOneIsNot()
        {
            Grades21.Params p = P;
            double par = Grades21.Par(10, p);
            double best = Grades21.Score(0, 100, Top, Top, par, 10, p);
            Assert.That(best, Is.EqualTo(1).Within(1e-9));
            Assert.That(Grades21.Letter(best, Bands), Is.EqualTo("S"));
            double flat = Grades21.Score(0, 100, 0, Top, par, 10, p);
            Assert.That(Grades21.Letter(flat, Bands), Is.Not.EqualTo("S"), "no style: not an S round");
        }

        [Test]
        public void TheScore_FallsWithDamage_AndWithSlowness_AndRisesWithRank()
        {
            Grades21.Params p = P;
            double par = Grades21.Par(10, p);
            double a = Grades21.Score(0, 100, 3, Top, par, 10, p);
            double hurt = Grades21.Score(30, 100, 3, Top, par, 10, p);
            double slow = Grades21.Score(0, 100, 3, Top, par * 2, 10, p);
            double ranked = Grades21.Score(0, 100, 4, Top, par, 10, p);
            Assert.That(hurt, Is.LessThan(a));
            Assert.That(slow, Is.LessThan(a));
            Assert.That(ranked, Is.GreaterThan(a));
            Assert.That(Grades21.Score(1000, 100, 0, Top, par * 10, 10, p), Is.EqualTo(0).Within(1e-9), "each part held to 0..1");
        }

        [Test]
        public void TheRoundP_IsTheRunPRule_GradeS_PeakSS_NoRevive()
        {
            Payoff.P rule = Payoff.P.From(Cfg);
            Assert.That(rule.enabled, Is.True, "design.grade.p is on in the design config");
            Assert.That(Grades21.IsRoundP(rule.grade, rule.peak, false, rule), Is.True);
            Assert.That(Grades21.IsRoundP(rule.grade, rule.peak, true, rule), Is.False, "a revive used: no P");
            Assert.That(Grades21.IsRoundP(rule.grade, rule.peak - 1, false, rule), Is.False, "the peak under SS: no P");
            Assert.That(Grades21.IsRoundP("A", rule.peak, false, rule), Is.False, "a grade under the rule's: no P");
            var off = new Payoff.P { enabled = false, grade = rule.grade, peak = rule.peak };
            Assert.That(Grades21.IsRoundP(rule.grade, rule.peak, false, off), Is.False, "the rule off: no round P");
            Assert.That(Grades21.IsRoundP(rule.grade, rule.peak, false, null), Is.False);
        }

        sealed class Rig
        {
            public readonly EventBus bus = new EventBus();
            public int rank;
            public bool revived;
            public readonly RoundGrader21 grader;
            public readonly List<RoundGradeEvent> said = new List<RoundGradeEvent>();

            public Rig(bool on)
            {
                grader = new RoundGrader21(Cfg, bus, () => rank, () => 100, () => revived);
                bus.On<RoundGradeEvent>("roundGrade", e => said.Add(e));
                grader.NewRun(on);
            }

            public void Round(int number, int kills, double seconds, double damage, int peak)
            {
                rank = 0;
                bus.Emit("roundBegin", new RoundEvent { number = number });
                for (int i = 0; i < kills; i++) bus.Emit("kill", new KillEvent());
                if (damage > 0) bus.Emit("hurt", new HurtEvent { owner = "player", amount = damage });
                bus.Emit("hurt", new HurtEvent { owner = "enemy", amount = 999 }); // not the player's
                int steps = (int)(seconds / Cfg.Dt);
                for (int i = 0; i < steps; i++)
                {
                    if (i == steps / 2) rank = peak;
                    grader.Step();
                }
                bus.Emit("roundClear", new RoundEvent { number = number });
            }
        }

        [Test]
        public void TheGrader_SaysRoundGradeAtEachClear_WithTheRoundsOwnNumbers()
        {
            var rig = new Rig(true);
            double par = Grades21.Par(10, P);
            rig.Round(3, 10, par, 0, Top);
            Assert.That(rig.said.Count, Is.EqualTo(1));
            RoundGradeEvent e = rig.said[0];
            Assert.That(e.round, Is.EqualTo(3));
            Assert.That(e.kills, Is.EqualTo(10));
            Assert.That(e.damage, Is.EqualTo(0));
            Assert.That(e.peakRank, Is.EqualTo(Top));
            Assert.That(e.time, Is.EqualTo(par).Within(Cfg.Dt * 2));
            Assert.That(e.letter, Is.EqualTo("S"));
            Assert.That(e.grade, Is.EqualTo("P"), "S at SS with no revive: the round's P");
            Assert.That(e.p, Is.True);
            Assert.That(rig.grader.Rounds.Count, Is.EqualTo(1));
            Assert.That(rig.grader.Last, Is.SameAs(e));
        }

        [Test]
        public void TheGrader_OnlyThePlayersDamageCounts_AndARevivedRunNeverPs()
        {
            var rig = new Rig(true);
            double par = Grades21.Par(8, P);
            rig.Round(1, 8, par, 40, Top);
            Assert.That(rig.said[0].damage, Is.EqualTo(40), "the enemy's hurt event is not the player's");
            Assert.That(rig.said[0].p, Is.False, "damage drops the round under S");
            rig.revived = true;
            rig.Round(2, 8, par, 0, Top);
            Assert.That(rig.said[1].letter, Is.EqualTo("S"));
            Assert.That(rig.said[1].p, Is.False, "a revive used this run: no round P");
            Assert.That(rig.said[1].grade, Is.EqualTo("S"));
        }

        [Test]
        public void TheGrader_Off_SaysNothing_AndANewRunClearsTheRounds()
        {
            var off = new Rig(false);
            off.Round(1, 5, 10, 0, Top);
            Assert.That(off.said, Is.Empty, "the row off: NIGHT20's game says nothing");
            Assert.That(off.grader.Rounds, Is.Empty);
            var on = new Rig(true);
            on.Round(1, 5, 10, 0, 2);
            Assert.That(on.said.Count, Is.EqualTo(1));
            on.grader.NewRun(true);
            Assert.That(on.grader.Rounds, Is.Empty);
            Assert.That(on.grader.Last, Is.Null);
        }
    }
}
