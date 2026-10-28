// NIGHT71 builder F (LEFTOVER L6, design.n71.glow71; Game/Glow71.cs). Pinned:
//   1. the switch: off when the row is off, under -skygrindOff71 (or older), -skygrindNoGlow71 (any case), ForceOffFrom 71, the same-run
//      toggle's held frame; on otherwise; off when a number is missing;
//   2. the repo keeps it OFF, with its five numbers in range;
//   3. the steps run from the body's back toward the muzzle, segment 0 the farthest back, over `span` of the length, each slot's gap dark.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;
using G = Skygrind.Game.Glow71;

namespace Skygrind.Game.Tests
{
    public class N71GlowTests
    {
        static string Repo()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
            catch (Exception) { return Path.GetFullPath("."); }
        }

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Repo(), "PortExport", "config.json"));

        static Dictionary<string, object> RowOf(GameConfig c) =>
            (Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)c.root["design"])["n71"])[G.Row];

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
                NightRows.ReelOverride.Remove(G.Row);
                GameConfig on = WithRow(true), off = WithRow(false);
                Assert.That(G.OffReason(off, new string[0]), Is.Not.Null, "the row off");
                Assert.That(G.OffReason(on, new string[0]), Is.Null, "the row on, no switch: on");
                Assert.That(G.OffReason(on, new[] { "-skygrindNoGlow71" }), Is.Not.Null, "this row alone off");
                Assert.That(G.OffReason(on, new[] { "-skygrindnoglow71" }), Is.Not.Null, "any case");
                Assert.That(G.OffReason(on, new[] { "-skygrindOff50" }), Is.Not.Null, "an older night's switch");
                Assert.That(G.OffReason(on, new[] { "-skygrindOff72" }), Is.Null, "a later night's switch leaves it on");
                NightRows.ForceOffFrom = 71;
                Assert.That(G.OffReason(on, new string[0]), Is.Not.Null, "ForceOffFrom 71");
                NightRows.ForceOffFrom = NightRows.NoOff;
                NightRows.ReelOverride[G.Row] = false;
                Assert.That(G.OffReason(on, new string[0]), Is.Not.Null, "the same-run toggle's held frame");
                NightRows.ReelOverride.Remove(G.Row);
                GameConfig bad = WithRow(true);
                ((Dictionary<string, object>)RowOf(bad)["numbers"]).Remove("lift");
                Assert.That(G.OffReason(bad, new string[0]), Does.Contain("numbers incomplete"));
            }
            finally
            {
                NightRows.ReelOverride.Remove(G.Row);
                NightRows.ForceOffFrom = was;
            }
            Assert.That(SameRun63Math.KindOf(G.Row), Is.EqualTo(SameRun63Math.Kind.Frame), "D1 claims the row (the shared commit's FrameRows)");
        }

        [Test]
        public void Repo_KeepsTheRowOff_WithItsNumbers()
        {
            GameConfig c = Cfg();
            Assert.That(RowOf(c)["enabled"], Is.EqualTo(false), "OFF in the repo until GG's keep");
            G.Numbers n = G.Of(c);
            Assert.That(n, Is.Not.Null, "five numbers in design.n71.glow71.numbers");
            Assert.That(n.segW, Is.GreaterThan(0f).And.LessThan(0.086f), "inside the forearm's width");
            Assert.That(n.segH, Is.GreaterThan(0f));
            Assert.That(n.lift, Is.GreaterThanOrEqualTo(0f));
            Assert.That(n.span, Is.GreaterThan(0f).And.LessThanOrEqualTo(1f));
            Assert.That(n.gapShare, Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
        }

        [Test]
        public void StepZs_BackToFront_TheFirstQuarterAtTheBack()
        {
            // the gauntlet: its back cap at +0.151, the knuckles toward -0.13 (the muzzle at -0.12)
            float[] z = G.StepZs(0.151f, -0.13f, 0.8f, 0.3f, 4, out float len);
            Assert.That(z.Length, Is.EqualTo(4));
            for (int i = 1; i < 4; i++) Assert.That(z[i], Is.LessThan(z[i - 1]), "each step nearer the knuckles than the one before");
            Assert.That(z[0], Is.LessThan(0.151f).And.GreaterThan(0.1f), "the first quarter near the back cap");
            float slot = (0.151f + 0.13f) * 0.8f / 4;
            Assert.That(len, Is.EqualTo(slot * 0.7f).Within(1e-5f), "each step its slot less the gap");
            Assert.That(z[3], Is.GreaterThan(0.151f - (0.151f + 0.13f) * 0.8f), "inside the span");
        }
    }
}
