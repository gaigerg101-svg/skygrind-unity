// NIGHT60 builder V (B2, kill55's marks near-black; design.n55.kill55): the marks' draw order against the enemy x-ray. Pinned:
//   1. the Kill59Mark materials draw AFTER the x-ray material's queue (Transparent + UnityTuning.EnemyXrayQueueOffset): the x-ray's ZTest
//      Greater pass lays aim.xray.color at alpha 0.35 over anything drawn before it in front of the body, and the marks stand in front of it
//      (Tools/night/n60-v-mark.py: under the x-ray the hole #070303 reads #A02B0F, V 0.63; the frame's mark pixels V 0.53; the reel's MAIN
//      counts V < 0.12)
//   2. a mark keeps the colour it was given (_BaseColor, linear) and stays opaque
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game.Tests
{
    public class N60MarkTests
    {
        [Test]
        public void Marks_DrawAfterTheEnemyXray_InTheirOwnColour()
        {
            Shader s = Shader.Find("SKYGRIND/Kill59Mark");
            Assert.That(s, Is.Not.Null, "SKYGRIND/Kill59Mark (Assets/Shaders/Resources)");
            Color c = Look.Hex(UnityTuning.Kill55HoleHex).linear;
            RenderParams rp = Kill55Damage.Params59(s, c);
            try
            {
                int xray = (int)RenderQueue.Transparent + UnityTuning.EnemyXrayQueueOffset;
                Assert.That(rp.material.renderQueue, Is.GreaterThan(xray), "the marks draw after the enemy x-ray (its ZTest Greater pass tints whatever stands in front of the body)");
                Vector4 got = rp.material.GetVector("_BaseColor");
                Assert.That(got.x, Is.EqualTo(c.r).Within(1e-6f));
                Assert.That(got.y, Is.EqualTo(c.g).Within(1e-6f));
                Assert.That(got.z, Is.EqualTo(c.b).Within(1e-6f));
                Assert.That(got.w, Is.EqualTo(1f), "opaque");
            }
            finally
            {
                Object.DestroyImmediate(rp.material);
            }
        }
    }
}
