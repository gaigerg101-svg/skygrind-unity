// Atmosphere motes, after skygrind/src/fx/motes.js (POLISH V4; NIGHT12 U3-25.2): config.motes.max motes (dust and data
// flecks) placed once in a box (motes.box) by Mulberry32(motes.seed) as the web places them (x, y, z, then the seed),
// drawn as one static mesh of camera-facing quads in one call (Graphics.RenderMesh; the vertex shader, Motes.shader,
// drifts them on motes.wind, wraps them around the camera and sizes them). The quality preset's share of max draws
// (setCount: the submesh's index range; 0 hides them). The clock runs on the view's time (LightAndLens: the world's in
// play, frame time on TITLE). Nothing is allocated per frame.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class Motes
    {
        readonly Mesh mesh;
        readonly Material material;
        readonly int max;
        readonly float boxLongest;
        int count;
        double time;
        RenderParams rp;
        static readonly int MoteTime = Shader.PropertyToID("_MoteTime");

        /// <summary>The mesh and material (null when the shader isn't in the build: then nothing draws).</summary>
        public Motes(GameConfig cfg, Shader shader)
        {
            max = cfg.Int("motes.max");
            double[] box = cfg.Nums("motes.box"), wind = cfg.Nums("motes.wind");
            boxLongest = (float)System.Math.Max(box[0], System.Math.Max(box[1], box[2]));
            if (shader == null) return;
            // motes.js: pos[i] = (rng() - 0.5) x box per axis, then seed[i] = rng(); web (x, y, z) -> Unity (x, y, -z).
            var rng = new Mulberry32(cfg.Num("motes.seed"));
            var verts = new List<Vector3>(max * 4);
            var uvs = new List<Vector4>(max * 4);
            var tris = new int[max * 6];
            for (int i = 0; i < max; i++)
            {
                double x = (rng.Next() - 0.5) * box[0], y = (rng.Next() - 0.5) * box[1], z = (rng.Next() - 0.5) * box[2];
                float seed = (float)rng.Next();
                Vector3 p = WebSpace.ToUnity(new Vec3(x, y, z));
                for (int k = 0; k < 4; k++)
                {
                    verts.Add(p);
                    uvs.Add(new Vector4(k == 0 || k == 3 ? -1 : 1, k < 2 ? -1 : 1, seed, 0));
                }
                int v = i * 4, t = i * 6;
                tris[t] = v;
                tris[t + 1] = v + 2;
                tris[t + 2] = v + 1;
                tris[t + 3] = v;
                tris[t + 4] = v + 3;
                tris[t + 5] = v + 2;
            }
            mesh = new Mesh { name = "Motes (fx/motes.js)", indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0, false);
            // The shader moves every vertex around the camera: the draw's bounds are the box around the camera (Render's
            // worldBounds); the mesh's own only need to be finite. The mesh stays readable (SetCount changes its submesh).
            mesh.bounds = new Bounds(Vector3.zero, new Vector3((float)box[0], (float)box[1], (float)box[2]));
            material = new Material(shader) { name = "Motes (fx/motes.js)" };
            material.SetVector("_Box", new Vector4((float)box[0], (float)box[1], (float)box[2], 0));
            Vector3 w = WebSpace.ToUnity(new Vec3(wind[0], wind[1], wind[2]));
            material.SetVector("_Wind", new Vector4(w.x, w.y, w.z, 0));
            material.SetColor("_Dust", Look.Hex((uint)cfg.Num("motes.dustColor")));
            material.SetColor("_Fleck", Look.Hex((uint)cfg.Num("motes.fleckColor")));
            material.SetFloat("_Size", (float)cfg.Num("motes.size"));
            material.SetFloat("_Opacity", (float)cfg.Num("motes.opacity"));
            material.SetFloat("_RefHeight", UnityTuning.MoteRefHeight);
            rp = new RenderParams(material)
            {
                layer = 0,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
            };
            count = max;
        }

        /// <summary>motes.js setCount: how many motes draw (the preset's share of max; 0 hides them).</summary>
        public void SetCount(double n)
        {
            int c = (int)System.Math.Max(0, System.Math.Min(max, JsMath.Round(n)));
            if (c == count || mesh == null) return;
            count = c;
            if (c > 0) mesh.SetSubMesh(0, new SubMeshDescriptor(0, c * 6), MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
        }

        /// <summary>motes.js update: the drift clock (the view's dt) and the draw around the camera.</summary>
        public void Render(double dt, Camera cam, bool visible)
        {
            if (mesh == null) return;
            time += dt;
            if (!visible || count == 0 || cam == null) return;
            material.SetFloat(MoteTime, (float)time);
            rp.worldBounds = new Bounds(cam.transform.position, Vector3.one * boxLongest);
            Graphics.RenderMesh(rp, mesh, 0, Matrix4x4.identity);
        }
    }
}
