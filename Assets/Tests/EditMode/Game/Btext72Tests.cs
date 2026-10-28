// NIGHT72 builder B (Game/Btext72.cs, the boss text's rules and counter; the HUD's half is UiBtext72.cs). Pinned (pure rules, no engine call):
//   1. LabelWord: the meter's name STAGGER is "" with the row on, the word with the row off and a built meter, "" with no meter
//   2. HintHidden: the bar's own order is hidden only with the row on, the fight line drawing and the bar having text (one order at a time)
//   3. Count: wrong-hand, STAGGER-label, two-orders frames and the smallest text px accumulate on counted frames only; a held frame (a same-run toggle's off half) counts apart
//   4. the totals line names every number the slate's proof reads (wrong-hand, STAGGER, two orders, smallest px, plates, row on frames)
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class Btext72Tests
    {
        [Test]
        public void The_meter_name_is_not_drawn_with_the_row_on()
        {
            Assert.That(Btext72.LabelWord(true, true, "STAGGER"), Is.EqualTo(""));
            Assert.That(Btext72.LabelWord(false, true, "STAGGER"), Is.EqualTo("STAGGER"));
            Assert.That(Btext72.LabelWord(false, false, "STAGGER"), Is.EqualTo(""));
        }

        [TestCase(true, true, true, true)]
        [TestCase(true, true, false, false)]
        [TestCase(true, false, true, false)]
        [TestCase(false, true, true, false)]
        public void The_bar_order_waits_for_the_fight_line_only_with_the_row_on(bool rowOn, bool lineDrawn, bool hasText, bool hidden)
        {
            Assert.That(Btext72.HintHidden(rowOn, lineDrawn, hasText), Is.EqualTo(hidden));
        }

        [Test]
        public void The_counter_counts_frames_and_keeps_held_frames_apart()
        {
            var c = new Btext72.Count();
            c.Reset("THE KILN WYRM");
            c.Frame(false, false, true, false, true, true, true, 0, 18f);   // the row off: a wrong-hand bar order, STAGGER, two orders
            c.Frame(false, false, false, true, false, true, true, 0, 40f);  // the row off: a wrong-hand fight line
            c.Frame(false, true, false, false, false, false, true, 2, 22f); // the row on: nothing wrong, the two orders hidden to one, two plates
            c.Frame(true, true, true, true, true, true, true, 2, 5f);       // a held frame: counted apart, nothing else
            Assert.That(c.Frames, Is.EqualTo(3));
            Assert.That(c.OnFrames, Is.EqualTo(1));
            Assert.That(c.WrongHand, Is.EqualTo(2), "frames a boss line names a weapon not held (the bar's order or the fight line)");
            Assert.That(c.WrongBar, Is.EqualTo(1));
            Assert.That(c.WrongLine, Is.EqualTo(1));
            Assert.That(c.StaggerLabel, Is.EqualTo(1));
            Assert.That(c.Two, Is.EqualTo(2));
            Assert.That(c.WouldTwo, Is.EqualTo(3));
            Assert.That(c.Plates, Is.EqualTo(2));
            Assert.That(c.Held, Is.EqualTo(1));
            Assert.That(c.MinPx, Is.EqualTo(18f), "the smallest boss text drawn on a counted frame (the held frame's 5 px is not counted)");
            Assert.That(c.Dirty, Is.True);
        }

        [Test]
        public void Every_boss_line_in_the_table_keeps_the_rule()
        {
            // the lead's t1 verdict: the ON line was nine words in lower case; the rule is at most 6 words, the verb in capitals, the input's glyph (a token)
            foreach (var row in UnityTuning.Btext72Lines)
            {
                foreach (string line in new[] { row.held, row.swap })
                {
                    Assert.That(Btext72.Words(line), Is.LessThanOrEqualTo(UnityTuning.Btext72MaxWords), "words: " + line);
                    // the t3 verdict: counted AS DISPLAYED on a keyboard ({fire} reads LEFT CLICK, two words) "SWAP to gauntlet, then SWING {fire}" was seven
                    Assert.That(Btext72.DisplayWords(line), Is.LessThanOrEqualTo(UnityTuning.Btext72MaxWords), "displayed words (the keyboard's LEFT CLICK is two): " + line);
                    Assert.That(Btext72.HasCapsVerb(line), Is.True, "a verb in capitals: " + line);
                }
                bool rawHasToken = row.raw.Contains("{");
                if (rawHasToken)
                {
                    Assert.That(row.held.Contains("{"), Is.True, "the glyph token stays in the held line of " + row.raw);
                    Assert.That(row.swap.Contains("{"), Is.True, "the glyph token stays in the swap line of " + row.raw);
                }
                if (row.weapon != null) Assert.That(row.swap, Does.Contain("SWAP"), "a line that names a weapon not held names the hand to take: " + row.raw);
            }
        }

        [Test]
        public void The_wyrm_order_names_the_hand_you_hold()
        {
            const string raw = "Swing the gauntlet {fire} at its charging head.";
            Assert.That(Btext72.Order(raw, "gauntlet"), Is.EqualTo("SWING {fire} at the head"));
            Assert.That(Btext72.Order(raw, "shotgun"), Is.EqualTo("SWAP gauntlet, then SWING {fire}"));
            Assert.That(Btext72.Order(raw, null), Is.EqualTo("SWAP gauntlet, then SWING {fire}"));
            Assert.That(Btext72.Order("an order not in the table", "gauntlet"), Is.Null);
            Assert.That(Btext72.Order(null, "gauntlet"), Is.Null);
            // an order that names no weapon (the Crown's ring) is the same in every hand
            Assert.That(Btext72.Order("Jump {jump} and dash {dash} through the ring.", "shotgun"), Is.EqualTo(Btext72.Order("Jump {jump} and dash {dash} through the ring.", "torch")));
        }

        [Test]
        public void The_word_count_and_the_caps_verb_rule()
        {
            // the t1 line: the verdicts said "nine words"; split on spaces it is TEN (Swap, to, the, gauntlet, then, swing, at, its, charging, head): the rule's limit is 6 either way
            Assert.That(Btext72.Words("Swap to the gauntlet, then swing at its charging head."), Is.EqualTo(10), "the t1 line: ten words");
            Assert.That(Btext72.Words("SWAP to gauntlet, then SWING {fire}"), Is.EqualTo(6), "a token is one word");
            Assert.That(Btext72.Words(""), Is.EqualTo(0));
            // the t3 verdict's count: as displayed, {fire} is LEFT CLICK (two words): the t3 ON line was seven, the cut line is six
            Assert.That(Btext72.DisplayWords("SWAP to gauntlet, then SWING {fire}"), Is.EqualTo(7), "the t3 ON line: seven as displayed");
            Assert.That(Btext72.DisplayWords("SWAP gauntlet, then SWING {fire}"), Is.EqualTo(6), "the cut line: six as displayed");
            Assert.That(Btext72.DisplayWords("PARRY {punch} yellow embers, then SHOOT"), Is.EqualTo(6), "a bound key or a pad glyph is one word");
            Assert.That(Btext72.DisplayWords("HOLD {altFire} to ZOOM"), Is.EqualTo(5), "{altFire} reads RIGHT CLICK: two words");
            Assert.That(Btext72.DisplayWords(null), Is.EqualTo(0));
            Assert.That(Btext72.HasCapsVerb("Swap to the gauntlet, then swing at its charging head."), Is.False, "the t1 line: lower-case verbs");
            Assert.That(Btext72.HasCapsVerb("SWAP to gauntlet, then SWING {fire}"), Is.True);
            Assert.That(Btext72.HasCapsVerb("go {fire} a"), Is.False, "a token and one-letter words are not a verb in capitals");
        }

        [Test]
        public void The_counter_remembers_a_long_line_once()
        {
            var c = new Btext72.Count();
            c.Reset("THE KILN WYRM");
            c.Frame(false, false, false, false, false, false, false, 0, 40f, true);
            c.NoteLong("Swap to the gauntlet, then swing at its charging head.", 9);
            c.Frame(false, false, false, false, false, false, false, 0, 40f, true);
            c.NoteLong("Swap to the gauntlet, then swing at its charging head.", 9);
            c.Frame(false, true, false, false, false, false, false, 0, 40f, false);
            Assert.That(c.LongFrames, Is.EqualTo(2));
            Assert.That(c.LongNotes.Count, Is.EqualTo(1));
            Assert.That(c.Line("x", true, 1.0), Does.Contain("boss lines over 6 words 2 frames (\"Swap to the gauntlet"));
        }

        [Test]
        public void The_totals_line_names_what_the_slate_reads()
        {
            var c = new Btext72.Count();
            c.Reset("THE CROWN");
            c.Frame(false, true, false, false, false, false, true, 1, 22f);
            string line = c.Line("the quit", true, 12.5);
            Assert.That(line, Does.StartWith("[btext72] THE CROWN"));
            Assert.That(line, Does.Contain("not held 0 frames"));
            Assert.That(line, Does.Contain("STAGGER drawn while not staggered 0 frames"));
            Assert.That(line, Does.Contain("two orders at once 0 frames (would be 1"));
            Assert.That(line, Does.Contain("smallest boss text 22 px"));
            Assert.That(line, Does.Contain("row btext72 on 1 of 1 frames"));
            Assert.That(line, Does.Contain("the quit, t 12.50 s (FeelClock)"));
        }
    }
}
