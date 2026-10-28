// NIGHT15 U3-28.4 (builder P): a Unity-authored map's exfil: the radio (radio set: spots.exfil.radio {x, y, z, yaw,
// zone}, one per map, forward its front: euler y = 180 - yaw) or a site (spots.exfil.sites [{x, y, z, zone}]: no yaw, as
// design.exfil.sites). spots.exfil is written only with the radio and at least one site (Exfil reads both).
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapExfil : MapMarker
    {
        public bool radio;
        public AuthoredUnitySpot spot = new AuthoredUnitySpot { zone = "start" };

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(spot.at), Quaternion.Euler(0, radio ? MapHandles.FacingDegrees(spot.yaw) : 0, 0));
            MarkPlaced();
        }

        public AuthoredUnitySpot Read()
        {
            AuthoredUnitySpot s = spot;
            s.at = ReadPoint(spot.at);
            if (radio && YawMoved) s.yaw = MapHandles.FacingYaw(transform.eulerAngles.y);
            return s;
        }

        void OnDrawGizmos() => MapHandles.PointGizmo(transform, UnityTuning.MapGizmoUsableRgb, radio, !radio);
    }
}
