// NIGHT72 builder L (the wash71 leftover; Game/Wash71.cs PopKOverride). Pinned: -skygrindWash71PopK=<k> (or the next argument) replaces the config's popK for
// the run when it is a number in 0..1; no switch, a bad number or one out of range keeps the config's.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Wash71PopKTests
    {
        [Test]
        public void Override_ReplacesOnlyAValidNumber()
        {
            Assert.AreEqual(0.6, Wash71.PopKOverride(new[] { "-skygrindWash71PopK=0.6" }, 1.0), 1e-12);
            Assert.AreEqual(0.8, Wash71.PopKOverride(new[] { "-skygrindwash71popk", "0.8" }, 1.0), 1e-12, "the next-argument form, any case");
            Assert.AreEqual(0.0, Wash71.PopKOverride(new[] { "-skygrindWash71PopK=0" }, 1.0), 1e-12);
            Assert.AreEqual(1.0, Wash71.PopKOverride(new string[0], 1.0), 1e-12, "no switch: the config's");
            Assert.AreEqual(1.0, Wash71.PopKOverride(null, 1.0), 1e-12);
            Assert.AreEqual(1.0, Wash71.PopKOverride(new[] { "-skygrindWash71PopK=1.5" }, 1.0), 1e-12, "over 1: ignored");
            Assert.AreEqual(1.0, Wash71.PopKOverride(new[] { "-skygrindWash71PopK=abc" }, 1.0), 1e-12, "not a number: ignored");
        }
    }
}
