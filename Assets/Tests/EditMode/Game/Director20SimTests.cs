// NIGHT20 U3-34.6 step 5 (builder E owns this file; tests first): the director's side of "enemies reach every floor"
// (design.director.n20; Core/Enemies/Director20.cs), on the Survival map's south-east spire top (40, 24, -40), a perch no
// walker from the floor reaches (Research/2026-10-06-enemy-routes.md 1.4). Measured in the worktree (NIGHT19's director):
// Zones samples 16 "ground" spawn nodes at y 0 inside the spire's box, the field routes them to you, and the round's
// squads were telegraphed in there.
//   1. the squads still spawn, never inside the spire (the block); off: NIGHT19 telegraphs into the box
//   2. the camp score notices height: on a perch no ground spawn reaches, the score starts after campHighStart (not the
//      web's lingerTime) and rises over campHighRise, and the HUNTERS spawn up there (a high node routed to you)
//   3. a walker that can't reach you, on screen for recycleAfter s, is recycled out of sight with its HP kept
//      (`enemyRecycle`, then a telegraph that lands it with the same HP); off: it stays
//   4. the web's highKinds (director.js spawnSquad): a squad of DRONEs spawns on a high node; off: NIGHT19's ground node
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Director20SimTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        static GameConfig cfg;
        static GameConfig Cfg => cfg ??= GameConfig.Load(Path.Combine(Root, "config.json"));

        static SurvivalMap map;
        static SurvivalMap Map() => map ??= SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));

        static readonly Vec3 Spire = new Vec3(40, 24, -40);

        static double N(string k) => Cfg.Num("design.director.n20." + k);

        static SurvivalSim Run(bool directorBlock)
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg, Map());
            sim.noRecords = sim.noLab = true;
            sim.n20Override = new N20Blocks { director = directorBlock };
            sim.StartRun(1, null, null);
            sim.zones.OpenAll();
            return sim;
        }

        static void Steps(SurvivalSim sim, double seconds)
        {
            int n = MovementMath.StepsFor(seconds, sim.cfg.Dt);
            for (int i = 0; i < n; i++) sim.Step();
        }

        static double YawDegTo(Vec3 from, Vec3 to) => Math.Atan2(-(to.x - from.x), -(to.z - from.z)) * 180 / Math.PI;

        /// <summary>The player on the spire top (its middle, or `at`) facing `look`, the round clock held while the nav field settles.</summary>
        static void Perch(SurvivalSim sim, Vec3 look, Vec3? at = null)
        {
            Vec3 spot = at ?? Spire;
            sim.holdRunClock = true;
            sim.Respawn(spot, YawDegTo(spot, look));
            Steps(sim, 1.5);
            Assert.That(sim.mover.position.y, Is.GreaterThan(20), "the player stands on the spire top");
        }

        /// <summary>A fight with no squads queued: the director's camp and recycle rules run, nothing else spawns.</summary>
        static void Fight(SurvivalSim sim, int round)
        {
            sim.director.StartWave(round, 7);
            sim.director.Clear();
            sim.run.state = SurvivalRun.State.Fight;
            sim.holdRunClock = false;
        }

        /// <summary>A husk on the floor; `brain` false keeps it out of the brain's watchdog (which relocates a stuck walker it
        /// judges off screen after 3 s), so a test sees the director's own failsafe.</summary>
        static Husk Ground(SurvivalSim sim, Vec3 at, bool brain = true)
        {
            Husk h = sim.husks.Spawn("husk", at, sim.mover.position);
            Assert.That(h, Is.Not.Null);
            if (brain) sim.brain.Attach(h);
            return h;
        }

        /// <summary>Inside the SE spire's box (x 32..48, z -48..-32, y 0..24): where Zones samples "ground" nodes under its top.</summary>
        static bool InsideSpire(Vec3 p) => p.x > 32 && p.x < 48 && p.z > -48 && p.z < -32 && p.y < 23;

        [Test]
        public void OnTheSpire_TheSquadsSpawn_NeverInsideIt()
        {
            foreach (bool on in new[] { false, true })
            {
                SurvivalSim sim = Run(on);
                Perch(sim, new Vec3(0, 0, 0));
                sim.director.StartWave(3, 7); // the opening squads
                Assert.That(sim.director.telegraphs.Count, Is.GreaterThan(0), "the round's squads spawn (no dead perch)");
                int inside = 0;
                foreach (Telegraph t in sim.director.telegraphs) if (InsideSpire(t.point)) inside++;
                if (on) Assert.That(inside, Is.EqualTo(0), "no squad inside the spire's box");
                else Assert.That(inside, Is.GreaterThan(0), "NIGHT19: the routed 'ground' nodes under the spire's top (the bug the block fixes)");
            }
        }

        [Test]
        public void OnAPerch_TheCampScoreStartsAfterCampHighStart_AndTheHuntersComeUp()
        {
            double exposedAt = Cfg.Num("director.camp.exposedAt");
            double toExposed = N("campHighStart") + N("campHighRise") * exposedAt + 0.5;
            double toHunted = N("campHighStart") + N("campHighRise") + 0.5;
            Assert.That(toHunted, Is.LessThan(Cfg.Num("director.camp.lingerTime")), "faster than the web's linger alone");
            foreach (bool on in new[] { false, true })
            {
                SurvivalSim sim = Run(on);
                Perch(sim, new Vec3(0, 0, 0));
                Fight(sim, 5);
                sim.husks.frozen = true;
                Ground(sim, new Vec3(0, 0, 30)); // an enemy alive (the camp rule's), far below
                var hunters = new List<string>();
                sim.events.On("hunters", o => hunters.Add(o as string));
                Steps(sim, toExposed);
                Assert.That(sim.director.Perched, Is.EqualTo(on), "the spire top is a perch no ground spawn reaches");
                Assert.That(sim.director.Exposed, Is.EqualTo(on), on ? "EXPOSED after campHighStart + campHighRise x exposedAt" : "the web's 8 s linger: not yet");
                Steps(sim, toHunted - toExposed);
                Assert.That(sim.director.Hunted, Is.EqualTo(on));
                if (!on) continue;
                Assert.That(hunters.Count, Is.EqualTo(1), "the HUNTERS came");
                bool up = false;
                foreach (Telegraph t in sim.director.telegraphs) up |= t.point.y > 20;
                Assert.That(up, Is.True, "the hunter squad spawns up there (the spire's high node, routed to you)");
            }
        }

        [Test]
        public void AWalkerThatCantReachYou_OnScreen_IsRecycledWithItsHp()
        {
            // the spire's west edge (the top spans x 32..48), looking down the clear lane at z = -46 to a husk 21 m out
            var edge = new Vec3(33, 24, -46);
            var at = new Vec3(12, 0, -46);
            foreach (bool on in new[] { false, true })
            {
                SurvivalSim sim = Run(on);
                Perch(sim, at, edge);
                Fight(sim, 5);
                sim.husks.frozen = true; // it stays in view
                Husk h = Ground(sim, at, false);
                h.hp = 30;
                int id = h.id;
                var recycled = new List<int>();
                sim.events.On<SpawnEvent>(Director.RecycleEvent, e => recycled.Add(e.id));
                Steps(sim, N("recycleAfter") - 0.5);
                Assert.That(h.alive, Is.True, "not before recycleAfter");
                Steps(sim, 1);
                if (!on)
                {
                    Assert.That(h.alive && h.id == id, Is.True, "NIGHT19: it stays below the perch");
                    Assert.That(recycled, Is.Empty);
                    continue;
                }
                Assert.That(recycled, Is.EqualTo(new[] { id }), "enemyRecycle with its id");
                Assert.That(h.alive && h.id == id, Is.False, "gone from where it stood");
                sim.husks.frozen = false;
                Steps(sim, Cfg.Num("spawner.telegraphTime") + 0.2);
                bool back = false;
                foreach (Husk o in sim.husks.PoolOf("husk")) back |= o.alive && o.hp == 30;
                Assert.That(back, Is.True, "back out of sight with its HP kept");
                Assert.That(sim.director.Remaining, Is.GreaterThan(0), "the round still counts it");
            }
        }

        [Test]
        public void TheWebsHighKinds_ADroneSquad_SpawnsOnAHighNode()
        {
            foreach (bool on in new[] { false, true })
            {
                SurvivalSim sim = Run(on);
                sim.n19Override = N19Blocks.FromBits(0xff); // the DRONE's block (design.enemies.n19) on
                sim.StartRun(1, null, null);
                sim.zones.OpenAll();
                sim.holdRunClock = true;
                Steps(sim, 0.5);
                Fight(sim, 6);
                sim.director.AddSquads(new List<List<string>> { new List<string> { "drone", "drone" } });
                Steps(sim, Cfg.Num("director.squadInterval") + 0.2);
                Telegraph drone = null;
                foreach (Telegraph t in sim.director.telegraphs) if (t.kinds[0] == "drone") drone = t;
                Assert.That(drone, Is.Not.Null, "the drone squad telegraphed");
                double highMin = Cfg.Num("zones.highNodeMin");
                if (on) Assert.That(drone.point.y, Is.GreaterThanOrEqualTo(highMin), "director.highKinds: a high node");
                else Assert.That(drone.point.y, Is.LessThan(highMin), "NIGHT19: a ground node");
            }
        }

        [Test]
        public void ADroneFarAndNotClosing_IsRecycled_OnAHighNode_WithItsHp()
        {
            // NIGHT20 (builder EN; THE FARM's stall, P2: seed 21 round 6): a drone further than droneFar that doesn't close
            // droneGain m for recycleAfter s, on screen or off (a flier has no watchdog), comes back on a high node with its HP
            double highMin = Cfg.Num("zones.highNodeMin");
            foreach (bool on in new[] { false, true })
            {
                SurvivalSim sim = Run(on);
                sim.n19Override = N19Blocks.FromBits(0xff); // the DRONE's block (design.enemies.n19) on
                sim.StartRun(1, null, null);
                sim.zones.OpenAll();
                sim.holdRunClock = true;
                Steps(sim, 0.5);
                Fight(sim, 6);
                sim.husks.frozen = true; // it stays where it is (as the pinned drones did)
                Vec3 p = sim.mover.position;
                Husk h = sim.husks.Spawn("drone", new Vec3(p.x + (p.x > 0 ? -1 : 1) * (N("droneFar") + 15), 0, p.z), p);
                Assert.That(h, Is.InstanceOf<Drone>());
                h.hp = 20;
                int id = h.id;
                var recycled = new List<int>();
                sim.events.On<SpawnEvent>(Director.RecycleEvent, e => recycled.Add(e.id));
                Steps(sim, N("recycleAfter") - 0.5);
                Assert.That(h.alive, Is.True, "not before recycleAfter");
                Steps(sim, 1);
                if (!on)
                {
                    Assert.That(h.alive && h.id == id, Is.True, "NIGHT19: it stays out there");
                    Assert.That(recycled, Is.Empty);
                    continue;
                }
                Assert.That(recycled, Is.EqualTo(new[] { id }), "enemyRecycle with its id");
                Telegraph back = null;
                foreach (Telegraph t in sim.director.telegraphs) if (t.kinds[0] == "drone") back = t;
                Assert.That(back, Is.Not.Null, "telegraphed back");
                Assert.That(back.point.y, Is.GreaterThanOrEqualTo(highMin), "on a high node (director.highKinds)");
                Assert.That(back.hps[0], Is.EqualTo(20), "with its HP");
            }
        }
    }
}
