// NIGHT20 (builder V): gunFx.js's sprays ported (Game/GunFx.Spray20.cs; particles.js spray()): a particle leaves along the
// aim inside sin(spread) of it at speed x (1 - speedJitter x r), rises by `rise`, carries the mover's velocity; its life is
// life x (1 - lifeJitter x r); the cone wave's five bursts stand 1.5 + 1.3 i m along the cone, 0.3 m low (gunFx.js :65-70);
// randomDir is uniform on the sphere.
using NUnit.Framework;
using UnityEngine;
using S = Skygrind.Game.SprayMath;

namespace Skygrind.Game.Tests
{
    public class Spray20Tests
    {
        [Test]
        public void ASprayParticleLeavesAlongTheAimInsideItsSpread()
        {
            Vector3 aim = Vector3.forward;
            for (int i = 0; i < 50; i++)
            {
                Vector3 rnd = S.UnitSphere(i / 50.0, (i * 7 % 50) / 50.0);
                Vector3 v = S.Velocity(aim, rnd, 0.5, 1, 18, 0.5, 0, 0, Vector3.zero);
                Assert.That(v.magnitude, Is.EqualTo(18).Within(1e-3), "speed x (1 - jitter x 0)");
                Assert.That(Vector3.Angle(aim, v), Is.LessThanOrEqualTo(Mathf.Asin(Mathf.Sin(0.5f)) * Mathf.Rad2Deg + 0.01f), "inside sin(spread) of the aim");
            }
            Vector3 w = S.Velocity(aim, Vector3.up, 0.2, 0, 15, 0.5, 1, 1.4, new Vector3(3, 0, 0));
            Assert.That(w.z, Is.EqualTo(7.5f).Within(1e-4), "the jitter: x (1 - 0.5 x 1); r 0: dead on the aim");
            Assert.That(w.y, Is.EqualTo(1.4f).Within(1e-4), "the flame's rise");
            Assert.That(w.x, Is.EqualTo(3f).Within(1e-4), "the mover's velocity carried");
        }

        [Test]
        public void LivesJitterAndTheConeWaveStandsAlongTheCone()
        {
            Assert.That(S.Life(0.4, 0.3, 0), Is.EqualTo(0.4).Within(1e-12));
            Assert.That(S.Life(0.4, 0.3, 1), Is.EqualTo(0.28).Within(1e-12));
            Vector3 o = new Vector3(1, 2, 3), d = Vector3.forward;
            Assert.That(Vector3.Distance(S.ConeBurstAt(o, d, 0), new Vector3(1, 1.7f, 4.5f)), Is.LessThan(1e-5f), "1.5 m out, 0.3 m low");
            Assert.That(Vector3.Distance(S.ConeBurstAt(o, d, 4), new Vector3(1, 1.7f, 3 + 1.5f + 4 * 1.3f)), Is.LessThan(1e-4f), "the fifth at 6.7 m");
        }

        [Test]
        public void RandomDirIsOnTheUnitSphere()
        {
            for (int i = 0; i < 20; i++)
                Assert.That(S.UnitSphere(i / 20.0, 0.37).magnitude, Is.EqualTo(1).Within(1e-5));
            Assert.That(S.UnitSphere(1, 0).y, Is.EqualTo(1).Within(1e-6), "u = 1: the pole");
        }
    }
}
