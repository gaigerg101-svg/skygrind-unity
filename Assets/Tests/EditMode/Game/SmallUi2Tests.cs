// NIGHT16 polish #54-#57 (builder U): the screen fades' triggers (menus.js :283, :1146-1154), the focused button's label
// (menus.js :201 / :205), PAUSED's YOUR KEYS on a pad (menus.js :1001-1003), disabled buttons (menus.js :203 / :417 / :670).
// Pure rules only: ScreenFade / NavLook / PauseKeysMode (UiFeelHud.cs).
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class SmallUi2Tests
    {
        [Test]
        public void EveryNewRunLiftsTheCurtainAndEveryReturnToTitleFadesItIn()
        {
            // a new run (PLAY, R, RESTART, RETRY, RETRY SAME SEED) while the UI shows the run: the curtain
            Assert.AreEqual(ScreenFade.Kind.Curtain, ScreenFade.For(Flow.Title, Flow.Playing, true));
            Assert.AreEqual(ScreenFade.Kind.Curtain, ScreenFade.For(Flow.Results, Flow.Playing, true));
            Assert.AreEqual(ScreenFade.Kind.Curtain, ScreenFade.For(Flow.Paused, Flow.Playing, true));
            Assert.AreEqual(ScreenFade.Kind.None, ScreenFade.For(Flow.Title, Flow.Title, true), "the attract's runs: TITLE stays (UiFlow)");
            // QUIT TO TITLE (and the end of a run's screens): TITLE fades in
            Assert.AreEqual(ScreenFade.Kind.Title, ScreenFade.For(Flow.Paused, Flow.Title, false));
            Assert.AreEqual(ScreenFade.Kind.Title, ScreenFade.For(Flow.Results, Flow.Title, false));
            Assert.AreEqual(ScreenFade.Kind.None, ScreenFade.For(Flow.Title, Flow.Title, false), "no change: nothing");
            Assert.AreEqual(ScreenFade.Kind.None, ScreenFade.For(Flow.Playing, Flow.Paused, false), "pausing: no fade");
            Assert.AreEqual(ScreenFade.Kind.None, ScreenFade.For(Flow.Playing, Flow.Down, false), "DOWN has its own");
        }

        [Test]
        public void AFocusedMenuButtonsLabelTurnsCyanAndADisabledOneIsDimmedAndSkipped()
        {
            Assert.IsTrue(NavLook.TintsLabel(true, true), "a menu button, enabled");
            Assert.IsFalse(NavLook.TintsLabel(false, true), "a small chip or a settings row keeps its own colours");
            Assert.IsFalse(NavLook.TintsLabel(true, false), "disabled: never focused");
            Assert.AreEqual(1f, NavLook.Alpha(true), 1e-6);
            Assert.AreEqual(0.35f, NavLook.Alpha(false), 1e-6); // menus.js :203 opacity 0.35
            // menus.js :417: REROLL is disabled with no reroll left or when it can't be paid
            Assert.IsTrue(NavLook.RerollEnabled(1, true));
            Assert.IsFalse(NavLook.RerollEnabled(0, true));
            Assert.IsFalse(NavLook.RerollEnabled(2, false));
        }

        [Test]
        public void PausedListsThePadsButtonsAfterAPadInput()
        {
            Assert.IsTrue(PauseKeysMode.Pad(InputDevice.Pad));
            Assert.IsFalse(PauseKeysMode.Pad(InputDevice.KeyboardMouse));
        }
    }
}
