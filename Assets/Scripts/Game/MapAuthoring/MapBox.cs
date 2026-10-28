// NIGHT15 U3-28.4 (builder P): a solid axis-aligned box (data.boxes; collision.boxes). The transform's position is its
// centre and its scale its size (rotation is ignored: boxes are axis-aligned). kind is the web's box kind (wall, bridge,
// cover, slab, spire, ledge, island, stone; RIVAL's graph walks the kinds in config nav.walkWalkers, rims go on
// world.rimKinds). Kind "floor": the first floor box under the MapRoot is the map's floor (map.floor, collision only);
// further floor boxes are extra floor slabs (data.boxes, drawn and collided); with none the export adds the web
// exporter's floor from floorSize. Note: Skygrind.Core also has a struct MapBox (the parsed box); this is the marker.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapBox : MapMarker
    {
        public AuthoredBox box = new AuthoredBox { kind = "" };

        public override void Place()
        {
            (Vector3 centre, Vector3 size) = MapHandles.BoxTransform(box);
            transform.SetPositionAndRotation(centre, Quaternion.identity);
            MapHandles.SetWorldScale(transform, size);
            MarkPlaced();
        }

        /// <summary>The box as exported: exact, or the transform's (snapped) when it moved or was resized.</summary>
        public AuthoredBox Read() => PositionMoved || ScaleMoved ? MapHandles.BoxFrom(transform, box) : box;

        public bool IsFloor => !box.kindUndefined && box.kind == "floor";

        void OnDrawGizmos() => MapHandles.BoxGizmo(transform, UnityTuning.MapGizmoStoneRgb);
    }
}
