// NIGHT20 U3-34.2 (the lead owns this file; builders never edit it): THE CLIMB. Can the horde reach every place a player
// can stand, and get back down? On a map's nav with every gate open (Zones.OpenAll):
// - THE PLAYER'S SET: the nodes of the bot's nav graph (BotNav: the web's navGraph.js port, every edge of which the build
//   flew with the real Mover: walk, drop, jump, hop, double jump, climb, dash, dash-double, the pads) reachable from the
//   map's spawn, each mapped onto the husks' grid node under it (NavGrid.NodeAt; graph nodes with no grid node under them
//   are left out and counted). Wall-jumps are not in the web's graph: a perch only a wall-jump reaches is not measured.
// - THE HORDE'S TIME: for each player node P, the husks' field Dijkstra toward P seeded only where a husk can hit P (P and
//   its 8 neighbour columns' nodes within 1 m of its height: never the field's seed ring under a perch), over the grid's
//   links (every kind the husks' jump field uses, including NIGHT20's route links when they are in the grid),
//   read at every ground spawn node the director can use (Zones.SpawnPoints' ground list); the minimum, in seconds at the
//   husk's speed (ai.kinds.husk.speed; the field's costs are metres). Unreachable: infinity.
// - R2's floor rule: the same for the node directly under P in its column (the next lower grid node); P's time may be at
//   most 6 s more than its floor's.
// - R2B, GETTING DOWN: a field seeded at every ground node (y within 0.3 m of the grid's lowest), read at every node a
//   ground spawn reaches (a forward search over the same links): the seconds to get back down.
// - THE PERCHES: the 10 player nodes with the worst horde time (infinity first, then by height), at least 8 m apart.
// - THE PERCH TEST (R3): THE CHASE's STAND bot (the autopilot, never moving) respawned on each perch at round 10, a 300 s
//   limit, the median of 5 seeds, against the same bot at the map's spawn (THE CHASE's STAND): its survival and its damage
//   per minute (a perch may buy time, never safety: dpm >= 50 % of the ground's, survival <= 2 x the ground's).
// Deterministic (no clock, no random): two runs print the same numbers (SandboxClimbTests).
using System;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class SandboxClimb
    {
        public const double Ground = 0.3; // m over the grid's lowest node: the ground floor
        public const double PerchSpacing = 8; // m: the 10 perches are this far apart at least (one per place, not 10 nodes of one spire)
        public const int Perches = 10;
        public const int PerchSeeds = 5;
        public const int PerchRound = 10;

        public sealed class PlayerNode
        {
            public int graph, grid;
            public double x, y, z;
            public string kind, group;
            public double horde = double.PositiveInfinity; // s from the nearest ground spawn (infinity: unreachable)
            public double floorHorde = double.NaN; // the same for the grid node under it (NaN: none)
            public double down = double.PositiveInfinity; // s back down to the ground for a husk standing here
            public string Name => $"{group ?? kind ?? "?"} y {y:0.0} ({x:0}, {z:0})";
        }

        public sealed class PerchResult
        {
            public PlayerNode node; // null: the ground (the map's spawn)
            public double survival, dpm; // medians
            public int died;
        }

        public sealed class Report
        {
            public string map;
            public int graphNodes, reached, unmapped, spawnNodes;
            public readonly List<PlayerNode> nodes = new List<PlayerNode>();
            public int unreachable;
            public double worstHorde;
            public int floorRuleFails;
            public int huskNodes; // grid nodes a ground spawn reaches
            public int downFails; // of them, more than 6 s to get down (or never)
            public double worstDown;
            public readonly List<PlayerNode> perches = new List<PlayerNode>();
            public readonly List<PerchResult> perchTests = new List<PerchResult>();
            public PerchResult ground;
        }

        /// <summary>R1, R2, R2b and the perches (no bot runs): the reach model and the fields.</summary>
        public static Report Measure(GameConfig cfg, SurvivalMap map)
        {
            if (BotNav.CacheDir == null) BotNav.CacheDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "skygrind-botnav");
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.metaSilent = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(1, null, null);
            sim.zones.OpenAll();
            var rep = new Report { map = map.name };
            NavGrid grid = sim.nav.grid;
            double speed = cfg.Num("ai.kinds.husk.speed");
            double lookBelow = cfg.Num("ai.grid.lookBelow"), lookAbove = cfg.Num("ai.grid.lookAbove");

            // The player's set: BFS over the bot's graph from the node nearest the spawn.
            NavGraph g = BotNav.Survival(sim);
            if (g == null) throw new InvalidOperationException("no bot nav graph on " + map.name + ": " + BotNav.LastSource);
            rep.graphNodes = g.nodes.Count;
            Vec3 spawn = sim.mover.position;
            int start = -1;
            double best = double.PositiveInfinity;
            for (int i = 0; i < g.nodes.Count; i++)
            {
                NavNode n = g.nodes[i];
                double d = (n.x - spawn.x) * (n.x - spawn.x) + (n.z - spawn.z) * (n.z - spawn.z) + 4 * (n.y - spawn.y) * (n.y - spawn.y);
                if (d < best)
                {
                    best = d;
                    start = i;
                }
            }
            var seen = new bool[g.nodes.Count];
            var stack = new Stack<int>();
            seen[start] = true;
            stack.Push(start);
            while (stack.Count > 0)
            {
                int a = stack.Pop();
                foreach (NavEdge e in g.edges[a])
                {
                    if (seen[e.to]) continue;
                    seen[e.to] = true;
                    stack.Push(e.to);
                }
            }
            var byGrid = new Dictionary<int, PlayerNode>();
            for (int i = 0; i < g.nodes.Count; i++)
            {
                if (!seen[i]) continue;
                rep.reached++;
                NavNode n = g.nodes[i];
                int gn = grid.NodeAt(n.x, n.y, n.z, lookBelow, lookAbove);
                if (gn < 0)
                {
                    rep.unmapped++;
                    continue;
                }
                if (byGrid.ContainsKey(gn)) continue;
                var p = new PlayerNode { graph = i, grid = gn, x = grid.NodeX(gn), y = grid.y[gn], z = grid.NodeZ(gn), kind = n.kind, group = n.group };
                byGrid[gn] = p;
                rep.nodes.Add(p);
            }

            // The ground spawns the director can use (every zone open).
            var groundSpawns = new List<SpawnNode>();
            var highSpawns = new List<SpawnNode>();
            sim.zones.SpawnPoints(groundSpawns, highSpawns, sim.core);
            var spawnGrid = new List<int>();
            foreach (SpawnNode s in groundSpawns)
            {
                int gn = grid.NodeAt(s.x, s.y, s.z, lookBelow, lookAbove);
                if (gn >= 0 && !spawnGrid.Contains(gn)) spawnGrid.Add(gn);
            }
            rep.spawnNodes = spawnGrid.Count;

            // The horde's time to each player node (and to the floor under it): the husks' own field toward a player there.
            var floorCache = new Dictionary<int, double>();
            foreach (PlayerNode p in rep.nodes)
            {
                p.horde = HordeTo(sim, grid, p.grid, spawnGrid, speed);
                int below = FloorUnder(grid, p.grid);
                if (below >= 0)
                {
                    if (!floorCache.TryGetValue(below, out double fh))
                    {
                        fh = HordeTo(sim, grid, below, spawnGrid, speed);
                        floorCache[below] = fh;
                    }
                    p.floorHorde = fh;
                }
            }

            // Getting down: a field seeded at the ground, read at every node a ground spawn reaches.
            float minY = float.PositiveInfinity;
            for (int n = 0; n < grid.count; n++) if (grid.blocked[n] == 0) minY = Math.Min(minY, grid.y[n]);
            var seeds = new List<double>();
            for (int n = 0; n < grid.count; n++)
            {
                if (grid.blocked[n] != 0 || grid.y[n] > minY + Ground) continue;
                seeds.Add(n);
                seeds.Add(0);
            }
            var down = new Skygrind.Core.Flow(); // the field's own Dijkstra (Core's Flow, not the Game's run flow)
            down.Start(grid, seeds, true);
            down.Continue(grid);
            bool[] huskReach = ForwardReach(grid, spawnGrid);
            rep.worstDown = 0;
            for (int n = 0; n < grid.count; n++)
            {
                if (!huskReach[n]) continue;
                rep.huskNodes++;
                double s = down.dist[n] / speed;
                if (!(s <= 6)) rep.downFails++;
                rep.worstDown = Math.Max(rep.worstDown, s);
            }
            foreach (PlayerNode p in rep.nodes) p.down = down.dist[p.grid] / speed;

            // R1, R2 and the floor rule.
            rep.worstHorde = 0;
            foreach (PlayerNode p in rep.nodes)
            {
                if (double.IsPositiveInfinity(p.horde)) rep.unreachable++;
                rep.worstHorde = Math.Max(rep.worstHorde, p.horde);
                if (!double.IsNaN(p.floorHorde) && !(p.horde <= p.floorHorde + 6)) rep.floorRuleFails++;
            }

            // The perches: the worst horde times (infinity first, then the highest), at least PerchSpacing apart.
            var order = new List<PlayerNode>(rep.nodes);
            order.Sort((a, b) =>
            {
                int c = b.horde.CompareTo(a.horde);
                if (c != 0) return c;
                c = b.y.CompareTo(a.y);
                return c != 0 ? c : a.grid.CompareTo(b.grid);
            });
            foreach (PlayerNode p in order)
            {
                if (rep.perches.Count >= Perches) break;
                if (p.y <= minY + Ground) continue; // the ground is the reference, not a perch
                bool near = false;
                foreach (PlayerNode q in rep.perches)
                {
                    double dx = p.x - q.x, dz = p.z - q.z;
                    if (dx * dx + dz * dz < PerchSpacing * PerchSpacing) near = true;
                }
                if (!near) rep.perches.Add(p);
            }
            return rep;
        }

        static readonly Skygrind.Core.Flow horde = new Skygrind.Core.Flow();
        static readonly List<double> hordeSeeds = new List<double>();

        /// <summary>The husks' travel time to a player standing on grid node `goal`: the field's Dijkstra over the grid's links
        /// (every kind the husks' jump field uses, NIGHT20's route links included when they are in the grid), seeded ONLY
        /// where a husk can hit that player: the goal node and its 8 neighbour columns' nodes within 1 m of its height
        /// (never the field's seed ring under a perch: a husk standing below a player it can't reach has not arrived,
        /// NIGHT20.md banned pattern 17). Read at the spawns: the minimum, in s at the husk's speed.</summary>
        static double HordeTo(SurvivalSim sim, NavGrid grid, int goal, List<int> spawns, double speed)
        {
            hordeSeeds.Clear();
            int c0 = grid.col[goal], ci = c0 % grid.size, ck = c0 / grid.size;
            for (int k = Math.Max(0, ck - 1); k <= Math.Min(grid.size - 1, ck + 1); k++)
            {
                for (int i = Math.Max(0, ci - 1); i <= Math.Min(grid.size - 1, ci + 1); i++)
                {
                    int c = k * grid.size + i;
                    for (int n = grid.colStart[c]; n < grid.colStart[c + 1]; n++)
                    {
                        if (grid.blocked[n] != 0 || Math.Abs(grid.y[n] - grid.y[goal]) > 1.0) continue;
                        hordeSeeds.Add(n);
                        hordeSeeds.Add(n == goal ? 0 : JsMath.Hypot(grid.ColX(c) - grid.NodeX(goal), grid.ColZ(c) - grid.NodeZ(goal)));
                    }
                }
            }
            horde.Start(grid, hordeSeeds, true);
            horde.Continue(grid);
            double bestD = double.PositiveInfinity;
            foreach (int s in spawns) bestD = Math.Min(bestD, horde.dist[s]);
            return bestD / speed;
        }

        /// <summary>The next lower grid node in `n`'s column (the floor under it), or -1.</summary>
        static int FloorUnder(NavGrid grid, int n)
        {
            int c = grid.col[n];
            int best = -1;
            for (int m = grid.colStart[c]; m < grid.colStart[c + 1]; m++)
            {
                if (m == n || grid.blocked[m] != 0 || grid.y[m] >= grid.y[n] - 1) continue;
                if (best < 0 || grid.y[m] > grid.y[best]) best = m;
            }
            return best;
        }

        /// <summary>Every node a husk starting on a spawn can reach (forward over the links the jump field uses; blocked
        /// nodes skipped).</summary>
        static bool[] ForwardReach(NavGrid grid, List<int> starts)
        {
            var seen = new bool[grid.count];
            var st = new Stack<int>();
            foreach (int s in starts)
            {
                if (seen[s]) continue;
                seen[s] = true;
                st.Push(s);
            }
            while (st.Count > 0)
            {
                int a = st.Pop();
                for (int li = grid.linkStart[a]; li < grid.linkStart[a + 1]; li++)
                {
                    int b = grid.linkTo[li];
                    if (seen[b] || grid.blocked[b] != 0) continue;
                    seen[b] = true;
                    st.Push(b);
                }
            }
            return seen;
        }

        /// <summary>R3: THE CHASE's STAND on each perch (and at the map's spawn: the ground), round PerchRound, PerchSeeds seeds.</summary>
        public static void PerchTests(GameConfig cfg, SurvivalMap map, Report rep, int seeds = PerchSeeds, Action<string> progress = null)
        {
            rep.ground = Stand(cfg, map, null, seeds);
            progress?.Invoke($"ground: survival {rep.ground.survival:0.0} s, dpm {rep.ground.dpm:0.0}");
            foreach (PlayerNode p in rep.perches)
            {
                PerchResult r = Stand(cfg, map, p, seeds);
                rep.perchTests.Add(r);
                progress?.Invoke($"{p.Name}: survival {r.survival:0.0} s, dpm {r.dpm:0.0}");
            }
        }

        static PerchResult Stand(GameConfig cfg, SurvivalMap map, PlayerNode at, int seeds)
        {
            var surv = new List<double>();
            var dpm = new List<double>();
            int died = 0;
            for (int s = 1; s <= seeds; s++)
            {
                Vec3? pos = at == null ? (Vec3?)null : new Vec3(at.x, at.y + 0.05, at.z);
                SandboxChase.Result r = SandboxChase.Run(cfg, map, "stand", PerchRound, (uint)s, pos);
                surv.Add(r.survival);
                dpm.Add(r.dpm);
                if (r.died) died++;
            }
            return new PerchResult { node = at, survival = Sandbox.Median(surv), dpm = Sandbox.Median(dpm), died = died };
        }

        /// <summary>The hand check: the bot graph's path from the spawn to `perch`, each edge flown with the real Mover
        /// (NavGraph.Fly: s, 0 = it can't); and one husk from the nearest ground spawn walked by the real brain toward a
        /// player standing on the perch (s until it is within 2.5 m, or the limit).</summary>
        public static (List<string> edges, bool flown, double huskSeconds) HandCheck(GameConfig cfg, SurvivalMap map, PlayerNode perch, double limit = 90)
        {
            if (BotNav.CacheDir == null) BotNav.CacheDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "skygrind-botnav");
            SurvivalSim sim = SurvivalSim.Headless(cfg, map);
            sim.noRecords = sim.noLab = true;
            sim.metaSilent = true;
            sim.setup.mode = RunSetup.Survival;
            sim.StartRun(1, null, null);
            sim.zones.OpenAll();
            NavGraph g = BotNav.Build(cfg, map); // NIGHT20 round 2 (builder N found it): a fresh build (a graph from the disk cache has no builder to fly with)
            Vec3 spawn = sim.mover.position;
            int start = 0;
            double best = double.PositiveInfinity;
            for (int i = 0; i < g.nodes.Count; i++)
            {
                NavNode n = g.nodes[i];
                double d = (n.x - spawn.x) * (n.x - spawn.x) + (n.z - spawn.z) * (n.z - spawn.z) + 4 * (n.y - spawn.y) * (n.y - spawn.y);
                if (d < best)
                {
                    best = d;
                    start = i;
                }
            }
            // BFS parents (fewest edges) from the spawn's node to the perch's.
            var parent = new int[g.nodes.Count];
            var via = new NavEdge[g.nodes.Count];
            for (int i = 0; i < parent.Length; i++) parent[i] = -2;
            var q = new Queue<int>();
            parent[start] = -1;
            q.Enqueue(start);
            while (q.Count > 0)
            {
                int a = q.Dequeue();
                if (a == perch.graph) break;
                foreach (NavEdge e in g.edges[a])
                {
                    if (parent[e.to] != -2) continue;
                    parent[e.to] = a;
                    via[e.to] = e;
                    q.Enqueue(e.to);
                }
            }
            var edges = new List<string>();
            bool flown = parent[perch.graph] != -2;
            for (int v = perch.graph; flown && parent[v] >= 0; v = parent[v])
            {
                int u = parent[v];
                double sec = via[v].kind == "pad" ? via[v].cost : g.Fly(g.nodes[u], g.nodes[v], via[v].kind, via[v].back); // a launch pad is the map's own (the graph timed it): not flown
                edges.Insert(0, $"{via[v].kind} ({g.nodes[u].x:0.#}, {g.nodes[u].y:0.#}, {g.nodes[u].z:0.#}) -> ({g.nodes[v].x:0.#}, {g.nodes[v].y:0.#}, {g.nodes[v].z:0.#}): {(sec > 0 ? $"{sec:0.00} s" : "CAN'T")}");
                if (!(sec > 0)) flown = false;
            }
            // One husk walked by the real brain toward a player on the perch (god mode; no director spawns).
            sim.holdRunClock = true;
            sim.director.Clear();
            sim.run.state = SurvivalRun.State.Fight;
            sim.health.max = sim.health.hp = 1e6;
            var target = new Vec3(perch.x, perch.y + 0.05, perch.z);
            sim.Respawn(target, 0);
            var groundSpawns = new List<SpawnNode>();
            var high = new List<SpawnNode>();
            sim.zones.SpawnPoints(groundSpawns, high, sim.core);
            SpawnNode? from = null;
            double bestS = double.PositiveInfinity;
            NavGrid grid = sim.nav.grid;
            double speed = cfg.Num("ai.kinds.husk.speed");
            sim.nav.UpdateNow(perch.x, perch.y, perch.z);
            foreach (SpawnNode s in groundSpawns)
            {
                int gn = grid.NodeAt(s.x, s.y, s.z, cfg.Num("ai.grid.lookBelow"), cfg.Num("ai.grid.lookAbove"));
                if (gn < 0) continue;
                double t = sim.nav.jumpField.dist[gn] / speed;
                if (t < bestS)
                {
                    bestS = t;
                    from = s;
                }
            }
            double huskSeconds = double.PositiveInfinity;
            if (from.HasValue)
            {
                Husk h = sim.husks.Spawn("husk", new Vec3(from.Value.x, from.Value.y, from.Value.z), target);
                if (h != null)
                {
                    sim.brain.Attach(h);
                    var idle = new ScriptedInput();
                    sim.UseInput(idle);
                    int max = MovementMath.StepsFor(limit, sim.step);
                    for (int i = 0; i < max; i++)
                    {
                        sim.Step(); // the player stands still on the perch (no input)
                        double dx = h.position.x - target.x, dy = h.position.y - target.y, dz = h.position.z - target.z;
                        if (dx * dx + dy * dy + dz * dz <= 2.5 * 2.5)
                        {
                            huskSeconds = (i + 1) * sim.step;
                            break;
                        }
                        if (!h.alive) break;
                    }
                }
            }
            sim.holdRunClock = false;
            edges.Add($"the judge's horde time to this node: {bestS:0.00} s (the nearest ground spawn by the field)");
            return (edges, flown, huskSeconds);
        }
    }
}
