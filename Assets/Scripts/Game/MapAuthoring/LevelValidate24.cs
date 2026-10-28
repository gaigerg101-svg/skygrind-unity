// NIGHT24 (builder L): the CAMPAIGN level's validation report (the MapValidate cousin for a rooms-and-doors map; MapValidate's
// SURVIVAL checks - zones bought in price order, perk and box spots - do not apply to a level). Pure (no UnityEngine); written by
// `levelgen` to Reports/LEVEL-<id>-validate.md from the map JSON. Every row is PASS or ERROR; `errors` is the count (0 wanted).
//   1. spots: every area's spawn and exit, and every encounter node, stand on the enemies' nav grid
//   2. the player's route (RIVAL's nav graph, all doors open): spawn -> exit inside each area, exit -> the next area's spawn,
//      with the time in seconds
//   3. doors close the nav: with door i shut, no grid node of the area behind it is reachable from the start on the enemies'
//      grid (jumpers' links), and with it open some are
//   4. every tier reachable by enemies from the floor (GG's standing rule 'there always needs to be a route that enemies can
//      climb'): in each room that holds enemies, every encounter node and the top of every raised ledge is reached from the
//      room's floor spawn by WALKERS (walk and drop links only: a ramp, a stair)
//   5. enemy nodes are reached by walkers from the floor too (an enemy never spawns where it would wait forever)
//   6. PITS (platforming stretches): from the pit's floor the room's spawn is reachable (jumpers' links: a stair, a ledge), so a failed hop never
//      soft-locks, and the player's graph still finds the way across
//   7. ROUTES (a room with "wallRoute"): the wall-jump line (a scripted climber on the real Mover, WallClimb) against the stepped route (RIVAL's
//      graph path spawn to exit, which has no wall jumps): both times are printed, and the wall line must be the faster one
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Skygrind.Core;

namespace Skygrind.Game.MapAuthoring
{
    public static class LevelValidate24
    {
        static Dictionary<string, object> O(object o) => o as Dictionary<string, object>;
        static List<object> L(object o) => o as List<object> ?? new List<object>();
        static double N(object o) => PortData.Num(o);
        static object Get(Dictionary<string, object> d, string k) => d != null && d.TryGetValue(k, out object v) ? v : null;
        static string F(double v) => (Math.Round(v * 100) / 100).ToString("0.##", CultureInfo.InvariantCulture);

        static bool[] Reach(NavGrid g, int from, int mask, Func<int, bool> allowed)
        {
            var seen = new bool[g.count];
            var queue = new Queue<int>();
            seen[from] = true;
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                for (int i = g.linkStart[v]; i < g.linkStart[v + 1]; i++)
                {
                    int u = g.linkTo[i];
                    if (seen[u] || !Link.Usable(g.linkKind[i], mask) || !allowed(u)) continue;
                    seen[u] = true;
                    queue.Enqueue(u);
                }
            }
            return seen;
        }

        /// <summary>A scripted wall-jump climber on a real Mover (builder L's `campaign --shaft` and this report's "wall-jump line"). Starts at (x, 0, z)
        /// beside the west wall running north (-z); the corridor between two facing walls is run ALONG its length (a wall ride needs speed along the
        /// wall: wallRide.minSpeed); it jumps into a ride, wall-jumps 0.3 s later to the facing wall, rides that, and so on, until it stands at or above
        /// `goalY` (feet). Returns the best height and the time (-1: never) and the wall jumps used.</summary>
        public static (double bestY, double seconds, int wallJumps) WallClimb(Mover m, double dt, double x, double z, double goalY, double maxSeconds, double yawLean = 0.25)
        {
            m.Reset(x, 0, z);
            double best = 0;
            int jumps = 0;
            bool westWall = true;
            int steps = MovementMath.StepsFor(maxSeconds, dt);
            int rideSteps = 0;
            for (int i = 0; i < steps; i++)
            {
                var f = new InputFrame { moveZ = 1 };
                f.yaw = westWall ? yawLean : -yawLean; // + turns left: toward -x (west)
                bool onWall = m.state == MoveState.WallRide;
                rideSteps = onWall ? rideSteps + 1 : 0;
                if (m.grounded) f.jump = new Button { held = true, pressed = true };
                else if (onWall && rideSteps * dt >= 0.3) f.jump = new Button { held = true, pressed = true };
                else f.jump = new Button { held = true, pressed = false };
                int before = m.wallJumpsLeft;
                m.Step(f, dt);
                if (m.wallJumpsLeft < before)
                {
                    jumps++;
                    westWall = !westWall;
                }
                if (m.position.y > best) best = m.position.y;
                if (m.grounded && m.position.y >= goalY - 0.05) return (best, i * dt, jumps);
            }
            return (best, -1, jumps);
        }

        /// <summary>The real Mover runs north (-z) from (x, 0, zStart) toward zGoal, sliding (held) or just running; returns whether it got there within 4 s.</summary>
        static (bool arrived, double seconds) TunnelRun(Mover m, double dt, double x, double zStart, double zGoal, bool slide)
        {
            m.Reset(x, 0, zStart);
            int steps = MovementMath.StepsFor(4, dt);
            for (int i = 0; i < steps; i++)
            {
                var f = new InputFrame { moveZ = 1, yaw = 0 };
                if (slide && i > 20) f.slide = new Button { held = true, pressed = i == 21 }; // run up first (a slide needs speed), then slide in
                m.Step(f, dt);
                if (m.position.z <= zGoal) return (true, i * dt);
            }
            return (false, -1);
        }

