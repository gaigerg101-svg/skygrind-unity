// NIGHT20 U3-34.9 C2 (builder V): the LANCER's laser and beam (Game/LancerBeam20.cs; lancer.js render :166-178): the laser
// thickens from 0.012 to 0.032 m over the aim (the shot is coming), the beam starts at 0.08 m and is gone by 0.2 s, and the
// tube runs exactly from the muzzle to its point.
using NUnit.Framework;
using UnityEngine;
using L = Skygrind.Game.LancerBeamMath;

namespace Skygrind.Game.Tests
{
    public class LancerBeam20Tests
    {
        [Test]
        public void TheLaserThickensTowardTheShot()
        {
            Assert.That(L.AimRadius(0), Is.EqualTo(0.012).Within(1e-12), "lancer.js: 0.012 as the aim starts");
            Assert.That(L.AimRadius(1), Is.EqualTo(0.032).Within(1e-12), "+ 0.02 at the shot");
            Assert.That(L.AimRadius(0.5), Is.EqualTo(0.022).Within(1e-12));
            Assert.That(L.AimRadius(2), Is.EqualTo(0.032).Within(1e-12), "never past the shot's");
        }

        [Test]
        public void TheBeamFlashesAndIsGoneByAFifthOfASecond()
        {
            Assert.That(L.BeamRadius(0), Is.EqualTo(0.08).Within(1e-12));
            Assert.That(L.BeamRadius(0.1), Is.EqualTo(0.04).Within(1e-12));
            Assert.That(L.BeamRadius(0.2), Is.EqualTo(0));
            Assert.That(L.BeamRadius(-1), Is.EqualTo(0));
        }

        [Test]
        public void TheTubeRunsFromTheMuzzleToItsPoint()
        {
            var from = new Vector3(3, 6, -2);
            var to = new Vector3(-5, 1.5f, 9);
            Matrix4x4 m = L.Place(from, to, 0.05f);
            Assert.That(Vector3.Distance(m.MultiplyPoint3x4(Vector3.zero), from), Is.LessThan(1e-4f), "y 0: the muzzle");
            Assert.That(Vector3.Distance(m.MultiplyPoint3x4(Vector3.up), to), Is.LessThan(1e-3f), "y 1: the aim point");
            Assert.That(m.MultiplyVector(Vector3.right).magnitude, Is.EqualTo(0.05f).Within(1e-5), "its radius");
        }
    }
}
