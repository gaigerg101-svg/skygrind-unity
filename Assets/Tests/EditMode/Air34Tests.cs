// NIGHT34 builder B (leftover 1, design.n34.air34): the air multiplier's pure clock (Core/Air34.cs) and its NIGHT34 bit.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Air34Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root(), "config.json"));
        const double Dt = 1.0 / 120;

        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        [Test]
        public void TheRow_IsInTheDesignConfig_Off_WithItsNumbers()
        {
            Assert.That(Cfg.Has("design.n34.air34._why"), Is.True);
            Assert.That(Cfg.Get("design.n34.air34.enabled"), Is.InstanceOf<bool>(), "the row has its switch (the lead turns it on: NIGHT34-LOG D34-5)");
            foreach (string k in new[] { "max", "risePerSecond", "fallSeconds" }) Assert.That(Cfg.Has("design.n34.air34." + k + "_why"), Is.True, k);
        }

        [Test]
        public void ItClimbsInTheAir_ToItsCap_AndFallsBackFastOnLanding()
        {
            Air34Rules r = Air34Rules.From(Cfg);
            var a = new Air34();
            a.Reset(r);
            Assert.That(a.mult, Is.EqualTo(1));
            for (int i = 0; i < 120 * 5; i++) a.Step(true, Dt);
            Assert.That(a.mult, Is.EqualTo(r.max));
            int n = MovementMath.StepsFor(r.fallSeconds, Dt);
            for (int i = 0; i < n + 1; i++) a.Step(false, Dt);
            Assert.That(a.mult, Is.EqualTo(1));
        }

        [Test]
        public void TheBit_RoundTrips_AndIsBitFour()
        {
            var b = new N34Blocks { air34 = true };
            Assert.That(b.Bits, Is.EqualTo(16));
            Assert.That(N34Blocks.FromBits(16).air34, Is.True);
            Assert.That(N34Blocks.Names[4], Is.EqualTo("air34"));
        }
    }
}
