// NIGHT20 row 6 (builder V): MovementFx textured (Game/MovementFx.Textured20.cs): a dust puff blooms from 0.55 to 1.45 of
// its size on an ease-out, fades in over its first 12 % and out from 45 %, its drift slows; a landing puffs 2 at the puff
// speed and 7 at the hardest, larger the harder; the climb's phases (N's ClimbEvent: start, up, top, knocked, land) each
// raise their dust, and only "up" streams grit along the face.
using NUnit.Framework;
using X = Skygrind.Game.Textured20Math;

namespace Skygrind.Game.Tests
{
    public class Textured20Tests
    {
        [Test]
        public void APuffBloomsThenHangsAndFades()
        {
            Assert.That(X.PuffScale(0), Is.EqualTo(0.55).Within(1e-12));
            Assert.That(X.PuffScale(1), Is.EqualTo(1.45).Within(1e-12));
            Assert.That(X.PuffScale(0.5), Is.GreaterThan(0.55 + 0.9 * 0.5), "an ease-out: past half its growth by half its life");
            Assert.That(X.PuffAlpha(0), Is.EqualTo(0));
            Assert.That(X.PuffAlpha(0.12), Is.EqualTo(1).Within(1e-12));
            Assert.That(X.PuffAlpha(0.3), Is.EqualTo(1).Within(1e-12));
            Assert.That(X.PuffAlpha(1), Is.EqualTo(0).Within(1e-12));
            Assert.That(X.PuffAlpha(0.8), Is.InRange(0.01, 0.99));
            Assert.That(X.Drift(2, 0), Is.EqualTo(0));
            Assert.That(X.Drift(2, 10), Is.EqualTo(2 / UnityTuning.PuffDrag).Within(1e-6), "the drift slows to a stop: v0 / k");
        }

        [Test]
        public void AHarderLandingPuffsMoreAndBigger()
        {
            Assert.That(X.LandPuffs(0), Is.EqualTo(2));
            Assert.That(X.LandPuffs(1), Is.EqualTo(7));
            Assert.That(X.LandPuffs(0.5), Is.InRange(3, 6));
            Assert.That(X.LandPuffs(-1), Is.EqualTo(2), "clamped");
            Assert.That(X.LandPuffSize(1), Is.GreaterThan(X.LandPuffSize(0)));
            Assert.That(X.LandPuffSize(0), Is.EqualTo(UnityTuning.LandPuffSize20).Within(1e-12));
        }

        [Test]
        public void EveryClimbPhaseRaisesItsDust()
        {
            Assert.That(X.ClimbPuffs("start"), Is.EqualTo(UnityTuning.ClimbStartPuffs), "the tell: grit at the foot");
            Assert.That(X.ClimbPuffs("top"), Is.EqualTo(UnityTuning.ClimbTopPuffs), "the mantle at the lip");
            Assert.That(X.ClimbPuffs("knocked"), Is.EqualTo(UnityTuning.ClimbKnockedPuffs), "knocked off: a burst");
            Assert.That(X.ClimbPuffs("land"), Is.EqualTo(UnityTuning.ClimbLandPuffs));
            Assert.That(X.ClimbPuffs("up"), Is.EqualTo(0), "the climb itself streams instead");
            Assert.That(X.ClimbStreams("up"), Is.True);
            Assert.That(X.ClimbStreams("top"), Is.False);
            Assert.That(X.ClimbPuffs("nonsense"), Is.EqualTo(0));
            Assert.That(X.ClimbPuffs(null), Is.EqualTo(0));
        }
    }
}
