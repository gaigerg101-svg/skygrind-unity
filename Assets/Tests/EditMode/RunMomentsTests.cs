// NIGHT11 U3-24.8: runMoments.js (Core/RunMoments.cs). The web's tests ported: tests/runModes.test.js :6-20 (the grade
// D .. S, the score line's contents) and tests/seeds.test.js :25-29 (the score line with and without the seed, exact).
// Then the template literal's JS semantics (String(number), seed / build truthiness, padStart), JS String(number)
// itself (JsNumber: values whose V8 text the ECMAScript layout fixes), and the tracker: the best style award (rounded,
// quiet ones skipped), a multi-kill within results.multiWindow (the port's sim clock), the top speed and the longest
// swing (rounded to 0.1), nothing counted outside a run. RunMoments.Step allocates nothing.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class RunMomentsTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        [Test]
        public void GradeOf_DToS_FromTheRoundTheBestRankAndTheAccuracy()
        {
            RunMoments.Grade g = RunMoments.Grade.From(cfg);
            Assert.That(RunMoments.GradeOf(1, 0, 0.1, g), Is.EqualTo("D"));
            Assert.That(RunMoments.GradeOf(25, 5, 0.6, g), Is.EqualTo("S"));
            Assert.That(RunMoments.GradeOf(12, 3, 0.4, g), Does.Match("[BC]"));
            Assert.That(g.letters, Is.EqualTo(new[] { "S", "A", "B", "C" }), "results.grade.bands in order");
        }

        [Test]
        public void ScoreLine_NamesTheRun()
        {
            string line = RunMoments.ScoreLine(12, 9000, "A", "SAVAGE", 725, new[] { "LOW GRAVITY" });
            Assert.That(line, Does.Contain("round 12"));
            Assert.That(line, Does.Contain("12:05"));
            Assert.That(line, Does.Contain("LOW GRAVITY"));
        }

        [Test]
        public void ScoreLine_NamesTheSeedWhenThereIsOne()
        {
            Assert.That(RunMoments.ScoreLine(3, 10, "D", "DULL", 61), Is.EqualTo("SKYGRIND survival: round 3, 10 pts, grade D, best DULL, 1:01"));
            Assert.That(RunMoments.ScoreLine(3, 10, "D", "DULL", 61, null, "00A1Z9"), Is.EqualTo("SKYGRIND survival: round 3, 10 pts, grade D, best DULL, 1:01, seed 00A1Z9"));
        }

        [Test]
        public void ScoreLine_KeepsTheTemplateLiteralsSemantics()
        {
            const string head = "SKYGRIND survival: round 7, 1234.5 pts, grade B, best BOLD, 59:59";
            Assert.That(RunMoments.ScoreLine(7, 1234.5, "B", "BOLD", 3599.9, new[] { "LOW GRAVITY", "GLASS CANNON" }, "K7Q2-X9", "2 mods, PROTOCOL"),
                Is.EqualTo(head + " [LOW GRAVITY + GLASS CANNON], seed K7Q2-X9, 2 mods, PROTOCOL"));
            // The JSON-shaped entry (a recorded call's values): falsy seeds and builds print nothing.
            foreach (object falsy in new object[] { null, "", 0.0, double.NaN, false })
            {
                Assert.That(RunMoments.ScoreLine(7.0, 1234.5, "B", "BOLD", 3599.9, null, falsy, falsy), Is.EqualTo(head), $"seed / build {falsy ?? "null"}");
            }
            Assert.That(RunMoments.ScoreLine(7.0, 1234.5, "B", "BOLD", 3599.9, new List<object>(), 12.0, true), Is.EqualTo(head + ", seed 12, true"));
            Assert.That(RunMoments.ScoreLine(7.0, 0.1 + 0.2, null, "BOLD", 5, new List<object> { "A", null, "B" }, null, null),
                Is.EqualTo("SKYGRIND survival: round 7, 0.30000000000000004 pts, grade null, best BOLD, 0:05 [A +  + B]"));
        }

        [Test]
        public void JsNumber_PrintsAsJsString()
        {
            var cases = new (double x, string s)[]
            {
                (0, "0"), (-0.0, "0"), (double.NaN, "NaN"), (double.PositiveInfinity, "Infinity"), (double.NegativeInfinity, "-Infinity"),
                (1, "1"), (-12.75, "-12.75"), (100, "100"), (1234.5, "1234.5"), (0.1, "0.1"), (0.1 + 0.2, "0.30000000000000004"),
                (1.0 / 3, "0.3333333333333333"), (2.0 / 3, "0.6666666666666666"), (4.35, "4.35"), (123.456, "123.456"),
                (1e20, "100000000000000000000"), (1e21, "1e+21"), (1.5e300, "1.5e+300"), (123456789012345680000.0, "123456789012345680000"),
                (1152921504606846976.0, "1152921504606847000"), (9007199254740992.0, "9007199254740992"), (9007199254740993.0, "9007199254740992"),
                (0.000001, "0.000001"), (0.0000015, "0.0000015"), (1e-7, "1e-7"), (1.5e-7, "1.5e-7"), (1.2345e-10, "1.2345e-10"),
                (5e-324, "5e-324"), (double.MaxValue, "1.7976931348623157e+308"), (2.2250738585072014e-308, "2.2250738585072014e-308"),
            };
            foreach (var (x, s) in cases) Assert.That(JsNumber.ToString(x), Is.EqualTo(s), $"String({x:R})");
            Assert.That(JsNumber.Truthy(0.0), Is.False);
            Assert.That(JsNumber.Truthy(""), Is.False);
            Assert.That(JsNumber.Truthy(null), Is.False);
            Assert.That(JsNumber.Truthy("0"), Is.True);
        }

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly RunMoments m;
            public double speed, t;
            public Vec3 pos;
            public bool attached;

            public Rig() => m = new RunMoments(cfg, events, () => speed, () => pos, () => attached, () => t);

            public void Award(string name, double points, bool quiet = false) => events.Emit("styleAward", new StyleAwardEvent { name = name, points = points, quiet = quiet });

            public void Kill(double at, string owner = "player")
            {
                t = at;
                events.Emit("kill", new KillEvent { owner = owner, kind = "husk" });
            }
        }

        [Test]
        public void Tracker_TheBestAward_Rounded_QuietOnesSkipped()
        {
            var r = new Rig();
            r.Award("BEFORE", 500); // not in a run
            r.m.Start();
            Assert.That(r.m.Get().bestAwardName, Is.Null, "none yet");
            r.Award("AIRSHOT", 30.4);
            r.Award("RICOSHOT", 50.6);
            r.Award("SECRET", 999, quiet: true);
            Assert.That(r.m.Get().bestAwardName, Is.EqualTo("RICOSHOT"));
            Assert.That(r.m.Get().bestAwardPoints, Is.EqualTo(51), "Math.round at the award");
            r.Award("CLOSE", 50.7); // not above the kept (rounded) 51
            Assert.That(r.m.Get().bestAwardName, Is.EqualTo("RICOSHOT"));
            r.Award("IMPACT", 51.2);
            Assert.That(r.m.Get().bestAwardName, Is.EqualTo("IMPACT"));
            Assert.That(r.m.Get().bestAwardPoints, Is.EqualTo(51));
            r.m.Stop();
            r.Award("AFTER", 9000);
            Assert.That(r.m.Get().bestAwardName, Is.EqualTo("IMPACT"), "nothing after the run");
            r.m.Start();
            Assert.That(r.m.Get().bestAwardName, Is.Null, "a new run starts clean");
        }

        [Test]
        public void Tracker_AMultiKill_WithinTheWindow()
        {
            var r = new Rig();
            double w = cfg.Num("results.multiWindow");
            r.Kill(0); // not in a run
            r.m.Start();
            r.Kill(10);
            r.Kill(10 + w / 3);
            r.Kill(10 + w * 2 / 3);
            r.Kill(10 + w * 2 / 3, "rival"); // not yours
            Assert.That(r.m.Get().bestMulti, Is.EqualTo(3));
            r.Kill(10 + w * 2 / 3 + w + 0.01); // the window has moved on: 1
            Assert.That(r.m.Get().bestMulti, Is.EqualTo(3), "the best stays");
            r.m.Start();
            Assert.That(r.m.Get().bestMulti, Is.EqualTo(0), "a new run starts clean");
            r.Kill(20);
            r.Kill(20 + w * 0.99);
            Assert.That(r.m.Get().bestMulti, Is.EqualTo(2));
            r.Kill(20 + w * 3);
            Assert.That(r.m.Get().bestMulti, Is.EqualTo(2));
        }

        [Test]
        public void Tracker_TopSpeed_AndTheLongestSwing_RoundedTo01()
        {
            var r = new Rig();
            r.speed = 99;
            r.m.Step(); // not in a run
            r.m.Start();
            r.speed = 12.345;
            r.m.Step();
            r.speed = 5;
            r.m.Step();
            Assert.That(r.m.Get().topSpeed, Is.EqualTo(12.3));
            // A swing: attach at the origin, swing 5 m then 6 m more, release.
            r.pos = new Vec3(0, 3, 0);
            r.m.OnMover(new MoverEvent { type = MoverEventType.RopeAttach });
            r.attached = true;
            r.pos = new Vec3(3, 3, 4);
            r.m.Step();
            r.pos = new Vec3(3, 9, 10); // height doesn't count (the horizontal distance)
            r.m.Step();
            r.attached = false;
            r.m.OnMover(new MoverEvent { type = MoverEventType.RopeRelease });
            Assert.That(r.m.Get().longestSwing, Is.EqualTo(11));
            // A shorter one and steps while not attached change nothing.
            r.m.RopeAttach();
            r.attached = true;
            r.pos = new Vec3(4, 3, 10);
            r.m.Step();
            r.attached = false;
            r.pos = new Vec3(40, 3, 10);
            r.m.Step();
            r.m.RopeRelease();
            r.m.RopeAttach("rival"); // another owner's rope
            Assert.That(r.m.Get().longestSwing, Is.EqualTo(11));
            var stats = new RunStats();
            r.m.Fill(stats);
            Assert.That((stats.topSpeed, stats.longestSwing, stats.bestMulti, stats.bestAwardName), Is.EqualTo((12.3, 11.0, 0, (string)null)));
        }

        [Test]
        public void Step_AllocatesNothing()
        {
            var r = new Rig();
            r.m.Start();
            r.attached = true;
            r.m.RopeAttach();
            for (int i = 0; i < 10; i++) r.m.Step(); // warm-up (the JIT)
            int allocs = Allocs(() =>
            {
                for (int i = 0; i < 600; i++)
                {
                    r.speed = i * 0.1;
                    r.pos = new Vec3(i * 0.05, 0, 0);
                    r.m.Step();
                }
            });
            Assert.That(r.m.Get().topSpeed, Is.EqualTo(59.9));
            Assert.That(allocs, Is.EqualTo(0), "GC allocations over 600 RunMoments.Step");
        }

#if UNITY_2017_1_OR_NEWER
        /// <summary>GC allocation samples made by `a` on this thread (ZeroAllocTests.Allocs).</summary>
        static int Allocs(Action a)
        {
            var recorder = UnityEngine.Profiling.Recorder.Get("GC.Alloc");
            recorder.enabled = false;
            recorder.FilterToCurrentThread();
            recorder.enabled = true;
            try
            {
                a();
            }
            finally
            {
                recorder.enabled = false;
                recorder.CollectFromAllThreads();
            }
            return recorder.sampleBlockCount;
        }
#else
        /// <summary>Bytes allocated by `a` on this thread (outside Unity: the scratch .NET run).</summary>
        static int Allocs(Action a)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            a();
            return (int)(GC.GetAllocatedBytesForCurrentThread() - before);
        }
#endif
    }
}
