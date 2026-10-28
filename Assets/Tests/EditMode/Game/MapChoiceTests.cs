// NIGHT15 U3-28.5: choosing a map. design.maps.list names the maps TITLE offers; each listed file loads into a fresh
// headless composition and plays a run; the records are keyed by mode and map ('survival' on the arena, where they
// always were); the arena stays the first map and its fingerprint stays the baseline.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class MapChoiceTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig Cfg() => GameConfig.Load(Path.Combine(Root, "config.json"));

        [Test]
        public void TheList_StartsWithTheArena_AndNamesDrydock()
        {
            List<SurvivalGame.MapEntry> maps = SurvivalGame.MapList(Cfg(), Root);
            Assert.That(maps.Count, Is.GreaterThanOrEqualTo(2), "the arena and the new map (Design/maps/drydock.json must exist)");
            Assert.That(maps[0].id, Is.EqualTo(RunSetup.DefaultMap));
            Assert.That(maps[0].label, Is.EqualTo("ARENA"));
            Assert.That(maps.Exists(m => m.id == "drydock" && m.label == "DRYDOCK"), Is.True);
        }

        // NIGHT39 V (C1 row 9): one test case per listed map, split from EveryListedMap_LoadsAndPlays600Steps (one test over every map),
        // which ran past the Test Framework's 180 s default timeout. Measured: `n39views maptime` (Tools/SimHarness, CoreCLR) runs the
        // 48 listed maps in 46.2 s, 35.0 s of it composing the headless sims (the collision worlds), slowest n35-kiln 2.4 s; the suite's
        // Mono run of the one test took 104 s (n34), 168 s (n36), 152 s (n37) and failed at 262 s (n35) and 247 s (n38, six n38 maps
        // added, 7.6 s on CoreCLR): about 3.9x CoreCLR when the box is quiet, so 46 s of maps is ~180 s before any load. A case per map
        // keeps each under the timeout (~2.4 s x 3.9, twice that under load) as the list grows, and a failure names its map. The body is
        // the old one; the config is loaded once and shared, as the old test shared it across the maps.
        static GameConfig shared;

        static GameConfig SharedCfg() => shared ??= Cfg();

        static IEnumerable<string> ListedMaps()
        {
            foreach (SurvivalGame.MapEntry m in SurvivalGame.MapList(SharedCfg(), Root)) yield return m.id;
        }

        [TestCaseSource(nameof(ListedMaps))]
        public void ListedMap_LoadsAndPlays600Steps(string id)
        {
            GameConfig cfg = SharedCfg();
            List<SurvivalGame.MapEntry> maps = SurvivalGame.MapList(cfg, Root);
            Assert.That(maps.Count, Is.GreaterThanOrEqualTo(2), "both maps");
            int at = maps.FindIndex(e => e.id == id);
            Assert.That(at, Is.GreaterThanOrEqualTo(0), $"{id} is listed");
            SurvivalGame.MapEntry m = maps[at];
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(m.file)));
            Assert.That(map.name, Is.EqualTo(m.id), m.file);
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noLab = true; // the headless store has no LAB rows (as the AUTOPILOT and the fingerprint run)
            sim.metaSilent = true; // no profile, records or contracts here
            sim.StartRun(77u, null, null);
            for (int i = 0; i < 600; i++) sim.Step();
            Assert.That(sim.flow, Is.EqualTo(Flow.Playing), $"{m.id}: a run plays");
            Assert.That(sim.RecordsKey, Is.EqualTo(m.id == RunSetup.DefaultMap ? "survival" : "survival@" + m.id));
        }

        [Test]
        public void TheRecordsKey_IsTheArenasOldKey_OrModeAtMap()
        {
            Assert.That(SurvivalSim.RecordsKeyFor("survival"), Is.EqualTo("survival"));
            Assert.That(SurvivalSim.RecordsKeyFor(null), Is.EqualTo("survival"));
            Assert.That(SurvivalSim.RecordsKeyFor("drydock"), Is.EqualTo("survival@drydock"));
        }
    }
}
