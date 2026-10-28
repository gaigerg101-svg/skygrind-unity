// NIGHT63 builder D (slate D1, same-run SPOT pairs; the red team's finding 2, Research/2026-10-22-forge63/redteam.md): the reel's live override
// of view rows (NightRows.ReelOverride, NightRows.Live). Pinned:
//   1. no override: ViewOn reads exactly as before (its design default, the off switch), for every row D1 claims and for an ON row
//   2. an override OFF turns a configured-on view row off; removing it gives the config's answer back
//   3. an override ON turns a row on that the config lacks or has off (the planted rows plant63 / noop63; a reel only)
//   4. an override is per row: the others read their config
//   5. Live(decided, row): the row's own decision (read once) unless the reel overrides the row (allocation-free, read every frame)
//   6. each row D1 claims (exitFrame61, bossRule61, bossMotion63, bossTell63, bossColours63, colours63) follows the override both ways
//   7. the sim never sees it: the sim rows' defaults and their tape record are the same bytes with an override set for every sim row's name
//   8. (the lead's hint, 2026-10-22: these rows are read every frame) ViewOn's answer with no override is NIGHT62's formula's for EVERY design row
//      of the config, under no off switch and under each night's; the override wins over the off switch, the off switch over the design;
//      and ViewOn / Live allocate nothing after a row's first call (with and without an override set)
// Written test first (NIGHT63.md section 4): RED on its commit (ViewOn ignores the override and builds its path string every call, Live
// answers `decided`); the Editor run is the lead's.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class N63SameRunTests
    {
        static readonly string[] Claimed = { "exitFrame61", "bossRule61", "bossMotion63", "bossTell63", "bossColours63", "colours63" };

        static GameConfig Load() => GameConfig.Load(System.IO.Path.Combine(Port.Root, "config.json"));

        /// <summary>The repo's config with `row` (design.nNN.row) set to `on` in memory.</summary>
        static GameConfig With(GameConfig c, string row, bool on)
        {
            var design = (Dictionary<string, object>)c.root["design"];
            var block = (Dictionary<string, object>)design["n" + NightRows.NightOfName(row)];
            ((Dictionary<string, object>)block[row])["enabled"] = on;
            return c;
        }

        [SetUp]
        public void Clean()
        {
            NightRows.ReelOverride.Clear();
            NightRows.ForceOffFrom = NightRows.NoOff;
        }

        [TearDown]
        public void Restore()
        {
            NightRows.ReelOverride.Clear();
            NightRows.ForceOffFrom = NightRows.NoOff;
        }

        [Test]
        public void NoOverride_ViewOnIsAsBefore()
        {
            GameConfig c = Load();
            foreach (string row in Claimed)
            {
                string at = "n" + NightRows.NightOfName(row) + "." + row + ".enabled";
                Assert.That(NightRows.ViewOn(c, row), Is.EqualTo(c.Design(at)), row + " reads its design default");
                NightRows.ForceOffFrom = NightRows.NightOfName(row);
                Assert.That(NightRows.ViewOn(c, row), Is.False, row + " under its night's off switch");
                NightRows.ForceOffFrom = NightRows.NoOff;
            }
            Assert.That(NightRows.ViewOn(c, "gunYellow61"), Is.EqualTo(c.Design("n61.gunYellow61.enabled")), "an ON row reads its design default");
            Assert.That(NightRows.ViewOn(c, "plant63"), Is.False, "a row the config lacks is off");
        }

        [Test]
        public void OverrideOff_TurnsAnOnRowOff_AndRemovingItGivesTheConfigBack()
        {
            GameConfig c = With(Load(), "exitFrame61", true);
            Assert.That(NightRows.ViewOn(c, "exitFrame61"), Is.True, "on in the config");
            NightRows.ReelOverride["exitFrame61"] = false;
            Assert.That(NightRows.ViewOn(c, "exitFrame61"), Is.False, "the reel's off frame");
            NightRows.ReelOverride.Remove("exitFrame61");
            Assert.That(NightRows.ViewOn(c, "exitFrame61"), Is.True, "restored");
        }

        [Test]
        public void OverrideOn_TurnsAMissingRowOn_ThePlantedRows()
        {
            GameConfig c = Load();
            foreach (string row in new[] { "plant63", "noop63" })
            {
                Assert.That(NightRows.ViewOn(c, row), Is.False, row + " is not in the config");
                NightRows.ReelOverride[row] = true;
                Assert.That(NightRows.ViewOn(c, row), Is.True, row + " forced on by the reel");
                NightRows.ForceOffFrom = 63;
                Assert.That(NightRows.ViewOn(c, row), Is.True, row + ": the override wins over the off switch (the reel's planted case)");
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride[row] = false;
                Assert.That(NightRows.ViewOn(c, row), Is.False, row + " in its off frame");
            }
        }

        [Test]
        public void Override_IsPerRow()
        {
            GameConfig c = Load();
            foreach (string row in Claimed) With(c, row, true);
            NightRows.ReelOverride["bossRule61"] = false;
            foreach (string row in Claimed)
                Assert.That(NightRows.ViewOn(c, row), Is.EqualTo(row != "bossRule61"), row);
        }

        [Test]
        public void Live_IsTheRowsOwnDecision_UnlessTheReelOverridesIt()
        {
            Assert.That(NightRows.Live(true, "exitFrame61"), Is.True);
            Assert.That(NightRows.Live(false, "exitFrame61"), Is.False);
            NightRows.ReelOverride["exitFrame61"] = false;
            Assert.That(NightRows.Live(true, "exitFrame61"), Is.False, "the off frame");
            Assert.That(NightRows.Live(true, "colours63"), Is.True, "another row");
            NightRows.ReelOverride["plant63"] = true;
            Assert.That(NightRows.Live(false, "plant63"), Is.True, "forced on");
            Assert.That(NightRows.Live(true, null), Is.True, "no row name: the decision");
        }

        [Test]
        public void EachClaimedRow_FollowsTheOverrideBothWays()
        {
            GameConfig c = Load();
            foreach (string row in Claimed) With(c, row, true);
            foreach (string row in Claimed)
            {
                Assert.That(NightRows.ViewOn(c, row), Is.True, row + " on in memory");
                NightRows.ReelOverride[row] = false;
                Assert.That(NightRows.ViewOn(c, row), Is.False, row + ": ViewOn in the off frame");
                Assert.That(NightRows.Live(true, row), Is.False, row + ": Live in the off frame");
                NightRows.ReelOverride.Remove(row);
                Assert.That(NightRows.ViewOn(c, row), Is.True, row + " back on");
                Assert.That(NightRows.Live(true, row), Is.True, row + " Live back on");
            }
        }

        [Test]
        public void TheSimNeverSeesIt_SimRowsAndTheirTapeRecordUnchanged()
        {
            GameConfig c = Load();
            NightRows reg = NightRows.From(c);
            Assert.That(reg.Count, Is.GreaterThan(0), "the config has sim rows");
            NightRowState before = reg.Defaults();
            byte[] tape = NightRowsTape.Encode(before);
            foreach (NightRow r in reg.rows) NightRows.ReelOverride[r.name] = !r.design;
            NightRowState during = NightRows.From(c).Defaults();
            Assert.That(during.ToString(), Is.EqualTo(before.ToString()), "the sim rows' defaults");
            Assert.That(NightRowsTape.Encode(during), Is.EqualTo(tape), "the tape record's bytes");
            for (int i = 0; i < reg.Count; i++) Assert.That(during.On(i), Is.EqualTo(before.On(i)), reg.rows[i].name);
        }

        /// <summary>NIGHT62's ViewOn, verbatim (the string built every call): the answer the new one must give with no override.</summary>
        static bool OldViewOn(GameConfig cfg, string row)
        {
            int nn = NightRows.NightOfName(row);
            return nn >= NightRows.FirstNight && nn < NightRows.ForceOffFrom && cfg != null && cfg.Design("n" + nn + "." + row + ".enabled");
        }

        [Test]
        public void NoOverride_EveryDesignRowReadsAsNight62s_UnderEveryOffSwitch()
        {
            GameConfig c = Load();
            var rows = new List<string>();
            foreach (KeyValuePair<string, object> night in c.Obj("design"))
                if (night.Value is Dictionary<string, object> block)
                    foreach (string row in block.Keys) rows.Add(row);
            rows.AddRange(new[] { "plant63", "noop63", "foo.bar61", "x", "", "nonight" });
            Assert.That(rows.Count, Is.GreaterThan(50), "the config's rows");
            int on = 0;
            foreach (int off in new[] { NightRows.NoOff, 46, 55, 61, 62, 63, 64 })
            {
                NightRows.ForceOffFrom = off;
                foreach (string row in rows)
                {
                    bool want = OldViewOn(c, row);
                    Assert.That(NightRows.ViewOn(c, row), Is.EqualTo(want), row + " under ForceOffFrom " + off);
                    Assert.That(NightRows.ViewOn(c, row), Is.EqualTo(want), row + " a second call (cached)");
                    if (want) on++;
                }
            }
            Assert.That(on, Is.GreaterThan(0), "some rows are on (a run of all-false proves nothing)");
            NightRows.ForceOffFrom = NightRows.NoOff;
            Assert.That(NightRows.ViewOn(null, "exitFrame61"), Is.False, "no config");
            Assert.That(NightRows.ViewOn(c, null), Is.False, "no row");
        }

        [Test]
        public void Order_OverrideWinsOverTheOffSwitch_TheOffSwitchOverTheDesign()
        {
            GameConfig c = With(Load(), "colours63", true);
            Assert.That(NightRows.ViewOn(c, "colours63"), Is.True, "the design");
            NightRows.ForceOffFrom = 63;
            Assert.That(NightRows.ViewOn(c, "colours63"), Is.False, "-skygrindOff63 wins over the design");
            NightRows.ReelOverride["colours63"] = true;
            Assert.That(NightRows.ViewOn(c, "colours63"), Is.True, "the reel's override wins over the off switch");
            NightRows.ReelOverride["colours63"] = false;
            NightRows.ForceOffFrom = NightRows.NoOff;
            Assert.That(NightRows.ViewOn(c, "colours63"), Is.False, "the reel's off frame wins over the design");
        }

        [Test]
        public void ViewOnAndLive_AllocateNothingAfterARowsFirstCall()
        {
            GameConfig c = Load();
            foreach (string row in Claimed) With(c, row, true);
            foreach (string row in Claimed) { NightRows.ViewOn(c, row); NightRows.Live(true, row); } // a row's first call: its key cached, the JIT
            int none = Allocs(() =>
            {
                for (int i = 0; i < 200; i++)
                    foreach (string row in Claimed) { NightRows.ViewOn(c, row); NightRows.Live(true, row); }
            });
            Assert.That(none, Is.EqualTo(0), "no override: GC allocations over 200 frames of the six rows");
            NightRows.ReelOverride["exitFrame61"] = false;
            NightRows.ViewOn(c, "exitFrame61");
            int some = Allocs(() =>
            {
                for (int i = 0; i < 200; i++)
                    foreach (string row in Claimed) { NightRows.ViewOn(c, row); NightRows.Live(true, row); }
            });
            Assert.That(some, Is.EqualTo(0), "an override set: GC allocations over 200 frames of the six rows");
        }

#if UNITY_2017_1_OR_NEWER
        /// <summary>GC allocation samples made by `a` on this thread (TrapsTests.Allocs).</summary>
        static int Allocs(Action a)
        {
            var recorder = UnityEngine.Profiling.Recorder.Get("GC.Alloc");
            recorder.enabled = false;
            recorder.FilterToCurrentThread();
            recorder.enabled = true;
            try
            {
                a();
            }
            finally
            {
                recorder.enabled = false;
                recorder.CollectFromAllThreads();
            }
            return recorder.sampleBlockCount;
        }
#else
        /// <summary>Bytes allocated by `a` on this thread (outside Unity: the scratch .NET run).</summary>
        static int Allocs(Action a)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            a();
            return (int)(GC.GetAllocatedBytesForCurrentThread() - before);
        }
#endif
    }
}
