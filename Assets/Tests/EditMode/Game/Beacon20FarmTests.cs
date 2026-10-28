// NIGHT20 A3 (builder P2): THE BEACON in THE FARM (Game/Bot/Farm.cs, AutopilotShopper.cs): the bot's coin (beaconShare of
// the standing beacons, a pure function of the run seed and the round), a farm run whose bot takes every beacon (it walks
// to it, holds BUY, the next round gets its elite) and the JSONL line's `beacons` rows (farm-report.cjs's beacon table).
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Beacon20FarmTests
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

        [Test]
        public void TheBotsCoin_TakesItsShare_TheSameEachTime()
        {
            int taken = 0, n = 0;
            for (uint seed = 1; seed <= 40; seed++)
            {
                for (int r = 4; r <= 23; r++)
                {
                    n++;
                    bool c = AutopilotShopper.TakesBeacon(seed, r, 0.5);
                    if (c) taken++;
                    Assert.That(AutopilotShopper.TakesBeacon(seed, r, 0.5), Is.EqualTo(c), "a pure function");
                    Assert.That(AutopilotShopper.TakesBeacon(seed, r, 0), Is.False, "share 0: never");
                    Assert.That(AutopilotShopper.TakesBeacon(seed, r, 1), Is.True, "share 1: always");
                }
            }
            Assert.That((double)taken / n, Is.EqualTo(0.5).Within(0.05), $"{taken} of {n}");
        }

        [Test]
        public void AFarmRun_TakingEveryBeacon_CallsItsElite_AndWritesItsRows()
        {
            Assert.That(N20Blocks.From(Cfg()).Add(Cfg(), "beacon"), Is.True, "the row and the adds bit on (the farm plays tonight's game)");
            Farm.Record r = Farm.RunOne(Cfg(), Map(), 1, "normal", 20 * 60, beaconShare: 1);
            Assert.That(r.beacons.Count, Is.GreaterThanOrEqualTo(1), "seed 1 stands a beacon before it ends");
            Farm.BeaconRow first = r.beacons[0];
            Assert.That(first.taken, Is.True, "share 1: the bot walked to it and held BUY");
            Assert.That(first.elites, Is.EqualTo(Beacon20.P.From(Cfg()).elites), "its round called the elite");
            string json = r.ToJson();
            StringAssert.Contains("\"beacons\":[{\"round\":" + first.round + ",\"taken\":true", json);
        }
    }
}
