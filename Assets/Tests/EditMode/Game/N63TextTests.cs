// NIGHT63 builder G (text63: text61's critic points, DECIDE63 D-7, Logs/n63/verdict-P3.md problems 3-5; design.n63.text63; Game/Text63.cs,
// Game/Text61.cs Text61Gate's keyed Pass, Game/UiText61.cs). The red (b3's verbs37 logs, `grep "\[text61\]" Logs/player/n47-reel-n63-b3-v?.log`):
// (a) 4 lines shown "(LATE: the room's title stood 3 s)", with the title still up (vi, vk, vs, vp); (b) kiln shows 10 lines in one run, 7 of
// them the boost, several the same aim-down air shot in different words. Pinned:
//   1. the row: design.n63.text63 a VIEW row, its numbers.verbs mapping n57-kiln's seven boost templates (brief55's wording) to four techniques:
//      boost (the aim-down air shot: three lines), boostHigher (jump and fire together), boostAgain (fire again at the top, and the summary that
//      says it), flue (fire, jump at the top, fire again); the step tutorial keeps one line per step (PRINCIPLES 1, GG's G26)
//   2. the key: a line the table's templates resolve to reads as its technique; any other line is its own key (text61's once-per-line)
//   3. b3's kiln sequence through the keyed gate: 7 shown, the 3 restatements withheld ("Midair, aim down, then ...", "Fire ... down to lift
//      off.", "Boost: ... twice."); with the line as the key (text61) all 10 show
//   4. the wait: text63's cap is UnityTuning.Text63WaitS (10 s), text61's 3 s; a title standing 5 s: text61 shows the line LATE at 3 s under it,
//      text63 shows it when the title has gone (not LATE)
//   T63b (the fix round after text63's GATE FAIL, Logs/n63/verdict-T63.md: on b6 a line waiting for the room's title was dropped without a log
//   line when the next lesson replaced it: kiln's "Jump SPACE, aim down, fire LEFT CLICK." and spire's "Hold LEFT CLICK up close to burn." never
//   showed; the boost key then went to the restatement "Midair ..."). With text63 on the gate QUEUES (Text61Gate.PassQueued):
//   5. a waiting line is never dropped by a newer one or by the slot going empty: the lines show one at a time, in the order the run reached them,
//      a line released after the slot moved on standing at least UnityTuning.Text63HoldS
//   6. the first wording of a technique the run reaches teaches it: a later wording of a technique waiting or taught is withheld (Queued /
//      Taught), whichever wording comes first (the a157 flow: Jump first; a quick bot: still Jump first, Midair withheld while Jump waits)
//   7. every line's fate is recorded: shown (or LATE), withheld (taught, or its technique waiting as the first wording), or still waiting at run
//      end (DrainWaiting); text61's own Pass is unchanged (the off path)
// Written test-first: RED on its own commit (the placeholder has no table, ignores the key, keeps the 3 s cap). Not run in Unity by its author.
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N63TextTests
    {
        static GameConfig Cfg()
        {
            string root;
            try { root = System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { root = System.IO.Path.GetFullPath("PortExport"); }
            return GameConfig.Load(System.IO.Path.Combine(root, "config.json"));
        }

        /// <summary>The keyboard's resolution of the tokens b3's logs show.</summary>
        static string Keys(string t) => t.Replace("{jump}", "SPACE").Replace("{fire}", "LEFT CLICK").Replace("{grapple}", "G").Replace("{dash}", "L-SHIFT").Replace("{quickSwap}", "Q");

        // b3's vk log, in order (Logs/player/n47-reel-n63-b3-vk.log)
        static readonly string[] Kiln = {
            "Jump SPACE, aim down, fire LEFT CLICK.", "Midair, aim down, then LEFT CLICK.", "Fire LEFT CLICK down to lift off.", "Press SPACE and LEFT CLICK together.",
            "At the top, press LEFT CLICK again.", "Boost: SPACE, aim down, LEFT CLICK twice.", "Press LEFT CLICK, SPACE, then LEFT CLICK again.",
            "Hold LEFT CLICK grounded for a slug.", "Get off the floor with SPACE.", "Shoot LEFT CLICK it as it surfaces." };

        [Test]
        public void Row_AViewRow_WithTheTechniqueTable()
        {
            GameConfig c = Cfg();
            const string at = "design.n63.text63";
            Assert.That(c.Has(at + "._why") && c.Get(at + "._why") is string w && w.Length > 0, Is.True, at + "._why");
            Assert.That(c.Has(at + ".enabled") && c.Get(at + ".enabled") is bool, Is.True, at + ".enabled");
            Assert.That(c.Has(at + ".enabled_why"), Is.True, at + ".enabled_why");
            Assert.That(c.Has(at + ".sim"), Is.False, "a view row");
            Assert.That(NightRows.From(c).IndexOf("text63"), Is.EqualTo(-1), "no tape bit");
            Assert.That(NightRows.Problems(c.Obj("design")).Count, Is.EqualTo(0), "the checker");
            Dictionary<string, string> v = Text63.Verbs(c);
            Assert.AreEqual("boost", v["Jump {jump}, aim down, fire {fire}."]);
            Assert.AreEqual("boost", v["Midair, aim down, then {fire}."]);
            Assert.AreEqual("boost", v["Fire {fire} down to lift off."]);
            Assert.AreEqual("boostHigher", v["Press {jump} and {fire} together."]);
            Assert.AreEqual("boostAgain", v["At the top, press {fire} again."]);
            Assert.AreEqual("boostAgain", v["Boost: {jump}, aim down, {fire} twice."]);
            Assert.AreEqual("flue", v["Press {fire}, {jump}, then {fire} again."]);
            Assert.AreEqual(4, new HashSet<string>(v.Values).Count, "four techniques, the step tutorial's steps kept apart");
            Assert.IsNotNull(Text63.OffReason(null, new string[0]), "no config: off");
            Assert.AreEqual("-skygrindNoText63", Text63.NoSwitch);
        }

        [Test]
        public void TheKey_ATablesLineReadsAsItsTechnique_AnyOtherLineIsItsOwn()
        {
            var verbs = new Dictionary<string, string> { { "Jump {jump}, aim down, fire {fire}.", "boost" }, { "Midair, aim down, then {fire}.", "boost" } };
            Dictionary<string, string> map = Text63.Resolved(verbs, Keys);
            Assert.AreEqual("boost", Text63.KeyOf("Jump SPACE, aim down, fire LEFT CLICK.", map));
            Assert.AreEqual("boost", Text63.KeyOf("Midair, aim down, then LEFT CLICK.", map));
            Assert.AreEqual("Hold LEFT CLICK grounded for a slug.", Text63.KeyOf("Hold LEFT CLICK grounded for a slug.", map), "not in the table: its own key");
            Assert.AreEqual(null, Text63.KeyOf(null, map));
        }

        static List<string> Shown(IList<string> lines, bool keyed, Dictionary<string, string> map, out int withheld)
        {
            var gate = new Text61Gate();
            var shown = new List<string>();
            foreach (string l in lines)
            {
                for (int f = 0; f < 3; f++) // the card closes between lines
                    if (keyed) gate.PassQueued("", "", false, false, false, 1.0 / 60, UnityTuning.Text63WaitS, UnityTuning.Text63HoldS);
                    else gate.Pass("", "", false, false, false, 1.0 / 60, 3.0);
                for (int f = 0; f < 30; f++)
                {
                    string r = keyed ? gate.PassQueued(l, Text63.KeyOf(l, map), false, false, false, 1.0 / 60, UnityTuning.Text63WaitS, UnityTuning.Text63HoldS)
                        : gate.Pass(l, l, false, false, false, 1.0 / 60, 3.0);
                    if (f == 29 && !string.IsNullOrEmpty(r)) shown.Add(r);
                }
            }
            withheld = gate.Withheld;
            return shown;
        }

        [Test]
        public void B3sKilnRun_SevenShown_TheThreeRestatementsWithheld_TheStepsKept()
        {
            Dictionary<string, string> map = Text63.Resolved(Text63.Verbs(Cfg()), Keys);
            List<string> keyed = Shown(Kiln, true, map, out int withheld);
            CollectionAssert.AreEqual(new[] {
                "Jump SPACE, aim down, fire LEFT CLICK.", "Press SPACE and LEFT CLICK together.", "At the top, press LEFT CLICK again.",
                "Press LEFT CLICK, SPACE, then LEFT CLICK again.", "Hold LEFT CLICK grounded for a slug.", "Get off the floor with SPACE.", "Shoot LEFT CLICK it as it surfaces." }, keyed);
            Assert.AreEqual(3, withheld, "the aim-down shot said twice more and the summary after its step");
            List<string> byLine = Shown(Kiln, false, map, out int w0);
            Assert.AreEqual(10, byLine.Count, "text61 alone (once per line): all ten");
            Assert.AreEqual(0, w0);
        }

        [Test]
        public void TheWait_TenSecondsForTheTitle_NotThreeThenStacked()
        {
            Assert.That(Text63.WaitS(true), Is.EqualTo(UnityTuning.Text63WaitS).Within(1e-9));
            Assert.That(Text63.WaitS(false), Is.EqualTo(UnityTuning.Text61WaitS).Within(1e-9));
            Assert.That(UnityTuning.Text63WaitS, Is.GreaterThanOrEqualTo(10.0 - 1e-9));
            foreach (bool t63 in new[] { false, true })
            {
                var gate = new Text61Gate();
                double dt = 1.0 / 60, shownAt = -1;
                bool shownUnderTitle = false;
                for (int f = 0; f < 600 && shownAt < 0; f++)
                {
                    double t = f * dt;
                    bool titleUp = t < 5.0; // a title standing 5 s (b3's LATE cases: longer than text61's 3 s)
                    const string far = "Shoot LEFT CLICK the far ones.";
                    string r = t63 ? gate.PassQueued(far, far, false, false, titleUp, dt, Text63.WaitS(true), UnityTuning.Text63HoldS)
                        : gate.Pass(far, far, false, false, titleUp, dt, Text63.WaitS(false));
                    if (!string.IsNullOrEmpty(r)) { shownAt = t; shownUnderTitle = titleUp; }
                }
                if (!t63)
                {
                    Assert.That(shownAt, Is.InRange(2.9, 3.1), "text61: shown at its 3 s cap");
                    Assert.IsTrue(shownUnderTitle, "text61: under the title (two blocks: the red)");
                    Assert.AreEqual(1, gate.Late);
                }
                else
                {
                    Assert.That(shownAt, Is.InRange(4.99, 5.05), "text63: shown when the title has gone");
                    Assert.IsFalse(shownUnderTitle, "text63: one line of text at a time");
                    Assert.AreEqual(0, gate.Late);
                }
            }
        }

        // ---- T63b: the queue (Text61Gate.PassQueued) ----

        const string J = "Jump SPACE, aim down, fire LEFT CLICK.", M = "Midair, aim down, then LEFT CLICK.", P = "Press SPACE and LEFT CLICK together.";
        const string H = "Hold LEFT CLICK up close to burn.", D = "Dash L-SHIFT and jump SPACE through.";

        sealed class Played
        {
            public readonly Text61Gate gate = new Text61Gate();
            public readonly List<string> shown = new List<string>();
            public readonly List<double> shownAt = new List<double>();
            public readonly List<Text63Event> events = new List<Text63Event>();
            public double t;
        }

        /// <summary>A run at 60 fps: each segment is the line the slot hands (null: none) for `s` seconds with the room's title up or not.</summary>
        static Played Play(Played run, Dictionary<string, string> map, params (string line, double s, bool title)[] segs)
        {
            const double dt = 1.0 / 60;
            foreach ((string line, double s, bool title) in segs)
                for (int f = 0; f < (int)System.Math.Round(s / dt); f++)
                {
                    string last = run.shown.Count > 0 ? run.shown[run.shown.Count - 1] : null;
                    string r = run.gate.PassQueued(line, line == null ? null : Text63.KeyOf(line, map), false, false, title, dt, UnityTuning.Text63WaitS, UnityTuning.Text63HoldS);
                    run.events.AddRange(run.gate.Events);
                    if (!string.IsNullOrEmpty(r) && r != last && !run.shown.Contains(r)) { run.shown.Add(r); run.shownAt.Add(run.t); }
                    run.t += dt;
                }
            return run;
        }

        static Dictionary<string, string> Map() => Text63.Resolved(Text63.Verbs(Cfg()), Keys);

        /// <summary>Each line the slot handed has exactly one final fate (shown, LATE, withheld as taught or as waiting, still waiting at run end).</summary>
        static void EveryLineHasOneFate(Played run, params string[] lines)
        {
            foreach (string l in lines)
            {
                int finals = run.events.FindAll(e => e.line == l && e.fate != Text63Fate.Waits).Count;
                Assert.AreEqual(1, finals, "one fate for '" + l + "': " + string.Join(", ", run.events.FindAll(e => e.line == l).ConvertAll(e => e.fate.ToString())));
            }
        }

        [Test]
        public void ALineWaitingForTheTitle_IsNeverDroppedByANewerOne_TheyShowInOrder()
        {
            Dictionary<string, string> map = Map();
            // b6 vk (verbs37's kiln): the title stands 3 s; the bot reaches the jump step, its restatement and the next step inside it
            Played run = Play(new Played(), map, (J, 1.0, true), (M, 1.0, true), (P, 1.0, true), (P, 2.0, false), (null, 8.0, false));
            CollectionAssert.AreEqual(new[] { J, P }, run.shown, "the first step shows first, then the next; the restatement withheld");
            Assert.That(run.shownAt[0], Is.GreaterThanOrEqualTo(3.0 - 1e-6), "nothing under the title");
            Assert.That(run.shownAt[1] - run.shownAt[0], Is.GreaterThanOrEqualTo(UnityTuning.Text63HoldS - 1e-6), "a line released after the slot moved on stands its hold");
            Text63Event m = run.events.Find(e => e.line == M && e.fate != Text63Fate.Waits);
            Assert.AreEqual(Text63Fate.Queued, m.fate, "Midair withheld: its technique waits as the first wording the run reached");
            Assert.AreEqual(J, m.by);
            EveryLineHasOneFate(run, J, M, P);
            Assert.AreEqual(0, run.gate.DrainWaiting().Count, "nothing left waiting");
        }

        [Test]
        public void TheFirstWordingTheRunReaches_TeachesTheTechnique()
        {
            Dictionary<string, string> map = Map();
            Played a157 = Play(new Played(), map, (J, 1.0, false), (null, 0.2, false), (M, 1.0, false), (null, 0.5, false));
            CollectionAssert.AreEqual(new[] { J }, a157.shown, "a157's order: the jump step shown, its restatement withheld");
            Assert.AreEqual(Text63Fate.Taught, a157.events.Find(e => e.line == M && e.fate != Text63Fate.Waits).fate);
            Played other = Play(new Played(), map, (M, 1.0, false), (null, 0.2, false), (J, 1.0, false), (null, 0.5, false));
            CollectionAssert.AreEqual(new[] { M }, other.shown, "whichever wording comes first teaches it");
            EveryLineHasOneFate(a157, J, M);
            EveryLineHasOneFate(other, J, M);
        }

        [Test]
        public void SpiresTorchLesson_WaitsThroughAnEmptySlot_AndShowsBeforeTheNext()
        {
            Dictionary<string, string> map = Map();
            // b6 vp: the TORCH lesson waits for the title, the dash line replaces it in the slot, the slot goes empty, the title then clears
            Played run = Play(new Played(), map, (H, 2.0, true), (D, 2.0, true), (null, 1.0, true), (null, 10.0, false));
            CollectionAssert.AreEqual(new[] { H, D }, run.shown, "both lessons show, in the order reached");
            EveryLineHasOneFate(run, H, D);
        }

        [Test]
        public void ALineStillWaitingAtRunEnd_IsNamed()
        {
            Dictionary<string, string> map = Map();
            Played run = Play(new Played(), map, (H, 2.0, true), (D, 2.0, true));
            CollectionAssert.AreEqual(new string[0], run.shown, "the title still up, 4 s into a 10 s wait");
            List<string> left = run.gate.DrainWaiting();
            CollectionAssert.AreEqual(new[] { H, D }, left, "still waiting at the run's end, in order");
            run.events.AddRange(run.gate.Events);
            Assert.AreEqual(2, run.events.FindAll(e => e.fate == Text63Fate.Left).Count, "a Left fate for each");
            EveryLineHasOneFate(run, H, D);
            Assert.AreEqual(0, run.gate.DrainWaiting().Count, "drained once");
        }
    }
}
