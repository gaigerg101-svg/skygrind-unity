// NIGHT20 U3-34.7 (builder V): the AIR streaks' pure rules (Game/SpeedLines20.cs; Research/2026-10-06-speed-lines.md 3.2):
// the intensity curve clamp((v - 9) / 15, 0, 1)^0.6 (sprint 0.25, slide 0.35, boost 1.0) on v_eff (the vertical at 0.6);
// nothing inside the rho 0.33 clear ellipse and every pixel under 0.30 alpha out to 0.55; the tail = head + v x T (a strafe
// sideways, a fall below the head: the flow upward), its length clamped; 0 streaks with REDUCED MOTION; x0.3 zoomed; the
// flow's density (L2: ~11 alive sprinting); the release half-life; the enemy rule's segment test; the clip; the birth ring.
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using M = Skygrind.Game.SpeedLines20Math;

namespace Skygrind.Game.Tests
{
    public class SpeedLines20Tests
    {
        [Test]
        public void TheIntensityCurveGatesAWalkAndFillsAtTheBoost()
        {
            Assert.That(M.Intensity(8), Is.EqualTo(0), "a walk never shows");
            Assert.That(M.Intensity(9), Is.EqualTo(0), "the gate");
            Assert.That(M.Intensity(10.5), Is.EqualTo(0.2512).Within(0.001), "the sprint: 0.25");
            Assert.That(M.Intensity(11.6), Is.EqualTo(0.3494).Within(0.001), "the slide floor: 0.35");
            Assert.That(M.Intensity(16.5), Is.EqualTo(System.Math.Pow(0.5, 0.6)).Within(1e-9), "half way: 0.5^0.6");
            Assert.That(M.Intensity(24), Is.EqualTo(1).Within(1e-12), "the boost: whole");
            Assert.That(M.Intensity(40), Is.EqualTo(1), "never past whole");
            Assert.That(M.Intensity(double.NaN), Is.EqualTo(0), "no speed: none");
        }

        [Test]
        public void TheSpeedSignalCountsTheVerticalAtSixTenths()
        {
            Assert.That(M.VEff(3, 0, 4), Is.EqualTo(5).Within(1e-12), "horizontal in full");
            Assert.That(M.VEff(0, -13.5, 0), Is.EqualTo(8.1).Within(1e-9), "a jump's fall: 8.1 ...");
            Assert.That(M.Intensity(M.VEff(0, -13.5, 0)), Is.EqualTo(0), "... stays under the gate");
            Assert.That(M.Intensity(M.VEff(0, -40, 0)), Is.EqualTo(1).Within(1e-12), "a slam at 40 m/s: whole");
        }

        [Test]
        public void NothingIsDrawnInsideTheClearEllipse()
        {
            Assert.That(M.Rho(0, 0), Is.EqualTo(0), "the crosshair");
            Assert.That(M.RhoPx(960, 540, 1920, 1080), Is.EqualTo(0).Within(1e-9), "the screen's middle");
            Assert.That(M.RhoPx(1920, 540, 1920, 1080), Is.EqualTo(1).Within(1e-9), "the right edge's midpoint");
            Assert.That(M.RhoPx(960, 0, 1920, 1080), Is.EqualTo(1).Within(1e-9), "the bottom edge's midpoint: an ellipse");
            Assert.That(M.Clear(0.2), Is.EqualTo(0));
            Assert.That(M.Clear(0.33), Is.EqualTo(0).Within(1e-12));
            Assert.That(M.Clear(0.44), Is.InRange(0.01, 0.99), "the ramp");
            Assert.That(M.Clear(0.55), Is.EqualTo(1).Within(1e-12));
            for (double rho = 0; rho <= 0.33; rho += 0.01)
                Assert.That(M.PixelAlpha(1, rho), Is.EqualTo(0), "no pixel inside rho 0.33 (rho " + rho + ")");
            for (double rho = 0.34; rho < 0.55; rho += 0.005)
                Assert.That(M.PixelAlpha(UnityTuning.SpeedLinesAlphaBurst, rho), Is.LessThan(0.30), "a burst's pixel under 0.30 out to 0.55 (rho " + rho + ")");
            Assert.That(M.PixelAlpha(UnityTuning.SpeedLinesAlphaFlow, 0.56), Is.EqualTo(UnityTuning.SpeedLinesAlphaFlow).Within(0.02), "the flow whole from 0.55");
            Assert.That(M.PixelAlpha(UnityTuning.SpeedLinesAlphaBurst, 0.75), Is.EqualTo(UnityTuning.SpeedLinesAlphaBurst).Within(1e-9), "a burst whole past the cap");
        }

