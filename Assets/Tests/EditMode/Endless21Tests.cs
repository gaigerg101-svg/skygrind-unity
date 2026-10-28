// NIGHT21 U3-35.7 THE GRIND (builder W): Core/Endless21.cs is endless.js line for line, Core/Records21.cs is records.js' endless
// record per HEAT. The web has no endless test (only records.test.js), so these pin the web's behaviour read from endless.js:
// the 4 s break with a full heal + stamina, the reshape hook (1.5 s) before the fight, the events with the web's names, death
// ends the run, score = style + 100 x waves. Tests-first: UNCOMPILED in Unity tonight (no Editor); the same assertions run
// headless in SimHarness `grind-selftest`.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Endless21Tests
    {
        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root(), "config.json"));

        static string Root()
        {
            try { return Port.Root; }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        sealed class FakeSpawner : IRoundSpawner
        {
            public int fightSteps = 10, left, started, cleared;
            public bool done;
            public void StartWave(int round, uint seed) { started = round; left = fightSteps; done = false; }
            public void Step(double dt) { if (left > 0 && --left == 0) done = true; }
            public bool Done => done;
            public void Clear() { cleared++; left = 0; done = false; }
        }

        sealed class Rig
        {
            public readonly FakeSpawner spawner = new FakeSpawner();
            public readonly List<string> events = new List<string>();
            public readonly List<object> payloads = new List<object>();
            public int heals, stamina, reshapes;
            public double style;
            public Endless21 endless;

            public Rig(bool withReshape = true)
            {
                endless = new Endless21(Endless21.Params.From(Cfg), 120, null, new EndlessDeps
                {
                    spawner = spawner,
                    healthReset = () => heals++,
                    restoreStamina = () => stamina++,
                    styleTotal = () => style,
                    styleBestRank = () => "C",
                    reshape = withReshape ? (System.Func<int, double>)(w => { reshapes++; return 1.5; }) : null,
                }, (type, payload) => { events.Add(type); payloads.Add(payload); });
            }

            public void Steps(int n) { for (int i = 0; i < n; i++) endless.Step(1.0 / 120); }
        }

        [Test]
        public void TheNumbers_AreTheWebsEndlessConfig()
        {
            Endless21.Params p = Endless21.Params.From(Cfg);
            Assert.That((p.breakTime, p.bannerTime, p.reshapeTime, p.pointsPerWave), Is.EqualTo((4.0, 1.6, 1.5, 100.0)));
        }

        [Test]
        public void Start_BreaksFirst_WithAFullHealAndStamina_AndTheWebsEvent()
        {
            var r = new Rig();
            r.endless.Start(7);
            Assert.That(r.endless.Phase, Is.EqualTo(Endless21.State.Break));
            Assert.That((r.endless.CurrentWave, r.heals, r.stamina), Is.EqualTo((1, 1, 1)));
            Assert.That(r.events, Is.EqualTo(new[] { "waveBreak" }));
            var e = (WaveEvent)r.payloads[0];
            Assert.That((e.number, e.time), Is.EqualTo((1, 4.0)));
        }

        [Test]
        public void TheBreak_IsFourSeconds_ThenTheReshape_ThenTheFight()
        {
            var r = new Rig();
            r.endless.Start(7);
            r.Steps(479);
            Assert.That(r.endless.Phase, Is.EqualTo(Endless21.State.Break), "still breaking one step before 4 s");
            r.Steps(1);
            Assert.That(r.endless.Phase, Is.EqualTo(Endless21.State.Reshape), "4 s: the reshape starts");
            Assert.That(r.reshapes, Is.EqualTo(1));
            r.Steps(179);
            Assert.That(r.endless.Phase, Is.EqualTo(Endless21.State.Reshape));
            r.Steps(1);
            Assert.That(r.endless.Phase, Is.EqualTo(Endless21.State.Fight), "1.5 s later the fight");
            Assert.That(r.spawner.started, Is.EqualTo(1));
            Assert.That(r.events, Is.EqualTo(new[] { "waveBreak", "waveStart" }));
        }

        [Test]
        public void WithoutAReshape_TheFightStartsRightAfterTheBreak()
        {
            var r = new Rig(withReshape: false);
            r.endless.Start(7);
            r.Steps(480);
            Assert.That(r.endless.Phase, Is.EqualTo(Endless21.State.Fight));
        }

        [Test]
        public void AClearedWave_Scores100PerWave_PlusStyle_AndBreaksAgainWithAHeal()
        {
            var r = new Rig();
            r.endless.Start(7);
            r.style = 250;
            r.Steps(660 + 10); // the break, the reshape, the ten-step fight
            Assert.That(r.endless.Cleared, Is.EqualTo(1));
            Assert.That(r.endless.Score, Is.EqualTo(350.0), "style 250 + 100 x 1");
            Assert.That(r.events, Is.EqualTo(new[] { "waveBreak", "waveStart", "waveClear", "waveBreak" }));
            Assert.That(((WaveEvent)r.payloads[2]).score, Is.EqualTo(350.0));
            Assert.That((r.endless.CurrentWave, r.heals, r.stamina), Is.EqualTo((2, 2, 2)));
            Assert.That(r.endless.Phase, Is.EqualTo(Endless21.State.Break));
        }

        [Test]
        public void End_StopsTheRun_EmitsRunEnd_AndReportsTheStats()
        {
            var r = new Rig();
            r.endless.Start(7);
            r.Steps(665); // mid-fight (the ten-step fight ends at step 670)
            r.style = 40;
            EndlessStats s = r.endless.End();
            Assert.That(s.mode, Is.EqualTo("endless"));
            Assert.That((s.lastWave, s.cleared, s.score), Is.EqualTo((1, 0, 40.0)));
            Assert.That(r.events[r.events.Count - 1], Is.EqualTo("runEnd"));
            Assert.That(r.endless.Phase, Is.EqualTo(Endless21.State.Over));
            int n = r.events.Count;
            r.Steps(1000);
            Assert.That(r.events.Count, Is.EqualTo(n), "an ended run steps no more");
            Assert.That(r.endless.End(), Is.Null, "and ends once");
        }

        [Test]
        public void DebugSetWave_ClearsTheFieldAndBreaksBeforeN()
        {
            var r = new Rig();
            r.endless.Start(7);
            r.endless.DebugSetWave(5);
            Assert.That((r.endless.CurrentWave, r.endless.Cleared, r.endless.Phase), Is.EqualTo((5, 4, Endless21.State.Break)));
        }

        [Test]
        public void PickPreset_NeverTheShapeTheCoreWears_AndIsSeeded()
        {
            string[] presets = { "stairs", "ring", "towers", "random", "platforms", "pen", "pillars" };
            var a = new Mulberry32(99);
            var b = new Mulberry32(99);
            string last = "flat";
            for (int i = 0; i < 200; i++)
            {
                string pa = Endless21.PickPreset(presets, last, a);
                Assert.That(pa, Is.Not.EqualTo(last));
                Assert.That(Endless21.PickPreset(presets, last, b), Is.EqualTo(pa), "the same seed, the same shapes");
                last = pa;
            }
            Assert.That(Endless21.PickPreset(new string[0], "flat", a), Is.Null);
        }

        [Test]
        public void PickPreset_WithALap_ShowsEveryShapeOnceBeforeOneReturns()
        {
            string[] presets = { "stairs", "ring", "towers", "random", "platforms", "pen", "pillars" };
            var rng = new Mulberry32(5);
            var used = new HashSet<string>();
            var lap = new HashSet<string>();
            string last = "flat";
            for (int i = 0; i < 7; i++)
            {
                last = Endless21.PickPreset(presets, last, rng, used);
                Assert.That(lap.Add(last), Is.True, "no shape twice in the first lap: " + last);
            }
            string next = Endless21.PickPreset(presets, last, rng, used);
            Assert.That(next, Is.Not.EqualTo(last), "the new lap does not repeat the shape on now");
        }

        [Test]
        public void HeightsHash_DiffersBetweenPresets()
        {
            var seen = new HashSet<ulong>();
            foreach (string p in new[] { "flat", "stairs", "ring", "towers", "platforms", "pen", "pillars" })
                Assert.That(seen.Add(Endless21.HeightsHash(CoreGrid.PresetHeights(p, 12, 4, 0.5, new Mulberry32(1)))), Is.True, p);
        }

        [Test]
        public void Records_BestPerHeat_TheWebsJudgeLines_AndTheVersionStamp()
        {
            var rec = new Records21();
            Assert.That(rec.Submit(0, 4, 900).text, Is.EqualTo("FIRST RUN ON RECORD"));
            Records21.Judgement j = rec.Submit(0, 6, 1250);
            Assert.That((j.newBest, j.text), Is.EqualTo((true, "NEW BEST · +350")));
            j = rec.Submit(0, 3, 600);
            Assert.That((j.newBest, j.text), Is.EqualTo((false, "BEST 1250 · 650 SHORT")));
            Assert.That(rec.BestWave(0), Is.EqualTo(6));
            Assert.That(rec.Submit(2, 2, 300).first, Is.True, "a HEAT rung has its own best");
            Records21 back = Records21.Read(rec.Serialize());
            Assert.That((back.BestWave(0), back.BestWave(2), back.BestWave(1)), Is.EqualTo((6, 2, 0)));
            Assert.That(Records21.Read("v2;0:9:9").BestWave(0), Is.EqualTo(0), "another version is dropped");
            Assert.That(Records21.Read("v1;nonsense").BestWave(0), Is.EqualTo(0));
            Assert.That(Records21.Read(null).BestWave(0), Is.EqualTo(0));
        }
    }
}
