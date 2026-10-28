// NIGHT9 U3-07: the web's tests/spawner.test.js ported (every test there is on the pure spawnMath.js): the wave budget
// and mix, spawn groups, spawn points and the stuck rule, through SpawnMath (mulberry32 is Mulberry32, CoreGrid.cs).
// Nothing skipped. The bit-exact replay of spawnMath.json is GoldenPorted.cs.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class SpawnMathTests
    {
        static GameConfig cfg;
        static SpawnParams sp;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            sp = SpawnParams.From(cfg);
        }

        static Func<double> Rng(double seed) => new Mulberry32(seed).Next;

        static string Json(List<List<string>> groups) => string.Join("|", groups.Select(g => string.Join(",", g)));

        // --- wave budget and mix (waveLoop) ------------------------------------------------------------------------

        [Test]
        public void BudgetIs5Plus2xWave()
        {
            Expect.Exactly(SpawnMath.WaveBudget(1, sp), 7);
            Expect.Exactly(SpawnMath.WaveBudget(5, sp), 15);
            Expect.Exactly(SpawnMath.WaveBudget(10, sp), 25);
        }

        [Test]
        public void EarlyWavesAreHusksOnly_LaterDronesNeverExceed40PercentOfTheBudget_AndSpendItExactly()
        {
            for (int wave = 1; wave <= 30; wave++)
            {
                for (int seed = 1; seed <= 20; seed++)
                {
                    WaveMix m = SpawnMath.WaveComposition(wave, Rng(seed * 31 + wave), sp);
                    string at = $"wave {wave} seed {seed}";
                    if (wave < sp.dronesFromWave) Assert.That(m.drones, Is.EqualTo(0), at);
                    else Assert.That(m.drones, Is.GreaterThanOrEqualTo(1), at);
                    Assert.That(m.drones * sp.droneCost, Is.LessThanOrEqualTo(m.budget * sp.droneMaxShare), at);
                    Assert.That(m.husks * sp.huskCost + m.drones * sp.droneCost, Is.EqualTo(m.budget), at);
                }
            }
        }

        [Test]
        public void TheSameSeedAndWaveAlwaysRollTheSameGroups()
        {
            Assert.That(Json(SpawnMath.WaveGroups(4, 1234, sp)), Is.EqualTo(Json(SpawnMath.WaveGroups(4, 1234, sp))));
            var all = new HashSet<string>();
            for (int seed = 1; seed < 50; seed++) all.Add(Json(SpawnMath.WaveGroups(6, seed, sp)));
            Assert.That(all.Count, Is.GreaterThan(1)); // different runs differ
        }

        // --- spawn groups ------------------------------------------------------------------------------------------

        [Test]
        public void DealsEveryEnemyIntoGroupsOf3To5_ATinyWaveIsOneGroup()
        {
            Func<double> rng = Rng(7);
            for (int n = 1; n <= 40; n++)
            {
                List<int> sizes = SpawnMath.GroupSizes(n, rng, sp);
                Assert.That(sizes.Sum(), Is.EqualTo(n), $"n {n}");
                if (n >= sp.groupMin)
                {
                    foreach (int size in sizes)
                    {
                        Assert.That(size, Is.GreaterThanOrEqualTo(sp.groupMin), $"n {n}");
                        Assert.That(size, Is.LessThanOrEqualTo(sp.groupMax), $"n {n}");
                    }
                }
            }
        }

        [Test]
        public void WaveGroupsKeepsTheWaveMix()
        {
            for (int wave = 1; wave <= 12; wave++)
            {
                var flat = SpawnMath.WaveGroups(wave, 99, sp).SelectMany(g => g).ToList();
                int drones = flat.Count(k => k == "drone");
                int husks = flat.Count(k => k == "husk");
                Assert.That(husks * sp.huskCost + drones * sp.droneCost, Is.EqualTo(SpawnMath.WaveBudget(wave, sp)), $"wave {wave}");
            }
        }

        // --- spawn points ------------------------------------------------------------------------------------------

        static readonly List<Vec3> Points = new List<Vec3> { new Vec3(0, 0, 10), new Vec3(0, 0, 30), new Vec3(40, 0, 0) };

        [Test]
        public void OnlyPicksPointsAtLeast25mFromThePlayer()
        {
            Func<double> rng = Rng(3);
            for (int i = 0; i < 50; i++)
            {
                int k = SpawnMath.PickSpawnPoint(Points, new Vec3(0, 0, 0), cfg.Num("spawner.minPlayerDistance"), rng);
                Vec3 p = Points[k];
                Assert.That(JsMath.Hypot(p.x, p.z), Is.GreaterThanOrEqualTo(cfg.Num("spawner.minPlayerDistance")));
            }
        }

        [Test]
        public void FallsBackToTheFarthestPointWhenNoneIsFarEnough()
        {
            Assert.That(SpawnMath.PickSpawnPoint(Points, new Vec3(0, 0, 20), 100, Rng(1)), Is.EqualTo(2)); // points[2]
        }

        // --- stuck rule (stuckRule) --------------------------------------------------------------------------------

        static double Dt => 1 / cfg.Num("loop.simHz");

        static int Steps(double seconds) => (int)JsMath.Round(seconds / Dt);

        [Test]
        public void AnEnemyThatStaysWithin1mAndDealsNoDamageFor12sIsStuck()
        {
            double dt = Dt, stuckMove = cfg.Num("spawner.stuckMove"), stuckTime = cfg.Num("spawner.stuckTime");
            var track = new StuckTrack();
            int stuckAt = -1;
            for (int i = 0; i < Steps(stuckTime + 1); i++)
            {
                var wobble = new Vec3(0.4 * JsMath.Sin(i * 0.1), 0, 0);
                if (SpawnMath.StuckStep(track, wobble, false, dt, stuckMove, stuckTime))
                {
                    stuckAt = i;
                    break;
                }
            }
            Assert.That(stuckAt, Is.GreaterThan(0));
            Expect.CloseTo(stuckAt * dt, stuckTime, 1);
        }

        [Test]
        public void MovingOver1mOrDealingDamageResetsTheTimer()
        {
            double dt = Dt, stuckMove = cfg.Num("spawner.stuckMove"), stuckTime = cfg.Num("spawner.stuckTime");
            var track = new StuckTrack();
            for (int i = 0; i < Steps(stuckTime - 1); i++) SpawnMath.StuckStep(track, new Vec3(0, 0, 0), false, dt, stuckMove, stuckTime);
            Assert.That(SpawnMath.StuckStep(track, new Vec3(1.5, 0, 0), false, dt, stuckMove, stuckTime), Is.False);
            Expect.Exactly(track.time, 0);
            for (int i = 0; i < Steps(stuckTime - 1); i++) SpawnMath.StuckStep(track, new Vec3(1.5, 0, 0), false, dt, stuckMove, stuckTime);
            SpawnMath.StuckStep(track, new Vec3(1.5, 0, 0), true, dt, stuckMove, stuckTime);
            Expect.Exactly(track.time, 0);
        }
    }
}
