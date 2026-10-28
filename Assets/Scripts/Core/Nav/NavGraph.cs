// Port of skygrind/src/nav/navGraph.js (NIGHT14 U3-27.4), line for line with its comments:
// Nav graph (SPEC 2.0 §6 Duel): nodes on every walkable top of the loaded map (the floor grid,
// cover, ledges, bridges, islands, spire tops, core pillar tops, pads) and edges the Mover can
// really fly. Walk edges need ground under every metre and a clear line. Every airborne edge (a
// drop, and the jump techniques below) is flown in a small simulation with the Mover's vertical
// rules (120 Hz, fall multiplier, dash-jump wind, double jump at the apex; no apex hang, so it is
// conservative) and the follower's air control (hold forward, brake once above the target), with
// head and feet spheres swept through the world collider; the landing point needs ground at the
// target's height. Built once when a Duel starts (the core is locked, so the graph stays valid).
//
// Techniques (edge kinds; the follower performs them the same way):
//   drop        walk off the edge at run speed
//   jump/double running jump (+ double jump at the apex); needs run-up ground behind the node
//   hop/climb   standing jump (+ double): start still, air-accelerate toward the target (ledge stairs)
//   dash/dashDouble  dash, jump inside the dash (dash jump), (+ double)
//   pad         stand on a pad; it launches to its target
//
// POWER P1: with the Survival nav grid (ai/navGrid.js, built on the same map) the graph also takes the grid's
// jump / gap / drop links: both ends of every such link join the nodes (thinned) and each link is flown with the
// techniques above, so RIVAL's A* runs over the same grid and the same jump links the enemies use.
//
// The port: `collider` is the world (GameWorld: the static Octree, the core, any live layers) behind collider.js's
// rayIntersect / capsuleIntersect / sphereIntersect; `core` the CoreGrid; `grid` the NavGrid. A node's pad is an index
// into map.pads (the web keeps the pad object), its box the map.boxes index. The web's string keys (the spatial hash's
// "cx,cz", the air map's group names) keep their insertion orders: the hash buckets list nodes in index order and the
// air groups are iterated in first-seen order; the per-group sort is stable (JS Array.sort). Built once (not per step).
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>What buildNavGraph reads of arena.map: boxes (min / max / kind), floorSize, pads, the core's spec.</summary>
    public sealed class NavMap
    {
        public readonly List<MapBox> boxes = new List<MapBox>();
        public double floorSize;
        public readonly List<PadSpec> pads = new List<PadSpec>();
        public CoreSpec? core;

        static Vec3 A(object o)
        {
            var a = PortData.Arr(o);
            return new Vec3(PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2]));
        }

        /// <summary>A PortExport/maps/*.json file (the map object arena.load() returned: data.boxes, floorSize, pads, core.spec).</summary>
        public static NavMap FromExport(Dictionary<string, object> mapJson)
        {
            var map = new NavMap { floorSize = PortData.Num(mapJson["floorSize"]) };
            var data = PortData.Obj(mapJson["data"]);
            foreach (object b in PortData.Arr(data["boxes"]))
            {
                var d = PortData.Obj(b);
                map.boxes.Add(new MapBox { min = A(d["min"]), max = A(d["max"]), kind = d.TryGetValue("kind", out object k) ? (string)k : "" });
            }
            foreach (object p in PortData.Arr(mapJson["pads"]))
            {
                var d = PortData.Obj(p);
                var pad = new PadSpec { pos = new Vec3(PortData.Num(d["x"]), PortData.Num(d["y"]), PortData.Num(d["z"])) };
                if (d.TryGetValue("target", out object t) && t != null)
                {
                    pad.aimed = true;
                    pad.target = PortData.V3(t);
                    pad.apexAbove = PortData.Num(d["apexAbove"]);
                }
                map.pads.Add(pad);
            }
            if (mapJson.TryGetValue("core", out object core) && core is Dictionary<string, object> c)
            {
                var s = PortData.Obj(c["spec"]);
                map.core = new CoreSpec { x0 = PortData.Num(s["x0"]), z0 = PortData.Num(s["z0"]), cells = (int)PortData.Num(s["cells"]), cellSize = PortData.Num(s["cellSize"]) };
            }
            return map;
        }

        /// <summary>The Survival map's (map.boxes, floorSize, pads, core).</summary>
        public static NavMap From(SurvivalMap m)
        {
            var map = new NavMap { floorSize = m.floorSize, core = m.core };
            map.boxes.AddRange(m.boxes);
            map.pads.AddRange(m.pads);
            return map;
        }
    }

    public sealed partial class NavGraph
    {
        /// <summary>Airborne edge kinds, cheapest first.</summary>
        public static readonly string[] FLIGHT_KINDS = { "jump", "hop", "double", "climb", "dash", "dashDouble" };

        /// <summary>stats.flights, stats.gridLinks.tried / added.</summary>
        public int statsFlights, gridTried, gridAdded;

        Builder builder;

        /// <summary>
        /// buildNavGraph(map, collider, config, core, grid): map = arena.map (boxes, floorSize, pads, core); collider = the
        /// world collider (rayIntersect, capsuleIntersect, sphereIntersect); core = the core grid, for pillar-top nodes;
        /// grid = the nav grid (POWER P1's links), or null.
        /// </summary>
        public static NavGraph Build(NavMap map, GameWorld collider, GameConfig cfg, CoreGrid core = null, NavGrid grid = null)
        {
            var b = new Builder(map, collider, cfg, core);
            var graph = new NavGraph { builder = b };
            b.Run(graph, grid);
            return graph;
        }

        /// <summary>graph.fly(a, b, kind, null, back): seconds, or 0 when the Mover can't make it (tests + debugging).</summary>
        public double Fly(NavNode a, NavNode b, string kind, double back = 0) => builder.Fly(a, b, kind, back);

        sealed class Builder
        {
            readonly NavMap map;
            readonly GameWorld collider;
            readonly CoreGrid core;
            readonly MovementConfig c;
            // config.nav
            readonly double floorSpacing, floorInset, topSpacing, topInset, probeUp, stepHeight, walkMax, maxEdge, downEdgeMax, reachMargin;
            readonly double groundSample, lineHeight, flightSphere, flightSteps, flightCheckEvery, landTolerance, fallLimit, flightsPerTop;
            readonly double floorGroup, runup, dashMin, dashJumpDelay, dropOverhead, padCost, jumpOverhead, dashPipCost, airBrakeSlack;
            readonly double gridSnap, gridLinkSpacing;
            readonly double[] standingBacks;
            readonly List<string> walkWalkers;
            // --- Flight simulation constants
            readonly double dt, g, v0, walk, airAccel, dashV, lowR, highR;
            readonly int windSteps;
            static readonly Vec3 DOWN = new Vec3(0, -1, 0);
            static readonly double[] NoBack = { 0 };

            public Builder(NavMap map, GameWorld collider, GameConfig cfg, CoreGrid core)
            {
                this.map = map;
                this.collider = collider;
                this.core = core;
                c = cfg.movement;
                floorSpacing = cfg.Num("nav.floorSpacing");
                floorInset = cfg.Num("nav.floorInset");
                topSpacing = cfg.Num("nav.topSpacing");
                topInset = cfg.Num("nav.topInset");
                probeUp = cfg.Num("nav.probeUp");
                stepHeight = cfg.Num("nav.stepHeight");
                walkMax = cfg.Num("nav.walkMax");
                maxEdge = cfg.Num("nav.maxEdge");
                downEdgeMax = cfg.Num("nav.downEdgeMax");
                reachMargin = cfg.Num("nav.reachMargin");
                groundSample = cfg.Num("nav.groundSample");
                lineHeight = cfg.Num("nav.lineHeight");
                flightSphere = cfg.Num("nav.flightSphere");
                flightSteps = cfg.Num("nav.flightSteps");
                flightCheckEvery = cfg.Num("nav.flightCheckEvery");
                landTolerance = cfg.Num("nav.landTolerance");
                fallLimit = cfg.Num("nav.fallLimit");
                flightsPerTop = cfg.Num("nav.flightsPerTop");
                floorGroup = cfg.Num("nav.floorGroup");
                runup = cfg.Num("nav.runup");
                standingBacks = cfg.Nums("nav.standingBacks");
                dashMin = cfg.Num("nav.dashMin");
                dashJumpDelay = cfg.Num("nav.dashJumpDelay");
                dropOverhead = cfg.Num("nav.dropOverhead");
                padCost = cfg.Num("nav.padCost");
                walkWalkers = cfg.Strs("nav.walkWalkers");
                jumpOverhead = cfg.Num("nav.jumpOverhead");
                dashPipCost = cfg.Num("nav.dashPipCost");
                airBrakeSlack = cfg.Num("nav.airBrakeSlack");
                gridSnap = cfg.Num("nav.gridSnap");
                gridLinkSpacing = cfg.Num("nav.gridLinkSpacing");

                dt = 1 / c.loop.simHz;
                g = MovementMath.GravityFor(c.jump.height, c.jump.apexTime);
                v0 = MovementMath.JumpVelocity(c.jump.height, c.jump.apexTime);
                walk = c.ground.walkSpeed;
                airAccel = c.air.accel * walk; // m/s², Quake air accel at full wish speed
                dashV = MovementMath.DashSpeed(walk, c.dash.speed);
                windSteps = MovementMath.StepsFor(c.dashJump.windowTime, dt);
                lowR = flightSphere + 0.05; // feet sphere centre above the feet (its bottom clears the floor)
                highR = JsMath.Max(lowR, c.body.capsuleHeight - flightSphere); // head sphere centre
            }

            /// <summary>The ground height right under (x, z) searching down from y (null when nothing within `below`).</summary>
            double? GroundAt(double x, double y, double z, double below)
            {
                bool hit = collider.RayIntersect(new Vec3(x, y, z), DOWN, out RayHit h);
                return hit && h.distance <= below ? y - h.distance : (double?)null;
            }

            /// <summary>Nothing solid on the straight line a -> b (both lifted).</summary>
            bool Clear(NavNode a, NavNode b, double liftA, double liftB)
            {
                var origin = new Vec3(a.x, a.y + liftA, a.z);
                var tmp = new Vec3(b.x - a.x, b.y + liftB - (a.y + liftA), b.z - a.z);
                double length = tmp.Length;
                if (length < 1e-6) return true;
                Vec3 direction = Vec3.DivideScalar(tmp, length);
                bool hit = collider.RayIntersect(origin, direction, out RayHit h);
                return !hit || h.distance >= length;
            }

            /// <summary>Is a body-sized sphere at (x, y, z) inside anything?</summary>
            bool SolidAt(double x, double y, double z) => collider.SphereIntersect(new Vec3(x, y, z), flightSphere, out _, out _);

            /// <summary>collider.capsuleIntersect(capsule) (truthy: any push-out).</summary>
            bool CapsuleHits(Vec3 start, Vec3 end) =>
                WorldCollider.CapsuleIntersect(collider, new Capsule { start = start, end = end, radius = c.body.capsuleRadius }, out _, out _);

            NavNode Candidate(double x, double y, double z, string kind, string group, int box = -1, int pad = -1) =>
                new NavNode { x = x, y = y, z = z, kind = kind, group = group, box = box, pad = pad };

            List<double> Along(double lo, double hi)
            {
                double size = hi - lo;
                if (size <= 2 * topInset) return new List<double> { (lo + hi) / 2 };
                int n = (int)JsMath.Max(1, JsMath.Round((size - 2 * topInset) / topSpacing));
                var list = new List<double>(n + 1);
                for (int i = 0; i <= n; i++) list.Add(lo + topInset + (i * (size - 2 * topInset)) / n);
                return list;
            }

            public void Run(NavGraph graph, NavGrid grid)
            {
                List<NavNode> nodes = graph.nodes;
                // --- Nodes ------------------------------------------------------------------------------------------
                var candidates = new List<NavNode>();
                double half = map.floorSize / 2 - floorInset;
                for (double x = -half; x <= half + 1e-6; x += floorSpacing)
                {
                    for (double z = -half; z <= half + 1e-6; z += floorSpacing) candidates.Add(Candidate(x, 0, z, "floor", "floor"));
                }
                for (int i = 0; i < map.boxes.Count; i++)
                {
                    MapBox b = map.boxes[i];
                    if (!walkWalkers.Contains(b.kind)) continue;
                    // sampleTop(b, b.kind, i)
                    foreach (double x in Along(b.min.x, b.max.x))
                    {
                        foreach (double z in Along(b.min.z, b.max.z)) candidates.Add(Candidate(x, b.max.y, z, b.kind, "box" + i, i));
                    }
                }
                if (map.core.HasValue && core != null)
                {
                    CoreSpec spec = map.core.Value;
                    double x0 = spec.x0, z0 = spec.z0, cellSize = spec.cellSize;
                    int cells = spec.cells;
                    for (int i = 0; i < cells * cells; i++)
                    {
                        double h = core.Height(i);
                        if (h <= c.core.minHeight) continue;
                        double x = x0 + ((i % cells) + 0.5) * cellSize;
                        double z = z0 + (Math.Floor((double)i / cells) + 0.5) * cellSize;
                        candidates.Add(Candidate(x, h, z, "pillar", "pillar" + i));
                    }
                }
                for (int i = 0; i < map.pads.Count; i++)
                {
                    PadSpec p = map.pads[i];
                    candidates.Add(Candidate(p.pos.x, p.pos.y, p.pos.z, "pad", "pad" + i, -1, i));
                }

                // Rays and capsule tests only see surfaces, so a point buried in a box or a pillar looks clear.
                double body = c.body.capsuleHeight;
                bool Buried(NavNode p)
                {
                    for (int i = 0; i < map.boxes.Count; i++)
                    {
                        MapBox b = map.boxes[i];
                        if (p.x > b.min.x && p.x < b.max.x && p.z > b.min.z && p.z < b.max.z && b.max.y > p.y + 0.05 && b.min.y < p.y + body) return true;
                    }
                    if (core == null) return false;
                    double? top = core.TopAt(p.x, p.z);
                    return (top ?? double.NegativeInfinity) > p.y + 0.05;
                }

                foreach (NavNode p in candidates)
                {
                    if (Buried(p)) continue;
                    double? top = GroundAt(p.x, p.y + probeUp, p.z, probeUp + 0.3);
                    if (top == null || Math.Abs(top.Value - p.y) > 0.1) continue; // covered, or not actually a top
                    double t = top.Value;
                    if (CapsuleHits(new Vec3(p.x, t + c.body.capsuleRadius + 0.05, p.z), new Vec3(p.x, t + c.body.capsuleHeight - c.body.capsuleRadius, p.z))) continue; // no room to stand
                    p.y = t; // { ...p, y: top }
                    nodes.Add(p);
                }

                // --- The nav grid's links (POWER P1) ------------------------------------------------------------------
                var gridPairs = new List<(int, int)>();
                if (grid != null)
                {
                    NavGrid s = grid;
                    int Near(double x, double y, double z, double r)
                    {
                        for (int i = 0; i < nodes.Count; i++)
                        {
                            NavNode q = nodes[i];
                            if (Math.Abs(q.y - y) < 0.3 && JsMath.Hypot(q.x - x, q.z - z) < r) return i;
                        }
                        return -1;
                    }
                    // The graph node for a grid node: an existing one close by, else a new 'link' node there.
                    int NodeFor(int n)
                    {
                        double x = s.NodeX(n);
                        double y = s.y[n];
                        double z = s.NodeZ(n);
                        int found = Near(x, y, z, gridSnap);
                        if (found >= 0) return found;
                        if (CapsuleHits(new Vec3(x, y + c.body.capsuleRadius + 0.05, z), new Vec3(x, y + c.body.capsuleHeight - c.body.capsuleRadius, z))) return -1;
                        nodes.Add(new NavNode { x = x, y = y, z = z, kind = "link", group = "link" + nodes.Count });
                        return nodes.Count - 1;
                    }
                    var taken = new List<(int kind, double x, double y, double z, double dy)>();
                    for (int a = 0; a < s.count; a++)
                    {
                        for (int l = s.linkStart[a]; l < s.linkStart[a + 1]; l++)
                        {
                            if (s.linkKind[l] != 1 && s.linkKind[l] != 3) continue; // jump + gap links (walks + drops: the graph's own)
                            int b = s.linkTo[l];
                            double ax = s.NodeX(a);
                            double az = s.NodeZ(a);
                            double ya = s.y[a], yb = s.y[b];
                            // Thin a long ledge's row of identical links to one every few metres.
                            bool thin = false;
                            for (int q = 0; q < taken.Count && !thin; q++)
                            {
                                var t = taken[q];
                                thin = t.kind == s.linkKind[l] && Math.Abs(t.y - ya) < 0.3 && Math.Abs(t.dy - (yb - ya)) < 0.3 && JsMath.Hypot(t.x - ax, t.z - az) < gridLinkSpacing;
                            }
                            if (thin) continue;
                            taken.Add((s.linkKind[l], ax, ya, az, yb - ya));
                            int i = NodeFor(a);
                            int j = NodeFor(b);
                            if (i >= 0 && j >= 0 && i != j) gridPairs.Add((i, j));
                        }
                    }
                }

                // --- Edges ------------------------------------------------------------------------------------------
                var edges = graph.edges;
                for (int i = 0; i < nodes.Count; i++) edges.Add(new List<NavEdge>());
                double cell = maxEdge;
                var hash = new Dictionary<(double, double), List<int>>();
                for (int i = 0; i < nodes.Count; i++)
                {
                    NavNode n = nodes[i];
                    var key = (Floor0(n.x / cell), Floor0(n.z / cell));
                    if (!hash.TryGetValue(key, out List<int> bucket)) hash[key] = bucket = new List<int>();
                    bucket.Add(i);
                }

                // groupOf(n): a floor node's floorGroup-sized square, else its own group (as a small id; first-seen order kept)
                var groupIds = new Dictionary<string, int>();
                var groupOf = new int[nodes.Count];
                for (int i = 0; i < nodes.Count; i++)
                {
                    NavNode n = nodes[i];
                    string name = n.kind == "floor" ? "floor" + Fmt(Math.Floor(n.x / floorGroup)) + "," + Fmt(Math.Floor(n.z / floorGroup)) : n.group;
                    if (!groupIds.TryGetValue(name, out int id)) groupIds[name] = id = groupIds.Count;
                    groupOf[i] = id;
                }
                var airOrder = new List<int>(); // the air map's keys in insertion order
                var airLists = new Dictionary<int, List<(int j, double dh)>>();
                var pool = new List<List<(int j, double dh)>>();
                int poolUsed;

                for (int i = 0; i < nodes.Count; i++)
                {
                    NavNode a = nodes[i];
                    double cx = Math.Floor(a.x / cell), cz = Math.Floor(a.z / cell);
                    airOrder.Clear(); // const air = new Map(); group -> nearest airborne candidates
                    airLists.Clear();
                    poolUsed = 0;
                    for (double gx = cx - 1; gx <= cx + 1; gx++)
                    {
                        for (double gz = cz - 1; gz <= cz + 1; gz++)
                        {
                            if (!hash.TryGetValue((Zero(gx), Zero(gz)), out List<int> bucket)) continue;
                            foreach (int j in bucket)
                            {
                                if (j == i) continue;
                                NavNode b = nodes[j];
                                double dh = JsMath.Hypot(b.x - a.x, b.z - a.z);
                                if (dh > maxEdge || dh < 0.5) continue;
                                if (a.kind == "floor" && b.kind == "floor" && dh > walkMax) continue; // floor goes node by node
                                if (Walkable(a, b, dh))
                                {
                                    edges[i].Add(new NavEdge { to = j, cost = dh / walk, kind = "walk" });
                                    continue;
                                }
                                if (a.group == b.group && a.kind != "floor") continue; // same top: walk edges cover it
                                if (a.kind == "link" || b.kind == "link") continue; // grid-link nodes: walk edges + their own link (below)
                                int grp = groupOf[j];
                                if (!airLists.TryGetValue(grp, out var list))
                                {
                                    if (poolUsed == pool.Count) pool.Add(new List<(int j, double dh)>());
                                    list = pool[poolUsed++];
                                    list.Clear();
                                    airLists[grp] = list;
                                    airOrder.Add(grp);
                                }
                                list.Add((j, dh));
                            }
                        }
                    }
                    // Airborne edges: only the nearest few nodes of each destination top are flown.
                    foreach (int grp in airOrder)
                    {
                        List<(int j, double dh)> list = airLists[grp];
                        StableSortByDh(list); // list.sort((p, q) => p.dh - q.dh)
                        int take = (int)JsMath.Min(list.Count, flightsPerTop); // list.slice(0, nv.flightsPerTop)
                        for (int q = 0; q < take; q++)
                        {
                            int j = list[q].j;
                            double dh = list[q].dh;
                            NavEdge best = Flights(a, nodes[j], j, dh, graph, down: true);
                            if (best != null) edges[i].Add(best);
                        }
                    }
                }

                // The grid's links (POWER P1): each one the Mover can fly (or walk off) becomes an edge, if there isn't one.
                graph.gridTried = gridPairs.Count;
                foreach (var (i, j) in gridPairs)
                {
                    bool has = false;
                    foreach (NavEdge e in edges[i]) has |= e.to == j;
                    if (has) continue;
                    NavNode a = nodes[i];
                    NavNode b = nodes[j];
                    double dh = JsMath.Hypot(b.x - a.x, b.z - a.z);
                    NavEdge best = Flights(a, b, j, dh, graph, down: false);
                    if (best != null)
                    {
                        edges[i].Add(best);
                        graph.gridAdded++;
                    }
                }
                // (link nodes joined `nodes` before the main loop, so they have walk edges too)

                // Pads: from the pad node straight to the node nearest its landing target (pads aim from where
                // you stand, so any entry lands there).
                for (int i = 0; i < nodes.Count; i++)
                {
                    NavNode n = nodes[i];
                    if (n.kind != "pad" || !map.pads[n.pad].aimed) continue;
                    Vec3 t = map.pads[n.pad].target;
                    int best = -1;
                    double bestD = double.PositiveInfinity;
                    for (int j = 0; j < nodes.Count; j++)
                    {
                        NavNode m = nodes[j];
                        double d = JsMath.Hypot(m.x - t.x, 2 * (m.y - t.y), m.z - t.z);
                        if (d < bestD)
                        {
                            bestD = d;
                            best = j;
                        }
                    }
                    if (best >= 0)
                    {
                        edges[i] = edges[i].FindAll(e => e.kind == "walk" || e.kind == "drop");
                        edges[i].Add(new NavEdge { to = best, cost = padCost, kind = "pad" });
                    }
                }
            }

            /// <summary>
            /// The airborne techniques for a -> b, cheapest first (the main loop's and the grid links' shared body): a drop
            /// when b is lower, then the first FLIGHT_KINDS technique that flies. `down`: the main loop's long-fall skip.
            /// </summary>
            NavEdge Flights(NavNode a, NavNode b, int j, double dh, NavGraph graph, bool down)
            {
                double dy = b.y - a.y;
                bool lower = dy < -stepHeight;
                if (down && lower && dh > downEdgeMax) return null; // long falls: there are plenty of shorter ones
                NavEdge best = null;
                if (lower)
                {
                    graph.statsFlights++;
                    double t = Fly(a, b, "drop");
                    if (t != 0 && !double.IsNaN(t)) best = new NavEdge { to = j, cost = t + dropOverhead, kind = "drop" };
                }
                foreach (string k in FLIGHT_KINDS)
                {
                    if (best != null) break;
                    if (Follower.IsDashing(k) && dh < dashMin) continue;
                    if (dh > Reach.JumpReach(ReachKind(k), dy, c) * reachMargin) continue;
                    if (Follower.IsRunning(k) && !HasRunup(a, b)) continue;
                    foreach (double back in Follower.IsStanding(k) ? standingBacks : NoBack)
                    {
                        if (back > 0 && !RoomBehind(a, b, back)) break;
                        graph.statsFlights++;
                        double t = Fly(a, b, k, back);
                        if (t == 0 || double.IsNaN(t)) continue; // if (!t) continue
                        best = new NavEdge { to = j, cost = t + jumpOverhead * Overhead(k) + (Follower.IsDashing(k) ? dashPipCost : 0), kind = k };
                        if (back > 0) best.back = back;
                        break;
                    }
                }
                return best;
            }

            static string ReachKind(string k) => Follower.IsDashing(k) ? k : Follower.IsDouble(k) ? "double" : "jump";

            // const overhead = { jump: 1, hop: 1.5, double: 2, climb: 2.5, dash: 2, dashDouble: 3 };
            static double Overhead(string k)
            {
                switch (k)
                {
                    case "jump": return 1;
                    case "hop": return 1.5;
                    case "double": return 2;
                    case "climb": return 2.5;
                    case "dash": return 2;
                    case "dashDouble": return 3;
                }
                return double.NaN;
            }

            /// <summary>A walk edge: gentle height change and ground under every sample point.</summary>
            bool Walkable(NavNode a, NavNode b, double dh)
            {
                if (Math.Abs(b.y - a.y) > stepHeight || dh > walkMax) return false;
                double samples = JsMath.Max(1, Math.Ceiling(dh / groundSample));
                for (double s = 1; s < samples; s++)
                {
                    double t = s / samples;
                    double y = a.y + (b.y - a.y) * t;
                    double? ground = GroundAt(a.x + (b.x - a.x) * t, y + 0.6, a.z + (b.z - a.z) * t, 0.6 + stepHeight);
                    if (ground == null) return false;
                }
                return Clear(a, b, lineHeight, lineHeight);
            }

            /// <summary>
            /// Fly a → b with one technique, taking off `back` m behind a (standing techniques back off to
            /// get the height before the target's edge). Returns the flight time (s) or 0 when it can't make
            /// it. The body travels the straight line a → b in plan (the follower steers onto it). Gravity is
            /// the Mover's own (jump held: apex hang; fall multiplier; dash-jump wind).
            /// </summary>
            public double Fly(NavNode a, NavNode b, string kind, double back = 0)
            {
                double span = JsMath.Hypot(b.x - a.x, b.z - a.z);
                double ux = (b.x - a.x) / span;
                double uz = (b.z - a.z) / span;
                double sx = a.x - ux * back;
                double sz = a.z - uz * back;
                double dxz = span + back;
                double dy = b.y - a.y;
                bool dash = Follower.IsDashing(kind);
                double v = Follower.IsStanding(kind) ? 0 : dash ? dashV : walk; // horizontal speed along the line
                double vy = kind == "drop" ? 0 : dash ? v0 * c.dashJump.v0Mult : v0;
                int wind = dash ? windSteps : 0;
                int doubleLeft = Follower.IsDouble(kind) ? 1 : 0;
                double h = dash ? dashV * dt * dashJumpDelay : 0; // a dash jump takes off a few steps into the dash
                if (kind == "drop")
                {
                    // Walk to the edge first: the fall starts where the source top ends.
                    while (h < dxz && GroundAt(sx + ux * h, a.y + 0.6, sz + uz * h, 0.6 + stepHeight) != null) h += groundSample / 4;
                    h += c.body.capsuleRadius;
                }
                double y = 0;
                var flags = new GravityFlags();
                for (int i = 1; i < flightSteps; i++)
                {
                    if (doubleLeft != 0 && vy <= 0)
                    {
                        vy = v0 * c.jump.doubleJumpMult;
                        doubleLeft = 0;
                    }
                    flags.jumpHeld = flags.jumpAirtime = kind != "drop";
                    flags.windActive = wind > 0;
                    double mult = MovementMath.GravityScale(MoveState.Air, vy, flags, c);
                    if (wind > 0) wind--;
                    VerticalStepResult vstep = MovementMath.VerticalStep(vy, g * mult, dt);
                    vy = vstep.vy;
                    y += vstep.dy;
                    // Air control like the follower: brake once above the target and about to overshoot, else
                    // accelerate toward it (standing techniques start still).
                    double rest = dxz - h;
                    bool above = y >= dy - 0.1;
                    if (above && (rest < 0.4 || (v > 0 && (v * v) / (2 * airAccel) > rest + airBrakeSlack))) v = JsMath.Max(-walk, v - airAccel * dt);
                    else if (v < walk) v = JsMath.Min(walk, v + airAccel * dt);
                    h += v * dt;
                    double px = sx + ux * h;
                    double pz = sz + uz * h;
                    if (vy < 0 && y <= dy && doubleLeft == 0)
                    {
                        // Landing: ground must be right there at the target's height.
                        double? ground = GroundAt(px, b.y + 0.6, pz, 0.6 + landTolerance);
                        return ground != null && Math.Abs(ground.Value - b.y) <= landTolerance ? i * dt : 0;
                    }
                    if (y < dy - fallLimit) return 0;
                    if (i % flightCheckEvery == 0 && (SolidAt(px, a.y + y + lowR, pz) || SolidAt(px, a.y + y + highR, pz))) return 0;
                }
                return 0;
            }

            /// <summary>Standing room on a's top `back` m behind a (away from b), with ground all the way.</summary>
            bool RoomBehind(NavNode a, NavNode b, double back)
            {
                double d = JsMath.Hypot(b.x - a.x, b.z - a.z);
                for (int k = 0; k < 2; k++)
                {
                    double f = k == 0 ? 0.5 : 1;
                    double rx = a.x - ((b.x - a.x) / d) * back * f;
                    double rz = a.z - ((b.z - a.z) / d) * back * f;
                    double? ground = GroundAt(rx, a.y + 0.6, rz, 0.6 + stepHeight);
                    if (ground == null || Math.Abs(ground.Value - a.y) > stepHeight || SolidAt(rx, a.y + lowR + 0.05, rz)) return false;
                }
                return true;
            }

            /// <summary>Running techniques need a short run-up on the same top behind the node.</summary>
            bool HasRunup(NavNode a, NavNode b) => RoomBehind(a, b, runup);

            /// <summary>Math.floor(v) as a hash key (-0 and 0 are one key, as `${-0}` is "0").</summary>
            static double Floor0(double v) => Zero(Math.Floor(v));

            static double Zero(double v) => v == 0 ? 0 : v;

            static string Fmt(double v) => Zero(v).ToString("R", System.Globalization.CultureInfo.InvariantCulture);

            /// <summary>Insertion sort by dh: stable, as JS Array.sort.</summary>
            static void StableSortByDh(List<(int j, double dh)> list)
            {
                for (int i = 1; i < list.Count; i++)
                {
                    var item = list[i];
                    int k = i - 1;
                    while (k >= 0 && list[k].dh - item.dh > 0)
                    {
                        list[k + 1] = list[k];
                        k--;
                    }
                    list[k + 1] = item;
                }
            }
        }
    }
}
