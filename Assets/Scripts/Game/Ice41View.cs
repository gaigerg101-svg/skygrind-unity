// NIGHT41 K2 round 2 (design.n41.look41; INVENTORY A3: "level 2 does not read as an ice canal"): level 2's canal look as VIEW props, built once
// at the level's build from the map's own boxes (no level data edited; NIGHT40 L2's stone tint, UnityTuning.Ice40*, stays as it is):
// - frost: a ragged skirt of pale blue-white crust round the base of every wall (kind "wall", >= Ice41WallMinH tall), in pieces of hashed
//   heights with some left bare, on the lit stone material (it takes the room's light like the floor);
// - sheen: faint additive streaks of blue-white polish along each floor's long side (the canal's way), as a lit ice floor shows its lamps;
// - the canal's frozen floor (campaign.surfaces kind "ice"): one whole faint glaze over it and brighter streaks, so the channel reads as ice.
// No collider anywhere (the sim collides on the C# web collider; nothing here is in it). Deterministic (a hash per piece; no RNG). Only while
// look41 is on and the level's theme is iceline (Look41.ThemeFor == 1); otherwise nothing is built. Numbers: UnityTuning.Look41.cs.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Ice41View
    {
        /// <summary>For the reel line / the report: what the last build drew (0 when look41 is off or the level is not iceline).</summary>
        public static int FrostPieces, SheenStreaks, Glazes;

        /// <summary>The iceline theme's index in LightPlan33.Themes.</summary>
        const int IcelineTheme = 1;

        /// <summary>Builds the canal props under `parent` when look41 is on and the map is an iceline level; null otherwise.</summary>
        public static GameObject Create(SurvivalGame g, Transform parent, SurvivalMaterials mats)
        {
            FrostPieces = SheenStreaks = Glazes = 0;
            if (g == null || g.map == null || g.map.json == null || mats == null) return null;
            if (Look41.ThemeFor(g.cfg, g.map.json) != IcelineTheme) return null;
            var root = new GameObject("Ice 41");
            root.transform.SetParent(parent, false);
            Color frost = Look.Hex(UnityTuning.Ice41FrostHex);
            var frostKit = new MeshKit();
            var sheenKit = new MeshKit();
            var glazeKit = new MeshKit();
            var glazeStreakKit = new MeshKit();
            int boxIndex = 0;
            foreach (MapBox b in g.map.boxes)
            {
                boxIndex++;
                Vector3 a = WebSpace.ToUnity(b.min), c = WebSpace.ToUnity(b.max);
                Vector3 lo = Vector3.Min(a, c), hi = Vector3.Max(a, c);
                if (b.kind == "wall" && hi.y - lo.y >= UnityTuning.Ice41WallMinH)
                {
                    lo.y = BaseY(g.map, b); // a wall sunk into the floor: its skirt from the floor it stands in, not under it
                    if (hi.y - lo.y >= UnityTuning.Ice41WallMinH) FrostSkirt(frostKit, lo, hi, boxIndex, frost);
                }
                else if (b.kind == "floor") Streaks(sheenKit, lo.x, lo.z, hi.x, hi.z, hi.y + (float)UnityTuning.Ice41SheenLift, boxIndex);
            }
            foreach (Dictionary<string, object> s in IceSurfaces(g.map.json))
            {
                if (!(s.TryGetValue("rect", out object ro) && ro is List<object> rc && rc.Count >= 4)) continue;
                double minY = s.TryGetValue("minY", out object mn) ? PortData.Num(mn) : -1e9, maxY = s.TryGetValue("maxY", out object mx) ? PortData.Num(mx) : 1e9;
                double x0 = PortData.Num(rc[0]), z0 = PortData.Num(rc[1]), x1 = PortData.Num(rc[2]), z1 = PortData.Num(rc[3]);
                double top = FloorTop(g.map, x0, z0, x1, z1, minY, maxY);
                // web -> Unity: z flips
                float ux0 = (float)System.Math.Min(x0, x1), ux1 = (float)System.Math.Max(x0, x1), uz0 = (float)System.Math.Min(-z0, -z1), uz1 = (float)System.Math.Max(-z0, -z1);
                float y = (float)(top + UnityTuning.Ice41SheenLift * 1.5); // just over the floor's own streaks
                Flat(glazeKit, ux0, uz0, ux1, uz1, y);
                Glazes++;
                Streaks(glazeStreakKit, ux0, uz0, ux1, uz1, y + 0.002f, 1000 + Glazes);
            }
            MeshKit.Object("Ice 41 frost", root.transform, frostKit.Build("ice41 frost"), mats.stone, shadows: false);
            Additive(root.transform, mats, "Ice 41 sheen", sheenKit, UnityTuning.Ice41SheenHex, UnityTuning.Ice41SheenGain);
            Additive(root.transform, mats, "Ice 41 glaze", glazeKit, UnityTuning.Ice41GlazeHex, UnityTuning.Ice41GlazeGain);
            Additive(root.transform, mats, "Ice 41 glaze streaks", glazeStreakKit, UnityTuning.Ice41SheenHex, UnityTuning.Ice41GlazeStreakGain);
            return root;
        }

        static void Additive(Transform parent, SurvivalMaterials mats, string name, MeshKit kit, uint hex, float gain)
        {
            Material m = FxDraw.Additive(mats.neon, name);
            m.SetColor("_BaseColor", Look.Hex(hex) * gain);
            MeshKit.Object(name, parent, kit.Build(name.ToLowerInvariant()), m, shadows: false);
        }

        static IEnumerable<Dictionary<string, object>> IceSurfaces(Dictionary<string, object> json)
        {
            if (!(json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> c && c.TryGetValue("surfaces", out object so) && so is List<object> list)) yield break;
            foreach (object o in list)
                if (o is Dictionary<string, object> s && s.TryGetValue("kind", out object k) && k as string == "ice") yield return s;
        }

        /// <summary>The top of the floor box under an ice rect (web space) whose top lies in [minY, maxY]; the band's middle when none does.</summary>
        static double FloorTop(SurvivalMap map, double x0, double z0, double x1, double z1, double minY, double maxY)
        {
            foreach (MapBox b in map.boxes)
            {
                if (b.kind != "floor" || b.max.y < minY || b.max.y > maxY) continue;
                if (b.max.x <= System.Math.Min(x0, x1) || b.min.x >= System.Math.Max(x0, x1) || b.max.z <= System.Math.Min(z0, z1) || b.min.z >= System.Math.Max(z0, z1)) continue;
                return b.max.y;
            }
            return minY > -1e8 && maxY < 1e8 ? (minY + maxY) * 0.5 : 0;
        }

        /// <summary>The y a wall's skirt starts at (web y = Unity y): the highest floor-box top that touches the wall's footprint (within 0.1 m) and lies
        /// inside the wall's height; the wall's own bottom when no floor touches it.</summary>
        static float BaseY(SurvivalMap map, MapBox w)
        {
            double y = w.min.y;
            const double near = 0.1;
            foreach (MapBox f in map.boxes)
            {
                if (f.kind != "floor" || f.max.y <= y || f.max.y >= w.max.y) continue;
                if (f.max.x < w.min.x - near || f.min.x > w.max.x + near || f.max.z < w.min.z - near || f.min.z > w.max.z + near) continue;
                y = f.max.y;
            }
            return (float)y;
        }

        /// <summary>A stable 0-1 value per (box, piece, salt): no RNG in a view that must look the same every run.</summary>
        static float H(int a, int b, int salt)
        {
            unchecked
            {
                uint h = (uint)(a * 73856093) ^ (uint)(b * 19349663) ^ (uint)(salt * 83492791);
                h ^= h >> 13; h *= 0x5bd1e995; h ^= h >> 15;
                return (h & 0xffffff) / 16777216f;
            }
        }

        /// <summary>The frost skirt round a wall's base: each face in pieces of Ice41FrostSeg, each of a hashed height, some left bare.</summary>
        static void FrostSkirt(MeshKit kit, Vector3 lo, Vector3 hi, int box, Color frost)
        {
            float o = (float)UnityTuning.Ice41FrostOut, wallH = hi.y - lo.y;
            for (int face = 0; face < 4; face++)
            {
                bool alongX = face < 2;
                float from = alongX ? lo.x : lo.z, to = alongX ? hi.x : hi.z, len = to - from;
                int n = Mathf.Max(1, Mathf.CeilToInt(len / (float)UnityTuning.Ice41FrostSeg));
                for (int k = 0; k < n; k++)
                {
                    if (H(box, face * 1000 + k, 1) > UnityTuning.Ice41FrostFill) continue;
                    float s0 = from + len * k / n, s1 = from + len * (k + 1) / n;
                    float h = Mathf.Min(wallH, Mathf.Lerp((float)UnityTuning.Ice41FrostMinH, (float)UnityTuning.Ice41FrostMaxH, H(box, face * 1000 + k, 2)));
                    Vector3 pLo, pHi;
                    switch (face)
                    {
                        case 0: pLo = new Vector3(s0, lo.y, lo.z - o); pHi = new Vector3(s1, lo.y + h, lo.z); break; // the -z face
                        case 1: pLo = new Vector3(s0, lo.y, hi.z); pHi = new Vector3(s1, lo.y + h, hi.z + o); break; // the +z face
                        case 2: pLo = new Vector3(lo.x - o, lo.y, s0); pHi = new Vector3(lo.x, lo.y + h, s1); break; // the -x face
                        default: pLo = new Vector3(hi.x, lo.y, s0); pHi = new Vector3(hi.x + o, lo.y + h, s1); break; // the +x face
                    }
                    kit.BoxUnity(pLo, pHi, frost, frost, frost, skipBottom: true);
                    FrostPieces++;
                }
            }
        }

        /// <summary>Sheen streaks over a floor rect (Unity space) at height y, along its long side; one per Ice41SheenSpacing of the short side.</summary>
        static void Streaks(MeshKit kit, float x0, float z0, float x1, float z1, float y, int box)
        {
            float sx = x1 - x0, sz = z1 - z0;
            if (sx <= 0.5f || sz <= 0.5f) return;
            bool alongX = sx >= sz;
            float lng = alongX ? sx : sz, shrt = alongX ? sz : sx;
            int n = Mathf.Clamp(Mathf.RoundToInt(shrt / (float)UnityTuning.Ice41SheenSpacing), 1, UnityTuning.Ice41SheenMax);
            for (int i = 0; i < n; i++)
            {
                float w = Mathf.Min(shrt / n, Mathf.Lerp((float)UnityTuning.Ice41SheenMinW, (float)UnityTuning.Ice41SheenMaxW, H(box, i, 3)));
                float slot = shrt / n, mid = slot * (i + 0.5f) + (H(box, i, 4) - 0.5f) * (slot - w);
                float len = lng * Mathf.Lerp((float)UnityTuning.Ice41SheenMinLen, (float)UnityTuning.Ice41SheenMaxLen, H(box, i, 5));
                float start = (lng - len) * H(box, i, 6);
                float a0 = start, a1 = start + len, b0 = mid - w * 0.5f, b1 = mid + w * 0.5f;
                if (alongX) Flat(kit, x0 + a0, z0 + b0, x0 + a1, z0 + b1, y);
                else Flat(kit, x0 + b0, z0 + a0, x0 + b1, z0 + a1, y);
                SheenStreaks++;
            }
        }

        /// <summary>A flat up-facing quad (Unity space), wound as MeshKit.BoxUnity's top face.</summary>
        static void Flat(MeshKit kit, float x0, float z0, float x1, float z1, float y) =>
            kit.Quad(new Vector3(x0, y, z0), new Vector3(x1, y, z0), new Vector3(x1, y, z1), new Vector3(x0, y, z1), Vector3.up, Color.white);
    }
}
