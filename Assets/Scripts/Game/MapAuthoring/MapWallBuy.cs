// NIGHT15 U3-28.4 (builder P): one gun slot of a wall row (wallBuys.js; one marker per slot, the gun by its order in the
// row): row "wall" is data.armoryWall {x, y, zs, facing} (the guns hang on the plane x = wall.x), "rack" / "back" are
// data.armoryRack / armoryBack {z, y, xs, facing} (the plane z). The transform's position is the slot; a row's plane, y
// and facing are its first slot's (the export warns when the others differ). facing is the fields' only.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapWallBuy : MapMarker
    {
        public string row = MapWallRows.Wall;
        public MapWebVec at;
        public double facing = 1;

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(at), Quaternion.identity);
            MarkPlaced();
        }

        public MapWebVec Read() => ReadPoint(at);

        void OnDrawGizmos()
        {
            MapHandles.PointGizmo(transform, UnityTuning.MapGizmoUsableRgb, false);
            Vector3 p = transform.position;
            Vector3 dir = row == MapWallRows.Wall ? new Vector3((float)facing, 0, 0) : new Vector3(0, 0, (float)-facing); // the web's +x / +z, z flipped
            Gizmos.DrawLine(p, p + dir * UnityTuning.MapGizmoFacingLength);
        }
    }
}
