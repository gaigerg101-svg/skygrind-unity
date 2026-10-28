// NIGHT15 U3-28.4 (builder P): the player spawn (spawns.playerSpawn / data.playerSpawn: x, y, z, yawDeg) or, with duel
// set, a Duel spawn (duelSpawns). The transform's position is the feet, its forward the look (euler y = -yawDeg).
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    public sealed class MapPlayerSpawn : MapMarker
    {
        public AuthoredPlayerSpawn spawn = new AuthoredPlayerSpawn { raw = "" };
        public bool duel;

        public override void Place()
        {
            transform.SetPositionAndRotation(MapHandles.ToUnity(spawn.at), Quaternion.Euler(0, MapHandles.LookDegrees(spawn.yawDeg), 0));
            MarkPlaced();
        }

        public AuthoredPlayerSpawn Read()
        {
            AuthoredPlayerSpawn s = spawn;
            if (!string.IsNullOrEmpty(s.raw) && !PositionMoved && !YawMoved) return s; // a verbatim {"$config": ...} spawn
            s.raw = "";
            s.at = ReadPoint(spawn.at);
            if (YawMoved) s.yawDeg = MapHandles.LookYawDeg(transform.eulerAngles.y);
            return s;
        }

        void OnDrawGizmos() => MapHandles.PointGizmo(transform, UnityTuning.MapGizmoUsableRgb, true, duel);
    }
}
