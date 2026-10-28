// NIGHT15 U3-28.4 (builder P): THE FORGE's cabinet (data.forgeSpot: x, y, z, [core], yaw), as a perk machine (its perk
// field is unused: the JSON has none). One per map: the first under the MapRoot is exported.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapForge : MapMarker
    {
        public AuthoredPerk spot = new AuthoredPerk { perk = "" };

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(spot.at), Quaternion.Euler(0, MapHandles.FacingDegrees(spot.yaw), 0));
            MarkPlaced();
        }

        public AuthoredPerk Read()
        {
            AuthoredPerk s = spot;
            s.at = ReadPoint(spot.at);
            if (YawMoved) s.yaw = MapHandles.FacingYaw(transform.eulerAngles.y);
            return s;
        }

        void OnDrawGizmos() => MapHandles.PointGizmo(transform, UnityTuning.MapGizmoUsableRgb, true);
    }
}
