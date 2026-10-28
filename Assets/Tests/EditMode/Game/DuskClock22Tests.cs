// NIGHT22 THE DUSK CLOCK (the lead): the pure rules and the config. The share is 0 at round 1, 1 at round 15, linear between and
// clamped; the ease reaches its target in easeSeconds and never overshoots; the config's round-15 colours are violets (no
// red-orange, nothing within 40 of magenta) and darker than the shipped dusk's horizon.
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class DuskClock22Tests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        [Test]
        public void TheShare_IsZeroAtRound1_OneAtRound15_AndLinearBetween()
        {
            Assert.That(DuskClockMath.Target(1, 15), Is.EqualTo(0));
            Assert.That(DuskClockMath.Target(0, 15), Is.EqualTo(0));
            Assert.That(DuskClockMath.Target(15, 15), Is.EqualTo(1));
            Assert.That(DuskClockMath.Target(40, 15), Is.EqualTo(1));
            Assert.That(DuskClockMath.Target(8, 15), Is.EqualTo(0.5).Within(1e-12));
            for (int r = 1; r < 15; r++) Assert.That(DuskClockMath.Target(r + 1, 15), Is.GreaterThan(DuskClockMath.Target(r, 15)));
        }

        [Test]
        public void TheEase_ReachesItsTargetInEaseSeconds_WithoutOvershoot()
        {
            double shown = 0;
            int steps = 0;
            while (shown < 1 && steps < 100000) { shown = DuskClockMath.Step(shown, 1, 1 / 60.0, 3); steps++; }
            Assert.That(shown, Is.EqualTo(1));
            Assert.That(steps / 60.0, Is.EqualTo(3).Within(0.05), "3 s of frame time for the whole span");
            Assert.That(DuskClockMath.Step(0.5, 0.5, 0.1, 3), Is.EqualTo(0.5));
            Assert.That(DuskClockMath.Step(1, 0, 0.5, 3), Is.EqualTo(1 - 0.5 / 3).Within(1e-12), "it eases back down too");
        }

        [Test]
        public void TheRound15Colours_AreVioletAndDarker_NeverMagentaOrRedOrange()
        {
            GameConfig cfg = Cfg();
            Assert.That(cfg.Has("design.n22.duskClock.lastRound"), Is.True);
            double lumHorizon0 = Lum(cfg, "colors.skyHorizon");
            foreach (string k in new[] { "zenith", "mid", "horizon", "fog" })
            {
                var (r, g, b) = cfg.Rgb("design.n22.duskClock." + k);
                double r8 = r * 255, g8 = g * 255, b8 = b * 255;
                Assert.That(b8, Is.GreaterThan(r8), k + ": blue-violet, not red-orange");
                double dm = Math.Sqrt((255 - r8) * (255 - r8) + g8 * g8 + (255 - b8) * (255 - b8));
                Assert.That(dm, Is.GreaterThan(40), k + ": not within 40 of magenta");
            }
            Assert.That(Lum(cfg, "design.n22.duskClock.horizon"), Is.LessThan(lumHorizon0), "the round-15 horizon is darker than the shipped dusk's");
        }

        [Test]
        public void TheRow_IsReadByCode_AndOffWithForceOff()
        {
            Assert.That(N22Blocks.From(Cfg()).duskClock, Is.True, "turned on for the Player contract run (NIGHT22 C1)");
        }

        static double Lum(GameConfig cfg, string path)
        {
            var (r, g, b) = cfg.Rgb(path);
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }
    }
}
