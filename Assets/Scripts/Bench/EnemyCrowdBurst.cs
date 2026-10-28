// The enemy crowd on Jobs + Burst: the same Husk chase and moveHusk as EnemyCrowd, in doubles, one parallel job per
// fixed step over a snapshot of last step's positions (the managed loop updates in place, like the web). Separation
// reads a counting-sorted grid built by a Burst job. "Full" adds the closer-to-real parts: line of sight every 0.25 s
// per enemy through RaycastCommand batches (PhysX, against the per-triangle colliders), the grid flow field, and the
// aggregated capsuleIntersect push-out against the web collider's triangles, bucketed in a static 4 m grid (three's
// per-triangle test; the resolve order follows the grid, not the Octree, so it is not bit identical to the web).
using System;
using Skygrind.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Skygrind.Bench
{
    public sealed class EnemyCrowdBurst : IDisposable
    {
        public struct Settings
        {
            public double speed, stopDistance, radius, height, sepRadius, sepSpeed, gravity;
            public double floorMinNormalY, edgeNormalY, edgeRiseSpeed, supportedDrop;
            public double gridOrigin, sepCell;
            public int gridW;
            public bool collide, useFlow;
            public double flowOriginX, flowOriginZ, flowCell;
            public int flowW, flowH;
            public double triOrigin, triCell;
            public int triW;
        }

        Settings s;
        NativeArray<double3> posA, posB, velA, velB;
        NativeArray<byte> groundedA, groundedB, edgeA, edgeB, los;
        NativeArray<int> cellStart, cellCount, sorted;
        NativeArray<float2> flowDir;
        NativeArray<double3> triA, triB, triC;
        NativeArray<int> triCellStart, triItems;
        NativeArray<RaycastCommand> rayCommands;
        NativeArray<RaycastHit> rayHits;
        NativeArray<int> rayEnemy;
        readonly bool useLos;
        readonly int losSteps;
        readonly FlowField flow;
        int flowVersion = -1;
        public int Count { get; private set; }
        public int raysThisStep;

        public EnemyCrowdBurst(MovementConfig config, bool full, FlowField flow, ThreeOctree octree, double floorSize)
        {
            HuskConfig h = config.husk;
            useLos = full;
            this.flow = full ? flow : null;
            losSteps = MovementMath.StepsFor(EnemyCrowd.LosInterval, 1 / config.loop.simHz);
            s = new Settings
            {
                speed = h.speed, stopDistance = h.stopDistance, radius = h.radius, height = h.height,
                sepRadius = h.separationRadius, sepSpeed = h.separationSpeed,
                gravity = MovementMath.GravityFor(config.jump.height, config.jump.apexTime),
                floorMinNormalY = config.surface.floorMinNormalY, edgeNormalY = config.ai.edgeNormalY,
                edgeRiseSpeed = config.ai.edgeRiseSpeed, supportedDrop = config.ai.supportedDrop,
                gridOrigin = -floorSize / 2 - 4, sepCell = h.separationRadius,
                gridW = (int)Math.Ceiling((floorSize + 8) / h.separationRadius),
                collide = full, useFlow = full,
            };
            int cells = s.gridW * s.gridW;
            cellStart = new NativeArray<int>(cells + 1, Allocator.Persistent);
            cellCount = new NativeArray<int>(cells, Allocator.Persistent);
            if (full)
            {
                s.flowOriginX = flow.originX;
                s.flowOriginZ = flow.originZ;
                s.flowCell = flow.cell;
                s.flowW = flow.w;
                s.flowH = flow.h;
                flowDir = new NativeArray<float2>(flow.w * flow.h, Allocator.Persistent);
                BuildTriangleGrid(octree, floorSize);
            }
            else
            {
                flowDir = new NativeArray<float2>(1, Allocator.Persistent);
                triA = new NativeArray<double3>(1, Allocator.Persistent);
                triB = new NativeArray<double3>(1, Allocator.Persistent);
                triC = new NativeArray<double3>(1, Allocator.Persistent);
                triCellStart = new NativeArray<int>(2, Allocator.Persistent);
                triItems = new NativeArray<int>(1, Allocator.Persistent);
            }
        }

        void BuildTriangleGrid(ThreeOctree octree, double floorSize)
        {
            var tris = octree.StoredTriangles();
            triA = new NativeArray<double3>(tris.Count, Allocator.Persistent);
            triB = new NativeArray<double3>(tris.Count, Allocator.Persistent);
            triC = new NativeArray<double3>(tris.Count, Allocator.Persistent);
            s.triCell = 4;
            s.triOrigin = -floorSize / 2 - 4;
            s.triW = (int)Math.Ceiling((floorSize + 8) / s.triCell);
            var lists = new System.Collections.Generic.List<int>[s.triW * s.triW];
            for (int c = 0; c < lists.Length; c++) lists[c] = new System.Collections.Generic.List<int>();
            for (int t = 0; t < tris.Count; t++)
            {
                ThreeOctree.Triangle tr = tris[t];
                triA[t] = new double3(tr.a.x, tr.a.y, tr.a.z);
                triB[t] = new double3(tr.b.x, tr.b.y, tr.b.z);
                triC[t] = new double3(tr.c.x, tr.c.y, tr.c.z);
                double minX = Math.Min(tr.a.x, Math.Min(tr.b.x, tr.c.x)), maxX = Math.Max(tr.a.x, Math.Max(tr.b.x, tr.c.x));
                double minZ = Math.Min(tr.a.z, Math.Min(tr.b.z, tr.c.z)), maxZ = Math.Max(tr.a.z, Math.Max(tr.b.z, tr.c.z));
                int k0 = Clamp((int)Math.Floor((minX - s.triOrigin) / s.triCell)), k1 = Clamp((int)Math.Floor((maxX - s.triOrigin) / s.triCell));
                int r0 = Clamp((int)Math.Floor((minZ - s.triOrigin) / s.triCell)), r1 = Clamp((int)Math.Floor((maxZ - s.triOrigin) / s.triCell));
                for (int r = r0; r <= r1; r++)
                    for (int k = k0; k <= k1; k++) lists[r * s.triW + k].Add(t);
            }
            triCellStart = new NativeArray<int>(lists.Length + 1, Allocator.Persistent);
            int total = 0;
            for (int c = 0; c < lists.Length; c++)
            {
                triCellStart[c] = total;
                total += lists[c].Count;
            }
            triCellStart[lists.Length] = total;
            triItems = new NativeArray<int>(total, Allocator.Persistent);
            int o = 0;
            foreach (var l in lists)
                foreach (int t in l) triItems[o++] = t;
        }

        int Clamp(int c) => Math.Max(0, Math.Min(s.triW - 1, c));

        public void Spawn(EnemyCrowd placement)
        {
            DisposeAgents();
            Count = placement.enemies.Count;
            posA = new NativeArray<double3>(Count, Allocator.Persistent);
            posB = new NativeArray<double3>(Count, Allocator.Persistent);
            velA = new NativeArray<double3>(Count, Allocator.Persistent);
            velB = new NativeArray<double3>(Count, Allocator.Persistent);
            groundedA = new NativeArray<byte>(Count, Allocator.Persistent);
            groundedB = new NativeArray<byte>(Count, Allocator.Persistent);
            edgeA = new NativeArray<byte>(Count, Allocator.Persistent);
            edgeB = new NativeArray<byte>(Count, Allocator.Persistent);
            los = new NativeArray<byte>(Count, Allocator.Persistent);
            sorted = new NativeArray<int>(Count, Allocator.Persistent);
            int maxRays = Count / losSteps + 2;
            rayCommands = new NativeArray<RaycastCommand>(maxRays, Allocator.Persistent);
            rayHits = new NativeArray<RaycastHit>(maxRays, Allocator.Persistent);
            rayEnemy = new NativeArray<int>(maxRays, Allocator.Persistent);
            for (int i = 0; i < Count; i++)
            {
                Vec3 p = placement.enemies[i].position;
                posA[i] = new double3(p.x, p.y, p.z);
                los[i] = 1;
            }
        }

        public double3 Position(int i) => posA[i];

        public void Step(Vec3 target, double dt, int stepIndex)
        {
            raysThisStep = 0;
            if (flow != null)
            {
                flow.SetTarget(target.x, target.z);
                if (flow.rebuilds != flowVersion)
                {
                    flowVersion = flow.rebuilds;
                    for (int c = 0; c < flowDir.Length; c++) flowDir[c] = new float2(flow.dirX[c], flow.dirZ[c]);
                }
            }
            JobHandle grid = new GridJob { s = s, pos = posA, cellStart = cellStart, cellCount = cellCount, sorted = sorted }.Schedule();
            if (useLos) UpdateLos(target, stepIndex);
            var job = new StepJob
            {
                s = s, dt = dt, target = new double3(target.x, target.y, target.z),
                posIn = posA, velIn = velA, groundedIn = groundedA, edgeIn = edgeA, los = los,
                cellStart = cellStart, sorted = sorted, flowDir = flowDir,
                triA = triA, triB = triB, triC = triC, triCellStart = triCellStart, triItems = triItems,
                posOut = posB, velOut = velB, groundedOut = groundedB, edgeOut = edgeB,
            };
            job.Schedule(Count, 32, grid).Complete();
            (posA, posB) = (posB, posA);
            (velA, velB) = (velB, velA);
            (groundedA, groundedB) = (groundedB, groundedA);
            (edgeA, edgeB) = (edgeB, edgeA);
        }

        /// <summary>The staggered subset (1 in 30 enemies a step): PhysX rays in one RaycastCommand batch.</summary>
        void UpdateLos(Vec3 target, int stepIndex)
        {
            int n = 0;
            var chest = new Vector3((float)target.x, (float)(target.y + 1.2), (float)-target.z);
            var qp = new QueryParameters(Physics.DefaultRaycastLayers & ~(1 << 2), false, QueryTriggerInteraction.Ignore, false);
            for (int i = stepIndex % losSteps; i < Count && n < rayCommands.Length; i += losSteps)
            {
                double3 p = posA[i];
                var eye = new Vector3((float)p.x, (float)(p.y + 1.5), (float)-p.z);
                Vector3 d = chest - eye;
                float len = d.magnitude;
                rayCommands[n] = new RaycastCommand(eye, len > 0 ? d / len : Vector3.forward, qp, Mathf.Max(0, len - (float)EnemyCrowd.LosSlack));
                rayEnemy[n] = i;
                n++;
            }
            raysThisStep = n;
            if (n == 0) return;
            RaycastCommand.ScheduleBatch(rayCommands.GetSubArray(0, n), rayHits.GetSubArray(0, n), 8, 1).Complete();
            for (int k = 0; k < n; k++) los[rayEnemy[k]] = (byte)(rayHits[k].collider == null ? 1 : 0);
        }

        [BurstCompile]
        struct GridJob : IJob
        {
            public Settings s;
            [ReadOnly] public NativeArray<double3> pos;
            public NativeArray<int> cellStart, cellCount, sorted;

            public void Execute()
            {
                for (int c = 0; c < cellCount.Length; c++) cellCount[c] = 0;
                for (int i = 0; i < pos.Length; i++) cellCount[Cell(pos[i])]++;
                int total = 0;
                for (int c = 0; c < cellCount.Length; c++)
                {
                    cellStart[c] = total;
                    total += cellCount[c];
                    cellCount[c] = 0;
                }
                cellStart[cellCount.Length] = total;
                for (int i = 0; i < pos.Length; i++)
                {
                    int c = Cell(pos[i]);
                    sorted[cellStart[c] + cellCount[c]++] = i;
                }
            }

            int Cell(double3 p)
            {
                int k = math.clamp((int)math.floor((p.x - s.gridOrigin) / s.sepCell), 0, s.gridW - 1);
                int r = math.clamp((int)math.floor((p.z - s.gridOrigin) / s.sepCell), 0, s.gridW - 1);
                return r * s.gridW + k;
            }
        }

        [BurstCompile]
        struct StepJob : IJobParallelFor
        {
            public Settings s;
            public double dt;
            public double3 target;
            [ReadOnly] public NativeArray<double3> posIn, velIn;
            [ReadOnly] public NativeArray<byte> groundedIn, edgeIn, los;
            [ReadOnly] public NativeArray<int> cellStart, sorted;
            [ReadOnly] public NativeArray<float2> flowDir;
            [ReadOnly] public NativeArray<double3> triA, triB, triC;
            [ReadOnly] public NativeArray<int> triCellStart, triItems;
            [WriteOnly] public NativeArray<double3> posOut, velOut;
            [WriteOnly] public NativeArray<byte> groundedOut, edgeOut;

            public void Execute(int i)
            {
                double3 p = posIn[i], v = velIn[i];
                bool grounded = groundedIn[i] != 0, onEdge = edgeIn[i] != 0;
                double dx = target.x - p.x, dz = target.z - p.z;
                double horizontal = math.sqrt(dx * dx + dz * dz);
                if (grounded || onEdge)
                {
                    double ux = horizontal > 0 ? dx / horizontal : 0, uz = horizontal > 0 ? dz / horizontal : 0;
                    if (s.useFlow && los[i] == 0)
                    {
                        int fk = (int)math.floor((p.x - s.flowOriginX) / s.flowCell), fr = (int)math.floor((p.z - s.flowOriginZ) / s.flowCell);
                        if (fk >= 0 && fk < s.flowW && fr >= 0 && fr < s.flowH)
                        {
                            float2 f = flowDir[fr * s.flowW + fk];
                            if (f.x != 0 || f.y != 0)
                            {
                                ux = f.x;
                                uz = f.y;
                            }
                        }
                    }
                    double vx = 0, vz = 0;
                    if (horizontal > s.stopDistance)
                    {
                        double speed = math.min(s.speed, (horizontal - s.stopDistance) / dt);
                        vx = ux * speed;
                        vz = uz * speed;
                    }
                    int k0 = (int)math.floor((p.x - s.gridOrigin) / s.sepCell), r0 = (int)math.floor((p.z - s.gridOrigin) / s.sepCell);
                    for (int r = r0 - 1; r <= r0 + 1; r++)
                    {
                        for (int k = k0 - 1; k <= k0 + 1; k++)
                        {
                            if (k < 0 || k >= s.gridW || r < 0 || r >= s.gridW) continue;
                            int c = r * s.gridW + k;
                            for (int q = cellStart[c]; q < cellStart[c + 1]; q++)
                            {
                                int j = sorted[q];
                                if (j == i) continue;
                                double3 o = posIn[j];
                                double ox = p.x - o.x, oz = p.z - o.z;
                                double od = math.sqrt(ox * ox + oz * oz);
                                if (od > 0 && od < s.sepRadius)
                                {
                                    double push = (1 - od / s.sepRadius) * s.sepSpeed;
                                    vx += (ox / od) * push;
                                    vz += (oz / od) * push;
                                }
                            }
                        }
                    }
                    v.x = vx;
                    v.z = vz;
                }
                // moveHusk
                double startY = p.y;
                double next = v.y - s.gravity * dt;
                double dy = ((v.y + next) / 2) * dt;
                v.y = next;
                p.x += v.x * dt;
                p.y += dy;
                p.z += v.z * dt;
                grounded = false;
                onEdge = false;
                if (!s.collide)
                {
                    if (p.y <= 0)
                    {
                        p.y = 0;
                        if (v.y < 0) v.y = 0;
                        grounded = true;
                    }
                }
                else if (CapsuleIntersect(p, out double3 n, out double depth))
                {
                    onEdge = n.y < s.floorMinNormalY && n.y > s.edgeNormalY;
                    grounded = n.y >= s.floorMinNormalY || (onEdge && v.y <= s.edgeRiseSpeed);
                    if (grounded)
                    {
                        if (v.y < 0) v.y = 0;
                    }
                    else v -= n * math.min(0, math.dot(n, v));
                    p += n * depth;
                    if (!grounded && v.y <= 0 && math.abs(p.y - startY) < s.supportedDrop) onEdge = true;
                }
                posOut[i] = p;
                velOut[i] = v;
                groundedOut[i] = (byte)(grounded ? 1 : 0);
                edgeOut[i] = (byte)(onEdge ? 1 : 0);
            }

            /// <summary>collider.js capsuleIntersect over the triangle grid: resolve each candidate on a copy, return the net push.</summary>
            bool CapsuleIntersect(double3 feet, out double3 normal, out double depth)
            {
                double3 start = feet + new double3(0, s.radius, 0), end = feet + new double3(0, s.height - s.radius, 0);
                double3 start0 = start;
                double r = s.radius;
                var seen = new FixedList512Bytes<int>();
                int k0 = math.clamp((int)math.floor((feet.x - r - s.triOrigin) / s.triCell), 0, s.triW - 1);
                int k1 = math.clamp((int)math.floor((feet.x + r - s.triOrigin) / s.triCell), 0, s.triW - 1);
                int r0 = math.clamp((int)math.floor((feet.z - r - s.triOrigin) / s.triCell), 0, s.triW - 1);
                int r1 = math.clamp((int)math.floor((feet.z + r - s.triOrigin) / s.triCell), 0, s.triW - 1);
                bool hit = false;
                for (int rr = r0; rr <= r1; rr++)
                {
                    for (int kk = k0; kk <= k1; kk++)
                    {
                        int c = rr * s.triW + kk;
                        for (int q = triCellStart[c]; q < triCellStart[c + 1]; q++)
                        {
                            int t = triItems[q];
                            if (seen.Contains(t)) continue;
                            if (seen.Length < seen.Capacity) seen.Add(t);
                            if (!TriangleCapsule(start, end, r, triA[t], triB[t], triC[t], out double3 cn, out double cd)) continue;
                            hit = true;
                            start += cn * cd;
                            end += cn * cd;
                        }
                    }
                }
                normal = default;
                depth = 0;
                if (!hit) return false;
                double3 d = start - start0;
                depth = math.length(d);
                normal = depth > 0 ? d * (1 / depth) : d;
                return true;
            }

            /// <summary>three's Octree.triangleCapsuleIntersect in Burst (doubles).</summary>
            static bool TriangleCapsule(double3 s0, double3 s1, double r, double3 a, double3 b, double3 c, out double3 normal, out double depth)
            {
                normal = default;
                depth = 0;
                double3 nrm = math.cross(c - b, a - b);
                double nl = math.length(nrm);
                nrm = nrm * (1 / (nl != 0 ? nl : 1));
                double constant = -math.dot(a, nrm);
                double d1 = math.dot(nrm, s0) + constant - r, d2 = math.dot(nrm, s1) + constant - r;
                if ((d1 > 0 && d2 > 0) || (d1 < -r && d2 < -r)) return false;
                double delta = math.abs(d1 / (math.abs(d1) + math.abs(d2)));
                double3 ip = s0 + (s1 - s0) * delta;
                if (Contains(ip, a, b, c))
                {
                    normal = nrm;
                    depth = math.abs(math.min(d1, d2));
                    return true;
                }
                double r2 = r * r;
                return Edge(s0, s1, a, b, r, r2, ref normal, ref depth) || Edge(s0, s1, b, c, r, r2, ref normal, ref depth) || Edge(s0, s1, c, a, r, r2, ref normal, ref depth);
            }

            static bool Edge(double3 s1, double3 e1, double3 s2, double3 e2, double rad, double r2, ref double3 normal, ref double depth)
            {
                double3 r = e1 - s1, sv = e2 - s2, w = s2 - s1;
                double a = math.dot(r, sv), b = math.dot(r, r), c = math.dot(sv, sv), d = math.dot(sv, w), e = math.dot(r, w);
                double t1, t2, divisor = b * c - a * a;
                if (math.abs(divisor) < 1e-10)
                {
                    double q1 = -d / c, q2 = (a - d) / c;
                    if (math.abs(q1 - 0.5) < math.abs(q2 - 0.5)) { t1 = 0; t2 = q1; }
                    else { t1 = 1; t2 = q2; }
                }
                else
                {
                    t1 = (d * a + e * c) / divisor;
                    t2 = (t1 * a - d) / c;
                }
                t2 = math.clamp(t2, 0, 1);
                t1 = math.clamp(t1, 0, 1);
                double3 p1 = r * t1 + s1, p2 = sv * t2 + s2, dp = p1 - p2;
                double dist2 = math.dot(dp, dp);
                if (!(dist2 < r2)) return false;
                double len = math.sqrt(dist2);
                normal = dp * (1 / (len != 0 ? len : 1));
                depth = rad - len;
                return true;
            }

            static bool Contains(double3 p, double3 a, double3 b, double3 c)
            {
                double3 v0 = c - a, v1 = b - a, v2 = p - a;
                double dot00 = math.dot(v0, v0), dot01 = math.dot(v0, v1), dot02 = math.dot(v0, v2), dot11 = math.dot(v1, v1), dot12 = math.dot(v1, v2);
                double denom = dot00 * dot11 - dot01 * dot01;
                if (denom == 0) return false;
                double inv = 1 / denom, u = (dot11 * dot02 - dot01 * dot12) * inv, v = (dot00 * dot12 - dot01 * dot02) * inv;
                double x = 1 - u - v;
                return x >= 0 && v >= 0 && x + v <= 1;
            }
        }

        void DisposeAgents()
        {
            if (posA.IsCreated) posA.Dispose();
            if (posB.IsCreated) posB.Dispose();
            if (velA.IsCreated) velA.Dispose();
            if (velB.IsCreated) velB.Dispose();
            if (groundedA.IsCreated) groundedA.Dispose();
            if (groundedB.IsCreated) groundedB.Dispose();
            if (edgeA.IsCreated) edgeA.Dispose();
            if (edgeB.IsCreated) edgeB.Dispose();
            if (los.IsCreated) los.Dispose();
            if (sorted.IsCreated) sorted.Dispose();
            if (rayCommands.IsCreated) rayCommands.Dispose();
            if (rayHits.IsCreated) rayHits.Dispose();
            if (rayEnemy.IsCreated) rayEnemy.Dispose();
        }

        public void Dispose()
        {
            DisposeAgents();
            if (cellStart.IsCreated) cellStart.Dispose();
            if (cellCount.IsCreated) cellCount.Dispose();
            if (flowDir.IsCreated) flowDir.Dispose();
            if (triA.IsCreated) triA.Dispose();
            if (triB.IsCreated) triB.Dispose();
            if (triC.IsCreated) triC.Dispose();
            if (triCellStart.IsCreated) triCellStart.Dispose();
            if (triItems.IsCreated) triItems.Dispose();
        }
    }
}
