// NIGHT33 builder U (design.n33.hud33): the HUD33 layout's rules on its own boxes (Hud33Layout), and the checker caught red on planted faults.
using System.Collections.Generic;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Hud33LayoutTests
    {
        [TestCase(1920, 1080)]
        [TestCase(1280, 720)]
        [TestCase(2560, 1440)]
        [TestCase(1440, 1080)] // 4:3: the top-centre column narrows clear of the round line (round 1's checker found the overlap)
        [TestCase(1680, 1050)] // 16:10
        [TestCase(2560, 1080)] // 21:9
        public void TheLayout_PassesEveryRule_AtRestSwitchingAndInABossFight(int w, int h)
        {
            foreach (bool switching in new[] { false, true })
                foreach (bool boss in new[] { false, true })
                {
                    List<string> bad = Hud33Layout.CheckLayout(w, h, 5, switching, boss);
                    Assert.That(bad, Is.Empty, $"{w}x{h} switching {switching} boss {boss}: " + string.Join(" ;; ", bad));
                }
        }

        [Test]
        public void The720pCanvas_IsThe1080pCanvas()
        {
            Assert.That(Hud33Layout.CanvasW(1280, 720), Is.EqualTo(1920).Within(1e-9));
            Assert.That(Hud33Layout.CanvasW(1920, 1080), Is.EqualTo(1920).Within(1e-9));
        }

        [Test]
        public void TheTypeHierarchy_HasThreeSizes_AndNoneUnder16()
        {
            var sizes = new HashSet<double>();
            foreach (Hud33Layout.Box b in Hud33Layout.All(5, true, true))
                if (b.Text)
                {
                    Assert.That(b.px, Is.GreaterThanOrEqualTo(16), b.name);
                    sizes.Add(b.px);
                }
            Assert.That(sizes.Count, Is.GreaterThanOrEqualTo(3));
            Assert.That(Hud33Layout.HealthNumber.px, Is.EqualTo(64));
            Assert.That(Hud33Layout.WeaponName.px, Is.EqualTo(24));
            Assert.That(Hud33Layout.LtLabel.px, Is.EqualTo(16));
        }

        static Hud33Layout.Item T(string name, string text, double x, double y, double w, double h, double px) =>
            new Hud33Layout.Item { name = name, text = text, x = x, y = y, w = w, h = h, px = px };

        [Test]
        public void TheChecker_CatchesAnOverlap_ASmallText_AMargin_AWideBox_AndARepeat()
        {
            var items = new List<Hud33Layout.Item>
            {
                T("Banner", "DOOR OPEN", 800, 150, 300, 32, 24),
                T("Cue", "7 m AHEAD", 820, 160, 200, 32, 24),          // overlaps the banner
                T("Tiny", "x1.2", 600, 600, 40, 14, 12),                 // under 16 px
                T("Corner", "R4", 20, 60, 60, 24, 20),                   // inside the 54 px edge
                T("Card", "KESTREL", 1300, 900, 470, 40, 24),           // wider than 360
                T("Row", "ZOOM", 1300, 960, 60, 20, 16),
                T("Row2", "ZOOM", 1400, 960, 60, 20, 16),               // the LT word twice
            };
            List<string> bad = Hud33Layout.Check(items, 1920, 1080);
            string all = string.Join(" ;; ", bad);
            Assert.That(all, Does.Contain("overlap: Banner"));
            Assert.That(all, Does.Contain("small text: Tiny"));
            Assert.That(all, Does.Contain("margin: Corner"));
            Assert.That(all, Does.Contain("wide: Card"));
            Assert.That(all, Does.Contain("repeated label: 'ZOOM'"));
        }

        [Test]
        public void ALabelAndItsShadowTwin_AreOneLabel_AndTheBossBarMayBe640()
        {
            var items = new List<Hud33Layout.Item>
            {
                T("Weapon", "KESTREL", 1500, 950, 300, 30, 24),
                T("Weapon/Shadow", "KESTREL", 1502, 952, 300, 30, 24),
                new Hud33Layout.Item { name = Hud33Layout.BossBarName, x = 640, y = 80, w = 640, h = 6, bossBar = true },
            };
            Assert.That(Hud33Layout.Check(items, 1920, 1080), Is.Empty);
        }
    }
}
