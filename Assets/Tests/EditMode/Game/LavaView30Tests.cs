// NIGHT30 builder V (item 3): the lava view's pure parts (Game/LavaView30.cs: LavaView30Math): the level JSON's lava row (builder L's format, LEVEL-FORMAT.md),
// the 0.8 Hz breath, the Lantern floor's heat through its tell.
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;
using M = Skygrind.Game.LavaView30Math;

namespace Skygrind.Game.Tests
{
    public class LavaView30Tests
    {
        [Test]
        public void ALavaRowGivesItsRectMinFirstAndItsSurface()
        {
            var row = new Dictionary<string, object> { ["rect"] = new List<object> { 12.0, -4.0, 9.5, 3.0 }, ["y"] = -0.8, ["room"] = "f2" };
            Assert.That(M.Rect(row, out double x0, out double z0, out double x1, out double z1, out double y), Is.True);
            Assert.That(new[] { x0, z0, x1, z1, y }, Is.EqualTo(new[] { 9.5, -4.0, 12.0, 3.0, -0.8 }));
            Assert.That(M.Rect(new Dictionary<string, object> { ["rect"] = new List<object> { 1.0, 2.0, 3.0 }, ["y"] = 0.0 }, out _, out _, out _, out _, out _), Is.False, "three numbers");
            Assert.That(M.Rect(new Dictionary<string, object> { ["rect"] = new List<object> { 1.0, 2.0, 3.0, 4.0 } }, out _, out _, out _, out _, out _), Is.False, "no y");
            Assert.That(M.Rect(new Dictionary<string, object> { ["rect"] = new List<object> { 1.0, 2.0, 1.0, 4.0 }, ["y"] = 0.0 }, out _, out _, out _, out _, out _), Is.False, "zero width");
            Assert.That(M.Rect("not a row", out _, out _, out _, out _, out _), Is.False);
        }

        [Test]
        public void TheSurfaceBreathesAtPointEightHertzAndHoldsWithReducedMotion()
        {
            float peak = M.Pulse(0.25 / UnityTuning.Lava30PulseHz, false), trough = M.Pulse(0.75 / UnityTuning.Lava30PulseHz, false);
            Assert.That(peak, Is.EqualTo(1f + UnityTuning.Lava30PulseDepth).Within(1e-4));
            Assert.That(trough, Is.EqualTo(1f - UnityTuning.Lava30PulseDepth).Within(1e-4));
            Assert.That(M.Pulse(1.0 / UnityTuning.Lava30PulseHz + 0.25 / UnityTuning.Lava30PulseHz, false), Is.EqualTo(peak).Within(1e-4), "one period later");
            Assert.That(M.Pulse(0.3, true), Is.EqualTo(1f));
        }

        [Test]
        public void TheLanternFloorHeatsThroughItsTellAndIsWholeWhenLive()
        {
            Assert.That(M.LanternFloorShare(Lantern29Stage.Rest, 0.5), Is.EqualTo(0f));
            Assert.That(M.LanternFloorShare(Lantern29Stage.Tell, 0), Is.EqualTo(UnityTuning.Lava30TellFrom).Within(1e-6));
            Assert.That(M.LanternFloorShare(Lantern29Stage.Tell, 1), Is.EqualTo(UnityTuning.Lava30TellTo).Within(1e-6));
            Assert.That(M.LanternFloorShare(Lantern29Stage.Tell, 0.5), Is.GreaterThan(UnityTuning.Lava30TellFrom).And.LessThan(UnityTuning.Lava30TellTo));
            Assert.That(M.LanternFloorShare(Lantern29Stage.Live, 0), Is.EqualTo(1f));
        }
    }
}
