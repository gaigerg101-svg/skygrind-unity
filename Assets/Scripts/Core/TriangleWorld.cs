// The reference collision world: a triangle soup queried exactly the way the web game's three.js Octree does it.
// TriangleCapsuleIntersect is a line-for-line port of three.js r186 Octree.triangleCapsuleIntersect (with Plane,
// Triangle.containsPoint and lineToLineClosestPoints), so a mover running on this world resolves contacts like
// mover.js. Candidate gathering is brute force over triangle bounds (the Octree returns a superset from its leaf
// boxes; either way every candidate is then tested exactly). Rays use Möller–Trumbore with back-face culling
// (three.js uses a watertight variant; hit distances agree, only edge-on rounding can differ).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class TriangleWorld : ICollisionWorld
    {
        struct Tri
        {
            public Vec3 a;
            public Vec3 b;
            public Vec3 c;
            public Vec3 min;
            public Vec3 max;
        }

        readonly List<Tri> tris = new List<Tri>();
        readonly List<int> candidates = new List<int>();

        public int TriangleCount => tris.Count;

        public void AddTriangle(Vec3 a, Vec3 b, Vec3 c)
        {
            tris.Add(new Tri
            {
                a = a,
                b = b,
                c = c,
                min = new Vec3(JsMath.Min(a.x, JsMath.Min(b.x, c.x)), JsMath.Min(a.y, JsMath.Min(b.y, c.y)), JsMath.Min(a.z, JsMath.Min(b.z, c.z))),
                max = new Vec3(JsMath.Max(a.x, JsMath.Max(b.x, c.x)), JsMath.Max(a.y, JsMath.Max(b.y, c.y)), JsMath.Max(a.z, JsMath.Max(b.z, c.z))),
            });
        }

        /// <summary>A triangle wound so its normal points away from `inside` (a point inside the solid).</summary>
        public void AddOutward(Vec3 a, Vec3 b, Vec3 c, Vec3 inside)
        {
            Vec3 n = Cross(b - a, c - a);
            Vec3 mid = (a + b + c) / 3 - inside;
            if (Vec3.Dot(n, mid) < 0) AddTriangle(a, c, b);
            else AddTriangle(a, b, c);
        }

        /// <summary>An axis-aligned box as 12 outward triangles (three.js BoxGeometry, non-indexed).</summary>
        public void AddBox(Vec3 min, Vec3 max)
        {
            Vec3 center = (min + max) * 0.5;
            Vec3 P(int i) => new Vec3((i & 1) == 0 ? min.x : max.x, (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z);
            int[][] quads =
            {
                new[] { 0, 1, 3, 2 }, // z min
                new[] { 4, 5, 7, 6 }, // z max
                new[] { 0, 1, 5, 4 }, // y min
                new[] { 2, 3, 7, 6 }, // y max
                new[] { 0, 2, 6, 4 }, // x min
                new[] { 1, 3, 7, 5 }, // x max
            };
            foreach (int[] q in quads)
            {
                AddOutward(P(q[0]), P(q[1]), P(q[2]), center);
                AddOutward(P(q[0]), P(q[2]), P(q[3]), center);
            }
        }

        /// <summary>The web's 20° slide ramp (arena.js wedgeGeometry): a prism with no bottom face.</summary>
        public void AddWedge(WedgeSpec w)
        {
            Vec3[] v = ArenaMap.WedgeCorners(w);
            Vec3 lowL = v[0], lowR = v[1], highL = v[2], highR = v[3], topL = v[4], topR = v[5];
            Vec3 center = (lowL + lowR + highL + highR + topL + topR) / 6;
            AddOutward(lowL, lowR, topR, center); // slope
            AddOutward(lowL, topR, topL, center);
            AddOutward(highL, topL, topR, center); // back
            AddOutward(highL, topR, highR, center);
            AddOutward(lowL, topL, highL, center); // sides
            AddOutward(lowR, highR, topR, center);
        }

        public int GatherCandidates(in Capsule capsule)
        {
            candidates.Clear();
            double r = capsule.radius;
            double minX = JsMath.Min(capsule.start.x, capsule.end.x) - r, maxX = JsMath.Max(capsule.start.x, capsule.end.x) + r;
            double minY = JsMath.Min(capsule.start.y, capsule.end.y) - r, maxY = JsMath.Max(capsule.start.y, capsule.end.y) + r;
            double minZ = JsMath.Min(capsule.start.z, capsule.end.z) - r, maxZ = JsMath.Max(capsule.start.z, capsule.end.z) + r;
            for (int i = 0; i < tris.Count; i++)
            {
                Tri t = tris[i];
                if (t.max.x < minX || t.min.x > maxX || t.max.y < minY || t.min.y > maxY || t.max.z < minZ || t.min.z > maxZ) continue;
                candidates.Add(i);
            }
            return candidates.Count;
        }

        public bool CapsuleIntersect(in Capsule capsule, int candidate, out CapsuleContact contact)
        {
            Tri t = tris[candidates[candidate]];
            return TriangleCapsuleIntersect(capsule, t.a, t.b, t.c, out contact);
        }

        public bool RayIntersect(Vec3 origin, Vec3 direction, out RayHit hit)
        {
            hit = default;
            double best = 1e100;
            for (int i = 0; i < tris.Count; i++)
            {
                Tri t = tris[i];
                if (!RayTriangle(origin, direction, t.a, t.b, t.c, out double d) || d >= best) continue;
                best = d;
                hit.distance = d;
                hit.normal = TriangleNormal(t.a, t.b, t.c);
            }
            return best < 1e100;
        }

        // --- three.js ports -------------------------------------------------------------------------------------

        /// <summary>three.js Octree.triangleCapsuleIntersect (r186).</summary>
        public static bool TriangleCapsuleIntersect(in Capsule capsule, Vec3 a, Vec3 b, Vec3 c, out CapsuleContact contact)
        {
            contact = default;
            // triangle.getPlane: Plane.setFromCoplanarPoints(a, b, c)
            Vec3 normal = Normalize(Cross(c - b, a - b));
            double constant = -Vec3.Dot(a, normal);
            double r = capsule.radius;
            double d1 = Vec3.Dot(normal, capsule.start) + constant - r;
            double d2 = Vec3.Dot(normal, capsule.end) + constant - r;

            if ((d1 > 0 && d2 > 0) || (d1 < -r && d2 < -r)) return false;

            double delta = Math.Abs(d1 / (Math.Abs(d1) + Math.Abs(d2)));
            Vec3 intersectPoint = Lerp(capsule.start, capsule.end, delta);

            if (ContainsPoint(intersectPoint, a, b, c))
            {
                contact.normal = normal;
                contact.point = intersectPoint;
                contact.depth = Math.Abs(JsMath.Min(d1, d2));
                return true;
            }

            double r2 = r * r;
            if (EdgeContact(capsule, a, b, r, r2, ref contact)) return true;
            if (EdgeContact(capsule, b, c, r, r2, ref contact)) return true;
            if (EdgeContact(capsule, c, a, r, r2, ref contact)) return true;
            return false;
        }

        static bool EdgeContact(in Capsule capsule, Vec3 e0, Vec3 e1, double r, double r2, ref CapsuleContact contact)
        {
            LineToLineClosestPoints(capsule.start, capsule.end, e0, e1, out Vec3 p1, out Vec3 p2);
            Vec3 d = p1 - p2;
            double dist2 = d.x * d.x + d.y * d.y + d.z * d.z;
            if (!(dist2 < r2)) return false;
            contact.normal = Normalize(d);
            contact.point = p2;
            contact.depth = r - Math.Sqrt(dist2);
            return true;
        }

        /// <summary>three.js Octree lineToLineClosestPoints.</summary>
        public static void LineToLineClosestPoints(Vec3 s1, Vec3 e1, Vec3 s2, Vec3 e2, out Vec3 target1, out Vec3 target2)
        {
            const double EPS = 1e-10;
            Vec3 r = e1 - s1;
            Vec3 s = e2 - s2;
            Vec3 w = s2 - s1;
            double a = Vec3.Dot(r, s), b = Vec3.Dot(r, r), c = Vec3.Dot(s, s), d = Vec3.Dot(s, w), e = Vec3.Dot(r, w);
            double t1, t2;
            double divisor = b * c - a * a;
            if (Math.Abs(divisor) < EPS)
            {
                double d1 = -d / c;
                double d2 = (a - d) / c;
                if (Math.Abs(d1 - 0.5) < Math.Abs(d2 - 0.5))
                {
                    t1 = 0;
                    t2 = d1;
                }
                else
                {
                    t1 = 1;
                    t2 = d2;
                }
            }
            else
            {
                t1 = (d * a + e * c) / divisor;
                t2 = (t1 * a - d) / c;
            }
            t2 = JsMath.Max(0, JsMath.Min(1, t2));
            t1 = JsMath.Max(0, JsMath.Min(1, t1));
            target1 = new Vec3(r.x * t1 + s1.x, r.y * t1 + s1.y, r.z * t1 + s1.z);
            target2 = new Vec3(s.x * t2 + s2.x, s.y * t2 + s2.y, s.z * t2 + s2.z);
        }

        /// <summary>three.js Triangle.containsPoint (via getBarycoord): the point projected onto the plane.</summary>
        public static bool ContainsPoint(Vec3 p, Vec3 a, Vec3 b, Vec3 c)
        {
            Vec3 v0 = c - a;
            Vec3 v1 = b - a;
            Vec3 v2 = p - a;
            double dot00 = Vec3.Dot(v0, v0);
            double dot01 = Vec3.Dot(v0, v1);
            double dot02 = Vec3.Dot(v0, v2);
            double dot11 = Vec3.Dot(v1, v1);
            double dot12 = Vec3.Dot(v1, v2);
            double denom = dot00 * dot11 - dot01 * dot01;
            if (denom == 0) return false;
            double invDenom = 1 / denom;
            double u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            double v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            double x = 1 - u - v;
            double y = v;
            return x >= 0 && y >= 0 && x + y <= 1;
        }

        /// <summary>three.js Triangle.getNormal.</summary>
        public static Vec3 TriangleNormal(Vec3 a, Vec3 b, Vec3 c) => Normalize(Cross(c - b, a - b));

        internal static bool RayTriangle(Vec3 o, Vec3 dir, Vec3 a, Vec3 b, Vec3 c, out double t)
        {
            t = 0;
            Vec3 e1 = b - a;
            Vec3 e2 = c - a;
            Vec3 p = Cross(dir, e2);
            double det = Vec3.Dot(e1, p);
            if (det < 1e-12) return false; // back face or parallel (culled)
            double inv = 1 / det;
            Vec3 s = o - a;
            double u = Vec3.Dot(s, p) * inv;
            if (u < 0 || u > 1) return false;
            Vec3 q = Cross(s, e1);
            double v = Vec3.Dot(dir, q) * inv;
            if (v < 0 || u + v > 1) return false;
            t = Vec3.Dot(e2, q) * inv;
            return t >= 0;
        }

        static Vec3 Cross(Vec3 a, Vec3 b) => new Vec3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);

        // three.js Vector3.normalize: multiplyScalar(1 / (length || 1)).
        static Vec3 Normalize(Vec3 v) => Vec3.Normalize(v);

        // three.js Vector3.lerp: this += (v - this) * alpha.
        static Vec3 Lerp(Vec3 from, Vec3 to, double alpha) =>
            new Vec3(from.x + (to.x - from.x) * alpha, from.y + (to.y - from.y) * alpha, from.z + (to.z - from.z) * alpha);
    }
}
