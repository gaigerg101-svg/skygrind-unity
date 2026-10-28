// NIGHT15 U3-28.4 (builder P): placing markers by code (MapImport, and a re-runnable map build script such as
// Assets/Editor/Maps/Build_<id>.cs). Each call makes a GameObject under `parent`, sets the marker's exact web values and
// places its transform (Place), so the export writes those values exactly until someone drags the handle. Coordinates
// are the web's (x, y, z; Unity z is -z). Groups are plain GameObjects; their order is the export's order.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public static class MapAuthor
    {
        public static MapWebVec V(double x, double y, double z) => new MapWebVec(x, y, z);

        /// <summary>A box from its min / max corners (web) and kind ("" none; zone boxes: kindUndefined).</summary>
        public static AuthoredBox B(MapWebVec min, MapWebVec max, string kind = "") => new AuthoredBox { min = min, max = max, kind = kind ?? "" };

        /// <summary>A new map root (in the active scene) with its id, floor square, kill plane and start zone.</summary>
        public static MapRoot Root(string mapId, double floorSize, double killPlaneY, string startZone, string why = "")
        {
            var go = new GameObject($"Map {mapId}");
            var root = go.AddComponent<MapRoot>();
            root.mapId = mapId;
            root.floorSize = floorSize;
            root.killPlaneY = killPlaneY;
            root.startZone = startZone ?? "";
            root.why = why ?? "";
            return root;
        }

        /// <summary>The child group `name` of `parent`, made when missing.</summary>
        public static Transform Group(Transform parent, string name)
        {
            Transform t = parent.Find(name);
            if (t != null) return t;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static T Add<T>(Transform parent, string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
        }

        public static MapBox Box(Transform parent, AuthoredBox b, string name = null)
        {
            var m = Add<MapBox>(parent, name ?? (string.IsNullOrEmpty(b.kind) ? "box" : $"box {b.kind}"));
            m.box = b;
            m.Place();
            return m;
        }

        public static MapBox Box(Transform parent, MapWebVec min, MapWebVec max, string kind, string name = null) => Box(parent, B(min, max, kind), name);

        public static MapStrip Strip(Transform parent, AuthoredBox b, string name = null)
        {
            var m = Add<MapStrip>(parent, name ?? "strip");
            m.box = b;
            m.Place();
            return m;
        }

        public static MapBarrier Barrier(Transform parent, AuthoredBox b, string name = null)
        {
            var m = Add<MapBarrier>(parent, name ?? "barrier");
            m.box = b;
            m.Place();
            return m;
        }

        public static MapWedge Wedge(Transform parent, AuthoredWedge w, string name = null)
        {
            var m = Add<MapWedge>(parent, name ?? "wedge");
            m.wedge = w;
            m.Place();
            return m;
        }

        /// <summary>A pad: aimed at `target` with apexAbove when target is given, else plain (straight up).</summary>
        public static MapPad Pad(Transform parent, MapWebVec at, MapWebVec? target, double apexAbove, string name = null)
        {
            var p = new AuthoredPad { at = at, aimed = target.HasValue, target = target ?? default, apexAbove = target.HasValue ? apexAbove : 0 };
            return Pad(parent, p, name);
        }

        public static MapPad Pad(Transform parent, AuthoredPad p, string name = null)
        {
            var m = Add<MapPad>(parent, name ?? "pad");
            m.pad = p;
            m.Place();
            return m;
        }

        public static MapGrapple Grapple(Transform parent, MapWebVec at, string name = null)
        {
            var m = Add<MapGrapple>(parent, name ?? "grapple node");
            m.at = at;
            m.Place();
            return m;
        }

        /// <summary>A map spawn (data.spawns.ground, or .high).</summary>
        public static MapSpawn Spawn(Transform parent, MapWebVec at, bool high, string name = null)
        {
            var m = Add<MapSpawn>(parent, name ?? (high ? "high spawn" : "ground spawn"));
            m.at = at;
            m.high = high;
            m.Place();
            return m;
        }

        /// <summary>The player spawn (duel false) or a Duel spawn.</summary>
        public static MapPlayerSpawn PlayerSpawn(Transform parent, MapWebVec at, double yawDeg, bool duel = false, string name = null) =>
            PlayerSpawn(parent, new AuthoredPlayerSpawn { at = at, yawDeg = yawDeg, raw = "" }, duel, name);

        public static MapPlayerSpawn PlayerSpawn(Transform parent, AuthoredPlayerSpawn s, bool duel = false, string name = null)
        {
            var m = Add<MapPlayerSpawn>(parent, name ?? (duel ? "duel spawn" : "player spawn"));
            m.spawn = s;
            m.duel = duel;
            m.Place();
            return m;
        }

        /// <summary>A zone (its boxes and buy spots go under it: ZoneBox / ZoneBuy with the zone's transform).</summary>
        public static MapZone Zone(Transform parent, string id, string zoneName, double priority, double? cost = null)
        {
            var z = Add<MapZone>(parent, $"zone {id}");
            z.zoneId = id;
            z.zoneName = zoneName;
            z.priority = priority;
            z.hasCost = cost.HasValue;
            z.cost = cost ?? 0;
            return z;
        }

        /// <summary>A zone box (under its MapZone), or with hole set a hole (anywhere under the root).</summary>
        public static MapZoneBox ZoneBox(Transform parent, AuthoredBox b, bool hole = false, string name = null)
        {
            var m = Add<MapZoneBox>(parent, name ?? (hole ? "hole" : "zone box"));
            m.box = b;
            m.hole = hole;
            m.Place();
            return m;
        }

        /// <summary>A zone box with the web's undefined kind.</summary>
        public static MapZoneBox ZoneBox(Transform parent, MapWebVec min, MapWebVec max, bool hole = false, string name = null) =>
            ZoneBox(parent, new AuthoredBox { min = min, max = max, kind = "", kindUndefined = true }, hole, name);

        public static MapZoneBuy ZoneBuy(Transform zone, MapWebVec at, double faceX, double faceZ, string name = null)
        {
            var m = Add<MapZoneBuy>(zone, name ?? "gate buy");
            m.spot = new AuthoredZoneSpot { at = at, faceX = faceX, faceZ = faceZ };
            m.Place();
            return m;
        }

        public static MapPerkSpot Perk(Transform parent, string perk, MapWebVec at, double yaw, bool core = false, string name = null, string verb = null)
        {
            var m = Add<MapPerkSpot>(parent, name ?? $"perk {perk}");
            m.spot = new AuthoredPerk { perk = perk, at = at, yaw = yaw, core = core, verb = verb ?? "" };
            m.Place();
            return m;
        }

        public static MapForge Forge(Transform parent, MapWebVec at, double yaw, bool core = false, string name = null, string verb = null)
        {
            var m = Add<MapForge>(parent, name ?? "forge");
            m.spot = new AuthoredPerk { perk = "", at = at, yaw = yaw, core = core, verb = verb ?? "" };
            m.Place();
            return m;
        }

        public static MapGadgetSpot Gadget(Transform parent, string gadget, MapWebVec at, double yaw, string name = null)
        {
            var m = Add<MapGadgetSpot>(parent, name ?? $"gadget {gadget}");
            m.spot = new AuthoredGadget { gadget = gadget, at = at, yaw = yaw };
            m.Place();
            return m;
        }

        /// <summary>One gun slot of a wall row (MapWallRows.Wall: the plane x = at.x; Rack / Back: the plane z = at.z).</summary>
        public static MapWallBuy WallBuy(Transform parent, string row, MapWebVec at, double facing, string name = null)
        {
            var m = Add<MapWallBuy>(parent, name ?? $"{row} gun");
            m.row = row;
            m.at = at;
            m.facing = facing;
            m.Place();
            return m;
        }

        public static MapTrap Trap(Transform parent, string trap, MapWebVec at, double sx, double sz, string name = null)
        {
            var m = Add<MapTrap>(parent, name ?? $"trap {trap}");
            m.spot = new AuthoredTrap { trap = trap, at = at, sx = sx, sz = sz };
            m.Place();
            return m;
        }

        public static MapHoldSpot Hold(Transform parent, MapWebVec at, string name = null, string verb = null)
        {
            var m = Add<MapHoldSpot>(parent, name ?? "hold ring");
            m.at = at;
            m.verb = verb ?? "";
            m.Place();
            return m;
        }

        public static MapRelay Relay(Transform parent, string id, string relayName, MapWebVec at, bool secret = false, string name = null, string verb = null)
        {
            var m = Add<MapRelay>(parent, name ?? (secret ? $"secret relay {id}" : $"relay {id}"));
            m.relay = new AuthoredRelay { id = id, name = relayName, at = at, verb = verb ?? "" };
            m.secret = secret;
            m.Place();
            return m;
        }

        /// <summary>A Mystery Box spot (spots.box).</summary>
        public static MapBoxSpot BoxSpot(Transform parent, MapWebVec at, double yaw, string zone, string name = null, string verb = null)
        {
            var m = Add<MapBoxSpot>(parent, name ?? "mystery box");
            m.spot = new AuthoredUnitySpot { at = at, yaw = yaw, zone = zone, verb = verb ?? "" };
            m.Place();
            return m;
        }

        /// <summary>The power switch (spots.power).</summary>
        public static MapPower Power(Transform parent, MapWebVec at, double yaw, string zone, string name = null, string verb = null)
        {
            var m = Add<MapPower>(parent, name ?? "power switch");
            m.spot = new AuthoredUnitySpot { at = at, yaw = yaw, zone = zone, verb = verb ?? "" };
            m.Place();
            return m;
        }

        /// <summary>The exfil radio (radio true, with its yaw) or an exfil site (spots.exfil).</summary>
        public static MapExfil Exfil(Transform parent, MapWebVec at, double yaw, string zone, bool radio, string name = null, string verb = null)
        {
            var m = Add<MapExfil>(parent, name ?? (radio ? "exfil radio" : "exfil site"));
            m.radio = radio;
            m.spot = new AuthoredUnitySpot { at = at, yaw = radio ? yaw : 0, zone = zone, verb = verb ?? "" };
            m.Place();
            return m;
        }
    }
}
