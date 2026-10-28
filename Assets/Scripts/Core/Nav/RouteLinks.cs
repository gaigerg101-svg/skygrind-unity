// NIGHT20 U3-34.6 (builder N): enemies reach every floor and get back down (GG: "the enemy AI struggles to go above floors
// and can't really get out of floors"; Research/2026-10-06-enemy-routes.md). design.routes, port-only (the web's grid has
// walk, jump <= 2.2 m, drop <= 5 m and gap links only, POWER.md P1; its answer to height was the DRONE and the LANCER):
// - RouteLayer: a Unity-side extra-link layer over the web's nav grid (NavField.cs). The web's grid export and the
//   flowField golden vectors stay untouched: the layer is built at a run's start from the export (y, clear, col) and the
//   map's collider, and Apply swaps the grid's CSR arrays between the web's own (off) and the layer's (on: every web link
//   in its order, then the layer's links per node). Its kinds (Link 4-7, per-kind masks in Link.Mask*):
//     LONG DROP (4) every walker: an orthogonal neighbour column 5-12 m down (longDrop; L4D's DeathDrop is ~10 m) whose
//       column is clear to the ledge's height plus a body, with nothing between the two columns at the ledge; cost
//       1 + speed x (the fall + dropStagger), landing in a dropStagger s stand (RouteJumper).
//     CLIMB (5) husks, spitters, swarmers; CLIMB HIGH (6) swarmers only; AUTHORED (7) the map's ladders (U3-34.6 steps
//       3-4, the next slices).
// - The reach (no dead perches, step 5): which nodes a ground husk reaches (and a CHARGER, without climbs), from the
//   ground nodes (y <= groundY) over the usable links, recomputed when a gate opens; NavField seeds the nearest reached
//   floor under a perch nobody reaches, so the director always has a spawn with a route and husks gather under you.
// - RouteJumper: the NavJumper the husk pool runs with the layer on: the web's nav jumps (HuskNavJumper, inner) plus
//   the fixed leap (step 2: a DANGER crouch of leapTell, an arc swept through the collider before it launches, at most
//   leapMaxRise up, from an approach point leapApproachMin-Max m out when the wall's foot has no clear arc; no leap into a
//   ceiling) and the long drop's landing stagger. Every phase change emits N20Events.Climb (ClimbEvent: "start", "up",
//   "top", "knocked", "land"; linkKind Link.Jump for a leap, Link.LongDrop for a landing) for GG's sounds and the views.
// A load-time build (it allocates); the per-step paths (the jumper, the reach on a gate, the seeds) allocate nothing.
using System;
using System.Collections.Generic;

namespace Skygrind.Core
{
    /// <summary>design.routes (Design/unity-config.json), with the web's numbers it builds on (husk.speed, ai.grid.*).</summary>
    public sealed class RouteParams
    {
        public double longDrop, dropStagger, climbMax, swarmerClimbMax, climbSpeed, swarmerClimbSpeed, climbTell, swarmerClimbTell, mantle;
        public double leapTell, leapMaxRise, leapApproachMin, leapApproachMax, leapRetry, leapApproachTime, leapMaxHorizontal;
        public double groundY, seedFallbackRadius, seedSpread;
        /// <summary>The web's: husk.speed and the swarmer's (the costs' m-equivalents), the husks' gravity, ai.grid.drop / jumpUp /
        /// bodyHeight / step / riserLift, surface.wallMaxAbsNormalY, husk.leapCooldown and stopDistance.</summary>
        public double speed, swarmerSpeed, gravity, drop, jumpUp, bodyHeight, step, riserLift, wallMaxAbsNormalY, leapCooldown, stopDistance;
        /// <summary>The bodies a climb is flown with at build: the larger of a husk's and a spitter's (CLIMB), a swarmer's (CLIMB HIGH).</summary>
        public double climbRadius, climbHeight, swarmRadius, swarmHeight;
        /// <summary>design.routes leapUpMax, ladderSpeed, ladderMax; the web's ai.grid.jumpCost, ai.jumps.husk.apex, ai.jumpLandIn.</summary>
        public double leapUpMax, ladderSpeed, ladderMax, jumpCost, jumpApex, landIn;
        /// <summary>design.routes leapUpCells: the columns a leap up may cross (1: next door).</summary>
        public int leapUpCells;

        public static RouteParams From(GameConfig cfg)
        {
            double R(string k) => cfg.Num("design.routes." + k);
            return new RouteParams
            {
                longDrop = R("longDrop"), dropStagger = R("dropStagger"), climbMax = R("climbMax"), swarmerClimbMax = R("swarmerClimbMax"),
                climbSpeed = R("climbSpeed"), swarmerClimbSpeed = R("swarmerClimbSpeed"), climbTell = R("climbTell"), swarmerClimbTell = R("swarmerClimbTell"),
                mantle = R("mantle"), leapTell = R("leapTell"), leapMaxRise = R("leapMaxRise"), leapApproachMin = R("leapApproachMin"),
                leapApproachMax = R("leapApproachMax"), leapRetry = R("leapRetry"), leapApproachTime = R("leapApproachTime"), groundY = R("groundY"),
                seedFallbackRadius = R("seedFallbackRadius"), seedSpread = R("seedSpread"),
                speed = cfg.Num("husk.speed"), swarmerSpeed = cfg.Num("swarmer.speed"),
                gravity = MovementMath.GravityFor(cfg.movement.jump.height, cfg.movement.jump.apexTime),
                drop = cfg.Num("ai.grid.drop"), jumpUp = cfg.Num("ai.grid.jumpUp"), bodyHeight = cfg.Num("ai.grid.bodyHeight"), step = cfg.Num("ai.grid.step"),
                riserLift = cfg.Num("ai.grid.riserLift"), wallMaxAbsNormalY = cfg.Num("surface.wallMaxAbsNormalY"),
                leapCooldown = cfg.Num("husk.leapCooldown"), stopDistance = cfg.Num("husk.stopDistance"), leapMaxHorizontal = cfg.Num("husk.leapMaxHorizontal"),
                climbRadius = Math.Max(cfg.Num("husk.radius"), cfg.Num("spitter.radius")), climbHeight = Math.Max(cfg.Num("husk.height"), cfg.Num("spitter.height")),
                swarmRadius = cfg.Num("swarmer.radius"), swarmHeight = cfg.Num("swarmer.height"),
                leapUpMax = R("leapUpMax"), ladderSpeed = R("ladderSpeed"), ladderMax = R("ladderMax"),
                leapUpCells = cfg.Has("design.routes.leapUpCells") ? (int)R("leapUpCells") : 1,
                jumpCost = cfg.Num("ai.grid.jumpCost"), jumpApex = cfg.Num("ai.jumps.husk.apex"), landIn = cfg.Num("ai.jumpLandIn"),
            };
        }
    }

    /// <summary>The layer's link costs, in the grid's m-equivalents: 1 + the kind's run speed x the link's seconds.</summary>
    public static class RouteCost
    {
        /// <summary>A long drop of `fall` m: the fall under the husks' gravity, then the landing stagger (10 m: 8.7).</summary>
        public static double LongDrop(double fall, RouteParams p) => 1 + p.speed * (Math.Sqrt(2 * fall / p.gravity) + p.dropStagger);

