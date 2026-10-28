// NIGHT28 (the lead): the stadium rink's centre line (Core/Enemies/Rink28Math.cs): a point placed at (s, lateral) projects back to the same (s, lateral) on
// every straight and cap, the tangent is a unit vector, the lap is 2 L + 2 pi rc, and the signed gap wraps the short way round.
using System;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Rink28MathTests
    {
        static Rink28Math Rink() => new Rink28Math(24, 5.75, 100, -40);

        [Test]
        public void TheLap_IsTwoStraightsAndTwoCaps()
        {
            Assert.That(Rink().Lap, Is.EqualTo(48 + 2 * Math.PI * 5.75).Within(1e-9));
        }

        [Test]
        public void APointAt_ProjectsBackToTheSameArcAndLateral_OnEveryStraightAndCap()
        {
            Rink28Math r = Rink();
            for (double lat = -2.5; lat <= 5.01; lat += 2.5)
                for (double s = 0.3; s < r.Lap; s += 1.07)
                {
                    r.PointAt(s, lat, out double x, out double z, out double tx, out double tz);
                    Assert.That(Math.Abs(tx * tx + tz * tz - 1), Is.LessThan(1e-9), $"unit tangent at s {s}");
                    r.Project(x, z, out double s2, out double lat2);
                    Assert.That(r.SignedGap(s, s2), Is.EqualTo(0).Within(1e-6), $"s {s} lateral {lat}: came back as {s2}");
                    Assert.That(lat2, Is.EqualTo(lat).Within(1e-6), $"lateral at s {s}");
                }
        }

        [Test]
        public void TheSignedGap_WrapsTheShortWayRound()
        {
            Rink28Math r = Rink();
            double lap = r.Lap;
            Assert.That(r.SignedGap(10, 20), Is.EqualTo(10).Within(1e-9));
            Assert.That(r.SignedGap(20, 10), Is.EqualTo(-10).Within(1e-9));
            Assert.That(r.SignedGap(lap - 5, 5), Is.EqualTo(10).Within(1e-9), "across the seam, ahead");
            Assert.That(r.SignedGap(5, lap - 5), Is.EqualTo(-10).Within(1e-9), "across the seam, behind");
        }

        [Test]
        public void TheTrack_IsContinuous_AtTheSeamsBetweenStraightsAndCaps()
        {
            Rink28Math r = Rink();
            foreach (double s in new[] { r.L, r.L + r.CapLength, 2 * r.L + r.CapLength, r.Lap })
            {
                r.PointAt(s - 1e-7, 0, out double x0, out double z0, out _, out _);
                r.PointAt(s + 1e-7, 0, out double x1, out double z1, out _, out _);
                Assert.That(Math.Abs(x1 - x0) + Math.Abs(z1 - z0), Is.LessThan(1e-5), $"a gap in the track at s {s}");
            }
        }
    }
}
