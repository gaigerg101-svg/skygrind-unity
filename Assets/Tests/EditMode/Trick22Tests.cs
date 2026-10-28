// NIGHT22 leftover 4 (builder M): TRICK CHAIN (Core/Trick22.cs), the pure rules. One focused test per rule. No UnityEngine: SimHarness
// runs the same assertions (`arcline2 --trick --selftest`).
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Trick22Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Port.Root, "config.json"));
        const double Dt = 1.0 / 120;

        static Trick22Chain Make() => new Trick22Chain(Cfg, Dt);

        [Test]
        public void TheFirstTrick_PaysNothingExtra()
        {
            Assert.That(Make().Trick("slideJump"), Is.EqualTo(0.0));
        }

        [Test]
        public void ADistinctSecondTrick_PaysTheBase_AndTheThirdMore()
        {
            Trick22Chain c = Make();
            double b = Cfg.Num("design.n22.trickChain.basePoints"), rise = Cfg.Num("design.n22.trickChain.rise");
            c.Trick("slideJump");
            Assert.That(c.Trick("slam"), Is.EqualTo(b));
            Assert.That(c.Trick("dashJump"), Is.EqualTo(b * (1 + rise)));
            Assert.That(c.Links, Is.EqualTo(3));
        }

        [Test]
        public void ARepeat_PaysNothing_AndDoesNotExtendTheWindow()
        {
            Trick22Chain c = Make();
            c.Trick("slideJump");
            c.Trick("slam");
            Assert.That(c.Trick("slam"), Is.EqualTo(0.0));
            Assert.That(c.Links, Is.EqualTo(2));
        }

        [Test]
        public void OutsideTheWindow_TheChainEnds_AndTheNextTrickStartsANewOne()
        {
            Trick22Chain c = Make();
            int window = MovementMath.StepsFor(Cfg.Num("design.n22.trickChain.window"), Dt);
            c.Trick("slideJump");
            for (int i = 0; i < window; i++) c.Step();
            Assert.That(c.Links, Is.EqualTo(0));
            Assert.That(c.Trick("slam"), Is.EqualTo(0.0), "a new chain's first trick pays nothing extra");
        }

        [Test]
        public void JustInsideTheWindow_TheChainHolds()
        {
            Trick22Chain c = Make();
            int window = MovementMath.StepsFor(Cfg.Num("design.n22.trickChain.window"), Dt);
            c.Trick("slideJump");
            for (int i = 0; i < window - 1; i++) c.Step();
            Assert.That(c.Trick("slam") > 0, Is.True);
        }

        [Test]
        public void TheMultiplierStopsRisingAtMaxLinks()
        {
            Trick22Chain c = Make();
            int max = Cfg.Int("design.n22.trickChain.maxLinks");
            string[] names = { "a", "b", "c", "d", "e", "f", "g", "h" };
            double last = 0, prev = 0;
            for (int i = 0; i < names.Length; i++)
            {
                prev = last;
                last = c.Trick(names[i]);
                if (i + 1 > max) Assert.That(last, Is.EqualTo(prev), "link " + (i + 1) + " pays the cap");
            }
        }
    }
}
