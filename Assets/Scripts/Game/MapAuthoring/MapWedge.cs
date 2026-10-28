// NIGHT15 U3-28.4 (builder P): a ramp (data.wedges / collision.wedges: x, z, yaw, length, width, rise). It rises `rise` m
// over `length` m from its low-end centre (x, 0, z) along (-sin yaw, -cos yaw) (arena.js wedgeGeometry): the transform's
// position is the low-end centre (y ignored), its forward the uphill direction (euler y = -yaw); length, width and rise
// are the fields (the survival map's are 8 x 4 m rising 2.9117618741296187 m: 20 degrees).
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapWedge : MapMarker
    {
        public AuthoredWedge wedge = new AuthoredWedge { length = 8, width = 4, rise = 2.9117618741296187 };

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(new MapWebVec(wedge.x, 0, wedge.z)), Quaternion.Euler(0, MapHandles.WedgeDegrees(wedge.yaw), 0));
            MarkPlaced();
        }

        public AuthoredWedge Read()
        {
            AuthoredWedge w = wedge;
            if (PositionMoved)
            {
                MapWebVec p = MapHandles.Snap(MapHandles.FromUnity(transform.position));
                w.x = p.x;
                w.z = p.z;
            }
            if (YawMoved) w.yaw = MapHandles.WedgeYaw(transform.eulerAngles.y);
            return w;
        }

        void OnDrawGizmos()
        {
            Vector3 o = transform.position, f = transform.forward, r = transform.right;
            float len = (float)wedge.length, hw = (float)wedge.width / 2, rise = (float)wedge.rise;
            Vector3 a = o - r * hw, b = o + r * hw, c = a + f * len, d = b + f * len, cu = c + Vector3.up * rise, du = d + Vector3.up * rise;
            Gizmos.color = MapHandles.Rgb(UnityTuning.MapGizmoStoneRgb);
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(a, c);
            Gizmos.DrawLine(b, d);
            Gizmos.DrawLine(cu, du);
            Gizmos.DrawLine(a, cu); // the slope
            Gizmos.DrawLine(b, du);
            Gizmos.DrawLine(c, cu); // the back
            Gizmos.DrawLine(d, du);
        }
    }
}
