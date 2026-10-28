// NIGHT27 builder B (boss2): THE GLACIER's pure rules (Core/Enemies/Glacier27.cs): the gap controller's clamp (+-15 % of the player's top speed, gap-based) and the curve's view data
// (heading change <= maxTurnDeg per tile, deterministic from the run seed). No scene, no UnityEngine.
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Glacier27Tests
    {
        static Glacier27Params P() => new Glacier27Params { top = 8.5, baseFrac = 0.9, k = 0.5, clampFrac = 0.15, targetGap = 22, targetStep = 4 };

        [Test]
        public void SpeedAt_NeverLeavesTheClampAroundTheBase()
        {
            Glacier27Params p = P();
            double b = p.baseFrac * p.top, lim = p.clampFrac * p.top;
            foreach (double gap in new[] { 0.5, 5, 12, 22, 40, 90, 1000 })
            {
                double v = Glacier27.SpeedAt(p, gap);
                Assert.That(v, Is.InRange(b - lim - 1e-9, b + lim + 1e-9), "gap " + gap);
            }
            Assert.That(Glacier27.SpeedAt(p, 22), Is.EqualTo(b).Within(1e-9), "at the target gap it skates at its base");
            Assert.That(Glacier27.SpeedAt(p, 1000), Is.EqualTo(b + lim).Within(1e-9));
            Assert.That(Glacier27.SpeedAt(p, 0.5), Is.EqualTo(b - lim).Within(1e-9));
        }

        [Test]
        public void SpeedAt_RunPastItAndItSkatesAway_AndLaterPhasesSitFurtherAhead()
        {
            Glacier27Params p = P();
            double b = p.baseFrac * p.top, lim = p.clampFrac * p.top;
            Assert.That(Glacier27.SpeedAt(p, -5), Is.EqualTo(b + lim).Within(1e-9));
            Assert.That(Glacier27.SpeedAt(p, 24, 1), Is.GreaterThan(Glacier27.SpeedAt(p, 24, 2)), "phase 2's target is further ahead: the same gap reads closer");
        }

        [Test]
        public void Curve_TurnsStayInsideTheCap_AndAreReproducible()
        {
            var a = new Glacier27Curve(1234, 77, 40, 22);
            var b = new Glacier27Curve(1234, 77, 40, 22);
            for (int k = 0; k < 12; k++)
            {
                Assert.That(System.Math.Abs(a.TurnDeg(k)), Is.LessThanOrEqualTo(22.0 + 1e-9));
                Assert.That(a.TurnDeg(k), Is.EqualTo(b.TurnDeg(k)));
            }
            a.Pose(0, out double x0, out double z0, out double h0);
            Assert.That(x0, Is.EqualTo(0).Within(1e-9));
            Assert.That(z0, Is.EqualTo(0).Within(1e-9));
            Assert.That(h0, Is.EqualTo(0).Within(1e-9));
            a.Pose(40, out double x1, out double z1, out _);
            Assert.That(JsMath.Hypot(x1, z1), Is.InRange(36, 40.001), "a tile's chord is at most its arc length (and within a 22 degree arc of it)");
        }
    }
}
