// NIGHT20 C3 (builder V): the affixes' looks (Game/AffixFx20.cs): VOLATILE's fuse pulses faster and grows toward its burst
// (the telegraph rim's rule: 0.75 + 0.25 sin(t (6 + 18 k)), steady on the moment); REGENERATING glows only while it heals
// (out of damage, under its spawn HP, alive); SWIFT trails streaks by its speed (none at a walk, longer the faster, capped).
using NUnit.Framework;
using UnityEngine;
using F = Skygrind.Game.AffixFxMath;

namespace Skygrind.Game.Tests
{
    public class AffixFx20Tests
    {
        [Test]
        public void VolatilesFusePulsesFasterAndGrowsToItsBurst()
        {
            Assert.That(F.FuseGlowSize(0, 0.4, 3.5), Is.GreaterThan(0), "it shows from the kill");
            Assert.That(F.FuseGlowSize(0.4, 0.4, 3.5), Is.GreaterThan(F.FuseGlowSize(0.1, 0.4, 3.5)), "it grows toward the burst");
            Assert.That(F.FuseGlowSize(0.4, 0.4, 3.5), Is.LessThanOrEqualTo(3.5), "never past the burst's radius");
            Assert.That(F.FusePulse(0.4, 0.4), Is.EqualTo(1).Within(1e-12), "the moment: whole");
            Assert.That(F.FusePulse(0, 0.4), Is.EqualTo(0.75).Within(1e-12));
            double lo = 1, hi = 0;
            for (double t = 0; t < 0.4; t += 0.001)
            {
                double p = F.FusePulse(t, 0.4);
                lo = System.Math.Min(lo, p);
                hi = System.Math.Max(hi, p);
            }
            Assert.That(lo, Is.GreaterThanOrEqualTo(0.5 - 1e-9));
            Assert.That(hi, Is.LessThanOrEqualTo(1 + 1e-9));
        }

        [Test]
        public void RegeneratingGlowsOnlyWhileItHeals()
        {
            Assert.That(F.Healing(true, 0, 50, 100, true), Is.True);
            Assert.That(F.Healing(true, 3, 50, 100, true), Is.False, "hit recently: calm");
            Assert.That(F.Healing(true, 0, 100, 100, true), Is.False, "full");
            Assert.That(F.Healing(false, 0, 50, 100, true), Is.False, "not regenerating");
            Assert.That(F.Healing(true, 0, 50, 100, false), Is.False, "dead");
        }

        /// <summary>Banned 13 (NIGHT20 row 4, builder HV): VOLATILE's burst was 30 untextured cubes on the shared pool; now
        /// textured sparks (trace_06 streaks along their velocity) with particles.js burst's own rules: speed x (1 - jitter r)
        /// in a random direction, gravity, shrinking with life x scale x fx.size.</summary>
        [Test]
        public void VolatilesBurst_IsTexturedSparks_WithTheWebsBurstRules()
        {
            Vector3 v = F.BurstVelocity(Vector3.up, 11, 0.5, 0);
            Assert.That(v.magnitude, Is.EqualTo(11).Within(1e-4), "affixes.js: 11 m/s");
            Assert.That(F.BurstVelocity(Vector3.right, 11, 0.5, 1).magnitude, Is.EqualTo(5.5).Within(1e-4), "x (1 - speedJitter r)");
            Vector3 fall = F.SparkFall(new Vector3(3, 0, 0), 20, 0.1);
            Assert.That(fall.y, Is.EqualTo(-2).Within(1e-5), "gravity (particles.js: a burst falls)");
            Assert.That(fall.x, Is.EqualTo(3).Within(1e-6));
            Assert.That(F.SparkWidth(0.5, 0.5, 1.8, 0.12), Is.EqualTo(1.8 * 0.12 * UnityTuning.AffixSparkWidth).Within(1e-6), "whole at its birth");
            Assert.That(F.SparkWidth(0.25, 0.5, 1.8, 0.12), Is.EqualTo(0.5 * 1.8 * 0.12 * UnityTuning.AffixSparkWidth).Within(1e-6), "shrinking as it dies");
            Assert.AreEqual(30, F.BurstCount, "affixes.js :194: 30");
        }

        [Test]
        public void SwiftTrailsBySpeed()
        {
            Assert.That(F.SwiftTrail(2), Is.EqualTo(0), "a shuffle: none");
            Assert.That(F.SwiftTrail(8), Is.GreaterThan(F.SwiftTrail(5)), "longer the faster");
            Assert.That(F.SwiftTrail(100), Is.LessThanOrEqualTo(UnityTuning.SwiftTrailMax), "capped");
        }
    }
}
