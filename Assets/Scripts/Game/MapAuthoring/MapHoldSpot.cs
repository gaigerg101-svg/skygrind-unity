// NIGHT15 U3-28.4 (builder P): a HOLD ring's centre (data.holdSpots: x, y, z; objectives.js): the transform's position.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapHoldSpot : MapMarker
    {
        public MapWebVec at;
        public string verb = ""; // a designed verb gate (UnityTuning.MapVerbs; "" none): data.holdSpots[i].verb

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(at), Quaternion.identity);
            MarkPlaced();
        }

        public MapWebVec Read() => ReadPoint(at);

        void OnDrawGizmos() => MapHandles.PointGizmo(transform, UnityTuning.MapGizmoUsableRgb, false, true);
    }
}
