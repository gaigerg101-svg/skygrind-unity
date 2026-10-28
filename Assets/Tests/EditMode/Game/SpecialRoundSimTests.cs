// NIGHT20 U3-34.9 A2 + A4 (builder E owns this file; tests first): THE HUNT and THE ELITE in the sim (Core/Enemies/
// SpecialRounds.cs, wired in Game/SurvivalSim.Director20.cs), behind design.adds.hunt / design.adds.elite:
//   1. THE HUNT (the web's swarm round, 8): `specialRound` hunt "announce" with countTime s left in the break before it,
//      "start" at its roundBegin with the round's count, its squads telegraphed in a ring around you (ringMin .. ringMax,
//      never inside ringMin), "last" once when one enemy is left, "end" at its roundClear; the rows off: no event, the
//      web's out-of-view spawns
//   2. THE ELITE (its first scheduled round): mid-round (half the round's squads out) "announce" and an elite telegraph;
//      it lands as a CHARGER with two affixes ("start" with its id); its kill drops a power-up from the run's stream and
//      says "end"
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class SpecialRoundSimTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap map;
        static SurvivalMap Map() => map ??= SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        sealed class Said
        {
            public readonly List<string> lines = new List<string>();
            public readonly List<SpecialRoundEvent> events = new List<SpecialRoundEvent>();
        }

        static SurvivalSim Run(bool adds, Said said)
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg, Map());
            sim.noRecords = sim.noLab = true;
            sim.n20Override = new N20Blocks { director = true, adds = adds };
            sim.StartRun(1, null, null);
            sim.zones.OpenAll();
            sim.health.max = sim.health.hp = 1e9; // god mode: the test watches the rounds, not the fight
            sim.events.On<SpecialRoundEvent>(N20Events.SpecialRound, e =>
            {
                said.lines.Add(e.name + " " + e.phase + " " + e.round);
                said.events.Add(new SpecialRoundEvent { name = e.name, round = e.round, phase = e.phase, count = e.count });
            });
            return sim;
        }

        static double Flat(Vec3 a, Vec3 b) => System.Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z));

        [Test]
        public void TheHunt_Announces_RingSpawns_LastOne_End()
        {
            double ringMin = Cfg.Num("design.adds.hunt.ringMin"), ringMax = Cfg.Num("design.adds.hunt.ringMax");
            double countTime = Cfg.Num("design.adds.hunt.countTime");
            foreach (bool on in new[] { false, true })
            {
                var said = new Said();
                SurvivalSim sim = Run(on, said);
                sim.run.DebugSkipTo(8);
                int guard = 0;
                double announcedAt = -1;
                while (sim.run.state == SurvivalRun.State.Break && guard++ < 5000)
                {
                    sim.Step();
                    if (announcedAt < 0 && said.lines.Contains("hunt announce 8")) announcedAt = sim.run.BreakLeft;
                }
                Assert.That(sim.run.round, Is.EqualTo(8));
                if (!on)
                {
                    for (int i = 0; i < 600; i++) sim.Step();
                    Assert.That(said.lines, Is.Empty, "the rows off: no special round");
                    continue;
                }
                Assert.That(announcedAt, Is.GreaterThanOrEqualTo(0), "announced in the break");
                Assert.That(announcedAt, Is.LessThanOrEqualTo(countTime + 1e-9), "with countTime s left (the count)");
                Assert.That(said.lines, Does.Contain("hunt start 8"));
                SpecialRoundEvent start = said.events.Find(e => e.phase == "start");
                Assert.That(start.count, Is.GreaterThan(0), "the round's count");
                // the ring: every squad telegraphed in the next 3 s
                var checkedPoints = new HashSet<Telegraph>();
                for (int i = 0; i < 360; i++)
                {
                    sim.Step();
                    foreach (Telegraph t in sim.director.telegraphs)
                    {
                        if (!checkedPoints.Add(t)) continue;
                        double d = Flat(t.point, sim.mover.position);
                        Assert.That(d, Is.GreaterThanOrEqualTo(ringMin - 1).And.LessThanOrEqualTo(ringMax + 1), "a hunt squad in the ring");
                        Assert.That(d, Is.GreaterThanOrEqualTo(ringMin - 0.75), "never inside the min distance (the mover drifts < 0.75 m)");
                    }
                }
                Assert.That(checkedPoints.Count, Is.GreaterThan(0), "squads came");
                // LAST ONE: the field cleared to one enemy
                sim.director.Clear();
                Husk last = sim.husks.Spawn("swarmer", new Vec3(0, 0, -30), sim.mover.position);
                Assert.That(last, Is.Not.Null);
                for (int i = 0; i < 30; i++) sim.Step();
                Assert.That(said.lines.FindAll(s => s == "hunt last 8").Count, Is.EqualTo(1), "LAST ONE, once");
                last.alive = false;
                for (int i = 0; i < 30; i++) sim.Step();
                Assert.That(said.lines, Does.Contain("hunt end 8"));
            }
        }

        /// <summary>Run to round `round` with `twist` forced for it (SpecialRounds.ForceTwist, the test's hook).</summary>
        static SurvivalSim TwistRound(string twist, int round, Said said)
        {
            SurvivalSim sim = Run(true, said);
            sim.specials.ForceTwist(round, twist);
            sim.run.DebugSkipTo(round);
            int guard = 0;
            while (sim.run.state == SurvivalRun.State.Break && guard++ < 5000) sim.Step();
            return sim;
        }

        [Test]
        public void ATwist_IsNamedInTheBreak_StartsAndEnds_AndLethalAndGoldenScaleTheirSystems()
        {
            var said = new Said();
            SurvivalSim sim = Run(true, said);
            double taken = sim.health.takenMult, points = sim.economy.boost, drops = sim.powerUps.dropMult();
            sim.specials.ForceTwist(9, "lethal");
            sim.run.DebugSkipTo(9);
            int guard = 0;
            while (sim.run.state == SurvivalRun.State.Break && guard++ < 5000) sim.Step();
            Assert.That(said.lines, Does.Contain("twist:lethal announce 9"), "named in the break");
            Assert.That(said.lines, Does.Contain("twist:lethal start 9"));
            Assert.That(sim.health.takenMult, Is.EqualTo(taken * Cfg.Num("design.adds.twists.lethalMult")), "LETHAL: enemy damage x lethalMult");
            Assert.That(sim.economy.boost, Is.EqualTo(points * Cfg.Num("design.adds.twists.lethalMult")).Within(1e-9), "LETHAL: points x lethalMult");
            sim.director.Clear();
            for (int i = 0; i < 30; i++) sim.Step(); // the field clear: the round clears
            Assert.That(said.lines, Does.Contain("twist:lethal end 9"));
            Assert.That(sim.health.takenMult, Is.EqualTo(taken), "back after the round");
            Assert.That(sim.economy.boost, Is.EqualTo(points).Within(1e-9));

            var said2 = new Said();
            SurvivalSim golden = TwistRound("golden", 9, said2);
            Assert.That(golden.powerUps.dropMult(), Is.EqualTo(drops * Cfg.Num("design.adds.twists.goldenMult")), "GOLDEN: drops x goldenMult");
            golden.director.Clear();
            for (int i = 0; i < 30; i++) golden.Step();
            Assert.That(golden.powerUps.dropMult(), Is.EqualTo(drops), "back after the round");
        }

        [Test]
        public void Swarmageddon_TricklesSwarmers_WhileTheRoundsSquadsRun()
        {
            var said = new Said();
            SurvivalSim sim = TwistRound("swarmageddon", 9, said);
            Assert.That(said.lines, Does.Contain("twist:swarmageddon start 9"));
            int queued = 0;
            foreach (List<string> s in sim.director.Queue) if (s.TrueForAll(k => k == "swarmer")) queued++;
            int guard = 0;
            int more = queued;
            while (guard++ < MovementMath.StepsFor(Cfg.Num("design.adds.twists.trickleEvery") + 0.1, Cfg.Dt))
            {
                sim.Step();
                int n = 0;
                foreach (List<string> s in sim.director.Queue) if (s.TrueForAll(k => k == "swarmer")) n++;
                more = System.Math.Max(more, n);
            }
            Assert.That(sim.specials.Trickled, Is.GreaterThan(0), "a swarmer squad added within trickleEvery s");
        }

        [Test]
        public void NoTwists_OnAHeatRun()
        {
            var said = new Said();
            SurvivalSim sim = Run(true, said);
            sim.run.stats.heat = 1;
            sim.specials.ForceTwist(9, "fog");
            sim.run.DebugSkipTo(9);
            for (int i = 0; i < 1200; i++) sim.Step();
            Assert.That(said.lines.FindAll(s => s.StartsWith("twist:")), Is.Empty, "a HEAT rung brings its own mutators: no twist on top");
        }

        [Test]
        public void TheElite_MidRound_AChargerWithTwoAffixes_ItsKillDrops()
        {
            int round = SpecialRoundMath.EliteRounds(1u ^ (uint)Cfg.Num("design.adds.elite.salt"), 40, SpecialRoundMath.EliteP.From(Cfg), DirectorMath.From(Cfg))[0];
            var said = new Said();
            SurvivalSim sim = Run(true, said);
            Assert.That(sim.specials.Schedule[0], Is.EqualTo(round), "the run's schedule from its seed");
            sim.run.DebugSkipTo(round);
            int guard = 0;
            while (!said.lines.Contains("elite announce " + round) && guard++ < 20000) sim.Step();
            Assert.That(said.lines, Does.Contain("elite announce " + round), "mid-round: ELITE INBOUND");
            Assert.That(sim.director.squadsSpawned * 2, Is.GreaterThanOrEqualTo(sim.specials.PlannedSquads), "half the round's squads out");
            bool flagged = false;
            foreach (Telegraph t in sim.director.telegraphs) flagged |= t.elite;
            Assert.That(flagged, Is.True, "its telegraph");
            guard = 0;
            while (sim.specials.EliteId < 0 && guard++ < 600) sim.Step();
            Assert.That(said.lines, Does.Contain("elite start " + round));
            Husk elite = null;
            foreach (Husk h in sim.husks.PoolOf(SpecialRoundMath.EliteP.From(Cfg).kind)) if (h.alive && h.id == sim.specials.EliteId) elite = h;
            Assert.That(elite, Is.InstanceOf<Charger>(), "a CHARGER");
            Assert.That(sim.specials.EliteAffixes.Count, Is.EqualTo(2), "two affixes");
            int drops = sim.powerUps.spawned;
            elite.alive = false;
            sim.events.Emit("kill", new KillEvent { owner = "player", weapon = "revolver", kind = elite.kind, enemy = elite.id, position = elite.position });
            Assert.That(sim.powerUps.spawned, Is.EqualTo(drops + 1), "a guaranteed drop");
            Assert.That(said.lines, Does.Contain("elite end " + round));
        }
    }
}
