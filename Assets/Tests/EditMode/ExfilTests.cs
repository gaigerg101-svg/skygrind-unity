// NIGHT11 U3-24.7: EXFIL (Core/Exfil.cs; new design: CoD Zombies' exfil, BO6's schedule, design.exfil). The schedule
// (the radio only in the breaks before 11, 16, 21 ...; none with the flag off), the kill demand (35 at 11, 45 at 16), the
// site by the round (the next open one when its zone is locked), success (reach, then the kills: exfilDone, EXFIL),
// failure in REACH and in HORDE (exfilFail "time", EXFIL FAILED) and by death, the queue top-up, Holding keeping the
// exfil round from clearing in a real SurvivalRun, and the records' exfil flag.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class ExfilTests
    {
        static GameConfig cfg;
        static double dt;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            dt = cfg.Dt;
        }

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly Exfil exfil;
            public readonly List<string> log = new List<string>();
            public Vec3 player = new Vec3(0, 0, 40); // the spawn
            public int remaining;
            public int added;

            public Rig(GameConfig c, uint seed = 11)
            {
                exfil = new Exfil(c, events, () => player, () => remaining, squads =>
                {
                    foreach (List<string> s in squads) added += s.Count;
                    foreach (List<string> s in squads) remaining += s.Count;
                });
                exfil.Reset(seed);
                // payloads are reused: read them here
                events.On<ExfilEvent>("exfilCall", e => log.Add($"exfilCall {e.round}"));
                events.On<ExfilEvent>("exfilStart", e => log.Add($"exfilStart {e.round} site {e.site} {e.zone}"));
                events.On<ExfilEvent>("exfilReach", e => log.Add($"exfilReach {e.round} needed {e.needed}"));
                events.On<ExfilEvent>("exfilDone", e => log.Add($"exfilDone {e.round} kills {e.kills}"));
                events.On<ExfilEvent>("exfilFail", e => log.Add($"exfilFail {e.round} {e.reason}"));
            }

            public void Break(int n) => events.Emit("roundBreak", new RoundEvent { number = n });

            public void Begin(int n) => events.Emit("roundBegin", new RoundEvent { number = n });

            public void Kill(string owner = "player") => events.Emit("kill", new KillEvent { owner = owner, weapon = "revolver", kind = "husk" });

            public void Steps(int n)
            {
                for (int i = 0; i < n; i++) exfil.Step(dt);
            }

            /// <summary>Call in the break before `n`, then begin it.</summary>
            public void CallAndBegin(int n)
            {
                Break(n);
                Assert.That(exfil.Call(), Is.True, $"the radio at round {n}'s break");
                Begin(n);
            }

            public void StandOnTheSite() => player = exfil.SitePos;
        }

        static GameConfig Off()
        {
            var off = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            PortData.Obj(PortData.Obj(off.root["design"])["exfil"])["enabled"] = false;
            return off;
        }

        [Test]
        public void TheSchedule_TheRadioOnlyInTheBreaksBefore11_16_21_AndTheDemand()
        {
            Assert.That(Exfil.Enabled(cfg), Is.True, "design.exfil.enabled ships on");
            var r = new Rig(cfg);
            var offered = new List<int>();
            for (int n = 1; n <= 30; n++)
            {
                r.Break(n);
                if (r.exfil.CanCall()) offered.Add(n);
                Assert.That(r.exfil.Radio.available(), Is.EqualTo(r.exfil.CanCall()));
                r.Begin(n);
                Assert.That(r.exfil.CanCall(), Is.False, "never during a fight");
            }
            Assert.That(offered, Is.EqualTo(new[] { 11, 16, 21, 26 }), "BO6: from 11, every 5");
            Assert.That(r.exfil.KillsFor(11), Is.EqualTo(35), "BO6's 35 at round 11");
            Assert.That(r.exfil.KillsFor(16), Is.EqualTo(45), "+10 per later exfil");
            Assert.That(r.exfil.KillsFor(21), Is.EqualTo(55));
            Assert.That(r.exfil.SiteFor(11), Is.EqualTo(0));
            Assert.That(r.exfil.SiteFor(16), Is.EqualTo(1));
            Assert.That(r.exfil.SiteFor(21), Is.EqualTo(2));
            Assert.That(r.exfil.SiteFor(26), Is.EqualTo(0), "the sites cycle");
            Assert.That(r.exfil.Radio.price(), Is.EqualTo(0), "free");
            Assert.That(r.exfil.Radio.reach, Is.EqualTo(cfg.Num("perks.reach")));
            Assert.That(r.exfil.radio.zone, Is.EqualTo("landing"), "the radio stands at the spawn");
            Assert.That(r.log, Is.Empty, "nothing called");
        }

        [Test]
        public void TheSite_IsTheNextOpenOne_AndNoOpenSiteMeansNoRadio()
        {
            var r = new Rig(cfg);
            var open = new HashSet<string> { "grinder", "heights" };
            r.exfil.zoneOpen = open.Contains;
            Assert.That(r.exfil.Site(0).zone, Is.EqualTo("slabs"));
            Assert.That(r.exfil.SiteFor(11), Is.EqualTo(1), "SLAB GARDENS locked: THE GRINDER");
            open.Clear();
            r.Break(11);
            Assert.That(r.exfil.CanCall(), Is.False, "no site's zone open: no radio");
            open.Add("slabs");
            Assert.That(r.exfil.CanCall(), Is.True);
            Assert.That(r.exfil.SiteFor(21), Is.EqualTo(0), "HEIGHTS locked at 21: on to SLAB GARDENS");
        }

        [Test]
        public void FlagOff_NoRadio_NothingHolds()
        {
            var r = new Rig(Off());
            Assert.That(r.exfil.enabled, Is.False);
            Assert.That(r.exfil.Radio, Is.Null, "no Buyables item");
            r.Break(11);
            Assert.That(r.exfil.CanCall(), Is.False);
            Assert.That(r.exfil.Call(), Is.False);
            r.Begin(11);
            r.Steps(10);
            Assert.That(r.exfil.Holding, Is.False);
            Assert.That(r.exfil.Outcome, Is.Null);
            Assert.That(r.log, Is.Empty);
            var noDesign = GameConfig.FromText(File.ReadAllText(Path.Combine(Port.Root, "config.json")), null);
            Assert.That(Exfil.Enabled(noDesign), Is.False, "no design config: no exfil");
        }

        [Test]
        public void Success_ReachTheSiteThenTheKills_ExfilDone_Outcome()
        {
            var r = new Rig(cfg);
            r.remaining = 50;
            r.CallAndBegin(11);
            Assert.That(r.log, Is.EqualTo(new[] { "exfilCall 11", "exfilStart 11 site 0 slabs" }));
            Assert.That(r.exfil.phase, Is.EqualTo(ExfilPhase.Reach));
            Assert.That(r.exfil.Holding, Is.True, "the exfil round can't clear");
            Assert.That(r.exfil.FullPace, Is.False);
            Assert.That(r.exfil.SecondsLeft, Is.EqualTo(cfg.Num("design.exfil.reachTime")).Within(1e-9));
            r.Kill();
            Assert.That(r.exfil.kills, Is.EqualTo(0), "kills before the site don't count");
            r.Steps(100);
            Assert.That(r.exfil.phase, Is.EqualTo(ExfilPhase.Reach), "still far from the site");
            // On the ring's edge, inside siteRadius and siteHeight
            Vec3 s = r.exfil.SitePos;
            r.player = new Vec3(s.x + 3.9, s.y + 2.5, s.z);
            r.Steps(1);
            Assert.That(r.exfil.phase, Is.EqualTo(ExfilPhase.Horde));
            Assert.That(r.exfil.FullPace, Is.True, "the director at full pace");
            Assert.That(r.exfil.needed, Is.EqualTo(35));
            Assert.That(r.exfil.SecondsLeft, Is.EqualTo(cfg.Num("design.exfil.killTime")).Within(1e-9));
            Assert.That(r.added, Is.EqualTo(0), "50 enemies left cover 35 kills: no top-up");
            for (int i = 0; i < 34; i++) r.Kill();
            r.Kill("rival");
            Assert.That(r.exfil.Outcome, Is.Null, "34 player kills");
            r.Kill();
            Assert.That(r.exfil.Outcome, Is.EqualTo(Exfil.Win));
            Assert.That(r.exfil.phase, Is.EqualTo(ExfilPhase.Done));
            Assert.That(r.exfil.Holding, Is.False);
            Assert.That(r.log[r.log.Count - 1], Is.EqualTo("exfilDone 11 kills 35"));
            Assert.That(r.log, Does.Contain("exfilReach 11 needed 35"));
        }

        [Test]
        public void Failure_InReach_TimeRunsOut()
        {
            var r = new Rig(cfg);
            r.CallAndBegin(11);
            int steps = MovementMath.StepsFor(cfg.Num("design.exfil.reachTime"), dt);
            r.Steps(steps - 1);
            Assert.That(r.exfil.Outcome, Is.Null, "not before reachTime");
            r.player = new Vec3(r.exfil.SitePos.x, r.exfil.SitePos.y + 4, r.exfil.SitePos.z); // over it, too high
            r.Steps(1);
            Assert.That(r.exfil.Outcome, Is.EqualTo(Exfil.Lost));
            Assert.That(r.exfil.failReason, Is.EqualTo("time"));
            Assert.That(r.log[r.log.Count - 1], Is.EqualTo("exfilFail 11 time"));
            Assert.That(r.exfil.Holding, Is.False);
        }

        [Test]
        public void Failure_InHorde_TimeRunsOut_AtRound16_TheDemandIs45_AndTheQueueIsToppedUp()
        {
            var r = new Rig(cfg);
            r.remaining = 5;
            r.CallAndBegin(16);
            Assert.That(r.log[r.log.Count - 1], Is.EqualTo("exfilStart 16 site 1 grinder"));
            r.StandOnTheSite();
            r.Steps(1);
            Assert.That(r.exfil.needed, Is.EqualTo(45));
            Assert.That(r.added, Is.GreaterThanOrEqualTo(40), "5 left of 45 needed: the round's own squads are added");
            Assert.That(r.exfil.topUpEnemies, Is.EqualTo(r.added));
            for (int i = 0; i < 44; i++)
            {
                r.Kill();
                r.remaining--;
            }
            int steps = MovementMath.StepsFor(cfg.Num("design.exfil.killTime"), dt); // counted from the step after the reach
            r.Steps(steps - 1);
            Assert.That(r.exfil.Outcome, Is.Null, "not before killTime");
            r.Steps(1);
            Assert.That(r.exfil.Outcome, Is.EqualTo(Exfil.Lost), "44 of 45");
            Assert.That(r.log[r.log.Count - 1], Is.EqualTo("exfilFail 16 time"));
        }

        [Test]
        public void TheTopUp_AddsOnlyWhatTheDemandNeeds_OncePerSquadInterval_FromTheSeed()
        {
            var a = new Rig(cfg, 99);
            var b = new Rig(cfg, 99);
            foreach (Rig r in new[] { a, b })
            {
                r.remaining = 0;
                r.CallAndBegin(11);
                r.StandOnTheSite();
                r.Steps(1);
            }
            Assert.That(a.added, Is.EqualTo(b.added), "the same run seed plans the same squads");
            Assert.That(a.added, Is.GreaterThanOrEqualTo(35));
            Assert.That(a.added, Is.LessThan(35 + 10), "squads stop once the demand is covered");
            int before = a.added;
            a.remaining = 0; // they all died some other way
            a.Steps(MovementMath.StepsFor(cfg.Num("director.squadInterval"), dt) - 1);
            Assert.That(a.added, Is.EqualTo(before), "at most one top-up per director.squadInterval");
            a.Steps(2);
            Assert.That(a.added, Is.GreaterThan(before));
        }

        [Test]
        public void DeathDuringTheExfil_FailsIt()
        {
            var r = new Rig(cfg);
            r.CallAndBegin(11);
            r.events.Emit("death", new DeathEvent { owner = "player", source = "husk" });
            Assert.That(r.exfil.Outcome, Is.EqualTo(Exfil.Lost));
            Assert.That(r.exfil.failReason, Is.EqualTo("death"));
            Assert.That(r.log[r.log.Count - 1], Is.EqualTo("exfilFail 11 death"));
        }

        sealed class EmptySpawner : IRoundSpawner
        {
            public void StartWave(int round, uint seed) { }
            public void Step(double dt) { }
            public bool Done => true; // every round is already empty
            public void Clear() { }
        }

        [Test]
        public void Holding_TheExfilRoundCantClear_InARealSurvivalRun_UntilItEnds()
        {
            var events = new EventBus();
            var economy = new Economy(cfg, events);
            var health = new Health(100, events, "player");
            var regen = new Regen(cfg, events, health, () => 0);
            var run = new SurvivalRun(cfg, events, new EmptySpawner(), economy, regen);
            Vec3 player = default;
            var exfil = new Exfil(cfg, events, () => player);
            exfil.Reset(5);
            run.holdRound = () => exfil.Holding;
            run.stepHook = exfil.Step;
            int clears = 0;
            events.On<RoundEvent>("roundClear", _ => clears++);
            run.Start(5);
            run.DebugSkipTo(11);
            Assert.That(run.round, Is.EqualTo(11));
            Assert.That(run.state, Is.EqualTo(SurvivalRun.State.Break));
            Assert.That(exfil.CanCall(), Is.True, "in round 11's break");
            Assert.That(exfil.Call(), Is.True);
            for (int i = 0; i < 2000 && run.state == SurvivalRun.State.Break; i++) run.Step(dt);
            Assert.That(run.state, Is.EqualTo(SurvivalRun.State.Fight));
            Assert.That(exfil.phase, Is.EqualTo(ExfilPhase.Reach));
            for (int i = 0; i < 120; i++) run.Step(dt);
            Assert.That(clears, Is.EqualTo(0), "the spawner is done, but the exfil holds the round");
            Assert.That(run.round, Is.EqualTo(11));
            player = exfil.SitePos;
            run.Step(dt);
            Assert.That(exfil.phase, Is.EqualTo(ExfilPhase.Horde), "reached through the step hook");
            for (int i = 0; i < 35; i++) events.Emit("kill", new KillEvent { owner = "player", weapon = "revolver", kind = "husk" });
            Assert.That(exfil.Outcome, Is.EqualTo(Exfil.Win));
            Assert.That(run.stats.kills, Is.EqualTo(35), "the run counted them too");
        }

        [Test]
        public void Records_KeepTheExfilFlag_OldRecordsReadFalse_JudgingUnchanged()
        {
            var store = new Records();
            var won = new Records.Run { round = 11, points = 4200, date = 1, exfil = true };
            Records.Judgement j = store.Submit("survival", won);
            Assert.That(j.text, Is.EqualTo("FIRST RUN ON RECORD"));
            string json = store.ToJson();
            Assert.That(json, Does.Contain("\"exfil\":true"));
            Records back = Records.Read(json);
            Assert.That(back.Best("survival").exfil, Is.True, "round-trips");
            Assert.That(back.Best("survival").round, Is.EqualTo(11));
            var plain = new Records();
            plain.Submit("survival", new Records.Run { round = 4, points = 1, date = 2 });
            Assert.That(plain.ToJson(), Is.EqualTo("{\"v\":1,\"modes\":{\"survival\":{\"round\":4,\"points\":1,\"date\":2}}}"), "no flag: the web's bytes");
            Assert.That(Records.Read("{\"v\":1,\"modes\":{\"survival\":{\"round\":4,\"points\":1}}}").Best("survival").exfil, Is.False, "an older record reads false");
            Assert.That(Records.Judge(new Records.Run { round = 12, points = 1, exfil = false }, won).text, Is.EqualTo(Records.Judge(new Records.Run { round = 12, points = 1 }, new Records.Run { round = 11, points = 4200 }).text), "the flag doesn't change judging");
        }

        [Test]
        public void TheSpots_TheRadioInLanding_ThreeSitesInThreeZones_OnTheirFloors()
        {
            var map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
            var world = new GameWorld(map.BuildOctree(), map.CreateCore(cfg.movement));
            var r = new Rig(cfg);
            Exfil x = r.exfil;
            Assert.That(ZoneMath.ZoneIndexAt(map.zoneParts, x.radio.pos.x, x.radio.pos.y + 1, x.radio.pos.z), Is.EqualTo(map.ZoneIndex(map.startZone)), "the radio is in LANDING");
            Assert.That(Vec3.DistanceTo(x.radio.pos, map.playerSpawn), Is.LessThan(6), "at the spawn");
            Assert.That(x.SiteCount, Is.EqualTo(3));
            var zones = new HashSet<string>();
            for (int i = 0; i < x.SiteCount; i++)
            {
                BoxSpot s = x.Site(i);
                Assert.That(s.zone, Is.Not.EqualTo(map.startZone), $"site {i} is not in LANDING");
                Assert.That(zones.Add(s.zone), Is.True, $"site {i}: one per zone");
                Assert.That(ZoneMath.ZoneIndexAt(map.zoneParts, s.pos.x, s.pos.y + 1, s.pos.z), Is.EqualTo(map.ZoneIndex(s.zone)), $"site {i} is in {s.zone}");
                for (int k = 0; k <= 8; k++)
                {
                    double a = k * Math.PI / 4, rr = k == 8 ? 0 : x.siteRadius;
                    bool on = world.RayIntersect(new Vec3(s.pos.x + Math.Cos(a) * rr, s.pos.y + 3, s.pos.z + Math.Sin(a) * rr), new Vec3(0, -1, 0), out RayHit h);
                    Assert.That(on && Math.Abs(h.distance - 3) < 1e-9, Is.True, $"site {i}: flat floor over its ring ({k})");
                }
            }
        }

        [Test]
        public void TheStep_AllocatesNothing_InReachAndHorde()
        {
            var r = new Rig(cfg);
            r.remaining = 100;
            r.CallAndBegin(11);
            r.Steps(10); // warm the delegates
            long reach = AllocCount.Of(() => r.Steps(600));
            r.StandOnTheSite();
            r.Steps(10);
            long horde = AllocCount.Of(() => r.Steps(600));
            Assert.That(r.exfil.phase, Is.EqualTo(ExfilPhase.Horde));
            Assert.That(reach, Is.EqualTo(0), "REACH");
            Assert.That(horde, Is.EqualTo(0), "HORDE with enough enemies left");
        }
    }

    /// <summary>
    /// NIGHT11: allocation on this thread while `a` runs: in Unity the GC.Alloc recorder's sample count (ZeroAllocTests'
    /// method), elsewhere (the harness's dotnet test) the bytes the runtime counted. 0 means none either way.
    /// </summary>
    static class AllocCount
    {
        public static long Of(Action a)
        {
#if UNITY_5_3_OR_NEWER
            var recorder = UnityEngine.Profiling.Recorder.Get("GC.Alloc");
            recorder.enabled = false;
            recorder.FilterToCurrentThread();
            recorder.enabled = true;
            try
            {
                a();
            }
            finally
            {
                recorder.enabled = false;
                recorder.CollectFromAllThreads();
            }
            return recorder.sampleBlockCount;
#else
            long before = GC.GetAllocatedBytesForCurrentThread();
            a();
            return GC.GetAllocatedBytesForCurrentThread() - before;
#endif
        }
    }
}
