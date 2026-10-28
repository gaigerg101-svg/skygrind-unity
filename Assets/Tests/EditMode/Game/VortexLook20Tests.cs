// NIGHT20 B5's look (builder HV): the VORTEX's orbs on builder G2's port (Core/Guns/Vortex.cs, Projectiles.vortexes), as
// projectiles.js render (:520-545) draws them: in flight a small orb (weapons.vortex.radius) turning at 2 rad/s; open, a
// bigger pulsing core radius x (1.6 + 0.8 t) x (1 + 0.08 sin(3 spin)) turning at 5 rad/s (t the pull's share,
// openSteps / simHz / pullTime), two turning rings at ringRadius x (1 - 0.5 t) and x 1.4, and motes drawn in every
// moteInterval s from a squashed sphere of pullRadius (y x 0.4) at pullRadius / 0.5 m/s; the port's: while the held alt
// repels, the motes fly out instead (a repel reads unlike a pull). Cyan: the player's (rule 8).
using NUnit.Framework;
using UnityEngine;
using V = Skygrind.Game.VortexLook20;

namespace Skygrind.Game.Tests
{
    public class VortexLook20Tests
    {
        [Test]
        public void TheOrbGrowsAndPulsesOpen_SmallInFlight()
        {
            Assert.That(V.OrbSize(false, 0, 1.3, 0.35), Is.EqualTo(0.35).Within(1e-9), "in flight: its radius");
            Assert.That(V.OrbSize(true, 0, 0, 0.35), Is.EqualTo(0.35 * 1.6).Within(1e-9), "open: x 1.6 at once");
            Assert.That(V.OrbSize(true, 1, 0, 0.35), Is.EqualTo(0.35 * 2.4).Within(1e-9), "x 2.4 at the burst");
            double spin = 0.4;
            Assert.That(V.OrbSize(true, 0.5, spin, 0.35), Is.EqualTo(0.35 * 2.0 * (1 + 0.08 * System.Math.Sin(spin * 3))).Within(1e-12), "the pulse");
            Assert.That(V.OpenShare(90, 120, 1.5), Is.EqualTo(0.5).Within(1e-12), "openSteps / simHz / pullTime");
            Assert.AreEqual(5.0, V.SpinRate(true));
            Assert.AreEqual(2.0, V.SpinRate(false));
            Assert.That(V.RingRadius(0, 1.4), Is.EqualTo(1.4).Within(1e-12));
            Assert.That(V.RingRadius(1, 1.4), Is.EqualTo(0.7).Within(1e-12), "the rings close in as it pulls");
        }

        /// <summary>viewmodel.js :477-482: the hand's orb shrinks to nothing as it fires and grows back with the reload
        /// (scale max(0.001, (1 - cooldownFraction) x (1 + 0.12 sin(5 t)))); the reload from the loadout's meter (G2's
        /// VortexMeter: 1 = ready), so cooldownFraction = 1 - meter.</summary>
        [Test]
        public void TheHandsOrb_RegrowsWithTheReload()
        {
            Assert.That(V.HandOrbScale(1, 0), Is.EqualTo(1).Within(1e-12), "ready: whole (t 0)");
            Assert.That(V.HandOrbScale(0, 0), Is.EqualTo(0.001).Within(1e-12), "just fired: gone");
            Assert.That(V.HandOrbScale(0.5, 0), Is.EqualTo(0.5).Within(1e-12), "half way back");
            double t = 0.3;
            Assert.That(V.HandOrbScale(1, t), Is.EqualTo(1 + 0.12 * System.Math.Sin(5 * t)).Within(1e-12), "its pulse");
        }

        [Test]
        public void ItsMotes_AreDrawnIn_FromItsPullRadius()
        {
            Vector3 c = new Vector3(1, 2, 3);
            for (int i = 0; i <= 10; i++)
            {
                Vector3 m = V.MoteFrom(c, 7, i / 10.0, (i * 3 % 10) / 10.0);
                Vector3 d = m - c;
                Assert.That(d.y, Is.InRange(-7 * 0.4f - 1e-4f, 7 * 0.4f + 1e-4f), "y squashed x 0.4");
                Assert.That(new Vector2(d.x, d.z).magnitude, Is.LessThanOrEqualTo(7 + 1e-4f));
            }
            Assert.That(V.MoteFrom(c, 7, 0.5, 0).x, Is.EqualTo(1 + 7).Within(1e-4), "u 0, a 0: on the rim");
            Assert.That(V.MoteSpeed(7), Is.EqualTo(14).Within(1e-12), "pullRadius / 0.5");
            double trail = 0;
            int n = 0;
            for (int f = 0; f < 60; f++) n += V.MoteEmits(ref trail, 1 / 60.0, 0.03);
            Assert.That(n, Is.InRange(32, 34), "one every moteInterval s (33 a second)");
            // a pull draws a mote in; a repel sends it out
            Vector3 at = new Vector3(4, 2, 3);
            Assert.That(Vector3.Dot(V.MoteVelocity(at, c, 14, false), c - at), Is.GreaterThan(0), "in");
            Assert.That(Vector3.Dot(V.MoteVelocity(at, c, 14, true), c - at), Is.LessThan(0), "out while it repels");
            Assert.That(V.MoteVelocity(at, c, 14, false).magnitude, Is.EqualTo(14).Within(1e-4));
        }
    }
}
