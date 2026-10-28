// NIGHT20 U3-34.3 (builder V): the THUNDERCLAP's shockwave, seen: NIGHT19's gust look re-aimed (MovementFx.Gust.cs,
// Viewmodel.Gust.cs, CameraFeel): the hands' blowback x ShockKickMult (bigger than the gust's, the same shape), still nothing
// with REDUCED MOTION; the shockwave's air is bigger than the gust's (more streaks, further, wider rings) and its FOV kick
// stays under the boost's.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class ShockwaveLookTests
    {
        static double PeakBack(HandsGust g, double seconds)
        {
            double best = 0;
            for (double t = 0; t < seconds; t += 1.0 / 240)
            {
                g.Step(1.0 / 240);
                double back = g.Sample().z;
                if (back > best) best = back;
            }
            return best;
        }

        [Test]
        public void TheShockwaveShovesTheGunHarderThanTheGust()
        {
            var gust = new HandsGust();
            gust.Fire(1, false);
            var shock = new HandsGust();
            shock.Fire(1, false, UnityTuning.ShockKickMult);
            double g = PeakBack(gust, 0.5), s = PeakBack(shock, 0.5);
            Assert.That(g, Is.GreaterThan(0), "the gust's blowback");
            Assert.That(s / g, Is.EqualTo(UnityTuning.ShockKickMult).Within(0.02), "the shockwave's: x ShockKickMult, the same shape");
            Assert.That(UnityTuning.ShockKickMult, Is.GreaterThan(1));
        }

        [Test]
        public void ReducedMotionStillsTheShockwavesKick()
        {
            var shock = new HandsGust();
            shock.Fire(0, true, UnityTuning.ShockKickMult);
            Assert.That(shock.Active, Is.False, "REDUCED MOTION: nothing moves");
            Assert.That(PeakBack(shock, 0.3), Is.EqualTo(0));
        }

        [Test]
        public void TheShockwavesAirIsBiggerThanTheGustsAndItsKickUnderTheBoosts()
        {
            Assert.That(System.Math.Round(UnityTuning.GustStreaks * UnityTuning.ShockStreakMult), Is.GreaterThan(UnityTuning.GustStreaks), "more streaks");
            Assert.That(System.Math.Round(UnityTuning.GustStreaks * UnityTuning.ShockStreakMult), Is.LessThanOrEqualTo(UnityTuning.GustPool * UnityTuning.GustStreaks), "inside the pool");
            double reach = GustFxMath.Travel(UnityTuning.GustStreakSpeed * UnityTuning.ShockStreakSpeedMult, UnityTuning.GustStreakDrag, UnityTuning.GustStreakLife);
            double gustReach = GustFxMath.Travel(UnityTuning.GustStreakSpeed, UnityTuning.GustStreakDrag, UnityTuning.GustStreakLife);
            Assert.That(reach, Is.GreaterThan(gustReach), "the air flies further");
            Assert.That(GustFxMath.FovKick(0, false) * UnityTuning.ShockFovMult, Is.LessThan(UnityTuning.BoostFovKickDegN18), "the FOV kick stays under the boost's");
        }
    }
}
