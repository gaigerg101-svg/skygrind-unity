// NIGHT38 builder H (design.n38.hud38): the bar's, the spin's and the ready edge's rules (Hud38Math, UiHud38.cs). Pure: no scene, no sim.
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Hud38Tests
    {
        const double Dt = 1.0 / 120;

        [Test]
        public void Cells_FillInTurn()
        {
            Assert.That(Hud38Math.Cell(0.5f, 3, 0), Is.EqualTo(1f));
            Assert.That(Hud38Math.Cell(0.5f, 3, 1), Is.EqualTo(0.5f).Within(1e-5));
            Assert.That(Hud38Math.Cell(0.5f, 3, 2), Is.EqualTo(0f));
            Assert.That(Hud38Math.Cell(1f, 3, 2), Is.EqualTo(1f));
        }

        [Test]
        public void ChargeGun_HoldFillsTheBar_FullOnlyAtLevelOne()
        {
            Meter38 half = Hud38Math.Read(true, true, 0.5, 0, 0, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(half.show && !half.ready && half.fill == 0.5f && half.spin < 0, "a half hold: bone, half, no spin");
            Meter38 full = Hud38Math.Read(true, true, 1.0, 0, 0, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(full.ready && full.fill == 1f, "a full hold: CYAN");
            Meter38 rest = Hud38Math.Read(true, false, 0, 0, 0, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(rest.show && rest.ready && rest.cells == UnityTuning.Hud38Segments, "a charge gun at rest: full, three cells");
        }

        [Test]
        public void ChargeCooldown_RefillsAndSpins()
        {
            Meter38 m = Hud38Math.Read(true, false, 0, 0, 0, 30, 120, false, 0, 0, 0, 0, Dt);
            Assert.That(m.fill, Is.EqualTo(0.75f).Within(1e-5));
            Assert.That(m.spin, Is.EqualTo(0.75f).Within(1e-5));
            Assert.That(m.ready, Is.False);
            Assert.That(Hud38Math.SpinAngle(m.spin), Is.EqualTo(0.75f * UnityTuning.Hud38SpinDeg).Within(1e-3));
        }

        [Test]
        public void ShortCooldown_NeverShowsNorSpins()
        {
            int pistol = (int)(0.2 / Dt);
            Meter38 m = Hud38Math.Read(false, false, 0, pistol / 2, pistol, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(m.show, Is.False, "a pistol's 0.2 s cycle: no bar");
            Assert.That(m.spin, Is.LessThan(0f), "and no spin");
            Assert.That(m.ready, Is.True);
            int pump = (int)(0.9 / Dt);
            Meter38 p = Hud38Math.Read(false, false, 0, pump / 3, pump, 0, 0, false, 0, 0, 0, 0, Dt);
            Assert.That(p.show && !p.ready && p.spin > 0.6f && p.spin < 0.7f, "a 0.9 s pump shows and spins");
        }

        [Test]
        public void Gauntlet_OneCellPerPip_ReadyOnlyFullAndCool()
        {
            Meter38 two = Hud38Math.Read(false, false, 0, 0, 0, 0, 0, true, 2, 5, 0, 0, Dt);
            Assert.That(two.cells, Is.EqualTo(5));
            Assert.That(two.fill, Is.EqualTo(0.4f).Within(1e-5));
            Assert.That(two.ready, Is.False);
            Meter38 five = Hud38Math.Read(false, false, 0, 0, 0, 0, 0, true, 5, 5, 0, 0, Dt);
            Assert.That(five.ready, Is.True);
            Meter38 cooling = Hud38Math.Read(false, false, 0, 0, 0, 0, 0, true, 5, 5, 60, 240, Dt);
            Assert.That(cooling.ready, Is.False, "the blast's cooldown holds the bar short of ready");
            Assert.That(cooling.spin, Is.EqualTo(0.75f).Within(1e-5));
            Assert.That(Hud38Math.Read(false, false, 0, 0, 0, 0, 0, true, 9, 9, 0, 0, Dt).cells, Is.EqualTo(UnityTuning.Hud38MaxSegments));
        }

        [Test]
        public void ReadyEdge_OnlyOnTheTurn()
        {
            Assert.That(Hud38Math.TurnedReady(false, true), Is.True);
            Assert.That(Hud38Math.TurnedReady(true, true), Is.False);
            Assert.That(Hud38Math.TurnedReady(false, false), Is.False);
        }
    }
}