        /// <summary>An authored ladder of `rise` m: the climb's tell and mantle, up at ladderSpeed (a 24 m spire: 33.1).</summary>
        public static double Ladder(double rise, RouteParams p) => 1 + p.speed * (p.climbTell + rise / p.ladderSpeed + p.mantle);

        /// <summary>A leap up `rise` m: the web's jump cost (navGrid.js: dist + jumpCost + dy, one cell).</summary>
        public static double LeapUp(double rise, RouteParams p) => 1 + p.jumpCost + rise;

        /// <summary>A leap link (up, down, authored) as the layer flies it: 1 + speed x (the leapTell crouch + the arc's flight
        /// from `from` to `land`, ai.jumps.husk.apex over the higher end, the husks' gravity).</summary>
        public static double Leap(Vec3 from, Vec3 land, RouteParams p)
        {
            CombatMath.LeapVelocity(from, land, p.jumpApex, p.gravity, out double time);
            return 1 + p.speed * (p.leapTell + time);
        }

        /// <summary>A climb of `rise` m: the tell, the climb, the mantle (a husk's 5 m: 14.3); `swarmer`: its own tell, speed and run.</summary>
        public static double Climb(double rise, RouteParams p, bool swarmer) => swarmer
            ? 1 + p.swarmerSpeed * (p.swarmerClimbTell + rise / p.swarmerClimbSpeed + p.mantle)
            : 1 + p.speed * (p.climbTell + rise / p.climbSpeed + p.mantle);
    }

    /// <summary>One link of the layer: its ends, kind, cost; a climb's foot (on the lower floor at the face), lip (the top's
    /// node) and the face's outward normal (horizontal, unit), and whether a map authored it.</summary>
    public struct RouteLink
    {
        public int from, to;
        public byte kind;
        public float cost;
        public double rise;
        public Vec3 foot, lip, normal;
        /// <summary>A leap's landing (the nav jump's aim: ai.jumpLandIn past the upper node, or its centre on a narrow floor).</summary>
        public Vec3 land;
        public bool authored;
    }

    /// <summary>An authored link (design.routes.authored.&lt;map&gt;): its two ends (snapped to the grid's nodes), its verb and why.</summary>
    public struct AuthoredLink
    {
        public Vec3 a, b;
        public string verb, why;
    }

    public sealed class RouteLayer
    {
        /// <summary>Authored entries the build couldn't place (no node at an end, no face, the body doesn't fit).</summary>
        public readonly List<string> rejected = new List<string>();

        /// <summary>Build with an explicit list of authored links (tests; the game reads design.routes.authored.&lt;map&gt;).</summary>
        public static RouteLayer Build(GameConfig cfg, NavGrid grid, Dictionary<string, object> gridJson, ICollisionWorld world, string mapName, List<AuthoredLink> authored)
        {
            var layer = new RouteLayer(RouteParams.From(cfg), mapName);
            layer.Generate(grid, gridJson, world, authored);
            layer.Pack(grid);
            return layer;
        }

        /// <summary>design.routes.authored.&lt;map&gt;: [{a: [x, y, z], b: [x, y, z], verb: "ladder", why}] (none: an empty list).</summary>
        public static List<AuthoredLink> AuthoredOf(GameConfig cfg, string mapName)
        {
            var list = new List<AuthoredLink>();
            string key = "design.routes.authored." + mapName;
            if (mapName == null || !cfg.Has(key)) return list;
            foreach (object o in cfg.Arr(key))
            {
                var d = PortData.Obj(o);
                List<object> a = PortData.Arr(d["a"]), b = PortData.Arr(d["b"]);
                list.Add(new AuthoredLink
                {
                    a = new Vec3(PortData.Num(a[0]), PortData.Num(a[1]), PortData.Num(a[2])),
                    b = new Vec3(PortData.Num(b[0]), PortData.Num(b[1]), PortData.Num(b[2])),
                    verb = d.TryGetValue("verb", out object v) ? v as string : "ladder",
                    why = d.TryGetValue("why", out object w) ? w as string : "",
                });
            }
            return list;
        }

        public readonly RouteParams p;
        public readonly string mapName;
        /// <summary>The layer's links (build order: by source node, then kind order within a node).</summary>
        public readonly List<RouteLink> links = new List<RouteLink>();
        /// <summary>Links per kind in the layer (index = Link kind).</summary>
        public readonly int[] counts = new int[16];
        /// <summary>The on arrays' link index -> its index in `links`, or -1 for a web link.</summary>
        public int[] extraOf;
        public bool On { get; private set; }
        /// <summary>Nodes a ground walker reaches (1) over the jumper's / the walker's links, the current blocked mask.</summary>
        public byte[] reachJump, reachWalk;
        /// <summary>Nodes a ground walker reaches on the web's own verbs (Link.MaskWebJumper): the rest are perches.</summary>
        public byte[] reachWeb;

        int[] webLinkStart, webLinkTo, webRevStart, webRevFrom, webRevLink;
        byte[] webLinkKind;
        float[] webLinkCost;
        int[] onLinkStart, onLinkTo, onRevStart, onRevFrom, onRevLink;
        byte[] onLinkKind;
        float[] onLinkCost;
        int[] queue;

        static readonly int[,] SIDES = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
        static readonly Vec3 DOWN = new Vec3(0, -1, 0);
        const double EPS_X = 0.0137, EPS_Z = 0.0071; // NavGridBuilder's: probe just off the cell centre

        RouteLayer(RouteParams p, string mapName)
        {
            this.p = p;
            this.mapName = mapName;
        }

        /// <summary>
        /// Build the layer over `grid` (the web's export, as NavGrid.FromExport read it, with no layer applied): `gridJson` is
        /// the same export (its nodes' `clear`: the headroom over each floor), `world` the map's collider (the static map and
        /// the core, as the grid was built), `mapName` the map's name (its authored links, design.routes.authored.&lt;name&gt;).
        /// </summary>
        public static RouteLayer Build(GameConfig cfg, NavGrid grid, Dictionary<string, object> gridJson, ICollisionWorld world, string mapName) =>
            Build(cfg, grid, gridJson, world, mapName, AuthoredOf(cfg, mapName));

        double[] clear;

