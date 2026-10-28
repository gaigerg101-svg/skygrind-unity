// NIGHT74 builder L (pads74; Game/Pad74.cs: the view parts, which need the Editor; the pure parts are Pad74MathTests.cs). Pinned:
//   1. the numbers against the shipped stream: the same 3 m of rise, fewer a second, bigger, a size floor that is never sub-pixel at 25 m, room for the burst, the sheet's fades as the pure tests assume
//   2. the texture (RGBA32, 1024 x 32, mip chain, clamped) and the material (the sheet as the base map, the glow material's colour times the tint, the glow material left alone)
//   3. the steady twin: a fixed seed, the tuned rate / life / speed / size, the sheet animation, a size that never falls under the floor, the renderer built OFF; Apply picks exactly one
//      renderer, both ways, null-safe
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Pad74Tests
    {
        [Test]
        public void TheNumbers_AreSteadierThanTheShippedStream()
        {
            Assert.AreEqual(UnityTuning.Pad27StreamSpeed * UnityTuning.Pad27StreamLife, UnityTuning.N74PadStreamSpeed * UnityTuning.N74PadStreamLife, 0.05f, "the same rise");
            Assert.Less(UnityTuning.N74PadStreamRate, UnityTuning.Pad27StreamRate, "fewer a second");
            Assert.Less(UnityTuning.N74PadStreamSpeed, UnityTuning.Pad27StreamSpeed, "slower");
            Assert.Greater(UnityTuning.N74PadStreamSize, UnityTuning.Pad27StreamSize, "bigger");
            Assert.Less(UnityTuning.N74PadBurstParticles, UnityTuning.Pad27BurstParticles, "a smaller burst");
            Assert.GreaterOrEqual(UnityTuning.N74PadStreamMax, UnityTuning.N74PadStreamRate * UnityTuning.N74PadStreamLife + UnityTuning.N74PadBurstParticles, "room for the burst on top of the steady stream");
            float smallest = UnityTuning.N74PadStreamSize * UnityTuning.N74PadSizeFloor;
            float px25 = smallest * 1080f / (2f * 25f * Mathf.Tan(45f * Mathf.Deg2Rad)); // pixels at 25 m for a 90 degree vertical field of view at 1080p
            Assert.GreaterOrEqual(px25, 3f, "a mote is never under 3 px at 25 m");
            Assert.AreEqual(0, UnityTuning.N74PadSpriteTexels & (UnityTuning.N74PadSpriteTexels - 1), "a power of two");
            // Pad74MathTests assumes these (a pure file cannot read the tuning)
            Assert.AreEqual(32, UnityTuning.N74PadSpriteTexels);
            Assert.AreEqual(32, UnityTuning.N74PadSpriteFrames);
            Assert.AreEqual(0.30f, UnityTuning.N74PadFadeIn, 1e-6f);
            Assert.AreEqual(0.45f, UnityTuning.N74PadFadeOut, 1e-6f);
            Assert.AreEqual(7401u, UnityTuning.N74PadSeed);
            // the beam's brightness (Pad74MathTests.BeamFactor_...): the numbers it assumes, and that the column keeps its presence
            Assert.AreEqual(0.5f, UnityTuning.N74PadBeamScale, 1e-6f);
            Assert.AreEqual(1.0f, UnityTuning.N74PadBeamNearM, 1e-6f);
            Assert.AreEqual(4.0f, UnityTuning.N74PadBeamFarM, 1e-6f);
            Assert.AreEqual(0.4f, UnityTuning.N74PadBeamNearFloor, 1e-6f);
            Assert.Greater(UnityTuning.N74PadBeamScale, 0.3f, "softened, not gone: the pad's far read is its column");
            Assert.GreaterOrEqual(UnityTuning.N74PadBeamScale * UnityTuning.N74PadBeamNearFloor, 0.1f, "even on top of the pad it keeps a tenth of the shipped light");
            Assert.Less(UnityTuning.N74PadBeamNearM, UnityTuning.N74PadBeamFarM);
        }

        [Test]
        public void SheetTexture_AndMaterial_AreWhatTheStreamReads()
        {
            Texture2D t = Pad74View.Sheet();
            Assert.AreSame(t, Pad74View.Sheet(), "made once");
            Assert.AreEqual(UnityTuning.N74PadSpriteTexels * UnityTuning.N74PadSpriteFrames, t.width);
            Assert.AreEqual(UnityTuning.N74PadSpriteTexels, t.height);
            Assert.AreEqual(TextureFormat.RGBA32, t.format);
            Assert.GreaterOrEqual(t.mipmapCount, 6, "mip 5 is one frame");
            Assert.AreEqual(TextureWrapMode.Clamp, t.wrapMode);
            int n = UnityTuning.N74PadSpriteTexels, f = UnityTuning.N74PadSpriteFrames;
            Color mid = t.GetPixel(f / 2 * n + n / 2, n / 2);
            Assert.Greater(mid.r, 0.9f, "the middle frame's centre is near whole");
            Color edge = t.GetPixel(f / 2 * n, n / 2);
            Assert.Less(edge.r, 0.02f, "and its border is dark");

            var glow = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            glow.SetColor("_BaseColor", new Color(0.2f, 1.0f, 1.25f, 1f));
            try
            {
                Material m = Pad74View.StreamMaterial(glow);
                Assert.IsNotNull(m);
                Assert.AreNotSame(glow, m, "a copy: the pad's glow material is left alone");
                Assert.AreSame(t, m.GetTexture("_BaseMap"));
                Color c = m.GetColor("_BaseColor");
                Assert.AreEqual(0.2f * UnityTuning.N74PadStreamTint, c.r, 1e-5f);
                Assert.AreEqual(1.25f * UnityTuning.N74PadStreamTint, c.b, 1e-5f);
                Assert.IsNull(glow.GetTexture("_BaseMap"), "the glow material has no sheet");
                Assert.AreEqual(1.25f, glow.GetColor("_BaseColor").b, 1e-5f);
                Assert.AreSame(m, Pad74View.StreamMaterial(glow), "one per glow material");
                Assert.IsNull(Pad74View.StreamMaterial(null));
            }
            finally { Object.DestroyImmediate(glow); }
        }

        [Test]
        public void SteadyTwin_IsBuiltToTheNumbersWithItsRendererOff_AndApplyPicksOne()
        {
            var glow = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            var root = new GameObject("pad74 test");
            try
            {
                ParticleSystem ps = Pad74View.MakeSteady(root.transform, 2.5f, 3, glow);
                Assert.IsNotNull(ps);
                Assert.IsFalse(ps.useAutoRandomSeed, "a fixed seed");
                Assert.AreEqual(Pad74Math.SeedFor(UnityTuning.N74PadSeed, 3), ps.randomSeed);
                Assert.AreEqual(UnityTuning.N74PadStreamRate, ps.emission.rateOverTime.constant, 1e-5f);
                Assert.AreEqual(UnityTuning.N74PadStreamLife, ps.main.startLifetime.constant, 1e-5f);
                Assert.AreEqual(UnityTuning.N74PadStreamSpeed, ps.main.startSpeed.constant, 1e-5f);
                Assert.AreEqual(UnityTuning.N74PadStreamSize, ps.main.startSize.constant, 1e-5f);
                Assert.AreEqual(UnityTuning.N74PadStreamMax, ps.main.maxParticles);
                Assert.AreEqual(ParticleSystemSimulationSpace.Local, ps.main.simulationSpace);
                Assert.IsTrue(ps.textureSheetAnimation.enabled);
                Assert.AreEqual(UnityTuning.N74PadSpriteFrames, ps.textureSheetAnimation.numTilesX);
                Assert.AreEqual(1, ps.textureSheetAnimation.numTilesY);
                Assert.AreEqual(ParticleSystemAnimationType.WholeSheet, ps.textureSheetAnimation.animation);
                Assert.AreEqual(1, ps.textureSheetAnimation.cycleCount);
                AnimationCurve size = ps.sizeOverLifetime.size.curve;
                Assert.AreEqual(UnityTuning.N74PadSizeFloor, size.Evaluate(0f), 1e-4f, "never smaller than the floor at birth");
                Assert.AreEqual(UnityTuning.N74PadSizeFloor, size.Evaluate(1f), 1e-4f, "or at death");
                for (int i = 0; i <= 20; i++) Assert.GreaterOrEqual(size.Evaluate(i / 20f), UnityTuning.N74PadSizeFloor - 1e-3f, "the curve at " + i / 20f);
                var rend = ps.GetComponent<ParticleSystemRenderer>();
                Assert.IsFalse(rend.enabled, "built off: the row decides, every frame");
                Assert.AreSame(Pad74View.StreamMaterial(glow), rend.sharedMaterial);

                var other = new GameObject("shipped stand-in").AddComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>();
                other.transform.SetParent(root.transform, false);
                Assert.IsTrue(other.enabled);
                Pad74View.Apply(true, other, rend);
                Assert.IsFalse(other.enabled, "row on: the shipped stream stops rendering");
                Assert.IsTrue(rend.enabled, "and the steady twin renders");
                Pad74View.Apply(false, other, rend);
                Assert.IsTrue(other.enabled, "row off: the shipped stream again");
                Assert.IsFalse(rend.enabled, "and not the twin");
                Pad74View.Apply(true, null, rend);
                Pad74View.Apply(false, other, null);
                Assert.IsTrue(other.enabled);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Object.DestroyImmediate(glow);
            }
        }
    }
}
