// Port of skygrind/src/survival/zones.js (the rules; the drawing is ZonesView in Skygrind.Game). Every zone but the
// start one is locked: its parts are a solid force field in the world collider (layer 'gates'), so wall jumps, pads,
// dash jumps and rocket jumps can't enter it. Buying (hold BUY at a spot) opens the zone: the field goes at once and
// gateOpen fires. Pads that launch into a locked zone sleep until it opens. Spawn nodes are sampled on every walkable
// surface at load; enemies only spawn on nodes in open zones.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public struct SpawnNode
    {
        public double x, y, z;
        public int zone;
        public bool high;
        public bool core;
    }

    public sealed class BuySpot
    {
        public int zone;
        public string zoneId;
        public string name;
        public ZoneSpot spot;
        public Vec3 label; // where the price sign floats
        public Vec3 at; // the buy point: on the gate, a step past the spot
        public double price;
    }

    /// <summary>NIGHT24: a door locked again (Zones.Lock).</summary>
    public sealed class GateLockEvent
    {
        public string zone, name;
    }

    public sealed class Zones : IRunState
    {
        // NIGHT13 U3-26.10: the run snapshot: the locks, the zones seen (zoneEnter fires once each), the zone you were in.
        // Load re-applies the locks (the gate layer's boxes, the pads, Version: the view rebuilds) and emits nothing (the
        // nav field's mask is the "nav" section's own).
        public void Save(StateWriter w)
        {
            w.Bools("locked", locked);
            w.Bools("seen", seen);
            w.I("here", here);
        }

        public void Load(StateReader r)
        {
            List<bool> l = r.Bools("locked"), s = r.Bools("seen");
            if (l.Count != locked.Length || s.Count != seen.Length) throw new StateException($"snapshot: zones: {l.Count} saved, {locked.Length} on this map");
            for (int z = 0; z < locked.Length; z++)
            {
                locked[z] = l[z];
                seen[z] = s[z];
            }
            here = r.Int("here");
            ApplyLocks();
        }

        readonly GameConfig cfg;
        readonly SurvivalMap map;
        readonly GameWorld world;
        readonly EventBus events;
        readonly BoxLayer layer = new BoxLayer();
        readonly bool[] locked;
        readonly bool[] seen;
        int here = -1;
        public readonly List<SpawnNode> nodes = new List<SpawnNode>();
        public readonly List<BuySpot> spots = new List<BuySpot>();
        public readonly bool[] padEnabled;

        /// <summary>Bumped on every lock change (the view rebuilds its gate mesh).</summary>
        public int Version { get; private set; }

        public IReadOnlyList<ZonePart> Parts => map.zoneParts;

        public Zones(GameConfig cfg, SurvivalMap map, GameWorld world, EventBus events)
        {
            this.cfg = cfg;
            this.map = map;
            this.world = world;
            this.events = events;
            locked = new bool[map.zones.Count];
            seen = new bool[map.zones.Count];
            padEnabled = new bool[map.pads.Count];
            SampleNodes(); // before the force fields join the collider
            world.SetLayer("gates", layer);
            for (int z = 0; z < map.zones.Count; z++)
            {
                ZoneDef zone = map.zones[z];
                foreach (ZoneSpot s in zone.spots)
                {
                    double labelForward = cfg.Num("zones.labelForward"), buyForward = cfg.Num("zones.buyForward");
                    spots.Add(new BuySpot
                    {
                        zone = z,
                        zoneId = zone.id,
                        name = zone.name,
                        spot = s,
                        label = new Vec3(s.pos.x + s.faceX * labelForward, s.pos.y + cfg.Num("zones.labelHeight"), s.pos.z + s.faceZ * labelForward),
                        at = new Vec3(s.pos.x + s.faceX * buyForward, s.pos.y, s.pos.z + s.faceZ * buyForward),
                        price = Cost(zone.id),
                    });
                }
            }
            Reset();
        }

        public bool IsLocked(int z) => z >= 0 && z < locked.Length && locked[z];

        public bool IsOpen(string id)
        {
            int z = map.ZoneIndex(id);
            return z >= 0 && !locked[z];
        }

        /// <summary>Walkable spots on every surface (a husk capsule fits), sampled top-down through each column.</summary>
        void SampleNodes()
        {
            nodes.Clear();
            ICollisionWorld geo = world.Geometry;
            double r = cfg.Num("husk.radius");
            double height = cfg.Num("husk.height");
            double half = map.floorSize / 2 - 2;
            double spacing = cfg.Num("zones.nodeSpacing");
            double highMin = cfg.Num("zones.highNodeMin");
            double floorMin = cfg.movement.surface.floorMinNormalY;
            CoreSpec? core = map.core;
            for (double x = -half; x <= half; x += spacing)
            {
                for (double z = -half; z <= half; z += spacing)
                {
                    double y = 39.5;
                    for (int guard = 0; guard < 8 && y > -1; guard++)
                    {
                        if (!geo.RayIntersect(new Vec3(x, y, z), new Vec3(0, -1, 0), out RayHit hit)) break;
                        double sy = y - hit.distance;
                        if (hit.normal.y >= floorMin)
                        {
                            var capsule = new Capsule { start = new Vec3(x, sy + r + 0.05, z), end = new Vec3(x, sy + height - r, z), radius = r };
                            int zone = ZoneMath.ZoneIndexAt(map.zoneParts, x, sy + 1, z);
                            if (zone >= 0 && !WorldCollider.CapsuleIntersect(geo, capsule, out _, out _))
                            {
                                bool inCore = core.HasValue && x > core.Value.x0 && x < core.Value.x0 + core.Value.cells * core.Value.cellSize &&
                                              z > core.Value.z0 && z < core.Value.z0 + core.Value.cells * core.Value.cellSize;
                                nodes.Add(new SpawnNode { x = x, y = sy, z = z, zone = zone, high = sy >= highMin, core = inCore });
                            }
                        }
                        y = sy - 0.6;
                    }
                }
            }
        }

        /// <summary>Lock state -> collision, pads, version.</summary>
        void ApplyLocks()
        {
            var solid = new List<(Vec3, Vec3)>();
            foreach (ZonePart p in map.zoneParts) if (locked[p.zone]) solid.Add((p.min, p.max));
            layer.SetBoxes(solid);
            for (int i = 0; i < map.pads.Count; i++)
            {
                PadSpec pad = map.pads[i];
                Vec3 t = pad.aimed ? pad.target : pad.pos;
                int zone = ZoneMath.ZoneIndexAt(map.zoneParts, t.x, t.y + 1, t.z);
                padEnabled[i] = zone < 0 || !locked[zone];
            }
            Version++;
        }

        /// <summary>Lock every zone but the start one (a new run).</summary>
        public void Reset()
        {
            for (int z = 0; z < locked.Length; z++)
            {
                locked[z] = map.zones[z].id != map.startZone;
                seen[z] = false;
            }
            here = -1;
            ApplyLocks();
            events.Emit("zoneClear");
        }

        /// <summary>Open zone `id` (a buy, or free). False if already open.</summary>
        public bool Open(string id, bool free = false)
        {
            int z = map.ZoneIndex(id);
            if (z < 0 || !locked[z]) return false;
            locked[z] = false;
            ApplyLocks();
            ZoneDef zone = map.zones[z];
            events.Emit("gateOpen", new GateOpenEvent { zone = zone.id, name = zone.name, cost = free ? 0 : Cost(zone.id), free = free });
            return true;
        }

        /// <summary>A zone's gate price: a Unity-authored map's own (data.zones[i].cost, NIGHT15), else config zones.costs (the web's), else 0.</summary>
        public double Cost(string id)
        {
            int z = map.ZoneIndex(id);
            if (z >= 0 && map.zones[z].cost.HasValue) return map.zones[z].cost.Value;
            return cfg.Has($"zones.costs.{id}") ? cfg.Num($"zones.costs.{id}") : 0;
        }

        /// <summary>NIGHT24 (the CAMPAIGN's doors): lock zone `id` again. The web's zones only ever open; a campaign door locks behind the player
        /// when a room's fight starts. Its parts become a solid force field in the collider at once (the caller makes sure nobody stands in
        /// it: the campaign locks a door only once the player is metres past it). False if already locked or unknown. Emits "gateLock"
        /// (port-only; the sim re-masks the nav). Save / Load already carry the locks.</summary>
        public bool Lock(string id)
        {
            int z = map.ZoneIndex(id);
            if (z < 0 || locked[z]) return false;
            locked[z] = true;
            ApplyLocks();
            events.Emit("gateLock", new GateLockEvent { zone = map.zones[z].id, name = map.zones[z].name });
            return true;
        }

        public void OpenAll()
        {
            foreach (ZoneDef zone in map.zones) Open(zone.id, free: true);
        }

        Func<int, bool> isLocked; // IsLocked as a delegate, made once (the buy prompt and the signs ask every step / frame; U2-03)

        public bool SpotBuyable(BuySpot s) => ZoneMath.SpotBuyable(map.zoneParts, s.spot.pos, s.zone, isLocked ??= IsLocked);

        public List<GateQuad> GateQuads() => ZoneMath.GateQuads(map.zoneParts, IsLocked, cfg.Num("zones.gateCell"));

        public string ZoneAt(Vec3 p)
        {
            int z = ZoneMath.ZoneIndexAt(map.zoneParts, p.x, p.y, p.z);
            return z >= 0 ? map.zones[z].id : "";
        }

        /// <summary>Zone index at a floor point (its air 1 m up): the nav grid's tag.</summary>
        public int TagAt(double x, double y, double z) => ZoneMath.ZoneIndexAt(map.zoneParts, x, y + 1, z);

        /// <summary>Is zone index `tag` locked (the nav grid masks its nodes)?</summary>
        public bool LockedTag(int tag) => tag >= 0 && tag < locked.Length && locked[tag];

        public bool LockedAt(Vec3 p)
        {
            int z = ZoneMath.ZoneIndexAt(map.zoneParts, p.x, p.y, p.z);
            return z >= 0 && locked[z];
        }

        /// <summary>Spawn points in open zones (core nodes ride the pillars).</summary>
        public void SpawnPoints(List<SpawnNode> ground, List<SpawnNode> high, CoreGrid coreGrid)
        {
            ground.Clear();
            high.Clear();
            for (int i = 0; i < nodes.Count; i++)
            {
                SpawnNode n = nodes[i];
                if (locked[n.zone]) continue;
                if (n.core && coreGrid != null) n.y = coreGrid.TopAt(n.x, n.z) ?? 0;
                (n.high ? high : ground).Add(n);
            }
        }

        /// <summary>Frame time: 'zoneEnter' on the first step into each zone in a run.</summary>
        public void Track(Vec3 feet)
        {
            int z = ZoneMath.ZoneIndexAt(map.zoneParts, feet.x, feet.y + 1, feet.z);
            if (z >= 0 && z != here && !seen[z])
            {
                seen[z] = true;
                ZoneDef zone = map.zones[z];
                uint accent = cfg.Has($"zones.accents.{zone.id}") ? (uint)cfg.Num($"zones.accents.{zone.id}") : 0xffffff;
                events.Emit("zoneEnter", new ZoneEnterEvent { id = zone.id, name = zone.name, color = accent });
            }
            if (z >= 0) here = z;
        }
    }
}
