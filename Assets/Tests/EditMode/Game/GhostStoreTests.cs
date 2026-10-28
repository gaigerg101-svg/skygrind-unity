// NIGHT14 U3-27.11 (agent V2): the GHOST's store and track (Game/Route/GhostStore.cs, GhostTrack.cs). A seed's best route
// (the higher round reached, else the same round reached sooner) is kept per seed in the profile's folder (ghosts/<seed
// code>.bin), at most design.ghost.maxSeeds files (the oldest dropped); a run that records nothing (metaSilent, LAB,
// automation without an override folder, a resumed run) stores nothing; the file round-trips exactly and refuses another
// format; the ghost is sampled by run step (interpolated) and splits are run steps against the best's round starts.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class GhostStoreTests
    {
        string folder;

        [SetUp]
        public void MakeFolder()
        {
            folder = Path.Combine(Path.GetTempPath(), "skygrind-ghost-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            GhostStore.FolderOverride = null;
        }

        [TearDown]
        public void DropFolder()
        {
            GhostStore.FolderOverride = null;
            try
            {
                if (Directory.Exists(folder)) Directory.Delete(folder, true);
            }
            catch (IOException)
            {
            }
        }

        /// <summary>A hand-made best: `points` points `every` steps apart along +x (1 m each), rounds begun at the given steps.</summary>
        static GhostTrack GhostMade(uint seed, int points, int every, params (int round, int step)[] rounds)
        {
            var g = new GhostTrack(points, rounds.Length)
            {
                seed = seed,
                heat = 0,
                everySteps = every,
                simHz = 120,
                startStep = 0,
                count = points,
                roundCount = rounds.Length,
            };
            for (int i = 0; i < points; i++)
            {
                g.px[i] = i;
                g.py[i] = 0.5f * i;
                g.pz[i] = -2f * i;
                g.speed[i] = 3 + i;
            }
            for (int i = 0; i < rounds.Length; i++)
            {
                g.roundNumber[i] = rounds[i].round;
                g.roundStep[i] = rounds[i].step;
            }
            return g;
        }

        [Test]
        public void RoundTrip_KeepsEveryPoint()
        {
            GhostTrack g = GhostMade(1234, 50, 30, (1, 600), (2, 3000), (3, 7000));
            g.savedTicks = 638000000000000000L;
            var ms = new MemoryStream();
            g.Write(ms);
            ms.Position = 0;
            GhostTrack back = GhostTrack.Read(ms);
            Assert.That(back, Is.Not.Null);
            Assert.That(back.seed, Is.EqualTo(1234u));
            Assert.That(back.savedTicks, Is.EqualTo(g.savedTicks));
            Assert.That(back.everySteps, Is.EqualTo(30));
            Assert.That(back.simHz, Is.EqualTo(120));
            Assert.That(back.count, Is.EqualTo(50));
            Assert.That(back.roundCount, Is.EqualTo(3));
            for (int i = 0; i < 50; i++)
            {
                Assert.That(back.px[i], Is.EqualTo(g.px[i]));
                Assert.That(back.py[i], Is.EqualTo(g.py[i]));
                Assert.That(back.pz[i], Is.EqualTo(g.pz[i]));
                Assert.That(back.speed[i], Is.EqualTo(g.speed[i]));
            }
            Assert.That(back.roundNumber[2], Is.EqualTo(3));
            Assert.That(back.roundStep[2], Is.EqualTo(7000));
            Assert.That(back.ReachedRound, Is.EqualTo(3));
            Assert.That(back.ReachedStep, Is.EqualTo(7000));
        }

        [Test]
        public void Read_RefusesAnotherFormat()
        {
            Assert.That(GhostTrack.Read(new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 })), Is.Null, "not a ghost");
            Assert.That(GhostTrack.Read(new MemoryStream(new byte[0])), Is.Null, "empty");
            var ms = new MemoryStream();
            var w = new BinaryWriter(ms);
            w.Write(GhostTrack.FileMagic);
            w.Write(GhostTrack.FileVersion + 1);
            w.Flush();
            ms.Position = 0;
            Assert.That(GhostTrack.Read(ms), Is.Null, "a later version");
            // A truncated file of this format: refused, never thrown.
            GhostTrack g = GhostMade(7, 20, 30, (1, 600));
            var full = new MemoryStream();
            g.Write(full);
            byte[] cut = new byte[full.Length / 2];
            Array.Copy(full.ToArray(), cut, cut.Length);
            Assert.That(GhostTrack.Read(new MemoryStream(cut)), Is.Null, "truncated");
        }

        [Test]
        public void Better_IsTheHigherRound_ThenTheSoonerStart()
        {
            GhostTrack r3 = GhostMade(1, 10, 30, (1, 600), (2, 2000), (3, 5000));
            Assert.That(GhostTrack.Better(r3, null), Is.True, "anything beats nothing");
            Assert.That(GhostTrack.Better(GhostMade(1, 10, 30, (1, 600), (2, 2000)), r3), Is.False, "a lower round");
            Assert.That(GhostTrack.Better(GhostMade(1, 10, 30, (1, 600), (2, 2000), (3, 4900)), r3), Is.True, "round 3 sooner");
            Assert.That(GhostTrack.Better(GhostMade(1, 10, 30, (1, 600), (2, 2000), (3, 5000)), r3), Is.False, "a tie keeps the stored best");
            Assert.That(GhostTrack.Better(GhostMade(1, 10, 30, (1, 600), (2, 2000), (3, 5100)), r3), Is.False, "round 3 later");
            Assert.That(GhostTrack.Better(GhostMade(1, 10, 30, (1, 600), (2, 2000), (3, 5000), (4, 9000)), r3), Is.True, "a higher round");
            Assert.That(GhostTrack.Better(GhostMade(1, 10, 30), null), Is.True, "no round begun still beats nothing");
            Assert.That(GhostTrack.Better(GhostMade(1, 10, 30), GhostMade(1, 4, 30)), Is.False, "no round begun never replaces a best");
        }

        [Test]
        public void Offer_KeepsTheSeedsBest()
        {
            var store = new GhostStore(folder, 50);
            Assert.That(store.Folder, Is.EqualTo(Path.Combine(folder, UnityTuning.GhostFolderName)));
            Assert.That(store.Load(9), Is.Null, "no best yet");
            Assert.That(store.Offer(GhostMade(9, 10, 30, (1, 600), (2, 2000), (3, 5000))), Is.True);
            Assert.That(File.Exists(store.PathFor(9)), Is.True);
            Assert.That(Path.GetFileName(store.PathFor(9)), Is.EqualTo(Seeds.SeedCode(9) + ".bin"));
            Assert.That(store.Offer(GhostMade(9, 10, 30, (1, 600), (2, 2000))), Is.False, "lower round");
            Assert.That(store.Offer(GhostMade(9, 12, 30, (1, 600), (2, 2000), (3, 4000))), Is.True, "same round sooner");
            Assert.That(store.Offer(GhostMade(9, 14, 30, (1, 600), (2, 2000), (3, 4500))), Is.False, "same round later");
            GhostTrack best = store.Load(9);
            Assert.That(best, Is.Not.Null);
            Assert.That(best.ReachedStep, Is.EqualTo(4000));
            Assert.That(best.count, Is.EqualTo(12));
            Assert.That(store.Load(10), Is.Null, "another seed");
        }

        [Test]
        public void Prune_DropsTheOldest()
        {
            var store = new GhostStore(folder, 3);
            long t = 0;
            store.clock = () => new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc).AddMinutes(++t);
            for (uint s = 1; s <= 4; s++) Assert.That(store.Offer(GhostMade(s, 5, 30, (1, 600))), Is.True);
            Assert.That(Directory.GetFiles(store.Folder, "*.bin").Length, Is.EqualTo(3));
            Assert.That(File.Exists(store.PathFor(1)), Is.False, "the oldest went");
            for (uint s = 2; s <= 4; s++) Assert.That(File.Exists(store.PathFor(s)), Is.True);
            // A better run on seed 2 makes it the newest: seed 3 goes next.
            Assert.That(store.Offer(GhostMade(2, 5, 30, (1, 600), (2, 900))), Is.True);
            Assert.That(store.Offer(GhostMade(5, 5, 30, (1, 600))), Is.True);
            Assert.That(File.Exists(store.PathFor(3)), Is.False);
            Assert.That(File.Exists(store.PathFor(2)), Is.True);
        }

        [Test]
        public void Override_FolderTakesTheGhosts()
        {
            string other = Path.Combine(folder, "smoke");
            GhostStore.FolderOverride = other;
            var store = new GhostStore(Path.Combine(folder, "profile"), 50);
            Assert.That(store.Folder, Is.EqualTo(other));
            Assert.That(store.Offer(GhostMade(11, 5, 30, (1, 600))), Is.True);
            Assert.That(File.Exists(Path.Combine(other, Seeds.SeedCode(11) + ".bin")), Is.True);
            Assert.That(Directory.Exists(Path.Combine(folder, "profile")), Is.False, "nothing in the profile's folder");
        }

        [Test]
        public void MetaRule_RunsThatRecordNothingStoreNothing()
        {
            Assert.That(GhostStore.MayStore(false, false, false, false, false), Is.True, "a plain run");
            Assert.That(GhostStore.MayStore(true, false, false, false, false), Is.False, "metaSilent (a bot's run)");
            Assert.That(GhostStore.MayStore(false, true, false, false, false), Is.False, "noRecords (automation)");
            Assert.That(GhostStore.MayStore(false, true, false, false, true), Is.True, "automation into its own folder");
            Assert.That(GhostStore.MayStore(true, true, false, false, true), Is.False, "a bot's run even with an override");
            Assert.That(GhostStore.MayStore(false, false, true, false, false), Is.False, "a LAB run");
            Assert.That(GhostStore.MayStore(false, false, false, true, false), Is.False, "a resumed run (its route is partial)");

            SurvivalSim sim = RouteTrackTests.RouteHeadless(out _, out _);
            var track = new RouteTrack(sim);
            RouteTrackTests.RouteScriptedRun(sim, 21, 60);
            Assert.That(GhostStore.MayStore(sim, track), Is.True);
            sim.metaSilent = true;
            Assert.That(GhostStore.MayStore(sim, track), Is.False);
            sim.metaSilent = false;
            sim.noRecords = true;
            Assert.That(GhostStore.MayStore(sim, track), Is.False);
            GhostStore.FolderOverride = folder;
            Assert.That(GhostStore.MayStore(sim, track), Is.True);
        }

        [Test]
        public void Sample_InterpolatesByRunStep()
        {
            GhostTrack g = GhostMade(1, 4, 30, (1, 600));
            Assert.That(g.Sample(15, out double x, out double y, out double z), Is.True);
            Assert.That(x, Is.EqualTo(0.5).Within(1e-9));
            Assert.That(y, Is.EqualTo(0.25).Within(1e-9));
            Assert.That(z, Is.EqualTo(-1).Within(1e-9));
            Assert.That(g.Sample(90, out x, out _, out _), Is.True, "the last point");
            Assert.That(x, Is.EqualTo(3).Within(1e-9));
            Assert.That(g.Sample(90.5, out _, out _, out _), Is.False, "past the best's end");
            Assert.That(g.Sample(-1, out x, out _, out _), Is.True, "before the start: the first point");
            Assert.That(x, Is.EqualTo(0));
            Assert.That(g.EndStep, Is.EqualTo(90));
        }

        [Test]
        public void Split_IsRunStepsAgainstTheBestsRoundStart()
        {
            GhostTrack g = GhostMade(1, 4, 30, (1, 600), (5, 1200));
            Assert.That(g.Split(5, 1080, out double ahead), Is.True);
            Assert.That(ahead, Is.EqualTo(-1).Within(1e-12), "120 steps sooner: 1 s ahead");
            Assert.That(g.Split(5, 1500, out double behind), Is.True);
            Assert.That(behind, Is.EqualTo(2.5).Within(1e-12));
            Assert.That(g.Split(6, 1500, out _), Is.False, "the best never began round 6");
        }

        [Test]
        public void Headless_ARunsRoute_BecomesItsSeedsGhost()
        {
            SurvivalSim sim = RouteTrackTests.RouteHeadless(out GameConfig cfg, out _);
            var track = new RouteTrack(sim);
            var store = new GhostStore(folder, cfg.Int("design.ghost.maxSeeds"));
            bool offered = false;
            track.Ended += s =>
            {
                if (GhostStore.MayStore(sim, track)) offered = store.Offer(GhostTrack.From(track));
            };
            RouteTrackTests.RouteScriptedRun(sim, 77, 3000);
            int reached = track.ReachedRound;
            int points = track.count;
            sim.EndRun();
            track.CheckEnded();
            Assert.That(offered, Is.True);
            GhostTrack g = store.Load(77);
            Assert.That(g, Is.Not.Null);
            Assert.That(g.seed, Is.EqualTo(77u));
            Assert.That(g.count, Is.EqualTo(points));
            Assert.That(g.ReachedRound, Is.EqualTo(reached));
            Assert.That(g.everySteps, Is.EqualTo(track.everySteps));
            Assert.That(g.Sample(0, out double x, out _, out double z), Is.True);
            Assert.That(x, Is.EqualTo(track.px[0]));
            Assert.That(z, Is.EqualTo(track.pz[0]));
        }

        [Test]
        public void Headless_ASecondRunOnTheSeed_RacesTheBest_AndSplits()
        {
            // SurvivalGame.Route.cs's glue on a headless composition: the end stores, the start loads, roundBegin splits.
            SurvivalSim sim = RouteTrackTests.RouteHeadless(out GameConfig cfg, out _);
            var track = new RouteTrack(sim);
            var store = new GhostStore(folder, cfg.Int("design.ghost.maxSeeds"));
            GhostTrack best = null;
            var splits = new List<(int round, double seconds)>();
            track.Ended += s =>
            {
                if (GhostStore.MayStore(sim, track)) store.Offer(GhostTrack.From(track));
            };
            sim.RunStarted += () => best = store.Load(sim.runSeed);
            sim.events.On<RoundEvent>("roundBegin", e =>
            {
                if (best != null && best.Split(e.number, sim.run.steps, out double s)) splits.Add((e.number, s));
            });
            RouteTrackTests.RouteScriptedRun(sim, 99, 1200);
            Assert.That(best, Is.Null, "the first run on the seed has no best");
            Assert.That(splits.Count, Is.EqualTo(0));
            sim.EndRun();
            RouteTrackTests.RouteScriptedRun(sim, 99, 1200);
            Assert.That(best, Is.Not.Null, "the second run races the first");
            Assert.That(splits.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(splits[0].round, Is.EqualTo(1));
            Assert.That(splits[0].seconds, Is.EqualTo(0), "round 1 begins after the same first break");
            Assert.That(best.Sample(sim.run.steps, out _, out _, out _), Is.True, "the best's track covers the run so far");
        }
    }
}
