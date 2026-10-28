// NIGHT20 F (builder V): the kill's floor ring under the new kinds (Chunks.cs KillRing20): a grounded kill keeps its ring at
// the feet (NIGHT19's), a DRONE shot down 5 m up lays it on the floor below, a kill over a drop lays none.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Kill20Tests
    {
        [Test]
        public void AGroundedKillKeepsItsRingAtTheFeet()
        {
            Assert.That(KillRing20.Place(2f, UnityTuning.KillRingProbeLift, out float y), Is.True);
            Assert.That(y, Is.EqualTo(2f), "the floor at the feet: unchanged");
            Assert.That(KillRing20.Place(2f, UnityTuning.KillRingProbeLift + 0.2, out y), Is.True);
            Assert.That(y, Is.EqualTo(2f), "a hair off (a step's lip): unchanged");
        }

        [Test]
        public void AFlyingKillLaysItsRingOnTheFloorBelow()
        {
            Assert.That(KillRing20.Place(6f, UnityTuning.KillRingProbeLift + 5, out float y), Is.True);
            Assert.That(y, Is.EqualTo(1f).Within(1e-5), "a DRONE 5 m up: the ring on the floor");
        }

        [Test]
        public void AKillOverADropLaysNoRing()
        {
            Assert.That(KillRing20.Place(6f, -1, out _), Is.False, "nothing below");
            Assert.That(KillRing20.Place(6f, UnityTuning.KillRingProbeLift + UnityTuning.KillRingReach + 1, out _), Is.False, "the floor too far down");
        }
    }
}
