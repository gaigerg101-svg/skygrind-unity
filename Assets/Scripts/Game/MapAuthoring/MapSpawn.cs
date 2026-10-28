// NIGHT15 U3-28.4 (builder P): a map spawn (data.spawns.ground / high: x, y, z): the transform's position. In the Unity
// game only the enemy bench reads these lists (EnemyBench: spawns.ground); SURVIVAL samples its own spawn nodes on the
// map's surfaces (Zones, config zones.nodeSpacing), which SKYGRIND/Map/Validate counts per zone.
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapSpawn : MapMarker
    {
        public MapWebVec at;
        public bool high;

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(at), Quaternion.identity);
            MarkPlaced();
        }

        public MapWebVec Read() => ReadPoint(at);

        void OnDrawGizmos() => MapHandles.PointGizmo(transform, UnityTuning.MapGizmoDangerRgb, false, high);
    }
}
