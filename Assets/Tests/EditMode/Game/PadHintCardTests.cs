// NIGHT20 U3-34.4 beyond (builder P): the first-pad hint card. The first time a pad plays (the pad used last, in a run, the
// v3 layout live) the card shows the four verbs a pad player can't guess, each as its live button's glyph and a word:
// L3 SPRINT, R3 MELEE, RB NEXT GUN, LT ZOOM (PromptText: remaps followed), for UnityTuning.PadHintSeconds; once per profile
// (the flag "padHint20"), never in the attract or a bot's run.
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class PadHintCardTests
    {
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

        static void Pad(PadLayout l)
        {
            PlayerDevices.N19 = true;
            PlayerDevices.Last = InputDevice.Pad;
            PlayerDevices.Glyphs = GlyphSet.Xbox;
            PlayerDevices.Layout = l;
            PromptText.ForceSprites = true;
        }

        [Test]
        public void TheCard_ListsSprintMeleeNextGunZoom_AsTheLiveButtons()
        {
            Pad(GamepadMath.DefaultLayoutV3);
            string[] lines = PadHintCard.Lines();
            Assert.That(lines.Length, Is.EqualTo(4));
            string B(PadButtonName b) => PromptText.Button(b, GlyphSet.Xbox);
            Assert.That(lines[0], Is.EqualTo(B(PadButtonName.LS) + " SPRINT"));
            Assert.That(lines[1], Is.EqualTo(B(PadButtonName.RS) + " MELEE"));
            Assert.That(lines[2], Is.EqualTo(B(PadButtonName.RB) + " NEXT GUN"));
            Assert.That(lines[3], Is.EqualTo(B(PadButtonName.LT) + " ZOOM"));
            // A remap is followed: JUMP on RB moves NEXT WEAPON to A.
            Pad(GamepadMath.RebindPad(GamepadMath.DefaultLayoutV3, PadAction.Jump, PadButtonName.RB).layout);
            Assert.That(PadHintCard.Lines()[2], Is.EqualTo(B(PadButtonName.A) + " NEXT GUN"));
        }

        [Test]
        public void Due_TheFirstPadInARun_OncePerProfile_NeverInABotsRunOrOffV3()
        {
            var p = new Profile();
            Assert.That(PadHintCard.Due(p, padLast: true, v3: true, playing: true, botOrAttract: false), Is.True);
            Assert.That(PadHintCard.Due(p, padLast: false, v3: true, playing: true, botOrAttract: false), Is.False, "the keyboard in hand");
            Assert.That(PadHintCard.Due(p, padLast: true, v3: false, playing: true, botOrAttract: false), Is.False, "the v2 layout (design.pad.n20 off)");
            Assert.That(PadHintCard.Due(p, padLast: true, v3: true, playing: false, botOrAttract: false), Is.False, "a menu");
            Assert.That(PadHintCard.Due(p, padLast: true, v3: true, playing: true, botOrAttract: true), Is.False, "the attract or a bot");
            Assert.That(PadHintCard.Seen(p), Is.True, "marked seen: the caller saves the profile");
            Assert.That(PadHintCard.Due(p, padLast: true, v3: true, playing: true, botOrAttract: false), Is.False, "once per profile");
            Assert.That(p.Flag(PadHintCard.Flag), Is.True);
        }
    }
}
