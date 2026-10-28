// NIGHT20 U3-34.3 (builder V): the zoom in the hands: the LONGWAVE's scope rims sit on the HUD vignette's clear edge (hud.js
// .hud-scope: 31vh) at any depth and FOV, the objective's rim reads smaller than the eyepiece's; the STITCHER's ADS pose
// stays near the web's default one (centred under the eye, slightly canted), and the zoomed FOVs are the web's formula's.
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class ZoomLookTests
    {
        static double ScreenShare(double radius, double fovDeg, double depth) =>
            radius / depth / System.Math.Tan(fovDeg * 0.5 * System.Math.PI / 180) * 0.5;

        [Test]
        public void TheScopeRimsSitOnTheVignettesClearEdge()
        {
            foreach (double fov in new[] { 54.0, 60, 74 })
                foreach (double d in new[] { 0.06, 0.32 })
                    Assert.That(ScreenShare(ScopeMath.RimRadius(0.31, fov, d), fov, d), Is.EqualTo(0.31).Within(1e-9), $"31vh at fov {fov}, {d} m");
            Assert.That(UnityTuning.ScopeObjectiveShrink, Is.InRange(0.8, 1.0), "the far rim a touch smaller: a tube");
            Assert.That(UnityTuning.ScopeObjectiveDepth, Is.GreaterThan(UnityTuning.ScopeEyeDepth));
            Assert.That(UnityTuning.ScopeShowFrom, Is.InRange(0.0, 0.99), "the rims arrive with the scope");
        }

        [Test]
        public void TheStitchersAdsPoseIsCentredUnderTheEye()
        {
            Assert.That(System.Math.Abs(UnityTuning.NailgunAdsX), Is.LessThan(0.03), "near the sight line");
            Assert.That(UnityTuning.NailgunAdsY, Is.InRange(-0.15, -0.09), "under the eye, as the web's ADS poses (-0.11 .. -0.13)");
            Assert.That(UnityTuning.NailgunAdsZ, Is.InRange(-0.32, -0.22), "ahead, as the web's (-0.24 .. -0.3)");
            Assert.That(System.Math.Abs(UnityTuning.NailgunAdsRollDeg), Is.LessThanOrEqualTo(5), "a slight cant");
        }

        // NIGHT33 W (view33, D-5): the pose's two paths. Off: the web's mix toward the ADS pose (centred, rolled). On: the hold pose, lowered 4-6 cm,
        // never centred, never rolled, eased in 120 ms. The numbers are the web's default hold / ADS pose (viewmodel.holdX/Y/Z, ads.default).
        const double HoldX = 0.135, HoldY = -0.147, HoldZ = 0.36, HoldPitch = 1, HoldYaw = 5, AdsX = 0.02, AdsY = -0.11, AdsZ = 0.30, AdsRoll = -4;

        [Test]
        public void TheOffPathIsTheWebsAdsMix()
        {
            ZoomPose33.Pose p = ZoomPose33.Of(false, 1, UnityTuning.ZoomLower33, HoldX, HoldY, HoldZ, HoldPitch, HoldYaw, AdsX, AdsY, AdsZ, 0, 0, AdsRoll);
            Assert.That(p.x, Is.EqualTo(AdsX).Within(1e-9), "zoomed: centred (the web's ADS pose)");
            Assert.That(p.y, Is.EqualTo(AdsY).Within(1e-9));
            Assert.That(p.yawDeg, Is.EqualTo(0).Within(1e-9));
            Assert.That(p.rollDeg, Is.EqualTo(AdsRoll).Within(1e-9), "the web's cant");
            ZoomPose33.Pose h = ZoomPose33.Of(false, 0, UnityTuning.ZoomLower33, HoldX, HoldY, HoldZ, HoldPitch, HoldYaw, AdsX, AdsY, AdsZ, 0, 0, AdsRoll);
            Assert.That(h.x, Is.EqualTo(HoldX).Within(1e-9));
            Assert.That(h.rollDeg, Is.EqualTo(0).Within(1e-9));
        }

        [Test]
        public void TheN33ZoomOnlyLowersTheGun()
        {
            Assert.That(UnityTuning.ZoomLower33, Is.InRange(0.04, 0.06), "lowers 4-6 cm");
            Assert.That(UnityTuning.ZoomEase33, Is.EqualTo(0.12).Within(1e-9), "eases in 120 ms");
            foreach (double ak in new[] { 0.0, 0.5, 1.0 })
            {
                ZoomPose33.Pose p = ZoomPose33.Of(true, ak, UnityTuning.ZoomLower33, HoldX, HoldY, HoldZ, HoldPitch, HoldYaw, AdsX, AdsY, AdsZ, 0, 0, AdsRoll);
                Assert.That(p.x, Is.EqualTo(HoldX).Within(1e-9), $"never slides to the centre (ak {ak})");
                Assert.That(p.z, Is.EqualTo(HoldZ).Within(1e-9));
                Assert.That(p.yawDeg, Is.EqualTo(HoldYaw).Within(1e-9));
                Assert.That(p.pitchDeg, Is.EqualTo(HoldPitch).Within(1e-9));
                Assert.That(p.rollDeg, Is.EqualTo(0).Within(1e-9), "never rolls");
                Assert.That(p.y, Is.EqualTo(HoldY - UnityTuning.ZoomLower33 * ak).Within(1e-12), "lowers with the zoom");
            }
        }

        [Test]
        public void TheZoomedFovsAreTheWebsFormulas()
        {
            Assert.That(CombatMath.ZoomedFov(74, 1.25), Is.EqualTo(62.2).Within(0.05), "STITCHER");
            Assert.That(CombatMath.ZoomedFov(74, 1.5), Is.EqualTo(53.3).Within(0.05), "KESTREL");
            Assert.That(CombatMath.ZoomedFov(74, 3.5), Is.EqualTo(24.3).Within(0.05), "LONGWAVE");
        }
    }
}
