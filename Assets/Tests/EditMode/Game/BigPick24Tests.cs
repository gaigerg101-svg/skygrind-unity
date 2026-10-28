// NIGHT24 (builder F): the BIG PICK's tests that need the Game assembly: its first-showing prompt (the first-fifteen rule's "bigpick" moment: shown at the
// draft's open, once per profile, gone on the first pick), its seeded stream across a save and a load, and the boot prewarm's promise that it leaves a
// sim's state alone. The rule itself is judged headless by `SimHarness fist --bigpick`.
using System.Collections;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class BigPick24Tests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static SurvivalSim BigSim(uint seed)
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            sim.noRecords = sim.noLab = true;
            sim.n24Override = new N24Blocks { bigPick = true, chargeFist = true };
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(seed, null, null);
            return sim;
        }

        [Test]
        public void ANewProfile_SeesTheBigPickPrompt_AtOnce_AndItIsGoneAfterTheFirstPick()
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            sim.n24Override = new N24Blocks { bigPick = true };
            sim.noLab = true; // the web's rules (a headless store has no LAB rows): as FirstFifteenWatchTests
            sim.metaSilent = false; // a new player's recording run
            var watch = new FirstFifteenWatch(sim, sim.profile, null) { Force = true };
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(1, null, null);
            Assert.That(sim.mods.big != null && sim.mods.big.Active, Is.True, "the BIG PICK is this run's draft");
            Assert.That(watch.rule.Has(FirstFifteenWatch.BigPick), Is.True, "design.adds.firstFifteen lists the bigpick moment");
            Assert.That(watch.rule.Active, Is.Null);
            sim.mods.Queue(5);
            sim.mods.Open(); // the draft opens: the prompt needs no wait (the contract is within 10 s)
            Assert.That(watch.rule.Active, Is.Not.Null, "a prompt is showing when the BIG PICK opens");
            Assert.That(watch.rule.Active.id, Is.EqualTo(FirstFifteenWatch.BigPick));
            Assert.That(watch.rule.Active.line, Does.Contain("TAKE ONE"));
            Assert.That(sim.mods.Pick(0), Is.True);
            Assert.That(watch.rule.Active, Is.Null, "gone on the first pick");
            Assert.That(watch.rule.Learned(FirstFifteenWatch.BigPick), Is.True, "learned for the profile");
            sim.mods.Queue(10);
            sim.mods.Open();
            Assert.That(watch.rule.Active, Is.Null, "shown once per profile: the second BIG PICK has no prompt");
        }

        [Test]
        public void ARunSavedBeforeRound5_AndLoaded_DrawsTheSameCards_AndKeepsTheStreamAfterAPick()
        {
            SurvivalSim a = BigSim(2424);
            string snap0 = RunSnapshot.Save(a.SnapshotSections());
            SurvivalSim b = BigSim(2424);
            RunSnapshot.Load(snap0, b.SnapshotSections());
            a.events.Emit("roundClear", new RoundEvent { number = 5 });
            b.events.Emit("roundClear", new RoundEvent { number = 5 });
            Assert.That(string.Join(",", b.mods.Open()), Is.EqualTo(string.Join(",", a.mods.Open())), "round 5's three cards after a load");
            a.mods.Pick(0);
            b.mods.Pick(0);
            // the stream has moved on in `a`: a fresh composition that loads the snapshot must continue it, not restart it
            string snap1 = RunSnapshot.Save(a.SnapshotSections());
            SurvivalSim c = BigSim(2424);
            RunSnapshot.Load(snap1, c.SnapshotSections());
            a.events.Emit("roundClear", new RoundEvent { number = 10 });
            c.events.Emit("roundClear", new RoundEvent { number = 10 });
            Assert.That(string.Join(",", c.mods.Open()), Is.EqualTo(string.Join(",", a.mods.Open())), "round 10's three cards after a save, a pick and a load");
        }

        [Test]
        public void ThePrewarm_LeavesTheSimStateHashAlone_AndTheScreensAsTheyWere()
        {
            SurvivalSim sim = BigSim(7);
            for (int i = 0; i < 240; i++) sim.Step();
            ulong before = SimFingerprint.Hash(sim);
            var shown = new GameObject("prewarm shown", typeof(RectTransform));
            var hidden = new GameObject("prewarm hidden", typeof(RectTransform));
            var grouped = new GameObject("prewarm grouped", typeof(RectTransform), typeof(CanvasGroup));
            hidden.SetActive(false);
            grouped.SetActive(false);
            grouped.GetComponent<CanvasGroup>().alpha = 0.5f;
            try
            {
                int woken = -1;
                IEnumerator pass = Prewarm24.WakeScreens(() => true, new[] { shown, hidden, grouped, null }, 2, n => woken = n);
                int frames = 0;
                while (pass.MoveNext()) frames++;
                Assert.That(woken, Is.EqualTo(3), "three screens woken (the null skipped)");
                Assert.That(frames, Is.EqualTo(6), "two frames each");
                Assert.That(shown.activeSelf, Is.True);
                Assert.That(hidden.activeSelf, Is.False, "put back hidden");
                Assert.That(grouped.activeSelf, Is.False);
                Assert.That(shown.GetComponent<CanvasGroup>(), Is.Null, "the CanvasGroup it added is gone");
                Assert.That(hidden.GetComponent<CanvasGroup>(), Is.Null);
                Assert.That(grouped.GetComponent<CanvasGroup>().alpha, Is.EqualTo(0.5f), "an existing group's alpha restored");
                Assert.That(SimFingerprint.Hash(sim), Is.EqualTo(before), "the sim's state hash is unchanged by the prewarm");
                // it stops at once when a run starts (the title is over)
                bool title = true;
                IEnumerator stops = Prewarm24.WakeScreens(() => title, new[] { shown, hidden }, 1, n => woken = n);
                stops.MoveNext();
                title = false;
                while (stops.MoveNext()) { }
                Assert.That(woken, Is.EqualTo(1), "only the screen woken before the run started");
            }
            finally
            {
                Object.DestroyImmediate(shown);
                Object.DestroyImmediate(hidden);
                Object.DestroyImmediate(grouped);
            }
        }
    }
}
