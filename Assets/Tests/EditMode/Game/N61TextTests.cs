// NIGHT61 builder G (C1, design.n61.text61; Game/Text61.cs, Game/UiText61.cs): the centre card's gate (Text61Gate, pure). Pinned:
//   1. the switch: on only with the row on and no off switch: -skygrindOff61 or an older night's turns it off, -skygrindNoText61 turns it
//      alone off; a later night's switch (-skygrindOff62) or another row's (-skygrindNoBossRule61) does not
//   2. level 1 keeps its overlay (GG 2026-10-11, OWNER.md:101): every line passes there, repeats included, nothing waits
//   3. a boss fight (the boss bar up) is hud47's: every line passes, nothing is recorded (the boss's rule line, bossRule61, is the one line a
//      boss fight allows at its start, and this gate never touches it)
//   4. outside level 1, ONE LINE AT A TIME: a new instruction waits while the message slot shows a room title, then passes; past waitMax it
//      passes anyway (Late); the line on the card stays while it is the same line, whatever the slot shows later (no flicker)
//   5. outside level 1, ONCE PER RUN: a line already shown is withheld when it comes again (Repeat, counted once per appearance); Reset (a new
//      run) forgets them; an empty or null line passes through and ends the current one
// Written test-first: RED on its own commit (the placeholder: always off, every line passes). Not run by its author (no Editor here).
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class N61TextTests
    {
        const double Dt = 1.0 / 60, Wait = 3.0;
        const string A = "Midair, aim down, then {fire}.", B = "Hold {fire} grounded for a slug.";

        [Test]
        public void Switch_RowOn_OffSwitchesTurnItOff()
        {
            Assert.IsTrue(Text61.Decide(true, new string[0]), "the row on, no switch: on");
            Assert.IsFalse(Text61.Decide(false, new string[0]), "the row off: off");
            Assert.IsFalse(Text61.Decide(true, new[] { "-skygrindOff61" }), "-skygrindOff61");
            Assert.IsFalse(Text61.Decide(true, new[] { "-skygrindOff60" }), "an older night's switch covers night 61");
            Assert.IsFalse(Text61.Decide(true, new[] { Text61.NoSwitch }), "the one-row switch");
            Assert.AreEqual("-skygrindNoText61", Text61.NoSwitch);
            Assert.IsTrue(Text61.Decide(true, new[] { "-skygrindOff62" }), "a later night's switch");
            Assert.IsTrue(Text61.Decide(true, new[] { "-skygrindNoBossRule61" }), "another row's switch");
            Assert.IsTrue(Text61.Decide(true, null), "no arguments: on");
        }

        [Test]
        public void LevelOne_KeepsItsOverlay_EverythingPasses()
        {
            var g = new Text61Gate();
            Assert.AreEqual(A, g.Pass(A, true, false, true, Dt, Wait), "a title up does not hold a level-1 lesson");
            Assert.AreEqual(null, g.Pass(null, true, false, false, Dt, Wait));
            Assert.AreEqual(A, g.Pass(A, true, false, false, Dt, Wait), "a repeat in level 1 shows");
            Assert.AreEqual(0, g.Withheld + g.Waited, "nothing withheld or held in level 1");
        }

        [Test]
        public void BossFight_EverythingPasses_NothingRecorded()
        {
            var g = new Text61Gate();
            Assert.AreEqual(A, g.Pass(A, false, true, true, Dt, Wait), "the bar up: hud47's queue decides");
            Assert.AreEqual(null, g.Pass(null, false, true, false, Dt, Wait));
            Assert.AreEqual(A, g.Pass(A, false, false, false, Dt, Wait), "a line seen only in a boss fight is not a repeat outside it");
            Assert.AreEqual(Text61Act.Shown, g.Act);
        }

        [Test]
        public void OutsideLevelOne_ANewLineWaitsForTheTitle_ThenStays()
        {
            var g = new Text61Gate();
            Assert.IsNull(g.Pass(A, false, false, true, Dt, Wait), "the title up: the instruction waits");
            Assert.AreEqual(Text61Act.Wait, g.Act, "the wait's first frame says so");
            Assert.IsNull(g.Pass(A, false, false, true, Dt, Wait), "still waiting");
            Assert.AreEqual(Text61Act.None, g.Act, "said once per wait");
            Assert.AreEqual(A, g.Pass(A, false, false, false, Dt, Wait), "the title gone: it shows");
            Assert.AreEqual(Text61Act.Shown, g.Act);
            Assert.AreEqual(A, g.Pass(A, false, false, true, Dt, Wait), "a title coming up later does not take the card away (no flicker)");
            Assert.AreEqual(1, g.Shown);
            Assert.AreEqual(1, g.Waited);
        }

        [Test]
        public void OutsideLevelOne_ALongTitleNeverEatsTheLine_ItShowsLate()
        {
            var g = new Text61Gate();
            int frames = 0;
            string got = null;
            while (got == null && frames < 1000) { got = g.Pass(B, false, false, true, Dt, Wait); frames++; }
            Assert.AreEqual(B, got, "past waitMax it shows anyway");
            Assert.AreEqual(Text61Act.Late, g.Act);
            Assert.That(frames * Dt, Is.InRange(Wait, Wait + 3 * Dt), "it waited waitMax, not longer");
            Assert.AreEqual(1, g.Late);
        }

        [Test]
        public void OutsideLevelOne_OncePerRun_ARepeatIsWithheld_ResetForgets()
        {
            var g = new Text61Gate();
            Assert.AreEqual(A, g.Pass(A, false, false, false, Dt, Wait));
            Assert.AreEqual(A, g.Pass(A, false, false, false, Dt, Wait), "the same line on the card: it stays");
            Assert.IsNull(g.Pass(null, false, false, false, Dt, Wait), "the slot empties");
            Assert.IsNull(g.Pass(A, false, false, false, Dt, Wait), "the same line again later: withheld");
            Assert.AreEqual(Text61Act.Repeat, g.Act);
            Assert.IsNull(g.Pass(A, false, false, false, Dt, Wait), "still withheld");
            Assert.AreEqual(Text61Act.None, g.Act, "counted once per appearance");
            Assert.AreEqual(B, g.Pass(B, false, false, false, Dt, Wait), "another line straight after: its own first showing");
            Assert.IsNull(g.Pass(A, false, false, false, Dt, Wait), "A again (no gap): still a repeat");
            Assert.AreEqual(2, g.Withheld, "two appearances of the repeat");
            Assert.AreEqual(2, g.Shown);
            g.Reset();
            Assert.AreEqual(A, g.Pass(A, false, false, false, Dt, Wait), "a new run: shown again");
            Assert.AreEqual("", g.Pass("", false, false, false, Dt, Wait), "an empty line passes through");
        }
    }
}
