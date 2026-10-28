// NIGHT20 U3-34.9 F2 (builder V): SKYGRIND TIME's rule (Game/SkygrindTime20.cs; KF2's Zed Time, rare and earned;
// Research/2026-10-06-additions-backlog.md row 13): a blast killing 3+ with one shot, or a headshot kill at BOLD (rank 2)
// or above, starts it on the next frame; the world at 0.25 for 1.5 s of real time, back to 1 within 0.15 s; at most one
// start in any 60 s and none within 20 s of another (0.3-1.0 per combat minute); none with SLOW-MO OFF / REDUCED MOTION,
// with the row off, or while DOWN / in a menu (a moment there is dropped, not saved); the hold waits out a freeze.
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class SkygrindTime20Tests
    {
        const double Frame = 1.0 / 120;

        static SkygrindTime20.Params P() => new SkygrindTime20.Params
        {
            scale = 0.25, hold = 1.5, easeOut = 0.15, minGap = 20, window = 60, perWindow = 1, blastKills = 3, headshotRank = 2, lookIn = 0.06,
        };

        static SkygrindTime20 Make()
        {
            var st = new SkygrindTime20(P()) { enabled = true, on = true };
            return st;
        }

        static HitEvent Kill(int shot, bool headshot = false) => new HitEvent { owner = "player", killed = true, shot = shot, headshot = headshot, weapon = "shotgun" };

        static void Run(SkygrindTime20 st, double seconds, bool blocked = false, bool frozen = false)
        {
            for (double t = 0; t < seconds - 1e-9; t += Frame) st.BeginFrame(Frame, blocked, frozen);
        }

        [Test]
        public void AThreeKillBlastSlowsTheWorldForItsHoldThenLetsGo()
        {
            SkygrindTime20 st = Make();
            st.OnHit(Kill(7), 2);
            st.OnHit(Kill(7), 2);
            Assert.That(st.BeginFrame(Frame, false, false), Is.EqualTo(1), "two kills: nothing");
            st.OnHit(Kill(7), 2);
            Assert.That(st.BeginFrame(Frame, false, false), Is.EqualTo(0.25).Within(1e-12), "the third kill of one shot: 25 % on the next frame");
            Assert.That(st.StartedThisFrame, Is.True);
            Assert.That(st.Cause, Is.EqualTo("blast"));
            Run(st, 1.4);
            Assert.That(st.Scale, Is.EqualTo(0.25).Within(1e-12), "held for 1.5 s of real time");
            Assert.That(st.Look, Is.EqualTo(1).Within(1e-12), "the look whole while held");
            Run(st, 0.1 + 0.15 + Frame);
            Assert.That(st.Scale, Is.EqualTo(1), "back to real time within 0.15 s of the hold");
            Assert.That(st.Active, Is.False);
            Assert.That(st.Look, Is.EqualTo(0));
            Assert.That(st.Starts, Is.EqualTo(1));
        }

        [Test]
        public void AHeadshotKillAtBoldOrAboveStartsIt()
        {
            SkygrindTime20 st = Make();
            st.OnHit(Kill(1, true), 1);
            Assert.That(st.BeginFrame(Frame, false, false), Is.EqualTo(1), "a headshot kill at COOL: nothing");
            st.OnHit(Kill(2, false), 5);
            Assert.That(st.BeginFrame(Frame, false, false), Is.EqualTo(1), "a body kill at SKYGRIND: nothing");
            st.OnHit(new HitEvent { owner = "player", killed = false, headshot = true, shot = 3 }, 5);
            Assert.That(st.BeginFrame(Frame, false, false), Is.EqualTo(1), "a headshot that didn't kill: nothing");
            st.OnHit(new HitEvent { owner = "rival", killed = true, headshot = true, shot = 4 }, 5);
            Assert.That(st.BeginFrame(Frame, false, false), Is.EqualTo(1), "not the player's: nothing");
            st.OnHit(Kill(5, true), 2);
            Assert.That(st.BeginFrame(Frame, false, false), Is.EqualTo(0.25).Within(1e-12), "a headshot kill at BOLD");
            Assert.That(st.Cause, Is.EqualTo("headshot"));
        }

        [Test]
        public void ItStaysRareAtMostOncePerMinuteAndNeverWithinTwentySeconds()
        {
            SkygrindTime20 st = Make();
            double sinceLast = double.PositiveInfinity, minGap = double.PositiveInfinity;
            int shot = 100;
            for (int s = 0; s < 600 * 120; s++)
            {
                if (s % 600 == 0) st.OnHit(Kill(shot++, true), 3); // a qualifying moment every 5 s for 10 minutes
                st.BeginFrame(Frame, false, false);
                sinceLast += Frame;
                if (st.StartedThisFrame)
                {
                    minGap = System.Math.Min(minGap, sinceLast);
                    sinceLast = 0;
                }
            }
            Assert.That(st.Starts, Is.InRange(3, 10), "0.3-1.0 per minute over 10 minutes of moments");
            Assert.That(minGap, Is.GreaterThanOrEqualTo(20 - 1e-6), "none within 20 s of another");
            Assert.That(minGap, Is.GreaterThanOrEqualTo(60 - 1e-6), "at most one in any 60 s");
        }

        [Test]
        public void SlowMoOffReducedMotionOrTheRowOffMeanNone()
        {
            SkygrindTime20 st = Make();
            st.on = false; // SLOW-MO OFF or REDUCED MOTION
            st.OnHit(Kill(9, true), 5);
            Assert.That(st.BeginFrame(Frame, false, false), Is.EqualTo(1));
            st.on = true;
            Assert.That(st.BeginFrame(Frame, false, false), Is.EqualTo(1), "the moment isn't saved for later");
            st.enabled = false; // design.adds.skygrindTime off (or design.adds)
            st.OnHit(Kill(10, true), 5);
            Assert.That(st.BeginFrame(Frame, false, false), Is.EqualTo(1));
            Assert.That(st.Starts, Is.EqualTo(0));
        }

        [Test]
        public void NeverWhileDownOrInAMenuAndTheHoldWaitsOutAFreeze()
        {
            SkygrindTime20 st = Make();
            st.OnHit(Kill(11, true), 5);
            Assert.That(st.BeginFrame(Frame, true, false), Is.EqualTo(1), "DOWN or a menu: none");
            Assert.That(st.BeginFrame(Frame, false, false), Is.EqualTo(1), "and dropped, not saved");
            st.OnHit(Kill(12, true), 5);
            st.BeginFrame(Frame, false, true); // it starts under the kill's freeze ...
            Run(st, 1.0, frozen: true);
            st.BeginFrame(Frame, false, false);
            Assert.That(st.Scale, Is.EqualTo(0.25).Within(1e-12), "... and its 1.5 s run after the freeze");
            st.Halt();
            Assert.That(st.Scale, Is.EqualTo(1), "the way to TITLE ends it");
            Assert.That(st.Look, Is.EqualTo(0));
        }

        [Test]
        public void TheLookEasesInWhileTheTimeDropsAtOnce()
        {
            SkygrindTime20 st = Make();
            st.OnHit(Kill(20, true), 4);
            st.BeginFrame(Frame, false, false);
            Assert.That(st.Scale, Is.EqualTo(0.25).Within(1e-12), "the time on its first frame");
            Assert.That(st.Look, Is.InRange(0.01, 0.5), "the look leans in over 0.06 s (no pop)");
            Run(st, 0.06);
            Assert.That(st.Look, Is.EqualTo(1).Within(1e-9));
        }
    }
}
