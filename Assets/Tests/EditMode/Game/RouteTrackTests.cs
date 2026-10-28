// NIGHT14 U3-27.10 (agent V2): THE ROUTE's track (Game/Route/RouteTrack.cs) on a headless composition. The track records
// a point every design.route.every s of run time (whole steps: 30 at 120 Hz) from the spawn, the player's kills where they
// fell, each round's start (its run step and place), the DOWN and the exfil; preallocated for design.route.maxMinutes;
// reset at each run's start (SurvivalSim.RunStarted); nothing allocated per step; once per ended run it raises Ended.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class RouteTrackTests
    {
        static readonly string Root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PortExport"));

        internal static SurvivalSim RouteHeadless(out GameConfig cfg, out SurvivalMap map)
        {
            cfg = GameConfig.Load(Path.Combine(Root, "config.json"));
            map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));
            return SurvivalSim.Headless(cfg, map);
        }

        /// <summary>A plain run on `seed`: the player at 1e6 HP, no LAB, the fingerprint's script for `steps` steps.</summary>
        internal static void RouteScriptedRun(SurvivalSim sim, uint seed, int steps, bool god = true)
        {
            var script = new ScriptedInput();
            script.SetLook(sim.input.Yaw, sim.input.Pitch);
            sim.UseInput(script);
            sim.noLab = true;
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(seed, null, null);
            if (god) sim.health.max = sim.health.hp = 1e6;
            for (int i = 0; i < steps; i++)
            {
                double yaw = script.Yaw, pitch = script.Pitch;
                SimFingerprint.Script(i, sim.Eye, sim.enemies, ref yaw, ref pitch, out InputFrame move, out CombatInput combat);
                script.SetLook(yaw, pitch);
                script.Set(move, combat);
                sim.Step();
            }
        }

        [Test]
        public void Points_EveryQuarterSecond_FromTheSpawn()
        {
            SurvivalSim sim = RouteHeadless(out GameConfig cfg, out SurvivalMap map);
            var track = new RouteTrack(sim);
            Assert.That(track.enabled, Is.True, "design.route.enabled");
            Assert.That(track.everySteps, Is.EqualTo(MovementMath.StepsFor(cfg.Num("design.route.every"), sim.step)));
            RouteScriptedRun(sim, 1, 1200);
            Assert.That(sim.run.steps, Is.EqualTo(1200), "the run's clock advanced every step");
            Assert.That(track.startStep, Is.EqualTo(0));
            Assert.That(track.count, Is.EqualTo(1200 / track.everySteps + 1), "point 0 at the spawn, then one per everySteps");
            Assert.That(track.px[0], Is.EqualTo((float)map.playerSpawn.x));
            Assert.That(track.pz[0], Is.EqualTo((float)map.playerSpawn.z));
            Assert.That(track.seed, Is.EqualTo(1u));
            Assert.That(track.resumed, Is.False);
            Assert.That(track.full, Is.False);
        }

        [Test]
        public void Capacity_IsMaxMinutes()
        {
            SurvivalSim sim = RouteHeadless(out GameConfig cfg, out _);
            var track = new RouteTrack(sim);
            double every = cfg.Num("design.route.every"), minutes = cfg.Num("design.route.maxMinutes");
            int expected = (int)Math.Ceiling(minutes * 60 / every) + 1;
            Assert.That(track.capacity, Is.EqualTo(expected));
            Assert.That(track.px.Length, Is.EqualTo(expected));
            Assert.That(track.speed.Length, Is.EqualTo(expected));
            // A short track fills up and stops (no wrap, no growth).
            var small = new RouteTrack(sim, true, every, 1.0 / 60); // one second: 4 points after the first
            RouteScriptedRun(sim, 1, 600);
            Assert.That(small.capacity, Is.EqualTo(5));
            Assert.That(small.count, Is.EqualTo(5));
            Assert.That(small.full, Is.True);
        }

        [Test]
        public void Kills_RoundStarts_AndTopGun()
        {
            SurvivalSim sim = RouteHeadless(out GameConfig cfg, out _);
            var track = new RouteTrack(sim);
            RouteScriptedRun(sim, 1, SimFingerprint.Steps);
            Assert.That(sim.run.stats.kills, Is.GreaterThan(0), "the script kills");
            Assert.That(track.kills, Is.EqualTo(sim.run.stats.kills), "every player kill of the live run");
            Assert.That(track.killCount, Is.EqualTo(Math.Min(track.kills, track.killX.Length)));
            Assert.That(track.roundCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(track.roundNumber[0], Is.EqualTo(1));
            Assert.That(track.roundStep[0], Is.EqualTo(MovementMath.StepsFor(cfg.Num("survival.firstBreakTime"), sim.step)), "round 1 begins after the first break");
            Assert.That(track.ReachedRound, Is.EqualTo(sim.run.state == SurvivalRun.State.Fight ? sim.run.round : track.roundNumber[track.roundCount - 1]));
            string gun = track.TopGun(out int gunKills);
            Assert.That(gun, Is.Not.Null);
            Assert.That(gunKills, Is.GreaterThan(0));
            Assert.That(gunKills, Is.LessThanOrEqualTo(track.kills));
        }

        [Test]
        public void NewRun_ResetsTheTrack()
        {
            SurvivalSim sim = RouteHeadless(out _, out _);
            var track = new RouteTrack(sim);
            RouteScriptedRun(sim, 1, SimFingerprint.Steps);
            int version = track.Version;
            Assert.That(track.count, Is.GreaterThan(1));
            RouteScriptedRun(sim, 2, 0);
            Assert.That(track.Version, Is.GreaterThan(version));
            Assert.That(track.count, Is.EqualTo(1), "only the spawn point");
            Assert.That(track.killCount, Is.EqualTo(0));
            Assert.That(track.kills, Is.EqualTo(0));
            Assert.That(track.roundCount, Is.EqualTo(0));
            Assert.That(track.down, Is.False);
            Assert.That(track.TopGun(out _), Is.Null);
            Assert.That(track.seed, Is.EqualTo(2u));
        }

        [Test]
        public void Down_IsMarkedWhereTheRunDied_AndEndedRaisedOnce()
        {
            SurvivalSim sim = RouteHeadless(out _, out _);
            var track = new RouteTrack(sim);
            var ended = new List<RunStats>();
            track.Ended += s => ended.Add(s);
            RouteScriptedRun(sim, 3, 400, god: false);
            for (int i = 0; i < 240 && !sim.health.Dead; i++)
            {
                sim.health.Damage(sim.health.max * 10, "fall"); // refused while dashing or shielded: try again a step later
                if (!sim.health.Dead) sim.Step();
            }
            Assert.That(sim.health.Dead, Is.True);
            Vec3 at = sim.mover.position; // where the run died
            sim.Step();
            Assert.That(sim.flow, Is.EqualTo(Flow.Down));
            Assert.That(track.down, Is.True);
            Assert.That(track.downX, Is.EqualTo((float)at.x));
            Assert.That(track.downZ, Is.EqualTo((float)at.z));
            for (int i = 0; i < 2000 && sim.flow != Flow.Results; i++) sim.Step();
            Assert.That(sim.flow, Is.EqualTo(Flow.Results));
            Assert.That(ended.Count, Is.EqualTo(1));
            Assert.That(ended[0], Is.SameAs(sim.lastStats));
            for (int i = 0; i < 10; i++) sim.Step();
            track.CheckEnded();
            Assert.That(ended.Count, Is.EqualTo(1), "once per ended run");
        }

        [Test]
        public void EndedBeforeTheNextRun_WhenTheRunEndsOutsideAStep()
        {
            // R from DOWN / PAUSED's RESTART: EndRun then StartRun in one call; the track reports the ended run before it resets.
            SurvivalSim sim = RouteHeadless(out _, out _);
            var track = new RouteTrack(sim);
            int countAtEnd = -1;
            track.Ended += s => countAtEnd = track.count;
            RouteScriptedRun(sim, 4, 600);
            int count = track.count;
            sim.EndRun();
            sim.StartRun(5, null, null);
            Assert.That(countAtEnd, Is.EqualTo(count), "Ended saw the ended run's track");
            Assert.That(track.count, Is.EqualTo(1));
        }

        [Test]
        public void FlagOff_RecordsNothing()
        {
            SurvivalSim sim = RouteHeadless(out GameConfig cfg, out _);
            var track = new RouteTrack(sim, false, cfg.Num("design.route.every"), cfg.Num("design.route.maxMinutes"));
            RouteScriptedRun(sim, 1, 600);
            Assert.That(track.count, Is.EqualTo(0));
            Assert.That(track.kills, Is.EqualTo(0));
            Assert.That(track.roundCount, Is.EqualTo(0));
        }

        [Test]
        public void Record_AllocatesNothing()
        {
            SurvivalSim sim = RouteHeadless(out _, out _);
            var track = new RouteTrack(sim);
            RouteScriptedRun(sim, 1, 300);
            track.Record(); // warm
            int points = track.count;
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 10000; i++)
            {
                sim.run.steps++; // the run's clock ticks: a point every everySteps
                track.Record();
            }
            long after = GC.GetAllocatedBytesForCurrentThread();
            Assert.That(after - before, Is.EqualTo(0));
            Assert.That(track.count - points, Is.EqualTo(10000 / track.everySteps), "the points were taken");
        }
    }
}
