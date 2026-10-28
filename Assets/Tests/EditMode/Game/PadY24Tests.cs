// NIGHT24 pad-y-weapons (builder R; design.n24.padY, GG 2026-10-08: "the Y ... should also be able to switch weapons"). Written first, the
// OFF tests before the ON ones:
// - OFF (the config row false, or N24Blocks.ForceOff = --off24): the reader is NIGHT22's v3 (LayoutVersion 3, RB NEXT WEAPON, Y SPECIAL + ARCLINE);
// - ON: layout v4: Y NEXT WEAPON (a tap on the release, a hold of design.pad.n20.wheelHold opens the wheel), RB SPECIAL + ARCLINE, LB DASH
//   (bound in every default layout); the old RB scroll is dropped; the store is skygrind.padLayout.v4 and a saved v3 migrates (MigrateV3);
// - a Y tap that moves the slot emits the port-only "weaponSwitch" on the bus (v4 only).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class PadY24Tests
    {
        static GameConfig cfg;

        static string Root()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (System.Exception) { return Path.GetFullPath("."); }
        }

        bool off19, off20, off24;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Root(), "PortExport", "config.json"));

        [SetUp]
        public void Save()
        {
            off19 = N19Blocks.ForceOff;
            off20 = N20Blocks.ForceOff;
            off24 = N24Blocks.ForceOff;
            N19Blocks.ForceOff = N20Blocks.ForceOff = N24Blocks.ForceOff = false;
        }

        [TearDown]
        public void Restore()
        {
            N19Blocks.ForceOff = off19;
            N20Blocks.ForceOff = off20;
            N24Blocks.ForceOff = off24;
        }

        static PadReader Reader(bool padY) => new PadReader(cfg) { n19 = true, n20 = true, padY = padY, meleeFire = false /* NIGHT25: NIGHT24's premise (v4); the v5 cases are PadMelee25Tests */, Connected = true };

        static void Frame(PadReader r, double dt, PadButtonName? down = null)
        {
            r.state.Clear();
            if (down.HasValue) r.state.Set(down.Value, true, 1);
            r.Step(dt, true);
        }

        // ---- OFF first ----

        [Test]
        public void Off_WithTheRowOff_TheReaderIsV3()
        {
            var r = new PadReader(cfg) { n19 = true, n20 = true, padY = false }; // the row forced off, whatever the config default is
            Assert.That((r.V3, r.V4, r.LayoutVersion), Is.EqualTo((true, false, 3)));
            Assert.That(r.DefaultLayoutNow, Is.SameAs(GamepadMath.DefaultLayoutV3));
        }

        [Test]
        public void Off24_ForcesV3_EvenWithTheRowOn()
        {
            PadReader r = Reader(true);
            Assert.That(r.LayoutVersion, Is.EqualTo(4));
            N24Blocks.ForceOff = true;
            Assert.That((r.V4, r.LayoutVersion), Is.EqualTo((false, 3)), "--off24 restores v3");
            Assert.That(r.DefaultLayoutNow, Is.SameAs(GamepadMath.DefaultLayoutV3));
            N24Blocks.ForceOff = false;
            N20Blocks.ForceOff = true;
            Assert.That(r.LayoutVersion, Is.EqualTo(2), "--off20 and older still reach v2");
        }

        // ---- the layout ----

        [Test]
        public void TheV4Default_IsYNextWeapon_RbSpecialAndArcline_LbDash_AndNoScrollOnRb()
        {
            PadLayout v4 = GamepadMath.DefaultLayoutV4;
            Assert.That(GamepadMath.ButtonFor(PadAction.Special, v4), Is.EqualTo(PadButtonName.RB));
            Assert.That(GamepadMath.ButtonFor(PadAction.NextWeapon, v4), Is.EqualTo(PadButtonName.Y));
            Assert.That(GamepadMath.ButtonFor(PadAction.Grapple, v4), Is.EqualTo(PadButtonName.RB), "the grapple stays bound (REQUIRED)");
            foreach (PadLayout l in new[] { GamepadMath.DefaultLayout, GamepadMath.DefaultLayoutV2, GamepadMath.DefaultLayoutV3, v4 })
                Assert.That(GamepadMath.ButtonFor(PadAction.Dash, l), Is.EqualTo(PadButtonName.LB), "LB is DASH in every default layout");
            Assert.That(v4.SameAs(GamepadMath.DefaultLayoutV2), Is.True, "v4 puts the rows back as NIGHT19's v2 had them");
            Assert.That(GamepadMath.DefaultForVersion(4), Is.SameAs(v4));
            Assert.That(GamepadMath.DefaultForVersion(3), Is.SameAs(GamepadMath.DefaultLayoutV3));
        }

        [Test]
        public void ASavedV3_MigratesToV4_TheDefaultToTheDefault_ACustomOneKeepsItsPicks()
        {
            Assert.That(GamepadMath.MigrateV3(GamepadMath.DefaultLayoutV3).SameAs(GamepadMath.DefaultLayoutV4), Is.True);
            PadLayout custom = GamepadMath.RebindPad(GamepadMath.DefaultLayoutV3, PadAction.Jump, PadButtonName.B).layout;
            PadLayout m = GamepadMath.MigrateV3(custom);
            Assert.That(GamepadMath.ButtonFor(PadAction.Jump, m), Is.EqualTo(PadButtonName.B), "the player's pick stays");
            Assert.That(GamepadMath.ButtonFor(PadAction.NextWeapon, m), Is.EqualTo(PadButtonName.Y), "RB and Y still held the v3 default: traded");
            // a v3 whose RB or Y the player re-picked is left as it is
            PadLayout mine = GamepadMath.RebindPad(GamepadMath.DefaultLayoutV3, PadAction.NextWeapon, PadButtonName.X).layout;
            Assert.That(GamepadMath.MigrateV3(mine).SameAs(mine), Is.True, "a customised RB / Y stays");
        }

        [Test]
        public void TheV4Store_WinsOverV3_NothingStoredIsTheDefault_AV3StoredIsMigratedOnce()
        {
            PadLayout v4 = GamepadMath.DefaultLayoutV4;
            string v3Json = GamepadMath.PadLayoutV3Json(GamepadMath.DefaultLayoutV3);
            Assert.That(GamepadMath.LoadPadLayoutV4(null, null, null, null, out bool m0).SameAs(v4), Is.True);
            Assert.That(m0, Is.False, "nothing stored: nothing to write");
            Assert.That(GamepadMath.LoadPadLayoutV4(null, v3Json, null, null, out bool m1).SameAs(v4), Is.True);
            Assert.That(m1, Is.True, "a stored v3 is migrated and the caller writes v4");
            PadLayout custom = GamepadMath.RebindPad(v4, PadAction.Jump, PadButtonName.B).layout;
            string v4Json = GamepadMath.PadLayoutV4Json(custom);
            Assert.That(GamepadMath.LoadPadLayoutV4(v4Json, v3Json, null, null, out bool m2).SameAs(custom), Is.True, "v4 wins over v3");
            Assert.That(m2, Is.False);
            Assert.That(GamepadMath.LoadPadLayoutV4(v3Json, null, null, null, out _).SameAs(v4), Is.True, "another version under the v4 key reads as missing");
            Assert.That(GamepadMath.LoadPadLayoutV4("{not json", null, null, null, out _).SameAs(v4), Is.True);
            // a NIGHT20 v3 store from a v2 / v1 chain migrates through both steps
            Assert.That(GamepadMath.LoadPadLayoutV4(null, null, GamepadMath.PadLayoutV2Json(GamepadMath.DefaultLayoutV2), null, out bool m3).SameAs(v4), Is.True);
            Assert.That(m3, Is.True);
        }

        // ---- through the reader ----

        [Test]
        public void On_YTap_IsNextWeaponOnTheRelease_YHeldWheelHold_OpensTheWheel_RbIsTheSpecial()
        {
            PadReader r = Reader(true);
            r.layout = GamepadMath.DefaultLayoutV4;
            double dt = 1.0 / 60, hold = cfg.Num("design.pad.n20.wheelHold");
            Assert.That(r.WheelHoldNow, Is.EqualTo(hold), "v4 keeps design.pad.n20.wheelHold");
            Assert.That(r.NextHoldsWheel, Is.True);
            Frame(r, dt, PadButtonName.Y);
            Frame(r, dt, PadButtonName.Y);
            Frame(r, dt);
            Assert.That(r.commands.nextWeapon, Is.True, "a Y tap is NEXT WEAPON on the release");
            Assert.That(r.commands.wheelOpen, Is.False);
            Frame(r, dt, PadButtonName.Y);
            bool opened = false;
            for (int f = 0; f < 40 && !opened; f++) // 40 frames of 1/60 s: well past wheelHold (0.25 s)
            {
                Frame(r, dt, PadButtonName.Y);
                opened = r.commands.wheelOpen;
                Assert.That(r.commands.nextWeapon, Is.False, "no NEXT WEAPON while held");
            }
            Assert.That(opened, Is.True, "a hold of wheelHold opens the wheel");
            Frame(r, dt);
            Assert.That(r.commands.wheelClose && !r.commands.nextWeapon, Is.True);
            // RB: SPECIAL + ARCLINE, never a weapon command
            Frame(r, dt, PadButtonName.RB);
            uint want = GamepadMath.Bit(PadAction.Special) | GamepadMath.Bit(PadAction.Grapple);
            Assert.That(r.frame.latched & want, Is.EqualTo(want), "RB latches SPECIAL and the grapple");
            Assert.That(r.commands.nextWeapon || r.commands.wheelOpen, Is.False);
            // LB: DASH
            Frame(r, dt);
            Frame(r, dt, PadButtonName.LB);
            Assert.That(r.frame.latched & GamepadMath.Bit(PadAction.Dash), Is.Not.EqualTo(0u), "LB stays DASH");
        }

        [Test]
        public void On_AYTapThatMovesTheSlot_EmitsWeaponSwitch_OnlyOnV4()
        {
            var move = new InputReader(cfg.movement);
            var combat = new CombatReader();
            combat.Init(cfg, null);
            PadReader pad = Reader(true);
            pad.layout = GamepadMath.DefaultLayoutV4;
            var bus = new EventBus();
            var got = new List<WeaponSwitchEvent>();
            bus.On(WeaponSwitchEvent.Name, o => got.Add((WeaponSwitchEvent)o));
            var input = new ReaderInput(move, combat, pad, null) { events = bus };
            const int n = 4;
            double dt = 1.0 / 60;
            int from = combat.select.weapon;
            Frame(pad, dt, PadButtonName.Y);
            input.ApplyPad(dt, n);
            Frame(pad, dt, PadButtonName.Y);
            input.ApplyPad(dt, n);
            Assert.That(got, Is.Empty, "held: not yet");
            Frame(pad, dt);
            input.ApplyPad(dt, n);
            Assert.That(got.Count, Is.EqualTo(1), "the release frame");
            Assert.That((got[0].fromSlot, got[0].toSlot), Is.EqualTo((from, (from + 1) % n)));
            Assert.That(combat.select.weapon, Is.EqualTo((from + 1) % n));

            // v3 (the row off): the same code path announces nothing
            PadReader v3 = Reader(false);
            v3.layout = GamepadMath.DefaultLayoutV3;
            var input3 = new ReaderInput(move, combat, v3, null) { events = bus };
            Frame(v3, dt, PadButtonName.RB);
            input3.ApplyPad(dt, n);
            Frame(v3, dt, PadButtonName.RB);
            input3.ApplyPad(dt, n);
            Frame(v3, dt);
            input3.ApplyPad(dt, n);
            Assert.That(got.Count, Is.EqualTo(1), "v3's RB scroll is no weaponSwitch");
        }
    }
}
