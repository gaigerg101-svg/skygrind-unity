// NIGHT15 U3-28.4 (builder P): a gadget pickup spot (data.gadgetSpots: gadget, x, y, z, yaw; survival's ARCLINE):
// position the feet, forward the facing (euler y = 180 - yaw), as a perk machine.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapGadgetSpot : MapMarker
    {
        public AuthoredGadget spot = new AuthoredGadget { gadget = "arcline" };

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(spot.at), Quaternion.Euler(0, MapHandles.FacingDegrees(spot.yaw), 0));
            MarkPlaced();
        }

        public AuthoredGadget Read()
        {
            AuthoredGadget s = spot;
            s.at = ReadPoint(spot.at);
            if (YawMoved) s.yaw = MapHandles.FacingYaw(transform.eulerAngles.y);
            return s;
        }

        void OnDrawGizmos() => MapHandles.PointGizmo(transform, UnityTuning.MapGizmoUsableRgb, true);
    }
}
