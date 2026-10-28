// NIGHT34 builder H (design.n34.rail34): THE ARSENAL RAIL's boxes pass HUD33's rules (Hud33Layout.Check) on every screen shape, with 1..6 guns owned,
// at rest and in a boss fight; the rail stays inside HUD33's 360 px rule at its widest.
using System.Collections.Generic;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Rail34LayoutTests
    {
        [TestCase(1920, 1080)]
        [TestCase(1280, 720)]
        [TestCase(2560, 1440)]
        [TestCase(1440, 1080)]
        [TestCase(1680, 1050)]
        [TestCase(2560, 1080)]
        public void TheRailLayout_PassesEveryHud33Rule(int w, int h)
        {
            double cw = Hud33Layout.CanvasW(w, h);
            for (int owned = 1; owned <= 6; owned++)
                foreach (bool boss in new[] { false, true })
                {
                    var items = new List<Hud33Layout.Item>();
                    foreach (Hud33Layout.Box b in Hud33Layout.AllRail34(owned, boss, cw)) items.Add(Hud33Layout.ToItem(b, cw, Hud33Layout.RefH));
                    List<string> bad = Hud33Layout.Check(items, cw, Hud33Layout.RefH);
                    Assert.That(bad, Is.Empty, $"{w}x{h} owned {owned} boss {boss}: " + string.Join(" ;; ", bad));
                }
        }

        [Test]
        public void TheRail_IsOneRowUnder360Px_AndItsTextsAre16And24()
        {
            Hud33Layout.Box r = Hud33Layout.Rail(99);
            Assert.That(r.w, Is.LessThanOrEqualTo(UnityTuning.Hud33MaxWidth));
            Assert.That(r.h, Is.LessThanOrEqualTo(UnityTuning.Hud33MaxHeight));
            Assert.That(Hud33Layout.RailLt.px, Is.EqualTo(16));
            Assert.That(Hud33Layout.RailWeaponName.px, Is.EqualTo(24));
            Assert.That(Hud33Layout.RailLt.y, Is.GreaterThanOrEqualTo(r.y + r.h)); // the LT label stands over the rail, never on it
        }
    }
}
