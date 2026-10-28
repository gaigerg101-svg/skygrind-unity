// NIGHT74 builder L (L2; Game/Reveal74Math.cs, reveal74's pure half). Pinned:
//   1. the timer: N LEFT showing, a live enemy somewhere and none on screen for the delay (3 s) wants the chevron; one frame short of it does not
//   2. any of the three conditions failing clears the wait at once (an enemy coming on screen, the counter going, nothing alive to point at), and the delay starts again from 0
//   3. Angle: ahead and to the right 0, straight above pi / 2, in front below-left in the third quadrant; behind the camera level, to the side the turn is shorter (the right when dead behind); dead ahead but hidden reads up
//   4. EdgePoint: the ray from the centre meets the edge rectangle; a point in the bottom band moves to the side edge at the band's top (the gun and the HP plate own the bottom), never to the bottom
//   5. BearingDeg: 0 ahead, + to the right, -180 .. 180, for any heading
//   6. the numbers: the fade-out is a whole frame short of the slate's 0.2 s, the delay is the slate's 3 s
//   7. (review S1) the row off skips the enemy scan unless the reel's same-run toggle is flipping this row; a skipped frame with the counter up clears the timer, a scanned one is Step
using System;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Reveal74Tests
    {
        const double Dt = 1.0 / 60;

        static int FramesUntilWanted(Reveal74Math.State s, bool counting, bool hasLive, bool onScreen, int max)
        {
            for (int k = 1; k <= max; k++) if (s.Step(Dt, counting, hasLive, onScreen, UnityTuning.N74RevealDelayS)) return k;
            return -1;
        }

        [Test]
        public void Timer_WantsTheChevronAfterTheDelay_AndNotOneFrameBefore()
        {
            var s = new Reveal74Math.State();
            int frames = FramesUntilWanted(s, true, true, false, 600);
            Assert.AreEqual((int)Math.Ceiling(UnityTuning.N74RevealDelayS / Dt - 1e-9), frames, "3 s at 60 fps");
            Assert.IsTrue(s.Want);
            Assert.IsTrue(s.Step(Dt, true, true, false, UnityTuning.N74RevealDelayS), "it stays wanted while nobody comes on screen");
        }

        [Test]
        public void Timer_AnyConditionFailingClearsTheWait()
        {
            foreach (int which in new[] { 0, 1, 2 })
            {
                var s = new Reveal74Math.State();
                Assert.Greater(FramesUntilWanted(s, true, true, false, 600), 0);
                bool counting = which != 0, hasLive = which != 1, onScreen = which == 2;
                Assert.IsFalse(s.Step(Dt, counting, hasLive, onScreen, UnityTuning.N74RevealDelayS), "case " + which);
                Assert.IsFalse(s.Want, "case " + which);
                Assert.AreEqual(0, s.NoneS, 1e-12, "case " + which);
                // the wait starts again from nothing: the same delay, not the rest of the old one
                Assert.AreEqual((int)Math.Ceiling(UnityTuning.N74RevealDelayS / Dt - 1e-9), FramesUntilWanted(s, true, true, false, 600), "case " + which);
            }
        }

        [Test]
        public void RowOff_SkipsTheScan_UnlessTheReelsToggleIsFlippingThisRow()
        {
            // NIGHT74 review S1: the scan (a viewport test and wall rays per enemy) is for the row on, or the reel's same-run toggle on this row (it holds one frame off: D1)
            Assert.IsTrue(Reveal74Math.ShouldScan(true, false), "row on: scan");
            Assert.IsTrue(Reveal74Math.ShouldScan(true, true), "row on, toggled: scan");
            Assert.IsTrue(Reveal74Math.ShouldScan(false, true), "the toggle's held off frame: scan, so the state runs through it as before");
            Assert.IsFalse(Reveal74Math.ShouldScan(false, false), "row off in play: no scan");
        }

        [Test]
        public void StepOrClear_ASkippedFrameClearsTheTimer_AScannedFrameIsStep()
        {
            var s = new Reveal74Math.State();
            int delay = (int)Math.Ceiling(UnityTuning.N74RevealDelayS / Dt - 1e-9);
            // scanned frames are exactly Step: the chevron is wanted on the delay's frame
            int k = 0;
            while (k < 600 && !s.StepOrClear(Dt, true, true, true, false, UnityTuning.N74RevealDelayS)) k++;
            Assert.AreEqual(delay - 1, k, "the same frame Step wants it on");
            Assert.IsTrue(s.Want);
            // a frame the counter is up and the scan was skipped (row off): cleared, nothing wanted, the wait starts again
            Assert.IsFalse(s.StepOrClear(Dt, true, false, false, false, UnityTuning.N74RevealDelayS));
            Assert.IsFalse(s.Want);
            Assert.AreEqual(0, s.NoneS, 1e-12);
            int again = 0;
            while (again < 600 && !s.StepOrClear(Dt, true, true, true, false, UnityTuning.N74RevealDelayS)) again++;
            Assert.AreEqual(delay - 1, again, "the full delay again");
            // not counting: the same clear as Step's, scanned or not
            Assert.IsFalse(s.StepOrClear(Dt, false, false, true, false, UnityTuning.N74RevealDelayS));
            Assert.AreEqual(0, s.NoneS, 1e-12);
            // the row off all the way through a fight: it never wants the chevron, however long
            for (int i = 0; i < 1200; i++) Assert.IsFalse(s.StepOrClear(Dt, true, false, true, false, UnityTuning.N74RevealDelayS), "frame " + i);
        }

        [Test]
        public void Angle_PointsTheWayToTurn()
        {
            Assert.AreEqual(0, Reveal74Math.Angle(3, 0, 5), 1e-9, "ahead and to the right");
            Assert.AreEqual(Math.PI / 2, Reveal74Math.Angle(0, 4, 5), 1e-9, "straight above");
            Assert.AreEqual(-3 * Math.PI / 4, Reveal74Math.Angle(-1, -1, 3), 1e-9, "in front, below left");
            Assert.AreEqual(Math.PI, Reveal74Math.Angle(-2, 1, -5), 1e-9, "behind and to the left: level, to the left");
            Assert.AreEqual(0, Reveal74Math.Angle(2, -3, -5), 1e-9, "behind and to the right: level, to the right (the height is not the turn)");
            Assert.AreEqual(0, Reveal74Math.Angle(0, 0, -5), 1e-9, "dead behind: the right");
            Assert.AreEqual(Math.PI / 2, Reveal74Math.Angle(0.01, 0.01, 5), 1e-9, "dead ahead but hidden: up");
        }

        [Test]
        public void EdgePoint_MeetsTheRectangle_AndNeverLandsOnTheBottomBand()
        {
            const double hx = 900, hy = 450, lowest = 200;
            Assert.IsFalse(Reveal74Math.EdgePoint(0, hx, hy, lowest, out double x, out double y));
            Assert.AreEqual(hx, x, 1e-6); Assert.AreEqual(0, y, 1e-6);
            Assert.IsFalse(Reveal74Math.EdgePoint(Math.PI / 2, hx, hy, lowest, out x, out y));
            Assert.AreEqual(0, x, 1e-6); Assert.AreEqual(hy, y, 1e-6);
            Assert.IsFalse(Reveal74Math.EdgePoint(Math.PI / 4, hx, hy, lowest, out x, out y), "the corner at 45 degrees of a 2:1 box is on the top edge");
            Assert.AreEqual(450, x, 1e-6); Assert.AreEqual(450, y, 1e-6);
            // down: the band's top on the right side edge
            Assert.IsTrue(Reveal74Math.EdgePoint(-Math.PI / 2, hx, hy, lowest, out x, out y));
            Assert.AreEqual(hx, x, 1e-6); Assert.AreEqual(-hy + lowest, y, 1e-6);
            // down and to the left: the left side edge at the band's top
            Assert.IsTrue(Reveal74Math.EdgePoint(-3 * Math.PI / 4, hx, hy, lowest, out x, out y));
            Assert.AreEqual(-hx, x, 1e-6); Assert.AreEqual(-hy + lowest, y, 1e-6);
            // left and a little down: above the band, so it stays on the ray
            Assert.IsFalse(Reveal74Math.EdgePoint(-170 * Math.PI / 180, hx, hy, lowest, out x, out y));
            Assert.AreEqual(-hx, x, 1e-6); Assert.Greater(y, -hy + lowest);
            // nothing the rule returns is on the bottom band
            for (int d = -180; d <= 180; d += 5)
            {
                Reveal74Math.EdgePoint(d * Math.PI / 180, hx, hy, lowest, out x, out y);
                Assert.GreaterOrEqual(y, -hy + lowest - 1e-6, "angle " + d);
                Assert.LessOrEqual(Math.Abs(x), hx + 1e-6, "angle " + d);
                Assert.LessOrEqual(Math.Abs(y), hy + 1e-6, "angle " + d);
            }
        }

        [Test]
        public void BearingDeg_ZeroAhead_PositiveToTheRight()
        {
            Assert.AreEqual(0, Reveal74Math.BearingDeg(0, 0, 0, 0, -10), 1e-9, "yaw 0 faces -z");
            Assert.AreEqual(90, Reveal74Math.BearingDeg(0, 0, 0, 10, 0), 1e-9, "+x is to the right of -z");
            Assert.AreEqual(-90, Reveal74Math.BearingDeg(0, 0, 0, -10, 0), 1e-9);
            Assert.AreEqual(180, Math.Abs(Reveal74Math.BearingDeg(0, 0, 0, 0, 10)), 1e-9, "behind");
            double west = Math.PI / 2; // a positive yaw turns left: from -z to -x
            Assert.AreEqual(0, Reveal74Math.BearingDeg(0, 0, west, -10, 0), 1e-9);
            Assert.AreEqual(90, Reveal74Math.BearingDeg(0, 0, west, 0, -10), 1e-9, "the right of west is north (-z)");
        }

        [Test]
        public void Numbers_AreTheSlates()
        {
            Assert.AreEqual(3.0, UnityTuning.N74RevealDelayS, 1e-9, "the slate's 3 s");
            Assert.Less(UnityTuning.N74RevealFadeOutS, 0.2f - (float)Dt, "gone within 0.2 s of an enemy entering the screen, a frame to spare");
            Assert.Greater(UnityTuning.N74RevealHoldS, UnityTuning.N74RevealShotAtS);
            Assert.Greater(UnityTuning.N74RevealShotAtS, UnityTuning.N74RevealDelayS + UnityTuning.N74RevealFadeInS, "the frame at 4 s has the chevron fully up");
        }
    }
}
