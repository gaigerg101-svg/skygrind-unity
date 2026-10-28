// NIGHT20 A3 (builder P): THE BEACON on a run (Game/Beacon20Run.cs; the rule is Core/Beacon20.cs). A beacon stands in a
// break the rule picks, at a seeded open spot (a HOLD ring's spot in an open zone) as a Buyables item (hold BUY, free);
// taken, the next round gets design.adds.beacon.elites elites (the director's SpawnElite: E's ELITE, its kill a reveal)
// and escortSquads squads more; ignored, the round is exactly what it would have been (opt-in only). Off with the block.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.Tests
{
    public class Beacon20RunTests
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

        /// <summary>A headless run with the beacon forced on, skipped to the break before the first round it stands in.</summary>
        static (SurvivalSim, Beacon20Run, int) ToABeacon(uint seed)
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            sim.noLab = true;
            Beacon20Run b = sim.Beacon20Ensure();
            b.Force = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(seed, null, null);
            Beacon20.P p = Beacon20.P.From(Cfg());
            DirectorMath.D d = DirectorMath.From(Cfg());
            int r = p.from;
            while (!Beacon20.Stands(sim.runSeed, r, p, d) && r < 60) r++;
            Assert.That(r, Is.LessThan(60), "a beacon round within 60");
            sim.run.DebugSkipTo(r); // the break before round r
            return (sim, b, r);
        }

        static int CountBy(SurvivalSim sim, string kind)
        {
            int n = 0;
            foreach (Enemy e in sim.enemies.all) if (e.alive && e.kind == kind) n++;
            return n;
        }

        [Test]
        public void InItsBreak_ItStandsAtASeededOpenSpot_AsAFreeBuy()
        {
            (SurvivalSim sim, Beacon20Run b, int r) = ToABeacon(3);
            Assert.That(b.Standing, Is.True);
            Assert.That(b.Item.available(), Is.True);
            Assert.That(b.Item.price(), Is.EqualTo(0), "free: its price is the harder round");
            Assert.That(sim.map.holdSpots.Contains(b.Spot), Is.True, "one of the map's spots");
            int zone = ZoneMath.ZoneIndexAt(sim.map.zoneParts, b.Spot.x, b.Spot.y + 1, b.Spot.z);
            Assert.That(sim.zones.IsLocked(zone), Is.False, "in an open zone");
            (SurvivalSim sim2, Beacon20Run b2, int r2) = ToABeacon(3);
            Assert.That((r2, b2.Spot.x, b2.Spot.z), Is.EqualTo((r, b.Spot.x, b.Spot.z)), "seeded: the same run, the same beacon");
        }

        [Test]
        public void Taken_TheNextRoundAddsTheElitesAndTheEscort_AsConfigured()
        {
            Beacon20.P p = Beacon20.P.From(Cfg());
            (SurvivalSim sim, Beacon20Run b, int r) = ToABeacon(5);
            int plannedOff = PlannedAfterBegin(5, false);
            b.Item.buy();
            Assert.That(b.Standing, Is.False, "taken: gone");
            Assert.That(b.TakenFor, Is.EqualTo(r));
            int guard = 0;
            while (sim.run.state != SurvivalRun.State.Fight && guard++ < 120 * 30) sim.Step();
            Assert.That(b.EscortAdded, Is.EqualTo(p.escortSquads), "the escort squads queued");
            Assert.That(b.ElitesCalled, Is.EqualTo(p.elites), "the elites called");
            int plannedOn = sim.director.Queue.Count + sim.director.squadsSpawned;
            Assert.That(plannedOn, Is.EqualTo(plannedOff + p.escortSquads), "the round's squads plus the escort");
        }

        /// <summary>The squads planned for the beacon round of `seed` when the beacon is taken or not (a fresh run).</summary>
        static int PlannedAfterBegin(uint seed, bool take)
        {
            (SurvivalSim sim, Beacon20Run b, int r) = ToABeacon(seed);
            if (take) b.Item.buy();
            int guard = 0;
            while (sim.run.state != SurvivalRun.State.Fight && guard++ < 120 * 30) sim.Step();
            return sim.director.Queue.Count + sim.director.squadsSpawned;
        }

        [Test]
        public void Ignored_TheRoundIsExactlyWhatItWouldHaveBeen()
        {
            (SurvivalSim a, Beacon20Run ba, int r) = ToABeacon(9);
            SurvivalSim c = SurvivalSim.Headless(Cfg(), Map()); // no beacon at all
            c.noLab = true;
            c.setup.mode = RunSetup.Survival;
            c.StartRun(9, null, null);
            c.run.DebugSkipTo(r);
            for (int i = 0; i < 120 * 40; i++)
            {
                a.Step();
                c.Step();
            }
            Assert.That(SimFingerprint.Hash(a), Is.EqualTo(SimFingerprint.Hash(c)), "an ignored beacon changes nothing");
            Assert.That(ba.Standing, Is.False, "and it went with its break");
        }

        /// <summary>The run snapshot's sections with THE BEACON's before the buy list's (its item must be listed before Buyables
        /// checks the ids): SurvivalSim.SnapshotSections plus ("beacon20", ...) until the lead's line puts it there.</summary>
        static List<(string name, IRunState state)> Sections(SurvivalSim sim)
        {
            List<(string name, IRunState state)> list = sim.SnapshotSections();
            if (list.Exists(s => s.name == "beacon20")) return list;
            list.Insert(list.FindIndex(s => s.name == "buyables"), ("beacon20", sim.Beacon20Ensure()));
            return list;
        }

        // NIGHT20 fix list (the lead, RunRestoreTests): SAVE & QUIT is allowed at any break, a beacon's among them. Its state
        // (standing, its round and spot, taken, the buy list's entry) must survive a snapshot and a restore, and the run go on
        // the same: ignored or taken.
        [TestCase(false)]
        [TestCase(true)]
        public void ASnapshotAtABeaconBreak_Restores_AndTheRunGoesOnTheSame(bool take)
        {
            (SurvivalSim a, Beacon20Run ba, int r) = ToABeacon(5);
            if (take) ba.Item.buy();
            string json = RunSnapshot.Save(Sections(a));
            SurvivalSim b = SurvivalSim.Headless(Cfg(), Map());
            b.noLab = true;
            Beacon20Run bb = b.Beacon20Ensure();
            bb.Force = true;
            b.setup.mode = RunSetup.Survival;
            b.StartRun(5, null, null);
            RunSnapshot.Load(json, Sections(b));
            Assert.That((bb.Standing, bb.StandFor, bb.TakenFor, bb.Spot.x, bb.Spot.z), Is.EqualTo((ba.Standing, ba.StandFor, ba.TakenFor, ba.Spot.x, ba.Spot.z)), "the beacon as it was");
            Assert.That(b.buyables.list.Contains(bb.Item), Is.EqualTo(a.buyables.list.Contains(ba.Item)), "its buy list entry");
            Assert.That(RunSnapshot.Save(Sections(b)), Is.EqualTo(json), "the restored run snapshots to the same bytes");
            for (int i = 0; i < 120 * 40; i++)
            {
                a.Step();
                b.Step();
            }
            Assert.That(SimFingerprint.Hash(b), Is.EqualTo(SimFingerprint.Hash(a)), "the run goes on the same");
            Assert.That((bb.EscortAdded, bb.ElitesCalled), Is.EqualTo((ba.EscortAdded, ba.ElitesCalled)), take ? "the escort came in both" : "no escort in either");
        }

        [Test]
        public void Off_NoBeacon()
        {
            SurvivalSim sim = SurvivalSim.Headless(Cfg(), Map());
            sim.noLab = true;
            Beacon20Run b = sim.Beacon20Ensure();
            sim.StartRun(1, null, null);
            Assert.That(b.On, Is.EqualTo(sim.n20.Add(Cfg(), "beacon")), "the run's adds bit and the row's switch");
            if (!b.On)
            {
                sim.run.DebugSkipTo(9);
                Assert.That(b.Standing, Is.False);
            }
        }
    }
}