        [Test]
        public void TheTailIsTheTravelOverTheExposure()
        {
            // the research's 0.040-0.065; build 2's reel (V2: the sprint at 61 px) and the samples: 0.026-0.050 along k^1.5
            Assert.That(M.Exposure(0), Is.EqualTo(0.026).Within(1e-12));
            Assert.That(M.Exposure(1), Is.EqualTo(0.050).Within(1e-12));
            Assert.That(M.Exposure(0.25), Is.EqualTo(0.029).Within(1e-12), "the sprint: 0.029 s");
            Assert.That(M.Exposure(0.5), Is.EqualTo(0.026 + 0.024 * System.Math.Pow(0.5, 1.5)).Within(1e-12));
            var head = new Vector3(2, 1.5f, 8);
            Vector3 strafe = M.Tail(head, new Vector3(10, 0, 0), 0.05, Vector3.forward) - head;
            Assert.That(strafe.x, Is.EqualTo(0.5f).Within(1e-5), "a strafe: along the travel ...");
            Assert.That(strafe.y, Is.EqualTo(0).Within(1e-6), "... sideways only");
            Assert.That(strafe.z, Is.EqualTo(0).Within(1e-6));
            Vector3 fall = M.Tail(head, new Vector3(0, -20, 0), 0.05, Vector3.forward) - head;
            Assert.That(fall.y, Is.EqualTo(-1f).Within(1e-5), "a fall: the tail under the head (the bright end on top, the flow upward)");
            Assert.That(fall.x, Is.EqualTo(0).Within(1e-6));
            Assert.That((M.Tail(head, new Vector3(100, 0, 0), 0.065, Vector3.forward) - head).magnitude, Is.EqualTo(2.5f).Within(1e-4), "never longer than 2.5 m");
            Assert.That((M.Tail(head, new Vector3(1, 0, 0), 0.04, Vector3.forward) - head).magnitude, Is.EqualTo(0.15f).Within(1e-5), "never shorter than 0.15 m");
            Vector3 still = M.Tail(head, Vector3.zero, 0.05, Vector3.forward) - head;
            Assert.That(still.z, Is.EqualTo(0.15f).Within(1e-5), "a stop: a dot along the last direction");
        }

        [Test]
        public void ReducedMotionDrawsNoStreak()
        {
            Assert.That(M.StreakAlpha(0.3, 0.1, 0.38, 6, 1, 1, false), Is.GreaterThan(0), "a streak in flight shows");
            Assert.That(M.StreakAlpha(0.3, 0.1, 0.38, 6, 1, 1, true), Is.EqualTo(0), "REDUCED MOTION: none");
            Assert.That(M.StreakAlpha(0.3, 0.1, 0.38, 6, 1, 0, false), Is.EqualTo(0), "EFFECTS INTENSITY 0: none");
            Assert.That(M.SpawnRate(1, true, false), Is.EqualTo(0), "no births with the motion gate shut");
            Assert.That(M.StreakAlpha(0.3, 0.1, 0.38, 6, 1, 0.5, false), Is.EqualTo(0.5 * M.StreakAlpha(0.3, 0.1, 0.38, 6, 1, 1, false)).Within(1e-12), "x EFFECTS INTENSITY");
        }

