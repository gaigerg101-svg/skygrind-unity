// NIGHT15 U3-28.4 (builder P): a gate buy spot of its parent MapZone (data.zones[i].spots: x, y, z, face [x, z]). The
// transform's position is the spot; face (web x, z: the way the buyer looks at the gate) is the fields' only.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapZoneBuy : MapMarker
    {
        public AuthoredZoneSpot spot = new AuthoredZoneSpot { faceX = 1 };

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(spot.at), Quaternion.identity);
            MarkPlaced();
        }

        public AuthoredZoneSpot Read()
        {
            AuthoredZoneSpot s = spot;
            s.at = ReadPoint(spot.at);
            return s;
        }

        void OnDrawGizmos()
        {
            MapHandles.PointGizmo(transform, UnityTuning.MapGizmoZoneRgb, false);
            Vector3 p = transform.position;
            Gizmos.DrawLine(p, p + new Vector3((float)spot.faceX, 0, (float)-spot.faceZ) * UnityTuning.MapGizmoFacingLength);
        }
    }
}
