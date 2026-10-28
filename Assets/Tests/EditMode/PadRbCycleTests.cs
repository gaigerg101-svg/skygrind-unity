// NIGHT20 U3-34.4 (builder P; the shared commit placed the empty fixture): the pad's RB scrolls the weapons (design.pad.n20,
// GG 2026-10-06: "the right bumper could be used to switch weapons as well, just scroll through weapons"), layout v3:
// - the v3 default: RB NEXT WEAPON (a tap scrolls; held design.pad.n20.wheelHold it opens the wheel: PadReader, tested in
//   Game/PadRbCycleReaderTests), Y SPECIAL + ARCLINE (v2's two rows swapped; the grapple stays bound: REQUIRED); every
//   other button as v2's, in v2's key order (CONTROLS' and padRows' rows);
// - the store: skygrind.padLayout.v3 ({"version":3,"layout":{..}}); with no v3 stored, the v2 store (itself migrated from a
//   v1 when there's no v2) is migrated once (MigrateV2) and written as v3, the v2 and v1 strings kept for a rollback;
// - the v2 -> v3 migration (the v1 -> v2 rule, MigrateV1): a v2 equal to the v2 default becomes the v3 default; a
//   customised v2 keeps its buttons, except that RB and Y trade their actions when both still hold the v2 default's (the
//   player never touched them); a v2 whose RB or Y was remapped stays as it is.
// Pure: no Unity runtime (Core's GamepadMath only).
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class PadRbCycleTests
    {
        static PadAction[] Acts(PadLayout l, PadButtonName b)
        {
            var list = l.ActionsOf(b);
            var a = new PadAction[list.Count];
            for (int i = 0; i < a.Length; i++) a[i] = list[i];
            return a;
        }

        static readonly PadAction[] YV2 = { PadAction.NextWeapon }, RbV2 = { PadAction.Special, PadAction.Grapple };

        [Test]
        public void TheV3Default_RbIsNextWeapon_YIsSpecialAndArcline_EveryOtherButtonAsV2()
        {
            PadLayout v2 = GamepadMath.DefaultLayoutV2, v3 = GamepadMath.DefaultLayoutV3;
            Assert.That(Acts(v3, PadButtonName.RB), Is.EqualTo(new[] { PadAction.NextWeapon }), "RB scrolls the weapons");
            Assert.That(Acts(v3, PadButtonName.Y), Is.EqualTo(new[] { PadAction.Special, PadAction.Grapple }), "Y: the special, the grapple kept bound");
            for (int i = 0; i < v2.Count; i++)
            {
                PadButtonName b = v2[i];
                Assert.That(v3[i], Is.EqualTo(b), "v2's key order (CONTROLS' and padRows' rows)");
                if (b == PadButtonName.RB || b == PadButtonName.Y) continue;
                Assert.That(Acts(v3, b), Is.EqualTo(Acts(v2, b)), $"{b} as v2's");
            }
            Assert.That(v3.Count, Is.EqualTo(v2.Count), "the same 16 buttons");
            Assert.That(Acts(v2, PadButtonName.Y), Is.EqualTo(YV2), "v2 itself unchanged (the block off: NIGHT19's pad)");
            Assert.That(Acts(v2, PadButtonName.RB), Is.EqualTo(RbV2));
        }

        [Test]
        public void TheV3Default_OnThePad_RbHoldsNextWeapon_YHoldsSpecialAndTheGrapple()
        {
            PadLayout v3 = GamepadMath.DefaultLayoutV3;
            var pad = new PadState();
            pad.Set(PadButtonName.RB, true, 1);
            uint held = GamepadMath.HeldActions(pad, v3);
            Assert.That(held, Is.EqualTo(GamepadMath.Bit(PadAction.NextWeapon)), "RB: NEXT WEAPON only (no special, no grapple)");
            pad.Clear();
            pad.Set(PadButtonName.Y, true, 1);
            held = GamepadMath.HeldActions(pad, v3);
            Assert.That(held, Is.EqualTo(GamepadMath.Bit(PadAction.Special) | GamepadMath.Bit(PadAction.Grapple)), "Y: SPECIAL + ARCLINE");
            Assert.That(GamepadMath.ButtonFor(PadAction.NextWeapon, v3), Is.EqualTo(PadButtonName.RB), "the prompts' NEXT WEAPON button");
            Assert.That(GamepadMath.ButtonFor(PadAction.Special, v3), Is.EqualTo(PadButtonName.Y));
            Assert.That(GamepadMath.ButtonFor(PadAction.Grapple, v3), Is.EqualTo(PadButtonName.Y));
            Assert.That(GamepadMath.ButtonFor(PadAction.PrevWeapon, v3), Is.EqualTo(PadButtonName.Left), "d-pad left stays PREVIOUS");
            Assert.That(GamepadMath.ButtonFor(PadAction.Wheel, v3), Is.Null, "no button of its own: RB held is the wheel");
        }

        [Test]
        public void TheV3Default_KeepsEveryRequiredActionBound_AndReadsBackThroughTheV3Store()
        {
            PadLayout v3 = GamepadMath.DefaultLayoutV3;
            Assert.That(GamepadMath.ReadPadLayout(v3.ToJson()).SameAs(v3), Is.True, "every REQUIRED action bound (else the read gives the default)");
            string json = GamepadMath.PadLayoutV3Json(v3);
            StringAssert.StartsWith("{\"version\":3,\"layout\":{", json);
            Assert.That(GamepadMath.LoadPadLayoutV3(json, null, null, out bool migrated).SameAs(v3), Is.True);
            Assert.That(migrated, Is.False, "a v3 store is read as it is");
            Assert.That(GamepadMath.DefaultForVersion(3), Is.SameAs(v3));
            Assert.That(GamepadMath.DefaultForVersion(2), Is.SameAs(GamepadMath.DefaultLayoutV2));
            Assert.That(GamepadMath.DefaultForVersion(1), Is.SameAs(GamepadMath.DefaultLayout));
        }

        [Test]
        public void NoStore_IsTheV3Default_NothingMigrated()
        {
            Assert.That(GamepadMath.LoadPadLayoutV3(null, null, null, out bool migrated).SameAs(GamepadMath.DefaultLayoutV3), Is.True);
            Assert.That(migrated, Is.False, "nothing to write");
        }

        [Test]
        public void AV2EqualToTheV2Default_BecomesTheV3Default_AsDoesAV1Default()
        {
            string v2Json = GamepadMath.PadLayoutV2Json(GamepadMath.DefaultLayoutV2);
            Assert.That(GamepadMath.LoadPadLayoutV3(null, v2Json, null, out bool m2).SameAs(GamepadMath.DefaultLayoutV3), Is.True);
            Assert.That(m2, Is.True, "written as v3 (the caller keeps the v2 string)");
            Assert.That(GamepadMath.LoadPadLayoutV3(null, null, GamepadMath.DefaultLayout.ToJson(), out bool m1).SameAs(GamepadMath.DefaultLayoutV3), Is.True,
                "no v2: the v1 default goes v1 -> v2 -> v3");
            Assert.That(m1, Is.True);
            // The same buttons in another key order is still the default (toEqual).
            var entries = GamepadMath.DefaultLayoutV2.Entries();
            entries.Reverse();
            Assert.That(GamepadMath.MigrateV2(new PadLayout(entries)).SameAs(GamepadMath.DefaultLayoutV3), Is.True);
            // A corrupt v2 reads as missing (LoadPadLayoutV2's rule): with no v1 either, the v3 default, nothing to write.
            Assert.That(GamepadMath.LoadPadLayoutV3(null, "{not json", null, out bool mc).SameAs(GamepadMath.DefaultLayoutV3), Is.True);
            Assert.That(mc, Is.True, "a corrupt store is replaced by the v3 store");
        }

        [Test]
        public void ACustomisedV2_ThatNeverTouchedRbAndY_GetsTheSwap_AndKeepsEveryOtherPick()
        {
            // JUMP to B (A and B swap: A slides, B jumps); RB and Y untouched.
            PadLayout custom = GamepadMath.RebindPad(GamepadMath.DefaultLayoutV2, PadAction.Jump, PadButtonName.B).layout;
            Assert.That(custom.SameAs(GamepadMath.DefaultLayoutV2), Is.False);
            PadLayout m = GamepadMath.LoadPadLayoutV3(null, GamepadMath.PadLayoutV2Json(custom), null, out bool migrated);
            Assert.That(migrated, Is.True);
            Assert.That(Acts(m, PadButtonName.A), Is.EqualTo(new[] { PadAction.Slide }), "the player's swap kept");
            Assert.That(Acts(m, PadButtonName.B), Is.EqualTo(new[] { PadAction.Jump }), "the player's swap kept");
            Assert.That(Acts(m, PadButtonName.RB), Is.EqualTo(new[] { PadAction.NextWeapon }), "RB scrolls now");
            Assert.That(Acts(m, PadButtonName.Y), Is.EqualTo(RbV2), "Y carries SPECIAL + ARCLINE");
            Assert.That(GamepadMath.ReadPadLayout(m.ToJson()).SameAs(m), Is.True, "every REQUIRED action still bound");
            for (int i = 0; i < custom.Count; i++) Assert.That(m[i], Is.EqualTo(custom[i]), "the player's key order");
        }

        [Test]
        public void ACustomisedV2_ThatRemappedRb_KeepsItsButtons()
        {
            // JUMP to RB: RB jumps, A carries SPECIAL + ARCLINE. RB is the player's pick: nothing moves.
            PadLayout custom = GamepadMath.RebindPad(GamepadMath.DefaultLayoutV2, PadAction.Jump, PadButtonName.RB).layout;
            PadLayout m = GamepadMath.MigrateV2(custom);
            Assert.That(m.SameAs(custom), Is.True, "RB holds the player's own pick");
            Assert.That(GamepadMath.ButtonFor(PadAction.NextWeapon, m), Is.EqualTo(PadButtonName.Y), "NEXT WEAPON stays on Y");
        }

        [Test]
        public void ACustomisedV2_ThatRemappedY_KeepsItsButtons()
        {
            // LOCK-ON to Y: Y locks on, d-pad up is NEXT WEAPON. Y is the player's pick: nothing moves.
            PadLayout custom = GamepadMath.RebindPad(GamepadMath.DefaultLayoutV2, PadAction.LockOn, PadButtonName.Y).layout;
            PadLayout m = GamepadMath.MigrateV2(custom);
            Assert.That(m.SameAs(custom), Is.True, "Y holds the player's own pick");
            Assert.That(Acts(m, PadButtonName.RB), Is.EqualTo(RbV2), "RB keeps SPECIAL + ARCLINE");
            Assert.That(GamepadMath.ButtonFor(PadAction.NextWeapon, m), Is.EqualTo(PadButtonName.Up));
        }

        [Test]
        public void APlayerWhoSwappedRbAndYThemselves_KeepsThatSwap()
        {
            // NEXT WEAPON to RB on v2: RB scrolls, Y carries SPECIAL + ARCLINE (the v3 default's rows, by hand).
            PadLayout custom = GamepadMath.RebindPad(GamepadMath.DefaultLayoutV2, PadAction.NextWeapon, PadButtonName.RB).layout;
            PadLayout m = GamepadMath.MigrateV2(custom);
            Assert.That(m.SameAs(custom), Is.True, "never swapped back");
            Assert.That(m.SameAs(GamepadMath.DefaultLayoutV3), Is.True, "and it is the v3 default");
        }

        [Test]
        public void TheV3Store_NeedsItsVersion_ACorruptOrOtherVersionReadsAsMissing()
        {
            PadLayout custom = GamepadMath.RebindPad(GamepadMath.DefaultLayoutV3, PadAction.Jump, PadButtonName.B).layout;
            string json = GamepadMath.PadLayoutV3Json(custom);
            Assert.That(GamepadMath.LoadPadLayoutV3(json, GamepadMath.PadLayoutV2Json(GamepadMath.DefaultLayoutV2), null, out bool m0).SameAs(custom), Is.True, "v3 wins over v2");
            Assert.That(m0, Is.False);
            string v2Version = json.Replace("\"version\":3", "\"version\":2");
            Assert.That(GamepadMath.LoadPadLayoutV3(v2Version, null, null, out bool m1).SameAs(GamepadMath.DefaultLayoutV3), Is.True, "another version under the v3 key: as missing");
            Assert.That(m1, Is.False, "nothing stored to migrate");
            Assert.That(GamepadMath.LoadPadLayoutV3("[1,2]", null, null, out _).SameAs(GamepadMath.DefaultLayoutV3), Is.True, "not an object");
            Assert.That(GamepadMath.LoadPadLayoutV3(custom.ToJson(), null, null, out _).SameAs(GamepadMath.DefaultLayoutV3), Is.True, "a bare layout has no version");
            string noPause = "{\"version\":3,\"layout\":{\"rt\":[\"fire\"]}}";
            Assert.That(GamepadMath.LoadPadLayoutV3(noPause, null, null, out _).SameAs(GamepadMath.DefaultLayoutV3), Is.True, "REQUIRED unbound: the v3 default");
        }

        [Test]
        public void ACustomisedV1_GoesThroughV2_AndKeepsWhatItsRbAndYHold()
        {
            // A v1 with A / B swapped: v1 -> v2 trades PUNCH and LOCK-ON (RB locks on: the player's v1 RB was PUNCH, a v1
            // default) and puts the ARCLINE on SPECIAL's d-pad up; its RB then isn't v2's, so v2 -> v3 keeps it.
            PadLayout v1 = GamepadMath.RebindPad(GamepadMath.DefaultLayout, PadAction.Jump, PadButtonName.B).layout;
            PadLayout m = GamepadMath.LoadPadLayoutV3(null, null, v1.ToJson(), out bool migrated);
            Assert.That(migrated, Is.True);
            Assert.That(m.SameAs(GamepadMath.MigrateV1(v1)), Is.True, "the v1 -> v2 rule's answer, kept");
            Assert.That(Acts(m, PadButtonName.Y), Is.EqualTo(YV2), "Y still scrolls");
        }
    }
}
