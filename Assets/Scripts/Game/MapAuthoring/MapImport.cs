// NIGHT15 U3-28.4 (builder P): SKYGRIND/Map/Import's work: any map JSON (a web export in PortExport/maps or a
// Design/maps/<id>.json) as a tree of markers under a new MapRoot in the active scene, each holding the file's exact values
// (MapModelJson.FromJson) and placed on them (MapAuthor). Exported unchanged, the markers give back the file's map data
// byte for byte (MapRoundTrip). Groups: Geometry (Boxes: the floor first, Strips, Barriers, Wedges, Pads, Grapple), Zones
// (one per zone, then Holes), Spawns (Player, Duel, Ground, High), Spots (Perks, Forge, Wall guns, Traps, Gadgets, Hold
// rings, Relays, Mystery box, Power, Exfil). What the model can't carry is in `warnings`.
using System.Collections.Generic;
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public static class MapImport
    {
        public static MapRoot Build(Dictionary<string, object> mapJson, string importedFrom = null) => Build(mapJson, importedFrom, out _);

        public static MapRoot Build(Dictionary<string, object> mapJson, string importedFrom, out List<string> warnings)
        {
            MapModel m = MapModelJson.FromJson(mapJson);
            warnings = m.warnings;
            MapRoot root = MapAuthor.Root(m.id, m.floorSize, m.killPlaneY, m.startZone, m.why);
            root.hasCore = m.hasCore;
            if (m.hasCore) root.core = m.core;
            root.importedFrom = importedFrom ?? "";
            root.importedJson = mapJson;
            root.extraDataJson = m.extraDataJson ?? "";
            Transform r = root.transform;

            Transform geometry = MapAuthor.Group(r, "Geometry");
            Transform boxes = MapAuthor.Group(geometry, "Boxes");
            if (m.hasFloor) MapAuthor.Box(boxes, m.floor, "floor");
            for (int i = 0; i < m.boxes.Count; i++) MapAuthor.Box(boxes, m.boxes[i], $"box {i} {m.boxes[i].kind}");
            Transform strips = MapAuthor.Group(geometry, "Strips");
            for (int i = 0; i < m.strips.Count; i++) MapAuthor.Strip(strips, m.strips[i], $"strip {i}");
            Transform barriers = MapAuthor.Group(geometry, "Barriers");
            for (int i = 0; i < m.barriers.Count; i++) MapAuthor.Barrier(barriers, m.barriers[i], $"barrier {i}");
            Transform wedges = MapAuthor.Group(geometry, "Wedges");
            for (int i = 0; i < m.wedges.Count; i++) MapAuthor.Wedge(wedges, m.wedges[i], $"wedge {i}");
            Transform pads = MapAuthor.Group(geometry, "Pads");
            for (int i = 0; i < m.pads.Count; i++) MapAuthor.Pad(pads, m.pads[i], $"pad {i}");
            Transform grapple = MapAuthor.Group(geometry, "Grapple");
            for (int i = 0; i < m.grappleNodes.Count; i++) MapAuthor.Grapple(grapple, m.grappleNodes[i], $"grapple node {i}");

            Transform zones = MapAuthor.Group(r, "Zones");
            foreach (AuthoredZone z in m.zones)
            {
                MapZone zone = MapAuthor.Zone(zones, z.id, z.name, z.priority, z.hasCost ? z.cost : (double?)null);
                for (int i = 0; i < z.boxes.Count; i++) MapAuthor.ZoneBox(zone.transform, z.boxes[i], false, $"zone box {i}");
                for (int i = 0; i < z.spots.Count; i++) MapAuthor.ZoneBuy(zone.transform, z.spots[i].at, z.spots[i].faceX, z.spots[i].faceZ, $"gate buy {i}");
            }
            if (m.zoneHoles.Count > 0)
            {
                Transform holes = MapAuthor.Group(zones, "Holes");
                for (int i = 0; i < m.zoneHoles.Count; i++) MapAuthor.ZoneBox(holes, m.zoneHoles[i], true, $"hole {i}");
            }

            Transform spawns = MapAuthor.Group(r, "Spawns");
            MapAuthor.PlayerSpawn(MapAuthor.Group(spawns, "Player"), m.playerSpawn, false, "player spawn");
            Transform duel = MapAuthor.Group(spawns, "Duel");
            for (int i = 0; i < m.duelSpawns.Count; i++) MapAuthor.PlayerSpawn(duel, m.duelSpawns[i], true, $"duel spawn {i}");
            Transform ground = MapAuthor.Group(spawns, "Ground");
            for (int i = 0; i < m.groundSpawns.Count; i++) MapAuthor.Spawn(ground, m.groundSpawns[i], false, $"ground spawn {i}");
            Transform high = MapAuthor.Group(spawns, "High");
            for (int i = 0; i < m.highSpawns.Count; i++) MapAuthor.Spawn(high, m.highSpawns[i], true, $"high spawn {i}");

            Transform spots = MapAuthor.Group(r, "Spots");
            if (m.perkSpots.Count > 0)
            {
                Transform perks = MapAuthor.Group(spots, "Perks");
                foreach (AuthoredPerk p in m.perkSpots) MapAuthor.Perk(perks, p.perk, p.at, p.yaw, p.core, verb: p.verb);
            }
            if (m.hasForge) MapAuthor.Forge(MapAuthor.Group(spots, "Forge"), m.forgeSpot.at, m.forgeSpot.yaw, m.forgeSpot.core, verb: m.forgeSpot.verb);
            if (m.armoryWall != null || m.armoryRack != null || m.armoryBack != null)
            {
                Transform guns = MapAuthor.Group(spots, "Wall guns");
                WallRow(guns, m.armoryWall, MapWallRows.Wall);
                WallRow(guns, m.armoryRack, MapWallRows.Rack);
                WallRow(guns, m.armoryBack, MapWallRows.Back);
            }
            if (m.trapSpots.Count > 0)
            {
                Transform traps = MapAuthor.Group(spots, "Traps");
                foreach (AuthoredTrap t in m.trapSpots) MapAuthor.Trap(traps, t.trap, t.at, t.sx, t.sz);
            }
            if (m.gadgetSpots.Count > 0)
            {
                Transform gadgets = MapAuthor.Group(spots, "Gadgets");
                foreach (AuthoredGadget g in m.gadgetSpots) MapAuthor.Gadget(gadgets, g.gadget, g.at, g.yaw);
            }
            if (m.holdSpots.Count > 0)
            {
                Transform holds = MapAuthor.Group(spots, "Hold rings");
                for (int i = 0; i < m.holdSpots.Count; i++) MapAuthor.Hold(holds, m.holdSpots[i], $"hold ring {i}", i < m.holdVerbs.Count ? m.holdVerbs[i] : null);
            }
            if (m.relaySpots.Count > 0 || m.hasSecretRelay)
            {
                Transform relays = MapAuthor.Group(spots, "Relays");
                foreach (AuthoredRelay rl in m.relaySpots) MapAuthor.Relay(relays, rl.id, rl.name, rl.at, verb: rl.verb);
                if (m.hasSecretRelay) MapAuthor.Relay(relays, m.secretRelay.id, m.secretRelay.name, m.secretRelay.at, true, verb: m.secretRelay.verb);
            }
            if (m.boxSpots.Count > 0)
            {
                Transform box = MapAuthor.Group(spots, "Mystery box");
                foreach (AuthoredUnitySpot s in m.boxSpots) MapAuthor.BoxSpot(box, s.at, s.yaw, s.zone, verb: s.verb);
            }
            if (m.hasPower) MapAuthor.Power(MapAuthor.Group(spots, "Power"), m.power.at, m.power.yaw, m.power.zone, verb: m.power.verb);
            if (m.hasExfilRadio || m.exfilSites.Count > 0)
            {
                Transform exfil = MapAuthor.Group(spots, "Exfil");
                if (m.hasExfilRadio) MapAuthor.Exfil(exfil, m.exfilRadio.at, m.exfilRadio.yaw, m.exfilRadio.zone, true, verb: m.exfilRadio.verb);
                for (int i = 0; i < m.exfilSites.Count; i++) MapAuthor.Exfil(exfil, m.exfilSites[i].at, 0, m.exfilSites[i].zone, false, $"exfil site {i}", m.exfilSites[i].verb);
            }
            return root;
        }

        static void WallRow(Transform parent, AuthoredWallRow row, string name)
        {
            if (row == null) return;
            for (int i = 0; i < row.slots.Count; i++)
            {
                MapWebVec at = row.zPlane ? new MapWebVec(row.slots[i], row.y, row.plane) : new MapWebVec(row.plane, row.y, row.slots[i]);
                MapAuthor.WallBuy(parent, name, at, row.facing, $"{name} gun {i}");
            }
        }
    }
}
