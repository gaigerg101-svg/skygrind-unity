// NIGHT15 U3-28.4 (builder P): a zone (data.zones[i]: id, name, priority, [cost], boxes, spots). Its boxes are the child
// MapZoneBox markers (hole off) and its gate buy spots the child MapZoneBuy markers, in hierarchy order. priority settles
// overlaps (zoneMath.js resolveZones: the higher wins). A Unity-authored map may set the gate's price here (hasCost: a
// data.zones[i].cost key); the web's zones price by config zones.costs.<id> and have none.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    [DisallowMultipleComponent]
    public sealed class MapZone : MonoBehaviour
    {
        public string zoneId = "zone";
        public string zoneName = "ZONE";
        public double priority = 1;
        public bool hasCost;
        public double cost;
    }
}
