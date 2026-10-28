// NIGHT63 builder G (leftover 2: text61, NIGHT62 P3, judgeable on a two-run pair with the hint pinned; Game/PinHint63.cs, Game/UiPinHint63.cs).
// Pinned:
//   1. the switch: -skygrindPinHint63 acts only in a reel (-skygrindReelOnly on the command line too), never in normal play; its text after it
//      ('|' separates lines, one per room in turn), else UnityTuning.PinHint63Default (no text, or the next argument is another switch)
//   2. the feed: no line before the first room entry; after each entry PinHint63GapFrames frames with none (the card closes, as at a room
//      change), then the room's pinned line, every frame; several lines go one per room, in turn; the counters count
//   3. two runs fed the same room entries give the same line on every frame (what the pin is for: NIGHT62's s1on / s1spot drifted 1 frame from
//      the kb2 clear and 9 frames by the kb3 entry, so their lesson rotation differed at the clear shot)
//   4. the SPOT pose this makes judgeable: the pinned line through text61's own gate (Text61Gate, outside level 1, a room's title up for 2.5 s
//      after each entry, the entry frame 1.5 s in): with text61 on no entry frame shows the instruction under the title (room 1: it waits
//      for the title; rooms 2-5: the same line again is withheld); with text61 off (the line as fed) all five do
// Written test-first: RED on its own commit (the placeholder reads no switch and feeds nothing). Not run in Unity by its author.
using System.Collections.Generic;
using NUnit.Framework;

namespace Skygrind.Game.Tests
{
    public class N63PinHintTests
    {
        [Test]
        public void Switch_ReelOnly_TextOrTheDefault()
        {
            Assert.IsNull(PinHint63.Parse(new string[0]), "no switch");
            Assert.IsNull(PinHint63.Parse(null), "no arguments");
            Assert.IsNull(PinHint63.Parse(new[] { "-skygrindPinHint63", "HOLD {fire}" }), "not a reel: never in normal play");
            CollectionAssert.AreEqual(new[] { "HOLD {fire}" }, PinHint63.Parse(new[] { "-skygrindReelOnly", "a157", "-skygrindPinHint63", "HOLD {fire}" }));
            CollectionAssert.AreEqual(new[] { UnityTuning.PinHint63Default }, PinHint63.Parse(new[] { "-skygrindPinHint63", "-skygrindMap", "n57-kiln", "-skygrindReelOnly", "a157" }), "no text: the default");
            CollectionAssert.AreEqual(new[] { UnityTuning.PinHint63Default }, PinHint63.Parse(new[] { "-skygrindReelOnly", "a157", "-skygrindPinHint63" }), "last argument: the default");
            CollectionAssert.AreEqual(new[] { "A", "B", "C" }, PinHint63.Parse(new[] { "-skygrindReelOnly", "a157", "-skygrindPinHint63", "A| B |C|" }), "lines by '|'");
            CollectionAssert.AreEqual(new[] { UnityTuning.PinHint63Default }, PinHint63.Parse(new[] { "-skygrindReelOnly", "a157", "-skygrindPinHint63", " | " }), "only separators: the default");
            StringAssert.StartsWith("STEP 1 · IN THE AIR", UnityTuning.PinHint63Default, "the boost chain's line as the config writes it (a middle dot)");
        }

        [Test]
        public void Feed_NothingBeforeARoom_AGapAtEachEntry_ThenTheRoomsLine()
        {
            var f = new PinHint63Feed(new[] { "L" });
            for (int i = 0; i < 5; i++) Assert.IsNull(f.Line(), "no line before the first room entry");
            f.RoomEnter();
            for (int i = 0; i < UnityTuning.PinHint63GapFrames; i++) Assert.IsNull(f.Line(), "the gap after the entry");
            for (int i = 0; i < 10; i++) Assert.AreEqual("L", f.Line(), "the line, every frame");
            f.RoomEnter();
            Assert.IsNull(f.Line(), "a new room: the gap again");
            Assert.AreEqual("L", f.Line());
            Assert.AreEqual(2, f.Rooms);
            Assert.AreEqual(11, f.Fed);
            Assert.AreEqual(2 * UnityTuning.PinHint63GapFrames, f.Gaps);
            Assert.That(UnityTuning.PinHint63GapFrames, Is.GreaterThanOrEqualTo(1), "at least one empty frame, so the card closes");
            var m = new PinHint63Feed(new[] { "A", "B" });
            var seen = new List<string>();
            for (int room = 0; room < 3; room++)
            {
                m.RoomEnter();
                for (int i = 0; i < UnityTuning.PinHint63GapFrames; i++) m.Line();
                seen.Add(m.Line());
            }
            CollectionAssert.AreEqual(new[] { "A", "B", "A" }, seen, "several lines: one per room, in turn");
        }

        static List<string> Run(int[] entryFrames, int frames)
        {
            var f = new PinHint63Feed(new[] { "X" });
            var outp = new List<string>();
            int k = 0;
            for (int i = 0; i < frames; i++)
            {
                if (k < entryFrames.Length && entryFrames[k] == i) { f.RoomEnter(); k++; }
                outp.Add(f.Line());
            }
            return outp;
        }

        [Test]
        public void TwoRuns_TheSameEntries_TheSameLineEveryFrame()
        {
            int[] entries = { 30, 400, 900, 1300, 1700 };
            List<string> a = Run(entries, 2000), b = Run(entries, 2000);
            CollectionAssert.AreEqual(a, b, "the line depends on the room entries only");
            Assert.AreEqual("X", a[31 + UnityTuning.PinHint63GapFrames], "fed after the first entry");
        }

        [Test]
        public void TheSpotPose_Text61OnShowsNoInstructionUnderTheTitle_OffShowsItInAllFive()
        {
            const double dt = 1.0 / 60, titleS = 2.5, shotS = 1.5;
            int perRoom = 360; // 6 s a room
            var f = new PinHint63Feed(new[] { "Midair, aim down, then LEFT CLICK." });
            var gate = new Text61Gate();
            int onUnderTitle = 0, offUnderTitle = 0, shots = 0;
            for (int room = 0; room < 5; room++)
            {
                f.RoomEnter();
                for (int i = 0; i < perRoom; i++)
                {
                    double t = i * dt;
                    bool titleUp = t < titleS;
                    string fed = f.Line();
                    string on = gate.Pass(fed, false, false, titleUp, dt, UnityTuning.Text61WaitS);
                    if (i == (int)(shotS / dt))
                    {
                        shots++;
                        Assert.IsTrue(titleUp, "the entry frame has the room's title");
                        if (!string.IsNullOrEmpty(on)) onUnderTitle++;
                        if (!string.IsNullOrEmpty(fed)) offUnderTitle++;
                    }
                }
            }
            Assert.AreEqual(5, shots);
            Assert.AreEqual(0, onUnderTitle, "text61 on: the title alone on every entry frame");
            Assert.AreEqual(5, offUnderTitle, "text61 off: the title and the instruction on every entry frame");
            Assert.AreEqual(1, gate.Shown, "on: the instruction shown once (in room 1, after its title)");
            Assert.AreEqual(4, gate.Withheld, "on: withheld in rooms 2-5");
        }
    }
}
