// NIGHT42 builder U1 (design.n42.meter42): the meter's one language (Hud42Math, UiHud38.Meter42.cs). Pure: no scene, no sim.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Hud42Tests
    {
        const double Dt = 1.0 / 120;

        [Test]
        public void Charge_FillIsTheLevel_NoEase()
        {
            for (int i = 0; i <= 20; i++)
            {
                double level = i / 20.0;
                Arc42 a = Hud42Math.Read(true, true, level, 0, 0, 0, 0, false, 0, 0, 0, 0, Dt);
                Assert.That(a.fill, Is.EqualTo((float)level).Within(1e-6), "the fill is the level at " + level);
                Assert.That(a.mode, Is.EqualTo(level >= 1 ? Hud42Math.Full : Hud42Math.Charging));
            }
            // arc40's before: a half charge showed about a third
            Assert.That(Hud42Math.Arc40Shown(0.5), Is.LessThan(0.35f).And.GreaterThan(0.31f));
        }

        [Test]
        public void Full_OnlyAtARealFull()
        {
            Arc42 rest = Hud42Math.Read(true, false, 0, 0, 0, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(rest.show && rest.mode == Hud42Math.Rest && rest.fill == 0f && !Hud42Math.IsFull(rest), "a charge gun at rest: the empty track, never FULL");
            Arc42 almost = Hud42Math.Read(true, true, 0.999, 0, 0, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(!Hud42Math.IsFull(almost));
            Arc42 full = Hud42Math.Read(true, true, 1.0, 0, 0, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(Hud42Math.IsFull(full) && full.fill == 1f);
            Arc42 cool = Hud42Math.Read(true, false, 0, 0, 0, 300, 300, false, 0, 0, 0, 0, Dt);
            Assert.That(!Hud42Math.IsFull(cool) && cool.mode == Hud42Math.Cooling && cool.fill == 0f, "the charge's cooldown starts empty");
        }

        [Test]
        public void Cooldowns_RefillLeftToRight()
        {
            Arc42 ch = Hud42Math.Read(true, false, 0, 0, 0, 90, 120, false, 0, 0, 0, 0, Dt);
            Assert.That(ch.mode, Is.EqualTo(Hud42Math.Cooling));
            Assert.That(ch.fill, Is.EqualTo(0.25f).Within(1e-5), "a quarter done: a quarter refilled");
            int pump = (int)(UnityTuning.Hud30MinCoolSeconds / Dt) + 12;
            Arc42 own = Hud42Math.Read(false, false, 0, pump / 4, pump, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(own.show && own.mode == Hud42Math.Cooling);
            Assert.That(own.fill, Is.EqualTo(0.75f).Within(0.01f));
            Arc42 quick = Hud42Math.Read(false, false, 0, 10, 20, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(!quick.show, "a quick gun's interval never shows (hud38's rule)");
        }

        [Test]
        public void Gauntlet_TheSwingIsDrawn_AndTheBlastLine()
        {
            // the swing: 1.8 s = 216 steps, 54 left: three quarters recovered, on line 2; the pips keep the band
            Arc42 swing = Hud42Math.Read(false, false, 0, 54, 216, 0, 0, true, 3, 5, 0, 1800, Dt);
            Assert.That(swing.segs == 5 && swing.mode == Hud42Math.Pips && swing.line < 0f);
            Assert.That(swing.line2, Is.EqualTo(0.75f).Within(1e-5), "the swing's recovery is drawn");
            Assert.That(swing.fill, Is.EqualTo(0.6f).Within(1e-5));
            Arc42 cool = Hud42Math.Read(false, false, 0, 0, 0, 0, 0, true, 5, 5, 450, 1800, Dt);
            Assert.That(cool.mode == Hud42Math.Pips && !Hud42Math.IsFull(cool), "a full glove with the BLAST cooling is not FULL");
            Assert.That(cool.line, Is.EqualTo(0.75f).Within(1e-5), "the BLAST's cooldown refills");
            Assert.That(cool.line2, Is.EqualTo(-1f));
            Arc42 ready = Hud42Math.Read(false, false, 0, 0, 0, 0, 0, true, 5, 5, 0, 1800, Dt);
            Assert.That(Hud42Math.IsFull(ready));
        }

        [Test]
        public void Thunderclap_PumpOnLine2_NotTheBand()
        {
            int pump = (int)(UnityTuning.Hud30MinCoolSeconds / Dt) + 12;
            Arc42 a = Hud42Math.Read(true, false, 0, pump / 2, pump, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(a.mode == Hud42Math.Rest && a.fill == 0f, "the special is ready to charge");
            Assert.That(a.line2, Is.EqualTo(0.5f).Within(0.01f), "the pump recovers on line 2");
        }

        [Test]
        public void Edges()
        {
            Assert.That(Hud42Math.Pinged(Hud42Math.Cooling, Hud42Math.Rest));
            Assert.That(!Hud42Math.Pinged(Hud42Math.Charging, Hud42Math.Rest), "a tap's release does not ping");
            Assert.That(Hud42Math.Flashed(Hud42Math.Charging, Hud42Math.Full));
            Assert.That(!Hud42Math.Flashed(Hud42Math.Full, Hud42Math.Full));
            Assert.That(!Hud42Math.Flashed(Hud42Math.Charging, Hud42Math.Rest), "a tap's release does not flash (arc40 did)");
            Assert.That(Hud42Math.Word(Hud42Math.Cooling), Is.EqualTo("COOLDOWN"));
        }
    }
}