        /// <summary>
        /// AUTHORED (U3-34.6 step 4): a map's ladder from node a (snapped within 0.6 m of `a`) up to the next column's node b
        /// (within 0.6 m of `b`), validated as a climb (ClimbFace, headroom over a) up to ladderMax; both ways: up at
        /// ladderSpeed (RouteCost.Ladder), down as a walk-off (RouteCost.LongDrop: the fall and the landing stagger).
        /// </summary>
        void Author(NavGrid g, ICollisionWorld world, AuthoredLink e, Dictionary<int, List<RouteLink>> byFrom)
        {
            int na = NodeNear(g, e.a), nb = NodeNear(g, e.b);
            string name = string.IsNullOrEmpty(e.why) ? $"({e.a.x}, {e.a.y}, {e.a.z}) -> ({e.b.x}, {e.b.y}, {e.b.z})" : e.why;
            if (na < 0 || nb < 0)
            {
                rejected.Add($"{name}: no grid node at {(na < 0 ? "a" : "b")}");
                return;
            }
            if (e.verb == "leap")
            {
                AuthorLeap(g, world, na, nb, name, byFrom);
                return;
            }
            int ca = g.col[na], cb = g.col[nb];
            int d = -1;
            for (int k = 0; k < 4; k++) if (ca % g.size + SIDES[k, 0] == cb % g.size && ca / g.size + SIDES[k, 1] == cb / g.size) d = k;
            double rise = (double)g.y[nb] - g.y[na];
            if (d < 0 || rise <= 0 || rise > p.ladderMax)
            {
                rejected.Add($"{name}: the ends aren't next-door columns going up <= ladderMax (rise {rise:0.00})");
                return;
            }
            if (clear[na] < rise + p.bodyHeight || !ClimbFace(g, world, na, d, cb, g.y[na], g.y[nb], p.climbRadius, p.climbHeight, out Vec3 foot))
            {
                rejected.Add($"{name}: no climbable face (headroom {clear[na]:0.0} m, rise {rise:0.0} m)");
                return;
            }
            Vec3 lip = new Vec3(g.NodeX(nb), g.y[nb], g.NodeZ(nb)), normal = new Vec3(-SIDES[d, 0], 0, -SIDES[d, 1]);
            Add(byFrom, new RouteLink { from = na, to = nb, kind = Link.Authored, cost = (float)RouteCost.Ladder(rise, p), rise = rise, foot = foot, lip = lip, normal = normal, authored = true });
            Add(byFrom, new RouteLink { from = nb, to = na, kind = Link.Authored, cost = (float)RouteCost.LongDrop(rise, p), rise = -rise, foot = lip, lip = new Vec3(g.NodeX(na), g.y[na], g.NodeZ(na)), normal = normal, authored = true });
        }

        /// <summary>
        /// An authored LEAP (verb "leap"): a nav jump from node a to node b anywhere within husk.leapMaxHorizontal and
        /// leapMaxRise (the floating islands and the tower tops a ladder can't reach: there's no face under them), its arc
        /// flown at load (RouteJumper.ArcClear; the landing ai.jumpLandIn past b, or b's centre on a narrow floor), kind LEAP
        /// UP (8: husks, spitters, swarmers); and back from b to a when that arc clears too.
        /// </summary>
        void AuthorLeap(NavGrid g, ICollisionWorld world, int na, int nb, string name, Dictionary<int, List<RouteLink>> byFrom)
        {
            bool there = LeapArc(g, world, na, nb, out double horiz, out Vec3 landB), back = LeapArc(g, world, nb, na, out _, out Vec3 landA);
            double rise = (double)g.y[nb] - g.y[na];
            if (!there || horiz > p.leapMaxHorizontal || rise > p.leapMaxRise)
            {
                rejected.Add($"{name}: no clear leap (rise {rise:0.0} m, {horiz:0.0} m across)");
                return;
            }
            Vec3 pa = new Vec3(g.NodeX(na), g.y[na], g.NodeZ(na)), pb = new Vec3(g.NodeX(nb), g.y[nb], g.NodeZ(nb));
            Add(byFrom, new RouteLink { from = na, to = nb, kind = Link.LeapUp, cost = (float)RouteCost.Leap(pa, landB, p), rise = rise, foot = pa, lip = pb, land = landB, authored = true });
            if (back && -rise <= p.leapMaxRise) Add(byFrom, new RouteLink { from = nb, to = na, kind = Link.LeapUp, cost = (float)RouteCost.Leap(pb, landA, p), rise = -rise, foot = pb, lip = pa, land = landA, authored = true });
        }

        /// <summary>The nav jump's arc from node a to node b clears (landing ai.jumpLandIn past b, or b's centre).</summary>
        bool LeapArc(NavGrid g, ICollisionWorld world, int a, int b, out double horiz, out Vec3 land)
        {
            var from = new Vec3(g.NodeX(a), g.y[a], g.NodeZ(a));
            var centre = new Vec3(g.NodeX(b), g.y[b], g.NodeZ(b));
            double dx = centre.x - from.x, dz = centre.z - from.z;
            horiz = JsMath.Hypot(dx, dz);
            land = horiz > 0 ? new Vec3(centre.x + dx / horiz * p.landIn, centre.y, centre.z + dz / horiz * p.landIn) : centre;
            if (!FloorAt(world, land, g.y[b])) land = centre;
            return FloorAt(world, land, g.y[b]) && RouteJumper.ArcClear(world, from, land, p.jumpApex, p.gravity, p.climbRadius, p.climbHeight);
        }

        /// <summary>The node in `at`'s column within 0.6 m of its height, locked zones or not (NavGrid.NodeAt skips the
        /// blocked: the layer is built at a run's start, behind the gates).</summary>
        static int NodeNear(NavGrid g, Vec3 at)
        {
            int c = g.ColOf(at.x, at.z);
            if (c < 0) return -1;
            int best = -1;
            for (int n = g.colStart[c]; n < g.colStart[c + 1]; n++) if (Math.Abs(g.y[n] - at.y) <= 0.6) best = n;
            return best;
        }

        static void Add(Dictionary<int, List<RouteLink>> byFrom, RouteLink l)
        {
            if (!byFrom.TryGetValue(l.from, out List<RouteLink> list)) byFrom[l.from] = list = new List<RouteLink>();
            list.Add(l);
        }

        void Generate(NavGrid g, Dictionary<string, object> gridJson, ICollisionWorld world, List<AuthoredLink> authored)
        {
            List<object> clearJson = PortData.Arr(PortData.Obj(gridJson["nodes"])["clear"]);
            clear = new double[g.count];
            for (int n = 0; n < g.count; n++) clear[n] = (float)PortData.Num(clearJson[n]); // a Float32Array on the web: widened
            var byFrom = new Dictionary<int, List<RouteLink>>();
            for (int a = 0; a < g.count; a++) GenerateNode(g, world, a);
            // LEAP DOWN: every leap up's way back (the same arc reversed, checked again): a husk that leapt up a ledge chain
            // can hop back down it (R2b: every node gets down).
            foreach (RouteLink l in links)
            {
                if (l.kind != Link.LeapUp || l.rise <= 0) continue;
                var from = new Vec3(g.NodeX(l.to), g.y[l.to], g.NodeZ(l.to));
                var centre = new Vec3(g.NodeX(l.from), g.y[l.from], g.NodeZ(l.from));
                double dx = centre.x - from.x, dz = centre.z - from.z, len = JsMath.Hypot(dx, dz);
                // The nav jump lands ai.jumpLandIn past the node; on a narrow floor (a 1 m slab) that is off its far side:
                // then the link is flown to the node's centre (the hop lands there short of landIn: a hand check's call).
                var land = len > 0 ? new Vec3(centre.x + dx / len * p.landIn, centre.y, centre.z + dz / len * p.landIn) : centre;
                if (!FloorAt(world, land, g.y[l.from])) land = centre;
                if (!FloorAt(world, land, g.y[l.from])) continue;
                if (!RouteJumper.ArcClear(world, from, land, p.jumpApex, p.gravity, p.climbRadius, p.climbHeight)) continue;
                Add(byFrom, new RouteLink
                {
                    from = l.to, to = l.from, kind = Link.LeapUp, cost = (float)RouteCost.Leap(from, land, p), rise = -l.rise,
                    foot = from, lip = new Vec3(g.NodeX(l.from), g.y[l.from], g.NodeZ(l.from)), normal = new Vec3(-l.normal.x, 0, -l.normal.z), land = land,
                });
            }
            foreach (AuthoredLink e in authored) Author(g, world, e, byFrom);
            // Merge: each node's generated links, then its leap-downs and authored links (sorted by source for Pack).
            var merged = new List<RouteLink>(links.Count + 64);
            int i0 = 0;
            for (int a = 0; a < g.count; a++)
            {
                for (; i0 < links.Count && links[i0].from == a; i0++) merged.Add(links[i0]);
                if (byFrom.TryGetValue(a, out List<RouteLink> mine)) merged.AddRange(mine);
            }
            links.Clear();
            links.AddRange(merged);
        }

