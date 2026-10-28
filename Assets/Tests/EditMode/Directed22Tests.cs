// NIGHT22 leftover 1 (builder M): DIRECTED STEPS (Core/Directed22.cs), the pure rules. One focused test per rule. No UnityEngine:
// SimHarness runs the same assertions (`directed --selftest`).
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Directed22Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static readonly Vec3 Spot = new Vec3(20, 0, -30);

        [Test]
        public void Beacon_Stands_StepZeroWithATarget()
        {
            var d = new Directed22(Cfg);
            d.BeaconStood(5, Spot);
            Assert.That(d.Kind, Is.EqualTo(Directed22Kind.Beacon));
            Assert.That(d.Step, Is.EqualTo(0));
            Assert.That(d.HasTarget, Is.True);
            Assert.That(d.Round, Is.EqualTo(5));
        }

        [Test]
        public void Beacon_NearIt_StepOne_AwayAgain_StepZero()
        {
            var d = new Directed22(Cfg);
            d.BeaconStood(5, Spot);
            d.Update(new Vec3(0, 0, 0));
            Assert.That(d.Step, Is.EqualTo(0));
            d.Update(new Vec3(20.5, 0, -30.5));
            Assert.That(d.Step, Is.EqualTo(1));
            d.Update(new Vec3(0, 0, 0));
            Assert.That(d.Step, Is.EqualTo(0));
        }

        [Test]
        public void Beacon_Taken_StepTwoAndNoTarget_ThenTheClearEndsIt()
        {
            var d = new Directed22(Cfg);
            d.BeaconStood(5, Spot);
            d.BeaconTaken();
            Assert.That(d.Step, Is.EqualTo(2));
            Assert.That(d.HasTarget, Is.False);
            d.RoundBegan();
            Assert.That(d.Active, Is.True, "a lit beacon stays through its round's begin");
            d.RoundCleared();
            Assert.That(d.Kind, Is.EqualTo(Directed22Kind.None));
        }

        [Test]
        public void Beacon_NobodyTook_LeavesAtTheRoundsBegin()
        {
            var d = new Directed22(Cfg);
            d.BeaconStood(5, Spot);
            d.RoundBegan();
            Assert.That(d.Kind, Is.EqualTo(Directed22Kind.None));
        }

        [Test]
        public void Hunt_FollowsItsPhases_WithNoTarget()
        {
            var d = new Directed22(Cfg);
            d.Special("hunt", 8, "announce");
            Assert.That(d.Kind, Is.EqualTo(Directed22Kind.Hunt));
            Assert.That(d.Step, Is.EqualTo(0));
            Assert.That(d.HasTarget, Is.False, "the hunt has no place to walk to; the cue never invents one");
            d.RoundBegan();
            d.Special("hunt", 8, "start");
            Assert.That(d.Step, Is.EqualTo(1));
            d.Special("hunt", 8, "last");
            Assert.That(d.Step, Is.EqualTo(2));
            d.Special("hunt", 8, "end");
            Assert.That(d.Active, Is.False);
        }

        [Test]
        public void OtherSpecialRounds_AreNotDirected()
        {
            var d = new Directed22(Cfg);
            d.Special("elite", 6, "announce");
            d.Special("boss", 10, "announce");
            d.Special("twist:deadline", 4, "announce");
            Assert.That(d.Active, Is.False);
        }

        [Test]
        public void Bearing_UsesTheGamesYawConvention()
        {
            var o = new Vec3(0, 0, 0);
            Assert.That(Directed22.SideOf(Directed22.BearingDeg(o, 0, new Vec3(0, 0, -10))), Is.EqualTo(Directed22Side.Ahead));
            Assert.That(Directed22.SideOf(Directed22.BearingDeg(o, 0, new Vec3(10, 0, 0))), Is.EqualTo(Directed22Side.Right));
            Assert.That(Directed22.SideOf(Directed22.BearingDeg(o, 0, new Vec3(-10, 0, 0))), Is.EqualTo(Directed22Side.Left));
            Assert.That(Directed22.SideOf(Directed22.BearingDeg(o, 0, new Vec3(0, 0, 10))), Is.EqualTo(Directed22Side.Behind));
            Assert.That(Directed22.SideOf(Directed22.BearingDeg(o, System.Math.PI / 2, new Vec3(-10, 0, 0))), Is.EqualTo(Directed22Side.Ahead), "yaw pi/2 faces -x");
        }
    }
}
