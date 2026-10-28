// NIGHT17 U3-30.5 (builder X): the crunch's pure rules (Game/Chunks.cs CrunchMath, ChunkRing): the chunk pool's cap under
// the quality preset and its oldest-first recycling, the blocks a part splits into, the hit flash and tilt curves, the
// victim's decaying vibration, the trauma shake's curve (trauma^2, capped, stilled at scale 0), the floor bounce, the
// muzzle star's two frames under FLASH INTENSITY, and the impact frame's multikill gate.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class CrunchTests
    {
        [Test]
        public void TheChunkCapIsAShareOfThePresetsParticleCapWithinThePool()
        {
            Assert.That(CrunchMath.ChunkCap(256, 160, 0.6), Is.EqualTo(96), "LOW");
            Assert.That(CrunchMath.ChunkCap(256, 400, 0.6), Is.EqualTo(240), "MEDIUM / HIGH");
            Assert.That(CrunchMath.ChunkCap(256, 1000, 0.6), Is.EqualTo(256), "never past the pool");
            Assert.That(CrunchMath.ChunkCap(256, 0.1, 0.6), Is.EqualTo(1), "at least one");
        }

        [Test]
        public void TheRingRecyclesTheOldestFirstAndACapKillsWhatIsPastIt()
        {
            var ring = new ChunkRing(8);
            Assert.That(ring.Pool, Is.EqualTo(8));
            Assert.That(ring.Cap, Is.EqualTo(8), "the cap starts at the pool");
            ring.SetCap(3);
            Assert.That(ring.Take(), Is.EqualTo(0));
            Assert.That(ring.Take(), Is.EqualTo(1));
            Assert.That(ring.Take(), Is.EqualTo(2));
            Assert.That(ring.Live, Is.EqualTo(3));
            Assert.That(ring.Take(), Is.EqualTo(0), "full: the oldest is recycled");
            Assert.That(ring.Live, Is.EqualTo(3), "a recycled slot stays one block");
            ring.Kill(1);
            Assert.That(ring.Alive(1), Is.False);
            Assert.That(ring.Live, Is.EqualTo(2));
            ring.SetCap(8);
            for (int i = 0; i < 8; i++) ring.Take();
            ring.SetCap(2);
            Assert.That(ring.Alive(5), Is.False, "past a lowered cap: dead at once");
            Assert.That(ring.Live, Is.LessThanOrEqualTo(2));
            int s = ring.Take();
            Assert.That(s, Is.LessThan(2), "a take stays inside the cap");
            ring.Clear();
            Assert.That(ring.Live, Is.EqualTo(0));
        }

        [Test]
        public void APartSplitsIntoBlocksOfAboutACellCappedPerSide()
        {
            Assert.That(CrunchMath.Cells(1.8f, 0.42f, 4), Is.EqualTo(4), "the husk's body is tall");
            Assert.That(CrunchMath.Cells(0.8f, 0.42f, 4), Is.EqualTo(2), "and two wide");
            Assert.That(CrunchMath.Cells(0.3f, 0.42f, 4), Is.EqualTo(1), "a small part stays one block");
            Assert.That(CrunchMath.Cells(5f, 0.42f, 4), Is.EqualTo(4), "capped");
            Assert.That(CrunchMath.Cells(0f, 0.42f, 4), Is.EqualTo(1), "never none");
        }

        [Test]
        public void TheHitFlashIsFullWhiteAtOnceThenEasesBack()
        {
            Assert.That(CrunchMath.HitFlash(0, 0.03f, 0.1f), Is.EqualTo(1f));
            Assert.That(CrunchMath.HitFlash(0.03f, 0.03f, 0.1f), Is.EqualTo(1f), "held");
            float a = CrunchMath.HitFlash(0.06f, 0.03f, 0.1f), b = CrunchMath.HitFlash(0.1f, 0.03f, 0.1f);
            Assert.That(a, Is.LessThan(1f).And.GreaterThan(b), "easing back");
            Assert.That(b, Is.GreaterThan(0f));
            Assert.That(CrunchMath.HitFlash(0.13f, 0.03f, 0.1f), Is.EqualTo(0f).Within(1e-6), "gone at hold + fade");
            Assert.That(CrunchMath.HitFlash(5f, 0.03f, 0.1f), Is.EqualTo(0f));
        }

        [Test]
        public void TheTiltRisesThenFallsInsideItsWindow()
        {
            Assert.That(CrunchMath.Tilt(0, 0.025f, 0.07f), Is.EqualTo(0f).Within(1e-6));
            Assert.That(CrunchMath.Tilt(0.025f, 0.025f, 0.07f), Is.EqualTo(1f).Within(1e-6), "peak");
            Assert.That(CrunchMath.Tilt(0.06f, 0.025f, 0.07f), Is.GreaterThan(0f).And.LessThan(1f));
            Assert.That(CrunchMath.Tilt(0.095f, 0.025f, 0.07f), Is.EqualTo(0f).Within(1e-6), "back by 95 ms");
            Assert.That(CrunchMath.Tilt(1f, 0.025f, 0.07f), Is.EqualTo(0f));
        }

        [Test]
        public void TheVictimVibratesLargeFirstAndStopsAtItsTime()
        {
            Assert.That(CrunchMath.Vib(0, 0.1f, 0.035f, 34f), Is.EqualTo(0.035f).Within(1e-6), "full at the hit");
            for (float t = 0; t < 0.1f; t += 0.004f) Assert.That(System.Math.Abs(CrunchMath.Vib(t, 0.1f, 0.035f, 34f)), Is.LessThanOrEqualTo(0.035f + 1e-6));
            Assert.That(System.Math.Abs(CrunchMath.Vib(0.09f, 0.1f, 0.035f, 34f)), Is.LessThan(0.035f * 0.05f), "decayed near the end");
            Assert.That(CrunchMath.Vib(0.1f, 0.1f, 0.035f, 34f), Is.EqualTo(0f));
            Assert.That(CrunchMath.Vib(1f, 0.1f, 0.035f, 34f), Is.EqualTo(0f));
        }

        [Test]
        public void TheShakeIsTraumaSquaredCappedAndStilledAtScaleZero()
        {
            Assert.That(CrunchMath.ShakeShare(0.5, 2, 1), Is.EqualTo(0.25).Within(1e-12));
            Assert.That(CrunchMath.ShakeShare(1, 2, 1), Is.EqualTo(1).Within(1e-12));
            Assert.That(CrunchMath.ShakeShare(0.5, 2, 0), Is.EqualTo(0), "REDUCED MOTION / effects 0");
            Assert.That(CrunchMath.ShakeShare(0, 2, 1), Is.EqualTo(0));
            Assert.That(CrunchMath.ShakeShare(0.3, 2, 1), Is.LessThan(CrunchMath.ShakeShare(0.6, 2, 1) / 3), "a light shake is far below a heavy one");
            Assert.That(CrunchMath.ShakeDeg(3, 1, 1, 1), Is.EqualTo(3), "never past the max");
            Assert.That(CrunchMath.ShakeDeg(3, 1, -1, -1), Is.EqualTo(-3));
            Assert.That(CrunchMath.ShakeDeg(2, 0.25, 0.5, 0), Is.EqualTo(0.25).Within(1e-12));
            Assert.That(CrunchMath.ShakeDeg(2, 0, 1, 1), Is.EqualTo(0));
            Assert.That(CrunchMath.Onset(0, 0.035), Is.EqualTo(1));
            Assert.That(CrunchMath.Onset(0.035, 0.035), Is.EqualTo(0));
            Assert.That(CrunchMath.Onset(0.02, 0.035), Is.GreaterThan(0).And.LessThan(1));
        }

        [Test]
        public void ABlockBouncesOffTheFloorAndComesToRest()
        {
            float y = -0.1f, vy = -6f, vx = 4f, vz = -2f;
            Assert.That(CrunchMath.Bounce(ref y, ref vy, ref vx, ref vz, 0f, 0.1f, 0.38f, 0.55f, 0.8f), Is.True, "it touched");
            Assert.That(y, Is.EqualTo(0.1f).Within(1e-6), "on the floor, not in it");
            Assert.That(vy, Is.EqualTo(6f * 0.38f).Within(1e-5), "back up with the bounce's share");
            Assert.That(vx, Is.EqualTo(4f * 0.55f).Within(1e-5));
            Assert.That(vz, Is.EqualTo(-2f * 0.55f).Within(1e-5));
            y = 0.05f;
            vy = -1f;
            CrunchMath.Bounce(ref y, ref vy, ref vx, ref vz, 0f, 0.1f, 0.38f, 0.55f, 0.8f);
            Assert.That(vy, Is.EqualTo(0f), "slow: it lies still");
            y = 0.5f;
            vy = -3f;
            Assert.That(CrunchMath.Bounce(ref y, ref vy, ref vx, ref vz, 0f, 0.1f, 0.38f, 0.55f, 0.8f), Is.False, "above the floor: no touch");
            Assert.That(vy, Is.EqualTo(-3f));
            y = 0.05f;
            vy = 2f;
            Assert.That(CrunchMath.Bounce(ref y, ref vy, ref vx, ref vz, 0f, 0.1f, 0.38f, 0.55f, 0.8f), Is.False, "rising: no touch");
        }

        [Test]
        public void TheMuzzleStarIsOneFramePlusAHalfStrengthSecondUnderFlashIntensity()
        {
            Assert.That(CrunchMath.StarStrength(0, 2, 0.5, 1), Is.EqualTo(1f));
            Assert.That(CrunchMath.StarStrength(1, 2, 0.5, 1), Is.EqualTo(0.5f));
            Assert.That(CrunchMath.StarStrength(2, 2, 0.5, 1), Is.EqualTo(0f), "never a third frame");
            Assert.That(CrunchMath.StarStrength(0, 2, 0.5, 0), Is.EqualTo(0f), "FLASH INTENSITY 0: none");
            Assert.That(CrunchMath.StarStrength(0, 2, 0.5, 0.4), Is.EqualTo(0.4f).Within(1e-6));
            Assert.That(CrunchMath.StarStrength(1, 1, 0.5, 1), Is.EqualTo(0f), "fx.flashFrames 1: one frame");
            Assert.That(CrunchMath.StarStrength(0, 2, 0.5, 3), Is.EqualTo(1f), "intensity clamped");
        }

        [Test]
        public void TheImpactFrameFiresOnEveryThirdKillOfAMultikillOnly()
        {
            Assert.That(CrunchMath.ImpactFrameDue(1, 3), Is.False);
            Assert.That(CrunchMath.ImpactFrameDue(2, 3), Is.False, "a double is not a big moment");
            Assert.That(CrunchMath.ImpactFrameDue(3, 3), Is.True);
            Assert.That(CrunchMath.ImpactFrameDue(4, 3), Is.False);
            Assert.That(CrunchMath.ImpactFrameDue(6, 3), Is.True);
            Assert.That(CrunchMath.ImpactFrameDue(0, 3), Is.False);
        }

        [Test]
        public void TheCrushFrameDarkensForItsHoldThenEasesBackUnderFlashIntensity()
        {
            Assert.That(CrunchMath.CrushEv(0, 0.025, 0.035, -2, 1), Is.EqualTo(-2).Within(1e-12), "full at once");
            Assert.That(CrunchMath.CrushEv(0.025, 0.025, 0.035, -2, 1), Is.EqualTo(-2).Within(1e-12), "held");
            double mid = CrunchMath.CrushEv(0.04, 0.025, 0.035, -2, 1);
            Assert.That(mid, Is.GreaterThan(-2).And.LessThan(0), "easing back");
            Assert.That(CrunchMath.CrushEv(0.06, 0.025, 0.035, -2, 1), Is.EqualTo(0).Within(1e-9), "gone by 60 ms");
            Assert.That(CrunchMath.CrushEv(1, 0.025, 0.035, -2, 1), Is.EqualTo(0));
            Assert.That(CrunchMath.CrushEv(-0.01, 0.025, 0.035, -2, 1), Is.EqualTo(0), "not before it starts");
            Assert.That(CrunchMath.CrushEv(0, 0.025, 0.035, -2, 0), Is.EqualTo(0), "FLASH INTENSITY 0: none");
            Assert.That(CrunchMath.CrushEv(0, 0.025, 0.035, -2, 0.5), Is.EqualTo(-1).Within(1e-12), "scaled");
            Assert.That(CrunchMath.CrushEv(0, 0.025, 0.035, -2, 4), Is.EqualTo(-2).Within(1e-12), "intensity clamped");
        }

        [Test]
        public void ABlockIsWhiteForItsFirstFramesOnlyThenEasesToItsColour()
        {
            Assert.That(CrunchMath.BlockWhite(0, 0, 2, 0.1f), Is.EqualTo(1f));
            Assert.That(CrunchMath.BlockWhite(1, 0, 2, 0.1f), Is.EqualTo(1f), "the second frame still white");
            float a = CrunchMath.BlockWhite(2, 0.02f, 2, 0.1f), b = CrunchMath.BlockWhite(3, 0.05f, 2, 0.1f);
            Assert.That(a, Is.LessThan(1f).And.GreaterThan(b), "then easing back");
            Assert.That(CrunchMath.BlockWhite(9, 0.1f, 2, 0.1f), Is.EqualTo(0f).Within(1e-6), "its own colour by the fade's end");
            Assert.That(CrunchMath.BlockWhite(30, 1f, 2, 0.1f), Is.EqualTo(0f));
        }

        [Test]
        public void EveryGunPicksItsOwnDeathSignature()
        {
            Assert.That(CrunchMath.SignatureOf("revolver", false), Is.EqualTo(DeathSig.Clean), "the KESTREL: a clean punch-through");
            Assert.That(CrunchMath.SignatureOf("ricochet", false), Is.EqualTo(DeathSig.Clean), "the KESTREL's bounce");
            Assert.That(CrunchMath.SignatureOf("shotgun", false), Is.EqualTo(DeathSig.Shove), "the THUNDERCLAP: shoved back, then burst");
            Assert.That(CrunchMath.SignatureOf("nailgun", false), Is.EqualTo(DeathSig.Nails), "the STITCHER: chips pinned on the nail's line");
            Assert.That(CrunchMath.SignatureOf("rocket", false), Is.EqualTo(DeathSig.Blast), "the COMET: a radial blast");
            Assert.That(CrunchMath.SignatureOf("rail", false), Is.EqualTo(DeathSig.Split), "the LONGWAVE: split along the beam");
            Assert.That(CrunchMath.SignatureOf("gauntlet", true), Is.EqualTo(DeathSig.Punch), "the PULSE GAUNTLET");
            Assert.That(CrunchMath.SignatureOf("punch", true), Is.EqualTo(DeathSig.Punch));
            Assert.That(CrunchMath.SignatureOf("revolver", true), Is.EqualTo(DeathSig.Punch), "any melee kill: the punch's");
            Assert.That(CrunchMath.SignatureOf("prism", false), Is.EqualTo(DeathSig.Clean), "a gun without its own: clean");
            Assert.That(CrunchMath.SignatureOf(null, false), Is.EqualTo(DeathSig.Clean));
        }

        [Test]
        public void EachSignatureReadsDifferently()
        {
            SigParams clean = CrunchMath.ParamsOf(DeathSig.Clean), shove = CrunchMath.ParamsOf(DeathSig.Shove),
                nails = CrunchMath.ParamsOf(DeathSig.Nails), blast = CrunchMath.ParamsOf(DeathSig.Blast),
                split = CrunchMath.ParamsOf(DeathSig.Split), punch = CrunchMath.ParamsOf(DeathSig.Punch);
            Assert.That(clean.leadHero, Is.True, "the KESTREL's hero on the shot line");
            Assert.That(shove.coneDeg, Is.GreaterThan(clean.coneDeg), "the THUNDERCLAP's cone is wider");
            Assert.That(shove.slide, Is.GreaterThanOrEqualTo(1f).And.LessThanOrEqualTo(2f), "shoved back 1-2 m");
            Assert.That(nails.pinned, Is.True);
            Assert.That(nails.blockScale, Is.LessThan(1f), "smaller chips");
            Assert.That(nails.keepShare, Is.LessThan(1f), "fewer chips");
            Assert.That(nails.holdTime, Is.GreaterThan(0f), "pinned for a beat");
            Assert.That(blast.radial, Is.True);
            Assert.That(blast.ringScale, Is.GreaterThan(clean.ringScale), "a bigger floor ring");
            Assert.That(split.split, Is.True);
            Assert.That(split.slide, Is.GreaterThan(0f), "the halves slide apart");
            Assert.That(punch.sig, Is.EqualTo(DeathSig.Punch));
            foreach (DeathSig s in new[] { DeathSig.Clean, DeathSig.Shove, DeathSig.Nails, DeathSig.Blast, DeathSig.Split, DeathSig.Punch })
            {
                SigParams p = CrunchMath.ParamsOf(s);
                Assert.That(p.sig, Is.EqualTo(s));
                Assert.That(p.keepShare, Is.GreaterThan(0f).And.LessThanOrEqualTo(1f), s + ": some blocks");
                Assert.That(p.blockScale, Is.GreaterThan(0f), s + ": a size");
            }
        }

        [Test]
        public void OverkillThrowsHarderWithinItsRange()
        {
            Assert.That(CrunchMath.Overkill(50, 100, 0.6f, 2.2f), Is.EqualTo(0.6f), "a finishing tap: the least");
            Assert.That(CrunchMath.Overkill(100, 100, 0.6f, 2.2f), Is.EqualTo(1f).Within(1e-6));
            Assert.That(CrunchMath.Overkill(400, 100, 0.6f, 2.2f), Is.EqualTo(2.2f), "capped");
            Assert.That(CrunchMath.Overkill(10, 0, 0.6f, 2.2f), Is.EqualTo(1f), "no max HP: as a plain kill");
        }
    }
}
