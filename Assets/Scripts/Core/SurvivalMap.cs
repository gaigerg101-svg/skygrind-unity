// The Survival map as the web game exports it (PortExport/maps/survival.json: arena.load() over
// skygrind/src/world/maps/survival.js, plus the resolved zone parts and the nav grid). Pure data + the collider build.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public struct MapBox
    {
        public Vec3 min;
        public Vec3 max;
        public string kind;
    }

    public struct ZoneSpot
    {
        public Vec3 pos;
        public double faceX;
        public double faceZ;
    }

    /// <summary>A perk machine's spot (map data.perkSpots): the perk, feet, the yaw it faces, riding a core pillar.</summary>
    public struct PerkSpot
    {
        public string perk;
        public Vec3 pos;
        public double yaw;
        public bool core;
    }

    /// <summary>NIGHT11: a PROTOCOL relay's spot (map data.relaySpots / secretRelay): id, name, feet.</summary>
    public struct RelaySpot
    {
        public string id;
        public string name;
        public Vec3 pos;
    }

    /// <summary>NIGHT11: a trap's spot (map data.trapSpots): the trap id, its centre (feet), its switch at (sx, y, sz).</summary>
    public struct TrapSpot
    {
        public string trap;
        public Vec3 pos;
        public double sx;
        public double sz;
    }

    public sealed class ZoneDef
    {
        public string id;
        public string name;
        public readonly List<ZoneSpot> spots = new List<ZoneSpot>();
        /// <summary>NIGHT15 U3-28.6: a Unity-authored map's gate price (data.zones[i].cost); null on the web's maps (config zones.costs).</summary>
        public double? cost;
    }

    public sealed class SurvivalMap
    {
        public string name;
        public double floorSize;
        public double killPlaneY;
        public readonly List<MapBox> boxes = new List<MapBox>(); // render: map.boxes (the floor is separate)
        public readonly List<MapBox> strips = new List<MapBox>(); // render: the emissive ride strips
        public readonly List<MapBox> barriers = new List<MapBox>();
        public readonly List<WedgeSpec> wedges = new List<WedgeSpec>();
        public MapBox floor;
        public readonly List<PadSpec> pads = new List<PadSpec>();
        public readonly List<Vec3> grappleNodes = new List<Vec3>();
        public readonly List<Vec3> groundSpawns = new List<Vec3>();
        public readonly List<Vec3> highSpawns = new List<Vec3>();
        public Vec3 playerSpawn;
        public double playerSpawnYawDeg;
        public CoreSpec? core;
        public readonly List<ZoneDef> zones = new List<ZoneDef>();
        public string startZone;
        public readonly List<ZonePart> zoneParts = new List<ZonePart>();
        public Dictionary<string, object> navGrid; // raw (NavGrid.FromExport reads it)
        public double armoryWallX, armoryWallY, armoryWallFacing; // wallBuys.js: the guns hang on the plane x = wall.x
        public readonly List<double> armoryWallZs = new List<double>();
        public readonly List<PerkSpot> perkSpots = new List<PerkSpot>(); // data.perkSpots: the perk machines (perks.js load)
        public readonly List<PerkSpot> gadgetSpots = new List<PerkSpot>(); // data.gadgetSpots (NIGHT21): the gadget terminals (gadgets.js load; `perk` holds the gadget id)
        public PerkSpot? forgeSpot; // data.forgeSpot: THE FORGE's cabinet (forge.js load; perk "forge"), or none
        public readonly List<Vec3> holdSpots = new List<Vec3>(); // NIGHT11: data.holdSpots (objectives.js HOLD rings)
        public readonly List<RelaySpot> relaySpots = new List<RelaySpot>(); // NIGHT11: data.relaySpots (protocol.js)
        public RelaySpot? secretRelay; // NIGHT11: data.secretRelay (protocol.js's hidden fifth relay), or none
        public readonly List<TrapSpot> trapSpots = new List<TrapSpot>(); // NIGHT11: data.trapSpots (traps.js)
        /// <summary>
        /// NIGHT15 U3-28.4: a Unity-authored map's own spots for the Unity-only systems (Design/maps/&lt;id&gt;.json "spots":
        /// box [design.box.spots' shape], power {x, y, z, yaw, zone}, exfil {radio, sites}); null on the web's maps, whose
        /// spots stay design.box / power / exfil (MysteryBox, Power and Exfil take this over the design block when set).
        /// </summary>
        public Dictionary<string, object> spots;
        public Dictionary<string, object> json;

        static Vec3 P(object o)
        {
            var d = PortData.Obj(o);
            return new Vec3(PortData.Num(d["x"]), PortData.Num(d["y"]), PortData.Num(d["z"]));
        }

        static Vec3 A(object o)
        {
            var a = PortData.Arr(o);
            return new Vec3(PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]));
        }

        static MapBox B(object o)
        {
            var d = PortData.Obj(o);
            return new MapBox { min = A(d["min"]), max = A(d["max"]), kind = d.TryGetValue("kind", out object k) ? (string)k : "" };
        }

        public static SurvivalMap FromExport(Dictionary<string, object> m)
        {
            var map = new SurvivalMap { json = m, name = (string)m["name"] };
            var data = PortData.Obj(m["data"]);
            map.floorSize = PortData.Num(m["floorSize"]);
            map.killPlaneY = PortData.Num(m["killPlaneY"]);
            var col = PortData.Obj(m["collision"]);
            foreach (object b in PortData.Arr(col["boxes"]))
            {
                MapBox box = B(b);
                if (box.kind == "floor") map.floor = box;
            }
            foreach (object b in PortData.Arr(data["boxes"])) map.boxes.Add(B(b));
            if (data.TryGetValue("strips", out object strips)) foreach (object b in PortData.Arr(strips)) map.strips.Add(B(b));
            foreach (object b in PortData.Arr(col["barriers"])) map.barriers.Add(B(b));
            foreach (object w in PortData.Arr(col["wedges"])) map.wedges.Add(PortData.Wedge(w));
            foreach (object p in PortData.Arr(m["pads"]))
            {
                var d = PortData.Obj(p);
                var pad = new PadSpec { pos = new Vec3(PortData.Num(d["x"]), PortData.Num(d["y"]), PortData.Num(d["z"])) };
                if (d.TryGetValue("target", out object t) && t != null)
                {
                    pad.aimed = true;
                    pad.target = P(t);
                    pad.apexAbove = PortData.Num(d["apexAbove"]);
                }
                map.pads.Add(pad);
            }
            foreach (object n in PortData.Arr(m["grappleNodes"])) map.grappleNodes.Add(P(n));
            var spawns = PortData.Obj(m["spawns"]);
            foreach (object s in PortData.Arr(spawns["ground"])) map.groundSpawns.Add(P(s));
            foreach (object s in PortData.Arr(spawns["high"])) map.highSpawns.Add(P(s));
            var ps = PortData.Obj(spawns["playerSpawn"]);
            map.playerSpawn = P(ps);
            map.playerSpawnYawDeg = ps.TryGetValue("yawDeg", out object yd) ? PortData.Num(yd) : 0;
            var core = PortData.Obj(PortData.Obj(m["core"])["spec"]);
            map.core = new CoreSpec { x0 = PortData.Num(core["x0"]), z0 = PortData.Num(core["z0"]), cells = (int)PortData.Num(core["cells"]), cellSize = PortData.Num(core["cellSize"]) };
            foreach (object z in PortData.Arr(data["zones"]))
            {
                var d = PortData.Obj(z);
                var def = new ZoneDef { id = (string)d["id"], name = (string)d["name"] };
                if (d.TryGetValue("cost", out object zc) && zc is double zcost) def.cost = zcost; // NIGHT15: a Unity map's own price
                if (d.TryGetValue("spots", out object spots))
                {
                    foreach (object s in PortData.Arr(spots))
                    {
                        var sd = PortData.Obj(s);
                        var spot = new ZoneSpot { pos = P(s) };
                        if (sd.TryGetValue("face", out object face))
                        {
                            var f = PortData.Arr(face);
                            spot.faceX = PortData.Num(f[0]);
                            spot.faceZ = PortData.Num(f[1]);
                        }
                        def.spots.Add(spot);
                    }
                }
                map.zones.Add(def);
            }
            var zp = PortData.Obj(m["zoneParts"]);
            map.startZone = (string)zp["startZone"];
            foreach (object p in PortData.Arr(zp["parts"]))
            {
                var d = PortData.Obj(p);
                map.zoneParts.Add(new ZonePart { zone = (int)PortData.Num(d["zone"]), min = A(d["min"]), max = A(d["max"]) });
            }
            map.navGrid = PortData.Obj(m["navGrid"]);
            if (data.TryGetValue("armoryWall", out object aw) && aw != null)
            {
                var w = PortData.Obj(aw);
                map.armoryWallX = PortData.Num(w["x"]);
                map.armoryWallY = PortData.Num(w["y"]);
                map.armoryWallFacing = PortData.Num(w["facing"]);
                foreach (object z in PortData.Arr(w["zs"])) map.armoryWallZs.Add(PortData.Num(z));
            }
            if (data.TryGetValue("perkSpots", out object ps2) && ps2 != null)
            {
                foreach (object o in PortData.Arr(ps2))
                {
                    var s = PortData.Obj(o);
                    map.perkSpots.Add(new PerkSpot
                    {
                        perk = (string)s["perk"], pos = new Vec3(PortData.Num(s["x"]), PortData.Num(s["y"]), PortData.Num(s["z"])),
                        yaw = s.TryGetValue("yaw", out object yw) ? PortData.Num(yw) : 0, core = s.TryGetValue("core", out object c) && c is bool cb && cb,
                    });
                }
            }
            if (data.TryGetValue("gadgetSpots", out object gs) && gs != null)
            {
                foreach (object o in PortData.Arr(gs))
                {
                    var s = PortData.Obj(o);
                    map.gadgetSpots.Add(new PerkSpot
                    {
                        perk = (string)s["gadget"], pos = new Vec3(PortData.Num(s["x"]), PortData.Num(s["y"]), PortData.Num(s["z"])),
                        yaw = s.TryGetValue("yaw", out object yw) ? PortData.Num(yw) : 0,
                    });
                }
            }
            if (data.TryGetValue("forgeSpot", out object fs) && fs != null)
            {
                var s = PortData.Obj(fs);
                map.forgeSpot = new PerkSpot
                {
                    perk = "forge", pos = new Vec3(PortData.Num(s["x"]), PortData.Num(s["y"]), PortData.Num(s["z"])),
                    yaw = s.TryGetValue("yaw", out object yw) ? PortData.Num(yw) : 0, core = s.TryGetValue("core", out object c) && c is bool cb && cb,
                };
            }
            // NIGHT11: the run's shape (objectives.js holdSpots, protocol.js relaySpots / secretRelay, traps.js trapSpots).
            Vec3 Feet(Dictionary<string, object> s) => new Vec3(PortData.Num(s["x"]), PortData.Num(s["y"]), PortData.Num(s["z"]));
            RelaySpot Relay(object o)
            {
                var s = PortData.Obj(o);
                return new RelaySpot { id = (string)s["id"], name = (string)s["name"], pos = Feet(s) };
            }
            if (data.TryGetValue("holdSpots", out object hs) && hs != null)
                foreach (object o in PortData.Arr(hs)) map.holdSpots.Add(Feet(PortData.Obj(o)));
            if (data.TryGetValue("relaySpots", out object rs) && rs != null)
                foreach (object o in PortData.Arr(rs)) map.relaySpots.Add(Relay(o));
            if (data.TryGetValue("secretRelay", out object sr) && sr != null) map.secretRelay = Relay(sr);
            if (data.TryGetValue("trapSpots", out object ts) && ts != null)
            {
                foreach (object o in PortData.Arr(ts))
                {
                    var s = PortData.Obj(o);
                    map.trapSpots.Add(new TrapSpot { trap = (string)s["trap"], pos = Feet(s), sx = PortData.Num(s["sx"]), sz = PortData.Num(s["sz"]) });
                }
            }
            if (m.TryGetValue("spots", out object mapSpots) && mapSpots != null) map.spots = PortData.Obj(mapSpots); // NIGHT15
            return map;
        }

        /// <summary>The static collider, as arena.js builds it ([floor, ...boxes], barriers, wedges through three's Octree).</summary>
        public ThreeOctree BuildOctree() => PortData.BuildMapOctree(json);

        /// <summary>The core at 'flat' (Survival's core: arena.setCore('flat') in startSurvival).</summary>
        public CoreGrid CreateCore(MovementConfig config)
        {
            if (!core.HasValue) return null;
            var grid = new CoreGrid(config, core.Value);
            grid.SetHeights(CoreGrid.PresetHeights("flat", core.Value.cells, config.core.maxHeight, config.core.heightStep, new Mulberry32(1)));
            return grid;
        }

        /// <summary>A zone's index by id (-1: none). NIGHT12: an indexed loop; FindIndex with a lambda allocated a closure and a
        /// delegate per call, every step through the power switch's buy check (zones.IsOpen).</summary>
        public int ZoneIndex(string id)
        {
            for (int i = 0; i < zones.Count; i++) if (zones[i].id == id) return i;
            return -1;
        }

        /// <summary>
        /// arena.js checkPads: launches a grounded mover standing on an awake pad (call after each mover step). Aimed
        /// pads land you on their target; plain pads go straight up to jumpPad.apexHeight. Returns the pad index or -1.
        /// </summary>
        public int CheckPads(Mover mover, MovementConfig config, bool[] enabled)
        {
            if (!mover.grounded || mover.velocity.y > 0) return -1;
            JumpPadConfig pad = config.jumpPad;
            Vec3 p = mover.position;
            for (int i = 0; i < pads.Count; i++)
            {
                if (enabled != null && !enabled[i]) continue;
                PadSpec spot = pads[i];
                double dx = p.x - spot.pos.x;
                double dz = p.z - spot.pos.z;
                if (dx * dx + dz * dz > pad.radius * pad.radius) continue;
                if (System.Math.Abs(p.y - spot.pos.y) > pad.triggerHeight) continue;
                double gravity = MovementMath.GravityFor(config.jump.height, config.jump.apexTime);
                if (spot.aimed)
                {
                    Vec3 v = MovementMath.PadLaunchVelocity(new Vec3(p.x, spot.pos.y, p.z), spot.target, spot.apexAbove, gravity, config.jump.fallGravityMult);
                    mover.Launch(v.y, v.x, v.z);
                }
                else
                {
                    mover.Launch(MovementMath.PadLaunchSpeed(pad.apexHeight, gravity));
                }
                return i;
            }
            return -1;
        }
    }
}
