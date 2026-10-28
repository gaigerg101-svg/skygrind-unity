// NIGHT15 U3-28.4 (builder P): SKYGRIND/Map/Export's work. Layer 1 (here, Unity): a MapRoot's markers -> MapModel (the
// plain model, web coordinates), each marker's exact values unless its handle moved (MapMarker). Layer 2 (UnityEngine-free:
// MapModelJson.Export): the model -> the map JSON, its navGrid and zoneParts from NavGridBuilder.ExportFor (or the
// imported file's, MapNavMode.KeepIfGeometryUnchanged), and the text as JSON.stringify writes it. Write puts the text in
// Design/maps/<id>.json and its byte-identical Player copy (Assets/StreamingAssets/Design/maps/<id>.json).
// Markers are read in hierarchy order; inactive GameObjects are left out.
using System.Collections.Generic;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapExportResult
    {
        public string path, playerCopyPath, text;
        public Dictionary<string, object> json;
        public bool navKept; // the imported nav grid kept (MapNavMode.KeepIfGeometryUnchanged with the geometry unchanged)
        public readonly List<string> warnings = new List<string>();
    }

    public static class MapExport
    {
        /// <summary>The markers under `root` as a model (web values: exact, or the moved handles' snapped ones).</summary>
        public static MapModel ToModel(MapRoot root)
        {
            var m = new MapModel
            {
                id = root.mapId, why = root.why ?? "", floorSize = root.floorSize, killPlaneY = root.killPlaneY, hasCore = root.hasCore, core = root.core,
                startZone = root.startZone ?? "", extraDataJson = root.extraDataJson ?? "",
            };
            List<string> w = m.warnings;
            foreach (MapBox b in root.GetComponentsInChildren<MapBox>())
            {
                AuthoredBox box = b.Read();
                if (!m.hasFloor && !box.kindUndefined && box.kind == "floor")
                {
                    m.hasFloor = true;
                    m.floor = box;
                }
                else m.boxes.Add(box);
                if (b.transform.rotation != Quaternion.identity) w.Add($"{b.name}: a box is axis-aligned; its rotation is ignored");
            }
            foreach (MapStrip s in root.GetComponentsInChildren<MapStrip>()) m.strips.Add(s.Read());
            foreach (MapBarrier b in root.GetComponentsInChildren<MapBarrier>()) m.barriers.Add(b.Read());
            foreach (MapWedge x in root.GetComponentsInChildren<MapWedge>()) m.wedges.Add(x.Read());
            foreach (MapPad p in root.GetComponentsInChildren<MapPad>()) m.pads.Add(p.Read());
            foreach (MapGrapple g in root.GetComponentsInChildren<MapGrapple>()) m.grappleNodes.Add(g.Read());
            bool player = false;
            foreach (MapPlayerSpawn s in root.GetComponentsInChildren<MapPlayerSpawn>())
            {
                if (s.duel) m.duelSpawns.Add(s.Read());
                else if (!player)
                {
                    player = true;
                    m.playerSpawn = s.Read();
                }
                else w.Add($"{s.name}: a second player spawn (the first under the root is the map's)");
            }
            if (!player) w.Add("no MapPlayerSpawn: the player spawns at (0, 0, 0)");
            foreach (MapSpawn s in root.GetComponentsInChildren<MapSpawn>()) (s.high ? m.highSpawns : m.groundSpawns).Add(s.Read());
            foreach (MapZone z in root.GetComponentsInChildren<MapZone>())
            {
                var zone = new AuthoredZone { id = z.zoneId, name = z.zoneName, priority = z.priority, hasCost = z.hasCost, cost = z.cost };
                foreach (MapZoneBox b in z.GetComponentsInChildren<MapZoneBox>())
                    if (!b.hole && b.GetComponentInParent<MapZone>() == z) zone.boxes.Add(b.Read());
                foreach (MapZoneBuy s in z.GetComponentsInChildren<MapZoneBuy>())
                    if (s.GetComponentInParent<MapZone>() == z) zone.spots.Add(s.Read());
                m.zones.Add(zone);
            }
            foreach (MapZoneBox b in root.GetComponentsInChildren<MapZoneBox>())
            {
                if (b.hole) m.zoneHoles.Add(b.Read());
                else if (b.GetComponentInParent<MapZone>() == null) w.Add($"{b.name}: a zone box outside every MapZone (not exported)");
            }
            foreach (MapZoneBuy s in root.GetComponentsInChildren<MapZoneBuy>())
                if (s.GetComponentInParent<MapZone>() == null) w.Add($"{s.name}: a gate buy spot outside every MapZone (not exported)");
            if (m.zones.Count > 0 && m.zones.FindIndex(z => z.id == m.startZone) < 0) w.Add($"startZone '{m.startZone}' is not a zone id");
            var rows = new Dictionary<string, AuthoredWallRow>();
            foreach (MapWallBuy b in root.GetComponentsInChildren<MapWallBuy>())
            {
                bool zPlane = b.row != MapWallRows.Wall;
                if (b.row != MapWallRows.Wall && b.row != MapWallRows.Rack && b.row != MapWallRows.Back)
                {
                    w.Add($"{b.name}: row '{b.row}' is none of wall, rack, back (not exported)");
                    continue;
                }
                MapWebVec at = b.Read();
                double plane = zPlane ? at.z : at.x, slot = zPlane ? at.x : at.z;
                if (!rows.TryGetValue(b.row, out AuthoredWallRow row))
                    rows[b.row] = row = new AuthoredWallRow { zPlane = zPlane, plane = plane, y = at.y, facing = b.facing };
                else if (row.plane != plane || row.y != at.y || row.facing != b.facing)
                    w.Add($"{b.name}: the {b.row} row hangs its guns on one plane ({(zPlane ? "z" : "x")} {row.plane}, y {row.y}, facing {row.facing}); this slot's {(zPlane ? "z" : "x")} {plane}, y {at.y}, facing {b.facing} are exported as the row's");
                row.slots.Add(slot);
            }
            rows.TryGetValue(MapWallRows.Wall, out m.armoryWall);
            rows.TryGetValue(MapWallRows.Rack, out m.armoryRack);
            rows.TryGetValue(MapWallRows.Back, out m.armoryBack);
            foreach (MapPerkSpot p in root.GetComponentsInChildren<MapPerkSpot>()) m.perkSpots.Add(p.Read());
            foreach (MapForge f in root.GetComponentsInChildren<MapForge>())
            {
                if (m.hasForge)
                {
                    w.Add($"{f.name}: a second forge (the first is the map's)");
                    continue;
                }
                m.hasForge = true;
                m.forgeSpot = f.Read();
            }
            foreach (MapGadgetSpot g in root.GetComponentsInChildren<MapGadgetSpot>()) m.gadgetSpots.Add(g.Read());
            foreach (MapTrap t in root.GetComponentsInChildren<MapTrap>()) m.trapSpots.Add(t.Read());
            foreach (MapHoldSpot h in root.GetComponentsInChildren<MapHoldSpot>())
            {
                m.holdSpots.Add(h.Read());
                m.holdVerbs.Add(h.verb ?? "");
            }
            foreach (MapRelay r in root.GetComponentsInChildren<MapRelay>())
            {
                if (!r.secret) m.relaySpots.Add(r.Read());
                else if (!m.hasSecretRelay)
                {
                    m.hasSecretRelay = true;
                    m.secretRelay = r.Read();
                }
                else w.Add($"{r.name}: a second secret relay (the first is the map's)");
            }
            foreach (MapBoxSpot b in root.GetComponentsInChildren<MapBoxSpot>()) m.boxSpots.Add(b.Read());
            foreach (MapPower p in root.GetComponentsInChildren<MapPower>())
            {
                if (m.hasPower)
                {
                    w.Add($"{p.name}: a second power switch (the first is the map's)");
                    continue;
                }
                m.hasPower = true;
                m.power = p.Read();
            }
            foreach (MapExfil x in root.GetComponentsInChildren<MapExfil>())
            {
                if (!x.radio) m.exfilSites.Add(x.Read());
                else if (!m.hasExfilRadio)
                {
                    m.hasExfilRadio = true;
                    m.exfilRadio = x.Read();
                }
                else w.Add($"{x.name}: a second exfil radio (the first is the map's)");
            }
            return m;
        }

        /// <summary>What the export says it came from (the JSON's "source").</summary>
        public static string Source(MapRoot root) =>
            $"MapRoot '{root.name}' in scene '{root.gameObject.scene.name}'" + (string.IsNullOrEmpty(root.importedFrom) ? "" : $" (imported from {root.importedFrom})");

        /// <summary>The imported map (this session's, or re-read from importedFrom under repoRoot), or null.</summary>
        public static Dictionary<string, object> Imported(MapRoot root, string repoRoot)
        {
            if (root.importedJson != null) return root.importedJson;
            if (string.IsNullOrEmpty(root.importedFrom) || repoRoot == null) return null;
            string path = Path.Combine(repoRoot, root.importedFrom);
            return File.Exists(path) ? PortData.Obj(PortData.LoadJson(path)) : null;
        }

        /// <summary>The map JSON and its text for `root` (no file written). repoRoot: to re-read an imported file (keep mode).</summary>
        public static MapExportResult Export(MapRoot root, GameConfig cfg, MapNavMode nav = MapNavMode.Build, string repoRoot = null)
        {
            MapModel m = ToModel(root);
            (Dictionary<string, object> json, string text, bool kept) = MapModelJson.Export(m, cfg, Source(root), nav, nav == MapNavMode.Build ? null : Imported(root, repoRoot));
            var r = new MapExportResult { json = json, text = text, navKept = kept };
            r.warnings.AddRange(m.warnings);
            return r;
        }

        /// <summary>The map's text (JSON.stringify + "\n").</summary>
        public static string ToJson(MapRoot root, GameConfig cfg, MapNavMode nav = MapNavMode.Build, string repoRoot = null) => Export(root, cfg, nav, repoRoot).text;

        /// <summary>Writes Design/maps/&lt;id&gt;.json and its byte-identical Player copy under repoRoot.</summary>
        public static MapExportResult Write(MapRoot root, GameConfig cfg, string repoRoot, MapNavMode nav = MapNavMode.Build)
        {
            MapExportResult r = Export(root, cfg, nav, repoRoot);
            (r.path, r.playerCopyPath) = MapModelJson.WriteFiles(repoRoot, root.mapId, r.text);
            return r;
        }

        /// <summary>Writes the map's text to `path` only (e.g. the round trip's Temp/MapRoundTrip/survival.json).</summary>
        public static MapExportResult WriteTo(MapRoot root, GameConfig cfg, string path, MapNavMode nav = MapNavMode.Build, string repoRoot = null)
        {
            MapExportResult r = Export(root, cfg, nav, repoRoot);
            MapModelJson.WriteTo(path, r.text);
            r.path = path;
            return r;
        }
    }
}
