// NIGHT63 builder G (P2 "the boss rule line rises with the name card, at or before 1.0 s"; Logs/n63/slate.json P2; the row stays
// design.n61.bossRule61): the rule line's clock from its boss's rise (BossRule61.Clock / Arm / Tick; UiBossRule61.cs ticks one each HUD frame).
// The red (NIGHT62 P2, Logs/feel/n62-p0/summary47.txt b61k and its telemetry.jsonl, read by frame): THE KILN WYRM's rise at t 0.317 s (its
// name card's first frame), the card up to t 1.800 s, its boss bar (bossbar43) from t 2.317 s (the wyrm's 2.0 s emerge: Wyrm.cs SetPhase
// "intro", and SurvivalSim.N43.Bosses.cs fills no bar while the phase is "intro"), the rule line in at t 2.750 s: 2.43 s after the rise, alpha
// 0.00 at the reel's 1.0 s frame ("no line at 1.0 s"). Pinned:
//   1. the line comes in BossRule61DelayS after the rise and stands at alpha >= 0.5 at 1.0 s (BossRule61ShotS) when the boss bar comes up 2.0 s
//      after the rise (THE KILN WYRM), at 30, 60 and 144 frames a second; it begins once, is never cut by the bar coming up, ends by itself and
//      is visible at most BossRule61MaxS
//   2. the same with the bar up from the rise (THE WICK, THE ICEBREAKER, THE LOOM, THE CROWN fill their bar from the rise)
//   3. a bar that never comes up (an intro longer than the line) neither drops nor cuts it: the line comes in, runs its time and ends
//   4. a bar that was up and goes down (the fight over) cuts it at once
//   5. a new rise re-arms it: the new line comes in BossRule61DelayS after the new rise
//   6. the timing: in with its fade-in done by the reel's 1.0 s frame (BossRule61DelayS + BossRule61InS <= BossRule61ShotS)
// Written test-first: RED on its own commit (the placeholder is NIGHT62's rule: the line waits for the boss bar; a bar never up drops it).
// Not run by its author (no Editor in the worktree).
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class N63BossRuleTimeTests
    {
        struct Run
        {
            public double beganAt, alphaAt1, visibleFrom, visibleTo, cutAt;
            public int began, cut, ended;
        }

        /// <summary>Arms a line at t = 0 (the rise) and ticks it `seconds` at `fps`, the bar up from `barAt` to `barDownAt` (-1: never down).</summary>
        static Run Drive(double barAt, double barDownAt, double seconds, int words, double fps)
        {
            double dt = 1.0 / fps;
            var c = BossRule61.Clock.Idle;
            BossRule61.Arm(ref c);
            var r = new Run { beganAt = -1, alphaAt1 = -1, visibleFrom = -1, visibleTo = -1, cutAt = -1 };
            int n = (int)System.Math.Round(seconds * fps);
            for (int i = 1; i <= n; i++)
            {
                double t = i * dt;
                bool bar = t >= barAt && (barDownAt < 0 || t < barDownAt);
                BossRule61.Beat b = BossRule61.Tick(ref c, dt, bar, words);
                if (b == BossRule61.Beat.Began) { r.began++; if (r.beganAt < 0) r.beganAt = t; }
                else if (b == BossRule61.Beat.Cut) { r.cut++; if (r.cutAt < 0) r.cutAt = t; }
                else if (b == BossRule61.Beat.Ended) r.ended++;
                float a = c.Showing ? BossRule61.Alpha(c.age, c.hold) : 0f;
                if (a > 0f) { if (r.visibleFrom < 0) r.visibleFrom = t; r.visibleTo = t; }
                if (r.alphaAt1 < 0 && t >= UnityTuning.BossRule61ShotS - 1e-9) r.alphaAt1 = a;
            }
            return r;
        }

        static void InOnTime(Run r, string what, double dt)
        {
            Assert.AreEqual(1, r.began, what + ": begins once");
            Assert.That(r.beganAt, Is.InRange(0.0, UnityTuning.BossRule61DelayS + 2 * dt), what + ": in BossRule61DelayS after the rise (began at " + r.beganAt + " s)");
            Assert.That(r.alphaAt1, Is.GreaterThanOrEqualTo(0.5f), what + ": alpha >= 0.5 at the reel's 1.0 s frame (NIGHT62: 0.00)");
            Assert.That(r.visibleTo - r.visibleFrom, Is.LessThanOrEqualTo(UnityTuning.BossRule61MaxS + 0.05), what + ": visible at most BossRule61MaxS");
        }

        [Test]
        public void TheWyrm_BarAfterItsEmerge_LineInWithTheNameCard()
        {
            foreach (double fps in new[] { 30.0, 60.0, 144.0 })
            {
                string what = "THE KILN WYRM (bar up 2.0 s after the rise), " + fps + " fps";
                Run r = Drive(2.0, -1, 8.0, 8, fps);
                InOnTime(r, what, 1.0 / fps);
                Assert.AreEqual(0, r.cut, what + ": the bar coming up never cuts it");
                Assert.AreEqual(1, r.ended, what + ": it ends by itself within the 8 s");
            }
        }

        [Test]
        public void BarUpFromTheRise_TheOtherFour_LineInWithTheNameCard()
        {
            Run r = Drive(0.0, -1, 8.0, 7, 60.0);
            InOnTime(r, "bar up from the rise", 1.0 / 60);
            Assert.AreEqual(0, r.cut, "no cut");
            Assert.AreEqual(1, r.ended, "ends by itself");
        }

        [Test]
        public void BarNeverUp_TheLineStillComesInAndRunsItsTime()
        {
            Run r = Drive(99.0, -1, 8.0, 8, 60.0);
            InOnTime(r, "no bar in 8 s", 1.0 / 60);
            Assert.AreEqual(0, r.cut, "a bar never up does not cut it");
            Assert.AreEqual(1, r.ended, "it runs its time and ends");
        }

        [Test]
        public void BarDownAfterItWasUp_CutsTheLineAtOnce()
        {
            Run r = Drive(0.0, 2.0, 8.0, 8, 60.0);
            Assert.AreEqual(1, r.began, "it came in");
            Assert.AreEqual(1, r.cut, "the fight's bar going down cuts it");
            Assert.That(r.cutAt, Is.InRange(2.0 - 1e-9, 2.0 + 2.0 / 60), "at once");
            Assert.That(r.visibleTo, Is.LessThan(2.0 + 1e-9), "nothing after the cut");
            Run w = Drive(2.0, 3.0, 8.0, 8, 60.0);
            Assert.AreEqual(1, w.cut, "THE KILN WYRM: its bar up at 2.0 s and down at 3.0 s cuts the line");
            Assert.That(w.cutAt, Is.InRange(3.0 - 1e-9, 3.0 + 2.0 / 60), "at 3.0 s");
        }

        [Test]
        public void ANewRise_ReArms_TheNewLineInDelaySLater()
        {
            double dt = 1.0 / 60;
            var c = BossRule61.Clock.Idle;
            BossRule61.Arm(ref c);
            int i = 0;
            for (; i < 120; i++) BossRule61.Tick(ref c, dt, true, 6); // 2.0 s: showing
            Assert.IsTrue(c.Showing, "showing 2.0 s after the first rise");
            BossRule61.Arm(ref c); // the second rise (the HUD cuts the shown line first)
            Assert.IsTrue(c.Armed, "armed again");
            double began = -1;
            for (int k = 1; k <= 120 && began < 0; k++)
                if (BossRule61.Tick(ref c, dt, true, 6) == BossRule61.Beat.Began) began = k * dt;
            Assert.That(began, Is.InRange(UnityTuning.BossRule61DelayS - 1e-9, UnityTuning.BossRule61DelayS + 2 * dt), "the new line comes in BossRule61DelayS after the new rise");
        }

        [Test]
        public void Timing_InWithItsFadeDone_ByTheReelsOneSecondFrame()
        {
            Assert.That(UnityTuning.BossRule61DelayS + UnityTuning.BossRule61InS, Is.LessThanOrEqualTo(UnityTuning.BossRule61ShotS),
                "in BossRule61DelayS after the rise, at full alpha BossRule61InS later, both by the reel's 1.0 s frame");
            Assert.That(UnityTuning.BossRule61ShotS, Is.EqualTo(1.0).Within(1e-9), "the slate's 1.0 s");
        }
    }
}