        [Test]
        public void ZoomedStreaksDrawAtThreeTenths()
        {
            Assert.That(M.AdsScale(0), Is.EqualTo(1).Within(1e-12));
            Assert.That(M.AdsScale(1), Is.EqualTo(0.3).Within(1e-12), "fully zoomed: x0.3");
            Assert.That(M.StreakAlpha(0.3, 0.1, 0.38, 6, M.AdsScale(1), 1, false), Is.EqualTo(0.3 * M.StreakAlpha(0.3, 0.1, 0.38, 6, 1, 1, false)).Within(1e-12));
        }

        [Test]
        public void TheFlowsDensityFollowsTheSpeed()
        {
            Assert.That(M.SpawnRate(M.Intensity(8), false, true), Is.EqualTo(0), "a walk: no births (L2: 0 alive)");
            Assert.That(M.SpawnRate(1, false, true), Is.EqualTo(UnityTuning.SpeedLinesRate).Within(1e-9), "the boost's flow");
            Assert.That(M.SpawnRate(1, true, true), Is.EqualTo(2 * UnityTuning.SpeedLinesRate).Within(1e-9), "x2 for the boost's 0.25 s");
            Assert.That(M.SpawnRate(0.5, false, true), Is.EqualTo(0.5 * UnityTuning.SpeedLinesRate).Within(1e-9), "density by speed");
            // the sprint's ~11 on screen: TheScreenLengthsAndDirectionsMeetL3AndL4ByConstruction (the visible share)
            // a birth is on screen at 92 % (the ring's area inside the screen: the rho 0.40-1.10 band less its 1.0-1.1 caps); 0.9 here
            double onset = UnityTuning.SpeedLinesBoostBurst * M.BoostShare(0) + M.SpawnRate(1, true, true) * 0.05 * 0.9;
            Assert.That(onset, Is.GreaterThanOrEqualTo(35), "L2: >= 35 within 50 ms of even a partial boost's onset from a standstill (the sprint's ~11 come on top)");
            Assert.That(M.BoostShare(0), Is.EqualTo(0.6).Within(1e-12), "a partial boost: 0.6 of the burst");
            Assert.That(M.BoostShare(1), Is.EqualTo(1).Within(1e-12));
            Assert.That(M.Life(0), Is.EqualTo(0.38 * 0.75).Within(1e-12), "life 0.38 s - 25 %");
            Assert.That(M.Life(1), Is.EqualTo(0.38 * 1.25).Within(1e-12), "+ 25 %");
        }

        [Test]
        public void TheIntensityRisesAtOnceAndFallsWithItsHalfLife()
        {
            Assert.That(M.SmoothK(0, 1, 0.001), Is.EqualTo(1), "attack: instant");
            Assert.That(M.SmoothK(1, 0, 0.08), Is.EqualTo(0.5).Within(1e-9), "release: half in 0.08 s");
            Assert.That(M.SmoothK(1, 0, 0.16), Is.EqualTo(0.25).Within(1e-9));
            Assert.That(M.SmoothK(0.4, 0.4, 1), Is.EqualTo(0.4).Within(1e-12), "steady");
            Assert.That(M.SmoothK(0.8, 0.2, 0), Is.EqualTo(0.8), "frozen in a hitstop (dt 0)");
        }

        [Test]
        public void AStreakFadesInAndOutOverItsLifeAndDistance()
        {
            Assert.That(M.LifeFade(0), Is.EqualTo(0));
            Assert.That(M.LifeFade(0.15), Is.EqualTo(1).Within(1e-12));
            Assert.That(M.LifeFade(0.5), Is.EqualTo(1).Within(1e-12));
            Assert.That(M.LifeFade(1), Is.EqualTo(0).Within(1e-12));
            Assert.That(M.DistFade(1), Is.EqualTo(0), "nothing an inch from the eye");
            Assert.That(M.DistFade(6), Is.EqualTo(1).Within(1e-12));
            Assert.That(M.DistFade(15), Is.EqualTo(0).Within(1e-12));
        }

