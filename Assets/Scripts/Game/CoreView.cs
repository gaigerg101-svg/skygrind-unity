// The Grinder in Unity: one BoxCollider per pillar, resized from CoreGrid's heights after each core step (Sync), and
// one visual box per pillar drawn at heights interpolated between the last two steps (Render). A pillar at or below
// config.core.minHeight has no collision, as in the web game (coreGrid.js eachNear).
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class CoreView
    {
        readonly CoreGrid grid;
        readonly double minHeight;
        readonly BoxCollider[] colliders;
        readonly Transform[] visuals;
        readonly float[] synced;

        public GameObject Root { get; }

        public CoreView(CoreGrid grid, MovementConfig config, Transform parent, Material material)
        {
            this.grid = grid;
            minHeight = config.core.minHeight;
            var root = new GameObject("Grinder core");
            Root = root;
            root.transform.SetParent(parent, false);
            colliders = new BoxCollider[grid.Count];
            visuals = new Transform[grid.Count];
            synced = new float[grid.Count];
            for (int i = 0; i < grid.Count; i++)
            {
                var go = new GameObject($"pillar {i}");
                go.transform.SetParent(root.transform, false);
                colliders[i] = go.AddComponent<BoxCollider>();
                synced[i] = -1;
                if (material == null) continue;
                var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
                vis.name = $"pillar {i} (render)";
                Object.DestroyImmediate(vis.GetComponent<Collider>());
                vis.GetComponent<Renderer>().sharedMaterial = material;
                vis.transform.SetParent(root.transform, false);
                visuals[i] = vis.transform;
            }
            Sync();
            Render(1);
        }

        Vector3 Footprint(int i, out float size)
        {
            CoreSpec s = grid.spec;
            int k = i % s.cells, r = i / s.cells;
            size = (float)s.cellSize;
            return WebSpace.ToUnity(new Vec3(s.x0 + (k + 0.5) * s.cellSize, 0, s.z0 + (r + 0.5) * s.cellSize));
        }

        /// <summary>After CoreGrid.Step: colliders follow the new heights.</summary>
        public void Sync()
        {
            bool changed = false;
            for (int i = 0; i < grid.Count; i++)
            {
                float h = (float)grid.Height(i);
                if (h == synced[i]) continue;
                synced[i] = h;
                changed = true;
                BoxCollider c = colliders[i];
                c.enabled = h > minHeight;
                Vector3 p = Footprint(i, out float size);
                c.transform.position = new Vector3(p.x, h * 0.5f, p.z);
                c.size = new Vector3(size, Mathf.Max(h, 0.001f), size);
            }
            if (changed) Physics.SyncTransforms();
        }

        /// <summary>Frame time: pillar visuals between the previous and the current step.</summary>
        public void Render(double alpha)
        {
            for (int i = 0; i < grid.Count; i++)
            {
                if (visuals[i] == null) continue;
                double h = grid.PreviousHeight(i) + (grid.Height(i) - grid.PreviousHeight(i)) * alpha;
                Vector3 p = Footprint(i, out float size);
                float hh = Mathf.Max((float)h, 0.02f);
                visuals[i].position = new Vector3(p.x, hh * 0.5f, p.z);
                visuals[i].localScale = new Vector3(size * 0.98f, hh, size * 0.98f);
            }
        }
    }
}
