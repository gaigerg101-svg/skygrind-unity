// NIGHT15 U3-28.4 (builder P): one box of its parent MapZone (data.zones[i].boxes), or with hole set a hole cut out of
// every zone (data.zoneHoles: anywhere under the MapRoot). The transform's position is its centre and its scale its size,
// as MapBox; its kind is the web's undefined (kindUndefined) unless a map gave it one.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapZoneBox : MapMarker
    {
        public AuthoredBox box = new AuthoredBox { kind = "", kindUndefined = true };
        public bool hole;

        public override void Place()
        {
            (Vector3 centre, Vector3 size) = MapHandles.BoxTransform(box);
            transform.SetPositionAndRotation(centre, Quaternion.identity);
            MapHandles.SetWorldScale(transform, size);
            MarkPlaced();
        }

        public AuthoredBox Read() => PositionMoved || ScaleMoved ? MapHandles.BoxFrom(transform, box) : box;

        void OnDrawGizmos()
        {
            Gizmos.color = MapHandles.Rgb(hole ? UnityTuning.MapGizmoHoleRgb : UnityTuning.MapGizmoZoneRgb);
            Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        }
    }
}
