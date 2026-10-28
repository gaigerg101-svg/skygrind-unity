// Golden vectors for the style math (PortExport/vectors/styleMath.json): every recorded call of styleMath.js replayed
// through StyleMath, numbers bit for bit (results, and the mutated arguments where recorded). The evaluation is
// Skygrind.Core's GoldenStyleMath (NIGHT9 U3-04), the code the Player's -skygrindGolden runs too, one part per test here;
// its params come from config.json alone (StyleMath.From reads only style.*, so the design config changes nothing).
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class StyleGoldenTests
    {
        [Test]
        public void Freshness() => PortGolden.Check("styleMath freshness", GoldenStyleMath.Freshness);

        [Test]
        public void Awards()
        {
            PortGolden.Check("styleMath awards", GoldenStyleMath.Awards);
            Assert.That(PortGolden.Data.Cases("styleMath", "killAwards").Count, Is.EqualTo(9));
        }

        [Test]
        public void Ranks() => PortGolden.Check("styleMath ranks", GoldenStyleMath.Ranks);

        [Test]
        public void DiminishingAndTheHitCap() => PortGolden.Check("styleMath diminishing + the hit cap", GoldenStyleMath.DiminishingAndTheHitCap);
    }
}
