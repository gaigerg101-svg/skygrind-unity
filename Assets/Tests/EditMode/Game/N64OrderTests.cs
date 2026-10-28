// NIGHT64 builder G (slate U1 "One order at a time", design.n64.order64; Game/Order64.N64G.cs, the HUD's write in Game/UiOrder64.N64G.cs, its
// hook in UiHud47.Boss.cs Fight47Late). gg-proxy (Logs/n63/gg-proxy.md #3): 4 of 18 boss shots print the same order twice at once, small under
// the boss bar (bossbar43's hint) and large in the centre (hud47's fight line); one string feeds both (UiBossBar43.cs:209-215). Pinned:
//   1. the switch: off when the row is off, under -skygrindOff64 (or older), -skygrindNoOrder64 (any case), ForceOffFrom 64; on otherwise; the
//      reel's live override (D1's same-run toggle) wins, so the row is read every frame; the row is one D1 claims (SameRun63Math.FrameRows)
//   2. the same order: the fight line's text and the bar's hint, both non-empty, equal with case and edge spaces ignored (Case47 draws the
//      line in sentence case; its source string is the bar's)
//   3. the bar's hint with the row on: not drawn on a frame the fight line draws the same order; on every other frame exactly as without the
//      row (its text; bossRule61's hold: BossRule61.HintShown, N63BossRuleHintTests); with the row off: always as without the row
//   4. the counter (the slate's proof): per boss, would-duplicate = frames the fight line draws the bar's order while the game without the row
//      draws the hint (the row-off duplicate: the red), duplicate shown = frames both are drawn; the same-run toggle's held frames apart; every
//      order (each new hint text) and where it was shown, so "orders shown" reads the same on and off; one line when the fight line stops
//      drawing an order, the totals line at the end
//   5. the write comes after Fight47Tick in Fight47Late, so after Bar43Draw's Set and UiBossRule61's every-frame hold and release (the red
//      team's finding: else a one-frame duplicate at the rule line's release)
// Written test first (NIGHT64.md section 4): RED on its commit (Order64's placeholders: the row always off, no order the same, the hint as
// without the row, a counter that counts nothing, no hook). Not run in Unity by its author (the Editor run is the lead's).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using O = Skygrind.Game.Order64;

namespace Skygrind.Game.Tests
{
    public class N64OrderTests
    {
        const string Stagger = "STAGGERED: HOLD LEFT CLICK, SLUG IT"; // gg-proxy #3's words (THE KILN WYRM's stagger shot)
        const string Punch = "PUNCH THE HEAD TO STAGGER IT";

        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        /// <summary>The repo's config with design.n64.order64 set in memory to `on` (the night's block made if the copy has none).</summary>
        static GameConfig WithRow(bool on)
        {
            GameConfig c = Cfg();
            var design = (Dictionary<string, object>)c.root["design"];
            if (!(design.TryGetValue("n64", out object n) && n is Dictionary<string, object> n64))
            {
                n64 = new Dictionary<string, object> { ["_why"] = "test" };
                design["n64"] = n64;
            }
            if (n64.TryGetValue(O.Row, out object r) && r is Dictionary<string, object> row) row["enabled"] = on;
            else n64[O.Row] = new Dictionary<string, object> { ["_why"] = "test", ["enabled"] = on, ["enabled_why"] = "test" };
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
                NightRows.ReelOverride.Remove(O.Row);
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(O.OffReason(null, new string[0]), Is.Not.Null, "no config: off");
                Assert.That(O.OffReason(off, new string[0]), Is.Not.Null, "the row off: off");
                Assert.That(O.OffReason(on, new string[0]), Is.Null, "the row on, no switch: on");
                Assert.That(O.OffReason(on, new[] { "-skygrindNoOrder64" }), Is.Not.Null, "-skygrindNoOrder64: this row alone off");
                Assert.That(O.OffReason(on, new[] { "-skygrindnoorder64" }), Is.Not.Null, "any case");
                Assert.That(O.OffReason(on, new[] { "-skygrindOff64" }), Is.Not.Null, "-skygrindOff64");
                Assert.That(O.OffReason(on, new[] { "-skygrindOff60" }), Is.Not.Null, "an older night's switch");
                Assert.That(O.OffReason(on, new[] { "-skygrindOff65" }), Is.Null, "a later night's switch leaves it on");
                Assert.That(O.OffReason(on, new[] { "-skygrindNoGoal64" }), Is.Null, "another row's switch leaves it on");
                NightRows.ForceOffFrom = 64;
                Assert.That(O.OffReason(on, new string[0]), Is.Not.Null, "ForceOffFrom 64");
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void Switch_TheReelsLiveOverrideWins_SoTheRowIsReadEveryFrame()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride[O.Row] = false;
                Assert.That(O.OffReason(WithRow(true), new string[0]), Is.Not.Null, "the same-run toggle's held frame: off with the row on");
                NightRows.ReelOverride[O.Row] = true;
                Assert.That(O.OffReason(WithRow(false), new string[0]), Is.Null, "an override on wins over the config");
            }
            finally
            {
                NightRows.ReelOverride.Remove(O.Row);
                NightRows.ForceOffFrom = was;
            }
            Assert.That(SameRun63Math.KindOf(O.Row), Is.EqualTo(SameRun63Math.Kind.Frame), "D1 claims the row (the lead's A1: FrameRows)");
        }

