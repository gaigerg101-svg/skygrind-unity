// NIGHT10 U3-13.8: THE MYSTERY BOX (Core/MysteryBox.cs; new design: CoD Zombies' box, numbers in Design/unity-config.json
// design.box). The flag; CoD's teddy ladder at each band; the spots (three zones, the first the starting zone, open floor
// by the collider); the seeded sequence; a teddy refunds what was paid, moves the box to another spot and resets the
// pulls; no roll offers an owned, melee or unported gun; an empty pool; Take; the sink; the reveal's timing and cycle.
// The Game builds no box when MysteryBox.Enabled(cfg) is false (the web's game).
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class BoxTests
    {
        static GameConfig cfg;
        static WeaponCatalog catalog;
        static readonly Func<string, bool> AllGuns = id => true; // every catalog gun as if ported (N11's pool)

        [OneTimeSetUp]
        public void Load()
        {
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            catalog = WeaponCatalog.From(cfg);
        }

        static int StepsOf(double seconds) => MovementMath.StepsFor(seconds, cfg.Dt);

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly Economy economy;
            public readonly MysteryBox box;
            public readonly List<string> log = new List<string>();

            public Rig(uint seed, double bank = 1e6)
            {
                economy = new Economy(cfg, events);
                economy.Start(bank);
                box = new MysteryBox(cfg, events, economy);
                box.Reset(seed);
                // payloads are reused: read them here
                events.On<BoxOpenEvent>("boxOpen", e => log.Add($"open {e.owner} {e.spot} pull {e.pull} price {e.price}"));
                events.On<BoxRollEvent>("boxRoll", e => log.Add($"roll {e.weapon} at {e.spot} pull {e.pull}"));
                events.On<BoxTeddyEvent>("boxTeddy", e => log.Add($"teddy {e.refund} at {e.spot} pull {e.pull}"));
                events.On<BoxMoveEvent>("boxMove", e => log.Add($"move {e.from} -> {e.to} {e.zone}"));
                events.On<BoxOfferEvent>("boxTake", e => log.Add($"take {e.weapon} at {e.spot}"));
                events.On<BoxOfferEvent>("boxSink", e => log.Add($"sink {e.weapon} at {e.spot}"));
            }

            /// <summary>Buy as Buyables does (the price after a pending REFUND, spent under MysteryBox.ItemId), then roll.</summary>
            public bool Buy(IList<string> owned, Func<string, bool> isPorted)
            {
                double price = economy.Price(box.price);
                if (!economy.Spend(price, MysteryBox.ItemId)) return false;
                return box.Roll(owned, isPorted);
            }

            public void Steps(int n)
            {
                for (int i = 0; i < n; i++) box.Step(cfg.Dt);
            }

            /// <summary>Step until the box is Idle again (a sink or a landing).</summary>
            public void ToIdle()
            {
                for (int i = 0; i < 100000 && box.state != BoxState.Idle; i++) box.Step(cfg.Dt);
                Assert.That(box.state, Is.EqualTo(BoxState.Idle));
            }

            /// <summary>A scripted run: `n` buys; every third offer is taken, the rest sink.</summary>
            public List<string> Script(int n, IList<string> owned, Func<string, bool> isPorted)
            {
                for (int i = 0; i < n; i++)
                {
                    Assert.That(Buy(owned, isPorted), Is.True);
                    Steps(StepsOf(box.revealTime));
                    if (box.state == BoxState.Offering && i % 3 == 0) box.Take();
                    ToIdle();
                }
                return log;
            }
        }

        [Test]
        public void TheFlag_IsDesignBoxEnabled_OffOrMissingMeansNoBox()
        {
            Assert.That(MysteryBox.Enabled(cfg), Is.True, "design.box.enabled ships on (D-20)");
            Assert.That(cfg.Num("design.box.price"), Is.EqualTo(950), "CoD's price");
            string path = Path.Combine(Port.Root, "config.json");
            var off = GameConfig.Load(path);
            PortData.Obj(PortData.Obj(off.root["design"])["box"])["enabled"] = false;
            Assert.That(MysteryBox.Enabled(off), Is.False, "flag off: the Game builds no box (the web's game)");
            Assert.That(MysteryBox.Enabled(GameConfig.FromText(File.ReadAllText(path), null)), Is.False, "no design config: no box");
        }

        [Test]
        public void TheTeddyLadder_CoDsBands_ByPullsSinceTheLastMove()
        {
            var box = new Rig(1).box;
            Assert.That(box.TeddyChance(1), Is.EqualTo(0));
            Assert.That(box.TeddyChance(3), Is.EqualTo(0));
            Assert.That(box.TeddyChance(4), Is.EqualTo(0.15));
            Assert.That(box.TeddyChance(7), Is.EqualTo(0.15));
            Assert.That(box.TeddyChance(8), Is.EqualTo(0.3));
            Assert.That(box.TeddyChance(12), Is.EqualTo(0.3));
            Assert.That(box.TeddyChance(13), Is.EqualTo(0.5));
            Assert.That(box.TeddyChance(30), Is.EqualTo(0.5));
            Assert.That(BoxMath.TeddyChance(5, new double[] { 3, 7, 12 }, new[] { 0, 0.15, 0.3, 0.5 }), Is.EqualTo(0.15));
        }

        [Test]
        public void TheSpots_ThreeZones_TheFirstInTheStartZone_OnOpenFloorAgainstAWall()
        {
            var map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
            var world = new GameWorld(map.BuildOctree(), map.CreateCore(cfg.movement));
            var box = new Rig(1).box;
            Assert.That(box.SpotCount, Is.EqualTo(3));
            Assert.That(box.Spot(0).zone, Is.EqualTo(map.startZone), "the first spot is in the starting zone");
            var zones = new HashSet<string>();
            for (int i = 0; i < box.SpotCount; i++)
            {
                BoxSpot s = box.Spot(i);
                Assert.That(zones.Add(s.zone), Is.True, $"spot {i}: one spot per zone");
                Assert.That(ZoneMath.ZoneIndexAt(map.zoneParts, s.pos.x, s.pos.y + 1, s.pos.z), Is.EqualTo(map.ZoneIndex(s.zone)), $"spot {i} is in {s.zone}");
                Assert.That(world.RayIntersect(new Vec3(s.pos.x, s.pos.y + 3, s.pos.z), new Vec3(0, -1, 0), out RayHit floor), Is.True);
                Assert.That(floor.distance, Is.EqualTo(3).Within(1e-9), $"spot {i} stands on the floor");
                // 1 m of clear space up to 2.2 m (a capsule over the floor touches nothing)
                var cap = new Capsule { start = new Vec3(s.pos.x, s.pos.y + 1.05, s.pos.z), end = new Vec3(s.pos.x, s.pos.y + 1.2, s.pos.z), radius = 1 };
                int n = world.GatherCandidates(cap), hits = 0;
                for (int k = 0; k < n; k++) if (world.CapsuleIntersect(cap, k, out _)) hits++;
                Assert.That(hits, Is.EqualTo(0), $"spot {i} has 1 m of clear space");
                // a wall right behind, room in front
                Vec3 eye = new Vec3(s.pos.x, s.pos.y + 1, s.pos.z), front = new Vec3(Math.Sin(s.yaw), 0, Math.Cos(s.yaw));
                Assert.That(world.RayIntersect(eye, -front, out RayHit back) && back.distance < 1.5, Is.True, $"spot {i} backs onto a wall");
                Assert.That(!world.RayIntersect(eye, front, out RayHit ahead) || ahead.distance > 3, Is.True, $"spot {i} faces open floor");
            }
        }

        [Test]
        public void TheSeededSequence_SameSeedSameRolls_ADifferentSeedDiffers_ResetReplays()
        {
            var owned = new List<string> { "revolver", "gauntlet" };
            List<string> a = new Rig(1234).Script(120, owned, AllGuns);
            List<string> b = new Rig(1234).Script(120, owned, AllGuns);
            Assert.That(b, Is.EqualTo(a));
            Assert.That(a.Exists(l => l.StartsWith("teddy")), Is.True, "120 rolls see a teddy");
            Assert.That(a.Exists(l => l.StartsWith("move")), Is.True);
            Assert.That(new Rig(1235).Script(120, owned, AllGuns), Is.Not.EqualTo(a), "another seed, another run");
            var again = new Rig(99);
            again.Script(30, owned, AllGuns);
            again.box.Reset(1234);
            again.log.Clear();
            Assert.That(again.Script(120, owned, AllGuns), Is.EqualTo(a), "Reset(seed) replays the run");
        }

        [Test]
        public void ATeddy_RefundsWhatWasPaid_MovesTheBoxToAnotherSpot_AndResetsThePulls()
        {
            var r = new Rig(7);
            int buys = 0;
            for (; buys < 500; buys++)
            {
                double before = r.economy.total;
                int from = r.box.spot;
                Assert.That(r.Buy(null, AllGuns), Is.True);
                Assert.That(r.economy.total, Is.EqualTo(before - 950));
                Assert.That(r.box.pulls, Is.EqualTo(buys + 1), "pulls count up until a move");
                r.Steps(StepsOf(r.box.revealTime));
                if (r.box.state == BoxState.Offering)
                {
                    r.ToIdle();
                    continue;
                }
                Assert.That(r.box.state, Is.EqualTo(BoxState.Teddy));
                Assert.That(r.box.pulls, Is.GreaterThan(3), "pulls 1-3 never bring the teddy");
                Assert.That(r.economy.total, Is.EqualTo(before), "the teddy gave the 950 back");
                Assert.That(r.log[r.log.Count - 1], Is.EqualTo($"teddy 950 at {from} pull {buys + 1}"));
                Assert.That(r.box.CanBuy, Is.False);
                r.Steps(StepsOf(r.box.teddyTime) - 1);
                Assert.That(r.box.state, Is.EqualTo(BoxState.Teddy));
                r.Steps(1);
                Assert.That(r.box.state, Is.EqualTo(BoxState.Moving));
                Assert.That(r.box.spot, Is.Not.EqualTo(from), "the box moved to another spot");
                Assert.That(r.box.fromSpot, Is.EqualTo(from));
                Assert.That(r.box.pulls, Is.EqualTo(0), "the ladder starts over");
                Assert.That(r.log[r.log.Count - 1], Is.EqualTo($"move {from} -> {r.box.spot} {r.box.Spot(r.box.spot).zone}"));
                r.Steps(StepsOf(r.box.moveTime));
                Assert.That(r.box.state, Is.EqualTo(BoxState.Idle));
                Assert.That(r.box.CanBuy, Is.True, "buyable again at the new spot");
                break;
            }
            Assert.That(buys, Is.LessThan(500), "a teddy came out");

            // A pending REFUND power-up halves the price; the teddy gives back what was paid, never more.
            var h = new Rig(8);
            for (int i = 0; i < 200; i++)
            {
                h.box.pulls = 100; // 50 % per roll
                h.economy.SetDiscount(0.5);
                double before = h.economy.total;
                double price = h.economy.Price(h.box.price);
                Assert.That(h.economy.Spend(price, MysteryBox.ItemId), Is.True);
                h.economy.SetDiscount(1); // PowerUps clears it on pointsSpent
                Assert.That(h.box.Roll(null, AllGuns), Is.True);
                Assert.That(h.box.paid, Is.EqualTo(475));
                h.Steps(StepsOf(h.box.revealTime));
                bool teddy = h.box.state == BoxState.Teddy;
                h.ToIdle();
                if (!teddy) continue;
                Assert.That(h.economy.total, Is.EqualTo(before));
                Assert.That(h.economy.earned, Is.EqualTo(0), "a refund is not an earning");
                return;
            }
            Assert.Fail("no teddy in 200 rolls at 50 %");
        }

        [Test]
        public void ATeddyOnlyMovesToAnOpenSpot_AndNoneOpenMeansNoTeddy()
        {
            var r = new Rig(11);
            r.box.spotOpen = i => i != 1; // spot 1's zone locked
            int moves = 0;
            for (int i = 0; i < 200; i++)
            {
                r.box.pulls = 100;
                int from = r.box.spot;
                r.Buy(null, AllGuns);
                r.Steps(StepsOf(r.box.revealTime));
                bool teddy = r.box.state == BoxState.Teddy;
                r.ToIdle();
                if (!teddy) continue;
                moves++;
                Assert.That(r.box.spot, Is.Not.EqualTo(1));
                Assert.That(r.box.spot, Is.Not.EqualTo(from));
            }
            Assert.That(moves, Is.GreaterThan(0));

            var stuck = new Rig(12);
            stuck.box.spotOpen = i => i == 0; // nowhere else to go
            for (int i = 0; i < 100; i++)
            {
                stuck.box.pulls = 100;
                stuck.Buy(null, AllGuns);
                stuck.Steps(StepsOf(stuck.box.revealTime));
                Assert.That(stuck.box.state, Is.EqualTo(BoxState.Offering), "no teddy without another open spot");
                stuck.ToIdle();
            }
        }

        [Test]
        public void NoRollEverOffersAnOwnedGun_AMeleeWeapon_OrAnUnportedGun()
        {
            var guns = catalog.order.FindAll(id => catalog.Get(id).kind != "melee");
            Func<string, bool> ported = id => id != "rail" && id != "choir";
            var r = new Rig(21);
            for (int i = 0; i < 400; i++)
            {
                var owned = new List<string> { "gauntlet" };
                for (int k = 0; k < guns.Count; k++) if ((i >> (k % 8) & 1) == 1 && k % 3 != 2) owned.Add(guns[k]);
                if (r.box.PoolCount(owned, ported) == 0) continue;
                Assert.That(r.Buy(owned, ported), Is.True);
                r.Steps(StepsOf(r.box.revealTime));
                if (r.box.state == BoxState.Offering)
                {
                    string g = r.box.Offered;
                    Assert.That(owned, Does.Not.Contain(g));
                    Assert.That(catalog.Get(g).kind, Is.Not.EqualTo("melee"));
                    Assert.That(ported(g), Is.True);
                    Assert.That(r.box.Take(), Is.EqualTo(g));
                }
                r.ToIdle();
            }

            // Tonight's arsenal: KESTREL and THUNDERCLAP fire, the start loadout owns KESTREL: every offer is THUNDERCLAP.
            var t = new Rig(22);
            var start = cfg.Strs("weapons.survivalStart");
            Func<string, bool> two = id => id == "revolver" || id == "shotgun";
            Assert.That(t.box.PoolCount(start, two), Is.EqualTo(1));
            for (int i = 0; i < 40; i++)
            {
                t.Buy(start, two);
                t.Steps(StepsOf(t.box.revealTime));
                if (t.box.state == BoxState.Offering) Assert.That(t.box.Offered, Is.EqualTo("shotgun"));
                t.ToIdle();
            }
        }

        [Test]
        public void AnEmptyPool_RefundsThePrice_AndChangesNothing()
        {
            var r = new Rig(31);
            var owned = catalog.order.FindAll(id => catalog.Get(id).kind != "melee");
            Assert.That(r.box.PoolCount(owned, AllGuns), Is.EqualTo(0));
            double before = r.economy.total;
            Assert.That(r.Buy(owned, AllGuns), Is.False);
            Assert.That(r.economy.total, Is.EqualTo(before), "refunded");
            Assert.That((r.box.state, r.box.pulls, r.box.totalPulls), Is.EqualTo((BoxState.Idle, 0, 0)));
            Assert.That(r.log, Is.Empty, "the box never opened");
            // the stream is untouched: the next roll is the one a fresh box makes
            var fresh = new Rig(31);
            r.Script(10, null, AllGuns);
            Assert.That(fresh.Script(10, null, AllGuns), Is.EqualTo(r.log));
        }

        [Test]
        public void Take_ReturnsTheGunOnce_OnlyWhileOffering_OnlyForTheRoller()
        {
            var r = new Rig(41);
            Assert.That(r.box.Take(), Is.Null, "nothing to take");
            r.Buy(null, AllGuns); // pull 1: never a teddy
            Assert.That(r.box.state, Is.EqualTo(BoxState.Revealing));
            Assert.That(r.box.Take(), Is.Null, "not during the reveal");
            Assert.That(r.box.Offered, Is.Null);
            r.Steps(StepsOf(r.box.revealTime));
            Assert.That(r.box.state, Is.EqualTo(BoxState.Offering));
            string g = r.box.Offered;
            Assert.That(g, Is.Not.Null);
            Assert.That(r.log[r.log.Count - 1], Is.EqualTo($"roll {g} at 0 pull 1"));
            Assert.That(r.box.Take("p2"), Is.Null, "only the roller takes it");
            Assert.That(r.box.Take(), Is.EqualTo(g));
            Assert.That(r.box.state, Is.EqualTo(BoxState.Idle));
            Assert.That(r.box.Take(), Is.Null, "once");
            Assert.That(r.log[r.log.Count - 1], Is.EqualTo($"take {g} at 0"));
        }

        [Test]
        public void AnUntakenOffer_SinksAfterOfferTime()
        {
            var r = new Rig(51);
            r.Buy(null, AllGuns);
            r.Steps(StepsOf(r.box.revealTime));
            string g = r.box.Offered;
            int offer = StepsOf(r.box.offerTime);
            r.Steps(offer - 1);
            Assert.That(r.box.state, Is.EqualTo(BoxState.Offering));
            Assert.That(r.box.OfferLeft, Is.EqualTo(cfg.Dt).Within(1e-12));
            r.Steps(1);
            Assert.That(r.box.state, Is.EqualTo(BoxState.Idle));
            Assert.That(r.box.spot, Is.EqualTo(0), "a sink doesn't move the box");
            Assert.That(r.log[r.log.Count - 1], Is.EqualTo($"sink {g} at 0"));
            Assert.That(r.box.pulls, Is.EqualTo(1), "a sink doesn't reset the pulls");
        }

        [Test]
        public void TheReveal_LastsRevealTime_CyclesThePortedGuns_ThenShowsTheOffer()
        {
            var r = new Rig(61);
            Func<string, bool> ported = id => id == "revolver" || id == "shotgun" || id == "rocket";
            r.Buy(new List<string> { "revolver" }, ported);
            Assert.That(r.log[0], Is.EqualTo("open player 0 pull 1 price 950"));
            int reveal = StepsOf(r.box.revealTime), cycle = StepsOf(r.box.cycleTime);
            Assert.That(r.box.RevealProgress, Is.EqualTo(0));
            var shown = new HashSet<string>();
            for (int i = 0; i < reveal - 1; i++)
            {
                string d = r.box.DisplayWeapon;
                Assert.That(ported(d), Is.True, "the cycle shows ported guns (owned ones too)");
                shown.Add(d);
                r.Steps(1);
                Assert.That(r.box.state, Is.EqualTo(BoxState.Revealing));
            }
            Assert.That(shown, Is.EquivalentTo(new[] { "revolver", "shotgun", "rocket" }));
            Assert.That(r.box.RevealProgress, Is.EqualTo(1 - 1.0 / reveal).Within(1e-12));
            r.Steps(1);
            Assert.That(r.box.state, Is.EqualTo(BoxState.Offering));
            Assert.That(r.box.RevealProgress, Is.EqualTo(1));
            Assert.That(r.box.DisplayWeapon, Is.EqualTo(r.box.Offered));
            Assert.That(new[] { "shotgun", "rocket" }, Does.Contain(r.box.Offered), "not the owned KESTREL");
            Assert.That(cycle, Is.GreaterThan(1));
        }

        [Test]
        public void ABusyBox_RefusesARoll_AndRefundsIt()
        {
            var r = new Rig(71);
            r.Buy(null, AllGuns);
            double before = r.economy.total;
            Assert.That(r.Buy(null, AllGuns), Is.False, "the box is mid-reveal");
            Assert.That(r.economy.total, Is.EqualTo(before));
            Assert.That(r.box.pulls, Is.EqualTo(1));
        }
    }
}
