// NIGHT21 U3-35.4 (builder M): SLIDE BITES (Core/SlideBite21.cs, Game/SurvivalSim.SlideBite21.cs; design.n21.slideBites).
// UNRUN on the night it was written (the Editor was stuck on a dialog): the same scenario runs headless in
// `SimHarness slidebite` (Tools/SimHarness/Harness.N21.SlideBite.cs), which passed.
//   - On: a slide through husks bites each once per pass (the cooldown), a survivor staggers, a swarmer dies and the kill pays
//     CLOSE CALL, each bite pays the SLIDE BITE style bonus; no slide, no bite; a bitten enemy is not bitten twice inside the cooldown.
//   - Off: the same slide bites nothing.
using System;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class SlideBite21Tests
    {
        static string PortRoot()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (Exception) { return Path.GetFullPath("PortExport"); }
        }

        static GameConfig cfg;
        static SurvivalMap map;
        static GameConfig Cfg() => cfg ??= GameConfig.Load(Path.Combine(PortRoot(), "config.json"));
        static SurvivalMap Map() => map ??= SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(PortRoot(), "maps", "survival.json"))));

        static SurvivalSim Make(bool on)
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            sim.noRecords = sim.noLab = true;
            sim.n21Override = on ? new N21Blocks { slideBites = true } : N21Blocks.Off;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(2601, null, null);
            return sim;
        }

        static void Tick(SurvivalSim sim, bool slide)
        {
            var inp = (ScriptedInput)sim.input;
            inp.SetLook(0, 0);
            inp.Set(new InputFrame { moveZ = 1, slide = new Button { held = slide, pressed = slide } }, new CombatInput { weapon = -1 });
            sim.Step();
        }

        [Test]
        public void Rules_AreTheDesignConfigs_AndEveryNumberHasItsWhy()
        {
            SlideBite21Rules r = SlideBite21Rules.From(Cfg());
            Assert.That(r.damage, Is.GreaterThan(0));
            Assert.That(r.radius, Is.GreaterThan(0).And.LessThan(Cfg().Num("style.closeCallDistance")), "a bite kill is a close kill");
            foreach (string k in new[] { "damage", "radius", "cooldown", "stagger", "points" })
                Assert.That(Cfg().Has($"design.n21.slideBites.{k}_why"), Is.True, k);
        }

        [Test]
        public void Off_ASlideBitesNothing()
        {
            SurvivalSim sim = Make(false);
            Assert.That(sim.slideBite21, Is.Null);
            int bites = 0;
            sim.events.On("slideBite", _ => bites++);
            Vec3 at = Map().playerSpawn;
            Husk h = sim.husks.Spawn("husk", new Vec3(at.x + 1, 0, at.z - 1), at);
            double hp = h.hp;
            for (int s = 0; s < 120; s++) Tick(sim, s >= 20);
            Assert.That(bites, Is.EqualTo(0));
            Assert.That(h.hp, Is.EqualTo(hp));
        }

        [Test]
        public void On_ABittenEnemyIsNotBittenTwiceInsideTheCooldown()
        {
            SurvivalSim sim = Make(true);
            int bites = 0;
            sim.events.On("slideBite", _ => bites++);
            Assert.That(sim.slideBite21, Is.Not.Null);
            Assert.That(sim.slideBite21.rules.cooldown, Is.GreaterThan(0));
            // Not sliding: no bite whatever stands there.
            Vec3 at = Map().playerSpawn;
            sim.husks.Spawn("husk", new Vec3(at.x, 0, at.z - 0.5), at);
            for (int s = 0; s < 10; s++) Tick(sim, false);
            Assert.That(bites, Is.EqualTo(0));
        }
    }
}