        // ---- 2. the same order ----

        [Test]
        public void Same_TheBarsTextInTheFightLine()
        {
            Assert.That(O.Same(Stagger, Stagger), Is.True, "one string feeds both (UiBossBar43.cs:209-215)");
            Assert.That(O.Same("Staggered: hold left click, slug it", Stagger), Is.True, "case ignored (Case47's sentence case)");
            Assert.That(O.Same(" " + Stagger, Stagger + " "), Is.True, "edge spaces ignored");
            Assert.That(O.Same("Keep moving!", Stagger), Is.False, "a banner in the fight line is not the order");
            Assert.That(O.Same(Punch, Stagger), Is.False, "the previous order");
            Assert.That(O.Same("", ""), Is.False, "no text");
            Assert.That(O.Same(null, Stagger), Is.False);
            Assert.That(O.Same(Stagger, null), Is.False);
        }

        // ---- 3. the bar's hint ----

        [Test]
        public void Hint_RowOn_NotDrawnWhileTheFightLineDrawsTheSameOrder()
        {
            Assert.That(O.HintShown(true, false, true, Stagger, Stagger), Is.False, "gg-proxy #3: one copy, the fight line's");
            Assert.That(O.HintShown(true, false, false, Stagger, Stagger), Is.True, "the line faded (or waits behind the card): the bar shows it again");
            Assert.That(O.HintShown(true, false, true, "Keep moving!", Stagger), Is.True, "another line in the centre: the order stays in the bar");
            Assert.That(O.HintShown(true, false, true, Punch, Stagger), Is.True, "the line still draws the previous order: the new one stands in the bar");
            Assert.That(O.HintShown(true, true, false, null, Stagger), Is.False, "bossRule61's line holds the place: hidden, as without the row");
            Assert.That(O.HintShown(true, false, false, null, ""), Is.False, "no text: no holder (Text33.Set)");
        }

        [Test]
        public void Hint_RowOff_AlwaysAsWithoutTheRow()
        {
            Assert.That(O.HintShown(false, false, true, Stagger, Stagger), Is.True, "the duplicate NIGHT63 drew (the red)");
            Assert.That(O.HintShown(false, false, false, Stagger, Stagger), Is.True);
            Assert.That(O.HintShown(false, true, true, Stagger, Stagger), Is.False, "bossRule61's hold");
            Assert.That(O.HintShown(false, false, true, Stagger, ""), Is.False);
            foreach (bool rule in new[] { false, true })
                foreach (bool drawn in new[] { false, true })
                    foreach (string h in new[] { Stagger, "", null })
                        Assert.That(O.HintShown(false, rule, drawn, Stagger, h), Is.EqualTo(O.OffShown(rule, h)), "off: exactly BossRule61.HintShown");
        }

        // ---- 4. the counter ----

