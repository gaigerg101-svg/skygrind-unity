// NIGHT9 U3-07: the arsenalMath.js parts of the web's tests/arsenal.test.js ported: freshness, the primed swap, cross
// combos (COLLAPSE, OVERLOAD) and the unlimited arsenal's ordering (hotbar digits, the wheel, the radial), through
// ArsenalMath. Skipped (they need the stateful arsenal, combat/arsenal.js createArsenal, not yet ported): "the arsenal
// system (POWER P3)": "stacks fresh x primed x cross, and a STUCK nail sets up a blast cross" and "uses wear a weapon out;
// out of hand it recovers; a kill right after a swap is a SWAP". The bit-exact replay of arsenalMath.json is
// GoldenPorted.cs.
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class ArsenalMathTests
    {
        static ArsenalParams a;
        static WeaponCatalog catalog;

        [OneTimeSetUp]
        public void Load()
        {
            var cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            a = ArsenalParams.From(cfg);
            catalog = WeaponCatalog.From(cfg);
        }

        static ArsenalRecord Rec(string family, double at, string status = null, double until = 0)
        {
            var r = new ArsenalRecord { family = family, at = at };
            if (status != null) r.statuses.Set(status, until);
            return r;
        }

        // --- freshness (POWER P3) ----------------------------------------------------------------------------------

        [Test]
        public void MultipliesDamageAndPointsFrom0_65StaleTo1_25Fresh()
        {
            Expect.CloseTo(ArsenalMath.FreshMult(0, a), 0.65);
            Expect.CloseTo(ArsenalMath.FreshMult(1, a), 1.25);
            Expect.CloseTo(ArsenalMath.FreshMult(0.5, a), 0.95);
            Expect.CloseTo(ArsenalMath.FreshMult(2, a), 1.25); // clamped
        }

        [Test]
        public void AUseCostsItsWeapon_OutOfHandItRecovers0_25PerSecond_x2WithFlip()
        {
            Expect.CloseTo(ArsenalMath.SpendFresh(1, 0.3), 0.7);
            Expect.Exactly(ArsenalMath.SpendFresh(0.1, 0.3), 0);
            Expect.CloseTo(ArsenalMath.RecoverFresh(0.5, 1, a.recover), 0.75);
            Expect.CloseTo(ArsenalMath.RecoverFresh(0.5, 1, a.recover, 2), 1);
            Expect.Exactly(ArsenalMath.RecoverFresh(0.9, 1, a.recover), 1);
        }

        [Test]
        public void SlowWeaponsCostMorePerUseThanFastOnes()
        {
            double Cost(string id) => catalog.Get(id).freshCost.Value;
            Assert.That(Cost("rail"), Is.GreaterThan(Cost("rocket")));
            Assert.That(Cost("rocket"), Is.GreaterThan(Cost("revolver")));
            Assert.That(Cost("revolver"), Is.GreaterThan(Cost("nailgun")));
        }

        // --- primed swap (POWER P3) --------------------------------------------------------------------------------

        [Test]
        public void Plus35PercentOnTheFirstHitWithin1sOfTheSwap_WithThatWeaponOnly()
        {
            var primed = new PrimedSwap { weapon = "shotgun", at = 10, used = false };
            Expect.CloseTo(ArsenalMath.PrimedMult(primed, "shotgun", 10.5, a.primedWindow, a.primedBonus), 1.35);
            Expect.Exactly(ArsenalMath.PrimedMult(primed, "shotgun", 11.2, a.primedWindow, a.primedBonus), 1);
            Expect.Exactly(ArsenalMath.PrimedMult(primed, "revolver", 10.5, a.primedWindow, a.primedBonus), 1);
            var used = new PrimedSwap { weapon = "shotgun", at = 10, used = true }; // {...primed, used: true}
            Expect.Exactly(ArsenalMath.PrimedMult(used, "shotgun", 10.5, a.primedWindow, a.primedBonus), 1);
            Expect.Exactly(ArsenalMath.PrimedMult(null, "shotgun", 10.5, a.primedWindow, a.primedBonus), 1);
        }

        // --- cross combos (POWER P3) -------------------------------------------------------------------------------

        [Test]
        public void MapsHitSourcesOntoWeaponsAndFamilies()
        {
            Assert.That(ArsenalMath.WeaponOf("punch"), Is.EqualTo("gauntlet"));
            Assert.That(ArsenalMath.WeaponOf("ricochet"), Is.EqualTo("revolver"));
            Assert.That(ArsenalMath.FamilyOf("punch", catalog), Is.EqualTo("kinetic"));
            Assert.That(ArsenalMath.FamilyOf("shotgun", catalog), Is.EqualTo("edge"));
            Assert.That(ArsenalMath.FamilyOf("slam", catalog), Is.EqualTo("impact"));
            Assert.That(ArsenalMath.FamilyOf("nope", catalog), Is.Null);
        }

        [Test]
        public void ADifferentFamilyThanTheLastHitWithin3s_1_5xCross_TheSameFamilyOrTooLate_None()
        {
            CrossOutcome x = ArsenalMath.CrossResult(Rec("kinetic", 9), "edge", "shotgun", 10, a, catalog);
            // toEqual({mult: 1.5, cross: true, combo: null})
            Expect.Exactly(x.mult, 1.5);
            Assert.That(x.cross, Is.True);
            Assert.That(x.combo, Is.Null);
            Expect.Exactly(ArsenalMath.CrossResult(Rec("edge", 9), "edge", "shotgun", 10, a, catalog).mult, 1);
            Expect.Exactly(ArsenalMath.CrossResult(Rec("kinetic", 6), "edge", "shotgun", 10, a, catalog).mult, 1);
            Expect.Exactly(ArsenalMath.CrossResult(null, "edge", "shotgun", 10, a, catalog).mult, 1);
        }

        [Test]
        public void PullThenABlastHitIsCollapse2x_ShockThenAMeleeHitIsOverload1_75x()
        {
            CrossOutcome collapse = ArsenalMath.CrossResult(Rec("gravity", 9.5, "pull", 11), "blast", "bouncer", 10, a, catalog);
            Expect.Exactly(collapse.mult, 2);
            Assert.That(collapse.combo, Is.EqualTo("COLLAPSE"));
            CrossOutcome overload = ArsenalMath.CrossResult(Rec("volt", 9.5, "shock", 11), "kinetic", "punch", 10, a, catalog);
            Expect.Exactly(overload.mult, 1.75);
            Assert.That(overload.combo, Is.EqualTo("OVERLOAD"));
            // An expired status is no combo (a plain cross still applies).
            CrossOutcome expired = ArsenalMath.CrossResult(Rec("volt", 9.5, "shock", 9), "kinetic", "punch", 10, a, catalog);
            Expect.Exactly(expired.mult, 1.5);
            Assert.That(expired.combo, Is.Null);
        }

        // --- unlimited arsenal ordering (POWER P3) -----------------------------------------------------------------

        [Test]
        public void OneToNineAndZeroPickTheFirstTenOwned_TheWheelWrapsOverAllOfThem()
        {
            Assert.That(ArsenalMath.SlotForDigit("1", 3), Is.EqualTo(0));
            Assert.That(ArsenalMath.SlotForDigit("3", 3), Is.EqualTo(2));
            Assert.That(ArsenalMath.SlotForDigit("4", 3), Is.EqualTo(-1));
            Assert.That(ArsenalMath.SlotForDigit("0", 12), Is.EqualTo(9));
            Assert.That(ArsenalMath.SlotForDigit("0", 9), Is.EqualTo(-1));
            Assert.That(ArsenalMath.CycleSlot(11, 12, 1), Is.EqualTo(0));
            Assert.That(ArsenalMath.CycleSlot(0, 12, -1), Is.EqualTo(11));
        }

        [Test]
        public void TheRadial_Sector0IsStraightUp_Clockwise_WithADeadZone()
        {
            Assert.That(ArsenalMath.RadialSector(0, -100, 8, 40), Is.EqualTo(0));
            Assert.That(ArsenalMath.RadialSector(100, 0, 8, 40), Is.EqualTo(2));
            Assert.That(ArsenalMath.RadialSector(0, 100, 8, 40), Is.EqualTo(4));
            Assert.That(ArsenalMath.RadialSector(-100, 0, 8, 40), Is.EqualTo(6));
            Assert.That(ArsenalMath.RadialSector(10, 10, 8, 40), Is.EqualTo(-1));
        }
    }
}
