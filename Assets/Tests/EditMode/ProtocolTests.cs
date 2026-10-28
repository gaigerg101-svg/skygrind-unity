// NIGHT11 U3-24.4: THE PROTOCOL (Core/Protocol.cs, survival/protocol.js) and Director.AddSquads / fullPace. The web's test
// (tests/night6.test.js, 'THE PROTOCOL finish (B3)': four relays on the Survival map, each standing on a box top), then
// the port's, with a real Director wired as SurvivalGame wires it: the relays as buyables (hold BUY), asleep before
// protocol.wakeRound, awake at its round begin (banner), lighting needs the relay's zone open, a lit relay is owned (a
// hold is denied), four lit call the SURGE (queued in a break, live in a fight, through AddSquads), the clear forges every
// owned gun free and pays protocol.bonus, the secret relay, the HUD and the summary; `powered` (design.power) holds the
// wake; AddSquads turns the unported kinds into husks; fullPace streams squads through PEAK.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints; // AllocatingGCMemory() is an extension method here
using Is = NUnit.Framework.Is;

namespace Skygrind.Core.Tests
{
    public class ProtocolTests
    {
        static GameConfig cfg;
        static SurvivalMap map;
        static double dt;

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
            dt = cfg.Dt;
        }

        // ---- night6.test.js :103-111 ----

        [Test]
        public void FourRelaysOnTheSurvivalMap_EachOnItsShowpiece()
        {
            Assert.That(map.relaySpots.Count, Is.EqualTo(4));
            foreach (RelaySpot r in map.relaySpots)
            {
                bool under = false;
                foreach (MapBox b in map.boxes)
                {
                    if (r.pos.x >= b.min.x && r.pos.x <= b.max.x && r.pos.z >= b.min.z && r.pos.z <= b.max.z && Math.Abs(b.max.y - r.pos.y) < 0.01)
                    {
                        under = true;
                        break;
                    }
                }
                Assert.That(under, Is.True, r.id); // stands on a top
            }
        }

        // ---- the port's ----

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly GameWorld world;
            public readonly Zones zones;
            public readonly NavField nav;
            public readonly Husks husks;
            public readonly Director director;
            public readonly Economy economy;
            public readonly Buyables buyables;
            public readonly Forge forge;
            public readonly List<string> owned;
            public readonly Protocol protocol;
            public SurvivalRun.State state = SurvivalRun.State.Break;
            public Vec3 player;
            public readonly List<string> log = new List<string>();
            public readonly List<string> banners = new List<string>();
            public readonly List<string> forged = new List<string>();

            public Rig()
            {
                var enemies = new EnemyRegistry();
                world = new GameWorld(map.BuildOctree(), map.CreateCore(cfg.movement));
                zones = new Zones(cfg, map, world, events);
                nav = new NavField(cfg, NavGrid.FromExport(map.navGrid));
                nav.SetBlocked(zones.LockedTag);
                var brain = new HuskBrain(cfg, events, nav, world.Geometry) { onScreen = _ => true };
                husks = new Husks(cfg, events, enemies, world, map.killPlaneY) { jumper = new HuskNavJumper(cfg, events) };
                player = map.playerSpawn;
                director = new Director(cfg, events, husks, brain, zones, world.core, world.Geometry, nav, () => player, () => 0, () => 0) { huskWorld = world };
                economy = new Economy(cfg, events);
                economy.Start(0);
                buyables = new Buyables(cfg, events, economy);
                forge = new Forge(cfg, events);
                forge.Reset(3);
                owned = new List<string>(cfg.Strs("weapons.survivalStart")); // the run's start loadout (loadout.owned)
                // the finishes aren't ported: SurvivalGame passes () => false (the banners' no-finish text)
                protocol = new Protocol(cfg, events, buyables, zones, director, forge, owned, economy, () => state, () => false, () => false);
                events.On<RelayLitEvent>("relayLit", e => log.Add($"relayLit {e.owner} {e.id} {e.name} {e.count}/{e.total}"));
                events.On<ProtocolSurgeEvent>("protocolSurge", e => log.Add($"protocolSurge {e.owner}"));
                events.On<ProtocolCompleteEvent>("protocolComplete", e => log.Add($"protocolComplete {e.owner} {e.newFinish}"));
                events.On<SecretRelayEvent>("secretRelay", e => log.Add($"secretRelay {e.owner} {e.id} {e.newFinish}"));
                events.On<BannerEvent>("banner", e => banners.Add(e.text));
                events.On<ForgeEvent>("forge", e => forged.Add($"{e.weapon} {e.price} {e.free}"));
                events.On("buyDenied", id => log.Add($"buyDenied {id}"));
            }

