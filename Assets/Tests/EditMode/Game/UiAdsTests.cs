// NIGHT16 polish #36 (builder V): the ADS HUD's pure maths (UiAdsMath: hud.js setAds :756-770, main.js updateAdsHud's gate,
// the scope's gradient :77, the ring's texture, the range label's tenths).
using System;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class UiAdsTests
    {
        const double Eps = 1e-12;

        [Test]
        public void TheRingFadesInAndShrinksForARingGun()
        {
            UiAdsLook l = UiAdsMath.Look(0.5, true, false, true, false);
            Assert.That(l.ringAlpha, Is.EqualTo(0.5).Within(Eps));
            Assert.That(l.ringScale, Is.EqualTo(1.6 - 0.6 * 0.5).Within(Eps), "scale(1.6 - 0.6 v)");
            Assert.That(l.scopeAlpha, Is.EqualTo(0));
            Assert.That(l.tickAlpha, Is.EqualTo(1 - 0.6 * 0.5).Within(Eps), "--tick: 1 - 0.6 k");
            Assert.That(l.showRange, Is.False, "a ring gun has no rangefinder");
            UiAdsLook full = UiAdsMath.Look(1, true, false, true, true);
            Assert.That(full.ringAlpha, Is.EqualTo(1));
            Assert.That(full.ringScale, Is.EqualTo(1).Within(Eps), "full zoom: the ring at 30 px");
            Assert.That(full.tickAlpha, Is.EqualTo(0.4).Within(Eps));
            UiAdsLook two = UiAdsMath.Look(0.123456, true, false, true, false);
            Assert.That(two.ringAlpha, Is.EqualTo(0.12).Within(Eps), "toFixed(2), as the web writes it");
            Assert.That(two.ringScale, Is.EqualTo(1.6 - 0.6 * 0.12).Within(Eps), "the scale from the written opacity");
        }

        [Test]
        public void TheScopeTakesOverForTheLongwave()
        {
            UiAdsLook l = UiAdsMath.Look(0.8, true, true, true, true);
            Assert.That(l.ringAlpha, Is.EqualTo(0), "no ring under a scope");
            Assert.That(l.scopeAlpha, Is.EqualTo(0.8).Within(Eps));
            Assert.That(l.tickAlpha, Is.EqualTo(1 - 0.6 * 0.8).Within(Eps));
            Assert.That(l.showRange, Is.True, "scoped, over half way, a hit: the range");
            Assert.That(UiAdsMath.Look(0.5, true, true, true, true).showRange, Is.False, "k > 0.5, not at it");
            Assert.That(UiAdsMath.Look(0.9, true, true, true, false).showRange, Is.False, "no hit: no range");
        }

        [Test]
        public void UnzoomedOrNotPlayingDrawsNothing()
        {
            foreach (UiAdsLook l in new[]
            {
                UiAdsMath.Look(0, true, false, true, false), // unzoomed
                UiAdsMath.Look(0.7, false, false, true, false), // a gun with no ads entry
                UiAdsMath.Look(0.7, true, true, false, true), // not playing (a menu, DOWN, the attract)
            })
            {
                Assert.That(l.ringAlpha, Is.EqualTo(0));
                Assert.That(l.scopeAlpha, Is.EqualTo(0));
                Assert.That(l.tickAlpha, Is.EqualTo(1), "the arms back at full");
                Assert.That(l.showRange, Is.False);
            }
        }

        [Test]
        public void TheScopeGradientIsTheWebsRadialGradient()
        {
            Assert.That(UiAdsMath.VignetteAlpha(0, 0.92), Is.EqualTo(0));
            Assert.That(UiAdsMath.VignetteAlpha(31, 0.92), Is.EqualTo(0), "clear to 31vh");
            Assert.That(UiAdsMath.VignetteAlpha(32, 0.92), Is.EqualTo(0.275).Within(Eps), "half way to 0.55 at 33vh");
            Assert.That(UiAdsMath.VignetteAlpha(33, 0.92), Is.EqualTo(0.55).Within(Eps));
            Assert.That(UiAdsMath.VignetteAlpha(39, 0.92), Is.EqualTo(0.55 + (0.92 - 0.55) * 0.5).Within(Eps));
            Assert.That(UiAdsMath.VignetteAlpha(45, 0.92), Is.EqualTo(0.92).Within(Eps));
            Assert.That(UiAdsMath.VignetteAlpha(80, 0.92), Is.EqualTo(0.92).Within(Eps), "and beyond");
            Assert.That(UiAdsMath.VignetteAlpha(80, 0.5), Is.EqualTo(0.5).Within(Eps), "ads.scopeVignette");
        }

        [Test]
        public void TheRangeLabelChangesByTenths()
        {
            Assert.That(UiAdsMath.RangeTenths(12.34), Is.EqualTo(123));
            Assert.That(UiAdsMath.RangeTenths(12.36), Is.EqualTo(124));
            Assert.That(UiAdsMath.RangeTenths(0), Is.EqualTo(0));
            Assert.That(UiAdsMath.RangeTenths(249.99), Is.EqualTo(2500));
        }

        [Test]
        public void TheRingTextureCoversItsAnnulus()
        {
            Assert.That(UiAdsMath.RingCoverage(15.5, 0, 15, 16), Is.EqualTo(1).Within(Eps), "on the ring");
            Assert.That(UiAdsMath.RingCoverage(0, 0, 15, 16), Is.EqualTo(0), "the centre is clear");
            Assert.That(UiAdsMath.RingCoverage(18, 0, 15, 16), Is.EqualTo(0), "outside");
            double edge = UiAdsMath.RingCoverage(15, 0, 15, 16);
            Assert.That(edge, Is.GreaterThan(0.2).And.LessThan(0.8), "a texel across the edge: partly covered (antialiased)");
        }
    }
}
