// NIGHT18 (the shared commit's fixture; the lead fills it): FeelProbe's pure parts. Every report is a no-op outside a reel,
// and FeelAB's command line reads `-skygrindFeelAB off` as NIGHT17's side.
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class FeelProbeTests
    {
        [Test]
        public void ReportsAreNoOpsOutsideAReel()
        {
            Assert.IsFalse(FeelProbe.Recording);
            FeelProbe.Effect(FeelFx.Burst, new Rect(0, 0, 10, 10));
            FeelProbe.ChunkPoint(Vector3.zero);
            FeelProbe.Event("kill");
            Assert.IsFalse(FeelProbe.Recording);
        }

        [Test]
        public void TheFeelABFlagReadsOffAndOn()
        {
            Assert.AreEqual(false, FeelAB.FromArgs(new[] { "x.exe", "-skygrindFeelAB", "off" }));
            Assert.AreEqual(true, FeelAB.FromArgs(new[] { "x.exe", "-skygrindFeelAB", "on" }));
            Assert.IsNull(FeelAB.FromArgs(new[] { "x.exe" }));
        }

        [Test]
        public void PickTakesTonightsValueOnlyWhenOn()
        {
            Assert.AreEqual(2f, UnityTuning.Pick(true, 2f, 1f));
            Assert.AreEqual(1f, UnityTuning.Pick(false, 2f, 1f));
        }
    }
}
