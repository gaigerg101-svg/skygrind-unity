// NIGHT74 builder L (L3; Game/Crown74.cs and Assets/Shaders/Resources/Ceiling27.shader's _Crown74 block). Pinned:
//   1. the fill is cool and not a danger hue: blue leads, red is never over green, its hue is in the blue-violet band (200 - 270 degrees), outside cyan's 170 - 200 and the danger band 345 - 40 (CLAUDE.md rule 8)
//   2. the shape numbers: the whole-fill luma is under the part-fill luma, both inside 0-1; the fades are under a second
//   3. the tune switch: a number scales the fill, clamped to 0.2 .. 3; no switch is 1
//   4. the shader declares the two globals the C# side sets by the same names, and skips its block at weight 0 (so the row OFF leaves the ceiling as it was)
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Crown74Tests
    {
        [Test]
        public void Fill_IsCool_AndNotADangerOrCyanHue()
        {
            float r = UnityTuning.N74CrownFillR, g = UnityTuning.N74CrownFillG, b = UnityTuning.N74CrownFillB;
            Assert.LessOrEqual(r, g, "red never over green");
            Assert.Greater(b, g, "blue leads");
            Assert.Greater(b, r + 0.05f);
            Color.RGBToHSV(new Color(r, g, b), out float h, out float s, out _);
            float deg = h * 360f;
            Assert.GreaterOrEqual(deg, 200f, "past cyan");
            Assert.LessOrEqual(deg, 270f, "not magenta (the RIVAL's)");
            Assert.Less(s, 0.5f, "a slate, not a saturated blue");
        }

        [Test]
        public void Shape_AndFades_AreSane()
        {
            Assert.Greater(UnityTuning.N74CrownL0, 0.118f, "the whole fill reaches past the 'dark < 30' line");
            Assert.Less(UnityTuning.N74CrownL0, UnityTuning.N74CrownL1);
            Assert.LessOrEqual(UnityTuning.N74CrownL1, 1f);
            Assert.Less(UnityTuning.N74CrownFadeInS, 1f);
            Assert.Less(UnityTuning.N74CrownFadeOutS, 1f);
        }

        [Test]
        public void TuneSwitch_ScalesAndClamps()
        {
            Assert.AreEqual(1f, Crown74View.ReadScale(null), 1e-6f);
            Assert.AreEqual(1f, Crown74View.ReadScale(new[] { "SKYGRIND.exe" }), 1e-6f);
            Assert.AreEqual(1.5f, Crown74View.ReadScale(new[] { "-skygrindCrown74Tune=1.5" }), 1e-6f);
            Assert.AreEqual(2f, Crown74View.ReadScale(new[] { "-skygrindCrown74Tune", "2" }), 1e-6f);
            Assert.AreEqual(3f, Crown74View.ReadScale(new[] { "-skygrindCrown74Tune=9" }), 1e-6f);
            Assert.AreEqual(0.2f, Crown74View.ReadScale(new[] { "-skygrindCrown74Tune=0" }), 1e-6f);
        }

        [Test]
        public void Ceiling27Shader_DeclaresTheGlobals_AndSkipsItsBlockAtWeightZero()
        {
            string text = File.ReadAllText(Path.Combine(Application.dataPath, "Shaders/Resources/Ceiling27.shader"));
            StringAssert.Contains("float4 _Crown74;", text);
            StringAssert.Contains("float4 _Crown74B;", text);
            StringAssert.Contains("if (_Crown74.a > 0.0)", text);
            // the retake: the arena's stone takes the same block, by the same global names
            string stone = File.ReadAllText(Path.Combine(Application.dataPath, "Shaders/Stone.shader"));
            StringAssert.Contains("float4 _Crown74;", stone);
            StringAssert.Contains("float4 _Crown74B;", stone);
            StringAssert.Contains("if (_Crown74.a > 0.0)", stone);
        }
    }
}
