// NIGHT15 U3-28.4 (builder P): the map pipeline's pure data model, between the markers (MapAuthoring/*: Unity
// transforms) and the map JSON (the web exporter's schema, port-export/scripts/maps.mjs exportMap, plus a Unity-authored
// map's _why and spots). Every value is the web's (x, y, z) double, kept exact: the markers serialize these structs, and
// their transforms are only the editing handles (MapHandles). No UnityEngine here, so the JSON side (MapModelJson,
// MapJsonWriter, MapValidate, MapRoundTrip) also builds and runs outside Unity. No gameplay code reads any of this at run
// time: the game reads the exported JSON (SurvivalMap.FromExport).
using System;
using System.Collections.Generic;

namespace Skygrind.Game.MapAuthoring
{
    /// <summary>A point in the web's coordinates (three.js: x right, y up, yaw 0 looking toward -z).</summary>
    [Serializable]
    public struct MapWebVec
    {
        public double x, y, z;

        public MapWebVec(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString() => $"({x}, {y}, {z})";
    }

    /// <summary>An axis-aligned box {min: [x, y, z], max: [x, y, z], kind}: kind is a string, or the web's undefined (zone boxes).</summary>
    [Serializable]
    public struct AuthoredBox
    {
        public MapWebVec min, max;
        public string kind;
        public bool kindUndefined; // {"$undefined": true} (data.zones[i].boxes, data.zoneHoles)
    }

    /// <summary>A ramp (data.wedges): rises `rise` m over `length` m along (-sin yaw, -cos yaw) from its low-end centre (x, z); width across.</summary>
    [Serializable]
    public struct AuthoredWedge
    {
        public double x, z, yaw, length, width, rise;
    }

    /// <summary>A jump pad (pads): aimed at a target with apexAbove, or plain ("target": null: straight up to jumpPad.apexHeight).</summary>
    [Serializable]
    public struct AuthoredPad
    {
        public MapWebVec at;
        public bool aimed;
        public MapWebVec target;
        public double apexAbove;
    }

    /// <summary>The player spawn or a Duel spawn (spawns.playerSpawn / duelSpawns): feet and the look yaw in degrees.</summary>
    [Serializable]
    public struct AuthoredPlayerSpawn
    {
        public MapWebVec at;
        public double yawDeg;
        /// <summary>A spawn that is not {x, y, z, yawDeg} (training's {"$config": "arena.spawn"}), as JSON text kept verbatim; "" otherwise.</summary>
        public string raw;
    }

    /// <summary>A perk machine (data.perkSpots: perk, x, y, z, [core], yaw) or THE FORGE (data.forgeSpot: x, y, z, [core], yaw).</summary>
    [Serializable]
    public struct AuthoredPerk
    {
        public string perk; // "" for the forge (its JSON has no perk key)
        public MapWebVec at;
        public double yaw; // its front faces (sin yaw, 0, cos yaw)
        public bool core; // rides a core pillar (written only when true, before yaw: the web's key order)
        /// <summary>A Unity-authored map's designed verb gate (UnityTuning.MapVerbs: "wallRide", ...; "" none), written last when set.</summary>
        public string verb;
    }

    /// <summary>A gadget spot (data.gadgetSpots: gadget, x, y, z, yaw).</summary>
    [Serializable]
    public struct AuthoredGadget
    {
        public string gadget;
        public MapWebVec at;
        public double yaw;
    }

    /// <summary>A trap (data.trapSpots: trap, x, y, z, sx, sz: its switch at (sx, y, sz)).</summary>
    [Serializable]
    public struct AuthoredTrap
    {
        public string trap;
        public MapWebVec at;
        public double sx, sz;
    }

    /// <summary>A PROTOCOL relay (data.relaySpots / secretRelay: id, name, x, y, z).</summary>
    [Serializable]
    public struct AuthoredRelay
    {
        public string id, name;
        public MapWebVec at;
        public string verb; // a designed verb gate ("" none), written last when set
    }

    /// <summary>A zone's gate buy spot (data.zones[i].spots: x, y, z, face [x, z]).</summary>
    [Serializable]
    public struct AuthoredZoneSpot
    {
        public MapWebVec at;
        public double faceX, faceZ;
    }

    /// <summary>
    /// A Unity-only system's spot (the map's "spots": box [{x, y, z, yaw, zone}], power {x, y, z, yaw, zone}, exfil radio
    /// {x, y, z, yaw, zone}, exfil sites [{x, y, z, zone}]: a site has no yaw, as design.exfil.sites).
    /// </summary>
    [Serializable]
    public struct AuthoredUnitySpot
    {
        public MapWebVec at;
        public double yaw;
        public string zone;
        public string verb; // a designed verb gate ("" none), written last when set
    }

    /// <summary>The core grid's spec (core.spec / data.core: x0, z0, cells, cellSize).</summary>
    [Serializable]
    public struct AuthoredCore
    {
        public double x0, z0, cells, cellSize;
    }

