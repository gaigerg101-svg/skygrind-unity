// NIGHT19 U3-33.8 (builder V): textured effect sprites (NIGHT19 banned pattern 13: "untextured quads as particles"). The
// textures are the Kenney Particle Pack's (1.1, CC0, kenney.nl/assets/particle-pack; the "PNG (Black background)" set:
// white shapes on black, vendored as they are in Assets/ThirdParty/KenneyParticles/Resources/KenneyParticles/ with the
// pack's License.txt beside them), loaded once with Resources.Load. White on black is what an additive blend wants: the
// black adds nothing, so the copies of the neon material (URP Unlit, Blend One One, no depth write: FxDraw.Additive, the
// path every Player build already draws) need no alpha and no new shader. URP Unlit has no vertex colour, so a sprite's
// colour and fade are its bucket: one additive material per colour x UnityTuning.FxSpriteLevels brightness steps (FxLines'
// way), every bucket one dynamic quad mesh drawn with one Graphics.RenderMesh. The colours are the palette's roles,
// passed in by the callers (UiTheme / config colours): the textures are only shapes. Preallocated, no allocation per frame.
// A missing texture (an Editor that has not imported it yet, the dotnet tests) leaves Ready false: the caller keeps its
// own path.
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class FxSprites
    {
        readonly Texture2D tex;
        readonly Material neon;
        readonly int layer, cap;
        const int Levels = UnityTuning.FxSpriteLevels;
        readonly Color32[] keys = new Color32[UnityTuning.FxColourBuckets];
        readonly RenderParams[] rps = new RenderParams[UnityTuning.FxColourBuckets * Levels];
        readonly QuadBatch[] batches = new QuadBatch[UnityTuning.FxColourBuckets * Levels];
        int colours;
        readonly string name;

        /// <summary>The texture loaded (false: nothing is drawn; the caller keeps its own look).</summary>
        public bool Ready => tex != null;

        /// <summary>A vendored sprite (UnityTuning.Kenney*), or null when it is not in the build yet.</summary>
        public static Texture2D Load(string path)
        {
            try
            {
                return Resources.Load<Texture2D>(path);
            }
            catch (Exception)
            {
                return null; // no engine (the dotnet tests) or no such asset
            }
        }

        public FxSprites(Material neon, string texturePath, int capacity, int layer = 0)
        {
            this.neon = neon;
            this.layer = layer;
            cap = Math.Max(1, capacity);
            name = "Fx sprites " + texturePath;
            tex = Load(texturePath);
        }

        /// <summary>PG3 V2: a sprite from a texture made at run time (rim74's clean radial glow: the Kenney flare has a lens streak through it). Nothing else changes: the batches, the colours and the draws are the same.</summary>
        public FxSprites(Material neon, Texture2D texture, string spriteName, int capacity, int layer = 0)
        {
            this.neon = neon;
            this.layer = layer;
            cap = Math.Max(1, capacity);
            name = "Fx sprites " + spriteName;
            tex = texture;
        }

        /// <summary>A colour's materials and batches made now (at load), not on its first sprite.</summary>
        public void Warm(Color c)
        {
            if (Ready) ColourOf(c);
        }

        int ColourOf(Color c)
        {
            Color32 k = new Color(Mathf.Clamp01(c.r / 4), Mathf.Clamp01(c.g / 4), Mathf.Clamp01(c.b / 4)); // HDR colours keyed / 4
            for (int i = 0; i < colours; i++)
            {
                if (keys[i].r == k.r && keys[i].g == k.g && keys[i].b == k.b) return i;
            }
            if (colours == keys.Length) return colours - 1;
            keys[colours] = k;
            for (int l = 0; l < Levels; l++)
            {
                Material m = FxDraw.Additive(neon, name);
                m.SetTexture("_BaseMap", tex);
                m.SetColor("_BaseColor", c * ((l + 1) / (float)Levels));
                m.SetFloat("_Cull", 0); // both faces
                rps[colours * Levels + l] = FxDraw.Params(m, layer);
                batches[colours * Levels + l] = new QuadBatch(cap, name);
            }
            return colours++;
        }

        /// <summary>A new frame's sprites.</summary>
        public void Begin()
        {
            for (int i = 0; i < colours * Levels; i++) batches[i].Begin();
        }

        QuadBatch BatchOf(Color c, float k)
        {
            if (!Ready || !(k > 0)) return null;
            int level = Math.Min(Levels, Math.Max(1, Mathf.CeilToInt(Mathf.Clamp01(k) * Levels)));
            return batches[ColourOf(c) * Levels + level - 1];
        }

        /// <summary>A quad at `center` spanning +-`halfRight` (u) and +-`halfUp` (v), `c` at brightness `k` (0..1).</summary>
        public void Quad(Vector3 center, Vector3 halfRight, Vector3 halfUp, Color c, float k) =>
            BatchOf(c, k)?.Add(center - halfRight - halfUp, center + halfRight - halfUp, center + halfRight + halfUp, center - halfRight + halfUp);

        /// <summary>A sprite facing `cam` (its plane is the camera's), `size` m across, turned `rollRad` in that plane.</summary>
        public void Billboard(Vector3 center, float size, float rollRad, Transform cam, Color c, float k)
        {
            float h = size * 0.5f, cs = Mathf.Cos(rollRad), sn = Mathf.Sin(rollRad);
            Vector3 r = cam.right, u = cam.up;
            Quad(center, (r * cs + u * sn) * h, (u * cs - r * sn) * h, c, k);
        }

        /// <summary>A streak from `a` (v 0) to `b` (v 1), `width` m across, turned to face `eye` (the trace sprites' long axis is v).</summary>
        public void Streak(Vector3 a, Vector3 b, float width, Vector3 eye, Color c, float k)
        {
            Vector3 d = b - a;
            if (d.sqrMagnitude < 1e-10f) return;
            Vector3 across = Vector3.Cross(d, eye - (a + b) * 0.5f);
            if (across.sqrMagnitude < 1e-12f) return;
            across = across.normalized * (width * 0.5f);
            Vector3 mid = (a + b) * 0.5f;
            Quad(mid, across, d * 0.5f, c, k);
        }

        /// <summary>A flat sprite in the plane through `center` with normal `normal`, `radius` m from the centre to its edge.</summary>
        public void Disc(Vector3 center, Vector3 normal, float radius, float rollRad, Color c, float k)
        {
            Vector3 n = normal.sqrMagnitude > 1e-10f ? normal.normalized : Vector3.up;
            Vector3 a = Vector3.Cross(n, Mathf.Abs(n.y) < 0.9f ? Vector3.up : Vector3.right).normalized, b = Vector3.Cross(n, a);
            float cs = Mathf.Cos(rollRad), sn = Mathf.Sin(rollRad);
            Quad(center, (a * cs + b * sn) * radius, (b * cs - a * sn) * radius, c, k);
        }

        /// <summary>This frame's sprites: one RenderMesh a bucket in use.</summary>
        public void Draw()
        {
            for (int i = 0; i < colours * Levels; i++) batches[i].Draw(rps[i]);
        }

        /// <summary>
        /// Textured quads as ONE dynamic mesh (CubeBatch's way: positions written on the CPU into preallocated arrays, the
        /// UVs and the triangles set up once), drawn with Graphics.RenderMesh. Both faces by the materials (_Cull Off), one winding.
        /// </summary>
        sealed class QuadBatch
        {
            readonly Vector3[] verts;
            readonly Vector2[] uvs;
            readonly int[] tris;
            readonly Mesh mesh;
            readonly int cap;
            int count;
            Vector3 min, max;

            public QuadBatch(int capacity, string name)
            {
                cap = capacity;
                verts = new Vector3[cap * 4];
                uvs = new Vector2[cap * 4];
                tris = new int[cap * 6];
                for (int i = 0; i < cap; i++)
                {
                    int v = i * 4, t = i * 6;
                    uvs[v] = new Vector2(0, 0);
                    uvs[v + 1] = new Vector2(1, 0);
                    uvs[v + 2] = new Vector2(1, 1);
                    uvs[v + 3] = new Vector2(0, 1);
                    // one winding: the materials draw both faces (_Cull Off); two windings would add the light twice
                    tris[t] = v; tris[t + 1] = v + 2; tris[t + 2] = v + 1;
                    tris[t + 3] = v; tris[t + 4] = v + 3; tris[t + 5] = v + 2;
                }
                mesh = new Mesh { name = name };
                mesh.MarkDynamic();
            }

            public void Begin()
            {
                count = 0;
                min = Vector3.positiveInfinity;
                max = Vector3.negativeInfinity;
            }

            public void Add(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                if (count >= cap) return;
                int v = count * 4;
                verts[v] = a;
                verts[v + 1] = b;
                verts[v + 2] = c;
                verts[v + 3] = d;
                min = Vector3.Min(Vector3.Min(min, a), Vector3.Min(Vector3.Min(b, c), d));
                max = Vector3.Max(Vector3.Max(max, a), Vector3.Max(Vector3.Max(b, c), d));
                count++;
            }

            public void Draw(RenderParams rp)
            {
                if (count == 0) return;
                mesh.Clear();
                mesh.SetVertices(verts, 0, count * 4);
                mesh.SetUVs(0, uvs, 0, count * 4);
                mesh.SetTriangles(tris, 0, count * 6, 0, false);
                var bounds = new Bounds((min + max) * 0.5f, max - min);
                mesh.bounds = bounds;
                rp.worldBounds = bounds;
                Graphics.RenderMesh(rp, mesh, 0, Matrix4x4.identity);
            }
        }
    }
}
