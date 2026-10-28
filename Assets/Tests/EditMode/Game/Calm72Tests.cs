// NIGHT72 builder L (L4; Game/Calm72.cs): pinned: the low-health bar is under 30 % of health (45 % and 100 % are not under it; exactly 30 % is not; since NIGHT73 K2 it
// is PostFx's low-HP vignette's start and the log's flag, the three edges draw at any health), and the thin frame's ramp is at most 60 px deep at 1080p (the slate's bar),
// shallower than UiEdge31's. NIGHT73 K2's tests are below.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Calm72Tests
    {
        [Test]
        public void EdgeOn_OnlyUnderThirtyPercent()
        {
            const float below = UnityTuning.L72CalmEdgeBelow;
            Assert.AreEqual(0.30f, below, 1e-6f);
            Assert.IsFalse(Calm72Math.EdgeOn(1.0f, below), "full health");
            Assert.IsFalse(Calm72Math.EdgeOn(0.45f, below), "45 %: the old low-HP line");
            Assert.IsFalse(Calm72Math.EdgeOn(0.30f, below), "exactly 30 %");
            Assert.IsTrue(Calm72Math.EdgeOn(0.299f, below));
            Assert.IsTrue(Calm72Math.EdgeOn(0.05f, below));
        }

        [Test]
        public void ThinFrame_IsAtMostSixtyPixelsDeepAt1080p()
        {
            Assert.LessOrEqual(Calm72Math.DepthPx(UnityTuning.L72CalmEdgeClear, 1920), 60.0, "the side edges");
            Assert.LessOrEqual(Calm72Math.DepthPx(UnityTuning.L72CalmEdgeClear, 1080), 60.0, "the top and bottom");
            Assert.Less(Calm72Math.DepthPx(UnityTuning.L72CalmEdgeClear, 1920), Calm72Math.DepthPx(UnityTuning.Edge31HurtClear, 1920), "thinner than UiEdge31's frame");
            Assert.AreEqual(57.6, Calm72Math.DepthPx(0.94, 1920), 1e-9);
            Assert.AreEqual(32.4, Calm72Math.DepthPx(0.94, 1080), 1e-9, "top and bottom at 1080p: the lead's 32 px");
        }

        // NIGHT73 K2 (calm72 reworked: the three edges draw at any health, thin; EXPOSED's pulse calmer). Pinned:
        //   - Ui's EXPOSED alpha is 0.35 + 0.35 sin(7 t) (0 .. 0.70); calm72 keeps the mean and at most half the range (the lead's bar)
        //   - a scale of 1 is Ui's alpha; the swell (f) carries the base term, so nothing pops at the start
        //   - the swell is Ui's FlashAttack18 (InCubic over Hud18FlashAttack), 1 with FeelAB off
        //   - a hit is a rise of the hurt alpha by at least Calm73HitJump in one frame; the breath's hundredths are not

        [Test]
        public void ExposedAlpha_KeepsTheMean_AndAtMostHalfTheRange()
        {
            const float b = 0.35f;
            float s = UnityTuning.Calm73ExposedSwingScale;
            Assert.LessOrEqual(s, 0.5f, "at most half of today's amplitude");
            float peak = Calm72Math.ExposedAlpha(0.70f, 1f, b, s), trough = Calm72Math.ExposedAlpha(0f, 1f, b, s), mean = Calm72Math.ExposedAlpha(0.35f, 1f, b, s);
            Assert.AreEqual(0.35f, mean, 1e-6f, "the mean is Ui's");
            Assert.LessOrEqual(peak - trough, 0.35f + 1e-6f, "peak-to-trough at most half of Ui's 0.70");
            Assert.Greater(trough, 0f, "the trough rises: the edge no longer goes to nothing and back");
            Assert.Less(peak, 0.70f, "the peak falls");
            Assert.AreEqual(0.70f, Calm72Math.ExposedAlpha(0.70f, 1f, b, 1f), 1e-6f, "scale 1 is Ui's");
            Assert.AreEqual(0f, Calm72Math.ExposedAlpha(0f, 1f, b, 1f), 1e-6f);
            // every Ui alpha written in hundredths over the pulse stays inside Ui's own range and inside the calmer one
            for (int i = 0; i <= 70; i++)
            {
                float w = i / 100f, o = Calm72Math.ExposedAlpha(w, 1f, b, s);
                Assert.That(o, Is.InRange(trough - 1e-6f, peak + 1e-6f), "wrote " + w);
            }
        }

        [Test]
        public void ExposedAlpha_StartsFromNothing()
        {
            Assert.AreEqual(0f, Calm72Math.ExposedAlpha(0f, 0f, 0.35f, UnityTuning.Calm73ExposedSwingScale), 1e-6f, "at the start (swell 0, Ui wrote 0) nothing is drawn: no pop-in");
            float half = Calm72Math.ExposedAlpha(0.1f, 0.5f, 0.35f, UnityTuning.Calm73ExposedSwingScale);
            Assert.Less(half, Calm72Math.ExposedAlpha(0.1f, 1f, 0.35f, UnityTuning.Calm73ExposedSwingScale), "half-swollen is dimmer than whole");
        }

        [Test]
        public void ExposedAttack_IsUisSwell()
        {
            const double attack = 0.04;
            Assert.AreEqual(1f, Calm72Math.ExposedAttack(0.0, false, attack), 1e-6f, "FeelAB off: whole at once");
            Assert.AreEqual(0f, Calm72Math.ExposedAttack(0.0, true, attack), 1e-6f);
            Assert.AreEqual(0.125f, Calm72Math.ExposedAttack(0.02, true, attack), 1e-6f, "InCubic: (0.02 / 0.04) ^ 3");
            Assert.AreEqual(1f, Calm72Math.ExposedAttack(0.04, true, attack), 1e-6f);
            Assert.AreEqual(1f, Calm72Math.ExposedAttack(5.0, true, attack), 1e-6f, "clamped");
        }

        // NIGHT73 K3 (the night's review S2 / S3): the HUD is rebuilt with every scene load. Pinned:
        //   - a lost HUD is an Image found once that is gone now (any of the three); an Image never found is not a loss
        //   - the search allowance (Max, Every apart) is spent by a HUD that lacks an Image, and Reset gives the next HUD the whole of it again: after any number of scene loads
        //     a new HUD is still searched for (the first build's count never came back: after about 40 loads calm72 silently stopped)

        [Test]
        public void HudLost_IsAnImageFoundOnceThatIsGone()
        {
            Assert.IsFalse(Calm72Math.HudLost(false, false, false, false, false, false), "nothing found yet");
            Assert.IsFalse(Calm72Math.HudLost(true, true, true, true, true, true), "all three alive");
            Assert.IsFalse(Calm72Math.HudLost(true, true, false, false, true, true), "an Image this HUD never had is not a loss");
            Assert.IsTrue(Calm72Math.HudLost(true, false, true, false, true, false), "a scene load destroyed all three");
            Assert.IsTrue(Calm72Math.HudLost(false, false, false, false, true, false), "the EXPOSED edge alone was found, and is gone");
            Assert.IsTrue(Calm72Math.HudLost(true, true, true, false, true, true), "any one gone");
        }

        [Test]
        public void Search_SpendsItsAllowance_ResetGivesItBack()
        {
            var s = new Calm72Search(UnityTuning.Calm73FindMax, UnityTuning.Calm73FindEveryS);
            Assert.AreEqual(40, s.Max);
            Assert.IsFalse(s.Due(false, 0), "nothing missing: no search");
            double now = 0;
            for (int hud = 0; hud < 100; hud++) // 100 scene loads in one session (the review's S2 needed about 40)
            {
                s.Reset();
                Assert.IsTrue(s.Due(true, now), "HUD " + hud + ": a fresh HUD is searched for");
                int tries = 0;
                while (s.Due(true, now))
                {
                    s.Searched(now);
                    tries++;
                    Assert.IsFalse(s.Due(true, now), "the pause: not due again at the same moment");
                    Assert.IsFalse(s.Due(true, now + s.Every * 0.5), "nor half a pause later");
                    now += s.Every;
                }
                Assert.AreEqual(s.Max, tries, "a HUD that lacks an Image is searched for Max times, then left");
                Assert.IsFalse(s.Due(true, now + 100), "the allowance is spent");
            }
        }

        [Test]
        public void HitStart_IsAJump_NotABreath()
        {
            float jump = UnityTuning.Calm73HitJump;
            Assert.IsTrue(Calm72Math.HitStart(0f, 0.04f, jump), "a hit's first frame (7 % of 0.6)");
            Assert.IsTrue(Calm72Math.HitStart(0.2f, 0.5f, jump), "a hit during a decay");
            Assert.IsFalse(Calm72Math.HitStart(0.30f, 0.29f, jump), "the decay");
            Assert.IsFalse(Calm72Math.HitStart(0.18f, 0.20f, jump), "the low-HP breath (hundredths a frame)");
            Assert.IsFalse(Calm72Math.HitStart(0.5f, 0.5f, jump), "no change");
        }
    }
}