        void GenerateNode(NavGrid g, ICollisionWorld world, int a)
        {
            {
                int c = g.col[a];
                int ci = c % g.size, ck = c / g.size;
                double ya = g.y[a];
                for (int d = 0; d < 4; d++)
                {
                    int ni = ci + SIDES[d, 0], nk = ck + SIDES[d, 1];
                    if (ni < 0 || ni >= g.size || nk < 0 || nk >= g.size) continue;
                    int nc = nk * g.size + ni;
                    // CLIMB / CLIMB HIGH: up a face to a floor over the web's jump (jumpUp) next door, headroom for the climb
                    // and the mantle over this floor (the web's jump rule: clear >= rise + bodyHeight), flown at build.
                    for (int b = g.colStart[nc]; b < g.colStart[nc + 1]; b++)
                    {
                        double rise = g.y[b] - ya;
                        if (rise <= p.jumpUp || rise > p.swarmerClimbMax || clear[a] < rise + p.bodyHeight) continue;
                        bool high = rise > p.climbMax;
                        if (!ClimbFace(g, world, a, d, nc, ya, g.y[b], high ? p.swarmRadius : p.climbRadius, high ? p.swarmHeight : p.climbHeight, out Vec3 foot)) continue;
                        links.Add(new RouteLink
                        {
                            from = a, to = b, kind = high ? Link.ClimbHigh : Link.Climb, cost = (float)RouteCost.Climb(rise, p, high), rise = rise,
                            foot = foot, lip = new Vec3(g.NodeX(b), g.y[b], g.NodeZ(b)), normal = new Vec3(-SIDES[d, 0], 0, -SIDES[d, 1]),
                        });
                    }
                    // LEAP UP: a floor next door over the web's jump, up to leapUpMax, with no climb to it (a slab's edge over
                    // open ground): the web's nav jump (ai.jumps.husk.apex over the higher floor, landing ai.jumpLandIn past
                    // the edge, HuskNavJumper) flown at build with the climb's body; the landing has its floor.
                    // Across up to leapUpCells columns (the web's gap: a column between with nothing in the arc's band ends
                    // the scan), so a ledge chain with a gap at its corner (ARENA's spire steps) links up.
                    for (int k = 1; k <= p.leapUpCells; k++)
                    {
                        int ti = ci + SIDES[d, 0] * k, tk = ck + SIDES[d, 1] * k;
                        if (ti < 0 || ti >= g.size || tk < 0 || tk >= g.size) break;
                        int tc = tk * g.size + ti;
                        if (k > 1)
                        {
                            int mc = (ck + SIDES[d, 1] * (k - 1)) * g.size + ci + SIDES[d, 0] * (k - 1);
                            bool between = false;
                            for (int m = g.colStart[mc]; m < g.colStart[mc + 1]; m++) if (g.y[m] >= ya - p.step && g.y[m] <= ya + p.leapUpMax + p.bodyHeight) between = true;
                            if (between) break;
                        }
                        for (int b = g.colStart[tc]; b < g.colStart[tc + 1]; b++)
                        {
                            double rise = g.y[b] - ya;
                            if (rise <= p.jumpUp || rise > p.leapUpMax || HasLink(a, b)) continue;
                            var from = new Vec3(g.NodeX(a), ya, g.NodeZ(a));
                            var to = new Vec3(g.NodeX(b) + SIDES[d, 0] * p.landIn, g.y[b], g.NodeZ(b) + SIDES[d, 1] * p.landIn);
                            if (!world.RayIntersect(new Vec3(to.x, to.y + 0.6, to.z), DOWN, out RayHit floor) || Math.Abs(to.y + 0.6 - floor.distance - g.y[b]) > p.step) continue;
                            if (!RouteJumper.ArcClear(world, from, to, p.jumpApex, p.gravity, p.climbRadius, p.climbHeight)) continue;
                            links.Add(new RouteLink
                            {
                                from = a, to = b, kind = Link.LeapUp, cost = (float)RouteCost.Leap(from, to, p), rise = rise,
                                foot = from, lip = new Vec3(g.NodeX(b), g.y[b], g.NodeZ(b)), normal = new Vec3(-SIDES[d, 0], 0, -SIDES[d, 1]), land = to,
                            });
                        }
                    }
                    // A floor within a step of this one next door: the ledge goes on, no drop here.
                    bool level = false;
                    for (int b = g.colStart[nc]; b < g.colStart[nc + 1]; b++) if (Math.Abs(g.y[b] - ya) <= p.step) level = true;
                    if (level) continue;
                    for (int b = g.colStart[nc]; b < g.colStart[nc + 1]; b++)
                    {
                        double fall = ya - g.y[b];
                        if (fall <= p.drop || fall > p.longDrop) continue;
                        if (clear[b] < fall + p.bodyHeight) continue; // the fall's column is open to the ledge's height
                        if (!StepOffClear(g, world, a, nc, ya)) continue;
                        links.Add(new RouteLink
                        {
                            from = a, to = b, kind = Link.LongDrop, cost = (float)RouteCost.LongDrop(fall, p), rise = -fall,
                            foot = new Vec3(g.NodeX(a), ya, g.NodeZ(a)), lip = new Vec3(g.NodeX(b), g.y[b], g.NodeZ(b)),
                        });
                    }
                }
            }
        }

