// NIGHT73 builder B (Game/Core73.cs: THE KILN WYRM's open weak point as a lit core, design.n73.core73). Pinned (the maths only: no engine call):
//   1. the layers: Core73Layers discs, the outermost radius share 1 and strictly smaller after it, every glow positive, and the brightness in the ring between disc i + 1 and disc i (the sum of glow[0..i]) equals the
//      falloff profile at the ring's middle: the discs ARE the falloff
//   2. the falloff is soft: the largest brightness step between neighbouring rings is at most Core73MaxStepShare of the peak, the edge ring at most a twentieth of it, and the centre reaches the peak within 2 %
//      (the NIGHT73 bar "soft falloff / glow instead of a flat translucent cube": a flat box is one step of 100 % at its edge; 3 discs at power 0.5 are 55 % steps)
//   3. the pulse: between 1 - depth and 1, `hz` cycles a second, and the tuned rate is under Core73PulseHzMax (the lead's "slower than 2 Hz") and 12-20 % deep at most (no flicker)
//   4. the fade-in: 0 at the open, 1 after its seconds, never decreasing, no step
//   5. (round 2) the glow's place: on the face of the head's box the camera sees, and its radius inside the head's smallest half size (named below)
using System;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Core73Tests
    {
        static void Build(int n, float power, out float[] radius, out float[] glow)
        {
            radius = new float[n];
            glow = new float[n];
            Core73Math.Layers(n, power, UnityTuning.Core73Peak, radius, glow);
        }

        [Test]
        public void The_discs_build_the_falloff()
        {
            int n = UnityTuning.Core73Layers;
            Build(n, UnityTuning.Core73Power, out float[] radius, out float[] glow);
            Assert.That(radius[0], Is.EqualTo(1f).Within(1e-6f), "the outermost disc is the outer radius");
            float sum = 0f;
            for (int i = 0; i < n; i++)
            {
                Assert.That(glow[i], Is.GreaterThan(0f), "disc " + i + " adds light");
                if (i > 0) Assert.That(radius[i], Is.LessThan(radius[i - 1]), "disc " + i + " is inside disc " + (i - 1));
                sum += glow[i];
                float mid = (radius[i] + (n - i - 1) / (float)n) * 0.5f;
                Assert.That(sum, Is.EqualTo(Core73Math.Profile(mid, UnityTuning.Core73Power, UnityTuning.Core73Peak)).Within(1e-4f), "ring " + i + " is the profile at its middle");
            }
        }

        [Test]
        public void The_falloff_is_soft_not_a_box()
        {
            Build(UnityTuning.Core73Layers, UnityTuning.Core73Power, out _, out float[] glow);
            float peak = UnityTuning.Core73Peak, sum = 0f, max = 0f;
            foreach (float g in glow) { sum += g; max = Math.Max(max, g); }
            Assert.That(max, Is.LessThanOrEqualTo(UnityTuning.Core73MaxStepShare * peak), "the largest step between neighbouring rings");
            Assert.That(glow[0], Is.LessThanOrEqualTo(0.05f * peak), "the edge ring");
            Assert.That(sum, Is.EqualTo(peak).Within(0.02f * peak), "the centre reaches the peak");
        }

        [Test]
        public void The_pulse_is_a_slow_breath()
        {
            float hz = UnityTuning.Core73PulseHz, depth = UnityTuning.Core73PulseDepth;
            Assert.That(hz, Is.LessThan(UnityTuning.Core73PulseHzMax), "slower than the lead's 2 Hz");
            Assert.That(depth, Is.LessThanOrEqualTo(0.2f), "shallow");
            float lo = 2f, hi = 0f;
            for (int i = 0; i < 2000; i++)
            {
                float p = Core73Math.Pulse(i / 1000.0, hz, depth);
                lo = Math.Min(lo, p);
                hi = Math.Max(hi, p);
            }
            Assert.That(hi, Is.EqualTo(1f).Within(1e-4f), "the top");
            Assert.That(lo, Is.EqualTo(1f - depth).Within(1e-3f), "the bottom");
            Assert.That(Core73Math.Pulse(1.0 / hz, hz, depth), Is.EqualTo(Core73Math.Pulse(0.0, hz, depth)).Within(1e-4f), "one cycle is 1 / hz seconds");
        }

        // 5. (ROUND 2, the lead's look: the core above the head read as a faint haze and the "shoot here" mark was gone) where the glow sits: ON the face of the head's box the camera sees: the point where the line from the
        //    box's centre toward the eye leaves the box. It is on the box's surface (the largest of |x| / half size in the box's frame is 1), on the segment toward the eye, on the face the eye is in front of, for a box turned
        //    any way; an eye inside the box gets its own side; and the glow's radius stays inside the smallest half size
        static void Frame(double yawDeg, out Skygrind.Core.Vec3 r, out Skygrind.Core.Vec3 u, out Skygrind.Core.Vec3 f)
        {
            double a = yawDeg * Math.PI / 180;
            r = new Skygrind.Core.Vec3(Math.Cos(a), 0, -Math.Sin(a));
            u = new Skygrind.Core.Vec3(0, 1, 0);
            f = new Skygrind.Core.Vec3(Math.Sin(a), 0, Math.Cos(a));
        }

        [TestCase(0.0, 0.0, 0.0, 10.0, 2)]
        [TestCase(0.0, 10.0, 0.0, 0.0, 0)]
        [TestCase(0.0, 0.0, 10.0, 0.0, 1)]
        [TestCase(30.0, 5.0, 3.0, 12.0, 2)]
        [TestCase(-70.0, 8.0, -2.0, 6.0, 0)]
        [TestCase(125.0, -3.0, 9.0, -2.0, 1)]
        public void The_glow_sits_on_the_face_the_camera_sees(double yaw, double ex, double ey, double ez, int wantAxis)
        {
            Frame(yaw, out var r, out var u, out var f);
            var c = new Skygrind.Core.Vec3(4, 2, -3);
            var eye = c + new Skygrind.Core.Vec3(ex, ey, ez);
            double hr = 1.5, hu = 1.5, hf = 2.0;
            Skygrind.Core.Vec3 p = Core73Math.FrontPoint(c, r, u, f, hr, hu, hf, eye, out int axis);
            Skygrind.Core.Vec3 d = p - c;
            double nr = Math.Abs(Skygrind.Core.Vec3.Dot(d, r)) / hr, nu = Math.Abs(Skygrind.Core.Vec3.Dot(d, u)) / hu, nf = Math.Abs(Skygrind.Core.Vec3.Dot(d, f)) / hf;
            Assert.That(Math.Max(nr, Math.Max(nu, nf)), Is.EqualTo(1.0).Within(1e-9), "on the box's surface (the largest normalised coordinate is 1)");
            Skygrind.Core.Vec3 toEye = eye - c;
            Assert.That(Skygrind.Core.Vec3.Dot(d, toEye), Is.EqualTo(d.Length * toEye.Length).Within(1e-9), "on the line from the centre to the eye");
            Assert.That(axis, Is.EqualTo(wantAxis), "the face the eye is in front of");
        }

        [Test]
        public void An_eye_inside_the_box_gets_its_own_side_and_a_degenerate_eye_the_centre()
        {
            Frame(0, out var r, out var u, out var f);
            var c = new Skygrind.Core.Vec3(0, 0, 0);
            var inside = new Skygrind.Core.Vec3(0.3, 0.2, 0.5);
            Assert.That((Core73Math.FrontPoint(c, r, u, f, 1.5, 1.5, 2.0, inside, out _) - inside).Length, Is.LessThan(1e-9), "t capped at 1: never past the eye");
            Assert.That(Core73Math.FrontPoint(c, r, u, f, 1.5, 1.5, 2.0, c, out _).Length, Is.LessThan(1e-9), "the eye on the centre");
        }

        [Test]
        public void The_glow_radius_stays_inside_the_heads_smallest_half_size()
        {
            double rad = Core73Math.FaceRadius(1.5, 1.5, 2.0, UnityTuning.Core73FaceShare);
            Assert.That(rad, Is.LessThan(1.5), "inside the face's outline from the front");
            Assert.That(rad, Is.GreaterThan(0.9), "big enough to read at a glance (the round-1 core was 1.43 m, above the head)");
        }

        [Test]
        public void The_fade_in_is_a_smooth_step()
        {
            float s = UnityTuning.Core73FadeS, prev = -1f;
            Assert.That(Core73Math.Fade(0.0, s), Is.EqualTo(0f));
            Assert.That(Core73Math.Fade(s, s), Is.EqualTo(1f));
            Assert.That(Core73Math.Fade(s * 5.0, s), Is.EqualTo(1f));
            for (int i = 0; i <= 100; i++)
            {
                float f = Core73Math.Fade(s * i / 100.0, s);
                Assert.That(f, Is.GreaterThanOrEqualTo(prev), "never decreasing");
                prev = f;
            }
        }
    }
}
