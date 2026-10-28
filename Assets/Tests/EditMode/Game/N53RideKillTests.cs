// EXT53 builder N (row N, the sprint contract written before the sim): RIDE KILL in a headless SurvivalSim (SurvivalSim.N53.RideKill.cs).
//   row off: a kill on the wall changes nothing (no event, the bar the same as a config without the row); the fingerprint is the baseline;
//   row on:  a kill on the wall pays back exactly one boost (design.stamina.boost.cost of the bar) and fires rideKill53 once;
//            a kill on foot (no ride this run) pays nothing; the grace after a ride counts whole steps (48 at 120 Hz: in at 48, out at 49).
// The ride is the Mover's state at the kill (set here: the contract is the rule, not the Mover's attach, which verbs37 and the a253 reel
// perform on a real wall); the kill is a real player kill (Loadout.Damage: the bus's `kill`, so the stamina's own refill runs first).
// No UnityEngine, so it also runs headless on the harness's mini NUnit:
//   dotnet run --project Tools/SimHarness -c Release -p:Extra=../../Assets/Tests/EditMode/Game/N53RideKillTests.cs -- n53ridekill selftest
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Skygrind.Core;

namespace Skygrind.Game.Tests
{
    public class N53RideKillTests
    {
        static string Root => Path.GetFullPath("PortExport");

        /// <summary>A fresh headless run on the survival map, the row forced (null: the config's own), the player invulnerable at the spawn.</summary>
        static SurvivalSim Run(bool? rideKill, out List<RideKill53Event> fired)
        {
            GameConfig cfg = GameConfig.Load(Path.Combine(Root, "config.json"));
            SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            if (rideKill.HasValue) sim.rowsOverride = sim.rowsRegistry.Defaults().Set("rideKill53", rideKill.Value);
            sim.setup.mode = RunSetup.Survival;
            sim.setup.heat = 0;
            sim.StartRun(4711, null, null);
            sim.Respawn(map.playerSpawn, 0);
            sim.mover.invulnerable = true;
            var list = new List<RideKill53Event>();
            sim.events.On<RideKill53Event>(RideKill53.Event, e => list.Add(new RideKill53Event { onWall = e.onWall, stepsSinceRide = e.stepsSinceRide, bar = e.bar, before = e.before, after = e.after, count = e.count, weapon = e.weapon }));
            fired = list;
            return sim;
        }

        /// <summary>One player kill 2 m ahead of the feet (a KESTREL hit for 1e6), with the bar emptied first; the bar after it.</summary>
        static double KillWithEmptyBar(SurvivalSim sim)
        {
            Vec3 p = sim.mover.position;
            Husk h = sim.husks.Spawn("husk", new Vec3(p.x, p.y, p.z - 2), p);
            Assert.That(h != null, Is.True, "the husk spawned");
            sim.mover.stamina.bar = 0;
            sim.mover.pips = 0;
            sim.loadout.Damage(h, 1e6, false, "revolver", sim.Eye);
            Assert.That(h.alive, Is.False, "the kill happened");
            return sim.mover.stamina.rules.Any ? sim.mover.stamina.bar : sim.mover.pips;
        }

        static void Steps(SurvivalSim sim, int n)
        {
            for (int i = 0; i < n; i++) sim.Step();
        }

        [Test]
        public void RowOff_AKillOnTheWall_ChangesNothing()
        {
            SurvivalSim plain = Run(null, out List<RideKill53Event> f0);
            Assert.That(plain.RideKill53On, Is.False, "the config ships the row off");
            plain.mover.state = MoveState.WallRide;
            double barPlain = KillWithEmptyBar(plain);
            SurvivalSim off = Run(false, out List<RideKill53Event> f1);
            off.mover.state = MoveState.WallRide;
            double barOff = KillWithEmptyBar(off);
            Assert.That(f0.Count + f1.Count, Is.EqualTo(0), "no rideKill53 event");
            Assert.That(barOff, Is.EqualTo(barPlain), "the bar after the kill is the kill's own refill only");
            Assert.That(off.RideKill53Count + off.RideKill53Missed, Is.EqualTo(0), "no handler counted anything");
        }

