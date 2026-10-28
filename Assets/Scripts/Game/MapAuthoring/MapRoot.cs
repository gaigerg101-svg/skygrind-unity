// NIGHT15 U3-28.4 (builder P): a map's root: its id (the JSON's name and the file Design/maps/<id>.json), its design
// reason (_why), floor size and kill plane, the start zone and the core spec (core.spec; hasCore false: no core). Every
// marker under it is part of the map (inactive GameObjects are left out). SKYGRIND/Map/Export writes it (MapExport); an
// imported root remembers its source file, for the export's option to keep the source's nav grid while the geometry is
// unchanged. No gameplay code reads it at run time.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Skygrind.Game.MapAuthoring
{
    [DisallowMultipleComponent]
    public sealed class MapRoot : MonoBehaviour
    {
        public string mapId = "newmap";
        [TextArea(2, 8)] public string why = "";
        public double floorSize = 96;
        public double killPlaneY = -30;
        public string startZone = "start";
        public bool hasCore;
        public AuthoredCore core = new AuthoredCore { x0 = -24, z0 = -24, cells = 12, cellSize = 4 };
        [Tooltip("The map JSON this root was imported from (repo-relative), or empty.")]
        public string importedFrom = "";
        [Tooltip("Map data keys no marker carries (e.g. training's signs), as JSON; exported as they are.")]
        [TextArea(1, 4)] public string extraDataJson = "";

        /// <summary>The imported map JSON while this Editor session holds it (MapNavMode.KeepIfGeometryUnchanged reads it).</summary>
        [NonSerialized] public Dictionary<string, object> importedJson;

        void OnDrawGizmos()
        {
            // The floor square (the nav grid's) at y 0 and the kill plane under it, in the boundary's pale violet-white.
            float h = (float)(floorSize / 2);
            Gizmos.color = MapHandles.Rgb(UnityTuning.MapGizmoBoundaryRgb);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(2 * h, 0, 2 * h));
            Gizmos.color = MapHandles.Rgb(UnityTuning.MapGizmoBoundaryRgb, 0.35f);
            Gizmos.DrawWireCube(new Vector3(0, (float)killPlaneY, 0), new Vector3(2 * h, 0, 2 * h));
            if (!hasCore) return;
            // The core grid's square (web x0, z0 .. + cells * cellSize; Unity z flipped).
            float size = (float)(core.cells * core.cellSize);
            var centre = MapHandles.ToUnity(new MapWebVec(core.x0 + size / 2, 0, core.z0 + size / 2));
            Gizmos.color = MapHandles.Rgb(UnityTuning.MapGizmoStoneRgb);
            Gizmos.DrawWireCube(centre, new Vector3(size, 0, size));
        }
    }
}
