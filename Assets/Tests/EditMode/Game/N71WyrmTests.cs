// NIGHT71 builder B (slate B3 "a wyrm motion that reads", design.n71.wyrm71; Game/Wyrm71.cs, the hook Game/WyrmView.Wyrm71.cs, the reel's
// stand Game/Stand71Rig.cs). Pinned:
//   1. the switch: off when the row is off, under -skygrindOff71 (or older), -skygrindNoWyrm71; the reel's override wins; a FrameRows row
//   2. the parameters: bossMotion63's own for THE KILN WYRM with design.n71.wyrm71.numbers over them (a number not given keeps bossMotion63's),
//      every spring valid; the guard stays inside the head (the drawn head's centre never leaves the sim's 1.5 m half-width head box)
// Not run in Unity by its author (the Editor run is the lead's).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using Y = Skygrind.Game.Wyrm71;

namespace Skygrind.Game.Tests
{
    public class N71WyrmTests
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
            ((Dictionary<string, object>)n71[Y.Row])["enabled"] = on;
            return c;
        }

        [Test]
        public void Switch_OffUnderTheRowAndItsSwitches()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride.Remove(Y.Row);
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(Y.OffReason(off, new string[0]), Is.Not.Null);
                Assert.That(Y.OffReason(on, new string[0]), Is.Null);
                Assert.That(Y.OffReason(on, new[] { "-skygrindNoWyrm71" }), Is.Not.Null);
                Assert.That(Y.OffReason(on, new[] { "-skygrindOff71" }), Is.Not.Null);
                Assert.That(Y.OffReason(on, new[] { "-skygrindNoBossMotion63" }), Is.Null, "bossMotion63's own switch is its own");
                NightRows.ReelOverride[Y.Row] = false;
                Assert.That(Y.OffReason(on, new string[0]), Is.Not.Null, "the same-run toggle's held frame");
            }
            finally
            {
                NightRows.ReelOverride.Remove(Y.Row);
                NightRows.ForceOffFrom = was;
            }
            Assert.That(SameRun63Math.KindOf(Y.Row), Is.EqualTo(SameRun63Math.Kind.Frame));
        }

        [Test]
        public void Params_TheRowsNumbersOverBossMotion63s()
        {
            GameConfig c = Cfg();
            BossMotionParams63 old = BossMotionParams63.From(c, BossAttackClock63.Wyrm), p = Y.Params(c);
            Assert.That(p.Valid, Is.True, "every spring has a frequency, the guard has room");
            Assert.That(p.gain, Is.EqualTo((float)c.Num(Y.Root + "gain")));
            Assert.That(p.tail, Is.EqualTo((float)c.Num(Y.Root + "tail")));
            Assert.That(p.breathLift, Is.EqualTo((float)c.Num(Y.Root + "breathLift")));
            Assert.That(p.slumpSquash, Is.EqualTo((float)c.Num(Y.Root + "slumpSquash")));
            Assert.That(p.gain * p.breathLift, Is.GreaterThan(old.gain * old.breathLift), "the body wave larger than NIGHT63's 2.4 cm");
            Assert.That(p.followF, Is.EqualTo(old.followF), "a number the block does not give keeps bossMotion63's");
            Assert.That(p.wave, Is.EqualTo(old.wave));
            Assert.That(p.rippleS, Is.EqualTo(old.rippleS));
            // THE GUARD (NIGHT71 B3 critic, Logs/n71/verdict-B3.md point 3): bossMotion63's own for the wyrm, its drawn boxes being its hitboxes
            Assert.That(p.limitShare, Is.EqualTo(old.limitShare), "limitShare: bossMotion63's own (bosses.wyrm 0.25)");
            Assert.That(p.limitDeg, Is.EqualTo(old.limitDeg), "limitDeg: bossMotion63's own (bosses.wyrm 14)");
            Assert.That(p.limitSquash, Is.EqualTo(old.limitSquash), "limitSquash: bossMotion63's own (the shared 0.32)");
            Assert.That(1f - p.slumpSquash * p.gain, Is.GreaterThanOrEqualTo(1f - p.limitSquash), "the full slump's squash inside the guard, not cut by it");
        }

        [Test]
        public void Params_AWiderGuardWrittenInTheBlockIsNeverTaken()
        {
            GameConfig c = Cfg(); // fresh: nothing read yet (GameConfig.Get caches per instance)
            var numbers = (Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)c.root["design"])["n71"])[Y.Row])["numbers"];
            numbers["limitShare"] = 0.9;
            numbers["limitDeg"] = 40.0;
            numbers["limitSquash"] = 0.6;
            BossMotionParams63 old = BossMotionParams63.From(c, BossAttackClock63.Wyrm), p = Y.Params(c);
            Assert.That(p.limitShare, Is.EqualTo(old.limitShare), "a planted 0.9 never widens the hitbox guard");
            Assert.That(p.limitDeg, Is.EqualTo(old.limitDeg));
            Assert.That(p.limitSquash, Is.EqualTo(old.limitSquash));
            GameConfig c2 = Cfg(); // a tighter one is taken (a fresh config: the first read)
            ((Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)c2.root["design"])["n71"])[Y.Row])["numbers"])["limitShare"] = 0.1;
            Assert.That(Y.Params(c2).limitShare, Is.EqualTo(0.1f).Within(1e-6f), "a block number may only tighten it");
        }
    }
}
