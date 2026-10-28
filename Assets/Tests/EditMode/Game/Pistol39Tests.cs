// NIGHT39 builder P1 (design.n39.p39): the KESTREL's view shapes (Pistol39Math, Pistol39View.cs) and its crosshair spin (Hud39Math, UiHud38.Kestrel39.cs).
// Pure: no scene, no sim.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Pistol39Tests
    {
        const double Dt = 1.0 / 120;

        [Test]
        public void Settle_SnapsUp_DipsOnce_ThenRests()
        {
            Assert.That(Pistol39Math.Settle(double.PositiveInfinity, 0.03, 0.3), Is.EqualTo(0));
            Assert.That(Pistol39Math.Settle(0, 0.03, 0.3), Is.EqualTo(0));
            Assert.That(Pistol39Math.Settle(0.03, 0.03, 0.3), Is.EqualTo(1).Within(1e-9), "the peak right after the attack");
            Assert.That(Pistol39Math.Settle(0.03 + 0.3 * 0.6, 0.03, 0.3), Is.LessThan(0), "one dip below rest in the settle");
            Assert.That(Pistol39Math.Settle(0.03 + 0.3 * 0.6, 0.03, 0.3), Is.GreaterThan(-0.2), "a small dip");
            Assert.That(Pistol39Math.Settle(0.4, 0.03, 0.3), Is.EqualTo(0), "at rest after attack + settle");
        }

        [Test]
        public void Fade_FullAtOnce_ZeroAtLife()
        {
            Assert.That(Pistol39Math.Fade(0, 0.1), Is.EqualTo(1));
            Assert.That(Pistol39Math.Fade(0.05, 0.1), Is.EqualTo(0.5).Within(1e-9));
            Assert.That(Pistol39Math.Fade(0.1, 0.1), Is.EqualTo(0));
            Assert.That(Pistol39Math.Fade(double.PositiveInfinity, 0.1), Is.EqualTo(0));
        }

        [Test]
        public void KestrelSpin_ThroughTheCharge_SnapAtFull()
        {
            Meter38 half = Hud39Math.Kestrel(Hud38Math.Read(true, true, 0.5, 0, 0, 0, 0, false, 0, 0, 0, 0, Dt), true, 0.5, 0, 0);
            Assert.That(half.spin, Is.EqualTo(0.5f), "half a charge: half a turn (hud38 alone: no spin)");
            Meter38 full = Hud39Math.Kestrel(Hud38Math.Read(true, true, 1.0, 0, 0, 0, 0, false, 0, 0, 0, 0, Dt), true, 1.0, 0, 0);
            Assert.That(full.spin, Is.EqualTo(1f), "FULL: the spin ends (the snap)");
        }

        [Test]
        public void KestrelSpin_ThroughTheTap_AQuarterTurn_ChargeCooldownKept()
        {
            // the tap's own 58-step interval, half done: NIGHT38's rule shows nothing under 0.6 s
            Meter38 hud38 = Hud38Math.Read(true, false, 0, 29, 58, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(hud38.spin, Is.LessThan(0), "hud38 alone: the KESTREL's interval never spins");
            Meter38 tap = Hud39Math.Kestrel(hud38, false, 0, 29, 58);
            Assert.That(Hud38Math.SpinAngle(tap.spin), Is.EqualTo(UnityTuning.Hud39TapSpinDeg * 0.5f).Within(1e-3), "half an interval: half a quarter turn");
            // the charged shot's cooldown: hud38's own full turn stays
            Meter38 cool = Hud38Math.Read(true, false, 0, 29, 58, 150, 300, false, 0, 0, 0, 0, Dt);
            Assert.That(Hud39Math.Kestrel(cool, false, 0, 29, 58).spin, Is.EqualTo(cool.spin), "the charge's cooldown keeps NIGHT38's turn");
            Assert.That(Hud39Math.Kestrel(Hud38Math.Read(true, false, 0, 0, 58, 0, 0, false, 0, 0, 0, 0, Dt), false, 0, 0, 58).spin, Is.LessThan(0), "at rest: no spin");
        }
    }
}
