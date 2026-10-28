// NIGHT15 U3-28.4 (builder P): a marker's transform as the editing handle of its exact web values (MapModel). Web (x, y, z)
// is Unity (x, y, -z) (WebSpace). A transform that moved since the marker last placed it exports its own values snapped
// to the authoring grid (UnityTuning.MapSnapPerMetre, MapYawSnapDegrees); one that didn't keeps the exact doubles (a float
// transform can't hold 1.5707963267948966 or 2.9117618741296187). Yaw conventions (degrees about Unity's +y):
// - a facing spot (perks, the forge, gadgets, the box, the power switch, the exfil radio): its front faces (sin yaw, 0,
//   cos yaw) in the web, (sin yaw, 0, -cos yaw) in Unity: euler y = 180 - yaw;
// - a look (the player and Duel spawns, yawDeg): yaw 0 looks toward web -z = Unity +z: euler y = -yawDeg (WebSpace.LookRotation);
// - a wedge: it rises along (-sin yaw, -cos yaw) in web (x, z) = Unity (-sin yaw, 0, cos yaw): euler y = -yaw.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public static class MapHandles
    {
        public static Vector3 ToUnity(MapWebVec v) => WebSpace.ToUnity(new Vec3(v.x, v.y, v.z));

        public static MapWebVec FromUnity(Vector3 p)
        {
            Vec3 w = WebSpace.FromUnity(p);
            return new MapWebVec(w.x, w.y, w.z);
        }

        /// <summary>A value on the authoring grid (never -0).</summary>
        public static double Snap(double v)
        {
            double s = Math.Round(v * UnityTuning.MapSnapPerMetre) / UnityTuning.MapSnapPerMetre;
            return s == 0 ? 0 : s;
        }

        public static MapWebVec Snap(MapWebVec v) => new MapWebVec(Snap(v.x), Snap(v.y), Snap(v.z));

        /// <summary>An angle (degrees) on the yaw grid.</summary>
        public static double SnapDegrees(double deg) => Math.Round(deg / UnityTuning.MapYawSnapDegrees) * UnityTuning.MapYawSnapDegrees;

        /// <summary>An angle in (-180, 180] degrees.</summary>
        public static double Wrap180(double deg)
        {
            double d = deg % 360;
            if (d > 180) d -= 360;
            if (d <= -180) d += 360;
            return d == 0 ? 0 : d;
        }

        public static bool Near(Vector3 a, Vector3 b) =>
            Math.Abs(a.x - b.x) <= UnityTuning.MapUnchangedTolerance && Math.Abs(a.y - b.y) <= UnityTuning.MapUnchangedTolerance && Math.Abs(a.z - b.z) <= UnityTuning.MapUnchangedTolerance;

        public static bool SameYaw(float a, float b) => Math.Abs(Mathf.DeltaAngle(a, b)) <= UnityTuning.MapUnchangedYawToleranceDeg;

        // --- the three yaw conventions ---

        public static float FacingDegrees(double yaw) => (float)(180 - yaw * 180 / Math.PI);

        public static double FacingYaw(float eulerY) => Wrap180(180 - SnapDegrees(eulerY)) * Math.PI / 180;

        public static float LookDegrees(double yawDeg) => (float)-yawDeg;

        public static double LookYawDeg(float eulerY) => Wrap180(-SnapDegrees(eulerY));

        public static float WedgeDegrees(double yaw) => (float)(-yaw * 180 / Math.PI);

        public static double WedgeYaw(float eulerY) => Wrap180(-SnapDegrees(eulerY)) * Math.PI / 180;

        /// <summary>Sets a transform's world scale (its parents' scale divided out; the map groups have none).</summary>
        public static void SetWorldScale(Transform t, Vector3 size)
        {
            Vector3 parent = t.parent != null ? t.parent.lossyScale : Vector3.one;
            t.localScale = new Vector3(parent.x != 0 ? size.x / parent.x : size.x, parent.y != 0 ? size.y / parent.y : size.y, parent.z != 0 ? size.z / parent.z : size.z);
        }

        /// <summary>A box's Unity centre and size (z flipped: the web's min z is Unity's max z).</summary>
        public static (Vector3 centre, Vector3 size) BoxTransform(AuthoredBox b)
        {
            var c = new MapWebVec((b.min.x + b.max.x) / 2, (b.min.y + b.max.y) / 2, (b.min.z + b.max.z) / 2);
            return (ToUnity(c), new Vector3((float)Math.Abs(b.max.x - b.min.x), (float)Math.Abs(b.max.y - b.min.y), (float)Math.Abs(b.max.z - b.min.z)));
        }

        /// <summary>A box from a transform: centre and size, each face snapped.</summary>
        public static AuthoredBox BoxFrom(Transform t, AuthoredBox keep)
        {
            MapWebVec c = FromUnity(t.position);
            Vector3 s = t.lossyScale;
            double hx = Math.Abs(s.x) / 2.0, hy = Math.Abs(s.y) / 2.0, hz = Math.Abs(s.z) / 2.0;
            keep.min = Snap(new MapWebVec(c.x - hx, c.y - hy, c.z - hz));
            keep.max = Snap(new MapWebVec(c.x + hx, c.y + hy, c.z + hz));
            return keep;
        }

        // --- gizmos ---

        public static Color Rgb(uint rgb, float alpha = 1) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, alpha);

        /// <summary>A box's gizmo: a faint fill (pickable in the Scene view) under a wire outline.</summary>
        public static void BoxGizmo(Transform t, uint rgb)
        {
            Gizmos.color = Rgb(rgb, UnityTuning.MapGizmoFillAlpha);
            Gizmos.DrawCube(t.position, t.lossyScale);
            Gizmos.color = Rgb(rgb);
            Gizmos.DrawWireCube(t.position, t.lossyScale);
        }

        /// <summary>A point's gizmo, with an arrow along the transform's forward when `facing`.</summary>
        public static void PointGizmo(Transform t, uint rgb, bool facing, bool wire = false)
        {
            Gizmos.color = Rgb(rgb);
            Vector3 p = t.position;
            if (wire) Gizmos.DrawWireSphere(p, UnityTuning.MapGizmoPointRadius);
            else Gizmos.DrawSphere(p, UnityTuning.MapGizmoPointRadius);
            if (facing) Gizmos.DrawLine(p, p + t.forward * UnityTuning.MapGizmoFacingLength);
        }
    }
}