        [Test]
        public void RowOn_AKillOnTheWall_RefundsExactlyOneBoost_AndFiresOnce()
        {
            SurvivalSim off = Run(false, out _);
            off.mover.state = MoveState.WallRide;
            double barOff = KillWithEmptyBar(off);
            SurvivalSim on = Run(true, out List<RideKill53Event> fired);
            Assert.That(on.RideKill53On, Is.True);
            on.mover.state = MoveState.WallRide;
            double barOn = KillWithEmptyBar(on);
            double cost = on.cfg.Num("design.stamina.boost.cost");
            Assert.That(on.mover.stamina.rules.Any, Is.True, "the shipped stamina bar");
            Assert.That(barOn - barOff, Is.EqualTo(cost), "exactly one boost's cost on top of the kill's own refill");
            Assert.That(fired.Count, Is.EqualTo(1), "rideKill53 fired once");
            Assert.That(fired[0].onWall, Is.True);
            Assert.That(fired[0].stepsSinceRide, Is.EqualTo(0));
            Assert.That(fired[0].after - fired[0].before, Is.EqualTo(cost));
            Assert.That(fired[0].weapon, Is.EqualTo("revolver"));
            Assert.That(on.RideKill53Count, Is.EqualTo(1));
        }

        [Test]
        public void RowOn_AKillOnFoot_PaysNothing()
        {
            SurvivalSim off = Run(false, out _);
            off.mover.state = MoveState.Ground;
            double barOff = KillWithEmptyBar(off);
            SurvivalSim on = Run(true, out List<RideKill53Event> fired);
            on.mover.state = MoveState.Ground;
            double barOn = KillWithEmptyBar(on);
            Assert.That(fired.Count, Is.EqualTo(0), "no event on foot");
            Assert.That(barOn, Is.EqualTo(barOff), "the kill's own refill only");
            Assert.That(on.RideKill53Missed, Is.EqualTo(1), "the kill was seen and did not count");
        }

        [Test]
        public void RowOn_TheGraceAfterARide_CountsWholeSteps()
        {
            SurvivalSim on = Run(true, out List<RideKill53Event> fired);
            int grace = on.rideKill53Params.graceSteps;
            Assert.That(grace, Is.EqualTo(48));
            Steps(on, 2);
            on.rideKill53Clock.Rode(on.simStep); // the ride's last step (the Mover's WallRideEnd or this hook mark it in a real ride)
            Steps(on, grace);
            on.mover.state = MoveState.Air;
            KillWithEmptyBar(on);
            Assert.That(fired.Count, Is.EqualTo(1), "a kill 48 steps after the ride counts");
            Assert.That(fired[0].onWall, Is.False);
            Assert.That(fired[0].stepsSinceRide, Is.EqualTo(grace));
            Steps(on, 1);
            on.mover.state = MoveState.Air;
            KillWithEmptyBar(on);
            Assert.That(fired.Count, Is.EqualTo(1), "49 steps after: no second refund");
            Assert.That(on.RideKill53Missed, Is.EqualTo(1));
        }

        [Test]
        public void RowOn_ARunStart_ForgetsTheRide()
        {
            SurvivalSim on = Run(true, out List<RideKill53Event> fired);
            on.rideKill53Clock.Rode(on.simStep);
            on.StartRun(4712, null, null);
            on.Respawn(on.map.playerSpawn, 0);
            on.mover.invulnerable = true;
            on.mover.state = MoveState.Air;
            KillWithEmptyBar(on);
            Assert.That(fired.Count, Is.EqualTo(0), "rule 6: a new run starts with no ride");
            Assert.That(on.RideKill53Count, Is.EqualTo(0));
        }

        [Test]
        public void RowOff_TheFingerprint_IsTheBaseline()
        {
            int was = NightRows.ForceOffFrom;
            try
            {
                NightRows.ForceOffFrom = NightRows.NoOff; // T-126: an earlier test in the batch can leave the off switch set (Lesson25Tests leaves 46)
                GameConfig cfg = GameConfig.Load(Path.Combine(Root, "config.json"));
                SurvivalMap map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Root, "maps", "survival.json"))));
                SurvivalSim sim = SurvivalSim.Headless(cfg, map);
                List<string> lines = sim.RunFingerprint(SimFingerprint.Seed, SimFingerprint.Steps, SimFingerprint.Every, out string summary);
                Assert.That(FingerprintBaseline.FirstDifference(lines), Is.EqualTo(-1), "the composed fingerprint with rideKill53 in config: " + summary);
            }
            finally { NightRows.ForceOffFrom = was; }
        }
    }
}
