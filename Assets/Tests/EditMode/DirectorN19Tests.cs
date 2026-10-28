// NIGHT19 U3-33.7 (builder E): the director's new kinds behind design.enemies.n19 (Director.Spawnable). Off: NIGHT18's
// three kinds (Director.Available; a charger or drone queued by a SURGE turns into a husk). On: the ported kinds join
// the planner at the web's rounds (director.rounds.introduce: the CHARGER from round 7, the DRONE from round 4) and
// keep their kind through AddSquads and the hunter squad; rounds before them plan exactly as NIGHT18's (the smoke's
// round 1 and every round before the DRONE's).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Skygrind.Core.Tests
{
    public class DirectorN19Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")), "config.json"));

        static readonly N19Blocks On = new N19Blocks { enemies = true };

        static bool Has(List<List<string>> plan, string kind)
        {
            foreach (List<string> s in plan) if (s.Contains(kind)) return true;
            return false;
        }

        [Test]
        public void Spawnable_IsNight18sThreeKinds_WithTheBlockOff()
        {
            foreach (string k in new[] { "husk", "swarmer", "spitter" }) Assert.That(Director.Spawnable(k, N19Blocks.Off), Is.True, k);
            foreach (string k in new[] { "charger", "drone", "lancer", "warden" }) Assert.That(Director.Spawnable(k, N19Blocks.Off), Is.False, k);
        }

        [Test]
        public void Spawnable_AddsTheCharger_WithTheBlockOn()
        {
            Assert.That(Director.Spawnable("charger", On), Is.True);
            Assert.That(Director.Spawnable("lancer", On), Is.False, "the LANCER is beyond tonight's ports");
            Assert.That(Director.Spawnable("warden", On), Is.False);
        }

        [Test]
        public void ThePlanner_DealsChargers_FromTheWebsRound_OnlyWithTheBlockOn()
        {
            DirectorMath.D d = DirectorMath.From(Cfg);
            int from = (int)Cfg.Num("director.rounds.introduce.charger");
            bool seenOn = false;
            for (uint seed = 1; seed <= 40; seed++)
            {
                Assert.That(Has(DirectorMath.PlanSquads(from - 1, new Mulberry32(seed).Next, d, k => Director.Spawnable(k, On)), "charger"), Is.False, $"round {from - 1}, seed {seed}");
                Assert.That(Has(DirectorMath.PlanSquads(from, new Mulberry32(seed).Next, d, k => Director.Spawnable(k, N19Blocks.Off)), "charger"), Is.False, "off");
                if (Has(DirectorMath.PlanSquads(from, new Mulberry32(seed).Next, d, k => Director.Spawnable(k, On)), "charger")) seenOn = true; // round 7 (round 8 is a swarm round: swarmers only)
            }
            Assert.That(seenOn, Is.True, "a charger squad within 40 seeds of round " + from);
        }

        [Test]
        public void RoundsBeforeTheNewKinds_PlanAsNight18()
        {
            DirectorMath.D d = DirectorMath.From(Cfg);
            int first = (int)System.Math.Min(Cfg.Num("director.rounds.introduce.drone"), Cfg.Num("director.rounds.introduce.charger"));
            for (int round = 1; round < first; round++)
            {
                for (uint seed = 1; seed <= 10; seed++)
                {
                    var off = DirectorMath.PlanSquads(round, new Mulberry32(seed).Next, d, Director.Available);
                    var on = DirectorMath.PlanSquads(round, new Mulberry32(seed).Next, d, k => Director.Spawnable(k, On));
                    Assert.That(on.Count, Is.EqualTo(off.Count), $"round {round} seed {seed}");
                    for (int i = 0; i < on.Count; i++) Assert.That(on[i], Is.EqualTo(off[i]), $"round {round} seed {seed} squad {i}");
                }
            }
        }

        [Test]
        public void SquadsAKindHasNoPortFor_BecomeHusks()
        {
            var squad = new List<string> { "charger", "lancer", "warden", "swarmer" };
            Assert.That(Director.PortedSquad(squad, N19Blocks.Off), Is.EqualTo(new List<string> { "husk", "husk", "husk", "swarmer" }));
            Assert.That(Director.PortedSquad(squad, On), Is.EqualTo(new List<string> { "charger", "husk", "husk", "swarmer" }));
        }
    }
}