        /// <summary>One planted order on a boss: `pending` frames before the fight line draws it (the bar alone), `line` frames drawn by the fight
        /// line, `after` frames after it faded; the hint drawn as the HUD would draw it with the row `rowOn`. Held frames (the same-run pair) at
        /// `heldAt` (a frame index into the fight line's frames), two of them: the pair's on half (the row as it is) and its off half.</summary>
        static O.Count Plant(bool rowOn, int pending, int line, int after, int heldAt, out int ends)
        {
            var c = new O.Count();
            c.Reset("THE KILN WYRM");
            int n = 0;
            void F(bool held, bool row, bool drawn)
            {
                bool off = O.OffShown(false, Stagger);
                bool hint = O.HintShown(row, false, drawn, Stagger, Stagger);
                if (c.Frame(held, row, drawn, Stagger, Stagger, off, hint)) n++;
            }
            for (int i = 0; i < pending; i++) F(false, rowOn, false);
            for (int i = 0; i < line; i++)
            {
                F(false, rowOn, true);
                if (i == heldAt) { F(true, rowOn, true); F(true, false, true); }
            }
            for (int i = 0; i < after; i++) F(false, rowOn, false);
            ends = n;
            return c;
        }

        [Test]
        public void Counter_RowOff_EveryLineFrameIsADuplicateShown()
        {
            O.Count c = Plant(false, 5, 10, 5, -1, out int ends);
            Assert.That(c.Frames, Is.EqualTo(20));
            Assert.That(c.OnFrames, Is.EqualTo(0));
            Assert.That(c.WouldDup, Is.EqualTo(10), "the fight line's 10 frames with the bar's order");
            Assert.That(c.DupShown, Is.EqualTo(10), "NIGHT63's HUD: both drawn on every one (the red)");
            Assert.That(ends, Is.EqualTo(1), "one order line when the fight line stops drawing it");
            Assert.That(c.Orders, Is.EqualTo(1));
            Assert.That(c.OrdersShown, Is.EqualTo(1));
        }

        [Test]
        public void Counter_RowOn_WouldDuplicateButNoneShown_TheOrderBackInTheBarAfter()
        {
            O.Count c = Plant(true, 5, 10, 5, -1, out int ends);
            Assert.That(c.Frames, Is.EqualTo(20));
            Assert.That(c.OnFrames, Is.EqualTo(20));
            Assert.That(c.WouldDup, Is.EqualTo(10), "the same frames would duplicate without the row");
            Assert.That(c.DupShown, Is.EqualTo(0), "U1's doneWhen: duplicate frames shown 0");
            Assert.That(ends, Is.EqualTo(1));
            Assert.That(c.RunText, Is.EqualTo(Stagger));
            Assert.That(c.RunFrames, Is.EqualTo(10));
            Assert.That(c.RunWould, Is.EqualTo(10));
            Assert.That(c.RunShown, Is.EqualTo(0));
            Assert.That(c.Orders, Is.EqualTo(1));
            Assert.That(c.OrdersShown, Is.EqualTo(1), "the count of orders shown is the same on and off");
            Assert.That(c.InLine, Is.EqualTo(1));
            Assert.That(c.InBar, Is.EqualTo(1), "before the line (5 frames) and after it (5)");
            Assert.That(c.BarAfter, Is.EqualTo(1), "the bar's hint shows the order after the fight line fades");
        }

        [Test]
        public void Counter_HeldFramesApart_TheOffHalfShowsTheDuplicate()
        {
            O.Count c = Plant(true, 5, 10, 5, 4, out _);
            Assert.That(c.Frames, Is.EqualTo(20), "held frames are not the run's");
            Assert.That(c.WouldDup, Is.EqualTo(10));
            Assert.That(c.DupShown, Is.EqualTo(0));
            Assert.That(c.Held, Is.EqualTo(2), "the pair's two held frames");
            Assert.That(c.HeldWould, Is.EqualTo(2));
            Assert.That(c.HeldShown, Is.EqualTo(1), "the off half (the row off) draws both: the SPOT pair's other half");
        }

