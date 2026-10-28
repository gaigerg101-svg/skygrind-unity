// NIGHT15 U3-28.4 (builder P): a Mystery Box spot of a Unity-authored map (spots.box: x, y, z, yaw, zone; replaces
// design.box.spots for this map): position the feet, forward the box's front (euler y = 180 - yaw); zone: the zone id
// it opens with.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapBoxSpot : MapMarker
    {
        public AuthoredUnitySpot spot = new AuthoredUnitySpot { zone = "start" };

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(spot.at), Quaternion.Euler(0, MapHandles.FacingDegrees(spot.yaw), 0));
            MarkPlaced();
        }

        public AuthoredUnitySpot Read()
        {
            AuthoredUnitySpot s = spot;
            s.at = ReadPoint(spot.at);
            if (YawMoved) s.yaw = MapHandles.FacingYaw(transform.eulerAngles.y);
            return s;
        }

        void OnDrawGizmos() => MapHandles.PointGizmo(transform, UnityTuning.MapGizmoUsableRgb, true);
    }
}
