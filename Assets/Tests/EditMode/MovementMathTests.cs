// Port of skygrind/tests/movementMath.test.js: same inputs, same expected values, same tolerances.
using System;
using NUnit.Framework;
using static Skygrind.Core.MovementMath;
using static Skygrind.Core.Tests.Expect;

namespace Skygrind.Core.Tests
{
    public class MovementMathTests
    {
        static readonly MovementConfig config = new MovementConfig();
        static readonly double DT = 1 / config.loop.simHz;
        const double EPS = 1e-9;

        static double Dot(Vec3 a, Vec3 b) => a.x * b.x + a.y * b.y + a.z * b.z;

        static double HSpeed(Vec3 v) => JsMath.Hypot(v.x, v.z);

        // describe('jumpVelocity / gravityFor')

        // The JS test's hand values (≈ 9.33, ≈ 31.1) are replaced by the golden vectors for the same call: exact.
        [Test, Description("h = 1.4 m, t = 0.3 s gives v0 ≈ 9.33 m/s and g ≈ 31.1 m/s²")]
        public void JumpPair_H1_4_T0_3()
        {
            double v0 = JumpVelocity(1.4, 0.3);
            double g = GravityFor(1.4, 0.3);
            Exactly(v0, PortData.Num(Port.Result("movementMath", "jumpVelocity", 1.4, 0.3)));
            Exactly(g, PortData.Num(Port.Result("movementMath", "gravityFor", 1.4, 0.3)));
            // The pair reproduces the requested apex height: h = v0² / 2g.
            CloseTo((v0 * v0) / (2 * g), 1.4, 10);
        }

        // describe('accelerate')

        [Test, Description("never adds speed along the wish direction beyond wishSpeed")]
        public void Accelerate_NeverAddsSpeedBeyondWishSpeed()
        {
            double wishSpeed = config.ground.walkSpeed;
            double[] accels = { config.ground.accel, config.air.accel, 1000 };
            for (int deg = 0; deg < 360; deg += 15)
            {
                double a = (deg * Math.PI) / 180;
                var wishDir = new Vec3(Math.Cos(a), 0, Math.Sin(a));
                foreach (double startSpeed in new double[] { 0, 4, 8, 12, 30 })
                {
                    for (int velDeg = 0; velDeg < 360; velDeg += 45)
                    {
                        double b = (velDeg * Math.PI) / 180;
                        foreach (double accel in accels)
                        {
                            var v = new Vec3(startSpeed * Math.Cos(b), 0, startSpeed * Math.Sin(b));
                            double startAlong = Dot(v, wishDir);
                            for (int i = 0; i < 240; i++) v = Accelerate(v, wishDir, wishSpeed, accel, DT);
                            Assert.LessOrEqual(Dot(v, wishDir), Math.Max(wishSpeed, startAlong) + EPS);
                        }
                    }
                }
            }
        }

        [Test, Description("air strafing at 90° to the velocity can increase total speed")]
        public void Accelerate_AirStrafeAt90DegIncreasesSpeed()
        {
            var v = new Vec3(10, 0, 0);
            var wishDir = new Vec3(0, 0, 1);
            var speeds = new double[11];
            speeds[0] = HSpeed(v);
            for (int i = 0; i < 10; i++)
            {
                v = Accelerate(v, wishDir, config.ground.walkSpeed, config.air.accel, DT);
                speeds[i + 1] = HSpeed(v);
            }
            Assert.Greater(speeds[1], 10);
            for (int i = 1; i < speeds.Length; i++) Assert.Greater(speeds[i], speeds[i - 1]);
        }

        // The JS checks `accelerate(v, ..., v)` returns the same object (toBe). Vec3 is a struct, so identity does
        // not exist; the in-place form is `v = Accelerate(v, ...)`, and the values must match.
        [Test, Description("does not mutate its inputs and can write in place")]
        public void Accelerate_DoesNotMutateInputs_CanWriteInPlace()
        {
            var v = new Vec3(1, 2, 3);
            var wishDir = new Vec3(1, 0, 0);
            Vec3 result = Accelerate(v, wishDir, 8, 10, DT);
            Assert.AreEqual(new Vec3(1, 2, 3), v);
            v = Accelerate(v, wishDir, 8, 10, DT);
            Assert.AreEqual(result, v);
        }

        // describe('applyFriction')

        [Test, Description("never reverses the direction of velocity")]
        public void Friction_NeverReversesDirection()
        {
            double friction = config.ground.friction;
            double stopSpeed = config.ground.stopSpeed;
            Vec3[] cases =
            {
                new Vec3(8, 0, 0),
                new Vec3(-3, 5, 4),
                new Vec3(0.1, -2, -0.1),
                new Vec3(40, 0, -40),
            };
            double[][] fdts = { new[] { friction, DT }, new[] { friction, 1.0 }, new[] { 1000, DT }, new[] { 1000, 10.0 } };
            foreach (Vec3 start in cases)
            {
                foreach (double[] fdt in fdts)
                {
                    Vec3 v = ApplyFriction(start, fdt[0], stopSpeed, fdt[1]);
                    var startH = new Vec3(start.x, 0, start.z);
                    Assert.GreaterOrEqual(Dot(v, startH), 0);
                    Assert.LessOrEqual(HSpeed(v), HSpeed(start) + EPS);
                    Exactly(v.y, start.y); // ground-only friction leaves vertical speed alone
                }
            }
        }

        [Test, Description("brings walk speed to a full stop in finite time")]
        public void Friction_StopsWalkSpeedInFiniteTime()
        {
            var v = new Vec3(config.ground.walkSpeed, 0, 0);
            int steps = 0;
            while (HSpeed(v) > 0 && steps < 10000)
            {
                v = ApplyFriction(v, config.ground.friction, config.ground.stopSpeed, DT);
                steps++;
            }
            Exactly(HSpeed(v), 0);
            Assert.Less(steps, config.loop.simHz); // under 1 s
        }
    }
}
