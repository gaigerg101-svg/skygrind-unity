// NIGHT15 U3-28.4 (builder P): a jump pad (pads: x, y, z; aimed: target {x, y, z} and apexAbove, launched with
// padLaunchVelocity; plain: "target": null, straight up to config jumpPad.apexHeight). The transform is the pad (its
// feet); an aimed pad's target is the handle `target` (a sibling Place makes): dragging it re-aims the pad, and dragging
// the pad leaves the target where it is.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapPad : MapMarker
    {
        public AuthoredPad pad = new AuthoredPad { aimed = true, apexAbove = 2.5 };
        public Transform target;
        [SerializeField, HideInInspector] bool targetPlaced;
        [SerializeField, HideInInspector] Vector3 targetPlacedAt;

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(pad.at), Quaternion.identity);
            if (pad.aimed)
            {
                SiblingHandle(ref target, "target").position = MapHandles.ToUnity(pad.target);
                targetPlaced = true;
                targetPlacedAt = target.position;
            }
            MarkPlaced();
        }

        public AuthoredPad Read()
        {
            AuthoredPad p = pad;
            p.at = ReadPoint(pad.at);
            if (p.aimed && HandleMoved(target, targetPlaced, targetPlacedAt)) p.target = MapHandles.Snap(MapHandles.FromUnity(target.position));
            return p;
        }

        void OnDrawGizmos()
        {
            MapHandles.PointGizmo(transform, UnityTuning.MapGizmoUsableRgb, false);
            if (!pad.aimed || target == null) return;
            Gizmos.color = MapHandles.Rgb(UnityTuning.MapGizmoUsableRgb);
            Vector3 a = transform.position, b = target.position;
            Vector3 apex = (a + b) / 2;
            apex.y = Mathf.Max(a.y, b.y) + (float)pad.apexAbove;
            Gizmos.DrawLine(a, apex);
            Gizmos.DrawLine(apex, b);
            Gizmos.DrawWireSphere(b, UnityTuning.MapGizmoPointRadius);
        }
    }
}