        [Test]
        public void AStreakNearAnEnemyIsCaught()
        {
            var r = new Rect(100, 100, 50, 80);
            Assert.That(M.SegmentHitsRect(0, 0, 300, 300, r), Is.True, "crossing it");
            Assert.That(M.SegmentHitsRect(110, 110, 120, 120, r), Is.True, "inside it");
            Assert.That(M.SegmentHitsRect(0, 0, 90, 300, r), Is.False, "beside it");
            Assert.That(M.SegmentHitsRect(0, 300, 300, 300, r), Is.False, "above it");
            Assert.That(M.SegmentHitsRect(125, 0, 125, 90, r), Is.False, "short of it");
        }

        [Test]
        public void TheClipFindsThePartInsideTheClearZone()
        {
            Assert.That(M.InsideSpan(0, 0, 1, 0, 0.33, out double t0, out double t1), Is.True);
            Assert.That(t0, Is.EqualTo(0).Within(1e-9));
            Assert.That(t1, Is.EqualTo(0.33).Within(1e-9));
            Assert.That(M.InsideSpan(-1, 0.1, 1, 0.1, 0.33, out t0, out t1), Is.True, "straight through");
            Assert.That((t1 - t0) * 2, Is.EqualTo(2 * System.Math.Sqrt(0.33 * 0.33 - 0.01)).Within(1e-9));
            Assert.That(M.InsideSpan(0.5, 0.5, 1, 1, 0.33, out _, out _), Is.False, "outside");
        }

        /// <summary>
        /// The drawn streaks of a steady move on a 1920 x 1080 screen, by the view's own rules: births on the ring at their
        /// depth, each `age` into its life (the eye moving on at `v`, view space: x right, y up, z ahead), drawn where its alpha
        /// shows; the visible part's length (outside the clear ellipse) and the segment's angle. Returns the medians.
        /// </summary>
        static void Sample(Vector3 v, bool burst, double fovDeg, out float medianLen, out float medianAngle, out int drawn, out double onScreen) =>
            Sample(v, burst, fovDeg, 0, 0, out medianLen, out medianAngle, out drawn, out onScreen);

        static void Sample(Vector3 v, bool burst, double fovDeg, float floor, out float medianLen, out float medianAngle, out int drawn, out double onScreen) =>
            Sample(v, burst, fovDeg, floor, 0, out medianLen, out medianAngle, out drawn, out onScreen);

