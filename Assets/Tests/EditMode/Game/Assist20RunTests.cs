// NIGHT20 E2 (builder P): ASSIST on a run (Game/Assist20Run.cs; the rule is Core/Assist20.cs). With SETTINGS > ACCESS >
// ASSIST on, the block on (design.adds + design.adds.assist) and a human's run: the run takes the damage x (1 -
// resistance) (through Health.takenMult, so HEAT's GLASS CANNON composes), is stamped (no records: sim.noRecords, RESULTS
// shows "ASSIST n%"), and each of the player's downs while it's on adds one to the saved count (the next run resists more).
// Off, or a bot's run: nothing changes, and noRecords is back as it was.
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Assist20RunTests
    {
        static string PortRoot()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        static GameConfig cfg;
        static SurvivalMap map;
        static GameConfig Cfg() => cfg ??= GameConfig.Load(Path.Combine(PortRoot(), "config.json"));
        static SurvivalMap Map() => map ??= SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(PortRoot(), "maps", "survival.json"))));

        int saved;
        bool setting = true;

        (SurvivalSim, Assist20Run) Make()
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            sim.noLab = true;
            sim.setup.mode = RunSetup.Survival;
            var a = new Assist20Run(sim, () => setting, () => saved, n => saved = n) { Force = true };
            return (sim, a);
        }

        [SetUp]
        public void Fresh()
        {
            saved = 0;
            setting = true;
        }

        [Test]
        public void On_TheRunTakes80PercentDamage_IsStamped_AndRecordsNothing()
        {
            (SurvivalSim sim, Assist20Run a) = Make();
            sim.StartRun(1, null, null);
            Assert.That(a.Active, Is.True);
            Assert.That(a.Resistance, Is.EqualTo(0.2).Within(1e-12));
            Assert.That(sim.health.takenMult, Is.EqualTo(0.8).Within(1e-12));
            double hp = sim.health.hp;
            sim.health.Damage(10, "husk");
            Assert.That(hp - sim.health.hp, Is.EqualTo(8).Within(1e-9), "10 damage lands as 8");
            Assert.That(sim.noRecords, Is.True, "stamped: no personal best, no contracts (as the LAB)");
            Assert.That(a.Stamp, Is.EqualTo("ASSIST 20%"));
            sim.EndRun();
            Assert.That(sim.lastStats.recorded, Is.False);
        }

        [Test]
        public void ADown_AddsOne_TheNextRunResistsMore()
        {
            (SurvivalSim sim, Assist20Run a) = Make();
            sim.StartRun(1, null, null);
            sim.health.Damage(1e6, "husk");
            Assert.That(saved, Is.EqualTo(1), "the down is saved");
            sim.StartRun(2, null, null);
            Assert.That(a.Resistance, Is.EqualTo(0.22).Within(1e-12));
            Assert.That(sim.health.takenMult, Is.EqualTo(0.78).Within(1e-12));
            Assert.That(a.Stamp, Is.EqualTo("ASSIST 22%"));
        }

        [Test]
        public void Off_NothingChanges_AndRecordsComeBack()
        {
            (SurvivalSim sim, Assist20Run a) = Make();
            sim.StartRun(1, null, null);
            Assert.That(sim.noRecords, Is.True);
            setting = false;
            sim.StartRun(2, null, null);
            Assert.That(a.Active, Is.False);
            Assert.That(sim.health.takenMult, Is.EqualTo(1));
            Assert.That(sim.noRecords, Is.False, "back as it was before ASSIST");
            Assert.That(a.Stamp, Is.Null);
            sim.health.Damage(1e6, "husk");
            Assert.That(saved, Is.EqualTo(0), "a down with ASSIST off counts nothing");
        }

        [Test]
        public void NeverInABotsRun_AndTheBlockDecides()
        {
            (SurvivalSim sim, Assist20Run a) = Make();
            sim.metaSilent = true; // the AUTOPILOT's runs
            sim.StartRun(1, null, null);
            Assert.That(a.Active, Is.False);
            Assert.That(sim.health.takenMult, Is.EqualTo(1));
            SurvivalSim s2 = SurvivalSim.Headless(Cfg(), Map());
            s2.noLab = true;
            var b = new Assist20Run(s2, () => true, () => 0, _ => { });
            s2.StartRun(1, null, null);
            Assert.That(b.Active, Is.EqualTo(s2.n20.Add(Cfg(), "assist")), "the run's adds bit and the row's switch");
        }
    }
}
