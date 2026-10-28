// The enemy scale bench's managed crowd: N Husk-like capsules chasing a target, after skygrind/src/enemies/husk.js
// (the chase branch: seek the target, stop short, light separation; then moveHusk: gravity, move, and the aggregated
// capsuleIntersect push-out with the grounded / edge rules). Options make the "closer to real" versions: a line-of-
// sight ray every 0.25 s per enemy (staggered), a grid flow field toward the target when it is out of sight, and
// collision through any ICollisionWorld (the C# web collider, or Unity physics). Without a world the floor is y = 0.
// Separation uses a uniform grid (the web loops over its whole pool: fine at its 24 alive, not at 1,000).
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Skygrind.Core
{
    /// <summary>A walkable grid with a BFS flow field toward a target cell (8 neighbours, no corner cutting).</summary>
    public sealed class FlowField
    {
        public readonly double originX, originZ, cell;
        public readonly int w, h;
        public readonly bool[] walkable;
        public readonly float[] dirX, dirZ; // unit direction toward the target per cell (0, 0 at the target or unreachable)
        readonly int[] dist;
        readonly int[] queue;
        public int targetCell = -1;
        public int rebuilds;

        public FlowField(IEnumerable<BoxSpec> boxes, double floorSize, double cellSize, double stepHeight, double bodyHeight)
        {
            cell = cellSize;
            originX = originZ = -floorSize / 2;
            w = h = (int)Math.Ceiling(floorSize / cellSize);
            walkable = new bool[w * h];
            dirX = new float[w * h];
            dirZ = new float[w * h];
            dist = new int[w * h];
            queue = new int[w * h];
            for (int i = 0; i < walkable.Length; i++) walkable[i] = true;
            foreach (BoxSpec b in boxes)
            {
                if (b.max.y <= stepHeight || b.min.y >= bodyHeight) continue; // the floor, or overhead
                for (int r = 0; r < h; r++)
                {
                    for (int k = 0; k < w; k++)
                    {
                        double cx = originX + (k + 0.5) * cell, cz = originZ + (r + 0.5) * cell;
                        if (cx > b.min.x && cx < b.max.x && cz > b.min.z && cz < b.max.z) walkable[r * w + k] = false;
                    }
                }
            }
        }

        public int CellOf(double x, double z)
        {
            int k = (int)Math.Floor((x - originX) / cell), r = (int)Math.Floor((z - originZ) / cell);
            return k >= 0 && k < w && r >= 0 && r < h ? r * w + k : -1;
        }

        /// <summary>Rebuild the field when the target moves to another cell.</summary>
        public void SetTarget(double x, double z)
        {
            int t = CellOf(x, z);
            if (t == targetCell || t < 0) return;
            targetCell = t;
            rebuilds++;
            for (int i = 0; i < dist.Length; i++) dist[i] = int.MaxValue;
            int head = 0, tail = 0;
            dist[t] = 0;
            queue[tail++] = t;
            while (head < tail)
            {
                int c = queue[head++];
                int ck = c % w, cr = c / w;
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dk = -1; dk <= 1; dk++)
                    {
                        if (dk == 0 && dr == 0) continue;
                        int nk = ck + dk, nr = cr + dr;
                        if (nk < 0 || nk >= w || nr < 0 || nr >= h) continue;
                        int n = nr * w + nk;
                        if (!walkable[n] || dist[n] != int.MaxValue) continue;
                        if (dk != 0 && dr != 0 && (!walkable[cr * w + nk] || !walkable[nr * w + ck])) continue; // no corner cutting
                        dist[n] = dist[c] + 1;
                        queue[tail++] = n;
                    }
                }
            }
            for (int c = 0; c < dist.Length; c++)
            {
                dirX[c] = dirZ[c] = 0;
                if (dist[c] == int.MaxValue || dist[c] == 0) continue;
                int ck = c % w, cr = c / w, best = dist[c], bk = 0, br = 0;
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dk = -1; dk <= 1; dk++)
                    {
                        int nk = ck + dk, nr = cr + dr;
                        if ((dk == 0 && dr == 0) || nk < 0 || nk >= w || nr < 0 || nr >= h) continue;
                        int n = nr * w + nk;
                        if (dist[n] >= best) continue;
                        if (dk != 0 && dr != 0 && (!walkable[cr * w + nk] || !walkable[nr * w + ck])) continue;
                        best = dist[n];
                        bk = dk;
                        br = dr;
                    }
                }
                double len = Math.Sqrt(bk * bk + br * br);
                if (len > 0)
                {
                    dirX[c] = (float)(bk / len);
                    dirZ[c] = (float)(br / len);
                }
            }
        }
    }

    /// <summary>
    /// The web bench's shared spawn plan (skygrind port-export/bench/spawnPlan.mjs planSpawns, from the perf presets'
    /// spawnSquads): the map's ground spawns within spawnMin..spawnMax of the player and at its height (+-1 m); squads
    /// of 5, squad s at node (s * 7) % nodes, member i offset (i % 2) * 1.5 m in x and floor(i / 2) * 1.5 m in z.
    /// </summary>
    public static class SpawnPlan
    {
        public static List<Vec3> Nodes(IEnumerable<Vec3> groundSpawns, Vec3 player, double spawnMin, double spawnMax)
        {
            var nodes = new List<Vec3>();
            foreach (Vec3 n in groundSpawns)
            {
                if (double.IsNaN(n.x) || double.IsInfinity(n.x) || double.IsNaN(n.z) || double.IsInfinity(n.z)) continue;
                double dist = JsMath.Hypot(n.x - player.x, n.z - player.z);
                if (dist >= spawnMin && dist <= spawnMax && Math.Abs(n.y - player.y) < 1) nodes.Add(n);
            }
            return nodes;
        }

        public static List<Vec3> Plan(int n, IReadOnlyList<Vec3> nodes)
        {
            var output = new List<Vec3>(n);
            for (int s = 0; output.Count < n; s++)
            {
                Vec3 b = nodes[(s * 7) % nodes.Count];
                for (int i = 0; i < 5 && output.Count < n; i++) output.Add(new Vec3(b.x + (i % 2) * 1.5, b.y, b.z + Math.Floor(i / 2.0) * 1.5));
            }
            return output;
        }
    }

    public sealed class EnemyCrowd
    {
        public sealed class Enemy
        {
            public Vec3 position;
            public Vec3 velocity;
            public bool grounded;
            public bool onEdge;
            public bool los = true;
        }

        public const double LosInterval = 0.25; // s between one enemy's line-of-sight rays
        public const double LosSlack = 0.4; // m short of the target that still counts as seen
        const double ChestHeight = 1.2; // m, the target point (the player's chest)
        const double EyeHeight = 1.5; // m, the enemy's head

        readonly MovementConfig config;
        readonly HuskConfig h;
        readonly ICollisionWorld world; // null: the floor plane y = 0
        readonly bool useLos;
        readonly FlowField flow; // null: seek straight at the target
        public readonly List<Enemy> enemies = new List<Enemy>();
        readonly double gravity;
        int losSteps;

        // separation grid
        readonly double sepCell, gridOrigin;
        readonly int gridW;
        readonly int[] cellHead;
        int[] next = new int[0];

        public int raysThisStep;

        /// <summary>A breakdown run only (adds timer reads): Stopwatch ticks spent in each phase, summed until reset.</summary>
        public bool profile;
        public readonly long[] phaseTicks = new long[4]; // grid + flow field, line of sight, steering + separation, move + push-out
        public long neighbourChecks; // separation pairs examined (profile only)

        public EnemyCrowd(MovementConfig config, ICollisionWorld world, bool useLos, FlowField flow, double floorSize)
        {
            this.config = config;
            h = config.husk;
            this.world = world;
            this.useLos = useLos;
            this.flow = flow;
            gravity = MovementMath.GravityFor(config.jump.height, config.jump.apexTime);
            losSteps = MovementMath.StepsFor(LosInterval, 1 / config.loop.simHz);
            sepCell = h.separationRadius;
            gridOrigin = -floorSize / 2 - 4;
            gridW = (int)Math.Ceiling((floorSize + 8) / sepCell);
            cellHead = new int[gridW * gridW];
        }

        /// <summary>N enemies on random walkable floor cells at least `minDistance` from the target (seeded).</summary>
        public void Spawn(int n, FlowField cells, Vec3 target, double minDistance, Mulberry32 rng)
        {
            enemies.Clear();
            int tries = 0;
            while (enemies.Count < n && tries++ < n * 1000)
            {
                int c = (int)(rng.Next() * cells.walkable.Length);
                if (!cells.walkable[c]) continue;
                double x = cells.originX + (c % cells.w + 0.5) * cells.cell, z = cells.originZ + (c / cells.w + 0.5) * cells.cell;
                if (JsMath.Hypot(x - target.x, z - target.z) < minDistance) continue;
                enemies.Add(new Enemy { position = new Vec3(x, h.spawnLift, z) });
            }
            next = new int[enemies.Count];
        }

        /// <summary>Enemies at these feet positions (+ husk.spawnLift, as the web's spawn does), in order.</summary>
        public void Place(IReadOnlyList<Vec3> feet)
        {
            enemies.Clear();
            foreach (Vec3 f in feet) enemies.Add(new Enemy { position = new Vec3(f.x, f.y + h.spawnLift, f.z) });
            next = new int[enemies.Count];
        }

        int GridCell(double x, double z)
        {
            int k = (int)Math.Floor((x - gridOrigin) / sepCell), r = (int)Math.Floor((z - gridOrigin) / sepCell);
            k = Math.Max(0, Math.Min(gridW - 1, k));
            r = Math.Max(0, Math.Min(gridW - 1, r));
            return r * gridW + k;
        }

        /// <summary>One fixed step for every enemy (in order, positions update in place like the web loop).</summary>
        public void Step(Vec3 target, double dt, int stepIndex)
        {
            raysThisStep = 0;
            long t = profile ? Stopwatch.GetTimestamp() : 0;
            if (flow != null) flow.SetTarget(target.x, target.z);
            for (int i = 0; i < cellHead.Length; i++) cellHead[i] = -1;
            for (int i = 0; i < enemies.Count; i++)
            {
                int c = GridCell(enemies[i].position.x, enemies[i].position.z);
                next[i] = cellHead[c];
                cellHead[c] = i;
            }
            Vec3 chest = new Vec3(target.x, target.y + ChestHeight, target.z);
            if (profile) Lap(0, ref t);
            int checks = 0;
            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy e = enemies[i];
                if (useLos && world != null && i % losSteps == stepIndex % losSteps) UpdateLos(e, chest);
                if (profile) Lap(1, ref t);
                double dx = target.x - e.position.x, dz = target.z - e.position.z;
                double horizontal = JsMath.Hypot(dx, dz);
                if (e.grounded || e.onEdge)
                {
                    double ux = horizontal > 0 ? dx / horizontal : 0, uz = horizontal > 0 ? dz / horizontal : 0;
                    if (flow != null && !e.los)
                    {
                        int fc = flow.CellOf(e.position.x, e.position.z);
                        if (fc >= 0 && (flow.dirX[fc] != 0 || flow.dirZ[fc] != 0))
                        {
                            ux = flow.dirX[fc];
                            uz = flow.dirZ[fc];
                        }
                    }
                    double vx = 0, vz = 0;
                    if (horizontal > h.stopDistance)
                    {
                        double speed = JsMath.Min(h.speed, (horizontal - h.stopDistance) / dt);
                        vx = ux * speed;
                        vz = uz * speed;
                    }
                    // Light separation from neighbours (husk.js), through the grid.
                    int k0 = (int)Math.Floor((e.position.x - gridOrigin) / sepCell), r0 = (int)Math.Floor((e.position.z - gridOrigin) / sepCell);
                    for (int r = r0 - 1; r <= r0 + 1; r++)
                    {
                        for (int k = k0 - 1; k <= k0 + 1; k++)
                        {
                            if (k < 0 || k >= gridW || r < 0 || r >= gridW) continue;
                            for (int j = cellHead[r * gridW + k]; j >= 0; j = next[j])
                            {
                                if (j == i) continue;
                                checks++;
                                Enemy o = enemies[j];
                                double ox = e.position.x - o.position.x, oz = e.position.z - o.position.z;
                                double od = JsMath.Hypot(ox, oz);
                                if (od > 0 && od < h.separationRadius)
                                {
                                    double push = (1 - od / h.separationRadius) * h.separationSpeed;
                                    vx += (ox / od) * push;
                                    vz += (oz / od) * push;
                                }
                            }
                        }
                    }
                    e.velocity.x = vx;
                    e.velocity.z = vz;
                }
                if (profile) Lap(2, ref t);
                Move(e, dt);
                if (profile) Lap(3, ref t);
            }
            if (profile) neighbourChecks += checks;
        }

        void Lap(int phase, ref long t)
        {
            long now = Stopwatch.GetTimestamp();
            phaseTicks[phase] += now - t;
            t = now;
        }

        void UpdateLos(Enemy e, Vec3 chest)
        {
            raysThisStep++;
            Vec3 eye = new Vec3(e.position.x, e.position.y + EyeHeight, e.position.z);
            Vec3 d = chest - eye;
            double len = d.Length;
            e.los = !(world.RayIntersect(eye, Vec3.DivideScalar(d, len != 0 ? len : 1), out RayHit hit) && hit.distance < len - LosSlack);
        }

        /// <summary>husk.js moveHusk: gravity, the velocity, then the aggregated capsule push-out.</summary>
        void Move(Enemy e, double dt)
        {
            double startY = e.position.y;
            VerticalStepResult vert = MovementMath.VerticalStep(e.velocity.y, gravity, dt);
            e.velocity.y = vert.vy;
            e.position.x += e.velocity.x * dt;
            e.position.y += vert.dy;
            e.position.z += e.velocity.z * dt;
            e.grounded = false;
            e.onEdge = false;
            if (world == null)
            {
                if (e.position.y <= 0)
                {
                    e.position.y = 0;
                    if (e.velocity.y < 0) e.velocity.y = 0;
                    e.grounded = true;
                }
                return;
            }
            var capsule = new Capsule
            {
                radius = h.radius,
                start = new Vec3(e.position.x, e.position.y + h.radius, e.position.z),
                end = new Vec3(e.position.x, e.position.y + h.height - h.radius, e.position.z),
            };
            if (!WorldCollider.CapsuleIntersect(world, capsule, out Vec3 n, out double depth)) return;
            // A slanted summed contact (the floor + a wall pressed into, or a ledge's corner) is ground while not rising.
            e.onEdge = n.y < config.surface.floorMinNormalY && n.y > config.ai.edgeNormalY;
            e.grounded = n.y >= config.surface.floorMinNormalY || (e.onEdge && e.velocity.y <= config.ai.edgeRiseSpeed);
            if (e.grounded)
            {
                if (e.velocity.y < 0) e.velocity.y = 0;
            }
            else
            {
                e.velocity -= n * JsMath.Min(0, Vec3.Dot(n, e.velocity));
            }
            capsule.Translate(n * depth);
            e.position = new Vec3(capsule.start.x, capsule.start.y - h.radius, capsule.start.z);
            if (!e.grounded && e.velocity.y <= 0 && Math.Abs(e.position.y - startY) < config.ai.supportedDrop) e.onEdge = true;
        }
    }
}
