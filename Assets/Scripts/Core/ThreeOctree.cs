// The web game's collision world, reproduced exactly: three.js r186 Octree (fromGraphNode -> addTriangle, build,
// split, getCapsuleTriangles, getRayTriangles, rayIntersect) with the helpers it calls (Box3.intersectsTriangle,
// Capsule.intersectsBox, Ray.intersectBox, the watertight Ray.intersectTriangle, Triangle.getNormal), and the
// triangle sources arena.js feeds it (BoxGeometry boxes, the wedge prisms) with their Float32 vertex buffers.
// Candidate order matters: mover.js resolves contacts one triangle after another, in the order the Octree returns
// them, so a different order can move the capsule by a few ulps. This port keeps three's order, including the
// reversal split() causes by popping triangles, and drops the same max-plane faces three's calcBox() loses.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class ThreeOctree : ICollisionWorld
    {
        public sealed class Triangle
        {
            public Vec3 a;
            public Vec3 b;
            public Vec3 c;
        }

        struct Box3
        {
            public Vec3 min;
            public Vec3 max;
        }

        sealed class Node
        {
            public Box3 box;
            public readonly List<Node> subTrees = new List<Node>();
            public readonly List<Triangle> triangles = new List<Triangle>();
        }

        const int TrianglesPerLeaf = 8;
        const int MaxLevel = 16;

        readonly Node root = new Node();
        Box3 bounds = new Box3
        {
            min = new Vec3(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity),
            max = new Vec3(double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity),
        };
        readonly List<Triangle> added = new List<Triangle>(); // every triangle fed in, in order
        readonly List<Triangle> candidates = new List<Triangle>();
        readonly HashSet<Triangle> seen = new HashSet<Triangle>();
        readonly List<Triangle> rayTriangles = new List<Triangle>(); // reused per ray (three allocates; Mono's GC is not V8's)
        readonly HashSet<Triangle> raySeen = new HashSet<Triangle>();

        public Vec3 BoundsMin => bounds.min;

        public Vec3 BoundsMax => bounds.max;

        // --- building ----------------------------------------------------------------------------------------------

        public void AddTriangle(Vec3 a, Vec3 b, Vec3 c)
        {
            var t = new Triangle { a = a, b = b, c = c };
            bounds.min.x = Min4(bounds.min.x, a.x, b.x, c.x);
            bounds.min.y = Min4(bounds.min.y, a.y, b.y, c.y);
            bounds.min.z = Min4(bounds.min.z, a.z, b.z, c.z);
            bounds.max.x = Max4(bounds.max.x, a.x, b.x, c.x);
            bounds.max.y = Max4(bounds.max.y, a.y, b.y, c.y);
            bounds.max.z = Max4(bounds.max.z, a.z, b.z, c.z);
            root.triangles.Add(t);
            added.Add(t);
        }

        /// <summary>Octree.build(): calcBox (pads only the min corner by 0.01) then split(0).</summary>
        public ThreeOctree Build()
        {
            root.box = bounds;
            root.box.min.x -= 0.01;
            root.box.min.y -= 0.01;
            root.box.min.z -= 0.01;
            Split(root, 0);
            return this;
        }

        static void Split(Node node, int level)
        {
            var subTrees = new List<Node>(8);
            Vec3 halfsize = (node.box.max - node.box.min) * 0.5;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        var sub = new Node();
                        sub.box.min = node.box.min + new Vec3(x * halfsize.x, y * halfsize.y, z * halfsize.z);
                        sub.box.max = sub.box.min + halfsize;
                        subTrees.Add(sub);
                    }
                }
            }
            // while (triangle = this.triangles.pop()): last first
            for (int k = node.triangles.Count - 1; k >= 0; k--)
            {
                Triangle t = node.triangles[k];
                for (int i = 0; i < subTrees.Count; i++)
                {
                    if (IntersectsTriangle(subTrees[i].box, t)) subTrees[i].triangles.Add(t);
                }
            }
            node.triangles.Clear();
            for (int i = 0; i < subTrees.Count; i++)
            {
                int len = subTrees[i].triangles.Count;
                if (len > TrianglesPerLeaf && level < MaxLevel) Split(subTrees[i], level + 1);
                if (len != 0) node.subTrees.Add(subTrees[i]);
            }
        }

        /// <summary>Every distinct triangle the built tree holds, in the order port-export's maps.mjs walks it.</summary>
        public List<Triangle> StoredTriangles()
        {
            var list = new List<Triangle>();
            var set = new HashSet<Triangle>();
            void Walk(Node n)
            {
                foreach (Triangle t in n.triangles)
                    if (set.Add(t)) list.Add(t);
                foreach (Node s in n.subTrees) Walk(s);
            }
            Walk(root);
            return list;
        }

        /// <summary>The triangles fed in that the tree does not hold (three's dropped max-plane faces).</summary>
        public List<Triangle> DroppedTriangles()
        {
            var kept = new HashSet<Triangle>(StoredTriangles());
            return added.FindAll(t => !kept.Contains(t));
        }

        // --- queries (ICollisionWorld) ------------------------------------------------------------------------------

        public int GatherCandidates(in Capsule capsule)
        {
            candidates.Clear();
            seen.Clear();
            GetCapsuleTriangles(root, capsule);
            return candidates.Count;
        }

        void GetCapsuleTriangles(Node node, in Capsule capsule)
        {
            for (int i = 0; i < node.subTrees.Count; i++)
            {
                Node sub = node.subTrees[i];
                if (!CapsuleIntersectsBox(capsule, sub.box)) continue;
                if (sub.triangles.Count > 0)
                {
                    for (int j = 0; j < sub.triangles.Count; j++)
                        if (seen.Add(sub.triangles[j])) candidates.Add(sub.triangles[j]);
                }
                else GetCapsuleTriangles(sub, capsule);
            }
        }

        public bool CapsuleIntersect(in Capsule capsule, int candidate, out CapsuleContact contact)
        {
            Triangle t = candidates[candidate];
            return TriangleWorld.TriangleCapsuleIntersect(capsule, t.a, t.b, t.c, out contact);
        }

        /// <summary>Octree.rayIntersect: the nearest front-facing hit; distance is |at(t) − origin| as three computes it.</summary>
        public bool RayIntersect(Vec3 origin, Vec3 direction, out RayHit hit)
        {
            hit = default;
            List<Triangle> tris = rayTriangles;
            tris.Clear();
            raySeen.Clear();
            GetRayTriangles(root, origin, direction, tris, raySeen);
            double distance = 1e100;
            Triangle best = null;
            for (int i = 0; i < tris.Count; i++)
            {
                Triangle t = tris[i];
                if (!IntersectTriangle(origin, direction, t.a, t.b, t.c, out double tt)) continue;
                Vec3 p = new Vec3(origin.x + direction.x * tt, origin.y + direction.y * tt, origin.z + direction.z * tt);
                double d = (p - origin).Length;
                if (distance > d)
                {
                    distance = d;
                    best = t;
                }
            }
            if (!(distance < 1e100)) return false;
            hit.distance = distance;
            hit.normal = TriangleWorld.TriangleNormal(best.a, best.b, best.c);
            return true;
        }

        // --- sphere queries (NIGHT14 U3-27.4: three r186 Octree.getSphereTriangles / triangleSphereIntersect /
        // sphereIntersect, for src/nav/navGraph.js's solidAt; new methods, nothing above changed) -----------------------

        readonly HashSet<Triangle> sphereSeen = new HashSet<Triangle>();
        readonly List<Triangle> sphereTriangles = new List<Triangle>();

        /// <summary>
        /// Octree.getSphereTriangles: every triangle of every leaf whose box the sphere touches (Sphere.intersectsBox), each
        /// once (`triangles.indexOf(t) === -1` against the whole output list), appended in three's order.
        /// </summary>
        public void GetSphereTriangles(Vec3 center, double radius, List<Triangle> output)
        {
            sphereSeen.Clear();
            for (int i = 0; i < output.Count; i++) sphereSeen.Add(output[i]);
            GetSphereTriangles(root, center, radius, output);
        }

        void GetSphereTriangles(Node node, Vec3 center, double radius, List<Triangle> output)
        {
            for (int i = 0; i < node.subTrees.Count; i++)
            {
                Node sub = node.subTrees[i];
                if (!SphereIntersectsBox(center, radius, sub.box)) continue;
                if (sub.triangles.Count > 0)
                {
                    for (int j = 0; j < sub.triangles.Count; j++)
                        if (sphereSeen.Add(sub.triangles[j])) output.Add(sub.triangles[j]);
                }
                else GetSphereTriangles(sub, center, radius, output);
            }
        }

        /// <summary>Sphere.intersectsBox = Box3.intersectsSphere: the box's point nearest the centre is inside the sphere.</summary>
        static bool SphereIntersectsBox(Vec3 c, double radius, Box3 box)
        {
            // box.clampPoint(center): Vector3.clamp(min, max), MathUtils.clamp(v, lo, hi) = Math.max(lo, Math.min(hi, v))
            double x = JsMath.Max(box.min.x, JsMath.Min(box.max.x, c.x));
            double y = JsMath.Max(box.min.y, JsMath.Min(box.max.y, c.y));
            double z = JsMath.Max(box.min.z, JsMath.Min(box.max.z, c.z));
            double dx = x - c.x, dy = y - c.y, dz = z - c.z;
            return dx * dx + dy * dy + dz * dz <= radius * radius;
        }

        /// <summary>
        /// Octree.triangleSphereIntersect: the triangle's plane within the radius, then the centre inside the triangle
        /// (Triangle.containsPoint of the centre itself, as three tests it) or an edge's closest point within the radius.
        /// </summary>
        public static bool TriangleSphereIntersect(Vec3 center, double radius, Triangle t, out Vec3 normal, out Vec3 point, out double depth)
        {
            normal = default;
            point = default;
            depth = 0;
            // triangle.getPlane: Plane.setFromCoplanarPoints(a, b, c)
            Vec3 n = TriangleWorld.TriangleNormal(t.a, t.b, t.c);
            double constant = -Vec3.Dot(t.a, n);
            double toCenter = Vec3.Dot(n, center) + constant; // plane.distanceToPoint(center)
            if (!(Math.Abs(toCenter) <= radius)) return false; // sphere.intersectsPlane
            double d0 = Math.Abs(toCenter - radius); // Math.abs(plane.distanceToSphere(sphere))
            double r2 = radius * radius - d0 * d0;
            // plane.projectPoint(center): center + normal * -distanceToPoint(center)
            double s = -toCenter;
            var plainPoint = new Vec3(center.x + n.x * s, center.y + n.y * s, center.z + n.z * s);
            if (TriangleWorld.ContainsPoint(center, t.a, t.b, t.c))
            {
                normal = n;
                point = plainPoint;
                depth = Math.Abs(toCenter - radius);
                return true;
            }
            if (EdgeSphere(t.a, t.b, plainPoint, center, radius, r2, ref normal, ref point, ref depth)) return true;
            if (EdgeSphere(t.b, t.c, plainPoint, center, radius, r2, ref normal, ref point, ref depth)) return true;
            if (EdgeSphere(t.c, t.a, plainPoint, center, radius, r2, ref normal, ref point, ref depth)) return true;
            return false;
        }

        /// <summary>One edge: Line3.closestPointToPoint(plainPoint, clamp) within sqrt(r2) of the centre.</summary>
        static bool EdgeSphere(Vec3 start, Vec3 end, Vec3 p, Vec3 center, double radius, double r2, ref Vec3 normal, ref Vec3 point, ref double depth)
        {
            // closestPointToPointParameter(point, true)
            Vec3 startP = p - start;
            Vec3 startEnd = end - start;
            double startEnd2 = Vec3.Dot(startEnd, startEnd);
            double t = 0;
            if (startEnd2 != 0)
            {
                double startEndStartP = Vec3.Dot(startEnd, startP);
                t = startEndStartP / startEnd2;
                t = JsMath.Max(0, JsMath.Min(1, t)); // MathUtils.clamp(t, 0, 1)
            }
            // this.delta(target).multiplyScalar(t).add(this.start)
            var v2 = new Vec3((end.x - start.x) * t + start.x, (end.y - start.y) * t + start.y, (end.z - start.z) * t + start.z);
            double dx = v2.x - center.x, dy = v2.y - center.y, dz = v2.z - center.z;
            double d = dx * dx + dy * dy + dz * dz; // v2.distanceToSquared(center)
            if (!(d < r2)) return false;
            normal = Vec3.Normalize(center - v2);
            point = v2;
            depth = radius - Math.Sqrt(d);
            return true;
        }

        /// <summary>
        /// Octree.sphereIntersect: gather the sphere's triangles, resolve each in order on a moving copy of the sphere, return
        /// the net push (unit normal, depth); false when nothing touches it.
        /// </summary>
        public bool SphereIntersect(Vec3 center, double radius, out Vec3 normal, out double depth)
        {
            sphereTriangles.Clear();
            GetSphereTriangles(center, radius, sphereTriangles);
            return ResolveSphere(sphereTriangles, center, radius, out normal, out depth);
        }

        /// <summary>The push-out loop three's Octree.sphereIntersect and world/collider.js sphereIntersect share.</summary>
        public static bool ResolveSphere(List<Triangle> triangles, Vec3 center, double radius, out Vec3 normal, out double depth)
        {
            Vec3 c = center;
            bool hit = false;
            for (int i = 0; i < triangles.Count; i++)
            {
                if (!TriangleSphereIntersect(c, radius, triangles[i], out Vec3 n, out _, out double d)) continue;
                hit = true;
                c = new Vec3(c.x + n.x * d, c.y + n.y * d, c.z + n.z * d); // center.add(normal.multiplyScalar(depth))
            }
            normal = default;
            depth = 0;
            if (!hit) return false;
            Vec3 v = c - center;
            depth = v.Length;
            normal = Vec3.Normalize(v);
            return true;
        }

        static void GetRayTriangles(Node node, Vec3 origin, Vec3 dir, List<Triangle> tris, HashSet<Triangle> set)
        {
            for (int i = 0; i < node.subTrees.Count; i++)
            {
                Node sub = node.subTrees[i];
                if (!RayIntersectsBox(origin, dir, sub.box)) continue;
                if (sub.triangles.Count > 0)
                {
                    for (int j = 0; j < sub.triangles.Count; j++)
                        if (set.Add(sub.triangles[j])) tris.Add(sub.triangles[j]);
                }
                else GetRayTriangles(sub, origin, dir, tris, set);
            }
        }

        // --- three.js helpers -------------------------------------------------------------------------------------

        static double Min4(double a, double b, double c, double d) => JsMath.Min(JsMath.Min(JsMath.Min(a, b), c), d);

        static double Max4(double a, double b, double c, double d) => JsMath.Max(JsMath.Max(JsMath.Max(a, b), c), d);

        /// <summary>Capsule.intersectsBox (checkAABBAxis on the xy, xz and yz planes).</summary>
        static bool CapsuleIntersectsBox(in Capsule c, Box3 box) =>
            CheckAabbAxis(c.start.x, c.start.y, c.end.x, c.end.y, box.min.x, box.max.x, box.min.y, box.max.y, c.radius) &&
            CheckAabbAxis(c.start.x, c.start.z, c.end.x, c.end.z, box.min.x, box.max.x, box.min.z, box.max.z, c.radius) &&
            CheckAabbAxis(c.start.y, c.start.z, c.end.y, c.end.z, box.min.y, box.max.y, box.min.z, box.max.z, c.radius);

        static bool CheckAabbAxis(double p1x, double p1y, double p2x, double p2y, double minx, double maxx, double miny, double maxy, double radius) =>
            (minx - p1x < radius || minx - p2x < radius) &&
            (p1x - maxx < radius || p2x - maxx < radius) &&
            (miny - p1y < radius || miny - p2y < radius) &&
            (p1y - maxy < radius || p2y - maxy < radius);

        /// <summary>Box3.intersectsTriangle (separating axis test).</summary>
        static bool IntersectsTriangle(Box3 box, Triangle t)
        {
            if (box.max.x < box.min.x || box.max.y < box.min.y || box.max.z < box.min.z) return false; // isEmpty
            Vec3 center = (box.min + box.max) * 0.5;
            Vec3 extents = box.max - center;
            Vec3 v0 = t.a - center, v1 = t.b - center, v2 = t.c - center;
            Vec3 f0 = v1 - v0, f1 = v2 - v1, f2 = v0 - v2;
            double[] axes =
            {
                0, -f0.z, f0.y, 0, -f1.z, f1.y, 0, -f2.z, f2.y,
                f0.z, 0, -f0.x, f1.z, 0, -f1.x, f2.z, 0, -f2.x,
                -f0.y, f0.x, 0, -f1.y, f1.x, 0, -f2.y, f2.x, 0,
            };
            if (!SatForAxes(axes, v0, v1, v2, extents)) return false;
            if (!SatForAxes(new double[] { 1, 0, 0, 0, 1, 0, 0, 0, 1 }, v0, v1, v2, extents)) return false;
            Vec3 n = new Vec3(f0.y * f1.z - f0.z * f1.y, f0.z * f1.x - f0.x * f1.z, f0.x * f1.y - f0.y * f1.x);
            return SatForAxes(new[] { n.x, n.y, n.z }, v0, v1, v2, extents);
        }

        static bool SatForAxes(double[] axes, Vec3 v0, Vec3 v1, Vec3 v2, Vec3 extents)
        {
            for (int i = 0, j = axes.Length - 3; i <= j; i += 3)
            {
                double ax = axes[i], ay = axes[i + 1], az = axes[i + 2];
                double r = extents.x * Math.Abs(ax) + extents.y * Math.Abs(ay) + extents.z * Math.Abs(az);
                double p0 = v0.x * ax + v0.y * ay + v0.z * az;
                double p1 = v1.x * ax + v1.y * ay + v1.z * az;
                double p2 = v2.x * ax + v2.y * ay + v2.z * az;
                if (JsMath.Max(-JsMath.Max(JsMath.Max(p0, p1), p2), JsMath.Min(JsMath.Min(p0, p1), p2)) > r) return false;
            }
            return true;
        }

        /// <summary>Ray.intersectsBox (intersectBox(...) !== null).</summary>
        static bool RayIntersectsBox(Vec3 origin, Vec3 dir, Box3 box)
        {
            double tmin, tmax, tymin, tymax, tzmin, tzmax;
            double invdirx = 1 / dir.x, invdiry = 1 / dir.y, invdirz = 1 / dir.z;
            if (invdirx >= 0)
            {
                tmin = (box.min.x - origin.x) * invdirx;
                tmax = (box.max.x - origin.x) * invdirx;
            }
            else
            {
                tmin = (box.max.x - origin.x) * invdirx;
                tmax = (box.min.x - origin.x) * invdirx;
            }
            if (invdiry >= 0)
            {
                tymin = (box.min.y - origin.y) * invdiry;
                tymax = (box.max.y - origin.y) * invdiry;
            }
            else
            {
                tymin = (box.max.y - origin.y) * invdiry;
                tymax = (box.min.y - origin.y) * invdiry;
            }
            if (tmin > tymax || tymin > tmax) return false;
            if (tymin > tmin || double.IsNaN(tmin)) tmin = tymin;
            if (tymax < tmax || double.IsNaN(tmax)) tmax = tymax;
            if (invdirz >= 0)
            {
                tzmin = (box.min.z - origin.z) * invdirz;
                tzmax = (box.max.z - origin.z) * invdirz;
            }
            else
            {
                tzmin = (box.max.z - origin.z) * invdirz;
                tzmax = (box.min.z - origin.z) * invdirz;
            }
            if (tmin > tzmax || tzmin > tmax) return false;
            if (tzmin > tmin || double.IsNaN(tmin)) tmin = tzmin;
            if (tzmax < tmax || double.IsNaN(tmax)) tmax = tzmax;
            return !(tmax < 0);
        }

        /// <summary>Ray.intersectTriangle (r186's watertight version), back faces culled. Returns the ray parameter t.</summary>
        static bool IntersectTriangle(Vec3 o, Vec3 d, Vec3 a, Vec3 b, Vec3 c, out double t)
        {
            t = 0;
            double dx = d.x, dy = d.y, dz = d.z;
            double aox = a.x - o.x, aoy = a.y - o.y, aoz = a.z - o.z;
            double box = b.x - o.x, boy = b.y - o.y, boz = b.z - o.z;
            double cox = c.x - o.x, coy = c.y - o.y, coz = c.z - o.z;
            double adx = Math.Abs(dx), ady = Math.Abs(dy), adz = Math.Abs(dz);
            double dkx, dky, dkz, akx, aky, akz, bkx, bky, bkz, ckx, cky, ckz;
            if (adx >= ady && adx >= adz)
            {
                dkz = dx; akz = aox; bkz = box; ckz = cox;
                if (dx >= 0) { dkx = dy; dky = dz; akx = aoy; aky = aoz; bkx = boy; bky = boz; ckx = coy; cky = coz; }
                else { dkx = dz; dky = dy; akx = aoz; aky = aoy; bkx = boz; bky = boy; ckx = coz; cky = coy; }
            }
            else if (ady >= adz)
            {
                dkz = dy; akz = aoy; bkz = boy; ckz = coy;
                if (dy >= 0) { dkx = dz; dky = dx; akx = aoz; aky = aox; bkx = boz; bky = box; ckx = coz; cky = cox; }
                else { dkx = dx; dky = dz; akx = aox; aky = aoz; bkx = box; bky = boz; ckx = cox; cky = coz; }
            }
            else
            {
                dkz = dz; akz = aoz; bkz = boz; ckz = coz;
                if (dz >= 0) { dkx = dx; dky = dy; akx = aox; aky = aoy; bkx = box; bky = boy; ckx = cox; cky = coy; }
                else { dkx = dy; dky = dx; akx = aoy; aky = aox; bkx = boy; bky = box; ckx = coy; cky = cox; }
            }
            if (dkz == 0) return false;
            double sx = dkx / dkz, sy = dky / dkz, sz = 1 / dkz;
            double ax = akx - sx * akz, ay = aky - sy * akz;
            double bx = bkx - sx * bkz, by = bky - sy * bkz;
            double cx = ckx - sx * ckz, cy = cky - sy * ckz;
            double u = cx * by - cy * bx;
            double v = ax * cy - ay * cx;
            double w = bx * ay - by * ax;
            if (u < 0 || v < 0 || w < 0) return false; // back-face culling
            double det = u + v + w;
            if (det == 0) return false;
            double tScaled = sz * (u * akz + v * bkz + w * ckz);
            if (det > 0 ? tScaled < 0 : tScaled > 0) return false;
            t = tScaled / det;
            return true;
        }
    }

    /// <summary>The triangles arena.js feeds the Octree, generated exactly as three.js does (Float32 vertex buffers).</summary>
    public static class ThreeGeometry
    {
        static double F32(double v) => (float)v;

        /// <summary>
        /// three Vector3.applyMatrix4 with makeTranslation(tx, ty, tz) (BufferGeometry.translate): every term of the 4x4
        /// product, w = 1 / (0x + 0y + 0z + 1), then the Float32 store.
        /// </summary>
        static Vec3 Translate(Vec3 q, double tx, double ty, double tz)
        {
            double w = 1 / (0 * q.x + 0 * q.y + 0 * q.z + 1);
            return new Vec3(
                F32((1 * q.x + 0 * q.y + 0 * q.z + tx) * w),
                F32((0 * q.x + 1 * q.y + 0 * q.z + ty) * w),
                F32((0 * q.x + 0 * q.y + 1 * q.z + tz) * w));
        }

        /// <summary>Octree.fromGraphNode's applyMatrix4(matrixWorld) with an identity matrix (meshes sit at the origin).</summary>
        static Vec3 Identity(Vec3 v)
        {
            double w = 1 / (0 * v.x + 0 * v.y + 0 * v.z + 1);
            return new Vec3((1 * v.x + 0 * v.y + 0 * v.z + 0) * w, (0 * v.x + 1 * v.y + 0 * v.z + 0) * w, (0 * v.x + 0 * v.y + 1 * v.z + 0) * w);
        }

        /// <summary>
        /// arena.js boxGeometry(b): BoxGeometry(w, h, d).translate(centre), then toNonIndexed() (two triangles per face,
        /// faces px, nx, py, ny, pz, nz). Calls add(a, b, c) per triangle.
        /// </summary>
        public static void Box(Vec3 min, Vec3 max, Action<Vec3, Vec3, Vec3> add)
        {
            double width = max.x - min.x, height = max.y - min.y, depth = max.z - min.z;
            double tx = min.x + width / 2, ty = min.y + height / 2, tz = min.z + depth / 2;
            var verts = new List<Vec3>(24);
            var index = new List<int>(36);
            // buildPlane(u, v, w, udir, vdir, width, height, depth): axes 0 = x, 1 = y, 2 = z
            void Plane(int u, int v, int w, double udir, double vdir, double pw, double ph, double pd)
            {
                int start = verts.Count;
                double widthHalf = pw / 2, heightHalf = ph / 2, depthHalf = pd / 2;
                for (int iy = 0; iy < 2; iy++)
                {
                    double y = iy * ph - heightHalf;
                    for (int ix = 0; ix < 2; ix++)
                    {
                        double x = ix * pw - widthHalf;
                        var p = new double[3];
                        p[u] = x * udir;
                        p[v] = y * vdir;
                        p[w] = depthHalf;
                        verts.Add(new Vec3(F32(p[0]), F32(p[1]), F32(p[2])));
                    }
                }
                int a = start + 0, b = start + 2, c = start + 3, d = start + 1;
                index.Add(a); index.Add(b); index.Add(d);
                index.Add(b); index.Add(c); index.Add(d);
            }
            Plane(2, 1, 0, -1, -1, depth, height, width); // px
            Plane(2, 1, 0, 1, -1, depth, height, -width); // nx
            Plane(0, 2, 1, 1, 1, width, depth, height); // py
            Plane(0, 2, 1, 1, -1, width, depth, -height); // ny
            Plane(0, 1, 2, 1, -1, width, height, depth); // pz
            Plane(0, 1, 2, -1, -1, width, height, -depth); // nz
            for (int i = 0; i < verts.Count; i++) verts[i] = Translate(verts[i], tx, ty, tz);
            for (int i = 0; i < index.Count; i += 3) add(Identity(verts[index[i]]), Identity(verts[index[i + 1]]), Identity(verts[index[i + 2]]));
        }

        /// <summary>arena.js wedgeGeometry(wd): the 20° prism (no bottom face), each face wound away from the centroid.</summary>
        public static void Wedge(WedgeSpec wd, Action<Vec3, Vec3, Vec3> add)
        {
            Vec3[] v = ArenaMap.WedgeCorners(wd);
            Vec3 lowL = v[0], lowR = v[1], highL = v[2], highR = v[3], topL = v[4], topR = v[5];
            Vec3[][] faces =
            {
                new[] { lowL, lowR, topR }, new[] { lowL, topR, topL }, // slope
                new[] { highL, topL, topR }, new[] { highL, topR, highR }, // back
                new[] { lowL, topL, highL }, new[] { lowR, highR, topR }, // sides
            };
            Vec3 center = Vec3.DivideScalar(Vec3.Zero + lowL + lowR + highL + highR + topL + topR, 6);
            foreach (Vec3[] f in faces)
            {
                Vec3 a = f[0], b = f[1], c = f[2];
                Vec3 ab = b - a, ac = c - a;
                Vec3 n = new Vec3(ab.y * ac.z - ab.z * ac.y, ab.z * ac.x - ab.x * ac.z, ab.x * ac.y - ab.y * ac.x);
                Vec3 mid = Vec3.DivideScalar(a + b + c, 3) - center;
                if (Vec3.Dot(n, mid) < 0) (b, c) = (c, b);
                add(Identity(Round(a)), Identity(Round(b)), Identity(Round(c)));
            }
        }

        static Vec3 Round(Vec3 v) => new Vec3(F32(v.x), F32(v.y), F32(v.z));

        /// <summary>The Octree arena.js builds: [floor, ...boxes], then barriers, then wedges, in that order.</summary>
        public static ThreeOctree BuildOctree(IEnumerable<BoxSpec> boxes, IEnumerable<WedgeSpec> wedges)
        {
            var octree = new ThreeOctree();
            foreach (BoxSpec b in boxes) Box(b.min, b.max, octree.AddTriangle);
            if (wedges != null)
                foreach (WedgeSpec w in wedges) Wedge(w, octree.AddTriangle);
            return octree.Build();
        }
    }
}
