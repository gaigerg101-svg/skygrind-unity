// NIGHT36 builder M (design.n36.text36): the teaching slot's box passes the hud33 rules with every other HUD box, sits in the lower middle (about 62 %
// down), is 32 px at 1080p and is apart from the top slot (the room name).
using System.Collections.Generic;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Text36LayoutTests
    {
        [TestCase(1920, 1080)]
        [TestCase(1280, 720)]
        [TestCase(2560, 1440)]
        [TestCase(1440, 1080)]
        [TestCase(1680, 1050)]
        [TestCase(2560, 1080)]
        public void TheTeachSlot_PassesEveryRule_WithTheWholeHud(int w, int h)
        {
            foreach (bool switching in new[] { false, true })
                foreach (bool boss in new[] { false, true })
                {
                    List<string> bad = Hud33Layout.CheckLayout(w, h, 5, switching, boss, true);
                    Assert.That(bad, Is.Empty, $"{w}x{h} switching {switching} boss {boss}: " + string.Join(" ;; ", bad));
                }
        }

        [Test]
        public void TheTeachSlot_IsLowerMiddle_32px_AndApartFromTheTopSlot()
        {
            Hud33Layout.Item t = Hud33Layout.ToItem(Hud33Layout.TeachSlot(), 1920, 1080);
            Hud33Layout.Item top = Hud33Layout.ToItem(Hud33Layout.Slot(false), 1920, 1080);
            double centre = (t.y + t.h / 2) / 1080;
            Assert.That(centre, Is.EqualTo(0.62).Within(0.01));
            Assert.That(t.px, Is.EqualTo(32));
            Assert.That(t.slot, Is.True, "the 640 px column is allowed for the teaching slot");
            Assert.That(t.x + t.w / 2, Is.EqualTo(960).Within(0.5), "centred");
            Assert.That(t.y, Is.GreaterThan(540), "under the crosshair's line");
            Assert.That(t.y, Is.GreaterThan(top.y + top.h + 400), "far from the top slot");
        }

        [Test]
        public void TheChecker_StillCatchesATeachSlotPlacedOverTheTopSlot()
        {
            Hud33Layout.Item top = Hud33Layout.ToItem(Hud33Layout.Slot(false), 1920, 1080, "1.1 INTAKE");
            Hud33Layout.Item t = Hud33Layout.ToItem(Hud33Layout.TeachSlot(), 1920, 1080, "HOLD RT to CHARGE");
            t.y = top.y; // planted: the old place
            List<string> bad = Hud33Layout.Check(new List<Hud33Layout.Item> { top, t }, 1920, 1080);
            Assert.That(string.Join(" ;; ", bad), Does.Contain("overlap: Message slot"));
        }
    }
}
