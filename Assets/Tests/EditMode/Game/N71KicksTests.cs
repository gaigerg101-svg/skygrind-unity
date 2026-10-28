// NIGHT71 builder F (F3, design.n71.kicks71; Game/Kicks71.cs). Pinned:
//   1. the switch: off when the row is off, under -skygrindOff71 (or older), -skygrindNoKicks71 (any case), ForceOffFrom 71, the same-run
//      toggle's held frame; on otherwise; off when a number is missing ("numbers incomplete", never a default typed in code);
//   2. the repo keeps it OFF, with all 15 numbers in its block, the ladder bigger than feel46's and in order (hit < body < head < punch),
//      the shot's kept shake below feel46's;
//   3. rarer: a hit kick at most once per hitGapS; a kill inside killGapS adds only what its tier has over the last kill kick's.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using K = Skygrind.Game.Kicks71;

namespace Skygrind.Game.Tests
{
    public class N71KicksTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        /// <summary>PortExport's config with the repo's design config (GameConfig.Load merges Design/unity-config.json).</summary>
        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        static Dictionary<string, object> RowOf(GameConfig c) =>
            (Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)c.root["design"])["n71"])[K.Row];

        static GameConfig WithRow(bool on)
        {
            GameConfig c = Cfg();
            RowOf(c)["enabled"] = on;
            return c;
        }

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride.Remove(K.Row);
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(K.OffReason(null, new string[0]), Is.Not.Null, "no config");
                Assert.That(K.OffReason(off, new string[0]), Is.Not.Null, "the row off");
                Assert.That(K.OffReason(on, new string[0]), Is.Null, "the row on, no switch: on");
                Assert.That(K.OffReason(on, new[] { "-skygrindNoKicks71" }), Is.Not.Null, "this row alone off");
                Assert.That(K.OffReason(on, new[] { "-skygrindnokicks71" }), Is.Not.Null, "any case");
                Assert.That(K.OffReason(on, new[] { "-skygrindOff71" }), Is.Not.Null);
                Assert.That(K.OffReason(on, new[] { "-skygrindOff46" }), Is.Not.Null, "an older night's switch");
                Assert.That(K.OffReason(on, new[] { "-skygrindOff72" }), Is.Null, "a later night's switch leaves it on");
                Assert.That(K.OffReason(on, new[] { "-skygrindNoWash71" }), Is.Null, "another row's switch leaves it on");
                NightRows.ForceOffFrom = 71;
                Assert.That(K.OffReason(on, new string[0]), Is.Not.Null, "ForceOffFrom 71");
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride[K.Row] = false;
                Assert.That(K.OffReason(on, new string[0]), Is.Not.Null, "the same-run toggle's held frame: off with the row on");
                NightRows.ReelOverride[K.Row] = true;
                Assert.That(K.OffReason(off, new string[0]), Is.Null, "an override on wins over the config");
            }
            finally
            {
                NightRows.ReelOverride.Remove(K.Row);
                NightRows.ForceOffFrom = was;
            }
            Assert.That(SameRun63Math.KindOf(K.Row), Is.EqualTo(SameRun63Math.Kind.Frame), "D1 claims the row (the shared commit's FrameRows)");
        }

        [Test]
        public void Switch_OffWhenANumberIsMissing()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride.Remove(K.Row);
                GameConfig c = WithRow(true);
                ((Dictionary<string, object>)RowOf(c)["numbers"]).Remove("killGapS");
                Assert.That(K.Of(c), Is.Null, "a missing number: no numbers");
                Assert.That(K.OffReason(c, new string[0]), Does.Contain("numbers incomplete"));
            }
            finally { NightRows.ForceOffFrom = was; }
        }

        [Test]
        public void Repo_KeepsTheRowOff_WithItsNumbers()
        {
            GameConfig c = Cfg();
            Assert.That(RowOf(c)["enabled"], Is.EqualTo(false), "OFF in the repo until GG's keep (NIGHT71: ON only in the play build)");
            K.Numbers n = K.Of(c);
            Assert.That(n, Is.Not.Null, "all 15 numbers in design.n71.kicks71.numbers");
            Assert.That(n.hitLightDeg, Is.GreaterThan(0).And.LessThanOrEqualTo(n.hitHeavyDeg));
            Assert.That(n.hitHeavyDeg, Is.LessThan(n.bodyKillDeg), "a hit kicks less than any kill");
            Assert.That(n.bodyKillDeg, Is.LessThan(n.headKillDeg), "the ladder: body < head");
            Assert.That(n.headKillDeg, Is.LessThan(n.punchKillDeg), "the ladder: head < punch");
            Assert.That(n.bodyKillDeg, Is.GreaterThan(UnityTuning.Kick46BodyKillDeg), "bigger than feel46's body kill");
            Assert.That(n.headKillDeg, Is.GreaterThan(UnityTuning.Kick46HeadKillDeg), "bigger than feel46's headshot kill");
            Assert.That(n.punchKillDeg, Is.GreaterThan(UnityTuning.Kick46PunchKillDeg), "bigger than feel46's punch kill");
            Assert.That(n.bodyKillFovDeg, Is.GreaterThan(UnityTuning.Fov46BodyKillDeg), "a bigger body-kill punch-in");
            Assert.That(n.heavyFireKeep, Is.GreaterThan(0).And.LessThan(UnityTuning.Shake46HeavyFireKeep), "the THUNDERCLAP's shot keeps less shake than feel46's");
            Assert.That(n.hitGapS, Is.GreaterThan(0), "rarer hits");
            Assert.That(n.killGapS, Is.GreaterThan(0), "merged kills");
            Assert.That(n.hitAttackS, Is.GreaterThan(0).And.LessThan(n.hitRecoverS));
            Assert.That(n.killAttackS, Is.GreaterThan(0).And.LessThan(n.killRecoverS));
            Assert.That(n.fovAttackS, Is.GreaterThan(0).And.LessThan(n.fovRecoverS));
        }

        [Test]
        public void HitKick_AtMostOncePerGap()
        {
            K.Numbers n = K.Of(Cfg());
            Assert.That(K.HitKick(n, false, 10, double.NegativeInfinity), Is.EqualTo(n.hitLightDeg), "the first hit kicks");
            Assert.That(K.HitKick(n, true, 10, double.NegativeInfinity), Is.EqualTo(n.hitHeavyDeg), "a heavy gun's hit kicks its own");
            Assert.That(K.HitKick(n, false, 10 + n.hitGapS * 0.5, 10), Is.EqualTo(0), "inside the gap: held back");
            Assert.That(K.HitKick(n, false, 10 + n.hitGapS, 10), Is.EqualTo(n.hitLightDeg), "at the gap: kicks again");
            Assert.That(K.HitKick(null, false, 10, double.NegativeInfinity), Is.EqualTo(0), "no numbers: no kick");
        }

        [Test]
        public void KillKick_AMultiKillIsOneKick_TheBiggestTiers()
        {
            K.Numbers n = K.Of(Cfg());
            double t = 20;
            Assert.That(K.KillKick(n, n.bodyKillDeg, t, double.NegativeInfinity, 0), Is.EqualTo(n.bodyKillDeg), "a first kill: its tier");
            Assert.That(K.KillKick(n, n.bodyKillDeg, t + n.killGapS * 0.5, t, n.bodyKillDeg), Is.EqualTo(0), "a second body kill inside the gap adds nothing");
            Assert.That(K.KillKick(n, n.punchKillDeg, t + n.killGapS * 0.5, t, n.bodyKillDeg), Is.EqualTo(n.punchKillDeg - n.bodyKillDeg).Within(1e-12), "a bigger tier inside the gap adds its difference");
            Assert.That(K.KillKick(n, n.bodyKillDeg, t + n.killGapS, t, n.bodyKillDeg), Is.EqualTo(n.bodyKillDeg), "at the gap: a kick of its own");
        }
    }
}
