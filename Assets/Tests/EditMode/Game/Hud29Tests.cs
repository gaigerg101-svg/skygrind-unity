// NIGHT29 item 3 (builder V): the boss gap bar's pure rules (Game/Hud29.cs): the fill share, the label, the defaults that hide it.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Hud29Tests
    {
        [Test]
        public void TheBarHidesByDefaultAndFillsByShare()
        {
            Hud29.Reset();
            Assert.That(Hud29.BossGapMetres, Is.LessThan(0), "no boss: the bar is hidden");
            Assert.That(Hud29.DoorsOpenTag, Is.False);
            Assert.That(Hud29.FillShare(25, 50), Is.EqualTo(0.5f).Within(1e-6));
            Assert.That(Hud29.FillShare(80, 50), Is.EqualTo(1f), "a gap past the scale: full");
            Assert.That(Hud29.FillShare(0, 50), Is.EqualTo(0f));
            Assert.That(Hud29.FillShare(-3, 50), Is.EqualTo(0f));
            Assert.That(Hud29.FillShare(UnityTuning.Hud29GapFallbackMax / 2, 0), Is.EqualTo(0.5f).Within(1e-6), "no scale set: the fallback");
        }

        [Test]
        public void TheLabelRoundsToWholeMetres()
        {
            Assert.That(Hud29.Label(22.6, -1), Is.EqualTo("GAP 23 M"));
            Assert.That(Hud29.Label(22.6, 15), Is.EqualTo("GAP 23 M   HURT AT 15 M"));
        }
    }
}
