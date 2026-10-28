// NIGHT15 U3-28.4 (builder P): a perk machine (data.perkSpots: perk, x, y, z, [core], yaw): the transform's position is
// its feet, its forward the way its front faces (euler y = 180 - yaw); core: it rides a core pillar.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapPerkSpot : MapMarker
    {
        public AuthoredPerk spot = new AuthoredPerk { perk = "steady" };

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
