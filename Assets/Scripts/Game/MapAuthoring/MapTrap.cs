// NIGHT15 U3-28.4 (builder P): a trap (data.trapSpots: trap, x, y, z, sx, sz; traps.js): the transform's position is
// the trap's centre (feet); its switch at (sx, y, sz) is the handle `switchAt` (a sibling made by Place).
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapTrap : MapMarker
    {
        public AuthoredTrap spot = new AuthoredTrap { trap = "plate" };
        public Transform switchAt;
        [SerializeField, HideInInspector] bool switchPlaced;
        [SerializeField, HideInInspector] Vector3 switchPlacedAt;

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(spot.at), Quaternion.identity);
            SiblingHandle(ref switchAt, "switch").position = MapHandles.ToUnity(new MapWebVec(spot.sx, spot.at.y, spot.sz));
            switchPlaced = true;
            switchPlacedAt = switchAt.position;
            MarkPlaced();
        }

        public AuthoredTrap Read()
        {
            AuthoredTrap t = spot;
            t.at = ReadPoint(spot.at);
            if (HandleMoved(switchAt, switchPlaced, switchPlacedAt))
            {
                MapWebVec s = MapHandles.Snap(MapHandles.FromUnity(switchAt.position));
                t.sx = s.x;
                t.sz = s.z;
            }
            return t;
        }

        void OnDrawGizmos()
        {
            MapHandles.PointGizmo(transform, UnityTuning.MapGizmoDangerRgb, false);
            if (switchAt == null) return;
            Gizmos.DrawLine(transform.position, switchAt.position);
            Gizmos.DrawWireSphere(switchAt.position, UnityTuning.MapGizmoPointRadius);
        }
    }
}
