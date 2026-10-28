// NIGHT16 polish #41 / #43 / #44 (builder V): the effects' pure gates (FxGates): the particle cube's scale is the web's
// (size x scale x life left), the motion flourishes' REDUCED MOTION / intensity gate (CameraFeel's reading), and the
// THUNDERCLAP's "nothing hit" shotResult (no sparks, no light in mid-air).
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class FxGatesTests
    {
        [Test]
        public void AParticleCubeIsTheWebsSizeTimesItsScaleShrinkingAsItDies()
        {
            Assert.That(FxGates.ParticleScale(0.12f, 1.6f, 0, 0.5f), Is.EqualTo(0.12f * 1.6f).Within(1e-6), "a rocket blast cube at birth");
            Assert.That(FxGates.ParticleScale(0.12f, 0.4f, 0.25f, 0.5f), Is.EqualTo(0.12f * 0.4f * 0.5f).Within(1e-6), "a spark half way");
            Assert.That(FxGates.ParticleScale(0.12f, 1, 0.5f, 0.5f), Is.EqualTo(0).Within(1e-6), "gone at its life");
            Assert.That(FxGates.ParticleScale(0.12f, 1, 0, 0.5f), Is.EqualTo(0.12f).Within(1e-6), "scale 1: as before");
        }

        [Test]
        public void MotionFlourishesStopWithReducedMotionOrNoEffects()
        {
            Assert.That(FxGates.MotionOn(false, 1), Is.True);
            Assert.That(FxGates.MotionOn(true, 1), Is.False, "REDUCED MOTION");
            Assert.That(FxGates.MotionOn(false, 0), Is.False, "EFFECTS INTENSITY 0");
            Assert.That(FxGates.MotionOn(false, 0.3), Is.True);
        }

        [Test]
        public void TheParticleCapIsTheWebsSetCapAndRing()
        {
            Assert.That(FxGates.ParticleCap(400, 160), Is.EqualTo(160), "LOW");
            Assert.That(FxGates.ParticleCap(400, 400), Is.EqualTo(400));
            Assert.That(FxGates.ParticleCap(400, 1000), Is.EqualTo(400), "never past the pool");
            Assert.That(FxGates.ParticleCap(400, 0.5), Is.EqualTo(1), "at least one");
            Assert.That(FxGates.ParticleCap(400, 159.9), Is.EqualTo(159), "floor");
            int next = 0;
            for (int i = 0; i < 3; i++) Assert.That(FxGates.TakeSlot(ref next, 3), Is.EqualTo(i));
            Assert.That(FxGates.TakeSlot(ref next, 3), Is.EqualTo(0), "full: the oldest is recycled");
            next = 300;
            Assert.That(FxGates.TakeSlot(ref next, 160), Is.EqualTo(0), "a cap lowered under next: back to 0 (take(): if (next >= cap) next = 0)");
            Assert.That(next, Is.EqualTo(1));
            Assert.That(FxGates.PresetValue("low", 160, 400, 400), Is.EqualTo(160));
            Assert.That(FxGates.PresetValue("medium", 160, 300, 400), Is.EqualTo(300));
            Assert.That(FxGates.PresetValue("high", 160, 300, 400), Is.EqualTo(400));
            Assert.That(FxGates.PresetValue(null, 160, 300, 400), Is.EqualTo(400), "no preset yet: high (LightAndLens' default)");
        }

        [Test]
        public void TheImpactRingBurstsByTheQualityPreset()
        {
            Assert.That(FxGates.ImpactBurstCount(1), Is.EqualTo(18), "MEDIUM / HIGH");
            Assert.That(FxGates.ImpactBurstCount(0.5), Is.EqualTo(9), "LOW");
        }

        [Test]
        public void AMeleeHitOfMineSparks()
        {
            Assert.That(FxGates.MeleeSparks(new HitEvent { melee = true, weapon = "gauntlet", position = new Vec3(1, 1, 1) }), Is.True);
            Assert.That(FxGates.MeleeSparks(new HitEvent { melee = true, weapon = "punch", position = new Vec3(1, 1, 1) }), Is.True, "the punch too (colors.spark)");
            Assert.That(FxGates.MeleeSparks(new HitEvent { melee = false, weapon = "revolver", position = new Vec3(1, 1, 1) }), Is.False, "a gun hit");
            Assert.That(FxGates.MeleeSparks(new HitEvent { owner = "rival", melee = true, weapon = "gauntlet", position = new Vec3(1, 1, 1) }), Is.False, "mine only");
        }

        [Test]
        public void TheDeathBurstIsAtTheWebsHeight()
        {
            Assert.That(FxGates.DeathBurstY("husk", 2, 1.8, 3), Is.EqualTo(3), "husk.js :107: aimHeight (the AimPoint)");
            Assert.That(FxGates.DeathBurstY("swarmer", 2, 0.75, 2.45), Is.EqualTo(2.375).Within(1e-12), "enemyKind.js :67: height x 0.5");
            Assert.That(FxGates.DeathBurstY("spitter", 0, 2, 1.1), Is.EqualTo(1).Within(1e-12));
        }

        [Test]
        public void TheOverloadBurstIsForMine()
        {
            Assert.That(FxGates.OverloadBurst(new ArsenalEvent { owner = "player" }), Is.True);
            Assert.That(FxGates.OverloadBurst(new ArsenalEvent { owner = "rival" }), Is.False, "gunFx.js :96 mine()");
        }

        [Test]
        public void TheMuzzleLinesUpOnScreenInTheWorldCamera()
        {
            double worldFov = 100, vmFov = 54;
            var vm = new UnityEngine.Vector3(0.18f, -0.15f, 0.52f);
            UnityEngine.Vector3 w = HandsMuzzle.ToWorldCameraSpace(vm, worldFov, vmFov);
            Assert.That(w.z, Is.EqualTo(vm.z), "the depth kept");
            double tw = System.Math.Tan(worldFov * System.Math.PI / 360), tv = System.Math.Tan(vmFov * System.Math.PI / 360);
            Assert.That(w.x / w.z / tw, Is.EqualTo(vm.x / vm.z / tv).Within(1e-5), "the same screen x");
            Assert.That(w.y / w.z / tw, Is.EqualTo(vm.y / vm.z / tv).Within(1e-5), "the same screen y");
            UnityEngine.Vector3 same = HandsMuzzle.ToWorldCameraSpace(vm, vmFov, vmFov);
            Assert.That(same.x, Is.EqualTo(vm.x).Within(1e-6), "equal FOVs: unchanged");
        }

        [Test]
        public void AThunderclapThatHitNothingIsAMiss()
        {
            double range = 60;
            var origin = new Vec3(1, 2, 3);
            var aim = new Vec3(0, 0, -1);
            var miss = new ShotEvent { weapon = "shotgun", origin = origin, dir = aim, end = origin + aim * range, surface = "world" };
            Assert.That(FxGates.ShotgunMissedAll(miss, range), Is.True, "the end at the range along the aim: nothing hit");
            var wall = new ShotEvent { weapon = "shotgun", origin = origin, dir = aim, end = origin + Vec3.Normalize(new Vec3(0.1, 0, -0.995)) * 14.2, surface = "world" };
            Assert.That(FxGates.ShotgunMissedAll(wall, range), Is.False, "pellet 1 on a wall 14 m away");
            var enemy = new ShotEvent { weapon = "shotgun", origin = origin, dir = aim, end = origin + aim * range, surface = "enemy" };
            Assert.That(FxGates.ShotgunMissedAll(enemy, range), Is.False, "a pellet hit an enemy");
            var kestrel = new ShotEvent { weapon = "revolver", origin = origin, dir = aim, end = origin + aim * range, surface = "world" };
            Assert.That(FxGates.ShotgunMissedAll(kestrel, range), Is.False, "the shotgun's rule only");
        }
    }
}
