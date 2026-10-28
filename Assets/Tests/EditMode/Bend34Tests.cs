// NIGHT34 builder B (B1, design.n34.bend34): THE GRAPPLE BEND's pure rule (Core/Bend34.cs): the tiers, the meter, the whole-step clocks per mode,
// the eased share, the split's 1-in-N world steps.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Bend34Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root(), "config.json"));
        const double Dt = 1.0 / 120;

        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        static Bend34 Make(bool split)
        {
            var b = new Bend34();
            b.Reset(Bend34Rules.From(Cfg), split, Dt);
            return b;
        }

        /// <summary>Step the bend's clock n steps (BeginStep then UpdateShare, as the sim does).</summary>
        static int Run(Bend34 b, int n)
        {
            int ends = 0;
            for (int i = 0; i < n; i++)
            {
                if (b.BeginStep()) ends++;
                b.UpdateShare();
            }
            return ends;
        }

        [Test]
        public void TheNumbers_AreInTheDesignConfig_WithTheirWhy_AndTheRowStartsOff()
        {
            Assert.That(Cfg.Has("design.n34.bend34._why"), Is.True);
            Assert.That(Cfg.Get("design.n34.bend34.enabled"), Is.InstanceOf<bool>(), "the row has its switch (the lead turns it on: NIGHT34-LOG D34-5)");
            foreach (string k in new[] { "scale", "inS", "outS", "shortS", "holdS", "killExtendS", "maxS", "refillPerKill", "rankDoubleMult", "rankDoubleLetter" })
                Assert.That(Cfg.Has("design.n34.bend34." + k + "_why"), Is.True, k);
            Bend34Rules r = Bend34Rules.From(Cfg);
            Assert.That(r.scale, Is.GreaterThan(0).And.LessThan(1));
            Assert.That(r.doubleRank, Is.GreaterThanOrEqualTo(0), "the rank letter is one of style.ranks");
            Assert.That(r.maxS, Is.GreaterThanOrEqualTo(r.holdS));
        }

        [Test]
        public void TheClocks_AreWholeSteps_GlobalScaled_SplitReal()
        {
            Bend34Rules r = Bend34Rules.From(Cfg);
            Bend34 g = Make(false), s = Make(true);
            Assert.That(g.shortSteps, Is.EqualTo(MovementMath.StepsFor(r.shortS * r.scale, Dt)));
            Assert.That(g.holdSteps, Is.EqualTo(MovementMath.StepsFor(r.holdS * r.scale, Dt)));
            Assert.That(g.maxSteps, Is.EqualTo(MovementMath.StepsFor(r.maxS * r.scale, Dt)));
            Assert.That(s.shortSteps, Is.EqualTo(MovementMath.StepsFor(r.shortS, Dt)));
            Assert.That(s.holdSteps, Is.EqualTo(MovementMath.StepsFor(r.holdS, Dt)));
            Assert.That(s.worldEvery, Is.EqualTo((int)System.Math.Round(1 / r.scale)));
        }

        [Test]
        public void TheFirstLatch_IsFull_FromTheStartingMeter_ThenEveryLatchIsShort()
        {
            Bend34 b = Make(false);
            Assert.That(b.MeterFull, Is.True, "full at the run start");
            var st = new Bend34Step();
            b.Latch(ref st);
            Assert.That(st.started && b.tier == Bend34Tier.Full && b.meter == 0, Is.True);
            int ends = Run(b, b.holdSteps + b.outSteps + 2);
            Assert.That(ends, Is.EqualTo(1));
            Assert.That(b.phase, Is.EqualTo(Bend34Phase.None));
            Assert.That(b.share, Is.EqualTo(1));
            for (int i = 0; i < 5; i++)
            {
                st = new Bend34Step();
                b.Latch(ref st);
                Assert.That(st.started && b.tier == Bend34Tier.Short, Is.True, "latch " + i);
                Run(b, b.shortSteps + b.outSteps + 2);
            }
            Assert.That(b.fullBends, Is.EqualTo(1));
            Assert.That(b.shortBends, Is.EqualTo(5));
        }

        [Test]
        public void Kills_RefillTheMeter_DoubleAtTheRank_NeverTime()
        {
            Bend34Rules r = Bend34Rules.From(Cfg);
            Bend34 b = Make(true);
            var st = new Bend34Step();
            b.Latch(ref st); // spends the starting meter
            Run(b, b.length + b.outSteps + 2);
            Run(b, 120 * 60); // a minute: nothing refills by time
            Assert.That(b.meter, Is.EqualTo(0));
            b.Kill(0, ref st);
            Assert.That(b.meter, Is.EqualTo(r.refillPerKill).Within(1e-12));
            b.Kill(r.doubleRank, ref st);
            Assert.That(b.meter, Is.EqualTo(r.refillPerKill * (1 + r.rankDoubleMult)).Within(1e-12));
            for (int i = 0; i < 10; i++) b.Kill(0, ref st);
            Assert.That(b.meter, Is.EqualTo(1));
        }

        [Test]
        public void AKillInsideTheFullBend_ExtendsIt_UpToTheCap()
        {
            Bend34 b = Make(true);
            var st = new Bend34Step();
            b.Latch(ref st);
            int len = b.length;
            b.Kill(0, ref st);
            Assert.That(b.length, Is.EqualTo(len + b.extendSteps));
            Assert.That(b.meter, Is.EqualTo(0), "a kill inside a full bend extends, it does not refill");
            for (int i = 0; i < 20; i++) b.Kill(0, ref st);
            Assert.That(b.length, Is.EqualTo(b.maxSteps));
        }

        [Test]
        public void AShortBend_UpgradesToFull_WhenTheMeterFillsAndYouLatchAgain()
        {
            Bend34 b = Make(true);
            var st = new Bend34Step();
            b.Latch(ref st);
            Run(b, b.length + b.outSteps + 2);
            b.Latch(ref st); // short
            Assert.That(b.tier, Is.EqualTo(Bend34Tier.Short));
            for (int i = 0; i < 4; i++) b.Kill(0, ref st);
            Assert.That(b.MeterFull, Is.True);
            st = new Bend34Step();
            b.Latch(ref st);
            Assert.That(st.upgraded && b.tier == Bend34Tier.Full && b.meter == 0, Is.True);
        }

        [Test]
        public void TheGlobalShare_EasesIn_HoldsAtScale_EasesOut()
        {
            Bend34Rules r = Bend34Rules.From(Cfg);
            Bend34 b = Make(false);
            var st = new Bend34Step();
            b.Latch(ref st);
            b.UpdateShare();
            Assert.That(b.share, Is.EqualTo(1), "the latch's own step");
            Run(b, b.inSteps);
            Assert.That(b.share, Is.EqualTo(r.scale).Within(1e-12));
            Assert.That(b.Depth01, Is.EqualTo(1).Within(1e-12));
            Run(b, b.length - b.inSteps);
            Assert.That(b.phase, Is.EqualTo(Bend34Phase.Out));
            Assert.That(b.share, Is.EqualTo(r.scale).Within(1e-12));
            Run(b, b.outSteps);
            Assert.That(b.share, Is.EqualTo(1));
            Assert.That(b.Depth01, Is.EqualTo(0));
        }

        [Test]
        public void TheSplit_HoldsTheWorld_OnAllButOneStepInN_InsideABend()
        {
            Bend34 b = Make(true);
            Assert.That(b.HoldWorld, Is.False, "no bend: the world steps");
            var st = new Bend34Step();
            b.Latch(ref st);
            int world = 0, held = 0, n = b.worldEvery * 30;
            for (int i = 0; i < n; i++)
            {
                b.BeginStep();
                if (b.HoldWorld) held++;
                else world++;
            }
            Assert.That(world, Is.EqualTo(30));
            Assert.That(held, Is.EqualTo(n - 30));
        }
    }
}
