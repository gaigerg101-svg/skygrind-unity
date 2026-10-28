// NIGHT20 row 6 (builder P; NIGHT20.md U3-34.4 beyond: "a HOLD / TOGGLE row for L3 in CONTROLS ... CONTROLS draws glyphs,
// not words"; Research/2026-10-04-controller.md checklist rows 2 and 12): CONTROLS' GAMEPAD page with the pad's drawn
// glyphs (design.pad.n19 live): the swap note ("<RB> SWAPPED WITH <Y>"), the page's hint (<A> SELECT ... <B> BACK), the
// sticks' rows (MOVE <left stick>, LOOK <right stick>), and the L3 SPRINT row: TOGGLE or HOLD, a setting
// (UnityTuning.PadSprintModeSetting, its default design.pad.n19.l3Sprint) that the input layer reads (ReaderInput.ApplySettings
// -> PadReader.sprintToggle). With the block off: NIGHT18's words.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class ControlsPad20Tests
    {
        static string PortRoot()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        static GameConfig cfg;
        static GameConfig Cfg() => cfg ??= GameConfig.Load(Path.Combine(PortRoot(), "config.json"));

        InputDevice last;
        GlyphSet glyphs;
        PadLayout layout;
        bool n19;

        [SetUp]
        public void Save()
        {
            last = PlayerDevices.Last;
            glyphs = PlayerDevices.Glyphs;
            layout = PlayerDevices.Layout;
            n19 = PlayerDevices.N19;
        }

        [TearDown]
        public void Restore()
        {
            PlayerDevices.Last = last;
            PlayerDevices.Glyphs = glyphs;
            PlayerDevices.Layout = layout;
            PlayerDevices.N19 = n19;
            PromptText.ForceSprites = false;
        }

        static void Pad(bool v2)
        {
            PlayerDevices.N19 = v2;
            PlayerDevices.Last = InputDevice.Pad;
            PlayerDevices.Glyphs = GlyphSet.Xbox;
            PlayerDevices.Layout = GamepadMath.DefaultLayoutV3;
            PromptText.ForceSprites = true;
        }

        [Test]
        public void TheGamepadPage_DrawsGlyphs_TheSwapNoteTheHintAndTheSticks()
        {
            Pad(true);
            string rb = PromptText.Button(PadButtonName.RB, GlyphSet.Xbox);
            Assert.That(ControlsPad20.Glyph(PadButtonName.RB, GlyphSet.Xbox, true), Is.EqualTo(rb), "the swap note's buttons: drawn");
            Assert.That(PromptText.IsSprite(rb), Is.True);
            string hint = ControlsPad20.HintLine(true);
            StringAssert.Contains(PromptText.Button(PadButtonName.A, GlyphSet.Xbox), hint, "A drawn");
            StringAssert.Contains(PromptText.Button(PadButtonName.B, GlyphSet.Xbox), hint, "B drawn");
            List<(string what, string glyph)> sticks = ControlsPad20.StickRows();
            Assert.That(sticks.Count, Is.EqualTo(2));
            Assert.That(sticks[0], Is.EqualTo(("MOVE", PromptText.Glyph(PromptText.LeftStick, GlyphSet.Xbox, false))));
            Assert.That(sticks[1], Is.EqualTo(("LOOK", PromptText.Glyph(PromptText.RightStick, GlyphSet.Xbox, false))));
        }

        [Test]
        public void TheBlockOff_IsNight18sWords()
        {
            Pad(false);
            Assert.That(ControlsPad20.Glyph(PadButtonName.RB, GlyphSet.Xbox, false), Is.EqualTo(Glyphs.Button(PadButtonName.RB, GlyphSet.Xbox)));
            Assert.That(ControlsPad20.HintLine(false), Is.EqualTo(UnityTuning.ControlsPadHint18), "the old line");
        }

        static SettingsStore Store()
        {
            var extras = new List<(SettingItem, object)> { (ControlsPad20.SprintModeItem(), ControlsPad20.SprintModeDefault(Cfg())) };
            var s = new SettingsStore(Cfg(), extras, new List<(string, string)>());
            s.Load("");
            return s;
        }

        [Test]
        public void TheL3Row_TogglesBetweenToggleAndHold_TheInputLayerReadsIt()
        {
            SettingsStore s = Store();
            Assert.That(s.Str(UnityTuning.PadSprintModeSetting), Is.EqualTo(Cfg().Str("design.pad.n19.l3Sprint")), "the default: GG's L3 SPRINT switch");
            var pad = new PadReader(Cfg());
            var reader = new ReaderInput(new InputReader(Cfg().movement), new CombatReader(), pad, null);
            reader.ApplySettings(s);
            Assert.That(pad.sprintToggle, Is.True, "TOGGLE");
            Assert.That(ControlsPad20.SprintModeLabel(s), Is.EqualTo(UnityTuning.PadSprintToggleLabel));
            s.Set(UnityTuning.PadSprintModeSetting, ControlsPad20.NextSprintMode(s));
            reader.ApplySettings(s);
            Assert.That(pad.sprintToggle, Is.False, "the row flipped it to HOLD");
            Assert.That(ControlsPad20.SprintModeLabel(s), Is.EqualTo(UnityTuning.PadSprintHoldLabel));
            s.Set(UnityTuning.PadSprintModeSetting, ControlsPad20.NextSprintMode(s));
            reader.ApplySettings(s);
            Assert.That(pad.sprintToggle, Is.True, "and back");
        }
    }
}