        /// <summary>
        /// A climbable face from floor ya (node a's column) toward column nc up to the floor yb: horizontal rays every 0.5 m
        /// from 0.3 m over the floor to 0.3 m under the lip all meet a wall-like face (|n.y| under surface.wallMaxAbsNormalY)
        /// within a cell and a bit, at one depth (+-0.35 m: no climbing up open air or round a step); over the lip the way is
        /// open to the next column's centre; then the body (radius, height) is flown: up from the foot (against the face) to
        /// 0.1 m over the lip, and over onto the upper node's centre (the mantle), never touching the collider (the web's
        /// navGraph builds its edges by flying them; Vermintide's ledgelator samples the face the same way).
        /// </summary>
        bool ClimbFace(NavGrid g, ICollisionWorld world, int a, int d, int nc, double ya, double yb, double radius, double height, out Vec3 foot)
        {
            foot = default;
            double cx = g.NodeX(a), cz = g.NodeZ(a);
            double ax = cx + EPS_X, az = cz + EPS_Z;
            var dir = new Vec3(SIDES[d, 0], 0, SIDES[d, 1]);
            double face = -1;
            for (double h = ya + 0.3; h <= yb - 0.3 + 1e-9; h += 0.5)
            {
                if (!world.RayIntersect(new Vec3(ax, h, az), dir, out RayHit hit) || hit.distance > 1.2 || Math.Abs(hit.normal.y) >= p.wallMaxAbsNormalY) return false;
                if (face < 0) face = hit.distance;
                else if (Math.Abs(hit.distance - face) > 0.35) return false;
            }
            if (face < 0) return false;
            if (world.RayIntersect(new Vec3(ax, yb + 0.3, az), dir, out RayHit over) && over.distance < g.cell + radius) return false;
            double off = Math.Max(0, face - radius - 0.06);
            foot = new Vec3(cx + dir.x * off, ya, cz + dir.z * off);
            double top = yb + 0.1;
            for (double y = ya + 0.1; ; y = Math.Min(top, y + 0.3))
            {
                if (BodyHits(world, foot.x, y, foot.z, radius, height)) return false;
                if (y >= top) break;
            }
            double bx = g.ColX(nc), bz = g.ColZ(nc);
            double len = JsMath.Hypot(bx - foot.x, bz - foot.z);
            int n = Math.Max(1, (int)Math.Ceiling(len / 0.25));
            for (int i = 1; i <= n; i++)
            {
                double t = (double)i / n;
                if (BodyHits(world, foot.x + (bx - foot.x) * t, top, foot.z + (bz - foot.z) * t, radius, height)) return false;
            }
            return true;
        }

        static bool BodyHits(ICollisionWorld world, double x, double y, double z, double radius, double height)
        {
            var cap = new Capsule { start = new Vec3(x, y + radius, z), end = new Vec3(x, y + height - radius, z), radius = radius };
            return WorldCollider.CapsuleIntersect(world, cap, out _, out _);
        }

        /// <summary>A floor at `at` within a step of height `y` (a ray down from 0.6 m over it).</summary>
        bool FloorAt(ICollisionWorld world, Vec3 at, double y) =>
            world.RayIntersect(new Vec3(at.x, y + 0.6, at.z), DOWN, out RayHit floor) && Math.Abs(y + 0.6 - floor.distance - y) <= p.step;

        /// <summary>A link from a to b already made for node a (its links are the last ones in the list).</summary>
        bool HasLink(int a, int b)
        {
            for (int i = links.Count - 1; i >= 0 && links[i].from == a; i--) if (links[i].to == b) return true;
            return false;
        }

        /// <summary>Nothing solid between the ledge's column and the next one, at a knee and a chest over the ledge (a parapet).</summary>
        static bool StepOffClear(NavGrid g, ICollisionWorld world, int a, int nc, double ya)
        {
            double ax = g.NodeX(a) + EPS_X, az = g.NodeZ(a) + EPS_Z;
            double bx = g.ColX(nc) + EPS_X, bz = g.ColZ(nc) + EPS_Z;
            double len = JsMath.Hypot(bx - ax, bz - az);
            var dir = new Vec3((bx - ax) / len, 0, (bz - az) / len);
            foreach (double lift in new[] { 0.5, 1.4 })
            {
                if (world.RayIntersect(new Vec3(ax, ya + lift, az), dir, out RayHit hit) && hit.distance < len) return false;
            }
            return true;
        }

        /// <summary>The on arrays: each node's web links in their order, then its layer links; the reverse index as
        /// NavGrid.FromExport builds it (counted, prefix-summed, filled in link order).</summary>
        void Pack(NavGrid g)
        {
            webLinkStart = g.linkStart;
            webLinkTo = g.linkTo;
            webLinkKind = g.linkKind;
            webLinkCost = g.linkCost;
            webRevStart = g.revStart;
            webRevFrom = g.revFrom;
            webRevLink = g.revLink;
            int n = g.count, mw = webLinkTo.Length, m = mw + links.Count;
            onLinkStart = new int[n + 1];
            onLinkTo = new int[m];
            onLinkKind = new byte[m];
            onLinkCost = new float[m];
            extraOf = new int[m];
            int k = 0, e = 0;
            for (int a = 0; a < n; a++)
            {
                onLinkStart[a] = k;
                for (int i = webLinkStart[a]; i < webLinkStart[a + 1]; i++)
                {
                    onLinkTo[k] = webLinkTo[i];
                    onLinkKind[k] = webLinkKind[i];
                    onLinkCost[k] = webLinkCost[i];
                    extraOf[k] = -1;
                    k++;
                }
                for (; e < links.Count && links[e].from == a; e++)
                {
                    onLinkTo[k] = links[e].to;
                    onLinkKind[k] = links[e].kind;
                    onLinkCost[k] = links[e].cost;
                    extraOf[k] = e;
                    counts[links[e].kind]++;
                    k++;
                }
            }
            onLinkStart[n] = k;
            onRevStart = new int[n + 1];
            onRevFrom = new int[m];
            onRevLink = new int[m];
            for (int i = 0; i < m; i++) onRevStart[onLinkTo[i] + 1]++;
            for (int i = 0; i < n; i++) onRevStart[i + 1] += onRevStart[i];
            var fill = new int[n];
            for (int from = 0; from < n; from++)
            {
                for (int i = onLinkStart[from]; i < onLinkStart[from + 1]; i++)
                {
                    int to = onLinkTo[i];
                    int slot = onRevStart[to] + fill[to]++;
                    onRevFrom[slot] = from;
                    onRevLink[slot] = i;
                }
            }
            reachJump = new byte[n];
            reachWalk = new byte[n];
            reachWeb = new byte[n];
            queue = new int[n];
        }

        /// <summary>The layer on or off: the grid's link arrays swapped (a change bumps grid.version: the fields restart).</summary>
        public void Apply(NavGrid g, bool on)
        {
            if (on == On) return;
            On = on;
            g.linkStart = on ? onLinkStart : webLinkStart;
            g.linkTo = on ? onLinkTo : webLinkTo;
            g.linkKind = on ? onLinkKind : webLinkKind;
            g.linkCost = on ? onLinkCost : webLinkCost;
            g.revStart = on ? onRevStart : webRevStart;
            g.revFrom = on ? onRevFrom : webRevFrom;
            g.revLink = on ? onRevLink : webRevLink;
            g.version++;
        }

        /// <summary>The layer's link behind an on-array link index (false: a web link, or the layer is off).</summary>
        public bool LinkAt(int linkIndex, out RouteLink link)
        {
            int e = On && linkIndex >= 0 && linkIndex < extraOf.Length ? extraOf[linkIndex] : -1;
            link = e >= 0 ? links[e] : default;
            return e >= 0;
        }

