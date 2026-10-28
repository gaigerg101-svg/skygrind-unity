// The core "Grinder", ported from skygrind/src/world/coreGrid.js: an n × n grid of square pillars that reshape
// (flat, stairs, ring, towers, random, ...) with a smoothstep tween on the fixed step; standing bodies ride with it and
// a body a pillar rises into is lifted onto its top. It is its own live collider answering the Octree calls (capsule
// triangles, rays), combined with the static world by LayeredWorld (world/collider.js order: static first, then core).
// presetHeights and the PRNG are checked against the export; the tween and the carry are untested against the web.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>The game's seeded PRNG (enemies/spawnMath.js mulberry32), exact to the bit.</summary>
    public sealed class Mulberry32
    {
        uint a;

        /// <summary>seed >>> 0 (JS ToUint32 of an integer seed).</summary>
        public Mulberry32(double seed) => a = unchecked((uint)(long)seed);

        /// <summary>
        /// NIGHT13 U3-26.10 (the run snapshot): the stream's whole state, the private uint `a` (seed + draws x 0x6D2B79F5,
        /// mod 2^32). Set it to continue a saved stream exactly. Read and written on save and load only.
        /// </summary>
        public uint State
        {
            get => a;
            set => a = value;
        }

        public double Next()
        {
            unchecked
            {
                a += 0x6D2B79F5;
                uint t = a;
                t = (t ^ (t >> 15)) * (t | 1);
                t ^= t + (t ^ (t >> 7)) * (t | 61);
                return (t ^ (t >> 14)) / 4294967296.0;
            }
        }
    }

    public struct CoreSpec
    {
        public double x0;
        public double z0;
        public int cells;
        public double cellSize;
    }

    public sealed class CoreGrid
    {
        public static readonly string[] Presets = { "flat", "stairs", "ring", "towers", "random" };

        readonly MovementConfig config;
        readonly CoreConfig core;
        public readonly CoreSpec spec;
        readonly int n;
        readonly int count;
        // Float32Array in the JS: every height is stored as a float32.
        readonly float[] heights;
        readonly float[] previous;
        readonly float[] from;
        readonly float[] to;
        int tweenSteps;
        int tweenTotal;
        readonly ThreeOctree.Triangle[][] tris;

        public CoreGrid(MovementConfig config, CoreSpec spec)
        {
            this.config = config;
            core = config.core;
            this.spec = spec;
            n = spec.cells;
            count = n * n;
            heights = new float[count];
            previous = new float[count];
            from = new float[count];
            to = new float[count];
            tris = new ThreeOctree.Triangle[count][];
            for (int i = 0; i < count; i++)
            {
                tris[i] = new ThreeOctree.Triangle[10];
                for (int j = 0; j < 10; j++) tris[i][j] = new ThreeOctree.Triangle();
                RebuildPillar(i);
            }
        }

        public bool Moving => tweenSteps > 0;

        public int Count => count;

        public double Height(int i) => heights[i];

        public double PreviousHeight(int i) => previous[i];

        /// <summary>Pure: pillar heights (m) for a preset, quantized to `step`. Row-major, row 0 at the min-z edge.</summary>
        public static double[] PresetHeights(string name, int n, double maxHeight, double step, Mulberry32 rng, bool[] keep = null)
        {
            double Q(double h) => JsMath.Min(maxHeight, JsMath.Max(0, JsMath.Round(h / step) * step));
            double c = (n - 1) / 2.0;
            var o = new double[n * n];
            for (int r = 0; r < n; r++)
            {
                for (int k = 0; k < n; k++)
                {
                    double ring = JsMath.Max(Math.Abs(k - c), Math.Abs(r - c)); // 0.5 at the middle .. c at the edge
                    double h = 0;
                    if (name == "stairs") h = ((k + r) / (2.0 * (n - 1))) * maxHeight;
                    else if (name == "ring") h = ring > c - 1 ? maxHeight / 2 : ring > c - 3 ? 0 : ring < 1.5 ? maxHeight : maxHeight / 4;
                    else if (name == "towers") h = k % 4 == 1 && r % 4 == 1 ? maxHeight : (k + r) % 5 == 0 ? maxHeight / 4 : 0;
                    else if (name == "random") h = rng.Next() < 0.45 ? 0 : rng.Next() * maxHeight;
                    else if (name == "platforms") h = k % 3 < 2 && r % 3 < 2 ? maxHeight / 2 : 0;
                    else if (name == "pen") h = (k == 0 || k == n - 1 || r == 0 || r == n - 1) && Math.Abs(k - c) > 1 && Math.Abs(r - c) > 1 ? maxHeight : 0;
                    else if (name == "pillars")
                    {
                        int pk = PillarIndex(k, n), pr = PillarIndex(r, n);
                        h = pk >= 0 && pr >= 0 && (keep == null || keep[pr * 2 + pk]) ? maxHeight : 0;
                    }
                    o[r * n + k] = Q(h);
                }
            }
            return o;
        }

        /// <summary>'pillars': 0 / 1 for the two 2-cell bands a quarter of the way in from each edge, else -1.</summary>
        public static int PillarIndex(int i, int n)
        {
            int a = (int)JsMath.Round(n / 4.0);
            if (i == a || i == a + 1) return 0;
            if (i == n - 2 - a || i == n - 1 - a) return 1;
            return -1;
        }

        static double Ease(double t) => t * t * (3 - 2 * t);

        void RebuildPillar(int i)
        {
            int k = i % n;
            int r = i / n;
            double ax = spec.x0 + k * spec.cellSize, az = spec.z0 + r * spec.cellSize;
            double bx = ax + spec.cellSize, bz = az + spec.cellSize;
            double h = heights[i];
            ThreeOctree.Triangle[] t = tris[i];
            void Set(int j, double x1, double y1, double z1, double x2, double y2, double z2, double x3, double y3, double z3)
            {
                t[j].a = new Vec3(x1, y1, z1);
                t[j].b = new Vec3(x2, y2, z2);
                t[j].c = new Vec3(x3, y3, z3);
            }
            Set(0, ax, h, az, ax, h, bz, bx, h, bz); Set(1, ax, h, az, bx, h, bz, bx, h, az); // top
            Set(2, ax, 0, az, ax, h, az, bx, h, az); Set(3, ax, 0, az, bx, h, az, bx, 0, az); // −z
            Set(4, bx, 0, bz, bx, h, bz, ax, h, bz); Set(5, bx, 0, bz, ax, h, bz, ax, 0, bz); // +z
            Set(6, ax, 0, bz, ax, h, bz, ax, h, az); Set(7, ax, 0, bz, ax, h, az, ax, 0, az); // −x
            Set(8, bx, 0, az, bx, h, az, bx, h, bz); Set(9, bx, 0, az, bx, h, bz, bx, 0, bz); // +x
        }

        int CellIndex(double x, double z)
        {
            int k = (int)Math.Floor((x - spec.x0) / spec.cellSize);
            int r = (int)Math.Floor((z - spec.z0) / spec.cellSize);
            return k >= 0 && k < n && r >= 0 && r < n ? r * n + k : -1;
        }

        /// <summary>topAt: the pillar top under (x, z), or null outside the core.</summary>
        public double? TopAt(double x, double z)
        {
            int i = CellIndex(x, z);
            return i < 0 ? (double?)null : heights[i];
        }

        /// <summary>getCapsuleTriangles: appends the 10 triangles of every colliding pillar near the capsule.</summary>
        public void GetCapsuleTriangles(in Capsule capsule, List<ThreeOctree.Triangle> output)
        {
            double r = capsule.radius;
            double minX = JsMath.Min(capsule.start.x, capsule.end.x) - r, maxX = JsMath.Max(capsule.start.x, capsule.end.x) + r;
            double minY = JsMath.Min(capsule.start.y, capsule.end.y) - r;
            double minZ = JsMath.Min(capsule.start.z, capsule.end.z) - r, maxZ = JsMath.Max(capsule.start.z, capsule.end.z) + r;
            int k0 = (int)JsMath.Max(0, Math.Floor((minX - spec.x0) / spec.cellSize));
            int k1 = (int)JsMath.Min(n - 1, Math.Floor((maxX - spec.x0) / spec.cellSize));
            int r0 = (int)JsMath.Max(0, Math.Floor((minZ - spec.z0) / spec.cellSize));
            int r1 = (int)JsMath.Min(n - 1, Math.Floor((maxZ - spec.z0) / spec.cellSize));
            for (int row = r0; row <= r1; row++)
            {
                for (int k = k0; k <= k1; k++)
                {
                    int i = row * n + k;
                    if (heights[i] > core.minHeight && minY < heights[i]) output.AddRange(tris[i]);
                }
            }
        }

        /// <summary>
        /// getSphereTriangles (NIGHT14 U3-27.4, a new method): appends the 10 triangles of every colliding pillar the
        /// sphere's bounds overlap (coreGrid.js eachNear(c.x - r, c.x + r, c.y - r, c.z - r, c.z + r)).
        /// </summary>
        public void GetSphereTriangles(Vec3 c, double r, List<ThreeOctree.Triangle> output)
        {
            double minX = c.x - r, maxX = c.x + r, minY = c.y - r, minZ = c.z - r, maxZ = c.z + r;
            int k0 = (int)JsMath.Max(0, Math.Floor((minX - spec.x0) / spec.cellSize));
            int k1 = (int)JsMath.Min(n - 1, Math.Floor((maxX - spec.x0) / spec.cellSize));
            int r0 = (int)JsMath.Max(0, Math.Floor((minZ - spec.z0) / spec.cellSize));
            int r1 = (int)JsMath.Min(n - 1, Math.Floor((maxZ - spec.z0) / spec.cellSize));
            for (int row = r0; row <= r1; row++)
            {
                for (int k = k0; k <= k1; k++)
                {
                    int i = row * n + k;
                    if (heights[i] > core.minHeight && minY < heights[i]) output.AddRange(tris[i]);
                }
            }
        }

        /// <summary>rayIntersect: the nearest pillar a ray hits, by its box, then the face the hit point lies on.</summary>
        public bool RayIntersect(Vec3 origin, Vec3 dir, out RayHit hit)
        {
            hit = default;
            bool found = false;
            for (int i = 0; i < count; i++)
            {
                if (heights[i] <= core.minHeight) continue;
                int k = i % n, r = i / n;
                var min = new Vec3(spec.x0 + k * spec.cellSize, 0, spec.z0 + r * spec.cellSize);
                var max = new Vec3(min.x + spec.cellSize, heights[i], min.z + spec.cellSize);
                if (!RayBox(origin, dir, min, max, out Vec3 p)) continue;
                double d = Vec3.DistanceTo(p, origin);
                if (found && d >= hit.distance) continue;
                const double eps = 1e-4;
                int face;
                if (Math.Abs(p.y - max.y) < eps) face = 0;
                else if (Math.Abs(p.z - min.z) < eps) face = 2;
                else if (Math.Abs(p.z - max.z) < eps) face = 4;
                else if (Math.Abs(p.x - min.x) < eps) face = 6;
                else if (Math.Abs(p.x - max.x) < eps) face = 8;
                else continue; // the ray starts inside this pillar
                ThreeOctree.Triangle t = tris[i][face];
                hit = new RayHit { distance = d, normal = TriangleWorld.TriangleNormal(t.a, t.b, t.c) };
                found = true;
            }
            return found;
        }

        /// <summary>three Ray.intersectBox, returning the hit point (at(tmin >= 0 ? tmin : tmax)).</summary>
        static bool RayBox(Vec3 o, Vec3 d, Vec3 bmin, Vec3 bmax, out Vec3 p)
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

        /// <summary>Set heights at once (map load).</summary>
        public void SetHeights(double[] values)
        {
            for (int i = 0; i < count; i++)
            {
                heights[i] = previous[i] = (float)values[i];
                RebuildPillar(i);
            }
            tweenSteps = 0;
        }

        /// <summary>Tween to new heights over `seconds` on the fixed step.</summary>
        public void Reshape(double[] values, double seconds, double dt)
        {
            for (int i = 0; i < count; i++)
            {
                from[i] = heights[i];
                to[i] = (float)values[i];
            }
            tweenTotal = tweenSteps = (int)JsMath.Max(1, JsMath.Round(seconds / dt));
        }

        /// <summary>One fixed step of the tween (before the mover steps). The body rides a moving pillar top or is lifted out.</summary>
        public void Step(Mover body)
        {
            for (int i = 0; i < count; i++) previous[i] = heights[i];
            if (tweenSteps <= 0) return;
            tweenSteps--;
            double t = Ease(1 - (double)tweenSteps / tweenTotal);
            for (int i = 0; i < count; i++)
            {
                double hd = (double)from[i] + ((double)to[i] - from[i]) * t; // the JS computes in doubles, stores float32
                float h = (float)hd;
                if (h == heights[i]) continue;
                heights[i] = h;
                RebuildPillar(i);
            }
            if (body == null) return;
            double carry = core.carryTolerance;
            int c = CellIndex(body.position.x, body.position.z);
            if (c < 0) return;
            double was = previous[c], now = heights[c], y = body.position.y;
            bool standing = Math.Abs(y - was) <= carry;
            bool inside = y < now && y > -carry;
            if (standing || inside)
            {
                body.position.y = now;
                if (body.velocity.y < 0) body.velocity.y = 0;
            }
        }
    }

    /// <summary>world/collider.js: the static map, then the live core grid, behind the same calls.</summary>
    public sealed class LayeredWorld : ICollisionWorld
    {
        readonly ICollisionWorld baseWorld;
        public readonly CoreGrid core;
        readonly List<ThreeOctree.Triangle> coreTris = new List<ThreeOctree.Triangle>();
        int baseCount;

        public LayeredWorld(ICollisionWorld baseWorld, CoreGrid core)
        {
            this.baseWorld = baseWorld;
            this.core = core;
        }

        public int GatherCandidates(in Capsule capsule)
        {
            baseCount = baseWorld.GatherCandidates(capsule);
            coreTris.Clear();
            core?.GetCapsuleTriangles(capsule, coreTris);
            return baseCount + coreTris.Count;
        }

        public bool CapsuleIntersect(in Capsule capsule, int candidate, out CapsuleContact contact)
        {
            if (candidate < baseCount) return baseWorld.CapsuleIntersect(capsule, candidate, out contact);
            ThreeOctree.Triangle t = coreTris[candidate - baseCount];
            return TriangleWorld.TriangleCapsuleIntersect(capsule, t.a, t.b, t.c, out contact);
        }

        public bool RayIntersect(Vec3 origin, Vec3 direction, out RayHit hit)
        {
            bool found = baseWorld.RayIntersect(origin, direction, out hit);
            if (core != null && core.RayIntersect(origin, direction, out RayHit h) && (!found || h.distance < hit.distance))
            {
                hit = h;
                found = true;
            }
            return found;
        }
    }
}
