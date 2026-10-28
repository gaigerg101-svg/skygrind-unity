// Flat-shaded mesh building for the map, the gates and the blocky kit: boxes and quads in web coordinates
// (converted with WebSpace: z flips, so winding flips too), with per-face vertex colors. One mesh per material.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class MeshKit
    {
        readonly List<Vector3> v = new List<Vector3>();
        readonly List<Vector3> n = new List<Vector3>();
        readonly List<Color> c = new List<Color>();
        readonly List<int> t = new List<int>();
        // NIGHT51 (stone51): each face's own coordinates in metres in uv3 (u, v, the face's width, its height; on a wall v runs up from
        // its foot), so SKYGRIND/Stone draws panel seams, bevels and grime that meet the box's edges. A triangle carries size 0 (none drawn).
        readonly List<Vector4> f = new List<Vector4>();

        public int Vertices => v.Count;

        public void Clear()
        {
            v.Clear();
            n.Clear();
            c.Clear();
            t.Clear();
            f.Clear();
        }

        /// <summary>NIGHT51: the face coordinates of the last `count` vertices (a quad's or a triangle's), in uv3.</summary>
        void Face(int i, int count, Vector3 normal, bool edges)
        {
            // the axes from the normal, not the corner order (n51-r3: quads listed a -> b along a diagonal drew diagonal seams): a wall's v
            // runs up, a top's u along world x (z when the face is tilted about x); an axis-aligned box's edges lie on them
            Vector3 nn = normal.normalized, ua, va;
            if (Mathf.Abs(nn.y) < 0.5f)
            {
                va = Vector3.ProjectOnPlane(Vector3.up, nn).normalized;
                ua = Vector3.Cross(va, nn).normalized;
            }
            else
            {
                ua = Vector3.ProjectOnPlane(Vector3.right, nn);
                if (ua.sqrMagnitude < 1e-4f) ua = Vector3.ProjectOnPlane(Vector3.forward, nn);
                ua.Normalize();
                va = Vector3.Cross(nn, ua).normalized;
            }
            float minU = float.MaxValue, minV = float.MaxValue, maxU = float.MinValue, maxV = float.MinValue;
            for (int k = 0; k < count; k++)
            {
                float pu = Vector3.Dot(v[i + k], ua), pv = Vector3.Dot(v[i + k], va);
                minU = Mathf.Min(minU, pu); maxU = Mathf.Max(maxU, pu);
                minV = Mathf.Min(minV, pv); maxV = Mathf.Max(maxV, pv);
            }
            float w = edges ? maxU - minU : 0, h = edges ? maxV - minV : 0;
            for (int k = 0; k < count; k++) f.Add(new Vector4(Vector3.Dot(v[i + k], ua) - minU, Vector3.Dot(v[i + k], va) - minV, w, h));
        }

        /// <summary>A quad from four Unity-space corners (any winding: it faces `normal`).</summary>
        public void Quad(Vector3 a, Vector3 b, Vector3 cc, Vector3 d, Vector3 normal, Color col)
        {
            int i = v.Count;
            v.Add(a); v.Add(b); v.Add(cc); v.Add(d);
            for (int k = 0; k < 4; k++)
            {
                n.Add(normal);
                c.Add(col);
            }
            Face(i, 4, normal, true);
            bool front = Vector3.Dot(Vector3.Cross(b - a, cc - a), normal) > 0;
            // Unity's front face: cross(b - a, c - a) points toward the viewer (as ArenaBuilder.Outward).
            if (front)
            {
                t.Add(i); t.Add(i + 1); t.Add(i + 2);
                t.Add(i); t.Add(i + 2); t.Add(i + 3);
            }
            else
            {
                t.Add(i); t.Add(i + 2); t.Add(i + 1);
                t.Add(i); t.Add(i + 3); t.Add(i + 2);
            }
        }

        /// <summary>NIGHT27 (builder V): a quad with a colour per corner (the AO gradient strips: black with alpha 1 - AO at the feature, 0 away from it).</summary>
        public void QuadGradient(Vector3 a, Vector3 b, Vector3 cc, Vector3 d, Vector3 normal, Color ca, Color cb, Color ccc, Color cd)
        {
            int i = v.Count;
            Quad(a, b, cc, d, normal, ca);
            c[i + 1] = cb; c[i + 2] = ccc; c[i + 3] = cd;
        }

        public void Triangle(Vector3 a, Vector3 b, Vector3 cc, Vector3 normal, Color col)
        {
            int i = v.Count;
            v.Add(a); v.Add(b); v.Add(cc);
            for (int k = 0; k < 3; k++)
            {
                n.Add(normal);
                c.Add(col);
            }
            Face(i, 3, normal, false);
            bool front = Vector3.Dot(Vector3.Cross(b - a, cc - a), normal) > 0;
            if (front) { t.Add(i); t.Add(i + 1); t.Add(i + 2); }
            else { t.Add(i); t.Add(i + 2); t.Add(i + 1); }
        }

        /// <summary>An axis-aligned box in web coordinates; tops get `top`, sides `side`, the bottom `bottom` (or side).</summary>
        public void Box(Vec3 min, Vec3 max, Color top, Color side, Color? bottom = null, bool skipBottom = false)
        {
            Vector3 a = WebSpace.ToUnity(min), b = WebSpace.ToUnity(max);
            Vector3 lo = Vector3.Min(a, b), hi = Vector3.Max(a, b);
            BoxUnity(lo, hi, top, side, bottom ?? side, skipBottom);
        }

        public void BoxUnity(Vector3 lo, Vector3 hi, Color top, Color side, Color bottom, bool skipBottom = false)
        {
            Vector3 P(int x, int y, int z) => new Vector3(x == 0 ? lo.x : hi.x, y == 0 ? lo.y : hi.y, z == 0 ? lo.z : hi.z);
            Quad(P(0, 1, 0), P(1, 1, 0), P(1, 1, 1), P(0, 1, 1), Vector3.up, top);
            if (!skipBottom) Quad(P(0, 0, 0), P(0, 0, 1), P(1, 0, 1), P(1, 0, 0), Vector3.down, bottom);
            Quad(P(0, 0, 0), P(1, 0, 0), P(1, 1, 0), P(0, 1, 0), Vector3.back, side);
            Quad(P(0, 0, 1), P(0, 1, 1), P(1, 1, 1), P(1, 0, 1), Vector3.forward, side);
            Quad(P(0, 0, 0), P(0, 1, 0), P(0, 1, 1), P(0, 0, 1), Vector3.left, side);
            Quad(P(1, 0, 0), P(1, 0, 1), P(1, 1, 1), P(1, 1, 0), Vector3.right, side);
        }

        /// <summary>A box by its Unity-space center and size (the blocky kit's parts).</summary>
        public void Cube(Vector3 center, Vector3 size, Color col) => BoxUnity(center - size * 0.5f, center + size * 0.5f, col, col, col);

        public Mesh Build(string name, Mesh reuse = null)
        {
            Mesh m = reuse != null ? reuse : new Mesh();
            m.Clear();
            m.name = name;
            if (v.Count > 65000) m.indexFormat = IndexFormat.UInt32;
            m.SetVertices(v);
            m.SetNormals(n);
            m.SetColors(c);
            m.SetUVs(3, f);
            m.SetTriangles(t, 0);
            m.RecalculateBounds();
            return m;
        }

        public static GameObject Object(string name, Transform parent, Mesh mesh, Material mat, bool shadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            r.receiveShadows = shadows;
            return go;
        }
    }
}
