// NIGHT7 S7: hold BUY at a gate: nothing without the points (buyDenied), then with them the zone opens after
// buy.holdTime, the price is spent, gateOpen fires; one buy per press.
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class BuyTests
    {
        [Test]
        public void HoldBuy_OpensTheArmoryGate()
        {
            var cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            var map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
            var events = new EventBus();
            var world = new GameWorld(map.BuildOctree(), map.CreateCore(cfg.movement));
            var zones = new Zones(cfg, map, world, events);
            var economy = new Economy(cfg, events);
            economy.Start(100);
            var buy = new Buyables(cfg, events, economy);
            foreach (BuySpot s in zones.spots)
            {
                BuySpot spot = s;
                buy.list.Add(new Buyables.Item { id = spot.zoneId, name = spot.name, position = spot.at, reach = cfg.Num("zones.buyReach"), price = () => spot.price, available = () => zones.SpotBuyable(spot), buy = () => zones.Open(spot.zoneId) });
            }
            var feet = new Vec3(12.3, 0, 41); // the ARMORY spot, facing +x (yaw -90 deg)
            double yaw = -System.Math.PI / 2, dt = cfg.Dt;
            int denied = 0, opened = 0;
            events.On("buyDenied", _ => denied++);
            events.On("gateOpen", _ => opened++);
            for (int i = 0; i < 120; i++) buy.Step(feet, yaw, true, dt);
            Assert.That(buy.candidate?.id, Is.EqualTo("armory"));
            Assert.That(denied, Is.EqualTo(1), "100 points: denied once (one per press)");
            Assert.That(zones.IsOpen("armory"), Is.False);
            buy.Step(feet, yaw, false, dt); // release
            economy.total = 2000;
            int hold = MovementMath.StepsFor(cfg.Num("buy.holdTime"), dt);
            for (int i = 0; i < hold - 1; i++) buy.Step(feet, yaw, true, dt);
            Assert.That(zones.IsOpen("armory"), Is.False, "not before the hold time");
            buy.Step(feet, yaw, true, dt);
            Assert.That(zones.IsOpen("armory"), Is.True);
            Assert.That(opened, Is.EqualTo(1));
            Assert.That(economy.total, Is.EqualTo(2000 - cfg.Num("zones.costs.armory")));
        }
    }
}
