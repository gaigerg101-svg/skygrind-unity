// NIGHT53 builder U: design.n53.polish53's pure rules (Polish53.cs). Pinned:
//   1. every U item's numbers hold an "on" of 0 or 1 with its _why, and the values the code reads (killWash.whiteFrames >= 1, scorchCore.glow > 0,
//      icelineGrade.lift > 1, killPop's punch > 1 and its times > 0)
//   2. the row off (the shared commit's default): every item reads off, K's kill cap and look52's lift / grade list are NIGHT52's exactly
//   3. the row on: the white frames take killWash.whiteFrames (a gun kill with FeelAB only), the scorch glow scorchCore.glow, iceline's slot
//      icelineGrade.lift and look52's grade list iceline's ids (span's slot unchanged)
//   4. the off switches (--off53, --off52) turn every item off with the row on
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N53PolishTests
    {
        static readonly string[] Items = { "firstShot", "killWash", "scorchCore", "icelineGrade", "killPop" };

        static string PortRoot()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (Exception) { return Path.GetFullPath("PortExport"); }
        }

        static GameConfig Cfg(bool on)
        {
            GameConfig c = GameConfig.Load(Path.Combine(PortRoot(), "config.json"));
            c.Obj("design.n53.polish53")["enabled"] = on;
            return c;
        }

        static void Switch(int offFrom, Action body)
        {
            int was = NightRows.ForceOffFrom;
            bool? force = Polish53.Force;
            try
            {
                NightRows.ForceOffFrom = offFrom;
                Polish53.Force = null;
                body();
            }
            finally
            {
                NightRows.ForceOffFrom = was;
                Polish53.Force = force;
            }
        }

        [Test]
        public void TheItems_HaveTheirSwitchAndNumbers()
        {
            GameConfig c = Cfg(false);
            const string p = "design.n53.polish53.numbers.";
            foreach (string k in Items)
            {
                Assert.That(c.Has(p + k + ".on"), Is.True, k + ".on");
                double v = c.Num(p + k + ".on");
                Assert.That(v == 0 || v == 1, Is.True, k + ".on is 0 or 1");
                Assert.That(c.Get(p + k + ".on_why") is string w && w.Length > 0, Is.True, k + ".on_why");
            }
            Assert.That(c.Num(p + "killWash.whiteFrames"), Is.GreaterThanOrEqualTo(1), "killWash.whiteFrames");
            Assert.That(c.Num(p + "scorchCore.glow"), Is.GreaterThan(0), "scorchCore.glow");
            Assert.That(c.Num(p + "icelineGrade.lift"), Is.GreaterThan(1), "icelineGrade.lift");
            Assert.That(c.Num(p + "killPop.punch"), Is.GreaterThan(1), "killPop.punch");
            foreach (string t in new[] { "punchS", "shardS", "heatS" }) Assert.That(c.Num(p + "killPop." + t), Is.GreaterThan(0), "killPop." + t);
        }

        [Test]
        public void TheRowOff_IsNight52()
        {
            Switch(NightRows.NoOff, () =>
            {
                GameConfig c = Cfg(false);
                Assert.That(Polish53.On(c), Is.False, "polish53 off");
                foreach (string k in Items) Assert.That(Polish53.Item(c, k), Is.False, k + " off");
                Assert.That(KillWash53.WhiteCap(c, UnityTuning.Fx49KillWhiteFrames, true, false), Is.EqualTo(UnityTuning.Fx49KillWhiteFrames), "K's cap kept");
                Assert.That(KillWash53.WhiteCap(c, -1, true, false), Is.EqualTo(-1), "no cap kept");
                Assert.That(Scorch53.Glow(c), Is.EqualTo(0f), "skin52's scorch rule");
                for (int s = 0; s < UnityTuning.Look52WallLift.Length; s++)
                {
                    Assert.That(IcelineGrade53.Lift(c, s), Is.EqualTo(0f), "no iceline lift, slot " + s);
                    Assert.That(Look52Lift.LiftFor(c, s), Is.EqualTo(UnityTuning.Look52WallLift[s]), "look52's lift, slot " + s);
                }
                var ids = new List<string>();
                IcelineGrade53.AddLevels(c, ids);
                Assert.That(ids, Is.Empty, "look52's grade list unchanged");
            });
        }

        [Test]
        public void TheRowOn_TakesEachItemsNumbers()
        {
            Switch(NightRows.NoOff, () =>
            {
                GameConfig c = Cfg(true);
                const string p = "design.n53.polish53.numbers.";
                Assert.That(Polish53.On(c), Is.True, "polish53 on");
                int wf = (int)c.Num(p + "killWash.whiteFrames");
                bool washOn = c.Num(p + "killWash.on") != 0;
                Assert.That(KillWash53.WhiteCap(c, UnityTuning.Fx49KillWhiteFrames, true, false), Is.EqualTo(washOn ? Math.Min(UnityTuning.Fx49KillWhiteFrames, wf) : UnityTuning.Fx49KillWhiteFrames), "a gun kill (FeelAB)");
                Assert.That(KillWash53.WhiteCap(c, -1, true, false), Is.EqualTo(washOn ? wf : -1), "a gun kill without K's cap");
                Assert.That(KillWash53.WhiteCap(c, -1, true, true), Is.EqualTo(-1), "a punch kill unchanged");
                Assert.That(KillWash53.WhiteCap(c, -1, false, false), Is.EqualTo(-1), "NIGHT17's chunks (FeelAB off) unchanged");
                bool scorchOn = c.Num(p + "scorchCore.on") != 0;
                Assert.That(Scorch53.Glow(c), Is.EqualTo(scorchOn ? (float)c.Num(p + "scorchCore.glow") : 0f), "the core's glow");
                bool iceOn = c.Num(p + "icelineGrade.on") != 0;
                float lift = (float)c.Num(p + "icelineGrade.lift");
                Assert.That(IcelineGrade53.Lift(c, UnityTuning.Polish53IcelineSlot), Is.EqualTo(iceOn ? lift : 0f), "iceline's lift");
                for (int s = 0; s < UnityTuning.Look52WallLift.Length; s++)
                    if (s != UnityTuning.Polish53IcelineSlot)
                    {
                        Assert.That(IcelineGrade53.Lift(c, s), Is.EqualTo(0f), "another slot: no polish53 lift");
                        Assert.That(Look52Lift.LiftFor(c, s), Is.EqualTo(UnityTuning.Look52WallLift[s]), "another slot: look52's lift");
                    }
                var ids = new List<string>();
                IcelineGrade53.AddLevels(c, ids);
                if (iceOn) foreach (string id in UnityTuning.Polish53IcelineLevels) Assert.That(ids, Does.Contain(id), "iceline's id " + id);
                else Assert.That(ids, Is.Empty);
            });
        }

        /// <summary>EXT53 B (T-111): killWash.ringInWash 0 leaves a kill burst's fx49 ring out of the wash box only with the item on; ringInWash 1,
        /// the item's on 0 or the row off keep the ring (U's rule).</summary>
        [Test]
        public void KillWashRingInWash_OnlyUnderTheItem()
        {
            Switch(NightRows.NoOff, () =>
            {
                GameConfig Wash(bool row, double on, double ring)
                {
                    GameConfig c = Cfg(row);
                    Dictionary<string, object> kw = c.Obj("design.n53.polish53.numbers.killWash");
                    kw["on"] = on;
                    kw["ringInWash"] = ring;
                    return c;
                }
                int s0 = KillWash53.RingsSkipped;
                Assert.That(KillWash53.RingInWash(Wash(true, 1, 0)), Is.False, "the item on, ringInWash 0: no ring in the box");
                Assert.That(KillWash53.RingsSkipped, Is.EqualTo(s0 + 1), "the probe counts it");
                Assert.That(KillWash53.RingInWash(Wash(true, 1, 1)), Is.True, "ringInWash 1: the ring as before");
                Assert.That(KillWash53.RingInWash(Wash(true, 0, 0)), Is.True, "the item's on 0: the ring as before");
                Assert.That(KillWash53.RingInWash(Wash(false, 1, 0)), Is.True, "polish53 off: the ring as before");
            });
        }

        [Test]
        public void TheOffSwitches_TurnEveryItemOff()
        {
            foreach (int from in new[] { 53, 52 })
                Switch(from, () =>
                {
                    GameConfig c = Cfg(true);
                    Assert.That(Polish53.On(c), Is.False, "--off" + from);
                    foreach (string k in Items) Assert.That(Polish53.Item(c, k), Is.False, "--off" + from + ": " + k);
                    Assert.That(KillWash53.WhiteCap(c, 4, true, false), Is.EqualTo(4), "--off" + from + ": K's cap");
                    Assert.That(Scorch53.Glow(c), Is.EqualTo(0f), "--off" + from + ": no glow");
                });
        }
    }
}