        /// <summary>Which unblocked nodes a walker standing on the ground (y <= groundY) reaches, for the jumper's and the
        /// walker's masks (a gate opened: NavField.SetBlocked). Allocates nothing.</summary>
        public void ComputeReach(NavGrid g)
        {
            Flood(g, Link.MaskJumper, reachJump);
            Flood(g, Link.MaskWalker, reachWalk);
            Flood(g, Link.MaskWebJumper, reachWeb);
        }

        void Flood(NavGrid g, int mask, byte[] reach)
        {
            int head = 0, tail = 0;
            for (int n = 0; n < g.count; n++)
            {
                reach[n] = (byte)(g.blocked[n] == 0 && g.y[n] <= p.groundY ? 1 : 0);
                if (reach[n] != 0) queue[tail++] = n;
            }
            while (head < tail)
            {
                int v = queue[head++];
                for (int li = g.linkStart[v]; li < g.linkStart[v + 1]; li++)
                {
                    int u = g.linkTo[li];
                    if (reach[u] != 0 || g.blocked[u] != 0 || !Link.Usable(g.linkKind[li], mask)) continue;
                    reach[u] = 1;
                    queue[tail++] = u;
                }
            }
        }
    }

    /// <summary>
    /// The husk pool's NavJumper with design.routes on: the web's nav jumps (the inner HuskNavJumper) plus the fixed leap and
    /// the long drop's landing stagger. The leap is planned by Husk.Step (PlanLeap, where the web's leap fires) and flown
    /// here: a DANGER crouch of leapTell, then the checked arc to the planned landing.
    /// </summary>
    public sealed class RouteJumper : NavJumper
    {
        readonly NavJumper inner;
        readonly EventBus events;
        public readonly RouteLayer layer;
        readonly RouteParams p;
        readonly double gravity, leapApex;
        readonly ClimbEvent ev = new ClimbEvent(); // reused (N20Events: no handler keeps it)
        static readonly double[] ApproachAngles = { 0, Math.PI / 6, -Math.PI / 6, Math.PI / 3, -Math.PI / 3, Math.PI / 2, -Math.PI / 2 };
        static readonly Vec3 DOWN = new Vec3(0, -1, 0);

        public RouteJumper(GameConfig cfg, EventBus events, NavJumper inner, RouteLayer layer)
        {
            this.inner = inner;
            this.events = events;
            this.layer = layer;
            p = layer.p;
            gravity = p.gravity;
            leapApex = cfg.Num("husk.leapApexAbove");
        }

        public override bool Step(Husk e, double dt, bool busy)
        {
            Fall(e, dt);
            if (e.leapFromSteps > 0) e.leapFromSteps--;
            NavJumpState s = e.jump;
            if (s != null && s.verb == NavJumpState.Leap) return StepLeap(e, s, dt, busy);
            if (s != null && s.verb == NavJumpState.Climb) return StepClimb(e, s, dt, busy);
            return inner.Step(e, dt, busy);
        }

        /// <summary>
        /// Row 4 (a hand check: husks missed the spire chain's leaps from wherever in the cell they stood, the arc flown at
        /// build from the cell's centre): the brain at a LEAP UP link's node: the layer's leap (a DANGER crouch of leapTell
        /// while it steps to the node's centre, then the arc flown at build, to its landing). Never a charger.
        /// </summary>
        public bool StartLeapLink(Husk e, int linkIndex)
        {
            if (e.jump != null || !(e.grounded || e.onEdge) || e.kind == "charger" || !layer.LinkAt(linkIndex, out RouteLink link) || link.kind != Link.LeapUp) return false;
            NavJumpState s = e.leapState ??= new NavJumpState();
            s.verb = NavJumpState.Leap;
            s.crouchSteps = -1;
            s.phase = 1; // 1: the link's leap (step to the foot in the crouch); 0: the reactive leap (PlanLeap)
            s.foot = link.foot;
            s.to = link.land;
            s.apex = p.jumpApex;
            s.hop = false;
            s.flying = false;
            s.flightSteps = 0;
            s.vx = s.vz = 0;
            e.jump = s;
            e.leapFromSteps = 0;
            return true;
        }

        /// <summary>
        /// The brain at a climb link's foot node (the field's link onward is a CLIMB, CLIMB HIGH or AUTHORED one): the body is
        /// the climb's from the next step. Only standing and free; never a CHARGER, a tall climb only for a swarmer.
        /// </summary>
        public bool StartClimb(Husk e, int linkIndex)
        {
            if (e.jump != null || !(e.grounded || e.onEdge) || e.kind == "charger" || !layer.LinkAt(linkIndex, out RouteLink link)) return false;
            if (link.kind == Link.ClimbHigh && e.kind != "swarmer") return false;
            if (link.kind != Link.Climb && link.kind != Link.ClimbHigh && link.kind != Link.Authored) return false;
            if (link.rise <= 0) return false; // an authored ladder's way down: it walks off the top (the brain steers to the foot)
            NavJumpState s = e.leapState ??= new NavJumpState();
            s.verb = NavJumpState.Climb;
            s.crouchSteps = -1;
            s.phase = 0;
            s.phaseSteps = 0;
            s.foot = link.foot;
            s.to = link.lip;
            s.normal = link.normal;
            s.linkKind = link.kind;
            s.hop = false;
            s.flying = false;
            s.flightSteps = 0;
            e.jump = s;
            e.leapFromSteps = 0;
            return true;
        }

