// NIGHT13 U3-26.7: the web's tests/holdToggle.test.js (OVERNIGHT3 P2: 6 cases) ported against Core/HoldToggle.cs: HOLD
// never touches the frame (every field compared bit for bit, the web's JSON.stringify compare); TOGGLE turns presses into
// a latched hold the Mover reads like a held key.
using System;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class HoldToggleTests
    {
        static InputFrame Fresh() => new InputFrame();

        static InputFrame PressSlide(InputFrame f)
        {
            f.slide = new Button { pressed = true, held = true };
            return f;
        }

        static InputFrame PressGrapple(InputFrame f)
        {
            f.grapple = new Button { pressed = true, held = true };
            return f;
        }

        static InputFrame PressJump(InputFrame f)
        {
            f.jump = new Button { pressed = true, held = true };
            return f;
        }

        static string B(Button b) => (b.held ? "1" : "0") + (b.pressed ? "1" : "0");

        static string D(double d) => BitConverter.DoubleToInt64Bits(d).ToString("x16");

        static string Bits(InputFrame f) =>
            D(f.moveX) + D(f.moveZ) + D(f.yaw) + D(f.pitch) + B(f.jump) + B(f.dash) + B(f.slide) + B(f.grapple) + B(f.reel) + (f.grappleTarget.HasValue ? "t" : "none");

        static InputFrame Apply(InputFrame f, ToggleState state, string slide, string grapple, RopePhase phase = RopePhase.None)
        {
            HoldToggle.ApplyToggles(ref f, state, slide, grapple, phase);
            return f;
        }

        [Test]
        public void Hold_LeavesEveryFrameExactlyAsItCame_TheDefaultIsTodaysGame()
        {
            var state = new ToggleState();
            InputFrame frame = PressGrapple(PressSlide(Fresh()));
            string before = Bits(frame);
            frame = Apply(frame, state, "hold", "hold", RopePhase.Attached);
            Assert.That(Bits(frame), Is.EqualTo(before));
            InputFrame idle = Apply(Fresh(), state, "hold", "hold");
            Assert.That(Bits(idle), Is.EqualTo(Bits(Fresh())));
        }

        [Test]
        public void SlideToggle_APressHoldsTheSlideUntilTheNextPress()
        {
            var state = new ToggleState();
            Assert.That(Apply(PressSlide(Fresh()), state, "toggle", "hold").slide.held, Is.True);
            for (int i = 0; i < 5; i++) Assert.That(Apply(Fresh(), state, "toggle", "hold").slide.held, Is.True); // key up, still sliding
            InputFrame off = Apply(PressSlide(Fresh()), state, "toggle", "hold");
            Assert.That(off.slide.held, Is.False);
            Assert.That(off.slide.pressed, Is.True); // the edge still reaches the Mover (a slam in the air)
            Assert.That(Apply(Fresh(), state, "toggle", "hold").slide.held, Is.False);
        }

        [Test]
        public void SlideToggle_AJumpStandsYouUp_SlideJumpChainsNeedNoExtraPress()
        {
            var state = new ToggleState();
            Apply(PressSlide(Fresh()), state, "toggle", null);
            InputFrame jump = Apply(PressJump(Fresh()), state, "toggle", null);
            Assert.That(jump.slide.held, Is.False);
            Assert.That(jump.jump.pressed, Is.True);
        }

        [Test]
        public void ArclineToggle_Hook_KeepSwingingWithTheKeyUp_TheNextPressLetsGoWithoutANewHook()
        {
            var state = new ToggleState();
            InputFrame fire = Apply(PressGrapple(Fresh()), state, null, "toggle", RopePhase.None);
            Assert.That(fire.grapple.pressed && fire.grapple.held, Is.True);
            Assert.That(Apply(Fresh(), state, null, "toggle", RopePhase.Flying).grapple.held, Is.True);
            Assert.That(Apply(Fresh(), state, null, "toggle", RopePhase.Attached).grapple.held, Is.True);
            InputFrame release = Apply(PressGrapple(Fresh()), state, null, "toggle", RopePhase.Attached);
            Assert.That(release.grapple.held, Is.False);
            Assert.That(release.grapple.pressed, Is.False);
        }

        [Test]
        public void ArclineToggle_OnceTheRopeIsGone_TheNextPressHooksAgain()
        {
            var state = new ToggleState();
            Apply(PressGrapple(Fresh()), state, null, "toggle", RopePhase.None);
            Apply(Fresh(), state, null, "toggle", RopePhase.Attached);
            Assert.That(Apply(Fresh(), state, null, "toggle", RopePhase.None).grapple.held, Is.False); // let go by a jump
            InputFrame again = Apply(PressGrapple(Fresh()), state, null, "toggle", RopePhase.None);
            Assert.That(again.grapple.pressed && again.grapple.held, Is.True);
        }

        [Test]
        public void AMissedHook_FlyingThenNothing_DropsTheLatch()
        {
            var state = new ToggleState();
            Apply(PressGrapple(Fresh()), state, null, "toggle", RopePhase.None);
            Apply(Fresh(), state, null, "toggle", RopePhase.Flying);
            Assert.That(Apply(Fresh(), state, null, "toggle", RopePhase.None).grapple.held, Is.False);
        }
    }
}
