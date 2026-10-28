// NIGHT20 U3-34.7 step 3 (builder V): the travel blur's pure rules (Game/Render/TravelBlurFeature.cs; the research's 3.3,
// Bauxitedev/godot-motion-blur's reprojection with the linear velocity only): 0 under 14 m/s (no blur on a sprint or a
// slide), whole at the boost's 24, x EFFECTS INTENSITY, off with the setting or REDUCED MOTION; the middle sharp inside
// rho 0.25, whole from 0.60; a strafe smears sideways, forward travel smears away from the middle, the sky barely moves;
// the smear clamped to 1.8 % of the width; the look never enters (no velocity: no smear, whatever the view's turn).
using NUnit.Framework;
using UnityEngine;
using B = Skygrind.Game.TravelBlurMath;

namespace Skygrind.Game.Tests
{
    public class TravelBlurTests
    {
        const float TanV = 0.75355405f; // tan(37 deg): the base FOV 74
        const float TanH = TanV * 16f / 9f;

        [Test]
        public void TheBlurWaitsForTheBoost()
        {
            Assert.That(B.Strength(10.5, 1, true), Is.EqualTo(0), "a sprint: none");
            Assert.That(B.Strength(11.6, 1, true), Is.EqualTo(0), "a slide: none");
            Assert.That(B.Strength(14, 1, true), Is.EqualTo(0));
            Assert.That(B.Strength(19, 1, true), Is.EqualTo(0.5).Within(1e-12), "half way: a smoothstep");
            Assert.That(B.Strength(24, 1, true), Is.EqualTo(1).Within(1e-12), "the boost: whole");
            Assert.That(B.Strength(40, 1, true), Is.EqualTo(1));
            Assert.That(B.Strength(24, 0.5, true), Is.EqualTo(0.5).Within(1e-12), "x EFFECTS INTENSITY");
            Assert.That(B.Strength(24, 1, false), Is.EqualTo(0), "MOTION BLUR off or REDUCED MOTION: none");
        }

        [Test]
        public void TheMiddleStaysSharp()
        {
            Assert.That(B.Mask(0.1), Is.EqualTo(0));
            Assert.That(B.Mask(0.25), Is.EqualTo(0).Within(1e-12));
            Assert.That(B.Mask(0.4), Is.InRange(0.01, 0.99));
            Assert.That(B.Mask(0.6), Is.EqualTo(1).Within(1e-12));
            Assert.That(B.Mask(1.3), Is.EqualTo(1));
        }

        [Test]
        public void TheSmearFollowsTheTravelNotTheLook()
        {
            var p = new Vector3(0.5f * TanH * 3, 0.2f * TanV * 3, 3); // a wall pixel 3 m ahead, right of the middle
            Vector2 strafe = B.Offset(p, new Vector3(0.24f, 0, 0), TanH, TanV);
            Assert.That(strafe.x, Is.Not.EqualTo(0), "a strafe smears ...");
            Assert.That(strafe.y, Is.EqualTo(0).Within(1e-6), "... sideways only");
            Vector2 ahead = B.Offset(new Vector3(0.8f * TanH * 3, 0, 3), new Vector3(0, 0, 0.24f), TanH, TanV);
            Assert.That(ahead.y, Is.EqualTo(0).Within(1e-6), "forward travel: along the radius from the middle");
            Assert.That(ahead.x, Is.Not.EqualTo(0));
            Assert.That(B.Offset(Vector3.forward * 3, new Vector3(0, 0, 0.24f), TanH, TanV).magnitude, Is.EqualTo(0).Within(1e-6), "the focus of expansion holds still");
            Vector2 sky = B.Offset(new Vector3(0.8f * TanH * 1000, 0, 1000), new Vector3(0.24f, 0, 0.24f), TanH, TanV);
            Assert.That(sky.magnitude, Is.LessThan(2e-4), "the sky barely moves");
            Vector3 still = B.ViewDisp(Vector3.zero, Vector3.right, Vector3.up, Vector3.forward, 0.01);
            Assert.That(still.magnitude, Is.EqualTo(0), "no travel, no smear, however the view turns");
            Vector3 d = B.ViewDisp(new Vector3(0, 0, 24), Vector3.right, Vector3.up, Vector3.forward, 0.01);
            Assert.That(d.z, Is.EqualTo(0.24f).Within(1e-6), "the eye's travel over the shutter, in view space");
            Vector3 turned = B.ViewDisp(new Vector3(24, 0, 0), Vector3.forward, Vector3.up, Vector3.left, 0.01);
            Assert.That(turned.x, Is.EqualTo(0).Within(1e-6), "looking along the travel: no sideways part");
            Assert.That(turned.z, Is.EqualTo(-0.24f).Within(1e-6));
        }

        [Test]
        public void TheSmearIsClampedToItsShareOfTheWidth()
        {
            // the research's sanity check: a wall point 3 m aside and 3 m ahead at the boost moves ~53 px at 1920 in 0.01 s
            Vector2 wall = B.Offset(new Vector3(3, 0, 3), new Vector3(0, 0, 0.24f), TanH, TanV);
            Assert.That(Mathf.Abs(wall.x) * 1920, Is.InRange(40f, 70f));
            Vector2 c = B.Clamp(wall, 16f / 9f, UnityTuning.TravelBlurClamp);
            Assert.That(c.magnitude * 1920, Is.EqualTo(UnityTuning.TravelBlurClamp * 1920).Within(0.01), "clamped to 1.8 % of the width (34.6 px)");
            Vector2 small = new Vector2(0.002f, 0.001f);
            Assert.That(B.Clamp(small, 16f / 9f, UnityTuning.TravelBlurClamp), Is.EqualTo(small), "a short smear is left alone");
            Vector2 tall = B.Clamp(new Vector2(0, 0.1f), 16f / 9f, UnityTuning.TravelBlurClamp);
            Assert.That(tall.y / (16f / 9f), Is.EqualTo(UnityTuning.TravelBlurClamp).Within(1e-6), "the height counted in widths");
        }
    }
}
