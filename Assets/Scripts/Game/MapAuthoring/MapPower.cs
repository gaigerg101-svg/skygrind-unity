// NIGHT15 U3-28.4 (builder P): the power switch of a Unity-authored map (spots.power: x, y, z, yaw, zone; replaces
// design.power.spot for this map): position the feet, forward its front (euler y = 180 - yaw). One per map.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapPower : MapMarker
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
