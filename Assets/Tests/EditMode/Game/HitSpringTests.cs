// NIGHT18 U3-31.4 (builder R, tests first and seen red): Game/HitSpring.cs, the view-only damped spring a body hit kicks
// (Source's view punch: a hit adds velocity, the spring overshoots once), and HitReactBody, one hit slot's reaction on it:
// a full hit peaks inside M5's 50 ms and is back by 200 ms, it overshoots (the squash rebounds into a stretch), the path is
// the same at any frame rate, a stream of hits holds the body out on one side instead of jerking it, pellets in one step
// stack only up to the cap, REDUCED MOTION stills it, and nothing allocates.
using System;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class HitSpringTests
    {
        const float K = UnityTuning.HitReactSpring, C = UnityTuning.HitReactDamping;
        const float Frame = 1f / 60;

        /// <summary>A full hit on a fresh body, then `frames` frames (the game's order: the hit, the frame's advance, the read).</summary>
        static void Run(int want, float scale, bool motionOn, int frames, float[] lat, float[] sq)
        {
            HitReactBody b = HitReactBody.Rest;
            b.Hit(want, 1, scale, motionOn);
            for (int f = 0; f < frames; f++)
            {
                b.Advance(Frame);
                lat[f] = b.Lateral;
                sq[f] = b.Squash;
            }
        }

        [Test]
        public void TheAdvanceIsTheExactDampedPath()
        {
            float sigma = C / 2, wd = (float)Math.Sqrt(K - sigma * sigma);
            var s = new HitSpring { x = 0, v = 1 };
            s.Advance(0.05f, K, C);
            double want = Math.Exp(-sigma * 0.05) * Math.Sin(wd * 0.05) / wd;
            Assert.That(s.x, Is.EqualTo(want).Within(1e-5), "x after 50 ms from a unit kick");
            // Source's own constants (65, 9) are underdamped too: a kick from rest comes back past 0 (the overshoot)
            var src = new HitSpring { x = 0, v = 1 };
            float min = 0;
            for (int i = 0; i < 120; i++)
            {
                src.Advance(Frame, 65, 9);
                min = Math.Min(min, src.x);
            }
            Assert.That(min, Is.LessThan(0), "Source's punch overshoots");
        }

        [Test]
        public void ThePeakTimeAndSizeMatchTheSampledPath()
        {
            float tp = HitSpring.PeakTime(K, C), ppv = HitSpring.PeakPerVelocity(K, C);
            Assert.That(tp, Is.InRange(0.02f, 0.045f), "the peak lands inside M5's 50 ms");
            float best = 0, bestT = 0;
            for (int i = 1; i <= 2000; i++)
            {
                var s = new HitSpring { x = 0, v = 1 };
                s.Advance(i * 0.0001f, K, C);
                if (s.x > best)
                {
                    best = s.x;
                    bestT = i * 0.0001f;
                }
            }
            Assert.That(bestT, Is.EqualTo(tp).Within(0.0002f));
            Assert.That(ppv, Is.EqualTo(best).Within(1e-5f));
        }

        [Test]
        public void AFullHitPeaksInsideThreeFramesAndShowsOnTheFirst()
        {
            float[] lat = new float[60], sq = new float[60];
            Run(1, 1, true, 60, lat, sq);
            int peakAt = 0;
            for (int f = 1; f < lat.Length; f++) if (lat[f] > lat[peakAt]) peakAt = f;
            Assert.That(peakAt, Is.LessThanOrEqualTo(2), "the peak by the third frame (50 ms)");
            Assert.That(lat[peakAt], Is.InRange(0.95f * UnityTuning.HitReactLatPeak, 1.001f * UnityTuning.HitReactLatPeak));
            Assert.That(lat[0], Is.GreaterThan(0.7f * UnityTuning.HitReactLatPeak), "the hit's own frame already shows it");
            float sqPeak = 0;
            for (int f = 0; f < 3; f++) sqPeak = Math.Max(sqPeak, sq[f]);
            Assert.That(sqPeak, Is.InRange(0.95f * UnityTuning.HitReactSquashPeak, 1.001f * UnityTuning.HitReactSquashPeak), "the squash peak");
        }

        [Test]
        public void ItOvershootsThenIsBackBy200Ms()
        {
            float[] lat = new float[60], sq = new float[60];
            Run(1, 1, true, 60, lat, sq);
            float latMin = 0, sqMin = 0;
            for (int f = 0; f < lat.Length; f++)
            {
                latMin = Math.Min(latMin, lat[f]);
                sqMin = Math.Min(sqMin, sq[f]);
            }
            Assert.That(latMin, Is.LessThan(-0.1f * UnityTuning.HitReactLatPeak), "the kick swings past rest");
            Assert.That(sqMin, Is.LessThan(-0.1f * UnityTuning.HitReactSquashPeak), "the squash rebounds into a stretch");
            for (int f = 11; f < lat.Length; f++) // frame index 11 = 200 ms after the hit
            {
                Assert.That(Math.Abs(lat[f]), Is.LessThan(0.04f * UnityTuning.HitReactLatPeak), $"lateral back at frame {f}");
                Assert.That(Math.Abs(sq[f]), Is.LessThan(0.04f * UnityTuning.HitReactSquashPeak), $"squash back at frame {f}");
            }
        }

        [Test]
        public void TheLeanFollowsTheKick()
        {
            HitReactBody b = HitReactBody.Rest;
            b.Hit(-1, 1, 1, true);
            b.Advance(2 * Frame);
            Assert.That(b.Lateral, Is.LessThan(0), "kicked to the side asked for");
            Assert.That(b.LeanDeg, Is.EqualTo(UnityTuning.HitReactLeanDeg * b.Lateral / UnityTuning.HitReactLatPeak).Within(1e-5f));
            Assert.That(b.LeanDeg, Is.LessThan(-0.9f * UnityTuning.HitReactLeanDeg), "near the full lean at the peak");
        }

        [Test]
        public void ACentredHitKeepsTheSideOrTakesTheFallback()
        {
            HitReactBody fresh = HitReactBody.Rest;
            fresh.Hit(0, -1, 1, true);
            fresh.Advance(2 * Frame);
            Assert.That(fresh.Lateral, Is.LessThan(0), "a fresh body takes the fallback side");

            HitReactBody kicked = HitReactBody.Rest;
            kicked.Hit(1, -1, 1, true);
            for (int i = 0; i < 60; i++) kicked.Advance(Frame); // a second later: settled, no stream
            kicked.Hit(0, -1, 1, true);
            kicked.Advance(2 * Frame);
            Assert.That(kicked.Lateral, Is.GreaterThan(0), "a centred hit keeps the side it was kicked to");
        }

        [Test]
        public void TheFrameRateDoesNotChangeThePath()
        {
            HitReactBody a = HitReactBody.Rest, b = HitReactBody.Rest;
            a.Hit(1, 1, 1, true);
            b.Hit(1, 1, 1, true);
            for (int i = 0; i < 5; i++) a.Advance(Frame); // 83.3 ms at 60 fps
            for (int i = 0; i < 12; i++) b.Advance(1f / 144); // 83.3 ms at 144 fps
            Assert.That(b.Lateral, Is.EqualTo(a.Lateral).Within(1e-4f));
            Assert.That(b.Squash, Is.EqualTo(a.Squash).Within(1e-4f));
        }

        [Test]
        public void AStreamHoldsTheBodyOutOnOneSide()
        {
            // the STITCHER's 12 a second for 1 s, at its smallest kick, landing on alternate sides of the body's axis
            HitReactBody b = HitReactBody.Rest;
            float interval = 1f / 12, sinceShot = interval, t = 0, minLate = float.MaxValue;
            int shots = 0;
            while (t < 1f)
            {
                if (sinceShot >= interval - 1e-6f)
                {
                    b.Hit(shots % 2 == 0 ? 1 : -1, 1, UnityTuning.HitReactMinScale, true);
                    shots++;
                    sinceShot = 0;
                }
                b.Advance(Frame);
                t += Frame;
                sinceShot += Frame;
                Assert.That(b.Lateral, Is.GreaterThan(0), $"the stream never swings it to the other side (t {t:0.000})");
                if (t > 0.5f) minLate = Math.Min(minLate, b.Lateral);
            }
            Assert.That(b.hold, Is.GreaterThan(0.1f), "the stream built a hold");
            Assert.That(minLate, Is.GreaterThan(0.25f * UnityTuning.HitReactMinScale * UnityTuning.HitReactLatPeak), "held out between the hits");
            for (int i = 0; i < 36; i++) b.Advance(Frame); // 0.6 s after the last hit
            Assert.That(Math.Abs(b.Lateral), Is.LessThan(0.005f), "back once the stream stops");
            Assert.That(Math.Abs(b.Squash), Is.LessThan(0.005f));
        }

        [Test]
        public void PelletsInOneStepStackOnlyUpToTheCap()
        {
            HitReactBody b = HitReactBody.Rest;
            float scale = HitReactBody.Scale(8, 25, false, true); // a THUNDERCLAP pellet: 8 damage, a heavy gun
            for (int i = 0; i < 8; i++) b.Hit(1, 1, scale, true);
            Assert.That(b.hold, Is.EqualTo(0), "pellets in one step are not a stream");
            float latMax = 0, sqMax = 0;
            for (int f = 0; f < 30; f++)
            {
                b.Advance(Frame);
                latMax = Math.Max(latMax, b.Lateral);
                sqMax = Math.Max(sqMax, b.Squash);
            }
            Assert.That(latMax, Is.GreaterThan(UnityTuning.HitReactLatPeak), "the blast reads bigger than one revolver hit");
            Assert.That(latMax, Is.LessThanOrEqualTo(UnityTuning.HitReactLatCap * 1.001f), "never past the cap");
            Assert.That(sqMax, Is.LessThanOrEqualTo(UnityTuning.HitReactSquashCap * 1.001f));
        }

        [Test]
        public void ReducedMotionStillsIt()
        {
            float[] lat = new float[30], sq = new float[30];
            Run(1, 1, false, 30, lat, sq);
            for (int f = 0; f < lat.Length; f++)
            {
                Assert.That(lat[f], Is.EqualTo(0), $"lateral at frame {f}");
                Assert.That(sq[f], Is.EqualTo(0), $"squash at frame {f}");
            }
        }

        [Test]
        public void TheScaleFollowsTheDamageAndTheWeight()
        {
            Assert.That(HitReactBody.Scale(25, 25, false, false), Is.EqualTo(1f));
            Assert.That(HitReactBody.Scale(50, 25, false, false), Is.EqualTo(1f), "never past a full hit by damage");
            Assert.That(HitReactBody.Scale(9, 25, false, false), Is.EqualTo(UnityTuning.HitReactMinScale), "a nailgun's 9 still reads");
            Assert.That(HitReactBody.Scale(20, 25, false, false), Is.EqualTo(0.8f).Within(1e-6f));
            Assert.That(HitReactBody.Scale(25, 25, true, false), Is.EqualTo(UnityTuning.HitReactMelee));
            Assert.That(HitReactBody.Scale(25, 25, false, true), Is.EqualTo(UnityTuning.HitReactHeavy));
            Assert.That(HitReactBody.Scale(25, 0, false, false), Is.EqualTo(1f), "no reference damage: a full hit");
        }

        [Test]
        public void ARestingBodyIsResting()
        {
            HitReactBody b = HitReactBody.Rest;
            Assert.That(b.Resting, Is.True);
            Assert.That(b.Lateral, Is.EqualTo(0));
            Assert.That(b.Squash, Is.EqualTo(0));
            b.Hit(1, 1, 1, true);
            Assert.That(b.Resting, Is.False);
            for (int i = 0; i < 120; i++) b.Advance(Frame);
            Assert.That(b.Resting, Is.True, "two seconds later");
        }

        [Test]
        public void TheRangeScaleIsTheDistanceOverTheReferenceWithinItsClamps()
        {
            Assert.That(HitReactBody.RangeScale(UnityTuning.HitReactRefDistance), Is.EqualTo(1f).Within(1e-6f));
            Assert.That(HitReactBody.RangeScale(6), Is.EqualTo(6f / UnityTuning.HitReactRefDistance).Within(1e-6f));
            Assert.That(HitReactBody.RangeScale(20), Is.EqualTo(20f / UnityTuning.HitReactRefDistance).Within(1e-6f));
            Assert.That(HitReactBody.RangeScale(1), Is.EqualTo(UnityTuning.HitReactRangeMin), "close up the world size holds");
            Assert.That(HitReactBody.RangeScale(100), Is.EqualTo(UnityTuning.HitReactRangeMax), "far off it stops growing");
            Assert.That(HitReactBody.RangeScale(0), Is.EqualTo(UnityTuning.HitReactRangeMin));
            Assert.That(HitReactBody.RangeScale(float.NaN), Is.EqualTo(UnityTuning.HitReactRangeMin), "no distance: the close size");
        }

        [Test]
        public void ARevolverHitsScreenSizeHoldsFrom3To20M()
        {
            // a pinhole camera: the kick's screen size goes as its drawn world size over the distance; it holds from 3 m to 20 m
            float at12 = UnityTuning.HitReactLatPeak * HitReactBody.RangeScale(12) / 12;
            for (float d = 3; d <= 20.001f; d += 0.5f)
                Assert.That(UnityTuning.HitReactLatPeak * HitReactBody.RangeScale(d) / d, Is.EqualTo(at12).Within(at12 * 0.001f), $"at {d} m");
            // outside it changes gently: at 2 m at most 1.6 x, at 30 m at least 0.75 x
            Assert.That(UnityTuning.HitReactLatPeak * HitReactBody.RangeScale(2) / 2, Is.LessThanOrEqualTo(1.6f * at12), "2 m");
            Assert.That(UnityTuning.HitReactLatPeak * HitReactBody.RangeScale(30) / 30, Is.GreaterThanOrEqualTo(0.75f * at12), "30 m");
        }

        [Test]
        public void NothingAllocates()
        {
            HitReactBody b = HitReactBody.Rest;
            b.Hit(1, 1, 1, true);
            b.Advance(Frame);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 10000; i++)
            {
                if (i % 5 == 0) b.Hit(i % 2 == 0 ? 1 : -1, 1, 0.5f, true);
                b.Advance(Frame);
                _ = b.Lateral + b.Squash + b.LeanDeg;
            }
            Assert.That(GC.GetAllocatedBytesForCurrentThread() - before, Is.EqualTo(0));
        }
    }
}
