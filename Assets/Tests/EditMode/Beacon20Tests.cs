// NIGHT20 A3 (builder P; Research/2026-10-06-additions-backlog.md row 16: RoR2's Shrine of the Mountain, DRG's Machine
// Events): THE BEACON (design.adds.beacon). In some breaks a beacon stands at a seeded spot; holding BUY at it makes the
// next round harder (an elite and an escort squad more) for a reward (the elite's kill: H's reveal); ignored, nothing
// changes. Core/Beacon20.cs is the rule: which breaks it stands in (from `from`, before a normal or gold round only, by a
// chance on the run's own stream), and where (a seeded pick of the spots offered).
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Beacon20Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root(), "config.json"));

        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        [Test]
        public void TheBlock_IsInTheDesignConfig_WithItsNumbers()
        {
            Assert.That(Cfg.Has("design.adds.beacon._why"), Is.True);
            Beacon20.P p = Beacon20.P.From(Cfg);
            Assert.That(p.enabled, Is.True);
            Assert.That(p.from, Is.GreaterThanOrEqualTo(2));
            Assert.That(p.chance, Is.GreaterThan(0).And.LessThan(1));
            Assert.That(p.elites, Is.GreaterThanOrEqualTo(1));
            Assert.That(p.escortSquads, Is.GreaterThanOrEqualTo(0));
            Assert.That(Cfg.Has($"enemies.{p.kind}") || p.kind == "charger" || p.kind == "husk", Is.True, "a web kind (rule 7)");
        }

        [Test]
        public void ItStands_OnlyFromItsRound_BeforeNormalOrGoldRounds_AboutChanceOfThem()
        {
            Beacon20.P p = Beacon20.P.From(Cfg);
            DirectorMath.D d = DirectorMath.From(Cfg);
            int stood = 0, eligible = 0;
            for (uint seed = 1; seed <= 200; seed++)
            {
                for (int r = 1; r <= 30; r++)
                {
                    bool s = Beacon20.Stands(seed, r, p, d);
                    string type = DirectorMath.RoundType(r, d);
                    if (r < p.from || (type != "normal" && type != "gold"))
                    {
                        Assert.That(s, Is.False, $"seed {seed} round {r} ({type})");
                        continue;
                    }
                    eligible++;
                    if (s) stood++;
                }
            }
            double share = (double)stood / eligible;
            Assert.That(share, Is.EqualTo(p.chance).Within(0.05), $"{stood} of {eligible} eligible breaks");
            Assert.That(Beacon20.Stands(7, 9, p, d), Is.EqualTo(Beacon20.Stands(7, 9, p, d)), "a pure function of the seed and the round");
        }

        [Test]
        public void TheSpot_IsASeededPickOfTheOffered()
        {
            for (uint seed = 1; seed <= 50; seed++)
            {
                int a = Beacon20.SpotIndex(seed, 9, 5, Beacon20.P.From(Cfg));
                Assert.That(a, Is.InRange(0, 4));
                Assert.That(Beacon20.SpotIndex(seed, 9, 5, Beacon20.P.From(Cfg)), Is.EqualTo(a), "the same each time");
            }
            Assert.That(Beacon20.SpotIndex(3, 9, 0, Beacon20.P.From(Cfg)), Is.EqualTo(-1), "no spot offered: no beacon");
        }

        [Test]
        public void Off_ItNeverStands()
        {
            Beacon20.P p = Beacon20.P.From(Cfg);
            p.enabled = false;
            DirectorMath.D d = DirectorMath.From(Cfg);
            for (int r = 1; r <= 30; r++) Assert.That(Beacon20.Stands(5, r, p, d), Is.False);
        }
    }
}
