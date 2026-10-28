// NIGHT19 U3-33.5 / U3-33.8 (builder V owns this file): the pure rules behind tonight's hands and effects: Spring-It-On's
// half-life springs (the port's maths against its own definitions), the gust's kick in the hands (REDUCED MOTION stills
// it), the gust's air (its cone, rings, wake and FOV kick), the alt-fires' poses, the R3 jab, the ZAPPER's arc
// (arcs.js's kinks: the ends stay put), the BOUNCER's fuse blink (projectiles.js render), and horde5's confetti cut.
// Pure (no scene, no native UnityEngine call): every test here also runs under dotnet (the builders' runner).
using System;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class HandsN19Tests
    {
        const double Dt = 1.0 / 120;

        // --- Spring-It-On's springs ------------------------------------------------------------------------------------------

        [Test]
        public void TheDamperCoversHalfTheDistanceInOneHalfLife()
        {
            double x = HalfLifeSpring.DamperExact(0, 1, 0.2, 0.2);
            Assert.That(x, Is.EqualTo(0.5).Within(0.01), "damper_exact: half the way to the goal in one half-life");
            Assert.That(HalfLifeSpring.FastNegexp(0), Is.EqualTo(1).Within(1e-12));
            Assert.That(HalfLifeSpring.FastNegexp(1), Is.EqualTo(Math.Exp(-1)).Within(0.01), "fast_negexp approximates e^-x");
            Assert.That(HalfLifeSpring.HalflifeToDamping(0.1), Is.EqualTo(4 * 0.69314718056 / (0.1 + 1e-5)).Within(1e-9));
        }

        [Test]
        public void TheCriticalSpringReachesItsGoalWithoutOvershoot()
        {
            double x = 1, v = 0, min = 1;
            for (int i = 0; i < 240; i++)
            {
                HalfLifeSpring.SimpleSpringDamperExact(ref x, ref v, 0, 0.1, Dt);
                min = Math.Min(min, x);
            }
            Assert.That(min, Is.GreaterThanOrEqualTo(-1e-6), "critically damped: never past the goal");
            Assert.That(x, Is.EqualTo(0).Within(1e-3), "settled within 2 s");
            double cx = 0, cv = 0;
            for (int i = 0; i < 240; i++) HalfLifeSpring.CriticalSpringDamperExact(ref cx, ref cv, 2, 0, 0.1, Dt);
            Assert.That(cx, Is.EqualTo(2).Within(1e-3), "the moving-goal spring reaches a still goal");
        }

        [Test]
        public void AKickPeaksAtItsGivenSizeOnTime()
        {
            const double half = 0.075;
            var k = new HalfLifeKick(2, half);
            k.Kick(new[] { 10.0, -0.1 });
            double peak = 0, at = 0, lowest = 0;
            for (int i = 1; i <= 120; i++)
            {
                k.Step(Dt);
                if (k.Value(0) > peak) { peak = k.Value(0); at = i * Dt; }
                lowest = Math.Min(lowest, k.Value(0));
            }
            Assert.That(peak, Is.EqualTo(10).Within(0.3), "the first channel peaks at its size");
            Assert.That(at, Is.EqualTo(HalfLifeSpring.PeakTime(half)).Within(2 * Dt), "at the decay spring's peak time");
            Assert.That(lowest, Is.GreaterThanOrEqualTo(-1e-6), "no swing past rest (no wobble)");
            Assert.That(k.Active, Is.False, "settled after 1 s");
            k.Kick(new[] { 1.0, 1.0 }, 0);
            Assert.That(k.Active, Is.False, "a kick at scale 0 moves nothing");
        }

        // --- the gust in the hands -------------------------------------------------------------------------------------------

        static void Peaks(HandsGust g, double seconds, out double back, out double pitch, out double end)
        {
            back = pitch = 0;
            HandsDrawPose p = default;
            for (int i = 0; i < (int)(seconds / Dt); i++)
            {
                g.Step(Dt);
                p = g.Sample();
                back = Math.Max(back, p.z);
                pitch = Math.Max(pitch, p.pitchDeg);
            }
            end = Math.Abs(p.z) + Math.Abs(p.pitchDeg) / 100 + Math.Abs(p.rollDeg) / 100;
        }

        [Test]
        public void TheGustShovesTheGunBackAndUpThenSettles()
        {
            var g = new HandsGust();
            g.Fire(1, false);
            Assert.That(g.Active, Is.True);
            Peaks(g, 0.8, out double back, out double pitch, out double end);
            Assert.That(back, Is.EqualTo(UnityTuning.GustKickBackZ).Within(UnityTuning.GustKickBackZ * 0.2), "the blowback toward the eye");
            Assert.That(pitch, Is.EqualTo(UnityTuning.GustKickPitchDeg).Within(UnityTuning.GustKickPitchDeg * 0.1), "the muzzle flips up");
            Assert.That(end, Is.LessThan(0.002), "back at rest by 0.8 s");
        }

        [Test]
        public void TheGustThrustsForwardBeforeTheBlowback()
        {
            var g = new HandsGust();
            g.Fire(1, false);
            double minZ = 0;
            for (int i = 0; i < 4; i++)
            {
                g.Step(Dt);
                minZ = Math.Min(minZ, g.Sample().z);
            }
            Assert.That(minZ, Is.LessThan(0), "the first frames push the gun forward (web -z), the air going out");
        }

        [Test]
        public void ReducedMotionStillsTheGustKick()
        {
            var g = new HandsGust();
            g.Fire(0, true);
            Peaks(g, 0.5, out double back, out double pitch, out _);
            Assert.That(back, Is.EqualTo(0));
            Assert.That(pitch, Is.EqualTo(0));
            Assert.That(g.Active, Is.False);
        }

        [Test]
        public void AGustThatLaunchesTheBodyDropsTheGunInView()
        {
            var a = new HandsGust();
            var b = new HandsGust();
            a.Fire(1, false);
            b.Fire(1, true);
            double minA = 0, minB = 0;
            for (int i = 0; i < 30; i++)
            {
                a.Step(Dt);
                b.Step(Dt);
                minA = Math.Min(minA, a.Sample().y);
                minB = Math.Min(minB, b.Sample().y);
            }
            Assert.That(minB, Is.LessThan(minA - UnityTuning.GustAirLiftY * 0.5), "launched up: the gun lags below");
        }

        // --- the gust's air --------------------------------------------------------------------------------------------------

        [Test]
        public void TheStreaksFillTheConeAndStayInsideIt()
        {
            Vector3 dir = new Vector3(0.3f, -0.2f, 0.93f).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, dir).normalized, up = Vector3.Cross(dir, right);
            double maxAngle = 0;
            for (int i = 0; i < 50; i++)
            {
                Vector3 d = GustFxMath.ConeDir(dir, right, up, 30, i / 49.0, (i * 0.618) % 1);
                Assert.That(d.magnitude, Is.EqualTo(1).Within(1e-4));
                maxAngle = Math.Max(maxAngle, Vector3.Angle(dir, d));
            }
            Assert.That(maxAngle, Is.LessThanOrEqualTo(30.01), "never outside the half-angle");
            Assert.That(maxAngle, Is.GreaterThan(25), "reaching its edge");
        }

        [Test]
        public void AStreakTravelsAboutTheGustsRangeAndSlows()
        {
            double far = GustFxMath.Travel(UnityTuning.GustStreakSpeed, UnityTuning.GustStreakDrag, UnityTuning.GustStreakLife);
            Assert.That(far, Is.InRange(4.0, 8.0), "a streak covers most of the 8 m cone");
            Assert.That(GustFxMath.Speed(10, 6, 0), Is.EqualTo(10).Within(1e-9));
            Assert.That(GustFxMath.Speed(10, 6, 0.5), Is.EqualTo(10 * Math.Exp(-3)).Within(1e-9));
            Assert.That(GustFxMath.StreakLength(0), Is.EqualTo(UnityTuning.GustStreakMinLen).Within(1e-9));
            Assert.That(GustFxMath.StreakLength(40), Is.EqualTo(40 * UnityTuning.GustStreakStretch).Within(1e-9));
            Assert.That(GustFxMath.StreakFade(0, 0.3), Is.EqualTo(1));
            Assert.That(GustFxMath.StreakFade(0.3, 0.3), Is.EqualTo(0));
        }

        [Test]
        public void TheRingsGrowOutAndFade()
        {
            Assert.That(GustFxMath.RingRadius(0, 0.2, 2), Is.EqualTo(0.2).Within(1e-9));
            Assert.That(GustFxMath.RingRadius(1, 0.2, 2), Is.EqualTo(2).Within(1e-9));
            Assert.That(GustFxMath.RingRadius(0.5, 0.2, 2), Is.GreaterThan(1.1), "an ease-out: most of the growth early");
            Assert.That(GustFxMath.RingAlpha(0), Is.EqualTo(UnityTuning.GustRingAlpha).Within(1e-9));
            Assert.That(GustFxMath.RingAlpha(1), Is.EqualTo(0));
            Assert.That(GustFxMath.RingAlpha(0.5), Is.LessThan(UnityTuning.GustRingAlpha * 0.5));
        }

        [Test]
        public void TheWakeRunsOnlyWhileThePushCarries()
        {
            Assert.That(GustFxMath.WakeRate(0.1, 12), Is.GreaterThan(0));
            Assert.That(GustFxMath.WakeRate(0.1, UnityTuning.GustWakeMinSpeed * 0.9), Is.EqualTo(0), "too slow: no wake");
            Assert.That(GustFxMath.WakeRate(UnityTuning.GustWakeTime + 0.01, 12), Is.EqualTo(0), "after its window: none");
            Assert.That(GustFxMath.WakeRate(0.1, 40), Is.EqualTo(UnityTuning.GustWakeRate).Within(1e-9), "capped at the full rate");
        }

        [Test]
        public void TheGustsFovKickIsWholeOnItsFirstFrameAndGoneAfter()
        {
            Assert.That(GustFxMath.FovKick(0, false), Is.EqualTo(UnityTuning.GustFovKickDeg).Within(1e-9));
            Assert.That(GustFxMath.FovKick(0, true), Is.EqualTo(UnityTuning.GustFovKickDeg * UnityTuning.GustFovAirMult).Within(1e-9));
            Assert.That(GustFxMath.FovKick(UnityTuning.GustFovBackTime, false), Is.EqualTo(0));
            Assert.That(GustFxMath.FovKick(-0.01, false), Is.EqualTo(0));
            Assert.That(GustFxMath.FovKick(UnityTuning.GustFovBackTime * 0.5, false), Is.InRange(0.01, UnityTuning.GustFovKickDeg));
        }

        // --- the alt-fires and the jab ----------------------------------------------------------------------------------------

        [Test]
        public void EachAltKicksTheGunItsOwnWayAndMotionZeroStillsIt()
        {
            var a = new HandsAlt();
            a.Parry(1);
            double minZ = 0, minRoll = 0;
            for (int i = 0; i < 20; i++)
            {
                a.Frame(Dt, false, 0);
                HandsDrawPose p = a.Sample(1);
                minZ = Math.Min(minZ, p.z);
                minRoll = Math.Min(minRoll, p.rollDeg);
            }
            Assert.That(minZ, Is.EqualTo(UnityTuning.AltParryZ).Within(Math.Abs(UnityTuning.AltParryZ) * 0.15), "the THUNDERCLAP bashes forward");
            Assert.That(minRoll, Is.EqualTo(UnityTuning.AltParryRollDeg).Within(Math.Abs(UnityTuning.AltParryRollDeg) * 0.15));
            var b = new HandsAlt();
            b.Detonate(1);
            double maxRoll = 0;
            for (int i = 0; i < 20; i++)
            {
                b.Frame(Dt, false, 0);
                maxRoll = Math.Max(maxRoll, b.Sample(1).rollDeg);
            }
            Assert.That(maxRoll, Is.EqualTo(UnityTuning.AltDetonateRollDeg).Within(UnityTuning.AltDetonateRollDeg * 0.15), "the STITCHER's key-turn");
            var c = new HandsAlt();
            c.Discharge(0);
            c.Airburst(0);
            c.Frame(Dt, false, 0);
            HandsDrawPose q = c.Sample(0);
            Assert.That(Math.Abs(q.z) + Math.Abs(q.pitchDeg) + Math.Abs(q.rollDeg), Is.EqualTo(0), "motion 0: no kick");
        }

        [Test]
        public void SteeringRaisesTheCometAndTheOverchargeTrembles()
        {
            var a = new HandsAlt();
            for (int i = 0; i < 60; i++) a.Frame(Dt, true, 0);
            Assert.That(a.steerW, Is.GreaterThan(0.95), "eased in over half a second");
            HandsDrawPose p = a.Sample(1);
            Assert.That(p.x, Is.EqualTo(UnityTuning.AltSteerX).Within(0.01));
            Assert.That(p.y, Is.EqualTo(UnityTuning.AltSteerY).Within(0.01));
            for (int i = 0; i < 60; i++) a.Frame(Dt, false, 0);
            Assert.That(a.steerW, Is.LessThan(0.05), "and out when it stops");
            var b = new HandsAlt();
            double spread = 0;
            for (int i = 0; i < 30; i++)
            {
                b.Frame(Dt, false, 1);
                spread = Math.Max(spread, Math.Abs(b.Sample(1).rollDeg));
            }
            Assert.That(spread, Is.GreaterThan(UnityTuning.AltOverchargeShakeDeg * 0.5), "a full overcharge trembles");
            Assert.That(spread, Is.LessThanOrEqualTo(UnityTuning.AltOverchargeShakeDeg + 1e-9));
            double still = 0;
            for (int i = 0; i < 30; i++)
            {
                b.Frame(Dt, false, 1);
                still = Math.Max(still, Math.Abs(b.Sample(0).rollDeg));
            }
            Assert.That(still, Is.EqualTo(0), "REDUCED MOTION: no tremble");
        }

        static HandsArmParams Arm() => new HandsArmParams
        {
            restX = -0.24, restY = -0.42, restZ = -0.32, restPitch = 40, restYaw = -25, restRoll = 0,
            punchX = -0.05, punchY = -0.09, punchZ = -0.5, punchPitch = 22, punchYaw = -18, punchRoll = -20,
            outTime = 0.06, holdTime = 0.135, cycleTime = 0.35,
        };

        [Test]
        public void TheJabLandsStraightAndLevelOnItsImpactFrame()
        {
            HandsArmParams a = Arm();
            HandsArmPose hit = HandsJab.Pose(a, a.outTime, true, 0, 0);
            Assert.That(hit.visible, Is.True);
            Assert.That(hit.x, Is.EqualTo(a.punchX + UnityTuning.JabAimX).Within(1e-6));
            Assert.That(hit.y, Is.EqualTo(a.punchY + UnityTuning.JabAimY).Within(1e-6));
            Assert.That(hit.z, Is.EqualTo(a.punchZ - UnityTuning.JabOverZ).Within(1e-6), "reaching past the target");
            Assert.That(hit.rollDeg, Is.EqualTo(a.punchRoll + UnityTuning.JabOverRollDeg).Within(1e-6), "the knuckles level");
            HandsArmPose cock = HandsJab.Pose(a, a.outTime * UnityTuning.JabCockShare, false, 0, 0);
            Assert.That(cock.z, Is.EqualTo(UnityTuning.JabCockZ).Within(1e-6), "a short wind-up");
            Assert.That(HandsJab.Pose(a, 0, false, 0, 0).visible, Is.False);
            Assert.That(HandsJab.Pose(a, a.cycleTime, false, 0, 0).visible, Is.False);
            HandsArmPose late = HandsJab.Pose(a, a.cycleTime - 1e-4, true, 0, 0);
            Assert.That(late.z, Is.EqualTo(a.restZ).Within(0.01), "back at rest by the cycle's end");
        }

        // --- the ZAPPER's arc, the BOUNCER's grenade --------------------------------------------------------------------------

        [Test]
        public void TheArcsEndsStayPutAndItsKinksStayWithinTheJag()
        {
            const int segments = 10;
            var kinks = new float[(segments + 1) * 2];
            uint state = 12345;
            ZapArcMath.Kink(kinks, segments, ref state);
            bool any = false;
            foreach (float k in kinks)
            {
                Assert.That(k, Is.InRange(-1f, 1f));
                any |= Math.Abs(k) > 0.05f;
            }
            Assert.That(any, Is.True, "kinked");
            var pts = new Vector3[segments + 1];
            Vector3 from = new Vector3(1, 2, 3), to = new Vector3(4, 2, 7);
            ZapArcMath.Build(from, to, kinks, segments, 0.09f, pts);
            Assert.That((pts[0] - from).magnitude, Is.LessThan(1e-4f));
            Assert.That((pts[segments] - to).magnitude, Is.LessThan(1e-4f));
            float len = (to - from).magnitude;
            Vector3 axis = (to - from) / len;
            for (int i = 1; i < segments; i++)
            {
                Vector3 off = pts[i] - from;
                Vector3 across = off - axis * Vector3.Dot(off, axis);
                Assert.That(across.magnitude, Is.LessThanOrEqualTo(0.09f * len * 1.4143f), "within the jag");
                Assert.That(Vector3.Dot(off, axis), Is.EqualTo(len * i / segments).Within(1e-3f), "evenly along");
            }
            Assert.That(ZapArcMath.Fade(0.14f, 0.14f), Is.EqualTo(1f));
            Assert.That(ZapArcMath.Fade(0.07f, 0.14f), Is.EqualTo(0.75f).Within(1e-5f), "arcs.js: min(1, life / maxLife x 1.5)");
            Assert.That(ZapArcMath.Fade(0, 0.14f), Is.EqualTo(0f));
        }

        [Test]
        public void TheGrenadeBlinksOnlyInItsLastSixTenths()
        {
            const double fuse = 1.6, hz = 120;
            Assert.That(BouncerFxMath.Blink(fuse, 0, hz), Is.EqualTo(1));
            Assert.That(BouncerFxMath.Blink(fuse, (int)((fuse - 0.7) * hz), hz), Is.EqualTo(1), "0.7 s left: no blink");
            int on = 0, off = 0;
            for (int s = (int)((fuse - 0.59) * hz); s < (int)(fuse * hz); s++)
            {
                double k = BouncerFxMath.Blink(fuse, s, hz);
                if (k == UnityTuning.GrenadeBlinkScale) on++;
                else if (k == 1) off++;
                else Assert.Fail("only 1 or the blink scale");
            }
            Assert.That(on, Is.GreaterThan(20));
            Assert.That(off, Is.GreaterThan(20));
            Vec3 axis = BouncerFxMath.TumbleAxis(new Vec3(0, 0, 0));
            Assert.That(axis.Length, Is.EqualTo(1).Within(1e-9));
            Vec3 web = new Vec3(0.3, 0.4, -0.2) / new Vec3(0.3, 0.4, -0.2).Length; // projectiles.js tmp2Axis with dir.z, dir.x = 0
            Assert.That(axis.x, Is.EqualTo(web.x).Within(1e-9));
            Assert.That(axis.z, Is.EqualTo(web.z).Within(1e-9));
        }

        // --- horde5's confetti --------------------------------------------------------------------------------------------------

        [Test]
        public void TheConfettiIsCutOnlyWhereTheChunksCarryTheKill()
        {
            Assert.That(CrunchMath.DeathBurstCount(26, true, false), Is.EqualTo(26), "FeelAB off: the web's");
            Assert.That(CrunchMath.DeathBurstCount(26, false, true), Is.EqualTo(26), "no chunks: the burst is the death");
            Assert.That(CrunchMath.DeathBurstCount(26, true, true), Is.EqualTo(UnityTuning.DeathBurstWithChunks), "the chunks are the death: no confetti");
            Assert.That(CrunchMath.BlastBurstCount(30, false), Is.EqualTo(30));
            Assert.That(CrunchMath.BlastBurstCount(30, true), Is.EqualTo((30 * UnityTuning.BlastBurstShareN19Pct + 50) / 100));
            Assert.That(CrunchMath.BlastBurstCount(30, true), Is.LessThan(30));
        }
    }
}
