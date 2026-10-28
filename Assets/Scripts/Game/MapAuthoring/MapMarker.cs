// NIGHT15 U3-28.4 (builder P): the base of every map marker. A marker's exact web values are its serialized fields
// (MapModel's structs); Place() moves its transform to them and remembers where (world position, scale, yaw). On export
// (MapExport), each aspect the transform still has where Place() left it keeps the exact fields; an aspect moved since,
// or a marker never placed (made by AddComponent and moved by hand or by code), exports the transform's values snapped
// to the authoring grid (MapHandles). Edit the numbers in the Inspector and use the context menu "Place from values" to
// move the handle to them; drag the handle and the export follows the handle. No gameplay code reads a marker at run time.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public abstract class MapMarker : MonoBehaviour
    {
        [SerializeField, HideInInspector] bool placed;
        [SerializeField, HideInInspector] Vector3 placedPosition;
        [SerializeField, HideInInspector] Vector3 placedScale;
        [SerializeField, HideInInspector] float placedYaw;

        /// <summary>Moves the transform (and any child handle) to the exact values, and remembers where.</summary>
        public abstract void Place();

        [ContextMenu("Place from values")]
        void PlaceFromValues() => Place();

        /// <summary>Call at the end of Place(): the transform as it is now is "unmoved".</summary>
        protected void MarkPlaced()
        {
            placed = true;
            placedPosition = transform.position;
            placedScale = transform.lossyScale;
            placedYaw = transform.eulerAngles.y;
        }

        public bool PositionMoved => !placed || !MapHandles.Near(transform.position, placedPosition);

        public bool ScaleMoved => !placed || !MapHandles.Near(transform.lossyScale, placedScale);

        public bool YawMoved => !placed || !MapHandles.SameYaw(transform.eulerAngles.y, placedYaw);

        /// <summary>The exact point, or the transform's snapped one when it moved.</summary>
        protected MapWebVec ReadPoint(MapWebVec exact) => PositionMoved ? MapHandles.Snap(MapHandles.FromUnity(transform.position)) : exact;

        /// <summary>A child handle (a pad's target, a trap's switch): its own placed position.</summary>
        protected static bool HandleMoved(Transform handle, bool handlePlaced, Vector3 handlePlacedAt) =>
            handle != null && (!handlePlaced || !MapHandles.Near(handle.position, handlePlacedAt));

        /// <summary>
        /// A second handle (a pad's target, a trap's switch), made when missing as this marker's next sibling ("&lt;name&gt;
        /// &lt;suffix&gt;"), not its child: dragging the marker leaves its target where it is.
        /// </summary>
        protected Transform SiblingHandle(ref Transform handle, string suffix)
        {
            if (handle == null)
            {
                var go = new GameObject($"{name} {suffix}");
                go.transform.SetParent(transform.parent, false);
                go.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
                handle = go.transform;
            }
            return handle;
        }
    }
}
