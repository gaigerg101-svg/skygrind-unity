// Builds the arena from ArenaMap. Collision is the web game's: one MeshCollider per triangle of three's Octree
// (UnityColliders.FromTriangles), so the faces three drops on the outer max-x / max-z planes are left out as in the
// web game; the Grinder's pillars are BoxColliders that CoreView resizes as the core moves. Render (no colliders):
// meshes with world-space UVs on a 1 m grid texture, so speed reads on every surface.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class ArenaBuilder
    {
        static readonly Color Floor = new Color(0.36f, 0.38f, 0.42f);
        static readonly Color Stone = new Color(0.62f, 0.60f, 0.56f);
        static readonly Color Feature = new Color(0.50f, 0.58f, 0.66f);
        static readonly Color Ramp = new Color(0.86f, 0.52f, 0.22f);
        static readonly Color MeshPiece = new Color(0.72f, 0.28f, 0.52f);
        static readonly Color Cyan = new Color(0.10f, 0.83f, 0.90f);

        /// <summary>The material set the builders share (grid-textured stone and floor, the ramp orange, the neon cyan).</summary>
        public static Material Lit(Material baseMaterial, Color c, bool glow = false)
        {
            var m = new Material(baseMaterial) { color = c };
            if (glow)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", c * 2.4f);
            }
            return m;
        }

        public static GameObject Build(ArenaMap map, Material baseMaterial, ThreeOctree octree)
        {
            var root = new GameObject("Arena");
            UnityColliders.FromTriangles(octree.StoredTriangles(), "Collision (web Octree triangles)").transform.SetParent(root.transform, false);
            Texture2D grid = GridTexture();
            Material Mat(Color c, bool glow = false)
            {
                var m = new Material(baseMaterial) { color = c };
                if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", glow ? null : grid);
                if (glow)
                {
                    m.EnableKeyword("_EMISSION");
                    m.SetColor("_EmissionColor", c * 2.4f);
                }
                return m;
            }
            Material floor = Mat(Floor), stone = Mat(Stone), feature = Mat(Feature), ramp = Mat(Ramp), piece = Mat(MeshPiece), cyan = Mat(Cyan, true);

            foreach (BoxSpec b in map.boxes)
            {
                Material m = b.kind == "floor" ? floor : b.kind == "stone" ? stone : feature;
                Box(root.transform, b, m);
            }
            foreach (WedgeSpec w in map.wedges) Wedge(root.transform, w, ramp);
            MeshPieceObject(root.transform, map.meshPiece, piece);
            foreach (PadSpec p in map.pads) Pad(root.transform, p, cyan, map);
            for (int i = 0; i < map.grappleNodes.Count; i++) Node(root.transform, map.grappleNodes[i], i + 1, cyan);
            Physics.SyncTransforms();
            return root;
        }

        static void Box(Transform parent, BoxSpec b, Material m)
        {
            Vector3 u0 = WebSpace.ToUnity(b.min);
            Vector3 u1 = WebSpace.ToUnity(b.max);
            Vector3 min = Vector3.Min(u0, u1), max = Vector3.Max(u0, u1);
            var go = new GameObject($"Box {b.kind}");
            go.transform.SetParent(parent, false);
            go.transform.position = (min + max) * 0.5f;
            AddMesh(go, BoxTriangles(min - go.transform.position, max - go.transform.position), go.transform.position, m);
        }

        static void Wedge(Transform parent, WedgeSpec w, Material m)
        {
            Vector3[] v = System.Array.ConvertAll(ArenaMap.WedgeCorners(w), WebSpace.ToUnity);
            Vector3 lowL = v[0], lowR = v[1], highL = v[2], highR = v[3], topL = v[4], topR = v[5];
            Vector3 center = (lowL + lowR + highL + highR + topL + topR) / 6;
            var go = new GameObject("Ramp");
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            var tris = new List<Vector3>();
            void T(Vector3 a, Vector3 b, Vector3 c) => Outward(tris, a - center, b - center, c - center, Vector3.zero);
            T(lowL, lowR, topR); T(lowL, topR, topL); // slope
            T(highL, topL, topR); T(highL, topR, highR); // back
            T(lowL, topL, highL); T(lowR, highR, topR); // sides
            T(lowL, highL, highR); T(lowL, highR, lowR); // bottom (render only; the web prism has none)
            AddMesh(go, tris, center, m);
        }

        /// <summary>The spike's L piece (render; its collision is in the triangle build).</summary>
        static void MeshPieceObject(Transform parent, List<BoxSpec> boxes, Material m)
        {
            if (boxes.Count == 0) return;
            var go = new GameObject("L piece");
            go.transform.SetParent(parent, false);
            var tris = new List<Vector3>();
            foreach (BoxSpec b in boxes)
            {
                Vector3 u0 = WebSpace.ToUnity(b.min), u1 = WebSpace.ToUnity(b.max);
                tris.AddRange(BoxTriangles(Vector3.Min(u0, u1), Vector3.Max(u0, u1)));
            }
            AddMesh(go, tris, Vector3.zero, m);
        }

        static void Pad(Transform parent, PadSpec p, Material m, ArenaMap map)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = p.aimed ? "Aimed pad" : "Jump pad";
            Kill(go.GetComponent<Collider>()); // pads never collide (the floor underneath does)
            go.transform.SetParent(parent, false);
            go.transform.position = WebSpace.ToUnity(p.pos) + new Vector3(0, 0.02f, 0);
            float d = 2f * 1.0f; // jumpPad.radius
            go.transform.localScale = new Vector3(d, 0.02f, d);
            go.GetComponent<Renderer>().sharedMaterial = m;
        }

        static void Node(Transform parent, Vec3 p, int id, Material m)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"Grapple node {id}";
            Kill(go.GetComponent<Collider>()); // no collision (they float out of reach)
            go.transform.SetParent(parent, false);
            go.transform.position = WebSpace.ToUnity(p);
            go.transform.localScale = Vector3.one * 1.1f;
            go.GetComponent<Renderer>().sharedMaterial = m;
        }

        /// <summary>Destroy that also works in edit mode (the EditMode parity tests build the arena too).</summary>
        static void Kill(Object o)
        {
            if (Application.isPlaying) Object.Destroy(o);
            else Object.DestroyImmediate(o);
        }

        // --- meshes ---------------------------------------------------------------------------------------------

        static List<Vector3> BoxTriangles(Vector3 min, Vector3 max)
        {
            var tris = new List<Vector3>();
            Vector3 c = (min + max) * 0.5f;
            Vector3 P(int i) => new Vector3((i & 1) == 0 ? min.x : max.x, (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z);
            int[][] quads = { new[] { 0, 1, 3, 2 }, new[] { 4, 5, 7, 6 }, new[] { 0, 1, 5, 4 }, new[] { 2, 3, 7, 6 }, new[] { 0, 2, 6, 4 }, new[] { 1, 3, 7, 5 } };
            foreach (int[] q in quads)
            {
                Outward(tris, P(q[0]), P(q[1]), P(q[2]), c);
                Outward(tris, P(q[0]), P(q[2]), P(q[3]), c);
            }
            return tris;
        }

        /// <summary>Adds a triangle wound so Unity treats the side away from `inside` as its front.</summary>
        static void Outward(List<Vector3> tris, Vector3 a, Vector3 b, Vector3 c, Vector3 inside)
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(n, (a + b + c) / 3 - inside) < 0) (b, c) = (c, b);
            tris.Add(a);
            tris.Add(b);
            tris.Add(c);
        }

        /// <summary>A flat-shaded mesh with world-space planar UVs (1 unit = 1 m of grid).</summary>
        static Mesh AddMesh(GameObject go, List<Vector3> tris, Vector3 origin, Material m)
        {
            var uvs = new Vector2[tris.Count];
            var idx = new int[tris.Count];
            for (int i = 0; i < tris.Count; i += 3)
            {
                Vector3 n = Vector3.Cross(tris[i + 1] - tris[i], tris[i + 2] - tris[i]).normalized;
                for (int k = 0; k < 3; k++)
                {
                    Vector3 w = tris[i + k] + origin;
                    float ax = Mathf.Abs(n.x), ay = Mathf.Abs(n.y), az = Mathf.Abs(n.z);
                    uvs[i + k] = ay >= ax && ay >= az ? new Vector2(w.x, w.z) : ax >= az ? new Vector2(w.z, w.y) : new Vector2(w.x, w.y);
                    idx[i + k] = i + k;
                }
            }
            var mesh = new Mesh { name = go.name };
            mesh.SetVertices(tris);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(idx, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = m;
            return mesh;
        }

        /// <summary>A 1 m grid: one darker line per meter on each axis.</summary>
        static Texture2D GridTexture()
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Trilinear, anisoLevel = 8, name = "Grid" };
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool line = x < 3 || y < 3;
                    byte v = line ? (byte)150 : (byte)255;
                    px[y * size + x] = new Color32(v, v, v, 255);
                }
            }
            tex.SetPixels32(px);
            tex.Apply(true);
            return tex;
        }
    }
}
