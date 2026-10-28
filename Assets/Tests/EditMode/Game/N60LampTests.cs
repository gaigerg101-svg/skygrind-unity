// NIGHT60 builder V (B8 attempt 2, the exit lamp; design.n60.lamp60): where the lamp's frame stands (Exit58.FaceAnchor). Pinned:
//   1. n57-kiln's d5 (rect x -2..2, z -197.5..-193.5: a 4 m deep passage, room B kb3 at lower z): the frame stands in front of room A's (kb2c's)
//      face z -193.5, within the frame's half thickness + Exit60FaceGap of it, never inside the passage (at the rect's centre z -195.5 the posts
//      and the lintel were inside the walls: the lamp's own gain 1.00-1.06x on n60-b6)
//   2. the same rule along x (a door spanning z, room B at higher x), and a thin door's anchor stays within a frame's thickness of its centre
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class N60LampTests
    {
        static double Back => UnityTuning.Exit58FrameW / 2 + UnityTuning.Exit60FaceGap; // the frame's centre off the face

        [Test]
        public void FaceAnchor_DeepDoor_InFrontOfRoomAsFace()
        {
            var (x, z) = Exit58.FaceAnchor(new[] { -2.0, -197.5, 2.0, -193.5 }, 0, -1, true);
            Assert.That(x, Is.EqualTo(0).Within(1e-9));
            Assert.That(z, Is.EqualTo(-193.5 + Back).Within(1e-9), "kb2c's face (z -193.5) + half the frame + the gap");
        }

        [Test]
        public void FaceAnchor_AlongX_AndAThinDoor()
        {
            var (x, z) = Exit58.FaceAnchor(new[] { 10.0, -4.0, 14.0, 0.0 }, 1, 0, false);
            Assert.That(x, Is.EqualTo(10.0 - Back).Within(1e-9), "room A's face x 10 (room B at higher x)");
            Assert.That(z, Is.EqualTo(-2.0).Within(1e-9));
            var (tx, tz) = Exit58.FaceAnchor(new[] { -2.0, -0.1, 2.0, 0.1 }, 0, 1, true);
            Assert.That(tz, Is.EqualTo(-0.1 - Back).Within(1e-9), "a thin door: its face is near its centre");
            Assert.That(tx, Is.EqualTo(0).Within(1e-9));
        }
    }
}
