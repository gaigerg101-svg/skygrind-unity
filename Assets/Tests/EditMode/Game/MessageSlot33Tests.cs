// NIGHT33 builder U (design.n33.hud33): the message slot's rule (MessageSlot33): one line, each held >= 1.2 s, by priority.
using System.Collections.Generic;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class MessageSlot33Tests
    {
        /// <summary>Steps the slot at 60 Hz from t0 to t1 and returns each change as "t:text".</summary>
        static List<string> Run(MessageSlot33 s, double t0, double t1, System.Action<double> each = null)
        {
            var log = new List<string>();
            string last = "";
            for (double t = t0; t < t1; t += 1.0 / 60)
            {
                each?.Invoke(t);
                string now = s.Step(t) ?? "";
                if (now != last) log.Add($"{t:0.00}:{now}");
                last = now;
            }
            return log;
        }

        [Test]
        public void TheRoomClear_TwoBannersAndTheDoorHint_ShowOneAtATime_EachHeld()
        {
            // A3's case: the clear posts "DOOR OPEN" and "1.3 DASH OPEN" in one frame while the door hint is wanted
            var s = new MessageSlot33();
            s.Post(MessageSlot33.Title, "DOOR OPEN", 0, 2.4);
            s.Post(MessageSlot33.Title, "1.3 DASH OPEN", 0, 2.4);
            var shown = new List<(double at, string text)>();
            string last = null;
            for (double t = 0; t < 8; t += 1.0 / 60)
            {
                s.Want(MessageSlot33.Door, "7 m AHEAD");
                string now = s.Step(t);
                if (now != last) shown.Add((t, now));
                last = now;
            }
            // in turn, never together: the first banner its 2.4 s, the second (it waited 2.4 s, under the 3 s drop), then the door hint
            Assert.That(shown[0].text, Is.EqualTo("DOOR OPEN"));
            Assert.That(shown[1].text, Is.EqualTo("1.3 DASH OPEN"));
            Assert.That(shown[2].text, Is.EqualTo("7 m AHEAD"));
            Assert.That(s.Dropped, Is.EqualTo(0));
            for (int i = 1; i < shown.Count; i++) Assert.That(shown[i].at - shown[i - 1].at, Is.GreaterThanOrEqualTo(1.2 - 1e-9), $"{shown[i - 1].text} held under 1.2 s");
        }

        [Test]
        public void AHigherLine_WaitsOutTheHold_ThenCutsIn_AndTheCutPostComesBack()
        {
            var s = new MessageSlot33();
            s.Post(MessageSlot33.Title, "ROUND 4", 0, 3.0);
            Assert.That(s.Step(0), Is.EqualTo("ROUND 4"));
            bool danger = true;
            List<string> log = Run(s, 0.5, 6, t =>
            {
                s.Want(MessageSlot33.Danger, danger && t < 2.5 ? "EXPOSED" : null);
            });
            // EXPOSED from 0.5 s, but ROUND 4 holds to 1.2 s; EXPOSED holds to 2.5 s (>= 1.2 s after it showed); ROUND 4 returns with its 1.8 s left
            Assert.That(log[0], Is.EqualTo("0.50:ROUND 4"), string.Join(" | ", log)); // the run's first frame (still ROUND 4)
            Assert.That(log[1], Does.StartWith("1.2").And.EndWith(":EXPOSED"), string.Join(" | ", log));
            Assert.That(log[2], Does.EndWith(":ROUND 4"));
            Assert.That(log[3], Does.EndWith(":"));
        }

        [Test]
        public void ALowerLine_NeverCutsIn_AndAWantedLineStaysWhileWanted()
        {
            var s = new MessageSlot33();
            List<string> log = Run(s, 0, 5, t =>
            {
                s.Want(MessageSlot33.Boss, t < 4 ? "RIDE THE WALL. SHOOT THE CORE." : null);
                s.Want(MessageSlot33.Lesson, "{dash} TO DODGE");
            });
            Assert.That(log.Count, Is.EqualTo(2), string.Join(" | ", log));
            Assert.That(log[0], Is.EqualTo("0.00:RIDE THE WALL. SHOOT THE CORE."));
            Assert.That(log[1], Does.StartWith("4.0").And.EndWith(":{dash} TO DODGE"));
        }

        [Test]
        public void APostThatWaitsOver3s_IsDropped()
        {
            var s = new MessageSlot33();
            s.Want(MessageSlot33.Danger, "EXPOSED");
            s.Post(MessageSlot33.Title, "ROUND 4", 0, 2);
            List<string> log = Run(s, 0, 5, t => s.Want(MessageSlot33.Danger, t < 3.5 ? "EXPOSED" : null));
            Assert.That(log, Is.EqualTo(new List<string> { "0.00:EXPOSED", log[1] }));
            Assert.That(log[1], Does.EndWith(":"));
            Assert.That(s.Dropped, Is.EqualTo(1));
        }

        [Test]
        public void TheSameLinePostedAgain_ExtendsIt_NoSecondCopy()
        {
            var s = new MessageSlot33();
            s.Post(MessageSlot33.Title, "THE STAG DOWN", 0, 2);
            s.Step(0);
            s.Post(MessageSlot33.Title, "THE STAG DOWN", 1.5, 2);
            Assert.That(s.Step(3.0), Is.EqualTo("THE STAG DOWN"));
            Assert.That(s.Step(3.6), Is.Null);
            Assert.That(s.Shows, Is.EqualTo(1));
        }

        [Test]
        public void ACountdown_TicksInPlace_AndABannerOfItsChannelGoesFirst()
        {
            var s = new MessageSlot33();
            s.Post(MessageSlot33.Title, "ROUND 1 CLEAR", 0, 1.5);
            var seen = new List<string>();
            string last = null;
            for (double t = 0; t < 6; t += 1.0 / 60)
            {
                int left = 8 - (int)t;
                s.WantLive(MessageSlot33.Title, t < 5 ? "ROUND 2 IN " + left : null);
                string now = s.Step(t);
                if (now != last) seen.Add(now ?? "-");
                last = now;
            }
            // the banner first, its 1.5 s; then the countdown ticking every second (in place: a new value is not a new 1.2 s message); then nothing
            Assert.That(seen[0], Is.EqualTo("ROUND 1 CLEAR"));
            Assert.That(seen, Does.Contain("ROUND 2 IN 7").And.Contain("ROUND 2 IN 6").And.Contain("ROUND 2 IN 5").And.Contain("ROUND 2 IN 4"));
            Assert.That(seen[seen.Count - 1], Is.EqualTo("-"));
            Assert.That(s.Shows, Is.EqualTo(2));
        }

        [Test]
        public void AWithdrawnLine_StillHoldsItsMinimum()
        {
            var s = new MessageSlot33();
            s.Want(MessageSlot33.Lesson, "SLIDE UNDER THE GAP");
            Assert.That(s.Step(0), Is.EqualTo("SLIDE UNDER THE GAP"));
            s.Want(MessageSlot33.Lesson, null);
            Assert.That(s.Step(0.5), Is.EqualTo("SLIDE UNDER THE GAP"));
            Assert.That(s.Step(1.25), Is.Null);
        }
    }
}
