// NIGHT20 E1 (builder P): the first fifteen minutes' moments of need on a live sim (Game/FirstFifteenWatch.cs; the rule is
// Core/FirstFifteen.cs, FirstFifteenTests). Each verb shows at its moment and goes on its first use:
// - PARRY: the first spitter volley (a glob in the air at you); used on a parry;
// - PUNCH (melee): the first enemy within design.adds.firstFifteen.meleeMeters; used on a punch;
// - BOX: the first time the bank holds the box's price; used on a box roll;
// - HOLD: a HOLD ring appears; used when you stand inside it (the game's own test); gone when its round ends;
// - MODS: the first draft; used on the pick; gone when the round begins;
// - EXFIL: the radio offered; used on the call; gone when the round begins.
// Off with the block (design.adds AND design.adds.firstFifteen, taken at the run's start), in a bot's run (metaSilent) and
// with the profile's learned flags. Driven on a headless sim by its own events and state (no Unity runtime).
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class FirstFifteenWatchTests
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

        int saves;

        /// <summary>A headless run with the watch forced on (the adds bit is the lead's to turn on).</summary>
        (SurvivalSim, FirstFifteenWatch) Start(uint seed = 1)
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            sim.n24Override = N24Blocks.Off; // NIGHT24 premise: these tests are NIGHT21's verbs, the MODS draft among them; a BIG PICK run teaches "bigpick" instead (BigPick24Tests)
            saves = 0;
            var w = new FirstFifteenWatch(sim, new Profile(), () => saves++) { Force = true };
            sim.setup.mode = RunSetup.Survival;
            sim.noLab = true; // the web's rules (a headless store has no LAB rows)
            sim.StartRun(seed, null, null);
            return (sim, w);
        }

        [Test]
        public void Parry_ShowsOnTheFirstVolley_GoesOnAParry_AndTheProfileIsSaved()
        {
            (SurvivalSim sim, FirstFifteenWatch w) = Start();
            Assert.That(w.On, Is.True);
            sim.events.Emit("spitterVolley", 7);
            Assert.That(w.rule.Active?.id, Is.EqualTo("parry"));
            sim.events.Emit("parry", new HitEvent { owner = "player", weapon = "punch", kind = "orb" });
            Assert.That(w.rule.Active, Is.Null);
            Assert.That(saves, Is.EqualTo(1), "a learned verb saves the profile once");
            sim.events.Emit("spitterVolley", 7);
            Assert.That(w.rule.Active, Is.Null, "learned");
        }

        [Test]
        public void Box_ShowsWhenTheBankHoldsItsPrice_GoesOnARoll()
        {
            (SurvivalSim sim, FirstFifteenWatch w) = Start();
            Assume.That(sim.box, Is.Not.Null, "design.box on");
            sim.economy.total = sim.box.price - 1;
            w.Step();
            Assert.That(w.rule.Active, Is.Null, "short of the price");
            sim.economy.total = sim.box.price;
            w.Step();
            Assert.That(w.rule.Active?.id, Is.EqualTo("box"));
            sim.events.Emit("boxOpen", new BoxOpenEvent { owner = "player", price = sim.box.price });
            Assert.That(w.rule.Active, Is.Null);
        }

        [Test]
        public void Hold_ShowsWhenTheRingAppears_GoesWhenYouStandInIt()
        {
            (SurvivalSim sim, FirstFifteenWatch w) = Start();
            sim.events.Emit("objectiveStart", new ObjectiveStartEvent { type = Objectives.Hold, round = 6 });
            Assert.That(w.rule.Active?.id, Is.EqualTo("hold"));
            sim.run.DebugSkipTo(6);
            for (int i = 0; i < 120 * 30 && !sim.objectives.Holding; i++) sim.Step();
            Assert.That(sim.objectives.Holding, Is.True);
            Vec3 ring = sim.objectives.Spot.Value;
            sim.Respawn(ring, 0);
            sim.Step();
            Assert.That(sim.objectives.Inside, Is.True);
            w.Step();
            Assert.That(w.rule.Active, Is.Null, "inside by the game's own test: learned");
        }

        [Test]
        public void Mods_And_Exfil_GoWhenTheirMomentPasses_Unlearned()
        {
            (SurvivalSim sim, FirstFifteenWatch w) = Start();
            sim.events.Emit("modDraft", new ModEvent { round = 3 });
            Assert.That(w.rule.Active?.id, Is.EqualTo("mods"));
            sim.events.Emit("roundBegin", new RoundEvent { number = 3 });
            Assert.That(w.rule.Active, Is.Null, "the draft closed without a pick");
            sim.events.Emit("modDraft", new ModEvent { round = 4 });
            sim.events.Emit("modPick", new ModEvent { round = 4, id = "x" });
            Assert.That(w.rule.Active, Is.Null);
            Assert.That(saves, Is.EqualTo(1), "the pick learned MODS");
        }

        [Test]
        public void Exfil_ShowsWhileTheRadioIsOffered_GoesOnTheCall()
        {
            (SurvivalSim sim, FirstFifteenWatch w) = Start(3);
            Assume.That(sim.exfil.enabled, Is.True);
            sim.zones.Open("slabs", true);
            sim.run.DebugSkipTo(sim.exfil.from);
            Assert.That(sim.exfil.CanCall(), Is.True);
            w.Step();
            Assert.That(w.rule.Active?.id, Is.EqualTo("exfil"));
            Assert.That(sim.exfil.Call(), Is.True);
            Assert.That(w.rule.Active, Is.Null, "called");
        }

        [Test]
        public void Melee_ShowsWithAnEnemyInReach_GoesOnAPunch()
        {
            (SurvivalSim sim, FirstFifteenWatch w) = Start();
            int guard = 0;
            while (w.rule.Active == null && guard++ < 120 * 120)
            {
                sim.health.max = sim.health.hp = 1e6;
                sim.Step(); // nobody moves the player: the round's husks walk up to it
                w.Step();
            }
            Assert.That(w.rule.Active?.id, Is.EqualTo("melee"), "a husk walked into punching reach");
            sim.events.Emit("punch", new MeleeEvent { owner = "player", weapon = "punch", hits = 1 });
            Assert.That(w.rule.Active, Is.Null);
        }

        [Test]
        public void Off_ByTheBlock_AndInABotsRun()
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            var w = new FirstFifteenWatch(sim, new Profile(), null);
            sim.noLab = true;
            sim.StartRun(1, null, null);
            Assert.That(w.On, Is.EqualTo(sim.n20.Add(Cfg(), "firstFifteen")), "the run's adds bit and the row's switch");
            w.Force = true;
            sim.metaSilent = true; // the AUTOPILOT's runs
            sim.StartRun(2, null, null);
            Assert.That(w.On, Is.False, "never in a bot's run");
            sim.events.Emit("spitterVolley", 1);
            Assert.That(w.rule.Active, Is.Null);
        }
    }
}
