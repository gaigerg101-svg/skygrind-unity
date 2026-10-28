// NIGHT15 U3-28.8 (builder K): the pad's gaps (Game/Input: PadReader, WeaponSelect, ReaderInput, Glyphs, Rumble; the
// numbers are Design/unity-config.json design.pad, read through GameConfig):
// - the weapon wheel on the pad: the NEXT WEAPON button (Y) held design.pad.wheelHold s opens it (a shorter tap stays NEXT
//   WEAPON, fired on the release); a button a saved layout binds to WEAPON WHEEL opens it at once (NEXT WEAPON then fires
//   on its press, as the web's); without the design block the pad is the web's (no wheel, NEXT WEAPON on the press);
// - the right stick past design.pad.wheelStickDeadzone points at a sector (ArsenalMath.RadialSector, as the mouse does),
//   back inside it keeps the pick; the release takes the pick; while the wheel is open the stick doesn't turn the view;
// - the Nintendo glyph set (A / B and X / Y at swapped positions) chosen by the device's names, Neutral the fallback;
// - the rumble: VIBRATION starts as design.pad.vibrationDefault, RUMBLE (settings.padRumble) scales the motors.
// Driven through PadReader.Step (the state filled by hand) and ReaderInput.ApplyPad: no device, no Input System type here.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class PadWheelTests
    {
        static GameConfig cfg, web;

        static string ConfigPath => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport", "config.json"));

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(ConfigPath);
            string none = Path.Combine(Path.GetTempPath(), "skygrind-padwheel-no-design.json");
            Assert.That(File.Exists(none), Is.False);
            web = GameConfig.Load(ConfigPath, none); // config.json alone: the web's game
        }

        /// <summary>One rendered frame of `dt` s with `down` held (none: every button up) and the right stick at (rx, ry) (web axes: y down).</summary>
        static void Frame(PadReader r, double dt, PadButtonName? down = null, double rx = 0, double ry = 0)
        {
            r.state.Clear();
            if (down.HasValue) r.state.Set(down.Value, true, 1);
            r.state.axes[2] = rx;
            r.state.axes[3] = ry;
            r.Step(dt, true);
        }

        static bool Listed(PadReader r, PadAction a)
        {
            for (int i = 0; i < r.pressedCount; i++)
                if (r.pressed[i] == a) return true;
            return false;
        }

        [Test]
        public void HoldingNextWeapon_OpensTheWheelAfterWheelHold_ATapStaysNextWeapon_FiredOnTheRelease()
        {
            double hold = cfg.Num("design.pad.wheelHold");
            double dt = hold * 0.3;
            var r = new PadReader(cfg);
            Assert.That(r.NextHoldsWheel, Is.True, "the default layout: Y carries NEXT WEAPON and holds the wheel");

            // A tap: Y down for the press frame and one more (0.3 x wheelHold), then up.
            Frame(r, dt, PadButtonName.Y);
            Assert.That(r.commands.nextWeapon || Listed(r, PadAction.NextWeapon), Is.False, "the press waits (a hold may open the wheel)");
            Frame(r, dt, PadButtonName.Y);
            Assert.That(r.commands.wheelOpen || r.commands.nextWeapon, Is.False);
            Frame(r, dt);
            Assert.That(r.commands.nextWeapon && Listed(r, PadAction.NextWeapon), Is.True, "a tap is NEXT WEAPON, on the release");
            Assert.That(r.commands.wheelClose, Is.False, "a tap never opened the wheel");

            // A hold: the press frame starts the clock; the wheel opens on the frame the hold reaches wheelHold, once.
            Frame(r, dt, PadButtonName.Y);
            int opened = -1, edges = 0;
            for (int f = 1; f <= 8; f++)
            {
                Frame(r, dt, PadButtonName.Y);
                if (r.commands.wheelOpen)
                {
                    edges++;
                    if (opened < 0) opened = f;
                }
                Assert.That(r.commands.nextWeapon || Listed(r, PadAction.NextWeapon), Is.False, $"frame {f}: no NEXT WEAPON while held");
            }
            Assert.That((opened, edges), Is.EqualTo((4, 1)), "open once, at 4 x 0.3 x wheelHold (the first frame past wheelHold)");
            Frame(r, dt);
            Assert.That(r.commands.wheelClose, Is.True, "the release lets the wheel go");
            Assert.That(r.commands.nextWeapon || Listed(r, PadAction.NextWeapon), Is.False, "a hold is no NEXT WEAPON");
        }

        [Test]
        public void AWheelButton_OpensAtOnce_ThenNextWeaponFiresOnItsPress_AsTheWebs()
        {
            // A saved layout may bind WEAPON WHEEL (it is in no default): here R3 (LOCK-ON's) carries it.
            var r = new PadReader(cfg) { layout = GamepadMath.DefaultLayout.With(PadButtonName.RS, PadAction.Wheel) };
            Assert.That(r.NextHoldsWheel, Is.False, "the wheel has its own button");
            Frame(r, 0.01, PadButtonName.RS);
            Assert.That(r.commands.wheelOpen, Is.True, "the wheel's button opens it on the press");
            Frame(r, 0.01, PadButtonName.RS);
            Assert.That(r.commands.wheelOpen || r.commands.wheelClose, Is.False, "edges only");
            Frame(r, 0.01);
            Assert.That(r.commands.wheelClose, Is.True, "its release lets go");
            Frame(r, 0.01, PadButtonName.Y);
            Assert.That(r.commands.nextWeapon && Listed(r, PadAction.NextWeapon), Is.True, "NEXT WEAPON on the press");

            // The NEXT WEAPON button carrying the wheel too is the default's hold.
            var both = new PadReader(cfg) { layout = GamepadMath.DefaultLayout.With(PadButtonName.Y, PadAction.NextWeapon, PadAction.Wheel) };
            Assert.That(both.NextHoldsWheel, Is.True);
        }

        [Test]
        public void WithoutTheDesignBlock_ThePadIsTheWebs_NoWheel_NextWeaponOnThePress()
        {
            Assert.That(web.Has("design.pad"), Is.False);
            var r = new PadReader(web);
            Assert.That(r.NextHoldsWheel, Is.False);
            Frame(r, 0.1, PadButtonName.Y);
            Assert.That(r.commands.nextWeapon && Listed(r, PadAction.NextWeapon), Is.True, "gamepad.js: NEXT WEAPON on the press");
            for (int f = 0; f < 10; f++)
            {
                Frame(r, 0.1, PadButtonName.Y);
                Assert.That(r.commands.wheelOpen, Is.False, "no wheel");
            }
            var bound = new PadReader(web) { layout = GamepadMath.DefaultLayout.With(PadButtonName.RS, PadAction.Wheel) };
            Frame(bound, 0.1, PadButtonName.RS);
            Assert.That(bound.commands.wheelOpen, Is.False, "a bound WEAPON WHEEL does nothing without design.pad");
        }

        [Test]
        public void TheWheel_TheStickPoints_BackInsideItsDeadzoneKeepsThePick_TheReleaseTakesIt_TheViewHoldsStill()
        {
            var move = new InputReader(cfg.movement);
            var combat = new CombatReader();
            combat.Init(cfg, null);
            var pad = new PadReader(cfg) { Connected = true };
            var input = new ReaderInput(move, combat, pad, null);
            const int n = 4; // slot 0 up, 1 right, 2 down, 3 left
            double dt = cfg.Num("design.pad.wheelHold") * 0.3, maxOffset = cfg.Num("weapons.radial.maxOffset");
            void F(bool y, double rx, double ry)
            {
                Frame(pad, dt, y ? PadButtonName.Y : (PadButtonName?)null, rx, ry);
                input.ApplyPad(dt, n);
            }

            double yaw0 = move.yaw;
            F(false, 1, 0);
            Assert.That(move.yaw, Is.Not.EqualTo(yaw0), "the wheel shut: the right stick turns the view");
            F(false, 0, 0);

            for (int f = 0; f < 5; f++) F(true, 0, 0);
            Assert.That(combat.radial.open && combat.radial.pad, Is.True, "held past wheelHold: the pad's wheel is open (the HUD reads combat.radial)");
            Assert.That(combat.radial.highlighted, Is.EqualTo(-1), "nothing pointed yet");

            double yaw1 = move.yaw, pitch1 = move.pitch;
            F(true, 1, 0);
            Assert.That(combat.radial.highlighted, Is.EqualTo(1), "right: slot 1");
            Assert.That((combat.radial.x, combat.radial.y), Is.EqualTo((maxOffset, 0.0)), "the pointer is the stick's tilt x radial.maxOffset");
            Assert.That((move.yaw, move.pitch), Is.EqualTo((yaw1, pitch1)), "the wheel open: the stick doesn't turn the view");
            F(true, 0, 1);
            Assert.That(combat.radial.highlighted, Is.EqualTo(2), "down: slot 2");
            F(true, 0.3, 0.2);
            Assert.That(combat.radial.highlighted, Is.EqualTo(2), "back inside design.pad.wheelStickDeadzone: the pick stays");
            Assert.That((move.yaw, move.pitch), Is.EqualTo((yaw1, pitch1)), "still held");

            F(false, 0.3, 0.2);
            Assert.That(combat.radial.open, Is.False, "released");
            Assert.That((combat.select.weapon, combat.select.Consume()), Is.EqualTo((2, 2)), "the release takes slot 2 (and no NEXT WEAPON after it)");

            // A tap afterwards is NEXT WEAPON (2 -> 3).
            F(true, 0, 0);
            F(false, 0, 0);
            Assert.That(combat.select.weapon, Is.EqualTo(3), "a tap: NEXT WEAPON");
        }

        [Test]
        public void PointRadial_TheStickPastItsDeadzonePicks_ThePointerClamps_TheCloseClearsThePadFlag()
        {
            WeaponSelect s = WeaponSelect.From(cfg);
            double maxOffset = cfg.Num("weapons.radial.maxOffset"), dz = cfg.Num("design.pad.wheelStickDeadzone");
            Assert.That(s.PointRadial(0, -1, dz, 8), Is.False, "shut: nothing");
            Assert.That(s.OpenRadial(8, pad: true), Is.True);
            Assert.That(s.radial.pad, Is.True);
            Assert.That(s.PointRadial(0, -1, dz, 8), Is.True);
            Assert.That((s.radial.highlighted, s.radial.x, s.radial.y), Is.EqualTo((0, 0.0, -maxOffset)), "up: sector 0");
            Assert.That(s.PointRadial(dz * 0.9, 0, dz, 8), Is.False, "inside the stick's dead zone");
            Assert.That(s.radial.highlighted, Is.EqualTo(0), "the pick stays");
            s.PointRadial(0.7071, 0.7071, dz, 8);
            Assert.That(s.radial.highlighted, Is.EqualTo(3), "down-right: sector 3 of 8");
            s.PointRadial(2, 0, dz, 8);
            Assert.That(JsMath.Hypot(s.radial.x, s.radial.y), Is.LessThanOrEqualTo(maxOffset + 1e-9), "clamped to radial.maxOffset");
            Assert.That(s.radial.highlighted, Is.EqualTo(2));
            Assert.That(s.CloseRadial(true, 8), Is.True);
            Assert.That((s.radial.open, s.radial.pad, s.weapon, s.Consume()), Is.EqualTo((false, false, 2, 2)));
        }

        [Test]
        public void TheNintendoSet_ByTheDevicesName_SwapsAAndB_XAndY_NeutralTheFallback()
        {
            Assert.That(Glyphs.ForDevice(false, false, "SwitchProControllerHID", "Switch Pro Controller", "Pro Controller", "Nintendo Co., Ltd."), Is.EqualTo(GlyphSet.Nintendo));
            Assert.That(Glyphs.ForDevice(false, false, "Gamepad", "Wireless Gamepad", "Joy-Con (L/R)", null), Is.EqualTo(GlyphSet.Nintendo), "Joy-Con by its product name");
            Assert.That(Glyphs.ForDevice(false, false, "Gamepad", "Smoke pad", null, null), Is.EqualTo(GlyphSet.Neutral), "an unknown pad: the web's neutral set");
            Assert.That(Glyphs.ForDevice(false, false, null, null, null, null), Is.EqualTo(GlyphSet.Neutral));
            Assert.That(Glyphs.ForDevice(true, false, "XInputControllerWindows", "Xbox Controller", null, null), Is.EqualTo(GlyphSet.Xbox));
            Assert.That(Glyphs.ForDevice(false, true, "DualSenseGamepadHID", "DualSense Wireless Controller", null, "Sony Interactive Entertainment"), Is.EqualTo(GlyphSet.PlayStation));

            // By position (the Input System's south / east / west / north): Nintendo prints B / A / Y / X there.
            Assert.That(
                (Glyphs.Button(PadButtonName.A, GlyphSet.Nintendo), Glyphs.Button(PadButtonName.B, GlyphSet.Nintendo), Glyphs.Button(PadButtonName.X, GlyphSet.Nintendo), Glyphs.Button(PadButtonName.Y, GlyphSet.Nintendo)),
                Is.EqualTo(("B", "A", "Y", "X")));
            Assert.That((Glyphs.Button(PadButtonName.LT, GlyphSet.Nintendo), Glyphs.Button(PadButtonName.RT, GlyphSet.Nintendo)), Is.EqualTo(("ZL", "ZR")));
            Assert.That(Glyphs.Button(PadButtonName.A, GlyphSet.Neutral), Is.EqualTo("A"), "the neutral set is unchanged");
            Assert.That(Glyphs.Label("jump", InputDevice.Pad, GlyphSet.Nintendo, null, GamepadMath.DefaultLayout), Is.EqualTo("B"), "JUMP (the south button) reads B");
            Assert.That(Glyphs.Label("nextWeapon", InputDevice.Pad, GlyphSet.Nintendo, null, GamepadMath.DefaultLayout), Is.EqualTo("X"), "NEXT WEAPON (the north button) reads X");
        }

        [Test]
        public void TheRumble_StartsAsDesignPadSays_RumbleScalesTheMotors_TheWebsWithoutTheBlock()
        {
            Assert.That(cfg.Bool("design.pad.vibrationDefault"), Is.True, "the design turns VIBRATION on");
            var rumble = new Rumble(cfg);
            try
            {
                Assert.That(rumble.enabled, Is.True, "on before the settings apply (design.pad.vibrationDefault)");
                Assert.That(rumble.strength, Is.EqualTo(cfg.Num("design.pad.rumbleDefault")));
                Assert.That(Rumble.Scale(0.6, 0.5), Is.EqualTo(0.3).Within(1e-12), "config.gamepad.vibration's strength x RUMBLE");
                Assert.That(Rumble.Scale(0.6, 0), Is.EqualTo(0));
                Assert.That(Rumble.Scale(0.8, 1), Is.EqualTo(0.8));

                var store = new SettingsStore(cfg, new List<(SettingItem, object)>
                {
                    (new SettingItem { key = UnityTuning.PadRumbleSetting, tab = "pad", label = UnityTuning.PadRumbleLabel, type = "range", min = 0, max = 1, step = UnityTuning.PadRumbleStep }, 1.0),
                });
                store.Set(UnityTuning.PadRumbleSetting, 0.3);
                store.Set("padVibration", true);
                var input = new ReaderInput(new InputReader(cfg.movement), new CombatReader(), new PadReader(cfg), rumble);
                input.ApplySettings(store);
                Assert.That((rumble.enabled, rumble.strength), Is.EqualTo((true, 0.3)), "VIBRATION and RUMBLE from the settings");
                store.Set("padVibration", false);
                input.ApplySettings(store);
                Assert.That(rumble.enabled, Is.False);
            }
            finally
            {
                rumble.Dispose();
            }

            var webRumble = new Rumble(web);
            try
            {
                Assert.That((webRumble.enabled, webRumble.strength), Is.EqualTo((web.Bool("settings.padVibration"), 1.0)), "the web's: VIBRATION as config.settings, full strength");
            }
            finally
            {
                webRumble.Dispose();
            }
        }
    }
}
