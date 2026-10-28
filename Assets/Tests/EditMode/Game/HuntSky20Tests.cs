// NIGHT20 U3-34.9 A2 (builder V): THE HUNT's dusk shift (Game/HuntSky20.cs): whole within 3 s of the hunt's announce, back
// over 3 s after its end, no snap at either end, the horizon toward DANGER the most and the zenith the least, only the
// hunt's events move it.
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using H = Skygrind.Game.HuntSkyMath;

namespace Skygrind.Game.Tests
{
    public class HuntSky20Tests
    {
        static double Run(double u, double target, double seconds)
        {
            for (double t = 0; t < seconds - 1e-9; t += 1.0 / 60) u = H.Step(u, target, 1.0 / 60);
            return u;
        }

        [Test]
        public void TheSkyTurnsWithinThreeSecondsOfTheAnnounceAndComesBack()
        {
            double u = Run(0, 1, 3.0);
            Assert.That(H.Share(u), Is.EqualTo(1).Within(1e-9), "whole within 3 s of the announce");
            Assert.That(H.Share(Run(0, 1, 1.0)), Is.InRange(0.05, 0.95), "on its way at 1 s");
            u = Run(u, 0, 3.0 + 1.0 / 60);
            Assert.That(H.Share(u), Is.EqualTo(0).Within(1e-9), "back to the dusk within 3 s of its end");
            Assert.That(H.Share(0.001), Is.LessThan(0.0001), "no snap at the start");
            Assert.That(1 - H.Share(0.999), Is.LessThan(0.0001), "none at the top");
            Assert.That(H.Step(0.4, 1, 0), Is.EqualTo(0.4), "no time, no change");
        }

        [Test]
        public void TheHorizonLeansMostAndTheZenithLeast()
        {
            var dusk = new Color(0.2f, 0.1f, 0.4f, 1);
            var danger = new Color(1f, 0.3f, 0.1f, 1);
            Color h = H.Lean(dusk, danger, 1, UnityTuning.HuntHorizon), z = H.Lean(dusk, danger, 1, UnityTuning.HuntZenith);
            Assert.That(h.r - dusk.r, Is.GreaterThan(z.r - dusk.r), "the horizon redder than the zenith");
            Assert.That(H.Lean(dusk, danger, 0, UnityTuning.HuntHorizon), Is.EqualTo(dusk), "share 0: the dusk itself");
            Assert.That(UnityTuning.HuntHorizon, Is.LessThan(1f), "never all DANGER: the sky stays a sky");
            Assert.That(UnityTuning.HuntSkyIn, Is.LessThanOrEqualTo(3.0), "the row: within 3 s");
        }

        [Test]
        public void OnlyTheHuntsEventsMoveIt()
        {
            Assert.That(H.Target(new SpecialRoundEvent { name = "hunt", phase = "announce" }), Is.EqualTo(1));
            Assert.That(H.Target(new SpecialRoundEvent { name = "hunt", phase = "start" }), Is.EqualTo(1));
            Assert.That(H.Target(new SpecialRoundEvent { name = "hunt", phase = "last" }), Is.EqualTo(1));
            Assert.That(H.Target(new SpecialRoundEvent { name = "hunt", phase = "end" }), Is.EqualTo(0));
            Assert.That(H.Target(new SpecialRoundEvent { name = "elite", phase = "announce" }), Is.EqualTo(-1), "another special round: no shift");
            Assert.That(H.Target(null), Is.EqualTo(-1));
        }
    }
}
