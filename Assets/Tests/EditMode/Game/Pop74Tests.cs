// PG3 V3 (KILL POP; Game/Pop74.cs, Pop74Math.cs, the pure half of the pop74 view row). Pinned:
//   1. BodyM: the kinds' sizes (husk 1.8, swarmer 0.75, spitter 2.0, charger 2.2, warden 2.3, drone 1.2 = 2 x its 0.6 radius, burrower 2.2, lancer 2.0); a boss, the RIVAL, an unlisted
//      kind and a null kind get none; Listed names exactly the table's kinds
//   2. Tier: the confirm's tier when one came (clamped to 2), else head or melee is 1, else 0; TierK / Unit grow with the tier; the far growth is 1 up close and capped; a bigger body is a bigger pop
//   3. the ring: a husk's plain pop expands to about 2 m, a bigger tier to more; the quad is sized so the texture's band lands on that radius
//   4. the counts: a full pop throws 10-24 shards, more by tier; a crowded frame thins the later pops (whole, half, a quarter) and never under the floor; embers by tier the same way
//   5. the flash: 4-6 frames at 60 fps (the retake), never fewer than four rendered frames on a slow frame, capped on a hitch
//   6. the shard's flight: gravity, a bounce that keeps a share of the fall speed, a rest when slow, never under its floor, no motion at rest or at dt 0, none through "no floor"
//   7. the palette: the enemy's red-orange toward white, red >= green >= blue at every mix (never cyan)
//   8. the shard texture: an irregular polygon (about a third of the quad), transparent in the corner, bright in the body and brighter at its rim
//   9. the shard pool: a shard lives its life and goes; the oldest is reused when the pool is full; a hit freeze (world dt 0) holds it
//  10. the numbers: the card's ranges (10-24 shards, 4-6 frames after the retake, ~2 m) and that the probe's line names what it counts and Reset zeroes it
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Pop74Tests
    {
        const float Eps = 1e-4f;

        [Test]
        public void BodyM_TheKindsSizes_BossesAndTheRivalNone()
        {
            Assert.AreEqual(1.8f, Pop74Math.BodyM("husk"), Eps);
            Assert.AreEqual(0.75f, Pop74Math.BodyM("swarmer"), Eps);
            Assert.AreEqual(2.0f, Pop74Math.BodyM("spitter"), Eps);
            Assert.AreEqual(2.2f, Pop74Math.BodyM("charger"), Eps);
            Assert.AreEqual(2.3f, Pop74Math.BodyM("warden"), Eps);
            Assert.AreEqual(1.2f, Pop74Math.BodyM("drone"), Eps, "a drone is 2 x its 0.6 m radius");
            Assert.AreEqual(2.2f, Pop74Math.BodyM("burrower"), Eps);
            Assert.AreEqual(2.0f, Pop74Math.BodyM("lancer"), Eps);
            foreach (string k in new[] { "boss", "wyrm", "loom", "stag", "crown", "crown37", "brawler", "bulwark", "glacier", "hulk", "sentry", "lantern", "wick", "rival", "", null })
                Assert.AreEqual(0f, Pop74Math.BodyM(k), Eps, "no pop for " + (k ?? "null") + ": a boss, the RIVAL (magenta) or an unlisted kind");
            foreach (string k in new[] { "husk", "swarmer", "spitter", "charger", "warden", "drone", "burrower", "lancer" }) Assert.IsTrue(Pop74Math.Listed(k), k);
            foreach (string k in new[] { "boss", "wyrm", "rival", "", null }) Assert.IsFalse(Pop74Math.Listed(k), k ?? "null");
        }

        [Test]
        public void Tier_FromTheConfirm_ElseFromTheKill_AndTheScaleGrows()
        {
            Assert.AreEqual(0, Pop74Math.Tier(0, true, true), "a confirm's tier wins over the kill's own flags");
            Assert.AreEqual(1, Pop74Math.Tier(1, false, false));
            Assert.AreEqual(2, Pop74Math.Tier(2, false, false));
            Assert.AreEqual(2, Pop74Math.Tier(7, false, false), "clamped");
            Assert.AreEqual(1, Pop74Math.Tier(-1, true, false), "no confirm: a headshot is tier 1");
            Assert.AreEqual(1, Pop74Math.Tier(-1, false, true), "... a melee kill too");
            Assert.AreEqual(0, Pop74Math.Tier(-1, false, false));
            Assert.AreEqual(1f, Pop74Math.TierK(0), Eps);
            Assert.Greater(Pop74Math.TierK(1), Pop74Math.TierK(0));
            Assert.Greater(Pop74Math.TierK(2), Pop74Math.TierK(1));
        }

        [Test]
        public void Unit_GrowsWithTierAndBody_FarGrowthIsCapped()
        {
            Assert.AreEqual(1f, Pop74Math.FarK(2f), Eps, "up close: no growth");
            Assert.AreEqual(1f, Pop74Math.FarK(UnityTuning.PG3V3FarRefM), Eps);
            Assert.Greater(Pop74Math.FarK(14f), 1f, "from the reference distance it grows");
            Assert.AreEqual(UnityTuning.PG3V3FarMaxK, Pop74Math.FarK(500f), Eps, "capped");
            Assert.AreEqual(1.8f, Pop74Math.Unit(1.8f, 0, 5f), Eps, "a husk's plain pop at 5 m: its own height");
            Assert.Greater(Pop74Math.Unit(1.8f, 1, 5f), Pop74Math.Unit(1.8f, 0, 5f));
            Assert.Greater(Pop74Math.Unit(1.8f, 2, 5f), Pop74Math.Unit(1.8f, 1, 5f));
            Assert.Less(Pop74Math.Unit(0.75f, 0, 5f), Pop74Math.Unit(1.8f, 0, 5f), "a swarmer pops smaller than a husk");
            Assert.Greater(Pop74Math.Unit(2.3f, 0, 5f), Pop74Math.Unit(1.8f, 0, 5f), "a warden pops bigger");
            Assert.Greater(Pop74Math.Unit(1.8f, 0, 30f), Pop74Math.Unit(1.8f, 0, 5f), "a far kill pops bigger so it reads");
        }

        [Test]
        public void Ring_AHusksPlainPopIsAboutTwoMetres_TheQuadPutsTheBandOnIt()
        {
            float r0 = Pop74Math.RingRadiusM(Pop74Math.Unit(1.8f, 0, 5f));
            Assert.AreEqual(2f, r0, 0.1f, "the card's ring that expands about 2 m");
            Assert.Greater(Pop74Math.RingRadiusM(Pop74Math.Unit(1.8f, 1, 5f)), r0);
            Assert.Greater(Pop74Math.RingRadiusM(Pop74Math.Unit(1.8f, 2, 5f)), Pop74Math.RingRadiusM(Pop74Math.Unit(1.8f, 1, 5f)));
            Assert.Less(Pop74Math.RingRadiusM(Pop74Math.Unit(0.75f, 0, 5f)), 1f, "a swarmer's ring is small");
            float quad = Pop74Math.RingQuadM(r0);
            Assert.AreEqual(r0, quad * 0.5f * UnityTuning.PG3V3RingBand, Eps, "the texture's band at 0.8 of the half width lands on the radius");
            Assert.Greater(UnityTuning.PG3V3Ring2K, 1f, "the multikill's second ring is wider than the first");
        }

        [Test]
        public void Counts_TenToTwentyFourShards_ByTier_AndACrowdedFrameThinsThem()
        {
            Assert.GreaterOrEqual(Pop74Math.ShardCount(0, 0), 10);
            Assert.LessOrEqual(Pop74Math.ShardCount(2, 0), 24);
            Assert.Greater(Pop74Math.ShardCount(1, 0), Pop74Math.ShardCount(0, 0));
            Assert.Greater(Pop74Math.ShardCount(2, 0), Pop74Math.ShardCount(1, 0));
            Assert.Greater(Pop74Math.EmberCount(1, 0), Pop74Math.EmberCount(0, 0));
            Assert.Greater(Pop74Math.EmberCount(2, 0), Pop74Math.EmberCount(1, 0));
            Assert.AreEqual(1f, Pop74Math.LodK(0), Eps);
            Assert.AreEqual(1f, Pop74Math.LodK(UnityTuning.PG3V3LodFullKills - 1), Eps, "the first few of a frame are whole");
            Assert.AreEqual(UnityTuning.PG3V3LodHalfK, Pop74Math.LodK(UnityTuning.PG3V3LodFullKills), Eps);
            Assert.AreEqual(UnityTuning.PG3V3LodFarK, Pop74Math.LodK(UnityTuning.PG3V3LodHalfKills), Eps);
            for (int tier = 0; tier <= 2; tier++)
            {
                int prevS = int.MaxValue, prevE = int.MaxValue;
                for (int i = 0; i < 12; i++)
                {
                    int s = Pop74Math.ShardCount(tier, i), e = Pop74Math.EmberCount(tier, i);
                    Assert.LessOrEqual(s, prevS, "a later pop never throws more");
                    Assert.LessOrEqual(e, prevE);
                    Assert.GreaterOrEqual(s, UnityTuning.PG3V3MinShards, "never under the floor");
                    Assert.GreaterOrEqual(e, UnityTuning.PG3V3MinEmbers);
                    prevS = s;
                    prevE = e;
                }
            }
            Assert.AreEqual(1f, Pop74Math.FlashK(0), Eps);
            Assert.AreEqual(1f, Pop74Math.FlashK(UnityTuning.PG3V3LodFullKills - 1), Eps, "the first few flashes are whole");
            Assert.Less(Pop74Math.FlashK(UnityTuning.PG3V3LodFullKills), 1f, "a crowded frame dims the later flashes (they overlap on one spot: no full-screen flash)");
            Assert.Less(Pop74Math.FlashK(UnityTuning.PG3V3LodHalfKills), Pop74Math.FlashK(UnityTuning.PG3V3LodFullKills));
            Assert.Greater(Pop74Math.FlashK(31), 0f, "dimmer, never gone: every kill still flashes");
            // a blast of eight multikills in one frame (a THUNDERCLAP into a crowd) throws no more shards than the pool holds: none of its own pops overwrites another's
            int total = 0;
            for (int i = 0; i < 8; i++) total += Pop74Math.ShardCount(2, i);
            Assert.LessOrEqual(total, UnityTuning.PG3V3ShardPool, "eight multikills in one frame fit the shard pool");
        }

        [Test]
        public void FlashLife_ThreeToFiveFramesAt60_NeverUnderThreeFrames_CappedOnAHitch()
        {
            float life60 = Pop74Math.FlashLife(UnityTuning.PG3V3FlashS, UnityTuning.PG3V3FlashMinFrames, 1f / 60f, UnityTuning.PG3V3FlashMaxS);
            Assert.GreaterOrEqual(life60 * 60f, 4f, "at least four frames at 60 fps (the retake: held 4-6)");
            Assert.LessOrEqual(life60 * 60f, 6f, "at most six");
            Assert.AreEqual(UnityTuning.PG3V3FlashS, Pop74Math.FlashLife(UnityTuning.PG3V3FlashS, 3, 1f / 144f, 0.15f), Eps, "a fast frame time keeps the base life");
            Assert.AreEqual(0.1f, Pop74Math.FlashLife(0.075f, 3, 1f / 30f, 0.15f), Eps, "a 30 fps frame: three of them");
            Assert.AreEqual(0.15f, Pop74Math.FlashLife(0.075f, 3, 0.2f, 0.15f), Eps, "a hitch is capped");
            Assert.AreEqual(0.075f, Pop74Math.FlashLife(0.075f, 3, 0f, 0.15f), Eps, "a held frame (dt 0) keeps the base");
            Assert.AreEqual(0.075f, Pop74Math.FlashLife(0.075f, 3, -1f, 0.15f), Eps, "a negative frame time is read as 0");
        }

        [Test]
        public void ShardStep_FallsBouncesAndRests_NeverUnderItsFloor()
        {
            Vector3 pos = new Vector3(0f, 2f, 0f), vel = new Vector3(1f, 0f, 0f);
            bool rest = false;
            float prevY = pos.y;
            Pop74Math.ShardStep(ref pos, ref vel, ref rest, 0f, 18f, 0.5f, 0.016f);
            Assert.Less(pos.y, prevY, "gravity pulls it down");
            Assert.Greater(pos.x, 0f, "and it moves on");
            Assert.Less(vel.y, 0f);

            // a hard landing bounces: the fall speed is kept in part and turned up; the sideways speed is cut
            pos = new Vector3(0f, 0.05f, 0f);
            vel = new Vector3(4f, -6f, 2f);
            rest = false;
            bool hit = Pop74Math.ShardStep(ref pos, ref vel, ref rest, 0f, 18f, 0.5f, 0.016f);
            Assert.IsTrue(hit);
            Assert.IsFalse(rest);
            Assert.Greater(vel.y, 0f, "up again");
            Assert.Less(vel.y, 6f * UnityTuning.PG3V3ShardBounce + 0.5f, "with a share of the fall speed");
            Assert.Less(vel.x, 4f * UnityTuning.PG3V3ShardFriction + 0.01f, "sideways speed cut");
            Assert.GreaterOrEqual(pos.y, UnityTuning.PG3V3ShardFloorLiftM - Eps, "never under the floor");

            // a slow landing rests
            pos = new Vector3(0f, UnityTuning.PG3V3ShardFloorLiftM, 0f);
            vel = new Vector3(0.5f, -0.2f, 0f);
            rest = false;
            Pop74Math.ShardStep(ref pos, ref vel, ref rest, 0f, 18f, 0.5f, 0.016f);
            Assert.IsTrue(rest);
            Assert.AreEqual(Vector3.zero, vel);
            Vector3 at = pos;
            Assert.IsFalse(Pop74Math.ShardStep(ref pos, ref vel, ref rest, 0f, 18f, 0.5f, 0.016f), "a resting shard does nothing");
            Assert.AreEqual(at, pos);

            // dt 0 (a hit freeze) holds it still
            pos = new Vector3(1f, 2f, 3f);
            vel = new Vector3(4f, 5f, 6f);
            rest = false;
            Assert.IsFalse(Pop74Math.ShardStep(ref pos, ref vel, ref rest, 0f, 18f, 0.5f, 0f));
            Assert.AreEqual(new Vector3(1f, 2f, 3f), pos);
            Assert.AreEqual(new Vector3(4f, 5f, 6f), vel);

            // no floor (negative infinity): it falls on
            pos = new Vector3(0f, 1f, 0f);
            vel = Vector3.zero;
            rest = false;
            for (int i = 0; i < 120; i++) Pop74Math.ShardStep(ref pos, ref vel, ref rest, float.NegativeInfinity, 18f, 0.5f, 0.016f);
            Assert.Less(pos.y, -5f, "nothing under it to stop it");
            Assert.IsFalse(rest);

            // from a throw, it comes to rest on the floor within a second or two and never goes under
            pos = new Vector3(0f, 1f, 0f);
            vel = new Vector3(3f, 6f, 0f);
            rest = false;
            float minY = pos.y;
            for (int i = 0; i < 240 && !rest; i++)
            {
                Pop74Math.ShardStep(ref pos, ref vel, ref rest, 0f, 18f, 0.5f, 1f / 60f);
                minY = Mathf.Min(minY, pos.y);
            }
            Assert.IsTrue(rest, "a thrown shard settles");
            Assert.GreaterOrEqual(minY, UnityTuning.PG3V3ShardFloorLiftM - Eps);
        }

        [Test]
        public void Palette_EnemyRedOrangeTowardWhite_NeverCyan()
        {
            Color enemy = new Color(1f, 58f / 255f, 28f / 255f, 1f); // colors.enemy 0xFF3A1C
            for (int i = 0; i <= 20; i++)
            {
                Color c = Pop74Math.Hot(enemy, i / 20f);
                Assert.GreaterOrEqual(c.r, c.g - Eps, "red leads at every mix");
                Assert.GreaterOrEqual(c.g, c.b - Eps, "then green, then blue: orange toward white, never cyan");
            }
            Assert.AreEqual(enemy.r, Pop74Math.Hot(enemy, 0f).r, Eps);
            Assert.AreEqual(1f, Pop74Math.Hot(enemy, 1f).b, Eps, "mix 1 is white");
            Assert.AreEqual(Pop74Math.Hot(enemy, 1f).g, Pop74Math.Hot(enemy, 5f).g, Eps, "clamped");
            Color white = Pop74Math.Hot(enemy, UnityTuning.PG3V3WhiteMix);
            Assert.Greater(white.g, 0.7f, "white-hot is mostly white");
            Assert.Less(white.g, 1f, "with the enemy's red still in it");
            Color ember = Pop74Math.Hot(enemy, UnityTuning.PG3V3EmberHotMix);
            Assert.Greater(ember.r - ember.b, 0.3f, "an ember stays orange, not gold or white");
            Assert.Greater(Pop74Math.LightFor(1), Pop74Math.LightFor(0));
            Assert.Greater(Pop74Math.LightFor(2), Pop74Math.LightFor(1));
        }

        [Test]
        public void SameSpot_MatchesTheConfirmToItsKill()
        {
            Assert.IsTrue(Pop74Math.SameSpot(1.5, -2.5, 1.5, -2.5));
            Assert.IsTrue(Pop74Math.SameSpot(1.5, -2.5, 1.50001, -2.50001));
            Assert.IsFalse(Pop74Math.SameSpot(1.5, -2.5, 1.6, -2.5));
            Assert.IsFalse(Pop74Math.SameSpot(1.5, -2.5, 1.5, -2.4));
        }

        [Test]
        public void ShardSize_AndSpeed_ScaleWithTheBody_WithinLimits()
        {
            Assert.AreEqual(UnityTuning.PG3V3ShardMinM, Pop74Math.ShardWidthM(0.2f, 0f), Eps, "a tiny unit hits the minimum");
            float husk0 = Pop74Math.ShardWidthM(1.8f, 0f), husk1 = Pop74Math.ShardWidthM(1.8f, 1f);
            Assert.Less(husk0, husk1, "the die roll widens it");
            Assert.AreEqual(1.8f * UnityTuning.PG3V3ShardSizeMaxK, husk1, Eps);
            Assert.Greater(husk1, 0.15f, "chunky: a husk's biggest shard is a hand wide or more");
            Assert.AreEqual(husk1, Pop74Math.ShardWidthM(1.8f, 7f), Eps, "the roll is clamped");
            Assert.AreEqual(1f, Pop74Math.ShardSpeedK(1.8f), Eps, "a husk: the reference speeds");
            Assert.Less(Pop74Math.ShardSpeedK(0.75f), 1f);
            Assert.GreaterOrEqual(Pop74Math.ShardSpeedK(0.01f), UnityTuning.PG3V3ShardSpeedKMin - Eps);
            Assert.LessOrEqual(Pop74Math.ShardSpeedK(50f), UnityTuning.PG3V3ShardSpeedKMax + Eps);
            Assert.Greater(Pop74Math.ShardSpeedK(2.3f), 1f, "a warden's shards fly a little faster");
        }

        [Test]
        public void ShardTexture_AnIrregularPolygon_BrightestAtTheRim()
        {
            Texture2D t = Pop74Draw.MakeShardTexture(64, true);
            try
            {
                Color32[] px = t.GetPixels32();
                int n = 64, lit = 0;
                foreach (Color32 c in px) if (c.r > 8) lit++;
                float cover = lit / (float)(n * n);
                Assert.Greater(cover, 0.25f, "a splinter, not a speck");
                Assert.Less(cover, 0.45f, "not the whole square: it has a silhouette");
                Assert.AreEqual(0, px[0].r, "the corner is empty");
                Assert.AreEqual(0, px[n * n - 1].r, "so is the opposite one");
                Assert.Greater(px[(n / 2) * n + n / 2].r, 90, "the body is lit");
                // along a row through the middle the brightest pixel is not in the middle: the rim carries the peak
                int row = n / 2, best = 0, bestX = 0;
                for (int x = 0; x < n; x++) if (px[row * n + x].r > best) { best = px[row * n + x].r; bestX = x; }
                Assert.AreNotEqual(n / 2, bestX, "the peak sits at an edge");
                Assert.Greater(best, px[row * n + n / 2].r, "brighter at the rim than in the body");
            }
            finally { Object.DestroyImmediate(t); }
        }

        [Test]
        public void ShardPool_LivesItsLife_ReusesTheOldest_HoldsOnAFreeze()
        {
            var go = new GameObject("Pop74Tests camera");
            try
            {
                Camera cam = go.AddComponent<Camera>();
                var shards = new Pop74Shards(2);
                shards.Add(Vector3.up, Vector3.up * 3f, 0.3f, 0.1f, 0.15f, 0f, 300f, float.NegativeInfinity, Color.white);
                shards.StepAndDraw(0.1f, 18f, 0.5f, cam);
                Assert.AreEqual(1, shards.Live);
                shards.StepAndDraw(0.25f, 18f, 0.5f, cam);
                Assert.AreEqual(0, shards.Live, "gone at the end of its life");
                Assert.AreEqual(1, shards.Added);

                shards.Add(Vector3.zero, Vector3.up, 1f, 0.1f, 0.15f, 0f, 0f, 0f, Color.white);
                shards.Add(Vector3.zero, Vector3.up, 1f, 0.1f, 0.15f, 0f, 0f, 0f, Color.white);
                shards.Add(Vector3.zero, Vector3.up, 1f, 0.1f, 0.15f, 0f, 0f, 0f, Color.white);
                shards.StepAndDraw(0.01f, 18f, 0.5f, cam);
                Assert.AreEqual(2, shards.Live, "two slots: the third add reused the oldest");

                shards.Clear();
                shards.Add(Vector3.zero, Vector3.up, 0.5f, 0.1f, 0.15f, 0f, 0f, 0f, Color.white);
                for (int i = 0; i < 60; i++) shards.StepAndDraw(0f, 18f, 0.5f, cam); // a hit freeze: the world's dt is 0
                Assert.AreEqual(1, shards.Live, "a frozen world keeps it however many frames run: the burst holds on the freeze");
                shards.StepAndDraw(0.6f, 18f, 0.5f, cam);
                Assert.AreEqual(0, shards.Live);
                shards.Clear();
                Assert.AreEqual(0, shards.Live);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Numbers_MatchTheCard_AndTheProbeNamesWhatItCounts()
        {
            Assert.GreaterOrEqual(UnityTuning.PG3V3Shards0, 10);
            Assert.LessOrEqual(UnityTuning.PG3V3Shards2, 24);
            Assert.GreaterOrEqual(UnityTuning.PG3V3FlashMinFrames, 4);
            Assert.LessOrEqual(UnityTuning.PG3V3FlashS * 60f, 6f);
            Assert.GreaterOrEqual(UnityTuning.PG3V3FlashS * 60f, 4f);
            Assert.LessOrEqual(UnityTuning.PG3V3ScorchS, 3f, "the scorch is short-lived");
            Assert.LessOrEqual(UnityTuning.PG3V3CapSoftDeg, 26f, "no full-screen flash: the soft flash stays under about a quarter of the view (V1's muzzle caps at 20)");
            Assert.Greater(UnityTuning.PG3V3CoreToK, 1.4f, "the retake: the core is about 1.5x the body's height");
            Assert.Greater(UnityTuning.PG3V3ShardSizeMaxK, 0.25f, "the retake: shards twice the first size");
            Assert.LessOrEqual(UnityTuning.PG3V3CapStarDeg, 34f);
            Assert.LessOrEqual(UnityTuning.PG3V3CapRingDeg, 56f);
            Assert.GreaterOrEqual(UnityTuning.PG3V3ShardPool, UnityTuning.PG3V3Shards2 * 3, "the pool holds at least three multikills' shards");
            Assert.Less(UnityTuning.PG3V3EmberLifeMax, 1.5f, "embers are brief");
            Assert.Greater(UnityTuning.PG3V3EmberGravityK, 0f, "embers fall with gravity");

            Pop74Probe.Reset();
            Pop74Probe.Pops = 3;
            Pop74Probe.Shards = 40;
            Pop74Probe.Tier2 = 1;
            StringAssert.Contains("[pop74]", Pop74Probe.Line());
            StringAssert.Contains("pops 3", Pop74Probe.Line());
            StringAssert.Contains("tier 2 1", Pop74Probe.Line());
            StringAssert.Contains("shards 40", Pop74Probe.Line());
            Pop74Probe.Reset();
            StringAssert.Contains("pops 0", Pop74Probe.Line());
            StringAssert.Contains("shards 0", Pop74Probe.Line());
        }
    }
}