        /// <summary>
        /// A climb (U3-34.6 step 3): 0, the DANGER tell (climbTell; a swarmer's swarmerClimbTell) while it steps to the foot,
        /// against the face; 1, up the face at climbSpeed (swarmerClimbSpeed) with gravity cancelled (Move integrates the
        /// set velocity: dy = vy dt - g dt^2 / 2); 2, the mantle (mantle s) over the lip onto the upper node. A knock, a
        /// stagger or a stun at any phase knocks it off ("knocked"): it falls and stands for dropStagger where it lands.
        /// </summary>
        bool StepClimb(Husk e, NavJumpState s, double dt, bool busy)
        {
            bool swarm = e.kind == "swarmer";
            if (s.phase == 0 && s.crouchSteps < 0)
            {
                if (!(e.grounded || e.onEdge) || busy)
                {
                    e.jump = null;
                    return false;
                }
                s.crouchSteps = Math.Max(1, MovementMath.StepsFor(swarm ? p.swarmerClimbTell : p.climbTell, dt));
                Emit(e, "start", s.foot, s.to, s.linkKind);
            }
            if (busy || e.knockSteps > 0 || e.staggerSteps > 0)
            {
                e.jump = null;
                if (s.phase > 0)
                {
                    e.landStagger = true;
                    if (e.velocity.y > 0) e.velocity.y = 0;
                }
                Emit(e, "knocked", s.foot, s.to, s.linkKind);
                return false;
            }
            e.yaw = JsMath.Atan2(s.normal.x, s.normal.z); // facing the face (the husks' yaw: atan2(-dx, -dz) of the way it looks)
            if (s.phase == 0)
            {
                int left = s.crouchSteps;
                e.velocity.x = (s.foot.x - e.position.x) / (left * dt);
                e.velocity.z = (s.foot.z - e.position.z) / (left * dt);
                if (--s.crouchSteps > 0) return true;
                s.phase = 1;
                s.phaseSteps = 0;
                Emit(e, "up", s.foot, s.to, s.linkKind);
                return true;
            }
            if (s.phase == 1)
            {
                double speed = s.linkKind == Link.Authored ? p.ladderSpeed : swarm ? p.swarmerClimbSpeed : p.climbSpeed;
                s.phaseSteps++;
                e.grounded = false;
                e.velocity.x = (s.foot.x - e.position.x) / dt * 0.5; // held on the face
                e.velocity.z = (s.foot.z - e.position.z) / dt * 0.5;
                double need = s.to.y + 0.05 - e.position.y;
                double stepUp = Math.Min(speed * dt, Math.Max(0, need));
                e.velocity.y = stepUp / dt + gravity * dt / 2;
                if (need <= speed * dt)
                {
                    s.phase = 2;
                    s.crouchSteps = Math.Max(1, MovementMath.StepsFor(p.mantle, dt));
                }
                else if (s.phaseSteps > MovementMath.StepsFor((s.to.y - s.foot.y) / speed + 1.5, dt))
                {
                    e.jump = null; // stuck on the face (something in the way): it lets go
                    e.landStagger = true;
                    Emit(e, "knocked", s.foot, s.to, s.linkKind);
                    return false;
                }
                return true;
            }
            int rest = s.crouchSteps;
            e.velocity.x = (s.to.x - e.position.x) / (rest * dt);
            e.velocity.z = (s.to.z - e.position.z) / (rest * dt);
            e.velocity.y = (s.to.y + 0.05 - e.position.y) / (rest * dt) + gravity * dt / 2;
            if (--s.crouchSteps > 0) return true;
            e.jump = null;
            e.fallTop = s.to.y;
            // Over the lip: it stops there until the brain's next think (as a leap lands), not carried on by the mantle.
            e.velocity.x = 0;
            e.velocity.z = 0;
            if (e.velocity.y > 0) e.velocity.y = 0;
            if (e.ai != null)
            {
                e.ai.vx = 0;
                e.ai.vz = 0;
            }
            Emit(e, "top", s.foot, s.to, s.linkKind);
            return true;
        }

        void Emit(Husk e, string phase, Vec3 foot, Vec3 lip, int linkKind)
        {
            ev.enemy = e.id;
            ev.kind = e.kind;
            ev.foot = foot;
            ev.lip = lip;
            ev.phase = phase;
            ev.linkKind = linkKind;
            events.Emit(N20Events.Climb, ev);
        }

        /// <summary>The fall's top while in the air; landing more than the web's drop (ai.grid.drop) below it, or knocked off a
        /// climb, stands it for dropStagger (a punish window; "land").</summary>
        void Fall(Husk e, double dt)
        {
            if (!(e.grounded || e.onEdge))
            {
                if (e.position.y > e.fallTop) e.fallTop = e.position.y;
                return;
            }
            if ((e.fallTop - e.position.y > p.drop || e.landStagger) && e.alive)
            {
                e.Stun(p.dropStagger, dt);
                Emit(e, "land", new Vec3(e.position.x, e.fallTop, e.position.z), e.position, Link.LongDrop);
            }
            e.landStagger = false;
            e.fallTop = e.position.y;
        }

