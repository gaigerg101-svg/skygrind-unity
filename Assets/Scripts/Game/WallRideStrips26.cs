// NIGHT26 (builder M, design.n26.wallRide): a cyan light strip along every long wall a player can ride (GG: wall riding should be the route the
// eye finds; RESEARCH.md S1: a guide line along the surface). View only: nothing here touches the sim. A "wall" box of the level whose long side
// is at least design.n26.wallRide.stripMinLength and whose top is at least stripHeight + stripClear above its base gets one thin emissive strip
// on each long face at stripHeight (the ride's own height band). Cyan only (player-usable; never red-orange or magenta). Created by
// CampaignLighting24 for a campaign map when the row is on; the strips are built once at the level's start.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class WallRideStrips26
    {
        /// <summary>The rideable-wall strips for the level's boxes under `parent`; returns how many strips were made (0 with the row off).</summary>
        public static int Build(SurvivalGame g, Transform parent, SurvivalMaterials mats)
        {
            GameConfig cfg = g.cfg;
            if (!cfg.Design("n26.wallRide.enabled") || N26Blocks.ForceOff || g.map == null) return 0;
            double minLen = cfg.Num("design.n26.wallRide.stripMinLength");
            double y = cfg.Num("design.n26.wallRide.stripHeight"), clear = cfg.Num("design.n26.wallRide.stripClear");
            float th = (float)cfg.Num("design.n26.wallRide.stripThickness"), off = (float)cfg.Num("design.n26.wallRide.stripOffset");
            var m = new Material(mats.neon);
            m.SetColor("_BaseColor", Look.C(cfg, "colors.usable") * (float)cfg.Num("design.n26.wallRide.stripBrightness"));
            var root = new GameObject("Wall ride strips");
            root.transform.SetParent(parent, false);
            int n = 0;
            foreach (MapBox b in g.map.boxes)
            {
                if (b.kind != "wall") continue;
                double dx = b.max.x - b.min.x, dz = b.max.z - b.min.z;
                bool alongX = dx >= dz;
                double len = alongX ? dx : dz;
                if (len < minLen || b.max.y - b.min.y < y + clear) continue;
                float sy = (float)(b.min.y + y);
                if (alongX)
                {
                    Strip(root.transform, m, (float)(b.min.x + b.max.x) * 0.5f, sy, (float)b.min.z - off, (float)len, th, th);
                    Strip(root.transform, m, (float)(b.min.x + b.max.x) * 0.5f, sy, (float)b.max.z + off, (float)len, th, th);
                }
                else
                {
                    Strip(root.transform, m, (float)b.min.x - off, sy, (float)(b.min.z + b.max.z) * 0.5f, th, th, (float)len);
                    Strip(root.transform, m, (float)b.max.x + off, sy, (float)(b.min.z + b.max.z) * 0.5f, th, th, (float)len);
                }
                n += 2;
            }
            return n;
        }

        static void Strip(Transform parent, Material m, float cx, float cy, float cz, float sx, float sy, float sz)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Cube);
            s.name = "Ride strip";
            Object.Destroy(s.GetComponent<Collider>());
            s.transform.SetParent(parent, false);
            s.transform.position = new Vector3(cx, cy, cz); // the map builder uses the web coordinates as they are
            s.transform.localScale = new Vector3(sx, sy, sz);
            s.GetComponent<MeshRenderer>().sharedMaterial = m;
        }
    }
}
