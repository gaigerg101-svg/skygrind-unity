// NIGHT30 builder V (design.n30.hud30, P0 e): the HUD layout check's pure part (Game/Hud30Layout.cs: Overlaps, KindOfName).
using System.Collections.Generic;
using NUnit.Framework;
using L = Skygrind.Game.Hud30Layout;

namespace Skygrind.Game.Tests
{
    public class Hud30Tests
    {
        static L.Item T(string path, string text, double x, double y, double w, double h) => new L.Item { path = path, text = text, kind = L.Kind.Text, x = x, y = y, w = w, h = h, alpha = 1 };
        static L.Item K(string path, L.Kind kind, double x, double y, double w, double h) => new L.Item { path = path, kind = kind, x = x, y = y, w = w, h = h, alpha = 1 };

        [Test]
        public void ContinueNamesTheNextLevelWithoutItsNumber()
        {
            Assert.That(Ui.LevelName30("LEVEL 4 THE SPAN"), Is.EqualTo("THE SPAN"));
            Assert.That(Ui.LevelName30("LEVEL 10 THE END"), Is.EqualTo("THE END"));
            Assert.That(Ui.LevelName30("THE ARENA"), Is.EqualTo("THE ARENA"));
            Assert.That(Ui.LevelName30(null), Is.EqualTo(""));
        }

        [Test]
        public void TheLayoutCheckFindsNight29sThreeKnownCollisions()
        {
            var items = new List<L.Item>
            {
                // "PULSE READY" over the hotbar's silhouette and the role header (shot 170047)
                T("UI/HUD values/Pulse glove 27/Words", "PULSE READY", 1500, 230, 160, 26),
                K("UI/HUD values/Weapon block/Hotbar/Hotbar cell 0/Silhouette", L.Kind.Icon, 1560, 232, 40, 20),
                T("UI/HUD values/Weapon block/Weapon role", "TRIGGER", 1480, 254, 110, 22),
                // "x1.2 AIR" over the rank plate (shot 170227)
                K("UI/HUD values/Style/Multiplier chip", L.Kind.Chip, 1160, 960, 150, 40),
                K("UI/HUD values/Style/Block plate", L.Kind.Plate, 1290, 900, 420, 110),
            };
            List<string> found = L.Overlaps(items);
            Assert.That(found.Count, Is.EqualTo(3), string.Join(" ;; ", found));
        }

        [Test]
        public void TheLayoutCheckIgnoresOneElementsPartsAndFaintOrEmptyTexts()
        {
            var items = new List<L.Item>
            {
                T("A/Label", "x", 0, 0, 50, 20),
                T("A/Label (offset)", "x", 2, -2, 50, 20), // its shadow twin
                T("A/Label/Inner", "y", 5, 5, 20, 10), // its child
                T("B/Faint", "z", 0, 0, 50, 20),
                T("C/Empty", "", 0, 0, 50, 20),
                K("D/Multiplier chip", L.Kind.Chip, 0, 0, 50, 20),
                K("D/Multiplier chip/Plate", L.Kind.Plate, 0, 0, 50, 20), // the chip's own plate
                T("E/Far", "w", 300, 300, 50, 20),
                K("F/Silhouette", L.Kind.Icon, 300, 300, 2, 2), // a 4 px2 touch: under the minimum area
            };
            var faint = items[3];
            faint.alpha = 0.1;
            items[3] = faint;
            List<string> found = L.Overlaps(items);
            Assert.That(found, Is.Empty, string.Join(" ;; ", found));
            Assert.That(L.KindOfName("Silhouette"), Is.EqualTo(L.Kind.Icon));
            Assert.That(L.KindOfName("Multiplier chip"), Is.EqualTo(L.Kind.Chip));
            Assert.That(L.KindOfName("Power chip refund"), Is.EqualTo(L.Kind.Chip));
            Assert.That(L.KindOfName("Block plate"), Is.EqualTo(L.Kind.Plate));
            Assert.That(L.KindOfName("Word"), Is.Null);
        }
    }
}
