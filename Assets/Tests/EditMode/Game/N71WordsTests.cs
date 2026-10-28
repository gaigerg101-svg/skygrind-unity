// NIGHT71 builder B (slate B2 "the orders name the hand you hold", design.n71.words71; Game/Words71.cs, the HUD's hooks in Game/UiWords71.cs).
// Pinned:
//   1. the switch: off when the row is off, under -skygrindOff71 (or older), -skygrindNoWords71; the reel's override wins; a FrameRows row
//   2. the table: read from the repo's config (names per Loadout weapon id, swapWords, lines), every variant itself telling a change of hands
//      or naming no weapon
//   3. the rule: a line naming a weapon not held (and no change of hands); a line naming the hand held is fine; the grapple's "hold swings" is
//      not the gauntlet's swing
//   4. the orders: THE KILN WYRM's head order with the THUNDERCLAP held becomes the gauntlet swap, never {punch}; the slug order without the
//      THUNDERCLAP becomes the window's words; an unlisted order waits ("")
// Not run in Unity by its author (the Editor run is the lead's).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using W = Skygrind.Game.Words71;

namespace Skygrind.Game.Tests
{
    public class N71WordsTests
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
            var n71 = (Dictionary<string, object>)design["n71"];
            ((Dictionary<string, object>)n71[W.Row])["enabled"] = on;
            return c;
        }

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride.Remove(W.Row);
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(W.OffReason(off, new string[0]), Is.Not.Null);
                Assert.That(W.OffReason(on, new string[0]), Is.Null);
                Assert.That(W.OffReason(on, new[] { "-skygrindNoWords71" }), Is.Not.Null);
                Assert.That(W.OffReason(on, new[] { "-skygrindOff71" }), Is.Not.Null);
                Assert.That(W.OffReason(on, new[] { "-skygrindOff65" }), Is.Not.Null);
                Assert.That(W.OffReason(on, new[] { "-skygrindNoVoice71" }), Is.Null, "another row's switch");
                NightRows.ReelOverride[W.Row] = false;
                Assert.That(W.OffReason(on, new string[0]), Is.Not.Null, "the same-run toggle's held frame");
            }
            finally
            {
                NightRows.ReelOverride.Remove(W.Row);
                NightRows.ForceOffFrom = was;
            }
            Assert.That(SameRun63Math.KindOf(W.Row), Is.EqualTo(SameRun63Math.Kind.Frame));
        }

        [Test]
        public void Table_ReadFromTheRepoConfig()
        {
            W.Table t = W.Of(Cfg());
            Assert.That(t.Weapons, Does.Contain("gauntlet").And.Contain("shotgun").And.Contain("revolver"));
            Assert.That(t.SwapWords, Does.Contain("SWAP"));
            Assert.That(t.Lines.ContainsKey("PUNCH THE HEAD TO STAGGER IT"), Is.True, "THE KILN WYRM's idle order (design.n43.wick43.bosses.wyrmIdle)");
            foreach (KeyValuePair<string, string> kv in t.Lines)
                Assert.That(W.Swaps(t, kv.Value) || W.Named(t, kv.Value) == null, Is.True, "a variant names the hand to take or no weapon: " + kv.Value);
        }

        [Test]
        public void Rule_ALineNamingAWeaponNotHeld()
        {
            W.Table t = W.Of(Cfg());
            Assert.That(W.NotHeld(t, "One LEFT CLICK swing drops a tank.", "revolver"), Is.True, "gg-proxy vi/f0400 (the KESTREL held)");
            Assert.That(W.NotHeld(t, "One LEFT CLICK swing drops a tank.", "gauntlet"), Is.False, "the gauntlet in hand");
            Assert.That(W.NotHeld(t, "PUNCH THE HEAD TO STAGGER IT", "shotgun"), Is.True, "gg-proxy bk/wyrm-windup (the THUNDERCLAP held)");
            Assert.That(W.NotHeld(t, "SWAP TO THE GAUNTLET: PUNCH ITS HEAD", "shotgun"), Is.False, "a change of hands names the hand to take");
            Assert.That(W.NotHeld(t, "Tap G to pull, hold swings.", "revolver"), Is.False, "the grapple's swing is not the gauntlet's");
            Assert.That(W.NotHeld(t, "STAGGERED: EVERY HIT COUNTS", "revolver"), Is.False, "no weapon named");
            Assert.That(W.NotHeld(t, "", "revolver"), Is.False);
            Assert.That(W.NotHeld(t, null, "revolver"), Is.False);
        }

        [Test]
        public void Orders_TheWyrmNamesTheGauntletNeverThePunch()
        {
            W.Table t = W.Of(Cfg());
            string idle = W.OrderFor(t, "PUNCH THE HEAD TO STAGGER IT", "PUNCH THE HEAD TO STAGGER IT", "shotgun");
            Assert.That(idle, Is.EqualTo("SWAP TO THE GAUNTLET: PUNCH ITS HEAD"));
            Assert.That(idle, Does.Not.Contain("{punch}"), "the fist does not stagger the head with wyrmTell48 off");
            Assert.That(W.OrderFor(t, "PUNCH THE HEAD TO STAGGER IT", "PUNCH THE HEAD TO STAGGER IT", "gauntlet"), Is.Null, "the gauntlet held: as it is");
            Assert.That(W.OrderFor(t, "STAGGERED: HOLD {fire}, SLUG IT", "STAGGERED: HOLD LEFT CLICK, SLUG IT", "gauntlet"), Is.EqualTo("STAGGERED: EVERY HIT COUNTS"));
            Assert.That(W.OrderFor(t, "STAGGERED: HOLD {fire}, SLUG IT", "STAGGERED: HOLD LEFT CLICK, SLUG IT", "shotgun"), Is.Null);
            Assert.That(W.OrderFor(t, "THE TORCH BURNS IT", "THE TORCH BURNS IT", "revolver"), Is.EqualTo(""), "an order not listed waits");
            Assert.That(W.OrderFor(t, "STAGGERED: EVERY HIT COUNTS", "STAGGERED: EVERY HIT COUNTS", "revolver"), Is.Null);
            Assert.That(W.LessonFor(t, "One LEFT CLICK swing drops a tank.", "revolver"), Is.Null, "the swing lesson waits");
            Assert.That(W.LessonFor(t, "One LEFT CLICK swing drops a tank.", "gauntlet"), Is.EqualTo("One LEFT CLICK swing drops a tank."));
        }

        [Test]
        public void Counter_HeldFramesApart()
        {
            var c = new W.Count();
            c.Frame(false, false, true, true);
            c.Frame(false, true, false, true);
            c.Frame(true, false, true, true);
            Assert.That(c.Frames, Is.EqualTo(2));
            Assert.That(c.Shown, Is.EqualTo(1));
            Assert.That(c.Would, Is.EqualTo(2));
            Assert.That(c.Held, Is.EqualTo(1));
            Assert.That(c.HeldShown, Is.EqualTo(1));
            Assert.That(c.Line("the quit", true, 1), Does.StartWith("[words71] lines naming a weapon not held 1 frames"));
        }
    }
}
