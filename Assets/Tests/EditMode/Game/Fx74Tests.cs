// PG3 V1 (WEAPON FEEL; Game/Fx74.cs, shot74 and charge74's pure half). Pinned:
//   1. Env: whole for the hold, then squared down to 0 at the end; 0 before the start and after the end; a zero life is 0
//   2. SizeAt: s0 at the start, s1 at the end, ease-out between (past the straight line at the middle); Subtend: 90 degrees at 1 m is 2 m across; CapDeg keeps a flash under its angle of view (a big one at the muzzle is cut, a small one left alone: GG's NIGHT43 "filled most of the screen")
//   3. Comet: the head runs out over the travel time, the tail trails it by the tail length (never past the head), then runs on to the end over the settle time; TravelS is capped
//   4. ChargeSize / ChargeGain: the start and the end sizes, monotonic; no glow at level 0, the floor just above it, the peak at 1
//   5. ReadyEdge fires on the frame a level crosses into FULL, not while it is held, not on the way down; MoteRadius stays in 0..1 and falls through each period
//   6. FlashAlive keeps a flash for its minimum frames however short the time; KickWeight is a 1-2 frame kick (1, the second share, then 0)
//   7. the pools: a sprite lives its life (and its delay) and goes; the oldest is reused when the pool is full; a comet lives travel + settle; sparks die at their life
//   8. the numbers: the flashes are 2-4 frames at 60 fps; the hold's swell is under 3 Hz; a refused flash is dimmer; the kick is two frames at most
//   9. the probes' lines name what they count and Reset zeroes them
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Fx74Tests
    {
        const float Eps = 1e-4f;

        [Test]
        public void Env_HoldsThenFallsSquared()
        {
            Assert.AreEqual(1f, Fx74Math.Env(0f, 0.1f, 0.3f), Eps);
            Assert.AreEqual(1f, Fx74Math.Env(0.03f, 0.1f, 0.3f), Eps, "whole to the end of the hold");
            Assert.AreEqual(0.25f, Fx74Math.Env(0.065f, 0.1f, 0.3f), Eps, "half way down the fall squares to a quarter: (1 - 0.5)^2");
            Assert.AreEqual(0f, Fx74Math.Env(0.1f, 0.1f, 0.3f), Eps);
            Assert.AreEqual(0f, Fx74Math.Env(0.2f, 0.1f, 0.3f), Eps);
            Assert.AreEqual(0f, Fx74Math.Env(-0.01f, 0.1f, 0.3f), Eps, "before the start");
            Assert.AreEqual(0f, Fx74Math.Env(0f, 0f, 0f), Eps, "a zero life");
            float prev = 2f;
            for (int i = 0; i <= 20; i++)
            {
                float e = Fx74Math.Env(i * 0.005f, 0.1f, 0.2f);
                Assert.LessOrEqual(e, prev + Eps, "never rises");
                prev = e;
            }
        }

        [Test]
        public void SizeAt_EaseOutBetweenTheEnds_AndSubtend()
        {
            Assert.AreEqual(0.2f, Fx74Math.SizeAt(0.2f, 1.0f, 0f), Eps);
            Assert.AreEqual(1.0f, Fx74Math.SizeAt(0.2f, 1.0f, 1f), Eps);
            Assert.Greater(Fx74Math.SizeAt(0.2f, 1.0f, 0.5f), 0.6f, "past the straight line at the middle: fast off the start");
            Assert.AreEqual(1.0f, Fx74Math.SizeAt(0.2f, 1.0f, 3f), Eps, "clamped");
            Assert.AreEqual(2f, Fx74Math.Subtend(1f, 90f), Eps);
            Assert.AreEqual(0f, Fx74Math.Subtend(5f, 0f), Eps);
        }

        [Test]
        public void Comet_HeadRunsOut_TailTrails_ThenRunsOn()
        {
            const float len = 20f, travel = 0.05f, tailM = 3f, settle = 0.05f;
            Fx74Math.Comet(len, 0f, travel, tailM, settle, out float tail, out float head);
            Assert.AreEqual(0f, head, Eps);
            Assert.AreEqual(0f, tail, Eps);
            Fx74Math.Comet(len, 0.025f, travel, tailM, settle, out tail, out head);
            Assert.AreEqual(10f, head, Eps);
            Assert.AreEqual(7f, tail, Eps);
            Fx74Math.Comet(len, travel, travel, tailM, settle, out tail, out head);
            Assert.AreEqual(20f, head, Eps);
            Assert.AreEqual(17f, tail, Eps);
            Fx74Math.Comet(len, travel + settle * 0.5f, travel, tailM, settle, out tail, out head);
            Assert.AreEqual(20f, head, Eps);
            Assert.AreEqual(18.5f, tail, Eps, "the tail runs on to the end");
            Fx74Math.Comet(len, travel + settle, travel, tailM, settle, out tail, out head);
            Assert.AreEqual(20f, tail, Eps);
            for (int i = 0; i <= 40; i++)
            {
                Fx74Math.Comet(len, i * 0.0025f, travel, tailM, settle, out tail, out head);
                Assert.LessOrEqual(tail, head + Eps, "the tail is never past the head");
                Assert.GreaterOrEqual(tail, 0f);
            }
            Fx74Math.Comet(2f, 0.025f, travel, tailM, settle, out tail, out head);
            Assert.AreEqual(0f, tail, Eps, "a segment shorter than the tail starts at 0");
            Fx74Math.Comet(0f, 0.01f, travel, tailM, settle, out tail, out head);
            Assert.AreEqual(0f, head, Eps);
        }

        [Test]
        public void CapDeg_KeepsAFlashUnderItsAngle()
        {
            Assert.AreEqual(Fx74Math.Subtend(0.64f, 20f), Fx74Math.CapDeg(2f, 0.64f, 20f), Eps, "a big flash at the muzzle is cut to 20 degrees of view");
            Assert.AreEqual(0.1f, Fx74Math.CapDeg(0.1f, 0.64f, 20f), Eps, "a small one is left alone");
            Assert.AreEqual(3f, Fx74Math.CapDeg(3f, 0f, 20f), Eps, "no distance: left alone");
            Assert.Less(Fx74Math.CapDeg(0.75f * UnityTuning.PG3V1SlugK, 0.64f, UnityTuning.PG3V1CapStarDeg), 0.45f, "the slug's release star at the muzzle stays well under half a metre");
            Assert.Less(UnityTuning.PG3V1CapSoftDeg, UnityTuning.PG3V1CapStarDeg, "a soft core may subtend less than a star");
            Assert.Less(UnityTuning.PG3V1CapStarDeg, UnityTuning.PG3V1CapRingDeg, "a star less than a ring (a thin line)");
            Assert.Less(UnityTuning.PG3V1CapRingDeg, 90f);
        }

        [Test]
        public void TravelS_IsCapped_AndLifeIsTravelPlusSettle()
        {
            Assert.AreEqual(10f / 380f, Fx74Math.TravelS(10f, 380f, 0.07f), Eps);
            Assert.AreEqual(0.07f, Fx74Math.TravelS(200f, 380f, 0.07f), Eps);
            Assert.AreEqual(0.07f, Fx74Math.TravelS(10f, 0f, 0.07f), Eps, "no speed: the cap");
            Assert.AreEqual(0.12f, Fx74Math.CometLife(0.07f, 0.05f), Eps);
        }

        [Test]
        public void ChargeGlow_GrowsWithTheLevel()
        {
            Assert.AreEqual(0.06f, Fx74Math.ChargeSize(0f, 0.06f, 0.26f), Eps);
            Assert.AreEqual(0.26f, Fx74Math.ChargeSize(1f, 0.06f, 0.26f), Eps);
            Assert.AreEqual(0.16f, Fx74Math.ChargeSize(0.5f, 0.06f, 0.26f), Eps, "smoothstep: the middle is the middle");
            float prev = -1f;
            for (int i = 0; i <= 20; i++)
            {
                float s = Fx74Math.ChargeSize(i / 20f, 0.06f, 0.26f);
                Assert.GreaterOrEqual(s, prev - Eps, "monotonic");
                prev = s;
            }
            Assert.AreEqual(0f, Fx74Math.ChargeGain(0f, 0.6f, 2.6f), Eps, "no glow without charge");
            Assert.AreEqual(0.6f, Fx74Math.ChargeGain(0.001f, 0.6f, 2.6f), 0.01f, "a touch of charge shows at the floor");
            Assert.AreEqual(2.6f, Fx74Math.ChargeGain(1f, 0.6f, 2.6f), Eps);
            Assert.Greater(Fx74Math.ChargeGain(0.9f, 0.6f, 2.6f), Fx74Math.ChargeGain(0.5f, 0.6f, 2.6f));
        }

        [Test]
        public void ReadyEdge_OnlyOnTheFrameItCrossesIntoFull()
        {
            Assert.IsTrue(Fx74Math.ReadyEdge(0.9f, 1f));
            Assert.IsTrue(Fx74Math.ReadyEdge(0f, 1f), "a jump from nothing");
            Assert.IsFalse(Fx74Math.ReadyEdge(1f, 1f), "held at full: no second flash");
            Assert.IsFalse(Fx74Math.ReadyEdge(0.4f, 0.5f));
            Assert.IsFalse(Fx74Math.ReadyEdge(1f, 0f), "released");
            Assert.IsFalse(Fx74Math.ReadyEdge(1f, 0.99f));
            Assert.IsTrue(Fx74Math.ReadyEdge(0.99f, 1f), "a second fill after a drop fires again");
        }

        [Test]
        public void MoteRadius_StaysInRange_AndFallsThroughEachPeriod()
        {
            for (int i = 0; i < 7; i++)
                for (int k = 0; k < 100; k++)
                {
                    float r = Fx74Math.MoteRadius(k * 0.013f, 2f, i, 7);
                    Assert.GreaterOrEqual(r, 0f);
                    Assert.LessOrEqual(r, 1f);
                }
            float a = Fx74Math.MoteRadius(0.1f, 2f, 0, 7), b = Fx74Math.MoteRadius(0.2f, 2f, 0, 7);
            Assert.Greater(a, b, "falling toward the muzzle within a period (0.1 s and 0.2 s at 2 Hz are inside the first)");
            Assert.AreNotEqual(Fx74Math.MoteRadius(0.3f, 2f, 0, 7), Fx74Math.MoteRadius(0.3f, 2f, 3, 7), "the motes are spread round the period");
        }

        [Test]
        public void FlashAlive_KeepsItsMinimumFrames_AndKickWeightIsTwoFramesAtMost()
        {
            Assert.IsTrue(Fx74Math.FlashAlive(0.01f, 0, 0.045f, 2));
            Assert.IsTrue(Fx74Math.FlashAlive(0.2f, 1, 0.045f, 2), "a slow frame: past the time, still its second frame");
            Assert.IsFalse(Fx74Math.FlashAlive(0.2f, 2, 0.045f, 2));
            Assert.IsFalse(Fx74Math.FlashAlive(0.05f, 6, 0.045f, 2));
            Assert.AreEqual(1f, Fx74Math.KickWeight(0, 0.45f), Eps);
            Assert.AreEqual(0.45f, Fx74Math.KickWeight(1, 0.45f), Eps);
            Assert.AreEqual(0f, Fx74Math.KickWeight(2, 0.45f), Eps);
            Assert.AreEqual(0f, Fx74Math.KickWeight(-1, 0.45f), Eps);
        }

        [Test]
        public void Numbers_AreWhatTheSlateAsks()
        {
            Assert.That(UnityTuning.PG3V1FlashS * 60f, Is.InRange(2f, 4f), "the KESTREL's flash is 2-4 frames at 60 fps");
            Assert.That(UnityTuning.PG3V1FlashThunderS * 60f, Is.InRange(2f, 4f), "the THUNDERCLAP's too");
            Assert.That(UnityTuning.PG3V1FlashMinFrames, Is.InRange(2, 4));
            Assert.Less(UnityTuning.PG3V1HoldSwellHz, 3f, "WCAG 2.3.1: a held swell is under three a second");
            Assert.Less(UnityTuning.PG3V1DimK, 1f, "a refused flash is dimmer");
            Assert.Less(UnityTuning.PG3V1KickSecond, 1f, "the second kick frame is weaker");
            Assert.Less(UnityTuning.PG3V1KickFovDeg, 4f, "a punch, not a lurch");
            Assert.Less(UnityTuning.PG3V1TracerMaxTravelS, 0.12f, "the tracer arrives within a few frames");
            Assert.Less(UnityTuning.PG3V1BeamNarrow, 1f);
            Assert.That(UnityTuning.PG3V1DecalS, Is.InRange(0.3f, 1f), "a hot decal that cools");
            Assert.That(UnityTuning.PG3V1FullAt, Is.InRange(0.9f, 1f));
            Assert.LessOrEqual(UnityTuning.PG3V1PelletSparks * 12 + UnityTuning.PG3V1BurstSparksBig * 2, UnityTuning.PG3V1SparkPool, "a volley's sparks fit the pool");
        }

        [Test]
        public void Pools_LiveTheirLife_AndReuseTheOldest()
        {
            var go = new GameObject("Fx74Tests camera");
            try
            {
                Camera cam = go.AddComponent<Camera>();
                var sprites = new Fx74Sprites(2);
                sprites.Billboard(Fx74Tex.Soft, Vector3.zero, 0.1f, 0.2f, 0.1f, 0f, Color.white, Color.white);
                sprites.StepAndDraw(0.05f, cam);
                Assert.AreEqual(1, sprites.Live);
                sprites.StepAndDraw(0.06f, cam);
                Assert.AreEqual(0, sprites.Live, "gone at the end of its life");
                sprites.Billboard(Fx74Tex.Ring, Vector3.zero, 0.1f, 0.2f, 1f, 0f, Color.white, Color.white, 0f, 0.2f);
                sprites.StepAndDraw(0.1f, cam);
                Assert.AreEqual(1, sprites.Live, "waiting out its delay it still holds its slot");
                sprites.Billboard(Fx74Tex.Ring, Vector3.up, 0.1f, 0.2f, 1f, 0f, Color.white, Color.white);
                sprites.Billboard(Fx74Tex.Ring, Vector3.down, 0.1f, 0.2f, 1f, 0f, Color.white, Color.white);
                sprites.StepAndDraw(0.01f, cam);
                Assert.AreEqual(2, sprites.Live, "two slots: the third put reused the oldest");
                sprites.Clear();
                Assert.AreEqual(0, sprites.Live);

                var comets = new Fx74Comets(4);
                comets.Add(Vector3.zero, Vector3.forward * 20f, 0.05f, 0.05f, 3f, 0.02f, 0.1f, Color.white, Color.cyan);
                comets.StepAndDraw(0.09f, cam);
                Assert.AreEqual(1, comets.Live);
                comets.StepAndDraw(0.02f, cam);
                Assert.AreEqual(0, comets.Live, "a comet lives its travel plus its settle");
                Assert.AreEqual(1, comets.Added);

                var sparks = new Fx74Sparks(4);
                sparks.Add(Vector3.zero, Vector3.up * 5f, 0.3f, 0.3f, 0.016f, Color.white);
                sparks.StepAndDraw(0.1f, 9.8f, 2.2f, cam);
                Assert.AreEqual(1, sparks.Live);
                sparks.StepAndDraw(0.25f, 9.8f, 2.2f, cam);
                Assert.AreEqual(0, sparks.Live);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Pools_WaitOnRealTime_ThenAgeOnTheWorldsTime()
        {
            var go = new GameObject("Fx74Tests camera 2");
            try
            {
                Camera cam = go.AddComponent<Camera>();
                var sprites = new Fx74Sprites(4);
                sprites.Billboard(Fx74Tex.Soft, Vector3.zero, 0.1f, 0.2f, 0.1f, 0f, Color.white, Color.white, 0f, 0.05f);
                for (int i = 0; i < 20; i++) sprites.StepAndDraw(0f, 0.01f, cam); // a hit freeze: the world's dt is 0 while real time runs
                Assert.AreEqual(1, sprites.Live, "a frozen world keeps it however long real time runs: the burst holds on the freeze");
                sprites.StepAndDraw(0.06f, 0.06f, cam);
                Assert.AreEqual(1, sprites.Live, "aging on the world's clock once it runs");
                sprites.StepAndDraw(0.06f, 0.06f, cam);
                Assert.AreEqual(0, sprites.Live, "gone at the end of its life");

                var sparks = new Fx74Sparks(4);
                sparks.Add(Vector3.zero, Vector3.up * 3f, 0.1f, 0.3f, 0.016f, Color.white, 0.05f);
                for (int i = 0; i < 4; i++) sparks.StepAndDraw(0.01f, 0.01f, 9.8f, 2.2f, cam);
                Assert.AreEqual(1, sparks.Live, "waiting out its delay, then flying");
                for (int i = 0; i < 20; i++) sparks.StepAndDraw(0.01f, 0.01f, 9.8f, 2.2f, cam);
                Assert.AreEqual(0, sparks.Live);
                Assert.AreEqual(1, sparks.Added);
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void Probes_NameWhatTheyCount_AndReset()
        {
            Shot74Probe.Reset();
            Shot74Probe.Flashes = 3;
            Shot74Probe.EnemyBursts = 2;
            StringAssert.Contains("[shot74]", Shot74Probe.Line());
            StringAssert.Contains("muzzle flashes 3", Shot74Probe.Line());
            StringAssert.Contains("enemy bursts 2", Shot74Probe.Line());
            Shot74Probe.Reset();
            StringAssert.Contains("muzzle flashes 0", Shot74Probe.Line());
            Charge74Probe.Reset();
            Charge74Probe.Readies = 4;
            Charge74Probe.Parries = 1;
            StringAssert.Contains("[charge74]", Charge74Probe.Line());
            StringAssert.Contains("READY flashes 4", Charge74Probe.Line());
            StringAssert.Contains("parries 1", Charge74Probe.Line());
            Charge74Probe.Reset();
            StringAssert.Contains("READY flashes 0", Charge74Probe.Line());
        }
    }
}
