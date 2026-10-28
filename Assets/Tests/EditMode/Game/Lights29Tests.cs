// NIGHT29 items 2 and 5 (builder V): the light sockets' depth caps and the door frames' geometry (Game/LightKit29.cs,
// Game/DoorFrames29.cs): no fixture reaches farther from its surface than the cap (so none is a thing to run through), and a door
// frame's jambs start exactly at the opening's edges (gap 0) and its lintel covers the whole rect.
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Lights29Tests
    {
        [Test]
        public void AFixtureNeverReachesFartherThanItsSurfaceAllows()
        {
            Assert.That(LightKit29Math.Depth("wall", 0.5f, 0.15f), Is.EqualTo(0.15f), "a wall fixture: maxWallDepth");
            Assert.That(LightKit29Math.Depth("wall", 0.05f, 0.15f), Is.EqualTo(0.05f));
            Assert.That(LightKit29Math.Depth("ceiling", 0.5f, 0.15f), Is.EqualTo(UnityTuning.CeilingFixtureMaxDepth), "a ceiling fixture: flush");
            Assert.That(LightKit29Math.Depth("pool", 0.5f, 0.15f), Is.EqualTo(0f), "a pool is flat");
            Assert.That(LightKit29Math.Depth("wall", 0f, 0.15f), Is.GreaterThan(0f));
        }

        [Test]
        public void TheFixtureStandsOnItsSurfaceNotInTheRoom()
        {
            Vector3 c = LightKit29Math.Centre(new Vector3(10, 3, -5), Vector3.back, 0.1f);
            Assert.That(c.z, Is.EqualTo(-5.05f).Within(1e-6), "the back face stays on the wall: the centre is half the depth out");
            Assert.That(c.x, Is.EqualTo(10f));
        }

        [Test]
        public void ADoorFrameStartsAtTheOpeningsEdgesAndTheLintelCoversTheRect()
        {
            // web rect [-4, 51, 4, 57]: 8 m wide in x, 6 m deep in z (Unity z = -web z)
            DoorFrameParts29 p = DoorFrames29Math.Parts(-4, 51, 4, 57, 14, 0.12f, 0.35f);
            Assert.That(p.spansX, Is.True);
            Assert.That(p.jamb0Lo.x, Is.EqualTo(-4f), "gap 0: the jamb's outer face is the opening's edge");
            Assert.That(p.jamb1Hi.x, Is.EqualTo(4f));
            Assert.That(p.jamb0Hi.x - p.jamb0Lo.x, Is.EqualTo(0.12f).Within(1e-6));
            Assert.That(p.jamb0Lo.z, Is.EqualTo(-57f));
            Assert.That(p.jamb0Hi.z, Is.EqualTo(-51f));
            Assert.That(p.lintelLo.x, Is.EqualTo(-4f));
            Assert.That(p.lintelHi.x, Is.EqualTo(4f));
            Assert.That(p.lintelHi.y, Is.EqualTo(14f));
            Assert.That(p.lintelLo.y, Is.EqualTo(14f - 0.35f).Within(1e-6));
        }

        [Test]
        public void ADoorInAnXWallRunsTheOtherWay()
        {
            DoorFrameParts29 p = DoorFrames29Math.Parts(51, -4, 57, 4, 8, 0.12f, 0.35f);
            Assert.That(p.spansX, Is.False);
            Assert.That(p.jamb0Lo.z, Is.EqualTo(-4f), "web z -4..4 = Unity z -4..4");
            Assert.That(p.jamb0Hi.z, Is.EqualTo(-4f + 0.12f).Within(1e-6));
            Assert.That(p.jamb1Hi.z, Is.EqualTo(4f));
        }
    }
}