        /// <summary>The same over a floor `floor` m under the eye and beside a wall `wall` m to the right (0: none): a birth whose
        /// ray meets either first moves in front of it along the ray (M.FreeAlong), or redraws (the view's SpawnTries), or is
        /// lost; the screen length capped by M.CapPx; a streak under the report floor (alpha, px) not counted.</summary>
        static void Sample(Vector3 v, bool burst, double fovDeg, float floor, float wall, out float medianLen, out float medianAngle, out int drawn, out double onScreen)
        {
            var rng = new Mulberry32(20261006);
            float tanV = (float)System.Math.Tan(fovDeg * 0.5 * System.Math.PI / 180), tanH = tanV * 16f / 9f;
            double k = M.Intensity(M.VEff(v.x, v.y, v.z));
            double exposure = burst ? UnityTuning.SpeedLinesBurstExposure : M.Exposure(k);
            var lens = new float[4000];
            var angs = new float[4000];
            drawn = 0;
            for (int i = 0; i < lens.Length; i++)
            {
                Vector3 born = Vector3.zero;
                bool placed = false;
                for (int attempt = 0; attempt < UnityTuning.SpeedLinesSpawnTries && !placed; attempt++)
                {
                    double theta = rng.Next() * System.Math.PI * 2;
                    double rho = burst ? M.RingRho(rng.Next(), UnityTuning.SpeedLinesBurstRingIn, UnityTuning.SpeedLinesBurstRingOut) : M.RingRho(rng.Next(), UnityTuning.SpeedLinesRingIn, UnityTuning.SpeedLinesRingOut);
                    double d = burst ? UnityTuning.SpeedLinesBurstDepthMin + rng.Next() * (UnityTuning.SpeedLinesBurstDepthMax - UnityTuning.SpeedLinesBurstDepthMin)
                        : UnityTuning.SpeedLinesDepthMin + rng.Next() * (UnityTuning.SpeedLinesDepthMax - UnityTuning.SpeedLinesDepthMin);
                    born = M.BirthView((float)(rho * System.Math.Cos(theta)), (float)(rho * System.Math.Sin(theta)), (float)d, tanH, tanV);
                    placed = true;
                    if (floor > 0 || wall > 0)
                    {
                        Vector3 dir = born.normalized;
                        double hit = floor > 0 && dir.y < -1e-6f ? floor / -dir.y : double.PositiveInfinity;
                        if (wall > 0 && dir.x > 1e-6f) hit = System.Math.Min(hit, wall / dir.x);
                        if (hit < born.magnitude)
                        {
                            placed = M.FreeAlong(hit, rng.Next(), out double along);
                            if (placed) born = dir * (float)along;
                        }
                    }
                }
                if (!placed) continue; // lost: every try met the floor too close
                double life = burst ? UnityTuning.SpeedLinesBurstLife : M.Life(rng.Next());
                double age = rng.Next() * life;
                Vector3 head = born - v * (float)age;
                if (head.z <= 0.1f) continue;
                double alpha = M.StreakAlpha(burst ? UnityTuning.SpeedLinesAlphaBurst : UnityTuning.SpeedLinesAlphaFlow, age, life, head.magnitude, 1, 1, false) * M.FadeShare(k);
                if (alpha < UnityTuning.SpeedLinesReportAlpha) continue; // the view neither draws nor counts it
                Vector3 tail = M.Tail(head, v, exposure, Vector3.forward);
                if (!M.ProjectView(head.x, head.y, head.z, tanH, tanV, 1920, 1080, 0.1f, out Vector2 a)) continue;
                if (!M.ProjectView(tail.x, tail.y, tail.z, tanH, tanV, 1920, 1080, 0.1f, out Vector2 b)) continue;
                float seg = Vector2.Distance(a, b), cap = M.CapPx(k, 1080);
                if (seg > cap) b = Vector2.Lerp(a, b, cap / seg); // the view pulls the tail in along its own line
                double ax = 2 * a.x / 1920 - 1, ay = 2 * a.y / 1080 - 1, bx = 2 * b.x / 1920 - 1, by = 2 * b.y / 1080 - 1;
                double inside = M.InsideSpan(ax, ay, bx, by, UnityTuning.SpeedLinesClearIn, out double t0, out double t1) ? t1 - t0 : 0;
                float visible = Vector2.Distance(a, b) * (float)System.Math.Max(0, M.ShareInRect(a.x, a.y, b.x, b.y, new Rect(0, 0, 1920, 1080)) - inside);
                if (visible < UnityTuning.SpeedLinesReportPx) continue; // off the screen or inside the clear zone: nothing seen
                lens[drawn] = visible;
                angs[drawn] = (float)M.AcuteAngleDeg(b.x - a.x, b.y - a.y);
                drawn++;
            }
            medianLen = M.Median(lens, drawn);
            medianAngle = M.Median(angs, drawn);
            // the steady flow's streaks on screen at once: births a second x the mean life x the share seen
            onScreen = M.SpawnRate(k, false, true) * UnityTuning.SpeedLinesLife * drawn / lens.Length;
            System.Console.WriteLine($"  sample v {v} burst {burst} fov {fovDeg}: seen {drawn} of {lens.Length}, flow on screen {onScreen:F1}, median length {medianLen:F1} px, angle {medianAngle:F1} deg");
        }

