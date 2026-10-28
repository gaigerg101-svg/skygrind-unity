// NIGHT9 U3-07: the momentumMath.js parts of the web's tests/momentum.test.js ported: M from the closing speed and a
// surge, the closing speed, the damage / radius multipliers, smoothing and the surge, the impact wave, through
// MomentumMath. Skipped (it needs the stateful combat/momentum.js createMomentum, not yet ported): "a dash toward a target
// from a walk reaches M above the impact threshold". The bit-exact replay of momentumMath.json is GoldenPorted.cs.
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class MomentumMathTests
    {
        static MomentumParams m;
        static ImpactWaveParams w;

        [OneTimeSetUp]
        public void Load()
        {
            var cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            m = MomentumParams.From(cfg);
            w = ImpactWaveParams.From(cfg);
        }

        [Test]
        public void MIsClamp01OfClosingMinus8Over24_ASurgeOver10mpsAdds0_3Capped()
        {
            Expect.Exactly(MomentumMath.MomentumOf(8, 0, m), 0);
            Expect.CloseTo(MomentumMath.MomentumOf(20, 0, m), 0.5, 10);
            Expect.Exactly(MomentumMath.MomentumOf(40, 0, m), 1);
            Expect.CloseTo(MomentumMath.MomentumOf(20, 12, m), 0.8, 10);
            Expect.Exactly(MomentumMath.MomentumOf(30, 12, m), 1);
            Expect.Exactly(MomentumMath.MomentumOf(0, 9, m), 0); // under the surge threshold
        }

        [Test]
        public void ClosingSpeedIsTheVelocityAlongTheDirectionToTheTarget()
        {
            var from = new Vec3(0, 0, 0);
            Expect.CloseTo(MomentumMath.ClosingSpeed(new Vec3(0, 0, -20), from, new Vec3(0, 0, -10)), 20, 10);
            Expect.CloseTo(MomentumMath.ClosingSpeed(new Vec3(20, 0, 0), from, new Vec3(0, 0, -10)), 0, 10); // strafing past
            Expect.CloseTo(MomentumMath.ClosingSpeed(new Vec3(0, 0, 20), from, new Vec3(0, 0, -10)), -20, 10); // moving away
        }

        [Test]
        public void MeleeTimesLerp1To3_GunsLerp1To1_35_Blasts25PercentRadiusAtM1()
        {
            Expect.Exactly(MomentumMath.MomentumDamage(0, true, m), 1);
            Expect.Exactly(MomentumMath.MomentumDamage(1, true, m), 3);
            Expect.Exactly(MomentumMath.MomentumDamage(0.5, true, m), 2);
            Expect.CloseTo(MomentumMath.MomentumDamage(1, false, m), 1.35, 10);
            Expect.CloseTo(MomentumMath.MomentumRadius(1, m), 1.25, 10);
            Expect.Exactly(MomentumMath.MomentumRadius(0, m), 1);
        }

        [Test]
        public void SmoothingFollowsOver60ms_TheSurgeIsTheGainOverTheWindow()
        {
            var s = new Vec3(0, 0, 0);
            for (int i = 0; i < 12; i++) MomentumMath.SmoothVelocity(ref s, new Vec3(24, 0, 0), 1.0 / 120, m.smoothTime);
            Assert.That(s.x, Is.GreaterThan(24 * 0.7)); // ~100 ms in: most of the way
            Expect.Exactly(MomentumMath.SurgeOf(new double[] { 8, 8, 8, 20 }, 24), 16);
            Expect.Exactly(MomentumMath.SurgeOf(new double[0], 5), 0);
        }

        [Test]
        public void TheImpactWaveFiresAbove0_55OffCooldown_Sharing35To60PercentOfTheHit_Pushing6To14mps()
        {
            Assert.That(MomentumMath.ImpactWave(0.5, 0, w).fires, Is.False);
            Assert.That(MomentumMath.ImpactWave(0.6, 0.1, w).fires, Is.False); // cooling down
            ImpactWaveResult lo = MomentumMath.ImpactWave(0.56, 0, w);
            ImpactWaveResult hi = MomentumMath.ImpactWave(1, 0, w);
            Assert.That(lo.fires && hi.fires, Is.True);
            Assert.That(lo.share, Is.GreaterThanOrEqualTo(0.35));
            Expect.CloseTo(hi.share, 0.6, 10);
            Expect.CloseTo(hi.knockback, 14, 10);
            Assert.That(lo.knockback, Is.GreaterThanOrEqualTo(6));
        }
    }
}