        /// <summary>NIGHT27: the lines the last Run's arena rules printed (the wall-vs-floor ratio per room, kills per level), for `levelgen` to echo.</summary>
        public static readonly List<string> N27Lines = new List<string>();

        public static (string markdown, int errors) Run(GameConfig cfg, Dictionary<string, object> map, string label)
        {
            N27Lines.Clear();
            var sb = new StringBuilder();
            var rows = new List<(string check, bool pass, string detail)>();
            LevelDef level = LevelDef.From(map);
            sb.Append($"# LEVEL {level.id}: validation\n\nGenerated by `levelgen` (Assets/Scripts/Game/MapAuthoring/LevelValidate24.cs, NIGHT24) from `{label}`. Rooms and doors from the plan; the enemies' grid is the map's nav grid, the player's graph is RIVAL's.\n\n");
            double lookBelow = cfg.Num("ai.grid.lookBelow"), lookAbove = cfg.Num("ai.grid.lookAbove");
            NavGrid g = NavGrid.FromExport(O(map["navGrid"]));
            if (cfg.Design("n33.ai33.enabled") && !N33Blocks.ForceOff)
            {
                // NIGHT33 E (design.n33.ai33): the enemies' grid as the sim has it: the lava nodes masked (SurvivalSim.Ai33.cs), so a room only walkers
                // could reach through the lava fails the reachability check
                var lava = new Lava30(Lava30Params.From(cfg), cfg.Dt);
                lava.Load(map);
                g.hazard = lava.NavMask(g, cfg.Num("design.n33.ai33.maskAbove"));
                if (g.hazard != null) g.SetBlocked(_ => false);
            }
            SurvivalMap sm = SurvivalMap.FromExport(map);
            CoreGrid core = sm.CreateCore(cfg.movement);
            var world = new GameWorld(sm.BuildOctree(), core);
            NavGraph graph = NavGraph.Build(NavMap.From(sm), world, cfg, core, NavGrid.FromExport(O(map["navGrid"])));
            int n = level.rooms.Count;
            var zoneIndex = new Dictionary<string, int>();
            for (int i = 0; i < sm.zones.Count; i++) zoneIndex[sm.zones[i].id] = i;

            int NodeOf(Vec3 p) => g.NodeAt(p.x, p.y, p.z, lookBelow, lookAbove);

            // 1. spots on the grid
            var spawnNode = new int[n];
            var exitNode = new int[n];
            var onGrid = new List<string>();
            for (int i = 0; i < n; i++)
            {
                RoomDef r = level.rooms[i];
                spawnNode[i] = NodeOf(r.spawn);
                exitNode[i] = NodeOf(r.exit);
                if (spawnNode[i] < 0) onGrid.Add($"{r.id} spawn");
                if (exitNode[i] < 0) onGrid.Add($"{r.id} exit");
                int off = 0;
                foreach (Vec3 nd in r.nodes) if (NodeOf(nd) < 0) { off++; onGrid.Add($"{r.id} node ({F(nd.x)}, {F(nd.y)}, {F(nd.z)})"); }
            }
            rows.Add(("spots on the nav grid", onGrid.Count == 0, onGrid.Count == 0 ? $"{n} areas: every spawn, exit and encounter node stands on a grid node" : "no grid node under: " + string.Join("; ", onGrid)));

            // 2. the player's route
            var legs = new List<string>();
            bool routeOk = true;
            var path = new NavPath();
            double total = 0;
            for (int i = 0; i < n && routeOk; i++)
            {
                RoomDef r = level.rooms[i];
                // NIGHT25: a room may carry "routeVia": crossings only the real Mover can do (a slide tunnel, a wall-jump flue), each proved by its own
                // row below; the route is then spawn to the crossing's start, the crossing's end to the next one's start, ..., to the exit.
                var chain = new List<Vec3> { r.spawn };
                var rawVia = L(Get(RoomJson(map, r.id), "routeVia"));
                foreach (object vo in rawVia)
                {
                    List<object> pair = L(vo);
                    chain.Add(V(pair[0]));
                    chain.Add(V(pair[1]));
                }
                chain.Add(r.exit);
                double legCost = 0;
                var legKinds = new List<string>();
                bool legOk = true;
                string failLeg = "";
                for (int c = 0; c + 1 < chain.Count && legOk; c += 2)
                {
                    int a0 = Astar.NearestNode(graph, chain[c]), b0 = Astar.NearestNode(graph, chain[c + 1]);
                    if (a0 < 0 || b0 < 0 || !Astar.Path(graph, a0, b0, cfg.movement.dash.speed, path)) { legOk = false; failLeg = $"leg {c / 2 + 1} ({F(chain[c].x)}, {F(chain[c].y)}, {F(chain[c].z)}) to ({F(chain[c + 1].x)}, {F(chain[c + 1].y)}, {F(chain[c + 1].z)}): {(a0 < 0 ? "no grid node at the start" : b0 < 0 ? "no grid node at the end" : "no path")}"; }
                    else
                    {
                        legCost += path.cost;
                        foreach (string k in Kinds(path)) if (!legKinds.Contains(k)) legKinds.Add(k);
                    }
                }
                if (!legOk) { routeOk = false; legs.Add($"{r.id}: spawn to exit has NO route ({failLeg})"); break; }
                int b = Astar.NearestNode(graph, r.exit);
                legs.Add($"{r.id} {F(legCost)} s ({string.Join(" ", legKinds)}{(rawVia.Count > 0 ? "; " + rawVia.Count + " crossing(s) by the real Mover, proved below" : "")})");
                total += legCost;
                if (i + 1 < n)
                {
                    int c = Astar.NearestNode(graph, level.rooms[i + 1].spawn);
                    if (!Astar.Path(graph, b, c, cfg.movement.dash.speed, path)) { routeOk = false; legs.Add($"{r.id} exit to {level.rooms[i + 1].id} spawn has NO route"); break; }
                    total += path.cost;
                }
            }
            rows.Add(("player route, doors open", routeOk, string.Join("; ", legs) + (routeOk ? $"; the whole line {F(total)} s of travel" : "")));

            // 3. doors close the nav
            var tagOfRoom = new int[n];
            for (int i = 0; i < n; i++) tagOfRoom[i] = zoneIndex.TryGetValue(level.rooms[i].id, out int z) ? z : -1;
            var doorTag = new int[Math.Max(0, n - 1)];
            for (int i = 0; i + 1 < n; i++) doorTag[i] = zoneIndex.TryGetValue(level.doors[i].id, out int z) ? z : -1;
            var closedBad = new List<string>();
            var openBad = new List<string>();
            for (int d = 0; d + 1 < n; d++)
            {
                if (exitNode[d] < 0) continue;
                // from room d's exit, inside room d only: door d shut, nothing of the door or the area behind is reachable; open, the area behind is
                int here = tagOfRoom[d], dt = doorTag[d], next = tagOfRoom[d + 1];
                bool[] shut = Reach(g, exitNode[d], Link.MaskJumper, u => g.tag[u] == here);
                int behind = 0;
                for (int u = 0; u < g.count; u++) if (shut[u] && (g.tag[u] == next || g.tag[u] == dt)) behind++;
                if (behind > 0) closedBad.Add($"{level.doors[d].id}: {behind} nodes reachable with it shut");
                bool[] opened = Reach(g, exitNode[d], Link.MaskJumper, u => g.tag[u] == here || g.tag[u] == dt || g.tag[u] == next);
                int ahead = 0;
                for (int u = 0; u < g.count; u++) if (opened[u] && g.tag[u] == next) ahead++;
                if (ahead == 0) openBad.Add($"{level.doors[d].id}: the area behind is unreachable from the exit with it open");
            }
            rows.Add(("doors close the nav", closedBad.Count == 0 && openBad.Count == 0,
                closedBad.Count + openBad.Count == 0 ? $"{Math.Max(0, n - 1)} doors: shut, nothing behind is reachable from the room's exit; open, the area behind is" : string.Join("; ", closedBad) + string.Join("; ", openBad)));

            // 4 + 5. tiers and enemy nodes from the floor, by walkers
            var ledges = new List<(Vec3 min, Vec3 max)>();
            foreach (object b in L(O(map["data"])["boxes"]))
            {
                var d = O(b);
                if ((d.TryGetValue("kind", out object k) ? k as string : "") != "ledge") continue;
                ledges.Add((V(d["min"]), V(d["max"])));
            }
            for (int i = 0; i < n; i++)
            {
                RoomDef r = level.rooms[i];
                if (r.kind == "link" || r.nodes.Count == 0 && r.waves.Count == 0) continue;
                if (spawnNode[i] < 0) continue;
                int tag = tagOfRoom[i];
                // NIGHT30 (builder L): a lava room whose floor is lava wall to wall (f5, f5b) names `reachFrom` [x, y, z]: the floor the enemies walk from (its
                // far platform), since no walker crosses lava; default the room's spawn
                int from = spawnNode[i];
                foreach (object ro in L(Get(O(map["campaign"]), "rooms")))
                    if (O(ro).TryGetValue("id", out object rid) && rid as string == r.id && O(ro).TryGetValue("reachFrom", out object rf) && rf is List<object>) { int u2 = NodeOf(V(rf)); if (u2 >= 0) from = u2; }
                bool[] walkers = Reach(g, from, Link.MaskWalker, u => g.tag[u] == tag);
                var missed = new List<string>();
                int tiers = 0;
                foreach (Vec3 nd in r.nodes)
                {
                    int u = NodeOf(nd);
                    if (u >= 0 && !walkers[u]) missed.Add($"node ({F(nd.x)}, {F(nd.y)}, {F(nd.z)})");
                }
                foreach (var (min, max) in ledges)
                {
                    Vec3 top = new Vec3((min.x + max.x) / 2, max.y, (min.z + max.z) / 2);
                    if (sm.zoneParts.Count > 0 && ZoneMath.ZoneIndexAt(sm.zoneParts, top.x, top.y + 1, top.z) != tag || max.y < 1) continue;
                    tiers++;
                    int u = NodeOf(top);
                    if (u < 0) { missed.Add($"tier at ({F(top.x)}, {F(top.y)}, {F(top.z)}) has no grid node"); continue; }
                    if (!walkers[u]) missed.Add($"tier at ({F(top.x)}, {F(top.y)}, {F(top.z)})");
                }
                rows.Add(($"enemies reach every tier: {r.id}", missed.Count == 0, missed.Count == 0
                    ? $"{tiers} raised ledge tops and {r.nodes.Count} encounter nodes, all reached from the floor spawn by walkers (walk and drop links only)"
                    : "not reached by walkers: " + string.Join("; ", missed)));
            }

            // 6. pits: the way out of each (the PLAYER's check: the grid without NIGHT33's enemy lava mask, whose pit floors are the lava)
            NavGrid gp = g.hazard == null ? g : NavGrid.FromExport(O(map["navGrid"]));
            foreach (object po in L(Get(O(map["campaign"]), "pits")))
            {
                var pit = O(po);
                string zid = pit.TryGetValue("zone", out object zz) ? zz as string : "";
                int ri = level.rooms.FindIndex(r => r.id == zid);
                if (ri < 0 || spawnNode[ri] < 0) continue;
                List<object> pr = L(pit["rect"]);
                double px = (N(pr[0]) + N(pr[2])) / 2, pz = (N(pr[1]) + N(pr[3])) / 2, py = N(pit["floorY"]);
                // every pit-floor cell: from each, the hall's spawn must be reachable
                int cells = 0, stuck = 0;
                double sx0 = 1e9, sx1 = -1e9, sz0 = 1e9, sz1 = -1e9; // NIGHT26: where the stuck cells are (printed)
                var seen = new HashSet<int>();
                for (double xx = N(pr[0]) + 1; xx < N(pr[2]); xx += 2)
                    for (double zz2 = N(pr[1]) + 1; zz2 < N(pr[3]); zz2 += 2)
                    {
                        int u = gp.NodeAt(xx, py, zz2, 1.5, 1.5);
                        if (u < 0) continue;
                        cells++;
                        // the zone boxes start at y 0, so the pit (below it) is in no zone: it counts as the room's own space
                        bool[] out1 = Reach(gp, u, Link.MaskJumper, v => gp.tag[v] == tagOfRoom[ri] || (gp.tag[v] < 0 && gp.y[v] < 0));
                        if (!out1[spawnNode[ri]]) { stuck++; sx0 = Math.Min(sx0, xx); sx1 = Math.Max(sx1, xx); sz0 = Math.Min(sz0, zz2); sz1 = Math.Max(sz1, zz2); }
                    }
                rows.Add(($"pit recovery: {zid} ({F(N(pr[0]))}..{F(N(pr[2]))} x {F(N(pr[1]))}..{F(N(pr[3]))})", cells > 0 && stuck == 0,
                    cells == 0 ? "no pit-floor grid node found (the pit has no floor)" : stuck == 0 ? $"{cells} pit-floor cells, each walks and jumps back to the room's spawn: no soft-lock" : $"stuck cells x {F(sx0)}..{F(sx1)}, z {F(sz0)}..{F(sz1)}: {stuck} of {cells} pit-floor cells cannot get back to the room's spawn: a soft-lock"));
            }

            // 7. routes: the wall-jump line against the stepped route
            var wallLine = new Dictionary<string, bool>(); // NIGHT25: per room, a required wall line reaches its deck
            var tunnelOk = new Dictionary<string, bool>(); // NIGHT25: per room, its slide tunnel passes
            // NIGHT26 (builder M): the plan's "surfaces" list (ice / mud / boost rects): well formed, inside the level, a boost strip on ground that exists
            {
                var sp = new List<string>();
                Surfaces26 sf = Surfaces26.From(map, SurfaceRules.From(cfg), sp);
                foreach (SurfaceZone q in sf.zones)
                {
                    double cx = (q.x0 + q.x1) / 2, cz = (q.z0 + q.z1) / 2;
                    bool inside = false;
                    foreach (RoomDef r0 in level.rooms) if (cx >= r0.x0 && cx <= r0.x1 && cz >= r0.z0 && cz <= r0.z1) inside = true;
                    if (!inside && level.rooms.Count > 0) sp.Add($"{q.kind} rect centre ({F(cx)}, {F(cz)}) is in no room");
                }
                if (sf.Any || sp.Count > 0)
                    rows.Add(("surfaces", sp.Count == 0, sp.Count == 0 ? $"{sf.zones.Count} zones ({sf.zones.FindAll(q => q.kind == SurfaceKind.Ice).Count} ice, {sf.zones.FindAll(q => q.kind == SurfaceKind.Mud).Count} mud, {sf.zones.FindAll(q => q.kind == SurfaceKind.Boost).Count} boost), each well formed and inside a room" : string.Join("; ", sp)));
            }

            var routeSummary = new List<string>(); // NIGHT26 (builder M): floor route vs wall route for every platforming section, printed after the routes rows
            foreach (object ro in L(Get(O(map["campaign"]), "rooms")))
            {
                var rd = O(ro);
                if (!rd.ContainsKey("wallRoute")) continue;
                string rid = rd["id"] as string;
                int ri = level.rooms.FindIndex(r => r.id == rid);
                RoomDef r = level.rooms[ri];
                int a = Astar.NearestNode(graph, r.spawn), b = Astar.NearestNode(graph, r.exit);
                bool stepped = Astar.Path(graph, a, b, cfg.movement.dash.speed, path);
                double steppedSeconds = stepped ? path.cost : double.NaN;
                var mover = new Mover(cfg.movement, world);
                double west = double.NaN, z1 = double.NaN;
                foreach (Skygrind.Core.MapBox bx in sm.boxes)
                {
                    if (bx.kind != "slab" || bx.min.x < r.x0 || bx.max.x > r.x1 || bx.min.z < r.z0 || bx.max.z > r.z1 || bx.max.x - bx.min.x > 3) continue;
                    if (double.IsNaN(west) || bx.max.x < west) west = bx.max.x;
                    z1 = double.IsNaN(z1) ? bx.max.z : Math.Max(z1, bx.max.z);
                }
                var wr = O(rd["wallRoute"]);
                bool required = wr != null && wr.TryGetValue("required", out object rq) && rq is bool rb && rb; // NIGHT25: the flue is the only way up (the tutorial): no stepped route to beat
                double goalY = wr != null && wr.TryGetValue("goalY", out object gy) ? N(gy) : r.exit.y;
                (double best, double fast, int wj) = double.IsNaN(west) ? (0, -1, 0) : WallClimb(mover, cfg.Dt, west + 0.8, z1 - 0.5, goalY, 20);
                if (required)
                {
                    wallLine[rid] = fast >= 0 && wj >= 2;
                    rows.Add(($"routes: {rid} wall-jump line (the only way up)", wallLine[rid],
                        fast >= 0 ? $"the real Mover wall-jumps to the {F(goalY)} m deck in {F(fast)} s with {wj} wall jumps (top {F(best)} m); a plain jump reaches {F(cfg.Num("jump.height"))} m: the deck needs the wall line"
                            : $"the wall line does NOT reach the {F(goalY)} m deck (top {F(best)} m): the stage cannot be cleared"));
                    continue;
                }
                // the wall line starts at the corridor's mouth; the stepped route starts at the room's spawn: add the walk from the spawn to the mouth
                double mouth = double.IsNaN(west) ? 0 : Math.Sqrt((west + 0.8 - r.spawn.x) * (west + 0.8 - r.spawn.x) + (z1 - 0.5 - r.spawn.z) * (z1 - 0.5 - r.spawn.z)) / cfg.Num("ground.walkSpeed");
                double fastTotal = fast < 0 ? double.NaN : fast + mouth;
                if (fast >= 0 && wr != null && wr.TryGetValue("after", out object afterRaw))
                {
                    // NIGHT25: the wall line ends on a deck that is not the exit: the rest of the way (deck point to exit) is RIVAL's graph too
                    int a1 = Astar.NearestNode(graph, V(afterRaw)), b1 = Astar.NearestNode(graph, r.exit);
                    fastTotal = a1 >= 0 && b1 >= 0 && Astar.Path(graph, a1, b1, cfg.movement.dash.speed, path) ? fastTotal + path.cost : double.NaN;
                }
                bool ok = stepped && fast >= 0 && fastTotal < steppedSeconds;
                rows.Add(($"routes: {rid} wall-jump line against the stepped route", ok,
                    $"the wall-jump line {(fast >= 0 ? F(fastTotal) + " s (" + F(mouth) + " s to the corridor's mouth + " + F(fast) + " s of climbing, " + wj + " wall jumps, top " + F(best) + " m)" : "does NOT reach the deck (top " + F(best) + " m)")}; " +
                    $"the stepped route {(stepped ? F(steppedSeconds) + " s on RIVAL's graph (steps and jumps, no wall jump)" : "has NO route")}: {(ok ? "the wall line is the faster by " + F(steppedSeconds - fastTotal) + " s" : "the wall line is NOT the fast one")}"));
                routeSummary.Add($"{rid}: floor route {(stepped ? F(steppedSeconds) + " s" : "none")}, wall route {(fast >= 0 ? F(fastTotal) + " s" : "does not reach")}");
            }
            if (routeSummary.Count > 0) rows.Add(("platforming routes: floor vs wall", true, string.Join("; ", routeSummary)));
            // NIGHT26 (builder M): the platforming kit's rows. A room with "hallRoute": the scripted wall-jump chain on the real Mover against RIVAL's floor route
            // (the wall route must be at least minFloorOverWall x faster); a room with "dashGap": a dash alone crosses, a running jump alone does not.
            foreach (object ro in L(Get(O(map["campaign"]), "rooms")))
            {
                var rd = O(ro);
                string rid = rd["id"] as string;
                int ri = level.rooms.FindIndex(r => r.id == rid);
                if (ri < 0) continue;
                RoomDef r = level.rooms[ri];
                if (rd.TryGetValue("hallRoute", out object hro))
                {
                    var hr = O(hro);
                    Vec3 hf = V(hr["from"]), ht = V(hr["to"]);
                    var gl = L(hr["gaps"]);
                    var gaps = new double[gl.Count][];
                    for (int gi = 0; gi < gaps.Length; gi++) gaps[gi] = new[] { N(L(gl[gi])[0]), N(L(gl[gi])[1]) };
                    PlatformRoutes26.RunResult run = PlatformRoutes26.HallRun(new Mover(cfg.movement, world), cfg.Dt, hf.x, hf.z, ht.z, gaps, 20, hr.TryGetValue("jumpAtZ", out object jz) ? N(jz) : double.PositiveInfinity);
                    Vec3 lipAt = hr.TryGetValue("lip", out object lipo) ? V(lipo) : r.spawn; // the floor route starts at the launch lip, like the wall route's jump (the run-up before it is common)
                    int a2 = Astar.NearestNode(graph, lipAt), b2 = Astar.NearestNode(graph, r.exit);
                    bool floor = a2 >= 0 && b2 >= 0 && Astar.Path(graph, a2, b2, cfg.movement.dash.speed, path);
                    double floorS = floor ? path.cost : double.NaN;
                    // the wall run starts at the ledge's lip (hallRoute.from): add the walk from the spawn to it, as the wall-jump line row does
                    double lead = -run.jumpSeconds; // the wall route's seconds are counted from the lip's jump (run.seconds - jumpSeconds)
                    double wallS = run.reached ? run.seconds + lead : double.NaN;
                    double need = hr.TryGetValue("minFloorOverWall", out object mf) ? N(mf) : 2;
                    bool okh = run.reached && floor && floorS >= need * wallS;
                    rows.Add(($"hall: {rid} wall route against the floor route", okh,
                        $"wall route {(run.reached ? F(wallS) + " s (" + run.wallJumps + " wall jumps, " + run.dashes + " dashes, " + F(run.wallRideSeconds) + " s riding)" : "does NOT reach the far floor (best z " + F(run.bestZ) + ")")}; " +
                        $"floor route {(floor ? F(floorS) + " s on RIVAL's graph (down the pit, the serpentine, the far stair)" : "has NO route")}: {(okh ? "the wall route is " + F(floorS / wallS) + "x faster" : "the wall route is NOT " + F(need) + "x faster")}"));
                    routeSummary.Add($"{rid}: floor route {(floor ? F(floorS) + " s" : "none")}, wall route {(run.reached ? F(wallS) + " s" : "does not reach")}");
                }
                if (rd.TryGetValue("dashGap", out object dgo))
                {
                    var dg = O(dgo);
                    Vec3 df = V(dg["from"]), dt2 = V(dg["to"]);
                    double gap = N(dg["gap"]);
                    double lip = df.z - 1; // `from` is 1 m before the lip
                    bool dashOk = PlatformRoutes26.DashCross(new Mover(cfg.movement, world), cfg.Dt, df.x, lip + 6, lip, gap, true);
                    bool jumpOk = PlatformRoutes26.DashCross(new Mover(cfg.movement, world), cfg.Dt, df.x, lip + 6, lip, gap, false);
                    rows.Add(($"dash gap: {rid} ({F(gap)} m)", dashOk && !jumpOk,
                        $"the real Mover running up and pressing dash at the lip {(dashOk ? "lands on the far deck" : "does NOT cross")}; pressing jump at the lip {(jumpOk ? "ALSO crosses (the gap is too short to need the dash)" : "falls short")}"));
                }
            }


            // 8. slide tunnels: the real Mover slides through, cannot walk through; the sizes say who else fits
            foreach (object to in L(Get(O(map["campaign"]), "tunnels")))
            {
                var t = O(to);
                List<object> tr = L(t["rect"]);
                double tx = (N(tr[0]) + N(tr[2])) / 2, tz0 = N(tr[1]), tz1 = N(tr[3]), ceil = N(t["ceiling"]);
                double slideH = cfg.Num("slide.colliderHeight"), huskH = cfg.Num("husk.height"), swarmH = cfg.Num("swarmer.height"), standH = cfg.Num("body.capsuleHeight");
                (bool slid, double secs) = TunnelRun(new Mover(cfg.movement, world), cfg.Dt, tx, tz1 + 4, tz0 - 1, true);
                (bool walked, _) = TunnelRun(new Mover(cfg.movement, world), cfg.Dt, tx, tz1 + 4, tz0 - 1, false);
                bool sizes = slideH <= ceil && ceil < standH && ceil < huskH && swarmH <= ceil;
                tunnelOk[(string)t["zone"]] = slid && !walked && sizes;
                rows.Add(($"slide tunnel: {t["zone"]} ({F(tx)}, z {F(tz0)}..{F(tz1)})", slid && !walked && sizes,
                    $"ceiling {F(ceil)} m: a sliding player (collider {F(slideH)} m) {(slid ? "passes in " + F(secs) + " s" : "does NOT pass")}, a standing one (body {F(standH)} m) {(walked ? "WALKS THROUGH (wrong)" : "is stopped")}; a swarmer ({F(swarmH)} m) {(swarmH <= ceil ? "fits" : "does NOT fit")}, a husk ({F(huskH)} m) {(huskH > ceil ? "goes round" : "fits (wrong)")}"));
            }

            // 8b. NIGHT25 shortcuts (a race track's second route): a room's "shortcuts": [{name, tunnelZone, a, b, minGain}]: the slide tunnel as a cut. Two routes with
            // measured times: the LONG one (RIVAL's graph spawn to exit: it cannot pass a tunnel, it goes round) and the CUT (graph spawn to `a`, the real Mover through the tunnel,
            // graph `b` to the exit). Passes when the cut saves at least minGain seconds.
            foreach (RoomDef r in level.rooms)
            {
                foreach (object so in L(Get(RoomJson(map, r.id), "shortcuts")))
                {
                    var sc = O(so);
                    string tz = sc.TryGetValue("tunnelZone", out object tzr) ? tzr as string : r.id;
                    Dictionary<string, object> tun = null;
                    foreach (object to in L(Get(O(map["campaign"]), "tunnels"))) if ((O(to)["zone"] as string) == tz) { tun = O(to); break; }
                    int sa = Astar.NearestNode(graph, r.spawn), sx = Astar.NearestNode(graph, r.exit);
                    bool longOk = sa >= 0 && sx >= 0 && Astar.Path(graph, sa, sx, cfg.movement.dash.speed, path);
                    double longT = longOk ? path.cost : double.NaN;
                    double cutT = double.NaN;
                    double cross = double.NaN;
                    bool cutOk = false;
                    if (tun != null)
                    {
                        List<object> tr = L(tun["rect"]);
                        double tx = (N(tr[0]) + N(tr[2])) / 2, tz0 = N(tr[1]), tz1 = N(tr[3]);
                        (bool slid, double secs) = TunnelRun(new Mover(cfg.movement, world), cfg.Dt, tx, tz1 + 4, tz0 - 1, true);
                        int an = Astar.NearestNode(graph, V(sc["a"])), bn = Astar.NearestNode(graph, V(sc["b"]));
                        double t1 = 0, t2 = 0;
                        bool p1 = an >= 0 && Astar.Path(graph, sa, an, cfg.movement.dash.speed, path);
                        if (p1) t1 = path.cost;
                        bool p2 = bn >= 0 && Astar.Path(graph, bn, sx, cfg.movement.dash.speed, path);
                        if (p2) t2 = path.cost;
                        cross = secs;
                        cutOk = slid && p1 && p2;
                        cutT = t1 + secs + t2;
                    }
                    double gain = sc.TryGetValue("minGain", out object mg) ? N(mg) : 0;
                    bool ok = longOk && cutOk && cutT + gain <= longT;
                    string nm = sc.TryGetValue("name", out object nmr) ? nmr as string : "cut";
                    rows.Add(($"routes: {r.id} {nm} against the long way", ok,
                        $"the long way {(longOk ? F(longT) + " s on RIVAL's graph (round the baffles)" : "has NO route")}; the cut {(cutOk ? F(cutT) + " s (graph to the mouth + " + F(cross) + " s through the tunnel on the real Mover + graph to the exit)" : "does NOT work")}: {(ok ? "the cut saves " + F(longT - cutT) + " s (at least " + F(gain) + ")" : "the cut is NOT faster by " + F(gain) + " s")}"));
                }
            }

            // 9. NIGHT25 lessons: every lesson is achievable (by the player's graph, the real Mover's rows above, or the room's own enemies), every
            // prompt is at most six words, and a stage's respawn line sits between a pit's floor and the hall floor
            foreach (RoomDef r in level.rooms)
            {
                if (r.lessons.Count == 0 && !r.hasRespawnY) continue;
                int ri = level.rooms.IndexOf(r);
                var rjson = RoomJson(map, r.id);
                var starts = new List<Vec3> { r.spawn };
                foreach (object vo in L(Get(rjson, "routeVia"))) starts.Add(V(L(vo)[1]));
                for (int li = 0; li < r.lessons.Count; li++)
                {
                    LessonDef d = r.lessons[li];
                    string name = $"lesson {r.id}.{li + 1} {LessonDef.KindName(d.kind)}";
                    string why;
                    bool ok;
                    switch (d.kind)
                    {
                        case LessonKind.Reached:
                        case LessonKind.JumpedPit:
                        case LessonKind.DashedGap:
                        case LessonKind.PadHop:
                        case LessonKind.FlickCrossed:
                            {
                                int target = -1;
                                for (int u = 0; u < g.count && target < 0; u++)
                                {
                                    double ux = g.origin + (g.col[u] % g.size + 0.5) * g.cell, uz = g.origin + (g.col[u] / g.size + 0.5) * g.cell; // the column's centre (NavGridColumnCenter)
                                    if (d.hasRect && ux >= d.rx0 && ux <= d.rx1 && uz >= d.rz0 && uz <= d.rz1 && g.y[u] >= d.minY && g.tag[u] == tagOfRoom[ri]) target = u;
                                }
                                bool reachable = false;
                                string via = "";
                                if (target >= 0)
                                {
                                    var tp = new Vec3(g.origin + (g.col[target] % g.size + 0.5) * g.cell, g.y[target], g.origin + (g.col[target] / g.size + 0.5) * g.cell);
                                    int tn = Astar.NearestNode(graph, tp);
                                    foreach (Vec3 s0 in starts)
                                    {
                                        int sn = Astar.NearestNode(graph, s0);
                                        if (sn >= 0 && tn >= 0 && Astar.Path(graph, sn, tn, cfg.movement.dash.speed, path))
                                        {
                                            reachable = true;
                                            via = string.Join(" ", Kinds(path));
                                            break;
                                        }
                                    }
                                }
                                ok = target >= 0 && reachable;
                                why = target < 0 ? "no grid node stands in the lesson's rect at or above minY" : reachable ? $"the player's graph reaches the rect ({(via == "" ? "walk" : via)})" : "the rect is NOT reachable from the stage's start";
                                if (d.kind == LessonKind.DashedGap && ok && !via.Contains("dash")) { ok = false; why += ": but not by a dash (the gap is jumpable, the lesson is not a dash)"; }
                                if (d.kind == LessonKind.PadHop && ok && !map.ContainsKey("pads")) { ok = false; why += ": no pad"; }
                                break;
                            }
                        case LessonKind.SlidTunnel:
                            ok = tunnelOk.TryGetValue(r.id, out bool to) && to && d.hasVia;
                            why = ok ? "the room's slide tunnel passes by the real Mover (the tunnel row)" : "no passing slide tunnel in this room";
                            break;
                        case LessonKind.WallJumped:
                            ok = wallLine.TryGetValue(r.id, out bool wl) && wl;
                            why = ok ? $"the room's wall line reaches its deck with >= {d.n} wall jumps (the routes row)" : "no passing wall line in this room";
                            break;
                        case LessonKind.WallRiddenAndFired: // NIGHT29: shots fired while wall riding (Lesson25): the room must carry a wall-lane proof
                            ok = Get(rjson, "proofs") is List<object> pl && pl.Exists(po => (O(po)?["kind"] as string) == "ride");
                            why = ok ? $"{d.n} shot(s) fired while wall riding; the room's `ride` proof rides the lane on the real Mover (completable row)" : "no `ride` proof in this room: nothing shows the wall can be ridden";
                            break;
                        case LessonKind.Shots:
                            ok = true;
                            why = "a shot needs nothing but the weapon";
                            break;
                        case LessonKind.TargetsHit:
                        case LessonKind.HusksDead:
                            ok = r.EnemyCount >= d.n || r.waves.Count > 0; // the top-up respawns the last wave when the enemies die early
                            why = ok ? $"{r.EnemyCount} enemies in {r.WaveCount} wave(s), the last wave is topped up if the lesson is not done" : "the room has no enemies";
                            break;
                        case LessonKind.PunchWhileFiring:
                            ok = r.waves.Count > 0;
                            why = ok ? "an enemy to punch while shooting (needs design.n25.meleeFire; the lesson is passed over when it is off)" : "the room has no enemies";
                            break;
                        default:
                            ok = false;
                            why = "unknown lesson kind";
                            break;
                    }
                    int words = d.prompt.Trim().Length == 0 ? 0 : d.prompt.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                    int wordsPad = d.promptPad.Trim().Length == 0 ? 0 : d.promptPad.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                    if (words > 6 || wordsPad > 6) { ok = false; why += $"; the prompt is {Math.Max(words, wordsPad)} words (at most 6)"; }
                    rows.Add((name, ok, why));
                }
                if (r.hasRespawnY)
                {
                    double floorY = 0;
                    foreach (object po in L(Get(O(map["campaign"]), "pits")))
                        if ((O(po)["zone"] as string) == r.id) floorY = Math.Min(floorY, N(O(po)["floorY"]));
                    bool ok = r.respawnY < 0 && r.respawnY > floorY;
                    rows.Add(($"stage respawn: {r.id}", ok, ok ? $"feet below y {F(r.respawnY)} (above the pit floor {F(floorY)}) return to the spawn with no death; the recovery stair is the fallback" : $"respawnY {F(r.respawnY)} must lie between the pit floor {F(floorY)} and 0"));
                }
            }

            // 10. NIGHT27 (builder A): the arena rules (size, wave budget, kills, limits, wall lanes against the floor route), for plans that ask
            LevelValidate27.Rows(cfg, map, level, graph, rows, N27Lines);
            // 11. NIGHT29 (builder R): the room standard (lights, props, doorFit, closedRoom), for plans with "n29": true
            LevelValidate29.Rows(cfg, map, level, world, rows);

            int errors = 0;
            sb.Append("| check | result | numbers |\n|---|---|---|\n");
            foreach (var row in rows)
            {
                if (!row.pass) errors++;
                sb.Append($"| {row.check} | {(row.pass ? "PASS" : "ERROR")} | {row.detail} |\n");
            }
            sb.Insert(sb.ToString().IndexOf("| check |", StringComparison.Ordinal), $"**errors: {errors}** ({rows.Count} checks)\n\n");
            return (sb.ToString(), errors);
        }

        /// <summary>The map JSON's campaign room row with this id (null when absent).</summary>
        static Dictionary<string, object> RoomJson(Dictionary<string, object> map, string id)
        {
            foreach (object ro in L(Get(O(map["campaign"]), "rooms")))
            {
                var rd = O(ro);
                if (rd != null && rd.TryGetValue("id", out object v) && (v as string) == id) return rd;
            }
            return null;
        }

        static Vec3 V(object o)
        {
            List<object> a = L(o);
            return new Vec3(N(a[0]), N(a[1]), N(a[2]));
        }

        static IEnumerable<string> Kinds(NavPath p)
        {
            var seen = new List<string>();
            foreach (string k in p.kinds) if (!seen.Contains(k)) seen.Add(k);
            return seen;
        }
    }
}
