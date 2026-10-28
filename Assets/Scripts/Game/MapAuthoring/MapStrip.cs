// NIGHT15 U3-28.4 (builder P): an emissive ride strip (data.strips: render only, kind "strip"; no collision). The
// transform's position is its centre and its scale its size, as MapBox.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapStrip : MapMarker
    {
        public AuthoredBox box = new AuthoredBox { kind = "strip" };

        public override void Place()
        {
            (Vector3 centre, Vector3 size) = MapHandles.BoxTransform(box);
            transform.SetPositionAndRotation(centre, Quaternion.identity);
            MapHandles.SetWorldScale(transform, size);
            MarkPlaced();
        }

        public AuthoredBox Read() => PositionMoved || ScaleMoved ? MapHandles.BoxFrom(transform, box) : box;

        void OnDrawGizmos() => MapHandles.BoxGizmo(transform, UnityTuning.MapGizmoUsableRgb);
    }
}
