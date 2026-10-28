// NIGHT7 S3: the Survival map on the C# web collider. Locked zones are solid (a walk into the ARMORY gate stops at its
// face), buying opens them, spawn nodes exist only where a husk fits and sit in a zone, pads into locked zones sleep.
using System.IO;
using NUnit.Framework;

namespace Skygrind.Core.Tests
{
    public class SurvivalWorldTests
    {
        static GameConfig cfg;
        static SurvivalMap map;

        static void Load()
        {
            if (cfg != null) return;
            cfg = GameConfig.Load(Path.Combine(Port.Root, "config.json"));
            map = SurvivalMap.FromExport(PortData.Obj(PortData.LoadJson(Path.Combine(Port.Root, "maps", "survival.json"))));
        }

        static (GameWorld world, Zones zones, EventBus events) Fresh()
        {
            Load();
            var world = new GameWorld(map.BuildOctree(), map.CreateCore(cfg.movement));
            var events = new EventBus();
            return (world, new Zones(cfg, map, world, events), events);
        }

        /// <summary>Walks +x from the ARMORY buy spot for `seconds`; returns the farthest x.</summary>
        static double WalkEast(ICollisionWorld world, double seconds)
        {
            var m = new Mover(cfg.movement, world);
            m.Reset(12.3, 0, 41);
            double dt = cfg.Dt;
            var f = new InputFrame { yaw = -System.Math.PI / 2, moveZ = 1 }; // yaw -90 deg looks toward +x
            double far = 0;
            for (int i = 0; i < (int)(seconds / dt); i++)
            {
                m.Step(f, dt);
                far = System.Math.Max(far, m.position.x);
            }
            return far;
        }

        [Test]
        public void ALockedGate_IsSolid_AndBuyingItOpensIt()
        {
            var (world, zones, events) = Fresh();
            Assert.That(zones.IsOpen("landing"), Is.True);
            Assert.That(zones.IsOpen("armory"), Is.False);
            double r = cfg.movement.body.capsuleRadius;
            double blocked = WalkEast(world, 2);
            Assert.That(blocked, Is.LessThanOrEqualTo(14 - r + 1e-6), "the ARMORY gate (x = 14) stops the body at its face");
            string opened = null;
            events.On<GateOpenEvent>("gateOpen", e => opened = e.zone);
            Assert.That(zones.Open("armory"), Is.True);
            Assert.That(opened, Is.EqualTo("armory"));
            Assert.That(WalkEast(world, 2), Is.GreaterThan(16), "open: the walk goes through");
        }

        [Test]
        public void SpawnNodes_FitAHusk_AndOnlyOpenZonesSpawn()
        {
            var (world, zones, _) = Fresh();
            Assert.That(zones.nodes.Count, Is.GreaterThan(50));
            foreach (SpawnNode n in zones.nodes) Assert.That(n.zone, Is.InRange(0, map.zones.Count - 1));
            var ground = new System.Collections.Generic.List<SpawnNode>();
            var high = new System.Collections.Generic.List<SpawnNode>();
            zones.SpawnPoints(ground, high, world.core);
            int landing = map.ZoneIndex("landing");
            foreach (SpawnNode n in ground) Assert.That(n.zone, Is.EqualTo(landing));
            foreach (SpawnNode n in high) Assert.That(n.zone, Is.EqualTo(landing));
            Assert.That(ground.Count + high.Count, Is.GreaterThan(0), "LANDING has spawn points");
            zones.OpenAll();
            zones.SpawnPoints(ground, high, world.core);
            Assert.That(ground.Count + high.Count, Is.EqualTo(zones.nodes.Count));
        }

        [Test]
        public void PadsIntoLockedZones_Sleep()
        {
            var (_, zones, _) = Fresh();
            bool anyAsleep = false;
            for (int i = 0; i < map.pads.Count; i++) if (!zones.padEnabled[i]) anyAsleep = true;
            Assert.That(anyAsleep, Is.True);
            zones.OpenAll();
            for (int i = 0; i < map.pads.Count; i++) Assert.That(zones.padEnabled[i], Is.True);
        }

        [Test]
        public void GateQuads_ShowOnlyWhereLockedMeetsOpen()
        {
            var (_, zones, _) = Fresh();
            var quads = zones.GateQuads();
            Assert.That(quads.Count, Is.GreaterThan(0));
            int landing = map.ZoneIndex("landing");
            foreach (GateQuad q in quads) Assert.That(q.zone, Is.Not.EqualTo(landing));
            zones.OpenAll();
            Assert.That(zones.GateQuads().Count, Is.EqualTo(0));
        }
    }
}
