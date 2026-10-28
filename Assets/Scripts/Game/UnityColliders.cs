// Unity colliders for a collision world. The primary build mirrors the web game's collider: one non-convex
// MeshCollider per triangle the three.js Octree holds (ThreeOctree.StoredTriangles), so the faces three's Octree
// drops on the outer max-x / max-z planes are left out exactly as in the web game, and Physics.ComputePenetration
// answers per triangle like Octree.triangleCapsuleIntersect does. The box build (a BoxCollider per box, a convex
// MeshCollider per ramp) is kept for comparison: it brings the dropped faces back and merges a box's faces into one
// collider.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class UnityColliders
    {
        /// <summary>
        /// One MeshCollider per triangle. Web -> Unity flips z, which mirrors the triangle, so b and c swap to keep each
        /// face's front (ComputePenetration ignores back faces) pointing out of the solid.
        /// </summary>
        public static GameObject FromTriangles(IEnumerable<ThreeOctree.Triangle> triangles, string name)
        {
            var root = new GameObject(name);
            int i = 0;
            foreach (ThreeOctree.Triangle t in triangles)
            {
                var mesh = new Mesh { name = $"tri {i}" };
                mesh.SetVertices(new List<Vector3> { WebSpace.ToUnity(t.a), WebSpace.ToUnity(t.c), WebSpace.ToUnity(t.b) });
                mesh.SetTriangles(new[] { 0, 1, 2 }, 0);
                mesh.RecalculateBounds();
                var go = new GameObject($"tri {i++}");
                go.transform.SetParent(root.transform, false);
                var col = go.AddComponent<MeshCollider>();
                col.sharedMesh = mesh;
                col.convex = false;
            }
            Physics.SyncTransforms();
            return root;
        }

        /// <summary>A BoxCollider per box and a convex MeshCollider per wedge (the comparison build).</summary>
        public static GameObject FromBoxes(IEnumerable<BoxSpec> boxes, IEnumerable<WedgeSpec> wedges, string name)
        {
            var root = new GameObject(name);
            foreach (BoxSpec b in boxes)
            {
                Vector3 u0 = WebSpace.ToUnity(b.min), u1 = WebSpace.ToUnity(b.max);
                Vector3 min = Vector3.Min(u0, u1), max = Vector3.Max(u0, u1);
                var go = new GameObject("box");
                go.transform.SetParent(root.transform, false);
                go.transform.position = (min + max) * 0.5f;
                go.AddComponent<BoxCollider>().size = max - min;
            }
            if (wedges != null)
            {
                foreach (WedgeSpec w in wedges)
                {
                    var verts = new List<Vector3>();
                    foreach (Vec3 v in ArenaMap.WedgeCorners(w)) verts.Add(WebSpace.ToUnity(v));
                    var mesh = new Mesh { name = "wedge" };
                    mesh.SetVertices(verts);
                    mesh.SetTriangles(new[] { 0, 1, 2, 3, 4, 5, 0, 2, 4, 1, 3, 5 }, 0); // hull points; convex cooking rebuilds the faces
                    var go = new GameObject("wedge");
                    go.transform.SetParent(root.transform, false);
                    var col = go.AddComponent<MeshCollider>();
                    col.sharedMesh = mesh;
                    col.convex = true;
                }
            }
            Physics.SyncTransforms();
            return root;
        }
    }
}
