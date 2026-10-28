// NIGHT71 builder F (LEFTOVER L4, design.n71.wash71; Game/Wash71.cs). Pinned:
//   1. the switch: off when the row is off, under -skygrindOff71 (or older), -skygrindNoWash71 (any case), ForceOffFrom 71, the same-run
//      toggle's held frame; on otherwise; off when a number is missing; it draws only with polish53 on (a153's OFF side forces it off);
//   2. the repo keeps it OFF, with its numbers (popFrameClock 1, popK in 0..1);
//   3. the pop's clock: a kill's pop on the frame dt at popK x the peak while live; a blast's pop, or the row off, the world dt and the peak;
//   4. polish53's killWash item is live while the row is on (its own "on" 0 in the repo), and not with it off.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using W = Skygrind.Game.Wash71;

namespace Skygrind.Game.Tests
{
    public class N71WashTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        static Dictionary<string, object> RowOf(GameConfig c) =>
            (Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)c.root["design"])["n71"])[W.Row];

        static GameConfig WithRow(bool on)
        {
            GameConfig c = Cfg();
            RowOf(c)["enabled"] = on;
            return c;
        }

        static void Clean(int was)
        {
            NightRows.ReelOverride.Remove(W.Row);
            NightRows.ForceOffFrom = was;
            Polish53.Force = null;
        }

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                Clean(NightRows.NoOff);
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(W.OffReason(null, new string[0]), Is.Not.Null, "no config");
                Assert.That(W.OffReason(off, new string[0]), Is.Not.Null, "the row off");
                Assert.That(W.OffReason(on, new string[0]), Is.Null, "the row on, no switch: on");
                Assert.That(W.OffReason(on, new[] { "-skygrindNoWash71" }), Is.Not.Null, "this row alone off");
                Assert.That(W.OffReason(on, new[] { "-skygrindnowash71" }), Is.Not.Null, "any case");
                Assert.That(W.OffReason(on, new[] { "-skygrindOff71" }), Is.Not.Null);
                Assert.That(W.OffReason(on, new[] { "-skygrindOff53" }), Is.Not.Null, "an older night's switch");
                Assert.That(W.OffReason(on, new[] { "-skygrindOff72" }), Is.Null, "a later night's switch leaves it on");
                Assert.That(W.OffReason(on, new[] { "-skygrindNoKicks71" }), Is.Null, "another row's switch leaves it on");
                NightRows.ForceOffFrom = 71;
                Assert.That(W.OffReason(on, new string[0]), Is.Not.Null, "ForceOffFrom 71");
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride[W.Row] = false;
                Assert.That(W.OffReason(on, new string[0]), Is.Not.Null, "the same-run toggle's held frame: off with the row on");
                NightRows.ReelOverride.Remove(W.Row);
                Polish53.Force = false;
                Assert.That(W.Live(on), Is.False, "polish53 off (a153's OFF side): not drawn");
                Polish53.Force = null;
            }
            finally { Clean(was); }
            Assert.That(SameRun63Math.KindOf(W.Row), Is.EqualTo(SameRun63Math.Kind.Frame), "D1 claims the row (the shared commit's FrameRows)");
        }

        [Test]
        public void Repo_KeepsTheRowOff_WithItsNumbers()
        {
            GameConfig c = Cfg();
            Assert.That(RowOf(c)["enabled"], Is.EqualTo(false), "OFF in the repo until GG's keep");
            W.Numbers n = W.Of(c);
            Assert.That(n, Is.Not.Null, "popFrameClock and popK in design.n71.wash71.numbers");
            Assert.That(n.popFrameClock, Is.True);
            Assert.That(n.popK, Is.GreaterThan(0).And.LessThanOrEqualTo(1));
            GameConfig bad = WithRow(true);
            ((Dictionary<string, object>)RowOf(bad)["numbers"]).Remove("popK");
            Assert.That(W.Of(bad), Is.Null, "a missing number: no numbers");
        }

        [Test]
        public void PopStep_AKillsPopOnTheFrameClock_ABlastsAsBefore()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                Clean(NightRows.NoOff);
                GameConfig on = WithRow(true), off = WithRow(false);
                W.Numbers n = W.Of(on);
                double dt = W.PopStep(on, true, 0, 1 / 60.0, 60, out double peak);
                Assert.That(dt, Is.EqualTo(1 / 60.0), "a kill's pop in a freeze (world dt 0) runs on the frame's dt");
                Assert.That(peak, Is.EqualTo(60 * n.popK), "at popK x the peak");
                dt = W.PopStep(on, false, 0, 1 / 60.0, 60, out peak);
                Assert.That(dt, Is.EqualTo(0) , "a blast's pop: the world dt");
                Assert.That(peak, Is.EqualTo(60), "a blast's pop: its peak");
                dt = W.PopStep(off, true, 0, 1 / 60.0, 60, out peak);
                Assert.That(dt, Is.EqualTo(0), "the row off: the world dt (held by the freeze, as shipped)");
                Assert.That(peak, Is.EqualTo(60));
            }
            finally { Clean(was); }
        }

        [Test]
        public void KillWashItem_LiveWithTheRow()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                Clean(NightRows.NoOff);
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(Polish53.Item(off, "killWash"), Is.False, "the repo: killWash.on 0 (DECIDE53 D-15)");
                Assert.That(Polish53.Item(on, "killWash"), Is.True, "wash71 on: killWash's cuts live");
                Assert.That(Polish53.Item(on, "scorchCore"), Is.EqualTo(Polish53.Item(off, "scorchCore")), "another polish53 item unchanged");
            }
            finally { Clean(was); }
        }
    }
}
