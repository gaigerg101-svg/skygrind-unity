// NIGHT72 builder N (slices N1, N3, N4): the pure halves of builder N's work, pinned in EditMode (the same cases as the SimHarness proofs n72goal / n72cam / n72lane):
//   goal72 (Goal72Math): a steady approach shows the line and no hint; a stuck player gets stage 2 at 10 s and stage 3 at 20 s, each counted once; a scripted player never earns a hint unless the stuck
//     run is planted; progress retires a hint and it does not come back in the room; a verb already used is not taught again; every text is at most 6 words; a new room restarts the clock
//   cam72 (Cam72Math): the strafe roll peaks at +-1 deg (x5 boosted), comes back by a smooth damp with no undershoot; the dash FOV x0.95 forward / x1.1 back, 2 frames each way at 60 fps; the slide
//     drop 0.32 m and the ceiling clamp
//   row 12's window (HopSite72): 4.0 s under the web's movement, 6.0 s under the ULTRAKILL profile's flat jump; the retry places: the longest clear lane first, never the place already tried
//   the line's size: UiTheme S24 is at least the slate's 22 px
using System;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N72NTests
    {
        // ---- goal72 ----
        [Test]
        public void Goal72_Approach_ShowsTheLine_AndNoHint()
        {
            var s = new Goal72Math.State();
            int max = 0;
            for (int i = 0; i < 40; i++) { s.Step(i, true, 0, 30 - i * 0.7, false, false, 0, 0); max = Math.Max(max, s.Stage); }
            Assert.That(max, Is.EqualTo(1));
            Assert.That(s.Lines, Is.EqualTo(1));
            Assert.That(s.Hint10 + s.Hint20, Is.EqualTo(0));
        }

        [Test]
        public void Goal72_Stuck_Stage2At10s_Stage3At20s_EachCountedOnce()
        {
            var s = new Goal72Math.State();
            double t2 = -1, t3 = -1;
            for (int i = 0; i <= 250; i++)
            {
                double t = i / 10.0;
                s.Step(t, true, 0, 30, false, false, 0, 0);
                if (s.Stage >= 2 && t2 < 0) t2 = t;
                if (s.Stage >= 3 && t3 < 0) t3 = t;
            }
            Assert.That(t2, Is.EqualTo(UnityTuning.Goal72Hint1S).Within(0.11));
            Assert.That(t3, Is.EqualTo(UnityTuning.Goal72Hint2S).Within(0.11));
            Assert.That((s.Lines, s.Hint10, s.Hint20), Is.EqualTo((1, 1, 1)));
        }

        [Test]
        public void Goal72_ScriptedPlayer_NeverGetsAHint_UnlessThePlantedRun()
        {
            var s = new Goal72Math.State();
            for (int i = 0; i <= 300; i++) s.Step(i / 10.0, true, 0, 30, true, false, 0, 0);
            Assert.That(s.Stage, Is.EqualTo(1));
            Assert.That(s.Hint10 + s.Hint20, Is.EqualTo(0));
            double t3 = -1;
            for (int i = 0; i <= 300; i++)
            {
                s.Step(30 + i / 10.0, true, 0, 30, true, true, 0, 0);
                if (s.Stage >= 3 && t3 < 0) t3 = i / 10.0;
            }
            Assert.That(t3, Is.EqualTo(UnityTuning.Goal72Hint2S).Within(0.11), "the planted run's clock starts when it is planted (the held clock follows a scripted player)");
        }

        [Test]
        public void Goal72_Progress_RetiresTheHint_AndItDoesNotComeBack()
        {
            var s = new Goal72Math.State();
            for (int i = 0; i <= 120; i++) s.Step(i / 10.0, true, 0, 30, false, false, 0, 0);
            Assert.That(s.Stage, Is.EqualTo(2));
            s.Step(12.1, true, 0, 30 - UnityTuning.Goal72ProgressM - 0.5, false, false, 0, 0);
            Assert.That(s.Stage, Is.EqualTo(1));
            Assert.That(s.Retired, Is.EqualTo(1));
            for (int i = 0; i <= 300; i++) s.Step(13 + i / 10.0, true, 0, 27, false, false, 0, 0);
            Assert.That(s.Stage, Is.EqualTo(1), "the hint is spent for the room");
        }

        [Test]
        public void Goal72_AVerbAlreadyUsed_IsNotTaughtAgain()
        {
            Goal72Math.Format(3, 5, 0, Goal72Math.Verb.None, out string a, out _);
            Goal72Math.Format(3, 5, 0, Goal72Math.Verb.DoubleJump, out string b, out _);
            Goal72Math.Format(3, 5, 0, Goal72Math.Verb.DoubleJump | Goal72Math.Verb.Ride, out string c, out _);
            Assert.That(a, Does.Contain("DOUBLE JUMP"));
            Assert.That(b, Does.Contain("RIDE"));
            Assert.That(c, Does.Contain("KEEP GOING"));
        }

        [Test]
        public void Goal72_EveryText_IsAtMostSixWords()
        {
            foreach (double dy in new[] { 0.0, 4.0, 9.0, -4.0 })
                foreach (double bearing in new[] { 0.0, 150.0 })
                    foreach (int stage in new[] { 1, 2, 3 })
                        foreach (var learned in new[] { Goal72Math.Verb.None, Goal72Math.Verb.DoubleJump, Goal72Math.Verb.DoubleJump | Goal72Math.Verb.Ride })
                        {
                            Goal72Math.Format(stage, dy, bearing, learned, out string raw, out _);
                            Assert.That(Goal72Math.Words(raw), Is.LessThanOrEqualTo(6), raw);
                        }
        }

        // NIGHT73 (NIGHT72's N3 verdict: "The exit is AHEAD" has no capitalised verb, so goal72 stayed OFF): every line OPENS with its [CAPITALS],
        // and the capitals start with a verb, so the player reads what to do first.
        [Test]
        public void Goal72_EveryText_OpensWithACapitalisedVerb()
        {
            var verbs = new[] { "GO", "DROP", "LOOK", "TURN", "DOUBLE", "RIDE", "KEEP", "WALK", "FOLLOW" };
            foreach (double dy in new[] { 0.0, 4.0, 9.0, -4.0 })
                foreach (double bearing in new[] { 0.0, 150.0 })
                    foreach (int stage in new[] { 1, 2, 3 })
                        foreach (var learned in new[] { Goal72Math.Verb.None, Goal72Math.Verb.DoubleJump, Goal72Math.Verb.DoubleJump | Goal72Math.Verb.Ride })
                        {
                            Goal72Math.Format(stage, dy, bearing, learned, out string raw, out _);
                            Assert.That(raw.StartsWith("["), Is.True, "the line opens with its capitals: " + raw);
                            string first = raw.Substring(1).Split(' ', ']')[0];
                            Assert.That(verbs, Does.Contain(first), "the capitals start with a verb: " + raw);
                        }
        }

        [Test]
        public void Goal72_ANewRoom_RestartsTheClock_AndTheHideRadiusShowsNothing()
        {
            var s = new Goal72Math.State();
            for (int i = 0; i <= 150; i++) s.Step(i / 10.0, true, 0, 30, false, false, 0, 0);
            Assert.That(s.Stage, Is.EqualTo(2));
            s.Step(15.1, true, 1, 30, false, false, 0, 0);
            Assert.That(s.Stage, Is.EqualTo(1));
            s.Step(15.2, false, 1, 3, false, false, 0, 0);
            Assert.That(s.Stage, Is.EqualTo(0));
        }

        [Test]
        public void Goal72_TheLineIsAtLeast22px_AtOneAndTheHintsAre10And20Seconds()
        {
            Assert.That(UiTheme.Px(UiTheme.Size.S24), Is.GreaterThanOrEqualTo(22f));
            Assert.That(UnityTuning.Goal72Hint1S, Is.EqualTo(10.0));
            Assert.That(UnityTuning.Goal72Hint2S, Is.EqualTo(20.0));
        }

        // ---- cam72 ----
        [Test]
        public void Cam72_StrafeRoll_PeaksAtOneDegree_AndComesBackWithoutUndershoot()
        {
            const double dt = 1.0 / 60;
            double roll = 0, vel = 0, peak = 0, min = 0;
            for (int i = 0; i < 240; i++)
            {
                double target = i * dt < 1.0 ? Cam72Math.RollTarget(1.0, false) : 0;
                roll = Cam72Math.RollStep(roll, ref vel, target, false, dt);
                peak = Math.Max(peak, roll);
                if (i * dt >= 1.0) min = Math.Min(min, roll);
            }
            Assert.That(peak, Is.EqualTo(1.0).Within(0.01));
            Assert.That(min, Is.GreaterThan(-0.01));
            Assert.That(Math.Abs(roll), Is.LessThan(0.05));
        }

        [Test]
        public void Cam72_DashingOrSliding_RollsFiveDegrees()
        {
            const double dt = 1.0 / 60;
            double roll = 0, vel = 0;
            for (int i = 0; i < 30; i++) roll = Cam72Math.RollStep(roll, ref vel, Cam72Math.RollTarget(-1.0, true), true, dt);
            Assert.That(roll, Is.EqualTo(-5.0).Within(0.01));
        }

        [Test]
        public void Cam72_DashFov_NarrowsForwardWidensBack_AtThreeHundredDegreesASecond()
        {
            Assert.That(105 * Cam72Math.FovFactor(1.0, true), Is.EqualTo(99.75).Within(1e-9));
            Assert.That(105 * Cam72Math.FovFactor(-1.0, true), Is.EqualTo(115.5).Within(1e-9));
            Assert.That(Cam72Math.FovFactor(0.0, true), Is.EqualTo(1.0));
            Assert.That(Cam72Math.FovFactor(1.0, false), Is.EqualTo(1.0));
            double off = 0;
            int n = 0;
            while (Math.Abs(off - (-5.25)) > 1e-9 && n < 100) { off = Cam72Math.FovStep(off, -5.25, 1.0 / 60); n++; }
            Assert.That(n, Is.EqualTo(2));
        }

        [Test]
        public void Cam72_SlideDrop_Is032m_AndNeverAboveACeilingOverTheSlide()
        {
            double drop = 0;
            int f = 0;
            while (drop < UnityTuning.Cam72SlideDropM - 1e-9 && f < 100) { drop = Cam72Math.DropStep(drop, UnityTuning.Cam72SlideDropM, 1.0 / 60); f++; }
            Assert.That(f, Is.LessThanOrEqualTo(4));
            Assert.That(Cam72Math.EyeY(1.6, 0.32, double.PositiveInfinity), Is.EqualTo(1.28).Within(1e-9));
            Assert.That(Cam72Math.EyeY(1.6, 0.32, 4.0), Is.EqualTo(1.28).Within(1e-9));
            Assert.That(Cam72Math.EyeY(1.6, 0.32, 1.2), Is.EqualTo(1.2 - UnityTuning.Cam72EyeClearM).Within(1e-9));
        }

        // ---- row 12's window and places ----
        [Test]
        public void HopSite72_Window_IsTheOldFourSecondsForTheWeb_AndSixUnderTheUltrakillProfile()
        {
            var web = new MovementConfig();
            Assert.That(HopSite72.Window(web), Is.EqualTo(UnityTuning.Hop72MinWindowS));
            var ultra = new MovementConfig();
            ultra.jump.height = 5.07;
            ultra.jump.apexTime = 0.702;
            ultra.jump.fallGravityMult = 1;
            Assert.That(HopSite72.AirTime(ultra), Is.EqualTo(1.404).Within(1e-9));
            Assert.That(HopSite72.Window(ultra), Is.EqualTo(0.3 + 3 * (1.404 + 0.5)).Within(1e-9));
            Assert.That(HopSite72.Window(ultra), Is.GreaterThan(UnityTuning.Hop72MinWindowS));
        }

        static TriangleWorld Floor(double halfLen)
        {
            var w = new TriangleWorld();
            w.AddBox(new Vec3(-halfLen, -1, -halfLen), new Vec3(halfLen, 0, halfLen));
            return w;
        }

        [Test]
        public void HopSite72_Candidates_LongestLaneFirst_NeverThePlaceAlreadyTried()
        {
            TriangleWorld w = Floor(60);
            var spawns = new System.Collections.Generic.List<Vec3> { new Vec3(0, 0, 0), new Vec3(40, 0, 0) };
            var open = new Vec3(0, 0.2, 0);
            double yaw0 = 0 * Math.PI / 8;
            var c = HopSite72.Candidates(w, spawns, open, yaw0);
            Assert.That(c.Count, Is.EqualTo(UnityTuning.Hop72Retries));
            for (int i = 1; i < c.Count; i++) Assert.That(c[i].lane, Is.LessThanOrEqualTo(c[i - 1].lane));
            foreach (var x in c) Assert.That(x.lane >= UnityTuning.Hop72MinLaneM, Is.True);
            foreach (var x in c) Assert.That(Math.Abs(x.at.x - open.x) < 0.01 && Math.Abs(x.yaw - yaw0) < 1e-6, Is.False, "Open35's own place is not offered again");
        }
    }
}
