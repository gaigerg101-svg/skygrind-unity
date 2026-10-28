// PG3 builder V2 (trail74 and rim74; Game/Trail74Math.cs, Game/Rim74Math.cs, Game/UnityTuning.PG3V2.cs). Pinned (pure math and state, no scene):
//   1. trail74: the fall's share clamps and grows; a ring's radius ends exactly at its end radius (the slam ring's outer edge is the sim's shockwave radius) and its brightness goes to nothing; the slam's pieces
//      grow with the fall; the slide's share is nothing at a walk, the floor just past it, whole at the full speed; the three dash echoes get dimmer and are under the web's melee afterimages; every curve is
//      smooth at 60 fps (no frame-to-frame jump: nothing here flickers)
//   2. rim74: the glow skips THE RIVAL (magenta is the RIVAL's colour), the burrower and every boss kind; the distance share is whole far, a third near, nothing past the cap; the boss key swells to 1, holds,
//      settles to its level in about 2 s, and is smooth; the key's colour is a red-orange hue (not the parry's yellow, not magenta)
//   3. the rows' names are the ones the lead adds to design.n74, and the probe lines carry them
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class PG3V2Tests
    {
        // ---- trail74 ----

        [Test]
        public void Fall01_ClampsAndGrows()
        {
            Assert.AreEqual(0.0, Trail74Math.Fall01(0), 1e-9);
            Assert.AreEqual(0.0, Trail74Math.Fall01(UnityTuning.N74SlamFallMinM), 1e-9);
            Assert.AreEqual(1.0, Trail74Math.Fall01(UnityTuning.N74SlamFallMaxM), 1e-9);
            Assert.AreEqual(1.0, Trail74Math.Fall01(99), 1e-9);
            double last = -1;
            for (double d = 0; d <= 20; d += 0.5)
            {
                double f = Trail74Math.Fall01(d);
                Assert.GreaterOrEqual(f, last);
                last = f;
            }
        }

        [Test]
        public void Ring_EndsAtItsEndRadius_AndFadesToNothing()
        {
            Assert.AreEqual(0.7, Trail74Math.RingRadius(0.7, 5.0, 0), 1e-9);
            Assert.AreEqual(5.0, Trail74Math.RingRadius(0.7, 5.0, 1), 1e-9, "a floor ring's outer edge is the shockwave radius, never more");
            Assert.AreEqual(5.0, Trail74Math.RingRadius(0.7, 5.0, 3), 1e-9, "clamped past its life");
            double last = 0;
            for (double t = 0; t <= 1.0001; t += 0.02)
            {
                double r = Trail74Math.RingRadius(0.7, 5.0, t);
                Assert.GreaterOrEqual(r, last - 1e-12, "it only grows");
                Assert.LessOrEqual(r, 5.0 + 1e-9);
                last = r;
            }
            Assert.AreEqual(0.6, Trail74Math.RingBrightness(0.6, 0), 1e-9);
            Assert.AreEqual(0.0, Trail74Math.RingBrightness(0.6, 1), 1e-9);
        }

        [Test]
        public void Slam_PiecesGrowWithTheFall()
        {
            Assert.Less(Trail74Math.SlamRingPeak(0), Trail74Math.SlamRingPeak(1));
            Assert.GreaterOrEqual(Trail74Math.SlamRingPeak(Trail74Math.Fall01(4.1)), 0.6, "a 4 m fall (0.18 of the range) still reads: the lead's pg3-g floor is 0.6");
            Assert.GreaterOrEqual(UnityTuning.N74SlamPoolPeak, 0.4f);
            Assert.LessOrEqual(UnityTuning.N74SlamPoolShare, 1.0f, "the pool stays inside the blast's radius");
            Assert.Less(Trail74Math.SlamFlashSize(0), Trail74Math.SlamFlashSize(1));
            Assert.Less(Trail74Math.SlamDust(0), Trail74Math.SlamDust(1));
            Assert.Less(Trail74Math.SlamShards(0), Trail74Math.SlamShards(1));
            Assert.Less(Trail74Math.SlamDustSpeed(0), Trail74Math.SlamDustSpeed(1));
            Assert.Less(Trail74Math.SlamDustSize(0), Trail74Math.SlamDustSize(1));
            Assert.Less(Trail74Math.SlamDustLife(0), Trail74Math.SlamDustLife(1));
            Assert.Less(Trail74Math.SlamShardSpeed(0), Trail74Math.SlamShardSpeed(1));
            Assert.AreEqual(UnityTuning.N74SlamDustMin, Trail74Math.SlamDust(0));
            Assert.AreEqual(UnityTuning.N74SlamDustMax, Trail74Math.SlamDust(1));
            Assert.IsFalse(Trail74Math.SlamHasInner(0.1));
            Assert.IsTrue(Trail74Math.SlamHasInner(0.9));
            Assert.LessOrEqual(UnityTuning.N74SlamDustMax, UnityTuning.N74TrailPuffPool, "one slam's puffs fit the pool");
            Assert.LessOrEqual(UnityTuning.N74SlamShardsMax, UnityTuning.N74TrailSparkPool);
        }

        [Test]
        public void SlideShare_NothingAtAWalk_FloorJustPast_WholeAtFull()
        {
            Assert.AreEqual(0.0, Trail74Math.SlideShare(5, 6.0, 20), 1e-9);
            Assert.AreEqual(0.0, Trail74Math.SlideShare(6, 6.0, 20), 1e-9);
            Assert.AreEqual(UnityTuning.N74SlideCueFloor, Trail74Math.SlideShare(6.1, 6.0, 20), 1e-9);
            Assert.AreEqual(1.0, Trail74Math.SlideShare(20, 6.0, 20), 1e-9);
            Assert.AreEqual(1.0, Trail74Math.SlideShare(40, 6.0, 20), 1e-9);
            double mid = Trail74Math.SlideShare(13, 6.0, 20);
            Assert.AreEqual(0.5, mid, 1e-9);
        }

        [Test]
        public void Step_RisesFast_FallsSlow_HoldsInAHitstop()
        {
            Assert.AreEqual(0.3, Trail74Math.Step(0.3, 1, 0, 0.04, 0.2), 1e-12, "dt 0 (a hitstop) holds");
            Assert.AreEqual(1.0, Trail74Math.Step(0, 1, 0.04, 0.04, 0.2), 1e-12);
            Assert.AreEqual(0.8, Trail74Math.Step(1, 0, 0.04, 0.04, 0.2), 1e-12, "falls over 0.2 s: 0.04 s is 0.2 of the way");
            double k = 1;
            for (int i = 0; i < 20; i++) k = Trail74Math.Step(k, 0, 1.0 / 60, 0.04, 0.2);
            Assert.AreEqual(0.0, k, 1e-12, "gone within a third of a second");
        }

        [Test]
        public void DashEchoes_AreDimmerEach_AndUnderTheWebsMeleeAfterimages()
        {
            double[] web = { 0.34, 0.20, 0.10 }; // PortExport/config.json viewmodel.afterimages.opacity
            for (int i = 0; i < UnityTuning.N74DashGhostAlpha.Length; i++)
            {
                Assert.LessOrEqual(UnityTuning.N74DashGhostAlpha[i], web[i] + 1e-6, "never over the web's own melee afterimages (GG called the fist's cyan smear too much, n36-r3; pg3-g: the lead asked for a peak near 0.55, which is the web's 0.34 x 1.6)");
                if (i > 0) Assert.Less(Trail74Math.GhostBrightness(i, 1), Trail74Math.GhostBrightness(i - 1, 1));
                Assert.Greater(Trail74Math.GhostScale(i), 1.0);
                if (i > 0) Assert.Greater(Trail74Math.GhostScale(i), Trail74Math.GhostScale(i - 1));
            }
            Assert.AreEqual(0.0, Trail74Math.GhostBrightness(0, 0), 1e-12, "no dash, no echo");
            Assert.AreEqual(0.0, Trail74Math.GhostBrightness(9, 1), 1e-12);
            Assert.Greater(Trail74Math.GhostBrightness(0, 1), 0.5, "the lead's pg3-g ask: a first echo near 0.55 so a still shows it");
            Assert.Less(Trail74Math.GhostBrightness(0, 1), 0.7, "an echo, never a flash");
        }

        [Test]
        public void TrailCurves_AreSmoothAt60Fps_NothingFlickers()
        {
            double dt = 1.0 / 60;
            double lastRing = Trail74Math.RingBrightness(1, 0), lastPuff = Trail74Math.PuffBrightness(0), lastSeg = Trail74Math.SegBrightness(0), lastSpark = Trail74Math.SparkBrightness(0);
            for (int i = 1; i <= 60; i++)
            {
                double t = i * dt / 0.5; // a half-second life
                if (t > 1) t = 1;
                double ring = Trail74Math.RingBrightness(1, t), puff = Trail74Math.PuffBrightness(t), seg = Trail74Math.SegBrightness(t), spark = Trail74Math.SparkBrightness(t);
                Assert.LessOrEqual(System.Math.Abs(ring - lastRing), 0.12, "ring brightness steps are small");
                Assert.LessOrEqual(System.Math.Abs(puff - lastPuff), UnityTuning.N74PuffPeak * 0.3, "puff brightness steps are small (under a third of its peak a frame)");
                Assert.LessOrEqual(System.Math.Abs(seg - lastSeg), 0.08, "ground-strip brightness steps are small");
                Assert.LessOrEqual(System.Math.Abs(spark - lastSpark), 0.12, "spark brightness steps are small");
                lastRing = ring; lastPuff = puff; lastSeg = seg; lastSpark = spark;
            }
            // monotone after the puff's rise: it never rises again (no pulse)
            double p = Trail74Math.PuffBrightness(0.2);
            for (double t = 0.2; t <= 1.0001; t += 0.01)
            {
                double now = Trail74Math.PuffBrightness(t);
                Assert.LessOrEqual(now, p + 1e-9);
                p = now;
            }
        }

        [Test]
        public void Drift_SlowsDown_AndStaysUnderItsLimit()
        {
            double limit = 1.0 / UnityTuning.N74PuffDrag;
            double last = 0;
            for (double t = 0; t <= 3; t += 0.1)
            {
                double d = Trail74Math.Drift(1, t);
                Assert.GreaterOrEqual(d, last);
                Assert.LessOrEqual(d, limit + 1e-9);
                last = d;
            }
        }

        [Test]
        public void Pools_AreSizedForTheirSpawners()
        {
            Assert.GreaterOrEqual(UnityTuning.N74TrailRingPool, 6);
            Assert.GreaterOrEqual(UnityTuning.N74TrailSegPool, 2 * (int)(UnityTuning.N74SlideSegLifeS / (UnityTuning.N74SlideSegMinM / 25.0)), "a 25 m/s slide lays a segment on each of two rails every 0.45 m: the pool holds a whole life of them");
            Assert.GreaterOrEqual(UnityTuning.N74TrailSparkPool, (int)(UnityTuning.N74SlideSparkRate * UnityTuning.N74SlideSparkLifeMax) + UnityTuning.N74SlamShardsMax);
            Assert.GreaterOrEqual(UnityTuning.N74TrailSpriteCap, UnityTuning.N74TrailPuffPool, "a sprite bucket holds every puff in the pool");
        }

        // ---- rim74 ----

        [Test]
        public void Halo_SkipsTheRivalTheBurrowerAndEveryBoss()
        {
            Assert.IsFalse(Rim74Math.WearsHalo("rival"), "magenta is the RIVAL's colour");
            Assert.IsFalse(Rim74Math.WearsHalo("burrower"));
            Assert.IsFalse(Rim74Math.WearsHalo(null));
            Assert.IsFalse(Rim74Math.WearsHalo(""));
            foreach (string boss in new[] { "bulwark", "brawler", "crown", "crown37", "glacier", "hulk", "lantern", "loom", "boss", "stag", "wick", "wyrm" })
            {
                Assert.IsTrue(Rim74Math.IsBossKind(boss), boss);
                Assert.IsFalse(Rim74Math.WearsHalo(boss), boss + " has the entrance key, not the per-enemy glow");
            }
            foreach (string kind in new[] { "husk", "swarmer", "spitter", "charger", "drone", "lancer", "warden" })
                Assert.IsTrue(Rim74Math.WearsHalo(kind), kind);
        }

        [Test]
        public void HaloShare_NothingClose_QuarterAtSix_WholeFar_NothingPastTheCap()
        {
            Assert.AreEqual(0.0, Rim74Math.HaloShare(0), 1e-9, "no halo at arm's length");
            Assert.AreEqual(0.0, Rim74Math.HaloShare(UnityTuning.N74HaloCloseM0), 1e-9);
            Assert.Less(Rim74Math.HaloShare(2.0), UnityTuning.N74HaloNearShare, "under the near share while the close fade is on");
            Assert.AreEqual(UnityTuning.N74HaloNearShare, Rim74Math.HaloShare(UnityTuning.N74HaloNearM), 1e-9);
            Assert.AreEqual(1.0, Rim74Math.HaloShare(UnityTuning.N74HaloFarM), 1e-9);
            Assert.AreEqual(1.0, Rim74Math.HaloShare(UnityTuning.N74HaloMaxM - UnityTuning.N74HaloMaxFadeM - 0.01), 1e-9, "whole until the fade band");
            Assert.Less(Rim74Math.HaloShare(UnityTuning.N74HaloMaxM - 1), 0.1, "nearly gone just inside the cap (no pop at it)");
            Assert.AreEqual(0.0, Rim74Math.HaloShare(UnityTuning.N74HaloMaxM), 1e-9);
            Assert.AreEqual(0.0, Rim74Math.HaloShare(500), 1e-9);
            double last = Rim74Math.HaloShare(0);
            for (double d = 0; d < UnityTuning.N74HaloMaxM - UnityTuning.N74HaloMaxFadeM; d += 0.5)
            {
                double s = Rim74Math.HaloShare(d);
                Assert.GreaterOrEqual(s, last - 1e-12, "farther is never dimmer inside the cap's fade band");
                Assert.LessOrEqual(s - last, 0.2, "no jump between half-metres (the close fade's steepest: 0.36 a metre)");
                last = s;
            }
            last = 1;
            for (double d = UnityTuning.N74HaloMaxM - UnityTuning.N74HaloMaxFadeM; d <= UnityTuning.N74HaloMaxM; d += 0.5)
            {
                double s = Rim74Math.HaloShare(d);
                Assert.LessOrEqual(s, last + 1e-12, "it only fades in the band");
                Assert.LessOrEqual(last - s, 0.2, "no jump between half-metres in the band");
                last = s;
            }
        }

        [Test]
        public void GlowProfile_IsRadial_WholeAtTheCentre_NothingAtTheEdge()
        {
            Assert.AreEqual(1.0, Rim74Math.GlowProfile(0), 1e-12);
            Assert.AreEqual(0.0, Rim74Math.GlowProfile(1), 1e-12);
            Assert.AreEqual(0.0, Rim74Math.GlowProfile(1.5), 1e-12);
            Assert.AreEqual(1.0, Rim74Math.GlowProfile(-0.3), 1e-12, "a negative radius is the centre");
            double last = 1;
            for (double r = 0; r <= 1.0001; r += 0.01)
            {
                double v = Rim74Math.GlowProfile(r);
                Assert.LessOrEqual(v, last + 1e-12, "it only falls");
                Assert.LessOrEqual(last - v, 0.06, "no step in the gradient (1 % of the radius)");
                last = v;
            }
            Assert.Less(Rim74Math.GlowProfile(0.99), 0.001, "no slope into the quad's edge: no ring where the sprite ends");
            Assert.Greater(Rim74Math.GlowProfile(0.4), 0.6, "the rim region (a body's silhouette sits near 0.2-0.4 of the half-size) stays bright");
        }

        [Test]
        public void HaloScale_ShrinksTowardTheEye_AndCrowdThins()
        {
            Assert.AreEqual(UnityTuning.N74HaloNearScale, Rim74Math.HaloScale(0), 1e-9);
            Assert.AreEqual(1.0, Rim74Math.HaloScale(UnityTuning.N74HaloFullSizeM), 1e-9);
            Assert.AreEqual(1.0, Rim74Math.HaloScale(60), 1e-9);
            double last = 0;
            for (double d = 0; d <= 20; d += 0.5)
            {
                double s = Rim74Math.HaloScale(d);
                Assert.GreaterOrEqual(s, last - 1e-12);
                last = s;
            }
            Assert.AreEqual(1.0, Rim74Math.CrowdShare(0), 1e-9);
            Assert.AreEqual(1.0, Rim74Math.CrowdShare(UnityTuning.N74HaloCrowdFree), 1e-9);
            Assert.AreEqual(System.Math.Sqrt((double)UnityTuning.N74HaloCrowdFree / 24), Rim74Math.CrowdShare(24), 1e-9, "24 glows share sqrt(free / 24) of their strength each");
            double prev = 1;
            for (int n = 0; n <= 64; n++)
            {
                double c = Rim74Math.CrowdShare(n);
                Assert.LessOrEqual(c, prev + 1e-12, "more glows never means brighter ones");
                Assert.LessOrEqual(n * c * c, UnityTuning.N74HaloCrowdFree + 1e-9, "n glows at c each: n c^2 never passes the free count (the crowd's light stays bounded)");
                prev = c;
            }
        }

        [Test]
        public void HaloExtents_HugTheBody_AndAreCapped()
        {
            // a husk-sized body: 1.8 m tall, 0.4 m radius
            double hh = Rim74Math.HaloHalfHeight(1.8), hw = Rim74Math.HaloHalfWidth(0.4, 1.8);
            Assert.AreEqual(0.9 * UnityTuning.N74HaloHalfHeightRatio, hh, 1e-9);
            Assert.AreEqual(0.4 * UnityTuning.N74HaloHalfWidthRadii, hw, 1e-9);
            Assert.Greater(hh, 0.9, "past the body's ends");
            Assert.Greater(hw, 0.4, "past the body's sides");
            Assert.Less(hh - 0.9, 1.0, "tight: under a metre of glow past the ends (the first glow was 4.7 m across and invisible)");
            Assert.Less(hw - 0.4, 0.6, "tight: under 0.6 m of glow past the sides");
            // the texture's value at the pill's silhouette (the glow's edge a viewer sees): bright enough to read
            Assert.Greater(Rim74Math.GlowProfile(0.4 / hw), 0.4, "the side's silhouette sits where the glow is still over 40 % of its centre");
            Assert.Greater(Rim74Math.GlowProfile(0.9 / hh), 0.25, "and the ends' not under a quarter");
            // the peak the eye sees at the side at 8 m on the dusk: peak x profile x the distance share, in 0..1 additive (the first glow: 0.04)
            double seen = UnityTuning.N74HaloPeak * Rim74Math.GlowProfile(0.4 / hw) * Rim74Math.HaloShare(8);
            Assert.Greater(seen, 0.15, "the rim at 6-25 m must be about ten times the first glow's 0.04 additive");
            Assert.AreEqual(UnityTuning.N74HaloMaxSizeM * 0.5, Rim74Math.HaloHalfHeight(50), 1e-9);
            Assert.AreEqual(0.0, Rim74Math.HaloHalfHeight(-1), 1e-9);
            Assert.LessOrEqual(Rim74Math.HaloHalfWidth(5, 1.8), Rim74Math.HaloHalfHeight(1.8) + 1e-9, "never wider than tall");
        }

        [Test]
        public void KeySwell_IsTheSwellOnly_AndTheWashStaysUnderAScreenFill()
        {
            Assert.AreEqual(0.0, Rim74Math.KeySwell(0), 1e-12);
            Assert.AreEqual(0.0, Rim74Math.KeySwell(UnityTuning.N74BossKeySettleLevel), 1e-12, "none once settled");
            Assert.AreEqual(1.0, Rim74Math.KeySwell(1), 1e-12);
            Assert.AreEqual(0.0, Rim74Math.KeySwell(0.1), 1e-12);
            double last = 0;
            for (double t = 0; t < 4; t += 1.0 / 60)
            {
                double s = Rim74Math.KeySwell(Rim74Math.KeyLevel(t));
                Assert.LessOrEqual(System.Math.Abs(s - last), 0.1, "the wash follows the smooth swell: no frame jumps");
                last = s;
            }
            Assert.AreEqual(0.0, last, 1e-9, "gone by 4 s");
            // the wash spans at most N74BossKeyWashMaxAngleDeg of view
            double tan = System.Math.Tan(UnityTuning.N74BossKeyWashMaxAngleDeg * 0.5 * System.Math.PI / 180);
            Assert.AreEqual(2 * 8 * tan, Rim74Math.KeyWashSize(100, 8), 1e-9, "capped by the angle at 8 m");
            Assert.AreEqual(6 * UnityTuning.N74BossKeyWashSizePerHeight, Rim74Math.KeyWashSize(6, 80), 1e-5, "a far boss is not capped");
            Assert.Greater(UnityTuning.N74BossKeyGlowPeak, 0.5f, "the aura reads (the first one, 0.34 behind a body that hid it, showed in no frame)");
        }

        [Test]
        public void KeyLevel_SwellsHoldsSettles_InAboutTwoSeconds_AndIsSmooth()
        {
            Assert.AreEqual(0.0, Rim74Math.KeyLevel(-1), 1e-12);
            Assert.AreEqual(0.0, Rim74Math.KeyLevel(0), 1e-12);
            Assert.AreEqual(1.0, Rim74Math.KeyLevel(UnityTuning.N74BossKeyRiseS), 1e-9);
            Assert.AreEqual(1.0, Rim74Math.KeyLevel(UnityTuning.N74BossKeyRiseS + UnityTuning.N74BossKeyHoldS * 0.5), 1e-9);
            Assert.AreEqual(UnityTuning.N74BossKeySettleLevel, Rim74Math.KeyLevel(Rim74Math.KeySwellS), 1e-9);
            Assert.AreEqual(UnityTuning.N74BossKeySettleLevel, Rim74Math.KeyLevel(60), 1e-9, "it stays settled while the boss lives");
            Assert.GreaterOrEqual(Rim74Math.KeySwellS, 1.5);
            Assert.LessOrEqual(Rim74Math.KeySwellS, 2.5, "the lead's brief: about 2 s, then settles");
            double last = 0;
            for (int i = 1; i <= 300; i++)
            {
                double t = i / 60.0;
                double l = Rim74Math.KeyLevel(t);
                Assert.LessOrEqual(System.Math.Abs(l - last), 0.1, "no jump at 60 fps (a swell, not a flicker)");
                if (t >= UnityTuning.N74BossKeyRiseS) Assert.GreaterOrEqual(l, UnityTuning.N74BossKeySettleLevel - 1e-9, "after the swell it never drops under its settled level");
                Assert.LessOrEqual(l, 1.0 + 1e-9);
                last = l;
            }
        }

        [Test]
        public void KeyRange_IsClamped()
        {
            Assert.AreEqual(UnityTuning.N74BossKeyRangeMin, Rim74Math.KeyRange(0), 1e-9);
            Assert.AreEqual(UnityTuning.N74BossKeyRangeMax, Rim74Math.KeyRange(100), 1e-9);
            Assert.AreEqual(3.5 * UnityTuning.N74BossKeyRangePerHeight, Rim74Math.KeyRange(3.5), 1e-9);
            Assert.AreEqual(UnityTuning.N74BossKeyGlowMaxSizeM, Rim74Math.KeyGlowSize(100), 1e-9);
        }

        [Test]
        public void KeyColour_IsARedOrangeHue_NotYellowNotMagenta()
        {
            Color c = UnityTuning.N74BossKeyColor;
            Color.RGBToHSV(c, out float h, out float s, out float v);
            float deg = h * 360f;
            Assert.GreaterOrEqual(deg, 0f);
            Assert.LessOrEqual(deg, 30f, "red-orange: the enemies' colour a step warmer (yellow, the parry's tell, starts near 45)");
            Assert.Greater(s, 0.5f, "a warm colour, not a white");
            Assert.Greater(v, 0.9f);
            Assert.Greater(c.r, c.g);
            Assert.Greater(c.g, c.b);
        }

        // ---- the rows ----

        [Test]
        public void TheRowsAreNamedAsTheLeadAddsThem()
        {
            Assert.AreEqual("trail74", Trail74Math.Row);
            Assert.AreEqual("rim74", Rim74Math.Row);
            Assert.AreEqual(Trail74Math.Row, Trail74View.Row);
            Assert.AreEqual(Rim74Math.Row, Rim74View.Row);
            Trail74Probe.Reset();
            StringAssert.Contains("[trail74]", Trail74Probe.Line());
            Rim74Probe.Reset();
            StringAssert.Contains("[rim74]", Rim74Probe.Line());
        }
    }
}
