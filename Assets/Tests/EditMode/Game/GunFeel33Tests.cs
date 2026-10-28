// NIGHT33 W round 3 (view33, DESIGN33 W "Viewmodel"): GunFeel33's layers, RED (view33 off: the web's) vs GREEN (on). The numbers are the web's
// viewmodel config (bobAmpY 0.01, bobMaxScale 1.5, walkSpeed 8; recoilSprings light 8 Hz z 0.5, medium 6 Hz z 0.45, heavy 4.5 Hz z 0.42).
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class GunFeel33Tests
    {
        [Test]
        public void IdleSwayIsASmallFigureEightOnlyWhenStill()
        {
            double maxX = 0, maxY = 0;
            for (double t = 0; t < 1 / UnityTuning.IdleSwayHz33; t += 0.01)
            {
                var s = GunFeel33.IdleSway(t, 1);
                maxX = System.Math.Max(maxX, System.Math.Abs(s.x));
                maxY = System.Math.Max(maxY, System.Math.Abs(s.y));
            }
            Assert.That(maxX, Is.InRange(0.001, 0.003), "1-3 mm across");
            Assert.That(maxY, Is.InRange(0.0005, 0.003));
            Assert.That(UnityTuning.IdleSwayHz33, Is.InRange(0.2, 0.5), "0.2-0.5 Hz");
            Assert.That(GunFeel33.IdleSway(0.7, 0).x, Is.EqualTo(0).Within(1e-12), "a bobbing gun does not idle-sway");
        }

        [Test]
        public void TheBobKeepsScalingPastTheWebsCap()
        {
            Assert.That(GunFeel33.BobScale(false, 16, 8, 1.5), Is.EqualTo(1.5).Within(1e-12), "RED: capped at 12 m/s");
            Assert.That(GunFeel33.BobScale(true, 16, 8, 1.5), Is.EqualTo(2.0).Within(1e-12), "GREEN: on to 16 m/s");
            Assert.That(GunFeel33.BobScale(true, 6, 8, 1.5), Is.EqualTo(GunFeel33.BobScale(false, 6, 8, 1.5)).Within(1e-12), "the same under the web's cap");
        }

        [Test]
        public void TheRecoilSettlesIn80To150Ms()
        {
            (string cls, double f, double z)[] springs = { ("light", 8, 0.5), ("medium", 6, 0.45), ("heavy", 4.5, 0.42) };
            foreach (var (cls, f, z) in springs)
            {
                double red = GunFeel33.SettleSeconds(f, z, GunFeel33.RecoilRate(false, cls));
                double green = GunFeel33.SettleSeconds(f, z, GunFeel33.RecoilRate(true, cls));
                Assert.That(red, Is.GreaterThan(0.15), $"{cls}: the web's settle");
                Assert.That(green, Is.InRange(0.08, 0.15), $"{cls}: view33's settle (G1: 80-150 ms)");
            }
        }

        [Test]
        public void ThePlayersFlashesAreNeverRedOrange()
        {
            UnityEngine.Color hot = Look.Hex(UnityTuning.WhiteHotHex33);
            foreach (uint warm in new uint[] { 0xffb000, 0xffe7b8, 0xffd27a, 0xff8c1a })
                Assert.That(WeaponFx.PlayerFx33(Look.Hex(warm)), Is.EqualTo(hot), $"#{warm:x6} (edge, muzzleFlash, spark, fire) -> white-hot");
            foreach (uint cool in new uint[] { 0x19d3e6, 0xb8ff2c, 0x8f5bff, 0x3d7bff, 0xeaf8ff, 0xd6fbff })
                Assert.That(WeaponFx.PlayerFx33(Look.Hex(cool)), Is.EqualTo(Look.Hex(cool)), $"#{cool:x6} kept");
        }

        [Test]
        public void TheLandingDipComesBackAndTipsOnlyWithView33()
        {
            var red = GunFeel33.LandDip(false, 0.05);
            var green = GunFeel33.LandDip(true, 0.05);
            Assert.That(red.y, Is.EqualTo(-0.05).Within(1e-12));
            Assert.That(red.z, Is.EqualTo(0).Within(1e-12));
            Assert.That(green.y, Is.EqualTo(-0.05).Within(1e-12), "the same depth (the web's fall-speed scale)");
            Assert.That(green.z, Is.GreaterThan(0), "back toward the eye");
            Assert.That(green.pitchDeg, Is.LessThan(0), "tipped down");
            Assert.That(UnityTuning.ViewmodelFov33, Is.InRange(54f, 60f), "the viewmodel FOV band");
        }
    }
}
