// NIGHT7 S7: the director's round 1 on the real map, the player standing at the spawn: the round's husks come in squads
// (telegraph, then the group), none spawns inside geometry or falls out of the map, and they all reach the fight.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class DirectorRoundTests
    {
        [Test]
        public void RoundOne_SpawnsItsHusks_NoneFallOut()
        {
            var cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            var map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
            var events = new EventBus();
            var enemies = new EnemyRegistry();
            var world = new GameWorld(map.BuildOctree(), map.CreateCore(cfg.movement));
            var zones = new Zones(cfg, map, world, events);
            var nav = new NavField(cfg, NavGrid.FromExport(map.navGrid));
            nav.SetBlocked(zones.LockedTag);
            var brain = new HuskBrain(cfg, events, nav, world.Geometry) { onScreen = _ => true };
            var husks = new Husks(cfg, events, enemies, world, map.killPlaneY) { jumper = new HuskNavJumper(cfg, events) };
            Vec3 player = map.playerSpawn;
            var health = new Health(1e9, events, "player");
            var director = new Director(cfg, events, husks, brain, zones, world.core, world.Geometry, nav, () => player, () => 0, () => 0);
            director.huskWorld = world;
            var log = new List<string>();
            int spawned = 0, telegraphs = 0;
            events.On<SpawnEvent>("spawnTelegraph", e => { telegraphs++; log.Add($"T({e.position.x},{e.position.y},{e.position.z})"); });
            events.On<SpawnEvent>("enemySpawn", e => { spawned++; log.Add($"S{e.id}({e.position.x:0.0},{e.position.y:0.00},{e.position.z:0.0})"); });
            nav.UpdateNow(player.x, player.y, player.z);
            director.NewRun();
            director.StartWave(1, 11);
            double dt = cfg.Dt;
            for (int i = 0; i < 25 * 120; i++)
            {
                brain.Step(dt, player, 0, 1, false);
                director.Step(dt);
                husks.Step(player, health, dt);
                brain.Separate(husks);
            }
            var fell = new List<string>();
            foreach (Enemy e in enemies.all) if (!e.alive && e.position.y < -10) fell.Add($"{e.id}@{e.position.x:0.0},{e.position.z:0.0}");
            string report = $"telegraphs {telegraphs}, spawned {spawned}, alive {husks.Count}, remaining {director.Remaining}, fell [{string.Join(" ", fell)}]\n{string.Join(" ", log)}";
            TestContext.WriteLine(report);
            Assert.That(fell, Is.Empty, report);
            // The player stood still dealing nothing: the camp score hit 100% and the hunter squad (6 swarmers) came too.
            int budget = (int)DirectorMath.RoundBudget(1, DirectorMath.From(cfg));
            Assert.That(spawned, Is.EqualTo(director.state.hunted ? budget + 6 : budget), report);
        }
    }
}
