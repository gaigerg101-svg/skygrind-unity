// Golden vectors for the director math (PortExport/vectors/directorMath.json): roundBudget, roundHpMult, isBossRound,
// isSwarmRound, roundKinds, phaseSpawnMult, outOfView, behind, and planSquads with every kind available (the recorded
// runs; tonight's game passes husk-only availability). Numbers bit for bit. The evaluation is Skygrind.Core's
// GoldenDirectorMath (NIGHT9 U3-04, GoldenAi.cs), the code the Player's -skygrindGolden runs too, one part per test here;
// its whole-set checker also walks createDirectorState, the stepDirector / stepCamp sequences and PHASES.
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class DirectorGoldenTests
    {
        [Test]
        public void Rounds()
        {
            PortGolden.Check("directorMath rounds", GoldenDirectorMath.Rounds);
            Assert.That(PortGolden.Data.Cases("directorMath", "roundBudget").Count, Is.EqualTo(40));
        }

        [Test]
        public void PhaseMult_OutOfView_Behind() => PortGolden.Check("directorMath phaseSpawnMult + outOfView + behind", GoldenDirectorMath.PhaseMultOutOfViewBehind);

        [Test]
        public void PlanSquads_TheRecordedDeals()
        {
            GoldenResult r = PortGolden.Check("directorMath planSquads", GoldenDirectorMath.PlanSquads);
            Assert.That(r.cases, Is.EqualTo(16));
        }
    }
}
