// NIGHT15 U3-28.4 (builder P): an ARCLINE grapple node (grappleNodes: x, y, z): the transform's position.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapGrapple : MapMarker
    {
        public MapWebVec at;

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(at), Quaternion.identity);
            MarkPlaced();
        }

        public MapWebVec Read() => ReadPoint(at);

        void OnDrawGizmos() => MapHandles.PointGizmo(transform, UnityTuning.MapGizmoUsableRgb, false, true);
    }
}