        [Test]
        public void Counter_OrdersShownTheSameOnAndOff_AnOrderTheLineNeverDraws()
        {
            foreach (bool rowOn in new[] { false, true })
            {
                var c = new O.Count();
                c.Reset("THE LOOM");
                // order 1 stands in the bar 3 frames, then order 2 replaces it before the fight line drew it; order 2 is drawn by the line 4
                // frames, then a banner takes the line 2 frames (not the order), then nothing 2 frames
                void F(bool drawn, string line, string hint)
                {
                    bool off = O.OffShown(false, hint);
                    c.Frame(false, rowOn, drawn, line, hint, off, O.HintShown(rowOn, false, drawn, line, hint));
                }
                for (int i = 0; i < 3; i++) F(false, null, Punch);
                for (int i = 0; i < 4; i++) F(true, Stagger, Stagger);
                for (int i = 0; i < 2; i++) F(true, "Keep moving!", Stagger);
                for (int i = 0; i < 2; i++) F(false, "Keep moving!", Stagger);
                string side = rowOn ? "row on" : "row off";
                Assert.That(c.Orders, Is.EqualTo(2), side);
                Assert.That(c.OrdersShown, Is.EqualTo(2), side + ": every order shown somewhere");
                Assert.That(c.InLine, Is.EqualTo(1), side);
                Assert.That(c.InBar, Is.EqualTo(2), side + ": order 1 in the bar; order 2 in the bar after (on) or with (off) the line");
                Assert.That(c.WouldDup, Is.EqualTo(4), side + ": the banner is not a duplicate");
                Assert.That(c.DupShown, Is.EqualTo(rowOn ? 0 : 4), side);
            }
        }

        [Test]
        public void Counter_TheRuleLinesHold_IsNoDuplicate()
        {
            var c = new O.Count();
            c.Reset("THE ICEBREAKER");
            for (int i = 0; i < 6; i++)
            {
                bool off = O.OffShown(true, Punch);
                c.Frame(false, true, true, "Parry the block. Punish stuck fists", Punch, off, O.HintShown(true, true, true, "Parry the block. Punish stuck fists", Punch));
            }
            Assert.That(c.WouldDup, Is.EqualTo(0));
            Assert.That(c.DupShown, Is.EqualTo(0));
            Assert.That(c.InBar, Is.EqualTo(0), "held by bossRule61 (N63BossRuleHintTests)");
        }

        [Test]
        public void Counter_TheLinesNameTheirNumbers()
        {
            O.Count c = Plant(true, 5, 10, 5, 4, out _);
            string t = c.Line("the boss bar went down", true, 17.25);
            StringAssert.StartsWith("[order64] THE KILN WYRM | ", t);
            StringAssert.Contains("row order64 on 20 of 20 frames", t);
            StringAssert.Contains("would-duplicate 10 frames", t);
            StringAssert.Contains("duplicate shown 0 frames", t);
            StringAssert.Contains("held 2 frames: would-duplicate 2, duplicate shown 1", t);
            StringAssert.Contains("orders 1: shown 1 (by the fight line 1, in the bar 1, in the bar after the fight line 1)", t);
            StringAssert.Contains("the boss bar went down", t);
            StringAssert.Contains("t 17.25 s", t);
            string o = c.OrderLine(true, true, 17.25);
            StringAssert.StartsWith("[order64] THE KILN WYRM order \"" + Stagger + "\"", o);
            StringAssert.Contains("drawn by the fight line 10 frames: would-duplicate 10, duplicate shown 0", o);
            StringAssert.Contains("the bar's hint after it: drawn", o);
        }

        // ---- 5. the hook's place ----

        [Test]
        public void Hook_TheWriteComesAfterFight47Tick()
        {
            string src = File.ReadAllText(Path.Combine(Repo(), "Assets", "Scripts", "Game", "UiHud47.Boss.cs"));
            int late = src.IndexOf("void Fight47Late(", StringComparison.Ordinal);
            int next = src.IndexOf("void Card47Size(", StringComparison.Ordinal);
            Assert.That(late, Is.GreaterThan(0));
            Assert.That(next, Is.GreaterThan(late));
            string body = src.Substring(late, next - late);
            int tick = body.IndexOf("Fight47Tick(frameDt);", StringComparison.Ordinal);
            int write = body.IndexOf("Order64Late();", StringComparison.Ordinal);
            Assert.That(tick, Is.GreaterThan(0), "Fight47Late ticks the fight line (and bossRule61's hold inside it)");
            Assert.That(write, Is.GreaterThan(tick), "order64's write is the frame's last on the bar's hint");
        }
    }
}
