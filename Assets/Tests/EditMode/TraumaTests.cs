// NIGHT8 U2-06: the trauma shake. The web's tests/feel.test.js "trauma shake" cases, and the cosmetic noise bit for bit
// against values the web's own fx/cosmetic.js printed (2026-10-02, node, from p17-night6).
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class TraumaTests
    {
        static readonly Trauma.Params C = new Trauma.Params { maxPitchDeg = 2, maxYawDeg = 2, maxRollDeg = 1.5, exponent = 2, noiseHz = 25 };

        [Test]
        public void DecaysLinearlyToExactlyZero_AndCapsAtOne()
        {
            var tr = new Trauma();
            tr.Add(0.7);
            tr.Add(0.7);
            Assert.That(tr.Value, Is.EqualTo(1));
            for (int i = 0; i < 25; i++) tr.Step(1.0 / 60, 2.5); // just past 0.4 s at 2.5 / s
            Assert.That(tr.Value, Is.EqualTo(0));
            Assert.That(tr.Sample(C, 1), Is.EqualTo((0.0, 0.0, 0.0)));
        }

        [Test]
        public void ShakesInsideTraumaSquaredTimesTheMax_AndNotAtAllAtScaleZero()
        {
            var tr = new Trauma();
            tr.Add(0.5);
            for (int i = 0; i < 30; i++)
            {
                tr.Step(1.0 / 120, 0);
                var o = tr.Sample(C, 1);
                Assert.That(System.Math.Abs(o.pitch), Is.LessThanOrEqualTo(2 * 0.25 + 1e-12));
                Assert.That(System.Math.Abs(o.roll), Is.LessThanOrEqualTo(1.5 * 0.25 + 1e-12));
                Assert.That(tr.Sample(C, 0), Is.EqualTo((0.0, 0.0, 0.0)));
            }
        }

        [Test]
        public void TheNoise_MatchesTheWebBitForBit()
        {
            Assert.That(Cosmetic.Lattice(0, 11), Is.EqualTo(-0.9410740444299471));
            Assert.That(Cosmetic.Lattice(5, 23), Is.EqualTo(-0.6677188984276072));
            Assert.That(Cosmetic.Lattice(-3, 37), Is.EqualTo(0.7020694077252572));
            Assert.That(Cosmetic.ValueNoise(3.7, 37), Is.EqualTo(-0.6486003680267838));
            Assert.That(Cosmetic.ValueNoise(12.345, 11), Is.EqualTo(-0.06754643862742873));
        }
    }
}
