// NIGHT25 (builder F): the HUD text overlap check the reels `cue` and `lit` run on their telemetry.
using System.Collections.Generic;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class HudOverlap25Tests
    {
        static HudOverlap25.Item T(string path, string text, double x, double y, double w, double h, double a = 1) =>
            new HudOverlap25.Item { path = path, text = text, x = x, y = y, w = w, h = h, alpha = a };

        [Test]
        public void TheCueOnTheRoomBanner_IsAnOverlap()
        {
            var items = new List<HudOverlap25.Item>
            {
                T("UI/HUD values/Door cue 24/Way", "36 m · TURN RIGHT", 840, 185, 240, 32),
                T("UI/HUD values/Banner", "3.3 THE NEST", 860, 190, 200, 40),
            };
            Assert.That(HudOverlap25.Overlaps(items).Count, Is.EqualTo(1));
        }

        [Test]
        public void SeparateLines_AParentChildPair_AnOffsetCopy_AndAFadedLine_AreNot()
        {
            var items = new List<HudOverlap25.Item>
            {
                T("UI/HUD values/Door cue 24/Head", "DOOR OPEN", 840, 150, 240, 32),
                T("UI/HUD values/Door cue 24/Way", "36 m", 840, 182, 240, 32),
                T("UI/HUD values/Banner", "3.3 THE NEST", 860, 250, 200, 40),
                T("UI/HUD values/Clear", "ROUND 4 CLEAR", 100, 100, 300, 60),
                T("UI/HUD values/Clear (offset)", "ROUND 4 CLEAR", 104, 104, 300, 60),
                T("UI/HUD values/Faded", "GHOST", 840, 150, 240, 32, 0.1),
                T("UI/HUD values/Door cue 24", "X", 840, 150, 240, 64),
            };
            Assert.That(HudOverlap25.Overlaps(items), Is.Empty);
        }
    }
}
