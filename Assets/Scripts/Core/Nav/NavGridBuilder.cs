// Port of skygrind/src/ai/navGrid.js (NIGHT15 U3-28.3, builder N): createNavGrid(config, {getCollider, tagAt})
// .build(floorSize, {sync: true}), the call the web's exporter makes (PortExport/scripts/maps.mjs navGridExport), line
// for line with its comments, and the exporter's step around it (maps.mjs exportMap: the collider, the zone parts,
// tagAt, the export object). The web module's header:
//
// The Survival navigation grid (POWER.md P1): a layered walkable grid at 1 m cells over the whole map, built
// from the world collider's geometry (the static map + the core; the zone force fields are left out: a locked
// zone masks its nodes instead, so opening a gate needs no rebuild). Every column is scanned top-down with rays;
// each up-facing surface where the probe capsule fits (a Husk with a margin) is a node with its floor height,
// its clearance (headroom to the next surface above) and a tag (the zone). Links between nodes:
//   walk  a neighbour (8 directions) within a 0.6 m step
//   jump  an orthogonal neighbour up to 2.2 m higher (headroom for the arc)
//   drop  an orthogonal neighbour up to 5 m lower
//   gap   over up to 3 m of nothing (or a much lower floor) to a floor at most 1 m higher / 5 m lower,
//         checked with two rays so a jump never goes through a wall
// Builds are time-sliced (a job scans columns, links nodes, then swaps in a finished snapshot on a per-call
// time budget), so queries always read a complete grid and a rebuild never hitches. A region rebuild (the core
// after a reshape) rescans only its columns. Link kinds, costs and the reverse links (for the flow field's
// Dijkstra toward the player) are flat typed arrays.
//
// The port: the sync build only, as one pass (scan every column, pack, link every node, finish the links: the same
// order as the time-sliced job, whose slicing changes nothing in the result). The run-time queries, the reverse links,
// setBlocked and the region rebuild are the game's (NavField.cs NavGrid.FromExport reads this export). `collider` is
// collider.js `geometry`: the static map Octree, then the core (LayeredWorld), no live layers; its capsule test is
// WorldCollider.CapsuleIntersect (the same push-out loop). Floats: the column scan stores [y, clearance, tag, edge] in a
// Float32Array and the snapshot's y, clear and linkCost are Float32Arrays: each value is computed in double and cast
// to float once at the store (PORT-RULES 2); every read of them is widened to double before any arithmetic. The web's
// `stats` counters (rays, capsules, ms) are debugging only and not ported. A load-time job: it allocates freely.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    public static class NavGridBuilder
    {
        // export const LINK = { walk: 0, jump: 1, drop: 2, gap: 3 }; (Link in NavField.cs)
        // export const LINK_NAMES = ['walk', 'jump', 'drop', 'gap'];
        static readonly string[] LINK_NAMES = { "walk", "jump", "drop", "gap" };
        static readonly int[,] DIRS = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }, { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };
        // Probe just off the cell centre: map boxes sit on whole and half metres, so a ray down an exact edge is avoided.
        const double EPS_X = 0.0137;
        const double EPS_Z = 0.0071;
        static readonly float[] EMPTY = new float[0];
        static readonly int[,] SIDES = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
        const int STRIDE = 4; // per floor in a column: y, clearance, tag, edge
        static readonly double SQRT2 = Math.Sqrt(2); // Math.SQRT2 (sqrt is correctly rounded: the same double)
        static readonly Vec3 DOWN = new Vec3(0, -1, 0);
        static readonly Vec3 UP = new Vec3(0, 1, 0);

        // The exporter's strings (maps.mjs navGridExport / exportMap), so a written grid reads like the web's export.
        const string NavGridSource = "createNavGrid(config, {getCollider: () => arena.octree.geometry, tagAt}).build(map.floorSize, {sync: true}) (src/ai/navGrid.js)";
        const string NavGridColumnCenter = "column c: x = origin + (c % size + 0.5) * cell, z = origin + (floor(c / size) + 0.5) * cell";
        const string NavGridNodesNote = "parallel arrays, one entry per node (index = node id). x, z: the column centre; y: floor height; clear: headroom to the next surface above (m); tag: zone index (-1: none); edge: 1 when a floor is missing under one side of the body; col: column index; blocked: all 0 at build (locked zones are masked at run time by tag)";
        const string NavGridLinksNote = "CSR: node i's links are linkStart[i] .. linkStart[i + 1] - 1; linkTo = target node, linkKind = linkKinds value, linkCost = m-equivalent cost";
        const string NavGridTagAtZones = "zoneIndexAt(zoneParts.parts, x, y + 1, z), as survival/zones.js tagAt";
        const string NavGridTagAtNone = "none (every tag is -1), as main.js when no zones are loaded";
        const string ZonePartsSource = "resolveZones(map.zones, map.zoneHoles) (src/survival/zoneMath.js)";

        /// <summary>
        /// createNavGrid(config, {getCollider: () => collider, tagAt}).build(floorSize, {sync: true}), returned in the
        /// exporter's shape (maps.mjs navGridExport: source, cell, size, origin, columnCenter, count, linkKinds, linkCounts,
        /// nodes {note, x, z, y, clear, tag, edge, col, blocked}, links {note, linkStart, linkTo, linkKind, linkCost}, then
        /// tagAt and corePreset; numbers as doubles, the float32 arrays rounded through float as the web's typed arrays
        /// are; -0, NaN and the infinities as {"$num": ...} like the export's enc()). tagAt: zone index at (x, y, z), or
        /// null (every tag -1). The collider is the map's Octree alone (a map without a core, or the core at 'flat', where
        /// no pillar collides).
        /// </summary>
        public static Dictionary<string, object> Build(GameConfig cfg, ThreeOctree collider, double floorSize, Func<double, double, double, int> tagAt)
        {
            return Build(cfg, (ICollisionWorld)collider, floorSize, tagAt, "flat");
        }

        /// <summary>
        /// Build over any web collider view (collider.js `geometry`: the map's Octree plus the core, LayeredWorld); corePreset
        /// is the export's corePreset string (the core's preset when the grid was built).
        /// </summary>
        public static Dictionary<string, object> Build(GameConfig cfg, ICollisionWorld collider, double floorSize, Func<double, double, double, int> tagAt, string corePreset)
        {
            var job = new NavGridJob(cfg, collider, tagAt);
            job.Build(floorSize);
            return job.Export(floorSize, tagAt != null, corePreset);
        }

        /// <summary>
        /// The whole exporter step for one map JSON (maps.mjs exportMap): the collider (PortData.BuildMapOctree, the core at
        /// 'flat'), the zone parts (ZoneMath.ResolveZones over data.zones / data.zoneHoles, or none), tagAt as survival/zones.js
        /// (zoneIndexAt(parts, x, y + 1, z)), then Build. Returns (navGrid, zoneParts export object or null).
        /// </summary>
        public static (Dictionary<string, object> navGrid, Dictionary<string, object> zoneParts) ExportFor(GameConfig cfg, Dictionary<string, object> mapJson)
        {
            var data = PortData.Obj(mapJson["data"]);
            // arena.load(): the static Octree ([floor, ...map.boxes], map.barriers, map.wedges), then the core grid inside the
            // world collider, its heights set to presetHeights('flat', ...) (every pillar 0 m: none collides; kept so the
            // collider is the web's by construction). The core's spec as SurvivalMap.FromExport reads it (core.spec).
            ICollisionWorld collider = PortData.BuildMapOctree(mapJson);
            if (mapJson.TryGetValue("core", out object coreJson) && coreJson is Dictionary<string, object> coreObj)
            {
                var spec = PortData.Obj(coreObj["spec"]);
                var cs = new CoreSpec { x0 = PortData.Num(spec["x0"]), z0 = PortData.Num(spec["z0"]), cells = (int)PortData.Num(spec["cells"]), cellSize = PortData.Num(spec["cellSize"]) };
                MovementConfig m = cfg.movement;
                var core = new CoreGrid(m, cs);
                core.SetHeights(CoreGrid.PresetHeights("flat", cs.cells, m.core.maxHeight, m.core.heightStep, new Mulberry32(1)));
                collider = new LayeredWorld(collider, core);
            }

            Dictionary<string, object> zoneParts = null;
            Func<double, double, double, int> tagAt = null;
            // if (map.zones): any array (an empty one too) is truthy; a missing / undefined / null one is not.
            if (data.TryGetValue("zones", out object zonesJson) && zonesJson is List<object> zoneList)
            {
                var zones = new List<ZoneSource>();
                var zoneIds = new List<object>();
                foreach (object z in zoneList)
                {
                    var d = PortData.Obj(z);
                    object id = d.TryGetValue("id", out object idv) ? idv : NavGridUndefined();
                    var src = new ZoneSource { id = id as string, priority = d.TryGetValue("priority", out object pv) && IsNumber(pv) ? PortData.Num(pv) : double.NaN };
                    if (d.TryGetValue("boxes", out object bv) && bv is List<object> boxes)
                        foreach (object b in boxes) src.boxes.Add(NavGridBox(b));
                    zones.Add(src);
                    zoneIds.Add(id);
                }
                // map.zoneHoles ?? []
                var holes = new List<MapBox>();
                if (data.TryGetValue("zoneHoles", out object hv) && hv is List<object> holeList)
                    foreach (object h in holeList) holes.Add(NavGridBox(h));
                List<ZonePart> parts = ZoneMath.ResolveZones(zones, holes);
                var partList = new List<object>(parts.Count);
                foreach (ZonePart p in parts)
                {
                    partList.Add(new Dictionary<string, object>
                    {
                        ["zone"] = Enc(p.zone),
                        ["min"] = new List<object> { Enc(p.min.x), Enc(p.min.y), Enc(p.min.z) },
                        ["max"] = new List<object> { Enc(p.max.x), Enc(p.max.y), Enc(p.max.z) },
                    });
                }
                zoneParts = new Dictionary<string, object>
                {
                    ["source"] = ZonePartsSource,
                    ["zoneIds"] = zoneIds,
                    ["startZone"] = data.TryGetValue("startZone", out object sz) ? sz : NavGridUndefined(),
                    ["parts"] = partList,
                };
                // survival/zones.js tagAt(x, y, z) = zoneIndexAt(parts, x, y + 1, z) (the air 1 m over the floor point).
                tagAt = (x, y, z) => ZoneMath.ZoneIndexAt(parts, x, y + 1, z);
            }
            var navGrid = Build(cfg, collider, PortData.Num(mapJson["floorSize"]), tagAt, "flat");
            return (navGrid, zoneParts);
        }

        // --- the export's value encoding (lib.mjs enc) ----------------------------------------------------------------

        /// <summary>A number as enc() writes it: -0, NaN and the infinities as {"$num": ...}, the rest as the double.</summary>
        static object Enc(double v)
        {
            if (v == 0 && BitConverter.DoubleToInt64Bits(v) != 0) return new Dictionary<string, object> { ["$num"] = "-0" };
            if (double.IsNaN(v)) return new Dictionary<string, object> { ["$num"] = "NaN" };
            if (double.IsPositiveInfinity(v)) return new Dictionary<string, object> { ["$num"] = "Infinity" };
            if (double.IsNegativeInfinity(v)) return new Dictionary<string, object> { ["$num"] = "-Infinity" };
            return v;
        }

        static Dictionary<string, object> NavGridUndefined() => new Dictionary<string, object> { ["$undefined"] = true };

        static bool IsNumber(object o) => o is double || (o is Dictionary<string, object> d && d.ContainsKey("$num"));

        /// <summary>A zone or hole box of the map data ({min, max, kind}; kind may be {"$undefined": true}).</summary>
        static MapBox NavGridBox(object o)
        {
            var d = PortData.Obj(o);
            return new MapBox { min = PortData.V3(d["min"]), max = PortData.V3(d["max"]), kind = d.TryGetValue("kind", out object k) ? k as string : null };
        }

        /// <summary>config.ai.grid (and the two config.surface limits), read once.</summary>
        sealed class NavGridParams
        {
            public readonly double cell, probeRadius, probeLift, probeHeight, minClear, bodyHeight, scanTop, scanBottom, maxLayers;
            public readonly double step, jumpUp, drop, stepSmooth, riserLift, gapCells, gapDiagCells, gapUp, gapDown, gapLiftHigh, gapLiftLow;
            public readonly double jumpCost, dropCost, gapCost, edgeCost;
            public readonly double floorMinNormalY, wallMaxAbsNormalY;

            public NavGridParams(GameConfig cfg)
            {
                double G(string k) => cfg.Num("ai.grid." + k);
                cell = G("cell");
                probeRadius = G("probeRadius");
                probeLift = G("probeLift");
                probeHeight = G("probeHeight");
                minClear = G("minClear");
                bodyHeight = G("bodyHeight");
                scanTop = G("scanTop");
                scanBottom = G("scanBottom");
                maxLayers = G("maxLayers");
                step = G("step");
                jumpUp = G("jumpUp");
                drop = G("drop");
                stepSmooth = G("stepSmooth");
                riserLift = G("riserLift");
                gapCells = G("gapCells");
                gapDiagCells = G("gapDiagCells");
                gapUp = G("gapUp");
                gapDown = G("gapDown");
                gapLiftHigh = G("gapLiftHigh");
                gapLiftLow = G("gapLiftLow");
                jumpCost = G("jumpCost");
                dropCost = G("dropCost");
                gapCost = G("gapCost");
                edgeCost = G("edgeCost");
                floorMinNormalY = cfg.Num("surface.floorMinNormalY");
                wallMaxAbsNormalY = cfg.Num("surface.wallMaxAbsNormalY");
            }
        }

        /// <summary>One build: createNavGrid's closure state, the scan, the snapshot (pack) and its links.</summary>
        sealed class NavGridJob
        {
            readonly GameConfig cfg;
            readonly NavGridParams g;
            readonly ICollisionWorld collider; // getCollider()
            readonly Func<double, double, double, int> tagAt;
            readonly List<double> layers = new List<double>(); // scratch for one column scan
            int size; // cells per side
            double origin; // world x / z of cell 0's min edge
            float[][] columns; // per column: Float32Array [y, clear, tag, edge, ...] per floor, lowest floor first

            // The snapshot (pack()): flat node arrays, then the links (finishLinks()).
            int count;
            int[] colStart, col;
            float[] y, clear;
            short[] tag;
            byte[] edge, blocked;
            int[] linkStart, linkTo;
            byte[] linkKind;
            float[] linkCost;
            readonly int[] linkCounts = new int[4];
            // The link list linkNode pushes (from, to, kind, cost), in production order.
            readonly List<int> listFrom = new List<int>();
            readonly List<int> listTo = new List<int>();
            readonly List<byte> listKind = new List<byte>();
            readonly List<double> listCost = new List<double>();

            public NavGridJob(GameConfig cfg, ICollisionWorld collider, Func<double, double, double, int> tagAt)
            {
                this.cfg = cfg;
                g = new NavGridParams(cfg);
                this.collider = collider;
                this.tagAt = tagAt;
            }

            double ColX(int c) => origin + ((c % size) + 0.5) * g.cell;

            double ColZ(int c) => origin + ((c / size) + 0.5) * g.cell; // Math.floor(c / size): c >= 0

            double Y(int n) => y[n]; // a Float32Array read: the float widened (never float arithmetic)

            double Clear(int n) => clear[n];

            /// <summary>`length || 1` (0 and NaN are falsy).</summary>
            static double Or1(double v) => v != 0 && !double.IsNaN(v) ? v : 1;

            /** Scan one column: every walkable floor with its clearance and tag. */
            float[] ScanColumn(int c)
            {
                double x = ColX(c) + EPS_X;
                double z = ColZ(c) + EPS_Z;
                layers.Clear();
                double yy = g.scanTop;
                double above = double.PositiveInfinity; // the surface found before this one (every down-ray hit is an up-facing surface)
                for (int guard = 0; guard < 32 && yy > g.scanBottom; guard++)
                {
                    if (!collider.RayIntersect(new Vec3(x, yy, z), DOWN, out RayHit hit)) break;
                    double sy = yy - hit.distance;
                    Vec3 normal = hit.normal; // hit.triangle.getNormal(normal)
                    if (normal.y >= g.floorMinNormalY && layers.Count < g.maxLayers * STRIDE)
                    {
                        bool roofHit = collider.RayIntersect(new Vec3(x, sy + 0.05, z), UP, out RayHit roof);
                        double clr = roofHit ? roof.distance + 0.05 : g.scanTop - sy;
                        // Buried: rays skip back faces, so from inside a solid the up ray never sees the solid's underside and
                        // runs past the top we came down from. (A capsule fully inside a box touches no face either.)
                        bool buried = sy + clr >= above - 0.01;
                        if (clr >= g.minClear && !buried)
                        {
                            var capsule = new Capsule
                            {
                                start = new Vec3(x, sy + g.probeRadius + g.probeLift, z),
                                end = new Vec3(x, sy + g.probeHeight - g.probeRadius, z),
                                radius = g.probeRadius,
                            };
                            if (!WorldCollider.CapsuleIntersect(collider, capsule, out _, out _))
                            {
                                // Support: a floor within a step under all four sides of the body; one missing = an edge node
                                // (routes pay more to walk along it: half a body hanging over a side slips off).
                                int sides = 0;
                                for (int i = 0; i < 4; i++)
                                {
                                    var o = new Vec3(x + SIDES[i, 0] * g.probeRadius, sy + g.step, z + SIDES[i, 1] * g.probeRadius);
                                    if (collider.RayIntersect(o, DOWN, out RayHit under) && Math.Abs(sy + g.step - under.distance - sy) <= g.step) sides++;
                                }
                                layers.Add(sy);
                                layers.Add(clr);
                                layers.Add(tagAt != null ? tagAt(x, sy, z) : -1);
                                layers.Add(sides < 4 ? 1 : 0);
                            }
                        }
                    }
                    above = sy;
                    yy = sy - 0.05;
                }
                if (layers.Count == 0) return EMPTY;
                // Found top-down: store lowest first.
                var output = new float[layers.Count];
                for (int i = 0, j = layers.Count - STRIDE; j >= 0; i += STRIDE, j -= STRIDE)
                {
                    for (int k = 0; k < STRIDE; k++) output[i + k] = (float)layers[j + k];
                }
                return output;
            }

            /** Pack the columns into flat node arrays (a new snapshot, links still empty). */
            void Pack()
            {
                int n = 0;
                for (int c = 0; c < columns.Length; c++) n += columns[c].Length / STRIDE;
                count = n;
                colStart = new int[size * size + 1];
                y = new float[n];
                clear = new float[n];
                tag = new short[n]; // Int16Array
                col = new int[n];
                edge = new byte[n]; // Uint8Array
                blocked = new byte[n];
                int k = 0;
                for (int c = 0; c < size * size; c++)
                {
                    colStart[c] = k;
                    float[] L = columns[c];
                    for (int i = 0; i < L.Length; i += STRIDE)
                    {
                        y[k] = L[i];
                        clear[k] = L[i + 1];
                        tag[k] = (short)L[i + 2]; // an integer zone index (or -1) stored as a float32: exact
                        edge[k] = (byte)L[i + 3];
                        col[k] = c;
                        k++;
                    }
                }
                colStart[size * size] = k;
            }

            /** Does column c have a floor with y in [lo, hi]? */
            bool HasFloor(int c, double lo, double hi)
            {
                for (int b = colStart[c]; b < colStart[c + 1]; b++) if (Y(b) >= lo && Y(b) <= hi) return true;
                return false;
            }

            /** Nothing solid on the straight line between two nodes, `lift` m over each floor. */
            bool ClearLine(int a, int b, double lift)
            {
                double ax = ColX(col[a]) + EPS_X;
                double az = ColZ(col[a]) + EPS_Z;
                double bx = ColX(col[b]) + EPS_X;
                double bz = ColZ(col[b]) + EPS_Z;
                var lineOrigin = new Vec3(ax, Y(a) + lift, az);
                var dir = new Vec3(bx - ax, Y(b) - Y(a), bz - az);
                double length = dir.Length;
                dir = Vec3.DivideScalar(dir, Or1(length));
                bool hit = collider.RayIntersect(lineOrigin, dir, out RayHit h);
                return !hit || h.distance >= length;
            }

            /**
             * Is there a riser (a vertical face) between two floors, rather than a slope? A horizontal ray just over the
             * lower floor toward the higher one hits a wall-like face only at a riser (a slope's face is floor-like).
             */
            bool Riser(int a, int b)
            {
                double lo = JsMath.Min(Y(a), Y(b));
                double ax = ColX(col[a]) + EPS_X;
                double az = ColZ(col[a]) + EPS_Z;
                var lineOrigin = new Vec3(ax, lo + g.riserLift, az);
                var dir = new Vec3(ColX(col[b]) + EPS_X - ax, 0, ColZ(col[b]) + EPS_Z - az);
                double length = dir.Length;
                dir = Vec3.DivideScalar(dir, Or1(length));
                return collider.RayIntersect(lineOrigin, dir, out RayHit hit) && hit.distance < length && Math.Abs(hit.normal.y) < g.wallMaxAbsNormalY;
            }

            void Push(int a, int b, byte kind, double cost)
            {
                listFrom.Add(a);
                listTo.Add(b);
                listKind.Add(kind);
                listCost.Add(cost);
            }

            /** Every link out of node a, pushed as (from, to, kind, cost) into `out`. */
            void LinkNode(int a)
            {
                int c = col[a];
                int ci = c % size;
                int ck = (c - ci) / size;
                double ya = Y(a);
                double ca = Clear(a);
                for (int d = 0; d < 8; d++)
                {
                    int di = DIRS[d, 0];
                    int dk = DIRS[d, 1];
                    bool diag = di != 0 && dk != 0;
                    int ni = ci + di;
                    int nk = ck + dk;
                    if (ni < 0 || ni >= size || nk < 0 || nk >= size) continue;
                    int nc = nk * size + ni;
                    double dist = (diag ? SQRT2 : 1) * g.cell;
                    for (int b = colStart[nc]; b < colStart[nc + 1]; b++)
                    {
                        double dy = Y(b) - ya;
                        if (Math.Abs(dy) <= g.step)
                        {
                            // A diagonal needs both side cells at this height (no corner cutting).
                            if (diag && !(HasFloor(c + di, ya - g.step, ya + g.step) && HasFloor(c + dk * size, ya - g.step, ya + g.step))) continue;
                            // Up a riser taller than a body can slide over (a ramp's side, a low step): a small jump, not a walk.
                            if (dy > g.stepSmooth && Riser(a, b))
                            {
                                if (!diag && ca >= dy + g.bodyHeight) Push(a, b, Link.Jump, dist + g.jumpCost * 0.5 + dy);
                                continue;
                            }
                            Push(a, b, Link.Walk, dist * (edge[b] != 0 ? g.edgeCost : 1));
                        }
                        else if (!diag && dy > 0 && dy <= g.jumpUp && ca >= dy + g.bodyHeight)
                        {
                            Push(a, b, Link.Jump, dist + g.jumpCost + dy);
                        }
                        else if (!diag && dy < 0 && -dy <= g.drop && Clear(b) >= -dy + g.bodyHeight)
                        {
                            Push(a, b, Link.Drop, dist + g.dropCost);
                        }
                    }
                    // Gaps: over cells with no floor near this height, to the first landing in this direction.
                    double kMax = diag ? g.gapDiagCells : g.gapCells;
                    for (int k = 2; k <= kMax; k++)
                    {
                        int mi = ci + di * (k - 1);
                        int mk = ck + dk * (k - 1);
                        int ti = ci + di * k;
                        int tk = ck + dk * k;
                        if (ti < 0 || ti >= size || tk < 0 || tk >= size) break;
                        // Anything in the arc's band between (a floor to walk on, or an obstacle top) ends the scan.
                        if (HasFloor(mk * size + mi, ya - g.step, ya + g.gapUp + g.bodyHeight)) break;
                        int tc = tk * size + ti;
                        bool found = false;
                        for (int b = colStart[tc]; b < colStart[tc + 1]; b++)
                        {
                            // A landing across the gap (the gap's own floor far below is a drop, not a gap).
                            double dy = Y(b) - ya;
                            if (dy < -g.gapDown || dy > g.gapUp || Clear(b) < g.bodyHeight + JsMath.Max(0, -dy)) continue;
                            found = true;
                            if (ClearLine(a, b, g.gapLiftHigh) && ClearLine(a, b, g.gapLiftLow))
                            {
                                Push(a, b, Link.Gap, k * (diag ? SQRT2 : 1) * g.cell + g.gapCost);
                            }
                        }
                        if (found) break;
                    }
                }
            }

            /** Flat link arrays (forward, sorted by source) + the reverse index. */
            void FinishLinks()
            {
                int m = listFrom.Count;
                linkStart = new int[count + 1];
                linkTo = new int[m];
                linkKind = new byte[m]; // Uint8Array
                linkCost = new float[m]; // Float32Array
                // Links were produced node by node, so they are already grouped by source.
                for (int i = 0; i < m; i++)
                {
                    linkStart[listFrom[i] + 1]++;
                    linkTo[i] = listTo[i];
                    linkKind[i] = listKind[i];
                    linkCost[i] = (float)listCost[i];
                    linkCounts[listKind[i]]++;
                }
                for (int i = 0; i < count; i++) linkStart[i + 1] += linkStart[i];
                // The reverse index (revStart / revFrom / revLink) is not exported: NavGrid.FromExport rebuilds it.
            }

            /**
             * build(floorSize, {sync: true}) on a new grid: size = Math.round(floorSize / cell), origin = -floorSize / 2,
             * then the job's phases in order: scan every column, pack, link every node, finish (applyBlocked: no blockFn
             * at build, so every node is 0).
             */
            public void Build(double floorSize)
            {
                int n = (int)JsMath.Round(floorSize / g.cell);
                size = n;
                origin = -floorSize / 2;
                columns = new float[n * n][];
                for (int c = 0; c < n * n; c++) columns[c] = ScanColumn(c);
                Pack();
                for (int a = 0; a < count; a++) LinkNode(a);
                FinishLinks();
            }

            static List<object> Nums(int n, Func<int, double> f)
            {
                var list = new List<object>(n);
                for (int i = 0; i < n; i++) list.Add(Enc(f(i)));
                return list;
            }

            /** maps.mjs navGridExport (without buildMs, which the exporter deletes), then exportMap's tagAt and corePreset. */
            public Dictionary<string, object> Export(double floorSize, bool zoned, string corePreset)
            {
                int m = linkTo.Length;
                var linkCountsObj = new Dictionary<string, object>();
                for (int k = 0; k < LINK_NAMES.Length; k++) linkCountsObj[LINK_NAMES[k]] = Enc(linkCounts[k]);
                return new Dictionary<string, object>
                {
                    ["source"] = NavGridSource,
                    ["cell"] = Enc(g.cell), // config.ai.grid.cell
                    ["size"] = Enc(size),
                    ["origin"] = Enc(-floorSize / 2),
                    ["columnCenter"] = NavGridColumnCenter,
                    ["count"] = Enc(count),
                    ["linkKinds"] = new Dictionary<string, object> { ["walk"] = Enc(Link.Walk), ["jump"] = Enc(Link.Jump), ["drop"] = Enc(Link.Drop), ["gap"] = Enc(Link.Gap) },
                    ["linkCounts"] = linkCountsObj,
                    ["nodes"] = new Dictionary<string, object>
                    {
                        ["note"] = NavGridNodesNote,
                        ["x"] = Nums(count, i => ColX(col[i])), // Float64Array
                        ["z"] = Nums(count, i => ColZ(col[i])),
                        ["y"] = Nums(count, i => y[i]),
                        ["clear"] = Nums(count, i => clear[i]),
                        ["tag"] = Nums(count, i => tag[i]),
                        ["edge"] = Nums(count, i => edge[i]),
                        ["col"] = Nums(count, i => col[i]),
                        ["blocked"] = Nums(count, i => blocked[i]),
                    },
                    ["links"] = new Dictionary<string, object>
                    {
                        ["note"] = NavGridLinksNote,
                        ["linkStart"] = Nums(count + 1, i => linkStart[i]),
                        ["linkTo"] = Nums(m, i => linkTo[i]),
                        ["linkKind"] = Nums(m, i => linkKind[i]),
                        ["linkCost"] = Nums(m, i => linkCost[i]),
                    },
                    ["tagAt"] = zoned ? NavGridTagAtZones : NavGridTagAtNone,
                    ["corePreset"] = corePreset,
                };
            }
        }
    }
}