        [Test]
        public void TheScreenLengthsAndDirectionsMeetL3AndL4ByConstruction()
        {
            // L3: the sprint (10.5 m/s ahead, FOV 74 + the sprint's 5.95) 25-45 px; the boost (24 m/s, FOV 74 + 8 + the kick) 80-140 px
            Sample(new Vector3(0, 0, 10.5f), false, 80, out float sprint, out _, out _, out double sprintSeen);
            Sample(new Vector3(0, 0, 24), false, 90, out float boostFlow, out _, out _, out _);
            Sample(new Vector3(0, 0, 24), true, 90, out float boostBurst, out _, out _, out _);
            Sample(new Vector3(17, 0, 0), false, 80, out _, out float strafe, out int n1, out _);
            Sample(new Vector3(0, -40, 0), false, 80, out _, out float slam, out int n2, out _);
            Sample(new Vector3(0, 0, 8), false, 74, out _, out _, out _, out double walkSeen);
            Assert.That(sprintSeen, Is.InRange(7.0, 15.0), "L2: 11 +- 4 on screen while sprinting");
            Assert.That(walkSeen, Is.EqualTo(0), "L2: 0 at a walk");
            Assert.That(sprint, Is.InRange(25f, 45f), "L3: the sprint's median length (1080p px)");
            Assert.That(boostFlow, Is.InRange(80f, 140f), "L3: the boost's flow");
            Assert.That(boostBurst, Is.InRange(80f, 140f), "L3: the boost's burst");
            // L4: a pure strafe draws within 20 deg of horizontal; a slam within 20 deg of vertical
            Assert.That(n1, Is.GreaterThan(100));
            Assert.That(strafe, Is.LessThanOrEqualTo(20f), "L4: a strafe's streaks lie flat");
            Assert.That(n2, Is.GreaterThan(100));
            Assert.That(slam, Is.GreaterThanOrEqualTo(70f), "L4: a slam's streaks stand up");
        }

        [Test]
        public void TheSprintOverAFloorMeetsL2AndL3()
        {
            // build 2's reel (V1, V2): the held sprint drew a median of 4 streaks at 61 px: the floor (1.6 m under the eye) took
            // the screen's lower half past ~4 m and every birth there was dropped; now it moves in front of the floor
            Sample(new Vector3(0, 0, 10.5f), false, 80, 1.6f, out float len, out _, out _, out double seen);
            Assert.That(seen, Is.InRange(7.0, 15.0), "L2: 11 +- 4 on screen sprinting over a floor");
            Assert.That(len, Is.InRange(25f, 45f), "L3: 25-45 px sprinting over a floor");
            Sample(new Vector3(0, 0, 24), false, 90, 1.6f, out float boost, out _, out _, out _);
            Assert.That(boost, Is.InRange(80f, 140f), "L3: the boost's flow over a floor");
            Sample(new Vector3(0, 0, 24), true, 90, 1.6f, out float burst, out _, out _, out _);
            Assert.That(burst, Is.InRange(80f, 140f), "L3: the boost's burst over a floor");
            Assert.That(M.FreeAlong(10, 0, out double a0), Is.True);
            Assert.That(a0, Is.EqualTo(UnityTuning.SpeedLinesFreeMin).Within(1e-12), "the nearest free place");
            Assert.That(M.FreeAlong(10, 1, out double a1), Is.True);
            Assert.That(a1, Is.EqualTo(10 - UnityTuning.SpeedLinesFreeMargin).Within(1e-12), "the farthest: off the surface");
            Assert.That(M.FreeAlong(UnityTuning.SpeedLinesFreeMin, 0.5, out _), Is.False, "no room: redraw");
        }

