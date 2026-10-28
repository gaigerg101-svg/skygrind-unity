// NIGHT15 U3-28.4 (builder P): a boundary barrier (data.barriers / collision.barriers, kind "barrier": an invisible wall
// or cap that collides). The transform's position is its centre and its scale its size, as MapBox.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapBarrier : MapMarker
    {
        public AuthoredBox box = new AuthoredBox { kind = "barrier" };

        public override void Place()
        {
            (Vector3 centre, Vector3 size) = MapHandles.BoxTransform(box);
            transform.SetPositionAndRotation(centre, Quaternion.identity);
            MapHandles.SetWorldScale(transform, size);
            MarkPlaced();
        }

        public AuthoredBox Read() => PositionMoved || ScaleMoved ? MapHandles.BoxFrom(transform, box) : box;

        void OnDrawGizmos()
        {
            Gizmos.color = MapHandles.Rgb(UnityTuning.MapGizmoBoundaryRgb);
            Gizmos.DrawWireCube(transform.position, transform.lossyScale); // wire only: a barrier is invisible in the game
        }
    }
}
