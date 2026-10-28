// Golden vectors for the aim-assist math (PortExport/vectors/assistMath.json): every recorded call of assistMath.js
// replayed through AssistMath, numbers bit for bit (results, and the mutated arguments where recorded), plus the
// recorded springStep sequence (SNAP's omega from x = 10). The evaluation is Skygrind.Core's GoldenAssistMath (NIGHT9
// U3-04, GoldenAi.cs), the code the Player's -skygrindGolden runs too, one part per test here.
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class AssistGoldenTests
    {
        [Test]
        public void ScalarsAndAngles()
        {
            GoldenResult r = PortGolden.Check("assistMath scalars + angles", GoldenAssistMath.ScalarsAndAngles);
            Assert.That(r.cases, Is.EqualTo(24));
        }

        [Test]
        public void LockParamsAndPayouts()
        {
            GoldenResult r = PortGolden.Check("assistMath lockParams + payouts", GoldenAssistMath.LockParamsAndPayouts);
            Assert.That(r.cases, Is.EqualTo(57));
        }

        [Test]
        public void ScoresPicksAndHysteresis()
        {
            GoldenResult r = PortGolden.Check("assistMath scores + picks + hysteresis", GoldenAssistMath.ScoresPicksAndHysteresis);
            Assert.That(r.cases, Is.EqualTo(32));
        }

        [Test]
        public void SpringGateBreakawayHoming()
        {
            GoldenResult r = PortGolden.Check("assistMath spring + fire gate + breakaway + homing", GoldenAssistMath.SpringGateBreakawayHoming);
            // records: 4 springStep calls, the springStep sequence (one record, every row checked), 6 fireReady, 5 breaksAway, 4 homeVelocity
            Assert.That(r.cases, Is.EqualTo(4 + 1 + 6 + 5 + 4));
        }
    }
}
