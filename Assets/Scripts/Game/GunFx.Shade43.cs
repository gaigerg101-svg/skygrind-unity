// NIGHT43 builder F1 (design.n43.feel43; the lead's n43-r1 frames: on level 2's pale ice the additive flame and burst "wash out"): sprites that
// DARKEN. Additive sprites only add light, so on a bright floor they lose their edge; these multiply the frame (Blend DstColor Zero on a copy of
// the neon URP Unlit material: the blend is a material property, no shader variant needed) with a procedural soft disc that is white at its rim
// (no change) and the shade's tint at its centre, so a puff of smoke or an ink halo behind a bright burst gives value contrast on any background.
// One material and one dynamic quad mesh per darkness level (URP Unlit has no vertex colour: the darkness is the bucket, FxSprites' way).
// Preallocated; nothing allocated per frame.
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class Fx43Shade
    {
        public const int Levels = 6;
        readonly Material[] mats = new Material[Levels];
        readonly RenderParams[] rps = new RenderParams[Levels];
        readonly Vector3[][] verts = new Vector3[Levels][];
        readonly Mesh[] meshes = new Mesh[Levels];
        readonly int[] counts = new int[Levels];
        readonly int cap;
        Vector3 min, max;

        /// <summary>`tint`: the colour at full darkness (the centre of a level-max puff); `maxDark` 0..1 how far toward it the darkest level goes.</summary>
        public Fx43Shade(Material neon, string name, int capacity, Color tint, float maxDark)
        {
            cap = Math.Max(4, capacity);
            var uv = new Vector2[cap * 4];
            var tris = new int[cap * 6];
            for (int i = 0; i < cap; i++)
            {
                uv[i * 4] = new Vector2(0, 0); uv[i * 4 + 1] = new Vector2(1, 0); uv[i * 4 + 2] = new Vector2(1, 1); uv[i * 4 + 3] = new Vector2(0, 1);
                tris[i * 6] = i * 4; tris[i * 6 + 1] = i * 4 + 2; tris[i * 6 + 2] = i * 4 + 1;
                tris[i * 6 + 3] = i * 4; tris[i * 6 + 4] = i * 4 + 3; tris[i * 6 + 5] = i * 4 + 2;
            }
            for (int l = 0; l < Levels; l++)
            {
                float dark = maxDark * (l + 1) / Levels;
                var m = new Material(neon) { name = name + " " + l, renderQueue = (int)RenderQueue.Transparent - 1 }; // before the additive sprites: the bright adds over the dark
                m.SetFloat("_SrcBlend", (float)BlendMode.DstColor);
                m.SetFloat("_DstBlend", (float)BlendMode.Zero);
                m.SetFloat("_SrcBlendAlpha", (float)BlendMode.Zero);
                m.SetFloat("_DstBlendAlpha", (float)BlendMode.One);
                m.SetFloat("_ZWrite", 0);
                m.SetFloat("_Cull", 0);
                m.SetColor("_BaseColor", Color.white);
                m.SetTexture("_BaseMap", Disc(tint, dark));
                mats[l] = m;
                rps[l] = new RenderParams(m) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
                verts[l] = new Vector3[cap * 4];
                var mesh = new Mesh { name = name + " " + l };
                mesh.MarkDynamic();
                meshes[l] = mesh;
                tri0 = tris;
                uv0 = uv;
            }
        }

        readonly int[] tri0;
        readonly Vector2[] uv0;

        /// <summary>A soft disc: white at the rim (multiply: no change), the tint pulled `dark` of the way at the centre (smoothstep falloff).</summary>
        static Texture2D Disc(Color tint, float dark)
        {
            const int n = 32;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "Fx43 shade" };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = (x + 0.5f) / n * 2 - 1, dy = (y + 0.5f) / n * 2 - 1;
                    float r = Mathf.Sqrt(dx * dx + dy * dy), f = Mathf.Clamp01(1 - r);
                    f = f * f * (3 - 2 * f) * dark;
                    px[y * n + x] = (Color32)Color.Lerp(Color.white, tint, f);
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return tex;
        }

        public void Begin()
        {
            for (int l = 0; l < Levels; l++) counts[l] = 0;
            min = Vector3.positiveInfinity;
            max = Vector3.negativeInfinity;
        }

        /// <summary>A puff facing the camera: `size` m across, darkness k 0..1 (its level), turned `roll`.</summary>
        public void Billboard(Vector3 c, float size, float roll, Transform cam, float k)
        {
            if (!(k > 0.02f) || !(size > 0)) return;
            int l = Mathf.Clamp(Mathf.CeilToInt(k * Levels) - 1, 0, Levels - 1);
            if (counts[l] >= cap) return;
            float h = size * 0.5f, cs = Mathf.Cos(roll), sn = Mathf.Sin(roll);
            Vector3 r = (cam.right * cs + cam.up * sn) * h, u = (cam.up * cs - cam.right * sn) * h;
            int b = counts[l]++ * 4;
            Vector3[] v = verts[l];
            v[b] = c - r - u; v[b + 1] = c + r - u; v[b + 2] = c + r + u; v[b + 3] = c - r + u;
            min = Vector3.Min(min, c - Vector3.one * size);
            max = Vector3.Max(max, c + Vector3.one * size);
        }

        public void Draw()
        {
            for (int l = 0; l < Levels; l++)
            {
                if (counts[l] == 0) continue;
                Mesh m = meshes[l];
                m.Clear();
                m.SetVertices(verts[l], 0, counts[l] * 4);
                m.SetUVs(0, uv0, 0, counts[l] * 4);
                m.SetTriangles(tri0, 0, counts[l] * 6, 0, false);
                var bounds = new Bounds((min + max) * 0.5f, max - min);
                m.bounds = bounds;
                rps[l].worldBounds = bounds;
                Graphics.RenderMesh(rps[l], m, 0, Matrix4x4.identity);
            }
        }
    }
}