        bool StepLeap(Husk e, NavJumpState s, double dt, bool busy)
        {
            if (!s.flying)
            {
                if (s.crouchSteps < 0)
                {
                    if (!(e.grounded || e.onEdge) || busy)
                    {
                        e.jump = null;
                        return false;
                    }
                    s.crouchSteps = Math.Max(1, MovementMath.StepsFor(p.leapTell, dt));
                    if (s.phase == 0) s.foot = e.position;
                    Emit(e, "start", s.foot, s.to, s.phase == 1 ? Link.LeapUp : Link.Jump);
                }
                // A link's leap steps to its foot (the node's centre, where its arc was flown) during the crouch.
                e.velocity.x = s.phase == 1 ? (s.foot.x - e.position.x) / (s.crouchSteps * dt) : 0;
                e.velocity.z = s.phase == 1 ? (s.foot.z - e.position.z) / (s.crouchSteps * dt) : 0;
                if (busy)
                {
                    e.jump = null;
                    Emit(e, "knocked", s.foot, s.to, s.phase == 1 ? Link.LeapUp : Link.Jump);
                    return false;
                }
                if (--s.crouchSteps > 0) return true;
                e.velocity = CombatMath.LeapVelocity(e.position, s.to, s.apex, gravity, out _);
                s.vx = e.velocity.x;
                s.vz = e.velocity.z;
                e.grounded = false;
                e.leaping = true;
                e.leapCooldownSteps = MovementMath.StepsFor(p.leapCooldown, dt);
                s.flying = true;
                s.flightSteps = 0;
                events.Emit("huskLeap", e.id); // husk.js's event, at the launch as the web's
                Emit(e, "up", s.foot, s.to, s.phase == 1 ? Link.LeapUp : Link.Jump);
                return true;
            }
            s.flightSteps++;
            if (e.knockSteps > 0)
            {
                // A punch in the air: the leap is lost (it falls with the knock and stands where it lands).
                e.jump = null;
                e.landStagger = true;
                Emit(e, "knocked", s.foot, s.to, s.phase == 1 ? Link.LeapUp : Link.Jump);
                return false;
            }
            if (!e.grounded)
            {
                e.velocity.x = s.vx;
                e.velocity.z = s.vz;
            }
            if ((e.grounded && s.flightSteps > 2) || s.flightSteps > MovementMath.StepsFor(2.5, dt))
            {
                e.jump = null;
                // Landed: it stops on the spot until the brain's next think (a hand check: the flight's speed and the last
                // steer carried husks off the far side of the 2.5 m spire ledges).
                e.velocity.x = 0;
                e.velocity.z = 0;
                if (e.ai != null)
                {
                    e.ai.vx = 0;
                    e.ai.vz = 0;
                }
                Emit(e, "top", s.foot, e.position, s.phase == 1 ? Link.LeapUp : Link.Jump);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Husk.Step's leap with design.routes on (the web's fires at your feet from wherever it stands): the landing is the
        /// floor under your feet, stopDistance short; at most leapMaxRise up; the arc must clear the collider (ArcClear) from
        /// here, else the husk takes an approach point (leapFrom, the brain steers to it) leapApproachMin-Max m out with a clear
        /// arc. True: a leap is planned (e.jump; the crouch starts next step). A failed plan waits leapRetry (or, with no
        /// approach point, husk.leapCooldown) before the next.
        /// </summary>
        public bool PlanLeap(Husk e, Vec3 tp, ICollisionWorld world, double dt)
        {
            HuskParams h = e.Params;
            double dx = tp.x - e.position.x, dz = tp.z - e.position.z;
            double horizontal = JsMath.Hypot(dx, dz);
            double stop = horizontal > 0 ? Math.Min(h.stopDistance, horizontal) / horizontal : 0;
            var to = new Vec3(tp.x - dx * stop, tp.y, tp.z - dz * stop);
            if (!FloorUnder(world, to, out double floorY) || floorY - e.position.y > p.leapMaxRise)
            {
                e.leapCooldownSteps = MovementMath.StepsFor(p.leapRetry, dt);
                return false;
            }
            to.y = floorY;
            if (ArcClear(world, e.position, to, leapApex, gravity, e.Radius, e.Height))
            {
                NavJumpState s = e.leapState ??= new NavJumpState();
                s.verb = NavJumpState.Leap;
                s.crouchSteps = -1;
                s.phase = 0; // the reactive leap: from where it stands
                s.to = to;
                s.apex = leapApex;
                s.hop = false;
                s.flying = false;
                s.flightSteps = 0;
                s.vx = s.vz = 0;
                e.jump = s;
                e.leapFromSteps = 0;
                return true;
            }
            bool arrived = e.leapFromSteps > 0 && JsMath.Hypot(e.leapFrom.x - e.position.x, e.leapFrom.z - e.position.z) < 0.6;
            if (e.leapFromSteps > 0 && !arrived)
            {
                e.leapCooldownSteps = MovementMath.StepsFor(p.leapRetry, dt);
                return false; // still walking to its approach point
            }
            if (!arrived && FindApproach(e, to, world, out Vec3 a))
            {
                e.leapFrom = a;
                e.leapFromSteps = MovementMath.StepsFor(p.leapApproachTime, dt);
                e.leapCooldownSteps = MovementMath.StepsFor(p.leapRetry, dt);
                return false;
            }
            e.leapFromSteps = 0;
            e.leapCooldownSteps = MovementMath.StepsFor(p.leapCooldown, dt);
            return false;
        }

        /// <summary>A floor within 1.4 m under `at` (the player's feet, mid-air or not): its height.</summary>
        static bool FloorUnder(ICollisionWorld world, Vec3 at, out double y)
        {
            y = 0;
            if (!world.RayIntersect(new Vec3(at.x, at.y + 0.6, at.z), DOWN, out RayHit hit) || hit.distance > 2.0) return false;
            y = at.y + 0.6 - hit.distance;
            return true;
        }

        /// <summary>An approach point leapApproachMin-Max m out from the husk, away from the landing (straight back first,
        /// then 30, 60 and 90 deg either side): a floor within a step of its own, room for its body, a clear walk to it, the
        /// landing within husk.leapMaxHorizontal and leapMaxRise, and a clear arc.</summary>
        bool FindApproach(Husk e, Vec3 to, ICollisionWorld world, out Vec3 at)
        {
            at = default;
            HuskParams h = e.Params;
            double bx = e.position.x - to.x, bz = e.position.z - to.z;
            double bl = JsMath.Hypot(bx, bz);
            if (bl < 0.1)
            {
                bx = 1;
                bz = 0;
                bl = 1;
            }
            bx /= bl;
            bz /= bl;
            foreach (double ang in ApproachAngles)
            {
                double cs = JsMath.Cos(ang), sn = JsMath.Sin(ang); // fdlibm (DeterminismTests)
                double dx = bx * cs - bz * sn, dz = bx * sn + bz * cs;
                for (double d = p.leapApproachMin; d <= p.leapApproachMax + 1e-9; d += 1)
                {
                    var c = new Vec3(e.position.x + dx * d, e.position.y, e.position.z + dz * d);
                    if (!FloorUnder(world, c, out double fy) || Math.Abs(fy - e.position.y) > p.step) continue;
                    c.y = fy;
                    if (JsMath.Hypot(to.x - c.x, to.z - c.z) > h.leapMaxHorizontal || to.y - c.y > p.leapMaxRise) continue;
                    var cap = new Capsule { start = new Vec3(c.x, c.y + h.radius + 0.1, c.z), end = new Vec3(c.x, c.y + h.height - h.radius, c.z), radius = h.radius };
                    if (WorldCollider.CapsuleIntersect(world, cap, out _, out _)) continue;
                    var dir = new Vec3(dx, 0, dz);
                    if (world.RayIntersect(new Vec3(e.position.x, e.position.y + 0.5, e.position.z), dir, out RayHit w0) && w0.distance < d) break;
                    if (world.RayIntersect(new Vec3(e.position.x, e.position.y + 1.5, e.position.z), dir, out RayHit w1) && w1.distance < d) break;
                    if (!ArcClear(world, c, to, leapApex, gravity, h.radius, h.height)) continue;
                    at = c;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// The leap's ballistic arc (CombatMath.LeapVelocity, the husks' gravity) swept through the collider: segments of at
        /// most 0.06 s, five rays each (0.25 m over the feet, the body's middle and 0.1 m under the top of the head on the
        /// axis; the feet and the head again at the body's leading edge, `radius` ahead along the flight: the part that meets
        /// a lip), any hit inside a segment blocks it (Vermintide's Rat Ogre checks its leap with segmented raycasts).
        /// </summary>
        public static bool ArcClear(ICollisionWorld world, Vec3 from, Vec3 to, double apex, double gravity, double radius, double height)
        {
            Vec3 v = CombatMath.LeapVelocity(from, to, apex, gravity, out double time);
            double hl = JsMath.Hypot(v.x, v.z);
            double fx = hl > 1e-9 ? v.x / hl * radius : 0, fz = hl > 1e-9 ? v.z / hl * radius : 0;
            int n = Math.Max(4, (int)Math.Ceiling(time / 0.06));
            double px = from.x, py = from.y, pz = from.z;
            for (int i = 1; i <= n; i++)
            {
                double t = time * i / n;
                double cx = from.x + v.x * t, cy = from.y + v.y * t - 0.5 * gravity * t * t, cz = from.z + v.z * t;
                double sx = cx - px, sy = cy - py, sz = cz - pz;
                double len = Math.Sqrt(sx * sx + sy * sy + sz * sz);
                if (len > 1e-9)
                {
                    var dir = new Vec3(sx / len, sy / len, sz / len);
                    if (Hit(world, new Vec3(px, py + 0.25, pz), dir, len)) return false;
                    if (Hit(world, new Vec3(px, py + height * 0.5, pz), dir, len)) return false;
                    if (Hit(world, new Vec3(px, py + height - 0.1, pz), dir, len)) return false;
                    if (i > 1 && Hit(world, new Vec3(px + fx, py + 0.25, pz + fz), dir, len)) return false; // not from inside the start's own wall
                    if (Hit(world, new Vec3(px + fx, py + height - 0.1, pz + fz), dir, len)) return false;
                }
                px = cx;
                py = cy;
                pz = cz;
            }
            return true;
        }

        static bool Hit(ICollisionWorld world, Vec3 o, Vec3 dir, double len) => world.RayIntersect(o, dir, out RayHit hit) && hit.distance < len;
    }
}
