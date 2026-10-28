// NIGHT29 round 2 (builder V): the hazard strip's brightness by the sim's state (Game/PropsView29.cs).
using NUnit.Framework;
using B = Skygrind.Game.PropsView29Math;

namespace Skygrind.Game.Tests
{
    public class Props29Tests
    {
        [Test]
        public void TheHazardStripRestsPulsesAndBurns()
        {
            Assert.That(B.HazardBrightness(0, 0, 1.5, false), Is.EqualTo(UnityTuning.HazardOff));
            Assert.That(B.HazardBrightness(2, 0, 1.5, false), Is.EqualTo(UnityTuning.HazardLive));
            Assert.That(UnityTuning.HazardLive, Is.GreaterThan(UnityTuning.HazardTeleHigh), "live outshines the telegraph");
            for (double t = 0; t < 1.5; t += 0.037)
                Assert.That(B.HazardBrightness(1, t, 1.5, false), Is.InRange(UnityTuning.HazardTeleLow - 1e-4f, UnityTuning.HazardTeleHigh + 1e-4f));
            Assert.That(B.HazardBrightness(1, 0.3, 1.5, true), Is.EqualTo(UnityTuning.HazardReduced), "REDUCED MOTION: steady");
        }
    }
}
