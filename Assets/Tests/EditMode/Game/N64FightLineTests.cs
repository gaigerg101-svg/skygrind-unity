// NIGHT64 builder G (NIGHT64.md section 3 leftover 2, design.n64.fightLine64; Game/FightLine64.N64G.cs, the HUD's half in
// Game/UiFightLine64.N64G.cs, its hook in UiHud33.cs RenderHud33). gg-proxy (Logs/n63/gg-proxy.md #3): the room's name stands as hud47's fight
// line in the middle of the fight. The red (n63-b12's boss63 telemetry, 1529 frames each): "THE STACK" 170 frames on THE WICK (f314-f483),
// "THE SPIRE" 170 on THE CROWN, "THE NARROWS" 170 on THE LOOM, each with the boss bar drawn, right after the rule line; never drawn in the
// slot before the fight (the bar is up from f19). Pinned:
//   1. the switch: off when the row is off, under -skygrindOff64 (or older), -skygrindNoFightLine64 (any case), ForceOffFrom 64; on otherwise;
//      the reel's live override wins (the row read every frame); D1 claims the row (SameRun63Math.FrameRows)
//   2. a room title: a zone's / room's name as posted ("3.2b THE STACK") or without its number (Copy47's "THE STACK"), case and edge spaces
//      ignored; a boss's order is not one
//   3. the hide: the row on, the boss bar up, the fight line drawing a room title: not drawn this frame (alpha 0); anything else as before.
//      The queue itself is untouched (the title keeps its turn and its time), so the same-run toggle's off frame draws what the off game draws
//   4. the counter: per boss, room-title frames in the fight line mid-fight: would draw (the queue's alpha: the off game) and drawn (now); held
//      frames apart; a title's run and whether it was hidden
//   5. never lost: a title hidden in a fight is held and posted to the message slot once, after the fight (the bar down), and counted when the
//      slot draws it
//   6. the hook comes after Fight47Late (after the queue and order64 drew this frame) and before Top52, in RenderHud33
// Written test first (NIGHT64.md section 4): RED on its commit (FightLine64's placeholders: the row always off, no title known, nothing
// hidden, a counter that counts nothing, nothing held, no hook). Not run in Unity by its author.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using F = Skygrind.Game.FightLine64;

namespace Skygrind.Game.Tests
{
    public class N64FightLineTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        static GameConfig WithRow(bool on)
        {
            GameConfig c = Cfg();
            var design = (Dictionary<string, object>)c.root["design"];
            if (!(design.TryGetValue("n64", out object n) && n is Dictionary<string, object> n64))
            {
                n64 = new Dictionary<string, object> { ["_why"] = "test" };
                design["n64"] = n64;
            }
            if (n64.TryGetValue(F.Row, out object r) && r is Dictionary<string, object> row) row["enabled"] = on;
            else n64[F.Row] = new Dictionary<string, object> { ["_why"] = "test", ["enabled"] = on, ["enabled_why"] = "test" };
            return c;
        }

