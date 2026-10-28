// NIGHT25 (builder F, design.n25.hitchFix): the route layer built at boot instead of at the first run start (138 ms cold in the Player's marks) changes
// nothing in the sim: the state hash is the same around the prebuild, the first run is still the fingerprint baseline's, and the layer the run takes is the
// one a run start would have built.
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class Hitch25Tests
    {
        [Test]
        public void PrebuildRoutes25_ChangesNoState_AndTheFirstRunIsTheBaseline()
        {
            SurvivalSim sim = FingerprintTests.Headless();
            ulong before = SimFingerprint.Hash(sim);
            bool built = sim.PrebuildRoutes25();
            Assert.That(SimFingerprint.Hash(sim), Is.EqualTo(before), "the sim's state hash is unchanged by the prebuild");
            Assert.That(sim.PrebuildRoutes25(), Is.False, "once");
            List<string> lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
            Assert.That(FingerprintBaseline.FirstDifference(lines), Is.EqualTo(-1), summary);
            if (built)
            {
                Assert.That(sim.routeLayer, Is.Not.Null, "the first run start took the prebuilt layer");
                RouteLayer fresh = RouteLayer.Build(sim.cfg, sim.nav.grid, sim.map.navGrid, sim.world.Geometry, sim.map.name);
                Assert.That(sim.routeLayer.links.Count, Is.EqualTo(fresh.links.Count), "the same layer a run start would build");
            }
        }
    }
}
