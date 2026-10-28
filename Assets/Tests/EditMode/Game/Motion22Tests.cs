// NIGHT22 T-12 (builder S): the shared HUD entrance (HudEntrance18) read the way feel-metrics M9 reads it: from the first drawn
// frame to the first frame that is within 3 % of rest scale at 95 % of rest alpha and stays so for 6 frames, at 60 fps. The
// Player reel (Logs/feel/n22c1dusk2) measured 83 ms for Round mark / Numeral / Round label / EXPOSED, under M9's 90 ms floor;
// Hud18EnterTime 0.15 -> 0.20 moves them into 90-300 ms. Seen red once with 0.15 (the arithmetic gave 83-100 ms).
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Motion22Tests
    {
        [Test]
        public void TheSharedEntrance_SettlesIn90To300Ms_ByM9sRule_At60Fps()
        {
            const double fps = 60;
            int n = 60;
            var ok = new bool[n];
            for (int k = 0; k < n; k++)
            {
                double age = k == 0 ? HudEntrance18.FirstAge(1 / fps) : HudEntrance18.FirstAge(1 / fps) + k / fps;
                ok[k] = System.Math.Abs(HudEntrance18.Scale(age) - 1) <= 0.03 && HudEntrance18.Alpha(age) >= 0.95;
            }
            int settle = -1;
            for (int j = 0; j + 6 <= n && settle < 0; j++)
            {
                bool all = true;
                for (int i = j; i < j + 6; i++) all &= ok[i];
                if (all) settle = j;
            }
            Assert.That(settle, Is.GreaterThanOrEqualTo(0), "it settles");
            double ms = settle * 1000.0 / fps;
            Assert.That(ms, Is.InRange(90, 300));
        }
    }
}
