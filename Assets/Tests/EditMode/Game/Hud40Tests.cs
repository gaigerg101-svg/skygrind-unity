// NIGHT40 builder H1 (design.n40.arc40): the arc's rules (Hud40Math, UiHud38.Arc40.cs). Pure: no scene, no sim.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Hud40Tests
    {
        const double Dt = 1.0 / 120;

        [Test]
        public void Charge_FillsEased_FullOnlyAtLevelOne()
        {
            Arc40 half = Hud40Math.Read(true, true, 0.5, 0, 0, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(half.show && !half.ready && half.mode == Hud40Math.Charging, "a half hold: shown, charging, not ready");
            Assert.That(half.fill, Is.LessThan(0.5f), "eased: slow first (level^pow under the level)");
            Assert.That(half.fill, Is.EqualTo((float)System.Math.Pow(0.5, UnityTuning.Hud40FillEasePow)).Within(1e-5));
            Arc40 full = Hud40Math.Read(true, true, 1.0, 0, 0, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(full.ready && full.fill == 1f, "a full hold: CYAN");
            Arc40 rest = Hud40Math.Read(true, false, 0, 0, 0, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(rest.show && rest.ready && rest.segs == 1 && rest.fill == 1f, "a charge gun at rest: one full segment");
        }

        [Test]
        public void Ease_EndsExact_AndRises()
        {
            Assert.That(Hud40Math.Ease(0), Is.EqualTo(0f));
            Assert.That(Hud40Math.Ease(1), Is.EqualTo(1f));
            Assert.That(Hud40Math.Ease(-1), Is.EqualTo(0f));
            Assert.That(Hud40Math.Ease(2), Is.EqualTo(1f));
            float prev = -1f;
            for (int i = 0; i <= 20; i++)
            {
                float v = Hud40Math.Ease(i / 20.0);
                Assert.That(v, Is.GreaterThan(prev));
                prev = v;
            }
        }

        [Test]
        public void Cooldown_Drains()
        {
            Arc40 ch = Hud40Math.Read(true, false, 0, 0, 0, 30, 120, false, 0, 0, 0, 0, Dt);
            Assert.That(ch.mode == Hud40Math.Draining && !ch.ready, "the charge shot's cooldown drains");
            Assert.That(ch.fill, Is.EqualTo(0.25f).Within(1e-5), "the share LEFT");
            int pump = (int)(UnityTuning.Hud30MinCoolSeconds / Dt) + 12;
            Arc40 own = Hud40Math.Read(false, false, 0, pump / 2, pump, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(own.show && own.mode == Hud40Math.Draining && !own.ready, "a slow gun's own cooldown drains");
            Assert.That(own.fill, Is.EqualTo(0.5f).Within(0.01f));
            Arc40 quick = Hud40Math.Read(false, false, 0, 10, 20, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(!quick.show, "a quick gun's interval never shows (hud38's rule)");
        }

        [Test]
        public void Gauntlet_PipsSegments_AndTheBlastLine()
        {
            Arc40 three = Hud40Math.Read(false, false, 0, 0, 0, 0, 0, true, 3, 5, 0, 1800, Dt);
            Assert.That(three.segs == 5 && three.mode == Hud40Math.Pips && !three.ready && three.line < 0, "3 of 5 pips, no cooldown: no line");
            Assert.That(three.fill, Is.EqualTo(0.6f).Within(1e-5));
            Arc40 cool = Hud40Math.Read(false, false, 0, 0, 0, 0, 0, true, 5, 5, 900, 1800, Dt);
            Assert.That(!cool.ready && cool.line == 0.5f, "full glove, blast cooling: not ready, the line at half");
            Arc40 ready = Hud40Math.Read(false, false, 0, 0, 0, 0, 0, true, 5, 5, 0, 1800, Dt);
            Assert.That(ready.ready && ready.fill == 1f && ready.line < 0, "full and cooled: CYAN");
            Arc40 many = Hud40Math.Read(false, false, 0, 0, 0, 0, 0, true, 2, 12, 0, 0, Dt);
            Assert.That(many.segs, Is.EqualTo(UnityTuning.Hud40MaxSegments));
        }

        [Test]
        public void Ready_MatchesHud38_OverAGrid()
        {
            // the ready edge (the flash, hud38Ready) is hud38's: the arc must agree with Meter38 on every input, and show when the bar shows
            bool[] tf = { false, true };
            double[] levels = { 0, 0.3, 0.99, 1 };
            int[] lefts = { 0, 30, 90 };
            foreach (bool charge in tf)
            foreach (bool holding in tf)
            foreach (double lv in levels)
            foreach (int cd in lefts)
            foreach (int chl in lefts)
            foreach (bool pips in tf)
            foreach (int pn in new[] { 0, 5 })
            foreach (int bl in lefts)
            {
                Meter38 m = Hud38Math.Read(charge, holding, lv, cd, 90, chl, 90, pips, pn, 5, bl, 90, Dt);
                Arc40 a = Hud40Math.Read(charge, holding, lv, cd, 90, chl, 90, pips, pn, 5, bl, 90, Dt);
                Assert.That(a.ready, Is.EqualTo(m.ready), $"ready: charge {charge} holding {holding} level {lv} cd {cd} ch {chl} pips {pips} {pn} bl {bl}");
                Assert.That(a.show, Is.EqualTo(m.show), $"show: charge {charge} holding {holding} level {lv} cd {cd} ch {chl} pips {pips} {pn} bl {bl}");
            }
        }

        [Test]
        public void Stitcher_HeatShrinksCyan_LockDrainsBone()
        {
            Arc40 cold = Hud40Math.Heat(0, false);
            Assert.That(!cold.show && cold.ready && cold.fill == 1f, "no heat: hidden, usable");
            Arc40 warm = Hud40Math.Heat(0.3, false);
            Assert.That(warm.show && warm.ready && warm.mode == Hud40Math.Rest, "heating: shown, usable (cyan)");
            Assert.That(warm.fill, Is.EqualTo(0.7f).Within(1e-5), "the capacity left");
            Arc40 locked = Hud40Math.Heat(0.6, true);
            Assert.That(locked.show && !locked.ready && locked.mode == Hud40Math.Draining && locked.fill == 0.6f, "the lock drains, not usable");
            Assert.That(Hud40Math.Heat(0, false).ready && !Hud40Math.Heat(0.01, true).ready, "the lock's end is the ready edge");
        }

        const double Dt120 = 1.0 / 120;

        static int Steps(double s) => (int)System.Math.Round(s / Dt120);

        [Test]
        public void Kestrel40_TapQuarterTurn_EndsAtTapSpinS()
        {
            int full = Steps(0.483);
            Meter38 m = new Meter38 { spin = -1f };
            Meter38 a = Kestrel40Math.Kestrel(m, false, 0, full - 1, full, 0, 0, Dt120, out double snap, out int kind);
            Assert.That(kind == Kestrel40Math.Tap && a.spin >= 0 && a.spin < 0.25f && snap == UnityTuning.Pistol40TapSnapS, "the tap's turn starts at the shot");
            int left = full - Steps(UnityTuning.Pistol40TapSpinS * 0.5);
            Meter38 mid = Kestrel40Math.Kestrel(m, false, 0, left, full, 0, 0, Dt120, out _, out _);
            Assert.That(mid.spin, Is.EqualTo(0.75f * 0.25f).Within(0.01f), "ease-out: 3/4 of the quarter turn at half its time");
            Meter38 done = Kestrel40Math.Kestrel(m, false, 0, full - Steps(UnityTuning.Pistol40TapSpinS) - 1, full, 0, 0, Dt120, out _, out int k2);
            Assert.That(k2 == Kestrel40Math.None && done.spin < 0, "past TapSpinS the turn has ended (the snap runs), the interval still running");
            Assert.That(UnityTuning.Pistol40TapSpinS + UnityTuning.Pistol40TapSnapS, Is.LessThanOrEqualTo(UnityTuning.Pistol40Share * 0.483 + 1e-6), "turn + snap inside 75 % of the tap interval");
        }

        [Test]
        public void Kestrel40_CannonFullTurn_ThenTheTap_AndTheCharge()
        {
            int ch = Steps(2.5);
            Meter38 m = new Meter38 { spin = -1f };
            Meter38 c = Kestrel40Math.Kestrel(m, false, 0, 0, 0, ch - Steps(1.0), ch, Dt120, out double snap, out int kind);
            Assert.That(kind == Kestrel40Math.Cannon && c.spin > 0.5f && c.spin < 1f && snap == UnityTuning.Pistol40CannonSnapS, "1 s after the charged shot: mid full turn");
            Meter38 over = Kestrel40Math.Kestrel(m, false, 0, 0, 0, ch - Steps(UnityTuning.Pistol40CannonSpinS) - 1, ch, Dt120, out _, out int k2);
            Assert.That(k2 == Kestrel40Math.None && over.spin < 0, "past CannonSpinS: ended, the cooldown still running");
            Assert.That(UnityTuning.Pistol40CannonSpinS + UnityTuning.Pistol40CannonSnapS, Is.LessThanOrEqualTo(UnityTuning.Pistol40Share * 2.5 + 1e-6), "turn + snap inside 75 % of the cooldown");
            Meter38 hold = Kestrel40Math.Kestrel(m, true, 0.4, 0, 0, 0, 0, Dt120, out double hs, out int k3);
            Assert.That(k3 == Kestrel40Math.Charge && hold.spin == 0.4f && hs == UnityTuning.Pistol40ChargeSnapS, "the hold: a full turn through the charge, the 0.08 s snap at FULL");
            Meter38 full = Kestrel40Math.Kestrel(m, true, 1, 0, 0, 0, 0, Dt120, out _, out _);
            Assert.That(full.spin, Is.EqualTo(1f), "FULL: the turn ends (hud38's snap)");
        }

        [Test]
        public void Arc_SitsUnderTheReticle()
        {
            Assert.That(Hud40Math.StartDeg + UnityTuning.Hud40SweepDeg * 0.5f, Is.EqualTo(270f), "centred straight down");
            Assert.That(UnityTuning.Hud40Radius - UnityTuning.Hud40Thick - UnityTuning.Hud40LineGap - UnityTuning.Hud40LineThick, Is.GreaterThan(18f + 2f), "the blast line stays clear of hud33's ticks (18 px out)");
            Assert.That(UnityTuning.Hud40GrappleY + 6f, Is.LessThan(-(UnityTuning.Hud40Radius + UnityTuning.Hud40FlashGrow + UnityTuning.Hud40StrokePx)), "the hook's top under the arc's stroke, even on the flash");
            Assert.That(UnityTuning.Hud40Radius - UnityTuning.Hud40Thick - UnityTuning.Hud40StrokePx, Is.GreaterThan(18f + 2f), "the band's stroke stays clear of hud33's ticks");
            Assert.That(UnityTuning.Hud40LineGap, Is.GreaterThanOrEqualTo(2f * UnityTuning.Hud40StrokePx), "the band's and the line's strokes never overlap");
        }
    }
}
