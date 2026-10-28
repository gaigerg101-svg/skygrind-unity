// NIGHT14 U3-27.5: THE AUTOPILOT (Game/Bot/AutopilotInput.cs), headless. The bot's run is a pure function of (seed,
// difficulty, build): the same seed twice on fresh compositions prints the same hashes (rule 8). The meta never records a
// bot (rule 7): an autopilot run to its end leaves the profile as it was. The bot never cheats (rule 6): the aim assist is
// off while it drives, and it plays at real HP. A waiting MODS draft is taken (design.autopilot.takeFirstMod).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class AutopilotTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        /// <summary>A fresh headless composition with the autopilot driving, its run started on `seed`.</summary>
        static (SurvivalSim, AutopilotInput) Start(uint seed, string difficulty)
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            var ap = new AutopilotInput(sim, difficulty, seed);
            sim.UseInput(ap);
            sim.StartRun(seed, null, null);
            return (sim, ap);
        }

        /// <summary>Step until RESULTS or `maxSteps`, the drafts taken; a hash every `every` steps into `hashes` (null: none).</summary>
        static int Play(SurvivalSim sim, int maxSteps, int every, List<string> hashes)
        {
            int i = 0;
            for (; i < maxSteps && sim.flow != Flow.Results; i++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                if (hashes != null && (i + 1) % every == 0) hashes.Add($"step {i + 1} hash {SimFingerprint.Hash(sim):x16}");
            }
            return i;
        }

        [Test]
        public void Enabled_ReadsTheFlag()
        {
            GameConfig cfg = Cfg();
            Assert.That(AutopilotInput.Enabled(cfg), Is.EqualTo(cfg.Design("autopilot.enabled")));
        }

        [Test]
        public void Constructor_SetsTheBotsRules()
        {
            (SurvivalSim sim, AutopilotInput ap) = Start(1, "normal");
            Assert.That(sim.metaSilent, Is.True, "rule 7: the meta records nothing");
            Assert.That(sim.noLab, Is.True, "the web's rules whatever the LAB says");
            Assert.That(sim.AssistMode, Is.EqualTo("off"), "rule 6: no aim assist turns the bot's aim");
            Assert.That(ap.Brain, Is.Not.Null);
            Assert.That(ap.Brain.Difficulty, Is.EqualTo("normal"));
            Assert.That(sim.health.max, Is.EqualTo(sim.cfg.Num("health.max")), "rule 6: real HP");
        }

        [Test]
        public void SameSeedTwice_SameHashes()
        {
            var a = new List<string>();
            var b = new List<string>();
            (SurvivalSim simA, _) = Start(1, "normal");
            Play(simA, 7200, 600, a);
            (SurvivalSim simB, _) = Start(1, "normal");
            Play(simB, 7200, 600, b);
            Assert.That(a.Count, Is.GreaterThan(0));
            Assert.That(b, Is.EqualTo(a), "the same seed and difficulty on fresh compositions: the same run");
        }

        [Test]
        public void AnotherSeed_PlaysAnotherRun()
        {
            var a = new List<string>();
            var b = new List<string>();
            (SurvivalSim simA, _) = Start(1, "normal");
            Play(simA, 2400, 600, a);
            (SurvivalSim simB, _) = Start(2, "normal");
            Play(simB, 2400, 600, b);
            Assert.That(b, Is.Not.EqualTo(a));
        }

        [Test]
        public void TheBotFights()
        {
            (SurvivalSim sim, AutopilotInput ap) = Start(3, "normal");
            Vec3 spawn = sim.mover.position;
            bool hadTarget = false;
            double moved = 0;
            for (int i = 0; i < 7200 && sim.flow == Flow.Playing; i++)
            {
                AutopilotInput.TakeDraft(sim);
                sim.Step();
                hadTarget |= ap.HasTarget;
                double dx = sim.mover.position.x - spawn.x, dz = sim.mover.position.z - spawn.z;
                moved = System.Math.Max(moved, System.Math.Sqrt(dx * dx + dz * dz));
            }
            Assert.That(hadTarget, Is.True, "the target adapter chose an enemy");
            Assert.That(sim.run.stats.shots, Is.GreaterThan(0), "the brain fired the player's gun");
            Assert.That(moved, Is.GreaterThan(2), "the brain moved the player's body");
        }

        [Test]
        public void TakeDraft_TakesTheFirstCard()
        {
            // the legacy percentage draft (NIGHT21's, a mod stacks): the NIGHT24 blocks forced off, the BIG PICK case is the next test
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            sim.n24Override = N24Blocks.Off;
            sim.UseInput(new AutopilotInput(sim, "normal", 4));
            sim.StartRun(4, null, null);
            Assert.That(sim.run.state, Is.EqualTo(SurvivalRun.State.Break));
            sim.mods.Queue(sim.run.round);
            Assert.That(sim.mods.Pending, Is.True);
            int before = 0;
            foreach (var kv in sim.mods.stacks) before += kv.Value;
            AutopilotInput.TakeDraft(sim);
            int after = 0;
            foreach (var kv in sim.mods.stacks) after += kv.Value;
            Assert.That(sim.mods.Pending, Is.False);
            Assert.That(sim.mods.offer, Is.Null, "no draft left open");
            Assert.That(after, Is.EqualTo(before + 1), "one card taken");
        }

        [Test]
        public void TakeDraft_TakesABigPickCard_InABreak_AndAtALayerEnd()
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            sim.n24Override = new N24Blocks { bigPick = true };
            sim.UseInput(new AutopilotInput(sim, "normal", 4));
            sim.StartRun(4, null, null);
            Assert.That(sim.run.state, Is.EqualTo(SurvivalRun.State.Break));
            Assert.That(sim.mods.big != null && sim.mods.big.Active, Is.True, "the BIG PICK is this run's draft");
            int takes = 0;
            sim.events.On("bigPickTake", _ => takes++);
            sim.mods.Queue(5);
            Assert.That(sim.mods.Pending, Is.True);
            AutopilotInput.TakeDraft(sim);
            Assert.That(sim.mods.Pending, Is.False);
            Assert.That(sim.mods.offer, Is.Null, "no draft left open");
            Assert.That(takes, Is.EqualTo(1), "one BIG PICK card taken (no mod stacks)");
            // the campaign's layer-end pick opens outside a break (Mods.LayerPick): the bot takes it too
            sim.mods.QueueLayer();
            Assert.That(sim.mods.LayerPick, Is.True);
            AutopilotInput.TakeDraft(sim);
            Assert.That(takes, Is.EqualTo(2), "the layer-end card taken");
        }

        /// <summary>
        /// GC allocation samples made on this thread inside each window, from ONE GC.Alloc recorder held for the whole test and
        /// filtered to this thread once (EXT53 B: the flake of NIGHT14-NIGHT53). Unity 6000.3's Recorder.Get makes a new wrapper on
        /// every call (UnityEngine.CoreModule IL: Recorder::.ctor => new ProfilerRecorder(handle, 1, 0x99), 0x99 = StartImmediately
        /// | WrapAroundWhenCapacityReached | SumAllSamplesInFrame | SharedRecorder): the marker's shared native recorder starts at
        /// once with no thread filter, and each wrapper's finalizer releases it from the finalizer thread (Recorder::Finalize =>
        /// ProfilerRecorder::Dispose => Control(Release)). The old per-window helper (ZeroAllocTests' logic) did that twice a
        /// step: 43,200 unfiltered starts and 43,200 finalizer releases racing the windows, the most of any zero-alloc test, and
        /// the red was 1 sample at a different step each time, only under load, never on CoreCLR (0 B in all 43,200 windows).
        /// Here nothing is created or finalized while the windows run, and the filter never leaves this thread.
        /// </summary>
        sealed class AllocWindows
        {
            readonly UnityEngine.Profiling.Recorder recorder;

            public AllocWindows()
            {
                System.GC.Collect();
                System.GC.WaitForPendingFinalizers(); // earlier tests' wrappers released before the first window
                recorder = UnityEngine.Profiling.Recorder.Get("GC.Alloc");
                recorder.enabled = false;
                recorder.FilterToCurrentThread();
                Count(() => { }); // one empty filtered window: what the unfiltered start caught is flushed here
            }

            /// <summary>GC allocation samples made by `a` on this thread.</summary>
            public int Count(System.Action a)
            {
                recorder.enabled = true;
                try
                {
                    a();
                }
                finally
                {
                    recorder.enabled = false;
                }
                return recorder.sampleBlockCount;
            }

            /// <summary>The shared recorder back to every thread (as the old helper left it after each window).</summary>
            public void Close()
            {
                recorder.enabled = false;
                recorder.CollectFromAllThreads();
            }
        }

        static object allocSink;

        /// <summary>EXT53 B: the one-recorder windows still see an allocation (the probe isn't blind), window by window: an
        /// allocating window, then empty ones read 0 again (no count carries over).</summary>
        [Test]
        public void AllocWindows_SeeAnAllocation_AndNothingInAnEmptyWindow()
        {
            var windows = new AllocWindows();
            try
            {
                System.Action none = () => { }, one = () => allocSink = new int[4];
                Assert.That(windows.Count(none), Is.EqualTo(0), "an empty window");
                Assert.That(windows.Count(one), Is.GreaterThanOrEqualTo(1), "an array allocated inside the window");
                Assert.That(windows.Count(none), Is.EqualTo(0), "the next empty window: nothing carried over");
                Assert.That(windows.Count(one), Is.GreaterThanOrEqualTo(1), "a second allocating window");
            }
            finally
            {
                windows.Close();
            }
        }

        /// <summary>PORT-RULES 6: the autopilot's own per-step path (SampleMove: the target, the shopper or the HOLD; SampleCombat)
        /// allocates nothing once warm. Measured around the two calls, made beside the sim's own (the extra brain steps change
        /// the run, which doesn't matter here), on the stand-in brain with no graph so the count is the adapter's alone (the
        /// ported brain, the follower and A*'s scratch growth are agent N's BotZeroAllocTests).</summary>
        [Test]
        public void PerStep_AllocatesNothing()
        {
            var factory = AutopilotInput.BrainFactory;
            var graphs = AutopilotInput.GraphSource;
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            AutopilotInput ap;
            try
            {
                AutopilotInput.BrainFactory = AutopilotInput.StandInBrain;
                AutopilotInput.GraphSource = AutopilotInput.NoGraph;
                ap = new AutopilotInput(sim, "normal", 6);
            }
            finally
            {
                AutopilotInput.BrainFactory = factory;
                AutopilotInput.GraphSource = graphs;
            }
            sim.UseInput(ap);
            sim.StartRun(6, null, null);
            System.Action move = () => ap.SampleMove(true), combat = () => ap.SampleCombat(true);
            int allocs = 0, fight = 0, shop = 0;
            // NIGHT39 V (C1 row 8): the windows that counted a sample, named, so a red names its step and call. The red is pre-existing
            // and not reproduced headless (NIGHT17, 18, 21, 33, 38 suites: "Expected 0, was 1", passing alone; `alloc` on CoreCLR: 0 B in
            // all 43,200 windows at u3-n37 and at NIGHT39, the run bit-identical with --off38): the same step twice says a code path
            // (run `alloc` there), a different step each red says the recorder under load. Built only after a window counts (no change
            // to a passing run's windows).
            var where = new List<string>();
            var windows = new AllocWindows(); // EXT53 B: one recorder for all 43,200 windows (AllocWindows' summary: the flake's cause)
            try
            {
                for (int i = 0; i < 24000 && sim.flow == Flow.Playing; i++)
                {
                    AutopilotInput.TakeDraft(sim);
                    if (i >= 2400)
                    {
                        bool breakStep = sim.run.state == SurvivalRun.State.Break;
                        int m = windows.Count(move), c = windows.Count(combat);
                        allocs += m + c;
                        if (m + c > 0 && where.Count < 8) where.Add($"step {i} ({(breakStep ? "break" : "fight")}): SampleMove {m}, SampleCombat {c}");
                        if (breakStep) shop++;
                        else fight++;
                    }
                    sim.Step();
                }
            }
            finally
            {
                windows.Close();
            }
            Assert.That(fight, Is.GreaterThan(0));
            Assert.That(shop, Is.GreaterThan(0));
            Assert.That(allocs, Is.EqualTo(0), $"over {fight} fight and {shop} break steps; windows: {string.Join("; ", where)}");
        }

        [Test]
        public void TheMetaRecordsNothing()
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            string before = sim.profile.ToJson();
            sim.UseInput(new AutopilotInput(sim, "normal", 5));
            sim.StartRun(5, null, null);
            int steps = Play(sim, 72000, 600, null);
            if (sim.flow != Flow.Results) sim.EndRun(); // still alive after 10 min of sim time: end it as a quit would
            Assert.That(sim.flow, Is.EqualTo(Flow.Results), $"after {steps} steps");
            Assert.That(sim.lastStats, Is.Not.Null);
            Assert.That(sim.lastStats.recorded, Is.False, "a bot's run is never recorded");
            Assert.That(sim.profile.runs, Is.EqualTo(0));
            Assert.That(sim.records.Best("survival"), Is.Null, "no personal best");
            Assert.That(sim.profile.ToJson(), Is.EqualTo(before), "records, contracts, mastery, DAILY, tips and runs unchanged");
        }
    }
}
