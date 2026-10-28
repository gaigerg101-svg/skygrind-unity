// NIGHT22 U3-36.6 THE GRIND wired (builder W): tests-first. One focused test per rule:
//  - Records21 survives a restart (a file under the persistent data path: written, then read by a fresh instance),
//  - the rising-edge telegraph: which pillars the next shape lifts (Endless21.Rising) and when the telegraph is on (Endless21.Telegraphing).
// The same assertions run headless in SimHarness `grind2 --selftest`.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Core.Tests
{
    public class Grind22Tests
    {
        static string TempFile(string name) => Path.Combine(Path.GetTempPath(), "skygrind-grind22-" + System.Guid.NewGuid().ToString("N"), name);

        [Test]
        public void Records_SurviveARestart_ThroughAFile()
        {
            string path = TempFile("grind-records.txt");
            try
            {
                var run1 = new Records21();
                run1.Submit(0, 5, 900);
                run1.Submit(2, 8, 1700);
                Assert.That(run1.Save(path), Is.True, "the first write makes the folder and the file");

                var restarted = new Records21(); // a new process: nothing in memory
                Assert.That(restarted.BestWave(0), Is.EqualTo(0));
                restarted = Records21.Load(path);
                Assert.That((restarted.BestWave(0), restarted.BestWave(2), restarted.BestWave(1)), Is.EqualTo((5, 8, 0)));

                restarted.Submit(0, 9, 3000); // a better run saves over the old best
                restarted.Save(path);
                Assert.That(Records21.Load(path).BestWave(0), Is.EqualTo(9));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
                string dir = Path.GetDirectoryName(path);
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
            }
        }

        [Test]
        public void Records_AMissingOrCorruptFile_IsEmpty_NeverAnError()
        {
            Assert.That(Records21.Load(TempFile("nothing-here.txt")).BestWave(0), Is.EqualTo(0));
            string path = TempFile("garbage.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            try
            {
                File.WriteAllText(path, "v1;not:a:record");
                Assert.That(Records21.Load(path).BestWave(0), Is.EqualTo(0));
                File.WriteAllText(path, "v9;0:7:100");
                Assert.That(Records21.Load(path).BestWave(0), Is.EqualTo(0), "another version is dropped");
            }
            finally
            {
                Directory.Delete(Path.GetDirectoryName(path), true);
            }
        }

        [Test]
        public void Telegraph_MarksOnlyThePillarsTheNextShapeLifts()
        {
            double[] now = { 0, 1, 2, 3, 2 };
            double[] next = { 2, 1, 1, 3.4, 4 };
            bool[] rising = Endless21.Rising(now, next, 0.5);
            Assert.That(rising, Is.EqualTo(new[] { true, false, false, false, true }), "up by a whole step or more; level, lower and a hair higher stay unmarked");
        }

        sealed class Spawner : IRoundSpawner
        {
            public void StartWave(int round, uint seed) { }
            public void Step(double dt) { }
            public bool Done => false;
            public void Clear() { }
        }

        [Test]
        public void Telegraph_IsOnForTheLastSecondsOfTheBreak_AndTheReshape_ThenOff()
        {
            const double hz = 120, dt = 1 / hz;
            var p = new Endless21.Params { breakTime = 4, bannerTime = 1.5, reshapeTime = 1.5, pointsPerWave = 100 };
            var e = new Endless21(p, hz, null, new EndlessDeps { spawner = new Spawner(), healthReset = () => { }, restoreStamina = () => { }, styleTotal = () => 0, reshape = w => 1.5 }, (t, o) => { });
            Assert.That(e.Telegraphing(2.5, dt), Is.False, "idle");
            e.Start(1);
            Assert.That(e.Phase, Is.EqualTo(Endless21.State.Break));
            Assert.That(e.Telegraphing(2.5, dt), Is.False, "the break's first 1.5 s are quiet");
            int firstOn = -1;
            for (int i = 1; i < 600 && e.Phase == Endless21.State.Break; i++)
            {
                e.Step(dt);
                if (firstOn < 0 && e.Telegraphing(2.5, dt)) firstOn = i;
            }
            Assert.That(firstOn >= 1.5 * hz - 2 && firstOn <= 1.5 * hz + 2, Is.True, $"(first on at step {firstOn}) on 2.5 s before the reshape begins (4 s break)");
            Assert.That(e.Phase, Is.EqualTo(Endless21.State.Reshape));
            Assert.That(e.Telegraphing(2.5, dt), Is.True, "the reshape itself still shows the marks");
            while (e.Phase == Endless21.State.Reshape) e.Step(dt);
            Assert.That(e.Phase, Is.EqualTo(Endless21.State.Fight));
            Assert.That(e.Telegraphing(2.5, dt), Is.False, "the fight has none");
        }
    }
}
