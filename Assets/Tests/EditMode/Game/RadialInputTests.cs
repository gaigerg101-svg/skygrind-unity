// NIGHT13 U3-26.7: the keyboard and pad weapon selection (Game/Input/WeaponSelect.cs; keyboardMouse.js :17-20, :80-85,
// :124-129, :168-201, :221-236, :266-273, :399-402; NIGHT14-RESEARCH section 2) on config's weapons.wheelCooldown and
// weapons.radial: digits 1-9 and 0 (unowned and current slots ignored, the last weapon remembered), quick swap, the wheel
// with its cooldown, the radial (opens at the centre, the mouse points, the offset clamped, the release picks), a slot
// change from outside, and the pad's next / previous. The pure maths (slotForDigit, cycleSlot, radialSector) are
// ArsenalMathTests'; this is the stateful layer on top.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class RadialInputTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport", "config.json")));

        [Test]
        public void Digits_PickTheFirstTenOwned_UnownedAndCurrentIgnored_TheLastRemembered()
        {
            WeaponSelect s = WeaponSelect.From(cfg);
            Assert.That(s.Digit(3, 12), Is.True);
            Assert.That((s.weapon, s.lastWeapon, s.Consume()), Is.EqualTo((2, 0, 2)));
            Assert.That(s.Digit(3, 12), Is.False, "the current slot");
            Assert.That(s.requested, Is.EqualTo(-1));
            Assert.That(s.Digit(0, 12), Is.True, "0 is the tenth");
            Assert.That(s.weapon, Is.EqualTo(9));
            Assert.That(s.Digit(0, 9), Is.False, "a slot you don't own");
            Assert.That(s.Digit(4, 3), Is.False);
            Assert.That(s.QuickSwap(12), Is.True);
            Assert.That((s.weapon, s.lastWeapon), Is.EqualTo((2, 9)), "Q goes back and remembers");
            s.ResetRun(0);
            Assert.That(s.QuickSwap(12), Is.False, "a new run forgets the last weapon");
        }

        [Test]
        public void TheWheel_WrapsOverEveryOwned_OneStepPerCooldown()
        {
            WeaponSelect s = WeaponSelect.From(cfg);
            double cooldown = cfg.Num("weapons.wheelCooldown");
            s.ResetRun(11);
            Assert.That(s.Wheel(120, 10, 12), Is.True, "down: the next, wrapping");
            Assert.That(s.weapon, Is.EqualTo(0));
            Assert.That(s.Wheel(120, 10 + cooldown / 2, 12), Is.False, "inside the cooldown");
            Assert.That(s.Wheel(-120, 10 + cooldown * 2, 12), Is.True, "up: the previous, wrapping");
            Assert.That(s.weapon, Is.EqualTo(11));
            Assert.That(s.Wheel(0, 20, 12), Is.False);
            s.ResetRun(0);
            Assert.That(s.Wheel(120, 30, 1), Is.False, "one gun: nothing to cycle to");
        }

        [Test]
        public void TheRadial_OpensAtTheCentre_TheMousePoints_TheOffsetClamps_TheReleasePicks()
        {
            WeaponSelect s = WeaponSelect.From(cfg);
            double maxOffset = cfg.Num("weapons.radial.maxOffset");
            Assert.That(s.OpenRadial(8), Is.True);
            Assert.That(s.OpenRadial(8), Is.False, "already open");
            Assert.That((s.radial.open, s.radial.highlighted, s.radial.slots), Is.EqualTo((true, -1, 8)));
            Assert.That(s.MoveRadial(0, -100, 8), Is.True);
            Assert.That(s.radial.highlighted, Is.EqualTo(0), "straight up: sector 0");
            s.MoveRadial(100, 100, 8);
            Assert.That(s.radial.highlighted, Is.EqualTo(2), "right: clockwise");
            s.MoveRadial(1000, 0, 8);
            Assert.That(JsMath.Hypot(s.radial.x, s.radial.y), Is.LessThanOrEqualTo(maxOffset + 1e-9), "clamped to radial.maxOffset");
            Assert.That(s.CloseRadial(true, 8), Is.True);
            Assert.That((s.radial.open, s.weapon, s.Consume()), Is.EqualTo((false, 2, 2)), "the release takes the highlighted slot");
            s.OpenRadial(8);
            s.MoveRadial(10, 10, 8);
            Assert.That(s.radial.highlighted, Is.EqualTo(-1), "inside the dead zone");
            s.CloseRadial(true, 8);
            Assert.That(s.requested, Is.EqualTo(-1), "nothing highlighted: nothing taken");
            s.OpenRadial(8);
            s.MoveRadial(0, 100, 8);
            s.CloseRadial(false, 8);
            Assert.That(s.requested, Is.EqualTo(-1), "a lost focus closes without a pick");
        }

        [Test]
        public void ASlotChangeFromOutside_IsRemembered_APendingRequestIsNot()
        {
            WeaponSelect s = WeaponSelect.From(cfg);
            s.Sync(3, 6); // a wall buy equipped slot 3
            Assert.That((s.weapon, s.lastWeapon, s.requested), Is.EqualTo((3, 0, -1)));
            s.Digit(5, 6);
            s.Sync(3, 6); // the step hasn't consumed the request yet: the loadout still shows 3
            Assert.That(s.weapon, Is.EqualTo(4), "a waiting request wins");
        }

        [Test]
        public void ThePad_YIsTheNext_TheDPadThePrevious_Wrapping()
        {
            WeaponSelect s = WeaponSelect.From(cfg);
            s.ResetRun(2);
            Assert.That(s.Next(3), Is.True);
            Assert.That(s.weapon, Is.EqualTo(0));
            Assert.That(s.Prev(3), Is.True);
            Assert.That(s.weapon, Is.EqualTo(2));
            s.ResetRun(0);
            Assert.That(s.Next(1), Is.False, "one gun: nothing to cycle to");
        }
    }
}
