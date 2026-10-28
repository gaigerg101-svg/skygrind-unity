// NIGHT20 (the lead owns this file): THE CLIMB (Game/Bot/SandboxClimb.cs) and THE MARATHON (SandboxMarathon.cs).
//   1. THE CLIMB is deterministic: two measures of the Survival map print the same rows and the same perches
//   2. its horde time counts a husk only where it can hit the player (never the field's seed ring under a perch): a 24 m
//      spire top the web's grid gives no route to stays unreachable (NIGHT19's grid: 0 nodes above 5 m reached)
//   3. THE MARATHON's run is deterministic for a seed (its JSON line)
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class SandboxClimbTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static string Line(SandboxClimb.Report r)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"{r.nodes.Count} {r.unreachable} {r.worstHorde} {r.floorRuleFails} {r.downFails} {r.huskNodes} {r.worstDown}");
            foreach (SandboxClimb.PlayerNode p in r.perches) sb.Append(" | ").Append(p.Name).Append(' ').Append(p.horde);
            return sb.ToString();
        }

        [Test]
        public void TheClimb_TwoMeasures_AreIdentical()
        {
            bool was = N20Blocks.ForceOff;
            try
            {
                N20Blocks.ForceOff = true; // NIGHT19's grid (the routes off)
                string a = Line(SandboxClimb.Measure(Cfg(), Map()));
                string b = Line(SandboxClimb.Measure(Cfg(), Map()));
                Assert.That(b, Is.EqualTo(a));
            }
            finally { N20Blocks.ForceOff = was; }
        }

        [Test]
        public void TheHordeTime_NeverCountsTheSeedRingUnderAPerch()
        {
            bool was = N20Blocks.ForceOff;
            try
            {
                N20Blocks.ForceOff = true;
                SandboxClimb.Report r = SandboxClimb.Measure(Cfg(), Map());
                Assert.That(r.perches.Count, Is.EqualTo(SandboxClimb.Perches));
                foreach (SandboxClimb.PlayerNode p in r.nodes)
                    if (p.y > 5) Assert.That(double.IsPositiveInfinity(p.horde), Is.True, p.Name + " (the web's grid reaches nothing above 5 m)");
                Assert.That(r.unreachable, Is.GreaterThan(0));
            }
            finally { N20Blocks.ForceOff = was; }
        }

        [Test]
        public void TheMarathon_ARun_IsDeterministic()
        {
            bool was = N20Blocks.ForceOff;
            try
            {
                N20Blocks.ForceOff = true;
                string a = SandboxMarathon.RunOne(Cfg(), Map(), 3, 120).ToJson();
                string b = SandboxMarathon.RunOne(Cfg(), Map(), 3, 120).ToJson();
                Assert.That(b, Is.EqualTo(a));
            }
            finally { N20Blocks.ForceOff = was; }
        }
    }
}