        [Test]
        public void TheScreenLengthIsCappedBySpeedAndTheStreaksFadeWithIt()
        {
            // build 3's reel: V3 (the sprint at 59 px on the real map), V5 (streaks 0.45 s after the speed fell under the gate)
            Assert.That(M.CapPx(0.25, 1080), Is.EqualTo(40).Within(1e-4), "the sprint's cap");
            Assert.That(M.CapPx(0.1, 1080), Is.EqualTo(40).Within(1e-4), "slower: the same");
            Assert.That(M.CapPx(1, 1080), Is.EqualTo(130).Within(1e-4), "the boost's");
            Assert.That(M.CapPx(1, 2160), Is.EqualTo(260).Within(1e-3), "x the screen's height / 1080");
            Assert.That(M.FadeShare(0), Is.EqualTo(0), "under the gate: none");
            Assert.That(M.FadeShare(0.25), Is.EqualTo(1).Within(1e-12), "the sprint: whole");
            Assert.That(M.FadeShare(0.06), Is.EqualTo(0.5).Within(1e-9));
            // from the boost (k 1) to a stop: the fade's k under the report floor (alpha 0.3 x share < 0.02) well inside 0.45 s
            double kf = 1;
            int frames = 0;
            while (UnityTuning.SpeedLinesAlphaBurst * M.FadeShare(kf) >= UnityTuning.SpeedLinesReportAlpha && frames < 1000)
            {
                kf = M.SmoothKWith(kf, 0, 1.0 / 60, UnityTuning.SpeedLinesFadeHalfLife);
                frames++;
            }
            Assert.That(frames / 60.0, Is.LessThan(0.3), "gone (alpha < 0.02) within 0.3 s of a stop, from even a burst");
        }

        [Test]
        public void TheSprintBesideAWallMeetsL3()
        {
            // the real map: a wall 3 m to the right and the floor 1.6 m below (build 3's sprint lane): the births in front of the wall
            // sit close and draw long; the cap holds the median
            Sample(new Vector3(0, 0, 10.5f), false, 80, 1.6f, 3f, out float len, out _, out _, out double seen);
            Assert.That(len, Is.InRange(25f, 45f), "L3: 25-45 px sprinting beside a wall over a floor");
            Assert.That(seen, Is.InRange(7.0, 15.0), "L2: 11 +- 4");
            Sample(new Vector3(0, 0, 24), false, 90, 1.6f, 3f, out float boost, out _, out _, out _);
            Assert.That(boost, Is.InRange(80f, 140f), "L3: the boost beside a wall");
        }

        [Test]
        public void BirthsStayInTheirRingAndAnglesReadFromTheHorizontal()
        {
            Assert.That(M.RingRho(0, 0.40, 1.10), Is.EqualTo(0.40).Within(1e-12));
            Assert.That(M.RingRho(1, 0.40, 1.10), Is.EqualTo(1.10).Within(1e-12));
            Assert.That(M.RingRho(0.5, 0.40, 1.10), Is.EqualTo(System.Math.Sqrt((0.16 + 1.21) / 2)).Within(1e-12), "area-uniform");
            Assert.That(M.AcuteAngleDeg(10, 0), Is.EqualTo(0).Within(1e-9), "horizontal");
            Assert.That(M.AcuteAngleDeg(-10, 0), Is.EqualTo(0).Within(1e-9));
            Assert.That(M.AcuteAngleDeg(0, 5), Is.EqualTo(90).Within(1e-9), "vertical");
            Assert.That(M.AcuteAngleDeg(5, -5), Is.EqualTo(45).Within(1e-5));
            float[] v = { 5, 1, 4, 2, 3 };
            Assert.That(M.Median(v, 5), Is.EqualTo(3));
            Assert.That(M.Median(v, 0), Is.EqualTo(0), "none: 0");
        }
    }
}
