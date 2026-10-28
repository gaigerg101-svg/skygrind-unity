// NIGHT27 builder B (boss1): THE LANTERN's shield rule as a pure function (Core/Enemies/Lantern27.cs Lantern27Shield) and the boss state machine's contract (Core/Enemies/BossFsm27.cs).
// The shield is down ONLY on the face that looks at the wall the player is riding right now: wall id + wall-ride state in, flag out. No scene, no UnityEngine.
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Lantern27ShieldTests
    {
        [Test]
        public void ShieldDown_OnlyOnTheFaceThatLooksAtTheRiddenWall()
        {
            Assert.That(Lantern27Shield.ShieldDown(3, 3, true), Is.True);
            Assert.That(Lantern27Shield.ShieldDown(3, 4, true), Is.False, "a face that looks at another wall stays shielded");
            Assert.That(Lantern27Shield.ShieldDown(5, 3, true), Is.False);
        }

        [Test]
        public void ShieldDown_NeverWhileNotRiding()
        {
            for (int w = 0; w < 8; w++) Assert.That(Lantern27Shield.ShieldDown(w, w, false), Is.False, "standing on the floor opens nothing (wall " + w + ")");
        }

        [Test]
        public void ShieldDown_NoRiddenWallIdMeansUp()
        {
            Assert.That(Lantern27Shield.ShieldDown(2, -1, true), Is.False);
        }

        [Test]
        public void SegmentAt_EightSectorsFromPlusX()
        {
            Assert.That(Lantern27Shield.SegmentAt(10, 1, 8), Is.EqualTo(0));
            Assert.That(Lantern27Shield.SegmentAt(10, 9, 8), Is.EqualTo(0), "up to 45 degrees");
            Assert.That(Lantern27Shield.SegmentAt(1, 10, 8), Is.EqualTo(1));
            Assert.That(Lantern27Shield.SegmentAt(-1, 10, 8), Is.EqualTo(2));
            Assert.That(Lantern27Shield.SegmentAt(-10, 1, 8), Is.EqualTo(3));
            Assert.That(Lantern27Shield.SegmentAt(-10, -1, 8), Is.EqualTo(4));
            Assert.That(Lantern27Shield.SegmentAt(-1, -10, 8), Is.EqualTo(5));
            Assert.That(Lantern27Shield.SegmentAt(1, -10, 8), Is.EqualTo(6));
            Assert.That(Lantern27Shield.SegmentAt(10, -1, 8), Is.EqualTo(7));
            Assert.That(Lantern27Shield.SegmentAt(0, 0, 8), Is.EqualTo(-1));
        }

        [Test]
        public void FaceWall_FourFacesQuarterTurnsApart()
        {
            // heading +x (0 rad): the front looks at sector 0 (just above +x); the right face (a quarter turn clockwise from above = -z) at sector 6 / 7
            Assert.That(Lantern27Shield.FaceWall(0, 0.3, 8), Is.EqualTo(0));
            Assert.That(Lantern27Shield.FaceWall(2, 0.3, 8), Is.EqualTo(4), "the back looks at the opposite wall");
            int right = Lantern27Shield.FaceWall(1, 0.3, 8), left = Lantern27Shield.FaceWall(3, 0.3, 8);
            Assert.That(right, Is.EqualTo(6));
            Assert.That(left, Is.EqualTo(2));
        }

        [Test]
        public void Fsm_OneTransitionPerStep_AndNeedsExitTimeHoldsAnAttack()
        {
            var m = new Fsm27("t");
            bool go = false;
            int enteredB = 0;
            m.Add("a", needsExitTime: true, minSteps: 3);
            m.Add("b", enter: s => enteredB++);
            m.Add("c");
            m.Transit("a", "b", () => go);
            m.Transit("b", "c", () => true);
            m.Enter();
            go = true;
            m.Step(); // a's logic ran once: not yet 3 steps, the exit is pending
            Assert.That(m.ActiveName, Is.EqualTo("a"));
            m.Step();
            m.Step();
            Assert.That(m.ActiveName, Is.EqualTo("a").Or.EqualTo("b"));
            for (int i = 0; i < 4; i++) m.Step();
            Assert.That(enteredB, Is.EqualTo(1));
            Assert.That(m.ActiveName, Is.EqualTo("c"), "b -> c took its own step: one transition per step");
        }

        [Test]
        public void Fsm_RollsAreMulberry32_Reproducible()
        {
            var a = new Fsm27("a") { rng = new Mulberry32(77) };
            var b = new Fsm27("b") { rng = new Mulberry32(77) };
            for (int i = 0; i < 20; i++) Assert.That(a.RollInt(5), Is.EqualTo(b.RollInt(5)));
        }
    }
}
