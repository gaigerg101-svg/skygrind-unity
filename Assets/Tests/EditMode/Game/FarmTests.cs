// NIGHT14 U3-27.6: THE FARM (Game/Bot/Farm.cs), headless. Each run is a fresh composition (SurvivalSim.Headless), the
// AUTOPILOT at real HP, one JSONL line per run appended to the file (the report reads them: Tools/farm-report.cjs). A run
// that outlives design.farm.maxMinutes ends as "timeout". The same seed gives the same line (a pure function of seed,
// difficulty and build).
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class FarmTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap Map() => SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static Dictionary<string, object> Parse(string line) => PortData.Obj(JsJson.Parse(line));

        [Test]
        public void Run_WritesOneLinePerRun()
        {
            string path = Path.Combine(Path.GetTempPath(), $"skygrind-farm-test-{System.Guid.NewGuid():N}.jsonl");
            var log = new List<string>();
            try
            {
                string summary = Farm.Run(Cfg(), Map(), 2, "normal", 11, path, log.Add, 1.0);
                string[] lines = File.ReadAllLines(path);
                Assert.That(lines.Length, Is.EqualTo(2), summary);
                Assert.That(log.Count, Is.GreaterThanOrEqualTo(2), "a progress line per run");
                for (int i = 0; i < lines.Length; i++)
                {
                    Dictionary<string, object> o = Parse(lines[i]);
                    Assert.That(PortData.Num(o["seed"]), Is.EqualTo(11 + i));
                    Assert.That((string)o["difficulty"], Is.EqualTo("normal"));
                    Assert.That(o.ContainsKey("build"), Is.True);
                    Assert.That(PortData.Num(o["round"]), Is.GreaterThanOrEqualTo(1));
                    Assert.That(PortData.Num(o["simTime"]), Is.GreaterThan(0).And.LessThanOrEqualTo(60.01));
                    Assert.That(o["cause"], Is.InstanceOf<string>());
                    foreach (string key in new[] { "earnedByRound", "spentByRound", "guns", "killsByGun", "perks", "mods", "forge", "rankSteps", "downs", "exfil" })
                        Assert.That(o.ContainsKey(key), Is.True, key);
                }
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Test]
        public void AShortCap_EndsAsTimeout()
        {
            Farm.Record r = Farm.RunOne(Cfg(), Map(), 21, "normal", 20);
            Assert.That(r.cause, Is.EqualTo("timeout"), "20 s of sim time: alive at the cap");
            Assert.That(r.simTime, Is.EqualTo(20).Within(0.01));
            Assert.That(r.ToJson(), Does.Not.Contain("\n"), "one line");
        }

        [Test]
        public void SameSeed_SameLine()
        {
            string a = Farm.RunOne(Cfg(), Map(), 31, "hard", 90).ToJson();
            string b = Farm.RunOne(Cfg(), Map(), 31, "hard", 90).ToJson();
            Assert.That(b, Is.EqualTo(a));
        }

        [Test]
        public void TheLine_CountsWhatTheRunDid()
        {
            Farm.Record r = Farm.RunOne(Cfg(), Map(), 41, "normal", 120);
            Dictionary<string, object> o = Parse(r.ToJson());
            double kills = 0;
            foreach (var kv in PortData.Obj(o["killsByGun"])) kills += PortData.Num(kv.Value);
            Assert.That(kills, Is.EqualTo(r.kills), "kills by gun add up to the run's kills");
            double rankSteps = 0;
            foreach (object n in PortData.Arr(o["rankSteps"])) rankSteps += PortData.Num(n);
            Assert.That(rankSteps, Is.EqualTo(r.steps), "every step is in one style rank");
            Assert.That(PortData.Arr(o["earnedByRound"]).Count, Is.EqualTo(r.round), "points earned per round reached");
        }
    }
}