    /// <summary>A zone (data.zones[i]: id, name, priority, [cost], boxes, spots).</summary>
    public sealed class AuthoredZone
    {
        public string id, name;
        public double priority;
        public bool hasCost; // a Unity-authored map's gate price (data.zones[i].cost); the web's zones price by config zones.costs
        public double cost;
        public readonly List<AuthoredBox> boxes = new List<AuthoredBox>();
        public readonly List<AuthoredZoneSpot> spots = new List<AuthoredZoneSpot>();
    }

    /// <summary>
    /// A row of wall guns (wallBuys.js): armoryWall {x, y, zs, facing} hangs on the plane x = plane; armoryRack and
    /// armoryBack {z, y, xs, facing} on the plane z = plane. One slot per gun, the gun by its order.
    /// </summary>
    public sealed class AuthoredWallRow
    {
        public bool zPlane; // false: armoryWall's x plane (zs); true: a rack's z plane (xs)
        public double plane, y, facing;
        public readonly List<double> slots = new List<double>();
    }

    /// <summary>The names of the three wall rows, as the map data names them.</summary>
    public static class MapWallRows
    {
        public const string Wall = "wall", Rack = "rack", Back = "back";
    }

    /// <summary>A whole map, in the web's values. Lists keep the map data's order (the markers' hierarchy order).</summary>
    public sealed class MapModel
    {
        public string id = "newmap"; // the map's name (Design/maps/<id>.json)
        public string why = ""; // the top-level "_why" (a Unity-authored map's design reason); "" for the web's maps
        public double floorSize = 96, killPlaneY = -30;
        public bool hasCore;
        public AuthoredCore core;
        /// <summary>
        /// The floor (map.floor: collision.boxes' last kind "floor" box, never in data.boxes); hasFloor false: the web
        /// exporter's floor from floorSize and config core.floorThickness. Extra floor slabs are kind "floor" boxes in `boxes`.
        /// </summary>
        public bool hasFloor;
        public AuthoredBox floor;
        public readonly List<AuthoredBox> boxes = new List<AuthoredBox>(); // data.boxes (collision: the floor inserted)
        public readonly List<AuthoredBox> strips = new List<AuthoredBox>();
        public readonly List<AuthoredBox> barriers = new List<AuthoredBox>();
        public readonly List<AuthoredWedge> wedges = new List<AuthoredWedge>();
        public readonly List<AuthoredPad> pads = new List<AuthoredPad>();
        public readonly List<MapWebVec> grappleNodes = new List<MapWebVec>();
        public AuthoredPlayerSpawn playerSpawn;
        public readonly List<AuthoredPlayerSpawn> duelSpawns = new List<AuthoredPlayerSpawn>();
        public readonly List<MapWebVec> groundSpawns = new List<MapWebVec>();
        public readonly List<MapWebVec> highSpawns = new List<MapWebVec>();
        public string startZone = "";
        public readonly List<AuthoredZone> zones = new List<AuthoredZone>();
        public readonly List<AuthoredBox> zoneHoles = new List<AuthoredBox>();
        public AuthoredWallRow armoryWall, armoryRack, armoryBack; // null: none
        public readonly List<AuthoredPerk> perkSpots = new List<AuthoredPerk>();
        public readonly List<AuthoredTrap> trapSpots = new List<AuthoredTrap>();
        public readonly List<AuthoredGadget> gadgetSpots = new List<AuthoredGadget>();
        public bool hasForge;
        public AuthoredPerk forgeSpot;
        public readonly List<MapWebVec> holdSpots = new List<MapWebVec>();
        /// <summary>holdSpots[i]'s designed verb gate when i &lt; Count and it isn't empty (a parallel list: holdSpots stays points).</summary>
        public readonly List<string> holdVerbs = new List<string>();
        public bool hasSecretRelay;
        public AuthoredRelay secretRelay;
        public readonly List<AuthoredRelay> relaySpots = new List<AuthoredRelay>();
        // The Unity-only systems' spots (top-level "spots"; SurvivalMap.spots).
        public readonly List<AuthoredUnitySpot> boxSpots = new List<AuthoredUnitySpot>();
        public bool hasPower;
        public AuthoredUnitySpot power;
        public bool hasExfilRadio;
        public AuthoredUnitySpot exfilRadio;
        public readonly List<AuthoredUnitySpot> exfilSites = new List<AuthoredUnitySpot>();
        /// <summary>data keys the model has no field for (e.g. training's "signs"), as a JSON object's text, kept in order.</summary>
        public string extraDataJson = "";
        /// <summary>What the import or the export could not carry exactly (shown in the menu's log and the tests).</summary>
        public readonly List<string> warnings = new List<string>();

        public bool HasUnitySpots => boxSpots.Count > 0 || hasPower || hasExfilRadio || exfilSites.Count > 0;
    }
}
