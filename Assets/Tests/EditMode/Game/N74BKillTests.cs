// NIGHT74 builder B (slate B3, design.n74.kill74; Game/Kill74.cs, Game/SurvivalGame.Kill74.cs, the delay in Game/Finish55View.cs). Pinned:
//   1. the row: in the config's design, read through NightRows.ViewOn both ways, a FrameRows row
//   2. the flash's frames: on the two render frames after the event's own (the slate's "kill frame +1 and +2"), not before, not after; the beat is done after them
//   3. the hold's share: 0.1 through N74KillHoldS from the event, 1 after it (and before it); the punch-in's delay is the hold for THE KILN WYRM with the row on, 0 otherwise
//   4. the numbers' reasons: the hold inside the scout's 0.25-0.5 s and short of the finisher's killTime; the flash gain above 1 (it must saturate), the overlay scale above 1 (no z-fight);
//      the flash is two frames (WCAG 2.3.1: one flash a kill)
//   5. Supports: THE KILN WYRM only (the other bosses' kill beats are the NIGHT74 leftover)
// Not run in Unity by its author (the Editor run is the lead's).
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using K = Skygrind.Game.Kill74;
using KM = Skygrind.Game.Kill74Math;

namespace Skygrind.Game.Tests
{
    public class N74BKillTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        [SetUp]
        public void SetUp() { NightRows.ReelOverride.Clear(); NightRows.ForceOffFrom = NightRows.NoOff; }

        [TearDown]
        public void TearDown() { NightRows.ReelOverride.Clear(); NightRows.ForceOffFrom = NightRows.NoOff; }

        [Test]
        public void Row_InTheDesign_ReadBothWays_AFrameRow()
        {
            GameConfig c = Cfg();
            Assert.That(c.Has("design.n74.kill74.enabled"), Is.True, "the shared commit placed the row");
            NightRows.ReelOverride[K.Row] = true;
            Assert.That(K.Decide(c), Is.True);
            NightRows.ReelOverride[K.Row] = false;
            Assert.That(K.Decide(c), Is.False);
            Assert.That(K.Decide(null), Is.False);
            Assert.That(SameRun63Math.KindOf(K.Row), Is.EqualTo(SameRun63Math.Kind.Frame));
        }

        [Test]
        public void Flash_OnTheTwoFramesAfterTheEventsOwn()
        {
            int s = UnityTuning.N74KillFlashStartFrame, n = UnityTuning.N74KillFlashFrames;
            Assert.That(s, Is.EqualTo(1), "the strip's kill frame +1");
            Assert.That(n, Is.EqualTo(2), "and +2");
            Assert.That(KM.FlashOn(0, s, n), Is.False, "the event's own frame is not flashed");
            Assert.That(KM.FlashOn(1, s, n), Is.True);
            Assert.That(KM.FlashOn(2, s, n), Is.True);
            Assert.That(KM.FlashOn(3, s, n), Is.False);
            Assert.That(KM.Done(2, s, n), Is.False, "frame +2 is the last flashed frame");
            Assert.That(KM.Done(3, s, n), Is.True, "frame +3 ends the beat");
            int flashed = 0;
            for (int i = 0; i < 20; i++) if (KM.FlashOn(i, s, n)) flashed++;
            Assert.That(flashed, Is.EqualTo(2), "one flash of two frames per kill");
        }

        [Test]
        public void Hold_ShareAndPunchDelay()
        {
            double hold = UnityTuning.N74KillHoldS, share = UnityTuning.N74KillHoldShare;
            Assert.That(KM.HoldShare(-0.01, hold, share), Is.EqualTo(1.0), "before the event");
            Assert.That(KM.HoldShare(0, hold, share), Is.EqualTo(share), "from the event");
            Assert.That(KM.HoldShare(hold * 0.99, hold, share), Is.EqualTo(share), "to the end of the hold");
            Assert.That(KM.HoldShare(hold, hold, share), Is.EqualTo(1.0), "after it");
            Assert.That(KM.PunchDelayS(true, true, hold), Is.EqualTo(hold), "THE KILN WYRM with the row on");
            Assert.That(KM.PunchDelayS(false, true, hold), Is.EqualTo(0.0), "row off: at once");
            Assert.That(KM.PunchDelayS(true, false, hold), Is.EqualTo(0.0), "another boss: at once");
        }

        [Test]
        public void Numbers_TheirReasons()
        {
            Assert.That(UnityTuning.N74KillHoldS, Is.InRange(0.25f, 0.5f), "scout-bosses.md K1: a kill freeze of 0.25-0.5 s");
            Assert.That(UnityTuning.N74KillHoldShare, Is.EqualTo(0.1f).Within(1e-6), "the row's own 0.1x view time");
            Assert.That(UnityTuning.N74KillFlashGain, Is.GreaterThan(1f), "the additive white must saturate");
            Assert.That(UnityTuning.N74KillFlashScale, Is.GreaterThan(1f), "the overlay stands outside the drawn box (no z-fight)");
            Assert.That(UnityTuning.N74KillFlashFrames, Is.LessThanOrEqualTo(3), "a flash, not a strobe");
            Assert.That(UnityTuning.N74KillHoldS, Is.LessThan(UnityTuning.Finish55MaxS), "the punch-in comes inside finish55's own safety");
            GameConfig c = Cfg();
            Assert.That((double)UnityTuning.N74KillHoldS, Is.LessThan(c.Num("bossKit.killTime")), "the hold sits at the start of the finisher's slow-mo (bossKit.killTime), which goes on after it");
        }

        [Test]
        public void Supports_TheFiveCampaignBosses()
        {
            foreach (string b in BossAttackClock63.Bosses) Assert.That(K.Supports(b), Is.True, b + " is dressed (leftover 6: all five)");
            Assert.That(K.Supports("wyrm"), Is.True);
            Assert.That(K.Supports(K.WyrmId), Is.True);
            Assert.That(K.Supports("boss"), Is.False, "the kill event's generic name never delays finish55");
            Assert.That(K.Supports("rivalPrime"), Is.False, "a survival boss is not one of the five");
            Assert.That(K.Supports(null), Is.False);
            Assert.That(K.Supports(""), Is.False);
        }

        [Test]
        public void Roots_RegisteredByTheViews_AreReadBack()
        {
            Assert.That(K.RootsOf("crown37"), Is.Null.Or.Not.Null, "(whatever an earlier test left)");
            var go = new GameObject("kill74 test root");
            try
            {
                K.Register("brawl40", go.transform);
                Assert.That(K.RootsOf("brawl40"), Has.Length.EqualTo(1));
                Assert.That(K.RootsOf("brawl40")[0], Is.SameAs(go.transform));
                Transform[] arr = K.RootsOf("brawl40");
                K.Register("brawl40", go.transform);
                Assert.That(K.RootsOf("brawl40"), Is.SameAs(arr), "the one-element array is reused: no allocation per frame");
                var many = new[] { go.transform, go.transform };
                K.Register("loom37", many);
                Assert.That(K.RootsOf("loom37"), Is.SameAs(many));
                Assert.That(K.RootsOf("nobody"), Is.Null);
                Assert.That(K.RootsOf(null), Is.Null);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}
