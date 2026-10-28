// NIGHT72 builder B (Game/Spring72.cs, the second-order spring of body72 and spring72). Pinned (the maths only: no engine call):
//   1. a step converges to its target (position within 1 mm and speed under 1 mm/s after 3 s) for a critically damped, an under-damped and a stiff spring
//   2. the step response's peak overshoots by Spring72Math.Overshoot(zeta) (the formula exp(-pi z / sqrt(1 - z^2))) within 3 % of the step, for z 0.3 / 0.5 / 0.7
//   3. a critical spring (z 1) does not overshoot
//   4. a spring following a target at a constant speed trails by SteadyLag(speed, hz, zeta) within 5 % once it has settled
//   5. a big dt is stable (a 50 ms frame at 8 Hz converges, no blow-up): the sub-steps keep w h under 0.5
//   6. Volume: stretching one axis by s and the other two by Volume(s) keeps the volume at 1 (the slate's squash bar: the product of scales 1 +- 0.02)
//   7. (NIGHT73) the flinch's tuning (UnityTuning.Spring72Flinch*): a clear squash at 60 and 30 fps inside the cap, a stretch on the way back, quiet by the next shot (named below, FlinchRun)
using System;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Spring72Tests
    {
        static float Peak(float zeta, float hz)
        {
            float x = 0f, v = 0f, peak = 0f;
            for (int i = 0; i < 600; i++) // 5 s at 120 Hz
            {
                Spring72Math.Step(ref x, ref v, 1f, hz, zeta, 1f / 120f);
                peak = Math.Max(peak, x);
            }
            return peak;
        }

        [TestCase(1.0f, 3f)]
        [TestCase(0.4f, 3f)]
        [TestCase(0.7f, 8f)]
        public void A_step_converges(float zeta, float hz)
        {
            float x = 0f, v = 0f;
            for (int i = 0; i < 360; i++) Spring72Math.Step(ref x, ref v, 2f, hz, zeta, 1f / 120f);
            Assert.That(Math.Abs(x - 2f), Is.LessThan(0.001f), "position after 3 s");
            Assert.That(Math.Abs(v), Is.LessThan(0.001f), "speed after 3 s");
        }

        [TestCase(0.3f)]
        [TestCase(0.5f)]
        [TestCase(0.7f)]
        public void The_overshoot_matches_the_formula(float zeta)
        {
            float over = Peak(zeta, 3f) - 1f;
            Assert.That(over, Is.EqualTo(Spring72Math.Overshoot(zeta)).Within(0.03f), "z " + zeta);
        }

        [Test]
        public void A_critical_spring_does_not_overshoot()
        {
            Assert.That(Peak(1f, 3f), Is.LessThanOrEqualTo(1.0005f));
            Assert.That(Spring72Math.Overshoot(1f), Is.EqualTo(0f));
        }

        [Test]
        public void A_following_spring_trails_by_the_steady_lag()
        {
            float hz = 4f, zeta = 0.35f, speed = 10f, dt = 1f / 120f, x = 0f, v = 0f, t = 0f;
            for (int i = 0; i < 480; i++)
            {
                Spring72Math.Step(ref x, ref v, speed * t, hz, zeta, dt); // the target at the start of the step (the way the view reads the rigid centre), then time moves on
                t += dt;
            }
            float lag = speed * t - x, want = Spring72Math.SteadyLag(speed, hz, zeta);
            Assert.That(lag, Is.EqualTo(want).Within(want * 0.05f));
        }

        [Test]
        public void A_long_frame_is_stable()
        {
            float x = 0f, v = 0f;
            for (int i = 0; i < 120; i++) Spring72Math.Step(ref x, ref v, 1f, 8f, 0.5f, 0.05f);
            Assert.That(Math.Abs(x - 1f), Is.LessThan(0.001f));
            Assert.That(float.IsNaN(x) || float.IsInfinity(x), Is.False);
        }

        // 7. NIGHT73 (the lead: "no player can see it"): the flinch's tuning, read from UnityTuning so a later retune cannot quietly drop the read. A hit kicks the spring's speed; it is stepped a
        // frame at a time the way EnemyViews.Spring72.cs steps it (Time.deltaTime, here 1 / 60 and 1 / 30). Pinned (Tools/night/n73-spring-mirror.py agrees: 17.0 % at 33 ms, 5.0 % stretch,
        // under 1.5 % from 133 ms; the NIGHT72 numbers -4.5 / 5 Hz / z 0.3 gave 8.2 %, 2.9 %, 183 ms):
        //   a. the first squash is at least Spring72PeakMin at 60 fps and at least Spring73FlinchGreenMin at 30 fps, and with the idle breath on top stays inside Spring72MaxSquash
        //   b. it rings back through a stretch of at least Spring72StretchMin (the snap, not a dead stop)
        //   c. it is under Spring72SettledShare, and stays there, by Spring72SettledS s (a faster settle than NIGHT72's: the spring is quiet before the next shot of a 0.5 s cadence)
        static void FlinchRun(float fps, out float squash, out float stretch, out float settledAt)
        {
            float x = 0f, v = UnityTuning.Spring72FlinchKick, dt = 1f / fps;
            squash = stretch = 0f;
            settledAt = 0f;
            int frames = (int)(fps * 1.5f);
            for (int i = 1; i <= frames; i++)
            {
                Spring72Math.Step(ref x, ref v, 0f, UnityTuning.Spring72FlinchHz, UnityTuning.Spring72FlinchZeta, dt);
                if (-x > squash) squash = -x;
                if (squash > 0.02f && x > stretch) stretch = x;
                if (Math.Abs(x) > UnityTuning.Spring72SettledShare) settledAt = i * dt; // the last frame still over the line
            }
        }

        [Test]
        public void The_flinch_squashes_clearly_at_60_and_30_fps_inside_the_cap()
        {
            FlinchRun(60f, out float squash60, out _, out _);
            FlinchRun(30f, out float squash30, out _, out _);
            Assert.That(squash60, Is.GreaterThanOrEqualTo(UnityTuning.Spring72PeakMin), "the squash at 60 fps");
            Assert.That(squash30, Is.GreaterThanOrEqualTo(UnityTuning.Spring73FlinchGreenMin), "the squash at 30 fps");
            Assert.That(squash60 + UnityTuning.Spring72BreathAmp, Is.LessThanOrEqualTo(UnityTuning.Spring72MaxSquash), "the flinch and the breath stay inside the squash cap");
        }

        [Test]
        public void The_flinch_rings_back_through_a_stretch_and_is_quiet_by_the_next_shot()
        {
            FlinchRun(60f, out _, out float stretch, out float settledAt);
            Assert.That(stretch, Is.GreaterThanOrEqualTo(UnityTuning.Spring72StretchMin), "the first rebound is a stretch");
            Assert.That(settledAt, Is.LessThanOrEqualTo(UnityTuning.Spring72SettledS), "the last frame over " + UnityTuning.Spring72SettledShare + " (s)");
        }

        [TestCase(1.3f)]
        [TestCase(0.8f)]
        public void Volume_is_kept(float stretch)
        {
            float other = Spring72Math.Volume(stretch);
            Assert.That(stretch * other * other, Is.EqualTo(1f).Within(0.02f));
        }
    }
}
