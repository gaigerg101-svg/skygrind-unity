// NIGHT71 builder B (slate B1 "one order at a time on THE CROWN", design.n71.voice71; Game/Voice71.cs, the HUD's read in Game/UiVoice71.cs,
// THE CROWN's write in Game/Crown37View.Voice71.cs, its hook in Crown37View.Crown40.cs Crown40Draw). Pinned:
//   1. the switch: off when the row is off, under -skygrindOff71 (or older), -skygrindNoVoice71 (any case), ForceOffFrom 71; on otherwise;
//      the reel's live override (D1's same-run toggle) wins, so the row is read every frame; the row is one D1 claims (SameRun63Math.FrameRows)
//   2. the call: with the row on, the window call is not drawn on a frame the HUD draws the window order; every other call ("BLOCKED",
//      a window call with no HUD order beside it) as without the row; with the row off always drawn
//   3. the counter: would both = the window call set while the HUD draws the window order (the red, in the same run); both drawn = 0 with
//      the row on; the same-run toggle's held frames apart; one window line when the window call ends
// Not run in Unity by its author (the Editor run is the lead's).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using V = Skygrind.Game.Voice71;

namespace Skygrind.Game.Tests
{
    public class N71VoiceTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        /// <summary>The repo's config with design.n71.voice71 set in memory to `on` (the night's block made if the copy has none).</summary>
        static GameConfig WithRow(bool on)
        {
            GameConfig c = Cfg();
            var design = (Dictionary<string, object>)c.root["design"];
            if (!(design.TryGetValue("n71", out object n) && n is Dictionary<string, object> n71))
            {
                n71 = new Dictionary<string, object> { ["_why"] = "test" };
                design["n71"] = n71;
            }
            if (n71.TryGetValue(V.Row, out object r) && r is Dictionary<string, object> row) row["enabled"] = on;
            else n71[V.Row] = new Dictionary<string, object> { ["_why"] = "test", ["enabled"] = on, ["enabled_why"] = "test" };
            return c;
        }

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride.Remove(V.Row);
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(V.OffReason(null, new string[0]), Is.Not.Null, "no config: off");
                Assert.That(V.OffReason(off, new string[0]), Is.Not.Null, "the row off: off");
                Assert.That(V.OffReason(on, new string[0]), Is.Null, "the row on, no switch: on");
                Assert.That(V.OffReason(on, new[] { "-skygrindNoVoice71" }), Is.Not.Null, "-skygrindNoVoice71: this row alone off");
                Assert.That(V.OffReason(on, new[] { "-skygrindnovoice71" }), Is.Not.Null, "any case");
                Assert.That(V.OffReason(on, new[] { "-skygrindOff71" }), Is.Not.Null, "-skygrindOff71");
                Assert.That(V.OffReason(on, new[] { "-skygrindOff65" }), Is.Not.Null, "an older night's switch (the play card's -skygrindOff65)");
                Assert.That(V.OffReason(on, new[] { "-skygrindNoOrder64" }), Is.Null, "another row's switch leaves it on");
                NightRows.ForceOffFrom = 71;
                Assert.That(V.OffReason(on, new string[0]), Is.Not.Null, "ForceOffFrom 71");
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
                NightRows.ReelOverride[V.Row] = false;
                Assert.That(V.OffReason(WithRow(true), new string[0]), Is.Not.Null, "the same-run toggle's held frame: off with the row on");
                NightRows.ReelOverride[V.Row] = true;
                Assert.That(V.OffReason(WithRow(false), new string[0]), Is.Null, "an override on wins over the config");
            }
            finally
            {
                NightRows.ReelOverride.Remove(V.Row);
                NightRows.ForceOffFrom = was;
            }
            Assert.That(SameRun63Math.KindOf(V.Row), Is.EqualTo(SameRun63Math.Kind.Frame), "D1 claims the row (the shared commit: FrameRows)");
        }

        [Test]
        public void Call_RowOn_TheWindowCallWaitsWhileTheHudDrawsTheOrder()
        {
            Assert.That(V.CallShown(true, true, true), Is.False, "two orders at once (bp/crown37-stagger): the call goes");
            Assert.That(V.CallShown(true, true, false), Is.True, "no HUD order (bossRule61's line holds the place, the card is up): the call stands");
            Assert.That(V.CallShown(true, false, true), Is.True, "BLOCKED (not the window's call) is never hidden");
            Assert.That(V.CallShown(true, false, false), Is.True);
        }

        [Test]
        public void Call_RowOff_AlwaysAsWithoutTheRow()
        {
            foreach (bool w in new[] { false, true })
                foreach (bool h in new[] { false, true })
                    Assert.That(V.CallShown(false, w, h), Is.True, "off: crown40's call as NIGHT70 draws it");
        }

        [Test]
        public void Counter_WouldBothIsTheRed_BothDrawnIsZeroWithTheRowOn()
        {
            var on = new V.Count();
            var off = new V.Count();
            // a window: 10 frames of the call; the fight line draws the order on 6, the bar on 2, nothing on 2 (the card up)
            for (int k = 0; k < 10; k++)
            {
                bool line = k < 6, bar = k >= 6 && k < 8;
                bool hud = line || bar;
                on.Frame(false, true, true, V.CallShown(true, true, hud), line, bar);
                off.Frame(false, false, true, V.CallShown(false, true, hud), line, bar);
            }
            Assert.That(on.Frame(false, true, false, true, false, false), Is.True, "the window call ended: a window line");
            Assert.That(off.Frame(false, false, false, true, false, false), Is.True);
            Assert.That(on.Would, Is.EqualTo(8), "the red in the same run (the game without the row)");
            Assert.That(on.Both, Is.EqualTo(0), "the row on: never both");
            Assert.That(on.CallAlone, Is.EqualTo(2), "the call stands where no HUD order is drawn");
            Assert.That(off.Both, Is.EqualTo(8), "the row off: both drawn = would both");
            Assert.That(off.Would, Is.EqualTo(8));
            Assert.That(on.Windows, Is.EqualTo(1));
            Assert.That(on.Call, Is.EqualTo(10));
            Assert.That(on.Hud, Is.EqualTo(8));
            Assert.That(on.HudLine, Is.EqualTo(6));
            Assert.That(on.HudBar, Is.EqualTo(2));
            Assert.That(on.OnFrames, Is.EqualTo(11));
            Assert.That(off.OnFrames, Is.EqualTo(0));
            Assert.That(on.Line("the quit", true, 1.0), Does.StartWith("[voice71] crown both drawn 0 frames"));
            Assert.That(off.Line("the quit", false, 1.0), Does.StartWith("[voice71] crown both drawn 8 frames"));
            Assert.That(on.WindowLine(true, 1.0), Does.Contain("both drawn 0 of 10 frames, would both 8"));
        }

        [Test]
        public void Counter_HeldFramesApart()
        {
            var c = new V.Count();
            c.Frame(false, true, true, false, true, false); // a run frame: the call hidden beside the fight line
            c.Frame(true, true, true, false, true, false); // the pair's on half (the row on: hidden)
            c.Frame(true, false, true, true, true, false); // its off half (the row off: drawn)
            Assert.That(c.Frames, Is.EqualTo(1), "held frames are not the run's");
            Assert.That(c.Held, Is.EqualTo(2));
            Assert.That(c.HeldWould, Is.EqualTo(2));
            Assert.That(c.HeldBoth, Is.EqualTo(1), "the off half draws both: the pair shows the change");
            Assert.That(c.Both, Is.EqualTo(0));
            Assert.That(c.Windows, Is.EqualTo(1), "a held frame never opens or closes a window");
        }
    }
}
