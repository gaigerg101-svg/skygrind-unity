// NIGHT74 builder L (leftover 4; Game/Ice74.cs). Pinned:
//   1. the arena's lift numbers are lit72's, stronger where the walls need it: A at least lit72's, l1 at least lit72's (a lit pixel is left alone), a gain cap over lit72's 9 and sized so a pixel at display luma 0.010
//      (THE ICEBREAKER's walls before the lift) is scaled to over 30 levels without the cap binding
//   2. PostVectorOf: not live -> lit72's vector exactly (the row off is lit72 as it was); live with lit72 live -> the larger of each; live with lit72 off -> the arena's own numbers (the shader's block runs)
//   3. Inside: the arena's centre is inside, a point past the rim walls plus the margin is not, nor one far above its height or far under its floor (a closed box, the entry gap's threshold counts)
//   4. the tune switch: a number scales A and the cap, clamped to 0.2 .. 4; no switch is 1
//   5. the fades are under a second
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Ice74Tests
    {
        [Test]
        public void ArenaNumbers_AreLit72sOrStronger_AndReachTheWallsOver30()
        {
            Assert.GreaterOrEqual(UnityTuning.N74IceA, UnityTuning.L72LitA);
            Assert.GreaterOrEqual(UnityTuning.N74IceL1, UnityTuning.L72LitL1);
            Assert.Greater(UnityTuning.N74IceCap, UnityTuning.L72LitKMax);
            // a wall pixel at display luma 0.010: scaled by (p + A) / p, under the cap, to a luma over 30 / 255
            float p = 0.010f, k = (p + UnityTuning.N74IceA) / p;
            Assert.Less(k, UnityTuning.N74IceCap, "the cap does not bind on the walls: they take the whole A");
            Assert.Greater(p * k * 255f, 30f);
            // and lit72's own numbers leave it under 30: that is what the arena's lift is for
            Assert.Less(p * Mathf.Min(UnityTuning.L72LitKMax, (p + UnityTuning.L72LitA) / p) * 255f, 30f);
        }

        [Test]
        public void PostVectorOf_IsLit72sWhenNotLive_TheLargerWhenLive_AndTheArenasOwnWhenLit72IsOff()
        {
            var lit = new Vector4(UnityTuning.L72LitA, UnityTuning.L72LitL0, UnityTuning.L72LitL1, UnityTuning.L72LitKMax);
            Assert.AreEqual(lit, Ice74View.PostVectorOf(lit, false, 1f, 1f), "not live: lit72's vector exactly");
            Assert.AreEqual(Vector4.zero, Ice74View.PostVectorOf(Vector4.zero, false, 1f, 1f));
            Vector4 on = Ice74View.PostVectorOf(lit, true, 1f, 1f);
            Assert.AreEqual(UnityTuning.N74IceA, on.x, 1e-6f);
            Assert.AreEqual(UnityTuning.L72LitL0, on.y, 1e-6f);
            Assert.GreaterOrEqual(on.z, UnityTuning.L72LitL1);
            Assert.AreEqual(UnityTuning.N74IceCap, on.w, 1e-6f);
            Vector4 half = Ice74View.PostVectorOf(lit, true, 0.5f, 1f);
            Assert.AreEqual(Mathf.Max(UnityTuning.L72LitA, UnityTuning.N74IceA * 0.5f), half.x, 1e-6f, "the weight scales A, never under lit72's own");
            Vector4 alone = Ice74View.PostVectorOf(Vector4.zero, true, 1f, 1f);
            Assert.AreEqual(UnityTuning.N74IceA, alone.x, 1e-6f);
            Assert.AreEqual(UnityTuning.N74IceL0, alone.y, 1e-6f, "lit72 off: the arena's own l0");
            Assert.Greater(alone.z, alone.y);
            Assert.AreEqual(UnityTuning.N74IceCap, alone.w, 1e-6f);
            Vector4 scaled = Ice74View.PostVectorOf(lit, true, 1f, 2f);
            Assert.AreEqual(UnityTuning.N74IceA * 2f, scaled.x, 1e-6f);
            Assert.AreEqual(UnityTuning.N74IceCap * 2f, scaled.w, 1e-6f);
        }

        [Test]
        public void Inside_IsTheClosedBox()
        {
            // THE ICEBREAKER's arena: half 18, height 8, centre at (100, 5, -40) on some level
            const double cx = 100, cy = 5, cz = -40, half = 18, height = 8, m = 0.5;
            Assert.IsTrue(Ice74View.Inside(cx, cy + 1, cz, cx, cy, cz, half, height, m), "the middle");
            Assert.IsTrue(Ice74View.Inside(cx + half + 0.4, cy, cz, cx, cy, cz, half, height, m), "on the rim wall's inner threshold");
            Assert.IsFalse(Ice74View.Inside(cx + half + 0.6, cy, cz, cx, cy, cz, half, height, m), "past the margin");
            Assert.IsFalse(Ice74View.Inside(cx, cy, cz - half - 3, cx, cy, cz, half, height, m), "the corridor outside");
            Assert.IsFalse(Ice74View.Inside(cx, cy + height + 5, cz, cx, cy, cz, half, height, m), "far above");
            Assert.IsFalse(Ice74View.Inside(cx, cy - 6, cz, cx, cy, cz, half, height, m), "far under the floor");
        }

        [Test]
        public void TuneSwitch_ScalesAndClamps()
        {
            Assert.AreEqual(1f, Ice74View.ReadScale(null), 1e-6f);
            Assert.AreEqual(1f, Ice74View.ReadScale(new[] { "SKYGRIND.exe" }), 1e-6f);
            Assert.AreEqual(1.5f, Ice74View.ReadScale(new[] { "-skygrindIce74Tune=1.5" }), 1e-6f);
            Assert.AreEqual(2f, Ice74View.ReadScale(new[] { "-skygrindIce74Tune", "2" }), 1e-6f);
            Assert.AreEqual(4f, Ice74View.ReadScale(new[] { "-skygrindIce74Tune=9" }), 1e-6f);
            Assert.AreEqual(0.2f, Ice74View.ReadScale(new[] { "-skygrindIce74Tune=0" }), 1e-6f);
        }

        [Test]
        public void Fades_AreUnderASecond()
        {
            Assert.Less(UnityTuning.N74IceFadeInS, 1f);
            Assert.Less(UnityTuning.N74IceFadeOutS, 1f);
        }
    }
}