            public void RoundBegin(int n) => events.Emit("roundBegin", new RoundEvent { number = n });

            public void RoundClear(int n) => events.Emit("roundClear", new RoundEvent { number = n });

            public Buyables.Item Item(string id) => buyables.list.Find(i => i.id == "relay:" + id);

            public int Queued()
            {
                int n = 0;
                foreach (List<string> s in director.Queue) n += s.Count;
                return n;
            }

            /// <summary>Stand on a relay and hold BUY for the hold time (buyables.js), then let go.</summary>
            public void HoldBuyAt(Vec3 at)
            {
                int steps = MovementMath.StepsFor(cfg.Num("buy.holdTime"), dt);
                for (int i = 0; i < steps; i++) buyables.Step(at, 0, true, dt);
                buyables.Step(at, 0, false, dt);
            }
        }

        static RelaySpot Spot(string id) => map.relaySpots.Find(r => r.id == id);

        [Test]
        public void Load_TheRelaysAreBuyables_ReloadReplacesThem_NoMapClears()
        {
            var g = new Rig();
            int before = g.buyables.list.Count;
            g.protocol.Load(map.relaySpots, map.secretRelay);
            Assert.That(g.buyables.list.Count, Is.EqualTo(before + 5), "four relays and the secret one");
            foreach (RelaySpot s in map.relaySpots)
            {
                Buyables.Item item = g.Item(s.id);
                Assert.That(item, Is.Not.Null, s.id);
                Assert.That(item.kind, Is.EqualTo("relay"));
                Assert.That(item.name, Is.EqualTo("RELAY · " + s.name));
                Assert.That(item.position, Is.EqualTo(s.pos));
                Assert.That(item.reach, Is.EqualTo(cfg.Num("protocol.reach")));
                Assert.That(item.price(), Is.EqualTo(0));
                Assert.That(item.owned(), Is.False);
            }
            Assert.That(g.protocol.relays.ConvertAll(r => r.id), Is.EqualTo(map.relaySpots.ConvertAll(r => r.id)));
            Assert.That(g.protocol.Secret.id, Is.EqualTo(map.secretRelay.Value.id));
            Assert.That(g.protocol.relays[0].zone, Is.EqualTo(g.zones.ZoneAt(new Vec3(map.relaySpots[0].pos.x, map.relaySpots[0].pos.y + 1, map.relaySpots[0].pos.z))));
            int version = g.protocol.Version;
            g.protocol.Load(map.relaySpots, map.secretRelay);
            Assert.That(g.buyables.list.Count, Is.EqualTo(before + 5), "a new run replaces the old relays");
            Assert.That(g.protocol.Version, Is.EqualTo(version + 1));
            g.protocol.Load(null, null);
            Assert.That(g.buyables.list.Count, Is.EqualTo(before));
            Assert.That(g.protocol.Summary.has, Is.False, "summary() is null outside a quest run");
            g.protocol.Load(new List<RelaySpot>(), map.secretRelay);
            Assert.That(g.protocol.Active || g.buyables.list.Count != before, Is.False, "a map without relaySpots: no quest");
        }

        [Test]
        public void TheQuest_WakesAtRound6_LightingNeedsTheZone_TheSurgeQueuedInABreak_TheClearForgesEveryGunAndPays()
        {
            var g = new Rig();
            g.protocol.Load(map.relaySpots, map.secretRelay);
            double wake = cfg.Num("protocol.wakeRound");
            Assert.That(wake, Is.EqualTo(6));
            // Asleep: nothing for sale, no HUD line; the summary counts from the start.
            for (int n = 1; n < wake; n++) g.RoundBegin(n);
            Assert.That(g.protocol.Awake || g.protocol.HudOn || g.protocol.GuideOn, Is.False);
            g.zones.OpenAll();
            foreach (RelaySpot s in map.relaySpots) Assert.That(g.Item(s.id).available(), Is.False, $"{s.id} asleep");
            ProtocolSummary sum = g.protocol.Summary;
            Assert.That(sum.has && sum.lit == 0 && sum.total == 4 && !sum.complete, Is.True);
            Assert.That(g.banners, Is.Empty);

            // Round 6 begins: the relays wake.
            var g2 = new Rig(); // the zones' locks as a run starts (only the landing open)
            g2.protocol.Load(map.relaySpots, map.secretRelay);
            for (int n = 1; n <= wake; n++) g2.RoundBegin(n);
            Assert.That(g2.protocol.Awake, Is.True);
            Assert.That(g2.banners, Is.EqualTo(new[] { "THE PROTOCOL WAKES · LIGHT THE FOUR RELAYS" }));
            Assert.That(g2.protocol.HudOn && g2.protocol.HudLit == 0 && g2.protocol.HudTotal == 4 && !g2.protocol.HudSurge, Is.True);
            Assert.That(g2.protocol.HudNext, Is.EqualTo(Spot("skybridge").name));
            Assert.That(g2.protocol.GuideOn && g2.protocol.NextRelay.id == "skybridge", Is.True);
            g2.RoundBegin(7);
            Assert.That(g2.banners.Count, Is.EqualTo(1), "wakes once");

            // Lighting needs the relay's zone open (THE SKYBRIDGE stands in THE HEIGHTS).
            Buyables.Item sky = g2.Item("skybridge");
            Assert.That(g2.protocol.relays[0].zone, Is.EqualTo("heights"));
            Assert.That(sky.available(), Is.False, "its zone is locked");
            g2.HoldBuyAt(Spot("skybridge").pos);
            Assert.That(g2.log, Is.Empty, "not for sale while locked");
            g2.zones.Open("heights", free: true);
            Assert.That(sky.available(), Is.True);
            Assert.That(g2.Item("chimney").available(), Is.False, "THE CHIMNEY is in THE ISLANDS, still locked");
            g2.HoldBuyAt(Spot("skybridge").pos); // hold BUY on it: free
            Assert.That(g2.log, Is.EqualTo(new[] { "relayLit player skybridge THE SKYBRIDGE 1/4" }));
            Assert.That(g2.banners[g2.banners.Count - 1], Is.EqualTo("RELAY 1 / 4 LIT · NEXT: THE CHIMNEY"));
            Assert.That(sky.owned(), Is.True);
            Assert.That(g2.economy.total, Is.EqualTo(0));
            // A lit relay is owned: holding BUY again is denied (buyables.js attempt), nothing lights twice.
            g2.HoldBuyAt(Spot("skybridge").pos);
            Assert.That(g2.log, Is.EqualTo(new[] { "relayLit player skybridge THE SKYBRIDGE 1/4", "buyDenied relay:skybridge" }));
            Assert.That(g2.protocol.HudLit, Is.EqualTo(1));
            Assert.That(g2.protocol.HudNext, Is.EqualTo("THE CHIMNEY"));

            // The rest, in a break: the fourth calls the SURGE, queued for the next round.
            g2.zones.Open("islands", free: true);
            g2.log.Clear();
            g2.Item("chimney").buy();
            g2.Item("ring").buy();
            Assert.That(g2.banners[g2.banners.Count - 1], Is.EqualTo("RELAY 3 / 4 LIT · NEXT: THE PINNACLE"));
            g2.state = SurvivalRun.State.Break;
            g2.Item("pinnacle").buy();
            Assert.That(g2.log, Is.EqualTo(new[]
            {
                "relayLit player chimney THE CHIMNEY 2/4", "relayLit player ring THE WALL RING 3/4", "relayLit player pinnacle THE PINNACLE 4/4",
                "protocolSurge player",
            }));
            Assert.That(g2.banners.GetRange(g2.banners.Count - 2, 2), Is.EqualTo(new[] { "ALL RELAYS LIT", "SURGE · THE PROTOCOL ANSWERS" }));
            Assert.That(g2.protocol.Surge, Is.EqualTo("queued"));
            Assert.That(g2.Queued(), Is.EqualTo(0), "queued: it joins the next round");
            Assert.That(g2.protocol.HudOn && g2.protocol.HudSurge && g2.protocol.HudNext == null && !g2.protocol.GuideOn, Is.True);
            g2.RoundClear(7); // a clear while queued completes nothing
            Assert.That(g2.protocol.Complete, Is.False);
            g2.state = SurvivalRun.State.Fight;
            g2.RoundBegin(8);
            Assert.That(g2.protocol.Surge, Is.EqualTo("live"));
            int surge = 0;
            foreach (object s in cfg.Arr("protocol.surge")) surge += PortData.Arr(s).Count;
            Assert.That(g2.Queued(), Is.EqualTo(surge), "the SURGE's squads join the round");
            Assert.That(g2.director.Remaining, Is.EqualTo(surge));

            // Clearing that round completes the PROTOCOL: every owned gun forged free, +protocol.bonus.
            g2.log.Clear();
            g2.RoundClear(8);
            Assert.That(g2.protocol.Complete, Is.True);
            var expectForged = new List<string>();
            foreach (string id in g2.owned) if (g2.forge.Known(id)) expectForged.Add($"{id} 0 True");
            Assert.That(expectForged, Is.Not.Empty);
            Assert.That(g2.forged, Is.EqualTo(expectForged));
            foreach (string id in g2.owned) Assert.That(g2.forge.IsForged(id), Is.EqualTo(g2.forge.Known(id)), id);
            Assert.That(g2.economy.byReason["protocol"], Is.EqualTo(cfg.Num("protocol.bonus")));
            Assert.That(g2.log, Is.EqualTo(new[] { "protocolComplete player False" }));
            Assert.That(g2.banners[g2.banners.Count - 1], Is.EqualTo("THE PROTOCOL IS COMPLETE · EVERY GUN FORGED"));
            Assert.That(g2.protocol.HudOn || g2.protocol.GuideOn, Is.False, "the HUD line goes once it is done");
            ProtocolSummary done = g2.protocol.Summary;
            Assert.That(done.has && done.lit == 4 && done.total == 4 && done.complete, Is.True);
            g2.RoundClear(9);
            Assert.That(g2.economy.byReason["protocol"], Is.EqualTo(cfg.Num("protocol.bonus")), "once");
        }

        [Test]
        public void TheSurge_LitInAFight_IsLiveAtOnce()
        {
            var g = new Rig();
            g.protocol.Load(map.relaySpots, map.secretRelay);
            g.protocol.DebugWake();
            g.state = SurvivalRun.State.Fight;
            foreach (RelaySpot s in map.relaySpots) g.protocol.DebugLight(s.id);
            Assert.That(g.protocol.Surge, Is.EqualTo("live"));
            Assert.That(g.Queued(), Is.EqualTo(12));
            g.RoundBegin(9); // nothing queued: nothing added
            Assert.That(g.Queued(), Is.EqualTo(12));
            g.RoundClear(9);
            Assert.That(g.protocol.Complete, Is.True);
        }

        [Test]
        public void TheSecretRelay_ForSaleWhenItsZoneIsOpen_AwakeOrNot_OutsideTheCount()
        {
            var g = new Rig();
            g.protocol.Load(map.relaySpots, map.secretRelay);
            RelaySpot secret = map.secretRelay.Value;
            Buyables.Item item = g.Item(secret.id);
            Assert.That(item.available(), Is.False, "its zone is locked");
            g.zones.Open(g.protocol.Secret.zone, free: true);
            Assert.That(item.available(), Is.True, "asleep, yet for sale");
            g.HoldBuyAt(secret.pos);
            Assert.That(g.log, Is.EqualTo(new[] { $"secretRelay player {secret.id} False" }));
            Assert.That(g.banners, Is.EqualTo(new[] { "A HIDDEN RELAY" }));
            Assert.That(g.protocol.Secret.lit && item.owned(), Is.True);
            ProtocolSummary s = g.protocol.Summary;
            Assert.That(s.lit == 0 && s.total == 4, Is.True, "outside the quest's count");
            g.protocol.DebugLight(secret.id); // once
            Assert.That(g.log.Count, Is.EqualTo(1));
        }

        [Test]
        public void Powered_FalseKeepsItAsleepAtRound6_TheFirstPoweredRoundBeginWakesIt()
        {
            var g = new Rig();
            g.protocol.Load(map.relaySpots, map.secretRelay);
            bool power = false;
            g.protocol.powered = () => power;
            for (int n = 1; n <= 8; n++) g.RoundBegin(n);
            Assert.That(g.protocol.Awake, Is.False, "no power, no wake");
            power = true;
            Assert.That(g.protocol.Awake, Is.False, "the power alone doesn't wake it: a round begin does");
            g.RoundBegin(9);
            Assert.That(g.protocol.Awake, Is.True);
            Assert.That(g.banners, Is.EqualTo(new[] { "THE PROTOCOL WAKES · LIGHT THE FOUR RELAYS" }));
            // powered before wakeRound: still the web's pacing
            var h = new Rig();
            h.protocol.Load(map.relaySpots, map.secretRelay);
            h.protocol.powered = () => true;
            for (int n = 1; n < 6; n++) h.RoundBegin(n);
            Assert.That(h.protocol.Awake, Is.False);
            h.RoundBegin(6);
            Assert.That(h.protocol.Awake, Is.True);
        }

        [Test]
        public void AddSquads_TurnsTheUnportedKindsIntoHusks_KeepsSpitter_LeavesTheCallersLists()
        {
            var g = new Rig();
            var list = new List<List<string>>
            {
                new List<string> { "charger", "lancer", "warden" },
                new List<string> { "spitter", "charger", "lancer" },
                new List<string> { "swarmer", "husk", "drone" },
            };
            g.director.AddSquads(list);
            Assert.That(g.director.Queue.Count, Is.EqualTo(3));
            Assert.That(g.director.Queue[0], Is.EqualTo(new[] { "husk", "husk", "husk" }));
            Assert.That(g.director.Queue[1], Is.EqualTo(new[] { "spitter", "husk", "husk" }));
            Assert.That(g.director.Queue[2], Is.EqualTo(new[] { "swarmer", "husk", "husk" }));
            Assert.That(list[0], Is.EqualTo(new[] { "charger", "lancer", "warden" }), "the caller's lists are unchanged");
            Assert.That(g.director.Remaining, Is.EqualTo(9));
        }

        [Test]
        public void TheRelaysForSale_AllocateNothingPerStep()
        {
            // The buy prompt asks every relay's available() each step (Buyables.Pick): no allocation.
            var g = new Rig();
            g.protocol.Load(map.relaySpots, map.secretRelay);
            g.protocol.DebugWake();
            g.zones.Open("heights", free: true);
            Vec3 at = Spot("skybridge").pos;
            TestDelegate run = () =>
            {
                for (int i = 0; i < 600; i++)
                {
                    g.buyables.Step(at, 0, false, dt);
                    bool on = g.protocol.HudOn && g.protocol.HudLit == 0 && g.protocol.HudNext != null && g.protocol.GuideOn && g.protocol.Summary.has;
                }
            };
            run(); // warm: the JIT
            Assert.That(g.buyables.candidate, Is.SameAs(g.Item("skybridge")));
            Assert.That(run, UnityEngine.TestTools.Constraints.Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void FullPace_StreamsSquadsThroughPeak_NewRunClearsIt()
        {
            var g = new Rig();
            g.nav.UpdateNow(g.player.x, g.player.y, g.player.z);
            g.director.NewRun();
            g.director.AddSquads(new List<List<string>> { new List<string> { "husk", "husk", "husk" } });
            g.director.state.phase = "PEAK"; // the web's PEAK: no spawns
            g.director.state.intensity = 1;
            g.director.Step(dt);
            Assert.That(g.director.telegraphs, Is.Empty, "PEAK holds the squads");
            Assert.That(g.director.Queue.Count, Is.EqualTo(1));
            g.director.fullPace = true;
            g.director.Step(dt);
            Assert.That(g.director.state.phase, Is.EqualTo("PEAK"));
            Assert.That(g.director.telegraphs.Count, Is.EqualTo(1), "full pace: the squad comes through PEAK");
            Assert.That(g.director.Queue.Count, Is.EqualTo(0));
            g.director.NewRun();
            Assert.That(g.director.fullPace, Is.False);
        }
    }
}
