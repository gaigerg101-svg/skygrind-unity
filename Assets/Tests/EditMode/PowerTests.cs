// NIGHT11 U3-24.6: THE POWER SWITCH (Core/Power.cs; new design: CoD Zombies' power switch, design.power). The flag off is
// the web's game (no switch, no event, everything powered); on, the switch is a free hold-E buy that throws once (powerOn
// and the banner), a new run turns it off, and its spot is HEIGHTS floor two gates from the spawn, clear and flat.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class PowerTests
    {
        static GameConfig cfg;

        [OneTimeSetUp]
        public void Load() => cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));

        static GameConfig Off()
        {
            var off = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            PortData.Obj(PortData.Obj(off.root["design"])["power"])["enabled"] = false;
            return off;
        }

        sealed class Rig
        {
            public readonly EventBus events = new EventBus();
            public readonly Economy economy;
            public readonly Buyables buy;
            public readonly Power power;
            public readonly List<string> log = new List<string>();

            public Rig(GameConfig c)
            {
                economy = new Economy(c, events);
                economy.Start(0); // a free switch needs no points
                buy = new Buyables(c, events, economy);
                power = new Power(c, events);
                if (power.Item != null) buy.list.Add(power.Item);
                events.On<PowerEvent>("powerOn", e => log.Add("powerOn " + e.owner));
                events.On<BannerEvent>("banner", e => log.Add("banner " + e.text));
            }

            /// <summary>Stand 1.5 m in front of the switch, facing it, and hold E for `steps` fixed steps.</summary>
            public void Hold(int steps)
            {
                BoxSpot s = power.spot;
                var feet = new Vec3(s.pos.x + System.Math.Sin(s.yaw) * 1.5, s.pos.y, s.pos.z + System.Math.Cos(s.yaw) * 1.5);
                double yaw = System.Math.Atan2(System.Math.Sin(s.yaw), System.Math.Cos(s.yaw)); // the look (-sin yaw, -cos yaw) points back at the switch
                for (int i = 0; i < steps; i++) buy.Step(feet, yaw, true, cfg.Dt);
            }

            public void Release() => buy.Step(default, 0, false, cfg.Dt);
        }

        static int HoldSteps => MovementMath.StepsFor(cfg.Num("buy.holdTime"), cfg.Dt);

        [Test]
        public void FlagOff_NoSwitch_NoEvent_AndEverythingIsPowered()
        {
            Assert.That(Power.Enabled(cfg), Is.True, "design.power.enabled ships on");
            var r = new Rig(Off());
            Assert.That(r.power.enabled, Is.False);
            Assert.That(r.power.Item, Is.Null, "no Buyables item");
            Assert.That(r.power.Powered, Is.True, "flag off: the web's rules (traps always buyable, the PROTOCOL at its wake round)");
            Assert.That(r.power.CanThrow(), Is.False);
            Assert.That(r.power.Throw(), Is.False);
            r.Hold(HoldSteps * 3);
            Assert.That(r.log, Is.Empty, "no powerOn, no banner");
            Assert.That(r.events.Emitted.ContainsKey("powerOn"), Is.False);
            var noDesign = GameConfig.FromText(File.ReadAllText(Path.Combine(Port.Root, "config.json")), null);
            Assert.That(Power.Enabled(noDesign), Is.False, "no design config: no switch");
            Assert.That(new Power(noDesign, new EventBus()).Item, Is.Null);
        }

        [Test]
        public void On_TheSwitchIsAFreeHoldEBuy_ThrownOnce_PowerOnAndTheBanner()
        {
            var r = new Rig(cfg);
            Power p = r.power;
            Assert.That(p.Item, Is.Not.Null);
            Assert.That(p.Item.price(), Is.EqualTo(0), "free");
            Assert.That(p.Item.reach, Is.EqualTo(cfg.Num("perks.reach")));
            Assert.That(p.Item.position.x, Is.EqualTo(p.spot.pos.x));
            Assert.That(p.Item.position.z, Is.EqualTo(p.spot.pos.z));
            Assert.That(p.On, Is.False);
            Assert.That(p.Powered, Is.False, "flag on: off until thrown");
            r.Hold(HoldSteps - 1);
            Assert.That(p.On, Is.False, "not before buy.holdTime");
            r.Hold(1);
            Assert.That(p.On, Is.True);
            Assert.That(p.Powered, Is.True);
            Assert.That(r.log, Is.EqualTo(new[] { "powerOn player", "banner " + Power.BannerText }));
            Assert.That(r.economy.total, Is.EqualTo(0), "it cost nothing");
            Assert.That(p.Item.available(), Is.False, "thrown: no longer offered");
            r.Release();
            r.Hold(HoldSteps * 2);
            Assert.That(r.log.Count, Is.EqualTo(2), "once a run");
            Assert.That(p.Throw(), Is.False);
        }

        [Test]
        public void Reset_TurnsThePowerOff_AndALockedZoneHidesTheSwitch()
        {
            var r = new Rig(cfg);
            Power p = r.power;
            Assert.That(p.Throw("player"), Is.True);
            p.Reset();
            Assert.That(p.On, Is.False, "a new run starts with the power off");
            Assert.That(p.CanThrow(), Is.True);
            bool open = false;
            p.zoneOpen = id => id == p.spot.zone && open;
            Assert.That(p.CanThrow(), Is.False, "its zone (HEIGHTS) is locked");
            open = true;
            Assert.That(p.CanThrow(), Is.True);
            r.Hold(HoldSteps);
            Assert.That(p.On, Is.True);
        }

        [Test]
        public void TheSpot_IsHeightsFloor_TwoGatesFromTheSpawn_FlatAndClear()
        {
            var map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
            var world = new GameWorld(map.BuildOctree(), map.CreateCore(cfg.movement));
            BoxSpot s = new Power(cfg, new EventBus()).spot;
            Assert.That(s.zone, Is.EqualTo("heights"));
            Assert.That(ZoneMath.ZoneIndexAt(map.zoneParts, s.pos.x, s.pos.y + 1, s.pos.z), Is.EqualTo(map.ZoneIndex("heights")));
            // The gate graph: HEIGHTS is bought from SLAB GARDENS, which is bought from LANDING (the start zone).
            int GateFrom(string zone)
            {
                foreach (ZoneSpot g in map.zones[map.ZoneIndex(zone)].spots)
                {
                    int z = ZoneMath.ZoneIndexAt(map.zoneParts, g.pos.x, g.pos.y + 1, g.pos.z);
                    if (z >= 0 && map.zones[z].id == map.startZone) return 0;
                }
                return -1;
            }
            Assert.That(GateFrom("slabs"), Is.EqualTo(0), "SLAB GARDENS: one gate from LANDING");
            Assert.That(GateFrom("heights"), Is.EqualTo(-1), "HEIGHTS: no gate from LANDING");
            bool heightsFromSlabs = false;
            foreach (ZoneSpot g in map.zones[map.ZoneIndex("heights")].spots)
                if (ZoneMath.ZoneIndexAt(map.zoneParts, g.pos.x, g.pos.y + 1, g.pos.z) == map.ZoneIndex("slabs")) heightsFromSlabs = true;
            Assert.That(heightsFromSlabs, Is.True, "HEIGHTS: bought from SLAB GARDENS (two gates)");
            // Floor, flat over 1 m, clear up to 2.2 m (the probe's checks)
            Assert.That(world.RayIntersect(new Vec3(s.pos.x, s.pos.y + 3, s.pos.z), new Vec3(0, -1, 0), out RayHit floor), Is.True);
            Assert.That(floor.distance, Is.EqualTo(3).Within(1e-9), "on the floor");
            for (int i = 0; i < 8; i++)
            {
                double a = i * System.Math.PI / 4;
                Assert.That(world.RayIntersect(new Vec3(s.pos.x + System.Math.Cos(a), s.pos.y + 3, s.pos.z + System.Math.Sin(a)), new Vec3(0, -1, 0), out RayHit h) && System.Math.Abs(h.distance - 3) < 1e-9, Is.True, $"flat at angle {i}");
            }
            var cap = new Capsule { start = new Vec3(s.pos.x, s.pos.y + 1.05, s.pos.z), end = new Vec3(s.pos.x, s.pos.y + 1.2, s.pos.z), radius = 1 };
            int n = world.GatherCandidates(cap), hits = 0;
            for (int k = 0; k < n; k++) if (world.CapsuleIntersect(cap, k, out _)) hits++;
            Assert.That(hits, Is.EqualTo(0), "1 m of clear space");
            // Clear of the pad landing and the ISLANDS gate (4.5 m), as the probe found.
            foreach (PadSpec pad in map.pads)
                if (pad.aimed) Assert.That(Vec3.DistanceTo(pad.target, s.pos), Is.GreaterThan(4.5));
        }
    }
}
