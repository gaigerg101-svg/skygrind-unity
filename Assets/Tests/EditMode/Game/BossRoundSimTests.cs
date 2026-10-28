// NIGHT20 U3-34.9 A1 (builder EN owns this file): the BULWARK's round in the sim (Game/SurvivalSim.Boss20.cs), behind
// design.adds.bulwark (with the adds bit), on the real Survival map, a god-mode player:
//   1. the row on: round 5 (bossKit.schedule) is announced in its break (`specialRound` boss announce), opens the Grinder,
//      raises the pen (the edge cells to core.maxHeight, the gaps open), plans no squads, starts the BULWARK (`bossIntro`,
//      `specialRound` boss start) and lasts while it lives; its kill (through its intro, the beat and the finisher) pays
//      bossKit.rewards.bulwark (the points, a free perk token), says "BULWARK DOWN", flattens the Grinder and says
//      `specialRound` boss end
//   2. the row off: round 5 is the director's ordinary round (its squads, no boss)
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class BossRoundSimTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static SurvivalMap map;
        static SurvivalMap Map() => map ??= SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        /// <summary>A fresh config with design.adds.bulwark.enabled as asked (set before any read: the config caches paths).</summary>
        static GameConfig Cfg(bool bulwark)
        {
            GameConfig c = GameConfig.Load(Path.Combine(Root, "config.json"));
            PortData.Obj(PortData.Obj(PortData.Obj(c.Get("design"))["adds"])["bulwark"])["enabled"] = bulwark;
            return c;
        }

        static SurvivalSim Run(GameConfig cfg, List<string> said)
        {
            SurvivalSim sim = SurvivalSim.Headless(cfg, Map());
            sim.noRecords = sim.noLab = true;
            sim.n20Override = new N20Blocks { director = true, adds = true };
            sim.StartRun(1, null, null);
            sim.health.max = sim.health.hp = 1e9; // god mode: the test watches the round, not the fight
            sim.events.On<SpecialRoundEvent>(N20Events.SpecialRound, e => said.Add(e.name + " " + e.phase + " " + e.round));
            sim.events.On<BannerEvent>("banner", e => said.Add("banner " + e.text));
            sim.events.On<BossEvent>("bossIntro", e => said.Add("bossIntro " + e.name));
            return sim;
        }

        static void ToRound(SurvivalSim sim, int n)
        {
            sim.run.DebugSkipTo(n);
            int guard = 0;
            while (sim.run.state == SurvivalRun.State.Break && guard++ < 5000) sim.Step();
            Assert.That(sim.run.round, Is.EqualTo(n));
        }

        [Test]
        public void TheRowOn_RoundFive_IsTheBulwarksRound()
        {
            GameConfig cfg = Cfg(true);
            var said = new List<string>();
            SurvivalSim sim = Run(cfg, said);
            Assert.That(sim.BossOn, Is.True);
            ToRound(sim, 5);
            Assert.That(said, Does.Contain("boss announce 5"), "announced in its break");
            Assert.That(said, Does.Contain("boss start 5"));
            Assert.That(said, Does.Contain("bossIntro BULWARK"));
            Assert.That(sim.bossKit.Active, Is.EqualTo("bulwark"));
            Assert.That(sim.bulwark.Alive, Is.True);
            Assert.That(sim.zones.IsOpen("grinder"), Is.True, "the Grinder opens for a boss round");
            Assert.That(sim.director.Queue.Count, Is.EqualTo(0), "no regular squads");
            for (int i = 0; i < MovementMath.StepsFor(cfg.Num("boss.roundReshape") + 0.1, cfg.Dt); i++) sim.Step();
            CoreGrid core = sim.core;
            int n = core.spec.cells;
            Assert.That(core.Height(0), Is.EqualTo(cfg.movement.core.maxHeight).Within(1e-3), "the pen's corner wall is up");
            Assert.That(core.Height(n / 2), Is.EqualTo(0).Within(1e-3), "its north gap is open");
            Assert.That(core.Height(n / 2 * n + n / 2), Is.EqualTo(0).Within(1e-3), "the floor inside is flat");
            for (int i = 0; i < 600; i++) sim.Step();
            Assert.That(sim.run.state, Is.EqualTo(SurvivalRun.State.Fight), "the round lasts while the BULWARK lives");
            Assert.That(sim.director.Done, Is.False);

            double points = sim.economy.total;
            int tokens = sim.perks.tokens;
            int guard = 0;
            while (sim.bulwark.Alive && guard++ < 6000)
            {
                if (sim.bulwark.phase == "p1" || sim.bulwark.phase == "p2") sim.bulwark.Hit(1e6, "revolver", 0, "torso", out _);
                sim.Step();
            }
            Assert.That(sim.bulwark.Alive, Is.False, "killed through its phases");
            Assert.That(sim.economy.total - points, Is.GreaterThanOrEqualTo(cfg.Num("bossKit.rewards.bulwark.points")), "the reward's points");
            Assert.That(sim.perks.tokens, Is.EqualTo(tokens + 1), "a free perk token");
            Assert.That(said, Does.Contain("banner BULWARK DOWN"));
            Assert.That(said, Does.Contain("boss end 5"));
            for (int i = 0; i < MovementMath.StepsFor(cfg.Num("boss.roundReshape") + 0.1, cfg.Dt); i++) sim.Step();
            Assert.That(core.Height(0), Is.EqualTo(0).Within(1e-3), "the Grinder flat again");
        }

        [Test]
        public void TheRowOff_RoundFive_IsTheDirectorsRound()
        {
            GameConfig cfg = Cfg(false);
            var said = new List<string>();
            SurvivalSim sim = Run(cfg, said);
            Assert.That(sim.BossOn, Is.False);
            ToRound(sim, 5);
            Assert.That(said.Exists(s => s.StartsWith("boss ")), Is.False, "no boss round");
            Assert.That(sim.bossKit.Alive, Is.False);
            Assert.That(sim.director.Remaining, Is.GreaterThan(0), "the planner's squads");
        }
    }
}
