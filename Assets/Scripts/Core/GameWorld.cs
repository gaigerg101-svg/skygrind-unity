// The world collider of a Survival run, after skygrind/src/world/collider.js + boxLayer.js: the static map Octree,
// then the live core grid, then named live box layers ('gates': the locked zones as solid force fields). Candidates
// are gathered in that order (mover.js resolves them in order). A layer added with rays: false blocks bodies but not
// shots (the web's boss layers). `Geometry` is the same world without the live layers (what the nav grid sees).
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>boxLayer.js: a live set of solid boxes, 12 outward triangles each, behind the Octree's calls.</summary>
    public sealed class BoxLayer
    {
        struct Entry
        {
            public Vec3 min;
            public Vec3 max;
            public ThreeOctree.Triangle[] tris;
            public bool soup; // NIGHT28: one triangle of a free triangle soup (Rink28Arena), not a box: rays test the triangle, not a box face
        }

        readonly List<Entry> boxes = new List<Entry>();

        public int Count => boxes.Count;

        /// <summary>Replace the solid set.</summary>
        public void SetBoxes(IEnumerable<(Vec3 min, Vec3 max)> list)
        {
            boxes.Clear();
            foreach (var (min, max) in list) boxes.Add(Build(min, max));
        }

        /// <summary>NIGHT28 (the rink): replace the solid set with a free triangle soup, one entry per triangle (a curved arena the boxes cannot make). Winding: the normal points out of the solid.</summary>
        public void SetTriangles(IEnumerable<(Vec3 a, Vec3 b, Vec3 c)> list)
        {
            boxes.Clear();
            foreach (var (a, b, c) in list)
            {
                var min = new Vec3(JsMath.Min(a.x, JsMath.Min(b.x, c.x)), JsMath.Min(a.y, JsMath.Min(b.y, c.y)), JsMath.Min(a.z, JsMath.Min(b.z, c.z)));
                var max = new Vec3(JsMath.Max(a.x, JsMath.Max(b.x, c.x)), JsMath.Max(a.y, JsMath.Max(b.y, c.y)), JsMath.Max(a.z, JsMath.Max(b.z, c.z)));
                boxes.Add(new Entry { min = min, max = max, soup = true, tris = new[] { new ThreeOctree.Triangle { a = a, b = b, c = c } } });
            }
        }

        static Entry Build(Vec3 lo, Vec3 hi)
        {
            double x0 = lo.x, y0 = lo.y, z0 = lo.z, x1 = hi.x, y1 = hi.y, z1 = hi.z;
            Vec3 V(double x, double y, double z) => new Vec3(x, y, z);
            ThreeOctree.Triangle T(Vec3 a, Vec3 c, Vec3 d) => new ThreeOctree.Triangle { a = a, b = c, c = d };
            // Counter-clockwise seen from outside (normals point out), in boxLayer.js's order.
            var tris = new[]
            {
                T(V(x0, y1, z0), V(x0, y1, z1), V(x1, y1, z1)), T(V(x0, y1, z0), V(x1, y1, z1), V(x1, y1, z0)), // +y
                T(V(x0, y0, z0), V(x1, y0, z1), V(x0, y0, z1)), T(V(x0, y0, z0), V(x1, y0, z0), V(x1, y0, z1)), // -y
                T(V(x0, y0, z0), V(x0, y1, z0), V(x1, y1, z0)), T(V(x0, y0, z0), V(x1, y1, z0), V(x1, y0, z0)), // -z
                T(V(x1, y0, z1), V(x1, y1, z1), V(x0, y1, z1)), T(V(x1, y0, z1), V(x0, y1, z1), V(x0, y0, z1)), // +z
                T(V(x0, y0, z1), V(x0, y1, z1), V(x0, y1, z0)), T(V(x0, y0, z1), V(x0, y1, z0), V(x0, y0, z0)), // -x
                T(V(x1, y0, z0), V(x1, y1, z0), V(x1, y1, z1)), T(V(x1, y0, z0), V(x1, y1, z1), V(x1, y0, z1)), // +x
            };
            return new Entry { min = lo, max = hi, tris = tris };
        }

        public void GetCapsuleTriangles(in Capsule capsule, List<ThreeOctree.Triangle> output)
        {
            double r = capsule.radius;
            double minX = JsMath.Min(capsule.start.x, capsule.end.x) - r, maxX = JsMath.Max(capsule.start.x, capsule.end.x) + r;
            double minY = JsMath.Min(capsule.start.y, capsule.end.y) - r, maxY = JsMath.Max(capsule.start.y, capsule.end.y) + r;
            double minZ = JsMath.Min(capsule.start.z, capsule.end.z) - r, maxZ = JsMath.Max(capsule.start.z, capsule.end.z) + r;
            foreach (Entry e in boxes)
            {
                // Box3.intersectsBox (touching counts)
                if (maxX < e.min.x || minX > e.max.x || maxY < e.min.y || minY > e.max.y || maxZ < e.min.z || minZ > e.max.z) continue;
                output.AddRange(e.tris);
            }
        }

        /// <summary>
        /// getSphereTriangles (NIGHT14 U3-27.4, a new method): the 12 triangles of every box the sphere's bounds touch
        /// (query = [c - r, c + r], Box3.intersectsBox, touching counts).
        /// </summary>
        public void GetSphereTriangles(Vec3 c, double r, List<ThreeOctree.Triangle> output)
        {
            double minX = c.x - r, minY = c.y - r, minZ = c.z - r, maxX = c.x + r, maxY = c.y + r, maxZ = c.z + r;
            foreach (Entry e in boxes)
            {
                if (maxX < e.min.x || minX > e.max.x || maxY < e.min.y || minY > e.max.y || maxZ < e.min.z || minZ > e.max.z) continue;
                output.AddRange(e.tris);
            }
        }

        /// <summary>The nearest box face a ray hits (Ray.intersectBox, then the face the hit point lies on).</summary>
        public bool RayIntersect(Vec3 origin, Vec3 dir, out RayHit hit)
        {
            hit = default;
            bool found = false;
            foreach (Entry e in boxes)
            {
                if (e.soup)
                {
                    ThreeOctree.Triangle st = e.tris[0];
                    if (!TriangleWorld.RayTriangle(origin, dir, st.a, st.b, st.c, out double sd) || (found && sd >= hit.distance)) continue;
                    hit = new RayHit { distance = sd, normal = TriangleWorld.TriangleNormal(st.a, st.b, st.c) };
                    found = true;
                    continue;
                }
                if (!RayBox(origin, dir, e.min, e.max, out Vec3 p)) continue;
                double d = Vec3.DistanceTo(p, origin);
                if (found && d >= hit.distance) continue;
                const double eps = 1e-4;
                int face;
                if (System.Math.Abs(p.y - e.max.y) < eps) face = 0;
                else if (System.Math.Abs(p.y - e.min.y) < eps) face = 2;
                else if (System.Math.Abs(p.z - e.min.z) < eps) face = 4;
                else if (System.Math.Abs(p.z - e.max.z) < eps) face = 6;
                else if (System.Math.Abs(p.x - e.min.x) < eps) face = 8;
                else if (System.Math.Abs(p.x - e.max.x) < eps) face = 10;
                else continue; // the ray starts inside this box
                ThreeOctree.Triangle t = e.tris[face];
                hit = new RayHit { distance = d, normal = TriangleWorld.TriangleNormal(t.a, t.b, t.c) };
                found = true;
            }
            return found;
        }

        public bool ContainsPoint(Vec3 p)
        {
            foreach (Entry e in boxes)
            {
                if (e.soup) continue;
                if (p.x >= e.min.x && p.x <= e.max.x && p.y >= e.min.y && p.y <= e.max.y && p.z >= e.min.z && p.z <= e.max.z) return true;
            }
            return false;
        }

        /// <summary>three Ray.intersectBox, returning the hit point (at(tmin >= 0 ? tmin : tmax)).</summary>
        public static bool RayBox(Vec3 o, Vec3 d, Vec3 bmin, Vec3 bmax, out Vec3 p)
        {
            p = default;
            double tmin, tmax, tymin, tymax, tzmin, tzmax;
            double ix = 1 / d.x, iy = 1 / d.y, iz = 1 / d.z;
            if (ix >= 0) { tmin = (bmin.x - o.x) * ix; tmax = (bmax.x - o.x) * ix; } else { tmin = (bmax.x - o.x) * ix; tmax = (bmin.x - o.x) * ix; }
            if (iy >= 0) { tymin = (bmin.y - o.y) * iy; tymax = (bmax.y - o.y) * iy; } else { tymin = (bmax.y - o.y) * iy; tymax = (bmin.y - o.y) * iy; }
            if (tmin > tymax || tymin > tmax) return false;
            if (tymin > tmin || double.IsNaN(tmin)) tmin = tymin;
            if (tymax < tmax || double.IsNaN(tmax)) tmax = tymax;
            if (iz >= 0) { tzmin = (bmin.z - o.z) * iz; tzmax = (bmax.z - o.z) * iz; } else { tzmin = (bmax.z - o.z) * iz; tzmax = (bmin.z - o.z) * iz; }
            if (tmin > tzmax || tzmin > tmax) return false;
            if (tzmin > tmin || double.IsNaN(tmin)) tmin = tzmin;
            if (tzmax < tmax || double.IsNaN(tmax)) tmax = tzmax;
            if (tmax < 0) return false;
            double t = tmin >= 0 ? tmin : tmax;
            p = new Vec3(o.x + d.x * t, o.y + d.y * t, o.z + d.z * t);
            return true;
        }
    }

    public sealed class GameWorld : ICollisionWorld
    {
        public readonly ThreeOctree octree;
        public readonly CoreGrid core;
        readonly List<(string name, BoxLayer layer, bool rays)> layers = new List<(string, BoxLayer, bool)>();
        readonly List<ThreeOctree.Triangle> extraTris = new List<ThreeOctree.Triangle>();
        int baseCount;

        /// <summary>The static map + the core only (no live layers): the nav grid's view.</summary>
        public readonly LayeredWorld Geometry;

        public GameWorld(ThreeOctree octree, CoreGrid core)
        {
            this.octree = octree;
            this.core = core;
            Geometry = new LayeredWorld(octree, core);
        }

        /// <summary>Add, replace or (null) remove a live layer; rays: false keeps it out of RayIntersect.</summary>
        public void SetLayer(string name, BoxLayer layer, bool rays = true)
        {
            layers.RemoveAll(l => l.name == name);
            if (layer != null) layers.Add((name, layer, rays));
        }

        public int GatherCandidates(in Capsule capsule)
        {
            baseCount = octree.GatherCandidates(capsule);
            extraTris.Clear();
            core?.GetCapsuleTriangles(capsule, extraTris);
            foreach (var l in layers) l.layer.GetCapsuleTriangles(capsule, extraTris);
            return baseCount + extraTris.Count;
        }

        public bool CapsuleIntersect(in Capsule capsule, int candidate, out CapsuleContact contact)
        {
            if (candidate < baseCount) return octree.CapsuleIntersect(capsule, candidate, out contact);
            ThreeOctree.Triangle t = extraTris[candidate - baseCount];
            return TriangleWorld.TriangleCapsuleIntersect(capsule, t.a, t.b, t.c, out contact);
        }

        readonly List<ThreeOctree.Triangle> sphereTris = new List<ThreeOctree.Triangle>();

        /// <summary>
        /// world/collider.js sphereIntersect (NIGHT14 U3-27.4, a new method; src/nav/navGraph.js's solidAt): the static
        /// Octree's, the core's and every live layer's sphere triangles (rays: false layers too), resolved in that order on
        /// a moving copy of the sphere (three's algorithm); the net push as a unit normal and a depth, false on no contact.
        /// </summary>
        public bool SphereIntersect(Vec3 center, double radius, out Vec3 normal, out double depth)
        {
            sphereTris.Clear();
            octree.GetSphereTriangles(center, radius, sphereTris);
            core?.GetSphereTriangles(center, radius, sphereTris);
            foreach (var l in layers) l.layer.GetSphereTriangles(center, radius, sphereTris);
            return ThreeOctree.ResolveSphere(sphereTris, center, radius, out normal, out depth);
        }

        public bool RayIntersect(Vec3 origin, Vec3 direction, out RayHit hit)
        {
            bool found = octree.RayIntersect(origin, direction, out hit);
            if (core != null && core.RayIntersect(origin, direction, out RayHit h) && (!found || h.distance < hit.distance))
            {
                hit = h;
                found = true;
            }
            foreach (var l in layers)
            {
                if (!l.rays) continue;
                if (l.layer.RayIntersect(origin, direction, out RayHit c) && (!found || c.distance < hit.distance))
                {
                    hit = c;
                    found = true;
                }
            }
            return found;
        }
    }
}
