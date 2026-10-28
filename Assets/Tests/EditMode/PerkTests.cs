// NIGHT8 U2-11: perks. From the web's tests/perks.test.js: the stack tax, every perk's numbers (all 14, pure), FLIP +
// QUICK's faster swap; the buy flow on Buyables + Economy (hold E buys once at the taxed price, the next costs 10 % more,
// too poor is refused, a short hold buys nothing). Unity's buyables don't offer an owned perk at all (the web offers it
// and denies it as 'owned'): a second hold spends nothing either way.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class PerkTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        [Test]
        public void TheStackTax_AddsTenPercentPerPerkOwned_ToTheNearestTen()
        {
            Assert.That(PerkMath.PerkPrice(750, 0, 0.1), Is.EqualTo(750));
            Assert.That(PerkMath.PerkPrice(750, 3, 0.1), Is.EqualTo(980)); // 975 -> 980
            Assert.That(PerkMath.PerkPrice(4000, 13, 0.1), Is.EqualTo(9200));
            Assert.That(PerkMath.PerkPrice(2250, 5, 0.1, true), Is.EqualTo(0));
        }

        [Test]
        public void PerkEffects_NothingOwned_ChangesNothing_AndEachPerkMapsToItsNumbers()
        {
            PerkEffects none = PerkMath.Effects(new List<string>(), cfg);
            Assert.That(none.bonusPips, Is.EqualTo(0));
            Assert.That(none.regen.HasValue, Is.False);
            Assert.That(none.maxHp.HasValue, Is.False);
            Assert.That((none.fireRate, none.swap, none.charge), Is.EqualTo((1.0, 1.0, 1.0)));
            Assert.That((none.bonusRideTime, none.bonusWallJumps, none.recoil, none.points), Is.EqualTo((0.0, 0, 1.0, 1.0)));
            Assert.That((none.slamDamage, none.slamRadius, none.styleDrain, none.styleGain), Is.EqualTo((1.0, 1.0, 1.0, 1.0)));
            Assert.That(none.staticAura || none.phoenix, Is.False);

            PerkEffects One(string id) => PerkMath.Effects(new List<string> { id }, cfg);
            Assert.That((One("steady").assistCone, One("steady").assistLockRange, One("steady").recoil), Is.EqualTo(((double?)1.5, (double?)1.5, 0.7)));
            Expect.CloseTo(One("flip").swap, cfg.Num("weapons.swapTime") / 0.08);
            Assert.That((One("flip").arsenalRecover, One("flip").arsenalPrimedBonus), Is.EqualTo(((double?)2, (double?)0.6)));
            Assert.That(One("bounty").points, Is.EqualTo(1.25));
            Assert.That(One("vigor").bonusPips, Is.EqualTo(1));
            Assert.That((One("slipstream").slideSpeed, One("slipstream").slideFriction, One("slipstream").slideJumpCap), Is.EqualTo(((double?)1.25, (double?)0.75, (double?)1.25)));
            Assert.That(One("mend").regen.Value.delay, Is.EqualTo(0.6));
            Assert.That(One("mend").regen.Value.rate, Is.EqualTo(1.5));
            Assert.That((One("anvil").slamDamage, One("anvil").slamRadius), Is.EqualTo((2.0, 1.5)));
            Assert.That((One("deadeye").headshotMult, One("deadeye").headshotPoints), Is.EqualTo(((double?)1.5, (double?)50)));
            Assert.That(One("plate").maxHp, Is.EqualTo(150));
            Assert.That((One("quick").fireRate, One("quick").swap, One("quick").charge), Is.EqualTo((1.15, 2.0, 0.75)));
            Assert.That((One("flow").styleDrain, One("flow").styleGain), Is.EqualTo((0.6, 1.2)));
            Assert.That((One("grip").bonusRideTime, One("grip").bonusWallJumps), Is.EqualTo((0.6, 1)));
            Assert.That(One("static").staticAura, Is.True);
            Assert.That(One("phoenix").phoenix, Is.True);
            // FLIP and QUICK together: the faster swap wins.
            Expect.CloseTo(PerkMath.Effects(cfg.Strs("perks.order"), cfg).swap, System.Math.Max(2, cfg.Num("weapons.swapTime") / 0.08));
        }

        sealed class Shop
        {
            public readonly EventBus events = new EventBus();
            public readonly Economy economy;
            public readonly Buyables buyables;
            public readonly Perks perks;
            public PerkEffects applied;
            readonly List<string> ids = cfg.Strs("perks.order");

            public Shop(double bank)
            {
                economy = new Economy(cfg, events);
                economy.Start(bank);
                buyables = new Buyables(cfg, events, economy);
                perks = new Perks(cfg, events) { apply = fx => applied = fx };
                for (int i = 0; i < ids.Count; i++)
                {
                    string id = ids[i];
                    buyables.list.Add(new Buyables.Item
                    {
                        id = $"perk:{id}", kind = "perk", name = id.ToUpperInvariant(), position = new Vec3(i * 10, 0, -2), reach = cfg.Num("perks.reach"),
                        price = () => perks.Price(id), available = () => !perks.Owns(id), buy = () => perks.Give(id),
                    });
                }
            }

            /// <summary>Stand in front of machine `id` (yaw 0 looks toward -z) and hold E for `seconds`, then let go.</summary>
            public void BuyAt(string id, double seconds = 0.5)
            {
                var at = new Vec3(ids.IndexOf(id) * 10, 0, 0);
                for (int s = 0; s < seconds / cfg.Dt; s++) buyables.Step(at, 0, true, cfg.Dt);
                buyables.Step(at, 0, false, cfg.Dt);
            }
        }

        [Test]
        public void HoldE_BuysOnceAtTheTaxedPrice_TheNextCostsTenPercentMore()
        {
            var w = new Shop(20000);
            w.BuyAt("vigor");
            Assert.That(w.perks.owned, Is.EqualTo(new List<string> { "vigor" }));
            Assert.That(w.economy.total, Is.EqualTo(20000 - 1500));
            Assert.That(w.applied.bonusPips, Is.EqualTo(1));
            w.BuyAt("vigor");
            Assert.That(w.economy.total, Is.EqualTo(20000 - 1500), "owned: nothing spent");
            double before = w.economy.total;
            w.BuyAt("plate");
            Assert.That(before - w.economy.total, Is.EqualTo(2750)); // 2500 x 1.1
            Assert.That(w.applied.maxHp, Is.EqualTo(150));
            w.perks.Reset();
            Assert.That(w.perks.owned, Is.Empty);
            Assert.That(w.applied.maxHp.HasValue, Is.False, "a new run drops every perk");
        }

        [Test]
        public void TooPoor_IsRefused_AndAShortHoldBuysNothing()
        {
            var w = new Shop(100);
            int denied = 0;
            w.events.On("buyDenied", _ => denied++);
            w.BuyAt("grip");
            Assert.That(w.perks.owned, Is.Empty);
            Assert.That(denied, Is.EqualTo(1));
            Assert.That(w.economy.total, Is.EqualTo(100));
            var rich = new Shop(20000);
            rich.BuyAt("quick", 0.2);
            Assert.That(rich.perks.owned, Is.Empty, "under buy.holdTime buys nothing");
        }

        // --- NIGHT9 U3-11: BOUNTY, DEADEYE, FLOW, SLIPSTREAM (and FLIP, U3-08): their machines and their effects -------------

        [Test, Description("the ten ported perks each have a machine spot on the Survival map (data.perkSpots)")]
        public void TheTenPortedPerks_HaveTheirMachineSpots()
        {
            var map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
            foreach (string id in new[] { "vigor", "mend", "plate", "quick", "grip", "flip", "bounty", "deadeye", "flow", "slipstream" })
            {
                Assert.That(Perks.Ported.Contains(id), Is.True, id);
                Assert.That(map.perkSpots.Exists(s => s.perk == id), Is.True, $"{id} has a spot");
            }
            Assert.That(Perks.Ported.Count, Is.EqualTo(10));
        }

        static double Earned(PerkEffects fx, bool headshot, out double deadeye)
        {
            var events = new EventBus();
            var eco = new Economy(cfg, events);
            eco.Start(0);
            eco.SetPerks(fx.points, fx.headshotPoints ?? 0);
            events.Emit("hit", new HitEvent { weapon = "revolver", kind = "husk", killed = true, headshot = headshot });
            deadeye = eco.byReason.TryGetValue("deadeye", out double d) ? d : 0;
            return eco.total;
        }

        [Test, Description("economy.js setPerks: BOUNTY x every earning; DEADEYE + headshot points per headshot hit (reason deadeye)")]
        public void Bounty_MultipliesEarnings_DeadeyeAddsHeadshotPoints()
        {
            PerkEffects none = PerkMath.Effects(new List<string>(), cfg), bounty = PerkMath.Effects(new List<string> { "bounty" }, cfg), deadeye = PerkMath.Effects(new List<string> { "deadeye" }, cfg);
            double plain = Earned(none, false, out double d0);
            double rich = Earned(bounty, false, out _);
            Assert.That(bounty.points, Is.GreaterThan(1));
            Assert.That(rich, Is.EqualTo(plain * bounty.points).Within(1), "x points (each earning rounded)");
            Assert.That(d0, Is.EqualTo(0));
            Earned(deadeye, true, out double head);
            Assert.That(head, Is.EqualTo(deadeye.headshotPoints.Value).Within(1), "+ the headshot points");
            Earned(deadeye, false, out double body);
            Assert.That(body, Is.EqualTo(0), "a body shot pays no DEADEYE points");
        }

        [Test, Description("DEADEYE x headshot damage through the loadout's damage (weapons.js headshotMult)")]
        public void Deadeye_MultipliesHeadshotDamage()
        {
            var events = new EventBus();
            var enemies = new EnemyRegistry();
            var world = new TriangleWorld();
            world.AddBox(new Vec3(-30, -1, -30), new Vec3(30, 0, 30));
            var mover = new Mover(cfg.movement, world);
            mover.Reset(0, 0, 0);
            var loadout = new Loadout(cfg, events, enemies, world, mover, new Health(100, events, "player"), () => (new Vec3(0, 1.6, 0), new Vec3(0, 0, -1)));
            loadout.arsenal.enabled = false; // base damage
            var husks = new Husks(cfg, events, enemies, world, -30);
            Husk h = husks.Spawn(new Vec3(0, 0, -5), new Vec3(0, 0, 0));
            h.hp = h.maxHp = 1e6;
            var dealt = new List<double>();
            events.On<HitEvent>("hit", e => dealt.Add(e.damage));
            loadout.Damage(h, 10, true, "revolver", new Vec3(0, 1.6, 0));
            PerkEffects fx = PerkMath.Effects(new List<string> { "deadeye" }, cfg);
            loadout.headshotMult = fx.headshotMult ?? 1;
            loadout.Damage(h, 10, true, "revolver", new Vec3(0, 1.6, 0));
            loadout.Damage(h, 10, false, "revolver", new Vec3(0, 1.6, 0));
            Assert.That(fx.headshotMult, Is.GreaterThan(1));
            Assert.That(dealt[1], Is.EqualTo(dealt[0] * fx.headshotMult.Value).Within(1e-9));
            Assert.That(dealt[2], Is.EqualTo(10), "body shots untouched");
        }

        [Test, Description("FLOW (style setPerkMods): x meter gain, x drain")]
        public void Flow_ScalesStyleGainAndDrain()
        {
            PerkEffects fx = PerkMath.Effects(new List<string> { "flow" }, cfg);
            double Meter(bool flow, int steps)
            {
                var events = new EventBus();
                var style = new StyleMeter(cfg, events, () => "GROUND", () => true);
                style.Start();
                if (flow) style.SetPerkMods(fx.styleDrain, fx.styleGain);
                events.Emit("kill", new KillEvent { weapon = "revolver", kind = "husk", distance = 10 });
                for (int i = 0; i < steps; i++) style.Step(cfg.Dt);
                return style.meter;
            }
            Assert.That(fx.styleGain, Is.GreaterThan(1));
            Assert.That(fx.styleDrain, Is.LessThan(1));
            Assert.That(Meter(true, 0), Is.EqualTo(Meter(false, 0) * fx.styleGain).Within(1e-9), "x gain on the award");
            double plainDrop = Meter(false, 0) - Meter(false, 60), flowDrop = Meter(true, 0) - Meter(true, 60);
            Assert.That(flowDrop, Is.LessThan(plainDrop), "drains slower");
        }

        [Test, Description("SLIPSTREAM (mover slideMods): a slide from standstill starts at slide.floorSpeed x its speed")]
        public void Slipstream_RaisesTheSlideFloorSpeed()
        {
            PerkEffects fx = PerkMath.Effects(new List<string> { "slipstream" }, cfg);
            double SlideSpeed(bool slip)
            {
                var world = new TriangleWorld();
                world.AddBox(new Vec3(-60, -1, -60), new Vec3(60, 0, 60));
                var mover = new Mover(cfg.movement, world);
                mover.Reset(0, 0, 0);
                if (slip)
                {
                    mover.slideSpeedMult = fx.slideSpeed ?? 1;
                    mover.slideFrictionMult = fx.slideFriction ?? 1;
                    mover.slideJumpCapMult = fx.slideJumpCap ?? 1;
                }
                var idle = new InputFrame();
                for (int i = 0; i < 30; i++) mover.Step(idle, cfg.Dt); // settle on the floor
                var slide = new InputFrame { moveZ = 1, slide = new Button { held = true, pressed = true } };
                mover.Step(slide, cfg.Dt);
                return mover.Speed;
            }
            Assert.That(fx.slideSpeed, Is.GreaterThan(1));
            double plain = SlideSpeed(false), slip = SlideSpeed(true);
            Assert.That(slip, Is.GreaterThan(plain), $"plain {plain:0.###} m/s, SLIPSTREAM {slip:0.###} m/s");
        }
    }
}
