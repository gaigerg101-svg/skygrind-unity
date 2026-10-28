// NIGHT35 builder V (design.n35.look35): the PS2 pass's pure rules (Game/Look30Math.cs Look35Math; Shaders/Resources/Crt29.shader draws the lines and the split).
using NUnit.Framework;
using M = Skygrind.Game.Look35Math;

namespace Skygrind.Game.Tests
{
    public class Look35Tests
    {
        [Test]
        public void TheLinesAre480AtFullHdAndFewerOnSmallScreens()
        {
            Assert.That(M.Lines(UnityTuning.Look35Lines, 1080, 0), Is.EqualTo(480f));
            Assert.That(M.Lines(UnityTuning.Look35Lines, 1440, 0), Is.EqualTo(480f));
            Assert.That(M.Lines(UnityTuning.Look35Lines, 720, 0), Is.EqualTo(480f)); // 1.5 px blocks
            Assert.That(M.Lines(UnityTuning.Look35Lines, 600, 0), Is.EqualTo(400f)); // never under 1.5 px a block
            Assert.That(M.Lines(UnityTuning.Look35Lines, 1080, 4), Is.EqualTo(270f)); // PIXEL SIZE 4 asks for bigger blocks
            Assert.That(M.Lines(UnityTuning.Look35Lines, 0, 0), Is.EqualTo(0f));
        }

        [Test]
        public void TheFlatSplitIsOneVirtualPixel()
        {
            float uv = M.FlatChromaUv(1f, 480f, 16.0 / 9.0);
            Assert.That(uv * 1920f, Is.EqualTo(1920f / (480f * 16f / 9f)).Within(1e-3f)); // 2.25 screen px at 1080p
            Assert.That(M.FlatChromaUv(1f, 0f, 1.7), Is.EqualTo(0f));
        }

        [Test]
        public void TheCrtSettingScalesAroundItsDefault()
        {
            Assert.That(M.Gain(0.35f, 0.35), Is.EqualTo(1f).Within(1e-6f));
            Assert.That(M.Gain(0f, 0.35), Is.EqualTo(0f));
            Assert.That(M.Gain(1f, 0.35), Is.EqualTo(UnityTuning.Look35GainMax));
            Assert.That(M.Gain(0.175f, 0.35), Is.EqualTo(0.5f).Within(1e-6f));
        }

        [Test]
        public void TheHudSpringOvershootsAndSettles()
        {
            float x = 0, v = UnityTuning.Tv35HitKickPx * UnityTuning.Tv35SpringHz * 6.2831853f, w = UnityTuning.Tv35SpringHz * 6.2831853f, peak = 0, under = 0;
            for (int i = 0; i < 240; i++) // 1 s at 240 Hz
            {
                Tv35Math.Spring(ref x, ref v, w, UnityTuning.Tv35SpringDamping, 1f / 240f);
                peak = System.Math.Max(peak, x);
                under = System.Math.Min(under, x);
            }
            Assert.That(peak, Is.InRange(4f, UnityTuning.Tv35MaxPx), "the first swing reads (px)");
            Assert.That(under, Is.LessThan(-1f), "it overshoots back (bouncy)");
            Assert.That(System.Math.Abs(x), Is.LessThan(0.5f), "settled within a second");
        }

        [Test]
        public void TheBounceOnlyEndsAtRestAndTheRideLookEases()
        {
            Assert.That(Tv35Math.Bounce(0f, UnityTuning.Tv35BounceS, UnityTuning.Tv35BouncePop), Is.EqualTo(1f + UnityTuning.Tv35BouncePop).Within(1e-5f));
            Assert.That(Tv35Math.Bounce(UnityTuning.Tv35BounceS, UnityTuning.Tv35BounceS, UnityTuning.Tv35BouncePop), Is.EqualTo(1f));
            float f = 0;
            for (int i = 0; i < 7; i++) f = Ride35Math.Ease(f, true, 1f / 60f);
            Assert.That(f, Is.EqualTo(1f), "full focus within 0.12 s");
            for (int i = 0; i < 12; i++) f = Ride35Math.Ease(f, false, 1f / 60f);
            Assert.That(f, Is.EqualTo(0f), "gone within 0.2 s");
            Ride35Math.Glitch(1f, 0.5, true, out float tear, out _, out float sweep, out float y);
            Assert.That(tear, Is.EqualTo(0f), "REDUCED MOTION: no tear");
            Assert.That(sweep, Is.GreaterThan(0f));
        }

        [Test]
        public void ThirtyTwoLevelsKeepThePaletteApart()
        {
            float gap = Look30Math.PaletteMinGap(UnityTuning.Look35Levels, UnityTuning.Look35QuantStrength);
            Assert.That(gap, Is.GreaterThanOrEqualTo(UnityTuning.Look30PaletteMinGap), $"the closest palette pair at 32 levels, full strength: {gap}");
        }
    }
}
