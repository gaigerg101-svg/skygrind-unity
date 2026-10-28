// NIGHT60 builder V (B3 step a, the probe reports the swarmers and the spitters): FeelProbe.Enemy drops every report past
// UnityTuning.ProbeMaxEnemies in a frame, silently (FeelProbe.cs: `enemyCount >= enemyId.Length` returns). At 8, NIGHT59's kill reels a258 / a355
// filled it on 94 frames each (Logs/feel/n59-b2/*/telemetry.jsonl), and the views report in render order (husks, swarmers, spitters, then the
// N19 / N20 / boss kinds), so the new swarmer and spitter reports would push the later kinds out first. Pinned: the cap holds every enemy the
// director keeps alive (director.maxAlive). Not pinned here: that the two views call FeelProbe.Enemy (the views need a live SurvivalGame with
// EnemyLook's materials and a recording reel; the Player reel's telemetry is that proof: the a358 a5-entry frame's enemies[] gains the spitter).
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class N60ProbeTests
    {
        static string PortRoot()
        {
            try { return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport")); }
            catch (System.Exception) { return Path.GetFullPath("PortExport"); }
        }

        static GameConfig cfg;
        static GameConfig Cfg() => cfg ??= GameConfig.Load(Path.Combine(PortRoot(), "config.json"));

        [Test]
        public void ProbeCap_HoldsEveryEnemyTheDirectorKeepsAlive()
        {
            int alive = (int)Cfg().Num("director.maxAlive");
            Assert.That(alive, Is.GreaterThan(0));
            Assert.That(UnityTuning.ProbeMaxEnemies, Is.GreaterThanOrEqualTo(alive), "a frame's reports past the cap are dropped without a trace");
        }
    }
}
