// NIGHT20 P0-3 (builder P; Research/2026-10-06-additions-backlog.md P0-3): THE FARM's pacing trace. Booth's curve (the
// director's BUILD_UP / PEAK / PEAK_FADE / RELAX, Core/Director.cs) exists in code but was never measured; the additions
// wave's rows (THE HUNT, THE ELITE, the boss rounds) are judged on it. Each farm line now carries one pace row per round
// reached: the round, its type (DirectorMath.RoundType: normal, hold, gold, swarm, boss; "exfil" for the round whose exfil
// the bot called), the break before it and its fight in seconds, the fight's seconds by director phase (they add up to
// the fight), the downs and the damage taken in it. Tools/farm-report.cjs prints the pacing table from them.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class FarmPaceTests
    {
        static string PortRoot()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); } // a plain .NET runner: the working folder is the project
        }

        static GameConfig cfg;
        static SurvivalMap map;

        static GameConfig Cfg() => cfg ??= GameConfig.Load(Path.Combine(PortRoot(), "config.json"));

        static SurvivalMap Map() => map ??= SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(PortRoot(), "maps", "survival.json"))));

        [TestCase(7u)]
        [TestCase(8u)]
        public void TheFarmLine_HasAPaceRowPerRoundReached_ItsTypeAndPhaseSecondsAddUp(uint seed)
        {
            Farm.Record r = Farm.RunOne(Cfg(), Map(), seed, "normal", 1200);
            DirectorMath.D d = DirectorMath.From(Cfg());
            Assert.That(r.pace.Count, Is.EqualTo(r.round), "one row per round reached");
            int downs = 0;
            double fight = 0;
            for (int i = 0; i < r.pace.Count; i++)
            {
                Farm.PaceRow p = r.pace[i];
                Assert.That(p.round, Is.EqualTo(i + 1));
                string want = r.exfilCalled == p.round ? "exfil" : DirectorMath.RoundType(p.round, d);
                Assert.That(p.type, Is.EqualTo(want), $"round {p.round}'s type");
                Assert.That(p.build + p.peak + p.fade + p.relax, Is.EqualTo(p.fight).Within(1e-6), $"round {p.round}: the phases add up to the fight");
                Assert.That(p.brk, Is.GreaterThan(0), $"round {p.round}: a break before it");
                if (i < r.pace.Count - 1) Assert.That(p.fight, Is.GreaterThan(0), $"round {p.round} was fought");
                Assert.That(p.hurt, Is.GreaterThanOrEqualTo(0));
                downs += p.downs;
                fight += p.fight + p.brk;
            }
            Assert.That(downs, Is.EqualTo(r.downs), "the downs land in their rounds");
            Assert.That(fight, Is.LessThanOrEqualTo(r.simTime + 1e-6), "the rows' seconds fit in the run");
            Assert.That(fight, Is.GreaterThan(r.simTime - 5), "and cover it (the first steps before the first break excepted)");
            if (r.round >= 6) Assert.That(r.pace[5].type, Is.EqualTo("hold"), "round 6 is the HOLD");
        }

        [Test]
        public void TheFarmLine_WritesThePace()
        {
            var r = new Farm.Record { seed = 1, difficulty = "normal", build = "test", cause = "HUSK" };
            r.pace.Add(new Farm.PaceRow { round = 1, type = "normal", brk = 3, fight = 20.5, build = 12, peak = 5, fade = 1.5, relax = 2, downs = 0, hurt = 17 });
            r.pace.Add(new Farm.PaceRow { round = 2, type = "hold", brk = 10, fight = 8, build = 8, downs = 1, hurt = 100 });
            Dictionary<string, object> o = PortData.Obj(JsJson.Parse(r.ToJson()));
            List<object> pace = PortData.Arr(o["pace"]);
            Assert.That(pace.Count, Is.EqualTo(2));
            Dictionary<string, object> p0 = PortData.Obj(pace[0]);
            Assert.That((string)p0["type"], Is.EqualTo("normal"));
            Assert.That((PortData.Num(p0["round"]), PortData.Num(p0["break"]), PortData.Num(p0["fight"])), Is.EqualTo((1.0, 3.0, 20.5)));
            Assert.That((PortData.Num(p0["build"]), PortData.Num(p0["peak"]), PortData.Num(p0["fade"]), PortData.Num(p0["relax"])), Is.EqualTo((12.0, 5.0, 1.5, 2.0)));
            Assert.That((PortData.Num(p0["downs"]), PortData.Num(p0["hurt"])), Is.EqualTo((0.0, 17.0)));
            Assert.That((string)PortData.Obj(pace[1])["type"], Is.EqualTo("hold"));
        }
    }
}