        // ---- 1. the switch ----

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride.Remove(F.Row);
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(F.OffReason(null, new string[0]), Is.Not.Null, "no config");
                Assert.That(F.OffReason(off, new string[0]), Is.Not.Null, "the row off");
                Assert.That(F.OffReason(on, new string[0]), Is.Null, "the row on, no switch: on");
                Assert.That(F.OffReason(on, new[] { "-skygrindNoFightLine64" }), Is.Not.Null, "this row alone off");
                Assert.That(F.OffReason(on, new[] { "-skygrindnofightline64" }), Is.Not.Null, "any case");
                Assert.That(F.OffReason(on, new[] { "-skygrindOff64" }), Is.Not.Null);
                Assert.That(F.OffReason(on, new[] { "-skygrindOff61" }), Is.Not.Null, "an older night's switch");
                Assert.That(F.OffReason(on, new[] { "-skygrindOff65" }), Is.Null, "a later night's switch leaves it on");
                Assert.That(F.OffReason(on, new[] { "-skygrindNoOrder64" }), Is.Null, "another row's switch leaves it on");
                NightRows.ForceOffFrom = 64;
                Assert.That(F.OffReason(on, new string[0]), Is.Not.Null, "ForceOffFrom 64");
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride[F.Row] = false;
                Assert.That(F.OffReason(on, new string[0]), Is.Not.Null, "the same-run toggle's held frame: off with the row on");
                NightRows.ReelOverride[F.Row] = true;
                Assert.That(F.OffReason(off, new string[0]), Is.Null, "an override on wins over the config");
            }
            finally
            {
                NightRows.ReelOverride.Remove(F.Row);
                NightRows.ForceOffFrom = was;
            }
            Assert.That(SameRun63Math.KindOf(F.Row), Is.EqualTo(SameRun63Math.Kind.Frame), "D1 claims the row (the lead's A1)");
        }

        // ---- 2. a room title ----

        [Test]
        public void Titles_ARoomsNameAsPostedOrWithoutItsNumber()
        {
            var t = new F.Titles();
            t.Add("3.2b THE STACK");
            t.Add("3.5 THE RING");
            t.Add(null);
            t.Add("");
            Assert.That(t.Has("THE STACK"), Is.True, "copy47 strips the number (the banner as drawn)");
            Assert.That(t.Has("3.2b THE STACK"), Is.True, "copy47 off: the name as it is");
            Assert.That(t.Has(" the stack "), Is.True, "case and edge spaces ignored");
            Assert.That(t.Has("THE RING"), Is.True);
            Assert.That(t.Has("STAGGERED: EVERY HIT COUNTS"), Is.False, "a boss's order is not a room title");
            Assert.That(t.Has("Parry F the yellow embers, then shoot."), Is.False, "the rule line");
            Assert.That(t.Has(""), Is.False);
            Assert.That(t.Has(null), Is.False);
            Assert.That(t.Count, Is.EqualTo(4), "two names, each as posted and without its number");
        }

        // ---- 3. the hide ----

        [Test]
        public void Hide_OnlyARoomTitleInTheFightLineMidFight_RowOn()
        {
            Assert.That(F.Hide(true, true, true, true), Is.True, "gg-proxy: THE STACK mid-fight");
            Assert.That(F.Hide(false, true, true, true), Is.False, "row off: as NIGHT63");
            Assert.That(F.Hide(true, false, true, true), Is.False, "no fight (no bar): the line is hud47's as before");
            Assert.That(F.Hide(true, true, false, true), Is.False, "not drawn anyway");
            Assert.That(F.Hide(true, true, true, false), Is.False, "the boss's order or the rule line: drawn");
        }

        // ---- 4. the counter ----

        /// <summary>A planted fight: the rule line `rule` frames (not a title), then the room title drawn by the queue `title` frames, then
        /// `after` frames with nothing; the held pair (on half, off half) at `heldAt` into the title's frames.</summary>
        static F.Count Plant(bool rowOn, int rule, int title, int after, int heldAt, out int ends)
        {
            var c = new F.Count();
            c.Reset("THE WICK");
            int n = 0;
            void Fr(bool held, bool row, bool queueDraws, bool isTitle, string text)
            {
                bool would = queueDraws && isTitle;
                bool drawn = queueDraws && !F.Hide(row, true, true, isTitle);
                if (c.Frame(held, row, would, would && drawn, text)) n++;
            }
            for (int i = 0; i < rule; i++) Fr(false, rowOn, true, false, "Parry F the yellow embers, then shoot.");
            for (int i = 0; i < title; i++)
            {
                Fr(false, rowOn, true, true, "THE STACK");
                if (i == heldAt) { Fr(true, rowOn, true, true, "THE STACK"); Fr(true, false, true, true, "THE STACK"); }
            }
            for (int i = 0; i < after; i++) Fr(false, rowOn, false, false, null);
            ends = n;
            return c;
        }

        [Test]
        public void Counter_RowOff_TheTitleDrawnMidFight()
        {
            F.Count c = Plant(false, 10, 8, 5, -1, out int ends);
            Assert.That(c.Frames, Is.EqualTo(23));
            Assert.That(c.OnFrames, Is.EqualTo(0));
            Assert.That(c.Would, Is.EqualTo(8));
            Assert.That(c.Drawn, Is.EqualTo(8), "NIGHT63's HUD (the red)");
            Assert.That(c.TitleRuns, Is.EqualTo(1));
            Assert.That(c.HiddenRuns, Is.EqualTo(0));
            Assert.That(ends, Is.EqualTo(1), "one title line when the run ends");
            Assert.That(c.RunText, Is.EqualTo("THE STACK"));
            Assert.That(c.RunHidden, Is.False);
        }

        [Test]
        public void Counter_RowOn_WouldDrawButNoneDrawn()
        {
            F.Count c = Plant(true, 10, 8, 5, -1, out int ends);
            Assert.That(c.Frames, Is.EqualTo(23));
            Assert.That(c.OnFrames, Is.EqualTo(23));
            Assert.That(c.Would, Is.EqualTo(8), "the same frames the off game draws it");
            Assert.That(c.Drawn, Is.EqualTo(0), "the fight line holds no room title mid-fight");
            Assert.That(c.TitleRuns, Is.EqualTo(1));
            Assert.That(c.HiddenRuns, Is.EqualTo(1));
            Assert.That(ends, Is.EqualTo(1));
            Assert.That(c.RunFrames, Is.EqualTo(8));
            Assert.That(c.RunWould, Is.EqualTo(8));
            Assert.That(c.RunDrawn, Is.EqualTo(0));
            Assert.That(c.RunHidden, Is.True);
        }

        [Test]
        public void Counter_HeldFramesApart()
        {
            F.Count c = Plant(true, 10, 8, 5, 3, out _);
            Assert.That(c.Frames, Is.EqualTo(23), "held frames are not the run's");
            Assert.That(c.Held, Is.EqualTo(2));
            Assert.That(c.HeldWould, Is.EqualTo(2));
            Assert.That(c.HeldDrawn, Is.EqualTo(1), "the off half draws the title (the SPOT pair's other half)");
            Assert.That(c.RunFrames, Is.EqualTo(8));
        }

        [Test]
        public void Counter_ARunOpenAtTheBarsGoingDown_IsEndedOnce()
        {
            var c = new F.Count();
            c.Reset("THE CROWN");
            for (int i = 0; i < 4; i++) c.Frame(false, true, true, false, "THE SPIRE");
            Assert.That(c.EndRun(), Is.True, "the fight ended inside the title's run");
            Assert.That(c.EndRun(), Is.False, "once");
            Assert.That(c.RunFrames, Is.EqualTo(4));
            Assert.That(c.HiddenRuns, Is.EqualTo(1));
        }

        // ---- 5. never lost ----

        [Test]
        public void Later_AHiddenTitleIsPostedOnceAfterTheFight_AndCountedWhenTheSlotDrawsIt()
        {
            var l = new F.Later();
            l.Hidden("THE STACK");
            l.Hidden("THE STACK");
            l.Hidden(null);
            Assert.That(l.Waiting, Is.EqualTo(1), "one title, held once");
            List<string> post = l.TakeForPost();
            CollectionAssert.AreEqual(new[] { "THE STACK" }, post);
            Assert.That(l.Waiting, Is.EqualTo(0));
            Assert.That(l.Posted, Is.EqualTo(1));
            Assert.That(l.TakeForPost().Count, Is.EqualTo(0), "posted once");
            Assert.That(l.Seen("ROUND 2"), Is.False);
            Assert.That(l.Seen("the stack"), Is.True, "the slot drew it (case ignored)");
            Assert.That(l.ShownAfter, Is.EqualTo(1));
            Assert.That(l.Seen("THE STACK"), Is.False, "counted once");
            l.Hidden("THE SPIRE");
            l.Clear();
            Assert.That(l.Waiting, Is.EqualTo(0), "a new run starts clean");
        }

        [Test]
        public void Counter_TheLinesNameTheirNumbers()
        {
            F.Count c = Plant(true, 10, 8, 5, 3, out _);
            var l = new F.Later();
            l.Hidden("THE STACK");
            string t = c.Line("the boss bar went down", true, l, 9.5);
            StringAssert.StartsWith("[fight64] THE WICK | ", t);
            StringAssert.Contains("row fightLine64 on 23 of 23 frames", t);
            StringAssert.Contains("room-title frames in the fight line: would draw 8, drawn 0", t);
            StringAssert.Contains("held 2 frames: would draw 2, drawn 1", t);
            StringAssert.Contains("titles mid-fight 1: hidden 1, held for after the fight 1, posted after 0, shown after 0", t);
            StringAssert.Contains("the boss bar went down, t 9.50 s", t);
            string o = c.TitleLine(true, 9.5);
            StringAssert.StartsWith("[fight64] THE WICK title \"THE STACK\" in the fight line mid-fight 8 frames: would draw 8, drawn 0", o);
            StringAssert.Contains("held for after the fight", o);
        }

        // ---- 6. the hook's place ----

        [Test]
        public void Hook_AfterFight47Late_BeforeTop52()
        {
            string src = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "UiHud33.cs"));
            int render = src.IndexOf("void RenderHud33(", StringComparison.Ordinal);
            Assert.That(render, Is.GreaterThan(0));
            int late = src.IndexOf("Fight47Late(frameDt);", render, StringComparison.Ordinal);
            int hook = src.IndexOf("FightLine64Late();", render, StringComparison.Ordinal);
            int top = src.IndexOf("Top52();", render, StringComparison.Ordinal);
            Assert.That(late, Is.GreaterThan(render), "hud47's queue (and order64) draw first");
            Assert.That(hook, Is.GreaterThan(late), "fightLine64 reads the fight line as the queue drew it");
            Assert.That(top, Is.GreaterThan(hook), "before the top plates");
        }
    }
}
