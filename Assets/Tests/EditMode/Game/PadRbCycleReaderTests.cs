// NIGHT20 U3-34.4 (builder P): the pad's RB on layout v3 (design.pad.n20) through the reader, the weapon select and the
// prompts (Core's layout and its migration: PadRbCycleTests):
// - a tap of RB is NEXT WEAPON, fired on the release (the hold may open the wheel), and the weapon select moves on that
//   frame (the hotbar reads it); held design.pad.n20.wheelHold s it opens the pad's wheel once, the release lets it go,
//   and no NEXT WEAPON comes with a hold; three taps scroll three slots;
// - Y is SPECIAL + ARCLINE (frame buttons, latched for the steps), never a weapon command, never the wheel;
// - the reader's layout version: 3 with design.pad.n19 and design.pad.n20 on, 2 with `--off20` (N20Blocks.ForceOff), 1 with
//   `--off19` (N19Blocks.ForceOff); the RESET default follows it;
// - the prompts follow the live layout: the hotbar's cycle line and the WHEEL prompt draw RB, SPECIAL draws Y; CONTROLS'
//   NEXT WEAPON cell draws RB.
// Driven through PadReader.Step (the state filled by hand) and ReaderInput.ApplyPad: no device, no Input System type.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class PadRbCycleReaderTests
    {
        static GameConfig cfg;

        static string Root()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (System.Exception) { return Path.GetFullPath("."); } // a plain .NET runner: the working folder is the project
        }

        InputDevice last;
        GlyphSet glyphs;
        PadLayout layout;
        bool n19, off19, off20;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Root(), "PortExport", "config.json"));

        [SetUp]
        public void Save()
        {
            last = PlayerDevices.Last;
            glyphs = PlayerDevices.Glyphs;
            layout = PlayerDevices.Layout;
            n19 = PlayerDevices.N19;
            off19 = N19Blocks.ForceOff;
            off20 = N20Blocks.ForceOff;
            N19Blocks.ForceOff = N20Blocks.ForceOff = false;
        }

        [TearDown]
        public void Restore()
        {
            PlayerDevices.Last = last;
            PlayerDevices.Glyphs = glyphs;
            PlayerDevices.Layout = layout;
            PlayerDevices.N19 = n19;
            PromptText.ForceSprites = false;
            N19Blocks.ForceOff = off19;
            N20Blocks.ForceOff = off20;
        }

        /// <summary>A reader on the v3 layout (both pad blocks on, design.n24.padY off, whatever the config says: NIGHT24 turned the padY row on, which makes the config default a v4 reader).</summary>
        static PadReader V3() => new PadReader(cfg) { n19 = true, n20 = true, padY = false, layout = GamepadMath.DefaultLayoutV3, Connected = true };

        static void Frame(PadReader r, double dt, PadButtonName? down = null)
        {
            r.state.Clear();
            if (down.HasValue) r.state.Set(down.Value, true, 1);
            r.Step(dt, true);
        }

        static bool Listed(PadReader r, PadAction a)
        {
            for (int i = 0; i < r.pressedCount; i++)
                if (r.pressed[i] == a) return true;
            return false;
        }

        [Test]
        public void RbTap_IsNextWeaponOnTheRelease_RbHeldWheelHold_OpensTheWheelOnce()
        {
            double hold = cfg.Num("design.pad.n20.wheelHold");
            double dt = hold * 0.3;
            PadReader r = V3();
            Assert.That(r.WheelHoldNow, Is.EqualTo(hold), "v3 reads design.pad.n20.wheelHold");
            Assert.That(r.NextHoldsWheel, Is.True, "RB carries NEXT WEAPON and holds the wheel");

            Frame(r, dt, PadButtonName.RB);
            Assert.That(r.commands.nextWeapon || Listed(r, PadAction.NextWeapon), Is.False, "the press waits (a hold may open the wheel)");
            Assert.That(r.frame.latched & (GamepadMath.Bit(PadAction.Special) | GamepadMath.Bit(PadAction.Grapple)), Is.EqualTo(0u), "RB is no special now");
            Frame(r, dt, PadButtonName.RB);
            Frame(r, dt);
            Assert.That(r.commands.nextWeapon && Listed(r, PadAction.NextWeapon), Is.True, "a tap of RB is NEXT WEAPON, on the release");
            Assert.That(r.commands.wheelOpen || r.commands.wheelClose, Is.False, "a tap never opens the wheel");

            Frame(r, dt, PadButtonName.RB);
            int opened = -1, edges = 0;
            for (int f = 1; f <= 8; f++)
            {
                Frame(r, dt, PadButtonName.RB);
                if (r.commands.wheelOpen)
                {
                    edges++;
                    if (opened < 0) opened = f;
                }
                Assert.That(r.commands.nextWeapon || Listed(r, PadAction.NextWeapon), Is.False, $"frame {f}: no NEXT WEAPON while held");
            }
            Assert.That((opened, edges), Is.EqualTo((4, 1)), "open once, on the first frame past wheelHold");
            Frame(r, dt);
            Assert.That(r.commands.wheelClose, Is.True, "the release lets the wheel go");
            Assert.That(r.commands.nextWeapon || Listed(r, PadAction.NextWeapon), Is.False, "a hold is no NEXT WEAPON");
        }

        [Test]
        public void Y_IsSpecialAndArcline_NeverAWeaponCommand_NeverTheWheel()
        {
            PadReader r = V3();
            Frame(r, 0.1, PadButtonName.Y);
            uint want = GamepadMath.Bit(PadAction.Special) | GamepadMath.Bit(PadAction.Grapple);
            Assert.That(r.frame.latched & want, Is.EqualTo(want), "Y's press latches SPECIAL and the grapple for the steps");
            Assert.That(r.frame.held & want, Is.EqualTo(want), "and holds them");
            for (int f = 0; f < 10; f++)
            {
                Frame(r, 0.1, PadButtonName.Y);
                Assert.That(r.commands.wheelOpen || r.commands.nextWeapon, Is.False, "held 1 s: no wheel, no weapon");
            }
            Frame(r, 0.1);
            Assert.That(r.commands.nextWeapon || r.commands.wheelClose || Listed(r, PadAction.NextWeapon), Is.False, "the release: nothing");
        }

        [Test]
        public void ThreeRbTaps_ScrollThreeSlots_TheSelectMovesOnEachReleaseFrame()
        {
            var move = new InputReader(cfg.movement);
            var combat = new CombatReader();
            combat.Init(cfg, null);
            PadReader pad = V3();
            var input = new ReaderInput(move, combat, pad, null);
            const int n = 4;
            double dt = 1.0 / 60;
            int want = combat.select.weapon;
            for (int press = 0; press < 3; press++)
            {
                Frame(pad, dt, PadButtonName.RB);
                input.ApplyPad(dt, n);
                Frame(pad, dt, PadButtonName.RB);
                input.ApplyPad(dt, n);
                Assert.That(combat.select.weapon, Is.EqualTo(want), $"press {press + 1}: held, not yet");
                Frame(pad, dt);
                input.ApplyPad(dt, n);
                want = (want + 1) % n;
                Assert.That(combat.select.weapon, Is.EqualTo(want), $"press {press + 1}: the release frame scrolls (the hotbar reads it)");
                Assert.That(combat.radial.open, Is.False);
            }
        }

        [Test]
        public void TheLayoutVersion_IsThreeWithBothBlocks_TwoWithOff20_OneWithOff19_TheResetDefaultFollows()
        {
            PadReader r = V3();
            Assert.That((r.V2, r.V3, r.LayoutVersion), Is.EqualTo((true, true, 3)));
            Assert.That(r.DefaultLayoutNow, Is.SameAs(GamepadMath.DefaultLayoutV3));
            N20Blocks.ForceOff = true;
            Assert.That((r.V2, r.V3, r.LayoutVersion), Is.EqualTo((true, false, 2)), "--off20: NIGHT19's v2 pad");
            Assert.That(r.DefaultLayoutNow, Is.SameAs(GamepadMath.DefaultLayoutV2));
            N19Blocks.ForceOff = true;
            Assert.That((r.V2, r.V3, r.LayoutVersion), Is.EqualTo((false, false, 1)), "--off19: NIGHT18's pad");
            Assert.That(r.DefaultLayoutNow, Is.SameAs(GamepadMath.DefaultLayout));
            N19Blocks.ForceOff = N20Blocks.ForceOff = false;
            // NIGHT24: with design.n24.padY on the same reader is layout v4 (its default and store), and --off24 puts v3 back
            var v4 = new PadReader(cfg) { n19 = true, n20 = true, padY = true, meleeFire = false }; // NIGHT25: v4 is the premise; meleeFire on makes v5
            Assert.That((v4.V3, v4.V4, v4.LayoutVersion), Is.EqualTo((true, true, 4)), "design.n24.padY on: v4");
            Assert.That(v4.DefaultLayoutNow, Is.SameAs(GamepadMath.DefaultLayoutV4));
            var v5 = new PadReader(cfg) { n19 = true, n20 = true, padY = true, meleeFire = true }; // NIGHT25 design.n25.meleeFire
            Assert.That((v5.V4, v5.V5, v5.LayoutVersion), Is.EqualTo((true, true, 5)), "design.n25.meleeFire on: v5");
            Assert.That(v5.DefaultLayoutNow, Is.SameAs(GamepadMath.DefaultLayoutV5));
            N24Blocks.ForceOff = true;
            Assert.That(v4.LayoutVersion, Is.EqualTo(3), "--off24: v3");
            N24Blocks.ForceOff = false;
            var v2Only = new PadReader(cfg) { n19 = true, n20 = false };
            Assert.That(v2Only.LayoutVersion, Is.EqualTo(2), "design.pad.n20 off: v2");
            var noV2 = new PadReader(cfg) { n19 = false, n20 = true };
            Assert.That(noV2.LayoutVersion, Is.EqualTo(1), "v3 builds on v2's rules: design.pad.n19 off is v1");
            Assert.That(new PadReader(cfg).n20, Is.EqualTo(cfg.Design("pad.n20.enabled")), "read from the config");
            Assert.That(new PadReader(cfg) { n19 = true, n20 = false }.WheelHoldNow, Is.EqualTo(cfg.Num("design.pad.wheelHold")), "v2: design.pad.wheelHold");
        }

        [Test]
        public void ThePrompts_FollowTheV3Layout_TheCycleLineAndTheWheelDrawRb_SpecialDrawsY()
        {
            PlayerDevices.N19 = true;
            PlayerDevices.Last = InputDevice.Pad;
            PlayerDevices.Glyphs = GlyphSet.Xbox;
            PlayerDevices.Layout = GamepadMath.DefaultLayoutV3;
            PromptText.ForceSprites = true;
            string rb = PromptText.Button(PadButtonName.RB, GlyphSet.Xbox), rbHold = PromptText.Button(PadButtonName.RB, GlyphSet.Xbox, true);
            string cycle = PromptText.For("nextWeapon", false, UnityTuning.HudPadCycleNeutral);
            StringAssert.StartsWith(rb + UnityTuning.PromptCycleNext, cycle, "the hotbar's cycle line: RB NEXT");
            StringAssert.Contains(PromptText.Button(PadButtonName.Left, GlyphSet.Xbox) + UnityTuning.PromptCycleBack, cycle, "d-pad left BACK");
            Assert.That(PromptText.For("weaponWheel", true, "HOLD Y"), Is.EqualTo(rbHold), "the WHEEL prompt: RB held");
            Assert.That(PromptText.For("radial", true, "HOLD TAB"), Is.EqualTo(rbHold));
            Assert.That(PromptText.For("special", false, "F"), Is.EqualTo(PromptText.Button(PadButtonName.Y, GlyphSet.Xbox)), "SPECIAL on Y");
            Assert.That(Glyphs.PadBindGlyph(PadAction.NextWeapon, GamepadMath.DefaultLayoutV3, GlyphSet.Xbox, true), Is.EqualTo(rb), "CONTROLS: NEXT WEAPON on RB");
            Assert.That(Glyphs.PadBindGlyph(PadAction.Grapple, GamepadMath.DefaultLayoutV3, GlyphSet.Xbox, true), Is.EqualTo(PromptText.Button(PadButtonName.Y, GlyphSet.Xbox)), "CONTROLS: ARCLINE on Y");
        }
    }
}
